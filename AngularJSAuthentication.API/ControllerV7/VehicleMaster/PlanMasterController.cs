using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.Model.TripPlanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using System.Transactions;
using System.Data.SqlClient;
using NLog;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers.TripPlanner;
using System.Threading.Tasks;
using AngularJSAuthentication.Common.Helpers.ReportMaker;
using System.Configuration;
using System.IO;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.DataContracts.Transaction.OrderProcess;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.API.Results;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Security.Claims;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.BusinessLayer.Managers.TripPlanner;
using System.IO.Compression;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.Common.Constants;
using Nito.AsyncEx.Synchronous;

namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/PlanMaster")]
    public class PlanMasterController : BaseApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("GetWarehouseList")]
        public List<PrimeNgDropDown<int?>> GetWarehouseList()
        {
            using (var authContext = new AuthContext())
            {
                var query = from w in authContext.Warehouses
                            join c in authContext.Cities on w.Cityid equals c.Cityid
                            join cl in authContext.Clusters on w.WarehouseId equals cl.WarehouseId
                            where c.active == true && c.Deleted != true && w.active == true && w.Deleted != true
                            && w.IsKPP == false && w.IsDeliveryOptimizationEnabled == true
                            group new { w, c, cl } by new { w.WarehouseId, c.CityName, w.WarehouseName } into abc
                            orderby abc.Key.CityName
                            select new PrimeNgDropDown<int?>
                            {
                                value = abc.Key.WarehouseId,
                                label = abc.Key.CityName + " (" + abc.Key.WarehouseName + ")"
                            };

                var result = query.ToList();
                result.Insert(0, new PrimeNgDropDown<int?>
                {
                    label = "Select Warehouse",
                    value = null
                });
                return result;
            }
        }

        [Route("GetClusterList/{warehouseId}")]
        public List<PrimeNgDropDown<int?>> GetClusterList(int warehouseId)
        {
            using (var authContext = new AuthContext())
            {
                var query = from c in authContext.Clusters
                            where c.Active == true && c.Deleted != true && c.WarehouseId == warehouseId
                            //&& c.IsAutoOrderEnable == true
                            orderby c.ClusterName
                            select new PrimeNgDropDown<int?>
                            {
                                value = c.ClusterId,
                                label = c.ClusterName
                            };

                var result = query.ToList();
                result.Insert(0, new PrimeNgDropDown<int?>
                {
                    label = "Select Cluster",
                    value = null
                });
                return result;
            }
        }

        [Route("GetTrip/{tripMasterId}")]
        public TripPlannerConfirmedMasterVM GetTrip(int tripMasterId)
        {
            using (var authContext = new AuthContext())
            {
                var query = from tm in authContext.TripPlannerMasters
                            join vm in authContext.VehicleMasterDB on tm.VehicleMasterId equals vm.Id
                            join fd in authContext.FleetMasterDetailDB on vm.FleetDetailId equals fd.Id
                            join fm in authContext.FleetMasterDB on fd.FleetMasterId equals fm.Id
                            join tcm in authContext.TripPlannerConfirmedMasters
                                on tm.Id equals tcm.TripPlannerMasterId into gj
                            from subset in gj.DefaultIfEmpty()
                            where tm.Id == tripMasterId && tm.IsActive == true && tm.IsDeleted == false
                            // && tm.TripDate == DateTime.Today
                            select new TripPlannerConfirmedMasterVM
                            {
                                WarehouseId = (subset != null ? subset.WarehouseId : tm.WarehouseId),
                                CustomerCount = (subset != null ? subset.CustomerCount : tm.CustomerCount),
                                DboyId = (subset != null ? subset.DboyId : tm.DboyId),
                                Id = (subset != null ? subset.Id : 0),
                                OrderCount = (subset != null ? subset.OrderCount : tm.OrderCount),
                                TotalAmount = (subset != null ? subset.TotalAmount : tm.TotalAmount),
                                TotalDistanceInMeter = (subset != null ? subset.TotalDistanceInMeter : tm.TotalDistanceInMeter),
                                TotalTimeInMins = (subset != null ? subset.TotalTimeInMins : tm.TotalTimeInMins),
                                TotalWeight = (subset != null ? subset.TotalWeight : tm.TotalWeight),
                                TripDate = (subset != null ? subset.TripDate : tm.TripDate),
                                TripNumber = (subset != null ? subset.TripNumber : tm.TripNumber),
                                TripPlannerMasterId = (subset != null ? subset.TripPlannerMasterId : tm.Id),
                                VehicleNumber = (subset != null ? subset.VehicleNumber : tm.VehicleNumber),
                                WarehouseLat = (subset != null ? subset.WarehouseLat : tm.WarehouseLat),
                                WarehouseLng = (subset != null ? subset.WarehouseLng : tm.WarehouseLng),
                                //VehicleCapacityInKg = (long)fd.VehicleCapacity,
                                VehicleCapacityInKg = 1500,
                                VehicleType = "",//fd.VehicleType,
                                AgentId = (subset != null ? subset.AgentId : null),
                                DriverId = (subset != null ? subset.DriverId : null),
                                VehicleId = (subset != null ? subset.VehicleMasterId : tm.VehicleMasterId),
                                IsFreezed = (subset != null ? subset.IsFreezed : false),
                                TripPlannerConfirmMasterId = (subset != null ? subset.Id : 0),
                                IsVisibleToDboy = (subset != null ? subset.IsVisibleToDboy : false),
                                IsPickerGenerated = (subset != null ? subset.IsPickerGenerated : false),
                                IsNotLastMileTrip = (subset != null ? subset.IsNotLastMileTrip : false),
                                TripTypeEnum = (subset != null ? subset.TripTypeEnum : 0),
                                FleetType = fm.FleetType
                            };


                var result = query.FirstOrDefault();
                result = result != null ? result : new TripPlannerConfirmedMasterVM();


                if (result != null)
                {

                    var q = from tm in authContext.TripPlannerConfirmedMasters
                            join vm in authContext.VehicleMasterDB on tm.VehicleMasterId equals vm.Id
                            join fd in authContext.FleetMasterDetailDB on vm.FleetDetailId equals fd.Id
                            join fm in authContext.FleetMasterDB on fd.FleetMasterId equals fm.Id
                            where tm.TripPlannerMasterId == tripMasterId
                            select fm;

                    var trip = q.FirstOrDefault();

                    result.FleetType = trip != null && !string.IsNullOrEmpty(trip.FleetType) ? trip.FleetType : result.FleetType;


                }
                if (result == null || result.VehicleId == 0)
                {
                    result.VehicleId = null;
                }
                if (result != null)
                {
                    //result.IsPickerGenerated = false;
                    var vehicle = authContext.TripPlannerVehicleDb.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == result.TripPlannerConfirmMasterId && x.IsActive == true && x.IsDeleted == false);

                    if (vehicle != null)
                    {
                        result.StartKm = vehicle.StartKm;
                        result.ReportingTime = vehicle.ReportingTime;
                        result.LateReportingTimeInMins = vehicle.LateReportingTimeInMins;
                        result.PenaltyCharge = vehicle.PenaltyCharge;
                    }
                    if (result.TripPlannerConfirmMasterId > 0)
                    {
                        TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();

                        if (result.TripTypeEnum != (int)TripTypeEnum.Damage_Expiry && result.TripTypeEnum != (int)TripTypeEnum.NonSellable)
                        {
                            result.IsPickerRequired = tripPlannerHelper.GetIsPickerRequired(result.TripPlannerConfirmMasterId, authContext);
                        }
                        else
                        {
                            result.IsPickerRequired = false;
                        }

                        var mappingList = authContext.TripPickerAssignmentMapping.Where(x => x.TripPlannerConfirmedMasterId == result.TripPlannerConfirmMasterId && x.IsActive == true && x.IsDeleted == false).ToList();

                        if (mappingList != null && mappingList.Any())
                        {
                            result.OrderPickerMasterId = mappingList.FirstOrDefault(x => x.OrderPickerMasterId != null)?.OrderPickerMasterId;
                            result.PickerIdList = mappingList.Where(x => x.OrderPickerMasterId != null).Select(y => y.OrderPickerMasterId.Value).ToList();

                            result.DeliveryIssuanceIdList = mappingList.Where(x => x.AssignmentId != null).Select(x => x.AssignmentId.Value).ToList();
                        }
                    }

                }
                return result;
            }
        }

        [HttpGet]
        [Route("GetTripV1/{tripMasterId}")]
        public TripPlannerConfirmedMasterVM GetTripV1(long tripMasterId)
        {
            using (var authContext = new AuthContext())
            {
                //var tripMasterIdParam = new SqlParameter
                //{
                //    ParameterName = "tripMasterId",
                //    Value = tripMasterId
                //};
                var tripMasterIdParam = new SqlParameter("tripMasterId", (long)tripMasterId);
                tripMasterIdParam.SqlDbType = SqlDbType.BigInt;
                var result = authContext.Database.SqlQuery<TripPlannerConfirmedMasterVM>("exec Operation.TripPlanner_GetTripList @tripMasterId", tripMasterIdParam).FirstOrDefault();
                result = result != null ? result : new TripPlannerConfirmedMasterVM();


                if (result != null)
                {

                    var q = from tm in authContext.TripPlannerConfirmedMasters
                            join vm in authContext.VehicleMasterDB on tm.VehicleMasterId equals vm.Id
                            join fd in authContext.FleetMasterDetailDB on vm.FleetDetailId equals fd.Id
                            join fm in authContext.FleetMasterDB on fd.FleetMasterId equals fm.Id
                            where tm.TripPlannerMasterId == tripMasterId
                            select fm;

                    var trip = q.FirstOrDefault();

                    result.FleetType = trip != null && !string.IsNullOrEmpty(trip.FleetType) ? trip.FleetType : result.FleetType;


                }
                if (result == null || result.VehicleId == 0)
                {
                    result.VehicleId = null;
                }
                if (result != null)
                {
                    //result.IsPickerGenerated = false;
                    var vehicle = authContext.TripPlannerVehicleDb.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == result.TripPlannerConfirmMasterId && x.IsActive == true && x.IsDeleted == false);

                    if (vehicle != null)
                    {
                        result.StartKm = vehicle.StartKm;
                        result.ReportingTime = vehicle.ReportingTime;
                        result.LateReportingTimeInMins = vehicle.LateReportingTimeInMins;
                        result.PenaltyCharge = vehicle.PenaltyCharge;
                    }
                    if (result.TripPlannerConfirmMasterId > 0)
                    {
                        TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();

                        if (result.TripTypeEnum != (int)TripTypeEnum.Damage_Expiry && result.TripTypeEnum != (int)TripTypeEnum.NonSellable)
                        {
                            result.IsPickerRequired = tripPlannerHelper.GetIsPickerRequired(result.TripPlannerConfirmMasterId, authContext);
                        }
                        else
                        {
                            result.IsPickerRequired = false;
                        }

                        var mappingList = authContext.TripPickerAssignmentMapping.Where(x => x.TripPlannerConfirmedMasterId == result.TripPlannerConfirmMasterId && x.IsActive == true && x.IsDeleted == false).ToList();

                        if (mappingList != null && mappingList.Any())
                        {
                            result.OrderPickerMasterId = mappingList.FirstOrDefault(x => x.OrderPickerMasterId != null)?.OrderPickerMasterId;
                            result.PickerIdList = mappingList.Where(x => x.OrderPickerMasterId != null).Select(y => y.OrderPickerMasterId.Value).ToList();

                            result.DeliveryIssuanceIdList = mappingList.Where(x => x.AssignmentId != null).Select(x => x.AssignmentId.Value).ToList();
                        }
                    }

                }
                return result;
            }
        }

        [Route("GetTripList/{warehouseId}")]
        public List<PrimeNgDropDown<long?>> GetTripList(int warehouseId)
        {
            TripPlannerConfirmedMasterManager manager = new TripPlannerConfirmedMasterManager();

            var list = manager.GetTripAllList(warehouseId);
            return list;
        }

        [Route("GetTripPlannerOrderList/{tripPlannerMasterId}")]
        public List<TripPlannerConfirmedOrderVM> GetTripPlannerOrderList(long tripPlannerMasterId)
        {
            using (var authContext = new AuthContext())
            {
                var query = from o in authContext.TripPlannerOrders
                            join d in authContext.TripPlannerDetails on o.TripPlannerDetailId equals d.Id
                            join m in authContext.TripPlannerMasters on d.TripMasterId equals m.Id
                            join c in authContext.Customers on d.CustomerId equals c.CustomerId
                            join od in authContext.DbOrderMaster on o.OrderId equals od.OrderId
                            where m.Id == tripPlannerMasterId && m.IsActive == true
                                && m.IsDeleted == false && d.IsActive == true && d.IsDeleted == false && m.TripDate == DateTime.Today
                            select new TripPlannerConfirmedOrderVM
                            {
                                OrderId = (int)o.OrderId,
                                CustomerName = c.Name,
                                ShippingAddress = od.OrderType == 11 ? od.ShippingAddress : c.ShippingAddress,
                                Amount = o.Amount,
                                TimeInMins = o.TimeInMins,
                                DistanceInMeter = o.DistanceInMeter,
                                TripPlannerConfirmedOrderId = 0,
                                IsActive = true,
                                TripPlannerConfirmedDetailId = 0,
                                IsActiveOld = true
                            };
                return query.ToList<TripPlannerConfirmedOrderVM>();
            }
        }
        [Route("GetTripPlannerConfirmedOrderList/{tripPlannerConfirmedMasterId}")]
        public List<TripPlannerConfirmedOrderVM> GetTripPlannerConfirmedOrderList(long tripPlannerConfirmedMasterId)
        {
            List<TripPlannerConfirmedOrderVM> orderList = null;

            using (var authContext = new AuthContext())
            {
                var query = from o in authContext.TripPlannerConfirmedOrders
                            join d in authContext.TripPlannerConfirmedDetails on o.TripPlannerConfirmedDetailId equals d.Id
                            join m in authContext.TripPlannerConfirmedMasters on d.TripPlannerConfirmedMasterId equals m.Id
                            join c in authContext.Customers on d.CustomerId equals c.CustomerId
                            join cl in authContext.Clusters on c.ClusterId equals cl.ClusterId
                            join odm in authContext.DbOrderMaster on o.OrderId equals odm.OrderId
                            where m.Id == tripPlannerConfirmedMasterId && m.IsActive == true
                                && m.IsDeleted == false && d.IsActive == true && d.IsDeleted == false && o.IsActive == true && o.IsDeleted == false
                            select new TripPlannerConfirmedOrderVM
                            {
                                OrderId = (int)o.OrderId,
                                CustomerName = c.Name,
                                ClusterName = cl.ClusterName,
                                ClusterId = cl.ClusterId,
                                ShippingAddress = odm.OrderType == 11 ? odm.ShippingAddress : c.ShippingAddress,
                                Amount = odm.GrossAmount,
                                TimeInMins = o.TimeInMins,
                                DistanceInMeter = o.DistanceInMeter,
                                TripPlannerConfirmedOrderId = o.Id,
                                IsActive = o.IsActive,
                                TripPlannerConfirmedDetailId = d.Id,
                                IsActiveOld = o.IsActive,
                                IsManuallyAdded = o.IsManuallyAdded,
                                Mobile = c.Mobile,
                                Skcode = c.Skcode,
                                CustomerId = d.CustomerId,
                                ShopName = odm.ShopName,
                                ReDispatchCount = odm.ReDispatchCount > 0 ? "-(R)" : "",
                                OrderDate = odm.CreatedDate,
                                WeightInKg = o.WeightInKg,
                                Status = odm.Status,
                                IsNewPickerOrder = o.IsNewPickerOrder,
                                CustomerType = c.CustomerType
                            };
                orderList = query.ToList();

                var pickerQuery = from mp in authContext.TripPickerAssignmentMapping
                                  join pkd in authContext.OrderPickerDetailsDb on mp.OrderPickerMasterId equals pkd.OrderPickerMasterId
                                  where mp.OrderPickerMasterId != null && mp.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId
                                  && mp.IsActive == true && mp.IsDeleted == false
                                  select new PickerOrderVM { OrderId = pkd.OrderId, OrderPickerMasterId = pkd.OrderPickerMasterId };
                var pickerOrderList = pickerQuery.ToList();

                if (pickerOrderList != null && pickerOrderList.Count() > 0)
                {
                    foreach (var item in pickerOrderList)
                    {
                        var ord = orderList.FirstOrDefault(x => x.OrderId == item.OrderId);
                        if (ord != null)
                        {
                            ord.OrderPickerMasterId = item.OrderPickerMasterId;
                        }
                    }
                }
            }

            return orderList;
        }

        [HttpPost]
        [Route("GetAllOrderList")]
        public TripPlannerOrderPageResult GetAllOrderList(TripPlannerOrderPager pager)
        {
            TripPlannerOrderPageResult orderList = new TripPlannerOrderPageResult();
            TripPlannerConfirmedMasterManager manager = new TripPlannerConfirmedMasterManager();
            using (var authContext = new AuthContext())
            {
                orderList = manager.GetAllOrderList(authContext, pager);
            }
            if (orderList != null && orderList.OrderList.Any())
            {
                var SkcodeList = orderList.OrderList.Select(x => x.Skcode).Distinct().ToList();
                CRMManager cRMManager = new CRMManager();
                var taglist = cRMManager.GetCRMCustomerWithTag(SkcodeList, CRMPlatformConstants.LastMilePlanning).WaitAndUnwrapException();

                foreach (var item in orderList.OrderList)
                {
                    if (taglist != null && taglist.Any())
                    {
                        var tag = taglist.Where(x => x.Skcode == item.Skcode).FirstOrDefault();
                        if (tag != null)
                        {
                            item.CRMTags = tag.CRMTags;
                        }
                    }

                }
            }

            return orderList;
        }



        [HttpGet]
        [Route("SaveTripPlannerConfirmedMaster/{tripPlannerMasterId}")]
        public long SaveTripPlannerConfirmedMaster(long tripPlannerMasterId)
        {
            var userId = GetLoginUserId();
            long id = 0;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var authContext = new AuthContext())
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    id = tripPlannerHelper.SaveTripPlannerConfirmedMaster(tripPlannerMasterId, userId, authContext, TripTypeEnum.City);
                    if (id > 0)
                    {
                        scope.Complete();
                    }
                    else
                    {
                        scope.Dispose();
                    }
                }
            }
            return id;
        }

        [HttpPost]
        [Route("UpdateOrder/{tripPlannerConfirmedMasterId}/{IsNewPickerOrder}")]
        public APIResponse UpdateOrder([FromUri] long tripPlannerConfirmedMasterId, [FromUri] bool IsNewPickerOrder, [FromBody] List<TripPlannerConfirmedOrderVM> orderList)
        {
            if (orderList != null && orderList.Where(x => x.IsActive).Count() < 1)
            {
                return new APIResponse { Status = false, Message = "Data Not Saved", Data = new List<string>() };
                //return false;
            }

            var orderIdList = orderList.Select(x => x.OrderId).Distinct().ToList();
            bool IsAlreadyOrder = false;
            var userId = GetLoginUserId();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var authContext = new AuthContext())
                {

                    var custids = orderList.Select(x => x.CustomerId).Distinct().ToList();
                    var dt = new DataTable();
                    dt.Columns.Add("IntValue");
                    foreach (var ctid in custids)
                    {
                        var dr = dt.NewRow();
                        dr["IntValue"] = ctid;
                        dt.Rows.Add(dr);
                    }
                    List<string> AppSKcodess = new List<string>();
                    List<string> PendingSKcodess = new List<string>();
                    var param = new SqlParameter("customerids", dt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var res = authContext.Database.SqlQuery<GetCustomersByIdDC>("exec [Picker].[GetCustomersByIds] @customerids", param).ToList();

                    foreach (var item in res)
                    {
                        if (item.IsGstRequestPending)
                        {
                            PendingSKcodess.Add(item.Skcode);
                        }
                        else
                            AppSKcodess.Add(item.Skcode);
                    }
                    if (PendingSKcodess.Count > 0)
                    {
                        //string msg = "";
                        //foreach(var data in PendingSKcodess)
                        //{
                        //    msg = data + ", ";
                        //}
                        return new APIResponse { Status = false, Message = "Data Not Saved", Data = PendingSKcodess };
                    }
                    else
                    {
                        var orderids = orderList.Where(x => x.TripPlannerConfirmedOrderId == 0 && x.IsActive == true).Select(x => x.OrderId).ToList();
                        if (orderids != null && orderids.Any())
                        {
                            var IdDt = new DataTable();
                            IdDt = new DataTable();
                            IdDt.Columns.Add("IntValue");
                            foreach (var item in orderids)
                            {
                                DataRow dr = IdDt.NewRow();
                                dr["IntValue"] = item;
                                IdDt.Rows.Add(dr);
                            }
                            var OrderIds = new SqlParameter
                            {
                                ParameterName = "@OrderIds",

                                SqlDbType = SqlDbType.Structured,
                                TypeName = "dbo.IntValues",
                                Value = IdDt
                            };
                            var tripPlannerConfirmedMasterIds = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId);
                            //if (orderids.Count() == orderList.Count())
                            //{
                            IsAlreadyOrder = authContext.Database.SqlQuery<bool>("EXEC [Operation].TripPlanner_GetTripDuplicateOrderCheck @OrderIds,@TripPlannerConfirmedMasterId", OrderIds, tripPlannerConfirmedMasterIds).First();
                            //}
                        }
                        if (!IsAlreadyOrder)
                        {
                            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                            bool isNewPickerChangesAllowed = tripPlannerHelper.CanAddOrderInNewPicker(tripPlannerConfirmedMasterId, authContext);
                            var master = authContext.TripPlannerConfirmedMasters.FirstOrDefault(x => x.Id == tripPlannerConfirmedMasterId);
                            if (master.IsFreezed || (master.IsPickerGenerated && !IsNewPickerOrder) || (IsNewPickerOrder && !isNewPickerChangesAllowed))
                            {
                                scope.Dispose();
                                return new APIResponse { Status = false, Message = "Data Not Saved" };

                                //return false;
                            }

                            TripPlannerConfirmedMaster tripPlannerConfirmedMaster = authContext.TripPlannerConfirmedMasters
                                .Include("TripPlannerConfirmedDetailList.TripPlannerConfirmedOrderList")
                                .Where(x => x.Id == tripPlannerConfirmedMasterId)
                                .FirstOrDefault();

                            foreach (var orderId in orderIdList)
                            {
                                var order = orderList.FirstOrDefault(x => x.OrderId == orderId);
                                var selectedDetail = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList
                                    .Where(x => x.CustomerId == order.CustomerId
                                    && x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId)//&& x.IsActive == true && x.IsDeleted == false
                                    .FirstOrDefault();


                                if (order.TripPlannerConfirmedDetailId == 0 && order.IsActive)
                                {
                                    if (selectedDetail == null)
                                    {
                                        if (!order.IsAddableDueToCustomerLocation)
                                        {
                                            var customer = authContext.Customers.FirstOrDefault(x => x.CustomerId == order.CustomerId);
                                            GeoHelper geoHelper = new GeoHelper();
                                            decimal? lat, longitude;
                                            geoHelper.GetLatLongWithZipCode(customer.ShippingAddress, customer.City, customer.ZipCode, out lat, out longitude);
                                            if (lat != null && longitude != null && lat > -90 && lat < 90 && longitude > -180 && longitude < 180)
                                            {
                                                customer.lat = (double)lat.Value;
                                                customer.lg = (double)longitude.Value;
                                                customer.UpdatedDate = DateTime.Now;
                                                customer.LastModifiedBy = authContext.Peoples.Where(x => x.PeopleID == userId).Select(y => y.DisplayName).FirstOrDefault();
                                                authContext.Commit();
                                                order.Lat = (double)lat.Value;
                                                order.Lng = (double)longitude.Value;
                                            }
                                            else
                                            {
                                                scope.Dispose();
                                                //return false;
                                                return new APIResponse { Status = false, Message = "Data Not Saved" };
                                            }
                                        }
                                        selectedDetail = new TripPlannerConfirmedDetail
                                        {
                                            CreatedBy = userId,
                                            CommaSeparatedOrderList = "",
                                            CreatedDate = DateTime.Now,
                                            Id = 0,
                                            CustomerId = order.CustomerId,
                                            IsActive = true,
                                            IsDeleted = false,
                                            Lat = order.Lat,
                                            Lng = order.Lng,
                                            ModifiedBy = null,
                                            ModifiedDate = null,
                                            OrderCount = 0,
                                            SequenceNo = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.IsActive == true && x.IsDeleted == false).Count() + 1,
                                            TotalAmount = 0,
                                            TotalDistanceInMeter = 0,
                                            TotalTimeInMins = 0,
                                            TotalWeight = 0,
                                            TripPlannerConfirmedMasterId = tripPlannerConfirmedMaster.Id,
                                            TripPlannerConfirmedOrderList = new List<TripPlannerConfirmedOrder>()
                                        };
                                        tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Add(selectedDetail);

                                    }

                                    if (selectedDetail.TripPlannerConfirmedOrderList != null
                                        && !selectedDetail.TripPlannerConfirmedOrderList.Any(x => x.IsActive == true && x.OrderId == order.OrderId))
                                    {
                                        selectedDetail.TripPlannerConfirmedOrderList.Add(new TripPlannerConfirmedOrder
                                        {
                                            Amount = order.Amount,
                                            CreatedBy = userId,
                                            CreatedDate = DateTime.Now,
                                            DistanceInMeter = order.DistanceInMeter,
                                            Id = 0,
                                            IsActive = true,
                                            IsDeleted = false,
                                            IsManuallyAdded = true,
                                            ModifiedBy = null,
                                            ModifiedDate = null,
                                            OrderId = order.OrderId,
                                            TimeInMins = order.TimeInMins,
                                            TripPlannerConfirmedDetailId = selectedDetail.Id,
                                            WeightInKg = order.WeightInKg,
                                            IsNewPickerOrder = order.IsNewPickerOrder
                                        });
                                    }
                                }
                                var selectedOrder = selectedDetail.TripPlannerConfirmedOrderList
                                                       .Where(y => y.OrderId == order.OrderId && y.IsDeleted == false)
                                                       .FirstOrDefault();
                                if (selectedOrder != null)
                                {
                                    selectedOrder.IsActive = order.IsActive;
                                    selectedOrder.IsDeleted = !selectedOrder.IsActive;

                                    if (order.IsActive != order.IsActiveOld && order.IsActive)
                                    {
                                        selectedDetail.IsActive = true;
                                        selectedDetail.IsDeleted = false;
                                        selectedDetail.TotalAmount += selectedOrder.Amount;
                                        selectedDetail.TotalTimeInMins += selectedOrder.TimeInMins;
                                        selectedDetail.TotalWeight += selectedOrder.WeightInKg;
                                        selectedDetail.OrderCount += 1;

                                    }
                                    else if (order.IsActive != order.IsActiveOld && !order.IsActive)
                                    {
                                        selectedDetail.TotalAmount -= selectedOrder.Amount;
                                        selectedDetail.TotalTimeInMins -= selectedOrder.TimeInMins;
                                        selectedDetail.TotalWeight -= selectedOrder.WeightInKg;
                                        selectedDetail.OrderCount -= 1;
                                        if (selectedDetail.OrderCount == 0)
                                        {
                                            selectedDetail.IsActive = false;
                                            selectedDetail.IsDeleted = true;
                                        }
                                    }
                                }
                            }

                            if (tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList != null && tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Any())
                            {
                                var detail = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.FirstOrDefault(x => x.CustomerId == 0);
                                detail.SequenceNo = 1000;
                            }
                            int seq = 1;
                            foreach (var detail in tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.OrderBy(x => x.SequenceNo).ToList())
                            {
                                detail.SequenceNo = seq++;
                                if (detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true).Count() > 0)
                                {
                                    detail.CommaSeparatedOrderList
                                        = String.Join(", ", detail.TripPlannerConfirmedOrderList.Where(x => x.IsActive == true).Select(x => x.OrderId).Distinct());
                                }
                                else
                                {
                                    detail.CommaSeparatedOrderList = "";
                                }
                            }

                            tripPlannerConfirmedMaster.CustomerCount
                                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.OrderCount > 0).Count();
                            tripPlannerConfirmedMaster.OrderCount
                                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.OrderCount > 0).Sum(x => x.OrderCount);
                            tripPlannerConfirmedMaster.TotalAmount
                                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.OrderCount > 0).Sum(x => x.TotalAmount);
                            tripPlannerConfirmedMaster.TotalDistanceInMeter
                                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalDistanceInMeter);
                            tripPlannerConfirmedMaster.TotalTimeInMins
                                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalTimeInMins);
                            tripPlannerConfirmedMaster.TotalWeight
                                = tripPlannerConfirmedMaster.TripPlannerConfirmedDetailList.Where(x => x.OrderCount > 0).Sum(x => x.TotalWeight);
                            tripPlannerConfirmedMaster.ModifiedBy = userId;
                            tripPlannerConfirmedMaster.ModifiedDate = DateTime.Now;

                            authContext.Commit();
                            scope.Complete();
                            return new APIResponse { Status = true, Data = PendingSKcodess };
                        }
                        else
                        {
                            scope.Dispose();
                            return new APIResponse { Status = false, Message = "Data Not Saved", Data = new List<string>() };
                        }

                    }

                }
            }
            return new APIResponse { Status = false, Message = "Data Not Saved", Data = new List<string>() };
        }

        [HttpPost]
        [Route("UpdateTrip/{IsNewPickerOrder}")]
        public bool UpdateTrip([FromBody] TripPlannerConfirmedMasterVM master, [FromUri] bool IsNewPickerOrder = false)
        {
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            var userId = GetLoginUserId();
            bool resultRes = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            TripPlannerConfirmedMaster tripPlannerConfirmedMaster = null;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var authContext = new AuthContext())
                {
                    tripPlannerConfirmedMaster = authContext.TripPlannerConfirmedMasters
                        .Where(x => x.Id == master.Id)
                        .FirstOrDefault();
                    if (tripPlannerConfirmedMaster.IsFreezed)
                    {
                        scope.Dispose();
                        return false;
                    }
                    bool isAlreadyPickerGenerated = tripPlannerConfirmedMaster.IsPickerGenerated;
                    bool isNewPickerChangesAllowed = tripPlannerHelper.CanAddOrderInNewPicker(tripPlannerConfirmedMaster.Id, authContext);

                    if (isAlreadyPickerGenerated && (!IsNewPickerOrder || (IsNewPickerOrder && !isNewPickerChangesAllowed)))
                    {
                        scope.Dispose();
                        return false;
                    }
                    tripPlannerConfirmedMaster.DriverId = master.DriverId;
                    tripPlannerConfirmedMaster.DboyId = master.DboyId;
                    tripPlannerConfirmedMaster.AgentId = master.AgentId;
                    tripPlannerConfirmedMaster.VehicleMasterId = master.VehicleId.HasValue ? master.VehicleId.Value : 0;
                    tripPlannerConfirmedMaster.VehicleNumber = master.VehicleNumber;
                    tripPlannerConfirmedMaster.ModifiedBy = userId;
                    tripPlannerConfirmedMaster.ModifiedDate = DateTime.Now;
                    // tripPlannerConfirmedMaster.TripDate = master.TripDate;
                    authContext.Commit();

                    if (tripPlannerConfirmedMaster != null)
                    {
                        var result = tripPlannerHelper.AssignmentCreateUpdate(tripPlannerConfirmedMaster.TripPlannerMasterId.Value, userId, authContext, scope);
                        if (result.Status)
                        {
                            scope.Complete();
                            resultRes = true;
                        }
                        else
                        {
                            scope.Dispose();
                            resultRes = false;
                        }
                    }
                }
            }
            return resultRes;
        }

        [HttpGet]
        [Route("GetTripInfo/{warehouseId}")]
        public List<CheckPlanningDc> GetTripInfo(int warehouseId)
        {
            using (var authContext = new AuthContext())
            {
                string spNameWithParam = "EXEC Operation.TripPlanner_GetCheckPlanningList @WarehouseId";
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@WarehouseId", warehouseId));
                List<CheckPlanningDc> list = authContext.Database.SqlQuery<CheckPlanningDc>(spNameWithParam, paramList.ToArray()).ToList();
                return list;
            }

        }

        [HttpGet]
        [Route("GetTripTouchPoints/{tripPlannerMasterId}")]
        public List<MapCheckPoint> GetTripTouchPoints(long tripPlannerMasterId)
        {
            using (var authContext = new AuthContext())
            {
                string spNameWithParam = "EXEC Operation.TripPlanner_GetTouchPoints @TripPlannerMasterId";
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@TripPlannerMasterId", tripPlannerMasterId));
                List<MapCheckPoint> list = authContext.Database.SqlQuery<MapCheckPoint>(spNameWithParam, paramList.ToArray()).ToList();
                return list;
            }

        }

        [Route("GetAgentList/{clusterId}")]
        public List<PrimeNgDropDown<int?>> GetAgentList(int clusterId)
        {
            using (var authContext = new AuthContext())
            {
                var query = from c in authContext.ClusterAgent
                            join p in authContext.Peoples on c.AgentId equals p.PeopleID
                            where p.Active == true && p.Deleted != true && c.Deleted != true && c.ClusterId == clusterId
                            orderby p.PeopleFirstName
                            select new PrimeNgDropDown<int?>
                            {
                                value = p.PeopleID,
                                label = p.DisplayName != null ? p.DisplayName : (p.PeopleFirstName.ToString() + " " + p.PeopleLastName.ToString()),
                            };

                var result = query.ToList();
                result.Insert(0, new PrimeNgDropDown<int?>
                {
                    label = "Select Agent",
                    value = null
                });
                return result;
            }
        }
        [Route("GetAgentMappingList/{wareHouseId}")]
        [AllowAnonymous]
        public VehicleDboyDriverDDs GetAgentMappingList(int wareHouseId)
        {
            VehicleDboyDriverDDs ddList = new VehicleDboyDriverDDs();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "Operation.TripPlanner_VehicleDboyDriverGet";


                cmd.Parameters.Add(new SqlParameter("@warehouseid", wareHouseId));
                cmd.Parameters.Add(new SqlParameter("@TripDate", DateTime.Today));

                //cmd.Parameters.Add(new SqlParameter("@tripMasterId", tripMasterId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                ddList.Vehicles = ((IObjectContextAdapter)context)
                               .ObjectContext
                               .Translate<VehicleDropDownList>(reader).ToList();
                reader.NextResult();

                if (reader.HasRows)
                {
                    ddList.Dboys = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<DboyDropDownList>(reader).ToList();
                }
                reader.NextResult();
                if (reader.HasRows)
                {
                    ddList.Drivers = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<DriverDropDownList>(reader).ToList();
                }
            }
            return ddList;
        }

        [Route("GetVehicleMappingList")]
        [AllowAnonymous]
        public VehicleDboyDriverDDs GetVehicleMappingList(int wareHouseId, DateTime TripDate)
        {
            VehicleDboyDriverDDs ddList = new VehicleDboyDriverDDs();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "Operation.TripPlanner_VehicleDboyDriverGet";


                cmd.Parameters.Add(new SqlParameter("@warehouseid", wareHouseId));
                cmd.Parameters.Add(new SqlParameter("@TripDate", TripDate));

                //cmd.Parameters.Add(new SqlParameter("@tripMasterId", tripMasterId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                ddList.Vehicles = ((IObjectContextAdapter)context)
                               .ObjectContext
                               .Translate<VehicleDropDownList>(reader).ToList();
                reader.NextResult();
            }
            return ddList;
        }

        [HttpGet]
        [Route("GetAgentMappingListV2/{wareHouseId}/{tripMasterId}")]
        [AllowAnonymous]
        public VehicleDboyDriverDDs GetAgentMappingList(int wareHouseId, long tripMasterId)
        {
            VehicleDboyDriverDDs ddList = new VehicleDboyDriverDDs();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "Operation.TripPlanner_VehicleDboyDriverGetV2";


                cmd.Parameters.Add(new SqlParameter("@warehouseid", wareHouseId));
                cmd.Parameters.Add(new SqlParameter("@tripMasterId", tripMasterId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                ddList.Vehicles = ((IObjectContextAdapter)context)
                               .ObjectContext
                               .Translate<VehicleDropDownList>(reader).ToList();
                reader.NextResult();

                if (reader.HasRows)
                {
                    ddList.Dboys = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<DboyDropDownList>(reader).ToList();
                }
                reader.NextResult();
                if (reader.HasRows)
                {
                    ddList.Drivers = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<DriverDropDownList>(reader).ToList();
                }
            }
            return ddList;
        }

        [HttpGet]
        [Route("GetAgentMappingListV3/{wareHouseId}/{tripMasterId}/{FleetType}")]
        [AllowAnonymous]
        public VehicleDboyDriverDDs GetAgentMappingListV3(int wareHouseId, long tripMasterId, string FleetType)
        {
            VehicleDboyDriverDDs ddList = new VehicleDboyDriverDDs();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "Operation.TripPlanner_VehicleDboyDriverGetV2";


                cmd.Parameters.Add(new SqlParameter("@warehouseid", wareHouseId));
                cmd.Parameters.Add(new SqlParameter("@tripMasterId", tripMasterId));
                cmd.Parameters.Add(new SqlParameter("@FleetType", FleetType));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                ddList.Vehicles = ((IObjectContextAdapter)context)
                               .ObjectContext
                               .Translate<VehicleDropDownList>(reader).ToList();
                reader.NextResult();

                if (reader.HasRows)
                {
                    ddList.Dboys = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<DboyDropDownList>(reader).ToList();
                }
                reader.NextResult();
                if (reader.HasRows)
                {
                    ddList.Drivers = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<DriverDropDownList>(reader).ToList();
                }
            }
            return ddList;
        }
        [HttpPost]
        [Route("CreatePicker")]
        [AllowAnonymous]
        public async Task<ResultViewModel<long>> CreatePicker(long tripPlannerConfirmedMasterId, long dboyId, bool IsNewPicker = false)
        {
            long userId = GetLoginUserId();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var authContext = new AuthContext())
                {
                    #region Sales Return Checking Exist Return Order
                    var tripConfirmMasterData = authContext.TripPlannerConfirmedMasters.Where(x => x.Id == tripPlannerConfirmedMasterId).FirstOrDefault();
                    List<long> returnOrderIds = new List<long>();

                    returnOrderIds = (from td in authContext.TripPlannerConfirmedDetails
                                      join to in authContext.TripPlannerConfirmedOrders
                                      on td.Id equals to.TripPlannerConfirmedDetailId
                                      where td.IsActive == true && td.IsDeleted == false
                                      && to.IsActive == true && to.IsDeleted == false && td.CustomerId > 0
                                      && td.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId
                                      select to.OrderId).ToList();

                    if (returnOrderIds != null && returnOrderIds.Count>0)
                    {
                        var orderMaster= authContext.DbOrderMaster.Where(x => returnOrderIds.Contains(x.OrderId)).ToList();
                        var existReturnOrder = orderMaster.Where(x => returnOrderIds.Contains(x.OrderId) && x.OrderType == 3).ToList();
                        var existDispatchOrder = orderMaster.Where(x => returnOrderIds.Contains(x.OrderId) && (x.Status == "Delivery Redispatch") && x.OrderType != 3).ToList();
                        if (existReturnOrder.Count > 0 && existReturnOrder.Count == returnOrderIds.Count)
                        {
                            tripConfirmMasterData.IsPickerGenerated = true;
                            tripConfirmMasterData.ModifiedBy = (int)userId;
                            tripConfirmMasterData.ModifiedDate = DateTime.Today;
                            authContext.Entry(tripConfirmMasterData).State = EntityState.Modified;
                            authContext.Commit();
                            scope.Complete();
                            return new ResultViewModel<long>
                            {
                                ErrorMessage = null,
                                IsSuceess = true,
                                ResultItem = 0,
                                ResultList = null,
                                SuccessMessage = "Return Order Generated Successfully!"
                            };
                        }
                        else if ((existDispatchOrder.Count > 0 && existReturnOrder.Count > 0) && (existDispatchOrder.Count + existReturnOrder.Count) == returnOrderIds.Count)
                        {
                            tripConfirmMasterData.IsPickerGenerated = true;
                            tripConfirmMasterData.ModifiedBy = (int)userId;
                            tripConfirmMasterData.ModifiedDate = DateTime.Today;
                            authContext.Entry(tripConfirmMasterData).State = EntityState.Modified;
                            authContext.Commit();
                            scope.Complete();
                            return new ResultViewModel<long>
                            {
                                ErrorMessage = null,
                                IsSuceess = true,
                                ResultItem = 0,
                                ResultList = null,
                                SuccessMessage = "Order Generated Successfully!"
                            };
                        }
                    }
                    #endregion


                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    bool isAlreadyPickerGenerated = tripPlannerHelper.GetIsPickerGenerated(tripPlannerConfirmedMasterId, authContext);
                    bool isAddNewPickerNotGenerated = tripPlannerHelper.IsAddNewPickerNotGenerated(tripPlannerConfirmedMasterId, authContext);

                    if ((isAlreadyPickerGenerated && !IsNewPicker) || (IsNewPicker && !isAddNewPickerNotGenerated))
                    {
                        return new ResultViewModel<long>

                        {
                            ErrorMessage = "No need to generate picker either its already generated or trip not contains any order for picker",
                            IsSuceess = false,
                            ResultItem = 0,
                            ResultList = null,
                            SuccessMessage = null
                        };
                    }

                    var query = from tcm in authContext.TripPlannerConfirmedMasters
                                join td in authContext.TripPlannerConfirmedDetails on tcm.Id equals td.TripPlannerConfirmedMasterId
                                join to in authContext.TripPlannerConfirmedOrders on td.Id equals to.TripPlannerConfirmedDetailId
                                where tcm.Id == tripPlannerConfirmedMasterId && to.IsActive == true && to.IsDeleted == false
                                //&& tcm.TripDate == DateTime.Today
                                select new
                                {
                                    orderId = to.OrderId,
                                    IsManuallyAdded = to.IsManuallyAdded,
                                    TripPlannerConfirmedMasterId = tcm.Id,
                                    IsNewPickerOrder = to.IsNewPickerOrder
                                };
                    var results = query.ToList();
                    List<int> pickerOrderIds = null;
                    var tripPlannerConfirmedMaster = authContext.TripPlannerConfirmedMasters.FirstOrDefault(x => x.Id == tripPlannerConfirmedMasterId);
                    if (results != null && results.Any())
                    {
                        List<long> OrderIds = null;
                        if (IsNewPicker == true)
                        {
                            OrderIds = results.Where(x => x.IsNewPickerOrder == true).Select(x => x.orderId).ToList();
                        }
                        else
                        {
                            OrderIds = results.Select(x => x.orderId).ToList();
                        }
                        var orderDMLists = authContext.OrderDispatchedMasters.Where(c => OrderIds.Contains(c.OrderId) && (c.Status == "Delivery Redispatch" || c.Status == "Issued")).ToList();
                        //harry sir code
                        pickerOrderIds = OrderIds.Where(x => !orderDMLists.Where(z => (z.Status == "Delivery Redispatch" || z.Status == "Issued")).Select(z => z.OrderId).ToList().Contains((int)x))
                        .Select(x => (int)x).ToList();
                    }

                    if (pickerOrderIds != null && pickerOrderIds.Any())
                    {
                        //var cluster = authContext.Clusters.FirstOrDefault(x => x.ClusterId == (int)tripPlannerConfirmedMaster.ClusterId);
                        var dboyPeopleId = authContext.DboyMasters.FirstOrDefault(x => x.Id == dboyId).PeopleId;
                        GeneratePickerAutoPickDc generatePickerAutoPickDc = new GeneratePickerAutoPickDc
                        {
                            OrderIds = pickerOrderIds,
                            //ClusterId = (int)tripPlannerConfirmedMaster.ClusterId,
                            CreatedBy = (int)userId,
                            DboyId = dboyPeopleId,
                            WarehouseId = tripPlannerConfirmedMaster.WarehouseId
                        };
                        ReadyToPickDispatchedHelper readyToPickDispatchedHelper = new ReadyToPickDispatchedHelper();
                        long pickerid = readyToPickDispatchedHelper.GeneratePickerForAutoPickOrder(generatePickerAutoPickDc, authContext, scope);
                        if (pickerid > 0)
                        {
                            TripPickerAssignmentMapping tripPickerAssignmentMappings = new TripPickerAssignmentMapping
                            {
                                TripPlannerConfirmedMasterId = tripPlannerConfirmedMasterId,
                                OrderPickerMasterId = pickerid,
                                AssignmentId = null,
                                CreatedBy = (int)userId,
                                ModifiedBy = null,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = null,
                                IsActive = true,
                                IsDeleted = false
                            };
                            authContext.TripPickerAssignmentMapping.Add(tripPickerAssignmentMappings);
                            tripPlannerConfirmedMaster.IsPickerGenerated = true;
                            authContext.Commit();
                            scope.Complete();
                            return new ResultViewModel<long>
                            {
                                ErrorMessage = "",
                                IsSuceess = true,
                                ResultItem = pickerid,
                                ResultList = null,
                                SuccessMessage = "Picker generated successfully"
                            };
                        }
                        else
                        {
                            scope.Dispose();
                            return new ResultViewModel<long>
                            {
                                ErrorMessage = "Issue in generate picker",
                                IsSuceess = false,
                                ResultItem = 0,
                                ResultList = null,
                                SuccessMessage = null
                            };
                        }
                    }
                    else
                    {
                        scope.Dispose();
                        return new ResultViewModel<long>
                        {
                            ErrorMessage = "No order to generate picker",
                            IsSuceess = false,
                            ResultItem = 0,
                            ResultList = null,
                            SuccessMessage = null
                        };
                    }
                }
            }

        }

        /// <summary>
        /// Freeze the trip
        /// Freeze the assignment
        /// </summary>
        /// <param name="tripPlannerConfirmedMasterId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateTrip")]
        public async Task<ResponceMsg> CreateTrip(FinalizeTripParam param)
        {
            SalesReturnOrderHelper salesReturnOrderHelper = new SalesReturnOrderHelper();
            string userName = "";
            var identity = User.Identity as ClaimsIdentity;
            ResponceMsg res = new ResponceMsg();
            TripPlannerVehicleManager tripPlannerVehicleManager = new TripPlannerVehicleManager();
            int userId = GetLoginUserId();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                userName = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            TripPlannerConfirmedMaster master = null;
            bool IsDboyUpdated = false;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var authContext = new AuthContext())
                {
                    master = authContext.TripPlannerConfirmedMasters.FirstOrDefault(x => x.Id == param.tripPlannerConfirmedMasterId);

                    #region Sales Return Checking Exist Return Order

                    List<long> returnOrderIds = new List<long>();

                    returnOrderIds = (from td in authContext.TripPlannerConfirmedDetails
                                        join to in authContext.TripPlannerConfirmedOrders
                                        on td.Id equals to.TripPlannerConfirmedDetailId
                                        where td.IsActive == true && td.IsDeleted == false
                                        && to.IsActive == true && to.IsDeleted == false && td.CustomerId > 0
                                        && td.TripPlannerConfirmedMasterId == master.Id
                                        select to.OrderId).ToList();

                    if (returnOrderIds != null && returnOrderIds.Count>0)
                    {
                        var existReturnOrder = authContext.DbOrderMaster.Where(x => returnOrderIds.Contains(x.OrderId) && x.OrderType == 3).Include("OrderDetails").ToList();
                        var orderDispatchedMaster = authContext.OrderDispatchedMasters.Where(x => returnOrderIds.Contains(x.OrderId)).ToList();
                        if (existReturnOrder.Count > 0 && existReturnOrder.Count == returnOrderIds.Count)
                        {
                            var changeDboyId = new SqlParameter("@ChangeDboyId", param.DboyId.Value);
                            var data = authContext.Database.SqlQuery<TripChangeBoyDC>("exec Operation.TripPlanner_TripChangeDboy @ChangeDboyId", changeDboyId).FirstOrDefault();

                            foreach (var x in existReturnOrder)
                            {
                                x.Status = "Shipped";
                                foreach (var od in x.orderDetails)
                                {
                                    od.Status = "Shipped";
                                }
                                authContext.Entry(x).State = EntityState.Modified;
                            }


                            if (master != null)
                            {
                                master.DriverId = param.DriverId;
                                master.VehicleMasterId = param.VehicleId;
                                master.AgentId = param.AgentId;
                                master.DboyId = data.DboyMasterId;
                                master.IsVisibleToDboy = true;
                                master.IsFreezed = true;
                                master.ModifiedBy = userId;
                                master.ModifiedDate = DateTime.Today;
                                master.VehicleFare = param.VehicleFare.HasValue ? param.VehicleFare.Value : 0 ;
                                authContext.Entry(master).State = EntityState.Modified;

                                foreach (var order in orderDispatchedMaster)
                                {
                                    OrderMasterHistories h1 = new OrderMasterHistories();
                                    h1.orderid = order.OrderId;
                                    h1.Status = order.Status;
                                    h1.Reasoncancel = "Due to Change in delivery Boy from " + order.DboyName + " " + order.DboyMobileNo + " To " + data.Dboyname + " " + data.DboyMobile + " " + "by Trip";
                                    h1.Warehousename = order.WarehouseName;
                                    h1.username = userName;
                                    h1.userid = userId;
                                    h1.CreatedDate = DateTime.Now;
                                    authContext.OrderMasterHistoriesDB.Add(h1);
                                    order.DBoyId = data.PeopleID;
                                    order.DboyName = data.Dboyname;
                                    order.DboyMobileNo = data.DboyMobile;
                                    order.Status = "Shipped";

                                    authContext.Entry(order).State = EntityState.Modified;

                                    bool resStatus = salesReturnOrderHelper.PostOrderStatus(order.OrderId, "Shipped", userId, authContext);
                                }
                                TripPlannerHelper tripPlanner = new TripPlannerHelper();
                                res = tripPlanner.AssignmentFinalize(param.tripPlannerConfirmedMasterId, userId, authContext, scope, param.startingKm, param.reportingTime, param.lateReportingTimeInMins, param.penaltyCharge,true);
                                if (res.Status)
                                {
                                    scope.Complete();
                                    return res;
                                }
                                else
                                {
                                    scope.Dispose();
                                    return res;
                                }

                            }
                        }
                        else
                        {
                            foreach (var x in existReturnOrder)
                            {
                                x.Status = "Shipped";
                                foreach (var od in x.orderDetails)
                                {
                                    od.Status = "Shipped";
                                }
                                authContext.Entry(x).State = EntityState.Modified;
                            }
                            var ReturnorderDispatchedMaster = orderDispatchedMaster.Where(x => existReturnOrder.Select(y => y.OrderId).ToList().Contains(x.OrderId)).Distinct().ToList();
                            foreach (var x in ReturnorderDispatchedMaster)
                            {
                                x.Status = "Shipped";
                                authContext.Entry(x).State = EntityState.Modified;
                                bool resStatus = salesReturnOrderHelper.PostOrderStatus(x.OrderId, "ReturnShipped", userId, authContext);
                            }
                        }
                    }
                    #endregion

                    master.TripDate = param.TripDate;
                    IsDboyUpdated = true;
                    //if (master.DboyId != param.DboyId)
                    //{
                    //    IsDboyUpdated = true;
                    //}
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    bool isNotADamageTrip = true;
                    if ((master.TripTypeEnum == (int)TripTypeEnum.Damage_Expiry || master.TripTypeEnum == (int)TripTypeEnum.NonSellable))
                    {
                        isNotADamageTrip = false;
                    }
                    bool isPickerAlreadyGenerated = tripPlannerHelper.GetIsPickerGenerated(param.tripPlannerConfirmedMasterId, authContext);
                    bool isAddNewPickerFinalized = tripPlannerHelper.GetIsAddNewPickerFinalized(param.tripPlannerConfirmedMasterId, authContext);

                    if (!isPickerAlreadyGenerated && isNotADamageTrip && isAddNewPickerFinalized)
                    {
                        res = new ResponceMsg()
                        {
                            Message = "Trip not finalized because picker not generated yet",
                            Status = false
                        };
                        scope.Dispose();
                        return res;
                    }

                    bool isPickerFinalzied = tripPlannerHelper.IsPickerFinalzied(param.tripPlannerConfirmedMasterId, authContext);
                    if (!isPickerFinalzied && isNotADamageTrip)
                    {
                        res = new ResponceMsg()
                        {
                            Message = "Trip not finalized because some assignment not finalized",
                            Status = false
                        };
                        scope.Dispose();
                        return res;
                    }
                    bool IsRunningTrip = tripPlannerHelper.GetIsTripRunning(param.VehicleId, param.tripPlannerConfirmedMasterId, authContext);
                    if (IsRunningTrip == true)
                    {
                        res = new ResponceMsg()
                        {
                            Message = "Trip alredy Running for this vehicle",
                            Status = false
                        };
                        scope.Dispose();
                        return res;
                    }

                    if (master.IsFreezed)
                    {
                        res = new ResponceMsg()
                        {
                            Message = "Trip alredy Freezed",
                            Status = false
                        };
                        scope.Dispose();
                        return res;
                    }
                    List<TripBlockedOrderVM> blockedOrderList = null;
                    GetReplaceVehicleListDc ReplaceVehicleList = null;
                    if(!string.IsNullOrEmpty(param.ReplacementVehicleNo))
                        ReplaceVehicleList = tripPlannerHelper.GetReplaceVehicle(master.Id, authContext, param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString());
                    if (ReplaceVehicleList != null && !ReplaceVehicleList.IsAlreadyEwaybillGenerate)
                    {
                        if (!ReplaceVehicleList.IsExistsVehicle)
                        {
                            var vehicleAttandance = authContext.TripPlannerVechicleAttandanceDb.FirstOrDefault(x => x.VehicleMasterId == param.VehicleId && x.AttendanceDate == master.TripDate && x.IsActive && x.IsDeleted == false);
                            if (vehicleAttandance != null)
                            {
                                if (param.IsReplacementVehicleNo.HasValue && string.IsNullOrEmpty(vehicleAttandance.ReplacementVehicleNo))
                                {
                                    vehicleAttandance.IsReplacementVehicleNo = param.IsReplacementVehicleNo;
                                    vehicleAttandance.ReplacementVehicleNo = param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString();
                                    vehicleAttandance.ModifiedDate = DateTime.Now;
                                    vehicleAttandance.ModifiedBy = userId;
                                    authContext.Entry(vehicleAttandance).State = EntityState.Modified;
                                }
                                else if (!string.IsNullOrEmpty(vehicleAttandance.ReplacementVehicleNo) && !string.IsNullOrEmpty(param.ReplacementVehicleNo)
                                    && vehicleAttandance.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString()
                                    == param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString() && param.IsReplacementVehicleNo.HasValue)
                                {
                                    vehicleAttandance.IsReplacementVehicleNo = param.IsReplacementVehicleNo;
                                    vehicleAttandance.ReplacementVehicleNo = param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString();
                                    vehicleAttandance.ModifiedDate = DateTime.Now;
                                    vehicleAttandance.ModifiedBy = userId;
                                    authContext.Entry(vehicleAttandance).State = EntityState.Modified;
                                }
                                else if (!string.IsNullOrEmpty(vehicleAttandance.ReplacementVehicleNo) && !string.IsNullOrEmpty(param.ReplacementVehicleNo)
                                    && vehicleAttandance.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString() != param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString())
                                {
                                    res = new ResponceMsg()
                                    {
                                        Message = "Trip alredy Running for this Replacement vehicle",
                                        Status = false
                                    };
                                    scope.Dispose();
                                    return res;
                                }
                            }
                            else
                            {
                                res = new ResponceMsg()
                                {
                                    Message = "Today this vehicle Absent!!",
                                    Status = false
                                };
                                scope.Dispose();
                                return res;
                            }
                        }
                        else
                        {
                            res = new ResponceMsg()
                            {
                                Message = "Replacement vehicle and Actual vehicle is same!!",
                                Status = false
                            };
                            scope.Dispose();
                            return res;
                        }
                    }
                    blockedOrderList = tripPlannerHelper.GetBlockedOrderList(master.Id, authContext);
                    if (blockedOrderList != null && blockedOrderList.Any())
                    {
                        res = new ResponceMsg()
                        {
                            Message = "Eway bill or irn issue",
                            Status = false,
                            blockedOrderList = blockedOrderList
                        };
                        scope.Dispose();
                        return res;
                    }

                    //tripPlannerHelper.UpdateTripSequence(param.tripPlannerConfirmedMasterId, authContext);
                    if (!master.IsNotLastMileTrip)
                    {
                        tripPlannerHelper.UpdateTripSequenceNew(param.tripPlannerConfirmedMasterId, authContext);
                    }
                    tripPlannerHelper.UpdateAmount(param.tripPlannerConfirmedMasterId, authContext);
                    if (IsDboyUpdated)
                    {
                        bool result = tripPlannerHelper.TripChangeDBoy(master.Id, param.DboyId.Value, authContext, userId, userName);
                        if (!result)
                        {
                            res = new ResponceMsg()
                            {
                                Message = "Trip Change DBoy not Updated",
                                Status = false
                            };
                            scope.Dispose();
                            return res;
                        }
                    }
                    scope.Complete();
                }
            }
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var authContext = new AuthContext())
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    master.DriverId = param.DriverId;
                    master.VehicleMasterId = param.VehicleId;
                    master.AgentId = param.AgentId;
                    master.DboyId = param.DboyId;
                    master.VehicleFare = param.VehicleFare.HasValue? param.VehicleFare.Value: 0;
                    authContext.Entry(master).State = EntityState.Modified;
                    res = tripPlannerHelper.AssignmentFinalize(param.tripPlannerConfirmedMasterId, userId, authContext, scope, param.startingKm, param.reportingTime, param.lateReportingTimeInMins, param.penaltyCharge);
                    bool result = tripPlannerHelper.SetTripDboyVisibiltyStatus(param.tripPlannerConfirmedMasterId, authContext);
                    TripPlannerVechicleAttandanceManager tripPlannerVechicleAttandanceManager = new TripPlannerVechicleAttandanceManager();
                    InsertVechicleAttandanceDc insertVechicleAttandanceDc = new InsertVechicleAttandanceDc
                    {
                        AttandanceDate = master.TripDate,
                        IsDateSendbyUser = true,
                        VehicleMasterId = master.VehicleMasterId,
                        IsTodayAttendance = true,
                        WarehouseId = master.WarehouseId
                    };
                    var updateAdhocAttandance = tripPlannerHelper.InsertVechicleAttandance(insertVechicleAttandanceDc, authContext);
                    if (res.Status && result)
                    {
                        authContext.Commit();
                        scope.Complete();
                    }
                    else
                    {
                        scope.Dispose();
                    }
                }
            }
            return res;
        }

        [HttpGet]
        [Route("GetClusterWarehouseWise")]
        public dynamic GetClusterWarehouseWise(int WarehouseId)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Cluster> c = db.Clusters.Include("LtLng").Where(a => a.WarehouseId == WarehouseId).ToList();
                Warehouse warehouse = db.Warehouses.Where(a => a.WarehouseId == WarehouseId).SingleOrDefault();

                return new
                {
                    warehouse = warehouse,
                    clusters = c.Select(x =>
                  new
                  {

                      x.ClusterId,
                      x.ClusterName,
                      clusterlatlng = x.LtLng.Select(y => new { lat = y.latitude, lng = y.longitude }).ToList()
                  }).ToList()
                };
            }
        }

        [HttpGet]
        [Route("GetClusterTripInfo/{warehouseId}")]
        public List<TripDetailsDC> GetClusterTripInfo(int warehouseId)
        {
            List<TripDetailsDC> list = new List<TripDetailsDC>();
            using (var authContext = new AuthContext())
            {
                var wareid = new SqlParameter("@WarehouseId", warehouseId);
                list = authContext.Database.SqlQuery<TripDetailsDC>("GetClusterTripInfo @WarehouseId", wareid).ToList();
                return list;
            }
        }

        [HttpGet]
        [Route("GetWarehouseVehicleDetail/{warehouseId}")]
        public VehicleSummaryDC GetWarehouseVehicleDetail(int warehouseId)
        {
            using (var authContext = new AuthContext())
            {
                var wareid = new SqlParameter("@WarehouseId", warehouseId);
                VehicleSummaryDC vehicleSummaryDC = authContext.Database.SqlQuery<VehicleSummaryDC>("Operation.TripPlanner_VehicleUtilizationSummary @WarehouseId", wareid).FirstOrDefault();
                return vehicleSummaryDC;
            }
        }


        [HttpGet]
        [Route("ExtraVehicleRequired/{warehouseId}")]
        public List<ExtraVehicleRequiredDC> GetExtraVehicleRequired(int warehouseId)
        {
            List<ExtraVehicleRequiredDC> list = new List<ExtraVehicleRequiredDC>();
            using (var authContext = new AuthContext())
            {
                var wareid = new SqlParameter("@WarehouseId", warehouseId);
                list = authContext.Database.SqlQuery<ExtraVehicleRequiredDC>("Operation.TripPlanner_ExtraVehicleRequired @WarehouseId", wareid).ToList();
                return list;
            }
        }
        [HttpGet]
        [Route("AvailableVehiclesDetails/{warehouseId}")]
        public List<AvailableVehiclesDetailsDC> AvailableVehiclesDetails(int warehouseId)
        {
            List<AvailableVehiclesDetailsDC> list = new List<AvailableVehiclesDetailsDC>();
            using (var authContext = new AuthContext())
            {
                var wareid = new SqlParameter("@WarehouseId", warehouseId);
                list = authContext.Database.SqlQuery<AvailableVehiclesDetailsDC>("Operation.TripPlanner_AvailableVehiclesDetails @WarehouseId", wareid).ToList();
                return list;
            }
        }
        [HttpGet]
        [Route("VehiclesNotUtilized/{warehouseId}")]
        public List<AvailableVehiclesDetailsDC> VehiclesNotUtilized(int warehouseId)
        {
            List<AvailableVehiclesDetailsDC> list = new List<AvailableVehiclesDetailsDC>();
            using (var authContext = new AuthContext())
            {
                var wareid = new SqlParameter("@WarehouseId", warehouseId);
                list = authContext.Database.SqlQuery<AvailableVehiclesDetailsDC>("Operation.TripPlanner_VehiclesNotUtilized @WarehouseId", wareid).ToList();
                return list;
            }
        }

        [Route("GenerateInvoiceManifest/{TripPlannerMasterId}")]
        [HttpGet]
        public async Task<ResultViewModel<TripBlockedOrderVM>> ShipmentManifestInvoice(long TripPlannerMasterId)
        {
            List<TripBlockedOrderVM> blockedOrderList = null;
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            using (var context = new AuthContext())
            {
                long tripPlannerConfirmedMasterId = context.TripPlannerConfirmedMasters.FirstOrDefault(x => x.TripPlannerMasterId == TripPlannerMasterId).Id;

                blockedOrderList = tripPlannerHelper.GetBlockedOrderList(tripPlannerConfirmedMasterId, context);
                if (blockedOrderList != null && blockedOrderList.Any())
                {
                    return new ResultViewModel<TripBlockedOrderVM>
                    {
                        ErrorMessage = "Order pending for Eway Bill and IRN",
                        IsSuceess = false,
                        ResultItem = null,
                        ResultList = blockedOrderList,
                        SuccessMessage = ""
                    };
                }
            }

            var invoice = await tripPlannerHelper.ShipmentManifest(TripPlannerMasterId);
            string fileName = Guid.NewGuid() + ".pdf";
            string downoadFolderName = ConfigurationManager.AppSettings["AllFileDownloadFolder"].ToString();

            string filePath = HttpContext.Current.Server.MapPath("~/" + downoadFolderName + "/TripPlannerManifest/");
            string headerFilePath = HttpContext.Current.Server.MapPath("~/Reports/TripPlannerManifestResources/Header.html");
            string bodyFilePath = HttpContext.Current.Server.MapPath("~/Reports/TripPlannerManifestResources/body.html");
            string bodyCSSFilePath = HttpContext.Current.Server.MapPath("~/Reports/TripPlannerManifestResources/body.css");
            string waterMarkImagePath = HttpContext.Current.Server.MapPath("~/Reports/TripPlannerManifestResources/logo.png");

            TripPlannerManifestHelper invoiceHelper = new TripPlannerManifestHelper(headerFilePath, bodyFilePath, bodyCSSFilePath);
            string css = invoiceHelper.GetBodyCSS();
            string body = invoiceHelper.GetBodyHtml(invoice);
            string header = invoiceHelper.GetHeaderHtml(invoice);
            PDFMaker invoicePDFMaker = new PDFMaker(filePath, fileName, header, body, css, waterMarkImagePath);
            invoicePDFMaker.SavePdf();


            return new ResultViewModel<TripBlockedOrderVM>
            {
                ErrorMessage = "",
                IsSuceess = true,
                ResultItem = null,
                ResultList = null,
                SuccessMessage = "/" + downoadFolderName + "/TripPlannerManifest/" + fileName
            };
        }

        [HttpPost]
        [Route("CreateCustomTrip")]
        public long CreateCustomTrip(CustomTrip customTrip)
        {
            int userId = GetLoginUserId();
            DateTime currentTime = DateTime.Now;
            DateTime today = DateTime.Today.AddDays(1).Date;
            using (var context = new AuthContext())
            {
                var warehouse = context.Warehouses.First(x => x.WarehouseId == customTrip.WarehouseId);
                var tripCount = context.TripPlannerMasters.Count(x => x.WarehouseId == customTrip.WarehouseId && x.TripDate == customTrip.TripDate && x.IsActive == true && x.IsDeleted == false);
                TripPlannerMaster master = new TripPlannerMaster
                {
                    AgentId = 0,
                    WarehouseId = customTrip.WarehouseId,
                    CreatedBy = userId,
                    CreatedDate = currentTime,
                    CustomerCount = 0,
                    DboyId = null,
                    Id = 0,
                    IsActive = true,
                    IsDeleted = false,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    OrderCount = 0,
                    TotalAmount = 0,
                    TotalDistanceInMeter = 0,
                    TotalTimeInMins = 0,
                    TotalWeight = 0,
                    TripDate = customTrip.TripDate,
                    TripNumber = tripCount + 1,
                    VehicleMasterId = customTrip.VehicleMasterId,
                    VehicleNumber = "",
                    WarehouseLat = warehouse.latitude,
                    WarehouseLng = warehouse.longitude
                };

                context.TripPlannerMasters.Add(master);
                context.Commit();

                TripPlannerConfirmedMaster confirmMaster = new TripPlannerConfirmedMaster
                {
                    IsManualTrip = true,
                    AgentId = 0,
                    WarehouseId = customTrip.WarehouseId,
                    CreatedBy = userId,
                    CreatedDate = currentTime,
                    CustomerCount = 0,
                    DboyId = null,
                    DriverId = null,
                    Id = 0,
                    IsActive = true,
                    IsDeleted = false,
                    IsFreezed = false,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    OrderCount = 0,
                    TotalAmount = 0,
                    TotalDistanceInMeter = 0,
                    TotalTimeInMins = 0,
                    TotalWeight = 0,
                    TripDate = customTrip.TripDate,
                    TripNumber = tripCount + 1,
                    TripPlannerConfirmedDetailList = null,
                    TripPlannerMasterId = master.Id,
                    VehicleMasterId = customTrip.VehicleMasterId,
                    VehicleNumber = "",
                    WarehouseLat = warehouse.latitude,
                    WarehouseLng = warehouse.longitude,
                    IsNotLastMileTrip = customTrip.TripType == (int)TripTypeEnum.City ? false : true,
                    TripTypeEnum = customTrip.TripType,
                    CustomerId = customTrip.TripType == (int)TripTypeEnum.SKP || customTrip.TripType == (int)TripTypeEnum.KPP ? customTrip.CustomerId : 0,
                    IsLocationEnabled = warehouse.IsLocationEnabled
                };
                List<TripPlannerConfirmedDetail> detilList = new List<TripPlannerConfirmedDetail>();
                detilList.Add(new TripPlannerConfirmedDetail
                {
                    CommaSeparatedOrderList = "",
                    CreatedBy = userId,
                    CreatedDate = currentTime,
                    CustomerId = 0,
                    IsActive = true,
                    IsDeleted = false,
                    IsProcess = false,
                    Lat = warehouse.latitude,
                    Lng = warehouse.longitude,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    OrderCount = 0,
                    SequenceNo = 1,
                    TotalAmount = 0,
                    TotalDistanceInMeter = 0,
                    TotalTimeInMins = 0,
                    TotalWeight = 0
                });
                confirmMaster.TripPlannerConfirmedDetailList = detilList;
                context.TripPlannerConfirmedMasters.Add(confirmMaster);
                context.Commit();
                return master.Id;
            }
        }

        [HttpPost]
        [Route("CreateCustomTripV1")]
        public long CreateCustomTripV1(CustomTrip customTrip)
        {
            int userId = GetLoginUserId();
            DateTime currentTime = DateTime.Now;
            DateTime today = DateTime.Today.AddDays(1).Date;
            using (var context = new AuthContext())
            {
                var warehouse = context.Warehouses.First(x => x.WarehouseId == customTrip.WarehouseId);
                var tripCount = context.TripPlannerMasters.Count(x => x.WarehouseId == customTrip.WarehouseId && x.TripDate == customTrip.TripDate && x.IsActive == true && x.IsDeleted == false);
                TripPlannerMaster master = new TripPlannerMaster
                {
                    AgentId = 0,
                    WarehouseId = customTrip.WarehouseId,
                    CreatedBy = userId,
                    CreatedDate = currentTime,
                    CustomerCount = 0,
                    DboyId = null,
                    Id = 0,
                    IsActive = true,
                    IsDeleted = false,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    OrderCount = 0,
                    TotalAmount = 0,
                    TotalDistanceInMeter = 0,
                    TotalTimeInMins = 0,
                    TotalWeight = 0,
                    TripDate = customTrip.TripDate,
                    TripNumber = tripCount + 1,
                    VehicleMasterId = 0,
                    VehicleNumber = "",
                    WarehouseLat = warehouse.latitude,
                    WarehouseLng = warehouse.longitude
                };

                context.TripPlannerMasters.Add(master);
                context.Commit();

                TripPlannerConfirmedMaster confirmMaster = new TripPlannerConfirmedMaster
                {
                    IsManualTrip = true,
                    AgentId = 0,
                    WarehouseId = customTrip.WarehouseId,
                    CreatedBy = userId,
                    CreatedDate = currentTime,
                    CustomerCount = 0,
                    DboyId = null,
                    DriverId = null,
                    Id = 0,
                    IsActive = true,
                    IsDeleted = false,
                    IsFreezed = false,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    OrderCount = 0,
                    TotalAmount = 0,
                    TotalDistanceInMeter = 0,
                    TotalTimeInMins = 0,
                    TotalWeight = 0,
                    TripDate = customTrip.TripDate,
                    TripNumber = tripCount + 1,
                    TripPlannerConfirmedDetailList = null,
                    TripPlannerMasterId = master.Id,
                    VehicleMasterId = 0,
                    VehicleNumber = "",
                    WarehouseLat = warehouse.latitude,
                    WarehouseLng = warehouse.longitude,
                    IsNotLastMileTrip = customTrip.TripType == (int)TripTypeEnum.City ? false : true,
                    TripTypeEnum = customTrip.TripType,
                    CustomerId = customTrip.TripType == (int)TripTypeEnum.SKP || customTrip.TripType == (int)TripTypeEnum.KPP ? customTrip.CustomerId : 0,
                    IsLocationEnabled = warehouse.IsLocationEnabled
                };
                List<TripPlannerConfirmedDetail> detilList = new List<TripPlannerConfirmedDetail>();
                detilList.Add(new TripPlannerConfirmedDetail
                {
                    CommaSeparatedOrderList = "",
                    CreatedBy = userId,
                    CreatedDate = currentTime,
                    CustomerId = 0,
                    IsActive = true,
                    IsDeleted = false,
                    IsProcess = false,
                    Lat = warehouse.latitude,
                    Lng = warehouse.longitude,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    OrderCount = 0,
                    SequenceNo = 1,
                    TotalAmount = 0,
                    TotalDistanceInMeter = 0,
                    TotalTimeInMins = 0,
                    TotalWeight = 0
                });
                confirmMaster.TripPlannerConfirmedDetailList = detilList;
                context.TripPlannerConfirmedMasters.Add(confirmMaster);
                context.Commit();
                return master.Id;
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetODDetailsList")]
        public List<ODDetailsList> GetODDetailsList(OrderIdListDc orderIdList)
        {

            string orderIds = "";
            if (orderIdList != null && orderIdList.OrderIdList != null && orderIdList.OrderIdList.Any())
            {
                orderIds = string.Join(",", orderIdList.OrderIdList);
            }

            string spName = "GetODDetailsList @OrderId";
            using (var context = new AuthContext())
            {
                var orderParam = new SqlParameter
                {
                    ParameterName = "OrderId",
                    Value = orderIds
                };

                var list = context.Database.SqlQuery<ODDetailsList>(spName, orderParam).ToList();
                return list;
            }
        }

        //[AllowAnonymous]
        //[HttpPost]
        //[Route("GetVechicleAttandance")]
        //public async Task<APIResponse> GetVechicleAttandance(TripPlannerVechicleAttandanceList tripPlannerVechicleAttandanceList)
        //{
        //    TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

        //    var list = await manager.GetVechicleAttandance(tripPlannerVechicleAttandanceList);
        //    return list;
        //}

        [AllowAnonymous]
        [HttpPost]
        [Route("InsertVechicleAttandance")]
        public async Task<AttendanceTripDetailDc> InsertVechicleAttandance(InsertVechicleAttandanceDc insertVechicleAttandanceDc)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();
            insertVechicleAttandanceDc.AttandanceDate = DateTime.Today;
            insertVechicleAttandanceDc.IsDateSendbyUser = false;
            return await manager.InsertVechicleAttandance(insertVechicleAttandanceDc);

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("VechicleMasterList")]
        public async Task<List<VehicleMasterList>> VechicleMasterList(string Keyword, int WarehouseId)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.VechicleMasterList(Keyword, WarehouseId);
            return list;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ExportTripPlannerVechicleAttandanceList")]
        public async Task<string> ExportTripPlannerVechicleAttandanceList(VechicleAttandanceExportList vechicleAttandanceExportList)
        {

            string fileUrl = string.Empty;
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.ExportTripPlannerVechicleAttandanceList(vechicleAttandanceExportList);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_TripPlannerVechicleAttandanceExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_TripPlannerVechicleAttandanceExport.csv";
            DataTable dt = ListtoDataTableConverter.ToDataTable(list);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int index = 0;
                dt.Columns["AttendanceDate"].DataType = typeof(string);
                foreach (var item in dt.Rows)
                {

                    string date1 = dt.Rows[index]["AttendanceDate"].ToString();
                    if (!string.IsNullOrEmpty(date1))
                    {
                        dt.Rows[index]["AttendanceDate"] = DateTime.Parse(dt.Rows[index]["AttendanceDate"].ToString()).ToString("dd-MM-yyyy");
                    }
                    index++;
                }
            }

            // rearrange DataTable columns
            dt.Columns["AttendanceDate"].SetOrdinal(0);
            dt.Columns["HubName"].SetOrdinal(1);
            dt.Columns["TransportName"].SetOrdinal(2);
            dt.Columns["Type"].SetOrdinal(3);
            dt.Columns["VehicleType"].SetOrdinal(4);
            dt.Columns["VehicleNo"].SetOrdinal(5);
            dt.Columns["MonthlycontractKM"].SetOrdinal(6);
            dt.Columns["UtilizedKM"].SetOrdinal(7);
            dt.Columns["MonthlyContractAmount"].SetOrdinal(8);
            dt.Columns["UtilizedAmount"].SetOrdinal(9);
            dt.Columns["OrderCount"].SetOrdinal(10);
            dt.Columns["ThresholdTouchPoint"].SetOrdinal(11);
            dt.Columns["ActualTouchPoint"].SetOrdinal(12);
            dt.Columns["ThresholdValueOfLoad"].SetOrdinal(13);
            dt.Columns["VisitedValueOfLoad"].SetOrdinal(14);
            dt.Columns["DeliveredValueOfLoad"].SetOrdinal(15);
            dt.Columns["StatusOfUtilization"].SetOrdinal(16);
            dt.Columns["DboyCost"].SetOrdinal(17);
            dt.Columns["ReplacementVehicleNo"].SetOrdinal(18);
            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt.WriteToCsvFile(path);


            return $"/ExcelGeneratePath/{fileName}";
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetAttandanceList")]
        public async Task<APIResponse> GetAttandanceList(VehicleAttandanceVm vehicleAttandanceVm)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.GetAttandanceList(vehicleAttandanceVm);
            return list;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DeleteVechicleAttandance")]
        public async Task<AttendanceTripDetailDc> DeleteVechicleAttandance(int AttandanceId, int UserId, long VehicleMasterId)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            return await manager.DeleteVechicleAttandance(AttandanceId, UserId, VehicleMasterId);

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("VechicleNumberList")]
        public async Task<List<VehicleNoList>> GetVechicleNumberList(int WarehouseId)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.GetVechicleNumberList(WarehouseId);
            return list;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("HistoryVechicleAttandanceList")]
        public async Task<APIResponse> HistoryAttandanceList(int Skip, int Take, long VehicleMasterId)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.HistoryAttandanceList(Skip, Take, VehicleMasterId);
            return list;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateTodayAttandance")]
        public StatusDc UpdateTodayAttandance(int WarehouseId)
        {
            TripPlannerVehicleManager manager = new TripPlannerVehicleManager();

            return manager.UpdateTodayAttandance(WarehouseId);

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ActiveStatuswiseAttandance")]
        public StatusDc ActiveStatuswiseAttandance(List<ActiveStatuswiseAttandanceDc> activeStatuswisAttandanceList)
        {
            int userid = GetLoginUserId();
            TripPlannerVehicleManager manager = new TripPlannerVehicleManager();

            return manager.ActiveStatuswiseAttandance(activeStatuswisAttandanceList, userid);

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("FutureAttandanceData")]
        public List<ActiveStatuswiseAttandanceDc> FutureAttandanceData(long VehicleMasterId, int WarehouseId, int month, int year)
        {
            TripPlannerVehicleManager manager = new TripPlannerVehicleManager();

            return manager.FutureAttandanceData(VehicleMasterId, WarehouseId, month, year);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetThresholdkgList")]
        public async Task<List<VehicleTypeDc>> GetThresholdkgList()
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var VehicleTypelist = await manager.GetThresholdkgList();
            return VehicleTypelist;
        }

        [HttpGet]
        [Route("IsAddNewPickerAllowed")]
        public bool IsAddNewPickerAllowed(long tripPlannerConfirmedMasterId)
        {
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            bool resultRes = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var authContext = new AuthContext())
                {
                    bool isNewPickerGenerated = tripPlannerHelper.

                        GetIsNewPickerAllowed(tripPlannerConfirmedMasterId, authContext);
                    if (isNewPickerGenerated)
                    {
                        scope.Complete();
                        resultRes = true;
                    }
                    else
                    {
                        scope.Dispose();
                        resultRes = false;
                    }
                }
            }
            return resultRes;
        }


        [HttpGet]
        [Route("IsAddNewPickerFinalized")]
        public bool IsAddNewPickerFinalized(long tripPlannerConfirmedMasterId)
        {
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            bool resultRes = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var authContext = new AuthContext())
                {
                    bool isNewPickerGenerated = tripPlannerHelper.GetIsAddNewPickerFinalized(tripPlannerConfirmedMasterId, authContext);
                    if (isNewPickerGenerated)
                    {
                        scope.Complete();
                        resultRes = true;
                    }
                    else
                    {
                        scope.Dispose();
                        resultRes = false;
                    }
                }
            }
            return resultRes;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ExportTripVechicleList")]
        public async Task<string> ExportTripVechicleList(VechicleAttandanceExportList vechicleAttandanceExportList)
        {
            string fileUrl = string.Empty;
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.ExportTripVechicleList(vechicleAttandanceExportList);
            string zipfilename = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_TripVechicleListExport.zip";
            var fileName = "_" + DateTime.Now.ToString("yyyyddMHHmmss") + "_TripVechicleListExport.csv";
            DataTable dt = ListtoDataTableConverter.ToDataTable(list);

            // rearrange DataTable columns

            dt.Columns["HubName"].SetOrdinal(0);
            dt.Columns["CreatedDate"].SetOrdinal(1);
            dt.Columns["VehicleNo"].SetOrdinal(2);
            dt.Columns["VehicleType"].SetOrdinal(3);
            dt.Columns["ClosingTime"].SetOrdinal(4);
            dt.Columns["TripNo"].SetOrdinal(5);
            dt.Columns["DriverName"].SetOrdinal(6);
            dt.Columns["Dboy"].SetOrdinal(7);
            dt.Columns["ReportingTime"].SetOrdinal(8);
            dt.Columns["StartKm"].SetOrdinal(9);
            dt.Columns["ClosingKm"].SetOrdinal(10);
            dt.Columns["TotalKm"].SetOrdinal(11);
            dt.Columns["OrderCount"].SetOrdinal(12);
            dt.Columns["OrderValue"].SetOrdinal(13);
            dt.Columns["DeliverdValue"].SetOrdinal(15);
            dt.Columns["DeliverdCount"].SetOrdinal(14);
            dt.Columns["DeliverdPercent"].SetOrdinal(16);
            dt.Columns["RedispatchValue"].SetOrdinal(18);
            dt.Columns["RedispatchCount"].SetOrdinal(17);
            dt.Columns["RedispatchPercent"].SetOrdinal(19);
            dt.Columns["DCCount"].SetOrdinal(20);
            dt.Columns["DCValue"].SetOrdinal(21);
            dt.Columns["DCPercent"].SetOrdinal(22);
            dt.Columns["DboyCost"].SetOrdinal(23);
            dt.Columns["ExpectedKm"].SetOrdinal(24);
            dt.Columns["ReplacementVehicleNo"].SetOrdinal(25);
            dt.Columns["TransportName"].SetOrdinal(26);

            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            dt.WriteToCsvFile(path);


            return $"/ExcelGeneratePath/{fileName}";
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAdhocFleetList")]
        public async Task<List<AdhocFleetDc>> GetAdhocFleetList(int WarehouseId)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.GetAdhocFleetList(WarehouseId);
            return list;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AddNewAdhocVehicle")]
        public async Task<long> AddNewAdhocVehicle(AddNewAdhocVehicleDc addNewAdhocVehicleDc)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.AddNewAdhocVehicle(addNewAdhocVehicleDc);
            return list;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetFleetTypeList")]
        public async Task<List<string>> GetFleetTypeList(int WarehouseId)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.GetFleetTypeList(WarehouseId);
            return list;
        }

        [HttpGet]
        [Route("ChangePeoplePassword")]
        public APIResponse ChangePassword(int peopleId, string newPassword)
        {

            using (AuthContext context = new AuthContext())
            {
                if (peopleId != null && newPassword != null)
                {
                    People people = context.Peoples.Where(x => x.PeopleID == peopleId).FirstOrDefault();
                    people.Password = newPassword;
                    people.UpdatedDate = DateTime.Now;
                    context.Entry(people).State = EntityState.Modified;
                    context.Commit();
                    return new APIResponse { Status = true, Message = "Password Change Successfully" };
                }
                else
                {
                    return new APIResponse { Status = false, Message = "Password Not Change" };
                }
            }

        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetVehicleNumberList")]
        public async Task<List<VehicleNoList>> GetVehicleNumberList(List<int> WarehouseIds)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.GetVehicleNumberList(WarehouseIds);
            return list;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("GetAllVehicleReportOfDate")]
        public async Task<List<AllVehicleReportDC>> GetAllVehicleReportOfDate(VechicleExportDC vechicleExportDC)
        {
            TripPlannerVechicleAttandanceManager manager = new TripPlannerVechicleAttandanceManager();

            var list = await manager.GetAllVehicleReportOfDate(vechicleExportDC);
            return list;
        }

        [HttpPost]
        [Route("GetFixedCost")]
        public async Task<double> GetFixedCost(int vehicleMasterId)
        {
            double result = 0;
            using (var context = new AuthContext())
            {
                var query = from d in context.FleetMasterDetailDB
                            join v in context.VehicleMasterDB on d.Id equals v.FleetDetailId
                            where v.Id == vehicleMasterId
                            select d;

                var data = query.FirstOrDefault();
                if(data !=null)
                {
                     result = data.FixedCost / 26.0;
                }
                return result;
            }
            
        }


        public class FinalizeTripParam
        {
            public long tripPlannerConfirmedMasterId { get; set; }
            public int startingKm { get; set; }
            public DateTime reportingTime { get; set; }

            public int? lateReportingTimeInMins { get; set; }
            public double? penaltyCharge { get; set; }
            public long DriverId { get; set; }
            public long VehicleId { get; set; }
            public long? AgentId { get; set; }
            public long? DboyId { get; set; }
            public DateTime TripDate { get; set; }
            public bool? IsReplacementVehicleNo { get; set; }
            public string ReplacementVehicleNo { get; set; }
            public double? VehicleFare { get; set; }
        }
        public class CustomTrip
        {
            public int TripNumber { get; set; }
            public long VehicleMasterId { get; set; }
            //public long ClusterId { get; set; }
            public long? AgentId { get; set; }
            public int WarehouseId { get; set; }
            public int TripType { get; set; }
            public int CustomerId { get; set; }
            public DateTime TripDate { get; set; }
        }
        public class VehicleSummaryDC
        {
            public int AvailableVehicleCount { get; set; }
            public int NotUtilizedVehicleCount { get; set; }
            public double? ExtraLoadInKG { get; set; }
            public double? ExtraLoadAmount { get; set; }
            public int OrderCount { get; set; }
        }
        public class ExtraVehicleRequiredDC
        {
            public int OrderId { get; set; }
            public string Skcode { get; set; }
            public string WarehouseName { get; set; }
            public string Customerphonenum { get; set; }
            public double GrossAmount { get; set; }
            public double OrderWeight { get; set; }

        }
        public class AvailableVehiclesDetailsDC
        {
            public string VehicleType { get; set; }
            public string FleetType { get; set; }
            public double VehicleWeight { get; set; }
            public string TripType { get; set; }
        }

        public class MapCheckPoint
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
            public long? CustomerId { get; set; }
            public double? TotalAmount { get; set; }
            public long TotalDistanceInMeter { get; set; }
            public string Skcode { get; set; }

        }
        public class ODDetailsList
        {
            public string itemname { get; set; }
            public int qty { get; set; }
            public double UnitPrice { get; set; }
            public double TotalAmt { get; set; }
            public string OrderIds { get; set; }

        }


        public class OrderIdListDc
        {
            public List<int> OrderIdList { get; set; }

        }

        public class GetCustomersByIdDC
        {
            public int CustomerId { get; set; }
            public string Skcode { get; set; }
            public string GSTNo { get; set; }
            public string CustomerType { get; set; }
            public string FcmId { get; set; }
            public bool IsGstRequestPending { get; set; }
            public bool IsGenerateIRN { get; set; }

        }

        public class PickerOrderVM
        {
            public int OrderId { get; set; }
            public long? OrderPickerMasterId { get; set; }
        }


    }
}