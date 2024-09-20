using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using LinqKit;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Comparison1")]
    public class Comparison1Controller : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("day")]
        [HttpPost]
        public dynamic GetDay(RequestParam requestParam)
        {
            logger.Info("start OrderMaster: ");
            List<MainReports> MainReport = new List<MainReports>();
            DateTime start = DateTime.Parse("01-01-2017 00:00:00");
            DateTime end = DateTime.Today.AddDays(1);
            List<RetailersCount> retailerCounts = new List<RetailersCount>();

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

                if (requestParam.datefrom == null)
                {
                    requestParam.datefrom = DateTime.Parse("01-01-2017 00:00:00");
                    requestParam.dateto = DateTime.Today.AddDays(1);
                }
                else
                {
                    start = requestParam.datefrom.GetValueOrDefault();
                    end = requestParam.dateto.GetValueOrDefault();
                }
                var array = requestParam.ids;

                var idList = array.Select(x => Convert.ToInt32(x)).ToList();
                var res = getdataList(requestParam.datefrom, requestParam.dateto, requestParam.type, idList, out retailerCounts);

                MainReports MainReport1 = new MainReports();
                List<orderMasterlist> list = new List<orderMasterlist>();
                List<orderMasterlist> report = new List<orderMasterlist>();

                if (requestParam.type == 7)
                {
                    if (res != null)
                    {
                        var dt = ClassToDataTable.CreateDataTable(res);
                        string fileName = "DiyDashboardData" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                        string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                        ExcelGenerator.DataTable_To_Excel(dt, "HubCity", path);

                        string fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , string.Format("ExcelGeneratePath/{0}", fileName));


                        MainReport1.FileUrl = fileUrl;
                        MainReport.Add(MainReport1);
                    }
                }
                else
                {
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueBrand = new List<orderMasterlist>();

                        List<orderMasterlist> dataToExport = new List<orderMasterlist>();

                        var groupedOrders = list.GroupBy(x => x.createdDate);

                        list.GroupBy(x => new { x.retaileId }).ToList().ForEach(x =>
                        {
                            uniqueCustomer.Add(x.FirstOrDefault());
                        });

                        uniqueOrdered = list.GroupBy(x => x.OrderId).Select(x => x.FirstOrDefault()).ToList();

                        list.GroupBy(x => new { x.brandId }).ToList().ForEach(x =>
                        {
                            uniqueBrand.Add(x.FirstOrDefault());
                        });


                        if (list != null && list.Any())
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("activeBrands",typeof(int));
                            dt.Columns.Add("TotalAmount", typeof(double));
                            dt.Columns.Add("name");

                            var storeNames = list.Select(x => x.StoreName).Distinct().ToList();
                            foreach (var item in storeNames)
                            {
                                dt.Columns.Add(item, typeof(double));
                            }

                            dt.Columns.Add("totalOrder", typeof(int));
                            dt.Columns.Add("activeRetailers", typeof(int));
                            dt.Columns.Add("AgentName");
                            dt.Columns.Add("WarehouseName");
                            dt.Columns.Add("FreqOfOrder", typeof(double));
                            dt.Columns.Add("AvgOrderValue", typeof(double));
                            dt.Columns.Add("AvgLineItem", typeof(double));
                            dt.Columns.Add("AppDownloads", typeof(int));
                            dt.Columns.Add("KisanKiranaPercent", typeof(double));
                            dt.Columns.Add("OnlineSales", typeof(double));
                            dt.Columns.Add("OnlineOrderPercent", typeof(double));
                            dt.Columns.Add("SignUp", typeof(int));
                            dt.Columns.Add("TotalActiveRetailers", typeof(int));
                            dt.Columns.Add("KisanKiranaAmount", typeof(double));
                            if (requestParam.type == 4)
                            {
                                foreach (var x in list.GroupBy(x => new { x.name,x.salespersonid }))
                                {
                                    DataRow dr = dt.NewRow();
                                    dr["activeBrands"] = x.Select(z => z.brandId).Distinct().Count();
                                    dr["TotalAmount"] = x.GroupBy(z => z.OrderId).Any() ? x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0;
                                    dr["name"] = x.Key.name;
                                    dr["totalOrder"] = x.Select(z => z.OrderId).Distinct().Count();
                                    dr["activeRetailers"] = x.Select(z => z.retaileId).Distinct().Count();
                                    dr["AgentName"] = string.Join(",", x.Select(z => z.AgentName).Distinct().ToList());
                                    dr["WarehouseName"] = string.Join(",", x.Select(z => z.WarehouseName).Distinct().ToList());
                                    dr["FreqOfOrder"] = x.Select(z => z.OrderId).Distinct().Count() / (x.Select(z => z.retaileId).Distinct().Count() == 0 ? 1 : x.Select(z => z.retaileId).Distinct().Count());
                                    dr["AvgOrderValue"] = (x.GroupBy(z => z.OrderId).Any() ? x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0) / (x.Select(z => z.OrderId).Distinct().Count() == 0 ? 1 : x.Select(z => z.OrderId).Distinct().Count());
                                    dr["AvgLineItem"] = x.Select(z => z.OrderId).Count() / (x.Select(z => z.OrderId).Distinct().Count() == 0 ? 1 : x.Select(z => z.OrderId).Distinct().Count());
                                    dr["AppDownloads"] = x.Where(z => !string.IsNullOrEmpty(z.OrderTakenSalesPerson) && z.OrderTakenSalesPerson.ToLower() == "self").Select(z => z.retaileId).Distinct().Count();
                                    dr["KisanKiranaPercent"] = x.Any(z => !string.IsNullOrEmpty(z.BrandName) && z.BrandName.ToLower().Contains("kisan kirana")) ? x.Where(z => !string.IsNullOrEmpty(z.BrandName) && z.BrandName.ToLower().Contains("kisan kirana")).Sum(z => z.KisanKiranaAmount) / ((x.GroupBy(z => z.OrderId).Any() ? x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0) == 0 ? 1 : x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount))) * 100 : 0;
                                    dr["OnlineSales"] = x.GroupBy(z => z.OrderId).Any() ? x.Where(z => !string.IsNullOrEmpty(z.OrderTakenSalesPerson) && z.OrderTakenSalesPerson.ToLower() == "self").GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0;
                                    dr["OnlineOrderPercent"] = (x.GroupBy(z => z.OrderId).Any() ? x.Where(z => !string.IsNullOrEmpty(z.OrderTakenSalesPerson) && z.OrderTakenSalesPerson.ToLower() == "self").GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0) / ((x.GroupBy(z => z.OrderId).Any() ? x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0) == 0 ? 1 : x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount))) * 100;
                                    dr["SignUp"] = retailerCounts.Where(a => a.name == x.Key.name).Sum(a => a.Signup);
                                    dr["TotalActiveRetailers"] = retailerCounts.Where(a => a.name == x.Key.name).Sum(a => a.TotalActiveRetailers);
                                    dr["KisanKiranaAmount"] = x.Any(z => !string.IsNullOrEmpty(z.BrandName) && z.BrandName.ToLower().Contains("kisan kirana")) ? x.Where(z => !string.IsNullOrEmpty(z.BrandName) && z.BrandName.ToLower().Contains("kisan kirana")).Sum(z => z.KisanKiranaAmount) : 0;

                                    foreach (var item in storeNames)
                                    {
                                        dr[item] = x.Any(z => !string.IsNullOrEmpty(z.StoreName) && z.StoreName.ToLower() == item.ToLower()) ? x.Where(z => !string.IsNullOrEmpty(z.StoreName) && z.StoreName.ToLower() == item.ToLower()).Sum(z => z.TotalAmount) : 0;
                                    }

                                    dt.Rows.Add(dr);
                                }
                            }
                            else
                            {
                                foreach (var x in list.GroupBy(x => new { x.name }))
                                {
                                    DataRow dr = dt.NewRow();
                                    dr["activeBrands"] = x.Select(z => z.brandId).Distinct().Count();
                                    dr["TotalAmount"] = x.GroupBy(z => z.OrderId).Any() ? x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0;
                                    dr["name"] = x.Key.name;
                                    dr["totalOrder"] = x.Select(z => z.OrderId).Distinct().Count();
                                    dr["activeRetailers"] = x.Select(z => z.retaileId).Distinct().Count();
                                    dr["AgentName"] = string.Join(",", x.Select(z => z.AgentName).Distinct().ToList());
                                    dr["WarehouseName"] = string.Join(",", x.Select(z => z.WarehouseName).Distinct().ToList());
                                    dr["FreqOfOrder"] = x.Select(z => z.OrderId).Distinct().Count() / (x.Select(z => z.retaileId).Distinct().Count() == 0 ? 1 : x.Select(z => z.retaileId).Distinct().Count());
                                    dr["AvgOrderValue"] = (x.GroupBy(z => z.OrderId).Any() ? x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0) / (x.Select(z => z.OrderId).Distinct().Count() == 0 ? 1 : x.Select(z => z.OrderId).Distinct().Count());
                                    dr["AvgLineItem"] = x.Select(z => z.OrderId).Count() / (x.Select(z => z.OrderId).Distinct().Count() == 0 ? 1 : x.Select(z => z.OrderId).Distinct().Count());
                                    dr["AppDownloads"] = x.Where(z => !string.IsNullOrEmpty(z.OrderTakenSalesPerson) && z.OrderTakenSalesPerson.ToLower() == "self").Select(z => z.retaileId).Distinct().Count();
                                    dr["KisanKiranaPercent"] = x.Any(z => !string.IsNullOrEmpty(z.BrandName) && z.BrandName.ToLower().Contains("kisan kirana")) ? x.Where(z => !string.IsNullOrEmpty(z.BrandName) && z.BrandName.ToLower().Contains("kisan kirana")).Sum(z => z.KisanKiranaAmount) / ((x.GroupBy(z => z.OrderId).Any() ? x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0) == 0 ? 1 : x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount))) * 100 : 0;
                                    dr["OnlineSales"] = x.GroupBy(z => z.OrderId).Any() ? x.Where(z => !string.IsNullOrEmpty(z.OrderTakenSalesPerson) && z.OrderTakenSalesPerson.ToLower() == "self").GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0;
                                    dr["OnlineOrderPercent"] = (x.GroupBy(z => z.OrderId).Any() ? x.Where(z => !string.IsNullOrEmpty(z.OrderTakenSalesPerson) && z.OrderTakenSalesPerson.ToLower() == "self").GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0) / ((x.GroupBy(z => z.OrderId).Any() ? x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount)) : 0) == 0 ? 1 : x.GroupBy(z => z.OrderId).Sum(z => z.Sum(s => s.TotalAmount))) * 100;
                                    dr["SignUp"] = retailerCounts.Where(a => a.name == x.Key.name).Sum(a => a.Signup);
                                    dr["TotalActiveRetailers"] = retailerCounts.Where(a => a.name == x.Key.name).Sum(a => a.TotalActiveRetailers);
                                    dr["KisanKiranaAmount"] = x.Any(z => !string.IsNullOrEmpty(z.BrandName) && z.BrandName.ToLower().Contains("kisan kirana")) ? x.Where(z => !string.IsNullOrEmpty(z.BrandName) && z.BrandName.ToLower().Contains("kisan kirana")).Sum(z => z.KisanKiranaAmount) : 0;

                                    foreach (var item in storeNames)
                                    {
                                        dr[item] = x.Any(z => !string.IsNullOrEmpty(z.StoreName) && z.StoreName.ToLower() == item.ToLower()) ? x.Where(z => !string.IsNullOrEmpty(z.StoreName) && z.StoreName.ToLower() == item.ToLower()).Sum(z => z.TotalAmount) : 0;
                                    }

                                    dt.Rows.Add(dr);
                                }
                            }
                            string fileName = "HubCityReport" + DateTime.Now.ToString("yyyyddMHHmmss") + ".xlsx";

                            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                            ExcelGenerator.DataTable_To_Excel(dt, "HubCity", path);

                            string fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                             , HttpContext.Current.Request.Url.DnsSafeHost
                                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                             , string.Format("ExcelGeneratePath/{0}", fileName));


                            MainReport1.FileUrl = fileUrl;
                        }

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
        public dynamic GetMonth(DateTime? datefrom, DateTime? dateto, int type, string ids)
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
                    var res = getdata(datefrom, dateto, type, id);
                    MainReports MainReport1 = new MainReports();
                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueMonth = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueBrand = new List<orderMasterlist>();
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
                            orderMasterlist o = uniqueBrand.Where(c => c.brandId == a.brandId && (c.createdDate.Month == a.createdDate.Month && c.createdDate.Year == a.createdDate.Year)).SingleOrDefault();
                            if (o == null)
                            {
                                uniqueBrand.Add(a);
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
                            List<orderMasterlist> Brand = uniqueBrand.Where(a => a.createdDate.Month == c.month && a.createdDate.Year == c.year).ToList();
                            if (Brand.Count == 0)
                            {
                                c.activeBrands = 0;
                            }
                            else
                            {
                                c.activeBrands = Brand.Count;
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
        public dynamic GetYear(DateTime? datefrom, DateTime? dateto, int type, string ids)
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
                    var res = getdata(datefrom, dateto, type, id);
                    MainReports MainReport1 = new MainReports();
                    List<orderMasterlist> list = new List<orderMasterlist>();
                    List<orderMasterlist> report = new List<orderMasterlist>();
                    list = res;
                    if (list.Count != 0)
                    {
                        List<orderMasterlist> uniqueYear = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueOrdered = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueCustomer = new List<orderMasterlist>();
                        List<orderMasterlist> uniqueBrand = new List<orderMasterlist>();
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
                            orderMasterlist o = uniqueBrand.Where(c => c.brandId == a.brandId && c.createdDate.Year == a.createdDate.Year).SingleOrDefault();
                            if (o == null)
                            {
                                uniqueBrand.Add(a);
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
                            List<orderMasterlist> Brand = uniqueBrand.Where(a => a.createdDate.Year == c.year).ToList();
                            if (Brand.Count == 0)
                            {
                                c.activeBrands = 0;
                            }
                            else
                            {
                                c.activeBrands = Brand.Count;
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

        public List<orderMasterlist> getdata(DateTime? datefrom, DateTime? dateto, int type, int id)
        {
            using (var db = new AuthContext())
            {
                List<orderMasterlist> list = new List<orderMasterlist>();
                List<orderMasterlist> report = new List<orderMasterlist>();
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



                    if (datefrom != null && dateto != null)
                    {
                        if (type == 4)
                        {
                            var data = (from i in db.DbOrderDetails
                                        where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid
                                        join j in db.itemMasters on i.ItemId equals j.ItemId
                                        join e in db.DbOrderMaster on i.OrderId equals e.OrderId
                                        join k in db.SubsubCategorys on j.SubsubCategoryid equals k.SubsubCategoryid
                                        select new orderMasterlist
                                        {
                                            salespersonid = i.ExecutiveId,
                                            name = i.ExecutiveName,
                                            cityid = i.CityId,
                                            warehouseid = i.WarehouseId,
                                            id = k.Categoryid,
                                            OrderId = i.OrderId,
                                            retaileId = i.CustomerId,
                                            brandId = k.SubsubCategoryid,
                                            TotalAmount = i.TotalAmt,
                                            createdDate = i.CreatedDate,
                                            updatedDate = i.UpdatedDate
                                        }).OrderBy(x => x.createdDate).ToList();
                            list = data.Where(x => x.salespersonid == id).ToList();
                        }
                        else if (type == 2)
                        {
                            list = (from i in db.DbOrderDetails
                                    where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.WarehouseId == id && i.CompanyId == compid
                                    join j in db.itemMasters on i.ItemId equals j.ItemId
                                    join k in db.SubsubCategorys on j.SubsubCategoryid equals k.SubsubCategoryid
                                    select new orderMasterlist
                                    {
                                        cityid = i.CityId,
                                        warehouseid = i.WarehouseId,
                                        name = i.WarehouseName,
                                        id = k.Categoryid,
                                        OrderId = i.OrderId,
                                        retaileId = i.CustomerId,
                                        brandId = k.SubsubCategoryid,
                                        TotalAmount = i.TotalAmt,
                                        createdDate = i.CreatedDate,
                                        updatedDate = i.UpdatedDate
                                    }).OrderBy(x => x.createdDate).ToList();
                        }
                        else if (type == 3)
                        {
                            list = (from i in db.DbOrderDetails
                                    where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CityId == id && i.CompanyId == compid
                                    join j in db.itemMasters on i.ItemId equals j.ItemId
                                    join k in db.SubsubCategorys on j.SubsubCategoryid equals k.SubsubCategoryid
                                    select new orderMasterlist
                                    {
                                        cityid = i.CityId,
                                        warehouseid = i.WarehouseId,
                                        name = i.City,
                                        id = k.SubsubCategoryid,
                                        OrderId = i.OrderId,
                                        retaileId = i.CustomerId,
                                        brandId = k.SubsubCategoryid,
                                        TotalAmount = i.TotalAmt,
                                        createdDate = i.CreatedDate,
                                        updatedDate = i.UpdatedDate
                                    }).OrderBy(x => x.createdDate).ToList();
                        }
                        else if (type == 5)
                        {
                            db.Database.Log = s => Debug.WriteLine(s);

                            var data = (from i in db.DbOrderDetails
                                        where i.CreatedDate >= datefrom && i.CreatedDate <= dateto && i.CompanyId == compid
                                        join j in db.itemMasters on i.ItemId equals j.ItemId
                                        join e in db.DbOrderMaster on i.OrderId equals e.OrderId
                                        join k in db.SubsubCategorys on j.SubsubCategoryid equals k.SubsubCategoryid
                                        select new orderMasterlist
                                        {
                                            clusterid = e.ClusterId,
                                            cityid = i.CityId,
                                            warehouseid = i.WarehouseId,
                                            name = e.ClusterName,
                                            id = k.SubsubCategoryid,
                                            OrderId = i.OrderId,
                                            retaileId = i.CustomerId,
                                            brandId = k.SubsubCategoryid,
                                            TotalAmount = i.TotalAmt,
                                            createdDate = i.CreatedDate,
                                            updatedDate = i.UpdatedDate
                                        }).OrderBy(x => x.createdDate).ToList();
                            list = data.Where(x => x.clusterid == id).ToList();
                        }
                    }
                    return list;
                }
                catch (Exception ex)
                {
                    return null;

                }
            }
        }

        public dynamic getdataList(DateTime? datefrom, DateTime? dateto, int type, List<int> ids, out List<RetailersCount> retailerCounts)
        {
            using (var db = new AuthContext())
            {
                List<orderMasterlist> list = new List<orderMasterlist>();
                List<orderMasterlist> report = new List<orderMasterlist>();
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

                    retailerCounts = new List<RetailersCount>();
                    var retCountQuery = string.Empty;
                    var execDt = new DataTable();
                    execDt.Columns.Add("IntValue");
                    foreach (var item in ids)
                    {
                        var dr = execDt.NewRow();
                        dr["IntValue"] = item;
                        execDt.Rows.Add(dr);
                    }

                    var Optparam = new SqlParameter("ExecutiveIds", execDt);
                    Optparam.SqlDbType = SqlDbType.Structured;
                    Optparam.TypeName = "dbo.IntValues";
                    var Typeparam = new SqlParameter("Type", type);


                    if (datefrom != null && dateto != null)
                    {
                        var idValues = new DataTable();
                        idValues.Columns.Add("IntValue");
                        foreach (var item in ids)
                        {
                            var dr = idValues.NewRow();
                            dr["IntValue"] = item;
                            idValues.Rows.Add(dr);
                        }

                        var tblParam = new SqlParameter("idTbl", idValues);
                        tblParam.SqlDbType = SqlDbType.Structured;
                        tblParam.TypeName = "dbo.IntValues";

                        var startDateParam = new SqlParameter("startdate", datefrom);
                        var endDateParam = new SqlParameter("enddate", dateto);
                        var typeParam = new SqlParameter("type", type);

                        if (type != 7)
                        {
                            db.Database.CommandTimeout = 1200;
                            var data = db.Database.SqlQuery<orderMasterlist>("exec HubCityStatisticalReport @startdate,@enddate,@type,@idTbl", startDateParam, endDateParam, typeParam, tblParam).ToList();

                            if (data != null && data.Any())
                                list = data.OrderBy(x => x.createdDate).ToList();
                        }
                        else
                        {
                            db.Database.CommandTimeout = 1200;

                            var data = db.Database.SqlQuery<DiyData>("exec HubCityStatisticalReport @startdate,@enddate,@type,@idTbl", startDateParam, endDateParam, typeParam, tblParam).ToList();
                            return data;
                        }
                    }

                    if (list != null && list.Any())
                    {
                        db.Database.CommandTimeout = 1200;

                        retailerCounts = db.Database.SqlQuery<RetailersCount>("exec GetExecutiveCustData @ExecutiveIds,@Type", Optparam, Typeparam).ToList();

                        var clusterIdDt = new DataTable();
                        clusterIdDt.Columns.Add("IntValue");
                        foreach (var item in list.Select(x => x.clusterid).Distinct())
                        {
                            var dr = clusterIdDt.NewRow();
                            dr["IntValue"] = item;
                            clusterIdDt.Rows.Add(dr);
                        }

                        var param = new SqlParameter("param", clusterIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        db.Database.CommandTimeout = 1200;

                        var agents = db.Database.SqlQuery<AgentDTO>("exec GetClusterAgents @param", param).ToList();

                        list.ForEach(x =>
                        {
                            var agent = agents.Where(z => z.ClusterId == x.clusterid).Select(z => z.AgentName);
                            if (agent != null && agent.Any())
                            {
                                x.AgentName = string.Join(", ", agent);
                            }
                            //x.StoreName = x.StoreName.Replace(" ", "_");

                        });
                    }

                    return list;
                }
                catch (Exception ex)
                {
                    retailerCounts = null;
                    return null;

                }
            }
        }

        #region to get cx signed up by Freelancer agent 
        //tejas 04-07-2019
        [Route("SalesManAppF1")]
        [HttpGet]
        public HttpResponseMessage SalesManAppF1(DateTime? datefrom, DateTime? dateto, int id)
        {
            using (var db = new AuthContext())
            {
                List<FAgentCXlist> list = new List<FAgentCXlist>();
                if (datefrom != null && dateto != null)
                {
                    var Customercount = db.freelancerAgentDB.Where(x => x.PeopleID == id && x.CXsignupDate >= datefrom && x.CXsignupDate <= dateto).ToList();
                    foreach (var item in Customercount)
                    {
                        FAgentCXlist OrderCount = db.Customers.Where(x => x.Skcode == item.Skcode && x.Deleted == false).Select(x => new FAgentCXlist { Skcode = x.Skcode, ShopName = x.ShopName, Name = x.Name, CustomerVerify = x.CustomerVerify }).SingleOrDefault();

                        list.Add(OrderCount);
                    }
                }
                var res = new
                {
                    list = list,
                };
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
        #endregion
    }
    // to get cx signed up by Freelancer agent tejas
    public class FAgentCXlist
    {
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string CustomerVerify { get; set; }
    }
    public class orderMasterlistDTM
    {
        public int Categoryid { get; set; }
        public string name { get; set; }
        public int OrderId { get; set; }
        public int retaileId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int? cityid { get; set; }
        public int warehouseid { get; set; }
        public int? salespersonid { get; set; }
        public int clusterid { get; set; }

        public int totalOrder { get; set; }
        public double TotalAmount { get; set; }
        public int activeRetailers { get; set; }
        public int activeBrands { get; set; }

        public DateTime createdDate { get; set; }
        public DateTime updatedDate { get; set; }

        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
    }

    public class RequestParam
    {
        public DateTime? datefrom { get; set; }
        public DateTime? dateto { get; set; }
        public int type { get; set; }
        public List<int> ids { get; set; }
        public int selType { get; set; }
        public string dateFormat { get; set; }
    }

}
