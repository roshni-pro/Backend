using AngularJSAuthentication.Model.TripPlanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.TripPlanner
{
    public class TripPlannerAppDashboardDC
    {
        public long TripPlannerConfirmedMasterId { get; set; }
        public int CurrentStatus { get; set; }
        public bool IsTripEnd { get; set; }
        public long TripPlannerVehicleId { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public long BreakTimeInSec { get; set; }
        public double StartKm { get; set; }
        public TripPlannerDistance tripPlannerDistance { get; set; }
        public MyTrip myTrip { get; set; }
        public List<AssignmentList> assignmentList { get; set; }
        public PendingOrderlist OrderStatuslist { get; set; }
        public int CustomerTripStatus { get; set; }
        public int CustomerId { get; set; }
        public int ApproveNotifyTimeLeftInMinute { get; set; }

        public int TripWorkingStatus { get; set; }
        public bool IsNotLastMileTrip { get; set; }
        public bool IsLocationEnabled { get; set; }
        public bool IsSKFixVehicle { get; set; }
    }
    public class TripPlannerDistance
    {
        public long TravelTime { get; set; }
        public long ReminingTime { get; set; }
        public double DistanceTraveled { get; set; }
        public double DistanceLeft { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public long TotalTime { get; set; }

    }
    public class MyTrip
    {
        public int CustomerCount { get; set; }
        public int TotalOrder { get; set; }
        public double TotalAmount { get; set; }
        public long? TripId { get; set; }
    }
    public class AssignmentList
    {
        public int AssignmentId { get; set; }
        public int NoOfOrder { get; set; }
        public double? Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public string AssignmentStatus { get; set; }
    }
    public class PendingOrderlist
    {
        public int TotalShippedOrder { get; set; }
        public double TotalShippedAmount { get; set; }
        public int TotalDeliveredOrder { get; set; }
        public double TotalDeliveredAmount { get; set; }
        public int TotalDeliveryCanceledOrder { get; set; }
        public double TotalDeliveryCanceledAmount { get; set; }
        public int TotalDeliveryRedispatchOrder { get; set; }
        public double TotalDeliveryRedispatchAmount { get; set; }
        public int TotalReAttemptOrder { get; set; }
        public double TotalReAttemptAmount { get; set; }
    }
    public class ResponceDc
    {
        public TripPlannerAppDashboardDC TripDashboardDC { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class NotifyDeliveryCancledResDc
    {
        public salesPersonDc salesPersonDc { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class salesPersonDc
    {
        public string SalePersonMobile { get; set; }
        public string SalePersonName { get; set; }
    }

    public class GetOrderResponceDc
    {
        public TripPlannerConfirmedOrder tripPlannerConfirmedOrder { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public enum VehicleliveStatus
    {
        OnDuty = 1,
        NotStarted = 2,
        InTransit = 3,
        OnBreak = 4,
        Delivering = 5,
        TripEnd = 6,
        RejectTrip = 7,
        CustomerFinalized = 8
    }
    public class AssignmentStartEndDc
    {
        public long TripPlannerConfirmedMasterId { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        //public double? ClosingKm { get; set; }
        //public string ClosingKMUrl { get; set; }
        public bool IsClosingKmManualReading { get; set; }
        public string StartKmUrl { get; set; }
        public int StartKm { get; set; }

        public int PeopleID { get; set; }

    }
    public class mytripOrderListDc
    {
        public double lg { get; set; }
        public double lat { get; set; }
        public int SequenceNo { get; set; }
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public double GrossAmount { get; set; }
        public string Status { get; set; }
        public string WarehouseAddress { get; set; }
        public string CustomerAddress { get; set; }
        public int NoOfItems { get; set; }
        public long TotalTimeInMins { get; set; }
        public long TimeInMins { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public string CustomerMobile { get; set; }
        public int ReDispatchCount
        {
            get;
            set;
        }
        public int ReAttemptCount { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public int CustomerId { get; set; }
        public int WorkingStatus { get; set; }
        public string Reason { get; set; }
        public bool IsOTPSent { get; set; }
        public int OTPSentRemaningTimeInSec { get; set; }
        public string RecordingUrl { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSkip { get; set; }
        public bool IsNotLastMileTrip { get; set; }
        public bool IsAnyTripRunning { get; set; }
        public bool IsProcess { get; set; }
        public int CustomerTripStatus { get; set; }
        public bool IsLocationEnabled { get; set; }
        public int TripTypeEnum { get; set; }
        public string OrderType { get; set; }
    }
    public class MytripOrderCustomerWiseDc
    {
        public double lg { get; set; }
        public double lat { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public int SequenceNo { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public double TotalOrderAmount { get; set; }
        public string WarehouseAddress { get; set; }
        public string CustomerAddress { get; set; }
        public int NoOfItems { get; set; }
        public long OrderCompletionTime { get; set; }
        public long UnloadingTime { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public string CustomerMobile { get; set; }
        public int CustomerId { get; set; }
        public int WorkingStatus { get; set; }
        public string Reason { get; set; }
        public bool IsOTPSent { get; set; }
        public int OTPSentRemaningTimeInSec { get; set; }
        public bool IsReAttemptShow { get; set; }
        public bool IsReDispatchShow { get; set; }
        public string RecordingUrl { get; set; }
        public bool IsVisible { get; set; }
        public bool IsSkip { get; set; }
        public List<OrderList> orderList { get; set; }

        //IsNonLastMileTrip 21-07-2022
        public bool IsNotLastMileTrip { get; set; }
        public bool IsAnyTripRunning { get; set; }
        public bool IsProcess { get; set; }
        public int CustomerTripStatus { get; set; }
        public bool IsLocationEnabled { get; set; }
        public int TripTypeEnum { get; set; }
        public string CRMTags { get; set; }
        public bool IsReturnOrder { get; set; }
        public bool IsGeneralOrder { get; set; }
    }
    public class OrderList
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public double Amount { get; set; }
        public long TotalTimeInMins { get; set; }
        public long TimeInMins { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public int NoOfItems { get; set; }
        public int ReDispatchCount { get; set; }
        public int ReAttemptCount { get; set; }
        public string OrderType { get; set; }
    }

    public class MapviewListDc
    {
        public Mapview mapview { get; set; }
        public List<CustomerListDc> customerListDc { get; set; }

    }
    public class Mapview
    {
        public long TotalKM { get; set; }
        public long TotalOrderCompletionTime { get; set; }
        public long TotalunlodingTime { get; set; }
        public double WarehouesLat { get; set; }
        public double WarehouesLng { get; set; }
        public int CustomerTripStatus { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string ShippingAddress { get; set; }
    }

    public class CustomerListDc
    {
        public int CustomerId { get; set; }
        public string MobileNumber { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public int OrderCount { get; set; }
        public string ShippingAddress { get; set; }
        public double TotalAmount { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public long TotalTimeInMins { get; set; }
        public long UnlodingTime { get; set; }
    }

    public class SingleOrderMapview
    {
        public SingleOrderMapviewInfoDC singleOrderMapviewInfoDC { get; set; }
        public CustomerOrderinfoDc customerOrderinfoDc { get; set; }
    }
    public class SingleOrderMapviewInfoDC
    {
        public long OrderCompletionTime { get; set; }
        public long UnloadingTime { get; set; }
        public long TotalDistanceInMeter { get; set; }
        public bool IsProcess { get; set; }
        public double WarehouesLat { get; set; }
        public double WarehouesLng { get; set; }
        public bool IsDestinationReached { get; set; }
    }
    public class CustomerOrderinfoDc
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public long TripPlannerConfirmedOrderId { get; set; }
        public int SequenceNo { get; set; }
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string ShippingAddress { get; set; }
        public int OrderCount { get; set; }
        public bool IsProcess { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string MobileNumber { get; set; }
        public string VoiceNote { get; set; }
        public bool IsTakeShopImage { get; set; }
        public bool IsReturnOrder { get; set; }
        public bool IsGeneralOrder { get; set; }

    }

    public class GetCustomerWiseOrderListDc
    {
        public CustomerInfo customerInfo { get; set; }
        public List<CustomerOrderInfo> customerOrderInfo { get; set; }

    }

    public class CustomerInfo
    {
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string BillingAddress { get; set; }
        public double GrossAmount { get; set; }
        public int CustomerId { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public int OTPSentRemaningTimeInSec { get; set; }
        public bool IsOTPSent { get; set; }
        public double TotalDeliverd { get; set; }
        public bool isReAttempt { get; set; }
        public bool isAllPaymentDone { get; set; }
        public bool IsQREnabled { set; get; }  //By Harry : 15March2023
        public string CRMTags { get; set; }

    }
    public class CustomerOrderInfo
    {
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public int NoOfItems { get; set; }
        public string Status { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public int ReDispatchCount { get; set; }
        public bool OnilnePayment { get; set; }
        public int WorkingStatus { get; set; }
        public bool boolWorkingStatus { get; set; }
        public string PaymentFrom { get; set; }
        public double OnlineAmount { get; set; }
        public double RemaningOrderAmount { get; set; }
        public long TripPlannerConfirmedOrderId { get; set; }
        public int ReAttemptCount { get; set; }
        public int OrderReDispatchCount { get; set; }
        public int RemaningTimeInMins { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public string Reason { get; set; }
        public bool IsOTPSent { get; set; }
        public bool IsReAttempt { get; set; }
        public string MessageText { get; set; }
        public int OTPSentRemaningTimeInSec { get; set; }
        public bool IsPaymantDone { get; set; }
        public string SalePersonMobile { get; set; }
        public string SalePersonName { get; set; }
        public double RemaningAmount { get; set; }
        public List<OrderDetailsDC> OrderDetails { get; set; }
        public bool IsDeliveryCancelledEnable { get; set; }
        public string OrderType { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public bool IsETAOrderHide { get; set; }
        public int WarehouseId { get; set; }

        public bool IsScaleUpCustomer{ get; set; }
        public bool IsScaleUpPaymentOverdue { get; set; }
    }
    public class OrderDetailsDC
    {
        public int OrderId { get; set; }
        public string itemname { get; set; }
        public double TotalAmt { get; set; }
        public int qty { get; set; }
        public string Status { get; set; }
    }

    public class CheckTripOrderCurrentStatusDC
    {
        public long TripPlannerConfirmedOrderId { get; set; }
        public int CurrentStatus { get; set; }
        public string Skcode { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int AssignmentId { get; set; }
        public int SequenceNo { get; set; }
        public bool IsProcess { get; set; }
        public int CustomerTripStatus { get; set; }
    }
    public class OnBreakDC
    {
        public long TripPlannerConfirmedMasterId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
    public class AssignmentIDc
    {
        public int DeliveryIssuanceId { get; set; }
        public long TripPlannerConfirmedMasterId { get; set; }
        public bool IsFreezed { get; set; }
    }

    public class orderlistDC
    {
        public List<orderlist> orderlists { get; set; }
        public int TotalCount { get; set; }
    }
    public class orderlist
    {
        public long OrderId { get; set; }
    }

    public class CheckPlanningDc
    {

        public double TotalWeightInKg { get; set; }
        public int CustomerCount { get; set; }
        public double TotalAmount { get; set; }
        public long TotalDistanceInMeter { get; set; }
        public long TotalTimeInMins { get; set; }
        public string VehicleType { get; set; }
        public int OrderCount { get; set; }
        public long TripNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsFreezed { get; set; }
    }
    public class TripPlannerVehicleHistorieDC
    {
        public long CurrentServingOrderId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int RecoardStatus { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public long TripPlannerVehicleId { get; set; }
    }
    public class UnlodingDc
    {
        public long TripPlannerConfirmedMasterId { get; set; }
        public long TripPlannerConfirmedOrderId { get; set; }
        public int OrderId { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public long CurrentServingOrderId { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
    }
    public class TripPlannerItemCheckListDC
    {
        public int OrderId { get; set; }
        public string Itemname { get; set; }
        public int Qty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool IsUnloaded { get; set; }
        public long TripPlannerConfirmedOrderId { get; set; }
    }

    public class ItemuUnloadingDC
    {

        public long tripPlannerConfirmedDetailId { get; set; }
        public List<int> OrderId { get; set; }
    }


    public class CheckUncheckDc
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public List<int> ItemMultiMRPId { get; set; }
        public bool IsUnloaded { get; set; }


    }

    public class GetUnloadItemListPageDC
    {
        public List<UnloadItemListDC> UnloadItemListPage { get; set; }
        public UnloadItemTotal unloadItemTotal { get; set; }

    }
    public class UnloadItemListDC
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public int OrderId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Itemname { get; set; }
        public int Qty { get; set; }
        public bool IsUnloaded { get; set; }
        public bool IsDone { get; set; }
    }
    public class UnloadItemTotal
    {
        public int TotalQty { get; set; }
        public int TotalItem { get; set; }
        public double TotalAmount { get; set; }
    }
    public class ReturnItemListDC
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public int NewOrderId { get; set; }
        public int NewOrderDetailId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Itemname { get; set; }
        public int Qty { get; set; }
        public double UnitPrice { get; set; }
        public string BatchCode { get; set; }
        public long BatchId { get; set; }
        public string Status { get; set; }
        public long KKRRRequestId { get; set; }
    }
    public enum WorKingStatus
    {
        Pending = 0,
        Unloading = 1,
        CollectingPayment = 2,
        AlredyCollected = 3,
        Completed = 4,
        failed = 5,
        DeliveryCanceled = 6,
        DeliveryRedispatch = 7,
        ReAttempt = 8
    }
    public enum CustomerTripStatusEnum
    {
        Pending = 0,
        unloading = 1,
        CollectingPayment = 2,
        VerifyingOTP = 3,
        Completed = 4,
        Skip = 5,
        ReattemptAll = 6,
        RedispatchAll = 7,
        NotifyDeliveryCancelled = 8,
        RedispatchAndOrderCancelVerifyingOTP = 9,
        AllVerifyingOTP = 10,
        ReachedDistination = 11,
        NotifyDeliveryRedispatch = 12,
        NotifyReAttempt = 13


    }
    public enum OrderStatusEnum
    {
        Delivered = 1,
        DeliveryCanceled = 2,
        DeliveryRedispatch = 3,
        ReAttempt = 4
    }
    public static class PaymentFromConstants
    {
        public const string Cash = "Cash";
        public const string RazorpayQR = "Razorpay QR";
        public const string DirectUdhar = "DirectUdhar";
        public const string chqbook = "chqbook";
        public const string Cheque = "Cheque";
        public const string hdfc = "hdfc";
        public const string RTGSNEFT = "RTGS/NEFT";
        public const string ePaylater = "ePaylater";
        public const string mPos = "mPos";
        public const string Gullak = "Gullak";
        public const string CreditHdfc = "credit hdfc";
    }

    public class GetCollectPaymentOrderListDc
    {
        public CustomerInfo customerInfo { get; set; }
        public List<PaymentGroupwise> PaymentGroupwisesList { get; set; }
    }

    public class PaymentGroupwise
    {
        public string PaymentMode { set; get; }
        public List<CustomerOrderInfo> customerOrderInfo { get; set; }
    }
    public class CollectPaymentDC
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public long TripPlannerConfirmedOrderId { get; set; }
        public List<int> OrderIds { get; set; }
        public double CashAmount { get; set; }
        public List<TripDeliveryPaymentDTO> TripDeliveryPayments { get; set; }
        public string BlockCashAmountGuid { get; set; }
    }

    public class DeliveryCancledConfirmOtpDc
    {
        public int OrderId { set; get; }
        public int Otp { set; get; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public double? lat { get; set; }
        public double? lg { get; set; }
    }

    public class OrderConfirmOtpDc
    {
        public string Otp { set; get; }
        public int OrderId { set; get; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public string DboyMobileNo { get; set; }
        public string Status { get; set; }
        public string comments { get; set; }
        public double? DeliveryLat { get; set; }
        public double? DeliveryLng { get; set; }
        public string NextRedispatchedDate { set; get; }
        public int userId { get; set; }
        public bool ReAttempt { get; set; }
        public bool IsReturnOrder { get; set; }
    }
    public class TripDeliveryPaymentDTO
    {
        public string TransId { get; set; }
        public double amount { get; set; }
        public string PaymentFrom { get; set; }
        public string ChequeImageUrl { get; set; }
        public string ChequeBankName { get; set; }
        public DateTime PaymentDate { get; set; }//payemnt Date
        public double RemaingAmount { get; set; }
        public bool IsVAN_RTGSNEFT { get; set; }

    }
    public class ReDispatReAttemptConfirmOtpDc
    {
        public int Otp { set; get; }
        public int OrderId { set; get; }
        public DateTime NextRedispatchedDate { set; get; }
    }

    public class NotifyDeliveryCancledDc
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public string Reason { set; get; }
        public int OrderId { set; get; }
        public double? lat { get; set; }
        public double? lg { get; set; }
    }

    public class NotifyDeliveryActionDC
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public string Reason { set; get; }
        public int OrderId { set; get; }
        public double? lat { get; set; }
        public double? lg { get; set; }
        public string VideoUrl { get; set; }
        public string Action { get; set; }
        public DateTime? NextRedispatchedDate { set; get; }
    }

    public class ReDispatchAndReAttemptNewDC
    {
        public bool IsReAttempt { set; get; }
        public string Reason { set; get; }
        public int OrderId { set; get; }
        public DateTime? NextRedispatchedDate { set; get; }
        public double? lat { get; set; }
        public double? lg { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public string VideoUrl { get; set; }
    }
    public class ReDispatchAndReAttemptDc
    {
        public bool IsReAttempt { set; get; }
        public string Reason { set; get; }
        public int OrderId { set; get; }
        public string NextRedispatchedDate { set; get; }
        public double? lat { get; set; }
        public double? lg { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
    }
    public class AllRedispatReattemptConfirmOtp
    {
        public bool ReAttempt { set; get; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public string Reason { set; get; }
        public string Otp { set; get; }
        public string NextRedispatchedDate { set; get; }
        public double? lat { get; set; }
        public double? lg { get; set; }
        public int userId { get; set; }
    }

    public class AllRedispatReattemptrequest
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public bool IsRedispatch { get; set; }
        public string Reason { set; get; }
        public DateTime? NextRedispatchedDate { set; get; }
        public double? lat { get; set; }
        public double? lg { get; set; }
    }

    public class TripOrderMobiledetail
    {
        public string salespersonmobile { get; set; }
        public string salespersonfcmid { get; set; }
        public string agentmobile { get; set; }
        public string agentfcmid { get; set; }
        public string customermobile { get; set; }
        public string customerfmcid { get; set; }
        public int warehouseid { get; set; }
    }
    public class OTPOrderResponceDc
    {
        public string OTP { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class OrderResponceDc
    {
        public OrderConfirmOtpDc OrderConfirmOtpDc { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class NewResponceDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
    }
    public class NewDeliveryResDTO
    {
        public OrderConfirmOtpDc op { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
    public class ResCheckRemaingOrderStatusDC
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public CheckRemaingOrderStatusDC checkRemaingOrderStatusDC { get; set; }
    }

    public class CheckRemaingOrderStatusDC
    {
        public int OrderCount { get; set; }
        public double TotalAmount { get; set; }
        public bool IsProcess { get; set; }
    }
    public class CustomerUnloadLocationDC
    {
        public int CustomerId { get; set; }
        public string ShopImageUrl { get; set; }
        public double latitude { get; set; }
        public double Longitude { get; set; }
        public int UserId { get; set; }
    }
    public class ResCustomerUnloadLocationDC
    {
        public CustomerUnloadLocation customerUnloadLocationDC { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
    public class TripGetRedispatchOrderDc
    {
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public string invoice_no { get; set; }
        public string Status { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public string CustomerType { get; set; }
        public string ClusterName { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime Deliverydate { get; set; }
        public int Redispatchcount { get; set; }
        public bool? IsApproved { get; set; }
        public string RedispatchStatus { get; set; }
    }
    public class VANTransactionsDC
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public string ObjectType { get; set; }
        public long ObjectId { get; set; }
        public string Comment { get; set; }
        public int CustomerId { get; set; }
        public double UsedAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string AlertSequenceNo { get; set; }
        public string UserReferenceNumber { get; set; }
        public long VANTransactionParentId { get; set; }
    }

    public class SendCloseKmApprovalRequestDc
    {
        public long tripPlannerConfirmMasterId { get; set; }
        public int closeKm { get; set; }
        public string closeKmUrl { get; set; }
        public int PeopleID { get; set; }

    }
    public class GetSKP_KPP_OwnerListDC 
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
       //public string Name { get; set; }
        public string ShopName { get; set; }
    }
    public class SKPResDc
    {
        public List<GetSKP_KPP_OwnerListDC> GetSKP_KPP_OwnerList { get; set; }
        public bool status { get; set; }
        public string msg { get; set; }
    }
    public enum TripTypeEnum
    {
        City = 0,
        SKP = 1,
        KPP = 2,
        Damage_Expiry = 3,
        NonSellable = 4,
        NonRevenue=5
    }
    public class GetTripDc
    {
        public long TripPlannerConfirmedMasterId { get; set; }
        public long TripPlannerMasterId { get; set; }
        public int DboyId { get; set; }
        public bool IsNotLastMileTrip { get; set; }
        public int OrderCount { get; set; }
        public double TotalAmount { get; set; }
        public string TripType { get; set; }
        public string TripCurrentStatus { get; set; }
        public bool IsLocationEnabled { get; set; }
    }
    public class StatusDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class TripPlannerConfirmedDc
    {
        public int NoOfOrderDelivered { get; set; }
        public int OrderAmountDelivered { get; set; }
        public int TotalWeightDelivered { get; set; }
        public int TouchPointVisited { get; set; }


        public int VisitedNoOfOrder { get; set; }
        public int VisitedOrderAmount { get; set; }
        public int VisitedTotalWeight { get; set; }
    }


    public class TeleCallerDc
    {
        public string DisplayName { set; get; }
        public string  Mobile { set; get; }
        
    }
}
