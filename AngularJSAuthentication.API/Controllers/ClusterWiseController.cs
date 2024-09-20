using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ClusterWise")]
    public class ClusterWiseController : ApiController
    {
        #region Variable

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        #endregion

        #region Get Report ganerate data by ClusterId 
        /// <summary>
        /// date: 05/04/2019
        /// </summary>
        /// <param name="clstid"></param>
        /// <returns></returns>       
        [HttpGet]
        [Route("Get")]
        public clusterwisereport Get(int clstid, DateTime start, DateTime end)
        {
            using (AuthContext db = new AuthContext())
            {


                clusterwisereport objreport = new clusterwisereport();

                //get total data cust list
                List<Customer> Cust = db.Customers.Where(x => x.ClusterId == clstid).ToList();
                var warehouseid = Cust != null && Cust.Any() ? Cust.FirstOrDefault().Warehouseid : 0;
                //this for Total Retailer Count
                var Customer = Cust.Where(x => x.ClusterId == clstid).Count();
                var clusterwarehouse = db.Clusters.Where(x => x.ClusterId == clstid);
                var clustercustomerskcode = Cust.Select(x => x.Skcode);
                //this for Total Active Retailer Count
                //var Active = Cust.Where(x => x.Active == true && x.CreatedDate >= start && x.CreatedDate <= end).Count();
                var activeskcodes = Cust.Where(x => x.Active == true && x.CreatedDate >= start && x.CreatedDate <= end).Select(x => x.Skcode);
                //this for Total NewRetailers Count
                var Newretailers = Cust.Where(x => x.ClusterId == clstid && x.CreatedDate >= start && x.CreatedDate <= end).Count();

                List<OrderMaster> order = db.DbOrderMaster.Where(x => clustercustomerskcode.Contains(x.Skcode) && x.CreatedDate >= start && x.CreatedDate <= end).Include(x => x.orderDetails).ToList();


                //this for pre order
                var WarehouseTotalSale = order.Select(x => new { x.TotalAmount, x.Status, x.OrderId, x.Skcode }).ToList();
                var warehouseorderdetail = order.SelectMany(x => x.orderDetails.Select(y => new { y.OrderId, y.ItemId, y.SubsubcategoryName, y.OrderDetailsId, y.TotalAmt })).ToList();


                // this for Total Sales            
                var ClusterTotalSale = WarehouseTotalSale.Select(x => x.TotalAmount).Sum();
                //this for Net Deliverd Amount
                var ClusterNetDelivered = WarehouseTotalSale.Where(x => x.Status == "Account settled " || x.Status == "sattled" || x.Status == "Delivered").Select(x => x.TotalAmount).Sum();//order.Where(x => x.ClusterId == clstid && x.CreatedDate >= start && x.CreatedDate <= end && x.Status == "Account settled " || x.Status == "sattled" || x.Status == "Delivered").Count();

                // this for Avg/Retailer
                var ClusterAvgRetailer = WarehouseTotalSale.Select(x => x.Skcode).Distinct().Count(); //activeskcodes.Count();
                var Active = ClusterAvgRetailer;
                var activeretaileramt = WarehouseTotalSale.Select(x => x.TotalAmount).Sum();
                var AvgRetailer = ClusterAvgRetailer > 0 ? (Convert.ToDouble(activeretaileramt) / Convert.ToDouble(ClusterAvgRetailer)) : 0;
                var totalorder = WarehouseTotalSale.Count();
                //this for Cancellation
                // var CancelOrder = WarehouseTotalSale.Count();
                var TotalCancel = WarehouseTotalSale.Count();
                var TotalCancelOrder = WarehouseTotalSale.Where(x => x.Status == "Order Canceled").Count();
                var PostTotalCancelOrder = WarehouseTotalSale.Where(x => x.Status == "Post Order Canceled" || x.Status == "Delivery Canceled").Count();

                var TotalPreCancelOrderamt = WarehouseTotalSale.Where(x => x.Status == "Order Canceled").Select(x => x.TotalAmount).Sum();
                var TotalPostCancelOrderamt = WarehouseTotalSale.Where(x => x.Status == "Post Order Canceled" || x.Status == "Delivery Canceled").Select(x => x.TotalAmount).Sum();

                var PreCancelOrderTotal = TotalCancel > 0 ? ((Convert.ToDouble(TotalCancelOrder) * 100) / Convert.ToDouble(TotalCancel)) : 0;

                var PostCancelOrderTotal = TotalCancel > 0 ? ((Convert.ToDouble(PostTotalCancelOrder) * 100) / Convert.ToDouble(TotalCancel)) : 0;

                var CancelOrderTotal = totalorder > 0 ? (((Convert.ToDouble(TotalCancelOrder + PostTotalCancelOrder)) * 100) / Convert.ToDouble(totalorder)) : 0;

                // kisan kirana Total Sale

                double totamount = warehouseorderdetail.Where(x => x.SubsubcategoryName == "Kisan Kirana").Select(y => y.TotalAmt).Sum();

                var brandsale = warehouseorderdetail.Select(x => x.SubsubcategoryName).Distinct().Count();

                var TotalWarehouse = clusterwarehouse != null ? 1 : 0; //db.Warehouses.Where(x => x.Deleted == false).ToList().Count;
                var TotalCity = clusterwarehouse != null ? 1 : 0; //db.Cities.Where(x => x.Deleted == false).ToList().Count;
                var AvgBrand = totalorder > 0 ? Convert.ToDouble(brandsale) / totalorder : 0;
                var TotalActiveItem = warehouseorderdetail.Where(x => x.SubsubcategoryName == "Kisan Kirana").Select(y => y.ItemId).Distinct().Count(); //db.itemMasters.Where(x => x.SubsubcategoryName == "Kisan Kirana" && x.active == true && x.WarehouseId == warehouseid).Count();
                var AvgLineItem = totalorder > 0 ? Convert.ToDouble(warehouseorderdetail.Count()) / totalorder : 0;



                //this for return value
                objreport.TotalRetailer = Customer;//Total Retailer Count
                objreport.TotalRetailerActive = Active;// Active Retailer Count
                                                       //objreport.TotalActiveAgent = stringactiveagent + " (" + activeagent.ToString() + ")";
                objreport.TotalNewRetailers = Newretailers;//New Retailer Count
                objreport.TotalWarehouse = TotalWarehouse; // No.of Warehouse
                objreport.TotalCity = TotalCity; // No.of Warehouse
                objreport.Preorder = Math.Round(TotalPreCancelOrderamt, 2).ToString() + "      (" + Math.Round(PreCancelOrderTotal, 2) + " % )"; //pre order cencel parcentage 
                objreport.Postorder = Math.Round(TotalPostCancelOrderamt, 2).ToString() + "      (" + Math.Round(PostCancelOrderTotal, 2) + " % )";//Post order cencel parcentage 
                objreport.TotalAmountSale = Math.Round(ClusterTotalSale, 2);//Total Amount sale
                objreport.DeliveredAmount = ClusterNetDelivered;//DeliveredAmount
                objreport.AvgofRetailer = Math.Round(AvgRetailer, 2);//Avg of Retailer
                objreport.Cancellation = Math.Round((TotalPostCancelOrderamt + TotalPreCancelOrderamt), 2).ToString() + "      (" + Math.Round(CancelOrderTotal, 2) + " % )";//Cancel Order Total
                objreport.kisankiranaSale = Math.Round(totamount, 2);
                objreport.Avgofbrands = AvgBrand;// Avg Of Brand
                objreport.KisanKiranaActive = TotalActiveItem; //Total Active Item
                objreport.Avglineitem = Math.Round(AvgLineItem, 2);// Avg line of Item
                objreport.TotalOrderCount = totalorder;
                return objreport;
            }
        }
        #endregion

        #region DigitalSalesDashboard
        public List<string> Field { get; set; }

        /// <summary>
        /// Get Digital Sales data
        /// </summary>
        /// <param name="warehouseid"></param>
        /// <returns>List<DigitalSalesData></returns>
        [HttpGet]
        [Route("DigitalSales")]
        public List<DigitalSalesData> GetDigitalSales(int? warehouseid)
        {
            using (AuthContext db = new AuthContext())
            {
                Field = new List<string>();
                Field.Add("New Customer");
                //Field.Add("Active Customer");
                Field.Add("Number of Customer using Online Order");
                Field.Add("Total Number of Orders");
                Field.Add("Number of Online Order");
                Field.Add("Value of Total Sales");
                Field.Add("Value of Online Order");
                Field.Add("Digital Sale Percentage");
                Field.Add("Digital User percentage");
                Field.Add("Average Line Items (Online)");
                Field.Add("Average Line Items (Offline)");

                var todaydate = indianTime;
                var yesterdaydate = todaydate.AddDays(-1);
                var lastmonthdate = todaydate.AddMonths(-1);

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

                List<DigitalSalesData> listdsd = new List<DigitalSalesData>();
                if (warehouseid > 0)
                {
                    var wid = warehouseid > 0 ? warehouseid : 0;
                    //&& (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Partial settled" || x.Status == "Delivered")
                    var activecustomer = db.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == warehouseid).ToList();
                    var OfflineOrder = db.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == wid && x.OrderTakenSalesPerson != "Self").Select(x => new { x.CreatedDate }).ToList();

                    var LineItemsOffline = (from c in db.DbOrderMaster
                                            join p in db.DbOrderDetails
                                            on c.OrderId equals p.OrderId
                                            where c.OrderTakenSalesPerson != "Self" && c.WarehouseId == wid
                                            select new
                                            {
                                                CreatedDate = p.CreatedDate
                                            }).ToList();
                    var LineItemsOnline = (from c in db.DbOrderMaster
                                           join p in db.DbOrderDetails
                                           on c.OrderId equals p.OrderId
                                           where c.OrderTakenSalesPerson == "Self" && c.WarehouseId == wid
                                           select new
                                           {
                                               CreatedDate = p.CreatedDate
                                           }).ToList();

                    foreach (var item in Field)
                    {
                        if (item == "New Customer")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var allcustomers = (from j in db.Customers
                                                where j.Deleted == false && j.Warehouseid == warehouseid
                                                join i in db.Customers on j.CustomerId equals i.CustomerId
                                                group j by j.CustomerId into uniqueIds
                                                select uniqueIds).Select(x => x.FirstOrDefault()).ToList();

                            var total = allcustomers.Count();
                            var lastmonth = allcustomers.Where(x => x.Warehouseid == warehouseid && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = allcustomers.Where(x => x.Warehouseid == warehouseid && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = allcustomers.Where(x => x.Warehouseid == warehouseid && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = allcustomers.Where(x => x.Warehouseid == warehouseid && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Active Customer")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = db.Customers.Where(x => x.Warehouseid == warehouseid && x.Active == true).Count();
                            var lastmonth = db.Customers.Where(x => x.Warehouseid == warehouseid && x.Active == true && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = db.Customers.Where(x => x.Warehouseid == warehouseid && x.Active == true && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = db.Customers.Where(x => x.Warehouseid == warehouseid && x.Active == true && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = db.Customers.Where(x => x.Warehouseid == warehouseid && x.Active == true && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();
                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Number of Customer using Online Order")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var total = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self").GroupBy(x => x.CustomerId).Count();
                            var lastmonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).GroupBy(x => x.CustomerId).Count();
                            var thismonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).GroupBy(x => x.CustomerId).Count();
                            var Yesterday = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).GroupBy(x => x.CustomerId).Count();
                            var today = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).GroupBy(x => x.CustomerId).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Total Number of Orders")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = activecustomer.Count();
                            var lastmonth = activecustomer.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = activecustomer.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = activecustomer.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = activecustomer.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            TONOTT = total;
                            TONOLM = lastmonth;
                            TONOTM = thismonth;
                            TONOY = Yesterday;
                            TONOTD = today;

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Number of Online Order")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self").Count();
                            var lastmonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            DGNOTT = total;
                            DGNOLM = lastmonth;
                            DGNOTM = thismonth;
                            DGNOY = Yesterday;
                            DGNOTD = today;

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Value of Total Sales")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = activecustomer.Select(o => (double?)o.TotalAmount).Sum();
                            var lastmonth = activecustomer.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Select(o => (double?)o.TotalAmount).Sum();
                            var thismonth = activecustomer.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Select(o => (double?)o.TotalAmount).Sum();
                            var Yesterday = activecustomer.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Select(o => (double?)o.TotalAmount).Sum();
                            var today = activecustomer.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Select(o => (double?)o.TotalAmount).Sum();

                            TSValueTT = total ?? 0;
                            TSValueLM = lastmonth ?? 0;
                            TSValueTM = thismonth ?? 0;
                            TSValueY = Yesterday ?? 0;
                            TSValueTD = today ?? 0;

                            dsd.Total = total ?? 0;
                            dsd.LastMonth = lastmonth ?? 0;
                            dsd.ThisMonth = thismonth ?? 0;
                            dsd.Yesterday = Yesterday ?? 0;
                            dsd.Today = today ?? 0;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Value of Online Order")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self").Select(o => (double?)o.TotalAmount).Sum();
                            var lastmonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Select(o => (double?)o.TotalAmount).Sum();
                            var thismonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Select(o => (double?)o.TotalAmount).Sum();
                            var Yesterday = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Select(o => (double?)o.TotalAmount).Sum();
                            var today = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Select(o => (double?)o.TotalAmount).Sum();

                            DOValueTT = total ?? 0;
                            DOValueLM = lastmonth ?? 0;
                            DOValueTM = thismonth ?? 0;
                            DOValueY = Yesterday ?? 0;
                            DOValueTD = today ?? 0;

                            dsd.Total = total ?? 0;
                            dsd.LastMonth = lastmonth ?? 0;
                            dsd.ThisMonth = thismonth ?? 0;
                            dsd.Yesterday = Yesterday ?? 0;
                            dsd.Today = today ?? 0;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Digital Sale Percentage")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            double total; try { total = (DOValueTT / TSValueTT) * 100; } catch (Exception) { total = 0; }
                            double lastmonth; try { lastmonth = (DOValueLM / TSValueLM) * 100; } catch (Exception) { lastmonth = 0; }
                            double thismonth; try { thismonth = (DOValueTM / TSValueTM) * 100; } catch (Exception) { thismonth = 0; }
                            double Yesterday; try { Yesterday = (DOValueY / TSValueY) * 100; } catch (Exception) { Yesterday = 0; }
                            double today; try { today = (DOValueTD / TSValueTD) * 100; } catch (Exception) { today = 0; }

                            dsd.Total = total;
                            dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                            dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                            dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                            dsd.Today = double.IsNaN(today) ? 0 : today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Digital User percentage")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            double total = 0; try { total = (DGNOTT / TONOTT) * 100; } catch (Exception) { total = 0; }
                            double lastmonth = 0; try { lastmonth = (DGNOLM / TONOLM) * 100; } catch (Exception) { lastmonth = 0; }
                            double thismonth = 0; try { thismonth = (DGNOTM / TONOTM) * 100; } catch (Exception) { thismonth = 0; }
                            double Yesterday = 0; try { Yesterday = (DGNOY / TONOY) * 100; } catch (Exception) { Yesterday = 0; }
                            double today = 0; try { today = (DGNOTD / TONOTD) * 100; } catch (Exception) { today = 0; }

                            dsd.Total = total;
                            dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                            dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                            dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                            dsd.Today = double.IsNaN(today) ? 0 : today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Average Line Items (Online)")//Average Line Items (Online)
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var total = LineItemsOnline.Count() / DGNOTT;
                            var lastmonth = LineItemsOnline.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count() / DGNOLM;
                            var thismonth = LineItemsOnline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count() / DGNOTM;
                            var Yesterday = LineItemsOnline.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count() / DGNOY;
                            var today = LineItemsOnline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count() / DGNOTD;

                            dsd.Total = total;
                            dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                            dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                            dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                            dsd.Today = double.IsNaN(today) ? 0 : today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Average Line Items (Offline)")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var total = 0; try { total = LineItemsOffline.Count() / OfflineOrder.Count(); } catch (Exception) { total = 0; };
                            var lastmonth = 0; try { lastmonth = LineItemsOffline.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count(); } catch (Exception) { lastmonth = 0; };
                            var thismonth = 0; try { thismonth = LineItemsOffline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count(); } catch (Exception) { thismonth = 0; };
                            var Yesterday = 0; try { Yesterday = LineItemsOffline.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count(); } catch (Exception) { Yesterday = 0; };
                            var today = 0; try { today = LineItemsOffline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count(); } catch (Exception) { today = 0; };


                            dsd.Total = total;
                            dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                            dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                            dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                            dsd.Today = double.IsNaN(today) ? 0 : today;
                            listdsd.Add(dsd);
                        }
                    }
                }
                return listdsd;
            }
        }


        [HttpPost]
        [Route("DigitalSalesAdvance")]
        public List<DigitalSalesData> GetDigitalSalesAdvance(List<int> warehouseids)
        {
            using (AuthContext db = new AuthContext())
            {
                Field = new List<string>();
                Field.Add("New Customer");
                //Field.Add("Active Customer");
                Field.Add("Number of Customer using Online Order");
                Field.Add("Total Number of Orders");
                Field.Add("Number of Online Order");
                Field.Add("Value of Total Sales");
                Field.Add("Value of Online Order");
                Field.Add("Digital Sale Percentage");
                Field.Add("Digital User percentage");
                Field.Add("Average Line Items (Online)");
                Field.Add("Average Line Items (Offline)");

                var todaydate = indianTime;
                var yesterdaydate = todaydate.AddDays(-1);
                var lastmonthdate = todaydate.AddMonths(-1);

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

                List<DigitalSalesData> listdsd = new List<DigitalSalesData>();
                if (warehouseids.Count > 0)
                {
                    //var wid = warehouseid > 0 ? warehouseid : 0;
                    //&& (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Partial settled" || x.Status == "Delivered")
                    var activecustomer = db.DbOrderMaster.Where(x => x.Deleted == false && warehouseids.Contains(x.WarehouseId)).Select(x => new { CreatedDate = x.CreatedDate, TotalAmount = (x.TotalAmount-x.TCSAmount), OrderTakenSalesPerson = x.OrderTakenSalesPerson, CustomerId = x.CustomerId }).ToList();
                    var OfflineOrder = db.DbOrderMaster.Where(x => x.Deleted == false && warehouseids.Contains(x.WarehouseId) && x.OrderTakenSalesPerson != "Self").Select(x => new { x.CreatedDate }).ToList();

                    var LineItemsOffline = (from c in db.DbOrderMaster
                                            join p in db.DbOrderDetails
                                            on c.OrderId equals p.OrderId
                                            where c.OrderTakenSalesPerson != "Self" && warehouseids.Contains(c.WarehouseId)
                                            select new
                                            {
                                                CreatedDate = p.CreatedDate
                                            }).ToList();
                    var LineItemsOnline = (from c in db.DbOrderMaster
                                           join p in db.DbOrderDetails
                                           on c.OrderId equals p.OrderId
                                           where c.OrderTakenSalesPerson == "Self" && warehouseids.Contains(c.WarehouseId)
                                           select new
                                           {
                                               CreatedDate = p.CreatedDate
                                           }).ToList();

                    foreach (var item in Field)
                    {
                        if (item == "New Customer")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var allcustomers = (from j in db.Customers
                                                where j.Deleted == false && warehouseids.Contains(j.Warehouseid.Value)
                                                join i in db.Customers on j.CustomerId equals i.CustomerId
                                                group j by j.CustomerId into uniqueIds
                                                select uniqueIds).Select(x => x.FirstOrDefault()).ToList();

                            var total = allcustomers.Count();
                            var lastmonth = allcustomers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = allcustomers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = allcustomers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = allcustomers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Active Customer")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var customers = db.Customers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.Active == true).Select(x => new { CreatedDate = x.CreatedDate, CustomerId = x.CustomerId }).ToList();
                            //var total = db.Customers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.Active == true).Count();
                            //var lastmonth = db.Customers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.Active == true && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            //var thismonth = db.Customers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.Active == true && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            //var Yesterday = db.Customers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.Active == true && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            //var today = db.Customers.Where(x => warehouseids.Contains(x.Warehouseid.Value) && x.Active == true && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            var total = customers.Count();
                            var lastmonth = customers.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = customers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = customers.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = customers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Number of Customer using Online Order")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var total = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self").GroupBy(x => x.CustomerId).Count();
                            var lastmonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).GroupBy(x => x.CustomerId).Count();
                            var thismonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).GroupBy(x => x.CustomerId).Count();
                            var Yesterday = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).GroupBy(x => x.CustomerId).Count();
                            var today = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).GroupBy(x => x.CustomerId).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Total Number of Orders")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = activecustomer.Count();
                            var lastmonth = activecustomer.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = activecustomer.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = activecustomer.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = activecustomer.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            TONOTT = total;
                            TONOLM = lastmonth;
                            TONOTM = thismonth;
                            TONOY = Yesterday;
                            TONOTD = today;

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Number of Online Order")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self").Count();
                            var lastmonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            DGNOTT = total;
                            DGNOLM = lastmonth;
                            DGNOTM = thismonth;
                            DGNOY = Yesterday;
                            DGNOTD = today;

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Value of Total Sales")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = activecustomer.Select(o => (double?)o.TotalAmount).Sum();
                            var lastmonth = activecustomer.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Select(o => (double?)o.TotalAmount).Sum();
                            var thismonth = activecustomer.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Select(o => (double?)o.TotalAmount).Sum();
                            var Yesterday = activecustomer.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Select(o => (double?)o.TotalAmount).Sum();
                            var today = activecustomer.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Select(o => (double?)o.TotalAmount).Sum();

                            TSValueTT = total ?? 0;
                            TSValueLM = lastmonth ?? 0;
                            TSValueTM = thismonth ?? 0;
                            TSValueY = Yesterday ?? 0;
                            TSValueTD = today ?? 0;

                            dsd.Total = total ?? 0;
                            dsd.LastMonth = lastmonth ?? 0;
                            dsd.ThisMonth = thismonth ?? 0;
                            dsd.Yesterday = Yesterday ?? 0;
                            dsd.Today = today ?? 0;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Value of Online Order")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var total = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self").Select(o => (double?)o.TotalAmount).Sum();
                            var lastmonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Select(o => (double?)o.TotalAmount).Sum();
                            var thismonth = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Select(o => (double?)o.TotalAmount).Sum();
                            var Yesterday = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Select(o => (double?)o.TotalAmount).Sum();
                            var today = activecustomer.Where(x => x.OrderTakenSalesPerson == "Self" && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Select(o => (double?)o.TotalAmount).Sum();

                            DOValueTT = total ?? 0;
                            DOValueLM = lastmonth ?? 0;
                            DOValueTM = thismonth ?? 0;
                            DOValueY = Yesterday ?? 0;
                            DOValueTD = today ?? 0;

                            dsd.Total = total ?? 0;
                            dsd.LastMonth = lastmonth ?? 0;
                            dsd.ThisMonth = thismonth ?? 0;
                            dsd.Yesterday = Yesterday ?? 0;
                            dsd.Today = today ?? 0;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Digital Sale Percentage")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            double total; try { total = (DOValueTT / TSValueTT) * 100; } catch (Exception) { total = 0; }
                            double lastmonth; try { lastmonth = (DOValueLM / TSValueLM) * 100; } catch (Exception) { lastmonth = 0; }
                            double thismonth; try { thismonth = (DOValueTM / TSValueTM) * 100; } catch (Exception) { thismonth = 0; }
                            double Yesterday; try { Yesterday = (DOValueY / TSValueY) * 100; } catch (Exception) { Yesterday = 0; }
                            double today; try { today = (DOValueTD / TSValueTD) * 100; } catch (Exception) { today = 0; }

                            dsd.Total = total;
                            dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                            dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                            dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                            dsd.Today = double.IsNaN(today) ? 0 : today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Digital User percentage")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            double total = 0; try { total = (DGNOTT / TONOTT) * 100; } catch (Exception) { total = 0; }
                            double lastmonth = 0; try { lastmonth = (DGNOLM / TONOLM) * 100; } catch (Exception) { lastmonth = 0; }
                            double thismonth = 0; try { thismonth = (DGNOTM / TONOTM) * 100; } catch (Exception) { thismonth = 0; }
                            double Yesterday = 0; try { Yesterday = (DGNOY / TONOY) * 100; } catch (Exception) { Yesterday = 0; }
                            double today = 0; try { today = (DGNOTD / TONOTD) * 100; } catch (Exception) { today = 0; }

                            dsd.Total = total;
                            dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                            dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                            dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                            dsd.Today = double.IsNaN(today) ? 0 : today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Average Line Items (Online)")//Average Line Items (Online)
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var total = LineItemsOnline.Count() / DGNOTT;
                            var lastmonth = LineItemsOnline.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count() / DGNOLM;
                            var thismonth = LineItemsOnline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count() / DGNOTM;
                            var Yesterday = LineItemsOnline.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count() / DGNOY;
                            var today = LineItemsOnline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count() / DGNOTD;

                            dsd.Total = total;
                            dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                            dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                            dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                            dsd.Today = double.IsNaN(today) ? 0 : today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Average Line Items (Offline)")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var total = 0; try { total = LineItemsOffline.Count() / OfflineOrder.Count(); } catch (Exception) { total = 0; };
                            var lastmonth = 0; try { lastmonth = LineItemsOffline.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count(); } catch (Exception) { lastmonth = 0; };
                            var thismonth = 0; try { thismonth = LineItemsOffline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count(); } catch (Exception) { thismonth = 0; };
                            var Yesterday = 0; try { Yesterday = LineItemsOffline.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count(); } catch (Exception) { Yesterday = 0; };
                            var today = 0; try { today = LineItemsOffline.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count() / OfflineOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count(); } catch (Exception) { today = 0; };


                            dsd.Total = total;
                            dsd.LastMonth = double.IsNaN(lastmonth) ? 0 : lastmonth;
                            dsd.ThisMonth = double.IsNaN(thismonth) ? 0 : thismonth;
                            dsd.Yesterday = double.IsNaN(Yesterday) ? 0 : Yesterday;
                            dsd.Today = double.IsNaN(today) ? 0 : today;
                            listdsd.Add(dsd);
                        }
                    }
                }
                return listdsd;
            }
        }

        #endregion

        #region PNLReport
        /// <summary>
        /// Get PNL Report
        /// </summary>
        /// <param name="warehouseid"></param>
        /// <returns>PNLData</returns>
        /// <returns>PNLData</returns>
        [HttpGet]
        [Route("PNLReport")]
        public List<DigitalSalesData> GetPNLReport(int? warehouseid, int Type)
        {
            using (AuthContext db = new AuthContext())
            {
                var todaydate = indianTime;
                var yesterdaydate = todaydate.AddDays(-1);
                var lastmonthdate = todaydate.AddMonths(-1);
                List<DigitalSalesData> listdsd = new List<DigitalSalesData>();

                List<Customer> Cust = db.Customers.Where(x => x.Warehouseid == warehouseid).ToList();

                if (Type == 0)
                {
                    Field = new List<string>();
                    Field.Add("No of Cities");
                    Field.Add("No of Active Clusters");
                    Field.Add("Total Retailers");
                    Field.Add("Active Retailers");
                    Field.Add("No of Hubs");
                    Field.Add("Active Agents");
                    Field.Add("New Retailers");

                    foreach (var item in Field)
                    {
                        if (item == "No of Cities")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var TotalCity = db.Cities.Where(x => x.Deleted == false).ToList();

                            var total = TotalCity.Count();
                            var lastmonth = TotalCity.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = TotalCity.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = TotalCity.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = TotalCity.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "No of Active Clusters")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var Active = Cust.Where(x => x.Active == true).ToList();

                            var total = Active.Count();
                            var lastmonth = Active.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = Active.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = Active.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = Active.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "Total Retailers")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var TotalRetailers = Cust.ToList();

                            var total = TotalRetailers.Count();
                            var lastmonth = TotalRetailers.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = TotalRetailers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = TotalRetailers.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = TotalRetailers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "Active Retailers")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var Active = Cust.Where(x => x.Active == true).ToList();

                            var total = Active.Count();
                            var lastmonth = Active.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = Active.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = Active.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = Active.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "No of Hubs")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var TotalWarehouse = db.Warehouses.Where(x => x.Deleted == false).ToList();

                            var total = TotalWarehouse.Count();
                            var lastmonth = TotalWarehouse.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = TotalWarehouse.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = TotalWarehouse.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = TotalWarehouse.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Active Agents")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var agents = db.Peoples.Where(x => x.Salesexecutivetype == "Salesman" && x.WarehouseId == warehouseid && x.Active == true).ToList();
                            var total = agents.Count();
                            var lastmonth = agents.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = agents.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = agents.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = agents.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "New Retailers")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var Newretailers = Cust.ToList();

                            var total = Newretailers.Count();
                            var lastmonth = Newretailers.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = Newretailers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = Newretailers.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = Newretailers.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                    }
                }
                else if (Type == 1)
                {
                    Field = new List<string>();
                    Field.Add("Sale");
                    Field.Add("Pre-Order Cancel%");
                    Field.Add("Post-Order Cancel%");
                    Field.Add("Active Retailers");
                    Field.Add("Average Line Items");
                    //Field.Add("Average No of Brands");
                    //Field.Add("Average Order Size");
                    Field.Add("Average Retailers");
                    Field.Add("Cancellation%");

                    var order = db.DbOrderMaster.ToList();
                    foreach (var item in Field)
                    {
                        if (item == "Sale")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var TotalSale = order.ToList();

                            var total = TotalSale.Select(x => x.TotalAmount).Sum();
                            var lastmonth = TotalSale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Select(x => x.TotalAmount).Sum();
                            var thismonth = TotalSale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Select(x => x.TotalAmount).Sum();
                            var Yesterday = TotalSale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Select(x => x.TotalAmount).Sum();
                            var today = TotalSale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Select(x => x.TotalAmount).Sum();
                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "Pre-Order Cancel%")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var Clusterorder = order.ToList();

                            var totalOrder = Clusterorder.Count();
                            var totalpreOrderCanceled = Clusterorder.Where(i => i.Status == "Order Canceled").Count();
                            var total = ((Convert.ToDouble(totalpreOrderCanceled) / Convert.ToDouble(totalOrder)) * 100);

                            var LMOrder = Clusterorder.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var LMpreOrderCanceled = Clusterorder.Where(i => i.Status == "Order Canceled" && i.CreatedDate.Month == lastmonthdate.Month && i.CreatedDate.Year == lastmonthdate.Year).Count();
                            var lastmonth = ((Convert.ToDouble(LMpreOrderCanceled) / Convert.ToDouble(LMOrder)) * 100);

                            var TMOrder = Clusterorder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var TMpreOrderCanceled = Clusterorder.Where(i => i.Status == "Order Canceled" && i.CreatedDate.Month == todaydate.Month && i.CreatedDate.Year == todaydate.Year).Count();
                            var thismonth = ((Convert.ToDouble(TMpreOrderCanceled) / Convert.ToDouble(TMOrder)) * 100);

                            var YOrder = Clusterorder.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var YpreOrderCanceled = Clusterorder.Where(i => i.Status == "Order Canceled" && i.CreatedDate.Month == yesterdaydate.Month && i.CreatedDate.Year == yesterdaydate.Year && i.CreatedDate.Day == yesterdaydate.Day).Count();
                            var Yesterday = ((Convert.ToDouble(YpreOrderCanceled) / Convert.ToDouble(YOrder)) * 100);

                            var TOrder = Clusterorder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();
                            var TpreOrderCanceled = Clusterorder.Where(i => i.Status == "Order Canceled" && i.CreatedDate.Month == todaydate.Month && i.CreatedDate.Year == todaydate.Year && i.CreatedDate.Day == todaydate.Day).Count();
                            var today = ((Convert.ToDouble(TpreOrderCanceled) / Convert.ToDouble(TOrder)) * 100);

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "Post-Order Cancel%")

                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var Clusterorder = order.ToList();

                            var totalOrder = Clusterorder.Count();
                            var totalpreOrderCanceled = Clusterorder.Where(i => i.Status == "Post Order Canceled").Count();
                            var total = ((Convert.ToDouble(totalpreOrderCanceled) / Convert.ToDouble(totalOrder)) * 100);

                            var LMOrder = Clusterorder.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var LMpreOrderCanceled = Clusterorder.Where(i => i.Status == "Post Order Canceled" && i.CreatedDate.Month == lastmonthdate.Month && i.CreatedDate.Year == lastmonthdate.Year).Count();
                            var lastmonth = ((Convert.ToDouble(LMpreOrderCanceled) / Convert.ToDouble(LMOrder)) * 100);

                            var TMOrder = Clusterorder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var TMpreOrderCanceled = Clusterorder.Where(i => i.Status == "Post Order Canceled" && i.CreatedDate.Month == todaydate.Month && i.CreatedDate.Year == todaydate.Year).Count();
                            var thismonth = ((Convert.ToDouble(TMpreOrderCanceled) / Convert.ToDouble(TMOrder)) * 100);

                            var YOrder = Clusterorder.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var YpreOrderCanceled = Clusterorder.Where(i => i.Status == "Post Order Canceled" && i.CreatedDate.Month == yesterdaydate.Month && i.CreatedDate.Year == yesterdaydate.Year && i.CreatedDate.Day == yesterdaydate.Day).Count();
                            var Yesterday = ((Convert.ToDouble(YpreOrderCanceled) / Convert.ToDouble(YOrder)) * 100);

                            var TOrder = Clusterorder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();
                            var TpreOrderCanceled = Clusterorder.Where(i => i.Status == "Post Order Canceled" && i.CreatedDate.Month == todaydate.Month && i.CreatedDate.Year == todaydate.Year && i.CreatedDate.Day == todaydate.Day).Count();
                            var today = ((Convert.ToDouble(TpreOrderCanceled) / Convert.ToDouble(TOrder)) * 100);

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "Active Retailers")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var Active = Cust.Where(x => x.Active == true).ToList();

                            var total = Active.Count();
                            var lastmonth = Active.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = Active.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = Active.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = Active.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "Average Line Items")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            double total = 0; double lastmonth = 0; double thismonth = 0; double Yesterday = 0; double today = 0;

                            var lineitem = db.DbOrderMaster.ToList();

                            #region TotalLineItem

                            var Totallineitem = lineitem;


                            List<OrderDetails> itemline = new List<OrderDetails>();

                            foreach (var ordermaster in Totallineitem)
                            {
                                var ddd = ordermaster;
                                var orderdetail = db.DbOrderDetails.Where(x => x.OrderId == ordermaster.OrderId).ToList();
                                itemline.AddRange(orderdetail);
                            }

                            int detailsorder = itemline.Count();
                            int masterOrder = Totallineitem.Count();
                            try
                            {
                                total = detailsorder / masterOrder;
                            }
                            catch (Exception ex)
                            {
                            }
                            #endregion

                            #region LastMonthLineItem

                            var LMlineitem = lineitem.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year);

                            List<OrderDetails> LMitemline = new List<OrderDetails>();

                            foreach (var ordermaster in LMlineitem)
                            {
                                var ddd = ordermaster;
                                var orderdetail = db.DbOrderDetails.Where(x => x.OrderId == ordermaster.OrderId).ToList();
                                LMitemline.AddRange(orderdetail);
                            }

                            int LMdetailsorder = LMitemline.Count();
                            int LMmasterOrder = LMlineitem.Count();
                            try
                            {
                                lastmonth = LMdetailsorder / LMmasterOrder;
                            }
                            catch (Exception ex)
                            {
                            }
                            #endregion

                            #region ThisMonthLineItem

                            var TMlineitem = lineitem.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year);

                            List<OrderDetails> TMitemline = new List<OrderDetails>();

                            foreach (var ordermaster in TMlineitem)
                            {
                                var ddd = ordermaster;
                                var orderdetail = db.DbOrderDetails.Where(x => x.OrderId == ordermaster.OrderId).ToList();
                                TMitemline.AddRange(orderdetail);
                            }

                            int TMdetailsorder = TMitemline.Count();
                            int TMmasterOrder = TMlineitem.Count();
                            try
                            {
                                thismonth = TMdetailsorder / TMmasterOrder;
                            }
                            catch (Exception ex)
                            {
                            }
                            #endregion

                            #region YesterdayLineItem

                            var Ylineitem = lineitem.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day);

                            List<OrderDetails> Yitemline = new List<OrderDetails>();

                            foreach (var ordermaster in Ylineitem)
                            {
                                var ddd = ordermaster;
                                var orderdetail = db.DbOrderDetails.Where(x => x.OrderId == ordermaster.OrderId).ToList();
                                Yitemline.AddRange(orderdetail);
                            }

                            int Ydetailsorder = Yitemline.Count();
                            int YmasterOrder = Ylineitem.Count();
                            try
                            {
                                Yesterday = Ydetailsorder / YmasterOrder;
                            }
                            catch (Exception ex)
                            {
                            }
                            #endregion

                            #region TodayLineItem

                            var Tlineitem = lineitem.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day);

                            List<OrderDetails> Titemline = new List<OrderDetails>();

                            foreach (var ordermaster in Tlineitem)
                            {
                                var ddd = ordermaster;
                                var orderdetail = db.DbOrderDetails.Where(x => x.OrderId == ordermaster.OrderId).ToList();
                                Titemline.AddRange(orderdetail);
                            }

                            int Tdetailsorder = Titemline.Count();
                            int TmasterOrder = Tlineitem.Count();
                            try
                            {
                                today = Tdetailsorder / TmasterOrder;
                            }
                            catch (Exception ex)
                            {
                            }
                            #endregion

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;

                            listdsd.Add(dsd);
                        }
                        else if (item == "Average No of Brands")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var ItemBrandAVG = order.Where(x => x.WarehouseId == warehouseid).ToList();
                            var brandAvg = new List<OrderDetails>();
                            foreach (var i in ItemBrandAVG)
                            {
                                brandAvg = db.DbOrderDetails.Where(x => x.OrderId == i.OrderId).ToList();
                            }
                            listdsd.Add(dsd);
                        }
                        else if (item == "Average Order Size")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Average Retailers")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            var ClusterTotalSale = order.Where(x => x.WarehouseId == warehouseid).ToList();
                            var ClusterAvgRetailer = Cust.Where(x => x.Warehouseid == warehouseid && x.Active == true).ToList();

                            var ClusterTotalSaleTotal = ClusterTotalSale.Select(x => x.TotalAmount).Sum();
                            var ClusterAvgRetailerTotal = ClusterAvgRetailer.Where(x => x.Active == true).Count();
                            var AvgRetailerTotal = (Convert.ToDouble(ClusterTotalSaleTotal) / Convert.ToDouble(ClusterAvgRetailerTotal));

                            var ClusterTotalSaleLM = ClusterTotalSale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Select(x => x.TotalAmount).Sum();
                            var ClusterAvgRetailerLM = ClusterAvgRetailer.Where(x => x.Active == true && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var AvgRetailerLM = (Convert.ToDouble(ClusterTotalSaleLM) / Convert.ToDouble(ClusterAvgRetailerLM));

                            var ClusterTotalSaleTM = ClusterTotalSale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Select(x => x.TotalAmount).Sum();
                            var ClusterAvgRetailerTM = ClusterAvgRetailer.Where(x => x.Active == true && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var AvgRetailerTM = (Convert.ToDouble(ClusterTotalSaleTM) / Convert.ToDouble(ClusterAvgRetailerTM));

                            var ClusterTotalSaleY = ClusterTotalSale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Select(x => x.TotalAmount).Sum();
                            var ClusterAvgRetailerY = ClusterAvgRetailer.Where(x => x.Active == true && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var AvgRetailerY = (Convert.ToDouble(ClusterTotalSaleY) / Convert.ToDouble(ClusterAvgRetailerY));

                            var ClusterTotalSaleT = ClusterTotalSale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Select(x => x.TotalAmount).Sum();
                            var ClusterAvgRetailerT = ClusterAvgRetailer.Where(x => x.Active == true && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();
                            var AvgRetailerT = (Convert.ToDouble(ClusterTotalSaleT) / Convert.ToDouble(ClusterAvgRetailerT));

                            dsd.Total = AvgRetailerTotal;
                            dsd.LastMonth = AvgRetailerLM;
                            dsd.ThisMonth = AvgRetailerTM;
                            dsd.Yesterday = AvgRetailerY;
                            dsd.Today = AvgRetailerT;

                            listdsd.Add(dsd);
                        }
                        else if (item == "Cancellation%")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;

                            //this for Cancellation
                            var CancelOrder = order.Where(x => x.WarehouseId == warehouseid).ToList();

                            var TotalCancel = CancelOrder.Count();
                            var TotalCancelOrder = CancelOrder.Where(x => x.Status == "Order Canceled" || x.Status == "Post Order Canceled").Count();
                            var CancelOrderTotal = ((Convert.ToDouble(TotalCancelOrder) / Convert.ToDouble(TotalCancel)) * 100);

                            var LMTotalCancel = CancelOrder.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var LMTotalCancelOrder = CancelOrder.Where(x => (x.Status == "Order Canceled" || x.Status == "Post Order Canceled") && x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var LMCancelOrderTotal = ((Convert.ToDouble(TotalCancelOrder) / Convert.ToDouble(TotalCancel)) * 100);

                            var TMTotalCancel = CancelOrder.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var TMTotalCancelOrder = CancelOrder.Where(x => (x.Status == "Order Canceled" || x.Status == "Post Order Canceled") && x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var TMCancelOrderTotal = ((Convert.ToDouble(TotalCancelOrder) / Convert.ToDouble(TotalCancel)) * 100);

                            var YTotalCancel = CancelOrder.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var YTotalCancelOrder = CancelOrder.Where(x => (x.Status == "Order Canceled" || x.Status == "Post Order Canceled") && x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var YCancelOrderTotal = ((Convert.ToDouble(TotalCancelOrder) / Convert.ToDouble(TotalCancel)) * 100);

                            var TTotalCancel = CancelOrder.Where(x => x.CreatedDate.Date == todaydate.Date).Count();
                            var TTotalCancelOrder = CancelOrder.Where(x => (x.Status == "Order Canceled" || x.Status == "Post Order Canceled") && x.CreatedDate.Date == todaydate.Date).Count();
                            var TCancelOrderTotal = ((Convert.ToDouble(TotalCancelOrder) / Convert.ToDouble(TotalCancel)) * 100);

                            dsd.Total = CancelOrderTotal;
                            dsd.LastMonth = LMCancelOrderTotal;
                            dsd.ThisMonth = TMCancelOrderTotal;
                            dsd.Yesterday = YCancelOrderTotal;
                            dsd.Today = TCancelOrderTotal;

                            listdsd.Add(dsd);
                        }
                    }
                }
                else if (Type == 2)
                {
                    Field = new List<string>();
                    Field.Add("Sale");
                    //Field.Add("Active Retailers");
                    Field.Add("Active Items");

                    foreach (var item in Field)
                    {
                        if (item == "Sale")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var TotKishansaleOrder = db.DbOrderMaster.Where(x => x.WarehouseId == warehouseid).ToList();
                            var TotalKisanKiranaSale = new List<KisankiranaSale>();
                            foreach (var i in TotKishansaleOrder)
                            {
                                TotalKisanKiranaSale = db.DbOrderDetails.Where(x => x.OrderId == i.OrderId && x.SubsubcategoryName == "kisan kirana").Select(x => new KisankiranaSale { TotalAmt = x.TotalAmt, CreatedDate = x.CreatedDate }).ToList();
                            }
                            var total = TotalKisanKiranaSale.Select(x => x.TotalAmt).Sum();
                            var lastmonth = TotalKisanKiranaSale.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Select(x => x.TotalAmt).Sum();
                            var thismonth = TotalKisanKiranaSale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Select(x => x.TotalAmt).Sum();
                            var Yesterday = TotalKisanKiranaSale.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Select(x => x.TotalAmt).Sum();
                            var today = TotalKisanKiranaSale.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Select(x => x.TotalAmt).Sum();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Active Items")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            var TotalActiveItem = db.SubsubCategorys.Where(x => x.SubsubcategoryName == "Kisan Kirana" && x.IsActive == true && x.Deleted == false).ToList();
                            var total = TotalActiveItem.Count();
                            var lastmonth = TotalActiveItem.Where(x => x.CreatedDate.Month == lastmonthdate.Month && x.CreatedDate.Year == lastmonthdate.Year).Count();
                            var thismonth = TotalActiveItem.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year).Count();
                            var Yesterday = TotalActiveItem.Where(x => x.CreatedDate.Month == yesterdaydate.Month && x.CreatedDate.Year == yesterdaydate.Year && x.CreatedDate.Day == yesterdaydate.Day).Count();
                            var today = TotalActiveItem.Where(x => x.CreatedDate.Month == todaydate.Month && x.CreatedDate.Year == todaydate.Year && x.CreatedDate.Day == todaydate.Day).Count();

                            dsd.Total = total;
                            dsd.LastMonth = lastmonth;
                            dsd.ThisMonth = thismonth;
                            dsd.Yesterday = Yesterday;
                            dsd.Today = today;
                            listdsd.Add(dsd);
                        }
                        else if (item == "Active Retailers")
                        {
                            DigitalSalesData dsd = new DigitalSalesData();
                            dsd.FieldName = item;
                            listdsd.Add(dsd);
                        }
                    }
                }

                return listdsd;
            }
        }
        #endregion

        [HttpGet]
        [Route("GetWarehouse")]
        public dynamic GetWarehouse(int clstid)
        {
            using (AuthContext db = new AuthContext())
            {

                var warehouse = db.Clusters.Where(x => x.ClusterId == clstid && x.Active == true).Select(x => new { x.WarehouseId, x.WarehouseName }).ToList();
                return warehouse;
            }
        }
        [HttpGet]
        [Route("GetAgentsHubwise")]
        public dynamic GetAgentHubwise(int clstid)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Customer> Cust = db.Customers.Where(x => x.ClusterId == clstid).ToList();
                var warehouseid = Cust != null && Cust.Any() ? Cust.FirstOrDefault().Warehouseid : 0;

                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + warehouseid + " and r.Name='Agent' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                var activepeople = db.Database.SqlQuery<People>(query).ToList();
                //var activepeople = db.Peoples.Where(x => x.WarehouseId == warehouseid && x.Active && x.Department == "Agent").Select(x => x.DisplayName).ToList();
                var activeagent = activepeople.Count();
                var stringactiveagent = string.Join(",", activepeople);
                return stringactiveagent;

            }
        }
    }

    #region DTO
    public class KisankiranaSale
    {
        public double TotalAmt { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class DigitalSalesData
    {
        public string FieldName { get; set; }
        public double Total { get; set; }
        public double LastMonth { get; set; }
        public double ThisMonth { get; set; }
        public double Yesterday { get; set; }
        public double Today { get; set; }
    }
    public class clusterwisereport
    {
        public int TotalRetailer { get; set; }
        public int TotalRetailerActive { get; set; }
        public string TotalActiveAgent { get; set; }
        public int TotalNewRetailers { get; set; }
        public int TotalWarehouse { get; set; }
        public int TotalCity { get; set; }
        public double TotalAmountSale { get; set; }
        public int KisanKiranaActive { get; set; }
        public string Preorder { get; set; }
        public string Postorder { get; set; }
        public string Cancellation { get; set; }
        public double DeliveredAmount { get; set; }
        public double Avglineitem { get; set; }
        public double Avgofbrands { get; set; }
        public double AvgofRetailer { get; set; }
        public int Salesavgretailer { get; set; }
        public double kisankiranaSale { get; set; }
        public int TotalOrderCount { get; set; }


    }
    #endregion
}



