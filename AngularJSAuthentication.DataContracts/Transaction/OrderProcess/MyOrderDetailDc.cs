using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.OrderProcess
{
    public class MyOrderDetailDc
    {
        public int orderid { get; set; }
        public DateTime Deliverydate { get; set; }

        public double GrossAmount { get; set; }
        public double PayableAmount { get; set; }
        public string ItemName { get; set; }
        public int qty { get; set; }
        public bool IsFreeItem { get; set; }
        public double ItemAmount { get; set; }
        public int OrderTakenSalesPersonId { get; set; }
        public string OrderTakenBy { get; set; }
        public string OrderTakenRating { get; set; }
        public string DeliveredBy { get; set; }
        public string OrderTakenSalesPersonMobile { get; set; }
        public string OrderTakenSalesPersonProfilePic { get; set; }
        public string DeliveryPersonMobile { get; set; }
        public string DboyProfilePic { get; set; }
        public string DeliveryBoyRating { get; set; }
        public int ItemCount { get; set; }
        public int TotalQty { get; set; }
        public bool IsETAEnable { get; set; }
        public int Rating { get; set; }
        public int WheelCount { get; set; }
        public List<int> WheelList { get; set; }
        public bool IsPlayWeel { get; set; }
        public List<OrderPaymentDCs> OrderPaymentDCs { get; set; }
        public bool IsOrderHold { get; set; }
        public DateTime? PrioritizedDate { get; set; }
    }
    public class OrderPaymentDCs
    {
        public int OrderId { get; set; }
        public string PaymentFrom { get; set; }
        public double Amount { get; set; }
        public string TransactionNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string statusDesc { get; set; }
        
    }
}
