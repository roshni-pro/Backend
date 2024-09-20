using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.ControllerV7.VehicleMaster;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.Zila;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.API.Managers.TripPlanner;
using AngularJSAuthentication.API.Managers.Zila;
using AngularJSAuthentication.BusinessLayer.Managers.Zila;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Controllers;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.External.MobileExecutiveDC;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.DataContracts.Transaction.Zila.OperationCapacity;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.DeliveryOptimization;
using AngularJSAuthentication.Model.TripPlanner;
using AngularJSAuthentication.Model.VAN;
using AngularJSAuthentication.Model.Zila.OperationCapacity;
using GenricEcommers.Models;
using LinqKit;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.External.Other.SellerStoreController;
using static AngularJSAuthentication.API.ControllerV7.VehicleMaster.PlanMasterController;

namespace AngularJSAuthentication.API.ControllerV7.Zila
{
    [RoutePrefix("api/ZilaDeliveryApp")]
    public class ZilaDeliveryAppController : BaseApiController
    {
        public static List<long> ordersToProcess = new List<long>();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllTrip")]
        public List<GetZilaTripDc> GetAllTrip(int DboyId)
        {
            List<GetZilaTripDc> getTripDc = new List<GetZilaTripDc>();
            using (var db = new AuthContext())
            {
                var dboyId = new SqlParameter("@DboyId", DboyId);
                getTripDc = db.Database.SqlQuery<GetZilaTripDc>("EXEC [Zila].[ZilaTripPlanner_GetMobileAppTripList] @DboyId", dboyId).ToList();
            }
            return getTripDc;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("CreateCustomTripV1")]
        public APIResponse CreateCustomTripV1(ZilaCustomTrip customTrip)
        {
            var result = CreateCustomTripPvt(customTrip);
            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("UpdateOrder/{ZilaTripMasterId}")]
        public APIResponse UpdateOrder([FromUri] long ZilaTripMasterId, [FromUri] bool IsNewPickerOrder, [FromBody] List<ZilaTripOrderVM> orderList)
        {
            if (orderList != null && orderList.Where(x => x.IsActive).Count() < 1)
            {
                return new APIResponse { Status = false, Message = "Data Not Saved", Data = new List<string>() };
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
                    var param = new SqlParameter("customerids", dt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";

                    if (true)
                    {
                        var orderids = orderList.Where(x => x.ZilaTripOrderId == 0 && x.IsActive == true).Select(x => x.OrderId).ToList();
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
                            var zilaTripMasterIds = new SqlParameter("@ZilaTripMasterId", ZilaTripMasterId);

                            IsAlreadyOrder = authContext.Database.SqlQuery<bool>("EXEC [Zila].Zila_GetTripDuplicateOrderCheck @OrderIds,@ZilaTripMasterId", OrderIds, zilaTripMasterIds).First();
                        }
                        if (!IsAlreadyOrder)
                        {
                            //TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                            //bool isNewPickerChangesAllowed = tripPlannerHelper.CanAddOrderInNewPicker(ZilaTripMasterId, authContext);
                            var master = authContext.ZilaTripMasters.FirstOrDefault(x => x.Id == ZilaTripMasterId);

                            //if (master.IsFreezed || (master.IsPickerGenerated && !IsNewPickerOrder) || (IsNewPickerOrder && !isNewPickerChangesAllowed))
                            //{
                            //    scope.Dispose();
                            //    return new APIResponse { Status = false, Message = "Data Not Saved" };
                            //} SEE 

                            ZilaTripMaster zilaTripMaster = authContext.ZilaTripMasters
                                .Include("ZilaTripDetailList.ZilaTripOrderList")
                                .Where(x => x.Id == ZilaTripMasterId)
                                .FirstOrDefault();

                            foreach (var orderId in orderIdList)
                            {
                                var order = orderList.FirstOrDefault(x => x.OrderId == orderId);
                                var selectedDetail = zilaTripMaster.ZilaTripDetailList
                                    .Where(x => x.CustomerId == order.CustomerId
                                    && x.ZilaTripMasterId == ZilaTripMasterId)
                                    .FirstOrDefault();


                                if (order.ZilaTripOrderId == 0 && order.IsActive)
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
                                        selectedDetail = new ZilaTripDetail
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
                                            SequenceNo = zilaTripMaster.ZilaTripDetailList.Where(x => x.IsActive == true && x.IsDeleted == false).Count() + 1,
                                            TotalAmount = 0,
                                            TotalDistanceInMeter = 0,
                                            TotalTimeInMins = 0,
                                            TotalWeight = 0,
                                            ZilaTripMasterId = zilaTripMaster.Id,
                                            ZilaTripOrderList = new List<ZilaTripOrder>()

                                        };
                                        zilaTripMaster.ZilaTripDetailList.Add(selectedDetail);
                                    }

                                    if (selectedDetail.ZilaTripOrderList != null
                                        && !selectedDetail.ZilaTripOrderList.Any(x => x.IsActive == true && x.OrderId == order.OrderId))
                                    {
                                        selectedDetail.ZilaTripOrderList.Add(new ZilaTripOrder
                                        {
                                            Amount = order.Amount,
                                            CreatedBy = userId,
                                            CreatedDate = DateTime.Now,
                                            DistanceInMeter = order.DistanceInMeter,
                                            Id = 0,
                                            IsActive = true,
                                            IsDeleted = false,
                                            //IsManuallyAdded = true,
                                            ModifiedBy = null,
                                            ModifiedDate = null,
                                            OrderId = order.OrderId,
                                            TimeInMins = order.TimeInMins,
                                            ZilaTripDetailId = selectedDetail.Id,
                                            WeightInKg = order.WeightInKg,
                                            PaymentMode = "",
                                            WorkingStatus = 0,
                                            OrderStatus = 0,
                                            PaymentRefId = ""
                                            //IsNewPickerOrder = order.IsNewPickerOrder
                                        });
                                    }
                                }
                                var selectedOrder = selectedDetail.ZilaTripOrderList
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

                            if (zilaTripMaster.ZilaTripDetailList != null && zilaTripMaster.ZilaTripDetailList.Any())
                            {
                                var detail = zilaTripMaster.ZilaTripDetailList.FirstOrDefault(x => x.CustomerId == 0);
                                detail.SequenceNo = 1000;
                            }
                            int seq = 1;
                            foreach (var detail in zilaTripMaster.ZilaTripDetailList.OrderBy(x => x.SequenceNo).ToList())
                            {
                                detail.SequenceNo = seq++;
                                if (detail.ZilaTripOrderList.Where(x => x.IsActive == true).Count() > 0)
                                {
                                    detail.CommaSeparatedOrderList
                                        = String.Join(", ", detail.ZilaTripOrderList.Where(x => x.IsActive == true).Select(x => x.OrderId).Distinct());
                                }
                                else
                                {
                                    detail.CommaSeparatedOrderList = "";
                                }
                            }

                            zilaTripMaster.CustomerCount
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0).Count();
                            zilaTripMaster.OrderCount
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0).Sum(x => x.OrderCount);
                            zilaTripMaster.TotalAmount
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0).Sum(x => x.TotalAmount);
                            zilaTripMaster.TotalDistanceInMeter
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalDistanceInMeter);
                            zilaTripMaster.TotalTimeInMins
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalTimeInMins);
                            zilaTripMaster.TotalWeight
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0).Sum(x => x.TotalWeight);
                            zilaTripMaster.ModifiedBy = userId;
                            zilaTripMaster.ModifiedDate = DateTime.Now;

                            authContext.Commit();
                            scope.Complete();
                            return new APIResponse { Status = true }; //, Data = PendingSKcodess
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
        [Route("ZilaCreateTrip")]
        [AllowAnonymous]
        public async Task<ResponceMsg> CreateTrip(ZilaFinalizeTripParam param)
        {
            SalesReturnOrderHelper salesReturnOrderHelper = new SalesReturnOrderHelper();
            string userName = "";
            var identity = User.Identity as ClaimsIdentity;
            ResponceMsg res = new ResponceMsg();
            int userId = (int)param.DboyId;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                userName = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            ZilaTripMaster master = null;
            bool IsDboyUpdated = false;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var authContext = new AuthContext())
                {

                    param.DboyId = authContext.DboyMasters.FirstOrDefault(x => x.PeopleId == param.DboyId.Value).Id;

                    master = authContext.ZilaTripMasters.FirstOrDefault(x => x.Id == param.ZilaTripMasterId);

                    //#region Sales Return Checking Exist Return Order

                    //List<long> returnOrderIds = new List<long>();

                    //returnOrderIds = (from td in authContext.ZilaTripDetails
                    //                  join to in authContext.ZilaTripOrders
                    //                  on td.Id equals to.ZilaTripDetailId
                    //                  where td.IsActive == true && td.IsDeleted == false
                    //                  && to.IsActive == true && to.IsDeleted == false && td.CustomerId > 0
                    //                  && td.ZilaTripMasterId == master.Id
                    //                  select to.OrderId).ToList();

                    //if (returnOrderIds != null && returnOrderIds.Count > 0)
                    //{
                    //    var existReturnOrder = authContext.DbOrderMaster.Where(x => returnOrderIds.Contains(x.OrderId) && x.OrderType == 3).Include("OrderDetails").ToList();
                    //    var orderDispatchedMaster = authContext.OrderDispatchedMasters.Where(x => returnOrderIds.Contains(x.OrderId)).ToList();
                    //    if (existReturnOrder.Count > 0 && existReturnOrder.Count == returnOrderIds.Count)
                    //    {
                    //        var changeDboyId = new SqlParameter("@ChangeDboyId", param.DboyId.Value);
                    //        var data = authContext.Database.SqlQuery<TripChangeBoyDC>("exec Operation.TripPlanner_TripChangeDboy @ChangeDboyId", changeDboyId).FirstOrDefault();

                    //        foreach (var x in existReturnOrder)
                    //        {
                    //            x.Status = "Shipped";
                    //            foreach (var od in x.orderDetails)
                    //            {
                    //                od.Status = "Shipped";
                    //            }
                    //            authContext.Entry(x).State = EntityState.Modified;
                    //        }


                    //        if (master != null)
                    //        {
                    //            master.DriverId = param.DriverId;
                    //            master.VehicleMasterId = param.VehicleId;
                    //            master.AgentId = param.AgentId;
                    //            master.DboyId = data.DboyMasterId;
                    //           // master.IsVisibleToDboy = true;
                    //            master.IsFreezed = true;
                    //            master.ModifiedBy = userId;
                    //            master.ModifiedDate = DateTime.Today;
                    //           // master.VehicleFare = param.VehicleFare.HasValue ? param.VehicleFare.Value : 0;
                    //            authContext.Entry(master).State = EntityState.Modified;

                    //            foreach (var order in orderDispatchedMaster)
                    //            {
                    //                OrderMasterHistories h1 = new OrderMasterHistories();
                    //                h1.orderid = order.OrderId;
                    //                h1.Status = order.Status;
                    //                h1.Reasoncancel = "Due to Change in delivery Boy from " + order.DboyName + " " + order.DboyMobileNo + " To " + data.Dboyname + " " + data.DboyMobile + " " + "by Trip";
                    //                h1.Warehousename = order.WarehouseName;
                    //                h1.username = userName;
                    //                h1.userid = userId;
                    //                h1.CreatedDate = DateTime.Now;
                    //                authContext.OrderMasterHistoriesDB.Add(h1);
                    //                order.DBoyId = data.PeopleID;
                    //                order.DboyName = data.Dboyname;
                    //                order.DboyMobileNo = data.DboyMobile;
                    //                order.Status = "Shipped";

                    //                authContext.Entry(order).State = EntityState.Modified;

                    //                bool resStatus = salesReturnOrderHelper.PostOrderStatus(order.OrderId, "Shipped", userId, authContext);
                    //            }
                    //            ZilaTripPlannerHelper tripPlanner = new ZilaTripPlannerHelper();
                    //            res = tripPlanner.AssignmentFinalize(param.ZilaTripMasterId, userId, authContext, scope, param.startingKm, param.reportingTime, null,null, true);
                    //            if (res.Status)
                    //            {
                    //                scope.Complete();
                    //                return res;
                    //            }
                    //            else
                    //            {
                    //                scope.Dispose();
                    //                return res;
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        foreach (var x in existReturnOrder)
                    //        {
                    //            x.Status = "Shipped";
                    //            foreach (var od in x.orderDetails)
                    //            {
                    //                od.Status = "Shipped";
                    //            }
                    //            authContext.Entry(x).State = EntityState.Modified;
                    //        }
                    //        var ReturnorderDispatchedMaster = orderDispatchedMaster.Where(x => existReturnOrder.Select(y => y.OrderId).ToList().Contains(x.OrderId)).Distinct().ToList();
                    //        foreach (var x in ReturnorderDispatchedMaster)
                    //        {
                    //            x.Status = "Shipped";
                    //            authContext.Entry(x).State = EntityState.Modified;
                    //            bool resStatus = salesReturnOrderHelper.PostOrderStatus(x.OrderId, "ReturnShipped", userId, authContext);
                    //        }
                    //    }
                    //}
                    //#endregion

                    IsDboyUpdated = true;
                    ZilaTripPlannerHelper zilatripPlannerHelper = new ZilaTripPlannerHelper();
                    bool isNotADamageTrip = true;
                    //if ((master.TripTypeEnum == (int)TripTypeEnum.Damage_Expiry || master.TripTypeEnum == (int)TripTypeEnum.NonSellable))
                    //{
                    //    isNotADamageTrip = false;
                    //}

                    //bool isPickerAlreadyGenerated = tripPlannerHelper.GetIsPickerGenerated(param.ZilaTripMasterId, authContext);
                    //bool isAddNewPickerFinalized = tripPlannerHelper.GetIsAddNewPickerFinalized(param.ZilaTripMasterId, authContext);

                    //if (!isPickerAlreadyGenerated && isNotADamageTrip && isAddNewPickerFinalized)
                    //{
                    //    res = new ResponceMsg()
                    //    {
                    //        Message = "Trip not finalized because picker not generated yet",
                    //        Status = false
                    //    };
                    //    scope.Dispose();
                    //    return res;
                    //}

                    bool isPickerFinalzied = zilatripPlannerHelper.IsPickerFinalzied(param.ZilaTripMasterId, authContext);
                    if (!isPickerFinalzied)
                    {
                        res = new ResponceMsg()
                        {
                            Message = "Trip not finalized because some assignment not finalized",
                            Status = false
                        };
                        scope.Dispose();
                        return res;
                    }

                    bool IsRunningTrip = zilatripPlannerHelper.GetIsTripRunning(master.VehicleMasterId, param.ZilaTripMasterId, authContext);
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


                    //GetReplaceVehicleListDc ReplaceVehicleList = null;
                    //if (!string.IsNullOrEmpty(param.ReplacementVehicleNo))
                    //    ReplaceVehicleList = tripPlannerHelper.GetReplaceVehicle(master.Id, authContext, param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString());
                    //if (ReplaceVehicleList != null && !ReplaceVehicleList.IsAlreadyEwaybillGenerate)
                    //{
                    //    if (!ReplaceVehicleList.IsExistsVehicle)
                    //    {
                    //        var vehicleAttandance = authContext.TripPlannerVechicleAttandanceDb.FirstOrDefault(x => x.VehicleMasterId == param.VehicleId && x.AttendanceDate == master.TripDate && x.IsActive && x.IsDeleted == false);
                    //        if (vehicleAttandance != null)
                    //        {
                    //            if (param.IsReplacementVehicleNo.HasValue && string.IsNullOrEmpty(vehicleAttandance.ReplacementVehicleNo))
                    //            {
                    //                vehicleAttandance.IsReplacementVehicleNo = param.IsReplacementVehicleNo;
                    //                vehicleAttandance.ReplacementVehicleNo = param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString();
                    //                vehicleAttandance.ModifiedDate = DateTime.Now;
                    //                vehicleAttandance.ModifiedBy = userId;
                    //                authContext.Entry(vehicleAttandance).State = EntityState.Modified;
                    //            }
                    //            else if (!string.IsNullOrEmpty(vehicleAttandance.ReplacementVehicleNo) && !string.IsNullOrEmpty(param.ReplacementVehicleNo)
                    //                && vehicleAttandance.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString()
                    //                == param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString() && param.IsReplacementVehicleNo.HasValue)
                    //            {
                    //                vehicleAttandance.IsReplacementVehicleNo = param.IsReplacementVehicleNo;
                    //                vehicleAttandance.ReplacementVehicleNo = param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString();
                    //                vehicleAttandance.ModifiedDate = DateTime.Now;
                    //                vehicleAttandance.ModifiedBy = userId;
                    //                authContext.Entry(vehicleAttandance).State = EntityState.Modified;
                    //            }
                    //            else if (!string.IsNullOrEmpty(vehicleAttandance.ReplacementVehicleNo) && !string.IsNullOrEmpty(param.ReplacementVehicleNo)
                    //                && vehicleAttandance.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString() != param.ReplacementVehicleNo.Trim().Replace(" ", "").ToUpper().ToString())
                    //            {
                    //                res = new ResponceMsg()
                    //                {
                    //                    Message = "Trip alredy Running for this Replacement vehicle",
                    //                    Status = false
                    //                };
                    //                scope.Dispose();
                    //                return res;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            res = new ResponceMsg()
                    //            {
                    //                Message = "Today this vehicle Absent!!",
                    //                Status = false
                    //            };
                    //            scope.Dispose();
                    //            return res;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        res = new ResponceMsg()
                    //        {
                    //            Message = "Replacement vehicle and Actual vehicle is same!!",
                    //            Status = false
                    //        };
                    //        scope.Dispose();
                    //        return res;
                    //    }
                    //}

                    blockedOrderList = zilatripPlannerHelper.GetBlockedOrderList(master.Id, authContext);
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

                    zilatripPlannerHelper.UpdateTripSequenceNew(param.ZilaTripMasterId, authContext);

                    zilatripPlannerHelper.UpdateAmount(param.ZilaTripMasterId, authContext);
                    if (IsDboyUpdated)
                    {
                        bool result = zilatripPlannerHelper.TripChangeDBoy(master.Id, param.DboyId.Value, authContext, userId, userName);
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
                    ZilaTripPlannerHelper zilatripPlannerHelper = new ZilaTripPlannerHelper();
                    master.DriverId = 0;
                    master.VehicleMasterId = param.VehicleId;
                    master.AgentId = 0;
                    master.DboyId = param.DboyId;
                    // master.VehicleFare = param.VehicleFare.HasValue ? param.VehicleFare.Value : 0;
                    authContext.Entry(master).State = EntityState.Modified;
                    res = zilatripPlannerHelper.AssignmentFinalize(param.ZilaTripMasterId, userId, authContext, scope, param.startingKm, param.reportingTime, null, null);
                    bool result = zilatripPlannerHelper.SetTripDboyVisibiltyStatus(param.ZilaTripMasterId, authContext);

                    //TripPlannerVechicleAttandanceManager tripPlannerVechicleAttandanceManager = new TripPlannerVechicleAttandanceManager();
                    //InsertVechicleAttandanceDc insertVechicleAttandanceDc = new InsertVechicleAttandanceDc
                    //{
                    //    AttandanceDate = master.TripDate,
                    //    IsDateSendbyUser = true,
                    //    VehicleMasterId = master.VehicleMasterId,
                    //    IsTodayAttendance = true,
                    //    WarehouseId = master.WarehouseId
                    //};
                    //var updateAdhocAttandance = tripPlannerHelper.InsertVechicleAttandance(insertVechicleAttandanceDc, authContext);

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

        [AllowAnonymous]
        [HttpPost]
        [Route("AddOrder")]
        public APIResponse AddOrder([FromUri] long ZilaTripMasterId, [FromUri] int orderId, [FromUri] int dboyId)
        {
            APIResponse res = null;
            if (ZilaTripMasterId == 0)
            {
                res = CreateCustomTripPvt(new ZilaCustomTrip
                {
                    DboyId = dboyId,
                    TripNumber = 0,
                    VehicleMasterId = 0
                });

                ZilaTripMasterId = (long)res.Data;
            }

            if (ZilaTripMasterId == 0)
            {
                res.Data = new List<string>();
                return res;
            }
            int userId = dboyId;

            OrderMaster orderMaster = null;
            OrderDispatchedMaster orderDispatchedMaster = null;
            bool isAssignmentCreatedForSameDboy = false;
            using (var authContext = new AuthContext())
            {
                orderMaster = authContext.DbOrderMaster.FirstOrDefault(x => x.OrderId == orderId && x.OrderType == 11 && x.Status == "Issued");
                orderDispatchedMaster = authContext.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderId && x.DeliveryIssuanceIdOrderDeliveryMaster > 0);
                var dboyWarehouse = authContext.DboyMasters.FirstOrDefault(x => x.PeopleId == userId);
                int warehouseId = 0;
                if (dboyWarehouse != null)
                {
                    warehouseId = dboyWarehouse.WarehouseId;
                }
                else
                {
                    return new APIResponse { Status = false, Message = "dboy not found", Data = new List<string>() };

                }
                if (orderDispatchedMaster == null && orderDispatchedMaster.WarehouseId != warehouseId)
                {
                    return new APIResponse { Status = false, Message = "order belongs to different warehouse", Data = new List<string>() };
                }
            }

            #region Assignment Created For Same Dboy work
            //pending
            #endregion

            if (orderMaster == null && !isAssignmentCreatedForSameDboy)
            {
                return new APIResponse { Status = false, Message = "order not found", Data = new List<string>() };
            }

            if (orderDispatchedMaster == null)
            {
                return new APIResponse { Status = false, Message = "Assignment Not Created", Data = new List<string>() };
            }

            bool IsAlreadyOrder = false;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var authContext = new AuthContext())
                {


                    if (true)
                    {
                        var orderids = new List<int> { orderMaster.OrderId };
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
                            var zilaTripMasterIds = new SqlParameter("@ZilaTripMasterId", ZilaTripMasterId);

                            IsAlreadyOrder = authContext.Database.SqlQuery<bool>("EXEC [Zila].Zila_GetTripDuplicateOrderCheck @OrderIds,@ZilaTripMasterId", OrderIds, zilaTripMasterIds).First();
                        }
                        if (!IsAlreadyOrder)
                        {
                            //TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                            //bool isNewPickerChangesAllowed = tripPlannerHelper.CanAddOrderInNewPicker(ZilaTripMasterId, authContext);
                            var master = authContext.ZilaTripMasters.FirstOrDefault(x => x.Id == ZilaTripMasterId);

                            //if (master.IsFreezed || (master.IsPickerGenerated && !IsNewPickerOrder) || (IsNewPickerOrder && !isNewPickerChangesAllowed))
                            //{
                            //    scope.Dispose();
                            //    return new APIResponse { Status = false, Message = "Data Not Saved" };
                            //} SEE 

                            ZilaTripMaster zilaTripMaster = authContext.ZilaTripMasters
                                .Include("ZilaTripDetailList.ZilaTripOrderList")
                                .Where(x => x.Id == ZilaTripMasterId)
                                .FirstOrDefault();


                            var order = orderId;
                            var selectedDetail = zilaTripMaster.ZilaTripDetailList
                                .Where(x => x.CustomerId == orderMaster.CustomerId
                                && x.ZilaTripMasterId == ZilaTripMasterId && x.IsActive && x.IsDeleted == false)
                                .FirstOrDefault();


                            if (true)
                            {
                                if (selectedDetail == null)
                                {
                                    if (true)
                                    {
                                        var customer = authContext.Customers.FirstOrDefault(x => x.CustomerId == orderMaster.CustomerId);
                                        GeoHelper geoHelper = new GeoHelper();
                                        decimal? lat, longitude;
                                        geoHelper.GetLatLongWithZipCode(customer.ShippingAddress, customer.City, customer.ZipCode, out lat, out longitude);
                                        var caList = authContext.ConsumerAddressDb.Where(x => x.CustomerId == orderMaster.CustomerId && x.IsActive == true).OrderByDescending(x => x.CreatedDate ).ToList();
                                        var ca = caList.FirstOrDefault(x => x.CompleteAddress == orderMaster.ShippingAddress);
                                        if (ca != null)
                                        {
                                            customer.lat = (double)ca.lat;
                                            customer.lg = (double)ca.lng;
                                        }
                                        //if (customer.lat > -90 && customer.lat < 90 && customer.lg > -180 && customer.lg < 180)
                                        //{
                                        //    orderMaster.Lat = (double)lat.Value;
                                        //    orderMaster.Lng = (double)longitude.Value;
                                        //}
                                        else if (lat != null && longitude != null && lat > -90 && lat < 90 && longitude > -180 && longitude < 180)
                                        {
                                            customer.lat = (double)lat.Value;
                                            customer.lg = (double)longitude.Value;
                                            customer.UpdatedDate = DateTime.Now;
                                            customer.LastModifiedBy = authContext.Peoples.Where(x => x.PeopleID == userId).Select(y => y.DisplayName).FirstOrDefault();
                                            authContext.Commit();
                                            orderMaster.Lat = 0;
                                            orderMaster.Lng = 0;
                                        }
                                        else
                                        {
                                            orderMaster.Lat = (double)customer.lat;
                                            orderMaster.Lng = (double)customer.lg;
                                            //scope.Dispose();
                                            ////return false;
                                            //return new APIResponse { Status = false, Message = "Data Not Saved" };
                                        }
                                    }
                                    selectedDetail = new ZilaTripDetail
                                    {
                                        CreatedBy = userId,
                                        CommaSeparatedOrderList = "",
                                        CreatedDate = DateTime.Now,
                                        Id = 0,
                                        CustomerId = orderMaster.CustomerId,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Lat = orderMaster.Lat.Value,
                                        Lng = orderMaster.Lng.Value,
                                        ModifiedBy = null,
                                        ModifiedDate = null,
                                        OrderCount = 0,
                                        SequenceNo = zilaTripMaster.ZilaTripDetailList.Where(x => x.IsActive == true && x.IsDeleted == false).Count() + 1,
                                        TotalAmount = 0,
                                        TotalDistanceInMeter = 0,
                                        TotalTimeInMins = 0,
                                        TotalWeight = 0,
                                        ZilaTripMasterId = zilaTripMaster.Id,
                                        ZilaTripOrderList = new List<ZilaTripOrder>()

                                    };
                                    zilaTripMaster.ZilaTripDetailList.Add(selectedDetail);
                                }

                                if (selectedDetail.ZilaTripOrderList != null
                                    && !selectedDetail.ZilaTripOrderList.Any(x => x.IsActive == true && x.OrderId == orderMaster.OrderId))
                                {
                                    selectedDetail.ZilaTripOrderList.Add(new ZilaTripOrder
                                    {
                                        Amount = orderMaster.GrossAmount,
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.Now,
                                        DistanceInMeter = 0,
                                        Id = 0,
                                        IsActive = true,
                                        IsDeleted = false,
                                        //IsManuallyAdded = true,
                                        ModifiedBy = null,
                                        ModifiedDate = null,
                                        OrderId = orderId,
                                        TimeInMins = 0,
                                        ZilaTripDetailId = selectedDetail.Id,
                                        WeightInKg = 0,
                                        PaymentMode = "",
                                        WorkingStatus = 0,
                                        OrderStatus = 0,
                                        PaymentRefId = ""
                                        //IsNewPickerOrder = order.IsNewPickerOrder
                                    });
                                }
                            }
                            var selectedOrder = selectedDetail.ZilaTripOrderList
                                                   .Where(y => y.OrderId == orderId && y.IsDeleted == false)
                                                   .FirstOrDefault();
                            if (selectedOrder != null)
                            {
                                selectedOrder.IsActive = true;
                                selectedOrder.IsDeleted = false;


                                selectedDetail.IsActive = true;
                                selectedDetail.IsDeleted = false;
                                selectedDetail.TotalAmount = selectedDetail.ZilaTripOrderList != null ? selectedDetail.ZilaTripOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Sum(y => y.Amount) : 0;
                                selectedDetail.TotalTimeInMins += selectedOrder.TimeInMins;
                                selectedDetail.TotalWeight += selectedOrder.WeightInKg;
                                selectedDetail.OrderCount = selectedDetail.ZilaTripOrderList != null ? selectedDetail.ZilaTripOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Count() : 0;

                            }


                            if (zilaTripMaster.ZilaTripDetailList != null && zilaTripMaster.ZilaTripDetailList.Any())
                            {
                                var detail = zilaTripMaster.ZilaTripDetailList.FirstOrDefault(x => x.CustomerId == 0);
                                detail.SequenceNo = 1000;
                            }
                            int seq = 1;
                            foreach (var detail in zilaTripMaster.ZilaTripDetailList.OrderBy(x => x.SequenceNo).ToList())
                            {
                                detail.SequenceNo = seq++;
                                if (detail.ZilaTripOrderList.Where(x => x.IsActive == true).Count() > 0)
                                {
                                    detail.CommaSeparatedOrderList
                                        = String.Join(", ", detail.ZilaTripOrderList.Where(x => x.IsActive == true).Select(x => x.OrderId).Distinct());
                                }
                                else
                                {
                                    detail.CommaSeparatedOrderList = "";
                                }
                            }

                            zilaTripMaster.CustomerCount
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0 && x.IsActive == true && x.IsDeleted == false).Count();
                            zilaTripMaster.OrderCount
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0 && x.IsActive == true && x.IsDeleted == false).Sum(x => x.OrderCount);
                            zilaTripMaster.TotalAmount
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0 && x.IsActive == true && x.IsDeleted == false).Sum(x => x.TotalAmount);
                            zilaTripMaster.TotalDistanceInMeter
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalDistanceInMeter);
                            zilaTripMaster.TotalTimeInMins
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalTimeInMins);
                            zilaTripMaster.TotalWeight
                                = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0).Sum(x => x.TotalWeight);
                            zilaTripMaster.ModifiedBy = userId;
                            zilaTripMaster.ModifiedDate = DateTime.Now;


                            var query = from d in authContext.DeliveryIssuanceDb
                                        join o in authContext.OrderDispatchedMasters
                                            on d.DeliveryIssuanceId equals o.DeliveryIssuanceIdOrderDeliveryMaster
                                        where o.OrderId == orderId
                                        select d;

                            var deliveryIssuance = query.FirstOrDefault();
                            deliveryIssuance.ZilaTripMasterId = ZilaTripMasterId;

                            authContext.TripPickerAssignmentMapping.Add(new Model.TripPlanner.TripPickerAssignmentMapping
                            {
                                AssignmentId = deliveryIssuance.DeliveryIssuanceId,
                                IsActive = true,
                                CreatedBy = userId,
                                CreatedDate = DateTime.Now,
                                IsDeleted = false,
                                ZilaTripMasterId = ZilaTripMasterId
                            });

                            authContext.Commit();
                            scope.Complete();
                            return new APIResponse { Status = true }; //, Data = PendingSKcodess
                        }
                        else
                        {
                            scope.Dispose();
                            return new APIResponse { Status = false, Message = "Order already added into a trip!", Data = new List<string>() };
                        }

                    }
                }
            }
            return new APIResponse { Status = false, Message = "Data Not Saved", Data = new List<string>() };
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("RemoveOrder")]
        public APIResponse RemoveOrder([FromUri] long ZilaTripMasterId, [FromUri] int orderId, [FromUri] int dboyId)
        {
            var userId = dboyId;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var authContext = new AuthContext())
                {
                    ZilaTripMaster zilaTripMaster = authContext.ZilaTripMasters
                                   .Include("ZilaTripDetailList.ZilaTripOrderList")
                                   .Where(x => x.Id == ZilaTripMasterId)
                                   .FirstOrDefault();

                    var orderMaster = authContext.DbOrderMaster.FirstOrDefault(x => x.OrderId == orderId);

                    if (zilaTripMaster != null && orderMaster != null)
                    {
                        var detail1 = zilaTripMaster.ZilaTripDetailList.FirstOrDefault(x => x.CustomerId == orderMaster.CustomerId);
                        if (detail1 != null)
                        {
                            var ord = detail1.ZilaTripOrderList.FirstOrDefault(x => x.OrderId == orderId && x.IsActive == true && x.IsDeleted == false);
                            if (ord != null)
                            {
                                ord.IsActive = false;
                                ord.IsDeleted = true;
                                if (!detail1.ZilaTripOrderList.Any(x => x.IsActive == true && x.IsDeleted == false))
                                {
                                    detail1.IsActive = false;
                                    detail1.IsDeleted = true;
                                    detail1.OrderCount = 0;
                                    detail1.TotalAmount = 0;
                                }
                                else
                                {
                                    detail1.OrderCount = detail1.ZilaTripOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Count();
                                    detail1.TotalAmount = detail1.ZilaTripOrderList.Where(x => x.IsActive == true && x.IsDeleted == false).Sum(y => y.Amount);
                                }
                            }
                            else
                            {
                                return new APIResponse { Status = false, Message = "Order Not found" };
                            }
                        }
                        else
                        {
                            return new APIResponse { Status = false, Message = "Customer Not found" };

                        }

                        int seq = 1;
                        foreach (var detail in zilaTripMaster.ZilaTripDetailList.OrderBy(x => x.SequenceNo).ToList())
                        {
                            detail.SequenceNo = seq++;
                            if (detail.ZilaTripOrderList.Where(x => x.IsActive == true).Count() > 0)
                            {
                                detail.CommaSeparatedOrderList
                                    = String.Join(", ", detail.ZilaTripOrderList.Where(x => x.IsActive == true).Select(x => x.OrderId).Distinct());
                            }
                            else
                            {
                                detail.CommaSeparatedOrderList = "";
                            }
                        }

                        zilaTripMaster.CustomerCount
                            = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0 && x.IsActive == true && x.IsDeleted == false).Count();
                        zilaTripMaster.OrderCount
                            = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0 && x.IsActive == true && x.IsDeleted == false).Sum(x => x.OrderCount);
                        zilaTripMaster.TotalAmount
                            = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0 && x.IsActive == true && x.IsDeleted == false).Sum(x => x.TotalAmount);
                        zilaTripMaster.TotalDistanceInMeter
                            = zilaTripMaster.ZilaTripDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalDistanceInMeter);
                        zilaTripMaster.TotalTimeInMins
                            = zilaTripMaster.ZilaTripDetailList.Where(x => x.CustomerId == 0 || x.OrderCount > 0).Sum(x => x.TotalTimeInMins);
                        zilaTripMaster.TotalWeight
                            = zilaTripMaster.ZilaTripDetailList.Where(x => x.OrderCount > 0).Sum(x => x.TotalWeight);
                        zilaTripMaster.ModifiedBy = userId;
                        zilaTripMaster.ModifiedDate = DateTime.Now;


                        var query = from d in authContext.DeliveryIssuanceDb
                                    join o in authContext.OrderDispatchedMasters
                                        on d.DeliveryIssuanceId equals o.DeliveryIssuanceIdOrderDeliveryMaster
                                    where o.OrderId == orderId
                                    select d;

                        var deliveryIssuance = query.FirstOrDefault();
                        deliveryIssuance.ZilaTripMasterId = null;
                        authContext.Entry(deliveryIssuance).State = EntityState.Modified;
                        var mapping = authContext.TripPickerAssignmentMapping.Where(x => x.ZilaTripMasterId == ZilaTripMasterId && x.AssignmentId == deliveryIssuance.DeliveryIssuanceId && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();
                        if (mapping != null)
                        {
                            mapping.IsActive = false;
                            mapping.IsDeleted = true;
                            mapping.ModifiedBy = userId;
                            mapping.ModifiedDate = DateTime.Now;
                        }

                        authContext.Commit();
                        scope.Complete();
                        return new APIResponse { Status = true };
                    }
                    else
                    {
                        return new APIResponse { Status = false, Message = "Trip Not found" };

                    }
                }
            }
        }

        [AllowAnonymous]
        [Route("ZilaDeliveryDashboard")]
        [HttpGet]
        public HttpResponseMessage DeliveryDashboard(int PeopleId, long ZilaTripMasterId)
        {
            int ApproveNotifyTimeLeftInMinute = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);
            bool IsReturnOnly = false;
            ZResponceDc res;
            bool isSKFixVehicle = false;
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            ZilaTripPlannerHelper zilaTripPlannerHelper = new ZilaTripPlannerHelper();
            List<ZilaAssignmentList> ListAssignment = new List<ZilaAssignmentList>();
            using (var context = new AuthContext())
            {

                var zilaTripMaster = zilaTripPlannerHelper.GetCurrentTripPlannerConfirmedMaster(ZilaTripMasterId, context);
                if (zilaTripMaster != null)
                {
                    var GetTripPlannerConfirmedDetailId = zilaTripPlannerHelper.GetTripPlannerConfirmedDetailId(zilaTripMaster.Id, context);
                    var tripPlannerVehicle = context.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == zilaTripMaster.Id).FirstOrDefault();
                    var deliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.ZilaTripMasterId == zilaTripMaster.Id && x.AssignmentType == 5).ToList();
                    double distanceLeft = ((double)tripPlannerVehicle.DistanceLeft) / 1000.00;
                    double distanceTraveled = ((double)tripPlannerVehicle.ActualDistanceTraveled) / 1000.00;
                    ZilaTripPlannerAppDashboardDC responceDc = new ZilaTripPlannerAppDashboardDC()
                    {
                        ZilaTripMasterId = zilaTripMaster.Id,
                        CurrentStatus = tripPlannerVehicle.CurrentStatus,
                        TripPlannerVehicleId = tripPlannerVehicle.Id,
                        TripPlannerConfirmedDetailId = GetTripPlannerConfirmedDetailId.Id,
                        BreakTimeInSec = tripPlannerVehicle.BreakTimeInSec,
                        StartKm = tripPlannerVehicle.StartKm ?? 0,
                        CustomerTripStatus = GetTripPlannerConfirmedDetailId.CustomerTripStatus,
                        //CustomerId= GetTripPlannerConfirmedDetailId.CustomerId,
                        tripPlannerDistance = new ZilaTripPlannerDistance()
                        {
                            EndTime = tripPlannerVehicle.EndTime,
                            DistanceLeft = Math.Round(distanceLeft, 2),
                            DistanceTraveled = Math.Round(distanceTraveled, 2),
                            ReminingTime = tripPlannerVehicle.ReminingTime,
                            StartTime = tripPlannerVehicle.StartTime,
                            TravelTime = tripPlannerVehicle.TravelTime,
                            TotalTime = zilaTripMaster.TotalTimeInMins
                        },
                        myTrip = new ZilaMyTrip()
                        {
                            TripId = zilaTripMaster.Id,
                            TotalAmount = zilaTripMaster.TotalAmount,
                            TotalOrder = zilaTripMaster.OrderCount,
                            CustomerCount = zilaTripMaster.CustomerCount
                        }
                    };
                    foreach (var y in deliveryIssuance)
                    {
                        ListAssignment.Add(new ZilaAssignmentList
                        {
                            AssignmentId = y.DeliveryIssuanceId,
                            Amount = y.TotalAssignmentAmount,
                            NoOfOrder = y.OrderIds.Split(',').Length,
                            AssignmentStatus = y.Status,
                            CreateDate = y.CreatedDate
                        });
                    }
                    responceDc.assignmentList = ListAssignment;
                    if (responceDc != null)
                    {
                        var peopleId = new SqlParameter("@PeopleId", PeopleId);
                        var zilaTripMasterids = new SqlParameter("@ZilaTripMasterId", zilaTripMaster.Id);
                        var data = context.Database.SqlQuery<ZilaPendingOrderlist>("EXEC [Zila].[Zila_getDeliveryDashboardOrdercount] @PeopleId,@ZilaTripMasterId", peopleId, zilaTripMasterids).FirstOrDefault();
                        responceDc.OrderStatuslist = data;

                        var DeliveryIssuanceId = deliveryIssuance.Select(x => x.DeliveryIssuanceId).ToList();
                        string query = " SELECT CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT)FROM OrderDeliveryMasters  odm " +
                                    " join DeliveryIssuances di on odm.DeliveryIssuanceId = di.DeliveryIssuanceId where  di.DeliveryIssuanceId  in ('" + string.Join("','", DeliveryIssuanceId) + "') and odm.Status in ('Shipped', 'Issued','Delivery Canceled Request') and di.Status !='Rejected' ";
                        bool IsPending = context.Database.SqlQuery<bool>(query).First();

                        var zilailaTripMasterIdParam = new SqlParameter("@ZilaTripMasterId", zilaTripMaster.Id);
                        var IsEnd = context.Database.SqlQuery<bool>("EXEC [Zila].[Zila_GetCheckTripEndStatus] @ZilaTripMasterId", zilailaTripMasterIdParam).First();

                        var VehicleMasterId = new SqlParameter("@VehicleMasterId", zilaTripMaster.VehicleMasterId);
                        isSKFixVehicle = context.Database.SqlQuery<bool>("EXEC Operation.TripPlanner_GetCheckSKFIXTripVehicle @VehicleMasterId", VehicleMasterId).First();
                        // responceDc.IsSKFixVehicle = isSKFixVehicle;
                        responceDc.OrderStatuslist = data;
                        if (!IsPending && !IsEnd)
                        {
                            responceDc.IsTripEnd = true;
                        }
                    }
                    responceDc.ApproveNotifyTimeLeftInMinute = ApproveNotifyTimeLeftInMinute;
                    //responceDc.IsNotLastMileTrip = tripPlannerConfirmedMaster.IsNotLastMileTrip;
                    // responceDc.IsLocationEnabled = zilaTripMaster.IsLocationEnabled;
                    zilaTripPlannerHelper.SetTripWorkingStatus(responceDc, tripPlannerVehicle);
                    res = new ZResponceDc()
                    {
                        TripDashboardDC = responceDc,
                        Status = true,
                        Message = "Data Get Successfully!!"
                    };

                    //AppVisits
                    var CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
                    AppVisits appVisit = new AppVisits();
                    MongoDbHelper<AppVisits> mongoAppVisitDbHelper = new MongoDbHelper<AppVisits>();
                    string mobileNoQyery = "select distinct p.Mobile from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + PeopleId + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                    var mobileNo = context.Database.SqlQuery<string>(mobileNoQyery).FirstOrDefault();
                    appVisit.UserName = mobileNo;
                    appVisit.UserId = PeopleId;
                    appVisit.AppType = "New Delivery App";
                    appVisit.VisitedOn = DateTime.Now;
                    var Status = mongoAppVisitDbHelper.InsertAsync(appVisit);

                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    res = new ZResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = false,
                        Message = "No Trip Available!!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }

        [Route("GetZilaTrip")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ZilaTripBriefDc> GetZilaTrip(long zilaTripMasterId)
        {
            using (var db = new AuthContext())
            {
                ZilaTripPlannerHelper helper = new ZilaTripPlannerHelper();
                var result = db.ZilaTripMasters.Where(x => x.Id == zilaTripMasterId).Select(x => new ZilaTripBriefDc
                {
                    IsFinalized = x.IsFreezed,
                    //CustomerList = new List<ZilaCustomerBriefDc>(),
                    ZilaTripMasterId = x.Id
                }).FirstOrDefault();

                result.CustomerList = helper.GetZilaCustomerInfoBrief(zilaTripMasterId, db);

                return result;
            }
        }

        [Route("ZilaReachedDestinationTrip/{ZilaTripDetailId}")]
        [HttpGet]
        public bool ReachedDestinationTrip(long ZilaTripDetailId)
        {
            using (var context = new AuthContext())
            {
                var zilaTripDetail = context.ZilaTripDetails.FirstOrDefault(x => x.Id == ZilaTripDetailId);
                if (zilaTripDetail != null)
                {
                    zilaTripDetail.IsDestinationReached = true;
                    context.Entry(zilaTripDetail).State = EntityState.Modified;
                    context.Commit();
                    return true;
                }
                return false;
            }
        }

        [Route("ZilaUnlodingItem")]
        [HttpPost]
        public ResponceDc UnlodingItem(ZilaItemuUnloadingDC itemuUnloadingDC)
        {
            ResponceDc res;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                var tripPlannerItemCheckListdc = new List<TripPlannerItemCheckListDC>();
                using (var context = new AuthContext())
                {
                    int userId = GetLoginUserId();
                    var zilaTripDetails = context.ZilaTripDetails.Where(x => x.Id == itemuUnloadingDC.zilaTripDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var zilatripPlannerVehicle = context.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == zilaTripDetails.ZilaTripMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    context.Database.CommandTimeout = 300;
                    var orderDetails = context.OrderDispatchedDetailss.Where(x => itemuUnloadingDC.OrderId.Contains(x.OrderId) && x.qty > 0 && x.Deleted == false).Select(x => new { x.OrderId, x.itemname, x.qty, x.ItemMultiMRPId }).ToList();
                    var zilaTripOrders = context.ZilaTripOrders.Where(x => x.ZilaTripDetailId == zilaTripDetails.Id && x.IsActive == true && x.IsDeleted == false).ToList();
                    var zilatripOrderIds = zilaTripOrders.Select(x => x.Id).ToList();
                    if (zilatripPlannerVehicle != null && zilaTripDetails != null && orderDetails != null && orderDetails.Any())
                    {
                        zilatripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.Delivering;
                        zilatripPlannerVehicle.ModifiedDate = indianTime;
                        zilatripPlannerVehicle.ModifiedBy = userId;
                        context.Entry(zilatripPlannerVehicle).State = EntityState.Modified;

                        var orders = zilaTripOrders.Where(x => (x.WorkingStatus == Convert.ToInt32(WorKingStatus.Unloading) || x.WorkingStatus == Convert.ToInt32(WorKingStatus.CollectingPayment))).ToList();
                        if (orders != null && orders.Any())
                        {
                            orders.ForEach(y =>
                            {
                                y.WorkingStatus = Convert.ToInt32(WorKingStatus.Pending);
                                y.ModifiedBy = userId;
                                y.ModifiedDate = DateTime.Now;
                                context.Entry(y).State = EntityState.Modified;
                            });
                        }
                        //setp 2 delete all check list item for given tripPlannerConfirmedDetailId those is done is not true 
                        var TtripPlannerItemCheckList = context.TripPlannerItemCheckListDb.Where(x => zilatripOrderIds.Contains(x.ZilaTripOrderId.Value) && x.IsActive == true && x.IsDeleted == false && x.IsDone == false).ToList();
                        if (TtripPlannerItemCheckList != null && TtripPlannerItemCheckList.Any())
                        {
                            TtripPlannerItemCheckList.ForEach(x =>
                            {
                                x.IsActive = false;
                                x.IsDeleted = true;
                                x.ModifiedBy = userId;
                                x.ModifiedDate = DateTime.Now;
                                context.Entry(x).State = EntityState.Modified;
                            });
                        }
                        //step 3. set is unloading workingstatus in TripPlannerConfirmedOrders for given orderid
                        var selectorders = zilaTripOrders.Where(x => itemuUnloadingDC.OrderId.Contains((int)x.OrderId)).ToList();
                        if (selectorders != null && selectorders.Any())
                        {
                            selectorders.ForEach(y =>
                            {
                                y.WorkingStatus = Convert.ToInt32(WorKingStatus.Unloading);
                                context.Entry(y).State = EntityState.Modified;
                            });
                        }
                        // step 4.insert item in check list table for given orderid
                        var OrderItemCheckList = orderDetails.GroupBy(x => new { x.ItemMultiMRPId, x.OrderId }).Select(x => new TripPlannerItemCheckListDC
                        {
                            OrderId = x.Select(w => w.OrderId).FirstOrDefault(),
                            ItemMultiMRPId = x.Key.ItemMultiMRPId,
                            Qty = x.Sum(y => y.qty),
                            Itemname = x.Select(w => w.itemname).FirstOrDefault()
                        }).ToList();

                        // Step 5: Set CustomerTripStatus to Unloading
                        // zilaTripDetails.IsVisible = true;
                        zilaTripDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.unloading);
                        if (zilaTripDetails.ServeStartTime == null)
                        {
                            zilaTripDetails.ServeStartTime = DateTime.Now;
                        }
                        context.Entry(zilaTripDetails).State = EntityState.Modified;

                        OrderItemCheckList.ForEach(x =>
                        {
                            TripPlannerItemCheckList tripPlannerItemCheckList1 = new TripPlannerItemCheckList();
                            tripPlannerItemCheckList1.OrderId = x.OrderId;
                            tripPlannerItemCheckList1.Itemname = x.Itemname;
                            tripPlannerItemCheckList1.ItemMultiMRPId = x.ItemMultiMRPId;
                            tripPlannerItemCheckList1.Qty = x.Qty;
                            tripPlannerItemCheckList1.IsUnloaded = false;
                            tripPlannerItemCheckList1.IsDone = false;
                            tripPlannerItemCheckList1.IsActive = true;
                            tripPlannerItemCheckList1.IsDeleted = false;
                            tripPlannerItemCheckList1.CreatedDate = DateTime.Now;
                            tripPlannerItemCheckList1.ZilaTripOrderId = zilaTripOrders.FirstOrDefault(y => y.OrderId == x.OrderId).Id;
                            context.TripPlannerItemCheckListDb.Add(tripPlannerItemCheckList1);
                        });

                        if (context.Commit() > 0)
                        {
                            scope.Complete();
                            res = new ResponceDc()
                            {
                                TripDashboardDC = null,
                                Status = true,
                                Message = "Order Unloding Item Started!!"
                            };
                        }
                        else
                        {
                            scope.Dispose();
                            res = new ResponceDc()
                            {
                                TripDashboardDC = null,
                                Status = false,
                                Message = "Order Unloding Item not Started!!"
                            };
                        }
                    }
                    else
                    {
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = false,
                            Message = "Order Unloding Item not Started!!"
                        };
                        scope.Dispose();
                    }
                }
            }
            return res;
        }

        [Route("ZilaGetUnloadItemListPage")]
        [HttpGet]
        public async Task<ZilaGetUnloadItemListPageDC> ZilaGetUnloadItemListPage(long ZilaTripDetailId)
        {
            ZilaGetUnloadItemListPageDC result = new ZilaGetUnloadItemListPageDC();
            ZilaDeliveryAppManager zilaDeliveryAppManager = new ZilaDeliveryAppManager();
            result = await zilaDeliveryAppManager.ZilaGetUnloadItemListPage(ZilaTripDetailId);
            return result;
        }

        [Route("ZilaCheckAndUnCheckedUnloadingItem")]
        [AllowAnonymous]
        [HttpPost]
        public ResponceDc ZilaCheckAndUnCheckedUnloadingItem(ZilaCheckUncheckDc checkUncheck)
        {
            ResponceDc res = null;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    int userId = GetLoginUserId();
                    var zilatripPlanner = context.ZilaTripOrders.Where(x => x.IsActive == true && x.IsDeleted == false && x.ZilaTripDetailId == checkUncheck.ZilaTripDetailId).Select(x => x.Id).ToList();
                    var tripPlannerItemCheckList = context.TripPlannerItemCheckListDb.Where(x => zilatripPlanner.Contains(x.TripPlannerConfirmedOrderId) && checkUncheck.ItemMultiMRPId.Contains(x.ItemMultiMRPId) && x.IsActive == true && x.IsDeleted == false && x.IsDone == false).ToList();

                    if (tripPlannerItemCheckList != null && tripPlannerItemCheckList.Any())
                    {
                        foreach (var item in tripPlannerItemCheckList)
                        {
                            if (item != null && checkUncheck.IsUnloaded)
                            {
                                item.IsUnloaded = checkUncheck.IsUnloaded;
                                item.ModifiedDate = indianTime;
                                item.ModifiedBy = userId;
                                context.Entry(item).State = EntityState.Modified;
                                //context.Commit();
                                //ssscope.Complete();
                                res = new ResponceDc()
                                {
                                    TripDashboardDC = null,
                                    Status = true,
                                    Message = "Unloding Item Successfully!!"
                                };
                            }
                            else
                            {
                                item.IsUnloaded = checkUncheck.IsUnloaded;
                                item.ModifiedDate = indianTime;
                                item.ModifiedBy = userId;
                                context.Entry(item).State = EntityState.Modified;
                                //context.Commit();
                                //scope.Complete();
                                res = new ResponceDc()
                                {
                                    TripDashboardDC = null,
                                    Status = true,
                                    Message = "Unloding Item Successfully!!"
                                };
                            }
                        }
                        if (context.Commit() > 0)
                        {
                            scope.Complete();
                        }
                    }
                    else
                    {
                        scope.Dispose();
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = false,
                            Message = "Unloding Item not Successfully!!"
                        };
                    }
                }
            }
            return res;
        }

        [Route("ZilaStartAssignment")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage ZilaStartAssignment(ZilaAssignmentStartEndDc assignmentStartEndDc)
        {
            ZilaTripManager zilaTripManager = new ZilaTripManager();
            var res = zilaTripManager.ZilaStartAssignment(assignmentStartEndDc, assignmentStartEndDc.PeopleID);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [Route("ZilaMytripOrderList")]
        [HttpGet]
        [AllowAnonymous]
        public List<ZilaMytripOrderCustomerWiseDc> ZilaMytripOrderList(long ZilaTripMasterId)
        {
            ZilaMytripOrderCustomerWiseDc list = new ZilaMytripOrderCustomerWiseDc();
            List<ZilamytripOrderListDc> Orderlist = new List<ZilamytripOrderListDc>();
            using (var context = new AuthContext())
            {
                var zilaTripMasterId = new SqlParameter("@ZilaTripMasterId", ZilaTripMasterId);
                Orderlist = context.Database.SqlQuery<ZilamytripOrderListDc>(" EXEC [Zila].[Zila_GetMyTripOrderList]  @ZilaTripMasterId", zilaTripMasterId).ToList();

                bool isAnyTripRunning = Orderlist.Max(x => x.IsAnyTripRunning);
                var data = Orderlist.GroupBy(x => new
                {
                    x.SequenceNo,
                    x.Skcode,
                    x.ShopName,
                    x.lat,
                    x.lg,
                    x.WarehouseAddress,
                    x.CustomerMobile,
                    x.ZilaTripDetailId,
                    x.CustomerId
                }).Select(x => new ZilaMytripOrderCustomerWiseDc
                {
                    ZilaTripDetailId = x.Key.ZilaTripDetailId,
                    CustomerId = x.Key.CustomerId,
                    SequenceNo = x.Key.SequenceNo,
                    Skcode = x.Key.Skcode,
                    lat = x.Key.lat,
                    lg = x.Key.lg,
                    ShopName = x.Key.ShopName,
                    TotalOrderAmount = x.Sum(z => z.GrossAmount),
                    NoOfItems = x.Sum(z => z.NoOfItems),
                    WarehouseAddress = x.Key.WarehouseAddress,
                    CustomerAddress = x.Max(z => z.CustomerAddress),
                    DeliveryIssuanceId = x.Max(z => z.DeliveryIssuanceId),
                    CustomerMobile = x.Key.CustomerMobile,
                    WorkingStatus = x.Max(z => z.WorkingStatus),
                    Reason = x.Max(z => z.Reason),
                    IsOTPSent = x.Max(z => z.IsOTPSent),
                    OTPSentRemaningTimeInSec = x.Max(z => z.OTPSentRemaningTimeInSec),
                    IsReAttemptShow = x.Max(z => z.ReAttemptCount >= 1) ? true : false,
                    IsReDispatchShow = x.Max(z => z.ReDispatchCount >= 1) ? true : false,
                    RecordingUrl = x.Max(z => z.RecordingUrl),
                    //IsVisible = x.Max(z => z.IsVisible),
                    //IsSkip = x.Max(z => z.IsSkip),
                    // IsNotLastMileTrip = x.Max(z => z.IsNotLastMileTrip),
                    IsAnyTripRunning = isAnyTripRunning,
                    IsProcess = x.Max(z => z.IsProcess),
                    CustomerTripStatus = x.Max(z => z.CustomerTripStatus),
                    //IsLocationEnabled = x.Max(z => z.IsLocationEnabled),
                    //TripTypeEnum = x.Max(z => z.TripTypeEnum),
                    IsReturnOrder = Orderlist.FirstOrDefault(y => y.OrderType == "ReturnOrder" && y.Status == "Shipped" && y.CustomerId == x.Key.CustomerId) != null ? true : false,
                    IsGeneralOrder = Orderlist.FirstOrDefault(y => y.OrderType != "ReturnOrder" && y.CustomerId == x.Key.CustomerId) != null ? true : false,
                    ZorderList = x.Select(a => new ZilaOrderList()
                    {
                        OrderId = a.OrderId,
                        Status = a.Status,
                        Amount = a.GrossAmount,
                        TotalTimeInMins = a.TotalTimeInMins,
                        TimeInMins = a.TimeInMins,
                        DeliveryIssuanceId = a.DeliveryIssuanceId,
                        NoOfItems = a.NoOfItems,
                        ReDispatchCount = a.ReDispatchCount,
                        ReAttemptCount = a.ReAttemptCount,
                        OrderType = a.OrderType
                    }).ToList(),
                }).ToList();
                foreach (var item in data)
                {
                    item.OrderCompletionTime = 0;
                    item.UnloadingTime = 0;
                    foreach (var a in item.ZorderList)
                    {
                        var totalTimeInMins = (a.TotalTimeInMins - a.TimeInMins);
                        if (item.OrderCompletionTime == 0)
                        {
                            item.OrderCompletionTime = totalTimeInMins;
                            item.UnloadingTime += a.TimeInMins;
                        }
                        else
                        {
                            item.OrderCompletionTime -= a.TimeInMins;
                            item.UnloadingTime += a.TimeInMins;
                        }
                    }
                }
                if (data != null && data.Any())
                {
                    data = data.OrderBy(x => x.SequenceNo).ToList();
                    data = data.OrderBy(x => x.IsProcess).ToList();
                    bool islocation = data.FirstOrDefault(x => x.CustomerId > 0).IsLocationEnabled == true ? true : false;
                    var warehouse = context.ZilaTripDetails
                       .Where(x => x.ZilaTripMasterId == ZilaTripMasterId && x.OrderCount == 0 && x.IsActive == true && x.IsDeleted == false)
                       .Select(y => new ZilaMytripOrderCustomerWiseDc
                       {
                           CustomerAddress = null,
                           CustomerId = 0,
                           CustomerMobile = null,
                           DeliveryIssuanceId = 0,
                           NoOfItems = 0,
                           OrderCompletionTime = y.TotalTimeInMins,
                           //orderList = null,
                           SequenceNo = y.SequenceNo,
                           ShopName = null,
                           Skcode = null,
                           TotalOrderAmount = 0,
                           ZilaTripDetailId = y.Id,
                           UnloadingTime = 0,
                           WarehouseAddress = null,
                           IsVisible = false,
                           IsSkip = false,
                           IsNotLastMileTrip = false,
                           IsAnyTripRunning = false,
                           IsLocationEnabled = islocation
                       }).FirstOrDefault();
                    data.Add(warehouse);
                }

                if (data != null && data.Any())
                {
                    var SkcodeList = data.Select(x => x.Skcode).Distinct().ToList();
                    CRMManager cRMManager = new CRMManager();
                    var taglist = cRMManager.GetCRMCustomerWithTag(SkcodeList, CRMPlatformConstants.LastMileApp).WaitAndUnwrapException();

                    foreach (var item in data)
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
                return data;
            }
        }

        [Route("ZilaCheckTripOrderCurrentStatus")]
        [HttpGet]
        public async Task<ZilaCheckTripOrderCurrentStatusDC> ZilaCheckTripOrderCurrentStatus(long ZilaTripMasterId)
        {
            {
                using (var context = new AuthContext())
                {
                    var zilaTripMasterIdMasterId = new SqlParameter("@ZilaTripMasterId", ZilaTripMasterId);
                    var list = await context.Database.SqlQuery<ZilaCheckTripOrderCurrentStatusDC>("[Zila].[Zila_CheckTripOrderCurrentStatus] @ZilaTripMasterId", zilaTripMasterIdMasterId).FirstOrDefaultAsync();
                    if (list == null)
                    {
                        var zilaTripMasterIdMasterIds = new SqlParameter("@ZilaTripMasterId", ZilaTripMasterId);
                        list = await context.Database.SqlQuery<ZilaCheckTripOrderCurrentStatusDC>("[Zila].[Zila_CheckTripWarehouseCurrentStatus] @ZilaTripMasterId", zilaTripMasterIdMasterIds).FirstOrDefaultAsync();
                    }
                    return list != null ? list : new ZilaCheckTripOrderCurrentStatusDC();
                }
            }
        }


        [Route("ZilaOrderDetail")]
        [HttpGet]
        public HttpResponseMessage getDeliveryAppOrders(int OrderId) //get orders for delivery
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var DBoyorders = (from a in db.OrderDispatchedMasters
                                      where (a.Status == "Shipped") && a.OrderId == OrderId
                                      join i in db.Customers on a.CustomerId equals i.CustomerId
                                      select new OrderDispatchedMasterDTO
                                      {
                                          lat = i.lat,
                                          lg = i.lg,
                                          ClusterId = a.ClusterId,
                                          ClusterName = a.ClusterName,
                                          active = a.active,
                                          BillingAddress = a.BillingAddress,
                                          CityId = a.CityId,
                                          comments = a.comments,
                                          CompanyId = a.CompanyId,
                                          CreatedDate = a.CreatedDate,
                                          CustomerId = a.CustomerId,
                                          CustomerName = a.CustomerName,
                                          ShopName = a.ShopName,
                                          Skcode = a.Skcode,
                                          Customerphonenum = a.Customerphonenum,
                                          DboyMobileNo = a.DboyMobileNo,
                                          DboyName = a.DboyName,
                                          Deleted = a.Deleted,
                                          Deliverydate = a.Deliverydate,
                                          DiscountAmount = a.DiscountAmount,
                                          DivisionId = a.DivisionId,
                                          GrossAmount = a.GrossAmount,
                                          invoice_no = a.invoice_no,
                                          orderDetails = a.orderDetails,
                                          OrderDispatchedMasterId = a.OrderDispatchedMasterId,
                                          OrderId = a.OrderId,
                                          RecivedAmount = a.RecivedAmount,
                                          ReDispatchCount = a.ReDispatchCount,
                                          //SalesPerson = a.SalesPerson,
                                          //SalesPersonId = a.SalesPersonId,
                                          ShippingAddress = a.ShippingAddress,
                                          Status = a.Status,
                                          TaxAmount = a.TaxAmount,
                                          TotalAmount = a.TotalAmount,
                                          UpdatedDate = a.UpdatedDate,
                                          WarehouseId = a.WarehouseId,
                                          WarehouseName = a.WarehouseName,
                                          DeliveryIssuanceId = a.DeliveryIssuanceIdOrderDeliveryMaster
                                      }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [AllowAnonymous]
        [Route("ZilaSingleOrderMapviewInfo")]
        [HttpGet]
        public async Task<ZilaSingleOrderMapview> SingleOrderMapviewInfo(long TripPlannerConfirmedMasterId, double lat, double lng)
        {
            ZilaSingleOrderMapview result = new ZilaSingleOrderMapview();
            ZilaDeliveryAppManager tripPlannerManager = new ZilaDeliveryAppManager();
            result = await tripPlannerManager.SingleOrderMapviewInfo(TripPlannerConfirmedMasterId, lat, lng);
            return result;
        }


        [Route("GetCustomerWiseOrderList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ZilaGetCustomerWiseOrderListDc> GetCustomerWiseOrderList(long TripPlannerConfirmedDetailId, bool ReturnOrder = false)
        {
            using (var db = new AuthContext())
            {
                var orderReDispatchCount = db.CompanyDetailsDB.Where(x => x.IsActive == true).Select(x => x.OrderReDispatchCount).FirstOrDefault();
                ZilaGetCustomerWiseOrderListDc result = new ZilaGetCustomerWiseOrderListDc();
                ZilaDeliveryAppManager tripPlannerManager = new ZilaDeliveryAppManager();
                result = await tripPlannerManager.GetCustomerWiseOrderList(TripPlannerConfirmedDetailId, orderReDispatchCount, ReturnOrder);

                if (result.customerOrderInfo.Any(x => x.WarehouseId != 2))
                {
                    #region for refund  IsDeliveryCancelledEnable
                    var OrderIdList = result.customerOrderInfo.Where(x => x.OnilnePayment == true).Select(x => x.OrderId).Distinct().ToList();
                    if (OrderIdList != null && OrderIdList.Any())
                    {
                        var PaymentResponseRetailerApps = db.PaymentResponseRetailerAppDb.Where(x => OrderIdList.Contains(x.OrderId) && x.status == "Success").ToList();
                        var PaymentResponseRetailerAppList = new List<RetailerOrderPaymentDc>();
                        foreach (var item in PaymentResponseRetailerApps.GroupBy(x => new { x.OrderId, x.PaymentFrom, x.GatewayTransId, x.IsOnline, x.status }))
                        {
                            PaymentResponseRetailerAppList.Add(new RetailerOrderPaymentDc
                            {
                                GatewayTransId = item.FirstOrDefault().GatewayTransId,
                                OrderId = item.FirstOrDefault().OrderId,
                                amount = item.Sum(x => x.amount),
                                status = item.FirstOrDefault().status,
                                PaymentFrom = item.FirstOrDefault().PaymentFrom,
                                ChequeImageUrl = item.FirstOrDefault().ChequeImageUrl,
                                ChequeBankName = item.FirstOrDefault().ChequeBankName,
                                IsOnline = item.FirstOrDefault().IsOnline,
                                TxnDate = item.OrderBy(c => c.CreatedDate).FirstOrDefault().CreatedDate
                            });
                        }
                        int oid = OrderIdList.FirstOrDefault();
                        var od = db.DbOrderMaster.Where(c => c.OrderId == oid).Select(c => new { c.WarehouseId, c.OrderType }).FirstOrDefault();
                        var RefundDays = db.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();
                        if (RefundDays != null && db.Warehouses.Any(x => x.WarehouseId == od.WarehouseId && x.IsOnlineRefundEnabled) /*&& od.OrderType != 8*/)
                        {
                            foreach (var order in result.customerOrderInfo.Where(x => x.OnilnePayment == true))
                            {
                                int check = 0;
                                foreach (var item in PaymentResponseRetailerAppList.Where(x => x.IsOnline == true && x.OrderId == order.OrderId))
                                {
                                    var PaymentRefundDays = RefundDays.FirstOrDefault(x => x.ApiName.Trim().ToLower() == item.PaymentFrom.Trim().ToLower());

                                    if (check == 0 && PaymentRefundDays != null && item.PaymentFrom.Trim().ToLower() != "gullak")
                                    {
                                        if (PaymentRefundDays != null && PaymentRefundDays.DaysForRefundEligible > 0 && item.TxnDate.AddDays(PaymentRefundDays.DaysForRefundEligible) < DateTime.Now)
                                        {
                                            order.IsDeliveryCancelledEnable = false;
                                            check++;
                                        }
                                        else
                                        {
                                            order.IsDeliveryCancelledEnable = true;
                                        }
                                    }
                                    else if (item.PaymentFrom.Trim().ToLower() == "gullak")
                                    {
                                        order.IsDeliveryCancelledEnable = true;

                                    }
                                }
                            }
                        }

                    }
                    #endregion
                }
                if (result != null && result.customerInfo != null)
                {
                    var SkcodeList = result.customerInfo.Skcode;
                    CRMManager cRMManager = new CRMManager();
                    var taglist = cRMManager.GetCRMCustomerWithTag(new List<string> { SkcodeList }, CRMPlatformConstants.LastMileApp).WaitAndUnwrapException();


                    if (taglist != null && taglist.Any())
                    {
                        var tag = taglist.Where(x => x.Skcode == result.customerInfo.Skcode).FirstOrDefault();
                        if (tag != null)
                        {
                            result.customerInfo.CRMTags = tag.CRMTags;
                        }
                    }
                }


                if (result != null && result.customerOrderInfo != null)
                {
                    var orderList = result.customerOrderInfo.Select(x => x.OrderId).ToList();

                    var query = from c in db.PaymentResponseRetailerAppDb
                                join p in db.ScaleUpCustomers on result.customerInfo.CustomerId equals p.CustomerId
                                join o in orderList on c.OrderId equals o
                                where c.status == "Success"
                                    && c.PaymentFrom == "ScaleUp"
                                    && p.IsActive == true
                                    && p.IsDeleted == false
                                    && p.ProductCode == "CreditLine"
                                select new
                                {
                                    OrderId = o,
                                    CustomerId = p.CustomerId,
                                    AccountId = p.AccountId
                                };
                    var customerList = query.ToList();


                }
                return result;
            }
        }

        [Route("ZilaOrderUnloding")]
        [HttpPost]
        public async Task<ZilaResponceDc> ZilaOrderUnloding(ZilaUnlodingDc unlodingDc)
        {
            ZilaResponceDc res;
            using (var context = new AuthContext())
            {
                int userId = GetLoginUserId();
                var tripPlannerVehicle = await context.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == unlodingDc.ZilaTripMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                if (tripPlannerVehicle != null)
                {
                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.Delivering;
                    tripPlannerVehicle.ModifiedDate = indianTime;
                    tripPlannerVehicle.ModifiedBy = userId;
                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                    //TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                    //tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                    //tripPlannerVehicleHistory.CurrentServingOrderId = unlodingDc.CurrentServingOrderId;
                    //tripPlannerVehicleHistory.RecordTime = indianTime;
                    //tripPlannerVehicleHistory.Lat = unlodingDc.lat;
                    //tripPlannerVehicleHistory.Lng = unlodingDc.lng;
                    //tripPlannerVehicleHistory.CreatedDate = indianTime;
                    //tripPlannerVehicleHistory.CreatedBy = userId;
                    //tripPlannerVehicleHistory.IsActive = true;
                    //tripPlannerVehicleHistory.IsDeleted = false;
                    //tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.Delivering;
                    //tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = unlodingDc.ZilaTripDetailId;
                    //context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                    context.Commit();
                    res = new ZilaResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = true,
                        Message = "Order Unloding Started!!"
                    };
                }
                else
                {
                    res = new ZilaResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = false,
                        Message = "Order Unloding not Started!!"
                    };
                }
            }
            return res;
        }

        [Route("ZilaCollectPaymentOrderStatusChange")]
        [HttpGet]
        public ZilaResponceDc ZilaCollectPaymentOrderStatusChange(long ZilaTripDetailId)
        {
            ZilaResponceDc res;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var db = new AuthContext())
                {
                    int userId = GetLoginUserId();
                    int idStatus = Convert.ToInt32(WorKingStatus.Unloading);
                    var zilaTripOrders = db.ZilaTripOrders.Where(x => x.ZilaTripDetailId == ZilaTripDetailId && x.IsActive == true && x.IsDeleted == false && x.WorkingStatus == idStatus).ToList();
                    var orderids = zilaTripOrders.Select(x => x.OrderId).ToList();
                    //var tripPlannerItemCheckList = db.TripPlannerItemCheckListDb.Where(x => orderids.Contains(x.OrderId) && x.IsUnloaded == true && x.IsDone == false).ToList();
                    var zilaTripDetail = db.ZilaTripDetails.Where(x => x.Id == ZilaTripDetailId).FirstOrDefault();
                    if (zilaTripOrders != null && zilaTripOrders.Any())
                    {
                        //tripPlannerItemCheckList.ForEach(x =>
                        //{
                        //    x.IsDone = true;
                        //    x.ModifiedBy = userId;
                        //    x.ModifiedDate = DateTime.Now;
                        //    db.Entry(x).State = EntityState.Modified;
                        //});
                        zilaTripOrders.ForEach(y =>
                        {
                            y.WorkingStatus = Convert.ToInt32(WorKingStatus.CollectingPayment);
                            y.ModifiedBy = userId;
                            y.ModifiedDate = DateTime.Now;
                            db.Entry(y).State = EntityState.Modified;
                        });
                        zilaTripDetail.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.CollectingPayment);
                        db.Entry(zilaTripDetail).State = EntityState.Modified;
                        db.Commit();
                        scope.Complete();
                        res = new ZilaResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = true,
                            Message = "Collect Payment Start!!"
                        };
                    }
                    else
                    {
                        scope.Dispose();
                        res = new ZilaResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = false,
                            Message = "Collect Payment Not Start!!"
                        };
                    }
                }
            }
            return res;
        }

        [Route("ZilaGetCollectPaymentOrderList")]
        [HttpGet]
        public async Task<ZilaGetCollectPaymentOrderListDc> ZilaGetCollectPaymentOrderList(long TripPlannerConfirmedDetailId)
        {
            ZilaGetCollectPaymentOrderListDc result = new ZilaGetCollectPaymentOrderListDc();
            ZilaDeliveryAppManager zilaDeliveryAppManager = new ZilaDeliveryAppManager();
            ZilaDeliveryAppManager tripPlannerManager = new ZilaDeliveryAppManager();
            result = await tripPlannerManager.ZilaGetCollectPaymentOrderList(TripPlannerConfirmedDetailId);
            if (result != null && result.customerInfo != null)
            {
                var SkcodeList = result.customerInfo.Skcode;
                CRMManager cRMManager = new CRMManager();
                var taglist = cRMManager.GetCRMCustomerWithTag(new List<string> { SkcodeList }, CRMPlatformConstants.LastMileApp).WaitAndUnwrapException();

                if (taglist != null && taglist.Any())
                {
                    var tag = taglist.Where(x => x.Skcode == result.customerInfo.Skcode).FirstOrDefault();
                    if (tag != null)
                    {
                        result.customerInfo.CRMTags = tag.CRMTags;
                    }
                }
            }
            return result;
        }

        [Route("ZilaRemoveOrder")]
        [HttpPost]
        public ResponceDc ZilaRemoveOrder(long TripPlannerConfirmOrderId, bool IsPaymentDone)
        {
            bool Status = false;
            ResponceDc res = null;
            var startdate = DateTime.Now.Date.ToString("yyyy-MM-dd 00:00:00");
            var Enddate = DateTime.Now.Date.ToString("yyyy-MM-dd 23:59:59");
            MongoDbHelper<BlockCashAmount> mongoDbHelper = new MongoDbHelper<BlockCashAmount>();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    var tripPlannerConfirmedOrders = context.ZilaTripOrders.FirstOrDefault(x => x.Id == TripPlannerConfirmOrderId);
                    if (tripPlannerConfirmedOrders != null)
                    {
                        var tripPlannerConfirmedDetails = context.ZilaTripDetails.FirstOrDefault(x => x.Id == tripPlannerConfirmedOrders.ZilaTripDetailId);
                        if (!IsPaymentDone)
                        {
                            if (tripPlannerConfirmedDetails != null)
                            {
                                tripPlannerConfirmedOrders.WorkingStatus = (int)WorKingStatus.Pending;
                                int userId = GetLoginUserId();
                                tripPlannerConfirmedOrders.ModifiedBy = userId;
                                tripPlannerConfirmedOrders.ModifiedDate = DateTime.Now;
                                context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                                //tripPlannerConfirmedDetails
                                context.Commit();
                                var TripPlannerConfirmedOrdersList = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == tripPlannerConfirmedDetails.Id && x.IsActive == true && x.IsDeleted == false).ToList();
                                if (TripPlannerConfirmedOrdersList.All(x => x.WorkingStatus == 0))
                                {
                                    tripPlannerConfirmedDetails.CustomerTripStatus = (int)CustomerTripStatusEnum.ReachedDistination;
                                    context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                }
                                var detailList = context.TripPlannerItemCheckListDb
                                    .Where(x => x.Id == TripPlannerConfirmOrderId && x.IsDeleted == false && x.IsActive == true).ToList();
                                foreach (var item in detailList)
                                {
                                    item.IsActive = false;
                                    item.IsUnloaded = false;
                                    item.IsDone = false;
                                    item.IsDeleted = true;
                                    context.Entry(item).State = EntityState.Modified;
                                }
                                if (tripPlannerConfirmedOrders.OrderId > 0)
                                {
                                    var orderamount = context.TripPaymentResponseAppDb.Where(x => x.ZilaTripOrderId == TripPlannerConfirmOrderId && x.Status == "Success" && ((x.PaymentFrom == "Cash" && x.IsOnline == false) || (x.PaymentFrom == "PayLater" && x.IsOnline == true)) && x.IsActive == true && x.IsDeleted == false).ToList().Sum(y => y.Amount);
                                    var cartPredicate1 = PredicateBuilder.New<BlockCashAmount>(x => x.CustomerId == (int)tripPlannerConfirmedDetails.CustomerId && x.Amount > 0 && x.IsActive == true && x.OrderId.Contains(tripPlannerConfirmedOrders.OrderId) && x.CreatedDate >= Convert.ToDateTime(startdate) && x.CreatedDate <= Convert.ToDateTime(Enddate));
                                    var blockCashAmountdb = mongoDbHelper.Select(cartPredicate1).FirstOrDefault();
                                    if (blockCashAmountdb != null && orderamount > 0)
                                    {
                                        var filterOrderIds = blockCashAmountdb.OrderId.Where(x => x != tripPlannerConfirmedOrders.OrderId).Distinct().ToList();
                                        blockCashAmountdb.Amount -= orderamount;
                                        blockCashAmountdb.OrderId = filterOrderIds;
                                        var result = mongoDbHelper.ReplaceAsync(blockCashAmountdb.Id, blockCashAmountdb);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (tripPlannerConfirmedOrders.OrderId > 0)
                            {
                                var orderamount = context.TripPaymentResponseAppDb.Where(x => x.ZilaTripOrderId == TripPlannerConfirmOrderId && x.Status == "Success" && ((x.PaymentFrom == "Cash" && x.IsOnline == false) || (x.PaymentFrom == "PayLater" && x.IsOnline == true)) && x.IsActive == true && x.IsDeleted == false).ToList().Sum(y => y.Amount);
                                var cartPredicate1 = PredicateBuilder.New<BlockCashAmount>(x => x.CustomerId == (int)tripPlannerConfirmedDetails.CustomerId && x.Amount > 0 && x.IsActive == true && x.OrderId.Contains(tripPlannerConfirmedOrders.OrderId) && x.CreatedDate >= Convert.ToDateTime(startdate) && x.CreatedDate <= Convert.ToDateTime(Enddate));
                                var blockCashAmountdb = mongoDbHelper.Select(cartPredicate1).FirstOrDefault();
                                if (blockCashAmountdb != null && orderamount > 0)
                                {
                                    var filterOrderIds = blockCashAmountdb.OrderId.Where(x => x != tripPlannerConfirmedOrders.OrderId).Distinct().ToList();
                                    blockCashAmountdb.Amount -= orderamount;
                                    blockCashAmountdb.OrderId = filterOrderIds;
                                    var result = mongoDbHelper.ReplaceAsync(blockCashAmountdb.Id, blockCashAmountdb);
                                }
                            }
                            var tripPlannerConfirmedOrderId = new SqlParameter("@TripPlannerConfirmedOrderId", TripPlannerConfirmOrderId);
                            var response = context.Database.ExecuteSqlCommand("EXEC Zila.ZilaTripPlanner_BackButtonOrderOTPScreen @TripPlannerConfirmedOrderId", tripPlannerConfirmedOrderId);

                        }
                        context.Commit();
                        Status = true;
                    }
                }
                if (Status)
                {
                    scope.Complete();
                    res = new ResponceDc()
                    {
                        Status = Status,
                        Message = "Remove Order SuccessFully!!",
                        TripDashboardDC = null
                    };
                }
                else
                {
                    scope.Dispose();
                    res = new ResponceDc()
                    {
                        Status = Status,
                        Message = "Someting went Wrong!!",
                        TripDashboardDC = null
                    };
                }
            }
            return res;
        }

        [AllowAnonymous]
        [Route("ZilaSubmitPayment")]
        [HttpPost]
        public ZilaNewResponceDc ZilaSubmitPayment(ZilaCollectPaymentDC collectPaymentDCs)
        {
            ZilaNewResponceDc res = null;
            string GUID = Guid.NewGuid().ToString();
            if (collectPaymentDCs != null && collectPaymentDCs.TripDeliveryPayments != null && collectPaymentDCs.TripDeliveryPayments.Any())
            {
                foreach (var item in collectPaymentDCs.TripDeliveryPayments)
                {
                    item.RemaingAmount = item.amount;
                }
            }
            MongoDbHelper<BlockCashAmount> mongoDbHelper = new MongoDbHelper<BlockCashAmount>();
            BlockCashAmount blockCashAmount = null;
            bool isSuccess = false;
            List<TripPaymentResponseApps> Paymentlist = new List<TripPaymentResponseApps>();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    int userId = GetLoginUserId();
                    var status = Convert.ToInt32(WorKingStatus.CollectingPayment);

                    var tripPlannerConfirmedDetails = context.ZilaTripDetails.Where(x => x.Id == collectPaymentDCs.ZilaTripDetailId).FirstOrDefault();

                    var AlltripPlannerConfirmedOrders = context.ZilaTripOrders.Where(x => x.ZilaTripDetailId == collectPaymentDCs.ZilaTripDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                    var tripPlannerConfirmedOrders = AlltripPlannerConfirmedOrders.Where(x => x.ZilaTripDetailId == collectPaymentDCs.ZilaTripDetailId && collectPaymentDCs.OrderIds.Contains((int)x.OrderId) && x.IsActive == true && x.IsDeleted == false && x.WorkingStatus == status).ToList();
                    var Allorderids = AlltripPlannerConfirmedOrders.Select(x => x.OrderId).Distinct().ToList();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).Distinct().ToList();
                    var AlltripPlannerConfirmedOrderIds = AlltripPlannerConfirmedOrders.Select(x => x.Id).Distinct().ToList();
                    var tripPlannerConfirmedOrderIds = tripPlannerConfirmedOrders.Select(x => x.Id).Distinct().ToList();
                    var AlltripPaymentResponseAppDb = context.TripPaymentResponseAppDb.Where(x => AlltripPlannerConfirmedOrderIds.Contains(x.ZilaTripOrderId.Value) && x.IsActive == true && x.IsDeleted == false).ToList();
                    var tripPaymentResponseAppDb = AlltripPaymentResponseAppDb.Where(x => tripPlannerConfirmedOrderIds.Contains(x.ZilaTripOrderId.Value) && x.IsActive == true && x.IsDeleted == false).ToList();
                    var AllorderDispatchedMaster = context.OrderDispatchedMasters.Where(x => Allorderids.Contains(x.OrderId)).ToList();
                    var orderDispatchedMaster = AllorderDispatchedMaster.Where(x => orderids.Contains(x.OrderId)).ToList();
                    if (collectPaymentDCs.CashAmount > 0)
                    {
                        blockCashAmount = new BlockCashAmount
                        {
                            CreatedDate = DateTime.Now,
                            CustomerId = (int)tripPlannerConfirmedDetails.CustomerId,
                            Guid = GUID,
                            Amount = collectPaymentDCs.CashAmount,
                            IsActive = true,
                            OrderId = collectPaymentDCs.OrderIds.Select(x => (long)x).ToList()
                        };
                        mongoDbHelper.InsertAsync(blockCashAmount);
                    }

                    var PaymentResponseRetailerAppList = context.PaymentResponseRetailerAppDb.Where(x => collectPaymentDCs.OrderIds.Contains(x.OrderId) && x.status == "Success").ToList();
                    var filterOd = AllorderDispatchedMaster.Where(x => !orderids.Contains(x.OrderId)).Select(x => x.OrderId).ToList();
                    var DispatchedOrderids = AllorderDispatchedMaster.Where(x => filterOd.Contains(x.OrderId)).Select(x => x.OrderId).ToList();
                    var oids = DispatchedOrderids.Select(x => (long)x).ToList();
                    var flitertripPlannerConfirmedOrderIds = AlltripPlannerConfirmedOrders.Where(x => oids.Contains(x.OrderId)).Select(x => x.Id).Distinct().ToList();

                    double ExistsCashAmount = AlltripPaymentResponseAppDb.Where(x => flitertripPlannerConfirmedOrderIds.Contains(x.ZilaTripOrderId.Value) && x.PaymentFrom == "Cash" && x.Status == "Success").Sum(x => x.Amount);

                    if (tripPlannerConfirmedDetails.CustomerId > 0 && collectPaymentDCs.CashAmount > 0)
                    {
                        double TcashAmount = 0;
                        if (ExistsCashAmount == 0)
                        {
                            TcashAmount = collectPaymentDCs.CashAmount;
                        }
                        else
                        {
                            TcashAmount = ExistsCashAmount + collectPaymentDCs.CashAmount;

                        }
                        TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                        bool CheckCashAmount = tripPlannerHelper.CheckTodayCustomerCashAmount((int)tripPlannerConfirmedDetails.CustomerId, collectPaymentDCs.CashAmount, blockCashAmount.Guid, context);
                        if (CheckCashAmount)
                        {
                            scope.Dispose();
                            res = new ZilaNewResponceDc()
                            {
                                Status = false,
                                Message = "Alert! You cannot exceed the cash limit of 2 lacs for this customer in a day.",
                                ZilaTripDetailId = 0
                            };
                            if (collectPaymentDCs.CashAmount > 0)
                            {
                                var cartPredicate1 = PredicateBuilder.New<BlockCashAmount>(x => x.CustomerId == (int)tripPlannerConfirmedDetails.CustomerId && x.Guid == blockCashAmount.Guid);
                                var blockCashAmountdb = mongoDbHelper.Select(cartPredicate1).FirstOrDefault();
                                if (blockCashAmountdb != null)
                                {
                                    blockCashAmountdb.IsActive = false;
                                    var result = mongoDbHelper.ReplaceAsync(blockCashAmountdb.Id, blockCashAmountdb);
                                };
                            }
                            return res;
                        }
                    }
                    if (tripPlannerConfirmedOrders != null && tripPlannerConfirmedOrders.Any() && tripPlannerConfirmedOrders != null && tripPlannerConfirmedDetails != null && orderDispatchedMaster != null && orderDispatchedMaster.Any())
                    {
                        foreach (var item in tripPlannerConfirmedOrders)
                        {
                            var ListTripPaymentResponseAppDb = tripPaymentResponseAppDb.Where(x => x.ZilaTripOrderId == item.Id).ToList();
                            if (ListTripPaymentResponseAppDb != null && ListTripPaymentResponseAppDb.Any())
                            {
                                ListTripPaymentResponseAppDb.ForEach(x =>
                                {
                                    x.IsActive = false;
                                    x.IsDeleted = true;
                                    x.ModifiedDate = DateTime.Now;
                                    x.ModifiedBy = userId;
                                    context.Entry(x).State = EntityState.Modified;
                                });
                            }
                            double Ordercashamount = 0;
                            double totalAmount = 0;
                            double totalChequeamt = 0;
                            double totalMposamount = 0;
                            var DispatchedMaster = orderDispatchedMaster.Where(y => y.OrderId == item.OrderId).FirstOrDefault();
                            //double onlineAmount = PaymentResponseRetailerAppList.Where(x => x.OrderId == item.OrderId && x.IsOnline == true).Sum(x => x.amount);
                            #region refund
                            var OnlinePaymentResponseRetailer = new List<RetailerOrderPaymentDc>();
                            foreach (var items in PaymentResponseRetailerAppList.Where(x => x.OrderId == item.OrderId && x.IsOnline == true).GroupBy(x => new { x.OrderId, x.PaymentFrom, x.GatewayTransId, x.IsOnline, x.status }))
                            {
                                OnlinePaymentResponseRetailer.Add(new RetailerOrderPaymentDc
                                {
                                    GatewayTransId = items.FirstOrDefault().GatewayTransId,
                                    OrderId = items.FirstOrDefault().OrderId,
                                    amount = items.Sum(x => x.amount),
                                    status = items.FirstOrDefault().status,
                                    PaymentFrom = items.FirstOrDefault().PaymentFrom,
                                    ChequeImageUrl = items.FirstOrDefault().ChequeImageUrl,
                                    ChequeBankName = items.FirstOrDefault().ChequeBankName,
                                    IsOnline = items.FirstOrDefault().IsOnline,
                                    TxnDate = items.OrderBy(c => c.CreatedDate).FirstOrDefault().CreatedDate
                                });
                            }
                            var onlineAmount = OnlinePaymentResponseRetailer.Where(x => x.IsOnline).Sum(x => x.amount);
                            #endregion
                            double cashamount = PaymentResponseRetailerAppList.Where(x => x.OrderId == item.OrderId && x.IsOnline == false).Sum(x => x.amount);
                            #region Payments 
                            foreach (var orderdata in collectPaymentDCs.TripDeliveryPayments.Where(x => x.RemaingAmount > 0).OrderByDescending(x => x.amount))
                            {
                                if (cashamount == 0)
                                {
                                    break;
                                }
                                double settleamount = 0;
                                if (cashamount <= orderdata.RemaingAmount)
                                {
                                    orderdata.RemaingAmount -= cashamount;
                                    settleamount = cashamount;
                                    cashamount = 0;
                                }
                                else
                                {
                                    settleamount = orderdata.RemaingAmount;
                                    cashamount -= settleamount;
                                    orderdata.RemaingAmount = 0;
                                }
                                if (orderdata.PaymentFrom == "Cheque" && settleamount > 0)
                                {
                                    context.TripPaymentResponseAppDb.Add(new TripPaymentResponseApps
                                    {
                                        ZilaTripOrderId = item.Id,
                                        Status = "Success",
                                        CreatedDate = indianTime,
                                        ModifiedDate = indianTime,
                                        PaymentFrom = "Cheque",
                                        Amount = Math.Round(settleamount, 0),
                                        GatewayTransId = orderdata.TransId,
                                        ChequeBankName = orderdata.ChequeBankName,
                                        ChequeImageUrl = orderdata.ChequeImageUrl,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = userId
                                    });
                                    totalChequeamt += Math.Round(settleamount, 0);
                                }
                                if (orderdata.PaymentFrom == "mPos" && settleamount > 0)
                                {
                                    context.TripPaymentResponseAppDb.Add(new TripPaymentResponseApps
                                    {
                                        ZilaTripOrderId = item.Id,
                                        Status = "Success",
                                        CreatedDate = indianTime,
                                        ModifiedDate = indianTime,
                                        PaymentFrom = "mPos",
                                        Amount = Math.Round(settleamount, 0),
                                        GatewayTransId = orderdata.TransId,
                                        IsOnline = true,
                                        IsActive = true,
                                        IsDeleted = false,
                                        CreatedBy = userId
                                    });
                                    totalMposamount += Math.Round(settleamount, 0);
                                }
                                if (orderdata.PaymentFrom == "RTGS/NEFT" && settleamount > 0)
                                {
                                    if (orderdata.IsVAN_RTGSNEFT)
                                    {
                                        context.TripPaymentResponseAppDb.Add(new TripPaymentResponseApps
                                        {
                                            ZilaTripOrderId = item.Id,
                                            Status = "Success",
                                            CreatedDate = indianTime,
                                            ModifiedDate = indianTime,
                                            PaymentFrom = "RTGS/NEFT",
                                            Amount = Math.Round(settleamount, 0),
                                            GatewayTransId = orderdata.TransId,
                                            IsOnline = true,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreatedBy = userId,
                                            IsVAN_RTGSNEFT = true
                                        });
                                        totalMposamount += Math.Round(settleamount, 0);
                                    }
                                    else
                                    {
                                        context.TripPaymentResponseAppDb.Add(new TripPaymentResponseApps
                                        {
                                            ZilaTripOrderId = item.Id,
                                            Status = "Success",
                                            CreatedDate = indianTime,
                                            ModifiedDate = indianTime,
                                            PaymentFrom = "RTGS/NEFT",
                                            Amount = Math.Round(settleamount, 0),
                                            GatewayTransId = orderdata.TransId,
                                            IsOnline = true,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreatedBy = userId,
                                            IsVAN_RTGSNEFT = false
                                        });
                                        totalMposamount += Math.Round(settleamount, 0);
                                    }
                                }
                            }
                            #endregion
                            totalAmount = totalChequeamt + totalMposamount + onlineAmount;
                            if (DispatchedMaster.GrossAmount != totalAmount)
                            {
                                Ordercashamount = DispatchedMaster.GrossAmount - totalAmount;
                            }
                            if (cashamount > 0 && Ordercashamount > 0 && cashamount == Ordercashamount)
                            {
                                context.TripPaymentResponseAppDb.Add(new TripPaymentResponseApps
                                {
                                    ZilaTripOrderId = item.Id,
                                    Status = "Success",
                                    CreatedDate = indianTime,
                                    ModifiedDate = indianTime,
                                    PaymentFrom = "Cash",
                                    Amount = Math.Round(Ordercashamount, 0),
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedBy = userId
                                });
                            }
                            if (DispatchedMaster.GrossAmount == (Ordercashamount + totalAmount))
                            {
                                item.WorkingStatus = Convert.ToInt32(WorKingStatus.AlredyCollected);
                                item.ModifiedBy = userId;
                                item.ModifiedDate = DateTime.Now;
                                context.Entry(item).State = EntityState.Modified;
                                isSuccess = true;
                            }
                            else
                            {
                                isSuccess = false;
                                scope.Dispose();
                                res = new ZilaNewResponceDc()
                                {
                                    Status = false,
                                    Message = "Payment not submitted successfully!!",
                                    ZilaTripDetailId = 0
                                };
                                break;
                            }
                        };
                        if (isSuccess)
                        {
                            tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.CollectingPayment);
                            context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                            context.Commit();
                            scope.Complete();
                            res = new ZilaNewResponceDc()
                            {
                                Status = true,
                                Message = "Payment successfully submitted!!",
                                ZilaTripDetailId = tripPlannerConfirmedDetails.Id
                            };
                        }
                    }
                    else
                    {
                        scope.Dispose();
                        res = new ZilaNewResponceDc()
                        {
                            Status = false,
                            Message = "Payment not found!!",
                            ZilaTripDetailId = 0
                        };
                        if (collectPaymentDCs.CashAmount > 0)
                        {
                            var cartPredicate1 = PredicateBuilder.New<BlockCashAmount>(x => x.CustomerId == (int)tripPlannerConfirmedDetails.CustomerId && x.Guid == blockCashAmount.Guid);
                            var blockCashAmountdb = mongoDbHelper.Select(cartPredicate1).FirstOrDefault();
                            if (blockCashAmountdb != null)
                            {
                                blockCashAmountdb.IsActive = false;
                                var result = mongoDbHelper.ReplaceAsync(blockCashAmountdb.Id, blockCashAmountdb);
                            }
                        }
                    }
                }
            }
            return res;
        }

        [AllowAnonymous]
        [Route("DeliveredGenerateOtp")]
        [HttpPost]
        public OTPOrderResponceDc GenerateOtp(long TripplannerConfirmdetailedId, string Status, double? lat, double? lg)
        {
            OTPOrderResponceDc res = null;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext context = new AuthContext())
                {
                    List<OrderDeliveryOTP> list = new List<OrderDeliveryOTP>();
                    int userId = GetLoginUserId();
                    #region APIPoint
                    // get order on TripplannerConfirmdetailedId
                    // check payment mode of all order check there is any is null the return with error payment not submitted
                    //generate otp  1111 
                    //otp wil same for all orders
                    //update status varifying otp on otp status in detail table
                    #endregion

                    var statusIds = Convert.ToInt32(WorKingStatus.AlredyCollected);
                    var statusId2 = Convert.ToInt32(WorKingStatus.CollectingPayment);
                    var tripPlannerConfirmedDetails = context.ZilaTripDetails.Where(x => x.Id == TripplannerConfirmdetailedId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var tripPlannerConfirmedOrders = context.ZilaTripOrders.Where(x => x.ZilaTripDetailId == TripplannerConfirmdetailedId && (x.WorkingStatus == statusIds || x.WorkingStatus == statusId2) && x.IsActive == true && x.IsDeleted == false).ToList();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).ToList();
                    string requireStatus = "Delivered";
                    if (requireStatus.Split(',').ToList().Contains(Status) && tripPlannerConfirmedOrders != null && tripPlannerConfirmedOrders.Any() && tripPlannerConfirmedDetails != null)
                    {
                        // " is Your Shopkirana Verification Code. :)";
                        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                        if (!string.IsNullOrEmpty(sRandomOTP))
                        {
                            var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => orderids.Contains(x.OrderId) && x.IsActive).ToList();
                            if (orderDeliveryOTPs != null)
                            {
                                orderDeliveryOTPs.ForEach(x =>
                                {
                                    x.ModifiedDate = DateTime.Now;
                                    x.ModifiedBy = userId;
                                    x.IsActive = false;
                                    context.Entry(x).State = EntityState.Modified;
                                });
                                tripPlannerConfirmedOrders.ForEach(x =>
                                {
                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                    {
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        OrderId = (int)x.OrderId,
                                        OTP = sRandomOTP,
                                        Status = Status,
                                        lat = lat,
                                        lg = lg
                                    };
                                    list.Add(OrderDeliveryOTP);
                                });
                                list.ForEach(x =>
                                {
                                    context.OrderDeliveryOTP.Add(x);
                                });
                            }

                            var orderid = tripPlannerConfirmedOrders.FirstOrDefault().OrderId;
                            var param = new SqlParameter("@OrderId", orderid);
                            var ordermaster = context.Database.SqlQuery<OrderDetailForOTPTemplateDc>("Exec GetOrderDetailForOTPTemplate  @OrderId", param).FirstOrDefault();

                            string query = "select a.warehouseid,sp.Mobile salespersonmobile,sp.FcmId salespersonfcmid,agent.Mobile agentmobile,agent.FcmId agentfcmid,c.Mobile customermobile,c.fcmId customerfmcid  from OrderMasters a Inner join  Customers c on  a.CustomerId=c.CustomerId  left join People agent on c.AgentCode=agent.PeopleID  Outer Apply (Select top 1 sp.Mobile,sp.FcmId  from OrderDetails od  Inner join People sp on od.ExecutiveId=sp.PeopleID  and od.OrderId=a.OrderId ) sp  Where  a.orderid =" + orderid;
                            var orderMobiledetail = context.Database.SqlQuery<TripOrderMobiledetail>(query).FirstOrDefault();
                            if (orderMobiledetail != null)
                            {
                                switch (Status)
                                {
                                    case "Delivered":
                                        if (!string.IsNullOrEmpty(orderMobiledetail.customermobile))
                                        {
                                            if (orderMobiledetail != null)
                                            {
                                                //Getotp(orderMobiledetail.customermobile, " is OTP for delivery of Order No (" + orderid + ")", sRandomOTP);
                                                string Message = ""; //"{#var1#} is Delivery Code for delivery of Order No. {#var2#} for Total Qty {#var3#} and Value of Rs. {#var4#}. Shopkirana";
                                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Delivered");
                                                Message = dltSMS == null ? "" : dltSMS.Template;

                                                Message = Message.Replace("{#var1#}", sRandomOTP);
                                                Message = Message.Replace("{#var2#}", ordermaster.OrderId.ToString());
                                                Message = Message.Replace("{#var3#}", ordermaster.TotalQty.ToString());
                                                Message = Message.Replace("{#var4#}", ordermaster.OrderAmount.ToString());
                                                if (dltSMS != null)
                                                    Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, Message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);

                                            }
                                        }
                                        break;
                                }
                                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.VerifyingOTP);
                                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                context.Commit();
                                scope.Complete();
                                res = new OTPOrderResponceDc()
                                {
                                    OTP = sRandomOTP,
                                    Status = true,
                                    Message = "OTP Generated Successfully!!"
                                };
                            }
                        }
                        else
                        {
                            scope.Dispose();
                            res = new OTPOrderResponceDc()
                            {
                                OTP = "",
                                Status = false,
                                Message = "OTP not found!!"
                            };
                        }
                    }
                    else
                    {
                        scope.Dispose();
                        res = new OTPOrderResponceDc()
                        {
                            OTP = "",
                            Status = false,
                            Message = "OTP not found!!"
                        };
                    }
                }
            }
            return res;
        }

        [AllowAnonymous]
        [Route("DeliverdConfirmOtpNew")]
        [HttpPost]
        public HttpResponseMessage DeliverdConfirmOtpNew(ZilaOrderConfirmOtpDc OrderConfirmOtp)
        {
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            ZilaOrderResponceDc res = null;
            Guid guid = Guid.NewGuid();
            var guidData = guid.ToString();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var context = new AuthContext())
                {
                    List<OrderMaster> ODMaster = null;
                    int userId = GetLoginUserId();
                    bool IsDeliverSmsSent = false;
                    bool Status = false;
                    #region API POINT
                    //Otp matched any order
                    //update alrwady complted status ko (completed 4.0) krna tripplannerconfirmordermaster
                    //update status otp status Completed or Pending in detail table and IsProcee true if completed
                    //write Delivered Code here check payment mode from tripPlannerConfirmedOrders for cash and mpos
                    #endregion
                    var statusIds = Convert.ToInt32(WorKingStatus.AlredyCollected);
                    var statusId1 = Convert.ToInt32(WorKingStatus.CollectingPayment);
                    var tripPlannerConfirmedOrders = context.ZilaTripOrders.Where(x => x.ZilaTripDetailId == OrderConfirmOtp.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false && (x.WorkingStatus == statusIds || x.WorkingStatus == statusId1)).ToList();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).ToList();
                    ordersToProcess.AddRange(orderids);

                    var otp = context.OrderDeliveryOTP.Where(x => orderids.Contains(x.OrderId) && x.IsActive && x.OTP == OrderConfirmOtp.Otp).ToList();
                    if (otp != null && otp.Any())
                    {
                        var People = context.Peoples.Where(x => x.Mobile == OrderConfirmOtp.DboyMobileNo).FirstOrDefault();
                        var ODM = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).Include("orderDetails").OrderBy(x => x.GrossAmount).ToList();
                        ODMaster = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Include("orderDetails").ToList();

                        var PaymentResponseRetailerAppList = context.PaymentResponseRetailerAppDb.Where(z => orderids.Contains(z.OrderId) && z.status == "Success").ToList();
                        if (OrderConfirmOtp != null && People != null && ODM != null)
                        {
                            List<VANTransactionsDC> VANTransactionList = new List<VANTransactionsDC>();
                            if (ODM[0].CustomerId > 0)
                            {
                                var customerId = new SqlParameter("@CustomerId", ODM[0].CustomerId);
                                VANTransactionList = context.Database.SqlQuery<VANTransactionsDC>("EXEC GetCustomerVANTransactions @CustomerId", customerId).ToList();
                            }
                            foreach (var newOrderMaster in ODM)
                            {
                                var DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == newOrderMaster.DeliveryIssuanceIdOrderDeliveryMaster).SingleOrDefault();
                                var ordermaster = ODMaster.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(z => z.DeliveryIssuanceId == DeliveryIssuance.DeliveryIssuanceId && z.OrderId == newOrderMaster.OrderId).FirstOrDefault();


                                //#region Damage Order status Update  
                                //if (ordermaster.OrderType == 6 && newOrderMaster.invoice_no != null)
                                //{
                                //    var DOM = context.DamageOrderMasterDB.Where(x => x.invoice_no == newOrderMaster.invoice_no).SingleOrDefault();

                                //    if (DOM != null)
                                //    {
                                //        DOM.UpdatedDate = indianTime;
                                //        DOM.Status = OrderConfirmOtp.Status;
                                //        context.Entry(DOM).State = EntityState.Modified;
                                //    }
                                //}
                                //#endregion

                                if (OrderConfirmOtp.Status == "Delivered" && newOrderMaster.Status != "Delivered")
                                {
                                    double OrderCashAmount = 0;
                                    double totalChequeamt = 0;
                                    double totalMposamount = 0;
                                    double totalAmount = 0;
                                    var tripPlannerOrders = tripPlannerConfirmedOrders.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                    var TripPaymentResponseAppslist = context.TripPaymentResponseAppDb.Where(z => z.ZilaTripOrderId == tripPlannerOrders.Id && z.Status == "Success" && z.IsActive == true && z.IsDeleted == false).ToList();

                                    double cashamount = TripPaymentResponseAppslist.Where(x => x.IsOnline == false).Sum(x => x.Amount);
                                    var RTGS_NEFTOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == newOrderMaster.OrderId && z.PaymentFrom == "RTGS/NEFT" && z.status == "Success").ToList();
                                    if (RTGS_NEFTOldEntries != null && RTGS_NEFTOldEntries.Any())
                                    {
                                        foreach (var RTGS in RTGS_NEFTOldEntries)
                                        {
                                            RTGS.status = "Failed";
                                            RTGS.statusDesc = "Due to double RTGS_NEFT request from DeliveryApp";
                                            context.Entry(RTGS).State = EntityState.Modified;
                                        }
                                    }
                                    var onlineAmount = PaymentResponseRetailerAppList.Where(x => x.IsOnline && x.OrderId == newOrderMaster.OrderId).Sum(x => x.amount);
                                    var allamount = TripPaymentResponseAppslist.Sum(x => x.Amount);
                                    if (allamount != onlineAmount && newOrderMaster.GrossAmount != onlineAmount)
                                    {
                                        #region Payments                                     
                                        foreach (var orderdata in TripPaymentResponseAppslist)
                                        {
                                            var ChequeOldEntries = PaymentResponseRetailerAppList.Where(z => z.OrderId == newOrderMaster.OrderId && z.PaymentFrom == "Cheque" && z.status == "Success").ToList();
                                            if (ChequeOldEntries != null && ChequeOldEntries.Any())
                                            {
                                                foreach (var Cheque in ChequeOldEntries)
                                                {
                                                    Cheque.status = "Failed";
                                                    Cheque.statusDesc = "Due to double Cheque request from DeliveryApp";
                                                    context.Entry(Cheque).State = EntityState.Modified;
                                                }
                                            }
                                            if (orderdata.PaymentFrom == "Cheque" && orderdata.Amount > 0)
                                            {
                                                context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                                {
                                                    OrderId = newOrderMaster.OrderId,
                                                    status = "Success",
                                                    CreatedDate = indianTime,
                                                    UpdatedDate = indianTime,
                                                    PaymentFrom = "Cheque",
                                                    statusDesc = "Due to Delivery",
                                                    amount = Math.Round(orderdata.Amount, 0),
                                                    GatewayTransId = orderdata.GatewayTransId,
                                                    ChequeBankName = orderdata.ChequeBankName,
                                                    ChequeImageUrl = orderdata.ChequeImageUrl
                                                });
                                                totalChequeamt += Math.Round(orderdata.Amount, 0);
                                            }
                                            if (orderdata.PaymentFrom == "mPos" && orderdata.Amount > 0)
                                            {
                                                context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                                {
                                                    OrderId = newOrderMaster.OrderId,
                                                    status = "Success",
                                                    CreatedDate = indianTime,
                                                    UpdatedDate = indianTime,
                                                    PaymentFrom = "mPos",
                                                    statusDesc = "Due to Delivery",
                                                    amount = Math.Round(orderdata.Amount, 0),
                                                    GatewayTransId = orderdata.GatewayTransId,
                                                    IsOnline = true
                                                });
                                                totalMposamount += Math.Round(orderdata.Amount, 0);
                                            }
                                            if (orderdata.PaymentFrom == "RTGS/NEFT" && orderdata.Amount > 0)
                                            {
                                                if (orderdata.IsVAN_RTGSNEFT)
                                                {
                                                    double totalPaybleAmount = orderdata.Amount;
                                                    foreach (var paymant in VANTransactionList.Where(x => Math.Round(x.Amount, 0) > x.UsedAmount).OrderBy(x => x.Id))
                                                    {
                                                        double RemainingAmount = 0;
                                                        double PaidAmount = 0;
                                                        RemainingAmount = Math.Round(paymant.Amount, 0) - paymant.UsedAmount;
                                                        if (totalPaybleAmount == 0)
                                                        {
                                                            break;
                                                        }
                                                        if (totalPaybleAmount <= RemainingAmount)
                                                        {
                                                            paymant.UsedAmount += totalPaybleAmount;
                                                            PaidAmount = totalPaybleAmount;
                                                            totalPaybleAmount = 0;
                                                        }
                                                        else
                                                        {
                                                            totalPaybleAmount -= RemainingAmount;
                                                            paymant.UsedAmount += RemainingAmount;
                                                            PaidAmount = RemainingAmount;
                                                        }
                                                        bool VANSettled = tripPlannerHelper.CheckVANSettled(paymant.UserReferenceNumber, context);
                                                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                                        {
                                                            OrderId = newOrderMaster.OrderId,
                                                            status = "Success",
                                                            CreatedDate = indianTime,
                                                            UpdatedDate = indianTime,
                                                            PaymentFrom = "RTGS/NEFT",
                                                            statusDesc = "Due to Delivery",
                                                            amount = Math.Round(PaidAmount, 0),
                                                            GatewayTransId = paymant.UserReferenceNumber,
                                                            IsOnline = true
                                                        });
                                                        context.VANTransactiones.Add(new VANTransaction
                                                        {
                                                            Amount = PaidAmount * -1,
                                                            ObjectType = "Order",
                                                            ObjectId = newOrderMaster.OrderId,
                                                            CustomerId = newOrderMaster.CustomerId,
                                                            CreatedDate = DateTime.Now,
                                                            IsActive = true,
                                                            IsDeleted = false,
                                                            CreatedBy = userId,
                                                            Comment = "OrderId : " + newOrderMaster.OrderId,
                                                            UsedAmount = PaidAmount,
                                                            VANTransactionParentId = paymant.Id,
                                                            IsSettled = VANSettled == false ? false : true,
                                                            Settledby = VANSettled == false ? 0 : userId,
                                                            SettledDate = VANSettled == false ? null : (DateTime?)DateTime.Now
                                                        });

                                                        var vANTransactiones = context.VANTransactiones.Where(x => x.Id == paymant.Id).FirstOrDefault();
                                                        if (vANTransactiones != null)
                                                        {
                                                            vANTransactiones.UsedAmount += PaidAmount;
                                                            context.Entry(vANTransactiones).State = EntityState.Modified;
                                                        }
                                                        totalMposamount += Math.Round(PaidAmount, 0);
                                                    }
                                                }
                                                else
                                                {
                                                    context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                                    {
                                                        OrderId = newOrderMaster.OrderId,
                                                        status = "Success",
                                                        CreatedDate = indianTime,
                                                        UpdatedDate = indianTime,
                                                        PaymentFrom = "RTGS/NEFT",
                                                        statusDesc = "Due to Delivery",
                                                        amount = Math.Round(orderdata.Amount, 0),
                                                        GatewayTransId = orderdata.GatewayTransId,
                                                        IsOnline = true
                                                    });
                                                    totalMposamount += Math.Round(orderdata.Amount, 0);
                                                }
                                            }
                                            if (orderdata.PaymentFrom == "Cash" && orderdata.Amount > 0)
                                            {
                                                context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                                {
                                                    OrderId = newOrderMaster.OrderId,
                                                    status = "Success",
                                                    CreatedDate = indianTime,
                                                    UpdatedDate = indianTime,
                                                    PaymentFrom = "Cash",
                                                    statusDesc = "Due to Delivery",
                                                    amount = Math.Round(orderdata.Amount, 0),
                                                    GatewayTransId = orderdata.GatewayTransId
                                                });
                                                totalMposamount += Math.Round(orderdata.Amount, 0);
                                            }
                                        }
                                    }
                                    #region refund
                                    var OnlinePaymentResponseRetailer = new List<RetailerOrderPaymentDc>();
                                    foreach (var item in PaymentResponseRetailerAppList.Where(x => x.IsOnline == true).GroupBy(x => new { x.OrderId, x.PaymentFrom, x.GatewayTransId, x.IsOnline, x.status }))
                                    {
                                        OnlinePaymentResponseRetailer.Add(new RetailerOrderPaymentDc
                                        {
                                            GatewayTransId = item.FirstOrDefault().GatewayTransId,
                                            OrderId = item.FirstOrDefault().OrderId,
                                            amount = item.Sum(x => x.amount),
                                            status = item.FirstOrDefault().status,
                                            PaymentFrom = item.FirstOrDefault().PaymentFrom,
                                            ChequeImageUrl = item.FirstOrDefault().ChequeImageUrl,
                                            ChequeBankName = item.FirstOrDefault().ChequeBankName,
                                            IsOnline = item.FirstOrDefault().IsOnline,
                                            TxnDate = item.OrderBy(c => c.CreatedDate).FirstOrDefault().CreatedDate
                                        });
                                    }
                                    var othermodeAmt = OnlinePaymentResponseRetailer.Where(x => x.IsOnline && x.OrderId == newOrderMaster.OrderId).Sum(x => x.amount);
                                    #endregion


                                    // var othermodeAmt = PaymentResponseRetailerAppList.Where(x => x.IsOnline /*x.PaymentFrom == "hdfc" || x.PaymentFrom == "ePaylater" || x.PaymentFrom == "Gullak"*/).Sum(x => x.amount);
                                    totalAmount = totalChequeamt + totalMposamount + othermodeAmt;
                                    if (newOrderMaster.GrossAmount != OrderCashAmount + totalAmount)
                                    {
                                        OrderCashAmount = newOrderMaster.GrossAmount - totalAmount;
                                    }
                                    var cashpayment = PaymentResponseRetailerAppList.FirstOrDefault(x => x.OrderId == newOrderMaster.OrderId && x.status == "Success" && x.PaymentFrom == "Cash");
                                    if (cashpayment != null)
                                    {
                                        cashpayment.amount = OrderCashAmount;
                                        cashpayment.UpdatedDate = indianTime;
                                        cashpayment.status = OrderCashAmount > 0 ? cashpayment.status : "Failed";
                                        cashpayment.statusDesc = "Due to Delivery";
                                        context.Entry(cashpayment).State = EntityState.Modified;
                                    }
                                    else if (cashpayment == null && allamount != onlineAmount && OrderCashAmount > 0)
                                    {
                                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                        {
                                            OrderId = newOrderMaster.OrderId,
                                            status = "Success",
                                            CreatedDate = indianTime,
                                            UpdatedDate = indianTime,
                                            statusDesc = "Due to Delivery",
                                            PaymentFrom = "Cash",
                                            amount = Math.Round(OrderCashAmount, 0)
                                        });
                                    }
                                    #endregion
                                    #region OrderDeliveryMaster
                                    OrderDeliveryMaster.Status = OrderConfirmOtp.Status;
                                    OrderDeliveryMaster.comments = OrderConfirmOtp.comments;
                                    OrderDeliveryMaster.UpdatedDate = indianTime;
                                    OrderDeliveryMaster.DeliveryLat = OrderConfirmOtp.DeliveryLat;
                                    OrderDeliveryMaster.DeliveryLng = OrderConfirmOtp.DeliveryLng; //added on 08/07/02019                              
                                    context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                                    #endregion

                                    newOrderMaster.Status = OrderConfirmOtp.Status;
                                    newOrderMaster.ReDispatchedStatus = OrderConfirmOtp.Status;
                                    newOrderMaster.comments = OrderConfirmOtp.comments;
                                    newOrderMaster.UpdatedDate = indianTime;
                                    newOrderMaster.DeliveryLat = OrderConfirmOtp.DeliveryLat;//added on 08/07/02019 
                                    newOrderMaster.DeliveryLng = OrderConfirmOtp.DeliveryLng;
                                    newOrderMaster.IsReAttempt = false;
                                    context.Entry(newOrderMaster).State = EntityState.Modified;

                                    //#endregion
                                    #region Order Master History for Status Delivered

                                    OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                    OrderMasterHistories.orderid = newOrderMaster.OrderId;
                                    OrderMasterHistories.Status = OrderConfirmOtp.Status;
                                    OrderMasterHistories.Reasoncancel = null;
                                    OrderMasterHistories.Warehousename = newOrderMaster.WarehouseName;
                                    OrderMasterHistories.DeliveryIssuanceId = newOrderMaster.DeliveryIssuanceIdOrderDeliveryMaster;//by sudhir 06-06-2019
                                    OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                    OrderMasterHistories.userid = People.PeopleID;
                                    OrderMasterHistories.CreatedDate = indianTime;
                                    context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                    #endregion

                                    ordermaster.Status = "Delivered";
                                    ordermaster.comments = OrderConfirmOtp.comments;
                                    ordermaster.DeliveredDate = indianTime;
                                    ordermaster.UpdatedDate = indianTime;
                                    context.Entry(ordermaster).State = EntityState.Modified;
                                    IsDeliverSmsSent = true;
                                    foreach (var detail in ordermaster.orderDetails)
                                    {
                                        detail.Status = OrderConfirmOtp.Status;
                                        detail.UpdatedDate = indianTime;
                                        context.Entry(detail).State = EntityState.Modified;
                                    }
                                    #region Sellerstock update
                                    if (newOrderMaster != null && newOrderMaster.CustomerType == "SellerStore" && ordermaster.Status == "Delivered")
                                    {
                                        UpdateSellerStockOfCFRProduct Postobj = new UpdateSellerStockOfCFRProduct();
                                        Postobj.OrderId = ordermaster.OrderId;
                                        Postobj.Skcode = ordermaster.Skcode;
                                        Postobj.ItemDetailDc = new List<SellerItemDetailDc>();
                                        foreach (var item in ordermaster.orderDetails)
                                        {
                                            SellerItemDetailDc newitem = new SellerItemDetailDc();
                                            newitem.ItemMultiMrpId = item.ItemMultiMRPId;
                                            newitem.SellingPrice = item.UnitPrice;
                                            newitem.qty = item.qty;
                                            Postobj.ItemDetailDc.Add(newitem);
                                        }
                                        BackgroundTaskManager.Run(() =>
                                        {
                                            try
                                            {
                                                var tradeUrl = ConfigurationManager.AppSettings["SellerURL"] + "/api/sk/RetailerAppApi/UpdateStockOnDeliveryFromSkApp";
                                                using (GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string> memberClient = new GenericRestHttpClient<UpdateSellerStockOfCFRProduct, string>(tradeUrl, "", null))
                                                {
                                                    AsyncContext.Run(() => memberClient.PostAsync(Postobj));
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                TextFileLogHelper.LogError("Error while Update Seller Stock Of CFR Product: " + ex.ToString());
                                            }
                                        });
                                    }
                                    #endregion
                                    //#region  for Franchises
                                    //if (context.Customers.Any(x => x.CustomerId == newOrderMaster.CustomerId && x.IsFranchise == true))
                                    //{
                                    //    var DeliveredOrderToFranchisesdb = context.DeliveredOrderToFranchises.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                    //    if (DeliveredOrderToFranchisesdb == null)
                                    //    {
                                    //        DeliveredOrderToFranchise FDB = new DeliveredOrderToFranchise();
                                    //        FDB.OrderId = newOrderMaster.OrderId;
                                    //        FDB.CreatedDate = indianTime;
                                    //        FDB.IsProcessed = false;
                                    //        context.DeliveredOrderToFranchises.Add(FDB);
                                    //    }
                                    //}
                                    //#endregion
                                    #region New Delivery Optimization process
                                    if (OrderConfirmOtp.TripPlannerConfirmedDetailId > 0)
                                    {
                                        //var tripPlannerConfirmedOrderId = new SqlParameter("@TripPlannerConfirmedOrderId", obj.DeliveryOptimizationdc.TripPlannerConfirmedOrderId);
                                        //long tripPlannerConfirmedDetailId = context.Database.SqlQuery<long>("GetTripPlannerConfirmedDetailId @TripPlannerConfirmedOrderId", tripPlannerConfirmedOrderId).FirstOrDefault();
                                        var tripPlannerConfirmedDetails = context.ZilaTripDetails.Where(x => x.Id == OrderConfirmOtp.TripPlannerConfirmedDetailId).FirstOrDefault();
                                        var orderIds = tripPlannerConfirmedDetails.CommaSeparatedOrderList.Split(',').Select(Int32.Parse).ToList();
                                        var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderIds.Contains(x.OrderId)).ToList();
                                        bool IsShipped = true;
                                        if (orderDispatchedMasters.Any() && orderDispatchedMasters != null)
                                        {
                                            foreach (var item in orderDispatchedMasters)
                                            {
                                                if (item.Status == "Shipped")
                                                {
                                                    IsShipped = false;
                                                }
                                            }
                                            tripPlannerConfirmedDetails.IsProcess = IsShipped;
                                            tripPlannerConfirmedDetails.ServeEndTime = DateTime.Now;

                                            var tripPlannerVehicle = context.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == tripPlannerConfirmedDetails.ZilaTripMasterId).FirstOrDefault();
                                            if (tripPlannerVehicle != null)
                                            {
                                                var tripPlannerConfirmedOrderss = context.ZilaTripOrders.Where(x => orderids.Contains((int)x.OrderId) && x.ZilaTripDetailId == OrderConfirmOtp.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                                                var tripPlannerOrderid = tripPlannerConfirmedOrderss.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                                if (tripPlannerConfirmedDetails.IsProcess)
                                                {
                                                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicle.ModifiedDate = indianTime;
                                                    tripPlannerVehicle.ModifiedBy = userId;
                                                    tripPlannerVehicle.CurrentLat = OrderConfirmOtp.DeliveryLat ?? 0;
                                                    tripPlannerVehicle.CurrentLng = OrderConfirmOtp.DeliveryLng ?? 0;
                                                    tripPlannerVehicle.ReminingTime -= (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrderss.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrderss.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault().TimeInMins);
                                                    tripPlannerVehicle.TravelTime += (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrderss.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrderss.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault().TimeInMins);
                                                    tripPlannerVehicle.DistanceLeft -= tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                    tripPlannerVehicle.DistanceTraveled += tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                    /////////////////////
                                                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                    //TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                    //tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                    //tripPlannerVehicleHistory.CurrentServingOrderId = newOrderMaster.OrderId;
                                                    //tripPlannerVehicleHistory.RecordTime = indianTime;
                                                    //tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                                    //tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                                    //tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                    //tripPlannerVehicleHistory.CreatedBy = userId;
                                                    //tripPlannerVehicleHistory.IsActive = true;
                                                    //tripPlannerVehicleHistory.IsDeleted = false;
                                                    //tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                    //tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                    //context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                                    //tripPlannerConfirmedDetails
                                                    tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.Completed);
                                                    tripPlannerConfirmedDetails.ServeEndTime = DateTime.Now;
                                                    context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                                    //tripPlannerConfirmedOrderMaster
                                                    tripPlannerOrderid.OrderStatus = Convert.ToInt32(OrderStatusEnum.Delivered);
                                                    tripPlannerOrderid.WorkingStatus = Convert.ToInt32(WorKingStatus.Completed);
                                                    context.Entry(tripPlannerOrderid).State = EntityState.Modified;
                                                }
                                                else
                                                {
                                                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicle.ModifiedDate = indianTime;
                                                    tripPlannerVehicle.ModifiedBy = userId;
                                                    tripPlannerVehicle.CurrentLat = OrderConfirmOtp.DeliveryLat ?? 0;
                                                    tripPlannerVehicle.CurrentLng = OrderConfirmOtp.DeliveryLng ?? 0;
                                                    tripPlannerVehicle.ReminingTime -= tripPlannerConfirmedOrderss.Where(x => x.OrderId == newOrderMaster.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                    tripPlannerVehicle.TravelTime += tripPlannerConfirmedOrderss.Where(x => x.OrderId == newOrderMaster.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                    //TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                    //tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                    //tripPlannerVehicleHistory.CurrentServingOrderId = newOrderMaster.OrderId;
                                                    //tripPlannerVehicleHistory.RecordTime = indianTime;
                                                    //tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                                    //tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                                    //tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                    //tripPlannerVehicleHistory.CreatedBy = userId;
                                                    //tripPlannerVehicleHistory.IsActive = true;
                                                    //tripPlannerVehicleHistory.IsDeleted = false;
                                                    //tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                    //tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                    //context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                                    tripPlannerOrderid.OrderStatus = Convert.ToInt32(OrderStatusEnum.Delivered);
                                                    tripPlannerOrderid.WorkingStatus = Convert.ToInt32(WorKingStatus.Completed);
                                                    context.Entry(tripPlannerOrderid).State = EntityState.Modified;
                                                    tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.ReachedDistination);
                                                    context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    string OrderConcernMessageFlag = "Y";//Convert.ToString(ConfigurationManager.AppSettings["OrderConcernMessageFlag"]);
                                    if (!string.IsNullOrEmpty(OrderConcernMessageFlag) && OrderConcernMessageFlag == "Y")
                                    {
                                        #region OrderConcern
                                        context.OrderConcernDB.Add(new Model.CustomerDelight.OrderConcern()
                                        {
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreatedDate = DateTime.Now,
                                            LinkId = guidData,
                                            OrderId = newOrderMaster.OrderId,
                                            CreatedBy = userId,
                                            //Status = "Open",
                                            IsCustomerRaiseConcern = false
                                        });
                                        #endregion
                                    }

                                    #region PayLaterOrderSettled
                                    var paylaterdata = context.PayLaterCollectionDb.Where(x => x.OrderId == ordermaster.OrderId && x.IsActive == true && x.IsDeleted == false && x.Status == 4).FirstOrDefault();
                                    if (paylaterdata != null)
                                    {
                                        #region ordermaster settle 
                                        CashCollectionNewController ctrl = new CashCollectionNewController();
                                        bool payres = ctrl.OrderSettle(context, paylaterdata.OrderId);
                                        #endregion
                                    }
                                    #endregion

                                }
                                #region By Kapil
                                ValidateOTPForOrder helper = new ValidateOTPForOrder();
                                string orderOTP = otp.FirstOrDefault(x => x.OrderId == newOrderMaster.OrderId).OTP;
                                if (!string.IsNullOrEmpty(orderOTP))
                                {
                                    helper.ValidateOTPForOrders(orderOTP, newOrderMaster.OrderId, OrderConfirmOtp.Status, context);
                                }
                                #endregion
                            }

                            if (context.Commit() > 0)
                            {
                                #region stock Hit on poc
                                //for currentstock
                                MultiStockHelper<OnDeliveredStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveredStockEntryDc>();
                                List<OnDeliveredStockEntryDc> OnDeliveredCStockList = new List<OnDeliveredStockEntryDc>();
                                foreach (var newOrderMaster in ODM)
                                {
                                    var ordermaster = ODMaster.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                    if (ordermaster != null && ordermaster.OrderType != 5 && newOrderMaster.Status == "Delivered")
                                    {
                                        foreach (var StockHit in newOrderMaster.orderDetails.Where(x => x.qty > 0))
                                        {
                                            var RefStockCode = ordermaster.OrderType == 8 ? "CL" : "C";
                                            bool isFree = ordermaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                            if (isFree) { RefStockCode = "F"; }
                                            else if (ordermaster.OrderType == 6) //6 Damage stock
                                            {
                                                RefStockCode = "D";
                                            }
                                            else if (ordermaster.OrderType == 9) //9 Non Sellable stock
                                            {
                                                RefStockCode = "N";
                                            }
                                            OnDeliveredCStockList.Add(new OnDeliveredStockEntryDc
                                            {
                                                ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                                OrderId = StockHit.OrderId,
                                                Qty = StockHit.qty,
                                                UserId = People.PeopleID,
                                                WarehouseId = StockHit.WarehouseId,
                                                RefStockCode = RefStockCode,

                                            });

                                        }
                                    }
                                }

                                bool results = true;
                                if (OnDeliveredCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(OnDeliveredCStockList, "Stock_OnDelivered_New", context, scope);
                                    if (!IsUpdated)
                                    {
                                        results = false;
                                        scope.Dispose();
                                        res = new ZilaOrderResponceDc()
                                        {
                                            OrderConfirmOtpDc = null,
                                            Message = "Not Delivered due insufficient stock",
                                            Status = false
                                        };
                                        ordersToProcess.RemoveAll(x => orderids.Contains(x));
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                                #endregion
                                if (results)
                                {
                                    List<Controllers.ScaleUp.OrderInvoiceRes> scalupRespones = null;
                                    if (PaymentResponseRetailerAppList.Any(x => x.PaymentFrom.ToLower() == "scaleup"))
                                    {
                                        scalupRespones = new List<Controllers.ScaleUp.OrderInvoiceRes>();
                                        ScaleUpManager scaleUpManager = new ScaleUpManager();
                                        foreach (var item in PaymentResponseRetailerAppList.Where(x => x.PaymentFrom.ToLower() == "scaleup").GroupBy(x => new { x.OrderId }))
                                        {
                                            var od = ODMaster.FirstOrDefault(x => x.OrderId == item.Key.OrderId && x.Status == "Delivered");
                                            var oDM = ODM.FirstOrDefault(x => x.OrderId == item.Key.OrderId && x.Status == "Delivered");


                                            string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                                             , HttpContext.Current.Request.Url.DnsSafeHost
                                                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                        , "/SKOrderPDF/" + od.OrderId + ".pdf");

                                            if (!string.IsNullOrEmpty(od.invoice_no) && FileUploadHelper.URLExists(FileUrl))
                                            {
                                                var response = AsyncContext.Run(async () => await scaleUpManager.OrderInvoice(item.Key.OrderId.ToString(), od.invoice_no, item.Sum(y => y.amount), oDM.CreatedDate, FileUrl, true));
                                                response.OrderNo = item.Key.OrderId.ToString();
                                                scalupRespones.Add(response);
                                            }
                                            else
                                            {
                                                scalupRespones.Add(new Controllers.ScaleUp.OrderInvoiceRes
                                                {
                                                    OrderNo = item.Key.OrderId.ToString(),
                                                    Message = "Invoice no or Order PDF path not found",
                                                    status = false
                                                });
                                            }
                                        }
                                    }

                                    if (scalupRespones == null || scalupRespones.All(x => x.status))
                                    {
                                        scope.Complete();
                                        Status = true;
                                        var order = ODM.FirstOrDefault();

                                        if (IsDeliverSmsSent && !string.IsNullOrEmpty(order.Customerphonenum))
                                        {
                                            //string message = "Hi " + order.CustomerName + " Your Order #" + order.OrderId + " is delivered on time if you have any complaint regarding your order kindly contact our customer care within next 1 Hours.";

                                            string message = ""; //"Hi {#var1#} Your Order {#var2#} is delivered on time if you have any complaint regarding your order kindly contact our customer care within {#var3#}. ShopKirana";
                                            string OrderConcernMessageFlag = "Y";//Convert.ToString(ConfigurationManager.AppSettings["OrderConcernMessageFlag"]);
                                            if (!string.IsNullOrEmpty(OrderConcernMessageFlag) && OrderConcernMessageFlag == "Y" && OrderConfirmOtp.Status == "Delivered") //
                                            {
                                                // orderCancelMsg = "Hi, Your Order Number  " + ODMaster.OrderId + ", has been delivered. In case of any concerns, please click on the link below " + ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + ODMaster.OrderId + "/" + guidData + ". ShopKirana";
                                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivered_Concern");
                                                message = dltSMS == null ? "" : dltSMS.Template;

                                                message = message.Replace("{#var1#}", order.ShopName);
                                                message = message.Replace("{#var2#}", order.OrderId.ToString());
                                                string shortUrl = Helpers.ShortenerUrl.ShortenUrl(ConfigurationManager.AppSettings["RetailerWebviewURL"] + "order-concern/" + order.OrderId + "/" + guidData);
                                                TextFileLogHelper.LogError("Error OrderConcern shortUrl" + shortUrl);
                                                message = message.Replace("{#var3#}", shortUrl);
                                                if (dltSMS != null)
                                                    Common.Helpers.SendSMSHelper.SendSMS(order.Customerphonenum, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);

                                            }
                                            else
                                            {
                                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Delivered_Order_Compaint");
                                                message = dltSMS == null ? "" : dltSMS.Template;

                                                message = message.Replace("{#var1#}", order.CustomerName);
                                                message = message.Replace("{#var2#}", order.OrderId.ToString());
                                                message = message.Replace("{#var3#}", " next 1 Hours");
                                                if (dltSMS != null)
                                                    Common.Helpers.SendSMSHelper.SendSMS(order.Customerphonenum, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                                            }

                                        }
                                        res = new ZilaOrderResponceDc()
                                        {
                                            OrderConfirmOtpDc = null,
                                            Message = "Order Delivered Succssfully!!",
                                            Status = true
                                        };
                                        ordersToProcess.RemoveAll(x => orderids.Contains(x));
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                    else
                                    {
                                        scope.Dispose();
                                        var Issueorder = scalupRespones.Where(x => !x.status).Select(x => x.OrderNo).ToList();
                                        res = new ZilaOrderResponceDc()
                                        {
                                            OrderConfirmOtpDc = OrderConfirmOtp,
                                            Message = "Due to scalup payment issue order not delivered for Orders " + string.Join(",", Issueorder) + ", please try after sometime",
                                            Status = false
                                        };

                                        logger.Error("Error when posting invoice to scalup for order " + string.Join(",", orderids) + " Error:" + string.Join(",", scalupRespones.Where(x => !x.status).Select(x => x.Message)));

                                        ordersToProcess.RemoveAll(x => orderids.Contains(x));
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            else
                            {
                                scope.Dispose();
                                res = new ZilaOrderResponceDc()
                                {
                                    OrderConfirmOtpDc = OrderConfirmOtp,
                                    Message = "Due to some error please after sometime",
                                    Status = false
                                };
                                ordersToProcess.RemoveAll(x => orderids.Contains(x));
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {
                            res = new ZilaOrderResponceDc()
                            {
                                OrderConfirmOtpDc = null,
                                Message = "Not Delivered",
                                Status = Status
                            };
                            scope.Dispose();
                            ordersToProcess.RemoveAll(x => orderids.Contains(x));
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        res = new ZilaOrderResponceDc()
                        {
                            OrderConfirmOtpDc = null,
                            Message = "Incorrect OTP",
                            Status = Status
                        };
                        scope.Dispose();
                        ordersToProcess.RemoveAll(x => orderids.Contains(x));
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }

        [Route("ZilaSendCloseKmApprovalRequest")]
        [HttpPost]
        public async Task<bool> ZilaSendCloseKmApprovalRequest(ZilaSendCloseKmApprovalRequestDc sendCloseKmApprovalRequestDc)
        {
            bool isSendNotification = false;
            string notify_type = "Last Mile Request Approval Notification";

            ZilaTripManager zilaTripManager = new ZilaTripManager();
            ConfigureNotifyHelper configureNotifyHelper = new ConfigureNotifyHelper();
            string Msg = string.Empty;
            using (var context = new AuthContext())
            {

                var people = context.Peoples.Where(x => x.PeopleID == sendCloseKmApprovalRequestDc.PeopleID).FirstOrDefault();
                if (people != null)
                {
                    Msg = people.DisplayName + " End his trip and need your approval";
                }
                else
                {
                    Msg = "End his trip and need your approval";
                }
                var vehicle = await context.ZilaTripVehicles.FirstOrDefaultAsync(x => x.ZilaTripMasterId == sendCloseKmApprovalRequestDc.zilaTripMasterId && x.IsActive == true && x.IsDeleted == false);
                if (vehicle.IsCloseKmRequestSend != true)
                {
                    isSendNotification = true;
                    zilaTripManager.ZilaInsertTripPlannerApprovalRequest(context, sendCloseKmApprovalRequestDc.zilaTripMasterId, vehicle, sendCloseKmApprovalRequestDc.PeopleID, TripPlannerApprovalRequestTypeConstants.EndKm, sendCloseKmApprovalRequestDc.closeKm, sendCloseKmApprovalRequestDc.closeKmUrl);
                    vehicle.ClosingKm = sendCloseKmApprovalRequestDc.closeKm;
                    vehicle.IsCloseKmRequestSend = true;
                    vehicle.ClosingKMUrl = sendCloseKmApprovalRequestDc.closeKmUrl;
                    context.Entry(vehicle).State = EntityState.Modified;
                    context.Commit();
                }
            }

            if (isSendNotification)
            {
                using (var context = new AuthContext())
                {
                    configureNotifyHelper.SendNotificationForSarthiTripApproval(Msg, sendCloseKmApprovalRequestDc.zilaTripMasterId, notify_type, context);
                    context.Commit();

                }
            }

            return true;
        }

        [Route("ZilaCompleteTripStatusChange")]
        [HttpGet]
        public ZilaResponceDc ZilaCompleteTripStatusChange(long ZilaTripdetailedId, double lat, double lng)
        {
            bool Status = false;
            ZilaResponceDc res = null;
            int userId = GetLoginUserId();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    var zilaTripDetails = context.ZilaTripDetails.FirstOrDefault(x => x.Id == ZilaTripdetailedId);
                    if (zilaTripDetails != null)
                    {
                        if (zilaTripDetails.OrderCount == 0)
                        {
                            zilaTripDetails.CustomerTripStatus = (int)CustomerTripStatusEnum.Completed;
                            zilaTripDetails.ServeEndTime = DateTime.Now;
                            context.Entry(zilaTripDetails).State = EntityState.Modified;
                            Status = true;
                        }
                        else
                        {
                            zilaTripDetails.CustomerTripStatus = (int)CustomerTripStatusEnum.ReachedDistination;
                            zilaTripDetails.ServeStartTime = DateTime.Now;
                            context.Entry(zilaTripDetails).State = EntityState.Modified;
                            Status = true;
                        }
                        zilaTripDetails.UnloadLat = lat;
                        zilaTripDetails.UnloadLng = lng;
                        zilaTripDetails.ModifiedBy = userId;
                        zilaTripDetails.ModifiedDate = DateTime.Now;
                        context.Entry(zilaTripDetails).State = EntityState.Modified;
                        Status = true;
                        context.Commit();
                    }
                }
                if (Status)
                {
                    scope.Complete();
                    res = new ZilaResponceDc()
                    {
                        Status = Status,
                        Message = "Complete Trip Status Change SuccessFully!!",
                        TripDashboardDC = null
                    };
                }
                else
                {
                    scope.Dispose();
                    res = new ZilaResponceDc()
                    {
                        Status = Status,
                        Message = "Someting went Wrong!!",
                        TripDashboardDC = null
                    };
                }
            }
            return res;
        }

        [Route("ZilaEndAssignment")]
        [HttpPost]
        public HttpResponseMessage ZilaEndAssignment(ZilaAssignmentStartEndDc assignmentStartEndDc)
        {
            ZilaTripManager zilaTripManager = new ZilaTripManager();
            var res = zilaTripManager.ZilaEndAssignment(assignmentStartEndDc, assignmentStartEndDc.PeopleID);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [Route("ZilaEnterMilometerLimit")]
        [HttpGet]
        public TripMilometerDc ZilaEnterMilometerLimit(long zilaTripMasterId)
        {
            using (var context = new AuthContext())
            {
                var vehicle = context.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == zilaTripMasterId).FirstOrDefault();
                TripMilometerDc tripMilometer = new TripMilometerDc
                {
                    StartKm = (int)vehicle.StartKm,
                    MaxEndKm = (int)vehicle.StartKm + int.Parse(ConfigurationManager.AppSettings["TripVehicleMaxTravelledDistanceInKm"])
                };
                return tripMilometer;
            }
        }

        [Route("ZilaTripGetDboyRatingOrder")]
        [HttpGet]
        [AllowAnonymous]
        public ZilaTripRatingDboyDC ZilaTripGetDboyRatingOrder(int Id) //Id : OrderId
        {
            ZilaTripRatingDboyDC ratingDboyDC = new ZilaTripRatingDboyDC();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("OrderId", Id);
                var Order = context.Database.SqlQuery<ZilaTripDeliveryDboyRatingOrderDc>("exec [Zila].[Zila_GetDboyRatingOrder] @OrderId", param).FirstOrDefault();
                var ratingConfig = context.RatingMasters.Where(x => x.AppType == 2 && x.IsActive == true && x.IsDeleted == false).Include(x => x.RatingDetails).ToList();
                var result = Mapper.Map(ratingConfig).ToANew<List<ZilaUserRatingDc>>();
                if (Order != null && ratingConfig != null)
                {
                    ratingDboyDC.DeliveryDboyRatingOrder = Order;
                    result.ForEach(x =>
                    {
                        x.AppTypeName = "Delivery Rating";
                    });
                    ratingDboyDC.userRatingDc = result;
                }
            }
            return ratingDboyDC;
        }

        private APIResponse CreateCustomTripPvt(ZilaCustomTrip customTrip)
        {

            APIResponse res;
            int userId = (int)customTrip.DboyId;
            DateTime currentTime = DateTime.Now;
            DateTime today = DateTime.Today.AddDays(1).Date;

            using (var context = new AuthContext())
            {
                var dboy = context.DboyMasters.FirstOrDefault(x => x.PeopleId == customTrip.DboyId && x.IsActive == true && x.IsDeleted == false);
                if (dboy == null || !dboy.VehicleMasterId.HasValue || dboy.VehicleMasterId == 0)
                {
                    return res = new APIResponse
                    {
                        Data = 0,
                        Message = "Vehicle not mapped!!",
                        Status = false
                    };
                    //  return 0;
                }

                customTrip.DboyId = dboy.Id;

                using (var db = new AuthContext())
                {
                    var dboyId = new SqlParameter("@DboyId", customTrip.DboyId);
                    var trips = db.Database.SqlQuery<GetZilaTripDc>("EXEC [Zila].[ZilaTripPlanner_GetMobileAppTripList] @DboyId", dboyId).ToList();
                    if (trips != null && trips.Any())
                    {
                        return res = new APIResponse
                        {
                            Data = trips.First().ZilaTripMasterId,
                            Message = "Success!!",
                            Status = true
                        };
                        // return trips.First().ZilaTripMasterId;
                    }
                }


                var warehouse = context.Warehouses.First(x => x.WarehouseId == dboy.WarehouseId);
                ZilaTripMaster master = new ZilaTripMaster
                {
                    AgentId = 0,
                    WarehouseId = dboy.WarehouseId,
                    CreatedBy = userId,
                    CreatedDate = currentTime,
                    CustomerCount = 0,
                    DboyId = customTrip.DboyId,
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
                    VehicleMasterId = dboy.VehicleMasterId.HasValue ? dboy.VehicleMasterId.Value : 0,
                    WarehouseLat = warehouse.latitude,
                    WarehouseLng = warehouse.longitude
                };


                List<ZilaTripDetail> detilList = new List<ZilaTripDetail>();
                detilList.Add(new ZilaTripDetail
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
                master.ZilaTripDetailList = detilList;
                context.ZilaTripMasters.Add(master);
                context.Commit();
                ZilaTripVehicle vehicle = new ZilaTripVehicle
                {
                    IsActive = true,
                    ActualDistanceTraveled = 0,
                    BreakStartTime = null,
                    BreakTimeInSec = 0,
                    CloseKmApprovedBy = null,
                    CloseKmApprovedDate = null,
                    ClosingKm = 0,
                    ClosingKMUrl = null,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    CurrentLat = 0,
                    CurrentLng = 0,
                    CurrentStatus = 0,
                    DistanceLeft = 0,
                    DistanceTraveled = 0,
                    EndTime = null,
                    IsCloseKmApproved = false,
                    IsCloseKmRequestSend = false,
                    IsClosingKmManualReading = false,
                    IsDeleted = false,
                    IsRearrangeDone = true,
                    IsStartKmApproved = false,
                    IsStartKmRequestSend = false,
                    LateReportingTimeInMins = null,
                    ModifiedBy = null,
                    ModifiedDate = null,
                    PenaltyCharge = null,
                    ReminingTime = 0,
                    ReportingTime = null,
                    StartKm = null,
                    StartKmApprovedBy = null,
                    StartKmApprovedDate = null,
                    StartKmUrl = null,
                    StartTime = null,
                    TravelTime = 0,
                    ZilaTripMasterId = master.Id
                };

                context.ZilaTripVehicles.Add(vehicle);
                context.Commit();

                return res = new APIResponse
                {
                    Data = master.Id,
                    Message = "Success!!",
                    Status = true
                };
                // return master.Id;
            }
        }

        [Route("ZilaGetTripList")]
        [HttpGet]
        public List<TripMasterForDropDown> ZilaGetTripList(int warehouseId, bool isTripExistsInAssignment, int FilterType)
        {
            ZilaTripManager zilaTripManager = new ZilaTripManager();
            var result = zilaTripManager.ZilaGetTripList(warehouseId, isTripExistsInAssignment, FilterType);
            return result;
        }

        [Route("ZilaGetTripSummary")]
        [HttpGet]
        public List<TripSummary> ZilaGetTripSummary(int tripMasterId)
        {
            using (var authContext = new AuthContext())
            {
                var tripPlannerMasterId = new SqlParameter("@ZilaMasterId", tripMasterId);
                var list = authContext.Database.SqlQuery<TripSummary>("[Zila].[Zila_GetTripSummary] @ZilaMasterId", tripPlannerMasterId).ToList();
                if (list != null && list.Any())
                {
                    list.ForEach(x =>
                    {
                        if (string.IsNullOrEmpty(x.CurrentStatus))
                        {
                            x.CurrentStatus = "Trip Not Freezed!";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.OnDuty)
                        {
                            x.CurrentStatus = "OnDuty";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.NotStarted)
                        {
                            x.CurrentStatus = "NotStarted";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.InTransit)
                        {
                            x.CurrentStatus = "InTransit";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.OnBreak)
                        {
                            x.CurrentStatus = "OnBreak";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.Delivering)
                        {
                            x.CurrentStatus = "Delivering";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.TripEnd)
                        {
                            x.CurrentStatus = "TripEnd";
                        }
                        else if (Int32.Parse(x.CurrentStatus) == (int)VehicleliveStatus.RejectTrip)
                        {
                            x.CurrentStatus = "RejectTrip";
                        }
                    });
                }
                return list;
            }
        }

        [Route("AllRedispatReattemptConfirmOtp")]
        [HttpPost]
        public HttpResponseMessage AllRedispatReattemptConfirmOtp(AllRedispatReattemptConfirmOtp allRedispatReattemptConfirmOtp)
        {
            ResponceDc res = null;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    int statusValue = 0;
                    List<OrderMaster> ODMaster = null;
                    bool Status = false;
                    bool ReAttempt = false;
                    bool OTPSuccess = false;
                    if (!allRedispatReattemptConfirmOtp.ReAttempt)
                    {
                        statusValue = Convert.ToInt32(WorKingStatus.DeliveryRedispatch);
                    }
                    else
                    {
                        statusValue = Convert.ToInt32(WorKingStatus.ReAttempt);
                        ReAttempt = true;
                    }
                    string orderStatus = "Delivery Redispatch";
                    var tripPlannerConfirmedOrders = context.ZilaTripOrders.Where(x => x.ZilaTripDetailId == allRedispatReattemptConfirmOtp.TripPlannerConfirmedDetailId && x.WorkingStatus == statusValue && x.IsActive == true && x.IsDeleted == false).ToList();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).ToList();
                    ordersToProcess.AddRange(orderids);

                    //if (ReAttempt)
                    //{
                    //    OTPSuccess = true;
                    //}
                    //else
                    //{
                    #region By Kapil
                    ValidateOTPForOrder helper = new ValidateOTPForOrder();
                    foreach (var orderid in orderids)
                    {
                        helper.ValidateOTPForOrders(allRedispatReattemptConfirmOtp.Otp, Convert.ToInt32(orderid), orderStatus, context);
                    }
                    #endregion


                    var otp = context.OrderDeliveryOTP.Where(x => orderids.Contains(x.OrderId) && x.IsActive && x.OTP == allRedispatReattemptConfirmOtp.Otp).ToList();
                    if (otp != null && otp.Any())
                    {
                        OTPSuccess = true;
                    }
                    else
                    {
                        OTPSuccess = false;
                    }
                    //}
                    if (OTPSuccess)
                    {
                        var People = context.Peoples.Where(x => x.PeopleID == allRedispatReattemptConfirmOtp.userId).FirstOrDefault();
                        var ODM = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId) && x.Status == "Shipped").Include("orderDetails").ToList();
                        ODMaster = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId) && x.Status == "Shipped").Include("orderDetails").ToList();
                        var deliveryIssuanceIds = ODM.Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster);
                        var DeliveryIssuances = context.DeliveryIssuanceDb.Where(x => deliveryIssuanceIds.Contains(x.DeliveryIssuanceId)).ToList();
                        if (allRedispatReattemptConfirmOtp != null && People != null && DeliveryIssuances != null && DeliveryIssuances.Any() && ODM != null)
                        {
                            var amount = ODM.Sum(x => x.GrossAmount);
                            var warehouseId = ODM.FirstOrDefault().WarehouseId;
                            MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges>();
                            var customerRedispatchCharges = mongoDbHelper.Select(x => x.MinValue < amount && x.MaxValue >= amount && x.WarehouseId == warehouseId).FirstOrDefault();

                            foreach (var newOrderMaster in ODM)
                            {
                                var DeliveryIssuance = DeliveryIssuances.Where(x => x.DeliveryIssuanceId == newOrderMaster.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault();
                                var ordermaster = ODMaster.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(z => z.DeliveryIssuanceId == DeliveryIssuance.DeliveryIssuanceId && z.OrderId == newOrderMaster.OrderId).FirstOrDefault();


                                if (orderStatus == "Delivery Redispatch" && newOrderMaster.Status != "Delivery Redispatch")
                                {
                                    OrderDeliveryMaster.Status = orderStatus;
                                    OrderDeliveryMaster.comments = allRedispatReattemptConfirmOtp.Reason;
                                    OrderDeliveryMaster.UpdatedDate = indianTime;
                                    OrderDeliveryMaster.DeliveryLat = allRedispatReattemptConfirmOtp.lat;//added on 08/07/02019 
                                    OrderDeliveryMaster.DeliveryLng = allRedispatReattemptConfirmOtp.lg;
                                    context.Entry(OrderDeliveryMaster).State = EntityState.Modified;

                                    newOrderMaster.Status = "Delivery Redispatch";
                                    newOrderMaster.ReDispatchedStatus = "Delivery Redispatch";
                                    newOrderMaster.comments = allRedispatReattemptConfirmOtp.Reason;
                                    newOrderMaster.UpdatedDate = indianTime;
                                    newOrderMaster.DeliveryLat = allRedispatReattemptConfirmOtp.lat;//added on 08/07/02019 
                                    newOrderMaster.DeliveryLng = allRedispatReattemptConfirmOtp.lg;
                                    newOrderMaster.ReDispatchedDate = DateTime.Now;
                                    if (allRedispatReattemptConfirmOtp.ReAttempt)
                                    {
                                        newOrderMaster.ReAttemptCount = newOrderMaster.ReAttemptCount + 1;
                                        newOrderMaster.IsReAttempt = true;
                                    }
                                    else
                                    {
                                        newOrderMaster.ReDispatchCount = newOrderMaster.ReDispatchCount + 1;
                                        newOrderMaster.IsReAttempt = false;
                                    }
                                    DateTime? NextRedispatchedDate = DateTime.ParseExact(allRedispatReattemptConfirmOtp.NextRedispatchedDate, "dd/MM/yyyy", null);
                                    newOrderMaster.NextRedispatchDate = NextRedispatchedDate;//allRedispatReattemptConfirmOtp.NextRedispatchedDate;
                                    context.Entry(newOrderMaster).State = EntityState.Modified;
                                    #region Order Master History for Status Delivery Redispatch

                                    OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                    OrderMasterHistories.orderid = newOrderMaster.OrderId;
                                    OrderMasterHistories.Status = newOrderMaster.Status;
                                    OrderMasterHistories.Reasoncancel = null;
                                    OrderMasterHistories.Warehousename = newOrderMaster.WarehouseName;
                                    OrderMasterHistories.DeliveryIssuanceId = OrderDeliveryMaster.DeliveryIssuanceId;
                                    OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                    OrderMasterHistories.userid = People.PeopleID;
                                    OrderMasterHistories.CreatedDate = DateTime.Now;
                                    if (allRedispatReattemptConfirmOtp.ReAttempt)
                                    {
                                        OrderMasterHistories.IsReAttempt = true;
                                    }
                                    context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                    #endregion
                                    ordermaster.Status = orderStatus;
                                    ordermaster.comments = allRedispatReattemptConfirmOtp.Reason;
                                    ordermaster.UpdatedDate = indianTime;
                                    ordermaster.ReDispatchCount = newOrderMaster.ReDispatchCount;
                                    context.Entry(ordermaster).State = EntityState.Modified;

                                    foreach (var detail in ordermaster.orderDetails)
                                    {
                                        detail.Status = orderStatus;
                                        detail.UpdatedDate = indianTime;
                                        context.Entry(detail).State = EntityState.Modified;

                                    }
                                    #region Commentcode
                                    #endregion
                                    #region New Delivery Optimization process

                                    if (tripPlannerConfirmedOrders != null)
                                    {
                                        var detialsId = tripPlannerConfirmedOrders.FirstOrDefault().ZilaTripDetailId;
                                        var tripPlannerConfirmedDetails = context.ZilaTripDetails.Where(x => x.Id == detialsId).FirstOrDefault();
                                        var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).ToList();
                                        bool IsShipped = true;
                                        if (orderDispatchedMasters.Any() && orderDispatchedMasters != null)
                                        {
                                            foreach (var item in orderDispatchedMasters)
                                            {
                                                if (item.Status == "Shipped")
                                                {
                                                    IsShipped = false;
                                                }
                                            }
                                            tripPlannerConfirmedDetails.IsProcess = IsShipped;
                                            var tripPlannerVehicle = context.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == tripPlannerConfirmedDetails.ZilaTripMasterId).FirstOrDefault();
                                            if (tripPlannerVehicle != null)
                                            {
                                                var tripPlannerOrderid = tripPlannerConfirmedOrders.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                                if (tripPlannerConfirmedDetails.IsProcess)
                                                {
                                                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicle.ModifiedDate = indianTime;
                                                    tripPlannerVehicle.ModifiedBy = allRedispatReattemptConfirmOtp.userId;
                                                    tripPlannerVehicle.CurrentLat = allRedispatReattemptConfirmOtp.lat ?? 0;
                                                    tripPlannerVehicle.CurrentLng = allRedispatReattemptConfirmOtp.lg ?? 0;
                                                    tripPlannerVehicle.ReminingTime -= (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault().TimeInMins);
                                                    tripPlannerVehicle.TravelTime += (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault().TimeInMins);
                                                    tripPlannerVehicle.DistanceLeft -= tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                    tripPlannerVehicle.DistanceTraveled += tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                                    /////////////////////
                                                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                    if (allRedispatReattemptConfirmOtp.ReAttempt)
                                                    {
                                                        tripPlannerOrderid.OrderStatus = Convert.ToInt32(OrderStatusEnum.ReAttempt);
                                                    }
                                                    else
                                                    {
                                                        tripPlannerOrderid.OrderStatus = Convert.ToInt32(OrderStatusEnum.DeliveryRedispatch);
                                                    }
                                                    tripPlannerOrderid.WorkingStatus = Convert.ToInt32(WorKingStatus.failed);
                                                    context.Entry(tripPlannerOrderid).State = EntityState.Modified;
                                                    tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.Completed);
                                                    tripPlannerConfirmedDetails.ServeEndTime = DateTime.Now;
                                                    context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                                }
                                                else
                                                {
                                                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicle.ModifiedDate = indianTime;
                                                    tripPlannerVehicle.ModifiedBy = allRedispatReattemptConfirmOtp.userId;
                                                    tripPlannerVehicle.CurrentLat = allRedispatReattemptConfirmOtp.lat ?? 0;
                                                    tripPlannerVehicle.CurrentLng = allRedispatReattemptConfirmOtp.lg ?? 0;
                                                    tripPlannerVehicle.ReminingTime -= tripPlannerConfirmedOrders.Where(x => x.OrderId == newOrderMaster.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                    tripPlannerVehicle.TravelTime += tripPlannerConfirmedOrders.Where(x => x.OrderId == newOrderMaster.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                                    if (allRedispatReattemptConfirmOtp.ReAttempt)
                                                    {
                                                        tripPlannerOrderid.OrderStatus = Convert.ToInt32(OrderStatusEnum.ReAttempt);
                                                    }
                                                    else
                                                    {
                                                        tripPlannerOrderid.OrderStatus = Convert.ToInt32(OrderStatusEnum.DeliveryRedispatch);
                                                    }
                                                    tripPlannerOrderid.WorkingStatus = Convert.ToInt32(WorKingStatus.failed);
                                                    context.Entry(tripPlannerOrderid).State = EntityState.Modified;
                                                }
                                            }
                                        }
                                    }
                                    otp = context.OrderDeliveryOTP.Where(x => x.OrderId == newOrderMaster.OrderId && x.IsActive == true).ToList();
                                    if (otp != null && otp.Any())
                                    {
                                        otp.ForEach(x =>
                                        {
                                            x.IsActive = false;
                                            context.Entry(x).State = EntityState.Modified;
                                        });
                                    }
                                    #endregion

                                    if (!allRedispatReattemptConfirmOtp.ReAttempt && ordermaster.Deliverydate.Date == DateTime.Now.Date)
                                    {
                                        if (customerRedispatchCharges != null && customerRedispatchCharges.RedispatchCharges > 0)
                                        {
                                            Wallet wlt = context.WalletDb.FirstOrDefault(c => c.CustomerId == ordermaster.CustomerId);
                                            if (wlt != null)
                                            {
                                                CustomerWalletHistory CWH = new CustomerWalletHistory();
                                                CWH.WarehouseId = ordermaster.WarehouseId;
                                                CWH.CompanyId = ordermaster.CompanyId;
                                                CWH.CustomerId = wlt.CustomerId;
                                                CWH.NewAddedWAmount = 0;
                                                CWH.NewOutWAmount = -(customerRedispatchCharges.RedispatchCharges * 10);
                                                CWH.OrderId = ordermaster.OrderId;
                                                CWH.Through = "From Order Redispatched";
                                                CWH.TotalWalletAmount = wlt.TotalAmount - (customerRedispatchCharges.RedispatchCharges * 10);
                                                CWH.CreatedDate = indianTime;
                                                CWH.UpdatedDate = indianTime;
                                                context.CustomerWalletHistoryDb.Add(CWH);

                                                wlt.TotalAmount -= (customerRedispatchCharges.RedispatchCharges * 10);
                                                wlt.TransactionDate = indianTime;
                                                context.Entry(wlt).State = EntityState.Modified;
                                            }
                                        }
                                    }
                                }
                            }
                            if (context.Commit() > 0)
                            {
                                #region stock Hit on poc
                                //for currentstock
                                MultiStockHelper<OnDeliveryRedispatchedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryRedispatchedStockEntryDc>();
                                List<OnDeliveryRedispatchedStockEntryDc> DeliveryRedispatchedCStockList = new List<OnDeliveryRedispatchedStockEntryDc>();
                                foreach (var newOrderMaster in ODM)
                                {
                                    var ordermaster = ODMaster.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                    if (ordermaster != null && ordermaster.OrderType != 5 && newOrderMaster.Status == "Delivery Redispatch")
                                    {
                                        foreach (var StockHit in newOrderMaster.orderDetails.Where(x => x.qty > 0))
                                        {
                                            var RefStockCode = ordermaster.OrderType == 8 ? "CL" : "C";
                                            bool isFree = ordermaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                            if (isFree) { RefStockCode = "F"; }
                                            else if (ordermaster.OrderType == 6) //6 Damage stock
                                            {
                                                RefStockCode = "D";
                                            }
                                            else if (ordermaster.OrderType == 9) //9 Non Sellable stock
                                            {
                                                RefStockCode = "N";
                                            }
                                            DeliveryRedispatchedCStockList.Add(new OnDeliveryRedispatchedStockEntryDc
                                            {
                                                ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                                OrderId = StockHit.OrderId,
                                                Qty = StockHit.qty,
                                                UserId = People.PeopleID,
                                                WarehouseId = StockHit.WarehouseId,
                                                RefStockCode = RefStockCode,
                                                IsDeliveryCancel = false
                                            });
                                        }
                                    }
                                }
                                bool results = true;
                                if (DeliveryRedispatchedCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryRedispatchedCStockList, "Stock_OnDeliveryRedispatch", context, scope);
                                    if (!IsUpdated)
                                    {
                                        scope.Dispose();
                                        results = false;
                                        res = new ResponceDc()
                                        {
                                            TripDashboardDC = null,
                                            Message = "Order Not Delivery Redispatch",
                                            Status = false
                                        };
                                        ordersToProcess.RemoveAll(x => orderids.Contains(x));
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                                #endregion
                                if (results)
                                {
                                    scope.Complete();
                                    Status = true;
                                    res = new ResponceDc()
                                    {
                                        TripDashboardDC = null,
                                        Message = "Order Delivery Redispatch Succssfully!!",
                                        Status = true
                                    };
                                    ordersToProcess.RemoveAll(x => orderids.Contains(x));
                                    return Request.CreateResponse(HttpStatusCode.OK, res);
                                }
                            }
                            else
                            {
                                scope.Dispose();
                                res = new ResponceDc()
                                {
                                    TripDashboardDC = null,
                                    Message = "Due to some error please after sometime",
                                    Status = false
                                };
                                ordersToProcess.RemoveAll(x => orderids.Contains(x));
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {
                            res = new ResponceDc()
                            {
                                TripDashboardDC = null,
                                Message = "Not Delivered",
                                Status = Status
                            };
                            scope.Dispose();
                            ordersToProcess.RemoveAll(x => orderids.Contains(x));
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                    }
                    else
                    {
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Message = "Incorrect OTP",
                            Status = Status
                        };
                        scope.Dispose();
                        ordersToProcess.RemoveAll(x => orderids.Contains(x));
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }


        [Route("GetStoreType")]
        [HttpGet]
        [AllowAnonymous]
        public int GetStoreType(int warehouseId)
        {
            using (var context = new AuthContext())
            {
                int storeType =context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId).StoreType;
                return storeType;
            }
        }
    }
}

