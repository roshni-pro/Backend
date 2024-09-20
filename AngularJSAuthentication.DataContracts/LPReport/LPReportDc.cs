using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.LPReport
{
   
        public class CycleCountReport
        {
            public int  ItemMultiMRPId { get; set; }
            public string ItemName { get; set; }
            public double MRP { get; set; }
            public int  RtdCount { get; set; }
            public int RTPQty { get; set; }

        // public int WarehouseId { get; set; }
        public string  WarehouseName { get; set; }
            public int  InventoryCount { get; set; }
            public string Comment { get; set; }
            public int CurrentInventory { get; set; }
            public DateTime? CreatedDate { get; set; }
            public int PastInventory { get; set; }

            public DateTime? UpdatedDate { get; set; }

            public string Updatedby { get; set; }

            public bool IsApproved { get; set; }

            public string ABCClassification { get; set; }
        }

           public class AssignmentClosure
             {
            public string  WarehouseName { get; set; }
        public int AssignmentID { get; set; }
        public string DboyName { get; set; }
        public double TotalAssignmentAmount { get; set;}
        public string Status { get; set; }

        public string OrderIds { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }



    }
    public class CashDepositDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
      
        public DateTime? BOD { get; set; }
        public int opening { get; set; }
        public int CashCollection { get; set; }
        public int Bank { get; set; }
        public int ExchangeIn { get; set; }
        public int ExchangeOut { get; set; }
        public int Closing { get; set; }
        public decimal onlineCollectionAmount { get; set; }


    }

    public class OrderMasterPoc
    {
        
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
        public int  orderid { get; set; }
        public string CustomerName { get; set; }
        public DateTime? Delivery_Canceled_Date { get; set; }
        public string Status { get; set; }
        public DateTime? Deliverydate { get; set; }
        public DateTime?  Poc_DATE { get; set; }


    }
    public class OrderClearanceDc
    {
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
        public int orderid { get; set; }
        public int? AssignmentId { get; set; }
        public string DboyName { get; set; }
        public string invoice_no { get; set; }
        public DateTime DispatchDate { get; set; }
        public string Status { get; set; }
        public DateTime OrderedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? Deliverydate { get; set; }

        public double GrossAmount { get; set; }

    }
    public class VirtualSettlementDc
    {
        public int WarehouseId { get; set; }//
        public string WarehouseName { get; set; }//
        public string ItemName { get; set; }//
        public string ItemNumber { get; set; }//
        public int  QTY { get; set; }//
        public string TransactionId { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Reason { get; set; }
        public double UnitPrice { get; set; }

    }

    public class LPPurchaseRegisterDataDc
    {
        public int PurchaseOrderId { get; set; }
        public string InvoiceNumbers { get; set; }

        public int ItemMultiMRPId { get; set; }


        public string WarehouseName { get; set; }
        public string BuyerName { get; set; }

        public string SupplierName { get; set; }


        public string Status { get; set; }

        public string ItemName { get; set; }
        public double PORate { get; set; } //CHANGED
        public double MRP { get; set; }

        public string SUPPLIERCODES { get; set; }
        public string GstinNumber { get; set; }
        public decimal IR1PreValue { get; set; }
        public decimal IR2PreValue { get; set; }
        public decimal IR3PreValue { get; set; }
        public decimal IR4PreValue { get; set; }
        public decimal IR5PreValue { get; set; }


        public double TotalTaxAmount { get; set; }

        public double SGSTTaxPercentage { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double IGSTTaxPercentage { get; set; }
        public double IGSTTaxAmmount { get; set; }

        public double CessTaxAmount { get; set; }
        public double CessPercentage { get; set; }

        public double IRWeightAvgPrice { get; set; }
        public string invoicedate { get; set; }
        public string GrDate1 { get; set; }
        public string GrDate2 { get; set; }
        public string GrDate3 { get; set; }
        public string GrDate4 { get; set; }
        public string GrDate5 { get; set; }
        public string IrDate1 { get; set; }//"Payment Status, Payment Date, IR Status
        public string IrDate2 { get; set; }
        public string IrDate3 { get; set; }
        public string IrDate4 { get; set; }
        public string IrDate5 { get; set; }
        public int? gr1Qty { get; set; }
        public int? gr2Qty { get; set; }
        public int? gr3Qty { get; set; }
        public int? gr4Qty { get; set; }
        public int? gr5Qty { get; set; }
        public int? ir1Qty { get; set; }
        public int? ir2Qty { get; set; }
        public int? ir3Qty { get; set; }
        public int? ir4Qty { get; set; }
        public int? ir5Qty { get; set; }
        public int? GrTotalQty { get; set; }
        public int? IRTotalQty { get; set; }
        public int? GrIrDiff { get; set; }  //comment
        public decimal IrAmount { get; set; }
        public decimal GRAmount { get; set; }
        public double TaxAmount1 { get; set; }
        public double TaxAmount2 { get; set; }

        public double TaxAmount3 { get; set; }

        public double TaxAmount4 { get; set; }

        public double TaxAmount5 { get; set; }


        public double Discount { get; set; }
        public double OtherAmount { get; set; }

        public double Expense { get; set; }
        public double Freight { get; set; }
        public double RoundOf { get; set; }
        public decimal TotalIRPrevalue { get; set; }



        public double TotalTdsAmount { get; set; }
        public decimal TdsPercentage { get; set; }

        public decimal IrAmount_Other_Expense { get; set; }
        public double TaxPercentage { get; set; }

    }
    public class LPSalesRegisterDC
    {
        public int OrderId { get; set; }
      //  public string ClusterName { get; set; }
        public string OrderType { get; set; }
        public string Skcode { get; set; }
        //public string StoreName { get; set; }
        //public string SalesPerson { get; set; }
        public string invoice_no { get; set; }

        public double invoiceAmount { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string RetailerName { get; set; }
        public string ShopName { get; set; }
        //public string Mobile { get; set; }
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string WarehouseName { get; set; }
        public double UnitPrice { get; set; }

        public int Quantity { get; set; }
        public double TotalAmt { get; set; }
        public int DispatchedQuantity { get; set; }
        public double DispatchedTotalAmt { get; set; }
        public string Status { get; set; }
        //public string DboyName { get; set; }
        public string HSNCode { get; set; }
        public string GSTno { get; set; }
        public int? AssignmentNo { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        public double AmtWithoutAfterTaxDisc { get; set; }
        public double TaxPercentage { get; set; }
        public double TaxAmmount { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double IGSTTaxPercentage { get; set; }
        public double IGSTTaxAmmount { get; set; }
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }
        public string PocCreditNoteNumber { get; set; }
        public double? WalletAmount { get; set; }
        public double? BillDiscountAmount { get; set; }
        public string IRNNo { get; set; }
       // public string comment { get; set; }
       // public string comments { get; set; }
        public string DeliveryCanceledComments { get; set; }
        public double TCSAmount { get; set; }
        public DateTime? CNDate { get; set; }
        public int? CNMonth { get; set; }

    }

    public class CreditNoteRegisterReport   //return sale register
    {
        public int OrderId { get; set; }
        //public string ClusterName { get; set; }
        public string OrderType { get; set; }
        public string Skcode { get; set; }
       // public string StoreName { get; set; }
       // public string SalesPerson { get; set; }
        public string invoice_no { get; set; }

        public double invoiceAmount { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string RetailerName { get; set; }
        public string ShopName { get; set; }
       // public string Mobile { get; set; }
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string WarehouseName { get; set; } 
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double TotalAmt { get; set; }
        public int DispatchedQuantity { get; set; }
        public double DispatchedTotalAmt { get; set; }
        public string Status { get; set; }
        //public string DboyName { get; set; }//mobile no sp
        public string HSNCode { get; set; }
        public string GSTno { get; set; }
        public int? AssignmentNo { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        public double AmtWithoutAfterTaxDisc { get; set; }
        public double TaxPercentage { get; set; }
        public double TaxAmmount { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double IGSTTaxPercentage { get; set; } //to add
        public double IGSTTaxAmmount { get; set; }//to addd
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }
        public DateTime? PocCreditNoteDate { get; set; }
        public string PocCreditNoteNumber { get; set; }
        public double? WalletAmount { get; set; }
        public double? BillDiscountAmount { get; set; }
        public string IRNNo { get; set; }
        public string comments { get; set; }
        public string DeliveryCanceledComments { get; set; }
        public double TCSAmount { get; set; }
        public string paymentMode { get; set; }
        public string paymentThrough { get; set; }
    }

    public class CMSReportData
    {
        public string WarehouseName { get; set; }
        public string currencyType { get; set; }
        public int Value { get; set; }

        public int Exchangecount { get; set; }

        public int Exchangevalue { get; set; }
        public string INOUT { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }

    }

    public class LPDebitNoteData
    {
        public int POId { get; set; }  
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public DateTime? InvoiceDate { get; set; } 
        public string InvoiceNumber { get; set; }  
        public double DebitNoteAmount { get; set; } 
        public string DebitNoteNo { get; set; }
        public DateTime CreatedDate { get; set; } 
        public int DamageQty { get; set; }   
        public int ShortQty { get; set; } 
        public int ExpiryQty { get; set; }
    }

    public class LPInternalTransferReport
    {
        public int TransferOrderId { get; set; }
        public string WarehouseName { get; set; }
        public string RequestToWarehouseName { get; set; }
        public string Status { get; set; }
        public double? RequestAmount { get; set; }
        public double? DispatchAmount { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DispatchDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string CreatedBy { get; set; }
        public string DispatchBy { get; set; }
        public string DeliveredBy { get; set; }

    }

    public class LPDamageOrderMasterReport
    {
        public int OrderId { get; set; }
        public string WarehouseName { get; set; }
        public string ShopName { get; set; }
        public int MultiMRPId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string Store { get; set; }
        public int Qty { get; set; }
        public double InvoiceAmount { get; set; }
        public double APP { get; set; }
        public DateTime? RtdDate { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? SetteledDate { get; set; }
        //public string OrderType { get; set; }
        //public string Skcode { get; set; }
        //public string invoice_no { get; set; }
       
        //public double DispatchedTotalAmt { get; set; }
      
        //public string Status { get; set; }
        //public DateTime? OrderDate { get; set; }
       
      
    

    }


    public class LPPRReport
    {
        public int PrPoNo { get; set; }
        public string PRDate { get; set; }
        public string SupplierName { get; set; }
        public string SupplierCode { get; set; }
        public string WarehouseName { get; set; }
        public int PRAmount { get; set; }
        public int PRPaidAmount { get; set; }
        public string PaymentDate { get; set; }
        public string IfGRDone { get; set; }
        public double BalanceAmount { get; set; }
        public string Remarks { get; set; }
        public double GRAmount { get; set; }

        public double RemainingGRAmt { get; set; }
        public double IRAmount { get; set; }
        public double BankAmount { get; set; }
        public double TransferredAmount { get; set; }
        public double SettledAmount { get; set; }

    }

    public class LPPOMasterReport
    {
        public int PurchaseOrderId { get; set; }
        public string WarehouseName { get; set; }
        public string SupplierName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string Store { get; set; }
        public int PoQty { get; set; }
        public int GrQty { get; set; }
        public int? IrQty { get; set; }
        //public int DamageQty { get; set; }
        //public int ExpiryQty { get; set; }
        public double ItemMrp { get; set; }
         public double PoPrice { get; set; }
        public double? IrPrice { get; set; }
        public string Status { get; set; }
        //public string GRstatus { get; set; }
        //public string GRNo { get; set; }
        public DateTime? PoDate { get; set; }
        public DateTime? GrDate { get; set; }
        //public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? LastIRDate { get; set; }
        public DateTime? PoCloseDate { get; set; }
        public double PoAmount { get; set; }
        public double GrAmount { get; set; }
        public string IrAmount { get; set; }
        public string IrApprovedBy { get; set; }
        public string IrCreatedBy { get; set; }
        public string PoCreatedBy { get; set; }
    }


    public class LPVirtualUnsettledReport
    {
        public long Id { get; set; }
        public string TransactionId { get; set; }
        public int ItemMultiMRPID { get; set; }
        //public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int Qty { get; set; }
        public double APP { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Reason { get; set; }
        public string CreatedBy { get; set; }
       


    }

    public class LPStockTransferReport
    {
        public string WarehouseName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public int? ManualInventoryInOut{ get; set; }
        public double MRP { get; set; }
        public string ManualReason { get; set; }
        public DateTime CreationDate { get; set; }
        public string UserName { get; set; }
        public double APP { get; set; }
    }

    public class LPEmployeeDetailsReport
    {
        public string WarehouseName { get; set; }
        public string City { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string MobileNumber { get; set; }
        //public bool ActiveInactive { get; set; }
        //public string Desgination { get; set; }
        public string Status { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime? ExitDate { get; set; }
    }
    public class SpType
        {
            public List<int> warehouseIds { get; set; }
         //public DateTime fromdate { get; set; }
        public int month { get; set; }
        public int year { get; set; }
         //public DateTime todate { get; set; }
        public int sptype { get; set; }


    }



}
