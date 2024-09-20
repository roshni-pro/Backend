using AngularJSAuthentication.Model.SalesApp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AngularJSAuthentication.Model
{
    public class OrderDispatchedDetailDc
    {
        public int OrderDetailsId { get; set; }
        public string ItemName { get; set; }
        public string Itempic { get; set; }
        public double price { get; set; }
        public int Qty { get; set; }
        public int ItemId { get; set; }
        public string SubcategoryName { get; set; }
        public bool IsReplaceable { get; set; }
        public bool IsReturnReplaced { get; set; }
    }

    public class PostKKReturnReplaceRequestDc
    {
        public int CustomerId { get; set; }
        public int ExecutiveId { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public int RequestType { get; set; } //Return =0/ Replace =1
        public string Cust_Comment { get; set; }
        public string OTP { get; set; }
        public virtual ICollection<KKReturnReplaceDetailDC> Details { get; set; }
    }
    public class KKReturnReplaceDetailDC
    {
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public int ReturnQty { get; set; }
        public int Qty { get; set; }
        public string ReturnReason { get; set; }
        public string BatchCode { get; set; }
        public long BatchMasterId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public List<string> ItemImages { get; set; }
    }

    public class GetReturnReplaceOrderDC
    {
        public int OrderId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Status { get; set; }
        public int RequestType { get; set; }
        public int KKRequstId { get; set; }
    }

    public class GetReturnReplaceHistoryDC
    {
        public int KKRequestId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }

    public class GetReturnReplaceItemDc
    {
        public int KKReturnDetailId { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public string ItemName { get; set; }
        public string Itempic { get; set; }
        public double price { get; set; }
        public int Qty { get; set; }
        public int RequestType { get; set; }
        public int ItemId { get; set; }
        public string ReturnReason { get; set; }
        public int Status { get; set; }
        public string Warehouse_Comment { get; set; }
        public string Buyer_Comment { get; set; }
        public string BuyerNames { get; set; }
        public string Name { get; set; }
        public string ShopName { get; set; }
        public string BatchCode { get; set; }
        public string Mobile { get; set; }
        public string ShippingAddress { get; set; }
        public string ReturnImage { get; set; }
        public DateTime CreatedDate { get; set; }
        public long KKRequestId { get; set; }
        public int ItemMultiMRPId { get; set; }
    }

    public class GetReturnDetailHistoryDc
    {
        public long KKRequestId { get; set; }
        public int KKReturnDetailId { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        //public string HQ_Comment { get; set; }
        public string Comment { get; set; }
        /*public string CustomerName { get; set; }
        public string UserName { get; set; }*/

    }


    public class GetReturnReplaceOrderForDBoyDC
    {
        public int OrderId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Status { get; set; }
        public int RequestType { get; set; }
        public int KKRequstId { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public string ShippingAddress { get; set; }
        public string Skcode { get; set; }
        public string Name { get; set; }
    }
    public class KKReturnReplaceDashboardDC
    {
        public int KkRequestId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public int ReturnStatus { get; set; }
        public int RequestType { get; set; } //Return =0/ Replace =1
        public string Cust_Comment { get; set; }
        public string Warehouse_Comment { get; set; }
        public string HQ_Comment { get; set; }
        public int Picker_PersonId { get; set; }//
        public int Receiver_PersonId { get; set; }
        public int Settled_PersonId { get; set; }
        public int DBoyId { get; set; }
        public string ReturnImage { get; set; }
        public double ManualWalletPoint { get; set; }
        public string Skcode { get; set; }
    }
    public class TripPickerIdDc
    {
        public pickerIdDc PickerId { get; set; }
        public TripIdDc TripId { get; set; }
    }
    public class pickerIdDc
    {
        public int OrderId { get; set; }

        public long OrderPickerMasterId { get; set; }
    }
    public class TripIdDc
    {
        public long OrderId { get; set; }

        public long TripPlannerMasterId { get; set; }
    }
    public class OrderDispatchedDetailsDC
    {
        public int OrderDispatchedDetailsId { get; set; }
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public string QtyChangeReason { get; set; }

        public int OrderDispatchedMasterId { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public String City { get; set; }
        public string Mobile { get; set; }
        public DateTime OrderDate { get; set; }
        public int CompanyId { get; set; }
        public int? CityId { get; set; }
        public double SizePerUnit { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string CategoryName { get; set; }
        public bool isDeleted { get; set; }
        public int ItemId { get; set; }
        public string SellingSku { get; set; }
        public string Itempic { get; set; }
        public string itemname { get; set; }
        public string SellingUnitName { get; set; }
        public string itemcode { get; set; }
        public string Barcode { get; set; }

        public double price { get; set; }
        public double UnitPrice { get; set; }
        public double Purchaseprice { get; set; }
        public double OnlineServiceTax { get; set; }

        public int MinOrderQty { get; set; }
        public double MinOrderQtyPrice { get; set; }
        public int qty { get; set; }

        // for new calculation
        public int Noqty { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        public double AmtWithoutAfterTaxDisc { get; set; }
        public double TotalAmountAfterTaxDisc { get; set; }

        public double NetAmmount { get; set; }
        public double DiscountPercentage { get; set; }
        public double DiscountAmmount { get; set; }
        public double NetAmtAfterDis { get; set; }
        public double TaxPercentage { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public double TaxAmmount { get; set; }

        //for cess
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }

        public double TotalAmt { get; set; }

        public int UnitId { get; set; }
        public string Unitname { get; set; }
        public string itemNumber { get; set; }
        public string HSNCode { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
        //public string Status { get; set; }

        public int ItemMultiMRPId { get; set; }
        public virtual ICollection<ItemMaster> itemMaster { get; set; }
        public OrderItemHistory OrderItemHistory { get; set; }
        public CurrentStockHistory CurrentStockHistory { get; set; }
        public CurrentStock StockToUpdate { get; set; }
        public FreeStockHistory FreeCurrentStockHistory { get; set; }// add by raj
        public FreeStock FreeStockToUpdate { get; set; }//add by raj
        public OrderDetails OrderDetail { get; set; }
        public bool IsFreeItem { get; set; }
        public bool IsDispatchedFreeStock { get; set; }
        public string Category { get; set; }
        public string StoreName { get; set; }
        public int SubCategoryId { get; set; }
        public bool PrepareSeparateAssignment { get; set; }
    }
    public class GetCategoryListDc
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
    }
    public class SalesReturnFilterDC
    {
        public long Id { get; set; }
        public int BrandId { get; set; }
        public List<int> CategoryId { get; set; }
        public double QtyPercent { get; set; }
        public bool IsPreExpiry { get; set; }
        public int DayBeforeExpiry { get; set; }
        public bool IsPostExpiry { get; set; }
        public int DayAfterExpiry { get; set; }
        public int DurationDeliveryDate { get; set; }
        public int DurationOrderDate { get; set; }
    }
    public class GetSalesReturnDC
    {
        public long Id { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public double QtyPercent { get; set; }
        public bool IsPreExpiry { get; set; }
        public int DayBeforeExpiry { get; set; }
        public bool IsPostExpiry { get; set; }
        public int DayAfterExpiry { get; set; }
        public long totalRecords { get; set; }
        public int DurationDeliveryDate { get; set; }
        public int DurationOrderDate { get; set; }
    }
    public class GetSalesReturnExportDC
    {
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public double QtyPercent { get; set; }
        public bool IsPreExpiry { get; set; }
        public int DayBeforeExpiry { get; set; }
        public bool IsPostExpiry { get; set; }
        public int DayAfterExpiry { get; set; }
        public int DurationOrderDate { get; set; }
        public int DurationDeliveryDate { get; set; }
    }
    public class SalesFilterDC
    {
        public int skip { get; set; }
        public int take { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public string KeyValue { get; set; }
    }

    public class ItemListdc
    {
        public string ItemName { get; set; }
        public int CustomerId { get; set; }
        public int ItemMultiMRPId { get; set; }
    }
    public class RetunOrderBatchItemDc
    {
        public int OrderId { get; set; }
        public int Qty { get; set; }
        public int OrderDetailsId { get; set; }
        public long BatchMasterId { get; set; }
        public double UnitPrice { get; set; }
        public int ReturnableQty { get; set; }
        public string BatchCode { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool IsFreeItem { get; set; }
    }

    public class SalesReturnDc
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int OrderDetailsId { get; set; }
        public string Status { get; set; }
        public int ReturnQty { get; set; }
        public string BatchCode { get; set; }
        public string ReturnReplaceImage { get; set; }
        public string Cust_Comment { get; set; }
        public List<string> ItemImages { get; set; }
    }
    public class SalesReturnListDC
    {
        public long RequestId { get; set; }
        public int OrderId { get; set; }
        public double OrderValue { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
    public class ReturnDetailListDC
    {
        public string ItemName { get; set; }
        public int Qty { get; set; }
        public double Rate { get; set; }
        public double TotalValue { get; set; }
    }
    public class ApproveRejectDC
    {
        public int KKReturnDetailId { get; set; }
        public string Comment { get; set; }
        public bool Status { get; set; }
        public bool IsWarehoues { get; set; }
    }
    public class MultiReturnOrderApproveRejectDC
    {
        public int KKReturnDetailId { get; set; }
        public string Comment { get; set; }
        public bool Status { get; set; }
        //public bool IsWarehoues { get; set; }
    }
    public class SalesReturnDetailDC
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        //public long KKRRRequestId { get; set; }
        public List<int> OrderDetailsId { get; set; }
    }
    public class ReturnDetailDC
    {
        public List<KKReturnReplaceDetail> ReturnDetails { get; set; }
        public List<OrderDispatchedDetails> OrderDetails { get; set; }
    }

    public class ItemPickByDBoyDc
    {
        public long TripPlannerConfirmedMasterId { get; set; }
        public List<OrderDetaileList> OrderDetaileList { get; set; }
        public string OTP { get; set; }
        public int PeopleId { get; set; }
        public double? DeliveryLat { get; set; }
        public double? DeliveryLng { get; set; } 
    }
    public class OrderDetaileList
    {
        public int NewOrderId { get; set; }
        public int NewOrderDetailId { get; set; }
        public int Qty { get; set; }
        public double unitprice { get; set; }
        public string PickItemImage { get; set; }
        public string BatchCode { get; set; }
        public int BatchId { get; set; }
        public long KKRRRequestId { get; set; }
    }
    public class BillDiscountCalculationDC
    {
        public int OrderId { get; set; }
        public int Status { get; set; }
        public int Skcode { get; set; }
        public int WarehouseName { get; set; }
        public int CategoryName { get; set; }
        public int SubcategoryName { get; set; }
        public int SubsubcategoryName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int itemNumber { get; set; }
        public int itemname { get; set; }
        public int OrderQty { get; set; }
        public int dispatchQty { get; set; }
        public int UnitPrice { get; set; }
        public int WalletAmount { get; set; }
        public int billdiscountAmount { get; set; }
    }
    public class ReturnOrderIdPostDc
    {
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public int qty { get; set; }
        public double unitprice { get; set; }
        public string PickItemImage { get; set; }
    }
    public class OrderBillDiscountDc
    {
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int OrderQty { get; set; }
        public int dispatchQty { get; set; }
        public double UnitPrice { get; set; }
        public double billdiscountAmount { get; set; }
        public double UnitbilldiscountAmt { get; set; }
        public double WalletAmount { get; set; }
        public double UnitWalletAmount { get; set; }
        public string Status { get; set; }
        public string Skcode { get; set; }
        public string itemNumber { get; set; }
        public string itemname { get; set; }
        public int NewGeneratedOrderId { get; set; }
        public int NewOrderDetailsId { get; set; }
    }
    public class SalesReturnOTPDc
    {
        public List<int> OrderId { get; set; }
        public string Status { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
    }
    public class ConfirmReturnOrderOTPRes
    {
        //OrderId TotalGrossAmount    TotalQty Mobile
        public string OrderId { get; set; }
        public int TotalQty { get; set; }
        public double TotalGrossAmount { get; set; }
        public string Mobile { get; set; }
    }
    public class SalesReturnDashboardDC
    {
        public int OrderId { get; set; }
        public string RequestIds { get; set; }
        public string  Skcode { get; set; }
        public double RefundAmount { get; set; }
        public string CreditNoteNumber { get; set; }
    }
    public class SalesReturnDashboardDetailDC
    {
        public long RequestId { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        //public string Buyer_Comment { get; set; }
        public string UserName { get; set; }
        //public string BuyerByName { get; set; }
        //public string CustomerName { get; set; }

    }
    public class SalesReturnExportDC
    {
        public string WarehouseName { get; set; }
        public string itemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double MRP { get; set; }
        public string itemname { get; set; }
        public int Qty { get; set; }
        public double UnitPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public double AveragePurchasePrice { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string StoreName { get; set; }
        public string Status { get; set; }
    }
}
