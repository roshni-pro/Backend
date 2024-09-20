using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.Model;
using System.Data.Entity;
using System.Collections.Concurrent;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.API.Helpers;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Configuration;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.API.Helper.Notification;


namespace AngularJSAuthentication.API.Controllers.External.SalesManApp
{
    [RoutePrefix("api/SalesAppLogin")]
    public class SalesAppLoginController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public static Logger logger = LogManager.GetCurrentClassLogger();

        #region Login via credentials
        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Saleslogin(string mob, string password, string FcmId, string DeviceId, string CurrentAPKversion, string PhoneOSversion, string UserDeviceName, string IMEI, string customerTracking = null)
        {

            using (var db = new AuthContext())
            {
                Peopleresponse res;
                People People = new People();
                People = db.Peoples.Where(x => x.Mobile == mob && x.Active == true && x.Deleted == false).FirstOrDefault();
                #region TODO:SalesAppMarch2023
                var WarehouseDetail = db.Warehouses.Where(x => x.WarehouseId == People.WarehouseId && x.active == true && x.Deleted == false).FirstOrDefault();
                People.WarehouseName = WarehouseDetail != null ? WarehouseDetail.WarehouseName : string.Empty;
                var ClusterIds = db.ClusterStoreExecutives.Where(x => x.ExecutiveId == People.PeopleID && x.IsActive == true && x.IsDeleted == false).Select(x => x.ClusterId).Distinct().ToList();
                if (ClusterIds.Count > 0)
                {
                    var ClusterNames = db.Clusters.Where(x => ClusterIds.Contains(x.ClusterId)).Select(x => x.ClusterName).Distinct().ToList();
                    People.clusterId = string.Join(",", ClusterIds);
                    People.clusterName = string.Join(",", ClusterNames);
                }

                var StoreIds = db.ClusterStoreExecutives.Where(x => x.ExecutiveId == People.PeopleID && x.IsActive == true && x.IsDeleted == false).Select(x => x.StoreId).Distinct().ToList();
                if (StoreIds.Count > 0)
                {
                    var StoreNames = db.StoreDB.Where(x => StoreIds.Contains(x.Id)).Select(x => x.Name).Distinct().ToList();
                    People.StoreId = string.Join(",", StoreIds);
                    People.StoreName = string.Join(",", StoreNames);
                }
                #endregion
                if (People != null)
                {
                    if (People.Password == password)
                    {
                        var registeredApk = db.GetAPKUserAndPwd("SalesApp");
                        People.RegisteredApk = registeredApk;
                        if (string.IsNullOrEmpty(customerTracking))
                        {
                            List<string> FCMIds = new List<string>();
                            if (People.FcmId != FcmId)
                                FCMIds.Add(People.FcmId);


                            string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + People.PeopleID + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                            var role = db.Database.SqlQuery<string>(query).ToList();
                            var IsRole = role.Any(x => x.Contains("Hub sales lead"));
                            if (IsRole)
                            {
                                People.Role = "Hub sales lead";
                            }
                            else if (role.Any(x => x.Contains("Digital sales executive")))
                            {
                                People.Role = "Digital sales executive";
                            }
                            else if (role.Any(x => x.Contains("Telecaller")))
                            {
                                People.Role = "Telecaller";
                            }
                            else
                            {
                                People.Role = "";
                            }
                            People.FcmId = FcmId;
                            People.DeviceId = DeviceId;
                            People.CurrentAPKversion = CurrentAPKversion;   //tejas for device info 
                            People.PhoneOSversion = PhoneOSversion;
                            People.UserDeviceName = UserDeviceName;
                            People.IMEI = IMEI;
                            People.UpdatedDate = DateTime.Now;
                            db.Entry(People).State = EntityState.Modified;
                            #region Device History
                            PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                            phonerecord.PeopleID = People.PeopleID;
                            phonerecord.PeopleFirstName = People.PeopleFirstName;
                            phonerecord.Department = People.Department;
                            phonerecord.Mobile = People.Mobile;
                            phonerecord.CurrentAPKversion = People.CurrentAPKversion;
                            phonerecord.IMEI = People.IMEI;
                            phonerecord.PhoneOSversion = People.PhoneOSversion;
                            phonerecord.UserDeviceName = People.UserDeviceName;
                            phonerecord.UpdatedDate = DateTime.Now;
                            db.PhoneRecordHistoryDB.Add(phonerecord);
                            int id = db.Commit();
                            #endregion


                            if (FCMIds.Any())
                            {
                                string Key = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                                //var objNotificationList = FCMIds.Distinct().Select(x => new
                                //{
                                //    to = x,
                                //    PeopleId = People.PeopleID,
                                //    data = new
                                //    {
                                //        title = "",
                                //        body = "",
                                //        icon = "",
                                //        typeId = "",
                                //        notificationCategory = "",
                                //        notificationType = "",
                                //        notificationId = "",
                                //        notify_type = "logout",
                                //        url = "",
                                //    }
                                //}).ToList();
                                var data = new FCMData
                                {
                                    title = "",
                                    body = "",
                                    icon = "",
                                    notificationCategory = "",
                                    notificationType = "",
                                    notify_type = "logout",
                                    url = "",
                                };
                                ConcurrentBag<ManualAutoNotification> AutoNotifications = new ConcurrentBag<ManualAutoNotification>();
                                MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();
                                ParallelLoopResult parellelResult = Parallel.ForEach(FCMIds.Distinct(), (x) =>
                                {
                                    var AutoNotification = new ManualAutoNotification
                                    {
                                        CreatedDate = DateTime.Now,
                                        FcmKey = Key,
                                        IsActive = true,
                                        IsSent = false,
                                        NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                                        ObjectId = People.PeopleID,
                                        ObjectType = "People"
                                    };
                                    try
                                    {
                                        var firebaseService = new FirebaseNotificationServiceHelper(Key);
                                        //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                                        var result = firebaseService.SendNotificationForApprovalAsync(x, data);
                                        if (result != null)
                                        {
                                            AutoNotification.IsSent = true;
                                        }
                                        else
                                        {
                                            AutoNotification.IsSent = false;
                                        }
                                        //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                        //tRequest.Method = "post";
                                        //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
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
                                        //                AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse>(responseFromFirebaseServer);
                                        //                if (response.success == 1)
                                        //                {
                                        //                    AutoNotification.IsSent = true;
                                        //                }
                                        //                else if (response.failure == 1)
                                        //                {
                                        //                    AutoNotification.IsSent = false;
                                        //                }
                                        //            }
                                        //        }
                                        //    }
                                        //}
                                    }
                                    catch (Exception asd)
                                    {
                                        AutoNotification.IsSent = false;
                                        logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(data.notify_type).Append(asd.ToString()).ToString());
                                    }
                                });
                                if (parellelResult.IsCompleted && AutoNotifications != null && AutoNotifications.Any())
                                {
                                    var autoNot = AutoNotifications.ToList();
                                    AutoNotificationmongoDbHelper.InsertMany(autoNot);
                                }
                            }
                        }
                        res = new Peopleresponse
                        {
                            people = People,
                            Status = true,
                            message = "Success."

                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new Peopleresponse
                        {
                            people = null,
                            Status = false,
                            message = "Wrong Password."

                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    res = new Peopleresponse
                    {
                        people = null,
                        Status = false,
                        message = "Record not found."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region login ViaOtp
        [Route("ViaOtp")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage SalesAppLoginByotpV2(People customer)
        {
            using (AuthContext db = new AuthContext())
            {

                SalesDTO res;
                string error = "";
                People People = new People();
                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Mobile='" + customer.Mobile + "' and (r.Name like '%sales executive%' or r.Name like '%sales lead%' or  r.Name like '%Digital sales executive%' or  r.Name like '%Telecaller%') and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                People = db.Database.SqlQuery<People>(query).FirstOrDefault();
                //People = db.Peoples.Where(x => x.Mobile == customer.Mobile && x.Department == "Sales Executive" && x.Deleted == false && x.Active == true).FirstOrDefault();
                query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + People.PeopleID + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                var role = db.Database.SqlQuery<string>(query).ToList();
                var IsRole = role.Any(x => x.Contains("Hub sales lead"));
                if (IsRole)
                {
                    People.Role = "Hub sales lead";
                }
                else if (role.Any(x => x.Contains("Digital sales executive")))
                {
                    People.Role = "Digital sales executive";
                }
                else if (role.Any(x => x.Contains("Telecaller")))
                {
                    People.Role = "Telecaller";
                }
                else
                {
                    People.Role = "";
                }
                try
                {
                    if (People == null)
                    {
                        res = new SalesDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Not a Registered Sales Executive"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        using (var context = new AuthContext())
                            try
                            {
                                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                                string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                                // string OtpMessage = " is Your login Code. :). ShopKirana";
                                string OtpMessage = ""; //"{#var1#} is Your login Code. {#var2#}. ShopKirana";
                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.SalesApp, "Login_Code");
                                OtpMessage = dltSMS == null ? "" : dltSMS.Template;                              
                                OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
                                OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

                                //string CountryCode = "91";
                                //string Sender = "SHOPKR";
                                //string authkey = Startup.smsauthKey; //"100498AhbWDYbtJT56af33e3";
                                //int route = 4;
                                //string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + People.Mobile + "&message=" + sRandomOTP + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

                                ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";

                                //var webRequest = (HttpWebRequest)WebRequest.Create(path);
                                //webRequest.Method = "GET";
                                //webRequest.ContentType = "application/json";
                                //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                                //webRequest.ContentLength = 0; // added per comment 
                                //webRequest.Credentials = CredentialCache.DefaultCredentials;
                                //webRequest.Accept = "*/*";
                                //var webResponse = (HttpWebResponse)webRequest.GetResponse();
                                bool result = dltSMS == null ? false : Common.Helpers.SendSMSHelper.SendSMS(People.Mobile, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                                if (!result)
                                {
                                    logger.Info("OTP Genrated: " + sRandomOTP);
                                }
                                else
                                {
                                    logger.Info("OTP Genrated: " + sRandomOTP);

                                    var check = context.CheckPeopleSalesPersonData(customer.Mobile);
                                    check.OTP = sRandomOTP;
                                    if (check != null)
                                    {
                                        res = new SalesDTO
                                        {

                                            P = new People { OTP = sRandomOTP },
                                            Status = true,
                                            Message = "Success."

                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                    else
                                    {
                                        res = new SalesDTO
                                        {
                                            P = null,
                                            Status = false,
                                            Message = "Not Success"

                                        };
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            catch (Exception sdf)
                            {
                                res = new SalesDTO
                                {
                                    P = null,
                                    Status = false,
                                    Message = "Not Success"

                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                    }
                }
                catch (Exception es)
                {
                    error = error + es;
                }
                res = new SalesDTO()
                {
                    P = null,
                    Status = false,
                    Message = ("This is something went wrong Sales Executive : " + error)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        #endregion
        #region Generate Random OTP
        /// <summary>
        /// Created by 29/04/2019 
        /// Create rendom otp//By Sudhir
        /// </summary>
        /// <param name="iOTPLength"></param>
        /// <param name="saAllowedCharacters"></param>
        /// <returns></returns>
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }
        #endregion

        [Route("GetLogedSalesPerson")]
        [AcceptVerbs("GET")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLogedSalesPerson(string MobileNumber, bool IsOTPverified, string fcmid, string CurrentAPKversion, string PhoneOSversion, string DeviceId, string UserDeviceName, string IMEI = "")
        {
            SalesDTO res;
            using (var db = new AuthContext())
            {
                try
                {
                    if (IsOTPverified == true)
                    {
                        People People = new People();
                        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Mobile='" + MobileNumber + "' and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        People = db.Database.SqlQuery<People>(query).FirstOrDefault();
                        //People = db.Peoples.Where(x => x.Mobile == MobileNumber && x.Department == "Sales Executive" && x.Deleted == false && x.Active == true).FirstOrDefault();
                        if (People != null)
                        {
                            List<string> FCMIds = new List<string>();
                            if (People.FcmId != fcmid)
                                FCMIds.Add(People.FcmId);

                            People.FcmId = fcmid;
                            People.DeviceId = DeviceId;
                            People.CurrentAPKversion = CurrentAPKversion;   //tejas for device info 
                            People.PhoneOSversion = PhoneOSversion;
                            People.UserDeviceName = UserDeviceName;
                            People.IMEI = IMEI;
                            People.UpdatedDate = DateTime.Now;
                            //db.Peoples.Attach(People);
                            db.Entry(People).State = EntityState.Modified;
                            db.Commit();
                            #region Device History
                            var Customerhistory = db.Peoples.Where(x => x.Mobile == People.Mobile).FirstOrDefault();
                            try
                            {
                                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                if (Customerhistory != null)
                                {
                                    phonerecord.PeopleID = Customerhistory.PeopleID;
                                    phonerecord.PeopleFirstName = Customerhistory.PeopleFirstName;
                                    phonerecord.Department = Customerhistory.Department;
                                    phonerecord.Mobile = Customerhistory.Mobile;
                                    phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                    phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                    phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                    phonerecord.IMEI = Customerhistory.IMEI;
                                    phonerecord.UpdatedDate = DateTime.Now;
                                    db.PhoneRecordHistoryDB.Add(phonerecord);
                                    int id = db.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                            #endregion

                            var registeredApk = db.GetAPKUserAndPwd("SalesApp");
                            People.RegisteredApk = registeredApk;
                            string queryrole = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + People.PeopleID + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                            var role = db.Database.SqlQuery<string>(queryrole).ToList();

                            var IsRole = role.Any(x => x.Contains("Hub sales lead"));
                            if (IsRole)
                            {
                                People.Role = "Hub sales lead";
                            }
                            else
                            {

                                People.Role = "";

                            }

                            if (FCMIds.Any())
                            {
                                string Key = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                                //var objNotificationList = FCMIds.Distinct().Select(x => new
                                //{
                                //    to = x,
                                //    PeopleId = People.PeopleID,
                                //    data = new
                                //    {
                                //        title = "",
                                //        body = "",
                                //        icon = "",
                                //        typeId = "",
                                //        notificationCategory = "",
                                //        notificationType = "",
                                //        notificationId = "",
                                //        notify_type = "logout",
                                //        url = "",
                                //    }
                                //}).ToList();
                                var data = new FCMData
                                {
                                    title = "",
                                    body = "",
                                    icon = "",
                                    notificationCategory = "",
                                    notificationType = "",
                                    notify_type = "logout",
                                    url = "",
                                };
                                ConcurrentBag<ManualAutoNotification> AutoNotifications = new ConcurrentBag<ManualAutoNotification>();
                                MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();
                                ParallelLoopResult parellelResult = Parallel.ForEach(FCMIds.Distinct(), (x) =>
                                {
                                    var AutoNotification = new ManualAutoNotification
                                    {
                                        CreatedDate = DateTime.Now,
                                        FcmKey = Key,
                                        IsActive = true,
                                        IsSent = false,
                                        NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                                        ObjectId = People.PeopleID,
                                        ObjectType = "People"
                                    };
                                    try
                                    {
                                        var firebaseService = new FirebaseNotificationServiceHelper(Key);
                                        //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                                        var result = firebaseService.SendNotificationForApprovalAsync(x, data);
                                        if (result != null)
                                        {
                                            AutoNotification.IsSent = true;
                                        }
                                        else
                                        {
                                            AutoNotification.IsSent = false;
                                        }
                                        //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                        //tRequest.Method = "post";
                                        //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
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
                                        //                AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse>(responseFromFirebaseServer);
                                        //                if (response.success == 1)
                                        //                {
                                        //                    AutoNotification.IsSent = true;
                                        //                }
                                        //                else if (response.failure == 1)
                                        //                {
                                        //                    AutoNotification.IsSent = false;
                                        //                }
                                        //            }
                                        //        }
                                        //    }
                                        //}
                                    }
                                    catch (Exception asd)
                                    {
                                        AutoNotification.IsSent = false;
                                        logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(data.notify_type).Append(asd.ToString()).ToString());
                                    }
                                });
                                if (parellelResult.IsCompleted && AutoNotifications != null && AutoNotifications.Any())
                                {
                                    var autoNot = AutoNotifications.ToList();
                                    AutoNotificationmongoDbHelper.InsertMany(autoNot);
                                }
                            }
                            res = new SalesDTO()
                            {
                                P = People,
                                Status = true,
                                Message = "Success."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            res = new SalesDTO()
                            {
                                P = null,
                                Status = false,
                                Message = "SalesPerson does not exist or Incorect mobile number."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {

                        res = new SalesDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "OTP not verified."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    res = new SalesDTO()
                    {
                        P = null,
                        Status = false,
                        Message = "Some Error Occurred."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

    }
}
