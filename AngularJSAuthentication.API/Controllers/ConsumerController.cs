using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.External.MobileExecutiveDC;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.OrderMastersAPIController;
using AngularJSAuthentication.Model.Login;
using System.Collections.Concurrent;
using System.IO;
using AngularJSAuthentication.Model.CustomerDelight;
using static AngularJSAuthentication.API.Controllers.CustomersController;
using AngularJSAuthentication.API.Controllers.External.Gamification;
using AngularJSAuthentication.DataContracts.CustomerReferralDc;
using System.Web;
using AngularJSAuthentication.Common.Helpers;
using Newtonsoft.Json;
using Nito.AsyncEx;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using OpenHtmlToPdf;
using AngularJSAuthentication.DataContracts.Consumer;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using GenricEcommers.Models;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.DataContracts.ElasticLanguageSearch;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using Nito.AspNetBackgroundTasks;
using System.Data.Entity.Infrastructure;
using static AngularJSAuthentication.API.Controllers.InActiveCustOrderMasterController;
using static AngularJSAuthentication.API.Controllers.OrderMasterrController;
using AngularJSAuthentication.API.Helper.Razorpay;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.DataContracts.RazorPay;
using AngularJSAuthentication.API.Helper.PaymentRefund;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using AngularJSAuthentication.Model.Consumer;
using AngularJSAuthentication.DataContracts.Transaction.OrderProcess;
using AngularJSAuthentication.Model.PaymentRefund;
using NLog;
using QRCoder;
using System.Drawing;
using AngularJSAuthentication.API.Helper.Notification;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ConsumerApp")]
    public class ConsumerController : BaseAuthController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static List<int> retailercustomerIds = new List<int>();
        OrderPlaceHelper orderPlaceHelper = new OrderPlaceHelper();
        public bool EnableOtherLanguage = false;
        public bool ElasticSearchEnable = AppConstants.ElasticSearchEnable;
        public bool ZilaElasticSearchEnable = AppConstants.ZilaElasticSearchEnable;
        public int MemberShipHours = AppConstants.MemberShipHours;
        public double xPointValue = AppConstants.xPoint;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("ConsumerProfileUpdate")]
        [HttpPut]
        public async Task<CustUpdateDetails> ConsumerProfileUpdate(ConsumerUpdateDC cust)
        {
            using (AuthContext context = new AuthContext())
            {
                RetailerAppDC Updatecustomer = new RetailerAppDC();
                CustUpdateDetails res;
                string message = string.Empty;
                var customerVerify = context.Customers.Where(x => x.Mobile == cust.Mobile).Include(x => x.ConsumerAddress).FirstOrDefault();

                Customer custdata = customerVerify;
                if (custdata != null)
                {
                    custdata.WhatsappNumber = cust.WhatsappNumber;
                    custdata.AnniversaryDate = cust.AnniversaryDate;
                    custdata.DOB = cust.DOB;
                    custdata.Emailid = cust.Email;
                    custdata.Name = cust.Name;
                    custdata.IsSignup = string.IsNullOrEmpty(cust.Name) == true ? false : true;

                    if (!string.IsNullOrEmpty(cust.UploadProfilePichure))
                    {
                        custdata.UploadProfilePichure = cust.UploadProfilePichure;
                    }
                    context.Entry(custdata).State = EntityState.Modified;
                    context.Commit();
                    Updatecustomer = Mapper.Map(custdata).ToANew<RetailerAppDC>();
                    Updatecustomer.CustomerType = "consumer";
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
                        Status = false,
                        Message = "Customer not found."
                    };
                    return res;
                }
            }
        }
        /// <summary>
        /// B2C
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        [Route("ConsumerOrderPlace")]
        [HttpPost]
        public async Task<HttpResponseMessage> ConsumerOrderPlace(ShoppingCart sc)
        {

            using (var context = new AuthContext())
            {

                var placeOrderResponse = new Model.PlaceOrder.PlaceOrderResponse();

                #region Handle Single Customer Request
                if (retailercustomerIds.Any(x => x == sc.CustomerId.Value))
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "आप का आर्डर प्रोसेस मे है प्लीज प्रतिक्षा करे।";
                    placeOrderResponse.cart = null;
                    return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
                }
                else
                {
                    retailercustomerIds.Add(sc.CustomerId.Value);
                }


                BlockGullakAmount blockGullakAmount = null;
                MongoDbHelper<BlockGullakAmount> mongoDbHelper = new MongoDbHelper<BlockGullakAmount>();
                if (sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
                {
                    blockGullakAmount = new BlockGullakAmount
                    {
                        CreatedDate = DateTime.Now,
                        CustomerId = sc.CustomerId.Value,
                        Guid = Guid.NewGuid().ToString(),
                        Amount = sc.GulkAmount,
                        IsActive = true
                    };
                    await mongoDbHelper.InsertAsync(blockGullakAmount);
                    sc.BlockGullakAmountGuid = blockGullakAmount.Guid;
                }
                #endregion

                #region Get Customer COD Limit
                CustomerCODLimitDc codLimitDc = await orderPlaceHelper.GetConsumerCODLimit(sc.CustomerId.Value);
                if (codLimitDc != null && codLimitDc.CODLimit > 0 && codLimitDc.IsActive == true && codLimitDc.IsCustomCODLimit == true)
                {
                    if ((codLimitDc.AvailableCODLimit - sc.CODAmount) < 0)
                    {
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "COD Limit Exceed";
                        placeOrderResponse.cart = null;
                        retailercustomerIds.Remove(sc.CustomerId.Value);
                        return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
                    }
                }
                #endregion

                try
                {
                    placeOrderResponse = await orderPlaceHelper.PushOrderMasterV6(sc, context);//Post order
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (retailercustomerIds.Any(x => x == sc.CustomerId.Value))
                    {
                        retailercustomerIds.Remove(sc.CustomerId.Value);
                    }
                    if (sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
                    {
                        var cartPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == sc.CustomerId && x.Guid == blockGullakAmount.Guid);
                        var blockGullakAmountdb = mongoDbHelper.Select(cartPredicate).FirstOrDefault();
                        blockGullakAmountdb.IsActive = false;
                        var result = await mongoDbHelper.ReplaceAsync(blockGullakAmountdb.Id, blockGullakAmountdb);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, placeOrderResponse);
            }
        }
        [HttpGet]
        [Route("ConsumerAppVersion")]
        [AllowAnonymous]
        public async Task<List<ConsumerappVersion>> Get()
        {
            using (AuthContext context = new AuthContext())
            {
                var item = context.ConsumerAppVersionDb.Where(x => x.IsActive).ToList();
                return item;
            }

        }



        [Route("ConsumerVerifyOTP")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<APIResponse> ConsumerVerifyOTP(ConsumerDC retailerAppDC)
        {
            try
            {
                RetailerAppDC newcustomer = new RetailerAppDC();
                Customer cust = new Customer();
                List<string> FCMIds = new List<string>();
                MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
                var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == retailerAppDC.MobileNumber);
                var CustomerOTPs = mongoDbHelper.Select(cartPredicate).ToList();
                if (CustomerOTPs != null && CustomerOTPs.Any(x => x.Otp == retailerAppDC.Otp))
                {
                    foreach (var item in CustomerOTPs)
                    {
                        await mongoDbHelper.DeleteAsync(item.Id);
                    }


                    using (var context = new AuthContext())
                    {
                        cust = context.Customers.Where(x => x.Deleted == false && x.Mobile == retailerAppDC.MobileNumber).Include(x => x.ConsumerAddress).FirstOrDefault();
                        if (cust != null)
                        {
                            if (cust.IsB2CApp)
                            {
                                Wallet d = new Wallet();
                                var wallet = context.WalletDb.Where(c => c.CustomerId == cust.CustomerId).FirstOrDefault();
                                if (wallet == null)
                                {
                                    d.ShopName = cust.ShopName;
                                    d.Skcode = cust.Skcode;
                                    d.CustomerId = cust.CustomerId;
                                    d.TotalAmount = 0;
                                    d.CreatedDate = indianTime;
                                    d.UpdatedDate = indianTime;
                                    d.Deleted = false;
                                    d.CompanyId = 1;
                                    context.WalletDb.Add(d);
                                    context.Commit();
                                }
                                if (!string.IsNullOrEmpty(retailerAppDC.deviceId))
                                {
                                    var deviceLogins = context.CustomerLoginDeviceDB.Where(x => x.CustomerId == cust.CustomerId).ToList();
                                    if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == retailerAppDC.deviceId && x.IsCurrentLogin))
                                    {
                                        var login = deviceLogins.FirstOrDefault(x => x.DeviceId == retailerAppDC.deviceId);
                                        login.LastLogin = DateTime.Now;
                                        login.CurrentAPKversion = retailerAppDC.CurrentAPKversion;
                                        login.PhoneOSversion = retailerAppDC.PhoneOSversion;
                                        context.Entry(login).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    else
                                    {

                                        if (deviceLogins == null || !deviceLogins.Any())
                                        {
                                            var login = new CustomerLoginDevice()
                                            {
                                                DeviceId = retailerAppDC.deviceId,
                                                CustomerId = cust.CustomerId,
                                                CreatedDate = DateTime.Now,
                                                IsCurrentLogin = true,
                                                LastLogin = DateTime.Now,
                                                CurrentAPKversion = retailerAppDC.CurrentAPKversion,
                                                PhoneOSversion = retailerAppDC.PhoneOSversion,
                                                UserDeviceName = retailerAppDC.UserDeviceName,
                                                FCMID = retailerAppDC.fcmId,
                                                Mobile = retailerAppDC.MobileNumber,
                                                AppType = 1
                                            };
                                            context.Entry(login).State = EntityState.Added;

                                        }
                                        else if (deviceLogins != null && !deviceLogins.Any(x => x.DeviceId == retailerAppDC.deviceId))
                                        {
                                            var login = new CustomerLoginDevice()
                                            {
                                                DeviceId = retailerAppDC.deviceId,
                                                CustomerId = cust.CustomerId,
                                                CreatedDate = DateTime.Now,
                                                IsCurrentLogin = true,
                                                LastLogin = DateTime.Now,
                                                CurrentAPKversion = retailerAppDC.CurrentAPKversion,
                                                PhoneOSversion = retailerAppDC.PhoneOSversion,
                                                UserDeviceName = retailerAppDC.UserDeviceName,
                                                FCMID = retailerAppDC.fcmId,
                                                Mobile = retailerAppDC.MobileNumber,
                                                AppType = 1
                                            };
                                            context.Entry(login).State = EntityState.Added;
                                            FCMIds.AddRange(deviceLogins.Where(x => x.IsCurrentLogin && !string.IsNullOrEmpty(x.FCMID)).Select(x => x.FCMID).ToList());
                                            deviceLogins.Where(x => x.IsCurrentLogin).ToList().ForEach(x =>
                                            {
                                                x.IsCurrentLogin = false;
                                                context.Entry(x).State = EntityState.Modified;
                                            });

                                        }
                                        else if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == retailerAppDC.deviceId && !x.IsCurrentLogin))
                                        {
                                            var login = deviceLogins.FirstOrDefault(x => x.DeviceId == retailerAppDC.deviceId && !x.IsCurrentLogin);
                                            login.LastLogin = DateTime.Now;
                                            login.IsCurrentLogin = true;
                                            login.CurrentAPKversion = retailerAppDC.CurrentAPKversion;
                                            login.PhoneOSversion = retailerAppDC.PhoneOSversion;
                                            context.Entry(login).State = EntityState.Modified;

                                            FCMIds.AddRange(deviceLogins.Where(x => x.DeviceId != retailerAppDC.deviceId && x.IsCurrentLogin && !string.IsNullOrEmpty(x.FCMID)).Select(x => x.FCMID).ToList());
                                            deviceLogins.Where(x => x.DeviceId != retailerAppDC.deviceId && x.IsCurrentLogin).ToList().ForEach(x =>
                                            {
                                                x.IsCurrentLogin = false;
                                                context.Entry(x).State = EntityState.Modified;
                                            });
                                        }

                                        context.Commit();


                                        if (FCMIds.Any())
                                        {

                                            string Key = ConfigurationManager.AppSettings["ConsumerFcmKey"];
                                            if (!string.IsNullOrEmpty(Key))
                                            {
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
                                                var firebaseService1 = new FirebaseNotificationServiceHelper(Key);

                                                ParallelLoopResult parellelResult = Parallel.ForEach(FCMIds,async (x) =>
                                                {
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

                                                        var results =await firebaseService1.SendNotificationForApprovalAsync(x, data);
                                                        if (results != null)
                                                        {
                                                            
                                                        }
                                                        else
                                                        {
                                                            
                                                        }
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
                                //if ((cust.CustomerType != null && cust.CustomerType.ToLower() != "consumer") || cust.CustomerType == null)
                                //    return new APIResponse { Status = false, Message = "You are not authorize" };
                                // else if (cust.CustomerType != null && cust.CustomerType.ToLower() == "consumer")
                                cust.ConsumerAddress = null;
                                newcustomer = Mapper.Map(cust).ToANew<RetailerAppDC>();
                                if (cust.ConsumerAddress != null && cust.ConsumerAddress.Any(x => x.Default))
                                {
                                    var defaultadd = cust.ConsumerAddress.FirstOrDefault(x => x.Default);
                                    var cluster = context.Clusters.FirstOrDefault(x => x.WarehouseId == defaultadd.WarehouseId);
                                    newcustomer.ShippingAddress = defaultadd.CompleteAddress;
                                    newcustomer.Warehouseid = defaultadd.WarehouseId;
                                    newcustomer.City = defaultadd.CityName;
                                    newcustomer.Cityid = defaultadd.Cityid;
                                    newcustomer.State = defaultadd.StateName;
                                    newcustomer.ClusterId = cluster.ClusterId;
                                    newcustomer.ZipCode = defaultadd.ZipCode;
                                    newcustomer.lat = defaultadd.lat;
                                    newcustomer.lg = defaultadd.lng;
                                    newcustomer.LandMark = defaultadd.LandMark;
                                    newcustomer.Name = defaultadd.PersonName;
                                    newcustomer.ShopName = defaultadd.PersonName;
                                }
                                else
                                {
                                    newcustomer.Warehouseid = 0;
                                    newcustomer.ShippingAddress = null;
                                }
                                return new APIResponse { Status = true, Data = newcustomer };
                            }
                            return new APIResponse { Status = false, Message = "Customer Not Active!!" };
                        }
                        else
                        {
                            var Skcode = skcode();
                            Customer c = new Customer();
                            c.Mobile = retailerAppDC.MobileNumber;
                            c.CompanyId = 1;
                            c.Skcode = Skcode;
                            c.CustomerType = "Consumer";
                            c.Active = true;
                            c.Deleted = false;
                            c.CreatedDate = indianTime;
                            c.UpdatedDate = indianTime;
                            context.Customers.Add(c);
                            context.Commit();
                            #region CODLimitCustomers
                            var codLimit = context.CompanyDetailsDB.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false);
                            CODLimitCustomer codLimitCustomer = new CODLimitCustomer();
                            codLimitCustomer.CustomerId = c.CustomerId;
                            codLimitCustomer.CODLimit = codLimit.CODLimit;
                            codLimitCustomer.IsActive = false;
                            codLimitCustomer.IsDeleted = false;
                            codLimitCustomer.IsCustomCODLimit = true;
                            codLimitCustomer.CreatedDate = DateTime.Now;
                            context.CODLimitCustomers.Add(codLimitCustomer);
                            if (!context.WalletDb.Any(x => x.CustomerId == c.CustomerId))
                            {
                                Wallet d = new Wallet();
                                d.ShopName = c.ShopName;
                                d.Skcode = c.Skcode;
                                d.CustomerId = c.CustomerId;
                                d.TotalAmount = 0;
                                d.CreatedDate = indianTime;
                                d.UpdatedDate = indianTime;
                                d.Deleted = false;
                                context.WalletDb.Add(d);
                            }
                            context.Commit();
                            #endregion
                            return new APIResponse { Status = true, Data = c };

                        }

                    }
                    return new APIResponse { Status = false, Message = "Something Went wrong" };
                }
                else
                {
                    return new APIResponse { Status = false, Message = "Invalid OTP" };
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
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

        [Route("ConsumerCommonDiscountOffer")]
        [HttpGet]
        public async Task<DataContracts.External.OfferdataDc> ConsumerCommonDiscountOffer(int CustomerId, string lang = "en")
        {
            List<DataContracts.External.OfferDc> FinalBillDiscount = new List<DataContracts.External.OfferDc>();
            DataContracts.External.OfferdataDc res;
            using (AuthContext context = new AuthContext())
            {
                CustomersManager manager = new CustomersManager();

                List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetWareBillDiscount(CustomerId, "Consumer");
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
                        var bdcheck = new DataContracts.External.OfferDc
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
                            OfferItems = billDiscountOfferDc.OfferItems.Select(y => new DataContracts.External.OfferItemdc
                            {
                                IsInclude = y.IsInclude,
                                itemId = y.itemId
                            }).ToList(),
                            RetailerBillDiscountFreeItemDcs = BillDiscountFreeItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).Select(x => new AngularJSAuthentication.DataContracts.External.RetailerBillDiscountFreeItemDc
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
                res = new DataContracts.External.OfferdataDc()
                {
                    offer = FinalBillDiscount.OrderBy(x => x.start).ToList(),
                    Status = true,
                    Message = "Success"
                };
                return res;
            }

        }

        [Route("SendInvoice")]
        [HttpGet]
        [AllowAnonymous]
        public string SendInvoice(int Orderid, string Mobile)
        {
            string result = string.Empty;
            using (var db = new AuthContext())
            {
                string FileName = "";
                string folderPath = "";
                string orderids = Orderid.ToString();
                FileName = orderids.Replace('/', '_') + ".pdf";
                folderPath = HttpContext.Current.Server.MapPath(@"~\ConsumerOrders");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                var fullPhysicalPath = folderPath + "\\" + FileName;
                var orderdata = db.DbOrderMaster.FirstOrDefault(x => x.OrderId == Orderid && x.IsInvoiceSent == true);
                if (!File.Exists(fullPhysicalPath))
                {

                }
                else
                {
                    if (!string.IsNullOrEmpty(Mobile))
                    {
                        bool isSent = false;
                        string Message = "";
                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "StoreOrderDelivered");
                        Message = dltSMS == null ? "" : dltSMS.Template;
                        Message = Message.Replace("{#var1#}", "User");
                        Message = Message.Replace("{#var2#}", Orderid.ToString());
                        string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                    , "/ConsumerOrders/" + Orderid + ".pdf");
                        TextFileLogHelper.LogError("Error ConsumerOrder FileUrl" + FileUrl);
                        string shortUrl = Helpers.ShortenerUrl.ShortenUrl(FileUrl);
                        TextFileLogHelper.LogError("Error ConsumerOrder shortUrl" + shortUrl);
                        Message = Message.Replace("{#var3#}", shortUrl);
                        TextFileLogHelper.LogError("Error ConsumerOrder SMS" + Message);
                        if (dltSMS != null)
                            isSent = Common.Helpers.SendSMSHelper.SendSMS(Mobile, Message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                        if (isSent)
                        {
                            if (orderdata == null)
                            {
                                orderdata.IsInvoiceSent = true;
                                db.Entry(orderdata).State = EntityState.Modified;
                                db.Commit();
                            }
                            result = "Sent Succesfullly.";
                        }
                        else
                        {
                            result = "Not Sent.";
                        }
                    }
                    else
                    {
                        result = "Please Enter Mobile Number";
                    }
                }
                return result;
            }
        }

        [Route("WarehouseChange")]
        [HttpPost]
        public bool WarehouseChange(LatLongChangeDC latLongChange)
        {
            //using (var context = new AuthContext())
            //{
            //    //if (latLongChange.lat != 0 && latLongChange.lg != 0)
            //    //{
            //    var customers = context.Customers.Where(x => x.CustomerId == latLongChange.CustomerId && !x.Deleted).FirstOrDefault();
            //    //var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(latLongChange.lat).Append("', '").Append(latLongChange.lg).Append("')");
            //    var query = new StringBuilder("select [dbo].[GetClusterFromLatLng_Store]('").Append(latLongChange.lat).Append("', '").Append(latLongChange.lg).Append("', '").Append(Convert.ToBoolean(1)).Append("')");//0-->Warehuose,1-->Store
            //    var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
            //    if (!clusterId.HasValue)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        var dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
            //        if (customers.Warehouseid == dd.WarehouseId)
            //            return false;
            //        else
            //            return true;
            //    }
            //}

            return false;
        }

        [Route("SignupUpdate")]
        [HttpPost]
        public async Task<APIResponse> SignupUpdate(ConsumerUpdateDC consumerUpdate)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var Customer = context.Customers.Where(x => x.CustomerId == consumerUpdate.CustomerId && !x.Deleted).FirstOrDefault();
                    if (Customer != null)
                    {
                        Customer.IsSignup = string.IsNullOrEmpty(consumerUpdate.Name) ? false : true;
                        Customer.Name = consumerUpdate.Name;
                        Customer.WhatsappNumber = consumerUpdate.WhatsappNumber;
                        Customer.DOB = consumerUpdate.DOB;
                        Customer.AnniversaryDate = consumerUpdate.AnniversaryDate;
                        Customer.Emailid = consumerUpdate.Email;
                        Customer.LastModifiedBy = Customer.Name;
                        Customer.UpdatedDate = DateTime.Now;
                        context.Entry(Customer).State = EntityState.Modified;
                        if (context.Commit() > 0)
                            return new APIResponse { Status = true, Message = "Signup Successfully", Data = Customer };
                        else
                            return new APIResponse { Status = false, Message = "Something went wrong" };
                    }
                    else
                        return new APIResponse { Status = false, Message = "Not Found" };
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }


        [Route("OrderCanceled")]
        [HttpGet]
        public async Task<APIResponse> OrderCanceled(int OrderId)
        {
            string msg = string.Empty;
            bool IsInserted = false;
            using (var context = new AuthContext())
            {
                if (OrderId > 0)
                {
                    var order = context.DbOrderMaster.Where(x => x.OrderId == OrderId).Include(x => x.orderDetails).FirstOrDefault();
                    if (order != null)
                    {
                        order.Status = "Order Canceled";
                        order.UpdatedDate = DateTime.Now;
                        if (order.orderDetails != null && order.orderDetails.Any())
                        {
                            foreach (var data in order.orderDetails)
                            {
                                data.Status = "Order Canceled";
                                data.UpdatedDate = DateTime.Now;
                            }
                        }
                        context.Entry(order).State = EntityState.Modified;
                        OrderMasterHistories oh = new OrderMasterHistories();
                        oh.orderid = OrderId;
                        oh.Status = "Order Canceled";
                        oh.Warehousename = order.WarehouseName;
                        oh.userid = order.CustomerId;
                        oh.CreatedDate = DateTime.Now;
                        oh.username = "By Customer";
                        context.OrderMasterHistoriesDB.Add(oh);

                        //case 3 : online payment list

                        var OnlineEntries = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == OrderId && z.IsOnline && z.status == "Success").ToList();
                        List<PaymentResponseRetailerApp> paymentRetailapp = new List<PaymentResponseRetailerApp>();
                        if (OnlineEntries != null && OnlineEntries.Any() && OnlineEntries.Sum(x => x.amount) == order.GrossAmount)
                        {
                            var RefundDays = context.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();
                            //double NetRefundAmount = OnlineEntries.Sum(x => x.amount) - order.GrossAmount;// Calculate Net total refund amount
                            PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                            foreach (var item in OnlineEntries.OrderBy(c => c.RefundPriority).OrderByDescending(c => c.id))
                            {
                                var PaymentRefundDays = RefundDays.FirstOrDefault(x => x.ApiName.Trim().ToLower() == item.PaymentFrom.Trim().ToLower());
                                if (PaymentRefundDays != null && PaymentRefundDays.DaysForRefundEligible > 0 && item.CreatedDate.AddDays(PaymentRefundDays.DaysForRefundEligible) < indianTime)
                                {
                                    msg = "Can't dispatch cut item  order , because online payment refund days expired for Order  : " + OrderId;
                                    return new APIResponse { Status = false, Message = msg };
                                }
                                else if (PaymentRefundDays == null && item.PaymentFrom.Trim().ToLower() != "gullak")
                                {
                                    msg = "refund apis or refund days not configured for payment mode " + item.PaymentFrom;
                                    return new APIResponse { Status = false, Message = msg };
                                }

                                double RefundAmount = item.amount;
                                //double RefundAmount = NetRefundAmount - sourceAmount > 0 ? sourceAmount : NetRefundAmount;
                                if (RefundAmount > 0)
                                {
                                    // addd Refund request
                                    var PaymentRefundRequestDc = new PaymentRefundRequest
                                    {
                                        Amount = RefundAmount,
                                        OrderId = item.OrderId,
                                        Source = item.PaymentFrom,
                                        Status = (int)PaymentRefundEnum.Initiated,
                                        CreatedBy = order.CustomerId,
                                        CreatedDate = indianTime,
                                        IsActive = true,
                                        IsDeleted = false,
                                        ModifiedBy = order.CustomerId,
                                        ModifiedDate = indianTime,
                                        RefundType = (int)RefundTypeEnum.Auto
                                    };

                                    var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                    {
                                        amount = (-1) * RefundAmount,
                                        CreatedDate = indianTime,
                                        currencyCode = "INR",
                                        OrderId = OrderId,
                                        PaymentFrom = item.PaymentFrom,
                                        GatewayTransId = item.GatewayTransId,
                                        GatewayOrderId = item.GatewayOrderId,
                                        status = "Success",
                                        UpdatedDate = indianTime,
                                        IsRefund = false,
                                        IsOnline = true,
                                        statusDesc = "Refund Initiated",
                                        paymentRefundRequest = new List<PaymentRefundRequest> { PaymentRefundRequestDc }
                                    };
                                    context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);

                                    paymentRetailapp.Add(PaymentResponseRetailerAppDb);
                                    //IsInserted = PRHelper.InsertPaymentRefundRequest(context, PaymentRefundRequestDc);
                                }
                            }
                        }

                        #region wallet point return

                        if (order.walletPointUsed > 0)
                        {
                            var wallet = context.WalletDb.FirstOrDefault(x => x.CustomerId == order.CustomerId);
                            if (wallet != null && order.walletPointUsed > 0)
                            {
                                wallet.TotalAmount = wallet.TotalAmount + order.walletPointUsed;
                                wallet.UpdatedDate = DateTime.Now;
                                context.Entry(wallet).State = EntityState.Modified;

                                CustomerWalletHistory obj = new CustomerWalletHistory();
                                obj.CustomerId = order.CustomerId;
                                obj.WarehouseId = order.WarehouseId;
                                obj.CompanyId = 1;

                                obj.OrderId = order.OrderId;
                                obj.NewAddedWAmount = order.walletPointUsed;
                                obj.TotalWalletAmount += obj.NewAddedWAmount;
                                obj.Through = "Zila order canceled.";
                                obj.TransactionDate = DateTime.Now;
                                obj.CreatedDate = DateTime.Now;
                                context.CustomerWalletHistoryDb.Add(obj);
                            }
                        }
                        #endregion

                        if (context.Commit() > 0)
                        {
                            if (paymentRetailapp.Any())
                            {
                                #region insert History
                                foreach (var item in paymentRetailapp.SelectMany(x => x.paymentRefundRequest))
                                {
                                    PaymentRefundHistory addHistory = new PaymentRefundHistory
                                    {
                                        PaymentRefundRequestId = item.Id,
                                        Status = (int)PaymentRefundEnum.Initiated,
                                        CreatedDate = DateTime.Now,
                                        CreatedBy = item.CreatedBy
                                    };
                                    context.PaymentRefundHistories.Add(addHistory);
                                }
                                context.Commit();
                                #endregion
                            }
                            return new APIResponse { Status = true, Message = "Order Cancelled" };
                        }
                        else
                        {
                            return new APIResponse { Status = false, Message = "Something went wrong" };
                        }
                    }
                    else
                        return new APIResponse { Status = false, Message = "Order Not Found" };
                }
                else
                    return new APIResponse { Status = false, Message = "Order Not Found" };
            }
        }

        [Route("RetailerGetItembycatesscatid")]
        [HttpGet]
        public async Task<DataContracts.External.ItemListDc> RetailerGetItembycatesscatid(string lang, int customerId, int catid, int scatid, int sscatid, string sortType, string direction, int skip = 0, int take = 10)
        {
            RetailerAppManager manager = new RetailerAppManager();
            return await manager.RetailerGetItembycatesscatid(lang, customerId, catid, scatid, sscatid, skip, take, sortType, direction, "Consumer");
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
                var ActiveCustomer = db.Customers.Where(x => x.CustomerId == searchitem.CustomerId).Include(x => x.ConsumerAddress).FirstOrDefault(); //added by Anurag 04-05-2021
                var defaultadd = ActiveCustomer.ConsumerAddress.FirstOrDefault(x => x.Default);
                ActiveCustomer.Warehouseid = defaultadd.WarehouseId;
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;

                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;


                var newdata = new List<DataContracts.External.ItemDataDC>();


                TextFileLogHelper.TraceLog("Item elastic Search:" + ZilaElasticSearchEnable.ToString());
                if (ZilaElasticSearchEnable)
                {
                    TextFileLogHelper.TraceLog("Item elastic Search:");
                    List<string> activelst = new List<string>();
                    //if (!searchitem.IsActive)
                    //{
                    //    activelst.Add("true");
                    //    activelst.Add("false");
                    //}
                    //else
                    //{
                    activelst.Add("true");
                    //}
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelperElastic = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "SuggestionItemSearch");
                    var searchQuery = mongoDbHelperElastic.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var suggest = new DataContracts.ElasticSearch.Suggest();

                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#keyword#}", searchitem.itemkeyword);
                    List<ElasticSearchItem> elasticSearchItemsSuggestion = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    TextFileLogHelper.TraceLog($"Item elastic Suggetion: {query}");

                    if (suggest != null && suggest.namesuggester != null && suggest.namesuggester.Any(x => x.options.Any()))
                    {
                        List<KeyValuePair<string, string>> itemNames = new List<KeyValuePair<string, string>>();
                        foreach (var suggitem in suggest.namesuggester)
                        {
                            itemNames.Add(new KeyValuePair<string, string>
                            (
                                suggitem.text,
                                suggitem.options.Any() ? suggitem.options.FirstOrDefault().text : suggitem.text
                            ));
                        }
                        foreach (var suggitem in itemNames)
                        {
                            searchitem.itemkeyword = searchitem.itemkeyword.Replace(suggitem.Key, suggitem.Value);
                        }

                    }

                    searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "ConsumerItemSearch");
                    searchQuery = mongoDbHelperElastic.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();

                    suggest = new DataContracts.ElasticSearch.Suggest();
                    string Uomstr = string.IsNullOrEmpty(searchitem.UOM) ? "" : (",{\"term\": {\"uom\":\"{UOM}\"}}").Replace("{UOM}", searchitem.UOM);
                    string Uoq = string.IsNullOrEmpty(searchitem.Unit) ? "" : (",{\"term\": {\"unitofquantity\":{UOQ}}}".Replace("{UOQ}", searchitem.Unit));
                    string catstr = searchitem.Category.Any() ? (",{\"terms\": {\"categoryid\":\"[{cat}]\"}}".Replace("{cat}", string.Join(",", searchitem.Category))) : "";
                    string basecatstr = searchitem.BaseCat.Any() ? (",{\"terms\": {\"basecategoryid\":\"[{basecat}]\"}}".Replace("{basecat}", string.Join(",", searchitem.BaseCat))) : "";
                    string subcatstr = searchitem.SubCat.Any() ? (",{\"terms\": {\"subcategoryid\":\"[{subcat}]\"}}".Replace("{subcat}", string.Join(",", searchitem.SubCat))) : "";
                    string subsubcatstr = searchitem.Brand.Any() ? (",{\"terms\": {\"subsubcategoryid\":\"[{brand}]\"}}".Replace("{brand}", string.Join(",", searchitem.Brand))) : "";
                    query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
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
                    TextFileLogHelper.TraceLog($"Item elastic data: {query}");

                    #region oldcode
                    /*
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
                        suggest = new DataContracts.ElasticSearch.Suggest();
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
                    */
                    #endregion
                    newdata = Mapper.Map(elasticSearchItems).ToANew<List<DataContracts.External.ItemDataDC>>();
                }
                else
                {
                    List<bool> activelst = new List<bool>();
                    //if (!searchitem.IsActive)
                    //{
                    //    activelst.Add(true);
                    //    activelst.Add(false);
                    //}
                    //else
                    //{
                    activelst.Add(true);
                    //}
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
                               select new DataContracts.External.ItemDataDC
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
                                   MinOrderQty = 1,
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

                newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                             Select(x => new DataContracts.External.ItemDataDC
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
                item.ItemMasters = new List<factoryItemdata>();
                var formatedData = await ItemValidateConsumerApp(newdata, ActiveCustomer, db, searchitem.lang);
                List<factoryItemdata> Data = Mapper.Map(formatedData).ToANew<List<factoryItemdata>>();
                item.ItemMasters.AddRange(Data);

                if (item.ItemMasters != null && item.ItemMasters.Any())
                {
                    #region ItemNetInventoryCheck
                    if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                    {
                        if (item.ItemMasters != null && item.ItemMasters.Any())
                        {
                            var itemInventory = new DataTable();
                            itemInventory.Columns.Add("ItemMultiMRPId");
                            itemInventory.Columns.Add("WarehouseId");
                            itemInventory.Columns.Add("Qty");
                            itemInventory.Columns.Add("isdispatchedfreestock");

                            foreach (var items in item.ItemMasters)
                            {
                                var dr = itemInventory.NewRow();
                                dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                                dr["WarehouseId"] = items.WarehouseId;
                                dr["Qty"] = 0;
                                dr["isdispatchedfreestock"] = false;
                                itemInventory.Rows.Add(dr);
                            }
                            var parmitemInventory = new SqlParameter
                            {
                                ParameterName = "ItemNetInventory",
                                SqlDbType = SqlDbType.Structured,
                                TypeName = "dbo.ItemNetInventory",
                                Value = itemInventory
                            };
                            var ItemNetInventoryDcs = db.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                            if (ItemNetInventoryDcs != null)
                            {
                                foreach (var items in item.ItemMasters)
                                {
                                    var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                    if (itemInventorys != null)
                                    {
                                        items.IsItemLimit = true;
                                        items.ItemlimitQty = itemInventorys.RemainingQty;
                                    }
                                    else
                                    {
                                        items.IsItemLimit = true;
                                        items.ItemlimitQty = 0;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

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
        private async Task<List<DataContracts.External.ItemDataDC>> ItemValidateConsumerApp(List<DataContracts.External.ItemDataDC> newdata, Customer ActiveCustomer, AuthContext db, string lang, bool IsSalesApp = false)
        {
            List<DataContracts.External.ItemDataDC> returnItems = new List<DataContracts.External.ItemDataDC>();
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
                var AppType = "Consumer";
                //if (IsSalesApp)
                //    AppType = "Sales App";
                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.OfferOn == "Item" && x.IsActive && !x.IsDeleted && (x.OfferAppType == AppType)).Select(x => x.OfferId).ToList() : new List<int>();
                List<DataContracts.External.ItemDataDC> freeItems = null;
                if (activeOfferids.Any())
                {
                    var freeItemIds = newdata.Where(x => x.OfferId.HasValue && x.OfferId > 0 && activeOfferids.Contains(x.OfferId.Value)).Select(x => x.OfferFreeItemId).ToList();
                    freeItems = db.itemMasters.Where(x => freeItemIds.Contains(x.ItemId)).Select(x => new DataContracts.External.ItemDataDC
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
                            it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / it.price) : 0;//MP;  we replce marginpoint value by margin for app here 

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
        [Route("RetailerGetAllItemByBrand")]
        [HttpGet]
        public async Task<DataContracts.External.ItemListDc> RetailerGetAllItemByBrand(string lang, int warehouseid, int SubsubCategoryid, int customerId)
        {
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.Where(x => x.CustomerId == customerId).Include(x => x.ConsumerAddress).FirstOrDefault(); //added by Anurag 04-05-2021
                var defaultadd = ActiveCustomer.ConsumerAddress.FirstOrDefault(x => x.Default);
                ActiveCustomer.Warehouseid = defaultadd.WarehouseId;
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;

                DataContracts.External.ItemListDc item = new DataContracts.External.ItemListDc();

                var newdata = new List<DataContracts.External.ItemDataDC>();
                if (ElasticSearchEnable)
                {
                    DataContracts.ElasticSearch.Suggest suggest = null;
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetAllItemByBrand");
                    var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", warehouseid.ToString())
                        .Replace("{#brand#}", SubsubCategoryid.ToString())
                        .Replace("{#from#}", "0")
                        .Replace("{#size#}", "1000");
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    newdata = Mapper.Map(elasticSearchItems).ToANew<List<DataContracts.External.ItemDataDC>>();
                }
                else
                {
                    newdata = (from a in context.itemMasters
                               where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && !a.IsDisContinued && a.SubsubCategoryid == SubsubCategoryid)
                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                               select new DataContracts.External.ItemDataDC
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
                                   MinOrderQty = 1,
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
                    newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                            Select(x => new DataContracts.External.ItemDataDC
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
                item.ItemMasters = new List<DataContracts.External.ItemDataDC>();
                var formatedData = await ItemValidateConsumerApp(newdata, ActiveCustomer, context, lang);
                item.ItemMasters.AddRange(formatedData);

                return new DataContracts.External.ItemListDc() { ItemMasters = item.ItemMasters, Status = true };

            }
        }
        [Route("SignupUpdateBasicInfo")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public CustUpdateDetails SignupUpdateBasicInfo(ConsumerAddressDC cust)
        {
            using (AuthContext context = new AuthContext())
            {
                var registeredApk = context.GetAPKUserAndPwd("RetailersApp");
                RetailerAppDC newcustomer = new RetailerAppDC();
                CustUpdateDetails res;
                string message = string.Empty;
                //Customer custdata = context.CustomerUpdateV3(cust);
                try
                {
                    int WarehouseId = 0;
                    Customer customer = context.Customers.Where(c => c.CustomerId == cust.CustomerId && c.Deleted == false).Include(x => x.ConsumerAddress).FirstOrDefault();

                    var city = context.Cities.FirstOrDefault(x => x.CityName.Trim().ToLower() == cust.CityName.Trim().ToLower());

                    DONPinCode CheckPin = null;
                    if (customer != null)
                    {

                        if (!customer.IsSignup)
                        {
                            customer.Name = cust.CustomerName;
                            customer.WhatsappNumber = cust.WhatsappNo;
                            customer.IsSignup = string.IsNullOrEmpty(cust.CustomerName) ? false : true;
                        }
                        //customer.IsCityVerified = city.Cityid > 0;

                        var custAddress = new ConsumerAddress();
                        if (!string.IsNullOrEmpty(cust.CompleteAddress))
                        {
                            if (customer.ShippingAddress == null)
                            {
                                customer.ShippingAddress = cust.CompleteAddress;
                                customer.ShippingCity = cust.CityName;
                                customer.lat = cust.Lat;
                                customer.lg = cust.lng;

                                customer.Addresslat = cust.Lat;
                                customer.Addresslg = cust.lng;
                            }

                            if (customer.ConsumerAddress == null)
                                customer.ConsumerAddress = new List<ConsumerAddress>();
                            else
                            {
                                customer.ConsumerAddress.ToList().ForEach(x => x.Default = false);
                            }


                            if (!customer.ConsumerAddress.Any(x => x.CompleteAddress == cust.CompleteAddress))
                            {
                                custAddress.CompleteAddress = cust.CompleteAddress;


                                if (!string.IsNullOrEmpty(cust.Address1))
                                {
                                    custAddress.Address1 = cust.Address1;
                                }
                                if (!string.IsNullOrEmpty(cust.Zipcode))
                                {
                                    custAddress.ZipCode = cust.Zipcode;
                                }

                                custAddress.Cityid = city?.Cityid ?? 0;
                                custAddress.CityName = city?.CityName ?? "";
                                custAddress.StateId = city?.Stateid ?? 0;
                                custAddress.StateName = city?.StateName ?? "";

                                if (string.IsNullOrEmpty(customer.Name))
                                {
                                    customer.Name = cust.ReciverName;
                                    customer.IsSignup = string.IsNullOrEmpty(customer.Name) ? false : true;
                                }
                                if (string.IsNullOrEmpty(customer.ShopName))
                                {
                                    customer.ShopName = cust.ReciverName;
                                }
                                custAddress.PersonName = cust.ReciverName;
                                //custAddress.MobileNumber = cust.Mobile;
                                custAddress.LandMark = cust.Landmark;
                                custAddress.CustomerId = cust.CustomerId;
                                custAddress.Country = "India";
                                custAddress.CreatedDate = DateTime.Now;
                                custAddress.CreatedBy = cust.CustomerId;
                                custAddress.IsActive = true;
                                custAddress.IsDeleted = false;
                                custAddress.lat = cust.Lat;
                                custAddress.lng = cust.lng;
                                custAddress.Default = true;
                                custAddress.WarehouseId = 0;
                                customer.ConsumerAddress.Add(custAddress);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(customer.ShopName))
                                {
                                    customer.ShopName = cust.ReciverName;
                                }
                                customer.ConsumerAddress.FirstOrDefault(x => x.CompleteAddress == cust.CompleteAddress).Default = true;
                                customer.ConsumerAddress.FirstOrDefault(x => x.CompleteAddress == cust.CompleteAddress).Address1 = cust.Address1;
                                if (!string.IsNullOrEmpty(cust.Landmark))
                                    customer.ConsumerAddress.FirstOrDefault(x => x.CompleteAddress == cust.CompleteAddress).LandMark = cust.Landmark;
                                customer.ConsumerAddress.FirstOrDefault(x => x.CompleteAddress == cust.CompleteAddress).PersonName = cust.ReciverName;
                            }
                        }
                        if (!string.IsNullOrEmpty(cust.Zipcode))
                        {
                            int pincode = !string.IsNullOrEmpty(cust.Zipcode) ? Convert.ToInt32(cust.Zipcode) : 0;
                            CheckPin = pincode > 0 ? context.DONPinCodeDB.Where(x => x.PinCode == pincode && x.IsActive).FirstOrDefault() : null;
                            if (CheckPin == null)
                            {
                                customer.ConsumerAddress.FirstOrDefault(x => x.Default).WarehouseId = 0;
                                //customer.InRegion = false;
                                message = "Thanks for registering with Zila. We will contact you soon";
                            }
                            else
                            {
                                var dd = context.Clusters.Where(x => x.ClusterId == CheckPin.DefaultClusterId).FirstOrDefault();
                                //customer.InRegion = true;
                                customer.ConsumerAddress.FirstOrDefault(x => x.Default).WarehouseId = dd.WarehouseId ?? 0;
                                if (!customer.Warehouseid.HasValue)
                                {
                                    customer.Cityid = dd.CityId;
                                    customer.City = dd.CityName;
                                    customer.State = Convert.ToString(customer.ConsumerAddress.FirstOrDefault(x => x.Default).StateId);
                                    customer.ClusterId = CheckPin.DefaultClusterId;
                                    customer.ClusterName = dd.ClusterName;
                                    customer.Warehouseid = dd.WarehouseId;
                                    customer.WarehouseName = dd.WarehouseName;
                                }
                                message = "Customer updated.";
                            }
                        }
                        if (string.IsNullOrEmpty(customer.BillingAddress))
                        {
                            customer.BillingAddress = cust.CompleteAddress;
                        }
                        customer.LandMark = cust.Landmark;
                        customer.CustomerAppType = 1;



                        if (!string.IsNullOrEmpty(cust.ReferralCode) && customer.Cityid.HasValue && customer.Cityid.Value > 0)
                        {
                            if (context.Customers.Any(x => x.Skcode.ToLower() == cust.ReferralCode.ToLower()))
                            {
                                if (customer.Skcode.ToLower() != cust.ReferralCode.ToLower())
                                {

                                    int ReferralTypeId = Convert.ToInt32(ReferralType.Consumer);
                                    var customerReferralConfiguration = context.CustomerReferralConfigurationDb.Where(x => x.CityId == customer.Cityid && x.IsActive == true && x.IsDeleted == false && x.ReferralType == ReferralTypeId).ToList();
                                    if (customerReferralConfiguration != null)
                                    {
                                        var referralWallet = context.ReferralWalletDb.FirstOrDefault(x => x.SkCode == customer.Skcode);
                                        var IsFirstSignup = context.ReferralWalletDb.Count(x => x.SkCode == customer.Skcode && x.ReferralSkCode == cust.ReferralCode);
                                        if (customerReferralConfiguration != null && referralWallet == null)
                                        {
                                            customer.ReferralSkCode = cust.ReferralCode;
                                            foreach (var item in customerReferralConfiguration)
                                            {
                                                context.ReferralWalletDb.Add(new Model.CustomerReferral.ReferralWallet
                                                {
                                                    SkCode = customer.Skcode,
                                                    ReferralSkCode = cust.ReferralCode,
                                                    CreatedBy = customer.CustomerId,
                                                    ReferralWalletPoint = item.ReferralWalletPoint,
                                                    CustomerWalletPoint = item.CustomerWalletPoint,
                                                    IsActive = true,
                                                    CreatedDate = DateTime.Now,
                                                    IsDeleted = false,
                                                    ReferralType = ReferralTypeId,
                                                    OnOrder = item.OnOrder,
                                                    IsSingupUsed = IsFirstSignup != 0,
                                                    CustomerReferralConfigurationId = item.Id,
                                                    IsUsed = 0
                                                });
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    res = new CustUpdateDetails()
                                    {
                                        customers = null,
                                        Status = false,
                                        Message = "You can't refer to your self.",
                                    };
                                    return res;
                                }
                            }
                            else
                            {
                                res = new CustUpdateDetails()
                                {
                                    customers = null,
                                    Status = false,
                                    Message = "Referral code not valid ",
                                };
                                return res;
                            }
                        }


                        context.Entry(customer).State = EntityState.Modified;
                        context.Commit();
                    }
                    newcustomer = Mapper.Map(customer).ToANew<RetailerAppDC>();
                    newcustomer.ShippingAddress = cust.CompleteAddress;
                    newcustomer.ShippingAddress1 = cust.Address1;
                    newcustomer.LandMark = cust.Landmark;
                    var defaultAddress = customer.ConsumerAddress.FirstOrDefault(x => x.Default);
                    newcustomer.Warehouseid = defaultAddress?.WarehouseId ?? 0;
                    if (newcustomer.Warehouseid.Value == 0)
                    {
                        newcustomer.ClusterId = null;
                        newcustomer.IsWarehouseLive = false;
                    }
                    else
                    {
                        newcustomer.ClusterId = CheckPin != null ? CheckPin.DefaultClusterId : 0;
                        newcustomer.Warehouseid = defaultAddress.WarehouseId;
                    }
                    newcustomer.ShippingAddress = defaultAddress.CompleteAddress;
                    newcustomer.City = defaultAddress.CityName;
                    newcustomer.Cityid = defaultAddress.Cityid;
                    newcustomer.State = defaultAddress.StateName;
                    newcustomer.ZipCode = defaultAddress.ZipCode;
                    newcustomer.lat = defaultAddress.lat;
                    newcustomer.lg = defaultAddress.lng;
                    newcustomer.LandMark = defaultAddress.LandMark;
                    newcustomer.Name = defaultAddress.PersonName;
                    newcustomer.ShopName = defaultAddress.PersonName;
                    newcustomer.APKType = registeredApk;
                    newcustomer.CustomerType = "consumer";
                    res = new CustUpdateDetails()
                    {
                        customers = newcustomer,
                        Status = true,
                        Message = message
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
        [Route("GetOfferVisibilityItem")]
        [HttpGet]
        public async Task<DataContracts.External.SalesItemResponseDc> GetOfferVisibilityItem(int customerId, int offerId, int SubCategoryId, int brandId, int step, int skip, int take, string lang, bool IsSalesApp = false)
        {
            Customer ActiveCustomer = new Customer();
            List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
            List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
            skip = skip / take;
            var itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.SalesAppItemDataDC>() };
            using (var context = new AuthContext())
            {

                ActiveCustomer = context.Customers.Where(x => x.CustomerId == customerId).Include(x => x.ConsumerAddress).FirstOrDefault(); //added by Anurag 04-05-2021
                var defaultadd = ActiveCustomer.ConsumerAddress.FirstOrDefault(x => x.Default);
                ActiveCustomer.Warehouseid = defaultadd.WarehouseId;
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;
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
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }

                ItemDataDCs = await ItemValidateConsumerApp(ItemData, ActiveCustomer, context, lang, IsSalesApp);

            }
            itemResponseDc.ItemDataDCs = ItemDataDCs.GroupBy(x => new { x.ItemNumber, x.ItemMultiMRPId, x.WarehouseId }).Select(x => new DataContracts.External.SalesAppItemDataDC
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
                MinOrderQty = 1,//x.FirstOrDefault().MinOrderQty,
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
                    MinOrderQty = 1,//y.MinOrderQty,
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
        [Route("RatailerFlashDealoffer")]
        [HttpGet]
        public async Task<DataContracts.External.ItemListDc> RatailerFlashDealoffer(int Warehouseid, int sectionid, int CustomerId, string lang = "en")
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);


                    DataContracts.External.ItemListDc res;
                    DateTime CurrentDate = indianTime.AddHours(-24);
                    var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Where(y => y.Active).Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                    if (data != null)
                    {
                        var customers = context.Customers.Where(x => x.CustomerId == CustomerId).Include(x => x.ConsumerAddress).FirstOrDefault(); //added by Anurag 04-05-2021
                        var defaultadd = customers.ConsumerAddress.FirstOrDefault(x => x.Default);
                        customers.Warehouseid = defaultadd.WarehouseId;
                        customers.CustomerType = "Consumer";
                        customers.Active = customers.IsB2CApp;
                        DataContracts.External.ItemListDc item = new DataContracts.External.ItemListDc();
                        var itemids = data.Select(x => x.ItemId);
                        var newdata = new List<DataContracts.External.ItemDataDC>();
                        if (ElasticSearchEnable)
                        {
                            DataContracts.ElasticSearch.Suggest suggest = null;
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
                            newdata = Mapper.Map(elasticSearchItems).ToANew<List<DataContracts.External.ItemDataDC>>();
                        }
                        else
                        {
                            newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId) && a.OfferType == "FlashDeal"
                                             && a.OfferEndTime >= indianTime && a.OfferQtyAvaiable > 0 && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                       select new DataContracts.External.ItemDataDC
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           ItemId = a.ItemId,
                                           ItemNumber = a.Number,
                                           itemname = a.itemname,
                                           HindiName = a.HindiName,
                                           LogoUrl = a.LogoUrl,
                                           MinOrderQty = 1,//a.MinOrderQty,
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

                        item.ItemMasters = new List<DataContracts.External.ItemDataDC>();
                        var olddata = newdata.Select(x => new { x.ItemId, x.FlashDealSpecialPrice }).ToList();
                        var formatedData = await ItemValidateConsumerApp(newdata, customers, context, lang);
                        foreach (var it in formatedData)
                        {
                            it.FlashDealSpecialPrice = olddata.FirstOrDefault(x => x.ItemId == it.ItemId).FlashDealSpecialPrice;
                        }

                        item.ItemMasters.AddRange(formatedData);

                        if (item.ItemMasters != null)
                        {
                            res = new DataContracts.External.ItemListDc
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
                            res = new DataContracts.External.ItemListDc
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
                        res = new DataContracts.External.ItemListDc
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
        [Route("Genotp")]
        [HttpGet]
        [AllowAnonymous]

        public OTP Getotp(string MobileNumber, string deviceId, string Apphash, string mode = "")
        {
            IEnumerable<string> username = new List<string>(); ;
            Request.Headers.TryGetValues("username", out username);


            bool TestUser = false;
            string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();//HttpContext.Current.Request.Headers.GetValues("customerType") !=null ? HttpContext.Current.Request.Headers.GetValues("customerType").FirstOrDefault() : "";//new OrderPlaceHelper().GetCustomerAppType();
            OTP b = new OTP();
            if (username != null && username.Any() && username.FirstOrDefault().Split('_')[0].Trim() == MobileNumber && ConsumerApptype.ToLower() == "consumer")
            {
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
                }
                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string sRandomOTP = ConsumerGenerateRandomOTP(4, saAllowedCharacters);
                // string OtpMessage = " is Your login Code. :). ShopKirana";
                string OtpMessage = ""; //"{#var1#} is Your login Code. {#var2#}. ShopKirana";
                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.ConsumerApp, "Login_Code");
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
                    MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
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


                b = new OTP()
                {
                    OtpNo = TestUser || (!string.IsNullOrEmpty(mode) && mode == "debug") ? sRandomOTP : "Successfully sent OTP",
                    Status = true,
                    Message = "Successfully sent OTP"

                };
            }
            else
            {
                b = new OTP()
                {
                    OtpNo = "You are not authorize",
                    Status = false,
                    Message = "You are not authorize"
                };
                return b;
            }
            return b;
        }
        private string ConsumerGenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
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

        [Route("GetConsumerETA")]
        [HttpGet]
        public DateTime GetConsumerETA(int WarehouseId)
        {
            DateTime ETAdate;
            ETAdate = new OrderPlaceHelper().GetConsumerETADate(WarehouseId);
            return ETAdate;
        }

        [Route("GetConsumerInvoice")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> GetConsumerInvoice(int orderid)
        {
            int id = orderid;
            AngularJSAuthentication.Model.Store.OrderInvoice invoice = new AngularJSAuthentication.Model.Store.OrderInvoice();
            string expiredHtml = string.Empty;
            string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/ConsumerTypeInvoice.html";
            string content = File.ReadAllText(pathToHTMLFile);
            int warehouseId, customerId = 0;
            using (var db = new AuthContext())
            {
                string filenamess = id + ".pdf";
                string ExcelSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/ConsumerTypeOrder"), filenamess);

                if (!File.Exists(ExcelSavePath))
                {
                    var ordermaster = db.DbOrderMaster.Where(orm => orm.OrderId == id).Include(x => x.orderDetails).FirstOrDefault();
                    var odm = db.OrderDispatchedMasters.Where(x => x.OrderId == id).Include(x => x.orderDetails).FirstOrDefault();
                    customerId = odm.CustomerId;
                    warehouseId = odm.WarehouseId;
                    var warehouseDetail = db.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);
                    var Query = "exec GetOrderpayment " + id;
                    var paymentdetail = db.Database.SqlQuery<PaymentResponseRetailerAppDc>(Query).ToList();

                    var cust = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                    var invoiceOrderOffer = new InvoiceOrderOffer();
                    List<InvoiceOrderOffer> invoiceOrderOffers = new List<InvoiceOrderOffer>();
                    var query = " select a.OrderId,b.OfferCode,b.ApplyOn,a.BillDiscountTypeValue,a.BillDiscountAmount from  BillDiscounts a  inner join Offers b on a.OfferId = b.OfferId  where a.orderid =  " + id + " and b.ApplyOn = 'PostOffer' Union all select orderid,'Flash Deal','',0,0 from FlashDealItemConsumeds a where a.orderid = " + id + " group by orderid";
                    invoiceOrderOffers = db.Database.SqlQuery<InvoiceOrderOffer>(query).ToList();
                    if (invoiceOrderOffers != null && invoiceOrderOffers.Any())
                    {
                        var offerCodes = invoiceOrderOffers.Select(x => x.OfferCode).ToList();
                        invoiceOrderOffer.OfferCode = string.Join(",", offerCodes);
                        double totalBillDicount = 0;
                        foreach (var item in invoiceOrderOffers)
                        {
                            if (item.BillDiscountAmount > 0)
                                totalBillDicount += item.BillDiscountAmount;
                            else
                                totalBillDicount += item.BillDiscountTypeValue;
                        }
                        invoiceOrderOffer.BillDiscountAmount = totalBillDicount > 0 ? totalBillDicount / 10 : 0;
                    }
                    // var CustomerCount = new OrderCountInfo();
                    var CustomerCount = (from i in db.Customers
                                         join k in db.DbOrderMaster on i.CustomerId equals k.CustomerId
                                         join com in db.Companies on i.CompanyId equals com.Id
                                         where k.OrderId == id && i.CustomerVerify == "Temporary Active"
                                         select new OrderCountInfo
                                         {
                                             OrderCount = (db.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.CreatedDate >= com.InActiveCustomerCountDate && x.CreatedDate <= k.CreatedDate).Count()),
                                             MaxOrderLimit = com.InActiveCustomerCount ?? 0,
                                         }).FirstOrDefault();
                    string KYCNote = "";

                    if (CustomerCount != null && CustomerCount.OrderCount > 0)
                    {
                        KYCNote = " Note:-Please complete your KYC.You are eligible for " + (CustomerCount.MaxOrderLimit - CustomerCount.OrderCount) + " more orders. Current Order Count = " + CustomerCount.OrderCount / CustomerCount.MaxOrderLimit;
                    }


                    var AddDatalists = odm.orderDetails.Where(z => z.OrderId == id).GroupBy(x => new { x.HSNCode }).Select(x => new getSuminvoiceHSNCodeDataDC
                    {
                        HSNCode = x.Key.HSNCode,
                        AmtWithoutTaxDisc = x.Sum(y => y.AmtWithoutTaxDisc),
                        SGSTTaxAmmount = x.Sum(y => y.SGSTTaxAmmount),
                        CGSTTaxAmmount = x.Sum(y => y.CGSTTaxAmmount),
                        TaxAmmount = x.Sum(y => y.TaxAmmount),
                        CessTaxAmount = x.Sum(y => y.CessTaxAmount),
                        TotalSum = x.Sum(y => y.AmtWithoutTaxDisc + y.SGSTTaxAmmount + y.CGSTTaxAmmount)
                    }).ToList();
                    string SumDataHSNDetailRows = "";
                    if (AddDatalists != null && AddDatalists.Count() > 0)
                    {
                        int rowNumber = 1;
                        foreach (var SumDataHSNDetail in AddDatalists)
                        {
                            SumDataHSNDetailRows += @"<tr>"
                                    //+ "<td>" + rowNumber.ToString() + "</td>"
                                    + "<td>" + SumDataHSNDetail.HSNCode + "</td>"
                                    + "<td>" + Math.Round(SumDataHSNDetail.AmtWithoutTaxDisc, 2) + "</td>"
                                    + "<td>" + (odm.IsIgstInvoice == false ? Math.Round(SumDataHSNDetail.CGSTTaxAmmount, 2) : 0) + "</td>"
                                    + "<td>" + (odm.IsIgstInvoice == false ? Math.Round(SumDataHSNDetail.SGSTTaxAmmount, 2) : 0) + "</td>"
                                    //+ "<td>" + (odm.IsIgstInvoice == true ? Math.Round((SumDataHSNDetail.TaxAmmount + SumDataHSNDetail.CessTaxAmount),2) : 0) + "</td>"
                                    //+ "<td>" + (SumDataHSNDetail.CessTaxAmount > 0 && odm.IsIgstInvoice == false ? Math.Round((SumDataHSNDetail.CessTaxAmount),2) : 0) + "</td>"
                                    + "<td>" + (Math.Round(SumDataHSNDetail.AmtWithoutTaxDisc, 2) + Math.Round(SumDataHSNDetail.SGSTTaxAmmount, 2) + Math.Round(SumDataHSNDetail.CGSTTaxAmmount, 2) + SumDataHSNDetail.CessTaxAmount) + "</td>"
                                + "</tr>";

                            rowNumber++;
                        }
                    }
                    else
                    {
                        //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
                    }
                    string result = "";

                    bool Ainfo = true, Binfo = true;
                    if (cust.lat == 0 || cust.lg == 0)
                    {
                        Ainfo = false;
                    }
                    if (string.IsNullOrEmpty(cust.RefNo) && (string.IsNullOrEmpty(cust.UploadGSTPicture) || string.IsNullOrEmpty(cust.UploadLicensePicture)))
                    {
                        Binfo = false;
                    }
                    if (!Ainfo || !Binfo)
                        result = "Your Critical info " + (!Ainfo ? "GPS" : "") + (!Ainfo && !Binfo ? " & " : "") + (!Binfo ? "Shop Licence/GST#" : "") + " is Missing. Your account can be blocked anytime.";


                    odm.IsPrimeCustomer = ordermaster.IsPrimeCustomer;
                    odm.IsFirstOrder = ordermaster.IsFirstOrder;

                    #region offerdiscounttype
                    if (odm.BillDiscountAmount > 0)
                    {
                        var billdiscountOfferId = db.BillDiscountDb.Where(x => x.OrderId == odm.OrderId && x.CustomerId == odm.CustomerId).Select(z => z.OfferId).ToList();
                        if (billdiscountOfferId.Count > 0)
                        {
                            List<string> offeron = db.OfferDb.Where(x => billdiscountOfferId.Contains(x.OfferId)).Select(x => new { x.OfferOn, x.OfferCode }).ToList().Select(x => (!string.IsNullOrEmpty(x.OfferCode) ? x.OfferCode : x.OfferOn)).ToList();
                            odm.offertype = string.Join(",", offeron);
                        }
                    }
                    #endregion
                    //for igst case if true then apply condion to hide column of cgst sgst cess
                    if (!string.IsNullOrEmpty(odm.Tin_No) && odm.Tin_No.Length >= 11)
                    {
                        string CustTin_No = odm.Tin_No.Substring(0, 2);

                        //if (!CustTin_No.StartsWith("0"))
                        //{
                        odm.IsIgstInvoice = !db.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == odm.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No);
                        //}

                    }
                    if (odm != null)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("IntValue");
                        var dr = dt.NewRow();
                        dr["IntValue"] = odm.CustomerId;
                        dt.Rows.Add(dr);
                        var param = new SqlParameter("CustomerId", dt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";

                        var GetStateCodeList = db.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByCustomerId @CustomerId", param).FirstOrDefault();

                        if (GetStateCodeList != null)
                        {
                            odm.shippingStateName = GetStateCodeList.shippingStateName != null ? GetStateCodeList.shippingStateName : " ";
                            odm.shippingStateCode = GetStateCodeList.shippingStateCode != null ? GetStateCodeList.shippingStateCode : " "; ;
                            odm.BillingStateName = GetStateCodeList.BillingStateName != null ? GetStateCodeList.BillingStateName : " "; ;
                            odm.BillingStateCode = GetStateCodeList.BillingStateCode != null ? GetStateCodeList.BillingStateCode : " "; ;
                        }
                    }


                    if (odm != null)
                    {
                        odm.WalletAmount = odm.WalletAmount > 0 ? odm.WalletAmount : 0;
                        odm.offertype = odm.offertype != null ? odm.offertype : "";
                        odm.EwayBillNumber = odm.EwayBillNumber != null ? odm.EwayBillNumber : "";
                        odm.IRNQRCodeUrl = odm.IRNQRCodeUrl != null ? odm.IRNQRCodeUrl : "";
                        odm.POCIRNQRCodeURL = odm.POCIRNQRCodeURL != null ? odm.POCIRNQRCodeURL : "";
                        odm.IRNNo = odm.IRNNo != null ? odm.IRNNo : "";
                        odm.POCIRNNo = odm.POCIRNNo != null ? odm.POCIRNNo : "";
                        odm.PocCreditNoteNumber = odm.PocCreditNoteNumber != null ? odm.PocCreditNoteNumber : "";
                        odm.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee((decimal)odm.GrossAmount);
                    }
                    if (invoiceOrderOffer.OfferCode == null)
                    {
                        invoiceOrderOffer.OfferCode = "";
                    }
                    if (CustomerCount != null)
                    {
                        expiredHtml = content.Replace("[CustomerCount.MaxOrderLimit]", CustomerCount.MaxOrderLimit.ToString()).Replace("[CustomerCount.OrderCount]", CustomerCount.OrderCount.ToString());
                    }
                    odm.ShopName = odm.ShopName != null ? odm.ShopName : "";
                    odm.Tin_No = odm.Tin_No != null ? odm.Tin_No : "";
                    odm.invoice_no = odm.invoice_no != null ? odm.invoice_no : "";
                    odm.SalesMobile = odm.SalesMobile != null ? odm.SalesMobile : "";
                    odm.SalesPerson = odm.SalesPerson != null ? odm.SalesPerson : "";
                    odm.DeliveryIssuanceIdOrderDeliveryMaster = odm.DeliveryIssuanceIdOrderDeliveryMaster != null ? odm.DeliveryIssuanceIdOrderDeliveryMaster : 0;
                    odm.paymentThrough = odm.paymentThrough != null ? odm.paymentThrough : " ";

                    expiredHtml = content
                        .Replace("[FromWarehouseDetail.CompanyName]", warehouseDetail.CompanyName.ToString())
                        .Replace("[FromWarehouseDetail.GSTin]", warehouseDetail.GSTin.ToString())
                        .Replace("[FromWarehouseDetail.FSSAILicenseNumber]", warehouseDetail.FSSAILicenseNumber.ToString())
                        .Replace("[FromWarehouseDetail.Address]", warehouseDetail.Address.ToString())
                        .Replace("[FromWarehouseDetail.StateName]", warehouseDetail.StateName.ToString())
                        .Replace("[FromWarehouseDetail.Phone]", warehouseDetail.Phone.ToString())
                        .Replace("[OrderData1.ShopName]", odm.ShopName.ToString())
                        .Replace("[OrderData1.BillingAddress]", odm.BillingAddress.ToString())
                        .Replace("[OrderData1.Tin_No]", odm.Tin_No.ToString())
                        .Replace("[OrderData1.CustomerName]", odm.CustomerName.ToString())
                        .Replace("[OrderData1.Skcode]", odm.Skcode.ToString())
                        .Replace("[OrderData1.Customerphonenum]", odm.Customerphonenum.ToString())
                        .Replace("[OrderData1.BillingStateName]", odm.BillingStateName.ToString())
                        .Replace("[OrderData1.BillingStateCode]", odm.BillingStateCode.ToString())
                        //.Replace("[OrderData1.IsPrimeCustomer]", odm.IsPrimeCustomer.ToString())
                        .Replace("[OrderData1.ShippingAddress]", odm.ShippingAddress.ToString())
                        .Replace("[OrderData1.shippingStateName]", odm.shippingStateName.ToString())
                        .Replace("[OrderData1.shippingStateCode]", odm.shippingStateCode.ToString())
                        .Replace("[OrderData1.invoice_no]", odm.invoice_no.ToString())
                        .Replace("[OrderData1.CreatedDate]", odm.CreatedDate.ToString())
                        .Replace("[OrderData1.OrderId]", odm.OrderId.ToString())
                        .Replace("[OrderData1.OrderedDate]", odm.OrderedDate.ToString())
                        //.Replace("[OrderData1.PocCreditNoteDate]", odm.PocCreditNoteDate.ToString())
                        .Replace("[OrderData1.IsIgstInvoice]", odm.IsIgstInvoice.ToString())
                        .Replace("[OrderData1.deliveryCharge]", odm.deliveryCharge.ToString())
                        .Replace("[OrderData1.paymentThrough]", odm.paymentThrough.ToString())
                        .Replace("[OrderData1.WalletAmount]", odm.WalletAmount.ToString())
                        .Replace("[OrderData1.PocCreditNoteNumber]", odm.PocCreditNoteNumber.ToString())
                        .Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[InvoiceAmountInWord]", odm.InvoiceAmountInWord.ToString())
                        .Replace("[OrderData1.BillDiscountAmount]", odm.BillDiscountAmount.ToString())
                        .Replace("[OrderData1.TCSAmount]", odm.TCSAmount.ToString())
                        .Replace("[OrderData1.GrossAmount]", odm.GrossAmount.ToString())
                        .Replace("[OrderData1.DiscountAmount]", odm.DiscountAmount.ToString())
                        .Replace("[OrderData1.Status]", odm.Status.ToString())
                        .Replace("[InvoiceOrderOffer.BillDiscountAmount]", invoiceOrderOffer.BillDiscountAmount.ToString())
                        .Replace("[InvoiceOrderOffer.OfferCode]", invoiceOrderOffer.OfferCode.ToString())
                        .Replace("[CustomerCriticalInfo]", result.ToString())
                        .Replace("[paymentdetail]", odm.Customerphonenum.ToString())
                        .Replace("[OrderData1.EwayBillNumber]", odm.EwayBillNumber.ToString())
                        .Replace("[OrderData1.offertype]", odm.offertype.ToString())
                        .Replace("[OrderData1.IRNQRCodeUrl]", odm.IRNQRCodeUrl.ToString())
                        .Replace("[OrderData1.POCIRNQRCodeURL]", odm.POCIRNQRCodeURL.ToString())
                                     //.Replace("[OrderData1.IRNNo]", odm.IRNNo.ToString())
                                     //.Replace("[OrderData1.POCIRNNo]", odm.POCIRNNo.ToString())
                                     ;




                    var ExecutiveIds = ordermaster.orderDetails.Where(z => z.ExecutiveId > 0).Select(z => z.ExecutiveId).Distinct().ToList();
                    if (ExecutiveIds != null && ExecutiveIds.Any())
                    {
                        var peoples = db.Peoples.Where(x => ExecutiveIds.Contains(x.PeopleID)).Select(x => new { x.DisplayName, x.Mobile }).ToList();
                        odm.SalesPerson = string.Join(",", peoples.Select(x => x.DisplayName));
                        odm.SalesMobile = string.Join(",", peoples.Select(x => x.Mobile));
                    }

                    odm.WalletAmount = odm.WalletAmount > 0 ? odm.WalletAmount : 0;
                    odm.offertype = odm.offertype != null ? odm.offertype : "";
                    odm.EwayBillNumber = odm.EwayBillNumber != null ? odm.EwayBillNumber : "";
                    odm.IRNQRCodeUrl = odm.IRNQRCodeUrl != null ? odm.IRNQRCodeUrl : "";
                    odm.POCIRNQRCodeURL = odm.POCIRNQRCodeURL != null ? odm.POCIRNQRCodeURL : "";
                    odm.IRNNo = odm.IRNNo != null ? odm.IRNNo : "";
                    odm.POCIRNNo = odm.POCIRNNo != null ? odm.POCIRNNo : "";
                    odm.PocCreditNoteNumber = odm.PocCreditNoteNumber != null ? odm.PocCreditNoteNumber : "";
                    var Amount = odm.GrossAmount - (odm.DiscountAmount > 0 ? odm.DiscountAmount : 0);
                    odm.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee((decimal)Amount);

                    if (invoiceOrderOffer.OfferCode == null)
                    {
                        invoiceOrderOffer.OfferCode = "";
                    }
                    double totalTaxableValue = 0;
                    double totalIGST = 0;
                    double totalCGST = 0;
                    double totalSGST = 0;
                    double totalCess = 0;
                    double TotalIOverall = 0;
                    double totalAmtIncTaxes = 0;
                    var OrderData = odm.orderDetails;
                    foreach (var i in OrderData)
                    {
                        totalTaxableValue = totalTaxableValue + i.AmtWithoutTaxDisc;
                        totalIGST = Math.Round(totalIGST + i.TaxAmmount + i.CessTaxAmount, 2);
                        totalCGST = Math.Round((totalCGST + i.CGSTTaxAmmount), 2);
                        totalSGST = Math.Round((totalSGST + i.SGSTTaxAmmount), 2);
                        totalCess = Math.Round((totalCess + i.CessTaxAmount), 2);
                        TotalIOverall = Math.Round(TotalIOverall + i.AmtWithoutTaxDisc + i.SGSTTaxAmmount + i.CGSTTaxAmmount + i.CessTaxAmount, 2);
                        totalAmtIncTaxes = Math.Round(totalAmtIncTaxes + i.TotalAmt, 2);
                    }
                    string OrderDataRows = "";
                    double TotalDetailQty = 0;

                    if (OrderData != null && OrderData.Count() > 0)
                    {
                        int rowNumber = 1;
                        foreach (var orderDetail in OrderData)
                        {
                            OrderDataRows += @"<tr>"
                                    + "<td>" + rowNumber.ToString() + "</td>"
                                    + "<td>" + orderDetail.itemname + (orderDetail.IsFreeItem ? "Free Item" : "") + "</td>"
                                    + "<td>" + orderDetail.price + "</td>"
                                    + "<td>" + ((orderDetail.UnitPrice == 0.0001 || orderDetail.UnitPrice == 0.01) ? (orderDetail.UnitPrice) : (orderDetail.UnitPrice)) + "</td>"
                                    + "<td>" + orderDetail.Noqty + "</td>"
                                    + "<td>" + Math.Round(orderDetail.AmtWithoutTaxDisc, 2) + "</td>"
                                    + "<td>" + orderDetail.HSNCode + "</td>"
                                      //+ "<td>" + orderDetail.DiscountAmmount + "</td>"
                                      // + "<td>" + orderDetail.HSNCode + "</td>"
                                      //+ "<td>" + (odm.IsIgstInvoice == true ? orderDetail.TaxPercentage + orderDetail.TotalCessPercentage : 0) + "</td>"
                                      //+ "<td>" + (odm.IsIgstInvoice == true ? Math.Round((orderDetail.TaxAmmount + orderDetail.CessTaxAmount),2) : 0) + "</td>"
                                      + "<td>" + (odm.IsIgstInvoice == false ? orderDetail.SGSTTaxPercentage : 0) + "</td>"
                                      + "<td>" + (odm.IsIgstInvoice == false ? Math.Round((orderDetail.SGSTTaxAmmount), 2) : 0) + "</td>"
                                      + "<td>" + (odm.IsIgstInvoice == false ? orderDetail.CGSTTaxPercentage : 0) + "</td>"
                                      + "<td>" + (odm.IsIgstInvoice == false ? Math.Round((orderDetail.CGSTTaxAmmount), 2) : 0) + "</td>"
                                      //+ "<td>" + (orderDetail.CessTaxAmount > 0 && odm.IsIgstInvoice == false ? orderDetail.TotalCessPercentage : 0) + "</td>"
                                      //+ "<td>" + (orderDetail.CessTaxAmount > 0 && odm.IsIgstInvoice == false ? orderDetail.CessTaxAmount : 0) + "</td>"
                                      + "<td>" + Math.Round(orderDetail.TotalAmt, 2) + "</td>"
                                + "</tr>";
                            TotalDetailQty = TotalDetailQty + orderDetail.Noqty;
                            rowNumber++;
                        }
                    }
                    else
                    {
                        //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
                    }
                    string ordertype = "";
                    if (odm.OrderType == 1 || odm.OrderType == 0)
                    {
                        ordertype = "General order";
                    }
                    else if (odm.OrderType == 2)
                    {
                        ordertype = "Bundle order";
                    }
                    else if (odm.OrderType == 3)
                    {
                        ordertype = "Return order";
                    }
                    else if (odm.OrderType == 4)
                    {
                        ordertype = "Distributer order";
                    }
                    else if (odm.OrderType == 6)
                    {
                        ordertype = "Damage order";
                    }
                    else
                    {
                        ordertype = "General order";
                    }
                    string AmtFrom = "";
                    foreach (var item in paymentdetail)
                    {
                        AmtFrom += item.PaymentFrom + "  " + item.amount + " ₹ /-   ";

                    }

                    expiredHtml = content.Replace("[FromWarehouseDetail.CompanyName]", warehouseDetail.CompanyName.ToString()).Replace("[FromWarehouseDetail.GSTin]", warehouseDetail.GSTin.ToString()).Replace("[FromWarehouseDetail.FSSAILicenseNumber]", warehouseDetail.FSSAILicenseNumber.ToString()).Replace("[FromWarehouseDetail.Address]", warehouseDetail.Address.ToString()).Replace("[FromWarehouseDetail.StateName]", warehouseDetail.StateName.ToString()).Replace("[FromWarehouseDetail.Phone]", warehouseDetail.Phone.ToString()).Replace("[OrderData1.ShopName]", odm.ShopName.ToString()).Replace("[OrderData1.BillingAddress]", odm.BillingAddress.ToString()).Replace("[OrderData1.Tin_No]", odm.Tin_No.ToString()).Replace("[OrderData1.CustomerName]", odm.CustomerName.ToString())
                        .Replace("[OrderData1.Skcode]", odm.Skcode.ToString()).Replace("[OrderData1.Customerphonenum]", odm.Customerphonenum.ToString()).Replace("[OrderData1.BillingStateName]", odm.BillingStateName.ToString()).Replace("[OrderData1.BillingStateCode]", odm.BillingStateCode.ToString()).Replace("[OrderData1.IsPrimeCustomer]", odm.IsPrimeCustomer.ToString()).Replace("[OrderData1.ShippingAddress]", odm.ShippingAddress.ToString()).Replace("[OrderData1.shippingStateName]", odm.shippingStateName.ToString()).Replace("[OrderData1.shippingStateCode]", odm.shippingStateCode.ToString()).Replace("[OrderData1.invoice_no]", odm.invoice_no.ToString()).Replace("[OrderData1.CreatedDate]", odm.CreatedDate.ToString()).Replace("[OrderData1.OrderId]", odm.OrderId.ToString()).Replace("[OrderData1.OrderedDate]", odm.OrderedDate.ToString()).Replace("[OrderData1.PocCreditNoteDate]", odm.PocCreditNoteDate.ToString())
                        .Replace("[OrderData1.IsIgstInvoice]", odm.IsIgstInvoice.ToString()).Replace("[OrderData1.deliveryCharge]", odm.deliveryCharge.ToString()).Replace("[OrderData1.paymentThrough]", odm.paymentThrough.ToString())
                        .Replace("[OrderData1.WalletAmount]", odm.WalletAmount.ToString()).Replace("[OrderData1.PocCreditNoteNumber]", odm.PocCreditNoteNumber.ToString()).Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[InvoiceAmountInWord]", odm.InvoiceAmountInWord.ToString())
                        .Replace("[OrderData1.BillDiscountAmount]", odm.BillDiscountAmount.ToString()).Replace("[OrderData1.TCSAmount]", odm.TCSAmount.ToString()).Replace("[OrderData1.GrossAmount]", odm.GrossAmount.ToString()).Replace("[OrderData1.DiscountAmount]", odm.DiscountAmount.ToString()).Replace("[OrderData1.Status]", odm.Status.ToString())
                        .Replace("[InvoiceOrderOffer.BillDiscountAmount]", invoiceOrderOffer.BillDiscountAmount.ToString())
                        .Replace("[InvoiceOrderOffer.OfferCode]", invoiceOrderOffer.OfferCode.ToString())
                        .Replace("[CustomerCriticalInfo]", result.ToString())
                    .Replace("[OrderData1.EwayBillNumber]", odm.EwayBillNumber.ToString()).Replace("[OrderData1.offertype]", odm.offertype.ToString())
                    .Replace("[OrderData1.IRNQRCodeUrl]", odm.IRNQRCodeUrl.ToString()).Replace("[OrderData1.POCIRNQRCodeURL]", odm.POCIRNQRCodeURL.ToString()).Replace("[OrderData1.IRNNo]", odm.IRNNo.ToString())
                    .Replace("[OrderData1.POCIRNNo]", odm.POCIRNNo.ToString())
                    .Replace("[InvoiceAmt]", (odm.GrossAmount - odm.DiscountAmount).ToString())
                    .Replace("[amount]", (paymentdetail[0].amount > 0 ? paymentdetail[0].amount : paymentdetail[0].amount).ToString())
                    .Replace("[PaymentFrom]", (paymentdetail[0].PaymentFrom != null ? paymentdetail[0].PaymentFrom : paymentdetail[0].PaymentFrom).ToString())
                    .Replace("[IsOnline]", (paymentdetail[0].amount > 0 && paymentdetail[0].IsOnline ? "Paid" : "Refund"))
                    .Replace("##SumDataHSNROWS##", SumDataHSNDetailRows)
                    .Replace("##OrderDataRows##", OrderDataRows)
                    //.Replace("##getOrderDataRows##", getOrderDataRows)
                    .Replace("[PaymentAmtFrom]", AmtFrom)
                    .Replace("[OrderData1.OrderType]", ordertype)
                    .Replace("[totalTaxableValue]", Math.Round(totalTaxableValue, 2).ToString())
                    .Replace("[totalIGST]", totalIGST.ToString())
                    .Replace("[totalCGST]", totalCGST.ToString())
                    .Replace("[totalSGST]", totalSGST.ToString())
                    .Replace("[totalCess]", totalCess.ToString())
                    .Replace("[TotalIOverall]", TotalIOverall.ToString())
                    .Replace("[KYCNote]", KYCNote.ToString())
                    //.Replace("[OrderLimit]", (CustomerCount != null ? (CustomerCount.MaxOrderLimit - CustomerCount.OrderCount).ToString() : ""))
                    // .Replace("[CurrentorderCount]", (CustomerCount != null ? (CustomerCount.OrderCount / CustomerCount.MaxOrderLimit).ToString() : ""))
                    //.Replace("[CustomerCount.MaxOrderLimit]", (CustomerCount != null ? CustomerCount.MaxOrderLimit.ToString() : ""))
                    //.Replace("[CustomerCount.OrderCount]", (CustomerCount != null ? CustomerCount.OrderCount.ToString() : ""))
                    .Replace("[TotalDetailQty]", TotalDetailQty.ToString());
                    if (!string.IsNullOrEmpty(expiredHtml))
                    {
                        string fileUrl = "";
                        string fullPhysicalPath = "";
                        string thFileName = "";
                        string TartgetfolderPath = "";

                        TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ConsumerTypeOrder");
                        if (!Directory.Exists(TartgetfolderPath))
                            Directory.CreateDirectory(TartgetfolderPath);


                        thFileName = id + ".pdf";
                        fileUrl = "/ConsumerTypeOrder" + "/" + thFileName;
                        fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;

                        var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/ConsumerTypeOrder"), thFileName);

                        byte[] pdf = null;

                        pdf = Pdf
                              .From(expiredHtml)
                              //.WithGlobalSetting("orientation", "Landscape")
                              //.WithObjectSetting("web.defaultEncoding", "utf-8")
                              .OfSize(OpenHtmlToPdf.PaperSize.A4)
                              .WithTitle("Invoice")
                              .WithoutOutline()
                              .WithMargins(PaperMargins.All(0.0.Millimeters()))
                              .Portrait()
                              .Comressed()
                              .Content();
                        FileStream file = File.Create(OutPutFile);
                        file.Write(pdf, 0, pdf.Length);
                        file.Close();
                        string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                             , HttpContext.Current.Request.Url.DnsSafeHost
                                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                        , "/ConsumerTypeOrder/" + id + ".pdf");
                        expiredHtml = FileUrl;
                    }
                }
                else
                {

                    string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                , "/ConsumerTypeOrder/" + id + ".pdf");


                    expiredHtml = FileUrl;
                }


            }

            return expiredHtml;
        }

        [Route("SendConsumerInvoice")]
        [HttpPost]
        public async Task<string> SendConsumerInvoice(int orderid, string Mobile)
        {
            string expiredHtml = string.Empty;
            using (var db = new AuthContext())
            {
                if (!string.IsNullOrEmpty(Mobile))
                {
                    string url = await GetConsumerInvoice(orderid);
                    bool isSent = false;
                    string Message = "";
                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "StoreOrderDelivered");
                    Message = dltSMS == null ? "" : dltSMS.Template;
                    Message = Message.Replace("{#var1#}", "User");
                    Message = Message.Replace("{#var2#}", orderid.ToString());
                    string FileUrl = url;
                    TextFileLogHelper.LogError("Error ConsumerTypeorder FileUrl" + FileUrl);

                    //var isGdApi = new IsGdAPI();
                    //string shortUrl = await isGdApi.ShortenUrlAsync(FileUrl);
                    //if (shortUrl == null)
                    //    shortUrl = "";
                    string shortUrl = Helpers.ShortenerUrl.ShortenUrl(FileUrl);
                    //string shortUrl = @is.gd.Url.GetShortenedUrl("https://uat.shopkirana.in/BO/139.pdf", v: true).Result;
                    TextFileLogHelper.LogError("Error ConsumerTypeorder shortUrl" + shortUrl);
                    Message = Message.Replace("{#var3#}", shortUrl);
                    TextFileLogHelper.LogError("Error ConsumerTypeorder SMS" + Message);
                    if (dltSMS != null)
                        isSent = Common.Helpers.SendSMSHelper.SendSMS(Mobile, Message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                    if (isSent)
                    {
                        expiredHtml = "Sent Succesfullly.";
                    }
                    else
                    {
                        expiredHtml = "Not Sent.";
                    }
                }
                else
                {
                    expiredHtml = "Please Enter Mobile Number";
                }
            }
            return expiredHtml;
        }

        //Freebies Api
        [Route("RetailerGetOfferItem")]
        [HttpGet]
        public async Task<AngularJSAuthentication.DataContracts.External.ItemListDc> RetailerGetOfferItem(int WarehouseId, int CustomerId, string lang = "en")
        {
            using (var context = new AuthContext())
            {
                DateTime CurrentDate = indianTime;
                var inActiveCustomer = false;

                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;
                ActiveCustomer.Warehouseid = WarehouseId;
                AngularJSAuthentication.DataContracts.External.ItemListDc res;
                AngularJSAuthentication.DataContracts.External.ItemListDc item = new AngularJSAuthentication.DataContracts.External.ItemListDc();
                item.ItemMasters = new List<AngularJSAuthentication.DataContracts.External.ItemDataDC>();
                List<AngularJSAuthentication.DataContracts.External.ItemDataDC> itemMasters = new List<AngularJSAuthentication.DataContracts.External.ItemDataDC>();

                if (ElasticSearchEnable)
                {
                    AngularJSAuthentication.DataContracts.ElasticSearch.Suggest suggest = null;
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
                    itemMasters = Mapper.Map(elasticSearchItems).ToANew<List<AngularJSAuthentication.DataContracts.External.ItemDataDC>>();
                }
                else
                {
                    itemMasters = (from a in context.itemMasters
                                   where (a.WarehouseId == WarehouseId && a.OfferStartTime <= CurrentDate
                                   && a.OfferEndTime >= indianTime && a.OfferCategory == 1 && a.active == true && a.Deleted == false && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                   join c in context.OfferDb on a.OfferId equals c.OfferId
                                   where (c.IsActive == true && c.IsDeleted == false && (c.OfferAppType == "Consumer"))
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new AngularJSAuthentication.DataContracts.External.ItemDataDC
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
                                       MinOrderQty = 1,
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
                    itemMasters = itemMasters.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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

                item.ItemMasters = new List<AngularJSAuthentication.DataContracts.External.ItemDataDC>();
                var formatedData = await ItemValidateConsumer(itemMasters, ActiveCustomer, context, lang);
                item.ItemMasters.AddRange(formatedData);

                #region ItemNetInventoryCheck
                if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                {
                    if (item.ItemMasters != null && item.ItemMasters.Any())
                    {
                        var itemInventory = new DataTable();
                        itemInventory.Columns.Add("ItemMultiMRPId");
                        itemInventory.Columns.Add("WarehouseId");
                        itemInventory.Columns.Add("Qty");
                        itemInventory.Columns.Add("isdispatchedfreestock");

                        foreach (var items in item.ItemMasters)
                        {
                            var dr = itemInventory.NewRow();
                            dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                            dr["WarehouseId"] = items.WarehouseId;
                            dr["Qty"] = 0;
                            dr["isdispatchedfreestock"] = false;
                            itemInventory.Rows.Add(dr);
                        }
                        var parmitemInventory = new SqlParameter
                        {
                            ParameterName = "ItemNetInventory",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.ItemNetInventory",
                            Value = itemInventory
                        };
                        var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        if (ItemNetInventoryDcs != null)
                        {
                            foreach (var items in item.ItemMasters)
                            {
                                var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                if (itemInventorys != null)
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = itemInventorys.RemainingQty;
                                }
                                else
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = 0;
                                }
                            }
                        }
                    }
                }
                #endregion

                if (item.ItemMasters.Count() != 0)
                {
                    res = new AngularJSAuthentication.DataContracts.External.ItemListDc
                    {
                        ItemMasters = item.ItemMasters,
                        Status = true,
                        Message = "Success."
                    };
                    return res;
                }
                else
                {
                    res = new AngularJSAuthentication.DataContracts.External.ItemListDc
                    {
                        ItemMasters = itemMasters,
                        Status = false,
                        Message = "Fail"
                    };
                    return res;
                }
            }
        }

        [Route("RetailerFavourite")]
        [HttpPost]
        public async Task<AngularJSAuthentication.DataContracts.External.ItemListDc> getFavItems(Cutomerfavourite FIt)
        {
            using (AuthContext context = new AuthContext())
            {
                if (string.IsNullOrEmpty(FIt.lang))
                    FIt.lang = "en";
                AngularJSAuthentication.DataContracts.External.ItemListDc res;
                var item = FIt.items;
                List<AngularJSAuthentication.DataContracts.External.ItemDataDC> itemlist = new List<AngularJSAuthentication.DataContracts.External.ItemDataDC>();
                var ActiveCustomer = context.Customers.Where(x => x.CustomerId == FIt.customerId).Include(x => x.ConsumerAddress).FirstOrDefault(); //added by Anurag 04-05-2021
                var defaultadd = ActiveCustomer.ConsumerAddress.FirstOrDefault(x => x.Default);
                ActiveCustomer.Warehouseid = defaultadd.WarehouseId;
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;
                List<int> ids = item.Select(x => x.ItemId).ToList();
                if (ids != null && ids.Any())
                {
                    var newdata = new List<AngularJSAuthentication.DataContracts.External.ItemDataDC>();
                    if (ElasticSearchEnable)
                    {
                        AngularJSAuthentication.DataContracts.ElasticSearch.Suggest suggest = null;
                        MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                        var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerFavourite");
                        var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                        var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                            .Replace("{#warehouseid#}", ActiveCustomer.Warehouseid.ToString())
                            .Replace("{#itemids#}", string.Join(",", ids))
                            .Replace("{#from#}", "0")
                            .Replace("{#size#}", "1000");
                        List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                        newdata = Mapper.Map(elasticSearchItems).ToANew<List<AngularJSAuthentication.DataContracts.External.ItemDataDC>>();

                    }
                    else
                    {
                        var excludecategory = context.Categorys.Where(x => x.CategoryName == "Face wash & Cream" || x.CategoryName == "Body cream & lotion" || x.CategoryName == "Baby & Fem Care").Select(x => x.Categoryid).ToList();
                        newdata = (from a in context.itemMasters
                                   where (a.Deleted == false && a.active == true && ids.Contains(a.ItemId)
                                    && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                   join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new AngularJSAuthentication.DataContracts.External.ItemDataDC
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
                                       MinOrderQty = 1,
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

                        newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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

                    itemlist = await ItemValidateConsumer(newdata, ActiveCustomer, context, FIt.lang);
                }

                if (itemlist != null)
                {
                    res = new AngularJSAuthentication.DataContracts.External.ItemListDc
                    {

                        ItemMasters = itemlist,
                        Status = true,
                        Message = "Success"
                    };
                    return res;
                }
                else
                {

                    res = new AngularJSAuthentication.DataContracts.External.ItemListDc
                    {
                        ItemMasters = null,
                        Status = false,
                        Message = "fail"
                    };
                    return res;
                }

            }
        }

        private async Task<List<AngularJSAuthentication.DataContracts.External.ItemDataDC>> ItemValidateConsumer(List<AngularJSAuthentication.DataContracts.External.ItemDataDC> newdata, Customer ActiveCustomer, AuthContext db, string lang, bool IsSalesApp = false, bool isDealItem = false)
        {
            List<AngularJSAuthentication.DataContracts.External.ItemDataDC> returnItems = new List<AngularJSAuthentication.DataContracts.External.ItemDataDC>();
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

                activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.OfferOn == "Item" && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Consumer")).Select(x => x.OfferId).ToList() : new List<int>();


                List<AngularJSAuthentication.DataContracts.External.ItemDataDC> freeItems = null;
                if (activeOfferids.Any())
                {
                    var freeItemIds = newdata.Where(x => x.OfferId.HasValue && x.OfferId > 0 && activeOfferids.Contains(x.OfferId.Value)).Select(x => x.OfferFreeItemId).ToList();
                    freeItems = db.itemMasters.Where(x => freeItemIds.Contains(x.ItemId)).Select(x => new AngularJSAuthentication.DataContracts.External.ItemDataDC
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
                    if (!isDealItem)
                    {
                        double cprice = backendOrderController.GetConsumerPrice(db, it.ItemMultiMRPId, it.price, it.UnitPrice, warehouseId);
                        it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice
                                                                       , it.WholeSalePrice ?? 0
                                                                       , it.TradePrice ?? 0, cprice);
                    }
                    else
                    {
                        it.UnitPrice = it.UnitPrice;
                    }
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
                            it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / it.price) : 0;//MP;  we replce marginpoint value by margin for app here 

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

                    it.MinOrderQty = 1;

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

        [Route("RazorPayCreateOrder")]
        [HttpGet]
        public async Task<string> RazorPayCreateOrder(long orderId, double Amount)
        {
            string razorPayOrderId = string.Empty;
            using (var context = new AuthContext())
            {
                //var orderAmount = context.DbOrderMaster.FirstOrDefault(x => x.OrderId == orderId).GrossAmount;
                if (orderId > 0 && Amount > 0)
                {
                    RazorPayTransactionHelper razorPayTransactionHelper = new RazorPayTransactionHelper();
                    int userId = GetLoginUserId();
                    razorPayOrderId = await razorPayTransactionHelper.CreateOrderAsync(orderId, Convert.ToInt32(Amount), userId, context);
                }
            }
            return razorPayOrderId;
        }


        [HttpGet]
        [Route("RetailerGetSearchItem")]

        public async Task<HttpResponseMessage> SearchCust(int warehouseId, int customerId, int skip, int take, string lang)
        {
            FilterSearchDc filterSearchDc = new FilterSearchDc();
            using (var authContext = new AuthContext())
            {
                var ActiveCustomer = authContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;

                ActiveCustomer.Warehouseid = warehouseId;
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

                        var newdata = Mapper.Map(MostSellingProduct).ToANew<List<AngularJSAuthentication.DataContracts.External.ItemDataDC>>();
                        newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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
                        var formatedData = AsyncContext.Run(() => ItemValidateConsumer(newdata, ActiveCustomer, authContext, lang));
                        filterSearchDc.MostSellingProduct = Mapper.Map(formatedData).ToANew<List<Itemsearch>>();

                        #region ItemNetInventoryCheck
                        if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                        {
                            if (filterSearchDc.MostSellingProduct != null && filterSearchDc.MostSellingProduct.Any())
                            {
                                var itemInventory = new DataTable();
                                itemInventory.Columns.Add("ItemMultiMRPId");
                                itemInventory.Columns.Add("WarehouseId");
                                itemInventory.Columns.Add("Qty");
                                itemInventory.Columns.Add("isdispatchedfreestock");

                                foreach (var items in filterSearchDc.MostSellingProduct)
                                {
                                    var dr = itemInventory.NewRow();
                                    dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                                    dr["WarehouseId"] = items.WarehouseId;
                                    dr["Qty"] = 0;
                                    dr["isdispatchedfreestock"] = false;
                                    itemInventory.Rows.Add(dr);
                                }
                                var parmitemInventory = new SqlParameter
                                {
                                    ParameterName = "ItemNetInventory",
                                    SqlDbType = SqlDbType.Structured,
                                    TypeName = "dbo.ItemNetInventory",
                                    Value = itemInventory
                                };
                                var ItemNetInventoryDcs = authContext.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();
                                if (ItemNetInventoryDcs != null)
                                {
                                    foreach (var items in filterSearchDc.MostSellingProduct)
                                    {
                                        var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                        if (itemInventorys != null)
                                        {
                                            items.IsItemLimit = true;
                                            items.ItemlimitQty = itemInventorys.RemainingQty;
                                        }
                                        else
                                        {
                                            items.IsItemLimit = true;
                                            items.ItemlimitQty = 0;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
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

                        var newdata = Mapper.Map(RecentPurchase).ToANew<List<AngularJSAuthentication.DataContracts.External.ItemDataDC>>();
                        newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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
                        var formatedData = AsyncContext.Run(() => ItemValidateConsumer(newdata, ActiveCustomer, authContext, lang));
                        filterSearchDc.RecentPurchase = Mapper.Map(formatedData).ToANew<List<Itemsearch>>();

                        #region ItemNetInventoryCheck
                        if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                        {
                            if (filterSearchDc.RecentPurchase != null && filterSearchDc.RecentPurchase.Any())
                            {
                                var itemInventory = new DataTable();
                                itemInventory.Columns.Add("ItemMultiMRPId");
                                itemInventory.Columns.Add("WarehouseId");
                                itemInventory.Columns.Add("Qty");
                                itemInventory.Columns.Add("isdispatchedfreestock");

                                foreach (var items in filterSearchDc.RecentPurchase)
                                {
                                    var dr = itemInventory.NewRow();
                                    dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                                    dr["WarehouseId"] = items.WarehouseId;
                                    dr["Qty"] = 0;
                                    dr["isdispatchedfreestock"] = false;
                                    itemInventory.Rows.Add(dr);
                                }
                                var parmitemInventory = new SqlParameter
                                {
                                    ParameterName = "ItemNetInventory",
                                    SqlDbType = SqlDbType.Structured,
                                    TypeName = "dbo.ItemNetInventory",
                                    Value = itemInventory
                                };
                                var ItemNetInventoryDcs = authContext.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();
                                if (ItemNetInventoryDcs != null)
                                {
                                    foreach (var items in filterSearchDc.RecentPurchase)
                                    {
                                        var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                        if (itemInventorys != null)
                                        {
                                            items.IsItemLimit = true;
                                            items.ItemlimitQty = itemInventorys.RemainingQty;
                                        }
                                        else
                                        {
                                            items.IsItemLimit = true;
                                            items.ItemlimitQty = 0;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

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
                                AngularJSAuthentication.DataContracts.ElasticSearch.Suggest suggest = null;
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

                            var newdata = Mapper.Map(CustFavoriteItem).ToANew<List<AngularJSAuthentication.DataContracts.External.ItemDataDC>>();
                            newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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
                            var formatedData = AsyncContext.Run(() => ItemValidateConsumer(newdata, ActiveCustomer, authContext, lang));
                            filterSearchDc.CustFavoriteItem = Mapper.Map(formatedData).ToANew<List<Itemsearch>>();

                            #region ItemNetInventoryCheck
                            if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                            {
                                if (filterSearchDc.CustFavoriteItem != null && filterSearchDc.CustFavoriteItem.Any())
                                {
                                    var itemInventory = new DataTable();
                                    itemInventory.Columns.Add("ItemMultiMRPId");
                                    itemInventory.Columns.Add("WarehouseId");
                                    itemInventory.Columns.Add("Qty");
                                    itemInventory.Columns.Add("isdispatchedfreestock");

                                    foreach (var items in filterSearchDc.CustFavoriteItem)
                                    {
                                        var dr = itemInventory.NewRow();
                                        dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                                        dr["WarehouseId"] = items.WarehouseId;
                                        dr["Qty"] = 0;
                                        dr["isdispatchedfreestock"] = false;
                                        itemInventory.Rows.Add(dr);
                                    }
                                    var parmitemInventory = new SqlParameter
                                    {
                                        ParameterName = "ItemNetInventory",
                                        SqlDbType = SqlDbType.Structured,
                                        TypeName = "dbo.ItemNetInventory",
                                        Value = itemInventory
                                    };
                                    var ItemNetInventoryDcs = authContext.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();
                                    if (ItemNetInventoryDcs != null)
                                    {
                                        foreach (var items in filterSearchDc.CustFavoriteItem)
                                        {
                                            var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                            if (itemInventorys != null)
                                            {
                                                items.IsItemLimit = true;
                                                items.ItemlimitQty = itemInventorys.RemainingQty;
                                            }
                                            else
                                            {
                                                items.IsItemLimit = true;
                                                items.ItemlimitQty = 0;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
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
                                AngularJSAuthentication.DataContracts.ElasticSearch.Suggest suggest = null;
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

                            var newdata = Mapper.Map(recentSearchItem).ToANew<List<AngularJSAuthentication.DataContracts.External.ItemDataDC>>();
                            newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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
                            var formatedData = AsyncContext.Run(() => ItemValidateConsumer(newdata, ActiveCustomer, authContext, lang));
                            filterSearchDc.RecentSearchItem = Mapper.Map(formatedData).ToANew<List<Itemsearch>>();

                            #region ItemNetInventoryCheck
                            if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                            {
                                if (filterSearchDc.RecentSearchItem != null && filterSearchDc.RecentSearchItem.Any())
                                {
                                    var itemInventory = new DataTable();
                                    itemInventory.Columns.Add("ItemMultiMRPId");
                                    itemInventory.Columns.Add("WarehouseId");
                                    itemInventory.Columns.Add("Qty");
                                    itemInventory.Columns.Add("isdispatchedfreestock");

                                    foreach (var items in filterSearchDc.RecentSearchItem)
                                    {
                                        var dr = itemInventory.NewRow();
                                        dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                                        dr["WarehouseId"] = items.WarehouseId;
                                        dr["Qty"] = 0;
                                        dr["isdispatchedfreestock"] = false;
                                        itemInventory.Rows.Add(dr);
                                    }
                                    var parmitemInventory = new SqlParameter
                                    {
                                        ParameterName = "ItemNetInventory",
                                        SqlDbType = SqlDbType.Structured,
                                        TypeName = "dbo.ItemNetInventory",
                                        Value = itemInventory
                                    };
                                    var ItemNetInventoryDcs = authContext.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();
                                    if (ItemNetInventoryDcs != null)
                                    {
                                        foreach (var items in filterSearchDc.RecentSearchItem)
                                        {
                                            var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                            if (itemInventorys != null)
                                            {
                                                items.IsItemLimit = true;
                                                items.ItemlimitQty = itemInventorys.RemainingQty;
                                            }
                                            else
                                            {
                                                items.IsItemLimit = true;
                                                items.ItemlimitQty = 0;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
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
        [Route("PostRazorPayTransaction")]
        [HttpPost]
        public async Task<IHttpActionResult> PostRazorPayTransaction(RazorPayTransactionDC razorPayTransactionDC)
        {
            using (var context = new AuthContext())
            {
                RazorPayTransactionHelper razorPayTransactionHelper = new RazorPayTransactionHelper();
                int createdBy = GetLoginUserId();
                bool res = await razorPayTransactionHelper.PostRazorPayTransactionAsync(razorPayTransactionDC, createdBy, context);
                return Ok(res);
            }
        }

        [Route("GetCompanyWheelConfig")]
        [HttpGet]
        public async Task<CompnayWheelConfigDc> GetCompanyWheelConfig(int WarehouseId)
        {
            CompnayWheelConfigDc compnayWheelConfigDc = new CompnayWheelConfigDc();
            using (var context = new AuthContext())
            {
                var wheelConfig = await context.CompanyWheelConfiguration.FirstOrDefaultAsync(x => x.IsStore == true);
                if (wheelConfig != null)
                {
                    compnayWheelConfigDc.OrderAmount = wheelConfig.OrderAmount;
                    compnayWheelConfigDc.LineItemCount = wheelConfig.LineItemCount;
                    compnayWheelConfigDc.IsKPPRequiredWheel = wheelConfig.IsKPPRequiredWheel;
                }
            }
            return compnayWheelConfigDc;
        }
        [Route("ConsumerWallet")]
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

                var con = context.CashConversionDb.FirstOrDefault(x => x.IsConsumer == true);
                Item.conversion = Mapper.Map(con).ToANew<CashConversionDc>();
                Item.Wallet = Mapper.Map(d).ToANew<WalletDc>();

                return Item;
            }


        }
        [Route("GetCompanyDetailsForConsumerAppWithToken")]
        [HttpGet]
        public async Task<consumercompanydetail> GetCompanyDetailsForConsumerAppWithToken(int customerId)
        {
            consumercompanydetail res;
            int WarehouseId = 0, cityId = 0;
            bool IsKpp = false;
            bool IsConsumer = false;
            bool IsSalesMan = false;
            bool IsActive = true;
            string FinBoxApiClientKey = ConfigurationManager.AppSettings["FinBoxClientKey"].ToString();
            long LogDboyLoctionMeter = Common.Constants.AppConstants.LogDboyLoctionMeter;

            using (AuthContext db = new AuthContext())
            {
                var companydetails = db.ConsumerCompanyDetailDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (companydetails != null)
                {
                    var query = "select  Operation.IsNewDeliveryAppOnCluster(" + customerId + ") ";

                    companydetails.ShowOrderTracking = db.Database.SqlQuery<bool>(query).FirstOrDefault();
                    var customer = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    var Addcustomer = db.ConsumerAddressDb.FirstOrDefault(x => x.CustomerId == customerId && x.Default);
                    customer.Active = customer.IsB2CApp;

                    IsActive = customer.Active;
                    IsKpp = customer.IsKPP;
                    IsConsumer = true;
                    WarehouseId = customer.Warehouseid.HasValue ? customer.Warehouseid.Value : 0;
                    if (Addcustomer != null)
                    {
                        WarehouseId = Addcustomer.WarehouseId;
                    }
                    else
                        WarehouseId = 0;

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
                        var extandedCompanyDetail = mongoDbHelper.Select(x => x.CityId == cityId && x.WarehouseId == WarehouseId && x.AppType == "Retailer").FirstOrDefault();

                        if (extandedCompanyDetail != null)
                        {
                            companydetails.IsShowCreditOption = extandedCompanyDetail.IsShowCreditOption;
                            companydetails.IsOnlinePayment = extandedCompanyDetail.IsOnlinePayment;
                            companydetails.ischeckBookShow = extandedCompanyDetail.ischeckBookShow;
                            companydetails.IsRazorpayEnable = extandedCompanyDetail.IsRazorpayEnable;
                            companydetails.IsePayLaterShow = extandedCompanyDetail.IsePayLaterShow;
                            companydetails.IsFinBox = extandedCompanyDetail.IsFinBox;
                            companydetails.IsCreditLineShow = extandedCompanyDetail.IsCreditLineShow;
                            companydetails.MinDealItemOrderAmt = extandedCompanyDetail.MinDealItemOrderAmt;
                            companydetails.MinDealItemPurchase = extandedCompanyDetail.MinDealItemPurchase;
                        }
                        else
                        {
                            companydetails.MinDealItemOrderAmt = 0;
                            companydetails.MinDealItemPurchase = 0;
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


                    res = new consumercompanydetail
                    {
                        CompanyDetails = companydetails,
                        Status = true,
                        Message = "GetCompanyDetailsForConsumer"
                    };
                    return res;
                }
                else
                {
                    res = new consumercompanydetail
                    {
                        Status = false,
                        Message = "Something went Wrong"
                    };
                    return res;
                }
            }
        }

        [Route("zilaimageupload")]
        [HttpPost]
        public bool zilaimageupload()
        {
            DataTable dt = null;
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    string ext = Path.GetExtension(httpPostedFile.FileName);
                    if (ext == ".xlsx" || ext == ".xls")
                    {
                        string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/ZilaItemImages");
                        string a1, b;

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                        b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/ZilaItemImages/"), a1);
                        httpPostedFile.SaveAs(b);

                        dt = Helpers.ExcelFileHelper.GetRequestsDataFromExcel(b);

                    }

                }
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                dt.Columns.Add("logoUrl");
                Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);
                Cloudinary cloudinary = new Cloudinary(account);
                string Logourl = "";

                string fname = "";
                string extension = "";
                string filename = "";
                List<string> itemnumbers = new List<string>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Logourl = dt.Rows[i]["URL"].ToString();
                    fname = Logourl.Substring(Logourl.LastIndexOf("/") + 1);
                    extension = fname.Substring(fname.LastIndexOf('.') + 1);
                    filename = dt.Rows[i]["Number"].ToString() + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(Logourl),
                        PublicId = "items_images/" + filename,
                        Overwrite = true,
                        Invalidate = true,
                        Backup = false
                    };

                    var uploadResult = cloudinary.Upload(uploadParams);

                    if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                    {
                        dt.Rows[i]["logoUrl"] = uploadResult.SecureUri.ToString();
                        itemnumbers.Add(dt.Rows[i]["Number"].ToString());
                    }
                }
                if (itemnumbers.Count > 0)
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var items = context.ItemMasterCentralDB.Where(x => itemnumbers.Contains(x.Number)).ToList();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(dt.Rows[i]["logoUrl"].ToString()))
                            {
                                var numberItems = items.Where(x => x.Number == dt.Rows[i]["Number"].ToString()).ToList();
                                foreach (var item in numberItems)
                                {
                                    item.LogoUrl = dt.Rows[i]["logoUrl"].ToString();
                                    context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                }
                            }
                        }
                        context.Commit();
                    }
                }




            }
            return true;
        }
        [Route("GetConsumerProfile")]
        [HttpGet]
        public async Task<CustDetails> GetConsumerProfile(int customerid, string deviceId)
        {
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

                    Customer customers = dbContext.Customers.Where(a => a.CustomerId == customerid).Include(x => x.ConsumerAddress).FirstOrDefault();
                    if (customers != null && customers.Deleted)
                    {
                        res = new CustDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "Your account is inactive please contact customer care."
                        };
                        return res;
                    }
                    customers.Active = customers.IsB2CApp;
                    newcustomer = Mapper.Map(customers).ToANew<RetailerAppDC>();
                    newcustomer.ReferralSkCode = customers.ReferralSkCode;
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

                        if (customers.ConsumerAddress != null && customers.ConsumerAddress.Any(x => x.Default))
                        {
                            var defaultadd = customers.ConsumerAddress.FirstOrDefault(x => x.Default);
                            if (defaultadd.WarehouseId > 0)
                            {
                                var cluster = dbContext.Clusters.FirstOrDefault(x => x.WarehouseId == defaultadd.WarehouseId);
                                newcustomer.Warehouseid = defaultadd.WarehouseId;
                                newcustomer.ClusterId = cluster.ClusterId;
                            }
                            else
                            {
                                newcustomer.Warehouseid = 0;
                                newcustomer.ClusterId = null;
                            }

                            newcustomer.ShippingAddress = defaultadd.CompleteAddress;
                            //newcustomer.ShippingAddress1 = defaultadd.Address1;
                            newcustomer.City = defaultadd.CityName;
                            newcustomer.State = defaultadd.StateName;
                            newcustomer.ZipCode = defaultadd.ZipCode;
                            newcustomer.lat = defaultadd.lat;
                            newcustomer.lg = defaultadd.lng;
                            //newcustomer.LandMark = defaultadd.LandMark;
                            newcustomer.Name = !string.IsNullOrEmpty(newcustomer.Name) ? newcustomer.Name : defaultadd.PersonName;
                            newcustomer.ShopName = !string.IsNullOrEmpty(newcustomer.ShopName) ? newcustomer.ShopName : defaultadd.PersonName;
                        }
                    }
                    if (newcustomer.Warehouseid > 0)
                    {
                        newcustomer.IsWarehouseLive = dbContext.GMWarehouseProgressDB.Any(x => x.WarehouseID == newcustomer.Warehouseid && x.IsLaunched);

                    }
                    if (!newcustomer.ClusterId.HasValue || newcustomer.ClusterId.Value == 0)
                    {
                        newcustomer.IsSelleravailable = true;
                    }
                    newcustomer.CustomerType = "consumer";
                    res = new CustDetails()
                    {
                        customers = newcustomer,
                        Status = true,
                        Message = "Success."
                    };
                    return res;
                }

                catch (Exception ex)
                {
                    return null;
                }
            }

        }
        [Route("GetConsumerTopMarginItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetConsumerTopMarginItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;

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
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                ItemData = ItemData.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                             Select(x => new DataContracts.External.ItemDataDC
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
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);

                #region ItemNetInventoryCheck
                if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                {
                    if (itemResponseDc.ItemDataDCs != null && itemResponseDc.ItemDataDCs.Any())
                    {
                        var itemInventory = new DataTable();
                        itemInventory.Columns.Add("ItemMultiMRPId");
                        itemInventory.Columns.Add("WarehouseId");
                        itemInventory.Columns.Add("Qty");
                        itemInventory.Columns.Add("isdispatchedfreestock");

                        foreach (var items in itemResponseDc.ItemDataDCs)
                        {
                            var dr = itemInventory.NewRow();
                            dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                            dr["WarehouseId"] = items.WarehouseId;
                            dr["Qty"] = 0;
                            dr["isdispatchedfreestock"] = false;
                            itemInventory.Rows.Add(dr);
                        }
                        var parmitemInventory = new SqlParameter
                        {
                            ParameterName = "ItemNetInventory",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.ItemNetInventory",
                            Value = itemInventory
                        };
                        var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        if (ItemNetInventoryDcs != null)
                        {
                            foreach (var items in itemResponseDc.ItemDataDCs)
                            {
                                var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                if (itemInventorys != null)
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = itemInventorys.RemainingQty;
                                }
                                else
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = 0;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            return itemResponseDc;
        }
        private async Task<List<DataContracts.External.ItemDataDC>> ItemValidate(List<DataContracts.External.ItemDataDC> newdata, Customer ActiveCustomer, AuthContext db, string lang, bool IsSalesApp = false)
        {
            List<DataContracts.External.ItemDataDC> returnItems = new List<DataContracts.External.ItemDataDC>();
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
                var AppType = "consumer";
                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = new List<int>();
                activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.OfferOn == "Item" && x.IsActive && !x.IsDeleted && (x.OfferAppType == AppType)).Select(x => x.OfferId).ToList() : new List<int>();


                List<DataContracts.External.ItemDataDC> freeItems = null;
                if (activeOfferids.Any())
                {
                    var freeItemIds = newdata.Where(x => x.OfferId.HasValue && x.OfferId > 0 && activeOfferids.Contains(x.OfferId.Value)).Select(x => x.OfferFreeItemId).ToList();
                    freeItems = db.itemMasters.Where(x => freeItemIds.Contains(x.ItemId)).Select(x => new DataContracts.External.ItemDataDC
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
                            it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / it.price) : 0;//MP;  we replce marginpoint value by margin for app here 

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

        [Route("ConsumerGetItemBySection")]
        [HttpGet]
        public async Task<DataContracts.External.ItemListDc> ConsumerGetItemBySection(int Warehouseid, int sectionid, int customerId, string lang = "en")
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var IsPrimeCustomer = context.PrimeCustomers.Any(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);

                    DataContracts.External.ItemListDc res;
                    DateTime CurrentDate = !IsPrimeCustomer ? indianTime.AddHours(-1 * MemberShipHours) : indianTime;
                    var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                    if (data != null)
                    {
                        List<int> ids = data.Select(x => x.SectionItemID).ToList();
                        var customers = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                        customers.CustomerType = "Consumer";
                        customers.Active = customers.IsB2CApp;

                        customers.Warehouseid = Warehouseid;
                        var inActiveCustomer = customers.Active == false || customers.Deleted == true;

                        DataContracts.External.ItemListDc item = new DataContracts.External.ItemListDc();
                        //foreach (var itemid in data)

                        var itemids = data.Select(x => x.ItemId).ToList();
                        if (itemids != null && itemids.Any())
                        {
                            var newdata = new List<DataContracts.External.ItemDataDC>();
                            {
                                newdata = (from a in context.itemMasters
                                               //where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId) && !excludecategory.Contains(a.Categoryid) && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                           where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId))
                                           //join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                           let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                           select new DataContracts.External.ItemDataDC
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
                                               MinOrderQty = 1,
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
                                newdata = newdata.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                             Select(x => new DataContracts.External.ItemDataDC
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
                            newdata = newdata.OrderByDescending(x => x.ItemNumber).ToList();
                            item.ItemMasters = new List<DataContracts.External.ItemDataDC>();
                            var formatedData = await ItemValidate(newdata, customers, context, lang);
                            item.ItemMasters.AddRange(formatedData);

                            #region ItemNetInventoryCheck
                            if (!string.IsNullOrEmpty(customers.CustomerType) && customers.CustomerType == "Consumer")
                            {
                                if (item.ItemMasters != null && item.ItemMasters.Any())
                                {
                                    var itemInventory = new DataTable();
                                    itemInventory.Columns.Add("ItemMultiMRPId");
                                    itemInventory.Columns.Add("WarehouseId");
                                    itemInventory.Columns.Add("Qty");
                                    itemInventory.Columns.Add("isdispatchedfreestock");

                                    foreach (var items in item.ItemMasters)
                                    {
                                        var dr = itemInventory.NewRow();
                                        dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                                        dr["WarehouseId"] = items.WarehouseId;
                                        dr["Qty"] = 0;
                                        dr["isdispatchedfreestock"] = false;
                                        itemInventory.Rows.Add(dr);
                                    }
                                    var parmitemInventory = new SqlParameter
                                    {
                                        ParameterName = "ItemNetInventory",
                                        SqlDbType = SqlDbType.Structured,
                                        TypeName = "dbo.ItemNetInventory",
                                        Value = itemInventory
                                    };
                                    var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                                    if (ItemNetInventoryDcs != null)
                                    {
                                        foreach (var items in item.ItemMasters)
                                        {
                                            var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                            if (itemInventorys != null)
                                            {
                                                items.IsItemLimit = true;
                                                items.ItemlimitQty = itemInventorys.RemainingQty;
                                            }
                                            else
                                            {
                                                items.IsItemLimit = true;
                                                items.ItemlimitQty = 0;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                        }

                        if (item.ItemMasters != null)
                        {
                            res = new DataContracts.External.ItemListDc
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
                            res = new DataContracts.External.ItemListDc
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
                        res = new DataContracts.External.ItemListDc
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


        [Route("GetConsumerAllStore")]
        [HttpGet]
        public async Task<List<RetailerStore>> GetConsumerAllStore(int customerId, int warehouseId, string lang)
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

        [Route("GetConsumerGoldenDealItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetConsumerGoldenDealItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;

                ActiveCustomer.Warehouseid = warehouseId;

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
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                ItemData = ItemData.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);

                #region ItemNetInventoryCheck
                if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                {
                    if (itemResponseDc.ItemDataDCs != null && itemResponseDc.ItemDataDCs.Any())
                    {
                        var itemInventory = new DataTable();
                        itemInventory.Columns.Add("ItemMultiMRPId");
                        itemInventory.Columns.Add("WarehouseId");
                        itemInventory.Columns.Add("Qty");
                        itemInventory.Columns.Add("isdispatchedfreestock");

                        foreach (var items in itemResponseDc.ItemDataDCs)
                        {
                            var dr = itemInventory.NewRow();
                            dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                            dr["WarehouseId"] = items.WarehouseId;
                            dr["Qty"] = 0;
                            dr["isdispatchedfreestock"] = false;
                            itemInventory.Rows.Add(dr);
                        }
                        var parmitemInventory = new SqlParameter
                        {
                            ParameterName = "ItemNetInventory",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.ItemNetInventory",
                            Value = itemInventory
                        };
                        var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        if (ItemNetInventoryDcs != null)
                        {
                            foreach (var items in itemResponseDc.ItemDataDCs)
                            {
                                var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                if (itemInventorys != null)
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = itemInventorys.RemainingQty;
                                }
                                else
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = 0;
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return itemResponseDc;
        }

        [Route("GetConsumerLastPurchaseItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetConsumerLastPurchaseItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;

                ActiveCustomer.Warehouseid = warehouseId;

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
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemcount"]);
                }
                ItemData = ItemData.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);

                #region ItemNetInventoryCheck
                if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                {
                    if (itemResponseDc.ItemDataDCs != null && itemResponseDc.ItemDataDCs.Any())
                    {
                        var itemInventory = new DataTable();
                        itemInventory.Columns.Add("ItemMultiMRPId");
                        itemInventory.Columns.Add("WarehouseId");
                        itemInventory.Columns.Add("Qty");
                        itemInventory.Columns.Add("isdispatchedfreestock");

                        foreach (var items in itemResponseDc.ItemDataDCs)
                        {
                            var dr = itemInventory.NewRow();
                            dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                            dr["WarehouseId"] = items.WarehouseId;
                            dr["Qty"] = 0;
                            dr["isdispatchedfreestock"] = false;
                            itemInventory.Rows.Add(dr);
                        }
                        var parmitemInventory = new SqlParameter
                        {
                            ParameterName = "ItemNetInventory",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.ItemNetInventory",
                            Value = itemInventory
                        };
                        var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        if (ItemNetInventoryDcs != null)
                        {
                            foreach (var items in itemResponseDc.ItemDataDCs)
                            {
                                var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                if (itemInventorys != null)
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = itemInventorys.RemainingQty;
                                }
                                else
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = 0;
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return itemResponseDc;
        }

        [Route("GetConsumerPrimeItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetConsumerPrimeItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {

                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;

                ActiveCustomer.Warehouseid = warehouseId;

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
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }
                ItemData = ItemData.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);

                #region ItemNetInventoryCheck
                if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                {
                    if (itemResponseDc.ItemDataDCs != null && itemResponseDc.ItemDataDCs.Any())
                    {
                        var itemInventory = new DataTable();
                        itemInventory.Columns.Add("ItemMultiMRPId");
                        itemInventory.Columns.Add("WarehouseId");
                        itemInventory.Columns.Add("Qty");
                        itemInventory.Columns.Add("isdispatchedfreestock");

                        foreach (var items in itemResponseDc.ItemDataDCs)
                        {
                            var dr = itemInventory.NewRow();
                            dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                            dr["WarehouseId"] = items.WarehouseId;
                            dr["Qty"] = 0;
                            dr["isdispatchedfreestock"] = false;
                            itemInventory.Rows.Add(dr);
                        }
                        var parmitemInventory = new SqlParameter
                        {
                            ParameterName = "ItemNetInventory",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.ItemNetInventory",
                            Value = itemInventory
                        };
                        var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        if (ItemNetInventoryDcs != null)
                        {
                            foreach (var items in itemResponseDc.ItemDataDCs)
                            {
                                var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                if (itemInventorys != null)
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = itemInventorys.RemainingQty;
                                }
                                else
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = 0;
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return itemResponseDc;
        }
        [Route("GetConsumerloged")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CustDetails> GetConsumerloged(string MobileNumber, bool IsOTPverified, string fcmid, string CurrentAPKversion, string PhoneOSversion, string UserDeviceName, string IMEI = "")
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
                            var defaultadd = db.ConsumerAddressDb.FirstOrDefault(x => x.Default && x.CustomerId == customersMobileExist.CustomerId);
                            if (defaultadd == null)
                            {
                                newcustomer.ShippingAddress = null;
                            }
                            else
                            {
                                newcustomer.ShippingAddress = defaultadd.CompleteAddress;
                                newcustomer.Warehouseid = defaultadd.WarehouseId;
                                newcustomer.City = defaultadd.CityName;
                                newcustomer.Cityid = defaultadd.Cityid;
                                newcustomer.State = defaultadd.StateName;
                                newcustomer.ZipCode = defaultadd.ZipCode;
                                newcustomer.lat = defaultadd.lat;
                                newcustomer.lg = defaultadd.lng;
                                newcustomer.LandMark = defaultadd.LandMark;
                                newcustomer.Name = defaultadd.PersonName;
                                newcustomer.ShopName = defaultadd.PersonName;
                            }
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

                                //var primeCustomer = db.PrimeCustomers.FirstOrDefault(x => x.CustomerId == newcustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                                //if (primeCustomer != null)
                                //{
                                //    newcustomer.IsPrimeCustomer = primeCustomer.StartDate <= indianTime && primeCustomer.EndDate >= indianTime;
                                //    newcustomer.PrimeStartDate = primeCustomer.StartDate;
                                //    newcustomer.PrimeEndDate = primeCustomer.EndDate;
                                //}
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
        [HttpGet]
        [Route("GetOrderDetail/{OrderId}")]
        public async Task<List<MyOrderDetailDc>> GetOrderDetail(int OrderId)
        {
            List<MyOrderDetailDc> orderDetail = new List<MyOrderDetailDc>();
            List<DialValuePoint> DialValues = new List<DialValuePoint>();
            using (var context = new AuthContext())
            {
                OrderProcessManager manager = new OrderProcessManager();
                orderDetail = manager.GetOrderDetails(OrderId, "Consumer");
                if (orderDetail != null && orderDetail.Any())
                {
                    var fromDate = DateTime.Now.AddHours(-24);
                    var toDate = DateTime.Now;
                    DialValues = context.DialValuePointDB.Where(dp => dp.OrderId.HasValue && dp.OrderId.Value == OrderId && !dp.IsPlayWeel
                                                          && dp.CreatedDate >= fromDate && dp.CreatedDate <= toDate).ToList();
                    orderDetail.ForEach(x =>
                    {
                        var orderPayment = x.OrderPaymentDCs;
                        x.OrderPaymentDCs = orderPayment.Where(z => z.PaymentFrom.ToLower() != "cash").Select(z => new OrderPaymentDCs
                        {
                            Amount = z.Amount,
                            PaymentFrom = z.PaymentFrom,
                            TransactionNumber = z.TransactionNumber,
                            TransactionDate = z.TransactionDate,
                            statusDesc = z.statusDesc
                        }).ToList();
                        x.IsPlayWeel = DialValues != null && DialValues.Any(y => y.OrderId == x.orderid && !string.IsNullOrEmpty(y.EarnWheelList));
                        x.WheelCount = DialValues != null && DialValues.Any(y => y.OrderId == x.orderid && !string.IsNullOrEmpty(y.EarnWheelList)) ? DialValues.FirstOrDefault(y => y.OrderId == x.orderid).EarnWheelCount : 0;
                        x.WheelList = DialValues != null && DialValues.Any(y => y.OrderId == x.orderid && !string.IsNullOrEmpty(y.EarnWheelList)) ? DialValues.FirstOrDefault(y => y.OrderId == x.orderid).EarnWheelList.Split(',').Select(y => Convert.ToInt32(y)).ToList() : new List<int>();
                        x.IsOrderHold = context.ReadyToPickHoldOrders.Where(Holdorder => Holdorder.OrderId == x.orderid && Holdorder.IsActive == true && Holdorder.IsDeleted == false).Count() > 0 ? true : false;

                        //x.IsETAEnable = x.IsOrderHold == true ? false : true;
                        if (x.PrioritizedDate == null)
                        {
                            x.IsETAEnable = x.IsOrderHold == true ? false : true;
                        }
                        else
                        {
                            x.IsETAEnable = false;
                        }
                    });
                }
                return orderDetail;
            }
        }

        [HttpGet]
        [Route("CheckConsumerAddress")]
        public async Task<bool> CheckConsumerAddress(int CustomerId)
        {
            using (var context = new AuthContext())
            {
                var data = context.ConsumerAddressDb.Where(x => x.CustomerId == CustomerId && x.IsActive == true && x.IsDeleted == false && x.Default).FirstOrDefault();
                if (data != null && (string.IsNullOrEmpty(data.Address1) || string.IsNullOrEmpty(data.PersonName)))
                    return false;
                return true;
            }
        }

        [Route("RetailerGetItembycatesscatidnew")]
        [HttpGet]
        public async Task<ItemResponseDc> RetailerGetItembycatesscatidnew(string lang, int customerId, int catid, int scatid, int sscatid, string sortType, string direction, int skip = 0, int take = 10)
        {
            RetailerAppManager manager = new RetailerAppManager();
            return await manager.RetailerGetItembycatesscatidnew(lang, customerId, catid, scatid, sscatid, skip, take, sortType, direction, "Consumer");
        }

        [Route("GenerateQRCodeBySkCode")]
        [HttpGet]
        public async Task<APIResponse> GenerateQRCodeBySkCode(string SkCode)
        {
            APIResponse result = new APIResponse { Status = false, Message = "QR code not generated." };

            if (!string.IsNullOrEmpty(SkCode))
            {
                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/ReferralQRCode")))
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/ReferralQRCode"));

                string fileName = SkCode + ".jpeg";
                string LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/ReferralQRCode"), fileName);
                string returnUrl = string.Format("{0}://{1}{2}/{3}/{4}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , "ReferralQRCode", fileName);

                if (!File.Exists(LogoUrl))
                {
                    string UPIUrl = ConfigurationManager.AppSettings["ReferralQRCodeUrl"];
                    if (!string.IsNullOrEmpty(UPIUrl))
                    {
                        UPIUrl = UPIUrl.Replace("[SKCODE]", SkCode);
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(UPIUrl, QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);
                        qrCodeImage.Save(LogoUrl, System.Drawing.Imaging.ImageFormat.Jpeg);
                        result.Status = true;
                        result.Message = "Generated Successfully";
                        result.Data = returnUrl;
                    }
                }
                else
                {
                    result.Status = true;
                    result.Message = "Generated Successfully";
                    result.Data = returnUrl;
                }
            }
            else
                result.Message = "SkCode Required.";

            return result;
        }

        [Route("GenerateQRCodeByUrl")]
        [HttpGet]
        public APIResponse GenerateQRCodeByUrl(string url)
        {
            APIResponse result = new APIResponse { Status = false, Message = "QR code not generated." };
            if (!string.IsNullOrEmpty(url))
            {
                if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/QRCode")))
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/QRCode"));

                string fileName = url + ".jpeg";
                string LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/QRCode"), fileName);
                string returnUrl = string.Format("{0}://{1}{2}/{3}/{4}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                        , HttpContext.Current.Request.Url.DnsSafeHost
                                                                        , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                        , "ReferralQRCode", fileName);
                if (!File.Exists(LogoUrl))
                {
                    string UPIUrl = url;
                    if (!string.IsNullOrEmpty(UPIUrl))
                    {
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(UPIUrl, QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);
                        qrCodeImage.Save(LogoUrl, System.Drawing.Imaging.ImageFormat.Jpeg);
                        result.Status = true;
                        result.Message = "Generated Successfully";
                        result.Data = returnUrl;
                    }
                }
                else
                {
                    result.Status = true;
                    result.Message = "Generated Successfully";
                    result.Data = returnUrl;
                }
            }
            else
                result.Message = "Url Required.";
            return result;
        }

        [Route("GetConsumerReferralConfigurations")]
        [HttpGet]
        public AppReferralConfig GetConsumerCustReferralConfigurations(int CityId)
        {
            AppReferralConfig result = new AppReferralConfig();
            List<DataContracts.CustomerReferralDc.GetCustReferralConfigDc> custReferralConfigList = new List<DataContracts.CustomerReferralDc.GetCustReferralConfigDc>();
            using (var db = new AuthContext())
            {
                result.SingupWallet = db.ConsumerCompanyDetailDB.FirstOrDefault().ReferralWalletPoint;
                custReferralConfigList = db.CustomerReferralConfigurationDb.Where(x => x.CityId == CityId && x.ReferralType == 3 && x.IsActive == true && x.IsDeleted == false)
                     .Select(x => new DataContracts.CustomerReferralDc.GetCustReferralConfigDc
                     {
                         OnOrder = x.OnOrder,
                         ReferralWalletPoint = x.ReferralWalletPoint,
                         CustomerWalletPoint = x.CustomerWalletPoint,
                         OnDeliverd = x.OnDeliverd
                     }).ToList().OrderBy(x => x.OnOrder).ToList();
                var statusids = custReferralConfigList.Select(x => x.OnDeliverd).Distinct().ToList();
                var customerReferralStatus = db.CustomerReferralStatusDb.Where(x => statusids.Contains((int)x.Id) && x.IsActive == true && x.IsDeleted == false).ToList();
                custReferralConfigList.ForEach(x =>
                {
                    x.OrderCount = x.OnOrder + " Order";
                    x.orderStatus = customerReferralStatus != null ? customerReferralStatus.FirstOrDefault(y => y.Id == x.OnDeliverd).OrderStatus : "NA";
                });
                result.CustReferralConfigDcs = custReferralConfigList;
                return result;
            }
        }

        [Route("GetConsumerReferralOrderList")]
        [HttpGet]
        public AppReferralHomeData GetConsumerReferralOrderList(int customerId)
        {
            AppReferralHomeData result = new AppReferralHomeData();
            using (var context = new AuthContext())
            {
                var customerIds = new SqlParameter("@CustomerId", customerId);
                List<GetCustomerReferralOrderListDc> CustomerReferralList = context.Database.SqlQuery<GetCustomerReferralOrderListDc>("exec GetCustomerReferralOrderList @CustomerId", customerIds).ToList();
                var customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                var custHis = context.CustomerWalletHistoryDb.Where(x => x.CustomerId == customerId && x.comment == "Referral Signup Customer Wallet Point Added" && x.NewAddedWAmount > 0).ToList();
                if (custHis != null && custHis.Any())
                {
                    var skcodes = custHis.Select(x => x.Through.Replace("Referral Signup points earned from", "")).ToList();
                    var refercusts = context.Customers.Where(x => skcodes.Contains(x.Skcode)).Select(x => new { x.Skcode, x.Name });
                    var walletHis = custHis
                       .Select(x => new GetCustomerReferralOrderListDc
                       {
                           IsUsed = 1,
                           CustomerWalletPoint = 0,
                           OnOrder = 1,
                           NewSignup = refercusts.Any(y => y.Skcode == x.Through.Replace("Referral Signup points earned from", "")) ? false : true,
                           ReferralSkCode = customer.Skcode,
                           OrderId = 0,
                           Status = "",
                           ReferralWalletPoint = x.NewAddedWAmount.HasValue ? x.NewAddedWAmount.Value : 0,
                           ShopName = refercusts.Any(y => y.Skcode == x.Through.Replace("Referral Signup points earned from", "")) ?
                           refercusts.FirstOrDefault(y => y.Skcode == x.Through.Replace("Referral Signup points earned from", "")).Name
                           : x.Through.Replace("Referral Signup points earned from", ""),
                           SkCode = x.Through.Replace("Referral Signup points earned from", ""),
                           CreatedDate = x.CreatedDate
                       }).ToList();

                    CustomerReferralList.AddRange(walletHis);
                    result.TotalReferral = skcodes.Distinct().Count();
                }
                result.CustomerReferrallst = CustomerReferralList.OrderByDescending(x => x.CreatedDate).ToList();
                result.TotalOrder = CustomerReferralList.Count(x => x.OrderId > 0);
                result.RewardsEarned = Convert.ToInt16(CustomerReferralList.Where(x => x.IsUsed == 1).Sum(x => x.ReferralWalletPoint));

            }
            return result;
        }

        [Route("CheckReferralSkCode")]
        [HttpGet]
        public APIResponse CheckReferralSkCode(string ReferralSkode, int CustomerId)
        {
            APIResponse res = new APIResponse();
            using (var context = new AuthContext())
            {
                var cust = context.Customers.Where(x => x.CustomerId == CustomerId).Include(x => x.ConsumerAddress).FirstOrDefault();
                if (!string.IsNullOrEmpty(ReferralSkode) && cust != null)
                {
                    if (cust.Skcode.ToLower() != ReferralSkode.ToLower())
                    {
                        if (string.IsNullOrEmpty(cust.ReferralSkCode))
                        {
                            if (cust.ConsumerAddress.Any(x => x.Default) && cust.ConsumerAddress.FirstOrDefault(x => x.Default).Cityid > 0)
                            {
                                int CityId = cust.ConsumerAddress.FirstOrDefault(x => x.Default).Cityid;
                                if (context.Customers.Any(x => x.Skcode.ToLower() == ReferralSkode.ToLower()))
                                {
                                    int ReferralTypeId = Convert.ToInt32(ReferralType.Consumer);
                                    var customerReferralConfiguration = context.CustomerReferralConfigurationDb.Where(x => x.CityId == CityId && x.IsActive == true && x.IsDeleted == false && x.ReferralType == ReferralTypeId).ToList();
                                    if (customerReferralConfiguration != null)
                                    {
                                        var referralWallet = context.ReferralWalletDb.FirstOrDefault(x => x.SkCode.ToLower() == cust.Skcode.ToLower());
                                        var IsFirstSignup = context.ReferralWalletDb.Count(x => x.SkCode.ToLower() == cust.Skcode.ToLower() && x.ReferralSkCode.ToLower() == ReferralSkode.ToLower());
                                        if (customerReferralConfiguration != null && referralWallet == null)
                                        {
                                            cust.ReferralSkCode = ReferralSkode;
                                            foreach (var item in customerReferralConfiguration)
                                            {
                                                context.ReferralWalletDb.Add(new Model.CustomerReferral.ReferralWallet
                                                {
                                                    SkCode = cust.Skcode,
                                                    ReferralSkCode = ReferralSkode,
                                                    CreatedBy = cust.CustomerId,
                                                    ReferralWalletPoint = item.ReferralWalletPoint,
                                                    CustomerWalletPoint = item.CustomerWalletPoint,
                                                    IsActive = true,
                                                    CreatedDate = DateTime.Now,
                                                    IsDeleted = false,
                                                    ReferralType = ReferralTypeId,
                                                    OnOrder = item.OnOrder,
                                                    IsSingupUsed = IsFirstSignup != 0,
                                                    CustomerReferralConfigurationId = item.Id,
                                                    IsUsed = 0
                                                });
                                            }
                                        }
                                        context.Entry(cust).State = EntityState.Modified;
                                        if (context.Commit() > 0)
                                        {
                                            res.Status = true;
                                            res.Message = "Referral code applied successfully.";
                                        }
                                        else
                                        {
                                            res.Status = false;
                                            res.Message = "Something went wrong.";
                                        }
                                    }
                                    else
                                    {
                                        res.Status = false;
                                        res.Message = "Configuration not exist.";
                                    }
                                }
                                else
                                {
                                    res.Status = false;
                                    res.Message = "Referral customer not found.";
                                }
                            }
                            else
                            {
                                res.Status = false;
                                res.Message = "Referral works on same city.";
                            }
                        }
                        else
                        {
                            res.Status = false;
                            res.Message = "Already Referred.";
                        }
                    }
                    else
                    {
                        res.Status = false;
                        res.Message = "You can't refer to your self.";
                    }
                }
                else
                {
                    res.Status = false;
                    res.Message = "Something went wrong.";
                }
                return res;
            }
        }


        [Route("GetCartDealItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetCartDealItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {

                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                //var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;
                ActiveCustomer.CustomerType = "Consumer";

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetConsumerCartDealItem]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }

                itemResponseDc.ItemDataDCs = await ItemValidateConsumer(ItemData, ActiveCustomer, context, lang, false, true);

                #region ItemNetInventoryCheck
                /*
               if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
               {
                   if (itemResponseDc.ItemDataDCs != null && itemResponseDc.ItemDataDCs.Any())
                   {
                       var itemInventory = new DataTable();
                       itemInventory.Columns.Add("ItemMultiMRPId");
                       itemInventory.Columns.Add("WarehouseId");
                       itemInventory.Columns.Add("Qty");
                       itemInventory.Columns.Add("isdispatchedfreestock");

                       foreach (var items in itemResponseDc.ItemDataDCs)
                       {
                           var dr = itemInventory.NewRow();
                           dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                           dr["WarehouseId"] = items.WarehouseId;
                           dr["Qty"] = 0;
                           dr["isdispatchedfreestock"] = false;
                           itemInventory.Rows.Add(dr);
                       }
                       var parmitemInventory = new SqlParameter
                       {
                           ParameterName = "ItemNetInventory",
                           SqlDbType = SqlDbType.Structured,
                           TypeName = "dbo.ItemNetInventory",
                           Value = itemInventory
                       };
                       var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                       if (ItemNetInventoryDcs != null)
                       {
                           foreach (var items in itemResponseDc.ItemDataDCs)
                           {
                               var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                               if (itemInventorys != null)
                               {
                                   items.IsItemLimit = true;
                                   items.ItemlimitQty = itemInventorys.RemainingQty;
                               }
                               else
                               {
                                   items.IsItemLimit = true;
                                   items.ItemlimitQty = 0;
                               }
                           }
                       }
                   }
               }
                */
                #endregion

            }

            return itemResponseDc;
        }


        [Route("GetConsumerItemWithCondition")]
        [HttpGet]
        public async Task<ItemResponseDc> GetConsumerItemWithCondition(int warehouseId, int customerId, int catid, int scatid, int sscatid, int minPrice, int maxPrice, int minDiscount, int maxDiscount, string lang, int skip = 0, int take = 10)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                ActiveCustomer.CustomerType = "Consumer";
                ActiveCustomer.Active = ActiveCustomer.IsB2CApp;

                ActiveCustomer.Warehouseid = warehouseId;

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetConsumerItemWithCondition]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@sscatid", sscatid));
                cmd.Parameters.Add(new SqlParameter("@scatid", scatid));
                cmd.Parameters.Add(new SqlParameter("@catid", catid));
                cmd.Parameters.Add(new SqlParameter("@minPrice", minPrice));
                cmd.Parameters.Add(new SqlParameter("@maxPrice", maxPrice));
                cmd.Parameters.Add(new SqlParameter("@minDiscount", minDiscount));
                cmd.Parameters.Add(new SqlParameter("@maxDiscount", maxDiscount));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemcount"]);
                }
                ItemData = ItemData.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                           Select(x => new DataContracts.External.ItemDataDC
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
                itemResponseDc.ItemDataDCs = await ItemValidate(ItemData, ActiveCustomer, context, lang);

                #region ItemNetInventoryCheck
                if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                {
                    if (itemResponseDc.ItemDataDCs != null && itemResponseDc.ItemDataDCs.Any())
                    {
                        var itemInventory = new DataTable();
                        itemInventory.Columns.Add("ItemMultiMRPId");
                        itemInventory.Columns.Add("WarehouseId");
                        itemInventory.Columns.Add("Qty");
                        itemInventory.Columns.Add("isdispatchedfreestock");

                        foreach (var items in itemResponseDc.ItemDataDCs)
                        {
                            var dr = itemInventory.NewRow();
                            dr["ItemMultiMRPId"] = items.ItemMultiMRPId;
                            dr["WarehouseId"] = items.WarehouseId;
                            dr["Qty"] = 0;
                            dr["isdispatchedfreestock"] = false;
                            itemInventory.Rows.Add(dr);
                        }
                        var parmitemInventory = new SqlParameter
                        {
                            ParameterName = "ItemNetInventory",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.ItemNetInventory",
                            Value = itemInventory
                        };
                        var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        if (ItemNetInventoryDcs != null)
                        {
                            foreach (var items in itemResponseDc.ItemDataDCs)
                            {
                                var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == items.ItemMultiMRPId);
                                if (itemInventorys != null)
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = itemInventorys.RemainingQty;
                                }
                                else
                                {
                                    items.IsItemLimit = true;
                                    items.ItemlimitQty = 0;
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return itemResponseDc;
        }

        [Route("GetOrderIdByInvoice")]
        [HttpGet]
        public async Task<APIResponse> GetOrderIdByInvoice(string InvoiceNo)
        {
            APIResponse res;
            long OrderId = 0;
            using (var context = new AuthContext())
            {
                if (!string.IsNullOrEmpty(InvoiceNo))
                {
                    OrderId = context.DbOrderMaster.Where(x => x.invoice_no.Trim().ToLower() == InvoiceNo.Trim().ToLower()).Select(x => x.OrderId).FirstOrDefault();
                    if (OrderId == 0)
                    {
                        return res = new APIResponse
                        {
                            Data = 0,
                            Message = "Enter correct barcode!!",
                            Status = false
                        };
                    }
                }
                else
                {
                    return res = new APIResponse
                    {
                        Data = 0,
                        Message = "Enter correct barcode!!",
                        Status = false
                    };
                }
            }
            return res = new APIResponse
            {
                Data = OrderId,
                Message = "Succesfully Added!!",
                Status = true
            };
        }
    }
}