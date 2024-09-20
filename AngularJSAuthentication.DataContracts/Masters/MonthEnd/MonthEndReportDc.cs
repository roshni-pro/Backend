using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.MonthEnd
{
    public class MonthEndReportDc
    {

        public class GetSalesRegisterDc
        {
            public int OrderId { get; set; }
            public string ClusterName { get; set; }
            public string OrderType { get; set; }
            public string Skcode { get; set; }
            public string StoreName { get; set; }
            public string SalesPerson { get; set; }
            public string invoice_no { get; set; }
            public double? invoiceAmount { get; set; }
            public DateTime? DispatchDate { get; set; }
            public DateTime? OrderDate { get; set; }
            public DateTime? DeliveredDate { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string RetailerName { get; set; }
            public string ShopName { get; set; }
            public int ItemId { get; set; }
            public int ItemMultiMRPId { get; set; }
            public string itemname { get; set; }
            public string CategoryName { get; set; }
            public string subcategoryName { get; set; }//sudhir 04-08-2023
            public string BrandName { get; set; }
            public string CityName { get; set; }//sudhir 04-08-2023
            public string WarehouseName { get; set; }
            public double? UnitPrice { get; set; }
            public double? PtrPrice { get; set; }
            public double? PurchasePrice { get; set; }
            public int Quantity { get; set; }
            public double? TotalAmt { get; set; }
            public int DispatchedQuantity { get; set; }
            public double? DispatchedTotalAmt { get; set; }
            public string Status { get; set; }
            public string DboyName { get; set; }
            public string DboyMobileNo { get; set; }
            public string HSNCode { get; set; }
            public string GSTno { get; set; }
            public int? AssignmentNo { get; set; }
            public double? AmtWithoutTaxDisc { get; set; }
            public double? AmtWithoutAfterTaxDisc { get; set; }
            public double? TaxPercentage { get; set; }
            public double? TaxAmmount { get; set; }
            public double? SGSTTaxPercentage { get; set; }
            public double? SGSTTaxAmmount { get; set; }
            public double? CGSTTaxPercentage { get; set; }
            public double? CGSTTaxAmmount { get; set; }
            public double? TotalCessPercentage { get; set; }
            public double? CessTaxAmount { get; set; }
            public double? IGSTTaxPercentage { get; set; }
            public double? IGSTTaxAmmount { get; set; }
            public DateTime? PocCreditNoteDate { get; set; }
            public string PocCreditNoteNumber { get; set; }
            public double? WalletAmount { get; set; }
            public double? BillDiscountAmount { get; set; }
            public string IRNNo { get; set; }
            public string comment { get; set; }
            public string comments { get; set; }
            public string DeliveryCanceledComments { get; set; }
            public double? TCSAmount { get; set; }
            public string paymentMode { get; set; }
            public string paymentThrough { get; set; }
            public string EwayBillNumber { get; set; }
            public string OfferCode { get; set; }
            public string OfferName { get; set; }
            public bool IsFreeItem { get; set; }
            public double? APP { get; set; }
        }
        public class CustomerledgersummaryDc
        {
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public string WarehouseName { get; set; }
            public string Name { get; set; }
            public double OpeningBalance { get; set; }
            public double Debit { get; set; }
            public double Credit { get; set; }
            public double Balance { get; set; }
            public int CustomerId { get; set; }
        }
        public class CMSDumpDc
        {
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
        public class DamageStockDc
        {
            public int TotalTaxPercentage { get; set; }
            public double AveragePurchasePrice { get; set; }
            public int DamageStockId { get; set; }
            public string WarehouseName { get; set; }
            public int ItemId { get; set; }
            public string ItemNumber { get; set; }
            public string ItemName { get; set; }
            public double UnitPrice { get; set; }
            public string ReasonToTransfer { get; set; }
            public int DamageInventory { get; set; }
            public DateTime? CreatedDate { get; set; }
            public int ItemMultiMRPId { get; set; }
        }
        public class FreeStockDc
        {
            public double AveragePurchasePrice { get; set; }
            public string ItemNumber { get; set; }
            public string WarehouseName { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int CurrentInventory { get; set; }
            public string itemname { get; set; }
            public double MRP { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? CreationDate { get; set; }
        }
        public class CurrentStockDc
        {
            public string WarehouseName { get; set; }
            public string ItemNumber { get; set; }
            public string ItemName { get; set; }
            public int CurrentInventory { get; set; }
            public DateTime? CreationDate { get; set; }
            public int ItemMultiMRPId { get; set; }
            public double MRP { get; set; }
            public string UnitofQuantity { get; set; }
            public string UOM { get; set; }
            public int SafetystockfQuantity { get; set; }
            public int RTPQTY { get; set; }
        }
        public class InTransitInventoryDc
        {
            public int ItemMultiMRPId { get; set; }
            public int IntransitQty { get; set; }
            public double IntransitAmount { get; set; }
            public string ItemName { get; set; }
            public string WarehouseName { get; set; }
            public string ItemNumber { get; set; }
            public string SupplierName { get; set; }
            public double AveragePurchasePrice { get; set; }
        }
        public class UnutilisedWalletPointDc
        {
            public string Skcode { get; set; }
            public string Name { get; set; }
            public string WarehouseName { get; set; }
            public double TotalAmount { get; set; }
            public  DateTime? CreatedDate { get; set; }
            public DateTime? UpdatedDate { get; set; }
        }
        public class TDSAdvancePaymentDc
        {
            public string Remark { get; set; }
            public string RefNo { get; set; }
            public int PurchaseOrderId { get; set; }
            public string SupplierName { get; set; }
            public string GSTN { get; set; }
            public DateTime? PaymentDate { get; set; }
            public double paymentAmount { get; set; }
            public double TDSAmount { get; set; }
            public string WarehouseName { get; set; }
            public double Rate_of_TDS { get; set; }
            public DateTime? entrydate { get; set; }
        }
        public class TDSReportBillToBillDc
        {
            public string Remark { get; set; }
            public string RefNo { get; set; }
            public int PurchaseOrderId { get; set; }
            public string IRID { get; set; }
            public string SupplierName { get; set; }
            public string SUPPLIERCODES { get; set; }
            public string GSTN { get; set; }
            public DateTime? PaymentDate { get; set; }
            public double paymentAmount { get; set; }
            public double TDSAmount { get; set; }
            public string WarehouseName { get; set; }
            public decimal Rate_of_TDS { get; set; }
            public DateTime? entrydate { get; set; }
        }
        public class FreebiesDataDc
        {
            public string Skcode { get; set; }
            public int OrderId { get; set; }
            public int ItemMultiMRPId { get; set; }
            public string itemname { get; set; }
            public double AveragePurchasePrice { get; set; }
            public double NetPurchasePrice { get; set; }
            public string itemNumber { get; set; }
            public string WarehouseName { get; set; }
            public int salequantity { get; set; }
            public double DispatchPrice { get; set; }
            public DateTime dispactedDate { get; set; }
            public string Status { get; set; }
            public DateTime OrderedDate { get; set; }
            public double TaxPercentage { get; set; }
            public string StoreName { get; set; }

        }
        public class FreebiesReturnDataDc
        {
            public string Skcode { get; set; }
            public int OrderId { get; set; }
            public int ItemMultiMRPId { get; set; }
            public string itemname { get; set; }
            public double AveragePurchasePrice { get; set; }
            public double NetPurchasePrice { get; set; }
            public string itemNumber { get; set; }
            public string WarehouseName { get; set; }
            public int salequantity { get; set; }
            public double DispatchPrice { get; set; }
            public DateTime dispactedDate { get; set; }
            public DateTime PocDate { get; set; }
            public string Status { get; set; }
            public DateTime OrderedDate { get; set; }
            public double TaxPercentage { get; set; }
            public string StoreName { get; set; }

        }
        public class OrderBillDiscountDataDc
        {
            public int OrderId { get; set; }
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public string Status { get; set; }
            public string invoice_no { get; set; }
            public double GrossAmount { get; set; }
            public string WarehouseName { get; set; }
            public DateTime? DispatchDate { get; set; }
            public DateTime? OrderedDate { get; set; }
            public string ClusterName { get; set; }
            public double BillDiscountAmount { get; set; }
            public string OfferCode { get; set; }
            public string OfferName { get; set; }
        }
        public class InventoryAgingDc
        {
            public string WarehouseName { get; set; }
            public int ItemMultiMrpId { get; set; }
            public double MRP { get; set; }
            public string itemBaseName { get; set; }
            public string InDate { get; set; }
            public int Ageing { get; set; }
            public int ClosingQty { get; set; }
            public double ClosingAmount { get; set; }
            public string Number { get; set; }
        }
        public class WarehouseInTransitStockDc
        {
            public int ItemMultiMRPId { get; set; }
            public int WarehouseId { get; set; }
            public int IntransitQty { get; set; }
            public double IntransitAmount { get; set; }
            public string ItemName { get; set; }
            public string WarehouseName { get; set; }
            public string ItemNumber { get; set; }
            public string SupplierName { get; set; }
            public double AveragePurchasePrice { get; set; }
        }
        public class InternalTransferDc
        {
            public int TransferOrderId { get; set; }
            public string WarehouseName { get; set; }
            public string RequestToWarehouseName { get; set; }
            public string Status { get; set; }
            public DateTime? CreationDate { get; set; }
            public string VehicleNo { get; set; }
            public string VehicleType { get; set; }
            public string EwaybillNumber { get; set; }
            public string InternalTransferNo { get; set; }
            public DateTime? ITCreatedDate { get; set; }
            public string InternalTransferNoCN { get; set; }
            public DateTime? ITCNCreatedDate { get; set; }
            public string DeliveryChallanNo { get; set; }
            public DateTime? DCCreatedDate { get; set; }
            public string ItemName { get; set; }
            public int OrderQuantity { get; set; }
            public int DispatchQty { get; set; }
            public int ItemMultiMRPId { get; set; }
            public double MRP { get; set; }
            public string UnitofQuantity { get; set; }
            public string UOM { get; set; }
            public string ItemNumber { get; set; }
            public string ItemHsn { get; set; }
            public double NPP { get; set; }
            public double TotalTaxPercentage { get; set; }
            public string ABCClassification { get; set; }
        }
        public class OrderDeliveryChargesDc
        {
            public string Skcode { get; set; }
            public string WarehouseName { get; set; }
            public int OrderId { get; set; }
            public string Status { get; set; }
            public DateTime? DeliveredDate { get; set; }
            public double GrossAmount { get; set; }
            public double deliveryCharge { get; set; }
        }
                                                       

         public class CurrentNetStockDC
        {
             public string WarehouseName { get; set; }
            public string ItemNumber { get; set; }
            public int ItemMultiMrpId { get; set; }
            public double MRP { get; set; }
            public int StockId{ get; set; }
            public string ItemName { get; set; } 
            public int NetInventory { get; set; }
            public int CurrentNetInventory { get; set; }
            public string LiveQty { get; set; }
            public int CurrentInventory { get; set; }
            public int OpenPOQTy { get; set; }
            public int CurrentDeliveryCanceledInventory { get; set; }
            public int FreestockNetInventory { get; set; }
            public double? Unitprice { get; set; }
            public double CurrentNetStockAmount { get; set; }
            public DateTime? CreatedDate { get; set; }
            public int IsActive { get; set; }
            public int AverageAging { get; set; }
            public double AgingAvgPurchasePrice { get; set; }
            public double AveragePurchasePrice { get; set; }
            public string ABCClassification { get; set; }
            public double MarginPercent { get; set; }
            public int ItemlimitQty { get; set; }
            public int ItemLimitSaleQty { get; set; }
            public int PurchaseMinOrderQty{ get; set; } 
            public string CategoryName{ get; set; }
            public string SubcategoryName { get; set; }
            public string Brand { get; set; }
            public string Store { get; set; }
            public int? RTDQty { get; set; }
            public double? RTDAmount { get; set; }
        }
        public class DirectUdharDC
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string LeadNo { get; set; }
            public string MobileNo { get; set; }
            public double AvgMonthlyBuying { get; set; }
            public int StatusOfEnhancement { get; set; }
            public DateTime DisbursalDate { get; set; }
            public double DisbursalAmount { get; set; }
            public double CumulativeLimit { get; set; }
            public string StoreLocationName { get; set; }
            public string CityName { get; set; }

        }
        public class ClearanceStockDataDC
        {
            public double? TotalTaxPercentage { get; set; }
            public double? AveragePurchasePrice{ get; set; }
            public long ClearanceStockId{ get; set; }
            public int WarehouseId{ get; set; }
            public string WarehouseName{ get; set; }
            public string ItemNumber{ get; set; }
            public string ItemName{ get; set; }
            public int Inventory{ get; set; }
            public DateTime CreatedDate{ get; set; }
            public int ItemMultiMRPId{ get; set; }
            public string Comment{ get; set; }
            
        }
        public class NonSaleableStockDC
        {
            public double? TotalTaxPercentage{ get; set; }
            public double? AveragePurchasePrice{ get; set; }
            public long NonSellableStockId{ get; set; }
            public int WarehouseId{ get; set; }
            public string WarehouseName{ get; set; }
            public string ItemNumber{ get; set; }
            public string ItemName{ get; set; }
            public int Inventory{ get; set; }
            public DateTime CreatedDate{ get; set; }
            public int ItemMultiMRPId{ get; set; }
            public string Comment{ get; set; }
        }

        public class RedispatchedWalletPointDataDC
        {
            public string Skcode{ get; set; }
            public string WarehouseName{ get; set; }
            public int? OrderId{ get; set; }
            public double? NewOutWAmount{ get; set; }
            public DateTime CreatedDate{ get; set; }
        }
        public class MonthDamageStockDc
        {
            public double TotalTaxPercentage { get; set; }
            public double AveragePurchasePrice { get; set; }
            public int DamageStockId { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public int ItemId { get; set; }
            public string ItemNumber { get; set; }
            public string ItemName { get; set; }
            public double UnitPrice { get; set; }
            public string ReasonToTransfer { get; set; }
            public int DamageInventory { get; set; }
            public DateTime? CreatedDate { get; set; }
            public int ItemMultiMRPId { get; set; }
        }
    }
}
