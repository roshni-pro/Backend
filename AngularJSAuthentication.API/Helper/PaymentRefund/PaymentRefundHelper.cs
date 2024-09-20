using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.ControllerV7;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters.PaymentRefund;
using AngularJSAuthentication.DataContracts.Masters.UPIPayment;
using AngularJSAuthentication.DataContracts.Transaction.EpayLater;
using AngularJSAuthentication.DataContracts.UdharCreditLending;
using AngularJSAuthentication.Model.PaymentRefund;
using CCA.Util;
using GenricEcommers.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Razorpay.Api;
using System.Text;
using AngularJSAuthentication.DataContracts.RazorPay;

namespace AngularJSAuthentication.API.Helper.PaymentRefund
{
    public class PaymentRefundHelper
    {

        private string Key = ConfigurationManager.AppSettings["RazorPayKey"].ToString();
        private string Secret = ConfigurationManager.AppSettings["RazorPaysecret"].ToString();

        private string PaymentIdKey
        {
            get
            {
                return "{#paymentId#}";
            }
        }
        public bool InsertPaymentRefundRequest(AuthContext context, PaymentRefundRequestDc addobj)
        {
            PaymentRefundRequest addpreq = Mapper.Map(addobj).ToANew<PaymentRefundRequest>();
            context.PaymentRefundRequests.Add(addpreq);
            context.Commit();

            #region insert History
            PaymentRefundHistory addHistory = new PaymentRefundHistory
            {
                PaymentRefundRequestId = addpreq.Id,
                Status = (int)PaymentRefundEnum.Initiated,
                CreatedDate = DateTime.Now,
                CreatedBy = addobj.CreatedBy
            };
            context.PaymentRefundHistories.Add(addHistory);
            #endregion
            return context.Commit() > 0;
        }

