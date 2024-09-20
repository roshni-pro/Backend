using AngularJSAuthentication.API.Managers.TripPlanner;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.DataContracts.Transaction.Zila.OperationCapacity;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Zila.OperationCapacity;
using AngularJSAuthentication.ORTools.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.Helper.Zila
{
    public class ZilaTripPlannerHelper
    {
        public ResponceMsg AssignmentFinalize(long zilaTripMasterId, int userid, AuthContext context, TransactionScope scope, int startingKm, DateTime reportingTime, int? lateReportingTimeInMins, double? penaltyCharge, bool IsReturn = false)
        {
            bool TripstatusCheck = false;
            ResponceMsg msg;
            TripPlannerVehicleManager tripPlannerVehicleManager = new TripPlannerVehicleManager();
            var trip = context.ZilaTripMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.Id == zilaTripMasterId).FirstOrDefault();
            var zilaTripDetails = context.ZilaTripDetails.Where(x => x.IsActive == true && x.IsDeleted == false && x.ZilaTripMasterId == zilaTripMasterId).ToList();
            //bool IsPendingCityAndDamage = checkCurrentTripStatusForCityAndDamage(trip.DboyId, context); 

            //if (!IsPendingCityAndDamage)
            //{
            //TripstatusCheck = true;
            //}

            TripstatusCheck = true;
            if (TripstatusCheck)
            {
                foreach (var item in zilaTripDetails)
                {
                    item.RealSequenceNo = item.SequenceNo;
                    context.Entry(item).State = EntityState.Modified;
                }
                trip.IsFreezed = true;
                trip.ModifiedBy = userid;
                trip.ModifiedDate = DateTime.Now;
                ZilaTripVehicle AddDetails = new ZilaTripVehicle()
                {
                    ZilaTripMasterId = trip.Id,
                    ReminingTime = trip.TotalTimeInMins,
                    DistanceLeft = trip.TotalDistanceInMeter,
                    ReportingTime = reportingTime,
                    StartKm = startingKm,
                    PenaltyCharge = penaltyCharge,
                    LateReportingTimeInMins = lateReportingTimeInMins
                };
                bool value = InsertTripDistance(AddDetails, userid, context);
                var peopledata = context.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false).FirstOrDefault(); //userdata
                var query = from tcm in context.ZilaTripMasters
                            join td in context.ZilaTripDetails on tcm.Id equals td.ZilaTripMasterId
                            join to in context.ZilaTripOrders on td.Id equals to.ZilaTripDetailId
                            where tcm.Id == trip.Id && to.IsActive == true && to.IsDeleted == false

                            select new
                            {
                                orderId = to.OrderId,
                                zilaTripMasterId = tcm.Id
                            };
                var result = query.ToList();
                if (result != null && result.Any() && peopledata != null)
                {
                    //all order
                    var zilaTripMasterIds = result.Select(x => x.zilaTripMasterId).FirstOrDefault();
                    var deliveryIssuanceList = context.DeliveryIssuanceDb.Where(x => x.ZilaTripMasterId == zilaTripMasterIds && x.Status == "Assigned").ToList();
                    //foreach (var obj in deliveryIssuanceList)
                    //{
                    //    obj.UpdatedDate = DateTime.Now;
                    //    obj.Status = "Assigned";
                    //    var AOrderIds = obj.OrderIds.Split(',').Select(Int32.Parse).ToList();
                    //    var orderDMLists = context.OrderDispatchedMasters.Where(c => AOrderIds.Contains(c.OrderId)).Include(c => c.orderDetails).ToList();
                    //    var orderMLists = context.DbOrderMaster.Where(c => AOrderIds.Contains(c.OrderId)).Include("orderDetails").ToList();
                    //    var Orderids = orderDMLists.Select(x => x.OrderId).Distinct().ToList();
                    //    var ReturnOrderId = orderMLists.Where(x => Orderids.Contains(x.OrderId) && x.OrderType == 3).Select(x => x.OrderId).ToList();
                    //    var ROrderids = ReturnOrderId != null ? ReturnOrderId : new List<int>();
                    //    obj.PeopleID = orderDMLists.FirstOrDefault().DBoyId;
                    //    obj.DisplayName = orderDMLists.FirstOrDefault().DboyName;
                    //    foreach (var o in orderDMLists)

                    //    {
                    //        o.Status = "Issued";
                    //        o.UpdatedDate = DateTime.Now;
                    //        #region Code For OrderDeliveryMaster
                    //        OrderDeliveryMaster oDm = new OrderDeliveryMaster();
                    //        oDm.OrderId = o.OrderId;
                    //        oDm.CityId = o.CityId;
                    //        oDm.CompanyId = o.CompanyId;
                    //        oDm.WarehouseId = o.WarehouseId;
                    //        oDm.WarehouseName = o.WarehouseName;
                    //        oDm.DboyMobileNo = o.DboyMobileNo;
                    //        oDm.DboyName = o.DboyName;
                    //        oDm.CustomerId = o.CustomerId;
                    //        oDm.CustomerName = o.CustomerName;
                    //        oDm.Customerphonenum = o.Customerphonenum;
                    //        oDm.ShopName = o.ShopName;
                    //        oDm.Skcode = o.Skcode;
                    //        oDm.Status = "Issued"; //OrderDMaster.Status;
                    //        oDm.ShippingAddress = o.ShippingAddress;
                    //        oDm.BillingAddress = o.BillingAddress;
                    //        oDm.CanceledStatus = o.CanceledStatus;
                    //        oDm.invoice_no = o.invoice_no;
                    //        oDm.OnlineServiceTax = o.OnlineServiceTax;
                    //        oDm.TotalAmount = o.TotalAmount;
                    //        oDm.GrossAmount = o.GrossAmount;
                    //        oDm.TaxAmount = o.TaxAmount;
                    //        oDm.SGSTTaxAmmount = o.SGSTTaxAmmount;
                    //        oDm.CGSTTaxAmmount = o.CGSTTaxAmmount;
                    //        oDm.ReDispatchedStatus = o.ReDispatchedStatus;
                    //        oDm.Trupay = o.Trupay;
                    //        oDm.comments = o.comments;
                    //        oDm.deliveryCharge = o.deliveryCharge;
                    //        oDm.DeliveryIssuanceId = o.DeliveryIssuanceIdOrderDeliveryMaster ?? 0;
                    //        oDm.DiscountAmount = o.DiscountAmount;
                    //        oDm.CheckNo = o.CheckNo;
                    //        oDm.CheckAmount = o.CheckAmount;
                    //        oDm.ElectronicPaymentNo = o.ElectronicPaymentNo;
                    //        oDm.ElectronicAmount = o.ElectronicAmount;
                    //        oDm.EpayLaterAmount = 0;
                    //        oDm.CashAmount = o.CashAmount;
                    //        oDm.OrderedDate = o.OrderedDate;
                    //        oDm.WalletAmount = o.WalletAmount;
                    //        oDm.RewardPoint = o.RewardPoint;
                    //        oDm.Tin_No = o.Tin_No;
                    //        oDm.ReDispatchCount = o.ReDispatchCount;
                    //        oDm.UpdatedDate = DateTime.Now;
                    //        oDm.CreatedDate = DateTime.Now;
                    //        context.OrderDeliveryMasterDB.Add(oDm);
                    //        #endregion

                    //        #region Order Master History
                    //        OrderMasterHistories hh1 = new OrderMasterHistories();
                    //        if (o != null)
                    //        {
                    //            hh1.orderid = o.OrderId;
                    //            hh1.Status = o.Status;
                    //            hh1.Reasoncancel = null;
                    //            hh1.Warehousename = o.WarehouseName;
                    //            if (peopledata.DisplayName == null || peopledata.DisplayName == "")
                    //            {
                    //                hh1.username = peopledata.PeopleFirstName;
                    //                hh1.Description = " (Issued AssignmentId : " + o.DeliveryIssuanceIdOrderDeliveryMaster + ") By" + peopledata.PeopleFirstName;
                    //            }
                    //            else
                    //            {
                    //                hh1.username = peopledata.DisplayName;
                    //                hh1.Description = " (Issued AssignmentId : " + o.DeliveryIssuanceIdOrderDeliveryMaster + ") By" + peopledata.DisplayName;
                    //            }
                    //            hh1.DeliveryIssuanceId = o.DeliveryIssuanceIdOrderDeliveryMaster;
                    //            hh1.userid = userid;
                    //            hh1.CreatedDate = DateTime.Now;
                    //            context.OrderMasterHistoriesDB.Add(hh1);

                    //        }
                    //        #endregion
                    //    }
                    //    foreach (var o in orderMLists)
                    //    {
                    //        o.OldStatus = o.Status;
                    //        o.Status = "Issued";
                    //        o.UpdatedDate = DateTime.Now;
                    //    }

                    //    //Stock code
                    //    MultiStockHelper<OnIssuedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnIssuedStockEntryDc>();
                    //    List<OnIssuedStockEntryDc> OnIssuedStockEntryList = new List<OnIssuedStockEntryDc>();
                    //    foreach (var oder in orderDMLists.Where(x => !ROrderids.Contains(x.OrderId)).ToList())
                    //    {
                    //        foreach (var StockHit in oder.orderDetails.Where(x => x.qty > 0))
                    //        {
                    //            var orderm = orderMLists.FirstOrDefault(x => x.OrderId == StockHit.OrderId);
                    //            var RefStockCode = (orderm.OrderType == 8) ? "CL" : "C";

                    //            bool isFree = orderm.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                    //            if (isFree) { RefStockCode = "F"; }
                    //            //else if (orderm.OrderType == 6) //6 Damage stock
                    //            //{
                    //            //    RefStockCode = "D";
                    //            //}
                    //            //else if (orderm.OrderType == 9) //6 Non Sellable stock
                    //            //{
                    //            //    RefStockCode = "N";
                    //            //}

                    //            bool IsDeliveryRedispatch = false;
                    //            if (orderm.OldStatus == "Delivery Redispatch")
                    //            {
                    //                IsDeliveryRedispatch = true;
                    //            }

                    //            OnIssuedStockEntryList.Add(new OnIssuedStockEntryDc
                    //            {
                    //                ItemMultiMRPId = StockHit.ItemMultiMRPId,
                    //                OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                    //                OrderId = StockHit.OrderId,
                    //                Qty = StockHit.qty,
                    //                UserId = peopledata.PeopleID,
                    //                WarehouseId = StockHit.WarehouseId,
                    //                IsDeliveryRedispatch = IsDeliveryRedispatch,
                    //                RefStockCode = RefStockCode,
                    //            });
                    //        }
                    //    }
                    //    if (OnIssuedStockEntryList.Any() && !IsReturn)
                    //    {
                    //        bool res = MultiStockHelpers.MakeEntry(OnIssuedStockEntryList, "Stock_OnIssued", context, scope);
                    //        if (!res)
                    //        {
                    //            msg = new ResponceMsg()
                    //            {
                    //                Status = false,
                    //                Message = "Can't Dispatched, Something went wrong"
                    //            };
                    //            return msg;
                    //        }
                    //    }
                    //    foreach (var item in orderDMLists)
                    //    {
                    //        context.Entry(item).State = EntityState.Modified;
                    //    }
                    //    foreach (var item in orderMLists)
                    //    {
                    //        context.Entry(item).State = EntityState.Modified;
                    //    }
                    //    context.Entry(obj).State = EntityState.Modified;
                    //    context.Commit();
                    //}
                    msg = new ResponceMsg()
                    {
                        Status = true,
                        Message = "Assignment Finalize successfully !!"
                    };
                    return msg;
                }
                else
                {
                    msg = new ResponceMsg()
                    {
                        Status = false,
                        Message = "OrderId Not Found!!"
                    };

                    return msg;
                }
            }
            else
            {
                msg = new ResponceMsg()
                {
                    Status = false,
                    Message = "Already trip running and new trip created after end trip!!"
                };
                return msg;
            }
        }

        public bool checkCurrentTripStatusForCityAndDamage(long? dboyId, AuthContext context)
        {
            string query = " select  CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT) from ZilaTripMasters tpm join ZilaTripVehicles tpv on tpm.Id = tpv.ZilaTripMasterId where tpm.DboyId = '" + dboyId + "' and EndTime is null and CurrentStatus not in (6, 7) and tpm.IsActive =1 and tpm.IsDeleted = 0 and tpv.IsActive = 1 and tpv.IsDeleted =0 ";
            bool IsPending = context.Database.SqlQuery<bool>(query).First();
            return IsPending;
        }

        public bool InsertTripDistance(ZilaTripVehicle zilaPlannerVehicle, int userId, AuthContext authContext)
        {
            bool res = false;
            var tripPlanner = authContext.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == zilaPlannerVehicle.ZilaTripMasterId).FirstOrDefault();
            if (zilaPlannerVehicle != null && tripPlanner == null)
            {
                ZilaTripVehicle add = new ZilaTripVehicle();
                add.ZilaTripMasterId = zilaPlannerVehicle.ZilaTripMasterId;
                add.CurrentStatus = (int)VehicleliveStatus.NotStarted;
                add.TravelTime = zilaPlannerVehicle.TravelTime;
                add.ReminingTime = zilaPlannerVehicle.ReminingTime;
                add.DistanceTraveled = zilaPlannerVehicle.DistanceTraveled;
                add.DistanceLeft = zilaPlannerVehicle.DistanceLeft;
                add.CreatedDate = DateTime.Now;
                add.CreatedBy = userId;
                add.IsActive = true;
                add.IsDeleted = false;
                add.StartKm = zilaPlannerVehicle.StartKm;
                add.ReportingTime = zilaPlannerVehicle.ReportingTime;
                add.PenaltyCharge = zilaPlannerVehicle.PenaltyCharge;
                add.LateReportingTimeInMins = zilaPlannerVehicle.LateReportingTimeInMins;
                authContext.ZilaTripVehicles.Add(add);
                res = authContext.Commit() > 0;
            }
            return res;
        }

        public bool IsPickerFinalzied(long tripPlannerConfirmedMasterId, AuthContext context)
        {
            var tripPlannerConfirmedMasteridParam = new SqlParameter("@ZilaTripMasterId", tripPlannerConfirmedMasterId);
            int? notFinalizedCount = context.Database.SqlQuery<int?>("Zila.Zila_IsPickerFinalzied @ZilaTripMasterId", tripPlannerConfirmedMasteridParam).FirstOrDefault();
            if (notFinalizedCount > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool GetIsTripRunning(long VehicleMasterId, long zilaTripMasterId, AuthContext context)
        {
            var VehicleMasterIdParam = new SqlParameter("@VehicleMasterId", VehicleMasterId);
            var zilaTripMasterIdParam = new SqlParameter("@ZilaTripMasterId", zilaTripMasterId);
            var RunningTrip = context.Database.SqlQuery<bool>("Zila.Zila_IsTripRunning @VehicleMasterId,@ZilaTripMasterId", VehicleMasterIdParam, zilaTripMasterIdParam).FirstOrDefault();
            return RunningTrip;
        }

        public List<TripBlockedOrderVM> GetBlockedOrderList(long zilaTripMasterId, AuthContext authContext)
        {
            var tripPlannerConfirmedMasterid = new SqlParameter("@ZilaTripMasterId", zilaTripMasterId);
            var orderList = authContext.Database.SqlQuery<TripBlockedOrderVM>("Zila.Zila_ChekcedBlockedOrderWithCondition  @ZilaTripMasterId", tripPlannerConfirmedMasterid).ToList();
            return orderList;
        }

        public void UpdateTripSequenceNew(long zilaTripMasterId, AuthContext authContext, bool isOptimizeRoute = true)
        {
            List<SqlParameter> paramList = new List<SqlParameter>();
            paramList.Add(new SqlParameter("@ZilaTripMasterId", zilaTripMasterId));
            string spNameWithParam = "EXEC [Zila].[Zila_WayPointGet] @ZilaTripMasterId";

            List<TripWayPoint> tripWayPointEntireList = authContext.Database.SqlQuery<TripWayPoint>(spNameWithParam, paramList.ToArray()).ToList();
            if (tripWayPointEntireList != null && tripWayPointEntireList.Count > 1)
            {
                tripWayPointEntireList.Insert(0, tripWayPointEntireList[tripWayPointEntireList.Count - 1]);
                RouteOptimizeHelper routeOptimizeHelper = new RouteOptimizeHelper();
                var result = routeOptimizeHelper.OptimizeRouteUsingAzure(tripWayPointEntireList, isOptimizeRoute);

                int index = 0;
                string latLng = "";
                if (result != null && result.optimizedWaypoints != null && result.optimizedWaypoints.Count > 0)
                {
                    for (index = 0; index < result.optimizedWaypoints.Count; index++)
                    {
                        int pointIndex = result.optimizedWaypoints[index].optimizedIndex;
                        var tripPoint = tripWayPointEntireList[pointIndex + 1];
                        long unloadTime = tripWayPointEntireList[pointIndex + 1].AllOrderUnloadTimeInMins;

                        var pointSummary = result.routes[0].legs[index].summary;
                        ZilaTripDetail detail = authContext.ZilaTripDetails.First(x => x.Id == tripPoint.TripPlannerConfirmedDetailId);
                        detail.TotalDistanceInMeter = pointSummary.lengthInMeters;
                        detail.TotalTimeInMins = unloadTime + (long)(pointSummary.travelTimeInSeconds / 60);
                        detail.SequenceNo = index + 1;
                        latLng += detail.Lat + "," + detail.Lng + "|";
                    }

                    var tripPointW = tripWayPointEntireList[0];
                    var pointSummaryW = result.routes[0].legs[index].summary;
                    ZilaTripDetail detailW = authContext.ZilaTripDetails.First(x => x.Id == tripPointW.TripPlannerConfirmedDetailId);
                    detailW.TotalDistanceInMeter = pointSummaryW.lengthInMeters;
                    detailW.TotalTimeInMins = (long)(pointSummaryW.travelTimeInSeconds / 60);
                    latLng += detailW.Lat + "," + detailW.Lng + "|";
                }
                authContext.Commit();
                UpdateTripPlannerConfirmedMaster(zilaTripMasterId, authContext);
                authContext.Commit();
            }
        }

        public int UpdateTripPlannerConfirmedMaster(long zilaTripMasterId, AuthContext authContext)
        {
            var tripPlannerConfirmedMasterIdParam = new SqlParameter
            {
                ParameterName = "@ZilaTripMasterId",
                Value = zilaTripMasterId
            };
            int result = authContext.Database.ExecuteSqlCommand("[Zila].[Zila_UpdateTripPlannerConfirmedMaster] @ZilaTripMasterId", tripPlannerConfirmedMasterIdParam);
            return result;
        }

        public bool UpdateAmount(long zilaTripMasterId, AuthContext context)
        {
            var zilaTripMasterIdParam = new SqlParameter("@ZilaTripMasterId", zilaTripMasterId);
            int notFinalizedCount = context.Database.ExecuteSqlCommand("[Zila].[Zila_UpdateAmount] @ZilaTripMasterId", zilaTripMasterIdParam);
            context.Commit();
            return true;
        }

        public bool TripChangeDBoy(long ZilaTripMasterId, long ChangeDboyId, AuthContext context, int userid, string username)
        {
            bool result = false;
            var changeDboyId = new SqlParameter("@ChangeDboyId", ChangeDboyId);
            var data = context.Database.SqlQuery<TripChangeBoyDC>("exec Operation.TripPlanner_TripChangeDboy @ChangeDboyId", changeDboyId).FirstOrDefault();
            if (data != null)
            {
                var zilaTripMasters = context.ZilaTripMasters.Where(x => x.Id == ZilaTripMasterId).FirstOrDefault();
                var deliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.ZilaTripMasterId == zilaTripMasters.Id && x.AssignmentType == 5).ToList();


                var deliveryIssuanceIds = deliveryIssuance.Select(x => x.DeliveryIssuanceId).ToList();

                var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => deliveryIssuanceIds.Contains(x.DeliveryIssuanceIdOrderDeliveryMaster ?? 0)).ToList();
                var orderDeliveryMaster = context.OrderDeliveryMasterDB.Where(x => deliveryIssuanceIds.Contains(x.DeliveryIssuanceId)).ToList();

                var Pickerids = context.TripPickerAssignmentMapping.Where(c => deliveryIssuanceIds.Contains(c.AssignmentId ?? 0) && c.IsDeleted == false).Select(c => c.OrderPickerMasterId).Distinct().ToList();
                var orderPickerMasters = context.OrderPickerMasterDb.Where(x => Pickerids.Contains(x.Id)).ToList();
                if (zilaTripMasters != null
                    && deliveryIssuance != null && deliveryIssuance.Any()
                    && orderDispatchedMasters != null && orderDispatchedMasters.Any()
                    )
                {
                    zilaTripMasters.DboyId = data.DboyMasterId;
                    context.Entry(zilaTripMasters).State = EntityState.Modified;

                    foreach (var item in deliveryIssuance)
                    {
                        item.PeopleID = data.PeopleID;
                        item.DisplayName = data.Dboyname;
                        context.Entry(item).State = EntityState.Modified;
                    }
                    foreach (var order in orderDispatchedMasters)
                    {
                        OrderMasterHistories h1 = new OrderMasterHistories();
                        h1.orderid = order.OrderId;
                        h1.Status = order.Status;
                        h1.Reasoncancel = "Due to Change in delivery Boy from " + order.DboyName + " " + order.DboyMobileNo + " To " + data.Dboyname + " " + data.DboyMobile + " " + "by Trip";
                        h1.Warehousename = order.WarehouseName;
                        h1.username = username;
                        h1.userid = userid;
                        h1.CreatedDate = DateTime.Now;
                        context.OrderMasterHistoriesDB.Add(h1);
                        order.DBoyId = data.PeopleID;
                        order.DboyName = data.Dboyname;
                        order.DboyMobileNo = data.DboyMobile;

                        context.Entry(order).State = EntityState.Modified;
                    }
                    orderDeliveryMaster.ForEach(x =>
                    {
                        x.DboyName = data.Dboyname;
                        x.DboyMobileNo = data.DboyMobile;
                        context.Entry(x).State = EntityState.Modified;
                    });
                    if (orderPickerMasters != null && orderPickerMasters.Any())
                    {
                        foreach (var picker in orderPickerMasters)
                        {
                            picker.DBoyId = data.PeopleID;
                            context.Entry(picker).State = EntityState.Modified;
                        }
                    }
                    context.Commit();
                    result = true;
                }
            }
            return result;
        }

        public bool SetTripDboyVisibiltyStatus(long ZilaTripMasterId, AuthContext context)
        {
            bool status = false;
            var zilTripMasters = context.ZilaTripMasters.FirstOrDefault(x => x.Id == ZilaTripMasterId);
            var zilaTripMasteridParam = new SqlParameter("@ZilaTripMasterId", ZilaTripMasterId);
            var result = context.Database.SqlQuery<int?>("[Zila].[Zila_CheckTripDboyVisibiltyStatus] @ZilaTripMasterId", zilaTripMasteridParam).FirstOrDefault();
            if (result > 0)
            {
                //zilTripMasters.IsVisibleToDboy = false;
                zilTripMasters.ModifiedDate = DateTime.Now;
                context.Entry(zilTripMasters).State = EntityState.Modified;
                status = true;
            }
            else
            {
                //zilTripMasters.IsVisibleToDboy = true;
                zilTripMasters.ModifiedDate = DateTime.Now;
                context.Entry(zilTripMasters).State = EntityState.Modified;
                status = false;
            }
            //context.Commit();
            return status;
        }

        public ZilaTripMaster GetCurrentTripPlannerConfirmedMaster(long ZilaTripMasterId, AuthContext authContext)
        {
            var query = from p in authContext.DboyMasters
                        join cm in authContext.ZilaTripMasters on p.Id equals cm.DboyId
                        join tv in authContext.ZilaTripVehicles on cm.Id equals tv.ZilaTripMasterId
                        where cm.Id == ZilaTripMasterId && cm.IsActive == true && cm.IsDeleted == false && tv.CurrentStatus != 6
                        //&& cm.IsVisibleToDboy == true
                        select cm;
            var zilaMaster = query.FirstOrDefault(); //.OrderBy(x => x.TripDate).ThenBy(x => x.TripNumber).FirstOrDefault();
            return zilaMaster;
        }

        public dynamic GetTripPlannerConfirmedDetailId(long ZilaTripMasterId, AuthContext authContext)
        {
            //if (IsNotLastMileTrip)
            //{
                var zilaTripDetailId = authContext.ZilaTripDetails.Where(cm => cm.ZilaTripMasterId == ZilaTripMasterId
                && cm.IsActive == true && cm.IsDeleted == false && cm.IsProcess == false && (cm.OrderCount > 0 || cm.CustomerId == 0)).OrderByDescending(cm => cm.CustomerTripStatus).FirstOrDefault();
                return zilaTripDetailId;
            //}
            //else
            //{
            //    var zilaTripDetailId = authContext.ZilaTripDetails.Where(cm => cm.ZilaTripMasterId == ZilaTripMasterId
            //   && cm.IsActive == true && cm.IsDeleted == false && cm.IsProcess == false && (cm.OrderCount > 0 || cm.CustomerId == 0)).OrderBy(cm => cm.SequenceNo).FirstOrDefault();
            //    return zilaTripDetailId;
            //}
        }

        public void SetTripWorkingStatus(ZilaTripPlannerAppDashboardDC tripPlannerAppDashboard, ZilaTripVehicle tripPlannerVehicles)
        {
            try
            {
                if (true)
                {
                    if (tripPlannerAppDashboard.assignmentList.Any(x => x.AssignmentStatus == "Assigned"))
                    {
                        tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.AcceptAssignmentPending;
                    }
                    else if (tripPlannerAppDashboard.assignmentList.Count(x => x.AssignmentStatus == "Rejected") == tripPlannerAppDashboard.assignmentList.Count)
                    {
                        tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.EndTrip;
                    }
                    else
                    {
                        if (!tripPlannerVehicles.IsRearrangeDone)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.Rearrange;
                        }
                        else if (!tripPlannerVehicles.IsStartKmRequestSend)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.Start;
                        }
                        else if (tripPlannerVehicles.IsStartKmRequestSend && !tripPlannerVehicles.IsStartKmApproved)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.StartKmApprovalPending;
                        }
                        else if (tripPlannerVehicles.IsStartKmApproved && !tripPlannerAppDashboard.IsTripEnd)
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.InProgress;
                        }
                        //else if (tripPlannerVehicles.IsStartKmApproved && tripPlannerAppDashboard.IsTripEnd && !tripPlannerVehicles.IsCloseKmRequestSend)
                        //{
                        //    tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.SendCloseKmApproval;
                        //}
                        //else if (tripPlannerVehicles.IsStartKmApproved && tripPlannerAppDashboard.IsTripEnd && !tripPlannerVehicles.IsCloseKmApproved)
                        //{
                        //    tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.CloseKmApprovalPending;
                        //}
                        else
                        {
                            tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.EndTrip;
                        }
                    }
                } 
            }
            catch (Exception ex)
            {
                tripPlannerAppDashboard.TripWorkingStatus = (int)TripWorkingStatusEnum.IssueInTrip;
            }
        }

        public List<ZilaCustomerBriefDc> GetZilaCustomerInfoBrief(long zilaTripMasterId, AuthContext context)
        {
            var query = from d in context.ZilaTripDetails
                        join c in context.Customers on d.CustomerId equals c.CustomerId
                        join o in context.ZilaTripOrders on d.Id equals o.ZilaTripDetailId
                        join od in context.OrderDispatchedMasters on o.OrderId equals od.OrderId
                        where d.ZilaTripMasterId == zilaTripMasterId
                        && d.IsActive == true && d.IsDeleted == false
                        && o.IsActive == true && o.IsDeleted == false
                        group new { d, c, o, od } by new
                        {
                            BillingAddress = c.BillingAddress,
                            CustomerId = c.CustomerId,
                            GrossAmount = d.TotalAmount,
                            ShopName = c.ShopName,
                            Skcode = c.Skcode
                        } into g
                        select new ZilaCustomerBriefDc
                        {
                            customerInfo = new ZilaCustomerInfoBriefDc
                            {
                                BillingAddress = g.Key.BillingAddress,
                                CustomerId = g.Key.CustomerId,
                                GrossAmount = g.Key.GrossAmount,
                                ShopName = g.Key.ShopName,
                                Skcode = g.Key.Skcode
                            },
                            customerOrderInfo = g.Select(x => new ZilaCustomerOrderBriefInfo
                            {
                                DeliveryIssuanceId = x.od.DeliveryIssuanceIdOrderDeliveryMaster.HasValue ? x.od.DeliveryIssuanceIdOrderDeliveryMaster.Value : 0,
                                GrossAmount = x.o.Amount,
                                WorkingStatus = x.o.WorkingStatus,
                                OrderId = (int)x.o.OrderId,
                                ReDispatchCount = x.od.ReDispatchCount,
                                Status = x.od.Status,
                                WarehouseId = x.od.WarehouseId

                            }).ToList()


                        };


            List<ZilaCustomerBriefDc> brief = query.ToList();
            return brief;
        }

        public bool ZilaAllNotifyCustomerStartTrip(long zilaTripMasterId, AuthContext context)
        {
            var Customerids = context.ZilaTripDetails.Where(x => x.ZilaTripMasterId == zilaTripMasterId && x.IsActive == true && x.IsDeleted == false).Select(x => x.CustomerId).Distinct().ToList();
            var fcmids = context.Customers.Where(x => Customerids.Contains(x.CustomerId) && x.fcmId != null).Select(x => new { x.fcmId, x.CustomerId }).ToList();
            //send notification  => to Customer
            if (fcmids != null && fcmids.Any())
            {
                foreach (var item in fcmids)
                {
                    ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                    bool IsSendNotification = helper.TripNotifyAllCustomer(item.fcmId, item.CustomerId);
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool ZilaMakeEntryInVehicleHistory(double lat, double lng, long zilaTripMasterId, AuthContext context)
        {
            long vehicleId = context.ZilaTripVehicles.FirstOrDefault(x => x.ZilaTripMasterId == zilaTripMasterId).Id;
            VehicleHistoryVM vehicleHistory = new VehicleHistoryVM
            {
                CurrentServingOrderId = 0,
                Lat = lat,
                Lng = lng,
                RecordStatus = 5,//start trip
                RecordTime = DateTime.Now,
                TripPlannerConfirmedDetailId = 0,
                TripPlannerVehicleId = vehicleId
            };

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            string url = ConfigurationManager.AppSettings["SecondaryApiName"].ToString();

            var res = client.PostAsJsonAsync(url + "/api/VehicleHistory/InsertFirestoreData", vehicleHistory).Result;
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}