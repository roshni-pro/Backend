using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.External.Gamification;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.BusinessLayer.FinBox;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.CustomerReferralDc;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.Customer;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using AngularJSAuthentication.Model.CustomerDelight;
using AngularJSAuthentication.Model.CustomerReferral;
using AngularJSAuthentication.Model.CustomerShoppingCart;
using AngularJSAuthentication.Model.Login;
using AngularJSAuthentication.Model.NotMapped;
using AngularJSAuthentication.Model.Others;
using AngularJSAuthentication.Model.Rating;
using GenricEcommers.Models;
using LinqKit;
using MongoDB.Bson;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.CustomersController;
using static AngularJSAuthentication.API.Controllers.NotificationController;
using static AngularJSAuthentication.DataContracts.External.GamificationDc;
using static AngularJSAuthentication.API.Controllers.BackendOrderController;
using AngularJSAuthentication.API.Helper.Notification;

namespace AngularJSAuthentication.API.Controllers.External.RetailerAPP
{
    [RoutePrefix("api/RetailerApp")]
    [Authorize]
    public class MobileRetailerAppController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public static int a = 0;
        public static int b = 0;
        public static int c = 0;
        public static int total = 0;
        public string MemberShipName = AppConstants.MemberShipName;
        public string MemberShipHindiName = "फायदा";
        public int MemberShipHours = AppConstants.MemberShipHours;
        public double xPointValue = AppConstants.xPoint;
        public bool ElasticSearchEnable = AppConstants.ElasticSearchEnable;
        public bool EnableOtherLanguage = false;


        [HttpGet]
        [Route("RetailerAppVersion")]
        [AllowAnonymous]
        public async Task<List<appVersion>> Get()
        {
            using (AuthContext context = new AuthContext())
            {
                var item = context.appVersionDb.Where(x => x.isCompulsory).ToList();
                return item;
            }

        }

        [Route("Singup")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public async Task<CustDetails> Post(RetailerAppDC customer)
        {
            using (AuthContext context = new AuthContext())
            {
                customeritems ibjtosend = new customeritems();
                CustDetails res;
                List<string> FCMIds = new List<string>();
                RetailerAppDC newcustomer = new RetailerAppDC();


                if (!string.IsNullOrEmpty(customer.deviceId) && !customer.TrueCustomer && context.CustomerLoginDeviceDB.Count(x => x.DeviceId == customer.deviceId && x.Mobile != customer.Mobile) > 2)
                {
                    res = new CustDetails()
                    {
                        customers = null,
                        Status = false,
                        NotAuthorize = true,
                        Message = "Your device is already attached with another account. Please contact customer care."
                    };
                    return res;
                }
                if (!string.IsNullOrEmpty(customer.deviceId) && !customer.TrueCustomer)
                {
                    var deviceIdCheck = context.Peoples.Any(x => x.DeviceId == customer.deviceId);
                    if (deviceIdCheck)
                    {
                        res = new CustDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "You are not authorized to login on this device."
                        };
                        return res;
                    }
                }

                if (!string.IsNullOrEmpty(customer.RefNo) && !customer.TrueCustomer)
                {
                    var gstCheck = context.Customers.Any(x => x.RefNo == customer.RefNo);
                    if (gstCheck)
                    {
                        res = new CustDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "GST Already Exists."
                        };
                        return res;
                    }
                }
                Customer custExist = context.Customers.Where(x => x.Deleted == false && x.Mobile == customer.Mobile).SingleOrDefault();
                newcustomer = Mapper.Map(custExist).ToANew<RetailerAppDC>();
                if (custExist != null)
                {
                    Customer cust = custExist;

                    if (cust != null && cust.Password != customer.Password)
                        cust = null;

                    if (cust == null)
                    {
                        res = new CustDetails()
                        {
                            customers = newcustomer,
                            Status = false,
                            Message = "Incorrect password. Try again"
                        };
                        return res;
                    }
                    else
                    {
                        if (cust.IsKPP && context.DistributorVerificationDB.Any(x => x.CustomerID == cust.CustomerId))
                        {
                            res = new CustDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "You are not eligible to access this app. Please contact customer care."
                            };
                            return res;
                        }

                        if (customer.fcmId != null && customer.fcmId.Trim() != "" && customer.fcmId.Trim().ToUpper() != "NULL")
                        {
                            if (customer.fcmId != cust.fcmId)
                                FCMIds.Add(cust.fcmId);


                            cust.CurrentAPKversion = customer.CurrentAPKversion;
                            cust.PhoneOSversion = customer.PhoneOSversion;
                            cust.UserDeviceName = customer.UserDeviceName;
                            cust.fcmId = !string.IsNullOrEmpty(customer.fcmId) ? customer.fcmId : cust.fcmId;
                            cust.imei = customer.deviceId;
                            context.Entry(cust).State = EntityState.Modified;
                            context.Commit();

                            newcustomer = Mapper.Map(cust).ToANew<RetailerAppDC>();

                            if (!string.IsNullOrEmpty(customer.deviceId))
                            {
                                var changePassword = false;
                                var deviceLogins = context.CustomerLoginDeviceDB.Where(x => x.CustomerId == cust.CustomerId).ToList();

                                if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == customer.deviceId && x.IsCurrentLogin))
                                {
                                    var login = deviceLogins.FirstOrDefault(x => x.DeviceId == customer.deviceId);
                                    login.LastLogin = DateTime.Now;
                                    login.CurrentAPKversion = customer.CurrentAPKversion;
                                    login.PhoneOSversion = customer.PhoneOSversion;
                                    context.Entry(login).State = EntityState.Modified;
                                    context.Commit();
                                }
                                else
                                {

                                    if (deviceLogins == null || !deviceLogins.Any())
                                    {
                                        var login = new CustomerLoginDevice()
                                        {
                                            DeviceId = customer.deviceId,
                                            CustomerId = cust.CustomerId,
                                            CreatedDate = DateTime.Now,
                                            IsCurrentLogin = true,
                                            LastLogin = DateTime.Now,
                                            CurrentAPKversion = customer.CurrentAPKversion,
                                            PhoneOSversion = customer.PhoneOSversion,
                                            UserDeviceName = customer.UserDeviceName,
                                            FCMID = customer.fcmId,
                                            Mobile = customer.Mobile
                                        };
                                        context.Entry(login).State = EntityState.Added;

                                    }
                                    else if (deviceLogins != null && !deviceLogins.Any(x => x.DeviceId == customer.deviceId))
                                    {
                                        var login = new CustomerLoginDevice()
                                        {
                                            DeviceId = customer.deviceId,
                                            CustomerId = cust.CustomerId,
                                            CreatedDate = DateTime.Now,
                                            IsCurrentLogin = true,
                                            LastLogin = DateTime.Now,
                                            CurrentAPKversion = customer.CurrentAPKversion,
                                            PhoneOSversion = customer.PhoneOSversion,
                                            UserDeviceName = customer.UserDeviceName,
                                            FCMID = customer.fcmId,
                                            Mobile = customer.Mobile
                                        };
                                        context.Entry(login).State = EntityState.Added;
                                        FCMIds.AddRange(deviceLogins.Where(x => x.IsCurrentLogin && !string.IsNullOrEmpty(x.FCMID)).Select(x => x.FCMID).ToList());
                                        deviceLogins.Where(x => x.IsCurrentLogin).ToList().ForEach(x =>
                                        {
                                            x.IsCurrentLogin = false;
                                            context.Entry(x).State = EntityState.Modified;
                                        });

                                        changePassword = true;
                                    }
                                    else if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == customer.deviceId && !x.IsCurrentLogin))
                                    {
                                        var login = deviceLogins.FirstOrDefault(x => x.DeviceId == customer.deviceId && !x.IsCurrentLogin);
                                        login.LastLogin = DateTime.Now;
                                        login.IsCurrentLogin = true;
                                        login.CurrentAPKversion = customer.CurrentAPKversion;
                                        login.PhoneOSversion = customer.PhoneOSversion;
                                        context.Entry(login).State = EntityState.Modified;

                                        FCMIds.AddRange(deviceLogins.Where(x => x.DeviceId != customer.deviceId && x.IsCurrentLogin && !string.IsNullOrEmpty(x.FCMID)).Select(x => x.FCMID).ToList());
                                        deviceLogins.Where(x => x.DeviceId != customer.deviceId && x.IsCurrentLogin).ToList().ForEach(x =>
                                        {
                                            x.IsCurrentLogin = false;
                                            context.Entry(x).State = EntityState.Modified;
                                        });

                                        changePassword = true;
                                    }

                                    context.Commit();


                                    if (FCMIds.Any())
                                    {
                                        //var randomPassword = PasswordGenerator.GetRandomPassword(4, "0123456789");
                                        //customers.Password = randomPassword;
                                        //customers.IsResetPasswordOnLogin = true;
                                        //customers.UpdatedDate = indianTime;
                                        //dbContext.Entry(customers).State = EntityState.Modified;
                                        //dbContext.Commit();
                                        string Key = ConfigurationManager.AppSettings["FcmApiKey"];
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
                                        //var objNotificationList = FCMIds.Select(x => new
                                        //{
                                        //    to = x,
                                        //    CustId = 0,
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

                                        var firebaseService = new FirebaseNotificationServiceHelper(Key);
                                        ParallelLoopResult parellelResult = Parallel.ForEach(FCMIds.Distinct(), async (x) =>
                                         {
                                             try
                                             {
                                                 var Result = await firebaseService.SendNotificationForApprovalAsync(x, data);
                                                 if (Result != null)
                                                 {
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
                                                 //                    //totalSent.Add(1);
                                                 //                }
                                                 //                else if (response.failure == 1)
                                                 //                {
                                                 //                    //totalNotSent.Add(1);
                                                 //                }
                                                 //            }
                                                 //        }
                                                 //    }
                                                 //}


                                             }
                                             catch (Exception asd)
                                             {
                                                 CustomersController.logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(data.notify_type).Append(asd.ToString()).ToString());
                                             }
                                         });

                                    }
                                }
                            }
                        }

                    }

                    var registered = context.GetAPKUserAndPwd("RetailersApp");
                    newcustomer.APKType = registered;
                    var primeCustomer = context.PrimeCustomers.FirstOrDefault(x => x.CustomerId == newcustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                    if (primeCustomer != null)
                    {
                        newcustomer.IsPrimeCustomer = primeCustomer.StartDate <= indianTime && primeCustomer.EndDate >= indianTime;
                        newcustomer.PrimeStartDate = primeCustomer.StartDate;
                        newcustomer.PrimeEndDate = primeCustomer.EndDate;
                    }


                    res = new CustDetails()
                    {
                        customers = newcustomer,
                        Status = true,
                        Message = "Customer Signup Succesfully."
                    };
                    return res;
                }

                else
                {
                    res = new CustDetails()
                    {
                        customers = newcustomer,
                        Status = false,
                        Message = "Customer not exist."
                    };
                    return res;
                }
            }
        }



        [Route("ForgetPassword")]
        [HttpGet]
        [AllowAnonymous]

        public async Task<MessageDetails> GetForget(string Mobile)
        {
            MessageDetails res;
            customerDetail cd;
            Customer customer = new Customer();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    int CompanyId = compid;

                    customer = db.GetCustomerbyId(Mobile);
                    if (customer != null)
                    {
                        new Sms().sendOtp(customer.Mobile, "Hi " + customer.ShopName + " \n\t You Recently requested a forget password on ShopKirana. Your account Password is '" + customer.Password + "'\n If you didn't request then ingore this message\n\t\t Thanks\n\t\t Shopkirana.com", "");

                        res = new MessageDetails()
                        {

                            Status = true,
                            Message = "Message send to your registered mobile number."
                        };

                        return res;
                    }
                    else
                    {
                        res = new MessageDetails()
                        {

                            Status = false,
                            Message = "Customer not exist."
                        };
                        return res;
                    }
                }
                catch (Exception ex)
                {

                    res = new MessageDetails()
                    {

                        Status = false,
                        Message = ex.Message
                    };
                    return res;
                }
            }
        }
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            using (AuthContext db = new AuthContext())
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
        }
        [Route("Genotp")]
        [HttpGet]
        [AllowAnonymous]

        public OTP Getotp(string MobileNumber, string deviceId, string Apphash, string mode = "")
        {
            var todaydate = DateTime.Now.Date;
            IEnumerable<string> username = new List<string>(); ;
            Request.Headers.TryGetValues("username", out username);

            if (username != null && username.Any() && username.FirstOrDefault().Split('_')[0].Trim() == MobileNumber)
            {
                MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
                var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == MobileNumber && x.CreatedDate >= todaydate);
                var CustomerOTPCount = mongoDbHelper.Count(cartPredicate);


                if (CustomerOTPCount >= 10)
                {
                    var ret = new OTP()
                    {
                        OtpNo = "You have exceeded your daily OTP limit.Please contact customer care.",
                        Status = false,
                        Message = "You have exceeded your daily OTP limit.Please contact customer care."
                    };
                    return ret;
                }

                bool TestUser = false;
                string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
                OTP b = new OTP();
                using (var context = new AuthContext())
                {
                    var checkDeviceId = context.Peoples.Any(x => x.DeviceId == deviceId);
                    if (checkDeviceId)
                    {
                        b = new OTP()
                        {
                            OtpNo = "You are not authorize",
                            Status = false,
                            Message = "You are not authorize"
                        };
                        return b;
                    }

                    Customer cust = context.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim())).FirstOrDefault();


                    if (cust != null && cust.IsKPP)
                    {
                        TestUser = cust.CustomerCategoryId.HasValue && cust.CustomerCategoryId.Value == 0;
                        if (context.DistributorVerificationDB.Any(x => x.CustomerID == cust.CustomerId))
                        {
                            b = new OTP()
                            {
                                OtpNo = "You are not authorize",
                                Status = false,
                                Message = "You are not authorize"
                            };
                            return b;
                        }
                    }
                    else if (cust != null && cust.Deleted)
                    {
                        b = new OTP()
                        {
                            Status = false,
                            Message = "Your account is not active please contact customer care."
                        };
                        return b;
                    }
                    else if (cust != null && !string.IsNullOrEmpty(cust.CustomerType))
                    {
                        if (cust.CustomerType.ToLower() == "consumer")
                        {
                            b = new OTP()
                            {
                                OtpNo = "You are not authorize",
                                Status = false,
                                Message = "You are not authorize"
                            };
                            return b;
                        }

                    }
                }
                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                // string OtpMessage = " is Your login Code. :). ShopKirana";
                string OtpMessage = ""; //"{#var1#} is Your login Code. {#var2#}. ShopKirana";
                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Login_Code");
                OtpMessage = dltSMS == null ? "" : dltSMS.Template;
                OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);

                if (string.IsNullOrEmpty(Apphash))
                {
                    Apphash = ConfigurationManager.AppSettings["Apphash"];
                }

                OtpMessage = OtpMessage.Replace("{#var2#}", Apphash);
                //string OtpMessage = string.Format("<#> {0} : is Your Shopkirana Verification Code for complete process.{1}{2} Shopkirana", sRandomOTP, Environment.NewLine, Apphash);
                // string message = sRandomOTP + " " + OtpMessage;
                //string message = OtpMessage;
                var status = dltSMS == null ? false : Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                if (status)
                {
                    Model.CustomerOTP.RetailerCustomerOTP CustomerOTP = new Model.CustomerOTP.RetailerCustomerOTP
                    {
                        CreatedDate = DateTime.Now,
                        DeviceId = deviceId,
                        IsActive = true,
                        Mobile = MobileNumber,
                        Otp = sRandomOTP,

                    };
                    mongoDbHelper.Insert(CustomerOTP);
                }


                OTP a = new OTP()
                {
                    OtpNo = TestUser || (!string.IsNullOrEmpty(mode) && mode == "debug") ? sRandomOTP : "Successfully sent OTP",
                    Status = true,
                    Message = "Successfully sent OTP"

                };
                return a;
            }
            else
            {
                var retVal = new OTP()
                {
                    OtpNo = "You are not authorize",
                    Status = false,
                    Message = "You are not authorize"
                };
                return retVal;
            }

        }

        [Route("ClearAllOtp")]
        [HttpGet]
        [AllowAnonymous]
        public bool ClearAllOtp(string MobileNumber)
        {
            MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
            var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == MobileNumber);
            var CustomerOTPs = mongoDbHelper.Select(cartPredicate).ToList();

            foreach (var item in CustomerOTPs)
            {
                mongoDbHelper.Delete(item.Id);
            }
            return true;
        }

        [Route("CheckOTP")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> CheckOTP(OtpCheckDc otpCheckDc)
        {
            MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
            var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == otpCheckDc.MobileNumber && x.DeviceId == otpCheckDc.deviceId);
            var CustomerOTPs = mongoDbHelper.Select(cartPredicate).ToList();
            if (CustomerOTPs != null && CustomerOTPs.Any(x => x.Otp == otpCheckDc.Otp))
            {
                foreach (var item in CustomerOTPs)
                {
                    await mongoDbHelper.DeleteAsync(item.Id);
                }


                using (var context = new AuthContext())
                {
                    var cust = context.Customers.Where(x => x.Deleted == false && x.Mobile == otpCheckDc.MobileNumber).FirstOrDefault();
                    if (cust != null)
                    {
                        List<string> FCMIds = new List<string>();
                        if (cust.fcmId != otpCheckDc.fcmId)
                            FCMIds.Add(cust.fcmId);

                        cust.CurrentAPKversion = otpCheckDc.CurrentAPKversion;
                        cust.PhoneOSversion = otpCheckDc.PhoneOSversion;
                        cust.UserDeviceName = otpCheckDc.UserDeviceName;
                        cust.fcmId = (otpCheckDc.fcmId != null || otpCheckDc.fcmId != "" || otpCheckDc.fcmId != "NULL") ? otpCheckDc.fcmId : cust.fcmId;
                        cust.imei = otpCheckDc.deviceId;
                        cust.UpdatedDate = indianTime;
                        context.Entry(cust).State = EntityState.Modified;
                        context.Commit();


                        if (!string.IsNullOrEmpty(otpCheckDc.deviceId))
                        {
                            var deviceLogins = context.CustomerLoginDeviceDB.Where(x => x.CustomerId == cust.CustomerId).ToList();
                            if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == otpCheckDc.deviceId && x.IsCurrentLogin))
                            {
                                var login = deviceLogins.FirstOrDefault(x => x.DeviceId == otpCheckDc.deviceId);
                                login.LastLogin = DateTime.Now;
                                login.CurrentAPKversion = otpCheckDc.CurrentAPKversion;
                                login.PhoneOSversion = otpCheckDc.PhoneOSversion;
                                context.Entry(login).State = EntityState.Modified;
                                context.Commit();
                            }
                            else
                            {

                                if (deviceLogins == null || !deviceLogins.Any())
                                {
                                    var login = new CustomerLoginDevice()
                                    {
                                        DeviceId = otpCheckDc.deviceId,
                                        CustomerId = cust.CustomerId,
                                        CreatedDate = DateTime.Now,
                                        IsCurrentLogin = true,
                                        LastLogin = DateTime.Now,
                                        CurrentAPKversion = otpCheckDc.CurrentAPKversion,
                                        PhoneOSversion = otpCheckDc.PhoneOSversion,
                                        UserDeviceName = otpCheckDc.UserDeviceName,
                                        FCMID = otpCheckDc.fcmId,
                                        Mobile = otpCheckDc.MobileNumber
                                    };
                                    context.Entry(login).State = EntityState.Added;

                                }
                                else if (deviceLogins != null && !deviceLogins.Any(x => x.DeviceId == otpCheckDc.deviceId))
                                {
                                    var login = new CustomerLoginDevice()
                                    {
                                        DeviceId = otpCheckDc.deviceId,
                                        CustomerId = cust.CustomerId,
                                        CreatedDate = DateTime.Now,
                                        IsCurrentLogin = true,
                                        LastLogin = DateTime.Now,
                                        CurrentAPKversion = otpCheckDc.CurrentAPKversion,
                                        PhoneOSversion = otpCheckDc.PhoneOSversion,
                                        UserDeviceName = otpCheckDc.UserDeviceName,
                                        FCMID = otpCheckDc.fcmId,
                                        Mobile = otpCheckDc.MobileNumber
                                    };
                                    context.Entry(login).State = EntityState.Added;
                                    FCMIds.AddRange(deviceLogins.Where(x => x.IsCurrentLogin && !string.IsNullOrEmpty(x.FCMID)).Select(x => x.FCMID).ToList());
                                    deviceLogins.Where(x => x.IsCurrentLogin).ToList().ForEach(x =>
                                    {
                                        x.IsCurrentLogin = false;
                                        context.Entry(x).State = EntityState.Modified;
                                    });
                                }
                                else if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == otpCheckDc.deviceId && !x.IsCurrentLogin))
                                {
                                    var login = deviceLogins.FirstOrDefault(x => x.DeviceId == otpCheckDc.deviceId && !x.IsCurrentLogin);
                                    login.LastLogin = DateTime.Now;
                                    login.IsCurrentLogin = true;
                                    login.CurrentAPKversion = otpCheckDc.CurrentAPKversion;
                                    login.PhoneOSversion = otpCheckDc.PhoneOSversion;
                                    context.Entry(login).State = EntityState.Modified;

                                    FCMIds.AddRange(deviceLogins.Where(x => x.DeviceId != otpCheckDc.deviceId && x.IsCurrentLogin && !string.IsNullOrEmpty(x.FCMID)).Select(x => x.FCMID).ToList());
                                    deviceLogins.Where(x => x.DeviceId != otpCheckDc.deviceId && x.IsCurrentLogin).ToList().ForEach(x =>
                                    {
                                        x.IsCurrentLogin = false;
                                        context.Entry(x).State = EntityState.Modified;
                                    });
                                }

                                context.Commit();


                                if (FCMIds.Any())
                                {
                                    string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                                    var firebaseService = new FirebaseNotificationServiceHelper(Key);

                                    //var objNotificationList = FCMIds.Distinct().Select(x => new
                                    //{
                                    //    to = x,
                                    //    CustId = 0,
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
                                    ParallelLoopResult parellelResult = Parallel.ForEach(FCMIds, async (x) =>
                                     {
                                        //var AutoNotification = new ManualAutoNotification
                                        //{
                                        //    CreatedDate = DateTime.Now,
                                        //    FcmKey = ConfigurationManager.AppSettings["FcmApiKey"],
                                        //    IsActive = true,
                                        //    IsSent = false,
                                        //    NotificationMsg = Newtonsoft.Json.JsonConvert.SerializeObject(x),
                                        //    ObjectId = cust.CustomerId,
                                        //    ObjectType = "Customer"
                                        //};
                                        try
                                         {
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
                                            var Result = await firebaseService.SendNotificationForApprovalAsync(x, data);
                                             if (Result != null)
                                             {
                                             }
                                         }
                                         catch (Exception asd)
                                         {
                                            //AutoNotification.IsSent = false;
                                            CustomersController.logger.Error(new StringBuilder("Error while sending FCM for type: ").Append(data.notify_type).Append(asd.ToString()).ToString());
                                         }
                                     });
                                    if (parellelResult.IsCompleted && AutoNotifications != null && AutoNotifications.Any())
                                    {
                                        var autoNot = AutoNotifications.ToList();
                                        AutoNotificationmongoDbHelper.InsertMany(autoNot);
                                    }
                                }
                            }
                        }

                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        [Route("GetCustomerloged")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CustDetails> GetLogedCustomer(string MobileNumber, bool IsOTPverified, string fcmid, string CurrentAPKversion, string PhoneOSversion, string UserDeviceName, string IMEI = "")
        {
            CustDetails res;
            RetailerAppDC newcustomer = new RetailerAppDC();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    if (IsOTPverified == true)
                    {
                        Customer customersMobileExist = db.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim())).FirstOrDefault();
                        newcustomer = Mapper.Map(customersMobileExist).ToANew<RetailerAppDC>();
                        if (customersMobileExist != null)
                        {
                            var registeredApk = db.GetAPKUserAndPwd("RetailersApp");
                            newcustomer.APKType = registeredApk;
                            if (customersMobileExist.IsCityVerified)
                            {
                                if (fcmid != null && fcmid.Trim() != "" && fcmid.Trim().ToUpper() != "NULL")
                                {
                                    customersMobileExist.fcmId = fcmid;
                                    customersMobileExist.CurrentAPKversion = CurrentAPKversion;
                                    customersMobileExist.PhoneOSversion = PhoneOSversion;
                                    customersMobileExist.UserDeviceName = UserDeviceName;
                                    customersMobileExist.IsResetPasswordOnLogin = false;
                                    customersMobileExist.imei = !string.IsNullOrEmpty(IMEI) ? IMEI : customersMobileExist.imei;
                                    db.Entry(customersMobileExist).State = EntityState.Modified;
                                    //newcustomer = Mapper.Map(customersMobileExist).ToANew<RetailerAppDC>();
                                    db.Commit();
                                }

                                var primeCustomer = db.PrimeCustomers.FirstOrDefault(x => x.CustomerId == newcustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                                if (primeCustomer != null)
                                {
                                    newcustomer.IsPrimeCustomer = primeCustomer.StartDate <= indianTime && primeCustomer.EndDate >= indianTime;
                                    newcustomer.PrimeStartDate = primeCustomer.StartDate;
                                    newcustomer.PrimeEndDate = primeCustomer.EndDate;
                                }
                                res = new CustDetails()
                                {
                                    customers = newcustomer,
                                    Status = true,
                                    Message = "Success."
                                };
                                return res;
                            }
                            else
                            {
                                res = new CustDetails()
                                {
                                    customers = newcustomer,
                                    Status = false,
                                    Message = "Customer city not exist or not verified."
                                };
                                return res;
                            }

                        }
                        else
                        {
                            res = new CustDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "Customer not exist or Incorect mobile number."
                            };
                            return res;
                        }
                    }
                    else
                    {
                        res = new CustDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "OTP not verified."
                        };
                        return res;
                    }


                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string skcode()
        {
            using (AuthContext db = new AuthContext())
            {
                var query = "select max(cast(replace(skcode,'SK','') as bigint)) from customers ";
                var intSkCode = db.Database.SqlQuery<long?>(query).FirstOrDefault();
                var skcode = "SK" + (intSkCode ?? 0 + 1);
                bool flag = false;
                while (flag == false)
                {
                    var check = db.Customers.Any(s => s.Skcode.Trim().ToLower() == skcode.Trim().ToLower());

                    if (!check)
                    {
                        flag = true;
                        return skcode;
                    }
                    else
                    {
                        intSkCode += 1;
                        skcode = "SK" + intSkCode;
                    }
                }

                return skcode;
            }
        }

        [Route("InsertCustomer")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]

        public async Task<custdata> addCustomer(string MobileNumber)
        {
            CustomersController.logger.Info("start add Customer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    Customer customers = db.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim())).FirstOrDefault();
                    if (MobileNumber == null)
                    {
                        throw new ArgumentNullException("MobileNumber");
                    }
                    if (customers == null)
                    {
                        var Skcode = skcode();
                        Customer c = new Customer();
                        c.Mobile = MobileNumber;
                        c.CompanyId = 1;
                        c.Skcode = Skcode;
                        c.CreatedDate = indianTime;
                        c.UpdatedDate = indianTime;
                        db.Customers.Add(c);
                        db.Commit();

                        #region CODLimitCustomers
                        var cust = db.Customers.FirstOrDefault(x => x.Mobile == MobileNumber);
                        var codLimit = db.CompanyDetailsDB.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false);
                        CODLimitCustomer codLimitCustomer = new CODLimitCustomer();
                        codLimitCustomer.CustomerId = cust.CustomerId;
                        codLimitCustomer.CODLimit = codLimit.CODLimit;
                        codLimitCustomer.IsActive = true;
                        codLimitCustomer.IsDeleted = false;
                        codLimitCustomer.IsCustomCODLimit = true;
                        codLimitCustomer.CreatedDate = DateTime.Now;
                        db.CODLimitCustomers.Add(codLimitCustomer);
                        db.Commit();
                        #endregion


                        var registeredApk = db.GetAPKUserAndPwd("RetailersApp");


                        custdata cs = new custdata
                        {
                            CustomerId = c.CustomerId,
                            CompanyId = c.CompanyId,
                            Mobile = c.Mobile,
                            Skcode = c.Skcode,
                            Warehouseid = c.Warehouseid,
                            RegisteredApk = Mapper.Map(registeredApk).ToANew<AngularJSAuthentication.DataContracts.KPPApp.ApkNamePwdResponse>()
                        };

                        #region gamification entry for New Customer sign up 

                        GamificationController gamification = new GamificationController();
                        var res = gamification.NewCustomerSignUp(cs.CustomerId, cs.Skcode);

                        #endregion

                        return cs;
                    }
                    else
                    {
                        int? WarehouseId = 0;
                        try
                        {
                            //CustWarehouse cw = db.CustWarehouseDB.Where(x => x.CustomerId == customers.CustomerId).FirstOrDefault();
                            Customer cw = db.Customers.Where(x => x.CustomerId == customers.CustomerId).FirstOrDefault();
                            if (cw != null)
                            {
                                WarehouseId = cw.Warehouseid;
                            }
                        }
                        catch (Exception Esy)
                        {
                        }

                        var registeredApk = db.GetAPKUserAndPwd("RetailersApp");

                        custdata cs = new custdata
                        {
                            CustomerId = customers.CustomerId,
                            CompanyId = customers.CompanyId,
                            Mobile = customers.Mobile,
                            Skcode = customers.Skcode,
                            Warehouseid = WarehouseId,
                            RegisteredApk = Mapper.Map(registeredApk).ToANew<AngularJSAuthentication.DataContracts.KPPApp.ApkNamePwdResponse>()
                        };
                        return cs;
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();

                    custdata cs = new custdata
                    {
                        IsCityVerified = false
                    };
                    return cs;
                }
            }
        }

        [Route("CustomerGSTVerify")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<CustGstDTOList> GetCustomerGSTVerify(string GSTNO)
        {
            string path = ConfigurationManager.AppSettings["GetCustomerGstUrl"];
            path = path.Replace("[[GstNo]]", GSTNO);
            var gst = new CustomerGst();

            using (GenericRestHttpClient<CustomerGst, string> memberClient
                   = new GenericRestHttpClient<CustomerGst, string>(path,
                   string.Empty, null))
            {
                try
                {
                    gst = await memberClient.GetAsync();
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("GST API error: " + ex.ToString());
                    gst.error = true;
                }

                if (gst.error == false)
                {
                    using (AuthContext db = new AuthContext())
                    {
                        CustGSTverifiedRequest GSTdata = new CustGSTverifiedRequest();
                        GSTdata.RequestPath = path;
                        GSTdata.RefNo = gst.taxpayerInfo.gstin;
                        GSTdata.Name = gst.taxpayerInfo.lgnm;
                        GSTdata.ShopName = gst.taxpayerInfo.tradeNam;
                        GSTdata.Active = gst.taxpayerInfo.sts;
                        GSTdata.ShippingAddress = gst.taxpayerInfo.pradr?.addr?.st;
                        //GSTdata.ShippingAddress=gst.taxpayerInfo.pradr.
                        GSTdata.State = gst.taxpayerInfo.pradr?.addr?.stcd;
                        GSTdata.City = gst.taxpayerInfo.pradr?.addr?.loc;
                        GSTdata.lat = gst.taxpayerInfo.pradr?.addr?.lt;
                        GSTdata.lg = gst.taxpayerInfo.pradr?.addr?.lg;
                        GSTdata.Zipcode = gst.taxpayerInfo.pradr?.addr?.pncd;
                        GSTdata.RegisterDate = gst.taxpayerInfo.rgdt;
                        GSTdata.LastUpdate = gst.taxpayerInfo.lstupdt;
                        GSTdata.HomeName = gst.taxpayerInfo.pradr?.addr?.bnm;
                        GSTdata.HomeNo = gst.taxpayerInfo.pradr?.addr?.bno;
                        GSTdata.CustomerBusiness = gst.taxpayerInfo.nba != null && gst.taxpayerInfo.nba.Any() ? gst.taxpayerInfo.nba[0] : "";
                        GSTdata.Citycode = gst.taxpayerInfo.ctjCd;
                        GSTdata.PlotNo = gst.taxpayerInfo.pradr?.addr?.flno;
                        GSTdata.Message = gst.error;
                        GSTdata.UpdateDate = DateTime.Now;
                        GSTdata.CreateDate = DateTime.Now;
                        GSTdata.Delete = false;
                        string CityM, CityD;
                        CityM = GSTdata.City;
                        CityD = gst.taxpayerInfo.pradr?.addr?.dst;
                        CityM = CityM.ToUpper();
                        CityD = CityD.ToUpper();
                        if (String.Compare(CityM, CityD) == 0)
                        {

                            CityD = "";
                        };

                        db.CustGSTverifiedRequestDB.Add(GSTdata);
                        db.Commit();

                        if (!string.IsNullOrEmpty(GSTdata.City) && !string.IsNullOrEmpty(GSTdata.State))
                        {
                            Managers.CustomerAddressRequestManager manager = new Managers.CustomerAddressRequestManager();
                            manager.AddGSTCityAndState(GSTdata.City, GSTdata.Zipcode, GSTdata.State, GSTdata.RefNo.Substring(0, 2), db);
                        }

                        CustGstVerify Cust = new CustGstVerify()
                        {
                            id = GSTdata.GSTVerifiedRequestId,
                            RefNo = gst.taxpayerInfo.gstin,
                            Name = gst.taxpayerInfo.lgnm,
                            ShopName = gst.taxpayerInfo.tradeNam,
                            //if (GSTdata.City== gst.taxpayerInfo.pradr?.addr?.dst)
                            //{ };
                            ShippingAddress = string.Format("{0}, {1}, {2}, {3},{4},{5},{6}-{7}", GSTdata.HomeNo, GSTdata.PlotNo, GSTdata.HomeName, GSTdata.ShippingAddress, GSTdata.City, CityD, GSTdata.State, GSTdata.Zipcode),
                            State = gst.taxpayerInfo.pradr?.addr?.stcd,
                            City = gst.taxpayerInfo.pradr?.addr?.loc,
                            Active = gst.taxpayerInfo.sts,
                            lat = gst.taxpayerInfo.pradr?.addr?.lt,
                            lg = gst.taxpayerInfo.pradr?.addr?.lg,
                            Zipcode = gst.taxpayerInfo.pradr?.addr?.pncd
                        };
                        if (Cust.Active == "Active")
                        {
                            CustGstDTOList Custlist = new CustGstDTOList()
                            {
                                custverify = Cust,
                                Status = true,
                                Message = "Customer GST Number Is Verify Successfully."
                            };
                            return Custlist;
                        }
                        else
                        {
                            CustGstDTOList Custlist = new CustGstDTOList()
                            {
                                custverify = Cust,
                                Status = false,
                                Message = "Customer GST Number Is " + Cust.Active
                            };
                            return Custlist;
                        }


                    }
                }

                else
                {
                    CustGstDTOList Custlist = new CustGstDTOList()
                    {
                        custverify = null,
                        Status = false,
                        Message = "Customer GST Number not valid."
                    };
                    return Custlist;
                }
            }

        }

        [Route("GetCity")]
        [HttpGet]
        public async Task<List<CityDC>> GetCity()
        {
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (Warehouse_id > 0)
                {
                    var wh = context.Warehouses.Where(x => x.WarehouseId == Warehouse_id).SingleOrDefault();
                    var ass = context.Cities.Where(x => x.Cityid == wh.Cityid).Select(x => new CityDC
                    {
                        Cityid = x.Cityid,
                        CityName = x.CityName,
                        CityLatitude = x.CityLatitude,
                        CityLongitude = x.CityLongitude
                    }).ToList();
                    return ass;
                }
                else
                {
                    var ass = context.Cities.Select(x => new CityDC
                    {
                        Cityid = x.Cityid,
                        CityName = x.CityName,
                        CityLatitude = x.CityLatitude,
                        CityLongitude = x.CityLongitude
                    }).ToList();
                    return ass;
                }
            }
        }


        [Route("signupCustomer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SignupCustomer(Customer obj)
        {
            CustomersController.logger.Info("start add Customer if already add update customer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    customerDetail res;
                    var checkDeviceId = db.Peoples.Where(x => x.DeviceId == obj.deviceId).Count();
                    if (checkDeviceId > 0)
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "You are not authorized."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }

                    Customer customers = db.Customers.Where(c => c.CustomerId == obj.CustomerId).FirstOrDefault();
                    if (customers.CustomerVerify == "Full Verified" || customers.CustomerVerify == "Partial Verified")
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Since you are verified customer, You can't update"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }


                    if (customers != null && (customers.CustomerDocumentStatus == (int)CustomerDocumentStatusEnum.Verified || customers.ShippingAddressStatus == (int)ShippingAddressStatusEnum.PhysicalVerified || customers.ShippingAddressStatus == (int)ShippingAddressStatusEnum.VirtualVerified))
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Since your document or address are verified, You can't update"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }



                    if (obj.LicenseExpiryDate != null && obj.LicenseExpiryDate < DateTime.Now)
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "License Expiry Date Already Expired, Please enter valid date"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }

                    City City = db.Cities.Where(a => a.CityName.Trim().ToLower() == obj.City.Trim().ToLower()).FirstOrDefault();

                    string oldshippingaddress = customers.ShippingAddress;

                    bool TrackRequest = false;
                    if (customers.lat != obj.lat || customers.lg != obj.lg || customers.RefNo != obj.RefNo || customers.LicenseNumber != obj.LicenseNumber || customers.ShippingAddress != obj.ShippingAddress)
                        TrackRequest = true;

                    #region CustomerAddress status and lt lg :14/07/2022
                    if (customers.ShippingAddressStatus == (int)ShippingAddressStatusEnum.NotVerified)
                    {
                        if (obj.CustomerAddress != null)
                        {
                            customers.ShippingAddress = null;
                            if (!string.IsNullOrEmpty(obj.CustomerAddress.AddressLineOne))
                            {
                                customers.ShippingAddress = obj.CustomerAddress.AddressLineOne + ",";
                            }
                            if (!string.IsNullOrEmpty(obj.CustomerAddress.AddressLineTwo))
                            {
                                customers.ShippingAddress += obj.CustomerAddress.AddressLineTwo + ",";
                            }
                            if (!string.IsNullOrEmpty(obj.CustomerAddress.AddressText))
                            {
                                customers.ShippingAddress += obj.CustomerAddress.AddressText;
                            }
                            if (!string.IsNullOrEmpty(obj.CustomerAddress.ZipCode))
                            {
                                customers.ShippingAddress += "," + obj.CustomerAddress.ZipCode;
                            }
                            customers.lat = obj.CustomerAddress.AddressLat;
                            customers.lg = obj.CustomerAddress.AddressLng;

                            customers.Addresslat = obj.CustomerAddress.AddressLat;
                            customers.Addresslg = obj.CustomerAddress.AddressLng;

                        }
                    }
                    #endregion
                    customers.Name = obj.Name;
                    customers.ShopName = obj.ShopName;
                    customers.Cityid = City?.Cityid;
                    customers.City = obj.City;
                    customers.fcmId = !string.IsNullOrEmpty(obj.fcmId) ? obj.fcmId : customers.fcmId;
                    customers.ShippingCity = City != null ? City.CityName : "";
                    customers.Password = string.IsNullOrEmpty(obj.Password) == true ? customers.Password : obj.Password;
                    customers.imei = obj.imei;//Sudhir to save device info 
                    customers.PhoneOSversion = obj.PhoneOSversion;
                    customers.deviceId = obj.deviceId;
                    customers.UserDeviceName = obj.UserDeviceName;
                    customers.IsSignup = string.IsNullOrEmpty(obj.ShopName) == true ? false : true;
                    customers.UpdatedDate = indianTime;
                    customers.ZipCode = obj.ZipCode;
                    customers.AreaName = obj.AreaName;
                    customers.LandMark = obj.LandMark;
                    customers.ShippingAddress1 = obj.ShippingAddress1;
                    if (customers.lat != 0 && customers.lg != 0 && customers.Cityid > 0)
                    {
                        var query = new StringBuilder("select [dbo].[GetClusterFromLatLngAndCity]('").Append(customers.lat).Append("', '").Append(customers.lg).Append("', ").Append(customers.Cityid).Append(")");
                        var clusterId = db.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                        if (!clusterId.HasValue)
                        {
                            customers.InRegion = false;
                        }
                        else
                        {
                            var agent = db.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active);

                            if (agent != null && agent.AgentId > 0)
                                customers.AgentCode = Convert.ToString(agent.AgentId);

                            customers.ClusterId = clusterId;
                            var cluster = db.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                            customers.ClusterName = cluster.ClusterName;
                            customers.InRegion = true;
                            customers.Warehouseid = cluster.WarehouseId;
                        }
                    }
                    db.Entry(customers).State = EntityState.Modified;
                    if (obj.CustomerAddress != null)
                    {

                        obj.CustomerAddress.CustomerId = customers.CustomerId;
                        CustomerAddressHelper customerAddressHelper = new CustomerAddressHelper();
                        bool IsInserted = customerAddressHelper.InsertCustomerAddress(obj.CustomerAddress, customers.CustomerId);

                    }//customer tracking
                    //var custVerifies = db.CustomerLatLngVerify.Where(x => x.CustomerId == customers.CustomerId && x.AppType == (int)AppEnum.RetailerApp && x.Status == 1).ToList();
                    //if (custVerifies != null && custVerifies.Any())
                    //{
                    //    foreach (var item in custVerifies)
                    //    {
                    //        item.IsActive = false;
                    //        item.IsDeleted = true;
                    //        item.ModifiedBy = customers.CustomerId;
                    //        item.ModifiedDate = DateTime.Now;
                    //        db.Entry(item).State = EntityState.Modified;
                    //    }
                    //}
                    //var customerLatLngVerify = new CustomerLatLngVerify
                    //{
                    //    CaptureImagePath = customers.Shopimage,
                    //    CreatedBy = customers.CustomerId,
                    //    CreatedDate = DateTime.Now,
                    //    CustomerId = customers.CustomerId,
                    //    IsActive = true,
                    //    IsDeleted = false,
                    //    lat = customers.lat,
                    //    lg = customers.lg,
                    //    Newlat = customers.lat,
                    //    Newlg = customers.lg,
                    //    NewShippingAddress = customers.ShippingAddress,
                    //    ShippingAddress = oldshippingaddress,
                    //    ShopFound = 1,
                    //    ShopName = customers.ShopName,
                    //    Skcode = customers.Skcode,
                    //    LandMark = customers.LandMark,
                    //    Status = 1,
                    //    Nodocument = customers.CustomerDocTypeMasterId > 0 ? true : false,
                    //    Aerialdistance = 0,
                    //    AppType = (int)AppEnum.RetailerApp
                    //};
                    //db.CustomerLatLngVerify.Add(customerLatLngVerify);
                    //}
                    #region Device History
                    PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                    phonerecord.CustomerId = customers.CustomerId;
                    phonerecord.Name = customers.Name;
                    phonerecord.Skcode = customers.Skcode;
                    phonerecord.Mobile = customers.Mobile;
                    phonerecord.CurrentAPKversion = customers.CurrentAPKversion;
                    phonerecord.PhoneOSversion = customers.PhoneOSversion;
                    phonerecord.UserDeviceName = customers.UserDeviceName;
                    phonerecord.IMEI = customers.imei;//Sudhir to save device info
                    phonerecord.UpdatedDate = DateTime.Now;
                    db.PhoneRecordHistoryDB.Add(phonerecord);
                    db.Commit();

                    #endregion
                    #region CODLimitCustomers
                    var codLimit = db.CompanyDetailsDB.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false);
                    CODLimitCustomer codLimitCustomer = new CODLimitCustomer();
                    codLimitCustomer.CustomerId = customers.CustomerId;
                    codLimitCustomer.CODLimit = codLimit.CODLimit;
                    codLimitCustomer.IsActive = true;
                    codLimitCustomer.IsDeleted = false;
                    codLimitCustomer.IsCustomCODLimit = true;
                    codLimitCustomer.CreatedDate = DateTime.Now;
                    db.CODLimitCustomers.Add(codLimitCustomer);
                    db.Commit();
                    #endregion
                    res = new customerDetail()
                    {
                        customers = customers,
                        Status = true,
                        Message = "Customer Detail Updated."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    CustomersController.logger.Error("Error in add Customer " + ex.Message);
                    customerDetail res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "Somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        [Route("SignupUpdate")]
        [AcceptVerbs("PUT")]
        public CustUpdateDetails PutcustUpdate(RetailerAppDC cust)
        {
            using (AuthContext context = new AuthContext())
            {
                RetailerAppDC newcustomer = new RetailerAppDC();
                CustUpdateDetails res;
                //Customer custdata = context.CustomerUpdateV3(cust);
                try
                {
                    int WarehouseId = 0;

                    Customer customer = context.Customers.Where(c => c.CustomerId == cust.CustomerId && c.Deleted == false).SingleOrDefault();
                    if (customer.CustomerVerify == "Full Verified" || customer.CustomerVerify == "Partial Verified")
                    {
                        res = new CustUpdateDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "Since you are verified customer, You can't update your profile"
                        };
                        return res;
                    };

                    if (customer != null && (customer.CustomerDocumentStatus == (int)CustomerDocumentStatusEnum.Verified || customer.ShippingAddressStatus == (int)ShippingAddressStatusEnum.PhysicalVerified || customer.ShippingAddressStatus == (int)ShippingAddressStatusEnum.VirtualVerified))
                    {

                        res = new CustUpdateDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "Since your document are verified, You can't update your profile"
                        };
                        return res;

                    }

                    #region for Duplicate PAN no. check
                    if (!string.IsNullOrEmpty(cust.PanNo.Trim()))
                    {
                        Customer IsExistPAN = context.Customers.Where(c => c.PanNo.ToLower() == cust.PanNo.ToLower()).FirstOrDefault();
                        if (IsExistPAN != null)
                        {
                            res = new CustUpdateDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "PAN Already Exists."
                            };
                            return res;
                        }
                    }
                    #endregion

                    var city = context.Cities.FirstOrDefault(x => x.CityName.Trim().ToLower() == cust.City.Trim().ToLower() && x.active);

                    if (city != null)
                    {
                        cust.Cityid = city.Cityid;
                        customer.State = city.StateName;
                        WarehouseId = context.Warehouses.Where(x => x.Cityid == city.Cityid && x.Deleted == false && x.active == true).Select(x => x.WarehouseId).FirstOrDefault();
                    }
                    else
                    {
                        WarehouseId = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DefaultWarehouseId"].ToString());
                    }


                    bool TrackRequest = false;
                    if (customer.lat != cust.lat || customer.lg != cust.lg || customer.RefNo != cust.RefNo || customer.LicenseNumber != cust.LicenseNumber || customer.ShippingAddress != cust.ShippingAddress)
                        TrackRequest = true;


                    if (customer != null)
                    {
                        if (!string.IsNullOrEmpty(cust.RefNo))
                        {
                            var checkgst = context.Customers.Where(x => x.RefNo == cust.RefNo && x.CustomerId != cust.CustomerId).Count();
                            if (checkgst > 0)
                            {
                                res = new CustUpdateDetails()
                                {
                                    customers = newcustomer,
                                    Status = false,
                                    Message = "Gst Already Exsits."
                                };
                                return res;
                            }
                        }
                        if (!string.IsNullOrEmpty(cust.LicenseNumber.Trim()) && cust.LicenseNumber != "0")
                        {
                            var checkgst = context.Customers.Where(x => x.LicenseNumber == cust.LicenseNumber && x.CustomerId != cust.CustomerId).Count();
                            if (checkgst > 0)
                            {
                                res = new CustUpdateDetails()
                                {
                                    customers = null,
                                    Status = false,
                                    Message = "License Number Already Exsits."
                                };
                                return res;
                            }
                        }
                        if (cust.LicenseExpiryDate != null && cust.LicenseExpiryDate < DateTime.Now)
                        {

                            res = new CustUpdateDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "License Expiry Date Already Expired, Please enter valid date"
                            };
                            return res;
                        }

                        #region CustomerAddress status and lt lg :14/07/2022

                        if (cust.CustomerAddress != null)
                        {
                            customer.ShippingAddress = null;
                            if (!string.IsNullOrEmpty(cust.CustomerAddress.AddressLineOne))
                            {
                                customer.ShippingAddress = cust.CustomerAddress.AddressLineOne + ",";
                            }
                            if (!string.IsNullOrEmpty(cust.CustomerAddress.AddressLineTwo))
                            {
                                customer.ShippingAddress += cust.CustomerAddress.AddressLineTwo + ",";
                            }
                            if (!string.IsNullOrEmpty(cust.CustomerAddress.AddressText))
                            {
                                customer.ShippingAddress += cust.CustomerAddress.AddressText;
                            }
                            if (!string.IsNullOrEmpty(cust.CustomerAddress.ZipCode))
                            {
                                customer.ShippingAddress += "," + cust.CustomerAddress.ZipCode;
                            }
                            customer.lat = cust.CustomerAddress.AddressLat;
                            customer.lg = cust.CustomerAddress.AddressLng;

                            customer.Addresslat = cust.CustomerAddress.AddressLat;
                            customer.Addresslg = cust.CustomerAddress.AddressLng;

                        }
                        //LicenseExpiryDate
                        if (cust.CustomerDocTypeMasterId > 0 && cust.CustomerDocTypeMasterId == 1 && cust.LicenseExpiryDate != null)
                        {
                            customer.GstExpiryDate = cust.LicenseExpiryDate;
                        }
                        else if (cust.CustomerDocTypeMasterId > 1 && cust.LicenseExpiryDate != null)
                        {
                            customer.LicenseExpiryDate = cust.LicenseExpiryDate;
                        }
                        #endregion


                        customer.IsCityVerified = cust.Cityid > 0;
                        customer.City = cust.City;
                        customer.Cityid = cust.Cityid;
                        customer.ShippingCity = cust.City;

                        customer.CustomerId = cust.CustomerId;
                        customer.Name = cust.Name;
                        customer.AreaName = cust.AreaName;
                        customer.UploadProfilePichure = cust.UploadProfilePichure;
                        customer.Mobile = cust.Mobile;
                        customer.Emailid = cust.Emailid;
                        customer.ZipCode = cust.ZipCode;
                        customer.BillingAddress = cust.BillingAddress;
                        customer.BillingCity = cust.BillingCity;
                        customer.BillingState = string.IsNullOrEmpty(cust.BillingState) == true ? customer.BillingState : cust.BillingState;
                        customer.BillingZipCode = cust.BillingZipCode;
                        customer.Password = string.IsNullOrEmpty(cust.Password) == true ? customer.Password : cust.Password;
                        customer.Skcode = cust.Skcode;
                        customer.RefNo = cust.RefNo;
                        customer.UploadGSTPicture = cust.UploadGSTPicture;
                        customer.ShopName = cust.ShopName;
                        customer.Shopimage = cust.Shopimage;
                        if (!string.IsNullOrEmpty(cust.Shopimage) && cust.Shoplat.HasValue && cust.Shoplg.HasValue)
                        {
                            customer.Shoplat = cust.Shoplat;
                            customer.Shoplg = cust.Shoplg;
                        }
                        customer.CurrentAPKversion = cust.CurrentAPKversion;
                        customer.PhoneOSversion = cust.PhoneOSversion;
                        customer.UserDeviceName = cust.UserDeviceName;
                        customer.deviceId = cust.deviceId;
                        customer.imei = cust.imei;
                        customer.IsSignup = string.IsNullOrEmpty(cust.ShopName) == true ? false : true;
                        customer.ShippingAddress1 = cust.ShippingAddress1;
                        customer.LandMark = cust.LandMark;
                        customer.LicenseNumber = cust.LicenseNumber;
                        customer.UploadRegistration = cust.UploadRegistration;
                        customer.fcmId = !string.IsNullOrEmpty(cust.fcmId) ? cust.fcmId : customer.fcmId;
                        customer.CustomerAppType = 1;
                        customer.AadharNo = cust.AadharNo;//Sudhir 30-11-2020  
                        customer.PanNo = cust.PanNo;//Sudhir 30-11-2020  
                        customer.NameOnGST = cust.NameOnGST;
                        if (customer.Warehouseid == null && WarehouseId > 0)
                        {
                            customer.Warehouseid = WarehouseId;
                        }

                        #region to assign cluster ID and determine if it is in cluster or not.                   
                        if (customer.lat != 0 && customer.lg != 0 && customer.Cityid > 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLngAndCity]('").Append(customer.lat).Append("', '").Append(customer.lg).Append("', ").Append(customer.Cityid).Append(")");
                            var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                            if (!clusterId.HasValue)
                            {
                                customer.InRegion = false;
                                string message = "Thanks for registering with Direct. We will contact you soon";

                                var notificationSent = context.DeviceNotificationDb.Any(x => x.CustomerId == customer.CustomerId && x.Message == message);

                                if (!notificationSent)
                                {
                                    NotificationHelper notHelper = new NotificationHelper();
                                    string title = "Congratulations! ";

                                    notHelper.SendNotificationtoCustomer(customer, message, message, true, true, title);
                                }
                                customer.Warehouseid = null;
                                customer.WarehouseName = null;
                            }
                            else
                            {
                                customer.ClusterId = clusterId;
                                var dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                customer.ClusterName = dd.ClusterName;
                                customer.InRegion = true;
                                customer.Warehouseid = dd.WarehouseId;
                                customer.WarehouseName = dd.WarehouseName;
                            }
                        }
                        #endregion


                        if (!string.IsNullOrEmpty(customer.RefNo))
                        {
                            var custGstVerifys = context.CustGSTverifiedRequestDB.Where(x => x.RefNo == customer.RefNo).ToList();
                            if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                            {
                                var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                                var state = context.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                                customer.BillingCity = gstVerify.City;
                                customer.BillingState = state != null ? state.StateName : gstVerify.State;
                                customer.BillingZipCode = gstVerify.Zipcode;
                                customer.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                                customer.NameOnGST = gstVerify.Name;
                            }

                        }

                        if (string.IsNullOrEmpty(customer.BillingAddress))
                        {
                            customer.BillingCity = customer.City;
                            customer.BillingState = customer.State;
                            customer.BillingZipCode = customer.ZipCode;
                            customer.BillingAddress = customer.ShippingAddress;
                        }
                        if (!string.IsNullOrEmpty(cust.ReferralSkCode))
                        {
                            var customerReferralConfiguration = context.CustomerReferralConfigurationDb.Where(x => x.CityId == cust.Cityid && x.IsActive == true && x.IsDeleted == false && x.ReferralType == 1).ToList();
                            var referralWallet = context.ReferralWalletDb.Where(x => x.SkCode == cust.Skcode).FirstOrDefault();
                            if (customerReferralConfiguration != null && referralWallet == null)
                            {
                                foreach (var item in customerReferralConfiguration)
                                {
                                    context.ReferralWalletDb.Add(new Model.CustomerReferral.ReferralWallet
                                    {
                                        SkCode = customer.Skcode,
                                        ReferralSkCode = cust.ReferralSkCode,
                                        CreatedBy = customer.CustomerId,
                                        ReferralWalletPoint = item.ReferralWalletPoint,
                                        CustomerWalletPoint = item.CustomerWalletPoint,
                                        IsActive = true,
                                        CreatedDate = DateTime.Now,
                                        IsDeleted = false,
                                        ReferralType = Convert.ToInt32(ReferralType.Customer),
                                        OnOrder = item.OnOrder,
                                        CustomerReferralConfigurationId = item.Id,
                                        IsUsed = 0
                                    });
                                }
                            }
                        }
                        context.Entry(customer).State = EntityState.Modified;

                        //if (customer.Warehouseid > 0 && TrackRequest == true)
                        //{
                        #region CustomerAddress :12/07/2022
                        if (cust.CustomerAddress != null)
                        {
                            cust.CustomerAddress.CustomerId = customer.CustomerId;
                            CustomerAddressHelper customerAddressHelper = new CustomerAddressHelper();
                            bool IsInserted = customerAddressHelper.InsertCustomerAddress(cust.CustomerAddress, customer.CustomerId);
                        }
                        #endregion
                        //customer tracking
                        //var custVerifies = context.CustomerLatLngVerify.Where(x => x.CustomerId == customer.CustomerId && x.AppType == (int)AppEnum.RetailerApp && x.Status == 1 && x.IsActive).ToList();
                        //if (custVerifies != null && custVerifies.Any())
                        //{
                        //    foreach (var item in custVerifies)
                        //    {
                        //        item.IsActive = false;
                        //        item.IsDeleted = true;
                        //        item.ModifiedBy = customer.CustomerId;
                        //        item.ModifiedDate = DateTime.Now;
                        //        context.Entry(item).State = EntityState.Modified;
                        //    }
                        //}
                        //var customerLatLngVerify = new CustomerLatLngVerify
                        //{
                        //    CaptureImagePath = cust.Shopimage,
                        //    CreatedBy = customer.CustomerId,
                        //    CreatedDate = DateTime.Now,
                        //    CustomerId = customer.CustomerId,
                        //    IsActive = true,
                        //    IsDeleted = false,
                        //    lat = customer.lat,
                        //    lg = customer.lg,
                        //    Newlat = cust.lat,
                        //    Newlg = cust.lg,
                        //    NewShippingAddress = cust.ShippingAddress,
                        //    ShippingAddress = customer.ShippingAddress,
                        //    ShopFound = 1,
                        //    ShopName = cust.ShopName,
                        //    Skcode = customer.Skcode,
                        //    LandMark = cust.LandMark,
                        //    Status = 1,
                        //    Nodocument = false,
                        //    Aerialdistance = 0,
                        //    AppType = (int)AppEnum.RetailerApp
                        //};
                        //context.CustomerLatLngVerify.Add(customerLatLngVerify);

                        #region Channel

                        var CustomerChannelExists = context.CustomerChannelMappings.Where(x => x.CustomerId == customer.CustomerId && x.IsDeleted == false).FirstOrDefault();
                        if (CustomerChannelExists == null)
                        {
                            List<CustomerChannelMapping> customerChannels = new List<CustomerChannelMapping>();
                            var StoreIds = context.StoreDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).Distinct().ToList();
                            var channel = context.ChannelMasters.Where(x => x.ChannelType.ToLower() == "retail").Select(x => x.ChannelMasterId).FirstOrDefault();
                            if (StoreIds != null && StoreIds.Count > 0)
                            {
                                StoreIds.ForEach(x =>
                                {
                                    customerChannels.Add(new CustomerChannelMapping
                                    {
                                        CustomerId = customer.CustomerId,
                                        StoreId = (int)x,
                                        ChannelMasterId = channel,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedDate = DateTime.Now,
                                        CreatedBy = customer.CustomerId,
                                        ModifiedBy = null,
                                        ModifiedDate = null
                                    });
                                });
                            }
                            if (customerChannels != null && customerChannels.Count > 0)
                                context.CustomerChannelMappings.AddRange(customerChannels);
                        }
                        #endregion

                        context.Commit();
                        // }
                        if (((!string.IsNullOrEmpty(cust.LicenseNumber) && cust.CustomerDocTypeMasterId > 0) || (!string.IsNullOrEmpty(cust.RefNo) && !string.IsNullOrEmpty(cust.UploadGSTPicture))))
                        {
                            var customerdocs = context.CustomerDocs.Where(x => x.CustomerId == customer.CustomerId && x.IsActive).ToList();
                            var docMaster = context.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();
                            foreach (var custdoc in customerdocs)
                            {
                                custdoc.ModifiedBy = 0;
                                custdoc.ModifiedDate = DateTime.Now;
                                custdoc.IsActive = false;
                                context.Entry(custdoc).State = EntityState.Modified;
                            }
                            if (!string.IsNullOrEmpty(cust.RefNo) && !string.IsNullOrEmpty(cust.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                            {
                                var docid = docMaster.FirstOrDefault(x => x.DocType == "GST").Id;

                                context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = customer.CustomerId,
                                    IsActive = true,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = docid,
                                    DocPath = cust.UploadGSTPicture,
                                    IsDeleted = false
                                });
                            }

                            if (!string.IsNullOrEmpty(cust.LicenseNumber) && cust.CustomerDocTypeMasterId > 0)
                            {
                                context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = customer.CustomerId,
                                    IsActive = true,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = cust.CustomerDocTypeMasterId,
                                    DocPath = cust.UploadRegistration,
                                    IsDeleted = false
                                });
                            }

                            context.Commit();
                        }


                    }
                    newcustomer = Mapper.Map(customer).ToANew<RetailerAppDC>();
                    res = new CustUpdateDetails()
                    {
                        customers = newcustomer,
                        Status = true,
                        Message = "Customer updated."
                    };
                    return res;
                }
                catch (Exception ex)
                {
                    res = new CustUpdateDetails()
                    {
                        customers = null,
                        Status = false,
                        Message = "Somthing went wrong. " + ex,
                    };
                    return res;
                }
            }
        }

        [Route("SignupUpdateBasicInfo")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public CustUpdateDetails SignupUpdateBasicInfo(RetailerAppDC cust)
        {
            using (AuthContext context = new AuthContext())
            {
                RetailerAppDC newcustomer = new RetailerAppDC();
                CustUpdateDetails res;
                //Customer custdata = context.CustomerUpdateV3(cust);
                try
                {
                    int WarehouseId = 0;
                    ApkNamePwdResponse registeredApk = null;
                    Customer customer = context.Customers.Where(c => c.CustomerId == cust.CustomerId && c.Deleted == false).SingleOrDefault();

                    var city = context.Cities.FirstOrDefault(x => x.CityName.Trim().ToLower() == cust.City.Trim().ToLower());

                    if (city != null)
                    {
                        cust.Cityid = city.Cityid;
                        customer.State = city.StateName;
                        WarehouseId = context.Warehouses.Where(x => x.Cityid == city.Cityid && x.Deleted == false && x.active == true).Select(x => x.WarehouseId).FirstOrDefault();
                    }
                    else
                    {
                        WarehouseId = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["DefaultWarehouseId"].ToString());
                    }
                    if (customer != null)
                    {
                        //removed on 13/07/2022 by Harry
                        //if (customer.ShippingAddress != (string.IsNullOrEmpty(cust.ShippingAddress1) ? "" : cust.ShippingAddress1 + ", ") + cust.ShippingAddress)
                        //{
                        //    GeoHelper geoHelper = new GeoHelper();
                        //    decimal? lat, longitude;
                        //    geoHelper.GetLatLongWithZipCode(customer.ShippingAddress, customer.City, customer.ZipCode, out lat, out longitude);
                        //    customer.Addresslat = (double)lat;
                        //    customer.Addresslg = (double)longitude;
                        //    customer.Distance = 0;
                        //}

                        //if (customer.Addresslat.HasValue && customer.Addresslg.HasValue && customer.lat > 0 && customer.lg > 0)
                        //{
                        //    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(customer.Addresslat.Value, customer.Addresslg.Value);
                        //    var destination = new System.Device.Location.GeoCoordinate(customer.lat, customer.lg);
                        //    customer.Distance = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                        //}

                        customer.IsCityVerified = cust.Cityid > 0;
                        customer.City = cust.City;
                        customer.Cityid = cust.Cityid;
                        customer.ShippingCity = cust.City;
                        customer.lat = cust.lat;
                        customer.lg = cust.lg;
                        customer.CustomerId = cust.CustomerId;
                        customer.ShippingAddress = (string.IsNullOrEmpty(cust.ShippingAddress1) ? "" : cust.ShippingAddress1 + ", ") + cust.ShippingAddress;
                        customer.ZipCode = cust.ZipCode;
                        customer.ShopName = cust.ShopName;
                        customer.CurrentAPKversion = cust.CurrentAPKversion;
                        customer.PhoneOSversion = cust.PhoneOSversion;
                        customer.UserDeviceName = cust.UserDeviceName;
                        customer.deviceId = cust.deviceId;
                        customer.imei = cust.imei;
                        customer.IsSignup = customer.CustomerType.ToLower() != "consumer" ? true : false;
                        customer.ShippingAddress1 = cust.ShippingAddress1;
                        customer.BillingAddress = customer.BillingAddress;
                        customer.BillingAddress1 = customer.ShippingAddress1;
                        customer.LandMark = cust.LandMark;
                        customer.fcmId = !string.IsNullOrEmpty(cust.fcmId) ? cust.fcmId : customer.fcmId;
                        customer.CustomerAppType = 1;
                        customer.Name = customer.CustomerType.ToLower() == "consumer" ? cust.Name : "";
                        registeredApk = context.GetAPKUserAndPwd("RetailersApp");

                        if (customer.Warehouseid == null && WarehouseId > 0)
                        {
                            customer.Warehouseid = WarehouseId;
                        }

                        #region to assign cluster ID and determine if it is in cluster or not.                   
                        if (customer.lat != 0 && customer.lg != 0 && customer.Cityid > 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLngAndCity]('").Append(customer.lat).Append("', '").Append(customer.lg).Append("', ").Append(customer.Cityid).Append(")");
                            var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                            if (!clusterId.HasValue)
                            {
                                customer.InRegion = false;
                                string message = "Thanks for registering with Direct. We will contact you soon";

                                var notificationSent = context.DeviceNotificationDb.Any(x => x.CustomerId == customer.CustomerId && x.Message == message);

                                if (!notificationSent)
                                {
                                    NotificationHelper notHelper = new NotificationHelper();
                                    string title = "Congratulations! ";

                                    notHelper.SendNotificationtoCustomer(customer, message, message, true, true, title);
                                }
                            }
                            else
                            {
                                customer.ClusterId = clusterId;
                                var dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                customer.ClusterName = dd.ClusterName;
                                customer.InRegion = true;
                                customer.Warehouseid = dd.WarehouseId;
                                customer.WarehouseName = dd.WarehouseName;
                            }
                        }
                        #endregion


                        context.Entry(customer).State = EntityState.Modified;
                        context.Commit();

                    }
                    newcustomer = Mapper.Map(customer).ToANew<RetailerAppDC>();
                    newcustomer.APKType = registeredApk;
                    res = new CustUpdateDetails()
                    {
                        customers = newcustomer,
                        Status = true,
                        Message = "Customer updated."
                    };
                    return res;
                }
                catch (Exception ex)
                {
                    res = new CustUpdateDetails()
                    {
                        customers = null,
                        Status = false,
                        Message = "Somthing went wrong. " + ex,
                    };
                    return res;
                }
            }
        }


        [Route("RetailerUpdateCustomerV2")]
        [HttpPut]
        public async Task<CustUpdateDetails> PutcV2(Customer cust)
        {
            using (AuthContext context = new AuthContext())
            {
                RetailerAppDC Updatecustomer = new RetailerAppDC();
                CustUpdateDetails res;

                var customerVerify = context.Customers.FirstOrDefault(x => x.Mobile == cust.Mobile);
                if (customerVerify.CustomerVerify == "Full Verified" || customerVerify.CustomerVerify == "Partial Verified")
                {
                    res = new CustUpdateDetails()
                    {
                        customers = null,
                        Status = false,
                        Message = "Since you are verified customer, You can't update your profile"
                    };
                    return res;
                }
                if (customerVerify != null && (customerVerify.CustomerDocumentStatus == (int)CustomerDocumentStatusEnum.Verified || customerVerify.ShippingAddressStatus == (int)ShippingAddressStatusEnum.PhysicalVerified || customerVerify.ShippingAddressStatus == (int)ShippingAddressStatusEnum.VirtualVerified))
                {

                    res = new CustUpdateDetails()
                    {
                        customers = null,
                        Status = false,
                        Message = "Since your document are verified, You can't update your profile"
                    };
                    return res;

                }

                if (customerVerify.CustomerVerify != "Full Verified" && customerVerify.CustomerVerify != "Address Not Found")
                {
                    if (!string.IsNullOrEmpty(cust.RefNo))
                    {
                        var checkgst = context.Customers.Where(x => x.RefNo == cust.RefNo && x.CustomerId != customerVerify.CustomerId).Count();
                        if (checkgst > 0)
                        {
                            res = new CustUpdateDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "Gst Already Exsits."
                            };
                            return res;
                        }
                    }

                    if (!string.IsNullOrEmpty(cust.LicenseNumber.Trim()) && cust.LicenseNumber != "0")
                    {
                        var checkgst = context.Customers.Where(x => x.LicenseNumber == cust.LicenseNumber && x.CustomerId != cust.CustomerId).Count();
                        if (checkgst > 0)
                        {
                            res = new CustUpdateDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "License Number Already Exsits."
                            };
                            return res;
                        }
                    }

                    #region for Duplicate PAN no. check
                    if (cust != null && !string.IsNullOrEmpty(cust.PanNo.Trim()))
                    {
                        Customer IsExistPAN = context.Customers.Where(c => c.PanNo.ToLower() == cust.PanNo.ToLower()).FirstOrDefault();

                        if (IsExistPAN != null && cust.CustomerId != IsExistPAN.CustomerId)
                        {
                            res = new CustUpdateDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "PAN Already Exists."
                            };
                            return res;
                        }
                    }
                    #endregion

                    Customer custdata = customerVerify;
                    bool TrackRequest = false;
                    if (custdata != null)
                    {
                        if (custdata.lat != cust.lat || custdata.lg != cust.lg || custdata.RefNo != cust.RefNo || custdata.LicenseNumber != cust.LicenseNumber || custdata.ShippingAddress != cust.ShippingAddress)
                            TrackRequest = true;
                        //removed on 13/07/2022 by Harry
                        //if (custdata.ShippingAddress != cust.ShippingAddress)
                        //{
                        //    GeoHelper geoHelper = new GeoHelper();
                        //    decimal? lat, longitude;
                        //    geoHelper.GetLatLongWithZipCode(cust.ShippingAddress, cust.City, cust.ZipCode, out lat, out longitude);
                        //    custdata.Addresslat = (double)lat;
                        //    custdata.Addresslg = (double)longitude;
                        //    custdata.Distance = 0;
                        //}
                        //if (custdata.Addresslat.HasValue && custdata.Addresslg.HasValue && cust.lat > 0 && cust.lg > 0)
                        //{
                        //    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(custdata.Addresslat.Value, custdata.Addresslg.Value);
                        //    var destination = new System.Device.Location.GeoCoordinate(cust.lat, cust.lg);
                        //    custdata.Distance = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                        //}
                        #region CustomerAddress status and lt lg :14/07/2022

                        if (cust.CustomerAddress != null)
                        {
                            custdata.ShippingAddress = null;
                            if (!string.IsNullOrEmpty(cust.CustomerAddress.AddressLineOne))
                            {
                                custdata.ShippingAddress += cust.CustomerAddress.AddressLineOne + ",";
                            }
                            if (!string.IsNullOrEmpty(cust.CustomerAddress.AddressLineTwo))
                            {
                                custdata.ShippingAddress += cust.CustomerAddress.AddressLineTwo + ",";
                            }
                            if (!string.IsNullOrEmpty(cust.CustomerAddress.AddressText))
                            {
                                custdata.ShippingAddress += cust.CustomerAddress.AddressText;
                            }
                            if (!string.IsNullOrEmpty(cust.CustomerAddress.ZipCode))
                            {
                                custdata.ShippingAddress += "," + cust.CustomerAddress.ZipCode;
                            }
                            custdata.lat = cust.CustomerAddress.AddressLat;
                            custdata.lg = cust.CustomerAddress.AddressLng;

                            custdata.Addresslat = cust.CustomerAddress.AddressLat;
                            custdata.Addresslg = cust.CustomerAddress.AddressLng;

                        }
                        //LicenseExpiryDate
                        if (cust.CustomerDocTypeMasterId > 0 && cust.CustomerDocTypeMasterId == 1 && cust.LicenseExpiryDate != null)
                        {
                            custdata.GstExpiryDate = cust.LicenseExpiryDate;
                        }
                        else if (cust.CustomerDocTypeMasterId > 1 && cust.LicenseExpiryDate != null)
                        {
                            custdata.LicenseExpiryDate = cust.LicenseExpiryDate;
                        }
                        #endregion
                        custdata.Emailid = cust.Emailid;
                        //custdata.lat = cust.lat;
                        //custdata.lg = cust.lg;
                        custdata.Name = cust.Name;
                        custdata.ShopName = cust.ShopName;
                        custdata.IsSignup = string.IsNullOrEmpty(cust.ShopName) == true ? false : true;
                        custdata.RefNo = cust.RefNo;
                        custdata.DOB = cust.DOB;
                        custdata.AnniversaryDate = cust.AnniversaryDate;     //tejas 25-05-2019
                        custdata.WhatsappNumber = cust.WhatsappNumber;      //tejas 25-05-2019
                        custdata.UploadLicensePicture = cust.UploadLicensePicture;  //tejas 04-06-2019 
                        custdata.UploadGSTPicture = cust.UploadGSTPicture;  //tejas 04-06-2019  
                        custdata.LicenseNumber = cust.LicenseNumber;      //tejas 04-06-2019  
                        custdata.Shopimage = cust.Shopimage;                //tejas 04-06-2019  
                        if (!string.IsNullOrEmpty(cust.Shopimage) && cust.Shoplat.HasValue && cust.Shoplg.HasValue && cust.Shoplat > 0 && cust.Shoplg > 0)
                        {
                            custdata.Shoplat = cust.Shoplat;
                            custdata.Shoplg = cust.Shoplg;
                        }
                        custdata.AreaName = cust.AreaName;
                        custdata.BillingAddress = cust.BillingAddress;
                        custdata.UploadRegistration = cust.UploadRegistration;
                        custdata.UploadProfilePichure = cust.UploadProfilePichure;
                        custdata.ResidenceAddressProof = cust.ResidenceAddressProof;
                        custdata.ShippingAddress1 = cust.ShippingAddress1;
                        custdata.BillingAddress = cust.BillingAddress;
                        custdata.ZipCode = cust.ZipCode;
                        custdata.LandMark = cust.LandMark;
                        custdata.AadharNo = cust.AadharNo;//sudhir 30-11-2020 
                        custdata.PanNo = cust.PanNo;//sudhir 30-11-2020 
                        custdata.NameOnGST = cust.NameOnGST;
                        custdata.BillingCity = cust.BillingCity;
                        custdata.BillingState = string.IsNullOrEmpty(cust.BillingState) == true ? custdata.BillingState : cust.BillingState;
                        custdata.BillingZipCode = cust.BillingZipCode;
                        if (string.IsNullOrEmpty(custdata.ShippingCity))
                            custdata.ShippingCity = custdata.City;
                        //Customers.Attach(customer);


                        #region to assign cluster ID and determine if it is in cluster or not.                   
                        if (custdata.lat != 0 && custdata.lg != 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(custdata.lat).Append("', '").Append(custdata.lg).Append("')");
                            var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                            if (!clusterId.HasValue)
                            {
                                custdata.ClusterId = 0;
                                custdata.ClusterName = "Out of Cluster.";
                                custdata.Warehouseid = 0;
                                custdata.WarehouseName = "";

                                custdata.InRegion = false;
                                string message = "Thanks for registering with ShopKirana. We will contact you soon";

                                var notificationSent = context.DeviceNotificationDb.Any(x => x.CustomerId == custdata.CustomerId && x.Message == message);

                                if (!notificationSent)
                                {
                                    NotificationHelper notHelper = new NotificationHelper();
                                    string title = "Congratulations! ";

                                    notHelper.SendNotificationtoCustomer(custdata, message, message, true, true, title);
                                }
                            }
                            else
                            {
                                custdata.ClusterId = clusterId;
                                var dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                custdata.ClusterName = dd.ClusterName;
                                custdata.InRegion = true;
                                custdata.Warehouseid = dd.WarehouseId;
                                custdata.WarehouseName = dd.WarehouseName;
                            }
                        }
                        #endregion

                        if (!string.IsNullOrEmpty(custdata.RefNo))
                        {
                            var custGstVerifys = context.CustGSTverifiedRequestDB.Where(x => x.RefNo == custdata.RefNo).ToList();
                            if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                            {
                                var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                                var state = context.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                                custdata.BillingCity = gstVerify.City;
                                custdata.BillingState = state != null ? state.StateName : gstVerify.State;
                                custdata.BillingZipCode = gstVerify.Zipcode;
                                custdata.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                                custdata.NameOnGST = gstVerify.Name;
                            }

                        }

                        if (string.IsNullOrEmpty(custdata.BillingAddress))
                        {
                            custdata.BillingCity = custdata.City;
                            custdata.BillingState = custdata.State;
                            custdata.BillingZipCode = custdata.ZipCode;
                            custdata.BillingAddress = custdata.ShippingAddress;
                            custdata.BillingAddress1 = custdata.AreaName;

                        }
                        context.Entry(custdata).State = EntityState.Modified;


                        //customer tracking
                        if (TrackRequest)
                        {
                            //var custVerifies = context.CustomerLatLngVerify.Where(x => x.CustomerId == custdata.CustomerId && x.AppType == (int)AppEnum.RetailerApp && x.Status == 1).ToList();
                            //if (custVerifies != null && custVerifies.Any())
                            //{
                            //    foreach (var item in custVerifies)
                            //    {
                            //        item.IsActive = false;
                            //        item.IsDeleted = true;
                            //        item.ModifiedBy = custdata.CustomerId;
                            //        item.ModifiedDate = DateTime.Now;
                            //        context.Entry(item).State = EntityState.Modified;
                            //    }
                            //}
                            //var customerLatLngVerify = new CustomerLatLngVerify
                            //{
                            //    CaptureImagePath = cust.Shopimage,
                            //    CreatedBy = custdata.CustomerId,
                            //    CreatedDate = DateTime.Now,
                            //    CustomerId = custdata.CustomerId,
                            //    IsActive = true,
                            //    IsDeleted = false,
                            //    lat = custdata.lat,
                            //    lg = custdata.lg,
                            //    Newlat = cust.lat,
                            //    Newlg = cust.lg,
                            //    NewShippingAddress = cust.ShippingAddress,
                            //    ShippingAddress = custdata.ShippingAddress,
                            //    ShopFound = 1,
                            //    ShopName = cust.ShopName,
                            //    Skcode = custdata.Skcode,
                            //    LandMark = cust.LandMark,
                            //    Status = 1,
                            //    Nodocument = custdata.CustomerDocTypeMasterId > 0 ? true : false,
                            //    Aerialdistance = 0,
                            //    AppType = (int)AppEnum.RetailerApp
                            //};
                            //context.CustomerLatLngVerify.Add(customerLatLngVerify);
                            #region CustomerAddress :12/07/2022
                            if (cust.CustomerAddress != null)
                            {
                                cust.CustomerAddress.CustomerId = custdata.CustomerId;
                                CustomerAddressHelper customerAddressHelper = new CustomerAddressHelper();
                                bool IsInserted = customerAddressHelper.InsertCustomerAddress(cust.CustomerAddress, custdata.CustomerId);
                            }
                            #endregion
                        }
                        if (context.Commit() > 0 && ((!string.IsNullOrEmpty(cust.LicenseNumber) && cust.CustomerDocTypeMasterId > 0) || (!string.IsNullOrEmpty(cust.RefNo) && !string.IsNullOrEmpty(cust.UploadGSTPicture))))
                        {
                            var customerdocs = context.CustomerDocs.Where(x => x.CustomerId == custdata.CustomerId).ToList();
                            var docMaster = context.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();
                            foreach (var custdoc in customerdocs)
                            {
                                custdoc.ModifiedBy = 0;
                                custdoc.ModifiedDate = DateTime.Now;
                                custdoc.IsActive = false;
                                context.Entry(custdoc).State = EntityState.Modified;
                            }
                            if (!string.IsNullOrEmpty(cust.RefNo) && !string.IsNullOrEmpty(cust.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                            {
                                var docid = docMaster.FirstOrDefault(x => x.DocType == "GST").Id;
                                //if (customerdocs.Any(x => x.CustomerDocTypeMasterId == docid && x.IsActive))
                                //{
                                //    var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == docid && x.IsActive);
                                //    custdoc.ModifiedBy = 0;
                                //    custdoc.ModifiedDate = DateTime.Now;
                                //    custdoc.IsActive = false;
                                //    context.Entry(custdoc).State = EntityState.Modified;
                                //}
                                context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = custdata.CustomerId,
                                    IsActive = true,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = docid,
                                    DocPath = cust.UploadGSTPicture,
                                    IsDeleted = false
                                });
                            }

                            if (!string.IsNullOrEmpty(cust.LicenseNumber) && cust.CustomerDocTypeMasterId > 0)
                            {
                                //if (customerdocs.Any(x => x.CustomerDocTypeMasterId == cust.CustomerDocTypeMasterId && x.IsActive))
                                //{
                                //    var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == cust.CustomerDocTypeMasterId && x.IsActive);
                                //    custdoc.ModifiedBy = 0;
                                //    custdoc.ModifiedDate = DateTime.Now;
                                //    custdoc.IsActive = false;
                                //    context.Entry(custdoc).State = EntityState.Modified;
                                //}
                                context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = custdata.CustomerId,
                                    IsActive = true,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = cust.CustomerDocTypeMasterId,
                                    DocPath = cust.UploadRegistration,
                                    IsDeleted = false
                                });
                            }

                            context.Commit();
                        }

                    }
                    Updatecustomer = Mapper.Map(custdata).ToANew<RetailerAppDC>();

                    if (Updatecustomer != null)
                    {
                        //var mobile = new SqlParameter("@mobile", cust.Mobile);
                        //try
                        //{
                        //    context.Database.ExecuteSqlCommand("UpdateTkCustomerLtLng @mobile", mobile);
                        //}
                        //catch (Exception cus) { }

                        res = new CustUpdateDetails()
                        {
                            customers = Updatecustomer,
                            Status = true,
                            Message = "Customer updated."
                        };
                        return res;

                    }
                    else
                    {
                        res = new CustUpdateDetails()
                        {
                            customers = null,
                            Status = true,
                            Message = "Somthing went wrong"
                        };
                        return res;

                    }
                }
                else
                {
                    Customer custdata = customerVerify;
                    if (custdata != null)
                    {

                        custdata.Emailid = cust.Emailid;
                        custdata.DOB = cust.DOB;
                        custdata.AnniversaryDate = cust.AnniversaryDate;
                        custdata.WhatsappNumber = cust.WhatsappNumber;
                        //custdata.UploadLicensePicture = cust.UploadLicensePicture;
                        //custdata.LicenseNumber = cust.LicenseNumber;
                        //custdata.Shopimage = cust.Shopimage;
                        //custdata.UploadRegistration = cust.UploadRegistration;
                        custdata.UploadProfilePichure = cust.UploadProfilePichure;
                        custdata.ResidenceAddressProof = cust.ResidenceAddressProof;
                        context.Entry(custdata).State = EntityState.Modified;
                        context.Commit();

                    }

                    Updatecustomer = Mapper.Map(custdata).ToANew<RetailerAppDC>();
                    if (Updatecustomer != null)
                    {
                        //var mobile = new SqlParameter("@mobile", cust.Mobile);
                        //try
                        //{
                        //    context.Database.ExecuteSqlCommand("UpdateTkCustomerLtLng @mobile", mobile);
                        //}
                        //catch (Exception cus) { }

                        res = new CustUpdateDetails()
                        {
                            customers = Updatecustomer,
                            Status = true,
                            Message = "Customer updated."
                        };
                        return res;

                    }
                    else
                    {
                        res = new CustUpdateDetails()
                        {
                            customers = null,
                            Status = true,
                            Message = "Somthing went wrong"
                        };
                        return res;

                    }
                }
            }

        }

        [Route("GetCustomerDocType")]
        [HttpGet]
        public async Task<dynamic> GetCustomerDocType(int warehouseId, int customerId)
        {
            using (AuthContext db = new AuthContext())
            {
                var CustomerDocTypes = await db.CustomerDocTypeMasters.Where(x => x.IsActive).ToListAsync();
                return Request.CreateResponse(HttpStatusCode.OK, CustomerDocTypes);
            }
        }

        [Route("Customerprofile")]
        [HttpGet]

        public async Task<CustDetails> Getmyprofile(int customerid, string deviceId)
        {

            MongoDbHelper<UdharOverDueDayValidation> UdharOverDueDay = new MongoDbHelper<UdharOverDueDayValidation>();
            MongoDbHelper<SKSocial> SKSocial = new MongoDbHelper<SKSocial>();
            var DueAmt = UdharOverDueDay.GetAll();
            var minDay = 0;
            if (DueAmt != null && DueAmt.Any())
            {
                minDay = DueAmt.Min(x => x.MinOverDueDay);
            }
            CheckDueAmtDc UDData = new CheckDueAmtDc();

            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            using (var dbContext = new AuthContext())
            {
                try
                {
                    RetailerAppDC newcustomer = new RetailerAppDC();
                    CustDetails res;

                    var checkDeviceId = dbContext.Peoples.Any(x => x.DeviceId == deviceId);
                    if (checkDeviceId)
                    {

                        res = new CustDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "You are not Authorized."
                        };
                        return res;

                    }

                    Customer customers = dbContext.Customers.FirstOrDefault(a => a.CustomerId == customerid);
                    if (customers != null && customers.CustomerId > 0 && customers.UdharDueDays > 0)
                    {
                        var param1 = new SqlParameter("@CustomerId", customers.CustomerId);
                        UDData = dbContext.Database.SqlQuery<CheckDueAmtDc>("Exec CheckDueAmt @CustomerId ", param1).FirstOrDefault();
                    }

                    if (customers != null && customers.Deleted == false)
                    {
                        var custDocs = dbContext.CustomerDocs.Where(x => x.CustomerId == customers.CustomerId && x.IsActive).ToList();
                        var gstDocttypeid = dbContext.CustomerDocTypeMasters.FirstOrDefault(x => x.IsActive && x.DocType == "GST")?.Id;
                        if (gstDocttypeid.HasValue && custDocs.Any(y => y.CustomerDocTypeMasterId != gstDocttypeid.Value))
                        {
                            customers.CustomerDocTypeMasterId = custDocs.FirstOrDefault(y => y.CustomerDocTypeMasterId != gstDocttypeid.Value).CustomerDocTypeMasterId;
                        }
                    }
                    else if (customers != null && customers.Deleted)
                    {
                        res = new CustDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "Your account is inactive please contact customer care."
                        };
                        return res;
                    }



                    newcustomer = Mapper.Map(customers).ToANew<RetailerAppDC>();
                    if (customers == null)
                    {
                        res = new CustDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer not exist."
                        };
                        return res;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            var deviceLogins = dbContext.CustomerLoginDeviceDB.Where(x => x.CustomerId == customerid).ToList();
                            if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == deviceId && !x.IsCurrentLogin))
                            {
                                res = new CustDetails()
                                {
                                    customers = newcustomer,
                                    Status = true,
                                    Message = "Log-Out from this device",
                                    LogOutFromThisDevice = true
                                };
                                return res;
                            }

                        }
                    }

                    bool Ainfo = true, Binfo = true;
                    string result = "";
                    if (customers.lat == 0 || customers.lg == 0)
                    {
                        Ainfo = false;
                    }
                    if (string.IsNullOrEmpty(customers.RefNo) || (string.IsNullOrEmpty(customers.UploadGSTPicture) || string.IsNullOrEmpty(customers.UploadRegistration)))
                    {
                        Binfo = false;
                    }

                    if (!Ainfo || !Binfo)
                        result = "Your Critical info " + (!Ainfo ? "GPS" : "") + (!Ainfo && !Binfo ? " & " : "") + (!Binfo ? " Shop Licence/GST# " : "") + " is Missing. Your account can be blocked anytime.";
                    var primeCustomer = dbContext.PrimeCustomers.FirstOrDefault(x => x.CustomerId == newcustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                    var todayDate = DateTime.Now.Date;
                    var customerLocationlst = dbContext.CustomerLocations.Where(x => x.CustomerId == customerid && !x.IsDeleted).ToList();
                    if (!customerLocationlst.Any() || customerLocationlst.Count < 10 && (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 19) && customerLocationlst.Max(x => x.CreatedDate).Date != todayDate)
                    {
                        newcustomer.IsRequiredLocation = true;
                    }

                    if (primeCustomer != null)
                    {
                        newcustomer.IsPrimeCustomer = primeCustomer.StartDate <= indianTime && primeCustomer.EndDate >= indianTime;
                        newcustomer.PrimeStartDate = primeCustomer.StartDate;
                        newcustomer.PrimeEndDate = primeCustomer.EndDate;
                    }

                    newcustomer.IsSelleravailable = (customers.Cityid.HasValue && customers.Cityid.Value > 0) ? SKSocial.Count(x => x.CityId == customers.Cityid) > 0 : true;

                    if (newcustomer.Warehouseid > 0)
                    {
                        newcustomer.IsWarehouseLive = dbContext.GMWarehouseProgressDB.Any(x => x.WarehouseID == newcustomer.Warehouseid && x.IsLaunched);

                    }
                    if (!newcustomer.ClusterId.HasValue || newcustomer.ClusterId.Value == 0)
                    {
                        newcustomer.IsSelleravailable = true;
                    }

                    res = new CustDetails()
                    {
                        customers = newcustomer,
                        Status = true,
                        Message = "Success.",
                        CriticalInfoMissingMsg = result,
                        IsUdharOverDue = ((customers != null && DueAmt.Any(x => x.MinOverDueDay > 0) && customers.UdharDueDays > minDay && UDData != null && UDData.Amount >= 1) ? true : false)
                    };

                    return res;
                }

                catch (Exception ex)
                {
                    return null;
                }
            }

        }


        [Route("GetCompanyDetailsForRetailerWithToken")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<companydetails> GetCompanyDetailsForRetailerWithToken(int customerId)
        {
            companydetails res;
            int WarehouseId = 0, cityId = 0;
            bool IsKpp = false;
            bool IsConsumer = false;
            bool IsSalesMan = false;
            bool IsActive = true;
            string FinBoxApiClientKey = ConfigurationManager.AppSettings["FinBoxClientKey"].ToString();
            long LogDboyLoctionMeter = Common.Constants.AppConstants.LogDboyLoctionMeter;

            using (AuthContext db = new AuthContext())
            {
                var companydetails = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (companydetails != null)
                {
                    var query = "select  Operation.IsNewDeliveryAppOnCluster(" + customerId + ") ";

                    companydetails.ShowOrderTracking = db.Database.SqlQuery<bool>(query).FirstOrDefault();

                    var customer = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    IsActive = customer.Active;
                    IsKpp = customer.IsKPP;
                    IsConsumer = customer != null && customer.CustomerType == "Consumer" ? true : false;
                    WarehouseId = customer.Warehouseid.HasValue ? customer.Warehouseid.Value : 0;
                    if (WarehouseId > 0)
                    {
                        cityId = db.Warehouses.FirstOrDefault(x => x.WarehouseId == WarehouseId).Cityid;
                    }


                    if (WarehouseId > 0)
                    {
                        MongoDbHelper<SalesAppDefaultCustomers> SalesAppmongoDbHelper = new MongoDbHelper<SalesAppDefaultCustomers>();
                        var defaultCustomer = SalesAppmongoDbHelper.Select(x => x.WarehouseId == WarehouseId).FirstOrDefault();
                        if (defaultCustomer != null)
                        {
                            companydetails.DefaultSalesSCcustomerId = defaultCustomer.CustomerId;
                        }
                        MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.WalletHundredPercentUse> mongoDbHelper_W = new MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.WalletHundredPercentUse>(); //!x.GeneratedOrderId.HasValue
                        var WalletHPer = mongoDbHelper_W.Select(x => x.WarehouseId == WarehouseId).FirstOrDefault();
                        companydetails.MaxWalletPointUsed = WalletHPer != null ? 1000000 : companydetails.MaxWalletPointUsed;
                    }
                    if (cityId > 0)
                    {
                        MongoDbHelper<ExtandedCompanyDetail> mongoDbHelper = new MongoDbHelper<ExtandedCompanyDetail>();
                        var extandedCompanyDetail = mongoDbHelper.Select(x => x.CityId == cityId && x.AppType == "Retailer").FirstOrDefault();

                        if (extandedCompanyDetail != null)
                        {
                            companydetails.IsShowCreditOption = extandedCompanyDetail.IsShowCreditOption;
                            companydetails.IsOnlinePayment = extandedCompanyDetail.IsOnlinePayment;
                            companydetails.ischeckBookShow = extandedCompanyDetail.ischeckBookShow;
                            companydetails.IsRazorpayEnable = extandedCompanyDetail.IsRazorpayEnable;
                            companydetails.IsePayLaterShow = extandedCompanyDetail.IsePayLaterShow;
                            companydetails.IsFinBox = extandedCompanyDetail.IsFinBox;
                            companydetails.IsCreditLineShow = extandedCompanyDetail.IsCreditLineShow;
                        }
                    }

                    companydetails.FinboxclientApikey = FinBoxApiClientKey;

                    MongoDbHelper<VATMCustomers> VATMCustomershelper = new MongoDbHelper<VATMCustomers>();
                    var vATMCustomer = VATMCustomershelper.Select(x => x.CustomerId == customerId && x.IsActive).FirstOrDefault();

                    if (vATMCustomer != null && !string.IsNullOrEmpty(vATMCustomer.Data))
                    {
                        companydetails.IsShowVATM = true;
                    }
                    if (IsKpp)
                    {

                        double IsKPPMaxWalletPointUsed = Convert.ToDouble(ConfigurationManager.AppSettings["IsKPPMaxWalletPointUsed"]);
                        if (IsKPPMaxWalletPointUsed > 0)
                        {
                            companydetails.MaxWalletPointUsed = IsKPPMaxWalletPointUsed;
                        }
                        else
                        {
                            companydetails.MaxWalletPointUsed = 8000;
                        }
                        companydetails.ischeckBookShow = false;
                        companydetails.IsShowCreditOption = false;
                        companydetails.IsePayLaterShow = false;

                    }

                    if (IsConsumer)
                    {

                        double ConsumerMaxWalletPointUsed = Convert.ToDouble(ConfigurationManager.AppSettings["ConsumerMaxWalletPointUsed"]);
                        if (ConsumerMaxWalletPointUsed > 0)
                        {
                            companydetails.MaxWalletPointUsed = ConsumerMaxWalletPointUsed;
                        }

                        companydetails.ischeckBookShow = false;
                        companydetails.IsShowCreditOption = false;
                        companydetails.IsePayLaterShow = false;

                    }

                    if (!IsActive)
                    {
                        companydetails.IsShowCreditOption = false;
                        companydetails.IsOnlinePayment = false;
                        companydetails.ischeckBookShow = false;
                        companydetails.IsRazorpayEnable = false;
                        companydetails.IsePayLaterShow = false;
                        companydetails.IsFinBox = false;
                        companydetails.IsCreditLineShow = false;

                    }
                    if (LogDboyLoctionMeter > 0)
                    {
                        companydetails.LogDboyLoctionMeter = LogDboyLoctionMeter;
                    }
                    if (IsSalesMan)
                    {
                        companydetails.MaxWalletPointUsed = 0;
                    }


                    res = new companydetails
                    {
                        CompanyDetails = companydetails,
                        Status = true,
                        Message = "GetCompanyDetailsForRetailer"
                    };
                    return res;
                }
                else
                {
                    res = new companydetails
                    {
                        Status = false,
                        Message = "Something went Wrong"
                    };
                    return res;
                }
            }
        }

        [Route("GetCompanyDetailsForRetailer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<companydetails> GetCompanyDetailsForRetailer()
        {
            string FinBoxApiClientKey = ConfigurationManager.AppSettings["FinBoxClientKey"].ToString();
            long LogDboyLoctionMeter = Common.Constants.AppConstants.LogDboyLoctionMeter;
            var loggedInUser = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null ? HttpContext.Current.User.Identity.Name : "System";
            int customerId = 0, WarehouseId = 0, cityId = 0;
            bool IsKpp = false;
            bool IsSalesMan = false;

            if ((string.IsNullOrEmpty(loggedInUser) || loggedInUser == "RetailerApp")
                && HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers.AllKeys.Any(x => x == "username"))
            {
                loggedInUser = Convert.ToString(HttpContext.Current.Request.Headers.GetValues("username").FirstOrDefault());

                customerId = loggedInUser.Split('_').Length > 1 ? Convert.ToInt32(loggedInUser.Split('_')[1]) : 0;
            }
            else
            {
                IsSalesMan = true;
            }
            companydetails res;
            bool IsActive = true;
            using (AuthContext db = new AuthContext())
            {
                if (customerId > 0)
                {
                    var customer = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    IsActive = customer.Active;
                    IsKpp = customer.IsKPP;
                    WarehouseId = customer.Warehouseid.HasValue ? customer.Warehouseid.Value : 0;
                    if (WarehouseId > 0)
                    {
                        cityId = db.Warehouses.FirstOrDefault(x => x.WarehouseId == WarehouseId).Cityid;
                    }
                }
                var companydetails = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (companydetails != null)
                {
                    var query = "select  Operation.IsNewDeliveryAppOnCluster(" + customerId + ") ";

                    companydetails.ShowOrderTracking = db.Database.SqlQuery<bool>(query).FirstOrDefault();

                    if (WarehouseId > 0)
                    {
                        MongoDbHelper<SalesAppDefaultCustomers> SalesAppmongoDbHelper = new MongoDbHelper<SalesAppDefaultCustomers>();
                        var defaultCustomer = SalesAppmongoDbHelper.Select(x => x.WarehouseId == WarehouseId).FirstOrDefault();
                        if (defaultCustomer != null)
                        {
                            companydetails.DefaultSalesSCcustomerId = defaultCustomer.CustomerId;
                        }

                    }
                    if (cityId > 0)
                    {
                        MongoDbHelper<ExtandedCompanyDetail> mongoDbHelper = new MongoDbHelper<ExtandedCompanyDetail>();
                        var extandedCompanyDetail = mongoDbHelper.Select(x => x.CityId == cityId && x.AppType == "Retailer").FirstOrDefault();

                        if (extandedCompanyDetail != null)
                        {
                            companydetails.IsShowCreditOption = extandedCompanyDetail.IsShowCreditOption;
                            companydetails.IsOnlinePayment = extandedCompanyDetail.IsOnlinePayment;
                            companydetails.ischeckBookShow = extandedCompanyDetail.ischeckBookShow;
                            companydetails.IsRazorpayEnable = extandedCompanyDetail.IsRazorpayEnable;
                            companydetails.IsePayLaterShow = extandedCompanyDetail.IsePayLaterShow;
                            companydetails.IsFinBox = extandedCompanyDetail.IsFinBox;
                            companydetails.IsCreditLineShow = extandedCompanyDetail.IsCreditLineShow;
                        }
                    }

                    companydetails.FinboxclientApikey = FinBoxApiClientKey;

                    MongoDbHelper<VATMCustomers> VATMCustomershelper = new MongoDbHelper<VATMCustomers>();
                    var vATMCustomer = VATMCustomershelper.Select(x => x.CustomerId == customerId && x.IsActive).FirstOrDefault();

                    if (vATMCustomer != null && !string.IsNullOrEmpty(vATMCustomer.Data))
                    {
                        companydetails.IsShowVATM = true;
                    }
                    if (IsKpp)
                    {
                        double IsKPPMaxWalletPointUsed = Convert.ToDouble(ConfigurationManager.AppSettings["IsKPPMaxWalletPointUsed"]);
                        if (IsKPPMaxWalletPointUsed > 0)
                        {
                            companydetails.MaxWalletPointUsed = IsKPPMaxWalletPointUsed;
                        }
                        else
                        {
                            companydetails.MaxWalletPointUsed = 8000;
                        }
                        companydetails.ischeckBookShow = false;
                        companydetails.IsShowCreditOption = false;
                        companydetails.IsePayLaterShow = false;
                    }

                    if (!IsActive)
                    {
                        companydetails.IsShowCreditOption = false;
                        companydetails.IsOnlinePayment = false;
                        companydetails.ischeckBookShow = false;
                        companydetails.IsRazorpayEnable = false;
                        companydetails.IsePayLaterShow = false;
                        companydetails.IsFinBox = false;
                        companydetails.IsCreditLineShow = false;

                    }
                    if (LogDboyLoctionMeter > 0)
                    {
                        companydetails.LogDboyLoctionMeter = LogDboyLoctionMeter;
                    }
                    if (IsSalesMan)
                    {
                        companydetails.MaxWalletPointUsed = 0;
                    }


                    res = new companydetails
                    {
                        CompanyDetails = companydetails,
                        Status = true,
                        Message = "GetCompanyDetailsForRetailer"
                    };
                    return res;
                }
                else
                {
                    res = new companydetails
                    {
                        Status = false,
                        Message = "Something went Wrong"
                    };
                    return res;
                }
            }
        }

        [Route("AddCustomerFavorite")]
        [HttpGet]
        public async Task<custFavoriteItem> AddCustomerFavorite(int itemId, int customerId, bool isLike)
        {
            custFavoriteItem res;
            bool result = false;
            using (AuthContext context = new AuthContext())
            {

                var custFavoriteItem = context.CustFavoriteItems.FirstOrDefault(x => x.ItemId == itemId && x.CustomerId == customerId);

                if (custFavoriteItem != null)
                {
                    custFavoriteItem.ModifiedBy = customerId;
                    custFavoriteItem.ModifiedDate = indianTime;
                    custFavoriteItem.IsLike = isLike;
                    context.Entry(custFavoriteItem).State = EntityState.Modified;
                }
                else
                {
                    custFavoriteItem = new CustFavoriteItem
                    {
                        CreatedBy = customerId,
                        CreatedDate = indianTime,
                        IsLike = isLike,
                        ItemId = itemId,
                        CustomerId = customerId
                    };
                    context.CustFavoriteItems.Add(custFavoriteItem);
                }
                result = context.Commit() > 0;
                res = new custFavoriteItem
                {
                    Status = true,
                    Message = "Add Favorite Successfully"

                };
                return res;
            }

        }

        [Route("GetFavoriteItem")]
        [HttpGet]

        public async Task<List<CustFavoriteItemDc>> GetFavoriteItem(int customerId)
        {
            List<CustFavoriteItemDc> custFavoriteItemDcs = new List<CustFavoriteItemDc>();
            using (AuthContext context = new AuthContext())
            {
                custFavoriteItemDcs = context.CustFavoriteItems.Where(x => x.CustomerId == customerId && x.IsLike == true).Select(x => new CustFavoriteItemDc
                {
                    CustomerId = x.CustomerId,
                    id = x.id,
                    IsLike = x.IsLike,
                    ItemId = x.ItemId
                }).ToList();

            }
            return custFavoriteItemDcs;
        }

        [Route("GetCustomerWalletPoints")]
        [HttpGet]
        [AllowAnonymous]
        public WalletPointsSummary GetCustomerWalletPoints(int CustomerId, int page)
        {
            WalletPointsSummary WalletPointsSummary = new WalletPointsSummary();
            using (var db = new AuthContext())
            {
                var take = 0; var skip = 0;
                List<GetCustomerWalletHistory> retailergame = new List<GetCustomerWalletHistory>();
                var Conversion = db.CashConversionDb.FirstOrDefault(x => x.IsConsumer == true);
                if (Conversion != null)
                {
                    WalletPointsSummary.point = Conversion.point;
                    WalletPointsSummary.rupee = Conversion.rupee;
                }

                if (page == 0)
                {
                    take = 10;
                    skip = 0;

                    double UpCommingReward = 0;
                    double CompleteReward = 0;
                    double RedeemReward = 0;

                    MongoDbHelper<GameCustomerLedger> mongohelper = new MongoDbHelper<GameCustomerLedger>();
                    var CustomerBucket = mongohelper.Select(x => x.CustomerId == CustomerId).ToList();
                    if (CustomerBucket != null)
                    {
                        var reward = CustomerBucket.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                        UpCommingReward = reward.Where(x => x.IsUpComingReward == true && x.IsCompleted == false && x.IsCanceled == false && x.IsRedeemedReward == false).Sum(x => x.RewardValue);
                        CompleteReward = reward.Where(x => x.IsUpComingReward == false && x.IsCompleted == true && x.IsCanceled == false && x.IsRedeemedReward == false).Sum(x => x.RewardValue);
                        RedeemReward = reward.Where(x => x.IsUpComingReward == false && x.IsCompleted == false && x.IsCanceled == false && x.IsRedeemedReward == true).Sum(x => x.RewardValue);
                    }

                    CustomersManager manager = new CustomersManager();
                    var wdata = manager.GetWalletPoints(CustomerId, page);

                    WalletPointsSummary.UpcomingPoints = wdata.UpcomingPoints + UpCommingReward;
                    WalletPointsSummary.TotalEarnPoint = wdata.TotalEarnPoint;//(wdata.TotalEarnPoint + CompleteReward) - RedeemReward;
                    WalletPointsSummary.TotalUsedPoints = wdata.TotalUsedPoints;
                    WalletPointsSummary.totalCount = wdata.totalCount;
                    WalletPointsSummary.HowToUseWalletPoints = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.Wallet_TermsCondition).SingleOrDefault();
                    int count = db.CustomerWalletHistoryDb.Where(x => x.CustomerId == CustomerId).Count();
                    if (count > 0)
                    {
                        var wdata1 = manager.GetWalletPointsDetails(CustomerId, count, 0);
                        WalletPointsSummary.CustomerWalletHistory = wdata1.CustomerWalletHistory;
                    }


                    retailergame = GetHistoryofRetailerReward(CustomerId);
                    if (retailergame.Count > 0)
                    {
                        WalletPointsSummary.CustomerWalletHistory.AddRange(retailergame);
                    }


                }
                else
                {

                    take = 10;
                    skip = page * 10;
                    CustomersManager manager = new CustomersManager();
                    int count = db.CustomerWalletHistoryDb.Where(x => x.CustomerId == CustomerId).Count();
                    if (count > 0)
                    {
                        var wdata1 = manager.GetWalletPointsDetails(CustomerId, count, 0);
                        WalletPointsSummary.CustomerWalletHistory = wdata1.CustomerWalletHistory;
                    }
                    //var wdata1 = manager.GetWalletPointsDetails(CustomerId,count,0);
                    //WalletPointsSummary.CustomerWalletHistory = wdata1.CustomerWalletHistory;
                    retailergame = GetHistoryofRetailerReward(CustomerId);
                    if (retailergame.Count > 0)
                    {
                        WalletPointsSummary.CustomerWalletHistory.AddRange(retailergame);
                    }


                }

                if (WalletPointsSummary.CustomerWalletHistory != null)
                {
                    if (WalletPointsSummary.CustomerWalletHistory.Count > 0)
                    {
                        WalletPointsSummary.CustomerWalletHistory = WalletPointsSummary.CustomerWalletHistory.OrderByDescending(x => x.TransactionDate).Skip(skip).Take(take).ToList();
                    }
                }

                double? EP = ExpiringGamePoint(CustomerId);


                WalletPointsSummary.ExpiringPoints = EP;
                return WalletPointsSummary;

            }

        }



        [Route("GetPublishedSection")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<AppHomeSectionsDsc>> GetPublishedSection(string appType, int wId, string lang, int CustomerId, double? lat, double? lg)
        {
            using (var context = new AuthContext())
            {

                var todayDate = DateTime.Now.Date;
                int cnt = context.CustomerLocations.Count(x => x.CustomerId == CustomerId && !x.IsDeleted);
                if (lat.HasValue && lg.HasValue && lat.Value > 0 && lg.Value > 0 && (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 19) && cnt <= 10 && !context.CustomerLocations.Any(x => x.CustomerId == CustomerId && !x.IsDeleted && EntityFunctions.TruncateTime(x.CreatedDate) == todayDate))
                {
                    context.CustomerLocations.Add(new Model.CustomerDelight.CustomerLocation
                    {
                        CreatedDate = DateTime.Now,
                        CustomerId = CustomerId,
                        lat = lat.Value,
                        lg = lg.Value
                    });
                    context.Commit();
                }

                var customer = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                var inActiveCustomer = customer != null ? customer.Active == false || customer.Deleted == true : false;
                var customerFinBoxActivity = customer.IsFinBox ? customer.CurrentFinBoxActivity : "";
                var customerCreditLineActivity = customer.IsFinBoxCreditLine ? customer.CurrentCreditLineActivity : "";
                var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);
                List<AppHomeSectionsDsc> sections = new List<AppHomeSectionsDsc>();
                List<FinBoxConfig> FinBoxConfigs = null;
                var datenow = !IsPrimeCustomer ? indianTime.AddHours(MemberShipHours) : indianTime;
#if !DEBUG
                Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                var publishedData = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.APPHomeCacheKey(appType.Replace(" ", ""), wId.ToString()), () => GetPublisheddata(appType, wId));
#else
                var publishedData = GetPublisheddata(appType, wId);
#endif
                if (publishedData == null)
                {

                }
                else
                {
                    List<AppHomeSections> dbsections = new List<AppHomeSections>();
                    dbsections = JsonConvert.DeserializeObject<List<AppHomeSections>>(publishedData.ApphomeSection);
                    sections = Mapper.Map(dbsections).ToANew<List<AppHomeSectionsDsc>>();
                    var sectionItemIds = dbsections.Where(x => x.SectionType == "Banner" && x.AppItemsList.Any(y => ((x.SectionSubType == "Flash Deal" && y.OfferEndTime < indianTime)
                    || (x.SectionSubType != "Flash Deal" && y.OfferEndTime < indianTime))
                    && !y.Expired)).SelectMany(x => x.AppItemsList.Select(y => y.SectionItemID)).ToList();
                    if (sectionItemIds != null && sectionItemIds.Any())
                    {
                        var sectionitems = context.AppHomeSectionItemsDB.Where(x => sectionItemIds.Contains(x.SectionItemID));
                        if (sectionitems != null && sectionitems.Any())
                        {
                            foreach (var ap in sectionitems)
                            {
                                ap.Expired = true;
                                context.Entry(ap).State = EntityState.Modified;
                            }
                            context.Commit();
                        }
                    }

                    sections = sections.ToList().Select(o => new AppHomeSectionsDsc
                    {
                        AppItemsList = o.Deleted == false && (o.SectionType == "Banner" || o.SectionType == "PopUp" || o.SectionSubType == "Flash Deal") ? o.AppItemsList.Where(i => i.Deleted == false &&
                                                       ((o.SectionSubType == "Flash Deal" && (i.OfferEndTime > indianTime || i.OfferEndTime == null)
                                                       /*&& (i.OfferStartTime < datenow || i.OfferStartTime == null)*/  )
                                                        || (o.SectionSubType != "Flash Deal" && (i.OfferEndTime > indianTime || i.OfferEndTime == null) && (i.OfferStartTime < indianTime || i.OfferStartTime == null)))
                                                        ).ToList() : o.AppItemsList.Where(x => x.Deleted == false).ToList(),
                        SectionID = o.SectionID,
                        SectionName = o.SectionName,
                        SectionHindiName = o.SectionHindiName,
                        SectionType = o.SectionType,
                        SectionSubType = o.SectionSubType,
                        IsTile = o.IsTile,
                        IsBanner = o.IsBanner,
                        IsPopUp = o.IsPopUp,
                        Sequence = o.Sequence,
                        RowCount = o.RowCount,
                        ColumnCount = o.ColumnCount,
                        HasBackgroundColor = o.HasBackgroundColor,
                        TileBackgroundColor = o.TileBackgroundColor,
                        BannerBackgroundColor = o.BannerBackgroundColor,
                        TileHeaderBackgroundColor = o.TileHeaderBackgroundColor,
                        TileBackgroundImage = o.TileBackgroundImage,
                        IsSingleBackgroundImage = o.IsSingleBackgroundImage,
                        HasHeaderBackgroundImage = o.HasHeaderBackgroundImage,
                        TileHeaderBackgroundImage = o.TileHeaderBackgroundImage,
                        IsTileSlider = o.IsTileSlider,
                        TileAreaHeaderBackgroundImage = o.TileAreaHeaderBackgroundImage,
                        HeaderTextColor = o.HeaderTextColor,
                        HeaderTextSize = o.HeaderTextSize,
                        sectionBackgroundImage = o.sectionBackgroundImage,
                        Deleted = o.Deleted,
                        ViewType = o.ViewType,
                        WebViewUrl = o.WebViewUrl,
                    }).ToList();


                    if (!string.IsNullOrEmpty(customerFinBoxActivity) || !string.IsNullOrEmpty(customerCreditLineActivity))
                    {
                        FinBoxConfigs = context.FinBoxConfigs.Where(x => x.Type == 0).ToList();
                    }

                    sections.Where(x => x.SectionType == "Banner" || x.SectionType == "PopUp" || x.SectionSubType == "Flash Deal").ToList().ForEach(x =>
                    {
                        if (x.SectionType == "Banner")
                        {
                            if (x.AppItemsList.Any() && x.SectionSubType == "SubCategory")
                            {
                                x.AppItemsList.ForEach(y =>
                                {
                                    y.TileImage = string.IsNullOrEmpty(y.TileImage) ? y.BannerImage : y.TileImage;
                                });
                            }
                        }

                        if (x.SectionSubType == "Flash Deal")
                        {
                            if (x.AppItemsList.Any())
                            {
                                x.AppItemsList.ForEach(y =>
                                {
                                    y.OfferStartTime = IsPrimeCustomer ? y.OfferStartTime.Value : y.OfferStartTime.Value.AddHours(MemberShipHours);
                                });
                            }
                        }
                        //FinBox Banner changes functionality
                        if (!string.IsNullOrEmpty(customerFinBoxActivity) && FinBoxConfigs != null && FinBoxConfigs.Any(z => z.AppType == 0) && x.WebViewUrl == "FinBox")
                        {
                            if (FinBoxConfigs.Any(y => y.AppType == 0 && y.Activity.Contains(customerFinBoxActivity)))
                            {
                                var finboxImage = FinBoxConfigs.FirstOrDefault(y => y.AppType == 0 && y.Activity.Contains(customerFinBoxActivity));
                                if (!string.IsNullOrEmpty(finboxImage.Text))
                                {
                                    if (x.AppItemsList.Any())
                                    {
                                        x.AppItemsList.ForEach(y =>
                                        {
                                            y.BannerImage = finboxImage.Text;
                                        });
                                    }
                                }
                                else
                                {
                                    x.AppItemsList = null;
                                }
                            }

                        }

                        //Credit Line changes 
                        if (!string.IsNullOrEmpty(customerCreditLineActivity) && FinBoxConfigs != null && FinBoxConfigs.Any(y => y.AppType == 1) && x.WebViewUrl == "CreditLine")
                        {
                            if (FinBoxConfigs.Any(y => y.AppType == 1 && y.Activity.Contains(customerCreditLineActivity)))
                            {
                                var finboxImage = FinBoxConfigs.FirstOrDefault(y => y.AppType == 1 && y.Activity.Contains(customerCreditLineActivity));
                                if (!string.IsNullOrEmpty(finboxImage.Text))
                                {
                                    if (x.AppItemsList.Any())
                                    {
                                        x.AppItemsList.ForEach(y =>
                                        {
                                            y.BannerImage = finboxImage.Text;
                                        });
                                    }
                                }
                                else
                                {
                                    x.AppItemsList = null;
                                }
                            }

                        }

                        if (x.WebViewUrl == "SKInsurance")
                        {
                            InsuranceCustomer insuranceCustomer = context.InsuranceCustomers.FirstOrDefault(y => y.CustomerId == CustomerId && y.IsActive);
                            if (insuranceCustomer != null && !string.IsNullOrEmpty(insuranceCustomer.InsuranceUrl))
                            {
                                x.WebViewUrl = insuranceCustomer.InsuranceUrl;
                                string img = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , ConfigurationManager.AppSettings["InsuranceAgentImage"]);
                                if (x.AppItemsList.Any())
                                {
                                    x.AppItemsList.ForEach(y =>
                                    {
                                        y.BannerImage = img;
                                    });
                                }
                            }
                            else if (insuranceCustomer != null && !string.IsNullOrEmpty(insuranceCustomer.RegisteredMessage))
                            {
                                string img = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , ConfigurationManager.AppSettings["InsuranceBecomeAgentImage"]);
                                x.WebViewUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , "/images/InsuranceImages/registrationsuccessfulhtml/index.html");
                                if (x.AppItemsList.Any())
                                {
                                    x.AppItemsList.ForEach(y =>
                                    {
                                        y.BannerImage = img;
                                    });
                                }
                            }
                            else
                            {
                                x.WebViewUrl = ConfigurationManager.AppSettings["InsuranceDefaultUrl"].ToString().Replace("[SkCode]", customer.Skcode);
                                string img = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , ConfigurationManager.AppSettings["InsuranceBecomeAgentImage"]);
                                if (x.AppItemsList.Any())
                                {
                                    x.AppItemsList.ForEach(y =>
                                    {
                                        y.BannerImage = img;
                                    });
                                }
                            }
                        }
                    });

                    sections = sections.Where(x => (x.AppItemsList != null && x.AppItemsList.Count > 0) || (x.SectionSubType == "Other" && x.SectionType != "Banner") || x.SectionSubType == "DynamicHtml" || x.SectionSubType == "Store").ToList();

                    //if (inActiveCustomer) ||(x.SectionSubType== "Flash Deal" && x.o.OfferEndTime > indianTime)
                    //{
                    //    sections = sections.Where(x => x.SectionSubType != "Flash Deal").ToList();
                    //}

                    #region block Barnd
                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    var custtype = customer.IsKPP ? 1 : 2;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, wId);
                    #endregion


                    if (!string.IsNullOrEmpty(lang) && lang.ToLower() == "hi")
                    {

                        var BaseCategoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.BaseCategoryId)).Distinct().ToList();
                        var Categoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.CategoryId)).Distinct().ToList();
                        var Brandids = sections.SelectMany(x => x.AppItemsList.Select(y => y.SubsubCategoryId)).Distinct().ToList();
                        var SubCategoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.SubCategoryId)).Distinct().ToList();

                        var CatNames = Categoryids.Any() ? context.Categorys.Where(x => Categoryids.Contains(x.Categoryid)).Select(x => new { x.Categoryid, x.CategoryName, x.HindiName }).ToList() : null;
                        var BaseCatNames = BaseCategoryids.Any() ? context.BaseCategoryDb.Where(x => BaseCategoryids.Contains(x.BaseCategoryId)).Select(x => new { x.BaseCategoryId, x.BaseCategoryName, x.HindiName }).ToList() : null;
                        var SubCatNames = SubCategoryids.Any() ? context.SubCategorys.Where(x => SubCategoryids.Contains(x.SubCategoryId)).Select(x => new { x.SubCategoryId, x.SubcategoryName, x.HindiName }).ToList() : null;
                        var Subsubcatnames = Brandids.Any() ? context.SubsubCategorys.Where(x => Brandids.Contains(x.SubsubCategoryid)).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName, x.HindiName }).ToList() : null;


                        sections.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x.sectionBackgroundImage) && !x.sectionBackgroundImage.Contains("http"))
                            {
                                x.sectionBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , x.sectionBackgroundImage);
                            }
                            if (!string.IsNullOrEmpty(x.TileAreaHeaderBackgroundImage) && !x.TileAreaHeaderBackgroundImage.Contains("http"))
                            {
                                x.TileAreaHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , x.TileAreaHeaderBackgroundImage);
                            }
                            if (!string.IsNullOrEmpty(x.TileBackgroundImage) && !x.TileBackgroundImage.Contains("http"))
                            {
                                x.TileBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , x.TileBackgroundImage);
                            }
                            if (!string.IsNullOrEmpty(x.TileHeaderBackgroundImage) && !x.TileHeaderBackgroundImage.Contains("http"))
                            {
                                x.TileHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , x.TileHeaderBackgroundImage);
                            }
                            string SectionName = !string.IsNullOrEmpty(x.SectionHindiName) ? x.SectionHindiName : x.SectionName;
                            x.SectionName = SectionName;
                            x.AppItemsList.ForEach(y =>
                            {

                                if (!string.IsNullOrEmpty(y.DynamicHeaderImage) && !y.DynamicHeaderImage.Contains("http"))
                                {
                                    y.DynamicHeaderImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , y.DynamicHeaderImage);
                                }

                                if (x.SectionSubType == "Base Category")
                                {
                                    var basecat = BaseCatNames != null && BaseCatNames.Any(s => s.BaseCategoryId == y.BaseCategoryId) ? BaseCatNames.FirstOrDefault(s => s.BaseCategoryId == y.BaseCategoryId) : null;
                                    if (basecat != null)
                                    {
                                        string tileName = !string.IsNullOrEmpty(basecat.HindiName) ? basecat.HindiName : basecat.BaseCategoryName;
                                        y.TileName = tileName;
                                    }
                                }
                                else if (x.SectionSubType == "Category")
                                {
                                    var catdata = CatNames != null && CatNames.Any(s => s.Categoryid == y.CategoryId) ? CatNames.FirstOrDefault(s => s.Categoryid == y.CategoryId) : null;
                                    if (catdata != null)
                                    {
                                        string tileName = !string.IsNullOrEmpty(catdata.HindiName) ? catdata.HindiName : catdata.CategoryName;
                                        y.TileName = tileName;
                                    }
                                }
                                else if (x.SectionSubType == "SubCategory")
                                {
                                    var subcat = SubCatNames != null && SubCatNames.Any(s => s.SubCategoryId == y.SubCategoryId) ? SubCatNames.FirstOrDefault(s => s.SubCategoryId == y.SubCategoryId) : null;
                                    if (subcat != null)
                                    {
                                        string tileName = !string.IsNullOrEmpty(subcat.HindiName) ? subcat.HindiName : subcat.SubcategoryName;
                                        y.TileName = tileName;
                                    }
                                }
                                else if (x.SectionSubType == "Brand")
                                {
                                    var subsubcat = Subsubcatnames != null && Subsubcatnames.Any(s => s.SubsubCategoryid == y.SubsubCategoryId) ? Subsubcatnames.FirstOrDefault(s => s.SubsubCategoryid == y.SubsubCategoryId) : null;
                                    if (subsubcat != null)
                                    {
                                        string tileName = !string.IsNullOrEmpty(subsubcat.HindiName) ? subsubcat.HindiName : subsubcat.SubsubcategoryName;
                                        y.TileName = tileName;
                                    }
                                }

                                if (!string.IsNullOrEmpty(y.BannerImage) && !y.BannerImage.Contains("http"))
                                {
                                    y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , y.BannerImage);
                                }
                                if (!string.IsNullOrEmpty(y.TileImage) && !y.TileImage.Contains("http"))
                                {
                                    y.TileImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , y.TileImage);
                                }
                                if (!string.IsNullOrEmpty(y.TileSectionBackgroundImage) && !y.TileSectionBackgroundImage.Contains("http"))
                                {
                                    y.TileSectionBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , y.TileSectionBackgroundImage);
                                }

                            });

                            if (x.SectionSubType == "Brand")
                            {
                                if (blockBarnds != null && blockBarnds.Any())
                                {
                                    x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                                }
                            }
                            else if (x.SectionSubType == "Item")
                            {
                                if (blockBarnds != null && blockBarnds.Any())
                                {
                                    x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.CatId).Contains(t.CategoryId) && blockBarnds.Select(z => z.SubCatId).Contains(t.SubCategoryId) && blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                                }
                            }


                        });

                    }
                    else
                    {
                        sections.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x.sectionBackgroundImage) && !x.sectionBackgroundImage.Contains("http"))
                            {
                                x.sectionBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , x.sectionBackgroundImage);
                            }
                            if (!string.IsNullOrEmpty(x.TileAreaHeaderBackgroundImage) && !x.TileAreaHeaderBackgroundImage.Contains("http"))
                            {
                                x.TileAreaHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , x.TileAreaHeaderBackgroundImage);
                            }
                            if (!string.IsNullOrEmpty(x.TileBackgroundImage) && !x.TileBackgroundImage.Contains("http"))
                            {
                                x.TileBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , x.TileBackgroundImage);
                            }
                            if (!string.IsNullOrEmpty(x.TileHeaderBackgroundImage) && !x.TileHeaderBackgroundImage.Contains("http"))
                            {
                                x.TileHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , x.TileHeaderBackgroundImage);
                            }


                            x.AppItemsList.ForEach(y =>
                            {

                                if (!string.IsNullOrEmpty(y.DynamicHeaderImage) && !y.DynamicHeaderImage.Contains("http"))
                                {
                                    y.DynamicHeaderImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , y.DynamicHeaderImage);
                                }


                                if (!string.IsNullOrEmpty(y.BannerImage) && !y.BannerImage.Contains("http"))
                                {
                                    y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , y.BannerImage);
                                }
                                if (!string.IsNullOrEmpty(y.TileImage) && !y.TileImage.Contains("http"))
                                {
                                    y.TileImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , y.TileImage);
                                }
                                if (!string.IsNullOrEmpty(y.TileSectionBackgroundImage) && !y.TileSectionBackgroundImage.Contains("http"))
                                {
                                    y.TileSectionBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                                          , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                          , y.TileSectionBackgroundImage);
                                }
                            });
                            if (x.SectionSubType == "Brand")
                            {
                                if (blockBarnds != null && blockBarnds.Any())
                                {
                                    x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                                }
                            }
                            else if (x.SectionSubType == "Item")
                            {
                                if (blockBarnds != null && blockBarnds.Any())
                                {
                                    x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.CatId).Contains(t.CategoryId) && blockBarnds.Select(z => z.SubCatId).Contains(t.SubCategoryId) && blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                                }
                            }
                        });
                    }


                    if (sections != null && sections.Any(x => (x.SectionSubType == "Brand" || x.SectionSubType == "Category" || x.SectionSubType == "SubCategory") && x.AppItemsList != null && x.AppItemsList.Any()))
                    {
                        List<SubSubCategoryDcF> CategoryIds = sections.Where(x => x.SectionSubType == "Category").SelectMany(x => x.AppItemsList.Where(y => y.CategoryId > 0).Select(y =>
                            new SubSubCategoryDcF
                            {
                                SubsubCategoryid = y.SubsubCategoryId,
                                Categoryid = y.CategoryId,
                                SubCategoryId = y.SubCategoryId
                            })).Distinct().ToList();

                        List<SubSubCategoryDcF> CompanyIds = sections.Where(x => x.SectionSubType == "SubCategory").SelectMany(x => x.AppItemsList.Where(y => y.SubCategoryId > 0).Select(y =>
                            new SubSubCategoryDcF
                            {
                                SubsubCategoryid = y.SubsubCategoryId,
                                Categoryid = y.CategoryId,
                                SubCategoryId = y.SubCategoryId
                            })).Distinct().ToList();

                        List<SubSubCategoryDcF> brandIds = sections.Where(x => x.SectionSubType == "Brand").SelectMany(x => x.AppItemsList.Where(y => y.SubsubCategoryId > 0).Select(y =>
                            new SubSubCategoryDcF
                            {
                                SubsubCategoryid = y.SubsubCategoryId,
                                Categoryid = y.CategoryId,
                                SubCategoryId = y.SubCategoryId
                            })).Distinct().ToList();

                        #region Active Section 
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        var IdDt = new DataTable();
                        SqlParameter param = null;
                        DbCommand cmd = null;
                        if (CategoryIds != null && CategoryIds.Any())
                        {
                            IdDt = new DataTable();
                            IdDt.Columns.Add("CategoryId");
                            IdDt.Columns.Add("SubCategoryId");
                            IdDt.Columns.Add("BrandId");
                            foreach (var item in CategoryIds)
                            {
                                var dr = IdDt.NewRow();
                                dr["BrandId"] = item.SubsubCategoryid;
                                dr["SubCategoryId"] = item.SubCategoryId;
                                dr["CategoryId"] = item.Categoryid;
                                IdDt.Rows.Add(dr);
                            }

                            param = new SqlParameter("ids", IdDt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.TblCatSubCatBrand";
                            cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[GetActiveAppHomeSection]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(param);
                            cmd.Parameters.Add(new SqlParameter("@warehouseId", wId));
                            var typparam = new SqlParameter("@type", dbType: SqlDbType.Int);
                            typparam.Value = 0;
                            cmd.Parameters.Add(typparam);
                            cmd.Parameters.Add(new SqlParameter("@appType", appType));
                            // Run the sproc
                            using (var reader = cmd.ExecuteReader())
                            {
                                CategoryIds = ((IObjectContextAdapter)context).ObjectContext.Translate<SubSubCategoryDcF>(reader).ToList();
                            }
                        }
                        if (CompanyIds != null && CompanyIds.Any())
                        {
                            IdDt = new DataTable();
                            IdDt.Columns.Add("CategoryId");
                            IdDt.Columns.Add("SubCategoryId");
                            IdDt.Columns.Add("BrandId");
                            foreach (var item in CompanyIds)
                            {
                                var dr = IdDt.NewRow();
                                dr["BrandId"] = item.SubsubCategoryid;
                                dr["SubCategoryId"] = item.SubCategoryId;
                                dr["CategoryId"] = item.Categoryid;
                                IdDt.Rows.Add(dr);
                            }

                            param = new SqlParameter("ids", IdDt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.TblCatSubCatBrand";
                            cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[GetActiveAppHomeSection]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(param);
                            cmd.Parameters.Add(new SqlParameter("@warehouseId", wId));
                            cmd.Parameters.Add(new SqlParameter("@type", 1));
                            cmd.Parameters.Add(new SqlParameter("@appType", appType));
                            // Run the sproc
                            using (var reader2 = cmd.ExecuteReader())
                            {
                                CompanyIds = ((IObjectContextAdapter)context).ObjectContext.Translate<SubSubCategoryDcF>(reader2).ToList();
                            }
                        }

                        if (brandIds != null && brandIds.Any())
                        {

                            IdDt = new DataTable();
                            IdDt.Columns.Add("CategoryId");
                            IdDt.Columns.Add("SubCategoryId");
                            IdDt.Columns.Add("BrandId");
                            foreach (var item in brandIds)
                            {
                                var dr = IdDt.NewRow();
                                dr["BrandId"] = item.SubsubCategoryid;
                                dr["SubCategoryId"] = item.SubCategoryId;
                                dr["CategoryId"] = item.Categoryid;
                                IdDt.Rows.Add(dr);
                            }

                            param = new SqlParameter("ids", IdDt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.TblCatSubCatBrand";
                            cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[GetActiveAppHomeSection]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.Parameters.Add(param);
                            cmd.Parameters.Add(new SqlParameter("@warehouseId", wId));
                            cmd.Parameters.Add(new SqlParameter("@type", 2));
                            cmd.Parameters.Add(new SqlParameter("@appType", appType));
                            // Run the sproc
                            using (var reader1 = cmd.ExecuteReader())
                            {
                                brandIds = ((IObjectContextAdapter)context).ObjectContext.Translate<SubSubCategoryDcF>(reader1).ToList();
                            }

                        }
                        #endregion


                        sections.Where(x => (x.SectionSubType == "Brand" || x.SectionSubType == "Category" || x.SectionSubType == "SubCategory")).ToList().ForEach(x =>
                        {
                            if (x.SectionSubType == "Category")
                            {
                                x.AppItemsList = x.AppItemsList.Where(y => y.CategoryId == 0 || CategoryIds.Any(z => y.CategoryId == z.Categoryid)).ToList();
                                x.Deleted = (x.AppItemsList == null || !x.AppItemsList.Any()) ? true : false;
                            }
                            else if (x.SectionSubType == "SubCategory")
                            {
                                x.AppItemsList = x.AppItemsList.Where(y => y.SubCategoryId == 0 || CompanyIds.Any(z => y.SubCategoryId == z.SubCategoryId && y.CategoryId == z.Categoryid)).ToList();
                                x.Deleted = (x.AppItemsList == null || !x.AppItemsList.Any()) ? true : false;
                            }
                            else if (x.SectionSubType == "Brand")
                            {
                                x.AppItemsList = x.AppItemsList.Where(y => y.SubsubCategoryId == 0 || brandIds.Any(z => y.SubsubCategoryId == z.SubsubCategoryid && y.SubCategoryId == z.SubCategoryId && y.CategoryId == z.Categoryid)).ToList();
                                x.Deleted = (x.AppItemsList == null || !x.AppItemsList.Any()) ? true : false;
                            }
                        });

                        sections = sections.Where(x => !x.Deleted).ToList();
                    }


                }

                if (sections.Any() && EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                {
                    List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                    ElasticLanguageDataRequests = sections.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SectionName }).ToList();
                    ElasticLanguageDataRequests.AddRange(sections.SelectMany(y => y.AppItemsList.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.TileName }).ToList()));


                    LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                    var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                    sections.ToList().ForEach(x =>
                    {
                        x.SectionName = ElasticLanguageDatas.Any(y => y.englishtext == x.SectionName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SectionName).converttext : x.SectionName;
                        x.AppItemsList.ForEach(z =>
                        {
                            z.TileName = ElasticLanguageDatas.Any(y => y.englishtext == z.TileName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == z.TileName).converttext : z.TileName;
                        });
                    });
                }
                return sections.OrderBy(x => x.Sequence).ToList();
            }
        }

        public PublishAppHome GetPublisheddata(string appType, int wId, int subCategoryId = 0)
        {
            using (var context = new AuthContext())
            {
                var publishedData = context.PublishAppHomeDB.Where(x => x.SubCategoryID == subCategoryId && x.WarehouseID == wId && x.AppType == appType && x.Deleted == false).FirstOrDefault();
                return publishedData;
            }
        }


        [Route("GetStoreHome")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<AppHomeSectionsDsc>> GetStoreHome(int subCategoryId, int wId, string lang, int CustomerId)
        {
            string appType = "Store";
            using (var context = new AuthContext())
            {
                var customer = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                var inActiveCustomer = customer != null ? customer.Active == false || customer.Deleted == true : false;
                var customerFinBoxActivity = customer.IsFinBox ? customer.CurrentFinBoxActivity : "";
                var customerCreditLineActivity = customer.IsFinBoxCreditLine ? customer.CurrentCreditLineActivity : "";
                var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);
                List<AppHomeSectionsDsc> sections = new List<AppHomeSectionsDsc>();
                List<FinBoxConfig> FinBoxConfigs = null;
                var datenow = !IsPrimeCustomer ? indianTime.AddHours(MemberShipHours) : indianTime;
                var stringappType = appType + "_" + subCategoryId.ToString();

#if !DEBUG
                Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                var publishedData = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.APPHomeCacheKey(stringappType.Replace(" ", ""), wId.ToString()), () => GetPublisheddata(appType, wId, subCategoryId));
#else
                var publishedData = GetPublisheddata(appType, wId, subCategoryId);
#endif

                if (publishedData != null)
                {
                    List<AppHomeSections> dbsections = new List<AppHomeSections>();
                    dbsections = JsonConvert.DeserializeObject<List<AppHomeSections>>(publishedData.ApphomeSection);
                    sections = Mapper.Map(dbsections).ToANew<List<AppHomeSectionsDsc>>();
                    var sectionItemIds = dbsections.Where(x => x.SectionType == "Banner" && x.AppItemsList.Any(y => ((x.SectionSubType == "Flash Deal" && y.OfferEndTime < indianTime)
                    || (x.SectionSubType != "Flash Deal" && y.OfferEndTime < indianTime))
                    && !y.Expired)).SelectMany(x => x.AppItemsList.Select(y => y.SectionItemID)).ToList();
                    if (sectionItemIds != null && sectionItemIds.Any())
                    {
                        var sectionitems = context.AppHomeSectionItemsDB.Where(x => sectionItemIds.Contains(x.SectionItemID));
                        if (sectionitems != null && sectionitems.Any())
                        {
                            foreach (var ap in sectionitems)
                            {
                                ap.Expired = true;
                                context.Entry(ap).State = EntityState.Modified;
                            }
                            context.Commit();
                        }
                    }

                    sections = sections.ToList().Select(o => new AppHomeSectionsDsc
                    {
                        AppItemsList = o.Deleted == false && (o.SectionType == "Banner" || o.SectionType == "PopUp" || o.SectionSubType == "Flash Deal") ? o.AppItemsList.Where(i => i.Deleted == false &&
                                                       ((o.SectionSubType == "Flash Deal" && (i.OfferEndTime > indianTime || i.OfferEndTime == null)
                                                       /*&& (i.OfferStartTime < datenow || i.OfferStartTime == null)*/  )
                                                        || (o.SectionSubType != "Flash Deal" && (i.OfferEndTime > indianTime || i.OfferEndTime == null) && (i.OfferStartTime < indianTime || i.OfferStartTime == null)))
                                                        ).ToList() : o.AppItemsList.Where(x => x.Deleted == false).ToList(),
                        SectionID = o.SectionID,
                        SectionName = o.SectionName,
                        SectionHindiName = o.SectionHindiName,
                        SectionType = o.SectionType,
                        SectionSubType = o.SectionSubType,
                        IsTile = o.IsTile,
                        IsBanner = o.IsBanner,
                        IsPopUp = o.IsPopUp,
                        Sequence = o.Sequence,
                        RowCount = o.RowCount,
                        ColumnCount = o.ColumnCount,
                        HasBackgroundColor = o.HasBackgroundColor,
                        TileBackgroundColor = o.TileBackgroundColor,
                        BannerBackgroundColor = o.BannerBackgroundColor,
                        TileHeaderBackgroundColor = o.TileHeaderBackgroundColor,
                        IsSingleBackgroundImage = o.IsSingleBackgroundImage,
                        TileBackgroundImage = o.TileBackgroundImage,
                        HasHeaderBackgroundImage = o.HasHeaderBackgroundImage,
                        TileHeaderBackgroundImage = o.TileHeaderBackgroundImage,
                        IsTileSlider = o.IsTileSlider,
                        TileAreaHeaderBackgroundImage = o.TileAreaHeaderBackgroundImage,
                        HeaderTextColor = o.HeaderTextColor,
                        sectionBackgroundImage = o.sectionBackgroundImage,
                        Deleted = o.Deleted,
                        ViewType = o.ViewType,
                        WebViewUrl = o.WebViewUrl,
                    }).ToList();
                }

                if (!string.IsNullOrEmpty(customerFinBoxActivity) || !string.IsNullOrEmpty(customerCreditLineActivity))
                {
                    FinBoxConfigs = context.FinBoxConfigs.Where(x => x.Type == 0).ToList();
                }

                sections.Where(x => x.SectionType == "Banner" || x.SectionType == "PopUp" || x.SectionSubType == "Flash Deal").ToList().ForEach(x =>
                {
                    if (x.SectionType == "Banner")
                    {
                        if (x.AppItemsList.Any() && x.SectionSubType == "SubCategory")
                        {
                            x.AppItemsList.ForEach(y =>
                            {
                                y.TileImage = string.IsNullOrEmpty(y.TileImage) ? y.BannerImage : y.TileImage;
                            });
                        }
                    }

                    if (x.SectionSubType == "Flash Deal")
                    {
                        if (x.AppItemsList.Any())
                        {
                            x.AppItemsList.ForEach(y =>
                            {
                                y.OfferStartTime = IsPrimeCustomer ? y.OfferStartTime.Value : y.OfferStartTime.Value.AddHours(MemberShipHours);
                            });
                        }
                    }
                    //FinBox Banner changes functionality
                    if (!string.IsNullOrEmpty(customerFinBoxActivity) && FinBoxConfigs != null && FinBoxConfigs.Any(z => z.AppType == 0) && x.WebViewUrl == "FinBox")
                    {
                        if (FinBoxConfigs.Any(y => y.AppType == 0 && y.Activity.Contains(customerFinBoxActivity)))
                        {
                            var finboxImage = FinBoxConfigs.FirstOrDefault(y => y.AppType == 0 && y.Activity.Contains(customerFinBoxActivity));
                            if (!string.IsNullOrEmpty(finboxImage.Text))
                            {
                                if (x.AppItemsList.Any())
                                {
                                    x.AppItemsList.ForEach(y =>
                                    {
                                        y.BannerImage = finboxImage.Text;
                                    });
                                }
                            }
                            else
                            {
                                x.AppItemsList = null;
                            }
                        }

                    }

                    //Credit Line changes 
                    if (!string.IsNullOrEmpty(customerCreditLineActivity) && FinBoxConfigs != null && FinBoxConfigs.Any(y => y.AppType == 1) && x.WebViewUrl == "CreditLine")
                    {
                        if (FinBoxConfigs.Any(y => y.AppType == 1 && y.Activity.Contains(customerCreditLineActivity)))
                        {
                            var finboxImage = FinBoxConfigs.FirstOrDefault(y => y.AppType == 1 && y.Activity.Contains(customerCreditLineActivity));
                            if (!string.IsNullOrEmpty(finboxImage.Text))
                            {
                                if (x.AppItemsList.Any())
                                {
                                    x.AppItemsList.ForEach(y =>
                                    {
                                        y.BannerImage = finboxImage.Text;
                                    });
                                }
                            }
                            else
                            {
                                x.AppItemsList = null;
                            }
                        }

                    }

                    if (x.WebViewUrl == "SKInsurance")
                    {
                        InsuranceCustomer insuranceCustomer = context.InsuranceCustomers.FirstOrDefault(y => y.CustomerId == CustomerId && y.IsActive);
                        if (insuranceCustomer != null && !string.IsNullOrEmpty(insuranceCustomer.InsuranceUrl))
                        {
                            x.WebViewUrl = insuranceCustomer.InsuranceUrl;
                            string img = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , ConfigurationManager.AppSettings["InsuranceAgentImage"]);
                            if (x.AppItemsList.Any())
                            {
                                x.AppItemsList.ForEach(y =>
                                {
                                    y.BannerImage = img;
                                });
                            }
                        }
                        else if (insuranceCustomer != null && !string.IsNullOrEmpty(insuranceCustomer.RegisteredMessage))
                        {
                            string img = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , ConfigurationManager.AppSettings["InsuranceBecomeAgentImage"]);
                            x.WebViewUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , "/images/InsuranceImages/registrationsuccessfulhtml/index.html");
                            if (x.AppItemsList.Any())
                            {
                                x.AppItemsList.ForEach(y =>
                                {
                                    y.BannerImage = img;
                                });
                            }
                        }
                        else
                        {
                            x.WebViewUrl = ConfigurationManager.AppSettings["InsuranceDefaultUrl"].ToString().Replace("[SkCode]", customer.Skcode);
                            string img = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , ConfigurationManager.AppSettings["InsuranceBecomeAgentImage"]);
                            if (x.AppItemsList.Any())
                            {
                                x.AppItemsList.ForEach(y =>
                                {
                                    y.BannerImage = img;
                                });
                            }
                        }
                    }
                });

                sections = sections.Where(x => (x.AppItemsList != null && x.AppItemsList.Count > 0) || x.SectionSubType == "Other" || x.SectionSubType == "DynamicHtml").ToList();
                //if (inActiveCustomer) ||(x.SectionSubType== "Flash Deal" && x.o.OfferEndTime > indianTime)
                //{
                //    sections = sections.Where(x => x.SectionSubType != "Flash Deal").ToList();
                //}

                #region block Barnd
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                var custtype = customer.IsKPP ? 1 : 2;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, wId);
                #endregion


                if (!string.IsNullOrEmpty(lang) && lang.ToLower() == "hi")
                {

                    var BaseCategoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.BaseCategoryId)).Distinct().ToList();
                    var Categoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.CategoryId)).Distinct().ToList();
                    var Brandids = sections.SelectMany(x => x.AppItemsList.Select(y => y.SubsubCategoryId)).Distinct().ToList();
                    var SubCategoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.SubCategoryId)).Distinct().ToList();

                    var CatNames = Categoryids.Any() ? context.Categorys.Where(x => Categoryids.Contains(x.Categoryid)).Select(x => new { x.Categoryid, x.CategoryName, x.HindiName }).ToList() : null;
                    var BaseCatNames = BaseCategoryids.Any() ? context.BaseCategoryDb.Where(x => BaseCategoryids.Contains(x.BaseCategoryId)).Select(x => new { x.BaseCategoryId, x.BaseCategoryName, x.HindiName }).ToList() : null;
                    var SubCatNames = SubCategoryids.Any() ? context.SubCategorys.Where(x => SubCategoryids.Contains(x.SubCategoryId)).Select(x => new { x.SubCategoryId, x.SubcategoryName, x.HindiName }).ToList() : null;
                    var Subsubcatnames = Brandids.Any() ? context.SubsubCategorys.Where(x => Brandids.Contains(x.SubsubCategoryid)).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName, x.HindiName }).ToList() : null;


                    sections.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x.sectionBackgroundImage) && !x.sectionBackgroundImage.Contains("http"))
                        {
                            x.sectionBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.sectionBackgroundImage);
                        }
                        if (!string.IsNullOrEmpty(x.TileAreaHeaderBackgroundImage) && !x.TileAreaHeaderBackgroundImage.Contains("http"))
                        {
                            x.TileAreaHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.TileAreaHeaderBackgroundImage);
                        }
                        if (!string.IsNullOrEmpty(x.TileBackgroundImage) && !x.TileBackgroundImage.Contains("http"))
                        {
                            x.TileBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.TileBackgroundImage);
                        }
                        if (!string.IsNullOrEmpty(x.TileHeaderBackgroundImage) && !x.TileHeaderBackgroundImage.Contains("http"))
                        {
                            x.TileHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.TileHeaderBackgroundImage);
                        }
                        string SectionName = !string.IsNullOrEmpty(x.SectionHindiName) ? x.SectionHindiName : x.SectionName;
                        x.SectionName = SectionName;
                        x.AppItemsList.ForEach(y =>
                        {
                            if (x.SectionSubType == "Base Category")
                            {
                                var basecat = BaseCatNames != null && BaseCatNames.Any(s => s.BaseCategoryId == y.BaseCategoryId) ? BaseCatNames.FirstOrDefault(s => s.BaseCategoryId == y.BaseCategoryId) : null;
                                if (basecat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(basecat.HindiName) ? basecat.HindiName : basecat.BaseCategoryName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Category")
                            {
                                var catdata = CatNames != null && CatNames.Any(s => s.Categoryid == y.CategoryId) ? CatNames.FirstOrDefault(s => s.Categoryid == y.CategoryId) : null;
                                if (catdata != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(catdata.HindiName) ? catdata.HindiName : catdata.CategoryName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Sub Category")
                            {
                                var subcat = SubCatNames != null && SubCatNames.Any(s => s.SubCategoryId == y.SubCategoryId) ? SubCatNames.FirstOrDefault(s => s.SubCategoryId == y.SubCategoryId) : null;
                                if (subcat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(subcat.HindiName) ? subcat.HindiName : subcat.SubcategoryName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Brand")
                            {
                                var subsubcat = Subsubcatnames != null && Subsubcatnames.Any(s => s.SubsubCategoryid == y.SubsubCategoryId) ? Subsubcatnames.FirstOrDefault(s => s.SubsubCategoryid == y.SubsubCategoryId) : null;
                                if (subsubcat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(subsubcat.HindiName) ? subsubcat.HindiName : subsubcat.SubsubcategoryName;
                                    y.TileName = tileName;
                                }
                            }

                            if (!string.IsNullOrEmpty(y.BannerImage) && !y.BannerImage.Contains("http"))
                            {
                                y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.BannerImage);
                            }
                            if (!string.IsNullOrEmpty(y.TileImage) && !y.TileImage.Contains("http"))
                            {
                                y.TileImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileImage);
                            }
                            if (!string.IsNullOrEmpty(y.TileSectionBackgroundImage) && !y.TileSectionBackgroundImage.Contains("http"))
                            {
                                y.TileSectionBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileSectionBackgroundImage);
                            }

                        });

                        if (x.SectionSubType == "Brand")
                        {
                            if (blockBarnds != null && blockBarnds.Any())
                            {
                                x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();


                            }
                        }
                        else if (x.SectionSubType == "Item")
                        {
                            if (blockBarnds != null && blockBarnds.Any())
                            {
                                x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.CatId).Contains(t.CategoryId) && blockBarnds.Select(z => z.SubCatId).Contains(t.SubCategoryId) && blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                            }
                        }


                    });

                }
                else
                {
                    sections.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x.sectionBackgroundImage) && !x.sectionBackgroundImage.Contains("http"))
                        {
                            x.sectionBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.sectionBackgroundImage);
                        }
                        if (!string.IsNullOrEmpty(x.TileAreaHeaderBackgroundImage) && !x.TileAreaHeaderBackgroundImage.Contains("http"))
                        {
                            x.TileAreaHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.TileAreaHeaderBackgroundImage);
                        }
                        if (!string.IsNullOrEmpty(x.TileBackgroundImage) && !x.TileBackgroundImage.Contains("http"))
                        {
                            x.TileBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.TileBackgroundImage);
                        }
                        if (!string.IsNullOrEmpty(x.TileHeaderBackgroundImage) && !x.TileHeaderBackgroundImage.Contains("http"))
                        {
                            x.TileHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.TileHeaderBackgroundImage);
                        }

                        x.AppItemsList.ForEach(y =>
                        {
                            if (!string.IsNullOrEmpty(y.BannerImage) && !y.BannerImage.Contains("http"))
                            {
                                y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.BannerImage);
                            }
                            if (!string.IsNullOrEmpty(y.TileImage) && !y.TileImage.Contains("http"))
                            {
                                y.TileImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileImage);
                            }
                            if (!string.IsNullOrEmpty(y.TileSectionBackgroundImage) && !y.TileSectionBackgroundImage.Contains("http"))
                            {
                                y.TileSectionBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileSectionBackgroundImage);
                            }
                        });
                        if (x.SectionSubType == "Brand")
                        {
                            if (blockBarnds != null && blockBarnds.Any())
                            {
                                x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                            }
                        }
                        else if (x.SectionSubType == "Item")
                        {
                            if (blockBarnds != null && blockBarnds.Any())
                            {
                                x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.CatId).Contains(t.CategoryId) && blockBarnds.Select(z => z.SubCatId).Contains(t.SubCategoryId) && blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                            }
                        }
                    });
                }

                if (sections != null && sections.Any(x => x.SectionSubType == "Brand" && x.AppItemsList != null && x.AppItemsList.Any()))
                {
                    List<SubSubCategoryDcF> brandIds = sections.Where(x => x.SectionSubType == "Brand").SelectMany(x => x.AppItemsList.Where(y => y.SubsubCategoryId > 0).Select(y =>
                        new SubSubCategoryDcF
                        {
                            SubsubCategoryid = y.SubsubCategoryId,
                            Categoryid = y.CategoryId,
                            SubCategoryId = y.SubCategoryId
                        })).Distinct().ToList();
                    if (brandIds != null && brandIds.Any())
                    {
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        var IdDt = new DataTable();
                        IdDt.Columns.Add("CategoryId");
                        IdDt.Columns.Add("SubCategoryId");
                        IdDt.Columns.Add("BrandId");
                        foreach (var item in brandIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["BrandId"] = item.SubsubCategoryid;
                            dr["SubCategoryId"] = item.SubCategoryId;
                            dr["CategoryId"] = item.Categoryid;
                            IdDt.Rows.Add(dr);
                        }

                        var param = new SqlParameter("ids", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.TblCatSubCatBrand";
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetActiveAppHomeSection]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", wId));
                        cmd.Parameters.Add(new SqlParameter("@type", 2));
                        cmd.Parameters.Add(new SqlParameter("@appType", appType));
                        // Run the sproc
                        using (var reader1 = cmd.ExecuteReader())
                        {
                            brandIds = ((IObjectContextAdapter)context).ObjectContext.Translate<SubSubCategoryDcF>(reader1).ToList();
                        }
                    }
                    sections.Where(x => x.SectionSubType == "Brand").ToList().ForEach(x =>
                    {

                        x.AppItemsList = x.AppItemsList.Where(y => y.SubsubCategoryId == 0 || brandIds.Any(z => y.SubsubCategoryId == z.SubsubCategoryid && y.SubCategoryId == z.SubCategoryId && y.CategoryId == z.Categoryid)).ToList();
                        x.Deleted = (x.AppItemsList == null || !x.AppItemsList.Any()) ? true : false;

                    });

                    sections = sections.Where(x => !x.Deleted).ToList();
                }



                if (sections.Any() && EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                {
                    List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                    ElasticLanguageDataRequests = sections.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SectionName }).ToList();
                    ElasticLanguageDataRequests.AddRange(sections.SelectMany(y => y.AppItemsList.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.TileName }).ToList()));


                    LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                    var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                    sections.ToList().ForEach(x =>
                    {
                        x.SectionName = ElasticLanguageDatas.Any(y => y.englishtext == x.SectionName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SectionName).converttext : x.SectionName;
                        x.AppItemsList.ForEach(z =>
                        {
                            z.TileName = ElasticLanguageDatas.Any(y => y.englishtext == z.TileName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == z.TileName).converttext : z.TileName;
                        });
                    });
                }


                return sections.OrderBy(x => x.Sequence).ToList();

            }
        }


        private async Task<List<ItemDataDC>> ItemValidate(List<ItemDataDC> newdata, Customer ActiveCustomer, AuthContext db, string lang, bool IsSalesApp = false)
        {
            List<ItemDataDC> returnItems = new List<ItemDataDC>();
            if (ActiveCustomer != null && newdata.Any())
            {
                if (newdata != null && newdata.Any(s => !s.active))
                {
                    var InactiveItems = newdata.Where(s => !s.active);
                    InactiveItems = InactiveItems.GroupBy(x => x.ItemMultiMRPId).Select(x => x.FirstOrDefault()).ToList();
                    newdata = newdata.Where(s => s.active).ToList();
                    newdata.AddRange(InactiveItems);
                }
                var IsPrimeCustomer = db.PrimeCustomers.Any(x => x.CustomerId == ActiveCustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);

                DateTime CurrentDate = !IsPrimeCustomer ? indianTime.AddHours(-1 * MemberShipHours) : indianTime;
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                var warehouseId = ActiveCustomer != null && ActiveCustomer.Warehouseid.HasValue ? ActiveCustomer.Warehouseid.Value : 0;


                string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + warehouseId +
                                     " WHERE a.[CustomerId]=" + ActiveCustomer.CustomerId;
                var FlashDealWithItemIds = db.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();



                #region block Barnd
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                var custtype = ActiveCustomer.IsKPP ? 1 : 2;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion
                var AppType = "Retailer App";
                if (IsSalesApp)
                    AppType = "Sales App";

                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = new List<int>();
                activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.OfferOn == "Item" && x.IsActive && !x.IsDeleted && (x.OfferAppType == AppType || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();


                List<ItemDataDC> freeItems = null;
                if (activeOfferids.Any())
                {
                    var freeItemIds = newdata.Where(x => x.OfferId.HasValue && x.OfferId > 0 && activeOfferids.Contains(x.OfferId.Value)).Select(x => x.OfferFreeItemId).ToList();
                    freeItems = db.itemMasters.Where(x => freeItemIds.Contains(x.ItemId)).Select(x => new ItemDataDC
                    {
                        ItemId = x.ItemId,
                        itemname = x.itemname,
                        HindiName = x.HindiName,
                        IsSensitive = x.IsSensitive,
                        IsSensitiveMRP = x.IsSensitiveMRP,
                        price = x.price,
                        UnitofQuantity = x.UnitofQuantity,
                        UOM = x.UOM,
                        LogoUrl = x.LogoUrl
                    }).ToList();

                    if (lang.Trim() == "hi")
                    {
                        foreach (var it in freeItems)
                        {
                            if (!string.IsNullOrEmpty(it.HindiName))
                            {
                                if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                                }
                                else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                                {
                                    it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                                }

                                else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                                {
                                    it.itemname = it.HindiName; //item display name
                                }
                                else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                                }
                            }
                        }
                    }
                }

                var itemMultiMRPIds = newdata.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                var PrimeItems = itemMultiMRPIds.Any() ? db.PrimeItemDetails.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.CityId == ActiveCustomer.Cityid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList() : null;

                List<ItemScheme> ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, ActiveCustomer.Warehouseid.Value, db);
                foreach (var it in newdata.Where(x => (x.ItemAppType == 0 || x.ItemAppType == 1)))
                {
                    BackendOrderController backendOrderController = new BackendOrderController();
                    double cprice = backendOrderController.GetConsumerPrice(db, it.ItemMultiMRPId, it.price, it.UnitPrice, warehouseId);
                    it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice
                                                                   , it.WholeSalePrice ?? 0
                                                                   , it.TradePrice ?? 0, cprice);
                    if (PrimeItems != null && PrimeItems.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty))
                    {
                        var primeItem = PrimeItems.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty);
                        it.IsPrimeItem = true;
                        it.PrimePrice = primeItem.PrimePercent > 0 ? Convert.ToDecimal(it.UnitPrice - (it.UnitPrice * Convert.ToDouble(primeItem.PrimePercent) / 100)) : primeItem.PrimePrice;
                    }
                    //Condition for offer end.
                    if (!inActiveCustomer)
                    {
                        it.IsFlashDealStart = false;
                        if (it.OfferStartTime.HasValue)
                            it.NoPrimeOfferStartTime = it.OfferStartTime.Value.AddHours(MemberShipHours);
                        it.CurrentStartTime = indianTime;
                        if (IsPrimeCustomer)
                        {
                            it.IsFlashDealStart = it.OfferStartTime.Value <= indianTime;
                        }
                        else
                        {
                            it.IsFlashDealStart = it.NoPrimeOfferStartTime <= indianTime;
                        }

                        if (!(it.OfferStartTime <= CurrentDate && it.OfferEndTime >= indianTime))
                        {
                            if (it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                            else if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }
                        else if ((it.OfferStartTime <= CurrentDate && it.OfferEndTime >= indianTime) && it.OfferCategory == 2)
                        {
                            if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                            {
                                it.IsFlashDealUsed = true;
                            }
                        }

                        if (it.OfferCategory == 1)
                        {
                            if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            {
                                it.IsOffer = true;
                                if (freeItems != null && freeItems.Any(y => y.ItemId == it.OfferFreeItemId))
                                {
                                    it.OfferFreeItemName = freeItems.FirstOrDefault(y => y.ItemId == it.OfferFreeItemId).itemname;
                                    it.OfferFreeItemImage = freeItems.FirstOrDefault(y => y.ItemId == it.OfferFreeItemId).LogoUrl;
                                }
                            }
                            else
                                it.IsOffer = false;
                        }
                    }
                    else
                    {
                        if (it.OfferCategory == 1)
                        {
                            if (!(it.OfferStartTime <= CurrentDate && it.OfferEndTime >= indianTime) || !(activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId)))
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }
                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                    }

                    try
                    {
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;

                            //Customer (0.2 * 10=1)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }

                        }
                        else { it.dreamPoint = 0; }

                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        var unitprice = it.UnitPrice;
                        if (it.OfferCategory == 2 && it.IsOffer && it.FlashDealSpecialPrice.HasValue && it.FlashDealSpecialPrice > 0)
                        {
                            unitprice = it.FlashDealSpecialPrice.Value;
                        }
                        if (it.price > unitprice)
                        {
                            it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / unitprice) : 0;//MP;  we replce marginpoint value by margin for app here 

                            if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.PTR > 0))
                            {
                                var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId);
                                var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                                var UPMRPMargin = it.marginPoint.Value;
                                if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                    it.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                            }

                        }
                        else
                        {
                            it.marginPoint = 0;
                        }

                    }
                    catch { }

                    if (lang.Trim() == "hi")
                    {
                        if (!string.IsNullOrEmpty(it.HindiName))
                        {
                            if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                            {
                                it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                            }
                            else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                            {
                                it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                            }

                            else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                            {
                                it.itemname = it.HindiName; //item display name
                            }
                            else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                            {
                                it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                            }
                        }
                    }



                    returnItems.Add(it);
                }
            }

            if (returnItems.Any())
            {
                if (EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                {
                    List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataReqests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                    ElasticLanguageDataReqests = returnItems.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.itemBaseName }).ToList();
                    // ElasticLanguageDatas.AddRange(returnItems.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = x.itemname }).ToList());
                    ElasticLanguageDataReqests.AddRange(returnItems.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.OfferFreeItemName }).ToList());
                    ElasticLanguageDataReqests.AddRange(returnItems.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SellingUnitName }).ToList());
                    ElasticLanguageDataReqests.AddRange(returnItems.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.UOM }).ToList());
                    ElasticLanguageDataReqests.AddRange(returnItems.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.Scheme }).ToList());
                    ElasticLanguageDataReqests.AddRange(returnItems.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.UnitofQuantity }).ToList());


                    LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                    var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataReqests.Distinct().ToList(), lang.ToLower());
                    returnItems.ForEach(x =>
                    {
                        x.itemBaseName = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                        x.itemname = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                        if (x.IsSensitive == true && x.IsSensitiveMRP == true)
                        {
                            x.itemname += " " + x.price + " MRP " + x.UnitofQuantity + " " + x.UOM;
                        }
                        else if (x.IsSensitive == true && x.IsSensitiveMRP == false)
                        {
                            x.itemname += " " + x.UnitofQuantity + " " + x.UOM; //item display name 
                        }

                        else if (x.IsSensitive == false && x.IsSensitiveMRP == false)
                        {
                            x.itemname = x.itemBaseName; //item display name
                        }
                        else if (x.IsSensitive == false && x.IsSensitiveMRP == true)
                        {
                            x.itemname += " " + x.price + " MRP";//item display name 
                        }

                        x.OfferFreeItemName = ElasticLanguageDatas.Any(y => y.englishtext == x.OfferFreeItemName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.OfferFreeItemName).converttext : x.OfferFreeItemName;
                        x.SellingUnitName = ElasticLanguageDatas.Any(y => y.englishtext == x.SellingUnitName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SellingUnitName).converttext : x.SellingUnitName;
                        x.UOM = ElasticLanguageDatas.Any(y => y.englishtext == x.UOM) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.UOM).converttext : x.UOM;
                        x.Scheme = ElasticLanguageDatas.Any(y => y.englishtext == x.Scheme) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.Scheme).converttext : x.Scheme;
                        x.UnitofQuantity = ElasticLanguageDatas.Any(y => y.englishtext == x.UnitofQuantity) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.UnitofQuantity).converttext : x.UnitofQuantity;

                    });
                }

            }

            return returnItems;
        }


        [Route("RetailerGetItemBySection")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemListDc> RetailerGetItemBySection(int Warehouseid, int sectionid, int customerId, string lang = "en")
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);

                    ItemListDc res;
                    DateTime CurrentDate = !IsPrimeCustomer ? indianTime.AddHours(-1 * MemberShipHours) : indianTime;
                    var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                    if (data != null)
                    {
                        List<int> ids = data.Select(x => x.SectionItemID).ToList();
                        var customers = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                        var inActiveCustomer = customers.Active == false || customers.Deleted == true;

                        ItemListDc item = new ItemListDc();
                        //foreach (var itemid in data)

                        var itemids = data.Select(x => x.ItemId).ToList();
                        if (itemids != null && itemids.Any())
                        {
                            var newdata = new List<ItemDataDC>();
                            if (ElasticSearchEnable)
                            {
                                Suggest suggest = null;
                                MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                                var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetItemBySection");
                                var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                                var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                                    .Replace("{#warehouseid#}", Warehouseid.ToString())
                                    .Replace("{#itemid#}", string.Join(",", itemids))
                                    .Replace("{#from#}", "0")
                                    .Replace("{#size#}", itemids.Count().ToString());
                                List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                                newdata = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                            }
                            else
                            {
                                newdata = (from a in context.itemMasters
                                               //where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId) && !excludecategory.Contains(a.Categoryid) && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                           where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId))
                                           //join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                           let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                           select new ItemDataDC
                                           {
                                               WarehouseId = a.WarehouseId,
                                               BaseCategoryId = a.BaseCategoryid,
                                               SellingSku = a.SellingSku,
                                               SellingUnitName = a.SellingUnitName,
                                               IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                               ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                               CompanyId = a.CompanyId,
                                               ItemId = a.ItemId,
                                               ItemNumber = a.Number,
                                               itemname = a.itemname,
                                               HindiName = a.HindiName,
                                               LogoUrl = a.LogoUrl,
                                               MinOrderQty = a.MinOrderQty,
                                               price = a.price,
                                               TotalTaxPercentage = a.TotalTaxPercentage,
                                               UnitPrice = a.UnitPrice,
                                               active = a.active,
                                               marginPoint = a.marginPoint,
                                               promoPerItems = a.promoPerItems,
                                               NetPurchasePrice = a.NetPurchasePrice,
                                               IsOffer = a.IsOffer,
                                               Deleted = a.Deleted,
                                               OfferCategory = a.OfferCategory,
                                               OfferStartTime = a.OfferStartTime,
                                               OfferEndTime = a.OfferEndTime,
                                               OfferQtyAvaiable = a.OfferQtyAvaiable,
                                               OfferQtyConsumed = a.OfferQtyConsumed,
                                               OfferId = a.OfferId,
                                               OfferType = a.OfferType,
                                               OfferWalletPoint = a.OfferWalletPoint,
                                               OfferFreeItemId = a.OfferFreeItemId,
                                               OfferPercentage = a.OfferPercentage,
                                               OfferFreeItemName = a.OfferFreeItemName,
                                               OfferFreeItemImage = a.OfferFreeItemImage,
                                               OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                               OfferMinimumQty = a.OfferMinimumQty,
                                               FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                               FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                               ItemMultiMRPId = a.ItemMultiMRPId,
                                               BillLimitQty = a.BillLimitQty,
                                               ItemAppType = a.ItemAppType,
                                               Categoryid = a.Categoryid,
                                               SubCategoryId = a.SubCategoryId,
                                               SubsubCategoryid = a.SubsubCategoryid,
                                               TradePrice = a.TradePrice,
                                               WholeSalePrice = a.WholeSalePrice
                                           }).ToList();
                            }
                            newdata = newdata.OrderByDescending(x => x.ItemNumber).ToList();
                            item.ItemMasters = new List<ItemDataDC>();
                            var formatedData = await ItemValidate(newdata, customers, context, lang);
                            item.ItemMasters.AddRange(formatedData);

                        }

                        if (item.ItemMasters != null)
                        {
                            res = new ItemListDc
                            {

                                ItemMasters = item.ItemMasters,
                                Status = true,
                                Message = "Success"
                            };
                            return res;
                        }
                        else
                        {
                            Array[] l = new Array[] { };
                            res = new ItemListDc
                            {
                                ItemMasters = null,
                                Status = false,
                                Message = "fail"
                            };
                            return res;
                        }
                    }
                    else
                    {
                        Array[] l = new Array[] { };
                        res = new ItemListDc
                        {
                            ItemMasters = null,
                            Status = false,
                            Message = "fail"
                        };
                        return res;
                    }
                }

                catch (Exception ee)
                {
                    throw;
                }
            }
        }

        [Route("RatailerFlashDealoffer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemListDc> RatailerFlashDealoffer(int Warehouseid, int sectionid, int CustomerId, string lang = "en")
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);


                    ItemListDc res;
                    DateTime CurrentDate = indianTime.AddHours(-24);
                    var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Where(y => y.Active).Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                    if (data != null)
                    {
                        var customers = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                        ItemListDc item = new ItemListDc();
                        var itemids = data.Select(x => x.ItemId);
                        var newdata = new List<ItemDataDC>();
                        if (ElasticSearchEnable)
                        {
                            Suggest suggest = null;
                            MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                            var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RatailerFlashDealoffer");
                            var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                            var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                                .Replace("{#warehouseid#}", Warehouseid.ToString())
                                .Replace("{#itemid#}", string.Join(",", itemids))
                                .Replace("{#offerenddate#}", indianTime.ToString("yyyy-MM-dd'T'HH:mm:ss"))
                                .Replace("{#from#}", "0")
                                .Replace("{#size#}", itemids.Count().ToString());
                            List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                            newdata = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                        }
                        else
                        {
                            newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId) && a.OfferType == "FlashDeal"
                                             && a.OfferEndTime >= indianTime && a.OfferQtyAvaiable > 0 && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                       select new ItemDataDC
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           ItemId = a.ItemId,
                                           ItemNumber = a.Number,
                                           itemname = a.itemname,
                                           HindiName = a.HindiName,
                                           LogoUrl = a.LogoUrl,
                                           MinOrderQty = a.MinOrderQty,
                                           price = a.price,
                                           TotalTaxPercentage = a.TotalTaxPercentage,
                                           UnitPrice = a.UnitPrice,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = a.IsOffer,
                                           Deleted = a.Deleted,
                                           OfferCategory = a.OfferCategory,
                                           OfferStartTime = a.OfferStartTime,
                                           OfferEndTime = a.OfferEndTime,
                                           OfferQtyAvaiable = a.OfferQtyAvaiable,
                                           OfferQtyConsumed = a.OfferQtyConsumed,
                                           OfferId = a.OfferId,
                                           OfferType = a.OfferType,
                                           OfferWalletPoint = a.OfferWalletPoint,
                                           OfferFreeItemId = a.OfferFreeItemId,
                                           OfferPercentage = a.OfferPercentage,
                                           OfferFreeItemName = a.OfferFreeItemName,
                                           OfferFreeItemImage = a.OfferFreeItemImage,
                                           OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                           OfferMinimumQty = a.OfferMinimumQty,
                                           FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                           FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                           ItemMultiMRPId = a.ItemMultiMRPId,
                                           BillLimitQty = a.BillLimitQty,
                                           ItemAppType = a.ItemAppType,
                                           Categoryid = a.Categoryid,
                                           SubCategoryId = a.SubCategoryId,
                                           SubsubCategoryid = a.SubsubCategoryid,
                                           BaseCategoryId = a.BaseCategoryid,
                                           SellingSku = a.SellingSku,
                                           SellingUnitName = a.SellingUnitName,
                                           IsSensitive = a.IsSensitive,
                                           IsSensitiveMRP = a.IsSensitiveMRP,
                                           TradePrice = a.TradePrice,
                                           WholeSalePrice = a.WholeSalePrice
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        }

                        item.ItemMasters = new List<ItemDataDC>();
                        var olddata = newdata.Select(x => new { x.ItemId, x.FlashDealSpecialPrice }).ToList();
                        var formatedData = await ItemValidate(newdata, customers, context, lang);
                        foreach (var it in formatedData)
                        {
                            it.FlashDealSpecialPrice = olddata.FirstOrDefault(x => x.ItemId == it.ItemId).FlashDealSpecialPrice;
                        }

                        item.ItemMasters.AddRange(formatedData);

                        if (item.ItemMasters != null)
                        {
                            res = new ItemListDc
                            {

                                ItemMasters = item.ItemMasters,
                                Status = true,
                                Message = "Success."
                            };
                            return res;
                        }
                        else
                        {
                            Array[] l = new Array[] { };
                            res = new ItemListDc
                            {
                                ItemMasters = null,
                                Status = false,
                                Message = "fail"
                            };
                            return res;
                        }
                    }
                    else
                    {
                        Array[] l = new Array[] { };
                        res = new ItemListDc
                        {
                            ItemMasters = null,
                            Status = false,
                            Message = "fail"
                        };
                        return res;
                    }
                }

                catch (Exception ee)
                {
                    throw;
                }
            }
        }


        [Route("RetailerStoreFlashDealoffer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemListDc> RetailerStoreFlashDealoffer(int Warehouseid, int sectionid, int CustomerId, int SubCategoryId, string lang = "en")
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);

                    ItemListDc item = new ItemListDc();
                    DateTime CurrentDate = indianTime.AddHours(-24);
                    var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SubCategoryID == SubCategoryId && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();

                    if (sectionid == -1)
                    {
                        data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SubCategoryID == SubCategoryId && x.SectionSubType == "Flash Deal" && x.Active && !x.Deleted).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                    }

                    if (data != null)
                    {
                        var customers = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                        var itemids = data.Select(x => x.ItemId);
                        var newdata = new List<ItemDataDC>();
                        if (ElasticSearchEnable)
                        {
                            Suggest suggest = null;
                            MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                            var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RatailerFlashDealoffer");
                            var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                            var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                                .Replace("{#warehouseid#}", Warehouseid.ToString())
                                .Replace("{#itemid#}", string.Join(",", itemids))
                                .Replace("{#offerenddate#}", indianTime.ToString("yyyy-MM-dd'T'HH:mm:ss"))
                                .Replace("{#from#}", "0")
                                .Replace("{#size#}", itemids.Count().ToString());
                            List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                            newdata = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                        }
                        else
                        {
                            newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == Warehouseid && a.Deleted == false && a.SubCategoryId == SubCategoryId && a.active == true && itemids.Contains(a.ItemId) && a.OfferType == "FlashDeal"
                                             && a.OfferEndTime >= indianTime && a.OfferQtyAvaiable > 0 && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                       select new ItemDataDC
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           ItemId = a.ItemId,
                                           ItemNumber = a.Number,
                                           itemname = a.itemname,
                                           LogoUrl = a.LogoUrl,
                                           MinOrderQty = a.MinOrderQty,
                                           price = a.price,
                                           TotalTaxPercentage = a.TotalTaxPercentage,
                                           UnitPrice = a.UnitPrice,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = a.IsOffer,
                                           Deleted = a.Deleted,
                                           OfferCategory = a.OfferCategory,
                                           OfferStartTime = a.OfferStartTime,
                                           OfferEndTime = a.OfferEndTime,
                                           OfferQtyAvaiable = a.OfferQtyAvaiable,
                                           OfferQtyConsumed = a.OfferQtyConsumed,
                                           OfferId = a.OfferId,
                                           OfferType = a.OfferType,
                                           OfferWalletPoint = a.OfferWalletPoint,
                                           OfferFreeItemId = a.OfferFreeItemId,
                                           OfferPercentage = a.OfferPercentage,
                                           OfferFreeItemName = a.OfferFreeItemName,
                                           OfferFreeItemImage = a.OfferFreeItemImage,
                                           OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                           OfferMinimumQty = a.OfferMinimumQty,
                                           FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                           FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                           ItemMultiMRPId = a.ItemMultiMRPId,
                                           BillLimitQty = a.BillLimitQty,
                                           ItemAppType = a.ItemAppType,
                                           Categoryid = a.Categoryid,
                                           SubCategoryId = a.SubCategoryId,
                                           SubsubCategoryid = a.SubsubCategoryid,
                                           BaseCategoryId = a.BaseCategoryid,
                                           SellingSku = a.SellingSku,
                                           SellingUnitName = a.SellingUnitName,
                                           TradePrice = a.TradePrice,
                                           WholeSalePrice = a.WholeSalePrice
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        }

                        item.ItemMasters = new List<ItemDataDC>();
                        var formatedData = await ItemValidate(newdata, customers, context, lang);
                        item.ItemMasters.AddRange(formatedData);

                        if (item.ItemMasters != null)
                        {
                            item.ItemMasters = item.ItemMasters;
                            item.Message = "Success.";
                            item.Status = true;
                            return item;
                        }
                        else
                        {
                            item.ItemMasters = null;
                            item.Message = "fail";
                            item.Status = false;
                            return item;
                        }
                    }
                    else
                    {
                        item.ItemMasters = null;
                        item.Message = "fail";
                        item.Status = false;
                        return item;
                    }
                }

                catch (Exception ee)
                {
                    throw;
                }
            }
        }


        [Route("RatailerFlashExists")]
        [HttpGet]
        public async Task<NotificationFlashDeal> RatailerFlashExists(int Warehouseid, int CustomerId)
        {
            using (var context = new AuthContext())
            {
                var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);

                string sqlquery = "SELECT top 1 b.SectionID,Min(c.OfferStartTime) StartTime FROM  AppHomeSectionItems c  inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.[Deleted]=0 and (c.OfferStartTime > DATEADD(HOUR,-24,GETDATE()) or (c.OfferEndTime > GETDATE() and  c.OfferStartTime < GETDATE())) and b.SectionSubType='Flash Deal' and b.WarehouseID=" + Warehouseid + " group by b.SectionID ";
                //if (IsPrimeCustomer)
                //    sqlquery = "SELECT top 1 b.SectionID,DATEADD(HOUR,-2,Min(c.OfferStartTime)) StartTime FROM  AppHomeSectionItems c  inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.[Deleted]=0 and (c.OfferStartTime > DATEADD(HOUR,-24,GETDATE()) or (c.OfferEndTime > GETDATE() and  c.OfferStartTime < DATEADD(HOUR,-2,GETDATE()))) and b.SectionSubType='Flash Deal' and b.WarehouseID=" + Warehouseid + " group by b.SectionID ";
                var NotificationFlashDeal = context.Database.SqlQuery<NotificationFlashDeal>(sqlquery).FirstOrDefault();
                if (NotificationFlashDeal != null)
                {
                    NotificationFlashDeal.FlashDealStatus = true;
                    if (NotificationFlashDeal.StartTime > DateTime.Now)
                    {
                        NotificationFlashDeal.NonPrimeStartTime = NotificationFlashDeal.StartTime.Value.AddHours(MemberShipHours);
                        NotificationFlashDeal.IsPrimeCustomer = IsPrimeCustomer;
                        NotificationFlashDeal.LogoUrl = context.NoFlashDealImage.Where(x => x.WarehouseId == Warehouseid && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ImagePath).FirstOrDefault();
                        if (string.IsNullOrEmpty(NotificationFlashDeal.LogoUrl))
                        {
                            NotificationFlashDeal.LogoUrl = ConfigurationManager.AppSettings["NotificationFlashDealUrl"].ToString();
                        }
                        else
                        {
                            var fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                                  , HttpContext.Current.Request.Url.DnsSafeHost
                                                                  , HttpContext.Current.Request.Url.Port
                                                                  , string.Format("UploadedImages/{0}", NotificationFlashDeal.LogoUrl));
                            NotificationFlashDeal.LogoUrl = fileUrl;

                        }
                    }
                    else
                    {
                        NotificationFlashDeal.StartTime = null;
                    }
                }
                else
                {

                    NotificationFlashDeal = new NotificationFlashDeal
                    {
                        FlashDealStatus = false,
                        LogoUrl = "",
                        SectionID = 0,
                        StartTime = null
                    };
                }
                return NotificationFlashDeal;
            }
        }

        [Route("RatailerFlashBannerUrl")]
        [HttpGet]
        public async Task<string> RatailerFlashBannerUrl(int Warehouseid, int CustomerId)
        {
            string FlashDealBannerURL = string.Empty;
            using (var context = new AuthContext())
            {
                FlashDealBannerURL = context.NoFlashDealImage.Where(x => x.WarehouseId == Warehouseid && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ImagePath).FirstOrDefault();
                if (string.IsNullOrEmpty(FlashDealBannerURL))
                {
                    FlashDealBannerURL = ConfigurationManager.AppSettings["NotificationFlashDealUrl"].ToString();
                }
                else
                {
                    var fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                          , HttpContext.Current.Request.Url.DnsSafeHost
                                                          , HttpContext.Current.Request.Url.Port
                                                          , string.Format("UploadedImages/{0}", FlashDealBannerURL));
                    FlashDealBannerURL = fileUrl;
                }
                return FlashDealBannerURL;
            }
        }

        [Route("RetailerGetallNotification")]
        [HttpGet]
        public PaggingDatas RetailerGetallNotification(int list, int page, int customerid)
        {
            using (var context = new AuthContext())
            {
                PaggingDatas data = new PaggingDatas();
                context.Database.CommandTimeout = 600;
                var query = "GetCustomerNotification " + customerid.ToString() + "," + ((page - 1) * list).ToString() + ",10";
                var DeviceNotificationDcs = context.Database.SqlQuery<DeviceNotificationDc>(query).ToList();

                //var notificationmaster = context.DeviceNotificationDb.Where(x => x.CustomerId == customerid).ToList();//.OrderByDescending(x => x.NotificationTime).Skip((page - 1) * list).Take(10).ToList();
                data.notificationmaster = DeviceNotificationDcs;
                data.total_count = DeviceNotificationDcs != null && DeviceNotificationDcs.Any() ? DeviceNotificationDcs.FirstOrDefault().TotalCount : 0;
                return data;
            }

        }

        [Route("RetailerGetNotificationItems")]
        [HttpGet]
        public async Task<List<ItemDataDC>> RetailerGetNotificationItems(string notificationType, int typeId, int wareHouseId, int customerId, string lang = "en")
        {
            List<ItemMaster> items = new List<ItemMaster>();
            using (var myContext = new AuthContext())
            {
                var ActiveCustomer = myContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;

                string condition = "";
                switch (notificationType)
                {
                    case "Item":
                        if (ElasticSearchEnable)
                        {
                            condition = "{\"term\": { \"itemid\": " + typeId + " }}";
                        }
                        else
                        {
                            items = myContext.itemMasters.Where(x => x.ItemId == typeId && (x.ItemAppType == 0 || x.ItemAppType == 1) && x.active && !x.Deleted && x.WarehouseId == wareHouseId).ToList();
                        }
                        break;

                    case "SubCategory":
                        if (ElasticSearchEnable)
                        {
                            condition = "{\"term\": { \"subcategoryid\": " + typeId + " }}";
                        }
                        else
                        {
                            items = myContext.itemMasters.Where(x => x.SubCategoryId == typeId && (x.ItemAppType == 0 || x.ItemAppType == 1) && x.active && !x.Deleted && x.WarehouseId == wareHouseId).ToList();

                        }
                        break;

                    case "Category":
                        if (ElasticSearchEnable)
                        {
                            condition = "{\"term\": { \"categoryid\": " + typeId + " }}";
                        }
                        else
                        {
                            items = myContext.itemMasters.Where(x => x.Categoryid == typeId && (x.ItemAppType == 0 || x.ItemAppType == 1) && x.active && !x.Deleted && x.WarehouseId == wareHouseId).ToList();
                        }
                        break;

                    case "Brand":
                        if (ElasticSearchEnable)
                        {
                            condition = "{\"term\": { \"subsubcategoryid\": " + typeId + " }}";
                        }
                        else
                        {
                            items = myContext.itemMasters.Where(x => x.SubsubCategoryid == typeId && (x.ItemAppType == 0 || x.ItemAppType == 1) && x.active && !x.Deleted && x.WarehouseId == wareHouseId).ToList();
                        }
                        break;
                }
                var retList = new List<ItemDataDC>();
                if (ElasticSearchEnable)
                {
                    Suggest suggest = null;
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetNotificationItems");
                    var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", wareHouseId.ToString())
                        .Replace("{#searchcondition#}", condition)
                        .Replace("{#from#}", "0")
                        .Replace("{#size#}", "1000");
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    retList = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                }
                else
                {
                    retList = items.Select(a => new ItemDataDC
                    {
                        WarehouseId = a.WarehouseId,
                        CompanyId = a.CompanyId,
                        ItemId = a.ItemId,
                        ItemNumber = a.Number,
                        itemBaseName = a.itemBaseName,
                        itemname = a.itemname,
                        HindiName = a.HindiName,
                        LogoUrl = a.LogoUrl,
                        MinOrderQty = a.MinOrderQty,
                        price = a.price,
                        TotalTaxPercentage = a.TotalTaxPercentage,
                        UnitPrice = a.UnitPrice,
                        active = a.active,
                        marginPoint = a.marginPoint,
                        NetPurchasePrice = a.NetPurchasePrice,
                        promoPerItems = a.promoPerItems,
                        IsOffer = a.IsOffer,
                        Deleted = a.Deleted,
                        OfferCategory = a.OfferCategory,
                        OfferStartTime = a.OfferStartTime,
                        OfferEndTime = a.OfferEndTime,
                        OfferQtyAvaiable = a.OfferQtyAvaiable,
                        OfferQtyConsumed = a.OfferQtyConsumed,
                        OfferId = a.OfferId,
                        OfferType = a.OfferType,
                        OfferWalletPoint = a.OfferWalletPoint,
                        OfferFreeItemId = a.OfferFreeItemId,
                        OfferPercentage = a.OfferPercentage,
                        OfferFreeItemName = a.OfferFreeItemName,
                        OfferFreeItemImage = a.OfferFreeItemImage,
                        OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                        OfferMinimumQty = a.OfferMinimumQty,
                        FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                        FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                        ItemAppType = a.ItemAppType,
                        ItemMultiMRPId = a.ItemMultiMRPId,
                        TradePrice = a.TradePrice,
                        WholeSalePrice = a.WholeSalePrice
                    }).OrderByDescending(x => x.ItemNumber).ToList();
                }
                var formatedData = await ItemValidate(retList, ActiveCustomer, myContext, lang);


                return formatedData;
            }

        }

        [Route("RetailerGetAllBrand")]
        [HttpGet]
        public async Task<List<SubsubCategoryDc>> RetailerGetAllBrand(int WarehouseId, int customerId, string lang)
        {
            using (var db = new AuthContext())
            {
                var customers = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                string query = "Exec GetRetailerAllBrandByWarehouse " + WarehouseId;
                var ass = db.Database.SqlQuery<SubsubCategoryDc>(query).ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = customers != null && customers.IsKPP ? 1 : 2;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, WarehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    ass = ass.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                if (ass != null && ass.Any())
                {
                    if (!string.IsNullOrEmpty(lang) && lang.ToLower() == "hi")
                    {
                        ass.ForEach(x => x.SubsubcategoryName = string.IsNullOrEmpty(x.HindiName) ? x.SubsubcategoryName : x.HindiName);
                    }
                    else if (EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                    {
                        List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                        ElasticLanguageDataRequests = ass.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubsubcategoryName }).ToList();
                        LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                        var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests, lang.ToLower());
                        ass.ForEach(x => { x.SubsubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubsubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubsubcategoryName).converttext : x.SubsubcategoryName; });
                    }
                }
                return ass;
            }


        }

        [Route("RetailerGetAllItemByBrand")]
        [HttpGet]
        public async Task<ItemListDc> RetailerGetAllItemByBrand(string lang, int warehouseid, int SubsubCategoryid, int customerId)
        {
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;

                ItemListDc item = new ItemListDc();

                var newdata = new List<ItemDataDC>();
                if (ElasticSearchEnable)
                {
                    Suggest suggest = null;
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetAllItemByBrand");
                    var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", warehouseid.ToString())
                        .Replace("{#brand#}", SubsubCategoryid.ToString())
                        .Replace("{#from#}", "0")
                        .Replace("{#size#}", "1000");
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    newdata = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                }
                else
                {
                    newdata = (from a in context.itemMasters
                               where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && !a.IsDisContinued && a.SubsubCategoryid == SubsubCategoryid)
                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                               select new ItemDataDC
                               {
                                   WarehouseId = a.WarehouseId,
                                   IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                   ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                   CompanyId = a.CompanyId,
                                   ItemId = a.ItemId,
                                   ItemNumber = a.Number,
                                   itemname = a.itemname,
                                   itemBaseName = a.itemBaseName,
                                   HindiName = a.HindiName,
                                   LogoUrl = a.LogoUrl,
                                   MinOrderQty = a.MinOrderQty,
                                   price = a.price,
                                   TotalTaxPercentage = a.TotalTaxPercentage,
                                   UnitPrice = a.UnitPrice,
                                   active = a.active,
                                   dreamPoint = 0,
                                   marginPoint = a.marginPoint,
                                   NetPurchasePrice = a.NetPurchasePrice,
                                   promoPerItems = a.promoPerItems,
                                   IsOffer = a.IsOffer,
                                   Deleted = a.Deleted,
                                   OfferCategory = a.OfferCategory,
                                   OfferStartTime = a.OfferStartTime,
                                   OfferEndTime = a.OfferEndTime,
                                   OfferQtyAvaiable = a.OfferQtyAvaiable,
                                   OfferQtyConsumed = a.OfferQtyConsumed,
                                   OfferId = a.OfferId,
                                   OfferType = a.OfferType,
                                   OfferWalletPoint = a.OfferWalletPoint,
                                   OfferFreeItemId = a.OfferFreeItemId,
                                   OfferPercentage = a.OfferPercentage,
                                   OfferFreeItemName = a.OfferFreeItemName,
                                   OfferFreeItemImage = a.OfferFreeItemImage,
                                   OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                   OfferMinimumQty = a.OfferMinimumQty,
                                   FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                   FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                   ItemMultiMRPId = a.ItemMultiMRPId,
                                   BillLimitQty = a.BillLimitQty,
                                   Categoryid = a.Categoryid,
                                   SubCategoryId = a.SubCategoryId,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   BaseCategoryId = a.BaseCategoryid,
                                   SellingSku = a.SellingSku,
                                   SellingUnitName = a.SellingUnitName,
                                   IsSensitive = a.IsSensitive,
                                   IsSensitiveMRP = a.IsSensitiveMRP,
                                   UOM = a.UOM,
                                   UnitofQuantity = a.UnitofQuantity,
                                   TradePrice = a.TradePrice,
                                   WholeSalePrice = a.WholeSalePrice
                               }).OrderByDescending(x => x.ItemNumber).ToList();

                }
                item.ItemMasters = new List<ItemDataDC>();
                var formatedData = await ItemValidate(newdata, ActiveCustomer, context, lang);
                item.ItemMasters.AddRange(formatedData);

                return new ItemListDc() { ItemMasters = item.ItemMasters, Status = true };

            }
        }

        [Route("RetailerGetCategoryImage")]
        [HttpGet]
        public async Task<List<CategoryImageDC>> GetCategoryImageByCId(int CategoryId)
        {
            List<CategoryImageDC> categoryImageDC = new List<CategoryImageDC>();
            using (var db = new AuthContext())
            {

                categoryImageDC = db.CategoryImageDB.Where(c => c.CategoryId == CategoryId && c.Deleted == false && c.IsActive == true).Select(x => new CategoryImageDC
                {
                    CategoryImageId = x.CategoryImageId,
                    CategoryImg = x.CategoryImg,

                }).ToList();
            }
            return categoryImageDC;
        }

        [HttpGet]
        [Route("RetailerGetSearchItem")]

        public async Task<HttpResponseMessage> SearchCust(int warehouseId, int customerId, int skip, int take, string lang)
        {
            FilterSearchDc filterSearchDc = new FilterSearchDc();
            using (var authContext = new AuthContext())
            {
                var ActiveCustomer = authContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                try
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    var taskList = new List<Task>();
                    var task1 = Task.Factory.StartNew(() =>
                    {
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetSearchItemForMostSellingProduct]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                        cmd.Parameters.Add(new SqlParameter("@skip", skip));
                        cmd.Parameters.Add(new SqlParameter("@take", take));

                        // Run the sproc
                        var reader = cmd.ExecuteReader();

                        // Read Blogs from the first result set
                        var MostSellingProduct = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<Itemsearch>(reader).ToList();

                        var newdata = Mapper.Map(MostSellingProduct).ToANew<List<ItemDataDC>>();
                        var formatedData = AsyncContext.Run(() => ItemValidate(newdata, ActiveCustomer, authContext, lang));
                        filterSearchDc.MostSellingProduct = Mapper.Map(formatedData).ToANew<List<Itemsearch>>();

                    });
                    taskList.Add(task1);

                    var task2 = Task.Factory.StartNew(() =>
                    {
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetSearchItemForRecentPurchase]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                        cmd.Parameters.Add(new SqlParameter("@skip", skip));
                        cmd.Parameters.Add(new SqlParameter("@take", take));

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var RecentPurchase = ((IObjectContextAdapter)authContext)
                            .ObjectContext
                            .Translate<Itemsearch>(reader).ToList();

                        var newdata = Mapper.Map(RecentPurchase).ToANew<List<ItemDataDC>>();
                        var formatedData = AsyncContext.Run(() => ItemValidate(newdata, ActiveCustomer, authContext, lang));
                        filterSearchDc.RecentPurchase = Mapper.Map(formatedData).ToANew<List<Itemsearch>>();


                    });
                    taskList.Add(task2);

                    var task3 = Task.Factory.StartNew(() =>
                    {
                        List<Itemsearch> CustFavoriteItem = new List<Itemsearch>();
                        var itemids = authContext.CustFavoriteItems.Where(x => x.CustomerId == customerId && x.IsLike).Select(x => x.ItemId).ToList(); ;


                        if (itemids != null && itemids.Any())
                        {
                            List<ElasticSearchItem> elasticSearchItems = new List<ElasticSearchItem>();
                            if (ElasticSearchEnable)
                            {
                                Suggest suggest = null;
                                MongoDbHelper<ElasticSearchQuery> mongoDbHelperElastic = new MongoDbHelper<ElasticSearchQuery>();
                                var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "GetItemForRecentSearch");
                                var searchQuery = mongoDbHelperElastic.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                                var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                                    .Replace("{#warehouseid#}", warehouseId.ToString())
                                    .Replace("{#itemids#}", string.Join(",", itemids))
                                    .Replace("{#from#}", "0")
                                    .Replace("{#size#}", "1000");
                                elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                                CustFavoriteItem = Mapper.Map(elasticSearchItems).ToANew<List<Itemsearch>>();
                            }
                            else
                            {
                                var cmd = authContext.Database.Connection.CreateCommand();
                                cmd.CommandText = "[dbo].[GetSearchItemForCustFavoriteItem]";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                                cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                                cmd.Parameters.Add(new SqlParameter("@skip", skip));
                                cmd.Parameters.Add(new SqlParameter("@take", take));

                                // Run the sproc
                                var reader = cmd.ExecuteReader();
                                CustFavoriteItem = ((IObjectContextAdapter)authContext)
                                .ObjectContext
                                .Translate<Itemsearch>(reader).ToList();
                            }

                            var newdata = Mapper.Map(CustFavoriteItem).ToANew<List<ItemDataDC>>();
                            var formatedData = AsyncContext.Run(() => ItemValidate(newdata, ActiveCustomer, authContext, lang));
                            filterSearchDc.CustFavoriteItem = Mapper.Map(formatedData).ToANew<List<Itemsearch>>();
                        }

                    });
                    taskList.Add(task3);

                    var task4 = Task.Factory.StartNew(() =>
                    {
                        MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
                        List<int> itemIds = mongoDbHelper.Select(x => x.customerId == customerId && !x.IsDeleted, x => x.OrderByDescending(y => y.CreatedDate), skip, take).ToList().SelectMany(x => x.Items).Distinct().ToList();
                        List<Itemsearch> recentSearchItem = new List<Itemsearch>();
                        if (itemIds != null && itemIds.Any())
                        {
                            List<ElasticSearchItem> elasticSearchItems = new List<ElasticSearchItem>();
                            if (ElasticSearchEnable)
                            {
                                Suggest suggest = null;
                                MongoDbHelper<ElasticSearchQuery> mongoDbHelperElastic = new MongoDbHelper<ElasticSearchQuery>();
                                var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "GetItemForRecentSearch");
                                var searchQuery = mongoDbHelperElastic.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                                var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                                    .Replace("{#warehouseid#}", warehouseId.ToString())
                                    .Replace("{#itemids#}", string.Join(",", itemIds))
                                    .Replace("{#from#}", "0")
                                    .Replace("{#size#}", "1000");
                                elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                                recentSearchItem = Mapper.Map(elasticSearchItems).ToANew<List<Itemsearch>>();
                            }
                            else
                            {
                                var orderIdDt = new DataTable();
                                orderIdDt.Columns.Add("IntValue");
                                foreach (var item in itemIds)
                                {
                                    var dr = orderIdDt.NewRow();
                                    dr["IntValue"] = item;
                                    orderIdDt.Rows.Add(dr);
                                }

                                var param = new SqlParameter("recentSearchItemIds", orderIdDt);
                                param.SqlDbType = SqlDbType.Structured;
                                param.TypeName = "dbo.IntValues";
                                var cmd = authContext.Database.Connection.CreateCommand();
                                cmd.CommandText = "[dbo].[GetSearchItemForRecentSearch]";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                                cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                                cmd.Parameters.Add(new SqlParameter("@skip", skip));
                                cmd.Parameters.Add(new SqlParameter("@take", take));
                                cmd.Parameters.Add(param);

                                // Run the sproc
                                var reader1 = cmd.ExecuteReader();
                                recentSearchItem = ((IObjectContextAdapter)authContext)
                                .ObjectContext
                                .Translate<Itemsearch>(reader1).ToList();
                            }
                            filterSearchDc.RecentSearchItem = recentSearchItem.Where(x => x.ItemAppType == 0 || x.ItemAppType == 1).ToList();

                            var newdata = Mapper.Map(recentSearchItem).ToANew<List<ItemDataDC>>();
                            var formatedData = AsyncContext.Run(() => ItemValidate(newdata, ActiveCustomer, authContext, lang));
                            filterSearchDc.RecentSearchItem = Mapper.Map(formatedData).ToANew<List<Itemsearch>>();
                        }
                    });
                    taskList.Add(task4);

                    Task.WaitAll(taskList.ToArray());

                }
                finally
                {
                    authContext.Database.Connection.Close();
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, filterSearchDc);
        }

        [HttpGet]
        [Route("RetailerGetCustomerSearchKeyword")]

        public async Task<HttpResponseMessage> SearchCustomerKeyWord(int customerId, int skip, int take, string lang)
        {
            MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
            List<string> keywords = mongoDbHelper.Select(x => x.customerId == customerId && x.IsDeleted == false, x => x.OrderByDescending(y => y.CreatedDate), skip, take).ToList().Select(x => x.keyword).ToList();
            keywords = keywords.Distinct().ToList();
            if (!string.IsNullOrEmpty(lang) && lang != "en")
            {
                Annotate annotate = new Annotate();
                var converttext = string.Join("|", keywords);
                var hindiText = await annotate.GetTranslatedText(converttext, lang);
                keywords = hindiText.Split('|').ToList();
            }
            return Request.CreateResponse(HttpStatusCode.OK, keywords);

        }

        [Route("RetailerItemSearch")]
        [HttpPost]
        public async Task<WRSITEMs> postitembyitemnamev4(SearchItem searchitem)
        {
            using (var db = new AuthContext())
            {

                if (!string.IsNullOrEmpty(searchitem.BarCode))
                {
                    var Number = db.ItemBarcodes.FirstOrDefault(i => i.Barcode == searchitem.BarCode && i.IsActive == true && i.IsDeleted == false).ItemNumber;
                    if (!string.IsNullOrEmpty(Number))
                    {
                        searchitem.Number = Number;
                    }
                }

                WRSITEMs item = new WRSITEMs();
                List<ItemMaster> itemList = new List<ItemMaster>();
                var ActiveCustomer = db.Customers.FirstOrDefault(x => x.CustomerId == searchitem.CustomerId);
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;


                var newdata = new List<ItemDataDC>();
                List<bool> activelst = new List<bool>();
                //if (!searchitem.IsActive)
                //{
                //    activelst.Add(true);
                //    activelst.Add(false);
                //}
                //else
                //{
                activelst.Add(true);
                // }


                if (ElasticSearchEnable)
                {
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelperElastic = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "ItemSearch");
                    var searchQuery = mongoDbHelperElastic.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();

                    var suggest = new Suggest();
                    string Uomstr = string.IsNullOrEmpty(searchitem.UOM) ? "" : (",{\"term\": {\"uom\":\"{UOM}\"}}").Replace("{UOM}", searchitem.UOM);
                    string Uoq = string.IsNullOrEmpty(searchitem.Unit) ? "" : (",{\"term\": {\"unitofquantity\":{UOQ}}}".Replace("{UOQ}", searchitem.Unit));
                    string catstr = searchitem.Category.Any() ? (",{\"terms\": {\"categoryid\":\"[{cat}]\"}}".Replace("{cat}", string.Join(",", searchitem.Category))) : "";
                    string basecatstr = searchitem.BaseCat.Any() ? (",{\"terms\": {\"basecategoryid\":\"[{basecat}]\"}}".Replace("{basecat}", string.Join(",", searchitem.BaseCat))) : "";
                    string subcatstr = searchitem.SubCat.Any() ? (",{\"terms\": {\"subcategoryid\":\"[{subcat}]\"}}".Replace("{subcat}", string.Join(",", searchitem.SubCat))) : "";
                    string subsubcatstr = searchitem.Brand.Any() ? (",{\"terms\": {\"subsubcategoryid\":\"[{brand}]\"}}".Replace("{brand}", string.Join(",", searchitem.Brand))) : "";
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", warehouseId.ToString())
                        .Replace("{#keyword#}", searchitem.itemkeyword)
                        .Replace("{#minprice#}", searchitem.minPrice.ToString())
                        .Replace("{#maxprice#}", searchitem.maxPrice.ToString())
                        .Replace("{#uom#}", string.IsNullOrEmpty(searchitem.UOM) ? "" : Uomstr)
                        .Replace("{#unitofquantity#}", searchitem.Unit)
                        .Replace("{#category#}", catstr)
                        .Replace("{#basecat#}", basecatstr)
                        .Replace("{#subcat#}", subcatstr)
                        .Replace("{#brand#}", subsubcatstr)
                        .Replace("{#from#}", "0")
                        .Replace("{#size#}", "1000")
                        .Replace("{#active#}", string.Join(",", activelst));
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    if ((elasticSearchItems == null || !elasticSearchItems.Any()) && suggest != null && suggest.namesuggester != null && suggest.namesuggester.Any(x => x.options.Any()))
                    {
                        searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "SuggestionItemSearch");
                        var newsearchQuery = mongoDbHelperElastic.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();


                        // string keyword = string.Join("+", suggest.namesuggester.FirstOrDefault().options.Select(x => x.text).ToList());
                        string keyword = ""; //string.Join("','", suggest.namesuggester.FirstOrDefault().options.Select(x => x.text).ToList());
                        foreach (var opt in suggest.namesuggester.FirstOrDefault().options)
                        {
                            if (!string.IsNullOrEmpty(keyword)) keyword += ",";
                            keyword += "{ \"query_string\": {             \"query\": \"" + opt.text + "*\",            \"fields\": [ \"itemname\", \"categoryname\",\"subcategoryname\",\"subsubcategoryName\",\"hindiname\" ]           }          }";
                        }
                        suggest = new Suggest();
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            query = newsearchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                               .Replace("{#warehouseid#}", warehouseId.ToString())
                               .Replace("{#keyword#}", keyword)
                               .Replace("{#minprice#}", searchitem.minPrice.ToString())
                               .Replace("{#maxprice#}", searchitem.maxPrice.ToString())
                               .Replace("{#uom#}", string.IsNullOrEmpty(searchitem.UOM) ? "" : Uomstr)
                               .Replace("{#unitofquantity#}", searchitem.Unit)
                               .Replace("{#category#}", catstr)
                               .Replace("{#basecat#}", basecatstr)
                               .Replace("{#subcat#}", subcatstr)
                               .Replace("{#brand#}", subsubcatstr)
                               .Replace("{#from#}", "0")
                               .Replace("{#size#}", "1000")
                               .Replace("{#active#}", string.Join(",", activelst));

                            elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                        }
                    }
                    newdata = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                }
                else
                {
                    newdata = (from a in db.itemMasters
                               where a.WarehouseId == warehouseId && (string.IsNullOrEmpty(searchitem.UOM) || a.UOM == searchitem.UOM)
                               && (string.IsNullOrEmpty(searchitem.Unit) || a.UnitofQuantity == searchitem.Unit)
                               && (searchitem.Category.Count == 0 || searchitem.Category.Contains(a.Categoryid))
                               && (searchitem.BaseCat.Count == 0 || searchitem.BaseCat.Contains(a.BaseCategoryid))
                               && (searchitem.SubCat.Count == 0 || searchitem.SubCat.Contains(a.SubCategoryId))
                               && (searchitem.Brand.Count == 0 || searchitem.Brand.Contains(a.SubsubCategoryid))
                               && a.Deleted == false
                               && !a.IsDisContinued
                               && activelst.Contains(a.active)
                               //&& !excludecategory.Contains(a.Categoryid)
                               && a.UnitPrice >= searchitem.minPrice
                               && a.UnitPrice <= searchitem.maxPrice
                               && (a.itemname.Trim().ToLower().Contains(searchitem.itemkeyword.Trim().ToLower())
                               || (a.Number.Trim().ToLower().Contains(searchitem.Number.Trim().ToLower()))
                               || a.CategoryName.Trim().ToLower().Contains(searchitem.itemkeyword.Trim().ToLower())
                               || (!string.IsNullOrEmpty(a.Description) && a.Description.Trim().ToLower().Contains(searchitem.itemkeyword.Trim().ToLower()))
                               )
                               && (a.ItemAppType == 0 || a.ItemAppType == 1)
                               let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                               select new ItemDataDC
                               {
                                   BaseCategoryId = a.BaseCategoryid,
                                   IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                   ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                   WarehouseId = a.WarehouseId,
                                   CompanyId = a.CompanyId,
                                   Categoryid = a.Categoryid,
                                   Discount = a.Discount,
                                   ItemId = a.ItemId,
                                   ItemNumber = a.Number,
                                   HindiName = a.HindiName,
                                   IsSensitive = a.IsSensitive,
                                   IsSensitiveMRP = a.IsSensitiveMRP,
                                   UnitofQuantity = a.UnitofQuantity,
                                   UOM = a.UOM,
                                   itemname = a.itemname,
                                   LogoUrl = a.LogoUrl,
                                   MinOrderQty = a.MinOrderQty,
                                   price = a.price,
                                   SubCategoryId = a.SubCategoryId,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   TotalTaxPercentage = a.TotalTaxPercentage,
                                   SellingUnitName = a.SellingUnitName,
                                   SellingSku = a.SellingSku,
                                   UnitPrice = a.UnitPrice,
                                   VATTax = a.VATTax,
                                   itemBaseName = a.itemBaseName,
                                   active = a.active,
                                   marginPoint = a.marginPoint,
                                   promoPerItems = a.promoPerItems,
                                   NetPurchasePrice = a.NetPurchasePrice,
                                   IsOffer = a.IsOffer,
                                   Deleted = a.Deleted,
                                   OfferCategory = a.OfferCategory,
                                   OfferStartTime = a.OfferStartTime,
                                   OfferEndTime = a.OfferEndTime,
                                   OfferQtyAvaiable = a.OfferQtyAvaiable,
                                   OfferQtyConsumed = a.OfferQtyConsumed,
                                   OfferId = a.OfferId,
                                   OfferType = a.OfferType,
                                   OfferWalletPoint = a.OfferWalletPoint,
                                   OfferFreeItemId = a.OfferFreeItemId,
                                   OfferPercentage = a.OfferPercentage,
                                   OfferFreeItemName = a.OfferFreeItemName,
                                   OfferFreeItemImage = a.OfferFreeItemImage,
                                   OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                   OfferMinimumQty = a.OfferMinimumQty,
                                   FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                   FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                   FreeItemId = a.OfferFreeItemId,
                                   BillLimitQty = a.BillLimitQty,
                                   ItemMultiMRPId = a.ItemMultiMRPId,
                                   TradePrice = a.TradePrice,
                                   WholeSalePrice = a.WholeSalePrice
                               }).OrderByDescending(x => x.ItemNumber).ToList();
                }
                item.ItemMasters = new List<factoryItemdata>();
                var formatedData = await ItemValidate(newdata, ActiveCustomer, db, searchitem.lang);
                List<factoryItemdata> Data = Mapper.Map(formatedData).ToANew<List<factoryItemdata>>();
                item.ItemMasters.AddRange(Data);

                if (item.ItemMasters != null && item.ItemMasters.Any())
                {
                    List<int> itemIds = item.ItemMasters.Select(x => x.ItemId).ToList();
                    BackgroundTaskManager.Run(() =>
                    {
                        MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
                        CustomerProductSearch customerProductSearch = new CustomerProductSearch
                        {
                            CreatedDate = indianTime,
                            customerId = searchitem.CustomerId,
                            keyword = searchitem.itemkeyword,
                            Items = itemIds,
                            IsDeleted = false
                        };
                        mongoDbHelper.Insert(customerProductSearch);
                    });
                }
                if (item.ItemMasters != null)
                {
                    item.Message = "Success";
                    item.Status = true;
                    return item;
                }
                else
                {
                    item.Status = false;
                    item.Message = "Failed";
                    return item;
                }
            }
        }

        [Route("RetailerCategory")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<customeritems> RetailerCategory(int warehouseid, int customerId, string lang)
        {
            customeritems ibjtosend = new customeritems();

            using (var unitOfWork = new DataLayer.Infrastructure.UnitOfWork())
            {
                string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
                DataContracts.KPPApp.customeritem CatSubCatBrands = await unitOfWork.KPPAppRepository.GetRetailCatSubCatAsync(warehouseid, 0, ConsumerApptype);
                ibjtosend.Basecats = CatSubCatBrands.Basecats.Select(x => new Basecats
                {
                    BaseCategoryId = x.BaseCategoryId,
                    BaseCategoryName = lang == "hi" && !string.IsNullOrEmpty(x.HindiName) && x.HindiName != "{nan}" ? x.HindiName : x.BaseCategoryName,
                    HindiName = x.HindiName,
                    LogoUrl = x.LogoUrl
                });
                ibjtosend.Categories = CatSubCatBrands.Categories.Select(x => new Categories
                {
                    BaseCategoryId = x.BaseCategoryId,
                    Categoryid = x.Categoryid,
                    CategoryName = lang == "hi" && !string.IsNullOrEmpty(x.HindiName) && x.HindiName != "{nan}" ? x.HindiName : x.CategoryName,
                    LogoUrl = x.LogoUrl
                }).ToList();

                ibjtosend.SubCategories = CatSubCatBrands.SubCategories.Select(x => new AngularJSAuthentication.API.Controllers.SubCategories
                {
                    Categoryid = x.Categoryid,
                    HindiName = x.HindiName,
                    itemcount = x.itemcount,
                    LogoUrl = x.LogoUrl,
                    SubCategoryId = x.SubCategoryId,
                    SubcategoryName = lang == "hi" && !string.IsNullOrEmpty(x.HindiName) && x.HindiName != "{nan}" ? x.HindiName : x.SubcategoryName,
                }).ToList();
                ibjtosend.SubSubCategories = CatSubCatBrands.SubSubCategories.Select(x => new SubSubCategories
                {
                    SubCategoryId = x.SubCategoryId,
                    SubSubCategoryId = x.SubSubCategoryId,
                    SubSubcategoryName = lang == "hi" && !string.IsNullOrEmpty(x.HindiName) && x.HindiName != "{nan}" ? x.HindiName : x.SubSubcategoryName,
                    Categoryid = x.Categoryid,
                    LogoUrl = x.LogoUrl,
                    itemcount = x.itemcount
                }).ToList();

            }

            if (ibjtosend.SubSubCategories != null && ibjtosend.SubSubCategories.Any())
            {
                using (var db = new AuthContext())
                {
                    var ActiveCustomer = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.IsKPP }).FirstOrDefault();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd

                    var custtype = ActiveCustomer.IsKPP ? 1 : 2;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, warehouseid);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        ibjtosend.SubSubCategories = ibjtosend.SubSubCategories.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.SubSubCategoryId))).ToList();
                        ibjtosend.SubCategories = ibjtosend.SubCategories.Where(x => ibjtosend.SubSubCategories.Select(y => y.Categoryid + " " + y.SubCategoryId).Contains(x.Categoryid + " " + x.SubCategoryId)).ToList();
                        ibjtosend.Categories = ibjtosend.Categories.Where(x => ibjtosend.SubCategories.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();
                        ibjtosend.Basecats = ibjtosend.Basecats.Where(x => ibjtosend.Categories.Select(y => y.BaseCategoryId).Contains(x.BaseCategoryId)).ToList();
                    }
                    #endregion
                }
            }


            if ((ibjtosend.SubSubCategories.Any() || ibjtosend.SubCategories.Any() || ibjtosend.Categories.Any() || ibjtosend.Basecats.Any()) && EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
            {
                List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                ElasticLanguageDataRequests = ibjtosend.Basecats.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.BaseCategoryName }).ToList();
                ElasticLanguageDataRequests.AddRange(ibjtosend.Categories.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.CategoryName }).ToList());
                ElasticLanguageDataRequests.AddRange(ibjtosend.SubCategories.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubcategoryName }).ToList());
                ElasticLanguageDataRequests.AddRange(ibjtosend.SubSubCategories.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubSubcategoryName }).ToList());

                LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                ibjtosend.Basecats.ToList().ForEach(x => { x.BaseCategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.BaseCategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.BaseCategoryName).converttext : x.BaseCategoryName; });
                ibjtosend.Categories.ToList().ForEach(x => { x.CategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.CategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.CategoryName).converttext : x.CategoryName; });
                ibjtosend.SubCategories.ToList().ForEach(x => { x.SubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubcategoryName).converttext : x.SubcategoryName; });
                ibjtosend.SubSubCategories.ToList().ForEach(x => { x.SubSubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubSubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubSubcategoryName).converttext : x.SubSubcategoryName; });

            }


            return ibjtosend;

        }


        [Route("RetailerHomePageGetCategories")]
        [HttpGet]
        public async Task<CatScatSscatDCs> GetCategories(string lang, int itemId, int wid, int customerId)
        {
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            var catScatSscatDCs = await retailerAppManager.GetCategories(lang, itemId, wid);
            if (catScatSscatDCs.subsubCategoryDc != null && catScatSscatDCs.subsubCategoryDc.Any())
            {
                using (var db = new AuthContext())
                {
                    var ActiveCustomer = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.IsKPP }).FirstOrDefault();
                    ;
                    #region block Barnd

                    var custtype = ActiveCustomer.IsKPP ? 1 : 2;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, wid);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        catScatSscatDCs.subsubCategoryDc = catScatSscatDCs.subsubCategoryDc.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.SubsubCategoryid))).ToList();
                        catScatSscatDCs.subCategoryDC = catScatSscatDCs.subCategoryDC.Where(x => catScatSscatDCs.subsubCategoryDc.Select(y => y.Categoryid + " " + y.SubCategoryId).Contains(x.Categoryid + " " + x.SubCategoryId)).ToList();
                        catScatSscatDCs.categoryDC = catScatSscatDCs.categoryDC.Where(x => catScatSscatDCs.subCategoryDC.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();
                    }
                    #endregion
                }


                if (EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                {
                    List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                    ElasticLanguageDataRequests.AddRange(catScatSscatDCs.categoryDC.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.CategoryName }).ToList());
                    ElasticLanguageDataRequests.AddRange(catScatSscatDCs.subCategoryDC.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubcategoryName }).ToList());
                    ElasticLanguageDataRequests.AddRange(catScatSscatDCs.subsubCategoryDc.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubsubcategoryName }).ToList());

                    LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                    var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                    catScatSscatDCs.categoryDC.ToList().ForEach(x => { x.CategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.CategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.CategoryName).converttext : x.CategoryName; });
                    catScatSscatDCs.subCategoryDC.ToList().ForEach(x => { x.SubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubcategoryName).converttext : x.SubcategoryName; });
                    catScatSscatDCs.subsubCategoryDc.ToList().ForEach(x => { x.SubsubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubsubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubsubcategoryName).converttext : x.SubsubcategoryName; });

                }
            }
            return catScatSscatDCs;
        }


        [Route("GetStoreCategories")]
        [HttpGet]
        public async Task<CatScatSscatDCs> GetStoreCategories(string lang, int baseCategoryId, int subCategoryId, int wid, int customerId)
        {
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            var catScatSscatDCs = await retailerAppManager.GetStoreCategories(lang, baseCategoryId, subCategoryId, wid);
            if (catScatSscatDCs.subsubCategoryDc != null && catScatSscatDCs.subsubCategoryDc.Any())
            {
                using (var db = new AuthContext())
                {
                    var ActiveCustomer = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.IsKPP }).FirstOrDefault();

                    #region block Barnd

                    var custtype = ActiveCustomer.IsKPP ? 1 : 2;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, wid);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        catScatSscatDCs.subsubCategoryDc = catScatSscatDCs.subsubCategoryDc.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.SubsubCategoryid))).ToList();
                        catScatSscatDCs.subCategoryDC = catScatSscatDCs.subCategoryDC.Where(x => catScatSscatDCs.subsubCategoryDc.Select(y => y.Categoryid + " " + y.SubCategoryId).Contains(x.Categoryid + " " + x.SubCategoryId)).ToList();
                        catScatSscatDCs.categoryDC = catScatSscatDCs.categoryDC.Where(x => catScatSscatDCs.subCategoryDC.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();
                    }
                    #endregion
                }

                if (EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                {
                    List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                    ElasticLanguageDataRequests.AddRange(catScatSscatDCs.categoryDC.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.CategoryName }).ToList());
                    ElasticLanguageDataRequests.AddRange(catScatSscatDCs.subCategoryDC.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubcategoryName }).ToList());
                    ElasticLanguageDataRequests.AddRange(catScatSscatDCs.subsubCategoryDc.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubsubcategoryName }).ToList());

                    LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                    var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                    catScatSscatDCs.categoryDC.ToList().ForEach(x => { x.CategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.CategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.CategoryName).converttext : x.CategoryName; });
                    catScatSscatDCs.subCategoryDC.ToList().ForEach(x => { x.SubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubcategoryName).converttext : x.SubcategoryName; });
                    catScatSscatDCs.subsubCategoryDc.ToList().ForEach(x => { x.SubsubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubsubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubsubcategoryName).converttext : x.SubsubcategoryName; });

                }
            }
            return catScatSscatDCs;
        }


        [Route("RetailerHomePageGetSubSubCategories")]
        [HttpGet]
        public async Task<CatScatSscatDCs> GetSubCategories(string lang, int subCategoryId, int wid, int customerId)
        {
            using (var db = new AuthContext())
            {
                RetailerAppManager retailerAppManager = new RetailerAppManager();

                List<Category> Cat = new List<Category>();
                List<SubCategory> Scat = new List<SubCategory>();
                List<SubsubCategory> SsCat = new List<SubsubCategory>();
                try
                {
                    var subCategoryQuery = "select SubCategoryId, 0 Categoryid,  '' CategoryName, [SubCategoryId],  (Case when '" + lang + "'='hi' and ( HindiName is not null or HindiName='') then HindiName else SubcategoryName end) SubcategoryName ,[LogoUrl],[itemcount],StoreBanner from SubCategories where IsActive=1 and Deleted=0 and SubCategoryId=" + subCategoryId;


                    var brandQuery = "Exec GetRetailerBrandBySubCategoryId " + wid + "," + subCategoryId + "," + lang;
                    var Scatv = db.Database.SqlQuery<SubCategoryDCs>(subCategoryQuery).ToList();
                    var SsCatv = db.Database.SqlQuery<SubsubCategoryDcs>(brandQuery).ToList();

                    CatScatSscatDCs CatScatSscatcdc = new CatScatSscatDCs
                    {
                        subCategoryDC = Mapper.Map(Scatv).ToANew<List<SubCategoryDCs>>(),
                        subsubCategoryDc = Mapper.Map(SsCatv).ToANew<List<SubsubCategoryDcs>>(),
                    };

                    if (CatScatSscatcdc.subsubCategoryDc != null && CatScatSscatcdc.subsubCategoryDc.Any())
                    {

                        var ActiveCustomer = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.IsKPP }).FirstOrDefault();
                        #region block Barnd

                        var custtype = ActiveCustomer.IsKPP ? 1 : 2;
                        var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, wid);
                        if (blockBarnds != null && blockBarnds.Any())
                        {
                            CatScatSscatcdc.subsubCategoryDc = CatScatSscatcdc.subsubCategoryDc.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.SubsubCategoryid))).ToList();
                            CatScatSscatcdc.subCategoryDC = CatScatSscatcdc.subCategoryDC.Where(x => CatScatSscatcdc.subsubCategoryDc.Select(y => y.Categoryid + " " + y.SubCategoryId).Contains(x.Categoryid + " " + x.SubCategoryId)).ToList();
                        }
                        #endregion

                    }

                    if ((CatScatSscatcdc.subsubCategoryDc.Any() || CatScatSscatcdc.subCategoryDC.Any()) && EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                    {
                        List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                        ElasticLanguageDataRequests.AddRange(CatScatSscatcdc.subCategoryDC.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubcategoryName }).ToList());
                        ElasticLanguageDataRequests.AddRange(CatScatSscatcdc.subsubCategoryDc.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubsubcategoryName }).ToList());

                        LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                        var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                        CatScatSscatcdc.subCategoryDC.ToList().ForEach(x => { x.SubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubcategoryName).converttext : x.SubcategoryName; });
                        CatScatSscatcdc.subsubCategoryDc.ToList().ForEach(x => { x.SubsubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubsubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubsubcategoryName).converttext : x.SubsubcategoryName; });

                    }

                    return CatScatSscatcdc;
                }
                catch (Exception ex)
                {
                    CustomersController.logger.Error("Error in AppHomeDynamic " + ex.Message);
                    return null;
                }
            }
        }

        //Freebies API
        [Route("RetailerGetOfferItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemListDc> RetailerGetOfferItem(int WarehouseId, int CustomerId, string lang = "en")
        {
            using (var context = new AuthContext())
            {
                DateTime CurrentDate = indianTime;
                var inActiveCustomer = false;

                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;

                ItemListDc res;
                ItemListDc item = new ItemListDc();
                item.ItemMasters = new List<ItemDataDC>();
                List<ItemDataDC> itemMasters = new List<ItemDataDC>();

                if (ElasticSearchEnable)
                {
                    Suggest suggest = null;
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetOfferItem");
                    var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", WarehouseId.ToString())
                        .Replace("{#offerstarttime#}", CurrentDate.ToString("yyyy-MM-dd'T'HH:mm:ss"))
                        .Replace("{#offerenddate#}", indianTime.ToString("yyyy-MM-dd'T'HH:mm:ss"))
                        .Replace("{#from#}", "0")
                        .Replace("{#size#}", "1000");
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    itemMasters = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                }
                else
                {

                    itemMasters = (from a in context.itemMasters
                                   where (a.WarehouseId == WarehouseId && a.OfferStartTime <= CurrentDate
                                   && a.OfferEndTime >= indianTime && a.OfferCategory == 1 && a.active == true && a.Deleted == false && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                   join c in context.OfferDb on a.OfferId equals c.OfferId
                                   where (c.IsActive == true && c.IsDeleted == false && (c.OfferAppType == "Retailer App" || c.OfferAppType == "Both"))
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new ItemDataDC
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       itemBaseName = a.itemBaseName,
                                       //itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                       itemname = a.itemname,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName != null ? a.HindiName : a.itemname,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       NetPurchasePrice = a.NetPurchasePrice,
                                       promoPerItems = a.promoPerItems,
                                       IsOffer = a.IsOffer,
                                       Deleted = a.Deleted,
                                       OfferCategory = a.OfferCategory,
                                       OfferStartTime = a.OfferStartTime,
                                       OfferEndTime = a.OfferEndTime,
                                       OfferQtyAvaiable = a.OfferQtyAvaiable,
                                       OfferQtyConsumed = a.OfferQtyConsumed,
                                       OfferId = a.OfferId,
                                       OfferType = a.OfferType,
                                       OfferWalletPoint = a.OfferWalletPoint,
                                       OfferFreeItemId = a.OfferFreeItemId,
                                       OfferPercentage = a.OfferPercentage,
                                       OfferFreeItemName = a.OfferFreeItemName,
                                       OfferFreeItemImage = a.OfferFreeItemImage,
                                       OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                       OfferMinimumQty = a.OfferMinimumQty,
                                       FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       BillLimitQty = a.BillLimitQty,
                                       ItemAppType = a.ItemAppType,
                                       Categoryid = a.Categoryid,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       BaseCategoryId = a.BaseCategoryid,
                                       SellingSku = a.SellingSku,
                                       SellingUnitName = a.SellingUnitName,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       TradePrice = a.TradePrice,
                                       WholeSalePrice = a.WholeSalePrice
                                   }).OrderByDescending(x => x.ItemNumber).ToList();

                }

                item.ItemMasters = new List<ItemDataDC>();
                var formatedData = await ItemValidate(itemMasters, ActiveCustomer, context, lang);
                item.ItemMasters.AddRange(formatedData);

                if (item.ItemMasters.Count() != 0)
                {
                    res = new ItemListDc
                    {
                        ItemMasters = item.ItemMasters,
                        Status = true,
                        Message = "Success."
                    };
                    return res;
                }
                else
                {
                    res = new ItemListDc
                    {
                        ItemMasters = itemMasters,
                        Status = false,
                        Message = "Fail"
                    };
                    return res;
                }
            }
        }

        //Freebies API
        [Route("RetailerGetStoreOfferItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemListDc> RetailerGetStoreOfferItem(int WarehouseId, int CustomerId, int SubCategoryId, string lang = "en")
        {
            using (var context = new AuthContext())
            {
                DateTime CurrentDate = indianTime;
                var inActiveCustomer = false;

                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;


                ItemListDc res;
                ItemListDc item = new ItemListDc();
                item.ItemMasters = new List<ItemDataDC>();
                List<ItemDataDC> itemMasters = new List<ItemDataDC>();

                if (ElasticSearchEnable)
                {
                    Suggest suggest = null;
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetOfferItem");
                    var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", WarehouseId.ToString())
                        .Replace("{#offerstarttime#}", CurrentDate.ToString("yyyy-MM-dd'T'HH:mm:ss"))
                        .Replace("{#offerenddate#}", indianTime.ToString("yyyy-MM-dd'T'HH:mm:ss"))
                        .Replace("{#from#}", "0")
                        .Replace("{#size#}", "1000");
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    itemMasters = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                }
                else
                {
                    itemMasters = (from a in context.itemMasters
                                   where (a.WarehouseId == WarehouseId && a.SubCategoryId == SubCategoryId && a.OfferStartTime <= CurrentDate
                                   && a.OfferEndTime >= indianTime && a.OfferCategory == 1 && a.active == true && a.Deleted == false && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                   join c in context.OfferDb on a.OfferId equals c.OfferId
                                   where (c.IsActive == true && c.IsDeleted == false && (c.OfferAppType == "Retailer App" || c.OfferAppType == "Both"))
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new ItemDataDC
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       itemname = a.itemname,
                                       itemBaseName = a.itemBaseName,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName != null ? a.HindiName : a.itemname,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       NetPurchasePrice = a.NetPurchasePrice,
                                       promoPerItems = a.promoPerItems,
                                       IsOffer = a.IsOffer,
                                       Deleted = a.Deleted,
                                       OfferCategory = a.OfferCategory,
                                       OfferStartTime = a.OfferStartTime,
                                       OfferEndTime = a.OfferEndTime,
                                       OfferQtyAvaiable = a.OfferQtyAvaiable,
                                       OfferQtyConsumed = a.OfferQtyConsumed,
                                       OfferId = a.OfferId,
                                       OfferType = a.OfferType,
                                       OfferWalletPoint = a.OfferWalletPoint,
                                       OfferFreeItemId = a.OfferFreeItemId,
                                       OfferPercentage = a.OfferPercentage,
                                       OfferFreeItemName = a.OfferFreeItemName,
                                       OfferFreeItemImage = a.OfferFreeItemImage,
                                       OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                       OfferMinimumQty = a.OfferMinimumQty,
                                       FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       BillLimitQty = a.BillLimitQty,
                                       ItemAppType = a.ItemAppType,
                                       Categoryid = a.Categoryid,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       BaseCategoryId = a.BaseCategoryid,
                                       SellingSku = a.SellingSku,
                                       SellingUnitName = a.SellingUnitName,
                                       TradePrice = a.TradePrice,
                                       WholeSalePrice = a.WholeSalePrice
                                   }).OrderByDescending(x => x.ItemNumber).ToList();
                }

                item.ItemMasters = new List<ItemDataDC>();
                var formatedData = await ItemValidate(itemMasters, ActiveCustomer, context, lang);
                item.ItemMasters.AddRange(formatedData);

                if (item.ItemMasters.Count() != 0)
                {
                    res = new ItemListDc
                    {
                        ItemMasters = item.ItemMasters,
                        Status = true,
                        Message = "Success."
                    };
                    return res;
                }
                else
                {
                    res = new ItemListDc
                    {
                        ItemMasters = itemMasters,
                        Status = false,
                        Message = "Fail"
                    };
                    return res;
                }
            }
        }



        [Route("RetailerGetItembycatesscatid")]
        [HttpGet]
        public async Task<ItemListDc> RetailerGetItembycatesscatid(string lang, int customerId, int catid, int scatid, int sscatid, string sortType, string direction, int skip = 0, int take = 10)
        {
            RetailerAppManager manager = new RetailerAppManager();
            return await manager.RetailerGetItembycatesscatid(lang, customerId, catid, scatid, sscatid, skip, take, sortType, direction);
        }

        [Route("RetailerGetItembySubCatAndBrand")]
        [HttpGet]
        public async Task<ItemListDc> RetailerGetItembySubCatAndBrand(string lang, int customerId, int scatid, int sscatid)
        {
            using (var context = new AuthContext())
            {
                List<ItemListDc> brandItem = new List<ItemListDc>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;
                ItemListDc item = new ItemListDc();
                List<ItemDataDC> newdata = new List<ItemDataDC>();
                if (ElasticSearchEnable)
                {
                    Suggest suggest = null;
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetItembySubCatAndBrand");
                    var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", warehouseId.ToString())
                        .Replace("{#subcategoryid#}", scatid.ToString())
                        .Replace("{#subsubcategoryid#}", sscatid.ToString())
                        .Replace("{#from#}", "0")
                        .Replace("{#size#}", "1000");
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    newdata = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                }
                else
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();


                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetItemBySubCatAndBrand]";
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                    cmd.Parameters.Add(new SqlParameter("@sscatid", sscatid));
                    cmd.Parameters.Add(new SqlParameter("@scatid", scatid));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ItemDataDC>(reader).ToList();

                    if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                    {
                        newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                             Select(x => new ItemDataDC
                             {
                                 active = x.FirstOrDefault().active,
                                 WarehouseId = x.Key.WarehouseId,
                                 ItemMultiMRPId = x.Key.ItemMultiMRPId,
                                 ItemNumber = x.Key.ItemNumber,
                                 BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                                 BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                                 BillLimitQty = x.FirstOrDefault().BillLimitQty,
                                 Categoryid = x.FirstOrDefault().Categoryid,
                                 Classification = x.FirstOrDefault().Classification,
                                 CompanyId = x.FirstOrDefault().CompanyId,
                                 CurrentStartTime = x.FirstOrDefault().CurrentStartTime,
                                 Deleted = x.FirstOrDefault().Deleted,
                                 Discount = x.FirstOrDefault().Discount,
                                 DistributionPrice = x.FirstOrDefault().DistributionPrice,
                                 DistributorShow = x.FirstOrDefault().DistributorShow,
                                 dreamPoint = x.FirstOrDefault().dreamPoint,
                                 FlashDealMaxQtyPersonCanTake = x.FirstOrDefault().FlashDealMaxQtyPersonCanTake,
                                 FlashDealSpecialPrice = x.FirstOrDefault().FlashDealSpecialPrice,
                                 FreeItemId = x.FirstOrDefault().FreeItemId,
                                 HindiName = x.FirstOrDefault().HindiName,
                                 IsFlashDealStart = x.FirstOrDefault().IsFlashDealStart,
                                 IsFlashDealUsed = x.FirstOrDefault().IsFlashDealUsed,
                                 IsItemLimit = x.FirstOrDefault().IsItemLimit,
                                 IsOffer = x.FirstOrDefault().IsOffer,
                                 IsPrimeItem = x.FirstOrDefault().IsPrimeItem,
                                 IsSensitive = x.FirstOrDefault().IsSensitive,
                                 IsSensitiveMRP = x.FirstOrDefault().IsSensitiveMRP,
                                 ItemAppType = x.FirstOrDefault().ItemAppType,
                                 itemBaseName = x.FirstOrDefault().itemBaseName,
                                 ItemId = x.FirstOrDefault().ItemId,
                                 ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                                 itemname = x.FirstOrDefault().itemname,
                                 Itemtype = x.FirstOrDefault().Itemtype,
                                 LastOrderDate = x.FirstOrDefault().LastOrderDate,
                                 LastOrderDays = x.FirstOrDefault().LastOrderDays,
                                 LastOrderQty = x.FirstOrDefault().LastOrderQty,
                                 LogoUrl = x.FirstOrDefault().LogoUrl,
                                 marginPoint = x.FirstOrDefault().marginPoint,
                                 MinOrderQty = 1,
                                 MRP = x.FirstOrDefault().MRP,
                                 NetPurchasePrice = x.FirstOrDefault().NetPurchasePrice,
                                 NoPrimeOfferStartTime = x.FirstOrDefault().NoPrimeOfferStartTime,
                                 Number = x.FirstOrDefault().Number,
                                 OfferCategory = x.FirstOrDefault().OfferCategory,
                                 OfferEndTime = x.FirstOrDefault().OfferEndTime,
                                 OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                                 OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                                 OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                                 OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                                 OfferId = x.FirstOrDefault().OfferId,
                                 OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                                 OfferPercentage = x.FirstOrDefault().OfferPercentage,
                                 OfferQtyAvaiable = x.FirstOrDefault().OfferQtyAvaiable,
                                 OfferQtyConsumed = x.FirstOrDefault().OfferQtyConsumed,
                                 OfferStartTime = x.FirstOrDefault().OfferStartTime,
                                 OfferType = x.FirstOrDefault().OfferType,
                                 OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                                 price = x.FirstOrDefault().price,
                                 PrimePrice = x.FirstOrDefault().PrimePrice,
                                 promoPerItems = x.FirstOrDefault().promoPerItems,
                                 PurchaseValue = x.FirstOrDefault().PurchaseValue,
                                 Rating = x.FirstOrDefault().Rating,
                                 Scheme = x.FirstOrDefault().Scheme,
                                 SellingSku = x.FirstOrDefault().SellingSku,
                                 SellingUnitName = x.FirstOrDefault().SellingUnitName,
                                 Sequence = x.FirstOrDefault().Sequence,
                                 SubCategoryId = x.FirstOrDefault().SubCategoryId,
                                 SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                                 TotalAmt = x.FirstOrDefault().TotalAmt,
                                 TotalTaxPercentage = x.FirstOrDefault().TotalTaxPercentage,
                                 TradePrice = x.FirstOrDefault().TradePrice,
                                 UnitofQuantity = x.FirstOrDefault().UnitofQuantity,
                                 UnitPrice = x.FirstOrDefault().UnitPrice,
                                 UOM = x.FirstOrDefault().UOM,
                                 VATTax = x.FirstOrDefault().VATTax,
                                 WholeSalePrice = x.FirstOrDefault().WholeSalePrice
                             })
                            .ToList();
                    }

                }

                item.ItemMasters = new List<ItemDataDC>();
                var formatedData = await ItemValidate(newdata, ActiveCustomer, context, lang);
                item.ItemMasters.AddRange(formatedData);

                ItemListDc res = new ItemListDc();
                if (item.ItemMasters != null && item.ItemMasters.Any())
                {
                    res.Status = true;
                    res.Message = "Success";
                    res.ItemMasters = item.ItemMasters;
                    return res;
                }
                else
                {
                    res.Status = false;
                    res.Message = "Failed";
                    return res;
                }


            }
        }


        [Route("RetailerGetRelatedItem")]
        [HttpPost]
        public async Task<RelatedItemSearchlist> RetailerGetRelatedItem(RelatedItem ri)
        {
            RelatedItemSearchlist res;
            var ItemSearch = new List<ItemDataDC>();
            using (var authContext = new AuthContext())
            {
                var ActiveCustomer = authContext.Customers.FirstOrDefault(x => x.CustomerId == ri.customerId);
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;

                try
                {

                    if (ElasticSearchEnable)
                    {
                        Suggest suggest = null;
                        MongoDbHelper<ElasticSearchQuery> mongoDbHelperElastic = new MongoDbHelper<ElasticSearchQuery>();
                        var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "GetItemForRecentSearch");
                        var searchQuery = mongoDbHelperElastic.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                        var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                            .Replace("{#warehouseid#}", ri.warehouseId.ToString())
                            .Replace("{#itemids#}", string.Join(",", ri.itemIds))
                            .Replace("{#from#}", ri.skip.ToString())
                            .Replace("{#size#}", ri.take.ToString());
                        List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                        ItemSearch = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                    }
                    else
                    {
                        if (authContext.Database.Connection.State != ConnectionState.Open)
                            authContext.Database.Connection.Open();

                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("IntValue");
                        foreach (var item in ri.itemIds)
                        {
                            var dr = orderIdDt.NewRow();
                            dr["IntValue"] = item;
                            orderIdDt.Rows.Add(dr);
                        }


                        var param = new SqlParameter("ItemIds", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = authContext.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetRelatedItem]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", ri.warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@skip", ri.skip));
                        cmd.Parameters.Add(new SqlParameter("@take", ri.take));
                        cmd.Parameters.Add(param);

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        ItemSearch = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<ItemDataDC>(reader).ToList();
                    }
                    ItemSearch = await ItemValidate(ItemSearch, ActiveCustomer, authContext, ri.lang);
                }
                finally
                {
                    authContext.Database.Connection.Close(); ;
                }
            }
            res = new RelatedItemSearchlist
            {
                relatedItemSearch = Mapper.Map(ItemSearch).ToANew<List<RelatedItemSearch>>(),
                Status = true,
                Message = "Success",
            };
            return res;
        }


        [Route("RetailerCommonDiscountOffer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<OfferdataDc> RetailerCommonDiscountOffer(int CustomerId, string lang = "en")
        {
            List<OfferDc> FinalBillDiscount = new List<OfferDc>();
            OfferdataDc res;
            using (AuthContext context = new AuthContext())
            {
                CustomersManager manager = new CustomersManager();

                List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetWareBillDiscount(CustomerId, "Retailer App");
                if (billDiscountOfferDcs.Any())
                {
                    var offerIds = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();
                    List<BillDiscountFreeItem> BillDiscountFreeItems = offerIds.Any() ? context.BillDiscountFreeItem.Where(x => offerIds.Contains(x.offerId) && x.RemainingOfferStockQty < x.OfferStockQty).ToList() : new List<BillDiscountFreeItem>();
                    var offertypeConfigs = context.OfferTypeDefaultConfigs.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                    foreach (var billDiscountOfferDc in billDiscountOfferDcs)
                    {
                        var OfferDefaultdata = billDiscountOfferDc.OfferOn == "Item" ? offertypeConfigs.Where(x => x.OfferType == billDiscountOfferDc.OfferOn && x.IsActive == true && x.IsDeleted == false).FirstOrDefault()
                            : offertypeConfigs.Where(x => x.OfferType == billDiscountOfferDc.OfferOn && x.DiscountOn == billDiscountOfferDc.BillDiscountOfferOn && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        OfferDefaultdata = OfferDefaultdata != null ? OfferDefaultdata : new Model.SalesApp.OfferTypeDefaultConfig();
                        var bdcheck = new OfferDc
                        {
                            OfferId = billDiscountOfferDc.OfferId,

                            OfferName = billDiscountOfferDc.OfferName,
                            OfferCode = billDiscountOfferDc.OfferCode,
                            OfferCategory = billDiscountOfferDc.OfferCategory,
                            OfferOn = billDiscountOfferDc.OfferOn,
                            start = billDiscountOfferDc.start,
                            end = billDiscountOfferDc.end,
                            DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
                            BillAmount = billDiscountOfferDc.BillAmount,
                            LineItem = billDiscountOfferDc.LineItem,
                            Description = billDiscountOfferDc.Description,
                            BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
                            BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
                            IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
                            IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
                            IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
                            BillDiscountType = billDiscountOfferDc.BillDiscountType,
                            OfferAppType = billDiscountOfferDc.OfferAppType,
                            ApplyOn = billDiscountOfferDc.ApplyOn,
                            WalletType = billDiscountOfferDc.WalletType,
                            MaxDiscount = billDiscountOfferDc.MaxDiscount,
                            ApplyType = billDiscountOfferDc.ApplyType,
                            ColorCode = !string.IsNullOrEmpty(billDiscountOfferDc.ColorCode) ? billDiscountOfferDc.ColorCode : OfferDefaultdata.ColorCode,
                            ImagePath = !string.IsNullOrEmpty(billDiscountOfferDc.ImagePath) ? billDiscountOfferDc.ImagePath : OfferDefaultdata.ImagePath,
                            IsBillDiscountFreebiesItem = billDiscountOfferDc.IsBillDiscountFreebiesItem,
                            IsBillDiscountFreebiesValue = billDiscountOfferDc.IsBillDiscountFreebiesValue,
                            offeritemname = billDiscountOfferDc.offeritemname,
                            offerminorderquantity = billDiscountOfferDc.offerminorderquantity,
                            OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new OfferBillDiscountItemDc
                            {
                                CategoryId = y.CategoryId,
                                Id = y.Id,
                                IsInclude = y.IsInclude,
                                SubCategoryId = y.SubCategoryId
                            }).ToList(),
                            OfferItems = billDiscountOfferDc.OfferItems.Select(y => new OfferItemdc
                            {
                                IsInclude = y.IsInclude,
                                itemId = y.itemId
                            }).ToList(),
                            RetailerBillDiscountFreeItemDcs = BillDiscountFreeItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).Select(x => new RetailerBillDiscountFreeItemDc
                            {
                                ItemId = x.ItemId,
                                ItemName = x.ItemName,
                                Qty = x.Qty
                            }).ToList(),
                            BillDiscountRequiredItems = billDiscountOfferDc.BillDiscountRequiredItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList(),
                            OfferLineItemValueDcs = billDiscountOfferDc.OfferLineItemValueDcs.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList()
                        };

                        if (bdcheck.BillDiscountOfferOn == "FreeItem" && bdcheck.RetailerBillDiscountFreeItemDcs.Any())
                            FinalBillDiscount.Add(bdcheck);
                        else
                            FinalBillDiscount.Add(bdcheck);

                    }

                    if (FinalBillDiscount.Any() && EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                    {
                        List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                        ElasticLanguageDataRequests = FinalBillDiscount.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.OfferName }).ToList();
                        ElasticLanguageDataRequests.AddRange(FinalBillDiscount.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.Description }).ToList());
                        ElasticLanguageDataRequests.AddRange(FinalBillDiscount.SelectMany(x => x.RetailerBillDiscountFreeItemDcs.Select(z => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = z.ItemName }).ToList()));

                        LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                        var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                        FinalBillDiscount.ForEach(x =>
                        {
                            x.OfferName = ElasticLanguageDatas.Any(y => y.englishtext == x.OfferName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.OfferName).converttext : x.OfferName;
                            x.Description = ElasticLanguageDatas.Any(y => y.englishtext == x.Description) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.Description).converttext : x.Description;
                            x.RetailerBillDiscountFreeItemDcs.ForEach(z =>
                            { z.ItemName = ElasticLanguageDatas.Any(y => y.englishtext == z.ItemName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == z.ItemName).converttext : z.ItemName; });
                        });
                    }
                }
                res = new OfferdataDc()
                {
                    offer = FinalBillDiscount.OrderBy(x => x.start).ToList(),
                    Status = true,
                    Message = "Success"
                };
                return res;
            }

        }

        [Route("RetailerSubCategoryOffer")]
        [HttpGet]
        public async Task<OfferdataDc> RetailerSubCategoryOffer(int CustomerId, int SubCategoryId, string lang = "en")
        {
            List<OfferDc> FinalBillDiscount = new List<OfferDc>();
            OfferdataDc res;
            using (AuthContext context = new AuthContext())
            {
                CustomersManager manager = new CustomersManager();

                List<BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(CustomerId, "Retailer App");
                if (billDiscountOfferDcs.Any())
                {
                    foreach (var billDiscountOfferDc in billDiscountOfferDcs.Where(x => x.BillDiscountType == "subcategory" && x.OfferBillDiscountItems.Any(y => y.Id == SubCategoryId)))
                    {

                        var bdcheck = new OfferDc
                        {
                            OfferId = billDiscountOfferDc.OfferId,

                            OfferName = billDiscountOfferDc.OfferName,
                            OfferCode = billDiscountOfferDc.OfferCode,
                            OfferCategory = billDiscountOfferDc.OfferCategory,
                            OfferOn = billDiscountOfferDc.OfferOn,
                            start = billDiscountOfferDc.start,
                            end = billDiscountOfferDc.end,
                            DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
                            BillAmount = billDiscountOfferDc.BillAmount,
                            LineItem = billDiscountOfferDc.LineItem,
                            Description = billDiscountOfferDc.Description,
                            BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
                            BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
                            IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
                            IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
                            IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
                            BillDiscountType = billDiscountOfferDc.BillDiscountType,
                            OfferAppType = billDiscountOfferDc.OfferAppType,
                            ApplyOn = billDiscountOfferDc.ApplyOn,
                            WalletType = billDiscountOfferDc.WalletType,
                            OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new OfferBillDiscountItemDc
                            {
                                CategoryId = y.CategoryId,
                                Id = y.Id,
                                IsInclude = y.IsInclude,
                                SubCategoryId = y.SubCategoryId
                            }).ToList(),
                            OfferItems = billDiscountOfferDc.OfferItems.Select(y => new OfferItemdc
                            {
                                IsInclude = y.IsInclude,
                                itemId = y.itemId
                            }).ToList(),
                            RetailerBillDiscountFreeItemDcs = null
                        };

                        FinalBillDiscount.Add(bdcheck);
                    }
                }

                if (FinalBillDiscount.Any() && EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                {
                    List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataReuests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                    ElasticLanguageDataReuests = FinalBillDiscount.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.OfferName }).ToList();
                    ElasticLanguageDataReuests.AddRange(FinalBillDiscount.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.Description }).ToList());
                    ElasticLanguageDataReuests.AddRange(FinalBillDiscount.SelectMany(x => x.RetailerBillDiscountFreeItemDcs.Select(z => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = z.ItemName }).ToList()));

                    LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                    var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataReuests.Distinct().ToList(), lang.ToLower());
                    FinalBillDiscount.ForEach(x =>
                    {
                        x.OfferName = ElasticLanguageDatas.Any(y => y.englishtext == x.OfferName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.OfferName).converttext : x.OfferName;
                        x.Description = ElasticLanguageDatas.Any(y => y.englishtext == x.Description) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.Description).converttext : x.Description;
                        x.RetailerBillDiscountFreeItemDcs.ForEach(z =>
                        { z.ItemName = ElasticLanguageDatas.Any(y => y.englishtext == z.ItemName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == z.ItemName).converttext : z.ItemName; });
                    });
                }

                res = new OfferdataDc()
                {
                    offer = FinalBillDiscount,
                    Status = true,
                    Message = "Success"
                };
                return res;
            }

        }

        [Route("RetailerMultiMoqItem")]
        [HttpGet]
        public async Task<ItemListDc> RetailerMultiMoqItem(int warehouseid, int customerId, string lang = "en")
        {
            ItemListDc res;
            using (var db = new AuthContext())
            {

                var MMOQ_Qunt = db.itemMasters.Where(a => a.WarehouseId == warehouseid && a.active && !a.IsDisContinued && a.Deleted == false)
                                       .GroupBy(x => x.Number).Where(x => x.Count() > 2).Select(x => new { x.Key, ProductCount = x.Count() }).ToList();

                List<ItemDataDC> _IListData = new List<ItemDataDC>();
                List<ItemDataDC> _FListData = new List<ItemDataDC>();


                if (MMOQ_Qunt != null && MMOQ_Qunt.Any())
                {
                    var ActiveCustomer = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                    var itemnumbers = MMOQ_Qunt.Select(x => x.Key).ToList();
                    if (ElasticSearchEnable)
                    {
                        Suggest suggest = null;
                        MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                        var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RatailerItemMOQ");
                        var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                        var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                            .Replace("{#warehouseid#}", warehouseid.ToString())
                            .Replace("{#itemnumbers#}", string.Join("+", itemnumbers))
                            .Replace("{#from#}", "0")
                            .Replace("{#size#}", "1000");
                        List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                        _IListData = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();

                    }
                    else
                    {
                        foreach (var data in MMOQ_Qunt)
                        {
                            var newdata = (from a in db.itemMasters
                                           where (a.WarehouseId == warehouseid && a.Deleted == false && (a.ItemAppType == 0 || a.ItemAppType == 1) && a.active && !a.IsDisContinued && a.Number == data.Key)
                                           //join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                           //join c in db.ItemLimitMasterDB on a.Number equals c.ItemNumber
                                           // where ( a.WarehouseId == warehouseid && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                           let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                                           select new ItemDataDC
                                           {
                                               WarehouseId = a.WarehouseId,
                                               IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                               ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                               CompanyId = a.CompanyId,
                                               ItemId = a.ItemId,
                                               ItemNumber = a.Number,
                                               itemname = a.itemname,
                                               itemBaseName = a.itemBaseName,
                                               LogoUrl = a.LogoUrl,
                                               MinOrderQty = a.MinOrderQty,
                                               price = a.price,
                                               TotalTaxPercentage = a.TotalTaxPercentage,
                                               UnitPrice = a.UnitPrice,
                                               HindiName = a.HindiName,
                                               active = a.active,
                                               NetPurchasePrice = a.NetPurchasePrice,
                                               marginPoint = a.marginPoint,
                                               promoPerItems = a.promoPerItems,
                                               IsOffer = a.IsOffer,
                                               Deleted = a.Deleted,
                                               OfferCategory = a.OfferCategory,
                                               OfferStartTime = a.OfferStartTime,
                                               OfferEndTime = a.OfferEndTime,
                                               OfferQtyAvaiable = a.OfferQtyAvaiable,
                                               OfferQtyConsumed = a.OfferQtyConsumed,
                                               OfferId = a.OfferId,
                                               OfferType = a.OfferType,
                                               OfferWalletPoint = a.OfferWalletPoint,
                                               OfferFreeItemId = a.OfferFreeItemId,
                                               OfferPercentage = a.OfferPercentage,
                                               OfferFreeItemName = a.OfferFreeItemName,
                                               OfferFreeItemImage = a.OfferFreeItemImage,
                                               OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                               OfferMinimumQty = a.OfferMinimumQty,
                                               FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                               FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                               BillLimitQty = a.BillLimitQty,
                                               ItemAppType = a.ItemAppType,
                                               ItemMultiMRPId = a.ItemMultiMRPId,
                                               SubCategoryId = a.SubCategoryId,
                                               Categoryid = a.Categoryid,
                                               SubsubCategoryid = a.SubsubCategoryid,
                                               BaseCategoryId = a.BaseCategoryid,
                                               SellingSku = a.SellingSku,
                                               SellingUnitName = a.SellingUnitName,
                                               IsSensitive = a.IsSensitive,
                                               IsSensitiveMRP = a.IsSensitiveMRP,
                                               UOM = a.UOM,
                                               UnitofQuantity = a.UnitofQuantity,
                                               TradePrice = a.TradePrice,
                                               WholeSalePrice = a.WholeSalePrice
                                           }).ToList();

                            _IListData.AddRange(newdata);

                        }
                    }
                    if (_IListData != null && _IListData.Any())
                    {
                        _FListData = await ItemValidate(_IListData, ActiveCustomer, db, lang);
                    }
                }

                res = new ItemListDc()
                {
                    ItemMasters = _FListData,
                    Status = true,
                    Message = "success."
                };
                return res;
            }
        }

        [Route("RetailerAllCategory")]
        [HttpGet]
        public async Task<customeritems> RetailerAllCategory(int warehouseid, int customerId, string lang)
        {
            customeritems ibjtosend = new customeritems();
            using (var unitOfWork = new DataLayer.Infrastructure.UnitOfWork())
            {
                string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
                DataContracts.KPPApp.customeritem CatSubCatBrands = await unitOfWork.KPPAppRepository.GetRetailCatSubCatAsync(warehouseid, 0, ConsumerApptype);
                ibjtosend.Basecats = CatSubCatBrands.Basecats.Select(x => new Basecats
                {
                    BaseCategoryId = x.BaseCategoryId,
                    BaseCategoryName = lang == "hi" && !string.IsNullOrEmpty(x.HindiName) && x.HindiName != "{nan}" ? x.HindiName : x.BaseCategoryName,
                    HindiName = x.HindiName,
                    LogoUrl = x.LogoUrl
                });
                ibjtosend.Categories = CatSubCatBrands.Categories.Select(x => new Categories
                {
                    BaseCategoryId = x.BaseCategoryId,
                    Categoryid = x.Categoryid,
                    CategoryName = lang == "hi" && !string.IsNullOrEmpty(x.HindiName) && x.HindiName != "{nan}" ? x.HindiName : x.CategoryName,
                    LogoUrl = x.LogoUrl
                }).ToList();

                ibjtosend.SubCategories = CatSubCatBrands.SubCategories.Select(x => new AngularJSAuthentication.API.Controllers.SubCategories
                {
                    Categoryid = x.Categoryid,
                    HindiName = x.HindiName,
                    itemcount = x.itemcount,
                    LogoUrl = x.LogoUrl,
                    SubCategoryId = x.SubCategoryId,
                    SubcategoryName = lang == "hi" && !string.IsNullOrEmpty(x.HindiName) && x.HindiName != "{nan}" ? x.HindiName : x.SubcategoryName,
                }).ToList();
                ibjtosend.SubSubCategories = CatSubCatBrands.SubSubCategories.Select(x => new SubSubCategories
                {
                    SubCategoryId = x.SubCategoryId,
                    SubSubCategoryId = x.SubSubCategoryId,
                    SubSubcategoryName = lang == "hi" && !string.IsNullOrEmpty(x.HindiName) && x.HindiName != "{nan}" ? x.HindiName : x.SubSubcategoryName,
                    Categoryid = x.Categoryid,
                    LogoUrl = x.LogoUrl,
                    itemcount = x.itemcount
                }).ToList();
            }

            if (ibjtosend.SubSubCategories != null && ibjtosend.SubSubCategories.Any())
            {
                using (var db = new AuthContext())
                {
                    var ActiveCustomer = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.IsKPP }).FirstOrDefault();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd

                    var custtype = ActiveCustomer.IsKPP ? 1 : 2;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, warehouseid);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        ibjtosend.SubSubCategories = ibjtosend.SubSubCategories.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubSubCategoryId))).ToList();
                        ibjtosend.SubCategories = ibjtosend.SubCategories.Where(x => ibjtosend.SubSubCategories.Select(y => y.SubCategoryId).Contains(x.SubCategoryId)).ToList();
                        ibjtosend.Categories = ibjtosend.Categories.Where(x => ibjtosend.SubCategories.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();
                        ibjtosend.Basecats = ibjtosend.Basecats.Where(x => ibjtosend.Categories.Select(y => y.BaseCategoryId).Contains(x.BaseCategoryId)).ToList();
                    }
                    #endregion
                }
            }


            if ((ibjtosend.Basecats.Any() || ibjtosend.Categories.Any() || ibjtosend.SubCategories.Any() || ibjtosend.SubSubCategories.Any()) && EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
            {
                List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                ElasticLanguageDataRequests = ibjtosend.Basecats.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.BaseCategoryName }).ToList();
                ElasticLanguageDataRequests.AddRange(ibjtosend.Categories.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.CategoryName }).ToList());
                ElasticLanguageDataRequests.AddRange(ibjtosend.SubCategories.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubcategoryName }).ToList());
                ElasticLanguageDataRequests.AddRange(ibjtosend.SubSubCategories.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubSubcategoryName }).ToList());

                LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                ibjtosend.Basecats.ToList().ForEach(x => { x.BaseCategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.BaseCategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.BaseCategoryName).converttext : x.BaseCategoryName; });
                ibjtosend.Categories.ToList().ForEach(x => { x.CategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.CategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.CategoryName).converttext : x.CategoryName; });
                ibjtosend.SubCategories.ToList().ForEach(x => { x.SubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubcategoryName).converttext : x.SubcategoryName; });
                ibjtosend.SubSubCategories.ToList().ForEach(x => { x.SubSubcategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubSubcategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubSubcategoryName).converttext : x.SubSubcategoryName; });

            }
            return ibjtosend;
        }

        [Route("RetailerWallet")]
        [HttpGet]
        public WalletRewardDC Get(int CustomerId)
        {
            using (AuthContext context = new AuthContext())
            {
                WalletRewardDC Item = new WalletRewardDC();

                Wallet d = new Wallet();
                var wallet = context.WalletDb.Where(c => c.CustomerId == CustomerId).SingleOrDefault();
                //var company = context.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).SingleOrDefault();
                var cust = context.Customers.Where(c => c.CustomerId == CustomerId).SingleOrDefault();

                d = new Wallet()
                {
                    TotalAmount = wallet == null ? 0 : wallet.TotalAmount,
                };

                if (wallet == null)
                {
                    if (cust != null)
                    {
                        d.ShopName = cust.ShopName;
                        d.Skcode = cust.Skcode;
                        d.CustomerId = CustomerId;
                        d.TotalAmount = 0;
                        d.CreatedDate = indianTime;
                        d.UpdatedDate = indianTime;
                        d.Deleted = false;
                        context.WalletDb.Add(d);
                        context.Commit();
                    }
                }

                var con = context.CashConversionDb.FirstOrDefault(x => x.IsConsumer == false);
                Item.conversion = Mapper.Map(con).ToANew<CashConversionDc>();
                Item.Wallet = Mapper.Map(d).ToANew<WalletDc>();

                return Item;
            }


        }

        [Route("RetailerCheckOffer")]
        [HttpGet]
        public HttpResponseMessage RetailerCheckOffer(string OfferId, int CustomerId)
        {

            List<int> OfferIds = OfferId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            List<OfferBDDTO> BillDiscount = new List<OfferBDDTO>();
            List<OfferValidation> OfferValidations = new List<OfferValidation>();

            using (AuthContext context = new AuthContext())
            {
                var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);

                DateTime CurrentDate = IsPrimeCustomer ? indianTime.AddHours(-1 * MemberShipHours) : indianTime;
                int? warehouseId = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false && x.Active == true).Select(x => x.Warehouseid).FirstOrDefault();
                List<BillDiscount> BillDiscountUsed = context.BillDiscountDb.Where(x => x.CustomerId == CustomerId && OfferIds.Contains(x.OfferId) && x.IsActive == true && x.IsDeleted == false).ToList();
                if (warehouseId != null && warehouseId > 0)
                {
                    BillDiscount = (from o in context.OfferDb
                                    where OfferIds.Contains(o.OfferId)
                                    select new OfferBDDTO
                                    {
                                        OfferId = o.OfferId,
                                        OfferOn = o.OfferOn,
                                        start = o.start,
                                        end = o.end,
                                        IsActive = o.IsActive,
                                        IsDeleted = o.IsDeleted,
                                        IsMultiTimeUse = o.IsMultiTimeUse,
                                        OfferUseCount = o.OfferUseCount,
                                        IsCRMOffer = o.IsCRMOffer
                                    }).ToList();
                    // check  customer used id or not  && o.start <= CurrentDate && o.end >= CurrentDate

                    foreach (var item in BillDiscount)
                    {
                        bool valid = true;
                        string message = "";

                        if (item.IsCRMOffer)
                        {
                            if (BillDiscountUsed.All(y => y.OfferId == item.OfferId && y.OrderId > 0))
                            {
                                valid = false;
                                message = "You have already taken this offer.";
                            }
                            //else if (BillDiscountUsed.All(y => y.OfferId == item.OfferId && item.OfferOn != "ScratchBillDiscount"))
                            //{
                            //    item.OfferUseCount = item.OfferUseCount.HasValue ? item.OfferUseCount.Value : (item.IsMultiTimeUse ? 1000 : 1);
                            //    if ((item.OfferUseCount.Value <= BillDiscountUsed.Count(x => x.OfferId == item.OfferId && x.OrderId > 0)))
                            //    {
                            //        valid = false;
                            //        message = "You have already taken this offer.";
                            //    }
                            //}
                        }
                        else
                        {
                            if (item.OfferOn == "ScratchBillDiscount" && BillDiscountUsed.All(y => y.OfferId == item.OfferId && y.OrderId > 0))
                            {
                                valid = false;
                                message = "You have already taken this scratch card";
                            }
                            else if (item.OfferOn != "ScratchBillDiscount" && BillDiscountUsed.Any(y => y.OfferId == item.OfferId))
                            {
                                item.OfferUseCount = item.OfferUseCount.HasValue ? item.OfferUseCount.Value : (item.IsMultiTimeUse ? 1000 : 1);
                                if ((item.OfferUseCount.Value <= BillDiscountUsed.Count(x => x.OfferId == item.OfferId && x.OrderId > 0)))
                                {
                                    valid = false;
                                    message = "You have already taken this offer.";
                                }
                            }
                        }
                        if (valid && !(item.start <= CurrentDate && item.end >= indianTime))
                        {
                            valid = false;
                            message = (item.OfferOn == "ScratchBillDiscount" ? "Scratch Card " : "Offer ") + "expired.";
                        }

                        if (valid && !(item.IsActive && !item.IsDeleted))
                        {
                            valid = false;
                            message = (item.OfferOn == "ScratchBillDiscount" ? "Scratch Card " : "Offer ") + "no more available.";
                        }

                        OfferValidations.Add(new OfferValidation { Message = message, Valid = valid, OfferId = item.OfferId });
                    }

                }
                var res = new
                {
                    BillDiscount = OfferValidations,
                    Status = OfferValidations.All(x => x.Valid)
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }


        }

        [Route("RetailerFavourite")]
        [HttpPost]
        public async Task<ItemListDc> getFavItems(Cutomerfavourite FIt)
        {
            using (AuthContext context = new AuthContext())
            {
                if (string.IsNullOrEmpty(FIt.lang))
                    FIt.lang = "en";
                ItemListDc res;
                var item = FIt.items;
                List<ItemDataDC> itemlist = new List<ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == FIt.customerId);

                List<int> ids = item.Select(x => x.ItemId).ToList();
                if (ids != null && ids.Any())
                {
                    var newdata = new List<ItemDataDC>();
                    if (ElasticSearchEnable)
                    {
                        Suggest suggest = null;
                        MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                        var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerFavourite");
                        var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                        var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                            .Replace("{#warehouseid#}", ActiveCustomer.Warehouseid.ToString())
                            .Replace("{#itemids#}", string.Join(",", ids))
                            .Replace("{#from#}", "0")
                            .Replace("{#size#}", "1000");
                        List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                        newdata = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();

                    }
                    else
                    {
                        var excludecategory = context.Categorys.Where(x => x.CategoryName == "Face wash & Cream" || x.CategoryName == "Body cream & lotion" || x.CategoryName == "Baby & Fem Care").Select(x => x.Categoryid).ToList();
                        newdata = (from a in context.itemMasters
                                   where (a.Deleted == false && a.active == true && ids.Contains(a.ItemId)
                                    && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                   join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new ItemDataDC
                                   {
                                       WarehouseId = a.WarehouseId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       CompanyId = a.CompanyId,
                                       ItemId = a.ItemId,
                                       ItemNumber = b.Number,
                                       itemname = a.itemname,
                                       itemBaseName = a.itemBaseName,
                                       LogoUrl = b.LogoUrl,
                                       MinOrderQty = b.MinOrderQty,
                                       price = a.price,
                                       TotalTaxPercentage = b.TotalTaxPercentage,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = b.HindiName,
                                       active = a.active,
                                       NetPurchasePrice = a.NetPurchasePrice,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       IsOffer = a.IsOffer,
                                       Deleted = a.Deleted,
                                       OfferCategory = a.OfferCategory,
                                       OfferStartTime = a.OfferStartTime,
                                       OfferEndTime = a.OfferEndTime,
                                       OfferQtyAvaiable = a.OfferQtyAvaiable,
                                       OfferQtyConsumed = a.OfferQtyConsumed,
                                       OfferId = a.OfferId,
                                       OfferType = a.OfferType,
                                       OfferWalletPoint = a.OfferWalletPoint,
                                       OfferFreeItemId = a.OfferFreeItemId,
                                       OfferPercentage = a.OfferPercentage,
                                       OfferFreeItemName = a.OfferFreeItemName,
                                       OfferFreeItemImage = a.OfferFreeItemImage,
                                       OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                       OfferMinimumQty = a.OfferMinimumQty,
                                       FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                       FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                       BillLimitQty = a.BillLimitQty,
                                       ItemAppType = a.ItemAppType,
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       Categoryid = a.Categoryid,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       BaseCategoryId = a.BaseCategoryid,
                                       SellingSku = a.SellingSku,
                                       SellingUnitName = a.SellingUnitName,
                                       TradePrice = a.TradePrice,
                                       WholeSalePrice = a.WholeSalePrice
                                   }).OrderByDescending(x => x.ItemNumber).ToList();
                    }

                    itemlist = await ItemValidate(newdata, ActiveCustomer, context, FIt.lang);
                }

                if (itemlist != null)
                {
                    res = new ItemListDc
                    {

                        ItemMasters = itemlist,
                        Status = true,
                        Message = "Success"
                    };
                    return res;
                }
                else
                {

                    res = new ItemListDc
                    {
                        ItemMasters = null,
                        Status = false,
                        Message = "fail"
                    };
                    return res;
                }

            }
        }


        [Route("RetailerRewardItem")]
        [HttpGet]
        public async Task<List<RewardItems>> RetailerRewardItem()
        {
            List<RewardItems> List = new List<RewardItems>();
            int Warehouse_id = 0; int compid = 0;

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {
                if (Warehouse_id > 0)
                {
                    List = context.RewardItemsDb.Where(r => r.IsActive == true && r.IsDeleted == false && r.WarehouseId == Warehouse_id).OrderByDescending(o => o.rPoint).ToList();
                    return List;
                }
                else
                {
                    List = context.RewardItemsDb.Where(r => r.IsActive == true && r.IsDeleted == false && r.CompanyId == 1).OrderByDescending(o => o.rPoint).ToList();

                    return List;
                }
            }
        }


        [Route("RetailerFeedback")]
        [HttpPost]
        public Feedback add(Feedback item)
        {
            using (AuthContext context = new AuthContext())
            {
                int Warehouse_id = 0; int compid = 0;

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                item.CompanyId = compid;
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                item = context.AddFeedBack(item);

                return item;
            }

        }

        [Route("RetailerRequest")]
        [HttpPost]
        public RequestItem add(RequestItem item)
        {
            using (var context = new AuthContext())
            {
                int Warehouse_id = 0; int compid = 0; int userid = 0;
                var identity = User.Identity as ClaimsIdentity;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                    Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                item.CompanyId = compid;
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                Customer customer = context.Customers.Where(c => c.Mobile == item.customerMobile && c.Deleted == false).FirstOrDefault();
                if (customer != null)
                {
                    item.customerId = customer.CustomerId;
                    item.shopName = customer.ShopName;
                    item.createdDate = indianTime;
                    context.RequestItems.Add(item);
                    context.Commit();
                    return item;
                }
                else
                    return null;
            }


        }

        [Route("RetailerChangePassword")]
        [HttpPut]
        public async Task<CustomerPassword> Changepassword(pwcdetail item)
        {
            using (AuthContext db = new AuthContext())
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                CustomerPassword res;
                Customer cust = db.Customers.Where(x => x.CustomerId == item.CustomerId).SingleOrDefault();
                var identity = User.Identity as ClaimsIdentity;
                if (cust != null && cust.Password != item.currentpassword)
                    cust = null;

                if (cust != null)
                {
                    cust.Password = item.newpassword;
                    cust.IsResetPasswordOnLogin = false;
                    db.Entry(cust).State = EntityState.Modified;
                    db.Commit();
                }
                else
                {
                    res = new CustomerPassword()
                    {
                        Password = null,
                        Status = false,
                        Message = "Customer not exist."
                    };
                    return res;
                }
                res = new CustomerPassword()
                {
                    Password = cust.Password,
                    Status = true,
                    Message = "Password is changed."
                };
                return res;

            }
        }


        [Route("RetailerUpdateRating")]
        [HttpPut]
        public async Task<customerrating> UpdateRating(Customer CustomerRating)
        {
            customerrating res;

            using (AuthContext db = new AuthContext())
            {
                Customer rating = db.Customers.Where(x => x.CustomerId == CustomerRating.CustomerId).FirstOrDefault();
                rating.CustomerRating = CustomerRating.CustomerRating;
                rating.CustomerRatingCommnets = CustomerRating.CustomerRatingCommnets;
                db.Commit();
                if (rating != null)
                {
                    res = new customerrating()
                    {
                        //customers = rating,
                        Status = true,
                        Message = "Success."
                    };
                    return res;
                }
                else
                {
                    res = new customerrating()
                    {
                        //customers = null,
                        Status = false,
                        Message = "Failed."
                    };
                    return res;
                }
            }
        }


        public Customer UpdateFullyVerifiedCustomer(Customer Cust)
        {
            using (AuthContext db = new AuthContext())
            {
                Customer customer = db.Customers.Where(c => c.CustomerId == Cust.CustomerId && c.Mobile.Trim().Equals(Cust.Mobile.Trim()) && c.Deleted == false).SingleOrDefault();
                try
                {
                    if (customer != null)
                    {
                        //var dbcity = db.Cities.FirstOrDefault(x => x.CityName == customer.City);

                        customer.Emailid = Cust.Emailid;
                        customer.lat = Cust.lat;
                        customer.lg = Cust.lg;
                        //customer.Name = Cust.Name;
                        //customer.ShopName = Cust.ShopName;
                        //customer.RefNo = Cust.RefNo;
                        customer.DOB = Cust.DOB;
                        customer.AnniversaryDate = Cust.AnniversaryDate;     //tejas 25-05-2019
                        customer.WhatsappNumber = Cust.WhatsappNumber;      //tejas 25-05-2019
                        customer.UploadLicensePicture = Cust.UploadLicensePicture;  //tejas 04-06-2019 
                                                                                    //customer.UploadGSTPicture = Cust.UploadGSTPicture;  //tejas 04-06-2019  
                        customer.LicenseNumber = Cust.LicenseNumber;      //tejas 04-06-2019  
                        customer.Shopimage = Cust.Shopimage;                //tejas 04-06-2019  
                                                                            //customer.AreaName = Cust.AreaName;
                                                                            //customer.ShippingAddress = Cust.ShippingAddress;
                        customer.UploadRegistration = Cust.UploadRegistration;
                        customer.UploadProfilePichure = Cust.UploadProfilePichure;
                        customer.ResidenceAddressProof = Cust.ResidenceAddressProof;
                        //customer.ShippingAddress1 = Cust.ShippingAddress1;
                        //customer.BillingAddress1 = Cust.BillingAddress1;
                        customer.ZipCode = Cust.ZipCode;
                        customer.BillingAddress = Cust.BillingAddress;
                        customer.ShippingAddress = Cust.ShippingAddress;
                        customer.LandMark = Cust.LandMark;
                        //customer.State = dbcity != null ? dbcity.StateName : "";
                        //customer.Cityid = dbcity != null ? dbcity.Cityid : 0;
                        //customer.City= dbcity != null ? dbcity.CityName : "";
                        //Customers.Attach(customer);


                        #region to assign cluster ID and determine if it is in cluster or not.                   
                        if (customer.lat != 0 && customer.lg != 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(customer.lat).Append("', '").Append(customer.lg).Append("')");
                            var clusterId = db.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                            if (!clusterId.HasValue)
                            {
                                customer.InRegion = false;
                                string message = "Thanks for registering with ShopKirana. We will contact you soon";

                                var notificationSent = db.DeviceNotificationDb.Any(x => x.CustomerId == customer.CustomerId && x.Message == message);

                                if (!notificationSent)
                                {
                                    NotificationHelper notHelper = new NotificationHelper();
                                    string title = "Congratulations! ";

                                    notHelper.SendNotificationtoCustomer(customer, message, message, true, true, title);
                                }
                            }
                            else
                            {
                                //customer.ClusterId = clusterId;
                                var dd = db.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                //customer.ClusterName = dd.ClusterName;
                                customer.InRegion = true;
                                customer.Warehouseid = dd.WarehouseId;
                                customer.WarehouseName = dd.WarehouseName;
                            }
                        }
                        #endregion
                        db.Entry(customer).State = EntityState.Modified;
                        db.Commit();
                        #region Customer Retailer History on Backend
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    CustomersController.logger.Error(ex.Message);
                }
                return customer;
            }
        }


        [Route("GetPrepaidOrder")]
        [HttpGet]
        public async Task<Messagedata> GetPrepaidOrder(int WarehouseId)
        {
            Messagedata res;
            CustomizedPrepaidOrders CustomizedPrepaidOrders = new CustomizedPrepaidOrders();
            int Warehouse_id = 0; int compid = 0;

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var context = new AuthContext())
            {

                CustomizedPrepaidOrders = context.CustomizedPrepaidOrders.Where(r => r.IsActive == true && r.IsDeleted == false && r.warehouseId == WarehouseId).FirstOrDefault();
                var wheelConfig = await context.CompanyWheelConfiguration.FirstOrDefaultAsync();
                CompnayWheelConfigDc compnayWheelConfigDc = new CompnayWheelConfigDc();
                if (wheelConfig != null)
                {
                    compnayWheelConfigDc.OrderAmount = wheelConfig.OrderAmount;
                    compnayWheelConfigDc.LineItemCount = wheelConfig.LineItemCount;
                }
                if (CustomizedPrepaidOrders != null)
                {
                    res = new Messagedata
                    {
                        CustomizedPrepaidOrders = CustomizedPrepaidOrders,
                        CompnayWheelConfig = compnayWheelConfigDc,
                        Status = true,
                        message = "Success."
                    };
                    return res;

                }
                else
                {
                    res = new Messagedata
                    {
                        CustomizedPrepaidOrders = CustomizedPrepaidOrders,
                        Status = false,
                        message = "No Data."
                    };
                    return res;
                }
            }
        }

        [Route("GetRetailerMinOrderAmount")]
        [HttpGet]
        public async Task<int> GetRetailerMinOrderAmount(int customerId)
        {
            string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
            int minOrderValue = Convert.ToInt32(ConfigurationManager.AppSettings["MinOrderValue"]);
            using (var context = new AuthContext())
            {
                var customer = await context.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
                if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                {
                    var defaultadd = context.ConsumerAddressDb.FirstOrDefault(x => x.CustomerId == customerId && x.Default);
                    if (defaultadd != null)
                    {
                        customer.Cityid = defaultadd.Cityid;
                        customer.Warehouseid = defaultadd.WarehouseId;
                    }
                }
                if (customer != null && customer.Cityid.HasValue)
                {
                    MongoDbHelper<DataContracts.Mongo.RetailerMinOrder> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.RetailerMinOrder>();
                    var cartPredicate = PredicateBuilder.New<DataContracts.Mongo.RetailerMinOrder>(x => x.CityId == customer.Cityid && x.WarehouseId == customer.Warehouseid);
                    var retailerMinOrder = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "RetailerMinOrder").FirstOrDefault();
                    if (retailerMinOrder != null)
                    {
                        minOrderValue = retailerMinOrder.MinOrderValue;
                    }
                    else
                    {
                        DataContracts.Mongo.RetailerMinOrder newRetailerMinOrder = new DataContracts.Mongo.RetailerMinOrder
                        {
                            CityId = customer.Cityid.Value,
                            WarehouseId = customer.Warehouseid.Value,
                            MinOrderValue = minOrderValue
                        };
                        var result = mongoDbHelper.Insert(newRetailerMinOrder);
                    }
                }
            }

            return minOrderValue;

        }

        //[Route("GetRetailerMinOrderAmountSalesAPP")]
        //[HttpGet]
        //public async Task<dynamic> GetRetailerMinOrderAmountSalesAPP(int warehouseId)
        //{
        //    int minOrderValue = Convert.ToInt32(ConfigurationManager.AppSettings["MinOrderValue"]);
        //    var companydetails = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new
        //    {
        //        x.NoOfLineItemSales,
        //    }).FirstOrDefault();
        //    using (var context = new AuthContext())
        //    {
        //        var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId);
        //        if (warehouse != null && warehouse.Cityid > 0)
        //        {
        //            MongoDbHelper<DataContracts.Mongo.RetailerMinOrder> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.RetailerMinOrder>();
        //            var cartPredicate = PredicateBuilder.New<DataContracts.Mongo.RetailerMinOrder>(x => x.CityId == warehouse.Cityid);
        //            var retailerMinOrder = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "RetailerMinOrder").FirstOrDefault();
        //            if (retailerMinOrder != null)
        //            {
        //                minOrderValue = retailerMinOrder.MinOrderValue;
        //            }
        //            else
        //            {
        //                DataContracts.Mongo.RetailerMinOrder newRetailerMinOrder = new DataContracts.Mongo.RetailerMinOrder
        //                {
        //                    CityId = warehouse.Cityid,
        //                    WarehouseId = warehouse.WarehouseId,
        //                    MinOrderValue = minOrderValue
        //                };
        //                var result = mongoDbHelper.Insert(newRetailerMinOrder);
        //            }
        //        }
        //    }

        //    return new { minOrderValue = minOrderValue, NoOfLineItem = companydetails.NoOfLineItemSales };

        //}

        [Route("GetCompanyWheelConfig")]
        [HttpGet]
        public async Task<CompnayWheelConfigDc> GetCompanyWheelConfig(int WarehouseId)
        {
            CompnayWheelConfigDc compnayWheelConfigDc = new CompnayWheelConfigDc();
            using (var context = new AuthContext())
            {
                var wheelConfig = await context.CompanyWheelConfiguration.FirstOrDefaultAsync();
                if (wheelConfig != null)
                {
                    compnayWheelConfigDc.OrderAmount = wheelConfig.OrderAmount;
                    compnayWheelConfigDc.LineItemCount = wheelConfig.LineItemCount;
                    compnayWheelConfigDc.IsKPPRequiredWheel = wheelConfig.IsKPPRequiredWheel;
                }
            }
            return compnayWheelConfigDc;
        }

        [Route("GetLastPurchaseItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetLastPurchaseItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<ItemDataDC>() };
            using (var context = new AuthContext())
            {
                List<ItemDataDC> ItemDataDCs = new List<ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetCustomerLastMonthPurchaseItem]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemcount"]);
                }

                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);
            }

            return itemResponseDc;
        }

        [Route("GetGoldenDealItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemResponseDc> GetGoldenDealItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<ItemDataDC>() };
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetGoldenItem]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);
            }

            return itemResponseDc;
        }


        [Route("GetTopMarginItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemResponseDc> GetTopMarginItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<ItemDataDC>() };
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetTopMarginItem]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);
            }

            return itemResponseDc;
        }

        [Route("GetCompanyTopMarginItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetCompanyTopMarginItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<ItemDataDC>() };
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetCompanyTopMarginItem]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);
            }

            return itemResponseDc;
        }

        [Route("GetAllStore")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<RetailerStore>> GetAllStore(int customerId, int warehouseId, string lang)
        {
            List<RetailerStore> retailerStore = new List<RetailerStore>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetAllStore]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@lang", lang));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                retailerStore = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<RetailerStore>(reader).ToList();

                //retailerStore = await context.SubCategorys.Where(x => x.StoreType != 0 && x.IsActive && !x.Deleted).Select(x => new RetailerStore
                //{

                //    Logo = string.IsNullOrEmpty(x.StoreImage) ? x.LogoUrl : x.StoreImage,
                //    SubCategoryId = x.SubCategoryId,
                //    SubCategoryName = x.SubcategoryName
                //}).ToListAsync();

                if ((retailerStore.Any()) && EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "en")
                {
                    List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                    ElasticLanguageDataRequests = retailerStore.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SubCategoryName }).ToList();

                    LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                    var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                    retailerStore.ToList().ForEach(x => { x.SubCategoryName = ElasticLanguageDatas.Any(y => y.englishtext == x.SubCategoryName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SubCategoryName).converttext : x.SubCategoryName; });

                }
            }
            return retailerStore;
        }

        [Route("ShowOtherLoginOption")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> ShowOtherLoginOption()
        {
            return Convert.ToBoolean(ConfigurationManager.AppSettings["ShowOtherLoginOption"]);

        }


        [Route("PrimePaymentRequest")]
        [AcceptVerbs("POST")]
        public async Task<PaymentRequestResponse> PrimePaymentRequest(PaymentRequest paymentRequest)
        {
            PaymentRequestResponse paymentRequestResponse = new PaymentRequestResponse { message = "", status = false, TransId = 0 };
            using (var db = new AuthContext())
            {
                var companydetails = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new
                {
                    x.IsPrimeActive,
                }).FirstOrDefault();

                var membership = db.MemberShips.FirstOrDefault(x => x.Id == paymentRequest.MemberShipId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                if (companydetails != null && membership != null && companydetails.IsPrimeActive)
                {

                    bool NextLevelMembership = true;
                    var SkCode = db.Customers.FirstOrDefault(x => x.CustomerId == paymentRequest.CustomerId).Skcode;
                    SkCode = SkCode.Replace("SK", "");
                    var PrimeCustomer = db.PrimeCustomers.Where(x => x.CustomerId == paymentRequest.CustomerId).Include(x => x.PrimeRegistrationDetails).FirstOrDefault();
                    //var existingMembership = PrimeCustomer != null &&
                    //    PrimeCustomer.MemberShipId > 0 ? 
                    //    db.MemberShips.FirstOrDefault(x => x.Id == PrimeCustomer.MemberShipId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).memberShipSequence : 0;
                    var Inpayment = new PrimeRegistrationDetail();
                    if (PrimeCustomer == null)
                    {
                        PrimeCustomer = new PrimeCustomer();
                        PrimeCustomer.StartDate = DateTime.Now;
                        PrimeCustomer.EndDate = DateTime.Now;
                        PrimeCustomer.IsActive = false;
                        PrimeCustomer.IsDeleted = false;
                        PrimeCustomer.MemberShipId = membership.Id;
                        PrimeCustomer.CustomerId = paymentRequest.CustomerId;
                        PrimeCustomer.CreatedBy = paymentRequest.CustomerId;
                        PrimeCustomer.CreatedDate = DateTime.Now;
                        PrimeCustomer.PrimeRegistrationDetails = new List<PrimeRegistrationDetail>();
                        Inpayment.FeeAmount = membership.Amount;
                        Inpayment.CreatedDate = DateTime.Now;
                        Inpayment.status = "Fail";
                        Inpayment.CreatedBy = Convert.ToInt32(paymentRequest.CustomerId);
                        Inpayment.GatewayRequest = paymentRequest.GatewayRequest;
                        Inpayment.PaymentFrom = paymentRequest.PaymentFrom;
                        Inpayment.IsActive = true;
                        Inpayment.IsDeleted = false;
                        PrimeCustomer.PrimeRegistrationDetails.Add(Inpayment);
                        db.PrimeCustomers.Add(PrimeCustomer);
                    }
                    else
                    {
                        //PrimeCustomer.MemberShipId = membership.Id;
                        //PrimeCustomer.CustomerId = paymentRequest.CustomerId;
                        PrimeCustomer.ModifiedBy = paymentRequest.CustomerId;
                        PrimeCustomer.ModifiedDate = DateTime.Now;
                        Inpayment.FeeAmount = membership.Amount;
                        Inpayment.CreatedDate = DateTime.Now;
                        Inpayment.status = "Fail";
                        Inpayment.CreatedBy = Convert.ToInt32(paymentRequest.CustomerId);
                        Inpayment.GatewayRequest = paymentRequest.GatewayRequest;
                        Inpayment.PaymentFrom = paymentRequest.PaymentFrom;
                        Inpayment.IsActive = true;
                        Inpayment.IsDeleted = false;
                        PrimeCustomer.PrimeRegistrationDetails.Add(Inpayment);
                        db.Entry(PrimeCustomer).State = EntityState.Modified;
                        //}
                        //else
                        //{
                        //    NextLevelMembership = false;
                        //    paymentRequestResponse.status = false;
                        //    paymentRequestResponse.message = "You have already taken higher Membership.";
                    }
                    //if (NextLevelMembership)
                    //{
                    if (db.Commit() > 0)
                    {
                        paymentRequestResponse.MemberShipId = membership.Id;
                        paymentRequestResponse.status = true;
                        paymentRequestResponse.TransId = Convert.ToInt64(Inpayment.Id.ToString() + "" + SkCode);
                    }
                    else
                    {
                        paymentRequestResponse.status = false;
                        paymentRequestResponse.message = "Some issue occurred during generate transaction. Please try after some time.";
                    }
                    // }
                }
                else
                {
                    paymentRequestResponse.status = false;
                    paymentRequestResponse.message = "Membership activation not started. Please try after some time.";
                }
            }
            return paymentRequestResponse;
        }

        [Route("PrimePaymentResponse")]
        [AcceptVerbs("POST")]
        public async Task<bool> PrimePaymentResponse(PaymentRequest paymentRequest)
        {
            bool result = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(120);
            Customer customer = new Customer();
            MemberShip MemberShip = new MemberShip();
            var PrimeCustomer = new PrimeCustomer();
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var db = new AuthContext())
                {

                    customer = db.Customers.Where(x => x.CustomerId == paymentRequest.CustomerId).FirstOrDefault();
                    var SkCode = customer.Skcode.Replace("SK", "");
                    paymentRequest.TransId = Convert.ToInt64(paymentRequest.TransId.ToString().Replace(SkCode, ""));
                    PrimeCustomer = db.PrimeCustomers.Where(x => x.CustomerId == paymentRequest.CustomerId).Include(x => x.PrimeRegistrationDetails).FirstOrDefault();
                    var walletAmount = 0;
                    if (PrimeCustomer != null && PrimeCustomer.PrimeRegistrationDetails.Any(x => x.Id == paymentRequest.TransId))
                    {
                        var primereg = PrimeCustomer.PrimeRegistrationDetails.FirstOrDefault(x => x.Id == paymentRequest.TransId);
                        primereg.GatewayRequest = paymentRequest.GatewayRequest;
                        primereg.GatewayResponse = paymentRequest.GatewayResponse;
                        primereg.GatewayTransId = paymentRequest.GatewayTransId;
                        primereg.PaymentFrom = paymentRequest.PaymentFrom;
                        primereg.status = paymentRequest.Status;
                        primereg.ModifiedBy = paymentRequest.CustomerId;
                        primereg.MemberShipId = paymentRequest.MemberShipId;
                        primereg.ModifiedDate = DateTime.Now;
                        primereg.InvoiceNo = "";
                        db.Entry(primereg).State = EntityState.Modified;
                        if (!string.IsNullOrEmpty(primereg.status) && primereg.status.ToLower() == "success")
                        {
                            MemberShip = db.MemberShips.FirstOrDefault(x => x.Id == paymentRequest.MemberShipId);
                            PrimeCustomer.StartDate = PrimeCustomer.EndDate > DateTime.Now && PrimeCustomer.IsActive ? PrimeCustomer.StartDate : DateTime.Now;
                            PrimeCustomer.EndDate = PrimeCustomer.StartDate.AddDays(MemberShip.MemberShipInDays);
                            PrimeCustomer.ModifiedDate = DateTime.Now;
                            PrimeCustomer.MemberShipId = MemberShip.Id;
                            PrimeCustomer.IsActive = true;
                            PrimeCustomer.IsDeleted = false;
                            PrimeCustomer.ModifiedBy = Convert.ToInt32(paymentRequest.CustomerId);
                            db.Entry(PrimeCustomer).State = EntityState.Modified;
                            walletAmount = Convert.ToInt32((MemberShip.Amount * 10) / (MemberShip.MemberShipInMonth * 3));
                        }
                    }
                    if (db.Commit() > 0)
                    {
                        if (!string.IsNullOrEmpty(paymentRequest.Status) && paymentRequest.Status.ToLower() == "success" && walletAmount > 0)
                        {
                            db.Database.ExecuteSqlCommand("exec InsertFaydaBucket " + paymentRequest.CustomerId + "," + walletAmount);
                        }
                        result = true;
                        dbContextTransaction.Complete();
                    }
                }
            }

            if (result)
            {
                try
                {
                    if (!string.IsNullOrEmpty(paymentRequest.Status) && paymentRequest.Status.ToLower() == "success")
                    {
                        NewHelper.CustomerLedgerHelper customerLedgerHelper = new NewHelper.CustomerLedgerHelper();
                        customerLedgerHelper.FaydaLedgerEntry(paymentRequest.CustomerId, paymentRequest.CustomerId, Convert.ToInt32(paymentRequest.TransId), MemberShip.Amount, DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    CustomersController.logger.Error("Error during Log Fayda ledger entry for customerid:" + paymentRequest.CustomerId + " Error:" + ex.ToString());
                }
                if (!string.IsNullOrEmpty(paymentRequest.Status) && paymentRequest.Status.ToLower() == "success")
                {
                    if (customer != null)
                    {
                        MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();
                        MongoDbHelper<DefaultNotificationMessage> DefalutmongoDbHelper = new MongoDbHelper<DefaultNotificationMessage>();
                        string defaultmessage = DefalutmongoDbHelper.Select(x => x.NotificationMsgType == "FaydaMemberShip").FirstOrDefault()?.NotificationMsg;//   "Hi [CustomerName]. You Have Left Something in Your Cart, complete your Purchase with ShopKirana on Sigle Click.";

                        string message = "Dear " + customer.ShopName + ",\n Congratulation, You have successfully subscribed " + AppConstants.MemberShipName + "  for " + MemberShip.MemberShipName + " , it's valid up to " + PrimeCustomer.EndDate.ToString("dd/MM/yyyy hh:mm tt") + " . ";

                        if (!string.IsNullOrEmpty(defaultmessage))
                        {
                            message = defaultmessage.Replace("[CustomerName]", customer.ShopName).Replace("[DefaultName]", AppConstants.MemberShipName).Replace("[MemberShipName]", MemberShip.MemberShipName).Replace("[ValidTill]", PrimeCustomer.EndDate.ToString("dd/MM/yyyy hh:mm tt"));
                        }

                        if (!string.IsNullOrEmpty(customer.Mobile))
                        {
                            SendSMSHelper.SendSMS(customer.Mobile, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), "");
                        }

                        if (!string.IsNullOrEmpty(customer.fcmId))
                        {
                            //var objNotificationList = new
                            //{
                            //    to = customer.fcmId,
                            //    CustId = paymentRequest.CustomerId,
                            //    data = new
                            //    {
                            //        title = "Congratulation For Join " + AppConstants.MemberShipName,
                            //        body = message,
                            //        icon = "",
                            //        typeId = "",
                            //        notificationCategory = "",
                            //        notificationType = "Actionable",
                            //        notificationId = 1,
                            //        notify_type = "prime",
                            //        url = "",
                            //    }
                            //};
                            var data = new FCMData
                            {
                                title = "Congratulation For Join " + AppConstants.MemberShipName,
                                body = message,
                                icon = "",
                                notificationCategory = "",
                                notificationType = "Actionable",
                                notify_type = "prime",
                                url = "",
                                notificationId = 1
                            };
                            try
                            {
                                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                                //tRequest.Method = "post";
                                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotificationList);
                                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                                //tRequest.Headers.Add(string.Format("Authorization: key={0}", ConfigurationManager.AppSettings["FcmApiKey"]));

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

                                //            }
                                //        }
                                //    }
                                //}
                                var Key = ConfigurationManager.AppSettings["FcmApiKey"];
                                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                                var Result = await firebaseService.SendNotificationForApprovalAsync(customer.fcmId, data);
                                if (Result != null)
                                {
                                }
                            }
                            catch (Exception asd)
                            {
                                CustomersController.logger.Error("Error during sent Join Membership  notification : " + asd.ToString());
                            }
                        }
                    }
                }
            }
            return result;
        }

        [Route("GetAllMemberShip")]
        [HttpGet]
        public async Task<List<MemberShipDc>> GetAllMemberShip(int customerId, string lang)
        {
            var MemberShipDcs = new List<MemberShipDc>();
            using (var db = new AuthContext())
            {
                var primeCustomer = db.PrimeCustomers.FirstOrDefault(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                MemberShipDcs = db.MemberShips.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))
                    .Select(x => new MemberShipDc
                    {
                        Amount = x.Amount,
                        Id = x.Id,
                        MemberShipDescription = lang == "hi" ? x.MemberShipHindiDescription : x.MemberShipDescription,
                        MemberShipInMonth = x.MemberShipInMonth,
                        MemberShipLogo = x.MemberShipLogo,
                        MemberShipName = lang == "hi" ? x.MemberShipHindiName : x.MemberShipName,
                        MemberShipSequence = x.memberShipSequence
                    }
                ).OrderBy(x => x.MemberShipSequence).ToList();

                var takenSequenct = primeCustomer != null && primeCustomer.MemberShipId > 0 ? MemberShipDcs.FirstOrDefault(x => x.Id == primeCustomer.MemberShipId).MemberShipSequence : 0;

                foreach (var item in MemberShipDcs)
                {
                    item.taken = takenSequenct >= item.MemberShipSequence;

                    if (primeCustomer != null && primeCustomer.MemberShipId > 0 && primeCustomer.MemberShipId == item.Id)
                    {
                        item.taken = true;
                        item.takenMemberId = item.Id;
                        item.StartDate = primeCustomer.StartDate;
                        item.EndDate = primeCustomer.EndDate;
                    }
                }

            }
            return MemberShipDcs;
        }

        [Route("CustomerMemberShipDetail")]
        [HttpGet]
        public async Task<CustomerMemberShipDetail> CustomerMemberShipDetail(int customerId, int warehouseId, string lang)
        {
            CustomerMemberShipDetail customerMemberShipDetail = new CustomerMemberShipDetail();
            MongoDbHelper<MemberShipBanifit> mongoDbHelper = new MongoDbHelper<MemberShipBanifit>();
            using (var db = new AuthContext())
            {
                var MemberShipBanifit = mongoDbHelper.Select(x => !string.IsNullOrEmpty(x.Logo)).FirstOrDefault();
                var primeCustomer = db.PrimeCustomers.FirstOrDefault(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);
                if (primeCustomer != null)
                {
                    if (db.Database.Connection.State != ConnectionState.Open)
                        db.Database.Connection.Open();


                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetCustomerPrimeBanifit]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                    var reader = cmd.ExecuteReader();
                    var customerBanifits = ((IObjectContextAdapter)db)
                    .ObjectContext
                    .Translate<customerBanifit>(reader).FirstOrDefault();
                    var membership = db.MemberShips.FirstOrDefault(x => x.Id == primeCustomer.MemberShipId);
                    customerMemberShipDetail.MemberShipName = membership.MemberShipName;
                    customerMemberShipDetail.StartDate = primeCustomer.StartDate;
                    customerMemberShipDetail.EndDate = primeCustomer.EndDate;
                    customerMemberShipDetail.takenMemberId = primeCustomer.MemberShipId;
                    customerMemberShipDetail.BanifitList = new List<Banifit>();
                    customerMemberShipDetail.TotalBanifit = Convert.ToInt32(Math.Round(customerBanifits.TotalDiscount + customerBanifits.TotalOrderAmount, 0));
                    customerMemberShipDetail.PrimeHtmL = MemberShipBanifit != null ? (lang == "hi" ? MemberShipBanifit.PrimeHindiHtmL : MemberShipBanifit.PrimeHtmL) : "";
                    customerMemberShipDetail.Logo = MemberShipBanifit != null ? MemberShipBanifit.Logo : "";

                    if (customerBanifits.TotalDiscount > 0)
                    {
                        customerMemberShipDetail.BanifitList.Add(new Banifit
                        {
                            Id = 1,
                            Amount = Convert.ToInt32(customerBanifits.TotalDiscount),
                            text = lang == "hi" ? "ऑफर्स बचत" : "Offer Saving"
                        });
                    }
                    if (customerBanifits.TotalOrderAmount > 0)
                    {
                        customerMemberShipDetail.BanifitList.Add(new Banifit
                        {
                            Id = 2,
                            Amount = Convert.ToInt32(customerBanifits.TotalOrderAmount),
                            text = lang == "hi" ? "प्रोडक्ट्स की बचत" : "Products Saving"
                        });
                    }
                    customerMemberShipDetail.BanifitList.Add(new Banifit
                    {
                        Id = 3,
                        Amount = -1,
                        text = lang == "hi" ? "वितरण मुफ्त" : "Delivery Free"
                    });
                    customerMemberShipDetail.BanifitList.Add(new Banifit
                    {
                        Id = 4,
                        Amount = -1,
                        text = lang == "hi" ? "बेस्ट रेट प्रोडक्ट्स" : "Best rate on Products"
                    });

                    customerMemberShipDetail.BanifitList.Add(new Banifit
                    {
                        Id = 5,
                        Amount = -1,
                        text = lang == "hi" ? "जल्दी पहुंच डील्स और ऑफर्स" : "Early Access Deal And Offer"
                    });


                }
            }
            return customerMemberShipDetail;
        }


        [Route("GetDynamicHtml")]
        [HttpGet]
        public string DynamicHtmlApi(int customerId, int warehouseId)
        {
            var msg = "<p>You cannot pay on delivery for your prime membership. By signing up for a pain Prime membership, you agree to the <a href='https://www.shopkirana.in'><span style='color: rgb(0, 0, 0);'>Shopkirana Prime Terms &amp; Conditions.</span></a></p>  <p style='text-align: justify;'><span style='color: rgb(235, 107, 86);'><strong><span style='font-size: 19px;'>One membership, many benefits</span></strong></span></p>  <p style='text-align: justify;'><span style='color: rgb(235, 107, 86);'><strong><span style='font-size: 19px;'><img src='https://uat.shopkirana.in/images/GameLogo/1588643541.jpg'></span></strong></span><span style='color: rgb(0, 0, 0);'><strong><span style='font-size: 16px;'></span></strong></span></p>  <p style='text-align: left;'><strong>Shop with unlimited FREE Delivery at your door step on over 10,000 eligible items from India&#39;s biggest brands.</strong></p>  <p style='text-align: left;'><strong><img src='https://uat.shopkirana.in/images/GameLogo/Grofer Smart Bachat Club Members benefits.png'></strong></p><span style='color: rgb(235, 107, 86);'><strong><span style='font-size: 19px;'>              <p style='text-align: left;'><br></p>          </span></strong><strong><span style='font-size: 19px;'>              <p><br></p>          </span></strong></span><br>";

            return msg;
        }

        public double? ExpiringGamePoint(int CustomerId)
        {
            double? ExpiringGamePoint = 0;
            MongoDbHelper<Controllers.CustomerPlayedGame> mongoDbHelper = new MongoDbHelper<Controllers.CustomerPlayedGame>();
            ExpiringGamePoint = mongoDbHelper.Select(x => !x.IsExpired && x.CustomerId == CustomerId).Sum(x => x.Point);
            return ExpiringGamePoint;
        }

        [Route("GetGamePointExpiredHtml")]
        [HttpGet]
        public string GetGamePointExpiredHtml(int customerId, int warehouseId)
        {
            string expiredHtml = string.Empty;
            MongoDbHelper<Controllers.CustomerPlayedGame> mongoDbHelper = new MongoDbHelper<Controllers.CustomerPlayedGame>();
            DateTime startDate = DateTime.Now.Date.AddDays(-1);
            DateTime endDate = startDate.AddDays(1).AddMinutes(-1);
            var CustomerPlayedGames = mongoDbHelper.Select(x => x.CustomerId == customerId && !x.IsExpired).ToList();
            if (CustomerPlayedGames != null && CustomerPlayedGames.Any())
            {
                var totalPoint = CustomerPlayedGames.Select(x => x.Point).Sum();
                var minDate = CustomerPlayedGames.Select(x => x.CreatedDate).Min().AddHours(8);
                //
                string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/ExpiredGamePoint.html";
                string content = File.ReadAllText(pathToHTMLFile);
                string enddate = minDate.Month + "/" + minDate.Day + "/" + minDate.Year + " " + (minDate.Hour % 12) + ":" + minDate.Minute + ":" + minDate.Second + " " + minDate.ToString("tt");// .ToString("dd/MM/yyyy hh:mm:ss tt");
                expiredHtml = content.Replace("[LastDate]", enddate).Replace("[Point]", totalPoint.ToString());

            }
            return expiredHtml;
        }

        [Route("ExpiringGamePoint")]
        [HttpGet]
        public async Task<List<GamerWalletHistoryDc>> ExpiringGamePointList(int CustomerId)
        {
            List<GamerWalletHistoryDc> ExpiringGamePointList = new List<GamerWalletHistoryDc>();

            MongoDbHelper<Controllers.CustomerPlayedGame> mongoDbHelper = new MongoDbHelper<Controllers.CustomerPlayedGame>();

            var CustomerPlayedGames = mongoDbHelper.Select(x => !x.IsExpired && x.CustomerId == CustomerId && x.Point > 0).ToList();
            if (CustomerPlayedGames != null && CustomerPlayedGames.Any())
            {
                foreach (var points in CustomerPlayedGames)
                {
                    GamerWalletHistoryDc add = new GamerWalletHistoryDc();
                    add.Point = points.Point;
                    add.TransactionDate = points.CreatedDate;
                    add.ExpiringDate = points.CreatedDate.AddHours(8);
                    add.Through = points.GameName + " " + points.GamePayType;
                    ExpiringGamePointList.Add(add);
                }
            }
            return ExpiringGamePointList;
        }

        [Route("RetailerItemDetail")]
        [HttpGet]
        public async Task<ItemDataDC> RetailerItemDetail(int wareHouseId, int customerId, int itemId, string lang = "en")
        {
            List<ItemMaster> items = new List<ItemMaster>();
            using (var myContext = new AuthContext())
            {
                var ActiveCustomer = myContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                items = myContext.itemMasters.Where(x => x.ItemId == itemId && (x.ItemAppType == 0 || x.ItemAppType == 1) && x.active && !x.Deleted && x.WarehouseId == wareHouseId && (x.ItemAppType == 0 || x.ItemAppType == 1)).ToList();

                var retList = items.Select(a => new ItemDataDC
                {
                    WarehouseId = a.WarehouseId,
                    CompanyId = a.CompanyId,
                    ItemId = a.ItemId,
                    ItemNumber = a.Number,
                    itemname = a.itemname,
                    LogoUrl = a.LogoUrl,
                    MinOrderQty = a.MinOrderQty,
                    price = a.price,
                    TotalTaxPercentage = a.TotalTaxPercentage,
                    UnitPrice = a.UnitPrice,
                    active = a.active,
                    marginPoint = a.marginPoint,
                    NetPurchasePrice = a.NetPurchasePrice,
                    promoPerItems = a.promoPerItems,
                    IsOffer = a.IsOffer,
                    Deleted = a.Deleted,
                    OfferCategory = a.OfferCategory,
                    OfferStartTime = a.OfferStartTime,
                    OfferEndTime = a.OfferEndTime,
                    OfferQtyAvaiable = a.OfferQtyAvaiable,
                    OfferQtyConsumed = a.OfferQtyConsumed,
                    OfferId = a.OfferId,
                    OfferType = a.OfferType,
                    OfferWalletPoint = a.OfferWalletPoint,
                    OfferFreeItemId = a.OfferFreeItemId,
                    OfferPercentage = a.OfferPercentage,
                    OfferFreeItemName = a.OfferFreeItemName,
                    OfferFreeItemImage = a.OfferFreeItemImage,
                    OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                    OfferMinimumQty = a.OfferMinimumQty,
                    FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                    FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                    ItemAppType = a.ItemAppType,
                    TradePrice = a.TradePrice,
                    WholeSalePrice = a.WholeSalePrice
                }).OrderByDescending(x => x.ItemNumber).ToList();

                retList = await ItemValidate(retList, ActiveCustomer, myContext, lang);

                return retList.FirstOrDefault();
            }

        }



        [Route("GetPrimeItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetPrimeItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<ItemDataDC>() };
            using (var context = new AuthContext())
            {

                List<ItemDataDC> ItemDataDCs = new List<ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetPrimeItem]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);

            }

            return itemResponseDc;
        }

        [Route("GetItemClassification")]
        [HttpGet]

        public async Task<ItemResponseDc> GetItemClassification(int customerId, int warehouseId, int skip, int take, string lang, string Classification)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<ItemDataDC>() };
            using (var context = new AuthContext())
            {
                List<ItemDataDC> ItemDataDCs = new List<ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemClassificationForRetailers]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Classification", Classification));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);

            }

            return itemResponseDc;
        }

        [Route("InsertCustomerContacts")]
        [HttpPost]
        public async Task<bool> InsertCustomerContacts(List<ImportedContacts> importedContacts)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString))
            {
                conn.Open();
                using (SqlBulkCopy copy = new SqlBulkCopy(conn))
                {
                    copy.BulkCopyTimeout = 3600;
                    copy.BatchSize = 1000;
                    copy.DestinationTableName = "ImportedContacts";
                    DataTable table = ClassToDataTable.CreateDataTable(importedContacts);
                    table.TableName = "ImportedContacts";
                    await copy.WriteToServerAsync(table);
                }
            }

            using (var context = new AuthContext())
            {
                var customerId = importedContacts.FirstOrDefault().CustomerId;
                var customer = await context.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
                customer.IsContactsRead = true;
                context.Entry(customer).State = EntityState.Modified;
                context.Commit();
            }
            return true;
        }

        [Route("GetVideoHtml")]
        [HttpGet]
        public string GetVideoHtml(int CustomerId, int warehouseId)
        {
            string expiredHtml = string.Empty;
            MongoDbHelper<Controllers.WarehouseVideo> mongoDbHelper = new MongoDbHelper<Controllers.WarehouseVideo>();
            WarehouseVideo warehouseVideo = mongoDbHelper.Select(x => x.WarehouseId == warehouseId && !x.IsExpired && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now).FirstOrDefault();
            if (warehouseVideo != null && !string.IsNullOrEmpty(warehouseVideo.Videolink))
            {
                string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/RetailerVideo.html";
                string content = File.ReadAllText(pathToHTMLFile);
                expiredHtml = content.Replace("[PlayVideo]", warehouseVideo.Videolink);
                //"https://www.youtube.com/embed/g2WpYn_8j4M"
            }
            return expiredHtml;
        }

        [Route("ManageWarehouseVideo")]
        [HttpPost]
        public async Task<WarehouseVideoDTO> ManageWarehouseVideo(WarehouseVideoDc warehouseVideoDc)
        {
            WarehouseVideoDTO res;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            var result = false;
            MongoDbHelper<Controllers.WarehouseVideo> mongoDbHelper = new MongoDbHelper<Controllers.WarehouseVideo>();
            WarehouseVideo warehouseVideo = new WarehouseVideo();

            var exists = mongoDbHelper.Select(x => x.WarehouseId == warehouseVideoDc.WarehouseId && x.IsActive && x.StartDate >= warehouseVideoDc.StartDate && x.EndDate <= warehouseVideoDc.EndDate && x.Videolink == warehouseVideoDc.Videolink).Any();


            if (!exists)
            {
                if (!string.IsNullOrEmpty(warehouseVideoDc.Id))
                {
                    var objectid = new MongoDB.Bson.ObjectId(warehouseVideoDc.Id);
                    warehouseVideo = mongoDbHelper.Select(x => x.Id == objectid).FirstOrDefault();
                    warehouseVideo.ModifiedDate = DateTime.Now;
                    warehouseVideo.CreatedBy = userid;

                    result = await mongoDbHelper.ReplaceWithoutFindAsync(warehouseVideo.Id, warehouseVideo);
                    res = new WarehouseVideoDTO()
                    {

                        Status = result,
                        Message = ""
                    };
                }
                else
                {
                    warehouseVideo = Mapper.Map(warehouseVideoDc).ToANew<WarehouseVideo>();
                    warehouseVideo.CreatedDate = DateTime.Now;
                    warehouseVideo.CreatedBy = userid;
                    result = await mongoDbHelper.InsertAsync(warehouseVideo);
                    res = new WarehouseVideoDTO()
                    {

                        Status = result,
                        Message = "Save Successfully!!"
                    };
                }
            }
            else
            {
                res = new WarehouseVideoDTO()
                {

                    Status = false,
                    Message = "Already Have Video Please Deactivate previous one!!"
                };
            }



            return res;
        }



        [Route("GetVideoWarehouse")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetVideoWarehouse(int? warehouseId, DateTime StartDate, DateTime EndDate)
        {

            using (AuthContext db = new AuthContext())
            {
                List<WHDC> fto = new List<WHDC>();
                MongoDbHelper<WarehouseVideo> mongoDbHelper = new MongoDbHelper<WarehouseVideo>();
                var _collection = mongoDbHelper.mongoDatabase.GetCollection<WarehouseVideo>("Gid");
                var endDate = EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                var cartPredicate = LinqKit.PredicateBuilder.New<WarehouseVideo>();
                if (warehouseId != 0 && StartDate != null && EndDate != null)
                {


                    List<WarehouseVideo> VList = mongoDbHelper.Select(x => x.WarehouseId == warehouseId && x.StartDate >= StartDate && x.EndDate < endDate).ToList();

                    foreach (var VI in VList)
                    {
                        if (warehouseId != null)
                        {
                            var whName = db.Warehouses.Where(x => x.WarehouseId == VI.WarehouseId).FirstOrDefault();
                            WHDC vData = new WHDC()
                            {
                                Id = VI.Id,
                                CreatedDate = VI.CreatedDate,
                                WarehouseId = VI.WarehouseId,
                                StartDate = VI.StartDate,
                                EndDate = VI.EndDate,
                                Videolink = VI.Videolink,
                                IsActive = VI.IsActive,
                                IsDeleted = VI.IsDeleted,
                                IsExpired = VI.IsExpired,
                                WarehouseName = whName.WarehouseName
                            };
                            fto.Add(vData);
                        }
                    }
                }
                else
                {
                    List<WarehouseVideo> VList = mongoDbHelper.Select(x => x.StartDate >= StartDate && x.EndDate <= endDate).ToList();
                    foreach (var VI in VList)
                    {

                        if (warehouseId != null)
                        {
                            var whName = db.Warehouses.Where(x => x.WarehouseId == VI.WarehouseId).FirstOrDefault();



                            WHDC vData = new WHDC()
                            {
                                Id = VI.Id,
                                CreatedDate = VI.CreatedDate,
                                WarehouseId = VI.WarehouseId,
                                StartDate = VI.StartDate,
                                EndDate = VI.EndDate,
                                Videolink = VI.Videolink,
                                IsActive = VI.IsActive,
                                IsDeleted = VI.IsDeleted,
                                IsExpired = VI.IsExpired,
                                WarehouseName = whName.WarehouseName

                            };
                            fto.Add(vData);
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, fto);
            }


        }



        [Route("ActiveVideoDetails")]
        [HttpGet]
        public async Task<WarehouseVideo> ActiveDetails(string Id, bool IsActive)
        {
            using (var authContext = new AuthContext())
            {
                var result = false;
                MongoDbHelper<WarehouseVideo> mongoDbHelper = new MongoDbHelper<WarehouseVideo>();
                WarehouseVideo warehouseVideo = new WarehouseVideo();
                //{
                var objectid = new MongoDB.Bson.ObjectId(Id);
                warehouseVideo = mongoDbHelper.Select(x => x.Id == objectid).FirstOrDefault();
                if (warehouseVideo.Id != null)
                {
                    warehouseVideo.IsActive = IsActive;
                    warehouseVideo.IsExpired = IsActive == true ? false : true;
                    warehouseVideo.ModifiedDate = indianTime;
                    result = await mongoDbHelper.ReplaceWithoutFindAsync(warehouseVideo.Id, warehouseVideo);
                }
                return warehouseVideo;
            }
        }

        [Route("InsertCompanyDetail")]
        [HttpGet]
        [AllowAnonymous]
        public bool InsertCompanyDetail()
        {
            MongoDbHelper<ExtandedCompanyDetail> mongoDbHelper = new MongoDbHelper<ExtandedCompanyDetail>();
            ExtandedCompanyDetail extandedCompanyDetail = new ExtandedCompanyDetail
            {
                AppType = "Retailer",
                CityId = 1,
                ischeckBookShow = true,
                IsePayLaterShow = true,
                IsFinBox = true,
                IsOnlinePayment = true,
                IsRazorpayEnable = true,
                IsShowCreditOption = true,
                IsCreditLineShow = true,
                WarehouseId = 0
            };
            var result = mongoDbHelper.Insert(extandedCompanyDetail);
            return result;
        }

        [Route("GetTargetHtml")]
        [HttpGet]
        public string GetTargetHtml(int CustomerId, string SkCode, int warehouseId)
        {
            string expiredHtml = string.Empty;

            var target = new CustomerTarget();
            var date = DateTime.Now;
            var isMonthComplete = false;
            if (date.Day < 6)
            {
                isMonthComplete = true;
                date = DateTime.Now.AddMonths(-1);
            }
            var lastDayOfMonth = DateTime.DaysInMonth(date.Year, date.Month);
            string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/RetailerTarget.html";
            string content = File.ReadAllText(pathToHTMLFile);
            if (!string.IsNullOrEmpty(content))
            {
                MongoDbHelper<CustomersTargets.MonthlyCustomerTarget> MonthlyCustomerTarget = new MongoDbHelper<CustomersTargets.MonthlyCustomerTarget>();
                string DocumentName = "MonthlyTargetData_" + date.Month.ToString() + date.Year.ToString();
                var GetExportData = MonthlyCustomerTarget.Select(x => x.Skcode == SkCode, null, null, null, false, "", DocumentName).FirstOrDefault();
                if (GetExportData != null)
                {
                    var totalpuramt = GetExportData.CurrentVolume;
                    target.IsClaimed = GetExportData.IsClaimed;
                    target.SKCode = GetExportData.Skcode;
                    target.TargetAmount = GetExportData.Target;
                    target.LeftDays = isMonthComplete ? 0 : (lastDayOfMonth - date.Day);
                    target.Level = GetExportData.Levels;
                    target.TotalPurchaseAmount = Convert.ToDecimal(totalpuramt);
                    target.TotalPendingPurchaseAmount = Convert.ToDecimal(GetExportData.PendingVolume);
                    try
                    {
                        target.AchivePercent = target.TotalPurchaseAmount / (target.TargetAmount / 100);
                    }
                    catch (Exception)
                    {
                        target.AchivePercent = 0;
                    }
                }
                expiredHtml = content.Replace("[TargetPercent]", target.AchivePercent.ToString()).Replace("[Target]", string.Format("0.00", target.TotalPurchaseAmount)).Replace("[Target]", string.Format("0.00", target.TargetAmount)).Replace("[CurrentPurchase]", string.Format("0.00", target.TotalPurchaseAmount)).Replace("[LeftDays]", target.LeftDays.ToString());

            }
            return expiredHtml;
        }


        [Route("NotifyMe")]
        [HttpGet]
        public async Task<bool> NotifyMe(int CustomerId, int warehouseId, string itemNumber)
        {
            string expiredHtml = string.Empty;
            MongoDbHelper<CustomerItemNotifyMe> mongoDbHelper = new MongoDbHelper<CustomerItemNotifyMe>();
            var customerItemNotifyMe = mongoDbHelper.Select(x => x.WarehouseId == warehouseId && x.ItemNumber == itemNumber).FirstOrDefault();
            string fcmId = "";
            Customer DbCustomers = null;
            using (var context = new AuthContext())
            {
                DbCustomers = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                fcmId = DbCustomers.fcmId;
            }
            var result = false;
            if (customerItemNotifyMe == null)
            {
                customerItemNotifyMe = new CustomerItemNotifyMe
                {
                    LastUpdated = DateTime.Now,
                    ItemRequireCount = 1,
                    ItemNumber = itemNumber,
                    WarehouseId = warehouseId,
                    IsSentNotify = false,
                    Customers = new List<CustomerNotifyMe> { new CustomerNotifyMe {
                        CreatedDate=DateTime.Now,CustomerId=CustomerId,IsNotify=false,fcmId= fcmId
                        ,CustomerName=string.IsNullOrEmpty(DbCustomers.ShopName)?DbCustomers.Name:DbCustomers.ShopName
                        ,Mobile=DbCustomers.Mobile
                        ,Skcode=DbCustomers.Skcode
                    } }
                };
                result = await mongoDbHelper.InsertAsync(customerItemNotifyMe);
            }
            else
            {
                var IsExists = false;
                if (customerItemNotifyMe.Customers != null && customerItemNotifyMe.Customers.Any(s => s.CustomerId == CustomerId))
                {
                    if (!customerItemNotifyMe.Customers.FirstOrDefault(s => s.CustomerId == CustomerId).IsNotify)
                    {
                        IsExists = true;
                    }
                    customerItemNotifyMe.Customers.FirstOrDefault(s => s.CustomerId == CustomerId).CreatedDate = DateTime.Now;
                    customerItemNotifyMe.Customers.FirstOrDefault(s => s.CustomerId == CustomerId).IsNotify = false;
                    customerItemNotifyMe.Customers.FirstOrDefault(s => s.CustomerId == CustomerId).fcmId = fcmId;
                }
                else
                {
                    if (customerItemNotifyMe.Customers == null)
                    {
                        customerItemNotifyMe.Customers = new List<CustomerNotifyMe>();
                    }
                    customerItemNotifyMe.Customers.Add(new CustomerNotifyMe
                    {
                        CreatedDate = DateTime.Now,
                        CustomerId = CustomerId,
                        IsNotify = false,
                        fcmId = fcmId,
                        CustomerName = string.IsNullOrEmpty(DbCustomers.ShopName) ? DbCustomers.Name : DbCustomers.ShopName,
                        Mobile = DbCustomers.Mobile,
                        Skcode = DbCustomers.Skcode
                    });
                }

                customerItemNotifyMe.LastUpdated = DateTime.Now;
                if (!IsExists)
                    customerItemNotifyMe.ItemRequireCount += 1;
                customerItemNotifyMe.IsSentNotify = false;
                result = await mongoDbHelper.ReplaceAsync(customerItemNotifyMe.Id, customerItemNotifyMe);
            }

            return result;
        }

        [Route("SendNotifyNotification")]
        [HttpGet]
        [AllowAnonymous]
        public bool SendNotifyNotification()
        {
            AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
            return autoNotificationManager.SentNotifyItemNotification();
        }

        [Route("TopMarginItemNotification")]
        [HttpGet]
        [AllowAnonymous]
        public bool TopMarginItemNotification()
        {
            AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
            return autoNotificationManager.TopMarginItemNotification();
        }

        [Route("ShoppingCartNotification")]
        [HttpGet]
        [AllowAnonymous]
        public bool ShoppingCartNotification()
        {
            AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
            return autoNotificationManager.ShoppingCartNotification();
        }

        [Route("GamePointExpireNotification")]
        [HttpGet]
        [AllowAnonymous]
        public bool GamePointExpireNotification()
        {
            AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
            return autoNotificationManager.GamePointExpireNotification();
        }

        [Route("SendSMS")]
        [HttpPost]
        [AllowAnonymous]
        public bool SendSMS(SendSMS sendSMS)
        {
            return Common.Helpers.SendSMSHelper.SendSMS(sendSMS.MobileNo, sendSMS.Message, sendSMS.RouteId, "");
        }

        [Route("GetCartDealItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetCartDealItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<ItemDataDC>() };
            using (var context = new AuthContext())
            {

                List<ItemDataDC> ItemDataDCs = new List<ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                //var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetCartDealItem]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }

                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);

            }

            return itemResponseDc;
        }

        [Route("GetVATMUrl")]
        [HttpGet]
        public string GetVATMUrl(int CustomerId, int warehouseId)
        {
            string url = string.Empty;
            MongoDbHelper<VATMCustomers> VATMCustomershelper = new MongoDbHelper<VATMCustomers>();
            var vATMCustomer = VATMCustomershelper.Select(x => x.CustomerId == CustomerId && x.IsActive).FirstOrDefault();

            if (vATMCustomer != null && !string.IsNullOrEmpty(vATMCustomer.Data))
            {
                var todayDate = Encrypt(indianTime.ToString("ddMMMyyyyHHmm"));
                url = ConfigurationManager.AppSettings["VATMUrl"].ToString().Replace("[Token]", vATMCustomer.Data);
                url = url + "&ValidateData=" + todayDate;
                //todayDate = Decrypt(todayDate);
            }
            return url;
        }

        [HttpGet]
        [Route("DirectHtml")]

        public string DirectHtml()
        {
            string html = string.Empty;
            string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/Direct.html";
            string content = File.ReadAllText(pathToHTMLFile);
            html = content;
            return html;
        }

        #region VATM Encryption Decryption 

        private readonly static byte[] SALT = Encoding.ASCII.GetBytes(AppConstants.VATMEncryptionKey.Length.ToString());

        public string Encrypt(string inputText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] plainText = Encoding.Unicode.GetBytes(inputText);
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(AppConstants.VATMEncryptionKey, SALT);
            using (ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16)))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                        return HttpUtility.UrlEncode(Convert.ToBase64String(memoryStream.ToArray()));
                    }
                }
            }
        }

        public string Decrypt(string inputText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] encryptedData = Convert.FromBase64String(inputText);
            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(AppConstants.VATMEncryptionKey, SALT);
            using (ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
            {
                using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] plainText = new byte[encryptedData.Length];
                        int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);
                        return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
                    }
                }
            }
        }


        #endregion

        #region Get Apphome Bottom Call 

        [Route("GetApphomeBottomCall")]
        [HttpGet]
        public async Task<List<ApphomeBottomCallDc>> GetApphomeBottomCall(int customerId)
        {
            List<ApphomeBottomCallDc> result = new List<ApphomeBottomCallDc>();
            using (var context = new AuthContext())
            {
                var tripParam = new SqlParameter("CustomerId", customerId);
                var customerTrips = await context.Database.SqlQuery<long>("exec operation.GetCustomerCurrentTrips @CustomerId", tripParam).ToListAsync();

                if (customerTrips != null && customerTrips.Any())
                {
                    result.AddRange(customerTrips.Select(x => new ApphomeBottomCallDc
                    {
                        Id = Convert.ToInt32(x),
                        Type = 3, // OrderTrack
                        RelativeUrl = "api/OrderProcess/GetCustomerTrip/{Id}/{CustomerId}"
                    }).ToList());
                }


                var Deliveryparam = new SqlParameter("CustomerId", customerId);
                var DeliveryRatingOrders = await context.Database.SqlQuery<int>("exec operation.GetOrderForDeliveryRating @CustomerId", Deliveryparam).ToListAsync();
                if (DeliveryRatingOrders != null && DeliveryRatingOrders.Any())
                {

                    result.AddRange(DeliveryRatingOrders.Select(x => new ApphomeBottomCallDc
                    {
                        Id = x,
                        Type = 2, // Delivery Rating
                        RelativeUrl = "api/RetailerApp/GetDboyRatingOrder/{Id}"
                    }).ToList());
                }

                var param = new SqlParameter("CustomerId", customerId);
                var SalesRatingOrders = await context.Database.SqlQuery<int>("exec operation.GetOrderForSalesRating @CustomerId", param).ToListAsync();
                if (SalesRatingOrders != null && SalesRatingOrders.Any())
                {

                    result.AddRange(SalesRatingOrders.Select(x => new ApphomeBottomCallDc
                    {
                        Id = x,
                        Type = 1, // SalesMan Rating
                        RelativeUrl = "api/RetailerApp/GetSalesManRatingOrder/{Id}"
                    }).ToList());
                }


            }
            return result;
        }
        #endregion

        #region  Rating

        //Insert Rating
        [Route("InsertRating")]
        [HttpPost]
        public async Task<bool> InsertRating(UserRatingDc AddObj)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                UserRating AddRating = new UserRating();
                AddRating = Mapper.Map(AddObj).ToANew<UserRating>();
                AddRating.UserRatingDetails = new List<UserRatingDetail>();
                if (!AddObj.IsRemoveFront)
                {
                    foreach (var item in AddObj.RatingDetails)
                    { UserRatingDetail od = new UserRatingDetail(); od.Detail = item.Detail; AddRating.UserRatingDetails.Add(od); }
                }
                AddRating.CreatedDate = DateTime.Now;
                AddRating.ModifiedDate = DateTime.Now;
                AddRating.IsTrip = false;
                AddRating.IsActive = true;
                AddRating.IsDeleted = false;
                AddRating.CreatedBy = userid;
                context.UserRatings.Add(AddRating);
                result = context.Commit() > 0;
            }
            return result;
        }

        //Sales Man Rating   (type 1)
        [Route("GetSalesManRatingOrder/{Id}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<UserRatingDc>> GetSalesManRatingOrder(int Id)  //Id : OrderId
        {
            List<UserRatingDc> result = new List<UserRatingDc>();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("OrderId", Id);
                var Order = await context.Database.SqlQuery<SalesManRatingOrderDc>("exec operation.GetSalesManRatingOrder @OrderId", param).FirstOrDefaultAsync();
                if (Order != null)
                {
                    var ratingConfig = await context.RatingMasters.Where(x => x.AppType == 1 && x.IsActive == true && x.IsDeleted == false).Include(x => x.RatingDetails).ToListAsync();
                    result = Mapper.Map(ratingConfig).ToANew<List<UserRatingDc>>();
                    foreach (var item in result)
                    {
                        item.UserId = Order.UserId;
                        item.OrderId = Order.OrderId;
                        item.ProfilePic = Order.ProfilePic;
                        item.OrderedDate = Order.OrderedDate;
                        item.DisplayName = Order.DisplayName;
                    }
                    return result.OrderBy(x => x.Rating).ToList();
                }
            }
            return result;
        }

        // Delivery Boy Rating  (type 2)
        [Route("GetDboyRatingOrder/{Id}")]
        [HttpGet]
        public async Task<List<UserRatingDc>> GetDboyRatingOrder(int Id) //Id : OrderId
        {
            List<UserRatingDc> result = new List<UserRatingDc>();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("OrderId", Id);
                var Order = await context.Database.SqlQuery<DboyRatingOrderDc>("exec operation.GetRetailerDboyRatingOrder @OrderId", param).FirstOrDefaultAsync();
                if (Order != null)
                {
                    var ratingConfig = await context.RatingMasters.Where(x => x.AppType == 2 && x.IsActive == true && x.IsDeleted == false).Include(x => x.RatingDetails).ToListAsync();
                    ratingConfig.ForEach(x => x.RatingDetails = x.RatingDetails.Where(y => y.IsActive == true && y.IsDeleted == false).ToList());

                    result = Mapper.Map(ratingConfig).ToANew<List<UserRatingDc>>();
                    if (Order != null && ratingConfig != null)
                    {
                        foreach (var item in result)
                        {
                            item.UserId = Order.UserId;
                            item.OrderId = Order.OrderId;
                            item.ProfilePic = Order.ProfilePic;
                            item.OrderedDate = Order.OrderedDate;
                            item.DisplayName = Order.DisplayName;
                            item.AppTypeName = "Delivery Rating";
                        }
                    }
                    return result.OrderBy(x => x.Rating).ToList();
                }
            }
            return result;
        }
        #endregion

        //Retailer Rating (type 3)
        [Route("GetRetailerRatingOrder/{Id}")]
        [HttpGet]
        public async Task<List<UserRatingDc>> GetRetailerRatingOrder(int Id)  //Id : OrderId
        {
            List<UserRatingDc> result = new List<UserRatingDc>();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("OrderId", Id);
                var Order = await context.Database.SqlQuery<SalesManRatingOrderDc>("exec operation.GetRetailerRatingOrder @OrderId", param).FirstOrDefaultAsync();
                var ratingConfig = await context.RatingMasters.Where(x => x.AppType == 3 && x.IsActive == true && x.IsDeleted == false).Include(x => x.RatingDetails).ToListAsync();
                result = Mapper.Map(ratingConfig).ToANew<List<UserRatingDc>>();
                foreach (var item in result)
                {
                    item.UserId = Order.UserId;
                    item.OrderId = Order.OrderId;
                    item.ProfilePic = Order.ProfilePic;
                    item.OrderedDate = Order.OrderedDate;
                    item.DisplayName = Order.DisplayName;
                }
            }
            return result.OrderBy(x => x.Rating).ToList();
        }

        //Insert Retailer Rating
        [Route("AddRating")]
        [HttpPost]
        public async Task<bool> AddRating(UserRatingDc AddObj)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                UserRating AddRating = new UserRating();
                AddRating = Mapper.Map(AddObj).ToANew<UserRating>();
                AddRating.UserRatingDetails = new List<UserRatingDetail>();
                if (!AddObj.IsRemoveFront)
                {
                    foreach (var item in AddObj.RatingDetails)
                    { UserRatingDetail od = new UserRatingDetail(); od.Detail = item.Detail; AddRating.UserRatingDetails.Add(od); }
                }
                AddRating.CreatedDate = DateTime.Now;
                AddRating.ModifiedDate = DateTime.Now;
                AddRating.IsTrip = true;
                AddRating.IsActive = true;
                AddRating.IsDeleted = false;
                AddRating.CreatedBy = userid;
                context.UserRatings.Add(AddRating);
                result = context.Commit() > 0;
            }
            return result;
        }

        [Route("GetCustomerSalesPersons/{customerId}")]
        [HttpGet]
        public async Task<List<CustomerSalesPersons>> GetCustomerSalesPersons(int customerId)
        {
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("customerid", customerId);
                var salesPersonList = await context.Database.SqlQuery<CustomerSalesPersons>("exec GetCustomerExecutiveWithStore @customerid", param).ToListAsync();

                return salesPersonList;
            }
        }

        [Route("GetCustReferralConfigurations")]
        [HttpGet]
        public List<GetCustReferralConfigDc> GetCustReferralConfigurations(int CityId)
        {
            List<GetCustReferralConfigDc> custReferralConfigList = new List<GetCustReferralConfigDc>();
            using (var db = new AuthContext())
            {
                custReferralConfigList = db.CustomerReferralConfigurationDb.Where(x => x.CityId == CityId && x.ReferralType == 1 && x.IsActive == true && x.IsDeleted == false)
                     .Select(x => new GetCustReferralConfigDc
                     {
                         OnOrder = x.OnOrder,
                         ReferralWalletPoint = x.ReferralWalletPoint,
                         CustomerWalletPoint = x.CustomerWalletPoint,
                         OnDeliverd = x.OnDeliverd
                     }).ToList();
                var statusids = custReferralConfigList.Select(x => x.OnDeliverd).Distinct().ToList();
                var customerReferralStatus = db.CustomerReferralStatusDb.Where(x => statusids.Contains((int)x.Id) && x.IsActive == true && x.IsDeleted == false).ToList();
                custReferralConfigList.ForEach(x =>
                {
                    x.OrderCount = x.OnOrder + " Order";
                    x.orderStatus = customerReferralStatus != null ? customerReferralStatus.FirstOrDefault(y => y.Id == x.OnDeliverd).OrderStatus : "NA";
                });
                return custReferralConfigList;
            }
        }
        [Route("GetCustomerReferralOrderList")]
        [HttpGet]
        public List<GetCustomerReferralOrderListDc> GetCustomerReferralOrderList(int customerId)
        {
            using (var context = new AuthContext())
            {
                var customerIds = new SqlParameter("@CustomerId", customerId);
                List<GetCustomerReferralOrderListDc> CustomerReferralList = context.Database.SqlQuery<GetCustomerReferralOrderListDc>("exec GetCustomerReferralOrderList @CustomerId", customerIds).ToList();
                return CustomerReferralList;
            }
        }
        [Route("InsertReferralWalletPointJob")]
        [HttpGet]
        public void InsertReferralWalletPointJob()
        {
            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 3600;
                var data = context.Database.ExecuteSqlCommand("exec GetReferralWalletPointJob");
            }
        }
        [Route("GetCustomerReferralData")]
        [HttpPost]
        public List<GetCustomerReferralDataList> GetCustomerReferralData(GetCustomerReferralDataDC getCustomerReferralDataDC)
        {
            List<GetCustomerReferralDataList> List = new List<GetCustomerReferralDataList>();
            using (var db = new AuthContext())
            {
                int Skiplist = (getCustomerReferralDataDC.Skip - 1) * getCustomerReferralDataDC.Take;
                var referralType = new SqlParameter("@ReferralType", getCustomerReferralDataDC.ReferralType);
                var skip = new SqlParameter("@Skip", Skiplist);
                var take = new SqlParameter("@Take", getCustomerReferralDataDC.Take);
                var keyType = new SqlParameter("@KeyType", getCustomerReferralDataDC.KeyType);

                List = db.Database.SqlQuery<GetCustomerReferralDataList>("EXEC GetCustomerReferralData @ReferralType,@Skip,@Take,@KeyType", referralType, skip, take, keyType).ToList();
                return List;
            }
        }
        [Route("GetCustomerWiseReferralList")]
        [HttpGet]
        public List<GetCustomerWiseReferralListDc> GetCustomerWiseReferralList(string Skcode)
        {
            List<GetCustomerWiseReferralListDc> CustomerReferralList = new List<GetCustomerWiseReferralListDc>();
            if (!string.IsNullOrEmpty(Skcode))
            {
                using (var context = new AuthContext())
                {
                    var skcode = new SqlParameter("@Skcode", Skcode);
                    CustomerReferralList = context.Database.SqlQuery<GetCustomerWiseReferralListDc>("exec GetCustomerWiseReferralList @Skcode", skcode).ToList();
                    return CustomerReferralList;
                }
            }
            else
            {
                return null;
            }
        }

        [Route("GetCustomerAddress")]
        [HttpGet]
        public async Task<CustomerAddressDc> GetCustomerAddress(int CustomerId)
        {
            var result = new CustomerAddressDc();
            using (AuthContext context = new AuthContext())
            {

                result = context.CustomerAddressDB.Where(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).Select(x => new CustomerAddressDc
                {
                    CityPlaceId = x.CityPlaceId,
                    CustomerId = x.CustomerId,
                    AddressLineOne = x.AddressLineOne,
                    AddressLineTwo = x.AddressLineTwo,
                    AddressLng = x.AddressLng,
                    AddressLat = x.AddressLat,
                    AreaLat = x.AreaLat,
                    AreaLng = x.AreaLng,
                    AddressPlaceId = x.AddressPlaceId,
                    AreaPlaceId = x.AreaPlaceId,
                    ZipCode = x.ZipCode,
                    AddressText = x.AddressText,
                    AreaText = x.AreaText,
                }).FirstOrDefault();
                if (result != null) { result.CityName = context.Customers.FirstOrDefault(z => z.CustomerId == CustomerId).City; }
            }
            return result;
        }

        public List<GetCustomerWalletHistory> GetHistoryofRetailerReward(int CustomerId)
        {
            using (var db = new AuthContext())
            {
                List<GetCustomerWalletHistory> retailergame = new List<GetCustomerWalletHistory>();
                RewardEarningHistoryDc Earning = new RewardEarningHistoryDc();

                int BucketNo = 0; int BucketNoTo = 1; int skip1 = 0; int take1 = 0; int IsActiveCurrent1 = 0;
                var fBucketNoFrom = new SqlParameter("@BucketNoFrom", BucketNo);
                var fBucketNoTo = new SqlParameter("@BucketNoTo", BucketNoTo);
                var fskip = new SqlParameter("@skip", skip1);
                var ftake = new SqlParameter("@take", take1);
                var fCreatedDate = new SqlParameter("@CreatedDate", DBNull.Value);
                var fIsActiveCurrent = new SqlParameter("@IsActiveCurrent", IsActiveCurrent1);
                var SqlStreakLevelConfigMasters = db.Database.SqlQuery<GameStreakLevelConfigDetailDc>("exec sp_GameStreakLevelConfigDetail @BucketNoFrom,@BucketNoTo, @skip, @take,@CreatedDate,@IsActiveCurrent", fBucketNoFrom, fBucketNoTo, fskip, ftake, fCreatedDate, fIsActiveCurrent).ToList();

                MongoDbHelper<GameCustomerLedger> mongoDbHelperCustomerLedger = new MongoDbHelper<GameCustomerLedger>();
                var dataCustomerLedgerList = mongoDbHelperCustomerLedger.Select(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false).ToList();

                var UpCommingPoint = dataCustomerLedgerList.Where(x => x.IsUpComingReward == true).Sum(x => x.RewardValue);
                var TotalEarningPoint = dataCustomerLedgerList.Where(x => x.IsCompleted == true).Sum(x => x.RewardValue);

                var GamesHistory = dataCustomerLedgerList.Select(x =>
                new CreateCustomerHistoryDc
                {
                    CustomerId = x.CustomerId,
                    GameBucketNo = x.GameBucketNo > 0 ? x.GameBucketNo - 1 : dataCustomerLedgerList.Where(z => z.BucketNo == x.BucketNo && z.GameBucketNo > 0).Select(z => z.GameBucketNo).FirstOrDefault() - 1,
                    BucketNo = x.BucketNo,
                    ForRewardStrack = x.ForRewardStrack == 1 ? "Reward" : x.ForRewardStrack == 2 ? "Strack" : "Redeem",   //"Reward =1  / Strack=2 / Redeem = 3
                    GameBucketRewardId = x.GameBucketRewardId,
                    GameStreakLevelConfigMasterId = x.GameStreakLevelConfigMasterId,
                    GameStreakLevelConfigDetailId = x.GameStreakLevelConfigDetailId,
                    StreakIdFrom = x.StreakIdFrom,
                    StreakIdTo = x.StreakIdTo,
                    RewardValue = x.RewardValue,
                    RewardStatus = x.IsUpComingReward == true ? "UpComing" : x.IsCompleted == true ? "Completed" : x.IsCanceled == true ? "Canceled" : "",
                    RewardStatusDate = x.IsUpComingReward == true ? x.IsUpComingRewardDate : x.IsCompleted == true ? x.IsCompletedDate : x.IsCanceled == true ? x.IsCanceledDate : null,
                    BucketStartDate = x.BucketStartDate,
                    BucketEndDate = x.BucketEndDate,
                    GameType = x.ForRewardStrack == 1 ? "Reward"
                    : x.ForRewardStrack == 3 ? "Redeem"
                    : SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.Type).FirstOrDefault(),
                    GameCondition = x.ForRewardStrack == 1 ? "-" : x.ForRewardStrack == 3 ? "-" :
                    SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.Type).FirstOrDefault() == "Outof"
                    ? SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.Condition).FirstOrDefault() + " / " + SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.OutOf_OutOfBucket).FirstOrDefault()
                    : SqlStreakLevelConfigMasters.Where(z => z.Id == x.GameStreakLevelConfigMasterId && z.GameStreakLevelConfigDetailId == x.GameStreakLevelConfigDetailId).Select(z => z.Condition).FirstOrDefault(),

                }
               ).ToList();

                if (GamesHistory.Count > 0)
                {

                    foreach (var data in GamesHistory)
                    {
                        GetCustomerWalletHistory getCustomerWalletHistory = new GetCustomerWalletHistory();
                        if (data.RewardStatus != "UpComing")
                        {
                            if (data.ForRewardStrack == "Reward")
                            {
                                getCustomerWalletHistory.TransactionDate = data.RewardStatusDate;
                                getCustomerWalletHistory.NewAddedWAmount = data.RewardValue;
                                getCustomerWalletHistory.Through = "Level " + data.GameBucketNo + " " + data.RewardStatus;
                            }
                            if (data.ForRewardStrack == "Strack")
                            {
                                if (data.StreakIdFrom == data.StreakIdTo)
                                {
                                    getCustomerWalletHistory.TransactionDate = data.RewardStatusDate;
                                    getCustomerWalletHistory.NewAddedWAmount = data.RewardValue;
                                    getCustomerWalletHistory.Through = "Streak-Individual " + data.StreakIdFrom + " " + data.RewardStatus;
                                }
                                else
                                {
                                    getCustomerWalletHistory.TransactionDate = data.RewardStatusDate;
                                    getCustomerWalletHistory.NewAddedWAmount = data.RewardValue;
                                    getCustomerWalletHistory.Through = "Streak from " + data.StreakIdFrom + " to" + data.StreakIdTo + data.RewardStatus;
                                }
                            }
                            if (getCustomerWalletHistory.NewAddedWAmount > 0)
                            {
                                retailergame.Add(getCustomerWalletHistory);
                            }
                        }
                    }
                }

                return retailergame;
            }

        }

        [Route("GetOfferVisibilityItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<DataContracts.External.SalesItemResponseDc> GetOfferVisibilityItem(int customerId, int offerId, int SubCategoryId, int brandId, int step, int skip, int take, string lang, bool IsSalesApp = false)
        {
            Customer ActiveCustomer = new Customer();
            List<ItemDataDC> ItemData = new List<ItemDataDC>();
            List<ItemDataDC> ItemDataDCs = new List<ItemDataDC>();
            skip = skip / take;
            var itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.SalesAppItemDataDC>() };
            using (var context = new AuthContext())
            {

                ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var IdDt = new DataTable();
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetOfferVisibilityItem]";
                cmd.Parameters.Add(new SqlParameter("@offerId", offerId));
                cmd.Parameters.Add(new SqlParameter("@SubCategoryId", SubCategoryId));
                cmd.Parameters.Add(new SqlParameter("@BrandId", brandId));
                cmd.Parameters.Add(new SqlParameter("@Step", step));
                cmd.Parameters.Add(new SqlParameter("@skip", (skip * take)));
                cmd.Parameters.Add(new SqlParameter("@take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }

                ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang, IsSalesApp);

            }
            itemResponseDc.ItemDataDCs = ItemDataDCs.GroupBy(x => new { x.ItemNumber }).Select(x => new DataContracts.External.SalesAppItemDataDC
            {
                BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                BillLimitQty = x.FirstOrDefault().BillLimitQty,
                Categoryid = x.FirstOrDefault().Categoryid,
                CompanyId = x.FirstOrDefault().CompanyId,
                dreamPoint = x.FirstOrDefault().dreamPoint,
                HindiName = x.FirstOrDefault().HindiName,
                IsItemLimit = x.FirstOrDefault().IsItemLimit,
                IsOffer = x.FirstOrDefault().IsOffer,
                ItemId = x.FirstOrDefault().ItemId,
                ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                ItemMultiMRPId = x.FirstOrDefault().ItemMultiMRPId,
                itemname = x.FirstOrDefault().itemname,
                ItemNumber = x.FirstOrDefault().ItemNumber,
                Itemtype = x.FirstOrDefault().Itemtype,
                LastOrderDate = x.FirstOrDefault().LastOrderDate,
                LastOrderDays = x.FirstOrDefault().LastOrderDays,
                LastOrderQty = x.FirstOrDefault().LastOrderQty,
                LogoUrl = x.FirstOrDefault().LogoUrl,
                marginPoint = x.FirstOrDefault().marginPoint,
                MinOrderQty = x.FirstOrDefault().MinOrderQty,
                OfferCategory = x.FirstOrDefault().OfferCategory,
                OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                OfferId = x.FirstOrDefault().OfferId,
                OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                OfferType = x.FirstOrDefault().OfferType,
                OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                price = x.FirstOrDefault().price,
                Sequence = x.FirstOrDefault().Sequence,
                SubCategoryId = x.FirstOrDefault().SubCategoryId,
                SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                UnitPrice = x.FirstOrDefault().UnitPrice,
                WarehouseId = x.FirstOrDefault().WarehouseId,
                Classification = x.FirstOrDefault().Classification,
                BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                Active = x.FirstOrDefault().active,
                OfferEndTime = x.FirstOrDefault().OfferEndTime,
                OfferQtyAvaiable = x.FirstOrDefault().OfferQtyAvaiable,
                OfferQtyConsumed = x.FirstOrDefault().OfferQtyConsumed,
                OfferStartTime = x.FirstOrDefault().OfferStartTime,
                CurrentStartTime = x.FirstOrDefault().CurrentStartTime,
                moqList = x.Count() > 1 ? x.Select(y => new DataContracts.External.SalesAppItemDataDC
                {
                    isChecked = (y.ItemMultiMRPId == x.FirstOrDefault().ItemMultiMRPId && y.MinOrderQty == x.FirstOrDefault().MinOrderQty),
                    BaseCategoryId = y.BaseCategoryId,
                    BillLimitQty = y.BillLimitQty,
                    Categoryid = y.Categoryid,
                    CompanyId = y.CompanyId,
                    dreamPoint = y.dreamPoint,
                    HindiName = y.HindiName,
                    IsItemLimit = y.IsItemLimit,
                    IsOffer = y.IsOffer,
                    ItemId = y.ItemId,
                    ItemlimitQty = y.ItemlimitQty,
                    ItemMultiMRPId = y.ItemMultiMRPId,
                    itemname = y.itemname,
                    ItemNumber = y.ItemNumber,
                    Itemtype = y.Itemtype,
                    LastOrderDate = y.LastOrderDate,
                    LastOrderDays = y.LastOrderDays,
                    LastOrderQty = y.LastOrderQty,
                    LogoUrl = y.LogoUrl,
                    marginPoint = y.marginPoint,
                    MinOrderQty = y.MinOrderQty,
                    OfferCategory = y.OfferCategory,
                    OfferFreeItemId = y.OfferFreeItemId,
                    OfferFreeItemImage = y.OfferFreeItemImage,
                    OfferFreeItemName = y.OfferFreeItemName,
                    OfferFreeItemQuantity = y.OfferFreeItemQuantity,
                    OfferId = y.OfferId,
                    OfferMinimumQty = y.OfferMinimumQty,
                    OfferType = y.OfferType,
                    OfferWalletPoint = y.OfferWalletPoint,
                    price = y.price,
                    Sequence = y.Sequence,
                    SubCategoryId = y.SubCategoryId,
                    SubsubCategoryid = y.SubsubCategoryid,
                    UnitPrice = y.UnitPrice,
                    WarehouseId = y.WarehouseId,
                    Classification = x.FirstOrDefault().Classification,
                    BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                    Active = x.FirstOrDefault().active,
                    OfferEndTime = x.FirstOrDefault().OfferEndTime,
                    OfferQtyAvaiable = x.FirstOrDefault().OfferQtyAvaiable,
                    OfferQtyConsumed = x.FirstOrDefault().OfferQtyConsumed,
                    OfferStartTime = x.FirstOrDefault().OfferStartTime,
                    CurrentStartTime = x.FirstOrDefault().CurrentStartTime,
                }).ToList() : new List<DataContracts.External.SalesAppItemDataDC>()
            }).OrderBy(x => x.Sequence).ToList();
            return itemResponseDc;
        }

        [Route("GetOfferCompanybrandList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CatScatSscatDCs> GetOfferCompanybrandList(int customerId, int offerId, int SubCategoryId, int brandId, int step, string lang)
        {
            CatScatSscatDCs catScatSscatDCs = new CatScatSscatDCs()
            {
                categoryDC = new List<CategoryDCs>(),
                subCategoryDC = new List<SubCategoryDCs>(),
                subsubCategoryDc = new List<SubsubCategoryDcs>()
            };
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var IdDt = new DataTable();
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetOfferCompanybrandList]";
                cmd.Parameters.Add(new SqlParameter("@offerId", offerId));
                cmd.Parameters.Add(new SqlParameter("@SubCategoryId", SubCategoryId));
                cmd.Parameters.Add(new SqlParameter("@BrandId", brandId));
                cmd.Parameters.Add(new SqlParameter("@Step", step));
                //cmd.Parameters.Add(new SqlParameter("@skip", (skip * take)));
                //cmd.Parameters.Add(new SqlParameter("@take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                catScatSscatDCs.subCategoryDC = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<SubCategoryDCs>(reader).ToList();
                reader.NextResult();
                if (reader.HasRows)
                {
                    catScatSscatDCs.subsubCategoryDc = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<SubsubCategoryDcs>(reader).ToList();
                }
            }
            return catScatSscatDCs;
        }

        [Route("RemoveOffer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> RemoveOffer(int customerId, int WarehouseId, int PeopleId = 0)
        {
            bool Status = false;
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == WarehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (PeopleId > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == PeopleId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            if (customerShoppingCart != null)
            {
                if (customerShoppingCart.ShoppingCartDiscounts.Any() && customerShoppingCart.ShoppingCartDiscounts != null)
                {
                    customerShoppingCart.ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
                    Status = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                }
            }
            return Status;
        }





    }
}


#region Return Dc
public class GetCustomerWiseReferralListDc
{
    public string City { get; set; }
    public string Skcode { get; set; }
    public string ReferralType { get; set; }
    public int OnOrder { get; set; }
    public string OrderStatus { get; set; }
    public double ReferralWalletPoint { get; set; }
    public double CustomerWalletPoint { get; set; }
    public string Action { get; set; }
    public DateTime CreatedDate { get; set; }
}
public class GetCustomerReferralDataList
{
    public string ReferralSkCode { get; set; }
    public string ShopName { get; set; }
    public string Mobile { get; set; }
    public double TotalEarnedPoint { get; set; }
    public double TotalPendingEarnedPoint { get; set; }
    public double CustomerReferralWalletPoint { get; set; }
    public double PendingCustomerReferralWalletPoint { get; set; }
    public int TotalCount { get; set; }
}
public class GetCustomerReferralDataDC
{
    public int ReferralType { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public string KeyType { get; set; }
}
public class GetCustomerReferralOrderListDc
{
    public string SkCode { get; set; }
    public string ReferralSkCode { get; set; }
    public double ReferralWalletPoint { get; set; }
    public double CustomerWalletPoint { get; set; }
    public int OnOrder { get; set; }
    public bool NewSignup { get; set; }
    public int OrderId { get; set; }
    public string ShopName { get; set; }
    public string Status { get; set; }
    public int IsUsed { get; set; }
    public DateTime? CreatedDate { get; set; }
}

public class AppReferralHomeData
{
    public int TotalReferral { get; set; }
    public int TotalOrder { get; set; }
    public int RewardsEarned { get; set; }
    public List<GetCustomerReferralOrderListDc> CustomerReferrallst { get; set; }
}
public class SendSMS
{
    public string MobileNo { get; set; }
    public string Message { get; set; }
    public string RouteId { get; set; }
}

public class WarehouseVideoDTO
{
    public bool Status { get; set; }
    public string Message { get; set; }
}
public class Banifit
{
    public int Id { get; set; }
    public string text { get; set; }
    public int Amount { get; set; }
}
public class WHDC
{
    public ObjectId Id { get; set; }
    //public string Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public string Videolink { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public int? ModifiedBy { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreatedDate { get; set; }
}
public class customerBanifit
{
    public double TotalDiscount { get; set; }
    public double TotalOrderAmount { get; set; }
}

public class CustomerMemberShipDetail
{
    public int takenMemberId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string MemberShipName { get; set; }
    public string Logo { get; set; }
    public int TotalBanifit { get; set; }
    public List<Banifit> BanifitList { get; set; }
    public string PrimeHtmL { get; set; }
}

public class PaymentRequest
{
    public int MemberShipId { get; set; }
    public string GatewayTransId { get; set; }
    public string Status { get; set; }
    public string GatewayRequest { get; set; }
    public string GatewayResponse { get; set; }
    public string PaymentFrom { get; set; }
    public int CustomerId { get; set; }
    public long TransId { get; set; }
}

public class PaymentRequestResponse
{
    public bool status { get; set; }
    public string message { get; set; }
    public long TransId { get; set; }
    public int MemberShipId { get; set; }
}

public class MemberShipDc
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int MemberShipInMonth { get; set; }
    public string MemberShipLogo { get; set; }
    public string MemberShipName { get; set; }
    public string MemberShipDescription { get; set; }
    public int MemberShipSequence { get; set; }
    public bool taken { get; set; }
    public int takenMemberId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class WRSITEMs
{
    public List<factoryItemdata> ItemMasters { get; set; }
    public List<factorySubSubCategory> SubsubCategories { get; set; }
    public string Message { get; set; }
    public bool Status { get; set; }
}

public class Cutomerfavourite
{
    public List<favItem> items { get; set; }
    public int customerId { get; set; }
    public int WarehouseId { get; set; }
    public string lang { get; set; }
}
public class Messagedata
{
    public CustomizedPrepaidOrders CustomizedPrepaidOrders { get; set; }
    public CompnayWheelConfigDc CompnayWheelConfig { get; set; }
    public bool Status { get; set; }
    public string message { get; set; }
}

public class CompnayWheelConfigDc
{
    public int LineItemCount { get; set; }
    public decimal OrderAmount { get; set; }
    public bool IsKPPRequiredWheel { get; set; }
}


public class NotificationFlashDeal
{
    public bool FlashDealStatus { get; set; }
    public int SectionID { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? NonPrimeStartTime { get; set; }
    public bool IsPrimeCustomer { get; set; }
    public string LogoUrl { get; set; }
}

public class RetailerStore
{
    public int SubCategoryId { get; set; }
    public string SubCategoryName { get; set; }

    public string Logo { get; set; }
}


public class ItemResponseDc
{
    public int TotalItem { get; set; }
    public List<ItemDataDC> ItemDataDCs { get; set; }
}

public class OtpCheckDc
{
    public string MobileNumber { get; set; }
    public string deviceId { get; set; }
    public string Otp { get; set; }
    public bool trueCustomer { get; set; }
    public string CurrentAPKversion { get; set; }
    public string PhoneOSversion { get; set; }
    public string UserDeviceName { get; set; }
    public string fcmId { get; set; }
}

public class WarehouseVideoDc
{
    public string Id { get; set; }
    public int WarehouseId { get; set; }

    public string Videolink { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public int? ModifiedBy { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreatedDate { get; set; }
}



#endregion








