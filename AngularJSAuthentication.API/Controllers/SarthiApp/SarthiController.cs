using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.DataContracts.PeopleNotification;
using AngularJSAuthentication.API.Helper;
using System.Threading.Tasks;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.API.Helpers;
using System.Security.Claims;
using AngularJSAuthentication.API.Helper.Notification;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Sarthi")]
    public class SarthiLoginController : BaseAuthController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> SarthiLogin(People peopleobj)
        {
            ResDTO res;
            People People = new People();

            using (var context = new AuthContext())
            {
                try
                {
                    var param = new SqlParameter("Mobile", peopleobj.Mobile);
                    RolesDc Roles = new RolesDc();

                    Roles.RoleList = await context.Database.SqlQuery<RoleDc>("exec CheckRoleForSarthi @Mobile", param).ToListAsync();

                    // string query = "select distinct r.Name as Role, p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where Mobile='" + peopleobj.Mobile + "' and r.Name in('WH Service lead','Receiving Supervisor','Inventory Supervisor','Hub Cashier') and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    //  var SpLogin = context.Database.SqlQuery<SpLogin>(query).FirstOrDefault();
                    if (Roles.RoleList.Count == 0 && !Roles.RoleList.Any())
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Not authorize"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    People = await context.Peoples.Where(x => x.Mobile == peopleobj.Mobile && x.Active == true).FirstOrDefaultAsync();
                    if (People == null)
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Not a Registered user"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (People.Password != peopleobj.Password)
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Wrong Password"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (People.Password == peopleobj.Password && People.Mobile == peopleobj.Mobile)
                    {
                        if (People.DeviceId == null)
                        {
                            People.FcmId = peopleobj.FcmId;
                            People.DeviceId = peopleobj.DeviceId;
                            People.UpdatedDate = indianTime;
                            People.CurrentAPKversion = peopleobj.CurrentAPKversion; // tejas to save device info 
                            People.PhoneOSversion = peopleobj.PhoneOSversion;
                            People.IMEI = peopleobj.IMEI;// Sudhir to save device info 
                            People.UserDeviceName = peopleobj.UserDeviceName;
                            //context.Peoples.Attach(People);
                            context.Entry(People).State = EntityState.Modified;
                            context.Commit();
                        }
                        else if (People.DeviceId.Trim().ToLower() == peopleobj.DeviceId.Trim().ToLower())
                        {
                            if (People.FcmId != null && People.FcmId.Trim() != "" && People.FcmId.Trim().ToUpper() != "NULL")
                            {
                                People.FcmId = peopleobj.FcmId;
                                People.UpdatedDate = indianTime;
                                People.CurrentAPKversion = peopleobj.CurrentAPKversion; // tejas to save device info 
                                People.PhoneOSversion = peopleobj.PhoneOSversion;
                                People.IMEI = peopleobj.IMEI;// Sudhir to save device info 
                                People.UserDeviceName = peopleobj.UserDeviceName;
                                context.Entry(People).State = EntityState.Modified;
                                #region Device History
                                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                if (People != null)
                                {
                                    phonerecord.PeopleID = People.PeopleID;
                                    phonerecord.PeopleFirstName = People.PeopleFirstName;
                                    phonerecord.Department = People.Department;
                                    phonerecord.Mobile = People.Mobile;
                                    phonerecord.CurrentAPKversion = People.CurrentAPKversion;
                                    phonerecord.IMEI = People.IMEI;
                                    phonerecord.PhoneOSversion = People.PhoneOSversion;
                                    phonerecord.UserDeviceName = People.UserDeviceName;
                                    phonerecord.UpdatedDate = DateTime.Now;
                                    context.PhoneRecordHistoryDB.Add(phonerecord);
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            bool IsSend = false;
                            if (People.FcmId != null)
                            {
                                IsSend = await SendLogOutNotification(People); //send Notification Dboy for Sign out
                            }
                            else
                            {
                                IsSend = true;
                            }

                            if (IsSend)
                            {
                                context.Commit();
                                People.FcmId = peopleobj.FcmId;
                                People.DeviceId = peopleobj.DeviceId;
                                People.UpdatedDate = indianTime;
                                People.CurrentAPKversion = peopleobj.CurrentAPKversion; // tejas to save device info 
                                People.PhoneOSversion = peopleobj.PhoneOSversion;
                                People.IMEI = peopleobj.IMEI;// Sudhir to save device info 
                                People.UserDeviceName = peopleobj.UserDeviceName;
                                context.Entry(People).State = EntityState.Modified;
                                #region Device History
                                PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                                if (People != null)
                                {
                                    phonerecord.PeopleID = People.PeopleID;
                                    phonerecord.PeopleFirstName = People.PeopleFirstName;
                                    phonerecord.Department = People.Department;
                                    phonerecord.Mobile = People.Mobile;
                                    phonerecord.IMEI = People.IMEI;
                                    phonerecord.CurrentAPKversion = People.CurrentAPKversion;
                                    phonerecord.PhoneOSversion = People.PhoneOSversion;
                                    phonerecord.UserDeviceName = People.UserDeviceName;
                                    phonerecord.UpdatedDate = DateTime.Now;
                                    context.PhoneRecordHistoryDB.Add(phonerecord);
                                }
                                #endregion
                            }

                        }
                        context.Commit();
                        //TEMP obj
                        PeopleTemp PtData = new PeopleTemp();
                        PtData.PeopleID = People.PeopleID;
                        PtData.Skcode = People.Skcode;
                        PtData.CompanyId = People.CompanyId;
                        PtData.WarehouseId = People.WarehouseId;
                        PtData.PeopleFirstName = People.PeopleFirstName;
                        PtData.PeopleLastName = People.PeopleLastName;
                        PtData.Email = People.Email;
                        PtData.DisplayName = People.DisplayName;
                        PtData.Mobile = People.Mobile;
                        PtData.Password = People.Password;
                        PtData.VehicleId = People.VehicleId;
                        PtData.VehicleName = People.VehicleName;
                        PtData.VehicleNumber = People.VehicleNumber;
                        PtData.VehicleCapacity = People.VehicleCapacity;
                        PtData.CreatedDate = People.CreatedDate;
                        PtData.UpdatedDate = People.UpdatedDate;
                        PtData.DeviceId = People.DeviceId;
                        PtData.FcmId = People.FcmId;
                        PtData.ImageUrl = People.ImageUrl;

                        PtData.Roles = Roles;

                        if (PtData != null)
                        {
                            var registeredApk = context.GetAPKUserAndPwd("DeliveryApp");
                            PtData.RegisteredApk = registeredApk;
                        }

                        res = new ResDTO()
                        {
                            P = PtData,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Failed. Something went wrong"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception er)
                {
                    res = new ResDTO()
                    {
                        P = null,
                        Status = false,
                        Message = "Failed. Something went wrong"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }

        [AllowAnonymous]
        [Route("testNotify")]
        [HttpGet]
        public async Task testNotify()
        {
            using (var context = new AuthContext())
            {
                People people = null;
                var res = await SendLogOutNotification(people);
            }
        }
        internal async Task<bool> SendLogOutNotification(People people)
        {
            bool IsSend = false;
            try
            {

                //Notification notification = new Notification();
                //notification.title = "true";
                //notification.Message = " चेतावनी ! आपका सत्र समाप्त हो चुका है!";
                //notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";
                //notification.priority = "high";

                string Key = ConfigurationManager.AppSettings["SarthiFcmApiKey"];
                //string id11 = ConfigurationManager.AppSettings["DFcmApiId"];

                var data = new FCMData
                {
                    title = "true",
                    body = " चेतावनी ! आपका सत्र समाप्त हो चुका है!",
                    icon = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png",
                    typeId = 0,
                    priority = "high",
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = "LogOut",
                    // OrderId = OrderId,
                    url = "", //OrderId, PeopleId
                              // OrderStatus = OrderStatus
                };
                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(people.FcmId, data);
                if (result != null)
                {
                    IsSend = true;
                }
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";

                //// Added Bt Harry & Harshita On request chnage 17/06/2019
                //var objNotification = new
                //{
                //    to = people.FcmId,
                //    notification = new
                //    {
                //        title = notification.title,
                //        body = notification.Message,
                //    },
                //    data = new
                //    {
                //        title = notification.title,
                //        icon = notification.Pic,
                //        priority = notification.priority,
                //        notify_type = "LogOut"
                //    }
                //};

                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                ////tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
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
                //                    IsSend = true;

                //                }
                //                else if (response.failure == 1)
                //                {
                //                    IsSend = false;

                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                IsSend = true;
            }
            return IsSend;
        }


        [AllowAnonymous]
        [Route("Version")]
        [HttpGet]
        public HttpResponseMessage SarthiVersionGet(string apptype)
        {
            bool status = false;
            string resultMessage = "";
            SarthiAppVersion obj = new SarthiAppVersion();
            using (AuthContext db = new AuthContext())
            {
                if (apptype == "SarthiApp")
                {

                    obj = db.SarthiAppVersionDb.Where(x => x.isCompulsory == true).FirstOrDefault();
                    if (obj != null)
                    {
                        status = true;
                        resultMessage = "Record found";
                    }
                }
                else { status = false; resultMessage = "No version activated"; }

                var res = new
                {
                    SarthiAppVersion = obj,
                    status = status,
                    Message = resultMessage

                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        #region Generate Random OTP
        private string GenerateRandomSarthiOTP(int iOTPLength, string[] saAllowedCharacters)
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
        #region Sarthi LoginViaOtp

        [Route("LoginViaOtp")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> LoginViaOtp(People peopleobj)
        {
            ResDTO res;
            People People = new People();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("Mobile", peopleobj.Mobile);
                RolesDc Roles = new RolesDc();

                Roles.RoleList = context.Database.SqlQuery<RoleDc>("exec CheckRoleForSarthi @Mobile", param).ToList();


                //var SpLogin = context.Database.SqlQuery<SpLogin>("exec CheckRoleForSarthi @Mobile", param).FirstOrDefault();

                // string query = "select distinct r.Name as Role, p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where Mobile='" + peopleobj.Mobile + "' and r.Name in('WH Service lead','Receiving Supervisor','Inventory Supervisor','Hub Cashier') and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                // var SpLogin = context.Database.SqlQuery<SpLogin>(query).FirstOrDefault();
                if (Roles.RoleList.Count == 0 || !Roles.RoleList.Any())
                {
                    res = new ResDTO()
                    {
                        P = null,
                        Status = false,
                        Message = "Not authorize"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                People = context.Peoples.Where(x => x.Mobile == peopleobj.Mobile).FirstOrDefault();
                if (People == null)
                {
                    res = new ResDTO()
                    {
                        P = null,
                        Status = false,
                        Message = "Not a Registered user"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                if (People.DeviceId == null)
                {
                    People.FcmId = peopleobj.FcmId;
                    People.DeviceId = peopleobj.DeviceId;
                    People.UpdatedDate = indianTime;
                    People.CurrentAPKversion = peopleobj.CurrentAPKversion; // tejas to save device info 
                    People.PhoneOSversion = peopleobj.PhoneOSversion;
                    People.IMEI = peopleobj.IMEI;// Sudhir to save device info 
                    People.UserDeviceName = peopleobj.UserDeviceName;
                    context.Entry(People).State = EntityState.Modified;
                    context.Commit();

                }
                else if (People.DeviceId.Trim().ToLower() == peopleobj.DeviceId.Trim().ToLower())
                {
                    if (People.FcmId != null && People.FcmId.Trim() != "" && People.FcmId.Trim().ToUpper() != "NULL")
                    {
                        People.FcmId = peopleobj.FcmId;
                        People.UpdatedDate = indianTime;
                        People.CurrentAPKversion = peopleobj.CurrentAPKversion; // tejas to save device info 
                        People.PhoneOSversion = peopleobj.PhoneOSversion;
                        People.IMEI = peopleobj.IMEI;// Sudhir to save device info 
                        People.UserDeviceName = peopleobj.UserDeviceName;

                        context.Entry(People).State = EntityState.Modified;

                        #region Device History

                        PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                        if (People != null)
                        {
                            phonerecord.PeopleID = People.PeopleID;
                            phonerecord.PeopleFirstName = People.PeopleFirstName;
                            phonerecord.Department = People.Department;
                            phonerecord.Mobile = People.Mobile;
                            phonerecord.CurrentAPKversion = People.CurrentAPKversion;
                            phonerecord.IMEI = People.IMEI;
                            phonerecord.PhoneOSversion = People.PhoneOSversion;
                            phonerecord.UserDeviceName = People.UserDeviceName;
                            phonerecord.UpdatedDate = DateTime.Now;
                            context.PhoneRecordHistoryDB.Add(phonerecord);

                        }

                        #endregion
                    }
                }
                else
                {
                    bool IsLogout = false;
                    if (People.FcmId != null)
                    {
                        IsLogout = await SendLogOutNotification(People); //send Notification Dboy for Sign out
                    }
                    else
                    {
                        IsLogout = true;
                    }

                    if (IsLogout)
                    {
                        People.FcmId = peopleobj.FcmId;
                        People.DeviceId = peopleobj.DeviceId;
                        People.UpdatedDate = indianTime;
                        People.CurrentAPKversion = peopleobj.CurrentAPKversion; // tejas to save device info 
                        People.PhoneOSversion = peopleobj.PhoneOSversion;
                        People.IMEI = peopleobj.IMEI;// Sudhir to save device info 
                        People.UserDeviceName = peopleobj.UserDeviceName;
                        //context.Peoples.Attach(People);
                        context.Entry(People).State = EntityState.Modified;
                        #region Device History

                        PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                        if (People != null)
                        {
                            phonerecord.PeopleID = People.PeopleID;
                            phonerecord.PeopleFirstName = People.PeopleFirstName;
                            phonerecord.Department = People.Department;
                            phonerecord.Mobile = People.Mobile;
                            phonerecord.IMEI = People.IMEI;
                            phonerecord.CurrentAPKversion = People.CurrentAPKversion;
                            phonerecord.PhoneOSversion = People.PhoneOSversion;
                            phonerecord.UserDeviceName = People.UserDeviceName;
                            phonerecord.UpdatedDate = DateTime.Now;
                            context.PhoneRecordHistoryDB.Add(phonerecord);

                        }
                    }

                    #endregion
                    context.Commit();
                }

                //TEMP obj

                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string sRandomOTP = GenerateRandomSarthiOTP(4, saAllowedCharacters);
                // string OtpMessage = " is Your login Code. :). ShopKirana";
                string OtpMessage = ""; //"{#var1#} is Your login Code. {#var2#}. ShopKirana";
                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.SarthiApp, "Login_Code");
                OtpMessage = dltSMS == null ? "" : dltSMS.Template;

                OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

                //string CountryCode = "91";
                //string Sender = "SHOPKR";
                //string authkey = Startup.smsauthKey;// "100498AhbWDYbtJT56af33e3";
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
                bool result = dltSMS == null ? false : Common.Helpers.SendSMSHelper.SendSMS(People.Mobile, (sRandomOTP + " " + OtpMessage), ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                if (!result)
                {
                    logger.Info("OTP Genrated: " + sRandomOTP);
                    {
                        res = new ResDTO()
                        {
                            P = null,
                            Status = false,
                            Message = "Failed. Something went wrong"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    logger.Info("OTP Genrated: " + sRandomOTP);
                    PeopleTemp PtData = new PeopleTemp();
                    PtData.PeopleID = People.PeopleID;
                    PtData.OtpNumbers = sRandomOTP;
                    PtData.Skcode = People.Skcode;
                    PtData.CompanyId = People.CompanyId;
                    PtData.WarehouseId = People.WarehouseId;
                    PtData.PeopleFirstName = People.PeopleFirstName;
                    PtData.PeopleLastName = People.PeopleLastName;
                    PtData.Email = People.Email;
                    PtData.DisplayName = People.DisplayName;
                    PtData.Mobile = People.Mobile;
                    PtData.Password = People.Password;
                    PtData.VehicleId = People.VehicleId;
                    PtData.VehicleName = People.VehicleName;
                    PtData.VehicleNumber = People.VehicleNumber;
                    PtData.VehicleCapacity = People.VehicleCapacity;
                    PtData.CreatedDate = People.CreatedDate;
                    PtData.UpdatedDate = People.UpdatedDate;
                    PtData.DeviceId = People.DeviceId;
                    PtData.FcmId = People.FcmId;
                    PtData.ImageUrl = People.ImageUrl;
                    PtData.Roles = Roles;

                    if (PtData != null)
                    {
                        var registeredApk = context.GetAPKUserAndPwd("DeliveryApp");
                        PtData.RegisteredApk = registeredApk;
                    }

                    res = new ResDTO()
                    {
                        P = PtData,
                        Status = true,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }


            }
        }

        #endregion


        [Route("GetallNotification")]
        [HttpGet]
        public PaggingDatas GetallNotification(int skip, int take, int PeopleId)
        {
            using (var context = new AuthContext())
            {
                PaggingDatas data = new PaggingDatas();
                context.Database.CommandTimeout = 600;
                var query = "[Operation].[GetPeopleNotification] " + PeopleId.ToString() + "," + ((take - 1) * skip).ToString() + "," + take;
                var PeopleSentNotificationDc = context.Database.SqlQuery<PeopleSentNotificationDc>(query).ToList();

                data.notificationmaster = PeopleSentNotificationDc;
                data.total_count = PeopleSentNotificationDc != null && PeopleSentNotificationDc.Any() ? PeopleSentNotificationDc.FirstOrDefault().TotalCount : 0;
                return data;
            }

        }

        #region InActivePeople Sarthi Profile
        [Route("SarthiProfile")]
        [HttpGet]
        public bool SarthiProfile(string Mobile)
        {
            //Peopleresponse res;
            bool activeStatus = false;
            People person = new People();
            if (Mobile != null)
            {
                using (var db = new AuthContext())
                {
                    person = db.Peoples.Where(u => u.Mobile == Mobile && u.Active && !u.Deleted).OrderByDescending(x => x.PeopleID).SingleOrDefault();
                    if (person != null)
                    {
                        AppVisits appVisit = new AppVisits();
                        var identity = User.Identity as ClaimsIdentity;
                        int userid = 0;
                        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                        var CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
                        MongoDbHelper<AppVisits> mongoDbHelper = new MongoDbHelper<AppVisits>();

                        appVisit.UserName = Mobile;
                        appVisit.UserId = person.PeopleID;
                        appVisit.AppType = "Sarthi App";
                        appVisit.VisitedOn = DateTime.Now;
                        var Status = mongoDbHelper.InsertAsync(appVisit);

                        return person.Active;
                    }
                    else
                    {
                        return activeStatus;
                    }
                }
            }

            return activeStatus;
        }

        #endregion

    }
    public class SpLogin
    {

        public int PeopleId { get; set; }
        public string Role { get; set; }

    }
    public class RolesDc
    {

        public List<RoleDc> RoleList { get; set; }

    }
    public class RoleDc
    {
        public string Role { get; set; }

    }

}



