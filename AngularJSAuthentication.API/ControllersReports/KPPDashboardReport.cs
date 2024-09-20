using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using LinqKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllersReports
{
    [Authorize]
    [RoutePrefix("api/KPP")]
    public class KPPDashboardReport : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [HttpGet]
        [Route("KPPDashboard")]
        public List<KPPDashboard> GetKPPDashboard()//List<int> warehouseids)
        {

            using (AuthContext db = new AuthContext())
            {
                var Field = new List<string>();
                Field.Add("New KPP (in Number)");
                Field.Add("Active KPP (in Number)");
                Field.Add("Sale");
                Field.Add("Kisan Kirana Sale");
                Field.Add("ShopKirana Sale");
                Field.Add("% of Kisan Kirana sale vs Shopkirana");
                Field.Add("Sale/Active Customer (in Number)");
                Field.Add("Order till date (in Number)");
                Field.Add("Order Delivered within 48Hr (in Number)");
                Field.Add("Order Sattled (in Number)");
                Field.Add("KPP > 2 order (in Number)");
                Field.Add("KPP > 2 order (in %)");

                var todaydate = indianTime;
                var yesterdaydate = todaydate.AddDays(-1);
                var lastmonthdate = todaydate.AddMonths(-1);

                DateTime now = DateTime.Now;
                var LMSD = new DateTime(now.Year, now.AddMonths(-1).Month, 1);
                var PMED = now.AddMonths(1).AddDays(-1);

                #region CalculationVariable
                double TSValueTT = 0;
                double TSValueLM = 0;
                double TSValueTM = 0;
                double TSValueY = 0;
                double TSValueTD = 0;

                double DOValueTT = 0;
                double DOValueLM = 0;
                double DOValueTM = 0;
                double DOValueY = 0;
                double DOValueTD = 0;

                double TONOTT = 0;
                double TONOLM = 0;
                double TONOTM = 0;
                double TONOY = 0;
                double TONOTD = 0;

                double DGNOTT = 0;
                double DGNOLM = 0;
                double DGNOTM = 0;
                double DGNOY = 0;
                double DGNOTD = 0;

                #endregion

                var kppcustomers = db.Customers.Where(x => x.IsKPP == true && x.Deleted == false).Select(x => new { CreatedDate = x.CreatedDate, Active = x.Active }).ToList();

                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted);
                var orderMasters = new List<MongoOrderMaster>();

                List<int> ids = db.Customers.Where(x => x.IsKPP == true && x.Active == true && x.Deleted == false).Select(x => x.CustomerId).ToList();
                orderPredicate.And(x => ids.Contains(x.CustomerId));
                orderPredicate.And(x => x.CreatedDate >= LMSD && x.CreatedDate <= PMED);

                orderMasters = mongoDbHelper.Select(orderPredicate, collectionName: "OrderMaster").ToList();

                List<KPPDashboard> listdsd = new List<KPPDashboard>();
                //if (warehouseids.Count > 0)
                //{

                foreach (var item in Field)
                {
                    if (item == "New KPP (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;



                        //var total = allcustomers.Count();
                        var lastmonth = kppcustomers.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                        var thismonth = kppcustomers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                        var Yesterday = kppcustomers.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                        var today = kppcustomers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Active KPP (in Number)")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        //var total = customers.Count();
                        var lastmonth = kppcustomers.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year && x.Active == true).Count();
                        var thismonth = kppcustomers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.Active == true).Count();
                        var Yesterday = kppcustomers.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day && x.Active == true).Count();
                        var today = kppcustomers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day && x.Active == true).Count();

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Sale")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        var sale = orderMasters.Select(x => new { CreatedDate = x.CreatedDate, TotalAmount = x.TotalAmount }).ToList();

                        var lastmonth = sale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Sum(x => x.TotalAmount);
                        var thismonth = sale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Sum(x => x.TotalAmount);
                        var Yesterday = sale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Sum(x => x.TotalAmount);
                        var today = sale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Sum(x => x.TotalAmount);

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "Kisan Kirana Sale")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        var kksale = db.DbOrderDetails.Where(x => x.Deleted == false && ids.Contains(x.OrderId) && x.CreatedDate.Date >= LMSD.Date && x.CreatedDate <= PMED.Date && (x.SubsubcategoryName == "Kisan Kirana" || x.SubsubcategoryName == "Kisan Kirana Jumbo")).Select(x => new { TotalAmt = x.TotalAmt, CreatedDate = x.CreatedDate }).ToList();

                        var lastmonth = kksale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Sum(x => x.TotalAmt);
                        var thismonth = kksale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Sum(x => x.TotalAmt);
                        var Yesterday = kksale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Sum(x => x.TotalAmt);
                        var today = kksale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Sum(x => x.TotalAmt);

                        //TONOTT = total;
                        TONOLM = lastmonth;
                        TONOTM = thismonth;
                        TONOY = Yesterday;
                        TONOTD = today;

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "ShopKirana Sale")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;

                        var sksale = db.DbOrderDetails.Where(x => x.Deleted == false && ids.Contains(x.OrderId) && x.CreatedDate.Date >= LMSD.Date && x.CreatedDate <= PMED.Date && x.SubsubcategoryName != "Kisan Kirana" && x.SubsubcategoryName != "Kisan Kirana Jumbo").Select(x => new { TotalAmt = x.TotalAmt, CreatedDate = x.CreatedDate }).ToList();

                        var lastmonth = sksale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Sum(x => x.TotalAmt);
                        var thismonth = sksale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Sum(x => x.TotalAmt);
                        var Yesterday = sksale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Sum(x => x.TotalAmt);
                        var today = sksale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Sum(x => x.TotalAmt);

                        //DGNOTT = total;
                        DGNOLM = lastmonth;
                        DGNOTM = thismonth;
                        DGNOY = Yesterday;
                        DGNOTD = today;

                        //dsd.Total = total;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    else if (item == "% of Kisan Kirana sale vs Shopkirana")
                    {
                        KPPDashboard dsd = new KPPDashboard();
                        dsd.FieldName = item;
                        double lastmonth = 0; try { lastmonth = TONOLM * 100 / DGNOLM; } catch (Exception) { lastmonth = 0; }
                        double thismonth = 0; try { thismonth = TONOTM * 100 / DGNOTM; } catch (Exception) { thismonth = 0; }
                        double Yesterday = 0; try { Yesterday = TONOLM * 100 / DGNOLM; } catch (Exception) { Yesterday = 0; }
                        double today = 0; try { today = TONOLM * 100 / DGNOLM; } catch (Exception) { today = 0; }

                        //TSValueTT = total ?? 0;
                        TSValueLM = lastmonth;
                        TSValueTM = thismonth;
                        TSValueY = Yesterday;
                        TSValueTD = today;

                        //dsd.Total = total ?? 0;
                        dsd.LastMonth = lastmonth;
                        dsd.ThisMonth = thismonth;
                        dsd.Yesterday = Yesterday;
                        dsd.Today = today;
                        listdsd.Add(dsd);
                    }
                    //else if (item == "Sale/Active Customer (in Number)")
                    //{
                    //    KPPDashboard dsd = new KPPDashboard();
                    //    dsd.FieldName = item;
                    //    //var total = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self").Select(o => (double?)o.TotalAmount).Sum();
                    //    var lastmonth = customer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Select(o => (double?)o.TotalAmount).Sum();
                    //    var thismonth = customer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Select(o => (double?)o.TotalAmount).Sum();
                    //    var Yesterday = customer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Select(o => (double?)o.TotalAmount).Sum();
                    //    var today = customer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Select(o => (double?)o.TotalAmount).Sum();

                    //    //DOValueTT = total ?? 0;
                    //    DOValueLM = lastmonth ?? 0;
                    //    DOValueTM = thismonth ?? 0;
                    //    DOValueY = Yesterday ?? 0;
                    //    DOValueTD = today ?? 0;

                    //    //dsd.Total = total ?? 0;
                    //    dsd.LastMonth = lastmonth ?? 0;
                    //    dsd.ThisMonth = thismonth ?? 0;
                    //    dsd.Yesterday = Yesterday ?? 0;
                    //    dsd.Today = today ?? 0;
                    //    listdsd.Add(dsd);
                    //}
                    //else if (item == "Order till date (in Number)")
                    //{
                    //    KPPDashboard dsd = new KPPDashboard();
                    //    dsd.FieldName = item;

                    //    //double total; try { total = (DOValueTT / TSValueTT) * 100; } catch (Exception) { total = 0; }
                    //    double lastmonth; try { lastmonth = (DOValueLM / TSValueLM) * 100; } catch (Exception) { lastmonth = 0; }
                    //    double thismonth; try { thismonth = (DOValueTM / TSValueTM) * 100; } catch (Exception) { thismonth = 0; }
                    //    double Yesterday; try { Yesterday = (DOValueY / TSValueY) * 100; } catch (Exception) { Yesterday = 0; }
                    //    double today; try { today = (DOValueTD / TSValueTD) * 100; } catch (Exception) { today = 0; }

                    //    //dsd.Total = total;
                    //    dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                    //    dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                    //    dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                    //    dsd.Today = double.IsNaN(today) ? 0 : today;
                    //    listdsd.Add(dsd);
                    //}
                    //else if (item == "Order Delivered within 48Hr (in Number)")
                    //{
                    //    KPPDashboard dsd = new KPPDashboard();
                    //    dsd.FieldName = item;

                    //    //double total = 0; try { total = (DGNOTT / TONOTT) * 100; } catch (Exception) { total = 0; }
                    //    double lastmonth = 0; try { lastmonth = (DGNOLM / TONOLM) * 100; } catch (Exception) { lastmonth = 0; }
                    //    double thismonth = 0; try { thismonth = (DGNOTM / TONOTM) * 100; } catch (Exception) { thismonth = 0; }
                    //    double Yesterday = 0; try { Yesterday = (DGNOY / TONOY) * 100; } catch (Exception) { Yesterday = 0; }
                    //    double today = 0; try { today = (DGNOTD / TONOTD) * 100; } catch (Exception) { today = 0; }

                    //    //dsd.Total = total;
                    //    dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                    //    dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                    //    dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                    //    dsd.Today = double.IsNaN(today) ? 0 : today;
                    //    listdsd.Add(dsd);
                    //}
                    //else if (item == "Order Sattled (in Number)")//Average Line Items (Online)
                    //{
                    //    KPPDashboard dsd = new KPPDashboard();
                    //    dsd.FieldName = item;

                    //    //var total = LineItemsOnline.Count() / DGNOTT;
                    //    var lastmonth = LineItemsOnline.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count() / DGNOLM;
                    //    var thismonth = LineItemsOnline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count() / DGNOTM;
                    //    var Yesterday = LineItemsOnline.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count() / DGNOY;
                    //    var today = LineItemsOnline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count() / DGNOTD;

                    //    //dsd.Total = total;
                    //    dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                    //    dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                    //    dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                    //    dsd.Today = double.IsNaN(today) ? 0 : today;
                    //    listdsd.Add(dsd);
                    //}
                    //else if (item == "KPP > 2 order (in Number)")
                    //{
                    //    KPPDashboard dsd = new KPPDashboard();
                    //    dsd.FieldName = item;

                    //    //var total = 0; try { total = LineItemsOffline.Count() / OfflineOrder.Count(); } catch (Exception) { total = 0; };
                    //    var lastmonth = 0; try { lastmonth = LineItemsOffline.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count(); } catch (Exception) { lastmonth = 0; };
                    //    var thismonth = 0; try { thismonth = LineItemsOffline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count(); } catch (Exception) { thismonth = 0; };
                    //    var Yesterday = 0; try { Yesterday = LineItemsOffline.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count(); } catch (Exception) { Yesterday = 0; };
                    //    var today = 0; try { today = LineItemsOffline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count(); } catch (Exception) { today = 0; };


                    //    //dsd.Total = total;
                    //    dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                    //    dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                    //    dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                    //    dsd.Today = double.IsNaN(today) ? 0 : today;
                    //    listdsd.Add(dsd);
                    //}
                    //else if (item == "KPP > 2 order (in %)")
                    //{
                    //    KPPDashboard dsd = new KPPDashboard();
                    //    dsd.FieldName = item;

                    //    //var total = 0; try { total = LineItemsOffline.Count() / OfflineOrder.Count(); } catch (Exception) { total = 0; };
                    //    var lastmonth = 0; try { lastmonth = LineItemsOffline.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count(); } catch (Exception) { lastmonth = 0; };
                    //    var thismonth = 0; try { thismonth = LineItemsOffline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count(); } catch (Exception) { thismonth = 0; };
                    //    var Yesterday = 0; try { Yesterday = LineItemsOffline.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count(); } catch (Exception) { Yesterday = 0; };
                    //    var today = 0; try { today = LineItemsOffline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count(); } catch (Exception) { today = 0; };


                    //    //dsd.Total = total;
                    //    dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                    //    dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                    //    dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                    //    dsd.Today = double.IsNaN(today) ? 0 : today;
                    //    listdsd.Add(dsd);
                    //}
                }
                //}
                return listdsd;
            }
        }
    }

    public class KPPDashboard
    {
        public string FieldName { get; set; }
        //public double Total { get; set; }
        public double LastMonth { get; set; }
        public double ThisMonth { get; set; }
        public double Yesterday { get; set; }
        public double Today { get; set; }
    }

    public class KPPReport
    {
        public string Region { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public string SkCode { get; set; }
        public string Status { get; set; }
        public string KPPName { get; set; }
        public string Town { get; set; }
        public string ContactNo { get; set; }
        public double KKLMTD { get; set; }
        public double KKMTD { get; set; }
        public double KKFTD { get; set; }
        public double SKLMTD { get; set; }
        public double SKMTD { get; set; }
        public double SKFTD { get; set; }
        public double TLMTD { get; set; }
        public double TMTD { get; set; }
        public double TFTD { get; set; }
    }

    public class CAD
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
        public string WorkingCity { get; set; }
        public string Agent { get; set; }
        public string Executive { get; set; }
        public string DeliveryBoy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CustomerCount { get; set; }
        public string CompletedCD { get; set; }
        public int ActiveCustomerCount { get; set; }
        public double ActiveInprogress { get; set; }

    }
}