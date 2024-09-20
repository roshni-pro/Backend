
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using AngularJSAuthentication.Model;


namespace AngularJSAuthentication.DataContracts.KPPApp
{
    public class paymentrequest
    {
        public int customerId { get; set; }
        public double amount { get; set; }
        public string comment { get; set; }
        public string GatewayRequest { get; set; }
        public string PaymentFrom { get; set; }

    }

    
    public class paymentpendingrequest
    {
        public long id { get; set; }
        public int customerId { get; set; }
        public double amount { get; set; }
        public string comment { get; set; }
        public string GatewayRequest { get; set; }
        public string PaymentFrom { get; set; }
        public string GullakImage { get; set; }
        public string GatewayTransId { get; set; }

    }


    public class gullak
    {
        public double Amount { get; set; }
        public bool status { get; set; }
    }

    public class paymentresponce
    {
        public long id { get; set; }
        public int CustomerId { get; set; }
        public string GatewayRequest { get; set; }
        public string GatewayResponse { get; set; }
        public string GatewayTransId { get; set; }
        public string PaymentFrom { get; set; }
        public string status { get; set; }
    }

    public class signup
    {
        public string Mobile { get; set; }
        public string TypeOfBusiness { get; set; }
        public string FirmName { get; set; }
        public string City { get; set; }
        public string Name { get; set; }
        public string ShopName { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string RefNo { get; set; }
        public string Password { get; set; }
        public string fcmId { get; set; }
        public string CurrentAPKversion { get; set; }
        public string imei { get; set; }
        public string PhoneOSversion { get; set; }
        public string UserDeviceName { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string LicenseNumber { get; set; }
        public string UploadRegistration { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IfscCode { get; set; }
        public string Emailid { get; set; }
        public string LandMark { get; set; }
        public string Shopimage { get; set; }
        public string UploadGSTPicture { get; set; }
        public string deviceId { get; set; }
        public string ZipCode { get; set; }
        public string UploadProfilePichure { get; set; }
        public string Type { get; set; }
        public string OtherNo { get; set; }
        public string UploadLicensePicture { get; set; }
        public string NameOnGST { get; set; }

        public string BillingState { get; set; }
        public string BillingCity { get; set; }
        public string BillingZipCode { get; set; }

    }

    public class Login
    {
        public string CurrentAPKversion { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; }
        public string PhoneOSversion { get; set; }
        public string UserDeviceName { get; set; }
        public string deviceId { get; set; }
        public string fcmId { get; set; }
        public string imei { get; set; }
        
    }

    public class loggedcust 
    {
        public string MobileNumber { get; set; }
        public bool IsOTPverified { get; set; }
        public string fcmid { get; set; }
        public string CurrentAPKversion { get; set; }
        public string PhoneOSversion { get; set; }
        public string UserDeviceName { get; set; }
        public string IMEI { get; set; }
    }

    public class profile
    {
        public customers customer { get; set; }
        public distributor distributor { get; set; }
        public bool status { get; set; }
    }

    public class customers
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Shopimage { get; set; }
        public int? Warehouseid { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string BillingAddress { get; set; }
        public string TypeOfBuissness { get; set; }
        public string FirmName { get; set; }
        public string ShippingAddress { get; set; }
        public string LandMark { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public int? Cityid { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string RefNo { get; set; }
        public string FSAAI { get; set; }       
        public string Emailid { get; set; }
        public double MonthlyTurnOver { get; set; }        
        public string SizeOfShop { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public bool IsResetPasswordOnLogin { get; set; }
        public string UploadProfilePichure { get; set; }
        public bool Active { get; set; }
        public bool IsSignup { get; set; }
        public bool IsKPP { get; set; }
        public DateTime DOB { get; set; }
        public DateTime AnniversaryDate { get; set; }
        public string UploadLicensePicture { get; set; }
        public string UploadGSTPicture { get; set; }
        public string LicenseNumber { get; set; }
        public string UploadRegistration { get; set; }
        public string WhatsappNumber { get; set; }
        public string Type { get; set; }
        public string AadharNo { get; set; }
        public int CustomerDocTypeMasterId { get; set; }
        public ApkNamePwdResponse RegisteredApk { get; set; }
        public string  WarehouseAddress { get; set; }
        public double WarehouseLat { get; set; }
        public double WarehouseLong { get; set; }

    }
    public class ApkNamePwdResponse
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
   
    //public class updation 
    //{
    //    public string Signature { get; set; }
    //    public string Createddate { get; set; }
    //    public string FirmName { get; set; }
    //    public string BillingAddress { get; set; }
    //    public string oneyear { get; set; }
    //    public string City { get; set; }
    //    public string CustomerName { get; set; }
    //    public string Mobile { get; set; }
    //    public string Skcode { get; set; }
    //}

    public class updateCustomerProfile
    {
        public DateTime DOB { get; set; }
        public string Name { get; set; }
        public string ShopName { get; set; }
        public string UploadGSTPicture { get; set; }
        public DateTime AnniversaryDate { get; set; }
        public string AreaName { get; set; }
        public string BillingAddress { get; set; }
        public int customerID { get; set; }
        public string Emailid { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string LicenseNumber { get; set; }
        public string Mobile { get; set; }
        public string RefNo { get; set; }
        public string ShippingAddress { get; set; }
        public string Shopimage { get; set; }
        public string UploadRegistration { get; set; }
        public string WhatsappNumber { get; set; }
        public string UploadProfilePichure { get; set; }
        public string Type { get; set; }
        public string OtherNo { get; set; }
        public string UploadLicensePicture { get; set; }
        public string PanNo { get; set; }
        public string AadharNo { get; set; }
        public string AadharPicture { get; set; }
        public string PanPicture { get; set; }
        public int CustomerDocTypeMasterId { get; set; }
    }

    public class distributor
    {
        public string  TypeOFBusiness { get; set; }
        public string FirmName { get; set; }
        public int customerID { get; set; }
        public string manpower { get; set; }
        public decimal advanceAmount { get; set; }
        public bool franchiseeKKP { get; set; }
        public string sourceofFund { get; set; }
        public string drugLicenseNo { get; set; }
        public string drugLicenseValidity { get; set; }
        public string drugLicense { get; set; }
        public string foodLicenseNo { get; set; }
        public string foodLicense { get; set; }
        public string gstNo { get; set; }
        public string NameOnGST { get; set; }
        public string gst { get; set; }
        public string panNo { get; set; }
        public string pan { get; set; }
        public string aadharNo { get; set; }
        public string aadhar { get; set; }
        public bool warehouseFacility { get; set; }
        public string warehouseSize { get; set; }
        public bool deliveryVehicle { get; set; }
        public string deliveryVehicleNo { get; set; }
        public string blankCheque { get; set; }
        public string signature { get; set; }
        public string status { get; set; }
        public bool isVerified { get; set; }
        public string SignedPdf { get; set; }
    }
    public class temOrderQBcodeD
    {
        public byte[] BarcodeImage { get; set; }
    }
    public class DistributorPrice
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int MultiMRPID { get; set; }
        public string ItemNumber { get; set; }
        public int MOQ { get; set; }
        public double Price { get; set; }
        public bool IsdistributorShow { get; set; }
        public double? DistributionPrice { get; set; }        
    }

    public class DistributerRole
    {
        public int PeopleID { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
    }

    public class DistributorPriceResponse
    {
        public List<DistributorPrice> DistributorPriceList { get; set; }
        public DistributerRole DistributerRole { get; set; }
    }

    public class searchitem
    {
        public int WarehouseId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string SearchItem { get; set; }
    }

    public class Categorydc
    {
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
    }
    public class SubCategorydc
    {
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
    }
    public class SubSubCategorydc
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
    }

    public class PlacedOrderMasterDTM
    {
        public int OrderId { get; set; }
        public double TotalAmount { get; set; }
        public int WheelCount { get; set; }
        public double WheelAmountLimit { get; set; }
        public int DialEarnigPoint { get; set; }
        public string Skcode { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public int? PlayedWheelCount { get; set; }
        public string RSAKey { get; set; }
        public string HDFCOrderId { get; set; }
        public string eplOrderId { get; set; }
    }


    public class CustomerKPPDc
    {
        public int CustomerId { get; set; }
        public int? CompanyId { get; set; }
        public int? CustomerCategoryId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public int? Warehouseid { get; set; }
        public string Mobile { get; set; }
        public string MobileSecond { get; set; }
        public string WarehouseName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public string CustomerType { get; set; }
        public string CustomerCategoryName { get; set; }
        public string BillingAddress { get; set; }
        public string TypeOfBuissness { get; set; }
        public string ShippingAddress { get; set; }
        public string LandMark { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public int? Cityid { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string BAGPSCoordinates { get; set; }
        public string SAGPSCoordinates { get; set; }
        public string RefNo { get; set; }
        public string FSAAI { get; set; }
        public string Type { get; set; }
        public string OfficePhone { get; set; }
        public string Emailid { get; set; }
        public string Familymember { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string imei { get; set; }
        public double MonthlyTurnOver { get; set; }
        public int? ExecutiveId { get; set; }
        public string SizeOfShop { get; set; }
        public int? Rating { get; set; }
        public int? ClusterId { get; set; }
        public string ClusterName { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string Day { get; set; }
        public int DivisionId { get; set; }
        public int? BeatNumber { get; set; }
        public string Rewardspoints { get; set; }
        public string fcmId { get; set; }
        public string BranchName { get; set; }
        public string IfscCode { get; set; }
        public string AadharNo { get; set; }
        public string PanNo { get; set; }
        public string ResidenceAddressProof { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? AnniversaryDate { get; set; }
        public string WhatsappNumber { get; set; }
        public string UploadRegistration { get; set; }
        public string UploadProfilePichure { get; set; }
        public string UploadLicensePicture { get; set; }
        public string UploadGSTPicture { get; set; }
        public string LicenseNumber { get; set; }
        public string Shopimage { get; set; }
        public bool IsSignup { get; set; }
        public double CustomerRating { get; set; }
        public string CustomerRatingCommnets { get; set; }
        public bool IsCityVerified { get; set; }
        public int? UdharLimit { get; set; }
        public int? UdharLimitRemaining { get; set; }
        public bool IsKPP { get; set; }
        public int? KPPWarehouseId { get; set; }
        public string CurrentAPKversion { get; set; }
        public string PhoneOSversion { get; set; }
        public string UserDeviceName { get; set; }
        public string CustomerVerify { get; set; }
        public string StatusSubType { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public string ShippingAddress1 { get; set; }
        public string BillingAddress1 { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string ReferralSkCode { get; set; }
        public bool? IsReferral { get; set; }
        public bool check { get; set; }
        public String Exception { get; set; }
        public string level { get; set; }
        public int ordercount { get; set; }
        public int thisordercount { get; set; }

        public int thisordercountCancelled { get; set; }

        public int thisordercountRedispatch { get; set; }

        public int thisordercountdelivered { get; set; }

        public int thisordercountpending { get; set; }

        public double thisordervalue { get; set; }

        public int thisRAppordercount { get; set; }

        public int thisSAppordercount { get; set; }
        public int? Areaid { get; set; }
        public string AreaName { get; set; }
        public int CustSupplierid { get; set; }
        public int userid { get; set; }
        public bool inTally { get; set; }
        public string AgentCode { get; set; }
        public object Decription { get; set; }
        public bool IsResetPasswordOnLogin { get; set; }

        public string FirstTimeRegOTPCode { get; set; }
        public bool InRegion { get; set; } //tejas 09-07-2019
        public ApkNamePwdResponse RegisteredApk { get; set; }
        public bool IsHide { get; set; }
        public string ExecutiveName { get; set; }
        public string deviceId { get; set; }

        public string CustomerLevel { get; set; } //Praveen

        public string ColourCode { get; set; } //Praveen
        public bool IsChequeAccepted { get; set; } //Praveen
        public double ChequeLimit { get; set; } //

    }

    public class customerDetails
    {
        public customers customers { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public decimal AdvanceAmount { get; set; }
        public bool IsVerified { get; set; }
    }

    public class SubsubCategoryDT
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

    public class FilterSearchDcs
    {
        public List<Itemsearchs> MostSellingProduct { get; set; }
        public List<Itemsearchs> RecentPurchase { get; set; }
        public List<Itemsearchs> CustFavoriteItem { get; set; }
        public List<Itemsearchs> RecentSearchItem { get; set; }
    }

    public class Itemsearchs
    {
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool IsItemLimit { get; set; }
        public int ItemlimitQty { get; set; }
        public string Number { get; set; }
        public string itemname { get; set; }
        public string UnitofQuantity { get; set; }
        public string UOM { get; set; }
        public string LogoUrl { get; set; }
        public double price { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int Categoryid { get; set; }
        public int BaseCategoryId { get; set; }
        public string CategoryName { get; set; }
        public int WarehouseId { get; set; }
        public int CompanyId { get; set; }
        public double UnitPrice { get; set; }
        public string HindiName { get; set; }
        public int? marginPoint { get; set; }
        public int? promoPerItems { get; set; }
        public bool IsOffer { get; set; }
        public int? OfferCategory { get; set; }
        public DateTime? OfferStartTime { get; set; }
        public DateTime? OfferEndTime { get; set; }
        public double? OfferQtyAvaiable { get; set; }
        public double? OfferQtyConsumed { get; set; }
        public int OfferId { get; set; }
        public string OfferType { get; set; }
        public double? OfferWalletPoint { get; set; }
        public int? OfferFreeItemId { get; set; }
        public double? OfferPercentage { get; set; }
        public string OfferFreeItemName { get; set; }
        public string OfferFreeItemImage { get; set; }
        public int? OfferFreeItemQuantity { get; set; }
        public int OfferMinimumQty { get; set; }
        public double? FlashDealSpecialPrice { get; set; }
        public int? OfferMaxQtyPersonCanTake { get; set; }
        public double NetPurchasePrice { get; set; }
        public double TotalTaxPercentage { get; set; }
        public int? dreamPoint { get; set; }
        public bool IsSensitive { get; set; }
        public bool IsSensitiveMRP { get; set; }
        public string itemBaseName { get; set; }
        public int MinOrderQty { get; set; }
        public double? DistributionPrice { get; set; }
        public bool DistributorShow { get; set; }
        public int ItemAppType { get; set; }
        public bool IsNotSell { get; set; }
        public string SellingSku { get; set; }
    }
    public class KPPData
    {
        public int CustomerId { get; set; }
        public int? CompanyId { get; set; }
        public string Skcode { get; set; }
        public int? Warehouseid { get; set; }
        public string Mobile { get; set; }
      
    }
      public class responsemsg
        {
            public bool IsCityVerified { get; set; }
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
    public class SubCategories
    {
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string SubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int itemcount { get; set; }
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

    public class FlashDealWithItems
    {
        public int FlashDealId { get; set; }
        public int ItemId { get; set; }

    }

    public class WRSITEMS
    {
        public List<factoryItemdatas> ItemMasters { get; set; }
        public List<factorySubSubCategorys> SubsubCategories { get; set; }
        public bool Message { get; set; }
    }

    public class WRSITEMN
    {
        public List<factoryItemdat> ItemMasters { get; set; }
        public List<factorySubSubCategorys> SubsubCategories { get; set; }
        public bool Message { get; set; }
    }


    public class factoryItemdatas
    {
        internal bool active;
        public int ItemId { get; set; }
        public bool IsItemLimit { get; set; }
        public int ItemlimitQty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string ItemNumber { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
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
        public double? marginPoint { get; internal set; }
        public int? promoPerItems { get; internal set; }
        public int? dreamPoint { get; internal set; }
        public bool IsOffer { get; set; }        
        public bool Deleted { get; internal set; }
        public double NetPurchasePrice { get; set; }
        public bool IsSensitive { get; set; }//sudhir
        public bool IsSensitiveMRP { get; set; }//sudhir
        public string UnitofQuantity { get; set; }//sudhir
        public string UOM { get; set; }//sudhir
        public string itemBaseName { get; set; }//sudhir

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

        public int BillLimitQty { get; set; }
        public double? DistributionPrice { get; set; }
        public bool DistributorShow { get; set; }
        public int ItemAppType { get; set; }
        public DateTime?  UpdatedDate { get; set; }
        public bool IsNotSell { get; set; }
    }
    public class factorySubSubCategorys
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public List<factoryItemdatas> ItemMasters { get; set; }
    }
    public class factoryItemdat
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
        public bool IsNotSell { get; set; }

    }
    public class CustomerUpdateDC
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
        public int WarehouseId { get; set; }
    }

    public class pwcdetail
    {
        public int CustomerId { get; set; }
        public string currentpassword { get; set; }
        public string newpassword { get; set; }
    }

    public class custdata
    {
        public int CustomerId { get; set; }
        public int? CompanyId { get; set; }
        public string Skcode { get; set; }
        public int? Warehouseid { get; set; }
        public string Mobile { get; set; }
        public ApkNamePwdResponse RegisteredApk { get; set; }
    }

    public class CatScatSscats
    {
        //public IEnumerable<Category> Categories { get; set; }
        //public IEnumerable<SubCategory> SubCategories { get; set; }
        //public IEnumerable<SubsubCategory> SubSubCategories { get; set; }

        public IEnumerable<Categories> Categories { get; set; }
        public IEnumerable<SubCategories> SubCategories { get; set; }
        public IEnumerable<SubSubCategories> SubSubCategories { get; set; }
    }
    public class getChequeallcustomer
    {
        public int CustomerId  { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public bool IsChequeAccepted { get; set; }
        public double ChequeLimit { get; set; }
        public string WarehouseName { get; set; }

    }
    public class updateChequeallcustomer
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public bool IsChequeAccepted { get; set; }
        public double ChequeLimit { get; set; }
        public string WarehouseName { get; set; }

    }

    public class DistributorMinOrder
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public int MinOrderValue { get; set; }
        public int MaxCODValue { get; set; }
    }



    public class OfferDc
    {
        [Key]
        public int OfferId { get; set; }
        public int CompanyId { get; set; }
        public int WarehouseId { get; set; }
        public int CityId { get; set; }
        public string OfferName { get; set; }
        public string OfferCode { get; set; }

        public string OfferCategory
        {
            get; set;
        }

        public string FreeOfferType { get; set; }  //Offer or FlashDeal

        public string OfferOn { get; set; }  //Item,Category,Brand ,
        public string OfferVolume { get; set; } // Single , OrderVolume,NoOfLineItems

        public int itemId { get; set; }
        public string itemname { get; set; }
        public int MinOrderQuantity { get; set; }
        public int NoOffreeQuantity { get; set; }
        public int FreeItemId { get; set; }

        public DateTime start { get; set; }
        public DateTime end { get; set; }

        public double QtyAvaiable { get; set; }  //This will be application on Flash Deals
        public double QtyConsumed { get; set; }

        public int MaxQtyPersonCanTake { get; set; }


        public string FreeItemName { get; set; }

        public double FreeItemMRP
        {
            get; set;
        }

        public int FreeWalletPoint
        {
            get; set;
        }
        public bool OfferWithOtherOffer
        {
            get; set;
        }
        public double DiscountPercentage
        {
            get; set;
        }

        public double BillAmount { get; set; }  // Bill Amount       
        public double MaxBillAmount { get; set; }
        public double MaxDiscount { get; set; }

        public int LineItem { get; set; }
        // Max Discount
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public string OfferLogoUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsOfferOnCart
        {
            get; set;
        }

        public string BillDiscountOfferOn
        {
            get; set;
        }

        public double? BillDiscountWallet
        {
            get; set;
        }
        // For Bill Discount use by Anushka  & Harry 17/06/2019
        public bool IsMultiTimeUse { get; set; }
        public bool IsUseOtherOffer { get; set; }
        public int? GroupId { get; set; } //Contain list of CustomerGroupId 
        //by sachin
        
        [NotMapped]
        public int CustomerId { get; set; }
        [NotMapped]
        public int userid { get; set; }

        public int? FreeItemLimit { get; set; }
        public int? OffersaleQty { get; set; }

        public int? Category { get; set; }
        public int? subCategory { get; set; }
        public int? OfferUseCount { get; set; }
        public int? subSubCategory { get; set; }
        [StringLength(150)]
        public string BillDiscountType { get; set; }
        [StringLength(100)]
        public string OfferAppType { get; set; }
        [StringLength(100)]
        public string ApplyOn { get; set; }
        [StringLength(100)]
        public string WalletType { get; set; }
        public bool IsDispatchedFreeStock //  Freeitem dispatched from Freestock (true)or currentstock (false)
        {
            get; set;
        }
    }

    public class DistributorVerificationDc
    {
        public int CustomerID { get; set; }
        
        public string TypeOfBusiness { get; set; }
      
        public string FirmName { get; set; }
        public string Manpower { get; set; }
        public decimal AdvanceAmount { get; set; }
        public bool FranchiseeKKP { get; set; }
        public string SourceofFund { get; set; }
       
        public string DrugLicenseNo { get; set; }
        public string DrugLicenseValidity { get; set; }
        
        public string DrugLicense { get; set; }
        
        public string FoodLicenseNo { get; set; }
       
        public string FoodLicense { get; set; }
       
        public string GSTNo { get; set; }
        
        public string GST { get; set; }
        
        public string PANNo { get; set; }
        
        public string PAN { get; set; }
        
        public string AadharNo { get; set; }
        
        public string Aadhar { get; set; }
        public bool WarehouseFacility { get; set; }
        public string WarehouseSize { get; set; }
        public bool DeliveryVehicle { get; set; }
        public string DeliveryVehicleNo { get; set; }
        public string BlankCheque { get; set; }
        
        public string Signature { get; set; }
       
        public string Status { get; set; }
        public bool IsVerified { get; set; }

        public string SignedPdf { get; set; }
        public string Type { get; set; }
        public string OtherNo { get; set; }
        public string UploadLicensePicture { get; set; }
    }

    public class RDSResponse
    {
        public bool Status { get; set; }
        public string message { get; set; }
    }

    public class RDSCustomerOTPVerified
    {
        public string MobileNumber { get; set; }
        public string Otp { get; set; }
       
    }

    public class RDSCustomerRegistor
    {
        public string MobileNumber { get; set; }       
        public string ShopName { get; set; }
        public string Shopimage { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string Name { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }
        public string AreaName { get; set; }
        
    }


    public class RDSCustomerAddress
    {
        public int CustomerId { get; set; }
        public string RefNo { get; set; }
        public string LicenseNumber { get; set; }
        public string AadharNo { get; set; }
        public string PanNo { get; set; }
        public string UploadLicensePicture { get; set; }
        public string UploadGSTPicture { get; set; }
        public string AadharPicture { get; set; }
        public string PanPicture { get; set; }
        public int CustomerDocTypeMasterId { get; set; }
    }
}
