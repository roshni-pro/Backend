using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.BusinessLayer.Managers.TripPlanner;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/DeliveyMapping")]
    public class DeliveyMappingController : BaseApiController
    {

        [Route("GetCityList")]
        public List<PrimeNgDropDown<int?>> GetClusterList()
        {
            using (var authContext = new AuthContext())
            {

                int userid = GetLoginUserId();                
                if (userid > 0)
                {
                    var warehouses = authContext.WarehousePermissionDB.Where(x => x.PeopleID == userid && x.IsActive == true && x.IsDeleted == false).ToList();
                    var warehouseIds = warehouses.Select(x => x.WarehouseId).ToList();

                    var query = from w in authContext.Warehouses
                                join c in authContext.Cities on w.Cityid equals c.Cityid
                                join cl in authContext.Clusters on w.WarehouseId equals cl.WarehouseId 
                                where w.active == true && w.Deleted != true && w.IsKPP == false
                                && warehouseIds.Contains(w.WarehouseId) && w.IsDeliveryOptimizationEnabled==true
                                group new {w,c,cl} by new {c.Cityid,c.CityName} into abc
                                orderby abc.Key.CityName
                                select new PrimeNgDropDown<int?>
                                {
                                    value = abc.Key.Cityid,
                                    label = abc.Key.CityName
                                };

                    var result = query.Distinct().ToList();
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        [HttpGet]
        [Route("GetCityIsCommon")]
        public List<PrimeNgDropDown<int?>> GetCityIsCommon()
        {
            using (var db = new AuthContext())
            {
                int userid = GetLoginUserId();
                var userIds = new SqlParameter("@userId", userid);
                var result = db.Database.SqlQuery<PrimeNgDropDown<int?>>("CityGetCommon @userId", userIds).ToList();
                return result;
            }
        }

        [HttpGet]
        [Route("GetCityByRegion")]
        public List<PrimeNgDropDown<int?>> GetCityByRegion(int RegionId)
        {
            using (var db = new AuthContext())
            {
                int userid = GetLoginUserId();
                var userIds = new SqlParameter("@userId", userid);
                var regionid = new SqlParameter("@regionId", RegionId);
                var result = db.Database.SqlQuery<PrimeNgDropDown<int?>>("CityGetCommonByRegion @userId,@regionId", userIds, regionid).ToList();
                return result;
            }
        }
        [HttpPost]
        [Route("WarehouseGetByCityListCommon")]
        public List<PrimeNgDropDown<int?>> WarehouseGetByCityListCommon(List<int> Cities)
        {
            using (var db = new AuthContext())
            {
                var CityIdIdDts = new DataTable();
                CityIdIdDts.Columns.Add("IntValue");

                if (Cities != null && Cities.Any())
                {
                    foreach (var item in Cities)
                    {
                        var dr = CityIdIdDts.NewRow();
                        dr["IntValue"] = item;
                        CityIdIdDts.Rows.Add(dr);
                    }
                }
                var Daysparam = new SqlParameter("cityId", CityIdIdDts);
                Daysparam.SqlDbType = SqlDbType.Structured;
                Daysparam.TypeName = "dbo.IntValues";

                int userid = GetLoginUserId();
                var userIds = new SqlParameter("@userId", userid);
                var result = db.Database.SqlQuery<PrimeNgDropDown<int?>>("exec WarehouseGetByCityListCommon @userId,@cityId", userIds, Daysparam).ToList();
                return result;
            }
        }

        [Route("GetWarehoueList/{cityId}")]
        public List<PrimeNgDropDownAddition<int?, int>> GetWarehoueList(int cityId)
        {
            using (var authContext = new AuthContext())
            {
                int userid = GetLoginUserId();
                if (userid > 0)
                {
                    var warehouses = authContext.WarehousePermissionDB.Where(x => x.PeopleID == userid && x.IsActive == true && x.IsDeleted == false).ToList();
                    var warehouseIds = warehouses.Select(x => x.WarehouseId).ToList();

                    var query = from w in authContext.Warehouses
                                join cl in authContext.Clusters on w.WarehouseId equals cl.WarehouseId
                                where w.active == true && w.Deleted == false && w.IsKPP == false && w.Cityid == cityId
                                && warehouseIds.Contains(w.WarehouseId) && w.IsDeliveryOptimizationEnabled == true
                                group new { w,cl } by new { w.WarehouseId, w.WarehouseName, w.StoreType } into abc
                                orderby abc.Key.WarehouseName
                                select new PrimeNgDropDownAddition<int?, int>
                                {
                                    value = abc.Key.WarehouseId,
                                    label = abc.Key.WarehouseName,
                                    AdditionalField = abc.Key.StoreType
                                };

                    var result = query.ToList();
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        [HttpGet]
        [Route("GetWarehouseIsCommon")]
        public List<PrimeNgDropDown<int?>> GetWarehouseIsCommon()
        {
            int userid = GetLoginUserId();
            using (var db = new AuthContext())
            {
                var userIds = new SqlParameter("@userid", userid);
                var result = db.Database.SqlQuery<PrimeNgDropDown<int?>>("WarehouseGetCommon @userid", userIds).ToList();
                return result;
            }
        }

        [HttpGet]
        [Route("GetStoreWarehouse")]
        public List<PrimeNgDropDown<int?>> GetStoreWarehouse()
        {
            int userid = GetLoginUserId();
            using (var db = new AuthContext())
            {
                var userIds = new SqlParameter("@userid", userid);
                var result = db.Database.SqlQuery<PrimeNgDropDown<int?>>("GetStoreWarehouse @userid", userIds).ToList();
                return result;
            }
        }

        [HttpGet]
        [Route("GetWarehouseCommonByRegion")]
        public List<PrimeNgDropDown<int?>> GetWarehouseCommonByRegion(int RegionId)
        {
            using (var db = new AuthContext())
            {
                int userid = GetLoginUserId();
                var userIds = new SqlParameter("@userid", userid);
                var regionid = new SqlParameter("@regionId", RegionId);
                var result = db.Database.SqlQuery<PrimeNgDropDown<int?>>("WarehouseGetCommonByRegion @userid,@regionId", userIds, regionid).ToList();
                return result;
            }
        }

        [HttpGet]
        [Route("GetWarehouseIsCommonByCityId")]
        public List<PrimeNgDropDown<int?>> GetWarehouseIsCommonByCityId(int cityid)
        {
            int userid = GetLoginUserId();
            using (var db = new AuthContext())
            {
                var userIds = new SqlParameter("@userid", userid);
                var cityIds = new SqlParameter("@cityId", cityid);
                var result = db.Database.SqlQuery<PrimeNgDropDown<int?>>("WarehouseGetCommon @userid,@cityId", userIds, cityIds).ToList();
                return result;
            }
        }


        [Route("OrderStatusCountList")]
        [HttpGet]
        public DeliveryMappingstatusCount OrderStatusCountList(int warehoueId)
        {
            List<OrderstatusCountListDC> list = new List<OrderstatusCountListDC>();
            DeliveryMappingstatusCount obj = new DeliveryMappingstatusCount();
            using (var authContext = new AuthContext())
            {
                if (warehoueId > 0)
                {
                    var warehouseId = new SqlParameter("@WarehouseId", warehoueId);
                    list = authContext.Database.SqlQuery<OrderstatusCountListDC>("Operation.TripPlanner_OrderStatusCountList @WarehouseId", warehouseId).ToList();

                    obj.NumberofOrder = list.Count();
                    obj.NotCompletedOrder = list.Where(x => x.Status == "Issued" || x.Status == "Shipped" || x.Status == "Ready to Dispatch").Count();
                    obj.DeliveredOrder = list.Where(x => x.Status == "Delivered").Count();
                    obj.RedispatchOrder = list.Where(x => x.Status == "Delivery Redispatch").Count();
                    obj.CancelledOrder = list.Where(x => x.Status == "Delivery Canceled" || x.Status == "Post Order Canceled").Count();
                }
                return obj;
            }
        }

        [Route("NumberOfOrderClusterWise")]
        [HttpGet]
        public List<ClusterWiseOrderCount> NumberOfOrderClusterWise(int warehoueId, int flage)
        {
            using (var authContext = new AuthContext())
            {
                List<ClusterWiseOrderCount> list = new List<ClusterWiseOrderCount>();
                var warehoueIds = new SqlParameter("@WarehouseId", warehoueId);
                var Flage = new SqlParameter("@Flage", flage);

                list = authContext.Database.SqlQuery<ClusterWiseOrderCount>("NumberOfOrderClusterWise @WarehouseId,@Flage", warehoueIds, Flage).ToList();
                return list;
            }
        }
        [Route("NumberOfOrderClusterWiseOrderlist")]
        [HttpGet]
        public async Task<ResponseDc> NumberOfOrderClusterWiseOrderlist(int WarehouseId, int flage, int Skip, int Take)
        {
            using (var authContext = new AuthContext())
            {
                int Skiplist = (Skip - 1) * Take;
                ResponseDc obj = new ResponseDc();
                List<OrderDC> orderlist = new List<OrderDC>();
                var Status = "";
                TripPlannerManager tripPlannerManager = new TripPlannerManager();
                var lists = await tripPlannerManager.NumberOfOrderClusterWiseOrderlist(WarehouseId, flage, Skiplist, Take);
                List<int> orderids = lists.orderlists.Select(x => (int)x.OrderId).ToList();
                if (flage > 0)
                {
                    var orderlists = authContext.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).Select(x => new
                    {
                        x.OrderId,
                        x.invoice_no,
                        x.ShopName,
                        x.Customerphonenum,
                        x.Skcode,
                        x.BillingAddress,
                        x.Deliverydate,
                        x.GrossAmount,
                        x.ReDispatchCount
                    }).ToList();
                    orderlist = Mapper.Map(orderlists).ToANew<List<OrderDC>>();

                }
                else
                {
                    var list = authContext.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).Select(x => new
                    {
                        x.OrderId,
                        x.invoice_no,
                        x.ShopName,
                        x.Customerphonenum,
                        x.Skcode,
                        x.BillingAddress,
                        x.Deliverydate,
                        x.GrossAmount,
                        x.ReDispatchCount
                    }).ToList();
                    orderlist = Mapper.Map(list).ToANew<List<OrderDC>>();
                }
                obj.listOrder = orderlist;
                obj.TotalCount = lists.TotalCount;
                return obj;
            }
        }



        //[Route("CreateAssignmnetDelivery")]
        //[HttpGet]
        //public HttpResponseMessage CreateAssginment(long TripPlannerMasterId)
        //{
        //    var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
        //    int userid = 0;

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    ResMsg res;
        //    string Msg = "";
        //    bool status = true;
        //    if (TripPlannerMasterId > 0 && userid > 0)
        //    {

        //        TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
        //        Msg = tripPlannerHelper.AssignmentCreateUpdate(TripPlannerMasterId, userid);
        //        status = true;
        //    }
        //    res = new ResMsg()
        //    {
        //        Status = status,
        //        Message = Msg
        //    };
        //    return Request.CreateResponse(HttpStatusCode.OK, res);
        //}

        [Route("AssignmentFinalize")]
        [HttpGet]
        public HttpResponseMessage AssignmentFinalize(int TripPlannerMasterId)
        {
            ResponceMsg res = new ResponceMsg();
            //string Msg = "something went wrong!!";
            //bool status = false;
            //var identity = User.Identity as System.Security.Claims.ClaimsIdentity;
            //int userid = 0;
            //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
            //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            //if (TripPlannerMasterId > 0 && userid > 0)
            //{
            //    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            //    res = tripPlannerHelper.AssignmentFinalize(TripPlannerMasterId, userid, authContext);
            //}
            //else
            //{
            //    res.Status = status;
            //    res.Message = Msg;
            //}            
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("GetVehicleLiveDetails")]
        [HttpGet]
        public VehicleLiveDetailsDc GetVehicleLiveDetails(int WarehouseId)
        {
            VehicleLiveDetailsDc vehicleLiveDetailsDc = new VehicleLiveDetailsDc();
            using (var context = new AuthContext())
            {
                if (WarehouseId > 0)
                {
                    var warehouseId = new SqlParameter("@WarehouseId", WarehouseId);
                    List<VehicleLiveDetailslistDc> list = context.Database.SqlQuery<VehicleLiveDetailslistDc>("Operation.TripPlanner_GetVehicleLiveDetail @WarehouseId", warehouseId).ToList();

                    vehicleLiveDetailsDc.OnDuty = new SingleVehicle
                    {
                        Id = (int)VehicleliveStatus.OnDuty,
                        Count = list.Where(x => x.CurrentStatus != (int)VehicleliveStatus.TripEnd).Count()
                    };


                    vehicleLiveDetailsDc.NotStarted = new SingleVehicle
                    {
                        Id = (int)VehicleliveStatus.NotStarted,
                        Count = list.Where(x => x.CurrentStatus == (int)VehicleliveStatus.NotStarted).Count()
                    };

                    vehicleLiveDetailsDc.Intransit = new SingleVehicle
                    {
                        Id = (int)VehicleliveStatus.InTransit,
                        Count = list.Where(x => x.CurrentStatus == (int)VehicleliveStatus.InTransit).Count()
                    };


                    vehicleLiveDetailsDc.OnBreak = new SingleVehicle
                    {
                        Id = (int)VehicleliveStatus.OnBreak,
                        Count = list.Where(x => x.CurrentStatus == (int)VehicleliveStatus.OnBreak).Count()
                    };

                    vehicleLiveDetailsDc.Delivering = new SingleVehicle
                    {
                        Id = (int)VehicleliveStatus.Delivering,
                        Count = list.Where(x => x.CurrentStatus == (int)VehicleliveStatus.Delivering).Count()
                    };

                    vehicleLiveDetailsDc.TripEnd = new SingleVehicle
                    {
                        Id = (int)VehicleliveStatus.TripEnd,
                        Count = list.Where(x => x.CurrentStatus == (int)VehicleliveStatus.TripEnd).Count()
                    };

                }
                return vehicleLiveDetailsDc;
            }
        }

        [Route("GetVehicleLiveDetailsList")]
        [HttpGet]
        [AllowAnonymous]
        public List<TripListDc> GetVehicleLiveDetailsList(int WarehouseId, int CurrentStatus)
        {
            using (var context = new AuthContext())
            {
                context.Database.Log = log => Debug.WriteLine(log);
                var query = from tcm in context.TripPlannerConfirmedMasters
                                //join c in context.Clusters on tcm.ClusterId equals c.ClusterId
                            join tpv in context.TripPlannerVehicleDb on tcm.Id equals tpv.TripPlannerConfirmedMasterId
                            join v in context.VehicleMasterDB on tcm.VehicleMasterId equals v.Id
                            //join p in context.Peoples on tcm.AgentId equals p.PeopleID
                            join d in context.DriverMasters on tcm.DriverId equals d.Id
                            join dboy in context.DboyMasters on tcm.DboyId equals dboy.Id
                            where tcm.WarehouseId == WarehouseId
                            && (
                                    (CurrentStatus == (int)VehicleliveStatus.OnDuty && tpv.CurrentStatus != (int)VehicleliveStatus.TripEnd && tpv.CurrentStatus != (int)VehicleliveStatus.RejectTrip)
                                    || (CurrentStatus == (int)VehicleliveStatus.TripEnd && tpv.CurrentStatus == CurrentStatus && tpv.ModifiedDate > DateTime.Today)
                                    || (CurrentStatus != (int)VehicleliveStatus.OnDuty && CurrentStatus != (int)VehicleliveStatus.TripEnd && tpv.CurrentStatus == CurrentStatus)
                               )
                            && tcm.IsActive == true && tcm.IsDeleted == false
                            //&& tcm.TripDate == DateTime.Today
                            select new TripListDc
                            {
                                TripId = tcm.TripPlannerMasterId,
                                TripNumber = tcm.TripNumber,
                                //ClusterId = c.ClusterId,
                                //ClusterName = c.ClusterName,
                                TripDetails = new TripDetailDc()
                                {
                                    TripNumber = tcm.TripNumber,
                                    VehicleName = v.VehicleType,
                                    //ClusterName = c.ClusterName,
                                    //AgentName = p.DisplayName,
                                    //AgentMobileNumber = p.Mobile,
                                    DriverName = d.Name,
                                    DriverMobileNumber = d.MobileNo,
                                    DeliveryBoyName = dboy.Name,
                                    DeliveryBoyNumber = dboy.MobileNo
                                }
                            };


                var list = query.ToList<TripListDc>();
                return list;
            }
            //return null;
        }
    }
}
