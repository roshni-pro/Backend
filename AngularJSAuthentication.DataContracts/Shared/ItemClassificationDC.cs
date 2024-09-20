namespace AngularJSAuthentication.DataContracts.Shared
{
    public class ItemClassificationDC
    {
        public string ItemNumber { get; set; }
        public int WarehouseId { get; set; }
        public string Category { get; set; }
        public long StockBatchMasterId { get; set; }
    }

    public class ItemClassificationReportDC
    {
        public string ItemNumber { get; set; }
        public int WarehouseId { get; set; }
        public string ItemName { get; set; }
        public string WarehouseName { get; set; }
        public string Category { get; set; }
        public double SalePercent { get; set; }
        public int Qty { get; set; }
        public double SaleAmount { get; set; }
        public double StockValue { get; set; }
        public int StockQty { get; set; }
        public double StockPercent { get; set; }
    }
    public class ItemClassificationInActiveReportDc
    {
        public string ItemNumber { get; set; }
        public string Category { get; set; }
        public string ItemBaseName { get; set; }
        public string WarehouseName { get; set; }
        public double StockAmount { get; set; }
        public int ItemActive { get; set; }
        public string Comment { get; set; }


    }
    public class CFRReportDc
    {
        public string ItemNumber { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string itemBaseName { get; set; }
        public double MRP { get; set; }
        public string WarehouseName { get; set; }
        public string Category { get; set; }
        public double LimitValue { get; set; }
        public int active { get; set; }
        public string activeItem { get; set; }
        public string CityName { get; set; }

    }

    public class ItemSalesInventoryData
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string CategoryName { get; set; }

        public string StoreName { get; set; }
        public double Inventory { get; set; }
        public double TotalInTransit { get; set; }
        public double TotalSale { get; set; }
    }
     public class IncentiveClassificationDc
    {
        public int ItemId { get; set; }
        public string IncentiveClassification { get; set; }
    }
}
