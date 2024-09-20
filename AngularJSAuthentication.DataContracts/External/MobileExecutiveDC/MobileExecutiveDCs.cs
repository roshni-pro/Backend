using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Forecasting;
using GenricEcommers.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External.MobileExecutiveDC
{
    public class SalesAppCounterDc
    {
        public int SalesPersonId { get; set; }
        public double lat { get; set; }
        public double Long { get; set; }
        public string Mobile { get; set; }
        public string PeopleFirstName { get; set; }
        public string PeopleLastName { get; set; }
        public string WarehouseName { get; set; }
        public int WarehouseId { get; set; }

    }
    public class Peopleresponse
    {
        public People people { get; set; }
        public string message { get; set; }
        public bool Status { get; set; }
    }
    public class ExecutiveInfo
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
    }
    public class PeopleLoginDCs
    {
        public PeopleLoginDC people { get; set; }
        public string message { get; set; }
        public bool Status { get; set; }
    }
    public class PeopleLoginDC
    {
        public PeopleLoginDC()
        {
            if (this.DisplayName == null)
            {
                this.DisplayName = PeopleFirstName + " " + PeopleLastName;
            }
        }
        public int PeopleID { get; set; }
        public int CompanyId { get; set; }

        public int WarehouseId { get; set; }

        public string PeopleFirstName { get; set; }

        public string PeopleLastName { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }

        public string Country { get; set; }

        public int? Stateid { get; set; }

        public string state { get; set; }
        public int? Cityid { get; set; }

        public string city { get; set; }

        public string Mobile { get; set; }//new fields 07/09/2015

        public string Password { get; set; }
        public int? RoleId { get; set; }

        public string Department { get; set; }
        public double BillableRate { get; set; }
        public string CostRate { get; set; }
        public string Permissions { get; set; }
        public string SUPPLIERCODES { get; set; }

        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public bool Deleted { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool Approved { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateBy { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public string VehicleNumber { get; set; }
        public double VehicleCapacity { get; set; }
        public string Skcode { get; set; }
        public string AgentCode { get; set; }
        public string Salesexecutivetype { get; set; }

        public decimal AgentAmount { get; set; }

        public string Empcode { get; set; }
        public string Desgination { get; set; }
        public string Status { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? DataOfJoin { get; set; }
        public DateTime? DataOfMarriage { get; set; }
        public DateTime? EndDate { get; set; }
        public string Unit { get; set; }
        public int Salary { get; set; }
        public string Reporting { get; set; }
        public string IfscCode { get; set; }
        public int Account_Number { get; set; }
        public object PeopleId { get; set; }

        public double DepositAmount { get; set; }
        public string DeleteComment { get; set; }

        public string DeviceId { get; set; }
        public string FcmId { get; set; }
        public string CurrentAPKversion { get; set; }
        public string PhoneOSversion { get; set; }
        public string UserDeviceName { get; set; }
        public string AddressProof { get; set; }
        public string IMEI { get; set; }
        public string IdProof { get; set; }
        public string pVerificationCopy { get; set; }

        public string OTP { get; set; }

        public ApkNamePwdResponse RegisteredApk { get; set; }
        public string UserName { get; set; }
        public bool tempdel { get; set; }

        public List<int> id { get; set; }
        public int? ReportPersonId { get; set; }
        public bool? OrangeBookAcceptance { get; set; }

        public string Role { get; set; }
        public bool? IsLocation { get; set; }
        public bool? IsRecording { get; set; }
        public int? LocationTimer { get; set; }
        public bool? IsSellerPortallogin { get; set; }

        public double StartLat { get; set; }
        public double StartLng { get; set; }
        public string ProfilePic { get; set; }

        public string WarehouseName { get; set; }

        public string clusterId { get; set; } //TODO:SalesApp31March2023

        public string clusterName { get; set; }//TODO:SalesApp31March2023

        public string StoreName { get; set; } //TODO:SalesApp31March2023

        public string StoreId { get; set; }//TODO:SalesApp31March2023
        public long ChannelId { get; set; }//TODO:SalesApp31March2023
        public string Channel { get; set; }//TODO:SalesApp31March2023

    }
    public class ApkNamePwdResponse
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class PersonDetail
    {
        public string WarehouseName { get; set; }
        public string Mobile { get; set; }
    }
    public class CustomerAddressDCs
    {
        public string CityPlaceId { get; set; }
        public string AreaPlaceId { get; set; }
        public string AreaText { get; set; }
        public double AreaLat { get; set; }
        public double AreaLng { get; set; }


        public string AddressPlaceId { get; set; }
        public string AddressText { get; set; }
        public double AddressLat { get; set; }
        public double AddressLng { get; set; }

        public string AddressLineOne { get; set; }
        public string AddressLineTwo { get; set; }

        public int CustomerId { get; set; }
        public string ZipCode { get; set; }

        public string CityName { get; set; }
    }

    public class PeopleCityDCs
    {
        public int PeopleId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string CityPlaceId { get; set; }
        public double CityLatitude { get; set; }
        public double CityLongitude { get; set; }

    }
    public class companydetails
    {
        public CompanyDetails CompanyDetails { get; set; }

        public bool Status { get; set; }
        public string Message { get; set; }

    }

    public class CategorySalesAppDc
    {
        public List<BaseCategory> Basecats { get; set; }
        public List<Category> Categories { get; set; }
        public List<SubCategories> SubCategories { get; set; }
        public List<SubSubCategories> SubSubCategories { get; set; }
    }
    public class SubSubCategories
    {
        public int SubSubCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string SubSubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int itemcount { get; set; }
    }
    public class SubCategories
    {
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string SubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int itemcount { get; set; }
    }
    public class customeritem
    {
        public IEnumerable<Basecats> Basecats { get; set; }
        public IEnumerable<Categories> Categories { get; set; }
        public IEnumerable<SubCategories> SubCategories { get; set; }
        public IEnumerable<SubSubCategories> SubSubCategories { get; set; }
    }
    public class Basecats
    {
        public int BaseCategoryId { get; set; }
        public string BaseCategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
    }
    public class Categories
    {
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string LogoUrl { get; set; }
        public string HindiName { get; set; }
        public string CategoryImg { get; set; }
    }
    public class ItemDataDC
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
        public int? OfferCategory
        {
            get; set;
        }
        public DateTime? OfferStartTime
        {
            get; set;
        }
        public DateTime? OfferEndTime
        {
            get; set;
        }
        public double? OfferQtyAvaiable
        {
            get; set;
        }

        public double? OfferQtyConsumed
        {
            get; set;
        }

        public int? OfferId
        {
            get; set;
        }

        public string OfferType
        {
            get; set;
        }

        public double? OfferWalletPoint
        {
            get; set;
        }
        public int? OfferFreeItemId
        {
            get; set;
        }
        public int? FreeItemId
        {
            get; set;
        }


        public double? OfferPercentage
        {
            get; set;
        }

        public string OfferFreeItemName
        {
            get; set;
        }

        public string OfferFreeItemImage
        {
            get; set;
        }
        public int? OfferFreeItemQuantity
        {
            get; set;
        }
        public int? OfferMinimumQty
        {
            get; set;
        }
        public double? FlashDealSpecialPrice
        {
            get; set;
        }
        public int? FlashDealMaxQtyPersonCanTake
        {
            get; set;
        }

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
        public double Rating { get; set; }
        public int Sequence { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int? LastOrderQty { get; set; }
        public int? LastOrderDays { get; set; }
        public float? TotalAmt { get; set; }

        public double? PurchaseValue { get; set; }

        public string Classification { get; set; }
        public string BackgroundRgbColor { get; set; }

        public double? TradePrice { get; set; }
        public double? WholeSalePrice { get; set; }
    }
    public class FlashDealWithItem
    {
        public int FlashDealId { get; set; }
        public int ItemId { get; set; }

    }
    public class WRSITEM
    {
        public List<factoryItemdata> ItemMasters { get; set; }
        public List<factorySubSubCategory> SubsubCategories { get; set; }
        public bool Message { get; set; }

    }
    public class factoryItemdata
    {
        public bool active { get; set; }
        public int ItemId { get; set; }
        public int ItemlimitQty { get; set; }
        public bool IsItemLimit { get; set; }
        public string ItemNumber { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public int DepoId { get; set; }
        public string DepoName { get; set; }
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public string itemname { get; set; }
        public string HindiName { get; set; }
        public double price { get; set; }
        public string SellingUnitName { get; set; }
        public string SellingSku { get; set; }
        public double UnitPrice { get; set; }
        public double VATTax { get; set; }
        public string LogoUrl { get; set; }
        public int MinOrderQty { get; set; }
        public double Discount { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? marginPoint { get; set; }
        public int? promoPerItems { get; set; }
        public int? dreamPoint { get; set; }
        public bool IsOffer { get; set; }//
                                         //by sachin (Date 14-05-2019)
                                         //public bool Isoffer { get; set; }
        public bool IsSensitive { get; set; }//sudhir
        public bool IsSensitiveMRP { get; set; }//sudhir
        public string UnitofQuantity { get; set; }//sudhir
        public string UOM { get; set; }//sudhir
        public string itemBaseName { get; set; }
        public bool Deleted { get; set; }
        public double NetPurchasePrice { get; set; }

        public bool IsFlashDealUsed { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int BillLimitQty { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int? LastOrderQty { get; set; }
        public int? LastOrderDays { get; set; }
        public int? OfferCategory
        {
            get; set;
        }
        public DateTime? OfferStartTime
        {
            get; set;
        }
        public DateTime? OfferEndTime
        {
            get; set;
        }
        public double? OfferQtyAvaiable
        {
            get; set;
        }

        public double? OfferQtyConsumed
        {
            get; set;
        }

        public int? OfferId
        {
            get; set;
        }

        public string OfferType
        {
            get; set;
        }

        public double? OfferWalletPoint
        {
            get; set;
        }
        public int? OfferFreeItemId
        {
            get; set;
        }
        public int? FreeItemId
        {
            get; set;
        }


        public double? OfferPercentage
        {
            get; set;
        }

        public string OfferFreeItemName
        {
            get; set;
        }

        public string OfferFreeItemImage
        {
            get; set;
        }
        public int? OfferFreeItemQuantity
        {
            get; set;
        }
        public int? OfferMinimumQty
        {
            get; set;
        }
        public double? FlashDealSpecialPrice
        {
            get; set;
        }
        public int? FlashDealMaxQtyPersonCanTake
        {
            get; set;
        }

        public double? DistributionPrice { get; set; }
        public bool DistributorShow { get; set; }
        public int ItemAppType { get; set; }
        public bool IsPrimeItem { get; set; }
        public decimal PrimePrice { get; set; }
        public DateTime CurrentStartTime { get; set; }
        public string Scheme { get; set; }
        public double Score { get; set; }

        public string Classification { get; set; }
        public string BackgroundRgbColor { get; set; }
        public string IncentiveClassification { get; set; }
    }

    public class factorySubSubCategory
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public List<factoryItemdata> ItemMasters { get; set; }
    }

    public class RetailerStore
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string Logo { get; set; }
    }
    public class SalesDTO
    {
        public People P { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
   
    public class SalesOrderDetail
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int qty { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public string StoreName { get; set; }
        public double UnitPrice { get; set; }
        public string itemname { get; set; }
        public string Itempic { get; set; }
        public double PTRPrice { get; set; }
        public double PTROrderAmount { get; set; }

        ///not used
        public int OrderDetailsId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public String City { get; set; }
        public string Mobile { get; set; }
        public DateTime OrderDate { get; set; }
        public int? CityId { get; set; }
        public string WarehouseName { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string SubsubcategoryName { get; set; }
        public string SellingSku { get; set; }
        public int? FreeWithParentItemId { get; set; }

        public string SellingUnitName { get; set; }
        public string itemcode { get; set; }
        public string itemNumber { get; set; }
        public string HSNCode { get; set; }
        public string Barcode { get; set; }

        public double price { get; set; }
        public double Purchaseprice { get; set; }

        public int MinOrderQty { get; set; }
        public double MinOrderQtyPrice { get; set; }
        public int Noqty { get; set; }
        public double AmtWithoutTaxDisc { get; set; }
        public double AmtWithoutAfterTaxDisc { get; set; }
        public double TotalAmountAfterTaxDisc { get; set; }

        public double NetAmmount { get; set; }
        public double DiscountPercentage { get; set; }
        public double DiscountAmmount { get; set; }
        public double NetAmtAfterDis { get; set; }
        public double TaxPercentage { get; set; }
        public double TaxAmmount { get; set; }
        public double SGSTTaxPercentage { get; set; }
        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxPercentage { get; set; }
        public double CGSTTaxAmmount { get; set; }
        //for cess
        public double TotalCessPercentage { get; set; }
        public double CessTaxAmount { get; set; }
        public double TotalAmt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
        public string Status { get; set; }
        public double SizePerUnit { get; set; }
        public int? marginPoint { get; set; }
        public int? promoPoint { get; set; }
        public double NetPurchasePrice { get; set; }
        public bool IsFreeItem { get; set; }
        public bool IsDispatchedFreeStock //  Freeitem dispatched from Freestock (true)or currentstock (false)
        {
            get; set;
        }
        public string ABCClassification { get; set; }
        public double CurrentStock { get; set; }
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        //public string status { get; set; }
        public string SupplierName { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double? OrderedTotalAmt { get; set; }
        public bool ISItemLimit { get; set; }
        public int ItemLimitQty { get; set; }
        public double SavingAmount { get; set; }
        public string Category { get; set; }
        public double ActualUnitPrice { get; set; }
        public long StoreId { get; set; }
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string ItemHindiName { get; set; }
        public DateTime? PrioritizedDate { get; set; }
        public double RemainingAmt { get; set; }
        public int OverDueDays { get; set; }
        public bool IsShowPaylaterAmountButton { get; set; }
    }
    public class OrderStatusHistoryDc
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class GetOrderResponseDc
    {
        public StatusCountDc StatusCountDcs { get; set; }
        public StatusCompletedCountDc StatusCompletedCountDcs { get; set; }
        public List<SalesOrder> salesOrders { get; set; }
    }
    public class StatusCountDc
    {
        public int PendingCount { get; set; }
        public int ShippedCount { get; set; }
        public int RedispatchCount { get; set; }
    }
    public class StatusCompletedCountDc
    {
        public int CanceledCount { get; set; }
        public int DeliveredCount { get; set; }
    }
    public class MyOrderItemDC
    {
        public double RemainingAmt { get; set; }
        public int OverDueDays { get; set; }
        public double PTROrderAmount { get; set; }
        public bool IsShowPaylaterAmountButton { get; set; }
        public List<SalesOrderDetail> orderDetails { get; set; }
        public List<OrderStatusHistoryDc> OrderStatusHistoryDcs { get; set; }
    }
    public class OTPDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string OtpNo { get; set; }
        public int? CustomerId { get; set; }
        public string SkCode { get; set; }
        public bool? CanUpdateCustomer { get; set; }
    }
    public class CheckCustomerSalesAppDC
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public int? CustomerId { get; set; }
        public string SkCode { get; set; }
        public bool? CanUpdateCustomer { get; set; }
    }
    public class SalesAppCounterDTO
    {
        public SalesAppCounter MUget { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }

    }
    public class GlobalcustomerDetail
    {
        public List<SalespDTO> customers { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class DayStartParamDC
    {
        public int PeopleId { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string DayStartAddress { get; set; }
    }
    public class UpdateSalesManProfileImageDC
    {
        public int PeopleId { get; set; }
        public string ProfilePic { get; set; }
    }
    public class ClusterStoreExecutiveDC
    {
        public long Id { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Empcode { get; set; }

    }
    public class orderDataDC
    {
        public double sale { get; set; }
        public long OrderCount { get; set; }
        public long OrderCustomerCount { get; set; }
        public double Storesale { get; set; }
        public string StoreName { get; set; }
        public long StoreOrderCount { get; set; }
    }
    public class StoreSalesDc
    {
        public double Storesale { get; set; }
        public string StoreName { get; set; }
        public long StoreOrderCount { get; set; }
    }
    public class WalletRewards
    {
        public Wallet wallet { get; set; }
        public RewardPoint reward { get; set; }
        public CashConversion conversion { get; set; }

    }
    public class CustomernewDC
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string Name { get; set; }
        public int Cityid { get; set; }
        public string City { get; set; }
        public string ShippingAddress { get; set; }
        public double DISTANCE { get; set; }
        public string ShopName { get; set; }
        public string CustomerVerify { get; set; }
        public string StatusSubType { get; set; }

    }
    public class CustomerGrabbedDC
    {
        public Customer cust { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class commissionDC
    {
        public decimal CommissionAmount { get; set; }
        public long Id { get; set; }
    }
    public class CustomerDC
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string UploadProfilePichure { get; set; }
        public string Mobile { get; set; }
        public string Emailid { get; set; }
        public string ShippingAddress { get; set; }
        public string ZipCode { get; set; }
        public string Password { get; set; }
        public int Cityid { get; set; }
        public string Skcode { get; set; }
        public string RefNo { get; set; }
        public string UploadGSTPicture { get; set; }
        public string ShopName { get; set; }
        public string Shopimage { get; set; }
        public string CurrentAPKversion { get; set; }
        public string PhoneOSversion { get; set; }
        public string UserDeviceName { get; set; }
        public string deviceId { get; set; }
        public string imei { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string City { get; set; }
        public string ShippingAddress1 { get; set; }
        public string BillingAddress1 { get; set; }
        public string LandMark { get; set; }
        public string LicenseNumber { get; set; }
        public string UploadRegistration { get; set; }
        // trade 
        public string IfscCode { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string ReferralSkCode { get; set; }
        public bool? IsReferral { get; set; }
        public string fcmId { get; set; }
        public int GrabbedBy { get; set; }
        public string CustomerVerify { get; set; }
        public string StatusSubType { get; set; }
    }
    public class AgentCommissionDc
    {
        public string City { get; set; }
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Name { get; set; }
        public string ShopName { get; set; }
        public string CustomerVerify { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string StatusSubType { get; set; }

    }
    public class SubsubCategoryDTOM
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public string LogoUrl { get; set; }
        public string HindiName { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public string CategoryName { get; set; }
        public int? BaseCategoryId { get; set; }
    }
    public class WarehousesCityDTO
    {
        public string CityName { get; set; }
        public int Cityid { get; set; }
    }
    public class CustomerClusters
    {
        public string Name { get; set; }
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Mobile { get; set; }
        public string ShippingAddress { get; set; }
        public string LandMark { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string CityName { set; get; }
    }
    public class ProductPerformanceFilterDC
    {
        public int clusterId { get; set; }
        public int peopleid { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime Todate { get; set; }
        public int Flag { get; set; }
        public int ObjectId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int WarehouseId { get; set; }
    }
    public class PerformanceDcs
    {
        public int ObjectId { get; set; }
        public string Name { get; set; }
        public int Customers { get; set; }
        public int TotalOrder { get; set; }
        public double Amount { get; set; }
    }
    public class PerformanceDashboardDcs
    {
        public int NewCustomer { get; set; }
        public int VerifiedRetailer { get; set; }
        public int ActiveRetailers { get; set; }
        public double SalesValue { get; set; }
        public double MTDDispatchValue { get; set; }
        public double Cancellation { get; set; }
        public double QoQ { get; set; }
        public double CustomerPercentage { get; set; }
        public double ProductPercentage { get; set; }
    }
    public class ClusterDC
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
    }
    public class OnBaseSalesAppDc
    {
        public List<Category> Categorys { get; set; }
        public List<SubsubCategory> SubsubCategorys { get; set; }
        public List<SubCategory> SubCategorys { get; set; }
    }

    public class orderMrpData
    {
        public DateTime CreatedDate { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int Qty { get; set; }
    }
    public class CompnayWheelConfigDc
    {
        public int LineItemCount { get; set; }
        public decimal OrderAmount { get; set; }
        public bool IsKPPRequiredWheel { get; set; }
    }
    public class CustomerChequeAccepted
    {
        public bool IsChequeAccepted { get; set; }
        public string msg { get; set; }
        public double ChequeLimit { get; set; }
    }
    public class OfferdataDc
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<OfferDc> offer { get; set; }
    }
    public class OfferDc
    {
        public int OfferId { get; set; }
        public string OfferName { get; set; }
        public string OfferCode { get; set; }
        public string OfferCategory { get; set; }
        public string OfferOn { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public int? OfferUseCount { get; set; }
        public double DiscountPercentage
        {
            get; set;
        }
        public double BillAmount
        {
            get; set;
        }

        public int LineItem { get; set; }



        public string Description { get; set; }


        public string BillDiscountOfferOn
        {
            get; set;
        }
        public double? BillDiscountWallet
        {
            get; set;
        }

        public bool IsScratchBDCode { get; set; } //if not scratch or done

        public string BillDiscountType { get; set; }
        public string ColorCode { get; set; }
        public string ImagePath { get; set; }
        public bool IsMultiTimeUse { get; set; }
        public bool IsUseOtherOffer { get; set; }
        public string OfferAppType { get; set; }
        public string ApplyOn { get; set; }
        public string ApplyType { get; set; }
        public string WalletType { get; set; }
        public double MaxDiscount { get; set; }
        public double MaxBillAmount { get; set; }
        public bool IsBillDiscountFreebiesItem { get; set; }
        public bool IsBillDiscountFreebiesValue { get; set; }
        public string offeritemname { get; set; }

        public int offerminorderquantity { get; set; }

        public List<OfferBillDiscountItemDc> OfferBillDiscountItems { get; set; }

        public List<OfferItemdc> OfferItems { get; set; }

        public List<RetailerBillDiscountFreeItemDc> RetailerBillDiscountFreeItemDcs { get; set; }
        public List<BillDiscountRequiredItemDc> BillDiscountRequiredItems { get; set; }
        public List<OfferLineItemValueDc> OfferLineItemValueDcs { get; set; }
    }

    public class OfferItemdc
    {
        public int itemId { get; set; }
        public bool IsInclude { get; set; }
    }
    public class RetailerBillDiscountFreeItemDc
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Qty { get; set; }
    }
    public class BillDiscountRequiredItemDc
    {
        public int offerId { get; set; }
        public string ObjectType { get; set; } //Item, category, subCategory, brand       
        public string ObjectId { get; set; }//item multimrpids,or ids
        public string ObjectText { get; set; }
        public string ValueType { get; set; }  //Qty, TotalAmount
        public int ObjectValue { get; set; }
        public int SubCategoryId { get; set; }
        public int CategoryId { get; set; }
    }
    public class OfferLineItemValueDc
    {
        public long Id { get; set; }
        public int offerId { get; set; }
        public double itemValue { get; set; }
    }

    public class OfferItemDc
    {
        public int OfferId { get; set; }
        public int itemId { get; set; }
        public bool IsInclude { get; set; }
    }
    public class ResScratchDTO
    {
        public OfferBDScratchDTO ScratchBillDiscount { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class OfferBDScratchDTO
    {

        public int OfferId { get; set; }
        public int WarehouseId { get; set; }
        public int CustomerId { get; set; }
        public int PeopleId { get; set; }
        public string OfferName { get; set; }
        public string OfferCode { get; set; }
        public string OfferCategory
        {
            get; set;
        }
        public string OfferOn { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public double DiscountPercentage
        {
            get; set;
        }
        public double BillAmount
        {
            get; set;
        }
        public double MaxBillAmount
        {
            get; set;
        }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string BillDiscountOfferOn
        {
            get; set;
        }
        public double? BillDiscountWallet
        {
            get; set;
        }
        public bool IsMultiTimeUse { get; set; }
        public bool IsUseOtherOffer { get; set; }

        public bool IsScratchBDCode { get; set; }//if not scratch or done
        public string OfferAppType { get; set; }
        public string ApplyType { get; set; }

    }
    public class SalesAppRouteParam
    {
        public int PeopleId { get; set; }
        public int CustomerId { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        public string CurrentAddress { get; set; }
        public double Distance { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Comment { get; set; }
        public string ShopCloseImage { get; set; }
        public bool IsEnd { get; set; }
        public bool IsBeat { get; set; }
    }
    public class SalesAppDefaultCustomersDC
    {
        public int DefaultSalesSCcustomerId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class MainDashboardDCs
    {
        public TodaySaleDC TodaySaleDcs { get; set; }
        public MTDSaleDC MTDSaleDcs { get; set; }
        public YesterdaySaleDCs YesterdaySaleDCs { get; set; }
    }
    public class TodaySaleDC
    {
        public double TodaySales { get; set; }
        public double TodayTargetSales { get; set; }
        public int TodayTotalVisitBeatCustomer { get; set; }
        public int TodayTotalBeatCustomer { get; set; }
    }
    public class MTDSaleDC
    {
        public double MTDSales { get; set; }
        public double MTDTargetSales { get; set; }
        public int MTDTotalVisitBeatCustomer { get; set; }
        public int MTDTotalBeatCustomer { get; set; }
    }
    public class YesterdaySaleDCs
    {
        public double YesterdaySales { get; set; }
        public double YesterdayTargetSales { get; set; }
        public int YesterdayTotalVisitBeatCustomer { get; set; }
        public int YesterdayTotalBeatCustomer { get; set; }
    }
    public class DashboardDc
    {
        public TodayDashboardDc TodayDashboardDcs { get; set; }
        public MTDDashboardDc MTDDashboardDcs { get; set; }
        public YesterdayDashboardDc YesterdayDashboardDcs { get; set; }
    }
    public class ReturnDashboardDc
    {
        public TodayDashboardDc TodayDashboardDcs { get; set; }
        public AttendenceDetailDC AttendenceDetailDCs { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public class TodayDashboardDc
    {
        public double TodayBeatSales { get; set; }
        public int TodayBeatOrder { get; set; }
        public double TodayExtraSales { get; set; }
        public double TodayTotalSales { get; set; }
        public int TodayProductiveCall { get; set; }
        public int TodayVisitPlanned { get; set; }
        public int TodayTotalCall { get; set; }
        public double TodayOutletCoverage { get; set; }
        public double TodayStrikeRate { get; set; }
        public int TodayExtraVisit { get; set; }
        public int TodayExtraCall { get; set; }
        public int TodayExtraOrder { get; set; }
        public int TodaySupperOrder { get; set; }
        public int TodayPerfactOrder { get; set; }
        public int PhoneOrder { get; set; }
        public int Eco { get; set; }
        public double AvgOrderValue { get; set; }
        public double AvgLineItem { get; set; }
    }
    public class MTDDashboardDc
    {
        public double MTDBeatSales { get; set; }
        public int MTDBeatOrder { get; set; }
        public double MTDExtraSales { get; set; }
        public double MTDTotalSales { get; set; }
        public int MTDProductiveCall { get; set; }
        public int MTDVisitPlanned { get; set; }
        public int MTDTotalCall { get; set; }
        public double MTDOutletCoverage { get; set; }
        public double MTDStrikeRate { get; set; }
        public int MTDExtraVisit { get; set; }
        public int MTDExtraCall { get; set; }
        public int MTDExtraOrder { get; set; }
        public double MTDAvgOrderValue { get; set; }
        public double MTDAvgLineItem { get; set; }
        public double MTDEco { get; set; }
        public int MTDSupperOrder { get; set; }
        public int MTDPerfactOrder { get; set; }
    }
    public class YesterdayDashboardDc
    {
        public double YesterdayBeatSales { get; set; }
        public int YesterdayBeatOrder { get; set; }
        public double YesterdayExtraSales { get; set; }
        public double YesterdayTotalSales { get; set; }
        public int YesterdayProductiveCall { get; set; }
        public int YesterdayVisitPlanned { get; set; }
        public int YesterdayTotalCall { get; set; }
        public double YesterdayOutletCoverage { get; set; }
        public double YesterdayStrikeRate { get; set; }
        public int YesterdayExtraVisit { get; set; }
        public int YesterdayExtraCall { get; set; }
        public int YesterdayExtraOrder { get; set; }
        public int YesterdaySupperOrder { get; set; }
        public int YesterdayPerfactOrder { get; set; }
    }
    public class DashboardDetailDc
    {
        public double BeatSales { get; set; }
        public int BeatOrder { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string ChannelName { get; set; }
        public double ExtraSales { get; set; }
        public double TotalSales { get; set; }
        public int ProductiveCall { get; set; }
        public int VisitPlanned { get; set; }
        public int TotalCall { get; set; }
        public double OutletCoverage { get; set; }
        public double StrikeRate { get; set; }
        public int ExtraVisit { get; set; }
        public int ExtraCall { get; set; }
        public int ExtraOrder { get; set; }
        public double AvgOrderValue { get; set; }
        public double AvgLineItem { get; set; }
        public int Eco { get; set; }
        public int SuccessPhoneOrder { get; set; } // order placed by phone order
        public int PerfactOrder { get; set; }
        public int SupperOrder { get; set; }
        public string WarehouseName { get; set; }
    }
    public class BeatCustomerDc
    {
        public List<ExecutiveBeatCustomerDc> TodayBeatCustomerIds { get; set; }
        public List<ExecutiveBeatCustomerDc> MonthBeatCustomerIds { get; set; }
        public List<ExecutiveBeatCustomerDc> YesterdayBeatCustomerIds { get; set; }
    }
    public class ExecutiveBeatCustomerDc
    {
        public int Customerid { get; set; }
    }
    public class PeopleSentNotificationDCs
    {
        public long Id { get; set; }
        public int OrderId { set; get; }
        public int AppId { set; get; } //DeliveryApp = 2, SalesApp = 3, SarthiApp = 4, RetailerApp = 1
        public int ToPeopleId { set; get; }
        public string FcmId { set; get; }
        public string Message { set; get; }
        public int NotificationType { set; get; }  // 1: Notifcation , 2 :Notifcation for IsApproval
        public bool IsApproved { set; get; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public int TotalCount { set; get; }
        public string ApprovalBy { get; set; }
        public DateTime TimeLeft { get; set; }
        public string OrderStatus { get; set; }
        public long TripPlannerConfirmedDetailId { get; set; }
        public bool IsRejected { set; get; }
        public int? RejectedBy { get; set; }
        public string RejectedByName { get; set; }
    }
    public class PeopleSentNotificationDC
    {
        public long Id { get; set; }
        public int ToPeopleId { set; get; }
        public int OrderId { set; get; }
        public string Message { set; get; }
        public int NotificationType { set; get; }  // 1: Notifcation , 2 :Notifcation for IsApproval
        public bool IsApproved { set; get; }
        public bool IsRejected { set; get; }
        public DateTime CreatedDate { get; set; }
        public DateTime TimeLeft { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string ShopAddress { get; set; }
        public string Status { get; set; }
        public string Customerphonenum { get; set; }
        public double TotalAmount { get; set; }
        public int DboyId { get; set; }
        public string DboyName { get; set; }
        public string DboyMobileNo { get; set; }
        public string Shopimage { get; set; }
        public List<Reason> reasons { get; set; }
        public int TotalCount { set; get; }
        public long TripPlannerConfirmedDetailId { get; set; }
    }
    public class Reason
    {
        public string reason { get; set; }
        public long PeopleSentNotificationId { get; set; }
    }
    public class FocusBrandDCs
    {
        public string SubsubcategoryName { get; set; }
        public string HindiName { get; set; }
        public double Amount { get; set; }
        public int CustomerTarget { get; set; }
        public int AchieveCustomerCount { get; set; }
    }
    public class GetTopHighMarginSkuItemListDCs
    {
        public int ItemId { get; set; }
        public string itemName { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
    }
    public class MyBeatDC
    {
        public object ObjectId { get; set; }
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string ShopName { get; set; }
        public string ShippingAddress { get; set; }
        public string Mobile { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int LastOrderDaysAgo { get; set; }
        public bool IsCompany { get; set; }
        public List<SalesGroupDc> SalesGroupDcs { get; set; }
        public string CustomerVerify { get; set; }
        public bool IsVisited { get; set; }
        public double MTDSales { get; set; }
        public int OrderCount { get; set; }
        public int PendingOrderCount { get; set; }
        public string Day { get; set; }
        public string CustomerName { get; set; }
        public int? WarehouseId { get; set; }
        public bool Active { get; set; }
        public bool IsKPP { get; set; }
        public bool IsBeatEdit { get; set; }
        public double Distance { get; set; }
        public bool IsReschedule { get; set; }
        public List<InsertCustomerRemarksDC> CustomerRemarksDCs { get; set; }
        public DateTime? CheckIn { get; set; }
        public bool IsCustomerUpdate { get; set; }
        public bool IsVerifyRequestPending { get; set; }
        public bool IsCustomerProfileEdit { get; set; }
        public int LastVisitDaysAgo { get; set; }
        public bool IsBeat { get; set; }
        public string CRMTag { get; set; }
        public string CustomerType { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
    }
    public class SalesGroupDc
    {
        public int CustomerId { get; set; }
        public long id { get; set; }
        public string MainGroup { get; set; }
        public string GroupName { get; set; }
        public int StoreId { get; set; }
        public int CreatedBy { get; set; }
        public bool isEdit { get; set; }
    }
    public class InsertCustomerRemarksDC
    {
        public long? Id { get; set; }
        public long CustomerId { get; set; }
        public long ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDelete { get; set; }
    }
    public class BeatCustomerOrderDCs
    {
        public int CustomerId { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int LastOrderDaysAgo { get; set; }
        public string CustomerVerify { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string ShippingAddress { get; set; }
        public double Distance { get; set; }
        public DateTime? CheckIn { get; set; }
        public bool IsCustomerUpdate { get; set; }
        public bool IsVerifyRequestPending { get; set; }
        public bool IsCustomerProfileEdit { get; set; }
        public string CustomerType { get; set; }

    }
    public class CRMCustomerWithTag
    {
        public int CustomerId { get; set; }
        public string CRMTags { get; set; }
        public string Skcode { get; set; }
    }
    public class StoreDCs
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public List<GroupDCs> GroupDcs { get; set; }
    }
    public class GroupDCs
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
    }
    public class GroupList
    {
        // u.GroupName, u.Id, u.StoreId
        public long Id { get; set; }
        public long StoreId { get; set; }
        public string GroupName { get; set; }
    }
    public class MyBeatCustomerDc
    {
        public List<MyBeatDC> MyBeatDCs { get; set; }
        public List<MyBeatDC> NoVisitMyBeatDCs { get; set; }
        public int VisitCount { get; set; }
        public int NonVisitCount { get; set; }
        public int totalRecord { get; set; }
        public bool IsBeatEdit { get; set; }
        public int Reschedule { get; set; }
    }
    public class BeatCustomerDTOdc
    {
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public int PeopleId { get; set; }
        public List<int> OrderDays { get; set; }
        public List<int> VisitDays { get; set; }
        public string Day { get; set; }
        public string KeyValue { get; set; }
        public int warehouseId { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public bool BeatEdit { get; set; }
        public bool IsPagination { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string APIType { get; set; }
    }
    public class AllBeatCustomerDc
    {
        public int CustomerId { get; set; }
        public bool IsVisited { get; set; }
        public string Day { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
    }
    public class SalesGroupMatrixDCs
    {
        public List<SalesMatrixDC> SalesMatrixDCs { get; set; }
        public List<GroupMatrixDC> GroupMatrixDCs { get; set; }
        public int Level { get; set; }
    }
    public class SalesMatrixDC
    {

        public int TotalSales { get; set; }
        public int OrderCount { get; set; }
        public double AvgLineItem { get; set; }
        public int PendingOrder { get; set; }
        public string Skcode { get; set; }
        public string ShippingAddress { get; set; }
        public string Mobile { get; set; }
        public double lat { get; set; }

        public double lg { get; set; }

        public string ShopName { get; set; }


    }

    public class GroupMatrixDC
    {
        public int StoreId { get; set; }
        public string Name { get; set; }

        public int OrderId { get; set; }
        public int TotalSales { get; set; }


    }
    public class GroupMetricsDTODC
    {
        public int CustomerId { get; set; }
        public int PeopleId { get; set; }
        public int flag { get; set; }
        public int warehouseid { get; set; }
    }
    public class CRMLevelBeatCustomerDC
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int Cityid { get; set; }
        public string SkCode { get; set; }
        public int CustomerId { get; set; }
        public int BrandCount { get; set; }
        public int OrderCount { get; set; }
        public double Volume { get; set; }
        public double KKvolume { get; set; }
        public int Selfordercount { get; set; }
        public int Salespersonordercount { get; set; }
        public bool IsActive { get; set; }
        public string Level { get; set; }
    }
    public class BeatCustomersCRMDC
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int OrderCount { get; set; }
        public int CustomerId { get; set; }
        public int BrandCount { get; set; }
        public int SelfOrderCount { get; set; }
        public double Volume { get; set; }
        public double KKvolume { get; set; }
        public int Level { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int Salespersonordercount { get; set; }
        public string Skcode { get; set; }
    }
    public class SalesMyGroupDCs
    {
        public long id { get; set; }
        public string GroupName { get; set; }
        public bool isEdit { get; set; }
    }
    public class SalesGroupFilterDC
    {
        public string GroupName { get; set; }
        public int PeopleId { get; set; }
        public int storeid { get; set; } //0 company,-1 salesperson, -2 All, 0 < store
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int IsNewGroup { get; set; } // 0-new group, 1 exist group
        public int customerid { get; set; }
        public int GroupId { get; set; }
    }
    public class GroupCustRemoveDC
    {
        public int PeopleId { get; set; }
        public int GroupId { get; set; }
        public int CustomerID { get; set; }
    }
    public class BeatEditDCs
    {
        public int CustomerID { get; set; }
        public int ExecutiveId { get; set; }
        public DateTime StartDate { get; set; }
        public string Day { get; set; }
        public int StoreID { get; set; }
        public bool? IsBeatEdit { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
    }
    public class DayWiseShopCountDC
    {
        public string Day { get; set; }
        public int ShopCount { get; set; }
    }
    public class ExecutiveAllBeatDC
    {
        public int Customerid { get; set; }
        public string day { get; set; }
    }
    public class RescheduleBeatDCs
    {
        public int CustomerID { get; set; }
        public int ExecutiveId { get; set; }
        public DateTime StartDate { get; set; }
        public string Day { get; set; }
        public int StoreID { get; set; }
        public bool? IsReschedule { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
    public class CheckInStatuDc
    {
        public int customerId { get; set; }
        public string SKCode { get; set; }
        public string ShopName { get; set; }
        public string CustomerType { get; set; }
    }


    public class ItemResponseDc
    {
        public int TotalItem { get; set; }
        public List<ItemDataDC> ItemDataDCs { get; set; }
    }
    public class PopularRecentSearchDC
    {
        public List<string> RecentSearch { get; set; }
        public List<string> PopularSearch { get; set; }
    }
    public class SaleDC
    {
        public double Sale { get; set; }
        public string SubsubcategoryName { get; set; }

    }
    public class AgentChequeBouncePaginatorDC
    {
        public int AgentId { get; set; }
        public int WarehouseId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class AgentChequeBounceInfoLists
    {
        public List<AgentChequeBounceInfoDC> agentChequeBounceInfo { get; set; }
        public int Count { get; set; }
        public double TotalAmount { get; set; }
    }
    public class AgentChequeBounceInfoDC
    {
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string ChequeNumber { get; set; }
        public decimal ChequeAmt { get; set; }
        public DateTime ChequeDate { get; set; }
        public string ChequeBankName { get; set; }
        public int Orderid { get; set; }

    }
    public class SalesPersonCommissionData
    {

        public long Id { get; set; }
        public string CategoryName { get; set; }
        public string EventCatName { get; set; }
        public string EventName { get; set; }
        public int WarehouseId { get; set; }
        public int ExecutiveId { get; set; }
        public string Name { get; set; }
        public double ReqBookedValue { get; set; }
        public int IncentiveType { get; set; }
        public double IncentiveValue { get; set; }
        public double BookedValue { get; set; }
        public double EarnValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShowColumnWithValueField { get; set; }

    }
    public class SalesPersonCommission
    {
        public string Name { get; set; }

        public int EarnValue
        {
            get
            {
                if (CategoryCommissions != null && CategoryCommissions.Any())
                    return CategoryCommissions.Sum(x => x.EarnValue);
                else
                    return 0;
            }
        }
        public List<CategoryCommission> CategoryCommissions { get; set; }

    }
    public class CategoryCommission
    {
        public string CategoryName { get; set; }

        public Dictionary<string, string> ShowColumnWithValueField { get; set; }
        public int EarnValue
        {
            get
            {
                if (EventCommissions != null && EventCommissions.Any())
                    return EventCommissions.Sum(x => x.EarnValue);
                else
                    return 0;
            }
        }
        public List<EventCommission> EventCommissions { get; set; }
    }
    public class EventCommission
    {
        public long Id { get; set; }
        public string EventCatName { get; set; }
        public string EventName { get; set; }
        public int ReqBookedValue { get; set; }
        public int IncentiveType { get; set; }
        public double IncentiveValue { get; set; }
        public int BookedValue { get; set; }
        public int EarnValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CalelogDc
    {
        public List<SalesCategory> SalesCategories { get; set; }
        public List<SalesCompany> SalesCompanies { get; set; }
        public List<SalesBrand> SalesBrands { get; set; }
        public List<SalesCompany> BrandCompanies { get; set; }
        public List<SalesBrand> Brands { get; set; }
    }

    public class SalesCategory
    {
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string LogoUrl { get; set; }
        public string HindiName { get; set; }
        public string CategoryImg { get; set; }
        public int itemcount { get; set; }
    }
    public class SalesCompany
    {
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string SubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int Sequence { get; set; }
        public int itemcount { get; set; }
    }

    public class SalesBrand
    {
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int Categoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int itemcount { get; set; }
    }
    public class SalesItemResponseDc
    {
        public int TotalItem { get; set; }
        public List<SalesAppItemDataDC> ItemDataDCs { get; set; }
    }
    public class SalesAppItemDataDC
    {
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public string itemname { get; set; }
        public double price { get; set; }
        public double UnitPrice { get; set; }
        public int MinOrderQty { get; set; }
        public string LogoUrl { get; set; }
        public string HindiName { get; set; }
        public int? dreamPoint { get; set; }
        public double? marginPoint { get; set; }
        public int WarehouseId { get; set; }
        public int CompanyId { get; set; }
        public string ItemNumber { get; set; }
        public bool IsOffer { get; set; }
        public string OfferFreeItemName { get; set; }
        public string OfferFreeItemImage { get; set; }
        public int? OfferFreeItemQuantity { get; set; }
        public int? OfferCategory { get; set; }
        public int? OfferId { get; set; }
        public double? OfferWalletPoint { get; set; }
        public int? OfferMinimumQty { get; set; }
        public int? OfferFreeItemId { get; set; }
        public string OfferType { get; set; }
        public int ItemlimitQty { get; set; }
        public bool IsItemLimit { get; set; }
        public int BillLimitQty { get; set; }
        public int Itemtype { get; set; } //1=baseitem,2=suggested item,3=promotional item
        public int Sequence { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int? LastOrderQty { get; set; }
        public int? LastOrderDays { get; set; }
        public string Scheme { get; set; }
        public bool isChecked { get; set; }
        public bool Active { get; set; }
        public string Classification { get; set; }
        public string BackgroundRgbColor { get; set; }
        public List<SalesAppItemDataDC> moqList { get; set; }

    }
    public class GetProductCatalogDC
    {
        public long Id { get; set; }
        public string SectionName { get; set; }
        public string SectionHindiName { get; set; }
        public bool IsPromotional { get; set; }
        public string Type { get; set; }
        public long WarehouseId { get; set; }
        public int Sequence { get; set; }
        public string URL { get; set; }
        public string CustomList { get; set; }
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
    
    public class ShoppingCartItemDc
    {
        public bool active;
        public int ItemId { get; set; }
        public int ItemlimitQty { get; set; }
        public bool IsItemLimit { get; set; }
        public int BillLimitQty { get; set; }
        public string ItemNumber { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public int DepoId { get; set; }
        public string DepoName { get; set; }
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public string itemname { get; set; }
        public string HindiName { get; set; }
        public double price { get; set; }
        public string SellingUnitName { get; set; }
        public string SellingSku { get; set; }
        public double UnitPrice { get; set; }
        public double VATTax { get; set; }
        public string LogoUrl { get; set; }
        public int MinOrderQty { get; set; }
        public double Discount { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? marginPoint { get; set; }
        public int? promoPerItems { get; set; }
        public int? dreamPoint { get; set; }
        public bool IsOffer { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsSensitiveMRP { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }
        public string itemBaseName { get; set; }
        public bool Deleted { get; set; }
        public double NetPurchasePrice { get; set; }
        public bool IsFlashDealUsed { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int? OfferCategory
        {
            get; set;
        }
        public DateTime? OfferStartTime
        {
            get; set;
        }
        public DateTime? OfferEndTime
        {
            get; set;
        }
        public double? OfferQtyAvaiable
        {
            get; set;
        }

        public double? OfferQtyConsumed
        {
            get; set;
        }

        public int? OfferId
        {
            get; set;
        }

        public string OfferType
        {
            get; set;
        }

        public double? OfferWalletPoint
        {
            get; set;
        }
        public int? OfferFreeItemId
        {
            get; set;
        }
        public int? FreeItemId
        {
            get; set;
        }


        public double? OfferPercentage
        {
            get; set;
        }

        public string OfferFreeItemName
        {
            get; set;
        }

        public string OfferFreeItemImage
        {
            get; set;
        }
        public int? OfferFreeItemQuantity
        {
            get; set;
        }
        public int? OfferMinimumQty
        {
            get; set;
        }
        public double? FlashDealSpecialPrice
        {
            get; set;
        }
        public int? FlashDealMaxQtyPersonCanTake
        {
            get; set;
        }

        public int qty { get; set; }
        public double CartUnitPrice { get; set; }
        public double NewUnitPrice { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string MessageKey { get; set; }

        public int TotalFreeItemQty { get; set; }
        public double TotalFreeWalletPoint { get; set; }

        public double? DistributionPrice { get; set; }
        public bool DistributorShow { get; set; }
        public bool IsPrimeItem { get; set; }
        public double PrimePrice { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreLogo { get; set; }
        public string Scheme { get; set; }
        public int ItemAppType { get; set; }
        public bool? IsDealItem { get; set; }
    }
    public class LastPOOrderData
    {
        public int OrderId { get; set; }
    }
    public class UdharOverDueResponse
    {
        public bool IsUdharOverDue { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
        public object Data { get; set; }
    }
    public class CheckDueAmtDc
    {
        public double Amount { get; set; }
        public int OverDueDays { get; set; }
    }
    public class UpdateCheckOutDC
    {
        public long Id { get; set; }
        public string Comment { get; set; }
        public int Sequence { get; set; }
    }
    public class WROFFERTEM
    {
        public List<factoryItemdata> ItemMasters { get; set; }
        public List<factorySubSubCategory> SubsubCategories { get; set; }
        public bool Message { get; set; }
    }
    public class NewLaunchesItemNotificationDC
    {
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public double UnitPrice { get; set; }
        public string WarehouseName { get; set; }
        public string LogoUrl { get; set; }
    }
    public class GetNotificationByPeopleDc
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LogoUrl { get; set; }
    }
    public class SalesIntentItemResponse
    {
        public string Itemname { get; set; }
        public string LogoUrl { get; set; }
        public double MRP { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsSensitiveMRP { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int SystemForecastQty { get; set; }
        public int WarehouseId { get; set; }
        public List<int> PurchaseMOQList { get; set; }

    }
    public class SalesIntentHistoryDC
    {
        public string itemBaseName { get; set; }

        public bool IsSensitive { get; set; }

        public bool IsSensitiveMRP { get; set; }

        public string LogoUrl { get; set; }

        public double MRP { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int RequestQty { get; set; }
        public double RequestPrice { get; set; }
        public int SystemSuggestedQty { get; set; }
        public int Warehouseid { get; set; }
        public int Status { get; set; }

        public bool Deleted { get; set; }

        public bool IsDisContinued { get; set; }

        public int SubsubCategoryid { get; set; }

        public int SubCategoryId { get; set; }

        public int Categoryid { get; set; }


        public string Number { get; set; }

        public string UnitofQuantity { get; set; }

        public string UOM { get; set; }

        public string HindiName { get; set; }

    }
    public class SalesIntentRequestDC
    {
        public long ItemForecastDetailId { get; set; }

        public ItemForecastDetail ItemForecastDetail { get; set; }

        public int PeopleId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int RequestQty { get; set; }
        public double RequestPrice { get; set; }
        public int SalesLeadApproveID { get; set; }
        public DateTime? SalesApprovedDate { get; set; }
        public int BuyerApproveID { get; set; }
        public DateTime? BuyerApprovedDate { get; set; }
        public int Status { get; set; }  // Pending for Lead = 0, Pending for buyer = 1, Rejected = 2, Approved = 3  
        public int Warehouseid { get; set; }
        public int CreatedBy { get; set; }
        public int? MinOrderQty { get; set; } // TO Do..New Change
        public int? NoOfSet { get; set; } //TO Do.. New Change
        public string ETADate { get; set; } //TO Do.. New Change datetime to string


    }
    public class SalesIncentiveItemClassification
    {
        public int PeopleId { get; set; }
        public string ItemClassification { get; set; }
        public long ItemIncentiveClassificationId { get; set; }
        public double CommissionPercentage { get; set; }
        public double SaleValue { get; set; }
        public double Earning { get; set; }
    }
    public class SalesTargetResponse
    {
        public double AchivePercent { get; set; }
        public List<SalesTargetCustomerItem> SalesTargetCustomerItems { get; set; }
    }

    public class SalesTargetCustomerItem
    {
        public int Achieveqty { get; set; }
        public int TargetQty { get; set; }

        public double Percent
        {
            get
            {

                return TargetQty > 0 ? Achieveqty * 100 / TargetQty : 0;
            }
        }

        public string ItemName { get; set; }
        public double price { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
    }
    public class CompanySalesTargetCustomer
    {
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public int TargetQty { get; set; }
        public int Achieveqty { get; set; }
    }
    public class GetCustReferralConfigDc
    {
        public int OnOrder { get; set; }
        public string OrderCount { get; set; }
        public string orderStatus { get; set; }
        public int OnDeliverd { get; set; }
        public double ReferralWalletPoint { get; set; }
        public double CustomerWalletPoint { get; set; }
    }
    public class GetPeopleReferralOrderListDc
    {
        public string PeopleName { get; set; }
        public string ShopName { get; set; }
        public string SkCode { get; set; }
        public string ReferralSkCode { get; set; }
        public double ReferralWalletPoint { get; set; }
        public double CustomerWalletPoint { get; set; }
        public int OnOrder { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; }
        public int IsUsed { get; set; }
    }
    public class AttendanceRuleConfigsLogDc
    {
        public long ConfigId { get; set; }
        public bool IsCheckinBeatShop { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public bool IsCheckOutBeatShop { get; set; }
        public bool IsFullDayBeatShop { get; set; }
        public bool IsMinimumVisit { get; set; }
        public int? DayMinVisits { get; set; }
        public bool IsTADABeatShop { get; set; }
        public bool IsTADARequired { get; set; }
        public int? TADACalls { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public long StoreId { get; set; }
        public int CityId { get; set; }
        public long ChannelMasterId { get; set; }
    }
    public class ExecutiveAttendanceDetailDc
    {
        public long Id { get; set; }
        public int ExecutiveId { get; set; }
        public string Day { get; set; }
        public DateTime? FirstCheckIn { get; set; }
        public DateTime? LastCheckOut { get; set; }
        public int TC { get; set; }
        public int PC { get; set; }
        public string Status { get; set; }
        public string TADA { get; set; }
        public string WarehouseIds { get; set; }
        public string StoreIds { get; set; }
        public string ClusterIds { get; set; }
        public string Warehouse { get; set; }
        public string Store { get; set; }
        public string Cluster { get; set; }
        public bool IsLate { get; set; }
        public bool ConfigIsBeatCheckIn { get; set; }
        public bool ConfigIsBeatCheckOut { get; set; }
        public bool ConfigIsBeatTADA { get; set; }
        public int ConfigFullDayMinVisitCount { get; set; }
        public int ConfigTADAMinProductiveCallCount { get; set; }
        public TimeSpan? ConfigFirstCheckInTime { get; set; }
        public bool IsPresent { get; set; }
        public int CityId { get; set; }
        public int StoreId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public long ConfigId { get; set; }
        public long ChannelMasterId { get; set; }
    }
    public class AttendenceDetailDC
    {
        public int ExecutiveId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? FirstCheckIn { get; set; }
        public TimeSpan? LastCheckOut { get; set; }
        public string TADA { get; set; }
        public string Status { get; set; }
        public bool IsLate { get; set; }
        public bool IsPresent { get; set; }
        public int CityId { get; set; }
        public string Description { get; set; }
    }
    public class ExecutiveAttendanceLogListDC
    {
        public double PresentDay { get; set; }
        public int AbsentDay { get; set; }
        public int TADADay { get; set; }
        public List<AttendenceDetailDC> AttendenceDetailList { get; set; }
    }
    public class ExecutiveAttendanceMonthReportDC
    {
        public int ExecutiveId { get; set; }
        public string Day { get; set; }
        public DateTime? FirstCheckIn { get; set; }
        public DateTime? LastCheckOut { get; set; }
        public int TC { get; set; }
        public int PC { get; set; }
        public string Status { get; set; }
        public string TADA { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsLate { get; set; }
        public bool IsPresent { get; set; }
        public int CityId { get; set; }
        public string Description { get; set; }
    }
    public class ClassificationMastersDcs
    {
        public long Id { get; set; }
        public string Classification { get; set; }
        //public long? StoreId { get; set; }
    }
    public class BeatDSR
    {
        public List<int> CityIds { get; set; }
        public string Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }

    }
    public class PeopleDataDC
    {
        public int PeopleID { get; set; }
        public int WarehouseId { get; set; }
        public string DisplayName { get; set; }
        public string CityName { get; set; }
    }
    public class BeatDSRReportResponseDc
    {
        public List<salesDashboardTodayDC> salesTodayDC { get; set; }
        public List<DigitalSalesBeatDSRDC> DigitalSalesBeatDSRMTDData { get; set; }
        public int Totalcount { get; set; }
    }
    public class salesDashboardTodayDC 
    {
        public int peopleId { get; set; }
        public int SalesPersonId { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string SalesPerson { get; set; }
        public int VisitPlanned { get; set; }
        public double BeatSales { get; set; }
        public double ExtraSales { get; set; }
        public double TotalSales { get; set; }
        public int ProductiveCall { get; set; }
        public int ExtraCall { get; set; }
        public int BeatOrder { get; set; }
        public int ExtraOrder { get; set; }
        public int TodayPerfactOrder { get; set; }
        public int TodaySupperOrder { get; set; }
        public int TotalCall { get; set; }
        public int ExtraVisit { get; set; }
        public long TotalRecord { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public double TodayStrikeRate { get; set; }
        public double TodayOutletCoverage { get; set; }
        public double Eco { get; set; }
        public int TodayProductiveCall { get; set; }
        public int TodayTotalCall { get; set; }
        public int TodayVisitPlanned { get; set; }
        public int PhoneOrder { get; set; }
        public double AvgLineItem { get; set; }
        public double AvgOrderValue { get; set; }
        public string ChannelName { get; set; }
    }
    public class DigitalSalesBeatDSRDC : DigitalSalesBeatDSR
    {
        public string CityName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
    public class BeatCustomers
    {
        public int Customertype { get; set; }
        public List<int> customers { get; set; }
    }

    public class AllExecutiveAttendanceRowDetailsDC
    {
        public string EmployeeCode { get; set; }
        public string ExecuitveName { get; set; }
        public string Store { get; set; }
        public string Warehouse { get; set; }
        public string Cluster { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public string SkCode { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string BeatCustomer { get; set; }
        public int OrderCount { get; set; }
    }

    public class ExecutiveDataList
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string Empcode { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string WarehouseName { get; set; }
    }

    public class MarketingCostDashboardDC
    {
        public List<long> StoreId { get; set; }
        public DateTime date { get; set; }
        public string OrderType { get; set; }
        public bool IsExport { get; set; }
        public List<string> RetailerType { get; set; }
        public List<string> RetailerClass { get; set; }
    }
    public class MarketingCostWarehouseDC
    {
        public string WarehouseName { get; set; }
        public double TotalSales { get; set; }
        public double Cancellation { get; set; }
        public double Offers { get; set; }
        public double WalletPoint { get; set; }
        public double Freebies { get; set; }
        public double TotalSpend { get; set; }
        public double MarketingCost { get; set; }
    }
    public class WarehouseAPPDC
    {
        public int WarehouseId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double APP { get; set; }
    }
    public class OrderElasticDataDC
    {
        public int executiveid { get; set; }
        public int custid { get; set; }
        public int orderid { get; set; }
        public int orderdetailid { get; set; }
        public int whid { get; set; }
        public int ordqty { get; set; }
        public double price { get; set; }
        public long storeid { get; set; }
        public string itemnumber { get; set; }
        public string itemname { get; set; }
        public DateTime createddate { get; set; }
        public long dispatchqty { get; set; }
        public string clustername { get; set; }
        public int brandid { get; set; }
        public string cityname { get; set; }
        public string whname { get; set; }
        public int catid { get; set; }
        public string catname { get; set; }
        public string compname { get; set; }
        public string brandname { get; set; }
        public string status { get; set; }
        public double walletamount { get; set; }
        public int itemmultimrpid { get; set; }
        public double billdiscountamount { get; set; }
        public bool isfreeitem { get; set; }
    }
    public class ExcelOrderElasticDataDC
    {
        public double custid { get; set; }
        public double orderid { get; set; }
        public double orderdetailid { get; set; }
        public double whid { get; set; }
        public double ordqty { get; set; }
        public double price { get; set; }
        public double storeid { get; set; }
        public string itemnumber { get; set; }
        public string itemname { get; set; }
        public DateTime createddate { get; set; }
        public DateTime? dispatchdate { get; set; }
        public DateTime updateddate { get; set; }
        public double dispatchqty { get; set; }
        public double brandid { get; set; }
        public string whname { get; set; }
        public double catid { get; set; }
        public string catname { get; set; }
        public string compname { get; set; }
        public string brandname { get; set; }
        public string status { get; set; }
        public double walletamount { get; set; }
        public double itemmultimrpid { get; set; }
        public double billdiscountamount { get; set; }
        public bool isfreeitem { get; set; }
        public string customertype { get; set; }
        public string customerclass { get; set; }
    }
    public class MarketingCostExportDC
    {
        public string WarehouseName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string BrandName { get; set; }
        public string StoreName { get; set; }
        public double TotalSales { get; set; }
        public double Cancellation { get; set; }
        public double Offers { get; set; }
        public double WalletPoints { get; set; }
        public double Freebies { get; set; }
        public double TotalSpend { get; set; }
        public double MarketingCost { get; set; }
    }
    public class SubCategoryList
    {
        public long SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public long CatId { get; set; }
        public long BrandId { get; set; }
    }
    public class OrderDiscountDC
    {
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public int OrderQty { get; set; }
        public int? DispatchQty { get; set; }
        public double UnitPrice { get; set; }
        public double BillDiscountAmount { get; set; }
        public double WalletAmount { get; set; }
    }
    public class ItemIncentiveClassificationDC
    {
        public int CityId { get; set; }
        public int ItemMultiMrpId { get; set; }
        public string Classification { get; set; }
        public string BackgroundRgbColor { get; set; }

    }
    public class AddCheckOutReason
    {
        public int CustomerId { get; set; }
        public int ExecutiveId { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        public string CurrentAddress { get; set; }
        public double Distance { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string ChackOutComment { get; set; }
        public string ShopCloseImage { get; set; }
        public bool IsEnd { get; set; }
        public bool IsBeat { get; set; }

        public bool IsCustInterested { get; set; }
        public bool OrderTaken { get; set; }
        public long? OrderId { get; set; }
        public double? OrderAmount { get; set; }
        public string TechProductInquiry { get; set; }
        public string SKUInquiry { get; set; }
        public string RequiredItemInquiry { get; set; }
        public bool OfferExplain { get; set; }
        public bool MyTargetExplain { get; set; }
        public bool GameSectionExplain { get; set; }
        public bool IsPhysicalVisit { get; set; }
        public int FormType { get; set; }
        public bool IsCall { get; set; }
        public TimeSpan? Duration { get; set; }
        public TimeSpan? ActualCallDuration { get; set; }
    }
    public class APIResponse
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public object Data { get; set; }
    }

    public class SalesKpiResponse
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public List<SalesPersonKpiResponse> SalesPersonKpi { get; set; }
    }
    public class SalesPersonKpiResponse
    {
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public double Target { get; set; }
        public double Achievement { get; set; }
        public double AchievementPercent { get; set; }
        public double Earning { get; set; }

    }
    public class SalesPersonKpiElasticData
    {
        public string skcode { get; set; }
        public long storeid { get; set; }
        public double dispatchamount { get; set; }
        public double linecount { get; set; }
    }
    public class CustStoreTargets
    {
        public string skcode { get; set; }
        public long StoreId { get; set; }
        public double Target { get; set; }
        public int? TargetLineItem { get; set; }
    }
    public class SalesOrder
    {
        public int OrderId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime Deliverydate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? ReadytoDispatchedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public double GrossAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string Status { get; set; }
        public string PaylaterStatus { get; set; }
        public double? walletPointUsed { get; set; }
        public double? RewardPoint { get; set; }

        public int CustomerId { get; set; }
        public string ShopName { get; set; }
        public string Skcode { get; set; }
        public List<SalesOrderDetail> orderDetails { get; set; }
        public List<OrderStatusHistoryDc> OrderStatusHistoryDcs { get; set; }


        //not used
        public int CompanyId { get; set; }
        public int? SalesPersonId { get; set; }
        public string SalesPerson { get; set; }
        public string SalesMobile { get; set; }

        public string invoice_no { get; set; }
        public string Trupay { get; set; }
        public string paymentThrough { get; set; }
        public string TrupayTransactionId { get; set; }
        public string paymentMode { get; set; }
        public int CustomerCategoryId { get; set; }
        public string CustomerCategoryName { get; set; }
        public string CustomerType { get; set; }
        public string LandMark { get; set; }
        public string Customerphonenum { get; set; }
        public double TotalAmount { get; set; }
        public double DiscountAmount { get; set; }
        public double TaxAmount { get; set; }

        public double TCSAmount { get; set; }

        public double SGSTTaxAmmount { get; set; }
        public double CGSTTaxAmmount { get; set; }
        public int? CityId { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public bool active { get; set; }
        public bool Deleted { get; set; }
        public int ReDispatchCount { get; set; }
        public int DivisionId { get; set; }
        public string ReasonCancle { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public double? deliveryCharge { get; set; }
        public double? WalletAmount { get; set; }
        public double? UsedPoint { get; set; }
        public double ShortAmount { get; set; }
        public string comments { get; set; }
        public int? OrderTakenSalesPersonId { get; set; }
        public string OrderTakenSalesPerson { get; set; }
        public string Tin_No { get; set; }
        public string ShortReason { get; set; }
        public bool orderProcess { get; set; }
        public bool accountProcess { get; set; }
        public bool chequeProcess { get; set; }
        public bool epaymentProcess { get; set; }
        public double Savingamount { get; set; }
        public double OnlineServiceTax { get; set; }
        public byte[] InvoiceBarcodeImage { get; set; }
        public int OrderType { get; set; } = 1;
        public bool? IsPrimeCustomer { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public int userid { get; set; }
        public string Description { get; set; }
        public bool IsLessCurrentStock { get; set; }
        public double? BillDiscountAmount { get; set; }
        public string offertype { get; set; }
        public double RemainingTime
        { get; set; }

        public bool IsIgstInvoice { get; set; }
        public bool InactiveStatus => Status == "Inactive";
        public List<OrderMasterHistories> OrderMasterHistories { get; set; }
        public int? DeliveryIssuanceIdOrderDeliveryMaster { get; set; }
        public int? OrderDispatchedMasterId { get; set; }
        public DateTime OrderDate { get; set; }
        public double OrderAmount { get; set; }
        public double? DispatchAmount { get; set; }
        public double? DeliveredAmount { get; set; }
        public string OfferCode { get; set; }
        public string EwayBillNumber { get; set; }
        public string CreditNoteNumber { get; set; }
        public DateTime? CreditNoteDate { get; set; }
        public bool RebookOrder { get; set; }
        public bool IsETAEnable { get; set; }
        public bool IsOrderHold { get; set; }
        public DateTime? PrioritizedDate { get; set; }

    }
    public class ItemListDc
    {
        public List<ItemDataDC> ItemMasters { get; set; }
        public int TotalItem { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class LastPOOrderItemNumberData
    {
        public string ItemNumber { get; set; }
    }
    public class TodaySaleDc
    {
        public double TodaySales { get; set; }
        public double TodayTargetSales { get; set; }
        public int TodayTotalVisitBeatCustomer { get; set; }
        public int TodayTotalBeatCustomer { get; set; }
    }
    public class MTDSaleDc
    {
        public double MTDSales { get; set; }
        public double MTDTargetSales { get; set; }
        public int MTDTotalVisitBeatCustomer { get; set; }
        public int MTDTotalBeatCustomer { get; set; }
    }
    public class YesterdaySaleDC
    {
        public double YesterdaySales { get; set; }
        public double YesterdayTargetSales { get; set; }
        public int YesterdayTotalVisitBeatCustomer { get; set; }
        public int YesterdayTotalBeatCustomer { get; set; }
    }
    public class CustomerTarget
    {
        public string TargetMonth { get; set; }
        public int CompanyId { get; set; }
        public long StoreId { get; set; }
        public long TargetDetailId { get; set; }
        public string StoreName { get; set; }
        public string BrandNames { get; set; }
        public string StoreUrl { get; set; }
        public string SKCode { get; set; }
        public string GiftImage { get; set; }
        public string GiftItemName { get; set; }
        public string OfferDesc { get; set; }
        public string Type { get; set; }
        public string Level { get; set; }
        public decimal Value { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
        public decimal TotalPendingPurchaseAmount { get; set; }
        public int? TargetLineItem { get; set; }
        public int? CurrentLineItem { get; set; }
        public decimal AchivePercent { get; set; }
        public int LeftDays { get; set; }
        public bool IsClaimed { get; set; }
        public int OfferValue { get; set; }
        public int? OfferType { get; set; }
        public int? MaxDiscount { get; set; }
        public int? MOVMultiplier { get; set; }
        public List<targetCondition> targetConditions { get; set; }
    }
    public class targetCondition
    {
        public string ConditionText { get; set; }
        public string ConditionCompleteText { get; set; }
        public int Target { get; set; }
        public int CurrentValue { get; set; }

        public decimal AchivePercent { get; set; }
        public string Message { get; set; }
    }
    public class LevelDc
    {
        public int Id { get; set; }
        public string LevelName { get; set; }
        public int Volume { get; set; }
        public int OrderCount { get; set; }
        public int BrandCount { get; set; }
        public int KKVolume { get; set; }
        public bool Selected { get; set; }
    }
    public class SubCategoryTargetCustomerDc
    {
        public long id { get; set; }
        public int CustomerId { get; set; }
        public string ShopName { get; set; }
        public string Skcode { get; set; }
        public decimal CurrentMonthSales { get; set; }
        public decimal Target { get; set; }
        public bool IsClaimed { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsTargetExpire { get; set; }
        public string valueType { get; set; }
        public int WalletValue { get; set; }
        public int SlabLowerLimit { get; set; }
        public int SlabUpperLimit { get; set; }
        public int SubCatId { get; set; }
        public int NoOfLineItem { get; set; }
        public int RequiredNoOfLineItem { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogoUrl { get; set; }
        public string BrandNames { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long TargetDetailId { get; set; }
        public List<TargetCustomerBrandDc> TargetCustomerBrandDcs { get; set; }
        public List<TargetCustomerItemDc> TargetCustomerItemDcs { get; set; }
        public List<GiftItemDc> GiftItemDcs { get; set; }

    }
    public class GiftItemDc
    {
        public long id { get; set; }
        public string ItemLogo { get; set; }
        public int itemid { get; set; }
        public string ItemName { get; set; }
        public int Qty { get; set; } //Dream Item         
    }
    public class TargetCustomerBrandDc
    {
        public long id { get; set; }
        public string BrandName { get; set; }
        public decimal Target { get; set; }
        public decimal currentTarget { get; set; }
    }

    public class TargetCustomerItemDc
    {
        public long id { get; set; }
        public string ItemName { get; set; }
        public decimal Target { get; set; }
        public decimal currentTarget { get; set; }
    }
    public class ElasticOrderData
    {
        public int custid { get; set; }
    }
    public class ClassificationMastersDc
    {
        public long Id { get; set; }
        public string Classification { get; set; }
        //public long? StoreId { get; set; }
    }
    public class CheckOutReasonRowDataDC
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public int FormType { get; set; }
        public string ExecutiveCode { get; set; }
        public string ExecutiveName { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string Duration { get; set; }
        public string ActualCallDuration { get; set; }
        public string CallConnected { get; set; }
        public string Warehouse { get; set; }
        public string Cluster { get; set; }
        public string Skcode { get; set; }
        public string CustomerName { get; set; }
        public string ShopName { get; set; }
        public string PhysicalVisitToggle { get; set; }
        public string Comment { get; set; }
        public string OtherComment { get; set; }
        public string IsCustInterested { get; set; }
        public string OrderTaken { get; set; }
        public long? OrderId { get; set; }
        public double? OrderAmount { get; set; }
        public string TechProductInquiry { get; set; }
        public string SKUInquiry { get; set; }
        public string RequiredItemInquiry { get; set; }
        public string OfferExplain { get; set; }
        public string MyTargetExplain { get; set; }
        public string GameSectionExplain { get; set; }
    }
    public class SalesPersonKpiListDc
    {
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public double Target { get; set; }
        public double Achievement { get; set; }
        public double AchievementPercent { get; set; }
        public double Earning { get; set; }
        public string ExecutiveName { get; set; }
        public string StoreName { get; set; }


    }

    public class doubleVal
    {
        public double val { get; set; }
    }
    public class SalesPersonKPIOrderData
    {
        public string skcode { get; set; }
        public long storeid { get; set; }
        public string itemnumber { get; set; }
        public double dispatchqty { get; set; }
        public double price { get; set; }
        public int custid { get; set; }
    }
    public class PeopleData
    {
        public int PeopleID { get; set; }
        public string DisplayName { get; set; }
        public long ChannelMasterId { get; set; }
        public string ChannelName { get; set; }
        public int WarehouseId { get; set; }
    }
    public class PerformanceTargetDc
    {
        public string Name { get; set; }
        public double NumPer { get; set; }
        public int PeopleID { get; set; }
    }
    public class SalesPerformanceDashbordFilter
    {
        public List<int> CityIds { get; set; }
        public List<int> WarehouseIds { get; set; }
        public List<int> StoreIds { get; set; }
        public DateTime FirstDate { get; set; }
        public DateTime LastDate { get; set; }
        public string KeyWord { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public bool IsExecutiveData { get; set; }
        public bool WarehouseData { get; set; }
    }
    public class SalesPerformanceDashbordListDC
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string ClusterName { get; set; }
        public string StoreName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double Sales { get; set; }
        public double BeatSales { get; set; }
        public string SalesColour { get; set; }
        public double Dispatch { get; set; }
        public double BeatVisitPercent { get; set; }
        public int BeatTC { get; set; }
        public int TC { get; set; }
        public string TCColour { get; set; }
        public int BeatPC { get; set; }
        public int ProductiveCall { get; set; }
        public string PCColour { get; set; }
        public int ActiveRetailer { get; set; }
        public double AvgLineItem { get; set; }
        public int PerfactOrder { get; set; }
        public int Range { get; set; }
        public double ActualTimeSpend { get; set; }
    }
    public class SalesPerformanceDashbordExecDC
    {
        public List<SalesPerformanceDashbordWarehouseDC> WarehouseDataList { get; set; }
        public List<SalesPerformanceDashbordListDC> salesPerformanceDashbordListDCs { get; set; }
        public int TotalRecords { get; set; }
    }
    public class SalesPerformanceDashbordWarehouseDC
    {
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
        public double Sales { get; set; }
        public string SalesColour { get; set; }
        public double Dispatch { get; set; }
        public double BeatVisitPercent { get; set; }
        public int TC { get; set; }
        public string TCColour { get; set; }
        public int ProductiveCall { get; set; }
        public string PCColour { get; set; }
        public int ActiveRetailer { get; set; }
        public double AvgLineItem { get; set; }
        public int PerfactOrder { get; set; }
        public double Range { get; set; }
        public double ActualTimeSpend { get; set; }
    }
    public class ActualRouteCustomerDC
    {
        public int ExecutiveId { get; set; }
        public int CustomerId { get; set; }
        public bool IsVisited { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsBeat { get; set; }
    }
    public class PerformanceDashBoardData
    {
        public int ExecutiveId { get; set; }
        public int ActiveRetailer { get; set; }
        public double BeatVisitPercent { get; set; }
        public int TC { get; set; }
    }
    public class ExecutiveDatas
    {
        public int ExecutiveId { get; set; }
        public string DisplayName { get; set; }
        public string StoreName { get; set; }
        public string city { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ClusterName { get; set; }
    }
    public class SalesTargetDashboardReportDC
    {
        public string Salesperson { get; set; }
        public string Cluster { get; set; }
        public string Store { get; set; }
        public int Target { get; set; }
        public int IncentiveAmount { get; set; }
        public string SalesKpi { get; set; }
    }
    public class ProductiveCallDc
    {
        public int ProductiveCall { get; set; }
        public int ExtraCall { get; set; }
        public int TotalProductiveCall { get; set; }
    }
    public class ExecutiveAttendanceReportDateWiseDC
    {
        public string EmpCode { get; set; }
        public string ExecutiveName { get; set; }
        public DateTime Date { get; set; }
        public string Warehouse { get; set; }
        public string Store { get; set; }
        public string Cluster { get; set; }
        public string CityName { get; set; }
        public string ChannelName { get; set; }
        public string Day { get; set; }
        public DateTime? Check_In { get; set; }
        public DateTime? Check_Out { get; set; }
        public int TC { get; set; }
        public int PC { get; set; }
        public string TADA { get; set; }
        public string Attendance { get; set; }
        public string FullDay_HalfDay { get; set; }
    }
    public class GetCityListDc
    {
        public long Cityid { get; set; }
        public string CityName { get; set; }
        public string CreatedDate { get; set; }
        public bool IsDisable { get; set; }
        public bool IsShow { get; set; }
    }
    public class ExportSuccessStoreDC
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public double Target { get; set; }
        public string Store { get; set; }
        //public int Achivement { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public string KpiName { get; set; }
        //public string DisplayName { get; set; }
        public string Type { get; set; }
        public double Achievement { get; set; }
        public double AchievementPercent { get; set; }
        public double Earning { get; set; }
        /*



        public double Target { get; set; }
        public double Achievement { get; set; }
        public double AchievementPercent { get; set; }
        public double Earning { get; set; }
        */
    }
    public class ExportSuccessStoreFilter
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<int> WarehouseIds { get; set; }
        public List<int> ClusterId { get; set; }
        public int StoreId { get; set; }
    }
    public class InsertCatalogConfigDc
    {
        public long? CityId { get; set; }
        public float Frequency { get; set; }
        public float CustomerReach { get; set; }
        public float Amount { get; set; }
        public bool IsRepeat { get; set; }
        public List<ListingConfigurationDC> listingConfigurationDCs { get; set; }
    }
    public class ListingConfigurationDC
    {
        public long? Id { get; set; }
        public int Sequence { get; set; }
        public string ConfigName { get; set; }
        public bool? Status { get; set; }
        public int ItemCount { get; set; }
        public string Sort { get; set; }
        public bool? Unbilled { get; set; }
        public bool? IsScoreCheck { get; set; }
        public float? ScoreFrom { get; set; }
        public float? ScoreTo { get; set; }
        public bool? NewLaunch { get; set; }
        public bool? PromotionalItems { get; set; }
    }

    public class PromotionalSKUItemListDC
    {
        public int ItemId { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public int WarehouseId { get; set; }
        public double SellingPrice { get; set; }
        public double Margin { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public string ItemNumber { get; set; }
        public string ActiveStatus { get; set; }
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
    public class InsertProductCatalogDataDC
    {
        public long Id { get; set; }
        public string SectionName { get; set; }
        public string SectionHindiName { get; set; }
        public bool IsPromotional { get; set; }
        public string Type { get; set; }
        public long WarehouseId { get; set; }
        public long StoreId { get; set; }
        public int Sequence { get; set; }
        public string URL { get; set; }
        public string CustomList { get; set; }
        public List<itemNumber> ItemNumber { get; set; }
    }
    public class itemNumber
    {
        public int StoreId { get; set; }
        public int Sequence { get; set; }
        public string ItemNumber { get; set; }
    }
    public class BeatEditDC
    {
        public long BeatEditId { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public bool IsAnytime { get; set; }
        public int FromDate { get; set; }
        public int ToDate { get; set; }
    }
    public class AttendanceConfigCityDC
    {
        public long AttendaceConfigId { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
    public class ItemClassificationReportDc
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string StoreName { get; set; }
        public string WarehouseName { get; set; }
        public double SaleValue { get; set; }
        public long ItemIncentiveClassificationId { get; set; }
        public string ItemClassification { get; set; }
        public double CommissionPercentage { get; set; }
        public double Earning { get; set; }
    }
    public class ReportFilterDc
    {
        public int cityid { get; set; }
        public List<int> warehouseids { get; set; }
        public List<int> storeids { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
    public class SalesKpiDataDC
    {
        //Salesperson	Cluster	WarehouseName	Store	Target	IncentiveAmount	SalesKpi

        public int Month { get; set; }
        public int Year { get; set; }
        public string Salesperson { get; set; }
        public string Cluster { get; set; }
        public string WarehouseName { get; set; }
        public string Store { get; set; }
        public string Channel { get; set; }
        public string SalesKpi { get; set; }
        public double Target { get; set; }
        public double IncentiveAmount { get; set; }
    }
    public class DownloadTargetDataDC
    {
        public string SalesPerson { get; set; }
        public string Cluster { get; set; }
        public string Store { get; set; }
        public double Target { get; set; }
        public double IncentiveAmount { get; set; }
        public double? Achievement { get; set; }
        public string SalesKpi { get; set; }
    }
    public class uploadtargetDTOdc
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsMonth { get; set; }
        public int warehouseid { get; set; }
        public int storeid { get; set; }
    }
    public class DownloadTargetDC
    {
        public int ExecutiveId { get; set; }
        public string SalesPerson { get; set; }
        public string Cluster { get; set; }
        public string Store { get; set; }
        public double Target { get; set; }
        public double IncentiveAmount { get; set; }
        public double? Achievement { get; set; }
        public string SalesKpi { get; set; }
        public int WarehouseId { get; set; }
        public long StoreId { get; set; }
        public int ClusterId { get; set; }

    }
    public class ElascticTargetMTD
    {
        public double dispatchamt { get; set; }
        public int executiveid { get; set; }
        public int custCount { get; set; }
        public int storeid { get; set; }
        public int clusterid { get; set; }
    }
    public class SalesPersonKpiElasticSuccssStoreData
    {
        public string skcode { get; set; }
        public long storeid { get; set; }
        public int executiveid { get; set; }
        public double dispatchamount { get; set; }
        public double linecount { get; set; }
    }
    public class SalesPersonKpiAchivementResponse
    {
        public int ExecutiveId { get; set; }
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public double Target { get; set; }
        public double Achievement { get; set; }
        public double AchievementPercent { get; set; }
        public double Earning { get; set; }
    }
    public class PeopleList
    {
        public int PeopleID { get; set; }
        public string PeopleName { get; set; }
    }
    public class ClusterList
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
    }
    public class StoreList
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
    }
    public class WarehouseList
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
    }
    public class SalesKpiList
    {
        public long KpiId { get; set; }
        public string KpiName { get; set; }
    }
    public class ChannelList
    {
        public long ChannelMasterId { get; set; }
        public string ChannelName { get; set; }
    }
    public class SalesPersonKpiAndIncentiveAchivement
    {
        public long KPIId { get; set; }
        public string KpiName { get; set; }
        public string DisplayName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int StoreId { get; set; }
        public double Target { get; set; }
        public double AchievePercent { get; set; }
        public double IncentivePercent { get; set; }
        public string Type { get; set; }
        public string ExecutiveName { get; set; }
        public double IncentiveAmount { get; set; }
        public int ClusterId { get; set; }
        public int ExecutiveId { get; set; }
    }
    public class SKUItemListDC
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        //public string itemName { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string itemname { get; set; }
        public double MRP { get; set; }
        public string Number { get; set; }
        public bool IsActive { get; set; }
    }
    public class InsertTopSkUItems
    {
        public List<TopSKUsItemDc> topSKUsItemDcs { get; set; }
        public bool IsPromotional { get; set; }
    }
    public class TopSKUsItemDc
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public double UnitPrice { get; set; }
        public double Margin { get; set; }
        public string itemName { get; set; }
        public int StoreId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string WarehouseName { get; set; }
        public int SequenceNo { get; set; }
    }
    public class beatDSRRR
    {
        public List<int> WarehouseId { get; set; }
        public List<int> StoreId { get; set; }
        public List<long> ChannelMasterId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int skip { get; set; }
        public int take { get; set; }

    }
    public class peopleDataDc
    {
        public int PeopleID { get; set; }
        public string WarehouseName { get; set; }
        public string DisplayName { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public int WarehouseId { get; set; }
        public long ChannelMasterId { get; set; }
        public string ChannelType { get; set; }

    }
    public class BeatDSRReportResponseDCs
    {
        public List<salesDashboardTodayDC> salesTodayDC { get; set; }
        public List<SalesDashboardTodayMTDDataa> SalesDashboardTodayMTDData { get; set; }
        public int Totalcount { get; set; }
    }
    public class SalesDashboardTodayMTDDataa : SalesDashboardTodayMTDData 
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
    public class ExecutiveAttendanceReportDC
    {
        public int ExecutiveId { get; set; }
        public string EmpCode { get; set; }
        public string ExecutiveName { get; set; }
        public string Mobile { get; set; }
        public int WarehouseId { get; set; }
        public long StoreId { get; set; }
        public int ClusterId { get; set; }
        public int CityId { get; set; } //new add
        public string CityName { get; set; }
        public string Warehouse { get; set; }
        public string ChannelName { get; set; }
        public string Store { get; set; }
        public string Cluster { get; set; }
        public int TotalWorkingDays { get; set; }
        public double PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int TADA { get; set; }
        public long TotalRecords { get; set; }
    }
    public class ExecutiveAttendanceReportDCs
    {
        public long TotalRecords { get; set; }
        public List<ExecutiveAttendanceReportDC> executiveAttendanceLogDCs { get; set; }
    }
    public class ExecutiveAttendanceRowDetailsDC
    {
        public string EmployeeCode { get; set; }
        public string ExecuitveName { get; set; }
        public string Store { get; set; }
        public string Channel { get; set; }
        public string Warehouse { get; set; }
        public string Cluster { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public string SkCode { get; set; }
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        public string BeatCustomer { get; set; }
        public int OrderCount { get; set; }
    }
    public class AllExecutiveAttendanceReportFilter
    {
        public List<int> StoreIds { get; set; }
        public List<int> ChannelMasterId { get; set; }
        public List<int> CityIds { get; set; }
        public List<int> WarehouseIds { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class ExportAllExecutiveAttendenceForReportDC
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<int> StoreIDs { get; set; }
        public List<int> ChannelMasterId { get; set; }
        public List<int> WarehousIds { get; set; }
    }
    public class AttendanceRuleConfigDC
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public long ChannelMasterId { get; set; }
        public string StoreName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public bool IsCheckinBeatShop { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public bool IsCheckOutBeatShop { get; set; }
        public bool IsFullDayBeatShop { get; set; }
        public bool IsMinimumVisit { get; set; }
        public int? DayMinVisits { get; set; }
        public bool IsTADABeatShop { get; set; }
        public bool IsTADARequired { get; set; }
        public int? TADACalls { get; set; }
        public string Description { get; set; }
    }
    public class AttendanceRuleConfigLogDC
    {
        public long Id { get; set; }
        public long AttendanceRuleConfigId { get; set; }
        public bool IsCheckinBeatShop { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public bool IsCheckOutBeatShop { get; set; }
        public bool IsFullDayBeatShop { get; set; }
        public bool IsMinimumVisit { get; set; }
        public int? DayMinVisits { get; set; }
        public bool IsTADABeatShop { get; set; }
        public bool IsTADARequired { get; set; }
        public int? TADACalls { get; set; }
        public string Description { get; set; }


        public bool? IsCheckinBeatShopUpdated { get; set; }
        public bool? IsCheckInTimeUpdated { get; set; }
        public bool? IsCheckOutBeatShopUpdated { get; set; }
        public bool? IsFullDayBeatShopUpdated { get; set; }
        public bool? IsMinimumVisitUpdated { get; set; }
        public bool? DayMinVisitsUpdated { get; set; }
        public bool? IsTADABeatShopUpdated { get; set; }
        public bool? IsTADARequiredUpdated { get; set; }
        public bool? TADACallsUpdated { get; set; }
        public bool? DescriptionUpdated { get; set; }

        public DateTime CreatedDate { get; set; }
        public string Remark { get; set; }

    }
    public class AttendanceRuleEditLog
    {
        public List<AttendanceRuleConfigLogDC> AttendanceRuleConfigLog { get; set; }
        public long TotalRecords { get; set; }
    }
    public class LatestBeatReportDc
    {
        public string SkCode { get; set; }
        public string Day { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
    }
    public class OldBeatReportPostDc
    {
        public List<int> PeopleId { get; set; }
        public List<int> clusterIds { get; set; }
        public List<int> ChannelMasterIds { get; set; }
        public int CurrentExecutiveId { get; set; }
        public int StoreIds { get; set; }
        public int WarehouseId { get; set; }
    }
    public class OldExectuiveListDC
    {
        public long OldExecutiveId { get; set; }
        public string OldExecutiveName { get; set; }
        public string OldExecutiveEmpCode { get; set; }
        public long OldStoreId { get; set; }
        public long OldClusterId { get; set; }
    }
    public class BeatReportPostDc
    {
        public int PeopleId { get; set; }
        public List<int> clusterIds { get; set; }
        public List<int> StoreIds { get; set; }
        public List<int> ChannelMasterIds { get; set; }
    }
    public class ResetEditBeatDC
    {
        public int ExecutiveId { get; set; }
        public int StoreId { get; set; }
        public List<int> ClusterIds { get; set; }
    }
    public class ClusterExecutiveBeat
    {
        public string Skcode { get; set; }
        public int? BeatNumber { get; set; }
        public string Day { get; set; }
        public int ExecutiveId { get; set; }
        public int SkipDays { get; set; }
        public int SkipWeeks { get; set; }
        public int MonthWeek { get; set; }
        public long StoreId { get; set; }
        public string EvenOrOddWeek { get; set; }
    }
    public class ValidatingAssignBeatDc
    {
        public List<ClusterExecutiveBeat> ClusterExecutiveBeat { get; set; }
        public List<int?> clusterIds { get; set; }
        public List<long> ChannelMasterIds { get; set; }

    }
    public class MappedCustomerOnClusterDc
    {
        public int? BeatNumber { get; set; }
        public string Day { get; set; }
        public string Skcode { get; set; }
        public int? SkipDays { get; set; }
        public int? SkipWeeks { get; set; }
        public string EvenOrOddWeek { get; set; }
        public int MonthWeek { get; set; }
    }
    public class SearchMappedExeOnClusterDc
    {
        public int ExecutiveId { get; set; }
        public List<int> clusterIds { get; set; }
        public List<int> ChannelMasterIds { get; set; }
        public long StoreId { get; set; }
    }
    public class StoreClusterExecutiveDc
    {
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ExecutiveName { get; set; }
        public int ExecutiveId { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }
        public int NoOfBeat { get; set; }
    }
    public class SearchStoreClusterDc
    {
        public List<int> ClusterIds { get; set; }
        public long StoreId { get; set; }
    }
    public class SearchMappedStoreClusterDc
    {
        public List<int> clusterIds { get; set; }
        public long StoreId { get; set; }
    }
    public class WarehouseClusterDc
    {
        public int WarehouseId { get; set; }
        public int ClusterId { get; set; }
        public string ClusterName { get; set; }

    }
    public class ExecutiveMappingDC
    {
        public int ExecutiveId { get; set; }
        public string ExecutiveName { get; set; }
        public string Empcode { get; set; }
        public string WarehouseName { get; set; }
        public string StoreName { get; set; }
        public string ClusterName { get; set; }
        public string ChannelName { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
    public class AddChannel
    {
        public string SkCode { get; set; }
        public List<int> StoreId { get; set; }
        public long ChannelId { get; set; }
    }
    public class PaylaterDashboardDc
    {
        public double OverDue { get; set; }
        public double DueToday { get; set; }
        public double TotalDue { get; set; }
    }
    public class PaylaterDashboardDetailDc
    {
        public int CustomerId { get; set; }
        public string SkCode { get; set; }
        public int DaysOverDue { get; set; }
        public double TotalDue { get; set; }
        public double OverDue { get; set; }
    }
    public class CustomerPaylaterDetailDc
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string SkCode { get; set; }
        public string ShopName { get; set; }
        public string ShippingAddress { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string Mobile { get; set; }
        public int WarehouseId { get; set; }
        public string CustomerVerify { get; set; }
        public bool Active { get; set; }
        public bool IsKpp { get; set; }
        public string CustomerType { get; set; }
        public bool IsBeat { get; set; }
        public int DaysOverDue { get; set; }
        public double TotalDue { get; set; }
        public double OverDue { get; set; }
    }
    public class PaylaterDashboardFilter
    {
        public int ExecutiveId { get; set; }
        public int CustomerId { get; set; }
        public string KeyWord { get; set; }
        public string Filter { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class PayLaterCollection
    {
        public int OrderId { get; set; }
        public int peopleId { get; set; }
        public double amount { get; set; }
    }
    public class CustomerPaylaterDue
    {
        public bool IsPaylaterDue { get; set; }
        public double TotalDue { get; set; }
        public double OverDue { get; set; }
    }
}

