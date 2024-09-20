using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Transaction.Mongo.BuyerDashboard
{
    public class ItemLedger
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double MRP { get; set; }
        public double TaxPercentage { get; set; }

        public List<ItemDailyLedger> DailyItemLedger { get; set; }
    }

    public class ItemDailyLedger
    {
        public int ItemMultiMrpId { get; set; }
        public DateTime GenerationDate { get; set; }
        public double APP { get; set; }
        public double SellingPrice { get; set; }
        public int OpeningStock { get; set; }
        public double OpeningStockAmount { get; set; }
        public int PoInwardQty { get; set; }
        public double PoInwardTotalAmount { get; set; }
        public int WareHouseInQty { get; set; }
        public double WarehouseInAmount { get; set; }
        public int WarehouseOutQty { get; set; }
        public double WarehouseOutAmount { get; set; }
        public int SaleQuantity { get; set; }
        public double SaleAmount { get; set; }
        public double pilferageAmount { get; set; }
        public int pilferageQty { get; set; }
        public int CancelInQty { get; set; }
        public double CancelInAmount { get; set; }
        public int ClosingStock { get; set; }
        public double ClosingAmount { get; set; }
        public int POReturnQty { get; set; }
        public double POReturnAmount { get; set; }
        public int ManualInventoryInQty { get; set; }
        public double ManualInventoryInAmount { get; set; }
        public int ManualInventoryOutQty { get; set; }
        public double ManualInventoryOutAmount { get; set; }
        public int FreeQuantity { get; set; }
        public int TotalInQty { get; set; }
        public int TotalOutQty { get; set; }
        public int InOutDiffQty { get; set; }
        public double InOutDiffAmount { get; set; }
        public double GrossProfit { get; set; }
        public double GrossProfitAfterPilferage { get; set; }
        public double IntransitQty { get; set; }
        public double FrontMargin { get; set; }

        public string VendorNames { get; set; }
    }

    public class BuyerDc
    {
        public int PeopleID { get; set; }
        public string DisplayName { get; set; }
    }
    
}
