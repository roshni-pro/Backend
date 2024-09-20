

    using AngularJSAuthentication.DataContracts.Masters;
    using AngularJSAuthentication.Model;
    using AngularJSAuthentication.Model.NotMapped;
    using GenricEcommers.Models;
    using MongoDB.Bson;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace AngularJSAuthentication.DataContracts.External
    {
        public class RetailerAppDC
        {
            public int CustomerId { get; set; }
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public DateTime? DOB { get; set; }
            public int? Warehouseid { get; set; }
            public string Mobile { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            public string CustomerType { get; set; }
            public int? CompanyId { get; set; }
            public string BillingAddress { get; set; }
            public string BillingState { get; set; }
            public string BillingCity { get; set; }
            public string BillingZipCode { get; set; }
            public string ShippingAddress { get; set; }
            public string ShippingAddress1 { get; set; }
            public string LandMark { get; set; }
            public int? Cityid { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string RefNo { get; set; }
            public string UploadGSTPicture { get; set; }
            public string LicenseNumber { get; set; }
            public string Emailid { get; set; }
            public bool Active { get; set; }
            public bool IsSignup { get; set; }
            public string UploadProfilePichure { get; set; }
            public string AreaName { get; set; }
            public DateTime? AnniversaryDate { get; set; }
            public string WhatsappNumber { get; set; }
            public string UploadRegistration { get; set; }
            public string Shopimage { get; set; }
            public double lat { get; set; }
            public double lg { get; set; }
            public double? Shoplat { get; set; }
            public double? Shoplg { get; set; }
            public bool IsResetPasswordOnLogin { get; set; }
            public string deviceId { get; set; }
            public string fcmId { get; set; }
            public string CurrentAPKversion { get; set; }
            public string PhoneOSversion { get; set; }
            public string UserDeviceName { get; set; }
            public string CustomerVerify
            {
                get; set;
            }
            public string imei { get; set; }
            public int? ClusterId { get; set; }
            public ApkNamePwdResponse APKType { get; set; }
            //public ApkNamePwdResponse RegisteredApk { get; set; }
            public bool TrueCustomer { get; set; }

            public bool IsPrimeCustomer { get; set; }
            public DateTime PrimeStartDate { get; set; }
            public DateTime PrimeEndDate { get; set; }
            public bool IsKPP { get; set; }
            public string AadharNo { get; set; } // by sudhir
            public string PanNo { get; set; }// by sudhir
            public string NameOnGST { get; set; }
            public bool IsContactsRead { get; set; }
            public long CustomerDocTypeMasterId { get; set; }
            public bool IsRequiredLocation { get; set; }
            public string ReferralSkCode { get; set; }
            public CustomerAddress CustomerAddress { set; get; }

            public DateTime? LicenseExpiryDate { get; set; }

            public bool IsWarehouseLive { get; set; }
            public bool IsSelleravailable { get; set; }
            public string otp { get; set; }
            public string WarehouseName { get; set; }
            public string ClusterName { get; set; }
        }

        public class CustDetails
        {
            public RetailerAppDC customers { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
		    public bool NotAuthorize { get; set; }
		    public string CriticalInfoMissingMsg { get; set; }
            public bool LogOutFromThisDevice { get; set; }
            public bool IsUdharOverDue { get; set; }


        
        }
        public class Responsemsg
        {
            public bool IsCityVerified { get; set; }
        }
        public class CityDC
        {
            public int SequenceNo { get; set; }
            public int Cityid { get; set; }
            public string CityName { get; set; }
            public double CityLatitude { get; set; }
            public double CityLongitude { get; set; }
        }
        public class CustGstVerify
        {
            public int id { get; set; }
            public string RefNo { get; set; }
            public string ShopName { get; set; }
            public string Name { get; set; }
            public string BillingAddress { get; set; }
            public string ShippingAddress { get; set; }
            public string LandMark { get; set; }
            public string Country { get; set; }
            public string State { get; set; }
            public int? Cityid { get; set; }
            public string City { get; set; }
            public DateTime CreatedDate { get; set; }         
            public string Zipcode { get; set; }
            public string Active { get; set; }
            public string lat { get; set; }
            public string lg { get; set; }

        }
        public class CustGstDTOList
        {
            public CustGstVerify custverify { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }


        public class MessageDetails
        {

            public bool Status { get; set; }
            public string Message { get; set; }

        }
        public class ApkNamePwdResponseDC
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
        public class CustUpdateDetails
        {
            public RetailerAppDC customers { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }

        public class companydetails
        {
            public CompanyDetails CompanyDetails { get; set; }

            public bool Status { get; set; }
            public string Message { get; set; }

        }
        public class consumercompanydetail
        {
            public ConsumerCompanyDetails CompanyDetails { get; set; }

            public bool Status { get; set; }
            public string Message { get; set; }

        }
        public class custFavoriteItem
        {
            // public CustFavoriteItem CustFavoriteItem { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }

        }
        public class SalesAppDefaultCustomersDC
        {
            public int DefaultSalesSCcustomerId { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }

        }
        public class AppHomeSectionsDsc
        {
            public int SectionID { get; set; }
            public string SectionName { get; set; }
            public bool IsTile { get; set; }
            public string SectionType { get; set; }
            public bool IsBanner { get; set; }
            public bool IsPopUp { get; set; }
            public int Sequence { get; set; }
            public int RowCount { get; set; }
            public int ColumnCount { get; set; }
            public bool HasBackgroundColor { get; set; }
            public string TileBackgroundColor { get; set; }
            public bool Deleted { get; set; }
            public bool Active { get; set; }
            public string BannerBackgroundColor { get; set; }
            public string TileHeaderBackgroundColor { get; set; }
            public string TileBackgroundImage { get; set; }
            public bool HasHeaderBackgroundImage { get; set; }
            public string TileHeaderBackgroundImage { get; set; }
            public bool?  IsSingleBackgroundImage { get; set; } ///by simran 10/19/2023
            public string TileAreaHeaderBackgroundImage { get; set; }///by sudhir 07/11/2019
            public string HeaderTextColor { get; set; }///by sudhir 07/11/2019
            public int HeaderTextSize { get; set; }        
            public string sectionBackgroundImage { get; set; }///by sudhir 07/11/2019
            public bool IsTileSlider { get; set; }//by sudhir 26/11/2019      
            public string SectionHindiName { get; set; }
            public string ViewType { get; set; }
            public string WebViewUrl { get; set; }
            public string SectionSubType { get; set; }       
            public List<AppHomeSectionItemsDC> AppItemsList { get; set; }
        }

        public class AppHomeSectionItemsDC
        {
            public string TileName { get; set; }
            public string TileImage { get; set; }
            public string BannerImage { get; set; }
            public string BannerName { get; set; }
            public string DynamicHeaderImage { get; set; }

            public string RedirectionType { get; set; }
            public string RedirectionUrl { get; set; }       
            public int RedirectionID { get; set; }
            public int BaseCategoryId { get; set; }
            public int CategoryId { get; set; }
            public int SubCategoryId { get; set; }
            public int SubsubCategoryId { get; set; }
            public string TileSectionBackgroundImage { get; set; }
            public DateTime? OfferStartTime { get; set; }
            public DateTime? OfferEndTime { get; set; }
            public bool Expired { get; set; }
            public bool Deleted { get; set; }
            public string BannerActivity { get; set; }

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

        public class orderMrpData
        {
            public DateTime CreatedDate { get; set; }
            public int ItemMultiMRPId { get; set; }
            public int Qty { get; set; }
        }

        public class LastPOOrderData
        {
            public int OrderId { get; set; }
        }

        public class LastPOOrderItemNumberData
        {
            public string ItemNumber { get; set; }
        }

        public class ItemListDc
        {
            public List<ItemDataDC> ItemMasters { get; set; }
            public int TotalItem { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
        }


        public class SubsubCategoryDc
        {
            public int SubsubCategoryid { get; set; }
            public string SubsubcategoryName { get; set; }
            public string LogoUrl { get; set; }
            public int Categoryid { get; set; }
            public string HindiName { get; set; }
            public int SubCategoryId { get; set; }

        }

        public class CategoryImageDC
        {
            public int CategoryImageId { get; set; }
            public string CategoryImg { get; set; }

        }

        public class CategoryDCs
        {
            public int Categoryid { get; set; }
            public string CategoryName { get; set; }
            public string LogoUrl { get; set; }
            public int itemcount { get; set; }
            public string CategoryImg { get; set; }
        }

        public class SubCategoryDCs
        {
            public int SubCategoryId { get; set; }
            public int Categoryid { get; set; }
            public string CategoryName { get; set; }
            public string SubcategoryName { get; set; }
            public string LogoUrl { get; set; }
            public int itemcount { get; set; }
            public string StoreBanner { get; set; }
        }
        public class SubsubCategoryDcs
        {
            public int SubsubCategoryid { get; set; }
            public string SubsubcategoryName { get; set; }
            public int BaseCategoryId
            {
                get; set;
            }
            public int Categoryid { get; set; }
            public int SubCategoryId { get; set; }
            public string LogoUrl { get; set; }
            public int itemcount { get; set; }
        }


        public class BrandSubCategoryDC
        {
            public int BaseCategoryId { get; set; }
            public int SubCategoryId { get; set; }
            public int Categoryid { get; set; }
            public int SubsubCategoryid { get; set; }        
            public string CategoryName { get; set; }
            public string SubcategoryName { get; set; }
            public string SubsubcategoryName { get; set; }
            public string SubSubCategoryLogoUrl { get; set; }
            public string SubCategoryLogoUrl { get; set; }
            public string CategoryLogoUrl { get; set; }      
            public int itemcount { get; set; }
        }
        public class CatScatSscatDCs
        {
            public IEnumerable<CategoryDCs> categoryDC { get; set; }
            public IEnumerable<SubCategoryDCs> subCategoryDC { get; set; }
            public IEnumerable<SubsubCategoryDcs> subsubCategoryDc { get; set; }
        }

        public class customeritemsdc
        {
            public People ps { get; set; }
            public Customer cs { get; set; }
            public IEnumerable<Basecats> Basecats { get; set; }
            public List<Categories> Categories { get; set; }
            public IEnumerable<SubCategories> SubCategories { get; set; }

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
        }
        public class SubCategories
        {
            public int SubCategoryId { get; set; }
            public int Categoryid { get; set; }
            public string SubcategoryName { get; set; }
            public string HindiName { get; set; }
        }
        public class RelatedItemSearch
        {

            public string itemname { get; set; }
            public string LogoUrl { get; set; }
            public double price { get; set; }
            public int SubCategoryId { get; set; }
            public int SubsubCategoryid { get; set; }
            public int Categoryid { get; set; }
            public int BaseCategoryId { get; set; }
            public double? marginPoint { get; set; }                       
            public string Scheme { get; set; }

        }
        public class RelatedItemSearchlist
        {
            public List<RelatedItemSearch> relatedItemSearch { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; }
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

        public class OfferdataDc
        {
            public bool Status { get; set; }
            public string Message { get; set; }
            public List<OfferDc> offer { get; set; }
        }

        public class OfferItemdc
        {
            public int itemId { get; set; }
            public bool IsInclude { get; set; }
        }
        public class WalletDc
        {
            public double? TotalAmount { get; set; }
        }
        public class CashConversionDc 
        {
        public double point { get; set; }
        public double rupee { get; set; }
        }
        public class WalletRewardDC
        {
            public WalletDc  Wallet { get; set; }       
            public CashConversionDc conversion { get; set; }

        }


        public class RetailerFilterSearchDc
        {
            public List<RetailerItemsearch> MostSellingProduct { get; set; }
            public List<RetailerItemsearch> RecentPurchase { get; set; }
            public List<RetailerItemsearch> CustFavoriteItem { get; set; }
            public List<RetailerItemsearch> RecentSearchItem { get; set; }
        }

        public class RetailerItemsearch
        {
            public int ItemId { get; set; }
            public string itemname { get; set; }
            public string LogoUrl { get; set; }
            public double price { get; set; }
            public int SubCategoryId { get; set; }
            public int SubsubCategoryid { get; set; }
            public int Categoryid { get; set; }
            public int BaseCategoryId { get; set; }
            public int? marginPoint { get; set; }
    
        }

        public class RetailerBillDiscountFreeItemDc
        {      
            public int ItemId { get; set; }
            public string ItemName { get; set; }      
            public int Qty { get; set; }      
        }

        public class CustomerData
        {
            public int CustomerId { get; set; }
            public int WarehouseId { get; set; }
            public int CityId { get; set; }
            public bool Active { get; set; }
            public bool Deleted { get; set; }
            public bool IsPrimeCustomer { get; set; }

            public string CustomerType { get; set; }
            public List<CustomerFlashDealWithItem> CustomerFlashDealWithItems { get; set; }
        }

        public class CustomerFlashDealWithItem
        {
            public int FlashDealId { get; set; }
            public int ItemId { get; set; }
        }
        public class AddressUpdatepaginationDTO
        {
            public int? WarehouseId { get; set; }
            public int Skip { get; set; }
            public int Take { get; set; }
            public string Keyword { get; set; }
            public int status { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }
        public class AddressUpdateResDTO
        {
            public int totalcount { get; set; }
            public dynamic result { get; set; }
        }

        public class ExportAddressUpdateRequest
        {
            public ObjectId Id { get; set; }
            public int CustomerId { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string SkCode { get; set; }
            public string ShopName { get; set; }
            public string Name { get; set; }
            public string MobileNo { get; set; }
            public string Address { get; set; }
            public double CurrentLat { get; set; }
            public double CurrentLng { get; set; }
            public string UpdateedAddress { get; set; }
            public double UpdatedLat { get; set; }
            public double UpdatedLng { get; set; }
           public int RequestBy { get; set; }
           public int UpdatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
           public DateTime UpdatedDate { get; set; }
            public int Status { get; set; } //0- Pending, 1- Approved, 2-Reject
            public string status { get; set; }
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
            public string OfferFreeItemImage{get; set;}
            public int? OfferFreeItemQuantity{get; set;}
            public int? OfferCategory{get; set;}
            public int? OfferId {get; set;}
            public double? OfferWalletPoint{get; set;}
            public int? OfferMinimumQty{get; set;}
            public int? OfferFreeItemId{get; set;}
            public string OfferType{get; set;}
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
            public DateTime? OfferStartTime { get; set;}
            public DateTime? OfferEndTime { get; set;}
            public double? OfferQtyAvaiable { get; set; }
            public double? OfferQtyConsumed { get; set; }
            public DateTime CurrentStartTime { get; set; }
            public List<SalesAppItemDataDC> moqList { get; set; }

        }

        public class SalesItemResponseDc
        {
            public int TotalItem { get; set; }
            public List<SalesAppItemDataDC> ItemDataDCs { get; set; }
        }


    }




