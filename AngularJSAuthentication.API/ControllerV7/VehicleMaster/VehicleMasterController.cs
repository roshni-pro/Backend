
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper.VehicleMasterHelper;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
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


namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/VehicleMaster")]
    public class VehicleMasterController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [AllowAnonymous]
        [Route("AddVehicleMaster")]
        [HttpPost]
        public async Task<ResVehicleMasterDC> AddVehicleMaster(VehicleMasterDC AddVehicleMasterdc)
        {
            ResVehicleMasterDC res = new ResVehicleMasterDC();
            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            var Message = "";
            int userid = 0;
            userid = GetUserId();
            using (var context = new AuthContext())
            {
                var IsDuplicateRegistrationNo = context.VehicleMasterDB.Where(x => x.RegistrationNo == AddVehicleMasterdc.RegistrationNo || x.VehicleNo == AddVehicleMasterdc.VehicleNo).FirstOrDefault();
                if (IsDuplicateRegistrationNo != null)
                {
                    res.msg = "Already Added Registration No.";
                    res.Result = false;
                    return res;
                }
                var IsDuplicateChasisNo = context.VehicleMasterDB.Where(x => x.ChasisNo == AddVehicleMasterdc.ChasisNo).FirstOrDefault();
                if (IsDuplicateChasisNo != null)
                {
                    res.msg = "Already Added Chasis No.";
                    res.Result = false;
                    return res;
                }
                var IsDuplicateEngineNo = context.VehicleMasterDB.Where(x => x.EngineNo == AddVehicleMasterdc.EngineNo).FirstOrDefault();
                if (IsDuplicateEngineNo != null)
                {
                    res.msg = "Already Added Engine No.";
                    res.Result = false;
                    return res;
                }
            }
            bool result = fleetMasterHelper.AddnewVehicleMaster(AddVehicleMasterdc, userid);
            if (result)
            {

                res.msg = "Vehicle Master Added";
                res.Result = true;
                return res;

            }
            else
            {

                res.msg = "Vehicle Master not Added";
                res.Result = false;
                return res;

            }
        }
        [AllowAnonymous]
        [Route("EditVehicleMaster")]
        [HttpPost]
        public async Task<ResVehicleMasterDC> EditVehicleMaster(VehicleMasterDC EditVehicleMasterdc)
        {
            ResVehicleMasterDC res = new ResVehicleMasterDC();
            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            int userid = 0;
            var Message = "";
            userid = GetUserId();
            bool result = fleetMasterHelper.EditVehicleMasters(EditVehicleMasterdc, userid);
            if (result)
            {
                res.msg = "Vehicle Master Update";
                res.Result = true;
                return res;
            }
            else
            {
                res.msg = "Vehicle Master not Update";
                res.Result = true;
                return res;
            }
        }

        [AllowAnonymous]
        [Route("GetVehicleMasterList")]
        [HttpGet]
        public HttpResponseMessage GetVehicleMasterList(int skip, int take)
        {
            int Skiplist = (skip - 1) * take;
            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            VehiclePaggingDC result = new VehiclePaggingDC();
            result = fleetMasterHelper.getAllListvehicleMaster(Skiplist, take);

            if (result != null && result.VehicleMasterlist.Any())
            {
                var response = new
                {
                    Status = true,
                    data = result,
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        [AllowAnonymous]
        [Route("VehicleMasterbyId")]
        [HttpGet]
        public HttpResponseMessage VehicleMasterbyId(long Id)
        {
            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            VehicleMasterDC result = new VehicleMasterDC();
            result = fleetMasterHelper.VehicleMasterId(Id);
            if (result != null)
            {
                var response = new
                {
                    Status = true,
                    data = result,
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                var response = new
                {
                    Status = false,
                    data = "No Data found"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }


        [AllowAnonymous]
        [Route("ExportVehicleMasterList")]
        [HttpGet]
        public AllData ExportVehicleMasterList(DateTime? startDate, DateTime? endDate, string search, int statusValue, int WarehouseId,int cityId,int skip, int take)
        {
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                
                FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
                AllData result = new AllData();
                result = fleetMasterHelper.exportListvehicleMaster(startDate, endDate, search, statusValue, WarehouseId, cityId, skip, take, userid) ;

                if (result != null)
                {

                    return result;
                }
                else
                {
                    return null;
                }
            }
        }


        [Route("TransportList")]
        [HttpGet]
        [AllowAnonymous]
        public List<TransportNameListDC> TransportList(string fleetType, int Warehouseid)
        {
            using (var db = new AuthContext())
            {
                var cityid = db.Warehouses.FirstOrDefault(x => x.WarehouseId == Warehouseid).Cityid;
                //DateTime startDate = DateTime.Now.Date;
                //DateTime StartDate = startDate.AddDays(1).Date.AddHours(00).AddMinutes(00).AddSeconds(00);
                ////     DateTime endDate = startDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                //   var list = db.FleetMasterDB.Where(x => fleetType.Contains(x.FleetType) && x.IsActive == true  && x.ContractStart <= StartDate && x.ContractEnd >= StartDate && x.IsBlocked == false).Select(x => new { x.TransportName, x.Id }).ToList();
                if (cityid > 0)
                {
                    string queary = "select Id,TransportName from FleetMasters where CAST( ContractStart as date) <=cast(GETDATE() as date) and CAST(ContractEnd as date)  >= cast(GETDATE() as date) and IsBlocked=0 and IsDeleted=0 and IsActive=1 and FleetType like '%" + fleetType + "%' and CityId=" + cityid + "";
                    var list = db.Database.SqlQuery<TransportNameListDC>(queary).ToList();
                    return list;
                }
                return null;
            }
        }
        [Route("VehicleList")]
        [HttpGet]
        [AllowAnonymous]

        public List<VehicleListDC> VehicleList(int FleetMasterId)
        {
            List<VehicleListDC> resultList = new List<VehicleListDC>();
            using (var db = new AuthContext())
            {
                //var list = db.FleetMasterDetailDB.Where(x => x.FleetMasterId == FleetMasterId && x.IsActive == true && x.IsDeleted == false).Select(x => new { x.VehicleType, x.Id }).ToList();
                //return list;
                var fleetMasterId = new SqlParameter("@FleetMasterId", FleetMasterId);
                resultList = db.Database.SqlQuery<VehicleListDC>("ExistsVehiclelist @FleetMasterId", fleetMasterId).ToList();
                if (resultList.Any(x => x.Id == 0))
                {
                    var emptylist = new List<VehicleListDC>();
                    return emptylist;
                }                                                 
            }
            return resultList;
        }
        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }
        [AllowAnonymous]
        [Route("AddOutBoundDeliveryMapping")]
        [HttpPost]
        public async Task<ResOutBoundDC> AddOutBoundDeliveryMapping(OutBoundDeliveryMappingDC outBoundDeliveryMappingDC)
        {
            int userid = 0;
            userid = GetUserId();
            ResOutBoundDC res = new ResOutBoundDC();
            using (var context = new AuthContext())
            {
                if (outBoundDeliveryMappingDC != null && outBoundDeliveryMappingDC.outBoundDeliveryDetails.Any())
                {
                    List<OutBoundDeliveryMapping> Addlist = new List<OutBoundDeliveryMapping>();
                    OutBoundDeliveryMapping add = new OutBoundDeliveryMapping();

                    var checkVehicleMaster = outBoundDeliveryMappingDC.outBoundDeliveryDetails.GroupBy(s => s.VehicleMasterId).Where(g => g.Count() > 1)
                             .Select(g => g.Key).Count();
                    var checkDriverMaster = outBoundDeliveryMappingDC.outBoundDeliveryDetails.GroupBy(s => s.DriverMasterId).Where(g => g.Count() > 1)
                            .Select(g => g.Key).Count();
                    var checkDboyMaster = outBoundDeliveryMappingDC.outBoundDeliveryDetails.GroupBy(s => s.DboyMasterId).Where(g => g.Count() > 1)
                            .Select(g => g.Key).Count();
                    if (checkDboyMaster != 0 || checkDriverMaster != 0 || checkVehicleMaster != 0)
                    {
                        res.msg = "Duplicate Data Not save!!";
                        res.Result = false;
                        return res;
                    }

                    var isMapping = context.OutBoundDeliveryMappings.Where(x => x.Id == outBoundDeliveryMappingDC.Id && x.IsDeleted == false).Include(x => x.OutBoundDeliveryDetails).FirstOrDefault();
                    if (isMapping == null)
                    {
                        add.ClusterId = outBoundDeliveryMappingDC.ClusterId;
                        add.AgentId = outBoundDeliveryMappingDC.AgentId;
                        add.WarehouseId = outBoundDeliveryMappingDC.WarehouseId;
                        add.CreatedBy = userid;
                        add.CreatedDate = DateTime.Now;
                        add.IsActive = true;
                        add.IsDeleted = false;
                        add.OutBoundDeliveryDetails = new List<OutBoundDeliveryDetail>();
                        foreach (var list in outBoundDeliveryMappingDC.outBoundDeliveryDetails)
                        {
                            OutBoundDeliveryDetail obj = new OutBoundDeliveryDetail
                            {
                                VehicleMasterId = list.VehicleMasterId,
                                DriverMasterId = list.DriverMasterId,
                                DboyMasterId = list.DboyMasterId,
                                CreatedBy = userid,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false
                            };
                            add.OutBoundDeliveryDetails.Add(obj);

                        }
                        context.OutBoundDeliveryMappings.Add(add);
                    }
                    else
                    {
                        List<long> newid = outBoundDeliveryMappingDC.outBoundDeliveryDetails.Select(x => x.Id).Distinct().ToList();
                        foreach (var list in outBoundDeliveryMappingDC.outBoundDeliveryDetails)
                        {
                            var details = isMapping.OutBoundDeliveryDetails.Where(x => x.Id == list.Id).FirstOrDefault();
                            if (details == null)
                            {
                                OutBoundDeliveryDetail obj = new OutBoundDeliveryDetail
                                {
                                    VehicleMasterId = list.VehicleMasterId,
                                    DriverMasterId = list.DriverMasterId,
                                    DboyMasterId = list.DboyMasterId,
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    IsDeleted = false
                                };
                                isMapping.OutBoundDeliveryDetails.Add(obj);
                            }
                            else
                            {
                                details.VehicleMasterId = list.VehicleMasterId;
                                details.DriverMasterId = list.DriverMasterId;
                                details.DboyMasterId = list.DboyMasterId;
                                details.CreatedBy = userid;
                                details.CreatedDate = DateTime.Now;
                                details.IsActive = true;
                                details.IsDeleted = false;
                                context.Entry(details).State = EntityState.Modified;
                            }
                        }

                        foreach (var obj in isMapping.OutBoundDeliveryDetails.Where(x => !newid.Contains(x.Id)).Select(z => z.Id).ToList())
                        {
                            var list = isMapping.OutBoundDeliveryDetails.Where(x => x.Id == obj).FirstOrDefault();
                            list.ModifiedDate = DateTime.Now;
                            list.ModifiedBy = userid;
                            list.IsActive = false;
                            list.IsDeleted = true;
                            context.Entry(list).State = EntityState.Modified;
                        }
                    }
                    if (context.Commit() > 0)
                    {
                        res.msg = "Out Bound Delivery Mapping Added";
                        res.Result = true;
                        return res;
                    }
                }
                else
                {
                    res.msg = "Out Bound Delivery Mapping Not Added";
                    res.Result = false;
                    return res;
                }
                return res;
            }
        }
        [AllowAnonymous]
        [Route("EditOutBoundDeliveryMapping")]
        [HttpPost]
        public async Task<ResOutBoundDC> EditOutBoundDeliveryMapping(OutBoundDeliveryMappingDC editOutBoundDeliveryDC)
        {
            using (var db = new AuthContext())
            {
                int userid = 0;
                userid = GetUserId();
                ResOutBoundDC res = new ResOutBoundDC();

                using (var context = new AuthContext())
                {
                    if (editOutBoundDeliveryDC != null && editOutBoundDeliveryDC.outBoundDeliveryDetails.Any())
                    {
                        var OutBoundlist = context.OutBoundDeliveryMappings.Where(x => x.Id == editOutBoundDeliveryDC.Id && x.IsDeleted == false).FirstOrDefault();
                        if (OutBoundlist != null)
                        {
                            OutBoundlist.ClusterId = editOutBoundDeliveryDC.ClusterId;
                            OutBoundlist.AgentId = editOutBoundDeliveryDC.AgentId;
                            OutBoundlist.ModifiedBy = userid;
                            OutBoundlist.ModifiedDate = DateTime.Now;
                            OutBoundlist.IsDeleted = false;
                            foreach (var item in editOutBoundDeliveryDC.outBoundDeliveryDetails)
                            {
                                var OutBoundDetailslist = context.OutBoundDeliveryDetails.Where(x => x.Id == item.Id && x.IsDeleted == false).FirstOrDefault();
                                if (OutBoundDetailslist != null)
                                {
                                    OutBoundDetailslist.DboyMasterId = item.DboyMasterId;
                                    OutBoundDetailslist.VehicleMasterId = item.VehicleMasterId;
                                    OutBoundDetailslist.DriverMasterId = item.DriverMasterId;
                                    OutBoundlist.ModifiedBy = userid;
                                    OutBoundlist.ModifiedDate = DateTime.Now;
                                    OutBoundlist.IsDeleted = false;
                                }
                            }
                            context.Entry(OutBoundlist).State = EntityState.Modified;
                            if (context.Commit() > 0)
                            {
                                res.msg = "Out Bound Delivery Mapping updated!!";
                                res.Result = true;
                                return res;
                            }
                        }
                        else
                        {
                            res.msg = "Out Bound Delivery Mapping Not updated";
                            res.Result = false;
                            return res;
                        }
                    }
                    else
                    {
                        res.msg = "Out Bound Delivery Mapping Not updated";
                        res.Result = false;
                        return res;
                    }
                    return res;
                }
            }
        }


        [Route("VehicleRegistrationNoLIst")]
        [HttpGet]
        [AllowAnonymous]
        public List<VehicleRegistrationNoLIstDC> getVehicleRegistrationNoLIst(int CityId)
        {
            using (var db = new AuthContext())
            {

                List<VehicleRegistrationNoLIstDC> list = new List<VehicleRegistrationNoLIstDC>();
                var Alllist = db.VehicleMasterDB.Where(x => x.CityId == CityId && x.IsActive == true && x.IsDeleted == false).Select(x => new { x.VehicleNo, x.Id }).ToList();
                Alllist.ForEach(x =>
                {
                    VehicleRegistrationNoLIstDC Addlist = new VehicleRegistrationNoLIstDC();
                    Addlist.Id = (int)x.Id;
                    Addlist.RegistrationNo = x.VehicleNo;
                    list.Add(Addlist);
                });
                // list = Mapper.Map(Alllist).ToANew<List<VehicleRegistrationNoLIstDC>>();
                return list;
            }
        }
        [Route("DriverList")]
        [HttpGet]
        [AllowAnonymous]
        public List<DriverListDC> getDriverList(int CityId)
        {
            using (var db = new AuthContext())
            {
                List<DriverListDC> DriverList = new List<DriverListDC>();
                var Alllist = db.DriverMasters.Where(x => x.CityId == CityId && x.IsActive == true && x.IsDeleted == false).Select(x => new { x.Name, x.Id }).ToList();
                DriverList = Mapper.Map(Alllist).ToANew<List<DriverListDC>>();
                return DriverList;
            }
        }
        [Route("getDboyList")]
        [HttpGet]
        [AllowAnonymous]
        public List<DboyListDC> getDboyList(int CityId)
        {
            using (var db = new AuthContext())
            {
                List<DboyListDC> DriverList = new List<DboyListDC>();
                var Alllist = db.DboyMasters.Where(x => x.CityId == CityId && x.IsActive == true && x.IsDeleted == false).Select(x => new { x.Name, x.Id }).ToList();
                DriverList = Mapper.Map(Alllist).ToANew<List<DboyListDC>>();
                return DriverList;
            }
        }
        [Route("ActiveDeactiveList")]
        [HttpGet]
        [AllowAnonymous]
        public bool ActiveDeactiveList(int Id, bool IsActive)
        {
            using (var db = new AuthContext())
            {
                var list = db.VehicleMasterDB.Where(x => x.Id == Id).FirstOrDefault();
                if(IsActive)
                {
                    var existingVehicleList = db.VehicleMasterDB.Where(x => x.IsActive && (x.IsDeleted == false || x.IsDeleted == null) && x.VehicleNo == list.VehicleNo && x.Id != Id).ToList();
                    if(existingVehicleList != null && existingVehicleList.Any())
                    {
                        return false;
                    }
                }
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
        [Route("VehicleBlockById")]
        [HttpGet]
        [AllowAnonymous]
        public bool VehicleBlockById(int Id, bool IsBlocked)
        {
            using (var db = new AuthContext())
            {
                var list = db.VehicleMasterDB.Where(x => x.Id == Id).FirstOrDefault();
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

        [AllowAnonymous]
        [Route("GetOutBoundMappingList")]
        [HttpGet]
        public OutBoundPaggingDC GetOutBoundMappingList(int skip, int take, int? warehouesid)
        {
            int Skiplist = (skip - 1) * take;
            using (var db = new AuthContext())
            {
                OutBoundPaggingDC res = new OutBoundPaggingDC();
                outBoundDC Alllist = new outBoundDC();
                if (warehouesid > 0)
                {
                    var list = db.OutBoundDeliveryMappings.Where(x => x.WarehouseId == warehouesid && x.IsDeleted == false).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                    var NEWlist = Mapper.Map(list).ToANew<List<outBoundDC>>();
                    var warehosuesids = list.Select(x => x.WarehouseId).Distinct().ToList();
                    var clusterids = list.Select(x => x.ClusterId).Distinct().ToList();
                    var Agentids = list.Select(x => x.AgentId).Distinct().ToList();
                    var AgentList = db.Peoples.Where(x => Agentids.Contains(x.PeopleID)).ToList();
                    var warehouses = db.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                    var Clusterlist = db.Clusters.Where(x => clusterids.Contains(x.ClusterId)).ToList();
                    NEWlist.ForEach(y =>
                    {
                        y.ClusterName = Clusterlist.Where(x => x.ClusterId == y.ClusterId).Select(x => x.ClusterName).FirstOrDefault();
                        y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        y.AgentName = AgentList.Where(x => x.PeopleID == y.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                    });
                    res.outBoundList = NEWlist;
                    res.totalcount = db.OutBoundDeliveryMappings.Count();
                }
                else
                {
                    var list = db.OutBoundDeliveryMappings.Where(x => x.IsDeleted == false).OrderByDescending(x => x.Id).Skip(Skiplist).Take(take).ToList();
                    var NEWlist = Mapper.Map(list).ToANew<List<outBoundDC>>();
                    var warehosuesids = list.Select(x => x.WarehouseId).Distinct().ToList();
                    var clusterids = list.Select(x => x.ClusterId).Distinct().ToList();
                    var Agentids = list.Select(x => x.AgentId).Distinct().ToList();
                    var AgentList = db.Peoples.Where(x => Agentids.Contains(x.PeopleID)).ToList();
                    var warehouses = db.Warehouses.Where(x => warehosuesids.Contains(x.WarehouseId)).ToList();
                    var Clusterlist = db.Clusters.Where(x => clusterids.Contains(x.ClusterId)).ToList();
                    NEWlist.ForEach(y =>
                    {
                        y.ClusterName = Clusterlist.Where(x => x.ClusterId == y.ClusterId).Select(x => x.ClusterName).FirstOrDefault();
                        y.WarehouseName = warehouses.Where(x => x.WarehouseId == y.WarehouseId).Select(x => x.WarehouseName).FirstOrDefault();
                        y.AgentName = AgentList.Where(x => x.PeopleID == y.AgentId).Select(x => x.DisplayName).FirstOrDefault();
                    });
                    res.outBoundList = NEWlist;
                    res.totalcount = db.OutBoundDeliveryMappings.Count();
                }
                return res;
            }
        }
        [AllowAnonymous]
        [Route("GetOutBoundDeliveryDetails")]
        [HttpGet]
        public OutBoundMappingDC GetOutBoundDeliveryDetails(long Id)
        {
            using (var db = new AuthContext())
            {
                OutBoundMappingDC list = new OutBoundMappingDC();
                var data = db.OutBoundDeliveryMappings.Where(x => x.Id == Id && x.IsDeleted == false).Include(y => y.OutBoundDeliveryDetails).FirstOrDefault();
                list = Mapper.Map(data).ToANew<OutBoundMappingDC>();
                list.OutBoundDeliveryDetails = list.OutBoundDeliveryDetails.Where(i => i.IsDeleted == false).ToList();

                if (list.OutBoundDeliveryDetails != null && list.OutBoundDeliveryDetails.Any())
                {
                    var VehicleIds = list.OutBoundDeliveryDetails.Select(x => x.VehicleMasterId).ToList();
                    var DriverIds = list.OutBoundDeliveryDetails.Select(x => x.DriverMasterId).ToList();
                    var DBoyIds = list.OutBoundDeliveryDetails.Select(x => x.DboyMasterId).ToList();
                    var Vehicle = db.VehicleMasterDB.Where(x => VehicleIds.Contains(x.Id) && x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).ToList();
                    var Driver = db.DriverMasters.Where(x => DriverIds.Contains(x.Id) && x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).ToList();
                    var DBoy = db.DboyMasters.Where(x => DBoyIds.Contains(x.Id) && x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).ToList();
                    foreach (var item in list.OutBoundDeliveryDetails)
                    {
                        var vehicle = Vehicle.Where(x => x == item.VehicleMasterId).FirstOrDefault();
                        if (vehicle > 0)
                        {
                            item.VehicleMasterId = vehicle;
                        }
                        else
                        {
                            item.VehicleMasterId = 0;
                        }

                        var driver = Driver.Where(x => x == item.DriverMasterId).FirstOrDefault();
                        if (driver > 0)
                        {
                            item.DriverMasterId = driver;
                        }
                        else
                        {
                            item.DriverMasterId = 0;
                        }

                        var dBoy = DBoy.Where(x => x == item.DboyMasterId).FirstOrDefault();
                        if (dBoy > 0)
                        {
                            item.DboyMasterId = dBoy;
                        }
                        else
                        {
                            item.DboyMasterId = 0;
                        }
                    }
                }
                return list;
            }
        }
        [AllowAnonymous]
        [Route("GetOutBoundDetailsById")]
        [HttpGet]
        public OutBoundMappingDC GetOutBoundDetailsById(int WarehouseId, int ClusterId, int AgentId)
        {
            using (var db = new AuthContext())
            {

                OutBoundMappingDC Alllist = new OutBoundMappingDC();
                var outbound = db.OutBoundDeliveryMappings.Where(x => x.WarehouseId == WarehouseId && x.ClusterId == ClusterId && x.AgentId == AgentId && x.IsDeleted == false).Include(x => x.OutBoundDeliveryDetails).FirstOrDefault();
                if (outbound != null)
                {
                    var filter = outbound.OutBoundDeliveryDetails.Where(x => x.IsDeleted == false).ToList();
                    outbound.OutBoundDeliveryDetails = filter;
                    Alllist = Mapper.Map(outbound).ToANew<OutBoundMappingDC>();
                    var VehicleMasterIds = Alllist.OutBoundDeliveryDetails.Select(x => x.VehicleMasterId).Distinct().ToList();
                    var DriverMasterIds = Alllist.OutBoundDeliveryDetails.Select(x => x.DriverMasterId).Distinct().ToList();
                    var DboyMasterIds = Alllist.OutBoundDeliveryDetails.Select(x => x.DboyMasterId).Distinct().ToList();
                    var VehicleList = db.VehicleMasterDB.Where(x => VehicleMasterIds.Contains(x.Id)).ToList();
                    var Driverlist = db.DriverMasters.Where(x => DriverMasterIds.Contains(x.Id)).ToList();
                    var DboyList = db.DboyMasters.Where(x => DboyMasterIds.Contains(x.Id)).ToList();
                    Alllist.OutBoundDeliveryDetails.ForEach(y =>
                    {
                        y.Vehiclename = VehicleList.Where(x => x.Id == y.VehicleMasterId).Select(x => x.VehicleNo).FirstOrDefault();
                        y.DriverName = Driverlist.Where(x => x.Id == y.DriverMasterId).Select(x => x.Name).FirstOrDefault();
                        y.DboyName = DboyList.Where(x => x.Id == y.DboyMasterId).Select(x => x.Name).FirstOrDefault();
                    });
                }
                return Alllist;
            }
        }
        [Route("MappingActiveDeactiveList")]
        [HttpGet]
        [AllowAnonymous]
        public bool MappingActiveDeactiveList(int Id, bool IsActive)
        {
            using (var db = new AuthContext())
            {
                var list = db.OutBoundDeliveryMappings.Where(x => x.Id == Id).FirstOrDefault();
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


        #region  Operation Capacity report

        [Route("GetVehicleListByWarehouseId")]
        [HttpGet]
        public async Task<List<VehicleMasterDC>> GetVehicleListByWarehouseId(int WarehouseId)
        {
            List<VehicleMasterDC> result = new List<VehicleMasterDC>();
            using (var db = new AuthContext())
            {
                if (WarehouseId > 0)
                {
                    var list = await db.VehicleMasterDB.Where(x => x.WarehouseId == WarehouseId && x.IsDeleted == false).ToListAsync();
                    result = Mapper.Map(list).ToANew<List<VehicleMasterDC>>();
                }
                return result;
            }
        }

        /// <summary>
        /// Get Vehicle Report  base on Date range
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [Route("GetVehicleReport")]
        [HttpPost]
        public async Task<VehicleReportViewModel> GetVehicleReport(SearchVehicleReportDC search)
        {
            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            var result = await fleetMasterHelper.GetVehicleReport(search);
            return result;
        }


        [Route("ExportVehicleReport")]
        [HttpPost]
        public async Task<string> ExportVehicleReport(SearchVehicleReportDC search)
        {
            string folderName = ConfigurationManager.AppSettings["AllFileDownloadFolder"].ToString();
            string fileName = Guid.NewGuid().ToString() + ".xlsx";
            string path = HttpContext.Current.Server.MapPath("~/" + folderName + "/reports");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            FleetMasterHelper fleetMasterHelper = new FleetMasterHelper();
            var result = await fleetMasterHelper.GetVehicleReport(search);
            bool isGenerated = fleetMasterHelper.ExportVehicleReport(result, Path.Combine(path, fileName));
            if (isGenerated)
            {
                return folderName + "/reports/" + fileName;
            }
            else
            {
                return "";
            }
        }



        /// <summary>
        /// Get All Vehicle Report Of date
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [Route("GetAllVehicleReportOfDate")]
        [HttpPost]
        public async Task<List<AllVehicleReportOfDateDc>> GetAllVehicleReportOfDate(SearchAllVehicleReportDC search)
        {
            List<AllVehicleReportOfDateDc> result = new List<AllVehicleReportOfDateDc>();
            using (var context = new AuthContext())
            {
                if (search != null && search.WarehouseId > 0)
                {
                    List<Object> parameters = new List<object>();
                    parameters.Add(new SqlParameter("@WarehouseId", search.WarehouseId));
                    parameters.Add(new SqlParameter("@Date ", search.Date));
                    string sqlquery = "exec GetAllVehicleReportOfDate @WarehouseId, @Date";
                    result = await context.Database.SqlQuery<AllVehicleReportOfDateDc>(sqlquery, parameters.ToArray()).ToListAsync();
                }
                return result;
            }
        }

        #endregion


    }
    public class DboyListDC
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class VehicleListDC
    {
        public long Id { get; set; }
        public string VehicleType { get; set; }
    }
    public class TransportNameListDC
    {
        public long Id { get; set; }
        public string TransportName { get; set; }
    }
    public class DriverListDC
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class VehicleRegistrationNoLIstDC
    {
        public int Id { get; set; }
        public string RegistrationNo { get; set; }
    }
    public class VehicleMasterDC
    {
        public long Id { get; set; }
        public string FleetType { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNo { get; set; }
        public string Model { get; set; }
        public string RegistrationNo { get; set; }
        public string OwnerName { get; set; }
        public string ChasisNo { get; set; }
        public string OwnershipType { get; set; }
        public string InsuranceNo { get; set; }
        public string PUCNo { get; set; }
        public string EngineNo { get; set; }
        public string Make { get; set; }
        public double VehicleWeight { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime PUCValidTill { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        //images
        public string RegistrationImage { get; set; }
        public string InsuranceImage { get; set; }
        public string PUCimage { get; set; }
        public DateTime InsuranceValidity { get; set; }
        public string MakerName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int FleetId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string RegistrationImageBack { get; set; }
        public string TransportName { get; set; }
        public int FleetDetailId { get; set; } //
        public long FleetMasterId { get; set; } //
        public int TripTypeEnum { get; set; } //City = 0,SKP = 1,KPP = 2,Damage_Expiry = 3
        public string TripTypeName { get; set; }
    }
    public class VehicleMasterExportDC
    {
        public long Id { get; set; }
        public string FleetType { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNo { get; set; }
        public string Model { get; set; }
        public string RegistrationNo { get; set; }
        public string OwnerName { get; set; }
        public string ChasisNo { get; set; }
        public string OwnershipType { get; set; }
        public string InsuranceNo { get; set; }
        public string PUCNo { get; set; }
        public string EngineNo { get; set; }
        // public string Make { get; set; }
        public double VehicleWeight { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime PUCValidTill { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime InsuranceValidity { get; set; }
        public string MakerName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int TripTypeEnum { get; set; } //City = 0,SKP = 1,KPP = 2,Damage_Expiry = 3
        public string TripTypeName { get; set; }
        public string TransportName { get; set; }
        public int FleetDetailId { get; set; }
        
    }
    public class AllData
    {
        public int totalCount { get; set; }
        public List<VehicleMasterExportDC> VehicleMasterExportDCList { get; set; }
    }
    public class VehiclePaggingDC
    {
        public int totalCount { get; set; }
        public List<VehicleMasterDC> VehicleMasterlist { get; set; }
    }
    public class ResVehicleMasterDC
    {
        public int totalCount { get; set; }
        public List<VehicleMasterDC> VehicleMasterlist { get; set; }
        public bool Result { get; set; }
        public string msg { get; set; }
    }
    public class OutBoundDeliveryMappingDC
    {
        public int Id { get; set; }
        public int ClusterId { get; set; }
        public int AgentId { get; set; }
        public int WarehouseId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<OutBoundDeliveryDetails> outBoundDeliveryDetails { get; set; }
    }
    public class OutBoundDeliveryDetails
    {
        public long Id { get; set; }
        public long OutBoundDeliveryMappingId { get; set; }
        public long VehicleMasterId { get; set; }
        public long DriverMasterId { get; set; }
        public long DboyMasterId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class ResOutBoundDC
    {
        public bool Result { get; set; }
        public string msg { get; set; }
    }
    public class OutBoundPaggingDC
    {
        public int totalcount { get; set; }
        public List<outBoundDC> outBoundList { get; set; }
    }
    public class outBoundDC
    {
        public int Id { get; set; }
        public int ClusterId { get; set; }
        public int AgentId { get; set; }
        public int WarehouseId { get; set; }
        public string ClusterName { get; set; }
        public string AgentName { get; set; }
        public string WarehouseName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

    }
    public class OutBoundMappingDC
    {
        public int Id { get; set; }
        public int ClusterId { get; set; }
        public int AgentId { get; set; }
        public int WarehouseId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<OutBoundDeliveryDetailDC> OutBoundDeliveryDetails { get; set; }
    }
    public class OutBoundDeliveryDetailDC
    {
        public int Id { get; set; }
        public long OutBoundDeliveryMappingId { get; set; }
        public long VehicleMasterId { get; set; }
        public long DriverMasterId { get; set; }
        public long DboyMasterId { get; set; }
        public string DboyName { get; set; }
        public string DriverName { get; set; }
        public string Vehiclename { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SearchAllVehicleReportDC
    {
        public int WarehouseId { get; set; }
        public DateTime Date { get; set; }
    }

    public class SearchVehicleReportDC
    {
        public int WarehouseId { get; set; }
        public long Id { get; set; } //VehicleId
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }


    public class VehicleReportViewModel
    {
        public List<VehicleReportDC> DayWiseTripList { get; set; }
        public VehicleReportDetailDC VehicleSummary { get; set; }
    }

    public class VehicleReportDC
    {
        public DateTime? TripDate { get; set; } // CreatedDate
        public DateTime? ReportingTime { get; set; } // Starttime
        public DateTime? ClosingTime { get; set; } //
        public string HourWorked { get; set; }
        public int HourWorkedInMins { get; set; }
        public double? StartKm { get; set; } //Opening
        public double? ClosingKm { get; set; } //Closing
        public double? TotalKm { get; set; } //TotalKm
        public double? ExtraTimeInHour { get; set; }
        public double? Toll { get; set; }
        public string LateReporting { get; set; }
        public int NumberOfTrips { get; set; }
    }


    public class AllVehicleReportOfDateDc
    {
        public int TripNo { get; set; }
        public string VehicleNo { get; set; }
        public string DriverName { get; set; }
        public string Location { get; set; }
        public string VehicleType { get; set; }
        public DateTime? ClosingTime { get; set; } // Outtime
        public DateTime? ReportingTime { get; set; } // Intime
        public double? TotalKm { get; set; } //Run
        public double? TotalGpsKm { get; set; } //Run km by google
        public DateTime CreatedDate { get; set; } // CreatedDate
        public int? OrderCount { get; set; }
        public double? OrderValue { get; set; }
        public double? DeliverdValue { get; set; }
        public int? DeliverdCount { get; set; }
        public double? DeliverdPercent { get; set; }
        public double? RedispatchValue { get; set; } //order
        public int? RedispatchCount { get; set; }
        public double? RedispatchPercent { get; set; }
        public int? POCCount { get; set; }
        public double? POCValue { get; set; }
        public double? POCPercent { get; set; }
        public int? DCCount { get; set; }
        public double? DCValue { get; set; }
        public double? DCPercent { get; set; }

    }
    public class VehicleReportDetailDC
    {
        public string VehicleNo { get; set; }
        public string Location_StationCode { get; set; }
        public string ClientName { get; set; }
        public string Month { get; set; }
        public string Driver { get; set; }
        public double ContractHours { get; set; }
        public int ContractDays { get; set; }
        public double ContractKMs { get; set; }
        public int GPSStatus { get; set; }
        public int ExtraDay { get; set; }
        public double ExtraKM { get; set; }
        public double Extrahours { get; set; }
        public string VehicleType { get; set; }
        public double TotalKM { get; set; }

        public double RemainingKM { get; set; }
        public double ExtraKMCharged { get; set; }
        public double PerExtraKmCharge { get; set; }
        public double FixedCost { get; set; }
        public double PerExtraHrCharge { get; set; }
        public double ExtraHrsCharge { get; set; }
        public double ExtraCost { get; set; }
    }
}