        #region refund job
        public async Task<bool> OrderPaymentRefundJob()
        {
            using (var context = new AuthContext())
            {
                var PaymentRefundRequestList = await context.Database.SqlQuery<PostPaymentRefundDc>("exec GetPostPaymentRefunds").ToListAsync();
                if (PaymentRefundRequestList != null && PaymentRefundRequestList.Any())
                {
                    var PaymentRefundApisList = await context.PaymentRefundApis.Where(x => x.IsActive && x.IsDeleted == false).ToListAsync();
                    foreach (var item in PaymentRefundRequestList.OrderBy(x => x.OrderId))
                    {
                        try
                        {
                            var PaymentRefundApi = PaymentRefundApisList.Where(z => z.ApiName.Trim().ToLower() == item.Source.Trim().ToLower()).FirstOrDefault();
                            if (item.Source.Trim().ToLower() == "chqbook")
                            {
                                var responseupdate = await ChqbookRefund(item.OrderId, item.GatewayTransId, PaymentRefundApi);
                                responseupdate.PaymentRefundRequestId = item.PaymentRefundRequestId;
                                responseupdate.CustomerId = item.CustomerId;
                                responseupdate.OrderId = item.OrderId;

                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                            }
                            else if (item.Source.Trim().ToLower() == "gullak")
                            {
                                //var customer = await context.DbOrderMaster.Where(x => x.OrderId == item.OrderId).Select(x => new { x.CustomerId, x.OrderId }).FirstOrDefaultAsync();
                                //var customerGullak = await context.GullakDB.FirstOrDefaultAsync(x => x.CustomerId == item.CustomerId);
                                //if (customerGullak != null)
                                //{
                                var responseupdate = new PaymentRefundResDc
                                {
                                    PaymentRefundRequestId = item.PaymentRefundRequestId,
                                    Status = true,
                                    ResponseMsg = Convert.ToString(item.Amount),
                                    RequestMsg = Convert.ToString(item.Amount),
                                    CustomerId = item.CustomerId,
                                    OrderId = item.OrderId
                                };
                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                                //  }
                            }
                            else if (item.Source.Trim().ToLower() == "epaylater")
                            {
                                var responseupdate = await epaylaterRefund(item, PaymentRefundApi);
                                responseupdate.CustomerId = item.CustomerId;
                                responseupdate.OrderId = item.OrderId;
                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                            }
                            else if (item.Source.Trim().ToLower() == "directudhar")
                            {
                            
                                TextFileLogHelper.TraceLog("Processing item with Source: " + item.Source);
                                TextFileLogHelper.TraceLog("Calling DirectUdharRefund with CustomerId: " + item.CustomerId +
                                                            ", OrderId: " + item.OrderId);
                                var responseupdate = await DirectUdharRefund(context, item, PaymentRefundApi);
                                TextFileLogHelper.TraceLog("DirectUdharRefund response received. CustomerId: " + responseupdate.CustomerId +
                                                            ", OrderId: " + responseupdate.OrderId +
                                                            ", Status: " + responseupdate.Status +
                                                            ", ResponseMsg: " + responseupdate.ResponseMsg);
                                responseupdate.CustomerId = item.CustomerId;
                                responseupdate.OrderId = item.OrderId;
                                TextFileLogHelper.TraceLog("Updating payment refund with CustomerId: " + responseupdate.CustomerId +
                                ", OrderId: " + responseupdate.OrderId);
                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                                // Log the result of the UpdatePaymentRefund method
                                TextFileLogHelper.TraceLog("UpdatePaymentRefund result: " + IsSuccess);
                            }
                            else if (item.Source.Trim().ToLower() == "hdfc")
                            {
                                var responseupdate = await hdfcRefund(item, PaymentRefundApi);
                                responseupdate.CustomerId = item.CustomerId;
                                responseupdate.OrderId = item.OrderId;
                                responseupdate.PaymentRefundRequestId = item.PaymentRefundRequestId;
                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                            }
                            else if (item.Source.Trim().ToLower() == "rtgs/neft")
                            {
                                //var customer = await context.DbOrderMaster.Where(x => x.OrderId == item.OrderId).Select(x => new { x.CustomerId, x.OrderId }).FirstOrDefaultAsync();
                                //if (customer != null)
                                //{
                                var responseupdate = new PaymentRefundResDc
                                {
                                    PaymentRefundRequestId = item.PaymentRefundRequestId,
                                    Status = true,
                                    ResponseMsg = Convert.ToString(item.Amount),
                                    RequestMsg = Convert.ToString(item.Amount),
                                    CustomerId = item.CustomerId,
                                    OrderId = item.OrderId
                                };
                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                                //}
                            }
                            else if (item.Source.Trim().ToLower() == "upi")
                            {
                                var UPITransaction = context.UPITransactions.FirstOrDefault(x => x.UPITxnID == item.GatewayTransId && x.OrderId == item.OrderId);
                                if (UPITransaction != null)
                                {
                                    var responseupdate = await UPIRefund(item, PaymentRefundApi, UPITransaction.ResponseMsg);
                                    responseupdate.CustomerId = item.CustomerId;
                                    responseupdate.OrderId = item.OrderId;
                                    responseupdate.PaymentRefundRequestId = item.PaymentRefundRequestId;

                                    bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                                }
                            }
                            else if (item.Source.Trim().ToLower() == "scaleup")
                            {
                                ScaleUpManager scaleUpManager = new ScaleUpManager();
                                var result = await scaleUpManager.RefundTransactionJob(item.OrderId.ToString(), item.Amount);

                                PaymentRefundResDc responseupdate = new PaymentRefundResDc();
                                responseupdate.CustomerId = item.CustomerId;
                                responseupdate.OrderId = item.OrderId;
                                responseupdate.PaymentRefundRequestId = item.PaymentRefundRequestId;
                                responseupdate.Status = result.status;
                                responseupdate.ResponseMsg = result.Message;
                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                            }
                            else if (item.Source.Trim().ToLower() == "paylater")
                            {
                                #region PayLaterCancelOrder

                                var responseupdate = new PaymentRefundResDc
                                {
                                    PaymentRefundRequestId = item.PaymentRefundRequestId,
                                    Status = true,
                                    ResponseMsg = Convert.ToString(item.Amount),
                                    RequestMsg = Convert.ToString(item.Amount),
                                    CustomerId = item.CustomerId,
                                    OrderId = item.OrderId
                                };
                                //PaymentRefundResDc responseupdate = new PaymentRefundResDc();
                                //responseupdate.CustomerId = item.CustomerId;
                                //responseupdate.OrderId = item.OrderId;
                                //responseupdate.PaymentRefundRequestId = item.PaymentRefundRequestId;
                                //responseupdate.Status = true;
                                //responseupdate.ResponseMsg = "Success";
                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                                #endregion
                            }
                            else if (item.Source.Trim().ToLower() == "razorpay")
                            {
                                var responseupdate = await RefundSinglePayment(item, PaymentRefundApi);
                                responseupdate.CustomerId = item.CustomerId;
                                responseupdate.OrderId = item.OrderId;
                                responseupdate.PaymentRefundRequestId = item.PaymentRefundRequestId;
                                bool IsSuccess = await UpdatePaymentRefund(context, responseupdate);
                            }
                        }
                        catch (Exception ex)
                        {
                            TextFileLogHelper.LogError("while Payment Refund Request, Messsage:" + ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString());
                            TextFileLogHelper.LogError("Payment Refund Request Item:" + (JsonConvert.SerializeObject(item)).ToString());
                        }
                    }
                }
            }
            return true;
        }
        public async Task<bool> UpdatePaymentRefund(AuthContext context, PaymentRefundResDc PaymentRefundRes)
        {
            bool result = false;

            #region insert History
            PaymentRefundHistory addHistory = new PaymentRefundHistory
            {
                PaymentRefundRequestId = PaymentRefundRes.PaymentRefundRequestId,
                RequestMsg = PaymentRefundRes.RequestMsg,
                ResponseMsg = PaymentRefundRes.ResponseMsg,
                Status = PaymentRefundRes.Status == true ? (int)PaymentRefundEnum.Success : (int)PaymentRefundEnum.Failed,
                CreatedDate = DateTime.Now,
                CreatedBy = 0,
            };
            bool IsInsert = await InsertPaymentRefundHistory(context, addHistory);
            #endregion

            #region 
            var PaymentRefundRequest = await context.PaymentRefundRequests.Where(x => x.Id == PaymentRefundRes.PaymentRefundRequestId).FirstOrDefaultAsync();
            if (PaymentRefundRequest != null && PaymentRefundRequest.Status == 0 && PaymentRefundRequest.IsActive == true && PaymentRefundRequest.RefundType == 0 && PaymentRefundRequest.IsDeleted == false)
            {
                var PaymentResponseRetailerApp = await context.PaymentResponseRetailerAppDb.Where(x => x.id == PaymentRefundRequest.PaymentResponseRetailerAppId).FirstOrDefaultAsync();
                if (PaymentResponseRetailerApp != null)
                {
                    PaymentRefundRequest.Status = PaymentRefundRes.Status == true ? (int)PaymentRefundEnum.Success : (int)PaymentRefundEnum.Failed;
                    PaymentRefundRequest.RequestMsg = PaymentRefundRes.RequestMsg;
                    PaymentRefundRequest.ResponseMsg = PaymentRefundRes.ResponseMsg;
                    PaymentRefundRequest.ModifiedDate = DateTime.Now;

                    PaymentResponseRetailerApp.IsApproved = PaymentRefundRes.Status == true ? 1 : 0;
                    PaymentResponseRetailerApp.IsRefund = PaymentRefundRes.Status == true ? true : false;
                    PaymentResponseRetailerApp.GatewayRequest = PaymentRefundRes.RequestMsg;
                    PaymentResponseRetailerApp.GatewayResponse = PaymentRefundRes.ResponseMsg;
                    if (PaymentRefundRes.Status == true)
                    {
                        PaymentResponseRetailerApp.ApproveDate = DateTime.Now;
                    }
                    PaymentResponseRetailerApp.UpdatedDate = DateTime.Now;
                    PaymentResponseRetailerApp.statusDesc = PaymentRefundRes.Status == true ? "Refund Success" : "Refund Failed";

                    if (PaymentRefundRequest.Source.Trim().ToLower() == "gullak")
                    {
                        var Idparam = new SqlParameter("@PaymentRefundRequestId", PaymentRefundRes.PaymentRefundRequestId);
                        var requestMsgparam = new SqlParameter("@RequestMsg", PaymentRefundRes.RequestMsg);
                        var responseMsgparam = new SqlParameter("@ResponseMsg", PaymentRefundRes.ResponseMsg);
                        var statusparam = new SqlParameter("status", PaymentRefundRes.Status);
                        int count = context.Database.ExecuteSqlCommand("exec GullakPaymentRefund @PaymentRefundRequestId, @RequestMsg, @ResponseMsg, @status ", Idparam, requestMsgparam, responseMsgparam, statusparam);
                    }
                    else if (PaymentRefundRequest.Source.Trim().ToLower() == "rtgs/neft")
                    {
                        var Idparam = new SqlParameter("@PaymentRefundRequestId", PaymentRefundRes.PaymentRefundRequestId);
                        var statusparam = new SqlParameter("status", PaymentRefundRes.Status);
                        int count = context.Database.ExecuteSqlCommand("exec RTGSNEFTPaymentRefund @PaymentRefundRequestId, @status ", Idparam, statusparam);
                    }
                    context.Entry(PaymentRefundRequest).State = EntityState.Modified;
                    context.Entry(PaymentResponseRetailerApp).State = EntityState.Modified;
                    result = context.Commit() > 0;
                }
            }
            #endregion
            return result;
        }
        #endregion

