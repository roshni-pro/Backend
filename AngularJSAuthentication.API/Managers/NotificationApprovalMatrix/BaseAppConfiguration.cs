using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.DeliveryOptimization;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static AngularJSAuthentication.API.Controllers.NotificationController;

namespace AngularJSAuthentication.API.Managers.NotificationApprovalMatrix
{
    public class BaseAppConfiguration : IAppConfiguration
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public bool SendNotification(string FcmId, string Msg, string FCMKey)
        {
            try
            {
                string Key = FCMKey;
                //FCMRequest objNotification = new FCMRequest();
                //objNotification.to = FcmId;
                //objNotification.MessageId = "";
                //objNotification.data = new FCMData
                //{
                //    title = "Warning !!",
                //    body = Msg,
                //    icon = "",
                //    typeId = 0,
                //    notificationCategory = "",
                //    notificationType = "",
                //    notificationId = 0,
                //    notify_type = "Alert",
                //    url = ""
                //};
                var data = new FCMData
                {
                    title = "Warning !!",
                    body = Msg,
                    icon = "",
                    typeId = 0,
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = "Alert",
                    url = ""
                };
                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(FcmId, data);
                if (result != null)
                {
                    return true;
                }
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ds)
            {
                logger.Error("Error during SendNotification for delivery: " + ds.ToString());
            }
            return false;
        }

        public async Task<bool> SendNotificationForApproval(string FcmId, string Msg, int OrderId, string FCMKey, string OrderStatus,string SkCode, string ShopName, double GrossAmount)
        {
            try
            {
                string Body = "OrderID: " + OrderId + "  |  " + "Order Amount: " + GrossAmount + "@" + "SKCode: " + SkCode + "  |  " + "ShopName: " + ShopName ;
                Body = Body.Replace("@", System.Environment.NewLine);


                //Notification notification = new Notification();
                //notification.title = "true";
                //notification.Message = "Order Cancel Approval Request";
                //notification.Pic = "";
                //notification.priority = "high";
                //string notify_type = "LogOut";

                var data = new FCMData
                {
                    title = "Order Cancel Approval Request",
                    body = Body,
                    icon = "",
                    typeId = 0,
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = "DCApproval",
                    OrderId = OrderId,
                    url = "", //OrderId, PeopleId
                    OrderStatus = OrderStatus
                };
                //string Key = ConfigurationManager.AppSettings["SarthiFcmApiKey"];
                var firebaseService = new FirebaseNotificationServiceHelper(FCMKey);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = await firebaseService.SendNotificationForApprovalAsync(FcmId, data);
                if (result != null)
                {
                    return true;
                }
                //string Key = FCMKey;
                //FCMRequest objNotification = new FCMRequest();
                //objNotification.to = FcmId;
                //objNotification.MessageId = "";
                //objNotification.data = new FCMData
                //{
                //    title = "Order Cancel Approval Request",
                //    body = Body,
                //    icon = "",
                //    typeId = 0,
                //    notificationCategory = "",
                //    notificationType = "",
                //    notificationId = 0,
                //    notify_type = "DCApproval",
                //    OrderId = OrderId,
                //    url = "", //OrderId, PeopleId
                //    OrderStatus = OrderStatus
                //};
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ds)
            {
                logger.Error("Error during SendNotification for delivery: " + ds.ToString());
            }
            return false;
        }

        public bool SendNotificationForApprovalDeliveyApp(string FcmId, string Msg, int OrderId, string FCMKey, string OrderStatus, int status, string notify_type)
        {
            try
            {
                //string Key = FCMKey;
                //FCMRequest objNotification = new FCMRequest();
                //objNotification.to = FcmId;
                //objNotification.MessageId = "";
                //objNotification.data = new FCMData
                //{
                //    title = "Order Response Successfully!!",
                //    body = Msg,
                //    icon = "",
                //    typeId = 0,
                //    notificationCategory = "",
                //    notificationType = "",
                //    notificationId = 0,
                //    notify_type = notify_type,
                //    OrderId = OrderId,
                //    url = "", //OrderId, PeopleId
                //    OrderApprovestatus = status,
                //    OrderStatus = OrderStatus
                //};

                var data = new FCMData
                {
                    title = "Order Response Successfully!!",
                    body = Msg,
                    icon = "",
                    typeId = 0,
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = notify_type,
                    OrderId = OrderId,
                    url = "", //OrderId, PeopleId
                    OrderApprovestatus = status,
                    OrderStatus = OrderStatus
                };
                var firebaseService = new FirebaseNotificationServiceHelper(FCMKey);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(FcmId, data);
                if (result != null)
                {
                    return true;
                }
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ds)
            {
                logger.Error("Error during SendNotification for delivery: " + ds.ToString());
            }
            return false;
        }

