using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters.Seller
{
    public class SellerOfferDc
    {
        public int SubCatId { get; set; }
        public List<int> CityIds { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string OfferName { get; set; }
        public string OfferOn { get; set; }
        public string WalletType { get; set; }
        public double DiscountPercentage { get; set; }
        public int BillDiscountWallet { get; set; }
        public double BillAmount { get; set; }
        public double MaxBillAmount { get; set; }
        public double MaxDiscount { get; set; }
        public bool IsMultiTimeUse { get; set; }
        public bool IsUseOtherOffer { get; set; }
        public string OfferAppType { get; set; }
        public int OfferUseCount { get; set; }

        public string BillDiscountOfferOn { get; set; } //Percentage, WalletPoint, FreeItem
        public string ApplyType { get; set; } //Warehouse customer

        public bool IsActive { get; set; }
        public bool IsDispatchedFreeStock { get; set; }
        public string OfferCode { get; set; }

        public int LineItem { get; set; }
        public string Description { get; set; }
        public List<int> itemIds { get; set; }
        public int NoOffreeQuantity { get; set; }
        public int FreeItemLimit { get; set; }
        public int FreeItemId { get; set; }
        public int ItemId { get; set; }
        public int MinOrderQuantity { get; set; }
    }
}
