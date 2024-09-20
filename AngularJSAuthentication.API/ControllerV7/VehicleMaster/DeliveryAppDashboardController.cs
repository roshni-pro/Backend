using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Managers.NotificationApprovalMatrix;
using AngularJSAuthentication.API.Managers.TripPlanner;
using AngularJSAuthentication.BusinessLayer.Managers.TripPlanner;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using AngularJSAuthentication.DataContracts.TripPlanner;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.DeliveryOptimization;
using AngularJSAuthentication.Model.TripPlanner;
using AngularJSAuthentication.Model.VAN;
using GenricEcommers.Models;
using LinqKit;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
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
using System.Transactions;
using System.Web;
using System.Web.Http;
using AngularJSAuthentication.DataContracts.Shared;
using static AngularJSAuthentication.API.Controllers.External.Other.SellerStoreController;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.DeliveryOptimization;
using AngularJSAuthentication.API.ControllerV1;
using AngularJSAuthentication.API.Managers.CRM;
using Nito.AsyncEx.Synchronous;
using System.Data;
using System.Text;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.API.Managers;
using static AngularJSAuthentication.DataContracts.ScaleUp.ScaleUpDc;
using AngularJSAuthentication.DataContracts.ScaleUp;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using AngularJSAuthentication.API.Helper.Notification;


namespace AngularJSAuthentication.API.ControllerV7.VehicleMaster
{
    [RoutePrefix("api/DeliveryApp")]
    public class DeliveryAppDashboardController : BaseAuthController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public static List<long> ordersToProcess = new List<long>();