        public async Task<bool> NotifyCustomer(string Title, string Message,string FcmId, int CustomerId)
        {
            bool Result = false;
            string Key = ConfigurationManager.AppSettings["FcmApiKey"];
            if (FcmId != null)
            {
                try
                {
                    //FCMRequest objNotification = new FCMRequest();
                    //objNotification.to = FcmId;
                    //objNotification.MessageId = "";
                    //objNotification.data = new FCMData
                    //{
                    //    title = Title,
                    //    body = Message,
                    //    icon = "",
                    //    typeId = 0,
                    //    notificationCategory = "",
                    //    notificationType = "",
                    //    notificationId = 0,
                    //    notify_type = "OrderShippedAlert",
                    //    url = ""
                    //};
                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                    //tRequest.Method = "post";
                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                    //tRequest.ContentLength = byteArray.Length;
                    //tRequest.ContentType = "application/json";
                    //using (Stream dataStream = tRequest.GetRequestStream())
                    //{
                    //    dataStream.Write(byteArray, 0, byteArray.Length);
                    //    using (WebResponse tResponse = tRequest.GetResponse())
                    //    {
                    //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    //        {
                    //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                    //            {
                    //                String responseFromFirebaseServer = tReader.ReadToEnd();
                    //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                    //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                    //                {
                    //                    return true;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    var data = new FCMData
                    {
                        title = Title,
                        body = Message,
                        icon = "",
                        typeId = 0,
                        notificationCategory = "",
                        notificationType = "",
                        notificationId = 0,
                        notify_type = "OrderShippedAlert",
                        url = ""
                    };
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    var result = await firebaseService.SendNotificationForApprovalAsync(FcmId, data);
                    if (result != null)
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                }
                catch (Exception ds)
                {
                    logger.Error("Error during SendNotification for ShippedAlert: " + ds.ToString());
                }
            }
            return Result;
        }

        public async Task<bool> SendNotificationForApprovalSarthiTrip_DeliveryApp(string FcmId, string Msg, long TripPlannerConfirmedMasterId, string FCMKey, string notify_type,bool trueIfApproveElseReject)
        {
            try
            {
                string Key = FCMKey;
                //FCMRequest objNotification = new FCMRequest();
                //objNotification.to = FcmId;
                //objNotification.MessageId = "";
                //objNotification.data = new FCMData
                //{
                //    title = Msg,
                //    body = Msg,
                //    icon = "",
                //    typeId = 0,
                //    notificationCategory = "",
                //    notificationType = "",
                //    notificationId = 0,
                //    notify_type = notify_type,
                //    OrderId =(int)TripPlannerConfirmedMasterId,
                //    url = "", //OrderId, PeopleId
                //    IsApproved= trueIfApproveElseReject
                //};
                var data = new FCMData
                {
                    title = Msg,
                    body = Msg,
                    icon = "",
                    typeId = 0,
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = notify_type,
                    OrderId = (int)TripPlannerConfirmedMasterId,
                    url = "", //OrderId, PeopleId
                    IsApproved = trueIfApproveElseReject
                };

                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(FcmId, data);
                if (result != null)
                {
                    return true;
                }
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ds)
            {
                logger.Error("Error during SendNotification for delivery: " + ds.ToString());
            }
            return false;
        }

        public bool CashMgSendNotificationForApproval(string FcmId, string Msg, string FCMKey, int AssignmentID)
        {
            try
            {
                //string Key = FCMKey;
                //FCMRequest objNotification = new FCMRequest();
                //objNotification.to = FcmId;
                //objNotification.MessageId = "";
                //objNotification.data = new FCMData
                //{
                //    title = "Submit OTP Successfully",
                //    body = Msg,
                //    icon = "",
                //    typeId = 0,
                //    notificationCategory = "",
                //    notificationType = "",
                //    notificationId = 0,
                //    notify_type = "CashOTPApproval",
                //    AssignmentID = AssignmentID,
                //    // OrderId = OrderId,
                //    url = "", //OrderId, PeopleId
                //              // OrderStatus = OrderStatus
                //};
                var data = new FCMData
                {
                    title = "Submit OTP Successfully",
                    body = Msg,
                    icon = "",
                    typeId = 0,
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = "CashOTPApproval",
                    AssignmentID = AssignmentID,
                    // OrderId = OrderId,
                    url = "", //OrderId, PeopleId
                              // OrderStatus = OrderStatus
                };
                var firebaseService = new FirebaseNotificationServiceHelper(FCMKey);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(FcmId, data);
                if (result != null)
                {
                    return true;
                }

                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                //                {
                //                    return true;
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ds)
            {
                logger.Error("Error during SendNotification for delivery: " + ds.ToString());
            }
            return false;
        }


    }
}