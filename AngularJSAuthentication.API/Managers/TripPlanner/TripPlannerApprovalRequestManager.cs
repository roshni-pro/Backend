using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers.NotificationApprovalMatrix;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace AngularJSAuthentication.API.Managers.TripPlanner
{
    public class TripPlannerApprovalRequestManager
    {
        public List<TripPlannerApprovalRequestDc> GetList(TripPlannerApprovalRequestInputDc input)
        {
            Warehouse warehouse = null;
            using (var context = new AuthContext())
            {
                var query = from w in context.Warehouses
                            join p in context.Peoples on w.WarehouseId equals p.WarehouseId
                            where p.PeopleID == input.PeopleId
                            select w;
                warehouse = query.FirstOrDefault();
            }

            if (warehouse != null && warehouse.IsStore == true && warehouse.StoreType == 1)
            {
                var list = ZilaGetList(input);
                return list;
            }
            else
            {
                var list = GetCityList(input);
                return list;
            }
        }
        public List<TripPlannerApprovalRequestDc> GetCityList(TripPlannerApprovalRequestInputDc input)
        {
            using (var context = new AuthContext())
            {
                var peopleIdParam = new SqlParameter
                {
                    ParameterName = "PeopleId",
                    Value = (object)input.PeopleId
                };

                var isPendingOnlyParam = new SqlParameter
                {
                    ParameterName = "IsPendingOnly",
                    Value = (object)input.IsPendingOnly
                };

                var skipParam = new SqlParameter
                {
                    ParameterName = "Skip",
                    Value = (object)input.Skip
                };

                var takeParam = new SqlParameter
                {
                    ParameterName = "Take",
                    Value = (object)input.Take
                };
                var list = context.Database.SqlQuery<TripPlannerApprovalRequestDc>("Operation.GetTripPlannerApprovalRequest @PeopleId, @IsPendingOnly, @Skip, @Take", peopleIdParam, isPendingOnlyParam, skipParam, takeParam).ToList();
                return list;
            }
        }

        public List<TripPlannerApprovalRequestDc> ZilaGetList(TripPlannerApprovalRequestInputDc input)
        {
            using (var context = new AuthContext())
            {
                var peopleIdParam = new SqlParameter
                {
                    ParameterName = "PeopleId",
                    Value = (object)input.PeopleId
                };

                var isPendingOnlyParam = new SqlParameter
                {
                    ParameterName = "IsPendingOnly",
                    Value = (object)input.IsPendingOnly
                };

                var skipParam = new SqlParameter
                {
                    ParameterName = "Skip",
                    Value = (object)input.Skip
                };

                var takeParam = new SqlParameter
                {
                    ParameterName = "Take",
                    Value = (object)input.Take
                };
                var list = context.Database.SqlQuery<TripPlannerApprovalRequestDc>("[Zila].[Zila_GetTripPlannerApprovalRequest] @PeopleId, @IsPendingOnly, @Skip, @Take", peopleIdParam, isPendingOnlyParam, skipParam, takeParam).ToList();
                return list;
            }
        }

        public async Task<bool> ApproveReject(TripPlannerApprovalRequestDc request, bool trueIfApproveElseReject, int peopleId)
        {
            Warehouse warehouse = null;
            using (var context = new AuthContext())
            {
                var query = from w in context.Warehouses
                            join p in context.Peoples on w.WarehouseId equals p.WarehouseId
                            where p.PeopleID == peopleId
                            select w;
                warehouse = query.FirstOrDefault();
            }

            if (warehouse != null && warehouse.IsStore == true && warehouse.StoreType == 1)
            {
                TripPlannerApprovalRequestManager manager = new TripPlannerApprovalRequestManager();
                var list = await manager.ZilaApproveReject(request, trueIfApproveElseReject, peopleId);
                return list;
            }
            else
            {
                TripPlannerApprovalRequestManager manager = new TripPlannerApprovalRequestManager();
                var list =await manager.CityApproveReject(request, trueIfApproveElseReject, peopleId);
                return list;
            }
        }

        public async Task<bool> CityApproveReject(TripPlannerApprovalRequestDc request, bool trueIfApproveElseReject, int peopleId)
        {
            DateTime date = DateTime.Now;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.Serializable;
            option.Timeout = TimeSpan.FromSeconds(90);
            People people = null;
            long tripPlannerConfirmedMasterId;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    people = context.Peoples.Where(x => x.PeopleID == peopleId).FirstOrDefault();
                    var dbRequest = context.TripPlannerApprovalRequestDb.FirstOrDefault(x => x.Id == request.RequestId);
                    tripPlannerConfirmedMasterId = dbRequest.TripPlannerConfirmedMasterId;
                    TripPlannerVehicles vehicle = context.TripPlannerVehicleDb.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == dbRequest.TripPlannerConfirmedMasterId);
                    if (dbRequest.IsApproved || dbRequest.IsRejected || vehicle.CurrentStatus == 6)//vehicle.CurrentStatus--> end Trip
                    {
                        return false;
                    }
                    else
                    {

                        dbRequest.IsApproved = trueIfApproveElseReject ? true : dbRequest.IsApproved;
                        dbRequest.IsRejected = !trueIfApproveElseReject ? true : dbRequest.IsRejected;
                        dbRequest.ProcessedBy = peopleId;
                        dbRequest.ProcessedDate = date;
                        dbRequest.ModifiedBy = peopleId;
                        dbRequest.ModifiedDate = date;
                        vehicle.ModifiedDate = date;
                        if (dbRequest.RequestType == TripPlannerApprovalRequestTypeConstants.StartKm)
                        {
                            vehicle.StartKm = dbRequest.Reading;
                            vehicle.StartKmUrl = dbRequest.ReadingUrl;
                            vehicle.StartKmApprovedBy = trueIfApproveElseReject ? peopleId : vehicle.StartKmApprovedBy;
                            vehicle.StartKmApprovedDate = trueIfApproveElseReject ? date : vehicle.StartKmApprovedDate;
                            vehicle.IsStartKmApproved = trueIfApproveElseReject ? true : vehicle.IsStartKmApproved;
                            vehicle.IsStartKmRequestSend = trueIfApproveElseReject ? vehicle.IsStartKmRequestSend : false;

                        }
                        else
                        {
                            vehicle.ClosingKm = dbRequest.Reading;
                            vehicle.ClosingKMUrl = dbRequest.ReadingUrl;
                            vehicle.CloseKmApprovedBy = peopleId;
                            vehicle.CloseKmApprovedDate = date;
                            vehicle.IsCloseKmApproved = trueIfApproveElseReject ? true : false;
                            vehicle.IsCloseKmRequestSend = trueIfApproveElseReject ? vehicle.IsCloseKmRequestSend : false;

                        }
                        context.Entry(dbRequest).State = EntityState.Modified;
                        context.Entry(vehicle).State = EntityState.Modified;
                        //Notification Delivery App 

                        context.Commit();
                        scope.Complete();

                    }
                }
            }

            using (var context = new AuthContext())
            {
                string Msg = string.Empty;
                if (trueIfApproveElseReject)
                {
                    Msg = people.DisplayName + " accepted your request you can start your given assignment";
                }
                else
                {
                    Msg = people.DisplayName + " rejected your request you can restart your assignment";
                }
                ConfigureNotifyHelper configureNotifyHelper = new ConfigureNotifyHelper();
                string notify_type = "Sarthi App Accpect Notification";
                bool res = await configureNotifyHelper.SendNotificationForApprovalSarthiTrip_DeliveryApp(Msg, tripPlannerConfirmedMasterId, notify_type, trueIfApproveElseReject, context);
                context.Commit();
                return true;
            }
        }
        public async Task<bool> ZilaApproveReject(TripPlannerApprovalRequestDc request, bool trueIfApproveElseReject, int peopleId)
        {
            DateTime date = DateTime.Now;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.Serializable;
            option.Timeout = TimeSpan.FromSeconds(90);
            People people = null;
            long tripPlannerConfirmedMasterId;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    people = context.Peoples.Where(x => x.PeopleID == peopleId).FirstOrDefault();
                    var dbRequest = context.TripPlannerApprovalRequestDb.FirstOrDefault(x => x.Id == request.RequestId);
                    tripPlannerConfirmedMasterId = dbRequest.TripPlannerConfirmedMasterId;
                    var vehicle = context.ZilaTripVehicles.FirstOrDefault(x => x.ZilaTripMasterId == dbRequest.TripPlannerConfirmedMasterId);
                    if (dbRequest.IsApproved || dbRequest.IsRejected || vehicle.CurrentStatus == 6)//vehicle.CurrentStatus--> end Trip
                    {
                        return false;
                    }
                    else
                    {

                        dbRequest.IsApproved = trueIfApproveElseReject ? true : dbRequest.IsApproved;
                        dbRequest.IsRejected = !trueIfApproveElseReject ? true : dbRequest.IsRejected;
                        dbRequest.ProcessedBy = peopleId;
                        dbRequest.ProcessedDate = date;
                        dbRequest.ModifiedBy = peopleId;
                        dbRequest.ModifiedDate = date;
                        vehicle.ModifiedDate = date;
                        if (dbRequest.RequestType == TripPlannerApprovalRequestTypeConstants.StartKm)
                        {
                            vehicle.StartKm = dbRequest.Reading;
                            vehicle.StartKmUrl = dbRequest.ReadingUrl;
                            vehicle.StartKmApprovedBy = trueIfApproveElseReject ? peopleId : vehicle.StartKmApprovedBy;
                            vehicle.StartKmApprovedDate = trueIfApproveElseReject ? date : vehicle.StartKmApprovedDate;
                            vehicle.IsStartKmApproved = trueIfApproveElseReject ? true : vehicle.IsStartKmApproved;
                            vehicle.IsStartKmRequestSend = trueIfApproveElseReject ? vehicle.IsStartKmRequestSend : false;

                        }
                        else
                        {
                            vehicle.ClosingKm = dbRequest.Reading;
                            vehicle.ClosingKMUrl = dbRequest.ReadingUrl;
                            vehicle.CloseKmApprovedBy = peopleId;
                            vehicle.CloseKmApprovedDate = date;
                            vehicle.IsCloseKmApproved = trueIfApproveElseReject ? true : false;
                            vehicle.IsCloseKmRequestSend = trueIfApproveElseReject ? vehicle.IsCloseKmRequestSend : false;

                        }
                        context.Entry(dbRequest).State = EntityState.Modified;
                        context.Entry(vehicle).State = EntityState.Modified;
                        //Notification Delivery App 

                        context.Commit();
                        scope.Complete();

                    }
                }
            }

            using (var context = new AuthContext())
            {
                string Msg = string.Empty;
                if (trueIfApproveElseReject)
                {
                    Msg = people.DisplayName + " accepted your request you can start your given assignment";
                }
                else
                {
                    Msg = people.DisplayName + " rejected your request you can restart your assignment";
                }
                ConfigureNotifyHelper configureNotifyHelper = new ConfigureNotifyHelper();
                string notify_type = "Sarthi App Accpect Notification";
                bool res = await configureNotifyHelper.SendNotificationForApprovalSarthiTrip_DeliveryApp(Msg, tripPlannerConfirmedMasterId, notify_type, trueIfApproveElseReject, context);
                context.Commit();
                return true;
            }
        }

    }
}