using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DashboardReport")]
    public class DashboardReportController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Authorize]
        [Route("")]
        [HttpPost]
        public dashboardReport Get(List<hisDTO> obj)
        {
            logger.Info("start get all Sales Executive: ");
            dashboardReport dbReport = new dashboardReport();
            return dbReport;

            //using (var db = new AuthContext())
            //{
            //    try
            //    {

            //        List<CustomerDTO> displist = new List<CustomerDTO>();

            //        var identity = User.Identity as ClaimsIdentity;
            //        int compid = 0, userid = 0;

            //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
            //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            //        if (compid == 0) { compid = 1; }

            //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
            //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


            //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


            //        var m = indianTime.Month;
            //        var y = indianTime.Year;
            //        var d = indianTime.Day;

            //        DateTime lM = DateTime.Now.AddMonths(-1);
            //        int lMonth = lM.Month;
            //        int lYear = lM.Year;

            //        DateTime yesterday = DateTime.Now.AddDays(-1);
            //        int ydMonth = yesterday.Month;
            //        int ydYear = yesterday.Year;
            //        int ydDay = yesterday.Day;

            //        List<Customer> displist1 = new List<Customer>();

            //        foreach (var item in obj)
            //        {
            //            var displist1data = db.Customers.Where(o => o.Deleted == false && o.Warehouseid == item.WarehouseId).ToList();

            //            displist1.AddRange(displist1data);


            //        }
            //        dbReport.TotalCust = displist1.Count;
            //        var CustToday = displist1.Where(x => x.CreatedDate.Date == indianTime.Date).ToList();
            //        dbReport.TodayCust = CustToday.Count;

            //        var CustMonth = displist1.Where(x => x.CreatedDate.Month == m && x.CreatedDate.Year == y).ToList();
            //        dbReport.MonthCust = CustMonth.Count;

            //        var CustYesturday = displist1.Where(x => x.CreatedDate.Date == yesterday.Date).ToList();
            //        dbReport.YesturdayCust = CustYesturday.Count;

            //        var MonthLCust = displist1.Where(x => x.CreatedDate.Month == lMonth && x.CreatedDate.Year == lYear).ToList();
            //        dbReport.LMonthCust = MonthLCust.Count;


            //        //END//
            //        List<OrderMaster> order = new List<OrderMaster>();
            //        //get OrderMaster final 
            //        foreach (var wareh in obj)
            //        {
            //            var ddsad = db.DbOrderMaster.Where(o => o.Deleted == false && o.WarehouseId == wareh.WarehouseId && o.CompanyId == compid && o.Status != "Payment Pending" && o.Status != "Inactive" && o.Status != "Failed" && o.Status != "Dummy Order Cancelled").ToList();
            //            order.AddRange(ddsad);

            //        }

            //        //  Kisan Kirana Total Amount//
            //        List<OrderDetails> ODetails = new List<OrderDetails>();


            //        var OrderIDs = order.Select(x => x.OrderId).ToList();
            //        foreach (var wareh in obj)
            //        {
            //            var details = db.DbOrderDetails.Where(x => x.Deleted == false && x.WarehouseId == wareh.WarehouseId && OrderIDs.Contains(x.OrderId) && x.SubcategoryName == "Kisan Kirana").ToList();

            //            ODetails.AddRange(details);
            //        }

            //        dbReport.OrderedDet = ODetails.Count;
            //        double TSaleDet = 0.00;
            //        double MSaleDet = 0.00;
            //        double DSaleDet = 0.00;
            //        double MLSaleDet = 0.00;
            //        double YDSaleDet = 0.00;
            //        foreach (var a in ODetails)
            //        {

            //            //a.TotalAmt += a.TotalAmt;

            //            if (a.CreatedDate.Date == indianTime.Date)
            //            {
            //                DSaleDet += Convert.ToDouble(a.TotalAmt);
            //            }

            //            if (a.CreatedDate.Date == yesterday.Date)
            //            {
            //                YDSaleDet += Convert.ToDouble(a.TotalAmt);
            //            }
            //            if (a.CreatedDate.Month == lMonth && a.CreatedDate.Year == lYear)
            //            {
            //                MLSaleDet += Convert.ToDouble(a.TotalAmt);
            //            }

            //            if (a.CreatedDate.Month == m && a.CreatedDate.Year == y)
            //            {
            //                MSaleDet += Convert.ToDouble(a.TotalAmt);
            //            }
            //            TSaleDet += Convert.ToDouble(a.TotalAmt);
            //        }

            //        dbReport.TodOSaleDet = DSaleDet;
            //        dbReport.ToYdOSaleDet = YDSaleDet;
            //        dbReport.MOLSaleDet = MLSaleDet;
            //        dbReport.MOSaleDet = MSaleDet;
            //        dbReport.OSaleDet = TSaleDet;


            //        // var order = db.DbOrderMaster.Where(o => o.Deleted == false && o.WarehouseId == Warehouseid && o.CompanyId == compid && o.Status != "Payment Pending" && o.Status != "Inactive" && o.Status != "Failed" && o.Status != "Dummy Order Cancelled").ToList();


            //        var ACustYesterday = order.Where(o => o.Deleted == false && o.CompanyId == compid && o.CreatedDate.Day == ydDay && o.CreatedDate.Month == ydMonth && o.CreatedDate.Year == ydYear).GroupBy(v => v.Skcode).ToList();





            //        dbReport.AcYesterdayCust = ACustYesterday.Count;

            //        foreach (var cust in ACustYesterday)
            //        {
            //            if (cust.Count() > 2)
            //            {
            //                dbReport.AcYesterdayCust2++;
            //            }
            //        }
            //        var ACustLMonth = order.Where(o => o.Deleted == false && o.CompanyId == compid && o.CreatedDate.Month == lMonth && o.CreatedDate.Year == lYear).GroupBy(v => v.Skcode).ToList();

            //        dbReport.AcLMonthCust = ACustLMonth.Count;

            //        foreach (var cust in ACustLMonth)
            //        {
            //            if (cust.Count() > 2)
            //            {
            //                dbReport.AcLMonthCust2++;
            //            }
            //        }
            //        try
            //        {
            //            var ActCust = (from x in order
            //                           where x.Deleted == false && x.CompanyId == compid
            //                           group x by x.Skcode).ToList();
            //            dbReport.ActiveCust = ActCust.Count;

            //            foreach (var cust in ActCust)
            //            {
            //                if (cust.Count() > 2)
            //                {
            //                    dbReport.ActiveCust2++;
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {

            //        }

            //        var ACustMonth = order.Where(o => o.Deleted == false && o.CompanyId == compid && o.CreatedDate.Month == m && o.CreatedDate.Year == y).GroupBy(v => v.Skcode).ToList();
            //        dbReport.AcMonthCust = ACustMonth.Count;

            //        foreach (var cust in ACustMonth)
            //        {
            //            if (cust.Count() > 2)
            //            {
            //                dbReport.AcMonthCust2++;
            //            }
            //        }
            //        var ACustToday = order.Where(o => o.Deleted == false && o.CompanyId == compid && o.CreatedDate.Day == indianTime.Day && o.CreatedDate.Month == m && o.CreatedDate.Year == y).GroupBy(v => v.Skcode).ToList();
            //        dbReport.AcTodayCust = ACustToday.Count;

            //        foreach (var cust in ACustToday)
            //        {
            //            if (cust.Count() > 2)
            //            {
            //                dbReport.AcTodayCust2++;
            //            }
            //        }
            //        dbReport.Ordered = order.Count;
            //        double TSale = 0.00;
            //        double MSale = 0.00;
            //        double DSale = 0.00;
            //        double MLSale = 0.00;
            //        double YDSale = 0.00;
            //        var DOrder = 0;
            //        var MOrder = 0;
            //        var MLOrder = 0;
            //        var DYOrder = 0;
            //        foreach (var a in order)
            //        {
            //            a.GrossAmount += (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0);

            //            if (a.CreatedDate.Date == indianTime.Date)
            //            {
            //                DOrder++;
            //                DSale += Convert.ToDouble(a.GrossAmount);
            //            }

            //            if (a.CreatedDate.Date == yesterday.Date)
            //            {
            //                DYOrder++;
            //                YDSale += Convert.ToDouble(a.GrossAmount);
            //            }
            //            if (a.CreatedDate.Month == lMonth && a.CreatedDate.Year == lYear)
            //            {
            //                MLSale += Convert.ToDouble(a.GrossAmount);
            //                MLOrder++;
            //            }

            //            if (a.CreatedDate.Month == m && a.CreatedDate.Year == y)
            //            {
            //                MSale += Convert.ToDouble(a.GrossAmount);
            //                MOrder++;
            //            }
            //            TSale += Convert.ToDouble(a.GrossAmount);
            //        }

            //        dbReport.TodOrdered = DOrder;
            //        dbReport.ToYOrdered = DYOrder;
            //        dbReport.MOrdered = MOrder;
            //        dbReport.TodOSale = DSale;
            //        dbReport.MLOrdered = MLOrder;
            //        dbReport.ToYdOSale = YDSale;
            //        dbReport.MOLSale = MLSale;
            //        dbReport.MOSale = MSale;
            //        dbReport.OSale = TSale;
            //        var DlOr = order.Where(x => x.Status == "sattled").ToList();
            //        dbReport.Odeliver = DlOr.Count;
            //        var DDlOrder = DlOr.Where(x => x.CreatedDate.Date == indianTime.Date).ToList();
            //        dbReport.ToOdeliver = DDlOrder.Count;
            //        var MDlOrder = DlOr.Where(x => x.CreatedDate.Month == m && x.CreatedDate.Year == y).ToList();
            //        dbReport.MOdeliver = MDlOrder.Count;
            //        var YDlOrder = DlOr.Where(x => x.CreatedDate.Date == yesterday.Date).ToList();
            //        dbReport.ToOYdeliver = YDlOrder.Count;
            //        var LMDlOrder = DlOr.Where(x => x.CreatedDate.Month == ydDay && x.CreatedDate.Year == ydYear).ToList();
            //        dbReport.LMOdeliver = LMDlOrder.Count;
            //        var DlOrder = order.Where(x => x.Deliverydate.Date > yesterday.Date && x.Status == "Delivered").ToList();

            //        var Dl48Order = 0;
            //        var MDl48Order = 0;
            //        var YDl48Order = 0;
            //        var TDl48Order = 0;
            //        foreach (var a in DlOrder)
            //        {
            //            TimeSpan diff = a.UpdatedDate - a.CreatedDate;
            //            double hours = diff.TotalHours;
            //            if (hours <= 48)
            //            {
            //                Dl48Order++;
            //                if (a.Deliverydate.Date == indianTime.Date)
            //                {
            //                    TDl48Order++;
            //                }
            //                if (a.Deliverydate.Date == yesterday.Date)
            //                {
            //                    YDl48Order++;
            //                }
            //                if (a.Deliverydate.Month == m && a.Deliverydate.Year == y)
            //                {
            //                    MDl48Order++;
            //                }
            //            }
            //        }
            //        dbReport.O48deliver = Dl48Order;
            //        dbReport.O48Tdeliver = TDl48Order;
            //        dbReport.O48Ydeliver = YDl48Order;
            //        dbReport.MO48deliver = MDl48Order;


            //        var todayInv = 0.0;
            //        var YdayInv = 0.0;
            //        var monthInv = 0.0;
            //        var totalInv = 0.0;
            //        var todayInvSale = 0.0;
            //        var monthInvSale = 0.0;
            //        var totalInvSale = 0.0;
            //        var YtotalInvSale = 0.0;
            //        List<AvgInventory> avgInv = new List<AvgInventory>();
            //        foreach (var item5 in obj)
            //        {
            //            var avgInvData = db.AvgInventoryDb.Where(x => x.WarehouseId == item5.WarehouseId && x.CompanyId == compid).ToList();
            //            avgInv.AddRange(avgInvData);
            //        }

            //        var mday = 0;
            //        var totDay = 0;
            //        if (avgInv.Count != 0)
            //        {
            //            if (indianTime.Hour >= 20)
            //            {
            //                totDay = Convert.ToInt32((indianTime.Date - avgInv[0].date.Date).TotalDays) + 1;
            //            }
            //            else
            //            {
            //                totDay = Convert.ToInt32((indianTime.Date - avgInv[0].date.Date).TotalDays);
            //            }
            //            foreach (var ac in avgInv)
            //            {
            //                totalInv += ac.totals;
            //                totalInvSale += ac.totalSale;
            //                if (ac.date.Date == indianTime.Date)
            //                {
            //                    todayInv += ac.totals;
            //                    todayInvSale += ac.totalSale;
            //                }


            //                if (ac.date.Date == yesterday.Date)
            //                {
            //                    YdayInv += ac.totals;
            //                    YtotalInvSale += ac.totalSale;
            //                }



            //                if (ac.date.Month == m && ac.date.Year == y)
            //                {
            //                    monthInv += ac.totals;
            //                    monthInvSale += ac.totalSale;
            //                    if (ac.date.Month == ac.firstdate.Month && ac.date.Year == ac.date.Year)
            //                    {
            //                        if (indianTime.Hour >= 20)
            //                        {
            //                            mday = Convert.ToInt32((indianTime.Date - ac.firstdate.Date).TotalDays) + 1;
            //                        }
            //                        else
            //                        {
            //                            mday = Convert.ToInt32((indianTime.Date - ac.firstdate.Date).TotalDays);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        mday = ac.date.Day;
            //                    }
            //                }
            //            }
            //            dbReport.ToInvTurn = (todayInv / todayInvSale);
            //            dbReport.ToYInvTurn = (YdayInv / YtotalInvSale);
            //            dbReport.MInvTurn = (monthInv / monthInvSale);
            //            dbReport.InvTurn = (totalInv / totalInvSale);

            //            dbReport.ToAvgInv = todayInv;
            //            dbReport.ToYAvgInv = YdayInv;
            //            dbReport.MAvInv = (monthInv / mday);
            //            dbReport.AvgInv = (totalInv / totDay);

            //            logger.Info("End Customer: ");
            //        }
            //        return dbReport;
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error("Error in getall  Customer " + ex.Message);
            //        logger.Info("End get all Customer: ");
            //        return null;
            //    }
            //}
        }


        [Route("updateInventory")]
        [HttpPost]
        public async Task<bool> updateInventory(List<hisDTO> WarehouseIds)
        {

            logger.Info("start get all Sales Executive: ");
            using (var db = new AuthContext())
            {
                try
                {
                    List<Customer> displist1 = new List<Customer>();

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (compid == 0) { compid = 1; }


                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var wids = WarehouseIds.Select(x => x.WarehouseId);
                    var d = indianTime;
                    var warehouse = await db.Warehouses.Where(x => x.Deleted == false && wids.Contains(x.WarehouseId)).ToListAsync();
                    var Currentstocklist = await db.DbCurrentStock.Where(x => x.Deleted == false && x.CurrentInventory != 0 && wids.Contains(x.WarehouseId) && x.CompanyId == compid).ToListAsync();
                    var itemMasterList = await db.itemMasters.Where(x => wids.Contains(x.WarehouseId) && x.Deleted == false && x.NetPurchasePrice > 0).ToListAsync();
                    var orderMasterlist = await db.DbOrderMaster.Where(x => x.Deleted == false && x.Status != "Payment Pending" && x.Status != "Inactive" && x.Status != "Failed" && x.Status != "Dummy Order Cancelled" && wids.Contains(x.WarehouseId) && x.CompanyId == compid && x.CreatedDate.Day == d.Day && x.CreatedDate.Month == d.Month && x.CreatedDate.Year == d.Year).ToListAsync();
                    var AvgInventoryList = await db.AvgInventoryDb.Where(x => x.deleted == false && wids.Contains(x.WarehouseId) && x.CompanyId == compid).ToListAsync();

                    foreach (var item in warehouse)
                    {
                        if (item.WarehouseId > 0)
                        {
                            var totals = 0.00;
                            var totSale = 0.00;
                            int countt = 0;
                            var list = Currentstocklist.Where(x => x.Deleted == false && x.CurrentInventory != 0 && x.WarehouseId == item.WarehouseId && x.CompanyId == compid).ToList();
                            foreach (var data in list)
                            {
                                try
                                {
                                    var itemMaster = itemMasterList.FirstOrDefault(x => x.ItemMultiMRPId == data.ItemMultiMRPId && x.CompanyId == compid && x.WarehouseId == item.WarehouseId && x.Deleted == false && x.NetPurchasePrice > 0);
                                    if (item != null)
                                    {
                                        countt++;
                                        totals = totals + (Convert.ToDouble(data.CurrentInventory) * itemMaster.NetPurchasePrice);
                                    }
                                }
                                catch (Exception es)
                                {

                                }
                            }
                            List<OrderMaster> orderlist = orderMasterlist.Where(x => x.Deleted == false && x.Status != "Payment Pending" && x.Status != "Inactive" && x.Status != "Failed" && x.Status != "Dummy Order Cancelled" && x.WarehouseId == item.WarehouseId && x.CompanyId == compid && x.CreatedDate.Day == d.Day && x.CreatedDate.Month == d.Month && x.CreatedDate.Year == d.Year).ToList();

                            foreach (var orderdata in orderlist)
                            {
                                totSale = totSale + orderdata.TotalAmount;
                            }
                            var ddddd = AvgInventoryList.Where(x => x.deleted == false && x.WarehouseId == item.WarehouseId && x.CompanyId == compid).ToList();
                            if (ddddd.Count == 0)
                            {
                                AvgInventory aaaa = new AvgInventory();
                                aaaa.firstdate = indianTime.Date;
                                aaaa.date = indianTime.Date;
                                aaaa.totalSale = totSale;
                                aaaa.totals = totals;
                                aaaa.CompanyId = compid;
                                aaaa.WarehouseId = item.WarehouseId;
                                aaaa.active = true;
                                db.AvgInventoryDb.Add(aaaa);
                                db.Commit();
                            }
                            else
                            {
                                var mmdd = ddddd.Where(x => x.date.Date == indianTime.Date).FirstOrDefault();
                                if (mmdd == null)
                                {
                                    AvgInventory aaaa = new AvgInventory();
                                    aaaa.firstdate = ddddd[0].firstdate;
                                    aaaa.date = indianTime.Date;
                                    aaaa.totals = totals;
                                    aaaa.totalSale = totSale;
                                    aaaa.CompanyId = compid;
                                    aaaa.WarehouseId = item.WarehouseId;
                                    aaaa.active = true;
                                    db.AvgInventoryDb.Add(aaaa);
                                    db.Commit();
                                }
                                else
                                {
                                    mmdd.date = indianTime.Date;
                                    mmdd.totals = totals;
                                    mmdd.totalSale = totSale;
                                    db.AvgInventoryDb.Attach(mmdd);
                                    db.Entry(mmdd).State = EntityState.Modified;
                                    db.Commit();
                                }
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall  Customer " + ex.Message);
                    logger.Info("End get all Customer: ");
                    return false;
                }
            }
        }

        [Authorize]
        [Route("totalSell")]
        [HttpGet]
        public async Task<string> totalsell()
        {
            logger.Info("start get all Sales Executive: ");
            using (var db = new AuthContext())
            {

                DateTime now = indianTime;
                var startDate = new DateTime(now.Year, now.Month, 1);//now.Hour, now.Minute, now.Second, now.Kind
                var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);

                var query = "select sum(grossamount + isnull(BillDiscountAmount,0) + isnull(WalletAmount,0) - isnull(TCSAmount,0)) MonthSale," +
                        " sum(case when cast(createddate as date) = cast(getdate() as date) then grossamount + isnull(BillDiscountAmount, 0) + isnull(WalletAmount, 0)  - isnull(TCSAmount,0) else 0 end) TodaySale" +
                        " from ordermasters with(nolock) where deleted = 0 and CreatedDate between @startdate and @enddate and status not in ('Payment Pending','Inactive','Failed','Dummy Order Cancelled')";

                var startDateparam = new SqlParameter("StartDate", startDate);
                var endDateParam = new SqlParameter("EndDate", endDate);

                var sale = await db.Database.SqlQuery<Sale>(query, startDateparam, endDateParam).FirstOrDefaultAsync();

                var TodayTotalsell = sale.TodaySale;
                var MonthTotalsell = sale.MonthSale;
                return TodayTotalsell + " " + MonthTotalsell;
            }
        }


        [Authorize]
        [Route("MonthSale")]
        [HttpGet]
        public async Task<DashboardSaleDc> MonthSale()
        {
            DashboardSaleDc _result = new DashboardSaleDc();
            using (var context = new AuthContext())
            {
                string Query = "Exec MonthSales";
                _result = context.Database.SqlQuery<DashboardSaleDc>(Query).FirstOrDefault();

            }
            GstReportManager gstReportManager = new GstReportManager();
            var orderMasters = await gstReportManager.GetDirectDashboard();
            _result.DMSale = orderMasters.TotalSaleMtd;
            _result.DTSale = orderMasters.TotalSaleToday;
            _result.DSKTSale = _result.TSale + orderMasters.TotalSaleToday;
            _result.DSKMSale = _result.MSale + orderMasters.TotalSaleMtd;
            return _result;

        }

        [Authorize]
        [Route("StoresMonthSales")]
        [HttpGet]
        public async Task<List<StoresMonthSalesDc>> StoresMonthSales()
        {
            List<StoresMonthSalesDc> _result = new List<StoresMonthSalesDc>();
            using (var context = new AuthContext())
            {
                string Query = "Exec StoresMonthSales";
                _result =await context.Database.SqlQuery<StoresMonthSalesDc>(Query).ToListAsync();
            }
            return _result;
        }

        [Authorize]
        [Route("fillrate")]
        [HttpPost]
        public HttpResponseMessage getfillrate(int WarehouseId) //get search orders for delivery
        {

            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                if (compid == 0) { compid = 1; }
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                string Query = "";
                if (WarehouseId > 0)
                {
                    Query = "select Cast((cast(sum(odd.qty) as float)) / cast(sum(od.qty) as float) * 100 as varchar)  as fillrates " +
                            " from orderdetails od with(nolock) left join OrderDispatchedDetails odd with(nolock) on od.OrderDetailsId = odd.OrderDetailsId" +
                            " where  month(odd.CreatedDate)=month(GETDATE()) and Year(odd.CreatedDate)=Year(GETDATE()) and od.WarehouseId=" + WarehouseId;
                }
                else
                {
                    Query = "select Cast((cast(sum(odd.qty) as float)) / cast(sum(od.qty) as float) * 100 as varchar)   as fillrates " +
                               " from orderdetails od with(nolock) left join OrderDispatchedDetails odd with(nolock) on od.OrderDetailsId = odd.OrderDetailsId" +
                               " where  month(odd.CreatedDate)=month(GETDATE()) and Year(odd.CreatedDate)=Year(GETDATE())";
                }
                var fillrates = db.Database.SqlQuery<string>(Query).FirstOrDefault();

                if (fillrates != null)
                {
                    decimal fr = Convert.ToDecimal(fillrates);
                    return Request.CreateResponse(HttpStatusCode.OK, fr);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, fillrates);
                }
            }
        }

        [Route("TotalActiveCustomer")]
        [HttpPost]
        public dynamic TotalActiveCustomer(List<hisDTO> obj)
        {
            using (var con = new AuthContext())
            {
                GetCount countdata = new GetCount();
                List<GetCount> counts = new List<GetCount>();
                foreach (var item in obj)
                {
                    if (item.WarehouseId > 0)
                    {
                        var query = "select count(customerid) as TotalSignup, sum((case  when Active=1 and Deleted=0 then 1 else 0 end)) as ActiveRetailer from Customers with (nolock) where Warehouseid='" + item.WarehouseId + "'";

                        var count = con.Database.SqlQuery<GetCount>(query).ToList();
                        foreach (var data in count)
                        {

                            countdata.TotalSignup += data.TotalSignup;
                            countdata.ActiveRetailer += data.ActiveRetailer;

                        }
                    }
                    else
                    {
                        var query = "select count(customerid) as TotalSignup, sum((case  when Active=1 and Deleted=0 then 1 else 0 end)) as ActiveRetailer from Customers with (nolock)";
                        var count = con.Database.SqlQuery<GetCount>(query).ToList();
                        counts.AddRange(count);
                    }

                }
                counts.Add(countdata);
                return counts;
            }

        }
    }




    public class StoresMonthSalesDc
    {
        public string Name { set; get; }
        public double MSale { get; set; }
        public double TSale { get; set; }
        public double TDispatchValues { get; set; }
        public double MDispatchValues { get; set; }
        public double DMSale { get; set; }//Digital Order
        public double DTSale { get; set; }//Digital Order

    }


    public class DashboardSaleDc
    {

        public double MSale { get; set; }
        public double TSale { get; set; }
        public double MSafoyaSale { get; set; }
        public double TSafoyaSale { get; set; }
        public double KKMSale { get; set; }
        public double KKTodSale { get; set; }
        public double CBMSale { get; set; }
        public double CBTodSale { get; set; }

        public double DMSale { get; set; }
        public double DTSale { get; set; }
        public double DSKMSale { get; set; }
        public double DSKTSale { get; set; }
     
    }

    public class GetCount
    {
        public int TotalSignup { get; set; }
        public int ActiveRetailer { get; set; }


    }
    public class fillrate
    {
        public double fillrates { get; set; }

    }

    public class hisDTO
    {
        public int WarehouseId { get; set; }
    }
    public class dashboardReport
    {
        public int TotalCust { get; set; }
        public int TodayCust { get; set; }
        public int MonthCust { get; set; }
        public int YesturdayCust { get; set; }
        public int LMonthCust { get; set; }
        public int AcTodayCust { get; set; }
        public int AcMonthCust { get; set; }
        public int ActiveCust { get; set; }

        public int AcYesterdayCust { get; set; }
        public int AcLMonthCust { get; set; }
        public int AcYesterdayCust2 { get; set; }
        public int AcLMonthCust2 { get; set; }
        public int AcTodayCust2 { get; set; }
        public int AcMonthCust2 { get; set; }
        public int ActiveCust2 { get; set; }
        public int Ordered { get; set; }
        public int MOrdered { get; set; }
        public int MLOrdered { get; set; }
        public int TodOrdered { get; set; }
        public int ToYOrdered { get; set; }
        public int Odeliver { get; set; }
        public int MOdeliver { get; set; }
        public int LMOdeliver { get; set; }
        public int ToOdeliver { get; set; }
        public int ToOYdeliver { get; set; }

        public int O48Tdeliver { get; set; }
        public int O48Ydeliver { get; set; }
        public int O48deliver { get; set; }
        public int MO48deliver { get; set; }
        public double OSale { get; set; }
        public double MOSale { get; set; }
        public double MOLSale { get; set; }
        public double TodOSale { get; set; }
        public double ToYdOSale { get; set; }
        public double ToAvgInv { get; set; }
        public double ToYAvgInv { get; set; }
        public double MAvInv { get; set; }
        public double AvgInv { get; set; }
        public double ToInvTurn { get; set; }
        public double ToYInvTurn { get; set; }
        public double MInvTurn { get; set; }
        public double InvTurn { get; set; }


        public double OSaleDet { get; set; }
        public double MOSaleDet { get; set; }
        public double MOLSaleDet { get; set; }
        public double TodOSaleDet { get; set; }
        public double ToYdOSaleDet { get; set; }
        public int OrderedDet { get; set; }


    }

    public class Sale
    {
        public double MonthSale { get; set; }
        public double TodaySale { get; set; }

    }
}
