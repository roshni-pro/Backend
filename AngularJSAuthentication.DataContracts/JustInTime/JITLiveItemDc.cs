using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.JustInTime
{
    public class BrandListDc
    {
        public long BrandId { get; set; }
        public string BrandName { get; set; }
    }

    public class LiveItemListFilterDc
    {
        public int WarehouseId { get; set; }
        public int BrandId { get; set; }
        public string Keyword { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }

    public class LiveItemListDc
    {
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public long SupplierId { get; set; }
        public long SupplierDepoId { get; set; }
        public List<ItemMultiMrpDc> ItemMultiMrpList { get; set; }
        public string ItemMultiMrpListString { get; set; }
        public int TotalCount { get; set; }
    }
    public class GetLiveItemListDc
    {
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public long SupplierId { get; set; }
        public long SupplierDepoId { get; set; }
        public bool IsUnitPriceChange { get; set; }
        public int TotalCount { get; set; }
    }
    public class GetRiskItemListDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int RiskQuantity { get; set; }
        public double RiskPurchasePrice { get; set; }
        public int RiskType { get; set; }
    }

    public class ItemMultiMrpDc
    {
        public int Id { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public double POPrice { get; set; }
        public double PurchasePrice { get; set; }
        public double APP { get; set; }
        public double NPP { get; set; }
        public int LimitQty { get; set; }
        public int RiskQuantity { get; set; }
        public double RiskPurchasePrice { get; set; }
        public int DelCancelQty { get; set; }
        public int OpenPOQty { get; set; }
        public int InternalQty { get; set; }
        public long NetInventory { get; set; }
        public int CurrentNetInventory { get; set; }
        public string Classification { get; set; }
        public int LiveLimitQuantity { get; set; }
        public int WarehouseId { get; set; } // New Add for ROC
        public int Tag { get; set; } // New Add for ROC
        public bool active { get; set; }
        public double MRP { get; set; }
        public double? WeightedPurchasePrice { get; set; }
        public bool EnableAutoPrice { get; set; }
        public int ItemId { get; set; }
        public bool IsUnitPriceChange { get; set; }

    }

    public class UpdateJITLiveItemDc
    {
        public int WarehouseId { get; set; }
        public int SupplierId { get; set; }
        public int DepoId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int ItemId { get; set; }
        public double POPurchasePrice { get; set; }
        public double PurchasePrice { get; set; }
        public double RiskPurchasePrice { get; set; }
        public double Discount { get; set; }
        public double Margin { get; set; }
        public int LimitQuantity { get; set; }
        public int RiskQuantity { get; set; }
        public int QtyToLive { get; set; }
        public double UnitPrice { get; set; }
        public bool Active { get; set; }
        public double WholesalePrice { get; set; }
        public double TradePrice { get; set; }
        public double WholesaleMargin { get; set; }
        public double TradeMargin { get; set; }
    }

    public class UpdateItemLimitDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int SupplierId { get; set; }
        public int DepoId { get; set; }
        public double POPurchasePrice { get; set; }
        public double PurchasePrice { get; set; }
        public double RiskPurchasePrice { get; set; }
        public int LimitQuantity { get; set; }
        public int RiskQuantity { get; set; }
        public int QtyToLive { get; set; }

    }
    public class UpdateRiskDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double RiskPurchasePrice { get; set; }
        public int RiskQuantity { get; set; }
        public int RiskType { get; set; }

    }
    public class POUpdateRiskDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double RiskPurchasePrice { get; set; }
        public int RiskQuantity { get; set; }
        public int RiskType { get; set; }

    }
    public class InternalUpdateRiskDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double RiskPurchasePrice { get; set; }
        public int RiskQuantity { get; set; }
        public int RiskType { get; set; }

    }

    public class UpdateJITLiveItemResDc
    {
        public string SellingSku { get; set; }
        public string Error { get; set; }
        public bool Status { get; set; }
    }

    public class OpenMoqDc
    {
        public int ItemId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double DiscountPercent { get; set; }
        public double Margin { get; set; }
        public double MRP { get; set; }
        public string ItemName { get; set; }
        public bool Active { get; set; }
        public int MinOrderQty { get; set; }
        public string SellingSku { get; set; }
        public double TotalTax { get; set; }
        public double ActualUnitPrice { get; set; }
        //public double? MarginUpto { get; set; }
        public double? WholesaleMargin { get; set; }
        public double? TradeMargin { get; set; }
        public double? WholesalePrice { get; set; }
        public double? TradePrice { get; set; }
        public float? RetailerRMargin { get; set; }
        public float? WholesaleRMargin { get; set; }
        public float? TradeRMargin { get; set; }
        public double? WeightedPurchasePrice { get; set; }
        public bool EnableAutoPrice { get; set; }
        public bool IsUnitPriceChange { get; set; }
    }
    public class OpenMoqFilterDc
    {
        public int WarehouseId { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
    }

    public class PrepAutoItemActivateDc
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int QtyToLive { get; set; }
        public int BrandId { get; set; }
        public double PTR { get; set; }
        public int CurrentInventory { get; set; }
        public string ShowTypes { get; set; }
        public int ItemlimitQty { get; set; } //get oldlimit
        public int ItemId { get; set; }
        public double? UnitPrice { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMsg { get; set; }
        public DateTime CreatedDate { get; set; }

    }

    public class PrepItemIdDc
    {
        public int ItemId { get; set; }

        public int WarehouseId { get; set; }
    }

    public class PrepAutoDataDc
    {
        public List<PrepAutoItemActivateDc> PrepAutoItemActivates { get; set; }
        public List<PrepItemIdDc> PrepItemIds { get; set; }


    }

    public class PrepItemMailDataDc
    {
        public string WarehouseName { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int BrandId { get; set; }
        public int QtyToLive { get; set; }
        public int OldliveQTY { get; set; }
        public double? UnitPrice { get; set; }
        public double PTR { get; set; }
        public int CurrentInventory { get; set; }
        public string ShowTypes { get; set; }
        public string BuyerEmail { get; set; }
        public string Reason { get; set; }

    }
    public class RiskQuantityHistoryDc
    {
        public int ItemMultiMRPId { get; set; }
        public int RiskQuantity { get; set; }
        public double RiskPurchasePrice { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public int TotalCount { get; set; }
        public int LiveQuantity { get; set; }
        public int OldLiveQuantity { get; set; }
        public double APP { get; set; }
        public int LimitQty { get; set; }
        public string Comment { get; set; }

    }

    public class RiskQtyToReduceDc
    {
      
        public int InternalRiskQty { get; set; }
        public int PORiskQty { get; set; }
        public bool EnableAutoPrice { get; set; }
        
    }
    public class DailyItemASPDC
    {
        public string Warehouse{ get; set; }
        public double ItemMRP{ get; set; }
        public string ItemName{ get; set; }
        public double ASP{ get; set; }
        public double UnitPrice{ get; set; }
        public double TradePrice{ get; set; }
        public double WholesalePrice{ get; set; }
        public int TotalCount { get; set; }
    }

}
