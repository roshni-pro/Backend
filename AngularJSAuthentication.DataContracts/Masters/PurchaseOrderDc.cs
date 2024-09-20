using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularJSAuthentication.Model
{
    public class PurchaseOrderMasterDc
    {
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public double ETotalAmount { get; set; }
        public string PoType { get; set; }
        public string Level { get; set; }
        public string progress { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string RejectedBy { get; set; }
        public string CanceledByName { get; set; }
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public List<PurchaseOrderDetailDc> PoItemDetail { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string GrNumber { get; set; }
        public string GrPersonName { get; set; }
        public DateTime? Gr_Date { get; set; }
        public bool IsAnyRejectedGr { get; set; } //IsRejected any Gr in Reject
        public bool IsGDN { get; set; }

    }
    public class POGR
    {
        public string GrNumber { get; set; }// This is Older
        public int GrSerialNumber { get; set; } //GrSerialNumber
        public string PickerType { get; set; }
        public bool IsGDNAllow { get; set; } //IsGDNAllow
        public List<PurchaseOrderDetailDc> PurchaseOrderDetailDc { get; set; }

    }


    public class PurchaseOrderDetailDc
    {
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }
        public double MRP { get; set; }
        public int MOQ { get; set; }
        public int TotalQuantity { get; set; }
        public string PurchaseSku { get; set; }
        public string PurchaseName { get; set; }
        public int ReceivingQty { get; set; }
        public int? TotalRecivedQty { get; set; }
        public string Barcode { get; set; }
        public List<string> Barcodes { get; set; }

        public int ItemMultiMRPId { get; set; }
        public List<ItemMultiMRPDc> multiMrpIds { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string BatchNo { get; set; }
        public DateTime? MFGDate { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int ShortQty { get; set; }
        public bool IsFreeItem { get; set; }
        public int GrSerialNumber { get; set; }
        public bool IsCommodity { get; set; }
        public string PickerType { get; set; }
        //public int SupplierCreditDay { get; set; }
        public int? DamageExpiryPhysicalQty { get; set; }
        public bool IsGDNDamage { get; set; }
        public bool IsGDNExpiry { get; set; }
        public string CompanyStockCode { get; set; }
        public double? weight { get; set; } //per piece 
        public List<GRNBatchDc> GRNBatchDcs { set; get; } //for batch code implementation
        public bool IsQualityItemTesting { get; set; }
        public bool IsQualityReportUpload{ get; set; }
        public string Image { get; set; }


    }
    public class ItemBarcodeDc
    {
        public string Barcode { get; set; }
        public string ItemNumber { get; set; }
    }




    public class ItemMultiMRPDc
    {

        public int ItemMultiMRPId { get; set; }
        public string ItemNumber { get; set; }
        public double MRP { get; set; }
        public string CompanyStockCode { get; set; }

    }

    public class POListDc
    {
        public int? PurchaseOrderId { get; set; }
        public string SupplierName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }

    public class POCountDc
    {
        public int? PendingPOCount { get; set; } //'Self Approved', 'Approved', 
        public int? PartialPOCount { get; set; } //'UN Partial Received','CN Partial Received'
        public List<POListDc> POPendingList { get; set; } //Pending List
        public List<POListDc> PartialPOList { get; set; } //PartialPOList List
    }



    public class POApproverDc
    {
        public int PurchaseOrderId { get; set; }
        public int UserId { get; set; }
        public string GrNumber { get; set; }
        public int GrSerialNumber { get; set; }
        public string POApproveStatus { get; set; }
        public bool IsQualityTesting { get; set; }
        public long GrInvoiceId { get; set; }
    }
    public class AppovePurchaseOrderMasterDc
    {

        public int PurchaseOrderId { get; set; }
        public string SupplierName { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public double ETotalAmount { get; set; }
        public string PoType { get; set; }
        public string Level { get; set; }
        public string progress { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string DepoName { get; set; }
        public List<AppovePurchaseOrderDetailDc> PoItemDetail { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string GrNumber { get; set; }
        public string GrPersonName { get; set; }
        public DateTime? Gr_Date { get; set; }
        public List<PoFreeItemMasterDC> PoFreeItemDetail { get; set; }



    }
    public class AppovePurchaseOrderDetailDc
    {
        public int PurchaseOrderDetailId { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }
        public int MOQ { get; set; }
        public int TotalQuantity { get; set; }
        public int? ReceivingQty { get; set; }
        public double MRP { get; set; }
        // public int? ReceivingDamagedQty { get; set; }


    }
    public class PoFreeItemMasterDC
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        public string HSNCode { get; set; }
        public string PurchaseSku { get; set; }
        public string ItemNumber { get; set; }
        public string Itemname { get; set; }
        public double MRP { get; set; }
        public int TotalQuantity { get; set; }
        public int ItemMultiMRPId { get; set; }

    }
    public class AddPOGRDetailDc
    {
        public int UserId { get; set; }
        public string GrNumber { get; set; }
        public int GrSerialNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public bool IsGDN { get; set; }
        public List<PurchaseOrderDetailDc> PurchaseOrderDetailDc { get; set; }
        //public List<PoFreeItemMasterDC> PurchaseOrderFreeItemMasterDC { get; set; }
    }

    public class GRNBatchDc
    {
        public int Qty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public string BatchCode { get; set; }
        public long? BatchMasterId { get; set; }
        public string ItemNumber { get; set; }
        public DateTime MFGDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int BestBeforeDay { get; set; }

    }
    public class GrDraftInvoiceDc
    {
        public int PurchaseOrderId { get; set; }
        public int UserId { get; set; }
        public string GrNumber { get; set; }
        public string ImagePath { get; set; }
    }
    public class AssignmentImageupload
    {
        public int DeliveryIssuanceId { get; set; }
        public int UserId { get; set; }
        public string AssignmentUpload { get; set; }
        public string DisplayName { get; set; }
        public DateTime? CreatedDate { get; set; }

    }


    public class POCanceledDc
    {
        public int PurchaseOrderId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
    }



    public class SarthiApprovalDTO
    {

        public long Id { get; set; }
        public int OrderId { get; set; }
        public int DeliveryIssuanceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsApproved { get; set; }
        public int UserId { get; set; }
        public int Redispatchcount { get; set; }

    }


    public class ItemForeCastWarehouseDc
    {
        public int InventoryDays { get; set; }
        public int CurrentInventory { get; set; }
        public int QtyForAction { get; set; }
        public int WarehouseId { get; set; }

    }

    public class GrnApproveDc
    {
        public bool IsSuccess { get; set; }
        public List<BatchCodeSubject> OnGRNBatchCodeApproveList { get; set; }
    }

}
