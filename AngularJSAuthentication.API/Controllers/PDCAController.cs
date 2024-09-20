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
    [RoutePrefix("api/PDCA")]
    public class WarehouseReportController : ApiController
    {
        #region PublicVariable        
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region PDCADetails


        /// <summary>
        /// Get Base Category Data
        /// </summary>
        /// <returns></returns>
        //[Authorize]
        [Route("GetBaseCategory")]
        [HttpGet]
        public IEnumerable<WarehouseReport> Get()
        {
            logger.Info("start Category: ");
            List<WarehouseReport> ass = new List<WarehouseReport>();
            using (var db = new AuthContext())
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
                    ass = db.WarehouseReportDB.ToList();
                    logger.Info("End  BaseCategory: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Get WarehouseReportData
        /// </summary>
        /// <param name="Month"></param>
        /// <returns>List<WarehouseReportData></returns>
        //[Authorize]
        [Route("GetWarehouseReport")]
        [HttpGet]
        public List<WarehouseReportData> GetWarehouseReport(DateTime Month)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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

                    var list = db.BaseCategoryDb.Where(x => x.IsActive == true && x.Deleted == false).ToList();
                    List<WarehouseReportData> reportdata = new List<WarehouseReportData>();
                    foreach (var item in list)
                    {

                        var catlist = db.Categorys.Where(x => x.BaseCategoryId == item.BaseCategoryId && x.IsActive == true && x.Deleted == false).ToList();
                        WarehouseReportData wrd = new WarehouseReportData();
                        wrd.BaseCategoryid = item.BaseCategoryId;
                        wrd.BaseCategoryName = item.BaseCategoryName;
                        wrd.Percentage = 0;
                        wrd.WarehouseID = 0;
                        List<Warehousecategory> sub = new List<Warehousecategory>();
                        foreach (var cat in catlist)
                        {
                            Warehousecategory categor = new Warehousecategory();
                            categor.CategoryID = cat.Categoryid;
                            categor.CategoryName = cat.CategoryName;
                            sub.Add(categor);
                        }
                        wrd.category = sub;
                        reportdata.Add(wrd);
                    }

                    return reportdata;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// GetWarehouseReportMonthWise
        /// </summary>
        /// <param name="datefrom"></param>
        /// <param name="dateto"></param>
        /// <returns></returns>
        //[Authorize]
        [Route("GetWarehouseReportMonthWise")]
        [HttpGet]
        public IEnumerable<WarehouseReportDetails> GetWarehouseReportMonthWise(DateTime datefrom, DateTime dateto)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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
                    var list = db.WarehouseReportDetailsDB.Where(x => x.TransactionMonth > datefrom && x.TransactionMonth < dateto).ToList();
                    return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// GetWarehouseDetailsReport
        /// </summary>
        /// <param name="basecategoryid"></param>
        /// <param name="warehouseid"></param>
        /// <param name="selectmonth"></param>
        /// <returns></returns>
        //[Authorize]
        [Route("GetWarehouseDetailsReport")]
        [HttpGet]
        public GetWarehouseReport GetWarehouseDetailsReport(int basecategoryid, int warehouseid, DateTime selectmonth)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    var StartDate = new DateTime(selectmonth.Year, selectmonth.Month, 1);
                    var lastdate = StartDate.AddMonths(1);
                    var PreStartDate = StartDate.AddMonths(-1);
                    var PreLastDate = lastdate.AddMonths(-1);
                    GetWarehouseReport responce = new GetWarehouseReport();
                    List<WarehouseReportDetails> sub = new List<WarehouseReportDetails>();
                    int updatecount = 0;
                    var details = db.WarehouseReportDetailsDB.Where(x => x.TransactionMonth.Year == StartDate.Year && x.TransactionMonth.Month == StartDate.Month && x.BaseCategoryid == basecategoryid && x.Warehouseid == warehouseid).ToList();
                    if (details.Count > 0)
                    {

                        var categorynames = details.Select(x => x.Categoryname).ToList();
                        var categoryamount = (from c in db.DbOrderMaster.Where(x => x.Status != "Order Canceled" && x.Status != "Post Order Canceled")
                                              join p in db.DbOrderDetails.Where(z => categorynames.Contains(z.CategoryName) && z.WarehouseId == warehouseid && z.OrderDate > PreStartDate && z.OrderDate < lastdate)
                                              on c.OrderId equals p.OrderId
                                              select new { p.TotalAmt, p.CategoryName, p.OrderDate }).ToList();

                        foreach (var item in details)
                        {

                            var todaymonthcategoryamount = categoryamount.Where(z => z.OrderDate > StartDate && z.OrderDate < lastdate);
                            var Premonthcategoryamount = categoryamount.Where(z => z.OrderDate > PreStartDate && z.OrderDate < PreLastDate);
                            WarehouseReportDetails categor = new WarehouseReportDetails();
                            categor.BaseCategoryid = item.BaseCategoryid;
                            categor.Categoryid = item.Categoryid;
                            categor.Categoryname = item.Categoryname;
                            categor.Percentage = item.Percentage;
                            categor.PDCAAmount = item.PDCAAmount;
                            categor.Amount = item.Amount;
                            categor.Target = item.Target;
                            categor.TargetAmount = item.TargetAmount;
                            double sum = 0;
                            if (todaymonthcategoryamount.Any(x => x.CategoryName == item.Categoryname))
                                sum = todaymonthcategoryamount.Where(x => x.CategoryName == item.Categoryname).Sum(x => x.TotalAmt);

                            categor.MTD = Convert.ToDecimal(Math.Round(sum, 2));

                            double preMTD = 0;
                            if (Premonthcategoryamount.Any(x => x.CategoryName == item.Categoryname))
                                preMTD = Premonthcategoryamount.Where(x => x.CategoryName == item.Categoryname).Sum(x => x.TotalAmt);

                            categor.PreviousMTD = Convert.ToDecimal(Math.Round(preMTD, 2));
                            categor.Comment = item.Comment;
                            categor.PDCAAmount = item.PDCAAmount;
                            updatecount = item.Updated;
                            sub.Add(categor);
                        }
                        responce.warehousereportdetails = sub;
                        responce.AddPermission = false;

                        var currentdate = indianTime;
                        if (indianTime.Month != selectmonth.Month)
                            responce.UpdatePermission = false;
                        //if (currentdate.Day > 4 && currentdate.Day < 11 && updatecount == 0)
                        else if (currentdate.Day > 4 && currentdate.Day < 11)
                            responce.UpdatePermission = true;
                        //else if (currentdate.Day > 14 && currentdate.Day < 21 && updatecount == 1)
                        else if (currentdate.Day > 14 && currentdate.Day < 21)
                            responce.UpdatePermission = true;
                        //else if (currentdate.Day > 24 && currentdate.Day < 32 && updatecount == 2)
                        else if (currentdate.Day > 24 && currentdate.Day < 32)
                            responce.UpdatePermission = true;
                        else responce.UpdatePermission = false;
                    }
                    else
                    {
                        var catlist = db.WarehouseReportCategoryDB.Where(x => x.BaseCategoryid == basecategoryid && x.Warehouseid == warehouseid).ToList();
                        if (catlist != null && catlist.Any())
                        {
                            var categorynames = catlist.Select(x => x.Categoryname).ToList();
                            var categoryamount = (from c in db.DbOrderMaster.Where(x => x.Status != "Order Canceled" && x.Status != "Post Order Canceled")
                                                  join p in db.DbOrderDetails.Where(z => categorynames.Contains(z.CategoryName) && z.WarehouseId == warehouseid && z.OrderDate > StartDate && z.OrderDate < lastdate)
                                                  on c.OrderId equals p.OrderId
                                                  select new { p.TotalAmt, p.CategoryName }).ToList();
                            foreach (var cat in catlist)
                            {
                                WarehouseReportDetails categor = new WarehouseReportDetails();
                                categor.BaseCategoryid = cat.BaseCategoryid;
                                categor.Categoryid = cat.Categoryid;
                                categor.Categoryname = cat.Categoryname;
                                double sum = 0;
                                if (categoryamount.Any(x => x.CategoryName == cat.Categoryname))
                                    sum = categoryamount.Where(x => x.CategoryName == cat.Categoryname).Sum(x => x.TotalAmt);
                                categor.MTD = Convert.ToDecimal(Math.Round(sum, 2));
                                categor.Percentage = cat.Percentage;
                                categor.PDCAAmount = 0;
                                categor.Amount = 0;
                                categor.Target = 0;
                                categor.TargetAmount = 0;
                                categor.Comment = "";
                                categor.PDCAAmount = 0;
                                sub.Add(categor);
                            }
                            responce.warehousereportdetails = sub;
                            responce.AddPermission = true;

                            var currentdate = indianTime;
                            responce.UpdatePermission = false;
                        }
                    }
                    return responce;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// GetWarehouseDetailsReportWithPreMTD
        /// </summary>
        /// <param name="basecategoryid"></param>
        /// <param name="warehouseid"></param>
        /// <param name="selectmonth"></param>
        /// <returns></returns>
        //[Authorize]
        [Route("GetWarehouseDetailsReportWithPreMTD")]
        [HttpGet]
        public List<WarehouseReportDetails> GetWarehouseDetailsReportWithPreMTD(int basecategoryid, int warehouseid, DateTime selectmonth)
        {
            logger.Info("start Category: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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

                    var StartDate = new DateTime(selectmonth.Year, selectmonth.Month, 1);
                    var lastdate = StartDate.AddMonths(1).AddDays(-1);
                    var PreStartDate = StartDate.AddMonths(-1);
                    var PreLastDate = lastdate.AddMonths(-1);
                    List<WarehouseReportDetails> sub = new List<WarehouseReportDetails>();
                    var details = db.WarehouseReportDetailsDB.Where(x => x.TransactionMonth.Year == StartDate.Year && x.TransactionMonth.Month == StartDate.Month && x.BaseCategoryid == basecategoryid && x.Warehouseid == warehouseid).ToList();
                    if (details.Count > 0)
                    {
                        foreach (var item in details)
                        {
                            WarehouseReportDetails categor = new WarehouseReportDetails();
                            categor.BaseCategoryid = item.BaseCategoryid;
                            categor.Categoryid = item.Categoryid;
                            categor.Categoryname = item.Categoryname;
                            categor.Percentage = item.Percentage;
                            categor.PDCAAmount = item.PDCAAmount;
                            categor.Amount = item.Amount;
                            categor.Target = item.Target;
                            categor.TargetAmount = item.TargetAmount;
                            var sum = db.DbOrderDetails.Where(z => z.CategoryName == item.Categoryname && z.WarehouseId == item.Warehouseid && z.OrderDate > StartDate && z.OrderDate < lastdate).Select(y => (double?)y.TotalAmt).Sum() ?? 0;
                            categor.MTD = Convert.ToDecimal(Math.Round(sum, 2));
                            var preMTD = db.DbOrderDetails.Where(z => z.CategoryName == item.Categoryname && z.WarehouseId == item.Warehouseid && z.OrderDate > PreStartDate && z.OrderDate < PreLastDate).Select(y => (double?)y.TotalAmt).Sum() ?? 0;
                            categor.PreviousMTD = Convert.ToDecimal(Math.Round(preMTD, 2));
                            categor.Comment = item.Comment;
                            categor.PDCAAmount = item.PDCAAmount;
                            sub.Add(categor);
                        }
                    }

                    return sub;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// GetPercentage
        /// </summary>
        /// <param name="basecategoryid"></param>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        //[Authorize]     
        [Route("GetPercentage")]
        [HttpGet]
        public decimal GetPercentage(int basecategoryid, int warehouseid)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    decimal per = db.WarehouseReportDB.Where(x => x.BaseCategoryid == basecategoryid && x.Warehouseid == warehouseid).Select(y => y.Percentage).SingleOrDefault();
                    return per;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Post API for Insert AddWarehouseDetails
        /// </summary>
        /// <param name="Report"></param>
        /// <returns></returns>
        [Route("AddWarehouseDetails")]
        [HttpPost]
        public HttpResponseMessage AddWarehouseDetails(PDCADetails Report)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0; string username = null;

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
                        if (claim.Type == "username")
                        {
                            username = (claim.Value);
                        }
                    }

                    var getdata = db.WarehouseReportDetailsDB.Where(x => x.Warehouseid == Report.WarehouseId && x.BaseCategoryid == Report.BaseCategoryId && x.TransactionMonth.Month == Report.SelectMonth.Month).ToList();
                    if (getdata.Count == 0)
                    {
                        foreach (var item in Report.WarehouseReportDetailslist)
                        {
                            item.CreatedDate = indianTime;
                            item.UpdatedDate = indianTime;
                            item.CreatedBy = username;
                            item.CompanyID = compid;
                            item.Warehouseid = Report.WarehouseId;
                            item.Deleted = false;
                            item.IsActive = true;
                            item.TransactionMonth = indianTime;
                            item.PDCAAmount = Report.PDCAAmount;
                            db.WarehouseReportDetailsDB.Add(item);
                            db.Commit();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, "Records Inserted");
                    }
                    else
                    { return Request.CreateResponse(HttpStatusCode.OK, "Records Already Exist"); }
                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        /// <summary>
        /// Post API for update WarehouseTargetDetails
        /// </summary>
        /// <param name="Report"></param>
        /// <returns></returns>
        [Route("UpdateWarehouseDetails")]
        [HttpPost]
        public HttpResponseMessage UpdateTargetWarehouseDetails(PDCADetails Report)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    string username = null;
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
                        if (claim.Type == "username")
                        {
                            username = (claim.Value);
                        }
                    }

                    var getdata = db.WarehouseReportDetailsDB.Where(x => x.Warehouseid == Report.WarehouseId && x.BaseCategoryid == Report.BaseCategoryId && x.TransactionMonth.Month == Report.SelectMonth.Month).ToList();
                    if (getdata.Count > 0)
                    {
                        var check = db.WarehouseReportHistoryDetailsDB.Where(x => x.Warehouseid == Report.WarehouseId && x.BaseCategoryid == Report.BaseCategoryId && x.TransactionMonth.Month == Report.SelectMonth.Month).ToList();
                        if (check.Count == 0)
                        {
                            foreach (var item in getdata)
                            {
                                WarehouseReportHistoryDetails warehouseReportHistoryDetails = new WarehouseReportHistoryDetails();
                                warehouseReportHistoryDetails.CompanyID = item.CompanyID;
                                warehouseReportHistoryDetails.Warehouseid = item.Warehouseid;
                                warehouseReportHistoryDetails.BaseCategoryid = item.BaseCategoryid;
                                warehouseReportHistoryDetails.Categoryid = item.Categoryid;
                                warehouseReportHistoryDetails.Categoryname = item.Categoryname;
                                warehouseReportHistoryDetails.Percentage = item.Percentage;
                                warehouseReportHistoryDetails.Amount = item.Amount;
                                warehouseReportHistoryDetails.Target = item.Target;
                                warehouseReportHistoryDetails.TargetAmount = item.TargetAmount;
                                warehouseReportHistoryDetails.MTD = item.MTD;
                                warehouseReportHistoryDetails.TransactionMonth = item.TransactionMonth;
                                warehouseReportHistoryDetails.PDCAAmount = item.PDCAAmount;
                                warehouseReportHistoryDetails.Comment = item.Comment;
                                warehouseReportHistoryDetails.IsActive = item.IsActive;
                                warehouseReportHistoryDetails.Deleted = item.Deleted;
                                warehouseReportHistoryDetails.CreatedBy = item.CreatedBy;
                                warehouseReportHistoryDetails.UpdatedBy = username;
                                warehouseReportHistoryDetails.CreatedDate = indianTime;
                                warehouseReportHistoryDetails.UpdatedDate = indianTime;
                                db.WarehouseReportHistoryDetailsDB.Add(warehouseReportHistoryDetails);
                                db.Commit();
                            }
                        }
                        else
                        {
                            foreach (var item in check)
                            {
                                var update = Report.WarehouseReportDetailslist.Where(x => x.BaseCategoryid == item.BaseCategoryid && x.Categoryid == item.Categoryid).SingleOrDefault();
                                if (item.Target1 == 0 && item.TargetAmount1 == 0)
                                {
                                    item.Target1 = update.Target;
                                    item.TargetAmount1 = update.TargetAmount;
                                    item.Comment1 = update.Comment;
                                    db.Entry(item).State = EntityState.Modified;
                                    db.Commit();
                                }
                                else if (item.Target2 == 0 && item.TargetAmount2 == 0)
                                {
                                    item.Target2 = update.Target;
                                    item.TargetAmount2 = update.TargetAmount;
                                    item.Comment2 = update.Comment;
                                    db.Entry(item).State = EntityState.Modified;
                                    db.Commit();
                                }
                            }
                        }

                        foreach (var item in getdata)
                        {
                            var update = Report.WarehouseReportDetailslist.Where(x => x.BaseCategoryid == item.BaseCategoryid && x.Categoryid == item.Categoryid).SingleOrDefault();
                            item.Target = update.Target;
                            item.TargetAmount = update.TargetAmount;
                            item.Comment = update.Comment;
                            item.Updated = item.Updated + 1;
                            item.UpdatedDate = indianTime;
                            item.UpdatedBy = username;
                            db.Entry(item).State = EntityState.Modified;
                            db.Commit();
                        }
                    }
                    { return Request.CreateResponse(HttpStatusCode.OK, "Records Inserted"); }

                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }

            }
        }
        /// <summary>
        /// Get Warehouse History Report
        /// </summary>
        /// <param name="basecategoryid"></param>
        /// <param name="warehouseid"></param>
        /// <param name="selectmonth"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("GetAllHistoryData")]
        public List<WarehouseReportHistoryDetails> GetAllHistoryData(int basecategoryid, int warehouseid, DateTime selectmonth)
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
                using (AuthContext db = new AuthContext())
                {
                    var StartDate = new DateTime(selectmonth.Year, selectmonth.Month, 1);
                    List<WarehouseReportHistoryDetails> gr = db.WarehouseReportHistoryDetailsDB.Where(x => x.TransactionMonth.Year == StartDate.Year && x.TransactionMonth.Month == StartDate.Month && x.BaseCategoryid == basecategoryid && x.Warehouseid == warehouseid).ToList();
                    return gr;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region PDCABaseCategory
        /// <summary>
        /// GetBaseCategorydata 
        /// </summary>
        /// <param name="warehouseid"></param>
        /// <returns></returns>
        //[Authorize]
        [Route("GetBaseCategory")]
        [HttpGet]
        public IEnumerable<BaseCategoryData> Get(int warehouseid)
        {
            logger.Info("start Category: ");
            List<BaseCategoryData> ass = new List<BaseCategoryData>();
            using (var db = new AuthContext())
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

                    var basecat = db.WarehouseReportDB.Where(x => x.Warehouseid == warehouseid).ToList();
                    if (basecat.Count > 0)
                    {
                        foreach (var item in basecat)
                        {
                            BaseCategoryData bsd = new BaseCategoryData();
                            bsd.BaseCategoryId = item.BaseCategoryid;
                            bsd.BaseCategoryName = item.BaseCategoryName;
                            bsd.Percentage = item.Percentage;
                            bsd.WarehouseId = warehouseid;
                            bsd.CompanyId = compid;
                            ass.Add(bsd);
                        }
                    }
                    else
                    {
                        //var data = db.itemMasters.Where(x => x.active == true && x.Deleted == false && x.WarehouseId == warehouseid).Select(t => t.BaseCategoryid).Distinct().ToList();
                        //foreach (var item in data)

                        //{
                        //    var bacecategory = db.BaseCategoryDb.Where(x => x.BaseCategoryId == item).Select(y => y.BaseCategoryName).SingleOrDefault();
                        //    BaseCategoryData bsd = new BaseCategoryData();
                        //    bsd.BaseCategoryId = item;
                        //    bsd.BaseCategoryName = bacecategory;
                        //    bsd.Percentage = 0;
                        //    bsd.WarehouseId = warehouseid;
                        //    bsd.CompanyId = compid;
                        //    ass.Add(bsd);
                        //}

                        var data = db.BaseCategoryDb.Where(x => x.Deleted == false).ToList();
                        foreach (var item in data)
                        {
                            BaseCategoryData bsd = new BaseCategoryData();
                            bsd.BaseCategoryId = item.BaseCategoryId;
                            bsd.BaseCategoryName = item.BaseCategoryName;
                            bsd.Percentage = 0;
                            bsd.WarehouseId = warehouseid;
                            bsd.CompanyId = compid;
                            ass.Add(bsd);
                        }
                    }
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Post API for Insert AddWarehouseReport
        /// </summary>
        /// <param name="Report"></param>
        /// <returns></returns>
        [Route("AddPDCABaseCategory")]
        [HttpPost]
        public HttpResponseMessage AddPDCABaseCategory(PDCABaseCategory Report)
        {
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

                    var GetBasecategory = db.WarehouseReportDB.Where(x => x.Warehouseid == Report.WarehouseId).ToList();
                    if (GetBasecategory.Count == 0)
                    {
                        foreach (var item in Report.WarehouseReportlist)
                        {
                            item.CreatedDate = indianTime;
                            item.UpdatedDate = indianTime;
                            item.Warehouseid = Report.WarehouseId;
                            item.CompanyID = compid;
                            db.WarehouseReportDB.Add(item);
                            db.Commit();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, "Records Inserted");
                    }
                    else
                    {
                        foreach (var item in GetBasecategory)
                        {
                            var edit = Report.WarehouseReportlist.Where(x => x.BaseCategoryid == item.BaseCategoryid && x.Warehouseid == item.Warehouseid).SingleOrDefault();
                            item.UpdatedDate = indianTime;
                            item.Percentage = edit.Percentage;
                            db.Commit();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, "Records Updated");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        #endregion

        #region PDCA Category
        /// <summary>
        /// GetPDCACategory data
        /// </summary>
        /// <param name="warehouseid"></param>
        /// <param name="basecategoryid"></param>
        /// <returns></returns>
        [Authorize]
        [Route("GetPDCACategory")]
        [HttpGet]
        public IEnumerable<CategoryData> GetPDCACategory(int warehouseid, int basecategoryid)
        {
            logger.Info("start Category: ");
            List<CategoryData> ass = new List<CategoryData>();
            using (var db = new AuthContext())
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

                    var basecat = db.WarehouseReportCategoryDB.Where(x => x.Warehouseid == warehouseid && x.BaseCategoryid == basecategoryid).ToList();
                    if (basecat.Count > 0)
                    {
                        try
                        {

                            foreach (var item in basecat)
                            {
                                CategoryData bsd = new CategoryData();
                                bsd.BaseCategoryId = item.BaseCategoryid;
                                bsd.CategoryName = item.Categoryname;
                                bsd.CategoryId = item.Categoryid;
                                bsd.Percentage = item.Percentage;
                                bsd.WarehouseId = item.Warehouseid;
                                bsd.CompanyId = item.CompanyID;
                                ass.Add(bsd);
                            }
                        }
                        catch (Exception)
                        {

                            return null;
                        }
                    }
                    else
                    {
                        try
                        {

                            //var data = db.itemMasters.Where(x => x.active == true && x.Deleted == false && x.BaseCategoryid == basecategoryid && x.WarehouseId == warehouseid).Select(t => t.Categoryid).Distinct().ToList();
                            //foreach (var item in data)
                            //{
                            //    var categoryname = db.Categorys.Where(x => x.Categoryid == item).Select(y => y.CategoryName).SingleOrDefault();
                            //    CategoryData bsd = new CategoryData();
                            //    bsd.BaseCategoryId = basecategoryid;
                            //    bsd.CategoryId = item;
                            //    bsd.CategoryName = categoryname;
                            //    bsd.Percentage = 0;
                            //    bsd.WarehouseId = warehouseid;
                            //    bsd.CompanyId = compid;
                            //    ass.Add(bsd);
                            //}

                            var data = db.Categorys.Where(x => x.Deleted == false && x.BaseCategoryId == basecategoryid).ToList();
                            foreach (var item in data)
                            {
                                CategoryData bsd = new CategoryData();
                                bsd.BaseCategoryId = item.BaseCategoryId;
                                bsd.CategoryId = item.Categoryid;
                                bsd.CategoryName = item.CategoryName;
                                bsd.Percentage = 0;
                                bsd.WarehouseId = warehouseid;
                                bsd.CompanyId = compid;
                                ass.Add(bsd);
                            }
                        }
                        catch (Exception)
                        {

                            return null;
                        }
                    }
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Post API for add PDCACategory
        /// </summary>
        /// <param name="Report"></param>
        /// <returns></returns>
        [Authorize]
        [Route("AddPDCACategory")]
        [HttpPost]
        public HttpResponseMessage AddPDCACategory(PDCACategory Report)
        {
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

                    var GetBasecategory = db.WarehouseReportCategoryDB.Where(x => x.Warehouseid == Report.WarehouseId && x.BaseCategoryid == Report.BaseCategoryId).ToList();
                    if (GetBasecategory.Count == 0)
                    {
                        foreach (var item in Report.WarehouseReportCategorylist)
                        {
                            item.CreatedDate = indianTime;
                            item.UpdatedDate = indianTime;
                            item.Warehouseid = Report.WarehouseId;
                            item.CompanyID = compid;
                            db.WarehouseReportCategoryDB.Add(item);
                            db.Commit();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, "Records Inserted");
                    }
                    else
                    {
                        foreach (var item in GetBasecategory)
                        {
                            var edit = Report.WarehouseReportCategorylist.Where(x => x.BaseCategoryid == item.BaseCategoryid && x.Warehouseid == item.Warehouseid && x.Categoryid == item.Categoryid).SingleOrDefault();
                            item.UpdatedDate = indianTime;
                            item.Percentage = edit.Percentage;
                            db.Commit();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, "Records Updated");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }

        #endregion

        #region PDCACamparisonReport
        /// <summary>
        /// Get PDCA Camparison data
        /// </summary>
        /// <param name="month"></param>
        /// <param name="WarehouseIds"></param>
        /// <returns>List</returns>
        [Authorize]
        [Route("GetWarehouseComparisonData")]
        [HttpPost]
        public List<WarehoseCategoryDc> GetWarehouseComparisonData(DateTime month, List<int> WarehouseIds)
        {
            List<WarehoseCategoryDc> WarehoseCategoryDcs = new List<WarehoseCategoryDc>();

            logger.Info("start Category: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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

                    int warehousecount = WarehouseIds.Count;

                    WarehoseCategoryDcs = db.WarehouseReportDetailsDB.Where(x => x.TransactionMonth.Month == month.Month
                                                          && x.TransactionMonth.Year == month.Year
                                                          && WarehouseIds.Contains(x.Warehouseid))
                                          .GroupBy(x => new { x.Categoryid, x.Categoryname }, (key, group) =>
                                                               new WarehoseCategoryDc
                                                               {
                                                                   Categoryid = key.Categoryid,
                                                                   CategoryName = key.Categoryname,
                                                                   WarehoseDetailDcs = group.GroupBy(x => x.Warehouseid, (wkey, wgroup) => new WarehoseDetailDc
                                                                   {
                                                                       WareHouseid = wkey,
                                                                       MTD = wgroup.Sum(y => y.MTD),
                                                                       TargetAmount = wgroup.Sum(y => y.TargetAmount)
                                                                   }).ToList()
                                                               }).ToList();

                    return WarehoseCategoryDcs;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
        }

        [Authorize]
        [Route("ExportData")]
        [HttpGet]
        public List<ExportData> GetExportDataItemandHubWise(DateTime month)
        {
            List<ExportData> export = new List<ExportData>();

            logger.Info("start Category: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0; int Warehouse_id = 0;
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

                    var order = db.DbOrderDetails.Where(x => x.CreatedDate.Month == month.Month && x.CreatedDate.Year == month.Year).Select(x => x.itemNumber).Distinct();
                    var salesAmount = db.DbOrderDetails.Where(x => x.CreatedDate.Month == month.Month && x.CreatedDate.Year == month.Year).ToList();
                    var warehouse = new List<int>();
                    warehouse.Add(1); warehouse.Add(7); warehouse.Add(9); warehouse.Add(10); warehouse.Add(11); warehouse.Add(12);
                    foreach (var item in order)
                    {
                        ExportData ed = new ExportData();
                        var h1 = salesAmount.Where(x => x.itemNumber == item && x.WarehouseId == warehouse[0]).Select(x => x.TotalAmt).Sum();
                        var h2 = salesAmount.Where(x => x.itemNumber == item && x.WarehouseId == warehouse[1]).Select(x => x.TotalAmt).Sum();
                        var h3 = salesAmount.Where(x => x.itemNumber == item && x.WarehouseId == warehouse[2]).Select(x => x.TotalAmt).Sum();
                        var h4 = salesAmount.Where(x => x.itemNumber == item && x.WarehouseId == warehouse[3]).Select(x => x.TotalAmt).Sum();
                        var h5 = salesAmount.Where(x => x.itemNumber == item && x.WarehouseId == warehouse[4]).Select(x => x.TotalAmt).Sum();
                        var h6 = salesAmount.Where(x => x.itemNumber == item && x.WarehouseId == warehouse[5]).Select(x => x.TotalAmt).Sum();
                        var ItemName = salesAmount.Where(x => x.itemNumber == item).ToList();
                        ed.H1Indore = h1;
                        ed.H2Indore = h2;
                        ed.H1Bhopal = h3;
                        ed.H1Jaipur = h4;
                        ed.H2Jaipur = h5;
                        ed.H2Bhopal = h6;
                        ed.ItemName = ItemName.Where(x => x.itemNumber == item).Select(x => x.itemname).FirstOrDefault();
                        ed.BrandName = ItemName.Where(x => x.itemNumber == item).Select(x => x.SubcategoryName).FirstOrDefault();
                        ed.CategoryName = ItemName.Where(x => x.itemNumber == item).Select(x => x.CategoryName).FirstOrDefault();
                        export.Add(ed);
                    }

                    return export;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in BaseCategory " + ex.Message);
                    logger.Info("End  BaseCategory: ");
                    return null;
                }
            }
            #endregion
        }
    }
    #region DTO(Data transfer objects)
    public class BaseCategoryData
    {
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public int BaseCategoryId { get; set; }
        public string BaseCategoryName { get; set; }
        public decimal Percentage { get; set; }
    }
    public class CategoryData
    {
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public int BaseCategoryId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Percentage { get; set; }
    }
    public class WarehouseReportData
    {
        public int WarehouseID { get; set; }
        public int BaseCategoryid { get; set; }
        public string BaseCategoryName { get; set; }
        public decimal Percentage { get; set; }
        public List<Warehousecategory> category { get; set; }
    }
    public class PDCABaseCategory
    {
        public List<WarehouseReport> WarehouseReportlist { get; set; }
        public int WarehouseId { get; set; }
    }
    public class PDCACategory
    {
        public List<WarehouseReportCategory> WarehouseReportCategorylist { get; set; }
        public int WarehouseId { get; set; }
        public int BaseCategoryId { get; set; }
    }
    public class PDCADetails
    {
        public List<WarehouseReportDetails> WarehouseReportDetailslist { get; set; }
        public int WarehouseId { get; set; }
        public int BaseCategoryId { get; set; }
        public decimal PDCAAmount { get; set; }
        public DateTime SelectMonth { get; set; }
    }
    public class Warehousecategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }
    public class WarehouseDetails
    {
        public int BaseCategoryid { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public decimal Percentage { get; set; }
        public double MTD { get; set; }
    }
    public class GetWarehouseReport
    {
        public bool AddPermission { get; set; }
        public bool UpdatePermission { get; set; }
        public List<WarehouseReportDetails> warehousereportdetails { get; set; }
    }
    public class WarehoseCategoryDc
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public List<WarehoseDetailDc> WarehoseDetailDcs { get; set; }
    }
    public class WarehoseDetailDc
    {
        public int WareHouseid { get; set; }
        public string WareHouseName { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal MTD { get; set; }
    }
    public class ExportData
    {
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string ItemName { get; set; }
        public double H1Indore { get; set; }
        public double H2Indore { get; set; }
        public double H1Bhopal { get; set; }
        public double H1Jaipur { get; set; }
        public double H2Jaipur { get; set; }
        public double H2Bhopal { get; set; }
    }
}
#endregion