using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.SalesAppDc
{
    public class ItemForSalesAppDc
    {
        public int BaseCategoryid { get; set; }
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string itemname { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }
        public string LogoUrl { get; set; }
        public double price { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int Categoryid { get; set; }
        public int WarehouseId { get; set; }
        public int CompanyId { get; set; }
        public double UnitPrice { get; set; }
        public string HindiName { get; set; }
        public double marginPoint { get; set; }
        public int promoPerItems { get; set; }
        public bool IsOffer { get; set; }
        public int OfferCategory { get; set; }
        public DateTime OfferStartTime { get; set; }
        public DateTime OfferEndTime { get; set; }
        public double OfferQtyAvaiable { get; set; }
        public double OfferQtyConsumed { get; set; }
        public int OfferId { get; set; }
        public string OfferType { get; set; }
        public double OfferWalletPoint { get; set; }
        public int OfferFreeItemId { get; set; }
        public double OfferPercentage { get; set; }
        public string OfferFreeItemName { get; set; }
        public string OfferFreeItemImage { get; set; }
        public int OfferFreeItemQuantity { get; set; }
        public int OfferMinimumQty { get; set; }
        public double FlashDealSpecialPrice { get; set; }
        public int FlashDealMaxQtyPersonCanTake { get; set; }
        public double NetPurchasePrice { get; set; }
        public double TotalTaxPercentage { get; set; }
        public bool IsSensitive { get; set; }
        public double IsSensitiveMRP { get; set; }
        public int MinOrderQty { get; set; }
        public string CategoryName { get; set; }
        public double Discount { get; set; }
        public double VATTax { get; set; }
        public string itemBaseName { get; set; }
        public bool active { get; set; }
        public bool Deleted { get; set; }
        public int BillLimitQty { get; set; }
        public double DistributionPrice { get; set; }
        public bool DistributorShow { get; set; }
        public int ItemAppType { get; set; }
        public int ItemLimitQty { get; set; }
        public bool IsItemLimit { get; set; }
        public double Margin { get; set; }
        public double MRP { get; set; }
        public int ItemMultiMRPId { get; set; }
    }
    public class ItemFilterPostDc
    {
        public int PeopleId { get; set; }
        public int warehouseId { get; set; }
        public string SearchKeyWord { get; set; }
        public int ScoreFrom { get; set; }
        public int ScoreTo { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class CatCompanyBrandDc
    {
        public List<CategoriesDc> categoryDcs { get; set; }
        public List<SubCategoryDCs> subCategoryDC { get; set; }
        public List<SubsubCategoryDcs> subsubCategoryDc { get; set; }
    }
    public class CategoriesDc
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int itemcount { get; set; }
        public string CategoryImg { get; set; }
    }

    //public class SubCategoryDCs
    //{
    //    public int SubCategoryId { get; set; }
    //    public int Categoryid { get; set; }
    //    public string CategoryName { get; set; }
    //    public string SubcategoryName { get; set; }
    //    public string LogoUrl { get; set; }
    //    public int itemcount { get; set; }
    //    public string StoreBanner { get; set; }
    //}
    //public class SubsubCategoryDcs
    //{
    //    public int SubsubCategoryid { get; set; }
    //    public string SubsubcategoryName { get; set; }
    //    public int BaseCategoryId { get; set; }
    //    public int Categoryid { get; set; }
    //    public int SubCategoryId { get; set; }
    //    public string LogoUrl { get; set; }
    //    public int itemcount { get; set; }
    //}
}
