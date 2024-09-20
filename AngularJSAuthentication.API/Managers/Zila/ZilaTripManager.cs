using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.Zila;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.DataContracts.Transaction.Zila.OperationCapacity;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.TripPlanner;
using AngularJSAuthentication.Model.Zila.OperationCapacity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.Managers.Zila
{
    public class ZilaTripManager
    {
        public ZilaResponceDc ZilaStartAssignment(ZilaAssignmentStartEndDc assignmentStartEndDc, int userId)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            bool isSKFixVehicle = false;
            string notify_type = "Last Mile Request Approval Notification";
            ConfigureNotifyHelper configureNotifyHelper = new ConfigureNotifyHelper();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                ZilaResponceDc res;
                using (var authContext = new AuthContext())
                {
                    ZilaTripPlannerHelper zilaTripPlannerHelper = new ZilaTripPlannerHelper();
                    zilaTripPlannerHelper.ZilaAllNotifyCustomerStartTrip(assignmentStartEndDc.ZilaTripMasterId, authContext);
                    var tripPlannerVehicle = authContext.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == assignmentStartEndDc.ZilaTripMasterId).FirstOrDefault();
                    var deliveryIssuances = authContext.DeliveryIssuanceDb.Where(x => x.ZilaTripMasterId == assignmentStartEndDc.ZilaTripMasterId && x.Status == "Accepted").ToList();
                    var zilaTripMaster = authContext.ZilaTripMasters.FirstOrDefault(x => x.Id == assignmentStartEndDc.ZilaTripMasterId);
                    string Msg = null;
                    if (zilaTripMaster != null)
                    {
                        var VehicleMasterId = new SqlParameter("@VehicleMasterId", zilaTripMaster.VehicleMasterId);
                        isSKFixVehicle = authContext.Database.SqlQuery<bool>("EXEC Operation.TripPlanner_GetCheckSKFIXTripVehicle @VehicleMasterId", VehicleMasterId).First();
                    }
                    if (deliveryIssuances.Any() && tripPlannerVehicle != null && deliveryIssuances != null)
                    {
                       
                        if (isSKFixVehicle)
                        {
                            Msg = deliveryIssuances.FirstOrDefault().DisplayName + " started his trip and need your approval";
                           // ZilaInsertTripPlannerApprovalRequest(authContext, assignmentStartEndDc.ZilaTripMasterId, tripPlannerVehicle, userId, TripPlannerApprovalRequestTypeConstants.StartKm, assignmentStartEndDc.StartKm, assignmentStartEndDc.StartKmUrl);                          
                        }
                        tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                        tripPlannerVehicle.CurrentLat = assignmentStartEndDc.CurrentLat;
                        tripPlannerVehicle.CurrentLng = assignmentStartEndDc.CurrentLng;
                        tripPlannerVehicle.StartTime = DateTime.Now;
                        tripPlannerVehicle.ModifiedBy = userId;
                        //tripPlannerVehicle.StartKm = assignmentStartEndDc.StartKm;
                        //tripPlannerVehicle.StartKmUrl = assignmentStartEndDc.StartKmUrl;
                        tripPlannerVehicle.IsStartKmRequestSend = true;
                        tripPlannerVehicle.ModifiedDate = DateTime.Now;
                        tripPlannerVehicle.IsStartKmApproved = true;
                        foreach (var DeliveryIssuance in deliveryIssuances)
                        {
                            DeliveryIssuance.Status = "Pending";
                            DeliveryIssuance.UpdatedDate = DateTime.Now;
                            #region  DeliveryHistory
                            OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                            AssginDeli.DeliveryIssuanceId = DeliveryIssuance.DeliveryIssuanceId;
                            AssginDeli.Cityid = DeliveryIssuance.Cityid;
                            AssginDeli.city = DeliveryIssuance.city;
                            AssginDeli.DisplayName = DeliveryIssuance.DisplayName;
                            AssginDeli.Status = DeliveryIssuance.Status;
                            AssginDeli.WarehouseId = DeliveryIssuance.WarehouseId;
                            AssginDeli.PeopleID = DeliveryIssuance.PeopleID;
                            AssginDeli.VehicleId = DeliveryIssuance.VehicleId;
                            AssginDeli.VehicleNumber = DeliveryIssuance.VehicleNumber;
                            AssginDeli.RejectReason = DeliveryIssuance.RejectReason;
                            AssginDeli.OrderdispatchIds = DeliveryIssuance.OrderdispatchIds;
                            AssginDeli.OrderIds = DeliveryIssuance.OrderIds;
                            AssginDeli.Acceptance = DeliveryIssuance.Acceptance;
                            AssginDeli.IsActive = DeliveryIssuance.IsActive;
                            AssginDeli.IdealTime = DeliveryIssuance.IdealTime;
                            AssginDeli.TravelDistance = DeliveryIssuance.TravelDistance;
                            AssginDeli.CreatedDate = DateTime.Now;
                            AssginDeli.UpdatedDate = DateTime.Now;
                            AssginDeli.userid = DeliveryIssuance.PeopleID;
                            AssginDeli.UpdatedBy = DeliveryIssuance.DisplayName;
                            authContext.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                            authContext.Entry(DeliveryIssuance).State = EntityState.Modified;
                            #endregion
                        }
                        authContext.Entry(tripPlannerVehicle).State = EntityState.Modified;
                     
                        if (isSKFixVehicle)
                        {
                            configureNotifyHelper.SendNotificationForSarthiTripApproval(Msg, assignmentStartEndDc.ZilaTripMasterId, notify_type, authContext);
                        }
                        authContext.Commit();
                        res = new ZilaResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = true,
                            Message = "start time record created"
                        };
                        scope.Complete();
                        return res;
                    }
                    else if (tripPlannerVehicle != null)
                    {
                      
                        if (isSKFixVehicle)
                        {
                            //ZilaInsertTripPlannerApprovalRequest(authContext, assignmentStartEndDc.ZilaTripMasterId, tripPlannerVehicle, userId, TripPlannerApprovalRequestTypeConstants.StartKm, assignmentStartEndDc.StartKm, assignmentStartEndDc.StartKmUrl);

                            //tripPlannerVehicle.StartKm = assignmentStartEndDc.StartKm;
                            //tripPlannerVehicle.StartKmUrl = assignmentStartEndDc.StartKmUrl;
                            tripPlannerVehicle.IsStartKmRequestSend = true;
                            tripPlannerVehicle.IsStartKmApproved = true;
                            tripPlannerVehicle.ModifiedDate = DateTime.Now;
                            authContext.Entry(tripPlannerVehicle).State = EntityState.Modified;
                            //notification KM Sarthi App 
                            Msg = "Started his trip and need your approval";
                            configureNotifyHelper.SendNotificationForSarthiTripApproval(Msg, assignmentStartEndDc.ZilaTripMasterId, notify_type, authContext);
                        }
                        authContext.Commit();
                        res = new ZilaResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = true,
                            Message = "start time record created"
                        };
                        scope.Complete();
                        return res;
                    }
                    zilaTripPlannerHelper.ZilaMakeEntryInVehicleHistory(assignmentStartEndDc.CurrentLat, assignmentStartEndDc.CurrentLng, assignmentStartEndDc.ZilaTripMasterId, authContext);
                    res = new ZilaResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = false,
                        Message = "start time not record created!!"
                    };
                    scope.Dispose();
                    return res;
                }
            }
        }

        public void ZilaInsertTripPlannerApprovalRequest(AuthContext context, long zilaTripMasterId, ZilaTripVehicle vehicle, int userId, string requestType, double meterReading, string readingUrl)
        {
            var oldRequest = context.TripPlannerApprovalRequestDb.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == zilaTripMasterId && x.IsActive == true && x.IsDeleted == false && x.IsRejected == false && x.IsApproved == false && x.RequestType == requestType);
            if (oldRequest == null)
            {
                TripPlannerApprovalRequest newRequest = new TripPlannerApprovalRequest
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsApproved = false,
                    IsDeleted = false,
                    IsRejected = false,
                    ModifiedBy = null,
                    ModifiedDate = null,    
                    ProcessedBy = null,
                    ProcessedDate = null,
                    TripPlannerConfirmedMasterId = zilaTripMasterId,
                    RequestType = requestType,
                    Reading = meterReading,
                    ReadingUrl = readingUrl,
                    IsZilaTrip = true
                };

                context.TripPlannerApprovalRequestDb.Add(newRequest);
            }
            else
            {
                oldRequest.CreatedBy = userId;
                oldRequest.ModifiedDate = DateTime.Now;
                oldRequest.ProcessedBy = null;
                oldRequest.ProcessedDate = null;
                oldRequest.Reading = meterReading;
                oldRequest.ReadingUrl = readingUrl;
                context.Entry(oldRequest).State = EntityState.Modified;
            }
            context.Commit();

            if (TripPlannerApprovalRequestTypeConstants.StartKm == requestType)
            {
                vehicle.IsStartKmRequestSend = true;
            }
            if (TripPlannerApprovalRequestTypeConstants.EndKm == requestType)
            {
                vehicle.IsCloseKmRequestSend = true;
            }
        }

        public ZilaResponceDc ZilaEndAssignment(ZilaAssignmentStartEndDc assignmentStartEndDc, int userId)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                ZilaResponceDc res;
                using (var authContext = new AuthContext())
                {
                    var deliveryIssuances = authContext.DeliveryIssuanceDb.Where(x => x.ZilaTripMasterId == assignmentStartEndDc.ZilaTripMasterId && x.Status != "Freezed" && x.Status != "Payment Submitted" && x.Status != "Payment Accepted").ToList();
                    var DeliveryIssuanceId = deliveryIssuances.Select(x => x.DeliveryIssuanceId).ToList();
                    string query = " SELECT CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT)FROM OrderDeliveryMasters  odm " +
                                    " join DeliveryIssuances di on odm.DeliveryIssuanceId = di.DeliveryIssuanceId where  di.DeliveryIssuanceId  in ('" + string.Join("','", DeliveryIssuanceId) + "') and odm.Status in ('Shipped', 'Issued','Delivery Canceled Request') and di.Status !='Rejected' ";
                    bool IsPending = authContext.Database.SqlQuery<bool>(query).First();
                    if (deliveryIssuances.Any() && deliveryIssuances != null)
                    {
                        if (!IsPending && deliveryIssuances.Any() && deliveryIssuances != null)
                        {
                            var zilaTripMasters = authContext.ZilaTripMasters.FirstOrDefault(x => x.Id == assignmentStartEndDc.ZilaTripMasterId);
                            var update = authContext.ZilaTripVehicles.Where(x => x.ZilaTripMasterId == assignmentStartEndDc.ZilaTripMasterId && (x.StartTime != null || !IsPending) && x.CurrentStatus != 6).FirstOrDefault();
                            if (zilaTripMasters != null && update != null)
                            {

                                //UpdateAttandance(zilaTripMasters.TripPlannerMasterId.Value, authContext, tripPlannerConfirmedMasters, update);
                                update.CurrentStatus = (int)VehicleliveStatus.TripEnd;
                                update.EndTime = DateTime.Now;
                                update.ModifiedBy = userId;
                                update.ModifiedDate = DateTime.Now;
                                update.ReminingTime = 0;
                                update.TravelTime = zilaTripMasters.TotalTimeInMins;
                                update.DistanceLeft = 0;
                                update.DistanceTraveled = zilaTripMasters.TotalDistanceInMeter;
                                //update.ClosingKm = assignmentStartEndDc.ClosingKm;
                                //update.ClosingKMUrl = assignmentStartEndDc.ClosingKMUrl;
                                //update.IsClosingKmManualReading = assignmentStartEndDc.IsClosingKmManualReading;
                                update.IsClosingKmManualReading = true;

                                foreach (var DeliveryIssuance in deliveryIssuances)
                                {
                                    if (DeliveryIssuance.Status != "Rejected")
                                    {
                                        DeliveryIssuance.Status = "Submitted";
                                    }
                                    DeliveryIssuance.UpdatedDate = DateTime.Now;
                                    authContext.Entry(DeliveryIssuance).State = EntityState.Modified;
                                    #region  DeliveryHistory
                                    OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                    AssginDeli.DeliveryIssuanceId = DeliveryIssuance.DeliveryIssuanceId;
                                    //AssginDeli.OrderId = delivery.o
                                    AssginDeli.Cityid = DeliveryIssuance.Cityid;
                                    AssginDeli.city = DeliveryIssuance.city;
                                    AssginDeli.DisplayName = DeliveryIssuance.DisplayName;
                                    AssginDeli.Status = DeliveryIssuance.Status;
                                    AssginDeli.WarehouseId = DeliveryIssuance.WarehouseId;
                                    AssginDeli.PeopleID = DeliveryIssuance.PeopleID;
                                    AssginDeli.VehicleId = DeliveryIssuance.VehicleId;
                                    AssginDeli.VehicleNumber = DeliveryIssuance.VehicleNumber;
                                    AssginDeli.RejectReason = DeliveryIssuance.RejectReason;
                                    AssginDeli.OrderdispatchIds = DeliveryIssuance.OrderdispatchIds;
                                    AssginDeli.OrderIds = DeliveryIssuance.OrderIds;
                                    AssginDeli.Acceptance = DeliveryIssuance.Acceptance;
                                    AssginDeli.IsActive = DeliveryIssuance.IsActive;
                                    AssginDeli.IdealTime = DeliveryIssuance.IdealTime;
                                    AssginDeli.TravelDistance = DeliveryIssuance.TravelDistance;
                                    AssginDeli.CreatedDate = DateTime.Now;
                                    AssginDeli.UpdatedDate = DateTime.Now;
                                    AssginDeli.userid = DeliveryIssuance.PeopleID;
                                    AssginDeli.UpdatedBy = DeliveryIssuance.DisplayName;
                                    authContext.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                    #endregion
                                }
                                authContext.Entry(update).State = EntityState.Modified;

                                authContext.Commit();
                                res = new ZilaResponceDc()
                                {
                                    TripDashboardDC = null,
                                    Status = true,
                                    Message = "End time record created"
                                };
                                scope.Complete();
                                return res;
                            }
                            else
                            {
                                res = new ZilaResponceDc()
                                {
                                    TripDashboardDC = null,
                                    Status = false,
                                    Message = "Something Went Wrong!!"
                                };
                            }
                        }
                        else
                        {
                            res = new ZilaResponceDc()
                            {
                                TripDashboardDC = null,
                                Status = false,
                                Message = "orders still in progress"
                            };
                        }
                    }
                    else
                    {
                        res = new ZilaResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = false,
                            Message = "This trip Assignment is already freezed"
                        };
                    }
                    scope.Dispose();
                    return res;
                }
            }
        }

        public List<TripMasterForDropDown> ZilaGetTripList(int warehouseId, bool isTripExistsInAssignment, int FilterType)
        {
            using (var authContext = new AuthContext())
            {
                string spNameWithParam = "EXEC [Zila].[Zila_TripGet] @WarehouseId, @IsTripExistsInAssignment, @filtertype";

                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@WarehouseId", warehouseId));

                paramList.Add(new SqlParameter("@IsTripExistsInAssignment", isTripExistsInAssignment));
                paramList.Add(new SqlParameter("@filtertype", FilterType));

                List<TripMasterForDropDown> list = authContext.Database.SqlQuery<TripMasterForDropDown>(spNameWithParam, paramList.ToArray()).ToList();
                return list;
            }
        }
    }
}