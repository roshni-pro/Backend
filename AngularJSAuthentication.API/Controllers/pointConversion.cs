using AngularJSAuthentication.API.Controllers.Base;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/pointConversion")]
    public class pointConversionController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
       
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //[Authorize]

        [Route("magin")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage GetRP()
        {
            RPConversion pointList = new RPConversion();
            using (var context = new AuthContext())
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

                    //if (Warehouse_id > 0)
                    //{

                    //    pointList = context.RPConversionDb.Where(x=>x.Warehouseid== Warehouse_id).FirstOrDefault();
                    //    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    //}
                    //else {
                    //pointList = context.RPConversionDb.Where(x => x.CompanyId == compid).FirstOrDefault();
                    pointList = context.RPConversionDb.FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    // }


                }
                catch (Exception ex)
                {
                    logger.Error("Error in conversion " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        [Route("promo")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage GetPromo()
        {
            promoPurConv pointList = new promoPurConv();
            using (var context = new AuthContext())
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

                    //if (compid > 0)
                    //{
                    pointList = context.promoPurConvDb.FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    //}
                    //else{
                    //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                    //}

                }
                catch (Exception ex)
                {
                    logger.Error("Error in conversion " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        [Route("milestone")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage GetMP()
        {
            List<MilestonePoint> pointList = new List<MilestonePoint>();
            using (var context = new AuthContext())
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
                    pointList = context.MilestonePointDb.Where(m => m.active == true).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in conversion " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        [Route("milestonbackend")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage GetMilestone()
        {
            //MilestonePoint pointList = new MilestonePoint();
            List<MilestonePoint> pointList = new List<MilestonePoint>();
            using (var context = new AuthContext())
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

                    //if (Warehouse_id > 0)
                    //{
                    pointList = context.MilestonePointDb.ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    //}
                    //else {
                    //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                    //}


                }
                catch (Exception ex)
                {
                    logger.Error("Error in conversion " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        [Route("share")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage Getshare(int id)
        {
            RetailerShare pointList = new RetailerShare();
            using (var context = new AuthContext())
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
                    pointList = context.RetailerShareDb.Where(c => c.cityid == id).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in conversion " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        [Route("shareAll")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage GetAllshare()
        {
            List<RetailerShare> pointList = new List<RetailerShare>();
            using (var context = new AuthContext())
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
                    //if (Warehouse_id > 0)
                    //{
                    //    pointList = context.RetailerShareDb.Where(x=>x.Warehouseid == Warehouse_id).ToList();
                    //    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    //}
                    //else {

                    pointList = context.RetailerShareDb.Where(x => x.CompanyId == compid).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    //}

                }
                catch (Exception ex)
                {
                    logger.Error("Error in conversion " + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        [Route("magin")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage postRP(RPConversion point)
        {
            using (var context = new AuthContext())
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
                    // point.Warehouseid = Warehouse_id;
                    point.CompanyId = compid;
                    //if (Warehouse_id > 0)
                    //{
                    if (point.Id > 0) { }
                    else
                        point.Id = 0;
                    var rpoint = context.RPConversionDb.Where(c => c.Id == point.Id).SingleOrDefault();
                    if (rpoint != null)
                    {
                        rpoint.point = point.point;
                        rpoint.rupee = point.rupee;

                        context.RPConversionDb.Attach(rpoint);
                        context.Entry(rpoint).State = EntityState.Modified;
                        context.Commit();
                    }
                    else
                    {
                        context.RPConversionDb.Add(point);
                        context.Commit();
                        rpoint = point;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, rpoint);
                    //}
                    //else {

                    //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                    //}

                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }

        }
        [Route("promo")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage postPromo(promoPurConv point)
        {
            using (var context = new AuthContext())
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

                    //point.Warehouseid = Warehouse_id;
                    //point.CompanyId = compid;
                    if (point.Id > 0) { }
                    else
                        point.Id = 0;
                    var rpoint = context.promoPurConvDb.Where(c => c.Id == point.Id).SingleOrDefault();
                    if (rpoint != null)
                    {
                        rpoint.point = point.point;
                        rpoint.rupee = point.rupee;

                        context.promoPurConvDb.Attach(rpoint);
                        context.Entry(rpoint).State = EntityState.Modified;
                        context.Commit();
                    }
                    else
                    {
                        context.promoPurConvDb.Add(point);
                        context.Commit();
                        rpoint = point;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, rpoint);
                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }
        [Route("milestone")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage postMP(MilestonePoint point)
        {
            using (var context = new AuthContext())
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
                    //point.CompanyId = compid;
                    //point.Warehouseid = Warehouse_id;
                    //if (Warehouse_id > 0)
                    //{
                    if (point.M_Id > 0) { }
                    else
                        point.M_Id = 0;
                    var rpoint = context.MilestonePointDb.Where(c => c.M_Id == point.M_Id).SingleOrDefault();
                    if (rpoint != null)
                    {
                        rpoint.rPoint = point.rPoint;
                        rpoint.mPoint = point.mPoint;
                        rpoint.active = point.active;
                        context.MilestonePointDb.Attach(rpoint);
                        context.Entry(rpoint).State = EntityState.Modified;
                        context.Commit();
                    }
                    else
                    {
                        context.MilestonePointDb.Add(point);
                        context.Commit();
                        rpoint = point;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, rpoint);
                    // }
                    //else {
                    //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                    //}

                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                }
            }
        }
        [Route("share")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage postShare(RetailerShare point)
        {
            using (var context = new AuthContext())
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
                    ///  point.Warehouseid = Warehouse_id;
                    point.CompanyId = compid;
                    //if (Warehouse_id > 0)
                    //{
                    var city = context.Cities.Where(c => c.Cityid == point.cityid).FirstOrDefault();
                    var rpoint = context.RetailerShareDb.Where(c => c.cityid == point.cityid).SingleOrDefault();
                    if (rpoint != null)
                    {
                        rpoint.cityid = city.Cityid;
                        rpoint.cityName = city.CityName;
                        rpoint.share = point.share;

                        context.RetailerShareDb.Attach(rpoint);
                        context.Entry(rpoint).State = EntityState.Modified;
                        context.Commit();
                    }
                    else
                    {
                        point.cityName = city.CityName;
                        context.RetailerShareDb.Add(point);
                        context.Commit();
                        rpoint = point;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, rpoint);
                    //}
                    //else {
                    //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                    //}

                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }

        [Route("promopurchase")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage getPromoPurchase(string SupplierCode)
        {
            using (var context = new AuthContext())
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

                    if (Warehouse_id > 0)
                    {
                        supplierPoint pointList = new supplierPoint();
                        try
                        {
                            pointList = context.supplierPointDb.Where(c => c.SupplierCode == SupplierCode && c.WarehouseId == Warehouse_id).SingleOrDefault();
                            return Request.CreateResponse(HttpStatusCode.OK, pointList);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error in conversion " + ex.Message);
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                        }
                    }
                    else
                    {
                        return null;

                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }

        [Route("promopur")]
        [AcceptVerbs("Get")]
        public HttpResponseMessage getPromo(string SupplierCode)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<supplierPoint> pointList = new List<supplierPoint>();
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

                        try
                        {
                            if (SupplierCode != "" && SupplierCode != null && SupplierCode != "null")
                                pointList = context.supplierPointDb.Where(x => x.SupplierCode == SupplierCode && x.WarehouseId == Warehouse_id).ToList();
                            else
                                pointList = context.supplierPointDb.Where(x => x.WarehouseId == Warehouse_id).ToList();
                            return Request.CreateResponse(HttpStatusCode.OK, pointList);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error in conversion " + ex.Message);
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error");
                        }
                    }


                    else
                    {
                        if (SupplierCode != "" && SupplierCode != null && SupplierCode != "null")
                            pointList = context.supplierPointDb.Where(x => x.SupplierCode == SupplierCode && x.CompanyId == compid).ToList();
                        else
                            pointList = context.supplierPointDb.Where(x => x.CompanyId == compid).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, pointList);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }

        [Route("promopurchase")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage postPromoPurchase(supplierPoint point)
        {
            using (var context = new AuthContext())
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
                    point.WarehouseId = Warehouse_id;
                    point.CompanyId = compid;
                    if (Warehouse_id > 0)
                    {
                        var supp = context.Suppliers.Where(c => c.SUPPLIERCODES == point.SupplierCode).FirstOrDefault();
                        var rpoint = context.supplierPointDb.Where(c => c.SupplierCode == point.SupplierCode).SingleOrDefault();
                        if (rpoint != null)
                        {
                            rpoint.SupplierName = supp.Name;
                            rpoint.Amount += point.Amount;
                            rpoint.Point += point.Point;
                            point.confirm = false;

                            context.supplierPointDb.Attach(rpoint);
                            context.Entry(rpoint).State = EntityState.Modified;
                            context.Commit();
                        }
                        else
                        {
                            point.SupplierName = supp.Name;
                            point.confirm = false;
                            context.supplierPointDb.Add(point);
                            context.Commit();
                            rpoint = point;
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, rpoint);


                    }
                    else
                    {
                        var supp = context.Suppliers.Where(c => c.SUPPLIERCODES == point.SupplierCode).FirstOrDefault();
                        var rpoint = context.supplierPointDb.Where(c => c.SupplierCode == point.SupplierCode).SingleOrDefault();
                        if (rpoint != null)
                        {
                            rpoint.SupplierName = supp.Name;
                            rpoint.Amount += point.Amount;
                            rpoint.Point += point.Point;
                            point.confirm = false;

                            context.supplierPointDb.Attach(rpoint);
                            context.Entry(rpoint).State = EntityState.Modified;
                            context.Commit();
                        }
                        else
                        {
                            point.SupplierName = supp.Name;
                            point.confirm = false;
                            context.supplierPointDb.Add(point);
                            context.Commit();
                            rpoint = point;
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, rpoint);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }
        [Route("comfirm")]
        [AcceptVerbs("Post")]
        public HttpResponseMessage postConfirm(supplierPoint point)
        {
            using (var context = new AuthContext())
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
                    point.WarehouseId = Warehouse_id;
                    point.CompanyId = compid;
                    if (Warehouse_id > 0)
                    {
                        var rpoint = context.supplierPointDb.Where(c => c.SupplierCode == point.SupplierCode).SingleOrDefault();
                        if (rpoint != null)
                        {
                            rpoint.Amount = 0;
                            if (rpoint.PromoPoint == null)
                            {
                                rpoint.PromoPoint = 0;
                            }
                            if (rpoint.UsedPoint == null)
                            {
                                rpoint.UsedPoint = 0;
                            }

                            rpoint.PromoPoint += point.Point;
                            rpoint.Point = 0;
                            point.confirm = true;

                            context.supplierPointDb.Attach(rpoint);
                            context.Entry(rpoint).State = EntityState.Modified;
                            context.Commit();
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, rpoint);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error" + ex.Message);
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Got Error"); ;
                }
            }
        }
        public class supplierPoint
        {
            [Key]
            public int id { get; set; }
            public int? CompanyId { get; set; }
            public int? WarehouseId { get; set; }
            public string SupplierCode { get; set; }
            public string SupplierName { get; set; }
            public int? PromoPoint { get; set; }
            public int? Point { get; set; }
            public int? UsedPoint { get; set; }
            public double Amount { get; set; }
            public bool confirm { get; set; }
        }
        public class promoPurConv
        {
            [Key]
            public int Id { get; set; }
            public double point { get; set; }
            public double rupee { get; set; }
        }
    }
}
