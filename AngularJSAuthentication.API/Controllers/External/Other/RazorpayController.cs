using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External.Razorpay;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.RazorPay;
using GenricEcommers.Models;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.External.Other
{
    [RoutePrefix("api/Razorpay")]
    public class RazorpayController : ApiController
    {
        [Route("QRWebhook")]
        [HttpPost]
        public bool CaptureWebHookQRPayment(RazorPayWebHookResponse webHookResponse)
        {
            var helper = new ReadyToDispatchHelper();
            var result = helper.CaptureWebHookQRPayment(webHookResponse);

            if (result.IsCaptured && !string.IsNullOrEmpty(result.DeliveryBoyFcmId))
            {
                var objNotification = new
                {
                    to = result.DeliveryBoyFcmId,
                    data = new
                    {
                        title = "UPI Callback",
                        body = result,// JsonConvert.SerializeObject(result),
                        icon = "",
                        typeId = "",
                        notificationCategory = "",
                        notificationType = "Actionable",
                        notificationId = 1,
                        notify_type = "UPI Callback",
                        url = "",
                    }
                };
                try
                {
                    WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                    tRequest.Method = "post";
                    string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                    Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", ConfigurationManager.AppSettings["DFcmApiKey"]));

                    tRequest.ContentLength = byteArray.Length;
                    tRequest.ContentType = "application/json";
                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    String responseFromFirebaseServer = tReader.ReadToEnd();
                                }
                            }
                        }
                    }
                }
                catch (Exception asd)
                {
                    TextFileLogHelper.LogError("Error while sending qr webhook notification for Order : " + result.OrderId.ToString());
                }
            }

            return true;
        }

        [Route("VerifyQRPayment")]
        [HttpGet]
        public WebHookDbResponse VerifyQRPayment(int orderId, int cashAmount)
        {
            var helper = new ReadyToDispatchHelper();
            WebHookDbResponse webHookDbResponse = new WebHookDbResponse { OrderId = orderId, IsCaptured = false, AmountCaptured = 0 };
            bool isCaptured = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var authContext = new AuthContext())
            {
                var RazorpayQRPayment = authContext.PaymentResponseRetailerAppDb.FirstOrDefault(x => x.OrderId == orderId
                                            && x.status == "Success" && x.PaymentFrom == "Razorpay QR");

                isCaptured = RazorpayQRPayment != null && RazorpayQRPayment.amount == cashAmount;

                if (!isCaptured)
                {
                    var virtualAccountDb = authContext.RazorpayVirtualAccounts.FirstOrDefault(x => x.OrderId == orderId && x.IsActive && !x.IsProcessed);

                    var companyDetail = authContext.CompanyDetailsDB.FirstOrDefault();

                    if (virtualAccountDb != null && companyDetail.IsRazorpayEnable && !string.IsNullOrEmpty(companyDetail.RazorpayBaseUrl)
                            && !string.IsNullOrEmpty(companyDetail.RazorpayApiKeyId)
                            && !string.IsNullOrEmpty(companyDetail.RazorpayApiKeySecret)
                        )
                    {
                        var webhookDb = new RazorpayWebhookRequest
                        {
                            CreatedDate = DateTime.Now,
                            //ResponseJson = JsonConvert.SerializeObject(virtualAccountResponse),
                            VirtualAccountId = virtualAccountDb.VirtualAccountId,
                            CallType = 2,
                            Url = companyDetail.RazorpayBaseUrl + "virtual_accounts/" + virtualAccountDb.VirtualAccountId + "/payments"
                        };

                        var byteArray = Encoding.ASCII.GetBytes($"{companyDetail.RazorpayApiKeyId}:{companyDetail.RazorpayApiKeySecret}");
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                                //client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)));

                                var response = AsyncContext.Run(() => client.GetAsync(companyDetail.RazorpayBaseUrl + "virtual_accounts/" + virtualAccountDb.VirtualAccountId + "/payments"));
                                response.EnsureSuccessStatusCode();
                                string responseBody = response.Content.ReadAsStringAsync().Result;
                                var virtualAccountResponse = JsonConvert.DeserializeObject<VirtualAccountPaymentResponse>(responseBody);
                                if (virtualAccountResponse != null)
                                {
                                    webhookDb.ResponseJson = JsonConvert.SerializeObject(virtualAccountResponse);

                                    if ((virtualAccountResponse.items?.FirstOrDefault()?.captured).HasValue && virtualAccountResponse.items.FirstOrDefault().captured
                                        && virtualAccountResponse.items.FirstOrDefault().status == "captured")
                                    {
                                        var cashPayments = authContext.PaymentResponseRetailerAppDb.Where(x => x.OrderId == virtualAccountDb.OrderId
                                                                           && x.status == "Success" && !x.IsOnline).ToList();

                                        if (cashPayments != null && cashPayments.Any())
                                        {
                                            foreach (var item in cashPayments)
                                            {
                                                item.status = "Failed";
                                                item.statusDesc = "Due to UPI from DeliveryApp";
                                                item.UpdatedDate = DateTime.Now;
                                                authContext.Entry(item).State = EntityState.Modified;
                                            }
                                        }

                                        authContext.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                        {
                                            OrderId = virtualAccountDb.OrderId,
                                            status = "Success",
                                            CreatedDate = DateTime.Now,
                                            UpdatedDate = DateTime.Now,
                                            PaymentFrom = "Razorpay QR",
                                            statusDesc = "Due to Delivery",
                                            amount = virtualAccountResponse.items.FirstOrDefault().amount / 100,
                                            GatewayTransId = virtualAccountResponse.items.FirstOrDefault().id,
                                            IsOnline = true
                                        });
                                        if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                                        {
                                            if (authContext.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == virtualAccountDb.OrderId && z.TransactionId == virtualAccountResponse.items.FirstOrDefault().id) == null)
                                            {
                                                var Customers = authContext.Customers.Where(x => x.RazorpayCustomerId == virtualAccountDb.RazorpayCustomerId).FirstOrDefault();
                                                OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                                Opdl.OrderId = virtualAccountDb.OrderId;
                                                Opdl.IsPaymentSuccess = true;
                                                Opdl.IsLedgerAffected = "Yes";
                                                Opdl.PaymentDate = DateTime.Now;
                                                Opdl.TransactionId = virtualAccountResponse.items.FirstOrDefault().id;
                                                Opdl.IsActive = true;
                                                Opdl.CustomerId = Customers.CustomerId;
                                                authContext.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                                            }
                                        }
                                        virtualAccountDb.UpdatedDate = DateTime.Now;
                                        virtualAccountDb.IsProcessed = true;
                                        virtualAccountDb.IsActive = false;

                                        authContext.Entry(virtualAccountDb).State = EntityState.Modified;

                                        webHookDbResponse.AmountCaptured = virtualAccountResponse.items.FirstOrDefault().amount / 100;
                                        webHookDbResponse.IsCaptured = true;
                                        webHookDbResponse.OrderId = orderId;
                                    }


                                }

                            }
                        }
                        catch (Exception exe)
                        {
                            webhookDb.ResponseJson = JsonConvert.SerializeObject(exe);
                        }
                        authContext.RazorpayWebhookRequest.Add(webhookDb);
                        authContext.Commit();
                    }
                }
                else
                {
                    webHookDbResponse.AmountCaptured = Convert.ToInt32(RazorpayQRPayment.amount);
                    webHookDbResponse.IsCaptured = true;
                    webHookDbResponse.OrderId = orderId;
                }
            }
            return webHookDbResponse;
        }

        [Route("ConsumerQRWebhook")]
        [HttpPost]
        public bool ConsumerQRWebhook(ConsumerRazorPayWebHookResponse webHookResponse)
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                var helper = new API.Helper.Razorpay.RazorPaymentVerifyHelper();
                result = helper.GetConsumerWebhookResponse(webHookResponse,context);
            }
            return result;

        }

    }
}