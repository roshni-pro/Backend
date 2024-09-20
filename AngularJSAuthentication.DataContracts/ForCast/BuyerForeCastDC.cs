using AngularJSAuthentication.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ForCast
{
    public class BuyerForecastdataDC
    {
        public long UploadID { get; set; }
        public string UploadedBy { get; set; }
        public string Status { get; set; }
        public DateTime uploadDate { get; set; }
        public string ValidationStatus { get; set; }
        public string GroupName { get; set; }

    }

    
    public class DownloadeBrandSummaryFileDC
    {
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string BrandName { get; set; }
        public decimal ValueInAmt { get; set; }
        public int userid { get; set; }
    }

    public class BuyerForeCastDC
    {
        public long  UplodeId { get; set; }
        public string UploadBy { get; set; }
        public string Category { get; set; }
        public string BrandName { get; set; }
        public string WarehouseName { get; set; }

        public string City { get; set; }
        public DateTime Month { get; set; }

        public double PercentValue { get; set; }

        public string Value { get; set; }

        public string ErrorMsg { get; set; }
        public int TotRec { get; set; }

        public string GroupName { get; set; }
    }

    public class BuyerForeCastDCExport
    {
        public string GroupName { get; set; }
        public long UplodeId { get; set; }
        public string UploadBy { get; set; }
        public string Category { get; set; }
        public string BrandName { get; set; }
        public string WarehouseName { get; set; }

       // public string City { get; set; }
        public DateTime Date { get; set; }

        public double PercentValue { get; set; }

       // public string Value { get; set; }

        public string ErrorMsg { get; set; }

    }
    public class BuyersForecastUploadedFileDc
    {
        public string Group { get; set; }//changes
        public string Dept { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string WarehouseName { get; set; }
        public double PercentValue { get; set; }
        //public List<BrandWarehouseDc> BrandWarehouseDcs { get; set; }

    }

    public class AddNewArticleUploadedFileDc : EntityDefaultField
    {
        public int ItemMultiMRPId { get; set; }//changes
        public int BuyerEdit{ get; set; }
        public int InventoryDays { get; set; }
        public double MaxSellingPrice { get; set; }
        public string WarehouseName { get; set; }
   
 

    }
    public class ForecastInventoryDaysUploadedFileDc: EntityDefaultField
    {
        
        public string CateName { get; set; }
        public string BrandName { get; set; } //SubCateName
        public string WarehouseName { get; set; }
        public int InventoryDays { get; set; }
        public string SubCateName { get; set; }
        public string StoreName { get; set; } // New Change 28-03
        public double CalculateInventoryDays { get; set; } // New at 20 April
        public int SafetyDays { get; set; } // new For Safety Days

    }

    public class BrandWarehouseDc
    {
        public string WarehouseName { get; set; }
        public double Amount { get; set; }
    }

    public enum ValidationStatus
    {
        Pending = 0,
        InProcess = 1,
        ValidationSuccessful = 2,
        ValidationFail = 3
    }

    public class GetItemSaleCompaireActualVSForecastDc
    {
        public List<int> warehouseIds { get; set; }
        public List<int> categoriesIds { get; set; }
        public List<int> subCategoriesIds { get; set; }
        public List<int> subSubCategoriesIds { get; set; }
        public DateTime? startDate { get; set; }
        public string itemName { get; set; }
        public int skip { get; set; }
       public int take { get; set; }
    }

    public class ItemSaleCompaireActualVSForecastMainList
    {
        public List<ItemSaleCompaireActualVSForecastListDc> ItemSaleCompaireActualVSForecastListDc { get; set; }
        public int totalRecords { get; set; }
    }

    public class ItemSaleCompaireActualVSForecastListDc
    {
        public string SubsubcategoryName { get; set; }
        public string itemname { get; set; }
        public string WarehouseName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int SystemSuggestedQty { get; set; }
        public int GrowthForecastQty { get; set; }
        public Double MaxSellingPrice { get; set; }
        public int salesIntent { get; set; }
        public int BuyerSalesForecast { get; set; }
        public int totalqtySale { get; set; }
        public int totalSale { get; set; }
        public double BuyerF_SystemF { get; set; }

        public double SalesF_ActualF { get; set; }
        public double SystemF_ActualF { get; set; }


    }

    public class GetBrandSaleCompaireActualVSForecastDc
    {
        public List<int> warehouseIds { get; set; }
        public List<int> categoriesIds { get; set; }
        public List<int> subCategoriesIds { get; set; }
        public List<int> subSubCategoriesIds { get; set; }
        public DateTime? startDate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class BrandSaleCompaireActualVSForecastMainList
    {
        public List<BrandSaleCompaireActualVSForecastListDc> brandSaleCompaireActualVSForecastListDcs { get; set; }
        public int totalRecords { get; set; }
    }
    public class BrandSaleCompaireActualVSForecastListDc
    {
        public string SubsubcategoryName { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int SystemSuggestedQty { get; set; }
        public double SystemSuggestedAmount { get; set; }
        public int GrowthForecastQty { get; set; }
        public int salesIntentQty { get; set; }
        public int BuyerSalesForecastQty { get; set; }
        public double BuyerSalesForecastAmount { get; set; }
        public int totalqtySale { get; set; }
        public double totalSale { get; set; }
        public int totalQty { get; set; }
        public double Forcasthit { get; set; }
        public double TeamHit { get; set; }
    }



    public class GetSubcategoryCategoryMappingsDC
    {
        public string BaseCategoryName { get; set; }
        public string CategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public int SubsubcategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int SubCategoryId { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
    }

    public class BuyersEditUploadedFileDc
    {
        public long ID { get; set; }
        public string SubsubcategoryName { get; set; }
        public double PercentValue { get; set; }
        public int BuyerEditForecastQty { get; set; }
        public int InventoryDays { get; set; }
        public int userid { get; set; }
    }

    public class BuyersBrandWisePerValDc
    {
        public string WarehouseName { get; set; }
        public double TotalPercentValue { get; set; }
    }
    public class BuyersGroupWisePerValDc
    {
        public string GroupName { get; set; }
       
    }


    public class BrandSummaryValueInAmtDc
    {
        public string WarehouseName { get; set; }
        public decimal ValueInAmt { get; set; }
    }

    public class HOPGroupHubPlan
    {
        public long WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double PlannedValue { get; set; }
    }


    public enum SalesIntentReqStatus
    {
        PendingForLead = 0,
        PendingForBuyer = 1,
        Rejected = 2,
        Approved = 3,
      //  BrandRestrict=4
    }

    public class GetForcastCityByPeople
    {
        public int Cityid { get; set; }
        public string CityName { get; set; }
    }
    public class InventoryDayBrandWisePerValDc
    {
        public string WarehouseName { get; set; }
       // public double TotalPercentValue { get; set; }
    }

    public class ForcastPeopleMaster
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
    }



    public class GetBrandsDc
    {
        public List<int> categoryId { get; set; }
        public List<int> subcategoryId { get; set; }
    }


    public class InventoryRestrictionResponsesDC
    {
        public List<InventoryRestrictionListDc> InventoryRestrictionList { get; set; }
        public int TotalRecord { get; set; }
    }


    public class InventoryRestrictionListDc
    {
        public long ID { get; set; }
        public string WarehouseName { get; set; }
        public string Store { get; set; }
        public string CateName { get; set; }
        public string SubCateName { get; set; }
        public string BrandName { get; set; }
        public double? CurrentAvgInvDays { get; set; }
        public int NoOfInvDays { get; set; }

//        public DateTime? CreatedDate { get; set; }
    }

    public class InventoryRestrictionUpdateStatusResponse
    {
        public bool Status { get; set; }
        public string msg { get; set; }
    }

    public class SalesIntentValidationDc
    {
        public int BuyerPDForecast { get; set; }
        public int CurrentNetStock { get; set; }
        public int OpenPOQTy { get; set; }
        public int CalculatedQuantity { get; set; }
    }

}