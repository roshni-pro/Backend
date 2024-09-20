using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/NotVisit")]
    public class NotVisitController : ApiController
    {
       
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        //[Authorize]
        //[Route("")]
        //public IEnumerable<People> Get()
        //{
        //    logger.Info("Get Peoples: ");
        //    int compid = 0, userid = 0;
        //    int Warehouse_id = 0;
        //    string email = "";
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {

        //            var identity = User.Identity as ClaimsIdentity;
        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }

        //                if (claim.Type == "Warehouseid")
        //                {
        //                    Warehouse_id = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "email")
        //                {
        //                    email = claim.Value;
        //                }
        //            }
        //            List<People> person = new List<People>();
        //            logger.Info("End Get Company: ");
        //            if (Warehouse_id > 0)
        //            {
        //                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouse_id + " and p.CompanyId=" + compid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                person = db.Database.SqlQuery<People>(query).ToList();
        //                // person = db.Peoples.Where(e => e.CompanyId == compid && e.Deleted == false && e.WarehouseId == Warehouse_id && e.Department == "Sales Executive").ToList();
        //                return person;
        //            }
        //            else
        //            {
        //                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.CompanyId=" + compid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                person = db.Database.SqlQuery<People>(query).ToList();
        //                //person = db.Peoples.Where(e => e.CompanyId == compid && e.Deleted == false && e.Department == "Sales Executive").ToList();
        //                return person;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in getting Peoples " + ex.Message);
        //            return null;
        //        }
        //    }
        //}

        //[HttpGet]
        //[Route("Total_Retailer")]
        //public int Get_Total_Retailer()
        //{
        //    logger.Info("Get Peoples: ");
        //    int compid = 1, userid = 0;
        //    int Warehouse_id = 0;
        //    string email = "";
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }

        //                if (claim.Type == "Warehouseid")
        //                {
        //                    Warehouse_id = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "email")
        //                {
        //                    email = claim.Value;
        //                }
        //            }
        //            List<Customer> customer = new List<Customer>();
        //            logger.Info("End Get Company: ");
        //            if (Warehouse_id > 0)
        //            {
        //                var p = db.Customers.Count(e => e.CompanyId == compid && e.Deleted == false && e.Warehouseid == Warehouse_id);
        //                return p;
        //            }
        //            else
        //            {
        //                var p = db.Customers.Count(e => e.CompanyId == compid && e.Deleted == false);
        //                return p;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in getting Peoples " + ex.Message);
        //            return 0;
        //        }
        //    }
        //}

        //[HttpGet]
        //[Route("Active_Retailer")]
        //public int Get_Active_Retailer()
        //{
        //    logger.Info("Get Active Retailer: ");
        //    int compid = 1, userid = 0;
        //    int Warehouse_id = 0;
        //    string email = "";
        //    using (AuthContext db = new AuthContext())
        //    {

        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }

        //                if (claim.Type == "Warehouseid")
        //                {
        //                    Warehouse_id = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "email")
        //                {
        //                    email = claim.Value;
        //                }
        //            }
        //            List<Customer> customer = new List<Customer>();
        //            logger.Info("End Active Retailer: ");
        //            if (Warehouse_id > 0)
        //            {
        //                var p = db.Customers.Count(e => e.CompanyId == compid && e.Active == true && e.Deleted == false && e.Warehouseid == Warehouse_id);

        //                return p;
        //            }
        //            else
        //            {
        //                var p = db.Customers.Count(e => e.CompanyId == compid && e.Active == true && e.Deleted == false);

        //                return p;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in getting Peoples " + ex.Message);
        //            return 0;
        //        }
        //    }
        //}
        //[HttpGet]
        //[Route("Total_Order")]
        //public int Get_Total_Order()
        //{
        //    logger.Info("Get Total Order: ");
        //    int compid = 1, userid = 0;
        //    int Warehouse_id = 0;
        //    string email = "";
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }

        //                if (claim.Type == "Warehouseid")
        //                {
        //                    Warehouse_id = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "email")
        //                {
        //                    email = claim.Value;
        //                }
        //            }
        //            List<OrderMaster> order = new List<OrderMaster>();
        //            logger.Info("End Get Order: ");
        //            if (Warehouse_id > 0)
        //            {
        //                var p = db.DbOrderMaster.Count(e => e.CompanyId == compid && e.Deleted == false && e.WarehouseId == Warehouse_id);
        //                return p;
        //            }
        //            else
        //            {
        //                var p = db.DbOrderMaster.Count(e => e.CompanyId == compid && e.Deleted == false);

        //                return p;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in getting Peoples " + ex.Message);
        //            return 0;
        //        }
        //    }
        //}


        ////////////////////search with day and date Taslim
        //[HttpPost]
        //[Route("Search")]
        //public dynamic Post(DBOYinfos DBI)
        //{
        //    return null;
        //    //DateTime baseDate = DateTime.Today;

        //    //var today = baseDate;
        //    //var yesterday = baseDate.AddDays(-1);
        //    //var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
        //    //var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

        //    //logger.Info("start OrderMaster: ");
        //    //List<SaleSmanReport> MainReport = new List<SaleSmanReport>();
        //    //DateTime start = DateTime.Parse("01-01-2017 00:00:00");
        //    //DateTime end = DateTime.Today.AddDays(1);
        //    //using (AuthContext db = new AuthContext())
        //    //{
        //    //    try
        //    //    {
        //    //        var identity = User.Identity as ClaimsIdentity;
        //    //        int compid = 0, userid = 0;
        //    //        foreach (Claim claim in identity.Claims)
        //    //        {
        //    //            if (claim.Type == "compid")
        //    //            {
        //    //                compid = int.Parse(claim.Value);
        //    //            }
        //    //            if (claim.Type == "userid")
        //    //            {
        //    //                userid = int.Parse(claim.Value);
        //    //            }
        //    //        }
        //    //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //    //        if (DBI.datefrom == null)
        //    //        {
        //    //            DBI.datefrom = DateTime.Parse("01-01-2017 00:00:00");
        //    //            DBI.dateto = DateTime.Today.AddDays(1);
        //    //        }
        //    //        else
        //    //        {
        //    //            start = DBI.datefrom.GetValueOrDefault();
        //    //            end = DBI.dateto.GetValueOrDefault();
        //    //        }
        //    //        if (DBI.value == 1)
        //    //        {
        //    //            SaleSmanReport oh = new SaleSmanReport();
        //    //            foreach (var i in DBI.ids)
        //    //            {

        //    //                var olist = getDSalemManOrdersHistory(i.mob, DBI.datefrom, DBI.dateto, i.id, compid);
        //    //                var TotalReatailercount = db.Customers.Where(x => x.Deleted == false && x.ExecutiveId == i.id && x.CreatedDate >= DBI.datefrom && x.CreatedDate <= DBI.dateto).ToList();

        //    //                var TotalActiveReatailercount = db.Customers.Where(x => x.Deleted == false && x.Active == true && x.ExecutiveId == i.id && x.CreatedDate >= DBI.datefrom && x.CreatedDate <= DBI.dateto).ToList();
        //    //                var TotalOrder = db.DbOrderMaster.Where(x => x.SalesPersonId == i.id && x.CreatedDate >= DBI.datefrom && x.CreatedDate <= DBI.dateto).ToList();
        //    //                //  var TotalCost = db.DbOrderMaster.Where(x => x.Deleted == false && x.SalesPersonId == i.id)
        //    //                //   var TotalCost = (from OrderMaster dtr in db.DbOrderMaster
        //    //                //      where dtr.SalesPersonId == i.id
        //    //                //      select dtr.TotalAmount).Sum();
        //    //                double TotalCost = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.SalesPersonId == i.id && pkg.CreatedDate >= DBI.datefrom && pkg.CreatedDate <= DBI.dateto).Sum(pkg => pkg.TotalAmount);
        //    //                oh = olist;
        //    //                oh.TotalRetailer = oh.TotalRetailer + TotalReatailercount.Count();
        //    //                oh.ActiveRetailer = oh.ActiveRetailer + TotalActiveReatailercount.Count();
        //    //                oh.TotalOrder = oh.TotalOrder + TotalOrder.Count();
        //    //                oh.TotalCost = Convert.ToInt32(TotalCost);
        //    //                oh.value = DBI.value;
        //    //                MainReport.Add(oh);
        //    //            }
        //    //        }
        //    //        else if (DBI.value == 2)
        //    //        {
        //    //            SaleSmanReport oh = new SaleSmanReport();
        //    //            foreach (var i in DBI.ids)
        //    //            {

        //    //                var olist = getDSalemManOrdersHistory(i.mob, thisWeekStart, thisWeekEnd, i.id, compid);
        //    //                var TotalReatailercount = db.Customers.Where(x => x.Deleted == false && x.ExecutiveId == i.id && x.CreatedDate >= thisWeekStart && x.CreatedDate <= thisWeekEnd).ToList();
        //    //                var Day = db.Customers.Where(x => x.Deleted == false && x.ExecutiveId == i.id && x.Day == DBI.Day && x.CreatedDate >= thisWeekStart && x.CreatedDate <= thisWeekEnd).ToList();
        //    //                var TotalActiveReatailercount = db.Customers.Where(x => x.Deleted == false && x.Active == true && x.ExecutiveId == i.id && x.CreatedDate >= thisWeekStart && x.CreatedDate <= thisWeekEnd).ToList();
        //    //                var TotalOrder = db.DbOrderMaster.Where(x => x.SalesPersonId == i.id && x.CreatedDate >= thisWeekStart && x.CreatedDate <= thisWeekEnd).ToList();
        //    //                double TotalCost = 0;

        //    //                try
        //    //                {
        //    //                    TotalCost = db.DbOrderMaster.AsQueryable().Where(pkg => pkg.SalesPersonId == i.id && pkg.CreatedDate >= thisWeekStart && pkg.CreatedDate <= thisWeekEnd).Sum(pkg => pkg.TotalAmount);
        //    //                }
        //    //                catch { }
        //    //                oh = olist;
        //    //                oh.TotalRetailer = oh.TotalRetailer + TotalReatailercount.Count();
        //    //                oh.ActiveRetailer = oh.ActiveRetailer + TotalActiveReatailercount.Count();
        //    //                oh.TotalOrder = oh.TotalOrder + TotalOrder.Count();
        //    //                oh.TotalCost = Convert.ToInt32(TotalCost);
        //    //                oh.Day = oh.Day + Day.Count();
        //    //                oh.value = DBI.value;
        //    //                MainReport.Add(oh);
        //    //            }
        //    //        }


        //    //        return MainReport;

        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        logger.Error("Error in OrderMaster " + ex.Message);
        //    //        logger.Info("End  OrderMaster: ");
        //    //        return null;
        //    //    }
        //    //}
        //}


        //public SaleSmanReport getDSalemManOrdersHistory(string mob, DateTime? start, DateTime? end, int dboyId, int compid)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {
        //            SaleSmanReport orderhistory = new SaleSmanReport();
        //            orderhistory.VisitedTotal = 0;
        //            orderhistory.NotVisitedTotal = 0;
        //            orderhistory.Totalbeat = 0;
        //            orderhistory.NonvisitreasonPercent = 0;
        //            orderhistory.TotalbeatOfSalesMan = 0;
        //            orderhistory.TotalRetailer = 0;
        //            int? DemoNonvisitreasonPercent = 0;

        //            var custware = db.Customers.Where(x => x.CreatedDate > start && x.CreatedDate <= end && x.ExecutiveId == dboyId && x.CompanyId == compid).ToList();
        //            foreach (var data in custware)
        //            {
        //                orderhistory.TotalbeatOfSalesMan = orderhistory.TotalbeatOfSalesMan + 1;
        //            }



        //            var BeatIssulist = db.SalesPersonBeatDb.Where(x => x.CreatedDate > start && x.CreatedDate <= end && x.SalesPersonId == dboyId && x.CompanyId == compid).ToList();

        //            foreach (var o in BeatIssulist)
        //            {
        //                orderhistory.SalespersonName = o.SalespersonName;
        //                if (o.Visited == false)
        //                {
        //                    orderhistory.NotVisitedTotal = orderhistory.NotVisitedTotal + 1;
        //                    orderhistory.Totalbeat = orderhistory.Totalbeat + 1;
        //                    DemoNonvisitreasonPercent = DemoNonvisitreasonPercent + 1;

        //                }
        //                if (o.Visited == true)
        //                {
        //                    orderhistory.VisitedTotal = orderhistory.VisitedTotal + 1;
        //                    orderhistory.Totalbeat = orderhistory.Totalbeat + 1;
        //                }
        //            }

        //            orderhistory.NonvisitreasonPercent = ((DemoNonvisitreasonPercent * orderhistory.Totalbeat) / 100);

        //            return orderhistory;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }
        //}


        //[HttpPost]
        //[Route("GetVisited")]
        //public dynamic GetVisited(DBOYinfos DBI)
        //{
        //    logger.Info("start OrderMaster: ");
        //    List<VisitedReport> MainReport = new List<VisitedReport>();
        //    DateTime start = DateTime.Parse("01-01-2017 00:00:00");
        //    DateTime end = DateTime.Today.AddDays(1);

        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            if (claim.Type == "compid")
        //            {
        //                compid = int.Parse(claim.Value);
        //            }
        //            if (claim.Type == "userid")
        //            {
        //                userid = int.Parse(claim.Value);
        //            }
        //        }
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //        if (DBI.datefrom == null)
        //        {
        //            DBI.datefrom = DateTime.Parse("01-01-2017 00:00:00");
        //            DBI.dateto = DateTime.Today.AddDays(1);
        //        }
        //        else
        //        {
        //            start = DBI.datefrom.GetValueOrDefault();
        //            end = DBI.dateto.GetValueOrDefault();
        //        }
        //        foreach (var i in DBI.ids)
        //        {
        //            VisitedReport oh = new VisitedReport();
        //            var olist = getDataOfVisited(i.mob, DBI.datefrom, DBI.dateto, i.id, compid);
        //            oh = olist;
        //            MainReport.Add(oh);
        //        }
        //        return MainReport;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in OrderMaster " + ex.Message);
        //        logger.Info("End  OrderMaster: ");
        //        return null;
        //    }
        //}

        //public VisitedReport getDataOfVisited(string mob, DateTime? start, DateTime? end, int dboyId, int compid)
        //{
        //    using (AuthContext db = new AuthContext())

        //        try
        //        {
        //            VisitedReport visited = new VisitedReport();
        //            visited.price = 0;
        //            visited.Other = 0;
        //            visited.ShopClosed = 0;
        //            visited.Notsatisfied = 0;
        //            visited.TotalVisited = 0;
        //            //int? DemoNonvisitreasonPercent = 0;

        //            visited.TotalVisited = db.Customers.Count(x => x.CreatedDate > start && x.CreatedDate <= end && x.ExecutiveId == dboyId && x.CompanyId == compid);
                   
        //            var BeatIssulist = db.SalesPersonBeatDb.Where(x => x.CreatedDate > start && x.CreatedDate <= end && x.SalesPersonId == dboyId && x.CompanyId == compid && x.Visited == true).ToList();

        //            foreach (var o in BeatIssulist)
        //            {
        //                visited.SalesPersonName = o.SalespersonName;
        //                if (o.status == "Price")
        //                {
        //                    visited.price = visited.price + 1;
        //                }
        //                if (o.status == "Other")
        //                {
        //                    visited.Other = visited.Other + 1;
        //                }
        //                if (o.status == "Shop Closed")
        //                {
        //                    visited.ShopClosed = visited.ShopClosed + 1;
        //                }
        //                if (o.status == "Not satisfied")
        //                {
        //                    visited.Notsatisfied = visited.Notsatisfied + 1;
        //                }
        //            }

        //            //visited.TotalVisited = ((DemoNonvisitreasonPercent * visited.TotalVisited) / 100);

        //            return visited;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //}


        //[HttpPost]
        //[Route("GetNotVisited")]
        //public dynamic GetNotVisited(DBOYinfos DBI)
        //{
        //    logger.Info("start OrderMaster: ");
        //    List<NotVisitedReport> MainReport = new List<NotVisitedReport>();
        //    DateTime start = DateTime.Parse("01-01-2017 00:00:00");
        //    DateTime end = DateTime.Today.AddDays(1);
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }
        //            }
        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //            if (DBI.datefrom == null)
        //            {
        //                DBI.datefrom = DateTime.Parse("01-01-2017 00:00:00");
        //                DBI.dateto = DateTime.Today.AddDays(1);
        //            }
        //            else
        //            {
        //                start = DBI.datefrom.GetValueOrDefault();
        //                end = DBI.dateto.GetValueOrDefault();
        //            }
        //            foreach (var i in DBI.ids)
        //            {
        //                NotVisitedReport oh = new NotVisitedReport();
        //                var olist = getDataOfNotVisited(i.mob, DBI.datefrom, DBI.dateto, i.id, compid);
        //                oh = olist;
        //                MainReport.Add(oh);
        //            }
        //            return MainReport;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in OrderMaster " + ex.Message);
        //            logger.Info("End  OrderMaster: ");
        //            return null;
        //        }
        //    }
        //}

        //public NotVisitedReport getDataOfNotVisited(string mob, DateTime? start, DateTime? end, int dboyId, int compid)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            NotVisitedReport Notvisited = new NotVisitedReport();
        //            Notvisited.Lackoftime = 0;
        //            Notvisited.NotAbleToFind = 0;
        //            Notvisited.WrongTerritory = 0;
        //            Notvisited.TotalNotVisited = 0;
        //            //int? DemoNonvisitreasonPercent = 0;

        //            var custware = db.Customers.Where(x => x.CreatedDate > start && x.CreatedDate <= end && x.ExecutiveId == dboyId && x.CompanyId == compid).ToList();
        //            foreach (var data in custware)
        //            {
        //                Notvisited.TotalNotVisited = Notvisited.TotalNotVisited + 1;
        //            }

        //            var BeatIssulist = db.SalesPersonBeatDb.Where(x => x.CreatedDate > start && x.CreatedDate <= end && x.SalesPersonId == dboyId && x.CompanyId == compid && x.Visited == false).ToList();

        //            foreach (var o in BeatIssulist)
        //            {
        //                Notvisited.SalesPersonName = o.SalespersonName;
        //                if (o.status == "Lack of time")
        //                {
        //                    Notvisited.Lackoftime = Notvisited.Lackoftime + 1;
        //                }
        //                if (o.status == "Not able to find ")
        //                {
        //                    Notvisited.NotAbleToFind = Notvisited.NotAbleToFind + 1;
        //                }
        //                if (o.status == "Wrong territory /beat")
        //                {
        //                    Notvisited.WrongTerritory = Notvisited.WrongTerritory + 1;
        //                }
        //            }

        //            //visited.TotalVisited = ((DemoNonvisitreasonPercent * visited.TotalVisited) / 100);

        //            return Notvisited;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }
        //}


        public class SaleSmanReport
        {
            public int id { get; set; }
            public int CompanyId { get; set; }
            public int WarehouseId { get; set; }
            public int SalesPersonId { get; set; }
            public string Skcode { get; set; }
            public int? VisitedTotal { get; set; }
            public int? NotVisitedTotal { get; set; }
            public int? Totalbeat { get; set; }

            public int? TotalbeatOfSalesMan { get; set; }
            public int? Orderd { get; set; }
            public int? NotOrderd { get; set; }
            public int? TotalBeat { get; set; }
            public double? NotordredreasonPercent { get; set; }
            public double? NonvisitreasonPercent { get; set; }
            public int? value { get; set; }
            public int TotalRetailer { get; set; }
            public int ActiveRetailer { get; set; }
            public int TotalOrder { get; set; }
            public int TotalCost { get; set; }
            public int Day { get; set; }
            public string ShopName { get; set; }
            public string SalespersonName { get; set; }
            public string status { get; set; }
            public string Comment { get; set; }
            public double lat { get; set; }
            public double lg { get; set; }
            public DateTime? CreatedDate { get; set; }
        }
        public class DBOYinfos
        {
            public List<dbinfs> ids { get; set; }
            public DateTime? datefrom { get; set; }
            public DateTime? dateto { get; set; }
            public string Day { get; set; }
            public int? value { get; set; }
        }
        public class dbinfs
        {
            public int id { get; set; }
            public string mob { get; set; }
        }

        public class VisitedReport
        {
            public int price
            {
                get; set;
            }

            public int Other
            {
                get; set;
            }

            public int ShopClosed
            {
                get; set;
            }

            public int Notsatisfied
            {
                get; set;
            }

            public int TotalVisited
            {
                get; set;
            }

            public string SalesPersonName
            {
                get; set;
            }

        }

        public class NotVisitedReport
        {
            public int Lackoftime
            {
                get; set;
            }

            public int NotAbleToFind
            {
                get; set;
            }


            public int WrongTerritory
            {
                get; set;
            }

            public int TotalNotVisited
            {
                get; set;
            }

            public string SalesPersonName
            {
                get; set;
            }

        }

    }

}
