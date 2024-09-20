using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Transaction.Customer;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ChatbotAPI")]
    [AllowAnonymous]
    public class WhatsAppChatbotController : ApiController
    {
        [Route("GetWorkingCity")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetWorkingCity()
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                List<string> citylst = new List<string>();
                var res = new
                {
                    CityList = citylst,
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            using (var context = new AuthContext())
            {
                var citylst = await (from w in context.Warehouses
                                     where w.IsKPP == false && !w.WarehouseName.Contains("Test")
                                     join pw in context.GMWarehouseProgressDB
                                     on w.WarehouseId equals pw.WarehouseID
                                     where pw.IsLaunched == true
                                     select new { w.CityName, w.Cityid }).ToListAsync();

                var CityDCs = citylst.Select(x => new CityDC { Cityid = x.Cityid, CityName = x.CityName }).Distinct().ToList();
                int i = 1;
                CityDCs.ForEach(x =>
                {
                    x.SequenceNo = i;
                    x.CityName = StringHelper.ToTitleCase(x.CityName);
                    i++;
                });

                var res = new
                {
                    CityList = CityDCs.Distinct(),
                    Message = "",
                    Status = true
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }


        [Route("GetCustomerDetail")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCustomerDetail(string mobile)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                List<string> citylst = new List<string>();
                var res = new
                {
                    Name = "",
                    SkCode = "",
                    CustomerId = 0,
                    IsVerified = false,
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            if (!string.IsNullOrEmpty(mobile) && mobile.Length > 10)
            {
                mobile = mobile.Substring(mobile.Length - 10);
            }
            using (var context = new AuthContext())
            {
                var customer = await context.Customers.Where(x => x.Mobile == mobile.Trim() && x.Deleted == false).Select(x => new { x.Name, x.ShopName, x.Skcode, x.Active, x.CustomerVerify, x.CustomerId }).FirstOrDefaultAsync();

                if (customer != null)
                {
                    var res = new
                    {
                        Name = string.IsNullOrEmpty(customer.Name) ? customer.ShopName : customer.Name,
                        SkCode = customer.Skcode,
                        CustomerId = customer.CustomerId,
                        IsVerified = customer.Active && customer.CustomerVerify == "Full Verified" ? true : false,
                        Status = true,
                        Message = ""
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        Name = "",
                        SkCode = "",
                        CustomerId = 0,
                        IsVerified = false,
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        [Route("GetOrderStatus")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetOrderStatus(int OrderId, int customerId)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                List<string> citylst = new List<string>();
                var res = new
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            using (var context = new AuthContext())
            {
                var order = await context.DbOrderMaster.Where(x => x.OrderId == OrderId && x.CustomerId == customerId).Select(x => new { x.Status, x.OrderId, x.DeliveredDate, x.Deliverydate }).FirstOrDefaultAsync();

                if (order != null)
                {
                    if (order.Status == "Pending" || order.Status == "ReadyToPick")
                    {
                        var res = new
                        {
                            Message = "Your Order #" + order.OrderId + " Delivered on  " + (order.DeliveredDate.HasValue ? order.DeliveredDate.Value.ToString("dd MMM yyyy") : order.Deliverydate.ToString("dd MMM yyyy")),
                            Status = true
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (order.Status == "Ready to Dispatch" || order.Status == "Shipped" || order.Status == "Issued" || order.Status == "Delivery Redispatch")
                    {
                        var res = new
                        {
                            Message = "Your Order #" + order.OrderId + " shortly received.",
                            Status = true
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (order.Status == "Delivered" || order.Status == "sattled")
                    {
                        var res = new
                        {
                            Message = "Your Order #" + order.OrderId + " already delivered on " + order.DeliveredDate.Value.ToString("dd MMM yyyy"),
                            Status = true
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (order.Status == "Dummy Order Cancelled" || order.Status == "Post Order Canceled" || order.Status == "Delivery Canceled")
                    {
                        var res = new
                        {
                            Message = "Your Order #" + order.OrderId + " is canceled.",
                            Status = true
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else //if (order.Status == "Inactive" || order.Status == "Payment Pending" || order.Status == "Failed" || order.Status == "InTransit")
                    {
                        var res = new
                        {
                            Message = "Your Order #" + order.OrderId + " is " + order.Status + ". Please contact customer care.",
                            Status = true
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    var res = new
                    {
                        Message = "",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        [Route("DealOfTheDay")]
        [HttpGet]
        public async Task<HttpResponseMessage> DealOfTheDay(int customerId)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                var res = new
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            using (var context = new AuthContext())
            {
                var customers = context.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.Warehouseid }).FirstOrDefault();

                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 600;
                cmd.CommandText = "[dbo].[GetRendomTopMarginItem]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", customers.Warehouseid));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();

                string message = "";
                if (ItemData != null && ItemData.Any())
                {
                    message = "We have brought special products for you today \n";
                    int i = 1;
                    foreach (var item in ItemData)
                    {
                        string struom = "";
                        if (item.UOM == "Gm")
                            struom = "gram";
                        else if (item.UOM == "Kg")
                            struom = "kilogram";
                        else if (item.UOM == "Combo")
                            struom = "combo";
                        else if (item.UOM == "Ltr")
                            struom = "liter";
                        else if (item.UOM == "Ml")
                            struom = "mili liter";
                        else if (item.UOM == "Pc")
                            struom = "pieces";
                        else if (item.UOM == "Size")
                            struom = "size";

                        message += i.ToString() + ". " + item.itemBaseName + " " + item.MRP + " MRP " + (!string.IsNullOrEmpty(struom) ? (item.UnitofQuantity + " " + struom) : "") + " @ " + item.UnitPrice.ToString("#.##") + "/-";


                        message += "\n";
                        i++;
                    }
                    message += " So hurry up this rate is available for limited period only.";
                }

                var res = new
                {
                    Message = message,
                    Status = string.IsNullOrEmpty(message) ? false : true
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [Route("SpacialOffer")]
        [HttpGet]
        public async Task<HttpResponseMessage> SpacialOffer(int customerId)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                var res = new
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            List<OfferDc> FinalBillDiscount = new List<OfferDc>();
            using (AuthContext context = new AuthContext())
            {
                CustomersManager manager = new CustomersManager();

                List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetWareBillDiscount(customerId, "Retailer App");
                List<string> Offers = new List<string>();
                if (billDiscountOfferDcs.Any())
                {
                    var offerIds = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();
                    List<BillDiscountFreeItem> BillDiscountFreeItems = offerIds.Any() ? context.BillDiscountFreeItem.Where(x => offerIds.Contains(x.offerId) && x.RemainingOfferStockQty < x.OfferStockQty).ToList() : new List<BillDiscountFreeItem>();

                    foreach (var billDiscountOfferDc in billDiscountOfferDcs)
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
                            MaxDiscount = billDiscountOfferDc.MaxDiscount,
                            ApplyType = billDiscountOfferDc.ApplyType,
                            //OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new OfferBillDiscountItemDc
                            //{
                            //    CategoryId = y.CategoryId,
                            //    Id = y.Id,
                            //    IsInclude = y.IsInclude,
                            //    SubCategoryId = y.SubCategoryId
                            //}).ToList(),
                            //OfferItems = billDiscountOfferDc.OfferItems.Select(y => new OfferItemdc
                            //{
                            //    IsInclude = y.IsInclude,
                            //    itemId = y.itemId
                            //}).ToList(),
                            //RetailerBillDiscountFreeItemDcs = BillDiscountFreeItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).Select(x => new RetailerBillDiscountFreeItemDc
                            //{
                            //    ItemId = x.ItemId,
                            //    ItemName = x.ItemName,
                            //    Qty = x.Qty
                            //}).ToList(),
                            //BillDiscountRequiredItems = billDiscountOfferDc.BillDiscountRequiredItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList(),
                            //OfferLineItemValueDcs = billDiscountOfferDc.OfferLineItemValueDcs.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList()
                        };

                        if (bdcheck.BillDiscountOfferOn == "FreeItem" && bdcheck.RetailerBillDiscountFreeItemDcs.Any())
                            FinalBillDiscount.Add(bdcheck);
                        else
                            FinalBillDiscount.Add(bdcheck);

                    }

                    int i = 1;
                    foreach (var item in FinalBillDiscount)
                    {
                        string offer = i.ToString() + ". ";
                        if (item.BillDiscountOfferOn == "Percentage")
                        {
                            offer = item.DiscountPercentage + "% OFF minimum purchase of ₹" + item.BillAmount;
                        }
                        else if (item.BillDiscountOfferOn == "FreeItem")
                        {
                            offer = "Get free item on minimum purchase of ₹ " + item.BillAmount;
                        }
                        else
                        {
                            String msgPostBill = item.ApplyOn != "PostOffer" ? " " : " credited after order delivered ";
                            if (item.WalletType == "WalletPercentage")
                            {
                                offer = item.BillDiscountWallet + "% OFF minimum purchase of ₹" + item.BillAmount + msgPostBill;
                            }
                            else
                            {
                                offer = "flat ₹ " + item.BillDiscountWallet + " OFF minimum purchase of ₹" + item.BillAmount + msgPostBill;
                            }
                        }

                        offer += " " + item.Description;
                        Offers.Add(offer);
                        i++;
                    }

                    var res = new
                    {
                        Offers = Offers,
                        Message = "Today offer for you\n" + string.Join("\n", Offers),
                        Status = true
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        Offers = Offers,
                        Message = "No Offer available.",
                        Status = true
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }

        [Route("GetBusinessDocType")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetBusinessDocType()
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                List<string> citylst = new List<string>();
                var res = new
                {
                    CityList = citylst,
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            using (var context = new AuthContext())
            {
                var docMaster = context.CustomerDocTypeMasters.Where(x => x.IsActive).Select(x => new { x.DocType, x.Id }).ToList();

                var res = new
                {
                    DocTypes = docMaster,
                    Message = "",
                    Status = true
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        [Route("CustomerGSTVerify")]
        [HttpGet]
        public async Task<CustGstDTOList> GetCustomerGSTVerify(string GSTNO)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                List<string> citylst = new List<string>();
                var res = new CustGstDTOList
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return res;
            }
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
                            ShippingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", GSTdata.HomeNo, GSTdata.HomeName, GSTdata.ShippingAddress, GSTdata.City, GSTdata.State, GSTdata.Zipcode),
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
        public OTP Getotp(string MobileNumber, string mode)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                List<string> citylst = new List<string>();
                var res = new OTP
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return res;
            }

            if (!string.IsNullOrEmpty(MobileNumber) && MobileNumber.Length > 10)
            {
                MobileNumber = MobileNumber.Substring(MobileNumber.Length - 10);
            }
            bool TestUser = false;

            OTP b = new OTP();
            using (var context = new AuthContext())
            {
                Customer cust = context.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim())).FirstOrDefault();
                if (cust != null && cust.IsKPP)
                {
                    TestUser = cust.CustomerCategoryId.HasValue && cust.CustomerCategoryId.Value == 0;
                    if (context.DistributorVerificationDB.Any(x => x.CustomerID == cust.CustomerId))
                    {
                        b = new OTP()
                        {
                            OtpNo = "You are not authorize"
                        };
                        return b;
                    }
                }
            }
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
            //string OtpMessage = " is Your Shopkirana Verification Code. :)";



            //string OtpMessage = string.Format("<#> {0} : is Your Shopkirana Verification Code for complete process.{1}{2} Shopkirana", sRandomOTP, Environment.NewLine, "");
            string OtpMessage = ""; //{#var1#} : is Your Verification Code for complete process. {#var2#} ShopKirana
            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Customer_Verification_Code_AutoRead");
            OtpMessage = dltSMS == null ? "" : dltSMS.Template;            
            OtpMessage = OtpMessage.Replace("{#var1#}", "<#> {0}");
            OtpMessage = OtpMessage.Replace("{#var2#}", sRandomOTP);
            //string message = sRandomOTP + " :" + OtpMessage;
            // string message = OtpMessage;

            var status = dltSMS == null ? false: Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
            if (status)
            {
                MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
                Model.CustomerOTP.RetailerCustomerOTP CustomerOTP = new Model.CustomerOTP.RetailerCustomerOTP
                {
                    CreatedDate = DateTime.Now,
                    DeviceId = "WhatsUp",
                    IsActive = true,
                    Mobile = MobileNumber,
                    Otp = sRandomOTP
                };
                mongoDbHelper.Insert(CustomerOTP);
            }


            OTP a = new OTP()
            {
                Message = "Successfully sent OTP",
                Status = true,
                OtpNo = TestUser || (!string.IsNullOrEmpty(mode) && mode == "debug") ? sRandomOTP : "Successfully sent OTP"
            };
            return a;
        }

        [Route("CheckOTP")]
        [HttpPost]
        public async Task<HttpResponseMessage> CheckOTP(OtpCheckDc otpCheckDc)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                var res = new
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            if (otpCheckDc != null && !string.IsNullOrEmpty(otpCheckDc.MobileNumber) && otpCheckDc.MobileNumber.Length > 10)
            {
                otpCheckDc.MobileNumber = otpCheckDc.MobileNumber.Substring(otpCheckDc.MobileNumber.Length - 10);
            }
            MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
            var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == otpCheckDc.MobileNumber);
            var CustomerOTPs = mongoDbHelper.Select(cartPredicate).ToList();
            if (CustomerOTPs != null && CustomerOTPs.Any(x => x.Otp == otpCheckDc.Otp))
            {
                foreach (var item in CustomerOTPs)
                {
                    await mongoDbHelper.DeleteAsync(item.Id);
                }

                var res = new
                {
                    Status = true,
                    Message = "OTP Verify Successfully."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            else
            {
                var res = new
                {
                    Status = false,
                    Message = "Please enter correct OTP."
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

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

        [Route("BasicCustomerRegistation")]
        [HttpPost]
        public async Task<HttpResponseMessage> BasicCustomerRegistation(RegistorChatbotUser registorChatbotUser)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                var res = new
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

            if (registorChatbotUser != null && !string.IsNullOrEmpty(registorChatbotUser.MobileNumber) && registorChatbotUser.MobileNumber.Length > 10)
            {
                registorChatbotUser.MobileNumber = registorChatbotUser.MobileNumber.Substring(registorChatbotUser.MobileNumber.Length - 10);
            }
            string filename = "";
            double lt = 0, lng = 0;
            if (!string.IsNullOrEmpty(registorChatbotUser.LatlngUrl))
            {
                try
                {
                    var latlngarr = registorChatbotUser.LatlngUrl.Split(',');
                    if (latlngarr.Length == 2)
                    {
                        lt = Convert.ToDouble(latlngarr[0]);
                        lng = Convert.ToDouble(latlngarr[1]);
                    }
                    else
                    {
                        lt = 0; lng = 0;
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("WhatsChatBot: " + ex.ToString(), true);
                    lt = 0; lng = 0;
                }

                if (lt == 0 || lng == 0)
                {
                    var res = new
                    {
                        Message = "Please enter correct customer shop location.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            else
            {
                var res = new
                {
                    Message = "Please enter customer shop location.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

            if (!string.IsNullOrEmpty(registorChatbotUser.Shopimage))
            {
                filename = "ShopImage_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + registorChatbotUser.Shopimage.Substring(registorChatbotUser.Shopimage.LastIndexOf("."));
                var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);

                try
                {
                    using (WebClient Client = new WebClient())
                    {
                        Client.DownloadFile(registorChatbotUser.Shopimage, ImageUrl);
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("WhatsChatBot: " + ex.ToString(), true);
                    ImageUrl = "";
                    filename = "";
                }
                if (string.IsNullOrEmpty(filename))
                {
                    var res = new
                    {
                        Message = "Shopimage is not in correct formate.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            else
            {
                var res = new
                {
                    Message = "Shopimage is required.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

            using (var context = new AuthContext())
            {
                var customer = await context.Customers.Where(x => x.Deleted == false && x.Mobile == registorChatbotUser.MobileNumber).FirstOrDefaultAsync();

                var city = context.Cities.FirstOrDefault(x => x.CityName.ToLower() == registorChatbotUser.City.ToLower() && x.active);

                if (city == null)
                {
                    var cityres = new
                    {
                        Message = "We are not serve our services at your area.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, cityres);
                }
                if (customer != null)
                {
                    var Custres = new
                    {
                        Name = string.IsNullOrEmpty(customer.Name) ? customer.ShopName : customer.Name,
                        SkCode = customer.Skcode,
                        CustomerId = customer.CustomerId,
                        IsVerified = customer.Active && customer.CustomerVerify == "Full Verified" ? true : false,
                        Message = "You are already registor.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, Custres);

                }
                else
                {

                    customer = new Customer();

                    customer.Cityid = city.Cityid;
                    customer.State = city.StateName;
                    //WarehouseId = context.Warehouses.Where(x => x.Cityid == city.Cityid && x.Deleted == false && x.active == true).Select(x => x.WarehouseId).FirstOrDefault();

                    customer.Skcode = skcode();
                    customer.ShopName = registorChatbotUser.ShopName;
                    customer.Shopimage = filename;
                    customer.Mobile = registorChatbotUser.MobileNumber;
                    customer.Active = false;
                    customer.Deleted = false;
                    customer.CreatedDate = DateTime.Now;
                    customer.lat = lt;
                    customer.lg = lng;
                    customer.Shoplat = lt;
                    customer.Shoplg = lng;
                    //customer.ShippingCity = registorChatbotUser.City;
                    customer.Name = registorChatbotUser.Name;
                    customer.ShippingAddress = registorChatbotUser.ShippingAddress;
                    //customer.AreaName = registorChatbotUser.AreaName;
                    customer.CustomerType = "Retailer";
                    customer.IsKPP = true;
                    customer.CustomerAppType = 1; //tejas 
                    customer.InRegion = false;
                    customer.IsCityVerified = false;
                    customer.IsSignup = false;
                    customer.UpdatedDate = DateTime.Now;

                    #region to assign cluster ID and determine if it is in cluster or not.   
                    string message = "Your registation process is completed.";
                    if (customer.lat != 0 && customer.lg != 0 && customer.Cityid > 0)
                    {
                        var query = new StringBuilder("select [dbo].[GetClusterFromLatLngAndCity]('").Append(customer.lat).Append("', '").Append(customer.lg).Append("', ").Append(customer.Cityid).Append(")");
                        var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                        if (!clusterId.HasValue)
                        {
                            customer.InRegion = false;
                            message = "We are not serve our services at your area.";
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

                    context.Customers.Add(customer);

                    var customerLatLngVerify = new CustomerLatLngVerify
                    {
                        CaptureImagePath = customer.Shopimage,
                        CreatedBy = customer.CustomerId,
                        CreatedDate = DateTime.Now,
                        CustomerId = customer.CustomerId,
                        IsActive = true,
                        IsDeleted = false,
                        lat = customer.lat,
                        lg = customer.lg,
                        Newlat = customer.lat,
                        Newlg = customer.lg,
                        NewShippingAddress = customer.ShippingAddress,
                        ShippingAddress = customer.ShippingAddress,
                        ShopFound = 1,
                        ShopName = customer.ShopName,
                        Skcode = customer.Skcode,
                        LandMark = customer.LandMark,
                        Status = 1,
                        Nodocument = true,
                        Aerialdistance = 0,
                        Comment = "WhatsApp"
                    };
                    context.CustomerLatLngVerify.Add(customerLatLngVerify);
                    context.Commit();

                    var res = new
                    {
                        Name = string.IsNullOrEmpty(customer.Name) ? customer.ShopName : customer.Name,
                        SkCode = customer.Skcode,
                        CustomerId = customer.CustomerId,
                        IsVerified = customer.Active && customer.CustomerVerify == "Full Verified" ? true : false,
                        Status = true,
                        Message = message,
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }


        [Route("UpdateDocRegistation")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateDocRegistation(UpdateDocChatbotUser updateDocChatbotUser)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                var res = new
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            string filename = "";
            if (!string.IsNullOrEmpty(updateDocChatbotUser.DocPath))
            {
                filename = updateDocChatbotUser.DocNumber + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + updateDocChatbotUser.DocPath.Substring(updateDocChatbotUser.DocPath.LastIndexOf("."));
                var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);

                try
                {
                    using (WebClient Client = new WebClient())
                    {
                        Client.DownloadFile(updateDocChatbotUser.DocPath, ImageUrl);
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("WhatsChatBot: " + ex.ToString(), true);
                    ImageUrl = "";
                    filename = "";
                }
                if (string.IsNullOrEmpty(ImageUrl))
                {
                    var res = new
                    {
                        Message = "Document image not uploaded.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            else
            {
                var res = new
                {
                    Message = "Document image required.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

            using (var context = new AuthContext())
            {
                if (!string.IsNullOrEmpty(updateDocChatbotUser.DocNumber))
                {
                    var customer = await context.Customers.Where(x => x.Deleted == false && x.CustomerId == updateDocChatbotUser.CustomerId).FirstOrDefaultAsync();
                    if (customer != null && customer.CustomerVerify != "Full Verified")
                    {
                        var docMaster = context.CustomerDocTypeMasters.Where(x => x.IsActive && x.DocType == updateDocChatbotUser.DocType).ToList();
                        if (docMaster.Any())
                        {
                            bool duplicate = false;
                            if (updateDocChatbotUser.DocType == "GST")
                            {
                                duplicate = context.Customers.Any(x => x.Deleted == false && x.CustomerId != updateDocChatbotUser.CustomerId && x.RefNo == updateDocChatbotUser.DocNumber);
                            }
                            else
                            {
                                duplicate = context.Customers.Any(x => x.Deleted == false && x.CustomerId != updateDocChatbotUser.CustomerId && x.LicenseNumber == updateDocChatbotUser.DocNumber);
                            }
                            if (!duplicate)
                            {
                                var customerdocs = context.CustomerDocs.Where(x => x.CustomerId == customer.CustomerId && x.IsActive).ToList();
                                foreach (var custdoc in customerdocs)
                                {
                                    custdoc.ModifiedBy = 0;
                                    custdoc.ModifiedDate = DateTime.Now;
                                    custdoc.IsActive = false;
                                    context.Entry(custdoc).State = EntityState.Modified;
                                }
                                context.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = customer.CustomerId,
                                    IsActive = true,
                                    CreatedBy = customer.CustomerId,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = docMaster.FirstOrDefault().Id,
                                    DocPath = filename,
                                    IsDeleted = false
                                });

                                if (updateDocChatbotUser.DocType == "GST")
                                {
                                    customer.RefNo = updateDocChatbotUser.DocNumber;
                                    customer.UploadGSTPicture = filename;
                                    var custGstVerifys = context.CustGSTverifiedRequestDB.Where(x => x.RefNo == updateDocChatbotUser.DocNumber).ToList();
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
                                else
                                {
                                    customer.LicenseNumber = updateDocChatbotUser.DocNumber;
                                    customer.UploadLicensePicture = filename;
                                }
                                context.Entry(customer).State = EntityState.Modified;
                                context.Commit();
                                var res = new
                                {
                                    Message = "document updated successfully.",
                                    Status = true
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else
                            {
                                var res = new
                                {
                                    Message = "This document already used other customer.",
                                    Status = true
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {
                            var res = new
                            {
                                Message = "document type not supported.",
                                Status = false
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        var res = new
                        {
                            Message = "Your document not updated due to you are verified customer please contact customer care.",
                            Status = false
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    var res = new
                    {
                        Message = "please provide document number.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }

        [Route("UpdateCustomerAddress")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateCustomerAddress(ChatbotUserAddress chatbotUserAddress)
        {
            if (!(HttpContext.Current.Request.Headers.GetValues("ChatbotUserName")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotUserName"].ToString() && HttpContext.Current.Request.Headers.GetValues("ChatbotPassword")?.FirstOrDefault() == ConfigurationManager.AppSettings["ChatbotPassword"].ToString()))
            {
                var res = new
                {
                    Message = "Authorization has been denied for this request.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }

            double lt = 0, lng = 0;
            if (!string.IsNullOrEmpty(chatbotUserAddress.LatlngUrl))
            {
                try
                {
                    var latlngarr = chatbotUserAddress.LatlngUrl.Split(',');
                    if (latlngarr.Length == 2)
                    {
                        lt = Convert.ToDouble(latlngarr[0]);
                        lng = Convert.ToDouble(latlngarr[1]);
                    }
                    else
                    {
                        lt = 0; lng = 0;
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError("WhatsChatBot: " + ex.ToString(), true);
                    lt = 0; lng = 0;
                }

                if (lt == 0 || lng == 0)
                {
                    var res = new
                    {
                        Message = "Please enter correct customer location.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            else
            {
                var res = new
                {
                    Message = "Please enter customer location.",
                    Status = false
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            using (var context = new AuthContext())
            {
                var customer = await context.Customers.Where(x => x.Deleted == false && x.CustomerId == chatbotUserAddress.CustomerId).FirstOrDefaultAsync();

                if (customer != null && (customer.ShippingAddress != chatbotUserAddress.ShippingAddress || customer.lat != lt || customer.lg != lng))
                {
                    var customerLatLngVerify = await context.CustomerLatLngVerify.FirstOrDefaultAsync(x => x.CustomerId == chatbotUserAddress.CustomerId && x.IsActive);
                    if (customerLatLngVerify == null)
                    {
                        customerLatLngVerify = new CustomerLatLngVerify
                        {
                            CaptureImagePath = customer.Shopimage,
                            CreatedBy = customer.CustomerId,
                            CreatedDate = DateTime.Now,
                            CustomerId = customer.CustomerId,
                            IsActive = true,
                            IsDeleted = false,
                            lat = customer.lat,
                            lg = customer.lg,
                            Newlat = customer.lat,
                            Newlg = customer.lg,
                            NewShippingAddress = chatbotUserAddress.ShippingAddress,
                            ShippingAddress = customer.ShippingAddress,
                            ShopFound = 1,
                            ShopName = customer.ShopName,
                            Skcode = customer.Skcode,
                            LandMark = customer.LandMark,
                            Status = 1,
                            Nodocument = true,
                            Aerialdistance = 0,
                            Comment = "WhatsApp"
                        };
                        context.CustomerLatLngVerify.Add(customerLatLngVerify);
                        context.Commit();

                        var Custres = new
                        {
                            Message = "Address updated successfully.",
                            Status = true
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Custres);
                    }
                    else
                    {
                        if (customerLatLngVerify.Status == 1 && customerLatLngVerify.Comment != "WhatsApp")
                        {
                            var Custres = new
                            {
                                Message = "Your previouse request already inprogress.",
                                Status = false
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, Custres);
                        }

                        if (customerLatLngVerify.Status == 1)
                        {
                            //customerLatLngVerify.NewShippingAddress = chatbotUserAddress.ShippingAddress;
                            //customerLatLngVerify.Newlat = lt;
                            //customerLatLngVerify.Newlg = lng;
                            customerLatLngVerify.IsActive = false;
                            customerLatLngVerify.ModifiedBy = chatbotUserAddress.CustomerId;
                            customerLatLngVerify.ModifiedDate = DateTime.Now;
                            context.Entry(customerLatLngVerify).State = EntityState.Modified;
                        }

                        customerLatLngVerify = new CustomerLatLngVerify
                        {
                            CaptureImagePath = customer.Shopimage,
                            CreatedBy = customer.CustomerId,
                            CreatedDate = DateTime.Now,
                            CustomerId = customer.CustomerId,
                            IsActive = true,
                            IsDeleted = false,
                            lat = customer.lat,
                            lg = customer.lg,
                            Newlat = customer.lat,
                            Newlg = customer.lg,
                            NewShippingAddress = chatbotUserAddress.ShippingAddress,
                            ShippingAddress = customer.ShippingAddress,
                            ShopFound = 1,
                            ShopName = customer.ShopName,
                            Skcode = customer.Skcode,
                            LandMark = customer.LandMark,
                            Status = 1,
                            Nodocument = true,
                            Aerialdistance = 0,
                            Comment = "WhatsApp"
                        };
                        context.CustomerLatLngVerify.Add(customerLatLngVerify);

                        context.Commit();
                        var Custres1 = new
                        {
                            Message = "Address updated request received successfully.",
                            Status = false
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, Custres1);
                    }
                }
                else
                {

                    var Custres = new
                    {
                        Message = "You are not shopkirana customer.",
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, Custres);
                }

            }
        }
    }
}