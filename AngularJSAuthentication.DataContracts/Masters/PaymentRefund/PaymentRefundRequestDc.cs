using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.PaymentRefund
{
    public class PaymentRefundRequestDc
    {
        public int OrderId { set; get; }
        public string ReqGatewayTransId { get; set; } //    GatewayTransId of PaymentResponseRetailerApps 
        public double Amount { set; get; }
        public string Source { set; get; }
        public int Status { get; set; } // 0: Pending, 1:Success , 2: Failed  (PaymentRefundEnum)//
        public string RequestMsg { get; set; }
        public string ResponseMsg { get; set; }
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int RefundType { get; set; }
        public int PaymentResponseRetailerAppId { set; get; } // PaymentResponseRetailerApps 


    }


    public class PaymentRefundResDc
    {
        public long PaymentRefundRequestId { set; get; }
        public bool Status { get; set; }
        public string RequestMsg { get; set; }
        public string ResponseMsg { get; set; }
        public int CustomerId { set; get; }
        public int OrderId { set; get; }

    }
    public class PaymentRefundResponse
    {
        public List<PaymentRefundListDc> PaymentRefundListDcs { get; set; }
        public List<PaymentRefundListExportDc> PaymentRefundListExportDcs { get; set; }
        public List<PaymentRefundHistoryDc> PaymentRefundHistoryDcs { get; set; }
        public int TotalCount { get; set; }
    }
    public class SearchPaymentRefundExportDc
    {
        public string Keyward { get; set; }
        public List<int> WarehouseIds { get; set; }
        public List<string> MOPs { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int RefundType { get; set; }
        public int status { get; set; }
    }
    public class SearchPaymentRefundDc
    {
        public string Keyward { get; set; }
        public List<int> WarehouseIds { get; set; }
        public List<string> MOPs { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public int RefundType { get; set; }
        public int status { get; set; }
    }

    public class PaymentRefundListDc
    {
        public long Id { set; get; }
        public string Skcode { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderAmount { get; set; }
        public double DispatchAmount { get; set; }
        public string MOP { get; set; }
        public double RefundAmount { get; set; }
        public double? OnlineAmount { get; set; }
        public DateTime RefundDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Status { get; set; }
        public int WarehouseId { get; set; }
        public string ReturnType { get; set; }
        public string OrderStatus { get; set; }        

    }
    public class PaymentRefundListExportDc
    {
        public long Id { set; get; }
        public string Skcode { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderAmount { get; set; }
        public string OrderStatus { get; set; }
        public double DispatchAmount { get; set; }
        public string MOP { get; set; }
        public double RefundAmount { get; set; }
        public DateTime RefundDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Status { get; set; }
        //public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ReturnType { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }

    }
    public class PaymentRefundHistoryDc
    {
        public int Id { set; get; }
        public string status { get; set; }
        public string RequestMsg { get; set; }
        public string ResponseMsg { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DisplayName { get; set; }
        public string Comment { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class HdfcRefundPostDc
    {
        public string reference_no { get; set; }
        public double refund_amount { get; set; }
        public string refund_ref_no { get; set; } // unique

    }

    public class RefundOrderResult
    {
        public string reason { get; set; }
        public string refund_status { get; set; }
        public string error_code { get; set; }

    }

    public class RefundAPIResponseDc
    {
        public RefundOrderResult Refund_Order_Result { get; set; }
    }
}
