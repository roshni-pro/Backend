using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper.ItemList
{
    public class ItemListHelper
    {

        public bool ElasticSearchEnable = AppConstants.ElasticSearchEnable;

        public async Task<ItemListDc> GetItemListByBrand(int WarehouseId, List<int> IncludingCategoryIds, List<int> ExcludingCategoryIds, AuthContext context)
        {

            ItemListDc itemList = new ItemListDc();

            //var newdata = new List<ItemDataDC>();
            if (ElasticSearchEnable)
            {
                //Suggest suggest = null;
                //MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                //var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetAllItemByBrand");
                //var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                //var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                //    .Replace("{#warehouseid#}", warehouseid.ToString())
                //    .Replace("{#brand#}", SubsubCategoryid.ToString())
                //    .Replace("{#from#}", "0")
                //    .Replace("{#size#}", "1000");
                //List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                //newdata = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
            }
            else
            {
                var itemDataList = (from a in context.itemMasters
                                    where (a.WarehouseId == WarehouseId && a.Deleted == false && a.active == true && !a.IsDisContinued && a.Categoryid == IncludingCategoryIds.FirstOrDefault())
                                    let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                                    select new ItemDataDC
                                    {
                                        WarehouseId = a.WarehouseId,
                                        IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                        ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                        CompanyId = a.CompanyId,
                                        ItemId = a.ItemId,
                                        ItemNumber = a.Number,
                                        itemname = a.itemname,
                                        itemBaseName = a.itemBaseName,
                                        HindiName = a.HindiName,
                                        LogoUrl = a.LogoUrl,
                                        MinOrderQty = a.MinOrderQty,
                                        price = a.price,
                                        TotalTaxPercentage = a.TotalTaxPercentage,
                                        UnitPrice = a.UnitPrice,
                                        active = a.active,
                                        dreamPoint = 0,
                                        marginPoint = a.marginPoint,
                                        NetPurchasePrice = a.NetPurchasePrice,
                                        promoPerItems = a.promoPerItems,
                                        IsOffer = a.IsOffer,
                                        Deleted = a.Deleted,
                                        OfferCategory = a.OfferCategory,
                                        OfferStartTime = a.OfferStartTime,
                                        OfferEndTime = a.OfferEndTime,
                                        OfferQtyAvaiable = a.OfferQtyAvaiable,
                                        OfferQtyConsumed = a.OfferQtyConsumed,
                                        OfferId = a.OfferId,
                                        OfferType = a.OfferType,
                                        OfferWalletPoint = a.OfferWalletPoint,
                                        OfferFreeItemId = a.OfferFreeItemId,
                                        OfferPercentage = a.OfferPercentage,
                                        OfferFreeItemName = a.OfferFreeItemName,
                                        OfferFreeItemImage = a.OfferFreeItemImage,
                                        OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                        OfferMinimumQty = a.OfferMinimumQty,
                                        FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                        FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                        ItemMultiMRPId = a.ItemMultiMRPId,
                                        BillLimitQty = a.BillLimitQty,
                                        Categoryid = a.Categoryid,
                                        SubCategoryId = a.SubCategoryId,
                                        SubsubCategoryid = a.SubsubCategoryid,
                                        BaseCategoryId = a.BaseCategoryid,
                                        SellingSku = a.SellingSku,
                                        SellingUnitName = a.SellingUnitName,
                                        IsSensitive = a.IsSensitive,
                                        IsSensitiveMRP = a.IsSensitiveMRP,
                                        UOM = a.UOM,
                                        UnitofQuantity = a.UnitofQuantity
                                    }).OrderByDescending(x => x.ItemNumber).ToList();

            }
            //itemList.ItemMasters = new List<ItemDataDC>();
            //var formatedData = await ItemValidate(newdata, ActiveCustomer, context, lang);
            //item.ItemMasters.AddRange(formatedData);
            return itemList;
        }
    }
}
