using System;
using System.Collections.Generic;

namespace AngularJSAuthentication.DataContracts.Masters
{
    public class AppHomeSectionsDc
    {
        public int SectionID { get; set; }
        public string AppType { get; set; }
        public int WarehouseID { get; set; }
        public string SectionName { get; set; }
        public string SectionHindiName { get; set; }
        public string SectionType { get; set; }
        public string SectionSubType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsTile { get; set; }
        //public bool IsSlider { get; set; }
        public bool IsBanner { get; set; }
        public bool IsPopUp { get; set; }
        public int Sequence { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public bool HasBackgroundColor { get; set; }
        public string TileBackgroundColor { get; set; }

        public string BannerBackgroundColor { get; set; }
        public bool HasHeaderBackgroundColor { get; set; }
        public string TileHeaderBackgroundColor { get; set; }
        public bool HasBackgroundImage { get; set; }
        public string TileBackgroundImage { get; set; }
        public bool HasHeaderBackgroundImage { get; set; }
        public string TileHeaderBackgroundImage { get; set; }
        public string TileAreaHeaderBackgroundImage { get; set; }
        public string sectionBackgroundImage { get; set; }
        public bool IsTileSlider { get; set; }
        public string HeaderTextColor { get; set; }
        public int HeaderTextSize { get; set; }
        public bool Deleted { get; set; }
        public bool Active { get; set; }
        public List<AppHomeSectionItemsDc> AppItemsList { get; set; }
        public string ViewType { get; set; }
        public string WebViewUrl { get; set; }
        public string BannerActivity { get; set; }
        public bool? IsSingleBackgroundImage { get; set; }//enhancement by simran 10/19/2023

    }

    public class AppHomeSectionItemsDc
    {
        public int SectionItemID { get; set; }
   
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string TileName { get; set; }
        public string TileImage { get; set; }
        public string BannerName { get; set; }

        public string DynamicHeaderImage { get; set; }
        public string BannerActivity { get; set; }

        public string BannerImage { get; set; }
        public string RedirectionUrl { get; set; }
        public string RedirectionType { get; set; }
        public int RedirectionID { get; set; }
        public int BaseCategoryId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryId { get; set; }
        public int ItemId { get; set; }
        public int ImageLevel { get; set; }
        public bool HasOffer { get; set; }
        public DateTime? OfferStartTime { get; set; }
        public DateTime? OfferEndTime { get; set; }
        public bool Deleted { get; set; }
        public bool Expired { get; set; }
        public bool Active { get; set; }
        public bool IsFlashDeal { get; set; }
        public int FlashDealQtyAvaiable { get; set; }
        public int FlashDealMaxQtyPersonCanTake { get; set; }
        public double FlashDealSpecialPrice { get; set; }
        public int MOQ { get; set; }
        public double UnitPrice { get; set; }
        public double PurchasePrice { get; set; }

        public string SellingSku { get; set; }
        public double? FlashdealRemainingQty { get; set; }
        public string TileSectionBackgroundImage { get; set; }
    }
}
