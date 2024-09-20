using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.ForCast
{
    public class ItemforcastEdit
    {
        public int TotalRecord { get; set;}
        public int AvgInventoryDays { get; set; }
        public double BrandTotalValue { get; set; }//Brand Target Value
        public double BrandSalesValue { get; set; } //changes

        public double BrandTotalPercent { get; set; }
        public int TodayDay { get; set; }
        public List<ItemForeCastResponse> ItemForeCastResponses { get; set; }
        public int TotalLiveInventory { get; set; }  // New Addition for Live Inventory
    }
    public class ItemForeCastResponse
    {
        public long Id { get; set; }
        public string SubsubcategoryName { get; set; }
        public string ItemName { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int Mnth { get; set; }
        public int SystemSuggestedQty { get; set; }
        public int GrowthForecastQty { get; set; }
        public double PercentValue { get; set; }
        public double GrowthInAmount { get; set; }  //BrandAmount
        public int BuyerEditForecast { get; set; }
        public int BuyerPDForecast { get; set; }
        public int SalesIntent { get; set; }
        public int TillYesterdayDemand { get; set; }//DemandQty
        public int CurrentInventory { get; set; }
        public int CalculatedInventoryDays { get; set; }
        public int InventoryDays { get; set; }
        public int FulFillQty { get; set; }
        public int OPQty { get; set; }
        public int CreatedBy { get; set; }
        public double LastDemandValue { get; set; } //LastDemandValue
        public double ASP { get; set; } //Average Selling Price

        public string Category { get; set; } // Addition for ABC Classification
        public int? Tag { get; set; } //ROC Addition

        public int? SafetyDays  { get; set; } // SafetyDays addition
        public int? Safetystockqty { get; set; }//SafetyDays addition


    }

    public class ItemforcastPOEdit
    {
        public int TotalRecord { get; set; }
        public int SaveasdraftCount { get; set; }
        public List<ItemForeCastPOResponse> ItemForeCastPOResponses { get; set; }
    }

    public class ItemForeCastPOResponse
    {
        public long Id { get; set; }
        public string SubsubcategoryName { get; set; }
        public string ItemName { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int RequiredQty { get; set; }
        public int YesterdayDemand { get; set; }
        public int SalesIntent { get; set; }
        public int CurrentStock { get; set; }
        public int NetStock { get; set; }
        public double MRP { get; set; }
        public double AveragePurchasePrice { get; set; }
        public List<OtherWarehouseDetails> OtherWarehouseDetails { get; set; }
        public int? FulfillThrow { get; set; }  // 0-Purchase Request, 1- Inernal Transfer        
        public double? BuyingPrice { get; set; }
        public int? SupplierId { get; set; }
        public string PRPaymentType { get; set; } // AdvancePR, CreditPR
        public int? MinOrderQty { get; set; }
        public int? NoOfSet { get; set; }
        public string Comments { get; set; } //changes for new requirment at 03-01-2023
        public string DisplayName { get; set; }
        public DateTime? PrDate { get; set; }
        public int? CreatedBy { get; set; }
        public long? PRId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int OPenQty { get; set; } // New Add
        public int? Tag { get; set; } 


    }
    public class ItemforcastfullfillmetExport
    {
        public int TotalRecord { get; set; }
        public int SaveasdraftCount { get; set; }
        public List<ItemForeCastfullfillmetExport> ItemForeCastPOResponses { get; set; }
    }
    public class ItemForeCastfullfillmetExport
    {
        
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }

        public string BrandName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double MRP { get; set; }
        public double AveragePurchasePrice { get; set; }
        public int? PurchaseMOQ { get; set; }
        public int? NoOfSet { get; set; }
        public int? ExecutedQty { get; set; }
        public int CurrentStock { get; set; }
        public int AllowedQty { get; set; }
        public int NetStock { get; set; }
        public int YesterdayDemand { get; set; }
        public DateTime? PrDate { get; set; }
        public long? PRId { get; set; }
        public string CatalogerName { get; set; }
        public string PickerType { get; set; }
        public DateTime? ETADate { get; set; }
        public string Supplier { get; set; }
        public string DepoName { get; set; }
        public string PRPaymentType { get; set; }

        public string BuyerName { get; set; }
        public string Comments { get; set; }


    }

    public class ItemforcastPOEditExport
    {
        //public int TotalRecord { get; set; }
        public List<ItemForeCastPOResponseExport> ItemForeCastPOResponses { get; set; }
    }

    public class ItemForeCastPOResponseExport
    {
       
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }

        public string BrandName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double MRP { get; set; }
        public double AveragePurchasePrice { get; set; }
        public int? MinOrderQty { get; set; }
        public int? NoOfSet { get; set; }
        public int? ExecutedQty { get; set; }
        public int CurrentStock { get; set; }
        public int AllowedQty { get; set; }
        public int NetStock { get; set; }
        public int YesterdayDemand { get; set; }
        public DateTime? PrDate { get; set; }
        public long PRId { get; set; }
        public string CatalogerName { get; set; }
        public string PickerType { get; set; }
        public DateTime? ETADate { get; set; }
        public string Supplier { get; set; }
        public string DepoName { get; set; }
        public string PRPaymentType { get; set; }

        public string BuyerName { get; set; }
        public string Comments { get; set; }



    }
    public class ItemForeCastPODataExcel
    {
        public long Id { get; set; }
        public string WarehouseName { get; set; }
        public string ItemName { get; set; }

        public string BrandName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public double MRP { get; set; }
        public double AveragePurchasePrice { get; set; }
        public int? PurchaseMOQ { get; set; }
        public int? NoOfSet { get; set; }
        public int? ExecutedQty { get; set; }
        public int CurrentStock { get; set; }
        public int AllowedQty { get; set; }
        public int NetStock { get; set; }
        public int YesterdayDemand { get; set; }
        public DateTime? PrDate { get; set; }
        public long? PRId { get; set; }
        public string CatalogerName { get; set; }
        public string PickerType { get; set; }
        public DateTime? ETADate { get; set; }
        public string  Supplier { get; set; }
        public string  DepoName { get; set; }
        public string PRPaymentType { get; set; }

        public string  BuyerName { get; set; }
        public string Comments { get; set; }

    }



    public class ItemForeCastPOData
    {
        public long Id { get; set; }
        public string SubsubcategoryName { get; set; }
        public string ItemName { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int RequiredQty { get; set; }
        public int YesterdayDemand { get; set; }
        public int CurrentStock { get; set; }
        public int NetStock { get; set; }
        public double MRP { get; set; }
        public double AveragePurchasePrice { get; set; }
        public int OtherWhStock { get; set; }
        public int OtherWhDemand { get; set; }
        public int OtherWhOpenPoQty { get; set; }
        public int OtherWhDelCancel { get; set; }
        public int OtherWhNetDemand { get; set; }
        public int OtherWarehouseId { get; set; }
        public int? otherWhReqQty { get; set; } //new 
        public string OtherWarehouseNM { get; set; }
        public int? FulfillThrow { get; set; }  // 0-Purchase Request, 1- Inernal Transfer        
        public double? BuyingPrice { get; set; }
        public int? SupplierId { get; set; }
        public string PRPaymentType { get; set; } // AdvancePR, CreditPR
        public int? MinOrderQty { get; set; }
        public int? NoOfSet { get; set; }
        public int SalesIntent { get; set; }
        public string Comments { get; set; } //changes for new requirment at 03-01-2023
        public string DisplayName { get; set; }
        public DateTime? PrDate { get; set; }

        public int? CreatedBy { get; set; }

        public long? PRId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int OPenQty { get; set; }




    }

    public class OtherWarehouseDetails
    {
        public int OtherWhStock { get; set; }
        public int OtherWhDemand { get; set; } //OtherWhDemand
        public int OtherWhOpenPoQty { get; set; }
        public int OtherWhDelCancel { get; set; }
        public int OtherWhNetDemand { get; set; }
        public int OtherWarehouseId { get; set; }
        public string OtherWarehouseName { get; set; }
        public int? otherWhReqQty { get; set; }
    }

    public class ItemforecastRequiredData
    {
        public List<int> MOQLst { get; set; }
        public List<supplierMinDc> SupplierLst { get; set; }

        public double POPurchasePrice { get; set; } //New Change
        public double PurchasePrice { get; set; }
    }

    public class supplierMinDc
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int Expirydays { get; set; }

        public string bussinessType { get; set; }

    }
    public class ItemforecastRequiredDataForBussinessType
    {
        public List<int> MOQLst { get; set; }
        public List<supplierMin> SupplierLst { get; set; }

        public double POPurchasePrice { get; set; } //New Change
        public double PurchasePrice { get; set; }
    }

    public class supplierMin
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string bussinessType { get; set; }
    }

    public class BuyerApproveItemForeCastData
    {
        public int TotalRecord { get; set; }
        public List<BuyerApproveItemForeCast> BuyerApproveItemForeCasts { get; set; }
    }
    public class BuyerApproveItemForeCastDataExport
    {
        public int TotalRecord { get; set; }
        public List<BuyerApproveItemForeCastExport> BuyerApproveItemForeCasts { get; set; }
    }


    public class BuyerApproveItemForeCast
    {
        public long Id { get; set; }
        public long? fulfillmentId { get; set; }
        public string SubsubcategoryName { get; set; }
        public string ItemName { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }
        public string InternalTransferWHName { get; set; }
        public int? InternalTransferWHId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int RequiredQty { get; set; }
        public int YesterdayDemand { get; set; }
        public int CurrentStock { get; set; }

      
        public int NetStock { get; set; }  //it shows as CurrentNetStock & fetch from  Netdemand
        public double MRP { get; set; }
        public double AveragePurchasePrice { get; set; }

        public int? FulfillThrow { get; set; }  // 0-Purchase Request, 1- Inernal Transfer        
        public double? BuyingPrice { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PRPaymentType { get; set; } // AdvancePR, CreditPR
        public int? MinOrderQty { get; set; }
        public int? NoOfSet { get; set; }

        public double? FreightCharge { get; set; }  //add at 6 jan for Freightcharge
        public string WeightType { get; set; }
        public double? Weight { get; set; }
        public string ItemNumber { get; set; }
        public string BuyerName { get; set; }// add for Buyer Name
        
        public int? BuyerId { get; set; }
        
        public int? DepoId { get; set; }
        public string DepoName { get; set; }

        public string bussinessType { get; set; }

    }

    public class BuyerApproveItemForeCastExport
    {
        public long Id { get; set; }
        //public long? fulfillmentId { get; set; }
        public string BrandName { get; set; }
        public string ProductName { get; set; }
        public string WarehouseName { get; set; }
        //public int WarehouseId { get; set; }
       // public string InternalTransferWHName { get; set; }
        //public int? InternalTransferWHId { get; set; }
       // public int CityId { get; set; }
      //  public string CityName { get; set; }
        public int itemmultimrpid { get; set; }

        public string ItemNumber { get; set; }
        public int CurrentStock { get; set; }
        public double AveragePurchasePrice { get; set; }
        public int RequiredQty { get; set; }
        public int Demand { get; set; }
        public double? BuyingPrice { get; set; }
        public string BuyerName { get; set; }

        public string PRPaymentType { get; set; }
        public string FulfillmentType { get; set; }
        public string DepoName { get; set; }
        public string CatalogerName { get; set; }
        public int? NoOfSet { get; set; }
        public int? MinOrderQty { get; set; }
        public double? Weight { get; set; }
        public string WeightType { get; set; }
        public DateTime CreatedDate { get; set; }//new

        public long PRID { get; set; }
        public double? FreightCharge { get; set; }
        public string Supplier { get; set; }
        public string UpdatedSupplier { get; set; }
        public string UpdatedDepo { get; set; }
        public int FinalmodifiedQty { get; set; }

    }
    public class ItemForeCastRequest
    {
        public List<int> cityIds { get; set; }
        public List<int> warehouseIds { get; set; }
        public List<int> categoriesIds { get; set; }
        public List<int> subCategoriesIds { get; set; }
        public List<int> subSubCategoriesIds { get; set; }
        public List<int> supplierIds { get; set; }
        
        public string itemname { get; set; } //search by item criteria
        public int fulfillthrowId { get; set; }
        public string prType { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }
    public class ItemForeCastRequestExport
    {
        public List<int> cityIds { get; set; }
        public List<int> warehouseIds { get; set; }
        public List<int> categoriesIds { get; set; }
        public List<int> subCategoriesIds { get; set; }
        public List<int> subSubCategoriesIds { get; set; }
        public List<int> supplierIds { get; set; }
        public string NetStock { get; set; }
        public int fulfillthrowId { get; set; }
        public string prType { get; set; }
       // public int skip { get; set; }
       // public int take { get; set; }
    }

    public class PurchaseItemForeCastRequest
    {
        public List<int> cityIds { get; set; }
        public List<int> warehouseIds { get; set; }
        //public List<int> categoriesIds { get; set; }
        //public List<int> subCategoriesIds { get; set; }
        //public List<int> subSubCategoriesIds { get; set; }
        public List<int> supplierIds { get; set; }
        public int fulfillthrowId { get; set; }
        public string prType { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public string NetStock { get; set; }
    }
    public class PurchaseItemForeCastRequestNew
    {
        public List<int> cityIds { get; set; }
        public List<int> warehouseIds { get; set; }
        //public List<int> categoriesIds { get; set; }
        //public List<int> subCategoriesIds { get; set; }
        //public List<int> subSubCategoriesIds { get; set; }
        public List<int> supplierIds { get; set; }
        public int fulfillthrowId { get; set; }
        public string prType { get; set; }
        public string NetStock { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class ItemFullfillmentRequest
    {
        public List<int> cityIds { get; set; }
        public List<int> warehouseIds { get; set; }
        public List<int> categoriesIds { get; set; }
        public List<int> subCategoriesIds { get; set; }
        public List<int> subSubCategoriesIds { get; set; }
        public List<int> supplierIds { get; set; }
        public string NetStock { get; set; }
        public int fulfillthrowId { get; set; }
        public string prType { get; set; }
        public string SearchItem { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }


    public class ItemForeCastHisResponse
    {
        public string BuyerName { get; set; }
        public string Subsubcategoryname { get; set; }
        public string ItemName { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public int ItemMultiMrpId { get; set; }
        public int month { get; set; }
        public int Year { get; set; }
        public double TotalAmount { get; set; }
        public int TotalQty { get; set; }
    }

    public class ItemForeCastUpdateResponse
    {
        public bool Status { get; set; }
        public string msg { get; set; }
    }


    public class SaveasDraftRequest
    {
        public List<SaveasDraftBulkDc> ItemForecastPRRequestForBulkobj { get; set; }
    }

    public class ItemForecastPRRequestForBulkDc
    {
        public List<ItemForecastPRRequestBulkDc> ItemForecastPRRequestForBulkobj { get; set; }
    }
    public class SaveasDraftPRRequestBulkDc
    {
        //New 
        public long ItemForecastDetailId { get; set; }
        public int FulfillThrow { get; set; }  // 0-Purchase Request, 1- Inernal Transfer        
        public double? BuyingPrice { get; set; }
        public int? SupplierId { get; set; }
        public string PRPaymentType { get; set; } // AdvancePR, CreditPR
        public int MinOrderQty { get; set; }
        public int NoOfSet { get; set; }
        public int? InternalTransferWHId { get; set; }
        public int SalesIntentQty { get; set; }
        public int Demand { get; set; }
        public DateTime? ETADate { get; set; }
        public string PickerType { get; set; }
        public int DepoId { get; set; }
        public int PeopleID { get; set; } //changes for add buyername
        public double? FreightCharge { get; set; }
        public string ItemName { get; set; }
        public double? APP { get; set; }

        public int? YesterdayDemand { get; set; } //for keeping report
        public int? RequiredQty { get; set; }//for keeping report
        public int? OPenQty { get; set; } //for keeping report

    }

    public class ItemForecastPRRequestDc
    {

        // Old 
        public long ItemForecastDetailId { get; set; }
        public int FulfillThrow { get; set; }  // 0-Purchase Request, 1- Inernal Transfer        
        public double? BuyingPrice { get; set; }
        public int? SupplierId { get; set; }
        public string PRPaymentType { get; set; } // AdvancePR, CreditPR
        public int MinOrderQty { get; set; }
        public int NoOfSet { get; set; }
        public int? InternalTransferWHId { get; set; }
        public int SalesIntentQty { get; set; }
        public int Demand { get; set; }
        public DateTime? ETADate { get; set; }
        public string PickerType { get; set; }
        public int DepoId { get; set; }
        public int PeopleID { get; set; }  // new addition
        public double? FreightCharge { get; set; }  // new addition
        public string bussinessType { get; set; } ////add by Priyanka
        public int? Warehouseid { get; set; }  //21 feb
        public long? Id { get; set; } //21 feb

        public double? Demandcases { get; set; } //new 13 feb
        public int? AllowedQtyOtherHub { get; set; }//new 13 feb
        public int? AllowedQty { get; set; }//new 15 feb
        public string Itemname { get; set; } //21 feb

    }

   

    public class ItemForecastPRRequestBulkDc
    {
        //New 
        public long ItemForecastDetailId { get; set; }
        public int FulfillThrow { get; set; }  // 0-Purchase Request, 1- Inernal Transfer        
        public double? BuyingPrice { get; set; }
        public int? SupplierId { get; set; }
        public string PRPaymentType { get; set; } // AdvancePR, CreditPR
        public int MinOrderQty { get; set; }
        public int NoOfSet { get; set; }
        public int? InternalTransferWHId { get; set; }
        public int SalesIntentQty { get; set; }
        public int Demand { get; set; }
        public DateTime? ETADate { get; set; }
        public string PickerType { get; set; }
        public int DepoId { get; set; }
        public int PeopleID { get; set; } //changes for add buyername
        public double? FreightCharge { get; set; }

        public int? Warehouseid { get; set; }
        public long? Id { get; set; }

        public double? Demandcases { get; set; } //new 13 feb
        public int? AllowedQtyOtherHub { get; set; }//new 13 feb
        public int? AllowedQty { get; set; }//new 15 feb
        public string Itemname { get; set; }
        public string bussinessType { get; set; } ////add by Priyanka


        public int? YesterdayDemand { get; set; } //for keeping report
        public int? RequiredQty { get; set; }//for keeping report
        public int? OPenQty { get; set; } //for keeping report
        public int? SubCategoryId { get; set; }
        public int? SubsubCategoryId { get; set; }
        public int MultiMrpId { get;set; }


    }

    public class SaveasDraftBulkDc
    {
        //New 
        public long ItemForecastDetailId { get; set; }
        public int FulfillThrow { get; set; }  // 0-Purchase Request, 1- Inernal Transfer        
        public double? BuyingPrice { get; set; }
        public int? SupplierId { get; set; }
        public string PRPaymentType { get; set; } // AdvancePR, CreditPR
        public int MinOrderQty { get; set; }
        public int NoOfSet { get; set; }
        public int? InternalTransferWHId { get; set; }
        public int SalesIntentQty { get; set; }
        public int Demand { get; set; }
        public DateTime? ETADate { get; set; }
        public string PickerType { get; set; }
        public int DepoId { get; set; }
        public int PeopleID { get; set; } //changes for add buyername
        public double? FreightCharge { get; set; }
        public double? Demandcases { get; set; } //13 feb
        public int? AllowedQtyOtherHub { get; set; } // 13 feb
        public int FinalQty { get; set; }
        public int WarehouseId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }

        public int? AllowedQty { get; set; }
        public string ItemName { get; set; }
        public double? APP { get; set; }
        public int ItemMultiMRPId { get; set; }//16 feb new
        public long? Id { get; set; }
        public string bussinessType { get; set; }// add by Priyanka

        public int? YesterdayDemand { get; set; } //for keeping report
        public int? RequiredQty { get; set; }//for keeping report
        public int? OPenQty { get; set; } //for keeping report
       
    }



    public class ItemForSystemForecast
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int OrderQty { get; set; }
        public DateTime monthDate { get; set; }
    }

    public class SystemForecast
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double Alpha { get; set; }
        public double ForeCastQty { get; set; }
        public double Map { get; set; }
        public List<AlphaCalculation> AlphaCalculations { get; set; }
    }

    public class AlphaCalculation
    {
        public double Alpha { get; set; }
        public double ForeCastQty { get; set; }
        public double Map { get; set; }
    }


    public class TblSystemItemForecast
    {
        public long Id { get; set; }
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double Alpha { get; set; }
        public double ForeCastQty { get; set; }
        public double Map { get; set; }
    }

    public class FutureForcastItemDC

    {
        // public int SubsubCategoryid { get; set; }
        public string Number { get; set; }
        public string ItemName { get; set; }

    }
    public class FutureForcastMappingDC

    {
        public bool Sel { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int MRP { get; set; }
        public string itemname { get; set; }

    }
    public class FutureForeCastAdd
    {
        public int ItemMultiMRPId { get; set; }
        public int WarehouseId { get; set; }
        public List<int> MappingMRPId { get; set; }


    }
    public class ForeCastUpdateResponse
    {

        public string msg { get; set; }
    }
    public class CreatePoOrInternalDc
    {
        public List<long> Ids { set; get; }
    }

    public class CreatePoOrInternalRes
    {
        public bool Status { set; get; }
        public string Message { set; get; }
    }

    public class ItemsToCreatePoOrInternalDc
    {
        public long Id { set; get; }  // ItemForecastPRRequestsId
        public int SupplierId { set; get; }
        public int DepoId { set; get; }
        public int PurchaseMinOrderQty { set; get; }
        public int FinalQty { set; get; }
        public int ItemMultiMRPId { set; get; }
        public int WarehouseId { set; get; }
        public string WarehouseName { set; get; }
        public int InternalTransferWHId { set; get; }
        public int Itemid { set; get; }
        public double? BuyingPrice { set; get; }
        public double MRP { set; get; }
        public string ItemName { set; get; }
        public string PurchaseUnitName { set; get; }
        public string itemBaseName { set; get; }
        public string HSNCode { set; get; }
        public string ItemNumber { set; get; }
        public string SellingSku { set; get; }
        public string PurchaseSku { set; get; }
        public string PRPaymentType { set; get; }
        public int? BuyerId { get; set; }
        public double TotalTaxPercentage { set; get; }
        public double? NetPurchasePrice { set; get; }
        public string PickerType { get; set; }
        public DateTime? ETADate { get; set; }
        public string bussinessType { get; set; }

    }

    public class OrderItemForIntent
    {
        public int ClusterId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int qty { get; set; }
        public int orderid { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class MapMrpId
    {
        public int ItemMultiMrpId { get; set; }
        public List<int> MapMrpIds { get; set; }
    }

    public class futureMrpResponse
    {
        public List<FutureForcastMappingDC> FutureForcastMappingDCs { get; set; }
        public List<MapMrpId> MapMrpIds { get; set; }
    }


    public class GetForcastCategorybyPeopleDc
    {
        public int BuyerId { get; set; }
        public List<int> warehouseIds { get; set; }
       public string RoleName { get; set; }

    }
    public class ForcastCategoryDc
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
    }

    public class ItemForecastInventoryDays
    {
        //public List<int> cityIds { get; set; }
        public List<int> warehouseIds { get; set; }
        //public List<int> categoriesIds { get; set; }
        //public List<int> subCategoriesIds { get; set; }
        //public List<int> subSubCategoriesIds { get; set; }

        public List<int> StoreIds { get; set; }
        public string BrandName { get; set; }
        // public List<int> supplierIds { get; set; }
        // public int fulfillthrowId { get; set; }
        // public string prType { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class ItemForecastInventoryDaysEdit
    {
        public int Total_Record { get; set; }
        public List<ItemForecastInventoryDaysResponse> ItemForecastInventoryDaysResponses { get; set; } // ItemForecastInventoryDaysResponses
    }
    public class ItemForecastInventoryDaysResponse
    {

        // public string CityName { get; set; }
        public long Id { get; set; }
        public long FId { get; set; }
        public string CateName { get; set; }
        public string BrandName { get; set; } 
        public string WarehouseName { get; set; }
        public int InventoryDays { get; set; }
        public string SubCateName { get; set; }
        public string StoreName { get; set; }
        public int Total_Record { get; set; }
        public double CalculateInventoryDays { get; set; }
        public int WarehouseId { get; set; }
        public long storeid { get; set; }
        public int subcatid { get; set; }
        public int subsubcatid { get; set; }
        public int cid { get; set; }
        public int? SafetyDays { get; set; }



        // public int TotalRecord { get; set; }
    }

    public class Groupnames
    {
        public string Name { get; set; }
        public long Id { get; set; }
    }



}
