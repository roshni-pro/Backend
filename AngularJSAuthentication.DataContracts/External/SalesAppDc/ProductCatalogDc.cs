using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    public class ProductCatalogDc
    {
        public string SectionName { get; set; }
        public string SectionHindiName { get; set; }
        public string Type { get; set; }
        public long WarehouseId { get; set; }
        public bool IsPromotional { get; set; }
        public int Sequence { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
    public class UpdateProductCatalogDc
    {
        public long Id { get; set; }
        public string SectionName { get; set; }
        public string SectionHindiName { get; set; }
        public string Type { get; set; }
        public long WarehouseId { get; set; }
        public bool IsPromotional { get; set; }
        public int Sequence { get; set; }
    }
    public class Get_ProductCatalogDC
    {
        public List<UpdateProductCatalogDc> GetAllProductCatalog { get; set; }
        public long TotalCount { get; set; }
    }

    public class PopularRecentSearchDc
    {
        public List<string> RecentSearch { get; set; }
        public List<string> PopularSearch { get; set; }
    }
    public class ProductCatalogItemDc
    {
        public int WarehouseId { get; set; }
        public int StoreId { get; set; }
        public string ItemNumber { get; set; }
        public bool IsPromotional { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }   

    public class CatalogItemListDc
    {
        public bool active { get; set; }
        public int ItemId { get; set; }
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int ItemlimitQty { get; set; }
        public bool IsItemLimit { get; set; }
        public string ItemNumber { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public double Discount { get; set; }
        public string SellingSku { get; set; }
        public string SellingUnitName { get; set; }
        public double VATTax { get; set; }
        public string itemname { get; set; }
        public double price { get; set; }
        public double UnitPrice { get; set; }
        public string LogoUrl { get; set; }
        public int MinOrderQty { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? marginPoint { get; set; }
        public int? promoPerItems { get; set; }
        public int? dreamPoint { get; set; }
        public bool IsOffer { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }
        public string itemBaseName { get; set; }
        public bool Deleted { get; set; }
        public bool IsFlashDealUsed { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double NetPurchasePrice { get; set; }
        public int BillLimitQty { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsSensitiveMRP { get; set; }
        public string HindiName { get; set; }
        public int? OfferCategory { get; set; }
        public DateTime? OfferStartTime { get; set; }
        public DateTime? OfferEndTime { get; set; }
        public double? OfferQtyAvaiable { get; set; }

        public double? OfferQtyConsumed { get; set; }

        public int? OfferId { get; set; }

        public string OfferType { get; set; }

        public double? OfferWalletPoint { get; set; }
        public int? OfferFreeItemId { get; set; }
        public int? FreeItemId { get; set; }
        public double? OfferPercentage { get; set; }
        public string OfferFreeItemName { get; set; }
        public string OfferFreeItemImage { get; set; }
        public int? OfferFreeItemQuantity { get; set; }
        public int? OfferMinimumQty { get; set; }
        public double? FlashDealSpecialPrice { get; set; }
        public int? FlashDealMaxQtyPersonCanTake { get; set; }
        public double? DistributionPrice { get; set; }
        public bool DistributorShow { get; set; }
        public int ItemAppType { get; set; }
        public double? MRP { get; set; }
        public bool IsPrimeItem { get; set; }
        public decimal PrimePrice { get; set; }
        public DateTime NoPrimeOfferStartTime { get; set; }
        public DateTime CurrentStartTime { get; set; }
        public bool IsFlashDealStart { get; set; }
        public string Scheme { get; set; }
        public string Number { get; set; }
        public int Itemtype { get; set; } //1=baseitem,2=suggested item,3=promotional item
        public DateTime LastOrder { get; set; }
        public int qty { get; set; }
        public string Classification { get; set; }
        public string BackgroundRgbColor { get; set; }
        public double? TradePrice { get; set; }
        public double? WholeSalePrice { get; set; }

    }
    public class UpdateProductCatalogItemDc
    {
        public long Id { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public double SellingPrice { get; set; }
        public int WarehouseId { get; set; }
        public int Sequence { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public bool IsPromotional { get; set; }
        public string ActiveStatus { get; set; }
        public long SectionId { get; set; }
    }
}
