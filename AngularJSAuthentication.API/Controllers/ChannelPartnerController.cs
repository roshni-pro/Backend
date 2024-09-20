using AngularJSAuthentication.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ChannelPartner")]
    public class ChannelPartnerController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public object MainReport { get; private set; }


        public class bound
        {
            public int TotalOrders { get; set; }
            public int cuscount { get; set; }
            public int TotalCustomersInMonth { get; set; }
            public int TotalCustomerslMonth { get; set; }
            public int TotalCustomersyDay { get; set; }
            public int TotalCustomersToday { get; set; }
            public double Totalsell { get; set; }
            public double TotalCostInMonthsell { get; set; }
            public double TotalCostTodaysell { get; set; }
            public double TotalCostYDaysell { get; set; }
            public int status { get; set; }
            public int statusor { get; set; }
            public int statusTotal { get; set; }
            public int TotalActive { get; set; }
            public int TotalInActives { get; set; }
            public int TotalActiveCustomerInMonth { get; set; }
            public int TotalActiveCustomerlMonth { get; set; }
            public int TotalActiveCustomerYDay { get; set; }
            public int TotalActiveCustomerToday { get; set; }
            public int TotalInActiveCustomerToday { get; set; }
            public int TotalInActiveCustomerInMonth { get; set; }
            public int TotalInActiveCustomerYDay { get; set; }
            public int TotalOrdersInmonth { get; set; }
            public int TotalOrdersToday { get; set; }
            public int TotalOrdersyDay { get; set; }
            public double totalcommission { get; set; }
        }

        [Route("GetAgentCustomer")]
        [HttpGet]

        public dynamic GetAgent(int CityId, string type, DateTime start, DateTime end)
        {
            List<listchannelpartner> ab = new List<listchannelpartner>();
            return 0;
            //bound b = new Controllers.ChannelPartnerController.bound();
            //logger.Info("start City: ");
            //Customer customer = new Customer();
            //try
            //{
            //    var identity = User.Identity as ClaimsIdentity;
            //    int compid = 0, userid = 0;
            //    foreach (Claim claim in identity.Claims)
            //    {
            //        if (claim.Type == "compid")
            //        {
            //            compid = int.Parse(claim.Value);
            //        }
            //        if (claim.Type == "userid")
            //        {
            //            userid = int.Parse(claim.Value);
            //        }
            //    }
            //    using (AuthContext db = new AuthContext())
            //    {
            //        int CompanyId = compid;
            //        DateTime now = DateTime.Now;
            //        var startDate = new DateTime(now.Year, now.Month, 1);
            //        var endDate = startDate.AddMonths(1).AddDays(-1);

            //        DateTime lM = DateTime.Now.AddMonths(-1).AddYears(-1);
            //        int lMonth = lM.Month;
            //        int lYear = lM.Year;

            //        DateTime yesterday = DateTime.Now.AddDays(-1);
            //        int ydMonth = yesterday.Month;
            //        int ydYear = yesterday.Year;
            //        int ydDay = yesterday.Day;

            //        b.TotalCustomersInMonth = 0;
            //        if (type == "Agent")
            //        {

            //            string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Cityid=" + CityId + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
            //            var peopl = db.Database.SqlQuery<People>(query).ToList();

            //            // var peopl = db.Peoples.Where(p => p.Active == true && p.Type == "Sales Executive" && p.Cityid == CityId).Select(x => new { x.PeopleID, x.DisplayName }).Distinct().ToList();
            //            var peopleids = peopl.Select(x => x.PeopleID).ToList();
            //            //var agent = db.Peoples.Where(a => peopleids.Contains(a.AgentCode) && a.Department == "Sales Executive").Select(a => new { a.PeopleID, a.DisplayName }).ToList();
            //            //var executiveid = agent.Select(x => x.PeopleID).ToList();
            //            var customers = db.Customers.Where(x => x.ExecutiveId.HasValue && peopleids.Contains(x.ExecutiveId.Value) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end)).Select(x => new { x.CustomerId, x.IsSignup, x.Active, x.ExecutiveId, x.Deleted }).ToList();

            //            //var orders = db.DbOrderMaster.Where(x => x.SalesPersonId.HasValue && peopleids.Contains(x.SalesPersonId.Value) && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Delivered") && EntityFunctions.TruncateTime(x.DeliveredDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.DeliveredDate) <= EntityFunctions.TruncateTime(end)).Select(x => new { x.OrderId, x.TotalAmount, x.Status, x.SalesPersonId,x.DeliveredDate,x.active,x.Deleted  }).ToList();


            //            var salesListDelPerc = db.DbOrderMaster.Where(x => x.active && !x.Deleted && x.SalesPersonId.HasValue && peopleids.Contains(x.SalesPersonId.Value) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end))
            //               .Select(x => new { x.SalesPersonId, x.TotalAmount, x.Skcode, x.active, x.Deleted, x.Status, x.DeliveredDate, x.OrderId }).ToList();

            //            var salesList = db.DbOrderMaster.Where(x => x.active == true && x.Deleted == false && x.SalesPersonId.HasValue && peopleids.Contains(x.SalesPersonId.Value) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end))
            //                .Select(x => new { x.SalesPersonId, x.TotalAmount, x.Skcode, x.active, x.Deleted, x.Status, x.DeliveredDate, x.OrderId }).ToList();

            //            var SaleListCom = db.DbOrderMaster.Where(x => x.active == true && x.Deleted == false && x.SalesPersonId.HasValue && peopleids.Contains(x.SalesPersonId.Value) &&
            //            EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end) &&
            //                                (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Delivered"))
            //                .Select(x => new { x.SalesPersonId, x.TotalAmount, x.Skcode, x.active, x.Deleted, x.Status, x.DeliveredDate, x.OrderId }).ToList();

            //            var salesListActive = db.DbOrderMaster.Where(x => x.active == true && x.Deleted == false && x.SalesPersonId.HasValue && peopleids.Contains(x.SalesPersonId.Value) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end))
            //               .Select(x => new { x.Skcode, x.SalesPersonId }).ToList();

            //            var salesListCancel = db.DbOrderMaster.Where(x => x.active && !x.Deleted && x.SalesPersonId.HasValue && peopleids.Contains(x.SalesPersonId.Value) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end) && (x.Status == "Delivery Canceled" || x.Status == "Order Canceled" || x.Status == "Post Order Canceled"))
            //              .Select(x => new { x.SalesPersonId, x.TotalAmount, x.Skcode, x.active, x.Deleted, x.Status, x.DeliveredDate, x.OrderId }).ToList();


            //            var customerList = db.Customers.Where(x => x.ExecutiveId.HasValue && peopleids.Contains(x.ExecutiveId.Value))
            //                                    .Select(x => new { x.ExecutiveName, x.CustomerId }).ToList();

            //            var customerIds = customerList.Select(z => z.CustomerId).ToList();

            //            var orderDispatchList = db.OrderDispatchedDetailss.Where(x => customerIds.Contains(x.CustomerId) && /*x.Status == "Account settled" &&*/ x.Deleted == false)
            //                                    .Select(x => new
            //                                    {
            //                                        ItemId = x.ItemId,
            //                                        TotalAmt = x.TotalAmt,
            //                                        CustomerId = x.CustomerId

            //                                    }).ToList();

            //            var itemIds = orderDispatchList.Select(z => z.ItemId).ToList();

            //            var itemList = db.itemMasters.AsQueryable().Where(x => itemIds.Contains(x.ItemId)).Select(x => new { x.SubsubcategoryName, x.ItemId }).Distinct().ToList();
            //            var subCatList = itemList.Select(z => z.SubsubcategoryName).ToList();

            //            var orderIds = salesList.Select(z => z.OrderId).ToList();
            //            var itemCommisionPercentList = db.SubsubCategorys.Where(x => subCatList.Contains(x.SubsubcategoryName)).Select(x => new { x.AgentCommisionPercent, x.SubsubcategoryName }).ToList();

            //            #region Get Data From SP

            //            var orderIdDt = new DataTable();
            //            orderIdDt.Columns.Add("IntValue");
            //            foreach (var item in orderIds)
            //            {
            //                var dr = orderIdDt.NewRow();
            //                dr["IntValue"] = item;
            //                orderIdDt.Rows.Add(dr);
            //            }

            //            var param = new SqlParameter("param", orderIdDt);
            //            param.SqlDbType = SqlDbType.Structured;
            //            param.TypeName = "dbo.IntValues";
            //            var ordHistories = db.Database.SqlQuery<OrderHistories>("exec GetOrderHistories @param", param).ToList();

            //            #endregion
            //            var result = new ConcurrentBag<listchannelpartner>();




            //            ParallelLoopResult loopResult = Parallel.ForEach(peopl, (l) =>
            //            {
            //                var totalDelivered = salesList.Where(x => x.SalesPersonId == l.PeopleID).Count();
            //                var totalCancelled = salesList.Where(x => (x.Status == "Delivery Canceled" || x.Status == "Order Canceled" || x.Status == "Post Order Canceled") && x.SalesPersonId == l.PeopleID).Count();


            //                listchannelpartner abl = new listchannelpartner();
            //                abl.agents = l.DisplayName;
            //                //abl.sales = orders.Where(x => x.SalesPersonId == l.PeopleID && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Delivered") && x.active==true && x.Deleted ==false).Select(x => x.TotalAmount).Sum();
            //                abl.sales = salesList.Where(x => x.SalesPersonId == l.PeopleID).Sum(x => x.TotalAmount);

            //                var sale = SaleListCom.Where(x => x.SalesPersonId == l.PeopleID).Sum(x => x.TotalAmount);



            //                //abl.custactive = salesListActive.Where(x => x.SalesPersonId == l.PeopleID).GroupBy(grp => grp.Skcode).Where(x => x.Count() == 1).Count();
            //                // abl.custactive = salesListActive.GroupBy(c => c.Skcode).Where(x => x.Count() > 1).Select(x => x.Key).Count();
            //                abl.custactive = salesListActive.Where(x => x.SalesPersonId == l.PeopleID).Select(x => x.Skcode).Distinct().Count();




            //                //abl.custactive=db.DbOrderMaster.Where(x=>x.SalesPersonId== l.PeopleID && x.CreatedDate >= EntityFunctions.Truncstart) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(end))

            //                //abl.custsignedup = customers.Where(x => x.ExecutiveId == l.PeopleID).Select(x => x.IsSignup && x.Deleted == false).Count();
            //                abl.custsignedup = customerList.Where(x => x.ExecutiveName == l.DisplayName).Count(); /*db.Customers.Where(x => x.ExecutiveId == l.PeopleID && x.IsSignup == true && x.Deleted == false).Select(x => new { x.CustomerId, x.IsSignup, x.Active, x.ExecutiveId, x.Deleted }).Count();*/

            //                double countD = salesListDelPerc.Where(x => x.SalesPersonId.Value == l.PeopleID).Count();
            //                double count = SaleListCom.Where(x => x.SalesPersonId.Value == l.PeopleID).Count();
            //                //var b = abl.DeliveryPerc
            //                double finalD = countD - count;
            //                double final = count * 100 / countD;
            //                abl.DeliveryPerc = final;
            //                if (abl.DeliveryPerc.ToString() == "NaN")
            //                {
            //                    abl.DeliveryPerc = 0;

            //                }
            //                double countC = salesListCancel.Where(x => x.SalesPersonId.Value == l.PeopleID).Count();
            //                var finalcancel = countD - countC;
            //                double countFi = countC * 100 / countD;
            //                abl.CancelPerc = countFi;

            //                if (abl.CancelPerc.ToString() == "NaN")
            //                {
            //                    abl.CancelPerc = 0;

            //                }


            //                var totalAmount = 0;
            //                var commisionamount = 0;


            //                try
            //                {
            //                    //var perc = db.SubsubCategorys.Select(s => s.AgentCommisionPercent).SingleOrDefault();

            //                    var com = sale * 2 / 100;
            //                    b.totalcommission = b.totalcommission + Convert.ToDouble(com);
            //                    abl.totCom = com;

            //                }
            //                catch (Exception ex)
            //                {

            //                }


            //                var orderid = salesList.Where(x => x.SalesPersonId == l.PeopleID).ToList();

            //                TimeSpan diffResult = new TimeSpan();
            //                if (orderid != null && orderid.Any())
            //                {

            //                    foreach (var history in ordHistories.Where(x => (x.Status == "Issued" || x.Status == "Delivered") && orderid.Select(z => z.OrderId).Contains(x.orderid)).GroupBy(x => x.orderid))
            //                    {
            //                        if (history.Any(z => z.Status == "Issued") && history.Any(z => z.Status == "Delivered"))
            //                        {
            //                            var rDDate = history.FirstOrDefault(x => x.Status == "Issued").CreatedDate;
            //                            var DDate = history.FirstOrDefault(x => x.Status == "Delivered").CreatedDate;
            //                            diffResult = DDate - rDDate;
            //                        }
            //                    }

            //                }



            //                abl.totTat = diffResult.ToString(@"hh\:mm");
            //                if (abl.custactive != 0 && abl.custsignedup != 0)
            //                {

            //                    result.Add(abl);
            //                }
            //            });

            //            if (loopResult.IsCompleted)
            //                ab = result.ToList();

            //        }
            //        else
            //        {



            //            //var agent = db.Peoples.Where(a => peopleids.Contains(a.AgentCode) && a.Department == "Sales Executive").Select(a => new { a.PeopleID, a.DisplayName }).ToList();
            //            //var executiveid = agent.Select(x => x.PeopleID).ToList();


            //            //var orders = db.DbOrderMaster.Where(x => x.SalesPersonId.HasValue && peopleids.Contains(x.SalesPersonId.Value) && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Delivered") && EntityFunctions.TruncateTime(x.DeliveredDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(x.DeliveredDate) <= EntityFunctions.TruncateTime(end)).Select(x => new { x.OrderId, x.TotalAmount, x.Status, x.SalesPersonId,x.DeliveredDate,x.active,x.Deleted  }).ToList();


            //            var kpp = db.Warehouses.Where(w => w.active == true && w.Cityid == CityId && w.Deleted == false).Select(w => new { w.WarehouseId, w.WarehouseName }).ToList();
            //            var warehousids = kpp.Select(k => k.WarehouseId).ToList();

            //            var customers = db.Customers.Where(c => c.Warehouseid.HasValue && warehousids.Contains(c.Warehouseid.Value) && EntityFunctions.TruncateTime(c.CreatedDate) >= EntityFunctions.TruncateTime(start) && EntityFunctions.TruncateTime(c.CreatedDate) <= EntityFunctions.TruncateTime(end)).Select(c => new { c.CustomerId, c.IsSignup, c.Active, c.Warehouseid, c.Deleted }).ToList();

            //            var salesListDelPerc = db.DbOrderMaster.Where(x => x.active && !x.Deleted && warehousids.Contains(x.WarehouseId) && x.DeliveredDate.HasValue)
            //             .Select(x => new { x.WarehouseId, x.TotalAmount, x.Skcode, x.active, x.Deleted, x.Status, x.DeliveredDate, x.OrderId }).ToList();

            //            var salesListW = db.DbOrderMaster.Where(x => x.active && !x.Deleted && warehousids.Contains(x.WarehouseId) && x.DeliveredDate.HasValue && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Delivered"))
            //               .Select(x => new { x.TotalAmount, x.Skcode, x.active, x.Deleted, x.Status, x.DeliveredDate, x.OrderId, x.WarehouseId }).ToList();

            //            var salesListCancel = db.DbOrderMaster.Where(x => x.active && !x.Deleted && warehousids.Contains(x.WarehouseId) && x.DeliveredDate.HasValue && (x.Status == "Delivery Canceled" || x.Status == "Order Canceled" || x.Status == "Post Order Canceled"))
            //             .Select(x => new { x.WarehouseId, x.TotalAmount, x.Skcode, x.active, x.Deleted, x.Status, x.DeliveredDate, x.OrderId }).ToList();


            //            var customerIdsw = customers.Select(z => z.CustomerId).ToList();


            //            var orderDispatchListW = db.OrderDispatchedDetailss.Where(x => customerIdsw.Contains(x.CustomerId) /*&& x.Status == "Account settled"*/ && x.Deleted == false)
            //                                   .Select(x => new
            //                                   {
            //                                       ItemId = x.ItemId,
            //                                       TotalAmt = x.TotalAmt,
            //                                       CustomerId = x.CustomerId

            //                                   }).ToList();

            //            var itemIds = orderDispatchListW.Select(z => z.ItemId).ToList();


            //            var itemList = db.itemMasters.AsQueryable().Where(x => itemIds.Contains(x.ItemId)).Select(x => new { x.SubsubcategoryName, x.ItemId }).Distinct().ToList();
            //            var subCatListW = itemList.Select(z => z.SubsubcategoryName).ToList();

            //            var orderIdsW = salesListW.Select(z => z.OrderId).ToList();
            //            var itemCommisionPercentListW = db.SubsubCategorys.Where(x => subCatListW.Contains(x.SubsubcategoryName)).Select(x => new { x.AgentCommisionPercent, x.SubsubcategoryName }).ToList();

            //            #region Get Data From SPs

            //            var orderIdDtw = new DataTable();
            //            orderIdDtw.Columns.Add("IntValue");
            //            foreach (var item in orderIdsW)
            //            {
            //                var dr = orderIdDtw.NewRow();
            //                dr["IntValue"] = item;
            //                orderIdDtw.Rows.Add(dr);
            //            }

            //            var paramw = new SqlParameter("param", orderIdDtw);
            //            paramw.SqlDbType = SqlDbType.Structured;
            //            paramw.TypeName = "dbo.IntValues";
            //            var ordHistoriesw = db.Database.SqlQuery<OrderHistories>("exec GetOrderHistories @param", paramw).ToList();

            //            #endregion







            //            //foreach (var k in kpp)
            //            var resultw = new ConcurrentBag<listchannelpartner>();

            //            //ParallelLoopResult loopResult = Parallel.ForEach(peopl, (l) =>
            //            //{


            //            //});


            //            ParallelLoopResult loopResultw = Parallel.ForEach(kpp, (k) =>
            //            {

            //                var totalDelivered = salesListW.Where(x => x.WarehouseId == k.WarehouseId).Count();
            //                var totalCancelled = salesListW.Where(x => (x.Status == "Delivery Canceled" || x.Status == "Order Canceled" || x.Status == "Post Order Canceled") && x.WarehouseId == k.WarehouseId).Count();



            //                listchannelpartner abk = new listchannelpartner();
            //                abk.agents = k.WarehouseName;
            //                abk.sales = salesListW.Where(x => x.WarehouseId == k.WarehouseId && x.DeliveredDate.Value.Date >= start.Date && x.DeliveredDate.Value.Date <= end.Date).Sum(x => x.TotalAmount);

            //                abk.custactive = salesListW.Where(x => x.WarehouseId == k.WarehouseId && x.active == true && x.Deleted == false && x.DeliveredDate.Value.Date >= start.Date && x.DeliveredDate.Value.Date <= end.Date).Select(x => new { x.Skcode }).Distinct().Count();

            //                abk.custsignedup = customers.Where(x => x.Warehouseid == k.WarehouseId && x.IsSignup == true && x.Deleted == false).Count(); /*db.Customers.Where(x => x.ExecutiveId == l.PeopleID && x.IsSignup == true && x.Deleted == false).Select(x => new { x.CustomerId, x.IsSignup, x.Active, x.ExecutiveId, x.Deleted }).Count();*/



            //                var countD = salesListDelPerc.Where(x => x.WarehouseId == k.WarehouseId && x.DeliveredDate.Value.Date >= start.Date && x.DeliveredDate.Value.Date <= end.Date).Sum(x => x.TotalAmount);
            //                var count = salesListCancel.Where(x => x.WarehouseId == k.WarehouseId && x.DeliveredDate.Value.Date >= start.Date && x.DeliveredDate.Value.Date <= end.Date).Sum(x => x.TotalAmount);
            //                //var b = abl.DeliveryPerc
            //                double finalD = countD - count;
            //                double final = finalD / countD * 100;
            //                abk.DeliveryPerc = final;
            //                if (abk.DeliveryPerc.ToString() == "NaN")
            //                {
            //                    abk.DeliveryPerc = 0;

            //                }
            //                double countC = salesListW.Where(x => x.WarehouseId == k.WarehouseId && x.DeliveredDate.Value.Date >= start.Date && x.DeliveredDate.Value.Date <= end.Date).Sum(x => x.TotalAmount);
            //                var finalcancel = countD - countC;
            //                double countFi = finalcancel / countD * 100;
            //                abk.CancelPerc = countFi;

            //                if (abk.CancelPerc.ToString() == "NaN")
            //                {
            //                    abk.CancelPerc = 0;

            //                }


            //                try
            //                {

            //                    var com = abk.sales * 2 / 100;
            //                    b.totalcommission = b.totalcommission + Convert.ToDouble(com);
            //                    abk.totCom = com;

            //                }
            //                catch (Exception ex)
            //                {

            //                }


            //                var orderid = salesListW.Where(x => x.WarehouseId == k.WarehouseId && x.DeliveredDate.Value.Date >= start.Date && x.DeliveredDate.Value.Date <= end.Date).ToList();
            //                TimeSpan diffResult = new TimeSpan();
            //                if (orderid != null && orderid.Any())
            //                {


            //                    foreach (var history in ordHistoriesw.Where(x => (x.Status == "Issued" || x.Status == "Delivered") && orderid.Select(z => z.OrderId).Contains(x.orderid)).GroupBy(x => x.orderid))
            //                    {
            //                        if (history.Any(z => z.Status == "Issued") && history.Any(z => z.Status == "Delivered"))
            //                        {
            //                            var rDDate = history.FirstOrDefault(x => x.Status == "Issued").CreatedDate;
            //                            var DDate = history.FirstOrDefault(x => x.Status == "Delivered").CreatedDate;
            //                            diffResult += DDate - rDDate;
            //                        }
            //                    }


            //                }

            //                abk.totTat = diffResult.ToString(@"hh\:mm");
            //                resultw.Add(abk);
            //            });

            //            if (loopResultw.IsCompleted)
            //                ab = resultw.ToList();



            //        }


            //    }

            //    return ab;
            //}


            //catch (Exception ex)
            //{
            //    logger.Error("Error in Customer " + ex.ToString());
            //    logger.Info("End  Customer: ");
            //    return 0;
            //}

        }

        [Route("GetKPP")]
        [HttpGet]
        public dynamic GetKPP()
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
            using (var db = new AuthContext())
            {

                var kpp = db.Warehouses.Where(x => x.active && !x.Deleted);
                return kpp;
            }


        }

        [Route("Chart")]
        [HttpGet]
        public dynamic GetChartDetail(string type, DateTime start, DateTime end, int cityid)
        {
            List<agentChartData> agentChartDatas = new List<Controllers.agentChartData>();
            using (AuthContext db = new AuthContext())
            {
                if (type == "Sales")
                {

                    //var peopl = db.Peoples.Where(p => p.Active == true && p.Type == "Sales Executive" && p.Cityid == cityid).Select(x => new { x.PeopleID, x.DisplayName, x.CreatedDate }).Distinct().ToList();


                    string query = "select distinct p.PeopleID,p.DisplayName, p.CreatedDate from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Cityid=" + cityid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    var peopl = db.Database.SqlQuery<People>(query).ToList();


                    var peopleids = peopl.Select(x => x.PeopleID).ToList();
                    string strpeopleids = string.Join(",", peopleids);






                    var startmonth = start.Month;
                    var startyear = start.Year;

                    var endmonth = end.Month;
                    var endyear = end.Year;
                    var sql = "select SalesPersonId,month(CreatedDate) mm,year(CreatedDate) yy,sum(totalamount) TotalAmount from ordermasters where  SalesPersonId in (" + strpeopleids + ") and active=1 and "
                              + " month(CreatedDate) >= " + startmonth + " and year(CreatedDate)= " + startyear + " and month(CreatedDate) <= " + endmonth + " and year(CreatedDate)= " + endyear + "  group by SalesPersonId,month(CreatedDate),year(CreatedDate)";

                    var data = db.Database.SqlQuery<agentSaleData>(sql).ToList();

                    int i = 1;
                    while (i <= 12)
                    {
                        agentChartData agentChartData = new agentChartData();
                        agentChartData.type = "stackedColumn100";
                        agentChartData.showInLegend = true;
                        agentChartData.yValueFormatString = "#,##0.##";
                        agentChartData.name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);





                        agentChartData.dataPoints = new List<agentData>();

                        foreach (var item in peopl)
                        {
                            agentChartData.dataPoints.Add(new agentData
                            {
                                label = item.DisplayName,
                                y = data.Any(x => x.mm == i && x.SalesPersonId == item.PeopleID) ? data.FirstOrDefault(x => x.mm == i && x.SalesPersonId == item.PeopleID).TotalAmount : 0
                            });

                        }

                        agentChartDatas.Add(agentChartData);
                        i++;
                    }




                    return agentChartDatas;

                }
                else if (type == "Cancellation")
                {
                    // var peopl = db.Peoples.Where(p => p.Active == true && p.Type == "Sales Executive" && p.Cityid == cityid).Select(x => new { x.PeopleID, x.DisplayName, x.CreatedDate }).Distinct().ToList();

                    string query = "select distinct p.PeopleID,p.DisplayName, p.CreatedDate from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.Cityid=" + cityid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    var peopl = db.Database.SqlQuery<People>(query).ToList();



                    var peopleids = peopl.Select(x => x.PeopleID).ToList();
                    string strpeopleids = string.Join(",", peopleids);






                    var startmonth = start.Month;
                    var startyear = start.Year;

                    var endmonth = end.Month;
                    var endyear = end.Year;
                    var sql = "select SalesPersonId,month(CreatedDate) mm,year(CreatedDate) yy,sum(totalamount) TotalAmount from ordermasters where  SalesPersonId in (" + strpeopleids + ") and active=1 and "
                              + " month(CreatedDate) >= " + startmonth + " and year(CreatedDate)= " + startyear + " and month(CreatedDate) <= " + endmonth + " and year(CreatedDate)= " + endyear + " and (Status='Order Canceled' or Status = 'Post Order Canceled' or Status = 'Delivery Canceled')  group by SalesPersonId,month(CreatedDate),year(CreatedDate)";

                    var data = db.Database.SqlQuery<agentSaleData>(sql).ToList();

                    int i = 1;
                    while (i <= 12)
                    {
                        agentChartData agentChartData = new agentChartData();
                        agentChartData.type = "stackedColumn100";
                        agentChartData.showInLegend = true;
                        agentChartData.yValueFormatString = "#,##0.##";
                        agentChartData.name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);





                        agentChartData.dataPoints = new List<agentData>();

                        foreach (var item in peopl)
                        {
                            agentChartData.dataPoints.Add(new agentData
                            {
                                label = item.DisplayName,
                                y = data.Any(x => x.mm == i && x.SalesPersonId == item.PeopleID) ? data.FirstOrDefault(x => x.mm == i && x.SalesPersonId == item.PeopleID).TotalAmount : 0
                            });

                        }

                        agentChartDatas.Add(agentChartData);
                        i++;
                    }






                }

                return agentChartDatas;
            }
        }

        [Route("Executive")]
        [HttpGet]
        public dynamic GetExec(int clstid)
        {
            using (AuthContext db = new AuthContext())
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

                string query = "select distinct p.PeopleID,p.DisplayName from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id inner join Customers c on p.PeopleID =c.ExecutiveId where c.ClusterId=" + clstid + " and r.Name='Sales Executive' and c.Active =1 and c.Deleted = 0 and ur.isActive=1 and p.Active=1 and p.Deleted=0 ";
                var Exec = db.Database.SqlQuery<People>(query).ToList();

                //var Exec = (from e in db.Peoples
                //            join c in db.Customers on e.PeopleID equals c.ExecutiveId
                //            where c.ClusterId == clstid && c.Active == true && c.Deleted == false && e.Type == "Sales Executive"
                //            select new
                //            {

                //                e.DisplayName,
                //                e.PeopleID


                //            }).Distinct().ToList();
                return Exec;

            }
        }


        [Route("GetBeats")]
        [HttpGet]

        public dynamic GetBeats(int cityId, int clusterId, int agentCode)
        {
            return 0;
            //List<listAgentPerformance> lstAgentPerfr = new List<listAgentPerformance>();
            //bound b = new Controllers.ChannelPartnerController.bound();
            //logger.Info("start City: ");
            //Customer customer = new Customer();
            //try
            //{
            //    var identity = User.Identity as ClaimsIdentity;
            //    int compid = 0, userid = 0;
            //    foreach (Claim claim in identity.Claims)
            //    {
            //        if (claim.Type == "compid")
            //        {
            //            compid = int.Parse(claim.Value);
            //        }
            //        if (claim.Type == "userid")
            //        {
            //            userid = int.Parse(claim.Value);
            //        }
            //    }
            //    using (AuthContext db = new AuthContext())
            //    {
            //        int CompanyId = compid;
            //        DateTime now = DateTime.Now;
            //        var startDate = new DateTime(now.Year, now.Month, 1);
            //        var endDate = startDate.AddMonths(1).AddDays(-1);

            //        DateTime lM = DateTime.Now.AddMonths(-1).AddYears(-1);
            //        int lMonth = lM.Month;
            //        int lYear = lM.Year;

            //        DateTime yesterday = DateTime.Now.AddDays(-1);
            //        int ydMonth = yesterday.Month;
            //        int ydYear = yesterday.Year;
            //        int ydDay = yesterday.Day;

            //        b.TotalCustomersInMonth = 0;



            //        var peopleList = db.Peoples.Where(x => x.Cityid == cityId && x.PeopleID == agentCode && x.Active == true && x.Deleted == false)
            //                                       .Select(x => new { x.PeopleID, x.Active, x.Deleted, x.Type }).ToList();

            //        var peopleids = peopleList.Select(x => x.PeopleID).ToList();

            //        //var agent = db.Peoples.Where(a => peopleids.Contains(a.PeopleID) && a.Department == "Sales Executive").Select(a => new { a.PeopleID, a.DisplayName }).ToList();
            //        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
            //        var agent = db.Database.SqlQuery<People>(query).ToList();

            //        var executiveid = agent.Select(x => x.PeopleID).ToList();

            //        var customers = db.Customers.Where(x => x.ExecutiveId == agentCode).
            //            Select(x => new { x.CustomerId, x.Active, x.ExecutiveId, x.Deleted, x.BeatNumber }).ToList();



            //        var salesList = db.DbOrderMaster.Where(x => x.active && !x.Deleted && x.SalesPersonId == agentCode && x.DeliveredDate.HasValue && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Delivered"))
            //            .Select(x => new { x.SalesPersonId, x.TotalAmount, x.Skcode, x.active, x.Deleted, x.Status, x.DeliveredDate, x.OrderId }).ToList();

            //        var customerList = db.Customers.Where(x => x.ExecutiveId == agentCode)
            //                                .Select(x => new { x.CustomerId, x.IsSignup, x.Active, x.ExecutiveId, x.Deleted }).ToList();

            //        var customerIds = customerList.Select(z => z.CustomerId).ToList();

            //        var orderDispatchList = db.OrderDispatchedDetailss.Where(x => customerIds.Contains(x.CustomerId) /*&& x.Status == "Account settled"*/ && x.Deleted == false)
            //                                .Select(x => new
            //                                {
            //                                    ItemId = x.ItemId,
            //                                    TotalAmt = x.TotalAmt,
            //                                    CustomerId = x.CustomerId

            //                                }).ToList();

            //        var itemIds = orderDispatchList.Select(z => z.ItemId).ToList();

            //        var itemList = db.itemMasters.AsQueryable().Where(x => itemIds.Contains(x.ItemId)).Select(x => new { x.SubsubcategoryName, x.ItemId }).Distinct().ToList();
            //        var subCatList = itemList.Select(z => z.SubsubcategoryName).ToList();

            //        var orderIds = salesList.Select(z => z.OrderId).ToList();
            //        var itemCommisionPercentList = db.SubsubCategorys.Where(x => subCatList.Contains(x.SubsubcategoryName)).Select(x => new { x.AgentCommisionPercent, x.SubsubcategoryName }).ToList();




            //        #region Get Data From SP

            //        var orderIdDt = new DataTable();
            //        orderIdDt.Columns.Add("IntValue");
            //        foreach (var item in orderIds)
            //        {
            //            var dr = orderIdDt.NewRow();
            //            dr["IntValue"] = item;
            //            orderIdDt.Rows.Add(dr);
            //        }

            //        var param = new SqlParameter("param", orderIdDt);
            //        param.SqlDbType = SqlDbType.Structured;
            //        param.TypeName = "dbo.IntValues";
            //        var ordHistories = db.Database.SqlQuery<OrderHistories>("exec GetOrderHistories @param", param).ToList();

            //        #endregion
            //        var result = new ConcurrentBag<listAgentPerformance>();




            //        ParallelLoopResult loopResult = Parallel.ForEach(customers, (l) =>
            //        {
            //            var totalDelivered = salesList.Where(x => x.SalesPersonId == l.ExecutiveId).Count();
            //            var totalCancelled = salesList.Where(x => (x.Status == "Delivery Canceled" || x.Status == "Order Canceled" || x.Status == "Post Order Canceled") && x.SalesPersonId == l.ExecutiveId).Count();


            //            listAgentPerformance abl = new listAgentPerformance();
            //            abl.agents = "Beat" + " " + l.BeatNumber.ToString();

            //            abl.sales = salesList.Where(x => x.SalesPersonId == l.ExecutiveId).Sum(x => x.TotalAmount);

            //            abl.custactive = salesList.Where(x => x.SalesPersonId == l.ExecutiveId && x.active == true && x.Deleted == false).Select(x => new { x.Skcode }).Distinct().Count();


            //            abl.custsignedup = customerList.Where(x => x.ExecutiveId == l.ExecutiveId && x.IsSignup == true && x.Deleted == false).Count(); /*db.Customers.Where(x => x.ExecutiveId == l.PeopleID && x.IsSignup == true && x.Deleted == false).Select(x => new { x.CustomerId, x.IsSignup, x.Active, x.ExecutiveId, x.Deleted }).Count();*/

            //            double count = salesList.Where(x => x.SalesPersonId.Value == l.ExecutiveId && (x.Status == "sattled" || x.Status == "Account settled" || x.Status == "Delivered")).Count();
            //            //var b = abl.DeliveryPerc
            //            double final = count / totalDelivered * 100;
            //            abl.DeliveryPerc = final;
            //            if (abl.DeliveryPerc.ToString() == "NaN")
            //            {
            //                abl.DeliveryPerc = 0;

            //            }
            //            double countC = salesList.Where(x => x.SalesPersonId.Value == l.ExecutiveId && (x.Status == "Delivery Canceled" || x.Status == "Order Canceled" || x.Status == "Post Order Canceled")).Count();
            //            double countFi = countC / totalCancelled * 100;
            //            abl.CancelPerc = countFi;

            //            if (abl.CancelPerc.ToString() == "NaN")
            //            {
            //                abl.CancelPerc = 0;

            //            }

            //            var custom = customerList.Where(x => x.ExecutiveId == l.ExecutiveId).Select(x => new { x.CustomerId }).ToList();

            //            //if (custom != null)
            //            //{
            //            //    foreach (var cst in custom)
            //            //    {
            //            //        var agentMapeddata = orderDispatchList.Where(x => x.CustomerId == cst.CustomerId).ToList();
            //            //        var totalAmount = 0;
            //            //        var commisionamount = 0;
            //            //        foreach (var a in agentMapeddata)
            //            //        {
            //            //            var getItem = itemList.Where(x => x.ItemId == a.ItemId).Select(x => x.SubsubcategoryName).SingleOrDefault();

            //            //            try
            //            //            {
            //            //                var commission = itemCommisionPercentList.FirstOrDefault(x => x.SubsubcategoryName == getItem).AgentCommisionPercent;
            //            //                var com = Convert.ToDouble(a.TotalAmt) * commission / 100;
            //            //                b.totalcommission = b.totalcommission + Convert.ToDouble(com);
            //            //                abl.totCom = b.totalcommission;

            //            //            }
            //            //            catch (Exception ex)
            //            //            {

            //            //            }
            //            //        }
            //            //    }
            //            //}
            //            //var orderid = salesList.Where(x => x.SalesPersonId == l.ExecutiveId).ToList();

            //            //TimeSpan diffResult = new TimeSpan();

            //            //foreach (var history in ordHistories.Where(x => x.Status == "Issued" && x.Status == "Delivered" && orderid.Select(z => z.OrderId).Contains(x.orderid)).GroupBy(x => x.orderid))
            //            //{
            //            //    var rDDate = history.FirstOrDefault(x => x.Status == "Issued").CreatedDate;
            //            //    var DDate = history.FirstOrDefault(x => x.Status == "Delivered").CreatedDate;
            //            //    diffResult = DDate - rDDate;
            //            //}




            //            //abl.totTat = diffResult.ToString(@"hh\:mm");
            //            if (abl.custactive != 0 && abl.custsignedup != 0)
            //            {

            //                result.Add(abl);
            //            }
            //        });

            //        if (loopResult.IsCompleted)
            //            lstAgentPerfr = result.ToList();

            //    }












            //    return lstAgentPerfr;
            //}


            //catch (Exception ex)
            //{
            //    logger.Error("Error in Customer " + ex.ToString());
            //    logger.Info("End  Customer: ");
            //    return 0;
            //}

        }





        #region SPA: Add Driver

        /// <summary>
        /// created by 20/Jun/2019
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        [ResponseType(typeof(People))]
        [Route("addDriver")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addDriver(People driver)
        {
            using (AuthContext db = new AuthContext())
            {

                driverDetail res;
                logger.Info("start addCustomer: ");
                try
                {
                    People c = new People();



                    var p = db.Peoples.Where(x => x.PeopleID == driver.PeopleID).ToList();


                    if (p.Count() > 0)
                    {
                        res = new driverDetail()
                        {
                            drivers = c,
                            Status = false,
                            Message = "Data Already Exists For This User"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }

                    else
                    {




                        c.CompanyId = 1;
                        c.WarehouseId = 0;
                        c.DisplayName = driver.DisplayName;
                        c.Mobile = driver.Mobile;
                        c.BillableRate = 0.00;
                        c.EmailConfirmed = false;
                        c.Approved = false;
                        c.Active = true;
                        c.CreatedDate = DateTime.Now;
                        c.UpdatedDate = DateTime.Now;
                        c.VehicleId = 0;
                        c.VehicleCapacity = 0.0;
                        c.AgentAmount = 0.0m;
                        c.Salary = 0;
                        c.Account_Number = 0;
                        c.DepositAmount = 0.00;
                        c.Type = "Driver";
                        c.Desgination = "Driver";


                        c.IdProof = driver.IdProof;
                        c.pVerificationCopy = driver.pVerificationCopy;
                        c.AddressProof = driver.AddressProof;
                        c.AgentCode = driver.AgentCode;



                        db.Peoples.Add(c);
                        db.Commit();
                    }

                    logger.Info("End  addDriver: ");
                    //#region call to whatsapp Bot 
                    //try
                    //{
                    //    Customer cust = new Customer();
                    //    cust.ClusterId = 8;//send for template
                    //    cust.Name = customer.Name;
                    //    cust.Mobile = customer.Mobile;
                    //    cust.BillingAddress = customer.Mobile;//send for Mobile
                    //    cust.CustomerId = Convert.ToInt32(customer.Password);//send for Order Id
                    //    db.Customersms(cust);
                    //}
                    //catch (Exception ex) { }
                    //#endregion
                    res = new driverDetail()
                    {
                        drivers = c,
                        Status = true,
                        Message = "Registration successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new driverDetail()
                    {
                        drivers = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }

        #endregion

        #region SPA: Add DeliveryBoy

        /// <summary>
        /// created by 20/Jun/2019
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        [ResponseType(typeof(People))]
        [Route("addDeliveryBoy")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addDeliveryBoy(People delboy)
        {
            using (AuthContext db = new AuthContext())
            {
                delBoyDetails res;
                logger.Info("start addCustomer: ");
                try
                {
                    People c = new People();

                    var p = db.Peoples.Where(x => x.PeopleID == delboy.PeopleID).ToList();

                    if (p.Count() > 0)
                    {
                        res = new delBoyDetails()
                        {
                            delBoy = c,
                            Status = false,
                            Message = "Data Already Exists For This User"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }
                    else
                    {


                        logger.Info("End  addDriver: ");

                        c.CompanyId = 1;
                        c.WarehouseId = 0;
                        c.DisplayName = delboy.DisplayName;
                        c.Mobile = delboy.Mobile;
                        c.BillableRate = 0.00;
                        c.EmailConfirmed = false;
                        c.Approved = true;
                        c.Active = true;
                        c.CreatedDate = DateTime.Now;
                        c.UpdatedDate = DateTime.Now;
                        c.VehicleId = 0;
                        c.VehicleCapacity = 0.0;
                        c.AgentAmount = 0.0m;
                        c.Salary = 0;
                        c.Account_Number = 0;
                        c.DepositAmount = 0.00;
                        c.Type = "Delivery Boy";
                        c.Desgination = "Delivery Boy";


                        c.IdProof = delboy.IdProof;
                        c.pVerificationCopy = delboy.pVerificationCopy;
                        c.AddressProof = delboy.AddressProof;
                        c.AgentCode = delboy.AgentCode;



                        db.Peoples.Add(c);
                        db.Commit();
                    }

                    //#region call to whatsapp Bot 
                    //try
                    //{
                    //    Customer cust = new Customer();
                    //    cust.ClusterId = 8;//send for template
                    //    cust.Name = customer.Name;
                    //    cust.Mobile = customer.Mobile;
                    //    cust.BillingAddress = customer.Mobile;//send for Mobile
                    //    cust.CustomerId = Convert.ToInt32(customer.Password);//send for Order Id
                    //    db.Customersms(cust);
                    //}
                    //catch (Exception ex) { }
                    //#endregion
                    res = new delBoyDetails()
                    {
                        delBoy = c,
                        Status = true,
                        Message = "Registration successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new delBoyDetails()
                    {
                        delBoy = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion

        #region SPA: Add Vehicle

        /// <summary>
        /// created by 20/Jun/2019
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        [ResponseType(typeof(Vehicle))]
        [Route("addVehicle")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addVehicle(Vehicle vehicle)
        {
            using (AuthContext db = new AuthContext())
            {
                vehicleDetail res;
                logger.Info("start addCustomer: ");
                try
                {
                    Vehicle c = new Vehicle();

                    var veh = db.VehicleDb.Where(x => x.agent == vehicle.agent).ToList();

                    if (veh.Count() > 0)
                    {
                        res = new vehicleDetail()
                        {
                            vehicle = c,
                            Status = false,
                            Message = "Data Already Exists For This Vehicle."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }



                    else
                    {

                        logger.Info("End  addDriver: ");

                        c.VehicleNumber = vehicle.VehicleNumber;
                        c.UploadRegistration = vehicle.UploadRegistration;
                        c.agent = vehicle.agent;
                        c.Capacity = 0.00;
                        c.isActive = true;
                        c.CompanyId = 1;
                        c.Cityid = 0;
                        c.Stateid = 0;
                        c.WarehouseId = 0;
                        c.CreatedDate = DateTime.Now;
                        c.UpdatedDate = DateTime.Now;
                        c.isDeleted = false;


                        db.VehicleDb.Add(c);
                        db.Commit();
                    }

                    //#region call to whatsapp Bot 
                    //try
                    //{
                    //    Customer cust = new Customer();
                    //    cust.ClusterId = 8;//send for template
                    //    cust.Name = customer.Name;
                    //    cust.Mobile = customer.Mobile;
                    //    cust.BillingAddress = customer.Mobile;//send for Mobile
                    //    cust.CustomerId = Convert.ToInt32(customer.Password);//send for Order Id
                    //    db.Customersms(cust);
                    //}
                    //catch (Exception ex) { }
                    //#endregion
                    res = new vehicleDetail()
                    {
                        vehicle = c,
                        Status = true,
                        Message = "Registration successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new vehicleDetail()
                    {
                        vehicle = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion

        #region SPA: Add SecurityDetails

        /// <summary>
        /// created by 20/Jun/2019
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        [ResponseType(typeof(SecurityDetails))]
        [Route("addSecurityDetails")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addSecurityDetails(SecurityDetails security)
        {
            using (AuthContext db = new AuthContext())
            {
                security res;
                logger.Info("start addSecurityDetails: ");
                try
                {
                    SecurityDetails c = new SecurityDetails();

                    var sec = db.securityDetails.Where(x => x.Agent == security.Agent).ToList();
                    if (sec.Count() > 0)
                    {
                        res = new security()
                        {
                            securi = c,
                            Status = false,
                            Message = "Data Already Exists For This Security."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }

                    else
                    {

                        logger.Info("End  SecurityDetails: ");

                        c.Agent = security.Agent;
                        c.AmountDeposited = security.AmountDeposited;
                        c.NeftChequeNumber = security.NeftChequeNumber;
                        c.DateofTransfer = security.DateofTransfer;
                        c.FromAccount = security.FromAccount;
                        c.ToAccount = security.ToAccount;
                        c.DateofRegistration = security.DateofRegistration;
                        c.DateofJoining = security.DateofJoining;
                        c.createdDate = DateTime.Now;
                        c.BeatAlloted = security.BeatAlloted;
                        c.Active = true;
                        c.Deleted = false;




                        db.securityDetails.Add(c);
                        db.Commit();

                    }
                    //#region call to whatsapp Bot 
                    //try
                    //{
                    //    Customer cust = new Customer();
                    //    cust.ClusterId = 8;//send for template
                    //    cust.Name = customer.Name;
                    //    cust.Mobile = customer.Mobile;
                    //    cust.BillingAddress = customer.Mobile;//send for Mobile
                    //    cust.CustomerId = Convert.ToInt32(customer.Password);//send for Order Id
                    //    db.Customersms(cust);
                    //}
                    //catch (Exception ex) { }
                    //#endregion
                    res = new security()
                    {
                        securi = c,
                        Status = true,
                        Message = "Registration successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new security()
                    {
                        securi = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion


        #region SPA: Add Training & Development

        /// <summary>
        /// created by 20/Jun/2019
        /// </summary>
        /// <param name="training"></param>
        /// <returns></returns>
        [ResponseType(typeof(TrainingDevelopment))]
        [Route("addTraining")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addTraining(TrainingDevelopment training)
        {
            using (AuthContext db = new AuthContext())
            {
                TrainDev res;
                logger.Info("start training: ");
                try
                {
                    TrainingDevelopment c = new TrainingDevelopment();


                    var t = db.trainingDevelopment.Where(x => x.Agent == training.Agent).ToList();
                    if (t.Count() > 0)
                    {
                        res = new TrainDev()
                        {
                            traind = c,
                            Status = false,
                            Message = "Data Already Exists."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);


                    }
                    else
                    {


                        logger.Info("End  training: ");

                        c.Agent = training.Agent;
                        c.SalesTraining = training.SalesTraining;
                        c.DeliveryTraining = training.DeliveryTraining;
                        c.CommissionStructure = training.CommissionStructure;
                        c.BehaviourTraining = training.BehaviourTraining;
                        c.createdDate = DateTime.Now;
                        c.Active = true;
                        c.Deleted = false;





                        db.trainingDevelopment.Add(c);
                        db.Commit();
                    }

                    //#region call to whatsapp Bot 
                    //try
                    //{
                    //    Customer cust = new Customer();
                    //    cust.ClusterId = 8;//send for template
                    //    cust.Name = customer.Name;
                    //    cust.Mobile = customer.Mobile;
                    //    cust.BillingAddress = customer.Mobile;//send for Mobile
                    //    cust.CustomerId = Convert.ToInt32(customer.Password);//send for Order Id
                    //    db.Customersms(cust);
                    //}
                    //catch (Exception ex) { }
                    //#endregion
                    res = new TrainDev()
                    {
                        traind = c,
                        Status = true,
                        Message = "Registration successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addCustomer " + ex.Message);
                    res = new TrainDev()
                    {
                        traind = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion

        #region SPA: Add AgentLicense

        /// <summary>
        /// created by 01/Jul/2019
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        [ResponseType(typeof(AgentLicense))]
        [Route("addlicense")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage addlicense(AgentLicense license)
        {
            using (AuthContext db = new AuthContext())
            {
                agetntlic res;
                logger.Info("start training: ");
                try
                {
                    AgentLicense c = new AgentLicense();


                    var li = db.agentLicense.Where(x => x.AgentId == license.AgentId).ToList();
                    if (li.Count() > 0)
                    {


                        res = new agetntlic()
                        {
                            license = c,
                            Status = false,
                            Message = "Data Already Exists"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }
                    else
                    {


                        logger.Info("End  addlicense: ");

                        c.AgentId = license.AgentId;
                        c.LicenseNumber = license.LicenseNumber;
                        c.LicenseImage = license.LicenseImage;
                        c.createdDate = DateTime.Now;
                        c.Active = true;
                        c.Deleted = false;





                        db.agentLicense.Add(c);
                        db.Commit();
                    }

                    //#region call to whatsapp Bot 
                    //try
                    //{
                    //    Customer cust = new Customer();
                    //    cust.ClusterId = 8;//send for template
                    //    cust.Name = customer.Name;
                    //    cust.Mobile = customer.Mobile;
                    //    cust.BillingAddress = customer.Mobile;//send for Mobile
                    //    cust.CustomerId = Convert.ToInt32(customer.Password);//send for Order Id
                    //    db.Customersms(cust);
                    //}
                    //catch (Exception ex) { }
                    //#endregion
                    res = new agetntlic()
                    {
                        license = c,
                        Status = true,
                        Message = "Registration successfully."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    logger.Error("Error in addlicense " + ex.Message);
                    res = new agetntlic()
                    {
                        license = null,
                        Status = false,
                        Message = "something went wrong."
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion


        #region SPA: GetAgents

        /// <summary>
        /// Created by Pravesh 27/06/2019
        /// Get agents percentage and names 
        /// </summary>
        /// <param name="WarehouseId"></param>
        /// <returns></returns>
        [Route("GetAgentPercent")]
        [HttpGet]

        public HttpResponseMessage GetAgentPercent(int warehouseId)
        {
            List<agentpercent> resultList = new List<agentpercent>();
            AgntCompleted objagnt = new AgntCompleted();
            //var agentid = agents.id;
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
                using (AuthContext db = new AuthContext())
                {

                    //List<int> agentid = db.Peoples.Where(x => x.WarehouseId == warehouseId && x.Active).Select(x => x.PeopleID).ToList();

                    var query = "SELECT Agent,DBoyName, ColumnName,  ColumnValue FROM  ( SELECT p.PeopleID agent,p.DisplayName as DBoyName, isnull(v.vehiclenumber,'') vehiclenumber,  isnull(v.UploadRegistration,'') UploadRegistration, isnull(SalesTraining,'') SalesTraining, isnull(DeliveryTraining,'') DeliveryTraining, "
                                + " isnull(CommissionStructure,'') CommissionStructure, isnull(BehaviourTraining,'') BehaviourTraining, ISNULL(p.DisplayName,'') DisplayName, isnull(p.mobile,'')mobile,isnull(p.IdProof,'') IdProof,isnull(p.AddressProof,'') AddressProof,isnull(pVerificationCopy,'') pVerificationCopy, "
                                + " convert(nvarchar(max), isnull(s.AmountDeposited,'')) as AmountDeposited, ISNULL(s.NeftChequeNumber,'') as NeftChequeNumber,convert(nvarchar(max),   isnull(s.DateofTransfer,'')) as DateofTransfer,ISNULL(s.FromAccount,'') as FromAccount,ISNULL(s.ToAccount,'') as ToAccount,   convert(nvarchar(max),  "
                                + " isnull(s.DateofRegistration,'')) as DateofRegistration , convert(nvarchar(max), isnull(s.DateofJoining,'')) as DateofJoining, ISNULL(s.BeatAlloted,'') BeatAlloted   FROM people p 	left join Vehicles v on v.agent = p.PeopleID and  v.agent > 0 and v.isActive=1	left join TrainingDevelopments t on v.agent = t.Agent  and t.Active=1 "
                                + " left join 	SecurityDetails s on v.agent =s.Agent and s.Active =1  where p.WarehouseId=" + warehouseId + " and p.Active =1	) p UNPIVOT (ColumnValue FOR ColumnName IN    "
                                + " (vehiclenumber, UploadRegistration, SalesTraining, DeliveryTraining, CommissionStructure, BehaviourTraining,DisplayName,mobile,IdProof,AddressProof,pVerificationCopy,AmountDeposited,NeftChequeNumber,DateofTransfer,FromAccount ,ToAccount,DateofRegistration,DateofJoining,BeatAlloted))AS unpvt ";

                    //var agentidDt = new DataTable();
                    //agentidDt.Columns.Add("IntValue");
                    //foreach (var item in agentid)
                    //{
                    //    var dr = agentidDt.NewRow();
                    //    dr["IntValue"] = item;
                    //    agentidDt.Rows.Add(dr);
                    //}

                    //var param = new SqlParameter("@agentids", agentidDt);
                    //param.SqlDbType = SqlDbType.Structured;
                    //param.TypeName = "dbo.IntValues";
                    var chartData = db.Database.SqlQuery<ChartData>(query).ToList();

                    //var columnNames = chartData.Select(x => new { x.ColumnName }).Distinct().ToList();

                    // var groupedByAgent = chartData.GroupBy(x => new { x.Agent, x.dboyName });

                    foreach (var item in chartData.GroupBy(x => new { x.Agent, x.dboyName }))
                    {
                        var agent = new agentpercent();
                        agent.dboyNames = item.Key.dboyName;
                        agent.Agent = item.Key.Agent;
                        double columncount = item.Distinct().Count();
                        double fillcolumncount = item.Where(x => x.ColumnValue != "" && x.ColumnValue != "Jan  1 1900 12:00AM").Count();

                        //var values = item.Where(x => x.ColumnValue != "").ToList();
                        //var incvalues = item.Where(x => x.ColumnValue == "").Select(x => x.ColumnName).ToList();
                        //var compValues = item.Where(x => x.ColumnValue != "").Select(x => x.ColumnName).ToList();
                        //agent.IncompleteColums = incvalues;
                        //agent.CompletColums = compValues;
                        //double columncount = columnNames.Count();
                        //double order = values.Count();
                        double difference = columncount - fillcolumncount;
                        double agentperc = (fillcolumncount / columncount) * 100.00;
                        agent.CompletePercent = Math.Round(agentperc, 2);

                        resultList.Add(agent);
                    }


                    if (query.Count() > 0)
                    {

                        objagnt = new AgntCompleted()
                        {
                            details = resultList,
                            Status = true,
                            Message = "Data  Found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, objagnt);
                    }
                    else
                    {
                        objagnt = new AgntCompleted()
                        {
                            details = resultList,
                            Status = true,
                            Message = "Data Not Found"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, objagnt);
                    }
                    //}

                }

            }


            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }
        #endregion

        /// <summary>
        /// Upload Image for documents
        /// </summary>
        /// <returns>Image path</returns>
        [Route("ChannelPartnerImage")]
        public async Task<HttpResponseMessage> ChannelPartnerImage()
        {


            using (AuthContext db = new AuthContext())
            {

                try
                {
                    string LogoUrl = "";
                    var fileuploadPath = HttpContext.Current.Server.MapPath("~/UploadedImages");

                    if (!Directory.Exists(fileuploadPath))
                    {
                        Directory.CreateDirectory(fileuploadPath);
                    }

                    var provider = new MultipartFormDataStreamProvider(fileuploadPath);
                    var content = new StreamContent(HttpContext.Current.Request.GetBufferlessInputStream(true));
                    foreach (var header in Request.Content.Headers)
                    {
                        content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    await content.ReadAsMultipartAsync(provider);

                    string uploadingFileName = provider.FileData.Select(x => x.LocalFileName).FirstOrDefault();
                    string originalFileName = String.Concat(fileuploadPath, "\\" + (provider.Contents[0].Headers.ContentDisposition.FileName).Trim(new Char[] { '"' }));

                    if (File.Exists(originalFileName))
                    {
                        File.Delete(originalFileName);
                    }

                    File.Move(uploadingFileName, originalFileName);

                    CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                    Cloudinary cloudinary = new Cloudinary(account);

                    string filename = Path.GetFileNameWithoutExtension(originalFileName);

                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(originalFileName),
                        PublicId = "ChannelPartnerImage/" + filename,
                        Overwrite = true,
                        Invalidate = true,
                        Backup = false
                    };

                    var uploadResult = cloudinary.Upload(uploadParams);

                    if (System.IO.File.Exists(fileuploadPath))
                    {
                        System.IO.File.Delete(fileuploadPath);
                    }

                    LogoUrl = uploadResult.SecureUri.ToString();

                    var response = new
                    {
                        LogoUrl = LogoUrl,
                        Status = true
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                catch (Exception ex)
                {
                    var response = new
                    {
                        Message = ex,
                        Status = false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }


        /// <summary>
        /// Created by Pravesh 19-07-2019
        /// Get Retailers connected 
        /// </summary>
        /// <param name="warehouseId"></param>
        /// <returns></returns>
        [Route("GetRetailersConnected")]
        [HttpGet]

        public HttpResponseMessage GetRetailersConnected(int warehouseId)
        {
            using (AuthContext db = new AuthContext())
            {

                Retailerconnected rtlr = new Retailerconnected();
                try
                {
                    var GetconnectedR = db.DbOrderMaster.Where(x => x.WarehouseId == warehouseId).Select(x => x.Skcode).Distinct().Count();

                    rtlr = new Retailerconnected()
                    {
                        RetailerCount = GetconnectedR,
                        Status = true
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, rtlr);
                }
                catch
                {
                    rtlr = new Retailerconnected()
                    {
                        RetailerCount = 0,
                        Status = false,
                        Message = "Data Not Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, rtlr);

                }
            }
        }

        public class Retailerconnected
        {
            public int RetailerCount { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }

        }

    }

    public class agentData
    {
        public double y { get; set; }
        public string label { get; set; }
    }

    public class agentChartData
    {
        public string type { get; set; }
        public bool showInLegend { get; set; }
        public string yValueFormatString { get; set; }
        public string name { get; set; }
        public List<agentData> dataPoints { get; set; }
    }

    public class agentSaleData
    {
        public int SalesPersonId { get; set; }
        public double TotalAmount { get; set; }
        public int mm { get; set; }

    }

    public class Agnt
    {
        public List<peopledata> peples { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }

    }
    public class peopledata
    {
        public string DisplayName { get; set; }
        public int PeopleID { get; set; }
        public int process { get; set; }

    }
    public class security
    {
        public SecurityDetails securi { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class TrainDev
    {
        public TrainingDevelopment traind { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class driverDetail
    {
        public People drivers { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class delBoyDetails
    {
        public People delBoy { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class vehicleDetail
    {
        public Vehicle vehicle { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class listchannelpartner
    {

        public string agents { get; set; }
        public int id { get; set; }
        public int custsignedup { get; set; }
        public int custactive { get; set; }
        public double sales { get; set; }
        public double DeliveryPerc { get; set; }
        public double CancelPerc { get; set; }
        public double? totCom { get; set; }
        public string totTat { get; set; }

    }


    public class listAgentPerformance
    {

        public string agents { get; set; }
        public int id { get; set; }
        public int custsignedup { get; set; }
        public int custactive { get; set; }
        public double sales { get; set; }
        public double DeliveryPerc { get; set; }
        public double CancelPerc { get; set; }
        public double totCom { get; set; }
        public string totTat { get; set; }

    }


    public class OrderHistories
    {
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public int orderid { get; set; }
    }


    public class agentpercent
    {
        public int Agent { get; set; }
        public double CompletePercent { get; set; }
        public string dboyNames { get; set; }
    }
    public class AgntCompleted
    {
        public List<agentpercent> details { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }

    }
    public class agetntlic
    {
        public AgentLicense license { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}