        #region  ChqbookRefund   (Partial Payment not allowed)
        private async Task<PaymentRefundResDc> ChqbookRefund(int OrderId, string GatewayTransId, PaymentRefundApi PaymentRefundApis)
        {
            PaymentRefundResDc PaymentRefundRes = new PaymentRefundResDc();
            ChqbookRefundDc reqt = new ChqbookRefundDc
            {
                OrderId = Convert.ToString(OrderId),
                GatewayTransId = GatewayTransId
            };
            PaymentRefundRes.RequestMsg = JsonConvert.SerializeObject(reqt);
            using (var httpClient = new HttpClient())
            {
                try
                {
                    using (var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), PaymentRefundApis.ApiUrl))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                        request.Headers.TryAddWithoutValidation("api-key", PaymentRefundApis.ApiSecret);
                        var contentList = new List<string>();
                        contentList.Add($"accountProvider={Uri.EscapeDataString("SHOP_KIRANA")}");
                        contentList.Add($"transactionId={Uri.EscapeDataString(reqt.GatewayTransId)}");
                        contentList.Add($"partnerTxRef={Uri.EscapeDataString(reqt.OrderId)}");
                        request.Content = new StringContent(string.Join("&", contentList));
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                        var response = await httpClient.SendAsync(request);
                        string jsonString = string.Empty;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            ChqbookBookRefundResponse res = JsonConvert.DeserializeObject<ChqbookBookRefundResponse>(jsonString);
                            PaymentRefundRes.ResponseMsg = jsonString;
                            PaymentRefundRes.Status = res.data.message == "Successful" ? true : false;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            PaymentRefundRes.ResponseMsg = jsonString;
                            RefundError res = JsonConvert.DeserializeObject<RefundError>(jsonString);
                            PaymentRefundRes.Status = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                    TextFileLogHelper.LogError("ChqbookBookRefund error : " + reqt.GatewayTransId + " error log : " + error);
                    PaymentRefundRes.Status = false;
                    PaymentRefundRes.ResponseMsg = error;
                }
            }
            return PaymentRefundRes;
        }
        #endregion

        #region epaylater  full/partial
        private async Task<PaymentRefundResDc> epaylaterRefund(PostPaymentRefundDc item, PaymentRefundApi PaymentRefundApis)
        {
            PaymentRefundResDc PaymentRefundRes = new PaymentRefundResDc();
            var url = PaymentRefundApis.ApiUrl;
            EpayLaterRefundRequestDc refrequest = new EpayLaterRefundRequestDc();
            url = url.Replace("[[GatewayOrderId]]", item.GatewayOrderId);
            refrequest = new EpayLaterRefundRequestDc
            {
                marketplaceOrderId = item.GatewayOrderId,
                refundDate = DateTime.Now,
                returnAcceptedDate = DateTime.Now,
                returnAmount = item.Amount * 100,
                returnShipmentReceivedDate = DateTime.Now,
                returnType = item.returnType
            };
            var newJson = JsonConvert.SerializeObject(refrequest);
            TextFileLogHelper.LogError("DirectUdharRefund: Prepared refund request JSON: " + newJson);

            PaymentRefundRes.RequestMsg = newJson;
            PaymentRefundRes.PaymentRefundRequestId = item.PaymentRefundRequestId;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    using (var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("PUT"), url))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        request.Headers.TryAddWithoutValidation("Accept", "*/*");
                        request.Headers.TryAddWithoutValidation("noencryption", "1");
                        request.Headers.TryAddWithoutValidation("Authorization", PaymentRefundApis.ApiSecret);
                        request.Content = new StringContent(newJson);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var res = await httpClient.SendAsync(request);

                        if (res.StatusCode == HttpStatusCode.Accepted)
                        {
                            string jsonString = (await res.Content.ReadAsStringAsync());
                            var EpayLaterRefundAcceptResponse = JsonConvert.DeserializeObject<EpayLaterRefundAcceptResponseDc>(jsonString);
                            PaymentRefundRes.ResponseMsg = jsonString;
                            PaymentRefundRes.Status = (EpayLaterRefundAcceptResponse.Status == "partially_returned" || EpayLaterRefundAcceptResponse.Status == "return_accepted") == true ? true : false;

                        }
                        else if (res.StatusCode == HttpStatusCode.BadRequest || res.StatusCode == HttpStatusCode.NotFound)
                        {
                            string jsonString = (await res.Content.ReadAsStringAsync());
                            PaymentRefundRes.ResponseMsg = jsonString;
                            PaymentRefundRes.Status = false;
                        }
                        else
                        {
                            string jsonString = (await res.Content.ReadAsStringAsync());
                            PaymentRefundRes.ResponseMsg = jsonString;
                            PaymentRefundRes.Status = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                    TextFileLogHelper.LogError("epaylaterRefund error : " + item.GatewayTransId + " error log : " + error);
                    PaymentRefundRes.Status = false;
                    PaymentRefundRes.ResponseMsg = error;
                }
            }

            return PaymentRefundRes;
        }
        #endregion

        #region DirectUdhar full/partial
        private async Task<PaymentRefundResDc> DirectUdharRefund(AuthContext context, PostPaymentRefundDc PostPaymentRefund, PaymentRefundApi PaymentRefundApis)
        {
            PaymentRefundResDc result = new PaymentRefundResDc();
            var customer = new CustomerDc();
            TextFileLogHelper.TraceLog("DirectUdharRefund method called with OrderId: " + PostPaymentRefund.OrderId + ", CustomerId: " + PostPaymentRefund.CustomerId);

            var param = new SqlParameter("@Id", PostPaymentRefund.CustomerId);
            customer = context.Database.SqlQuery<CustomerDc>("Exec GetCustomerInfoById @Id", param).FirstOrDefault();
            
            if (customer != null && customer.CreditLendingApiKey != null && customer.CreditLendingApiKeySecret != null)
            {
                TextFileLogHelper.TraceLog("DirectUdharRefund: Customer retrieved successfully with CreditLendingApiKey: " + customer.CreditLendingApiKey);

                SkCreditLendingController skcontroller = new SkCreditLendingController();
                var CreditLendingToken = await skcontroller.GenerateTokenCreditLending(customer);
                if (CreditLendingToken != null)
                {
                    RefundPaymentRequestDc refrequest = new RefundPaymentRequestDc();
                    refrequest = new RefundPaymentRequestDc
                    {
                        OrderId = PostPaymentRefund.OrderId,
                        Amount = PostPaymentRefund.Amount,
                        TrasanctionId = PostPaymentRefund.GatewayTransId,
                        returnType = PostPaymentRefund.returnType
                    };
                    var newJson = JsonConvert.SerializeObject(refrequest);

                    refrequest.CustomerId = PostPaymentRefund.CustomerId;
                    result.RequestMsg = JsonConvert.SerializeObject(refrequest);
                    result.PaymentRefundRequestId = PostPaymentRefund.PaymentRefundRequestId;

                    using (var httpClient = new HttpClient())
                    {
                        try
                        {
                            using (var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), PaymentRefundApis.ApiUrl))
                            {
                                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                                request.Headers.TryAddWithoutValidation("noencryption", "1");
                                request.Headers.TryAddWithoutValidation("Authorization", CreditLendingToken.token_type + " " + CreditLendingToken.access_token);
                                request.Content = new StringContent(newJson);
                                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                                TextFileLogHelper.TraceLog("DirectUdharRefund: Sending HTTP POST request to " + PaymentRefundApis.ApiUrl);


                                var response = await httpClient.SendAsync(request);
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    string jsonString = (await response.Content.ReadAsStringAsync());
                                    var Extractres = JsonConvert.DeserializeObject<CreditLendingResponseMasterDc>(jsonString);
                                    var res = Extractres.Data;
                                    if (res != null && res.Result && res.Msg == null)
                                    {
                                        result.ResponseMsg = jsonString;
                                        result.Status = res.Result;
                                        TextFileLogHelper.TraceLog("DirectUdharRefund: API response processed successfully. Status: " + res.Result);
                                    }
                                    else
                                    {
                                        result.Status = res.Result;
                                        result.ResponseMsg = res.Msg;
                                        TextFileLogHelper.TraceLog("DirectUdharRefund: API response contains message. Status: " + res.Result + ", Message: " + res.Msg);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                            TextFileLogHelper.LogError("DirectUdhar Refund error : " + PostPaymentRefund.GatewayTransId + " error log : " + error);
                            result.Status = false;
                            result.ResponseMsg = error;
                        }
                    }
                }
                else
                {
                    result.Status = false;
                    result.ResponseMsg = "current request is unauthorized for payment";
                }
            }

            return result;
        }
        #endregion

        #region Hdfc 
        private async Task<PaymentRefundResDc> hdfcRefund(PostPaymentRefundDc item, PaymentRefundApi PaymentRefundApis)
        {
            PaymentRefundResDc result = new PaymentRefundResDc();
            try
            {
                HdfcRefundPostDc req = new HdfcRefundPostDc
                {
                    refund_amount = item.Amount,
                    reference_no = item.GatewayTransId,
                    refund_ref_no = "RF" + DateTime.Now.Ticks.ToString()
                };
                result.RequestMsg = JsonConvert.SerializeObject(req);
                result.PaymentRefundRequestId = item.PaymentRefundRequestId;

                string merchantId = ConfigurationManager.AppSettings["CcAvenueMerchantId"];  //"222355";
                string accessCode = ConfigurationManager.AppSettings["CcAvenueAccessCode"];  //"AVCT02GF76BJ43TCJB";

                string workingKey = ConfigurationManager.AppSettings["WorkingKey"];// from avenues
                string orderStatusQueryJson = result.RequestMsg;// "{ \"reference_no\":\"103001198924\", \"order_no\":\"77141516\" }"; //Ex. { "reference_no":"CCAvenue_Reference_No" , "order_no":"123456"} 
                string encJson = "";

                string queryUrl = PaymentRefundApis.ApiUrl;

                CCACrypto ccaCrypto = new CCACrypto();
                encJson = ccaCrypto.Encrypt(orderStatusQueryJson, workingKey);

                // make query for the status of the order to ccAvenues change the command param as per your need
                string authQueryUrlParam = "enc_request=" + encJson + "&access_code=" + accessCode + "&command=refundOrder&request_type=JSON&response_type=JSON";

                // Url Connection
                String message = postPaymentRequestToGateway(queryUrl, authQueryUrlParam);
                //Response.Write(message);
                NameValueCollection param = getResponseMap(message);
                String status = "";
                String encResJson = "";
                if (param != null && param.Count == 2)
                {
                    for (int i = 0; i < param.Count; i++)
                    {
                        if ("status".Equals(param.Keys[i]))
                        {
                            status = param[i];
                        }
                        if ("enc_response".Equals(param.Keys[i]))
                        {
                            encResJson = param[i];
                            //Response.Write(encResXML);
                        }
                    }
                    if (!"".Equals(status) && status.Equals("0"))
                    {
                        String ResJson = ccaCrypto.Decrypt(encResJson, workingKey);
                        result.ResponseMsg = ResJson;
                        RefundAPIResponseDc resobject = new RefundAPIResponseDc();
                        resobject = JsonConvert.DeserializeObject<RefundAPIResponseDc>(ResJson);
                        if (resobject.Refund_Order_Result.refund_status == "0") { result.Status = true; } else { result.Status = false; }
                        return result;
                    }
                    else if (!"".Equals(status) && status.Equals("1"))
                    {
                        String ResJson = ccaCrypto.Decrypt(encResJson, workingKey);
                        result.ResponseMsg = ResJson;
                        result.Status = false;
                    }

                }

            }
            catch (Exception ex)
            {
                String error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                TextFileLogHelper.LogError("hdfc Refund error : " + item.GatewayTransId + " error log : " + error);
                result.Status = false;
                result.ResponseMsg = error;

            }
            return result;
        }
        private string postPaymentRequestToGateway(String queryUrl, String urlParam)
        {

            String message = "";
            try
            {
                StreamWriter myWriter = null;// it will open a http connection with provided url
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                WebRequest objRequest = WebRequest.Create(queryUrl);//send data using objxmlhttp object
                objRequest.Method = "POST";
                //objRequest.ContentLength = TranRequest.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";//to set content type
                myWriter = new System.IO.StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(urlParam);//send data
                myWriter.Close();//closed the myWriter object

                // Getting Response
                System.Net.HttpWebResponse objResponse = (System.Net.HttpWebResponse)objRequest.GetResponse();//receive the responce from objxmlhttp object 
                using (System.IO.StreamReader sr = new System.IO.StreamReader(objResponse.GetResponseStream()))
                {
                    message = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                String error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                TextFileLogHelper.LogError("HDFC Refund Exception occured while connection. : " + error);

            }
            return message;

        }
        private NameValueCollection getResponseMap(String message)
        {
            NameValueCollection Params = new NameValueCollection();
            if (message != null || !"".Equals(message))
            {
                string[] segments = message.Split('&');
                foreach (string seg in segments)
                {
                    string[] parts = seg.Split('=');
                    if (parts.Length > 0)
                    {
                        string Key = parts[0].Trim();
                        string Value = parts[1].Trim();
                        Params.Add(Key, Value);
                    }
                }
            }
            return Params;
        }
        #endregion

        #region  InsertPaymentRefundHistory
        public async Task<bool> InsertPaymentRefundHistory(AuthContext context, PaymentRefundHistory addobj)
        {
            context.PaymentRefundHistories.Add(addobj);
            return context.Commit() > 0;
        }
        #endregion


        #region UPI  full/partial
        private async Task<PaymentRefundResDc> UPIRefund(PostPaymentRefundDc item, PaymentRefundApi PaymentRefundApis, string UPIPaymentRes)
        {
            var upiRes = JsonConvert.DeserializeObject<callBackResDc>(UPIPaymentRes);
            string pgMerchantId = ConfigurationManager.AppSettings["pgMerchantId"];
            PaymentRefundResDc PaymentRefundRes = new PaymentRefundResDc();
            var url = PaymentRefundApis.ApiUrl;
            PaymentRefundRes.PaymentRefundRequestId = item.PaymentRefundRequestId;
            // PGMerchantId | NewOrderNo | Original Order No| Original Trn Ref No | Original Cust Ref No | Remarks | Refund AMT | Currency | Transaction Type | Payment Type | add1 | add2 | add3 | add4 | add5 | add6 | add7 | add8 | add9 | add10
            string quaryparam = pgMerchantId + "|" + DateTime.Now.ToString("ddMMyyyyHHmmss") + "|" + upiRes.MerchantTrnxReference + "|" + upiRes.UPITxnID + "|" + upiRes.CustomerReferenceNo + "|RefundRemark|" + item.Amount + "|INR|P2P|PAY|||||||||NA|NA";
            string quaryparamEncrypt = UPIKitHelper.Encrypt(quaryparam, PaymentRefundApis.ApiSecret);
            TextFileLogHelper.TraceLog("UPI Payment  RefundAPI  : " + quaryparamEncrypt);

            PostInetentReqDc PostInetentReq = new PostInetentReqDc();
            PostInetentReq.pgMerchantId = pgMerchantId;
            PostInetentReq.requestMsg = quaryparamEncrypt;
            var PostJson = JsonConvert.SerializeObject(PostInetentReq);
            PaymentRefundRes.RequestMsg = PostJson;
            TextFileLogHelper.TraceLog("UPI Payment  RefundAPI  Post Json : " + PostJson);

            var PayInetentReqResDc = new PayInetentReqResDc();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var EndPoint = url;
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            using (var httpClient = new HttpClient(httpClientHandler))
            {
                try
                {
                    using (var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), EndPoint))
                    {
                        request.Headers.TryAddWithoutValidation("Accept", "*/*");
                        request.Content = new StringContent(PostJson);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await httpClient.SendAsync(request);
                        if (HttpStatusCode.OK == response.StatusCode)
                        {
                            string responseBody = response.Content.ReadAsStringAsync().Result;
                            TextFileLogHelper.TraceLog("UPI Payment response RefundAPI Start Decrypt : " + responseBody);
                            var SplitRes = responseBody.Split('&');
                            string decData = UPIKitHelper.Decrypt(responseBody, PaymentRefundApis.ApiSecret);
                            TextFileLogHelper.TraceLog("UPI Payment response RefundAPI end Decrypt: " + decData);
                            string[] Res = decData.Split('|');

                            TextFileLogHelper.TraceLog("UPI Payment response RefundAPI end Decrypt Description: " + Res[5]);

                            var Response = new callBackResDc();
                            Response.UPITxnID = !string.IsNullOrEmpty(Res[0]) ? Convert.ToInt64(Res[0]) : 0;
                            Response.MerchantTrnxReference = Res[1];
                            Response.Amount = !string.IsNullOrEmpty(Res[2]) && Convert.ToDouble(Res[2]) > 0 ? Convert.ToDouble(Res[2]) : 0;
                            Response.TransactionAuthDate = Res[3];
                            Response.Status = Res[4];
                            Response.StatusDescription = Res[5];
                            Response.ResponseCode = Res[6];
                            Response.ApprovalNumber = Res[7];
                            Response.PayerVirtualAddress = Res[8];
                            Response.CustomerReferenceNo = Res[9];
                            Response.ReferenceID = Res[10];
                            Response.AdditionalField1 = Res[11];
                            Response.AdditionalField2 = Res[12];
                            Response.AdditionalField3 = Res[13];
                            Response.AdditionalField4 = Res[14];
                            Response.AdditionalField5 = Res[15];
                            Response.AdditionalField6 = Res[16];
                            Response.AdditionalField7 = Res[17];
                            Response.AdditionalField8 = Res[18];
                            Response.AdditionalField9 = Res[19];
                            Response.AdditionalField10 = Res[20];
                            Response.pgMerchantId = pgMerchantId;
                            string jsonInRes = JsonConvert.SerializeObject(Response);
                            if (Response.StatusDescription.Trim().ToLower() == "Refund Already Processed".Trim().ToLower())
                            {
                                PaymentRefundRes.ResponseMsg = jsonInRes;
                                PaymentRefundRes.Status = true;

                            }

                            else if (!string.IsNullOrEmpty(Response.MerchantTrnxReference))
                            {
                                PaymentRefundRes.ResponseMsg = jsonInRes;
                                PaymentRefundRes.Status = true;

                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
                        {
                            string jsonString = (await response.Content.ReadAsStringAsync());
                            PaymentRefundRes.ResponseMsg = jsonString;
                            PaymentRefundRes.Status = false;
                        }
                        else
                        {
                            string jsonString = (await response.Content.ReadAsStringAsync());
                            PaymentRefundRes.ResponseMsg = jsonString;
                            PaymentRefundRes.Status = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                    TextFileLogHelper.LogError("upi error : " + item.GatewayTransId + " error log : " + error);
                    PaymentRefundRes.Status = false;
                    PaymentRefundRes.ResponseMsg = error;
                }
            }
            return PaymentRefundRes;
        }
        #endregion
        #region RazorPayRefund
        //private async Task<PaymentRefundResDc> RazorpayRefund(PostPaymentRefundDc item, PaymentRefundApi PaymentRefundApis)
        //{
        //    bool results = false;
        //    PaymentRefundResDc result = new PaymentRefundResDc();
        //    ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 |
        //   (SecurityProtocolType)768 | (SecurityProtocolType)3072;


        //   RazorpayClient client = new RazorpayClient(Key, Secret);

        //    //string paymentId = "pay_Z6t7VFTb9xHeOs";
        //    Dictionary<string, object> refundRequest = new Dictionary<string, object>();
        //    refundRequest.Add("amount", item.Amount * 100);
        //    refundRequest.Add("speed", "normal");
        //    Dictionary<string, object> notes = new Dictionary<string, object>();
        //    notes.Add("notes_key_1", "Order Canceled");
        //    notes.Add("notes_key_2", "Order Canceled");
        //    refundRequest.Add("notes", notes);
        //    refundRequest.Add("receipt", "Receipt No." + item.OrderId + "");
        //    result.RequestMsg = JsonConvert.SerializeObject(refundRequest);
        //    Refund refund = client.Payment.Fetch(item.GatewayTransId).Refund(refundRequest);
        //    result.ResponseMsg = refund.Attributes;
        //    results = true;
        //    return result;
        //}
        #endregion
        #region RazorpayRefund
        public async Task<PaymentRefundResDc> RefundSinglePayment(PostPaymentRefundDc item, PaymentRefundApi PaymentRefundApis)
        {
            PaymentRefundResDc result = new PaymentRefundResDc();
            using (var httpClient = new HttpClient())
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string url = PaymentRefundApis.ApiUrl;
                url = url.Replace(PaymentIdKey, item.GatewayTransId);
                using (var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), url))
                {
                    string authorizationstr = PaymentRefundApis.ApiKey + ":" + PaymentRefundApis.ApiSecret;
                    var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(authorizationstr));
                    request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                    PaymentRefundDc paymentRefundDc = new PaymentRefundDc
                    {
                        amount = (Convert.ToInt32(item.Amount * 100)),//amountInPaisa
                        reverse_all = 1
                    };
                    request.Content = new StringContent(JsonConvert.SerializeObject(paymentRefundDc));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    try
                    {
                        result.RequestMsg = request.ToString();
                        var response = await httpClient.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            RazorpayTransferResponse res = JsonConvert.DeserializeObject<RazorpayTransferResponse>(responseBody);
                            result.ResponseMsg = responseBody;
                            result.Status = true;
                        }
                        else
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            ErrorRazorpayResponse res = JsonConvert.DeserializeObject<ErrorRazorpayResponse>(responseBody);
                            result.ResponseMsg = responseBody;
                            result.Status = false;
                            //throw new Exception("Razorpay refund api issue!");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            return result;
        }
        #endregion

    }
}