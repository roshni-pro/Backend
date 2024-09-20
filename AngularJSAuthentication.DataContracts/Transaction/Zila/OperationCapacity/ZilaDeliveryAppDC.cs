using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Zila.OperationCapacity
{
    public class GetZilaTripDc
    {
        public long ZilaTripMasterId { get; set; }
        public int DboyId { get; set; }
        public int OrderCount { get; set; }
        public double TotalAmount { get; set; }
        public string TripCurrentStatus { get; set; }
        public bool IsFreezed { get; set; }
    }

    public class ZilaCustomTrip
    {
        public int TripNumber { get; set; }
        public long VehicleMasterId { get; set; }
        //public long ClusterId { get; set; }
        //public long? AgentId { get; set; }
        //public int WarehouseId { get; set; }
        public long DboyId { get; set; }
    }

    public class ZilaTripOrderVM
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string ClusterName { get; set; }
        public int ClusterId { get; set; }
        public string ShippingAddress { get; set; }
        public double Amount { get; set; }
        public long TimeInMins { get; set; }
        public long DistanceInMeter { get; set; }
        public long ZilaTripOrderId { get; set; }
        public bool IsActive { get; set; }
        public long ZilaTripDetailId { get; set; }
        public bool IsActiveOld { get; set; }
        public bool IsManuallyAdded { get; set; }
        public string Skcode { get; set; }
        public string Mobile { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public long CustomerId { get; set; }
        public double WeightInKg { get; set; }
        public string ShopName { get; set; }
        public string ReDispatchCount { get; set; }
        public bool IsRightLocation { get; set; }
        public bool IsAddableDueToCustomerLocation { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public bool IsNewPickerOrder { get; set; }
        public long? OrderPickerMasterId { get; set; }
        public string CRMTags { get; set; }
        public string OrderType { get; set; }
        public DateTime ETADate { get; set; }
        public string CustomerType { get; set; }
        public DateTime? ExpectedRtdDate { get; set; }
        public DateTime? PrioritizedDate { get; set; }

    }

    public class ZilaFinalizeTripParam
    {
        public long ZilaTripMasterId { get; set; }
        public int startingKm { get; set; }
        public DateTime reportingTime { get; set; }
        public long DriverId { get; set; }
        public long VehicleId { get; set; }
        public long? AgentId { get; set; }
        public long? DboyId { get; set; }
        public DateTime TripDate { get; set; }
       // public bool? IsReplacementVehicleNo { get; set; }
      //  public string ReplacementVehicleNo { get; set; }
       //    public double? VehicleFare { get; set; }
    }

    public class ZilaTripPlannerAppDashboardDC
    {
        public long ZilaTripMasterId { get; set; }
        public int CurrentStatus { get; set; }
        public bool IsTripEnd { get; set; }
        public long TripPlannerVehicleId { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public long BreakTimeInSec { get; set; }
        public double StartKm { get; set; }
        public ZilaTripPlannerDistance tripPlannerDistance { get; set; }
        public ZilaMyTrip myTrip { get; set; }
        public List<ZilaAssignmentList> assignmentList { get; set; }
        public ZilaPendingOrderlist OrderStatuslist { get; set; }
        public int CustomerTripStatus { get; set; }
        public int CustomerId { get; set; }
        public int ApproveNotifyTimeLeftInMinute { get; set; }
        public int TripWorkingStatus { get; set; }
        //public bool IsNotLastMileTrip { get; set; }
        //public bool IsLocationEnabled { get; set; }
        public bool IsSKFixVehicle { get; set; }
    }
    public class ZilaTripPlannerDistance
    {
        public long TravelTime { get; set; }
        public long ReminingTime { get; set; }
        public double DistanceTraveled { get; set; }
        public double DistanceLeft { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public long TotalTime { get; set; }
    }
    public class ZilaMyTrip
    {
        public int CustomerCount { get; set; }
        public int TotalOrder { get; set; }
        public double TotalAmount { get; set; }
        public long? TripId { get; set; }
    }
    public class ZilaAssignmentList
    {
        public int AssignmentId { get; set; }
        public int NoOfOrder { get; set; }
        public double? Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public string AssignmentStatus { get; set; }
    }
    public class ZilaPendingOrderlist
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
    public class ZResponceDc
    {
        public ZilaTripPlannerAppDashboardDC TripDashboardDC { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    #region GetTripOrders

    public class ZilaTripBriefDc
    {
        public long ZilaTripMasterId { get; set; }
        public bool IsFinalized { get; set; }
        public List<ZilaCustomerBriefDc> CustomerList { get; set; }
    }
    public class ZilaCustomerBriefDc
    {
        public ZilaCustomerInfoBriefDc customerInfo { get; set; }
        public List<ZilaCustomerOrderBriefInfo> customerOrderInfo { get; set; }

    }

    public class ZilaCustomerInfoBriefDc
    {
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string BillingAddress { get; set; }
        public double GrossAmount { get; set; }
        public int CustomerId { get; set; }
    }
    public class ZilaCustomerOrderBriefInfo
    {
        public int OrderId { get; set; }
        public double GrossAmount { get; set; }
        public string Status { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public int ReDispatchCount { get; set; }
        public int WorkingStatus { get; set; }
        public int WarehouseId { get; set; }
    }
    public class ZilaItemuUnloadingDC
    {
        public long zilaTripDetailId { get; set; }
        public List<int> OrderId { get; set; }
    }
    #endregion

    public class ZilaGetUnloadItemListPageDC
    {
        public List<ZilaUnloadItemListDC> UnloadItemListPage { get; set; }
        public ZilaUnloadItemTotal unloadItemTotal { get; set; }
    }
    public class ZilaUnloadItemListDC
    {
        public long TripPlannerConfirmedDetailId { get; set; }
        public int OrderId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Itemname { get; set; }
        public int Qty { get; set; }
        public bool IsUnloaded { get; set; }
        public bool IsDone { get; set; }
    }
    public class ZilaUnloadItemTotal
    {
        public int TotalQty { get; set; }
        public int TotalItem { get; set; }
        public double TotalAmount { get; set; }
    }
    public class ZilaCheckUncheckDc
    {
        public long ZilaTripDetailId { get; set; }
        public List<int> ItemMultiMRPId { get; set; }
        public bool IsUnloaded { get; set; }
    }
    public class ZilaResponceDc
    {
        public ZilaTripPlannerAppDashboardDC TripDashboardDC { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class ZilaAssignmentStartEndDc
    {
        public long ZilaTripMasterId { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        //public double? ClosingKm { get; set; }
        //public string ClosingKMUrl { get; set; }
        //public bool IsClosingKmManualReading { get; set; }
        //public string StartKmUrl { get; set; }
        //public int StartKm { get; set; }

        public int PeopleID { get; set; }

    }

    public class ZilamytripOrderListDc
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
        public long ZilaTripDetailId { get; set; }
        public int CustomerId { get; set; }
        public int WorkingStatus { get; set; }
        public string Reason { get; set; }
        public bool IsOTPSent { get; set; }
        public int OTPSentRemaningTimeInSec { get; set; }
        public string RecordingUrl { get; set; }
        //public bool IsVisible { get; set; }
        //public bool IsSkip { get; set; }
        //public bool IsNotLastMileTrip { get; set; }
        public bool IsAnyTripRunning { get; set; }
        public bool IsProcess { get; set; }
        public int CustomerTripStatus { get; set; }
        //public bool IsLocationEnabled { get; set; }
        //public int TripTypeEnum { get; set; }
        public string OrderType { get; set; }
    }

    public class ZilaMytripOrderCustomerWiseDc
    {
        public double lg { get; set; }
        public double lat { get; set; }
        public long ZilaTripDetailId { get; set; }
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
        public List<ZilaOrderList> ZorderList { get; set; }

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
    public class ZilaOrderList
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

    public class ZilaCheckTripOrderCurrentStatusDC
    {
        public long ZilaTripOrderId { get; set; }
        public int CurrentStatus { get; set; }
        public string Skcode { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int AssignmentId { get; set; }
        public int SequenceNo { get; set; }
        public bool IsProcess { get; set; }
        public int CustomerTripStatus { get; set; }
    }


    public class ZilaSingleOrderMapview
    {
        public ZilaSingleOrderMapviewInfoDC singleOrderMapviewInfoDC { get; set; }
        public ZilaCustomerOrderinfoDc customerOrderinfoDc { get; set; }
    }
    public class ZilaSingleOrderMapviewInfoDC
    {
        public long OrderCompletionTime { get; set; }
        public long UnloadingTime { get; set; }
        public long TotalDistanceInMeter { get; set; }
        public bool IsProcess { get; set; }
        public double WarehouesLat { get; set; }
        public double WarehouesLng { get; set; }
        public bool IsDestinationReached { get; set; }
    }
    public class ZilaCustomerOrderinfoDc
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



    public class ZilaGetCustomerWiseOrderListDc
    {
        public ZilaCustomerInfo customerInfo { get; set; }
        public List<ZilaCustomerOrderInfo> customerOrderInfo { get; set; }

    }

    public class ZilaCustomerInfo
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
    public class ZilaCustomerOrderInfo
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
        public List<ZilaOrderDetailsDC> OrderDetails { get; set; }
        public bool IsDeliveryCancelledEnable { get; set; }
        public string OrderType { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public bool IsETAOrderHide { get; set; }
        public int WarehouseId { get; set; }

        public bool IsScaleUpCustomer { get; set; }
        public bool IsScaleUpPaymentOverdue { get; set; }
    }
    public class ZilaOrderDetailsDC
    {
        public int OrderId { get; set; }
        public string itemname { get; set; }
        public double TotalAmt { get; set; }
        public int qty { get; set; }
        public string Status { get; set; }
    }
    public class ZilaUnlodingDc
    {
        public long ZilaTripMasterId { get; set; }
        public long ZilaTripOrderId { get; set; }
        public int OrderId { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public long CurrentServingOrderId { get; set; }
        public long ZilaTripDetailId { get; set; }
    }
    public class ZilaGetCollectPaymentOrderListDc
    {
        public ZilaCustomerInfo customerInfo { get; set; }
        public List<ZilaPaymentGroupwise> PaymentGroupwisesList { get; set; }
    }
    public class ZilaPaymentGroupwise
    {
        public string PaymentMode { set; get; }
        public List<ZilaCustomerOrderInfo> customerOrderInfo { get; set; }
    }
    public class ZilaNewResponceDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public long ZilaTripDetailId { get; set; }
    }
    public class ZilaCollectPaymentDC
    {
        public long ZilaTripDetailId { get; set; }
        public long ZilaTripOrderId { get; set; }
        public List<int> OrderIds { get; set; }
        public double CashAmount { get; set; }
        public List<ZilaTripDeliveryPaymentDTO> TripDeliveryPayments { get; set; }
        public string BlockCashAmountGuid { get; set; }
    }
    public class ZilaTripDeliveryPaymentDTO
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
    public class ZilaOrderConfirmOtpDc
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
    public class ZilaOrderResponceDc
    {
        public ZilaOrderConfirmOtpDc OrderConfirmOtpDc { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class ZilaSendCloseKmApprovalRequestDc
    {
        public long zilaTripMasterId { get; set; }
        public int closeKm { get; set; }
        public string closeKmUrl { get; set; }
        public int PeopleID { get; set; }
    }
    public class ZilaTripRatingDboyDC
    {
        public ZilaTripDeliveryDboyRatingOrderDc DeliveryDboyRatingOrder { get; set; }
        public List<ZilaUserRatingDc> userRatingDc { get; set; }
    }
    public class ZilaTripDeliveryDboyRatingOrderDc
    {
        public long OrderId { set; get; }
        public int CustomerId { set; get; }
        public string Shopimage { set; get; }
        public string ShopName { set; get; }
        public string ShippingAddress { set; get; }
    }
    public class ZilaUserRatingDc
    {
        public int UserId { set; get; }  //(Sales  , Delivery ) : PeopleId   &  Retailer  2 : CustomerId
        public int AppType { set; get; } // 1 : Sales Rating   , 2: Delivery Rating , 3:  Retailer Rating
        public string AppTypeName { set; get; }
        public int Rating { set; get; }
        public int? OrderId { set; get; }
        public string ShopVisited { set; get; }  // Use only for Sales  , Delivery 
        public List<ZilaUserRatingDetailDc> RatingDetails { set; get; }
        public string DisplayName { set; get; }
        public string ProfilePic { set; get; }
        public DateTime OrderedDate { set; get; }
        public bool IsRemoveFront { set; get; }
        public bool IsTrip { set; get; }
    }
    public class ZilaUserRatingDetailDc
    {
        public string Detail { set; get; }
    }
}
