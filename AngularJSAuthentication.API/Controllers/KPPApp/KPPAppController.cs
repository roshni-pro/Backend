using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.KPPApp;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.KPPApp;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CustomerShoppingCart;
using AngularJSAuthentication.Model.Gullak;
using Hangfire;
using iTextSharp.text.pdf;
using LinqKit;
using Nito.AspNetBackgroundTasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Drawing;
using LinqKit;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using iTextSharp.text;
using AngularJSAuthentication.API.Controllers.Base;
using OpenHtmlToPdf;
using System.Security.Claims;
using System.Configuration;
using AngularJSAuthentication.DataContracts.Mongo;
using System.Data.Common;
using AngularJSAuthentication.DataContracts.External;
using Newtonsoft.Json;
using AngularJSAuthentication.BusinessLayer.FinBox;
using System.Data.Entity.Core.Objects;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Enums;
using Newtonsoft.Json.Linq;
using NLog;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.DataContracts.RazorPay;
using AngularJSAuthentication.API.Helper.Razorpay;

namespace AngularJSAuthentication.API.Controllers.KPPApp
{
    [RoutePrefix("api/DistributorApp")]
    public class KPPAppController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private static Logger logger = LogManager.GetCurrentClassLogger();
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static List<int> OrderToProcess = new List<int>();
        #region Signup-Login Process

