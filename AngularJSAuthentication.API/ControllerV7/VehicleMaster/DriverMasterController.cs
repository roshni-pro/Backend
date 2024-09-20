using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Claims;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using AgileObjects.AgileMapper;
using AngularJSAuthentication.DataContracts.Masters.VehicleMaster;
using AngularJSAuthentication.Model;
using LinqKit;

namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/DriverMaster")]
    public class DriverMasterController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("PostDriverMaster")]
        [HttpPost]
        public ResDriverMaster AddDriver(DriverMaster obj)
        {
            ResDriverMaster res = new ResDriverMaster();
            try
            {
                using (var context = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    var IsDuplicateMob = context.DriverMasters.FirstOrDefault(x => x.Id != obj.Id && x.IsDeleted == false && x.MobileNo == obj.MobileNo);
                    if (IsDuplicateMob != null)
                    {
                        res.msg = "Mobile No. Already Exist";
                        res.Result = false;
                        return res;
                    }

                    var driverData = context.DriverMasters.FirstOrDefault(x => x.AadharNo == obj.AadharNo && x.Id == obj.Id && x.IsDeleted == false);

                    if (driverData != null)
                    {
                        driverData.Name = obj.Name;
                        driverData.Address = obj.Address;
                        driverData.MobileNo = obj.MobileNo;
                        driverData.AadharNo = obj.AadharNo;
                        driverData.AadharCopy = obj.AadharCopy;
                        driverData.AadharCopyBack = obj.AadharCopyBack;
                        driverData.Photo = obj.Photo;
                        driverData.DLCopy = obj.DLCopy;
                        driverData.DLNo = obj.DLNo;
                        driverData.DLNoValidity = obj.DLNoValidity;
                        driverData.TransportName = obj.TransportName;
                        driverData.IsActive = obj.IsActive;
                        driverData.IsDeleted = false;
                        driverData.IsBlocked = obj.IsBlocked;
                        driverData.ModifiedBy = userid;
                        driverData.ModifiedDate = DateTime.Now;
                        driverData.CityId = obj.CityId;
                        driverData.WarehouseId = obj.WarehouseId;
                        context.Entry(driverData).State = EntityState.Modified;
                    }
                    else
                    {
                        var IsDuplicateBlock = context.DriverMasters.FirstOrDefault(x => x.IsDeleted == false  && (x.AadharNo == obj.AadharNo || x.DLNo == obj.DLNo));
                        if (IsDuplicateBlock != null)
                        {
                            res.msg = "Your Number/Aadhar/ Dl is blocked";
                            res.Result = false;
                            return res;
                        }
                        //var IsDuplicate = context.DriverMasters.FirstOrDefault(x => x.IsDeleted == false && x.IsBlocked == false && (x.MobileNo == obj.MobileNo || x.AadharNo == obj.AadharNo || x.DLNo == obj.DLNo));
                        //if (IsDuplicate != null)
                        //{
                        //    res.msg = "Driver Already Registerd";
                        //    res.Result = false;
                        //    return res;
                        //}
                        driverData = new DriverMaster
                        {
                            Name = obj.Name,
                            Address = obj.Address,
                            MobileNo = obj.MobileNo,
                            AadharNo = obj.AadharNo,
                            AadharCopy = obj.AadharCopy,
                            AadharCopyBack = obj.AadharCopyBack,
                            Photo = obj.Photo,
                            DLCopy = obj.DLCopy,
                            DLNo = obj.DLNo,
                            DLNoValidity = obj.DLNoValidity,
                            TransportName = obj.TransportName,
                            IsActive = obj.IsActive,
                            IsDeleted = false,
                            IsBlocked = obj.IsBlocked,
                            CreatedBy = userid,
                            CreatedDate = DateTime.Now,
                            CityId = obj.CityId,
                            WarehouseId = obj.WarehouseId
                        };
                        context.DriverMasters.Add(driverData);
                    }
                    if (context.Commit() > 0)
                    {
                        res.msg = "Successfully Saved";
                        res.Result = true;
                        return res;
                    }
                    else
                    {
                        res.msg = "Failed";
                        res.Result = false;
                        return res;
                    }
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [Route("DriverActiveDeactiveList")]
        [HttpGet]
        [AllowAnonymous]
        public bool ActiveDeactiveList(int Id, bool IsActive)
        {
            using (var db = new AuthContext())
            {
                var list = db.DriverMasters.Where(x => x.Id == Id).FirstOrDefault();
                if (list != null)
                {
                    list.IsActive = IsActive;
                    db.Entry(list).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        [Route("DriverBlockById")]
        [HttpGet]
        [AllowAnonymous]
        public bool DriverBlockById(int Id, bool IsBlocked)
        {
            using (var db = new AuthContext())
            {
                var list = db.DriverMasters.Where(x => x.Id == Id).FirstOrDefault();
                if (list != null)
                {
                    list.IsBlocked = IsBlocked;
                    db.Entry(list).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        [Route("GetDriverMaster")]
        [HttpGet]
        public ResDriverMaster GetDriverList()
        {
            ResDriverMaster res = new ResDriverMaster();
            try
            {
                List<DriverMaster> DriverMasterList = new List<DriverMaster>();
                List<DriverMasterDc> List = new List<DriverMasterDc>();
                using (var context = new AuthContext())
                {
                    DriverMasterList = context.DriverMasters.Where(x => x.IsDeleted == false && x.IsActive == true).ToList();
                    List = Mapper.Map(DriverMasterList).ToANew<List<DriverMasterDc>>();
                    res.totalcount = List.Count();
                    res.DriverMasterDcs = List;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return res;
        }
        [Route("GetDriverMasterPagination")]
        [HttpGet]
        public ResDriverMaster GetDriverPagination(int skip, int take)
        {
            int Skiplist = (skip - 1) * take;
            ResDriverMaster res = new ResDriverMaster();
            try
            {
                List<DriverMaster> DriverMasterList = new List<DriverMaster>();
                List<DriverMasterDc> List = new List<DriverMasterDc>();
                using (var context = new AuthContext())
                {
                    DriverMasterList = context.DriverMasters.Where(x => x.IsDeleted == false).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                    List = Mapper.Map(DriverMasterList).ToANew<List<DriverMasterDc>>();
                    List = Mapper.Map(DriverMasterList).ToANew<List<DriverMasterDc>>();
                    var cityids = List.Select(x => x.CityId).Distinct().ToList();
                    var warehosuesids = List.Select(x => x.WarehouseId).Distinct().ToList();
                    var citylist = context.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                    var warehouses = context.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();

                    List.ForEach(y =>
                    {
                        y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                        y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    });
                    res.totalcount = context.DriverMasters.Count();
                    res.DriverMasterDcs = List;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return res;
        }
        [Route("SearchDriverMaster")]
        [HttpGet]
        public ResDriverMaster SearchDriverMaster(string key, int statusValue, int? cityId, int? warehousid , int skip, int take)
        {
            ResDriverMaster res = new ResDriverMaster();
            try
            {
                int Skiplist = (skip - 1) * take;
                List<DriverMaster> DriverMasterList = new List<DriverMaster>();
                List<DriverMasterDc> List = new List<DriverMasterDc>();
                using (var context = new AuthContext())
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    var wid = context.Peoples.Where(x => x.PeopleID == userid && x.Active==true).Select(x => x.WarehouseId).FirstOrDefault();
                    var predicate = PredicateBuilder.True<DriverMaster>();

                    predicate = predicate.And(x => x.IsDeleted == false);
                    //if (wid > 0)
                    //{
                    //    predicate = predicate.And(x => x.WarehouseId == wid);
                    //}
                    if (key != null)
                    {
                        predicate = predicate.And(x => x.Name.Contains(key) || x.MobileNo.Contains(key) || x.AadharNo.Contains(key) || x.TransportName.Contains(key));
                    }
                    if (cityId.HasValue && cityId.Value > 0)
                    {
                        predicate = predicate.And(x => x.CityId == cityId);
                    }
                    if (warehousid.HasValue && warehousid.Value > 0)
                    {
                        predicate = predicate.And(x => x.WarehouseId == warehousid);
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
                    
                    if((cityId==0 || cityId==null) && (warehousid == 0 || warehousid==null) && (key == null || key == "") && (statusValue == 0 || statusValue == null))
                    {
                        DriverMasterList = context.DriverMasters.Where(x => x.IsDeleted == false && x.WarehouseId==wid).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                    }
                    else
                    {
                        DriverMasterList = context.DriverMasters.Where(predicate).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                    }
                    
                    List = Mapper.Map(DriverMasterList).ToANew<List<DriverMasterDc>>();
                    var cityids = List.Select(x => x.CityId).Distinct().ToList();
                    var warehosuesids = List.Select(x => x.WarehouseId).Distinct().ToList();
                    var citylist = context.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                    var warehouses = context.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();

                    List.ForEach(y =>
                    {
                        y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                        y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                    });

                    
                        if ((cityId == 0 || cityId == null) && (warehousid == 0 || warehousid == null) && (key == null || key == "") && (statusValue == 0 || statusValue == null))
                        {
                            res.totalcount = context.DriverMasters.Where(x => x.IsDeleted == false && x.WarehouseId == wid).Count();
                            res.DriverMasterDcs = List;
                        }
                        else
                        {
                            res.totalcount = context.DriverMasters.Where(predicate).Count();
                            res.DriverMasterDcs = List;
                        }
                   
                        
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return res;
        }

        [Route("ExportDriverMaster")]
        [HttpGet]
        public List<DriverMasterExportDc> ExportDriverMaster(int wid)
        {
            ResDriverMaster res = new ResDriverMaster();
            try
            {
                List<DriverMaster> DriverMasterList = new List<DriverMaster>();
                List<DriverMasterExportDc> list = new List<DriverMasterExportDc>();
                using (var context = new AuthContext())
                {
                    DriverMasterList = context.DriverMasters.Where(x => x.IsDeleted == false && x.WarehouseId == wid).ToList();
                    list = Mapper.Map(DriverMasterList).ToANew<List<DriverMasterExportDc>>();
                    var cityids = list.Select(x => x.CityId).Distinct().ToList();
                    var warehosuesids = list.Select(x => x.WarehouseId).Distinct().ToList();
                    var citylist = context.Cities.Where(x => cityids.Contains(x.Cityid)).ToList();
                    var warehouses = context.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                    list.ForEach(y =>
                    {
                        y.CityName = citylist.Where(x => x.Cityid == y.CityId).Select(x => x.CityName).FirstOrDefault();
                        y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
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

                        if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/DriverImage")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/DriverImage"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DriverImage"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);

                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(httpPostedFile.FileName, "~/DriverImage", LogoUrl);

                        LogoUrl = "/DriverImage/" + fileName;

                    }

                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
