using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.BusinessLayer.Managers.TripPlanner;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.TripPlanner;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;

namespace AngularJSAuthentication.API.Managers.TripPlanner
{
    public class TripPlannerVehicleManager
    {        
        public bool InsertTripDistance(TripPlannerVehicles tripPlannerVehicle, int userId, AuthContext authContext)
        {
            bool res = false;
            var tripPlanner = authContext.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerVehicle.TripPlannerConfirmedMasterId).FirstOrDefault();
            if (tripPlannerVehicle != null && tripPlanner == null)
            {
                TripPlannerVehicles add = new TripPlannerVehicles();
                add.TripPlannerConfirmedMasterId = tripPlannerVehicle.TripPlannerConfirmedMasterId;
                add.CurrentStatus = (int)VehicleliveStatus.NotStarted;
                add.TravelTime = tripPlannerVehicle.TravelTime;
                add.ReminingTime = tripPlannerVehicle.ReminingTime;
                add.DistanceTraveled = tripPlannerVehicle.DistanceTraveled;
                add.DistanceLeft = tripPlannerVehicle.DistanceLeft;
                add.CreatedDate = DateTime.Now;
                add.CreatedBy = userId;
                add.IsActive = true;
                add.IsDeleted = false;
                add.StartKm = tripPlannerVehicle.StartKm;
                add.ReportingTime = tripPlannerVehicle.ReportingTime;
                add.PenaltyCharge = tripPlannerVehicle.PenaltyCharge;
                add.LateReportingTimeInMins = tripPlannerVehicle.LateReportingTimeInMins;
                authContext.TripPlannerVehicleDb.Add(add);
                res = authContext.Commit() > 0;

            }
            return res;
        }

