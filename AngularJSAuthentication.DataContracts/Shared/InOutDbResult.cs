using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class InOutDbResult
    {
        public HashSet<ItemDetails> ItemDetails { get; set; }
        public List<InOutTransactions> InOutTransactions { get; set; }
    }

    public class DbResultInOut
    {
        public List<ItemDetailsInOut> ItemDetails { get; set; }
        public List<TransactionsInOut> InOutTransactions { get; set; }
        public List<ItemLastPurchasePrice> ItemLastPurchasePrices { get; set; }
    }

    public class ItemDetailsInOut
    {
        public string multimrpwarehouse { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double ItemTaxPercent { get; set; }
        public string ItemCode { get; set; }
        public int BrandId { get; set; }
        public string Brand { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int FreeQuantity { get; set; }
        public int IntransitQty { get; set; }
        public double IntransitAmount { get; set; }
        public double InvoiceDiscount { get; set; }
        public double? Ptr { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public string CityName { get; set; }
        public string StoreName { get; set; }
        //public int FreeInCS { get; set; }
        //public int FreeOutCS { get; set; }
    }

    public class TransactionsInOut
    {
        public string multimrpwarehouse { get; set; }
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public int Qty { get; set; }
        public double amount { get; set; }
        public string source { get; set; }
        public int TransType { get; set; }
    }

    public class InOutDbResultForInOut
    {
        public List<ItemDetails> ItemDetails { get; set; }
        public List<InOutTransactions> InOutTransactions { get; set; }
        public List<ItemLastPurchasePrice> ItemLastPurchasePrices { get; set; }
    }

    public class InOutDbResultMongoNew
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string HashKey { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedDate { get; set; }
        //public List<ItemDetails> ItemDetails { get; set; }
        //public List<InOutTransactions> InOutTransactions { get; set; }
        //public List<ItemLastPurchasePrice> ItemLastPurchasePrices { get; set; }
    }

    public class SaleVsInventoryResultInOut
    {
        public List<ItemDetails> ItemDetails { get; set; }
        public List<reportSaleInOut> reportSaleInOuts { get; set; }
        public List<reportInventoryInOut> reportInventoryInOuts { get; set; }
    }

    public class reportSaleInOut
    {
        public string multimrpwarehouse { get; set; } // Inventory
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public int Qty { get; set; }
        public double amount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
    public class reportInventoryInOut
    {
        public string multimrpwarehouse { get; set; } // Inventory
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public int Qty { get; set; }
        public double amount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class ItemLastPurchasePrice
    {
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public double price { get; set; }
    }
    public class ItemDetails
    {
        public string multimrpwarehouse { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double ItemTaxPercent { get; set; }
        public string ItemCode { get; set; }
        public int BrandId { get; set; }
        public string Brand { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public int FreeQuantity { get; set; }
        public int IntransitQty { get; set; }
        public double IntransitAmount { get; set; }
        public double InvoiceDiscount { get; set; }
    }
    public class InOutTransactions
    {
        public string multimrpwarehouse { get; set; }
        public int itemmultimrpid { get; set; }
        public int warehouseid { get; set; }
        public int Qty { get; set; }
        public double amount { get; set; }
        public string source { get; set; }
        public int TransType { get; set; }
        public double PurchasePrice { get; set; }
        public double SellingPrice { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string itemname { get; set; }
        public string ItemCode { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string WarehouseName { get; set; }
        public string BuyerName { get; set; }
        public int BrandId { get; set; }
        public int BuyerId { get; set; }
        public int CategoryId { get; set; }
        public double MRP { get; set; }
        public double ItemTaxPercent { get; set; }
        public int IntransitQty { get; set; }
        public double IntransitAmount { get; set; }
        public double InvoiceDiscount { get; set; }
        public int FreeQuantity { get; set; }
    }


    public class CombinedDataMongo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string HashKey { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public int BrandId { get; set; }
        public string Brand { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? MRP { get; set; }
        public double? TaxRate { get; set; }
        public double? SellingPrice { get; set; }
        public int? OpeningQty { get; set; }
        public double? OpeningAmount { get; set; }
        public int? POInwardQty { get; set; }
        public double? POInwardAmount { get; set; }
        public int? WhInQty { get; set; }
        public double? WhInAmount { get; set; }
        public int? WhOutQty { get; set; }
        public double? WhOutAmount { get; set; }
        public int? SaleQty { get; set; }
        public double? SaleAmount { get; set; }
        public int? PilferageQty { get; set; }
        public double? PilferageAmount { get; set; }
        public int? CancelInQty { get; set; }
        public double? CancelInAmount { get; set; }
        public int FreeInQty { get; set; }
        public double? FreeInAmount { get; set; }
        public int FreeOutQty { get; set; }
        public double? FreeOutAmount { get; set; }

        public int? POReturnQty { get; set; }
        public double? POReturnAmount { get; set; }
        public int? ManualInQty { get; set; }
        public double? ManualInAmount { get; set; }

        public int? ErrorInQty { get; set; }
        public double? ErrorInAmount { get; set; }

        public int? ManualOutQty { get; set; }
        public double? ManualOutAmount { get; set; }
        public int? DamageOutQty { get; set; }
        public double? DamageOutAmount { get; set; }
        public int? ExpiryOutQty { get; set; }
        public double? ExpiryOutAmount { get; set; }
        public int? StockTransferInQty { get; set; }
        public double? StockTransferInAmount { get; set; }
        public int? StockTransferOutQty { get; set; }
        public double? StockTransferOutAmount { get; set; }
        public int? ClosingQty { get; set; }
        public double? ClosingAmount { get; set; }
        public int? IntransitQty { get; set; }
        public double? IntransitAmount { get; set; }
        public int? TotalInQty { get; set; }
        public int? TotalOutQty { get; set; }
        public int? InOutDiffQty { get; set; }
        public double? InOutDiffAmount { get; set; }
        public double? FrontMargin { get; set; }
        public double? InvoiceDiscount { get; set; }
        public string SupplierName { get; set; }
    }

    public class CombinedData
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public int ItemMultiMrpId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public int BrandId { get; set; }
        public string Brand { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public string CityName { get; set; }
        public string StoreName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public double? MRP { get; set; }
        public double? TaxRate { get; set; }
        public double? SellingPrice { get; set; }
        public int? OpeningQty { get; set; }
        public double? OpeningAmount { get; set; }
        public int? POInwardQty { get; set; }
        public double? POInwardAmount { get; set; }
        public int? WhInQty { get; set; }
        public double? WhInAmount { get; set; }
        public int? WhOutQty { get; set; }
        public double? WhOutAmount { get; set; }
        public int? SaleQty { get; set; }
        public double? SaleAmount { get; set; }
        public int? PilferageQty { get; set; }
        public double? PilferageAmount { get; set; }
        public int? CancelInQty { get; set; }
        public double? CancelInAmount { get; set; }
        public int FreeInQty { get; set; }
        public double? FreeInAmount { get; set; }
        public int FreeOutQty { get; set; }
        public double? FreeOutAmount { get; set; }

        public int? POReturnQty { get; set; }
        public double? POReturnAmount { get; set; }
        public int? ManualInQty { get; set; }
        public double? ManualInAmount { get; set; }

        public int? ErrorInQty { get; set; }
        public double? ErrorInAmount { get; set; }

        public int? ManualOutQty { get; set; }
        public double? ManualOutAmount { get; set; }
        public int? DamageInQty { get; set; }
        public double? DamageInAmount { get; set; }
        public int? DamageOutQty { get; set; }
        public double? DamageOutAmount { get; set; }
        public int? ClearanceInQty { get; set; }
        public double? ClearanceInAmount { get; set; }
        public int? ClearanceOutQty { get; set; }
        public double? ClearanceOutAmount { get; set; }
        public int? NonRevenueInQty { get; set; }//sudhir 04-08-2023
        public double? NonRevenueInAmount { get; set; }//sudhir 04-08-2023
        public int? NonRevenueOutQty { get; set; }//sudhir 04-08-2023
        public double? NonRevenueOutAmount { get; set; }//sudhir 04-08-2023

        public int? NonSellableInQty { get; set; }
        public double? NonSellableInAmount { get; set; }
        public int? NonSellableOutQty { get; set; }
        public double? NonSellableOutAmount { get; set; }
        public int? ExpiryOutQty { get; set; }
        public double? ExpiryOutAmount { get; set; }
        public int? StockTransferInQty { get; set; }
        public double? StockTransferInAmount { get; set; }
        public int? StockTransferOutQty { get; set; }
        public double? StockTransferOutAmount { get; set; }
        public int? ClosingQty { get; set; }
        public double? ClosingAmount { get; set; }
        public int? IntransitQty { get; set; }
        public double? IntransitAmount { get; set; }
        public int? TotalInQty { get; set; }
        public int? TotalOutQty { get; set; }
        public int? InOutDiffQty { get; set; }
        public double? InOutDiffAmount { get; set; }
        public double? FrontMargin { get; set; }
        public double? InvoiceDiscount { get; set; }
        public double? PtrPrice { get; set; }
        public double? SaleOnPtr { get; set; }
        public double? CancelOnPtr { get; set; }

        public double? PtrMargin { get; set; }
        public double? OpeningClaim { get; set; }
        public double? ClaimsGenerated { get; set; }
        public double? MonthClaimBackMargin { get; set; }
        public double? ClosingClaim { get; set; }
        public double? Difference { get; set; }
        public double? Totalmargin { get; set; }

    }


    public class CatSubCatBrandFlatData
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string BrandLogo { get; set; }
        public string SubCatLogo { get; set; }
        public string CatLogo { get; set; }
        public int SubCategoryid { get; set; }
        public string SubcategoryName { get; set; }
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
    }


    public class BuyerCatSubSubCategory
    {
        public List<BuyerCategoryDc> buyerCategoryDcs { get; set; }
        public List<BuyerSubcategoryDc> buyerSubcategoryDcs { get; set; }
        public List<BuyerSunSubCategoryDc> buyerSunSubCategoryDcs { get; set; }
    }
    public class BuyerCategoryDc
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string LogoUrl { get; set; }
    }
    public class BuyerSubcategoryDc
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryid { get; set; }
        public string SubcategoryName { get; set; }
    }
    public class BuyerSunSubCategoryDc
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public string LogoUrl { get; set; }
    }
    public class BuyerItemDetailDc
    {
        public int ItemId { get; set; }
        public string Number { get; set; }
        public string itemname { get; set; }
        public string MinOrderQty { get; set; }
        public double DisplaySellingPrice { get; set; }
        public double MRP { get; set; }
        public double price { get; set; }
        public double UnitPrice { get; set; }
        public double VATTax { get; set; }
        public string HSNCode { get; set; }
        public string SupplierName { get; set; }
        public string LogoUrl { get; set; }
        public bool active { get; set; }
    }


    public class BuyerDataExportDc
    {
        public int ItemMultiMrpId { get; set; } //
        public string ItemName { get; set; }//
        public string ItemNumber { get; set; }//
        public int BrandId { get; set; }//
        public string Brand { get; set; }//
        public int CategoryId { get; set; }//
        public string Category { get; set; }//
        public int WarehouseId { get; set; } //
        public string WarehouseName { get; set; }//
        public int BuyerId { get; set; }//
        public string BuyerName { get; set; }//
        public double? MRP { get; set; }//
        public double? TaxRate { get; set; }//
        public double? CancelAmt { get; set; }
        public double? SaleAmt { get; set; }
        public double? NetSale { get; set; }
        public double? GrossMargin { get; set; }
        public double? ClosingAmount { get; set; }
        public double? MarginPercent { get; set; }
        public double? InventoryDays { get; set; }
    }
}
