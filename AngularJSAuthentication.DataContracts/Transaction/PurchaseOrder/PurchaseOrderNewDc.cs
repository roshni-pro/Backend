using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.GDN;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.PurchaseOrder
{
    public class PurchaseOrderNewDc
    {
        public string PoInvoiceNo { get; set; }
        public string TransactionNumber { get; set; }
        public int? CompanyId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public double? discount { get; set; }
        public double TotalAmount { get; set; }
        public double Advance_Amt { get; set; }
        public string PoType { get; set; }
        public string Comment { get; set; }
        public int? ApprovalBy { get; set; }
        public string Level { get; set; }
        public string progress { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string SupplierRejectReason { get; set; }
        public double ETotalAmount { get; set; }
        // create by Anushka
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public bool IsCashPurchase { get; set; }
        public string CashPurchaseName { get; set; }
        public int SupplierStatus { get; set; } //  For Suplier  0.NA  1.PO Accepted , 2.PO Rejected, 3.PO Canceled 4. PO Ongoing Orders 5.Po Past Orders
        public List<PurchaseOrderDetailNewDc> PurchaseOrderDetail { get; set; }
        public int PurchaseOrderId { get; set; }
        public long Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public int? Approval1 { get; set; }

        public int? Approval2 { get; set; }

        public int? Approval3 { get; set; }

        public int? Approval4 { get; set; }

        public int? Approval5 { get; set; }

        public string ApprovalName1 { get; set; }

        public string ApprovalName2 { get; set; }

        public string ApprovalName3 { get; set; }

        public string ApprovalName4 { get; set; }

        public string ApprovalName5 { get; set; }
        public int PRStatus { get; set; }
        public string ApprovedBy { get; set; }
        public bool IsPR { get; set; }
        public string PickerType { get; set; }
        public int SupplierCreditDay { get; set; }
        public string PRPaymentType { get; set; }
        public bool IsAdjustmentPo { get; set; }
        public string isGDN { get; set; }
        public double? FreightCharge { get; set; }

        public bool IsGrnDone { get; set; }
        public double? Totalweight { get; set; }
      


    }


    public class PurchaseOrderDetailNewDc
    {
        public int PurchaseOrderDetailId { get; set; }
        public int? CompanyId { get; set; }
        public int PurchaseOrderId { get; set; }

        public long? PurchaseOrderNewId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public string SupplierName { get; set; }
        public string SellingSku { get; set; }
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string HSNCode { get; set; }
        public string SKUCode { get; set; }
        public string ItemName { get; set; }
        public string itemBaseName { get; set; }
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
        public int QtyRecived { get; set; }

        //multimrp
        public int ItemMultiMRPId { get; set; }

        // create by Anushka
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public string Barcode { get; set; }
        public List<ItemMultiMRPDc> multiMrpIds { get; set; }
        public bool IsCommodity { get; set; }//just for comodity item not show mrp and multi mrp in drop down

        //public PurchaseOrderNewDc PurchaseOrder { get; set; }

        //public GoodsReceivedDetail GoodsReceivedDetail { get; set; }
        public bool IsFreeItem { get; set; }
        public string Category { get; set; }

        public string CompanyStockCode { get; set; }
        public int DemandQty { get; set; }
        public int OpenPOQTy { get; set; }
        public double Weight { get; set; }
        public string WeightType { get; set; }
        public double? WeightInGram { get; set; }
        public double? TotalItemWeight { get; set; }
        public int Tag { get; set; } //new Add for ROC 

    }



    public class GoodsReceivedDetailDc
    {
        public long Id { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int ShortQty { get; set; }
        public int GrSerialNumber { get; set; } //gr serial number
        public double Price { get; set; }
        public int Status { get; set; } // 1= Pending for Checker Side, 2=Approved , 3=Reject
        public int CurrentStockHistoryId { get; set; }
        public string BatchNo { get; set; }
        public DateTime? MFGDate { get; set; }
        public string Barcode { get; set; }
        public int ApprovedBy { get; set; }//approved by or rejectby
        public string VehicleType { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsFreeItem { get; set; }
        public string Comment { get; set; }
        public int PurchaseOrderId { get; set; }

        public bool? IsDamageExpiryPhysical { get; set; }
        public double TotalAmount { get; set; }
        public bool IsGDN { get; set; }
        public double? weight { get; set; } //per piece 
    }

    public class GoodsReceivedDc
    {
        public int GrSerialNumber { get; set; } //gr serial number
        public string GrSerialNumberWithPO { get; set; } //gr serial number with PoId
        public int Status { get; set; } // 1= Pending for Checker Side, 2=Approved , 3=Reject

        public string strStatus
        {
            get
            {
                string values = "";
                if (Status == 1)
                    values = "Pending for Checker Side";
                else if (Status == 2)
                    values = "Approved";
                else if (Status == 3)
                    values = "Reject";
                else
                    values = "Inprogress";

                return values;
            }

        }
        public int ApprovedBy { get; set; }//approved by or rejectby
        public string ApproverName { get; set; }//approved by or rejectby
        public string VehicleType { get; set; }
        public string VehicleNumber { get; set; }
        public double GRAmount { get; set; }
        public DateTime CreatedDate { get; set; }//GrDate
        public DateTime? ModifiedDate { get; set; }
        public int CreatedBy { get; set; }//GrPersonName
        public string GrPersonName { get; set; }//approved by or rejectby
        public int? ModifiedBy { get; set; }
        public List<GoodsReceivedItemDc> GoodsReceivedItemDcs { get; set; }

        public bool IsGDN { get; set; }
    }
    public class GoodsReceivedItemDc
    {
        public long Id { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public int PurchaseOrderId { get; set; }
        public string ItemName { get; set; }
        public string Itemnumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public int TotalQuantity { get; set; } //Total qty
        public int Qty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }

        public int ShortQty { get; set; }
        public bool IsFreeItem { get; set; }
        public double Price { get; set; }
        public int CurrentStockHistoryId { get; set; }
        public string BatchNo { get; set; }
        public DateTime? MFGDate { get; set; }
        public string Barcode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Comment { get; set; }
        public string HSNCode { get; set; }

        public int GrSerialNumber { get; set; }
        public int? MOQ { get; set; }
        public string Category { get; set; }
        public bool? IsDamageExpiryPhysical { get; set; }
        public double TotalAmount { get; set; }
        public string CompanyStockCode { get; set; }
        public double? weight { get; set; } //per piece 
        public List<BatchMasterDC> BatchMasterDC { get; set; }
        public string QualityImage { get; set; }
        public string QualityComment { get; set; }
        public long GrQualityInvoiceId { get; set; }
        public List<GrQualityList> GrQualityInvoiceList { get; set; }
        public bool IsShowQaReport { get; set; }
        public string QaName{ get; set; }
        public string QaPhoneNo{ get; set; }
        public string QaStatus{ get; set; }
        public int QaCheckerId { get; set; }

    }

    public class GrQualityList
    {
        public long GoodsReceivedDetailId { get; set; }
        public string Image { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public string DisplayName { get; set; }
        public string QaPhoneNo { get; set; }
        public string QaName { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
    public class IRItemDc
    {
        public long IRItemId { get; set; }
        public long GoodsReceivedDetailId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public string ItemName { get; set; }
        public string Itemnumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int TotalQuantity { get; set; } //Total qty
        public int Qty { get; set; } //GR Qty
        public int IRQuantity { get; set; }   //IRQuantity

        public int CNShortQty { get; set; }  // Debit Note Field
        public int CNDamageQty { get; set; }// Debit Note Field
        public int CNExpiryQty { get; set; }// Debit Note Field

        public string Comment { get; set; } //Short Qty
        public int IRRemainingQuantity { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public double Price { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double TotalCessPercentage { get; set; }
        public string HSNCode { get; set; }
        public string distype { get; set; }
        public double? DesP { get; set; }
        public double? DesA { get; set; }
        public double CashDiscount { get; set; }
        public double TaxableAmount { get; set; }
        public double GSTAmount { get; set; }
        public double SGSTAmount { get; set; }
        public double CGSTAmount { get; set; }
        public double CESSAmount { get; set; }
        public double TotalAmount { get; set; }
        public double MRP { get; set; }
        public int Status { get; set; }
        public bool IsFreeItem { get; set; }

        public int TotalPoQuantity { get; set; }

        public int TotalGRQuantity { get; set; }

        public int FinalIrQuantity { get; set; }
        public bool ShowShortQty { get; set; }

        public int ShortQuantity { get; set; }

        public bool IsVisible { get; set; }

        public DamageExpiryDC DamageExpiryDC { get; set; }
        public string Category { get; set; }

        public bool? ISDamageExpiryPhysical { get; set; }

        public int DamageQtyDone { get; set; }
        public int ExpiryQtyDone { get; set; }

        public int UnitGrQty { get; set; }
        public int UnitDamageQty { get; set; }
        public int UnitExpiryGrQty { get; set; }
        public long FinalGoodRecievedDetailId { get; set; }
        public string CompanyStockCode { get; set; }
        //public bool IsDamageQtyDNote { get; set; }
        //public bool IsExpiryQtyDNote { get; set; }        
        public List<GDNItemforIRDc> GoodsDescripancyNoteDetails { set; get; }
        public List<long> GoodsDescripancyNoteDetailIds { get; set; }

    }

    public class DamageExpiryDC
    {
        public int DamageRemainingQty { get; set; }
        public int ExpiryRemainingQty { get; set; }

        public int ItemMultiMrpId { get; set; }
        public bool IsFreeItem { get; set; }
    }
    public class IRMasterDc
    {
        public int Id { get; set; }
        public long PurchaseOrderId { get; set; }
        public string distype { get; set; }
        public double? DesP { get; set; }
        public double? DesA { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public int? DueDays { get; set; }
        public double? FreightAmount { get; set; }
        public double BillAmount { get; set; }
        public int IRSerialNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public bool IsIgstIR { get; set; }
        public string IRStatus { get; set; }
        public List<IRItemDc> IRItemDcs { get; set; }
        public List<IRCreditNoteMasterDc> IRCreditNoteMasterDcs { get; set; }
        public int TotalPOQuantityMst { get; set; }

        public int TotalGRQuantityMst { get; set; }

        public string IRNNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }

    }



    public class GetIRMasterDc
    {

        public long Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public string IRID { get; set; }
        public int supplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public double TotalAmount { get; set; }
        public string IRStatus { get; set; }
        public double? Gstamt { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? Discount { get; set; }
        public double IRAmountWithTax { get; set; }
        public double IRAmountWithOutTax { get; set; }
        public double TotalAmountRemaining { get; set; }
        public string PaymentStatus { get; set; }
        public int PaymentTerms { get; set; }
        public string IRType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Deleted { get; set; }
        public int? Progres { get; set; }
        public string Remark { get; set; }
        public string RejectedComment { get; set; }
        public string ApprovedComment { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? OtherAmount { get; set; }
        public string OtherAmountRemark { get; set; }
        public double? ExpenseAmount { get; set; }
        public string ExpenseAmountRemark { get; set; }
        public double? RoundofAmount { get; set; }
        public string ExpenseAmountType { get; set; }
        public string OtherAmountType { get; set; }
        public string RoundoffAmountType { get; set; }
        public string InvoiceNumber { get; set; }
        public int? DueDays { get; set; }
        public double? CashDiscount { get; set; }
        public double? FreightAmount { get; set; }
        public int IrSerialNumber { get; set; }
        public bool IsDraft { get; set; }
        public bool IsIrExtendInvoiceDate { get; set; }
        public List<IRItemDc> IRItemDcs { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public List<IRCreditNoteMasterDc> IRCreditNoteMasterDcs { get; set; }

        public List<IRCreditNoteDetailDc> IRCreditNoteDetailDc { get; set; }

        public bool IsShortDebitNoteGenerate { get; set; }
        public bool IsExcessDebitNoteGenerate { get; set; }
        public bool IsDamageDebitNoteGenerate { get; set; }
        public string IRNNumber { get; set; }
        public double TotalTDSAmount { get; set; }
        public string wrongGRComment { get; set; }
        public string CNNumber { get; set; }
        public bool IsIgstIR { get; set; }
        public double? CreditNoteAmount { get; set; }
        public DateTime? CreditNoteDate { get; set; }
    }

    public class InvoiceImageDc
    {
        public int Id { get; set; }
        public int IRMasterId { get; set; }
        public string InvoiceNumber { get; set; }
        public int PurchaseOrderId { get; set; }
        public int WarehouseId { get; set; }
        public double IRAmount { get; set; }
        public string IRLogoURL { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Remark { get; set; }

    }
    public class GDNItemforIRDc
    {
        public long Id { get; set; }
        public int GrSerialNo { get; set; }
        public long GoodsDescripancyNoteMasterId { get; set; }
        public long GoodsReceivedDetailId { get; set; }
        public int ShortQty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool check { get; set; }
        public string Status { get; set; }
        public string GDNNumber { get; set; }

    }

    public class ItemWithMRPDc
    {
        public string Number { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double TotalCessPercentage { get; set; }
        public string HSNCode { get; set; }
        public double MRP { get; set; }
        public int ItemMultiMRPId { get; set; }

    }
    public class IRCreditNoteMasterDc
    {
        public long Id { get; set; }
        public int IRMasterId { get; set; }
        public int CNForId { get; set; }
        public string CNForName { get; set; }
        public string CNNumber { get; set; }
        public string CNGenerateFile { get; set; }
        public string Comment { get; set; }
        public List<IRCreditNoteDetailDc> IRCreditNoteDetails { get; set; }
        public string Message { get; set; }

        public string SupplierAddress { get; set; }
        public string SupplierName { get; set; }
        public string SupplierGstNo { get; set; }
        public string SupplierState { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyGstInNo { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceCreatedDate { get; set; }

        public int TotalShortQty { get; set; }
        public int TotalDamageQty { get; set; }
        public int TotalExpiryQty { get; set; }

        public int? TotalQty { get; set; }

        public double? TotalAmountAfterTax { get; set; }

        public double? TotalTaxablevalue { get; set; }

        public double? TotalSgstAmount { get; set; }
        public double? TotalCgstAmount { get; set; }

        public double? TotalIgstAmount { get; set; }

        public double? TotalCessAmount { get; set; }

        public double? TotalAmountBeforeTax { get; set; }

        public double? TotalDiscount { get; set; }

        public bool IsDebitNoteGenerate { get; set; }
        public string CreditNoteNumber { get; set; }
        public double? CreditNoteAmount { get; set; }
        public DateTime? CreditNoteDate { get; set; }  
        


    }
    public class IRCreditNoteDetailDc
    {
        public long Id { get; set; }
        public long IRCreditNoteMasterId { get; set; }
        public string ItemName { get; set; }
        public int ShortQty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public double IRPrice { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? CessTaxPercentage { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }

        public string HSNCode { get; set; }

        public string Uom { get; set; }

        public int? Qty { get; set; }

        public double? Amount { get; set; }

        public double? Taxablevalue { get; set; }

        public double? SgstAmount { get; set; }

        public double? CessRate { get; set; }

        public double? CgstAmount { get; set; }

        public double? IgstAmount { get; set; }
        public double? IgstRate { get; set; }
        public double? GstRate { get; set; }

        public double? TotalAmount { get; set; }

        public double? ShortQuantityDiscount { get; set; }

        public double? CessAmount { get; set; }
    }
    public class PurchaseRequestNewDC
    {
        public string PoInvoiceNo { get; set; }
        public string TransactionNumber { get; set; }
        public int? CompanyId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public double? discount { get; set; }
        public double TotalAmount { get; set; }
        public double Advance_Amt { get; set; }
        public string PoType { get; set; }
        public string Comment { get; set; }
        public int? ApprovalBy { get; set; }
        public string Level { get; set; }
        public string progress { get; set; }
        public int? BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string SupplierRejectReason { get; set; }
        public double ETotalAmount { get; set; }
        // create by Anushka
        public int? DepoId { get; set; }
        public string DepoName { get; set; }
        public bool IsCashPurchase { get; set; }
        public string CashPurchaseName { get; set; }
        public int SupplierStatus { get; set; } //  For Suplier  0.NA  1.PO Accepted , 2.PO Rejected, 3.PO Canceled 4. PO Ongoing Orders 5.Po Past Orders
        public List<PurchaseOrderRequestDetailDC> PurchaseOrderRequestDetail { get; set; }
        public int PurchaseOrderId { get; set; }
        public long Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

        public int? Approval1 { get; set; }

        public int? Approval2 { get; set; }

        public int? Approval3 { get; set; }

        public int? Approval4 { get; set; }

        public int? Approval5 { get; set; }

        public string ApprovalName1 { get; set; }

        public string ApprovalName2 { get; set; }

        public string ApprovalName3 { get; set; }

        public string ApprovalName4 { get; set; }

        public string ApprovalName5 { get; set; }
        public int PRStatus { get; set; }
        public bool IsPR { get; set; }
        public bool IsAdjustmentPo { get; set; }


    }
    public class PurchaseOrderRequestDetailDC
    {
        public int PurchaseOrderRequestDetailId { get; set; }
        public int? CompanyId { get; set; }
        public int PRID { get; set; }

        public long? PurchaseOrderNewId { get; set; }
        public int SupplierId { get; set; }
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public string SupplierName { get; set; }
        public string SellingSku { get; set; }
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string HSNCode { get; set; }
        public string SKUCode { get; set; }
        public string ItemName { get; set; }
        public string itemBaseName { get; set; }
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
        [NotMapped]
        public int QtyRecived { get; set; }

        //multimrp
        public int ItemMultiMRPId { get; set; }

        // create by Anushka
        public int? DepoId { get; set; }
        public string DepoName { get; set; }

        //public GoodsReceivedDetail GoodsReceivedDetail { get; set; }
        public bool IsDeleted { get; set; }
        public int PurchaseOrderId { get; set; }


        public int PurchaseOrderDetailId { get; set; }
        public string Barcode { get; set; }
        public List<ItemMultiMRPDc> multiMrpIds { get; set; }
        public bool IsCommodity { get; set; }//just for comodity item not show mrp and multi mrp in drop down

        //public PurchaseOrderNewDc PurchaseOrder { get; set; }

        //public GoodsReceivedDetail GoodsReceivedDetail { get; set; }
        public bool IsFreeItem { get; set; }
        public string Category { get; set; }

    }
    public class CreditNoteInvoiceDC
    {
        public IRCreditNoteMasterDc IRCreditNoteMasterDc { get; set; }
        public List<IRCreditNoteDetailDc> IRCreditNoteDetailDc { get; set; }

        public int TotalShortQty { get; set; }
        public int TotalDamageQty { get; set; }
        public int TotalExpiryQty { get; set; }

        public int? TotalQty { get; set; }

        public double? TotalAmountAfterTax { get; set; }

        public double? TotalTaxablevalue { get; set; }

        public double? TotalSgstAmount { get; set; }
        public double? TotalCgstAmount { get; set; }

        public double? TotalIgstAmount { get; set; }

        public double? TotalAmountBeforeTax { get; set; }

        public double? TotalDiscount { get; set; }

        public double? TotalCessAmount { get; set; }
        public int Poid { get; set; }
    }


    public class GoodsDescripancyNoteMasterDC
    {

        public string GDNNumber { get; set; }
        public long PurchaseOrderId { get; set; }
        public string Status { get; set; } // Pending , Approved, Reject
        public bool IsGDNGenerate { get; set; }
        public int? OTP { get; set; }
        public List<GoodsDescripancyNoteDetailDC> goodsDescripancyNoteDetail { get; set; }
    }
    public class GoodsDescripancyNoteDetailDC
    {
        public long Id { get; set; }
        public long GoodsDescripancyNoteMasterId { get; set; }
        public long GoodsReceivedDetailId { get; set; }
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int ShortQty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public int? ChangedShortQty { get; set; }
        public int? ChangedDamageQty { get; set; }
        public int? ChangedExpiryQty { get; set; }
        public int PurchaseOrderId { get; set; }
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }

    }

    public class BatchMasterDC
    {
        public long BatchMasterId { get; set; }
        public string BatchCode { get; set; }
        public int Qty { get; set; }
        public int DamageQty { get; set; }
        public int ExpiryQty { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ItemNumber { get; set; }
        public long GoodRecievedDetailId { get; set; }

    }

    public class GetIRDetailViaIcApproveDC
    {
        public int PurchaseOrderId { get; set; }
        public int IRMasterID { get; set; }
        public string IRID { get; set; }
        public string IRType { get; set; }
        public string IRStatus { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string BuyerName { get; set; }
        public string CreatedBy { get; set; }
        public string InvoiceNumber { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double TotalTDSAmount { get; set; }
    }
}
