using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class BillDiscountOfferDc
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

        public int LineItem { get; set; }
        public string ImagePath { get; set; }
        public string ColorCode { get; set; }


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
        public bool IsScratchBDCode { get; set; } //if not scratch or done
        public int? Category { get; set; }
        public int? subCategory { get; set; }

        public int? subSubCategory { get; set; }
        public string BillDiscountType { get; set; }
        public double MaxDiscount { get; set; }
        public int? OfferUseCount { get; set; }
        public string OfferAppType { get; set; }
        public string ApplyOn { get; set; }
        public string WalletType { get; set; }
        public int itemId { get; set; }
        public string itemname { get; set; }
        public bool DistributorOfferType { get; set; }
        public int? OffersaleQty { get; set; }
        public string ApplyType { get; set; }
        public List<OfferBillDiscountItemDc> OfferBillDiscountItems { get; set; }

        public List<OfferItemDc> OfferItems { get; set; }
        public List<BillDiscountRequiredItemDc> BillDiscountRequiredItems { get; set; }
        public List<OfferLineItemValueDc> OfferLineItemValueDcs { get; set; }
        public double? FreeWalletPoint { get; set; }
        public string FreeOfferType { get; set; }

        public decimal DistributorDiscountAmount { get; set; }
        public decimal DistributorDiscountPercentage { get; set; }
        public long StoreId { get; set; }
        public bool IsAutoApply { get; set; }
        public string IncentiveClassification { get; set; }//by Sudhir 27-06-2023
        public bool IsAppliedOffer { get; set; }
        public bool IsBillDiscountFreebiesItem { get; set; }
        public bool IsBillDiscountFreebiesValue { get; set; }
        public int offerminorderquantity { get; set; }
        public string offeritemname { get; set; }
        public long CombinedGroupId { get; set; }
    }

    public class OfferItemDc
    {
        public int OfferId { get; set; }
        public int itemId { get; set; }
        public bool IsInclude { get; set; }
    }

    public class OfferBillDiscountItemDc
    {
        public int OfferId { get; set; }
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public bool IsInclude { get; set; }
    }


    public class CustomerWalletBillDiscountDc
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public double BillDiscountTypeValue { get; set; }
    }
    public class SlapOfferDC
    {
        public string BillDiscountOfferOn { get; set; }
        public double DiscountPercentage { get; set; }
        public double? WalletAmount { get; set; }
        public double OfferOnPrice { get; set; }
    }
    public class PostItemOfferDC
    {
        public int? OffersaleQty { get; set; }
        public int? OffersaleWeight { get; set; }
        public int? OffersaleAmount { get; set; }
        public double MaxDiscount { get; set; }
        public string FreeOfferType { get; set; }

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

    public class BrandCategorySubCategory
    {
        public int BrandCategoryMappingId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
    }
    public class VanDC
    {
        public List<int> WarehouseIds { get; set; }
        public string keyward { get; set; }
    }
    public class BillDiscountFreebiesItemQtyDC
    {
        public int Offerid { get; set; }
        public int BillDiscountItemQty { get; set; }
        public int BillDiscountValueQty { get; set; }
        
    }
}
