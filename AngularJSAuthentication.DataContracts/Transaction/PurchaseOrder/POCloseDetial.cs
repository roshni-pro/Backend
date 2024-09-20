using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder
{
    public class POCloseRequest
    {
        public List<int> WarehouseIds { get; set; }
        public List<int> SupplierIds { get; set; }
        public string PurchaseOrderId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class POCloseDetial
    {
        public int PurchaseOrderId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreationDate { get; set; }        
        public string BuyerName { get; set; }
        public string SupplierName { get; set; }
        public string Status { get; set; }
        public string Itemname { get; set; }
        public int TotalPOQty { get; set; }
        public int GrTotalQty { get; set; }
        public int IRTotalQty { get; set; }
        public decimal TotalPOPrice { get; set; }
        public decimal GRAmount { get; set; }
        public decimal IrAmount { get; set; }
        public bool IsCloseRejected { get; set; }
        public bool IsPOCloseProgress { get; set; }

        public int? ApprovalStatus { get; set; }
        public string PickerType { get; set; }
        public double FreightCharge { get; set; }
    }

    public class PODetial
    {
        public int PurchaseOrderId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreationDate { get; set; }
        public string BuyerName { get; set; }
        public string SupplierName { get; set; }
        public string Status { get; set; }
        public decimal TotalPOPrice { get; set; }
        public decimal TotalGRAmount { get; set; }
        public decimal TotalIrAmount { get; set; }

        public int? ApprovalStatus{ get; set; }
        public bool IsCloseRejected { get; set; }
        public bool IsPOCloseProgress { get; set; }        
        public DateTime SendApprovalDate { get; set; }
        public string PickerType { get;set; }
        public double FreightCharge { get;set; }
        public List<POItemDetial> POItemDetials { get; set; }
    }
    public class POItemDetial
    {
        public int ItemMultiMRPId { get; set; }
        public string Itemname { get; set; }
        public int TotalPOQty { get; set; }
        public int TotalGrQty { get; set; }
        public int TotalIRQty { get; set; }
        public decimal TotalPOPrice { get; set; }
        public decimal TotalGRAmount { get; set; }
        public decimal TotalIrAmount { get; set; }
    }

    public class POCloseApprovalRequest
    {
        public int purchaseOrderId { get; set; }
        public int ApprovedStatus { get; set; }
        public string RejectedReasion { get; set; }
        public List<int> purchaseOrderIds { get; set; }
    }
}
