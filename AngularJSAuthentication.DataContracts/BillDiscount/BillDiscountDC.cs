using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.BillDiscount
{
    public class BillDiscountDC
    {

    }
    public class BillDiscountAllFilesDC
    {

        public List<BillDiscountResponseExcel> BillDiscountResponseExcel { get; set; }
        public List<IncludeItemResponseExcel> IncludeItemResponseExcel { get; set; }

        public List<ExcludeItemResponseExcel> ExcludeItemResponseExcel { get; set; }

        public List<MandatoryResponseExcel> MandatoryResponseExcel { get; set; }

        public List<FreeitemResponseExcel> FreeitemResponseExcel { get; set; }

        public bool Status { get; set; }
        public string msg { get; set; }


    }
    public class BillDiscountResponseExcel
    {
        public List<billDiscountDC> billDiscountDC { get; set; }
        public bool Status { get; set; }
        public string msg { get; set; }
    }
    public class billDiscountDC
    {
        public string WarehouseName { get; set; }
        public string SelectAppType { get; set; }
        public string OfferName { get; set; }
        public string OfferOn { get; set; }
        public bool AutoApply { get; set; }
        public string BillDiscountType { get; set; }
        public string OfferBy { get; set; }
        public string SelectStore { get; set; }
        public string Usertype { get; set; }
        public string GroupLevelName { get; set; }
        public bool MultiTimeUse { get; set; }
        public int? OfferUseCount { get; set; }
        public bool UseOtherOffer { get; set; }
        public int OfferLimit { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string DiscountOn { get; set; }
        public double Percentage { get; set; }
        public string WalletType { get; set; }
        public string ApplyOn { get; set; }
        public double WalletPoint { get; set; }
        public double BillAmountMinLimit { get; set; }
        public double BillAmountMaxLimit { get; set; }
        public double MaximumDiscount { get; set; }
        public int OfferCode { get; set; }
        public int NumberOfLineItem { get; set; }
        //public int AddLimeItemValue { get; set; }
        public string Description { get; set; }
        public int userid { get; set; }
        public string ExcludeUserGroup { get; set; }
        public string CombinedOfferGroup { get; set; }

    }
    public class IncludeItemResponseExcel
    {
        public List<offeruploadIncludeItemDC> IncludeItemDC { get; set; }
        public bool Status { get; set; }
        public string msg { get; set; }
    }
    public class offeruploadIncludeItemDC  //Name Offer Name Category    Sub Category    Brand ItemMultiMRPId
    {
        public string WarehouseName { get; set; }
        public string Offer_Name { get; set; }
        public string Category { get; set; }
        public string Sub_Category { get; set; }
        public string BrandName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int userid { get; set; }

        public int SubCategoryMappingId { get; set; }
        public int BrandCategoryMappingId { get; set; }

    }

    public class ExcludeItemResponseExcel
    {
        public List<offeruploadExcludeItemDC> ExcludeItemDC { get; set; }
        public bool Status { get; set; }
        public string msg { get; set; }
    }

    public class offeruploadExcludeItemDC  //Name Offer Name Category    Sub Category    Brand ItemMultiMRPId
    {
        public string WarehouseName { get; set; }
        public string Offer_Name { get; set; }
        public string Category { get; set; }
        public string Sub_Category { get; set; }
        public string BrandName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int userid { get; set; }
    }

    public class FreeitemResponseExcel
    {
        public List<offeruploadFreeitemDC> FreeitemDC { get; set; }
        public bool Status { get; set; }
        public string msg { get; set; }
    }
    public class offeruploadFreeitemDC
    {
        public string WarehouseName { get; set; }
        public string Offer_Name { get; set; }
        public string Category { get; set; }
        public string Sub_Category { get; set; }
        public string BrandName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string Stock_Hit { get; set; }
        public int Freeitem_Qty { get; set; }
        public int userid { get; set; }
        public string SellingSku { get; set; }
    }

    public class MandatoryResponseExcel
    {

        public List<offeruploadMandatoryDC> MandatoryDC { get; set; }
        public bool Status { get; set; }
        public string msg { get; set; }
    }

    //Warehouse Name	Offer Name	Category	Sub Category	Brand	ItemName	Qty/Value
    public class offeruploadMandatoryDC
    {
        public string ItemRequiredOn { get; set; }
        public string ValueType { get; set; }
        public string WarehouseName { get; set; }
        public string Offer_Name { get; set; }
        public string Category { get; set; }
        public string Sub_Category { get; set; }
        public string BrandName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }

        public int userid { get; set; }
        public int BrandId { get; set; }
    }

    public class billDiscountvalidDC
    {
        public string WarehouseName { get; set; }
        public string Offer_Name { get; set; }
    }


    public class OfferColumn
    {
        public string ColumnName { get; set; }
        public Type DataType { get; set; }
        public bool IsRequired { get; set; }
        public List<string> RequiredValues { get; set; }
    }




    public class OfferUploderDc
    {
        public string WarehouseIds { get; set; }
        public string OfferName { get; set; }
        public string OfferAppType { get; set; }
        public bool IsActive { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string OfferOn { get; set; }  //Item,Category,Brand ,  
        public bool IsAutoApply { get; set; }
        public long StoreId { get; set; }
        public int CustomerId { get; set; }
        public int? FreeItemLimit { get; set; }
        public bool IsMultiTimeUse { get; set; }
        public bool IsUseOtherOffer { get; set; }
        public int? OfferUseCount { get; set; }
        public string BillDiscountOfferOn { get; set; }
        public string ApplyOn { get; set; }
        public double DiscountPercentage { get; set; }
        public double BillAmount { get; set; }  // Bill Amount       
        public double MaxBillAmount { get; set; }
        public double MaxDiscount { get; set; }
        public int LineItem { get; set; }
        public string Description { get; set; }
        public double? BillDiscountWallet { get; set; }
        public string WalletType { get; set; }
        public string BillDiscountType { get; set; }  // Fill By Code

        public string ApplyType { get; set; }  // Fill By Code
        public int? GroupId { get; set; } //Contain list of CustomerGroupId 
        public int? ExcludeGroupId { get; set; } 
        public long? CombinedGroupId { get; set; }
        public string ChannelIds { get; set; }
        public List<OfferScratchWeightDc> OfferScratchWeights { get; set; }
        public List<OfferLineItemValueDc> OfferLineItemValues { get; set; }
        public List<BillDiscountFreeItemDc> BillDiscountFreeItems { get; set; }

    }

    public class offeruploadItemDC
    {
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string BrandName { get; set; }

        public int SubCategoryMappingId { get; set; }
        public int BrandCategoryMappingId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool IsInclude { get; set; }
    }

    public class offeruploadRequireItemDC
    {
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string BrandName { get; set; }
        public int BrandId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ValueType { get; set; }
        public int ValueAmount { get; set; }
    }

    public class OfferScratchWeightDc
    {
        public int WalletPoint { get; set; }
        public double Weight { get; set; }
    }

    public class OfferLineItemValueDc
    {
        public double itemValue { get; set; }

    }

    public class BillDiscountFreeItemDc
    {
        public int WarehouseId { get; set; }
        public int ItemId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double MRP { get; set; }
        public string ItemName { get; set; }
        public int StockQty { get; set; }
        public int OfferStockQty { get; set; }
        public int Qty { get; set; }
        public int StockType { get; set; } //1-Current Stock 2- Free Stock
        public int RemainingOfferStockQty { get; set; }
    }

    public class GetBrandCategoryMappingsDC
    {
        public string BaseCategoryName { get; set; }
        public string CategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public int SubsubcategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public int SubCategoryMappingId { get; set; }
        public int BrandCategoryMappingId { get; set; }
    }


    public class OfferminDc
    {
        public int OfferId { get; set; }
        public int WarehouseId { get; set; }

        public string BillDiscountType { get; set; }
    }
    public class IncludeSection
    {
        public string OfferOn { get; set; }
        public bool IsInclude { get; set; }
    }

    public class ExcludeSection
    {
        public string ExcludeItemName { get; set; }
        public bool IsInclude { get; set; }
    }


    public class OfferSectionDetail
    {
        public string OfferType { get; set; }
        public List<IncludeSection> IncludeSections { get; set; }
        public List<ExcludeSection> ExcludeSections { get; set; }
        public List<Masters.BillDiscountRequiredItemDc> BillDiscountRequiredItemDcs { get; set; }
    }

    public class SearchOfferForCRMDc
    {
        public string Keyword { get; set; }

    }
    public class SearchNoticationForCRMDc
    {
        public string Keyword { get; set; }

    }

    public class OfferForCRMDc
    {
        public string OfferCode { get; set; }
        public string OfferName { get; set; }
        public string Description { get; set; }
    }


    public class NotificationForCRMDc
    {
        public int Id { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationDesc { get; set; }
    }

    public class AddCRMOfferDc
    {
        public string GroupName { get; set; }
        public string GroupValue { get; set; }
        public string OfferType { get; set; }  //on value or on Percent
        public double OfferValue { get; set; } // Value or Percent value
        public string OfferCode { get; set; }
    }

    public class CustomerCRMSearchDc
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public int Warehouseid { get; set; }
    }
    public class CustomerCRMOfferDc
    {
        public List<CRMSkCodesDC> Skcodes { get; set; }
        public string OfferCode { get; set; }

    }

    public class CRMSkCodesDC
    {
        public string SkCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CustomerCRMNotificationDc
    {
        public List<string> Skcodes { get; set; }
        public int NotificationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        //public string NotificationDesc { get; set; }
        //public string NotificationTitle { get; set; }
        //public string GroupName { get; set; }
        //public string GroupValue { get; set; }
        //public DateTime StartDate { get; set; }
        //public DateTime EndDate { get; set; }
    }

    public class CustomerCRMWhatsappNotificationDc
    {
        public List<string> Skcodes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long? WhatsappTemplateId { get; set; }
    }

    public class FreeItemStockDc
    {
        public int Stock { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string StockType { get; set; }
    }

    public class offerItemMRP
    {
        public int ItemMultiMRPId { get; set; }
        public string ItemNumber { get; set; }
    }
}

