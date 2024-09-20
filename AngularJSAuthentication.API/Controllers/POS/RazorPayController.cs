using AngularJSAuthentication.DataContracts.RazorPay.POS;
using AngularJSAuthentication.Model.RazorPay;
using Newtonsoft.Json;
using Rest;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthentication.Common.Helpers;

namespace AngularJSAuthentication.API.Controllers.POS
{
    [RoutePrefix("api/RazorPayPos")]
    public class RazorPayController : ApiController
    {
        [HttpPost]
        [Route("GeneratePos")]
        public async Task<RazorPayPosResponse> GeneratePos(RazorPayPosRequest razorPayPosRequest)
        {
            RazorPayPosResponse res = new RazorPayPosResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                var warehousemachine = new WarehousePosMachine();

                if (razorPayPosRequest.WarehousePosMachineId.HasValue)
                {
                    warehousemachine = context.WarehousePosMachines.FirstOrDefault(x => x.Id == razorPayPosRequest.WarehousePosMachineId.Value);
                }
                else
                {
                    warehousemachine = context.WarehousePosMachines.FirstOrDefault(x => x.WarehouseId == razorPayPosRequest.WarehouseId);
                }
                var razorpaycredetntial = context.RazorPayPosCredentials.FirstOrDefault();
                if (warehousemachine != null && razorpaycredetntial != null)
                {
                    var ordermaster = context.DbOrderMaster.Where(x => x.OrderId == razorPayPosRequest.OrderId).FirstOrDefault();
                    DataContracts.RazorPay.POS.RazorPayPosNotificationRequest requestJson = new DataContracts.RazorPay.POS.RazorPayPosNotificationRequest
                    {
                        amount = Convert.ToDecimal(razorPayPosRequest.Amount * 100),
                        externalRefNumber = Convert.ToString(razorPayPosRequest.OrderId),
                        appKey = razorpaycredetntial.Appkey,
                        username = razorpaycredetntial.ApiUserName,
                        customerMobileNumber = ordermaster.Customerphonenum,
                        accountLabel = razorpaycredetntial.AccountLabel,
                        mode = "ALL",
                        pushTo = new Pushto
                        {
                            deviceId = warehousemachine.PosMachineId
                        }
                    };
                    string jsonData = JsonConvert.SerializeObject(requestJson);
                    TextFileLogHelper.TraceLog("GeneratePos Request Json"+ jsonData);
                    Model.RazorPay.RazorPayPosNotificationRequest razorPayPosNotificationRequest = new Model.RazorPay.RazorPayPosNotificationRequest
                    {
                        amount = Convert.ToDecimal(razorPayPosRequest.Amount * 100),
                        OrderId = razorPayPosRequest.OrderId,
                        accountLabel = razorpaycredetntial.AccountLabel,
                        appKey = razorpaycredetntial.Appkey,
                        CreatedBy = userid,
                        CreatedDate = DateTime.Now,
                        externalRefNumber = Convert.ToString(razorPayPosRequest.OrderId),
                        pushTo = warehousemachine.PosMachineId,
                        customerMobileNumber = ordermaster.Customerphonenum,
                        username = razorpaycredetntial.ApiUserName,
                        mode = "ALL",
                        IsActive = true,
                        IsDeleted = false,
                        RequestType = "Notification",
                        IsSuccess = false
                    };
                    context.RazorPayPosNotificationRequests.Add(razorPayPosNotificationRequest);
                    context.Commit();
                    if (!string.IsNullOrEmpty(razorpaycredetntial.NotificationApiUrl))
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var client = new RestSharp.RestClient(razorpaycredetntial.NotificationApiUrl);
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", jsonData, ParameterType.RequestBody);
                        IRestResponse response = await client.ExecuteAsync(request);
                        string jsonString = "";
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = response.Content;
                            TextFileLogHelper.TraceLog("GeneratePos Response Json" + jsonString);
                            NotificationResponse notificationresponse = JsonConvert.DeserializeObject<NotificationResponse>(jsonString);
                            razorPayPosNotificationRequest.ResponseJson = jsonString;
                            razorPayPosNotificationRequest.P2PRequestId = notificationresponse.p2pRequestId;
                            razorPayPosNotificationRequest.ResponseMessageCode = notificationresponse.messageCode;
                            razorPayPosNotificationRequest.ResponseMessage = notificationresponse.message;

                            if (notificationresponse.errorCode != null)
                            {
                                var errors = ErrorCodes.GetErrorCodes().FirstOrDefault(x => x.ErrorCode == notificationresponse.errorCode);

                                res.Message = errors != null ? errors.ErrorDescription : "Something Went Wrong";
                                res.Status = false;
                                razorPayPosNotificationRequest.IsSuccess = false;
                                razorPayPosNotificationRequest.ErrorCode = errors?.ErrorCode;
                                razorPayPosNotificationRequest.ErrorMessage = errors?.ErrorDescription;


                            }
                            else
                            {
                                razorPayPosNotificationRequest.IsSuccess = true;

                                res.Message = "Payment Initiated";
                                res.Status = true;
                            }

                        }
                        else
                        {
                            razorPayPosNotificationRequest.IsSuccess = false;
                            razorPayPosNotificationRequest.ResponseJson = response.ErrorException.ToString();
                            res.Message = "Something Went Wrong !!";
                            res.Status = false;
                        }
                    }