        public ResponceDc StartAssignment(AssignmentStartEndDc assignmentStartEndDc, int userId)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            bool isSKFixVehicle = false;
            string notify_type = "Last Mile Request Approval Notification";
            ConfigureNotifyHelper configureNotifyHelper = new ConfigureNotifyHelper();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                ResponceDc res;
                using (var authContext = new AuthContext())
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    tripPlannerHelper.AllNotifyCustomerStartTrip(assignmentStartEndDc.TripPlannerConfirmedMasterId, authContext);
                    var tripPlannerVehicle = authContext.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == assignmentStartEndDc.TripPlannerConfirmedMasterId).FirstOrDefault();
                    var deliveryIssuances = authContext.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == assignmentStartEndDc.TripPlannerConfirmedMasterId && x.Status == "Accepted").ToList();
                    var tripPlannerConfirmedMaster = authContext.TripPlannerConfirmedMasters.FirstOrDefault(x => x.Id == assignmentStartEndDc.TripPlannerConfirmedMasterId);
                    string Msg = null;
                    if (tripPlannerConfirmedMaster != null)
                    {
                        var VehicleMasterId = new SqlParameter("@VehicleMasterId", tripPlannerConfirmedMaster.VehicleMasterId);
                        isSKFixVehicle = authContext.Database.SqlQuery<bool>("EXEC Operation.TripPlanner_GetCheckSKFIXTripVehicle @VehicleMasterId", VehicleMasterId).First();
                    }
                    if (deliveryIssuances.Any() && tripPlannerVehicle != null && deliveryIssuances != null)
                    {
                        //if (!tripPlannerConfirmedMaster.IsNotLastMileTrip)//SK FIX Vehical Work
                        //{
                        if (isSKFixVehicle)
                        {
                            Msg = deliveryIssuances.FirstOrDefault().DisplayName + " started his trip and need your approval";
                            InsertTripPlannerApprovalRequest(authContext, assignmentStartEndDc.TripPlannerConfirmedMasterId, tripPlannerVehicle, userId, TripPlannerApprovalRequestTypeConstants.StartKm, assignmentStartEndDc.StartKm, assignmentStartEndDc.StartKmUrl);
                            //}
                        }
                        tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                        tripPlannerVehicle.CurrentLat = assignmentStartEndDc.CurrentLat;
                        tripPlannerVehicle.CurrentLng = assignmentStartEndDc.CurrentLng;
                        tripPlannerVehicle.StartTime = DateTime.Now;
                        tripPlannerVehicle.ModifiedBy = userId;
                        tripPlannerVehicle.StartKm = assignmentStartEndDc.StartKm;
                        tripPlannerVehicle.StartKmUrl = assignmentStartEndDc.StartKmUrl;
                        tripPlannerVehicle.IsStartKmRequestSend = true;
                        tripPlannerVehicle.ModifiedDate = DateTime.Now;
                        foreach (var DeliveryIssuance in deliveryIssuances)
                        {
                            DeliveryIssuance.Status = "Pending";
                            DeliveryIssuance.UpdatedDate = DateTime.Now;
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
                            authContext.Entry(DeliveryIssuance).State = EntityState.Modified;
                            #endregion
                        }
                        authContext.Entry(tripPlannerVehicle).State = EntityState.Modified;
                        //notification KM Sarthi App 
                        //if (!tripPlannerConfirmedMaster.IsNotLastMileTrip)
                        //{
                        if (isSKFixVehicle)
                        {
                            configureNotifyHelper.SendNotificationForSarthiTripApproval(Msg, assignmentStartEndDc.TripPlannerConfirmedMasterId, notify_type, authContext);
                        }
                        authContext.Commit();
                        res = new ResponceDc()
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
                        //if (!tripPlannerConfirmedMaster.IsNotLastMileTrip)
                        //{
                        if (isSKFixVehicle)
                        {
                            InsertTripPlannerApprovalRequest(authContext, assignmentStartEndDc.TripPlannerConfirmedMasterId, tripPlannerVehicle, userId, TripPlannerApprovalRequestTypeConstants.StartKm, assignmentStartEndDc.StartKm, assignmentStartEndDc.StartKmUrl);

                            tripPlannerVehicle.StartKm = assignmentStartEndDc.StartKm;
                            tripPlannerVehicle.StartKmUrl = assignmentStartEndDc.StartKmUrl;
                            tripPlannerVehicle.IsStartKmRequestSend = true;
                            tripPlannerVehicle.ModifiedDate = DateTime.Now;
                            authContext.Entry(tripPlannerVehicle).State = EntityState.Modified;
                            //notification KM Sarthi App 
                            Msg = "Started his trip and need your approval";
                            configureNotifyHelper.SendNotificationForSarthiTripApproval(Msg, assignmentStartEndDc.TripPlannerConfirmedMasterId, notify_type, authContext);
                        }
                        authContext.Commit();
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = true,
                            Message = "start time record created"
                        };
                        scope.Complete();
                        return res;
                    }
                    tripPlannerHelper.MakeEntryInVehicleHistory(assignmentStartEndDc.CurrentLat, assignmentStartEndDc.CurrentLng, assignmentStartEndDc.TripPlannerConfirmedMasterId, authContext);
                    res = new ResponceDc()
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

        public ResponceDc EndAssignment(AssignmentStartEndDc assignmentStartEndDc, int userId)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                ResponceDc res;
                using (var authContext = new AuthContext())
                {
                    var deliveryIssuances = authContext.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == assignmentStartEndDc.TripPlannerConfirmedMasterId && x.Status != "Freezed" && x.Status != "Payment Submitted" && x.Status != "Payment Accepted").ToList();
                    var DeliveryIssuanceId = deliveryIssuances.Select(x => x.DeliveryIssuanceId).ToList();
                    string query = " SELECT CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT)FROM OrderDeliveryMasters  odm " +
                                    " join DeliveryIssuances di on odm.DeliveryIssuanceId = di.DeliveryIssuanceId where  di.DeliveryIssuanceId  in ('" + string.Join("','", DeliveryIssuanceId) + "') and odm.Status in ('Shipped', 'Issued','Delivery Canceled Request') and di.Status !='Rejected' ";
                    bool IsPending = authContext.Database.SqlQuery<bool>(query).First();
                    if (deliveryIssuances.Any() && deliveryIssuances != null)
                    {
                        if (!IsPending && deliveryIssuances.Any() && deliveryIssuances != null)
                        {
                            // var tripPlannerConfirmedDetails = authContext.TripPlannerConfirmedDetails.Where(x => x.TripPlannerConfirmedMasterId == assignmentStartEndDc.TripPlannerConfirmedMasterId && x.IsProcess == false && x.OrderCount == 0 && x.CustomerId == 0).FirstOrDefault();
                            var tripPlannerConfirmedMasters = authContext.TripPlannerConfirmedMasters.FirstOrDefault(x => x.Id == assignmentStartEndDc.TripPlannerConfirmedMasterId);
                            var update = authContext.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == assignmentStartEndDc.TripPlannerConfirmedMasterId && (x.StartTime != null || !IsPending) && x.CurrentStatus != 6).FirstOrDefault();
                            if (tripPlannerConfirmedMasters != null && update != null)
                            {

                                UpdateAttandance(tripPlannerConfirmedMasters.TripPlannerMasterId.Value, authContext, tripPlannerConfirmedMasters, update);
                                update.CurrentStatus = (int)VehicleliveStatus.TripEnd;
                                update.EndTime = DateTime.Now;
                                update.ModifiedBy = userId;
                                update.ModifiedDate = DateTime.Now;
                                update.ReminingTime = 0;
                                update.TravelTime = tripPlannerConfirmedMasters.TotalTimeInMins;
                                update.DistanceLeft = 0;
                                update.DistanceTraveled = tripPlannerConfirmedMasters.TotalDistanceInMeter;
                                //update.ClosingKm = assignmentStartEndDc.ClosingKm;
                                //update.ClosingKMUrl = assignmentStartEndDc.ClosingKMUrl;
                                update.IsClosingKmManualReading = assignmentStartEndDc.IsClosingKmManualReading;

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
                                res = new ResponceDc()
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
                                res = new ResponceDc()
                                {
                                    TripDashboardDC = null,
                                    Status = false,
                                    Message = "Something Went Wrong!!"
                                };
                            }
                        }
                        else
                        {
                            res = new ResponceDc()
                            {
                                TripDashboardDC = null,
                                Status = false,
                                Message = "orders still in progress"
                            };
                        }
                    }
                    else
                    {
                        res = new ResponceDc()
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
        public bool checkCurrentTripStatus(long? dboyId, AuthContext context)
        {
            string query = " select  CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT) from TripPlannerConfirmedMasters tpm join TripPlannerVehicles tpv on tpm.Id = tpv.TripPlannerConfirmedMasterId where tpm.DboyId = '" + dboyId + "' and EndTime is null and CurrentStatus not in (6, 7)  and tpm.TripTypeEnum not in (0,3) ";
            bool IsPending = context.Database.SqlQuery<bool>(query).First();
            return IsPending;
        }
        public bool checkCurrentTripStatusForCityAndDamage(long? dboyId, AuthContext context)
        {
            string query = " select  CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT) from TripPlannerConfirmedMasters tpm join TripPlannerVehicles tpv on tpm.Id = tpv.TripPlannerConfirmedMasterId where tpm.DboyId = '" + dboyId + "' and EndTime is null and CurrentStatus not in (6, 7) and tpm.TripTypeEnum in (0,3)  ";
            bool IsPending = context.Database.SqlQuery<bool>(query).First();
            return IsPending;
        }

        public void InsertTripPlannerApprovalRequest(AuthContext context, long tripPlannedConfirmedMasterId, TripPlannerVehicles vehicle, int userId, string requestType, double meterReading, string readingUrl)
        {
            var oldRequest = context.TripPlannerApprovalRequestDb.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == tripPlannedConfirmedMasterId && x.IsActive == true && x.IsDeleted == false && x.IsRejected == false && x.IsApproved == false && x.RequestType == requestType);
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
                    TripPlannerConfirmedMasterId = tripPlannedConfirmedMasterId,
                    RequestType = requestType,
                    Reading = meterReading,
                    ReadingUrl = readingUrl
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

        public StatusDc UpdateAttandance(long TripPlannerMasterId, AuthContext authContext, TripPlannerConfirmedMaster tripPlannerConfirmesMaster, TripPlannerVehicles tripPlannerVehicle)
        {

            StatusDc res;
            string spName = "UpdateTripPlannerVehicle @TripPlannerConfirmedMasterId";

            var startTime = authContext.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmesMaster.Id).FirstOrDefault();
            var stime = startTime.StartTime.Value.Date;
            var updateAttandance = authContext.TripPlannerVechicleAttandanceDb.Where(x => x.VehicleMasterId == tripPlannerConfirmesMaster.VehicleMasterId && x.AttendanceDate == stime && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

            var Param = new SqlParameter
            {
                ParameterName = "TripPlannerConfirmedMasterId",
                Value = tripPlannerConfirmesMaster.Id
            };

            var TripPlannerConfirmData = authContext.Database.SqlQuery<TripPlannerConfirmedDc>(spName, Param).FirstOrDefault();

            string spName1 = "Operation.TripPlanner_GetDBoyCost @TripPlannerConfirmMasterId";
            var Param1 = new SqlParameter
            {
                ParameterName = "TripPlannerConfirmMasterId",
                Value = tripPlannerConfirmesMaster.Id
            };

            var TripPlannerDboyCost = authContext.Database.SqlQuery<double>(spName1, Param1).FirstOrDefault();

            if (tripPlannerConfirmesMaster != null && updateAttandance != null)
            {
                updateAttandance.TouchPoints += tripPlannerConfirmesMaster.CustomerCount;
                updateAttandance.NoOfOrder += tripPlannerConfirmesMaster.OrderCount;
                updateAttandance.OrderAmount += (int)tripPlannerConfirmesMaster.TotalAmount;
                updateAttandance.PlannedKM = tripPlannerConfirmesMaster.TotalDistanceInMeter / 1000;
                updateAttandance.TotalWeightCarry += tripPlannerConfirmesMaster.TotalWeight;
                updateAttandance.KMTravelled += (double)((tripPlannerVehicle.ClosingKm != null ? tripPlannerVehicle.ClosingKm : 0) - (tripPlannerVehicle.StartKm != null ? tripPlannerVehicle.StartKm : 0));
                updateAttandance.DboyCost += TripPlannerDboyCost;

                if(tripPlannerConfirmesMaster.VehicleFare >0)
                {
                    updateAttandance.Fare = tripPlannerConfirmesMaster.VehicleFare;
                }

                if (TripPlannerConfirmData != null)
                {
                    updateAttandance.TotalWeightDelivered += TripPlannerConfirmData.TotalWeightDelivered;
                    updateAttandance.TouchPointVisited += TripPlannerConfirmData.TouchPointVisited;
                    updateAttandance.NoOfOrderDelivered += TripPlannerConfirmData.NoOfOrderDelivered;
                    updateAttandance.OrderAmountDelivered += TripPlannerConfirmData.OrderAmountDelivered;


                    updateAttandance.VisitedNoOfOrder += TripPlannerConfirmData.VisitedNoOfOrder;
                    updateAttandance.VisitedOrderAmount += TripPlannerConfirmData.VisitedOrderAmount;
                    updateAttandance.VisitedTotalWeight += TripPlannerConfirmData.VisitedTotalWeight;
                }

                authContext.Entry(updateAttandance).State = EntityState.Modified;
                authContext.Commit();

                res = new StatusDc()
                {
                    Status = true,
                    Message = "Updated Attandance"
                };
                return res;
            }
            else
            {
                res = new StatusDc()
                {
                    Status = true,
                    Message = "Attandance not Updated"
                };
            }
            return res;


        }

        public StatusDc UpdateTodayAttandance(int WarehouseId)
        {
            StatusDc res;

            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var authContext = new AuthContext())
            {
                List<TripPlannerVechicleAttandance> VehicleAttandance = new List<TripPlannerVechicleAttandance>();
                DateTime YesterdayDate = DateTime.Now.AddDays(-1);
                VehicleAttandance = authContext.TripPlannerVechicleAttandanceDb.Where(x => x.WarehouseId == WarehouseId && EntityFunctions.TruncateTime
                (x.AttendanceDate) == EntityFunctions.TruncateTime(YesterdayDate) && x.IsActive == true && x.IsDeleted == false).ToList();

                if (VehicleAttandance.Count > 0)
                {
                    foreach (var list in VehicleAttandance)
                    {
                        var existToday = authContext.TripPlannerVechicleAttandanceDb.Where(x => x.VehicleMasterId == list.VehicleMasterId && x.WarehouseId == WarehouseId && EntityFunctions.TruncateTime(x.AttendanceDate) == EntityFunctions.TruncateTime(DateTime.Today) && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (existToday != null && existToday.Count > 0)
                        {
                            //do  nothing  
                        }
                        else
                        {

                            TripPlannerVechicleAttandanceManager oneDayFareAmt = new TripPlannerVechicleAttandanceManager();
                            double amt = oneDayFareAmt.GetOneDayFareAmount(list.VehicleMasterId);
                            TripPlannerVechicleAttandance AttandanceList = new TripPlannerVechicleAttandance
                            {
                                CreatedBy = userid,
                                AttendanceDate = DateTime.Today,
                                CreatedDate = DateTime.Now,
                                Fare = amt,
                                IsActive = true,
                                KMTravelled = 0,
                                IsDeleted = false,
                                ModifiedBy = null,
                                ModifiedDate = null,
                                NoOfOrder = 0,
                                NoOfOrderDelivered = 0,
                                OrderAmount = 0,
                                OrderAmountDelivered = 0,
                                PlannedKM = 0,
                                TollFare = 0,
                                TotalWeightCarry = 0,
                                TotalWeightDelivered = 0,
                                TouchPoints = 0,
                                TouchPointVisited = 0,
                                VehicleMasterId = list.VehicleMasterId,
                                WarehouseId = list.WarehouseId

                            };

                            authContext.TripPlannerVechicleAttandanceDb.Add(AttandanceList);

                        }

                    }
                    authContext.Commit();
                    res = new StatusDc()
                    {

                        Status = true,
                        Message = "AttandanceList Added Successfully!!"
                    };
                    return res;
                }
                else
                {
                    res = new StatusDc()
                    {

                        Status = false,
                        Message = "Something went Wrong!!"
                    };
                    return res;
                }
            }
        }

        public StatusDc ActiveStatuswiseAttandance(List<ActiveStatuswiseAttandanceDc> activeStatuswisAttandanceList, int userid)
        {
            StatusDc res;
            //TransactionOptions option = new TransactionOptions();
            //option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            //option.Timeout = TimeSpan.FromSeconds(120);

            //int userid = 0;
            //var identity = User.Identity as ClaimsIdentity;
            //if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
            //    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            //using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            //{
            using (var authContext = new AuthContext())
            {
                if (activeStatuswisAttandanceList != null && activeStatuswisAttandanceList.Any())
                {
                    var deletedAttandanceList = activeStatuswisAttandanceList.Where(x => x.Id > 0 && x.IsActive == false).ToList();
                    if (deletedAttandanceList != null && deletedAttandanceList.Any())
                    {
                        foreach (var item in deletedAttandanceList)
                        {
                            var attandance = authContext.TripPlannerVechicleAttandanceDb.First(x => x.Id == item.Id);

                            attandance.IsActive = false;
                            attandance.IsDeleted = true;
                            authContext.Entry(attandance).State = EntityState.Modified;
                        }
                    }

                    var newAttandanceList = activeStatuswisAttandanceList.Where(x => x.Id == 0 && x.IsActive == true).ToList();
                    if (newAttandanceList != null && newAttandanceList.Any())
                    {
                        var vehicleMasterId = newAttandanceList.First().VehicleMasterId;
                        TripPlannerVechicleAttandanceManager oneDayFareAmt = new TripPlannerVechicleAttandanceManager();
                        double amt = oneDayFareAmt.GetOneDayFareAmount(vehicleMasterId);
                        foreach (var item in newAttandanceList)
                        {
                            if (authContext.TripPlannerVechicleAttandanceDb.Any(x => x.VehicleMasterId == item.VehicleMasterId && x.AttendanceDate == item.AttendanceDate && x.IsActive == true && x.IsDeleted == false))
                            {
                                //do nothing
                            }
                            else
                            {

                                TripPlannerVechicleAttandance AttandanceList = new TripPlannerVechicleAttandance
                                {
                                    CreatedBy = userid,
                                    AttendanceDate = item.AttendanceDate,
                                    CreatedDate = DateTime.Now,
                                    Fare = amt,
                                    IsActive = true,
                                    KMTravelled = 0,
                                    IsDeleted = false,
                                    ModifiedBy = null,
                                    ModifiedDate = null,
                                    NoOfOrder = 0,
                                    NoOfOrderDelivered = 0,
                                    OrderAmount = 0,
                                    OrderAmountDelivered = 0,
                                    PlannedKM = 0,
                                    TollFare = 0,
                                    TotalWeightCarry = 0,
                                    TotalWeightDelivered = 0,
                                    TouchPoints = 0,
                                    TouchPointVisited = 0,
                                    VehicleMasterId = item.VehicleMasterId,
                                    WarehouseId = item.WarehouseId

                                };

                                authContext.TripPlannerVechicleAttandanceDb.Add(AttandanceList);
                            }
                        }
                    }
                }

                authContext.Commit();
                // dbContextTransaction.Complete();

                return new StatusDc
                {
                    Message = "Success",
                    Status = true
                };
            }
            // }
        }
        public List<ActiveStatuswiseAttandanceDc> FutureAttandanceData(long VehicleMasterId, int WarehouseId, int month, int year)
        {
            List<ActiveStatuswiseAttandanceDc> res = new List<ActiveStatuswiseAttandanceDc>();

            using (var authContext = new AuthContext())
            {
                List<AttandanceListDc> attandanceListDcs = new List<AttandanceListDc>();
                var todayDate = DateTime.Today;
                var Futurelist = authContext.TripPlannerVechicleAttandanceDb.Where(x => x.VehicleMasterId == VehicleMasterId && x.WarehouseId == WarehouseId && x.IsActive == true && x.IsDeleted == false && x.AttendanceDate.Month == month && x.AttendanceDate.Year == year /* && x.AttendanceDate > todayDate*/).ToList();
                if (Futurelist != null)
                {
                    foreach (var item in Futurelist)
                    {
                        res.Add(new ActiveStatuswiseAttandanceDc
                        {
                            AttendanceDate = item.AttendanceDate,
                            Id = item.Id,
                            IsActive = item.IsActive,
                            VehicleMasterId = item.VehicleMasterId,
                            WarehouseId = item.WarehouseId
                        });
                    }
                    res.OrderByDescending(x => x.AttendanceDate);
                }
            }

            return res;
        }


    }
}