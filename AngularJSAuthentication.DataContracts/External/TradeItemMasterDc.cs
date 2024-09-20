using System;

namespace AngularJSAuthentication.DataContracts.External
{
    public class TradeItemMasterDc
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public double MRP { get; set; }
        public string ImagePath { get; set; }
        public double BasePrice { get; set; }
        public int? BaseCategoryId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? BrandId { get; set; }
        public string BaseCategoryName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string BrandName { get; set; }
        public string UnitOfQuantity { get; set; }
        public string UnitOfMeasurement { get; set; }
        public int weigth { get; set; }
        public bool IsInWishList { get; set; }
        public int SellingDP { get; set; }
        public int BuyingDP { get; set; }

        public double LastTradePrice { get; set; }
        public string BrandImagePath { get; set; }
        public double BestDemandPrice { get; set; }
        public double BestStockPrice { get; set; }
        public string TradeUpOrDown { get; set; }
        public string DemandUpOrDown { get; set; }
        public string StockUpOrDown { get; set; }
        public string ItemFullName
        {
            get
            {
                return (ItemName);
            }
        }

        public bool IsSellBidAvailable { get; set; }
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

        public string TotalTaxPercent { get; set; }
        public string CGST { get; set; }
        public string SGST { get; set; }
        public string HSNCode { get; set; }
        public string CESS { get; set; }
    }
}
