/// SPA sales person app
/// RA retailer app
/// SPA V2 sales person app 2nd version
/// RA V2 retailer app 2nd version

using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.AgentCommission;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.KPPApp;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Customer;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Agentcommision;
using AngularJSAuthentication.Model.Login;
using AngularJSAuthentication.Model.NotMapped;
using GenricEcommers.Models;
using Hangfire;
using LinqKit;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using static AngularJSAuthentication.API.Controllers.Reporting.CustomerWalletPointController;
using AngularJSAuthentication.DataContracts.CustomerReferralDc;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Model.CustomerDelight;
using AngularJSAuthentication.API.Controllers.External.Gamification;
using AngularJSAuthentication.API.Managers.CRM;
using System.Runtime.Serialization.Formatters.Binary;
using NPOI.SS.UserModel;
using System.Web.Script.Serialization;
using NPOI.XSSF.UserModel;
using AngularJSAuthentication.API.ControllerV7.Store;
using AngularJSAuthentication.DataContracts.External.MobileExecutiveDC;
using System.Net.Http.Headers;
using AngularJSAuthentication.API.Helper.Notification;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Customers")]
    public class CustomersController : BaseAuthController
    {


        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public double xPointValue = AppConstants.xPoint;

        [Route("CRMLevelCustomerDetail")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<List<CustDetailLabelResponse>> CRMLevelCustomerDetail(int month, int year)
        {
            using (var myContext = new AuthContext())
            {
                var monthParam = new SqlParameter("@month", month);
                var yearParam = new SqlParameter("@year", year);

                var result = myContext.Database.SqlQuery<CustDetailLabelResponse>("CRMLevelCustomerDetail @month,@year", monthParam, yearParam).ToList();
                return result;
            }
        }

        [Route("GetCustDetailLabel")]
        [AllowAnonymous]
        public async Task<List<CustDetailLabelResponse>> GetCustDetailLabel(int month, int year)
        {
            using (var myContext = new AuthContext())
            {
                var monthParam = new SqlParameter("@month", month);
                var yearParam = new SqlParameter("@year", year);

                var result = myContext.Database.SqlQuery<CustDetailLabelResponse>("GetLabelCustomerDetail @month,@year", monthParam, yearParam).ToList();
                return result;
            }
        }

        [Route("GetCustWarehouseDetailLabel")]
        [AllowAnonymous]
        public async Task<List<CustDetailLabelResponse>> GetCustWarehouseDetailLabel(int month, int year, int WarehouseId)
        {
            using (var myContext = new AuthContext())
            {
                var monthParam = new SqlParameter("@month", month);
                var yearParam = new SqlParameter("@year", year);
                var Warehouse = new SqlParameter("@WarehouseId", WarehouseId);
                var result = myContext.Database.SqlQuery<CustDetailLabelResponse>("GetLabelCustomerDetailForTarget @month,@year,@WarehouseId", monthParam, yearParam, Warehouse).ToList();
                return result;
            }
        }


        [Authorize]
        [Route("")]
        public IEnumerable<Customer> Get()
        {
            using (AuthContext context = new AuthContext())
            {
                logger.Info("start Get Customer: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouse_id = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    logger.Info("End  Customer: ");

                    if (Warehouse_id > 0)
                    {
                        var customer = context.AllCustomers(compid).Where(a => a.Warehouseid == Warehouse_id).OrderByDescending(c => c.CreatedDate).ToList();
                        var warehouseIds = customer.Select(x => x.Warehouseid).Distinct().ToList();
                        var warehouses = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();

                        customer.ForEach(x =>
                        {
                            x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";

                        });

                        return customer.ToList();
                    }
                    else
                    {
                        var customer = context.AllCustomers(compid).OrderByDescending(c => c.CreatedDate).ToList();
                        var warehouseIds = customer.Select(x => x.Warehouseid).Distinct().ToList();
                        var warehouses = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();

                        customer.ForEach(x =>
                        {
                            x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";

                        });
                        return customer.ToList();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }


        [Route("")]
        public Customer Get(string Mobile)
        {
            logger.Info("start City: ");
            using (AuthContext context = new AuthContext())
            {
                Customer customer = new Customer();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    customer = context.GetCustomerbyId(Mobile);
                    var warehouses = context.Warehouses.FirstOrDefault(x => customer.Warehouseid == x.WarehouseId);
                    customer.WarehouseName = warehouses.WarehouseName;
                    logger.Info("End  Customer: ");
                    return customer;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }


        [Route("DirectCustomer")]
        [HttpGet]
        [AllowAnonymous]
        public Customer DirectCustomer(string Mobile)
        {
            logger.Info("start City: ");
            using (AuthContext context = new AuthContext())
            {
                Customer customer = new Customer();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    customer = context.GetCustomerbyId(Mobile);
                    logger.Info("End  Customer: ");
                    return customer;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Customer Order Search by sudhir
        /// </summary>
        /// <param name="Customerid"></param>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        [Route("Customerdata")]
        [HttpGet]
        public List<OrderMaster> Gets(int Customerid, int warehouseid)
        {
            logger.Info("start City: ");
            using (AuthContext db = new AuthContext())
            {
                List<OrderMaster> data = new List<OrderMaster>();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    data = db.DbOrderMaster.Where(x => x.CustomerId == Customerid && x.WarehouseId == warehouseid && x.Status == "Pending" && x.Deleted == false).ToList();
                    return data;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;

                }
            }
        }

        /// <summary>
        /// Hub to hub Transfer  by sudhir 28-08-2019
        /// </summary>
        /// <param name="Customerid"></param>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        [Route("PendingOrderHubTransfer")]
        [HttpPut]
        public HttpResponseMessage PendingOrderHubTransfer(int Customerid, int FromWarehouseId, int ToWarehouseId)
        {
            // select b.ItemId,a.ItemId,b.ItemMultiMRPId,a.ItemMultiMRPId
            //--update a set a.ItemId = b.ItemId,a.ItemMultiMRPId = b.ItemMultiMRPId,a.WarehouseId = 92,a.WarehouseName = 'GJ-AMD-3'
            //from OrderDetails a
            //inner
            //join ItemMasters b on a.SellingSku = b.SellingSku and b.WarehouseId = 92
            //where a.orderid = 370341
            //update OrderMasters set WarehouseId = 92, WarehouseName = 'GJ-AMD-3' where orderid = 370341

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            int CompanyId = compid;

            using (AuthContext db = new AuthContext())
            {
                List<OrderMaster> OrderList = new List<OrderMaster>();
                var customers = db.Customers.Where(x => x.CustomerId == Customerid).FirstOrDefault();
                OrderList = db.DbOrderMaster.Where(x => x.CustomerId == customers.CustomerId && x.WarehouseId == FromWarehouseId && x.Status == "Pending" && x.Deleted == false && x.active == true).Include(x => x.orderDetails).ToList();
                var WH = db.Warehouses.Where(x => x.WarehouseId == ToWarehouseId).FirstOrDefault();
                if (WH != null && OrderList != null && OrderList.Any())
                {
                    foreach (var item in OrderList)
                    {
                        if (item.WarehouseId != ToWarehouseId)
                        {
                            item.WarehouseId = WH.WarehouseId;
                            item.WarehouseName = WH.WarehouseName;
                            db.Entry(item).State = EntityState.Modified;

                            foreach (var itemod in item.orderDetails)
                            {
                                if (itemod.IsFreeItem)
                                {
                                    itemod.FreeWithParentItemId = db.OfferItemDb.Where(x => x.OrderId == itemod.OrderId && x.FreeItemId == itemod.ItemId).Select(x => x.itemId).FirstOrDefault();
                                    itemod.qty = 0;
                                    itemod.Noqty = 0;
                                    itemod.TotalAmt = 0;
                                }
                                itemod.ItemId = db.itemMasters.Where(x => x.SellingSku == itemod.SellingSku && x.WarehouseId == WH.WarehouseId).Select(x => x.ItemId).FirstOrDefault();
                                itemod.WarehouseId = WH.WarehouseId;
                                itemod.WarehouseName = WH.WarehouseName;

                                db.Entry(itemod).State = EntityState.Modified;
                            }
                        }
                    }
                    if (customers.Warehouseid != ToWarehouseId)
                    {
                        customers.Warehouseid = WH.WarehouseId;
                        customers.WarehouseName = WH.WarehouseName;
                        db.Entry(customers).State = EntityState.Modified;
                    }
                    if (db.Commit() > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Hub To Hub Transfer Successfully!!");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "somethong went wrong!!");
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Please Select correct Hub!!");
            }
        }
        [Route("InActive")]
        public List<Customer> GetInActive()
        {
            logger.Info("start customer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    List<Customer> customer = db.Customers.Where(x => x.Active == false).ToList();
                    var warehouseIds = customer.Select(x => x.Warehouseid).Distinct().ToList();
                    var warehouses = db.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();

                    customer.ForEach(x =>
                    {
                        x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
                    });

                    logger.Info("End  Customer: ");
                    return customer;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }

        [Route("Forgrt")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetForgrt(string Mobile)
        {
            logger.Info("start City: ");
            using (AuthContext db = new AuthContext())
            {
                Customer customer = new Customer();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    customer = db.GetCustomerbyId(Mobile);

                    var warehouses = db.Warehouses.FirstOrDefault(x => customer.Warehouseid == x.WarehouseId);
                    customer.WarehouseName = warehouses.WarehouseName;
                    if (customer != null)
                    {
                        new Sms().sendOtp(customer.Mobile, "Hi " + customer.ShopName + " \n\t You Recently requested a forget password on ShopKirana. Your account Password is '" + customer.Password + "'\n If you didn't request then ingore this message\n\t\t Thanks\n\t\t Shopkirana.com", "");
                        return Request.CreateResponse(HttpStatusCode.OK, true);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("serach")]
        public HttpResponseMessage serach(string key)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    var customer = db.Customers.Where(c => (c.Skcode.Contains(key) || c.ShopName.Contains(key) || c.Mobile.Contains(key)) && c.Deleted == false).ToList();

                    var warehouseIds = customer.Select(x => x.Warehouseid).Distinct().ToList();
                    var warehouses = db.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();

                    customer.ForEach(x =>
                    {
                        x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
                    });


                    return Request.CreateResponse(HttpStatusCode.OK, customer);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }




        [Route("export")]
        [HttpGet]
        public List<CustomerExport> export()
        {
            return ExportCustomer(1);
        }


        [Route("GetClusters")]
        [HttpGet]
        public dynamic GetClusters(double lat, double longt)
        {
            logger.Info("start City: ");
            //  dynamic customer = null;
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    string clstname;
                    int clstid = 0;
                    string warehousename;
                    int? warehouseid = 0;
                    List<Cluster> c = db.Clusters.Include("LtLng").ToList();
                    foreach (var d in c)
                    {
                        foreach (var l in d.LtLng)
                        {
                            if (l.latitude == lat && l.longitude == longt)
                            {

                                clstname = d.ClusterName;
                                clstid = d.ClusterId;
                                d.ClstId = clstid;
                                warehousename = d.WarehouseName;
                                warehouseid = d.WarehouseId;

                                return d;
                            }

                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }

        [Route("CheckGst")]
        [HttpGet]
        public HttpResponseMessage CheckGst(string Gst)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    logger.Info("Get Customer: ");
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string email = "";
                    var identity = User.Identity as ClaimsIdentity;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }

                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");

                    var RDGst = context.Customers.Where(x => x.RefNo == Gst).FirstOrDefault();

                    return Request.CreateResponse(HttpStatusCode.OK, RDGst);



                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }
            }
        }

        [ResponseType(typeof(Customer))]
        [Route("")]
        public IEnumerable<CustomerExport> Get(string Cityid, string mobile, string skcode, DateTime? datefrom, DateTime? dateto)
        {
            return ExportCustomer(0, Cityid, mobile, skcode, datefrom, dateto);


        }

        private List<CustomerExport> ExportCustomer(int isAll, string Cityid = "", string mobile = "", string skcode = "", DateTime? datefrom = null, DateTime? dateto = null)
        {
            List<CustomerExport> result = new List<CustomerExport>();
            using (AuthContext db = new AuthContext())
            {
                if (mobile == "undefined")
                {
                    mobile = "";
                }
                if (skcode == "undefined")
                {
                    skcode = "";
                }
                var paramList = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "isall",
                        Value = isAll
                    }
                };

                paramList.Add(new SqlParameter
                {
                    ParameterName = "Cityid",
                    Value = !string.IsNullOrEmpty(Cityid) ? Convert.ToInt32(Cityid) : 0
                });

                paramList.Add(new SqlParameter
                {
                    ParameterName = "mobile",
                    Value = !string.IsNullOrEmpty(mobile) ? mobile : ""

                });

                paramList.Add(new SqlParameter
                {
                    ParameterName = "skcode",
                    Value = !string.IsNullOrEmpty(skcode) ? skcode : ""
                });

                if (datefrom.HasValue && dateto.HasValue)
                {
                    paramList.Add(new SqlParameter
                    {
                        ParameterName = "datefrom",
                        Value = datefrom.Value
                    });

                    paramList.Add(new SqlParameter
                    {
                        ParameterName = "dateto",
                        Value = dateto.Value
                    });
                }
                else
                {
                    paramList.Add(new SqlParameter
                    {
                        ParameterName = "datefrom",
                        Value = DBNull.Value
                    });

                    paramList.Add(new SqlParameter
                    {
                        ParameterName = "dateto",
                        Value = DBNull.Value
                    });
                }

                result = db.Database.SqlQuery<CustomerExport>("exec ExportAllCustomers @isall,@Cityid,@mobile,@skcode,@datefrom,@dateto", paramList.ToArray()).ToList();
            }

            #region CRMTAG

            var skcodeList = result.Select(x => x.RetailersCode).Distinct().ToList();

            CRMManager cRMManager = new CRMManager();
            var TagData = cRMManager.GetCRMCustomerWithTag(skcodeList, CRMPlatformConstants.AllTagsOnCustomerMaster);
            result.ForEach(x =>
            {
                x.CRMTags = TagData != null && TagData.Result != null ? TagData.Result.Where(y => y.Skcode == x.RetailersCode).Select(z => z.CRMTags).FirstOrDefault() : null;
            });

            #endregion

            return result;
        }


        [ResponseType(typeof(Customer))]
        [Route("")]
        [AcceptVerbs("POST")]
        public Customer add(Customer item)
        {
            logger.Info("start addCustomer: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    if (compid == 0)
                    {
                        compid = 1;
                    }
                    item.CompanyId = compid;
                    item.userid = userid;
                    var data = context.AddCustomer(item);

                    if (data.Active && !data.Deleted && data.CustomerVerify == "Full Verified")
                    {
                        using (var myContext = new AuthContext())
                        {
                            int rewardPoints = 0;
                            var custRewardCityBased = myContext.CityBaseCustomerRewards.FirstOrDefault(x => x.CityId == data.Cityid && x.IsActive && !x.IsDeleted && x.StartDate <= indianTime && x.EndDate >= indianTime
                                               && x.RewardType == "IsSignup");
                            if (custRewardCityBased != null)
                            {
                                var rewardGiven = myContext.RewardedCustomers.Any(x => (x.CityBaseCustomerRewardId == custRewardCityBased.Id && x.CustomerId == data.CustomerId) || data.ordercount > 0);

                                if (!rewardGiven)
                                {
                                    rewardPoints = custRewardCityBased.Point;

                                    NotificationHelper notHelper = new NotificationHelper();
                                    string title = "बधाई हो ! ";
                                    string notificationMessage = "शॉपकिराना से जुड़ने के लिए धन्यवाद् ! आपको मिले हैं " + rewardPoints + " ड्रीम पॉइंट्स फ्री ! अब सारे ब्रांड एक जगह और फ्री डिलीवरी 24X7 आपके द्वार ";
                                    string smsMessage = "Thanks for associating with Shopkirana ! You have been awarded " + rewardPoints + " free dream points ! Now all brands are at one place and free delivery 24X7 at your doorstep. ";
                                    notHelper.SendNotificationtoCustomer(item, notificationMessage, smsMessage, true, true, title);

                                    var walt = myContext.WalletDb.FirstOrDefault(x => x.CustomerId == data.CustomerId);
                                    CustomerWalletHistory od = new CustomerWalletHistory();
                                    od.CustomerId = walt.CustomerId;

                                    od.WarehouseId = data.Warehouseid ?? 0;
                                    od.CompanyId = data.CompanyId ?? 0;
                                    od.NewAddedWAmount = rewardPoints;
                                    od.TotalWalletAmount = walt.TotalAmount + od.NewAddedWAmount;
                                    od.UpdatedDate = indianTime;
                                    od.Through = "Customer Activation";
                                    od.TransactionDate = indianTime;
                                    od.CreatedDate = indianTime;
                                    myContext.CustomerWalletHistoryDb.Add(od);
                                    // this.SaveChanges();

                                    walt.CustomerId = walt.CustomerId;
                                    if (walt.TotalAmount == 0)
                                    {
                                        walt.TotalAmount = rewardPoints;
                                    }
                                    else
                                    {
                                        walt.TotalAmount = walt.TotalAmount + rewardPoints;
                                    }
                                    walt.UpdatedDate = indianTime;
                                    myContext.Entry(walt).State = EntityState.Modified;

                                    var rewardedCustomer = new RewardedCustomer
                                    {
                                        CustomerId = data.CustomerId,
                                        CityBaseCustomerRewardId = custRewardCityBased.Id,
                                        CreatedBy = userid,
                                        UpdateBy = userid,
                                        IsDeleted = false,
                                        CreatedDate = indianTime
                                    };
                                    myContext.RewardedCustomers.Add(rewardedCustomer);

                                    myContext.Commit();
                                }

                            }
                        }
                    }


                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    logger.Info("End  addCustomer: ");
                    return data;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    return null;
                }
            }
        }

        //missingCustomer
        [Route("getcustomers")]
        public IEnumerable<CustomerExport> getcustomersMissingDetailNull()
        {
            return ExportCustomer(2);

        }
        //end
        //[Authorize]
        [ResponseType(typeof(Customer))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public Customer Put(Customer item)
        {
            logger.Info("start putCustomer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }

                    }
                    using (AuthContext context = new AuthContext())
                    {
                        item.userid = userid;
                        if (item == null)
                        {
                            throw new ArgumentNullException("putCustomer");
                        }
                        if (compid == 0)
                        {
                            compid = 1;
                        }
                        item.CompanyId = compid;
                        item = context.PutCustomer(item);

                        if (item.Active && !item.Deleted && item.CustomerVerify == "Full Verified")
                        {
                            using (var myContext = new AuthContext())
                            {
                                int rewardPoints = 0;
                                var custRewardCityBased = myContext.CityBaseCustomerRewards.FirstOrDefault(x => x.CityId == item.Cityid && x.IsActive && !x.IsDeleted && x.StartDate <= indianTime && x.EndDate >= indianTime
                                                   && x.RewardType == "IsSignup");
                                if (custRewardCityBased != null)
                                {
                                    var rewardGiven = myContext.RewardedCustomers.Any(x => (x.CityBaseCustomerRewardId == custRewardCityBased.Id && x.CustomerId == item.CustomerId) || item.ordercount > 0);

                                    if (!rewardGiven)
                                    {
                                        rewardPoints = custRewardCityBased.Point;

                                        NotificationHelper notHelper = new NotificationHelper();
                                        string title = "बधाई हो ! ";
                                        string notificationMessage = "शॉपकिराना से जुड़ने के लिए धन्यवाद् ! आपको मिले हैं " + rewardPoints + " ड्रीम पॉइंट्स फ्री ! अब सारे ब्रांड एक जगह और फ्री डिलीवरी 24X7 आपके द्वार ";
                                        string smsMessage = "Thanks for associating with Shopkirana ! You have been awarded " + rewardPoints + " free dream points ! Now all brands are at one place and free delivery 24X7 at your doorstep. ";
                                        notHelper.SendNotificationtoCustomer(item, notificationMessage, smsMessage, true, true, title);

                                        var walt = myContext.WalletDb.FirstOrDefault(x => x.CustomerId == item.CustomerId);
                                        CustomerWalletHistory od = new CustomerWalletHistory();
                                        od.CustomerId = walt.CustomerId;
                                        od.WarehouseId = item.Warehouseid ?? 0;
                                        od.CompanyId = item.CompanyId ?? 0;
                                        od.NewAddedWAmount = rewardPoints;
                                        od.TotalWalletAmount = walt.TotalAmount + od.NewAddedWAmount;
                                        od.UpdatedDate = indianTime;
                                        od.Through = "Customer Activation";
                                        od.TransactionDate = indianTime;
                                        od.CreatedDate = indianTime;
                                        myContext.CustomerWalletHistoryDb.Add(od);
                                        // this.SaveChanges();

                                        walt.CustomerId = walt.CustomerId;
                                        if (walt.TotalAmount == 0)
                                        {
                                            walt.TotalAmount = rewardPoints;
                                        }
                                        else
                                        {
                                            walt.TotalAmount = walt.TotalAmount + rewardPoints;
                                        }
                                        walt.UpdatedDate = indianTime;
                                        myContext.Entry(walt).State = EntityState.Modified;

                                        var rewardedCustomer = new RewardedCustomer
                                        {
                                            CustomerId = item.CustomerId,
                                            CityBaseCustomerRewardId = custRewardCityBased.Id,
                                            CreatedBy = userid,
                                            UpdateBy = userid,
                                            IsDeleted = false,
                                            CreatedDate = indianTime
                                        };
                                        myContext.RewardedCustomers.Add(rewardedCustomer);
                                        myContext.Commit();
                                    }

                                }
                            }
                        }
                        return item;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in put Customer " + ex.Message);
                    return null;
                }
            }
        }

        #region Customer Update api       
        [Route("V1")]
        [AcceptVerbs("PUT")]
        public CompleteCustomerDC Putcust(CompleteCustomerDC item)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            string username = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity.Name != null && identity.Name != null && identity.Name.Any())
                username = identity.Name;

            using (AuthContext myContext = new AuthContext())
            {
                string oldRefNo = "";
                Customer customer = myContext.Customers.Where(x => x.CustomerId == item.CustomerId).FirstOrDefault();

                if (!customer.Deleted)
                {
                    var city = myContext.Cities.FirstOrDefault(x => x.Cityid == item.Cityid);
                    if (city != null && city.Cityid > 0 && city.StateName != null)
                    {
                        customer.State = city.StateName;
                    }
                    string ExistsSkcode = null;
                    if (!string.IsNullOrEmpty(item.PanNo))
                        ExistsSkcode = myContext.Customers.FirstOrDefault(x => (x.PanNo == item.PanNo) && x.CustomerId != item.CustomerId)?.Skcode;
                    if (ExistsSkcode != null)
                    {
                        item.IsPanOrGSTExists = true;
                        item.PanOrGSTExistsSkCode = ExistsSkcode;
                        return item;
                    }
                    //removed on 13/07/2022 by Harry
                    //if (customer.ShippingAddress != item.ShippingAddress)
                    //{
                    //    GeoHelper geoHelper = new GeoHelper();
                    //    decimal? lat, longitude;
                    //    geoHelper.GetLatLongWithZipCode(item.ShippingAddress, customer.City, customer.ZipCode, out lat, out longitude);
                    //    customer.Addresslat = (double?)lat;
                    //    customer.Addresslg = (double?)longitude;
                    //    customer.Distance = 0;
                    //}

                    //if (customer.Addresslat.HasValue && customer.Addresslg.HasValue && item.lat > 0 && item.lg > 0)
                    //{
                    //    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(customer.Addresslat.Value, customer.Addresslg.Value);
                    //    var destination = new System.Device.Location.GeoCoordinate(item.lat, item.lg);
                    //    customer.Distance = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                    //}

                    oldRefNo = customer.RefNo;// Check If old Value not change
                    //customer.Mobile = item.Mobile;
                    customer.Name = item.Name;
                    customer.ShopName = item.ShopName;
                    customer.City = item.City;
                    customer.Cityid = item.Cityid;
                    customer.ClusterId = item.ClusterId;
                    customer.ClusterName = item.ClusterName;
                    customer.Agents = item.Agents;
                    customer.AgentCode = item.AgentCode;
                    customer.Active = item.Active;
                    customer.IsSignup = item.IsSignup;
                    customer.IsKPP = item.IsKPP;

                    customer.AC_HolderName = (item.IsKPP || item.CustomerType == "RDS" || item.CustomerType == "SKP Owner") ? item.AC_HolderName : null;
                    customer.BankName = (item.IsKPP || item.CustomerType == "RDS" || item.CustomerType == "SKP Owner") ? item.BankName : null;
                    customer.IfscCode = (item.IsKPP || item.CustomerType == "RDS" || item.CustomerType == "SKP Owner") ? item.IfscCode : null;
                    customer.AccountNumber = (item.IsKPP || item.CustomerType == "RDS" || item.CustomerType == "SKP Owner") ? item.AccountNumber : null;

                    customer.CustomerType = customer.IsPermanentType == true ? customer.CustomerType : item.CustomerType;

                    customer.KPPWarehouseId = item.KPPWarehouseId;
                    customer.IsChequeAccepted = item.IsChequeAccepted;
                    customer.ChequeLimit = item.ChequeLimit;
                    customer.Description = item.Description;
                    //customer.ExecutiveName = item.ExecutiveName;

                    // Due to customeraddress Date 27/07/2022
                    //if (customer.CustomerDocumentStatus != (int)CustomerDocumentStatusEnum.Verified)
                    //{
                    customer.RefNo = item.RefNo;
                    customer.ShippingAddress = item.ShippingAddress;
                    customer.ShippingAddress1 = item.ShippingAddress1;
                    customer.BillingAddress = item.BillingAddress;
                    customer.BillingAddress1 = item.BillingAddress1;
                    customer.BillingCity = item.BillingCity;
                    customer.BillingState = item.BillingState;
                    customer.BillingZipCode = item.BillingZipCode;
                    customer.ZipCode = item.ZipCode;
                    customer.ShippingCity = item.ShippingCity;
                    customer.LicenseNumber = item.LicenseNumber;
                    customer.LicenseExpiryDate = item.LicenseExpiryDate;
                    customer.NameOnGST = item.NameOnGST;
                    if (customer.lat != item.lat && customer.lg != item.lg)
                    {
                        customer.lat = item.lat;
                        customer.lg = item.lg;
                    }
                    customer.Warehouseid = item.Warehouseid;
                    customer.WarehouseName = item.WarehouseName;
                    customer.AreaName = item.AreaName;
                    //}
                    customer.DOB = item.DOB;
                    //customer.ExecutiveId = item.ExecutiveId;
                    customer.CustomerVerify = item.CustomerVerify;
                    //customer.GrabbedBy = 0; ///default 
                    customer.UpdatedDate = indianTime;
                    customer.StatusSubType = item.StatusSubType;
                    customer.LastModifiedBy = username;

                    customer.Description = item.Description;
                    customer.Password = item.Password;
                    customer.IsKPP = item.IsKPP;
                    customer.IsSignup = item.IsSignup;
                    customer.IsChequeAccepted = item.IsChequeAccepted;
                    customer.ChequeLimit = item.ChequeLimit;
                    customer.UpdatedDate = indianTime;
                    customer.LastModifiedBy = username;
                    customer.Active = item.Active;
                    customer.CustomerType = item.CustomerType;
                    customer.AadharNo = item.AadharNo;
                    customer.PanNo = item.PanNo;
                    customer.TypeOfBuissness = item.StoreType;
                    customer.IsTCSExemption = item.IsTCSExemption;
                    customer.TCSExemptionDeclarationDOC = item.TCSExemptionDeclarationDOC;
                    customer.IsPanVerified = item.IsPanVerified;
                    if (item.CustomerType == "SKP Retailer")
                    {
                        customer.SKPOwnerId = item.SKPOwnerId;//by sudhir 04-07-2022
                    }
                    else
                    {
                        customer.SKPOwnerId = 0;
                    }

                    if ((customer.CustomerVerify == "Not Verified" || customer.CustomerVerify == "Temporary Active"))
                    {
                        AgentCommissionforCity agentCommissionforCity = myContext.AgentCommissionforCityDB.Where(x => x.CustomerId == item.CustomerId).FirstOrDefault();
                        if (agentCommissionforCity != null)
                        {
                            agentCommissionforCity.IsDeleted = true;
                            agentCommissionforCity.IsActive = false;
                            agentCommissionforCity.ModifiedBy = userid;
                            agentCommissionforCity.ModifiedDate = indianTime;
                            myContext.Entry(agentCommissionforCity).State = EntityState.Modified;
                        }
                    }

                    if (item.CustomerVerify == "Full Verified" && (customer.CustomerVerify == "Not Verified" || customer.CustomerVerify == "Temporary Active"))
                    {
                        customer.VerifiedBy = userid;
                        customer.VerifiedDate = DateTime.Now;

                        var custRewardCityBased = myContext.CityBaseCustomerRewards.FirstOrDefault(x => x.CityId == item.Cityid && x.IsActive && !x.IsDeleted && x.StartDate <= indianTime && x.EndDate >= indianTime
                                       && x.RewardType == "IsSignup");
                        if (custRewardCityBased != null)
                        {
                            var rewardGiven = myContext.RewardedCustomers.Any(x => (x.CityBaseCustomerRewardId == custRewardCityBased.Id && x.CustomerId == item.CustomerId) || item.ordercount > 0);
                            int rewardPoints = 0;
                            if (!rewardGiven)
                            {
                                rewardPoints = custRewardCityBased.Point;

                                NotificationHelper notHelper = new NotificationHelper();
                                string title = "बधाई हो ! ";
                                string notificationMessage = "शॉपकिराना से जुड़ने के लिए धन्यवाद् ! आपको मिले हैं " + rewardPoints + " ड्रीम पॉइंट्स फ्री ! अब सारे ब्रांड एक जगह और फ्री डिलीवरी 24X7 आपके द्वार ";
                                string smsMessage = "Thanks for associating with Shopkirana ! You have been awarded " + rewardPoints + " free dream points ! Now all brands are at one place and free delivery 24X7 at your doorstep. ";
                                notHelper.SendNotificationtoCustomer(customer, notificationMessage, smsMessage, true, true, title);

                                var walt = myContext.WalletDb.FirstOrDefault(x => x.CustomerId == item.CustomerId);

                                CustomerWalletHistory od = new CustomerWalletHistory();
                                od.CustomerId = walt.CustomerId;
                                od.WarehouseId = item.Warehouseid ?? 0;
                                od.CompanyId = item.CompanyId ?? 0;
                                od.NewAddedWAmount = rewardPoints;
                                od.TotalWalletAmount = walt.TotalAmount + od.NewAddedWAmount;
                                od.UpdatedDate = indianTime;
                                od.Through = "Customer Activation";
                                od.TransactionDate = indianTime;
                                od.CreatedDate = indianTime;
                                myContext.CustomerWalletHistoryDb.Add(od);
                                // this.SaveChanges();

                                walt.CustomerId = walt.CustomerId;
                                if (walt.TotalAmount == 0)
                                {
                                    walt.TotalAmount = rewardPoints;
                                }
                                else
                                {
                                    walt.TotalAmount = walt.TotalAmount + rewardPoints;
                                }
                                walt.UpdatedDate = indianTime;
                                myContext.Entry(walt).State = EntityState.Modified;

                                var rewardedCustomer = new RewardedCustomer
                                {
                                    CustomerId = item.CustomerId,
                                    CityBaseCustomerRewardId = custRewardCityBased.Id,
                                    CreatedBy = userid,
                                    UpdateBy = userid,
                                    IsDeleted = false,
                                    CreatedDate = indianTime
                                };
                                myContext.RewardedCustomers.Add(rewardedCustomer);
                                //myContext.Commit();
                            }

                        }
                    }
                    var customerBrandAcess = myContext.CustomerBrandAcessDB.Where(x => x.CustomerId == item.CustomerId).ToList();
                    foreach (var custBrandMapp in customerBrandAcess)
                    {
                        if (item.BrandIds.Contains(custBrandMapp.BrandId))
                            custBrandMapp.IsActive = true;
                        else
                            custBrandMapp.IsActive = false;

                        custBrandMapp.ModifiedDate = indianTime;
                        custBrandMapp.ModifiedBy = userid;
                        myContext.Entry(custBrandMapp).State = EntityState.Modified;

                    }

                    foreach (var obj in item.BrandIds.Where(x => !customerBrandAcess.Select(z => z.BrandId).Contains(x)))
                    {
                        CustomerBrandAcess CustBrandMapp = new CustomerBrandAcess();
                        CustBrandMapp.CustomerId = item.CustomerId;
                        CustBrandMapp.BrandId = obj;
                        CustBrandMapp.CreatedBy = userid;
                        CustBrandMapp.ModifiedBy = userid;
                        CustBrandMapp.CreatedDate = indianTime;
                        CustBrandMapp.ModifiedDate = indianTime;
                        CustBrandMapp.IsActive = true;
                        myContext.CustomerBrandAcessDB.Add(CustBrandMapp);
                    }
                    if (item.IsFranchise && !customer.IsFranchise && customer.Warehouseid > 0)
                    {
                        customer.FranchiseApprovedby = compid;
                        customer.FranchiseApprovedDate = indianTime;
                        customer.IsFranchise = item.IsFranchise;
                        GenerateFranchiseSK(customer);
                    }

                    bool IsGSTChange = false;

                    if (!string.IsNullOrEmpty(customer.RefNo) && oldRefNo != customer.RefNo)
                    {
                        var custGstVerifys = myContext.CustGSTverifiedRequestDB.Where(x => x.RefNo == customer.RefNo).ToList();
                        if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                        {
                            var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                            var state = myContext.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                            customer.BillingCity = gstVerify.City;
                            customer.BillingState = state != null ? state.StateName : gstVerify.State;
                            customer.BillingZipCode = gstVerify.Zipcode;
                            customer.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                            customer.NameOnGST = gstVerify.Name;
                            IsGSTChange = true;

                        }
                    }
                    if (string.IsNullOrEmpty(customer.BillingAddress))
                    {
                        customer.BillingCity = customer.City;
                        customer.BillingState = customer.State;
                        customer.BillingZipCode = customer.ZipCode;
                        customer.BillingAddress = customer.ShippingAddress;
                    }
                    //People people = myContext.Peoples.Where(q => q.PeopleID == customer.ExecutiveId).FirstOrDefault();                    
                    if (myContext.Commit() > 0 && ((!string.IsNullOrEmpty(customer.LicenseNumber) && !string.IsNullOrEmpty(item.Type)) || (!string.IsNullOrEmpty(customer.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture))))
                    {
                        var docMaster = myContext.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();
                        if (!string.IsNullOrEmpty(customer.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                        {
                            myContext.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = customer.CustomerId,
                                IsActive = true,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = docMaster.FirstOrDefault(x => x.DocType == "GST").Id,
                                DocPath = customer.UploadGSTPicture,
                                IsDeleted = false
                            });
                        }

                        if (!string.IsNullOrEmpty(customer.LicenseNumber) && !string.IsNullOrEmpty(item.Type))
                        {
                            myContext.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = customer.CustomerId,
                                IsActive = true,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = docMaster.FirstOrDefault(x => x.DocType == item.Type).Id,
                                DocPath = customer.UploadRegistration,
                                IsDeleted = false
                            });
                        }
                    }

                    myContext.Entry(customer).State = EntityState.Modified;



                    myContext.Commit();

                    // Add VATM Data
                    MongoDbHelper<VATMCustomers> mongoDbHelper = new MongoDbHelper<VATMCustomers>();
                    {
                        var vatm = mongoDbHelper.Select(x => x.CustomerId == item.CustomerId).FirstOrDefault();
                        if (vatm != null)
                        {
                            vatm.Data = item.VATM;
                            vatm.ModifiedBy = userid;
                            vatm.ModifiedDate = indianTime;

                            mongoDbHelper.Replace(vatm.Id, vatm);
                        }
                        else
                        {
                            VATMCustomers insertvatm = new VATMCustomers
                            {
                                CustomerId = item.CustomerId,
                                Data = item.VATM,
                                CreatedBy = userid,
                                CreatedDate = indianTime,
                                IsActive = true,
                            };
                            mongoDbHelper.Insert(insertvatm);
                        }
                    }

                    if (IsGSTChange)
                    {
                        myContext.Database.ExecuteSqlCommand("Exec UpdatePendingOrderGST " + customer.CustomerId);
                    }
                }

                return item;
            }

        }
        #endregion

        [Route("GetBankName")]
        [HttpGet]
        public HttpResponseMessage GetBankName()
        {

            List<DataContract.CurrencySettlementBankDc> currencySettlementBankDcs = new List<DataContract.CurrencySettlementBankDc>();
            try
            {
                using (AuthContext context = new AuthContext())
                {

                    currencySettlementBankDcs = context.CurrencySettlementBank.Where(x => x.ChequeBank).Select(x =>
                          new DataContract.CurrencySettlementBankDc
                          {
                              BankImage = x.BankImage,
                              BankName = x.BankName,
                              Id = x.Id
                          }).OrderBy(x=>x.BankName).ToList();
                }

                var res = new
                {
                    BankNameDc = currencySettlementBankDcs,
                    status = true,
                    Message = "Success"
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                var res = new
                {
                    BankNameDc = currencySettlementBankDcs,
                    status = false,
                    Message = "Fail"
                };

                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }




        #region CheckCustomerLatLong
        [Route("CheckCustomerLatLong")]
        [AcceptVerbs("Post")]
        public CompleteCustomerDC CheckCustomerLatLong(CompleteCustomerDC item)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            string username = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity.Name != null && identity.Name != null && identity.Name.Any())
                username = identity.Name;

            using (AuthContext myContext = new AuthContext())
            {
                Customer customer = myContext.Customers.Where(x => x.CustomerId == item.CustomerId).FirstOrDefault();
                if (customer.lat != item.lat && customer.lg != item.lg && item.lat != 0 && item.lg != 0)
                {
                    var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(item.lat).Append("', '").Append(item.lg).Append("')");
                    var clusterId = myContext.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                    if (!clusterId.HasValue)
                    {
                        item.Msg = "Cluster Not Found.";
                    }
                    else
                    {
                        var getClusterData = myContext.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                        if (getClusterData != null)
                        {
                            if (customer.ClusterId == getClusterData.ClusterId)
                            {
                                item.Msg = "Cluster is same.";
                            }
                            else
                            {
                                item.Warehouseid = getClusterData.WarehouseId;
                                item.WarehouseName = getClusterData.WarehouseName;
                                item.ClusterId = getClusterData.ClusterId;
                                item.ClusterName = getClusterData.ClusterName;
                                item.Msg = "Updated LatLong Belongs to " + getClusterData.ClusterName + " Clusters.";
                            }
                        }
                        else
                        {
                            item.Msg = "Cluster Not Found.";
                        }
                    }
                }
                else
                {
                    if (customer.ClusterId == item.ClusterId)
                    {
                        item.Msg = "Cluster is same.";
                    }
                }
                return item;
            }
        }
        #endregion


        [ResponseType(typeof(Customer))]
        [Route("")]
        [AcceptVerbs("Delete")]
        public void Remove(int id)
        {
            logger.Info("start delete Customer: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteCustomer(id);
                    logger.Info("End  delete Customer: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Customer " + ex.Message);
                }
            }
        }


        [ResponseType(typeof(Customer))]
        [Route("DeleteV7")]
        [AcceptVerbs("Delete")]
        public Boolean DeleteV7(int id)
        {
            logger.Info("start delete Customer: ");
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.DeleteCustomer(id);
                    logger.Info("End  delete Customer: ");
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Customer " + ex.Message);
                    return false;
                }
            }
        }



        [Route("favourite")]
        [HttpPost]
        public HttpResponseMessage getFavItems(favourite FIt)
        {
            using (AuthContext context = new AuthContext())
            {
                var item = FIt.items;
                try
                {
                    logger.Info("favourite ID : {0} , Company Id : {1}");

                    List<factoryItemdata> itemlist = new List<factoryItemdata>();
                    //foreach (var aa in item)
                    //{
                    try
                    {
                        List<int> ids = item.Select(x => x.ItemId).ToList();
                        var newdata = (from a in context.itemMasters
                                       where (a.Deleted == false && a.active == true && ids.Contains(a.ItemId))
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                           ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = b.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = b.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = a.IsOffer,
                                           Deleted = a.Deleted,
                                           ItemMultiMRPId = a.ItemMultiMRPId,
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
                            if (itemlist == null)
                            {
                                itemlist = new List<factoryItemdata>();
                            }
                            try
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
                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }
                            itemlist.Add(it);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }
                    //}
                    return Request.CreateResponse(HttpStatusCode.OK, itemlist);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        [Route("AddFavorite")]
        [HttpGet]
        public HttpResponseMessage AddFavorite(int itemId, int customerId, bool isLike)
        {
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
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("GetFavoriteItem")]
        [HttpGet]
        public HttpResponseMessage GetFavoriteItem(int customerId)
        {
            List<CustFavoriteItemDc> custFavoriteItemDcs = new List<CustFavoriteItemDc>();
            using (AuthContext context = new AuthContext())
            {
                context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                custFavoriteItemDcs = context.CustFavoriteItems.Where(x => x.CustomerId == customerId).Select(x => new CustFavoriteItemDc
                {
                    CustomerId = x.CustomerId,
                    id = x.id,
                    IsLike = x.IsLike,
                    ItemId = x.ItemId
                }).ToList();

            }
            return Request.CreateResponse(HttpStatusCode.OK, custFavoriteItemDcs);
        }

        [Route("Mobile")]
        [HttpGet]
        public HttpResponseMessage CheckMobile(string Mobile)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    logger.Info("Get Peoples: ");
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string email = "";
                    var identity = User.Identity as ClaimsIdentity;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }

                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");

                    var RDMobile = db.Customers.Where(x => x.Mobile == Mobile).FirstOrDefault();
                    var warehouses = db.Warehouses.FirstOrDefault(x => RDMobile.Warehouseid == x.WarehouseId);
                    RDMobile.WarehouseName = warehouses.WarehouseName;
                    return Request.CreateResponse(HttpStatusCode.OK, RDMobile);



                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }



            }
        }
        [Route("Email")]
        [HttpGet]
        public HttpResponseMessage CheckEmail(string Email)
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    logger.Info("Get Peoples: ");
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string email = "";
                    var identity = User.Identity as ClaimsIdentity;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }

                        if (claim.Type == "Warehouseid")
                        {
                            Warehouse_id = int.Parse(claim.Value);
                        }
                        if (claim.Type == "email")
                        {
                            email = claim.Value;
                        }
                    }
                    logger.Info("End Get Company: ");

                    var RDMobile = context.Customers.Where(x => x.Emailid == Email).FirstOrDefault();
                    var warehouses = context.Warehouses.FirstOrDefault(x => RDMobile.Warehouseid == x.WarehouseId);
                    RDMobile.WarehouseName = warehouses.WarehouseName;
                    return Request.CreateResponse(HttpStatusCode.OK, RDMobile);



                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, false);
                }
            }
        }
        //get Custearehouse customer
        [Route("CustWarehouseCustomer")]
        [HttpGet]
        //public CustWarehouse CustWarehouseCustomer(int CustomerId)
        public Customer CustomerCustomer(int CustomerId)
        {
            logger.Info("start City: ");
            using (AuthContext db = new AuthContext())
            {
                Customer customer = new Customer();
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    //customer = db.CustWarehouseDB.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                    customer = db.Customers.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                    customer.Agents = customer.ClusterId.HasValue ? (from a in db.ClusterAgent
                                                                     join p in db.Peoples on a.AgentId equals p.PeopleID
                                                                     where !a.Deleted && a.active
                                                                     && a.CompanyId == compid && a.ClusterId == customer.ClusterId
                                                                     select new AgentDTO
                                                                     {
                                                                         AgentCode = p.AgentCode,
                                                                         AgentName = p.DisplayName
                                                                     }).ToList() : new List<AgentDTO>();

                    logger.Info("End  Customer: ");
                    return customer;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }
        #region get customerhistory
        [Route("customerhistory")]
        [HttpGet]
        public dynamic customerhistory(int CustomerId)
        {


            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                int CompanyId = compid;
                using (var db = new AuthContext())
                {

                    var data = db.CustomerHistoryDB.Where(x => x.CustomerId == CustomerId).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion


        #region get customerImages
        [Route("customerImages")]
        [HttpGet]
        public dynamic customerImages(int CustomerId)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                int CompanyId = compid;
                using (var db = new AuthContext())
                {
                    var data = db.Customers.Where(x => x.CustomerId == CustomerId).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region RBL Registration 
        [Route("RBLCustInformation")]
        [HttpPost]
        public HttpResponseMessage Post(RBLCustomerInformation obj)
        {
            logger.Info("start Registration RBLCustInformation: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {

                    customerDetail res;
                    Customer customer = db.Customers.Where(x => x.CustomerId == obj.customerId).FirstOrDefault();
                    RBLCustomerInformation RblCust = db.RBLCustomerInformationDB.Where(x => x.customerId == obj.customerId).FirstOrDefault();
                    if (customer == null)
                    {

                        res = new customerDetail()
                        {
                            RBLCustomerInformation = RblCust,
                            Status = false,
                            Message = "Do Registration of customer first then update RBLCustInformation."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else if (RblCust == null)
                    {

                        //if (obj.dateOfBirth != null)
                        //{
                        //    try
                        //    {
                        //        DateTime dateTime11 = Convert.ToDateTime(obj.dateOfBirth);
                        //        obj.dateOfBirth = dateTime11;
                        //    }
                        //    catch (Exception sdff)
                        //    {
                        //    }

                        //}
                        obj.createdDate = indianTime;
                        obj.updatedDate = indianTime;
                        obj.deleted = false;
                        db.RBLCustomerInformationDB.Add(obj);
                        db.Commit();


                        res = new customerDetail()
                        {
                            RBLCustomerInformation = obj,
                            Status = true,
                            Message = "Registration of RBLCustInformation Done."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        RblCust.peopleId = obj.peopleId;
                        RblCust.gender = obj.gender;
                        RblCust.title = obj.title;
                        RblCust.firstName = obj.firstName;
                        RblCust.lastName = obj.lastName;
                        RblCust.dateOfBirth = obj.dateOfBirth;
                        RblCust.motherMaidenName = obj.motherMaidenName;
                        RblCust.community = obj.community;
                        RblCust.maritalStatus = obj.maritalStatus;
                        RblCust.grossIncome = obj.grossIncome;
                        RblCust.doNotCall = obj.doNotCall;
                        RblCust.panNumber = obj.panNumber;
                        RblCust.address1 = obj.address1;
                        RblCust.addressFormat1 = obj.addressFormat1;
                        RblCust.addressType1 = obj.addressType1;
                        RblCust.addressLabel1 = obj.addressLabel1;
                        RblCust.addressLine11 = obj.addressLine11;
                        RblCust.addressLine21 = obj.addressLine21;
                        RblCust.addressLine31 = obj.addressLine31;
                        RblCust.city1 = obj.city1;
                        RblCust.state1 = obj.state1;
                        RblCust.postalCode1 = obj.postalCode1;
                        RblCust.phone1 = obj.phone1;
                        RblCust.shopImage = obj.shopImage;
                        RblCust.createdBy = obj.createdBy;
                        RblCust.updatedDate = indianTime;
                        db.RBLCustomerInformationDB.Attach(RblCust);
                        db.Entry(RblCust).State = EntityState.Modified;
                        db.Commit();

                        res = new customerDetail()
                        {
                            RBLCustomerInformation = RblCust,
                            Status = true,
                            Message = "RBLCustInformation Updated."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Something issue in Customer" + ex);
                }
            }
        }

        #endregion

        #region  Get RBL Data Using CustomerId 
        [Route("GetRBLCustInformation")]
        [HttpGet]
        public HttpResponseMessage Get(int customerId)
        {
            logger.Info("start get RBLCustInformation: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    customerDetail res;
                    RBLCustomerInformation RblCust = db.RBLCustomerInformationDB.Where(x => x.customerId == customerId).FirstOrDefault();
                    if (RblCust != null)
                    {
                        res = new customerDetail()
                        {
                            RBLCustomerInformation = RblCust,
                            Status = true,
                            Message = "RBLCustInformation Found."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new customerDetail()
                        {
                            RBLCustomerInformation = RblCust,
                            Status = false,
                            Message = "No record found."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Something issue " + ex);
                }
            }
        }
        #endregion

        #region SPA: Add customer

        /// <summary>
        /// created by 19/01/2019
        /// add customer for sales person app
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(Customer))]
        [Route("addcust")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addcust(Customer customer)
        {
            customerDetail res;
            logger.Info("start addCustomer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    Customer c = new Customer();
                    Customer customers = db.Customers.Where(s => s.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();
                    People people = db.Peoples.Where(q => q.PeopleID == customer.ExecutiveId).FirstOrDefault();
                    City city = db.Cities.Where(x => x.CityName.Trim().ToLower() == customer.City.Trim().ToLower() && x.Deleted == false).FirstOrDefault();

                    Warehouse wh = new Warehouse();

                    if (people != null)
                    {
                        wh = db.Warehouses.Where(x => x.WarehouseId == people.WarehouseId && x.Deleted == false).FirstOrDefault();
                    }

                    if (!string.IsNullOrEmpty(customer.RefNo))
                    {
                        var checkgst = false;
                        if (customers == null)
                            checkgst = db.Customers.Any(x => x.RefNo == customer.RefNo);
                        else
                            checkgst = db.Customers.Any(x => x.RefNo == customer.RefNo && x.CustomerId != customers.CustomerId);

                        if (checkgst)
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "Gst Already Exsits."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }

                    if (!string.IsNullOrEmpty(customer.LicenseNumber.Trim()) && customer.LicenseNumber != "0")
                    {
                        var checkgst = db.Customers.Where(x => x.LicenseNumber == customer.LicenseNumber && x.CustomerId != customer.CustomerId).Count();
                        if (checkgst > 0)
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "License Number Already Exsits."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }


                    if (customer.LicenseExpiryDate != null && customer.LicenseExpiryDate < DateTime.Now)
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "License Expiry Date Already Expired, Please enter valid date"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }


                    if (customers != null && (customers.CustomerDocumentStatus == (int)CustomerDocumentStatusEnum.Verified || customers.ShippingAddressStatus == (int)ShippingAddressStatusEnum.PhysicalVerified || customers.ShippingAddressStatus == (int)ShippingAddressStatusEnum.VirtualVerified))
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "you can't update this customer profile, because customer document or address is verified"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }
                    if (customers != null && (customers.CustomerVerify == "Full Verified" || customers.CustomerVerify == "Partial Verified"))
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "you can't update this customer profile, because customer status is verified"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }

                    #region for Duplicate PAN no. check
                    if (!string.IsNullOrEmpty(customer.PanNo.Trim()))
                    {
                        Customer IsExistPAN = db.Customers.Where(x => x.PanNo.ToLower() == customer.PanNo.ToLower()).FirstOrDefault();
                        if (IsExistPAN != null)
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "PAN Already Exists."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    #endregion


                    if (customers == null)
                    {
                       

                        Cluster dd = null;
                        c.Distance = 0;
                        #region CustomerAddress status and lt lg :14/07/2022
                        if (customer.CustomerAddress != null)
                        {
                            c.ShippingAddress = null;
                            if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressLineOne))
                            {
                                c.ShippingAddress = customer.CustomerAddress.AddressLineOne + ",";
                            }
                            if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressLineTwo))
                            {
                                c.ShippingAddress += customer.CustomerAddress.AddressLineTwo + ",";
                            }
                            if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressText))
                            {
                                c.ShippingAddress += customer.CustomerAddress.AddressText;
                            }
                            if (!string.IsNullOrEmpty(customer.CustomerAddress.ZipCode))
                            {
                                c.ShippingAddress += "," + customer.CustomerAddress.ZipCode;
                            }
                            c.lat = customer.CustomerAddress.AddressLat;
                            c.lg = customer.CustomerAddress.AddressLng;
                            c.Addresslat = customer.CustomerAddress.AddressLat;
                            c.Addresslg = customer.CustomerAddress.AddressLng;
                        }
                        //LicenseExpiryDate
                        if (customer.CustomerDocTypeMasterId > 0 && customer.CustomerDocTypeMasterId == 1 && customer.LicenseExpiryDate != null)
                        {
                            c.GstExpiryDate = customer.LicenseExpiryDate;
                        }
                        else if (customer.CustomerDocTypeMasterId > 0 && customer.LicenseExpiryDate != null)
                        {
                            c.LicenseExpiryDate = customer.LicenseExpiryDate;
                        }
                        #endregion

                        c.Skcode = skcode();
                        c.BAGPSCoordinates = customer.BAGPSCoordinates;
                        c.BillingAddress = customer.BillingAddress;
                        c.ZipCode = customer.ZipCode;
                        c.BillingCity = customer.BillingCity;
                        c.BillingState = c.BillingState = (c.City == c.BillingCity) == true ? c.State : customer.BillingState;

                        c.BillingZipCode = customer.BillingZipCode;
                        c.ExecutiveId = people.PeopleID;
                        c.FSAAI = customer.FSAAI;
                        c.LandMark = customer.LandMark;
                        c.Mobile = customer.Mobile;
                        c.MobileSecond = customer.MobileSecond;
                        c.MonthlyTurnOver = customer.MonthlyTurnOver;
                        c.Name = customer.Name;
                        c.Password = customer.Password;
                        c.RefNo = customer.RefNo;
                        c.SAGPSCoordinates = customer.SAGPSCoordinates;
                        c.ShopName = customer.ShopName;
                        c.SizeOfShop = customer.SizeOfShop;
                        c.UploadRegistration = customer.UploadRegistration;

                        if (customer.Shoplat > 0)
                        {
                            c.Shoplat = customer.Shoplat;
                            c.Shoplg = customer.Shoplg;
                        }
                        c.ShippingCity = city != null ? city.CityName : "";
                        c.State = city != null ? city.StateName : "";
                        c.Cityid = wh.Cityid;
                        c.City = wh.CityName;
                        c.AadharNo = customer.AadharNo;//sudhir
                        c.PanNo = customer.PanNo;//sudhir
                        c.AreaName = customer.AreaName;
                        #region to assign cluster ID and determine if it is in cluster or not.
                        // < summary >
                        // Updated by 28 - 06 - 2019
                        // </ summary > tejas to assign cluster and refine if cx is in region or not
                        if (customer.lat != 0 && customer.lg != 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(customer.lat).Append("', '").Append(customer.lg).Append("')");
                            var clusterId = db.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                            if (!clusterId.HasValue)
                            {
                                customer.InRegion = false;
                            }
                            else
                            {
                                var agent = db.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active);

                                if (agent != null && agent.AgentId > 0)
                                    customer.AgentCode = Convert.ToString(agent.AgentId);


                                customer.ClusterId = clusterId;
                                dd = db.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                customer.ClusterName = dd.ClusterName;
                                customer.InRegion = true;
                            }
                        }
                        #endregion

                        if (dd != null)
                        {
                            c.Warehouseid = dd.WarehouseId;
                            c.WarehouseName = dd.WarehouseName;
                            c.ClusterId = dd.ClusterId;
                            c.ClusterName = dd.ClusterName;
                            c.City = dd.CityName;
                            c.Cityid = dd.CityId;
                        }

                        c.CompanyId = wh.CompanyId;
                        c.Shopimage = customer.Shopimage;
                        c.Active = false;//change on demand of Salesman 
                        c.IsCityVerified = true;
                        c.IsSignup = true;
                        c.CreatedBy = people.DisplayName;
                        c.CreatedDate = indianTime;
                        c.UpdatedDate = indianTime;
                        c.AnniversaryDate = customer.AnniversaryDate;
                        c.DOB = customer.DOB;
                        c.WhatsappNumber = customer.WhatsappNumber;
                        c.LicenseNumber = customer.LicenseNumber;     //tejas 07-06-19
                        c.UploadLicensePicture = customer.UploadLicensePicture;
                        c.UploadGSTPicture = customer.UploadGSTPicture;
                        if (!string.IsNullOrEmpty(c.RefNo))
                        {
                            var custGstVerifys = db.CustGSTverifiedRequestDB.Where(x => x.RefNo == customer.RefNo).ToList();
                            if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                            {
                                var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                                var state = db.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                                c.BillingCity = gstVerify.City;
                                c.BillingState = state != null ? state.StateName : gstVerify.State;
                                c.BillingZipCode = gstVerify.Zipcode;
                                c.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                                c.NameOnGST = gstVerify.Name;
                                c.UploadGSTPicture = customer.UploadGSTPicture;
                            }

                        }

                        if (string.IsNullOrEmpty(c.BillingAddress))
                        {
                            c.BillingCity = c.City;
                            c.BillingState = c.State;
                            c.BillingZipCode = c.ZipCode;
                            c.BillingAddress = c.ShippingAddress;
                        }
                        if (c.BillingAddress == c.ShippingAddress)
                        {
                            c.BillingAddress1 = c.AreaName;
                        }
                        db.Customers.Add(c);
                        #region Referral Customer
                        if (!string.IsNullOrEmpty(customer.ReferralSkCode))
                        {
                            var customerReferralConfiguration = db.CustomerReferralConfigurationDb.Where(x => x.CityId == c.Cityid && x.ReferralType == 2 && x.IsActive == true && x.IsDeleted == false).ToList();
                            var referralWallet = db.ReferralWalletDb.Where(x => x.SkCode == c.Skcode).FirstOrDefault();
                            if (customerReferralConfiguration != null && referralWallet == null)
                            {
                                foreach (var item in customerReferralConfiguration)
                                {
                                    db.ReferralWalletDb.Add(new Model.CustomerReferral.ReferralWallet
                                    {
                                        SkCode = customer.Skcode,
                                        ReferralSkCode = customer.ReferralSkCode,
                                        CreatedBy = customer.CustomerId,
                                        ReferralWalletPoint = item.ReferralWalletPoint,
                                        CustomerWalletPoint = item.CustomerWalletPoint,
                                        IsActive = true,
                                        CreatedDate = DateTime.Now,
                                        IsDeleted = false,
                                        ReferralType = Convert.ToInt32(ReferralType.People),
                                        OnOrder = item.OnOrder,
                                        CustomerReferralConfigurationId = item.Id,
                                        IsUsed = 0
                                    });
                                }
                            }
                        }
                        #endregion


                        if (db.Commit() > 0 && ((!string.IsNullOrEmpty(customer.LicenseNumber) && customer.CustomerDocTypeMasterId > 0) || (!string.IsNullOrEmpty(c.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture))))
                        {
                            var docMaster = db.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();
                            if (!string.IsNullOrEmpty(c.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                            {
                                db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = c.CustomerId,
                                    IsActive = true,
                                    CreatedBy = people.PeopleID,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = docMaster.FirstOrDefault(x => x.DocType == "GST").Id,
                                    DocPath = customer.UploadGSTPicture,
                                    IsDeleted = false
                                });
                            }

                            if (!string.IsNullOrEmpty(c.LicenseNumber) && customer.CustomerDocTypeMasterId > 0)
                            {
                                db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = c.CustomerId,
                                    IsActive = true,
                                    CreatedBy = people.PeopleID,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = customer.CustomerDocTypeMasterId,
                                    DocPath = customer.UploadRegistration,
                                    IsDeleted = false
                                });
                            }

                            db.Commit();
                        }
                        //var customerLatLngVerify = new CustomerLatLngVerify
                        //{
                        //    CaptureImagePath = c.Shopimage,
                        //    CreatedBy = people.PeopleID,
                        //    CreatedDate = DateTime.Now,
                        //    CustomerId = c.CustomerId,
                        //    IsActive = true,
                        //    IsDeleted = false,
                        //    lat = c.lat,
                        //    lg = c.lg,
                        //    Newlat = c.lat,
                        //    Newlg = c.lg,
                        //    NewShippingAddress = c.ShippingAddress,
                        //    ShippingAddress = c.ShippingAddress,
                        //    ShopFound = 0,
                        //    ShopName = c.ShopName,
                        //    Skcode = c.Skcode,
                        //    LandMark = c.LandMark,
                        //    Status = 1,
                        //    Nodocument = customer.CustomerDocTypeMasterId > 0 ? true : false,
                        //    Aerialdistance = 0,
                        //    AppType = (int)AppEnum.SalesApp
                        //};
                        //db.CustomerLatLngVerify.Add(customerLatLngVerify);

                        #region CustomerAddress :12/07/2022
                        if (customer.CustomerAddress != null)
                        {
                            customer.CustomerAddress.CustomerId = c.CustomerId;
                            CustomerAddressHelper customerAddressHelper = new CustomerAddressHelper();
                            bool IsInserted = customerAddressHelper.InsertCustomerAddress(customer.CustomerAddress, people.PeopleID);
                        }
                        #endregion
                        #region CODLimitCustomers
                        var codLimit = db.CompanyDetailsDB.FirstOrDefault(x => x.IsActive == true && x.IsDeleted == false);
                        CODLimitCustomer codLimitCustomer = new CODLimitCustomer();
                        codLimitCustomer.CustomerId = c.CustomerId;
                        codLimitCustomer.CODLimit = codLimit.CODLimit;
                        codLimitCustomer.IsActive = true;
                        codLimitCustomer.IsDeleted = false;
                        codLimitCustomer.IsCustomCODLimit = true;
                        codLimitCustomer.CreatedDate = DateTime.Now;
                        db.CODLimitCustomers.Add(codLimitCustomer);
                        #endregion
                        db.Commit();
                        res = new customerDetail()
                        {
                            customers = c,
                            Status = true,
                            Message = "Registration successfully."
                        };

                        #region gamification entry for New Customer sign up 

                        GamificationController gamification = new GamificationController();
                        var result = gamification.NewCustomerSignUp(c.CustomerId, c.Skcode);

                        #endregion

                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {

                        long OldDocId = customers.CustomerDocTypeMasterId;
                        string OldShippingAddress = customers.ShippingAddress;
                        Cluster dd = null;
                        customers.BAGPSCoordinates = customer.BAGPSCoordinates;
                        customers.BillingAddress = customer.BillingAddress;
                        #region CustomerAddress status and lt lg :14/07/2022

                        if (customer.CustomerAddress != null)
                        {
                            customers.ShippingAddress = null;
                            if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressLineOne))
                            {
                                customers.ShippingAddress = customer.CustomerAddress.AddressLineOne + ",";
                            }
                            if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressLineTwo))
                            {
                                customers.ShippingAddress += customer.CustomerAddress.AddressLineTwo + ",";
                            }
                            if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressText))
                            {
                                customers.ShippingAddress += customer.CustomerAddress.AddressText;
                            }
                            if (!string.IsNullOrEmpty(customer.CustomerAddress.ZipCode))
                            {
                                customers.ShippingAddress += "," + customer.CustomerAddress.ZipCode;
                            }
                            customers.lat = customer.CustomerAddress.AddressLat;
                            customers.lg = customer.CustomerAddress.AddressLng;
                            customers.Addresslat = customer.CustomerAddress.AddressLat;
                            customers.Addresslg = customer.CustomerAddress.AddressLng;

                        }
                        //LicenseExpiryDate
                        if (customer.CustomerDocTypeMasterId > 0 && customer.CustomerDocTypeMasterId == 1 && customer.LicenseExpiryDate != null)
                        {
                            customers.GstExpiryDate = customer.LicenseExpiryDate;
                        }
                        else if (customer.CustomerDocTypeMasterId > 1 && customer.LicenseExpiryDate != null)
                        {
                            customers.LicenseExpiryDate = customer.LicenseExpiryDate;
                        }
                        #endregion

                        customers.ZipCode = customer.ZipCode;
                        customers.BillingCity = customer.BillingCity;
                        customers.BillingState = customers.BillingState = (customers.City == customers.BillingCity) == true ? customers.State : customer.BillingState; //customer.BillingState;
                        customers.BillingZipCode = customer.BillingZipCode;
                        customers.ExecutiveId = people.PeopleID;
                        customers.FSAAI = customer.FSAAI;
                        customers.LandMark = customer.LandMark;
                        customers.Mobile = customer.Mobile;
                        customers.MobileSecond = customer.MobileSecond;
                        customers.MonthlyTurnOver = customer.MonthlyTurnOver;
                        customers.Name = customer.Name;
                        customers.Password = customer.Password;
                        customers.RefNo = customer.RefNo;
                        customers.SAGPSCoordinates = customer.SAGPSCoordinates;
                        customers.ShopName = customer.ShopName;
                        customers.SizeOfShop = customer.SizeOfShop;
                        customers.UploadRegistration = customer.UploadRegistration;

                        customers.AreaName = customer.AreaName;
                        if (customer.Shoplat > 0)
                        {
                            customers.Shoplat = customer.Shoplat;
                            customers.Shoplg = customer.Shoplg;
                        };
                        customers.ShippingCity = city != null ? city.CityName : "";
                        customers.State = city != null ? city.StateName : "";
                        customers.Cityid = wh.Cityid;
                        customers.City = wh.CityName;
                        customers.AadharNo = customer.AadharNo;//sudhir
                        customers.PanNo = customer.PanNo;//sudhir
                        #region to assign cluster ID and determine if it is in cluster or not.
                        // < summary >
                        // Updated by 28 - 06 - 2019
                        // </ summary > tejas to assign cluster and refine if cx is in region or not
                        if (customers.lat != 0 && customers.lg != 0)
                        {
                            var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(customers.lat).Append("', '").Append(customers.lg).Append("')");
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
                                dd = db.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                customers.ClusterName = dd.ClusterName;
                                customers.InRegion = true;
                            }
                        }
                        #endregion

                        if (dd != null)
                        {
                            customers.Warehouseid = dd.WarehouseId;
                            customers.WarehouseName = dd.WarehouseName;
                            customers.ClusterId = dd.ClusterId;
                            customers.ClusterName = dd.ClusterName;
                            customers.City = dd.CityName;
                            customers.Cityid = dd.CityId;

                        }

                        customers.CompanyId = wh.CompanyId;
                        customers.Shopimage = customer.Shopimage;

                        customers.Active = false;//change on demand of Salesman 
                        customers.IsCityVerified = true;
                        customers.IsSignup = true;
                        customers.LastModifiedBy = people.DisplayName;
                        customers.UpdatedDate = indianTime;
                        customers.AnniversaryDate = customer.AnniversaryDate;
                        customers.DOB = customer.DOB;
                        customers.WhatsappNumber = customer.WhatsappNumber;
                        customers.LicenseNumber = customer.LicenseNumber;     //tejas 07-06-19
                        customers.UploadLicensePicture = customer.UploadLicensePicture;
                        customers.UploadGSTPicture = customer.UploadGSTPicture;
                        if (!string.IsNullOrEmpty(customers.RefNo))
                        {
                            var custGstVerifys = db.CustGSTverifiedRequestDB.Where(x => x.RefNo == customer.RefNo).ToList();
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
                            customers.BillingAddress1 = customers.AreaName;
                            customers.BillingAddress = customers.ShippingAddress;
                        }

                        db.Entry(customers).State = EntityState.Modified;


                        #region Referral Customer
                        if (!string.IsNullOrEmpty(customer.ReferralSkCode))
                        {
                            var customerReferralConfiguration = db.CustomerReferralConfigurationDb.Where(x => x.CityId == customers.Cityid && x.ReferralType == 2 && x.IsActive == true && x.IsDeleted == false).ToList();
                            var referralWallet = db.ReferralWalletDb.Where(x => x.SkCode == customers.Skcode).FirstOrDefault();
                            if (customerReferralConfiguration != null && referralWallet == null)
                            {
                                foreach (var item in customerReferralConfiguration)
                                {
                                    db.ReferralWalletDb.Add(new Model.CustomerReferral.ReferralWallet
                                    {
                                        SkCode = customers.Skcode,
                                        ReferralSkCode = customer.ReferralSkCode,
                                        CreatedBy = customers.CustomerId,
                                        ReferralWalletPoint = item.ReferralWalletPoint,
                                        CustomerWalletPoint = item.CustomerWalletPoint,
                                        IsActive = true,
                                        CreatedDate = DateTime.Now,
                                        IsDeleted = false,
                                        ReferralType = Convert.ToInt32(ReferralType.People),
                                        OnOrder = item.OnOrder,
                                        CustomerReferralConfigurationId = item.Id,
                                        IsUsed = 0
                                    });
                                }
                            }
                        }
                        #endregion
                        if (db.Commit() > 0 && ((!string.IsNullOrEmpty(customer.LicenseNumber) && customer.CustomerDocTypeMasterId > 0) || (!string.IsNullOrEmpty(customer.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture))))
                        {
                            var docMaster = db.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();
                            if (!string.IsNullOrEmpty(customers.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                            {
                                db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = customers.CustomerId,
                                    IsActive = true,
                                    CreatedBy = people.PeopleID,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = docMaster.FirstOrDefault(x => x.DocType == "GST").Id,
                                    DocPath = customer.UploadGSTPicture,
                                    IsDeleted = false
                                });
                            }

                            if (!string.IsNullOrEmpty(customers.LicenseNumber) && customer.CustomerDocTypeMasterId > 0)
                            {
                                db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                                {
                                    CustomerId = customers.CustomerId,
                                    IsActive = true,
                                    CreatedBy = people.PeopleID,
                                    CreatedDate = DateTime.Now,
                                    CustomerDocTypeMasterId = customer.CustomerDocTypeMasterId,
                                    DocPath = customer.UploadRegistration,
                                    IsDeleted = false
                                });
                            }
                            db.Commit();
                        }
                        //if (customers.Warehouseid > 0 && (customers.ShippingAddress != OldShippingAddress || customers.CustomerDocTypeMasterId != OldDocId))
                        //{
                        //var custVerifies = db.CustomerLatLngVerify.Where(x => x.CustomerId == customers.CustomerId && x.AppType == (int)AppEnum.SalesApp && x.Status == 1).ToList();
                        //if (custVerifies != null && custVerifies.Any())
                        //{
                        //    foreach (var item in custVerifies)
                        //    {
                        //        item.IsActive = false;
                        //        item.IsDeleted = true;
                        //        item.ModifiedBy = people.PeopleID;
                        //        item.ModifiedDate = DateTime.Now;
                        //        db.Entry(item).State = EntityState.Modified;
                        //    }
                        //}

                        //var customerLatLngVerify = new CustomerLatLngVerify
                        //{
                        //    CaptureImagePath = customers.Shopimage,
                        //    CreatedBy = people.PeopleID,
                        //    CreatedDate = DateTime.Now,
                        //    CustomerId = customers.CustomerId,
                        //    IsActive = true,
                        //    IsDeleted = false,
                        //    lat = customers.lat,
                        //    lg = customers.lg,
                        //    Newlat = customers.lat,
                        //    Newlg = customers.lg,
                        //    NewShippingAddress = customers.ShippingAddress,
                        //    ShippingAddress = customers.ShippingAddress,
                        //    ShopFound = 1,
                        //    ShopName = customers.ShopName,
                        //    Skcode = customers.Skcode,
                        //    LandMark = customers.LandMark,
                        //    Status = 1,
                        //    Nodocument = customer.CustomerDocTypeMasterId > 0 ? true : false,
                        //    Aerialdistance = 0,
                        //    AppType = (int)AppEnum.SalesApp
                        //};
                        //db.CustomerLatLngVerify.Add(customerLatLngVerify);

                        #region CustomerAddress :12/07/2022
                        if (customer.CustomerAddress != null)
                        {
                            customer.CustomerAddress.CustomerId = customers.CustomerId;
                            CustomerAddressHelper customerAddressHelper = new CustomerAddressHelper();
                            bool IsInserted = customerAddressHelper.InsertCustomerAddress(customer.CustomerAddress, people.PeopleID);
                        }
                        #endregion
                        #region Channel

                        var CustomerChannelExists = db.CustomerChannelMappings.Where(x => x.CustomerId == customers.CustomerId && x.IsDeleted == false).FirstOrDefault();
                        if (CustomerChannelExists == null)
                        {
                            List<CustomerChannelMapping> customerChannels = new List<CustomerChannelMapping>();
                            var StoreIds = db.StoreDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).Distinct().ToList();
                            if (StoreIds != null && StoreIds.Count > 0 && customer.ChannelId > 0)
                            {
                                StoreIds.ForEach(x =>
                                {
                                    customerChannels.Add(new CustomerChannelMapping
                                    {
                                        CustomerId = customers.CustomerId,
                                        StoreId = (int)x,
                                        ChannelMasterId = customer.ChannelId,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedDate = DateTime.Now,
                                        CreatedBy = customer.ExecutiveId ?? 0,
                                        ModifiedBy = null,
                                        ModifiedDate = null
                                    });
                                });
                            }
                            if (customerChannels != null && customerChannels.Count > 0)
                                db.CustomerChannelMappings.AddRange(customerChannels);
                        }
                        #endregion
                        //}

                        db.Commit();
                        res = new customerDetail()
                        {
                            customers = customers,
                            Status = true,
                            Message = "Registration successfully."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + (ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString()));
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion




        #region SPA: Update customer

        /// <summary>
        /// created by 01/04/2019
        /// Update customer for sales person app
        /// Created By raj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [ResponseType(typeof(Customer))]
        [Route("Updatecust")]
        [AcceptVerbs("PUT")]

        public async Task<customerDetail> Updatecust(Customer customer)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            customerDetail res;
            logger.Info("start UpdateCustomer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    Customer c = db.Customers.Where(s => s.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();
                    //Customer customers = db.Customers.Where(s => s.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();
                    People people = db.Peoples.Where(q => q.PeopleID == customer.ExecutiveId).SingleOrDefault();
                    //City city = db.Cities.Where(x => x.Cityid == customer.Cityid && x.Deleted == false).SingleOrDefault();
                    Warehouse wh = db.Warehouses.Where(x => x.WarehouseId == people.WarehouseId && x.Deleted == false).FirstOrDefault();

                    //logger.Info("End  UpdateCustomer: ");

                    //c.Skcode = skcode();
                    c.BAGPSCoordinates = customer.BAGPSCoordinates;
                    c.BillingAddress = customer.BillingAddress;
                    c.ShippingAddress = customer.ShippingAddress;
                    //c.ExecutiveId = people.PeopleID;
                    c.FSAAI = customer.FSAAI;
                    c.LandMark = customer.LandMark;
                    c.Mobile = customer.Mobile;
                    c.MobileSecond = customer.MobileSecond;
                    c.MonthlyTurnOver = customer.MonthlyTurnOver;
                    c.Name = customer.Name;
                    c.Password = customer.Password;
                    c.RefNo = customer.RefNo;
                    c.SAGPSCoordinates = customer.SAGPSCoordinates;
                    c.ShopName = customer.ShopName;
                    c.ShippingAddress = customer.ShippingAddress;//sudhir
                    c.SizeOfShop = customer.SizeOfShop;//sudhir
                    c.UploadRegistration = customer.UploadRegistration;
                    c.lat = customer.lat;
                    c.lg = customer.lg;
                    c.City = wh.CityName;
                    c.Cityid = wh.Cityid;
                    c.Warehouseid = people.WarehouseId;
                    c.WarehouseName = wh.WarehouseName;
                    c.CompanyId = people.CompanyId;
                    c.Shopimage = customer.Shopimage;
                    c.AnniversaryDate = customer.AnniversaryDate;
                    c.DOB = customer.DOB;
                    c.WhatsappNumber = customer.WhatsappNumber;
                    c.LicenseNumber = customer.LicenseNumber;
                    c.UpdatedDate = indianTime;
                    c.CustomerVerify = "Pending For Activation";
                    c.AadharNo = customer.AadharNo;//sudhir
                    c.PanNo = customer.PanNo;//sudhir         
                    c.ExecutiveId = people.PeopleID;
                    c.ExecutiveName = people.DisplayName;

                    db.Entry(c).State = EntityState.Modified;

                    //CustWarehouse cs = db.CustWarehouseDB.Where(x => x.CustomerId == c.CustomerId).FirstOrDefault();
                    //Customer cs = db.Customers.Where(x => x.CustomerId == c.CustomerId).FirstOrDefault();
                    //cs.ExecutiveId = people.PeopleID;
                    //cs.ExecutiveName = people.DisplayName;
                    //cs.CustomerId = c.CustomerId;
                    //cs.Warehouseid = wh.WarehouseId;
                    //cs.WarehouseName = wh.WarehouseName;
                    //cs.CompanyId = wh.CompanyId;
                    //cs.UpdatedDate = DateTime.Now;
                    ////db.CustWarehouseDB.Attach(cs);
                    //db.Entry(c).State = EntityState.Modified;
                    db.Commit();


                    res = new customerDetail()
                    {
                        customers = c,
                        Status = true,
                        Message = "Update successfully."
                    };
                    return res;
                }
                catch (Exception ex)
                {

                    res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return res;
                }
            }
        }
        #endregion


        #region SPA: UpdatecustForMyLead
        /// <summary>
        /// created by 20/02/2020
        /// Update customer for My lead Agent  App 
        /// Created By raj
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [Route("UpdatecustForMyLead")]
        [HttpPut]
        public async Task<customerDetail> UpdatecustForMyLead(Customer customer)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            customerDetail res;
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    if (customer.LicenseExpiryDate != null && customer.LicenseExpiryDate < DateTime.Now)
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "License Expiry Date Already Expired, Please enter valid date"
                        };
                        return res;

                    }
                    Customer c = db.Customers.Where(s => s.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();
                    if (c.CustomerVerify == "Full Verified" || c.CustomerVerify == "Partial Verified")
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer status is already in :" + c.CustomerVerify
                        };
                        return res;
                    };

                    if (c != null && (c.CustomerDocumentStatus == (int)CustomerDocumentStatusEnum.Verified || c.ShippingAddressStatus == (int)ShippingAddressStatusEnum.PhysicalVerified || c.ShippingAddressStatus == (int)ShippingAddressStatusEnum.VirtualVerified))
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "you can't update this customer profile, because customer document or address is verified"
                        };
                        return res;

                    }


                    City city = db.Cities.Where(x => x.Cityid == customer.Cityid && x.Deleted == false).FirstOrDefault();
                    People people = db.Peoples.Where(q => q.PeopleID == customer.ExecutiveId).FirstOrDefault();
                    Warehouse wh = db.Warehouses.Where(x => x.WarehouseId == people.WarehouseId && x.Deleted == false).FirstOrDefault();
                    if (wh == null)
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Primary Warehouse is not mapped to login user :" + people.DisplayName
                        };
                        return res;
                    }

                    if (!string.IsNullOrEmpty(customer.RefNo))
                    {
                        var checkgst = db.Customers.Where(x => x.RefNo == customer.RefNo && x.CustomerId != c.CustomerId).Count();
                        if (checkgst > 0)
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "Gst Already Exsits."
                            };
                            return res;
                        }
                    }

                    if (!string.IsNullOrEmpty(customer.LicenseNumber.Trim()) && customer.LicenseNumber != "0")
                    {
                        var checkgst = db.Customers.Where(x => x.LicenseNumber == customer.LicenseNumber && x.CustomerId != c.CustomerId).Count();

                        if (checkgst > 0)
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "License Number Already Exsits."
                            };
                            return res;
                        }
                    }

                    bool TrackRequest = false;
                    if (customer.lat != c.lat || customer.lg != c.lg || customer.RefNo != c.RefNo || customer.LicenseNumber != c.LicenseNumber || customer.ShippingAddress != c.ShippingAddress)
                        TrackRequest = true;

                    c.BAGPSCoordinates = customer.BAGPSCoordinates;
                    c.BillingAddress = customer.BillingAddress;

                    #region CustomerAddress status and lt lg :14/07/2022

                    if (customer.CustomerAddress != null)
                    {
                        c.ShippingAddress = null;
                        if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressLineOne))
                        {
                            c.ShippingAddress = customer.CustomerAddress.AddressLineOne + ",";
                        }
                        if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressLineTwo))
                        {
                            c.ShippingAddress += customer.CustomerAddress.AddressLineTwo + ",";
                        }
                        if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressText))
                        {
                            c.ShippingAddress += customer.CustomerAddress.AddressText;
                        }
                        if (!string.IsNullOrEmpty(customer.CustomerAddress.ZipCode))
                        {
                            c.ShippingAddress += "," + customer.CustomerAddress.ZipCode;
                        }

                        c.lat = customer.CustomerAddress.AddressLat;
                        c.lg = customer.CustomerAddress.AddressLng;

                        c.Addresslat = customer.CustomerAddress.AddressLat;
                        c.Addresslg = customer.CustomerAddress.AddressLng;

                    }
                    //LicenseExpiryDate
                    if (customer.CustomerDocTypeMasterId > 0 && customer.CustomerDocTypeMasterId == 1 && customer.LicenseExpiryDate != null)
                    {
                        c.GstExpiryDate = customer.LicenseExpiryDate;
                    }
                    else if (customer.CustomerDocTypeMasterId > 1 && customer.LicenseExpiryDate != null)
                    {
                        c.LicenseExpiryDate = customer.LicenseExpiryDate;
                    }
                    #endregion
                    c.FSAAI = customer.FSAAI;
                    c.LandMark = customer.LandMark;
                    c.Mobile = customer.Mobile;
                    c.MobileSecond = customer.MobileSecond;
                    c.MonthlyTurnOver = customer.MonthlyTurnOver;
                    c.Name = customer.Name;

                    if (customer.Password != null)
                    {
                        c.Password = customer.Password;
                    }

                    c.RefNo = customer.RefNo;
                    c.SAGPSCoordinates = customer.SAGPSCoordinates;
                    c.ShopName = customer.ShopName;
                    c.ShippingAddress = customer.ShippingAddress;//sudhir
                    c.SizeOfShop = customer.SizeOfShop;//sudhir
                    c.UploadRegistration = customer.UploadRegistration;
                    //c.Warehouseid = wh.WarehouseId;
                    //c.WarehouseName = wh.WarehouseName;
                    c.AreaName = customer.AreaName;
                    c.IsSignup = string.IsNullOrEmpty(customer.ShopName) ? false : true;
                    #region to assign cluster ID and determine if it is in cluster or not.                   
                    if (customer.lat != c.lat || customer.lg != c.lg)
                    {
                        var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(customer.lat).Append("', '").Append(customer.lg).Append("')");
                        var clusterId = db.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                        if (!clusterId.HasValue)
                        {
                            c.InRegion = false;

                        }
                        else
                        {
                            c.ClusterId = clusterId;
                            var dd = db.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                            c.ClusterName = dd.ClusterName;
                            c.InRegion = true;
                            c.Warehouseid = dd.WarehouseId;
                            c.WarehouseName = dd.WarehouseName;
                        }
                    }
                    #endregion
                    if (customer.Shoplat > 0 && customer.Shoplg > 0)
                    {
                        c.Shoplat = customer.Shoplat;
                        c.Shoplg = customer.Shoplg;
                    }
                    c.City = city != null ? city.CityName : "";
                    c.ShippingCity = city != null ? city.CityName : "";
                    c.Cityid = city != null ? city.Cityid : 0;
                    c.State = city != null ? city.StateName : "";

                    c.Shopimage = customer.Shopimage;
                    c.AnniversaryDate = customer.AnniversaryDate;
                    c.DOB = customer.DOB;
                    c.WhatsappNumber = customer.WhatsappNumber;
                    c.LicenseNumber = customer.LicenseNumber;
                    c.UpdatedDate = indianTime;
                    c.CustomerVerify = "Pending For Activation";
                    c.AadharNo = customer.AadharNo;//sudhir
                    c.PanNo = customer.PanNo;//sudhir
                    c.CompanyId = wh.CompanyId;
                    c.UpdatedDate = indianTime;
                    c.ZipCode = customer.ZipCode;
                    c.UploadGSTPicture = customer.UploadGSTPicture;
                    c.BillingCity = customer.BillingCity;
                    c.BillingState = (c.City == c.BillingCity) == true ? c.State : customer.BillingState;
                    c.BillingZipCode = customer.BillingZipCode;

                    if (c != null && c.GrabbedBy == 0)
                    {
                        c.GrabbedBy = people.PeopleID;
                        string query = "Select top 1 citywise.Id,citywise.CommissionAmount from CityWiseActivationConfigurations citywise inner join Customers cust on citywise.CityId=cust.Cityid where cust.GrabbedBy>0 and citywise.IsActive=1 and citywise.IsDeleted=0 and cust.GrabbedBy=" + people.PeopleID;
                        var commission = await db.Database.SqlQuery<commissionDc>(query).FirstOrDefaultAsync();
                        if (commission != null)
                        {
                            AgentCommissionforCity agentCommissionforCity = new AgentCommissionforCity();
                            agentCommissionforCity.Amount = commission.CommissionAmount;
                            agentCommissionforCity.ConfigurationId = commission.Id;
                            agentCommissionforCity.PeopleId = people.PeopleID;
                            agentCommissionforCity.CustomerId = c.CustomerId;
                            agentCommissionforCity.IsActive = true;
                            agentCommissionforCity.IsDeleted = false;
                            agentCommissionforCity.CreatedDate = DateTime.Now;
                            agentCommissionforCity.CreatedBy = userid;
                            db.AgentCommissionforCityDB.Add(agentCommissionforCity);
                        }
                    }
                    else if (c.GrabbedBy > 0)
                    {
                        c.CustomerVerify = "Pending For Submitted";
                        c.UpdatedDate = indianTime;
                    }

                    if (!string.IsNullOrEmpty(c.RefNo))
                    {
                        var custGstVerifys = db.CustGSTverifiedRequestDB.Where(x => x.RefNo == customer.RefNo).ToList();
                        if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                        {
                            var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                            var state = db.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                            c.BillingCity = gstVerify.City;
                            c.BillingState = state != null ? state.StateName : gstVerify.State;
                            c.BillingZipCode = gstVerify.Zipcode;
                            c.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                            c.NameOnGST = gstVerify.Name;
                        }

                    }

                    if (string.IsNullOrEmpty(c.BillingAddress))
                    {
                        c.BillingCity = c.City;
                        c.BillingState = c.State;
                        c.BillingZipCode = c.ZipCode;
                        c.BillingAddress1 = c.AreaName;
                        c.BillingAddress = c.ShippingAddress;
                    }

                    db.Entry(c).State = EntityState.Modified;

                    //customer tracking
                    //if (TrackRequest && c.Warehouseid > 0)
                    //{
                    //var custVerifies = db.CustomerLatLngVerify.Where(x => x.CustomerId == customer.CustomerId && x.AppType == (int)AppEnum.SalesApp && x.Status == 1).ToList();
                    //if (custVerifies != null && custVerifies.Any())
                    //{
                    //    foreach (var item in custVerifies)
                    //    {
                    //        item.IsActive = false;
                    //        item.IsDeleted = true;
                    //        item.ModifiedBy = people.PeopleID;
                    //        item.ModifiedDate = DateTime.Now;
                    //        db.Entry(item).State = EntityState.Modified;
                    //    }
                    //}
                    //var customerLatLngVerify = new CustomerLatLngVerify
                    //{
                    //    CaptureImagePath = customer.Shopimage,
                    //    CreatedBy = people.PeopleID,
                    //    CreatedDate = DateTime.Now,
                    //    CustomerId = customer.CustomerId,
                    //    IsActive = true,
                    //    IsDeleted = false,
                    //    lat = c.lat,
                    //    lg = c.lg,
                    //    Newlat = customer.lat,
                    //    Newlg = customer.lg,
                    //    NewShippingAddress = customer.ShippingAddress,
                    //    ShippingAddress = c.ShippingAddress,
                    //    ShopFound = 1,
                    //    ShopName = customer.ShopName,
                    //    Skcode = c.Skcode,
                    //    LandMark = customer.LandMark,
                    //    Status = 1,
                    //    Nodocument = false,
                    //    Aerialdistance = 0,
                    //    AppType = (int)AppEnum.SalesApp

                    //};
                    //db.CustomerLatLngVerify.Add(customerLatLngVerify);

                    #region CustomerAddress :12/07/2022
                    if (customer.CustomerAddress != null)
                    {
                        customer.CustomerAddress.CustomerId = c.CustomerId;
                        CustomerAddressHelper customerAddressHelper = new CustomerAddressHelper();
                        bool IsInserted = customerAddressHelper.InsertCustomerAddress(customer.CustomerAddress, people.PeopleID);
                    }
                    #endregion
                    //}

                    if (db.Commit() > 0 && ((!string.IsNullOrEmpty(customer.LicenseNumber) && customer.CustomerDocTypeMasterId > 0) || (!string.IsNullOrEmpty(customer.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture))))
                    {
                        var customerdocs = db.CustomerDocs.Where(x => x.CustomerId == c.CustomerId).ToList();
                        var docMaster = db.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();
                        if (!string.IsNullOrEmpty(c.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                        {
                            var docid = docMaster.FirstOrDefault(x => x.DocType == "GST").Id;
                            if (customerdocs.Any(x => x.CustomerDocTypeMasterId == docid && x.IsActive))
                            {
                                var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == docid && x.IsActive);
                                custdoc.ModifiedBy = people.PeopleID;
                                custdoc.ModifiedDate = DateTime.Now;
                                custdoc.IsActive = false;
                                db.Entry(custdoc).State = EntityState.Modified;
                            }
                            db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = c.CustomerId,
                                IsActive = true,
                                CreatedBy = people.PeopleID,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = docid,
                                DocPath = customer.UploadGSTPicture,
                                IsDeleted = false
                            });
                        }

                        if (!string.IsNullOrEmpty(c.LicenseNumber) && customer.CustomerDocTypeMasterId > 0)
                        {
                            if (customerdocs.Any(x => x.CustomerDocTypeMasterId == customer.CustomerDocTypeMasterId && x.IsActive))
                            {
                                var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == customer.CustomerDocTypeMasterId && x.IsActive);
                                custdoc.ModifiedBy = people.PeopleID;
                                custdoc.ModifiedDate = DateTime.Now;
                                custdoc.IsActive = false;
                                db.Entry(custdoc).State = EntityState.Modified;
                            }
                            db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = c.CustomerId,
                                IsActive = true,
                                CreatedBy = people.PeopleID,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = customer.CustomerDocTypeMasterId,
                                DocPath = customer.UploadRegistration,
                                IsDeleted = false
                            });
                        }

                        db.Commit();
                    }



                    res = new customerDetail()
                    {
                        customers = c,
                        Status = true,
                        Message = "Update Details successfully."
                    };
                    return res;
                }
                catch (Exception ex)
                {

                    res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "Something Went Wrong Details."
                    };
                    return res;
                }
            }
        }

        [Route("CustomerUpdateByExecutive")]
        [HttpPut]
        public async Task<customerDetail> CustomerUpdateByExecutiveAsync(Customer customer)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            customerDetail res;
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    if (customer.LicenseExpiryDate != null && customer.LicenseExpiryDate < DateTime.Now)
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "License Expiry Date Already Expired, Please enter valid date"
                        };
                        return res;

                    }
                    Customer c = db.Customers.Where(s => s.Mobile.Trim().Equals(customer.Mobile.Trim())).FirstOrDefault();
                    if (c.CustomerVerify == "Full Verified" || c.CustomerVerify == "Partial Verified")
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer status is already in :" + c.CustomerVerify
                        };
                        return res;
                    };

                    if (c != null && (c.CustomerDocumentStatus == (int)CustomerDocumentStatusEnum.Verified || c.ShippingAddressStatus == (int)ShippingAddressStatusEnum.PhysicalVerified || c.ShippingAddressStatus == (int)ShippingAddressStatusEnum.VirtualVerified))
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "you can't update this customer profile, because customer document or address is verified"
                        };
                        return res;

                    }


                    City city = db.Cities.Where(x => x.Cityid == customer.Cityid && x.Deleted == false).FirstOrDefault();
                    People people = db.Peoples.Where(q => q.PeopleID == customer.ExecutiveId).FirstOrDefault();
                    Warehouse wh = db.Warehouses.Where(x => x.WarehouseId == people.WarehouseId && x.Deleted == false).FirstOrDefault();
                    if (wh == null)
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Primary Warehouse is not mapped to login user :" + people.DisplayName
                        };
                        return res;
                    }

                    if (!string.IsNullOrEmpty(customer.RefNo))
                    {
                        var checkgst = db.Customers.Where(x => x.RefNo == customer.RefNo && x.CustomerId != c.CustomerId).Count();
                        if (checkgst > 0)
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "Gst Already Exsits."
                            };
                            return res;
                        }
                    }

                    if (!string.IsNullOrEmpty(customer.LicenseNumber.Trim()) && customer.LicenseNumber != "0")
                    {
                        var checkgst = db.Customers.Where(x => x.LicenseNumber == customer.LicenseNumber && x.CustomerId != c.CustomerId).Count();

                        if (checkgst > 0)
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "License Number Already Exsits."
                            };
                            return res;
                        }
                    }


                    #region for Duplicate PAN no. check
                    if (!string.IsNullOrEmpty(customer.PanNo.Trim()))
                    {
                        Customer IsExistPAN = db.Customers.Where(x => x.PanNo.ToLower() == customer.PanNo.ToLower()).FirstOrDefault();
                        if (IsExistPAN != null )
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "PAN Already Exists."
                            };
                            return res;
                        }
                    }
                    #endregion

                    bool TrackRequest = false;
                    if (customer.lat != c.lat || customer.lg != c.lg || customer.RefNo != c.RefNo || customer.LicenseNumber != c.LicenseNumber || customer.ShippingAddress != c.ShippingAddress)
                        TrackRequest = true;

                    c.BAGPSCoordinates = customer.BAGPSCoordinates;
                    c.BillingAddress = customer.BillingAddress;

                    #region CustomerAddress status and lt lg :14/07/2022

                    if (customer.CustomerAddress != null)
                    {
                        c.ShippingAddress = null;
                        if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressLineOne))
                        {
                            c.ShippingAddress = customer.CustomerAddress.AddressLineOne + ",";
                        }
                        if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressLineTwo))
                        {
                            c.ShippingAddress += customer.CustomerAddress.AddressLineTwo + ",";
                        }
                        if (!string.IsNullOrEmpty(customer.CustomerAddress.AddressText))
                        {
                            c.ShippingAddress += customer.CustomerAddress.AddressText;
                        }
                        if (!string.IsNullOrEmpty(customer.CustomerAddress.ZipCode))
                        {
                            c.ShippingAddress += "," + customer.CustomerAddress.ZipCode;
                        }

                        c.lat = customer.CustomerAddress.AddressLat;
                        c.lg = customer.CustomerAddress.AddressLng;

                        c.Addresslat = customer.CustomerAddress.AddressLat;
                        c.Addresslg = customer.CustomerAddress.AddressLng;

                    }
                    //LicenseExpiryDate
                    if (customer.CustomerDocTypeMasterId > 0 && customer.CustomerDocTypeMasterId == 1 && customer.LicenseExpiryDate != null)
                    {
                        c.GstExpiryDate = customer.LicenseExpiryDate;
                    }
                    else if (customer.CustomerDocTypeMasterId > 1 && customer.LicenseExpiryDate != null)
                    {
                        c.LicenseExpiryDate = customer.LicenseExpiryDate;
                    }
                    #endregion
                    c.FSAAI = customer.FSAAI;
                    c.LandMark = customer.LandMark;
                    c.Mobile = customer.Mobile;
                    c.MobileSecond = customer.MobileSecond;
                    c.MonthlyTurnOver = customer.MonthlyTurnOver;
                    c.Name = customer.Name;

                    if (customer.Password != null)
                    {
                        c.Password = customer.Password;
                    }

                    c.RefNo = customer.RefNo;
                    c.SAGPSCoordinates = customer.SAGPSCoordinates;
                    c.ShopName = customer.ShopName;
                    c.ShippingAddress = customer.ShippingAddress;//sudhir
                    c.SizeOfShop = customer.SizeOfShop;//sudhir
                    c.UploadRegistration = customer.UploadRegistration;
                    //c.Warehouseid = wh.WarehouseId;
                    //c.WarehouseName = wh.WarehouseName;
                    c.AreaName = customer.AreaName;
                    c.IsSignup = string.IsNullOrEmpty(customer.ShopName) ? false : true;
                    #region to assign cluster ID and determine if it is in cluster or not.                   
                    if (customer.lat != c.lat || customer.lg != c.lg)
                    {
                        var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(customer.lat).Append("', '").Append(customer.lg).Append("')");
                        var clusterId = db.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                        if (!clusterId.HasValue)
                        {
                            c.InRegion = false;

                        }
                        else
                        {
                            c.ClusterId = clusterId;
                            var dd = db.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                            c.ClusterName = dd.ClusterName;
                            c.InRegion = true;
                            c.Warehouseid = dd.WarehouseId;
                            c.WarehouseName = dd.WarehouseName;
                        }
                    }
                    #endregion
                    if (customer.Shoplat > 0 && customer.Shoplg > 0)
                    {
                        c.Shoplat = customer.Shoplat;
                        c.Shoplg = customer.Shoplg;
                    }
                    c.City = city != null ? city.CityName : "";
                    c.ShippingCity = city != null ? city.CityName : "";
                    c.Cityid = city != null ? city.Cityid : 0;
                    c.State = city != null ? city.StateName : "";

                    c.Shopimage = customer.Shopimage;
                    c.AnniversaryDate = customer.AnniversaryDate;
                    c.DOB = customer.DOB;
                    c.WhatsappNumber = customer.WhatsappNumber;
                    c.LicenseNumber = customer.LicenseNumber;
                    c.UpdatedDate = indianTime;
                    //c.CustomerVerify = "Pending For Activation";//commented 
                    c.AadharNo = customer.AadharNo;//sudhir
                    c.PanNo = customer.PanNo;//sudhir
                    c.CompanyId = wh.CompanyId;
                    c.UpdatedDate = indianTime;
                    c.ZipCode = customer.ZipCode;
                    c.UploadGSTPicture = customer.UploadGSTPicture;
                    c.BillingCity = customer.BillingCity;
                    c.BillingState = (c.City == c.BillingCity) == true ? c.State : customer.BillingState;
                    c.BillingZipCode = customer.BillingZipCode;

                    if (c != null && c.GrabbedBy == 0)
                    {
                        c.GrabbedBy = people.PeopleID;
                        string query = "Select top 1 citywise.Id,citywise.CommissionAmount from CityWiseActivationConfigurations citywise inner join Customers cust on citywise.CityId=cust.Cityid where cust.GrabbedBy>0 and citywise.IsActive=1 and citywise.IsDeleted=0 and cust.GrabbedBy=" + people.PeopleID;
                        var commission = await db.Database.SqlQuery<commissionDc>(query).FirstOrDefaultAsync();
                        if (commission != null)
                        {
                            AgentCommissionforCity agentCommissionforCity = new AgentCommissionforCity();
                            agentCommissionforCity.Amount = commission.CommissionAmount;
                            agentCommissionforCity.ConfigurationId = commission.Id;
                            agentCommissionforCity.PeopleId = people.PeopleID;
                            agentCommissionforCity.CustomerId = c.CustomerId;
                            agentCommissionforCity.IsActive = true;
                            agentCommissionforCity.IsDeleted = false;
                            agentCommissionforCity.CreatedDate = DateTime.Now;
                            agentCommissionforCity.CreatedBy = userid;
                            db.AgentCommissionforCityDB.Add(agentCommissionforCity);
                        }
                    }
                    else if (c.GrabbedBy > 0)
                    {
                        c.CustomerVerify = "Pending For Submitted";
                        c.UpdatedDate = indianTime;
                    }

                    if (!string.IsNullOrEmpty(c.RefNo))
                    {
                        var custGstVerifys = db.CustGSTverifiedRequestDB.Where(x => x.RefNo == customer.RefNo).ToList();
                        if (custGstVerifys.Any() && custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault().Active == "Active")
                        {
                            var gstVerify = custGstVerifys.OrderByDescending(x => x.GSTVerifiedRequestId).FirstOrDefault();
                            var state = db.States.FirstOrDefault(x => x.AliasName.ToLower().Trim() == gstVerify.State.ToLower().Trim() || x.StateName.ToLower().Trim() == gstVerify.State.ToLower().Trim());
                            c.BillingCity = gstVerify.City;
                            c.BillingState = state != null ? state.StateName : gstVerify.State;
                            c.BillingZipCode = gstVerify.Zipcode;
                            c.BillingAddress = string.Format("{0}, {1}, {2}, {3}, {4}-{5}", gstVerify.HomeNo, gstVerify.HomeName, gstVerify.ShippingAddress, gstVerify.City, gstVerify.State, gstVerify.Zipcode);
                            c.NameOnGST = gstVerify.Name;
                        }

                    }

                    if (string.IsNullOrEmpty(c.BillingAddress))
                    {
                        c.BillingCity = c.City;
                        c.BillingState = c.State;
                        c.BillingZipCode = c.ZipCode;
                        c.BillingAddress1 = c.AreaName;
                        c.BillingAddress = c.ShippingAddress;
                    }

                    db.Entry(c).State = EntityState.Modified;

                    //customer tracking
                    if (TrackRequest)//&& c.Warehouseid > 0
                    {
                        var custVerifies = db.CustomerLatLngVerify.Where(x => x.CustomerId == customer.CustomerId && x.AppType == (int)AppEnum.SalesApp && x.Status == 1).ToList();
                        if (custVerifies != null && custVerifies.Any())
                        {
                            foreach (var item in custVerifies)
                            {
                                item.IsActive = false;
                                item.IsDeleted = true;
                                item.ModifiedBy = people.PeopleID;
                                item.ModifiedDate = DateTime.Now;
                                db.Entry(item).State = EntityState.Modified;
                            }
                        }
                        var customerLatLngVerify = new CustomerLatLngVerify
                        {
                            CaptureImagePath = customer.Shopimage,
                            CreatedBy = people.PeopleID,
                            CreatedDate = DateTime.Now,
                            CustomerId = customer.CustomerId,
                            IsActive = true,
                            IsDeleted = false,
                            lat = c.lat,
                            lg = c.lg,
                            Newlat = customer.lat,
                            Newlg = customer.lg,
                            NewShippingAddress = customer.ShippingAddress,
                            ShippingAddress = c.ShippingAddress,
                            ShopFound = 1,
                            ShopName = customer.ShopName,
                            Skcode = c.Skcode,
                            LandMark = customer.LandMark,
                            Status = 1,
                            Nodocument = false,
                            Aerialdistance = 0,
                            AppType = (int)AppEnum.SalesApp

                        };
                        db.CustomerLatLngVerify.Add(customerLatLngVerify);
                    }
                    #region CustomerAddress :12/07/2022
                    if (customer.CustomerAddress != null)
                    {
                        customer.CustomerAddress.CustomerId = c.CustomerId;
                        CustomerAddressHelper customerAddressHelper = new CustomerAddressHelper();
                        bool IsInserted = customerAddressHelper.InsertCustomerAddress(customer.CustomerAddress, people.PeopleID);
                    }
                    #endregion
                    //}



                    #region Channel

                    var CustomerChannelexists = db.CustomerChannelMappings.Where(x => x.CustomerId == c.CustomerId && x.IsDeleted == false).FirstOrDefault();
                    if (CustomerChannelexists == null)
                    {
                        List<CustomerChannelMapping> customerChannels = new List<CustomerChannelMapping>();
                        var StoreIds = db.StoreDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).Distinct().ToList();
                        if (StoreIds != null && StoreIds.Count > 0 && customer.ChannelId > 0)
                        {
                            StoreIds.ForEach(x =>
                            {
                                customerChannels.Add(new CustomerChannelMapping
                                {
                                    CustomerId = c.CustomerId,
                                    StoreId = (int)x,
                                    ChannelMasterId = customer.ChannelId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = customer.ExecutiveId ?? 0,
                                    ModifiedBy = null,
                                    ModifiedDate = null
                                });
                            });
                        }
                        if (customerChannels != null && customerChannels.Count > 0)
                            db.CustomerChannelMappings.AddRange(customerChannels);
                    }

                    #endregion


                    if (db.Commit() > 0 && ((!string.IsNullOrEmpty(customer.LicenseNumber) && customer.CustomerDocTypeMasterId > 0) || (!string.IsNullOrEmpty(customer.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture))))
                    {
                        var customerdocs = db.CustomerDocs.Where(x => x.CustomerId == c.CustomerId).ToList();
                        var docMaster = db.CustomerDocTypeMasters.Where(x => x.IsActive).ToList();
                        if (!string.IsNullOrEmpty(c.RefNo) && !string.IsNullOrEmpty(customer.UploadGSTPicture) && docMaster.Any(x => x.DocType == "GST"))
                        {
                            var docid = docMaster.FirstOrDefault(x => x.DocType == "GST").Id;
                            if (customerdocs.Any(x => x.CustomerDocTypeMasterId == docid && x.IsActive))
                            {
                                var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == docid && x.IsActive);
                                custdoc.ModifiedBy = people.PeopleID;
                                custdoc.ModifiedDate = DateTime.Now;
                                custdoc.IsActive = false;
                                db.Entry(custdoc).State = EntityState.Modified;
                            }
                            db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = c.CustomerId,
                                IsActive = true,
                                CreatedBy = people.PeopleID,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = docid,
                                DocPath = customer.UploadGSTPicture,
                                IsDeleted = false
                            });
                        }

                        if (!string.IsNullOrEmpty(c.LicenseNumber) && customer.CustomerDocTypeMasterId > 0)
                        {
                            if (customerdocs.Any(x => x.CustomerDocTypeMasterId == customer.CustomerDocTypeMasterId && x.IsActive))
                            {
                                var custdoc = customerdocs.FirstOrDefault(x => x.CustomerDocTypeMasterId == customer.CustomerDocTypeMasterId && x.IsActive);
                                custdoc.ModifiedBy = people.PeopleID;
                                custdoc.ModifiedDate = DateTime.Now;
                                custdoc.IsActive = false;
                                db.Entry(custdoc).State = EntityState.Modified;
                            }
                            db.CustomerDocs.Add(new Model.CustomerDelight.CustomerDoc
                            {
                                CustomerId = c.CustomerId,
                                IsActive = true,
                                CreatedBy = people.PeopleID,
                                CreatedDate = DateTime.Now,
                                CustomerDocTypeMasterId = customer.CustomerDocTypeMasterId,
                                DocPath = customer.UploadRegistration,
                                IsDeleted = false
                            });
                        }

                        db.Commit();
                    }



                    res = new customerDetail()
                    {
                        customers = c,
                        Status = true,
                        Message = "Update Details successfully."
                    };
                    return res;
                }
                catch (Exception ex)
                {

                    res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "Something Went Wrong Details."
                    };
                    return res;
                }
            }
        }

        #endregion
        #region RA: Forgot Password
        /// <summary>
        /// Created by 30/01/2019
        /// </summary>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        [Route("Forgrt/v2")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetForgrtV2(string Mobile)
        {
            logger.Info("start City: ");
            customerDetail cd;
            Customer customer = new Customer();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    using (AuthContext context = new AuthContext())
                    {
                        int CompanyId = compid;
                        customer = context.GetCustomerbyId(Mobile);
                        if (customer != null)
                        {
                            new Sms().sendOtp(customer.Mobile, "Hi " + customer.ShopName + " \n\t You Recently requested a forget password on ShopKirana. Your account Password is '" + customer.Password + "'\n If you didn't request then ingore this message\n\t\t Thanks\n\t\t Shopkirana.com", "");

                            cd = new customerDetail()
                            {
                                customers = customer,
                                Status = true,
                                Message = "Message send to your registered mobile number."
                            };

                            return Request.CreateResponse(HttpStatusCode.OK, cd);
                        }
                        else
                        {
                            cd = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "Customer not exist."
                            };
                            return Request.CreateResponse(HttpStatusCode.BadRequest, cd);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    cd = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = ex.Message
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, cd);
                }
            }
        }
        #endregion

        #region SPA: Update Customer Info from sales person app

        /// <summary>
        /// Update Customer Info from sales person app
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        [Route("updateInfo")]
        [AcceptVerbs("put")]
        [HttpPut]
        public HttpResponseMessage UpdateCustinfo(Customer cust)
        {
            customerDetail res;
            logger.Info("start update Customer city: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var FirstCityWh = db.Warehouses.Where(a => a.Cityid == cust.Cityid && a.active == true).FirstOrDefault();
                    Customer customers = db.Customers.Where(c => c.CustomerId == cust.CustomerId).FirstOrDefault();
                    City City = db.Cities.Where(a => a.Cityid == cust.Cityid).FirstOrDefault();
                    if (customers != null)
                    {
                        Customer c = db.Customers.Where(x => x.CustomerId == customers.CustomerId).FirstOrDefault();
                        c.Name = cust.Name;
                        c.Emailid = cust.Emailid;
                        c.UploadProfilePichure = cust.UploadProfilePichure;
                        c.UpdatedDate = DateTime.Now;
                        db.Entry(c).State = EntityState.Modified;
                        db.Commit();

                        res = new customerDetail()
                        {
                            customers = c,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer not exist or Incorect mobile number."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region Update Customer City

        /// <summary>
        /// Update Customer City
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        [Route("updateCustcity")]
        [AcceptVerbs("put")]
        [HttpPut]
        public HttpResponseMessage UpdateCustcity(Customer cust)
        {
            customerDetail res;
            logger.Info("start update Customer city: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var FirstCityWh = db.Warehouses.Where(a => a.Cityid == cust.Cityid && a.active == true).FirstOrDefault();
                    Customer customers = db.Customers.Where(c => c.Mobile.Trim().Equals(cust.Mobile.Trim())).FirstOrDefault();
                    City City = db.Cities.Where(a => a.Cityid == cust.Cityid).FirstOrDefault();
                    if (customers != null)
                    {
                        //CustWarehouse cs = db.CustWarehouseDB.Where(x => x.CustomerId == cust.CustomerId).FirstOrDefault();
                        Customer cs = db.Customers.Where(x => x.CustomerId == cust.CustomerId).FirstOrDefault();
                        cs.Warehouseid = FirstCityWh.WarehouseId;
                        cs.WarehouseName = FirstCityWh.WarehouseName;
                        cs.CompanyId = cust.CompanyId;
                        cs.UpdatedDate = indianTime;
                        db.Commit();

                        Customer c = db.Customers.Where(x => x.CustomerId == cs.CustomerId).FirstOrDefault();
                        c.Warehouseid = FirstCityWh.WarehouseId;
                        c.WarehouseName = FirstCityWh.WarehouseName;
                        c.Cityid = City.Cityid;
                        c.City = City.CityName;
                        c.IsCityVerified = true;
                        c.IsSignup = true;
                        c.UpdatedDate = indianTime;
                        db.Commit();

                        res = new customerDetail()
                        {
                            customers = c,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer not exist or Incorect mobile number."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region Generate Random OTP
        /// <summary>
        /// Created by 18/12/2018 
        /// Create rendom otp
        /// </summary>
        /// <param name="iOTPLength"></param>
        /// <param name="saAllowedCharacters"></param>
        /// <returns></returns>
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
        #endregion

        #region Generate Customer OTP
        /// <summary>
        /// Created by 18/12/2018 
        /// OTP Genration code 
        /// </summary>
        /// <returns></returns>
        [Route("Genotp")]
        [AllowAnonymous]
        public OTP Getotp(string MobileNumber, string deviceId)
        {
            logger.Info("start Gen OTP: ");

            try
            {

                using (var context = new AuthContext())
                {
                    OTP b = new OTP();

                    var checkDeviceId = context.Peoples.Where(x => x.DeviceId == deviceId).Count();
                    if (checkDeviceId > 0)
                    {
                        b = new OTP()
                        {
                            OtpNo = "You are not authorize"
                        };
                        return b;
                    }
                    Customer cust = context.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim()) && c.IsKPP).FirstOrDefault();
                    if (cust != null && cust.IsKPP)
                    {
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
                // string OtpMessage = " : is Your Verification Code. :).ShopKirana";
                string OtpMessage = ""; //"{#var1#} : is Your Verification Code. {#var2#}.ShopKirana";
                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Customer_Verification_Code");
                OtpMessage = dltSMS == null ? "" : dltSMS.Template;
                OtpMessage = OtpMessage.Replace("{#var1#}", sRandomOTP);
                OtpMessage = OtpMessage.Replace("{#var2#}", ":)");

                //string message = sRandomOTP + " :" + OtpMessage;
                if (dltSMS != null)
                    Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                OTP a = new OTP()
                {
                    OtpNo = sRandomOTP
                };
                return a;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OTP Genration.");
                return null;
            }
        }
        #endregion

        #region Customer Mapping from city and warehouse
        /// <summary>
        /// Created by 18/12/2018 
        /// Mapped customer to city warehouse
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("CustMappedCity")]
        [HttpPost]
        public Customer CustMappedCity(CustomerMappedCity obj)
        {
            logger.Info("start Mapped Customer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var FirstCityWh = db.Warehouses.Where(a => a.Cityid == obj.CityId && a.active == true).FirstOrDefault();
                    Customer cust = db.Customers.Where(x => x.CustomerId == obj.CustomerId).FirstOrDefault();
                    if (FirstCityWh != null)
                    {
                        //CustWarehouse cust = db.CustWarehouseDB.Where(x => x.CustomerId == obj.CustomerId && x.WarehouseId == FirstCityWh.WarehouseId).FirstOrDefault();

                        if (cust != null && cust.Warehouseid != FirstCityWh.WarehouseId)
                        {
                            //Customer cd = db.Customers.Where(x => x.CustomerId == obj.CustomerId).FirstOrDefault();

                            cust.Warehouseid = FirstCityWh.WarehouseId;
                            cust.WarehouseName = FirstCityWh.WarehouseName;
                            cust.CompanyId = FirstCityWh.CompanyId;
                            cust.Cityid = FirstCityWh.Cityid;
                            cust.City = FirstCityWh.CityName;
                            //cust.CreatedDate = indianTime;
                            //cust.UpdatedDate = indianTime;
                            cust.IsCityVerified = true;
                            cust.fcmId = obj.FcmId;
                            db.Entry(cust).State = EntityState.Modified;
                            db.Commit();
                        }
                        return cust;
                    }
                    else
                    {
                        return cust;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return null;
                }
            }
        }
        #endregion

        #region customer Mobile Insertion.
        /// <summary>
        /// Created by 19/12/2018 
        /// Customer insert
        /// </summary>   // tejas to save device info 
        /// <param name="MobileNumber"></param>
        /// <returns></returns>
        [ResponseType(typeof(Customer))]
        [Route("insert")]
        [AcceptVerbs("POST")]
        [AllowAnonymous]
        public HttpResponseMessage addCustomer(string MobileNumber)
        {
            logger.Info("start add Customer: ");
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

                        //try
                        //{
                        //    var wallet = db.GetWalletbyCustomerid(c.CustomerId);
                        //    var reward = db.GetRewardbyCustomerid(c.CustomerId);
                        //}
                        //catch (Exception ex)
                        //{
                        //}
                        logger.Info("User ID : {0} , Company Id : {1}");
                        logger.Info("End  addCustomer: ");

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
                        return Request.CreateResponse(HttpStatusCode.OK, cs);
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
                        return Request.CreateResponse(HttpStatusCode.OK, cs);
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                    logger.Error("Error in addCustomer " + error);
                    responsemsg msg = new responsemsg()
                    {
                        IsCityVerified = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, msg);
                }
            }
        }
        #endregion

        #region Get Customer detail after otp verified.
        /// <summary>
        /// Get Exist customer detail on login time
        /// </summary>   // tejas to save device info 
        /// <param name="MobileNumber"></param>
        /// <param name="IsOTPverified"></param>
        /// <returns></returns>
        [Route("GetLogedCust")]
        [AcceptVerbs("GET")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLogedCustomer(string MobileNumber, bool IsOTPverified, string fcmid, string CurrentAPKversion, string PhoneOSversion, string UserDeviceName, string IMEI = "")
        {
            customerDetail res;
            logger.Info("start add Customer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    if (IsOTPverified == true)
                    {
                        Customer customersMobileExist = db.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim())).FirstOrDefault();
                        if (customersMobileExist != null)
                        {
                            // Customer customers = db.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim()) && c.IsCityVerified == true).FirstOrDefault();
                            //if (!db.DistributorVerificationDB.Any(x => x.CustomerID == customersMobileExist.CustomerId))
                            //{

                            if (customersMobileExist.IsCityVerified)
                            {
                                if (fcmid != null && fcmid.Trim() != "" && fcmid.Trim().ToUpper() != "NULL")
                                {
                                    customersMobileExist.fcmId = fcmid;
                                    customersMobileExist.CurrentAPKversion = CurrentAPKversion;           // tejas to save device info 
                                    customersMobileExist.PhoneOSversion = PhoneOSversion;
                                    customersMobileExist.UserDeviceName = UserDeviceName;
                                    customersMobileExist.IsResetPasswordOnLogin = false; // added on 22/06/2019
                                    customersMobileExist.imei = !string.IsNullOrEmpty(IMEI) ? IMEI : customersMobileExist.imei;// sudhir to save device info 
                                                                                                                               // db.Customers.Attach(customers);
                                    db.Entry(customersMobileExist).State = EntityState.Modified;
                                    db.Commit();
                                }
                                var registeredApk = db.GetAPKUserAndPwd("RetailersApp");
                                customersMobileExist.RegisteredApk = registeredApk;
                                res = new customerDetail()
                                {
                                    customers = customersMobileExist,
                                    Status = true,
                                    Message = "Success."
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else
                            {
                                res = new customerDetail()
                                {
                                    customers = null,
                                    Status = false,
                                    Message = "Customer city not exist or not verified."
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            //}
                            //else
                            //{
                            //    res = new customerDetail()
                            //    {
                            //        customers = null,
                            //        Status = false,
                            //        Message = "You are not eligible to access this app. Please contact customer care."
                            //    };
                            //    return Request.CreateResponse(HttpStatusCode.OK, res);
                            //}
                        }
                        else
                        {
                            res = new customerDetail()
                            {
                                customers = null,
                                Status = false,
                                Message = "Customer not exist or Incorect mobile number."
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "OTP not verified."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    return null;
                }
            }
        }
        #endregion

        #region Customer Signup.
        /// <summary>
        /// Customer SignUp
        /// </summary>
        /// <param name="MobileNumber"></param>
        /// <returns></returns>
        [ResponseType(typeof(Customer))]
        [Route("signupCustomer")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SignupCustomer(Customer obj)
        {
            logger.Info("start add Customer if already add update customer: ");
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

                    Customer customers = db.Customers.Where(c => c.Mobile.Trim().Equals(obj.Mobile.Trim())).FirstOrDefault();
                    if (customers != null)
                    {
                        //removed on 13/07/2022 by Harry
                        //if (customers.ShippingAddress != obj.ShippingAddress)
                        //{
                        //    GeoHelper geoHelper = new GeoHelper();
                        //    decimal? lat, longitude;
                        //    geoHelper.GetLatLongWithZipCode(obj.ShippingAddress, obj.City, obj.ZipCode, out lat, out longitude);
                        //    customers.Addresslat = (double)lat;
                        //    customers.Addresslg = (double)longitude;
                        //    customers.Distance = 0;
                        //}
                        //if (customers.Addresslat.HasValue && customers.Addresslg.HasValue && obj.lat > 0 && obj.lg > 0)
                        //{
                        //    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(customers.Addresslat.Value, customers.Addresslg.Value);
                        //    var destination = new System.Device.Location.GeoCoordinate(obj.lat, obj.lg);
                        //    customers.Distance = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                        //}

                        //CustWarehouse cw = db.CustWarehouseDB.Where(x => x.CustomerId == customers.CustomerId).FirstOrDefault();
                        //Customer cw = db.Customers.Where(x => x.CustomerId == customers.CustomerId).FirstOrDefault();
                        City City = db.Cities.Where(a => a.CityName.Trim().ToLower() == obj.City.Trim().ToLower()).FirstOrDefault();
                        customers.Name = obj.Name;
                        customers.ShopName = obj.ShopName;
                        customers.ShippingAddress = obj.ShippingAddress;
                        customers.Cityid = City?.Cityid;
                        customers.City = obj.City;
                        customers.fcmId = obj.fcmId;
                        customers.Password = obj.Password;
                        customers.imei = obj.imei;//Sudhir to save device info 
                        customers.PhoneOSversion = obj.PhoneOSversion;
                        customers.deviceId = obj.deviceId;
                        customers.UserDeviceName = obj.UserDeviceName;
                        customers.IsSignup = true;
                        customers.UpdatedDate = indianTime;
                        //customers.lat = obj.lat;
                        //customers.lg = obj.lg;
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

                        db.Commit();
                        #region Device History
                        //tejas 29-05-2019
                        var Customerhistory = db.Customers.Where(x => x.Mobile == obj.Mobile).FirstOrDefault();
                        try
                        {
                            PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                            if (Customerhistory != null)
                            {
                                phonerecord.CustomerId = Customerhistory.CustomerId;
                                phonerecord.Name = Customerhistory.Name;
                                phonerecord.Skcode = Customerhistory.Skcode;
                                phonerecord.Mobile = Customerhistory.Mobile;
                                phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                                phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                                phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
                                phonerecord.IMEI = Customerhistory.imei;//Sudhir to save device info
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
                        res = new customerDetail()
                        {
                            customers = customers,
                            Status = true,
                            Message = "Customer Detail Updated."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        //var FirstCityWh = db.Warehouses.Where(a => a.Cityid == obj.Cityid && a.active == true).FirstOrDefault();
                        var FirstCityWh = db.Warehouses.Where(a => a.Cityid == obj.Cityid && a.active == true).First();
                        var Skcode = this.skcode();
                        Customer cust = new Customer();
                        if (FirstCityWh != null)
                        {
                            //removed on 13/07/2022 by Harry
                            //if (!string.IsNullOrEmpty(obj.ShippingAddress))
                            //{
                            //    GeoHelper geoHelper = new GeoHelper();
                            //    decimal? lat, longitude;
                            //    geoHelper.GetLatLongWithZipCode(obj.ShippingAddress, FirstCityWh.CityName, obj.ZipCode, out lat, out longitude);
                            //    cust.Addresslat = (double)lat;
                            //    cust.Addresslg = (double)longitude;
                            //    cust.Distance = 0;
                            //}

                            //if (customers.Addresslat.HasValue && customers.Addresslg.HasValue && obj.lat > 0 && obj.lg > 0)
                            //{
                            //    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(cust.Addresslat.Value, cust.Addresslg.Value);
                            //    var destination = new System.Device.Location.GeoCoordinate(obj.lat, obj.lg);
                            //    cust.Distance = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                            //}

                            cust.Mobile = obj.Mobile;
                            cust.Name = obj.Name;
                            cust.Skcode = Skcode;
                            cust.ShopName = obj.ShopName;
                            cust.ShippingAddress = obj.ShippingAddress;
                            cust.RefNo = obj.RefNo;
                            cust.Password = obj.Password;
                            cust.Cityid = FirstCityWh.Cityid;
                            cust.City = FirstCityWh.CityName;
                            cust.fcmId = obj.fcmId;
                            cust.Warehouseid = FirstCityWh.WarehouseId;
                            cust.WarehouseName = FirstCityWh.WarehouseName;
                            cust.CurrentAPKversion = obj.CurrentAPKversion;                 // tejas to save device info
                            cust.imei = obj.imei;// Sudhir to save device info
                            cust.PhoneOSversion = obj.PhoneOSversion;
                            cust.UserDeviceName = obj.UserDeviceName;
                            cust.IsCityVerified = true;
                            cust.IsSignup = true;
                            cust.CreatedDate = indianTime;
                            cust.UpdatedDate = indianTime;
                            db.Customers.Add(cust);
                            int cid = db.Commit();
                            #region Device History
                            //tejas 29-05-2019
                            var Customerhistory = db.Customers.Where(x => x.Mobile == obj.Mobile).FirstOrDefault();
                            try
                            {
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
                                    int id = db.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error loading  \n\n" + ex.Message + "\n\n" + ex.InnerException + "\n\n" + ex.StackTrace);
                            }
                            #endregion


                        }
                        cust.Warehouseid = FirstCityWh.WarehouseId;
                        res = new customerDetail()
                        {
                            customers = cust,
                            Status = true,
                            Message = "Customer Saved."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in add Customer " + ex.Message);
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
        #endregion

        #region Customer Password Change
        /// <summary>
        /// customer password change 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(Customer))]
        [Route("changepassword")]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage Changepassword(pwcdetail item)
        {
            logger.Info("start putCustomer: ");
            using (AuthContext db = new AuthContext())
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
                try
                {
                    customerDetail res;
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
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    res = new customerDetail()
                    {
                        customers = cust,
                        Status = true,
                        Message = "Password is changed."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in put Customer " + ex.Message);
                    customerDetail res;
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "Somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region IsMobileVerfied for Forgot password
        /// <summary>
        /// IsMobileVerfied for Forgot password
        /// </summary>
        /// <param name="MobileNumber"></param>
        /// <returns></returns>

        [Route("IsMobileVerfied")]
        [AcceptVerbs("GET")]
        [HttpGet]
        public HttpResponseMessage IsMobileVerfied(string MobileNumber)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    resmsg rm;
                    Customer customers = db.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim())).FirstOrDefault();
                    if (customers != null)
                    {
                        rm = new resmsg
                        {
                            Message = true,
                        };
                    }
                    else
                    {
                        rm = new resmsg
                        {
                            Message = false,
                        };
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, rm);
                }
                catch
                {
                    return Request.CreateResponse(HttpStatusCode.OK, false);
                }
            }
        }
        #endregion

        #region Forgot Password 
        /// <summary>
        /// Forgot Password 
        /// </summary>
        /// <param name="fp"></param>
        /// <returns></returns>
        [Route("forgotpassword")]
        [AcceptVerbs("PUT")]
        [HttpGet]
        public HttpResponseMessage forgotpw(forgotpassword fp)
        {
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    resmsg rm;
                    Customer customers = db.Customers.Where(c => c.Mobile.Trim().Equals(fp.MobileNumber.Trim())).FirstOrDefault();
                    if (customers != null)
                    {
                        customers.Password = fp.NewPassword;
                        //db.Customers.Attach(customers);
                        db.Entry(customers).State = EntityState.Modified;
                        db.Commit();
                        rm = new resmsg()
                        {
                            Message = true
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, rm);
                    }
                    else
                    {
                        rm = new resmsg()
                        {
                            Message = false
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, rm);
                    }
                }
                catch
                {
                    return Request.CreateResponse(HttpStatusCode.OK, false);
                }
            }
        }
        #endregion

        #region Get customer profile
        /// <summary>
        /// Created by 26/12/2018
        /// Get customer profile 
        /// </summary>
        /// <param name="customerid"></param>
        /// <returns></returns>
        [Route("myprofile")]
        [HttpGet]
        public HttpResponseMessage Getmyprofile(int customerid, string deviceId)
        {
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
            using (var dbContext = new AuthContext())
            {
                try
                {
                    customerDetail res;



                    var checkDeviceId = dbContext.Peoples.Where(x => x.DeviceId == deviceId).Count();
                    if (checkDeviceId > 0)
                    {

                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "You are not Authorized."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }




                    Customer customers = dbContext.Customers.Where(a => a.CustomerId == customerid).SingleOrDefault();
                    if (customers == null)
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            var changePassword = false;
                            var deviceLogins = dbContext.CustomerLoginDeviceDB.Where(x => x.CustomerId == customers.CustomerId).ToList();

                            if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == deviceId && x.IsCurrentLogin))
                            {
                                var login = deviceLogins.FirstOrDefault(x => x.DeviceId == deviceId);
                                login.LastLogin = now;
                                dbContext.Entry(login).State = EntityState.Modified;
                                dbContext.Commit();
                            }
                            else
                            {
                                if (deviceLogins == null || !deviceLogins.Any())
                                {
                                    var login = new CustomerLoginDevice()
                                    {
                                        DeviceId = deviceId,
                                        CustomerId = customers.CustomerId,
                                        CreatedDate = now,
                                        IsCurrentLogin = true,
                                        LastLogin = now,
                                        Mobile = customers.Mobile
                                    };
                                    dbContext.Entry(login).State = EntityState.Added;

                                }
                                else if (deviceLogins != null && !deviceLogins.Any(x => x.DeviceId == deviceId))
                                {
                                    var login = new CustomerLoginDevice()
                                    {
                                        DeviceId = deviceId,
                                        CustomerId = customers.CustomerId,
                                        CreatedDate = now,
                                        IsCurrentLogin = true,
                                        LastLogin = now,
                                        Mobile = customers.Mobile
                                    };
                                    dbContext.Entry(login).State = EntityState.Added;

                                    deviceLogins.Where(x => x.IsCurrentLogin).ToList().ForEach(x =>
                                    {
                                        x.IsCurrentLogin = false;
                                        dbContext.Entry(x).State = EntityState.Modified;
                                    });

                                    changePassword = true;
                                }
                                else if (deviceLogins != null && deviceLogins.Any(x => x.DeviceId == deviceId && !x.IsCurrentLogin))
                                {
                                    var login = deviceLogins.FirstOrDefault(x => x.DeviceId == deviceId && !x.IsCurrentLogin);
                                    login.LastLogin = now;
                                    login.IsCurrentLogin = true;
                                    dbContext.Entry(login).State = EntityState.Modified;

                                    deviceLogins.Where(x => x.DeviceId != deviceId && x.IsCurrentLogin).ToList().ForEach(x =>
                                    {
                                        x.IsCurrentLogin = false;
                                        dbContext.Entry(x).State = EntityState.Modified;
                                    });

                                    changePassword = true;
                                }

                                dbContext.Commit();


                                if (changePassword)
                                {
                                    var randomPassword = PasswordGenerator.GetRandomPassword(4, "0123456789");
                                    customers.Password = randomPassword;
                                    customers.IsResetPasswordOnLogin = true;
                                    customers.UpdatedDate = indianTime;
                                    dbContext.Entry(customers).State = EntityState.Modified;
                                    dbContext.Commit();


                                    Sms smsHelper = new Sms();
                                    // string OtpMessage = "Your account has logged in on another device. Please reset your password using below OTP:\n" + randomPassword + " Shopkirana";
                                    string OtpMessage = "";//"Your account has logged in on another device. Please reset your password using below OTP: {#var#} Shopkirana";
                                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Customer_Reset_Password");
                                    OtpMessage = dltSMS == null ? "" : dltSMS.Template;
                                    if (dltSMS != null)
                                    {
                                        OtpMessage = OtpMessage.Replace("{#var#}", randomPassword);
                                        smsHelper.sendOtp(customers.Mobile, OtpMessage, dltSMS.DLTId);
                                    }

                                }
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


                    res = new customerDetail()
                    {
                        customers = customers,
                        Status = true,
                        Message = "Success.",
                        CriticalInfoMissingMsg = result

                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);


                }

                catch (Exception ex)
                {
                    logger.Error("Error in getting customer by mobile" + ex.Message);
                    return null;
                }
            }

        }
        #endregion

        #region Get Customer Notification.
        /// <summary>
        /// Created by 18/01/2019
        /// Get Customer Notification.
        /// </summary>
        /// <param name="customerid"></param>
        /// <returns></returns>
        [Route("getcustnot")]
        [AcceptVerbs("GET")]
        [HttpGet]
        public HttpResponseMessage getcustnot(int list, int page, int customerid)
        {
            CustNotiDetail res;
            logger.Info("start add Customer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    PaggingDatas data = new PaggingDatas();
                    var total_count = db.DeviceNotificationDb.Where(x => x.CustomerId == customerid).Count();
                    var notificationmaster = db.DeviceNotificationDb.Where(x => x.CustomerId == customerid).OrderByDescending(x => x.NotificationTime).Skip((page - 1) * list).Take(list).ToList();
                    data.notificationmaster = notificationmaster;
                    data.total_count = total_count;
                    res = new CustNotiDetail()
                    {
                        PaggingDatas = data,
                        Status = true,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    CustNotiDetail rest;
                    rest = new CustNotiDetail()
                    {
                        PaggingDatas = null,
                        Status = false,
                        Message = "Something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, rest);
                }
            }
        }
        #endregion

        #region SKCode genrate Function.
        /// <summary>
        /// Created by 19/12/2018 
        /// Get New Skcode function
        /// </summary>
        /// <returns></returns>
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
        #endregion

        /// <summary>
        /// creaetd by 02/02/2019
        /// Update Shop Image 
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        [Route("UpdateShopimage/V2")]
        [AcceptVerbs("put")]
        [HttpPut]
        public HttpResponseMessage UpdateShopimage(ShopimgUpdate cust)
        {
            customerDetail res;
            logger.Info("start update Customer city: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    Customer customers = db.Customers.Where(c => c.CustomerId == cust.CustomerId).FirstOrDefault();
                    if (customers != null)
                    {
                        customers.Shopimage = cust.Shopimage;
                        customers.UpdatedDate = DateTime.Now;
                        db.Entry(customers).State = EntityState.Modified;
                        db.Commit();

                        res = new customerDetail()
                        {
                            customers = customers,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new customerDetail()
                        {
                            customers = null,
                            Status = false,
                            Message = "Customer not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new customerDetail()
                    {
                        customers = null,
                        Status = false,
                        Message = "somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        ///// <summary>
        ///// Created Harry by 27/02/2019
        ///// Global customer Serach(Skcode/ShopName/Mobile)  
        ///// </summary>
        ///// <param name="WarehouseId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("GlobalSearch")]
        //public HttpResponseMessage globalsearch(int WarehouseId, string Globalkey)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {
        //            GlobalcustomerDetail obj = new GlobalcustomerDetail();
        //            //var wareHouseName = db.Warehouses.FirstOrDefault(x => x.WarehouseId == WarehouseId).WarehouseName;
        //            //var customer = db.Customers.Where(c => (c.Skcode.Contains(Globalkey) || c.ShopName.Contains(Globalkey) || c.Mobile.Contains(Globalkey)) && c.Deleted == false).ToList();
        //            // var customer = db.Customers.Where(i => i.IsHide == false && i.Warehouseid == WarehouseId && i.Deleted == false && (i.Skcode.Contains(Globalkey) || i.ShopName.Contains(Globalkey) || i.Mobile.Contains(Globalkey))).
        //            //Select(i => new SalespDTO
        //            //{
        //            //    CustomerId = i.CustomerId,
        //            //    CompanyId = i.CompanyId,
        //            //    Active = i.Active,
        //            //    City = i.City,
        //            //    WarehouseId = i.Warehouseid,
        //            //    WarehouseName = i.WarehouseName,//wareHouseName,
        //            //    lat = i.lat,
        //            //    lg = i.lg,
        //            //    ExecutiveId = i.ExecutiveId,
        //            //    BeatNumber = i.BeatNumber,
        //            //    Day = i.Day,
        //            //    Skcode = i.Skcode,
        //            //    Mobile = i.Mobile,
        //            //    ShopName = i.ShopName,
        //            //    BillingAddress = i.BillingAddress,
        //            //    Name = i.Name,
        //            //    Emailid = i.Emailid,
        //            //    RefNo = i.RefNo,
        //            //    Password = i.Password,
        //            //    UploadRegistration = i.UploadRegistration,
        //            //    ResidenceAddressProof = i.ResidenceAddressProof,
        //            //    DOB = i.DOB
        //            //}).OrderBy(x => x.CustomerId).ToList();


        //            var customer = new List<SalespDTO>();
        //            if (!string.IsNullOrEmpty(Globalkey) && Globalkey.Length > 5)
        //            {
        //                var query = "select CustomerId,CompanyId,CreatedDate,Active,City,WarehouseId,lat,lg,ExecutiveId,BeatNumber,Day,Skcode,Mobile, ShopName,BillingAddress,Name,RefNo,Password, Emailid,UploadRegistration,ResidenceAddressProof,DOB from Customers with(nolock) Where  WarehouseId =" + WarehouseId + " and  Deleted = 0 and Active = 1 and IsHide = 0 and Skcode like '%" + Globalkey + "%' or Mobile like '%" + Globalkey + "%' or ShopName like '%" + Globalkey + "%' ";
        //                customer = db.Database.SqlQuery<SalespDTO>(query).ToList();
        //            }
        //            if (customer.Count() > 0)
        //            {
        //                obj = new GlobalcustomerDetail()
        //                {
        //                    customers = customer,
        //                    Status = true,
        //                    Message = "Customer Found"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, obj);
        //            }
        //            else
        //            {
        //                obj = new GlobalcustomerDetail()
        //                {
        //                    customers = customer,
        //                    Status = false,
        //                    Message = "No Customer found"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, obj);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        //        }
        //    }
        //}


        //#region Global Search for sales app
        ///// <summary>
        ///// Created Raj by 25/02/2020
        ///// Global customer Serach(Skcode/ShopName/Mobile)  
        ///// </summary>
        ///// <param name="WarehouseId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("GlobalSearch/V1")]
        //[AllowAnonymous]

        //public HttpResponseMessage GlobalsearchV1(int WarehouseId, string Globalkey)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {

        //        GlobalcustomerDetail obj = new GlobalcustomerDetail();

        //        var customer = new List<SalespDTO>();
        //        if (!string.IsNullOrEmpty(Globalkey) && Globalkey.Length > 5)
        //        {

        //            //var query = "select cust.CustomerId,cust.CompanyId,cust.CreatedDate,cust.Active,cust.City,cust.WarehouseId,cust.lat,cust.lg,cust.ExecutiveId,cust.BeatNumber,cust.Day,cust.Skcode,cust.Mobile,cust.ShopName,cust.BillingAddress,cust.Name,cust.RefNo,cust.Password,cust.Emailid,cust.UploadRegistration,cust.ResidenceAddressProof,cust.DOB,com.InActiveCustomerCount as MaxOrderCount from Customers Cust  with(nolock) inner join Companies com on cust.CompanyId=com.Id Where  cust.WarehouseId =" + WarehouseId + " and  cust.Deleted = 0  and cust.IsHide = 0 and cust.Skcode like '%" + Globalkey + "%' or cust.Mobile like '%" + Globalkey + "%' or cust.ShopName like '%" + Globalkey + "%' ";
        //            var Warehouseid = new SqlParameter
        //            {
        //                ParameterName = "WarehouseId",
        //                Value = WarehouseId,

        //            };
        //            var GlobalKey = new SqlParameter
        //            {
        //                ParameterName = "Globalkey",
        //                Value = Globalkey,

        //            };
        //            customer = db.Database.SqlQuery<SalespDTO>("CustomerGlobalSearch @WarehouseId,@Globalkey", Warehouseid, GlobalKey).ToList();
        //        }
        //        if (customer.Count() > 0)
        //        {
        //            obj = new GlobalcustomerDetail()
        //            {
        //                customers = customer,
        //                Status = true,
        //                Message = "Customer Found"
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, obj);
        //        }
        //        else
        //        {
        //            obj = new GlobalcustomerDetail()
        //            {
        //                customers = customer,
        //                Status = false,
        //                Message = "No Customer found"
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, obj);
        //        }

        //    }
        //}
        //#endregion
        #region Update Customer Rating
        /// <summary>
        /// Update Customer Rating //by sudhir 27-09-2019
        /// </summary>
        /// <param name="CustomerRating"></param>
        /// <returns></returns>
        [Route("UpdateRating")]
        [AcceptVerbs("put")]
        [HttpPut]
        public HttpResponseMessage UpdateRating(Customer CustomerRating)
        {
            customerrating res;
            logger.Info("start update Customer Rating: ");

            using (AuthContext db = new AuthContext())
            {
                try
                {
                    Customer rating = db.Customers.Where(x => x.CustomerId == CustomerRating.CustomerId).FirstOrDefault();
                    rating.CustomerRating = CustomerRating.CustomerRating;
                    rating.CustomerRatingCommnets = CustomerRating.CustomerRatingCommnets;
                    db.Commit();

                    res = new customerrating()
                    {
                        customers = rating,
                        Status = true,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in UpdateRating " + ex.Message);
                    res = new customerrating()
                    {
                        Status = false,
                        Message = "somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        #endregion

        #region save APK version  of every user with respect to Skcode
        //    not in use now but if we remove OTP in future can use this with signin or signup to save device info 
        [Route("SaveAPKVersion")]
        [AcceptVerbs("PUT")]
        public HttpResponseMessage SaveAPKVersion(Customer PCdata)
        {
            SaveAPKVersionDTO CHData;
            using (AuthContext db = new AuthContext())
            {
                try
                {

                    Customer data = new Customer();

                    var APKv = db.Customers.Where(c => c.Skcode == PCdata.Skcode).SingleOrDefault();
                    APKv.Skcode = PCdata.Skcode;
                    APKv.CurrentAPKversion = PCdata.CurrentAPKversion;         // tejas to save device info 
                    APKv.PhoneOSversion = PCdata.PhoneOSversion;                 // tejas to save device info 
                    APKv.UserDeviceName = PCdata.UserDeviceName;                    // tejas to save device info
                                                                                    //db.Customers.Attach(APKv);
                    db.Entry(APKv).State = EntityState.Modified;
                    db.Commit();
                    #region Device History
                    //tejas 29-05-2019
                    var Customerhistory = db.Customers.Where(x => x.Mobile == PCdata.Mobile).FirstOrDefault();
                    try
                    {
                        PhoneRecordHistory phonerecord = new PhoneRecordHistory();
                        if (Customerhistory != null)
                        {
                            phonerecord.CustomerId = Customerhistory.CustomerId;
                            phonerecord.Name = Customerhistory.Name;
                            phonerecord.Skcode = Customerhistory.Skcode;
                            phonerecord.Mobile = Customerhistory.Mobile;
                            phonerecord.CurrentAPKversion = Customerhistory.CurrentAPKversion;
                            phonerecord.PhoneOSversion = Customerhistory.PhoneOSversion;
                            phonerecord.UserDeviceName = Customerhistory.UserDeviceName;
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



                    CHData = new SaveAPKVersionDTO()
                    {
                        Status = true,
                        Message = "Save  successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, CHData);
                }
                catch (Exception ex)

                {
                    CHData = new SaveAPKVersionDTO()
                    {
                        Status = false,
                        Message = "  Unsuccessfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, CHData);
                }
            }
        }
        #endregion


        #region Get Customer list with there device info 
        /// <summary>
        /// Get Customer list with there device info  //by tejas 21-05-2019
        /// </summary>

        /// <returns></returns>
        //[Authorize]
        [Route("GetCustomersDeviceInfo")]
        [HttpGet]
        public HttpResponseMessage GetCustomersDeviceInfo(int Cityid)
        {
            logger.Info("start GetCustomersDeviceInfo: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }

                    List<Customer> TotalItem = db.Customers.Where(x => x.Cityid == Cityid).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, TotalItem);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in TotalLineItem " + ex.Message);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }

        }

        #endregion

        #region get customerDevicehistory
        /// <summary>
        /// tejas 28-05-2019
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("customerDevicehistory")]
        [HttpGet]
        public dynamic customerDevicehistory(int CustomerId)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
                // Access claims
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                int CompanyId = compid;
                using (var db = new AuthContext())
                {
                    var data = db.PhoneRecordHistoryDB.Where(x => x.CustomerId == CustomerId).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion


        #region get udhar limits info on app
        /// <summary>
        /// Created by 18/01/2019
        /// Get Customer udhar limits.
        /// </summary>
        /// <returns></returns>
        [Route("GetUdhar")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage GetUdhar(int CustomerId)
        {
            logger.Info("start single  GetcusomerWallets: ");
            using (AuthContext db = new AuthContext())
            {
                Customer Item = new Customer();
                var cust = db.Customers.Where(c => c.CustomerId == CustomerId).SingleOrDefault();
                Customer w = new Customer();
                try
                {
                    if (cust != null)
                    {
                        Item.UdharLimit = cust.UdharLimit;
                        Item.UdharLimitRemaining = cust.UdharLimitRemaining;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, Item);

                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        #endregion
        #region for update version 

        /// <summary>
        /// created by 02/07/2019
        /// Created by Raj
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>

        [Route("updateversion")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage updateversion(string apptype, string version)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            customerDetail res;
            logger.Info("start addCustomer: ");
            using (AuthContext db = new AuthContext())
            {
                if (apptype == "RetailerApp")
                {
                    appVersion c = new appVersion();
                    try
                    {
                        appVersion lastdata = db.appVersionDb.Where(x => x.isCompulsory == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.isCompulsory = false;

                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }


                        c.App_version = version;
                        c.isCompulsory = true;
                        c.createdDate = DateTime.Now;
                        db.appVersionDb.Add(c);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }
                }
                else if (apptype == "DeliveryApp")
                {
                    DeliveryAppVersion c = new DeliveryAppVersion();

                    try
                    {
                        DeliveryAppVersion lastdata = db.DeliveryAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.isCompulsory = false;
                            lastdata.Active = false;
                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }


                        c.App_version = version;
                        c.isCompulsory = true;
                        c.Active = true;
                        c.createdDate = DateTime.Now;
                        db.DeliveryAppVersionDB.Add(c);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }

                }
                else if (apptype == "TradeApp")
                {
                    TradeAppVersion c = new TradeAppVersion();

                    try
                    {
                        TradeAppVersion lastdata = db.TradeAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.isCompulsory = false;
                            lastdata.Active = false;
                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }


                        c.App_version = version;
                        c.isCompulsory = true;
                        c.Active = true;
                        c.createdDate = DateTime.Now;
                        db.TradeAppVersionDB.Add(c);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }

                }
                else if (apptype == "SalesApp")
                {
                    SalesappVersion c = new SalesappVersion();
                    try
                    {

                        SalesappVersion lastdata = db.SalesappVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.isCompulsory = false;
                            lastdata.Active = false;
                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }

                        c.App_version = version;
                        c.isCompulsory = true;
                        c.Active = true;
                        c.createdDate = DateTime.Now;
                        db.SalesappVersionDB.Add(c);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }
                }
                else if (apptype == "SupplierApp")
                {
                    SupplierAppVersion c = new SupplierAppVersion();
                    try
                    {

                        SupplierAppVersion lastdata = db.SupplierAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.isCompulsory = false;
                            lastdata.Active = false;
                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }

                        c.App_version = version;
                        c.isCompulsory = true;
                        c.Active = true;
                        c.createdDate = DateTime.Now;
                        db.SupplierAppVersionDB.Add(c);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }
                }

                else if (apptype == "SarthiApp")
                {
                    SarthiAppVersion c = new SarthiAppVersion();

                    try
                    {
                        SarthiAppVersion lastdata = db.SarthiAppVersionDb.Where(x => x.isCompulsory == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.isCompulsory = false;
                            lastdata.IsActive = false;
                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }


                        c.App_version = version;
                        c.isCompulsory = true;
                        c.IsActive = true;
                        c.CreatedDate = DateTime.Now;
                        db.SarthiAppVersionDb.Add(c);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }

                }


                else if (apptype == "WuduApp")
                {
                    WuduAppVersion w = new WuduAppVersion();

                    try
                    {
                        WuduAppVersion lastdata = db.WuduAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.isCompulsory = false;
                            lastdata.Active = false;
                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }


                        w.App_version = version;
                        w.isCompulsory = true;
                        w.Active = true;
                        w.createdDate = DateTime.Now;
                        db.WuduAppVersionDB.Add(w);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, w);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, w);
                    }

                }
                else if (apptype == "DistributorApp")
                {
                    MongoDbHelper<DistributorAppVersion> mongoDbHelper = new MongoDbHelper<DistributorAppVersion>();
                    var DistPredicate = PredicateBuilder.New<DistributorAppVersion>(x => x.isCompulsory && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    DistributorAppVersion lastdata = mongoDbHelper.Select(DistPredicate).FirstOrDefault();
                    if (lastdata != null)
                    {
                        lastdata.isCompulsory = false;
                        lastdata.IsActive = false;
                        lastdata.ModifiedDate = DateTime.Now;
                        lastdata.ModifiedBy = userid;
                        mongoDbHelper.ReplaceWithoutFind(lastdata.Id, lastdata);
                    }

                    DistributorAppVersion lastdatanew = new DistributorAppVersion();

                    lastdatanew.App_version = version;
                    lastdatanew.isCompulsory = true;
                    lastdatanew.IsActive = true;
                    lastdatanew.CreatedBy = userid;
                    lastdatanew.CreatedDate = DateTime.Now;
                    mongoDbHelper.Insert(lastdatanew);


                    return Request.CreateResponse(HttpStatusCode.OK, lastdata);

                }
                else if (apptype == "NewDeliveryApp")
                {
                    NewDeliveryAppVersion c = new NewDeliveryAppVersion();

                    try
                    {
                        NewDeliveryAppVersion lastdata = db.NewDeliveryAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.isCompulsory = false;
                            lastdata.Active = false;
                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }


                        c.App_version = version;
                        c.isCompulsory = true;
                        c.Active = true;
                        c.createdDate = DateTime.Now;
                        db.NewDeliveryAppVersionDB.Add(c);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }

                }
                else if (apptype == "Consumer")
                {
                    ConsumerappVersion c = new ConsumerappVersion();

                    try
                    {
                        ConsumerappVersion lastdata = db.ConsumerAppVersionDb.Where(x => x.IsActive == true).FirstOrDefault();
                        if (lastdata != null)
                        {
                            lastdata.IsActive = false;
                            db.Entry(lastdata).State = EntityState.Modified;
                            db.Commit();
                        }


                        c.App_version = version;
                        c.IsActive = true;
                        c.createdDate = DateTime.Now;
                        db.ConsumerAppVersionDb.Add(c);
                        db.Commit();


                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, apptype, version);
            }
        }

        #endregion
        #region for current version 

        /// <summary>
        /// created by 08/08/2019
        /// Created by neha
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// 


        [Route("currentversion")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage currentversion(string apptype)
        {
            customerDetail res;
            logger.Info("start addCustomer: ");
            using (AuthContext db = new AuthContext())
            {
                if (apptype == "RetailerApp")
                {
                    appVersion c = new appVersion();
                    try
                    {
                        c = db.appVersionDb.Where(x => x.isCompulsory == true).FirstOrDefault();
                        logger.Info("End  Customer: ");
                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }
                }
                else if (apptype == "DeliveryApp")
                {
                    DeliveryAppVersion c = new DeliveryAppVersion();

                    try
                    {
                        c = db.DeliveryAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();

                        logger.Info("End  Customer: ");
                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }

                }
                else if (apptype == "TradeApp")
                {
                    TradeAppVersion c = new TradeAppVersion();

                    try
                    {
                        c = db.TradeAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();

                        logger.Info("End  Customer: ");
                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }

                }
                else if (apptype == "SalesApp")
                {
                    SalesappVersion c = new SalesappVersion();
                    try
                    {

                        c = db.SalesappVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();


                        logger.Info("End  Customer: ");
                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }
                }
                else if (apptype == "SupplierApp")
                {
                    SupplierAppVersion c = new SupplierAppVersion();
                    try
                    {

                        c = db.SupplierAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                        logger.Info("End SupplierApp: ");
                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }
                }
                else if (apptype == "SarthiApp")
                {
                    SarthiAppVersion c = new SarthiAppVersion();
                    try
                    {

                        c = db.SarthiAppVersionDb.Where(x => x.isCompulsory == true).FirstOrDefault();


                        logger.Info("End  Customer: ");
                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }
                }

                else if (apptype == "WuduApp")
                {
                    WuduAppVersion w = new WuduAppVersion();
                    try
                    {

                        w = db.WuduAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();


                        logger.Info("End  Customer: ");
                        return Request.CreateResponse(HttpStatusCode.OK, w);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, w);
                    }
                }
                else if (apptype == "DistributorApp")
                {
                    MongoDbHelper<DistributorAppVersion> mongoDbHelper = new MongoDbHelper<DistributorAppVersion>();
                    var DistPredicate = PredicateBuilder.New<DistributorAppVersion>(x => x.isCompulsory && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    DistributorAppVersion lastdata = mongoDbHelper.Select(DistPredicate).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, lastdata);
                }
                else if (apptype == "NewDeliveryApp")
                {
                    NewDeliveryAppVersion c = new NewDeliveryAppVersion();

                    try
                    {
                        c = db.NewDeliveryAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();

                        logger.Info("End  Customer: ");
                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }

                }
                else if (apptype == "Consumer")
                {
                    ConsumerappVersion c = new ConsumerappVersion();
                    try
                    {

                        c = db.ConsumerAppVersionDb.Where(x => x.IsActive == true).FirstOrDefault();


                        logger.Info("End  Customer: ");
                        return Request.CreateResponse(HttpStatusCode.OK, c);

                    }
                    catch (Exception ex)
                    {

                        return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                    }

                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, apptype);
            }
        }


        #endregion
        #region for update sales app version 

        /// <summary>
        /// created by 06/08/2019
        /// Created by Raj
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// 
        #endregion
        #region for update sales app version 

        /// <summary>
        /// created by 06/08/2019
        /// Created by Raj
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// 

        [Route("updateversionsalesapp")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage updateversionsalesapp(string version)
        {
            customerDetail res;
            SalesappVersion c = new SalesappVersion();


            logger.Info("start sales app version: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    SalesappVersion lastdata = db.SalesappVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                    if (lastdata != null)
                    {
                        lastdata.isCompulsory = false;
                        lastdata.Active = false;
                        db.Entry(lastdata).State = EntityState.Modified;
                        db.Commit();
                    }

                    c.App_version = version;
                    c.isCompulsory = true;
                    c.Active = true;
                    c.createdDate = DateTime.Now;
                    db.SalesappVersionDB.Add(c);
                    db.Commit();


                    return Request.CreateResponse(HttpStatusCode.OK, c);

                }
                catch (Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                }
            }
        }

        #endregion
        #region for update delivery app version 

        /// <summary>
        /// created by 06/08/2019
        /// Created by Raj
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>

        [Route("updateversiondelveryapp")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage updateversiondelveryapp(string version)
        {
            customerDetail res;
            DeliveryAppVersion c = new DeliveryAppVersion();


            logger.Info("start addCustomer: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    DeliveryAppVersion lastdata = db.DeliveryAppVersionDB.Where(x => x.isCompulsory == true).FirstOrDefault();
                    if (lastdata != null)
                    {
                        lastdata.isCompulsory = false;
                        lastdata.Active = false;
                        db.Entry(lastdata).State = EntityState.Modified;
                        db.Commit();
                    }


                    c.App_version = version;
                    c.isCompulsory = true;
                    c.Active = true;
                    c.createdDate = DateTime.Now;
                    db.DeliveryAppVersionDB.Add(c);
                    db.Commit();


                    return Request.CreateResponse(HttpStatusCode.OK, c);

                }
                catch (Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.BadRequest, c);
                }
            }
        }

        #endregion

        #region Generate Customer Password via OTP
        /// <summary>
        /// Created by 18/12/2018 
        /// OTP Genration code 
        /// </summary>
        /// <returns></returns>
        [Route("GenPwdotp")]
        [AllowAnonymous]
        public OTP GenPwdotp(string mobileNumber)
        {
            logger.Info("start Gen OTP: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    string password = "";
                    string MobileNumber = "";
                    using (var con = new AuthContext())
                    {
                        var cust = db.Customers.Where(x => x.Mobile == mobileNumber).Select(x => new { x.Mobile, x.Password }).FirstOrDefault();
                        if (cust != null)
                        {
                            password = cust.Password;
                            MobileNumber = cust.Mobile;
                        }
                    }
                    if (!string.IsNullOrEmpty(MobileNumber))
                    {
                        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        //string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                        string OtpMessage = " is Your Shopkirana Password. :)";
                        //string CountryCode = "91";
                        //string Sender = "SHOPKR";
                        //string authkey = Startup.smsauthKey;
                        //int route = 4;
                        //string path = "http://bulksms.newrise.in/api/sendhttp.php?authkey=" + authkey + "&mobiles=" + MobileNumber + "&message=" + password + " :" + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;

                        ////string path ="http://bulksms.newrise.in/api/sendhttp.php?authkey=100498AhbWDYbtJT56af33e3&mobiles=9770838685&message= SK OTP is : " + sRandomOTP + " &sender=SHOPKR&route=4&country=91";

                        //var webRequest = (HttpWebRequest)WebRequest.Create(path);
                        //webRequest.Method = "GET";
                        //webRequest.ContentType = "application/json";
                        //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                        //webRequest.ContentLength = 0; // added per comment 
                        //webRequest.Credentials = CredentialCache.DefaultCredentials;
                        //webRequest.Accept = "*/*";
                        //var webResponse = (HttpWebResponse)webRequest.GetResponse();
                        //if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);
                        //logger.Info("OTP Genrated: " + password);

                        Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, (password + " :" + OtpMessage), ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), "");
                    }
                    OTP a = new OTP()
                    {
                        OtpNo = password
                    };
                    return a;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OTP Genration.");
                    return null;
                }
            }
        }
        #endregion


        [Route("UpdateMissingClusters")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<bool> UpdateMissingClusters()
        {
            using (AuthContext db = new AuthContext())
            {
                var customers = await db.Database.SqlQuery<CustomerWithMissingCluster>("select customerid,lat,lg,ClusterId,ShippingAddress from Customers where  (ClusterId is null or clusterid =0) and lat<>0 and lg<>0 and CreatedBy = 'FromTradeApp' ").ToListAsync();

                if (customers != null && customers.Any())
                {
                    //GeoHelper geoHelper = new GeoHelper();
                    //var updateCustQuery = "update customers set  lat={0},lg={1} where customerid={2}";

                    //foreach (var item in customers)
                    //{
                    //    decimal? lat = null, lng = null;

                    //    if (!item.lat.HasValue || item.lat.Value == 0 || !item.lg.HasValue || item.lg.Value == 0)
                    //    {
                    //        geoHelper.GetLatLong(item.ShippingAddress, out lat, out lng);
                    //        item.lat = (double?)lat;
                    //        item.lg = (double?)lng;

                    //        if (item.lat.HasValue && item.lat.Value != 0 && item.lg.HasValue && item.lg.Value != 0)
                    //        {
                    //            var q21 = string.Format(updateCustQuery, item.lat, item.lg, item.customerId);

                    //            db.Database.ExecuteSqlCommand(q21);
                    //        }
                    //    }
                    //}

                    int i = 0;

                    if (customers.Any(x => x.lat.HasValue && x.lat.Value != 0 && x.lg.HasValue && x.lg.Value != 0))
                    {
                        var customersToUpdate = customers.Where(x => x.lat.HasValue && x.lat.Value != 0 && x.lg.HasValue && x.lg.Value != 0).ToList();
                        foreach (var item in customersToUpdate)
                        {
                            try
                            {
                                var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(item.lat.Value).Append("', '").Append(item.lg.Value).Append("')");
                                var clusterId = await db.Database.SqlQuery<int?>(query.ToString()).FirstOrDefaultAsync();
                                if (clusterId.HasValue)
                                {
                                    item.ClusterId = clusterId;
                                    i += 1;
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        if (customersToUpdate.Any(x => x.ClusterId.HasValue && x.ClusterId.Value > 0))
                        {
                            var clusterIds = customersToUpdate.Where(x => x.ClusterId.HasValue && x.ClusterId.Value > 0).Select(x => x.ClusterId.Value).Distinct().ToList();

                            var clusters = db.Clusters.Where(x => clusterIds.Contains(x.ClusterId)).Select(x => new ClusterMin
                            {
                                ClusterId = x.ClusterId,
                                ClusterName = x.ClusterName
                            }).ToList();

                            var query = "update customers set clusterid = {0}, clustername = '{1}' where customerid={2} ";
                            foreach (var item in customersToUpdate.Where(x => x.ClusterId.HasValue && x.ClusterId.Value > 0))
                            {
                                try
                                {
                                    var cluster = clusters.FirstOrDefault(x => x.ClusterId == item.ClusterId.Value);
                                    var updatequery = string.Format(query, item.ClusterId.Value, cluster.ClusterName, item.customerId);

                                    db.Database.ExecuteSqlCommand(updatequery);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        [Route("CheckCustomerCriticalInfo")]
        [HttpGet]
        public string CheckCustomerCriticalInfo(int customerId)
        {
            string result = "";
            using (var con = new AuthContext())
            {
                var cust = con.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.RefNo, x.UploadGSTPicture, x.UploadLicensePicture, x.lat, x.lg }).FirstOrDefault();
                if (cust != null)
                {
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

                }
            }

            return result;
        }


        [Route("GetCustomerV7")]
        [HttpPost]
        public IHttpActionResult GetCustomerV7(PagerDataUIViewModel pager)
        {
            CustomerPagerListDTO customerpagerList = new CustomerPagerListDTO();
            using (AuthContext context = new AuthContext())
            {
                logger.Info("start Get Customer: ");
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    int CompanyId = compid;
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    logger.Info("End  Customer: ");

                    var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

                    var customerlevel = new MonthlyCustomerLevel();

                    customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

                    var levels = customerlevel?.CustomerLevels;


                    var Totalcount = context.Customers.Count();
                    //customer = customer.ToList();
                    //var propertyInfo = typeof(Customer).GetProperty(pager.ColumnName);
                    List<Customer> data;
                    if (pager.IsAscending == true)
                    {
                        data = context.Customers.Where(x => x.Deleted == false).OrderByDescending(c => c.CreatedDate).Skip(pager.First).Take(pager.Last - pager.First).ToList();//ass.Count();
                    }
                    else
                    {
                        data = context.Customers.Where(x => x.Deleted == false).OrderByDescending(c => c.CreatedDate).Skip(pager.First).Take(pager.Last - pager.First).ToList();//ass.Count();
                    }
                    var warehouseIds = data.Select(x => x.Warehouseid).Distinct().ToList();
                    var warehouses = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId) && x.Deleted == false && x.IsKPP == false).ToList();

                    //var ChannelType = context.ChannelMasters.Where(x => x.Active == true && x.Deleted == false).ToList();
                    data.ForEach(x =>
                    {
                        // x.ChannelType = (ChannelType != null && ChannelType.Count() > 0) ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == x.ChannelMasterId) != null ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == x.ChannelMasterId).ChannelType : "" : "";
                        x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
                        if (levels != null && levels.Any())
                        {
                            var leveldata = levels.Where(a => a.SKCode == x.Skcode).Select(a => new { LevelName = a.LevelName, ColourCode = a.ColourCode }).FirstOrDefault();
                            if (leveldata != null)
                            {
                                x.CustomerLevel = leveldata.LevelName;
                                x.ColourCode = leveldata.ColourCode;
                            }
                        }
                        x.CustomerAppTypes = x.CustomerAppType == 1 ? "SkRetailer" : x.CustomerAppType == 2 ? "Trade Customer" :
                                  x.CustomerAppType == 3 ? "Zaruri Customer" : x.CustomerAppType == 4 ? "Distributor Customer" : null;

                        //
                        var customerDoc = context.CustomerDocs.Where(cd => cd.CustomerId == x.CustomerId && cd.IsActive).Select(y => new { y.DocPath, y.CustomerDocTypeMasterId, y.CreatedDate }).OrderByDescending(y => y.CreatedDate).FirstOrDefault();
                        if (customerDoc != null)
                        {
                            var custDocTypeMaster = context.CustomerDocTypeMasters.Where(cdm => cdm.Id == customerDoc.CustomerDocTypeMasterId && cdm.IsActive).Select(y => new { y.DocType }).FirstOrDefault();
                            if (custDocTypeMaster != null)
                            {
                                x.Type = custDocTypeMaster.DocType;
                            }
                            x.UploadLicensePicture = customerDoc.DocPath;
                        }
                        //
                    });
                    customerpagerList.TotalRecords = Totalcount;
                    customerpagerList.CustomerPagerList = data;
                    return Ok(customerpagerList);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }

        //[ResponseType(typeof(Customer))]
        [AllowAnonymous]
        [Route("GetSearchCustomerV7")]
        [HttpPost]
        //public IHttpActionResult GetSearchCustomerV7(string Cityid, string mobile, string skcode, DateTime? datefrom, DateTime? dateto, PagerDataUIViewModel pager, bool? IsDistributor ,string levelname, int? CustomerAppType)
        public IHttpActionResult GetSearchCustomerV7(SearchFilter searchFilter)
        {
            logger.Info("start OrderMaster: ");
            using (AuthContext context = new AuthContext())
            {
                CustomerPagerListDTO customerPagerList = new CustomerPagerListDTO();
                List<Customer> ass = new List<Customer>();

                if (searchFilter.mobile == "undefined" || searchFilter.mobile == null) { searchFilter.mobile = ""; }
                if (searchFilter.skcode == "undefined" || searchFilter.skcode == null) { searchFilter.skcode = ""; }
                if (searchFilter.Cityid == "undefined" || searchFilter.Cityid == null) { searchFilter.Cityid = "0"; }
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                //var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

                //var customerlevel = new MonthlyCustomerLevel();

                //customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

                //var levels = customerlevel?.CustomerLevels;

                int CompanyId = compid;

                List<int> ids = new List<int>();

                //levelname = "Level 0";

                //if (!string.IsNullOrEmpty(searchFilter.levelname) && levels != null && levels.Any())
                //{
                //    ids = levels.Where(x => x.LevelName == searchFilter.levelname).Select(x => x.CustomerId).ToList();
                //}

                List<int> Didids = new List<int>();
                if (searchFilter.IsDistributor == true)
                {
                    Didids = context.DistributorVerificationDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.CustomerID).ToList();
                }

                //ass = context.filteredCustomerMaster(searchFilter.Cityid, searchFilter.datefrom, searchFilter.dateto, searchFilter.mobile, searchFilter.skcode, ids).ToList();
                #region
                if (searchFilter.dateto != null && searchFilter.datefrom != null)
                {
                    searchFilter.dateto = searchFilter.dateto.AddDays(1).AddSeconds(-1);
                }


                int cityid = Convert.ToInt32(searchFilter.Cityid);
                int Warehouseid = Convert.ToInt32(searchFilter.Warehouseid);
                int ClusterId = Convert.ToInt32(searchFilter.ClusterId);

                var predicate = PredicateBuilder.True<Customer>();
                predicate = predicate.And(x => x.Deleted == false);
                predicate = predicate.And(x => x.IsHide == false);
                if (ids != null && ids.Any())
                    predicate = predicate.And(x => ids.Contains(x.CustomerId));

                if (Didids != null && Didids.Any())
                    predicate = predicate.And(x => Didids.Contains(x.CustomerId));

                if (cityid > 0)
                    predicate = predicate.And(x => x.Cityid == cityid);

                if (Warehouseid > 0)
                    predicate = predicate.And(x => x.Warehouseid == Warehouseid);

                if (ClusterId > 0)
                    predicate = predicate.And(x => x.ClusterId == ClusterId);


                if (!string.IsNullOrEmpty(searchFilter.mobile))
                    predicate = predicate.And(x => x.Mobile == searchFilter.mobile);
                string skcode = searchFilter.skcode;
                if (!string.IsNullOrEmpty(searchFilter.skcode))
                    predicate = predicate.And(x => x.Skcode == searchFilter.skcode);

                if (searchFilter.datefrom != null && searchFilter.dateto != null)
                    predicate = predicate.And(x => x.CreatedDate > searchFilter.datefrom && x.CreatedDate < searchFilter.dateto);
                int CustomerAppType = searchFilter.CustomerAppType;
                if (searchFilter.CustomerAppType > 0)
                    predicate = predicate.And(x => x.CustomerAppType == CustomerAppType);

                ass = context.Customers.Where(predicate).OrderBy(x => x.CustomerId).Skip(searchFilter.pager.First).Take(searchFilter.pager.Last - searchFilter.pager.First).ToList();
                //ass = context.Customers.Where(predicate).ToList();
                var totalcount = context.Customers.Where(predicate).Count();
                #endregion
                var warehouseIds = ass.Select(x => x.Warehouseid).Distinct().ToList();
                var warehouses = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId) && x.Deleted == false && x.IsKPP == false).ToList();
                var peopleIds = ass.Select(x => x.GrabbedBy).ToList();
                peopleIds.AddRange(ass.Where(x => x.VerifiedBy.HasValue).Select(x => x.VerifiedBy.Value).ToList());
                var peopleData = context.Peoples.Where(s => peopleIds.Contains(s.PeopleID)).Select(x => new { x.DisplayName, x.PeopleID });
                // var ChannelType = context.ChannelMasters.Where(x => x.Active == true && x.Deleted == false).ToList();
                ass.ForEach(x =>
                {
                    // x.ChannelType = (ChannelType != null && ChannelType.Count() > 0) ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == x.ChannelMasterId) != null ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == x.ChannelMasterId).ChannelType : "" : "";
                    x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
                    if (x.GrabbedBy > 0 && peopleData != null && peopleData.Any(y => y.PeopleID == x.GrabbedBy))
                    {
                        x.grabbedByName = peopleData.FirstOrDefault(y => y.PeopleID == x.GrabbedBy).DisplayName;
                    }

                    if (x.VerifiedBy.HasValue && x.VerifiedBy.Value > 0 && peopleData != null && peopleData.Any(y => y.PeopleID == x.VerifiedBy.Value))
                    {
                        x.VerifiedByName = peopleData.FirstOrDefault(y => y.PeopleID == x.VerifiedBy.Value).DisplayName;
                    }

                    //
                    var customerDoc = context.CustomerDocs.Where(cd => cd.CustomerId == x.CustomerId && cd.IsActive).Select(y => new { y.DocPath, y.CustomerDocTypeMasterId, y.CreatedDate }).OrderByDescending(y => y.CreatedDate).FirstOrDefault();
                    if (customerDoc != null)
                    {
                        var custDocTypeMaster = context.CustomerDocTypeMasters.Where(cdm => cdm.Id == customerDoc.CustomerDocTypeMasterId && cdm.IsActive).Select(y => new { y.DocType }).FirstOrDefault();
                        if (custDocTypeMaster != null)
                        {
                            x.Type = custDocTypeMaster.DocType;
                        }
                        x.UploadLicensePicture = customerDoc.DocPath;
                    }
                    //
                    //if (levels != null && levels.Any())
                    //{
                    //    var leveldata = levels.Where(a => a.SKCode.Contains(x.Skcode)).Select(a => new { LevelName = a.LevelName, ColourCode = a.ColourCode }).FirstOrDefault();
                    //    if (leveldata != null)
                    //    {
                    //        x.CustomerLevel = leveldata.LevelName;
                    //        x.ColourCode = leveldata.ColourCode;
                    //    }
                    //}
                });
                var propertyInfo = typeof(Customer).GetProperty(searchFilter.pager.ColumnName);
                List<Customer> data;

                //if (searchFilter.pager != null && searchFilter.pager.IsAscending != null && searchFilter.pager.IsAscending == true)
                //{
                //    data = context.Customers.Where(x => x.Deleted == false).OrderByDescending(c => c.CreatedDate).Skip(searchFilter.pager.First).Take(searchFilter.pager.Last - searchFilter.pager.First).ToList();//ass.Count();
                //}
                //else
                //{
                //    data = context.Customers.Where(x => x.Deleted == false).OrderByDescending(c => c.CreatedDate).Skip(searchFilter.pager.First).Take(searchFilter.pager.Last - searchFilter.pager.First).ToList();//ass.Count();
                //}


                if (searchFilter.pager.IsAscending == true)
                {
                    data = ass.AsEnumerable().OrderBy(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
                }
                else
                {
                    data = ass.AsEnumerable().OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
                }
                #region CRMTAG

                var skcodeList = data.Select(x => x.Skcode).Distinct().ToList();

                CRMManager cRMManager = new CRMManager();
                var TagData = cRMManager.GetCRMCustomerWithTag(skcodeList, CRMPlatformConstants.AllTagsOnCustomerMaster);
                foreach (var x in data)
                {
                    x.CRMTags = TagData != null && TagData.Result != null ? TagData.Result.Where(y => y.Skcode == x.Skcode).Select(z => z.CRMTags).FirstOrDefault() : null;
                }

                #endregion

                customerPagerList.TotalRecords = totalcount;
                customerPagerList.CustomerPagerList = data.ToList();
                return Ok(customerPagerList);
            }

        }
        [Route("GetCustomersMissingDetailV7")]
        [HttpPost]
        public IHttpActionResult GetCustomersMissingDetailV7(PagerDataUIViewModel pager)
        {
            logger.Info("start Customer: ");
            List<Customer> ass = new List<Customer>();
            CustomerPagerListDTO customerPagerList = new CustomerPagerListDTO();
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var list = db.Customers.Where(p => p.Name == null || p.ShopName == null || p.ResidenceAddressProof == null || p.Mobile == null || p.UploadRegistration == null || p.City == null || !p.Warehouseid.HasValue || p.Skcode == null || p.AreaName == null || p.lat == 0 || p.lg == 0 || p.AgentCode == null).ToList();
                    var warehouseIds = list.Select(x => x.Warehouseid).Distinct().ToList();
                    var warehouses = db.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
                    // var ChannelType = db.ChannelMasters.Where(x => x.Active == true && x.Deleted == false).ToList();
                    list.ForEach(x =>
                    {
                        x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
                        // x.ChannelType = (ChannelType != null && ChannelType.Count() > 0) ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == x.ChannelMasterId) != null ? ChannelType.FirstOrDefault(y => y.ChannelMasterId == x.ChannelMasterId).ChannelType : "" : "";
                    });
                    logger.Info("End  Customer: ");
                    var propertyInfo = typeof(Customer).GetProperty(pager.ColumnName);
                    List<Customer> data;
                    if (pager.IsAscending == true)
                    {
                        data = list.AsEnumerable().OrderBy(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
                    }
                    else
                    {
                        data = list.AsEnumerable().OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
                    }
                    customerPagerList.TotalRecords = data.Count();
                    customerPagerList.CustomerPagerList = data.Skip(pager.First).Take(pager.Last - pager.First).ToList();
                    return Ok(customerPagerList);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }

        [Route("UploadDoc")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult UploadDoc(string Mobile)
        {

            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    // int CompanyId = compid;
                    int? CustomerId = db.Customers.Where(x => x.Mobile.Trim() == Mobile).Select(y => y.CustomerId).FirstOrDefault();
                    if (CustomerId > 0)
                    {

                        return Ok(CustomerId);
                    }
                    else
                    {
                        return Ok("Customer Not Found");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return InternalServerError();
                }
            }
        }
        [Route("CustSMSDocHistory")]
        [HttpGet]
        [AllowAnonymous]
        //public CustWarehouse CustWarehouseCustomer(int CustomerId)
        public dynamic CustSMSDocHistory(int CustomerId)
        {
            logger.Info("start City: ");
            using (AuthContext db = new AuthContext())
            {
                dynamic result = null;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    int CompanyId = compid;
                    //customer = db.Customers.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                    result = (from a in db.CustomerSmsDB
                              join p in db.CustomerDocumentDB on a.CustomerId equals CustomerId

                              select new CustSMSDocDTO
                              {
                                  CustomerId = a.CustomerId,
                                  createdate = a.CreatedDate,
                                  updateDate = p.UpdateDate,
                                  Shopimage = p.Shopimage,
                                  UploadGSTPicture = p.UploadGSTPicture,
                                  UploadRegistration = p.UploadRegistration,
                                  IsBulk = a.IsBulk
                              }).ToList(); new List<CustSMSDocDTO>();
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customer " + ex.Message);
                    logger.Info("End  Customer: ");
                    return null;
                }
            }
        }


        /// <summary>
        /// Created by 09/12/2018 
        /// OTP Genration code 
        /// </summary>
        /// <returns></returns>   
        [Route("CustomerUploadDocSMS")]
        [HttpGet]
        [AllowAnonymous]
        public bool CustomerUploadDocSMS(string Mobile)
        {
            logger.Info("start Gen OTP: ");
            try
            {
                Customer cust = null;
                using (var context = new AuthContext())
                {
                    cust = context.Customers.FirstOrDefault(x => x.Mobile == Mobile);

                    if (cust != null)
                    {
                        String sRandomOTP = "Dear " + cust.Skcode + "," + Environment.NewLine + "Please click on below URL to upload Documents: " + Environment.NewLine + ConfigurationManager.AppSettings["CustomerUploadUrl"].Replace("[[Mobile]]", Mobile);
                        string OtpMessage = (sRandomOTP + Environment.NewLine + " Shopkirana ");
                        //string CountryCode = "91";
                        //string Sender = "SHOPKR";
                        //string authkey = Startup.smsauthKey;
                        //int route = 4;

                        //var queryString = "?authkey=" + authkey + "&mobiles=" + Mobile + "&message= " + OtpMessage + " &sender=" + Sender + "&route=" + route + "&country=" + CountryCode;
                        //string sendWithKnowlarity = ConfigurationManager.AppSettings["SendMsgFromKnowlarity"];
                        //string path = sendWithKnowlarity == "true"
                        //                        ? ConfigurationManager.AppSettings["KnowlaritySMSUrl"].Replace("[[Mobile]]", Mobile).Replace("[[Message]]", OtpMessage)
                        //                        : "http://bulksms.newrise.in/api/sendhttp.php" + queryString;
                        //var webRequest = (HttpWebRequest)WebRequest.Create(path);
                        //webRequest.Method = "GET";
                        //webRequest.ContentType = "application/json";
                        //webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                        //webRequest.ContentLength = 0;
                        //webRequest.Credentials = CredentialCache.DefaultCredentials;
                        //webRequest.Accept = "*/*";
                        //var webResponse = (HttpWebResponse)webRequest.GetResponse();
                        //if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);
                        Common.Helpers.SendSMSHelper.SendSMS(Mobile, OtpMessage, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), "");
                        CustomerSms sms = new CustomerSms();
                        sms.CustomerId = cust.CustomerId;
                        sms.IsSMSSend = true;
                        sms.CreatedDate = indianTime;
                        context.CustomerSmsDB.Add(sms);
                        context.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in OTP Genration. " + ex.ToString());
            }

            return true;

        }


        [Route("CallCustomerForMissingDocs")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> CallCustomerForMissingDocs(int customerId)
        {
            using (var context = new AuthContext())
            {
                var customer = await context.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);

                var knowlarityUrl = ConfigurationManager.AppSettings["KnowlarityMissingDocApi"];
                if (customer != null && !string.IsNullOrEmpty(knowlarityUrl) && (string.IsNullOrEmpty(customer.CustomerVerify) || customer.CustomerVerify.ToLower() != "full verified"))
                {
                    knowlarityUrl = knowlarityUrl.Replace("{{Mobile}}", customer.Mobile);
                    using (GenericRestHttpClient<string, string> memberClient
                        = new GenericRestHttpClient<string, string>(knowlarityUrl,
                        "", null))
                    {

                        var response = memberClient.GetAsync();

                    }
                }
            }

            return true;
        }


        [Route("SendSMSToAllCustomer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> SendSMSToAllCustomer()
        {
            try
            {
                List<Customer> list = null;
                List<CustomerSms> customerSMSes = new List<CustomerSms>();
                using (var context = new AuthContext())
                {
                    var customers = context.Customers.Where(x => /*x.ClusterId == 47*/ x.Mobile == "9111292909" || x.Mobile == "9770501428").ToList();

                    list = customers.ToList();
                }

                #region Trade Kirana Promotion
                string url = @"http://obd1.clouddial.in/api/voice/voice_broadcast.php?username=u1689&token=expIyY&plan_id=4395&announcement_id=199377&caller_id=Test&contact_numbers={#number#}&retry_json={'FNA':'1','FBZ':0,'FCG':'2','FFL':'1'}";
                string messageText = HttpUtility.UrlEncode("नमस्कार आपका ट्रेड किराना मे स्वागत है। खुदरा व्यापार में खरीदना एवं बेचना हुआ आसान| दिए गए लिंक पर Click करे और Download करे TRADE KIRANA APP. http://bit.ly/31G9OmL");
                //string messageText2 = HttpUtility.UrlEncode("Dhampure Sugar Sulphurless loose 50kg sirf Rs 2225. Only on TRADE KIRANA APP, Indore. Abhi Link par Click (http://bit.ly/31G9OmL), Download Aur Order");
                //string messageText3 = HttpUtility.UrlEncode("Varalakshmi Sabudana 1kg X 25kg sirf Rs 1730. Only on TRADE KIRANA APP, Indore. Abhi Link par Click (http://bit.ly/31G9OmL), Download Aur Order");
                //string messageText4 = HttpUtility.UrlEncode("Lakshya Mordhan 500gm X 25Kg sirf Rs 2125. Only on TRADE KIRANA APP, Indore. Abhi Link par Click (http://bit.ly/31G9OmL), Download Aur Order");
                #endregion


                for (int i = 0; i < list.Count; i += 50)
                {
                    var custBatch = list.Skip(i).Take(50).ToList();
                    var mobileNumbers = custBatch.Select(x => x.Mobile).ToList();
                    var knowLarityUrl = url.Replace("{#number#}", string.Join(",", mobileNumbers));

                    try
                    {

                        if (DateTime.Now.TimeOfDay.Hours < 21 && DateTime.Now.TimeOfDay.Hours > 9)
                        {
                            BackgroundTaskManager.Run(() =>
                            {
                                BackgroundJob.Schedule(() => CallCustomers(knowLarityUrl), TimeSpan.FromSeconds(3));
                            });
                        }

                        BackgroundTaskManager.Run(() =>
                        {
                            string sendWithKnowlarity = ConfigurationManager.AppSettings["SendMsgFromKnowlarity"];
                            var messageAPIUrl = "http://sms.o2technology.in/api/sendhttp.php?authkey=303246AVBbm4zFs5dc941d2&mobiles=[[Mobile]]&message=[[Message]]&sender=TRADKR&route=4&unicode=1";//ConfigurationManager.AppSettings["KnowlaritySMSUrl"];
                            string path = messageAPIUrl.Replace("SHOPKR", "TRADKR").Replace("[[Mobile]]", string.Join(",", mobileNumbers)).Replace("[[Message]]", messageText);

                            var webRequest = (HttpWebRequest)WebRequest.Create(path);
                            webRequest.Method = "GET";
                            webRequest.ContentType = "application/json";
                            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                            webRequest.ContentLength = 0;
                            webRequest.Credentials = CredentialCache.DefaultCredentials;
                            webRequest.Accept = "*/*";
                            var webResponse = (HttpWebResponse)webRequest.GetResponse();
                            if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);

                            //Thread.Sleep(3000);

                            //path = ConfigurationManager.AppSettings["KnowlaritySMSUrl"].Replace("SHOPKR", "TRADKR").Replace("[[Mobile]]", string.Join(",", mobileNumbers)).Replace("[[Message]]", messageText2);
                            //var webRequest2 = (HttpWebRequest)WebRequest.Create(path);
                            //webRequest2.Method = "GET";
                            //webRequest2.ContentType = "application/json";
                            //webRequest2.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                            //webRequest2.ContentLength = 0;
                            //webRequest2.Credentials = CredentialCache.DefaultCredentials;
                            //webRequest2.Accept = "*/*";
                            //var webResponse2 = (HttpWebResponse)webRequest2.GetResponse();
                            //if (webResponse2.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse2.Headers);

                            ////Thread.Sleep(3000);

                            //path = ConfigurationManager.AppSettings["KnowlaritySMSUrl"].Replace("SHOPKR", "TRADKR").Replace("[[Mobile]]", string.Join(",", mobileNumbers)).Replace("[[Message]]", messageText3);
                            //var webRequest3 = (HttpWebRequest)WebRequest.Create(path);
                            //webRequest3.Method = "GET";
                            //webRequest3.ContentType = "application/json";
                            //webRequest3.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                            //webRequest3.ContentLength = 0;
                            //webRequest3.Credentials = CredentialCache.DefaultCredentials;
                            //webRequest3.Accept = "*/*";
                            //var webResponse3 = (HttpWebResponse)webRequest3.GetResponse();
                            //if (webResponse3.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse3.Headers);

                            ////Thread.Sleep(3000);

                            //path = ConfigurationManager.AppSettings["KnowlaritySMSUrl"].Replace("SHOPKR", "TRADKR").Replace("[[Mobile]]", string.Join(",", mobileNumbers)).Replace("[[Message]]", messageText4);
                            //var webRequest4 = (HttpWebRequest)WebRequest.Create(path);
                            //webRequest4.Method = "GET";
                            //webRequest4.ContentType = "application/json";
                            //webRequest4.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                            //webRequest4.ContentLength = 0;
                            //webRequest4.Credentials = CredentialCache.DefaultCredentials;
                            //webRequest4.Accept = "*/*";
                            //var webResponse4 = (HttpWebResponse)webRequest4.GetResponse();
                            //if (webResponse4.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse4.Headers);

                        });

                        customerSMSes.AddRange(custBatch.Select(x => new CustomerSms
                        {
                            CreatedDate = indianTime,
                            CustomerId = x.CustomerId,
                            IsBulk = true,
                            IsSMSSend = true
                        }).ToList());

                    }
                    catch (Exception exe)
                    {
                        TextFileLogHelper.LogError("SendSMSToAllCustomer error for customers :  " + string.Join(",", mobileNumbers) + Environment.NewLine + exe.ToString());
                    }
                }

                using (var context = new AuthContext())
                {
                    context.CustomerSmsDB.AddRange(customerSMSes);
                    context.Commit();
                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.LogError("SendSMSToAllCustomer Outer error : " + ex.ToString());
            }

            return true;
        }

        [Route("SendMissingDocSMSToAllCustomer")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> SendMissingDocSMSToAllCustomer()
        {
            try
            {
                List<Customer> list = null;
                List<CustomerSms> customerSMSes = new List<CustomerSms>();
                using (var context = new AuthContext())
                {
                    var customers = await context.Customers.Where(x => (string.IsNullOrEmpty(x.CustomerVerify) || x.CustomerVerify != "Full Verified") && (x.ClusterId == 51 || x.ClusterId == 55)/*x.Mobile == "9111292909" || x.Mobile == "9770501428"*/).ToListAsync();

                    list = customers.ToList();
                }

                var url = ConfigurationManager.AppSettings["KnowlarityMissingDocApi"];
                var messageText = "Please click on below URL to upload Documents: " + Environment.NewLine + ConfigurationManager.AppSettings["CustomerUploadUrl"] + Environment.NewLine + "-- Shopkirana";//.Replace("[[Mobile]]", Mobile);

                foreach (var item in list)
                {
                    //var custBatch = list.Skip(i).Take(50).ToList();
                    var mobileNumbers = item.Mobile;
                    var knowLarityUrl = url.Replace("{#number#}", mobileNumbers);
                    var sendmessageText = HttpUtility.UrlEncode(messageText.Replace("[[Mobile]]", mobileNumbers));
                    try
                    {

                        if (DateTime.Now.TimeOfDay.Hours < 21 && DateTime.Now.TimeOfDay.Hours > 9)
                        {
                            BackgroundTaskManager.Run(() =>
                            {
                                BackgroundJob.Schedule(() => CallCustomers(knowLarityUrl), TimeSpan.FromSeconds(3));
                            });
                        }

                        BackgroundTaskManager.Run(() =>
                        {

                            string path = ConfigurationManager.AppSettings["KnowlaritySMSUrl"].Replace("[[Mobile]]", mobileNumbers).Replace("[[Message]]", sendmessageText);

                            var webRequest = (HttpWebRequest)WebRequest.Create(path);
                            webRequest.Method = "GET";
                            webRequest.ContentType = "application/json";
                            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
                            webRequest.ContentLength = 0;
                            webRequest.Credentials = CredentialCache.DefaultCredentials;
                            webRequest.Accept = "*/*";
                            var webResponse = (HttpWebResponse)webRequest.GetResponse();
                            if (webResponse.StatusCode != HttpStatusCode.OK) Console.WriteLine("{0}", webResponse.Headers);

                        });

                        customerSMSes.Add(new CustomerSms
                        {
                            CreatedDate = indianTime,
                            CustomerId = item.CustomerId,
                            IsBulk = true,
                            IsSMSSend = true
                        });

                    }
                    catch (Exception exe)
                    {
                        TextFileLogHelper.LogError("SendMissingDocSMSToAllCustomer error for customers :  " + string.Join(",", mobileNumbers) + Environment.NewLine + exe.ToString());
                    }
                }

                using (var context = new AuthContext())
                {
                    context.CustomerSmsDB.AddRange(customerSMSes);
                    context.Commit();
                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.LogError("SendSMSToAllCustomer Outer error : " + ex.ToString());
            }

            return true;
        }


        public void CallCustomers(string knowlarityUrl)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(knowlarityUrl);
            webRequest.Method = "GET";
            webRequest.ContentType = "application/json";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:28.0) Gecko/20100101 Firefox/28.0";
            webRequest.ContentLength = 0;
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            webRequest.Accept = "*/*";
            var webResponse = (HttpWebResponse)webRequest.GetResponse();

        }



        [Route("UpdateMongoOrder")]
        [AllowAnonymous]
        [HttpGet]
        public bool Getdata(int OrderId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;

                    OrderMaster ss = context.DbOrderMaster.Where(a => a.OrderId == OrderId).SingleOrDefault();
                    ss.BillingAddress = ss.BillingAddress + " ";
                    context.Entry(ss).State = EntityState.Modified;
                    context.Commit();
                    return true;
                }


                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return false;
                }
            }
        }

        /// creaetd by 14/11/2019
        [Route("CustomerFeedbackQuestion")]
        [AcceptVerbs("Get")]
        [HttpGet]
        [AllowAnonymous]

        public HttpResponseMessage CustomerFeedbackQuestion(int WarehouseId)
        {
            customerQuestionDTO res;
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var Customerquestion = db.CustomerFeedbackQuestionDB.Where(c => c.WarehouseId == WarehouseId && c.IsActive == true && c.Deleted == false).ToList();

                    if (Customerquestion.Count() > 0)
                    {
                        res = new customerQuestionDTO()
                        {
                            Questiondata = Customerquestion,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new customerQuestionDTO()
                        {
                            Questiondata = null,
                            Status = false,
                            Message = "Question not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new customerQuestionDTO()
                    {
                        Questiondata = null,
                        Status = false,
                        Message = "somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }
        /// creaetd by 14/11/2019
        [Route("CustomerOrderFeedback")]
        [AcceptVerbs("Post")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage CustomerOrderFeedback(CustomerOrderFeedback obj)
        {
            customerfeedbackDTO res;
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    if (obj != null)
                    {
                        CustomerOrderFeedback CustomerFeedback = new CustomerOrderFeedback();

                        CustomerFeedback.CustomerId = obj.CustomerId;
                        CustomerFeedback.OrderId = obj.OrderId;
                        CustomerFeedback.Rating = obj.Rating;
                        CustomerFeedback.FeedbackQuestionIds = obj.FeedbackQuestionIds;
                        CustomerFeedback.Comments = obj.Comments;
                        CustomerFeedback.CreatedDate = DateTime.Now;
                        db.CustomerOrderFeedbackDB.Add(CustomerFeedback);
                        db.Commit();

                        res = new customerfeedbackDTO()
                        {
                            Questionfeedbackdata = CustomerFeedback,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        res = new customerfeedbackDTO()
                        {
                            Questionfeedbackdata = null,
                            Status = false,
                            Message = "FeedBack not exist."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new customerfeedbackDTO()
                    {
                        Questionfeedbackdata = null,
                        Status = false,
                        Message = "somthing went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }

        [Route("FilterCustomerData")]
        [HttpPost]
        public dynamic FilterCustomerData(PagerDataUIViewModelcustomer pager)
        {
            CustomerPagerListDTO customerpagerList = new CustomerPagerListDTO();
            using (AuthContext db = new AuthContext())
            {
                var MainData = new List<Customer>();
                if (pager.Data == "Retailer")
                {
                    MainData = db.Customers.Where(x => x.Deleted == false && x.Active == true && x.IsKPP == false).OrderByDescending(c => c.CreatedDate).ToList();
                }
                else if (pager.Data == "IsKPP")
                {
                    MainData = db.Customers.Where(x => x.Deleted == false && x.Active == true && x.IsKPP == true).OrderByDescending(c => c.CreatedDate).ToList();
                }
                else if (pager.Data == "Direct Open Network")
                {
                    MainData = db.Customers.Where(x => x.Deleted == false && x.CustomerType == "Direct Open Network").OrderByDescending(c => c.CreatedDate).ToList();
                }
                else if (pager.Data == "SKP Owner")
                {
                    MainData = db.Customers.Where(x => x.Deleted == false && x.CustomerType == "SKP Owner").OrderByDescending(c => c.CreatedDate).ToList();
                }
                else if (pager.Data == "SKP Retailer")
                {
                    MainData = db.Customers.Where(x => x.Deleted == false && x.CustomerType == "SKP Retailer").OrderByDescending(c => c.CreatedDate).ToList();
                }
                else if (pager.Data == "Trader")
                {
                    MainData = db.Customers.Where(x => x.Deleted == false && x.CustomerType == "Trader").OrderByDescending(c => c.CreatedDate).ToList();
                }
                else if (pager.Data == "Wholesaler")
                {
                    MainData = db.Customers.Where(x => x.Deleted == false && x.CustomerType == "Wholesaler").OrderByDescending(c => c.CreatedDate).ToList();
                }
                var warehouseIds = MainData.Select(x => x.Warehouseid).Distinct().ToList();
                var warehouses = db.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
                MainData.ForEach(x =>
                {
                    x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
                });
                var propertyInfo = typeof(Customer).GetProperty(pager.ColumnName);
                List<Customer> data;
                if (pager.IsAscending == true)
                {
                    data = MainData.Where(x => (pager.Contains == null || (x.Name.Contains(pager.Contains)))).AsEnumerable().OrderBy(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
                }
                else
                {
                    data = MainData.Where(x => (pager.Contains == null || (x.Name.Contains(pager.Contains)))).AsEnumerable().OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();//ass.Count();
                }
                customerpagerList.TotalRecords = data.Count();
                customerpagerList.CustomerPagerList = data.Skip(pager.First).Take(pager.Last - pager.First).ToList();
                return customerpagerList;
            }
            return null;
        }

        [Route("getAllCustomerFeedbackData")]
        [HttpGet]
        public dynamic getAllCustomerFeedback(int? WarehouseId, string search, DateTime? from, DateTime? To)
        {
            using (var db = new AuthContext())
            {

                if (search == "undefined")
                {
                    search = "";
                }
                if (WarehouseId == null)
                {
                    WarehouseId = 0;
                }
                string Start = from?.ToString("yyyy-MM-dd");
                string End = To?.ToString("yyyy-MM-dd");

                var WarehouseIds = new SqlParameter("WarehouseId", WarehouseId ?? (object)DBNull.Value);
                var searchs = new SqlParameter("Search", search);
                var StartDate = new SqlParameter("StartDate", Start ?? (object)DBNull.Value);
                var EndDate = new SqlParameter("EndDate", End ?? (object)DBNull.Value);

                var getAllCustFeedback = db.Database.SqlQuery<CustomerFeedbackDTO>("exec CustomerFeedback @WarehouseId,@Search,@StartDate,@EndDate", WarehouseIds, searchs, StartDate, EndDate).ToList().OrderByDescending(x => x.RatingDate);
                return getAllCustFeedback;
            }

        }

        [AllowAnonymous]
        [Route("GetWalletPoints")]
        [HttpGet]
        public WalletPointsSummary GetWalletPoints(int CustomerId, int page)
        {
            WalletPointsSummary WalletPointsSummary = new WalletPointsSummary();
            using (var db = new AuthContext())
            {

                if (page == 0)
                {
                    var take = 10;
                    var skip = 0;
                    //var query1 = "select IsNull(TotalAmount,0) as TotalEarnPoint,TransactionDate from Wallets where CustomerId=" + CustomerId;
                    //var query2 = "select SUM(IsNull(BILLDISCOUNTTYPEVALUE,0))  as UpcomingPoints  from BillDiscounts where IsAddNextOrderWallet=1 and customerid=" + CustomerId;
                    //var query3 = "select  IsNull(ORDERID,0) ORDERID,IsNull(NewAddedWAmount,0) NewAddedWAmount,IsNull(NewOutWAmount,0)NewOutWAmount,TransactionDate,Through "
                    //                + "from CustomerWalletHist ories where CustomerId = " + CustomerId + "order by id "
                    //                + "desc  offset " + skip + " rows fetch next " + take + " rows only";
                    //var query4 = "select  COUNT(Id) from CustomerWalletHistories where  CustomerId=" + CustomerId;




                    CustomersManager manager = new CustomersManager();
                    var wdata = manager.GetWalletPoints(CustomerId, page);

                    WalletPointsSummary.UpcomingPoints = wdata.UpcomingPoints;
                    WalletPointsSummary.TotalEarnPoint = wdata.TotalEarnPoint;
                    WalletPointsSummary.TotalUsedPoints = wdata.TotalUsedPoints;
                    WalletPointsSummary.totalCount = wdata.totalCount;
                    WalletPointsSummary.HowToUseWalletPoints = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => x.Wallet_TermsCondition).SingleOrDefault();


                    var wdata1 = manager.GetWalletPointsDetails(CustomerId, take, skip);

                    WalletPointsSummary.CustomerWalletHistory = wdata1.CustomerWalletHistory;
                    //var daata = db.Database.SqlQuery<WalletPointsSummary>(query1).FirstOrDefault();
                    //WalletPointsSummary.UpcomingPoints = db.Database.SqlQuery<double?>(query2).FirstOrDefault();
                    //WalletPointsSummary.CustomerWalletHistory = db.Database.SqlQuery<GetCustomerWalletHistory>(query3).ToList();
                    //int count = db.Database.SqlQuery<int>(query4).FirstOrDefault();
                    //WalletPointsSummary.totalCount = count;
                    //WalletPointsSummary.TotalEarnPoint = daata.TotalEarnPoint;
                    //WalletPointsSummary.TransactionDate = daata.TransactionDate;
                    //WalletPointsSummary.TotalUsedPoints = 0;
                    //WalletPointsSummary.UpcomingPoints = WalletPointsSummary.UpcomingPoints == null ? 0 : WalletPointsSummary.UpcomingPoints;
                }
                else
                {
                    var take = 10;
                    var skip = page * 10;
                    CustomersManager manager = new CustomersManager();
                    var wdata1 = manager.GetWalletPointsDetails(CustomerId, take, skip);
                    WalletPointsSummary.CustomerWalletHistory = wdata1.CustomerWalletHistory;



                    //var query3 = "select  IsNull(ORDERID,0) ORDERID,IsNull(NewAddedWAmount,0) NewAddedWAmount,IsNull(NewOutWAmount,0)NewOutWAmount,TransactionDate,Through "
                    //                 + "from CustomerWalletHistories where CustomerId = " + CustomerId + "order by id "
                    //                 + "desc  offset " + skip + " rows fetch next " + take + " rows only";
                    //var query4 = "select  COUNT(Id) from CustomerWalletHistories where  CustomerId=" + CustomerId;

                    //WalletPointsSummary.CustomerWalletHistory = db.Database.SqlQuery<GetCustomerWalletHistory>(query3).ToList();
                    //int count = db.Database.SqlQuery<int>(query4).FirstOrDefault();
                    //WalletPointsSummary.totalCount = count;

                }
                //walletPointsDetails.TotalEarnPoint = db.Database.SqlQuery<double?>(query1).FirstOrDefault();






                return WalletPointsSummary;

            }

        }

        [Route("IsChequeAccepted")]
        [HttpGet]
        [AllowAnonymous]
        public CustomerChequeAccepted IsChequeAccepted(int CustomerId)
        {
            CustomerChequeAccepted CustomerChequeAccepted = new CustomerChequeAccepted();
            using (var db = new AuthContext())
            {
                var cust = db.Customers.Where(x => x.CustomerId == CustomerId).Select(x => new { x.IsChequeAccepted, x.ChequeLimit }).FirstOrDefault();
                CustomerChequeAccepted.ChequeLimit = cust.ChequeLimit;
                CustomerChequeAccepted.IsChequeAccepted = cust.IsChequeAccepted;
                if (cust.IsChequeAccepted)
                {
                    string query = "exec GetCutomerChequLimit " + CustomerChequeAccepted.ChequeLimit + "," + CustomerId;
                    var lstChequeStatus = db.Database.SqlQuery<double>(query).FirstOrDefault();
                    if (lstChequeStatus == 0)
                    {
                        CustomerChequeAccepted.IsChequeAccepted = false;
                        CustomerChequeAccepted.msg = "Your previous cheque was return or cheque limit exceed.";
                    }
                    else
                    {
                        CustomerChequeAccepted.ChequeLimit = lstChequeStatus;
                    }
                }
                else
                {
                    CustomerChequeAccepted.msg = "";
                }
            }
            return CustomerChequeAccepted;
        }
        [Route("getChequeallcustomer")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<List<getChequeallcustomer>> getChequeallcustomer(int WarehouseId)
        {
            using (var myContext = new AuthContext())
            {
                var warehouseid = new SqlParameter("@WarehouseId", WarehouseId);

                var result = myContext.Database.SqlQuery<getChequeallcustomer>("GetChequeLimitCustomer @WarehouseId", warehouseid).ToList();
                return result;
            }
        }

        [Route("updateChequeCustomer")]
        [HttpPost]
        [AllowAnonymous]
        public bool updateChequeCustomer(List<updateChequeallcustomer> updateChequeCustomer)
        {
            using (var myContext = new AuthContext())
            {

                if (myContext.Database.Connection.State != ConnectionState.Open)
                    myContext.Database.Connection.Open();

                var customer = new DataTable();
                customer.Columns.Add("CustomerId");
                customer.Columns.Add("IsChequeAccepted");
                customer.Columns.Add("ChequeLimit");
                foreach (var item in updateChequeCustomer)
                {
                    var dr = customer.NewRow();
                    dr["CustomerId"] = item.CustomerId;
                    dr["IsChequeAccepted"] = item.IsChequeAccepted;
                    dr["ChequeLimit"] = item.ChequeLimit;
                    customer.Rows.Add(dr);
                }


                var param = new SqlParameter("CustomerChequeLimit", customer);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.CustomerChequeLimit";
                var cmd = myContext.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[UpdateCustomerChequeLimit]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);

                // Run the sproc
                if (cmd.ExecuteNonQuery() > 0)
                    return true;
                else
                    return false;

            }

        }

        [Route("getChequeBounceDate")]
        [HttpGet]
        public dynamic getChequeBounceDate(int CustomerId)
        {
            using (var myContext = new AuthContext())
            {
                var customerId = new SqlParameter("@customerid", CustomerId);

                var result = myContext.Database.SqlQuery<ChequeBounseDate>("GetChequeBounseDate @customerid", customerId).FirstOrDefault();
                return result;
            }
        }
        [Route("GetCutomerChequLimit")]
        [HttpGet]
        public dynamic GetCutomerChequLimit(double ChequeLimit, int CustomerId)
        {
            using (var myContext = new AuthContext())
            {
                var chequeLimit = new SqlParameter("@chequeLimit", ChequeLimit);
                var customerId = new SqlParameter("@customerid", CustomerId);

                var results = myContext.Database.SqlQuery<double>("GetCutomerChequLimit @chequeLimit,@customerid", chequeLimit, customerId).FirstOrDefault();
                return results;
            }
        }
        [Route("GetChequeinoperation")]
        [HttpGet]
        public dynamic GetChequeinoperation(int CustomerId)
        {
            using (var myContext = new AuthContext())
            {

                var results = myContext.Database.SqlQuery<ChequeinOpreationDC>("Select a.ChequeNumber,a.ChequeAmt,a.ChequeDate,a.CreatedDate,a.Orderid,a.ChequeStatus,a.ChequeBankName from  ChequeCollections a with (nolock)  inner join OrderMasters b with(nolock) on a.Orderid = b.OrderId and  a.ChequeStatus = 1 and  b.CustomerId = " + CustomerId + "").ToList();
                return results;
            }
        }

        [Route("GetChequeinBank")]
        [HttpGet]
        public dynamic GetChequeinBank(int CustomerId)
        {
            using (var myContext = new AuthContext())
            {

                var results = myContext.Database.SqlQuery<ChequeinOpreationDC>("Select a.ChequeNumber,a.ChequeAmt,a.ChequeDate,a.CreatedDate,a.Orderid,a.ChequeStatus,a.ChequeBankName from  ChequeCollections a with (nolock)  inner join OrderMasters b with(nolock) on a.Orderid = b.OrderId and  a.ChequeStatus = 2 and  b.CustomerId = " + CustomerId + "").ToList();
                return results;
            }
        }
        [Route("HDFCPaymentUpdate")]
        [HttpPost]
        public bool HDFCPaymentUpdate(paymentupdatehdfc updatepayment)
        {
            using (var db = new AuthContext())
            {
                int orderstatus = db.Database.SqlQuery<int>("Select count(a.OrderId) from OrderDeliveryMasters a inner join DeliveryIssuances b on a.DeliveryIssuanceId=b.DeliveryIssuanceId where a.Status='Delivered' and (b.Status='Freezed' or b.Status='Payment Submitted') and a.orderid=" + updatepayment.Orderid).FirstOrDefault();
                if (orderstatus == 0)
                {
                    PaymentResponseRetailerApp paymentupdate = db.PaymentResponseRetailerAppDb.Where(x => x.OrderId == updatepayment.Orderid && x.status == "Success" && x.PaymentFrom == updatepayment.PaymentFrom && x.id == updatepayment.id).SingleOrDefault();


                    if (updatepayment.Type == "Cash")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "Cash";
                            update.OrderId = updatepayment.Orderid;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type == "hdfc")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "hdfc";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type == "UPI")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "UPI";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type == "ePaylater")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "ePaylater";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.GatewayOrderId = updatepayment.GatewayOrderId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type == "mPos")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "mPos";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type == "credit hdfc")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "credit hdfc";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type == "Razorpay QR")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "Razorpay QR";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type.ToLower() == "chqbook")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "chqbook";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type == "Failed")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            paymentupdate.statusDesc = "Manual Payment Change";
                            paymentupdate.UpdatedDate = DateTime.Now;
                            db.Entry(paymentupdate).State = EntityState.Modified;
                        }
                    }
                    else if (updatepayment.Type == "Gullak")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "Gullak";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    else if (updatepayment.Type == "DirectUdhar")
                    {
                        if (paymentupdate != null)
                        {
                            paymentupdate.status = "Failed";
                            db.Entry(paymentupdate).State = EntityState.Modified;

                            PaymentResponseRetailerApp update = new PaymentResponseRetailerApp();
                            update.amount = updatepayment.amount;
                            update.currencyCode = "INR";
                            update.status = "Success";
                            update.statusDesc = "Manual Payment Change";
                            update.PaymentFrom = "DirectUdhar";
                            update.OrderId = updatepayment.Orderid;
                            update.GatewayTransId = updatepayment.GatewayTransId;
                            update.CreatedDate = updatepayment.CreatedDate;
                            update.UpdatedDate = DateTime.Now;
                            update.IsOnline = true;
                            db.PaymentResponseRetailerAppDb.Add(update);

                        }
                    }
                    if (updatepayment.Type != "Cash")
                    {
                        if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                        {
                            if (db.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == updatepayment.Orderid && z.TransactionId == updatepayment.GatewayTransId) == null)
                            {
                                var CustomerId = db.DbOrderMaster.Where(z => z.OrderId == updatepayment.Orderid).FirstOrDefault().CustomerId;
                                OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                                Opdl.OrderId = updatepayment.Orderid;
                                Opdl.IsPaymentSuccess = true;
                                Opdl.IsLedgerAffected = "Yes";
                                Opdl.PaymentDate = DateTime.Now;
                                Opdl.TransactionId = updatepayment.GatewayTransId;
                                Opdl.IsActive = true;
                                Opdl.CustomerId = CustomerId;
                                db.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                            }
                        }
                    }

                    db.Commit();
                    return true;
                }
                else
                    return false;
            }

        }
        [Route("Searchdata")]
        [HttpGet]
        public dynamic Searchdata(int OrderId)
        {
            using (var db = new AuthContext())
            {
                var query = @"select p.id,o.OrderId,o.Skcode,o.WarehouseName,o.Status,
                                                                 p.PaymentFrom, p.amount, o.CreatedDate, d.Status as AssignmentStatus
                                                                 from OrderMasters o with(nolock) inner join PaymentResponseRetailerApps p with(nolock) on o.OrderId = p.OrderId
                                                                 left join OrderDispatchedMasters odm with(nolock) on o.orderid = odm.OrderId
                                                                 left join DeliveryIssuances d with(nolock) on odm.DeliveryIssuanceIdOrderDeliveryMaster = d.DeliveryIssuanceId
                                                                 where p.status = 'Success' and p.PaymentFrom != 'Cheque' and p.amount>0 and o.OrderId= " + OrderId + "";
                var data = db.Database.SqlQuery<SearchorderDC>(query).ToList();
                return data;
            }
        }

        [Route("GetCustomerPrimeMemberShip")]
        [HttpGet]
        public PrimeCustomerDc GetCustomerPrimeMemberShip(int customerId)
        {
            PrimeCustomerDc primeCustomerDc = new PrimeCustomerDc();
            using (var context = new AuthContext())
            {
                var primeCustomer = context.PrimeCustomers.FirstOrDefault(x => x.CustomerId == customerId);

                if (primeCustomer == null)
                {
                    var memberShip = context.MemberShips.FirstOrDefault(x => x.memberShipSequence > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    if (memberShip == null)
                    {
                        primeCustomerDc = new PrimeCustomerDc
                        {
                            PaymentFrom = "OffLine",
                            CustomerId = customerId,
                            IsPrimeCustomer = false,
                            MemberShipId = memberShip.Id,
                            MemberShipName = memberShip.MemberShipName,
                            FeeAmount = memberShip.Amount,
                            StartDate = new DateTime(1900, 1, 1),
                            EndDate = new DateTime(1900, 1, 1)
                        };
                    }
                }
                else
                {
                    if (primeCustomer.IsActive && (!primeCustomer.IsDeleted.HasValue || !primeCustomer.IsDeleted.Value))
                    {
                        primeCustomerDc.PaymentFrom = "OffLine";
                        primeCustomerDc.IsPrimeCustomer = primeCustomer.StartDate <= DateTime.Now && primeCustomer.EndDate >= DateTime.Now;
                        if (primeCustomerDc.IsPrimeCustomer)
                        {
                            var existsmemberShip = context.MemberShips.FirstOrDefault(x => x.Id == primeCustomer.MemberShipId);
                            primeCustomerDc.MemberShipId = existsmemberShip.Id;
                            primeCustomerDc.MemberShipName = existsmemberShip.MemberShipName;
                            primeCustomerDc.FeeAmount = existsmemberShip.Amount;
                        }
                        else
                        {
                            var memberShip = context.MemberShips.FirstOrDefault(x => x.memberShipSequence > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                            primeCustomerDc.MemberShipId = memberShip.Id;
                            primeCustomerDc.MemberShipName = memberShip.MemberShipName;
                            primeCustomerDc.FeeAmount = memberShip.Amount;
                        }
                        primeCustomerDc.StartDate = primeCustomer.StartDate;
                        primeCustomerDc.EndDate = primeCustomer.EndDate;
                        primeCustomerDc.CustomerId = primeCustomer.CustomerId;
                    }
                    else
                    {
                        var memberShip = context.MemberShips.FirstOrDefault(x => x.memberShipSequence > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                        primeCustomerDc = new PrimeCustomerDc
                        {
                            MemberShipId = memberShip.Id,
                            MemberShipName = memberShip.MemberShipName,
                            PaymentFrom = "OffLine",
                            CustomerId = customerId,
                            IsPrimeCustomer = false,
                            FeeAmount = memberShip.Amount,
                            StartDate = new DateTime(1900, 1, 1),
                            EndDate = new DateTime(1900, 1, 1)
                        };
                    }
                }

                primeCustomerDc.MinMemberShipDcs = context.MemberShips.Where(x => x.memberShipSequence > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).OrderBy(x => x.memberShipSequence).Select(x =>
                 new MinMemberShipDc
                 {
                     Id = x.Id,
                     MemberShipName = x.MemberShipName,
                     FeeAmount = x.Amount
                 }
                ).ToList();
            }
            return primeCustomerDc;
        }

        [Route("MakeCustomerPrimeMember")]
        [HttpPost]
        public async Task<bool> MakeCustomerPrimeMember(PrimeCustomerDc primeCustomerDc)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(120);
            Customer customer = new Customer();
            MemberShip memberShip = new MemberShip();
            PrimeCustomer PrimeCustomer = new PrimeCustomer();
            long id = 0;
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var context = new AuthContext())
                {
                    customer = context.Customers.Where(x => x.CustomerId == primeCustomerDc.CustomerId).FirstOrDefault();
                    PrimeCustomer = context.PrimeCustomers.Where(x => x.CustomerId == primeCustomerDc.CustomerId).Include(x => x.PrimeRegistrationDetails).FirstOrDefault();
                    var companydetails = context.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new
                    {
                        x.IsPrimeActive,
                    }).FirstOrDefault();
                    memberShip = context.MemberShips.Where(x => x.Id == primeCustomerDc.MemberShipId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).OrderBy(x => x.memberShipSequence).FirstOrDefault();

                    if (companydetails.IsPrimeActive && memberShip != null)
                    {
                        var primereg = new PrimeRegistrationDetail();
                        if (PrimeCustomer == null)
                        {

                            PrimeCustomer = new PrimeCustomer();
                            PrimeCustomer.StartDate = DateTime.Now;
                            PrimeCustomer.EndDate = DateTime.Now.AddDays(memberShip.MemberShipInDays);
                            PrimeCustomer.MemberShipId = memberShip.Id;
                            PrimeCustomer.IsActive = true;
                            PrimeCustomer.IsDeleted = false;
                            PrimeCustomer.CreatedBy = userid;
                            PrimeCustomer.CreatedDate = DateTime.Now;
                            PrimeCustomer.CustomerId = primeCustomerDc.CustomerId;
                            PrimeCustomer.PrimeRegistrationDetails = new List<PrimeRegistrationDetail>();
                            primereg.GatewayRequest = "";
                            primereg.GatewayResponse = "";
                            primereg.GatewayTransId = primeCustomerDc.GatewayTransId;
                            primereg.PaymentFrom = primeCustomerDc.PaymentFrom;
                            primereg.MemberShipId = memberShip.Id;
                            primereg.FeeAmount = primeCustomerDc.FeeAmount;
                            primereg.status = "Success";
                            primereg.CreatedBy = userid;
                            primereg.ModifiedDate = DateTime.Now;
                            primereg.ModifiedBy = userid;
                            primereg.IsActive = true;
                            primereg.IsDeleted = false;
                            primereg.CreatedDate = DateTime.Now;
                            PrimeCustomer.PrimeRegistrationDetails.Add(primereg);
                            context.PrimeCustomers.Add(PrimeCustomer);
                        }
                        else
                        {

                            PrimeCustomer.MemberShipId = memberShip.Id;
                            PrimeCustomer.StartDate = PrimeCustomer.EndDate > DateTime.Now ? PrimeCustomer.StartDate : DateTime.Now;
                            PrimeCustomer.EndDate = DateTime.Now.AddDays(memberShip.MemberShipInDays);
                            PrimeCustomer.ModifiedDate = DateTime.Now;
                            PrimeCustomer.ModifiedBy = userid;
                            PrimeCustomer.IsActive = true;
                            PrimeCustomer.IsDeleted = false;
                            primereg.GatewayRequest = "";
                            primereg.GatewayResponse = "";
                            primereg.GatewayTransId = primeCustomerDc.GatewayTransId;
                            primereg.PaymentFrom = primeCustomerDc.PaymentFrom;
                            primereg.MemberShipId = memberShip.Id;
                            primereg.FeeAmount = primeCustomerDc.FeeAmount;
                            primereg.status = "Success";
                            primereg.CreatedBy = userid;
                            primereg.IsActive = true;
                            primereg.IsDeleted = false;
                            primereg.ModifiedDate = DateTime.Now;
                            primereg.ModifiedBy = userid;
                            primereg.CreatedDate = DateTime.Now;
                            PrimeCustomer.PrimeRegistrationDetails.Add(primereg);
                            context.Entry(PrimeCustomer).State = EntityState.Modified;

                        }
                        if (context.Commit() > 0)
                        {
                            var walletAmount = Convert.ToInt32((memberShip.Amount * 10) / (memberShip.MemberShipInMonth * 3));
                            context.Database.ExecuteSqlCommand("exec InsertFaydaBucket " + primeCustomerDc.CustomerId + "," + walletAmount);

                            primereg.InvoiceNo = "";
                            context.Entry(primereg).State = EntityState.Modified;
                            context.Commit();
                            id = primereg.Id;
                            result = true;
                            dbContextTransaction.Complete();
                        }
                    }
                }
            }

            if (result)
            {
                try
                {
                    NewHelper.CustomerLedgerHelper customerLedgerHelper = new NewHelper.CustomerLedgerHelper();
                    customerLedgerHelper.FaydaLedgerEntry(primeCustomerDc.CustomerId, userid, Convert.ToInt32(id), primeCustomerDc.FeeAmount, DateTime.Now);
                }
                catch (Exception ex)
                {
                    logger.Error("Error during entry Fayda ledger entry for customerid:" + primeCustomerDc.CustomerId + " Error:" + ex.ToString());
                }
                if (customer != null)
                {
                    MongoDbHelper<ManualAutoNotification> AutoNotificationmongoDbHelper = new MongoDbHelper<ManualAutoNotification>();
                    MongoDbHelper<DefaultNotificationMessage> DefalutmongoDbHelper = new MongoDbHelper<DefaultNotificationMessage>();
                    string defaultmessage = DefalutmongoDbHelper.Select(x => x.NotificationMsgType == "FaydaMemberShip").FirstOrDefault().NotificationMsg;//   "Hi [CustomerName]. You Have Left Something in Your Cart, complete your Purchase with ShopKirana on Sigle Click.";

                    string message = "Dear " + customer.ShopName + ",\n Congratulation, You have successfully subscribed Fayda Club Membership for " + memberShip.MemberShipName + " , it's valid up to " + PrimeCustomer.EndDate.ToString("dd/MM/yyyy hh:mm tt") + " . ";

                    if (!string.IsNullOrEmpty(defaultmessage))
                    {
                        message = defaultmessage.Replace("[CustomerName]", customer.ShopName).Replace("[DefaultName]", AppConstants.MemberShipName).Replace("[MemberShipName]", memberShip.MemberShipName).Replace("[ValidTill]", PrimeCustomer.EndDate.ToString("dd/MM/yyyy hh:mm tt"));
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
                        //    CustId = primeCustomerDc.CustomerId,
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
                            notificationType = "Actionable",
                            body = message,
                            notificationId = 1,
                            notify_type = "prime",

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
                            string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                            var firebaseService = new FirebaseNotificationServiceHelper(Key);
                            var Result =await firebaseService.SendNotificationForApprovalAsync(customer.fcmId, data);
                            if (Result != null)
                            {
                                result = true;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                        catch (Exception asd)
                        {
                            logger.Error("Error during sent Join Membership  notification : " + asd.ToString());
                        }
                    }
                }
            }
            return result;
        }

        [Route("RevokMemberShip")]
        [HttpGet]
        public bool RevokMemberShip(int customerId)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                var primeCustomer = context.PrimeCustomers.FirstOrDefault(x => x.CustomerId == customerId);

                if (primeCustomer != null)
                {
                    primeCustomer.MemberShipId = 0;
                    primeCustomer.IsActive = false;
                    primeCustomer.ModifiedBy = userid;
                    primeCustomer.ModifiedDate = DateTime.Now;
                    context.Entry(primeCustomer).State = EntityState.Modified;
                    result = context.Commit() > 0;
                    if (result)
                    {
                        var customerMemberShipBuckets = context.CustomerMemberShipBuckets.Where(x => x.customerId == customerId && x.IsActive).ToList();
                        if (customerMemberShipBuckets != null && customerMemberShipBuckets.Any())
                        {
                            foreach (var item in customerMemberShipBuckets)
                            {
                                item.IsActive = false;
                                context.Entry(item).State = EntityState.Modified;
                            }
                            context.Commit();
                        }
                    }
                }
            }
            return result;
        }

        [Route("GetCustomerPrimeRegDetails")]
        [HttpGet]
        public HttpResponseMessage GetCustomerPrimeRegDetails(int customerId)
        {
            using (var context = new AuthContext())
            {
                // var primeCustomer = context.PrimeCustomers.Where(x => x.CustomerId == customerId).Include(x => x.PrimeRegistrationDetails).FirstOrDefault();
                var query = "select ms.MemberShipName,pRD.FeeAmount,pRD.CreatedDate,pRD.status from PrimeCustomers pc join PrimeRegistrationDetails pRD" +
                      " on pc.Id=pRD.PrimeCustomerId join MemberShips ms on ms.Id=pRD.MemberShipId" +
                      "  where pc.IsDeleted=0 and  pc.CustomerId=" + customerId + " order by pRD.CreatedDate desc";
                var result = context.Database.SqlQuery<PrimeRegistrationDetails>(query).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
        }
        [Route("AddMemberShip")]
        [HttpPost]
        public bool AddMemberShip(AddMemberShipDC addMemberShipDC)
        {
            bool result = false;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {

                var memberShip = context.MemberShips.Where(x => x.Id == addMemberShipDC.Id && x.IsDeleted == false).FirstOrDefault();

                if (memberShip == null)
                {
                    var memberShipSeq = context.MemberShips.Where(x => x.IsDeleted == false).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                    var MemberShip = new MemberShip();
                    MemberShip.memberShipSequence = (memberShipSeq != null ? memberShipSeq.memberShipSequence : 0) + 1;
                    MemberShip.IsActive = true;
                    MemberShip.IsDeleted = false;
                    MemberShip.CreatedBy = userid;
                    MemberShip.CreatedDate = DateTime.Now;
                    MemberShip.Amount = addMemberShipDC.Amount;
                    MemberShip.MemberShipName = addMemberShipDC.MemberShipName;
                    MemberShip.MemberShipHindiName = addMemberShipDC.MemberShipHindiName;
                    MemberShip.MemberShipInMonth = addMemberShipDC.MemberShipInMonth;
                    MemberShip.MemberShipDescription = addMemberShipDC.MemberShipDescription;
                    MemberShip.MemberShipHindiDescription = addMemberShipDC.MemberShipHindiDescription;
                    MemberShip.MemberShipLogo = addMemberShipDC.MemberShipLogo;
                    context.MemberShips.Add(MemberShip);
                }
                else
                {
                    memberShip.IsActive = addMemberShipDC.IsActive;
                    memberShip.ModifiedBy = userid;
                    memberShip.ModifiedDate = DateTime.Now;
                    memberShip.Amount = addMemberShipDC.Amount;
                    memberShip.MemberShipName = addMemberShipDC.MemberShipName;
                    memberShip.MemberShipHindiName = addMemberShipDC.MemberShipHindiName;
                    memberShip.MemberShipInMonth = addMemberShipDC.MemberShipInMonth;
                    memberShip.MemberShipDescription = addMemberShipDC.MemberShipDescription;
                    memberShip.MemberShipHindiDescription = addMemberShipDC.MemberShipHindiDescription;
                    memberShip.MemberShipLogo = addMemberShipDC.MemberShipLogo;
                    context.Entry(memberShip).State = EntityState.Modified;
                }
                result = context.Commit() > 0;
            }
            return result;
        }

        [Route("GetmemberShipList")]
        [HttpGet]
        public List<AddMemberShipDC> GetmemberShipList()
        {
            using (var context = new AuthContext())
            {
                // var memberShipDC = new MemberShiplistDC();
                var memberShiplist = context.MemberShips.Where(x => x.IsDeleted == false).
                   Select(x => new AddMemberShipDC
                   {
                       Id = x.Id,
                       MemberShipInMonth = x.MemberShipInMonth,
                       MemberShipName = x.MemberShipName,
                       MemberShipHindiName = x.MemberShipHindiName,
                       CreatedDate = x.CreatedDate,
                       Amount = x.Amount,
                       IsActive = x.IsActive,
                       MemberShipLogo = x.MemberShipLogo

                   }).ToList();

                return memberShiplist;
            }
        }

        [Route("GetmemberShipbyId")]
        [HttpGet]
        public AddMemberShipDC GetmemberShipList(int Id)
        {
            using (var context = new AuthContext())
            {
                // var memberShipDC = new MemberShiplistDC();
                var memberShiplist = context.MemberShips.Where(x => x.Id == Id).Select(x => new AddMemberShipDC
                {
                    Id = x.Id,
                    MemberShipInMonth = x.MemberShipInMonth,
                    MemberShipName = x.MemberShipName,
                    MemberShipHindiName = x.MemberShipHindiName,
                    CreatedDate = x.CreatedDate,
                    Amount = x.Amount,
                    IsActive = x.IsActive,
                    MemberShipLogo = x.MemberShipLogo,
                    MemberShipDescription = x.MemberShipDescription,
                    MemberShipHindiDescription = x.MemberShipHindiDescription

                }).FirstOrDefault();
                return memberShiplist;
            }
        }

        [Route("GetCustomerMembershiplist")]
        [HttpPost]
        public getCustomerMembershipDc GetCustomerMembershiplist(CustomerFilterDc customerFilterDc)
        {
            using (var context = new AuthContext())
            {
                getCustomerMembershipDc GetCustomerMembershipDc = new getCustomerMembershipDc();
                // var primeCustomer = context.PrimeCustomers.Where(x => x.CustomerId == customerId).Include(x => x.PrimeRegistrationDetails).FirstOrDefault();
                var query = "select c.Skcode,c.ShopName,c.Mobile,ms.MemberShipName,pc.StartDate,pc.EndDate,pc.IsActive from Customers as c join" +
                    " PrimeCustomers as pc on c.CustomerId = pc.CustomerId join MemberShips ms " +
                    "on ms.Id = pc.MemberShipId where c.Deleted=0 ";
                var Searchquery = "select COUNT(pc.CustomerId) from Customers as c join PrimeCustomers as pc " +
                    "on c.CustomerId = pc.CustomerId join MemberShips as ms on ms.Id = pc.MemberShipId where c.Deleted=0 ";

                string searchclause = "";
                if (customerFilterDc.Warehouseid > 0)
                {
                    searchclause += "and c.Warehouseid =" + customerFilterDc.Warehouseid;
                }
                if (customerFilterDc.IsActive == 1)
                {
                    searchclause += "and pc.IsActive= 1";
                }
                if (customerFilterDc.IsActive == 2)
                {
                    searchclause += "and pc.IsActive= 0";
                }
                if (!string.IsNullOrEmpty(searchclause))
                {
                    query += "  " + searchclause;
                    Searchquery += " " + searchclause;
                }
                query += "  order by pc.CreatedDate DESC offset " + customerFilterDc.Skip + " rows fetch next " + customerFilterDc.Take + "  rows only";
                var result = context.Database.SqlQuery<CustomerMembershiplist>(query).ToList();
                int total = context.Database.SqlQuery<int>(Searchquery).FirstOrDefault();
                GetCustomerMembershipDc.customerMembershiplist = result;
                GetCustomerMembershipDc.totalRecords = total;
                return GetCustomerMembershipDc;
            }
        }

        [Route("getOTPbyMobileNo")]
        [HttpGet]
        public HttpResponseMessage getOTPbyMobileNo(string Mobile)
        {
            MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
            var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == Mobile);
            var CustomerOTPs = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.CreatedDate)).FirstOrDefault();
            return Request.CreateResponse(HttpStatusCode.OK, CustomerOTPs);
        }

        [Route("getCustomerMobileNo")]
        [HttpGet]
        public HttpResponseMessage getMobileNos(int CustomerId, string mobile)
        {
            using (var context = new AuthContext())
            {
                var query = "select count(customerid) from Customers with(nolock) where mobile='" + mobile + "' and CustomerId !=" + CustomerId;
                var result = context.Database.SqlQuery<int>(query).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, result > 0);
            }
        }

        #region Generate FranchiseSk
        public static void GenerateFranchiseSK(Customer customer)
        {
            if (customer != null)
            {
                var tradeUrl = ConfigurationManager.AppSettings["FranchiseAPIurl"] + "api/FranchisePurchaseOrder/CreateWareouseNPeople";
                TextFileLogHelper.TraceLog(tradeUrl);
                using (var client = new HttpClient())
                {
                    var newJson = JsonConvert.SerializeObject(customer);
                    using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
                    {

                        //var requestContent = content.ReadAsStringAsync();//Request Content 
                        var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));
                        response.EnsureSuccessStatusCode();
                        string responseBody = response.Content.ReadAsStringAsync().Result;

                        //var responseContent = response.Content.ReadAsStringAsync();//Response  Content 
                        var result = JsonConvert.DeserializeObject<ResponseMetaData>(responseBody);
                        if (result.Status != "Error")
                        {
                            TextFileLogHelper.LogError("Error while Generate Franchise for SK : " + Environment.NewLine
                           + "Request : "
                           + Environment.NewLine
                           + newJson
                           + "Response : "
                           + Environment.NewLine
                           + JsonConvert.SerializeObject(responseBody)
                           + Environment.NewLine);
                            TextFileLogHelper.TraceLog(result.ErrorMessage);
                        }
                        else
                        {
                            TextFileLogHelper.LogError("Error while Generate Franchise for SK : " + Environment.NewLine
                         + "Request : "
                         + Environment.NewLine
                         + newJson
                         + "Response : "
                         + Environment.NewLine
                         + JsonConvert.SerializeObject(responseBody)
                         + Environment.NewLine);
                        }
                    }
                }
            }
        }
        #endregion
        [Route("GetVATM")]
        [HttpGet]
        public dynamic GetVATM(int CustId)
        {
            MongoDbHelper<VATMCustomers> mongoDbHelper = new MongoDbHelper<VATMCustomers>();
            {
                var vatmdata = mongoDbHelper.Select(x => x.CustomerId == CustId).FirstOrDefault();
                if (vatmdata != null)
                {
                    return vatmdata.Data;
                }
                else
                {
                    return null;
                }

            }
        }

        [Route("IsGstExist")]
        [HttpGet]
        public dynamic IsGstExist(string RefNo)
        {
            using (var context = new AuthContext())
            {
                var gstCheck = context.Customers.Where(x => x.RefNo == RefNo).FirstOrDefault();
                if (gstCheck != null)
                {
                    return gstCheck;
                }
                else
                {
                    return null;
                }

            }
        }
        [Route("IsLicenseNumberExist")]
        [HttpGet]
        public dynamic IsLicenseNumberExist(string LicenseNumber)
        {
            using (var context = new AuthContext())
            {
                var isLicenseNumberCheck = context.Customers.Where(x => x.LicenseNumber == LicenseNumber).FirstOrDefault();
                if (isLicenseNumberCheck != null)
                {
                    return isLicenseNumberCheck;
                }
                else
                {
                    return null;
                }

            }
        }


        #region DTO Class

        public class CustomerFilterDc
        {
            public int Warehouseid { get; set; }
            public int IsActive { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }
        }

        public class CustMobileDc
        {
            public string Mobile { get; set; }
        }
        public class getCustomerMembershipDc
        {
            public List<CustomerMembershiplist> customerMembershiplist { get; set; }

            public int totalRecords { get; set; }
        }
        public class PrimeCustomerDc
        {
            public int CustomerId { get; set; }
            public bool IsPrimeCustomer { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal FeeAmount { get; set; }
            public string GatewayTransId { get; set; }
            public string PaymentFrom { get; set; }
            public string MemberShipName { get; set; }
            public int MemberShipId { get; set; }

            public List<MinMemberShipDc> MinMemberShipDcs { get; set; }
        }

        public class MinMemberShipDc
        {
            public int Id { get; set; }
            public string MemberShipName { get; set; }
            public decimal FeeAmount { get; set; }
        }

        public class CustomerChequeAccepted
        {
            public bool IsChequeAccepted { get; set; }
            public string msg { get; set; }
            public double ChequeLimit { get; set; }
        }
        public class CustomerChequeLimit
        {
            public bool IsChequeAccepted { get; set; }
            public int CustomerId { get; set; }
            public double ChequeLimit { get; set; }
        }
        public class ChequeBounseDate
        {
            public DateTime BounseDate { get; set; }
            public decimal ChequeAmt { get; set; }
        }
        public class Chequelimit
        {
            public float ChequeLimit { get; set; }
        }
        public class paymentupdatehdfc
        {
            public int id { get; set; }
            public int Orderid { get; set; }
            public string GatewayTransId { get; set; }
            public double amount { get; set; }
            public string PaymentFrom { get; set; }
            public string Type { get; set; }
            public string GatewayOrderId { get; set; }
            public DateTime CreatedDate { get; set; }

        }

        public class PrimeRegistrationDetails
        {
            public string MemberShipName { get; set; }
            public decimal FeeAmount { get; set; }
            public string status { get; set; }
            public DateTime CreatedDate { get; set; }
        }
        public class CustomerMembershiplist
        {
            public string MemberShipName { get; set; }
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Mobile { get; set; }

            public bool IsActive { get; set; }
        }
        public class AddMemberShipDC
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }
            public string MemberShipName { get; set; }
            public string MemberShipHindiName { get; set; }
            public string MemberShipHindiDescription { get; set; }
            public int MemberShipInMonth { get; set; }
            public string MemberShipLogo { get; set; }
            public string MemberShipDescription { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedDate { get; set; }
        }


        public class SearchFilter
        {
            public string Cityid { get; set; }
            public string mobile { get; set; }
            public string skcode { get; set; }
            public DateTime datefrom { get; set; }
            public DateTime dateto { get; set; }
            public PagerDataUIViewModel pager { get; set; }
            public bool IsDistributor { get; set; }
            public string levelname { get; set; }

            public int CustomerAppType { get; set; }
            public string Warehouseid { get; set; }
            public string ClusterId { get; set; }

        }



        //public class WalletPointsDetails
        //{
        //    public double? TotalEarnPoint { get; set; }
        //    public double? TotalUsedPoints { get; set; }
        //    public double? UpcomingPoints { get; set; }
        //    public DateTime? TransactionDate { get; set; }
        //    public int? totalCount { get; set; }
        //    public string HowToUseWalletPoints { get; set; }
        //    public List<GetCustomerWalletHistory> CustomerWalletHistory { get; set; }
        //}

        //public class GetCustomerWalletHistory
        //{
        //    public int? OrderId { get; set; }
        //    public double? NewAddedWAmount { get; set; }
        //    public double? NewOutWAmount { get; set; }
        //    public DateTime? TransactionDate { get; set; }
        //    public string Through { get; set; }
        //}

        public class CustomerWithMissingCluster
        {
            public int customerId { get; set; }
            public double? lat { get; set; }
            public double? lg { get; set; }
            public int? ClusterId { get; set; }
            public string ShippingAddress { get; set; }
        }

        public class ClusterMin
        {
            public int ClusterId { get; set; }
            public string ClusterName { get; set; }
        }

        public class customerrating
        {
            public Customer customers { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class favourite
        {
            public List<favItem> items { get; set; }
        }
        public class favItem
        {
            public int ItemId { get; set; }
        }
        public class OTP
        {
            public string OtpNo { get; set; }
        }
        public class CustomerMappedCity
        {
            public int CityId { get; set; }
            public int CustomerId { get; set; }
            public string FcmId { get; set; }
        }
        public class custdata
        {
            public int CustomerId { get; set; }
            public int? CompanyId { get; set; }
            public string Skcode { get; set; }
            public int? Warehouseid { get; set; }
            public string Mobile { get; set; }
            public DataContracts.KPPApp.ApkNamePwdResponse RegisteredApk { get; set; }
            public bool IsCityVerified { get; set; }
        }
        public class responsemsg
        {

            public bool IsCityVerified { get; set; }
        }
        public class pwcdetail
        {
            public int CustomerId { get; set; }
            public string currentpassword { get; set; }
            public string newpassword { get; set; }
        }
        public class forgotpassword
        {
            public string MobileNumber { get; set; }
            public string NewPassword { get; set; }
        }
        public class resmsg
        {
            public Boolean Message { get; set; }
        }
        public class customerDetail
        {
            public Customer customers { get; set; }

            public RBLCustomerInformation RBLCustomerInformation { get; set; }


            public bool Status { get; set; }
            public string Message { get; set; }
            public string CriticalInfoMissingMsg { get; set; }
        }
        public class customerQuestionDTO
        {
            public List<CustomerFeedbackQuestion> Questiondata { get; set; }

            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class customerfeedbackDTO
        {
            public CustomerOrderFeedback Questionfeedbackdata { get; set; }

            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class CustNotiDetail
        {
            public PaggingDatas PaggingDatas { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }
        public class ShopimgUpdate
        {
            public int CustomerId { get; set; }
            public string Shopimage { get; set; }
        }

        public class CustDetailForMobile
        {
            public string SKCode { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }

        public class GlobalcustomerDetail
        {
            public List<SalespDTO> customers { get; set; }

            public bool Status { get; set; }
            public string Message { get; set; }
        }

        public class SaveAPKVersionDTO
        {
            public Customer chdata { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }

        public class CustomerPagerListDTO
        {
            public List<Customer> CustomerPagerList { get; set; }
            public int TotalRecords { get; set; }

        }

        public class PagerDataUIViewModelcustomer
        {
            public int First { get; set; }
            public int Last { get; set; }
            public String ColumnName { get; set; }
            public bool IsAscending { get; set; }
            public string Contains { get; set; }
            public string Data { get; set; }
        }

        public class CustomerFeedbackDTO
        {
            public string Skcode { get; set; }
            public string Name { get; set; }
            public string Mobile { get; set; }
            public int Rating { get; set; }
            public DateTime RatingDate { get; set; }
            public DateTime OrderDate { get; set; }
            public string Comments { get; set; }
            public int OrderId { get; set; }
            public string Question { get; set; }
            public string WarehouseName { get; set; }
            public string CityName { get; set; }
        }
        public class ChequeinOpreationDC
        {
            public string ChequeNumber { get; set; }
            public decimal ChequeAmt { get; set; }
            public DateTime CreatedDate { get; set; }
            public int Orderid { get; set; }
            public DateTime ChequeDate { get; set; }
            public int ChequeStatus { get; set; }
            public string ChequeBankName { get; set; }
        }
        public class SearchorderDC
        {
            public int id { get; set; }
            public int OrderId { get; set; }
            public string Skcode { get; set; }
            public string WarehouseName { get; set; }
            public string Status { get; set; }
            public string PaymentFrom { get; set; }
            public double amount { get; set; }
            public DateTime CreatedDate { get; set; }
            public string AssignmentStatus { get; set; }
        }
        #endregion


        [Route("BulkVerifyAndUpdateGST")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<custGstResponse>> GetCustomerGSTVerify()
        {
            List<custGstResponse> custGstRequests = new List<custGstResponse>();

            //var gst = new CustomerGst();

            var gstList = new List<CustomerGst>();
            ConcurrentBag<custGstResponse> bag = new ConcurrentBag<custGstResponse>();

            #region comment
            //if (HttpContext.Current.Request.Files.AllKeys.Any())
            //{
            //    System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];
            //    if (httpPostedFile != null)
            //    {
            //        byte[] buffer = new byte[httpPostedFile.ContentLength];

            //        using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
            //        {
            //            br.Read(buffer, 0, buffer.Length);
            //        }
            //        XSSFWorkbook hssfwb;
            //        using (MemoryStream memStream = new MemoryStream())
            //        {
            //            BinaryFormatter binForm = new BinaryFormatter();
            //            memStream.Write(buffer, 0, buffer.Length);
            //            memStream.Seek(0, SeekOrigin.Begin);
            //            hssfwb = new XSSFWorkbook(memStream);
            //            string sSheetName = hssfwb.GetSheetName(0);
            //            ISheet sheet = hssfwb.GetSheet(sSheetName);
            //            IRow rowData;
            //            ICell cellData = null;
            //            ItemSchemeExcelUploaderMaster ItemSchemeExcelUploaderMaster = new ItemSchemeExcelUploaderMaster();
            //            ItemSchemeExcelUploaderMaster.ItemSchemeExcelUploaderDetails = new List<ItemSchemeExcelUploaderDetail>();
            //            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
            //            {
            //                if (iRowIdx == 0)
            //                {
            //                    rowData = sheet.GetRow(iRowIdx);
            //                }
            //                else
            //                {
            //                    custGstResponse custGstRequest = new custGstResponse();
            //                    rowData = sheet.GetRow(iRowIdx);
            //                    cellData = rowData.GetCell(0);
            //                    rowData = sheet.GetRow(iRowIdx);
            //                    if (rowData != null && cellData != null)
            //                    {
            //                        string col = null;
            //                        cellData = rowData.GetCell(0);
            //                        col = cellData == null ? "" : cellData.ToString();
            //                        custGstRequest.Skcode = col.Trim();

            //                        col = string.Empty;
            //                        cellData = rowData.GetCell(1);
            //                        col = cellData == null ? "" : cellData.ToString();
            //                        custGstRequest.GSTNo = col.Trim();
            //                    }
            //                    custGstRequests.Add(custGstRequest);
            //                }
            //            }
            //        }

            //    }

            //}
            #endregion
            using (AuthContext db = new AuthContext())
            {

                List<Customer> customers = db.Customers.Where(x => !string.IsNullOrEmpty(x.RefNo) && x.RefNo.Length == 15 && string.IsNullOrEmpty(x.NameOnGST)).ToList();

                if (customers != null && customers.Any())
                {
                    custGstRequests = customers.Select(x => new custGstResponse
                    {
                        GSTNo = x.RefNo,
                        Skcode = x.Skcode,
                        Status = false,
                        message = ""
                    }).ToList();
                    //var skcodes = custGstRequests.Select(x => x.Skcode).Distinct().ToList();

                    //ParallelLoopResult parellelResult = Parallel.ForEach(customers, (x) =>
                    //{
                    //    string path = ConfigurationManager.AppSettings["GetCustomerGstUrl"];
                    //    path = path.Replace("[[GstNo]]", x.RefNo);
                    //    using (GenericRestHttpClient<CustomerGst, string> memberClient
                    //       = new GenericRestHttpClient<CustomerGst, string>(path,
                    //       string.Empty, null))
                    //    {

                    //        var custGstRequest = new custGstResponse
                    //        {
                    //            Skcode = x.Skcode,
                    //            GSTNo = x.RefNo,
                    //        };
                    //        try
                    //        {
                    //            custGstRequest.GstResponse = AsyncContext.Run(() => memberClient.GetAsync());
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            TextFileLogHelper.LogError("GST API error: " + ex.ToString());
                    //            custGstRequest.GstResponse = new CustomerGst { error = true };
                    //        }
                    //        bag.Add(custGstRequest);
                    //    }
                    //});

                    //if (parellelResult.IsCompleted)
                    //{
                    foreach (var item in custGstRequests)
                    {
                        var customer = customers.FirstOrDefault(x => x.Skcode == item.Skcode);
                        if (customer != null)
                        {
                            string path = ConfigurationManager.AppSettings["GetCustomerGstUrl"];
                            path = path.Replace("[[GstNo]]", customer.RefNo);
                            using (GenericRestHttpClient<CustomerGst, string> memberClient
                               = new GenericRestHttpClient<CustomerGst, string>(path,
                               string.Empty, null))
                            {

                                try
                                {
                                    item.GstResponse = AsyncContext.Run(() => memberClient.GetAsync());
                                }
                                catch (Exception ex)
                                {
                                    TextFileLogHelper.LogError("GST API error: " + ex.ToString());
                                    item.GstResponse = new CustomerGst { error = true };
                                }

                            }

                            if (item.GstResponse.error == false)
                            {
                                var gst = item.GstResponse;

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

                                CustomerGSTVerify cust = new CustomerGSTVerify()
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

                                if (cust.Active == "Active")
                                {
                                    //customer.RefNo = cust.RefNo;
                                    customer.BillingAddress = cust.ShippingAddress;
                                    customer.BillingCity = cust.City;
                                    customer.BillingState = cust.State;
                                    customer.BillingZipCode = cust.Zipcode;
                                    customer.NameOnGST = cust.Name;
                                    db.Entry(customer).State = EntityState.Modified;
                                    item.Status = true;
                                    item.message = "Customer GST Number Is Verify Successfully.";

                                }
                                else
                                {
                                    item.Status = true;
                                    item.message = "Customer GST Number Is " + cust.Active;
                                }

                                db.Commit();
                            }
                            else
                            {
                                item.Status = false;
                                item.message = "Customer GST Number not valid.";
                            }


                            db.Database.ExecuteSqlCommand("Exec UpdatePendingOrderGST " + customer.CustomerId);
                        }
                    }

                    //}

                }
            }

            return custGstRequests;
        }

        [Route("GetCustomerLtLgFromAddress")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> GetCustomerLtLgFromAddress(List<int> warehouseId)
        {
            List<CustomerLatLgAddress> customerLatLgAddresslst = new List<CustomerLatLgAddress>();
            List<Warehousedto> warehosues = new List<Warehousedto>();
            using (AuthContext db = new AuthContext())
            {
                customerLatLgAddresslst = db.Customers.Where(x => !x.Deleted && warehouseId.Contains(x.Warehouseid.Value)).Select(x => new CustomerLatLgAddress
                {
                    City = x.City,
                    ZipCode = x.ZipCode,
                    lat = x.lat,
                    lg = x.lg,
                    ShippingAddress = x.ShippingAddress,

                    SKCode = x.Skcode,
                    WarehouseId = x.Warehouseid
                }).ToList();

                warehosues = db.Warehouses.Where(x => warehouseId.Contains(x.WarehouseId)).Select(x => new Warehousedto { WarehouseId = x.WarehouseId, WarehouseName = x.WarehouseName }).ToList();
            }
            int i = 0;
            while (customerLatLgAddresslst.Count() > i)
            {
                System.Threading.Thread.Sleep(600);
                ParallelLoopResult parellelResult = Parallel.ForEach(customerLatLgAddresslst.Skip(i).Take(100), (customer) =>
                {
                    try
                    {
                        customer.WarehouseName = customer.WarehouseId.HasValue && warehosues.Any(x => x.WarehouseId == customer.WarehouseId) ? warehosues.FirstOrDefault(x => x.WarehouseId == customer.WarehouseId).WarehouseName : "";
                        if (!string.IsNullOrEmpty(customer.ShippingAddress))
                        {
                            string googleApiUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json?business_status=OPERATIONAL&fields=formatted_phone_number&query=" + customer.ShippingAddress + "&key=AIzaSyC9QVbsRyWGaEqMU3langey0mvjnuOHLD8&components=country:IN";
                            if (!string.IsNullOrEmpty(customer.City))
                                googleApiUrl += "|locality:" + customer.City;
                            if (!string.IsNullOrEmpty(customer.ZipCode))
                                googleApiUrl += "|postal_code:" + customer.ZipCode;
                            //AIzaSyC9QVbsRyWGaEqMU3langey0mvjnuOHLD8
                            var placeSearchResponses = GetPlaceAPIResponse(googleApiUrl);
                            if (placeSearchResponses != null && placeSearchResponses.status == "OK" && placeSearchResponses.results != null && placeSearchResponses.results.Any())
                            {
                                foreach (var place in placeSearchResponses.results)
                                {
                                    customer.googleAddress = place.formatted_address;
                                    customer.Addresslat = place.geometry.location.lat;
                                    customer.Addresslg = place.geometry.location.lng;
                                    break;
                                }
                                if (customer.Addresslat > 0 && customer.lat > 0 && customer.Addresslg > 0 && customer.lg > 0)
                                {
                                    var sourceGeoCordinates = new System.Device.Location.GeoCoordinate(customer.Addresslat, customer.Addresslg);
                                    var destination = new System.Device.Location.GeoCoordinate(customer.lat, customer.lg);
                                    customer.Distance = GeoHelper.AerialDistance(sourceGeoCordinates, destination);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                });
                if (parellelResult.IsCompleted)
                    i += 100;
            }
            if (customerLatLgAddresslst != null && customerLatLgAddresslst.Any())
            {
                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/Customer/");

                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(customerLatLgAddresslst);
                dt.TableName = customerLatLgAddresslst.Where(x => !string.IsNullOrEmpty(x.City)).FirstOrDefault().City;
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "CustomerAddressLt_Lg_" + dt.TableName + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
            }
            return true;
        }

        internal GoogleResponse.PlaceSearchResponse GetPlaceAPIResponse(string url)
        {
            using (var httpClient = new HttpClient())
            {
                using (HttpResponseMessage response = AsyncContext.Run(() => httpClient.GetAsync(url)))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = AsyncContext.Run(() => response.Content.ReadAsStringAsync());

                    var placeSearchResponses = JsonConvert.DeserializeObject<GoogleResponse.PlaceSearchResponse>(responseBody);

                    return placeSearchResponses;

                }

            }
        }

        [Route("CustomerDocumentStatus/{CustomerId}/{CustomerDocumentStatus}")]
        [HttpGet]
        public async Task<string> CustomerDocumentStatus(int CustomerId, int CustomerDocumentStatus)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string res = "";
            if (CustomerId > 0 && userid > 0)
            {
                using (var db = new AuthContext())
                {
                    var DisplayName = db.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Active == true && x.Deleted == false).DisplayName;
                    var customer = db.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                    if (customer != null && DisplayName != null)
                    {
                        customer.UpdatedDate = DateTime.Now;
                        customer.CustomerDocumentStatus = CustomerDocumentStatus;
                        customer.LastModifiedBy = DisplayName;
                        db.Entry(customer).State = EntityState.Modified;
                        if (db.Commit() > 0)
                        {
                            res = "Customer document status change succesfully";
                        }
                    }
                    else
                    {
                        res = "something went wrong";
                    }
                }
            }
            return res;
        }


        [Route("GetCODLimitCustomerList")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CODLimitCustomerRes> GetCODLimitCustomerList(CODLimitCustomerSearchDc obj)
        {
            CustomersManager manager = new CustomersManager();
            return await manager.GetCODLimitCustomerList(obj);
        }

        [Route("GetExportCODLimitCustomerList")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CODLimitCustomerRes> GetExportCODLimitCustomerList(CODLimitCustomerSearchDc obj)
        {
            CustomersManager manager = new CustomersManager();
            return await manager.GetExportCODLimitCustomerList(obj);
        }

        [Route("UpdateCustomersCODLimit")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> UpdateCustomersCODLimit(List<UpdateCustomersCODLimitDc> UpdateCustomersCODLimits)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            bool result = false;
            if (UpdateCustomersCODLimits != null && UpdateCustomersCODLimits.Any() && userid > 0)
            {
                var ids = UpdateCustomersCODLimits.Select(x => x.CustomerId).ToList();
                using (AuthContext context = new AuthContext())
                {
                    var custcodlimit = context.CODLimitCustomers.Where(x => ids.Contains(x.CustomerId)).ToList();
                    if (custcodlimit != null && custcodlimit.Any())
                    {
                        custcodlimit.ForEach(x =>
                        {
                            x.IsCustomCODLimit = UpdateCustomersCODLimits.FirstOrDefault(y => y.CustomerId == x.CustomerId).IsCustomCODLimit;
                            x.CODLimit = UpdateCustomersCODLimits.FirstOrDefault(y => y.CustomerId == x.CustomerId).CODLimit;
                            x.ModifiedDate = DateTime.Now;
                            x.ModifiedBy = userid;
                            context.Entry(x).State = EntityState.Modified;
                        });
                        result = context.Commit() > 0;
                    }
                    else
                    {
                        UpdateCustomersCODLimits.ForEach(x =>
                        {
                            context.CODLimitCustomers.Add(new CODLimitCustomer()
                            {
                                IsActive = true,
                                IsDeleted = false,
                                CreatedDate = DateTime.Now,
                                CreatedBy = userid,
                                CustomerId = x.CustomerId,
                                IsCustomCODLimit = x.IsCustomCODLimit,
                                CODLimit = x.CODLimit
                            });
                        });
                        result = context.Commit() > 0;

                    }
                }
            }
            return result;
        }

        [Route("GetCODLimitHistoryCustomerIdWise")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<CODLimitCustomerHistory> GetCODLimitHistoryCustomerIdWise(int CustomerId)
        {
            CustomersManager manager = new CustomersManager();
            return await manager.GetCODLimitHistoryCustomerIdWise(CustomerId);
        }


        [Route("UploadCustomerChannelTypeFile")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult UploadCustomerChannelTypeFile()
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromMinutes(10);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    List<UploadCustomerListDc> Lists = new List<UploadCustomerListDc>();
                    List<string> SkCodeList = new List<string>();
                    List<string> DuplicateSkcodes = new List<string>();
                    string Col0, col2, col3;
                    string MSG = "";
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        using (var db = new AuthContext())
                        {
                            string ext = Path.GetExtension(httpPostedFile.FileName);
                            if (ext == ".xlsx" || ext == ".xls")
                            {
                                byte[] buffer = new byte[httpPostedFile.ContentLength];
                                using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                                {
                                    br.Read(buffer, 0, buffer.Length);
                                }
                                XSSFWorkbook hssfwb;
                                using (MemoryStream memStream = new MemoryStream())
                                {
                                    BinaryFormatter binForm = new BinaryFormatter();
                                    memStream.Write(buffer, 0, buffer.Length);
                                    memStream.Seek(0, SeekOrigin.Begin);
                                    hssfwb = new XSSFWorkbook(memStream);
                                    string sSheetName = hssfwb.GetSheetName(0);
                                    ISheet sheet = hssfwb.GetSheet(sSheetName);
                                    ICell cellData = null;
                                    IRow rowData;
                                    int? SKCodeCellIndex = null;
                                    int? ChannelTypeCellIndex = null;
                                    int? StoreCellIndex = null;
                                    string SKCode, Mobile, ChannelType, Store;

                                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)
                                    {
                                        if (iRowIdx == 0)
                                        {
                                            rowData = sheet.GetRow(iRowIdx);
                                            if (rowData != null)
                                            {
                                                string strJSON = null;
                                                string field = string.Empty;
                                                field = rowData.GetCell(0).ToString();
                                                SKCodeCellIndex = rowData.Cells.Any(x => x.ToString().ToLower().Trim() == "SKCode".ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().ToLower().Trim() == "SKCode".ToLower()).ColumnIndex : (int?)null;
                                                if (!SKCodeCellIndex.HasValue)
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SKCode does not exist..try again");
                                                    return Created(strJSON, strJSON);
                                                }


                                                ChannelTypeCellIndex = rowData.Cells.Any(x => x.ToString().ToLower().Trim() == "ChannelType".ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().ToLower().Trim() == "ChannelType".ToLower()).ColumnIndex : (int?)null;

                                                if (!ChannelTypeCellIndex.HasValue)
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ChannelType does not exist..try again");
                                                    return Created(strJSON, strJSON);
                                                }

                                                StoreCellIndex = rowData.Cells.Any(x => x.ToString().ToLower().Trim() == "Store".ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().ToLower().Trim() == "Store".ToLower()).ColumnIndex : (int?)null;

                                                if (!StoreCellIndex.HasValue)
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Store does not exist..try again");
                                                    return Created(strJSON, strJSON);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            UploadCustomerListDc List = new UploadCustomerListDc();
                                            rowData = sheet.GetRow(iRowIdx);
                                            if (rowData != null && rowData.Cells.Count > 0)
                                            {
                                                rowData = sheet.GetRow(iRowIdx);
                                                cellData = rowData.GetCell(SKCodeCellIndex.Value);
                                                Col0 = cellData == null ? "" : cellData.ToString();
                                                SKCode = Col0;

                                                cellData = rowData.GetCell(ChannelTypeCellIndex.Value);
                                                col2 = cellData == null ? "" : cellData.ToString();
                                                ChannelType = col2;

                                                cellData = rowData.GetCell(StoreCellIndex.Value);
                                                col3 = cellData == null ? "" : cellData.ToString();
                                                Store = col3;
                                                if (SKCode == null || ChannelType == null || Store == null)
                                                {
                                                    return Created(" Please fill data in a row.", " Please fill data in a row.");
                                                }

                                                if (SKCode != null && SKCode != "")
                                                {
                                                    List.SKCode = SKCode;

                                                    var ExistSkcode = Lists.Where(x => x.SKCode == SKCode && x.Store == Store).FirstOrDefault();
                                                    if (ExistSkcode != null && ExistSkcode.ChannelType == ChannelType)
                                                    {
                                                        return Created("Duplicate data found, Please check file.", "Duplicate data found, Please check file.");
                                                    }
                                                    if (ExistSkcode != null && ExistSkcode.ChannelType != ChannelType)
                                                    {
                                                        return Created(" Only one channel can assigned in one Store.", "Only one channel can assigned in one Stored, (SKCODE: )" + ExistSkcode.SKCode);
                                                    }
                                                }
                                                if (ChannelType != null && ChannelType != "")
                                                {
                                                    List.ChannelType = ChannelType;
                                                }
                                                if (Store != null && ChannelType != "")
                                                {
                                                    List.Store = Store;
                                                }

                                            }
                                            Lists.Add(List);
                                        }
                                    }
                                    var ChanneltypesList = Lists.Select(x => x.ChannelType).Distinct().ToList();
                                    var SkcodeList = Lists.Select(x => x.SKCode).Distinct().ToList();
                                    var StoresList = Lists.Select(x => x.Store).Distinct().ToList();

                                    var StoreDetails = db.StoreDB.Where(x => StoresList.Contains(x.Name)).ToList();
                                    var CustomerDetails = db.Customers.Where(x => SkcodeList.Contains(x.Skcode)).ToList();
                                    var ChannelTypes = db.ChannelMasters.Where(x => ChanneltypesList.Contains(x.ChannelType)).ToList();

                                    if(CustomerDetails != null && CustomerDetails.Count() > 0)
                                    {
                                        var consumerSkcode = CustomerDetails.Where(x => x.CustomerType != null && x.CustomerType.Trim().ToLower() == "consumer").Select(x=>x.Skcode).Distinct().ToList();
                                        if (consumerSkcode != null && consumerSkcode.Count() > 0)
                                        {
                                            MSG = "Consumer type Customer "+string.Join(",", consumerSkcode); 
                                            return Created(MSG, MSG);
                                        }
                                    }

                                    //var CustIds = CustomerDetails.Select(x => x.CustomerId).ToList();
                                    // var CustomerChannelList = db.CustomerChannelMappings.Where(x => CustIds.Contains(x.CustomerId) && x.IsActive == true).ToList();

                                    int result = 0;
                                    if (Lists != null && Lists.Count() > 0)
                                    {
                                        foreach (var item in Lists)
                                        {
                                            var storeid = StoreDetails.Where(x => x.Name.ToLower() == item.Store.ToLower()).FirstOrDefault();
                                            var Customerid = CustomerDetails.Where(x => x.Skcode.ToLower() == item.SKCode.ToLower()).FirstOrDefault();
                                            var ChannelMasterId = ChannelTypes.Where(x => x.ChannelType.ToLower() == item.ChannelType.ToLower()).FirstOrDefault();
                                            //var ExistCustomerChannelMapping = CustomerChannelList.Where(x => x.StoreId == storeid.Id && x.CustomerId == Customerid.CustomerId).FirstOrDefault();

                                            //if (ExistCustomerChannelMapping != null)
                                            //{
                                            //    //ExistCustomerChannelMapping.ChannelMasterId = ChannelMasterId.ChannelMasterId;
                                            //    //ExistCustomerChannelMapping.ModifiedDate = DateTime.Now;
                                            //    //ExistCustomerChannelMapping.ModifiedBy = userid;
                                            //    //db.Entry(ExistCustomerChannelMapping).State = EntityState.Modified;

                                            //    var param1 = new SqlParameter("@Customerid", ChannelMasterId.ChannelMasterId);
                                            //    var param2 = new SqlParameter("@storeid", storeid.Id);
                                            //    var param3 = new SqlParameter("@ChannelMasterid", ChannelMasterId.ChannelMasterId);
                                            //    var abc = db.Database.SqlQuery<int>("Exec UpdateCustomerChannelMappingSp @Customerid,@storeid,@ChannelMasterid", param1, param2,param3).FirstOrDefault();
                                            //}
                                            //else
                                            //{
                                            //    CustomerChannelMapping obj = new CustomerChannelMapping();
                                            //    obj.StoreId = Convert.ToInt32(storeid.Id);
                                            //    obj.ChannelMasterId = ChannelMasterId.ChannelMasterId;
                                            //    obj.CustomerId = Customerid.CustomerId;
                                            //    obj.CreatedDate = DateTime.Now;
                                            //    obj.IsActive = true;
                                            //    obj.IsDeleted = false;
                                            //    db.CustomerChannelMappings.Add(obj);
                                            //}


                                            if (storeid != null && Customerid != null && ChannelMasterId != null)
                                            {
                                                var param1 = new SqlParameter("@Customerid", Customerid.CustomerId);
                                                var param2 = new SqlParameter("@storeid", storeid.Id);
                                                var param3 = new SqlParameter("@ChannelMasterid", ChannelMasterId.ChannelMasterId);
                                                result = db.Database.SqlQuery<int>("Exec UpdateCustomerChannelMappingSp @Customerid,@storeid,@ChannelMasterid", param1, param2, param3).FirstOrDefault();
                                            }
                                        }
                                    }
                                    if (result > 0)
                                    {
                                        scope.Complete();
                                        MSG = "File Saved Sucessfully"; return Created(MSG, MSG);
                                    }
                                    else
                                    {
                                        scope.Dispose();
                                        MSG = "Failed To Save"; return Created(MSG, MSG);
                                    }
                                }
                            }
                            else
                            {
                                return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                            }
                        }
                    }
                    return Created("Error", "Error");
                }
                return Created("Error", "Error");
            }

        }



        //[Route("UploadCustomerChannelTypeFile")]
        //[HttpPost]
        //[AllowAnonymous]
        //public IHttpActionResult UploadCustomerChannelTypeFile()
        //{
        //    int userid = 0;
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    if (HttpContext.Current.Request.Files.AllKeys.Any())
        //    {
        //        List<UploadCustomerListDc> Lists = new List<UploadCustomerListDc>();
        //        List<string> SkCodeList = new List<string>();
        //        List<string> DuplicateSkcodes = new List<string>();
        //        string Col0, col2, col3;
        //        string MSG = "";
        //        var httpPostedFile = HttpContext.Current.Request.Files["file"];
        //        if (httpPostedFile != null)
        //        {
        //            using (var db = new AuthContext())
        //            {
        //                string ext = Path.GetExtension(httpPostedFile.FileName);
        //                if (ext == ".xlsx" || ext == ".xls")
        //                {
        //                    byte[] buffer = new byte[httpPostedFile.ContentLength];
        //                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
        //                    {
        //                        br.Read(buffer, 0, buffer.Length);
        //                    }
        //                    XSSFWorkbook hssfwb;
        //                    using (MemoryStream memStream = new MemoryStream())
        //                    {
        //                        BinaryFormatter binForm = new BinaryFormatter();
        //                        memStream.Write(buffer, 0, buffer.Length);
        //                        memStream.Seek(0, SeekOrigin.Begin);
        //                        hssfwb = new XSSFWorkbook(memStream);
        //                        string sSheetName = hssfwb.GetSheetName(0);
        //                        ISheet sheet = hssfwb.GetSheet(sSheetName);
        //                        ICell cellData = null;
        //                        IRow rowData;
        //                        int? SKCodeCellIndex = null;
        //                        int? ChannelTypeCellIndex = null;
        //                        int? StoreCellIndex = null;
        //                        string SKCode, Mobile, ChannelType, Store;

        //                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)
        //                        {
        //                            if (iRowIdx == 0)
        //                            {
        //                                rowData = sheet.GetRow(iRowIdx);
        //                                if (rowData != null)
        //                                {
        //                                    string strJSON = null;
        //                                    string field = string.Empty;
        //                                    field = rowData.GetCell(0).ToString();
        //                                    SKCodeCellIndex = rowData.Cells.Any(x => x.ToString().ToLower().Trim() == "SKCode".ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().ToLower().Trim() == "SKCode".ToLower()).ColumnIndex : (int?)null;
        //                                    if (!SKCodeCellIndex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SKCode does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }


        //                                    ChannelTypeCellIndex = rowData.Cells.Any(x => x.ToString().ToLower().Trim() == "ChannelType".ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().ToLower().Trim() == "ChannelType".ToLower()).ColumnIndex : (int?)null;

        //                                    if (!ChannelTypeCellIndex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ChannelType does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }

        //                                    StoreCellIndex = rowData.Cells.Any(x => x.ToString().ToLower().Trim() == "Store".ToLower()) ? rowData.Cells.FirstOrDefault(x => x.ToString().ToLower().Trim() == "Store".ToLower()).ColumnIndex : (int?)null;

        //                                    if (!StoreCellIndex.HasValue)
        //                                    {
        //                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Store does not exist..try again");
        //                                        return Created(strJSON, strJSON);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                UploadCustomerListDc List = new UploadCustomerListDc();
        //                                rowData = sheet.GetRow(iRowIdx);
        //                                if (rowData != null && rowData.Cells.Count > 0)
        //                                {
        //                                    rowData = sheet.GetRow(iRowIdx);
        //                                    cellData = rowData.GetCell(SKCodeCellIndex.Value);
        //                                    Col0 = cellData == null ? "" : cellData.ToString();
        //                                    SKCode = Col0;

        //                                    cellData = rowData.GetCell(ChannelTypeCellIndex.Value);
        //                                    col2 = cellData == null ? "" : cellData.ToString();
        //                                    ChannelType = col2;

        //                                    cellData = rowData.GetCell(StoreCellIndex.Value);
        //                                    col3 = cellData == null ? "" : cellData.ToString();
        //                                    Store = col3;
        //                                    if (SKCode == null || ChannelType == null || Store == null)
        //                                    {
        //                                        return Created(" Please fill data in a row.", " Please fill data in a row.");
        //                                    }

        //                                    if (SKCode != null && SKCode != "")
        //                                    {
        //                                        List.SKCode = SKCode;

        //                                        var ExistSkcode = Lists.Where(x => x.SKCode == SKCode && x.Store == Store).FirstOrDefault();
        //                                        if (ExistSkcode != null && ExistSkcode.ChannelType == ChannelType)
        //                                        {
        //                                            return Created("Duplicate data found, Please check file.", "Duplicate data found, Please check file.");
        //                                        }
        //                                        if (ExistSkcode != null && ExistSkcode.ChannelType != ChannelType)
        //                                        {
        //                                            return Created(" Only one channel can assigned in one Store.", "Only one channel can assigned in one Stored, (SKCODE: )" + ExistSkcode.SKCode);
        //                                        }
        //                                    }
        //                                    if (ChannelType != null && ChannelType != "")
        //                                    {
        //                                        List.ChannelType = ChannelType;
        //                                    }
        //                                    if (Store != null && ChannelType != "")
        //                                    {
        //                                        List.Store = Store;
        //                                    }

        //                                }
        //                                Lists.Add(List);
        //                            }
        //                        }
        //                        var ChanneltypesList = Lists.Select(x => x.ChannelType).Distinct().ToList();
        //                        var SkcodeList = Lists.Select(x => x.SKCode).Distinct().ToList();
        //                        var StoresList = Lists.Select(x => x.Store).Distinct().ToList();

        //                        if (ChanneltypesList != null && SkcodeList != null && StoresList != null)
        //                        {
        //                            var ChannelTypess = new DataTable();
        //                            ChannelTypess = new DataTable();
        //                            ChannelTypess.Columns.Add("StringValue");

        //                            foreach (var item in ChanneltypesList)
        //                            {
        //                                var dr = ChannelTypess.NewRow();
        //                                dr["StringValue"] = item;
        //                                ChannelTypess.Rows.Add(dr);
        //                            }
        //                            var param = new SqlParameter("ChannelTypes", ChannelTypess);
        //                            param.SqlDbType = SqlDbType.Structured;
        //                            param.TypeName = "dbo.stringValues";


        //                            var SKCodes = new DataTable();
        //                            SKCodes = new DataTable();
        //                            SKCodes.Columns.Add("StringValue");

        //                            foreach (var item in SkcodeList)
        //                            {
        //                                var dr = SKCodes.NewRow();
        //                                dr["StringValue"] = item;
        //                                SKCodes.Rows.Add(dr);
        //                            }
        //                            var param2 = new SqlParameter("SKCodes", SKCodes);
        //                            param2.SqlDbType = SqlDbType.Structured;
        //                            param2.TypeName = "dbo.stringValues";

        //                            var Stores = new DataTable();
        //                            Stores = new DataTable();
        //                            Stores.Columns.Add("StringValue");
        //                            foreach (var item in StoresList)
        //                            {
        //                                var dr = Stores.NewRow();
        //                                dr["StringValue"] = item;
        //                                Stores.Rows.Add(dr);
        //                            }
        //                            var param3 = new SqlParameter("Stores", Stores);
        //                            param3.SqlDbType = SqlDbType.Structured;
        //                            param3.TypeName = "dbo.stringValues";


        //                            var StoreDetails = db.Database.SqlQuery<storelistt>("Exec  @Stores", param3).ToList();
        //                            var CustomerDetails = db.Database.SqlQuery<skcodeslistt>("Exec  @SKCodes", param2).ToList();
        //                            var ChannelTypes = db.Database.SqlQuery<chanellistt>("Exec  @ChannelTypes", param).ToList();

        //                            var CustIds = CustomerDetails.Select(x => x.CustomerId).ToList();
        //                            var CustomerChannel = new DataTable();
        //                            CustomerChannel = new DataTable();
        //                            CustomerChannel.Columns.Add("IntValue");
        //                            foreach (var item in CustIds)
        //                            {
        //                                var dr = CustomerChannel.NewRow();
        //                                dr["IntValue"] = item;
        //                                CustomerChannel.Rows.Add(dr);
        //                            }
        //                            var param4 = new SqlParameter("CustomerChannels", CustomerChannel);
        //                            param4.SqlDbType = SqlDbType.Structured;
        //                            param4.TypeName = "dbo.intValues";

        //                            var CustomerChannelList = db.Database.SqlQuery<CustomerChannelMapping>("Exec  @CustomerChannels", param4).ToList();

        //                            //var StoreDetails = db.StoreDB.Where(x => StoresList.Contains(x.Name)).ToList();
        //                            //var CustomerDetails = db.Customers.Where(x => SkcodeList.Contains(x.Skcode)).ToList();
        //                            //var ChannelTypes = db.ChannelMasters.Where(x => ChanneltypesList.Contains(x.ChannelType)).ToList();
        //                            //var CustomerChannelList = db.CustomerChannelMappings.Where(x => CustIds.Contains(x.CustomerId) && x.IsActive == true).ToList();

        //                            if (Lists != null && Lists.Count() > 0)
        //                            {
        //                                foreach (var item in Lists)
        //                                {
        //                                    var storeid = StoreDetails.Where(x => x.store.ToLower() == item.Store.ToLower()).FirstOrDefault();
        //                                    var Customerid = CustomerDetails.Where(x => x.SKcode.ToLower() == item.SKCode.ToLower()).FirstOrDefault();
        //                                    var ChannelMasterId = ChannelTypes.Where(x => x.Channel.ToLower() == item.ChannelType.ToLower()).FirstOrDefault();
        //                                    var ExistCustomerChannelMapping = CustomerChannelList.Where(x => x.StoreId == storeid.StoreId && x.CustomerId == Customerid.CustomerId).FirstOrDefault();

        //                                    if (ExistCustomerChannelMapping != null)
        //                                    {
        //                                        ExistCustomerChannelMapping.ChannelMasterId = ChannelMasterId.ChannelId;
        //                                        ExistCustomerChannelMapping.ModifiedDate = DateTime.Now;
        //                                        ExistCustomerChannelMapping.ModifiedBy = userid;
        //                                        db.Entry(ExistCustomerChannelMapping).State = EntityState.Modified;
        //                                    }
        //                                    else
        //                                    {
        //                                        CustomerChannelMapping obj = new CustomerChannelMapping();
        //                                        obj.StoreId = Convert.ToInt32(storeid.StoreId);
        //                                        obj.ChannelMasterId = ChannelMasterId.ChannelId;
        //                                        obj.CustomerId = Customerid.CustomerId;
        //                                        obj.CreatedDate = DateTime.Now;
        //                                        obj.IsActive = true;
        //                                        obj.IsDeleted = false;
        //                                        db.CustomerChannelMappings.Add(obj);
        //                                    }
        //                                }
        //                            }
        //                        }

        //                        if (db.Commit() > 0)
        //                        {
        //                            MSG = "File Saved Sucessfully"; return Created(MSG, MSG);
        //                        }
        //                        else { MSG = "Failed To Save"; return Created(MSG, MSG); }
        //                    }
        //                }
        //                else
        //                {
        //                    return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
        //                }
        //            }
        //        }
        //        return Created("Error", "Error");
        //    }
        //    return Created("Error", "Error");
        //}


        [Route("CustomerChannelTypeList")]
        [HttpGet]
        public List<ChannelMaster> CustomerChannelTypeList()
        {

            using (var db = new AuthContext())
            {
                List<ChannelMaster> list = new List<ChannelMaster>();
                list = db.ChannelMasters.Where(x => x.Active == true && x.Deleted == false).Distinct().ToList();
                return list;
            }
        }


        [Route("Add_Update_Delete_CustomerMapping")]
        [HttpPost]
        public APIResponse Insert(CustomerChannelDc obj)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            CustomerChannelMapping add = new CustomerChannelMapping();
            APIResponse Result = new APIResponse();
            using (var context = new AuthContext())
            {
                if (obj.SKCode != null && obj.storeid > 0 && obj.ChannelMasterId > 0)
                {
                    var Customer = context.CustomerChannelMappings.FirstOrDefault(x => x.CustomerId == obj.CustomerId && x.StoreId == obj.storeid && x.IsActive == true);

                    if (!obj.IsDeleted && Customer != null)
                    {
                        if (Customer.ChannelMasterId != obj.ChannelMasterId)
                        {
                            Customer.ChannelMasterId = obj.ChannelMasterId;
                            Customer.ModifiedBy = userid;
                            Customer.ModifiedDate = DateTime.Now;
                            context.Entry(Customer).State = EntityState.Modified;


                            Result.Message = "Updated Successfully";
                            Result.Status = true;
                            Result.Data = null;
                        }
                        else
                        {
                            Result.Message = "Already Saved.";
                            Result.Status = false;
                            Result.Data = null;
                        }
                    }
                    else if (obj.IsDeleted && Customer != null)
                    {
                        Customer.IsActive = false;
                        Customer.IsDeleted = true;
                        Customer.ModifiedBy = userid;
                        Customer.ModifiedDate = DateTime.Now;
                        context.Entry(Customer).State = EntityState.Modified;
                        Result.Message = "Deleted Successfully";
                        Result.Status = true;
                        Result.Data = null;
                    }
                    //else // Add
                    //{
                    //    add.StoreId = obj.storeid;
                    //    add.ChannelMasterId = obj.ChannelMasterId;
                    //    add.CreatedBy = userid;
                    //    add.CreatedDate = DateTime.Now;
                    //    add.IsActive = true;
                    //    add.IsDeleted = false;
                    //    context.CustomerChannelMappings.Add(add);

                    //    Result.Message = "Added Successfully";
                    //    Result.Status = true;
                    //    Result.Data = null;
                    //}
                    if (context.Commit() > 0)
                    {
                        return Result;
                    }
                }
                else
                {
                    Result.Message = "something went wrong";
                    Result.Status = false;
                    Result.Data = null;
                    return Result;
                }
                return Result;
            }

        }
        //[Route("Add_Update_Delete_CustomerMapping")]
        //[HttpPost]
        //public APIResponse Insert(CustomerChannelDc obj )
        //{
        //    int userid = 0;
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    CustomerChannelMapping add = new CustomerChannelMapping();
        //    APIResponse Result = new APIResponse();
        //    using (var context = new AuthContext())
        //    {
        //        if (obj.CustomerId > 0 && obj.SKCode != null && obj.storeid > 0)
        //        {
        //            if (obj.Id > 0)
        //            {
        //                var Customer = context.CustomerChannelMappings.FirstOrDefault(x => x.Id == obj.Id);

        //                if (!obj.IsDeleted && Customer != null)
        //                {
        //                    Customer.ChannelMasterId = obj.ChannelMasterId;
        //                    Customer.ModifiedBy = userid;
        //                    Customer.ModifiedDate = DateTime.Now;
        //                    context.Entry(Customer).State = EntityState.Modified;
        //                }
        //                else if (obj.IsDeleted && Customer != null)
        //                {
        //                    Customer.IsActive = false;
        //                    Customer.IsDeleted = true;
        //                    Customer.ModifiedBy = userid;
        //                    Customer.ModifiedDate = DateTime.Now;
        //                    context.Entry(Customer).State = EntityState.Modified;
        //                }
        //                else
        //                {
        //                    Result.Message = "Data Not Found";
        //                    Result.Status = false;
        //                    return Result;
        //                }
        //            }
        //            else if (obj.Id == 0) // Add
        //            {
        //                add.StoreId = obj.storeid;
        //                add.ChannelMasterId = obj.ChannelMasterId;
        //                add.CustomerId = obj.CustomerId;
        //                add.CreatedBy = userid;
        //                add.CreatedDate = DateTime.Now;
        //                add.IsActive = true;
        //                add.IsDeleted = false;
        //                context.CustomerChannelMappings.Add(add);
        //            }
        //            if (context.Commit() > 0)
        //            {
        //                Result.Message = obj.Id == 0 ? "Added Successfully" : obj.IsDeleted ? "Deleted Successfully" : "Updated Successfully";
        //                Result.Status = true;
        //            }
        //            else
        //            {
        //                Result.Message = "something went wrong";
        //                Result.Status = false;
        //            }
        //        }
        //        else
        //        {
        //            Result.Message = "something went wrong";
        //            Result.Status = false;
        //        }
        //        return Result;
        //    }

        //}

        [Route("GetSKCodeByStoreChannel")]
        [HttpGet]
        public List<ClusterStoreChannelSkCodeListDc> ClusterStoreChannelSkCodeList(string SkCode)
        {
            List<ClusterStoreChannelSkCodeListDc> list = new List<ClusterStoreChannelSkCodeListDc>();

            if (SkCode != null)
            {
                using (var db = new AuthContext())
                {
                    var param = new SqlParameter("@SKCode", SkCode);
                    list = db.Database.SqlQuery<ClusterStoreChannelSkCodeListDc>("Exec ClusterStoreChannelSkCodeList @SKCode", param).ToList();
                }
            }
            else
            {
                list = null;
            }
            return list;
        }


        [Route("ExportStoreChannelSkCodes")]
        [HttpPost]
        public List<ExportCustomerChannelDc> ExportStoreChannelSkCodes(ExportPayLoadCustomerChannelDc obj)
        {
            List<ExportCustomerChannelDc> list = new List<ExportCustomerChannelDc>();

            if (obj.Storeid > 0 && obj.ChannelMasterIds.Count() > 0)
            {
                using (var db = new AuthContext())
                {
                    var param = new SqlParameter("@Storeid", obj.Storeid);

                    var IdDt = new DataTable();
                    IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");

                    foreach (var item in obj.ChannelMasterIds)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param2 = new SqlParameter("@ChannelMasterId", IdDt);
                    param2.SqlDbType = SqlDbType.Structured;
                    param2.TypeName = "dbo.IntValues";

                    list = db.Database.SqlQuery<ExportCustomerChannelDc>("Exec ExportStoreChannelSkCodeList @Storeid,@ChannelMasterId", param, param2).ToList();
                }
            }
            else
            {
                list = null;
            }
            return list;
        }

        [Route("UploadTCSExemptionDocument")]
        [HttpPost]
        public IHttpActionResult UploadTCSExemptionDocument(int CustomerId)
        {
            string LogoUrl = "";
            logger.Info("start logo upload");
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/TCSExemptionDocument")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/TCSExemptionDocument"));

                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        var filename = httpPostedFile.FileName;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/TCSExemptionDocument"), filename);
                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/TCSExemptionDocument", LogoUrl);

                        using (var con = new AuthContext())
                        {
                            Customer c = con.Customers.Where(s => s.CustomerId == CustomerId).FirstOrDefault();
                            c.TCSExemptionDeclarationDOC = filename;
                            con.Customers.Attach(c);
                            con.Entry(c).State = System.Data.Entity.EntityState.Modified;
                            con.Commit();
                            return Created(c.TCSExemptionDeclarationDOC, c.TCSExemptionDeclarationDOC);
                        }
                    }
                    return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
                }
                return Created("Upload Unsuccessfull", "Upload Unsuccessfull");
            }
            catch (Exception ex)
            {
                logger.Error("Error in  logo upload " + ex.Message);
                logger.Info("End   logo upload: ");
                return InternalServerError(); ;
            }

        }

        [Route("VerifyCustomerPan")]
        [HttpGet]
        public async Task<PanResponse> KarzaPanProfile(string PanNo)
        {
            string paramstr = "{\"pan\":\"{#PanNo#}\",\"aadhaarLastFour\":\"\",\"dob\":\"\",\"name\":\"\",\"address\":\"\",\"getContactDetails\":\"N\",\"PANStatus\":\"N\",\"isSalaried\":\"N\",\"isDirector\":\"N\",\"isSoleProp\":\"N\",\"fathersName\":\"N\",\"consent\":\"Y\",\"clientData\":{\"caseId\":\"\"}}";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    PanResponse Result = new PanResponse();
                    // using System.Net;
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons
                    string SecretKey = ConfigurationManager.AppSettings["PanSecretKey"];
                    string URL = ConfigurationManager.AppSettings["PanURL"];
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), URL))
                    {
                        paramstr = paramstr.Replace("{#PanNo#}", PanNo);
                        request.Headers.TryAddWithoutValidation("x-karza-key", SecretKey);
                        request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                        request.Content = new StringContent(paramstr);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await httpClient.SendAsync(request);
                        string jsonString = string.Empty;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            var res = JsonConvert.DeserializeObject<KarzaPanProfileResponseDTO>(jsonString);
                            if (res != null && res.statusCode == 101)
                            {
                               
                                Result.IsSuccess =true;
                                Result.Name = res.result.name;
                                Result.PanNo = res.result.pan;
                                Result.DOB = res.result.dob;
                            }
                            else
                            {
                                Result.IsSuccess = false;
                                Result.Name = "";
                                Result.PanNo = " PAN not found.";
                                Result.DOB = "";
                            }
                        }
                    }
                    return Result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Route("GetCustomerMasterData")]
        [HttpGet]
        //public List<CustomerMasterDc> GetCustomerMasterData()
        //{

        //        using (var db = new AuthContext())
        //        {
        //            list = db.Database.SqlQuery<CustomerMasterDc>("Exec GetCustomerMasterData").ToList();
        //        }
            
        //    return list;
        //}

        public string GetCustomerMasterData()
        {
            List<CustomerMasterDc> list = new List<CustomerMasterDc>();

            string result = "";
           
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            var fileName = "CustomerMasterData.xlsx";
            string filePath = ExcelSavePath + fileName;
            var returnPath = string.Format("{0}://{1}{2}/{3}", HttpContext.Current.Request.UrlReferrer.Scheme
                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                            , HttpContext.Current.Request.Url.Port == 80 || HttpContext.Current.Request.Url.Port == 443 ? "" : ":" + HttpContext.Current.Request.Url.Port
                                                            , string.Format("ExcelGeneratePath/{0}", fileName));





            using (var db = new AuthContext())
            {
                //List<ExcelPRPayment> PRSts = db.Database.SqlQuery<ExcelPRPayment>(sqlquery).OrderByDescending(a => a.PurchaseOrderId).ToList();
                list = db.Database.SqlQuery<CustomerMasterDc>("Exec GetCustomerMasterData").ToList();

                var dataTables = new List<DataTable>();
                var customermasterData = ClassToDataTable.CreateDataTable(list);
                customermasterData.TableName = "list";
                dataTables.Add(customermasterData);
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    result = returnPath;
                }
            }
            return result;
        }

    }

    public class PanResponse
    {
        public bool IsSuccess { get; set; }
        public string Name { get; set; }
        public string PanNo { get; set; }
        public string DOB { get; set; }
    }


    public class KarzaPanProfileResponseDTO
    {
        public string requestId { get; set; }
        public ProfileResultDTO result { get; set; }
        public int statusCode { get; set; }
        public string message { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class ProfileResultDTO
    {
        public string pan { get; set; }
        public string name { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public bool aadhaarLinked { get; set; }
        public string fathersName { get; set; }
        public ProfileAddressDTO adddress { get; set; }
    }

    public class ProfileAddressDTO
    {
        public string buildingName { get; set; }
        public string locality { get; set; }
        public string streetName { get; set; }
        public string pinCode { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
    }

   
    

    public class UploadCustomerListDc
    {
        public string SKCode { get; set; }
        public string ChannelType { get; set; }
        public string Store { get; set; }
    }
    public class ClusterStoreChannelSkCodeListDc
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public string SKCode { get; set; }
        public long StoreId { get; set; }
        public string Store { get; set; }
        public string ClusterName { get; set; }
        public string ChannelType { get; set; }
    }

    public class CustomerChannelDc
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string SKCode { get; set; }
        public int storeid { get; set; }
        public long ChannelMasterId { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class ExportCustomerChannelDc
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public string Store { get; set; }
        public string ChannelType { get; set; }
        public string Cluster { get; set; }
        public string Warehouse { get; set; }
    }
    public class ExportPayLoadCustomerChannelDc
    {
        public int Storeid { get; set; }
        public List<int> ChannelMasterIds { get; set; }
    }

    public class CustomerLatLgAddress
    {
        public string SKCode { get; set; }
        public string WarehouseName { get; set; }
        public int? WarehouseId { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string ShippingAddress { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string googleAddress { get; set; }
        public double Addresslat { get; set; }
        public double Addresslg { get; set; }
        public double Distance { get; set; }
    }

    public class CustomerMasterDc
    {
        public bool IsKPP { get; set; } 
        public int RetailerId { get; set; }
        public string RetailersCode { get; set; } 
        public string LicenseNumber { get; set; } 
        public string CreatedBy { get; set; } 
        public string ShopName { get; set; } 
        public string DocType { get; set; } // cd.DocType
        public DateTime? ActivationDate { get; set; } 
        public string ActivationBy { get; set; } 
        public string RetailerName { get; set; } 
        public string City { get; set; } 
        public string Mobile { get; set; } 
        public bool IsFranchise { get; set; } 
        public DateTime? FranchiseApprovedDate { get; set; } 
        public string Warehouse { get; set; } 
        public int ExecutiveId { get; set; } 
        public string ExecutiveName { get; set; } 
        public string Emailid { get; set; } 
        public int? ClusterId { get; set; } 
        public string ClusterName { get; set; } 
        public double Latitude { get; set; } 
        public double Longitude { get; set; } 
        public string Description { get; set; } 
        public string CustomerVerify { get; set; } 
        public bool Active { get; set; } 
        public bool Deleted { get; set; }
        public string AgentCode { get; set; } 
        public DateTime CreationDate { get; set; } 
        public DateTime? UpdatedDate { get; set; } 
        public string LastModifiedBy { get; set; } 
        public string CurrentAPKversion { get; set; } 
        public string VerifiedBy { get; set; } 
        public DateTime? VerifiedDate { get; set; } 
        public string CustomerAppTypes { get; set; } 
        public string GSTNo { get; set; } 
        public string BillingAddress { get; set; } 
        public string BillingZipCode { get; set; } 
        public string BillingCity { get; set; } 
        public string ShippingAddress { get; set; } 
        public string ShippingPincode { get; set; } 
        public string ShippingCity { get; set; } 
        public string State { get; set; } 
        public string ChannelType { get; set; } 
    }

}

namespace GoogleResponse
{
    public class PlaceSearchResponse
    {
        public IList<object> html_attributions { get; set; }
        public string next_page_token { get; set; }
        public List<Result> results { get; set; }
        public Result result { get; set; }
        public string status { get; set; }
    }


    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Viewport
    {
        public Northeast northeast { get; set; }
        public Southwest southwest { get; set; }
    }

    public class Geometry
    {
        public Location location { get; set; }
        public Viewport viewport { get; set; }
    }

    public class OpeningHours
    {
        public bool open_now { get; set; }
    }

    public class PlusCode
    {
        public string compound_code { get; set; }
        public string global_code { get; set; }
    }

    public class Photo
    {
        public int height { get; set; }
        public IList<string> html_attributions { get; set; }
        public string photo_reference { get; set; }
        public int width { get; set; }
    }

    public class Result
    {
        public string business_status { get; set; }
        public string formatted_address { get; set; }
        public string formatted_phone_number { get; set; }
        public Geometry geometry { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public OpeningHours opening_hours { get; set; }
        public string place_id { get; set; }
        public PlusCode plus_code { get; set; }
        public double rating { get; set; }
        public string reference { get; set; }
        public IList<string> types { get; set; }
        public int user_ratings_total { get; set; }
        public IList<Photo> photos { get; set; }
        public List<AddressComponents> address_components { get; set; }
    }
    public class AddressComponents
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public string[] types { get; set; }

    }

}