        [Route("GetLogedCust")]
        [AcceptVerbs("POST")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<DataContracts.KPPApp.customerDetails> GetLogedCustomer(loggedcust cust)
        {
            DataContracts.KPPApp.customerDetails res;
            using (AuthContext db = new AuthContext())
            {
                if (cust.IsOTPverified == true)
                {
                    Customer customersMobileExist = db.Customers.Where(c => c.Deleted == false && c.Mobile.Trim().Equals(cust.MobileNumber.Trim())).FirstOrDefault();
                    if (customersMobileExist != null)
                    {
                        //if (!customersMobileExist.IsKPP)
                        //{
                        //    res = new DataContracts.KPPApp.customerDetails()
                        //    {
                        //        customers = null,
                        //        Status = false,
                        //        Message = "You are not a distributor. Please contact Customer care for more details",
                        //        IsVerified = false
                        //    };
                        //    return res;
                        //}


                        if (cust.fcmid != null && cust.fcmid.Trim() != "" && cust.fcmid.Trim().ToUpper() != "NULL")
                        {
                            customersMobileExist.fcmId = cust.fcmid;
                            customersMobileExist.CurrentAPKversion = cust.CurrentAPKversion;
                            customersMobileExist.PhoneOSversion = cust.PhoneOSversion;
                            customersMobileExist.UserDeviceName = cust.UserDeviceName;
                            customersMobileExist.IsResetPasswordOnLogin = false;
                            customersMobileExist.imei = !string.IsNullOrEmpty(cust.IMEI) ? cust.IMEI : customersMobileExist.imei;
                            db.Entry(customersMobileExist).State = EntityState.Modified;
                            db.Commit();
                        }
                        var registeredApk = db.GetAPKUserAndPwd("DistributorApp");
                        customersMobileExist.RegisteredApk = registeredApk;

                        var dis = db.DistributorVerificationDB.Where(x => x.CustomerID == customersMobileExist.CustomerId).Select(x => new { Status = x.Status, IsVerified = x.IsVerified, FirmName = x.FirmName, TypeOfBusiness = x.TypeOfBusiness }).FirstOrDefault();
                        if (dis != null)
                        {
                            customers custkpp = Mapper.Map(customersMobileExist).ToANew<customers>();
                            custkpp.FirmName = dis.FirmName;
                            custkpp.TypeOfBuissness = dis.TypeOfBusiness;
                            res = new DataContracts.KPPApp.customerDetails()
                            {
                                customers = custkpp,
                                Status = true,
                                Message = dis.Status,
                                IsVerified = true//dis.IsVerified
                            };
                            return res;
                        }
                        else
                        {
                            var customerGullak = db.GullakDB.Where(x => x.CustomerId == customersMobileExist.CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (customerGullak == null)
                            {
                                customerGullak = new Gullak
                                {
                                    CustomerId = customersMobileExist.CustomerId,
                                    TotalAmount = 0,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedBy = Convert.ToInt32(customersMobileExist.CustomerId),
                                    CreatedDate = DateTime.Now,
                                };
                                db.GullakDB.Add(customerGullak);
                                db.Commit();
                            }

                            res = new DataContracts.KPPApp.customerDetails()
                            {
                                customers = Mapper.Map(customersMobileExist).ToANew<customers>(),
                                Status = true,
                                Message = "Signup Complete",
                                IsVerified = true //customersMobileExist.Active
                            };
                            return res;
                        }

                    }
                    else
                    {
                        res = new DataContracts.KPPApp.customerDetails()
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
                    res = new DataContracts.KPPApp.customerDetails()
                    {
                        customers = null,
                        Status = false,
                        Message = "OTP not verified."
                    };
                    return res;
                }
            }
        }

        [Route("GetLogedCustForDistributor")]
        [AcceptVerbs("POST")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<DataContracts.KPPApp.customerDetails> GetLogedCustForDistributor(loggedcust cust)
        {
            DataContracts.KPPApp.customerDetails res;
            using (AuthContext db = new AuthContext())
            {
                if (cust.IsOTPverified == true)
                {
                    Customer customersMobileExist = db.Customers.Where(c => c.Deleted == false && c.Mobile.Trim().Equals(cust.MobileNumber.Trim())).FirstOrDefault();
                    if (customersMobileExist != null)
                    {
                        if (!customersMobileExist.IsKPP)
                        {
                            res = new DataContracts.KPPApp.customerDetails()
                            {
                                customers = null,
                                Status = false,
                                Message = "You are not a distributor. Please contact Customer care for more details",
                                IsVerified = false
                            };
                            return res;
                        }


                        if (cust.fcmid != null && cust.fcmid.Trim() != "" && cust.fcmid.Trim().ToUpper() != "NULL")
                        {
                            customersMobileExist.fcmId = cust.fcmid;
                            customersMobileExist.CurrentAPKversion = cust.CurrentAPKversion;
                            customersMobileExist.PhoneOSversion = cust.PhoneOSversion;
                            customersMobileExist.UserDeviceName = cust.UserDeviceName;
                            customersMobileExist.IsResetPasswordOnLogin = false;
                            customersMobileExist.imei = !string.IsNullOrEmpty(cust.IMEI) ? cust.IMEI : customersMobileExist.imei;
                            db.Entry(customersMobileExist).State = EntityState.Modified;
                            db.Commit();
                        }
                        var registeredApk = db.GetAPKUserAndPwd("DistributorApp");
                        customersMobileExist.RegisteredApk = registeredApk;

                        var dis = db.DistributorVerificationDB.Where(x => x.CustomerID == customersMobileExist.CustomerId).Select(x => new { Status = x.Status, IsVerified = x.IsVerified, FirmName = x.FirmName, TypeOfBusiness = x.TypeOfBusiness }).FirstOrDefault();
                        if (dis != null)
                        {
                            customers custkpp = Mapper.Map(customersMobileExist).ToANew<customers>();
                            custkpp.FirmName = dis.FirmName;
                            custkpp.TypeOfBuissness = dis.TypeOfBusiness;
                            res = new DataContracts.KPPApp.customerDetails()
                            {
                                customers = custkpp,
                                Status = true,
                                Message = dis.Status,
                                IsVerified = true//dis.IsVerified
                            };
                            return res;
                        }
                        else
                        {
                            var customerGullak = db.GullakDB.Where(x => x.CustomerId == customersMobileExist.CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (customerGullak == null)
                            {
                                customerGullak = new Gullak
                                {
                                    CustomerId = customersMobileExist.CustomerId,
                                    TotalAmount = 0,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedBy = Convert.ToInt32(customersMobileExist.CustomerId),
                                    CreatedDate = DateTime.Now,
                                };
                                db.GullakDB.Add(customerGullak);
                                db.Commit();
                            }

                            res = new DataContracts.KPPApp.customerDetails()
                            {
                                customers = Mapper.Map(customersMobileExist).ToANew<customers>(),
                                Status = true,
                                Message = "Signup Complete",
                                IsVerified = true //customersMobileExist.Active
                            };
                            return res;
                        }

                    }
                    else
                    {
                        res = new DataContracts.KPPApp.customerDetails()
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
                    res = new DataContracts.KPPApp.customerDetails()
                    {
                        customers = null,
                        Status = false,
                        Message = "OTP not verified."
                    };
                    return res;
                }
            }
        }

        [ResponseType(typeof(Distributor))]
        [Route("InsertKpp")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public async Task<custdata> addCustomer(string MobileNumber)
        {
            using (AuthContext db = new AuthContext())
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
                    c.IsKPP = true;
                    db.Customers.Add(c);
                    db.Commit();

                    var registeredApk = db.GetAPKUserAndPwd("DistributorApp");

                    custdata cs = new custdata
                    {
                        CustomerId = c.CustomerId,
                        CompanyId = c.CompanyId,
                        Mobile = c.Mobile,
                        Skcode = c.Skcode,
                        Warehouseid = c.Warehouseid,
                        RegisteredApk = Mapper.Map(registeredApk).ToANew<AngularJSAuthentication.DataContracts.KPPApp.ApkNamePwdResponse>()
                    };
                    return cs;
                }
                else
                {
                    int? WarehouseId = 0;
                    try
                    {
                        Customer cw = db.Customers.Where(x => x.CustomerId == customers.CustomerId).FirstOrDefault();
                        if (cw != null)
                        {
                            WarehouseId = cw.Warehouseid;
                        }
                    }
                    catch (Exception Esy)
                    {
                    }

                    var registeredApk = db.GetAPKUserAndPwd("DistributorApp");

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
        }


        [HttpGet]
        [Route("GetCompanyDetails")]
        [AllowAnonymous]
        public HttpResponseMessage GetCompanyDetails(int? WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    using (var con = new AuthContext())
                    {
                        CompanyDetails companydetails = con.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                        MongoDbHelper<ExtandedCompanyDetail> mongoDbHelper = new MongoDbHelper<ExtandedCompanyDetail>();
                        var extandedCompanyDetail = mongoDbHelper.Select(x => x.WarehouseId == WarehouseId && x.AppType == "DON").FirstOrDefault();
                        int CODCharges = 0;
                        int CancelationPercent = 0;
                        bool StopCOD = false;
                        if (extandedCompanyDetail != null)
                        {
                            CODCharges = extandedCompanyDetail.DONCODChanges ?? 0;
                            StopCOD = extandedCompanyDetail.StopCOD ?? true;
                            companydetails.IsShowCreditOption = extandedCompanyDetail.IsShowCreditOption;
                            companydetails.IsOnlinePayment = extandedCompanyDetail.IsOnlinePayment;
                            companydetails.ischeckBookShow = extandedCompanyDetail.ischeckBookShow;
                            companydetails.IsRazorpayEnable = extandedCompanyDetail.IsRazorpayEnable;
                            companydetails.IsePayLaterShow = extandedCompanyDetail.IsePayLaterShow;
                            companydetails.IsFinBox = extandedCompanyDetail.IsFinBox;
                            companydetails.IsCreditLineShow = extandedCompanyDetail.IsCreditLineShow;
                            CancelationPercent = extandedCompanyDetail.DeliveryCancelationPer ?? 0;
                        }
                        var response = new
                        {
                            companydetails = companydetails,
                            CODCharges = CODCharges,
                            StopCOD = StopCOD,
                            CancelationPercent = CancelationPercent,
                            Status = true,
                            Message = "companydetails",

                        };
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }
                }
                catch (Exception ex)
                {
                    var response = new
                    {
                        Status = false,
                        Message = ex
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }

        }

        //[Route("Genotp")]
        //[AllowAnonymous]
        //public OTP Getotp(string MobileNumber, string deviceId, string OTPKey)
        //{
        //    OTP b = new OTP();
        //    string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        //    string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
        //    //string OtpMessage = " is Your Shopkirana Verification Code. :)";
        //    //string message  = HttpUtility.UrlEncode(string.Format("<#> {0} : is Your Test Shopkirana Verification Code.", sRandomOTP, OTPKey));
        //    string message = string.Format("<#> {0} : is Your Shopkirana Verification Code for complete process." + OTPKey, sRandomOTP);
        //    //string message = sRandomOTP + " :" + OtpMessage;
        //    Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString());
        //    OTP a = new OTP()
        //    {
        //        OtpNo = sRandomOTP
        //    };
        //    return a;
        //}

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

        [ResponseType(typeof(CustomerRegistration))]
        [Route("loginKPP")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public async Task<DataContracts.KPPApp.customerDetails> Postc(Login customer)
        {
            using (AuthContext context = new AuthContext())
            {
                DataContracts.KPPApp.customerDetails res;
                Distributor newcustomer = new Distributor();

                Customer custExist = context.Customers.Where(x => x.Deleted == false && x.Mobile == customer.Mobile).SingleOrDefault();
                if (custExist != null)
                {
                    if (!custExist.IsKPP)
                    {
                        res = new DataContracts.KPPApp.customerDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "You are not a distributor. Please contact Customer care for more details",
                            IsVerified = false
                        };
                        return res;
                    }

                    if (custExist.Password != customer.Password)
                        custExist = null;

                    if (custExist == null)
                    {
                        res = new DataContracts.KPPApp.customerDetails()
                        {
                            customers = Mapper.Map(custExist).ToANew<customers>(),
                            Status = false,
                            Message = "Incorrect password. Try again"
                        };
                        return res;
                    }
                    else
                    {
                        if (customer.fcmId != null && customer.fcmId.Trim() != "" && customer.fcmId.Trim().ToUpper() != "NULL")
                        {
                            custExist.CurrentAPKversion = customer.CurrentAPKversion;
                            custExist.PhoneOSversion = customer.PhoneOSversion;
                            custExist.UserDeviceName = customer.UserDeviceName;
                            custExist.fcmId = customer.fcmId;
                            custExist.imei = customer.imei;
                            context.Entry(custExist).State = EntityState.Modified;
                            context.Commit();
                            #region Device History
                            var Customerhistory = custExist;

                            PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                            if (Customerhistory != null)
                            {
                                phonerecord.CustomerId = Customerhistory.CustomerId;
                                phonerecord.Name = Customerhistory.Name;
                                phonerecord.Skcode = Customerhistory.Skcode;
                                phonerecord.Mobile = Customerhistory.Mobile;
                                phonerecord.IMEI = Customerhistory.imei;
                                phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                phonerecord.UpdatedDate = DateTime.Now;
                                context.PhoneRecordHistoryDB.Add(phonerecord);
                                int id = context.Commit();
                            }

                            #endregion

                        }
                        var registeredApk = context.GetAPKUserAndPwd("DistributorApp");
                        custExist.RegisteredApk = registeredApk;

                        var dis = context.DistributorVerificationDB.Where(x => x.CustomerID == custExist.CustomerId).Select(x => new { Status = x.Status, IsVerified = x.IsVerified, FirmName = x.FirmName, TypeOfBusiness = x.TypeOfBusiness }).FirstOrDefault();

                        if (dis != null)
                        {
                            customers custkpp = Mapper.Map(custExist).ToANew<customers>();
                            custkpp.FirmName = dis.FirmName;
                            custkpp.TypeOfBuissness = dis.TypeOfBusiness;
                            res = new DataContracts.KPPApp.customerDetails()
                            {
                                customers = custkpp,
                                Status = true,
                                Message = dis.Status,
                                IsVerified = dis.IsVerified
                            };
                            return res;
                        }
                        else
                        {
                            res = new DataContracts.KPPApp.customerDetails()
                            {
                                customers = Mapper.Map(custExist).ToANew<customers>(),
                                Status = true,
                                Message = "Signup Complete",
                                IsVerified = false
                            };
                            return res;
                        }
                    }
                }
                else
                {
                    res = new DataContracts.KPPApp.customerDetails()
                    {
                        customers = null,
                        Status = false,
                        Message = "Customer not exist. Please Registor"
                    };
                    return res;
                }
            }

        }

        [Route("SignupKPP")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public async Task<DataContracts.KPPApp.customerDetails> SignupCustomer(signup obj)
        {
            using (AuthContext db = new AuthContext())
            {
                DataContracts.KPPApp.customerDetails res;

                Customer customers = db.Customers.Where(c => c.Deleted == false && c.Mobile.Trim().Equals(obj.Mobile.Trim())).FirstOrDefault();
                if (!string.IsNullOrEmpty(obj.RefNo))
                {
                    var checkgst = db.Customers.Where(x => x.RefNo == obj.RefNo).Count();
                    if (checkgst > 0)
                    {
                        res = new DataContracts.KPPApp.customerDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "Gst Already Exsits."
                        };
                        return res;
                    }
                }
                if (customers != null)
                {
                    var dv = db.DistributorVerificationDB.FirstOrDefault(x => x.CustomerID == customers.CustomerId);
                    if (dv == null)
                    {
                        dv = new DistributorVerification();
                        dv.CustomerID = customers.CustomerId;
                        dv.TypeOfBusiness = obj.TypeOfBusiness;
                        dv.FirmName = obj.FirmName;
                        dv.CreatedDate = DateTime.Now;
                        dv.CreatedBy = customers.CustomerId;
                        dv.IsActive = true;
                        dv.IsDeleted = false;
                        db.DistributorVerificationDB.Add(dv);
                        db.Commit();

                        var registeredApk = db.GetAPKUserAndPwd("DistributorApp");
                        customers.RegisteredApk = registeredApk;
                        var cust = Mapper.Map(customers).ToANew<customers>();
                        cust.FirmName = obj.FirmName;

                        res = new DataContracts.KPPApp.customerDetails()
                        {
                            customers = cust,
                            Status = true,
                            Message = "Customer registor successfully."
                        };
                    }
                    else
                    {
                        res = new DataContracts.KPPApp.customerDetails()
                        {
                            customers = Mapper.Map(customers).ToANew<customers>(),
                            Status = false,
                            Message = "Customer already exist. please login"
                        };
                    }
                    return res;
                }
                else
                {
                    City City = db.Cities.Where(a => a.CityName == obj.City).FirstOrDefault();
                    customers = new Customer();
                    customers.Mobile = obj.Mobile;
                    customers.Name = obj.Name;
                    customers.Skcode = this.skcode();
                    customers.ShopName = obj.ShopName;
                    customers.ShippingAddress = obj.ShippingAddress;
                    customers.BillingAddress = obj.BillingAddress;

                    customers.BillingCity = obj.BillingCity;
                    customers.BillingState = obj.BillingState;
                    customers.BillingZipCode = obj.BillingZipCode;

                    customers.RefNo = obj.RefNo;
                    customers.NameOnGST = obj.NameOnGST;
                    customers.Password = obj.Password;
                    customers.Cityid = City?.Cityid;
                    customers.City = obj.City;
                    customers.ShippingCity = obj.City;
                    customers.fcmId = obj.fcmId;
                    customers.CurrentAPKversion = obj.CurrentAPKversion;
                    customers.imei = obj.imei;// Sudhir to save device info
                    customers.PhoneOSversion = obj.PhoneOSversion;
                    customers.UserDeviceName = obj.UserDeviceName;
                    customers.IsCityVerified = true;
                    customers.IsSignup = true;
                    customers.CreatedDate = indianTime;
                    customers.UpdatedDate = indianTime;
                    //customers.ExecutiveId = 0;
                    customers.IsKPP = true;
                    customers.lat = obj.lat;
                    customers.lg = obj.lg;
                    customers.BankName = obj.BankName;
                    customers.IfscCode = obj.IfscCode;
                    customers.AccountNumber = obj.AccountNumber;
                    customers.LicenseNumber = obj.LicenseNumber;
                    customers.FSAAI = obj.LicenseNumber;
                    customers.UploadRegistration = obj.UploadRegistration;
                    customers.Emailid = obj.Emailid;
                    customers.LandMark = obj.LandMark;
                    customers.Shopimage = obj.Shopimage;
                    customers.UploadGSTPicture = obj.UploadGSTPicture;
                    customers.UserDeviceName = obj.UserDeviceName;
                    customers.ZipCode = obj.ZipCode;
                    customers.deviceId = obj.deviceId;
                    customers.TypeOfBuissness = obj.TypeOfBusiness;
                    customers.UploadProfilePichure = obj.UploadProfilePichure;
                    customers.Active = false;
                    customers.Deleted = false;
                    customers.CustomerAppType = 4; //tejas 
                    customers.Type = obj.Type;
                    customers.UploadLicensePicture = obj.UploadLicensePicture;
                    customers.AadharNo = obj.OtherNo;

                    WarehouseMinDc warehouseMinDc = null;
                    if (City != null)
                    {
                        var warehousesId = db.Warehouses.Where(x => x.Cityid == City.Cityid).Select(x => x.WarehouseId).ToList();
                        warehouseMinDc = db.itemMasters.Where(x => warehousesId.Contains(x.WarehouseId) && x.active && x.DistributorShow && x.DistributionPrice.HasValue && x.DistributionPrice.Value > 0).Select(x => new WarehouseMinDc { WarehouseId = x.WarehouseId, WarehouseName = x.WarehouseName }).FirstOrDefault();
                    }
                    if (customers.lat != 0 && customers.lg != 0 && warehouseMinDc == null)
                    {
                        var query = new StringBuilder("Exec GetWarehouseNearyou ").Append(customers.lat).Append(",").Append(customers.lg).Append(" ");
                        warehouseMinDc = db.Database.SqlQuery<WarehouseMinDc>(query.ToString()).FirstOrDefault();
                    }

                    if (warehouseMinDc != null)
                    {
                        var cluster = db.Clusters.Where(x => x.WarehouseId == warehouseMinDc.WarehouseId && x.Active && !x.Deleted).FirstOrDefault();
                        customers.InRegion = true;
                        customers.Warehouseid = warehouseMinDc.WarehouseId;
                        customers.WarehouseName = warehouseMinDc.WarehouseName;
                        customers.ClusterId = cluster?.ClusterId;
                        customers.ClusterName = cluster?.ClusterName;
                    }
                    else
                    {
                        customers.InRegion = false;
                    }

                    if (!string.IsNullOrEmpty(customers.RefNo))
                    {
                        var custGstVerifys = db.CustGSTverifiedRequestDB.Where(x => x.RefNo == customers.RefNo).ToList();
                        if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                        {
                            var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                            var state = db.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                            customers.BillingCity = gstVerify.City;
                            customers.BillingState = state != null ? state.StateName : gstVerify.State;
                            customers.BillingZipCode = gstVerify.Zipcode;
                            customers.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                            customers.NameOnGST = gstVerify.Name;
                        }

                    }

                    if (string.IsNullOrEmpty(customers.BillingAddress))
                    {
                        customers.BillingCity = customers.City;
                        customers.BillingState = customers.State;
                        customers.BillingZipCode = customers.ZipCode;
                        customers.BillingAddress = customers.ShippingAddress;
                    }

                    db.Customers.Add(customers);
                    if (db.Commit() > 0)
                    {
                        DistributorVerification dv = new DistributorVerification();
                        dv.CustomerID = customers.CustomerId;
                        dv.TypeOfBusiness = obj.TypeOfBusiness;
                        dv.FirmName = obj.FirmName;
                        dv.CreatedDate = DateTime.Now;
                        dv.CreatedBy = customers.CustomerId;
                        dv.Status = "Signup Complete";
                        dv.IsActive = true;
                        dv.IsDeleted = false;
                        db.DistributorVerificationDB.Add(dv);

                        #region Device History

                        var Customerhistory = customers;

                        PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                        if (Customerhistory != null)
                        {
                            phonerecord.CustomerId = Customerhistory.CustomerId;
                            phonerecord.Name = Customerhistory.Name;
                            phonerecord.Skcode = Customerhistory.Skcode;
                            phonerecord.Mobile = Customerhistory.Mobile;
                            phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                            phonerecord.IMEI = Customerhistory.imei;//Sudhir to save device info
                            phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                            phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                            phonerecord.UpdatedDate = DateTime.Now;
                            db.PhoneRecordHistoryDB.Add(phonerecord);

                        }

                        #endregion
                        int id = db.Commit();

                        var registeredApk = db.GetAPKUserAndPwd("DistributorApp");
                        customers.RegisteredApk = registeredApk;
                        var cust = Mapper.Map(customers).ToANew<customers>();
                        cust.FirmName = obj.FirmName;

                        res = new DataContracts.KPPApp.customerDetails()
                        {
                            customers = cust,
                            Status = true,
                            Message = "Customer registor successfully."
                        };
                    }
                    else
                    {
                        res = new DataContracts.KPPApp.customerDetails()
                        {
                            customers = null,
                            Status = false,
                            Message = "Some error occurred during registation."
                        };
                    }

                    return res;
                }
            }
        }

        [Route("PaymentReceived")]
        [HttpGet]
        public async Task<profile> PaymentReceived(int customerId)
        {
            using (var db = new AuthContext())
            {
                var res = new profile();
                var distributor = db.DistributorVerificationDB.FirstOrDefault(x => x.CustomerID == customerId && x.Status == "Documents Uploaded");
                if (distributor != null)
                {
                    distributor.Status = "Payment Received";
                    distributor.ModifiedBy = customerId;
                    distributor.ModifiedDate = DateTime.Now;
                    db.Entry(distributor).State = EntityState.Modified;
                    db.Commit();
                    var profile = new distributor
                    {
                        aadhar = distributor.Aadhar,
                        advanceAmount = distributor.AdvanceAmount,
                        aadharNo = distributor.AadharNo,
                        blankCheque = distributor.BlankCheque,
                        customerID = distributor.CustomerID,
                        deliveryVehicle = distributor.DeliveryVehicle,
                        deliveryVehicleNo = distributor.DeliveryVehicleNo,
                        drugLicense = distributor.DrugLicense,
                        drugLicenseNo = distributor.DrugLicenseNo,
                        drugLicenseValidity = distributor.DrugLicenseValidity,
                        foodLicense = distributor.FoodLicense,
                        foodLicenseNo = distributor.FoodLicenseNo,
                        franchiseeKKP = distributor.FranchiseeKKP,
                        gst = distributor.GST,
                        gstNo = distributor.GSTNo,
                        isVerified = distributor.IsVerified,
                        manpower = distributor.Manpower,
                        pan = distributor.PAN,
                        panNo = distributor.PANNo,
                        signature = distributor.Signature,
                        sourceofFund = distributor.SourceofFund,
                        status = distributor.Status,
                        warehouseFacility = distributor.WarehouseFacility,
                        warehouseSize = distributor.WarehouseSize
                    };
                    var customer = db.Customers.Where(x => x.CustomerId == customerId).FirstOrDefault();
                    if (profile != null)
                    {
                        var registeredApk = db.GetAPKUserAndPwd("DistributorApp");
                        customer.RegisteredApk = registeredApk;
                        res.customer = Mapper.Map(customer).ToANew<DataContracts.KPPApp.customers>();
                        res.distributor = profile;
                        res.status = true;
                        return res;
                    }
                    else
                    {
                        res.customer = null;
                        res.distributor = null;
                        res.status = false;
                        return res;
                    }
                }
                else
                {
                    res.customer = null;
                    res.distributor = null;
                    res.status = false;
                    return res;
                }
            }
        }
        #endregion

        [Route("UpdateDistributor")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public async Task<distributor> UpdateDistributor(distributor dv)
        {
            using (var db = new AuthContext())
            {
                var distributor = db.DistributorVerificationDB.Where(x => x.CustomerID == dv.customerID).FirstOrDefault();
                var customer = await db.Customers.FirstOrDefaultAsync(x => x.CustomerId == dv.customerID);
                var customerGullak = db.GullakDB.Where(x => x.CustomerId == customer.CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (customerGullak == null)
                {
                    customerGullak = new Gullak
                    {
                        CustomerId = customer.CustomerId,
                        TotalAmount = 0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = Convert.ToInt32(customer.CustomerId),
                        CreatedDate = DateTime.Now,
                    };
                    db.GullakDB.Add(customerGullak);
                }
                if (distributor != null)
                {
                    if (distributor.IsVerified == false)
                    {
                        if (!string.IsNullOrEmpty(dv.TypeOFBusiness) && !string.IsNullOrEmpty(dv.FirmName))
                        {
                            distributor.TypeOfBusiness = dv.TypeOFBusiness;
                            distributor.FirmName = dv.FirmName;
                        }
                        distributor.Manpower = dv.manpower;
                        distributor.AdvanceAmount = dv.advanceAmount;
                        distributor.FranchiseeKKP = dv.franchiseeKKP;
                        distributor.SourceofFund = dv.sourceofFund;
                        distributor.DrugLicenseNo = dv.drugLicenseNo;
                        distributor.DrugLicenseValidity = dv.drugLicenseValidity;
                        distributor.DrugLicense = dv.drugLicense;
                        distributor.FoodLicenseNo = dv.foodLicenseNo;
                        distributor.FoodLicense = dv.foodLicense;
                        distributor.GSTNo = dv.gstNo;
                        customer.RefNo = dv.gstNo;
                        customer.NameOnGST = dv.NameOnGST;
                        if (!string.IsNullOrEmpty(customer.RefNo))
                        {
                            var custGstVerifys = db.CustGSTverifiedRequestDB.Where(x => x.RefNo == customer.RefNo).ToList();
                            if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                            {
                                var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                                var state = db.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
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


                        distributor.GST = dv.gst;
                        distributor.PANNo = dv.panNo;
                        distributor.PAN = dv.pan;
                        distributor.AadharNo = dv.aadharNo;
                        distributor.Aadhar = dv.aadhar;
                        distributor.WarehouseFacility = dv.warehouseFacility;
                        distributor.WarehouseSize = dv.warehouseSize;
                        distributor.DeliveryVehicle = dv.deliveryVehicle;
                        distributor.DeliveryVehicleNo = dv.deliveryVehicleNo;
                        distributor.BlankCheque = dv.blankCheque;
                        distributor.Status = "Documents Uploaded";
                        distributor.IsActive = true;
                        distributor.IsDeleted = false;
                        db.Entry(distributor).State = EntityState.Modified;
                        db.Entry(customer).State = EntityState.Modified;
                        db.Commit();
                    }
                    return dv;
                }
                else
                {
                    distributor = new DistributorVerification();

                    distributor.CustomerID = dv.customerID;
                    distributor.TypeOfBusiness = dv.TypeOFBusiness;
                    distributor.FirmName = dv.FirmName;
                    distributor.Manpower = dv.manpower;
                    distributor.AdvanceAmount = dv.advanceAmount;
                    distributor.FranchiseeKKP = dv.franchiseeKKP;
                    distributor.SourceofFund = dv.sourceofFund;
                    distributor.DrugLicenseNo = dv.drugLicenseNo;
                    distributor.DrugLicenseValidity = dv.drugLicenseValidity;
                    distributor.DrugLicense = dv.drugLicense;
                    distributor.FoodLicenseNo = dv.foodLicenseNo;
                    distributor.FoodLicense = dv.foodLicense;
                    distributor.GSTNo = dv.gstNo;
                    customer.RefNo = dv.gstNo;
                    customer.NameOnGST = dv.NameOnGST;
                    distributor.GST = dv.gst;
                    distributor.PANNo = dv.panNo;
                    distributor.PAN = dv.pan;
                    distributor.AadharNo = dv.aadharNo;
                    distributor.Aadhar = dv.aadhar;
                    distributor.WarehouseFacility = dv.warehouseFacility;
                    distributor.WarehouseSize = dv.warehouseSize;
                    distributor.DeliveryVehicle = dv.deliveryVehicle;
                    distributor.DeliveryVehicleNo = dv.deliveryVehicleNo;
                    distributor.BlankCheque = dv.blankCheque;
                    distributor.Status = "Documents Uploaded";
                    distributor.CreatedDate = DateTime.Now;
                    distributor.CreatedBy = dv.customerID;
                    distributor.IsActive = true;
                    distributor.IsDeleted = false;
                    db.DistributorVerificationDB.Add(distributor);
                    db.Entry(customer).State = EntityState.Modified;
                    db.Commit();
                    return dv;
                }
            }
        }



        [Route("AcceptAgreement")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public async Task<distributor> AcceptAgreement(distributor dv)
        {
            using (var db = new AuthContext())
            {
                var distributor = db.DistributorVerificationDB.Where(x => x.CustomerID == dv.customerID).FirstOrDefault();
                var customer = await db.Customers.FirstOrDefaultAsync(x => x.CustomerId == dv.customerID);
                var customerGullak = db.GullakDB.Where(x => x.CustomerId == dv.customerID && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (customerGullak == null)
                {
                    customerGullak = new Gullak
                    {
                        CustomerId = dv.customerID,
                        TotalAmount = 0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = Convert.ToInt32(dv.customerID),
                        CreatedDate = DateTime.Now,
                    };
                    db.GullakDB.Add(customerGullak);
                }
                if (distributor != null)
                {
                    if (distributor.IsVerified == false)
                    {
                        distributor.Status = "Agreement Accepted";
                        distributor.Signature = dv.signature;
                        var SignedPdf = await GetSignedPDF(distributor, customer);
                        distributor.SignedPdf = SignedPdf;
                        distributor.IsActive = true;
                        distributor.IsDeleted = false;
                        db.Commit();
                        dv.SignedPdf = SignedPdf;
                    }

                }
                return dv;
            }
        }




        [Route("GetSignedAgreement")]
        [AcceptVerbs("GET")]
        [AllowAnonymous]
        public async Task<string> GetSignedAgreement(int customerId, int warehouseid)
        {
            using (var db = new AuthContext())
            {
                var res = new profile();
                var profile = db.DistributorVerificationDB.Where(x => x.CustomerID == customerId).FirstOrDefault();
                var cust = db.Customers.Where(x => x.CustomerId == customerId).FirstOrDefault();
                var DistributorCompanydetail = db.DistributorCompanydetailDB.Where(x => x.WarehouseId == 1).FirstOrDefault();
                //string signaturePath = "";

                if (cust != null)
                {
                    string agreement = DistributorCompanydetail.AgreementHtml;

                    //string logopath = HttpContext.Current.Server.MapPath("~/SignedDistributorPdf/") + "logo.png";
                    //string logopath = @"C:\Users\SKPC-praveen\Desktop\New folder (2)\rds-agreement\images\logo.png";
                    // string signaturepath = Path.Combine(HttpContext.Current.Server.MapPath("~/DistributorImages/"), profile?.Signature??"");
                    var logopath = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , HttpContext.Current.Request.Url.Port
                                                              , string.Format("SignedDistributorPdf/{0}", "logo.png"));
                    var signaturepath = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                              , HttpContext.Current.Request.Url.DnsSafeHost
                                                              , HttpContext.Current.Request.Url.Port
                                                              , string.Format("DistributorImages/{0}", profile?.Signature ?? ""));

                    agreement = agreement.Replace("@LogoImage", logopath);
                    agreement = agreement.Replace("@AgreementDate", DateTime.Now.ToString("dd/MM/yyyy"));
                    agreement = agreement.Replace("@FirmName", profile?.FirmName ?? "");
                    agreement = agreement.Replace("@Address", cust.BillingAddress);
                    //agreement = agreement.Replace("@CityStatePin", cust.City + " " + cust.State + " " +cust.pin);
                    agreement = agreement.Replace("@Mobile", cust.Mobile);
                    agreement = agreement.Replace("@OneYear", DateTime.Now.AddYears(+1).ToString("dd/MM/yyyy"));
                    agreement = agreement.Replace("@City", cust.City);
                    agreement = agreement.Replace("@SignatureImage", signaturepath);
                    agreement = agreement.Replace("@CustomerName", cust.Name);

                    return agreement;
                }
                else
                {
                    return null;
                }
            }
        }

        [Route("GetDistributorScheme")]
        [AcceptVerbs("GET")]
        [AllowAnonymous]
        public async Task<string> GetDistributorScheme(int customerId, int warehouseid)
        {
            using (var db = new AuthContext())
            {
                var DistributorCompanydetail = db.DistributorCompanydetailDB.Where(x => x.WarehouseId == 1).FirstOrDefault();
                string agreement = DistributorCompanydetail.SchemeHtml;
                return agreement;

            }
        }

        private async Task<string> GetSignedPDF(DistributorVerification distributor, Customer customer)
        {
            string returnfile = "";

            using (var db = new AuthContext())
            {
                var DistributorCompanydetail = db.DistributorCompanydetailDB.Where(x => x.WarehouseId == 1).FirstOrDefault();
                //string PathLogo = "logo.png";
                if (distributor != null && customer != null)
                {
                    string agreement = DistributorCompanydetail.AgreementHtml;
                    //string agreement = System.IO.File.ReadAllText(@"C:\Users\SKPC-praveen\Desktop\new.html");
                    //DistributorCompanydetail.AgreementHtml = agreement;
                    //db.Entry(DistributorCompanydetail).State = EntityState.Modified;
                    //db.Commit();

                    //string logopath = HttpContext.Current.Server.MapPath("~/SignedDistributorPdf/") + "logo.png";
                    ////string logopath = @"C:\Users\SKPC-praveen\Desktop\New folder (2)\rds-agreement\images\logo.png";
                    //string signaturepath = HttpContext.Current.Server.MapPath("~/DistributorImages/") + distributor.Signature;



                    var logopath = string.Empty;
                    var signaturepath = string.Empty;
                    if (AppConstants.Environment == "prod")
                    {
                        logopath = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                                                                 , HttpContext.Current.Request.Url.Port
                                                                 , string.Format("SignedDistributorPdf/{0}", "logo.png"));
                        signaturepath = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                                 , HttpContext.Current.Request.Url.DnsSafeHost
                                                                 , HttpContext.Current.Request.Url.Port
                                                                 , string.Format("DistributorImages/{0}", distributor?.Signature ?? ""));
                    }
                    else
                    {
                        logopath = string.Format("{0}/{1}", "http://192.168.1.50:6787"
                                                                 , string.Format("SignedDistributorPdf/{0}", "logo.png"));
                        signaturepath = string.Format("{0}/{1}", "http://192.168.1.50:6787"
                                                                 , string.Format("DistributorImages/{0}", distributor?.Signature ?? ""));
                    }

                    agreement = agreement.Replace("@LogoImage", logopath);
                    agreement = agreement.Replace("@AgreementDate", DateTime.Now.ToString("dd/MM/yyyy"));
                    agreement = agreement.Replace("@FirmName", distributor.FirmName);
                    agreement = agreement.Replace("@Address", customer.ShippingAddress);
                    //agreement = agreement.Replace("@CityStatePin", cust.City + " " + cust.State + " " +cust.pin);
                    agreement = agreement.Replace("@Mobile", customer.Mobile);
                    agreement = agreement.Replace("@OneYear", DateTime.Now.AddYears(+1).ToString("dd/MM/yyyy"));
                    agreement = agreement.Replace("@City", customer.City);
                    agreement = agreement.Replace("@SignatureImage", signaturepath);
                    agreement = agreement.Replace("@CustomerName", customer.Name);

                    if (!string.IsNullOrEmpty(agreement))
                    {
                        logger.Error("Agreement: " + agreement);
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/SignedDistributorPdf")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/SignedDistributorPdf"));

                        var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/SignedDistributorPdf"), customer.Skcode + "_SignedAgreement.pdf");
                        //var OutPutFile = @"C:\Users\SKPC-praveen\Desktop\Orange Book Committee.pdf";

                        byte[] pdf = null;

                        pdf = Pdf
                              .From(agreement)
                              //.WithGlobalSetting("orientation", "Landscape")
                              //.WithObjectSetting("web.defaultEncoding", "utf-8")
                              .OfSize(PaperSize.A4)
                              .WithTitle("Invoice")
                              .WithoutOutline()
                              .WithMargins(PaperMargins.All(0.0.Millimeters()))
                              .Portrait()
                              .Comressed()
                              .Content();
                        FileStream file = File.Create(OutPutFile);
                        file.Write(pdf, 0, pdf.Length);
                        file.Close();
                        return returnfile = "/SignedDistributorPdf/" + customer.Skcode + "_SignedAgreement.pdf";
                    }
                }

            }


            return returnfile;
        }


        //[Route("GetSignedPDF1")]
        //[AcceptVerbs("POST")]
        //[AllowAnonymous]
        //public void GetSignedPDF(updation a)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        var DistributorCompanydetail = db.DistributorCompanydetailDB.Where(x => x.WarehouseId == 1).FirstOrDefault();

        //        string agreement = DistributorCompanydetail.AgreementHtml;

        //        var logopath = "https://er15.xyz:4436/SignedDistributorPdf/logo.png";
        //        var signaturepath = "https://er15.xyz:4436/DistributorImages/" + a.Signature;

        //        agreement = agreement.Replace("@LogoImage", logopath);
        //        agreement = agreement.Replace("@AgreementDate", a.Createddate);
        //        agreement = agreement.Replace("@FirmName", a.FirmName);
        //        agreement = agreement.Replace("@Address", a.BillingAddress);
        //        agreement = agreement.Replace("@Mobile", a.Mobile);
        //        agreement = agreement.Replace("@OneYear", a.oneyear);
        //        agreement = agreement.Replace("@City", a.City);
        //        agreement = agreement.Replace("@SignatureImage", signaturepath);
        //        agreement = agreement.Replace("@CustomerName", a.CustomerName);

        //        if (!string.IsNullOrEmpty(agreement))
        //        {
        //            var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/SignedDistributorPdf"), a.Skcode + "_SignedAgreement.pdf");

        //            byte[] pdf = null;

        //            pdf = Pdf
        //                  .From(agreement)
        //                  .OfSize(PaperSize.A4)
        //                  .WithTitle("Invoice")
        //                  .WithoutOutline()
        //                  .WithMargins(PaperMargins.All(0.0.Millimeters()))
        //                  .Portrait()
        //                  .Comressed()
        //                  .Content();
        //            FileStream file = File.Create(OutPutFile);
        //            file.Write(pdf, 0, pdf.Length);
        //            file.Close();
        //        }
        //    }            
        //}



        [Route("UpdateCustProfile")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> UpdateCustProfile(updateCustomerProfile cp)
        {
            using (var db = new AuthContext())
            {
                if (!string.IsNullOrEmpty(cp.LicenseNumber.Trim()) && cp.LicenseNumber != "0")
                {
                    var checkgst = db.Customers.Where(x => x.LicenseNumber == cp.LicenseNumber && x.CustomerId != cp.customerID).Count();
                    if (checkgst > 0)
                    {
                        var rs1 = new
                        {
                            Status = false,
                            Message = "License Number Already Exsits."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, rs1);
                    }
                }

                #region for Duplicate PAN no. check
                if (!string.IsNullOrEmpty(cp.PanNo.Trim()))
                {
                    Customer IsExistPAN = db.Customers.Where(c => c.PanNo.ToLower() == cp.PanNo.ToLower()).FirstOrDefault();
                    if (IsExistPAN != null)
                    {
                        var rs2 = new
                        {
                            Status = false,
                            Message = "PAN Already Exists."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, rs2);

                    }
                }
                #endregion


                var customer = db.Customers.Where(x => x.CustomerId == cp.customerID).FirstOrDefault();
                if (customer != null)
                {
                    if (cp.DOB != DateTime.MinValue)
                    {
                        customer.DOB = cp.DOB;
                    }
                    if (cp.AnniversaryDate != DateTime.MinValue)
                    {
                        customer.AnniversaryDate = cp.AnniversaryDate;
                    }
                    customer.Name = cp.Name;
                    customer.ShopName = cp.ShopName;
                    customer.UploadGSTPicture = cp.UploadGSTPicture;
                    customer.AreaName = cp.AreaName;
                    customer.BillingAddress = cp.BillingAddress;
                    customer.Emailid = cp.Emailid;
                    customer.lat = cp.lat;
                    customer.lg = cp.lg;
                    customer.LicenseNumber = cp.LicenseNumber;
                    customer.Mobile = cp.Mobile;
                    customer.RefNo = cp.RefNo;
                    customer.ShippingAddress = cp.ShippingAddress;
                    customer.Shopimage = cp.Shopimage;
                    customer.UploadRegistration = cp.UploadRegistration;
                    customer.WhatsappNumber = cp.WhatsappNumber;
                    customer.UpdatedDate = DateTime.Now;
                    customer.UploadProfilePichure = cp.UploadProfilePichure;
                    customer.Type = cp.Type;
                    customer.AadharNo = cp.AadharNo;
                    customer.UploadLicensePicture = cp.UploadLicensePicture;
                    customer.PanNo = cp.PanNo;
                    db.Entry(customer).State = EntityState.Modified;



                    if (!string.IsNullOrEmpty(cp.RefNo))
                    {
                        var custGstVerifys = db.CustGSTverifiedRequestDB.Where(x => x.RefNo == cp.RefNo).ToList();
                        if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                        {
                            var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                            var state = db.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                            customer.BillingCity = gstVerify.City;
                            customer.BillingState = state != null ? state.StateName : gstVerify.State;
                            customer.BillingZipCode = gstVerify.Zipcode;
                            customer.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                            customer.NameOnGST = gstVerify.Name;
                        }
                    }

                    if ((!string.IsNullOrEmpty(cp.LicenseNumber) && cp.CustomerDocTypeMasterId > 0) || (!string.IsNullOrEmpty(cp.RefNo) && !string.IsNullOrEmpty(cp.UploadGSTPicture)))
                    {
                        var customerdocs = db.CustomerDocs.Where(x => x.CustomerId == customer.CustomerId).ToList();
                        var docMaster = db.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();

                        foreach (var custdoc in customerdocs)
                        {
                            custdoc.ModifiedBy = 0;
                            custdoc.ModifiedDate = DateTime.Now;
                            custdoc.IsActive = false;
                            db.Entry(custdoc).State = EntityState.Modified;
                        }

                        if (!string.IsNullOrEmpty(cp.RefNo) && !string.IsNullOrEmpty(cp.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                        {
                            var docid = docMaster.FirstOrDefault(x => x.DocType == "GST").Id;
                            //if (customerdocs.Any(x => x.CustomerDocTypeMasterId == docid && x.IsActive))
                            //{
                            //    var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == docid && x.IsActive);
                            //    custdoc.ModifiedBy = 0;
                            //    custdoc.ModifiedDate = DateTime.Now;
                            //    custdoc.IsActive = false;
                            //    db.Entry(custdoc).State = EntityState.Modified;
                            //}
                            db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = customer.CustomerId,
                                IsActive = true,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = docid,
                                DocPath = cp.UploadGSTPicture,
                                IsDeleted = false
                            });
                        }

                        if (!string.IsNullOrEmpty(cp.LicenseNumber) && cp.CustomerDocTypeMasterId > 0)
                        {
                            //if (customerdocs.Any(x => x.CustomerDocTypeMasterId == cp.CustomerDocTypeMasterId && x.IsActive))
                            //{
                            //    var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == cp.CustomerDocTypeMasterId && x.IsActive);
                            //    custdoc.ModifiedBy = 0;
                            //    custdoc.ModifiedDate = DateTime.Now;
                            //    custdoc.IsActive = false;
                            //    db.Entry(custdoc).State = EntityState.Modified;
                            //}
                            db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = customer.CustomerId,
                                IsActive = true,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = cp.CustomerDocTypeMasterId,
                                DocPath = cp.UploadRegistration,
                                IsDeleted = false
                            });
                        }
                    }


                    var dv = db.DistributorVerificationDB.FirstOrDefault(x => x.CustomerID == customer.CustomerId);
                    if (dv != null)
                    {
                        dv.Aadhar = cp.AadharPicture;
                        dv.PAN = cp.PanPicture;
                        dv.AadharNo = cp.AadharNo;
                        dv.PANNo = cp.PanNo;
                        dv.ModifiedDate = DateTime.Now;
                        dv.ModifiedBy = customer.CustomerId;
                        dv.GSTNo = cp.RefNo;
                        db.Entry(dv).State = EntityState.Modified;
                    }

                    //return Mapper.Map(customer).ToANew<customers>();
                }
                db.Commit();
                var rs = new
                {
                    customer = customer,
                    Status = true,
                    Message = "Updated Successfully.."
                };
                return Request.CreateResponse(HttpStatusCode.OK, rs);
            }
        }

        [Route("ConvertPDF")]
        [AcceptVerbs("GET")]
        public async Task<string> ConvertPDF(string skcode, string signed)
        {
            return await AddSignatureToPDF(signed, skcode, "Test name");
        }

        private async Task<string> AddSignatureToPDF(string signaturePath, string skcode, string customerName)
        {
            var fileuploadPath = HttpContext.Current.Server.MapPath("~/SignedDistributorPdf");
            signaturePath = HttpContext.Current.Server.MapPath("~/DistributorImages/") + signaturePath;
            var Defaultfile = HttpContext.Current.Server.MapPath("~/UploadedImages/") + "KK_RDS_Agreement_Final.pdf";
            if (!Directory.Exists(fileuploadPath))
            {
                Directory.CreateDirectory(fileuploadPath);
            }
            // Validate the uploaded image(optional)
            // Get the complete file path

            var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/SignedDistributorPdf"), skcode + "_SignedAgreement.pdf");
            string resultfile = "/SignedDistributorPdf/" + skcode + "_SignedAgreement.pdf";
            using (Stream inputPdfStream = new FileStream(Defaultfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream inputImageStream = new FileStream(signaturePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (Stream outputPdfStream = new FileStream(OutPutFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var reader = new PdfReader(inputPdfStream);
                var stamper = new PdfStamper(reader, outputPdfStream);
                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(inputImageStream);
                image.SetAbsolutePosition(565, 31);
                image.ScaleToFit(170f, 50f);
                image.SpacingBefore = 1f;
                image.SpacingAfter = 1f;
                for (int page = 2; page <= reader.NumberOfPages; page++) // for loop will add image to each page. Based on the condition you can add image to single page also 
                {
                    var pagesize = reader.GetPageSize(page);
                    var pdfContentByte = stamper.GetOverContent(page);
                    pdfContentByte.AddImage(image);
                    iTextSharp.text.Font font = new iTextSharp.text.Font();
                    font.Size = 10;

                    //setting up the X and Y coordinates of the document
                    int x = 565;
                    int y = 80;

                    // y = (int)(pagesize.Height - y);

                    ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(DateTime.Now.ToString("dd/MM/yyyy"), font), x, y, 0);
                    x = 550;
                    y = 70;
                    ColumnText.ShowTextAligned(pdfContentByte, Element.ALIGN_CENTER, new Phrase(customerName, font), x, y, 0);
                }

                stamper.Close();
            }
            return resultfile;
        }


        [Route("GetDistributorProfile")]
        [AcceptVerbs("GET")]

        public async Task<profile> GetDistributorProfile(int customerId)
        {
            using (var db = new AuthContext())
            {
                var res = new profile();
                var profile = db.DistributorVerificationDB.Where(x => x.CustomerID == customerId).FirstOrDefault();
                var customer = db.Customers.Where(x => x.CustomerId == customerId).FirstOrDefault();
                if (customer != null)
                {
                    //CustomerWarehouseAddress:
                    var customerWarehouse = db.Warehouses.Where(x => x.active == true && x.WarehouseId == customer.Warehouseid).FirstOrDefault();

                    var registeredApk = db.GetAPKUserAndPwd("DistributorApp");
                    customer.RegisteredApk = registeredApk;
                    var custDocs = db.CustomerDocs.Where(x => x.CustomerId == customer.CustomerId && x.IsActive).ToList();
                    var gstDocttypeid = db.CustomerDocTypeMasters.FirstOrDefault(x => x.IsActive && x.DocType == "GST")?.Id;
                    if (gstDocttypeid.HasValue && custDocs.Any(y => y.CustomerDocTypeMasterId != gstDocttypeid.Value))
                    {
                        customer.CustomerDocTypeMasterId = custDocs.FirstOrDefault(y => y.CustomerDocTypeMasterId != gstDocttypeid.Value).CustomerDocTypeMasterId;
                    }
                    res.customer = Mapper.Map(customer).ToANew<customers>();
                    res.customer.WarehouseAddress = customerWarehouse.Address;
                    res.customer.WarehouseLat = customerWarehouse.latitude;
                    res.customer.WarehouseLong = customerWarehouse.longitude;
                    res.distributor = profile != null ? Mapper.Map(profile).ToANew<distributor>() : new distributor { status = "" };
                    res.status = true;
                    return res;
                }
                else
                {
                    res.customer = null;
                    res.distributor = null;
                    res.status = false;
                    return res;
                }
            }
        }

        [Route("GetDistributorGullak")]
        [AcceptVerbs("GET")]
        public async Task<gullak> GetDistributorGullak(int customerId)
        {
            using (var db = new AuthContext())
            {
                var customergullak = new gullak();
                var Gullak = db.GullakDB.Where(x => x.CustomerId == customerId).FirstOrDefault();
                if (Gullak != null)
                {
                    customergullak.Amount = Gullak.TotalAmount;
                    customergullak.status = true;
                    return customergullak;
                }
                else
                {
                    customergullak.Amount = 0;
                    customergullak.status = false;
                    return customergullak;
                }
            }
        }

        [Route("GetDistributorUPIid")]
        [AcceptVerbs("GET")]
        public async Task<string> GetDistributorUPIid()
        {
            using (var db = new AuthContext())
            {
                var upiid = db.CompanyDetailsDB.Select(x => x.DistributorUPIId).FirstOrDefault();
                if (upiid != null)
                {
                    return upiid;
                }
                else
                {
                    return null;
                }
            }
        }

        #region Payment


        [Route("PaymentRequest")]
        [AcceptVerbs("POST")]
        public async Task<long> PaymentRequest(paymentrequest payment)
        {
            using (var db = new AuthContext())
            {
                var customerGullak = db.GullakDB.Where(x => x.CustomerId == payment.customerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (customerGullak == null)
                {
                    customerGullak = new Gullak
                    {
                        CustomerId = payment.customerId,
                        TotalAmount = 0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = Convert.ToInt32(payment.customerId),
                        CreatedDate = DateTime.Now,
                    };
                    db.GullakDB.Add(customerGullak);
                    db.Commit();
                }
                var Inpayment = new GullakInPayment();
                Inpayment.GullakId = customerGullak.Id;
                Inpayment.amount = payment.amount;
                Inpayment.Comment = payment.comment;
                Inpayment.CreatedDate = DateTime.Now;
                Inpayment.status = "Fail";
                Inpayment.CreatedBy = Convert.ToInt32(payment.customerId);
                Inpayment.CustomerId = payment.customerId;
                Inpayment.GatewayRequest = payment.GatewayRequest;
                Inpayment.PaymentFrom = payment.PaymentFrom;
                db.GullakInPaymentDB.Add(Inpayment);
                if (db.Commit() > 0)
                {
                    return Inpayment.Id;
                }
                else
                {
                    return 0;
                }
            }
        }

        [AllowAnonymous]
        [Route("PaymentPendingRequest")]
        [AcceptVerbs("POST")]
        public async Task<long> PaymentPendingRequest(paymentpendingrequest payment)
        {
            using (var db = new AuthContext())
            {
                var customerGullak = db.GullakDB.Where(x => x.CustomerId == payment.customerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (customerGullak == null)
                {
                    customerGullak = new Gullak
                    {
                        CustomerId = payment.customerId,
                        TotalAmount = 0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = Convert.ToInt32(payment.customerId),
                        CreatedDate = DateTime.Now,
                    };
                    db.GullakDB.Add(customerGullak);
                    db.Commit();
                }
                var Inpayment = new GullakInPayment();
                Inpayment.GullakId = customerGullak.Id;
                Inpayment.amount = payment.amount;
                Inpayment.Comment = payment.comment;
                Inpayment.CreatedDate = DateTime.Now;
                Inpayment.status = "Pending";
                Inpayment.CreatedBy = Convert.ToInt32(payment.customerId);
                Inpayment.CustomerId = payment.customerId;
                Inpayment.GatewayRequest = payment.GatewayRequest;
                Inpayment.PaymentFrom = payment.PaymentFrom;
                Inpayment.GullakImage = payment.GullakImage;
                Inpayment.GatewayTransId = payment.GatewayTransId;
                db.GullakInPaymentDB.Add(Inpayment);
                if (db.Commit() > 0)
                {
                    return Inpayment.Id;
                }
                else
                {
                    return 0;
                }
            }
        }

        [AllowAnonymous]
        [Route("PaymentPending")]
        [AcceptVerbs("Put")]
        public async Task<long> PaymentPending(paymentpendingrequest payment)
        {
            using (var db = new AuthContext())
            {
                var customerGullak = db.GullakDB.Where(x => x.CustomerId == payment.customerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (customerGullak == null)
                {
                    customerGullak = new Gullak
                    {
                        CustomerId = payment.customerId,
                        TotalAmount = 0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = Convert.ToInt32(payment.customerId),
                        CreatedDate = DateTime.Now,
                    };
                    db.GullakDB.Add(customerGullak);
                    db.Commit();
                }
                //var Inpayment = new GullakInPayment();
                var Inpayment = db.GullakInPaymentDB.Where(x => x.Id == payment.id).FirstOrDefault();
                if (Inpayment.Id == payment.id && Inpayment.CustomerId == payment.customerId)
                {
                    //Inpayment.GullakId = customerGullak.Id;
                    Inpayment.amount = payment.amount;
                    Inpayment.Comment = payment.comment;
                    Inpayment.CreatedDate = DateTime.Now;
                    Inpayment.status = "Pending";
                    Inpayment.GatewayRequest = payment.GatewayRequest;
                    Inpayment.PaymentFrom = payment.PaymentFrom;
                    Inpayment.GullakImage = payment.GullakImage;
                    Inpayment.GatewayTransId = payment.GatewayTransId;
                    Inpayment.ModifiedBy = payment.customerId;
                    Inpayment.ModifiedDate = DateTime.Now;
                }
                db.Entry(Inpayment).State = EntityState.Modified;
                if (db.Commit() > 0)
                {
                    return Inpayment.Id;
                }
                else
                {
                    return 0;
                }
            }
        }



        [Route("PaymentResponse")]
        [AcceptVerbs("POST")]
        public async Task<bool> PaymentResponse(paymentresponce payment)
        {
            if (payment.PaymentFrom.ToLower() == "epaylater")
                return false;

            using (var db = new AuthContext())
            {
                var Inpayment = db.GullakInPaymentDB.Where(x => x.Id == payment.id).FirstOrDefault();
                if (Inpayment != null)
                {
                    Inpayment.GatewayRequest = payment.GatewayRequest;
                    Inpayment.GatewayResponse = payment.GatewayResponse;
                    Inpayment.GatewayTransId = payment.GatewayTransId;
                    Inpayment.PaymentFrom = payment.PaymentFrom;
                    Inpayment.status = payment.status;
                    Inpayment.ModifiedBy = payment.CustomerId;
                    Inpayment.ModifiedDate = DateTime.Now;
                    db.Entry(Inpayment).State = EntityState.Modified;
                    if (!string.IsNullOrEmpty(Inpayment.status) && Inpayment.status.ToLower() == "success")
                    {
                        string query = "Exec GetGullakCashBack " + payment.CustomerId + "," + Inpayment.amount;
                        var CashBack = db.Database.SqlQuery<double>(query).FirstOrDefault();
                        GullakTransaction gullakTransaction = new GullakTransaction
                        {
                            Amount = Inpayment.amount + CashBack,
                            CreatedBy = payment.CustomerId,
                            CreatedDate = DateTime.Now,
                            CustomerId = payment.CustomerId,
                            GullakId = Inpayment.GullakId,
                            IsActive = true,
                            IsDeleted = false,
                            ObjectId = Inpayment.Id.ToString(),
                            ObjectType = "GullakInPayment"
                        };
                        db.GullakTransactionDB.Add(gullakTransaction);
                        var gullak = db.GullakDB.Where(x => x.CustomerId == payment.CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                        if (gullak != null)
                        {
                            gullak.TotalAmount = gullak.TotalAmount + Inpayment.amount;
                            gullak.ModifiedDate = DateTime.Now;
                            gullak.ModifiedBy = Convert.ToInt32(payment.CustomerId);
                            db.Entry(gullak).State = EntityState.Modified;
                        }
                    }
                }
                return db.Commit() > 0;
            }
        }


        [Route("MobilePaymentRequest")]
        [AcceptVerbs("POST")]
        public async Task<long> MobilePaymentRequest(paymentrequest payment)
        {
            if (payment.PaymentFrom.ToLower() == "epaylater")
                return 0;

            using (var db = new AuthContext())
            {
                var customerGullak = db.GullakDB.Where(x => x.CustomerId == payment.customerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var SkCode = db.Customers.FirstOrDefault(x => x.CustomerId == payment.customerId).Skcode;
                SkCode = SkCode.Replace("SK", "");
                if (customerGullak == null)
                {
                    customerGullak = new Gullak
                    {
                        CustomerId = payment.customerId,
                        TotalAmount = 0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = Convert.ToInt32(payment.customerId),
                        CreatedDate = DateTime.Now,
                    };
                    db.GullakDB.Add(customerGullak);
                    db.Commit();
                }
                var Inpayment = new GullakInPayment();
                Inpayment.GullakId = customerGullak.Id;
                Inpayment.amount = payment.amount;
                Inpayment.Comment = payment.comment;
                Inpayment.CreatedDate = DateTime.Now;
                Inpayment.status = "Fail";
                Inpayment.CreatedBy = Convert.ToInt32(payment.customerId);
                Inpayment.CustomerId = payment.customerId;
                Inpayment.GatewayRequest = payment.GatewayRequest;
                Inpayment.PaymentFrom = payment.PaymentFrom;
                db.GullakInPaymentDB.Add(Inpayment);
                if (db.Commit() > 0)
                {
                    return Convert.ToInt64(Inpayment.Id.ToString() + "" + SkCode);
                }
                else
                {
                    return 0;
                }
            }
        }

        [Route("MobilePaymentResponse")]
        [AcceptVerbs("POST")]
        public async Task<bool> MobilePaymentResponse(paymentresponce payment)
        {
            if (payment.PaymentFrom.ToLower() == "epaylater")
                return false;
            RazorpayPaymentResponse razorpayRes = null;
            using (var db = new AuthContext())
            {
                var SkCode = db.Customers.FirstOrDefault(x => x.CustomerId == payment.CustomerId).Skcode;
                SkCode = SkCode.Replace("SK", "");
                payment.id = Convert.ToInt64(payment.id.ToString().Replace(SkCode, ""));
                var Inpayment = db.GullakInPaymentDB.Where(x => x.Id == payment.id).FirstOrDefault();
                if (Inpayment != null)
                {
                    Inpayment.GatewayRequest = payment.GatewayRequest;
                    Inpayment.GatewayResponse = payment.GatewayResponse;
                    Inpayment.GatewayTransId = payment.GatewayTransId;
                    Inpayment.PaymentFrom = payment.PaymentFrom;
                    Inpayment.status = payment.status;
                    Inpayment.ModifiedBy = payment.CustomerId;
                    Inpayment.ModifiedDate = DateTime.Now;
                    db.Entry(Inpayment).State = EntityState.Modified;
                    if (!string.IsNullOrEmpty(Inpayment.status) && Inpayment.status.ToLower() == "success")
                    {
                        string query = "Exec GetGullakCashBack " + payment.CustomerId + "," + Inpayment.amount;
                        var CashBack = db.Database.SqlQuery<double>(query).FirstOrDefault();
                        if (CashBack > 0)
                        {
                            var gullakCashbackTransaction = new GullakTransaction
                            {
                                Amount = CashBack,
                                CreatedBy = payment.CustomerId,
                                CreatedDate = DateTime.Now,
                                CustomerId = payment.CustomerId,
                                GullakId = Inpayment.GullakId,
                                IsActive = true,
                                IsDeleted = false,
                                ObjectId = Inpayment.Id.ToString(),
                                ObjectType = "GullakInPayment",
                                Comment = "Cashback Scheme"
                            };
                            db.GullakTransactionDB.Add(gullakCashbackTransaction);

                        }
                        GullakTransaction gullakTransaction = new GullakTransaction
                        {
                            Amount = Inpayment.amount,
                            CreatedBy = payment.CustomerId,
                            CreatedDate = DateTime.Now,
                            CustomerId = payment.CustomerId,
                            GullakId = Inpayment.GullakId,
                            IsActive = true,
                            IsDeleted = false,
                            ObjectId = Inpayment.Id.ToString(),
                            ObjectType = "GullakInPayment",
                            Comment = "Advance payment"
                        };
                        db.GullakTransactionDB.Add(gullakTransaction);

                        var GullakCCCharges = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.GullakCCCharges).FirstOrDefault();

                        if (payment.PaymentFrom == "HDFC" && GullakCCCharges > 0)
                        {
                            var hdfcObj = JObject.Parse(payment.GatewayResponse);
                            var payMode = hdfcObj.Value<string>("payment_mode");
                            if (payMode == "Credit Card")
                            {
                                var ccCharges = Math.Round(Inpayment.amount * GullakCCCharges / 100, 2);
                                Inpayment.amount -= ccCharges;

                                GullakTransaction gullakCCTransaction = new GullakTransaction
                                {
                                    Amount = (-1) * ccCharges,
                                    CreatedBy = payment.CustomerId,
                                    CreatedDate = DateTime.Now,
                                    CustomerId = payment.CustomerId,
                                    GullakId = Inpayment.GullakId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ObjectId = Inpayment.Id.ToString(),
                                    ObjectType = "Debit",
                                    Comment = "Due to Credit Card Payment charges"
                                };
                                db.GullakTransactionDB.Add(gullakCCTransaction);
                            }
                        }

                        if (CashBack > 0)
                        {
                            Inpayment.amount += CashBack;
                        }

                        if (payment.PaymentFrom.ToLower() == "razorpay")
                        {
                            razorpayRes = JsonConvert.DeserializeObject<RazorpayPaymentResponse>(payment.GatewayResponse);
                            if (razorpayRes != null)
                            {
                                RazorPayTransactionHelper razorPayTransactionHelper = new RazorPayTransactionHelper();
                                RazorPayTransactionDC razorPayTransactionDC = new RazorPayTransactionDC()
                                {
                                    OrderId = Convert.ToInt32(Inpayment.Id.ToString() + "" + SkCode),
                                    Amount = Math.Round(Inpayment.amount, 0),
                                    response = razorpayRes,
                                };
                                bool res = await razorPayTransactionHelper.PostRazorPayTransactionAsync(razorPayTransactionDC, 0, db);
                                if (!res)
                                    return false;
                            }
                        }

                        var gullak = db.GullakDB.Where(x => x.CustomerId == payment.CustomerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                        if (gullak != null)
                        {
                            gullak.TotalAmount = gullak.TotalAmount + Inpayment.amount;
                            gullak.ModifiedDate = DateTime.Now;
                            gullak.ModifiedBy = Convert.ToInt32(payment.CustomerId);
                            db.Entry(gullak).State = EntityState.Modified;
                        }
                    }
                }
                return db.Commit() > 0;
            }
        }

        public async Task<double> GetGullakCashBack(int customerId, double amount)
        {
            double cashBackAmount = 0;
            using (var context = new AuthContext())
            {
                string query = "Exec GetGullakCashBack " + customerId + "," + amount;
                cashBackAmount = context.Database.SqlQuery<double>(query).FirstOrDefault();
            }
            return cashBackAmount;
        }
        #endregion
        public string skcode()
        {
            using (AuthContext db = new AuthContext())
            {
                var query = "select max(cast(replace(skcode,'SK','') as bigint)) from customers ";
                var intSkCode = db.Database.SqlQuery<long>(query).FirstOrDefault();
                var skcode = "SK" + (intSkCode + 1);
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

        [HttpGet]
        [Route("GetCustomerSearchKeyword")]
        public async Task<List<string>> SearchCustomerKeyWord(int customerId, int skip, int take, string lang)
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
            return keywords;

        }

        [HttpGet]
        [Route("GetAllBrand/V2")]
        public async Task<List<SubsubCategoryDT>> GetBrandWarehouseId(int WarehouseId)
        {
            List<SubsubCategoryDT> ass = new List<SubsubCategoryDT>();
            using (var db = new AuthContext())
            {
                string query = "select b.SubsubCategoryid,b.SubsubcategoryName,b.LogoUrl,b.HindiName,b.Categoryid,b.SubCategoryId,b.SubcategoryName from ItemMasters a with(nolock) inner join SubsubCategories b with(nolock) on a.SubsubCategoryid=b.SubsubCategoryid " +
                              "and a.Deleted = 0  and a.active = 1  and (a.ItemAppType=0 or a.ItemAppType=2)  and a.WarehouseId = " + WarehouseId + "and b.Deleted =0 and b.IsActive =1 and a.DistributorShow = 1 group by b.SubsubCategoryid,b.SubsubcategoryName,b.LogoUrl,b.HindiName,b.Categoryid,b.SubCategoryId,b.SubcategoryName ";
                ass = db.Database.SqlQuery<SubsubCategoryDT>(query).ToList();
                return ass;
            }
        }

        [Route("UploadDistributorImage")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> UploadDistributorImage()
        {
            UploadImageResponse uploadImageResponse = new UploadImageResponse { status = false, Name = "" };

            using (AuthContext context = new AuthContext())
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        var fileuploadPath = HttpContext.Current.Server.MapPath("~/DistributorImages");

                        if (!Directory.Exists(fileuploadPath))
                        {
                            Directory.CreateDirectory(fileuploadPath);
                        }
                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DistributorImages"), filename);
                        httpPostedFile.SaveAs(ImageUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/DistributorImages", ImageUrl);
                        uploadImageResponse.Name = filename;
                        uploadImageResponse.status = true;

                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, uploadImageResponse);
            }
        }

        //[Route("SignupKPP1")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<DataContracts.KPPApp.customerDetails> SignupKPP(CustomerKPPDc CustomerKPPDc)
        //{
        //    var manager = new KPPAppManager();
        //    var cust = await manager.KppSignup(CustomerKPPDc);
        //    return cust;
        //}

        [Route("category/V2")]
        public async Task<customeritem> GetCategoryv2(int warehouseid, string lang)
        {
            var manager = new KPPAppManager();
            var customerInsert = await manager.getItemMasterv2(warehouseid, lang);
            return customerInsert;
        }


        [HttpGet]
        [Route("GetSearchItem")]
        public async Task<FilterSearchDcs> SearchCust(int warehouseId, int customerId, int skip, int take, string lang)
        {
            FilterSearchDcs filterSearchDc = new FilterSearchDcs();
            using (var authContext = new AuthContext())
            {
                var ActiveCustomer = authContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;

                var customerBrandIds = authContext.CustomerBrandAcessDB.Where(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.BrandId).ToList();

                try
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    var freeItemoffers = authContext.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == warehouseId && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems).SelectMany(x => x.OfferFreeItems).ToList();

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
                            .Translate<Itemsearchs>(reader).ToList();

                        MostSellingProduct = MostSellingProduct.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();
                        filterSearchDc.MostSellingProduct = MostSellingProduct;


                        foreach (var it in MostSellingProduct)
                        {
                            it.IsNotSell = true;
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                            it.marginPoint = 0;
                            if (customerBrandIds != null && customerBrandIds.Any())
                            {
                                if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                {
                                    it.IsNotSell = false;
                                }
                            }

                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion

                            #region old code for itemnumnber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.Number))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.Number);
                            //    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion

                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }

                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }
                        }
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
                            .Translate<Itemsearchs>(reader).ToList();

                        RecentPurchase = RecentPurchase.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();
                        filterSearchDc.RecentPurchase = RecentPurchase;

                        foreach (var it in RecentPurchase)
                        {
                            it.IsNotSell = true;
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                            it.marginPoint = 0;

                            #region old code for itemnumber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.Number))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.Number);
                            //    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion

                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion


                            if (customerBrandIds != null && customerBrandIds.Any())
                            {
                                if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                {
                                    it.IsNotSell = false;
                                }
                            }
                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }

                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }
                        }
                    });
                    taskList.Add(task2);

                    var task3 = Task.Factory.StartNew(() =>
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
                        var CustFavoriteItem = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<Itemsearchs>(reader).ToList();

                        CustFavoriteItem = CustFavoriteItem.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();

                        filterSearchDc.CustFavoriteItem = CustFavoriteItem;

                        foreach (var it in CustFavoriteItem)
                        {
                            it.IsNotSell = true;
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                            it.marginPoint = 0;

                            #region old code for itemnumber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.Number))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.Number);
                            //    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion

                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion


                            if (customerBrandIds != null && customerBrandIds.Any())
                            {
                                if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                {
                                    it.IsNotSell = false;
                                }
                            }
                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }

                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }
                        }

                    });
                    taskList.Add(task3);

                    var task4 = Task.Factory.StartNew(() =>
                    {
                        MongoDbHelper<CustomerProductSearch> mongoDbHelper = new MongoDbHelper<CustomerProductSearch>();
                        List<int> itemIds = mongoDbHelper.Select(x => x.customerId == customerId && !x.IsDeleted, x => x.OrderByDescending(y => y.CreatedDate), skip, take).ToList().SelectMany(x => x.Items).Distinct().ToList();
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
                        var recentSearchItem = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<Itemsearchs>(reader1).ToList();

                        recentSearchItem = recentSearchItem.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();

                        filterSearchDc.RecentSearchItem = recentSearchItem;

                        foreach (var it in filterSearchDc.RecentSearchItem)
                        {
                            it.IsNotSell = true;
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                            it.marginPoint = 0;

                            #region #region old code for itemnumber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.Number))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.Number);
                            //    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion

                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.Number && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (authContext.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion

                            if (customerBrandIds != null && customerBrandIds.Any())
                            {
                                if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                {
                                    it.IsNotSell = false;
                                }
                            }
                            if (lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }

                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }
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
            return filterSearchDc;
        }

        [Route("GetAllItemByBrand/V3")]
        [HttpGet]

        public async Task<WRSITEMN> getItemOnBrandv3(string lang, int customerid, int warehouseid, string brandName)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var customerBrandIds = context.CustomerBrandAcessDB.Where(x => x.CustomerId == customerid && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.BrandId).ToList();


                    WRSITEMN item = new WRSITEMN();

                    if (lang.Trim() == "hi")
                    {
                        //Increase some parameter For offer
                        var newdatahi = (from a in context.itemMasters
                                         where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.DistributorShow == true && a.SubsubcategoryName.Trim().ToLower() == brandName.Trim().ToLower() && (a.ItemAppType == 0 || a.ItemAppType == 2))
                                         join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                         let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                                         select new factoryItemdat
                                         {
                                             itemBaseName = b.itemBaseName,
                                             WarehouseId = a.WarehouseId,
                                             IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                             ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                             CompanyId = a.CompanyId,
                                             Categoryid = b.Categoryid,
                                             Discount = b.Discount,
                                             ItemId = a.ItemId,
                                             ItemNumber = b.Number,
                                             itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                             LogoUrl = b.LogoUrl,
                                             MinOrderQty = b.MinOrderQty,
                                             price = a.price,
                                             SubCategoryId = b.SubCategoryId,
                                             SubsubCategoryid = b.SubsubCategoryid,
                                             TotalTaxPercentage = b.TotalTaxPercentage,
                                             SellingUnitName = a.SellingUnitName,
                                             SellingSku = b.SellingSku,
                                             UnitPrice = a.UnitPrice,
                                             HindiName = a.HindiName,
                                             VATTax = b.VATTax,
                                             active = a.active,
                                             dreamPoint = 0,
                                             marginPoint = a.marginPoint,
                                             NetPurchasePrice = a.NetPurchasePrice,
                                             promoPerItems = a.promoPerItems,
                                             IsOffer = a.IsOffer,
                                             Deleted = a.Deleted,
                                             IsSensitive = b.IsSensitive,
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
                                             DistributionPrice = a.DistributionPrice,
                                             DistributorShow = a.DistributorShow,
                                             IsSensitiveMRP = a.IsSensitiveMRP,
                                             UnitofQuantity = a.UnitofQuantity,
                                             UOM = a.UOM,

                                         }).OrderByDescending(x => x.ItemNumber).ToList();

                        var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == warehouseid && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems).SelectMany(x => x.OfferFreeItems).ToList();


                        foreach (factoryItemdat it in newdatahi)
                        {
                            it.IsNotSell = true;
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                            #region old code for itemnumber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                            //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion

                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion

                            if (customerBrandIds != null && customerBrandIds.Any())
                            {
                                if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                {
                                    it.IsNotSell = false;
                                }
                            }
                            if (item.ItemMasters == null)
                            {
                                item.ItemMasters = new List<factoryItemdat>();
                            }

                            if (lang.Trim() == "hi")
                            {
                                if (!string.IsNullOrEmpty(it.HindiName))
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                                }
                            }

                            item.ItemMasters.Add(it);
                        }

                    }
                    else
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.DistributorShow == true && a.SubsubcategoryName.Trim().ToLower() == brandName.Trim().ToLower() && (a.ItemAppType == 0 || a.ItemAppType == 2))
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                       select new factoryItemdat
                                       {
                                           WarehouseId = a.WarehouseId,
                                           IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                           ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
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
                                           DistributionPrice = a.DistributionPrice,
                                           DistributorShow = a.DistributorShow
                                       }).OrderByDescending(x => x.ItemNumber).ToList();

                        var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == warehouseid && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems).SelectMany(x => x.OfferFreeItems).ToList();

                        foreach (factoryItemdat it in newdata)
                        {
                            it.IsNotSell = true;
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                            #region old code for itemnumber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                            //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion

                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion

                            if (customerBrandIds != null && customerBrandIds.Any())
                            {
                                if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                {
                                    it.IsNotSell = false;
                                }
                            }
                            if (item.ItemMasters == null)
                            {
                                item.ItemMasters = new List<factoryItemdat>();
                            }
                            item.ItemMasters.Add(it);
                        }
                    }
                    return new WRSITEMN() { ItemMasters = item.ItemMasters, Message = true };
                }
                catch (Exception ex)
                {
                    return new WRSITEMN() { ItemMasters = null, Message = false };
                }
            }
        }

        [Route("getItembycatesscatid")]
        [HttpGet]
        public async Task<WRSITEMS> getitembysscatid(string lang, int customerId, int catid, int scatid, int sscatid)
        {
            using (var context = new AuthContext())
            {
                List<WRSITEMS> brandItem = new List<WRSITEMS>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;
                List<factoryItemdatas> newdata = new List<factoryItemdatas>();
                var customerBrandIds = context.CustomerBrandAcessDB.Where(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.BrandId).ToList();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetDistributorItemByCatSubAndSubCat]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@sscatid", sscatid));
                cmd.Parameters.Add(new SqlParameter("@scatid", scatid));
                cmd.Parameters.Add(new SqlParameter("@catid", catid));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                using (var reader = cmd.ExecuteReader())
                {
                    newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<factoryItemdatas>(reader).ToList();

                    newdata = newdata.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();
                }
                WRSITEMS item = new WRSITEMS();
                var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == warehouseId && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems).SelectMany(x => x.OfferFreeItems).ToList();
                if (lang.Trim() == "hi")
                {

                    foreach (var it in newdata)
                    {
                        it.IsNotSell = true;
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;

                        #region old code for itemnumber
                        //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                        //{
                        //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                        //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                        //    {
                        //        it.IsOffer = true;
                        //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                        //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                        //        it.OfferId = freeItemoffer.offerId;
                        //        it.OfferType = freeItemoffer.OfferType;
                        //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                        //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                        //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                        //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                        //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                        //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                        //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                        //    }
                        //}
                        #endregion

                        #region code changes for sellingsku
                        if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                        {
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                {
                                    it.IsOffer = true;
                                    it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                    it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                    it.OfferId = freeItemoffer.offerId;
                                    it.OfferType = freeItemoffer.OfferType;
                                    it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                    it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                    //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                    it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                    it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                    it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                    it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                }
                            }
                            else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                {
                                    it.IsOffer = true;
                                    it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                    it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                    it.OfferId = freeItemoffer.offerId;
                                    it.OfferType = freeItemoffer.OfferType;
                                    it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                    it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                    //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                    it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                    it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                    it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                    it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                }
                            }
                        }
                        #endregion

                        if (customerBrandIds != null && customerBrandIds.Any())
                        {
                            if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                            {
                                it.IsNotSell = false;
                            }
                        }
                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdatas>();
                        }

                        if (it.HindiName != null)
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

                        item.ItemMasters.Add(it);
                    }
                }
                else
                {
                    foreach (var it in newdata)
                    {
                        it.IsNotSell = true;
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;

                        #region old code for itemnumber
                        //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                        //{
                        //    var freeItemoffr = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                        //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffr.FreeItemId && !x.Deleted))
                        //    {
                        //        it.IsOffer = true;
                        //        var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                        //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                        //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                        //        it.OfferId = freeItemoffer.offerId;
                        //        it.OfferType = freeItemoffer.OfferType;
                        //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                        //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                        //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                        //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                        //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                        //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                        //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                        //    }
                        //    else
                        //    {
                        //        it.IsOffer = false;
                        //        it.FlashDealSpecialPrice = 0;
                        //        it.OfferCategory = 0;
                        //    }
                        //}
                        //else
                        //{
                        //    it.IsOffer = false;
                        //    it.FlashDealSpecialPrice = 0;
                        //    it.OfferCategory = 0;
                        //}
                        #endregion

                        #region code changes for sellingsku
                        if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                        {
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                {
                                    it.IsOffer = true;
                                    it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                    it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                    it.OfferId = freeItemoffer.offerId;
                                    it.OfferType = freeItemoffer.OfferType;
                                    it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                    it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                    //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                    it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                    it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                    it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                    it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                }
                            }
                            else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                {
                                    it.IsOffer = true;
                                    it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                    it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                    it.OfferId = freeItemoffer.offerId;
                                    it.OfferType = freeItemoffer.OfferType;
                                    it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                    it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                    //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                    it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                    it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                    it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                    it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                }
                            }
                        }
                        #endregion

                        if (customerBrandIds != null && customerBrandIds.Any())
                        {
                            if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                            {
                                it.IsNotSell = false;
                            }
                        }

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdatas>();
                        }
                        item.ItemMasters.Add(it);
                    }
                }

                if (item.ItemMasters != null && item.ItemMasters.Any())
                {
                    item.Message = true;
                    return item;
                }
                else
                {
                    item.Message = false;
                    return item;
                }
            }
        }


        [Route("GetHomePageItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<WRSITEMS> GetHomePageItem(string lang, int customerId, int size)
        {
            using (var context = new AuthContext())
            {
                List<WRSITEMS> brandItem = new List<WRSITEMS>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;
                List<factoryItemdatas> newdata = new List<factoryItemdatas>();

                if (size > 0)
                {
                    newdata = (from a in context.itemMasters
                               where a.WarehouseId == warehouseId && a.DistributorShow == true
                               && a.Deleted == false
                               && a.active == true
                               && a.DistributorShow == true
                               && (a.ItemAppType == 0 || a.ItemAppType == 2)
                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                               select new factoryItemdatas
                               {
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
                                   NetPurchasePrice = a.NetPurchasePrice,
                                   IsOffer = a.IsOffer,
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
                                   DistributionPrice = a.DistributionPrice,
                                   DistributorShow = a.DistributorShow,
                                   UpdatedDate = a.UpdatedDate
                               }).OrderByDescending(x => x.UpdatedDate).Skip(0).Take(10).ToList();
                }
                else
                {
                    newdata = (from a in context.itemMasters
                               where a.WarehouseId == warehouseId && a.DistributorShow == true
                               && a.Deleted == false
                               && a.active == true
                               && a.DistributorShow == true
                               && (a.ItemAppType == 0 || a.ItemAppType == 2)
                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                               select new factoryItemdatas
                               {
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
                                   NetPurchasePrice = a.NetPurchasePrice,
                                   IsOffer = a.IsOffer,
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
                                   DistributionPrice = a.DistributionPrice,
                                   DistributorShow = a.DistributorShow,
                                   UpdatedDate = a.UpdatedDate
                               }).OrderByDescending(x => x.UpdatedDate).ToList();
                }

                WRSITEMS item = new WRSITEMS();

                if (lang.Trim() == "hi")
                {
                    var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();

                    foreach (var it in newdata)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdatas>();
                        }

                        if (it.HindiName != null)
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

                        item.ItemMasters.Add(it);
                    }
                }
                else
                {
                    var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();

                    foreach (var it in newdata)
                    {

                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdatas>();
                        }
                        item.ItemMasters.Add(it);
                    }
                }

                if (item.ItemMasters != null && item.ItemMasters.Any())
                {
                    item.Message = true;
                    return item;
                }
                else
                {
                    item.Message = false;
                    return item;
                }
            }
        }



        [HttpPost]
        [Route("GetRelatedItem")]
        public async Task<List<Itemsearchs>> GetRelatedItem(RelatedItem ri)
        {
            var ItemSearch = new List<Itemsearchs>();
            using (var authContext = new AuthContext())
            {
                var ActiveCustomer = authContext.Customers.FirstOrDefault(x => x.CustomerId == ri.customerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;


                try
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

                    try
                    {

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
                        var RelatedItem = ((IObjectContextAdapter)authContext)
                        .ObjectContext
                        .Translate<Itemsearchs>(reader).ToList();

                        RelatedItem = RelatedItem.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();

                        ItemSearch = RelatedItem;
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                    foreach (var it in ItemSearch)
                    {

                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                        it.marginPoint = 0;

                        if (ri.lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }

                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                    }
                }
                finally
                {
                    authContext.Database.Connection.Close(); ;
                }
            }
            return ItemSearch;
        }

        [Route("V4")]
        [HttpPost]
        public async Task<WRSITEMN> postitembyitemnamev4(SearchItem searchitem)
        {
            using (var db = new AuthContext())
            {
                if (!string.IsNullOrEmpty(searchitem.BarCode))
                {
                    var Number = db.ItemBarcodes.FirstOrDefault(i => i.Barcode == searchitem.BarCode && i.IsDeleted == false && i.IsActive == true).ItemNumber;
                    if (!string.IsNullOrEmpty(Number))
                    {
                        searchitem.Number = Number;
                    }
                }

                WRSITEMN item = new WRSITEMN();

                List<ItemMaster> itemList = new List<ItemMaster>();
                var ActiveCustomer = db.Customers.FirstOrDefault(x => x.CustomerId == searchitem.CustomerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;
                var customerBrandIds = db.CustomerBrandAcessDB.Where(x => x.CustomerId == searchitem.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.BrandId).ToList();

                var newdata = (from a in db.itemMasters
                               where a.WarehouseId == warehouseId && a.DistributorShow == true
                               && (string.IsNullOrEmpty(searchitem.UOM) || a.UOM == searchitem.UOM)
                               && (string.IsNullOrEmpty(searchitem.Unit) || a.UnitofQuantity == searchitem.Unit)
                               && (searchitem.Category.Count == 0 || searchitem.Category.Contains(a.Categoryid))
                               && (searchitem.BaseCat.Count == 0 || searchitem.BaseCat.Contains(a.BaseCategoryid))
                               && (searchitem.SubCat.Count == 0 || searchitem.SubCat.Contains(a.SubCategoryId))
                               && (searchitem.Brand.Count == 0 || searchitem.Brand.Contains(a.SubsubCategoryid))
                               && a.Deleted == false
                               && a.active == true
                               && a.DistributorShow == true
                               && (a.ItemAppType == 0 || a.ItemAppType == 2)
                               && a.DistributionPrice >= searchitem.minPrice
                               && a.DistributionPrice <= searchitem.maxPrice
                               && (a.itemname.Trim().ToLower().Contains(searchitem.itemkeyword.Trim().ToLower()) || (a.Number.Trim().ToLower().Contains(searchitem.Number.Trim().ToLower())))
                               //  join b in db.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                               let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                               select new factoryItemdat
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
                                   DistributionPrice = a.DistributionPrice,
                                   DistributorShow = a.DistributorShow,
                                   ItemMultiMRPId = a.ItemMultiMRPId
                               }).OrderByDescending(x => x.ItemNumber).ToList();

                //var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                //var activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();
                var freeItemoffers = db.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == warehouseId && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems).SelectMany(x => x.OfferFreeItems).ToList();
                foreach (var it in newdata)
                {
                    it.IsNotSell = true;
                    it.IsOffer = false;
                    it.FlashDealSpecialPrice = 0;
                    it.OfferCategory = 0;
                    #region code changes for sellingsku
                    if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                    {
                        if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                        {
                            var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                            if (db.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            {
                                it.IsOffer = true;
                                it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                it.OfferId = freeItemoffer.offerId;
                                it.OfferType = freeItemoffer.OfferType;
                                it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            }
                        }
                        else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                        {
                            var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                            if (db.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            {
                                it.IsOffer = true;
                                it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                it.OfferId = freeItemoffer.offerId;
                                it.OfferType = freeItemoffer.OfferType;
                                it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            }
                        }
                    }
                    #endregion


                    #region oldcode for itemnumber
                    //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                    //{
                    //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                    //    if (db.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                    //    {
                    //        it.IsOffer = true;
                    //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                    //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                    //        it.OfferId = freeItemoffer.offerId;
                    //        it.OfferType = freeItemoffer.OfferType;
                    //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                    //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                    //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                    //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                    //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                    //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                    //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                    //    }
                    //}
                    #endregion
                    if (customerBrandIds != null && customerBrandIds.Any())
                    {
                        if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                        {
                            it.IsNotSell = false;
                        }
                    }

                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<factoryItemdat>();
                    }

                    //// by sudhir 22-08-2019
                    if (searchitem.lang.Trim() == "hi")
                    {
                        if (it.HindiName != null)
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

                    item.ItemMasters.Add(it);
                }
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
                    item.Message = true;
                    return item;
                }
                else
                {
                    item.Message = false;
                    return item;
                }
            }
        }


        [Route("InsertKpp1")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<Customer> InsertKPP(string MobileNumber)
        {
            var manager = new KPPAppManager();
            var customerInsert = await manager.InsertKPP(MobileNumber);
            return customerInsert;
        }

        [Route("ForgrtPasword")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<DataContracts.KPPApp.customerDetails> ForgrtPasword(string Mobile)
        {
            DataContracts.KPPApp.customerDetails cd;
            Customer customer = new Customer();
            using (AuthContext db = new AuthContext())
            {
                customer = db.Customers.Where(x => x.Mobile == Mobile).FirstOrDefault();
                if (customer != null)
                {
                    string msg = "";//"Hi You Recently requested a forget password on ShopKirana. Your account Password is {#var#} If you didn't request then ingore this messageThanks Shopkirana.com";
                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Customer_Forget_Password");
                    msg = dltSMS == null ? "" : dltSMS.Template;
                    msg = msg.Replace("{#var#}", customer.Password.ToString());

                    if (dltSMS != null)
                        new Sms().sendOtp(customer.Mobile, msg, dltSMS.DLTId);

                    cd = new DataContracts.KPPApp.customerDetails()
                    {
                        customers = Mapper.Map(customer).ToANew<customers>(),
                        Status = true,
                        Message = "Message send to your registered mobile number."
                    };
                    return cd;
                }
                else
                {
                    cd = new DataContracts.KPPApp.customerDetails()
                    {
                        customers = null,
                        Status = false,
                        Message = "Distributer not exist."
                    };
                    return cd;
                }
            }
        }

        [Route("KPPUpdate")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<Customer> KPPUpdate(CustomerUpdateDC Cust)
        {
            var manager = new KPPAppManager();
            var KppUpdate = await manager.KPPUpdate(Cust);
            return KppUpdate;
        }



        //[Route("loginKPP1")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<DataContracts.KPPApp.customerDetails> loginKPP(CustomerKPPDc CustomerUpdateDC)
        //{
        //    try
        //    {
        //        var manager = new KPPAppManager();
        //        var loginKPP = await manager.loginKpp(CustomerUpdateDC);
        //        return loginKPP;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}


        //[Route("changepassword")]
        //[AcceptVerbs("PUT")]
        //public async Task<DataContracts.KPPApp.customerDetails> Changepassword(pwcdetail item)
        //{
        //    var manager = new KPPAppManager();
        //    var loginKPP = await manager.Changepassword(item);
        //    return loginKPP;
        //}



        #region ShoppingCart
        [Route("AddItem")]
        [HttpPost]
        public async Task<ReturnShoppingCart> InsertCartItem(CartItemDc cartItemDc)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            using (var context = new AuthContext())
            {
                MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
                var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == cartItemDc.CustomerId && x.WarehouseId == cartItemDc.WarehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                var customerShoppingCart = mongoDbHelper.Select(cartPredicate).FirstOrDefault();
                //var itemmaster = await context.itemMasters.FirstOrDefaultAsync(x => x.ItemId == cartItemDc.ItemId);
                if (customerShoppingCart != null)
                {
                    if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == cartItemDc.ItemId && x.IsFreeItem == cartItemDc.IsFreeItem))
                    {
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsActive = true;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).qty = cartItemDc.qty;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).UnitPrice = cartItemDc.UnitPrice;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsDeleted = false;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).ModifiedDate = DateTime.Now;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).ModifiedBy = cartItemDc.CustomerId;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsFreeItem = cartItemDc.IsFreeItem;
                    }
                    else
                    {
                        customerShoppingCart.ShoppingCartItems.Add(new ShoppingCartItem
                        {
                            CreatedBy = cartItemDc.CustomerId,
                            CreatedDate = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            IsFreeItem = cartItemDc.IsFreeItem,
                            ModifiedBy = cartItemDc.CustomerId,
                            ItemId = cartItemDc.ItemId,
                            qty = cartItemDc.qty,
                            UnitPrice = cartItemDc.UnitPrice,
                            TaxAmount = 0
                            //TaxPercentage = itemmaster.TotalTaxPercentage,
                            //TaxAmount = itemmaster.TotalTaxPercentage > 0 ? (cartItemDc.qty * cartItemDc.UnitPrice) * itemmaster.TotalTaxPercentage / 100 : 0
                        });
                    }
                    returnShoppingCart.Status = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart);
                }
                else
                {
                    customerShoppingCart = new CustomerShoppingCart
                    {
                        IsActive = true,
                        CartTotalAmt = 0,
                        CreatedBy = cartItemDc.CustomerId,
                        CreatedDate = DateTime.Now,
                        CustomerId = cartItemDc.CustomerId,
                        DeamPoint = 0,
                        DeliveryCharges = 0,
                        GrossTotalAmt = 0,
                        IsDeleted = false,
                        TotalDiscountAmt = 0,
                        TotalTaxAmount = 0,
                        WalletAmount = 0,
                        WarehouseId = cartItemDc.WarehouseId,
                        ShoppingCartItems = new List<ShoppingCartItem> {
                     new ShoppingCartItem {
                         CreatedBy = cartItemDc.CustomerId,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        IsFreeItem = cartItemDc.IsFreeItem,
                        ModifiedBy = cartItemDc.CustomerId,
                        ItemId = cartItemDc.ItemId,
                        qty = cartItemDc.qty,
                        UnitPrice = cartItemDc.UnitPrice,
                        //TaxPercentage = itemmaster.TotalTaxPercentage,
                       TaxAmount= 0//itemmaster.TotalTaxPercentage>0? (cartItemDc.qty * cartItemDc.UnitPrice) * itemmaster.TotalTaxPercentage/100:0
                     }
                    }
                    };
                    returnShoppingCart.Status = await mongoDbHelper.InsertAsync(customerShoppingCart);
                }
                if (cartItemDc.IsCartRequire)
                {
                    customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, cartItemDc.lang);
                }
                else
                {
                    customerShoppingCartDc = null;
                    //BackgroundJob.Enqueue(() => RefereshCartSync(customerShoppingCart, cartItemDc.lang));
                }
            }
            returnShoppingCart.Cart = customerShoppingCartDc;

            return returnShoppingCart;
        }

        private async Task<CustomerShoppingCartDc> RefereshCart(CustomerShoppingCart customerShoppingCart, AuthContext context, string lang)
        {
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            List<ShoppingCartItemDc> ShoppingCartItemDcs = new List<ShoppingCartItemDc>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            int walletPoint = 0;
            List<int> FreeItemOfferIds = new List<int>();
            if (customerShoppingCart != null)
            {
                customerShoppingCartDc = new CustomerShoppingCartDc
                {
                    CartTotalAmt = 0,
                    CustomerId = customerShoppingCart.CustomerId,
                    DeamPoint = 0,
                    DeliveryCharges = 0,
                    GrossTotalAmt = 0,
                    TotalDiscountAmt = 0,
                    TotalTaxAmount = 0,
                    WalletAmount = 0,
                    WarehouseId = customerShoppingCart.WarehouseId,
                };
                if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any())
                {
                    var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == customerShoppingCart.WarehouseId && x.isDeleted == false && x.IsDistributor && x.IsActive).ToList();
                    var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerShoppingCart.CustomerId);
                    var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                    List<int> itemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();
                    //string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + customerShoppingCart.WarehouseId +
                    //                  " WHERE a.[CustomerId]=" + customerShoppingCart.CustomerId;
                    //var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();



                    var orderIdDt = new DataTable();
                    orderIdDt.Columns.Add("IntValue");
                    foreach (var item in itemids)
                    {
                        var dr = orderIdDt.NewRow();
                        dr["IntValue"] = item;
                        orderIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("ItemIds", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetShoppingCardItem]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    var newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<factoryItemdata>(reader).ToList();

                    newdata = newdata.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();

                    //var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    //var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted).Select(x => x.OfferId).ToList() : new List<int>();
                    List<int> Itemids = newdata.Select(x => x.ItemId).ToList();
                    var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.WarehouseId == ActiveCustomer.Warehouseid && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == customerShoppingCart.WarehouseId && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems)
                        .SelectMany(x =>
                    x.OfferFreeItems.Select(y => new
                    {
                        y.FreeItemId,
                        y.Id,
                        y.ItemNumber,
                        y.OfferFreeItemImage,
                        y.OfferFreeItemName,
                        y.OfferFreeItemQuantity,
                        y.offerId,
                        y.OfferMinimumQty,
                        y.OfferQtyAvaiable,
                        y.OfferQtyConsumed,
                        y.OfferType,
                        y.OfferWalletPoint,
                        y.Offer.itemId,
                        y.Offer.QtyAvaiable,
                        y.OfferOn,
                        y.ItemMultiMRPId
                    })).ToList();


                    foreach (var it in newdata)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;

                        #region old code for itemnumber
                        //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                        //{
                        //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                        //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                        //    {
                        //        it.IsOffer = true;
                        //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                        //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                        //        it.OfferId = freeItemoffer.offerId;
                        //        it.OfferType = freeItemoffer.OfferType;
                        //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                        //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                        //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                        //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                        //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                        //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                        //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                        //    }
                        //}
                        #endregion

                        #region code changes for sellingsku
                        if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                        {
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                {
                                    it.IsOffer = true;
                                    it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                    it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                    it.OfferId = freeItemoffer.offerId;
                                    it.OfferType = freeItemoffer.OfferType;
                                    it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                    it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                    //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                    it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                    it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                    it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                    it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                }
                            }
                            else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                {
                                    it.IsOffer = true;
                                    it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                    it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                    it.OfferId = freeItemoffer.offerId;
                                    it.OfferType = freeItemoffer.OfferType;
                                    it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                    it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                    //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                    it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                    it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                    it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                    it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                }
                            }
                        }
                        #endregion


                        if (it.IsOffer)
                        {
                            if (it.OfferType == "WalletPoint" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferWalletPoint.HasValue && it.OfferWalletPoint.Value > 0)
                            {
                                var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                if (item.qty >= it.OfferMinimumQty)
                                {
                                    var FreeWalletPoint = it.OfferWalletPoint.Value;
                                    int calfreeItemQty = item.qty / it.OfferMinimumQty.Value;
                                    FreeWalletPoint *= calfreeItemQty;
                                    item.TotalFreeWalletPoint = FreeWalletPoint;
                                    walletPoint += Convert.ToInt32(FreeWalletPoint);
                                }

                            }
                            else if (it.OfferType == "ItemMaster" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferFreeItemQuantity.HasValue && it.OfferFreeItemQuantity.Value > 0)
                            {
                                var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                if (item.qty >= it.OfferMinimumQty)
                                {
                                    var FreeItemQuantity = it.OfferFreeItemQuantity.Value;
                                    int calfreeItemQty = Convert.ToInt32(item.qty / it.OfferMinimumQty);
                                    FreeItemQuantity *= calfreeItemQty;
                                    if (FreeItemQuantity > 0)
                                    {
                                        item.FreeItemqty = FreeItemQuantity;
                                    }
                                }
                                else
                                    item.FreeItemqty = 0;
                            }
                        }
                        if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
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

                    CustomersManager manager = new CustomersManager();
                    List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(customerShoppingCart.CustomerId, "Distributor App");
                    if (billDiscountOfferDcs.Any())
                    {
                        // List<int> applyedOfferIds = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList();

                        //billDiscountOfferDcs = billDiscountOfferDcs.Where(x => applyedOfferIds.Contains(x.OfferId) && !(x.OfferOn == "Slab" || x.OfferOn == "ItemPost")).ToList();
                        billDiscountOfferDcs = billDiscountOfferDcs.Where(x => !(x.OfferOn == "Slab" || x.OfferOn == "ItemPost")).ToList();
                        int MaxSingleUsedOfferId = 0;
                        double MaxDiscountAmount = 0;
                        bool IsUsedWithOfferApply = false;
                        foreach (var Offer in billDiscountOfferDcs)
                        {
                            ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                            double totalamount = 0;
                            double BillDiscountamount = 0;
                            var OrderLineItems = 0;
                            if ((Offer.OfferOn == "BillDiscount" || Offer.OfferOn == "ScratchBillDiscount") && Offer.OfferOn != "ItemMarkDown")
                            {

                                if (Offer.BillDiscountType == "category" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                    var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                    Itemids = newdata.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                        {
                                            int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                            if (ItemId == 0)
                                            {
                                                totalamount = 0;
                                                break;
                                            }
                                            else
                                                lineItemValueItemExists.Add(ItemId);
                                        }
                                    }

                                }
                                else if (Offer.BillDiscountType == "subcategory" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                    Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                        {
                                            int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                            if (ItemId == 0)
                                            {
                                                totalamount = 0;
                                                break;
                                            }
                                            else
                                                lineItemValueItemExists.Add(ItemId);
                                        }
                                    }
                                }
                                else if (Offer.BillDiscountType == "brand" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();

                                    Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                        {
                                            int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                            if (ItemId == 0)
                                            {
                                                totalamount = 0;
                                                break;
                                            }
                                            else
                                                lineItemValueItemExists.Add(ItemId);
                                        }
                                    }
                                }
                                else if (Offer.BillDiscountType == "items")
                                {
                                    var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                    if (Offer.OfferItems.FirstOrDefault().IsInclude)
                                    {
                                        Itemids = newdata.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    }
                                    var incluseItemIds = newdata.Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                        {
                                            int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                            if (ItemId == 0)
                                            {
                                                totalamount = 0;
                                                break;
                                            }
                                            else
                                                lineItemValueItemExists.Add(ItemId);
                                        }
                                    }
                                }
                                else
                                {
                                    var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                    Itemids = newdata.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();
                                    var incluseItemIds = newdata.Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).ToList();
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                        {
                                            int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                            if (ItemId == 0)
                                            {
                                                totalamount = 0;
                                                break;
                                            }
                                            else
                                                lineItemValueItemExists.Add(ItemId);
                                        }
                                    }
                                }

                                if (Offer.BillDiscountRequiredItems != null && Offer.BillDiscountRequiredItems.Any())
                                {
                                    bool IsRequiredItemExists = true;
                                    var objectIds = Offer.BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                                    if (Offer.BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                                    {
                                        objectIds.AddRange(newdata.Where(x => Offer.BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                                    }
                                    var cartrequiredItems = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                    foreach (var reqitem in Offer.BillDiscountRequiredItems)
                                    {
                                        if (reqitem.ObjectType == "Item")
                                        {
                                            var reqobjectids = reqitem.ObjectId.Split(',').Select(z => Convert.ToInt32(z)).ToList();
                                            var cartitem = cartrequiredItems.Where(x => reqobjectids.Contains(x.ItemMultiMRPId));
                                            if (cartitem != null && cartitem.Any())
                                            {
                                                if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitem.Sum(x => x.qty))
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                                else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitem.Sum(x => x.qty * x.UnitPrice))
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                IsRequiredItemExists = false;
                                                break;
                                            }
                                        }
                                        else if (reqitem.ObjectType == "brand")
                                        {
                                            var reqobjectids = reqitem.ObjectId.Split(',').Select(z => z).ToList();
                                            var multiMrpIds = newdata.Where(x => reqobjectids.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
                                            var cartitems = cartrequiredItems.Where(x => multiMrpIds.Contains(x.ItemMultiMRPId));
                                            if (cartitems != null && cartitems.Any())
                                            {
                                                if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitems.Sum(x => x.qty))
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                                else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitems.Sum(x => x.qty * x.UnitPrice))
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                IsRequiredItemExists = false;
                                                break;
                                            }

                                        }
                                    }
                                    if (!IsRequiredItemExists)
                                    {
                                        totalamount = 0;
                                    }
                                }

                                if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                                {
                                    totalamount = Offer.MaxBillAmount;
                                }
                                else if (Offer.BillAmount > totalamount)
                                {
                                    totalamount = 0;
                                }

                                if (Offer.LineItem > 0 && Offer.LineItem > OrderLineItems)
                                {
                                    totalamount = 0;
                                }

                                if (!Offer.IsScratchBDCode && Offer.OfferOn == "ScratchBillDiscount")
                                    totalamount = 0;
                            }
                            else if (Offer.OfferOn == "ItemMarkDown")
                            {
                                Itemids = new List<int>();
                                if (Offer.BillDiscountType == "category" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                    var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                    Itemids = newdata.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                    BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                }
                                else if (Offer.BillDiscountType == "subcategory" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                    Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                    BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                }
                                else if (Offer.BillDiscountType == "brand" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();

                                    Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                    BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                }
                            }
                            //else
                            //{
                            //    totalamount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice);

                            //    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                            //    {
                            //        totalamount = Offer.MaxBillAmount;
                            //    }
                            //    else if (Offer.BillAmount > totalamount)
                            //    {
                            //        totalamount = 0;
                            //    }

                            //    if (!Offer.IsScratchBDCode)
                            //        totalamount = 0;
                            //}

                            if (Offer.OfferOn != "ItemMarkDown")
                            {
                                if (totalamount > 0)
                                {
                                    if (Offer.BillDiscountOfferOn == "Percentage")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                    }
                                    else
                                    {
                                        int WalletPoint = 0;
                                        if (Offer.WalletType == "WalletPercentage")
                                        {
                                            WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * (Offer.BillDiscountWallet ?? 0) / 100));
                                            WalletPoint = WalletPoint * 10;
                                        }
                                        else
                                        {
                                            WalletPoint = Convert.ToInt32(Offer.BillDiscountWallet ?? 0);
                                        }
                                        if (Offer.ApplyOn == "PostOffer")
                                        {
                                            ShoppingCartDiscount.DiscountAmount = 0;
                                            ShoppingCartDiscount.NewBillingWalletPoint = WalletPoint;
                                        }
                                        else
                                        {
                                            ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10); ;
                                            ShoppingCartDiscount.NewBillingWalletPoint = 0;
                                        }
                                    }
                                    if (Offer.MaxDiscount > 0)
                                    {
                                        var walletmultipler = 1;

                                        if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage" && Offer.BillDiscountOfferOn != "DynamicAmount")
                                        {
                                            walletmultipler = 10;
                                        }
                                        if (Offer.BillDiscountOfferOn != "DynamicAmount")
                                        {
                                            if (Offer.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                            {
                                                ShoppingCartDiscount.DiscountAmount = Offer.MaxDiscount * walletmultipler;
                                            }
                                            if (Offer.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint)
                                            {
                                                ShoppingCartDiscount.NewBillingWalletPoint = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                                            }
                                        }
                                    }

                                    if (ShoppingCartDiscount.NewBillingWalletPoint > 0)
                                    {
                                        ShoppingCartDiscount.DiscountAmount = ShoppingCartDiscount.NewBillingWalletPoint / 10;
                                    }

                                    if (ShoppingCartDiscount.DiscountAmount > 0 && Offer.IsUseOtherOffer)
                                    {
                                        IsUsedWithOfferApply = true;
                                    }

                                    if (MaxDiscountAmount < ShoppingCartDiscount.DiscountAmount)
                                    {
                                        MaxDiscountAmount = ShoppingCartDiscount.DiscountAmount;
                                        MaxSingleUsedOfferId = Offer.OfferId;
                                    }
                                    if (IsUsedWithOfferApply)
                                        MaxSingleUsedOfferId = 0;
                                }
                            }
                            else if (Offer.OfferOn == "ItemMarkDown" && BillDiscountamount > 0)
                            {
                                ShoppingCartDiscount.DiscountAmount = BillDiscountamount;
                            }
                        }


                        foreach (var Offer1 in billDiscountOfferDcs.OrderByDescending(x => x.IsUseOtherOffer).ToList())
                        {
                            ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                            double totalamount = 0;
                            double BillDiscountamount = 0;
                            var OrderLineItems = 0;
                            if ((Offer1.OfferOn == "BillDiscount" || Offer1.OfferOn == "ScratchBillDiscount") && Offer1.OfferOn != "ItemMarkDown")
                            {
                                if (Offer1.BillDiscountType == "category" && Offer1.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer1.OfferItems.Select(x => x.itemId).ToList();
                                    var ids = Offer1.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                    Itemids = newdata.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                }
                                else if (Offer1.BillDiscountType == "subcategory" && Offer1.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer1.OfferItems.Select(x => x.itemId).ToList();
                                    Itemids = newdata.Where(x => Offer1.OfferBillDiscountItems.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                }
                                else if (Offer1.BillDiscountType == "brand" && Offer1.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer1.OfferItems.Select(x => x.itemId).ToList();

                                    Itemids = newdata.Where(x => Offer1.OfferBillDiscountItems.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                }
                                else if (Offer1.BillDiscountType == "items")
                                {
                                    var iteminofferlist = Offer1.OfferItems.Select(x => x.itemId).ToList();
                                    if (Offer1.OfferItems.FirstOrDefault().IsInclude)
                                    {
                                        Itemids = newdata.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    }
                                    var incluseItemIds = newdata.Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                }
                                else
                                {
                                    var ids = Offer1.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                    Itemids = newdata.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();
                                    var incluseItemIds = newdata.Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                }

                                if (Offer1.BillDiscountRequiredItems != null && Offer1.BillDiscountRequiredItems.Any())
                                {
                                    bool IsRequiredItemExists = true;
                                    var objectIds = Offer1.BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                                    if (Offer1.BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                                    {
                                        objectIds.AddRange(newdata.Where(x => Offer1.BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                                    }
                                    var cartrequiredItems = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                    foreach (var reqitem in Offer1.BillDiscountRequiredItems)
                                    {
                                        if (reqitem.ObjectType == "Item")
                                        {
                                            var mrpIds = reqitem.ObjectId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                                            var cartitem = cartrequiredItems.Where(x => mrpIds.Contains(x.ItemMultiMRPId));
                                            if (cartitem != null && cartitem.Any())
                                            {
                                                if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitem.Sum(x => x.qty))
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                                else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitem.Sum(x => x.qty * x.UnitPrice))
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                IsRequiredItemExists = false;
                                                break;
                                            }
                                        }
                                        else if (reqitem.ObjectType == "brand")
                                        {
                                            var objIds = reqitem.ObjectId.Split(',').Select(x => x).ToList();
                                            var multiMrpIds = newdata.Where(x => objIds.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
                                            var cartitems = cartrequiredItems.Where(x => multiMrpIds.Contains(x.ItemMultiMRPId));
                                            if (cartitems != null && cartitems.Any())
                                            {
                                                if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitems.Sum(x => x.qty))
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                                else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitems.Sum(x => x.qty * x.UnitPrice))
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                IsRequiredItemExists = false;
                                                break;
                                            }

                                        }
                                    }
                                    if (!IsRequiredItemExists)
                                    {
                                        totalamount = 0;
                                    }
                                }
                                if (Offer1.MaxBillAmount > 0 && totalamount > Offer1.MaxBillAmount)
                                {
                                    totalamount = Offer1.MaxBillAmount;
                                }
                                else if (Offer1.BillAmount > totalamount)
                                {
                                    totalamount = 0;
                                }

                                if (Offer1.LineItem > 0 && Offer1.LineItem > OrderLineItems)
                                {
                                    totalamount = 0;
                                }

                                if (!Offer1.IsScratchBDCode && Offer1.OfferOn == "ScratchBillDiscount")
                                    totalamount = 0;
                            }
                            else if (Offer1.OfferOn == "ItemMarkDown")
                            {
                                Itemids = new List<int>();
                                if (Offer1.BillDiscountType == "category" && Offer1.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer1.OfferItems.Select(x => x.itemId).ToList();
                                    var ids = Offer1.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                    Itemids = newdata.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer1.DistributorDiscountPercentage)) / 100))) : 0;
                                    BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                }
                                else if (Offer1.BillDiscountType == "subcategory" && Offer1.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer1.OfferItems.Select(x => x.itemId).ToList();
                                    Itemids = newdata.Where(x => Offer1.OfferBillDiscountItems.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer1.DistributorDiscountPercentage)) / 100))) : 0;
                                    BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                }
                                else if (Offer1.BillDiscountType == "brand" && Offer1.OfferBillDiscountItems.Any())
                                {
                                    var iteminofferlist = Offer1.OfferItems.Select(x => x.itemId).ToList();

                                    Itemids = newdata.Where(x => Offer1.OfferBillDiscountItems.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer1.DistributorDiscountPercentage)) / 100))) : 0;
                                    BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                }
                            }
                            //else
                            //{
                            //    totalamount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice);

                            //    if (Offer1.MaxBillAmount > 0 && totalamount > Offer1.MaxBillAmount)
                            //    {
                            //        totalamount = Offer1.MaxBillAmount;
                            //    }
                            //    else if (Offer1.BillAmount > totalamount)
                            //    {
                            //        totalamount = 0;
                            //    }

                            //    if (!Offer1.IsScratchBDCode)
                            //        totalamount = 0;
                            //}


                            bool IsUsed = true;
                            if (!Offer1.IsUseOtherOffer && (ShoppingCartDiscounts.Any() || MaxSingleUsedOfferId != Offer1.OfferId))
                                IsUsed = false;


                            if (IsUsed && totalamount > 0)
                            {
                                if (Offer1.OfferOn != "ItemMarkDown")
                                {
                                    if (Offer1.BillDiscountOfferOn == "Percentage")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = totalamount * Offer1.DiscountPercentage / 100;
                                    }
                                    else if (Offer1.BillDiscountOfferOn == "FreeItem")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = 0;
                                    }
                                    else
                                    {
                                        int WalletPoint = 0;
                                        if (Offer1.WalletType == "WalletPercentage")
                                        {
                                            WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * (Offer1.BillDiscountWallet ?? 0) / 100));
                                            WalletPoint = WalletPoint * 10;
                                        }
                                        else
                                        {
                                            WalletPoint = Convert.ToInt32(Offer1.BillDiscountWallet ?? 0);
                                        }
                                        if (Offer1.ApplyOn == "PostOffer")
                                        {
                                            ShoppingCartDiscount.DiscountAmount = 0;
                                            ShoppingCartDiscount.NewBillingWalletPoint = WalletPoint;
                                        }
                                        else
                                        {
                                            ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10); ;
                                            ShoppingCartDiscount.NewBillingWalletPoint = 0;
                                        }
                                    }
                                    if (Offer1.MaxDiscount > 0)
                                    {
                                        var walletmultipler = 1;

                                        if (!string.IsNullOrEmpty(Offer1.BillDiscountOfferOn) && Offer1.BillDiscountOfferOn != "Percentage")
                                        {
                                            walletmultipler = 10;
                                        }
                                        if (Offer1.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                        {
                                            ShoppingCartDiscount.DiscountAmount = Offer1.MaxDiscount * walletmultipler;
                                        }
                                        if (Offer1.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint)
                                        {
                                            ShoppingCartDiscount.NewBillingWalletPoint = Convert.ToInt32(Offer1.MaxDiscount * walletmultipler);
                                        }
                                    }

                                    ShoppingCartDiscount.OfferId = Offer1.OfferId;
                                    ShoppingCartDiscount.CreatedDate = DateTime.Now;
                                    ShoppingCartDiscount.IsActive = Offer1.IsActive;
                                    ShoppingCartDiscount.IsDeleted = false;
                                    ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                                }
                                else if (Offer1.OfferOn == "ItemMarkDown" && BillDiscountamount > 0)
                                {
                                    ShoppingCartDiscount.DiscountAmount = BillDiscountamount;
                                    ShoppingCartDiscount.OfferId = Offer1.OfferId;
                                    ShoppingCartDiscount.CreatedDate = DateTime.Now;
                                    ShoppingCartDiscount.IsActive = Offer1.IsActive;
                                    ShoppingCartDiscount.IsDeleted = false;
                                    ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                                }
                            }
                        }
                    }
                    customerShoppingCart.ShoppingCartDiscounts = ShoppingCartDiscounts;


                    customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList().ForEach(x =>
                    {
                        if (newdata.Any(y => y.ItemId == x.ItemId))
                        {

                            x.ItemMultiMRPId = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemMultiMRPId;
                            x.ItemNumber = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemNumber;
                            x.ItemName = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).itemname;
                        }
                        var itemActive = newdata.FirstOrDefault(y => y.ItemId == x.ItemId);
                        if (itemActive != null)
                        {
                            x.IsActive = itemActive.active;
                            x.IsDeleted = !itemActive.active;
                        }
                        else
                        {
                            x.IsActive = false;
                            x.IsDeleted = true;
                        }
                    });
                    ShoppingCartItemDcs = newdata.Select(a => new ShoppingCartItemDc
                    {
                        ItemMultiMRPId = a.ItemMultiMRPId,
                        BaseCategoryId = a.BaseCategoryId,
                        IsItemLimit = a.IsItemLimit,
                        ItemlimitQty = a.ItemlimitQty,
                        BillLimitQty = a.BillLimitQty,
                        WarehouseId = a.WarehouseId,
                        CompanyId = a.CompanyId,
                        Categoryid = a.Categoryid,
                        Discount = a.Discount,
                        ItemId = a.ItemId,
                        ItemNumber = a.ItemNumber,
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
                        UnitPrice = a.DistributionPrice ?? 0,
                        VATTax = a.VATTax,
                        itemBaseName = a.itemBaseName,
                        active = a.active,
                        marginPoint = a.marginPoint.HasValue ? a.marginPoint.Value : 0,
                        promoPerItems = a.promoPerItems.HasValue ? a.promoPerItems.Value : 0,
                        NetPurchasePrice = a.NetPurchasePrice,
                        IsOffer = a.IsOffer,
                        Deleted = a.Deleted,
                        ItemAppType = a.ItemAppType,
                        OfferCategory = a.OfferCategory.HasValue ? a.OfferCategory.Value : 0,
                        OfferStartTime = a.OfferStartTime,
                        OfferEndTime = a.OfferEndTime,
                        OfferQtyAvaiable = a.OfferQtyAvaiable.HasValue ? a.OfferQtyAvaiable.Value : 0,
                        OfferQtyConsumed = a.OfferQtyConsumed.HasValue ? a.OfferQtyConsumed.Value : 0,
                        OfferId = a.OfferId.HasValue ? a.OfferId.Value : 0,
                        OfferType = a.OfferType,
                        dreamPoint = a.dreamPoint.HasValue ? a.dreamPoint.Value : 0,
                        OfferWalletPoint = a.OfferWalletPoint.HasValue ? a.OfferWalletPoint.Value : 0,
                        OfferFreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                        OfferPercentage = a.OfferPercentage.HasValue ? a.OfferPercentage.Value : 0,
                        OfferFreeItemName = a.OfferFreeItemName,
                        OfferFreeItemImage = a.OfferFreeItemImage,
                        DistributionPrice = a.DistributionPrice,
                        DistributorShow = a.DistributorShow,
                        OfferFreeItemQuantity = a.OfferFreeItemQuantity.HasValue ? a.OfferFreeItemQuantity.Value : 0,
                        OfferMinimumQty = a.OfferMinimumQty.HasValue ? a.OfferMinimumQty.Value : 0,
                        FlashDealSpecialPrice = a.FlashDealSpecialPrice.HasValue ? a.FlashDealSpecialPrice.Value : 0,
                        FlashDealMaxQtyPersonCanTake = a.FlashDealMaxQtyPersonCanTake.HasValue ? a.FlashDealMaxQtyPersonCanTake.Value : 0,
                        FreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                        qty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).qty,
                        CartUnitPrice = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).UnitPrice,
                        TotalFreeItemQty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).FreeItemqty,
                        TotalFreeWalletPoint = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).TotalFreeWalletPoint,
                    }).ToList();
                    foreach (var item in ShoppingCartItemDcs)
                    {
                        item.IsSuccess = true;
                        bool valid = true;
                        if (!item.DistributorShow || !item.active)
                        {
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "Item is not Active";
                        }

                        if (valid && !(item.ItemAppType == 2 || item.ItemAppType == 0))
                        {
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "Item is not Active";
                        }

                        var mod = Convert.ToDecimal(item.qty) % item.MinOrderQty;
                        if (mod != 0 || (item.ItemlimitQty > 0 && item.qty > 0 && item.MinOrderQty > item.ItemlimitQty))
                        {
                            valid = false;
                            item.IsSuccess = false;
                            item.qty = 0;
                            item.Message = "Item qty is not multiples of min order qty.";
                            if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                            {
                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                            }
                        }

                        if (valid && item.DistributionPrice != item.CartUnitPrice)
                        {
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "Item Unit Price has changed";
                            item.NewUnitPrice = item.UnitPrice;
                            if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                            {
                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.UnitPrice;
                            }

                        }



                        if (valid && item.IsItemLimit && item.ItemlimitQty < item.qty)
                        {
                            item.qty = item.qty > item.ItemlimitQty ? item.ItemlimitQty : (item.ItemlimitQty - item.qty);
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "Item Limit Exceeded";
                            if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                            {
                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                            }
                        }
                        if (valid && item.BillLimitQty > 0 && item.BillLimitQty < item.qty)
                        {
                            item.qty = item.qty > item.BillLimitQty ? item.BillLimitQty : (item.BillLimitQty - item.qty);
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "Item Bill Limit Exceeded";
                            if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                            {
                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                            }
                        }

                        if (valid && item.OfferFreeItemId > 0 && item.TotalFreeItemQty > 0)
                        {
                            #region new code for selling sku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == item.ItemNumber && x.ItemMultiMRPId == item.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == item.SellingSku && x.ItemMultiMRPId == item.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == item.ItemNumber && x.ItemMultiMRPId == item.ItemMultiMRPId))
                                {
                                    var freeItemofferitem = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == item.ItemNumber && x.ItemMultiMRPId == item.ItemMultiMRPId);
                                    if (freeItemofferitem != null && freeItemofferitem.QtyAvaiable < item.TotalFreeItemQty)
                                    {
                                        valid = false;
                                        item.IsOffer = false;
                                        item.IsSuccess = false;
                                        item.Message = "Free Item expired";
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == item.SellingSku && x.ItemMultiMRPId == item.ItemMultiMRPId))
                                {
                                    var freeItemoffersku = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == item.SellingSku && x.ItemMultiMRPId == item.ItemMultiMRPId);
                                    if (freeItemoffersku != null && freeItemoffersku.QtyAvaiable < item.TotalFreeItemQty)
                                    {
                                        valid = false;
                                        item.IsOffer = false;
                                        item.IsSuccess = false;
                                        item.Message = "Free Item expired";
                                    }
                                }
                            }
                            
                            #endregion

                            #region old code for itemnumber
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == item.ItemNumber);
                            //if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.TotalFreeItemQty)
                            //{
                            //    valid = false;
                            //    item.IsSuccess = false;
                            //    item.Message = "Free Item expired";
                            //}
                            #endregion
                        }
                    }

                    if (ShoppingCartItemDcs != null && ShoppingCartItemDcs.Any(x => x.FreeItemId > 0 && x.TotalFreeItemQty > 0))
                    {
                        foreach (var item in ShoppingCartItemDcs.GroupBy(x => new { x.FreeItemId, x.ItemNumber }))
                        {
                            if (item.Sum(x => x.TotalFreeItemQty) > 0)
                            {
                                var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == item.Key.ItemNumber);
                                if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.Sum(x => x.TotalFreeItemQty))
                                {
                                    var qtyAvailable = Convert.ToInt32(freeItemoffer.QtyAvaiable);
                                    foreach (var shoppingCart in ShoppingCartItemDcs.Where(x => x.ItemNumber == item.Key.ItemNumber))
                                    {
                                        if (shoppingCart.TotalFreeItemQty > qtyAvailable)
                                        {
                                            shoppingCart.OfferCategory = 0;
                                            shoppingCart.IsOffer = false;
                                            shoppingCart.IsSuccess = false;
                                            shoppingCart.Message = "Free Item expired";
                                        }
                                        else
                                        {
                                            qtyAvailable = qtyAvailable - shoppingCart.TotalFreeItemQty;
                                        }

                                    }
                                }
                            }
                        }
                    }



                    customerShoppingCart.TotalDiscountAmt = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.DiscountAmount) : 0;
                    customerShoppingCart.NewBillingWalletPoint = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.NewBillingWalletPoint) : 0;
                    customerShoppingCart.DeamPoint = newdata.Where(x => x.dreamPoint.HasValue).Sum(x => x.dreamPoint.Value * customerShoppingCart.ShoppingCartItems.FirstOrDefault(y => y.ItemId == x.ItemId).qty);
                    customerShoppingCart.CartTotalAmt = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice) - customerShoppingCart.TotalDiscountAmt;

                    customerShoppingCart.TotalTaxAmount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.TaxAmount);
                    customerShoppingCart.GrossTotalAmt = Math.Round(customerShoppingCart.CartTotalAmt, 0, MidpointRounding.AwayFromZero);

                    customerShoppingCart.WalletAmount = walletPoint;

                    customerShoppingCart.TotalSavingAmt = ShoppingCartItemDcs.Sum(x => (x.qty * x.price) - (x.qty * x.CartUnitPrice));
                    customerShoppingCart.TotalQty = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                    customerShoppingCart.WheelCount = Convert.ToInt32(Math.Truncate((customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt) / 4000));
                    customerShoppingCart.SkCode = ActiveCustomer != null ? ActiveCustomer.Skcode : "";
                    customerShoppingCart.Mobile = ActiveCustomer != null ? ActiveCustomer.Mobile : "";
                    customerShoppingCart.ShopName = ActiveCustomer != null ? ActiveCustomer.ShopName : "";
                    customerShoppingCart.City = ActiveCustomer != null ? ActiveCustomer.City : "";
                    double TotalAmount = customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt;
                    double DeliveryAmount = 0;
                    var storeIds = ShoppingCartItemDcs.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();
                    if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount))
                    {
                        if (storeIds.All(x => x == storeIds.Max(y => y))
                            && deliveryCharges.Any(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount)
                            )
                            DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));
                        else
                            DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (!x.storeId.HasValue || x.storeId.Value == 0) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));


                    }
                    customerShoppingCart.DeliveryCharges = DeliveryAmount;

                    FreeItemOfferIds = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();


                    #region TCS Calculate
                    GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                    var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(ActiveCustomer.CustomerId, ActiveCustomer.PanNo, context);
                    if (!ActiveCustomer.IsTCSExemption && tcsConfig != null && (tcsConfig.IsAlreadyTcsUsed == true || (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + customerShoppingCart.CartTotalAmt > tcsConfig.TCSAmountLimit)))
                    {
                        customerShoppingCart.TCSPercent = !ActiveCustomer.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                        customerShoppingCart.PreTotalDispatched = tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount;
                        customerShoppingCart.TCSLimit = tcsConfig.TCSAmountLimit;
                    }
                    else
                    {
                        customerShoppingCart.TCSPercent = 0;
                        customerShoppingCart.PreTotalDispatched = 0;
                        customerShoppingCart.TCSLimit = 0;
                    }
                    #endregion
                }
                customerShoppingCart.ModifiedDate = DateTime.Now;

                var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart);


                customerShoppingCartDc.TCSPercent = customerShoppingCart.TCSPercent;
                customerShoppingCartDc.TCSLimit = customerShoppingCart.TCSLimit;
                customerShoppingCartDc.PreTotalDispatched = customerShoppingCart.PreTotalDispatched;
                customerShoppingCartDc.ApplyOfferId = string.Join(",", ShoppingCartDiscounts.Where(x => FreeItemOfferIds.Contains(x.OfferId.Value) || (x.DiscountAmount > 0 || x.NewBillingWalletPoint > 0) && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId).ToList());
                customerShoppingCartDc.CartTotalAmt = customerShoppingCart.CartTotalAmt;
                customerShoppingCartDc.CustomerId = customerShoppingCart.CustomerId;
                customerShoppingCartDc.DeamPoint = customerShoppingCart.DeamPoint;
                customerShoppingCartDc.DeliveryCharges = customerShoppingCart.DeliveryCharges;
                customerShoppingCartDc.GeneratedOrderId = customerShoppingCart.GeneratedOrderId;
                customerShoppingCartDc.GrossTotalAmt = customerShoppingCart.GrossTotalAmt;
                customerShoppingCartDc.TotalDiscountAmt = customerShoppingCart.TotalDiscountAmt;
                customerShoppingCartDc.TotalTaxAmount = customerShoppingCart.TotalTaxAmount;
                customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
                customerShoppingCartDc.WarehouseId = customerShoppingCart.WarehouseId;
                customerShoppingCartDc.TotalSavingAmt = customerShoppingCart.TotalSavingAmt;
                customerShoppingCartDc.TotalQty = customerShoppingCart.TotalQty;
                customerShoppingCartDc.WheelCount = customerShoppingCart.WheelCount;
                customerShoppingCartDc.NewBillingWalletPoint = customerShoppingCart.NewBillingWalletPoint;
                customerShoppingCartDc.ShoppingCartItemDcs = ShoppingCartItemDcs;

            }
            return customerShoppingCartDc;
        }

        [AutomaticRetry(Attempts = 0)]
        public CustomerShoppingCartDc RefereshCartSync(CustomerShoppingCart customerShoppingCart, string lang)
        {
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            List<ShoppingCartItemDc> ShoppingCartItemDcs = new List<ShoppingCartItemDc>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            int walletPoint = 0;
            if (customerShoppingCart != null)
            {
                customerShoppingCartDc = new CustomerShoppingCartDc
                {
                    CartTotalAmt = 0,
                    CustomerId = customerShoppingCart.CustomerId,
                    DeamPoint = 0,
                    DeliveryCharges = 0,
                    GrossTotalAmt = 0,
                    TotalDiscountAmt = 0,
                    TotalTaxAmount = 0,
                    WalletAmount = 0,
                    WarehouseId = customerShoppingCart.WarehouseId,
                };
                using (var context = new AuthContext())
                {
                    if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any())
                    {
                        var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == customerShoppingCart.WarehouseId && x.isDeleted == false && x.IsActive && x.IsDistributor).ToList();
                        var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerShoppingCart.CustomerId);
                        var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                        List<int> itemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("IntValue");
                        foreach (var item in itemids)
                        {
                            var dr = orderIdDt.NewRow();
                            dr["IntValue"] = item;
                            orderIdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("ItemIds", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetShoppingCardItem]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var newdata = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<factoryItemdata>(reader).ToList();
                        newdata = newdata.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();
                        var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.WarehouseId == ActiveCustomer.Warehouseid && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == customerShoppingCart.WarehouseId && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems)
                          .SelectMany(x =>
                      x.OfferFreeItems.Select(y => new
                      {
                          y.FreeItemId,
                          y.Id,
                          y.ItemNumber,
                          y.OfferFreeItemImage,
                          y.OfferFreeItemName,
                          y.OfferFreeItemQuantity,
                          y.offerId,
                          y.OfferMinimumQty,
                          y.OfferQtyAvaiable,
                          y.OfferQtyConsumed,
                          y.OfferType,
                          y.OfferWalletPoint,
                          y.Offer.itemId,
                          y.Offer.QtyAvaiable,
                          y.OfferOn,
                          y.ItemMultiMRPId
                      })).ToList();
                        foreach (var it in newdata)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                            #region old code for itemnumber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                            //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion


                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion

                            if (it.IsOffer)
                            {
                                if (it.OfferType == "WalletPoint" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferWalletPoint.HasValue && it.OfferWalletPoint.Value > 0)
                                {
                                    var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                    if (item.qty >= it.OfferMinimumQty)
                                    {
                                        var FreeWalletPoint = it.OfferWalletPoint.Value;
                                        int calfreeItemQty = item.qty / it.OfferMinimumQty.Value;
                                        FreeWalletPoint *= calfreeItemQty;
                                        item.TotalFreeWalletPoint = FreeWalletPoint;
                                        walletPoint += Convert.ToInt32(FreeWalletPoint);
                                    }

                                }
                                else if (it.OfferType == "ItemMaster" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferFreeItemQuantity.HasValue && it.OfferFreeItemQuantity.Value > 0)
                                {
                                    var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                    if (item.qty >= it.OfferMinimumQty)
                                    {
                                        var FreeItemQuantity = it.OfferFreeItemQuantity.Value;
                                        int calfreeItemQty = Convert.ToInt32(item.qty / it.OfferMinimumQty);
                                        FreeItemQuantity *= calfreeItemQty;
                                        if (FreeItemQuantity > 0)
                                        {
                                            item.FreeItemqty = FreeItemQuantity;
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi")
                            {
                                if (it.HindiName != null)
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

                        CustomersManager manager = new CustomersManager();
                        List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(customerShoppingCart.CustomerId, "Distributor App");
                        if (billDiscountOfferDcs.Any())
                        {
                            billDiscountOfferDcs = billDiscountOfferDcs.Where(x => !(x.OfferOn == "Slab" || x.OfferOn == "ItemPost")).ToList();
                            foreach (var Offer in billDiscountOfferDcs)
                            {
                                ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                                double totalamount = 0;
                                var OrderLineItems = 0;
                                if (Offer.OfferOn != "ScratchBillDiscount")
                                {
                                    List<int> Itemids = new List<int>();
                                    if (Offer.BillDiscountType == "category" && Offer.OfferBillDiscountItems.Any())
                                    {
                                        var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                        var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                        Itemids = newdata.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                        var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                        if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                        {
                                            List<int> lineItemValueItemExists = new List<int>();
                                            foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                            {
                                                int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                                if (ItemId == 0)
                                                {
                                                    totalamount = 0;
                                                    break;
                                                }
                                                else
                                                    lineItemValueItemExists.Add(ItemId);
                                            }
                                        }

                                    }
                                    else if (Offer.BillDiscountType == "subcategory" && Offer.OfferBillDiscountItems.Any())
                                    {
                                        var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                        Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                        var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                        if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                        {
                                            List<int> lineItemValueItemExists = new List<int>();
                                            foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                            {
                                                int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                                if (ItemId == 0)
                                                {
                                                    totalamount = 0;
                                                    break;
                                                }
                                                else
                                                    lineItemValueItemExists.Add(ItemId);
                                            }
                                        }
                                    }
                                    else if (Offer.BillDiscountType == "brand" && Offer.OfferBillDiscountItems.Any())
                                    {
                                        var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();

                                        Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                        var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                        if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                        {
                                            List<int> lineItemValueItemExists = new List<int>();
                                            foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                            {
                                                int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                                if (ItemId == 0)
                                                {
                                                    totalamount = 0;
                                                    break;
                                                }
                                                else
                                                    lineItemValueItemExists.Add(ItemId);
                                            }
                                        }
                                    }
                                    else if (Offer.BillDiscountType == "items")
                                    {
                                        var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                        if (Offer.OfferItems.FirstOrDefault().IsInclude)
                                        {
                                            Itemids = newdata.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                        }
                                        var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                        var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                        if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                        {
                                            List<int> lineItemValueItemExists = new List<int>();
                                            foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                            {
                                                int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                                if (ItemId == 0)
                                                {
                                                    totalamount = 0;
                                                    break;
                                                }
                                                else
                                                    lineItemValueItemExists.Add(ItemId);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                        Itemids = newdata.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();
                                        var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                        var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).ToList();
                                        if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                        {
                                            List<int> lineItemValueItemExists = new List<int>();
                                            foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                            {
                                                int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                                if (ItemId == 0)
                                                {
                                                    totalamount = 0;
                                                    break;
                                                }
                                                else
                                                    lineItemValueItemExists.Add(ItemId);
                                            }
                                        }
                                    }

                                    if (Offer.BillDiscountRequiredItems != null && Offer.BillDiscountRequiredItems.Any())
                                    {
                                        bool IsRequiredItemExists = true;
                                        var objectIds = Offer.BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                                        if (Offer.BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                                        {
                                            objectIds.AddRange(newdata.Where(x => Offer.BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                                        }
                                        var cartrequiredItems = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                        foreach (var reqitem in Offer.BillDiscountRequiredItems)
                                        {
                                            if (reqitem.ObjectType == "Item")
                                            {
                                                var reqobjectids = reqitem.ObjectId.Split(',').Select(z => Convert.ToInt32(z)).ToList();
                                                var cartitem = cartrequiredItems.Where(x => reqobjectids.Contains(x.ItemMultiMRPId));
                                                if (cartitem != null && cartitem.Any())
                                                {
                                                    if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitem.Sum(x => x.qty))
                                                    {
                                                        IsRequiredItemExists = false;
                                                        break;
                                                    }
                                                    else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitem.Sum(x => x.qty * x.UnitPrice))
                                                    {
                                                        IsRequiredItemExists = false;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }
                                            }
                                            else if (reqitem.ObjectType == "brand")
                                            {
                                                var reqobjectids = reqitem.ObjectId.Split(',').Select(z => z).ToList();
                                                var multiMrpIds = newdata.Where(x => reqobjectids.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
                                                var cartitems = cartrequiredItems.Where(x => multiMrpIds.Contains(x.ItemMultiMRPId));
                                                if (cartitems != null && cartitems.Any())
                                                {
                                                    if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitems.Sum(x => x.qty))
                                                    {
                                                        IsRequiredItemExists = false;
                                                        break;
                                                    }
                                                    else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitems.Sum(x => x.qty * x.UnitPrice))
                                                    {
                                                        IsRequiredItemExists = false;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    IsRequiredItemExists = false;
                                                    break;
                                                }

                                            }
                                        }
                                        if (!IsRequiredItemExists)
                                        {
                                            totalamount = 0;
                                        }
                                    }

                                    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                                    {
                                        totalamount = Offer.MaxBillAmount;
                                    }
                                    else if (Offer.BillAmount > totalamount)
                                    {
                                        totalamount = 0;
                                    }

                                    if (Offer.LineItem > 0 && Offer.LineItem > OrderLineItems)
                                    {
                                        totalamount = 0;
                                    }
                                }
                                else
                                {
                                    totalamount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice);
                                    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount && Offer.IsScratchBDCode)
                                    {
                                        totalamount = Offer.MaxBillAmount;
                                    }
                                    else if (Offer.BillAmount > totalamount)
                                    {
                                        totalamount = 0;
                                    }
                                    if (!Offer.IsScratchBDCode)
                                        totalamount = 0;
                                }

                                bool IsUsed = true;
                                if (!Offer.IsUseOtherOffer && ShoppingCartDiscounts.Any())
                                    IsUsed = false;

                                if (IsUsed && totalamount > 0)
                                {
                                    if (Offer.BillDiscountOfferOn == "Percentage")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                    }
                                    else if (Offer.BillDiscountOfferOn == "FreeItem")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = 0;
                                    }
                                    else
                                    {
                                        int WalletPoint = 0;
                                        if (Offer.WalletType == "WalletPercentage")
                                        {
                                            WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * (Offer.BillDiscountWallet ?? 0) / 100));
                                            WalletPoint = WalletPoint * 10;
                                        }
                                        else
                                        {
                                            WalletPoint = Convert.ToInt32(Offer.BillDiscountWallet.Value);
                                        }
                                        if (Offer.ApplyOn == "PostOffer")
                                        {
                                            ShoppingCartDiscount.DiscountAmount = 0;
                                            ShoppingCartDiscount.NewBillingWalletPoint = WalletPoint;
                                        }
                                        else
                                        {
                                            ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10); ;
                                            ShoppingCartDiscount.NewBillingWalletPoint = 0;
                                        }
                                    }
                                    if (Offer.MaxDiscount > 0)
                                    {
                                        var walletmultipler = 1;

                                        if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage" && Offer.BillDiscountOfferOn != "DynamicAmount")
                                        {
                                            walletmultipler = 10;
                                        }
                                        if (Offer.BillDiscountOfferOn != "DynamicAmount")
                                        {
                                            if (Offer.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                            {
                                                ShoppingCartDiscount.DiscountAmount = Offer.MaxDiscount * walletmultipler;
                                            }
                                            if (Offer.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint)
                                            {
                                                ShoppingCartDiscount.NewBillingWalletPoint = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                                            }
                                        }
                                    }

                                    ShoppingCartDiscount.OfferId = Offer.OfferId;
                                    ShoppingCartDiscount.CreatedDate = DateTime.Now;
                                    ShoppingCartDiscount.IsActive = Offer.IsActive;
                                    ShoppingCartDiscount.IsDeleted = false;
                                    ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                                }
                            }
                        }
                        customerShoppingCart.ShoppingCartDiscounts = ShoppingCartDiscounts;

                        customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList().ForEach(x =>
                        {
                            if (newdata.Any(y => y.ItemId == x.ItemId))
                            {
                                x.ItemMultiMRPId = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemMultiMRPId;
                                x.ItemNumber = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemNumber;
                                x.ItemName = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).itemname;
                            }
                        });

                        ShoppingCartItemDcs = newdata.Select(a => new ShoppingCartItemDc
                        {
                            BaseCategoryId = a.BaseCategoryId,
                            IsItemLimit = a.IsItemLimit,
                            ItemlimitQty = a.ItemlimitQty,
                            BillLimitQty = a.BillLimitQty,
                            WarehouseId = a.WarehouseId,
                            CompanyId = a.CompanyId,
                            Categoryid = a.Categoryid,
                            Discount = a.Discount,
                            ItemId = a.ItemId,
                            ItemNumber = a.ItemNumber,
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
                            UnitPrice = a.DistributionPrice ?? 0,
                            VATTax = a.VATTax,
                            itemBaseName = a.itemBaseName,
                            active = a.active,
                            marginPoint = a.marginPoint.HasValue ? a.marginPoint.Value : 0,
                            promoPerItems = a.promoPerItems.HasValue ? a.promoPerItems.Value : 0,
                            NetPurchasePrice = a.NetPurchasePrice,
                            IsOffer = a.IsOffer,
                            Deleted = a.Deleted,
                            OfferCategory = a.OfferCategory.HasValue ? a.OfferCategory.Value : 0,
                            OfferStartTime = a.OfferStartTime,
                            OfferEndTime = a.OfferEndTime,
                            OfferQtyAvaiable = a.OfferQtyAvaiable.HasValue ? a.OfferQtyAvaiable.Value : 0,
                            OfferQtyConsumed = a.OfferQtyConsumed.HasValue ? a.OfferQtyConsumed.Value : 0,
                            OfferId = a.OfferId.HasValue ? a.OfferId.Value : 0,
                            OfferType = a.OfferType,
                            dreamPoint = a.dreamPoint.HasValue ? a.dreamPoint.Value : 0,
                            OfferWalletPoint = a.OfferWalletPoint.HasValue ? a.OfferWalletPoint.Value : 0,
                            OfferFreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                            OfferPercentage = a.OfferPercentage.HasValue ? a.OfferPercentage.Value : 0,
                            OfferFreeItemName = a.OfferFreeItemName,
                            OfferFreeItemImage = a.OfferFreeItemImage,
                            OfferFreeItemQuantity = a.OfferFreeItemQuantity.HasValue ? a.OfferFreeItemQuantity.Value : 0,
                            OfferMinimumQty = a.OfferMinimumQty.HasValue ? a.OfferMinimumQty.Value : 0,
                            FlashDealSpecialPrice = a.FlashDealSpecialPrice.HasValue ? a.FlashDealSpecialPrice.Value : 0,
                            FlashDealMaxQtyPersonCanTake = a.FlashDealMaxQtyPersonCanTake.HasValue ? a.FlashDealMaxQtyPersonCanTake.Value : 0,
                            FreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                            DistributionPrice = a.DistributionPrice,
                            DistributorShow = a.DistributorShow,
                            qty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).qty,
                            CartUnitPrice = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).UnitPrice,
                            TotalFreeItemQty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).FreeItemqty,
                            TotalFreeWalletPoint = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).TotalFreeWalletPoint,
                        }).ToList();
                        foreach (var item in ShoppingCartItemDcs)
                        {
                            item.IsSuccess = true;
                            bool valid = true;
                            if (!item.DistributorShow || !item.active)
                            {
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "Item is not Active";
                            }

                            if (valid && item.UnitPrice != item.CartUnitPrice)
                            {
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "Item Unit Price has changed";
                                item.NewUnitPrice = item.UnitPrice;
                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                {
                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.UnitPrice;
                                }
                            }
                            if (valid && item.IsItemLimit && item.ItemlimitQty < item.qty)
                            {
                                item.qty = item.qty > item.ItemlimitQty ? item.ItemlimitQty : (item.ItemlimitQty - item.qty);
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "Item Limit Exceeded";
                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                {
                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                                }
                            }
                            if (valid && item.BillLimitQty > 0 && item.BillLimitQty < item.qty)
                            {
                                item.qty = item.qty > item.BillLimitQty ? item.BillLimitQty : (item.BillLimitQty - item.qty);
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "Item Bill Limit Exceeded";
                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                {
                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                                }
                            }

                            if (valid && item.OfferFreeItemId > 0 && item.OfferFreeItemQuantity > 0)
                            {
                                var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == item.ItemNumber);
                                if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.TotalFreeItemQty)
                                {
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.Message = "Free Item expired";
                                }
                            }
                        }

                        if (ShoppingCartItemDcs != null && ShoppingCartItemDcs.Any(x => x.FreeItemId > 0 && x.OfferFreeItemQuantity > 0))
                        {
                            foreach (var item in ShoppingCartItemDcs.GroupBy(x => new { x.FreeItemId, x.ItemNumber }))
                            {
                                if (item.Sum(x => x.OfferFreeItemQuantity) > 0)
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == item.Key.ItemNumber);
                                    if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.Sum(x => x.TotalFreeItemQty))
                                    {
                                        var qtyAvailable = Convert.ToInt32(freeItemoffer.QtyAvaiable);
                                        foreach (var shoppingCart in ShoppingCartItemDcs.Where(x => x.ItemNumber == item.Key.ItemNumber))
                                        {
                                            if (shoppingCart.TotalFreeItemQty > qtyAvailable)
                                            {
                                                shoppingCart.IsSuccess = false;
                                                shoppingCart.Message = "Free Item expired";
                                            }
                                            else
                                            {
                                                qtyAvailable = qtyAvailable - shoppingCart.TotalFreeItemQty;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        #region TCS Calculate
                        string fy = (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString();
                        MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
                        var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();
                        customerShoppingCart.TCSPercent = 0;
                        if (tcsConfig != null)
                        {
                            MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                            var tcsCustomer = mHelper.Select(x => x.CustomerId == ActiveCustomer.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                            if (tcsCustomer != null && tcsCustomer.TotalPurchase >= tcsConfig.TCSAmountLimit)
                            {
                                customerShoppingCart.TCSPercent = string.IsNullOrEmpty(ActiveCustomer.PanNo) ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                            }
                        }
                        #endregion
                        customerShoppingCart.TotalDiscountAmt = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.DiscountAmount) : 0;
                        customerShoppingCart.NewBillingWalletPoint = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.NewBillingWalletPoint) : 0;
                        customerShoppingCart.DeamPoint = newdata.Where(x => x.dreamPoint.HasValue).Sum(x => x.dreamPoint.Value * customerShoppingCart.ShoppingCartItems.FirstOrDefault(y => y.ItemId == x.ItemId).qty);
                        customerShoppingCart.CartTotalAmt = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice) - customerShoppingCart.TotalDiscountAmt;
                        customerShoppingCart.TotalTaxAmount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.TaxAmount);

                        customerShoppingCart.GrossTotalAmt = Math.Round(customerShoppingCart.CartTotalAmt, 0, MidpointRounding.AwayFromZero);
                        customerShoppingCart.WalletAmount = walletPoint;
                        customerShoppingCart.TotalSavingAmt = ShoppingCartItemDcs.Sum(x => (x.qty * x.price) - (x.qty * x.CartUnitPrice));
                        customerShoppingCart.TotalQty = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                        customerShoppingCart.WheelCount = Convert.ToInt32(Math.Truncate((customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt) / 4000));
                        customerShoppingCart.SkCode = ActiveCustomer != null ? ActiveCustomer.Skcode : "";
                        customerShoppingCart.Mobile = ActiveCustomer != null ? ActiveCustomer.Mobile : "";
                        customerShoppingCart.ShopName = ActiveCustomer != null ? ActiveCustomer.ShopName : "";
                        customerShoppingCart.City = ActiveCustomer != null ? ActiveCustomer.City : "";
                        double TotalAmount = customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt;
                        double DeliveryAmount = 0;
                        var storeIds = ShoppingCartItemDcs.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();
                        if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount))
                        {
                            if (storeIds.Count() > 1)
                                DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (!x.storeId.HasValue || x.storeId.Value == 0) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));
                            else
                                DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));

                            if (ActiveCustomer.IsPrimeCustomer)
                                DeliveryAmount = 0;
                        }
                        customerShoppingCart.DeliveryCharges = DeliveryAmount;


                    }
                    var result = mongoDbHelper.ReplaceWithoutFind(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                }
                customerShoppingCartDc.TCSPercent = customerShoppingCart.TCSPercent;
                customerShoppingCartDc.ApplyOfferId = string.Join(",", ShoppingCartDiscounts.Where(x => (x.DiscountAmount > 0 || x.NewBillingWalletPoint > 0) && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId).ToList());
                customerShoppingCartDc.CartTotalAmt = customerShoppingCart.CartTotalAmt;
                customerShoppingCartDc.CustomerId = customerShoppingCart.CustomerId;
                customerShoppingCartDc.DeamPoint = customerShoppingCart.DeamPoint;
                customerShoppingCartDc.DeliveryCharges = customerShoppingCart.DeliveryCharges;
                customerShoppingCartDc.GeneratedOrderId = customerShoppingCart.GeneratedOrderId;
                customerShoppingCartDc.GrossTotalAmt = customerShoppingCart.GrossTotalAmt;
                customerShoppingCartDc.TotalDiscountAmt = customerShoppingCart.TotalDiscountAmt;
                customerShoppingCartDc.TotalTaxAmount = customerShoppingCart.TotalTaxAmount;
                customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
                customerShoppingCartDc.WarehouseId = customerShoppingCart.WarehouseId;
                customerShoppingCartDc.TotalSavingAmt = customerShoppingCart.TotalSavingAmt;
                customerShoppingCartDc.TotalQty = customerShoppingCart.TotalQty;
                customerShoppingCartDc.WheelCount = customerShoppingCart.WheelCount;
                customerShoppingCartDc.NewBillingWalletPoint = customerShoppingCart.NewBillingWalletPoint;
                customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
                customerShoppingCartDc.ShoppingCartItemDcs = ShoppingCartItemDcs;
            }
            return customerShoppingCartDc;
        }

        [Route("DeleteItem")]
        [HttpPost]
        public async Task<ReturnShoppingCart> DeleteCartItem(CartItemDc cartItemDc)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == cartItemDc.CustomerId && x.WarehouseId == cartItemDc.WarehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            using (var context = new AuthContext())
            {
                if (customerShoppingCart != null)
                {
                    if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == cartItemDc.ItemId && x.IsFreeItem == cartItemDc.IsFreeItem))
                    {
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsActive = false;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsDeleted = true;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).ModifiedDate = DateTime.Now;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).ModifiedBy = cartItemDc.CustomerId;
                    }

                    returnShoppingCart.Status = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                }
                if (cartItemDc.IsCartRequire)
                {
                    customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, cartItemDc.lang);
                }
                else
                {
                    customerShoppingCartDc = null;
                    //BackgroundJob.Enqueue(() => RefereshCartSync(customerShoppingCart, cartItemDc.lang));
                }

            }
            returnShoppingCart.Cart = customerShoppingCartDc;
            return returnShoppingCart;
        }

        [Route("ClearCart")]
        [HttpGet]
        public async Task<ReturnShoppingCart> ClearCart(int customerId, int warehouseId)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            using (var context = new AuthContext())
            {
                if (customerShoppingCart != null)
                {
                    if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                    {
                        foreach (var item in customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                        {
                            item.IsActive = false;
                            item.IsDeleted = true;
                            item.ModifiedDate = DateTime.Now;
                            item.ModifiedBy = customerId;
                        }
                    }
                    var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                }

                customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, "en");
            }

            returnShoppingCart.Status = true;
            returnShoppingCart.Cart = customerShoppingCartDc;
            return returnShoppingCart;
        }

        [Route("GetCustomerCart")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ReturnShoppingCart> GetCustomerCart(int customerId, int warehouseId, string lang)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            using (var context = new AuthContext())
            {
                customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, lang);
            }

            returnShoppingCart.Status = true;
            returnShoppingCart.Cart = customerShoppingCartDc;
            return returnShoppingCart;
        }

        [Route("ApplyNewOffer")]
        [HttpGet]
        public async Task<ReturnShoppingCart> ApplyOffer(int customerId, int warehouseId, int offerId, bool IsApply, string lang)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            using (var context = new AuthContext())
            {
                if (customerShoppingCart != null)
                {
                    if (IsApply)
                    {
                        if (customerShoppingCart.ShoppingCartDiscounts == null)
                        {
                            customerShoppingCart.ShoppingCartDiscounts = new List<ShoppingCartDiscount> {
                            new ShoppingCartDiscount {

                              OfferId =offerId,
                              CreatedBy=customerId,
                              CreatedDate=DateTime.Now,
                              IsActive=true,
                              IsDeleted=false
                            }};

                        }
                        else if (customerShoppingCart.ShoppingCartDiscounts.Any(x => x.OfferId == offerId))
                        {
                            List<int> ExistingOfferIds = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.OfferId != offerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList();
                            bool Isvalid = ExistingOfferIds.Any() ? await IsValidOffer(ExistingOfferIds, offerId) : true;
                            if (Isvalid)
                            {
                                customerShoppingCart.ShoppingCartDiscounts.FirstOrDefault(x => x.OfferId == offerId).IsActive = true;
                                customerShoppingCart.ShoppingCartDiscounts.FirstOrDefault(x => x.OfferId == offerId).IsDeleted = false;
                            }
                        }
                        else
                        {
                            List<int> ExistingOfferIds = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.OfferId != offerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList();
                            bool Isvalid = ExistingOfferIds.Any() ? await IsValidOffer(ExistingOfferIds, offerId) : true;
                            if (Isvalid)
                            {
                                customerShoppingCart.ShoppingCartDiscounts.Add(new ShoppingCartDiscount
                                {

                                    OfferId = offerId,
                                    CreatedBy = customerId,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    IsDeleted = false
                                });
                            }
                        }
                    }
                    else
                    {
                        if (customerShoppingCart.ShoppingCartDiscounts != null && customerShoppingCart.ShoppingCartDiscounts.Any(x => x.OfferId == offerId))
                        {
                            customerShoppingCart.ShoppingCartDiscounts.FirstOrDefault(x => x.OfferId == offerId).IsActive = false;
                            customerShoppingCart.ShoppingCartDiscounts.FirstOrDefault(x => x.OfferId == offerId).IsDeleted = true;
                        }
                    }
                    returnShoppingCart.Status = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");

                    customerShoppingCartDc = await RefereshCartWithOutAutoApplyDis(customerShoppingCart, context, lang);

                    if (IsApply && customerShoppingCartDc != null && !customerShoppingCartDc.ApplyOfferId.Contains(offerId.ToString()))
                    {
                        returnShoppingCart.Status = false;
                        returnShoppingCart.Message = "You are not eligible for this offer!";
                    }
                    returnShoppingCart.Cart = customerShoppingCartDc;
                }


            }
            return returnShoppingCart;

        }

        private async Task<CustomerShoppingCartDc> RefereshCartWithOutAutoApplyDis(CustomerShoppingCart customerShoppingCart, AuthContext context, string lang)
        {
            List<ShoppingCartItemDc> ShoppingCartItemDcs = new List<ShoppingCartItemDc>();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = new List<DataContracts.Masters.BillDiscountOfferDc>();
            int walletPoint = 0;
            customerShoppingCartDc = new CustomerShoppingCartDc
            {
                CartTotalAmt = 0,
                CustomerId = customerShoppingCart.CustomerId,
                DeamPoint = 0,
                DeliveryCharges = 0,
                GrossTotalAmt = 0,
                TotalDiscountAmt = 0,
                TotalTaxAmount = 0,
                WalletAmount = 0,
                WarehouseId = customerShoppingCart.WarehouseId,
            };
            if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any())
            {
                var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == customerShoppingCart.WarehouseId && x.isDeleted == false && x.IsActive && x.IsDistributor).ToList();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerShoppingCart.CustomerId);
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                List<int> itemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var orderIdDt = new DataTable();
                orderIdDt.Columns.Add("IntValue");
                foreach (var item in itemids)
                {
                    var dr = orderIdDt.NewRow();
                    dr["IntValue"] = item;
                    orderIdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("ItemIds", orderIdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetShoppingCardItem]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var newdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<factoryItemdata>(reader).ToList();

                newdata = newdata.Where(x => x.DistributorShow == true && (x.ItemAppType == 0 || x.ItemAppType == 2)).ToList();
                //var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                //var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted).Select(x => x.OfferId).ToList() : new List<int>();

                var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.WarehouseId == ActiveCustomer.Warehouseid && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == customerShoppingCart.WarehouseId && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems)
                          .SelectMany(x =>
                      x.OfferFreeItems.Select(y => new
                      {
                          y.FreeItemId,
                          y.Id,
                          y.ItemNumber,
                          y.OfferFreeItemImage,
                          y.OfferFreeItemName,
                          y.OfferFreeItemQuantity,
                          y.offerId,
                          y.OfferMinimumQty,
                          y.OfferQtyAvaiable,
                          y.OfferQtyConsumed,
                          y.OfferType,
                          y.OfferWalletPoint,
                          y.Offer.itemId,
                          y.Offer.QtyAvaiable,
                          y.OfferOn,
                          y.ItemMultiMRPId
                      })).ToList();
                foreach (var it in newdata)
                {
                    it.IsOffer = false;
                    it.FlashDealSpecialPrice = 0;
                    it.OfferCategory = 0;

                    #region old code for itemnumber
                    //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                    //{
                    //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                    //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                    //    {
                    //        it.IsOffer = true;
                    //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                    //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                    //        it.OfferId = freeItemoffer.offerId;
                    //        it.OfferType = freeItemoffer.OfferType;
                    //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                    //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                    //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                    //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                    //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                    //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                    //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                    //    }
                    //}
                    #endregion

                    #region code changes for sellingsku
                    if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                    {
                        if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                        {
                            var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                            if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            {
                                it.IsOffer = true;
                                it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                it.OfferId = freeItemoffer.offerId;
                                it.OfferType = freeItemoffer.OfferType;
                                it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            }
                        }
                        else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                        {
                            var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                            if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            {
                                it.IsOffer = true;
                                it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                it.OfferId = freeItemoffer.offerId;
                                it.OfferType = freeItemoffer.OfferType;
                                it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            }
                        }
                    }
                    #endregion

                    if (it.IsOffer)
                    {
                        if (it.OfferType == "WalletPoint" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferWalletPoint.HasValue && it.OfferWalletPoint.Value > 0)
                        {
                            var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                            if (item.qty >= it.OfferMinimumQty)
                            {
                                var FreeWalletPoint = it.OfferWalletPoint.Value;
                                int calfreeItemQty = item.qty / it.OfferMinimumQty.Value;
                                FreeWalletPoint *= calfreeItemQty;
                                item.TotalFreeWalletPoint = FreeWalletPoint;
                                walletPoint += Convert.ToInt32(FreeWalletPoint);
                            }

                        }
                        else if (it.OfferType == "ItemMaster" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferFreeItemQuantity.HasValue && it.OfferFreeItemQuantity.Value > 0)
                        {
                            var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                            if (item.qty >= it.OfferMinimumQty)
                            {
                                var FreeItemQuantity = it.OfferFreeItemQuantity.Value;
                                int calfreeItemQty = Convert.ToInt32(item.qty / it.OfferMinimumQty);
                                FreeItemQuantity *= calfreeItemQty;
                                if (FreeItemQuantity > 0)
                                {
                                    item.FreeItemqty = FreeItemQuantity;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi")
                    {
                        if (it.HindiName != null)
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

                CustomersManager manager = new CustomersManager();
                List<int> offerIds = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList();
                billDiscountOfferDcs = offerIds.Any() ? manager.GetBillDiscountById(offerIds) : new List<DataContracts.Masters.BillDiscountOfferDc>();
                if (billDiscountOfferDcs.Any())
                {

                    foreach (var Offer in billDiscountOfferDcs.OrderByDescending(x => x.IsUseOtherOffer))
                    {
                        ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                        double totalamount = 0;
                        double BillDiscountamount = 0;
                        var OrderLineItems = 0;
                        if ((Offer.OfferOn == "ScratchBillDiscount" || Offer.OfferOn == "BillDiscount") && Offer.OfferOn != "ItemMarkDown")
                        {
                            List<int> Itemids = new List<int>();
                            if (Offer.BillDiscountType == "category" && Offer.OfferBillDiscountItems.Any())
                            {
                                var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                Itemids = newdata.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                        if (ItemId == 0)
                                        {
                                            totalamount = 0;
                                            break;
                                        }
                                        else
                                            lineItemValueItemExists.Add(ItemId);
                                    }
                                }

                            }
                            else if (Offer.BillDiscountType == "subcategory" && Offer.OfferBillDiscountItems.Any())
                            {
                                var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                        if (ItemId == 0)
                                        {
                                            totalamount = 0;
                                            break;
                                        }
                                        else
                                            lineItemValueItemExists.Add(ItemId);
                                    }
                                }
                            }
                            else if (Offer.BillDiscountType == "brand" && Offer.OfferBillDiscountItems.Any())
                            {
                                var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();

                                Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                        if (ItemId == 0)
                                        {
                                            totalamount = 0;
                                            break;
                                        }
                                        else
                                            lineItemValueItemExists.Add(ItemId);
                                    }
                                }
                            }
                            else if (Offer.BillDiscountType == "items")
                            {
                                var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                if (Offer.OfferItems.FirstOrDefault().IsInclude)
                                {
                                    Itemids = newdata.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                        if (ItemId == 0)
                                        {
                                            totalamount = 0;
                                            break;
                                        }
                                        else
                                            lineItemValueItemExists.Add(ItemId);
                                    }
                                }
                            }
                            else
                            {
                                var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                Itemids = newdata.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();
                                var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).ToList();
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                        if (ItemId == 0)
                                        {
                                            totalamount = 0;
                                            break;
                                        }
                                        else
                                            lineItemValueItemExists.Add(ItemId);
                                    }
                                }
                            }

                            if (Offer.BillDiscountRequiredItems != null && Offer.BillDiscountRequiredItems.Any())
                            {
                                bool IsRequiredItemExists = true;
                                var objectIds = Offer.BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                                if (Offer.BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                                {
                                    objectIds.AddRange(newdata.Where(x => Offer.BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                                }
                                var cartrequiredItems = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                foreach (var reqitem in Offer.BillDiscountRequiredItems)
                                {
                                    if (reqitem.ObjectType == "Item")
                                    {
                                        var reqobjectids = reqitem.ObjectId.Split(',').Select(z => Convert.ToInt32(z)).ToList();
                                        var cartitem = cartrequiredItems.Where(x => reqobjectids.Contains(x.ItemMultiMRPId));
                                        if (cartitem != null && cartitem.Any())
                                        {
                                            if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitem.Sum(x => x.qty))
                                            {
                                                IsRequiredItemExists = false;
                                                break;
                                            }
                                            else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitem.Sum(x => x.qty * x.UnitPrice))
                                            {
                                                IsRequiredItemExists = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            IsRequiredItemExists = false;
                                            break;
                                        }
                                    }
                                    else if (reqitem.ObjectType == "brand")
                                    {
                                        var reqobjectids = reqitem.ObjectId.Split(',').Select(z => z).ToList();
                                        var multiMrpIds = newdata.Where(x => reqobjectids.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
                                        var cartitems = cartrequiredItems.Where(x => multiMrpIds.Contains(x.ItemMultiMRPId));
                                        if (cartitems != null && cartitems.Any())
                                        {
                                            if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitems.Sum(x => x.qty))
                                            {
                                                IsRequiredItemExists = false;
                                                break;
                                            }
                                            else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitems.Sum(x => x.qty * x.UnitPrice))
                                            {
                                                IsRequiredItemExists = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            IsRequiredItemExists = false;
                                            break;
                                        }

                                    }
                                }
                                if (!IsRequiredItemExists)
                                {
                                    totalamount = 0;
                                }
                            }

                            if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                            {
                                totalamount = Offer.MaxBillAmount;
                            }
                            else if (Offer.BillAmount > totalamount)
                            {
                                totalamount = 0;
                            }

                            if (Offer.LineItem > 0 && Offer.LineItem > OrderLineItems)
                            {
                                totalamount = 0;
                            }

                            if (Offer.OfferOn == "ScratchBillDiscount")
                            {
                                var billdiscount = context.BillDiscountDb.FirstOrDefault(x => x.OfferId == Offer.OfferId && x.CustomerId == customerShoppingCart.CustomerId);
                                Offer.IsScratchBDCode = false;
                                if (billdiscount != null)
                                    Offer.IsScratchBDCode = billdiscount.OrderId == 0 ? billdiscount.IsScratchBDCode : false;

                                if (!Offer.IsScratchBDCode)
                                    totalamount = 0;
                            }
                        }
                        else if (Offer.OfferOn == "ItemMarkDown")
                        {
                            List<int> Itemids = new List<int>();
                            if (Offer.BillDiscountType == "category" && Offer.OfferBillDiscountItems.Any())
                            {
                                var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                Itemids = newdata.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                            }
                            else if (Offer.BillDiscountType == "subcategory" && Offer.OfferBillDiscountItems.Any())
                            {
                                var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                                Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                            }
                            else if (Offer.BillDiscountType == "brand" && Offer.OfferBillDiscountItems.Any())
                            {
                                var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();

                                Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                BillDiscountamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                            }
                        }
                        //else
                        //{
                        //    totalamount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice);
                        //    var billdiscount = context.BillDiscountDb.FirstOrDefault(x => x.OfferId == Offer.OfferId && x.CustomerId == customerShoppingCart.CustomerId);
                        //    Offer.IsScratchBDCode = false;
                        //    if (billdiscount != null)
                        //        Offer.IsScratchBDCode = billdiscount.OrderId == 0 ? billdiscount.IsScratchBDCode : false;

                        //    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                        //    {
                        //        totalamount = Offer.MaxBillAmount;
                        //    }
                        //    else if (Offer.BillAmount > totalamount)
                        //    {
                        //        totalamount = 0;
                        //    }

                        //    if (!Offer.IsScratchBDCode)
                        //        totalamount = 0;
                        //}


                        bool IsUsed = true;
                        if (!Offer.IsUseOtherOffer && ShoppingCartDiscounts.Any())
                            IsUsed = false;



                        if (IsUsed && totalamount > 0)
                        {
                            if (Offer.OfferOn != "ItemMarkDown")
                            {
                                if (Offer.BillDiscountOfferOn == "Percentage")
                                {
                                    ShoppingCartDiscount.DiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                }
                                else if (Offer.BillDiscountOfferOn == "FreeItem")
                                {
                                    ShoppingCartDiscount.DiscountAmount = 0;
                                }
                                else
                                {
                                    int WalletPoint = 0;
                                    if (Offer.WalletType == "WalletPercentage")
                                    {
                                        WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * (Offer.BillDiscountWallet ?? 0) / 100));
                                        WalletPoint = WalletPoint * 10;
                                    }
                                    else
                                    {
                                        WalletPoint = Convert.ToInt32(Offer.BillDiscountWallet ?? 0);
                                    }
                                    if (Offer.ApplyOn == "PostOffer")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = 0;
                                        ShoppingCartDiscount.NewBillingWalletPoint = WalletPoint;
                                    }
                                    else
                                    {
                                        ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10); ;
                                        ShoppingCartDiscount.NewBillingWalletPoint = 0;
                                    }
                                }
                                if (Offer.MaxDiscount > 0)
                                {
                                    var walletmultipler = 1;

                                    if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage" && Offer.BillDiscountOfferOn != "DynamicAmount")
                                    {
                                        walletmultipler = 10;
                                    }
                                    if (Offer.BillDiscountOfferOn != "DynamicAmount")
                                    {
                                        if (Offer.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                        {
                                            ShoppingCartDiscount.DiscountAmount = Offer.MaxDiscount * walletmultipler;
                                        }
                                        if (Offer.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint)
                                        {
                                            ShoppingCartDiscount.NewBillingWalletPoint = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                                        }
                                    }
                                }

                                ShoppingCartDiscount.OfferId = Offer.OfferId;
                                ShoppingCartDiscount.CreatedDate = DateTime.Now;
                                ShoppingCartDiscount.IsActive = Offer.IsActive;
                                ShoppingCartDiscount.IsDeleted = false;
                                ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                            }
                            else if (Offer.OfferOn == "ItemMarkDown" && BillDiscountamount > 0)
                            {
                                ShoppingCartDiscount.DiscountAmount = BillDiscountamount;
                                ShoppingCartDiscount.OfferId = Offer.OfferId;
                                ShoppingCartDiscount.CreatedDate = DateTime.Now;
                                ShoppingCartDiscount.IsActive = Offer.IsActive;
                                ShoppingCartDiscount.IsDeleted = false;
                                ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                            }
                        }

                    }
                }
                customerShoppingCart.ShoppingCartDiscounts = ShoppingCartDiscounts;

                customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList().ForEach(x =>
                {
                    if (newdata.Any(y => y.ItemId == x.ItemId))
                    {
                        x.ItemMultiMRPId = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemMultiMRPId;
                        x.ItemNumber = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemNumber;
                        x.ItemName = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).itemname;
                    }
                });
                ShoppingCartItemDcs = newdata.Select(a => new ShoppingCartItemDc
                {
                    BaseCategoryId = a.BaseCategoryId,
                    IsItemLimit = a.IsItemLimit,
                    ItemlimitQty = a.ItemlimitQty,
                    BillLimitQty = a.BillLimitQty,
                    WarehouseId = a.WarehouseId,
                    CompanyId = a.CompanyId,
                    Categoryid = a.Categoryid,
                    Discount = a.Discount,
                    ItemId = a.ItemId,
                    ItemNumber = a.ItemNumber,
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
                    UnitPrice = a.DistributionPrice ?? 0,
                    VATTax = a.VATTax,
                    itemBaseName = a.itemBaseName,
                    active = a.active,
                    marginPoint = a.marginPoint.HasValue ? a.marginPoint.Value : 0,
                    promoPerItems = a.promoPerItems.HasValue ? a.promoPerItems.Value : 0,
                    NetPurchasePrice = a.NetPurchasePrice,
                    IsOffer = a.IsOffer,
                    Deleted = a.Deleted,
                    OfferCategory = a.OfferCategory.HasValue ? a.OfferCategory.Value : 0,
                    OfferStartTime = a.OfferStartTime,
                    OfferEndTime = a.OfferEndTime,
                    OfferQtyAvaiable = a.OfferQtyAvaiable.HasValue ? a.OfferQtyAvaiable.Value : 0,
                    OfferQtyConsumed = a.OfferQtyConsumed.HasValue ? a.OfferQtyConsumed.Value : 0,
                    OfferId = a.OfferId.HasValue ? a.OfferId.Value : 0,
                    OfferType = a.OfferType,
                    dreamPoint = a.dreamPoint.HasValue ? a.dreamPoint.Value : 0,
                    OfferWalletPoint = a.OfferWalletPoint.HasValue ? a.OfferWalletPoint.Value : 0,
                    OfferFreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                    OfferPercentage = a.OfferPercentage.HasValue ? a.OfferPercentage.Value : 0,
                    OfferFreeItemName = a.OfferFreeItemName,
                    OfferFreeItemImage = a.OfferFreeItemImage,
                    OfferFreeItemQuantity = a.OfferFreeItemQuantity.HasValue ? a.OfferFreeItemQuantity.Value : 0,
                    OfferMinimumQty = a.OfferMinimumQty.HasValue ? a.OfferMinimumQty.Value : 0,
                    FlashDealSpecialPrice = a.FlashDealSpecialPrice.HasValue ? a.FlashDealSpecialPrice.Value : 0,
                    FlashDealMaxQtyPersonCanTake = a.FlashDealMaxQtyPersonCanTake.HasValue ? a.FlashDealMaxQtyPersonCanTake.Value : 0,
                    FreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                    DistributionPrice = a.DistributionPrice,
                    DistributorShow = a.DistributorShow,
                    qty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).qty,
                    CartUnitPrice = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).UnitPrice,
                    TotalFreeItemQty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).FreeItemqty,
                    TotalFreeWalletPoint = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).TotalFreeWalletPoint,
                }).ToList();
                foreach (var item in ShoppingCartItemDcs)
                {
                    item.IsSuccess = true;
                    bool valid = true;
                    if (!item.DistributorShow || !item.active)
                    {
                        valid = false;
                        item.IsSuccess = false;
                        item.Message = "Item is not Active";
                    }

                    if (valid && item.UnitPrice != item.CartUnitPrice)
                    {

                        valid = false;
                        item.IsSuccess = false;
                        item.Message = "Item Unit Price has changed";
                        item.NewUnitPrice = item.UnitPrice;
                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                        {
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.UnitPrice;
                        }

                    }


                    if (valid && item.IsItemLimit && item.ItemlimitQty < item.qty)
                    {
                        item.qty = item.qty > item.ItemlimitQty ? item.ItemlimitQty : (item.ItemlimitQty - item.qty);
                        valid = false;
                        item.IsSuccess = false;
                        item.Message = "Item Limit Exceeded";
                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                        {
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                        }
                    }
                    if (valid && item.BillLimitQty > 0 && item.BillLimitQty < item.qty)
                    {
                        item.qty = item.qty > item.BillLimitQty ? item.BillLimitQty : (item.BillLimitQty - item.qty);
                        valid = false;
                        item.IsSuccess = false;
                        item.Message = "Item Bill Limit Exceeded";
                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                        {
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                        }
                    }

                    if (valid && item.OfferFreeItemId > 0 && item.TotalFreeItemQty > 0)
                    {
                        var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == item.ItemNumber);
                        if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.TotalFreeItemQty)
                        {
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "Free Item expired";
                        }
                    }

                }


                if (ShoppingCartItemDcs != null && ShoppingCartItemDcs.Any(x => x.FreeItemId > 0 && x.TotalFreeItemQty > 0))
                {
                    foreach (var item in ShoppingCartItemDcs.GroupBy(x => new { x.FreeItemId, x.ItemNumber }))
                    {
                        if (item.Sum(x => x.TotalFreeItemQty) > 0)
                        {
                            var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == item.Key.ItemNumber);
                            if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.Sum(x => x.TotalFreeItemQty))
                            {
                                var qtyAvailable = Convert.ToInt32(freeItemoffer.QtyAvaiable);
                                foreach (var shoppingCart in ShoppingCartItemDcs.Where(x => x.ItemNumber == item.Key.ItemNumber))
                                {
                                    if (shoppingCart.TotalFreeItemQty > qtyAvailable)
                                    {
                                        shoppingCart.OfferCategory = 0;
                                        shoppingCart.IsOffer = false;
                                        shoppingCart.IsSuccess = false;
                                        shoppingCart.Message = "Free Item expired";
                                    }
                                    else
                                    {
                                        qtyAvailable = qtyAvailable - shoppingCart.TotalFreeItemQty;
                                    }

                                }
                            }
                        }
                    }
                }



                customerShoppingCart.TotalDiscountAmt = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.DiscountAmount) : 0;
                customerShoppingCart.NewBillingWalletPoint = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.NewBillingWalletPoint) : 0;
                customerShoppingCart.DeamPoint = newdata.Where(x => x.dreamPoint.HasValue).Sum(x => x.dreamPoint.Value * customerShoppingCart.ShoppingCartItems.FirstOrDefault(y => y.ItemId == x.ItemId).qty);
                customerShoppingCart.CartTotalAmt = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice) - customerShoppingCart.TotalDiscountAmt;
                customerShoppingCart.TotalTaxAmount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.TaxAmount);

                customerShoppingCart.GrossTotalAmt = Math.Round(customerShoppingCart.CartTotalAmt, 0, MidpointRounding.AwayFromZero);
                customerShoppingCart.WalletAmount = walletPoint;
                customerShoppingCart.TotalSavingAmt = ShoppingCartItemDcs.Sum(x => (x.qty * x.price) - (x.qty * x.CartUnitPrice));
                customerShoppingCart.TotalQty = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                customerShoppingCart.WheelCount = Convert.ToInt32(Math.Truncate((customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt) / 4000));
                customerShoppingCart.SkCode = ActiveCustomer != null ? ActiveCustomer.Skcode : "";
                customerShoppingCart.Mobile = ActiveCustomer != null ? ActiveCustomer.Mobile : "";
                customerShoppingCart.ShopName = ActiveCustomer != null ? ActiveCustomer.ShopName : "";
                customerShoppingCart.City = ActiveCustomer != null ? ActiveCustomer.City : "";
                double TotalAmount = customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt;
                double DeliveryAmount = 0;
                var storeIds = ShoppingCartItemDcs.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();
                if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount))
                {
                    if (storeIds.All(x => x == storeIds.Max(y => y))
                        && deliveryCharges.Any(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount)
                            )
                        DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));
                    else
                        DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (!x.storeId.HasValue || x.storeId.Value == 0) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));

                }
                customerShoppingCart.DeliveryCharges = DeliveryAmount;

                #region TCS Calculate
                GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(ActiveCustomer.CustomerId, ActiveCustomer.PanNo, context);
                if (!ActiveCustomer.IsTCSExemption && tcsConfig != null && (tcsConfig.IsAlreadyTcsUsed == true || (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + customerShoppingCart.CartTotalAmt > tcsConfig.TCSAmountLimit)))
                {
                    customerShoppingCart.TCSPercent = !ActiveCustomer.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                    customerShoppingCart.PreTotalDispatched = tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount;
                    customerShoppingCart.TCSLimit = tcsConfig.TCSAmountLimit;
                }
                else
                {
                    customerShoppingCart.TCSPercent = 0;
                    customerShoppingCart.PreTotalDispatched = 0;
                    customerShoppingCart.TCSLimit = 0;
                }
                #endregion
            }
            var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
            var freeofferids = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();
            customerShoppingCartDc.TCSPercent = customerShoppingCart.TCSPercent;
            customerShoppingCartDc.TCSLimit = customerShoppingCart.TCSLimit;
            customerShoppingCartDc.PreTotalDispatched = customerShoppingCart.PreTotalDispatched;
            customerShoppingCartDc.ApplyOfferId = string.Join(",", ShoppingCartDiscounts.Where(x => (x.DiscountAmount > 0 || x.NewBillingWalletPoint > 0 || freeofferids.Contains(x.OfferId.Value)) && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId).ToList());
            customerShoppingCartDc.CartTotalAmt = customerShoppingCart.CartTotalAmt;
            customerShoppingCartDc.CustomerId = customerShoppingCart.CustomerId;
            customerShoppingCartDc.DeamPoint = customerShoppingCart.DeamPoint;
            customerShoppingCartDc.DeliveryCharges = customerShoppingCart.DeliveryCharges;
            customerShoppingCartDc.GeneratedOrderId = customerShoppingCart.GeneratedOrderId;
            customerShoppingCartDc.GrossTotalAmt = customerShoppingCart.GrossTotalAmt;
            customerShoppingCartDc.TotalDiscountAmt = customerShoppingCart.TotalDiscountAmt;
            customerShoppingCartDc.TotalTaxAmount = customerShoppingCart.TotalTaxAmount;
            customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
            customerShoppingCartDc.WarehouseId = customerShoppingCart.WarehouseId;
            customerShoppingCartDc.TotalSavingAmt = customerShoppingCart.TotalSavingAmt;
            customerShoppingCartDc.TotalQty = customerShoppingCart.TotalQty;
            customerShoppingCartDc.WheelCount = customerShoppingCart.WheelCount;
            customerShoppingCartDc.NewBillingWalletPoint = customerShoppingCart.NewBillingWalletPoint;
            customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
            customerShoppingCartDc.ShoppingCartItemDcs = ShoppingCartItemDcs;

            return customerShoppingCartDc;
        }


        private async Task<bool> IsValidOffer(List<int> offerIds, int applyofferId)
        {
            bool isvalid = false;
            using (var context = new AuthContext())
            {
                var offerusedWithother = await context.OfferDb.Where(x => offerIds.Contains(x.OfferId)).Select(x => x.IsUseOtherOffer).FirstOrDefaultAsync();
                var ApplyofferusedWithOther = (await context.OfferDb.FirstOrDefaultAsync(x => x.OfferId == applyofferId)).IsUseOtherOffer;
                if (offerusedWithother && ApplyofferusedWithOther)
                {
                    isvalid = true;
                }
            }

            return isvalid;
        }
        #endregion


        [Route("GetDeliveryCharge")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetDeliveryCharge(int WarehouseId)//get all Issuances which are active for the delivery boy
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var DeliveryCharge = context.DeliveryChargeDb.Where(x => x.IsActive == true && x.WarehouseId == WarehouseId && x.isDeleted == false && x.IsDistributor).ToList();
                    if (DeliveryCharge.Count > 0) { }
                    return Request.CreateResponse(HttpStatusCode.OK, DeliveryCharge);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("MyOrdersWithPage")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<MyOrders>> MyOrdersWithPage(int customerId, int page = 0, int totalOrder = 10)
        {
            List<MyOrders> myOrders = new List<MyOrders>();
            using (var myContext = new AuthContext())
            {
                if (page > 0) page = page - 1;

                //var orders = await myContext.DbOrderMaster.Include("orderDetails").Where(x => x.CustomerId == customerId).ToListAsync();
                CustomersManager manager = new CustomersManager();
                myOrders = manager.GetCustomerOrder(customerId, page, totalOrder, "ALL");

                if (myOrders != null && myOrders.Any())
                {
                    myOrders.ForEach(x =>
                    {
                        var orderPayment = x.OrderPayments;
                        if (x.status.ToLower() == "failed" || x.status.ToLower() == "payment pending" || x.status.ToLower() == "pending" || x.status.ToLower() == "intransit" || x.status.ToLower() == "ready to dispatch" || x.status.ToLower() == "shipped" || x.status.ToLower() == "issued")
                        {
                            if ((x.status.ToLower() == "payment pending" && (orderPayment == null || !orderPayment.Any())) || (orderPayment != null && orderPayment.Any() && orderPayment.Any(z => z.PaymentFrom.ToLower() == "cash") && orderPayment.Where(z => z.PaymentFrom.ToLower() == "cash").Sum(y => y.Amount) > 0))
                            {
                                x.EnablePayNowButton = true;
                                x.PayNowOption = x.status.ToLower() == "failed" || x.status.ToLower() == "payment pending" ? "Both"
                                        : x.status.ToLower() == "pending" || x.status.ToLower() == "shipped" || x.status.ToLower() == "intransit" || x.status.ToLower() == "ready to dispatch" || x.status.ToLower() == "issued" ? "Gullak"
                                        : "";
                                x.RemainingAmount = x.GrossAmount - orderPayment.Where(z => z.PaymentFrom.ToLower() != "cash").Sum(z => z.Amount);

                            }
                            else
                                x.EnablePayNowButton = false;
                        }

                        x.OrderPayments = orderPayment.Where(z => z.PaymentFrom.ToLower() != "cash").Select(z => new OrderPayments
                        {
                            Amount = z.Amount,
                            PaymentFrom = z.PaymentFrom,
                            TransactionNumber = z.TransactionNumber,
                            TransactionDate = z.TransactionDate
                        }).ToList();

                    });


                }
            }

            return myOrders != null && myOrders.Any() ? myOrders.OrderByDescending(x => x.OrderId).ToList() : myOrders;
        }



        [Route("MyGullakTransWithPage")]
        [HttpGet]
        public async Task<List<CustomerGullakTransaction>> MyGullakTransWithPage(int customerId, int page = 0, int totalOrder = 10)
        {
            List<CustomerGullakTransaction> customerGullakTransactions = new List<CustomerGullakTransaction>();
            using (var myContext = new AuthContext())
            {
                if (page > 0) page = page - 1;

                CustomersManager manager = new CustomersManager();
                customerGullakTransactions = manager.GetCustomerGullakTrans(customerId, page, totalOrder);
            }

            return customerGullakTransactions;
        }

        //[Route("getdistributionDetails")]
        //[HttpGet]
        //public HttpResponseMessage getdistributionDetails(int customerId)
        #region
        [Route("getdistributionDetails")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getdistributionDetails(int customerId)
        {
            using (AuthContext db = new AuthContext())
            {
                var result = (from d in db.DistributorVerificationDB
                              join c in db.Customers on d.CustomerID equals c.CustomerId
                              //join cd in db.CustomerDocs on d.CustomerID equals cd.CustomerId//
                              //join cdm in db.CustomerDocTypeMasters on cd.CustomerDocTypeMasterId equals cdm.Id//
                              where d.CustomerID == customerId && d.IsDeleted == false && d.IsActive == true
                              select new DistributorVerificationDc
                              {
                                  AdvanceAmount = d.AdvanceAmount,
                                  GST = c.UploadGSTPicture,
                                  GSTNo = c.RefNo,
                                  FoodLicense = c.UploadRegistration,
                                  FoodLicenseNo = c.LicenseNumber,
                                  Type = c.Type,//cdm.DocType,
                                  UploadLicensePicture = c.UploadLicensePicture,//cd.DocPath,
                                  OtherNo = c.AadharNo,
                                  CustomerID = d.CustomerID,
                                  TypeOfBusiness = d.TypeOfBusiness,
                                  FirmName = d.FirmName,
                                  Manpower = d.Manpower,
                                  FranchiseeKKP = d.FranchiseeKKP,
                                  SourceofFund = d.SourceofFund,
                                  DrugLicenseNo = d.DrugLicenseNo,
                                  DrugLicenseValidity = d.DrugLicenseValidity,
                                  DrugLicense = d.DrugLicense,
                                  PANNo = d.PANNo,
                                  PAN = d.PAN,
                                  AadharNo = d.AadharNo,
                                  Aadhar = d.Aadhar,
                                  WarehouseFacility = d.WarehouseFacility,
                                  WarehouseSize = d.WarehouseSize,
                                  DeliveryVehicle = d.DeliveryVehicle,
                                  DeliveryVehicleNo = d.DeliveryVehicleNo,
                                  BlankCheque = d.BlankCheque,
                                  Signature = d.Signature,
                                  Status = d.Status,
                                  IsVerified = d.IsVerified,
                                  SignedPdf = d.SignedPdf
                              }).FirstOrDefault();

                //var result = db.DistributorVerificationDB.Where(x => x.CustomerID == customerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (result != null)
                {
                    double gullak = db.GullakDB.Where(x => x.CustomerId == customerId).Select(x => x.TotalAmount).FirstOrDefault();
                    result.AdvanceAmount = Convert.ToDecimal(gullak);

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

            }
        }
        #endregion


        [Route("updatedistributionVerify")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage updatedistributionVerify(int customerId)
        {
            using (AuthContext db = new AuthContext())
            {
                var result = db.DistributorVerificationDB.Where(x => x.CustomerID == customerId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (result != null)
                {
                    result.IsVerified = true;
                    result.Status = "Payment Received";
                    db.Entry(result).State = EntityState.Modified;
                    db.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
        }

        #region MyRegion
        [Route("SearchDistributorItem")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<List<DistributorPrice>> SearchDistributorItem(searchitem item)
        {
            using (var db = new AuthContext())
            {
                var predicate = PredicateBuilder.True<ItemMaster>();

                predicate = predicate.And(x => x.WarehouseId == item.WarehouseId && x.active == true && x.Deleted == false);

                if (item.CategoryId > 0)
                {
                    predicate = predicate.And(x => x.Categoryid == item.CategoryId);
                }
                if (item.SubCategoryId > 0)
                {
                    predicate = predicate.And(x => x.SubCategoryId == item.SubCategoryId);
                }
                if (item.BrandId > 0)
                {
                    predicate = predicate.And(x => x.SubsubCategoryid == item.BrandId);
                }
                if (!string.IsNullOrEmpty(item.SearchItem))
                {
                    predicate = predicate.And(x => x.itemname.Contains(item.SearchItem));
                }

                var itemlist = db.itemMasters.Where(predicate).Select(x => new DistributorPrice
                {
                    ItemId = x.ItemId,
                    ItemName = x.itemname,
                    ItemNumber = x.Number,
                    MultiMRPID = x.ItemMultiMRPId,
                    MOQ = x.MinOrderQty,
                    Price = x.price,
                    IsdistributorShow = x.DistributorShow,
                    DistributionPrice = x.DistributionPrice
                }).ToList();

                return itemlist;
            }
        }

        [Route("SearchDistributorItemV7")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult SearchDistributorItemV7(searchitem item)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                var predicate = PredicateBuilder.True<ItemMaster>();

                predicate = predicate.And(x => x.WarehouseId == item.WarehouseId && x.active == true && x.Deleted == false);

                if (item.CategoryId > 0)
                {
                    predicate = predicate.And(x => x.Categoryid == item.CategoryId);
                }
                if (item.SubCategoryId > 0)
                {
                    predicate = predicate.And(x => x.SubCategoryId == item.SubCategoryId);
                }
                if (item.BrandId > 0)
                {
                    predicate = predicate.And(x => x.SubsubCategoryid == item.BrandId);
                }
                if (!string.IsNullOrEmpty(item.SearchItem))
                {
                    predicate = predicate.And(x => x.itemname.Contains(item.SearchItem));
                }

                var itemlist = db.itemMasters.Where(predicate).Select(x => new DistributorPrice
                {
                    ItemId = x.ItemId,
                    ItemName = x.itemname,
                    ItemNumber = x.Number,
                    MultiMRPID = x.ItemMultiMRPId,
                    MOQ = x.MinOrderQty,
                    Price = x.price,
                    IsdistributorShow = x.DistributorShow,
                    DistributionPrice = x.DistributionPrice
                }).ToList();
                string sqlquery = "select distinct r.Name as Role, p.PeopleID,p.DisplayName from People p inner join AspNetUsers u on p.Email = u.Email inner join AspNetUserRoles ur on u.Id = ur.UserId inner join AspNetRoles r on ur.RoleId = r.Id where r.Name in('Sourcing Senior Executive') and ur.isActive = 1 and p.Active = 1 and p.Deleted = 0 and p.PeopleID = " + userid;
                DistributerRole newdata = db.Database.SqlQuery<DistributerRole>(sqlquery).FirstOrDefault();

                DistributorPriceResponse itemlistResponse = new DistributorPriceResponse()
                {
                    DistributorPriceList = itemlist,
                    DistributerRole = newdata
                };
                return Ok(itemlistResponse);
            }
        }


        [Route("UpdateDistributionPrice")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> UpdateDistributionPrice(List<DistributorPrice> itemupdate)
        {
            var identity = User.Identity as ClaimsIdentity;
            string username = "";
            var itemmasters = new List<ItemMaster>();




            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                username = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;

            using (var db = new AuthContext())
            {
                foreach (var item in itemupdate)
                {
                    var check = db.itemMasters.Where(x => x.ItemId == item.ItemId).FirstOrDefault();
                    check.DistributorShow = item.IsdistributorShow;
                    check.DistributionPrice = item.DistributionPrice;
                    check.UpdatedDate = DateTime.Now;
                    check.UpdateBy = username;
                    itemmasters.Add(check);
                    //db.Entry(check).State = EntityState.Modified;
                    //db.Commit();
                }
                foreach (var item in itemmasters)
                {
                    db.Entry(item).State = EntityState.Modified;
                }
                if (db.Commit() > 0) { return true; } else { return false; }

            }
        }

        [Route("GetCategory")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<Categorydc>> GetCategory(int warehouseid)
        {
            using (var db = new AuthContext())
            {
                var cat = db.Database.SqlQuery<Categorydc>("select distinct(Categoryid),CategoryName from ItemMasters where WarehouseId = " + warehouseid).ToList();
                return cat;
            }
        }

        [Route("GetSubCategory")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<SubCategorydc>> GetSubCategory(int catid)
        {
            using (var db = new AuthContext())
            {
                var subcat = new List<SubCategorydc>();
                if (catid == 0)
                {
                    subcat = db.Database.SqlQuery<SubCategorydc>("SELECT  isc.SubCategoryId,  isc.SubcategoryName   FROM  SubCategories isc with(nolock)   inner join SubcategoryCategoryMappings sccm with(nolock) on isc.SubCategoryId = sccm.SubCategoryId and sccm.IsActive=1  inner join Categories c with(nolock) on c.Categoryid = sccm.Categoryid  group by isc.SubcategoryName,isc.SubCategoryId").ToList();
                }
                else
                {
                    subcat = db.Database.SqlQuery<SubCategorydc>("SELECT  isc.SubCategoryId,  isc.SubcategoryName   FROM  SubCategories isc with(nolock)   inner join SubcategoryCategoryMappings sccm with(nolock) on isc.SubCategoryId = sccm.SubCategoryId and sccm.IsActive=1  inner join Categories c with(nolock) on c.Categoryid = sccm.Categoryid and c.Categoryid=" + catid + "  group by isc.SubcategoryName,isc.SubCategoryId").ToList();
                }
                return subcat;
            }
        }

        [Route("GetBrand")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<SubSubCategorydc>> GetBrand(int catid, int subcatid)
        {
            using (var db = new AuthContext())
            {
                var brand = new List<SubSubCategorydc>();
                if (subcatid == 0 && catid == 0)
                {
                    brand = db.Database.SqlQuery<SubSubCategorydc>("select a.SubsubcategoryName, a.SubsubCategoryid	from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid=b.SubsubCategoryId and b.IsActive=1 and b.Deleted=0 inner join SubcategoryCategoryMappings c on b.SubCategoryMappingId=c.SubCategoryMappingId and c.IsActive=1 and c.Deleted=0 inner join Categories d on c.Categoryid=d.Categoryid inner join SubCategories e on c.SubCategoryId=e.SubCategoryId where a.Deleted=0  group by  a.SubsubcategoryName,a.SubsubCategoryid").ToList();
                }
                else if (subcatid > 0 && catid > 0)
                {
                    brand = db.Database.SqlQuery<SubSubCategorydc>("select a.SubsubcategoryName, a.SubsubCategoryid	from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid=b.SubsubCategoryId and b.IsActive=1 and b.Deleted=0 inner join SubcategoryCategoryMappings c on b.SubCategoryMappingId=c.SubCategoryMappingId and c.IsActive=1 and c.Deleted=0 inner join Categories d on c.Categoryid=d.Categoryid inner join SubCategories e on c.SubCategoryId=e.SubCategoryId where a.Deleted=0 and e.SubCategoryId=" + subcatid + " and d.Categoryid=" + catid + "  group by  a.SubsubcategoryName,a.SubsubCategoryid").ToList();
                }
                return brand;
            }
        }
        #endregion

        [Route("GetDistributorMinOrder")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<DistributorMinOrder> GetDistributorMinOrder(int customerId)
        {
            int minOrderValue = Convert.ToInt32(ConfigurationManager.AppSettings["DistributorMinOrderValue"]);
            int maxCODValue = 0;
            DistributorMinOrder distributorMinOrder = null;
            using (var db = new AuthContext())
            {
                var customer = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.Cityid, x.Warehouseid }).FirstOrDefault();
                if (customer != null && customer.Cityid.HasValue && customer.Warehouseid.HasValue)
                {
                    MongoDbHelper<DistributorMinOrder> mongoDbHelper = new MongoDbHelper<DistributorMinOrder>();
                    var cartPredicate = PredicateBuilder.New<DistributorMinOrder>(x => x.CityId == customer.Cityid);
                    distributorMinOrder = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "DistributorMinOrder").FirstOrDefault();
                    if (distributorMinOrder != null)
                    {
                        minOrderValue = distributorMinOrder.MinOrderValue;
                        maxCODValue = distributorMinOrder.MaxCODValue;
                    }
                    else
                    {
                        distributorMinOrder = new DistributorMinOrder
                        {
                            CityId = customer.Cityid.Value,
                            WarehouseId = customer.Warehouseid.Value,
                            MinOrderValue = minOrderValue,
                            MaxCODValue = maxCODValue
                        };
                        var result = await mongoDbHelper.InsertAsync(distributorMinOrder);
                    }
                }
            }
            return distributorMinOrder;
        }

        [Route("RDSPayNow")]
        [HttpGet]
        public async Task<RDSResponse> PayNow(int orderId, int customerId)
        {

            RDSResponse rDSResponse = new RDSResponse { Status = false, message = "" };
            //rDSResponse = new RDSResponse { Status = false, message = " गुलक से पेमेंट स्वीकार नहीं कर रहे हैं। असुविधा के लिए खेद है।" };
            //return rDSResponse;

            if (!OrderToProcess.Any(x => x == orderId))
            {
                OrderToProcess.Add(orderId);
                using (var context = new AuthContext())
                {
                    var customerGullak = context.GullakDB.FirstOrDefault(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    var cashPayments = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == orderId && z.PaymentFrom == "Cash"
                                                                && z.status == "Success").ToList();
                    var order = context.DbOrderMaster.Where(x => x.OrderId == orderId).Include(x => x.orderDetails).FirstOrDefault();
                    if (cashPayments != null && cashPayments.Count > 0 && order != null)
                    {
                        if (cashPayments.Sum(x => x.amount) > 0)
                        {
                            if (customerGullak == null || customerGullak.TotalAmount < cashPayments.Sum(x => x.amount))
                            {
                                rDSResponse = new RDSResponse { Status = false, message = "Insufficient fund in your gullak please add money to your gullak." };
                            }
                            else
                            {
                                foreach (var cash in cashPayments)
                                {
                                    cash.status = "Failed";
                                    cash.statusDesc = "Due to Pay using Pay now button";
                                    context.Entry(cash).State = EntityState.Modified;
                                }

                                var GullakPaymentResponseRetailerAppDbs =
                                                 new GenricEcommers.Models.PaymentResponseRetailerApp
                                                 {
                                                     amount = cashPayments.Sum(x => x.amount),
                                                     CreatedDate = DateTime.Now,
                                                     currencyCode = "INR",
                                                     OrderId = orderId,
                                                     PaymentFrom = "Gullak",
                                                     status = "Success",
                                                     UpdatedDate = DateTime.Now,
                                                     IsRefund = false,
                                                     statusDesc = "Due to pay using pay now button",
                                                     IsOnline = true,
                                                     GatewayRequest = "Gullak",
                                                     GatewayTransId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                                                     GatewayOrderId = customerGullak.Id.ToString()
                                                 };
                                context.PaymentResponseRetailerAppDb.Add(GullakPaymentResponseRetailerAppDbs);

                                context.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                                {
                                    CreatedDate = indianTime,
                                    CreatedBy = customerId,
                                    Comment = "Pay now : " + orderId.ToString(),
                                    Amount = (-1) * cashPayments.Sum(x => x.amount),
                                    GullakId = customerGullak.Id,
                                    CustomerId = customerId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ObjectId = orderId.ToString(),
                                    ObjectType = "Order"
                                }); ;

                                customerGullak.TotalAmount -= cashPayments.Sum(x => x.amount);
                                customerGullak.ModifiedBy = customerGullak.CustomerId;
                                customerGullak.ModifiedDate = indianTime;
                                context.Entry(customerGullak).State = EntityState.Modified;
                                if (context.Commit() > 0)
                                {
                                    if (order.Status == "InTransit" || order.Status == "Payment Pending")
                                    {
                                        order.Status = "Pending";
                                        order.UpdatedDate = DateTime.Now;
                                        order.orderDetails.ToList().ForEach(x => x.status = "Pending");
                                        context.Entry(order).State = EntityState.Modified;
                                        context.Commit();
                                    }
                                    rDSResponse = new RDSResponse { Status = true, message = "Successfully pay using paynow." };
                                }
                            }
                        }
                        else
                        {
                            rDSResponse = new RDSResponse { Status = false, message = "Payment already in process" };
                        }
                    }
                    else if (order != null && order.GrossAmount > 0)
                    {
                        if (customerGullak == null || customerGullak.TotalAmount < order.GrossAmount)
                        {
                            rDSResponse = new RDSResponse { Status = false, message = "Insufficient fund in your gullak please add money to your gullak." };
                        }
                        else
                        {
                            var GullakPaymentResponseRetailerAppDbs =
                                             new GenricEcommers.Models.PaymentResponseRetailerApp
                                             {
                                                 amount = order.GrossAmount,
                                                 CreatedDate = DateTime.Now,
                                                 currencyCode = "INR",
                                                 OrderId = orderId,
                                                 PaymentFrom = "Gullak",
                                                 status = "Success",
                                                 UpdatedDate = DateTime.Now,
                                                 IsRefund = false,
                                                 statusDesc = "Due to pay using pay now button",
                                                 IsOnline = true,
                                                 GatewayRequest = "Gullak",
                                                 GatewayTransId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                                                 GatewayOrderId = customerGullak.Id.ToString()
                                             };
                            context.PaymentResponseRetailerAppDb.Add(GullakPaymentResponseRetailerAppDbs);

                            context.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                            {
                                CreatedDate = indianTime,
                                CreatedBy = customerId,
                                Comment = "Pay now : " + orderId.ToString(),
                                Amount = (-1) * order.GrossAmount,
                                GullakId = customerGullak.Id,
                                CustomerId = customerId,
                                IsActive = true,
                                IsDeleted = false,
                                ObjectId = orderId.ToString(),
                                ObjectType = "Order"
                            }); ;

                            customerGullak.TotalAmount -= order.GrossAmount;
                            customerGullak.ModifiedBy = customerGullak.CustomerId;
                            customerGullak.ModifiedDate = indianTime;
                            context.Entry(customerGullak).State = EntityState.Modified;
                            if (context.Commit() > 0)
                            {
                                if (order.Status == "InTransit" || order.Status == "Payment Pending")
                                {
                                    order.Status = "Pending";
                                    order.UpdatedDate = DateTime.Now;
                                    order.orderDetails.ToList().ForEach(x => x.status = "Pending");
                                    context.Entry(order).State = EntityState.Modified;
                                    context.Commit();
                                }
                                rDSResponse = new RDSResponse { Status = true, message = "Successfully pay using paynow." };
                            }
                        }
                    }
                    else
                    {
                        rDSResponse = new RDSResponse { Status = false, message = "Payment already in process" };
                    }
                }
                OrderToProcess.RemoveAll(x => x == orderId);
            }
            else
            {
                rDSResponse = new RDSResponse { Status = false, message = "Order payment in process" };
            }
            return rDSResponse;
        }


        #region DOM


        [Route("Genotp")]
        [HttpGet]
        [AllowAnonymous]
        public OTP Getotp(string MobileNumber, bool type, string mode = "")
        {
            string Apphash = "";
            bool TestUser = false;
            OTP b = new OTP();

            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
            //string OtpMessage = " is Your Shopkirana Verification Code. :)";

            if (string.IsNullOrEmpty(Apphash))
            {
                Apphash = ConfigurationManager.AppSettings["Apphash"];
            }

            //string OtpMessage = string.Format("<#> {0} : is Your Shopkirana Verification Code for complete process.{1}{2} Shopkirana", sRandomOTP, Environment.NewLine, Apphash);
            string OtpMessage = ""; //{#var1#} : is Your Verification Code for complete process. {#var2#} ShopKirana
            //var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Customer_Verification_Code_AutoRead");
            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "Customer_Verification_Code");
            OtpMessage = dltSMS == null ? "" : dltSMS.Template;
            OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
            OtpMessage = OtpMessage.Replace("{#var2#}", ":)");
            //string message = sRandomOTP + " :" + OtpMessage;
            // string message = OtpMessage;
            var status = dltSMS == null ? false : Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

            MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
            Model.CustomerOTP.RetailerCustomerOTP CustomerOTP = new Model.CustomerOTP.RetailerCustomerOTP
            {
                CreatedDate = DateTime.Now,
                DeviceId = "",
                IsActive = true,
                Mobile = MobileNumber,
                Otp = sRandomOTP
            };
            mongoDbHelper.Insert(CustomerOTP);


            OTP a = new OTP()
            {
                OtpNo = TestUser || (!string.IsNullOrEmpty(mode) && mode == "debug") ? sRandomOTP : "",
                Status = true,
                Message = "Successfully sent OTP."
            };
            return a;
        }



        [Route("CheckOTP")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> CheckOTP(RDSCustomerOTPVerified otpCheckDc)
        {
            MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
            var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == otpCheckDc.MobileNumber);
            Customer cust = null;
            var CustomerOTPs = mongoDbHelper.Select(cartPredicate).ToList();
            if (CustomerOTPs != null && CustomerOTPs.Any(x => x.Otp == otpCheckDc.Otp))
            {
                foreach (var item in CustomerOTPs)
                {
                    await mongoDbHelper.DeleteAsync(item.Id);
                }

                using (var context = new AuthContext())
                {
                    cust = context.Customers.Where(x => x.Deleted == false && x.Mobile == otpCheckDc.MobileNumber).FirstOrDefault();
                    if (cust != null)
                    {
                        if (context.DistributorVerificationDB.Any(x => x.CustomerID == cust.CustomerId && x.IsActive))
                        {
                            var registeredApk = context.GetAPKUserAndPwd("DistributorApp");
                            cust.RegisteredApk = registeredApk;
                        }
                        else
                        {
                            cust = null;
                            var res1 = new
                            {
                                Customers = cust,
                                Status = false,
                                Message = "Yor are not authorize to access this app."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res1);

                        }
                    }
                }

                var res = new
                {
                    Customers = cust,
                    Status = true,
                    Message = "OTP Verify Successfully."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);

            }
            else
            {
                var res = new
                {
                    Customers = cust,
                    Status = false,
                    Message = "Please enter correct OTP."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        [Route("RegistorDONCustomer")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> RegistorDONCustomer(RDSCustomerRegistor registorCustomer)
        {
            WarehouseMinDc warehouseMinDc = null;

            using (var context = new AuthContext())
            {
                var cust = await context.Customers.Where(x => x.Deleted == false && x.Mobile == registorCustomer.MobileNumber).FirstOrDefaultAsync();
                //var WPIN = await context.DONPinCodeDB.Where(x => x.IsActive && x.PinCode == registorCustomer.ZipCode).FirstOrDefaultAsync();

                if (cust != null)
                {
                    cust.ShopName = registorCustomer.ShopName;
                    cust.Shopimage = registorCustomer.Shopimage;
                    cust.Mobile = registorCustomer.MobileNumber;
                    cust.Active = false;
                    cust.Deleted = false;
                    cust.CreatedDate = indianTime;
                    cust.lat = registorCustomer.lat;
                    cust.lg = registorCustomer.lg;
                    cust.Shoplat = registorCustomer.lat;
                    cust.Shoplg = registorCustomer.lg;
                    cust.ShippingCity = registorCustomer.City;
                    cust.ZipCode = registorCustomer.ZipCode.ToString();
                    cust.State = registorCustomer.State;
                    cust.Name = registorCustomer.Name;
                    cust.ShippingAddress = registorCustomer.ShippingAddress;
                    cust.IsKPP = true;
                    cust.CustomerAppType = 4; //tejas 
                    cust.IsKPP = true;
                    cust.CustomerAppType = 4; //tejas 
                    cust.InRegion = false;
                    cust.IsCityVerified = false;
                    cust.IsSignup = false;
                    cust.AreaName = registorCustomer.AreaName;
                    cust.CustomerType = "Direct Open Network";

                    if (registorCustomer.lat != 0 && registorCustomer.lg != 0 && warehouseMinDc == null)
                    {
                        var query = new StringBuilder("Exec GetWarehouseNearyou ").Append(registorCustomer.lat).Append(",").Append(registorCustomer.lg).Append(" ");
                        warehouseMinDc = context.Database.SqlQuery<WarehouseMinDc>(query.ToString()).FirstOrDefault();
                    }

                    if (warehouseMinDc != null)
                    {
                        var cluster = context.Clusters.Where(x => x.WarehouseId == warehouseMinDc.WarehouseId && x.Active && !x.Deleted).FirstOrDefault();

                        //    if (WPIN != null)
                        //{
                        //    var cluster = await context.Clusters.FirstOrDefaultAsync(x => x.ClusterId == WPIN.DefaultClusterId);
                        cust.Warehouseid = cluster.WarehouseId;
                        cust.WarehouseName = cluster.WarehouseName;
                        cust.ClusterId = cluster.ClusterId;
                        cust.ClusterName = cluster.ClusterName;
                        cust.Cityid = cluster.CityId;
                        cust.City = cluster.CityName;
                        cust.IsCityVerified = true;
                        cust.InRegion = true;
                        cust.IsSignup = true;
                    }
                    else
                        cust.InRegion = false;

                    context.Entry(cust).State = EntityState.Modified;

                }
                else
                {
                    cust = new Customer();
                    cust.Skcode = skcode();
                    cust.ShopName = registorCustomer.ShopName;
                    cust.Shopimage = registorCustomer.Shopimage;
                    cust.Mobile = registorCustomer.MobileNumber;
                    cust.Active = false;
                    cust.Deleted = false;
                    cust.CreatedDate = indianTime;
                    cust.lat = registorCustomer.lat;
                    cust.lg = registorCustomer.lg;
                    cust.Shoplat = registorCustomer.lat;
                    cust.Shoplg = registorCustomer.lg;
                    cust.ShippingCity = registorCustomer.City;
                    cust.Name = registorCustomer.Name;
                    cust.ShippingAddress = registorCustomer.ShippingAddress;
                    cust.AreaName = registorCustomer.AreaName;
                    cust.CustomerType = "Direct Open Network";
                    cust.IsKPP = true;
                    cust.CustomerAppType = 4; //tejas 
                    cust.InRegion = false;
                    cust.IsCityVerified = false;
                    cust.IsSignup = false;
                    cust.UpdatedDate = indianTime;
                    cust.CompanyId = 1;


                    if (registorCustomer.lat != 0 && registorCustomer.lg != 0 && warehouseMinDc == null)
                    {
                        var query = new StringBuilder("Exec GetWarehouseNearyou ").Append(registorCustomer.lat).Append(",").Append(registorCustomer.lg).Append(" ");
                        warehouseMinDc = context.Database.SqlQuery<WarehouseMinDc>(query.ToString()).FirstOrDefault();
                    }

                    if (warehouseMinDc != null)
                    {
                        var cluster = context.Clusters.Where(x => x.WarehouseId == warehouseMinDc.WarehouseId && x.Active && !x.Deleted).FirstOrDefault();

                        //    if (WPIN != null)
                        //{
                        //    var cluster = await context.Clusters.FirstOrDefaultAsync(x => x.ClusterId == WPIN.DefaultClusterId);
                        cust.Warehouseid = cluster.WarehouseId;
                        cust.WarehouseName = cluster.WarehouseName;
                        cust.ClusterId = cluster.ClusterId;
                        cust.ClusterName = cluster.ClusterName;
                        cust.Cityid = cluster.CityId;
                        cust.City = cluster.CityName;
                        cust.IsCityVerified = true;
                        cust.InRegion = true;
                        cust.IsSignup = true;
                    }
                    else
                        cust.InRegion = false;
                    context.Customers.Add(cust);
                }
                context.Commit();
                var custGullak = context.GullakDB.FirstOrDefault(x => x.CustomerId == cust.CustomerId);
                if (custGullak == null)
                {
                    var gullak = new Gullak
                    {
                        CreatedBy = 1,
                        CreatedDate = DateTime.Now,
                        CustomerId = cust.CustomerId,
                        IsActive = true,
                        IsDeleted = false,
                        ModifiedBy = 1,
                        ModifiedDate = DateTime.Now,
                        TotalAmount = 0
                    };
                    context.GullakDB.Add(gullak);
                }
                var dv = context.DistributorVerificationDB.FirstOrDefault(x => x.CustomerID == cust.CustomerId);
                if (dv == null)
                {
                    dv = new DistributorVerification();
                    dv.CustomerID = cust.CustomerId;
                    dv.TypeOfBusiness = "Direct Open Network";
                    dv.Status = "Signup Complete";
                    dv.FirmName = cust.ShopName;
                    dv.CreatedDate = DateTime.Now;
                    dv.CreatedBy = cust.CustomerId;
                    dv.IsActive = true;
                    dv.IsDeleted = false;
                    context.DistributorVerificationDB.Add(dv);
                    context.Commit();
                }
                var registeredApk = context.GetAPKUserAndPwd("DistributorApp");
                cust.RegisteredApk = registeredApk;

                var res = new
                {
                    Customer = cust,
                    Status = warehouseMinDc != null ? true : false,
                    Message = warehouseMinDc != null ? "Your registation process is completed" : "Oops! We are currently not delivering in your area! " + Environment.NewLine + "We hope to serve you soon.Thank You!",
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [Route("UpdateDONCustomer")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> UpdateDONCustomer(RDSCustomerAddress customerAddress)
        {
            using (var context = new AuthContext())
            {
                var cust = await context.Customers.Where(x => x.CustomerId == customerAddress.CustomerId).FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(customerAddress.RefNo))
                {
                    var gstCheck = context.Customers.Any(x => x.CustomerId != customerAddress.CustomerId && x.RefNo == customerAddress.RefNo);
                    if (gstCheck)
                    {
                        cust = null;
                        var rs = new
                        {
                            Customer = cust,
                            Status = false,
                            Message = "GST Already Exists."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, rs);
                    }
                }

                if (!string.IsNullOrEmpty(customerAddress.LicenseNumber.Trim()) && customerAddress.LicenseNumber != "0")
                {
                    var checkgst = context.Customers.Where(x => x.LicenseNumber == customerAddress.LicenseNumber && x.CustomerId != customerAddress.CustomerId).Count();
                    if (checkgst > 0)
                    {
                        var rs = new
                        {
                            Customer = cust,
                            Status = false,
                            Message = "License Number Already Exsits."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, rs);
                    }
                }
                #region for Duplicate PAN no. check
                if (!string.IsNullOrEmpty(customerAddress.PanNo.Trim()))
                {
                    Customer IsExistPAN = context.Customers.Where(c => c.PanNo.ToLower() == customerAddress.PanNo.ToLower()).FirstOrDefault();
                    if (IsExistPAN != null)
                    {
                        var rs = new
                        {
                            Customer = cust,
                            Status = false,
                            Message = "PAN Already Exists."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, rs);

                    }
                }
                #endregion
                if (cust != null)
                {
                    cust.RefNo = customerAddress.RefNo;
                    cust.UploadGSTPicture = customerAddress.UploadGSTPicture;
                    cust.AadharNo = customerAddress.AadharNo;
                    cust.PanNo = customerAddress.PanNo;
                    cust.LicenseNumber = customerAddress.LicenseNumber;
                    cust.UploadLicensePicture = customerAddress.UploadLicensePicture;

                    if (!string.IsNullOrEmpty(customerAddress.RefNo))
                    {
                        var custGstVerifys = context.CustGSTverifiedRequestDB.Where(x => x.RefNo == customerAddress.RefNo).ToList();
                        if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                        {
                            var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                            var state = context.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                            cust.BillingCity = gstVerify.City;
                            cust.BillingState = state != null ? state.StateName : gstVerify.State;
                            cust.BillingZipCode = gstVerify.Zipcode;
                            cust.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                            cust.NameOnGST = gstVerify.Name;
                        }
                    }


                    if ((!string.IsNullOrEmpty(cust.LicenseNumber) && customerAddress.CustomerDocTypeMasterId > 0) || (!string.IsNullOrEmpty(cust.RefNo) && !string.IsNullOrEmpty(cust.UploadGSTPicture)))
                    {
                        var customerdocs = context.CustomerDocs.Where(x => x.CustomerId == cust.CustomerId).ToList();
                        var docMaster = context.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();
                        if (!string.IsNullOrEmpty(cust.RefNo) && !string.IsNullOrEmpty(cust.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                        {
                            var docid = docMaster.FirstOrDefault(x => x.DocType == "GST").Id;
                            if (customerdocs.Any(x => x.CustomerDocTypeMasterId == docid && x.IsActive))
                            {
                                var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == docid && x.IsActive);
                                custdoc.ModifiedBy = 0;
                                custdoc.ModifiedDate = DateTime.Now;
                                custdoc.IsActive = false;
                                context.Entry(custdoc).State = EntityState.Modified;
                            }
                            context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = cust.CustomerId,
                                IsActive = true,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = docid,
                                DocPath = cust.UploadGSTPicture,
                                IsDeleted = false
                            });
                        }

                        if (!string.IsNullOrEmpty(cust.LicenseNumber) && customerAddress.CustomerDocTypeMasterId > 0)
                        {
                            if (customerdocs.Any(x => x.CustomerDocTypeMasterId == cust.CustomerDocTypeMasterId && x.IsActive))
                            {
                                var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == cust.CustomerDocTypeMasterId && x.IsActive);
                                custdoc.ModifiedBy = 0;
                                custdoc.ModifiedDate = DateTime.Now;
                                custdoc.IsActive = false;
                                context.Entry(custdoc).State = EntityState.Modified;
                            }
                            context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = cust.CustomerId,
                                IsActive = true,
                                CreatedBy = 0,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = customerAddress.CustomerDocTypeMasterId,
                                DocPath = cust.UploadRegistration,
                                IsDeleted = false
                            });
                        }

                    }


                    var dv = context.DistributorVerificationDB.FirstOrDefault(x => x.CustomerID == cust.CustomerId);
                    if (dv != null)
                    {
                        dv.Aadhar = customerAddress.AadharPicture;
                        dv.PAN = customerAddress.PanPicture;
                        dv.AadharNo = customerAddress.AadharNo;
                        dv.PANNo = customerAddress.PanNo;
                        dv.ModifiedDate = DateTime.Now;
                        dv.ModifiedBy = cust.CustomerId;
                        dv.GSTNo = customerAddress.RefNo;
                        dv.Status = "Documents Uploaded";
                        context.Entry(dv).State = EntityState.Modified;
                    }

                    context.Entry(cust).State = EntityState.Modified;
                }
                context.Commit();

                var registeredApk = context.GetAPKUserAndPwd("DistributorApp");
                cust.RegisteredApk = registeredApk;

                var res = new
                {
                    Customer = cust,
                    Status = true,
                    Message = "Customer Detail Update Successfully.",
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
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
                if (lat.HasValue && lg.HasValue && lat.Value > 0 && lg.Value > 0 && (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 19) && !context.CustomerLocations.Any(x => x.CustomerId == CustomerId && EntityFunctions.TruncateTime(x.CreatedDate) == todayDate))
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
                var customerBrandIds = context.CustomerBrandAcessDB.Where(x => x.CustomerId == CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.BrandId).ToList();
                List<AppHomeSectionsDsc> sections = new List<AppHomeSectionsDsc>();
                List<FinBoxConfig> FinBoxConfigs = null;
                var datenow = indianTime;
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
                                    y.OfferStartTime = y.OfferStartTime.Value;
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
                                if (customerBrandIds != null && customerBrandIds.Any())
                                {
                                    x.AppItemsList = x.AppItemsList.Where(t => !(customerBrandIds.Select(z => z).Contains(t.SubsubCategoryId))).ToList();
                                }
                            }
                            else if (x.SectionSubType == "Item")
                            {
                                if (customerBrandIds != null && customerBrandIds.Any())
                                {
                                    x.AppItemsList = x.AppItemsList.Where(t => !(customerBrandIds.Select(z => z).Contains(t.SubsubCategoryId))).ToList();
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
                                if (customerBrandIds != null && customerBrandIds.Any())
                                {
                                    x.AppItemsList = x.AppItemsList.Where(t => !(customerBrandIds.Select(z => z).Contains(t.SubsubCategoryId))).ToList();
                                }
                            }
                            else if (x.SectionSubType == "Item")
                            {
                                if (customerBrandIds != null && customerBrandIds.Any())
                                {
                                    x.AppItemsList = x.AppItemsList.Where(t => !(customerBrandIds.Select(z => z).Contains(t.SubsubCategoryId))).ToList();
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



        [Route("GetItemBySectionV2")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetItemBySectionV2(int Warehouseid, int sectionid, int customerId, string lang)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    //List<string> itemids = new List<string>(new string[] { "AE1011A",  "AE1013A" });
                    DateTime CurrentDate = DateTime.Now;
                    var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                    if (data != null)
                    {
                        List<int> ids = data.Select(x => x.SectionItemID).ToList();

                        var inActiveCustomer = context.Customers.Any(x => x.CustomerId == customerId && (x.Active == false || x.Deleted == true));
                        List<WRSITEM> brandItem = new List<WRSITEM>();
                        WRSITEMN item = new WRSITEMN();
                        //foreach (var itemid in data)
                        var customerBrandIds = context.CustomerBrandAcessDB.Where(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.BrandId).ToList();

                        var itemids = data.Select(x => x.ItemId).ToList();
                        if (itemids != null && itemids.Any())
                        {
                            var newdata = (from a in context.itemMasters
                                           where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId) && a.DistributorShow == true)
                                            && (a.ItemAppType == 0 || a.ItemAppType == 2)
                                           //join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                           let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                           select new factoryItemdat
                                           {
                                               WarehouseId = a.WarehouseId,
                                               IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                               ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                               CompanyId = a.CompanyId,
                                               Categoryid = a.Categoryid,
                                               Discount = a.Discount,
                                               ItemId = a.ItemId,
                                               ItemNumber = a.Number,
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
                                               HindiName = a.HindiName,
                                               VATTax = a.VATTax,
                                               active = a.active,
                                               dreamPoint = 0,
                                               marginPoint = a.marginPoint,
                                               NetPurchasePrice = a.NetPurchasePrice,
                                               promoPerItems = a.promoPerItems,
                                               IsOffer = a.IsOffer,
                                               Deleted = a.Deleted,
                                               IsSensitive = a.IsSensitive,
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
                                               DistributionPrice = a.DistributionPrice,
                                               DistributorShow = a.DistributorShow,
                                               IsSensitiveMRP = a.IsSensitiveMRP
                                           }).ToList();

                            newdata = newdata.OrderByDescending(x => x.ItemNumber).ToList();

                            var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == Warehouseid && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems).SelectMany(x => x.OfferFreeItems).ToList();


                            foreach (var it in newdata)
                            {
                                it.IsNotSell = true;
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;

                                #region old code for itemnumber
                                //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                                //{
                                //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                                //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                //    {
                                //        it.IsOffer = true;
                                //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                //        it.OfferId = freeItemoffer.offerId;
                                //        it.OfferType = freeItemoffer.OfferType;
                                //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                //    }
                                //}
                                #endregion

                                #region code changes for sellingsku
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                    {
                                        var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                        if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                        {
                                            it.IsOffer = true;
                                            it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                            it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                            it.OfferId = freeItemoffer.offerId;
                                            it.OfferType = freeItemoffer.OfferType;
                                            it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                            it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                            //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                            it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                            it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                            it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                            it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                        }
                                    }
                                    else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                    {
                                        var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                        if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                        {
                                            it.IsOffer = true;
                                            it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                            it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                            it.OfferId = freeItemoffer.offerId;
                                            it.OfferType = freeItemoffer.OfferType;
                                            it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                            it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                            //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                            it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                            it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                            it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                            it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                        }
                                    }
                                }
                                #endregion

                                if (customerBrandIds != null && customerBrandIds.Any())
                                {
                                    if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                    {
                                        it.IsNotSell = false;
                                    }
                                }
                                if (item.ItemMasters == null)
                                {
                                    item.ItemMasters = new List<factoryItemdat>();
                                }

                                if (lang.Trim() == "hi")
                                {
                                    if (it.IsSensitive == true)
                                    {
                                        if (it.IsSensitiveMRP == false)
                                        {
                                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                        }
                                        else
                                        {
                                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                        }
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                    }
                                }

                                item.ItemMasters.Add(it);

                            }
                        }
                        if (item.ItemMasters != null)
                        {
                            var res = new
                            {

                                ItemMasters = item.ItemMasters,
                                Status = true,
                                Message = "Success."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        else
                        {
                            Array[] l = new Array[] { };
                            var res = new
                            {
                                ItemMasters = l,
                                Status = false,
                                Message = "fail"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        Array[] l = new Array[] { };
                        var res = new
                        {
                            ItemMasters = l,
                            Status = false,
                            Message = "fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                catch (Exception ee)
                {
                    throw;
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


        [Route("SubCategoryDON")]
        public async Task<customeritem> SubCategoryDON(int warehouseid, int subCategoryId, string lang)
        {
            var manager = new KPPAppManager();
            var customerInsert = await manager.getItemMasterv2(warehouseid, lang);
            if (customerInsert != null)
            {
                customerInsert.SubCategories = customerInsert.SubCategories.Where(x => x.SubCategoryId == subCategoryId).ToList();
                if (customerInsert.SubCategories.Any())
                {
                    customerInsert.Categories = customerInsert.Categories.Where(x => customerInsert.SubCategories.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();
                    customerInsert.SubSubCategories = customerInsert.SubSubCategories.Where(x => customerInsert.SubCategories.Select(y => y.SubCategoryId).Contains(x.SubCategoryId)).ToList();
                }
            }
            return customerInsert;
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
                cmd.CommandText = "[dbo].[GetAllStoreForDON]";
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


            }
            return retailerStore;
        }

        [Route("GetAllDonItem")]
        [HttpGet]
        public async Task<DonItems> GetAllDonItem(int customerId, int warehouseId, int skip, int take, string lang)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var customerBrandIds = context.CustomerBrandAcessDB.Where(x => x.CustomerId == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.BrandId).ToList();

                    DonItems item = new DonItems();

                    if (lang.Trim() == "hi")
                    {
                        //Increase some parameter For offer
                        var newdatahi = (from a in context.itemMasters
                                         where (a.WarehouseId == warehouseId && a.Deleted == false && a.active == true && a.DistributorShow == true && (a.ItemAppType == 0 || a.ItemAppType == 2))
                                         join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                         let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                         select new factoryItemdat
                                         {
                                             WarehouseId = a.WarehouseId,
                                             IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                             ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                             CompanyId = a.CompanyId,
                                             Categoryid = b.Categoryid,
                                             Discount = b.Discount,
                                             ItemId = a.ItemId,
                                             ItemNumber = b.Number,
                                             itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                             LogoUrl = b.LogoUrl,
                                             MinOrderQty = b.MinOrderQty,
                                             price = a.price,
                                             SubCategoryId = b.SubCategoryId,
                                             SubsubCategoryid = b.SubsubCategoryid,
                                             TotalTaxPercentage = b.TotalTaxPercentage,
                                             SellingUnitName = a.SellingUnitName,
                                             SellingSku = b.SellingSku,
                                             UnitPrice = a.UnitPrice,
                                             HindiName = a.HindiName,
                                             VATTax = b.VATTax,
                                             active = a.active,
                                             dreamPoint = 0,
                                             marginPoint = a.marginPoint,
                                             NetPurchasePrice = a.NetPurchasePrice,
                                             promoPerItems = a.promoPerItems,
                                             IsOffer = a.IsOffer,
                                             Deleted = a.Deleted,
                                             IsSensitive = b.IsSensitive,
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
                                             DistributionPrice = a.DistributionPrice,
                                             DistributorShow = a.DistributorShow,
                                             IsSensitiveMRP = a.IsSensitiveMRP
                                         }).OrderByDescending(x => x.ItemNumber).Skip(skip).Take(take).ToList();

                        var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == warehouseId && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems).SelectMany(x => x.OfferFreeItems).ToList();


                        foreach (factoryItemdat it in newdatahi)
                        {
                            it.IsNotSell = true;
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                            #region old code for itemnumber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                            //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion

                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion

                            if (customerBrandIds != null && customerBrandIds.Any())
                            {
                                if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                {
                                    it.IsNotSell = false;
                                }
                            }
                            if (item.ItemDataDCs == null)
                            {
                                item.ItemDataDCs = new List<factoryItemdat>();
                            }

                            if (lang.Trim() == "hi")
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }
                            }

                            item.ItemDataDCs.Add(it);
                        }

                    }
                    else
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == warehouseId && a.Deleted == false && a.active == true && a.DistributorShow == true && (a.ItemAppType == 0 || a.ItemAppType == 2))
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                       select new factoryItemdat
                                       {
                                           WarehouseId = a.WarehouseId,
                                           IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                           ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
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
                                           DistributionPrice = a.DistributionPrice,
                                           DistributorShow = a.DistributorShow
                                       }).OrderByDescending(x => x.ItemNumber).Skip(skip).Take(take).ToList();

                        var freeItemoffers = context.OfferDb.Where(x => x.OfferAppType == "Distributor App" && x.IsActive && !x.IsDeleted && x.start <= DateTime.Now && x.end >= DateTime.Now && x.WarehouseId == warehouseId && x.OfferFreeItems.Any()).Include(x => x.OfferFreeItems).SelectMany(x => x.OfferFreeItems).ToList();

                        foreach (factoryItemdat it in newdata)
                        {
                            it.IsNotSell = true;
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                            #region old code for itemnumber
                            //if (freeItemoffers.Any(x => x.ItemNumber == it.ItemNumber))
                            //{
                            //    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.ItemNumber == it.ItemNumber);
                            //    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                            //    {
                            //        it.IsOffer = true;
                            //        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                            //        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                            //        it.OfferId = freeItemoffer.offerId;
                            //        it.OfferType = freeItemoffer.OfferType;
                            //        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                            //        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                            //        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                            //        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                            //        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                            //        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                            //        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                            //    }
                            //}
                            #endregion

                            #region code changes for sellingsku
                            if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId) || freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                            {
                                if (freeItemoffers.Any(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "Item" && x.ItemNumber == it.ItemNumber && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                                else if (freeItemoffers.Any(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId))
                                {
                                    var freeItemoffer = freeItemoffers.FirstOrDefault(x => x.OfferOn == "SellingSku" && x.ItemNumber == it.SellingSku && x.ItemMultiMRPId == it.ItemMultiMRPId);
                                    if (context.itemMasters.Any(x => x.ItemId == freeItemoffer.FreeItemId && !x.Deleted))
                                    {
                                        it.IsOffer = true;
                                        it.OfferQtyAvaiable = freeItemoffer.OfferQtyAvaiable;
                                        it.OfferQtyConsumed = freeItemoffer.OfferQtyConsumed;
                                        it.OfferId = freeItemoffer.offerId;
                                        it.OfferType = freeItemoffer.OfferType;
                                        it.OfferWalletPoint = freeItemoffer.OfferWalletPoint;
                                        it.OfferFreeItemId = freeItemoffer.FreeItemId;
                                        //it.OfferPercentage = freeItemoffer.OfferPercentage;
                                        it.OfferFreeItemName = freeItemoffer.OfferFreeItemName;
                                        it.OfferFreeItemImage = freeItemoffer.OfferFreeItemImage;
                                        it.OfferFreeItemQuantity = freeItemoffer.OfferFreeItemQuantity;
                                        it.OfferMinimumQty = freeItemoffer.OfferMinimumQty;
                                    }
                                }
                            }
                            #endregion

                            if (customerBrandIds != null && customerBrandIds.Any())
                            {
                                if (!customerBrandIds.Any(x => it.SubsubCategoryid == x))
                                {
                                    it.IsNotSell = false;
                                }
                            }
                            if (item.ItemDataDCs == null)
                            {
                                item.ItemDataDCs = new List<factoryItemdat>();
                            }
                            item.ItemDataDCs.Add(it);
                        }
                    }
                    return new DonItems() { ItemDataDCs = item.ItemDataDCs, TotalItem = item.ItemDataDCs.Count };
                }
                catch (Exception ex)
                {
                    return new DonItems() { ItemDataDCs = null, TotalItem = 0 };
                }
            }
        }
        #endregion
    }

    public class DonItems
    {
        public List<factoryItemdat> ItemDataDCs { get; set; }
        public int TotalItem { get; set; }
    }
}