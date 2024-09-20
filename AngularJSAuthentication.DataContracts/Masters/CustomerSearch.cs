using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class FilterSearchDc
    {
        public List<Itemsearch> MostSellingProduct { get; set; }
        public List<Itemsearch> RecentPurchase { get; set; }
        public List<Itemsearch> CustFavoriteItem { get; set; }
        public List<Itemsearch> RecentSearchItem { get; set; }
    }

    public class Itemsearch
    {
        public int ItemId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public bool IsItemLimit { get; set; }
        public int ItemlimitQty { get; set; }
        public string ItemNumber { get { return Number; } }
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
        public double? marginPoint { get; set; }
        public int? promoPerItems { get; set; }
        public bool IsOffer { get; set; }
        public bool active { get; set; }
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
        public int ItemAppType { get; set; }
        public bool IsFlashDealUsed { get; set; }
        public int BillLimitQty { get; set; }
        public bool IsPrimeItem { get; set; }
        public decimal PrimePrice { get; set; }
        public string Scheme { get; set; }
        public double? TradePrice { get; set; }
        public double? WholeSalePrice { get; set; }
    }
}



