using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using AngularJSAuthentication.Model;
using LinqKit;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

using System.Web.Http;
using static AngularJSAuthentication.DataContracts.Masters.MonthEnd.MonthEndReportDc;
using AngularJSAuthentication.DataContracts.LPReport;
#pragma warning disable CS0105 // The using directive for 'AngularJSAuthentication.DataContracts.Masters' appeared previously in this namespace
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using static AngularJSAuthentication.API.Controllers.WarehouseController;
#pragma warning restore CS0105 // The using directive for 'AngularJSAuthentication.DataContracts.Masters' appeared previously in this namespace


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Report")]
    public class ReportController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Authorize]

        //[Route("MailAlert")]
        //[HttpGet]
        //public bool InvoiceNoNotGeneratedMailAlert()
        //{

        //    ReportManager reportManager = new ReportManager();
        //    return  reportManager.invoiceNoNotgnerated();
        //}

        [Route("first")]
        [HttpGet]
        public dynamic reportfirst(string type, int WarehouseId)
        {
            dataSelect result = new dataSelect();
            logger.Info("start Get Report1: ");
            using (var db = new AuthContext())
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    var d = indianTime.Day;
                    var m = indianTime.Month;
                    var y = indianTime.Year;
                    var TotalOrder = db.DbOrderMaster.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId && x.CompanyId == compid && (x.Status != "sattled" && x.Status != "Partial settled" && x.Status != "Account settled")).ToList();

                    var data = TotalOrder.Where(x => x.CreatedDate.Day == d && x.CreatedDate.Month == m && x.CreatedDate.Year == y).ToList();
                    result.totalOrder = data.Count;
                    foreach (var a in data)
                    {
                        result.totalSale += a.TotalAmount;
                        if (a.Status == "Delivery Canceled" || a.Status == "Order Canceled")
                        {
                            result.cancelOrder += 1;
                        }
                    }
                    var data1 = TotalOrder.Where(x => x.Status == "Pending").ToList();
                    result.pendingOrder = data1.Count;
                    foreach (var b in data1)
                    {
                        result.PendingSale += b.TotalAmount;
                    }
                    var data2 = TotalOrder.Where(x => x.CreatedDate.AddDays(2).Date < indianTime.Date && x.Status == "Pending").ToList();
                    result.pendingOrder_2 = data2.Count;
                    foreach (var c in data2)
                    {
                        result.PendingSale2 += c.TotalAmount;
                    }
                    var data3 = TotalOrder.Where(x => x.UpdatedDate.Day == d && x.UpdatedDate.Month == m && x.UpdatedDate.Year == y && x.Status == "Delivered").ToList();
                    result.totalDelivered = data3.Count;
                    foreach (var f in data3)
                    {
                        result.deliveredSale += f.TotalAmount;
                    }
                    var data4 = TotalOrder.Where(x => x.CreatedDate.AddDays(2).Date < indianTime.Date && ((x.Status == "Ready to Dispatch" || x.Status == "Issued" || x.Status == "Assigned" || x.Status == "Shipped" || x.Status == "Delivery Redispatch") && x.CreatedDate.AddDays(2).Date < indianTime.Date)).ToList();
                    //data4= data4.Where(x => x.CreatedDate.AddDays(2).Date < indianTime.Date).ToList();
                    result.notDelivered = data4.Count;
                    foreach (var g in data4)
                    {
                        result.notDeliveredSale += g.TotalAmount;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
            }
        }

        [Route("select")]
        [HttpGet]
        public dynamic Get(int value)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (var db = new AuthContext())
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (value == 1)
                    {
                        //result = db.Customers.Where(x => x.Deleted == false && x.CompanyId == compid).ToList();
                        result = (from i in db.Customers
                                  where i.CompanyId == compid
                                  join k in db.Customers on i.CustomerId equals k.CustomerId
                                  select new CustomerDTOM
                                  {
                                      CustomerId = i.CustomerId,
                                      //Name = k.Name,
                                      Skcode = k.Skcode,
                                      City = k.City,
                                      ShopName = k.ShopName,
                                      //Mobile = k.Mobile,
                                      Emailid = k.Emailid,
                                      BillingAddress = k.BillingAddress,
                                      CreatedDate = i.CreatedDate,
                                      UpdatedDate = i.UpdatedDate,
                                      CompanyId = i.CompanyId,
                                      Active = k.Active
                                  }).ToList();

                    }
                    if (value == 2 || value == 7)
                    {
                        var WCWH = "select w.WarehouseId,w.WarehouseName, w.RegionId from Warehouses " +
                                   "w inner join GMWarehouseProgresses b on w.WarehouseId = b.WarehouseID and b.IsLaunched=1 " +
                                   "and w.active=1 and w.Deleted=0 and w.IsKPP=0 and w.CityName not like '%test%' order by (w.WarehouseId)";
                        result = db.Database.SqlQuery<WCWarehouse>(WCWH).ToList();
                    }
                    else if (value == 3)
                    {
                        result = db.Cities.Where(x => x.Deleted == false).ToList();
                    }
                    else if (value == 4)
                    {
                        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.CompanyId=" + compid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        result = db.Database.SqlQuery<People>(query).ToList();
                        //result = db.Peoples.Where(x => x.Department == "Sales Executive" && x.Deleted == false && x.CompanyId == compid).ToList();
                    }

                    else if (value == 5)
                    {
                        result = db.Clusters.Where(x => x.Deleted == false && x.CompanyId == compid).ToList();
                    }
                    else if (value == 6)
                    {
                        result = db.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => new
                        {
                            x.Id,
                            x.Name
                        }).ToList();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
            }
        }

        [Route("Catogory")]
        [HttpGet]
        public dynamic GetCatogery(int value)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (Warehouse_id > 0)
                    {
                        if (value == 1)
                        {
                            result = db.Categorys.Where(x => x.IsActive == true).OrderBy(x => x.CategoryName).ToList();
                        }
                        else if (value == 2)
                        {
                            result = (from i in db.SubCategorys
                                      where i.IsActive == true && i.Deleted == false
                                      select new
                                      {
                                          SubCategoryId = i.SubCategoryId,
                                          SubcategoryName = i.CategoryName.Trim() + " " + i.SubcategoryName.Trim(),
                                          Categoryid = i.Categoryid,
                                          CategoryName = i.CategoryName
                                      }).OrderBy(x => x.SubcategoryName).ToList();
                        }
                        else if (value == 3)
                        {
                            result = (from i in db.SubsubCategorys
                                      where i.IsActive == true && i.Deleted == false
                                      join j in db.SubCategorys on i.SubCategoryId equals j.SubCategoryId
                                      select new
                                      {
                                          SubsubCategoryid = i.SubsubCategoryid,
                                          SubsubcategoryName = i.CategoryName.Trim() + " (" + j.SubcategoryName.Trim() + ")" + i.SubsubcategoryName.Trim(),
                                          Categoryid = i.Categoryid,
                                          CategoryName = i.CategoryName,
                                          SubCategoryId = i.SubCategoryId
                                      }).OrderBy(x => x.SubsubcategoryName).ToList();
                        }
                        else if (value == 4)
                        {
                            result = db.itemMasters.Where(x => x.active == true && x.CompanyId == compid && x.WarehouseId == Warehouse_id).OrderBy(x => x.itemname).ToList();
                        }
                        return result;
                    }

                    else
                    {
                        if (value == 1)
                        {
                            result = db.Categorys.Where(x => x.IsActive == true).OrderBy(x => x.CategoryName).ToList();
                        }
                        else if (value == 2)
                        {
                            result = (from i in db.SubCategorys
                                      where i.IsActive == true && i.Deleted == false
                                      select new
                                      {
                                          SubCategoryId = i.SubCategoryId,
                                          SubcategoryName = i.CategoryName.Trim() + " " + i.SubcategoryName.Trim(),
                                          Categoryid = i.Categoryid,
                                          CategoryName = i.CategoryName
                                      }).OrderBy(x => x.SubcategoryName).ToList();
                        }
                        else if (value == 3)
                        {
                            result = (from i in db.SubsubCategorys
                                      where i.IsActive == true && i.Deleted == false
                                      join j in db.SubCategorys on i.SubCategoryId equals j.SubCategoryId
                                      select new
                                      {
                                          SubsubCategoryid = i.SubsubCategoryid,
                                          SubsubcategoryName = i.CategoryName.Trim() + " (" + j.SubcategoryName.Trim() + ")" + i.SubsubcategoryName.Trim(),
                                          Categoryid = i.Categoryid,
                                          CategoryName = i.CategoryName,
                                          SubCategoryId = i.SubCategoryId
                                      }).OrderBy(x => x.SubsubcategoryName).ToList();
                        }
                        else if (value == 4)
                        {
                            result = db.itemMasters.Where(x => x.active == true && x.CompanyId == compid).OrderBy(x => x.itemname).ToList();
                        }
                        return result;
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
            }
        }

        [Route("day")]
        [HttpGet]
        public dynamic GetDay(DateTime? datefrom, DateTime? dateto, int type, int value, string ids)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
                var array = ids.Split(',');
                foreach (var iidd in array)
                {
                    int id = Convert.ToInt32(iidd);
                    var res = getdata(datefrom, dateto, value, id);
                    MainReports MainReport1 = new MainReports();
                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueDay = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        foreach (var a in list)
                        {
                            orderMasterlist l = uniqueDay.Where(x => x.createdDate.Date == a.createdDate.Date).SingleOrDefault();
                            if (l == null)
                            {
                                a.TotalAmount = a.TotalAmount;
                                uniqueDay.Add(a);
                            }
                            else
                            {
                                l.TotalAmount = l.TotalAmount + a.TotalAmount;
                            }
                            orderMasterlist m = uniqueOrdered.Where(c => c.OrderId == a.OrderId && c.createdDate.Date == a.createdDate.Date).SingleOrDefault();
                            if (m == null)
                            {
                                uniqueOrdered.Add(a);
                            }
                            orderMasterlist n = uniqueCustomer.Where(c => c.retaileId == a.retaileId && c.createdDate.Date == a.createdDate.Date).SingleOrDefault();
                            if (n == null)
                            {
                                uniqueCustomer.Add(a);
                            }
                        }
                        for (var day = start.Date; day.Date <= end.Date; day = day.AddDays(1))
                        {
                            orderMasterlist c = new orderMasterlist();

                            c.name = list[0].name;
                            c.day = day.Day;
                            c.month = day.Month;
                            c.year = day.Year;
                            c.createdDate = day;
                            var total = uniqueDay.Where(a => a.createdDate.Date == day).FirstOrDefault();
                            if (total == null)
                            {
                                c.TotalAmount = 0.00;
                            }
                            else
                            {
                                c.TotalAmount = total.TotalAmount;
                            }
                            List<orderMasterlist> order = uniqueOrdered.Where(a => a.createdDate.Date == day).ToList();
                            if (order.Count == 0)
                            {
                                c.totalOrder = 0;
                            }
                            else
                            {
                                c.totalOrder = order.Count;
                            }
                            List<orderMasterlist> retailer = uniqueCustomer.Where(a => a.createdDate.Date == day).ToList();
                            if (retailer.Count == 0)
                            {
                                c.activeRetailers = 0;
                            }
                            else
                            {
                                c.activeRetailers = retailer.Count;
                            }
                            report.Add(c);
                        }
                        MainReport1.reports = report;
                        MainReport.Add(MainReport1);
                    }
                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }

        [Route("month")]
        [HttpGet]
        public dynamic GetMonth(DateTime? datefrom, DateTime? dateto, int type, int value, string ids)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
                var array = ids.Split(',');
                foreach (var iidd in array)
                {
                    int id = Convert.ToInt32(iidd);
                    var res = getdata(datefrom, dateto, value, id);
                    MainReports MainReport1 = new MainReports();
                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueMonth = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        foreach (var a in list)
                        {
                            orderMasterlist l = uniqueMonth.Where(x => x.createdDate.Month == a.createdDate.Month && x.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (l == null)
                            {
                                a.TotalAmount = a.TotalAmount;
                                uniqueMonth.Add(a);
                            }
                            else
                            {
                                l.TotalAmount = l.TotalAmount + a.TotalAmount;
                            }
                            orderMasterlist m = uniqueOrdered.Where(c => c.OrderId == a.OrderId && (c.createdDate.Month == a.createdDate.Month && c.createdDate.Year == a.createdDate.Year)).SingleOrDefault();
                            if (m == null)
                            {
                                uniqueOrdered.Add(a);
                            }
                            orderMasterlist n = uniqueCustomer.Where(c => c.retaileId == a.retaileId && (c.createdDate.Month == a.createdDate.Month && c.createdDate.Year == a.createdDate.Year)).SingleOrDefault();
                            if (n == null)
                            {
                                uniqueCustomer.Add(a);
                            }
                        }

                        var d = start.Date;
                        var day = d.Month;
                        var year = d.Year;
                        do
                        {
                            orderMasterlist c = new orderMasterlist();
                            c.name = list[0].name;
                            c.month = d.Month;
                            c.year = d.Year;
                            c.createdDate = new DateTime(d.Year, d.Month, 1);
                            var total = uniqueMonth.Where(a => a.createdDate.Month == c.month && a.createdDate.Year == c.year).FirstOrDefault();
                            if (total == null)
                            {
                                c.TotalAmount = 0.00;
                            }
                            else
                            {
                                c.TotalAmount = total.TotalAmount;
                            }
                            List<orderMasterlist> order = uniqueOrdered.Where(a => a.createdDate.Month == c.month && a.createdDate.Year == c.year).ToList();
                            if (order.Count == 0)
                            {
                                c.totalOrder = 0;
                            }
                            else
                            {
                                c.totalOrder = order.Count;
                            }
                            List<orderMasterlist> retailer = uniqueCustomer.Where(a => a.createdDate.Month == c.month && a.createdDate.Year == c.year).ToList();
                            if (retailer.Count == 0)
                            {
                                c.activeRetailers = 0;
                            }
                            else
                            {
                                c.activeRetailers = retailer.Count;
                            }
                            report.Add(c);
                            d = d.AddMonths(1);
                            day = d.Month;
                            year = d.Year;
                        } while (day <= end.Month && year <= end.Year);
                        MainReport1.reports = report;
                        MainReport.Add(MainReport1);
                    }
                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }

        [Route("year")]
        [HttpGet]
        public dynamic GetYear(DateTime? datefrom, DateTime? dateto, int type, int value, string ids)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
                var array = ids.Split(',');
                foreach (var iidd in array)
                {
                    int id = Convert.ToInt32(iidd);
                    var res = getdata(datefrom, dateto, value, id);
                    MainReports MainReport1 = new MainReports();
                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueYear = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        foreach (var a in list)
                        {
                            orderMasterlist l = uniqueYear.Where(x => x.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (l == null)
                            {
                                a.TotalAmount = a.TotalAmount;
                                uniqueYear.Add(a);
                            }
                            else
                            {
                                l.TotalAmount = l.TotalAmount + a.TotalAmount;
                            }
                            orderMasterlist m = uniqueOrdered.Where(c => c.OrderId == a.OrderId && c.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (m == null)
                            {
                                uniqueOrdered.Add(a);
                            }
                            orderMasterlist n = uniqueCustomer.Where(c => c.retaileId == a.retaileId && c.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (n == null)
                            {
                                uniqueCustomer.Add(a);
                            }
                        }
                        var d = start.Date;
                        var day = d.Year;
                        do
                        {
                            orderMasterlist c = new orderMasterlist();
                            c.name = list[0].name;
                            c.year = d.Year;
                            c.createdDate = new DateTime(d.Year, 1, 1);
                            var total = uniqueYear.Where(a => a.createdDate.Year == c.year).FirstOrDefault();
                            if (total == null)
                            {
                                c.TotalAmount = 0.00;
                            }
                            else
                            {
                                c.TotalAmount = total.TotalAmount;
                            }
                            List<orderMasterlist> order = uniqueOrdered.Where(a => a.createdDate.Year == c.year).ToList();
                            if (order.Count == 0)
                            {
                                c.totalOrder = 0;
                            }
                            else
                            {
                                c.totalOrder = order.Count;
                            }
                            List<orderMasterlist> retailer = uniqueCustomer.Where(a => a.createdDate.Year == c.year).ToList();
                            if (retailer.Count == 0)
                            {
                                c.activeRetailers = 0;
                            }
                            else
                            {
                                c.activeRetailers = retailer.Count;
                            }
                            report.Add(c);
                            d = d.AddYears(1);
                            day = d.Year;
                        } while (day <= end.Year);

                        MainReport1.reports = report;
                        MainReport.Add(MainReport1);
                    }
                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }

        public List<orderMasterlist> getdata(DateTime? datefrom, DateTime? dateto, int value, int id)
        {
            using (var db = new AuthContext())
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
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();

                    if (datefrom != null && dateto != null)
                    {
                        if (value == 1)
                        {
                            var data = (from i in db.DbOrderDetails
                                        where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid
                                        join j in db.itemMasters on i.ItemId equals j.ItemId
                                        join k in db.Categorys on j.Categoryid equals k.Categoryid
                                        select new orderMasterlist
                                        {
                                            cityid = i.CityId,
                                            warehouseid = i.WarehouseId,
                                            name = k.CategoryName,
                                            id = k.Categoryid,
                                            OrderId = i.OrderId,
                                            retaileId = i.CustomerId,
                                            TotalAmount = i.TotalAmt,
                                            createdDate = i.CreatedDate,
                                            updatedDate = i.UpdatedDate
                                        }).OrderBy(x => x.createdDate).ToList();
                            list = data.Where(x => x.id == id).ToList();
                        }
                        else if (value == 2)
                        {
                            var data = (from i in db.DbOrderDetails
                                        where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid
                                        join j in db.itemMasters on i.ItemId equals j.ItemId
                                        join k in db.SubCategorys on j.SubCategoryId equals k.SubCategoryId
                                        select new orderMasterlist
                                        {
                                            cityid = i.CityId,
                                            warehouseid = i.WarehouseId,
                                            name = k.CategoryName.Trim() + " " + k.SubcategoryName.Trim(),
                                            id = k.SubCategoryId,
                                            OrderId = i.OrderId,
                                            retaileId = i.CustomerId,
                                            TotalAmount = i.TotalAmt,
                                            createdDate = i.CreatedDate,
                                            updatedDate = i.UpdatedDate
                                        }).OrderBy(x => x.createdDate).ToList();
                            list = data.Where(x => x.id == id).ToList();
                        }
                        else if (value == 3)
                        {
                            var data = (from i in db.DbOrderDetails
                                        where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid
                                        join j in db.itemMasters on i.ItemId equals j.ItemId
                                        join k in db.SubsubCategorys on j.SubsubCategoryid equals k.SubsubCategoryid
                                        select new orderMasterlist
                                        {
                                            cityid = i.CityId,
                                            warehouseid = i.WarehouseId,
                                            name = k.CategoryName.Trim() + " (" + k.SubcategoryName.Trim() + ")" + k.SubsubcategoryName.Trim(),
                                            id = k.SubsubCategoryid,
                                            OrderId = i.OrderId,
                                            retaileId = i.CustomerId,
                                            TotalAmount = i.TotalAmt,
                                            createdDate = i.CreatedDate,
                                            updatedDate = i.UpdatedDate
                                        }).OrderBy(x => x.createdDate).ToList();
                            list = data.Where(x => x.id == id).ToList();
                        }
                        else if (value == 4)
                        {
                            list = (from i in db.DbOrderDetails
                                    where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.ItemId == id && i.CompanyId == compid
                                    select new orderMasterlist
                                    {
                                        cityid = i.CityId,
                                        warehouseid = i.WarehouseId,
                                        name = i.itemname,
                                        id = i.ItemId,
                                        OrderId = i.OrderId,
                                        retaileId = i.CustomerId,
                                        TotalAmount = i.TotalAmt,
                                        createdDate = i.CreatedDate,
                                        updatedDate = i.UpdatedDate
                                    }).OrderBy(x => x.createdDate).ToList();
                        }

                    }
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }



        #region Anushka
        //after Anushka code done
        [Route("day")]
        [HttpGet]
        public dynamic GetDay(DateTime? datefrom, DateTime? dateto, int type, int value, string ids, int WarehouseId)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
                var array = ids.Split(',');
                foreach (var iidd in array)
                {
                    int id = Convert.ToInt32(iidd);
                    var res = getdataWarehouse(datefrom, dateto, value, id, WarehouseId);
                    MainReports MainReport1 = new MainReports();
                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueDay = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        foreach (var a in list)
                        {
                            orderMasterlist l = uniqueDay.Where(x => x.createdDate.Date == a.createdDate.Date).SingleOrDefault();
                            if (l == null)
                            {
                                a.TotalAmount = a.TotalAmount;
                                uniqueDay.Add(a);
                            }
                            else
                            {
                                l.TotalAmount = l.TotalAmount + a.TotalAmount;
                            }
                            orderMasterlist m = uniqueOrdered.Where(c => c.OrderId == a.OrderId && c.createdDate.Date == a.createdDate.Date).SingleOrDefault();
                            if (m == null)
                            {
                                uniqueOrdered.Add(a);
                            }
                            orderMasterlist n = uniqueCustomer.Where(c => c.retaileId == a.retaileId && c.createdDate.Date == a.createdDate.Date).SingleOrDefault();
                            if (n == null)
                            {
                                uniqueCustomer.Add(a);
                            }
                        }
                        for (var day = start.Date; day.Date <= end.Date; day = day.AddDays(1))
                        {
                            orderMasterlist c = new orderMasterlist();

                            c.name = list[0].name;
                            c.day = day.Day;
                            c.month = day.Month;
                            c.year = day.Year;
                            c.createdDate = day;
                            var total = uniqueDay.Where(a => a.createdDate.Date == day).FirstOrDefault();
                            if (total == null)
                            {
                                c.TotalAmount = 0.00;
                            }
                            else
                            {
                                c.TotalAmount = total.TotalAmount;
                            }
                            List<orderMasterlist> order = uniqueOrdered.Where(a => a.createdDate.Date == day).ToList();
                            if (order.Count == 0)
                            {
                                c.totalOrder = 0;
                            }
                            else
                            {
                                c.totalOrder = order.Count;
                            }
                            List<orderMasterlist> retailer = uniqueCustomer.Where(a => a.createdDate.Date == day).ToList();
                            if (retailer.Count == 0)
                            {
                                c.activeRetailers = 0;
                            }
                            else
                            {
                                c.activeRetailers = retailer.Count;
                            }
                            report.Add(c);
                        }
                        MainReport1.reports = report;
                        MainReport.Add(MainReport1);
                    }
                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }

        [Route("month")]
        [HttpGet]
        public dynamic GetMonth(DateTime? datefrom, DateTime? dateto, int type, int value, string ids, int WarehouseId)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
                var array = ids.Split(',');
                foreach (var iidd in array)
                {
                    int id = Convert.ToInt32(iidd);
                    var res = getdataWarehouse(datefrom, dateto, value, id, WarehouseId);
                    MainReports MainReport1 = new MainReports();
                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueMonth = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        foreach (var a in list)
                        {
                            orderMasterlist l = uniqueMonth.Where(x => x.createdDate.Month == a.createdDate.Month && x.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (l == null)
                            {
                                a.TotalAmount = a.TotalAmount;
                                uniqueMonth.Add(a);
                            }
                            else
                            {
                                l.TotalAmount = l.TotalAmount + a.TotalAmount;
                            }
                            orderMasterlist m = uniqueOrdered.Where(c => c.OrderId == a.OrderId && (c.createdDate.Month == a.createdDate.Month && c.createdDate.Year == a.createdDate.Year)).SingleOrDefault();
                            if (m == null)
                            {
                                uniqueOrdered.Add(a);
                            }
                            orderMasterlist n = uniqueCustomer.Where(c => c.retaileId == a.retaileId && (c.createdDate.Month == a.createdDate.Month && c.createdDate.Year == a.createdDate.Year)).SingleOrDefault();
                            if (n == null)
                            {
                                uniqueCustomer.Add(a);
                            }
                        }

                        var d = start.Date;
                        var day = d.Month;
                        var year = d.Year;
                        do
                        {
                            orderMasterlist c = new orderMasterlist();
                            c.name = list[0].name;
                            c.month = d.Month;
                            c.year = d.Year;
                            c.createdDate = new DateTime(d.Year, d.Month, 1);
                            var total = uniqueMonth.Where(a => a.createdDate.Month == c.month && a.createdDate.Year == c.year).FirstOrDefault();
                            if (total == null)
                            {
                                c.TotalAmount = 0.00;
                            }
                            else
                            {
                                c.TotalAmount = total.TotalAmount;
                            }
                            List<orderMasterlist> order = uniqueOrdered.Where(a => a.createdDate.Month == c.month && a.createdDate.Year == c.year).ToList();
                            if (order.Count == 0)
                            {
                                c.totalOrder = 0;
                            }
                            else
                            {
                                c.totalOrder = order.Count;
                            }
                            List<orderMasterlist> retailer = uniqueCustomer.Where(a => a.createdDate.Month == c.month && a.createdDate.Year == c.year).ToList();
                            if (retailer.Count == 0)
                            {
                                c.activeRetailers = 0;
                            }
                            else
                            {
                                c.activeRetailers = retailer.Count;
                            }
                            report.Add(c);
                            d = d.AddMonths(1);
                            day = d.Month;
                            year = d.Year;
                        } while (day <= end.Month && year <= end.Year);
                        MainReport1.reports = report;
                        MainReport.Add(MainReport1);
                    }
                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }

        [Route("year")]
        [HttpGet]
        public dynamic GetYear(DateTime? datefrom, DateTime? dateto, int type, int value, string ids, int WarehouseId)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
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
                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                if (datefrom == null)
                {
                    datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = datefrom.GetValueOrDefault();
                    end = dateto.GetValueOrDefault();
                }
                var array = ids.Split(',');
                foreach (var iidd in array)
                {
                    int id = Convert.ToInt32(iidd);
                    var res = getdataWarehouse(datefrom, dateto, value, id, WarehouseId);
                    MainReports MainReport1 = new MainReports();
                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueYear = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        foreach (var a in list)
                        {
                            orderMasterlist l = uniqueYear.Where(x => x.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (l == null)
                            {
                                a.TotalAmount = a.TotalAmount;
                                uniqueYear.Add(a);
                            }
                            else
                            {
                                l.TotalAmount = l.TotalAmount + a.TotalAmount;
                            }
                            orderMasterlist m = uniqueOrdered.Where(c => c.OrderId == a.OrderId && c.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (m == null)
                            {
                                uniqueOrdered.Add(a);
                            }
                            orderMasterlist n = uniqueCustomer.Where(c => c.retaileId == a.retaileId && c.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (n == null)
                            {
                                uniqueCustomer.Add(a);
                            }
                        }
                        var d = start.Date;
                        var day = d.Year;
                        do
                        {
                            orderMasterlist c = new orderMasterlist();
                            c.name = list[0].name;
                            c.year = d.Year;
                            c.createdDate = new DateTime(d.Year, 1, 1);
                            var total = uniqueYear.Where(a => a.createdDate.Year == c.year).FirstOrDefault();
                            if (total == null)
                            {
                                c.TotalAmount = 0.00;
                            }
                            else
                            {
                                c.TotalAmount = total.TotalAmount;
                            }
                            List<orderMasterlist> order = uniqueOrdered.Where(a => a.createdDate.Year == c.year).ToList();
                            if (order.Count == 0)
                            {
                                c.totalOrder = 0;
                            }
                            else
                            {
                                c.totalOrder = order.Count;
                            }
                            List<orderMasterlist> retailer = uniqueCustomer.Where(a => a.createdDate.Year == c.year).ToList();
                            if (retailer.Count == 0)
                            {
                                c.activeRetailers = 0;
                            }
                            else
                            {
                                c.activeRetailers = retailer.Count;
                            }
                            report.Add(c);
                            d = d.AddYears(1);
                            day = d.Year;
                        } while (day <= end.Year);

                        MainReport1.reports = report;
                        MainReport.Add(MainReport1);
                    }
                }
                return MainReport;
            }
            catch (Exception ex)
            {
                logger.Error("Error in OrderMaster " + ex.Message);
                logger.Info("End  OrderMaster: ");
                return null;
            }
        }
        public List<orderMasterlist> getdataWarehouse(DateTime? datefrom, DateTime? dateto, int value, int id, int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0,
                        userid = 0;
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


                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();

                    if (datefrom != null && dateto != null)
                    {
                        if (value == 1)
                        {
                            if (WarehouseId == 0)
                            {
                                var data = (from i in db.DbOrderDetails
                                            where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid
                                            join j in db.itemMasters on i.ItemId equals j.ItemId
                                            join k in db.Categorys on j.Categoryid equals k.Categoryid
                                            select new orderMasterlist
                                            {
                                                cityid = i.CityId,
                                                warehouseid = i.WarehouseId,
                                                name = k.CategoryName,
                                                id = k.Categoryid,
                                                OrderId = i.OrderId,
                                                retaileId = i.CustomerId,
                                                TotalAmount = i.TotalAmt,
                                                createdDate = i.CreatedDate,
                                                updatedDate = i.UpdatedDate
                                            }).OrderBy(x => x.createdDate).ToList();
                                list = data.Where(x => x.id == id).ToList();
                            }
                            else
                            {
                                var data = (from i in db.DbOrderDetails
                                            where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid && i.WarehouseId == WarehouseId
                                            join j in db.itemMasters on i.ItemId equals j.ItemId
                                            join k in db.Categorys on j.Categoryid equals k.Categoryid
                                            select new orderMasterlist
                                            {
                                                cityid = i.CityId,
                                                warehouseid = i.WarehouseId,
                                                name = k.CategoryName,
                                                id = k.Categoryid,
                                                OrderId = i.OrderId,
                                                retaileId = i.CustomerId,
                                                TotalAmount = i.TotalAmt,
                                                createdDate = i.CreatedDate,
                                                updatedDate = i.UpdatedDate
                                            }).OrderBy(x => x.createdDate).ToList();
                                list = data.Where(x => x.id == id).ToList();
                            }
                        }
                        else if (value == 2)
                        {
                            if (WarehouseId == 0)
                            {
                                var data = (from i in db.DbOrderDetails
                                            where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid
                                            join j in db.itemMasters on i.ItemId equals j.ItemId
                                            join k in db.SubCategorys on j.SubCategoryId equals k.SubCategoryId
                                            select new orderMasterlist
                                            {
                                                cityid = i.CityId,
                                                warehouseid = i.WarehouseId,
                                                name = k.CategoryName.Trim() + " " + k.SubcategoryName.Trim(),
                                                id = k.SubCategoryId,
                                                OrderId = i.OrderId,
                                                retaileId = i.CustomerId,
                                                TotalAmount = i.TotalAmt,
                                                createdDate = i.CreatedDate,
                                                updatedDate = i.UpdatedDate
                                            }).OrderBy(x => x.createdDate).ToList();
                                list = data.Where(x => x.id == id).ToList();
                            }
                            else
                            {
                                var data = (from i in db.DbOrderDetails
                                            where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid && i.WarehouseId == WarehouseId
                                            join j in db.itemMasters on i.ItemId equals j.ItemId
                                            join k in db.SubCategorys on j.SubCategoryId equals k.SubCategoryId
                                            select new orderMasterlist
                                            {
                                                cityid = i.CityId,
                                                warehouseid = i.WarehouseId,
                                                name = k.CategoryName.Trim() + " " + k.SubcategoryName.Trim(),
                                                id = k.SubCategoryId,
                                                OrderId = i.OrderId,
                                                retaileId = i.CustomerId,
                                                TotalAmount = i.TotalAmt,
                                                createdDate = i.CreatedDate,
                                                updatedDate = i.UpdatedDate
                                            }).OrderBy(x => x.createdDate).ToList();
                                list = data.Where(x => x.id == id).ToList();
                            }
                        }
                        else if (value == 3)
                        {
                            if (WarehouseId == 0)
                            {
                                var data = (from i in db.DbOrderDetails
                                            where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid
                                            join j in db.itemMasters on i.ItemId equals j.ItemId
                                            join k in db.SubsubCategorys on j.SubsubCategoryid equals k.SubsubCategoryid
                                            select new orderMasterlist
                                            {
                                                cityid = i.CityId,
                                                warehouseid = i.WarehouseId,
                                                name = k.CategoryName.Trim() + " (" + k.SubcategoryName.Trim() + ")" + k.SubsubcategoryName.Trim(),
                                                id = k.SubsubCategoryid,
                                                OrderId = i.OrderId,
                                                retaileId = i.CustomerId,
                                                TotalAmount = i.TotalAmt,
                                                createdDate = i.CreatedDate,
                                                updatedDate = i.UpdatedDate
                                            }).OrderBy(x => x.createdDate).ToList();
                                list = data.Where(x => x.id == id).ToList();
                            }
                            else
                            {
                                var data = (from i in db.DbOrderDetails
                                            where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid && i.WarehouseId == WarehouseId
                                            join j in db.itemMasters on i.ItemId equals j.ItemId
                                            join k in db.SubsubCategorys on j.SubsubCategoryid equals k.SubsubCategoryid
                                            select new orderMasterlist
                                            {
                                                cityid = i.CityId,
                                                warehouseid = i.WarehouseId,
                                                name = k.CategoryName.Trim() + " (" + k.SubcategoryName.Trim() + ")" + k.SubsubcategoryName.Trim(),
                                                id = k.SubsubCategoryid,
                                                OrderId = i.OrderId,
                                                retaileId = i.CustomerId,
                                                TotalAmount = i.TotalAmt,
                                                createdDate = i.CreatedDate,
                                                updatedDate = i.UpdatedDate
                                            }).OrderBy(x => x.createdDate).ToList();
                                list = data.Where(x => x.id == id).ToList();
                            }
                        }
                        else if (value == 4)
                        {
                            if (WarehouseId == 0)
                            {
                                list = (from i in db.DbOrderDetails
                                        where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.ItemId == id && i.CompanyId == compid
                                        select new orderMasterlist
                                        {
                                            cityid = i.CityId,
                                            warehouseid = i.WarehouseId,
                                            name = i.itemname,
                                            id = i.ItemId,
                                            OrderId = i.OrderId,
                                            retaileId = i.CustomerId,
                                            TotalAmount = i.TotalAmt,
                                            createdDate = i.CreatedDate,
                                            updatedDate = i.UpdatedDate
                                        }).OrderBy(x => x.createdDate).ToList();
                            }
                            else
                            {
                                list = (from i in db.DbOrderDetails
                                        where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.ItemId == id && i.CompanyId == compid && i.WarehouseId == WarehouseId
                                        select new orderMasterlist
                                        {
                                            cityid = i.CityId,
                                            warehouseid = i.WarehouseId,
                                            name = i.itemname,
                                            id = i.ItemId,
                                            OrderId = i.OrderId,
                                            retaileId = i.CustomerId,
                                            TotalAmount = i.TotalAmt,
                                            createdDate = i.CreatedDate,
                                            updatedDate = i.UpdatedDate
                                        }).OrderBy(x => x.createdDate).ToList();
                            }
                        }

                    }
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderMaster " + ex.Message);
                    logger.Info("End  OrderMaster: ");
                    return null;
                }
            }
        }


        #endregion


        [Route("DailyOrderInventoryStatus")]
        [HttpGet]
        [AllowAnonymous]
        public bool DailyOrderInventoryStatus()
        {
            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
            var orderMasters = new List<MongoOrderMaster>();
            var orderStatusAmount = new List<OrderStatusAmount>();
            var hubInventory = new List<HubAvgInventory>();
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            var taskList = new List<Task>();

            var task1 = Task.Factory.StartNew(() =>
            {
                orderMasters = mongoDbHelper.Select(x => x.WarehouseId != 67 && !x.Deleted && (x.Status == "Shipped" || x.Status == "Issued" || x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch"
                                                    || x.Status == "Delivery Canceled" || x.Status == "Delivered")
                                , collectionName: "OrderMaster")
                                .ToList();

                orderStatusAmount = orderMasters.GroupBy(x => new { x.WarehouseId, x.Status }).Select(x => new OrderStatusAmount
                {
                    WarehouseId = x.Key.WarehouseId,
                    Status = x.Key.Status,
                    TotalAmount = x.Sum(z => z.DispatchAmount ?? z.GrossAmount)
                }).ToList();
            });

            taskList.Add(task1);


            var task2 = Task.Factory.StartNew(() =>
            {
                using (var context = new AuthContext())
                {
                    context.Database.CommandTimeout = 600;
                    //var query = "select c.WarehouseId, sum(c.CurrentInventory * item.purchasePrice) as Inventory from CurrentStocks c with (nolock) inner join GMWarehouseProgresses gw with (nolock) on c.WarehouseId=gw.WarehouseID and gw.IsLaunched=1 and c.WarehouseId!=67  cross apply(select max(i.netpurchaseprice) purchasePrice from  itemmasters i  with (nolock) where c.WarehouseId = i.WarehouseId and c.ItemNumber = i.Number and c.ItemMultiMRPId = i.ItemMultiMRPId and i.Deleted=0 group by i.WarehouseId, i.ItemMultiMRPId ) item group by c.WarehouseId";
                    //hubInventory = context.Database.SqlQuery<HubAvgInventory>(query).ToList();
                    var query = "Exec GetInventoryForWorkingCapital";
                    hubInventory = context.Database.SqlQuery<HubAvgInventory>(query).ToList();
                }
            });

            taskList.Add(task2);
            Task.WaitAll(taskList.ToArray());

            var warehouseIds = orderStatusAmount.Select(x => x.WarehouseId).Distinct().ToList();

            if (hubInventory != null && hubInventory.Any())
                warehouseIds.AddRange(hubInventory.Select(x => x.WarehouseId).ToList());

            warehouseIds = warehouseIds.Select(x => x).Distinct().ToList();

            var warehouses = new List<WarehouseMinDc>();

            using (var context = new AuthContext())
            {
                warehouses = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).Select(x => new WarehouseMinDc
                {
                    WarehouseId = x.WarehouseId,
                    WarehouseName = x.WarehouseName
                }).ToList();
            }

            List<DailyOrderInventoryData> reportData = new List<DailyOrderInventoryData>();

            //foreach (var item in orderStatusAmount.GroupBy(x => x.WarehouseId))
            //{
            //    var data = new DailyOrderInventoryData();
            //    data.WarehouseName = warehouses.FirstOrDefault(x => x.WarehouseId == item.Key)?.WarehouseName;
            //    data.DeliveryCanceledAmount = item.Any(x => x.Status == "Delivery Canceled") ? item.FirstOrDefault(x => x.Status == "Delivery Canceled").TotalAmount : 0;
            //    data.DeliveryRedispatchAmount = item.Any(x => x.Status == "Delivery Redispatch") ? item.FirstOrDefault(x => x.Status == "Delivery Redispatch").TotalAmount : 0;
            //    data.IssuedAmount = item.Any(x => x.Status == "Issued") ? item.FirstOrDefault(x => x.Status == "Issued").TotalAmount : 0;
            //    data.ReadyToDispatchAmount = item.Any(x => x.Status == "Ready to Dispatch") ? item.FirstOrDefault(x => x.Status == "Ready to Dispatch").TotalAmount : 0;
            //    data.ShippedAmount = item.Any(x => x.Status == "Shipped") ? item.FirstOrDefault(x => x.Status == "Shipped").TotalAmount : 0;
            //    data.InventoryAmount = hubInventory.Any(x => x.WarehouseId == item.Key) ? hubInventory.FirstOrDefault(x => x.WarehouseId == item.Key).Inventory : 0;
            //    data.DeliveredButNotReconciled = item.Any(x => x.Status == "Payment Submitted") ? item.FirstOrDefault(x => x.Status == "Payment Submitted").TotalAmount : 0;
            //    reportData.Add(data);
            //}


            foreach (var item in warehouses)
            {
                var data = new DailyOrderInventoryData();

                data.WarehouseName = item.WarehouseName;
                data.DeliveryCanceledAmount = orderStatusAmount.Any(x => x.Status == "Delivery Canceled" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Delivery Canceled" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
                data.DeliveryRedispatchAmount = orderStatusAmount.Any(x => x.Status == "Delivery Redispatch" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Delivery Redispatch" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
                data.IssuedAmount = orderStatusAmount.Any(x => x.Status == "Issued" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Issued" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
                data.ReadyToDispatchAmount = orderStatusAmount.Any(x => x.Status == "Ready to Dispatch" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Ready to Dispatch" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
                data.ShippedAmount = orderStatusAmount.Any(x => x.Status == "Shipped" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Shipped" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
                data.InventoryAmount = hubInventory.Any(x => x.WarehouseId == item.WarehouseId) ? hubInventory.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Inventory : 0;
                data.DeliveredButNotReconciled = orderStatusAmount.Any(x => x.Status == "Payment Submitted" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Payment Submitted" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
                reportData.Add(data);
            }



            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);


            var dataTables = new List<DataTable>();
            DataTable dt = ClassToDataTable.CreateDataTable(reportData);
            dt.TableName = "DailyOrderStatus_Inventory";
            dataTables.Add(dt);



            foreach (var item in orderMasters.GroupBy(x => x.Status))
            {
                List<OrderSummaryStatusWise> statusWiseOrders = new List<OrderSummaryStatusWise>();
                DataTable statusDatatable = new DataTable();
                foreach (var order in item)
                {
                    statusWiseOrders.Add(new OrderSummaryStatusWise
                    {
                        OrderId = order.OrderId,
                        Status = item.Key,
                        WarehouseId = order.WarehouseId,
                        WarehouseName = warehouses.FirstOrDefault(x => x.WarehouseId == order.WarehouseId)?.WarehouseName,
                        DispatchAmount = order.DispatchAmount ?? order.GrossAmount
                    });
                }

                statusDatatable = ClassToDataTable.CreateDataTable(statusWiseOrders);
                statusDatatable.TableName = item.Key;
                dataTables.Add(statusDatatable);
            }


            string filePath = ExcelSavePath + "DailyOrderStatus_Inventory_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";

            if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
            {

                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                string To = "", From = "", Bcc = "";
                DataTable emaildatatable = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='DailyOrderStatusInventory'", connection))
                    {

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(emaildatatable);
                        da.Dispose();
                        connection.Close();
                    }
                }
                if (emaildatatable.Rows.Count > 0)
                {
                    To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                    From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                    Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                }
                string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Order Status & Inventory Report";
                string message = "Please find attach Daily Order Status & Inventory Report";
                if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                    EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                else
                    logger.Error("Daily Order Status & Inventory Report To and From empty");

            }

            return true;


        }




        //Praveen 07-11-2019
        #region StockInTransit 

        [Route("GetStockInTransitReport")]
        [HttpGet]
        public dynamic GetStockInTransitReport(string warehouseId, DateTime from, DateTime to)
        {

            using (var con = new AuthContext())
            {
                var param = new SqlParameter("startDate", from);
                var param1 = new SqlParameter("endDate", to);
                var param2 = new SqlParameter("warehouseId", warehouseId);

                var ordHistories = con.Database.SqlQuery<StockTransit>("exec GetStockInTransitData @startDate,@endDate,@warehouseId", param, param1, param2).ToList();

                return Ok(ordHistories);
            }
        }

        #endregion


        [Route("GetWorkingCapital")]
        [HttpPost]
        public DisplayWorkingCapitalDc GetWorkingCapital(GetWorkingCapitalfilters filter)
        {
            DisplayWorkingCapitalDc displayWorkingCapitalDc = new DisplayWorkingCapitalDc();
            MongoDbHelper<WorkingCapitalData> mongoDbHelper = new MongoDbHelper<WorkingCapitalData>();
            var wcDC = new WorkingCapitalDC();
            DateTime date = DateTime.Now;
            if (filter.endDate.HasValue && filter.startDate.HasValue)
            {
                date = filter.startDate.Value;
            }
            else
            {
                if (DateTime.Now.AddDays(-7).Month == DateTime.Now.Month)
                {
                    date = DateTime.Now.AddDays(-7);
                    filter.endDate = DateTime.Now.Date;
                    filter.startDate = date.Date;
                }
                else
                {
                    date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    filter.startDate = date.Date;
                    filter.endDate = DateTime.Now.Date;
                }
            }
            DateTime monthStartDate = new DateTime(filter.Month.Year, filter.Month.Month, 1);
            DateTime monthendDate = monthStartDate.AddMonths(1).AddDays(-1);
            var data = mongoDbHelper.Select(x => filter.warehouseids.Contains(x.WarehouseId) && x.CreateDate >= monthStartDate && x.CreateDate <= monthendDate).ToList();


            var result = data.Select(x => new WorkingCapitalDC
            {
                WarehouseName = x.WarehouseName,
                WarehouseId = x.WarehouseId,
                AvgSale = x.AvgSale,
                AgentDues = x.AgentDues,
                ShippedAmount = x.ShippedAmount,
                IssuedAmount = x.IssuedAmount,
                ReadyToDispatchAmount = x.ReadyToDispatchAmount,
                DeliveryRedispatchAmount = x.DeliveryRedispatchAmount,
                DeliveryCanceledAmount = x.DeliveryCanceledAmount,
                InventoryAmount = x.InventoryAmount,
                DeliveredButNotReconciled = x.DeliveredButNotReconciled,
                CashInOperation = x.CashInOperation,
                ChequeInOperation = Convert.ToDouble(x.ChequeInOperation),
                ChequeInBank = Convert.ToDouble(x.ChequeInBank),
                ChequeBounce = Convert.ToDouble(x.ChequeBounce),
                SupplierCredit = x.SupplierCredit,
                SupplierAdvances = x.SupplierAdvances,
                Invoiceintransit = x.Invoiceintransit,
                CreateDate = x.CreateDate.Date,
                AverageAssetDays = x.AverageAssetDays,
                DamageStockAmount = x.DamageStockAmount,
                NonSellableStockAmount = x.NonSellableStockAmount.HasValue ? x.NonSellableStockAmount.Value : 0,
                OnlinePrePaidAmount = x.OnlinePrePaidAmount,
                OnlinePayments = (x.OnlinePayments == null || !x.OnlinePayments.Any()) && x.OnlinePrePaidAmountePaylater.HasValue && x.OnlinePrePaidAmounthdfc.HasValue ? new List<OnlinePayment> { new OnlinePayment { Amount = x.OnlinePrePaidAmountePaylater, PaymentFrom = "ePaylater" }, new OnlinePayment { Amount = x.OnlinePrePaidAmounthdfc, PaymentFrom = "hdfc" }, new OnlinePayment { Amount = x.OnlinePrePaidAmountmPos, PaymentFrom = "mPos" }, new OnlinePayment { Amount = x.OnlinePrePaidAmounthdfc, PaymentFrom = "UPI" } } : x.OnlinePayments,
                OnlinePrePaidAmountePaylater = x.OnlinePrePaidAmountePaylater,
                OnlinePrePaidAmounthdfc = x.OnlinePrePaidAmounthdfc,
                OnlinePrePaidAmountmPos = x.OnlinePrePaidAmountmPos,
                PendingGRNAmount = x.PendingGRNAmount,
                GoodsReceivedNotInvoiced = x.GoodsReceivedNotInvoiced,
                IRPendingBuyerSide = x.IRPendingBuyerSide
            }).ToList();

            if (result != null && result.Any())
            {
                MongoDbHelper<WorkingCapitalCalander> WCmongoDbHelper = new MongoDbHelper<WorkingCapitalCalander>();
                WorkingCapitalCalander calanderdata = WCmongoDbHelper.Select(x => x.Year == DateTime.Now.Year && x.Month == DateTime.Now.Month && x.IsDeleted == false).FirstOrDefault();
                var monthday = calanderdata != null && calanderdata.DaysList.Any() ? calanderdata.DaysList.Where(x => !x.IsHoliday).Select(x => x.date.Day).ToList() : GetDates(DateTime.Now.Year, DateTime.Now.Month).Select(x => x.Day).ToList();
                result.ForEach(x =>
                {
                    if (result.Any(y => y.CreateDate < x.CreateDate && y.WarehouseId == x.WarehouseId))
                    {
                        x.IncreaseInWC = result.FirstOrDefault(y => y.CreateDate < x.CreateDate && y.WarehouseId == x.WarehouseId).WorkingCapital - x.WorkingCapital;
                    }
                    else
                    {
                        x.IncreaseInWC = 0;
                    }
                });
                var totalDay = DateTime.Now.AddDays(-1).Day;
                displayWorkingCapitalDc.newdc = result.GroupBy(x => x.CreateDate).Select(y => new newdc
                {
                    CreateDate = y.Key,
                    WorkingCapitalDCs = y.Select(z => new WorkingCapitalDC
                    {
                        AvgSale = y.Sum(k => k.AvgSale),
                        AgentDues = y.Sum(k => k.AgentDues),
                        ShippedAmount = y.Sum(k => k.ShippedAmount),
                        IssuedAmount = y.Sum(k => k.IssuedAmount),
                        ReadyToDispatchAmount = y.Sum(k => k.ReadyToDispatchAmount),
                        DeliveryRedispatchAmount = y.Sum(k => k.DeliveryRedispatchAmount),
                        DeliveryCanceledAmount = y.Sum(k => k.DeliveryCanceledAmount),
                        InventoryAmount = y.Sum(k => k.InventoryAmount),
                        DeliveredButNotReconciled = y.Sum(k => k.DeliveredButNotReconciled),
                        CashInOperation = y.Sum(k => k.CashInOperation),
                        ChequeInOperation = Convert.ToDouble(y.Sum(k => k.ChequeInOperation)),
                        ChequeInBank = Convert.ToDouble(y.Sum(k => k.ChequeInBank)),
                        ChequeBounce = Convert.ToDouble(y.Sum(k => k.ChequeBounce)),
                        SupplierCredit = -1 * y.Sum(k => k.SupplierCredit),
                        SupplierAdvances = y.Sum(k => k.SupplierAdvances),
                        Invoiceintransit = y.Sum(k => k.Invoiceintransit),
                        IncreaseInWC = y.Sum(k => k.IncreaseInWC),
                        DamageStockAmount = y.Sum(k => k.DamageStockAmount),
                        NonSellableStockAmount = y.Sum(k => k.NonSellableStockAmount),
                        OnlinePayments = y.SelectMany(k => k.OnlinePayments).GroupBy(t => t.PaymentFrom).Select(s => new OnlinePayment { PaymentFrom = s.Key, Amount = s.Sum(p => p.Amount) }).ToList(),
                        OnlinePrePaidAmount = y.Sum(k => k.OnlinePrePaidAmount),
                        OnlinePrePaidAmountePaylater = y.Sum(k => k.OnlinePrePaidAmountePaylater),
                        OnlinePrePaidAmounthdfc = y.Sum(k => k.OnlinePrePaidAmounthdfc),
                        OnlinePrePaidAmountmPos = y.Sum(k => k.OnlinePrePaidAmountmPos),
                        PendingGRNAmount = y.Sum(k => k.PendingGRNAmount),
                        GoodsReceivedNotInvoiced = y.Sum(k => k.GoodsReceivedNotInvoiced),
                        IRPendingBuyerSide = y.Sum(k => k.IRPendingBuyerSide),
                    }).FirstOrDefault()
                }).OrderByDescending(x => x.CreateDate).ToList();
                displayWorkingCapitalDc.AverageRTDDays = displayWorkingCapitalDc.newdc.Where(x => monthday.Contains(x.CreateDate.Day)).Select(x => x.WorkingCapitalDCs).Sum(x => x.ReadyToDispatchDays) / monthday.Where(x => x <= DateTime.Now.Day - 1).Count();
                displayWorkingCapitalDc.AverageInventoryDays = displayWorkingCapitalDc.newdc.Where(x => monthday.Contains(x.CreateDate.Day)).Select(x => x.WorkingCapitalDCs).Sum(x => x.InventoryDays) / monthday.Where(x => x <= DateTime.Now.Day - 1).Count();
                displayWorkingCapitalDc.AverageAssetDays = displayWorkingCapitalDc.newdc.Where(x => monthday.Contains(x.CreateDate.Day)).Select(x => x.WorkingCapitalDCs).Sum(x => x.AssetDays) / monthday.Where(x => x <= DateTime.Now.Day - 1).Count();
                displayWorkingCapitalDc.AverageCashDays = displayWorkingCapitalDc.newdc.Where(x => monthday.Contains(x.CreateDate.Day)).Select(x => x.WorkingCapitalDCs).Sum(x => x.CashDays) / monthday.Where(x => x <= DateTime.Now.Day - 1).Count();
                displayWorkingCapitalDc.NetWorkingCapital = displayWorkingCapitalDc.newdc.Where(x => monthday.Contains(x.CreateDate.Day)).Select(x => x.WorkingCapitalDCs).Sum(x => x.WorkingCapital) / monthday.Where(x => x <= DateTime.Now.Day - 1).Count();
                displayWorkingCapitalDc.AvgSuplierCreditDays = displayWorkingCapitalDc.newdc.Where(x => monthday.Contains(x.CreateDate.Day)).Select(x => x.WorkingCapitalDCs).Sum(x => x.SupplierCreditDays) / monthday.Where(x => x <= DateTime.Now.Day - 1).Count();

            }
            return displayWorkingCapitalDc;
        }

        private static List<DateTime> GetDates(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(year, month, day)) // Map each day to a date
                             .ToList(); // Load dates into a list
        }

        [Route("GetWorkingCapitalById")]
        [HttpGet]
        public IHttpActionResult GetWorkingCapitalById(int warehouseid, DateTime date)
        {
            //var searchDate = date;
            //DateTime monthStartDate = new DateTime(date.Year, date.Month, date.Day);
            MongoDbHelper<WorkingCapitalData> mongoDbHelper = new MongoDbHelper<WorkingCapitalData>();
            var data = mongoDbHelper.Select(x => x.WarehouseId == warehouseid && x.CreateDate == date).FirstOrDefault();
            return Ok(data);
        }

        [Route("UpdateWorkingCaptialById")]
        [HttpPut]
        public IHttpActionResult UpdateWorkingCaptialById(GetWorkingCapitalFields data)
        {
            MongoDbHelper<WorkingCapitalData> mongoDbHelper = new MongoDbHelper<WorkingCapitalData>();

            try
            {
                if (data != null)
                {
                    var Id = new ObjectId(data.Id);
                    var getdata = mongoDbHelper.Select(x => x.Id == Id).FirstOrDefault();

                    //  var id = new Object(getdata.Id);
                    getdata.ChequeBounce = data.ChequeBounce;
                    getdata.AgentDues = data.AgentDues;
                    getdata.SupplierCredit = data.SupplierCredit;
                    getdata.SupplierAdvances = data.SupplierAdvances;
                    mongoDbHelper.Replace(Id, getdata);
                    return Ok(data);
                }
                return Ok(data);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }



        }

        [Route("GetOrderStatusReport")]
        [HttpPost]
        public IHttpActionResult GetOrderStatusReport(OrderStatusParamRequest param)
        {

            using (AuthContext db = new AuthContext())
            {
                //DateTime start_Date = TimeZoneInfo.ConvertTimeFromUtc(param.StartDate, INDIAN_ZONE);
                //DateTime end_date = TimeZoneInfo.ConvertTimeFromUtc(param.EndDate, INDIAN_ZONE);
                var WareIdDtw = new DataTable();
                WareIdDtw.Columns.Add("IntValue");
                foreach (var item in param.Warehouseid)
                {
                    var dr = WareIdDtw.NewRow();
                    dr["IntValue"] = item;
                    WareIdDtw.Rows.Add(dr);
                }

                var Warehouseid = new SqlParameter
                {
                    ParameterName = "warehouseid",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = WareIdDtw
                };

                var ClusterIdDtw = new DataTable();
                ClusterIdDtw.Columns.Add("IntValue");
                foreach (var item in param.Clusterid)
                {
                    var dr = ClusterIdDtw.NewRow();
                    dr["IntValue"] = item;
                    ClusterIdDtw.Rows.Add(dr);
                }

                var Clusterid = new SqlParameter
                {
                    ParameterName = "clusterid",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = ClusterIdDtw
                };
                var StartDate = new SqlParameter
                {
                    ParameterName = "startDate",
                    Value = param.StartDate.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var EndDate = new SqlParameter
                {
                    ParameterName = "endDate",
                    Value = param.EndDate.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var result = db.Database.SqlQuery<GetOrderStatusReportDC>("GetOrderStatusCountSaleReport @startDate,@endDate,@WarehouseId,@ClusterId", StartDate, EndDate, Warehouseid, Clusterid).ToList();

                if (result != null && result.Any())
                {
                    result = result.GroupBy(x => new { x.ClusterId, x.ClusterName, x.WarehouseId, x.WarehouseName }).Select(x =>
                      new GetOrderStatusReportDC
                      {
                          ClusterId = x.Key.ClusterId,
                          ClusterName = x.Key.ClusterName,
                          WarehouseId = x.Key.WarehouseId,
                          WarehouseName = x.Key.WarehouseName,
                          AgentName = string.Join(",", x.Where(y => !string.IsNullOrEmpty(y.AgentName)).Select(y => y.AgentName).Distinct().ToList()),
                          DeliveredCount = x.Sum(y => y.DeliveredCount),
                          DeliveredValue = x.Sum(y => y.DeliveredValue),
                          DeliveryRedispatchCount = x.Sum(y => y.DeliveryRedispatchCount),
                          DeliveryRedispatchValue = x.Sum(y => y.DeliveryRedispatchValue),
                          IssuedCount = x.Sum(y => y.IssuedCount),
                          IssuedValue = x.Sum(y => y.IssuedValue),
                          PendingCount = x.Sum(y => y.PendingCount),
                          PendingValue = x.Sum(y => y.PendingValue),
                          ReadytoDispatchCount = x.Sum(y => y.ReadytoDispatchCount),
                          ReadytoDispatchValue = x.Sum(y => y.ReadytoDispatchValue),
                          ShippedCount = x.Sum(y => y.ShippedCount),
                          ShippedValue = x.Sum(y => y.ShippedValue),
                          InTransitCount = x.Sum(y => y.ShippedCount) + x.Sum(y => y.IssuedCount) + x.Sum(y => y.DeliveryRedispatchCount),
                          InTransitValue = x.Sum(y => y.ShippedValue) + x.Sum(y => y.IssuedValue) + x.Sum(y => y.DeliveryRedispatchValue),
                      }
                    ).ToList();
                }
                return Ok(result);
            }
        }

        [Route("GetOrderColorReport")]
        [HttpPost]
        public dynamic GetOrderColorReport(OrderColorRequest orderColorRequest)
        {
            using (AuthContext db = new AuthContext())
            {
                //DateTime start_Date = TimeZoneInfo.ConvertTimeFromUtc(param.StartDate, INDIAN_ZONE);
                //DateTime end_date = TimeZoneInfo.ConvertTimeFromUtc(param.EndDate, INDIAN_ZONE);
                var WareIdDtw = new DataTable();
                WareIdDtw.Columns.Add("IntValue");
                foreach (var item in orderColorRequest.WarehouseIds)
                {
                    var dr = WareIdDtw.NewRow();
                    dr["IntValue"] = item;
                    WareIdDtw.Rows.Add(dr);
                }

                var Warehouseid = new SqlParameter
                {
                    ParameterName = "WarehouseId",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = WareIdDtw
                };

                var StartDate = new SqlParameter
                {
                    ParameterName = "StartDate",
                    Value = orderColorRequest.StartDate
                };
                var EndDate = new SqlParameter
                {
                    ParameterName = "EndDate",
                    Value = orderColorRequest.EndDate
                };
                var ReportType = new SqlParameter
                {
                    ParameterName = "ReportType",
                    Value = orderColorRequest.ReportType
                };

                var result = db.Database.SqlQuery<OrderColorDc>("GetPendingOrderWithColor @WarehouseId,@StartDate,@EndDate,@ReportType", Warehouseid, StartDate, EndDate, ReportType).ToList();
                List<DisplayOrderColorCount> LstDisplayOrderColorCount = new List<DisplayOrderColorCount>();
                List<DisplayOrderColorAmount> LstDisplayOrderColorAmount = new List<DisplayOrderColorAmount>();
                List<DisplayOrderCountTime> LstDisplayOrderCountTime = new List<DisplayOrderCountTime>();
                if (result != null && result.Any())
                {
                    //result.Where(x => x.QtyAvailableStatus == 1).ToList().ForEach(x =>
                    //       x.QtyAvailableStatus = (x.TotalAmount * 4 / 100 >= (result.Where(y => y.QtyAvailableStatus == 1 && y.orderid == x.orderid && !y.IsFreeItem).Sum(y => y.detailTotalAmount)) ? 3 : 1)
                    //);

                    var Orders = result.GroupBy(x => new { x.WarehouseId, x.WarehouseName, x.orderid }).Select(x => new
                    {
                        WarehouseId = x.Key.WarehouseId,
                        WarehouseName = x.Key.WarehouseName,
                        orderid = x.Key.orderid,
                        RedOrderCount = x.Any(y => y.QtyAvailableStatus == 1) ? 1 : 0,
                        YellowOrderCount = !x.Any(y => y.QtyAvailableStatus == 1) && x.Any(y => y.QtyAvailableStatus == 3) ? 1 : 0,
                        BlueOrderCount = x.All(y => y.QtyAvailableStatus == 2) ? 1 : 0,
                        WhiteOrderCount = x.Any(y => y.QtyAvailableStatus == 0) && x.All(y => y.QtyAvailableStatus == 0 || y.QtyAvailableStatus == 2) ? 1 : 0,
                        TotalAmount = x.FirstOrDefault().TotalAmount,
                        CreatedDate = x.FirstOrDefault().CreatedDate,
                    }).ToList();
                    if (orderColorRequest.ReportType == "OrderColorCount")
                    {
                        LstDisplayOrderColorCount = Orders.GroupBy(x => new { x.WarehouseId, x.WarehouseName })
                                                   .Select(x => new DisplayOrderColorCount
                                                   {
                                                       WarehouseId = x.Key.WarehouseId,
                                                       WarehouseName = x.Key.WarehouseName,
                                                       YellowOrderCount = x.Sum(y => y.YellowOrderCount),
                                                       BlueOrderCount = x.Sum(y => y.BlueOrderCount),
                                                       RedOrderCount = x.Sum(y => y.RedOrderCount),
                                                       WhiteOrderCount = x.Sum(y => y.WhiteOrderCount),
                                                   }).ToList();
                    }
                    else if (orderColorRequest.ReportType == "OrderColorAmount")
                    {
                        LstDisplayOrderColorAmount = Orders.GroupBy(x => new { x.WarehouseId, x.WarehouseName })
                                                   .Select(x => new DisplayOrderColorAmount
                                                   {
                                                       WarehouseId = x.Key.WarehouseId,
                                                       WarehouseName = x.Key.WarehouseName,
                                                       YellowOrderAmount = x.Where(y => y.YellowOrderCount > 0).Sum(y => y.TotalAmount),
                                                       BlueOrderAmount = x.Where(y => y.BlueOrderCount > 0).Sum(y => y.TotalAmount),
                                                       RedOrderAmount = x.Where(y => y.RedOrderCount > 0).Sum(y => y.TotalAmount),
                                                       WhiteOrderAmount = x.Where(y => y.WhiteOrderCount > 0).Sum(y => y.TotalAmount),
                                                   }).ToList();
                    }
                    else
                    {
                        List<OrderCountTime> orderCountTimes = Orders
                                                   .Select(x => new OrderCountTime
                                                   {
                                                       WarehouseId = x.WarehouseId,
                                                       WarehouseName = x.WarehouseName,
                                                       CreatedDate = x.CreatedDate,
                                                       OrderId = x.orderid
                                                   }).ToList();

                        LstDisplayOrderCountTime = orderCountTimes.GroupBy(x => new { x.WarehouseId, x.WarehouseName })
                            .Select(x => new DisplayOrderCountTime
                            {
                                WarehouseId = x.Key.WarehouseId,
                                WarehouseName = x.Key.WarehouseName,
                                Hour24OrderCount = x.Sum(y => y.Hour24OrderCount),
                                Hour48OrderCount = x.Sum(y => y.Hour48OrderCount),
                                Hour72OrderCount = x.Sum(y => y.Hour72OrderCount),
                                Hour100OrderCount = x.Sum(y => y.Hour100OrderCount),
                                Hour100GreaterOrderCount = x.Sum(y => y.Hour100GreaterOrderCount),
                            }).ToList();
                    }
                }

                if (orderColorRequest.ReportType == "OrderColorAmount")
                {
                    return LstDisplayOrderColorAmount;
                }
                else if (orderColorRequest.ReportType == "OrderColorCount")
                {
                    return LstDisplayOrderColorCount;
                }
                else
                {
                    return LstDisplayOrderCountTime;
                }

            }
        }


        [Route("OPReportExport")]
        [HttpPost]
        public OP_Report_Export_Response OPReportExport(OrderColorRequest orderColorRequest)
        {
            OP_Report_Export_Response res = new OP_Report_Export_Response();
            string fileUrl = "";
            using (AuthContext db = new AuthContext())
            {
                if (orderColorRequest != null && orderColorRequest.ReportType != "OrderColorTime")
                {
                    var WareIdDtw = new DataTable();
                    WareIdDtw.Columns.Add("IntValue");
                    foreach (var item in orderColorRequest.WarehouseIds)
                    {
                        var dr = WareIdDtw.NewRow();
                        dr["IntValue"] = item;
                        WareIdDtw.Rows.Add(dr);
                    }

                    var Warehouseid = new SqlParameter
                    {
                        ParameterName = "WarehouseId",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IntValues",
                        Value = WareIdDtw
                    };

                    var StartDate = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = orderColorRequest.StartDate
                    };
                    var EndDate = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = orderColorRequest.EndDate
                    };


                    var result = db.Database.SqlQuery<OPReportExportDc>("OPReportExport @WarehouseId,@StartDate,@EndDate", Warehouseid, StartDate, EndDate).ToList();
                    if (result != null && result.Count() > 0)
                    {
                        DataTable dt = ListtoDataTableConverter.ToDataTable(result);
                        string fileName = "OP_Report" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                        string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                        ExcelGenerator.DataTable_To_Excel(dt, "OP_Report", path);

                        fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                            , HttpContext.Current.Request.Url.DnsSafeHost
                                                                            , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                           , string.Format("ExcelGeneratePath/{0}", fileName));
                    }
                    if (fileUrl != null && fileUrl != "")
                    {
                        res.Message = "Generated Succesfully.";
                        res.Status = true;
                        res.Data = fileUrl;
                    }
                    else
                    {
                        res.Message = "Not Generated.";
                        res.Status = false;
                        res.Data = fileUrl;
                    }
                }
                else
                {
                    res.Message = "Can't Generate for OrderColorTime.";
                    res.Status = false;
                    res.Data = fileUrl;
                }
            }
            return res;
        }


        [Route("DailyPRGRReport")]
        [HttpGet]
        public async Task<bool> DailyPRGRReport()
        {
            var table = new DataTable();
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 6000;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = @"exec DailyPRGRReportData";
                cmd.Connection.Open();
                table.Load(cmd.ExecuteReader());
            }


            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);


            if (table.Rows.Count > 0)
            {
                string filePath = ExcelSavePath + "DailyPRGR_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";

                if (ExcelGenerator.DataTable_To_Excel(table, "DailyPRGRReport", filePath))
                {

                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='DailyPRGRReport'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Purchase Request GR Report";
                    string message = "Please find attach Daily Purchase Request GR Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Daily Order Status & Inventory Report To and From empty");

                }
            }
            else
                logger.Error("Daily PR GR repoet data is empty");

            //Run GenerateItemSalesInventoryData 
            var reportManager = new Managers.ReportManager();
            bool rsult = await reportManager.GenerateItemSalesInventoryData();
            bool gullakPaymentList = await reportManager.GetGullakInPayments();
            return true;
        }


        [Route("DailyPendingBuyerApproval")]
        [HttpGet]
        public bool DailyPendingBuyer()
        {
            var table = new DataTable();
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 600;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "Select IR.PurchaseOrderId,IR.IRID,IR.SupplierName,W.WarehouseName" +
                                  ",IR.TotalAmount,IR.IRStatus,IR.Gstamt,IR.Discount,IR.IRAmountWithTax" +
                                  ",IR.IRAmountWithOutTax,IR.TotalAmountRemaining,IR.PaymentStatus" +
                                  ",IR.IRType,IR.CreatedBy,convert(varchar,IR.CreationDate, 103) CreationDate,IR.Deleted,IR.Progres,IR.Remark" +
                                 ",IR.RejectedComment,IR.BuyerId,IR.BuyerName,IR.ApprovedComment,IR.OtherAmount" +
                                 ",IR.OtherAmountRemark,IR.ExpenseAmount,IR.ExpenseAmountRemark,IR.RoundofAmount" +
                                 ",IR.ExpenseAmountType,IR.OtherAmountType,IR.OtherAmountType,IR.CashDiscount" +
                                 ",IR.FreightAmount,convert(varchar,IR.InvoiceDate, 103) InvoiceDate from IRMasters IR INNER JOIN Warehouses W ON IR.WarehouseId = W.WarehouseId Where IR.IRStatus = 'Pending from Buyer side' and IR.Deleted = 0";

                cmd.Connection.Open();
                table.Load(cmd.ExecuteReader());
            }


            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);


            if (table.Rows.Count > 0)
            {
                string filePath = ExcelSavePath + "DailyBuyerPendingReport_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";

                if (ExcelGenerator.DataTable_To_Excel(table, "DailyPendingBuyerApproval", filePath))
                {

                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='DailyPendingBuyerApproval'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Pending Buyer Approval Report";
                    string message = "Please find attach  Daily Pending Buyer Approval Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Daily Order Status & Inventory Report To and From empty");

                }
            }
            else
                logger.Error("Daily  Pending Buyer Approval data is empty");

            return true;


        }

        [Route("InsertRetailerLogData")]
        [HttpGet]
        [AllowAnonymous]
        public bool InsertRetailerLogData(string CollectionName)
        {
            MongoDbHelper<Model.CustomerShoppingCart.CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<Model.CustomerShoppingCart.CustomerShoppingCart>();
            IMongoDatabase db = mongoDbHelper.mongoDatabase;//    dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            //var CollectionName = "TraceLog_" + DateTime.Now.AddDays(-1).ToString(@"MMddyyyy");
            var cmd = new JsonCommand<BsonDocument>("{ eval: \"InsertRetailerLog('" + CollectionName + "')\" }");
            var result = db.RunCommand(cmd);
            return true;
        }

        [Route("GetLowInventory")]
        [HttpGet]
        [AllowAnonymous]
        public bool GetLowInventory()
        {
            Managers.ReportManager reportManager = new Managers.ReportManager();
            return reportManager.GetCatSubCatLiveItem();
        }


        [Route("GenerateItemClassificationInActiveReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GenerateItemClassificationInActiveReport()
        {
            var reportManager = new Managers.ReportManager();
            return await reportManager.GenerateItemClassificationInActiveReport();
        }

        [Route("GenerateCRFReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GenerateCRFReport()
        {
            var reportManager = new Managers.ReportManager();
            return await reportManager.GenerateCFRReport();
        }
        [Route("GenerateinActiveReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GenerateinActiveReport()
        {
            var reportManager = new Managers.ReportManager();
            return await reportManager.GenerateinActivePeopleReport();
        }

        [Route("OrderNotReflectLadger")]
        [HttpGet]
        [AllowAnonymous]
        public bool OrderNotReflectLadger()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            using (var context = new AuthContext())
            {
                List<OrderMasterDC> RTDLedger = context.Database.SqlQuery<OrderMasterDC>("select OM.OrderId,om.WarehouseName,OM.Status,OM.Skcode,OM.CustomerName,OM.Customerphonenum as Mobile,OM.invoice_no,om.GrossAmount,om.CreatedDate from OrderMasters OM  "
                           + "  LEFT JOIN LadgerEntries LE ON OM.OrderId = LE.ObjectID AND LE.ObjectType = 'Order' AND LE.VouchersTypeID = 1 "
                           + "  where "
                           + "  OM.CreatedDate > '2020-01-01' and "
                           + "  OM.Status NOT IN('Pending', 'Inactive', 'Failed', 'Dummy Order Cancelled', 'init', 'Order Canceled', 'Payment Pending') "
                           + "  and LE.ID IS NULL "
                           + "  order by OM.CreatedDate DESC").ToList();

                List<OrderMasterDC> POCLedger = context.Database.SqlQuery<OrderMasterDC>("select OM.OrderId,om.WarehouseName,OM.Status,OM.Skcode,OM.CustomerName,OM.Customerphonenum as Mobile,OM.invoice_no,om.GrossAmount,om.CreatedDate from OrderMasters OM  "
                          + "  LEFT JOIN LadgerEntries LE "
                          + "  ON OM.OrderId = LE.ObjectID AND LE.ObjectType = 'Order'  and Le.VouchersTypeID in (20) and LE.Debit > 0 "
                          + "  WHERE OM.Status in ('Post Order Canceled') AND LE.ID IS NULL "
                          + "  AND OM.CreatedDate > '2019-03-31' "
                          + "   ORDER BY OM.DeliveredDate DESC").ToList();

                List<OrderMasterDC> SettleLedger = context.Database.SqlQuery<OrderMasterDC>("select OM.OrderId,om.WarehouseName,OM.Status,OM.Skcode,OM.CustomerName,OM.Customerphonenum as Mobile,OM.invoice_no,om.GrossAmount,om.CreatedDate from OrderMasters OM  "
                          + "  LEFT JOIN LadgerEntries LE "
                          + "  ON OM.OrderId = LE.ObjectID AND LE.ObjectType = 'Order'  and Le.VouchersTypeID in (2, 3) and LE.Credit > 0 "
                          + "  WHERE OM.Status in ('sattled') AND LE.ID IS NULL "
                          + "  AND OM.DeliveredDate > '2019-03-31' "
                          + "  ORDER BY OM.DeliveredDate DESC").ToList();


                if (!Directory.Exists(ExcelSavePath))
                    Directory.CreateDirectory(ExcelSavePath);
                #region Create dataset and excel file for them
                var dataTables = new List<DataTable>();
                var rtdLedger = ClassToDataTable.CreateDataTable(RTDLedger);
                rtdLedger.TableName = "RTD_Order";
                dataTables.Add(rtdLedger);
                var pocLedger = ClassToDataTable.CreateDataTable(POCLedger);
                pocLedger.TableName = "POC_Order";
                dataTables.Add(pocLedger);
                var settleLedger = ClassToDataTable.CreateDataTable(SettleLedger);
                settleLedger.TableName = "Settle_Order";
                dataTables.Add(settleLedger);

                var fileName = "OrderNotReflectLadger" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";
                string filePath = ExcelSavePath + fileName;
                #endregion
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {

                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='OrderNotReflectLadger'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Order_Not_Reflect_Ladger Report";
                    string message = "Please find attach Daily Order_Not_Reflect_Ladger Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Daily Order_Not_Reflect_Ladger Report To and From empty");

                }
                return true;
            }
        }


        [Route("LiveCfr")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CFRReportDc>> LiveCfr(int? CityId, int? Subcat)
        {
            var LiveCfrList = new List<CFRReportDc>();
            using (var authContext = new AuthContext())
            {
                var CityIdParam = new SqlParameter
                {
                    ParameterName = "@CityId",
                    Value = CityId > 0 ? CityId : 0
                };
                var SubcatParam = new SqlParameter
                {
                    ParameterName = "@SubCatId",
                    Value = Subcat > 0 ? Subcat : 0
                };
                LiveCfrList = await authContext.Database.SqlQuery<CFRReportDc>("exec Generate_CfrArticleReport @CityId,@SubCatId", CityIdParam, SubcatParam).ToListAsync();
                if (LiveCfrList.Any())
                {
                    foreach (var item in LiveCfrList)
                    {
                        item.LimitValue = Math.Round(item.LimitValue, 2);
                        if (item.active == 0) { item.activeItem = "Inactive"; } else if (item.active == 1) { item.activeItem = "Active"; } else { item.activeItem = "Not Considered Active"; }
                    }
                }
            }
            return LiveCfrList;
        }

        [Route("GetCustomerNotifyItem")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic GetCustomerNotifyItem(int warehouseId, DateTime? startDate, DateTime? endDate, int skip, int take)
        {
            int Skiplist = (skip - 1) * take;
            if (endDate != null)
            {
                endDate = endDate.Value.AddDays(1).AddSeconds(-1);
            }
            List<CustomerItemNotifyMeDc> customerItemNotifyMeDcs = new List<CustomerItemNotifyMeDc>();
            MongoDbHelper<CustomerItemNotifyMe> mongoDbHelper = new MongoDbHelper<CustomerItemNotifyMe>();
            var searchPredicate = PredicateBuilder.New<CustomerItemNotifyMe>(x => x.ItemRequireCount > 0);
            if (warehouseId > 0)
            {
                searchPredicate = searchPredicate.And(x => x.WarehouseId == warehouseId);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                searchPredicate = searchPredicate.And(x => x.LastUpdated >= startDate && x.LastUpdated <= endDate);
            }

            var customerItemNotifyMe = mongoDbHelper.Select(searchPredicate, x => x.OrderByDescending(y => y.LastUpdated), Skiplist, take).ToList();
            int totcount = mongoDbHelper.Select(searchPredicate).Count;
            if (customerItemNotifyMe != null && customerItemNotifyMe.Any())
            {
                Suggest suggest = null;
                customerItemNotifyMeDcs = Mapper.Map(customerItemNotifyMe).ToANew<List<CustomerItemNotifyMeDc>>();
                var warehouseIds = customerItemNotifyMe.Select(x => x.WarehouseId).Distinct().ToList();
                var itemnumbers = customerItemNotifyMe.Select(x => x.ItemNumber).Distinct().ToList();
                MongoDbHelper<ElasticSearchQuery> mongoDbHelperElastic = new MongoDbHelper<ElasticSearchQuery>();
                var searchPredicate1 = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "ItemNotify");
                var searchQuery = mongoDbHelperElastic.Select(searchPredicate1, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                   .Replace("{#warehouseids#}", string.Join(",", warehouseIds))
                   .Replace("{#itemnumbers#}", string.Join("+", itemnumbers))
                   .Replace("{#itemactive#}", "")
                   .Replace("{#from#}", "0")
                   .Replace("{#size#}", "5000");
                List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                foreach (var item in customerItemNotifyMeDcs)
                {
                    var searchitem = elasticSearchItems.FirstOrDefault(x => x.itemnumber == item.ItemNumber && x.warehouseid == item.WarehouseId);
                    if (searchitem != null)
                    {
                        item.ItemName = searchitem.itemname;
                        item.BrandName = searchitem.subsubcategoryname;
                        item.WarehoueName = searchitem.warehousename;
                    }
                }
            }
            CustomerItemNotifyMePaging obj = new CustomerItemNotifyMePaging();
            obj.CustomerItemNotifyMeDcs = customerItemNotifyMeDcs.ToList();
            obj.total_count = totcount;
            return obj;
        }

        [Route("CustomerNotifyItemExport")]
        [HttpGet]
        [AllowAnonymous]
        public dynamic CustomerNotifyItemExport(int warehouseId, DateTime? startDate, DateTime? endDate)
        {
            if (endDate != null)
            {
                endDate = endDate.Value.AddDays(1).AddSeconds(-1);
            }
            List<CustomerItemNotifyMeDc> customerItemNotifyMeDcs = new List<CustomerItemNotifyMeDc>();
            MongoDbHelper<CustomerItemNotifyMe> mongoDbHelper = new MongoDbHelper<CustomerItemNotifyMe>();
            var searchPredicate = PredicateBuilder.New<CustomerItemNotifyMe>(x => x.ItemRequireCount > 0);
            if (warehouseId > 0)
            {
                searchPredicate = searchPredicate.And(x => x.WarehouseId == warehouseId);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                searchPredicate = searchPredicate.And(x => x.LastUpdated >= startDate && x.LastUpdated <= endDate);
            }

            var customerItemNotifyMe = mongoDbHelper.Select(searchPredicate, x => x.OrderByDescending(y => y.ItemRequireCount)).ToList();
            int totcount = mongoDbHelper.Select(searchPredicate).Count;
            if (customerItemNotifyMe != null && customerItemNotifyMe.Any())
            {
                Suggest suggest = null;
                customerItemNotifyMeDcs = Mapper.Map(customerItemNotifyMe).ToANew<List<CustomerItemNotifyMeDc>>();
                var warehouseIds = customerItemNotifyMe.Select(x => x.WarehouseId).Distinct().ToList();
                var itemnumbers = customerItemNotifyMe.Select(x => x.ItemNumber).Distinct().ToList();
                MongoDbHelper<ElasticSearchQuery> mongoDbHelperElastic = new MongoDbHelper<ElasticSearchQuery>();
                var searchPredicate1 = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "ItemNotify");
                var searchQuery = mongoDbHelperElastic.Select(searchPredicate1, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                   .Replace("{#warehouseids#}", string.Join(",", warehouseIds))
                   .Replace("{#itemnumbers#}", string.Join("+", itemnumbers))
                   .Replace("{#itemactive#}", "")
                   .Replace("{#from#}", "0")
                   .Replace("{#size#}", "5000");
                List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                foreach (var item in customerItemNotifyMeDcs)
                {
                    var searchitem = elasticSearchItems.FirstOrDefault(x => x.itemnumber == item.ItemNumber && x.warehouseid == item.WarehouseId);
                    if (searchitem != null)
                    {
                        item.ItemName = searchitem.itemname;
                        item.BrandName = searchitem.subsubcategoryname;
                        item.WarehoueName = searchitem.warehousename;
                    }
                }
            }
            CustomerItemNotifyMePaging obj = new CustomerItemNotifyMePaging();
            obj.CustomerItemNotifyMeDcs = customerItemNotifyMeDcs.OrderByDescending(x => x.ItemRequireCount).ToList();
            return obj;
        }

        [Route("GetBuyerDataReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> BuyerData()
        {
            int userid = 0; //string LevelName = "Level 0";
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            string zipfilename = ExcelSavePath + userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_ExportData.zip";

            using (var context = new AuthContext())
            {
                string path = "";
                var fileName = "";
                string zipCreatePath = "";
                #region Buyer Data

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 600;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[SPBuyerData]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var reportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<IRMasters>(reader).ToList();

                //var reportdata = await context.Database.SqlQuery<IRMasters>("Exec SPBuyerData").ToListAsync();
                string fileUrl = string.Empty;
                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_BuyerData.csv";

                DataTable dt = ListtoDataTableConverter.ToDataTable(reportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                //  zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion
                #region Supplier Summary

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 600;
                var cmd1 = context.Database.Connection.CreateCommand();
                cmd1.CommandText = "[dbo].[GetSupplierSummary]";
                cmd1.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader1 = cmd1.ExecuteReader();
                var SuppplierReportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<SupplierSummaryDTO>(reader1).ToList();
                //var SuppplierReportdata = await context.Database.SqlQuery<SupplierSummaryDTO>("Exec GetSupplierSummary").ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_SupplierSummary.csv";

                DataTable dt1 = ListtoDataTableConverter.ToDataTable(SuppplierReportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt1.WriteToCsvFile(path);

                //zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion
                #region GR-IR Difference
                DateTime startDates = new DateTime(2023, 4, 1);
                var date = DateTime.Now;

                var StartDate = new SqlParameter
                {
                    ParameterName = "startDate",
                    Value = startDates
                };
                var EndDate = new SqlParameter
                {
                    ParameterName = "endDate",
                    Value = date.Date.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59)
                };

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 600;
                var cmd2 = context.Database.Connection.CreateCommand();
                cmd2.CommandText = "[dbo].GrButNoIR";
                cmd2.Parameters.Add(StartDate);
                cmd2.Parameters.Add(EndDate);
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader2 = cmd2.ExecuteReader();
                List<GrButNoIrReportDc> GRIRDifferenceReportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<GrButNoIrReportDc>(reader2).ToList();


                //var GRIRDifferenceReportdata = await context.Database.SqlQuery<GrButNoIrReportDc>("Exec GrButNoIR @startdate,@enddate", StartDate, EndDate).ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_GRIRDifferenceReport.csv";

                DataTable dt2 = ListtoDataTableConverter.ToDataTable(GRIRDifferenceReportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt2.WriteToCsvFile(path);

                //   zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion

                //fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                //                                         , HttpContext.Current.Request.Url.DnsSafeHost
                //                                         , HttpContext.Current.Request.Url.Port
                //                                         , string.Format("ExcelGeneratePath/{0}", zipfilename));

                if (!string.IsNullOrEmpty(zipfilename))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='BuyerData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Supplier Summary Report ";
                    string message = "Please find attach Daily Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, zipfilename);
                    else
                        logger.Error("Supplier Summary Report To and From empty");
                }
            }
            return true;
        }

        [Route("GetWalletPointHistory")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> WalletPointHistory()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 6000;
                List<WalletPointHistory> reportdata = await context.Database.SqlQuery<WalletPointHistory>("Exec SPWalletPointHistory").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "WalletPointHistory";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "WalletPointHistory" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='WalletPointHistory'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Wallet Point History Report";
                    string message = "Please find attach Wallet Point History Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Wallet Point History Report To and From empty");
                }
            }
            return true;
        }



        [Route("DailyCurrentNetStock")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> DailyCurrentNetStock()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 6000;
                List<CurrentNetStockDC> reportdata = await context.Database.SqlQuery<CurrentNetStockDC>("Exec DailyCurrentNetStock").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "CurrentNetStock";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "CurrentNetStock" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='CurrentNetStock'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " CurrentNetStock Report";
                    string message = "Please find attach CurrentNetStock Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("CurrentNetStock Report To and From empty");
                }
            }
            return true;
        }
        [Route("GetFreebiesData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> FreebiesData()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 6000;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].SPFreebiesData";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<FreebiesData> reportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<FreebiesData>(reader).ToList();

                //List<FreebiesData> reportdata = await context.Database.SqlQuery<FreebiesData>("Exec SPFreebiesData").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "FreebiesData";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "FreebiesData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='FreebiesData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Freebies Data Report";
                    string message = "Please find attach Freebies Data Report";
                    //await GetBillDiscountFreeItemData();
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(AppConstants.MasterEmail, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Freebies Data Report To and From empty");
                }
            }

            await GetBillDiscountFreeItemData();
            return true;
        }



        [Route("GetBillDiscountFreeItemData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetBillDiscountFreeItemData()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 600;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].SPBillDiscountFreeItemData";
                //cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                //cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                //cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                //cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<GetBillDiscountFreeItemData> reportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<GetBillDiscountFreeItemData>(reader).ToList();
                //List<GetBillDiscountFreeItemData> reportdata = await context.Database.SqlQuery<GetBillDiscountFreeItemData>("Exec SPBillDiscountFreeItemData").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "BillDiscountFreeItemData";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "BillDiscountFreeItemData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='FreebiesData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " BillDiscountFreeItem Data Report";
                    string message = "Please find attach BillDiscountFreeItem Data Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(AppConstants.MasterEmail, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("BillDiscountFreeItem Data Report To and From empty");
                }
            }
            return true;
        }
        [Route("GetDamageMovement")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> DailyDamageMovementReport()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/DamageStock");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                List<DamageMovement> reportdata = await context.Database.SqlQuery<DamageMovement>("Exec SPDamageMovement").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "DamageMovement";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "DamageMovement" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='DailyDamageMovement'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Damage Movement Report";
                    string message = "Please find attach Daily Damage Movement Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Daily Damage Movement Report To and From empty");
                }
            }
            return true;
        }


        [Route("GetNonSellableMovement")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> DailyNonSellableMovementReport()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/NonSellableStock");
            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                List<NonSellableMovement> reportdata = await context.Database.SqlQuery<NonSellableMovement>("Exec SPNonSellableMovement").ToListAsync();
                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "NonSellableMovement";
                dataTables.Add(dt);
                string filePath = ExcelSavePath + "NonSellableMovement" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='DailyNonSellableMovement'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily NonSellable Movement Report";
                    string message = "Please find attach Daily NonSellable Movement Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Daily NonSellable Movement Report To and From empty");
                }

            }

            return true;
        }


        [Route("GetSalesRegisterReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetSalesRegisterReport()
        {
            int userid = 0; //string LevelName = "Level 0";
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            string zipfilename = ExcelSavePath + userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_SalesRegisterReport.zip";
            var NewExcelName = zipfilename.Replace(ExcelSavePath, "");


            using (var context = new AuthContext())
            {
                string path = "";
                var fileName = "";
                string zipCreatePath = "";
                #region GetSalesRegister

                //************************************************************
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd2 = context.Database.Connection.CreateCommand();
                cmd2.CommandText = "[dbo].[GetSalesRegister]";
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                cmd2.CommandTimeout = 6000;
                // Run the sproc
                var reader2 = cmd2.ExecuteReader();
                List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context).ObjectContext.Translate<GetSalesRegisterDc>(reader2).ToList();

                //************************************************************
                //var reportdata = await context.Database.SqlQuery<GetSalesRegisterDc>("Exec GetSalesRegister").ToListAsync();
                string fileUrl = string.Empty;
                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_GetSalesRegister.csv";

                DataTable dt = ListtoDataTableConverter.ToDataTable(reportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);
                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion
                #region GetReturnSalesRegister

                //*******************************************************************
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd1 = context.Database.Connection.CreateCommand();
                cmd1.CommandText = "[dbo].[GetReturnSalesRegister]";
                cmd1.CommandType = System.Data.CommandType.StoredProcedure;
                cmd1.CommandTimeout = 6000;
                // Run the sproc
                var reader1 = cmd1.ExecuteReader();
                List<GetSalesRegisterDc> SuppplierReportdata = ((IObjectContextAdapter)context).ObjectContext.Translate<GetSalesRegisterDc>(reader1).ToList();

                //********************************************************************
                // var SuppplierReportdata = await context.Database.SqlQuery<GetSalesRegisterDc>("Exec GetReturnSalesRegister").ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_GetReturnSalesRegister.csv";

                DataTable dt1 = ListtoDataTableConverter.ToDataTable(SuppplierReportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt1.WriteToCsvFile(path);

                //zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);
                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion
                if (!string.IsNullOrEmpty(zipfilename))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='SalesRegisterReport'", connection))
                        {
                            command.CommandTimeout = 1200;
                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Sales Register Report ";
                    string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , "/ExcelGeneratePath/" + NewExcelName);
                    string message = "Please find below link for Daily Sales Register Report :" + FileUrl;
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                    {
                        EmailHelper.SendMail(From, To, Bcc, subject, message, "");
                        bool res = await GetDailyDeliveredReport();
                        return res;
                    }
                    else
                        logger.Error("Sales Register Report To and From empty");


                }
            }
            return true;
        }

        [Route("DirectUdharReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> DirectUdharReport(int month, int year)
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            SqlParameter mn = new SqlParameter("month", month);
            SqlParameter yr = new SqlParameter("year", year);
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 6000;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[MonthEnd].[DirectUdharReport]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(mn);
                cmd.Parameters.Add(yr);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<DirectUdharDC> reportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DirectUdharDC>(reader).ToList();

                //List<FreebiesData> reportdata = await context.Database.SqlQuery<FreebiesData>("Exec SPFreebiesData").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "DirectUdharReport";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "DirectUdharReport" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='DirectUdharReport'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + "Direct Udhar Report";
                    string message = "Please find attach Direct Udhar Report";
                    //await GetBillDiscountFreeItemData();
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(AppConstants.MasterEmail, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("DirectUdharReport To and From empty");
                }
            }


            return true;
        }

        [Route("CurrentNetStock")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> CurrentNetStock()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                //SqlParameter mn = new SqlParameter("month", month);
                //SqlParameter yr = new SqlParameter("year", year);
                context.Database.CommandTimeout = 6000;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[MonthEnd].[CurrentNetStockWithLiveQtyNew]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //cmd.Parameters.Add(mn);
                //cmd.Parameters.Add(yr);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<CurrentNetStockDC> reportdata = ((IObjectContextAdapter)context).ObjectContext.Translate<CurrentNetStockDC>(reader).ToList();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "CurrentNetStock";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "CurrentNetStock" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='CurrentNetStock'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + "Current Net Stock Data";
                    string message = "Please find attach Current Net Stock Data";
                    //await GetBillDiscountFreeItemData();
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(AppConstants.MasterEmail, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("CurrentNetStock To and From empty");
                }
            }


            return true;
        }

        [Route("ClearanceStockData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> ClearanceStockData(int month, int year)
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            SqlParameter mn = new SqlParameter("month", month);
            SqlParameter yr = new SqlParameter("year", year);
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 6000;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[MonthEnd].[ClearanceStockData]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(mn);
                cmd.Parameters.Add(yr);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<ClearanceStockDataDC> reportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<ClearanceStockDataDC>(reader).ToList();

                //List<FreebiesData> reportdata = await context.Database.SqlQuery<FreebiesData>("Exec SPFreebiesData").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "ClearanceStockData";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "ClearanceStockData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='ClearanceStockData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + "ClearanceStockData Report";
                    string message = "Please find attach ClearanceStockData Report";
                    //await GetBillDiscountFreeItemData();
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(AppConstants.MasterEmail, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("ClearanceStockData To and From empty");
                }
            }


            return true;
        }
        [Route("NonSaleableStock")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> NonSaleableStock(int month, int year)
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            SqlParameter mn = new SqlParameter("month", month);
            SqlParameter yr = new SqlParameter("year", year);
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 6000;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[MonthEnd].[NonSaleableStock]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(mn);
                cmd.Parameters.Add(yr);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<NonSaleableStockDC> reportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<NonSaleableStockDC>(reader).ToList();

                //List<FreebiesData> reportdata = await context.Database.SqlQuery<FreebiesData>("Exec SPFreebiesData").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "NonSaleableStock";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "NonSaleableStock" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='NonSaleableStock'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + "NonSaleableStock Report";
                    string message = "Please find attach NonSaleableStock Report";
                    //await GetBillDiscountFreeItemData();
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(AppConstants.MasterEmail, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("NonSaleableStock To and From empty");
                }
            }


            return true;
        }

        [Route("RedispatchedWalletPointData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> RedispatchedWalletPointData(int month, int year)
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                SqlParameter mn = new SqlParameter("month", month);
                SqlParameter yr = new SqlParameter("year", year);
                context.Database.CommandTimeout = 6000;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[MonthEnd].[RedispatchedWalletPointData]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(mn);
                cmd.Parameters.Add(yr);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<RedispatchedWalletPointDataDC> reportdata = ((IObjectContextAdapter)context).ObjectContext.Translate<RedispatchedWalletPointDataDC>(reader).ToList();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "RedispatchedWalletPointData";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "RedispatchedWalletPointData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='RedispatchedWalletPointData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + "RedispatchedWalletPointData";
                    string message = "Please find attach RedispatchedWalletPointData";
                    //await GetBillDiscountFreeItemData();
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(AppConstants.MasterEmail, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("RedispatchedWalletPointData To and From empty");
                }
            }


            return true;
        }
        [Route("InsertCustomerCurrentFYSales")]
        [HttpGet]
        [AllowAnonymous]
        public bool InsertCustomerCurrentFYSales()
        {
            using (var context = new AuthContext())
            {
                DateTime sDate = new DateTime(2021, 4, 1);

                var customersSale = context.OrderDispatchedMasters.Where(x => x.CreatedDate >= sDate && x.CreatedDate <= DateTime.Now && x.Status != "Post Order Canceled" && x.Status != "Delivery Canceled").GroupBy(x => x.CustomerId)
                    .Select(x => new { CustomerId = x.Key, TotalPurchase = x.Sum(y => y.GrossAmount) }).ToList();
                List<TCSCustomer> TCSCustomers = customersSale.Select(x => new TCSCustomer
                {
                    CustomerId = x.CustomerId,
                    FinancialYear = "2022",
                    LastUpdatedDate = DateTime.Now,
                    TotalPurchase = x.TotalPurchase
                }).ToList();
                MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                var result = mHelper.InsertMany(TCSCustomers);

            }

            return true;
        }


        [Route("GetAdjustmentPoData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetAdjustmentPoData()
        {

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                context.Database.CommandTimeout = 6000;
                var cmd3 = context.Database.Connection.CreateCommand();
                cmd3.CommandText = "[dbo].[GetAdjustmentPoData]";
                cmd3.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader3 = cmd3.ExecuteReader();
                List<AdjustmentPoDataDC> getAdjustmentPoData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<AdjustmentPoDataDC>(reader3).ToList();

                //List<AdjustmentPoDataDC> getAdjustmentPoData = await context.Database.SqlQuery<AdjustmentPoDataDC>("Exec GetAdjustmentPoData").ToListAsync();
                if (getAdjustmentPoData.Any() && getAdjustmentPoData != null)
                {
                    var dataTables = new List<DataTable>();
                    DataTable dt = ClassToDataTable.CreateDataTable(getAdjustmentPoData);
                    dt.TableName = "AdjustmentPoData";
                    dataTables.Add(dt);

                    string filePath = ExcelSavePath + "AdjustmentPoData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            if (connection.State != ConnectionState.Open)
                                connection.Open();
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='AdjustmentPoData'", connection))
                            {
                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(emaildatatable);
                                da.Dispose();
                                connection.Close();
                            }
                        }
                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Adjustment Po Data Report";
                        string message = "Please find attach Daily Adjustment Po Data Report";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        else
                            logger.Error("Daily Adjustment Po Data Report To and From empty");
                    }
                }
            }
            return true;
        }


        [Route("InactiveInventoryNotificationToBuyerData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> InactiveInventoryNotificationToBuyerData()
        {

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            string zipfilename = ExcelSavePath + "_InactiveInventoryNotification_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_ExportData.zip";

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            DataTable dtmain = new DataTable();
            using (var context = new AuthContext())
            {
                var WarehouseIds = context.Warehouses.Where(x => x.active == true && x.Deleted == false && x.IsKPP == false).Select(y => y.WarehouseId).ToList();
                foreach (var WHid in WarehouseIds)
                {
                    var WarehouseID = new SqlParameter("@WareHouseId", WHid);

                    List<InactiveInventoryDC> data = await context.Database.SqlQuery<InactiveInventoryDC>("Exec InactiveInventoryNotificationToBuyer @WareHouseId ", WarehouseID).ToListAsync();
                    if (data.Any() && data != null)
                    {

                        DataTable dt = ClassToDataTable.CreateDataTable(data);
                        dtmain.Merge(dt);
                        //dt.TableName = "InactiveInventoryNotificationToBuyer";
                        // dataTables.Add(dt);
                    }
                }

                if (dtmain.Rows.Count > 0)
                {

                    string filePath = ExcelSavePath + "InactiveInventoryNotification" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";

                    string fileName = "InactiveInventoryNotification" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + "_BuyerData.csv";


                    string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                    dtmain.WriteToCsvFile(path);


                    using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(path, fileName);
                    }
                    File.Delete(path);


                    if (!string.IsNullOrEmpty(zipfilename))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                        string To = "", From = "", Bcc = "";
                        DataTable emaildatatable = new DataTable();
                        using (var connection = new SqlConnection(connectionString))
                        {
                            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='InactiveInventoryNotification'", connection))
                            {

                                if (connection.State != ConnectionState.Open)
                                    connection.Open();

                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(emaildatatable);
                                da.Dispose();
                                connection.Close();
                            }
                        }
                        if (emaildatatable.Rows.Count > 0)
                        {
                            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                        }
                        string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily InactiveInventoryNotificationToBuyer Data Report";
                        string message = "Please find attach Daily InactiveInventoryNotificationToBuyer Data Report";
                        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                            EmailHelper.SendMail(From, To, Bcc, subject, message, zipfilename);
                        else
                            logger.Error("Daily InactiveInventoryNotificationToBuyerData Report To and From empty");
                    }
                }
            }
            return true;
        }

        [Route("GSTReport")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<string> GetGSTReport(GSTDc gSTDc)
        {
            int userid = 0;
            string path = "";
            string fileName = "";
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
            string zipfilename = ExcelSavePath + "_GSTReportPurpose_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_ExportData.zip";
            string fileUrl = string.Empty;
            fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_GetSalesRegister.csv";


            string returnPath = "";
            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var db = new AuthContext())
            {
                DataTable dt = new DataTable();
                var from = new SqlParameter("@startDate", gSTDc.From);

                var tO = new SqlParameter("@endDate", gSTDc.TO);
                var reporttype = new SqlParameter("@ReportType", gSTDc.Reporttype);
                if (gSTDc.Reporttype == 1 || gSTDc.Reporttype == 2)//sales and sales Return
                {
                    List<SalesRegisterReportDC> reportdata = await db.Database.SqlQuery<SalesRegisterReportDC>("Exec GSTPurposeReport @ReportType,@startDate,@endDate", reporttype, from, tO).ToListAsync();

                    dt = ListtoDataTableConverter.ToDataTable(reportdata);
                }
                else if (gSTDc.Reporttype == 3)//TDS advanced 
                {
                    List<gstTDS> reportdata = await db.Database.SqlQuery<gstTDS>("Exec GSTPurposeReport @ReportType,@startDate,@endDate", reporttype, from, tO).ToListAsync();

                    dt = ListtoDataTableConverter.ToDataTable(reportdata);
                }
                else if (gSTDc.Reporttype == 4) //TDS bill to bill
                {
                    List<TDSBILLtoBill> reportdata = await db.Database.SqlQuery<TDSBILLtoBill>("Exec GSTPurposeReport @ReportType,@startDate,@endDate", reporttype, from, tO).ToListAsync();

                    dt = ListtoDataTableConverter.ToDataTable(reportdata);
                }
                else if (gSTDc.Reporttype == 5)//TCS
                {
                    List<GstTCS> reportdata = await db.Database.SqlQuery<GstTCS>("Exec GSTPurposeReport @ReportType,@startDate,@endDate", reporttype, from, tO).ToListAsync();

                    dt = ListtoDataTableConverter.ToDataTable(reportdata);
                }
                else if (gSTDc.Reporttype == 6)//Customer's whose sales have crossed 50 Lakhs
                {
                    List<gstCustomer50> reportdata = await db.Database.SqlQuery<gstCustomer50>("Exec GSTPurposeReport @ReportType,@startDate,@endDate", reporttype, from, tO).ToListAsync();

                    dt = ListtoDataTableConverter.ToDataTable(reportdata);
                }
                else if (gSTDc.Reporttype == 7)
                {
                    List<gstvender50> reportdata = await db.Database.SqlQuery<gstvender50>("Exec GSTPurposeReport @ReportType,@startDate,@endDate", reporttype, from, tO).ToListAsync();

                    dt = ListtoDataTableConverter.ToDataTable(reportdata);
                }
                if (dt.Rows.Count == 0)
                {
                    return "";
                }
                else
                {
                    //string fileUrl = string.Empty;
                    fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_GST.csv";

                    path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                    dt.WriteToCsvFile(path);
                    returnPath = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                               , HttpContext.Current.Request.Url.DnsSafeHost
                                                               , HttpContext.Current.Request.Url.Port
                                                               , string.Format("ExcelGeneratePath/{0}", fileName));

                }
                return returnPath;

            }

        }



        [Route("GetYesterdayDemand")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetYesterdayDemand()
        {
            List<KeyValuePair<string, string>> filePaths = new List<KeyValuePair<string, string>>();

            List<DataTable> datas = new List<DataTable>();
            List<YesterdayDemandDC> demandData = new List<YesterdayDemandDC>();
            EmailRecipients emailRecipients = new EmailRecipients();

            using (var db = new AuthContext())
            {
                db.Database.CommandTimeout = 1200;
                demandData = await db.Database.SqlQuery<YesterdayDemandDC>("Exec GetYesterdayDemand").ToListAsync();
                emailRecipients = await db.Database.SqlQuery<EmailRecipients>("Select * from EmailRecipients where EmailType='SendDemand' and IsActive=1").FirstOrDefaultAsync();
            }

            if (!Directory.Exists(Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/"), "Demand")))
                Directory.CreateDirectory(Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/"), "Demand"));

            if (demandData != null && demandData.Any())
            {
                foreach (var item in demandData.GroupBy(x => new { x.BuyerId, x.BuyerName, x.BuyerEmail }))
                {
                    if (item.Any(x => x.YesterdayDemand + x.OldDemand > 0))
                    {
                        var demandItems = item.Where(x => x.YesterdayDemand + x.OldDemand > 0).ToList();

                        var dt = new DataTable();

                        dt.Columns.Add("Warehouse");
                        dt.Columns.Add("ItemMultiMrpId", typeof(int));
                        dt.Columns.Add("Item Name");
                        dt.Columns.Add("Item Number");
                        dt.Columns.Add("Yesterday Demand", typeof(int));
                        dt.Columns.Add("Old Demand", typeof(int));
                        dt.Columns.Add("Total Demand", typeof(int));
                        dt.Columns.Add("Closing Stock", typeof(double));
                        dt.Columns.Add("Open Po", typeof(int));
                        dt.Columns.Add("Delivery Cancel", typeof(int));
                        dt.Columns.Add("Net Demand", typeof(int));
                        dt.Columns.Add("2 Warehouse");
                        dt.Columns.Add("2 Warehouse NetStock", typeof(int));

                        var maxOtherWarehouses = demandItems.GroupBy(x => new { x.cityid })
                                                 .Select(x => new
                                                 {
                                                     CityId = x.Key,
                                                     WhCount = x.Select(s => s.warehouseid).Distinct().Count()
                                                 })
                                                 .Max(x => x.WhCount);

                        if (maxOtherWarehouses > 2)
                        {
                            for (int i = 1; i < maxOtherWarehouses; i++)
                            {
                                dt.Columns.Add(i + 2 + " Warehouse");
                                dt.Columns.Add(i + 2 + "  Warehouse  NetStock", typeof(int));
                            }
                        }


                        dt.Columns.Add("New Demand", typeof(int));

                        foreach (var demand in demandItems)
                        {
                            int otherWhnetStock = 0;
                            DataRow dr = dt.NewRow();

                            if (demandData.Any(x => x.itemmultimrpid == demand.itemmultimrpid && x.cityid == demand.cityid && x.warehouseid != demand.warehouseid))
                            {
                                var otherwh = 2;
                                foreach (var otherDemand in demandData.Where(x => x.itemmultimrpid == demand.itemmultimrpid && x.cityid == demand.cityid && x.warehouseid != demand.warehouseid))
                                {
                                    dr[otherwh + " Warehouse"] = otherDemand.warehousename;
                                    dr[otherwh + " Warehouse NetStock"] = otherDemand.CurrentStock - otherDemand.Demand + otherDemand.openpoqty + otherDemand.DelCancel;

                                    otherWhnetStock += (otherDemand.CurrentStock - otherDemand.Demand + otherDemand.openpoqty + otherDemand.DelCancel) > 0 ? otherDemand.CurrentStock - otherDemand.Demand + otherDemand.openpoqty + otherDemand.DelCancel : 0;
                                }
                            }

                            //if ((demand.CurrentStock - demand.Demand + demand.openpoqty + demand.DelCancel + otherWhnetStock) < 0)
                            //{
                            dr["Warehouse"] = demand.warehousename;
                            dr["ItemMultiMrpId"] = demand.itemmultimrpid;
                            dr["Item Name"] = demand.ItemName;
                            dr["Item Number"] = demand.itemnumber;
                            dr["Yesterday Demand"] = demand.YesterdayDemand;
                            dr["Old Demand"] = demand.OldDemand;
                            dr["Total Demand"] = demand.Demand;
                            dr["Closing Stock"] = demand.CurrentStock;
                            dr["Open Po"] = demand.openpoqty;
                            dr["Delivery Cancel"] = demand.DelCancel;

                            dr["Net Demand"] = demand.CurrentStock - demand.Demand + demand.openpoqty + demand.DelCancel;
                            dr["New Demand"] = (demand.CurrentStock - demand.Demand + demand.openpoqty + demand.DelCancel + otherWhnetStock);

                            dt.Rows.Add(dr);
                            //}
                        }

                        string fileName = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/"), "Demand", item.Key.BuyerName + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx");

                        ExcelGenerator.DataTable_To_Excel(dt, item.Key.BuyerName, fileName);

                        filePaths.Add(new KeyValuePair<string, string>(item.Key.BuyerEmail, fileName));
                    }
                }

            }

            string fromEmail = ConfigurationManager.AppSettings["MasterEmail"];
            string subject = " Daily Demand " + DateTime.Now.ToString("dd-MMM-yyyy");
            string message = "PFA the Daily Demand";

            if (filePaths.Any())
            {
                foreach (var item in filePaths)
                {
                    if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(fromEmail))
                        EmailHelper.SendMail(fromEmail, item.Key, "", subject, message, item.Value);
                }

                if (!string.IsNullOrEmpty(fromEmail) && emailRecipients != null && !string.IsNullOrEmpty(emailRecipients.To))
                    EmailHelper.SendMail(fromEmail, emailRecipients.To, !string.IsNullOrEmpty(emailRecipients.Bcc) ? emailRecipients.Bcc : "", subject, message, string.Join(",", filePaths.Select(x => x.Value)));
            }


            return true;
        }


        [Route("SendDfrPerformance")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> SendDfrPerformance()
        {
            List<DFRPercentage> dfrData = new List<DFRPercentage>();
            List<KeyValuePair<string, string>> filePaths = new List<KeyValuePair<string, string>>();

            EmailRecipients emailRecipients = new EmailRecipients();

            using (var db = new AuthContext())
            {
                db.Database.CommandTimeout = 1200;
                dfrData = await db.Database.SqlQuery<DFRPercentage>("Exec CalculateDFRPercent").ToListAsync();
                emailRecipients = await db.Database.SqlQuery<EmailRecipients>("Select * from EmailRecipients where EmailType='SendDFR' and IsActive=1").FirstOrDefaultAsync();
            }

            if (!Directory.Exists(Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/"), "Demand")))
                Directory.CreateDirectory(Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/"), "Demand"));

            if (dfrData != null && dfrData.Any())
            {
                foreach (var item in dfrData.GroupBy(x => new { x.BuyerName, x.BuyerEmail }))
                {
                    //if (item.Any(x => x.DFRPercent < 100))
                    //{
                    var demandItems = item.ToList();
                    var dt = new DataTable();

                    dt.Columns.Add("itemmultimrpid", typeof(int));
                    dt.Columns.Add("Warehouse");
                    dt.Columns.Add("ItemNumber");
                    dt.Columns.Add("ItemName");
                    dt.Columns.Add("BuyerName");
                    dt.Columns.Add("Yesterday Demand", typeof(int));
                    dt.Columns.Add("Old Demand", typeof(int));
                    dt.Columns.Add("Demand", typeof(int));
                    dt.Columns.Add("CurrentStock", typeof(int));
                    dt.Columns.Add("NetDemand", typeof(int));
                    dt.Columns.Add("openpoqty", typeof(int));
                    dt.Columns.Add("DelCancel", typeof(int));
                    dt.Columns.Add("OtherHubsStock", typeof(int));
                    dt.Columns.Add("OtherHubsDemand", typeof(int));
                    dt.Columns.Add("NewDemand", typeof(int));
                    dt.Columns.Add("TotalGrQty", typeof(int));
                    dt.Columns.Add("TotalInternalTransfer", typeof(int));
                    dt.Columns.Add("ClosingStock", typeof(int));
                    dt.Columns.Add("RtpCount", typeof(int));
                    dt.Columns.Add("DFRPercent", typeof(double));
                    dt.Columns.Add("DfrOnInventory", typeof(double));
                    dt.Columns.Add("DemandDate");
                    dt.Columns.Add("CreatedDate");
                    dt.Columns.Add("UpdatedDate");

                    foreach (var demand in demandItems)
                    {
                        DataRow dr = dt.NewRow();

                        dr["itemmultimrpid"] = demand.itemmultimrpid;
                        dr["Warehouse"] = demand.warehousename;
                        dr["ItemNumber"] = demand.itemnumber;
                        dr["ItemName"] = demand.ItemName;
                        dr["Yesterday Demand"] = demand.YesterdayDemand;
                        dr["Old Demand"] = demand.OldDemand;
                        dr["Demand"] = demand.Demand;
                        dr["CurrentStock"] = demand.CurrentStock;
                        dr["NetDemand"] = demand.NetDemand;
                        dr["BuyerName"] = demand.BuyerName;
                        dr["openpoqty"] = demand.openpoqty;
                        dr["DelCancel"] = demand.DelCancel;
                        dr["OtherHubsStock"] = demand.OtherHubsStock;
                        dr["OtherHubsDemand"] = demand.OtherHubsDemand;
                        dr["NewDemand"] = demand.NewDemand;
                        dr["DemandDate"] = demand.DemandDate;
                        dr["CreatedDate"] = demand.CreatedDate;
                        dr["TotalGrQty"] = demand.TotalGrQty;
                        dr["TotalInternalTransfer"] = demand.TotalInternalTransfer;
                        dr["ClosingStock"] = demand.ClosingStock;
                        dr["DFRPercent"] = demand.DFRPercent;
                        dr["DfrOnInventory"] = demand.DfrOnInventory;
                        dr["UpdatedDate"] = demand.UpdatedDate;
                        dr["RtpCount"] = demand.RTPCount;


                        dt.Rows.Add(dr);
                    }

                    string fileName = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/"), "Demand", "DFR_" + item.Key.BuyerName + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx");

                    ExcelGenerator.DataTable_To_Excel(dt, item.Key.BuyerName, fileName);

                    filePaths.Add(new KeyValuePair<string, string>(item.Key.BuyerEmail, fileName));
                    //}
                }

            }

            if (filePaths.Any())
            {
                string fromEmail = ConfigurationManager.AppSettings["MasterEmail"];
                string subject = " Daily DFR Percent " + DateTime.Now.ToString("dd-MMM-yyyy");
                string message = "PFA the Daily DFR data";
                string environment = ConfigurationManager.AppSettings["Environment"];

                foreach (var item in filePaths)
                {
                    if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(fromEmail))
                        EmailHelper.SendMail(fromEmail, item.Key, "", subject, message, item.Value);
                }

                if (!string.IsNullOrEmpty(fromEmail) && emailRecipients != null && !string.IsNullOrEmpty(emailRecipients.To))
                    EmailHelper.SendMail(fromEmail, emailRecipients.To, !string.IsNullOrEmpty(emailRecipients.Bcc) ? emailRecipients.Bcc : "", subject, message, string.Join(",", filePaths.Select(x => x.Value)));

            }
            return true;
        }

        [Route("GetOverallstatus")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetOverallstatus()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/Overallstatus");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {


                List<OverallOrderMasterStatusReportDC> reportdata = await context.Database.SqlQuery<OverallOrderMasterStatusReportDC>("Exec GetOverallOrderMasterStatusData").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "OverallOrderMasterStatusData";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "OverallOrderMasterStatusData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='OverallOrder'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Overall Order Master Status Report";
                    string message = "Please find attach  Daily Overall Order Master Status Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error(" Daily Overall Order Master Status Report To and From empty");
                }
            }
            return true;
        }


        #region Store 2 report item sale target
        [Route("CustomerItemTargetReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> CustomerItemTargetReport()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/CustomerItemTargetReport/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                List<CustomerItemTargetReportDc> reportdata = await context.Database.SqlQuery<CustomerItemTargetReportDc>("Exec [Seller].[CustomerItemTargetReport]").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "CustomerItemTargetReport";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "CustomerItemTargetReport" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='CustomerItemTargetReport'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Customer Item Target Report";
                    string message = "Please find attach Customer Item Target Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error(" Daily Customer Item Target Report To and From empty");
                }
            }
            return true;
        }
        [Route("SalesManItemTargetReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> SalesManItemTargetReport()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/SalesManItemTargetReport/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[Seller].[SalesManItemTargetReport]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<SalesManItemTargetReportDc> reportdata = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<SalesManItemTargetReportDc>(reader).ToList();
                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "SalesManItemTargetReport";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "SalesManItemTargetReport" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='SalesManItemTargetReport'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " SalesMan Item Target Report";
                    string message = "Please find attach  SalesMan Item Target Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error(" Daily SalesMan Item Target Report To and From empty");
                }
            }
            return true;
        }
        [Route("BrandCompanyLevelTargetReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> BrandCompanyLevelTargetReport()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/BrandCompanyLevelTargetReport/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[Seller].[BrandCompanyLevelTargetReport]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<BrandCompanyLevelTargetReportDc> reportdata = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<BrandCompanyLevelTargetReportDc>(reader).ToList();
                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "BrandCompanyLevelTargetReport";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "BrandCompanyLevelTargetReport" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='BrandCompanyLevelTargetReport'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Brand Company Level Target Report";
                    string message = "Please find attach  Brand Company Level Target Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error(" Daily Brand Company Level Target Report To and From empty");
                }
            }
            return true;
        }

        [Route("GenerateItemSalesInventoryData")]

        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GenerateItemSalesInventoryData()
        {
            var reportManager = new Managers.ReportManager();
            return await reportManager.GenerateItemSalesInventoryData();
        }

        [Route("GetDFRDashBoardData")]
        [HttpPost]
        public async Task<DFRDashboardDc> GetDFRDashBoardData(DFRDashboardRequestDc dfrDashboardRequestDc)
        {
            DFRDashboardDc dfrDashboardDc = new DFRDashboardDc();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                SqlParameter parStartDate = new SqlParameter("startDate", dfrDashboardRequestDc.StartDate);
                SqlParameter parEndDate = new SqlParameter("endDate", dfrDashboardRequestDc.EndDate);

                DataTable dtBuyerId = new DataTable();
                dtBuyerId.Columns.Add("IntValue");
                foreach (var item in dfrDashboardRequestDc.buyerIds)
                {
                    var dr = dtBuyerId.NewRow();
                    dr["IntValue"] = item;
                    dtBuyerId.Rows.Add(dr);
                }
                var parbuyerId = new SqlParameter("buyerIds", dtBuyerId);
                parbuyerId.SqlDbType = SqlDbType.Structured;
                parbuyerId.TypeName = "dbo.IntValues";

                DataTable dtwarehouseId = new DataTable();
                dtwarehouseId.Columns.Add("IntValue");
                foreach (var item in dfrDashboardRequestDc.warehouseIds)
                {
                    var dr = dtwarehouseId.NewRow();
                    dr["IntValue"] = item;
                    dtwarehouseId.Rows.Add(dr);
                }
                var parwarehouseId = new SqlParameter("warehouseIds", dtwarehouseId);
                parwarehouseId.SqlDbType = SqlDbType.Structured;
                parwarehouseId.TypeName = "dbo.IntValues";


                var cmd = context.Database.Connection.CreateCommand();
                cmd.Parameters.Add(parStartDate);
                cmd.Parameters.Add(parEndDate);
                cmd.Parameters.Add(parbuyerId);
                cmd.Parameters.Add(parwarehouseId);

                cmd.CommandText = "[dbo].[GetDFRDashBoardData]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<DFRDashboardDataDc> DFRDashboardDataDcs = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<DFRDashboardDataDc>(reader).ToList();
                dfrDashboardDc.DFRDashboardDataDcs = DFRDashboardDataDcs;
                if (DFRDashboardDataDcs != null && DFRDashboardDataDcs.Any())
                {
                    if (dfrDashboardRequestDc.StartDate == dfrDashboardRequestDc.EndDate)
                    {
                        List<DFRDashboardGraphDc> DFRDashboardGraphDcs = DFRDashboardDataDcs.GroupBy(x => x.BuyerName).Select(x =>
                                                                           new DFRDashboardGraphDc { BuyerName = x.Key, DFRPercent = Convert.ToInt32(x.Average(y => y.DFRPercent)) }).ToList();
                        dfrDashboardDc.DFRDashboardGraphDcs = DFRDashboardGraphDcs;
                        dfrDashboardDc.TotalRed = DFRDashboardDataDcs.Where(x => x.status == "Red").Count() * 100 / DFRDashboardDataDcs.Count();
                        dfrDashboardDc.TotalGreen = DFRDashboardDataDcs.Where(x => x.status == "Green").Count() * 100 / DFRDashboardDataDcs.Count();
                    }
                    else
                    {
                        List<DFRDashboardGraphDc> DFRDashboardGraphDcs = DFRDashboardDataDcs.GroupBy(x => new { x.BuyerName, x.DemandDate, x.warehousename }).Select(x =>
                                                                                 new DFRDashboardGraphDc
                                                                                 {
                                                                                     BuyerName = x.Key.BuyerName,
                                                                                     DFRPercent = Convert.ToInt32(x.Average(y => y.DFRPercent)),
                                                                                     DemandDay = x.Key.DemandDate.ToString("dd MMM")
                                                                                 }).OrderByDescending(x => x.DemandDay).ToList();
                        dfrDashboardDc.DFRDashboardGraphDcs = DFRDashboardGraphDcs;
                        dfrDashboardDc.TotalRed = DFRDashboardDataDcs.Where(x => x.status == "Red").Sum(x => x.DFRPercent) * 100 / DFRDashboardDataDcs.Sum(x => x.DFRPercent);
                        dfrDashboardDc.TotalGreen = DFRDashboardDataDcs.Where(x => x.status == "Green").Sum(x => x.DFRPercent) * 100 / DFRDashboardDataDcs.Sum(x => x.DFRPercent);

                    }
                }

            }
            return dfrDashboardDc;
        }

        [Route("GetCFRDashBoardData")]
        [HttpPost]
        public async Task<CFRDashboardDc> GetCFRDashBoardData(CFRDashboardRequestDc cfrDashboardRequestDc)
        {
            CFRDashboardDc cfrDashboardDc = new CFRDashboardDc();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                SqlParameter parStartDate = new SqlParameter("startDate", cfrDashboardRequestDc.StartDate);
                SqlParameter parEndDate = new SqlParameter("endDate", cfrDashboardRequestDc.EndDate);

                DataTable dtBuyerId = new DataTable();
                dtBuyerId.Columns.Add("IntValue");
                foreach (var item in cfrDashboardRequestDc.buyerIds)
                {
                    var dr = dtBuyerId.NewRow();
                    dr["IntValue"] = item;
                    dtBuyerId.Rows.Add(dr);
                }
                var parbuyerId = new SqlParameter("buyerIds", dtBuyerId);
                parbuyerId.SqlDbType = SqlDbType.Structured;
                parbuyerId.TypeName = "dbo.IntValues";

                DataTable dtwarehouseId = new DataTable();
                dtwarehouseId.Columns.Add("IntValue");
                foreach (var item in cfrDashboardRequestDc.warehouseIds)
                {
                    var dr = dtwarehouseId.NewRow();
                    dr["IntValue"] = item;
                    dtwarehouseId.Rows.Add(dr);
                }
                var parwarehouseId = new SqlParameter("warehouseIds", dtwarehouseId);
                parwarehouseId.SqlDbType = SqlDbType.Structured;
                parwarehouseId.TypeName = "dbo.IntValues";

                DataTable dtcategoriesId = new DataTable();
                dtcategoriesId.Columns.Add("IntValue");
                foreach (var item in cfrDashboardRequestDc.categoriesIds)
                {
                    var dr = dtcategoriesId.NewRow();
                    dr["IntValue"] = item;
                    dtcategoriesId.Rows.Add(dr);
                }
                var parcategoriesId = new SqlParameter("categoriesIds", dtcategoriesId);
                parcategoriesId.SqlDbType = SqlDbType.Structured;
                parcategoriesId.TypeName = "dbo.IntValues";


                DataTable dtsubcategoriesId = new DataTable();
                dtsubcategoriesId.Columns.Add("IntValue");
                foreach (var item in cfrDashboardRequestDc.subcategoriesIds)
                {
                    var dr = dtsubcategoriesId.NewRow();
                    dr["IntValue"] = item;
                    dtsubcategoriesId.Rows.Add(dr);
                }
                var parsubcategoriesId = new SqlParameter("subcategoriesIds", dtsubcategoriesId);
                parsubcategoriesId.SqlDbType = SqlDbType.Structured;
                parsubcategoriesId.TypeName = "dbo.IntValues";

                DataTable dtbrandId = new DataTable();
                dtbrandId.Columns.Add("IntValue");
                foreach (var item in cfrDashboardRequestDc.brandIds)
                {
                    var dr = dtbrandId.NewRow();
                    dr["IntValue"] = item;
                    dtbrandId.Rows.Add(dr);
                }
                var parbrandId = new SqlParameter("brandIds", dtbrandId);
                parbrandId.SqlDbType = SqlDbType.Structured;
                parbrandId.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.Parameters.Add(parStartDate);
                cmd.Parameters.Add(parEndDate);
                cmd.Parameters.Add(parbuyerId);
                cmd.Parameters.Add(parwarehouseId);
                cmd.Parameters.Add(parcategoriesId);
                cmd.Parameters.Add(parsubcategoriesId);
                cmd.Parameters.Add(parbrandId);

                cmd.CommandText = "[dbo].[GetCFRDashBoardData]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                // Run the sproc
                var reader = cmd.ExecuteReader();
                List<CFRDashboardDataDc> CFRDashboardDataDcs = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<CFRDashboardDataDc>(reader).ToList();
                cfrDashboardDc.CFRDashboardDataDcs = CFRDashboardDataDcs;

                if (CFRDashboardDataDcs != null && CFRDashboardDataDcs.Any())
                {
                    List<CFRDashboardGraphDc> CFRDashboardGraphDcs = CFRDashboardDataDcs.GroupBy(x => new { x.buyerName, x.CreatedDate, x.warehouseName }).Select(x =>
                                                                             new CFRDashboardGraphDc
                                                                             {
                                                                                 BuyerName = x.Key.buyerName,
                                                                                 CFRPercent = Convert.ToInt32(x.Average(y => y.Active_per)),
                                                                                 DemandDay = x.Key.CreatedDate.ToString("dd MMM")
                                                                             }).OrderByDescending(x => x.DemandDay).ToList();

                    cfrDashboardDc.CFRDashboardGraphDcs = CFRDashboardGraphDcs;
                    cfrDashboardDc.TotalRed = CFRDashboardDataDcs.Where(x => x.status == "Red").Count() * 100 / CFRDashboardDataDcs.Count();
                    cfrDashboardDc.TotalGreen = CFRDashboardDataDcs.Where(x => x.status == "Green").Count() * 100 / CFRDashboardDataDcs.Count();
                }
            }
            return cfrDashboardDc;
        }
        #endregion


        #region Customer Master Data
        [Route("CustomerMasterData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> CustomerMaster()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                List<CustomerMasterData> reportdata = await context.Database.SqlQuery<CustomerMasterData>("Exec sp_CustomerMasterData").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "CustomerMasterData";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "CustomerMasterData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='CustomerMasterData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Customer Master Data Report";
                    string message = "Please find attach Customer Master Data Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Customer Master Data Report To and From empty");
                }
            }
            return true;
        }
        #endregion


        #region Customer Master Data for Customer-Delight
        [Route("CustomerMasterDataForCustomerDelight")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> CustomerMasterForCustomerDelight()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            string zipfilename = ExcelSavePath + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_CustomerMasterDataForCustomerDelight.zip";
            var NewExcelName = zipfilename.Replace(ExcelSavePath, "");


            using (var context = new AuthContext())
            {
                context.Database.CommandTimeout = 400;
                List<CustomerMasterData_CustomerDelight> reportdata = await context.Database.SqlQuery<CustomerMasterData_CustomerDelight>("Exec sp_CustomerMasterDataForCustomerDelight").ToListAsync();
                //-----------------------------------------------
                //var dataTables = new List<DataTable>();
                //DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //dt.TableName = "CustomerMasterDataForCustomerDelight";
                //dataTables.Add(dt);

                //string filePath = ExcelSavePath + "CustomerMasterDataForCustomerDelight" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                //if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))

                var fileName = "";
                string path = "";
                int userid = 0; //string LevelName = "Level 0";
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddM") + "CustomerMasterDataForCustomerDelight.csv";
                DataTable dt = ListtoDataTableConverter.ToDataTable(reportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);
                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                if (!string.IsNullOrEmpty(zipfilename))
                //-----------------------------------------------
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='CustomerMasterDataForCustomerDelight'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Customer Master Delight Report";
                    string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                   , HttpContext.Current.Request.Url.DnsSafeHost
                                                                   , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                   , "/ExcelGeneratePath/" + NewExcelName);
                    string message = "Please find below link for Daily Customer Delight Report :" + FileUrl;
                    //string message = "Please find attach Customer Master Data For CustomerDelight Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        //EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                        EmailHelper.SendMail(From, To, Bcc, subject, message, FileUrl);
                    else
                        logger.Error("Customer Master Delight Report To and From empty");
                }
            }
            return true;
        }
        #endregion


        #region Gullak Transaction Report
        [Route("GullakTransactionData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GullakTransaction()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {


                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                context.Database.CommandTimeout = 900;
                var cmd2 = context.Database.Connection.CreateCommand();
                cmd2.CommandText = "[dbo].[sp_GullakTransactionData]";
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader2 = cmd2.ExecuteReader();
                List<GullakTransactionData> reportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<GullakTransactionData>(reader2).ToList();



                // List<GullakTransactionData> reportdata = await context.Database.SqlQuery<GullakTransactionData>("Exec sp_GullakTransactionData").ToListAsync();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "GullakTransactionData";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "GullakTransactionData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='GullakTransactionData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Gullak Transaction Data Report";
                    string message = "Please find attach Gullak Transaction Data Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Gullak Transaction To and From empty");
                }
            }
            return true;
        }
        #endregion


        #region wallet Points
        [Route("WalletPointsData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> WalletPoints()
        {
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            using (var context = new AuthContext())
            {
                //List<WalletPointsData> reportdata = await context.Database.SqlQuery<WalletPointsData>("Exec sp_WalletPointsData").ToListAsync();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                context.Database.CommandTimeout = 900;
                var cmd2 = context.Database.Connection.CreateCommand();
                cmd2.CommandText = "[dbo].[sp_WalletPointsData]";
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader2 = cmd2.ExecuteReader();
                List<WalletPointsData> reportdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<WalletPointsData>(reader2).ToList();

                var dataTables = new List<DataTable>();
                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                dt.TableName = "WalletPointsData";
                dataTables.Add(dt);

                string filePath = ExcelSavePath + "WalletPointsData" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='WalletPointsData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Wallet Points Data Report";
                    string message = "Please find attach Wallet Point History Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
                    else
                        logger.Error("Wallet Points Data Report To and From empty");
                }
            }
            return true;
        }

        #endregion

        #region Order Master, Sales Reg - Return Sales Register Data
        [Route("OrderMaster_Sales_ReturnSalesData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> OrderMaster_Sales_ReturnSalesData()
        {

            int userid = 0; //string LevelName = "Level 0";
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            string zipfilename = ExcelSavePath + userid + "_ExportData.zip";
            var NewExcelName = zipfilename.Replace(ExcelSavePath, "");

            using (var context = new AuthContext())
            {
                string path = "";
                var fileName = "";
                string zipCreatePath = "";

                #region Order Master Data
                var reportdata = await context.Database.SqlQuery<OrderMasterData>("Exec sp_OrderMasterData").ToListAsync();
                string fileUrl = string.Empty;
                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_OrderMasterData.csv";

                DataTable dt = ListtoDataTableConverter.ToDataTable(reportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                //  zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion

                var Currentmonth = DateTime.Now.AddDays(-60);
                var month = Currentmonth.Month;
                var year = DateTime.Now.Year;

                #region Sales Register Data
                context.Database.CommandTimeout = 900;
                var Months = new SqlParameter("month", month);
                var Year = new SqlParameter("year", year);

                List<GetSalesRegisterDc> AssignmentData = context.Database.SqlQuery<GetSalesRegisterDc>("Exec MonthEnd.GetSalesRegister @month,@year", Months, Year).ToList();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_SalesRegData.csv";

                DataTable dt1 = ListtoDataTableConverter.ToDataTable(AssignmentData);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt1.WriteToCsvFile(path);

                //zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion



                #region Return Sales Register Data    
                context.Database.CommandTimeout = 900;
                Months = new SqlParameter("month", month);
                Year = new SqlParameter("year", year);
                List<GetSalesRegisterDc> ReturnSalesData = context.Database.SqlQuery<GetSalesRegisterDc>("Exec MonthEnd.GetReturnSalesRegister @month,@year", Months, Year).ToList();


                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_ReturnSalesRegData.csv";

                DataTable dtReturnSales = ListtoDataTableConverter.ToDataTable(ReturnSalesData);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dtReturnSales.WriteToCsvFile(path);

                //zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion



                if (!string.IsNullOrEmpty(zipfilename))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='OrderMaster_Sales_ReturnSalesData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " OrderMaster_Sales_ReturnSales Data Report ";

                    string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                            , HttpContext.Current.Request.Url.DnsSafeHost
                            , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                            , "/ExcelGeneratePath/" + NewExcelName);
                    string message = "Please find attach Assignment OrderMaster_Sales_ReturnSales Data Report :" + FileUrl;
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, "");
                    else
                        logger.Error("OrderMaster_Sales_ReturnSales Data Report To and From empty");
                }
            }
            return true;
        }
        #endregion


        #region Order Master, Assignment Data, Cash Collection data, Warehouse Collection Data-Cash Data &  Online Data
        [Route("AssignmentOrderData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> OrderMasterData()
        {
            int userid = 0; //string LevelName = "Level 0";
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            string zipfilename = ExcelSavePath + userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_ExportData.zip";

            using (var context = new AuthContext())
            {
                string path = "";
                var fileName = "";
                string zipCreatePath = "";
                context.Database.CommandTimeout = 6000;
                #region Order Master Data

                var reportdata = await context.Database.SqlQuery<OrderMasterData>("Exec sp_OrderMasterData").ToListAsync();
                string fileUrl = string.Empty;
                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_OrderMasterData.csv";

                DataTable dt = ListtoDataTableConverter.ToDataTable(reportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                //  zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion


                #region Assignment Data
                var AssignmentData = await context.Database.SqlQuery<AssignmentData>("Exec sp_AssignmentData").ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_AssignmentData.csv";

                DataTable dt1 = ListtoDataTableConverter.ToDataTable(AssignmentData);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt1.WriteToCsvFile(path);

                //zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion


                #region Cash Collection Data
                DateTime startDates = new DateTime(2022, 4, 1);
                var date = DateTime.Now;

                var StartDate = new SqlParameter
                {
                    ParameterName = "startDate",
                    Value = startDates
                };
                var EndDate = new SqlParameter
                {
                    ParameterName = "endDate",
                    Value = date.Date.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59)
                };
                var CashCollection = await context.Database.SqlQuery<CashCollectionData>("Exec sp_CashCollectionData").ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_CashCollectionData.csv";

                DataTable dt2 = ListtoDataTableConverter.ToDataTable(CashCollection);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt2.WriteToCsvFile(path);

                //   zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion


                #region Warehouse Collection-Cash Data
                DateTime startDates1 = new DateTime(2022, 4, 1);
                var date1 = DateTime.Now;

                var StartDate1 = new SqlParameter
                {
                    ParameterName = "startDate",
                    Value = startDates
                };
                var EndDate1 = new SqlParameter
                {
                    ParameterName = "endDate",
                    Value = date.Date.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59)
                };
                var WarehouseCashCollection = await context.Database.SqlQuery<WarehouseCollectionData_Cash>("Exec sp_WarehouseCollection_CashData").ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_WarehouseCollection_CashData.csv";

                DataTable dt3 = ListtoDataTableConverter.ToDataTable(WarehouseCashCollection);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt3.WriteToCsvFile(path);

                //   zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion


                #region Warehouse Collection-Online Data
                //DateTime startDates1 = new DateTime(2022, 4, 1);
                //var date1 = DateTime.Now;

                StartDate1 = new SqlParameter
                {
                    ParameterName = "startDate",
                    Value = startDates
                };
                EndDate1 = new SqlParameter
                {
                    ParameterName = "endDate",
                    Value = date.Date.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59)
                };
                var WarehouseOnlineCollection = await context.Database.SqlQuery<WarehouseCollectionData_Online>("Exec sp_WarehouseCollection_OnlineData").ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_WarehouseCollection_OnlineData.csv";

                DataTable dt4 = ListtoDataTableConverter.ToDataTable(WarehouseOnlineCollection);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt4.WriteToCsvFile(path);

                //   zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion



                if (!string.IsNullOrEmpty(zipfilename))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='AssignmentOrderData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + " Assignment Order Data Report ";
                    string message = "Please find attach Assignment Order Data Report";
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        EmailHelper.SendMail(From, To, Bcc, subject, message, zipfilename);
                    else
                        logger.Error("Assignment Order Data Report To and From empty");
                }
            }
            return true;
        }
        #endregion


        [Route("GetAccountMonthEndData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetAccountMonthEndData()
        {
            var firstDayOfMonth = DateTime.Now.Day;
            if (firstDayOfMonth == 1)
            {
                ReportManager report = new ReportManager();
                return report.GetAccountMonthEndData();

                #region old code
                //var Currentmonth = DateTime.Now.AddMonths(-1);
                //var month = Currentmonth.Month;
                //var year = Currentmonth.Year;
                //string ExcelSavePath = HttpContext.Current.Server.MapPath("~/MonthEndData/" + DateTime.Now.ToString("MMM-yyyy"));

                //if (!Directory.Exists(ExcelSavePath))
                //    Directory.CreateDirectory(ExcelSavePath);
                ////file delete code..
                //string[] files = Directory.GetFiles(ExcelSavePath);
                //foreach (string file in files)
                //{
                //    File.Delete(file);
                //}
                ////end
                //DateTime start = DateTime.Parse("2019-04-01 00:00:00");
                //DateTime end = DateTime.Today.AddDays(1);
                //var previousEndDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");

                //List<KeyValuePair<string, string>> AccountData = new List<KeyValuePair<string, string>>();

                //AccountData.Add(new KeyValuePair<string, string>("SalesRegister", "MonthEnd.GetSalesRegister"));
                //AccountData.Add(new KeyValuePair<string, string>("SalesReturnRegister", "MonthEnd.GetReturnSalesRegister"));
                //AccountData.Add(new KeyValuePair<string, string>("SalesRegisterExcludingSettledAndPOC", "MonthEnd.GetSalesRegisterExcludingSettled_POC"));
                //AccountData.Add(new KeyValuePair<string, string>("Customerledgersummary", "MonthEnd.GetCustomerledgersummary"));
                //AccountData.Add(new KeyValuePair<string, string>("SupplierledgerSummary", "GetSupplierSummary"));
                //AccountData.Add(new KeyValuePair<string, string>("GRIRDifference", "GrButNoIR"));
                //AccountData.Add(new KeyValuePair<string, string>("PendingFromBuyerSide", "SPBuyerData"));
                //AccountData.Add(new KeyValuePair<string, string>("CMSDump", "MonthEnd.GetCMSDump"));
                //AccountData.Add(new KeyValuePair<string, string>("DamageStock", "MonthEnd.GetDamageStock"));
                //AccountData.Add(new KeyValuePair<string, string>("FreeStock", "MonthEnd.GetFreeStock"));
                //AccountData.Add(new KeyValuePair<string, string>("CurrentStock", "MonthEnd.GetCurrentStock"));
                //AccountData.Add(new KeyValuePair<string, string>("InTransitInventory", "MonthEnd.GetInTransitInventory"));
                //AccountData.Add(new KeyValuePair<string, string>("UnutilisedWalletDiscountPoints", "MonthEnd.GetUnutilisedWalletPoint"));
                //AccountData.Add(new KeyValuePair<string, string>("InventoryAging", "MonthEnd.GetInventoryAging"));
                //AccountData.Add(new KeyValuePair<string, string>("TDSReportAdvancePayment", "MonthEnd.GetTDSAdvancePayment"));
                //AccountData.Add(new KeyValuePair<string, string>("TDSReportBillToBill", "MonthEnd.GetTDSBillToBill"));
                //AccountData.Add(new KeyValuePair<string, string>("TCSReport", "MonthEnd.GetTCSOrderReport"));
                //AccountData.Add(new KeyValuePair<string, string>("FreebiesData", "MonthEnd.GetFreebiesData"));
                //AccountData.Add(new KeyValuePair<string, string>("OrderBillDiscountData", "MonthEnd.GetOrderWiseOfferData"));
                //AccountData.Add(new KeyValuePair<string, string>("WarehouseInTransitStock", "MonthEnd.GetWarehouseInTransitStock"));
                //AccountData.Add(new KeyValuePair<string, string>("InternalTransfers", "MonthEnd.InternalTransfer"));
                //AccountData.Add(new KeyValuePair<string, string>("OrderDeliveryCharges", "MonthEnd.OrderDeliveryCharges"));
                //AccountData.Add(new KeyValuePair<string, string>("DirectUdharReport", "MonthEnd.DirectUdharReport"));
                ////AccountData.Add(new KeyValuePair<string, string>("CurrentNetStock", "MonthEnd.CurrentNetStock"));
                //AccountData.Add(new KeyValuePair<string, string>("ClearanceStockData", "MonthEnd.ClearanceStockData"));
                //AccountData.Add(new KeyValuePair<string, string>("NonSaleableStock", "MonthEnd.NonSaleableStock"));
                //AccountData.Add(new KeyValuePair<string, string>("RedispatchedWalletPointData", "MonthEnd.RedispatchedWalletPointData"));



                //ParallelLoopResult parellelResult = Parallel.ForEach(AccountData, (x) =>
                //{
                //    try
                //    {
                //        var dataTables = new List<DataTable>();
                //        if (x.Key == "SalesRegister")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);

                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<GetSalesRegisterDc>(reader2).ToList();
                //                //    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "SalesReturnRegister")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<GetSalesRegisterDc>(reader2).ToList();
                //                //**********************************************************************
                //                // List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "SalesRegisterExcludingSettledAndPOC")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var StartDate = new SqlParameter("StartDate", start);
                //                var EndDate = new SqlParameter("EndDate", end);
                //                //*************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =  x.Value;
                //                cmd2.Parameters.Add(StartDate);
                //                cmd2.Parameters.Add(EndDate);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<GetSalesRegisterDc>(reader2).ToList();
                //                //****************************************************************
                //                //List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @StartDate,@EndDate", StartDate, EndDate).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "Customerledgersummary")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                //**************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =  x.Value;
                //                //cmd2.Parameters.Add(Months);
                //                //cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<CustomerledgersummaryDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<CustomerledgersummaryDc>(reader2).ToList();
                //                //*****************************************************************
                //                //List<CustomerledgersummaryDc> reportdata = context.Database.SqlQuery<CustomerledgersummaryDc>("Exec " + x.Value).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "SupplierledgerSummary")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = "[dbo]." + x.Value;
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<SupplierSummaryDTO> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<SupplierSummaryDTO>(reader2).ToList();
                //                //***************************************************
                //                // List<SupplierSummaryDTO> reportdata = context.Database.SqlQuery<SupplierSummaryDTO>("Exec " + x.Value).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "GRIRDifference")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var StartDate = new SqlParameter("StartDate", start);
                //                var EndDate = new SqlParameter("EndDate", end);
                //                //**********************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = "[dbo]." + x.Value;
                //                cmd2.Parameters.Add(StartDate);
                //                cmd2.Parameters.Add(EndDate);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<GrButNoIrReportDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<GrButNoIrReportDc>(reader2).ToList();
                //                //**********************************************************
                //                //  List<GrButNoIrReportDc> reportdata = context.Database.SqlQuery<GrButNoIrReportDc>("Exec " + x.Value + " @StartDate,@EndDate", StartDate, EndDate).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "PendingFromBuyerSide")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                //**************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = "[dbo]." + x.Value;
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<IRMasters> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<IRMasters>(reader2).ToList();
                //                //************************************************
                //                // List<IRMasters> reportdata = context.Database.SqlQuery<IRMasters>("Exec " + x.Value).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "CMSDump")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*******************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<CMSDumpDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<CMSDumpDc>(reader2).ToList();

                //                //******************************************************
                //                // List<CMSDumpDc> reportdata = context.Database.SqlQuery<CMSDumpDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "DamageStock")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                //***************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =  x.Value;
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<MonthDamageStockDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<MonthDamageStockDc>(reader2).ToList();
                //                //****************************************************
                //                //List<MonthDamageStockDc> reportdata = context.Database.SqlQuery<MonthDamageStockDc>("Exec " + x.Value).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "FreeStock")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                //***********************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =  x.Value;
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<FreeStockDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<FreeStockDc>(reader2).ToList();
                //                //***********************************************
                //                // List<FreeStockDc> reportdata = context.Database.SqlQuery<FreeStockDc>("Exec " + x.Value).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "CurrentStock")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =  x.Value;
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<CurrentStockDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<CurrentStockDc>(reader2).ToList();
                //                //******************************************************************
                //                //List<CurrentStockDc> reportdata = context.Database.SqlQuery<CurrentStockDc>("Exec " + x.Value).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "InTransitInventory")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var EndDate = new SqlParameter("EndDate", end);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(EndDate);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<InTransitInventoryDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<InTransitInventoryDc>(reader2).ToList();
                //                //******************************************************************
                //                // List<InTransitInventoryDc> reportdata = context.Database.SqlQuery<InTransitInventoryDc>("Exec " + x.Value + " @EndDate", EndDate).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "UnutilisedWalletDiscountPoints")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =x.Value;
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<UnutilisedWalletPointDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<UnutilisedWalletPointDc>(reader2).ToList();
                //                //******************************************************************
                //                //List<UnutilisedWalletPointDc> reportdata = context.Database.SqlQuery<UnutilisedWalletPointDc>("Exec " + x.Value).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "TDSReportAdvancePayment")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<TDSAdvancePaymentDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<TDSAdvancePaymentDc>(reader2).ToList();
                //                //******************************************************************
                //                //List<TDSAdvancePaymentDc> reportdata = context.Database.SqlQuery<TDSAdvancePaymentDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "TDSReportBillToBill")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<TDSReportBillToBillDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<TDSReportBillToBillDc>(reader2).ToList();
                //                //******************************************************************
                //                // List<TDSReportBillToBillDc> reportdata = context.Database.SqlQuery<TDSReportBillToBillDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "TCSReport")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<GetSalesRegisterDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<GetSalesRegisterDc>(reader2).ToList();
                //                //******************************************************************
                //                //  List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "FreebiesData")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<FreebiesDataDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<FreebiesDataDc>(reader2).ToList();
                //                //******************************************************************
                //                // List<FreebiesDataDc> reportdata = context.Database.SqlQuery<FreebiesDataDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }

                //        if (x.Key == "OrderBillDiscountData")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =  x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<OrderBillDiscountDataDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<OrderBillDiscountDataDc>(reader2).ToList();
                //                //******************************************************************
                //                // List<OrderBillDiscountDataDc> reportdata = context.Database.SqlQuery<OrderBillDiscountDataDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "InventoryAging")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =  x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<InventoryAgingDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<InventoryAgingDc>(reader2).ToList();
                //                //******************************************************************
                //                //List<InventoryAgingDc> reportdata = context.Database.SqlQuery<InventoryAgingDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "WarehouseInTransitStock")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var monthEndDate = new SqlParameter("monthEndDate", previousEndDate);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(monthEndDate);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<WarehouseInTransitStockDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<WarehouseInTransitStockDc>(reader2).ToList();
                //                //******************************************************************
                //                //List<WarehouseInTransitStockDc> reportdata = context.Database.SqlQuery<WarehouseInTransitStockDc>("Exec " + x.Value + " @monthEndDate", monthEndDate).ToList();
                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "InternalTransfers")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText =x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<InternalTransferDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<InternalTransferDc>(reader2).ToList();
                //                //******************************************************************
                //                //List<InternalTransferDc> reportdata = context.Database.SqlQuery<InternalTransferDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "OrderDeliveryCharges")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);
                //                //*****************************************************************
                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();                             
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<OrderDeliveryChargesDc> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<OrderDeliveryChargesDc>(reader2).ToList();
                //                //******************************************************************
                //                // List<OrderDeliveryChargesDc> reportdata = context.Database.SqlQuery<OrderDeliveryChargesDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "DirectUdharReport")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);

                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<DirectUdharDC> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<DirectUdharDC>(reader2).ToList();
                //                //    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        //if (x.Key == "CurrentNetStock")
                //        //{
                //        //    using (var context = new AuthContext())
                //        //    {
                //        //        var Months = new SqlParameter("month", month);
                //        //        var Year = new SqlParameter("year", year);

                //        //        if (context.Database.Connection.State != ConnectionState.Open)
                //        //            context.Database.Connection.Open();
                //        //        var cmd2 = context.Database.Connection.CreateCommand();
                //        //        cmd2.CommandText = x.Value;
                //        //        cmd2.Parameters.Add(Months);
                //        //        cmd2.Parameters.Add(Year);
                //        //        cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //        //        cmd2.CommandTimeout = 1200;
                //        //        // Run the sproc
                //        //        var reader2 = cmd2.ExecuteReader();
                //        //        List<CurrentNetStockDC> reportdata = ((IObjectContextAdapter)context)
                //        //        .ObjectContext
                //        //        .Translate<CurrentNetStockDC>(reader2).ToList();
                //        //        //    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //        //        DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //        //        dt.TableName = x.Key;
                //        //        dataTables.Add(dt);
                //        //        string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //        //        ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //        //    }
                //        //}
                //        if (x.Key == "ClearanceStockData")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);

                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<ClearanceStockDataDC> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<ClearanceStockDataDC>(reader2).ToList();
                //                //    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "NonSaleableStock")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);

                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<NonSaleableStockDC> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<NonSaleableStockDC>(reader2).ToList();
                //                //    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //        if (x.Key == "RedispatchedWalletPointData")
                //        {
                //            using (var context = new AuthContext())
                //            {
                //                var Months = new SqlParameter("month", month);
                //                var Year = new SqlParameter("year", year);

                //                if (context.Database.Connection.State != ConnectionState.Open)
                //                    context.Database.Connection.Open();
                //                var cmd2 = context.Database.Connection.CreateCommand();
                //                cmd2.CommandText = x.Value;
                //                cmd2.Parameters.Add(Months);
                //                cmd2.Parameters.Add(Year);
                //                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                //                cmd2.CommandTimeout = 1200;
                //                // Run the sproc
                //                var reader2 = cmd2.ExecuteReader();
                //                List<RedispatchedWalletPointDataDC> reportdata = ((IObjectContextAdapter)context)
                //                .ObjectContext
                //                .Translate<RedispatchedWalletPointDataDC>(reader2).ToList();
                //                //    List<GetSalesRegisterDc> reportdata = context.Database.SqlQuery<GetSalesRegisterDc>("Exec " + x.Value + " @month,@year", Months, Year).ToList();

                //                DataTable dt = ClassToDataTable.CreateDataTable(reportdata);
                //                dt.TableName = x.Key;
                //                dataTables.Add(dt);
                //                string filePath = ExcelSavePath + "\\" + x.Key + DateTime.Now.ToString("MMM-yyyy") + ".xlsx";
                //                ExcelGenerator.DataTable_To_Excel(dataTables, filePath);
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        TextFileLogHelper.LogError("Error During File Create for " + x.Key + ":" + ex.ToString());
                //        EmailHelper.SendMail(AppConstants.MasterEmail, "s.patil@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " --Month End Account Report Error", ex.ToString(), "");
                //    }
                //});
                //if (parellelResult.IsCompleted) ;
                #endregion
            }
            return true;
        }

        [Route("GetMonthEndData")]
        [HttpGet]
        [AllowAnonymous]
        public List<KeyValuePair<string, string>> GetMonthEndData(int month, int Year)
        {
            List<KeyValuePair<string, string>> Datalist = new List<KeyValuePair<string, string>>();
            //var firstDayOfMonth = DateTime.Now.Day;
            DateTime date = new DateTime(Year, month, 1);
            var my = date.ToString("MMM");
            //var Currentmonth = DateTime.Now.ToString("MMM");
            var year = DateTime.Now.Year;
            string CreateMonthName = my + "-" + year;
            //string foldername = Currentmonth + "-" + year;
            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/MonthEndData/" + CreateMonthName);

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);
            string[] files = Directory.GetFiles(ExcelSavePath);
            if (files.Length > 0)
            {
                foreach (string file in files)
                {

                    string filename = Path.GetFileName(file);
                    string Month = CreateMonthName + ".xlsx";
                    var ExcelName = filename.Remove(filename.Length - Month.Length);
                    string FileUrl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                    , "/MonthEndData/" + CreateMonthName + "/" + filename);
                    KeyValuePair<string, string> data = new KeyValuePair<string, string>(ExcelName, FileUrl);
                    Datalist.Add(data);
                }
            }
            else if (files.Length == 0)
            {
                KeyValuePair<string, string> data = new KeyValuePair<string, string>("No record found!!", "No record found!!");
                Datalist.Add(data);
            }
            return Datalist;
        }

        [Route("External/GetCLusterSalesReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<ClusterSaleData>> GetCLusterSalesReport()
        {
            using (var db = new AuthContext())
            {
                db.Database.CommandTimeout = 1200;
                List<ClusterSaleData> clusterData = await db.Database.SqlQuery<ClusterSaleData>("Exec [External].GetCLusterSalesReport").ToListAsync();
                return clusterData;
            }
        }


        [Route("MonthEndDeliveredData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> MonthEndDeliveredData(int month, int year)
        {
            bool status = false;
            using (var db = new AuthContext())
            {
                db.Database.CommandTimeout = 1200;
                // NewSalesAccToDeliveredDate
                ReportManager report = new ReportManager();
                status = await report.MonthEndDeliveredData(month, year);
                if (status)
                    return status;
                else
                    return status;
            }
        }


        [Route("MTDSalesMonthReport")]
        [HttpGet]
        public List<MTDSalesMonthReport> MTDSalesMonthReport()
        {
            using (var db = new AuthContext())
            {
                db.Database.CommandTimeout = 1200;
                List<MTDSalesMonthReport> MTDSalesReportData = db.Database.SqlQuery<MTDSalesMonthReport>("Exec GetMonthWiseSalesDashboard").ToList();
                return MTDSalesReportData;
            }
        }

        [Route("GetDispatchToSpendTracker")]
        [HttpPost]
        public async Task<List<DispatchToSpendTrackerResponse>> DispatchToSpendTrackereExport(DispatchToSpendTrackerOpostDcs dispatchToSpendTrackerOpostDC)
        {
            using (var db = new AuthContext())
            {
                List<DispatchToSpendTrackerResponse> DispatchToSpendTrackerDCData = new List<DispatchToSpendTrackerResponse>();
                var manager = new PoReportsManager();
                DispatchToSpendTrackerDCData = manager.DispatchToSpendTracker(dispatchToSpendTrackerOpostDC);
                return DispatchToSpendTrackerDCData;

            }
        }

        //ritka start//
        [Route("GetSalesRegisterAccount")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetSalesRegisterAccount()
        {
            int userid = 0; //string LevelName = "Level 0";
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            string zipfilename = ExcelSavePath + userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "GetSalesRegisterAccount.zip";

            var NewExcelName = zipfilename.Replace(ExcelSavePath, "");
            using (var context = new AuthContext())
            {
                string path = "";
                var fileName = "";
                string zipCreatePath = "";




                DataTable dt = new DataTable();
                using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                {
                    using (var cmd2 = new SqlCommand("[dbo].[GetSalesRegister_Account]", connection))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();
                        cmd2.Connection = connection;

                        cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd2.CommandTimeout = 1200;
                        cmd2.Parameters.Add("@type", SqlDbType.VarChar).Value = "S";
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd2))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                string fileUrl = string.Empty;
                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_GetSalesRegister.csv";


                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                //  zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);



                DataTable dt1 = new DataTable();
                using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                {
                    using (var cmd2 = new SqlCommand("[dbo].[GetSalesRegister_Account]", connection))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();
                        cmd2.Connection = connection;
                        cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd2.CommandTimeout = 1200;
                        cmd2.Parameters.Add("@type", SqlDbType.VarChar).Value = "R";
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd2))
                        {
                            da.Fill(dt1);
                        }
                    }
                }

                //var reportdata = await context.Database.SqlQuery<IRMasters>("Exec SPBuyerData").ToListAsync();
                string fileUrll = string.Empty;
                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_ReturnSalesRegister.csv";

                //DataTable dtt = ListtoDataTableConverter.ToDataTable(reportdataa);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt1.WriteToCsvFile(path);

                //  zipCreatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ExcelGeneratePath/" + zipfilename);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
            }


            if (!string.IsNullOrEmpty(zipfilename))
            {
                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                string To = "", From = "", Bcc = "";
                DataTable emaildatatable = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='GetSalesRegisterAccount'", connection))
                    {

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(emaildatatable);
                        da.Dispose();
                        connection.Close();
                    }
                }
                if (emaildatatable.Rows.Count > 0)
                {
                    To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                    From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                    Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                }
                string subject = DateTime.Now.ToString("dd MMM yyyy") + " GetSalesRegisterAccount Report ";
                string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                   , HttpContext.Current.Request.Url.DnsSafeHost
                                                                   , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                   , "/ExcelGeneratePath/" + NewExcelName);
                string message = "Please find attach Daily Report : " + FileUrl;
                if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                    EmailHelper.SendMail(From, To, Bcc, subject, message, FileUrl);
                else
                    logger.Error("GetSalesRegisterAccount Report To and From empty");
            }

            return true;
        }
        //ritika end//

        //Daily OrderMaster Data   

        [Route("OrderMasterData")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> OrderDailyData()
        {
            int userid = 0; //string LevelName = "Level 0";
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            string zipfilename = ExcelSavePath + userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_ExportData.zip";
            var NewExcelName = zipfilename.Replace(ExcelSavePath, "");
            using (var context = new AuthContext())
            {
                string path = "";
                var fileName = "";
                #region Order Master Data
                context.Database.CommandTimeout = 1200;
                var reportdata = await context.Database.SqlQuery<OrderMasterData>("Exec sp_DailyOrderData").ToListAsync();
                string fileUrl = string.Empty;
                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_OrderMasterData.csv";

                DataTable dt = ListtoDataTableConverter.ToDataTable(reportdata);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion

                ///Return sale data here
                #region Return Sale Register Data
                context.Database.CommandTimeout = 1200;
                var ReturnSaleRegisterData = await context.Database.SqlQuery<ReturnSaleRegisterData>("Exec sp_ReturnSaleRegisterData").ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_ReturnSaleData.csv";

                DataTable dt1 = ListtoDataTableConverter.ToDataTable(ReturnSaleRegisterData);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt1.WriteToCsvFile(path);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion

                //sale Reg data here 

                #region Sales Register Data
                //DateTime startDates = new DateTime(2022, 4, 1);
                context.Database.CommandTimeout = 1200;
                var SaleRegisterData = await context.Database.SqlQuery<SaleRegisterData>("Exec sp_SaleRegisterData").ToListAsync();

                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "SaleRegisterData.csv";

                DataTable dt2 = ListtoDataTableConverter.ToDataTable(SaleRegisterData);
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt2.WriteToCsvFile(path);

                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
                #endregion

                if (!string.IsNullOrEmpty(zipfilename))
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                    string To = "", From = "", Bcc = "";
                    DataTable emaildatatable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='OrderMasterData'", connection))
                        {

                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(emaildatatable);
                            da.Dispose();
                            connection.Close();
                        }
                    }
                    if (emaildatatable.Rows.Count > 0)
                    {
                        From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                        To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;

                        Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                    }
                    string subject = DateTime.Now.ToString("dd MMM yyyy") + "  OrderMaster,ReturnSale,SaleRegister Data  Report ";
                    string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                  , HttpContext.Current.Request.Url.DnsSafeHost
                                                                  , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                  , "/ExcelGeneratePath/" + NewExcelName);

                    string message = "Please find attach OrderMaster,ReturnSale,SaleRegister Data Report :-" + FileUrl;
                    if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                        // EmailHelper.SendMail(From, To, Bcc, subject, message, zipfilename);
                        EmailHelper.SendMail(From, To, Bcc, subject, message, FileUrl);



                    else
                        logger.Error("OrderMaster,ReturnSale,SaleRegister Report To and From empty");
                }
            }
            return true;
        }

        [Route("GetInventoryAgingData")]
        [HttpPost]
        [AllowAnonymous]
        public List<InventoryAgingDataDC> GetInventoryAgingData(InventoryAgingResponse inventoryAgingData)
        {

            List<InventoryAgingDataDC> inventory = new List<InventoryAgingDataDC>();


            using (var authContext = new AuthContext())
            {
                if (authContext.Database.Connection.State != ConnectionState.Open)
                    authContext.Database.Connection.Open();
                var month = new SqlParameter("@month", inventoryAgingData.month);
                var year = new SqlParameter("@year", inventoryAgingData.year);

                var IdDt = new DataTable();
                IdDt = new DataTable();
                IdDt.Columns.Add("IntValue");
                foreach (var item in inventoryAgingData.warehouseId)
                {
                    DataRow dr = IdDt.NewRow();
                    dr["IntValue"] = item;
                    IdDt.Rows.Add(dr);
                }
                var WarehouseIds = new SqlParameter
                {
                    ParameterName = "@warehouseId",

                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = IdDt
                };


                //var cmd = authContext.Database.Connection.CreateCommand();
                //cmd.CommandText = "[dbo].[GetInventoryAgingData]";
                //cmd.CommandType = System.Data.CommandType.StoredProcedure;
                //cmd.Parameters.Add(param);

                inventory = authContext.Database.SqlQuery<InventoryAgingDataDC>("EXEC GetInventoryAgingData @month,@year,@warehouseId", month, year, WarehouseIds).ToList();
                return inventory;

            }


        }



        [Route("LossPreventionDAta")]
        [HttpPost]
        [AllowAnonymous]
        public dynamic LossAndPreventionData(SpType spType)
        {
            dynamic dataList = null;
            using (var context = new AuthContext())
            {
                if (spType != null && spType.month != null && spType.year != null)
                {
                    if (spType.sptype == 1)//"CycleCount"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 2000;
                        dataList = context.Database.SqlQuery<CycleCountReport>("EXEC LP_CycleCount @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 2)//"Assignment Closer Report"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<AssignmentClosure>("EXEC LP_AssignmentClosure  @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 3)//"Cash Deposit Report"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<CashDepositDc>("EXEC LP_CashDeposit  @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 4)//"Order Master POC Report"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<OrderMasterPoc>("EXEC LP_OrderMasterPoc @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 5)//"Order Clearance Report"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<OrderClearanceDc>("EXEC LP_OrderClearanceData @month,@year,@warehouseIds", month, year, param).ToList();

                    }
                    else if (spType.sptype == 6)//"Virtual Settle Report"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<VirtualSettlementDc>("EXEC LP_VirtualSettlementData  @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 7)//"purchase Register Report"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouse", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@todate", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPPurchaseRegisterDataDc>("EXEC GetPurchaseRegistorData  @month,@year,@warehouse", month, year, param).ToList();
                    }
                    else if (spType.sptype == 8)//"Sales Register Report:"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPSalesRegisterDC>("EXEC Lp_SaleRegisterData  @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 9)//"Credit Note Register Report/return sale register data"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<CreditNoteRegisterReport>("EXEC LP_ReturnSalesRegister @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 10)//"CMS Report"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<CMSReportData>("EXEC LP_CmsReportData @month,@year,@warehouseIds", month, year, param).ToList();
                    }

                    else if (spType.sptype == 11)//"LP_DebitNoteData"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPDebitNoteData>("EXEC LP_DebitNoteData @month,@year,@warehouseIds", month, year, param).ToList();
                    }

                    else if (spType.sptype == 12)//"LP_InternalTransferReport"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPInternalTransferReport>("EXEC LP_InternalTransferReport @month,@year,@warehouseIds", month, year, param).ToList();
                    }

                    else if (spType.sptype == 13 || spType.sptype == 19 || spType.sptype == 20)//"LP_DamageOrderMasterReport"
                    {
                        int ordertype = 0;
                        if (spType.sptype == 13) { ordertype = 6; } //damage 
                        else if (spType.sptype == 19) { ordertype = 10; }//non revenue
                        else { ordertype = 9; } //non sellable

                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        var ordertypes = new SqlParameter("@ordertypes", ordertype);

                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPDamageOrderMasterReport>("EXEC LP_DamageOrderMasterReport @month,@year,@ordertypes,@warehouseIds", month, year, ordertypes, param).ToList();
                    }
                    else if (spType.sptype == 14)//"LP_PRReport"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPPRReport>("EXEC LP_PRReport  @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 15)//"LP_POMasterReport"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPPOMasterReport>("EXEC LP_POMasterReport  @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 16)//"LP_VirtualUnsettledReport"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPVirtualUnsettledReport>("EXEC LP_VirtualUnsettledReport  @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                    else if (spType.sptype == 17)//"LP_EmployeeDetailsReport"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPEmployeeDetailsReport>("EXEC LP_EmployeeDetailsReport  @month,@year,@warehouseIds", month, year, param).ToList();

                    }
                    else if (spType.sptype == 18)//"LP_StockTransferReport"
                    {
                        var IdDt = new DataTable();
                        IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");

                        foreach (var item in spType.warehouseIds)
                        {
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = item;
                            IdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("warehouseIds", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var month = new SqlParameter("@month", spType.month);
                        var year = new SqlParameter("@year", spType.year);
                        context.Database.CommandTimeout = 1500;
                        dataList = context.Database.SqlQuery<LPStockTransferReport>("EXEC LP_StockTransferReport  @month,@year,@warehouseIds", month, year, param).ToList();
                    }
                }
            }
            return dataList;
        }

        #region APIs for new DFR  Pages
        [Route("DFRPendingOrderDetails")]
        [HttpPost]
        public async Task<APIResponse> DFRPendingOrderDetails(DFRPendingOrderDetailsDc dfr)
        {
            APIResponse res = new APIResponse();
            List<BuyerWiseBrandList> result = new List<BuyerWiseBrandList>();
            if (dfr != null && dfr.warehouseids.Any())
            {
                using (AuthContext context = new AuthContext())
                {
                    var BuyerIdList = new DataTable();
                    BuyerIdList.Columns.Add("IntValue");
                    foreach (var item in dfr.buyerIds)
                    {
                        var dr = BuyerIdList.NewRow();
                        dr["IntValue"] = item;
                        BuyerIdList.Rows.Add(dr);
                    }
                    var BuyerIds = new SqlParameter("BuyerIds", BuyerIdList);
                    BuyerIds.SqlDbType = SqlDbType.Structured;
                    BuyerIds.TypeName = "dbo.IntValues";

                    var WarehouseIdList = new DataTable();
                    WarehouseIdList.Columns.Add("IntValue");
                    foreach (var item in dfr.warehouseids)
                    {
                        var dr = WarehouseIdList.NewRow();
                        dr["IntValue"] = item;
                        WarehouseIdList.Rows.Add(dr);
                    }
                    var WarehouseIds = new SqlParameter("warehousetbl", WarehouseIdList);
                    WarehouseIds.SqlDbType = SqlDbType.Structured;
                    WarehouseIds.TypeName = "dbo.IntValues";

                    var BrandIdList = new DataTable();
                    BrandIdList.Columns.Add("IntValue");
                    foreach (var item in dfr.brandids)
                    {
                        var dr = BrandIdList.NewRow();
                        dr["IntValue"] = item;
                        BrandIdList.Rows.Add(dr);
                    }
                    var Brandids = new SqlParameter("SubSubCategories", BrandIdList);
                    Brandids.SqlDbType = SqlDbType.Structured;
                    Brandids.TypeName = "dbo.IntValues";

                    var Date = new SqlParameter("date", dfr.date);

                    res.Data = await context.Database.SqlQuery<DFRPendingOrderDetailsResoponceDc>("exec NewDFRsp @warehousetbl,@SubSubCategories,@BuyerIds,@date", WarehouseIds, Brandids, BuyerIds, Date).ToListAsync();
                    res.Status = true;
                }
            }
            else
            {
                res.Status = false;
                res.Message = "parameters cant be empty";
            }
            return res;
        }
        #endregion


        #region Email For DailyDelivered Report
        [Route("GetDailyDeliveredReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetDailyDeliveredReport()
        {
            int userid = 0; //string LevelName = "Level 0";
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");

            if (!Directory.Exists(ExcelSavePath))
                Directory.CreateDirectory(ExcelSavePath);

            string zipfilename = ExcelSavePath + userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "GetDailyDeliveredReport.zip";

            var NewExcelName = zipfilename.Replace(ExcelSavePath, "");
            using (var context = new AuthContext())
            {
                string path = "";
                var fileName = "";
                string zipCreatePath = "";
                DataTable dt = new DataTable();
                using (var connection = new SqlConnection(context.Database.Connection.ConnectionString))
                {
                    using (var cmd2 = new SqlCommand("[dbo].[Sp_DailyDeliveredReport]", connection))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();
                        cmd2.Connection = connection;

                        cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd2.CommandTimeout = 1200;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd2))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                string fileUrl = string.Empty;
                fileName = userid + "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_GetDailyDelivered.csv";
                path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                dt.WriteToCsvFile(path);
                using (ZipArchive archive = ZipFile.Open(zipfilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(path, fileName);
                }
                File.Delete(path);
            }


            if (!string.IsNullOrEmpty(zipfilename))
            {
                string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
                string To = "", From = "", Bcc = "";
                DataTable emaildatatable = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='GetDailyDelivered'", connection))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(emaildatatable);
                        da.Dispose();
                        connection.Close();
                    }
                }
                if (emaildatatable.Rows.Count > 0)
                {
                    To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
                    From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
                    Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
                }
                string subject = DateTime.Now.ToString("dd MMM yyyy") + " GetDailyDelivered Report ";
                string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                   , HttpContext.Current.Request.Url.DnsSafeHost
                                                                   , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                   , "/ExcelGeneratePath/" + NewExcelName);
                string message = "Please find attach GetDailyDelivered Report : " + FileUrl;
                if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
                    EmailHelper.SendMail(From, To, Bcc, subject, message, FileUrl);
                else
                    logger.Error("GetDailyDelivered Report To and From empty");
            }

            return true;
        }
        #endregion


        #region For invoice no not generated Email
        [Route("OnSubscribeErrorSendMail")]
        [HttpGet]
        [AllowAnonymous]
        public bool OnSubscribeErrorSendMail()
        {
            ReportManager manager = new ReportManager();
            var resp = manager.OnSubscribeErrorSendMail();
            return true;
        }
        #endregion




    }

    //public class DispatchToSpendTrackerDC
    //{
    //    public string WarehouseName { get;set;}ssss
    //    public string ClusterName { get;set;}
    //    public string OrderTakenSalesPerson { get;set;}
    //    public string Skcode { get;set;}
    //    public string CategoryName { get;set;}
    //    public string SubcategoryName { get;set;}
    //    public string SubsubcategoryName { get;set;}
    //    public string itemname { get;set;}
    //    public int orderid { get;set;}
    //    public DateTime OrderDate { get;set;}
    //    public DateTime RTDDate { get;set;}
    //    public DateTime DeliveredDate { get;set;}
    //    public String OrderToDeliveryHrs
    //    {
    //        get
    //        {
    //            return (OrderDate - DeliveredDate).TotalHours.ToString();
    //        }
    //    }
    //    public float UnitPrice { get; set; }
    //    public float BookingValueTotalAmount { get {            
    //            return qty * UnitPrice;
    //        } }
    //    public int qty { get;set;}
    //    public float dispatchUnitPrice { get;set;}
    //    public int dispatchqty { get;set;}
    //    public float CancelValue{get;set;}
    //    public int CancelQty{get;set;}
    //    public string CancelRemarks{get;set;}
    //    public DateTime CancelationDate { get;set;}
    //    public float BillDiscountAmount { get;set;}
    //    public float FreebiesValue{
    //        get
    //        {
    //            return (UnitPrice * APP);
    //        }
    //     }
    //    public float WalletAmount { get;set;}
    //    public string CustomerName { get;set;}
    //    public string ShippingAddress { get;set;}
    //    public float APP { get;set;}

    //}

    //public class DispatchToSpendTrackerOpostDC
    //{
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public int? DateType { get; set; }
    //    public List<int?> WarehouseIds { get; set; }
    //    public List<int?> ClusterIds { get; set; }
    //    public List<int?> SalesPersoneIds { get; set; }
    //}


    public class DFRPendingOrderDetailsDc
    {
        public List<int> warehouseids { get; set; }
        public List<int> brandids { get; set; }
        public List<int> buyerIds { get; set; }
        public string date { get; set; }
    }

    public class DFRPendingOrderDetailsResoponceDc
    {
        public string Brand { get; set; }
        public string BuyerName { get; set; }
        public string WarehouseName { get; set; }
        public int TotalPendingOrder { get; set; }
        public string TotalRedOrder { get; set; }
        public string TotalETAPendingOrder { get; set; }
        public string TotalETARedOrder { get; set; }
        public double Fulfillment { get; set; }
        public double MTDFulfillment { get; set; }
    }

    public class InventoryAgingDataDC
    {
        public string WarehouseName { get; set; }
        public int ItemMultiMrpId { get; set; }

        public double MRP { get; set; }

        public string itemBaseName { get; set; }
        public int WarehouseId { get; set; }
        public DateTime InDate { get; set; }

        public int Ageing { get; set; }
        public int ClosingQty { get; set; }
        public double ClosingAmount { get; set; }
        public string Number { get; set; }

        public string CategoryType { get; set; } // Task SN2-T10425

    }

    public class InventoryAgingResponse
    {
        public int month { get; set; }
        public int year { get; set; }
        public List<int> warehouseId { get; set; }


    }

    public class EmailResponse
    {
        public int OrderId { get; set; }
    }





    public class MTDSalesMonthReport
    {
        public string RegionName { get; set; }
        public string warehouseName { get; set; }
        public double MSale { get; set; }
        public double MDispatch { get; set; }
        public double TSale { get; set; }
        public double TDispatch { get; set; }
        public double CityMSale { get; set; }
        public double CityTSale { get; set; }
        public double KPPMSale { get; set; }
        public double KPPTSale { get; set; }
        public double MSafoyaSale { get; set; }
        public double MSafoyaDispatch { get; set; }
        public double TSafoyaSale { get; set; }
        public double TSafoyaDispatch { get; set; }
        public double KKMSale { get; set; }
        public double KKMDispatch { get; set; }
        public double KKTSale { get; set; }
        public double KKTDispatch { get; set; }
        public double Store1MSale { get; set; }
        public double Store1MDispatch { get; set; }
        public double Store1TSale { get; set; }
        public double Store1TDispatch { get; set; }
        public double Store2MSale { get; set; }
        public double Store2MDispatch { get; set; }
        public double Store2TSale { get; set; }
        public double Store2TDispatch { get; set; }
        public double Store3MSale { get; set; }
        public double Store3MDispatch { get; set; }
        public double Store3TSale { get; set; }
        public double Store3TDispatch { get; set; }

        public int KPPCustomer { get; set; }
        public int CityCustomer { get; set; }
        public int KPPOrder { get; set; }
        public int CityOrder { get; set; }
        public int KKCustomer { get; set; }
        public int KKOrder { get; set; }
        public int Store1Customer { get; set; }
        public int Store1Order { get; set; }
        public int Store2Customer { get; set; }
        public int Store2Order { get; set; }
        public int Store3Customer { get; set; }
        public int Store3Order { get; set; }


    }
    public class CustomerItemTargetReportDc
    {
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public string SalesMan { get; set; }
        public string Skcode { get; set; }
        public string ItemNumber { get; set; }
        public string itemBaseName { get; set; }
        public string SubcategoryName { get; set; }
        public int TargetQty { get; set; }
        public int ThisMonthsSale { get; set; }
        public string AchievedPercentage { get; set; }

    }

    public class SalesManItemTargetReportDc
    {
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
        public string SalesMan { get; set; }
        public string ItemNumber { get; set; }
        public string itemBaseName { get; set; }
        public string SubcategoryName { get; set; }
        public int TargetQty { get; set; }
        public int ThisMonthsSale { get; set; }
        public string AchievedPercentage { get; set; }
    }
    public class BrandCompanyLevelTargetReportDc
    {
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string ClusterName { get; set; }
        public decimal CurrentMonthSales { get; set; }
        public decimal target { get; set; }
        public bool isclaimed { get; set; }
        public DateTime createdDate { get; set; }
        public bool iscompleted { get; set; }
        public string Type { get; set; }
        public int WalletValue { get; set; }
        public int SlabLowerLimit { get; set; }
        public int SlabUpperLimit { get; set; }
        public int NoOfLineItem { get; set; }
        public int RequiredNoOfLineItem { get; set; }
        public string SubcategoryName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsTargetExpire { get; set; }
    }
    public class GSTDc
    {
        public DateTime From { get; set; }
        public DateTime TO { get; set; }
        public int Reporttype { get; set; }
    }

    public class IRMasters
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public string IRID { get; set; }
        public int supplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public double TotalAmount { get; set; }
        public string IRStatus { get; set; }
        public double? Gstamt { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? Discount { get; set; }
        public double IRAmountWithTax { get; set; }
        public double IRAmountWithOutTax { get; set; }
        public double TotalAmountRemaining { get; set; }
        public string PaymentStatus { get; set; }
        public int PaymentTerms { get; set; }
        public string IRType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Deleted { get; set; }
        public int? Progres { get; set; }
        public string Remark { get; set; }
        public string RejectedComment { get; set; }
        public string ApprovedComment { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }

        public int? DueDays { get; set; }
        public double? CashDiscount { get; set; }
        public double? FreightAmount { get; set; }

        //public ICollection<InvoiceReceiptDetail> InvoiceReceiptDetails { get; set; }
        //public ICollection<IRCreditNoteMaster> IRCreditNoteMasters { get; set; }
        //[NotMapped]
        //public List<IR_Confirm> purDetails { get; set; }
        public int IrSerialNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? IRApprovedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string IRNNumber { get; set; }
        public string WarehouseName { get; set; }
    }

    public class OrderStatusParamRequest
    {
        public List<int> Warehouseid { get; set; }
        public List<int> Clusterid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class GetWorkingCapitalFields
    {
        public string Id { get; set; }
        public decimal ChequeBounce { get; set; }
        public double AgentDues { get; set; }
        public double SupplierCredit { get; set; }
        public double SupplierAdvances { get; set; }
    }
    public class GetWorkingCapitalfilters
    {
        public List<int> warehouseids { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public DateTime Month { get; set; }
    }
    public class StockTransit
    {
        public int PurchaseOrderId { get; set; }
        public DateTime PoDate { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string WarehouseName { get; set; }
        public double? IntransitQty { get; set; }
        public double? IntransitAmount { get; set; }
    }
    public class dataSelect
    {
        public int totalOrder { get; set; }
        public double totalSale { get; set; }
        public int pendingOrder { get; set; }
        public double PendingSale { get; set; }
        public int notDelivered { get; set; }
        public double notDeliveredSale { get; set; }
        public int pendingOrder_2 { get; set; }
        public double PendingSale2 { get; set; }
        public int totalDelivered { get; set; }
        public double deliveredSale { get; set; }
        public int cancelOrder { get; set; }
        public int activeRetailers { get; set; }
        public int activeBrands { get; set; }
    }
    public class orderMasterlist
    {
        public int id { get; set; }
        public string name { get; set; }
        public string WarehouseName { get; set; }
        public int OrderId { get; set; }
        public int retaileId { get; set; }
        public int brandId { get; set; }
        public int? cityid { get; set; }
        public int warehouseid { get; set; }
        public int? salespersonid { get; set; }
        public int clusterid { get; set; }

        public int totalOrder { get; set; }
        public double TotalAmount { get; set; }
        public int activeRetailers { get; set; }
        public int activeBrands { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime updatedDate { get; set; }

        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string AgentName { get; set; }
        public int SignUp { get; set; }
        public int TotalActiveRetailers { get; set; }
        public int FreqOfOrder { get; set; }
        public double AvgOrderValue { get; set; }
        public double AvgLineItem { get; set; }
        public int AppDownloads { get; set; }
        public double OnlineOrderPercent { get; set; }
        public double KisanKiranaPercent { get; set; }
        public double OnlineSales { get; set; }
        public string BrandName { get; set; }
        public double KisanKiranaAmount { get; set; }
        public string StoreName { get; set; }
    }

    public class DiyData
    {
        public int SubCategoryId { get; set; }
        public string Brand { get; set; }
        public int SkuCount { get; set; }
        public int TargetSku { get; set; }
        public int SuccessSku { get; set; }
        public int StoreViews { get; set; }
        public int TargetStores { get; set; }
        public int ActiveRetailers { get; set; }
        public int OrderCount { get; set; }
        public double Sale { get; set; }
        public double DispatchAmount { get; set; }
        public double CancelAmount { get; set; }
        public double AvgLineItem { get; set; }
        public int SuccessSkuAdoption { get; set; }
        public double AchievementPercent { get; set; }
        public double CfrPercent { get; set; }

    }


    public class RetailersCount
    {
        public int Id { get; set; }
        public string name { get; set; }
        public int Signup { get; set; }
        public int TotalActiveRetailers { get; set; }
    }


    public class MainReports
    {
        public List<orderMasterlist> reports { get; set; }
        //public List<dynamic> ConsolidatedList { get; set; }
        public List<GraphData> GraphData { get; set; }
        //public string AlaSqlQuery { get; set; }
        public string FileUrl { get; set; }
    }

    public class GraphData
    {
        public string name { get; set; }
        public string type { get; set; }
        public bool showInLegend { get; set; }
        public List<dataPoints> dataPoints { get; set; }
    }

    public class dataPoints
    {
        public DateTime x { get; set; }
        public double TotalAmount { get; set; }
        public int activeRetailers { get; set; }
        public int activeBrands { get; set; }
        public int totalOrder { get; set; }
        public double OnlineSales { get; set; }
        public double KisanKiranaSales { get; set; }
        public dynamic y { get; set; }
    }

    public class GetOrderStatusReportDC
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public string AgentName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double PendingValue { get; set; }
        public int PendingCount { get; set; }
        public double ShippedValue { get; set; }
        public int ShippedCount { get; set; }
        public double IssuedValue { get; set; }
        public int IssuedCount { get; set; }
        public double ReadytoDispatchValue { get; set; }
        public int ReadytoDispatchCount { get; set; }
        public double DeliveryRedispatchValue { get; set; }
        public int DeliveryRedispatchCount { get; set; }
        public double DeliveredValue { get; set; }
        public int DeliveredCount { get; set; }
        public double InTransitValue { get; set; }
        public int InTransitCount { get; set; }
    }
    public class OrderMasterDC
    {
        public int OrderId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public string invoice_no { get; set; }
        public double GrossAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalCount { get; set; }

    }

    public class PoOrderMasterDC
    {
        public int OrderId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public string invoice_no { get; set; }
        public double GrossAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? TotalCount { get; set; }
        public int CityId { get; set; }

    }

    public class ExportPoOrderMasterDC
    {
        public int POID { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; }
        public double? MRP { get; set; }
        public double? SellingPrice { get; set; }
        public int? Quantity { get; set; }
        public double? UnitPrice { get; set; }
        public double? TotalPrice { get; set; }
        public double? OrderValue { get; set; }
        public string Status { get; set; }
        public DateTime? DeliveryCancelledDate { get; set; }

    }

    public class POOrderMasterResponseDC
    {
        public List<PoOrderMasterDC> PoOrderMasterDCList { get; set; }
        public int totalRecords { get; set; }
    }

    public class ERPPageVisistData
    {
        public string UserName { get; set; }
        public DateTime VisitedOn { get; set; }
        public int RemainingTimeinHrs { get; set; }
    }

    public class GetPoOrderListDc
    {

        public List<string> Skcode { get; set; }
        public string Keyword { get; set; }
        public DateTime? Fromdate { get; set; }
        public DateTime? Todate { get; set; }
        public List<string> Status { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public List<int> CityId { get; set; }
        public string SortField { get; set; }
        public string SortOrder { get; set; }

    }
    public class WalletPointHistory
    {
        public int OrderId { get; set; }
        public double WalletAmount { get; set; }
        public double TotalAmount { get; set; }
        public string Skcode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string comment { get; set; }
        public string Status { get; set; }
        public string invoice_no { get; set; }
        public string BillingAddress { get; set; }
    }

    public class FreebiesData
    {
        public string Skcode { get; set; }
        public int OrderId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public double AveragePurchasePrice { get; set; }
        public double NetPurchasePrice { get; set; }
        public string itemNumber { get; set; }
        public string WarehouseName { get; set; }
        public int salequantity { get; set; }
        public double DispatchPrice { get; set; }
        public DateTime dispactedDate { get; set; }
        public string Status { get; set; }
        public DateTime OrderedDate { get; set; }
        public string OrderType { get; set; }
        public bool IsFreeItem { get; set; }

    }

    public class GetBillDiscountFreeItemData
    {
        public string Skcode { get; set; }
        public int OrderId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public double AveragePurchasePrice { get; set; }
        public string itemNumber { get; set; }
        public string WarehouseName { get; set; }
        public int salequantity { get; set; }
        public double DispatchPrice { get; set; }
        public DateTime? DispactedDate { get; set; }
        public string Status { get; set; }
        public DateTime? OrderDate { get; set; }
        public string OrderType { get; set; }

    }

    public class DamageMovement
    {
        public int DamageStockId { get; set; }
        public string WarehouseName { get; set; }
        public string StoreName { get; set; }

        //public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        //public double UnitPrice { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        //public double PurchasePrice { get; set; }
        public int InwordQty { get; set; }
        public int outwordQty { get; set; }
        public string ReasonToTransfer { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public double? APP { get; set; }

        //public string ABCClassification { get; set; }
    }

    public class NonSellableMovement
    {
        public string WarehouseName { get; set; }
        public string StoreName { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public int? InwordQty { get; set; }
        public int? outwordQty { get; set; }
        public string ReasonToTransfer { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string CreatedBy { get; set; }

        public double? APP { get; set; }



    }


    public class SupplierSummaryDTO
    {
        public string SUPPLIERCODES { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public double OpeningBalance { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
        public double Balance { get; set; }

        public string WarehouseName { get; set; }
    }

    public class InactiveInventoryDC
    {
        public string ItemNumber { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double? MRP { get; set; }
        public int IsActive { get; set; }
        public int CurrentNetInventory { get; set; }
        public int currentinventory { get; set; }
        public int OpenPOQTy { get; set; }
        public int CurrentDeliveryCanceledInventory { get; set; }
        public int NetInventory { get; set; }
        public int FreestockNetInventory { get; set; }
        public int AverageAging { get; set; }
        public double? AgingAvgPurchasePrice { get; set; }
        public double? Unitprice { get; set; }
        public double? CurrentNetStockAmount { get; set; }
        public string ABCClassification { get; set; }

        public double? AveragePurchasePrice { get; set; }
        public double? MarginPercent { get; set; }
        public int ItemlimitQty { get; set; }
        public int ItemLimitSaleQty { get; set; }
        public string BrandName { get; set; }
        public string storeName { get; set; }
    }
    public class OverallOrderMasterStatusReportDC
    {
        public string OrderType { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string CustomerName { get; set; }
        public double TotalAmount { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? orderdate { get; set; }
        public string paymentMode { get; set; }
        public DateTime? DispatchedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public int? AssignmentNumber { get; set; }
        public string DboyName { get; set; }
    }
    public class GetGullakInPaymentsDc
    {
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
        public DateTime CreatedDate { get; set; }
        public double amount { get; set; }
        public string PaymentFrom { get; set; }
        public string GatewayTransId { get; set; }
        public string GatewayResponse { get; set; }
        public string status { get; set; }
        public string Comment { get; set; }
    }


    public class CustomerMasterData
    {
        public string RetailersCode { get; set; }
        public string ClusterName { get; set; }
        public string Warehouse { get; set; }
        public string CustomerVerify { get; set; }
    }

    public class OrderMasterDailyData
    {
        public string OrderType { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string Skcode { get; set; }
        public double TotalAmount { get; set; }
        public string paymentMode { get; set; }
        public DateTime? Order_Place_Date { get; set; }
        public string WarehouseName { get; set; }
        public int? AssignmentNo { get; set; }
        public string Assignment_Status { get; set; }
        public DateTime? Date_of_Assignment_freeze { get; set; }
        public DateTime? Tax_Invoice_date { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }
        public DateTime? Settlement_Date { get; set; }
        public double Dispatched_Total_Amt { get; set; }
        public double TCSAmount { get; set; }
        public double WalletAmount { get; set; }
        public double BillDiscountAmount { get; set; }
        public double Order_place_value { get; set; }
        public double IGSTTaxAmmount { get; set; }
        public double CGST_SGST_Amount { get; set; }
        public double CessTaxAmount { get; set; }
        public double Total_Invoice_Value { get; set; }
        public string Advance { get; set; }
        public string MOP { get; set; }
        public string MOP_Reference_number { get; set; }
        public double Cash { get; set; }
        public double hdfc { get; set; }
        public double Gullak { get; set; }
        public double RTGSNEFT { get; set; }
        public double mPos { get; set; }
        public double chqbook { get; set; }
        public double ePaylater { get; set; }
        public double Cheque { get; set; }
        public double DirectUdhar { get; set; }
        public string Payment_date { get; set; }
    }


    public class CustomerMasterData_CustomerDelight
    {
        public bool IsKPP { get; set; }
        public int? RetailerId { get; set; }  //to show
        public string RetailersCode { get; set; }
        public string ShopName { get; set; }
        public bool IsFranchise { get; set; }
        public DateTime? FranchiseApprovedDate { get; set; }
        public string Warehouse { get; set; }
        public int? ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public int? ClusterId { get; set; }
        public string ClusterName { get; set; }
        public double latitute { get; set; }
        public double longitute { get; set; }
        public string Description { get; set; }
        public string CustomerVerify { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public string AgentCode { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string CurrentAPKversion { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string CustomerAppTypes { get; set; }
        public string PanNumber { get; set; }  //to show
        public string GSTNo { get; set; }
        public string BillingAddress { get; set; }
        public string BillingZipCode { get; set; }
        public string BillingCity { get; set; }
        public string ShippingAddress { get; set; }
        public string Shipping_Pincode { get; set; }
        public string ShippingCity { get; set; }
        public string State { get; set; }
        public string LicenseNumber { get; set; }
        public string Mobile { get; set; }
        public string DocType { get; set; }
        public DateTime? ActivationDate { get; set; }
        public string ActivationBy { get; set; }


        //TO DO NOT SHOW
        //public string RetailerName { get; set; }
        //public string City { get; set; }
        //public string CreatedBy { get; set; }
        //public bool IsFranchise { get; set; }
        //public DateTime? FranchiseApprovedDate { get; set; }
        //public string Address { get; set; }
        //public string Area { get; set; }
        //public int? ExecutiveId { get; set; }
        //public string ExecutiveName { get; set; }
        //public string Emailid { get; set; }
        //public double latitute { get; set; }
        //public double longitute { get; set; }
        //public string Description { get; set; }
        //public string AgentCode { get; set; }
        //public string GSTNo { get; set; }
        //public string BillingAddress { get; set; }

    }


    public class GullakTransactionData
    {
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public double Amount { get; set; }
        public string ObjectType { get; set; }
        public string ObjectId { get; set; }
        public string Comment { get; set; }
        public string Skcode { get; set; }
        public string Mobile { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string WarehouseName { get; set; }
    }


    public class WalletPointsData
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string Skcode { get; set; }
        public string ClusterName { get; set; }
        public string ShopName { get; set; }
        public string invoice_no { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string itemname { get; set; }
        public string WarehouseName { get; set; }
        public int marginPoint { get; set; }
        public double wheelPoint { get; set; }
    }


    public class OrderMasterData
    {
        public string OrderType { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string Skcode { get; set; }
        public double TotalAmount { get; set; }
        public string paymentMode { get; set; }
        public DateTime? Order_Place_Date { get; set; }
        public string WarehouseName { get; set; }
        public int? AssignmentNo { get; set; }
        public double? deliveryCharge { get; set; }
        public string Assignment_Status { get; set; }
        public DateTime? Date_of_Assignment_freeze { get; set; }
        public DateTime? Tax_Invoice_date { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }
        public DateTime? Settlement_Date { get; set; }
        public double Dispatched_Total_Amt { get; set; }
        public double TCSAmount { get; set; }
        public double WalletAmount { get; set; }
        public double BillDiscountAmount { get; set; }
        public double Order_place_value { get; set; }
        public double IGSTTaxAmmount { get; set; }
        public double CGST_SGST_Amount { get; set; }
        public double CessTaxAmount { get; set; }
        public double Total_Invoice_Value { get; set; }
        public string Advance { get; set; }
        public string MOP { get; set; }
        public string MOP_Reference_number { get; set; }
        public double Cash { get; set; }
        public double hdfc { get; set; }
        public double Gullak { get; set; }
        public double RTGSNEFT { get; set; }
        //public double mPos { get; set; }
        //public double chqbook { get; set; }
        //public double ePaylater { get; set; }
        public double UPI { get; set; }
        public double ScaleUp { get; set; }
        public double PayLater { get; set; }
        public double Cheque { get; set; }
        public double DirectUdhar { get; set; }
        public string Payment_date { get; set; }

        public DateTime? DeliveredDate { get; set; }
    }


    public class AssignmentData
    {
        public long Id { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int Deliveryissueid { get; set; }
        public decimal TotalCashAmt { get; set; }
        public decimal TotalCheckAmt { get; set; }
        public decimal TotalDeliveryissueAmt { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; }
        public decimal RTGSNEFT { get; set; }
        public decimal hdfc { get; set; }
        public decimal chqbook { get; set; }
        public decimal ePaylater { get; set; }
        public decimal DirectUdhar { get; set; }
        public decimal Gullak { get; set; }
        public decimal mPos { get; set; }
    }


    public class ReturnSaleRegisterData
    {  //56
        public int OrderId { get; set; }
        public string ClusterName { get; set; }
        public string OrderType { get; set; }//change
        public string Skcode { get; set; }
        public string StoreName { get; set; }
        public string SalesPerson { get; set; }
        public string invoice_no { get; set; }
        public double invoiceAmount { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string RetailerName { get; set; }

        public string ShopName { get; set; }
        public string Mobile { get; set; }//change
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }

        public string CategoryName { get; set; }
        public string BrandName { get; set; }

        public string WarehouseName { get; set; }

        public double? UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalAmt { get; set; }
        public int DispatchedQuantity { get; set; }
        public double DispatchedTotalAmt { get; set; }
        public string Status { get; set; }

        public string DboyName { get; set; }
        public string DboyMobileNo { get; set; }//changes
        public string HSNCode { get; set; }  //change
        public string GSTno { get; set; }
        public int? AssignmentNo { get; set; }

        public double? AmtWithoutTaxDisc { get; set; }
        public double? AmtWithoutAfterTaxDisc { get; set; }
        public double? TaxPercentage { get; set; }
        public double? TaxAmmount { get; set; }
        public double? SGSTTaxPercentage { get; set; }

        public double? SGSTTaxAmmount { get; set; }

        public double? CGSTTaxPercentage { get; set; }
        public double? CGSTTaxAmmount { get; set; }
        public double? TotalCessPercentage { get; set; }
        public double? CessTaxAmount { get; set; }
        public double IGSTTaxPercentage { get; set; }
        public double? IGSTTaxAmmount { get; set; }
        //PocCreditNoteDate
        public DateTime? PocCreditNoteDate { get; set; }
        public string PocCreditNoteNumber { get; set; }

        public double? WalletAmount { get; set; }
        public double BillDiscountAmount { get; set; }
        public string IRNNo { get; set; }

        public string comment { get; set; }
        public string comments { get; set; }
        public string DeliveryCanceledComments { get; set; }
        public double TCSAmount { get; set; }
        public string paymentMode { get; set; }
        public string paymentThrough { get; set; }
        public string EwayBillNumber { get; set; }

    }

    public class CashCollectionData
    {
        public int Warehouseid { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? BOD { get; set; }
        public int opening { get; set; }
        public int CashCollection { get; set; }
        public int Bank { get; set; }
        public int ExchangeIn { get; set; }
        public int ExchangeOut { get; set; }
        public int Closing { get; set; }
        public decimal onlineCollectionAmount { get; set; }
    }


    public class SaleRegisterData
    {  //56
        public int OrderId { get; set; }
        public string ClusterName { get; set; }
        public string OrderType { get; set; }
        public string Skcode { get; set; }
        public string StoreName { get; set; }
        public string SalesPerson { get; set; }
        public string invoice_no { get; set; }
        public double invoiceAmount { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string RetailerName { get; set; }

        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }

        public string CategoryName { get; set; }
        public string BrandName { get; set; }

        //
        public string WarehouseName { get; set; }
        // public  DateTime? OrderDate { get; set; }
        public double? UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalAmt { get; set; }
        public int DispatchedQuantity { get; set; }
        public double DispatchedTotalAmt { get; set; }
        public string Status { get; set; }

        public string DboyName { get; set; }
        public string DboyMobileNo { get; set; }
        public string HSNCode { get; set; }
        public string GSTno { get; set; }
        public int? AssignmentNo { get; set; }

        public double? AmtWithoutTaxDisc { get; set; }
        public double? AmtWithoutAfterTaxDisc { get; set; }
        public double? TaxPercentage { get; set; }
        public double? TaxAmmount { get; set; }
        public double? SGSTTaxPercentage { get; set; }

        public double? SGSTTaxAmmount { get; set; }

        public double? CGSTTaxPercentage { get; set; }
        public double? CGSTTaxAmmount { get; set; }
        public double? TotalCessPercentage { get; set; }
        public double? CessTaxAmount { get; set; }
        public double IGSTTaxPercentage { get; set; }
        public double? IGSTTaxAmmount { get; set; }
        //PocCreditNoteDate
        public DateTime? PocCreditNoteDate { get; set; }
        public string PocCreditNoteNumber { get; set; }

        public double? WalletAmount { get; set; }
        public double BillDiscountAmount { get; set; }
        public string IRNNo { get; set; }

        public string comment { get; set; }
        public string comments { get; set; }
        public string DeliveryCanceledComments { get; set; }
        public double TCSAmount { get; set; }
        public string paymentMode { get; set; }
        public string paymentThrough { get; set; }
        public string EwayBillNumber { get; set; }
    }


    public class WarehouseCollectionData_Cash
    {
        public int Deliveryissueid { get; set; }
        public double Cashamount { get; set; }
        public double OrderAmount { get; set; }
        public int OrderId { get; set; }
        public string paymentMode { get; set; }
        public string PaymentFrom { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? OrderedDate { get; set; }
        public DateTime? collection_date { get; set; }
    }

    public class WarehouseCollectionData_Online
    {
        public int Deliveryissueid { get; set; }
        public double Onlineamount { get; set; }
        public double OrderAmount { get; set; }
        public int OrderId { get; set; }
        public string paymentMode { get; set; }
        public string PaymentFrom { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? OrderedDate { get; set; }
        public DateTime? collection_date { get; set; }
    }

    //ritika work
    public class GetSalesRegisterAccountDc
    {
        public int OrderId { get; set; }
        public string ClusterName { get; set; }
        public string OrderType { get; set; }
        public string Skcode { get; set; }
        public string StoreName { get; set; }
        public DateTime DispatchDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? OrderDate { get; set; }
        public double MRP { get; set; }
        public double UnitPrice { get; set; }
        public double PtrPrice { get; set; }
        public double TotalAmt { get; set; }
        public double DispatchedTotalAmt { get; set; }
        public string Status { get; set; }
        public double TaxPercentage { get; set; }
        public double TaxAmmount { get; set; }
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }
        public double? WalletAmount { get; set; }
        public double? BillDiscountAmount { get; set; }
        public double? TCSAmount { get; set; }
        public int Quantity { get; set; }
        public int DispatchedQuantity { get; set; }
        public double? PurchasePrice { get; set; }
        public string CustomerType { get; set; }
        public bool IsFreeItem { get; set; }
        public DateTime? POCDate { get; set; }
    }
}