                    razorPayPosNotificationRequest.ModifiedDate = DateTime.Now;
                    razorPayPosNotificationRequest.ModifiedBy = userid;

                    context.Entry(razorPayPosNotificationRequest).State = System.Data.Entity.EntityState.Modified;
                    context.Commit();
                }
                else
                {
                    res.Message = "POS is not configured !!";
                    res.Status = false;
                }
            }
            return res;
        }

        [HttpPost]
        [Route("CheckStatus")]
        public async Task<RazorPayPosStatusResponse> CheckStatus(int OrderId)
        {
            RazorPayPosStatusResponse res = new RazorPayPosStatusResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                var pos = context.RazorPayPosNotificationRequests.Where(x => x.OrderId == OrderId && x.IsSuccess == true).ToList();
                var razorpaycredetntial = context.RazorPayPosCredentials.FirstOrDefault();
                if (pos != null && pos.Any())
                {
                    if (pos.Any(x => x.RequestType == "Status" && x.IsSuccess == true))
                    {
                        res.Status = true;
                        res.Message = "Payment Received";
                        res.TransactionId = pos.FirstOrDefault(x => x.RequestType == "Status" && x.IsSuccess == true).TransactionId;
                        res.PaymentMode = "RazorPay Pos";
                        return res;
                    }
                    if (pos.Any(x => x.RequestType == "Cancel" && x.IsSuccess == true))
                    {
                        res.Status = false;
                        res.Message = "Payment Cancelled";
                        res.TransactionId = pos.FirstOrDefault(x => x.RequestType == "Status" && x.IsSuccess == true).TransactionId;
                        res.PaymentMode = "RazorPay Pos";
                        return res;
                    }
                    if (pos.Any(x => x.RequestType == "Notification" && x.IsSuccess == true))
                    {
                        if (razorpaycredetntial != null && !string.IsNullOrEmpty(razorpaycredetntial.NotificationStatusUrl))
                        {
                            var NotificationData = pos.FirstOrDefault(x => x.RequestType == "Notification" && x.IsSuccess == true);
                            //DateTime newdate = NotificationData.CreatedDate.AddSeconds(150);
                            //if(newdate >= DateTime.Now)
                            //{
                            //}
                            Model.RazorPay.RazorPayPosNotificationRequest razorpaypos = new Model.RazorPay.RazorPayPosNotificationRequest
                            {
                                OrderId = OrderId,
                                appKey = NotificationData.appKey,
                                RequestType = "Status",
                                username = NotificationData.username,
                                amount = NotificationData.amount,
                                customerMobileNumber = NotificationData.customerMobileNumber,
                                externalRefNumber = NotificationData.externalRefNumber,
                                externalRefNumber2 = NotificationData.externalRefNumber2,
                                externalRefNumber3 = NotificationData.externalRefNumber3,
                                accountLabel = NotificationData.accountLabel,
                                customerEmail = NotificationData.customerEmail,
                                pushTo = NotificationData.pushTo,
                                mode = NotificationData.mode,
                                IsSuccess = false,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now
                            };
                            context.RazorPayPosNotificationRequests.Add(razorpaypos);
                            context.Commit();
                            CheckPosRequest checkPosRequest = new CheckPosRequest
                            {
                                username = NotificationData.username,
                                appKey = NotificationData.appKey,
                                origP2pRequestId = NotificationData.P2PRequestId
                            };
                            string jsonData = JsonConvert.SerializeObject(checkPosRequest);
                            TextFileLogHelper.TraceLog("Check Status Request Json" + jsonData);
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                            var client = new RestSharp.RestClient(razorpaycredetntial.NotificationStatusUrl);
                            var request = new RestRequest(Method.POST);
                            request.AddHeader("Content-Type", "application/json");
                            request.AddParameter("application/json", jsonData, ParameterType.RequestBody);
                            IRestResponse response = await client.ExecuteAsync(request);
                            string jsonString = "";
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = response.Content;
                                TextFileLogHelper.TraceLog("Check Status Response Json" + jsonString);
                                razorpaypos.ResponseJson = jsonString;
                                NotificationStatusResponse notificationresponse = JsonConvert.DeserializeObject<NotificationStatusResponse>(jsonString);
                                if (notificationresponse.status == "AUTHORIZED")
                                {
                                    var errors = ErrorCodes.GetErrorCodes().FirstOrDefault(x => x.ErrorCode == notificationresponse.messageCode);
                                    razorpaypos.TransactionId = notificationresponse.txnId;
                                    razorpaypos.P2PRequestId = notificationresponse.P2PRequestId;
                                    razorpaypos.ResponseMessageCode = !string.IsNullOrEmpty(notificationresponse.messageCode) ? notificationresponse.messageCode : "";
                                    razorpaypos.ResponseMessage = errors != null ? errors.ErrorDescription : "Something Went Wrong";
                                    res.PaymentMode = "RazorPay Pos";
                                    res.Status = true;
                                    res.TransactionId = razorpaypos.TransactionId;
                                    res.Message = "Payment Success";
                                }
                                else
                                {
                                    var errors = ErrorCodes.GetErrorCodes().FirstOrDefault(x => x.ErrorCode == notificationresponse.messageCode);
                                    razorpaypos.TransactionId = notificationresponse.txnId;
                                    razorpaypos.P2PRequestId = notificationresponse.P2PRequestId;
                                    razorpaypos.ErrorCode = !string.IsNullOrEmpty(notificationresponse.messageCode) ? notificationresponse.messageCode : "";
                                    razorpaypos.ErrorMessage = errors != null ? errors.ErrorDescription : "Something Went Wrong";
                                    res.PaymentMode = "RazorPay Pos";
                                    res.Status = false;
                                    res.Message = razorpaypos.ErrorMessage;
                                }
                            }
                            else
                            {
                                razorpaypos.IsSuccess = false;
                                razorpaypos.ResponseJson = response.ErrorException.ToString();
                                res.Message = "Something Went Wrong !!";
                                res.Status = false;
                            }
                            razorpaypos.ModifiedBy = userid;
                            razorpaypos.ModifiedDate = DateTime.Now;
                            context.Entry(razorpaypos).State = System.Data.Entity.EntityState.Modified;
                            context.Commit();

                        }
                        else
                        {
                            res.Status = false;
                            res.Message = "NotificationStatusUrl not Configured";
                        }
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "Notification Not Found";
                    }

                }
                else
                {
                    res.Status = false;
                    res.Message = "Order Not Found";
                }
                
            }
            return res;
        }

        [HttpPost]
        [Route("CancelNotification")]
        public async Task<RazorPayPosStatusResponse> CancelNotification(int OrderId)
        {
            RazorPayPosStatusResponse res = new RazorPayPosStatusResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                var pos = context.RazorPayPosNotificationRequests.Where(x => x.OrderId == OrderId && x.IsSuccess == true).ToList();
                var razorpaycredetntial = context.RazorPayPosCredentials.FirstOrDefault();
                if (pos != null && pos.Any())
                {
                    if (pos.Any(x => x.RequestType == "Status" && x.IsSuccess == true))
                    {
                        res.Status = true;
                        res.Message = "Payment Received";
                        res.TransactionId = pos.FirstOrDefault(x => x.RequestType == "Status" && x.IsSuccess == true).TransactionId;
                        res.PaymentMode = "RazorPay Pos";
                        return res;
                    }
                    if (pos.Any(x => x.RequestType == "Cancel" && x.IsSuccess == true))
                    {
                        res.Status = false;
                        res.Message = "Payment Cancelled";
                        res.TransactionId = pos.FirstOrDefault(x => x.RequestType == "Status" && x.IsSuccess == true).TransactionId;
                        res.PaymentMode = "RazorPay Pos";
                        return res;
                    }
                    if (pos.Any(x => x.RequestType == "Notification" && x.IsSuccess == true))
                    {
                        if (razorpaycredetntial != null && !string.IsNullOrEmpty(razorpaycredetntial.CancelNotificationApiUrl))
                        {
                            var NotificationData = pos.FirstOrDefault(x => x.RequestType == "Notification" && x.IsSuccess == true);
                            Model.RazorPay.RazorPayPosNotificationRequest razorpaypos = new Model.RazorPay.RazorPayPosNotificationRequest
                            {
                                OrderId = OrderId,
                                appKey = NotificationData.appKey,
                                RequestType = "Cancel",
                                username = NotificationData.username,
                                amount = NotificationData.amount,
                                customerMobileNumber = NotificationData.customerMobileNumber,
                                externalRefNumber = NotificationData.externalRefNumber,
                                externalRefNumber2 = NotificationData.externalRefNumber2,
                                externalRefNumber3 = NotificationData.externalRefNumber3,
                                accountLabel = NotificationData.accountLabel,
                                customerEmail = NotificationData.customerEmail,
                                pushTo = NotificationData.pushTo,
                                mode = NotificationData.mode,
                                IsSuccess = false,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now
                            };
                            context.RazorPayPosNotificationRequests.Add(razorpaypos);
                            context.Commit();
                            CancelNoticationRequest cancelNoticationRequest = new CancelNoticationRequest
                            {
                                username = NotificationData.username,
                                appKey = NotificationData.appKey,
                                origP2pRequestId = NotificationData.P2PRequestId,
                                pushTo = new Pushto
                                {
                                    deviceId = NotificationData.pushTo
                                }
                            };
                            string jsonData = JsonConvert.SerializeObject(cancelNoticationRequest);
                            TextFileLogHelper.TraceLog("Cancel Notification Request Json" + jsonData);
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                            var client = new RestSharp.RestClient(razorpaycredetntial.CancelNotificationApiUrl);
                            var request = new RestRequest(Method.POST);
                            request.AddHeader("Content-Type", "application/json");
                            request.AddParameter("application/json", jsonData, ParameterType.RequestBody);
                            IRestResponse response = await client.ExecuteAsync(request);
                            string jsonString = "";
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                jsonString = response.Content;
                                TextFileLogHelper.TraceLog("Cancel Notification Response Json" + jsonString);
                                NotificationResponse notificationresponse = JsonConvert.DeserializeObject<NotificationResponse>(jsonString);
                                razorpaypos.ResponseJson = jsonString;
                                razorpaypos.P2PRequestId = notificationresponse.p2pRequestId;
                                razorpaypos.ResponseMessageCode = notificationresponse.messageCode;
                                razorpaypos.ResponseMessage = notificationresponse.message;
                                if (notificationresponse.errorCode != null)
                                {
                                    var errors = ErrorCodes.GetErrorCodes().FirstOrDefault(x => x.ErrorCode == notificationresponse.errorCode);
                                    res.Message = errors != null ? errors.ErrorDescription : "Something Went Wrong";
                                    res.Status = false;
                                    razorpaypos.IsSuccess = false;
                                    razorpaypos.ErrorCode = errors?.ErrorCode;
                                    razorpaypos.ErrorMessage = errors?.ErrorDescription;
                                }
                                else
                                {
                                    razorpaypos.IsSuccess = true;
                                    res.Message = "Payment Cancelled";
                                    res.Status = true;
                                }
                            }
                            else
                            {
                                razorpaypos.IsSuccess = false;
                                razorpaypos.ResponseJson = response.ErrorException.ToString();
                                res.Message = "Something Went Wrong !!";
                                res.Status = false;
                            }
                            razorpaypos.ModifiedBy = userid;
                            razorpaypos.ModifiedDate = DateTime.Now;
                            context.Entry(razorpaypos).State = System.Data.Entity.EntityState.Modified;
                            context.Commit();
                        }
                        else
                        {
                            res.Status = false;
                            res.Message = "CancelNotificationApiUrl not Configured";
                        }
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "Notification Not Found";
                    }

                }
                else
                {
                    res.Status = false;
                    res.Message = "Order Not Found";
                }

            }
            return res;
        }

    }
}
