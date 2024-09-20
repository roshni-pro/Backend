using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class CustomerShoppingCartDc
    {
        public int PeopleId { get; set; }
        public double DeliveryCharges { get; set; }
        public double CartTotalAmt { get; set; }
        public double GrossTotalAmt { get; set; }
        public double TotalTaxAmount { get; set; }
        public double TotalDiscountAmt { get; set; }
        public double WalletAmount { get; set; }
        public double TCSPercent { get; set; }
        public int NewBillingWalletPoint { get; set; }
        public int DeamPoint { get; set; }
        public int? GeneratedOrderId { get; set; }
        public int TotalQty { get; set; }
        public int WheelCount { get; set; }
        public double TotalSavingAmt { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public string ApplyOfferId { get; set; }
        public bool NotEligiblePrimeOffer { get; set; }
        public List<ShoppingCartItemDc> ShoppingCartItemDcs { get; set; }
        public List<DiscountDetail> DiscountDetails { get; set; }
        public bool IsPayLater { get; set; }
        public double PayLaterLimit { get; set; }
        public double PreTotalDispatched { get; set; }
        public double TCSLimit { get; set; }
    }

    public class DiscountDetail
    {
        public int? OfferId { get; set; }
        public double DiscountAmount { get; set; }
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

    public class CartItemDc
    {
        public int PeopleId { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public int ItemId { get; set; }
        public int qty { get; set; }
        public double UnitPrice { get; set; }
        public bool IsPrimeItem { get; set; }
        public bool IsDealItem { get; set; }
        public bool IsFreeItem { get; set; }
        public bool IsCartRequire { get; set; }
        public string lang { get; set; }
    }

    public class ReturnShoppingCart
    {
        public CustomerShoppingCartDc Cart { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    public class StorePayLaterLimitDc
    {
        public long StoreId { get; set; }
        public double CreditLimit { get; set; }
    }
}