        [AllowAnonymous]
        [Route("DeliveryDashboard")]
        [HttpGet]
        public HttpResponseMessage DeliveryDashboard(int PeopleId, long TripPlannerConfirmedMasterId)
        {
            int ApproveNotifyTimeLeftInMinute = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);
            bool IsReturnOnly = false;
            ResponceDc res;
            bool isSKFixVehicle = false;
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            List<AssignmentList> ListAssignment = new List<AssignmentList>();
            using (var context = new AuthContext())
            {

                var tripPlannerConfirmedMaster = tripPlannerHelper.GetCurrentTripPlannerConfirmedMaster(TripPlannerConfirmedMasterId, context);
                if (tripPlannerConfirmedMaster != null)
                {
                    var GetTripPlannerConfirmedDetailId = tripPlannerHelper.GetTripPlannerConfirmedDetailId(tripPlannerConfirmedMaster.Id, tripPlannerConfirmedMaster.IsNotLastMileTrip, context);
                    var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMaster.Id).FirstOrDefault();
                    var deliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMaster.Id).ToList();
                    double distanceLeft = ((double)tripPlannerVehicle.DistanceLeft) / 1000.00;
                    double distanceTraveled = ((double)tripPlannerVehicle.ActualDistanceTraveled) / 1000.00;
                    TripPlannerAppDashboardDC responceDc = new TripPlannerAppDashboardDC()
                    {
                        TripPlannerConfirmedMasterId = tripPlannerConfirmedMaster.Id,
                        CurrentStatus = tripPlannerVehicle.CurrentStatus,
                        TripPlannerVehicleId = tripPlannerVehicle.Id,
                        TripPlannerConfirmedDetailId = GetTripPlannerConfirmedDetailId.Id,
                        BreakTimeInSec = tripPlannerVehicle.BreakTimeInSec,
                        StartKm = tripPlannerVehicle.StartKm ?? 0,
                        CustomerTripStatus = GetTripPlannerConfirmedDetailId.CustomerTripStatus,
                        //CustomerId= GetTripPlannerConfirmedDetailId.CustomerId,
                        tripPlannerDistance = new TripPlannerDistance()
                        {
                            EndTime = tripPlannerVehicle.EndTime,
                            DistanceLeft = Math.Round(distanceLeft, 2),
                            DistanceTraveled = Math.Round(distanceTraveled, 2),
                            ReminingTime = tripPlannerVehicle.ReminingTime,
                            StartTime = tripPlannerVehicle.StartTime,
                            TravelTime = tripPlannerVehicle.TravelTime,
                            TotalTime = tripPlannerConfirmedMaster.TotalTimeInMins
                        },
                        myTrip = new MyTrip()
                        {
                            TripId = tripPlannerConfirmedMaster.TripPlannerMasterId,
                            TotalAmount = tripPlannerConfirmedMaster.TotalAmount,
                            TotalOrder = tripPlannerConfirmedMaster.OrderCount,
                            CustomerCount = tripPlannerConfirmedMaster.CustomerCount
                        }
                    };
                    foreach (var y in deliveryIssuance)
                    {
                        ListAssignment.Add(new AssignmentList
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
                        var tripPlannerConfirmedMasterids = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMaster.Id);
                        var data = context.Database.SqlQuery<PendingOrderlist>("GetDeliveryDashboardOrdercount @PeopleId,@TripPlannerConfirmedMasterId", peopleId, tripPlannerConfirmedMasterids).FirstOrDefault();
                        responceDc.OrderStatuslist = data;

                        var DeliveryIssuanceId = deliveryIssuance.Select(x => x.DeliveryIssuanceId).ToList();
                        string query = " SELECT CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT)FROM OrderDeliveryMasters  odm " +
                                    " join DeliveryIssuances di on odm.DeliveryIssuanceId = di.DeliveryIssuanceId where  di.DeliveryIssuanceId  in ('" + string.Join("','", DeliveryIssuanceId) + "') and odm.Status in ('Shipped', 'Issued','Delivery Canceled Request') and di.Status !='Rejected' ";
                        bool IsPending = context.Database.SqlQuery<bool>(query).First();

                        var tripPlannerConfirmedMasterId = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmedMaster.Id);
                        var IsEnd = context.Database.SqlQuery<bool>("EXEC Operation.TripPlanner_GetCheckTripEndStatus @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId).First();

                        var VehicleMasterId = new SqlParameter("@VehicleMasterId", tripPlannerConfirmedMaster.VehicleMasterId);
                        isSKFixVehicle = context.Database.SqlQuery<bool>("EXEC Operation.TripPlanner_GetCheckSKFIXTripVehicle @VehicleMasterId", VehicleMasterId).First();
                        responceDc.IsSKFixVehicle = isSKFixVehicle;
                        responceDc.OrderStatuslist = data;
                        //Operation.TripPlanner_GetCheckSKPTripVehicle
                        if (!IsPending && !IsEnd)
                        {
                            responceDc.IsTripEnd = true;
                        }
                    }
                    responceDc.ApproveNotifyTimeLeftInMinute = ApproveNotifyTimeLeftInMinute;
                    responceDc.IsNotLastMileTrip = tripPlannerConfirmedMaster.IsNotLastMileTrip;
                    responceDc.IsLocationEnabled = tripPlannerConfirmedMaster.IsLocationEnabled;
                    tripPlannerHelper.SetTripWorkingStatus(responceDc, tripPlannerVehicle, responceDc.IsSKFixVehicle);
                    res = new ResponceDc()
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
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = false,
                        Message = "No Trip Available!!"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }

        [Route("StartAssignment")]
        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage StartAssignment(AssignmentStartEndDc assignmentStartEndDc)
        {

            TripPlannerVehicleManager tripPlannerVehicleManager = new TripPlannerVehicleManager();
            var res = tripPlannerVehicleManager.StartAssignment(assignmentStartEndDc, assignmentStartEndDc.PeopleID);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [Route("EndAssignment")]
        [HttpPost]
        public HttpResponseMessage EndAssignment(AssignmentStartEndDc assignmentStartEndDc)
        {
            TripPlannerVehicleManager tripPlannerVehicleManager = new TripPlannerVehicleManager();
            var res = tripPlannerVehicleManager.EndAssignment(assignmentStartEndDc, assignmentStartEndDc.PeopleID);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
        [Route("MytripOrderList")]
        [HttpGet]
        [AllowAnonymous]
        public List<MytripOrderCustomerWiseDc> MytripOrderList(long TripPlannerConfirmedMasterId)
        {
            MytripOrderCustomerWiseDc list = new MytripOrderCustomerWiseDc();
            List<mytripOrderListDc> Orderlist = new List<mytripOrderListDc>();
            using (var context = new AuthContext())
            {
                var tripPlannerConfirmedMasterId = new SqlParameter("@TripPlannerConfirmedMasterId", TripPlannerConfirmedMasterId);
                Orderlist = context.Database.SqlQuery<mytripOrderListDc>(" EXEC GetMyTripOrderList  @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId).ToList();

                bool isAnyTripRunning = Orderlist.Max(x => x.IsAnyTripRunning);
                var data = Orderlist.GroupBy(x => new
                {
                    //x.OrderId,
                    x.SequenceNo,
                    x.Skcode,
                    x.ShopName,
                    x.lat,
                    x.lg,
                    x.WarehouseAddress,
                    x.CustomerMobile,
                    x.TripPlannerConfirmedDetailId,
                    x.CustomerId
                }).Select(x => new MytripOrderCustomerWiseDc
                {
                    TripPlannerConfirmedDetailId = x.Key.TripPlannerConfirmedDetailId,
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
                    IsVisible = x.Max(z => z.IsVisible),
                    IsSkip = x.Max(z => z.IsSkip),
                    IsNotLastMileTrip = x.Max(z => z.IsNotLastMileTrip),
                    IsAnyTripRunning = isAnyTripRunning,
                    IsProcess = x.Max(z => z.IsProcess),
                    CustomerTripStatus = x.Max(z => z.CustomerTripStatus),
                    IsLocationEnabled = x.Max(z => z.IsLocationEnabled),
                    TripTypeEnum = x.Max(z => z.TripTypeEnum),
                    IsReturnOrder = Orderlist.FirstOrDefault(y => y.OrderType == "ReturnOrder" && y.Status == "Shipped" && y.CustomerId == x.Key.CustomerId) != null ? true : false,
                    IsGeneralOrder = Orderlist.FirstOrDefault(y => y.OrderType != "ReturnOrder" && y.CustomerId == x.Key.CustomerId) != null ? true : false,
                    orderList = x.Select(a => new OrderList()
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
                    foreach (var a in item.orderList)
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
                    var warehouse = context.TripPlannerConfirmedDetails
                       .Where(x => x.TripPlannerConfirmedMasterId == TripPlannerConfirmedMasterId && x.OrderCount == 0 && x.IsActive == true && x.IsDeleted == false)
                       .Select(y => new MytripOrderCustomerWiseDc
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
                           TripPlannerConfirmedDetailId = y.Id,
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

        [Route("MapViewOrderlist")]
        [HttpGet]
        public async Task<MapviewListDc> MapViewOrderlist(long TripPlannerConfirmedMasterId)
        {
            MapviewListDc resultList = new MapviewListDc();
            TripPlannerManager tripPlannerManager = new TripPlannerManager();
            resultList = await tripPlannerManager.GetMapViewOrderList(TripPlannerConfirmedMasterId);
            return resultList;
        }
        [Route("SingleOrderMapviewInfo")]
        [HttpGet]
        public async Task<SingleOrderMapview> SingleOrderMapviewInfo(long TripPlannerConfirmedMasterId, double lat, double lng)
        {
            SingleOrderMapview result = new SingleOrderMapview();
            TripPlannerManager tripPlannerManager = new TripPlannerManager();
            result = await tripPlannerManager.SingleOrderMapviewInfo(TripPlannerConfirmedMasterId, lat, lng);
            return result;
        }
        [Route("GetCustomerWiseOrderList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<GetCustomerWiseOrderListDc> GetCustomerWiseOrderList(long TripPlannerConfirmedDetailId, bool ReturnOrder = false)
        {
            using (var db = new AuthContext())
            {
                var orderReDispatchCount = db.CompanyDetailsDB.Where(x => x.IsActive == true).Select(x => x.OrderReDispatchCount).FirstOrDefault();
                GetCustomerWiseOrderListDc result = new GetCustomerWiseOrderListDc();
                TripPlannerManager tripPlannerManager = new TripPlannerManager();
                result = await tripPlannerManager.GetCustomerWiseOrderList(TripPlannerConfirmedDetailId, orderReDispatchCount, ReturnOrder);

                result.customerOrderInfo.ForEach(x =>
                {
                    if (x.WarehouseId == 2)
                    {
                        x.IsDeliveryCancelledEnable = true;
                    }
                    else
                    {
                        x.IsDeliveryCancelledEnable = (x.OnilnePayment == true) ? false : true;
                    }
                });
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

                    if (customerList != null && customerList.Any() && customerList.First().AccountId.HasValue)
                    {
                        bool isOverDue = await IsCustomerScaleupOverdue(customerList.First().AccountId.Value, (int)customerList.First().CustomerId);

                        foreach (var item in result.customerOrderInfo)
                        {
                            if (isOverDue && customerList.Any(x => x.OrderId == item.OrderId))
                            {
                                item.IsScaleUpPaymentOverdue = true;
                                item.IsScaleUpCustomer = true;
                            }
                            else
                            {
                                {
                                    item.IsScaleUpPaymentOverdue = false;
                                    item.IsScaleUpCustomer = false;
                                }
                            }
                        }
                    }
                }
                return result;
            }
        }
        [Route("OrderUnloding")]
        [HttpPost]
        public async Task<ResponceDc> OrderUnloding(UnlodingDc unlodingDc)
        {
            ResponceDc res;
            using (var context = new AuthContext())
            {
                int userId = GetLoginUserId();
                var tripPlannerVehicle = await context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == unlodingDc.TripPlannerConfirmedMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                if (tripPlannerVehicle != null)
                {
                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.Delivering;
                    tripPlannerVehicle.ModifiedDate = indianTime;
                    tripPlannerVehicle.ModifiedBy = userId;
                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                    TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                    tripPlannerVehicleHistory.CurrentServingOrderId = unlodingDc.CurrentServingOrderId;
                    tripPlannerVehicleHistory.RecordTime = indianTime;
                    tripPlannerVehicleHistory.Lat = unlodingDc.lat;
                    tripPlannerVehicleHistory.Lng = unlodingDc.lng;
                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                    tripPlannerVehicleHistory.CreatedBy = userId;
                    tripPlannerVehicleHistory.IsActive = true;
                    tripPlannerVehicleHistory.IsDeleted = false;
                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.Delivering;
                    tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = unlodingDc.TripPlannerConfirmedDetailId;
                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                    context.Commit();
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = true,
                        Message = "Order Unloding Started!!"
                    };
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = false,
                        Message = "Order Unloding not Started!!"
                    };
                }
            }
            return res;
        }
        [Route("CheckTripOrderCurrentStatus")]
        [HttpGet]
        public async Task<CheckTripOrderCurrentStatusDC> CheckTripOrderCurrentStatus(long TripPlannerConfirmedMasterId)
        {
            {
                using (var context = new AuthContext())
                {
                    var tripPlannerConfirmedMasterId = new SqlParameter("@TripPlannerConfirmedMasterId", TripPlannerConfirmedMasterId);
                    var list = await context.Database.SqlQuery<CheckTripOrderCurrentStatusDC>("Operation.TripPlanner_CheckTripOrderCurrentStatus @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId).FirstOrDefaultAsync();
                    if (list == null)
                    {
                        var tripPlannerConfirmedMasterIds = new SqlParameter("@TripPlannerConfirmedMasterId", TripPlannerConfirmedMasterId);
                        list = await context.Database.SqlQuery<CheckTripOrderCurrentStatusDC>("Operation.TripPlanner_CheckTripWarehouseCurrentStatus @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterIds).FirstOrDefaultAsync();
                    }
                    return list != null ? list : new CheckTripOrderCurrentStatusDC();
                }
            }
        }

        [Route("TripOnBreak")]
        [HttpPost]
        public async Task<ResponceDc> TripOnBreak(OnBreakDC onBreakDc)
        {
            ResponceDc res;
            TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
            using (var context = new AuthContext())
            {
                int userId = GetLoginUserId();
                var tripPlannerVehicle = await context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == onBreakDc.TripPlannerConfirmedMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                if (tripPlannerVehicle != null)
                {
                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.OnBreak;
                    tripPlannerVehicle.ModifiedDate = indianTime;
                    tripPlannerVehicle.ModifiedBy = userId;
                    tripPlannerVehicle.BreakStartTime = DateTime.Now;
                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;

                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                    tripPlannerVehicleHistory.RecordTime = indianTime;
                    tripPlannerVehicleHistory.Lat = onBreakDc.Lat;
                    tripPlannerVehicleHistory.Lng = onBreakDc.Lng;
                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                    tripPlannerVehicleHistory.CreatedBy = userId;
                    tripPlannerVehicleHistory.IsActive = true;
                    tripPlannerVehicleHistory.IsDeleted = false;
                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.OnBreak;
                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                    context.Commit();
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = true,
                        Message = "Break Started!!"
                    };
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = false,
                        Message = "Break not Started!!"
                    };
                }
            }
            return res;
        }
        [Route("TripOnBreakstart")]
        [HttpPost]
        public async Task<ResponceDc> TripOnBreakstart(OnBreakDC onBreakDc)
        {
            ResponceDc res;
            TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
            using (var context = new AuthContext())
            {
                int userId = GetLoginUserId();
                var tripPlannerVehicle = await context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == onBreakDc.TripPlannerConfirmedMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefaultAsync();
                if (tripPlannerVehicle != null)
                {
                    long sec = 0;
                    ///Break Time Calculate
                    if (tripPlannerVehicle.BreakStartTime.HasValue)
                    {
                        TimeSpan diff = DateTime.Now - tripPlannerVehicle.BreakStartTime.Value;
                        sec = Convert.ToInt64(diff.TotalSeconds);
                    }
                    //end
                    tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                    tripPlannerVehicle.ModifiedDate = indianTime;
                    tripPlannerVehicle.ModifiedBy = userId;
                    tripPlannerVehicle.BreakTimeInSec += sec;
                    tripPlannerVehicle.BreakStartTime = null;
                    context.Entry(tripPlannerVehicle).State = EntityState.Modified;

                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                    tripPlannerVehicleHistory.RecordTime = indianTime;
                    tripPlannerVehicleHistory.Lat = onBreakDc.Lat;
                    tripPlannerVehicleHistory.Lng = onBreakDc.Lng;
                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                    tripPlannerVehicleHistory.CreatedBy = userId;
                    tripPlannerVehicleHistory.IsActive = true;
                    tripPlannerVehicleHistory.IsDeleted = false;
                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                    context.Commit();
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = true,
                        Message = "Break End!!"
                    };
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = false,
                        Message = "Break not End!!"
                    };
                }
            }
            return res;
        }
        [Route("RejectAssignmentUpdateTrip")]
        [HttpPost]
        public ResMsg RejectAssignmentUpdateTrip(AssignmentAccRejDTO obj)
        {
            ResMsg res = new ResMsg();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    res = tripPlannerHelper.RejectAssignmentUpdateTrip(scope, context, obj);
                    if (res.Status)
                    {
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

        [Route("MilometerDocumentImageUpload")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult MilometerDocumentImageUpload()
        {
            string LogoUrl = "";
            try
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/DeliveryOptimization")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/DeliveryOptimization"));

                        string extension = Path.GetExtension(httpPostedFile.FileName);
                        string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;
                        LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/DeliveryOptimization"), fileName);

                        httpPostedFile.SaveAs(LogoUrl);
                        AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/DeliveryOptimization", LogoUrl);

                        LogoUrl = "/DeliveryOptimization/" + fileName;
                    }
                }
                return Created<string>(LogoUrl, LogoUrl);
            }
            catch (Exception ex)
            {
                logger.Error("Error in DeliveryOptimization Method: " + ex.Message);
                return null;
            }
        }

        [Route("ReachedDestinationTrip/{tripPlannerConfirmedDetailId}")]
        [HttpGet]
        public bool ReachedDestinationTrip(long tripPlannerConfirmedDetailId)
        {
            using (var context = new AuthContext())
            {
                var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.FirstOrDefault(x => x.Id == tripPlannerConfirmedDetailId);
                if (tripPlannerConfirmedDetails != null)
                {
                    tripPlannerConfirmedDetails.IsDestinationReached = true;
                    context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                    context.Commit();
                    return true;
                }
                return false;
            }
        }


        #region Jan 2022 Build

        [Route("UnlodingItem")]
        [HttpPost]
        public ResponceDc UnlodingItem(ItemuUnloadingDC itemuUnloadingDC)
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
                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == itemuUnloadingDC.tripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    context.Database.CommandTimeout = 300;
                    var orderDetails = context.OrderDispatchedDetailss.Where(x => itemuUnloadingDC.OrderId.Contains(x.OrderId) && x.qty > 0 && x.Deleted == false).Select(x => new { x.OrderId, x.itemname, x.qty, x.ItemMultiMRPId }).ToList();
                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == tripPlannerConfirmedDetails.Id && x.IsActive == true && x.IsDeleted == false).ToList();
                    var tripPlannerConfirmedOrderIds = tripPlannerConfirmedOrders.Select(x => x.Id).ToList();
                    if (tripPlannerVehicle != null && tripPlannerConfirmedDetails != null && orderDetails != null && orderDetails.Any())
                    {
                        tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.Delivering;
                        tripPlannerVehicle.ModifiedDate = indianTime;
                        tripPlannerVehicle.ModifiedBy = userId;
                        context.Entry(tripPlannerVehicle).State = EntityState.Modified;

                        //step 1. set all orders woking status pending those workingstatus is unloading  for giving  tripPlannerConfirmedDetailId                   
                        var orders = tripPlannerConfirmedOrders.Where(x => (x.WorkingStatus == Convert.ToInt32(WorKingStatus.Unloading) || x.WorkingStatus == Convert.ToInt32(WorKingStatus.CollectingPayment))).ToList();
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
                        var TtripPlannerItemCheckList = context.TripPlannerItemCheckListDb.Where(x => tripPlannerConfirmedOrderIds.Contains(x.TripPlannerConfirmedOrderId) && x.IsActive == true && x.IsDeleted == false && x.IsDone == false).ToList();
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
                        var selectorders = tripPlannerConfirmedOrders.Where(x => itemuUnloadingDC.OrderId.Contains((int)x.OrderId)).ToList();
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
                        tripPlannerConfirmedDetails.IsVisible = true;
                        tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.unloading);
                        if (tripPlannerConfirmedDetails.ServeStartTime == null)
                        {
                            tripPlannerConfirmedDetails.ServeStartTime = DateTime.Now;
                        }
                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;


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
                            tripPlannerItemCheckList1.TripPlannerConfirmedOrderId = tripPlannerConfirmedOrders.FirstOrDefault(y => y.OrderId == x.OrderId).Id;
                            context.TripPlannerItemCheckListDb.Add(tripPlannerItemCheckList1);
                        });

                        //TripPlannerVehicleHistory history = new TripPlannerVehicleHistory
                        //{
                        //    CreatedBy = userId,
                        //    CreatedDate = DateTime.Now,
                        //    CurrentServingOrderId = 0,
                        //    DistanceInMeter = 0,
                        //    IsActive = true,
                        //    IsCalculationRecord=true,
                        //    IsDeleted=false,
                        //    Lat= itemuUnloadingDC.Lat,
                        //    Lng = itemuUnloadingDC.Lng,
                        //    TripPlannerConfirmedDetailId= itemuUnloadingDC.tripPlannerConfirmedDetailId,
                        //    RecordTime=DateTime.Now,
                        //    TripPlannerVehicleId= tripPlannerVehicle.Id,
                        //    RecoardStatus= (int)VehicleliveStatus.Unloading

                        //};
                        //context.TripPlannerVehicleHistoryDb.Add(history);
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


        [Route("GetUnloadItemListPage")]
        [HttpGet]
        public async Task<GetUnloadItemListPageDC> GetUnloadItemListPage(long TripPlannerConfirmedDetailId)
        {
            GetUnloadItemListPageDC result = new GetUnloadItemListPageDC();
            TripPlannerManager tripPlannerManager = new TripPlannerManager();
            result = await tripPlannerManager.GetUnloadItemListPage(TripPlannerConfirmedDetailId);
            return result;
        }

        //[Route("CheckAndUnCheckedUnloadingItem")]
        //[HttpGet]
        //public ResponceDc CheckAndUnCheckedUnloadingItem(bool IsUnloaded, long TripPlannerConfirmedDetailId, int ItemMultiMRPId)
        //{
        //    ResponceDc res = null;
        //    TransactionOptions option = new TransactionOptions();
        //    option.IsolationLevel = System.Transactions.System.Transactions.IsolationLevel.RepeatableRead;
        //    option.Timeout = TimeSpan.FromSeconds(90);
        //    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            int userId = GetLoginUserId();
        //            var tripPlannerConfirmedOrderIds = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).ToList();
        //            var tripPlannerItemCheckList = context.TripPlannerItemCheckListDb.Where(x => tripPlannerConfirmedOrderIds.Contains(x.TripPlannerConfirmedOrderId) && x.ItemMultiMRPId == ItemMultiMRPId && x.IsActive == true && x.IsDeleted == false && x.IsDone == false).ToList();
        //            if (tripPlannerItemCheckList != null && tripPlannerItemCheckList.Any())
        //            {
        //                foreach (var item in tripPlannerItemCheckList)
        //                {
        //                    if (item != null && IsUnloaded)
        //                    {
        //                        item.IsUnloaded = IsUnloaded;
        //                        item.ModifiedDate = indianTime;
        //                        item.ModifiedBy = userId;
        //                        context.Entry(item).State = EntityState.Modified;
        //                        //context.Commit();
        //                        //ssscope.Complete();
        //                        res = new ResponceDc()
        //                        {
        //                            TripDashboardDC = null,
        //                            Status = true,
        //                            Message = "Unloding Item Successfully!!"
        //                        };
        //                    }
        //                    else
        //                    {
        //                        item.IsUnloaded = IsUnloaded;
        //                        item.ModifiedDate = indianTime;
        //                        item.ModifiedBy = userId;
        //                        context.Entry(item).State = EntityState.Modified;
        //                        //context.Commit();
        //                        //scope.Complete();
        //                        res = new ResponceDc()
        //                        {
        //                            TripDashboardDC = null,
        //                            Status = true,
        //                            Message = "Unloding Item Successfully!!"
        //                        };
        //                    }
        //                }
        //                if (context.Commit() > 0)
        //                {
        //                    scope.Complete();
        //                }
        //            }
        //            else
        //            {
        //                scope.Dispose();
        //                res = new ResponceDc()
        //                {
        //                    TripDashboardDC = null,
        //                    Status = false,
        //                    Message = "Unloding Item not Successfully!!"
        //                };
        //            }
        //        }
        //    }
        //    return res;
        //}
        [Route("CheckAndUnCheckedUnloadingItem")]
        [AllowAnonymous]
        [HttpPost]
        //  public ResponceDc CheckAndUnCheckedUnloadingItem(bool IsUnloaded, long TripPlannerConfirmedDetailId, int ItemMultiMRPId)
        public ResponceDc CheckAndUnCheckedUnloadingItem(CheckUncheckDc checkUncheck)
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
                    var tripPlanner = context.TripPlannerConfirmedOrders.Where(x => x.IsActive == true && x.IsDeleted == false && x.TripPlannerConfirmedDetailId == checkUncheck.TripPlannerConfirmedDetailId).Select(x => x.Id).ToList();
                    var tripPlannerItemCheckList = context.TripPlannerItemCheckListDb.Where(x => tripPlanner.Contains(x.TripPlannerConfirmedOrderId) && checkUncheck.ItemMultiMRPId.Contains(x.ItemMultiMRPId) && x.IsActive == true && x.IsDeleted == false && x.IsDone == false).ToList();
                    //var tripPlannerItemCheckList = context.TripPlannerItemCheckListDb.Where(x => tripPlannerConfirmedOrderIds.Contains(x.TripPlannerConfirmedOrderId) && x.ItemMultiMRPId == ItemMultiMRPId && x.IsActive == true && x.IsDeleted == false && x.IsDone == false).ToList();

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

        [Route("CollectPaymentOrderStatusChange")]
        [HttpGet]
        public ResponceDc CollectPaymentOrderStatusChange(long tripPlannerConfirmedDetailId)
        {
            ResponceDc res;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var db = new AuthContext())
                {
                    int userId = GetLoginUserId();
                    int idStatus = Convert.ToInt32(WorKingStatus.Unloading);
                    var tripPlannerConfirmedOrders = db.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == tripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false && x.WorkingStatus == idStatus).ToList();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).ToList();
                    var tripPlannerItemCheckList = db.TripPlannerItemCheckListDb.Where(x => orderids.Contains(x.OrderId) && x.IsUnloaded == true && x.IsDone == false).ToList();
                    var tripPlannerConfirmedDetail = db.TripPlannerConfirmedDetails.Where(x => x.Id == tripPlannerConfirmedDetailId).FirstOrDefault();
                    if (tripPlannerItemCheckList != null && tripPlannerItemCheckList.Any())
                    {
                        tripPlannerItemCheckList.ForEach(x =>
                        {
                            x.IsDone = true;
                            x.ModifiedBy = userId;
                            x.ModifiedDate = DateTime.Now;
                            db.Entry(x).State = EntityState.Modified;
                        });
                        tripPlannerConfirmedOrders.ForEach(y =>
                        {
                            y.WorkingStatus = Convert.ToInt32(WorKingStatus.CollectingPayment);
                            y.ModifiedBy = userId;
                            y.ModifiedDate = DateTime.Now;
                            db.Entry(y).State = EntityState.Modified;
                        });
                        tripPlannerConfirmedDetail.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.CollectingPayment);
                        db.Entry(tripPlannerConfirmedDetail).State = EntityState.Modified;
                        db.Commit();
                        scope.Complete();
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = true,
                            Message = "Collect Payment Start!!"
                        };
                    }
                    else
                    {
                        scope.Dispose();
                        res = new ResponceDc()
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

        [Route("GetCollectPaymentOrderList")]
        [HttpGet]
        public async Task<GetCollectPaymentOrderListDc> GetCollectPaymentOrderList(long TripPlannerConfirmedDetailId)
        {
            GetCollectPaymentOrderListDc result = new GetCollectPaymentOrderListDc();
            TripPlannerManager tripPlannerManager = new TripPlannerManager();
            result = await tripPlannerManager.GetCollectPaymentOrderList(TripPlannerConfirmedDetailId);
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
        [Route("SubmitPayment")]
        [HttpPost]
        public NewResponceDc SubmitPayment(CollectPaymentDC collectPaymentDCs)
        {
            NewResponceDc res = null;
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

                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == collectPaymentDCs.TripPlannerConfirmedDetailId).FirstOrDefault();

                    var AlltripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == collectPaymentDCs.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                    var tripPlannerConfirmedOrders = AlltripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == collectPaymentDCs.TripPlannerConfirmedDetailId && collectPaymentDCs.OrderIds.Contains((int)x.OrderId) && x.IsActive == true && x.IsDeleted == false && x.WorkingStatus == status).ToList();
                    var Allorderids = AlltripPlannerConfirmedOrders.Select(x => x.OrderId).Distinct().ToList();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).Distinct().ToList();
                    var AlltripPlannerConfirmedOrderIds = AlltripPlannerConfirmedOrders.Select(x => x.Id).Distinct().ToList();
                    var tripPlannerConfirmedOrderIds = tripPlannerConfirmedOrders.Select(x => x.Id).Distinct().ToList();
                    var AlltripPaymentResponseAppDb = context.TripPaymentResponseAppDb.Where(x => AlltripPlannerConfirmedOrderIds.Contains(x.TripPlannerConfirmedOrderId.Value) && x.IsActive == true && x.IsDeleted == false).ToList();
                    var tripPaymentResponseAppDb = AlltripPaymentResponseAppDb.Where(x => tripPlannerConfirmedOrderIds.Contains(x.TripPlannerConfirmedOrderId.Value) && x.IsActive == true && x.IsDeleted == false).ToList();
                    var AllorderDispatchedMaster = context.OrderDispatchedMasters.Where(x => Allorderids.Contains(x.OrderId)).ToList();
                    var orderDispatchedMaster = AllorderDispatchedMaster.Where(x => orderids.Contains(x.OrderId)).ToList();
                    if (collectPaymentDCs.CashAmount > 0)
                    {
                        //tripPlannerConfirmedOrders.ForEach(x =>
                        //{
                        //    var OrderAmount = orderDispatchedMaster.FirstOrDefault(y => y.OrderId == x.OrderId).GrossAmount;
                        //    if (OrderAmount > 0)
                        //    {
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
                        //    }
                        //});
                    }

                    var PaymentResponseRetailerAppList = context.PaymentResponseRetailerAppDb.Where(x => collectPaymentDCs.OrderIds.Contains(x.OrderId) && x.status == "Success").ToList();
                    var filterOd = AllorderDispatchedMaster.Where(x => !orderids.Contains(x.OrderId)).Select(x => x.OrderId).ToList();
                    var DispatchedOrderids = AllorderDispatchedMaster.Where(x => filterOd.Contains(x.OrderId)).Select(x => x.OrderId).ToList();
                    var oids = DispatchedOrderids.Select(x => (long)x).ToList();
                    var flitertripPlannerConfirmedOrderIds = AlltripPlannerConfirmedOrders.Where(x => oids.Contains(x.OrderId)).Select(x => x.Id).Distinct().ToList();

                    double ExistsCashAmount = AlltripPaymentResponseAppDb.Where(x => flitertripPlannerConfirmedOrderIds.Contains(x.TripPlannerConfirmedOrderId.Value) && x.PaymentFrom == "Cash" && x.Status == "Success").Sum(x => x.Amount);

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
                            res = new NewResponceDc()
                            {
                                Status = false,
                                Message = "Alert! You cannot exceed the cash limit of 2 lacs for this customer in a day.",
                                TripPlannerConfirmedDetailId = 0
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
                            var ListTripPaymentResponseAppDb = tripPaymentResponseAppDb.Where(x => x.TripPlannerConfirmedOrderId == item.Id).ToList();
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
                                        TripPlannerConfirmedOrderId = item.Id,
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
                                        TripPlannerConfirmedOrderId = item.Id,
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
                                            TripPlannerConfirmedOrderId = item.Id,
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
                                            TripPlannerConfirmedOrderId = item.Id,
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
                                    TripPlannerConfirmedOrderId = item.Id,
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
                                res = new NewResponceDc()
                                {
                                    Status = false,
                                    Message = "Payment not submitted successfully!!",
                                    TripPlannerConfirmedDetailId = 0
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
                            res = new NewResponceDc()
                            {
                                Status = true,
                                Message = "Payment successfully submitted!!",
                                TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id
                            };
                        }
                    }
                    else
                    {
                        scope.Dispose();
                        res = new NewResponceDc()
                        {
                            Status = false,
                            Message = "Payment not found!!",
                            TripPlannerConfirmedDetailId = 0
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

        #region Delivered  

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
                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == TripplannerConfirmdetailedId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == TripplannerConfirmdetailedId && (x.WorkingStatus == statusIds || x.WorkingStatus == statusId2) && x.IsActive == true && x.IsDeleted == false).ToList();
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


        [Route("DeliverdConfirmOtp")]
        [HttpPost]
        public HttpResponseMessage DeliverdConfirmOtp(OrderConfirmOtpDc OrderConfirmOtp)
        {
            OrderResponceDc res = null;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    List<OrderMaster> ODMaster = null;
                    int userId = GetLoginUserId();
                    List<string> MposGatId = new List<string>();
                    double totalMposamount = 0;
                    bool IsDeliverSmsSent = false;
                    bool Status = false;
                    #region API POINT
                    //Otp matched any order
                    //update alrwady complted status ko (completed 4.0) krna tripplannerconfirmordermaster
                    //update status otp status Completed or Pending in detail table and IsProcee true if completed
                    //write Delivered Code here check payment mode from tripPlannerConfirmedOrders for cash and mpos
                    #endregion
                    var statusIds = Convert.ToInt32(WorKingStatus.AlredyCollected);
                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == OrderConfirmOtp.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false && x.WorkingStatus == statusIds).ToList();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).ToList();
                    ordersToProcess.AddRange(orderids);
                    var otp = context.OrderDeliveryOTP.Where(x => orderids.Contains(x.OrderId) && x.IsActive && x.OTP == OrderConfirmOtp.Otp).ToList();
                    if (otp != null && otp.Any())
                    {
                        var People = context.Peoples.Where(x => x.Mobile == OrderConfirmOtp.DboyMobileNo).FirstOrDefault();
                        var ODM = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).Include("orderDetails").ToList();
                        ODMaster = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Include("orderDetails").ToList();
                        var deliveryIssuanceId = ODM.FirstOrDefault().DeliveryIssuanceIdOrderDeliveryMaster;
                        var DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == deliveryIssuanceId).SingleOrDefault();
                        if (OrderConfirmOtp != null && People != null && DeliveryIssuance.DeliveryIssuanceId > 0 && ODM != null)
                        {
                            foreach (var newOrderMaster in ODM)
                            {
                                var ordermaster = ODMaster.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(z => z.DeliveryIssuanceId == DeliveryIssuance.DeliveryIssuanceId && z.OrderId == newOrderMaster.OrderId).Include("orderDetails").FirstOrDefault();
                                var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == DeliveryIssuance.DeliveryIssuanceId && x.OrderId == newOrderMaster.OrderId);
                                var Ordercancellation = context.DeptOrderCancellationDb.Where(o => o.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                DeptOrderCancellation deptOrderCacellation = new DeptOrderCancellation();
                                deptOrderCacellation.OrderId = newOrderMaster.OrderId;

                                if (AssignmentRechangeOrder != null)
                                {
                                    AssignmentRechangeOrder.Status = 0;
                                    AssignmentRechangeOrder.ModifiedDate = indianTime;
                                    AssignmentRechangeOrder.ModifiedBy = People.PeopleID;
                                    context.Entry(AssignmentRechangeOrder).State = EntityState.Modified;
                                }

                                #region Damage Order status Update                          
                                if (ordermaster.OrderType == 6 && newOrderMaster.invoice_no != null)
                                {
                                    var DOM = context.DamageOrderMasterDB.Where(x => x.invoice_no == newOrderMaster.invoice_no).SingleOrDefault();

                                    if (DOM != null)
                                    {
                                        DOM.UpdatedDate = indianTime;
                                        DOM.Status = OrderConfirmOtp.Status;
                                        context.Entry(DOM).State = EntityState.Modified;
                                    }
                                }
                                #endregion

                                if (OrderConfirmOtp.Status == "Delivered" && newOrderMaster.Status != "Delivered")
                                {
                                    var tripPlannerOrders = tripPlannerConfirmedOrders.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                    var PaymentResponseRetailerAppList = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == newOrderMaster.OrderId && z.status == "Success").ToList();
                                    double cashamount = PaymentResponseRetailerAppList.Where(x => x.IsOnline == false).Sum(x => x.amount);
                                    #region Payments                                                                     
                                    if (tripPlannerOrders.PaymentMode == "mPos" && cashamount > 0)
                                    {
                                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                        {
                                            OrderId = newOrderMaster.OrderId,
                                            status = "Success",
                                            CreatedDate = indianTime,
                                            UpdatedDate = indianTime,
                                            PaymentFrom = "mPos",
                                            statusDesc = "Due to Delivery",
                                            amount = Math.Round(cashamount, 0),
                                            GatewayTransId = tripPlannerOrders.PaymentRefId,
                                            IsOnline = true
                                        });
                                        MposGatId.Add(tripPlannerOrders.PaymentRefId);
                                        totalMposamount += Math.Round(cashamount, 0);
                                    }
                                    if (tripPlannerOrders.PaymentMode == "RTGS/NEFT" && cashamount > 0)
                                    {
                                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                        {
                                            OrderId = newOrderMaster.OrderId,
                                            status = "Success",
                                            CreatedDate = indianTime,
                                            UpdatedDate = indianTime,
                                            PaymentFrom = "RTGS/NEFT",
                                            statusDesc = "Due to Delivery",
                                            amount = Math.Round(cashamount, 0),
                                            GatewayTransId = tripPlannerOrders.PaymentRefId,
                                            IsOnline = true
                                        });
                                        MposGatId.Add(tripPlannerOrders.PaymentRefId);
                                        totalMposamount += Math.Round(cashamount, 0);
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
                                    var othermodeAmt = OnlinePaymentResponseRetailer.Where(x => x.IsOnline).Sum(x => x.amount);
                                    #endregion


                                    // var othermodeAmt = PaymentResponseRetailerAppList.Where(x => x.IsOnline /*x.PaymentFrom == "hdfc" || x.PaymentFrom == "ePaylater" || x.PaymentFrom == "Gullak"*/).Sum(x => x.amount);
                                    var totalAmount = totalMposamount + othermodeAmt;

                                    if (newOrderMaster.GrossAmount != cashamount + totalAmount)
                                    {
                                        cashamount = newOrderMaster.GrossAmount - totalAmount;
                                    }
                                    var cashpayment = PaymentResponseRetailerAppList.FirstOrDefault(x => x.OrderId == newOrderMaster.OrderId && x.status == "Success" && x.PaymentFrom == "Cash");
                                    if (cashpayment != null)
                                    {
                                        cashpayment.amount = cashamount;
                                        cashpayment.UpdatedDate = indianTime;
                                        cashpayment.status = cashamount > 0 ? cashpayment.status : "Failed";
                                        cashpayment.statusDesc = "Due to Delivery";
                                        context.Entry(cashpayment).State = EntityState.Modified;
                                    }
                                    else if (cashpayment == null && cashamount > 0)
                                    {
                                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                        {
                                            OrderId = newOrderMaster.OrderId,
                                            status = "Success",
                                            CreatedDate = indianTime,
                                            UpdatedDate = indianTime,
                                            statusDesc = "Due to Delivery",
                                            PaymentFrom = "Cash",
                                            amount = Math.Round(cashamount, 0)
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
                                    #region  for Franchises
                                    if (context.Customers.Any(x => x.CustomerId == newOrderMaster.CustomerId && x.IsFranchise == true))
                                    {
                                        var DeliveredOrderToFranchisesdb = context.DeliveredOrderToFranchises.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                        if (DeliveredOrderToFranchisesdb == null)
                                        {
                                            DeliveredOrderToFranchise FDB = new DeliveredOrderToFranchise();
                                            FDB.OrderId = newOrderMaster.OrderId;
                                            FDB.CreatedDate = indianTime;
                                            FDB.IsProcessed = false;
                                            context.DeliveredOrderToFranchises.Add(FDB);
                                        }
                                    }
                                    #endregion
                                    #region New Delivery Optimization process
                                    if (OrderConfirmOtp.TripPlannerConfirmedDetailId > 0)
                                    {
                                        //var tripPlannerConfirmedOrderId = new SqlParameter("@TripPlannerConfirmedOrderId", obj.DeliveryOptimizationdc.TripPlannerConfirmedOrderId);
                                        //long tripPlannerConfirmedDetailId = context.Database.SqlQuery<long>("GetTripPlannerConfirmedDetailId @TripPlannerConfirmedOrderId", tripPlannerConfirmedOrderId).FirstOrDefault();
                                        var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == OrderConfirmOtp.TripPlannerConfirmedDetailId).FirstOrDefault();
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

                                            var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
                                            if (tripPlannerVehicle != null)
                                            {
                                                var tripPlannerConfirmedOrderss = context.TripPlannerConfirmedOrders.Where(x => orderids.Contains((int)x.OrderId) && x.TripPlannerConfirmedDetailId == OrderConfirmOtp.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
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
                                                    TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                    tripPlannerVehicleHistory.CurrentServingOrderId = newOrderMaster.OrderId;
                                                    tripPlannerVehicleHistory.RecordTime = indianTime;
                                                    tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                                    tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                    tripPlannerVehicleHistory.CreatedBy = userId;
                                                    tripPlannerVehicleHistory.IsActive = true;
                                                    tripPlannerVehicleHistory.IsDeleted = false;
                                                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                                    //tripPlannerConfirmedDetails
                                                    tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.Completed);
                                                    tripPlannerConfirmedDetails.ServeEndTime = DateTime.Now;
                                                    context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                                    //tripPlannerConfirmedOrderMaster
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
                                                    TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                    tripPlannerVehicleHistory.CurrentServingOrderId = newOrderMaster.OrderId;
                                                    tripPlannerVehicleHistory.RecordTime = indianTime;
                                                    tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                                    tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                    tripPlannerVehicleHistory.CreatedBy = userId;
                                                    tripPlannerVehicleHistory.IsActive = true;
                                                    tripPlannerVehicleHistory.IsDeleted = false;
                                                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                                    tripPlannerOrderid.WorkingStatus = Convert.ToInt32(WorKingStatus.Completed);
                                                    context.Entry(tripPlannerOrderid).State = EntityState.Modified;
                                                    tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.ReachedDistination);
                                                    context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
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
                                        res = new OrderResponceDc()
                                        {
                                            OrderConfirmOtpDc = null,
                                            Message = "Not Delivered",
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
                                    var order = ODM.FirstOrDefault();
                                    if (IsDeliverSmsSent && !string.IsNullOrEmpty(order.Customerphonenum))
                                    {
                                        // string message = "Hi " + order.CustomerName + " Your Order #" + order.OrderId + " is delivered on time if you have any complaint regarding your order kindly contact our customer care within next 1 Hours.";
                                        string message = ""; //"Hi {#var1#} Your Order {#var2#} is delivered on time if you have any complaint regarding your order kindly contact our customer care within {#var3#}. ShopKirana";
                                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Delivered_Order_Compaint");
                                        message = dltSMS == null ? "" : dltSMS.Template;


                                        message = message.Replace("{#var1#}", order.CustomerName);
                                        message = message.Replace("{#var2#}", order.OrderId.ToString());
                                        message = message.Replace("{#var3#}", " next 1 Hours");
                                        if (dltSMS != null)
                                            Common.Helpers.SendSMSHelper.SendSMS(order.Customerphonenum, message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                                    }
                                    res = new OrderResponceDc()
                                    {
                                        OrderConfirmOtpDc = null,
                                        Message = "Order Delivered Succssfully!!",
                                        Status = true
                                    };
                                    ordersToProcess.RemoveAll(x => orderids.Contains(x));
                                    return Request.CreateResponse(HttpStatusCode.OK, res);
                                }
                            }
                            else
                            {
                                scope.Dispose();
                                res = new OrderResponceDc()
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
                            res = new OrderResponceDc()
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
                        res = new OrderResponceDc()
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
        [Route("DeliverdConfirmOtpNew")]
        [HttpPost]
        public HttpResponseMessage DeliverdConfirmOtpNew(OrderConfirmOtpDc OrderConfirmOtp)
        {
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            OrderResponceDc res = null;
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
                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == OrderConfirmOtp.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false && (x.WorkingStatus == statusIds || x.WorkingStatus == statusId1)).ToList();
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
                                //var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == DeliveryIssuance.DeliveryIssuanceId && x.OrderId == newOrderMaster.OrderId);
                                //var Ordercancellation = context.DeptOrderCancellationDb.Where(o => o.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                //DeptOrderCancellation deptOrderCacellation = new DeptOrderCancellation();
                                //deptOrderCacellation.OrderId = newOrderMaster.OrderId;

                                //if (AssignmentRechangeOrder != null)
                                //{
                                //    AssignmentRechangeOrder.Status = 0;
                                //    AssignmentRechangeOrder.ModifiedDate = indianTime;
                                //    AssignmentRechangeOrder.ModifiedBy = People.PeopleID;
                                //    context.Entry(AssignmentRechangeOrder).State = EntityState.Modified;
                                //}

                                #region Damage Order status Update  
                                if (ordermaster.OrderType == 6 && newOrderMaster.invoice_no != null)
                                {
                                    var DOM = context.DamageOrderMasterDB.Where(x => x.invoice_no == newOrderMaster.invoice_no).SingleOrDefault();

                                    if (DOM != null)
                                    {
                                        DOM.UpdatedDate = indianTime;
                                        DOM.Status = OrderConfirmOtp.Status;
                                        context.Entry(DOM).State = EntityState.Modified;
                                    }
                                }
                                #endregion

                                if (OrderConfirmOtp.Status == "Delivered" && newOrderMaster.Status != "Delivered")
                                {
                                    double OrderCashAmount = 0;
                                    double totalChequeamt = 0;
                                    double totalMposamount = 0;
                                    double totalAmount = 0;
                                    var tripPlannerOrders = tripPlannerConfirmedOrders.Where(x => x.OrderId == newOrderMaster.OrderId).FirstOrDefault();
                                    var TripPaymentResponseAppslist = context.TripPaymentResponseAppDb.Where(z => z.TripPlannerConfirmedOrderId == tripPlannerOrders.Id && z.Status == "Success" && z.IsActive == true && z.IsDeleted == false).ToList();

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
                                        var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == OrderConfirmOtp.TripPlannerConfirmedDetailId).FirstOrDefault();
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

                                            var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
                                            if (tripPlannerVehicle != null)
                                            {
                                                var tripPlannerConfirmedOrderss = context.TripPlannerConfirmedOrders.Where(x => orderids.Contains((int)x.OrderId) && x.TripPlannerConfirmedDetailId == OrderConfirmOtp.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
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
                                                    TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                    tripPlannerVehicleHistory.CurrentServingOrderId = newOrderMaster.OrderId;
                                                    tripPlannerVehicleHistory.RecordTime = indianTime;
                                                    tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                                    tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                    tripPlannerVehicleHistory.CreatedBy = userId;
                                                    tripPlannerVehicleHistory.IsActive = true;
                                                    tripPlannerVehicleHistory.IsDeleted = false;
                                                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
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
                                                    TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                    tripPlannerVehicleHistory.CurrentServingOrderId = newOrderMaster.OrderId;
                                                    tripPlannerVehicleHistory.RecordTime = indianTime;
                                                    tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                                    tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                    tripPlannerVehicleHistory.CreatedBy = userId;
                                                    tripPlannerVehicleHistory.IsActive = true;
                                                    tripPlannerVehicleHistory.IsDeleted = false;
                                                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
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
                                        res = new OrderResponceDc()
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
                                                TextFileLogHelper.TraceLog("scaleUp OrderInvoice OrderNo : " + item.Key.OrderId.ToString());
                                                var response = AsyncContext.Run(async () => await scaleUpManager.OrderInvoice(item.Key.OrderId.ToString(), od.invoice_no, item.Sum(y => y.amount), oDM.CreatedDate, FileUrl, true));
                                                response.OrderNo = item.Key.OrderId.ToString();
                                                scalupRespones.Add(response);
                                                TextFileLogHelper.TraceLog("scaleUp OrderInvoice OrderNo : " + item.Key.OrderId.ToString() + " Response with status: " + response.OrderNo);
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
                                        res = new OrderResponceDc()
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
                                        res = new OrderResponceDc()
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
                                res = new OrderResponceDc()
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
                            res = new OrderResponceDc()
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
                        res = new OrderResponceDc()
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

        #endregion

        #region  Redispatched and reattempt single Order

        // call in Redispatched and reattempt 
        //[AllowAnonymous]
        [AllowAnonymous]
        [Route("NotifyReDispatchAndReAttempt")]
        [HttpPost]
        public async Task<ResponceDc> NotifyReDispatchAndReAttempt(ReDispatchAndReAttemptNewDC ReDispatchAndReAttemptDc)
        {
            int userId = GetLoginUserId();
            ResponceDc res;
            bool result = false;
            int statusValue = 0;
            using (var context = new AuthContext())
            {
                var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == ReDispatchAndReAttemptDc.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == ReDispatchAndReAttemptDc.TripPlannerConfirmedDetailId && x.OrderId == ReDispatchAndReAttemptDc.OrderId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                //send notification  => call GetActionList
                if (tripPlannerConfirmedDetails != null && tripPlannerConfirmedOrders != null)
                {
                    var ordertype = context.DbOrderMaster.Where(x => x.OrderId == tripPlannerConfirmedOrders.OrderId).FirstOrDefault();
                    string OnAction = "", status = "Delivery Redispatch";
                    if (!ReDispatchAndReAttemptDc.IsReAttempt)
                    {
                        OnAction = "Delivery Redispatch";
                        statusValue = Convert.ToInt32(WorKingStatus.DeliveryRedispatch); ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                        //bool IsSendNotification = await helper.GetActionList(userId, OnAction, ReDispatchAndReAttemptDc.OrderId, ReDispatchAndReAttemptDc.TripPlannerConfirmedDetailId, ReDispatchAndReAttemptDc.NextRedispatchedDate, ReDispatchAndReAttemptDc.Reason, context);

                        if (ordertype.OrderType != 6 && ordertype.OrderType != 9)
                        {
                            result = GenerateOTPForDeliveryAPP(ReDispatchAndReAttemptDc.OrderId, status, userId, ReDispatchAndReAttemptDc.lat, ReDispatchAndReAttemptDc.lg, ReDispatchAndReAttemptDc.VideoUrl);
                        }
                        tripPlannerConfirmedDetails.IsVisible = true;
                        tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.RedispatchAndOrderCancelVerifyingOTP);
                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                        tripPlannerConfirmedOrders.WorkingStatus = statusValue;
                        context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                        context.Commit();


                        if (ordertype.OrderType == 6 || ordertype.OrderType == 9)
                        {

                            string sRandomOTP = "";
                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                            sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                            List<int> OrderIDs = new List<int>();
                            if (true)
                            {
                                var ExistsOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == tripPlannerConfirmedOrders.OrderId && x.IsActive == true).OrderByDescending(x => x.CreatedDate).ToList();
                                if (ExistsOTPs != null && ExistsOTPs.Any())
                                {
                                    foreach (var ExistsOTP in ExistsOTPs)
                                    {
                                        ExistsOTP.IsActive = false;
                                        ExistsOTP.ModifiedDate = DateTime.Now;
                                        ExistsOTP.ModifiedBy = userId;
                                        context.Entry(ExistsOTP).State = EntityState.Modified;
                                    }
                                }
                                if (!string.IsNullOrEmpty(sRandomOTP))
                                {
                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                    {
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        OrderId = (int)tripPlannerConfirmedOrders.OrderId,
                                        OTP = sRandomOTP,
                                        Status = "Delivery Redispatch",
                                        UserType = "HQ Operation",
                                        IsUsed = false,
                                        UserId = 0,
                                        lat = ReDispatchAndReAttemptDc.lat,
                                        lg = ReDispatchAndReAttemptDc.lg,
                                        IsVideoSeen = false,
                                        VideoUrl = ""

                                    };
                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                    context.Commit();

                                    res = new ResponceDc()
                                    {
                                        Message = "Otp sent succesfully !!"
                                    };
                                    result = true;
                                }
                            }
                        }

                    }
                    else
                    {
                        OnAction = "ReAttempt";
                        statusValue = Convert.ToInt32(WorKingStatus.ReAttempt);
                        ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                        bool IsSendNotification = await helper.GetActionList(userId, OnAction, ReDispatchAndReAttemptDc.OrderId, ReDispatchAndReAttemptDc.TripPlannerConfirmedDetailId, ReDispatchAndReAttemptDc.NextRedispatchedDate, ReDispatchAndReAttemptDc.Reason, context, ReDispatchAndReAttemptDc.VideoUrl, IsVideoSeen: false);
                        //result = GenerateOTPForDeliveryAPP(ReDispatchAndReAttemptDc.OrderId, status, userId, ReDispatchAndReAttemptDc.lat, ReDispatchAndReAttemptDc.lg);
                        //tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.RedispatchAndOrderCancelVerifyingOTP);
                        //context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                        tripPlannerConfirmedOrders.WorkingStatus = statusValue;
                        context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                        result = true;
                    }
                }
                if (result)
                {
                    context.Commit();
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Notification Successfully Sent!!"
                    };
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Notification not sent!!"
                    };
                }
            }
            return res;
        }

        // Use Old OTP Generate Method on App for Below 
        #region ReDispatReAttemptConfirmOtp
        [Route("ReDispatReAttemptAndOrderCancelConfirmOtp")]
        [HttpPost]
        public HttpResponseMessage ReDispatReAttemptAndOrderCancelConfirmOtp(OrderConfirmOtpDc OrderConfirmOtp)
        {
            NewDeliveryResDTO res;
            //if (ordersToProcess.Any(x => x == OrderConfirmOtp.OrderId))
            //{
            //    res = new NewDeliveryResDTO()
            //    {
            //        op = null,
            //        Message = "Order #: " + OrderConfirmOtp.OrderId + " is already in process..",
            //        Status = false
            //    };
            //    return Request.CreateResponse(HttpStatusCode.OK, res);
            //}
            //else
            //{
            //    ordersToProcess.Add(OrderConfirmOtp.OrderId);
            //}
            try
            {

                var msg = "";
                bool Status = false;
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var context = new AuthContext())
                    {
                        OrderMaster ODMaster = null;
                        var ODM = context.OrderDispatchedMasters.Where(x => x.OrderId == OrderConfirmOtp.OrderId).Include("orderDetails").FirstOrDefault();
                        ODMaster = context.DbOrderMaster.Where(x => x.OrderId == OrderConfirmOtp.OrderId).Include("orderDetails").FirstOrDefault();
                        var People = context.Peoples.Where(x => x.PeopleID == ODM.DBoyId).FirstOrDefault();
                        var DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == ODM.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault();
                        if (DeliveryIssuance.Status == "Accepted")
                        {
                            DeliveryIssuance.Status = "Pending";
                            DeliveryIssuance.UpdatedDate = indianTime;
                            context.Entry(DeliveryIssuance).State = EntityState.Modified;
                        }
                        else if (DeliveryIssuance.Status == "Pending")
                        {
                        }
                        else
                        {
                            Status = false;
                            res = new NewDeliveryResDTO()
                            {
                                op = null,
                                Message = "Not Delivered due to Assignment in status : " + DeliveryIssuance.Status,
                                Status = Status
                            };
                            //ordersToProcess.RemoveAll(x => x == OrderConfirmOtp.OrderId);
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        if (OrderConfirmOtp != null && People != null && DeliveryIssuance.DeliveryIssuanceId > 0 && ODM != null)
                        {
                            //var ODM = context.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == obj.OrderDispatchedMasterId).Include("orderDetails").SingleOrDefault();
                            var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(z => z.DeliveryIssuanceId == DeliveryIssuance.DeliveryIssuanceId && z.OrderId == OrderConfirmOtp.OrderId).FirstOrDefault();
                            //var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == DeliveryIssuance.DeliveryIssuanceId && x.OrderId == OrderConfirmOtp.OrderId);
                            //var Ordercancellation = context.DeptOrderCancellationDb.Where(o => o.OrderId == OrderConfirmOtp.OrderId).FirstOrDefault();
                            //DeptOrderCancellation deptOrderCacellation = new DeptOrderCancellation();
                            //deptOrderCacellation.OrderId = ODM.OrderId;

                            //if (AssignmentRechangeOrder != null)
                            //{
                            //    AssignmentRechangeOrder.Status = 0;
                            //    AssignmentRechangeOrder.ModifiedDate = indianTime;
                            //    AssignmentRechangeOrder.ModifiedBy = People.PeopleID;
                            //    context.Entry(AssignmentRechangeOrder).State = EntityState.Modified;
                            //}

                            #region Damage Order status Update
                            if (ODMaster.OrderType == 6 && ODM.invoice_no != null)
                            {
                                var DOM = context.DamageOrderMasterDB.Where(x => x.invoice_no == ODM.invoice_no).SingleOrDefault();

                                if (DOM != null)
                                {
                                    DOM.UpdatedDate = indianTime;
                                    DOM.Status = OrderConfirmOtp.Status;
                                    context.Entry(DOM).State = EntityState.Modified;
                                }
                            }
                            #endregion
                            if (OrderConfirmOtp.Status == "Delivery Canceled" && ODM.Status != "Delivery Canceled")
                            {
                                OrderDeliveryMaster.Status = OrderConfirmOtp.Status;
                                OrderDeliveryMaster.comments = OrderConfirmOtp.comments;
                                //OrderDeliveryMaster.ChequeDate = obj.ChequeDate;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = OrderConfirmOtp.DeliveryLat;//added on 08/07/02019 
                                OrderDeliveryMaster.DeliveryLng = OrderConfirmOtp.DeliveryLng;
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;
                                //done on 28/02/2020 by PoojaZ //start
                                if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Late Delivery (OPS)" || OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                {
                                    //string query = "select sp.PeopleID  from OrderMasters a inner join Customers c on a.CustomerId=c.CustomerId and a.orderid = " + OrderDeliveryMaster.OrderId + "  inner join People sp on a.SalesPersonId=sp.PeopleID";
                                    //var salesLeadId = context.Database.SqlQuery<int?>(query).FirstOrDefault();
                                    //if (salesLeadId.HasValue && salesLeadId.Value > 0)
                                    //{
                                    //    deptOrderCacellation.SalesLeadId = salesLeadId.Value;
                                    //    deptOrderCacellation.OrderId = OrderDeliveryMaster.OrderId;
                                    //    deptOrderCacellation.ChargePoint = 1;
                                    //    deptOrderCacellation.IsActive = true;
                                    //    deptOrderCacellation.IsDeleted = false;
                                    //    deptOrderCacellation.IsEmailSend = false;
                                    //    deptOrderCacellation.CreatedDate = DateTime.Now;
                                    //    deptOrderCacellation.CreatedBy = People.PeopleID;
                                    //    if (OrderDeliveryMaster.comments == "Dont Want")
                                    //    {
                                    //        deptOrderCacellation.DepId = 6;
                                    //    }
                                    //    else if (OrderDeliveryMaster.comments == "Late Delivery (OPS)")
                                    //    {
                                    //        deptOrderCacellation.DepId = 33;
                                    //    }
                                    //    else if (OrderDeliveryMaster.comments == "Damage Expiry (Pur)")
                                    //    {
                                    //        deptOrderCacellation.DepId = 29;
                                    //    }
                                    //    context.DeptOrderCancellationDb.Add(deptOrderCacellation);
                                    //}
                                }
                                //done on 28/02/2020 by PoojaZ //end

                                if (ODM != null)
                                {
                                    ODM.Status = OrderConfirmOtp.Status;
                                    ODM.CanceledStatus = OrderConfirmOtp.Status;
                                    ODM.comments = OrderConfirmOtp.comments;
                                    ODM.UpdatedDate = indianTime;
                                    ODM.DeliveryLat = OrderConfirmOtp.DeliveryLat;//added on 08/07/02019 
                                    ODM.DeliveryLng = OrderConfirmOtp.DeliveryLng;
                                    ODM.IsReAttempt = false;
                                    context.Entry(ODM).State = EntityState.Modified;
                                    //pz//start
                                    if (OrderDeliveryMaster.comments == "Dont Want" || OrderDeliveryMaster.comments == "Price Issue (sales)")
                                    {
                                        if (!context.CustomerWalletHistoryDb.Any(x => x.OrderId == ODM.OrderId && x.NewOutWAmount == -100 && x.Through == "From Order Cancelled"))
                                        {
                                            Wallet wlt = context.WalletDb.Where(c => c.CustomerId == ODM.CustomerId).SingleOrDefault();

                                            CustomerWalletHistory CWH = new CustomerWalletHistory();
                                            CWH.WarehouseId = ODM.WarehouseId;
                                            CWH.CompanyId = ODM.CompanyId;
                                            CWH.CustomerId = wlt.CustomerId;
                                            CWH.NewAddedWAmount = 0;
                                            CWH.NewOutWAmount = -100;
                                            CWH.OrderId = ODM.OrderId;
                                            CWH.Through = "From Order Cancelled";
                                            CWH.TotalWalletAmount = wlt.TotalAmount - 100;
                                            CWH.CreatedDate = indianTime;
                                            CWH.UpdatedDate = indianTime;
                                            context.CustomerWalletHistoryDb.Add(CWH);

                                            wlt.TotalAmount -= 100;
                                            wlt.TransactionDate = indianTime;
                                            context.Entry(wlt).State = EntityState.Modified;
                                        }
                                    }
                                    ///pz///             

                                    #region Order Master History for Status Delivery Canceled
                                    OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                    OrderMasterHistories.orderid = ODM.OrderId;
                                    OrderMasterHistories.Status = ODM.Status;
                                    OrderMasterHistories.Reasoncancel = null;
                                    OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                    OrderMasterHistories.DeliveryIssuanceId = DeliveryIssuance.DeliveryIssuanceId;
                                    OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                    OrderMasterHistories.userid = People.PeopleID;
                                    OrderMasterHistories.CreatedDate = DateTime.Now;
                                    OrderMasterHistories.Description = OrderConfirmOtp.comments;
                                    context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                    #endregion
                                    ODMaster.Status = OrderConfirmOtp.Status;
                                    ODMaster.comments = OrderConfirmOtp.comments;
                                    ODMaster.UpdatedDate = indianTime;
                                    context.Entry(ODMaster).State = EntityState.Modified;
                                    foreach (var detail in ODMaster.orderDetails)
                                    {
                                        detail.Status = OrderConfirmOtp.Status;
                                        detail.UpdatedDate = indianTime;
                                        context.Entry(detail).State = EntityState.Modified;
                                    }
                                    //#region KKRequestReturnCancelOrder
                                    //var requestData =  context.KKReturnReplaceRequests.Where(x => x.NewGeneratedOrderId == OrderConfirmOtp.OrderId && x.IsActive && x.IsDeleted == false).ToList();
                                    //var requestIds = requestData.Select(x => x.Id).ToList();
                                    //var returnDetails = context.KKReturnReplaceDetails.Where(y => requestIds.Contains(y.KKRRRequestId) && y.Status == 5).ToList();
                                    //KKReturnReplaceDetail kKReturnReplaceDetail = new KKReturnReplaceDetail();
                                    //requestData.ForEach(x =>
                                    //{
                                    //    x.IsActive = false;
                                    //    x.IsDeleted = true;
                                    //    context.Entry(x).State = EntityState.Modified;
                                    //});
                                    //returnDetails.ForEach(x =>
                                    //{
                                    //    x.Status = 8;//Return Cancelled(Delivered Cancelled);
                                    //    context.Entry(x).State = EntityState.Modified;
                                    //});
                                    //#endregion

                                }
                                #region New Delivery Optimization process
                                if (OrderConfirmOtp.TripPlannerConfirmedDetailId > 0)
                                {
                                    var tripPlannerConfirmedDetailId = new SqlParameter("@TripPlannerConfirmedDetailId", OrderConfirmOtp.TripPlannerConfirmedDetailId);
                                    var OrderId = new SqlParameter("@OrderId", ODM.OrderId);
                                    var userId = new SqlParameter("@userid", OrderConfirmOtp.userId);
                                    var DeliveryLat = new SqlParameter("@DeliveryLat", OrderConfirmOtp.DeliveryLat != null ? OrderConfirmOtp.DeliveryLat.Value : 0);
                                    var DeliveryLng = new SqlParameter("@DeliveryLng", OrderConfirmOtp.DeliveryLat != null ? OrderConfirmOtp.DeliveryLng.Value : 0);
                                    var WorkingStatus = new SqlParameter("@WorkingStatus", 5);//Convert.ToInt32(WorKingStatus.failed);
                                    var OrderStatus = new SqlParameter("@OrderStatus", 2);//Convert.ToInt32(OrderStatusEnum.DeliveryCanceled);
                                    context.Database.ExecuteSqlCommand("EXEC [Operation].[TripPlanner_SingleOrderUpdateForLastMileApp] " +
                                        "@TripPlannerConfirmedDetailId,@OrderId,@userid,@DeliveryLat,@DeliveryLng,@WorkingStatus,@OrderStatus",
                                        tripPlannerConfirmedDetailId, OrderId, userId, DeliveryLat, DeliveryLng, WorkingStatus, OrderStatus);
                                }
                                #region old Code
                                //if (OrderConfirmOtp.TripPlannerConfirmedDetailId > 0)
                                //{
                                //    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == OrderConfirmOtp.TripPlannerConfirmedDetailId).FirstOrDefault();
                                //    var orderids = tripPlannerConfirmedDetails.CommaSeparatedOrderList.Split(',').Select(Int32.Parse).ToList();
                                //    var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).ToList();
                                //    bool IsShipped = true;
                                //    if (orderDispatchedMasters.Any() && orderDispatchedMasters != null)
                                //    {
                                //        foreach (var item in orderDispatchedMasters)
                                //        {
                                //            if (item.Status == "Shipped")
                                //            {
                                //                IsShipped = false;
                                //            }
                                //        }
                                //        tripPlannerConfirmedDetails.IsProcess = IsShipped;
                                //        var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
                                //        if (tripPlannerVehicle != null)
                                //        {
                                //            var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => orderids.Contains((int)x.OrderId) && x.TripPlannerConfirmedDetailId == OrderConfirmOtp.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                                //            var singleOrder = tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault();
                                //            if (tripPlannerConfirmedDetails.IsProcess)
                                //            {
                                //                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                //                tripPlannerVehicle.ModifiedDate = indianTime;
                                //                tripPlannerVehicle.ModifiedBy = OrderConfirmOtp.userId;
                                //                tripPlannerVehicle.CurrentLat = OrderConfirmOtp.DeliveryLat ?? 0;
                                //                tripPlannerVehicle.CurrentLng = OrderConfirmOtp.DeliveryLng ?? 0;
                                //                tripPlannerVehicle.ReminingTime -= (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault().TimeInMins);
                                //                tripPlannerVehicle.TravelTime += (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault().TimeInMins);
                                //                tripPlannerVehicle.DistanceLeft -= tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                //                tripPlannerVehicle.DistanceTraveled += tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                //                /////////////////////
                                //                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                //                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                //                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                //                tripPlannerVehicleHistory.CurrentServingOrderId = ODM.OrderId;
                                //                tripPlannerVehicleHistory.RecordTime = indianTime;
                                //                tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                //                tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                //                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                //                tripPlannerVehicleHistory.CreatedBy = OrderConfirmOtp.userId;
                                //                tripPlannerVehicleHistory.IsActive = true;
                                //                tripPlannerVehicleHistory.IsDeleted = false;
                                //                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                //                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                //                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                //                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.Completed);
                                //                tripPlannerConfirmedDetails.ServeEndTime = DateTime.Now;
                                //                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                //                singleOrder.WorkingStatus = Convert.ToInt32(WorKingStatus.failed);
                                //                singleOrder.OrderStatus = Convert.ToInt32(OrderStatusEnum.DeliveryCanceled);
                                //                context.Entry(singleOrder).State = EntityState.Modified;
                                //            }
                                //            else
                                //            {
                                //                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                //                tripPlannerVehicle.ModifiedDate = indianTime;
                                //                tripPlannerVehicle.ModifiedBy = OrderConfirmOtp.userId;
                                //                tripPlannerVehicle.CurrentLat = OrderConfirmOtp.DeliveryLat ?? 0;
                                //                tripPlannerVehicle.CurrentLng = OrderConfirmOtp.DeliveryLng ?? 0;
                                //                tripPlannerVehicle.ReminingTime -= tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                //                tripPlannerVehicle.TravelTime += tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                //                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                //                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                //                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                //                tripPlannerVehicleHistory.CurrentServingOrderId = ODM.OrderId;
                                //                tripPlannerVehicleHistory.RecordTime = indianTime;
                                //                tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                //                tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                //                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                //                tripPlannerVehicleHistory.CreatedBy = OrderConfirmOtp.userId;
                                //                tripPlannerVehicleHistory.IsActive = true;
                                //                tripPlannerVehicleHistory.IsDeleted = false;
                                //                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                //                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                //                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                //                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.ReachedDistination);
                                //                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                //                singleOrder.WorkingStatus = Convert.ToInt32(WorKingStatus.failed);
                                //                singleOrder.OrderStatus = Convert.ToInt32(OrderStatusEnum.DeliveryCanceled);
                                //                context.Entry(singleOrder).State = EntityState.Modified;
                                //            }
                                //        }
                                //    }
                                //}
                                #endregion
                                #endregion
                                //}//end dt:-9/3/2020
                                #region Remove Notification
                                var peopleSentNotifications = context.PeopleSentNotifications.Where(x => x.OrderId == ODM.OrderId && x.IsDeleted == false).ToList();
                                if (peopleSentNotifications != null && peopleSentNotifications.Any())
                                {
                                    peopleSentNotifications.ForEach(x =>
                                    {
                                        x.IsDeleted = true;
                                        context.Entry(x).State = EntityState.Modified;
                                    });
                                }
                                #endregion
                            }
                            else if (ODM.Status == "Delivery Canceled")
                            {
                                Status = true;
                                msg = "Order already Delivery Canceled";
                                res = new NewDeliveryResDTO()
                                {
                                    op = OrderConfirmOtp,
                                    Message = msg,
                                    Status = Status
                                };
                                //ordersToProcess.RemoveAll(x => x == OrderConfirmOtp.OrderId);

                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                            else if (OrderConfirmOtp.Status == "Delivery Redispatch" && ODM.Status != "Delivery Redispatch")
                            {
                                OrderDeliveryMaster.Status = OrderConfirmOtp.Status;
                                // OrderDeliveryMaster.comments = OrderConfirmOtp.comments;
                                OrderDeliveryMaster.UpdatedDate = indianTime;
                                OrderDeliveryMaster.DeliveryLat = OrderConfirmOtp.DeliveryLat;//added on 08/07/02019 
                                OrderDeliveryMaster.DeliveryLng = OrderConfirmOtp.DeliveryLng;
                                if (OrderConfirmOtp.ReAttempt)
                                {
                                    OrderDeliveryMaster.comments = ODM.comments;
                                }
                                else
                                {
                                    OrderDeliveryMaster.comments = OrderConfirmOtp.comments;
                                }
                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;

                                ODM.Status = "Delivery Redispatch";
                                ODM.ReDispatchedStatus = "Delivery Redispatch";
                                //ODM.comments = OrderConfirmOtp.comments;
                                ODM.UpdatedDate = indianTime;
                                ODM.DeliveryLat = OrderConfirmOtp.DeliveryLat;//added on 08/07/02019 
                                ODM.DeliveryLng = OrderConfirmOtp.DeliveryLng;
                                ODM.ReDispatchedDate = DateTime.Now;
                                DateTime? NextRedispatchedDate = DateTime.ParseExact(OrderConfirmOtp.NextRedispatchedDate, "dd/MM/yyyy", null);
                                ODM.NextRedispatchDate = NextRedispatchedDate;
                                if (OrderConfirmOtp.ReAttempt)
                                {
                                    ODM.ReAttemptCount = ODM.ReAttemptCount + 1;
                                    ODM.IsReAttempt = true;
                                }
                                else
                                {
                                    ODM.IsReAttempt = false;
                                    ODM.ReDispatchCount = ODM.ReDispatchCount + 1;
                                    ODM.comments = OrderConfirmOtp.comments;
                                }
                                context.Entry(ODM).State = EntityState.Modified;
                                #region Order Master History for Status Delivery Redispatch

                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();

                                OrderMasterHistories.orderid = ODM.OrderId;
                                OrderMasterHistories.Status = ODM.Status;
                                OrderMasterHistories.Reasoncancel = null;
                                OrderMasterHistories.Warehousename = ODM.WarehouseName;
                                OrderMasterHistories.DeliveryIssuanceId = DeliveryIssuance.DeliveryIssuanceId;
                                OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                                OrderMasterHistories.userid = People.PeopleID;
                                OrderMasterHistories.CreatedDate = DateTime.Now;
                                OrderMasterHistories.Description = OrderConfirmOtp.comments;

                                if (OrderConfirmOtp.ReAttempt)
                                {
                                    OrderMasterHistories.IsReAttempt = true;
                                    OrderMasterHistories.Description = ODM.comments;
                                }
                                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                #endregion
                                ODMaster.Status = OrderConfirmOtp.Status;
                                ODMaster.comments = OrderConfirmOtp.comments;
                                ODMaster.UpdatedDate = indianTime;
                                ODMaster.ReDispatchCount = ODM.ReDispatchCount;
                                context.Entry(ODMaster).State = EntityState.Modified;
                                foreach (var detail in ODMaster.orderDetails)
                                {
                                    detail.Status = OrderConfirmOtp.Status;
                                    detail.UpdatedDate = indianTime;
                                    context.Entry(detail).State = EntityState.Modified;

                                }


                                //var RO = context.RedispatchWarehouseDb.Where(x => x.OrderId == ODM.OrderId && x.DboyMobileNo == ODM.DboyMobileNo).FirstOrDefault();
                                //if (RO != null)
                                //{
                                //    RO.Status = OrderConfirmOtp.Status;
                                //    RO.comments = OrderConfirmOtp.comments;
                                //    RO.ReDispatchCount = ODM.ReDispatchCount;
                                //    RO.UpdatedDate = indianTime;
                                //    context.Entry(RO).State = EntityState.Modified;
                                //}
                                //else
                                //{
                                //    RO = new RedispatchWarehouse();
                                //    RO.active = true;
                                //    RO.comments = OrderConfirmOtp.comments;
                                //    RO.CompanyId = ODM.CompanyId;
                                //    RO.CreatedDate = indianTime;
                                //    RO.UpdatedDate = indianTime;
                                //    RO.DboyMobileNo = People.Mobile;
                                //    RO.DboyName = People.DisplayName;
                                //    RO.Deleted = false;
                                //    RO.OrderDispatchedMasterId = ODM.OrderDispatchedMasterId;
                                //    RO.OrderId = ODM.OrderId;
                                //    RO.WarehouseId = ODM.WarehouseId;
                                //    RO.ReDispatchCount = ODM.ReDispatchCount;
                                //    RO.Status = OrderConfirmOtp.Status;
                                //    context.RedispatchWarehouseDb.Add(RO);
                                //}
                                #region New Delivery Optimization process
                                if (OrderConfirmOtp.TripPlannerConfirmedDetailId > 0)
                                {
                                    var tripPlannerConfirmedDetailId = new SqlParameter("@TripPlannerConfirmedDetailId", OrderConfirmOtp.TripPlannerConfirmedDetailId);
                                    var OrderId = new SqlParameter("@OrderId", ODM.OrderId);
                                    var userId = new SqlParameter("@userid", OrderConfirmOtp.userId);
                                    var DeliveryLat = new SqlParameter("@DeliveryLat", OrderConfirmOtp.DeliveryLat != null ? OrderConfirmOtp.DeliveryLat.Value : 0);
                                    var DeliveryLng = new SqlParameter("@DeliveryLng", OrderConfirmOtp.DeliveryLat != null ? OrderConfirmOtp.DeliveryLng.Value : 0);
                                    var WorkingStatus = new SqlParameter("@WorkingStatus", 5);//Convert.ToInt32(WorKingStatus.failed);
                                    var OrderStatus = new SqlParameter("@OrderStatus", (OrderConfirmOtp.ReAttempt ? 4 : 3));//Convert.ToInt32(OrderStatusEnum.DeliveryRedispatch);
                                    context.Database.ExecuteSqlCommand("EXEC [Operation].[TripPlanner_SingleOrderUpdateForLastMileApp] " +
                                        "@TripPlannerConfirmedDetailId,@OrderId,@userid,@DeliveryLat,@DeliveryLng,@WorkingStatus,@OrderStatus",
                                        tripPlannerConfirmedDetailId, OrderId, userId, DeliveryLat, DeliveryLng, WorkingStatus, OrderStatus);
                                }
                                #region old code
                                //if (OrderConfirmOtp.TripPlannerConfirmedDetailId > 0)
                                //{
                                //    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == OrderConfirmOtp.TripPlannerConfirmedDetailId).FirstOrDefault();
                                //    var orderids = tripPlannerConfirmedDetails.CommaSeparatedOrderList.Split(',').Select(Int32.Parse).ToList();
                                //    var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).ToList();
                                //    bool IsShipped = true;
                                //    if (orderDispatchedMasters.Any() && orderDispatchedMasters != null)
                                //    {
                                //        foreach (var item in orderDispatchedMasters)
                                //        {
                                //            if (item.Status == "Shipped")
                                //            {
                                //                IsShipped = false;
                                //            }
                                //        }
                                //        tripPlannerConfirmedDetails.IsProcess = IsShipped;
                                //        var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
                                //        if (tripPlannerVehicle != null)
                                //        {
                                //            var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => orderids.Contains((int)x.OrderId) && x.TripPlannerConfirmedDetailId == OrderConfirmOtp.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                                //            var singleOrder = tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault();
                                //            if (tripPlannerConfirmedDetails.IsProcess)
                                //            {
                                //                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                //                tripPlannerVehicle.ModifiedDate = indianTime;
                                //                tripPlannerVehicle.ModifiedBy = OrderConfirmOtp.userId;
                                //                tripPlannerVehicle.CurrentLat = OrderConfirmOtp.DeliveryLat ?? 0;
                                //                tripPlannerVehicle.CurrentLng = OrderConfirmOtp.DeliveryLng ?? 0;
                                //                tripPlannerVehicle.ReminingTime -= (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault().TimeInMins);
                                //                tripPlannerVehicle.TravelTime += (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault().TimeInMins);
                                //                tripPlannerVehicle.DistanceLeft -= tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                //                tripPlannerVehicle.DistanceTraveled += tripPlannerConfirmedDetails.TotalDistanceInMeter;
                                //                /////////////////////
                                //                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                //                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                //                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                //                tripPlannerVehicleHistory.CurrentServingOrderId = OrderConfirmOtp.OrderId;
                                //                tripPlannerVehicleHistory.RecordTime = indianTime;
                                //                tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                //                tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                //                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                //                tripPlannerVehicleHistory.CreatedBy = OrderConfirmOtp.userId;
                                //                tripPlannerVehicleHistory.IsActive = true;
                                //                tripPlannerVehicleHistory.IsDeleted = false;
                                //                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                //                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                //                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                //                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.Completed);
                                //                tripPlannerConfirmedDetails.ServeEndTime = DateTime.Now;
                                //                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                //                singleOrder.WorkingStatus = Convert.ToInt32(WorKingStatus.failed);
                                //                singleOrder.OrderStatus = Convert.ToInt32(OrderStatusEnum.DeliveryRedispatch);
                                //                context.Entry(singleOrder).State = EntityState.Modified;
                                //            }
                                //            else
                                //            {
                                //                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
                                //                tripPlannerVehicle.ModifiedDate = indianTime;
                                //                tripPlannerVehicle.ModifiedBy = OrderConfirmOtp.userId;
                                //                tripPlannerVehicle.CurrentLat = OrderConfirmOtp.DeliveryLat ?? 0;
                                //                tripPlannerVehicle.CurrentLng = OrderConfirmOtp.DeliveryLng ?? 0;
                                //                tripPlannerVehicle.ReminingTime -= tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                //                tripPlannerVehicle.TravelTime += tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
                                //                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
                                //                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                //                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                //                tripPlannerVehicleHistory.CurrentServingOrderId = ODM.OrderId;
                                //                tripPlannerVehicleHistory.RecordTime = indianTime;
                                //                tripPlannerVehicleHistory.Lat = OrderConfirmOtp.DeliveryLat ?? 0;
                                //                tripPlannerVehicleHistory.Lng = OrderConfirmOtp.DeliveryLng ?? 0;
                                //                tripPlannerVehicleHistory.CreatedDate = indianTime;
                                //                tripPlannerVehicleHistory.CreatedBy = OrderConfirmOtp.userId;
                                //                tripPlannerVehicleHistory.IsActive = true;
                                //                tripPlannerVehicleHistory.IsDeleted = false;
                                //                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                //                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                //                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
                                //                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.ReachedDistination);
                                //                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                //                singleOrder.WorkingStatus = Convert.ToInt32(WorKingStatus.failed);
                                //                singleOrder.OrderStatus = Convert.ToInt32(OrderStatusEnum.DeliveryRedispatch);
                                //                context.Entry(singleOrder).State = EntityState.Modified;
                                //            }
                                //        }
                                //    }
                                //}
                                #endregion
                                #endregion

                                if (ODM.Deliverydate.Date == DateTime.Now.Date && !OrderConfirmOtp.ReAttempt && !OrderConfirmOtp.IsReturnOrder)
                                {
                                    MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges>();
                                    var customerRedispatchCharges = mongoDbHelper.Select(x => x.MinValue < ODM.GrossAmount && x.MaxValue >= ODM.GrossAmount && x.WarehouseId == ODM.WarehouseId).FirstOrDefault();

                                    if (customerRedispatchCharges != null && customerRedispatchCharges.RedispatchCharges > 0)
                                    {
                                        Wallet wlt = context.WalletDb.FirstOrDefault(c => c.CustomerId == ODM.CustomerId);
                                        if (wlt != null)
                                        {
                                            CustomerWalletHistory CWH = new CustomerWalletHistory();
                                            CWH.WarehouseId = ODM.WarehouseId;
                                            CWH.CompanyId = ODM.CompanyId;
                                            CWH.CustomerId = wlt.CustomerId;
                                            CWH.NewAddedWAmount = 0;
                                            CWH.NewOutWAmount = -(customerRedispatchCharges.RedispatchCharges * 10);
                                            CWH.OrderId = ODM.OrderId;
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
                            else if (ODM.Status == "Delivery Redispatch")
                            {
                                Status = true;
                                msg = "Order already Redispatch";
                                res = new NewDeliveryResDTO()
                                {
                                    op = OrderConfirmOtp,
                                    Message = msg,
                                    Status = Status
                                };
                                //ordersToProcess.RemoveAll(x => x == OrderConfirmOtp.OrderId);
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }

                            #region By Kapil
                            ValidateOTPForOrder helper = new ValidateOTPForOrder();

                            helper.ValidateOTPForOrders(OrderConfirmOtp.Otp, ODM.OrderId, ODM.Status, context);

                            #endregion

                            var otp = context.OrderDeliveryOTP.Where(x => x.OrderId == ODM.OrderId && x.IsActive == true).ToList();
                            if (otp != null && otp.Any())
                            {
                                otp.ForEach(x =>
                                {
                                    x.IsActive = false;
                                    context.Entry(x).State = EntityState.Modified;

                                });
                            }


                        }
                        else
                        {
                            res = new NewDeliveryResDTO()
                            {
                                op = null,
                                Message = "Not Delivered",
                                Status = Status
                            };
                            // ordersToProcess.RemoveAll(x => x == OrderConfirmOtp.OrderId);
                            return Request.CreateResponse(HttpStatusCode.OK, res);
                        }
                        if (context.Commit() > 0)
                        {
                            #region stock Hit on poc
                            //for currentstock                           
                            if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivery Canceled" && !OrderConfirmOtp.IsReturnOrder)
                            {

                                MultiStockHelper<OnDeliveryCanceledStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryCanceledStockEntryDc>();
                                List<OnDeliveryCanceledStockEntryDc> DeliveryCanceledCStockList = new List<OnDeliveryCanceledStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    else if (ODMaster.OrderType == 9) //9 Non Sellable stock
                                    {
                                        RefStockCode = "N";
                                    }
                                    DeliveryCanceledCStockList.Add(new OnDeliveryCanceledStockEntryDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = People.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        RefStockCode = RefStockCode,
                                        IsDeliveryRedispatchCancel = false
                                    });
                                }
                                if (DeliveryCanceledCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryCanceledCStockList, "Stock_OnDeliveryCancel_New", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new NewDeliveryResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Canceled",
                                            Status = Status
                                        };
                                        //ordersToProcess.RemoveAll(x => x == OrderConfirmOtp.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }

                            }
                            else if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivery Redispatch" && !OrderConfirmOtp.IsReturnOrder)
                            {

                                MultiStockHelper<OnDeliveryRedispatchedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryRedispatchedStockEntryDc>();
                                List<OnDeliveryRedispatchedStockEntryDc> DeliveryRedispatchedCStockList = new List<OnDeliveryRedispatchedStockEntryDc>();
                                foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
                                {
                                    var RefStockCode = ODMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; }
                                    else if (ODMaster.OrderType == 6) //6 Damage stock
                                    {
                                        RefStockCode = "D";
                                    }
                                    else if (ODMaster.OrderType == 9) //9 Non Sellable stock
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
                                if (DeliveryRedispatchedCStockList.Any())
                                {
                                    bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryRedispatchedCStockList, "Stock_OnDeliveryRedispatch", context, dbContextTransaction);
                                    if (!IsUpdated)
                                    {
                                        Status = false;
                                        dbContextTransaction.Dispose();
                                        res = new NewDeliveryResDTO()
                                        {
                                            op = null,
                                            Message = "Order Not Delivery Redispatch",
                                            Status = Status
                                        };
                                        //ordersToProcess.RemoveAll(x => x == OrderConfirmOtp.OrderId);
                                        return Request.CreateResponse(HttpStatusCode.OK, res);
                                    }
                                }
                            }
                            #endregion

                            dbContextTransaction.Complete();
                            Status = true;
                        }
                        else
                        {
                            dbContextTransaction.Dispose();
                            Status = false;
                        }
                        res = new NewDeliveryResDTO()
                        {
                            op = OrderConfirmOtp,
                            Message = "Success",
                            Status = Status
                        };
                        //ordersToProcess.RemoveAll(x => x == OrderConfirmOtp.OrderId);
                        //return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                if (OrderConfirmOtp.Status == "Delivery Canceled")
                {
                    using (var db = new AuthContext())
                    {
                        ConfigureNotifyHelper configureNotifyHelper = new ConfigureNotifyHelper();
                        configureNotifyHelper.GetSalesActionList(OrderConfirmOtp.userId, OrderConfirmOtp.Status, OrderConfirmOtp.OrderId, OrderConfirmOtp.TripPlannerConfirmedDetailId, db);
                        db.Commit();
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                //ordersToProcess.RemoveAll(x => x == OrderConfirmOtp.OrderId);
                throw ex;
            }
        }
        #endregion
        #endregion

        #region Cancled Single Order
        // call NotifyDeliveryCancled
        [Route("NotifyDeliveryCancled")]
        [HttpPost]
        public async Task<NotifyDeliveryCancledResDc> NotifyDeliveryCancled(NotifyDeliveryCancledDc NotifyDeliveryCancledDc)
        {
            NotifyDeliveryCancledResDc res;
            var salesPersonDc = new salesPersonDc();

            bool IsSendApprovalNotification = false;
            using (var context = new AuthContext())
            {
                int userId = GetLoginUserId();

                //send notification  => call GetActionList
                string OnAction = "Delivery Canceled";
                var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == NotifyDeliveryCancledDc.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == NotifyDeliveryCancledDc.TripPlannerConfirmedDetailId && x.OrderId == NotifyDeliveryCancledDc.OrderId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (tripPlannerConfirmedDetails != null && tripPlannerConfirmedOrders != null)
                {
                    ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                    IsSendApprovalNotification = await helper.GetActionList(userId, OnAction, NotifyDeliveryCancledDc.OrderId, NotifyDeliveryCancledDc.TripPlannerConfirmedDetailId, null, NotifyDeliveryCancledDc.Reason, context);
                    tripPlannerConfirmedDetails.IsVisible = true;
                    tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.NotifyDeliveryCancelled);
                    context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                    tripPlannerConfirmedOrders.WorkingStatus = Convert.ToInt32(WorKingStatus.DeliveryCanceled);
                    context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                    context.Commit();
                    var peopleSentNotifications = context.PeopleSentNotifications.Where(x => x.OrderId == NotifyDeliveryCancledDc.OrderId && x.OrderStatus == "Delivery Canceled").OrderByDescending(x => x.Id).FirstOrDefault();


                    //var param = new SqlParameter("@OrderId", NotifyDeliveryCancledDc.OrderId);
                    //var param1 = new SqlParameter("@orderStatus", Status);
                    //var orderMobiledetail = context.Database.SqlQuery<OrderMobiledetail>("Exec GetOrderMobileDetailsOTP  @OrderId,@orderStatus", param, param1).FirstOrDefault();

                    if (peopleSentNotifications != null)
                    {
                        var people = context.Peoples.FirstOrDefault(x => x.PeopleID == peopleSentNotifications.ToPeopleId);
                        if (people != null)
                        {
                            salesPersonDc.SalePersonMobile = people.Mobile;
                            salesPersonDc.SalePersonName = people.DisplayName;
                        }

                    }
                }
                if (IsSendApprovalNotification)
                {
                    res = new NotifyDeliveryCancledResDc()
                    {
                        salesPersonDc = salesPersonDc,
                        Status = IsSendApprovalNotification,
                        Message = "Notification Successfully Sent!!"
                    };
                }
                else
                {
                    res = new NotifyDeliveryCancledResDc()
                    {
                        salesPersonDc = null,
                        Status = IsSendApprovalNotification,
                        Message = "Notification Not Sent!!"
                    };
                }
            }
            return res;
        }

        #region SaveVideo By Anshika
        [AllowAnonymous]
        [Route("NotifyDeliveryAction")]
        [HttpPost]
        public async Task<NotifyDeliveryCancledResDc> NotifyDeliveryAction(NotifyDeliveryActionDC notifyDeliveryActionDC)
        {
            NotifyDeliveryCancledResDc res;
            var salesPersonDc = new salesPersonDc();

            string msg = "";
            bool result = false;
            bool IsSendApprovalNotification = false;
            using (var context = new AuthContext())
            {
                int userId = GetLoginUserId();

                string OnAction = notifyDeliveryActionDC.Action;
                var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == notifyDeliveryActionDC.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == notifyDeliveryActionDC.TripPlannerConfirmedDetailId && x.OrderId == notifyDeliveryActionDC.OrderId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (tripPlannerConfirmedDetails != null && tripPlannerConfirmedOrders != null)
                {
                    var ordertype = context.DbOrderMaster.Where(x => x.OrderId == tripPlannerConfirmedOrders.OrderId).FirstOrDefault();

                    if (ordertype.OrderType != 6 && ordertype.OrderType != 9)
                    {
                        ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                        IsSendApprovalNotification = await helper.GetActionList(userId, OnAction, notifyDeliveryActionDC.OrderId, notifyDeliveryActionDC.TripPlannerConfirmedDetailId, null, notifyDeliveryActionDC.Reason, context, notifyDeliveryActionDC.VideoUrl, false);

                    }
                    // Only use for Delivery Canceled
                    if (OnAction == "Delivery Canceled")
                    {
                        tripPlannerConfirmedDetails.IsVisible = true;
                        tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.NotifyDeliveryCancelled);
                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                        tripPlannerConfirmedOrders.WorkingStatus = Convert.ToInt32(WorKingStatus.DeliveryCanceled);
                        context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                        context.Commit();


                        if (ordertype.OrderType == 6 || ordertype.OrderType == 9)
                        {

                            string sRandomOTP = "";
                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                            sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                            List<int> OrderIDs = new List<int>();
                            if (true)
                            {
                                var ExistsOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == tripPlannerConfirmedOrders.OrderId && x.IsActive == true).OrderByDescending(x => x.CreatedDate).ToList();
                                if (ExistsOTPs != null && ExistsOTPs.Any())
                                {
                                    foreach (var ExistsOTP in ExistsOTPs)
                                    {
                                        ExistsOTP.IsActive = false;
                                        ExistsOTP.ModifiedDate = DateTime.Now;
                                        ExistsOTP.ModifiedBy = userId;
                                        context.Entry(ExistsOTP).State = EntityState.Modified;
                                    }
                                }

                                if (!string.IsNullOrEmpty(sRandomOTP))
                                {
                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                    {
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        OrderId = (int)tripPlannerConfirmedOrders.OrderId,
                                        OTP = sRandomOTP,
                                        Status = "Delivery Canceled",
                                        UserType = "HQ Operation",
                                        IsUsed = false,
                                        UserId = 0,
                                        lat = notifyDeliveryActionDC.lat,
                                        lg = notifyDeliveryActionDC.lg,
                                        IsVideoSeen = false,
                                        VideoUrl = ""

                                    };
                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                    context.Commit();

                                    res = new NotifyDeliveryCancledResDc()
                                    {
                                        salesPersonDc = salesPersonDc,
                                        Status = IsSendApprovalNotification,
                                        Message = "Otp sent succesfully!!"
                                    };
                                }
                            }
                        }
                        else
                        {
                            var peopleSentNotifications1 = context.PeopleSentNotifications.Where(x => x.OrderId == notifyDeliveryActionDC.OrderId && x.OrderStatus == "Delivery Canceled").OrderByDescending(x => x.Id).FirstOrDefault();

                            if (peopleSentNotifications1 != null)
                            {
                                var people = context.Peoples.FirstOrDefault(x => x.PeopleID == peopleSentNotifications1.ToPeopleId);
                                if (people != null)
                                {
                                    salesPersonDc.SalePersonMobile = people.Mobile;
                                    salesPersonDc.SalePersonName = people.DisplayName;
                                }
                            }
                        }
                    }
                    else if (OnAction == "Delivery Redispatch")
                    {
                        tripPlannerConfirmedDetails.IsVisible = true;
                        tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.NotifyDeliveryRedispatch);
                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                        tripPlannerConfirmedOrders.WorkingStatus = Convert.ToInt32(WorKingStatus.DeliveryRedispatch);
                        context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                        context.Commit();

                        if (ordertype.OrderType == 6 || ordertype.OrderType == 9)
                        {

                            string sRandomOTP = "";
                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                            sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                            List<int> OrderIDs = new List<int>();
                            if (true)
                            {
                                var ExistsOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == tripPlannerConfirmedOrders.OrderId && x.IsActive == true).OrderByDescending(x => x.CreatedDate).ToList();
                                if (ExistsOTPs != null && ExistsOTPs.Any())
                                {
                                    foreach (var ExistsOTP in ExistsOTPs)
                                    {
                                        ExistsOTP.IsActive = false;
                                        ExistsOTP.ModifiedDate = DateTime.Now;
                                        ExistsOTP.ModifiedBy = userId;
                                        context.Entry(ExistsOTP).State = EntityState.Modified;
                                    }
                                }

                                if (!string.IsNullOrEmpty(sRandomOTP))
                                {
                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                    {
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        OrderId = (int)tripPlannerConfirmedOrders.OrderId,
                                        OTP = sRandomOTP,
                                        Status = "Delivery Redispatch",
                                        UserType = "HQ Operation",
                                        IsUsed = false,
                                        UserId = 0,
                                        lat = notifyDeliveryActionDC.lat,
                                        lg = notifyDeliveryActionDC.lg,
                                        IsVideoSeen = false,
                                        VideoUrl = ""

                                    };
                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                    context.Commit();

                                    res = new NotifyDeliveryCancledResDc()
                                    {
                                        salesPersonDc = salesPersonDc,
                                        Status = IsSendApprovalNotification,
                                        Message = "Otp send succesfully for non-sellable and damage order!!"
                                    };

                                }


                            }
                        }
                        else
                        {
                            var peopleSentNotifications2 = context.PeopleSentNotifications.Where(x => x.OrderId == notifyDeliveryActionDC.OrderId && x.OrderStatus == "Delivery Redispatch").OrderByDescending(x => x.Id).FirstOrDefault();

                            if (peopleSentNotifications2 != null)
                            {
                                var people = context.Peoples.FirstOrDefault(x => x.PeopleID == peopleSentNotifications2.ToPeopleId);
                                if (people != null)
                                {
                                    salesPersonDc.SalePersonMobile = people.Mobile;
                                    salesPersonDc.SalePersonName = people.DisplayName;
                                }
                            }
                        }
                    }
                    else if (OnAction == "ReAttempt")
                    {
                        tripPlannerConfirmedDetails.IsVisible = true;
                        tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.NotifyReAttempt);
                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                        tripPlannerConfirmedOrders.WorkingStatus = Convert.ToInt32(WorKingStatus.ReAttempt);
                        context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                        context.Commit();
                        var peopleSentNotifications3 = context.PeopleSentNotifications.Where(x => x.OrderId == notifyDeliveryActionDC.OrderId && x.OrderStatus == "ReAttempt").OrderByDescending(x => x.Id).FirstOrDefault();

                        if (peopleSentNotifications3 != null)
                        {
                            var people = context.Peoples.FirstOrDefault(x => x.PeopleID == peopleSentNotifications3.ToPeopleId);
                            if (people != null)
                            {
                                salesPersonDc.SalePersonMobile = people.Mobile;
                                salesPersonDc.SalePersonName = people.DisplayName;
                            }
                        }
                    }
                }
                if (IsSendApprovalNotification)
                {
                    res = new NotifyDeliveryCancledResDc()
                    {
                        salesPersonDc = salesPersonDc,
                        Status = IsSendApprovalNotification,
                        Message = "Notification Successfully Sent!!"
                    };
                }
                else
                {
                    res = new NotifyDeliveryCancledResDc()
                    {
                        salesPersonDc = null,
                        Status = IsSendApprovalNotification,
                        Message = "Notification Not Sent!!"
                    };
                }
            }
            return res;
        }

        [AllowAnonymous]
        [Route("UpdateNotifyVideoSeen")]
        [HttpGet]
        public async Task<bool> UpdateNotifyVideoSeen(long Id, string OrderStatus, int RequestId)
        {
            using (var context = new AuthContext())
            {
                bool result = false;
                if (OrderStatus == "Delivery Canceled" && RequestId == 1)
                {
                    var UpdateVideostatus = context.PeopleSentNotifications.Where(x => x.Id == Id).FirstOrDefault();
                    if (UpdateVideostatus != null)
                    {
                        UpdateVideostatus.IsVideoSeen = true;
                        context.Entry(UpdateVideostatus).State = EntityState.Modified;
                        result = context.Commit() > 0;
                    }
                }
                else
                {
                    var UpdateStatus = context.OrderDeliveryOTP.Where(x => x.Id == Id).FirstOrDefault();
                    if (UpdateStatus != null)
                    {
                        UpdateStatus.IsVideoSeen = true;
                        context.Entry(UpdateStatus).State = EntityState.Modified;
                        result = context.Commit() > 0;
                    }
                }
                return result;
            }
        }
        #endregion

        // Use Same Method for Generate Otp 


        [Route("DeliveryCancledConfirmOtp")]
        [HttpPost]
        public ResponceDc DeliveryCancledConfirmOtp(DeliveryCancledConfirmOtpDc DeliveryCancledConfirmOtpDc)
        {
            ResponceDc res = null;
            bool result = false;
            using (var context = new AuthContext())
            {
                if (context.OrderDispatchedMasters.Any(x => x.OrderId == DeliveryCancledConfirmOtpDc.OrderId && x.Status == "Shipped"))
                {
                    string OnAction = "Delivery Canceled";
                    int userId = GetLoginUserId();
                    result = GenerateOTPForDeliveryAPP(DeliveryCancledConfirmOtpDc.OrderId, OnAction, userId, DeliveryCancledConfirmOtpDc.lat, DeliveryCancledConfirmOtpDc.lg);
                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == DeliveryCancledConfirmOtpDc.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (tripPlannerConfirmedDetails != null)
                    {
                        tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.RedispatchAndOrderCancelVerifyingOTP);
                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                        context.Commit();
                    }
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "OTP Successfully Sent!!"
                    };
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Order Already Delivered!!"
                    };
                }
            }
            return res;
        }


        [Route("NotifyDeliveryAPPForOrderCancled")]
        [HttpGet]
        [AllowAnonymous]
        public ResponceDc NotifyDeliveryAPPForOrderCancled(long TripPlannerConfirmedDetailId, int orderId, int Peopleid, int status, string orderStatus, long? Id = null, string VideoUrl = "", bool IsVideoSeen = false)
        {
            int ApproveNotifyTimeLeftInMinute = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);
            bool checkTrpStatus = false;
            // status = 1 Approved, 0= Reject
            ResponceDc res = null;
            using (var context = new AuthContext())
            {
                if (context.OrderDispatchedMasters.Any(x => x.OrderId == orderId && x.Status == "Shipped"))
                {
                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                    if ((int)CustomerTripStatusEnum.unloading == tripPlannerConfirmedDetails.CustomerTripStatus)
                    {
                        checkTrpStatus = true;
                    }

                    else if ((int)CustomerTripStatusEnum.CollectingPayment == tripPlannerConfirmedDetails.CustomerTripStatus)
                    {
                        checkTrpStatus = true;
                    }
                    else if ((int)CustomerTripStatusEnum.VerifyingOTP == tripPlannerConfirmedDetails.CustomerTripStatus)
                    {
                        checkTrpStatus = true;
                    }
                    if (!checkTrpStatus)
                    {
                        string FCMKey = "";
                        IAppConfiguration AppConfiguration = new BaseAppConfiguration();
                        if (Id != null)
                        {
                            var peopleSentNotification = context.PeopleSentNotifications.Where(x => x.OrderId == orderId && !x.IsApproved && !x.IsRejected && !x.IsDeleted && x.Id == Id).FirstOrDefault();
                            if (peopleSentNotification.CreatedDate.AddMinutes(ApproveNotifyTimeLeftInMinute) >= DateTime.Now)
                            {
                                //Get Delivery boy FCM ID for Notify
                                ///status update order trip
                                var OrderId = new SqlParameter("@OrderId", orderId);
                                string FCMID = context.Database.SqlQuery<string>(" EXEC GetDboyFCMId @OrderId", OrderId).FirstOrDefault();


                                FCMKey = ConfigurationManager.AppSettings["DeliveryFcmApiKey"];
                                if (status == 1)
                                {
                                    string Msg = "Order Action Notification";
                                    string notify_type = "Order Action Updated Notification";
                                    if (FCMID != null)
                                    {
                                        AppConfiguration.SendNotificationForApprovalDeliveyApp(FCMID, Msg, orderId, FCMKey, orderStatus, status, notify_type);
                                    }
                                    if (GenerateOTPForDeliveryAPP(orderId, orderStatus, Peopleid, null, null, VideoUrl, IsVideoSeen))
                                    {
                                        tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.RedispatchAndOrderCancelVerifyingOTP);
                                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                    }
                                }
                                else
                                {
                                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == TripPlannerConfirmedDetailId && x.OrderId == orderId).FirstOrDefault();
                                    if (tripPlannerConfirmedOrders != null)
                                    {
                                        string Msg = "Order Reject Action Notification";
                                        string notify_type = "Sales Person Order Reject Notification";
                                        AppConfiguration.SendNotificationForApprovalDeliveyApp(FCMID, Msg, orderId, FCMKey, orderStatus, status, notify_type);
                                        tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.ReachedDistination);
                                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                        tripPlannerConfirmedOrders.WorkingStatus = Convert.ToInt32(WorKingStatus.Pending);
                                        context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                                    }
                                }

                            }
                            if (peopleSentNotification != null)
                            {
                                if (status == 1)
                                {
                                    peopleSentNotification.IsApproved = true;
                                    peopleSentNotification.ApprovedBy = Peopleid;
                                    peopleSentNotification.ApprovedDate = DateTime.Now;
                                }
                                else
                                {
                                    peopleSentNotification.IsRejected = true;
                                    peopleSentNotification.RejectedBy = Peopleid;
                                    peopleSentNotification.RejectedDate = DateTime.Now;
                                }
                                context.Entry(peopleSentNotification).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                        else
                        {
                            var peopleSentNotification = context.PeopleSentNotifications.Where(x => x.OrderId == orderId && !x.IsApproved && !x.IsRejected && !x.IsDeleted).FirstOrDefault();
                            if (peopleSentNotification.CreatedDate.AddMinutes(ApproveNotifyTimeLeftInMinute) >= DateTime.Now)
                            {
                                //Get Delivery boy FCM ID for Notify
                                ///status update order trip
                                var OrderId = new SqlParameter("@OrderId", orderId);
                                string FCMID = context.Database.SqlQuery<string>(" EXEC GetDboyFCMId @OrderId", OrderId).FirstOrDefault();
                                if (FCMID != null)
                                {

                                    FCMKey = ConfigurationManager.AppSettings["DeliveryFcmApiKey"];
                                    if (status == 1)
                                    {
                                        peopleSentNotification.NotificationType = 1;
                                        string Msg = "Order Action Notification";
                                        string notify_type = "Order Action Updated Notification";
                                        AppConfiguration.SendNotificationForApprovalDeliveyApp(FCMID, Msg, orderId, FCMKey, orderStatus, status, notify_type);
                                        if (GenerateOTPForDeliveryAPP(orderId, orderStatus, Peopleid, null, null))
                                        {
                                            tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.RedispatchAndOrderCancelVerifyingOTP);
                                            context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                        }
                                    }
                                    else
                                    {
                                        peopleSentNotification.NotificationType = 1;
                                        var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == TripPlannerConfirmedDetailId && x.OrderId == orderId).FirstOrDefault();
                                        if (tripPlannerConfirmedOrders != null)
                                        {
                                            string Msg = "Order Reject Action Notification";
                                            string notify_type = "Sales Person Order Reject Notification";
                                            AppConfiguration.SendNotificationForApprovalDeliveyApp(FCMID, Msg, orderId, FCMKey, orderStatus, status, notify_type);
                                            tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.ReachedDistination);
                                            context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                            tripPlannerConfirmedOrders.WorkingStatus = Convert.ToInt32(WorKingStatus.Pending);
                                            context.Entry(tripPlannerConfirmedOrders).State = EntityState.Modified;
                                        }
                                    }
                                }
                            }
                            if (peopleSentNotification != null)
                            {
                                if (status == 1)
                                {
                                    peopleSentNotification.IsApproved = true;
                                    peopleSentNotification.ApprovedBy = Peopleid;
                                    peopleSentNotification.ApprovedDate = DateTime.Now;
                                }
                                else
                                {
                                    peopleSentNotification.IsRejected = true;
                                    peopleSentNotification.RejectedBy = Peopleid;
                                    peopleSentNotification.RejectedDate = DateTime.Now;
                                }
                                context.Entry(peopleSentNotification).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = true,
                            Message = "Your order status send successfully."
                        };
                    }
                    else
                    {
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = false,
                            Message = "Order in Progress Collecting Payment"
                        };
                    }
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = false,
                        Message = "Order Already Delivered!!"
                    };
                }
            }
            return res;
        }


        #endregion

        #region  AllRedispatReattemptGenerateOtp
        [Route("NotifyALLReDispatchAndReAttempt")]
        [HttpPost]
        public async Task<ResponceDc> NotifyALLReDispatchAndReAttempt(ReDispatchAndReAttemptDc ReDispatchAndReAttemptDc)
        {
            int userId = GetLoginUserId();
            ResponceDc res;
            bool result = false;
            int statusValue = 0;
            using (var context = new AuthContext())
            {
                DateTime? NextRedispatchedDate = DateTime.ParseExact(ReDispatchAndReAttemptDc.NextRedispatchedDate, "dd/MM/yyyy", null);
                //send notification  => call GetActionList
                var ResAllRedispatReattemptConfirmOtp = new AllRedispatReattemptrequest()
                {
                    TripPlannerConfirmedDetailId = ReDispatchAndReAttemptDc.TripPlannerConfirmedDetailId,
                    IsRedispatch = ReDispatchAndReAttemptDc.IsReAttempt,
                    Reason = ReDispatchAndReAttemptDc.Reason,
                    NextRedispatchedDate = NextRedispatchedDate,
                    lat = ReDispatchAndReAttemptDc.lat,
                    lg = ReDispatchAndReAttemptDc.lg
                };
                var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == ReDispatchAndReAttemptDc.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                if (tripPlannerConfirmedOrders != null)
                {
                    string OnAction = "", status = "Delivery Redispatch";
                    if (!ReDispatchAndReAttemptDc.IsReAttempt)
                    {
                        OnAction = "Delivery Redispatch";
                        statusValue = Convert.ToInt32(WorKingStatus.DeliveryRedispatch);
                        ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                        var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).Distinct();
                        try
                        {
                            foreach (var item in orderids)
                            {
                                bool IsSendNotification = await helper.GetActionList(userId, OnAction, (int)item, ReDispatchAndReAttemptDc.TripPlannerConfirmedDetailId, NextRedispatchedDate, ReDispatchAndReAttemptDc.Reason, context);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error AllNotification Redispatch Order" + ex.Message.ToString());
                        }

                        var data = AllRedispatReattemptGenerateOtp(ResAllRedispatReattemptConfirmOtp);
                        tripPlannerConfirmedOrders.ForEach(x =>
                        {
                            x.WorkingStatus = statusValue;
                            context.Entry(x).State = EntityState.Modified;
                        });

                        result = true;
                    }
                    else
                    {
                        OnAction = "ReAttempt";
                        statusValue = Convert.ToInt32(WorKingStatus.ReAttempt);


                        var data = AllReattemptGenerateOtp(ResAllRedispatReattemptConfirmOtp);
                        tripPlannerConfirmedOrders.ForEach(x =>
                        {
                            x.WorkingStatus = statusValue;
                            context.Entry(x).State = EntityState.Modified;
                        });

                        result = true;
                    }
                    context.Commit();
                }
                if (result)
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Notification Successfully Sent!!"
                    };
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Notification not sent!!"
                    };
                }
            }
            return res;
        }

        [Route("AllRedispatReattemptGenerateOtp")]
        [HttpPost]
        public ResponceDc AllRedispatReattemptGenerateOtp(AllRedispatReattemptrequest allRedispatReattemptrequest)
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
                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == allRedispatReattemptrequest.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == allRedispatReattemptrequest.TripPlannerConfirmedDetailId).FirstOrDefault();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).ToList();
                    if (tripPlannerConfirmedOrders != null && tripPlannerConfirmedOrders.Any() && tripPlannerConfirmedDetails != null)
                    {
                        string status = "Delivery Redispatch";
                        string requireStatus = "Delivered,Delivery Redispatch,Delivery Canceled Request,Delivery Canceled";
                        if (requireStatus.Split(',').ToList().Contains(status))
                        {
                            // " is Your Shopkirana Verification Code. :)";
                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                            string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                            if (!string.IsNullOrEmpty(sRandomOTP))
                            {
                                foreach (var OrderId in orderids)
                                {
                                    var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive);
                                    if (orderDeliveryOTPs != null)
                                    {
                                        foreach (var orderDeliveryOTP in orderDeliveryOTPs)
                                        {
                                            orderDeliveryOTP.ModifiedDate = DateTime.Now;
                                            orderDeliveryOTP.ModifiedBy = userId;
                                            orderDeliveryOTP.IsActive = false;
                                            //context.OrderDeliveryOTP.Attach(orderDeliveryOTP);
                                            context.Entry(orderDeliveryOTP).State = EntityState.Modified;
                                        }
                                    }
                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                    {
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        OrderId = (int)OrderId,
                                        OTP = sRandomOTP,
                                        Status = status,
                                        lat = allRedispatReattemptrequest.lat,
                                        lg = allRedispatReattemptrequest.lg
                                    };
                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                }
                                var result = context.Commit() > 0;
                            }


                            //string query = "select a.warehouseid,sp.Mobile salespersonmobile,sp.FcmId salespersonfcmid,agent.Mobile agentmobile,agent.FcmId agentfcmid,c.Mobile customermobile,c.fcmId customerfmcid  from OrderMasters a Inner join  Customers c on  a.CustomerId=c.CustomerId  left join People agent on c.AgentCode=agent.PeopleID  Outer Apply (Select top 1 sp.Mobile,sp.FcmId  from OrderDetails od  Inner join People sp on od.ExecutiveId=sp.PeopleID  and od.OrderId=a.OrderId ) sp  Where  a.orderid  in (" + string.Join(",", orderids) + ")";
                            //var orderMobiledetail = context.Database.SqlQuery<ControllerV1.OrderMobiledetail>(query).FirstOrDefault();

                            var orderIdDt = new System.Data.DataTable();
                            orderIdDt.Columns.Add("IntValue");
                            foreach (var item in orderids)
                            {
                                var dr = orderIdDt.NewRow();
                                dr["IntValue"] = item;
                                orderIdDt.Rows.Add(dr);
                            }

                            var param = new SqlParameter("OrderIds", orderIdDt);
                            param.SqlDbType = System.Data.SqlDbType.Structured;
                            param.TypeName = "dbo.IntValues";

                            //string query = "select a.warehouseid,sp.Mobile salespersonmobile,sp.FcmId salespersonfcmid,agent.Mobile agentmobile,agent.FcmId agentfcmid,c.Mobile customermobile,c.fcmId customerfmcid  from OrderMasters a Inner join  Customers c on  a.CustomerId=c.CustomerId  left join People agent on c.AgentCode=agent.PeopleID  Outer Apply (Select top 1 sp.Mobile,sp.FcmId  from OrderDetails od  Inner join People sp on od.ExecutiveId=sp.PeopleID  and od.OrderId=a.OrderId ) sp  Where  a.orderid =" + OrderId;
                            var orderMobiledetail = context.Database.SqlQuery<ControllerV1.OrderMobiledetail>("Exec GetMultiOrderMobileDetailsOTP  @OrderIds", param).FirstOrDefault();


                            if (orderMobiledetail != null)
                            {
                                var DeliveryRedispatchSmsToSales = ConfigurationManager.AppSettings["DeliveryRedispatchSmsToSales"];
                                if (!string.IsNullOrEmpty(DeliveryRedispatchSmsToSales) && DeliveryRedispatchSmsToSales == "true")
                                {
                                    List<ControllerV1.SalesLeadMobile> SalesLeadMobile = new List<ControllerV1.SalesLeadMobile>();
                                    string query = "select a.Mobile,a.FcmId from People a inner join AspNetUsers b on a.Email=b.Email and a.WarehouseId=" + orderMobiledetail.warehouseid + " and a.Active=1 and a.Deleted=0 inner join AspNetUserRoles c on b.id=c.UserId  inner join AspNetRoles d on c.RoleId=d.Id and d.Name='Hub sales lead'";
                                    SalesLeadMobile = context.Database.SqlQuery<ControllerV1.SalesLeadMobile>(query).ToList();

                                    //if (!string.IsNullOrEmpty(orderMobiledetail.agentmobile))
                                    //{
                                    //    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.SalesApp, "Order_Delivery_Redispatch");
                                    //    string Message = dltSMS == null ? "" : dltSMS.Template;
                                    //    Message = Message.Replace("{#var1#}", sRandomOTP);
                                    //    Message = Message.Replace("{#var2#}", string.Join(",", orderids));
                                    //    if (dltSMS != null)
                                    //        Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.agentmobile, Message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                                    //}

                                    if (SalesLeadMobile != null && SalesLeadMobile.Any())
                                    {
                                        foreach (var item in SalesLeadMobile)
                                        {
                                            if (!string.IsNullOrEmpty(item.Mobile))
                                            {
                                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.SalesApp, "Order_Delivery_Redispatch");
                                                string Message = dltSMS == null ? "" : dltSMS.Template;
                                                Message = Message.Replace("{#var1#}", sRandomOTP);
                                                Message = Message.Replace("{#var2#}", string.Join(",", orderids));
                                                if (dltSMS != null)
                                                    Common.Helpers.SendSMSHelper.SendSMS(item.Mobile, Message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                            }
                                        }
                                    }
                                }
                                //}
                                //if (!string.IsNullOrEmpty(orderMobiledetail.salespersonmobile))
                                //{
                                //    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.SalesApp, "Order_Delivery_Redispatch");
                                //    string Message = dltSMS == null ? "" : dltSMS.Template;
                                //    Message = Message.Replace("{#var1#}", sRandomOTP);
                                //    Message = Message.Replace("{#var2#}", string.Join(",", orderids));
                                //    if (dltSMS != null)
                                //        Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.salespersonmobile, Message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                //}

                                if (!string.IsNullOrEmpty(orderMobiledetail.customermobile))
                                {
                                    if (orderMobiledetail.Deliverydate == DateTime.Now)
                                    {
                                        MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges>();
                                        var customerRedispatchCharges = mongoDbHelper.Select(x => x.MinValue < orderMobiledetail.OrderAmount && x.MaxValue >= orderMobiledetail.OrderAmount && x.WarehouseId == orderMobiledetail.warehouseid).FirstOrDefault();
                                        string amount = "";
                                        if (customerRedispatchCharges != null && customerRedispatchCharges.RedispatchCharges > 0)
                                        {
                                            amount = (customerRedispatchCharges.RedispatchCharges * orderids.Count()).ToString();
                                        }

                                        string message = "";
                                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Redispatch");
                                        message = dltSMS == null ? "" : dltSMS.Template;

                                        if (!string.IsNullOrEmpty(amount))
                                        {
                                            dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "RedispatchCharges");
                                            message = dltSMS == null ? "" : dltSMS.Template;
                                        }

                                        message = message.Replace("{#var1#}", sRandomOTP);
                                        message = message.Replace("{#var2#}", string.Join(",", orderids));
                                        message = message.Replace("{#var3#}", amount.ToString());
                                        if (dltSMS != null)
                                            Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                                    }
                                }


                                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.AllVerifyingOTP);
                                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                                context.Commit();
                                scope.Complete();
                                res = new ResponceDc()
                                {
                                    TripDashboardDC = null,
                                    Status = true,
                                    Message = "Success!!"
                                };
                            }
                        }
                    }
                    else
                    {
                        scope.Dispose();
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = false,
                            Message = "Data not found."
                        };
                    }
                }
            }
            return res;
        }

        public ResponceDc AllReattemptGenerateOtp(AllRedispatReattemptrequest allRedispatReattemptrequest)
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
                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == allRedispatReattemptrequest.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false).ToList();
                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == allRedispatReattemptrequest.TripPlannerConfirmedDetailId).FirstOrDefault();
                    var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).ToList();
                    if (tripPlannerConfirmedOrders != null && tripPlannerConfirmedOrders.Any() && tripPlannerConfirmedDetails != null)
                    {
                        string status = "Delivery Redispatch";
                        string requireStatus = "Delivered,Delivery Redispatch,Delivery Canceled Request,Delivery Canceled";
                        if (requireStatus.Split(',').ToList().Contains(status))
                        {
                            // " is Your Shopkirana Verification Code. :)";
                            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                            string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                            if (!string.IsNullOrEmpty(sRandomOTP))
                            {
                                foreach (var OrderId in orderids)
                                {
                                    var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive);
                                    if (orderDeliveryOTPs != null)
                                    {
                                        foreach (var orderDeliveryOTP in orderDeliveryOTPs)
                                        {
                                            orderDeliveryOTP.ModifiedDate = DateTime.Now;
                                            orderDeliveryOTP.ModifiedBy = userId;
                                            orderDeliveryOTP.IsActive = false;
                                            //context.OrderDeliveryOTP.Attach(orderDeliveryOTP);
                                            context.Entry(orderDeliveryOTP).State = EntityState.Modified;
                                        }
                                    }
                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                    {
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        OrderId = (int)OrderId,
                                        OTP = sRandomOTP,
                                        Status = status,
                                        lat = allRedispatReattemptrequest.lat,
                                        lg = allRedispatReattemptrequest.lg,
                                        UserType = "HQ Operation(ReAttempt)",
                                        UserId = 0
                                    };
                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                }
                                var result = context.Commit() > 0;
                            }

                            tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.AllVerifyingOTP);
                            context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                            context.Commit();
                            scope.Complete();
                            res = new ResponceDc()
                            {
                                TripDashboardDC = null,
                                Status = true,
                                Message = "Success!!"
                            };

                        }
                    }
                    else
                    {
                        scope.Dispose();
                        res = new ResponceDc()
                        {
                            TripDashboardDC = null,
                            Status = false,
                            Message = "Data not found."
                        };
                    }
                }
            }
            return res;
        }


        #region old Code
        //[Route("AllRedispatReattemptConfirmOtp")]
        //[HttpPost]
        //public ResponceDc AllRedispatReattemptConfirmOtp(AllRedispatReattemptConfirmOtp allRedispatReattemptConfirmOtp)
        //{
        //    ResponceDc res = new ResponceDc();
        //    int statusValue = 0;
        //    TransactionOptions option = new TransactionOptions();
        //    option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
        //    option.Timeout = TimeSpan.FromSeconds(90);
        //    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            if (!allRedispatReattemptConfirmOtp.ReAttempt)
        //            {                      
        //                statusValue = Convert.ToInt32(WorKingStatus.DeliveryRedispatch);
        //            }
        //            else
        //            {                      
        //                statusValue = Convert.ToInt32(WorKingStatus.ReAttempt);
        //            }
        //            string Status = "Delivery Redispatch";
        //            int userId = GetLoginUserId();
        //            var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == allRedispatReattemptConfirmOtp.TripPlannerConfirmedDetailId && x.WorkingStatus == statusValue && x.IsActive == true && x.IsDeleted == false).ToList();
        //            var orderids = tripPlannerConfirmedOrders.Select(x => x.OrderId).ToList();
        //            ordersToProcess.AddRange(orderids);
        //            //Otp matched any order
        //            // set status failed (tripplanner confirm order) 
        //            // detail me completed is process true
        //            // Order Master Redispacted  both case 
        //            try
        //            {
        //                var otp = context.OrderDeliveryOTP.Where(x => orderids.Contains(x.OrderId) && x.IsActive && x.OTP == allRedispatReattemptConfirmOtp.Otp).ToList();
        //                if (otp != null && otp.Any())
        //                {
        //                    var People = context.Peoples.Where(x => x.PeopleID == userId).FirstOrDefault();
        //                    var ODMs = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).Include("orderDetails").ToList();
        //                    List<OrderMaster> ODMasters = context.DbOrderMaster.Where(x => orderids.Contains(x.OrderId)).Include("orderDetails").ToList();
        //                    var DeliveryIssuanceIds = ODMs.Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).ToList();
        //                    var OrderDeliveryMasters = context.OrderDeliveryMasterDB.Where(z => DeliveryIssuanceIds.Contains(z.DeliveryIssuanceId) && orderids.Contains(z.OrderId)).Include("orderDetails").ToList();
        //                    if (!ODMs.Any(x => x.Status == Status))
        //                    {
        //                        foreach (var ODM in ODMs)
        //                        {
        //                            var ODMaster = ODMasters.FirstOrDefault(x => x.OrderId == ODM.OrderId);
        //                            var OrderDeliveryMaster = OrderDeliveryMasters.FirstOrDefault(x => x.OrderId == ODM.OrderId);
        //                            #region Damage Order status Update
        //                            if (ODMaster.OrderType == 6 && ODM.invoice_no != null)
        //                            {
        //                                var DOM = context.DamageOrderMasterDB.Where(x => x.invoice_no == ODM.invoice_no).SingleOrDefault();

        //                                if (DOM != null)
        //                                {
        //                                    DOM.UpdatedDate = indianTime;
        //                                    DOM.Status = Status;
        //                                    context.Entry(DOM).State = EntityState.Modified;
        //                                }
        //                            }
        //                            #endregion

        //                            if (Status == "Delivery Redispatch" && ODM.Status != "Delivery Redispatch")
        //                            {
        //                                OrderDeliveryMaster.Status = Status;
        //                                OrderDeliveryMaster.comments = allRedispatReattemptConfirmOtp.Reason;
        //                                OrderDeliveryMaster.UpdatedDate = indianTime;
        //                                OrderDeliveryMaster.DeliveryLat = allRedispatReattemptConfirmOtp.lat;//added on 08/07/02019 
        //                                OrderDeliveryMaster.DeliveryLng = allRedispatReattemptConfirmOtp.lg;
        //                                context.Entry(OrderDeliveryMaster).State = EntityState.Modified;

        //                                ODM.Status = "Delivery Redispatch";
        //                                ODM.ReDispatchedStatus = "Delivery Redispatch";
        //                                ODM.ReDispatchCount = ODM.ReDispatchCount + 1;
        //                                ODM.comments = allRedispatReattemptConfirmOtp.Reason;
        //                                ODM.UpdatedDate = indianTime;
        //                                ODM.DeliveryLat = allRedispatReattemptConfirmOtp.lat;//added on 08/07/02019 
        //                                ODM.DeliveryLng = allRedispatReattemptConfirmOtp.lg;
        //                                ODM.ReDispatchedDate = allRedispatReattemptConfirmOtp.NextRedispatchedDate;
        //                                if (allRedispatReattemptConfirmOtp.ReAttempt)
        //                                {
        //                                    ODM.ReAttemptCount = ODM.ReAttemptCount + 1;
        //                                    ODM.IsReAttempt = true;                                            
        //                                }
        //                                ODM.NextRedispatchDate = allRedispatReattemptConfirmOtp.NextRedispatchedDate;
        //                                context.Entry(ODM).State = EntityState.Modified;
        //                                #region Order Master History for Status Delivery Redispatch

        //                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();

        //                                OrderMasterHistories.orderid = ODM.OrderId;
        //                                OrderMasterHistories.Status = ODM.Status;
        //                                OrderMasterHistories.Reasoncancel = null;
        //                                OrderMasterHistories.Warehousename = ODM.WarehouseName;
        //                                OrderMasterHistories.DeliveryIssuanceId = OrderDeliveryMaster.DeliveryIssuanceId;
        //                                OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
        //                                OrderMasterHistories.userid = People.PeopleID;
        //                                OrderMasterHistories.CreatedDate = DateTime.Now;
        //                                context.OrderMasterHistoriesDB.Add(OrderMasterHistories);
        //                                #endregion
        //                                ODMaster.Status = Status;
        //                                ODMaster.comments = allRedispatReattemptConfirmOtp.Reason;
        //                                ODMaster.UpdatedDate = indianTime;
        //                                ODMaster.ReDispatchCount = ODM.ReDispatchCount;
        //                                context.Entry(ODMaster).State = EntityState.Modified;

        //                                foreach (var detail in ODMaster.orderDetails)
        //                                {
        //                                    detail.Status = Status;
        //                                    detail.UpdatedDate = indianTime;
        //                                    context.Entry(detail).State = EntityState.Modified;

        //                                }

        //                                //var RO = context.RedispatchWarehouseDb.Where(x => x.OrderId == ODM.OrderId && x.DboyMobileNo == People.Mobile).FirstOrDefault();
        //                                //if (RO != null)
        //                                //{
        //                                //    RO.Status = Status;
        //                                //    RO.comments = allRedispatReattemptConfirmOtp.Reason;
        //                                //    RO.ReDispatchCount = ODM.ReDispatchCount;
        //                                //    RO.UpdatedDate = indianTime;
        //                                //    context.Entry(RO).State = EntityState.Modified;

        //                                //}
        //                                //else
        //                                //{
        //                                //    RO = new RedispatchWarehouse();
        //                                //    RO.active = true;
        //                                //    RO.comments = allRedispatReattemptConfirmOtp.Reason;
        //                                //    RO.CompanyId = ODM.CompanyId;
        //                                //    RO.CreatedDate = indianTime;
        //                                //    RO.UpdatedDate = indianTime;
        //                                //    RO.DboyMobileNo = People.Mobile;
        //                                //    RO.DboyName = People.DisplayName;
        //                                //    RO.Deleted = false;
        //                                //    RO.OrderDispatchedMasterId = ODM.OrderDispatchedMasterId;
        //                                //    RO.OrderId = ODM.OrderId;
        //                                //    RO.WarehouseId = ODM.WarehouseId;
        //                                //    RO.ReDispatchCount = ODM.ReDispatchCount;
        //                                //    RO.Status = Status;
        //                                //    context.RedispatchWarehouseDb.Add(RO);
        //                                //}
        //                                #region New Delivery Optimization process

        //                                if (tripPlannerConfirmedOrders != null)
        //                                {
        //                                    var detialsId = tripPlannerConfirmedOrders.FirstOrDefault().TripPlannerConfirmedDetailId;
        //                                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == detialsId).FirstOrDefault();
        //                                    //var orderids = tripPlannerConfirmedDetails.CommaSeparatedOrderList.Split(',').Select(Int32.Parse).ToList();
        //                                    var orderDispatchedMasters = context.OrderDispatchedMasters.Where(x => orderids.Contains(x.OrderId)).ToList();
        //                                    bool IsShipped = true;
        //                                    if (orderDispatchedMasters.Any() && orderDispatchedMasters != null)
        //                                    {
        //                                        foreach (var item in orderDispatchedMasters)
        //                                        {
        //                                            if (item.Status == "Shipped")
        //                                            {
        //                                                IsShipped = false;
        //                                            }
        //                                        }
        //                                        tripPlannerConfirmedDetails.IsProcess = IsShipped;
        //                                        var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
        //                                        if (tripPlannerVehicle != null)
        //                                        {
        //                                            var tripPlannerOrderid = tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault();
        //                                            if (tripPlannerConfirmedDetails.IsProcess)
        //                                            {
        //                                                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
        //                                                tripPlannerVehicle.ModifiedDate = indianTime;
        //                                                tripPlannerVehicle.ModifiedBy = userId;
        //                                                tripPlannerVehicle.CurrentLat = allRedispatReattemptConfirmOtp.lat ?? 0;
        //                                                tripPlannerVehicle.CurrentLng = allRedispatReattemptConfirmOtp.lg ?? 0;
        //                                                tripPlannerVehicle.ReminingTime -= (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault().TimeInMins);
        //                                                tripPlannerVehicle.TravelTime += (tripPlannerConfirmedDetails.TotalTimeInMins - tripPlannerConfirmedOrders.Sum(x => x.TimeInMins)) + (tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).FirstOrDefault().TimeInMins);
        //                                                tripPlannerVehicle.DistanceLeft -= tripPlannerConfirmedDetails.TotalDistanceInMeter;
        //                                                tripPlannerVehicle.DistanceTraveled += tripPlannerConfirmedDetails.TotalDistanceInMeter;
        //                                                /////////////////////
        //                                                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
        //                                                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
        //                                                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
        //                                                tripPlannerVehicleHistory.CurrentServingOrderId = ODM.OrderId;
        //                                                tripPlannerVehicleHistory.RecordTime = indianTime;
        //                                                tripPlannerVehicleHistory.Lat = allRedispatReattemptConfirmOtp.lat ?? 0;
        //                                                tripPlannerVehicleHistory.Lng = allRedispatReattemptConfirmOtp.lg ?? 0;
        //                                                tripPlannerVehicleHistory.CreatedDate = indianTime;
        //                                                tripPlannerVehicleHistory.CreatedBy = userId;
        //                                                tripPlannerVehicleHistory.IsActive = true;
        //                                                tripPlannerVehicleHistory.IsDeleted = false;
        //                                                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
        //                                                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
        //                                                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
        //                                                tripPlannerOrderid.WorkingStatus = Convert.ToInt32(WorKingStatus.failed);
        //                                                context.Entry(tripPlannerOrderid).State = EntityState.Modified;
        //                                                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.Completed);
        //                                                context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
        //                                            }
        //                                            else
        //                                            {
        //                                                tripPlannerVehicle.CurrentStatus = (int)VehicleliveStatus.InTransit;
        //                                                tripPlannerVehicle.ModifiedDate = indianTime;
        //                                                tripPlannerVehicle.ModifiedBy = userId;
        //                                                tripPlannerVehicle.CurrentLat = allRedispatReattemptConfirmOtp.lat ?? 0;
        //                                                tripPlannerVehicle.CurrentLng = allRedispatReattemptConfirmOtp.lg ?? 0;
        //                                                tripPlannerVehicle.ReminingTime -= tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
        //                                                tripPlannerVehicle.TravelTime += tripPlannerConfirmedOrders.Where(x => x.OrderId == ODM.OrderId).Select(x => x.TimeInMins).FirstOrDefault();
        //                                                context.Entry(tripPlannerVehicle).State = EntityState.Modified;
        //                                                TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
        //                                                tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
        //                                                tripPlannerVehicleHistory.CurrentServingOrderId = ODM.OrderId;
        //                                                tripPlannerVehicleHistory.RecordTime = indianTime;
        //                                                tripPlannerVehicleHistory.Lat = allRedispatReattemptConfirmOtp.lat ?? 0;
        //                                                tripPlannerVehicleHistory.Lng = allRedispatReattemptConfirmOtp.lg ?? 0;
        //                                                tripPlannerVehicleHistory.CreatedDate = indianTime;
        //                                                tripPlannerVehicleHistory.CreatedBy = userId;
        //                                                tripPlannerVehicleHistory.IsActive = true;
        //                                                tripPlannerVehicleHistory.IsDeleted = false;
        //                                                tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
        //                                                tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
        //                                                context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
        //                                                tripPlannerOrderid.WorkingStatus = Convert.ToInt32(WorKingStatus.failed);
        //                                                context.Entry(tripPlannerOrderid).State = EntityState.Modified;
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                                #endregion
        //                            }
        //                        }
        //                        if (context.Commit() > 0)
        //                        {
        //                            #region stock Hit on poc
        //                            //for currentstock
        //                            MultiStockHelper<OnDeliveryRedispatchedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnDeliveryRedispatchedStockEntryDc>();
        //                            List<OnDeliveryRedispatchedStockEntryDc> DeliveryRedispatchedCStockList = new List<OnDeliveryRedispatchedStockEntryDc>();
        //                            foreach (var ODM in ODMs)
        //                            {
        //                                var ODMaster = ODMasters.FirstOrDefault(x => x.OrderId == ODM.OrderId);
        //                                if (ODMaster != null && ODMaster.OrderType != 5 && ODM.Status == "Delivery Redispatch")
        //                                {
        //                                    foreach (var StockHit in ODM.orderDetails.Where(x => x.qty > 0))
        //                                    {
        //                                        var RefStockCode = "C";
        //                                        bool isFree = ODMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
        //                                        if (isFree) { RefStockCode = "F"; }
        //                                        else if (ODMaster.OrderType == 6) //6 Damage stock
        //                                        {
        //                                            RefStockCode = "D";
        //                                        }
        //                                        DeliveryRedispatchedCStockList.Add(new OnDeliveryRedispatchedStockEntryDc
        //                                        {
        //                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
        //                                            OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
        //                                            OrderId = StockHit.OrderId,
        //                                            Qty = StockHit.qty,
        //                                            UserId = People.PeopleID,
        //                                            WarehouseId = StockHit.WarehouseId,
        //                                            RefStockCode = RefStockCode,
        //                                            IsDeliveryCancel = false
        //                                        });
        //                                    }
        //                                }
        //                            }
        //                            bool results = true;
        //                            if (DeliveryRedispatchedCStockList.Any())
        //                            {
        //                                bool IsUpdated = MultiStockHelpers.MakeEntry(DeliveryRedispatchedCStockList, "Stock_OnDeliveryRedispatch", context, scope);
        //                                if (!IsUpdated)
        //                                {
        //                                    scope.Dispose();
        //                                    results = false;
        //                                    res = new ResponceDc()
        //                                    {
        //                                        TripDashboardDC = null,
        //                                        Message = "Order Not Delivery Redispatch",
        //                                        Status = false
        //                                    };
        //                                    ordersToProcess.RemoveAll(x => orderids.Contains(x));
        //                                }
        //                            }
        //                            #endregion
        //                            if (results)
        //                            {
        //                                scope.Complete();
        //                                res = new ResponceDc()
        //                                {
        //                                    TripDashboardDC = null,
        //                                    Message = "Success",
        //                                    Status = true
        //                                };
        //                                ordersToProcess.RemoveAll(x => orderids.Contains(x));
        //                            }
        //                        }
        //                        else
        //                        {
        //                            scope.Dispose();
        //                            res = new ResponceDc()
        //                            {
        //                                TripDashboardDC = null,
        //                                Message = "Due to some error please after sometime",
        //                                Status = false
        //                            };
        //                            ordersToProcess.RemoveAll(x => orderids.Contains(x));                                   
        //                        }
        //                    }
        //                    else
        //                    {
        //                        res = new ResponceDc()
        //                        {
        //                            TripDashboardDC = null,
        //                            Message = "Some Order already Redispatch",
        //                            Status = true
        //                        };
        //                        ordersToProcess.RemoveAll(x => orderids.Contains(x));
        //                    }
        //                }
        //                else
        //                {
        //                    scope.Dispose();
        //                    res = new ResponceDc()
        //                    {
        //                        TripDashboardDC = null,
        //                        Status = false,
        //                        Message = "Incorrect OTP"
        //                    };
        //                    ordersToProcess.RemoveAll(x => orderids.Contains(x));
        //                }                       
        //            }
        //            catch (Exception ex)
        //            {
        //                ordersToProcess.RemoveAll(x => orderids.Contains(x));
        //                throw ex;
        //            }
        //        }
        //    }
        //    return res;
        //}
        #endregion
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
                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.Where(x => x.TripPlannerConfirmedDetailId == allRedispatReattemptConfirmOtp.TripPlannerConfirmedDetailId && x.WorkingStatus == statusValue && x.IsActive == true && x.IsDeleted == false).ToList();
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
                                    //var RO = context.RedispatchWarehouseDb.Where(x => x.OrderId == ODM.OrderId && x.DboyMobileNo == People.Mobile).FirstOrDefault();
                                    //if (RO != null)
                                    //{
                                    //    RO.Status = Status;
                                    //    RO.comments = allRedispatReattemptConfirmOtp.Reason;
                                    //    RO.ReDispatchCount = ODM.ReDispatchCount;
                                    //    RO.UpdatedDate = indianTime;
                                    //    context.Entry(RO).State = EntityState.Modified;

                                    //}
                                    //else
                                    //{
                                    //    RO = new RedispatchWarehouse();
                                    //    RO.active = true;
                                    //    RO.comments = allRedispatReattemptConfirmOtp.Reason;
                                    //    RO.CompanyId = ODM.CompanyId;
                                    //    RO.CreatedDate = indianTime;
                                    //    RO.UpdatedDate = indianTime;
                                    //    RO.DboyMobileNo = People.Mobile;
                                    //    RO.DboyName = People.DisplayName;
                                    //    RO.Deleted = false;
                                    //    RO.OrderDispatchedMasterId = ODM.OrderDispatchedMasterId;
                                    //    RO.OrderId = ODM.OrderId;
                                    //    RO.WarehouseId = ODM.WarehouseId;
                                    //    RO.ReDispatchCount = ODM.ReDispatchCount;
                                    //    RO.Status = Status;
                                    //    context.RedispatchWarehouseDb.Add(RO);
                                    //}
                                    #endregion
                                    #region New Delivery Optimization process

                                    if (tripPlannerConfirmedOrders != null)
                                    {
                                        var detialsId = tripPlannerConfirmedOrders.FirstOrDefault().TripPlannerConfirmedDetailId;
                                        var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.Where(x => x.Id == detialsId).FirstOrDefault();
                                        //var orderids = tripPlannerConfirmedDetails.CommaSeparatedOrderList.Split(',').Select(Int32.Parse).ToList();
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
                                            var tripPlannerVehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedDetails.TripPlannerConfirmedMasterId).FirstOrDefault();
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
                                                    TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                    tripPlannerVehicleHistory.CurrentServingOrderId = newOrderMaster.OrderId;
                                                    tripPlannerVehicleHistory.RecordTime = indianTime;
                                                    tripPlannerVehicleHistory.Lat = allRedispatReattemptConfirmOtp.lat ?? 0;
                                                    tripPlannerVehicleHistory.Lng = allRedispatReattemptConfirmOtp.lg ?? 0;
                                                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                    tripPlannerVehicleHistory.CreatedBy = allRedispatReattemptConfirmOtp.userId;
                                                    tripPlannerVehicleHistory.IsActive = true;
                                                    tripPlannerVehicleHistory.IsDeleted = false;
                                                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
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
                                                    TripPlannerVehicleHistory tripPlannerVehicleHistory = new TripPlannerVehicleHistory();
                                                    tripPlannerVehicleHistory.TripPlannerVehicleId = tripPlannerVehicle.Id;
                                                    tripPlannerVehicleHistory.CurrentServingOrderId = newOrderMaster.OrderId;
                                                    tripPlannerVehicleHistory.RecordTime = indianTime;
                                                    tripPlannerVehicleHistory.Lat = allRedispatReattemptConfirmOtp.lat ?? 0;
                                                    tripPlannerVehicleHistory.Lng = allRedispatReattemptConfirmOtp.lg ?? 0;
                                                    tripPlannerVehicleHistory.CreatedDate = indianTime;
                                                    tripPlannerVehicleHistory.CreatedBy = allRedispatReattemptConfirmOtp.userId;
                                                    tripPlannerVehicleHistory.IsActive = true;
                                                    tripPlannerVehicleHistory.IsDeleted = false;
                                                    tripPlannerVehicleHistory.RecoardStatus = (int)VehicleliveStatus.InTransit;
                                                    tripPlannerVehicleHistory.TripPlannerConfirmedDetailId = tripPlannerConfirmedDetails.Id;
                                                    context.TripPlannerVehicleHistoryDb.Add(tripPlannerVehicleHistory);
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
        #endregion

        #region SkipAll
        [Route("SkipAll")]
        [HttpPost]
        public ResponceDc SkipAll(long TripplannerConfirmdetailedId)
        {
            bool Status = false;
            DateTime modifiedDate = DateTime.Now;
            ResponceDc res = null;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {

                using (var context = new AuthContext())
                {
                    var detail = context.TripPlannerConfirmedDetails.FirstOrDefault(x => x.Id == TripplannerConfirmdetailedId);
                    //detail.CustomerTripStatus = (int)CustomerTripStatusEnum.Skip;
                    int userId = GetLoginUserId();
                    detail.ModifiedBy = userId;
                    detail.SequenceNo = 1000;
                    detail.IsSkip = true;
                    detail.ModifiedDate = modifiedDate;
                    // set status skip 
                    context.Commit();
                    var detailList = context.TripPlannerConfirmedDetails
                        .Where(x => x.TripPlannerConfirmedMasterId == detail.TripPlannerConfirmedMasterId && x.OrderCount > 0 && x.IsDeleted == false && x.IsActive == true)
                        .OrderBy(x => x.SequenceNo).ToList();
                    int seq = 1;
                    foreach (var item in detailList)
                    {
                        item.SequenceNo = seq++;
                    }

                    var warehouse = context.TripPlannerConfirmedDetails
                        .FirstOrDefault(x => x.TripPlannerConfirmedMasterId == detail.TripPlannerConfirmedMasterId && x.OrderCount == 0 && x.IsDeleted == false && x.IsActive == true);

                    warehouse.SequenceNo = seq++;
                    context.Commit();
                    Status = true;
                }
                if (Status)
                {
                    scope.Complete();
                    res = new ResponceDc()
                    {
                        Status = Status,
                        Message = "SkipAll SuccessFully!!",
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
        #endregion

        #region Remove Order
        [Route("RemoveOrder")]
        [HttpPost]
        public ResponceDc RemoveOrder(long TripPlannerConfirmOrderId, bool IsPaymentDone)
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
                    var tripPlannerConfirmedOrders = context.TripPlannerConfirmedOrders.FirstOrDefault(x => x.Id == TripPlannerConfirmOrderId);
                    if (tripPlannerConfirmedOrders != null)
                    {
                        var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.FirstOrDefault(x => x.Id == tripPlannerConfirmedOrders.TripPlannerConfirmedDetailId);
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
                                    var orderamount = context.TripPaymentResponseAppDb.Where(x => x.TripPlannerConfirmedOrderId == TripPlannerConfirmOrderId && x.Status == "Success" && ((x.PaymentFrom == "Cash" && x.IsOnline == false) || (x.PaymentFrom == "PayLater" && x.IsOnline == true)) && x.IsActive == true && x.IsDeleted == false).ToList().Sum(y => y.Amount);
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
                                var orderamount = context.TripPaymentResponseAppDb.Where(x => x.TripPlannerConfirmedOrderId == TripPlannerConfirmOrderId && x.Status == "Success" && ((x.PaymentFrom == "Cash" && x.IsOnline == false) || (x.PaymentFrom == "PayLater" && x.IsOnline == true)) && x.IsActive == true && x.IsDeleted == false).ToList().Sum(y => y.Amount);
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
                            var response = context.Database.ExecuteSqlCommand("EXEC [Operation].[TripPlanner_BackButtonOrderOTPScreen] @TripPlannerConfirmedOrderId", tripPlannerConfirmedOrderId);

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
        #endregion
        #region GetCurrentTripStatusOrder
        [Route("GetCurrentTripStatusOrder")]
        [HttpGet]
        public GetOrderResponceDc GetCurrentTripStatusOrder(long tripPlannerConfirmedDetailId, int workingStatus)
        {
            bool Status = false;
            GetOrderResponceDc res = null;
            TripPlannerConfirmedOrder tripPlannerConfirmedOrder = new TripPlannerConfirmedOrder();
            using (var context = new AuthContext())
            {
                tripPlannerConfirmedOrder = context.TripPlannerConfirmedOrders.FirstOrDefault(x => x.TripPlannerConfirmedDetailId == tripPlannerConfirmedDetailId && x.WorkingStatus == workingStatus && x.IsActive == true && x.IsDeleted == false);
                if (tripPlannerConfirmedOrder != null)
                {
                    Status = true;
                }
            }
            if (Status)
            {
                res = new GetOrderResponceDc()
                {
                    Status = Status,
                    Message = "Remove Order SuccessFully!!",
                    tripPlannerConfirmedOrder = tripPlannerConfirmedOrder
                };
            }
            else
            {
                res = new GetOrderResponceDc()
                {
                    Status = Status,
                    Message = "Someting went Wrong!!",
                    tripPlannerConfirmedOrder = null
                };
            }
            return res;
        }
        #endregion

        [Route("CheckRemaingOrderStatus")]
        [HttpGet]
        public ResCheckRemaingOrderStatusDC CheckRemaingOrderStatus(long TripplannerConfirmdetailedId)
        {
            ResCheckRemaingOrderStatusDC res = null;
            using (var db = new AuthContext())
            {
                var tripplannerConfirmdetailedId = new SqlParameter("@TripplannerConfirmdetailedId", TripplannerConfirmdetailedId);
                var result = db.Database.SqlQuery<CheckRemaingOrderStatusDC>("EXEC Operation.TripPlanner_GetCheckRemaingOrderStatus @TripplannerConfirmdetailedId", tripplannerConfirmdetailedId).FirstOrDefault();

                if (result != null)
                {
                    res = new ResCheckRemaingOrderStatusDC
                    {
                        checkRemaingOrderStatusDC = result,
                        Status = true,
                        Message = "Success!!"
                    };
                }
                else
                {
                    res = new ResCheckRemaingOrderStatusDC
                    {
                        checkRemaingOrderStatusDC = null,
                        Status = true,
                        Message = "No Order found!!"
                    };
                }
            }
            return res;
        }

        #region CompleteTripStatusChange
        [Route("CompleteTripStatusChange")]
        [HttpGet]
        public ResponceDc CompleteTripStatusChange(long TripplannerConfirmdetailedId, double lat, double lng)
        {
            bool Status = false;
            ResponceDc res = null;
            int userId = GetLoginUserId();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    var tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.FirstOrDefault(x => x.Id == TripplannerConfirmdetailedId);
                    if (tripPlannerConfirmedDetails != null)
                    {
                        if (tripPlannerConfirmedDetails.OrderCount == 0)
                        {
                            tripPlannerConfirmedDetails.CustomerTripStatus = (int)CustomerTripStatusEnum.Completed;
                            tripPlannerConfirmedDetails.ServeEndTime = DateTime.Now;
                            context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                            Status = true;
                        }
                        else
                        {
                            tripPlannerConfirmedDetails.CustomerTripStatus = (int)CustomerTripStatusEnum.ReachedDistination;
                            tripPlannerConfirmedDetails.ServeStartTime = DateTime.Now;
                            context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                            Status = true;
                        }
                        tripPlannerConfirmedDetails.UnloadLat = lat;
                        tripPlannerConfirmedDetails.UnloadLng = lng;
                        tripPlannerConfirmedDetails.ModifiedBy = userId;
                        tripPlannerConfirmedDetails.ModifiedDate = DateTime.Now;
                        context.Entry(tripPlannerConfirmedDetails).State = EntityState.Modified;
                        Status = true;
                        context.Commit();
                    }
                }
                if (Status)
                {
                    scope.Complete();
                    res = new ResponceDc()
                    {
                        Status = Status,
                        Message = "Complete Trip Status Change SuccessFully!!",
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
        #endregion

        [Route("TripGetDboyRatingOrder")]
        [HttpGet]
        [AllowAnonymous]
        public TripRatingDboyDC TripGetDboyRatingOrder(int Id) //Id : OrderId
        {
            TripRatingDboyDC ratingDboyDC = new TripRatingDboyDC();
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("OrderId", Id);
                var Order = context.Database.SqlQuery<TripDeliveryDboyRatingOrderDc>("exec Operation.TripPlanner_GetDboyRatingOrder @OrderId", param).FirstOrDefault();
                var ratingConfig = context.RatingMasters.Where(x => x.AppType == 2 && x.IsActive == true && x.IsDeleted == false).Include(x => x.RatingDetails).ToList();
                var result = Mapper.Map(ratingConfig).ToANew<List<UserRatingDc>>();
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
        public bool GenerateOTPForDeliveryAPP(int OrderId, string Status, int userid, double? lat, double? lg, string VideoUrl = "", bool IsVideoSeen = false)
        {
            int CustOtp = 0;
            int SalesOtp = 0;
            string sRandomOTP = "";
            bool result = false;
            var Notification = "";
            var sent = false;

            try
            {
                string requireStatus = "Delivered,Delivery Canceled Request,Delivery Redispatch,Delivery Canceled";
                string NewStatus = "Delivered,Delivery Canceled Request";
                using (AuthContext context = new AuthContext())
                {
                    if (context.OrderDispatchedMasters.Any(x => x.OrderId == OrderId && x.Status == "Shipped"))
                    {
                        if (requireStatus.Split(',').ToList().Contains(Status))
                        {
                            if (NewStatus.Split(',').ToList().Contains(Status))
                            {
                                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                                sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                                if (!string.IsNullOrEmpty(sRandomOTP))
                                {
                                    var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive);

                                    if (orderDeliveryOTPs != null)
                                    {
                                        foreach (var orderDeliveryOTP in orderDeliveryOTPs)
                                        {
                                            orderDeliveryOTP.ModifiedDate = DateTime.Now;
                                            orderDeliveryOTP.ModifiedBy = userid;
                                            orderDeliveryOTP.IsActive = false;
                                            //context.OrderDeliveryOTP.Attach(orderDeliveryOTP);
                                            context.Entry(orderDeliveryOTP).State = EntityState.Modified;

                                        }
                                    }
                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                    {
                                        CreatedBy = userid,
                                        CreatedDate = DateTime.Now,
                                        IsActive = true,
                                        OrderId = OrderId,
                                        OTP = sRandomOTP,
                                        Status = Status,
                                        lat = lat,
                                        lg = lg,
                                        VideoUrl = VideoUrl,
                                        IsVideoSeen = IsVideoSeen
                                    };
                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                    result = context.Commit() > 0;
                                }
                            }

                            var param = new SqlParameter("@OrderId", OrderId);
                            var param1 = new SqlParameter("@orderStatus", Status);
                            var orderMobiledetail = context.Database.SqlQuery<OrderMobiledetail>("Exec GetOrderMobileDetailsOTP  @OrderId,@orderStatus", param, param1).FirstOrDefault();

                            var ODM = context.DbOrderMaster.Where(x => x.OrderId == OrderId).SingleOrDefault();
                            if (orderMobiledetail != null && ODM != null)
                            {
                                List<SalesLeadMobile> SalesLeadMobile = new List<SalesLeadMobile>();
                                MongoDbHelper<DataContracts.Mongo.RdCancellationConfiguration> mongoDb = new MongoDbHelper<DataContracts.Mongo.RdCancellationConfiguration>();
                                List<DataContracts.Mongo.RdCancellationConfiguration> MongoData = new List<RdCancellationConfiguration>();
                                Random random = new Random();
                                CustOtp = random.Next(1000, 10000);
                                SalesOtp = random.Next(1000, 10000);
                                switch (Status)
                                {
                                    case "Delivered":
                                        if (!string.IsNullOrEmpty(orderMobiledetail.customermobile))
                                        {
                                            if (orderMobiledetail != null && orderMobiledetail.OrderType == 5)
                                            {
                                                //Getotp(orderMobiledetail.customermobile, " is OTP for delivery of Order No (" + ordermaster.invoice_no + ") Shopkirana", sRandomOTP);
                                                string message = ""; //"{#var1#} is Delivery Code for delivery of Order No. {#var2#} for Total Qty {#var3#} and Value of Rs. {#var4#}. Shopkirana";
                                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Delivered");
                                                message = dltSMS == null ? "" : dltSMS.Template;

                                                message = message.Replace("{#var1#}", sRandomOTP);
                                                message = message.Replace("{#var2#}", OrderId.ToString());
                                                message = message.Replace("{#var3#}", orderMobiledetail.TotalQty.ToString());
                                                message = message.Replace("{#var4#}", orderMobiledetail.OrderAmount.ToString());
                                                if (dltSMS != null)
                                                    Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                            }
                                            else
                                            {
                                                //Getotp(orderMobiledetail.customermobile, " is OTP for delivery of Order No (" + OrderId + ") Shopkirana", sRandomOTP);
                                                string message = ""; //"{#var1#} is Delivery Code for delivery of Order No. {#var2#} for Total Qty {#var3#} and Value of Rs. {#var4#}. Shopkirana";
                                                var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Delivered");
                                                message = dltSMS == null ? "" : dltSMS.Template;

                                                message = message.Replace("{#var1#}", sRandomOTP);
                                                message = message.Replace("{#var2#}", OrderId.ToString());
                                                message = message.Replace("{#var3#}", orderMobiledetail.TotalQty.ToString());
                                                message = message.Replace("{#var4#}", orderMobiledetail.OrderAmount.ToString());
                                                if (dltSMS != null)
                                                    Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                            }
                                        }
                                        break;
                                    case "Delivery Redispatch":
                                        MongoData = mongoDb.GetAll();
                                        foreach (var data in MongoData)
                                        {
                                            if (CustOtp > 0)
                                            {
                                                if (ODM.IsDigitalOrder == true && data.DepartmentName == "Sales")
                                                {
                                                    data.DepartmentName = "Digital";
                                                }
                                                var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive == true && x.UserType == data.DepartmentName).FirstOrDefault();
                                                if (orderDeliveryOTPs != null)
                                                {
                                                    orderDeliveryOTPs.ModifiedDate = DateTime.Now;
                                                    orderDeliveryOTPs.ModifiedBy = userid;
                                                    orderDeliveryOTPs.IsActive = false;
                                                    context.Entry(orderDeliveryOTPs).State = EntityState.Modified;
                                                }

                                                if (data.DepartmentName == "Sales" || data.DepartmentName == "Digital")
                                                {

                                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        OrderId = OrderId,
                                                        OTP = SalesOtp.ToString(),
                                                        Status = Status,
                                                        lat = lat,
                                                        lg = lg,
                                                        UserType = ODM.IsDigitalOrder == true ? "Digital" : data.DepartmentName,
                                                        UserId = orderMobiledetail.SalesId,
                                                        IsUsed = false,
                                                        VideoUrl = VideoUrl,
                                                        IsVideoSeen = IsVideoSeen
                                                    };
                                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                }
                                                else if (data.DepartmentName == "Customer")
                                                {
                                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        OrderId = OrderId,
                                                        OTP = CustOtp.ToString(),
                                                        Status = Status,
                                                        lat = lat,
                                                        lg = lg,
                                                        UserType = data.DepartmentName,
                                                        UserId = orderMobiledetail.customerid,
                                                        IsUsed = false,
                                                        IsVideoSeen = IsVideoSeen,
                                                        VideoUrl = VideoUrl
                                                    };
                                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                }
                                                else
                                                {
                                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        OrderId = OrderId,
                                                        OTP = random.Next(1000, 10000).ToString(),
                                                        Status = Status,
                                                        lat = lat,
                                                        lg = lg,
                                                        UserType = data.DepartmentName,
                                                        UserId = 0,
                                                        IsUsed = false,
                                                        VideoUrl = VideoUrl,
                                                        IsVideoSeen = IsVideoSeen
                                                    };
                                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                }

                                                result = context.Commit() > 0;
                                            }
                                            if (data.DepartmentName == "Customer")
                                            {

                                                if (!string.IsNullOrEmpty(orderMobiledetail.customermobile))
                                                {
                                                    if (orderMobiledetail.Deliverydate.Date == DateTime.Now.Date)
                                                    {
                                                        MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.CustomerRedispatchCharges>();
                                                        var customerRedispatchCharges = mongoDbHelper.Select(x => x.MinValue < orderMobiledetail.OrderAmount && x.MaxValue >= orderMobiledetail.OrderAmount && x.WarehouseId == orderMobiledetail.warehouseid).FirstOrDefault();
                                                        string amount = "";
                                                        if (customerRedispatchCharges != null && customerRedispatchCharges.RedispatchCharges > 0)
                                                        {
                                                            amount = customerRedispatchCharges.RedispatchCharges.ToString();
                                                        }

                                                        string message = "";
                                                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Redispatch");
                                                        message = dltSMS == null ? "" : dltSMS.Template;

                                                        if (!string.IsNullOrEmpty(amount))
                                                        {
                                                            dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "RedispatchCharges");
                                                            message = dltSMS == null ? "" : dltSMS.Template;
                                                        }

                                                        message = message.Replace("{#var1#}", CustOtp.ToString());
                                                        message = message.Replace("{#var2#}", OrderId.ToString());
                                                        message = message.Replace("{#var3#}", amount.ToString());
                                                        if (dltSMS != null)
                                                            Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                                                    }
                                                    else
                                                    {
                                                        string message = "";
                                                        var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Redispatch");
                                                        message = dltSMS == null ? "" : dltSMS.Template;

                                                        message = message.Replace("{#var1#}", CustOtp.ToString());
                                                        message = message.Replace("{#var2#}", OrderId.ToString());
                                                        //message = message.Replace("{#var3#}", amount.ToString());
                                                        if (dltSMS != null)
                                                            Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                                    }
                                                }
                                            }

                                        }

                                        sent = SendNotification(orderMobiledetail, ODM, Status, -1);
                                        break;

                                    case "Delivery Canceled":
                                        MongoData = mongoDb.GetAll();
                                        foreach (var data in MongoData)
                                        {
                                            if (CustOtp > 0)
                                            {
                                                if (ODM.IsDigitalOrder == true && data.DepartmentName == "Sales")
                                                {
                                                    data.DepartmentName = "Digital";
                                                }
                                                var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == OrderId && x.IsActive == true && x.UserType == data.DepartmentName).FirstOrDefault();
                                                if (orderDeliveryOTPs != null)
                                                {
                                                    orderDeliveryOTPs.ModifiedDate = DateTime.Now;
                                                    orderDeliveryOTPs.ModifiedBy = userid;
                                                    orderDeliveryOTPs.IsActive = false;
                                                    context.Entry(orderDeliveryOTPs).State = EntityState.Modified;
                                                }

                                                if (data.DepartmentName == "Sales" || data.DepartmentName == "Digital")
                                                {

                                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        OrderId = OrderId,
                                                        OTP = SalesOtp.ToString(),
                                                        Status = Status,
                                                        lat = lat,
                                                        lg = lg,
                                                        UserType = ODM.IsDigitalOrder == true ? "Digital" : data.DepartmentName,
                                                        UserId = orderMobiledetail.SalesId,
                                                        IsUsed = false,
                                                        IsVideoSeen = IsVideoSeen,
                                                        VideoUrl = VideoUrl
                                                    };
                                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                }
                                                else if (data.DepartmentName == "Customer")
                                                {

                                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        OrderId = OrderId,
                                                        OTP = CustOtp.ToString(),
                                                        Status = Status,
                                                        lat = lat,
                                                        lg = lg,
                                                        UserType = data.DepartmentName,
                                                        UserId = orderMobiledetail.customerid,
                                                        IsUsed = false,
                                                        VideoUrl = VideoUrl,
                                                        IsVideoSeen = IsVideoSeen
                                                    };
                                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                }
                                                else
                                                {

                                                    OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                                    {
                                                        CreatedBy = userid,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        OrderId = OrderId,
                                                        OTP = random.Next(1000, 10000).ToString(),
                                                        Status = Status,
                                                        lat = lat,
                                                        lg = lg,
                                                        UserType = data.DepartmentName,
                                                        UserId = 0,
                                                        IsUsed = false,
                                                        IsVideoSeen = IsVideoSeen,
                                                        VideoUrl = VideoUrl
                                                    };
                                                    context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                                }

                                                result = context.Commit() > 0;
                                            }

                                            if (data.DepartmentName == "Customer")
                                            {
                                                if (!string.IsNullOrEmpty(orderMobiledetail.customermobile))
                                                {
                                                    string Message = ""; //" is OTP for delivery canceled of Order No {#var2#} . ShopKirana";
                                                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Cancellation");
                                                    Message = dltSMS == null ? "" : dltSMS.Template;
                                                    Message = Message.Replace("{#var1#}", CustOtp.ToString());
                                                    Message = Message.Replace("{#var2#}", OrderId.ToString());
                                                    if (dltSMS != null)
                                                        Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.customermobile, Message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);

                                                }
                                            }
                                        }
                                        sent = SendNotification(orderMobiledetail, ODM, Status, SalesOtp);
                                        break;
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GenerateOTPForOrder Method: " + ex.Message);
                result = false;
            }
            return result;
        }

        private bool SendNotification(OrderMobiledetail orderMobiledetail, OrderMaster ODM, string Status, int SalesOtp)
        {
            bool sent = false;
            var Notification = "";
            if (!string.IsNullOrEmpty(orderMobiledetail.salespersonfcmid) && ODM != null && (SalesOtp > 0 || SalesOtp == -1))
            {
                string Body = "OrderID: " + ODM.OrderId + "  |  " + "Order Amount:" + orderMobiledetail.TotalAmt + "@" + "SKCode: " + ODM.Skcode + "  |  " + "ShopName: " + ODM.ShopName + "@" + (SalesOtp == -1 ? "" : ("OTP: " + SalesOtp));
                //string Body = "OrderID: " + ODM.OrderId + "  |  " + "Order Amount:" + orderMobiledetail.TotalAmt + "@" + "SKCode: " + ODM.Skcode + "  |  " + "ShopName: " + ODM.ShopName;
                Body = Body.Replace("@", System.Environment.NewLine);
                // string DeliveryCancelApproveReqBody = "OrderID: " +ODM.OrderId + "<br>" + "Order Amount" + ODM.OrderAmount + "<br>" + "SKCode: " +ODM.Skcode +"<br>" +"ShopName: " + ODM.ShopName +"<br>" ;

                string Key1 = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                //var fcmdcs = new FcmidDCss();
                //fcmdcs.body = (Status == "Delivery Redispatch" || Status == "Delivery Canceled") ? Body : "";
                //fcmdcs.title = Status == "Delivery Redispatch" ? "Delivery Redispatch OTP" : Status == "Delivery Canceled" ? "Delivery Cancel OTP" : "";
                //fcmdcs.notify_type = "OTP_SMS";
                //var fcmdc = new FcmidDC();
                //fcmdc.to = orderMobiledetail.salespersonfcmid;
                //fcmdc.data = fcmdcs;
                var data = new FCMData
                {
                    title = Status == "Delivery Redispatch" ? "Delivery Redispatch OTP" : Status == "Delivery Canceled" ? "Delivery Cancel OTP" : "",
                    body = (Status == "Delivery Redispatch" || Status == "Delivery Canceled") ? Body : "",
                    icon = "",
                    typeId = 0,
                    notificationCategory = "",
                    notificationType = "",
                    notificationId = 0,
                    notify_type = "OTP_SMS",
                    // OrderId = OrderId,
                    url = "", //OrderId, PeopleId
                              // OrderStatus = OrderStatus
                };
                var firebaseService = new FirebaseNotificationServiceHelper(Key1);
                //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                var result = firebaseService.SendNotificationForApprovalAsync(orderMobiledetail.salespersonfcmid, data);
                if (result != null)
                {
                    //fcmdc.MessageId = response.results.FirstOrDefault().message_id;
                    Notification = "Notification sent successfully";
                    sent = true;
                }
                else
                {
                    sent = false;
                    Notification = "Notification Not sent";
                }
                //logger.Info("OTP Notification: " + fcmdcs.title + " : " + Body);
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "POST";
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(fcmdc);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key1));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1)
                //                {
                //                    fcmdc.MessageId = response.results.FirstOrDefault().message_id;
                //                    Notification = "Notification sent successfully";
                //                    sent = true;
                //                }
                //                else if (response.failure == 1)
                //                {
                //                    sent = false;
                //                    Notification = "Notification Not sent";
                //                }
                //            }
                //        }
                //    }
                //}
            }

            return sent;
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
        //public OTP Getotp(string MobileNumber, string msg, string sRandomOTP, string DLTId)
        //{
        //    logger.Info("start Gen OTP: ");
        //    try
        //    {
        //        string OtpMessage = msg;
        //        string message = sRandomOTP + " :" + OtpMessage;
        //        Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), DLTId);
        //        OTP a = new OTP()
        //        {
        //            OtpNo = sRandomOTP
        //        };
        //        return a;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in OTP Genration for Order.");
        //        return null;
        //    }
        //}

        // call in Redispatched and reattempt 
        [Route("NotifyCustomer")]
        [HttpGet]
        public async Task<ResponceDc> NotifyCustomer(int CustomerId)
        {
            ResponceDc res;
            bool result = false;
            using (var context = new AuthContext())
            {
                string CustFcmId = await context.Customers.Where(x => x.CustomerId == CustomerId && x.fcmId != null).Select(x => x.fcmId).FirstOrDefaultAsync();
                //send notification  => to Customer
                if (CustFcmId != null)
                {

                    ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                    bool IsSendNotification = await helper.NotifyCustomer(CustFcmId, CustomerId);

                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Notification Successfully Sent!!"
                    };
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Notification not sent!!"
                    };
                }
            }
            return res;
        }
        [Route("AllNotifyCustomerStartTrip")]
        [HttpGet]
        public async Task<ResponceDc> AllNotifyCustomerStartTrip(long tripPlannerConfirmedMasterId)
        {
            ResponceDc res;
            bool result = false;
            using (var context = new AuthContext())
            {
                var Customerids = await context.TripPlannerConfirmedDetails.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId && x.IsActive == true && x.IsDeleted == false).Select(x => x.CustomerId).Distinct().ToListAsync();
                var fcmids = await context.Customers.Where(x => Customerids.Contains(x.CustomerId) && x.fcmId != null).Select(x => new { x.fcmId, x.CustomerId }).ToListAsync();
                //send notification  => to Customer
                if (fcmids != null && fcmids.Any())
                {
                    foreach (var item in fcmids)
                    {
                        ConfigureNotifyHelper helper = new ConfigureNotifyHelper();
                        bool IsSendNotification = await helper.NotifyCustomer(item.fcmId, item.CustomerId);
                    }
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Notification Successfully Sent!!"
                    };
                }
                else
                {
                    res = new ResponceDc()
                    {
                        TripDashboardDC = null,
                        Status = result,
                        Message = "Notification not sent!!"
                    };
                }
            }
            return res;
        }
        #endregion
        [Route("TripVerifyAssignmentRefNo")]
        [HttpGet]
        public async Task<bool> TripVerifyAssignmentRefNo(int DeliveryIssuanceId, string RefNo, int custId)
        {
            if (DeliveryIssuanceId > 0 && RefNo != null)
            {
                using (var context = new AuthContext())
                {
                    var DId = new SqlParameter("@DeliveryIssuanceId", DeliveryIssuanceId);
                    var Refn = new SqlParameter("@GatewayTransId", RefNo);
                    var custIds = new SqlParameter("@CustId", custId);
                    context.Database.CommandTimeout = 300;
                    return await context.Database.SqlQuery<bool>("exec Operation.TripPlanner_TripVerifyAssignmentRefNo @DeliveryIssuanceId, @GatewayTransId, @CustId", DId, Refn, custIds).FirstOrDefaultAsync();
                }
            }
            return false;
        }
        [Route("CustomerUnloadLocation")]
        [HttpPost]
        public ResCustomerUnloadLocationDC CustomerUnloadLocation(CustomerUnloadLocationDC customerUnloadLocationDC)
        {
            ResCustomerUnloadLocationDC res = new ResCustomerUnloadLocationDC();
            if (customerUnloadLocationDC.CustomerId > 0)
            {
                using (var context = new AuthContext())
                {
                    List<CustomerUnloadLocation> customerUnloadLocationsList = context.CustomerUnloadLocationDb.Where(x => x.CustomerId == customerUnloadLocationDC.CustomerId && x.IsActive == true && x.IsDeleted == false).ToList();
                    if (customerUnloadLocationsList != null && customerUnloadLocationsList.Any())
                    {
                        foreach (var cul in customerUnloadLocationsList)
                        {
                            cul.IsActive = false;
                            cul.IsDeleted = true;
                        }
                    }
                    context.Commit();


                    CustomerUnloadLocation customerUnloadLocation = new CustomerUnloadLocation();
                    customerUnloadLocation.CustomerId = customerUnloadLocationDC.CustomerId;
                    customerUnloadLocation.ShopImageUrl = customerUnloadLocationDC.ShopImageUrl;
                    customerUnloadLocation.latitude = customerUnloadLocationDC.latitude;
                    customerUnloadLocation.Longitude = customerUnloadLocationDC.Longitude;
                    customerUnloadLocation.IsActive = true;
                    customerUnloadLocation.IsDeleted = false;
                    customerUnloadLocation.CreatedDate = DateTime.Now;
                    customerUnloadLocation.CreatedBy = customerUnloadLocationDC.UserId;
                    context.CustomerUnloadLocationDb.Add(customerUnloadLocation);
                    context.Commit();
                    res = new ResCustomerUnloadLocationDC()
                    {
                        customerUnloadLocationDC = null,
                        Status = true,
                        Message = "Customer Unload Added"
                    };

                }
            }
            else
            {
                res = new ResCustomerUnloadLocationDC()
                {
                    customerUnloadLocationDC = null,
                    Status = false,
                    Message = "Someting Went Wrong!!"
                };
            }
            return res;
        }
        [Route("GetRedispatchOrder")]
        [HttpGet]
        public List<TripGetRedispatchOrderDc> GetRedispatchOrder(int WarehouseId)
        {
            if (WarehouseId > 0)
            {
                using (var context = new AuthContext())
                {
                    var warehouseId = new SqlParameter("@WarehouseId", WarehouseId);
                    var list = context.Database.SqlQuery<TripGetRedispatchOrderDc>("exec Operation.TripPlanner_getRedispatchOrder @WarehouseId", warehouseId).ToList();
                    return list;
                }
            }
            return null;
        }


        [Route("EnterMilometerLimit")]
        [HttpGet]
        public TripMilometerDc EnterMilometerLimit(long tripPlannerConfirmedMasterId)
        {
            using (var context = new AuthContext())
            {
                var vehicle = context.TripPlannerVehicleDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId).FirstOrDefault();
                TripMilometerDc tripMilometer = new TripMilometerDc
                {
                    StartKm = (int)vehicle.StartKm,
                    MaxEndKm = (int)vehicle.StartKm + int.Parse(ConfigurationManager.AppSettings["TripVehicleMaxTravelledDistanceInKm"])
                };
                return tripMilometer;
            }
        }


        [Route("SendCloseKmApprovalRequest")]
        [HttpPost]
        public async Task<bool> SendCloseKmApprovalRequest(SendCloseKmApprovalRequestDc sendCloseKmApprovalRequestDc)
        {
            bool isSendNotification = false;
            string notify_type = "Last Mile Request Approval Notification";

            //int userId = GetLoginUserId();
            TripPlannerVehicleManager tripPlannerVehicleManager = new TripPlannerVehicleManager();
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
                var vehicle = await context.TripPlannerVehicleDb.FirstOrDefaultAsync(x => x.TripPlannerConfirmedMasterId == sendCloseKmApprovalRequestDc.tripPlannerConfirmMasterId && x.IsActive == true && x.IsDeleted == false);
                if (vehicle.IsCloseKmRequestSend != true)
                {
                    isSendNotification = true;
                    tripPlannerVehicleManager.InsertTripPlannerApprovalRequest(context, sendCloseKmApprovalRequestDc.tripPlannerConfirmMasterId, vehicle, sendCloseKmApprovalRequestDc.PeopleID, TripPlannerApprovalRequestTypeConstants.EndKm, sendCloseKmApprovalRequestDc.closeKm, sendCloseKmApprovalRequestDc.closeKmUrl);
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
                    configureNotifyHelper.SendNotificationForSarthiTripApproval(Msg, sendCloseKmApprovalRequestDc.tripPlannerConfirmMasterId, notify_type, context);
                    context.Commit();

                }
            }

            return true;
        }
        #region SKP/KPP Work
        [Route("GetSKP_KPP_OwnerList")]
        [HttpGet]
        public SKPResDc GetSKP_KPP_OwnerList(int Warehouseid, int CustomerId)
        {
            SKPResDc SKPResdc = new SKPResDc();
            using (var db = new AuthContext())
            {
                var customerCount = db.Customers.Where(x => x.SKPOwnerId == CustomerId && x.Deleted == false).Count();
                if (customerCount == 0)
                {
                    List<GetSKP_KPP_OwnerListDC> SKP_KPPCustomerOwnerList = db.Customers.Where(x => x.CustomerType == "SKP Owner" && x.Warehouseid == Warehouseid && x.Deleted == false).Select(
                       x => new GetSKP_KPP_OwnerListDC
                       {
                           CustomerId = x.CustomerId,
                           Skcode = x.Skcode + "-(" + x.Name + ")",
                           ShopName = x.ShopName
                       }).ToList();
                    SKPResdc.GetSKP_KPP_OwnerList = SKP_KPPCustomerOwnerList;
                    SKPResdc.status = true;
                    SKPResdc.msg = "Get Data Successfully!!";
                }
                else
                {
                    SKPResdc.GetSKP_KPP_OwnerList = null;
                    SKPResdc.status = false;
                    SKPResdc.msg = "SKP Owner Not Change!!";
                };
            }
            return SKPResdc;
        }

        [Route("SearchSKP_KPP_OwnerList")]
        [HttpGet]
        public List<GetSKP_KPP_OwnerListDC> SearchSKP_KPP_OwnerList(int Warehouseid, string TripType)
        {
            List<GetSKP_KPP_OwnerListDC> list = new List<GetSKP_KPP_OwnerListDC>();
            using (var db = new AuthContext())
            {
                var predicate = PredicateBuilder.True<Customer>();
                predicate = predicate.And(x => x.Deleted == false && x.Warehouseid == Warehouseid);
                if (TripType == "SKP Owner")
                {
                    predicate = predicate.And(x => x.CustomerType == "SKP Owner");
                }
                else if (TripType == "KPP")
                {
                    predicate = predicate.And(x => x.CustomerType == "KPP");
                }

                list = db.Customers.Where(predicate).Select(
                    x => new GetSKP_KPP_OwnerListDC
                    {
                        CustomerId = x.CustomerId,
                        Skcode = x.Skcode + "-(" + x.Name + ")",
                        ShopName = x.ShopName
                    }).ToList();
            }
            return list;
        }
        #endregion

        [Route("GetTripTouchPointToRearrange")]
        [HttpGet]
        public List<TripTouchPointRearrange> GetTripTouchPointToRearrange(long tripPlannerConfirmMasterId)
        {
            using (var db = new AuthContext())
            {
                var tripPlannerConfirmedMasterId = new SqlParameter("@TripPlannerConfirmedMasterId", tripPlannerConfirmMasterId);
                var touchPointList = db.Database.SqlQuery<TripTouchPointRearrange>(" EXEC Operation.GetRearrangeTouchPoints  @TripPlannerConfirmedMasterId", tripPlannerConfirmedMasterId).ToList();
                return touchPointList;
            }
        }

        [Route("UpdateTripTouchPointToRearrange")]
        [HttpPost]
        public bool UpdateTripTouchPointToRearrange(List<TripTouchPointRearrange> pointList)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var db = new AuthContext())
                {
                    long tripPlannerConfirmedMasterId = 0;
                    int sequence = 1;
                    foreach (var item in pointList)
                    {
                        var detail = db.TripPlannerConfirmedDetails.First(x => x.Id == item.Id);
                        tripPlannerConfirmedMasterId = detail.TripPlannerConfirmedMasterId;
                        detail.SequenceNo = sequence++;
                        db.Entry(detail).State = EntityState.Modified;
                    }
                    var vehicle = db.TripPlannerVehicleDb.First(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasterId);
                    vehicle.IsRearrangeDone = true;
                    vehicle.IsStartKmRequestSend = false;
                    db.Entry(vehicle).State = EntityState.Modified;


                    db.Commit();
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    tripPlannerHelper.UpdateTripSequenceNew(tripPlannerConfirmedMasterId, db, false);

                }
                scope.Complete();

                return true;
            }
        }



        #region Nov 2022 build 
        [HttpGet]
        [Route("GetAllTrip")]
        public List<GetTripDc> GetAllTrip(int DboyId)
        {
            List<GetTripDc> getTripDc = new List<GetTripDc>();
            using (var db = new AuthContext())
            {
                var dboyId = new SqlParameter("@DboyId", DboyId);
                getTripDc = db.Database.SqlQuery<GetTripDc>("EXEC [Operation].[TripPlanner_GetMobileAppTripList] @DboyId", dboyId).ToList();
                //AppVisits
                var CurrentDate = DateTime.Now.ToString("dd/MM/yyyy");
                AppVisits appVisit = new AppVisits();
                MongoDbHelper<AppVisits> mongoAppVisitDbHelper = new MongoDbHelper<AppVisits>();
                string mobileNoQyery = "select distinct p.Mobile from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + DboyId + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
                var mobileNo = db.Database.SqlQuery<string>(mobileNoQyery).FirstOrDefault();
                appVisit.UserName = mobileNo;
                appVisit.UserId = DboyId;
                appVisit.AppType = "New Delivery App";
                appVisit.VisitedOn = DateTime.Now;
                var Status = mongoAppVisitDbHelper.InsertAsync(appVisit);

            }
            return getTripDc;
        }
        #endregion

        [Route("SkipRearrange")]
        [HttpGet]
        public bool SkipRearrange(long TripPlannerConfMasterID)
        {
            using (var db = new AuthContext())
            {
                var vehicle = db.TripPlannerVehicleDb.FirstOrDefault(x => x.TripPlannerConfirmedMasterId == TripPlannerConfMasterID);
                if (vehicle != null)
                {
                    vehicle.IsRearrangeDone = true;
                    db.Entry(vehicle).State = EntityState.Modified;
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        [Route("GetHolidayOnRedispatch")]
        [HttpGet]
        [AllowAnonymous]
        public List<DateTime> GetHolidayOnRedispatch(int orderId, bool IsReattemt = false)
        {
            List<DateTime> ResponsedateList = new List<DateTime>();
            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(DateTime.Today);
            dateList.Add(DateTime.Today.AddDays(1));
            dateList.Add(DateTime.Today.AddDays(2));
            //dateList.Add(DateTime.Today.AddDays(3));
            if (orderId > 0)
            {
                using (var db = new AuthContext())
                {
                    var deliveryIssuanceIdParam = new SqlParameter
                    {
                        ParameterName = "orderId",
                        Value = orderId
                    };
                    List<HolidayOnRedispatchDC> list = db.Database.SqlQuery<HolidayOnRedispatchDC>("exec GetHolidayOnRedispatch @orderId", deliveryIssuanceIdParam).ToList();

                    List<HolidayOnRedispatchDC> holidayList = new List<HolidayOnRedispatchDC>();

                    if (list != null && list.Any())
                    {
                        var warehouseHoliday = list.Where(x => x.HolidayType == "WarehouseHoliday").ToList();
                        if (warehouseHoliday != null && warehouseHoliday.Any())
                        {
                            holidayList.AddRange(warehouseHoliday);
                        }
                        var WarehouseCapacitiesHoliday = list.Where(x => x.HolidayType == "WarehouseCapacitiesHoliday").ToList();
                        if (WarehouseCapacitiesHoliday != null && WarehouseCapacitiesHoliday.Any())
                        {
                            holidayList.AddRange(WarehouseCapacitiesHoliday);
                        }
                        var customerHoliday = list.FirstOrDefault(x => x.HolidayType == "CustomerHoliday");
                        var clusterHoliday = list.FirstOrDefault(x => x.HolidayType == "ClusterHoliday");

                        if (customerHoliday != null)
                        {
                            if (!holidayList.Any(x => x.Holiday == customerHoliday.Holiday))
                            {
                                holidayList.Add(customerHoliday);
                            }
                        }
                        else if (clusterHoliday != null && !holidayList.Any(x => x.Holiday == clusterHoliday.Holiday && x.UpdateCapacity == -1))
                        {
                            holidayList.Add(clusterHoliday);
                        }
                    }

                    #region return one date in case of Reattempt
                    if (IsReattemt)
                    {
                        var DeliveryDate = list.Select(x => x.Deliverydate).FirstOrDefault();
                        var OrderDate = list.Select(x => x.OrderDate).FirstOrDefault();

                        if (OrderDate != null && DeliveryDate != null)
                        {
                            if ((OrderDate.Date == DateTime.Now.Date) || (DeliveryDate.Date > DateTime.Now.Date))
                            {
                                ResponsedateList.Add(DeliveryDate);
                                return ResponsedateList;

                            }
                            else if (DeliveryDate.Date == DateTime.Now.Date)
                            {
                                int i = 1;
                                while (i > 0)
                                {
                                    DateTime date = DeliveryDate.AddDays(i);
                                    var data = holidayList.Where(x => date.ToString("dddd").ToLower() == x.Holiday.Trim().ToLower()).FirstOrDefault();
                                    if (data == null)
                                    {
                                        ResponsedateList.Add(date);
                                        return ResponsedateList;
                                    }
                                    i++;
                                }
                            }
                            else if (DeliveryDate < DateTime.Now)
                            {
                                int i = 1;
                                while (i > 0)
                                {
                                    DateTime date = DateTime.Now.AddDays(i);
                                    var data = holidayList.Where(x => date.ToString("dddd").ToLower() == x.Holiday.Trim().ToLower()).FirstOrDefault();
                                    if (data == null)
                                    {
                                        ResponsedateList.Add(date);
                                        return ResponsedateList;
                                    }
                                    i++;
                                }
                            }

                        }

                    }
                    #endregion

                    if (holidayList != null && holidayList.Any())
                    {
                        //for (int i = 0; i < dateList.Count; i++)
                        //{
                        //    var date = dateList[i];
                        //    var data = holidayList.Where(x => date.ToString("dddd").ToLower() == x.Holiday.Trim().ToLower() /*&& x.date == date*/).FirstOrDefault();
                        //    //if (data == null)
                        //    //    data = holidayList.Where(x => date.ToString("dddd").ToLower() == x.Holiday.Trim().ToLower() &&x.UpdateCapacity == -1).FirstOrDefault();
                        //    //if (data != null && (data.UpdateCapacity == 0 || data.UpdateCapacity == -1))
                        //    //{
                        //    //    var newdate = dateList.Last().AddDays(1);
                        //    //    dateList.Add(newdate);
                        //    //}
                        //    //else
                        //    if (data == null)
                        //    {
                        //        ResponsedateList.Add(date);
                        //    }
                        //}

                        int i = 0;
                        while (i >= 0)
                        {
                            DateTime date = DateTime.Now.AddDays(i);
                            var data = holidayList.Where(x => date.ToString("dddd").ToLower() == x.Holiday.Trim().ToLower()).FirstOrDefault();
                            if (data == null)
                            {
                                ResponsedateList.Add(date);
                                if (ResponsedateList.Count() == 3)
                                {
                                    return ResponsedateList;
                                }
                            }
                            i++;
                        }
                    }
                }
            }

            return ResponsedateList;
        }

        #region March 2023 Build
        [Route("BackOrderUpdate")]
        [HttpGet]
        public bool BackOrderUpdate(long TripPlannerConfirmedDetailId, long TripPlannerConfirmedOrderId)
        {
            bool status = false;
            using (var db = new AuthContext())
            {
                var tripPlannerConfirmedDetail = db.TripPlannerConfirmedDetails.Where(x => x.Id == TripPlannerConfirmedDetailId && x.CustomerTripStatus != 4).FirstOrDefault();
                var tripPlannerConfirmedOrder = db.TripPlannerConfirmedOrders.Where(x => x.Id == TripPlannerConfirmedOrderId && (x.WorkingStatus != 4 || x.WorkingStatus != 5)).FirstOrDefault();
                if (tripPlannerConfirmedDetail != null && tripPlannerConfirmedOrder != null)
                {
                    var orderDeliveryOTPs = db.OrderDeliveryOTP.Where(x => x.OrderId == tripPlannerConfirmedOrder.OrderId && x.IsActive == true).ToList();
                    if (orderDeliveryOTPs != null)
                    {
                        foreach (var orderDeliveryOTP in orderDeliveryOTPs)
                        {
                            orderDeliveryOTP.IsActive = false;
                            orderDeliveryOTP.ModifiedDate = DateTime.Now;
                            db.Entry(orderDeliveryOTP).State = EntityState.Modified;
                        }
                    }
                    tripPlannerConfirmedDetail.CustomerTripStatus = 0;
                    tripPlannerConfirmedDetail.ModifiedDate = DateTime.Now;
                    db.Entry(tripPlannerConfirmedDetail).State = EntityState.Modified;

                    tripPlannerConfirmedOrder.WorkingStatus = 0;
                    tripPlannerConfirmedOrder.ModifiedDate = DateTime.Now;
                    db.Entry(tripPlannerConfirmedOrder).State = EntityState.Modified;
                    db.Commit();
                    status = true;
                }
            }
            return status;
        }
        [Route("OrderstatusUpdate")]
        [HttpGet]
        [AllowAnonymous]
        public bool OrderstatusUpdate(long TripPlannerMasterId, int OrderId)
        {
            bool Status = false;
            if (TripPlannerMasterId > 0 && OrderId > 0)
            {
                using (var db = new AuthContext())
                {
                    var tripPlannerMasterId = new SqlParameter("@TripPlannerMasterId", TripPlannerMasterId);
                    var orderId = new SqlParameter("@OrderId", OrderId);
                    db.Database.ExecuteSqlCommand("EXEC Operation.TripPlanner_OrderstatusUpdate  @TripPlannerMasterId,@OrderId", tripPlannerMasterId, orderId);
                    db.Commit();
                    Status = true;
                }
            }
            return Status;
        }
        #endregion

        #region Sales Return 21/08/2023

        [Route("ReturnItemList")]
        [HttpGet]
        public List<ReturnItemListDC> ReturnItemList(int tripPlannerConfirmedDetailId)
        {
            var tripPlannerItemCheckListdc = new List<TripPlannerItemCheckListDC>();
            List<ReturnItemListDC> returnItems = new List<ReturnItemListDC>();
            using (var context = new AuthContext())
            {
                var TripId = new SqlParameter("@TripPlannerConfirmedDetailId", tripPlannerConfirmedDetailId);
                /*var orderIdDt = new System.Data.DataTable();
                orderIdDt.Columns.Add("IntValue");
                foreach (var item in itemuUnloadingDC.OrderId)
                {
                    var dr = orderIdDt.NewRow();
                    dr["IntValue"] = item;
                    orderIdDt.Rows.Add(dr);
                }

                var param = new SqlParameter("OrderIds", orderIdDt);
                param.SqlDbType = System.Data.SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";*/

                var data = context.Database.SqlQuery<ReturnItemListDC>("[Operation].[TripPlanner_GetReturnItemListPage] @TripPlannerConfirmedDetailId", TripId).ToList();
                if (data != null && data.Count > 0)
                    return data;
                else
                    return returnItems;
            }
        }

        #endregion




        #region Generate OTP of SalesPerson for ReAttempt Order
        [AllowAnonymous]
        [Route("GenerateOTPofSalesPersonforReattempt")]
        [HttpPost]
        public Response GenerateOTPofSalesPersonforReattempt(payload obj)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            int OrderID = 0;
            var Notification = "";
            var sent = false;

            Response Result = new Response();
            using (var context = new AuthContext())
            {
                if (obj != null && obj.Status == "Delivery Redispatch")
                {
                    var IdDt = new DataTable();
                    IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");

                    foreach (var item in obj.OrderIds)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@OrderIds", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.intValues";

                    string sRandomOTP = "";
                    string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                    sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);


                    //var salesPersonMobiledetail = context.Database.SqlQuery<salesPersonMobiledetails>("Exec GetSalesPersonForReattemptOrder  @OrderIds", param).FirstOrDefault();
                    //var SalesLead = context.Peoples.FirstOrDefault(x => x.PeopleID == (salesPersonMobiledetail.SalesId > 0 ? salesPersonMobiledetail.SalesId : 0) && x.Active == true);
                    //var WHLead = context.Peoples.FirstOrDefault(x => x.PeopleID == (salesPersonMobiledetail.WHLeadId > 0 ? salesPersonMobiledetail.WHLeadId : 0) && x.Active == true);


                    TripPlannerConfirmedOrder TripOrders = new TripPlannerConfirmedOrder();
                    TripPlannerConfirmedDetail tripPlannerConfirmedDetails = new TripPlannerConfirmedDetail();
                    if (obj.OrderIds.Count() == 1)
                    {
                        List<long> orderIdList = obj.OrderIds.Select(x => (long)x).ToList();
                        //TripOrders = context.TripPlannerConfirmedOrders.FirstOrDefault(x => orderIdList.Contains(x.OrderId) && x.IsActive == true && x.IsDeleted == false);
                        TripOrders = context.TripPlannerConfirmedOrders.OrderByDescending(x => x.Id).FirstOrDefault(x => orderIdList.Contains(x.OrderId) && x.IsActive == true && x.IsDeleted == false);
                        if (TripOrders != null)
                            tripPlannerConfirmedDetails = context.TripPlannerConfirmedDetails.FirstOrDefault(x => x.Id == TripOrders.TripPlannerConfirmedDetailId && x.IsActive == true && x.IsDeleted == false);
                    }
                    List<int> OrderIDs = new List<int>();
                    //if (salesPersonMobiledetail != null && (SalesLead != null || WHLead != null))
                    if (true)
                    {
                        List<OrderDeliveryOTP> orderDeliveryOTPs = new List<OrderDeliveryOTP>();
                        foreach (var orderid in obj.OrderIds)
                        {
                            #region Change Working Status in Trip
                            if (obj.OrderIds.Count() == 1 && TripOrders != null && tripPlannerConfirmedDetails != null)
                            {
                                tripPlannerConfirmedDetails.CustomerTripStatus = Convert.ToInt32(CustomerTripStatusEnum.NotifyReAttempt);
                                TripOrders.WorkingStatus = Convert.ToInt32(WorKingStatus.ReAttempt);
                                TripOrders.ModifiedBy = userid;
                                TripOrders.ModifiedDate = DateTime.Now;
                                context.Entry(TripOrders).State = EntityState.Modified;
                            }
                            #endregion


                            var ExistsOTPs = context.OrderDeliveryOTP.Where(x => x.OrderId == orderid && x.IsActive == true).OrderByDescending(x => x.CreatedDate).ToList();
                            if (ExistsOTPs != null && ExistsOTPs.Any())
                            {
                                foreach (var ExistsOTP in ExistsOTPs)
                                {
                                    ExistsOTP.IsActive = false;
                                    ExistsOTP.ModifiedDate = DateTime.Now;
                                    ExistsOTP.ModifiedBy = userid;
                                    context.Entry(ExistsOTP).State = EntityState.Modified;
                                }
                            }

                            if (!string.IsNullOrEmpty(sRandomOTP))
                            {
                                OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                {
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    OrderId = orderid,
                                    OTP = sRandomOTP,
                                    Status = obj.Status,
                                    UserType = "HQ Operation(ReAttempt)",
                                    IsUsed = false,
                                    UserId = 0,
                                    lat = obj.lat,
                                    lg = obj.lg,
                                    IsVideoSeen = false,
                                    VideoUrl = obj.VideoUrl

                                };
                                orderDeliveryOTPs.Add(OrderDeliveryOTP);

                            }

                            var orderDispatchMaster = context.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderid);
                            orderDispatchMaster.comments = obj.Reason;
                            orderDispatchMaster.comment = obj.Reason;
                            //orderDispatchMaster.UpdatedDate = DateTime.Now;
                            //orderDispatchMaster.userid = userid;
                            context.Entry(orderDispatchMaster).State = EntityState.Modified;

                            //var orderdispatchHistory = context.OrderMasterHistoriesDB.Where(x => x.orderid == orderid && x.Status == "Delivery Redispatch").OrderByDescending(x=> x.id).FirstOrDefault();
                            //orderdispatchHistory.Reasoncancel = obj.Reason;
                            // orderdispatchHistory.Description = obj.Reason;
                            // context.Entry(orderdispatchHistory).State = EntityState.Modified;

                            OrderIDs.Add(orderid);
                        }



                        Result.res = true;
                        Result.msg = "OTP generated for HQ Operation Role ";
                        context.OrderDeliveryOTP.AddRange(orderDeliveryOTPs);

                        context.Commit();
                    }
                    else
                    {
                        Result.msg = "Please Use Reattempt by Operation.";
                        Result.res = false;
                    }
                }
                else
                {
                    Result.msg = "OTP not Generated.";
                    Result.res = false;
                }
            }
            return Result;
        }

        #endregion

        [AllowAnonymous]
        [Route("Smstest")]
        [HttpPost]
        public bool test()
        {
            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "Order_Delivery_Redispatch");
            string message = dltSMS == null ? "" : dltSMS.Template;


            dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.RetailerApp, "RedispatchCharges");
            message = dltSMS == null ? "" : dltSMS.Template;

            message = message.Replace("{#var1#}", "1234");
            message = message.Replace("{#var2#}", "1111");
            message = message.Replace("{#var3#}", "100");
            if (dltSMS != null)
                Common.Helpers.SendSMSHelper.SendSMS("9644498924", message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
            return true;

        }

        private async Task<bool> IsCustomerScaleupOverdue(long accountId, int customerId)
        {

            LeadRequestPost obj = new LeadRequestPost();
            ScaleUpResponse res = new ScaleUpResponse();
            InitiateLeadDetail data = new InitiateLeadDetail();
            ScaleUpConfigDc scaleup = new ScaleUpConfigDc();

            using (var db = new AuthContext())
            {
                var customer = db.Customers.FirstOrDefault(x => x.CustomerId == customerId && !x.Deleted);
                var ScaleUps = db.ScaleUpConfig.Where(x => x.IsActive == true && x.IsDeleted == false && x.ProductType == "CreditLine").ToList();
                var ScaleUp = ScaleUps.FirstOrDefault(x => x.Name == "token");
                if (ScaleUp != null)
                {
                    obj.companyCode = ScaleUp.CompanyCode;
                    obj.ProductCode = ScaleUp.ProductCode;
                    //var scaleupCust = db.ScaleUpCustomers.FirstOrDefault(x => x.CustomerId == customer.CustomerId && x.ProductCode == obj.ProductCode);

                    if (customer != null && ScaleUp != null)
                    {
                        obj.Mobile = customer.Mobile;
                        obj.CustomerReferenceNo = customer.Skcode;
                        scaleup = new ScaleUpConfigDc
                        {
                            ApiKey = ScaleUp.ApiKey,
                            ApiSecretKey = ScaleUp.ApiSecretKey,
                            ScaleUpUrl = ScaleUp.ScaleUpUrl,
                            ApiUrl = ScaleUp.ApiUrl
                        };
                        TextFileLogHelper.TraceLog("Lead Mobile - " + obj.Mobile);
                        var ScaleUpToken = await GenerateTokenScaleUp(scaleup);
                        if (ScaleUpToken != null)
                        {
                            TextFileLogHelper.TraceLog("Lead Token - " + ScaleUpToken.access_token);
                            using (var httpClient = new HttpClient())
                            {
                                using (var request = new HttpRequestMessage(new HttpMethod("POST"), scaleup.ScaleUpUrl + "/services/loanaccount/v1/GetOverdueLoanAccountList"))
                                {
                                    ServicePointManager.Expect100Continue = true;
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                                    request.Headers.TryAddWithoutValidation("noencryption", "1");
                                    request.Headers.TryAddWithoutValidation("Authorization", ScaleUpToken.token_type + " " + ScaleUpToken.access_token);


                                    var lst = new List<GetOverdueLoanAccountIdRequest>();
                                    lst.Add(new GetOverdueLoanAccountIdRequest
                                    {
                                        LoanAccountID = accountId
                                    });
                                    var newJson = JsonConvert.SerializeObject(lst);
                                    request.Content = new StringContent(newJson);
                                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                                    var response = await httpClient.SendAsync(request);
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        string jsonString = (await response.Content.ReadAsStringAsync());
                                        var respose = JsonConvert.DeserializeObject<List<GetOverdueLoanAccountResponse>>(jsonString);
                                        if (respose != null && respose.Any())
                                        {
                                            TextFileLogHelper.TraceLog("Check If Overdue - " + respose.First().TransactionStatus);
                                            return respose.First().TransactionStatus;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public async Task<ScaleUpTokenDc> GenerateTokenScaleUp(ScaleUpConfigDc scaleup)
        {
            ScaleUpTokenDc accessToken = new ScaleUpTokenDc();

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), scaleup.ScaleUpUrl + "/" + scaleup.ApiUrl /*"/services/identity/v1/connect/token"*/))
                {
                    request.Headers.TryAddWithoutValidation("noencryption", "1");
                    var contentList = new List<string>();
                    contentList.Add($"grant_type={Uri.EscapeDataString("client_credentials")}");
                    contentList.Add($"client_Id={Uri.EscapeDataString(scaleup.ApiKey)}");
                    contentList.Add($"client_secret={Uri.EscapeDataString(scaleup.ApiSecretKey)}");
                    request.Content = new StringContent(string.Join("&", contentList));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    var response = await httpClient.SendAsync(request);
                    if (System.Net.HttpStatusCode.OK == response.StatusCode)
                    {
                        string jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                        accessToken = JsonConvert.DeserializeObject<ScaleUpTokenDc>(jsonString);
                    }
                    else
                    {
                        accessToken = null;
                    }
                }
            }
            return accessToken;
        }

    }
    public class payload
    {
        public List<int> OrderIds { get; set; }
        public string Status { get; set; }
        public double? lat { get; set; }
        public double? lg { get; set; }
        public string VideoUrl { get; set; }
        public string Reason { get; set; }


    }
    public class Response
    {
        public string msg { get; set; }
        public bool res { get; set; }
    }
    public class salesPersonMobiledetails
    {
        public string salespersonmobile { get; set; }
        public string salespersonfcmid { get; set; }
        public int SalesId { get; set; }
        public string WHLeadmobile { get; set; }
        public string WHLeadfcmid { get; set; }
        public int WHLeadId { get; set; }
        public double TotalAmt { get; set; }
        public double GrossAmount { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
    }
    public class HolidayOnRedispatchDC
    {
        public string Holiday { get; set; }
        public string HolidayType { get; set; }
        public DateTime date { get; set; }
        public int UpdateCapacity { get; set; }
        public DateTime Deliverydate { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class GetOverdueLoanAccountIdRequest
    {
        public long LoanAccountID { get; set; }
    }

    public class GetOverdueLoanAccountResponse
    {
        public long LoanAccountID { get; set; }
        public bool TransactionStatus { get; set; }
    }
}
