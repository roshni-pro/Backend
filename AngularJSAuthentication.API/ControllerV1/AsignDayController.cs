using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
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
using static AngularJSAuthentication.API.Controllers.CustomersController;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AsignDay")]
    public class AsignDayController : BaseAuthController
    {
       
        //public static Logger logger = LogManager.GetCurrentClassLogger();
        //[Route("")]

        //public List<People> Get()
        //{
        //    logger.Info("start get all Sales Executive: ");
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            int Warehouse_id = 0;
        //            List<People> displist = new List<People>();
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

        //            }
        //            if (Warehouse_id > 0)
        //            {
        //                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouse_id + " and p.CompanyId=" + compid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                displist = db.Database.SqlQuery<People>(query).ToList();
        //                //displist = db.Peoples.Where(x => x.Department == "Sales Executive" && x.CompanyId == compid && x.WarehouseId == Warehouse_id).ToList();
        //                logger.Info("End  Sales Executive: ");
        //                return displist;

        //            }
        //            else
        //            {
        //                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //                string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.CompanyId=" + compid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                displist = db.Database.SqlQuery<People>(query).ToList();
        //                // displist = db.Peoples.Where(x => x.Department == "Sales Executive" && x.CompanyId == compid).ToList();
        //                logger.Info("End  Sales Executive: ");
        //                return displist;
        //            }


        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in getall Sales Executive " + ex.Message);
        //            logger.Info("End getall Sales Executive: ");
        //            return null;
        //        }
        //    }
        //}
        #region Get all Active Agent 
        /// <summary>
        /// Created Date 19/04/2019
        /// Created By Raj
        /// 
        /// </summary>
        /// <returns>displist</returns>
        [Route("Activesalesexe")]
        public List<People> GetActivesalesAgent(int WarehouseId)
        {
            logger.Info("start get all Sales Executive: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;
                    List<People> displist = new List<People>();
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
                    Warehouse_id = WarehouseId;
                    if (Warehouse_id > 0)
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.WarehouseId=" + Warehouse_id + " and p.CompanyId=" + compid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        displist = db.Database.SqlQuery<People>(query).ToList();
                        //displist = db.Peoples.Where(x => x.Department == "Sales Executive" && x.Active == true && x.CompanyId == compid && x.WarehouseId == Warehouse_id).ToList();
                        logger.Info("End  Sales Executive: ");
                        return displist;

                    }
                    else
                    {
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        string query = "select distinct p.* from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.CompanyId=" + compid + " and r.Name='Sales Executive' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                        displist = db.Database.SqlQuery<People>(query).ToList();
                        //displist = db.Peoples.Where(x => x.Department == "Sales Executive" && x.Active == true && x.CompanyId == compid).ToList();
                        logger.Info("End  Sales Executive: ");
                        return displist;
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall Sales Executive " + ex.Message);
                    logger.Info("End getall Sales Executive: ");
                    return null;
                }
            }
        }
        #endregion

        //[Route("search")]
        //public List<Customer> Get(string key)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            List<People> displist = new List<People>();
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

        //            List<Customer> ass = db.Customers.Where(t => (t.ShopName.Contains(key) || t.Skcode.Contains(key)) && t.Active == true).ToList();
        //            return ass;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in getall Sales Executive " + ex.Message);
        //            logger.Info("End getall Sales Executive: ");
        //            return null;
        //        }
        //    }
        //}
        //public class setday
        //{
        //    public List<Customer> clist { get; set; }
        //}
        //[Route("addBeat")]
        //[AcceptVerbs("POST")]
        //public SalesPersonBeat POST(SalesPersonBeat obj)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            List<Customer> displist = new List<Customer>();
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
        //            //obj.CompanyId = compid;

        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //            return db.Addsalesbeat(obj);
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }
        //}

        //[Route("customer")]
        //public HttpResponseMessage Get(int id, string day)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {


        //            if (day == "undefined" || day == null)
        //            {

        //                var data = (from j in db.Customers
        //                            where j.ExecutiveId == id
        //                            join i in db.Customers on j.CustomerId equals i.CustomerId
        //                            join k in db.Warehouses on j.Warehouseid equals k.WarehouseId
        //                            select new SalespDTO
        //                            {

        //                                CustomerId = j.CustomerId,
        //                                CompanyId = j.CompanyId,
        //                                Active = i.Active,
        //                                City = i.City,
        //                                WarehouseId = j.Warehouseid,
        //                                WarehouseName = k.WarehouseName,
        //                                lat = i.lat,
        //                                lg = i.lg,
        //                                ExecutiveId = j.ExecutiveId,
        //                                BeatNumber = j.BeatNumber,
        //                                Day = j.Day,
        //                                Skcode = i.Skcode,
        //                                Mobile = i.Mobile,
        //                                ShopName = i.ShopName,
        //                                BillingAddress = i.BillingAddress,
        //                                Name = i.Name,
        //                                Emailid = i.Emailid,
        //                                RefNo = i.RefNo,
        //                                Password = i.Password,
        //                                UploadRegistration = i.UploadRegistration,
        //                                ResidenceAddressProof = i.ResidenceAddressProof,
        //                                DOB = i.DOB
        //                            }).OrderBy(x => x.CustomerId).ToList();
        //                return Request.CreateResponse(HttpStatusCode.OK, data);
        //            }
        //            else
        //            {
        //                var data = (from j in db.Customers
        //                            where j.ExecutiveId == id && j.Day == day
        //                            join i in db.Customers on j.CustomerId equals i.CustomerId
        //                            join k in db.Warehouses on j.Warehouseid equals k.WarehouseId
        //                            select new SalespDTO
        //                            {

        //                                CustomerId = j.CustomerId,
        //                                CompanyId = j.CompanyId,
        //                                Active = i.Active,
        //                                City = i.City,
        //                                WarehouseId = j.Warehouseid,
        //                                WarehouseName = k.WarehouseName,
        //                                ExecutiveId = j.ExecutiveId,
        //                                BeatNumber = j.BeatNumber,
        //                                lat = i.lat,
        //                                lg = i.lg,
        //                                Day = j.Day,
        //                                Skcode = i.Skcode,
        //                                Mobile = i.Mobile,
        //                                ShopName = i.ShopName,
        //                                BillingAddress = i.BillingAddress,
        //                                Name = i.Name,
        //                                Emailid = i.Emailid,
        //                                RefNo = i.RefNo,
        //                                Password = i.Password,
        //                                UploadRegistration = i.UploadRegistration,
        //                                ResidenceAddressProof = i.ResidenceAddressProof,
        //                                DOB = i.DOB
        //                            }).OrderBy(x => x.CustomerId).ToList();
        //                return Request.CreateResponse(HttpStatusCode.OK, data);
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        //        }
        //    }
        //}



        //[Route("customer/V2")]
        //public HttpResponseMessage GetBeatDataV2(int id, string day)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            GlobalcustomerDetail obj = new GlobalcustomerDetail();



        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            int Warehouse_id = 0;
        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
        //                Warehouse_id = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

        //            var monthlycustomerlevel = new MongoDbHelper<MonthlyCustomerLevel>();

        //            var customerlevel = new MonthlyCustomerLevel();


        //            customerlevel = monthlycustomerlevel.Select(x => x.Month == DateTime.Now.Month && x.Year == DateTime.Now.Year).FirstOrDefault();

        //            var levels = customerlevel?.CustomerLevels;

        //            var people = db.Peoples.Where(x => x.PeopleID == id).SingleOrDefault();
        //            List<SalespDTO> data = new List<SalespDTO>();
        //            if (day == "undefined" || day == null)
        //            {


        //                if (people != null)
        //                {
        //                    data = (from i in db.Customers
        //                            where i.ExecutiveId == id && i.Deleted == false && i.Active == true && i.IsHide == false && i.Warehouseid == people.WarehouseId
        //                            join k in db.Warehouses on i.Warehouseid equals k.WarehouseId
        //                            select new SalespDTO
        //                            {

        //                                CustomerId = i.CustomerId,
        //                                CompanyId = i.CompanyId,
        //                                Active = i.Active,
        //                                City = i.City,
        //                                WarehouseId = i.Warehouseid,
        //                                WarehouseName = k.WarehouseName,
        //                                lat = i.lat,
        //                                lg = i.lg,
        //                                ExecutiveId = i.ExecutiveId,
        //                                BeatNumber = i.BeatNumber,
        //                                Day = i.Day,
        //                                Skcode = i.Skcode,
        //                                Mobile = i.Mobile,
        //                                ShopName = i.ShopName,
        //                                BillingAddress = i.BillingAddress,
        //                                Name = i.Name,
        //                                Emailid = i.Emailid,
        //                                RefNo = i.RefNo,
        //                                Password = i.Password,
        //                                UploadRegistration = i.UploadRegistration,
        //                                ResidenceAddressProof = i.ResidenceAddressProof,
        //                                DOB = i.DOB
        //                            }).OrderBy(x => x.CustomerId).ToList();
        //                }
        //                if (data.Count() > 0)
        //                {
        //                    if (levels != null && levels.Any())
        //                    {
        //                        foreach (var item in data)
        //                        {

        //                            var leveldata = levels.Where(x => x.SKCode == item.Skcode).Select(x => new { LevelName = x.LevelName, ColourCode = x.ColourCode }).FirstOrDefault();
        //                            if (leveldata != null)
        //                            {
        //                                //item.CustomerLevel = leveldata.LevelName;
        //                                item.ColourCode = leveldata.ColourCode;
        //                            }
        //                        }
        //                    }
        //                    obj = new GlobalcustomerDetail()
        //                    {
        //                        customers = data,
        //                        Status = true,
        //                        Message = "Customer Found"
        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, obj);
        //                }
        //                else
        //                {
        //                    obj = new GlobalcustomerDetail()
        //                    {
        //                        customers = data,
        //                        Status = false,
        //                        Message = "No Customer found"
        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, obj);
        //                }


        //            }
        //            else
        //            {

        //                if (people != null)
        //                {

        //                    data = (from i in db.Customers
        //                            where i.ExecutiveId == id && i.Day == day && i.Deleted == false && i.Active == true && i.IsHide == false && i.Warehouseid == people.WarehouseId
        //                            join k in db.Warehouses on i.Warehouseid equals k.WarehouseId
        //                            select new SalespDTO
        //                            {

        //                                CustomerId = i.CustomerId,
        //                                CompanyId = i.CompanyId,
        //                                Active = i.Active,
        //                                City = i.City,
        //                                WarehouseId = i.Warehouseid,
        //                                WarehouseName = k.WarehouseName,
        //                                ExecutiveId = i.ExecutiveId,
        //                                BeatNumber = i.BeatNumber,
        //                                lat = i.lat,
        //                                lg = i.lg,
        //                                Day = i.Day,
        //                                Skcode = i.Skcode,
        //                                Mobile = i.Mobile,
        //                                ShopName = i.ShopName,
        //                                BillingAddress = i.BillingAddress,
        //                                Name = i.Name,
        //                                Emailid = i.Emailid,
        //                                RefNo = i.RefNo,
        //                                Password = i.Password,
        //                                UploadRegistration = i.UploadRegistration,
        //                                ResidenceAddressProof = i.ResidenceAddressProof,
        //                                DOB = i.DOB
        //                            }).OrderBy(x => x.CustomerId).ToList();
        //                }
        //                if (data.Count() > 0)
        //                {
        //                    if (levels != null && levels.Any())
        //                    {
        //                        foreach (var item in data)
        //                        {

        //                            var leveldata = levels.Where(x => x.SKCode == item.Skcode).Select(x => new { LevelName = x.LevelName, ColourCode = x.ColourCode }).FirstOrDefault();
        //                            if (leveldata != null)
        //                            {
        //                                //item.CustomerLevel = leveldata.LevelName;
        //                                item.ColourCode = leveldata.ColourCode;
        //                            }
        //                        }
        //                    }
        //                    obj = new GlobalcustomerDetail()
        //                    {
        //                        customers = data,
        //                        Status = true,
        //                        Message = "Customer Found"
        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, obj);
        //                }
        //                else
        //                {
        //                    obj = new GlobalcustomerDetail()
        //                    {
        //                        customers = data,
        //                        Status = false,
        //                        Message = "No Customer found"
        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, obj);
        //                }
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        //        }
        //    }
        //}


        //[Route("")]
        //[AcceptVerbs("PUT")]
        //public HttpResponseMessage PUT(setday itemlist)  // Asign orders
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;
        //            int Warehouse_id = 0;
        //            List<CustSupplier> displist = new List<CustSupplier>();
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
        //            }
        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);



        //            foreach (var item in itemlist.clist)
        //            {
        //                Customer asss = db.Customers.Where(b => b.CustomerId == item.CustomerId && b.Warehouseid == Warehouse_id).FirstOrDefault();
        //                if (asss != null)
        //                {
        //                    try
        //                    {
        //                        asss.Day = item.Day;
        //                        asss.BeatNumber = item.BeatNumber;
        //                        asss.UpdatedDate = DateTime.Now;
        //                        db.Entry(asss).State = EntityState.Modified;
        //                        db.Commit();

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        return null;
        //                    }
        //                }
        //                else
        //                {
        //                }

        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, itemlist);
        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //        }
        //    }
        //}
    }

    
}
