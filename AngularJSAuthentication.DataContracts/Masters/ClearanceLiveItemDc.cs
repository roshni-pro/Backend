using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class ClearanceLiveItemDc
    {
        public long Id { set; get; }
        public int LiveStockQty { get; set; }
        public int RemainingStockQty { get; set; }
        public double UnitPrice { get; set; }
        public string ItemName { get; set; }
        public int MOQ { get; set; }
        public int ShelfLife { get; set; }
        public string ImageUrl { get; set; }
        public double MRP { get; set; }
        public int CategoryId { set; get; }
        public int totalcount { set; get; }
    }

    public class ClearanceOrderItemMasterDc
    {
        public int ItemId { set; get; }
        public string HSNCode { get; set; }
        public double NetPurchasePrice { get; set; }
        public double TotalTaxPercentage { get; set; }
        public string LogoUrl { get; set; }
        public int ItemMultiMprId { get; set; }
        public string SupplierName { get; set; }
        public string CategoryName { get; set; }
        public string SubsubcategoryName { set; get; }
        public string SubcategoryName { set; get; }
        public string SellingSku { set; get; }
        public string CityName { set; get; }
        public string itemcode { set; get; }
        public string Number { set; get; }
        public double MRP { set; get; }
        public double SizePerUnit { set; get; }
        public double TotalCessPercentage { set; get; }
        public double PurchasePrice { set; get; }
        public string ABCClassification { set; get; }
        public int SubsubCategoryid { set; get; }
        public int SubCategoryId { set; get; }
        public int Categoryid { set; get; }
        public string itemname { set; get; }
        public int MOQ { set; get; }

    }

    public class SearchClearanceLiveItemDc
    {

        public int Skip { set; get; }
        public int take { get; set; }
        public string keyword { get; set; }
        public int WarehouseId { get; set; }
        public int CategoryId { get; set; }
        public int Customerid { get; set; }
        public string lang { get; set; }

    }

    public class SearchClearanceStockDc
    {

        public int Skip { set; get; }
        public int take { get; set; }
        public string keyword { get; set; }
        public int WarehouseId { get; set; }
        public bool IsExport { get; set; }

    }

    public class ClearanceLiveItemListDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public long Id { set; get; }
        public int TotalQty { get; set; }
        public int AvailableQty { get; set; }
        public double UnitPrice { get; set; }
        public string itemname { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? MFGDate { get; set; }
        public string BatchCode { get; set; }
        public double? ShelfLifeLeft { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool OfferActive { get; set; }
        public string StockType { get; set; }
        public long ClearanceStockBatchMasterId { set; get; }
        public string ABCClassification { get; set; }
        public int RemainShelfLifedays { get; set; }
    }
    public class ClearanceLiveItemList
    {
        public int TotalRecords { get; set; }
        public List<ClearanceLiveItemListDc> ClearanceLiveItemLists { get; set; }
    }
    public class ClearanceStockLists
    {
        public int TotalRecords { get; set; }
        public List<ClearanceStockListDc> ClearanceStockListDc { get; set; }
    }


    public class ClearanceStockListDc
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public long Id { set; get; }
        public int TotalQty { get; set; }
        public int AvailableQty { get; set; }
        public double UnitPrice { get; set; }
        public string itemname { get; set; }
        public string GroupName { get; set; }
        public int LiveRemainingStockQty { get; set; }

        public double StoreDiscount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? MFGDate { get; set; }
        public string BatchCode { get; set; }
        public double? ShelfLifeLeft { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool OfferActive { get; set; }
        public string StockType { get; set; }
        public long ClearanceStockBatchMasterId { set; get; }
        public string ABCClassification { get; set; }
        //public int? ClPrice { get; set; }
        public int? PromoCost { get; set; }
        public long BatchMasterId { set; get; }
        public long ClearanceStockId { set; get; }
        public double DefaultUnitPrice { get; set; }
        public string DiscountType { get; set; }
        public double Discount { get; set; }
        public int ItemId { get; set; }
        public int Cityid { get; set; }
        public int RemainShelfLifedays { get; set; }
    }

    public class ExportClearanceLiveItemFilterDc
    {
        public int WarehouseId { get; set; }
        public string keyword { get; set; }

    }
    public class UpdateClearanceStockDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public long Id { set; get; }
        public long ClearanceStockId { set; get; }
        public int TotalQty { get; set; }
        public int AvailableQty { get; set; }
        public double UnitPrice { get; set; }
        public string BatchCode { get; set; }
        public string StockType { get; set; }
        public long ClearanceStockBatchMasterId { set; get; }
        public double ClPrice { get; set; }
        public string DiscountType { get; set; }
        public int Discount { get; set; }
        public int GroupId { get; set; }
        public string ApplyType { get; set; }
        public string itemname { get; set; }

        public bool IsOfferGenerated { get; set; }
    }


    public class AvailableItemForClNSOrderList
    {
        public int TotalRecords { get; set; }
        public List<AvailableItemForClNSOrderDC> AvailableItemForClNSOrderDC { get; set; }
    }


    public class AvailableItemForClNSOrderDC
    {
        //public long Id { set; get; }
        public int ItemMultiMRPId { get; set; }
        public int AvailableQty { get; set; }
        public string itemname { get; set; }
        public double MRP { get; set; }
        public string BatchCode { get; set; }
        public string StockType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime MFGDate { get; set; }
        public double ShelfLife { get; set; }
        public string BuyerName { get; set; }
    }

    public class GetGroupNameDc
    {
        public long Id { get; set; }
        public string GroupName { get; set; }
    }

    public class ItemListsDc
    {
        public string Number { get; set; }
        public string itemBaseName { get; set; }
    }


    public class GetHistoryOfStocksList
    {
        public int TotalRecords { get; set; }
        public List<GetHistoryOfStocksDc> GetHistoryOfStocksLists { get; set; }
    }
    public class GetHistoryOfStocksDc
    {

        public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public string ItemNumber { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Inventory { get; set; }
        public string Comment { get; set; }
        public string PeopleFirstName { get; set; }
    }

    public class GetBuyerMailsForItems
    {
        public string BuyerName { get; set; }
        public string Email { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Number { get; set; }
        public string BatchCode { get; set; }
        public int StockQty { get; set; }
        public int LiveQty { get; set; }
        public int RemainingQty { get; set; }
        public double RemainingQtyValue { get; set; }
        public double APP { get; set; }
        public DateTime? ExpiryDate { get; set; }

    }

    public class getEmailList
    {
        public string Email { get; set; }
        public string ItemList { get; set; }
    }
    public class GetRemainAndLiveStock
    {
        public int RemainingQty { get; set; }
        public int LiveQty { get; set; }
    }

    public class GetClearanceDashboardDataDc
    {
        public int WarehouseId { get; set; }
        public String WarehouseName { get; set; }

        public String Status { get; set; }
        public int MTDQty { get; set; }
        public double MTDValue { get; set; }
        public int YTDQty { get; set; }
        public double YTDValue { get; set; }
        public int MTDTotalQty { get; set; }
        public double MTDTotalValue { get; set; }
        public int YTDTotalQty { get; set; }
        public double YTDTotalValue { get; set; }
    }
    public class GetClearanceDashboardPayLoadDc
    {
        public List<int> WarehouseIds { get; set; }
        public DateTime Date { get; set; }
        public string Value { get; set; }
        public List<int> BrandIds { get; set; }   
        public string Status { get; set; }
    }

    public class AllExportClearanceDashboardDataDc
    {
        public List<PendingExportClearanceDashboardDataDc> PendingExportClearanceDashboardDataDcs { get; set; }
        public List<ApprovedExportClearanceDashboardDataDc> ApprovedExportClearanceDashboardDataDcs { get; set; }
        public List<PhysicallMovedExportClearanceDashboardDataDc> PhysicallMovedExportClearanceDashboardDataDcs { get; set; }
        public List<PrepareItemExportClearanceDashboardDataDc> PrepareItemExportClearanceDashboardDataDcs { get; set; }

    }
    public class PrepareItemExportClearanceDashboardDataDc
    {
        public string WarehouseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public int Quantity { get; set; }
        public double TotalValue { get; set; }
        public double CurrentShelfLife { get; set; }
        public string BuyerName { get; set; }

    }

    public class PendingExportClearanceDashboardDataDc
    {
        public string WarehouseName { get; set; }
        public long ClearanceMovementOrderID { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public int Quantity { get; set; }
        public double TotalValue { get; set; }

        public DateTime OrderCreatedDate { get; set; }

        public string OrderCreatedBy { get; set; }
        public string BuyerName { get; set; }

    }
    public class ApprovedExportClearanceDashboardDataDc
    {
        public string WarehouseName { get; set; }
        public long ClearanceMovementOrderID { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public int Quantity { get; set; }
        public double TotalValue { get; set; }
        public DateTime OrderCreatedDate { get; set; }
        public string OrderCreatedBy { get; set; }
        public string BuyerName { get; set; }
        public int ApprovedQty { get; set; }
        public double ApprovedValue { get; set; }

        public DateTime? PickerCreatedDate { get; set; }


    }
    public class PhysicallMovedExportClearanceDashboardDataDc
    {
        public string WarehouseName { get; set; }
        public long ClearanceMovementOrderID { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string itemname { get; set; }
        public int Quantity { get; set; }
        public double TotalValue { get; set; }
        public DateTime OrderCreatedDate { get; set; }
        public string OrderCreatedBy { get; set; }
        public string BuyerName { get; set; }
        public int ApprovedQty { get; set; }
        public double ApprovedValue { get; set; }

        public DateTime? PickerCreatedDate { get; set; }
        public DateTime? PhysicallyMovedDate { get; set; }
        public int PhysicallyMovedQty { get; set; }
        public string PhysicallyMovedBy { get; set; }

    }
    public class ClearanceDashboardBrandPayLoad
    {
        public List<int> WarehouseId { get; set; }

    }
    public class GetClearanceDashboardBrandListDc
    {
        public int BrandNumber { get; set; }
        public String BrandName { get; set; }
       
    }

    public class ClearanceLiveItemsAllExportDC
    {
        public List<int> WarehouseIds { get; set; }
       
    }

    public class TCSDc
    {
        public double TCSPercent { get; set; }
        public double PreTotalDispatched { get; set; }
        public double TCSLimit { get; set; }
    }

}
