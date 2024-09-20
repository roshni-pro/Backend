using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.DataContracts.Masters.VehicleMaster;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.Infrastructure;
using AngularJSAuthentication.Model;
using LinqKit;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;


namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/DBoyMaster")]
    [AllowAnonymous]
    public class DBoyMasterController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("PostDBoyMaster")]
        [HttpPost]
        public async Task<ResDBoyMaster> AddDBoy(DboyMaster obj)
        {
            ResDBoyMaster res = new ResDBoyMaster();

            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var IsDuplicateMob = context.DboyMasters.FirstOrDefault(x => x.Id != obj.Id && x.IsDeleted == false && x.MobileNo == obj.MobileNo);
                if (IsDuplicateMob != null)
                {
                    res.msg = "Mobile No. Already Exist";
                    res.Result = false;
                    return res;
                }

                var DboyData = context.DboyMasters.FirstOrDefault(x => x.Id == obj.Id && x.IsDeleted == false);

                if (DboyData != null)
                {
                    DboyData.Name = obj.Name;
                    DboyData.Address = obj.Address;
                    DboyData.MobileNo = obj.MobileNo;
                    DboyData.AadharNo = obj.AadharNo;
                    DboyData.AadharCopy = obj.AadharCopy;
                    DboyData.AadharCopyBack = obj.AadharCopyBack;
                    DboyData.Photo = obj.Photo;
                    DboyData.Type = obj.Type;
                    DboyData.AgencyName = obj.AgencyName;
                    DboyData.AgentOrTransport = obj.AgentOrTransport;
                    DboyData.ValidFrom = obj.ValidFrom;
                    DboyData.ValidTill = obj.ValidTill;
                    DboyData.IsActive = obj.IsActive;
                    DboyData.IsDeleted = false;
                    DboyData.IsBlocked = obj.IsBlocked;
                    DboyData.ModifiedBy = userid;
                    DboyData.ModifiedDate = indianTime;
                    DboyData.CityId = obj.CityId;
                    DboyData.AgentId = obj.AgentId;
                    DboyData.WarehouseId = obj.WarehouseId;
                    DboyData.DboyCost = obj.DboyCost;
                    DboyData.VehicleMasterId = obj.VehicleMasterId;
                    //DboyData.TripTypeEnum = obj.TripTypeEnum;
                    context.Entry(DboyData).State = EntityState.Modified;

                    var Peoles = context.Peoples.Where(x => x.PeopleID == DboyData.PeopleId).FirstOrDefault();
                    if (Peoles != null)
                    {
                        Peoles.PeopleFirstName = obj.Name;
                        Peoles.DisplayName = obj.Name;
                        Peoles.Mobile = obj.MobileNo;
                        context.Entry(Peoles).State = EntityState.Modified;
                    }
                    if (context.Commit() > 0)
                    {
                        res.msg = "Successfully Saved";
                        res.Result = true;
                    }
                    //else
                    //{
                    //    var IsDuplicateBlock = context.DboyMasters.FirstOrDefault(x => x.IsDeleted == false && x.IsBlocked == true && (x.MobileNo == obj.MobileNo || x.AadharNo == obj.AadharNo));
                    //    if (IsDuplicateBlock != null)
                    //    {
                    //        res.msg = "your number/aadhar is blocked";
                    //        res.Result = false;
                    //        return res;
                    //    }
                    //    var IsDuplicate = context.DboyMasters.FirstOrDefault(x => x.IsDeleted == false && x.IsBlocked == false && (x.MobileNo == obj.MobileNo || x.AadharNo == obj.AadharNo));
                    //    if (IsDuplicate != null)
                    //    {
                    //        res.msg = "Dboy Already Registerd";
                    //        res.Result = false;
                    //        return res;
                    //    }

                    //    DboyData = new DboyMaster
                    //    {
                    //        Name = obj.Name,
                    //        Address = obj.Address,
                    //        MobileNo = obj.MobileNo,
                    //        AadharNo = obj.AadharNo,
                    //        AadharCopy = obj.AadharCopy,
                    //        AadharCopyBack = obj.AadharCopyBack,
                    //        Photo = obj.Photo,
                    //        Type = obj.Type,
                    //        AgencyName = obj.AgencyName,
                    //        AgentOrTransport = obj.AgentOrTransport,
                    //        ValidFrom = obj.ValidFrom,
                    //        ValidTill = obj.ValidTill,
                    //        IsActive = obj.IsActive,
                    //        IsDeleted = false,
                    //        IsBlocked = obj.IsBlocked,
                    //        CreatedBy = userid,
                    //        CreatedDate = indianTime,
                    //        CityId = obj.CityId,
                    //        AgentId = obj.AgentId,
                    //        WarehouseId = obj.WarehouseId

                    //    };
                    //    var PeopleId =await CreatePeopleOfDboy(DboyData, context);
                    //    if (PeopleId > 0)
                    //    {
                    //        DboyData.PeopleId = PeopleId ?? 0;
                    //        context.DboyMasters.Add(DboyData);
                    //        if (context.Commit() > 0)
                    //        {
                    //            res.msg = "Successfully Saved";
                    //            res.Result = true;
                    //            return res;
                    //        }
                    //        else
                    //        {
                    //            res.msg = "Failed";
                    //            res.Result = false;
                    //            return res;
                    //        }
                    //    }
                    //}
                }
                return res;
            }
        }

        [Route("DboyActiveDeactiveList")]
        [HttpGet]
        public bool ActiveDeactiveList(int Id, bool IsActive)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var dboy = db.DboyMasters.Where(x => x.Id == Id).FirstOrDefault();
                if (dboy != null)
                {
                    var param = new SqlParameter("peopleId", dboy.PeopleId);
                    int result = new int();
                    result = db.Database.SqlQuery<int>("exec RoleAssignCountForDBoy @peopleId", param).FirstOrDefault();

                    if ((result == 0 && IsActive == false) || IsActive== true)
                    {
                        bool IsUpdate = ActiveInactivePeople(db, dboy.PeopleId, IsActive, userid);
                    }
                    //bool IsUpdate = ActiveInactivePeople(db, dboy.PeopleId, IsActive, userid);
                    dboy.IsActive = IsActive;
                    dboy.ModifiedBy = userid;
                    dboy.ModifiedDate = DateTime.Now;
                    db.Entry(dboy).State = EntityState.Modified;

                    AddRemoveDboyBoy(IsActive, dboy.PeopleId);
                    return db.Commit() > 0;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool AddRemoveDboyBoy(bool IsAdd, int PeopleId)
        {
            using (var db = new AuthContext())
            {
                var peopleParam = new SqlParameter("peopleId", PeopleId);
                var isAddParam = new SqlParameter("IsAdd", IsAdd);

                int result = new int();
                result = db.Database.ExecuteSqlCommand("exec AddRemoveDeliveryBoyRole @peopleId, @IsAdd", peopleParam, isAddParam);
                db.Commit();
            }

            return true;
        }


        [Route("DboyBlockById")]
        [HttpGet]
        [AllowAnonymous]
        public bool DboyBlockById(int Id, bool IsBlocked)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var db = new AuthContext())
            {
                var dboy = db.DboyMasters.Where(x => x.Id == Id).FirstOrDefault();
                if (dboy != null)
                {
                    if (IsBlocked)
                    {
                        bool IsUpdate = ActiveInactivePeople(db, dboy.PeopleId, IsBlocked, userid);
                    }
                    dboy.ModifiedBy = userid;
                    dboy.ModifiedDate = DateTime.Now;
                    dboy.IsActive = IsBlocked;
                    dboy.IsBlocked = IsBlocked;
                    db.Entry(dboy).State = EntityState.Modified;

                    return db.Commit() > 0;
                }
                else
                {
                    return false;
                }
            }
        }
        [Route("GetDBoyMaster")]
        [HttpGet]
        public ResDBoyMaster DBoyMasterList()
        {
            ResDBoyMaster res = new ResDBoyMaster();
            try
            {
                List<DboyMaster> DBoyMasterList = new List<DboyMaster>();
                List<DBoyMasterDc> List = new List<DBoyMasterDc>();
                using (var context = new AuthContext())
                {
                    DBoyMasterList = context.DboyMasters.Where(x => x.IsDeleted == false && x.IsActive == true).ToList();
                    List = Mapper.Map(DBoyMasterList).ToANew<List<DBoyMasterDc>>();
                    res.totalcount = List.Count();
                    res.DBoyMasterDcs = List;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return res;
        }

        [Route("GetDBoyMasterById")]
        [HttpGet]
        public IHttpActionResult GetDBoyMasterById(int Id)
        {
            ResDBoyMaster res = new ResDBoyMaster();
            try
            {
                DboyMaster DBoyMasterList = new DboyMaster();
                DBoyMasterDc List = new DBoyMasterDc();
                using (var context = new AuthContext())
                {
                    DBoyMasterList = context.DboyMasters.FirstOrDefault(x => x.IsDeleted == false && x.Id == Id);
                    List = Mapper.Map(DBoyMasterList).ToANew<DBoyMasterDc>();
                    //res.totalcount = List.Count();
                    return Ok(List);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Route("GetVehicleDetailsByDboyId")]
        [HttpGet]
        public List<RegistrationNoListDC> GetVehicleDetailsByDboyId(int Id)
        {
            List<RegistrationNoListDC> resList = new List<RegistrationNoListDC>();
            using (var db = new AuthContext())
            {
                var Param = new SqlParameter("DboyId", Id);
                resList = db.Database.SqlQuery<RegistrationNoListDC>("exec GetVehicleDetailsByDboyId @DboyId", Param).ToList();
            }
            return resList;
        }

        [Route("GetDBoyMasterPagination")]
        [HttpGet]
        public ResDBoyMaster DBoyMasterPagination(int skip, int take)
        {
            int Skiplist = (skip - 1) * take;
            ResDBoyMaster res = new ResDBoyMaster();
            try
            {
                List<DboyMaster> DBoyMasterList = new List<DboyMaster>();
                List<DBoyMasterDc> List = new List<DBoyMasterDc>();
                using (var context = new AuthContext())
                {
                    DBoyMasterList = context.DboyMasters.Where(x => x.IsDeleted == false).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                    List = Mapper.Map(DBoyMasterList).ToANew<List<DBoyMasterDc>>();
                    var cityids = List.Select(x => x.CityId).Distinct().ToList();
                    var warehosuesids = List.Select(x => x.WarehouseId).Distinct().ToList();
                    var citylist = context.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                    var warehouses = context.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                    var Agentids = List.Select(x => x.AgentId).Distinct().ToList();
                    var AgentList = context.Peoples.Where(x => Agentids.Contains(x.PeopleID)).ToList();

                    List.ForEach(y =>
                    {
                        y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                        y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        y.AgentName = AgentList.Where(x => x.PeopleID == y.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                        //if (y.TripTypeEnum == 0)
                        //{
                        //    y.TripTypeName = "City";
                        //}
                        //else if (y.TripTypeEnum == 1)
                        //{
                        //    y.TripTypeName = "SKP";
                        //}
                        //else if (y.TripTypeEnum == 2)
                        //{
                        //    y.TripTypeName = "KPP";
                        //}
                        //else if (y.TripTypeEnum == 3)
                        //{
                        //    y.TripTypeName = "Damage_Expiry";
                        //}
                    });
                    res.totalcount = context.DboyMasters.Count();
                    res.DBoyMasterDcs = List;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return res;
        }
        [Route("SearchDBoyMaster")]
        [HttpGet]
        public ResDBoyMaster SearchDBoyMasterList(string key, int statusValue, int? cityId, int? warehousid, int skip, int take)
        
        {
            ResDBoyMaster res = new ResDBoyMaster();
            try
            {
               
                int Skiplist = (skip - 1) * take;
                List<DboyMaster> DBoyMasterList = new List<DboyMaster>();

                List<DBoyMasterDc> List = new List<DBoyMasterDc>();
                using (var context = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    var wid = context.Peoples.Where(x => x.PeopleID == userid && x.Active == true).Select(x => x.WarehouseId).FirstOrDefault();
                    var predicate = PredicateBuilder.True<DboyMaster>();

                    predicate = predicate.And(x => x.IsDeleted == false);
                    //if(wid>0)
                    //{
                    //    predicate = predicate.And(x=>x.WarehouseId==wid);
                    //}
                    if (key != null )
                    {
                        predicate = predicate.And(x =>x.Name.Contains(key) || x.MobileNo==key || x.AadharNo.Contains(key) || x.AgentOrTransport.Contains(key));
                    }
                    if (cityId.HasValue && cityId.Value > 0)
                    {
                        predicate = predicate.And(x => x.CityId == cityId);
                    }
                    if ((warehousid.HasValue && warehousid.Value > 0) )
                    {
                        predicate = predicate.And(x => (x.WarehouseId == warehousid) );
                    }
                    if (statusValue == 1) //Active
                    {
                        predicate = predicate.And(x => x.IsActive == true);
                    }
                    if (statusValue == 2) //InActive
                    {
                        predicate = predicate.And(x => x.IsActive == false);
                    }
                    if (statusValue == 3) //IsBlocked
                    {
                        predicate = predicate.And(x => x.IsBlocked == true);
                    }
                   
                    if ((cityId == 0 || cityId == null) && (warehousid == 0 || warehousid == null) &&(key==null || key=="") && (statusValue==0 || statusValue==null))
                    {
                        DBoyMasterList = context.DboyMasters.Where(x => x.IsDeleted == false && x.WarehouseId == wid).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                    }
                    else
                    {
                       
                        DBoyMasterList = context.DboyMasters.Where(predicate).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                       
                    }
                    
                    List = Mapper.Map(DBoyMasterList).ToANew<List<DBoyMasterDc>>();
                    var cityids = List.Select(x => x.CityId).Distinct().ToList();
                    var warehosuesids = List.Select(x => x.WarehouseId).Distinct();
                    var citylist = context.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                    var warehouses = context.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                    var Agentids = List.Select(x => x.AgentId).Distinct().ToList();
                    var AgentList = context.Peoples.Where(x => Agentids.Contains(x.PeopleID)).ToList();

                    List.ForEach(y =>
                    {
                        y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                        y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        y.AgentName = AgentList.Where(x => x.PeopleID == y.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                        //if (y.TripTypeEnum == 0)
                        //{
                        //    y.TripTypeName = "City";
                        //}
                        //else if (y.TripTypeEnum == 1)
                        //{
                        //    y.TripTypeName = "SKP";
                        //}
                        //else if (y.TripTypeEnum == 2)
                        //{
                        //    y.TripTypeName = "KPP";
                        //}
                        //else if (y.TripTypeEnum == 3)
                        //{
                        //    y.TripTypeName = "Damage_Expiry";
                        //}
                    });
                   
                        if ((cityId == 0 || cityId == null) && (warehousid == 0 || warehousid == null) && (key == null || key == "") && (statusValue == 0 || statusValue == null))
                        {

                            res.totalcount = context.DboyMasters.Where(x => x.IsDeleted == false && x.WarehouseId == wid).Count();
                            res.DBoyMasterDcs = List;
                        }
                        else
                        {

                            res.totalcount = context.DboyMasters.Where(predicate).Count();
                            res.DBoyMasterDcs = List;
                        }
                    
                }
                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
            return res;
        }
        [Route("ExportDBoyMaster")]
        [HttpGet]
        public List<DboyExportDc> ExportDBoyMaster(int wid)
        {

            try
            {
                List<DboyMaster> DBoyMasterList = new List<DboyMaster>();
                List<DboyExportDc> list = new List<DboyExportDc>();
                using (var context = new AuthContext())
                {
                    DBoyMasterList = context.DboyMasters.Where(x => x.IsDeleted == false && x.WarehouseId == wid).ToList();
                    list = Mapper.Map(DBoyMasterList).ToANew<List<DboyExportDc>>();

                    var cityids = list.Select(x => x.CityId).Distinct().ToList();
                    var warehosuesids = list.Select(x => x.WarehouseId).Distinct().ToList();
                    var citylist = context.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                    var warehouses = context.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                    var Agentids = list.Select(x => x.AgentId).Distinct().ToList();
                    var AgentList = context.Peoples.Where(x => Agentids.Contains(x.PeopleID)).ToList();

                    list.ForEach(y =>
                    {
                        y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                        y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        y.AgentName = AgentList.Where(x => x.PeopleID == y.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                    });
                    return list;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Route("DocumentImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult DocumentImageUpload()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {

                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/DboyImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/DboyImage"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DboyImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/DboyImage", LogoUrl);

                        LogoUrl = "/DboyImage/" + fileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [Route("Activeagent")]
        [HttpGet]
        public IEnumerable<People> GetActive(int CityId)
        {
            using (var context = new AuthContext())
            {
                int compid = 0;
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                    }
                    List<People> person = AllPeoplesWidActiveAgent(compid, CityId).ToList();
                    return person;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

        }
        public IEnumerable<People> AllPeoplesWidActiveAgent(int compid, int CityId)
        {
            try
            {
                using (var context = new AuthContext())
                {

                    string query = "select distinct p.PeopleID from People p inner join AspNetUsers u on p.Email=u.Email and p.Deleted=0  inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where p.CompanyId=" + 1 + " and p.CityId=" + CityId + " and r.Name='Agent' and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    List<int> PeopleId = context.Database.SqlQuery<int>(query).ToList();
                    var person = context.Peoples.Where(p => PeopleId.Contains(p.PeopleID)).ToList().OrderBy(x => x.DisplayName).ToList();
                    return person;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int?> CreatePeopleOfDboy(DboyMaster DboyData, AuthContext context)
        {
            #region Create Dboy in People

            string query = "select PeopleID from People where Mobile='" + DboyData.MobileNo + "'";
            int? PeopleID = context.Database.SqlQuery<int?>(query).FirstOrDefault();
            if (PeopleID == null)
            {
                byte Levels = 4;
                People p = new People();
                p.UserName = DboyData.MobileNo;
                p.Password = "Sk@123";
                var user = new ApplicationUser()
                {
                    UserName = p.UserName,
                    Email = DboyData.MobileNo + "@" + DboyData.MobileNo + ".com",
                    FirstName = DboyData.Name,
                    LastName = "",
                    Level = Levels,
                    JoinDate = DateTime.Now.Date,
                    EmailConfirmed = true,
                };

                AccountController s = new AccountController();
                var UserId = s.CreatePeopleForDBoy(p, user);

                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == DboyData.WarehouseId);
                p.Email = user.Email;
                p.PeopleFirstName = DboyData.Name;
                p.WarehouseId = warehouse.WarehouseId;
                p.Stateid = warehouse.Stateid;
                p.Cityid = warehouse.Cityid;
                p.Mobile = p.UserName;
                p.Active = true;
                p.EmailConfirmed = true;
                p.Empcode = "";
                p.Desgination = "Delivery Boy";
                p.DOB = DateTime.Now;
                p.DataOfJoin = DateTime.Now;
                p.CreatedDate = DateTime.Now;
                p.UpdatedDate = DateTime.Now;
                p.DisplayName = DboyData.Name;
                context.Peoples.Add(p);
                context.Commit();
                PeopleID = p.PeopleID;
            }
            #endregion
            return PeopleID;
        }


        public bool ActiveInactivePeople(AuthContext db, int PeopleId, bool IsActive, int UserId)
        {
            var People = db.Peoples.FirstOrDefault(x => x.PeopleID == PeopleId);
            People.Active = IsActive;
            People.UpdatedDate = DateTime.Now;
            db.Entry(People).State = EntityState.Modified;
            return db.Commit() > 0;
        }

    }
}
