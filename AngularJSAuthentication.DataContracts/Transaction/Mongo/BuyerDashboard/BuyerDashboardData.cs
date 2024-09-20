using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.Mongo.BuyerDashboard
{
    public class BuyerDashboardData
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double SaleAmount { get; set; }
        public double ClosingStock { get; set; }
        public double GrossMargin { get; set; }
        public double InventoryDays { get; set; }
        public int ActiveArticles { get; set; }
        public int ActiveVendors { get; set; }
        public List<string>  Vendors { get; set; }
        public List<int> ItemMultiMrpIds { get; set; }
        public double NetSale { get; set; }
        public double MarginPercent { get; set; }

    }

    public class BuyerDashboardDataExport
    {
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public double SaleAmount { get; set; }
        public double NetSale { get; set; }
        public double CancelAmt { get; set; }
        public double ClosingStock { get; set; }
        public double GrossMarginPercent { get; set; }
        public double GrossMargin { get; set; }
        public double InventoryDays { get; set; }
        //public int ActiveArticles { get; set; }
        //public int ActiveVendors { get; set; }
        //public string Vendors { get; set; }
        //public List<int> ItemMultiMrpIds { get; set; }
    }
}
