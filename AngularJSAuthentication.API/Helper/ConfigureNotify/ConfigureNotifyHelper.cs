using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.ControllerV7.VehicleMaster;
using AngularJSAuthentication.API.Managers.NotificationApprovalMatrix;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.PeopleNotification;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.Model.DeliveryOptimization;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static AngularJSAuthentication.API.Controllers.NotificationController;

namespace AngularJSAuthentication.API.Helper
{

    public class ConfigureNotifyHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        //GetList on behalf of Status and OrderId
        public async Task<bool> GetActionList(int UserId, string OnAction, int OrderId, long TripPlannerConfirmedDetailId, DateTime? NextRedispatchedDate, string Reason, AuthContext context, string VideoUrl = "", bool? IsVideoSeen = null)
        {
            var res = new List<ConfigureNotify>();
            //if (context.DbOrderDetails.Any(x => x.OrderId == OrderId && x.SubsubcategoryName == "Kisan Kirana"))
            //{
            //    res = await context.ConfigureNotifys.Where(x => x.OnAction == OnAction && x.IsActive == true && x.IsDeleted == false).Take(1).ToListAsync();
            //}
            //else   --BY KAPIL
            {
                res = await context.ConfigureNotifys.Where(x => x.OnAction == OnAction && x.IsActive == true && x.IsDeleted == false).ToListAsync();
            }
            var result = Mapper.Map(res).ToANew<List<ConfigureNotifyDc>>();
            if (result != null)
            {
                foreach (var item in result)
                {
                    await SendNotification(UserId, OrderId, TripPlannerConfirmedDetailId, OnAction, NextRedispatchedDate, Reason, item, context, VideoUrl, IsVideoSeen);
                }
            }
            return true;
        }
        //SendNotification
        public async Task<bool> SendNotification(int UserId, int OrderId, long TripPlannerConfirmedDetailId, string OrderStatus, DateTime? NextRedispatchedDate, string Reason, ConfigureNotifyDc ConfigureNotifyDc, AuthContext context, string VideoUrl = "", bool? IsVideoSeen = null)
        {
            //get fcmid logic base on orderid and roleId
            var param = new SqlParameter("OrderId", OrderId);
            var param1 = new SqlParameter("RoleId", ConfigureNotifyDc.RoleId);
            SendConfigureNotifyDc SendConfigureNotify = await context.Database.SqlQuery<SendConfigureNotifyDc>("exec operation.GetOrderPeopleFCM @OrderId,@RoleId", param, param1).FirstOrDefaultAsync();


            if (SendConfigureNotify != null && SendConfigureNotify.PeopleId > 0)
            {
                // by kapil
                var peopleNotification = context.PeopleSentNotifications.Where(x => x.OrderId == OrderId && !x.IsApproved && !x.IsRejected && !x.IsDeleted && x.ToPeopleId == SendConfigureNotify.PeopleId).OrderByDescending(x => x.Id).ToList();
                if (peopleNotification != null && peopleNotification.Any())
                {
                    foreach (var item in peopleNotification)
                    {
                        item.IsDeleted = true;
                        context.Entry(item).State = EntityState.Modified;
                    }

                }
                //
                string Msg = ConfigureNotifyDc.IsApproval ? "Msg for Approval Notification" : "Msg for Notification";
                PeopleSentNotification addnotification = new PeopleSentNotification()
                {
                    OrderId = OrderId,
                    NextRedispatchedDate = NextRedispatchedDate,
                    AppId = ConfigureNotifyDc.AppId,
                    ToPeopleId = SendConfigureNotify.PeopleId,
                    FcmId = SendConfigureNotify.FcmId,
                    Message = Msg,
                    NotificationType = ConfigureNotifyDc.IsApproval ? 2 : 1,// 1: Notifcation , 2 :Notifcation for IsApproval
                    CreatedDate = DateTime.Now,
                    CreatedBy = UserId,
                    OrderStatus = OrderStatus,
                    TripPlannerConfirmedDetailId = TripPlannerConfirmedDetailId,
                    SentNotificationReasons = new List<SentNotificationReason>(),
                    VideoUrl = VideoUrl,
                    IsVideoSeen = IsVideoSeen
                };
                addnotification.SentNotificationReasons.Add(new SentNotificationReason { Reason = Reason });
                await InsertNotification(addnotification, context);
                string FCMKey = "";
                switch (ConfigureNotifyDc.AppId)
                {
                    case (int)AppEnum.SalesApp:
                        //FCMKey = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                        FCMKey = ConfigurationManager.AppSettings["SalesDebugFcmApiKey"];
                        break;
                    case (int)AppEnum.SarthiApp:
                        FCMKey = ConfigurationManager.AppSettings["SarthiFcmApiKey"];
                        break;
                        //case (int)AppEnum.RetailerApp:
                        //    FCMKey = ConfigurationManager.AppSettings["FcmApiKey"];
                        //    break;
                }
                IAppConfiguration AppConfiguration = new BaseAppConfiguration();
                if (ConfigureNotifyDc.IsApproval)
                {
                    var OrderDetail = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderId).FirstOrDefault();
                    await AppConfiguration.SendNotificationForApproval(SendConfigureNotify.FcmId, Msg, OrderId, FCMKey, OrderStatus, OrderDetail.Skcode, OrderDetail.ShopName, OrderDetail.GrossAmount);

                }
                else
                {
                    AppConfiguration.SendNotification(SendConfigureNotify.FcmId, Msg, FCMKey);
                }
            }
            return true;
        }
        //For Approve Notification
        public async Task<bool> IsNotificationApproved(long Id, int PeopleId, bool IsNotificationApproved, AuthContext context)
        {
            int ApproveTimeLeft = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);

            var update = await context.PeopleSentNotifications.Where(x => x.Id == Id).FirstOrDefaultAsync();
            var result = false;
            if (update != null && !update.IsApproved && !update.IsRejected)
            {
                int status = 0;
                if (IsNotificationApproved)
                {
                    update.IsApproved = IsNotificationApproved;
                    update.ApprovedBy = PeopleId;
                    update.ApprovedDate = DateTime.Now;
                    status = 1;
                }
                else
                {
                    update.IsRejected = IsNotificationApproved;
                    update.RejectedBy = PeopleId;
                    update.RejectedDate = DateTime.Now;
                }
                context.Entry(update).State = EntityState.Modified;

                if (update != null && update.CreatedDate.AddMinutes(ApproveTimeLeft) >= DateTime.Now)
                {
                    var OrderId = new SqlParameter("@OrderId", update.OrderId);
                    string FCMID = context.Database.SqlQuery<string>("EXEC GetDboyFCMId @OrderId", OrderId).FirstOrDefault();
                    if (FCMID != null)
                    {
                        //string orderStatus = context.DbOrderMaster.Where(x => x.OrderId == update.OrderId).Select(x => x.Status).FirstOrDefault();
                        IAppConfiguration AppConfiguration = new BaseAppConfiguration();
                        var FCMKey = ConfigurationManager.AppSettings["DeliveryFcmApiKey"];

                        if (status == 1)
                        {
                            string Msg = "Order Action Notification";
                            string notify_type = "Order Action Updated Notification";
                            AppConfiguration.SendNotificationForApprovalDeliveyApp(FCMID, Msg, update.OrderId, FCMKey, update.OrderStatus, status, notify_type);
                            DeliveryAppDashboardController deliveryAppDashboardController = new DeliveryAppDashboardController();
                            if (deliveryAppDashboardController.GenerateOTPForDeliveryAPP(update.OrderId, update.OrderStatus, PeopleId, null, null))
                            {
                                var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == update.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.RedispatchAndOrderCancelVerifyingOTP);
                                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == update.TripPlannerConfirmedDetailId && x.OrderId == update.OrderId).FirstOrDefault();
                            if (tripPlannerConfirmedOrders != null)
                            {
                                string Msg = "Order Reject Action Notification";
                                string notify_type = "Sales Person Order Reject Notification";
                                AppConfiguration.SendNotificationForApprovalDeliveyApp(FCMID, Msg, update.OrderId, FCMKey, update.OrderStatus, status, notify_type);
                                var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == update.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.ReachedDistination);
                                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                tripPlannerConfirmedOrders.WorkingStatus = Convert.ToInt32(WorKingStatus.Pending);
                                context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                            }
                        }
                    }
                }
                result = await context.CommitAsync() > 0;
            }
            return result;
        }
        internal async Task InsertNotification(PeopleSentNotification PeopleSentNotification, AuthContext context)
        {
            try
            {
                PeopleSentNotification.CreatedDate = DateTime.Now;
                context.PeopleSentNotifications.Add(PeopleSentNotification);
                context.Commit();
            }
            catch (Exception ss)
            {
            }
        }
        internal async Task<bool> NotifyCustomer(string FcmId, int CustomerId)
        {
            string Title = "Order Arrived";
            string Message = "Dear customer your order is arrived!!";
            if (FcmId != null)
            {
                IAppConfiguration AppConfiguration = new BaseAppConfiguration();
                AppConfiguration.NotifyCustomer(Title, Message, FcmId, CustomerId);
            }
            return true;
        }
        internal bool TripNotifyAllCustomer(string FcmId, int CustomerId)
        {
            string Title = "Order shipped";
            string Message = "Your order has been shipped !!";
            if (FcmId != null)
            {
                IAppConfiguration AppConfiguration = new BaseAppConfiguration();
                AppConfiguration.NotifyCustomer(Title, Message, FcmId, CustomerId);
            }
            return true;
        }
        public bool GetSalesActionList(int UserId, string OnAction, int OrderId, long TripPlannerConfirmedDetailId, AuthContext context)
        {
            var res = context.ConfigureNotifys.Where(x => x.OnAction == OnAction && x.IsActive == true && x.IsDeleted == false).ToList();
            var result = Mapper.Map(res).ToANew<List<ConfigureNotifyDc>>();
            if (result != null)
            {
                foreach (var item in result)
                {
                    SalesOrderCancelSendNotification(UserId, OrderId, TripPlannerConfirmedDetailId, OnAction, item, context);
                }
            }
            return true;
        }
        //SendNotification
        public bool SalesOrderCancelSendNotification(int UserId, int OrderId, long TripPlannerConfirmedDetailId, string OrderStatus, ConfigureNotifyDc ConfigureNotifyDc, AuthContext context)
        {
            //get fcmid logic base on orderid and roleId
            var param = new SqlParameter("OrderId", OrderId);
            var param1 = new SqlParameter("RoleId", ConfigureNotifyDc.RoleId);
            SendConfigureNotifyDc SendConfigureNotify = context.Database.SqlQuery<SendConfigureNotifyDc>("exec operation.GetOrderPeopleFCM @OrderId,@RoleId", param, param1).FirstOrDefault();
            if (SendConfigureNotify != null && SendConfigureNotify.PeopleId > 0)
            {
                string Msg = "Order Action Notification";
                string FCMKey = "";
                FCMKey = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                IAppConfiguration AppConfiguration = new BaseAppConfiguration();
                string notify_type = "Dboy Order Canceled Successfully";
                AppConfiguration.SendNotificationForApprovalDeliveyApp(SendConfigureNotify.FcmId, Msg, OrderId, FCMKey, OrderStatus, 1, notify_type);
            }
            return true;
        }
        public async Task<bool> SendNotificationForApprovalSarthiTrip_DeliveryApp(string Msg, long TripPlannerConfirmedMasterId, string notify_type, bool trueIfApproveElseReject, AuthContext context)
        {
            var tripPlannerConfirmedMasterId = new SqlParameter("@TripPlannerConfirmedMasterId", TripPlannerConfirmedMasterId);
            string FCMID = context.Database.SqlQuery<string>("EXEC [Operation].TripPlanner_GetDboyFCMId @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId).FirstOrDefault();
            if (FCMID != null)
            {
                IAppConfiguration AppConfiguration = new BaseAppConfiguration();
                var FCMKey = ConfigurationManager.AppSettings["DeliveryFcmApiKey"];
               await AppConfiguration.SendNotificationForApprovalSarthiTrip_DeliveryApp(FCMID, Msg, TripPlannerConfirmedMasterId, FCMKey, notify_type, trueIfApproveElseReject);
            }
            return true;
        }
        public async Task<bool> SendNotificationForSarthiTripApproval(string Msg, long TripPlannerConfirmedMasterId, string notify_type, AuthContext context)
        {
            var warehouseId = context.TripPlannerConfirmedMasters.Where(x => x.Id == TripPlannerConfirmedMasterId).Select(x => x.WarehouseId).FirstOrDefault();
            if (warehouseId > 0)
            {
                var WarehouseId = new SqlParameter("@WarehouseId", warehouseId);
                var FCMIDList = context.Database.SqlQuery<string>("EXEC [Operation].TripPlanner_GetSarthiAppFCMId @WarehouseId", WarehouseId).ToList();
                foreach (var FCMID in FCMIDList)
                {
                    if (FCMID != null)
                    {
                        IAppConfiguration AppConfiguration = new BaseAppConfiguration();
                        var FCMKey = ConfigurationManager.AppSettings["SarthiFcmApiKey"];
                      await  AppConfiguration.SendNotificationForApprovalSarthiTrip_DeliveryApp(FCMID, Msg, TripPlannerConfirmedMasterId, FCMKey, notify_type, false);
                    }
                }
            }
            return true;
        }
    }
}
