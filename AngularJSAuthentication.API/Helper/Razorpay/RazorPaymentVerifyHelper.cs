using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External.Razorpay;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Razorpay.Api;
using Razorpay.Api.Errors;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace AngularJSAuthentication.API.Helper.Razorpay
{
    public class RazorPaymentVerifyHelper
    {
        public bool VerifySignature(string orderid, string paymentid, string signature, string key, string secretkey)
        {
            bool IsValidRequest = false;
            try
            {
                //var payload = orderid + '|' + paymentid;
                //Utils.verifyWebhookSignature(payload, signature, secretkey);

                RazorpayClient client = new RazorpayClient(key, secretkey);

                Dictionary<string, string> attributes = new Dictionary<string, string>();

                attributes.Add("razorpay_payment_id", paymentid);
                attributes.Add("razorpay_order_id", orderid);
                attributes.Add("razorpay_signature", signature);

                Utils.verifyPaymentSignature(attributes);
                IsValidRequest = true;
            }
            catch (SignatureVerificationError ex)
            {
                IsValidRequest = false;
            }
            return IsValidRequest;
        }
        //public string CapturePayment(double amount, RazorPayTransaction razorPayTransaction, Payment payment)
        //{

        //    Dictionary<string, object> options = new Dictionary<string, object>();

        //    double amountinpaise = amount * 100;
        //    options.Add("amount", amountinpaise);
        //    options.Add("currency", "INR");
        //    Payment paymentCaptured = payment.Capture(options);
        //    razorPayTransaction.RazorpayCaptureResult = paymentCaptured;
        //    string paymentId = Convert.ToString(paymentCaptured["id"]);
        //    return paymentId;

        //}

        //public string CaptureInvoicePayment(double amount, RazorPayInvoiceTransaction razorPayTransaction, Payment payment)
        //{

        //    Dictionary<string, object> options = new Dictionary<string, object>();

        //    double amountinpaise = amount * 100;
        //    options.Add("amount", amountinpaise);
        //    options.Add("currency", "INR");
        //    Payment paymentCaptured = payment.Capture(options);
        //    razorPayTransaction.RazorpayCaptureResult = paymentCaptured;
        //    string paymentId = Convert.ToString(paymentCaptured["id"]);
        //    return paymentId;

        //}

        public bool GetConsumerWebhookResponse(ConsumerRazorPayWebHookResponse webHookResponse, AuthContext context)
        {
            TextFileLogHelper.TraceLog("consumer razorpay json :" + JsonConvert.SerializeObject(webHookResponse));
            string paymentresponse = JsonConvert.SerializeObject(webHookResponse);
            var qrcode = webHookResponse.payload.qr_code.entity;
            TextFileLogHelper.TraceLog("consumer razorpay qrcode :" + JsonConvert.SerializeObject(qrcode));
            var payment = webHookResponse.payload.payment;
            string events = webHookResponse.@event;
            if (events != "qr_code.created")
            {
                if (qrcode != null)
                {
                    TextFileLogHelper.TraceLog("Inside Qrcode Code");
                    string razorpayqrid = qrcode.id;
                    var razorpayorder = context.ConsumerRazorPayOrders.Where(x => x.RazorPayQRId == razorpayqrid).FirstOrDefault();
                    if (razorpayorder != null)
                    {
                        razorpayorder.PaymentResponse = paymentresponse;
                        context.Entry(razorpayorder).State = EntityState.Modified;
                        context.Commit();
                        string status = qrcode.status;
                        if (status == "closed")
                        {
                            if (payment != null)
                            {
                                razorpayorder.RazorPayId = payment.entity.id;
                                razorpayorder.payments_amount_received = payment.entity.amount;
                                razorpayorder.IsSuccess = true;
                                razorpayorder.Status = "SUCCESS";
                            }
                            else
                            {
                                razorpayorder.IsSuccess = false;
                                razorpayorder.Status = "FAILED";
                                razorpayorder.QRCode_Status = status;
                            }
                        }
                        else
                        {
                            razorpayorder.IsSuccess = false;
                            razorpayorder.Status = "FAILED";
                            razorpayorder.QRCode_Status = status;
                        }
                        context.Entry(razorpayorder).State = EntityState.Modified;
                        context.Commit();
                    }
                    else
                    {
                        TextFileLogHelper.TraceLog("consumer razorpay razorpayorder not found :");
                    }

                    if (razorpayorder != null && razorpayorder.IsSuccess)
                    {
                        #region Send qr to Warehouse Mobile
                        try
                        {
                            var whid = context.DbOrderMaster.AsNoTracking().Where(x => x.OrderId == razorpayorder.OrderId).Select(s => s.WarehouseId).FirstOrDefault();

                            var whmobile = context.WarehouseQrDevices.FirstOrDefault(s => s.WarehouseId == whid && s.IsActive);
                            if (whmobile != null)
                            {
                                string jsonPath = ConfigurationManager.AppSettings["WhQrFcmJsonPath"];
                                var firebaseService = new FirebaseNotificationServiceHelper(jsonPath);
                                var notification = new AngularJSAuthentication.Model.Notification
                                {
                                    title = "qrcode_success",
                                    Message = $"Rs. {Convert.ToString(razorpayorder.OrderAmount)}",
                                    priority = "high",
                                    CompanyId = razorpayorder.OrderId
                                };

                                var notResult = AsyncContext.Run(() => firebaseService.SendNotificationAsync(whmobile.FcmId, notification, "qrcode_success"));


                            }
                        }
                        catch (Exception ex)
                        {
                            TextFileLogHelper.LogError(ex.ToString());
                        }

                        #endregion
                    }



                }
                else
                {
                    TextFileLogHelper.TraceLog("consumer razorpay qrcode not found :");
                }
            }
            else
            {
                TextFileLogHelper.TraceLog("consumer razorpay qrcode created :");
            }
            return true;
        }
    }
}