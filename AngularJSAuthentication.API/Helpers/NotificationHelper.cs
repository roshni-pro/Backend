using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.AutoNotification;
using GenricEcommers.Models;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Helpers
{
    public class NotificationHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<bool> SendNotificationtoCustomer(Customer cust, string notificationMessage, string smsMessage, bool sendSms, bool sendFcmNotification, string title = "", string imagePath = "")
        {
            bool Result = false;
            if (sendFcmNotification && cust.fcmId != null)
            {

                //string regId;
                //regId = cust.Name;
                //string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                //string id = ConfigurationManager.AppSettings["FcmApiId"];

                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";

                //var objNotification = new
                //{
                //    data = new
                //    {
                //        title = title,
                //        body = notificationMessage,
                //        icon = imagePath
                //    },
                //    to = cust.fcmId
                //};
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                //tRequest.Headers.Add(string.Format("Sender: id={0}", id));
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

                //                NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationController.FCMResponse>(responseFromFirebaseServer);
                //                using (var context = new AuthContext())
                //                {
                //                    if (response.success == 1)
                //                    {
                //                        Console.Write(response);
                //                        try
                //                        {
                //                            DeviceNotification obj = new DeviceNotification();
                //                            obj.CustomerId = cust.CustomerId;
                //                            obj.DeviceId = cust.fcmId;
                //                            obj.title = title;
                //                            obj.Message = notificationMessage;
                //                            obj.ImageUrl = imagePath;
                //                            obj.NotificationTime = DateTime.Now;
                //                            context.DeviceNotificationDb.Add(obj);
                //                            int Id = context.Commit();
                //                            // return true;
                //                        }
                //                        catch (Exception ex)
                //                        {
                //                            logger.Error("Error in Add message " + ex.Message);
                //                        }
                //                    }
                //                }

                //            }
                //        }
                //    }
                //}

                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                var data = new FCMData
                {
                    title = title,
                    body = notificationMessage,
                    icon = imagePath
                };
                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                var result = await firebaseService.SendNotificationForApprovalAsync(cust.fcmId, data);
                if (result != null)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
                }
            }

            if (sendSms)
            {
                // string CountryCode = "91";
                // string Sender = "SHOPKR";
                // string authkey = Startup.smsauthKey; //"100498AhbWDYbtJT56af33e3";



                // int route = 4;

                //string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + cust.Mobile + "&message=" + smsMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

                // var webRequest = (HttpWebRequest)WebRequest.Create(path);
                // webRequest.Method = "GET";
                // webRequest.ContentType = "application/json";
                // webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                // webRequest.ContentLength = 0; // added per comment 
                // webRequest.Credentials = CredentialCache.DefaultCredentials;
                // webRequest.Accept = "*/*";
                // var webResponse = (HttpWebResponse)webRequest.GetResponse();
                // if (webResponse.StatusCode != HttpStatusCode.OK)
                //     logger.Error("{0}", webResponse.Headers);
                bool result = Common.Helpers.SendSMSHelper.SendSMS(cust.Mobile, smsMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP,"").ToString(),"");

            }

            return Result;
        }


        public async Task<bool> SendNotificationtoSupplier(int SupplierId, string notificationMessage, string smsMessage, bool sendSms, bool sendFcmNotification, string title = "")
        {
            bool Result = false;
            using (var context = new AuthContext())
            {
                var suppdetails = context.Suppliers.Where(x => x.SupplierId == SupplierId).Select(x => new { x.MobileNo, x.Name, x.fcmId }).FirstOrDefault();
                if (sendFcmNotification && suppdetails.fcmId != null)
                {

                    string regId;
                    regId = suppdetails.Name;

                    //string id = ConfigurationManager.AppSettings["FcmApiId"];

                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                    //tRequest.Method = "post";

                    //var objNotification = new
                    //{
                    //    data = new
                    //    {
                    //        title = title,
                    //        body = notificationMessage

                    //    },
                    //    to = suppdetails.fcmId
                    //};
                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                    //tRequest.Headers.Add(string.Format("Sender: id={0}", id));
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
                    //                NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationController.FCMResponse>(responseFromFirebaseServer);
                    //                if (response.success == 1)
                    //                {

                    //                    MongoDbHelper<SupplierNotificationDetails> suppnotification = new MongoDbHelper<SupplierNotificationDetails>();
                    //                    SupplierNotificationDetails suppdetailsnoti = new SupplierNotificationDetails();
                    //                    suppdetailsnoti.SupplierId = SupplierId;
                    //                    suppdetailsnoti.DeviceId = suppdetails.fcmId;
                    //                    suppdetailsnoti.title = title;
                    //                    suppdetailsnoti.Message = notificationMessage;
                    //                    suppdetailsnoti.NotificationTime = DateTime.Now;
                    //                    suppnotification.Insert(suppdetailsnoti);

                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                    var data = new FCMData
                    {
                        title = title,
                        body = notificationMessage
                    };
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    var result = await firebaseService.SendNotificationForApprovalAsync(suppdetails.fcmId, data);
                    if (result != null)
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                }

                if (sendSms)
                {
                    //string CountryCode = "91";
                    //string Sender = "SHOPKR";
                    //string authkey = Startup.smsauthKey; //"100498AhbWDYbtJT56af33e3";
                    //int route = 4;
                    //string username = ConfigurationManager.AppSettings["NewriseOTPUsername"].ToString();
                    //string passwrod = ConfigurationManager.AppSettings["NewriseOTPPasswrod"].ToString();
                    //string path = "http://www.smsjust.com/blank/sms/user/urlsms.php?username=" + username + "&pass=" + passwrod + "&senderid=" + Sender + "&dest_mobileno=" + suppdetails.MobileNo + "&message=" + smsMessage + " &response=Y";


                    //// string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + suppdetails.MobileNo + "&message=" + smsMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

                    //var webRequest = (HttpWebRequest)WebRequest.Create(path);
                    //webRequest.Method = "GET";
                    //webRequest.ContentType = "application/json";
                    //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                    //webRequest.ContentLength = 0; // added per comment 
                    //webRequest.Credentials = CredentialCache.DefaultCredentials;
                    //webRequest.Accept = "*/*";
                    //var webResponse = (HttpWebResponse)webRequest.GetResponse();
                    //if (webResponse.StatusCode != HttpStatusCode.OK)
                    //    logger.Error("{0}", webResponse.Headers);
                    Common.Helpers.SendSMSHelper.SendSMS(suppdetails.MobileNo, smsMessage, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(),"");

                }
            }
            return Result;
        }
    }
}