using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Shared
{
    public class DiscountOffers
    {
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

            //public DateTime start { get; set; }
            //public DateTime end { get; set; }

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

            public double BillAmount
            {
                get; set;
            }  // Bill Amount

            public string Description { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsActive { get; set; }
            public string OfferLogoUrl { get; set; }
            //public DateTime CreatedDate { get; set; }
            //public DateTime? UpdateDate { get; set; }


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
            //[NotMapped]
            //public ItemMaster itemMaster { get; set; }
            //[NotMapped]
            public int CustomerId { get; set; }
            //[NotMapped]
            //public int userid { get; set; }

            public int? FreeItemLimit { get; set; }
            public int? OffersaleQty { get; set; }

            public int? Category { get; set; }
            public int? subCategory { get; set; }

            public int? subSubCategory { get; set; }

            //[NotMapped]
            public bool isInclude { get; set; }
            
            public List<OfferItemsBillDiscountDTO> OfferItemsBillDiscountDTO { get; set; }
    }
    }



public class OfferItemsBillDiscountDTO
    {

    public int OfferId { get; set; }
    public string itemname { get; set; }
    public bool IsInclude { get; set; }

}




    

