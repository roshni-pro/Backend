using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder
{
    public class PurchaseOrderReturnDc
    {
        public POReturnPOMasterDC POMaster { get; set; }
        public List<POReturnPODetailDC> PODetailList { get; set; }
        public List<POReturnGoodReceivedDetailDC> GoodReceivedDetailList { get; set; }
        public List<POReturnInvoiceReceiptDetailDc> InvoiceReceiptDetailList { get; set; }
    }


    public class POReturnPOMasterDC
    {
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public double TotalAmount { get; set; }
        public bool Acitve { get; set; }
        public double Advance_Amt { get; set; }
        public double ETotalAmount { get; set; }
        public string PoType { get; set; }
        public string Comment { get; set; }
        public string CommentApvl { get; set; }
        public string Commentsystem { get; set; }
        public string progress { get; set; }
        public string ApprovedBy { get; set; }
        public string RejectedBy { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public string WarehouseCity { get; set; }
        public string PoInvoiceNo { get; set; }
        public string TransactionNumber { get; set; }
        public int SupplierStatus { get; set; }
    }

    public class POReturnPODetailDC
    {
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public string SellingSku { get; set; }
        public int ItemId { get; set; }
        public string HSNCode { get; set; }
        public string SKUCode { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }
        public double MRP { get; set; }
        public int MOQ { get; set; }
        public int TotalQuantity { get; set; }
        public string PurchaseSku { get; set; }
        public double TaxAmount { get; set; }
        public double TotalAmountIncTax { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public int ConversionFactor { get; set; }
        public string PurchaseName { get; set; }
        public double PurchaseQty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public string ItemNumber { get; set; }
        public string itemBaseName { get; set; }
        public long? PurchaseOrderNewId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class POReturnGoodReceivedDetailDC
    {
        public long Id { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int GrSerialNumber { get; set; }
        public double Price { get; set; }
        public int Status { get; set; }
        public int CurrentStockHistoryId { get; set; }
        public string BatchNo { get; set; }
        public DateTime? MFGDate { get; set; }
        public string Barcode { get; set; }
        public int ApprovedBy { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNumber { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public bool IsActive { get; set; }
        //public bool? IsDeleted { get; set; }
        //public int CreatedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        public bool? IsFreeItem { get; set; }
        public string Comment { get; set; }
        public string ItemName { get; set; }


        public int? POReturnRequestApprovedBy { get; set; }
        public DateTime? POReturnRequestApprovedDate { get; set; }
        public long? POReturnRequestId { get; set; }
    }

    public class POReturnInvoiceReceiptDetailDc
    {
        public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public long Id { get; set; }
        public int IRMasterId { get; set; }
        public long GoodsReceivedDetailId { get; set; }
        public int IRQuantity { get; set; }
        public double Price { get; set; }
        public double? DiscountPercent { get; set; }
        public double? DiscountAmount { get; set; }
        public double GSTPercentage { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? CessTaxPercentage { get; set; }
        public double GSTAmount { get; set; }
        public double TotalTaxAmount { get; set; }
        public double? CessTaxAmount { get; set; }
        public int Status { get; set; }
        public int ApprovedBy { get; set; }
        public string IRID { get; set; }
        public DateTime? InvoiceDate { get; set; }

        //public DateTime CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public bool IsActive { get; set; }
        //public bool? IsDeleted { get; set; }
        //public int CreatedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        public string Comment { get; set; }


        public int? POReturnRequestApprovedBy { get; set; }
        public DateTime? POReturnRequestApprovedDate { get; set; }
        public long? POReturnRequestId { get; set; }

        public string POReturnRequestStatus { get; set; }

        public string IRStatus { get; set; }
        
        public int PurchaseOrderId { get; set; }
        public string WarehouseName { get; set; }

    }

    public class POPagerDC
    {
        public List<POReturnRequestPageDc> RequestReturnList { get; set; }
        public int recordCount { get; set; }
    }
}
