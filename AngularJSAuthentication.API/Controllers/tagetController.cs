using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/target")]
    public class tagetController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
      
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]
        //[Route("")]
        [HttpGet]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Get()
        {
            try
            {
                using (var db = new AuthContext())
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
                    if (Warehouse_id > 0)
                    {
                        var item = db.TargetDb.Where(x => x.WarehouseId == compid).AsEnumerable();
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        var item = db.TargetDb.Where(x => x.CompanyId == compid).AsEnumerable();
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in add feedBack " + ex.Message);
                logger.Info("End  addCity: ");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("")]
        [HttpPost]
        [AcceptVerbs("POST")]
        public HttpResponseMessage add(Target item)
        {
            logger.Info("start add RequestItem: ");
            try
            {
                using (var db = new AuthContext())
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
                    item.CompanyId = compid;
                    item.WarehouseId = Warehouse_id;
                    if (Warehouse_id > 0)
                    {
                        Target fdback = db.TargetDb.Where(f => f.name.Trim() == item.name.Trim() && f.WarehouseId == Warehouse_id).SingleOrDefault();
                        if (fdback == null)
                        {
                            item.createdDate = indianTime;
                            db.TargetDb.Add(item);
                            db.Commit();
                        }
                        else
                        {
                            fdback.name = item.name;
                            fdback.value = item.value;
                            fdback.monthValue = item.monthValue;
                            db.TargetDb.Attach(fdback);
                            db.Entry(fdback).State = EntityState.Modified;
                            db.Commit();
                        }
                        logger.Info("End add RequestItem: ");
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }

                    else
                    {
                        return null;

                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in add RequestItem " + ex.Message);
                logger.Info("End  RequestItem: ");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
        


        [Route("Report")]
        [HttpGet]
        [AcceptVerbs("GET")]
        public dynamic Get(string day, string skcode)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
                    int Warehouse_id = 1;

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
                    if (Warehouse_id > 0)
                    {
                        var sdate = indianTime.AddDays(-30).Date;
                        var today = indianTime;
                        if (day == "3 month")
                        {
                            sdate = indianTime.AddMonths(-3).Date;
                        }

                        var list = db.DbOrderMaster.Where(i => i.CreatedDate >= sdate && i.CreatedDate <= today && i.Skcode.Trim() == skcode.Trim()).ToList();

                        List<Target> uniqe = new List<Target>();
                        if (list.Count != 0)
                        {
                            foreach (var a in list)
                            {
                                Target l = uniqe.Where(x => x.createdDate.Date == a.CreatedDate.Date && x.status == a.Status).SingleOrDefault();
                                int i = 1;
                                if (l != null)
                                {
                                    l.TotalAmount = l.TotalAmount + a.TotalAmount;
                                    l.TotalOrder = l.TotalOrder + i++;
                                }
                                else
                                {
                                    Target b = new Target();

                                    b.TotalOrder = i++;
                                    b.status = a.Status;
                                    b.Comment = a.comments;
                                    b.name = a.ShopName;
                                    b.CompanyId = a.CompanyId;
                                    b.WarehouseId = a.WarehouseId;
                                    b.createdDate = a.CreatedDate.Date;
                                    b.day = a.CreatedDate.Day;
                                    b.month = a.CreatedDate.Month;
                                    b.year = a.CreatedDate.Year;
                                    b.TotalAmount = a.GrossAmount;
                                    uniqe.Add(b);
                                }
                            }
                        }


                        return uniqe;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in add feedBack " + ex.Message);
                logger.Info("End  addCity: ");
                return null;
            }
        }

        [Route("CustomerPurchaseData")]
        [HttpGet]
        [AcceptVerbs("GET")]
        public dynamic Get(string day, int CustomerId, int top)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 1, userid = 0;
                    int Warehouse_id = 1;

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
                    if (Warehouse_id > 0)
                    {
                        var sdate = indianTime.AddDays(-30).Date;
                        var today = indianTime;
                        if (day == "3 month")
                        {
                            sdate = indianTime.AddMonths(-3).Date;
                        }

                        var list = db.DbOrderDetails.Where(i => i.CreatedDate >= sdate && i.CreatedDate <= today && i.CustomerId == CustomerId).ToList();

                        List<OrderDetails> ODFilter = new List<OrderDetails>();

                        List<string> SubsubcategoryName = new List<string>();

                        foreach (var l1 in list)
                        {

                            if (l1.SubsubcategoryName != null && SubsubcategoryName.Any(x => x == l1.SubsubcategoryName))
                            {
                                var od = ODFilter.Where(x => x.SubsubcategoryName == l1.SubsubcategoryName).Select(x => x.TotalAmt).SingleOrDefault();
                                var cc = od;
                                double TotalAmt = Convert.ToDouble(cc);
                                TotalAmt = TotalAmt + l1.TotalAmt;
                                foreach (var asd in ODFilter)
                                {
                                    var odd = ODFilter.Where(x => x.SubsubcategoryName == l1.SubsubcategoryName).SingleOrDefault();
                                    if (odd != null)
                                    {
                                        odd.TotalAmt = TotalAmt;
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                            else
                            {

                                ODFilter.Add(l1);
                                SubsubcategoryName.Add(l1.SubsubcategoryName);
                            }


                        }


                        List<OrderDetailsDTM> OrderDetailMain = new List<OrderDetailsDTM>();
                        foreach (var item in ODFilter)
                        {
                            OrderDetailsDTM b = new OrderDetailsDTM();
                            b.status = item.Status;
                            b.SubsubcategoryName = item.SubsubcategoryName;
                            b.CategoryName = item.CategoryName;
                            b.CompanyId = item.CompanyId;
                            b.WarehouseId = item.WarehouseId;
                            b.CreatedDate = item.CreatedDate;
                            b.day = item.CreatedDate.Day;
                            b.month = item.CreatedDate.Month;
                            b.year = item.CreatedDate.Year;
                            b.TotalAmt = item.TotalAmt;
                            OrderDetailMain.Add(b);

                        }
                        return OrderDetailMain;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in add feedBack " + ex.Message);
                logger.Info("End  addCity: ");
                return null;
            }
        }
    }
    public class Target
    {
        [Key]
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public int? WarehouseId { get; set; }
        public string name { get; set; }

        public double value { get; set; }
        public double monthValue { get; set; }
        public DateTime createdDate { get; set; }
        [NotMapped]
        public double TotalAmount { get; set; }
        [NotMapped]
        public int day { get; set; }
        [NotMapped]
        public int month { get; set; }
        [NotMapped]
        public int year { get; set; }
        [NotMapped]
        public string status { get; set; }
        [NotMapped]
        public string Comment { get; set; }
        [NotMapped]
        public int? TotalOrder { get; set; }
    }
    public class OrderDetailsDTM
    {
        public int? CompanyId { get; set; }
        public int? WarehouseId { get; set; }
        public string name { get; set; }
        public double TotalAmt { get; set; }
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        public int day { get; set; }
        [NotMapped]
        public int month { get; set; }
        [NotMapped]
        public int year { get; set; }
        [NotMapped]
        public string status { get; set; }
        [NotMapped]
        public string SubsubcategoryName { get; set; }
        [NotMapped]
        public string CategoryName { get; set; }
    }
    public class ReportData
    {
        public int? id { get; set; }
        public string Type { get; set; }
        public double TotalAmount { get; set; }
    }
}