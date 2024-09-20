using AngularJSAuthentication.DataContracts.RazorPay;
using AngularJSAuthentication.Model.RazorPay;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using AngularJSAuthentication.DataContracts.Masters.PaymentRefund;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AngularJSAuthentication.API.Helper.Razorpay
{
    public class RazorPayTransactionHelper
    {

        private string Key = ConfigurationManager.AppSettings["RazorPayKey"].ToString();
        private string Secret = ConfigurationManager.AppSettings["RazorPaysecret"].ToString();
        private string paymentAuthorizedStatus = "AUTHORIZED";
        private string paymentCapturedStatus = "CAPTURED";


        public async Task<string> CreateOrderAsync(long orderId, int orderAmountInRs, int userId, AuthContext context)
        {
            decimal? outstandingAmount = orderAmountInRs;
            if (outstandingAmount.HasValue && outstandingAmount.Value > 0)
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 |
                    (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                decimal amount = outstandingAmount.Value * 100;
                amount = Math.Floor(amount);
                RazorpayClient client = new RazorpayClient(Key, Secret);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", amount); // amount in the smallest currency unit
                //options.Add("receipt", "order_rcptid_11");
                options.Add("currency", "INR");
                Order order = client.Order.Create(options);
                string razorPayOrderId = order["id"].ToString();

                if (!string.IsNullOrEmpty(razorPayOrderId))
                {
                    context.RazorpayOrders.Add(new RazorpayOrder
                    {
                        Amount = outstandingAmount.Value,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        OrderId = orderId,
                        RazorpayOrderId = razorPayOrderId,
                        Status = "created",
                        TryCount = 0,
                        ModifiedBy = null,
                        ModifiedDate = null,
                        razorpay_signature = null,
                        razorpay_payment_id = null
                    });
                    context.Commit();
                }
                return razorPayOrderId;
            }
            else
            {
                return string.Empty;
            }
        }
        public async Task<bool> PostRazorPayTransactionAsync(RazorPayTransactionDC razorPayTransactionDC, long createdBy, AuthContext context)
        {
            bool finalresult = false;
            bool vsResult = false;
            string generated_signature = string.Empty;
            if (razorPayTransactionDC != null && razorPayTransactionDC.response != null)
            {
                generated_signature = calculateRFC2104HMAC(razorPayTransactionDC.response.razorpay_order_id + "|" + razorPayTransactionDC.response.razorpay_payment_id, Secret);

                if (generated_signature == razorPayTransactionDC.response.razorpay_signature)
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 |
                        (SecurityProtocolType)768 | (SecurityProtocolType)3072;

                    RazorPaymentVerifyHelper razorPaymentVerifyHelper = new RazorPaymentVerifyHelper();
                    vsResult = razorPaymentVerifyHelper.VerifySignature(razorPayTransactionDC.response.razorpay_order_id, razorPayTransactionDC.response.razorpay_payment_id, razorPayTransactionDC.response.razorpay_signature, Key, Secret);

                    var razorpayOrders = context.RazorpayOrders.FirstOrDefault(x => x.OrderId == razorPayTransactionDC.OrderId
                      && x.TryCount < 5 && x.Status == "created" && x.IsActive == true && x.IsDeleted == false);
                    if (razorpayOrders != null)
                    {
                        razorpayOrders.razorpay_payment_id = razorPayTransactionDC.response.razorpay_payment_id;
                        razorpayOrders.razorpay_signature = razorPayTransactionDC.response.razorpay_signature;
                        context.Entry(razorpayOrders).State = EntityState.Modified;

                        RazorpayClient client = new RazorpayClient(Key, Secret);
                        string razorpay_orderid = razorPayTransactionDC.response.razorpay_order_id;
                        Order order = client.Order.Fetch(razorpay_orderid);
                        if (order.Payments() != null && order.Payments().Any() && vsResult)
                        {
                            Payment payment = order.Payments().First();
                            string paymentId = Convert.ToString(payment["id"]);
                            string paymentStatus = payment["status"].ToString().ToUpper();
                            if (paymentStatus == paymentCapturedStatus)
                            {
                                razorpayOrders.TryCount += 1;
                                razorpayOrders.Status = "captured";
                                finalresult = true;
                            }
                            else if (paymentStatus == paymentAuthorizedStatus)
                            {
                                Dictionary<string, object> options = new Dictionary<string, object>();
                                double amountinpaise = razorPayTransactionDC.Amount;
                                options.Add("amount", amountinpaise);
                                options.Add("currency", "INR");
                                Payment paymentCaptured = payment.Capture(options);
                                paymentId = Convert.ToString(paymentCaptured["id"]);
                                if (!string.IsNullOrEmpty(paymentId))
                                {
                                    razorpayOrders.TryCount += 1;
                                    razorpayOrders.Status = "captured";
                                    finalresult = true;
                                }
                                else
                                {
                                    razorpayOrders.TryCount += 1;
                                    razorpayOrders.Status = "failed";
                                }
                            }
                        }
                        else
                        {
                            razorpayOrders.TryCount += 1;
                            razorpayOrders.Status = "failed";
                        }
                        context.Entry(razorpayOrders).State = EntityState.Modified;
                        context.Commit();
                    }
                }
            }
            return finalresult;
        }
        public string calculateRFC2104HMAC(string data, string Secret)
        {
            string result = string.Empty;
            try
            {
                using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(Secret)))
                {
                    var payload = Encoding.ASCII.GetBytes(data);
                    var rawhmac = hmac.ComputeHash(payload);
                    result = HashEncode(rawhmac);
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return result;
        }
        private string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        public async Task<bool> RazorpayRefund(string dfs, int OrderId, double amt)
        {
            bool results = false;
            PaymentRefundResDc result = new PaymentRefundResDc();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 |
           (SecurityProtocolType)768 | (SecurityProtocolType)3072;


            RazorpayClient client = new RazorpayClient(Key, Secret);

            //string paymentId = "pay_Z6t7VFTb9xHeOs";
            Dictionary<string, object> refundRequest = new Dictionary<string, object>();
            refundRequest.Add("amount", amt * 100);
            refundRequest.Add("speed", "normal");
            Dictionary<string, object> notes = new Dictionary<string, object>();
            notes.Add("notes_key_1", "Order Canceled");
            notes.Add("notes_key_2", "Order Canceled");
            refundRequest.Add("notes", notes);
            refundRequest.Add("receipt", "Receipt No." + OrderId + "");

            result.RequestMsg = JsonConvert.SerializeObject(refundRequest);
            Refund refund = client.Payment.Fetch(dfs).Refund(refundRequest);
            result.ResponseMsg = refund.Attributes;
            results = true;
            return results;
        }
    
        //public async Task RefundSinglePayment(string paymentId, int amount)
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //        string url = RefundPaymenUrl;
        //        url = url.Replace(PaymentIdKey, paymentId);
        //        using (var request = new HttpRequestMessage(new System.Net.Http.HttpMethod("POST"), url))
        //        {
        //            string authorizationstr = Key + ":" + Secret;
        //            var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(authorizationstr));
        //            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

        //            PaymentRefundDc paymentRefundDc = new PaymentRefundDc
        //            {
        //                amount = (amount * 100),//amountInPaisa
        //                reverse_all = 1
        //            };
        //            request.Content = new StringContent(JsonConvert.SerializeObject(paymentRefundDc));
        //            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        //            try
        //            {
        //                var response = await httpClient.SendAsync(request);
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    string responseBody = await response.Content.ReadAsStringAsync();
        //                    RazorpayTransferResponse res = JsonConvert.DeserializeObject<RazorpayTransferResponse>(responseBody);

        //                }
        //                else
        //                {
        //                    throw new Exception("Razorpay refund api issue!");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }

        //        }
        //    }
        //}
       
    }
}