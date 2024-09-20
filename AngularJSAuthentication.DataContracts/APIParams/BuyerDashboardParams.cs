using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class BuyerDashboardParams
    {
        public List<int> BuyerIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StockType { get; set; }
    }

    public class BuyerDashboardSalesParams
    {
        public int BuyerId { get; set; }
        public List<int> ItemMultiMrpIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Columns { get; set; } //brand, warehouse,both
        public string ExportType { get; set; } // Sales, Closing, GrossMargin, InventoryDays, 
    }

    public class ItemSaleVsSPandPPParams
    {
        public int WarehouseId { get; set; }
        public List<int> ItemMultiMrpIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ItemSaleVsInventoryParams
    {
        public int WarehouseId { get; set; }
        public List<int> ItemMultiMrpIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class BuyerItemDetailParams
    {
        public int SubsubCategoryid { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int WarehouseID { get; set; }
    }
}
