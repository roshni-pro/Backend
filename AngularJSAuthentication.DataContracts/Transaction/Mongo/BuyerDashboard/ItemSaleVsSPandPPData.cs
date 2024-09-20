using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.Mongo.BuyerDashboard
{
    public class ItemSaleVsSPandPPData
    {
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? MRP { get; set; }
        public double? TaxPercentage { get; set; }
        public List<SaleAndSPPPData> SaleAndSPPPData { get; set; }
    }

    public class SaleAndSPPPData
    {
        public int Qty { get; set; }
        public double SaleAmount { get; set; }
        public double SellingPrice { get; set; }
        public double PurchasePrice { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
