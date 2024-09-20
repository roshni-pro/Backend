/// SPA sales person app
/// RA retailer app
/// SPA V2 sales person app 2nd version
/// RA V2 retailer app 2nd version

using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/HubPhase")]
    public class HubPhaseController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpPost]
        public HttpResponseMessage Get(HubPhaseDTO HubPhase)
        {
            logger.Info("start Get Customer: ");
            HubPhaseDTO dbReport = new HubPhaseDTO();
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                using (AuthContext authcontext = new AuthContext())
                {
                    var m = HubPhase.MonthYear.Month;
                    var y = HubPhase.MonthYear.Year;

                    //get all Customer  (Customer) 1
                    var customerList = authcontext.Customers.Where(x => x.Deleted == false && x.Warehouseid == HubPhase.WarehouseId).ToList();
                    //get total order  month 
                    var OrderList = authcontext.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == HubPhase.WarehouseId && x.CreatedDate.Month == m && x.CreatedDate.Year == y && x.Status != "Payment Pending" && x.Status != "Inactive" && x.Status != "Failed" && x.Status != "Dummy Order Cancelled").Include("orderDetails").ToList();


                    var OrderId = OrderList.Select(x => x.OrderId).ToList();
                    //get kisan kirana month details (OrderDetails) 2
                    var OrderdetailList = authcontext.DbOrderDetails.Where(x => x.Deleted == false && x.WarehouseId == HubPhase.WarehouseId
                               && x.CreatedDate.Month == m && x.CreatedDate.Year == y && OrderId.Contains(x.OrderId)).ToList();

                    // (itemMasters) 3
                    var itemmasterslist = new List<ItemMaster>();// authcontext.itemMasters.Where(x => x.WarehouseId == HubPhase.WarehouseId && x.Deleted == false).ToList();

                    // (DPurchaseOrderMaster) 4
                    var purchaseorderSupplierDistinct = authcontext.DPurchaseOrderMaster.Where(x => x.WarehouseId == HubPhase.WarehouseId && x.CreationDate.Month == m && x.CreationDate.Year == y).Select(x => x.SupplierId).Distinct().Count();


                    var OrderdetailKisankirana = OrderdetailList.Where(x => x.Deleted == false && (x.SubsubcategoryName.Trim().ToLower().Contains(("kisan Kirana").Trim().ToLower()))).ToList();
                    // Scale
                    dbReport.NumberofSignup = customerList.Where(x => x.CreatedDate.Month < (m + 1) && x.CreatedDate.Year < (y + 1)).Count(); //Number of Sign up Customers till date
                    dbReport.ActiveStores = OrderList.Where(x => x.Deleted == false && x.WarehouseId == HubPhase.WarehouseId && x.CreatedDate.Month == m && x.CreatedDate.Year == y).Select(x => x.CustomerId).Distinct().Count();

                    //Active Stores
                    dbReport.Monthsale = OrderList.Where(x => x.WarehouseId == HubPhase.WarehouseId && x.Deleted == false
                                         && x.CreatedDate.Month == m && x.CreatedDate.Year == y).Sum(a => a.GrossAmount + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0));//Sales (+ WalletUSedAmput+ BillDiscountAmount)
                    dbReport.Monthsale = dbReport.Monthsale / 100000;

                    //Product reliance:
                    dbReport.Monthorder = OrderList.Where(x => x.CreatedDate.Month == m && x.CreatedDate.Year == y).Count();//Current Monthly Orders
                    dbReport.freqOrder = (dbReport.Monthorder / dbReport.ActiveStores);//Freq of Orders
                    double AverageOrdervalue = Convert.ToDouble((dbReport.Monthsale * 100000) / dbReport.Monthorder);//Average Order value
                    dbReport.AverageOrdervalue = AverageOrdervalue;

                    dbReport.AverageLineItem = (OrderdetailList.Count() / dbReport.Monthorder);//Average line item
                    dbReport.onlineOrder = (OrderList.Where(x => x.CreatedDate.Month == m && x.CreatedDate.Year == y && x.OrderTakenSalesPersonId == 0).Count() / dbReport.Monthorder) * 100;   //Online order %

                    //Kisan kirana 
                    var KisanKiranaActive = OrderdetailKisankirana.Select(z => z.CustomerId).Distinct().Count(); // Kisan kirana Activation
                    dbReport.KisanKiranaActive = KisanKiranaActive;

                    dbReport.KisanKiranaSale = OrderdetailKisankirana.Sum(z => z.TotalAmt) / 100000;    //Kisan kirana Sale
                    dbReport.KisanKiranaOrder = OrderdetailKisankirana.Select(z => z.OrderId).Distinct().Count();

                    dbReport.KisanOrderFreq = dbReport.KisanKiranaOrder / dbReport.KisanKiranaActive;  // Kisan kirana Order freq

                    //Service KPIs:
                    dbReport.PerOfPostOrdCancel = OrderList.Count(x => x.Status == "Post Order Canceled") / dbReport.Monthorder;  //Post Order Cancellation %

                    //Sourcing :
                    dbReport.ActiveArticles = itemmasterslist.Where(x => x.active).Count();  //Active articles

                    dbReport.SoldArtciles = OrderdetailList.Select(x => x.ItemId).Distinct().Count();  //Sold artciles

                    dbReport.Numberofvendors = itemmasterslist.Where(x => x.SupplierId > 0).Select(x => x.SupplierId).Distinct().Count();  //Number of vendors

                    dbReport.ActiveVendors = purchaseorderSupplierDistinct;  //Active vendors

                    var avg = authcontext.AvgInventoryDb.Where(r => r.date.Month == m && r.date.Year == y && r.WarehouseId == HubPhase.WarehouseId).ToList();
                    if (avg != null)
                    {
                        dbReport.Inventorydays = Convert.ToInt32(avg.Sum(x => x.totals) / avg.Sum(x => x.totalSale));    //Inventory days
                    }


                    return Request.CreateResponse(HttpStatusCode.OK, dbReport);     // return dbReport;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error in Customer " + ex.Message);
                logger.Info("End  Customer: ");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Something went wrong");
            }
        }


        #region get market share percentage data
        /// <summary>
        /// Created by Vinayak pol
        /// Created date 19/07/2019
        /// </summary>
        /// <returns></returns>
      //  [ResponseType(typeof(HubPhase))]
        [Route("Getpercentage")]
        [AcceptVerbs("GET")]
        public List<HubPhasedata> Getdata(int Categoryid)
        {
            logger.Info("start get percentage: ");
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 1, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


                using (AuthContext db = new AuthContext())
                {
                    List<HubPhasedata> item = db.HubPhasedataDb.Where(x => x.Categoryid == Categoryid && x.Percentage != 0).ToList();
                    return item;
                }

            }
            catch (Exception ex)
            {
                logger.Error(" HubPhase " + ex.Message);
                return null;
            }
        }


        [Route("AddCategoryMarketShare")]
        [HttpPost]
        public HttpResponseMessage add(HubPhasedata item)
        {
            logger.Info("start HubPhase Data: ");

            var identity = User.Identity as ClaimsIdentity;
            int compid = 1, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

            using (AuthContext db = new AuthContext())
            {
                HubPhasedata HubRecord = db.HubPhasedataDb.FirstOrDefault(x => x.Categoryid == item.Categoryid && x.WarehouseId == item.WarehouseId);
                if (HubRecord == null && item.Categoryid > 0 && item.WarehouseId > 0)
                {
                    Category Category = db.Categorys.FirstOrDefault(x => x.Categoryid == item.Categoryid && x.Deleted == false);
                    if (Category != null)
                    {
                        item.CategoryName = Category.CategoryName;
                        db.HubPhasedataDb.Add(item);
                        db.Commit();
                        return Request.CreateResponse(HttpStatusCode.OK, true);
                    }
                }
                else if (item.Categoryid > 0 && item.WarehouseId > 0 && HubRecord != null)
                {
                    Category Category = db.Categorys.FirstOrDefault(x => x.Categoryid == item.Categoryid && x.Deleted == false);
                    if (Category != null)
                    {
                        HubRecord.Categoryid = Category.Categoryid;
                        HubRecord.CategoryName = Category.CategoryName;
                        HubRecord.TAM = item.TAM;
                        HubRecord.MS = item.MS;
                        HubRecord.Percentage = item.Percentage;
                        db.Entry(HubRecord).State = EntityState.Modified;
                        db.Commit();

                        return Request.CreateResponse(HttpStatusCode.OK, true);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, false);
            }
        }

        public class HubPhaseDTO
        {
            public int Cityid { get; set; }
            public int WarehouseId { get; set; }
            public double NumberofSignup { get; set; }
            public double ActiveStores { get; set; }
            public double? Monthsale { get; set; }
            public double TotalOrderTill { get; set; }
            public double Monthorder { get; set; }
            public double TotalSale { get; set; }
            public double freqOrder { get; set; }
            public double AverageOrdervalue { get; set; }
            public double ToatalAvgorder { get; set; }
            public double AverageLineItem { get; set; }
            public double onlineOrder { get; set; }
            public int KisanKiranaActive { get; set; }
            public double KisanKiranaSale { get; set; }
            public double KisanKiranaOrder { get; set; }
            public double KisanOrderFreq { get; set; }
            public double orderDetails { get; set; }
            public double PerOfPostOrdCancel { get; set; }


            public int ActiveArticles { get; set; }
            public int SoldArtciles { get; set; }

            public int Numberofvendors { get; set; }
            public int ActiveVendors { get; set; }

            public int Inventorydays { get; set; }

            public DateTime MonthYear { get; set; }

        }

    }
}
#endregion