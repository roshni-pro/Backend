using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataLayer.Infrastructure;
using AngularJSAuthentication.Model;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
//using static AngularJSAuthentication.API.Controllers.BackendOrderController;

namespace AngularJSAuthentication.API.Managers
{
    public class RetailerAppManager
    {
        public double xPointValue = AppConstants.xPoint;
        public bool EnableOtherLanguage = true;
        public async Task<ItemListDc> RetailerGetItembycatesscatid(string lang, int customerId, int catid, int scatid, int sscatid, int skip, int take, string sortType, string direction, string Apptype = null)
        {
            bool ElasticSearchEnable = AppConstants.ElasticSearchEnable;
            ItemListDc res = new ItemListDc();
            using (var context = new AuthContext())
            {
                CustomerData customerData = GetCustomerData(customerId, context);
                if (!string.IsNullOrEmpty(Apptype) && Apptype == "Consumer")
                {
                    var defaultadd = context.ConsumerAddressDb.FirstOrDefault(x => x.CustomerId == customerId && x.Default);
                    customerData.WarehouseId = defaultadd.WarehouseId;
                    customerData.CustomerType = Apptype;
                }
                var warehouseId = customerData.WarehouseId;
                List<ItemDataDC> ItemDataDCs = new List<ItemDataDC>();

                if (ElasticSearchEnable)
                {
                    Suggest suggest = null;
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetItembycatesscatid");
                    var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", warehouseId.ToString())
                        .Replace("{#categoryid#}", catid.ToString())
                        .Replace("{#subcategoryid#}", scatid.ToString())
                        .Replace("{#subsubcategoryid#}", sscatid.ToString())
                        .Replace("{#from#}", skip.ToString())
                        .Replace("{#size#}", take.ToString());
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    ItemDataDCs = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                }
                else
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetItemByCatSubAndSubCat]";
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                    cmd.Parameters.Add(new SqlParameter("@sscatid", sscatid));
                    cmd.Parameters.Add(new SqlParameter("@scatid", scatid));
                    cmd.Parameters.Add(new SqlParameter("@catid", catid));
                    cmd.Parameters.Add(new SqlParameter("@sorttype", sortType));
                    cmd.Parameters.Add(new SqlParameter("@direction", direction));
                    cmd.Parameters.Add(new SqlParameter("@xpoint", xPointValue));

                    cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                    cmd.Parameters.Add(new SqlParameter("@Take", take));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    ItemDataDCs = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ItemDataDC>(reader).ToList();
                    reader.NextResult();
                    if (reader.Read())
                    {
                        res.TotalItem = Convert.ToInt32(reader["itemCount"]);
                    }
                }
                //added by Anurag 04-05-2021
                //RetailerAppManager retailerAppManager = new RetailerAppManager();
                //#region block Barnd
                //var custtype = customerData.IsKPP ? 1 : 2;
                //var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, warehouseId);
                //if (blockBarnds != null && blockBarnds.Any())
                //{
                //    ItemDataDCs = ItemDataDCs.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                //}
                //#endregion
                if (ItemDataDCs != null && ItemDataDCs.Any())
                {

                    var InactiveItems = ItemDataDCs.Where(s => !s.active);
                    InactiveItems = InactiveItems.GroupBy(x => x.ItemMultiMRPId).Select(x => x.FirstOrDefault()).ToList();
                    ItemDataDCs = ItemDataDCs.Where(s => s.active).ToList();
                    ItemDataDCs.AddRange(InactiveItems);
                    if (!string.IsNullOrEmpty(Apptype) && Apptype == "Consumer")
                    {
                        ItemDataDCs = ItemDataDCs.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                             Select(x => new ItemDataDC
                             {
                                 active = x.FirstOrDefault().active,
                                 WarehouseId = x.Key.WarehouseId,
                                 ItemMultiMRPId = x.Key.ItemMultiMRPId,
                                 ItemNumber = x.Key.ItemNumber,
                                 BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                                 BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                                 BillLimitQty = x.FirstOrDefault().BillLimitQty,
                                 Categoryid = x.FirstOrDefault().Categoryid,
                                 Classification = x.FirstOrDefault().Classification,
                                 CompanyId = x.FirstOrDefault().CompanyId,
                                 CurrentStartTime = x.FirstOrDefault().CurrentStartTime,
                                 Deleted = x.FirstOrDefault().Deleted,
                                 Discount = x.FirstOrDefault().Discount,
                                 DistributionPrice = x.FirstOrDefault().DistributionPrice,
                                 DistributorShow = x.FirstOrDefault().DistributorShow,
                                 dreamPoint = x.FirstOrDefault().dreamPoint,
                                 FlashDealMaxQtyPersonCanTake = x.FirstOrDefault().FlashDealMaxQtyPersonCanTake,
                                 FlashDealSpecialPrice = x.FirstOrDefault().FlashDealSpecialPrice,
                                 FreeItemId = x.FirstOrDefault().FreeItemId,
                                 HindiName = x.FirstOrDefault().HindiName,
                                 IsFlashDealStart = x.FirstOrDefault().IsFlashDealStart,
                                 IsFlashDealUsed = x.FirstOrDefault().IsFlashDealUsed,
                                 IsItemLimit = x.FirstOrDefault().IsItemLimit,
                                 IsOffer = x.FirstOrDefault().IsOffer,
                                 IsPrimeItem = x.FirstOrDefault().IsPrimeItem,
                                 IsSensitive = x.FirstOrDefault().IsSensitive,
                                 IsSensitiveMRP = x.FirstOrDefault().IsSensitiveMRP,
                                 ItemAppType = x.FirstOrDefault().ItemAppType,
                                 itemBaseName = x.FirstOrDefault().itemBaseName,
                                 ItemId = x.FirstOrDefault().ItemId,
                                 ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                                 itemname = x.FirstOrDefault().itemname,
                                 Itemtype = x.FirstOrDefault().Itemtype,
                                 LastOrderDate = x.FirstOrDefault().LastOrderDate,
                                 LastOrderDays = x.FirstOrDefault().LastOrderDays,
                                 LastOrderQty = x.FirstOrDefault().LastOrderQty,
                                 LogoUrl = x.FirstOrDefault().LogoUrl,
                                 marginPoint = x.FirstOrDefault().marginPoint,
                                 MinOrderQty = 1,
                                 MRP = x.FirstOrDefault().MRP,
                                 NetPurchasePrice = x.FirstOrDefault().NetPurchasePrice,
                                 NoPrimeOfferStartTime = x.FirstOrDefault().NoPrimeOfferStartTime,
                                 Number = x.FirstOrDefault().Number,
                                 OfferCategory = x.FirstOrDefault().OfferCategory,
                                 OfferEndTime = x.FirstOrDefault().OfferEndTime,
                                 OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                                 OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                                 OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                                 OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                                 OfferId = x.FirstOrDefault().OfferId,
                                 OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                                 OfferPercentage = x.FirstOrDefault().OfferPercentage,
                                 OfferQtyAvaiable = x.FirstOrDefault().OfferQtyAvaiable,
                                 OfferQtyConsumed = x.FirstOrDefault().OfferQtyConsumed,
                                 OfferStartTime = x.FirstOrDefault().OfferStartTime,
                                 OfferType = x.FirstOrDefault().OfferType,
                                 OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                                 price = x.FirstOrDefault().price,
                                 PrimePrice = x.FirstOrDefault().PrimePrice,
                                 promoPerItems = x.FirstOrDefault().promoPerItems,
                                 PurchaseValue = x.FirstOrDefault().PurchaseValue,
                                 Rating = x.FirstOrDefault().Rating,
                                 Scheme = x.FirstOrDefault().Scheme,
                                 SellingSku = x.FirstOrDefault().SellingSku,
                                 SellingUnitName = x.FirstOrDefault().SellingUnitName,
                                 Sequence = x.FirstOrDefault().Sequence,
                                 SubCategoryId = x.FirstOrDefault().SubCategoryId,
                                 SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                                 TotalAmt = x.FirstOrDefault().TotalAmt,
                                 TotalTaxPercentage = x.FirstOrDefault().TotalTaxPercentage,
                                 TradePrice = x.FirstOrDefault().TradePrice,
                                 UnitofQuantity = x.FirstOrDefault().UnitofQuantity,
                                 UnitPrice = x.FirstOrDefault().UnitPrice,
                                 UOM = x.FirstOrDefault().UOM,
                                 VATTax = x.FirstOrDefault().VATTax,
                                 WholeSalePrice = x.FirstOrDefault().WholeSalePrice
                             })
                            .ToList();
                    }
                }

                ItemDataDCs = ItemDataDCs.Where(x => (x.ItemAppType == 0 || x.ItemAppType == 1)).ToList();
                ItemDataDCs = GetItem(ItemDataDCs, customerData, lang, context, Apptype);

                #region ItemNetInventoryCheck
                if (!string.IsNullOrEmpty(Apptype) && Apptype == "Consumer")
                {
                    if (ItemDataDCs != null && ItemDataDCs.Any())
                    {
                        var itemInventory = new DataTable();
                        itemInventory.Columns.Add("ItemMultiMRPId");
                        itemInventory.Columns.Add("WarehouseId");
                        itemInventory.Columns.Add("Qty");
                        itemInventory.Columns.Add("isdispatchedfreestock");

                        foreach (var item in ItemDataDCs)
                        {
                            var dr = itemInventory.NewRow();
                            dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                            dr["WarehouseId"] = item.WarehouseId;
                            dr["Qty"] = 0;
                            dr["isdispatchedfreestock"] = false;
                            itemInventory.Rows.Add(dr);
                        }
                        var parmitemInventory = new SqlParameter
                        {
                            ParameterName = "ItemNetInventory",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.ItemNetInventory",
                            Value = itemInventory
                        };
                        var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        if (ItemNetInventoryDcs != null)
                        {
                            foreach (var item in ItemDataDCs)
                            {
                                var itemInventorys= ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == item.ItemMultiMRPId);
                                if (itemInventorys != null)
                                {
                                    item.IsItemLimit = true;
                                    item.ItemlimitQty = itemInventorys.RemainingQty;
                                }
                                else
                                {
                                    item.IsItemLimit = true;
                                    item.ItemlimitQty = 0;
                                }
                            }
                        }
                    }
                }
                #endregion

                if (ItemDataDCs != null && ItemDataDCs.Any())
                {

                    if (EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                    {
                        List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                        ElasticLanguageDataRequests = ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.itemBaseName }).ToList();
                        //ElasticLanguageDatas.AddRange(ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = x.itemname }).ToList());
                        ElasticLanguageDataRequests.AddRange(ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.OfferFreeItemName }).ToList());
                        ElasticLanguageDataRequests.AddRange(ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SellingUnitName }).ToList());
                        ElasticLanguageDataRequests.AddRange(ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.UOM }).ToList());

                        LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                        var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                        AngularJSAuthentication.API.Controllers.BackendOrderController backendOrderController = new AngularJSAuthentication.API.Controllers.BackendOrderController();
                        ItemDataDCs.ForEach(x =>
                            {
                                double cprice = backendOrderController.GetConsumerPrice(context, x.ItemMultiMRPId, x.price, x.UnitPrice, warehouseId);
                                x.UnitPrice = SkCustomerType.GetPriceFromType(customerData.CustomerType, x.UnitPrice, x.WholeSalePrice ?? 0, x.TradePrice ?? 0, cprice);
                                x.itemBaseName = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                                x.itemname = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                                if (x.IsSensitive == true && x.IsSensitiveMRP == true)
                                {
                                    x.itemname += " " + x.price + " MRP " + x.UnitofQuantity + " " + x.UOM;
                                }
                                else if (x.IsSensitive == true && x.IsSensitiveMRP == false)
                                {
                                    x.itemname += " " + x.UnitofQuantity + " " + x.UOM; //item display name 
                                }

                                else if (x.IsSensitive == false && x.IsSensitiveMRP == false)
                                {
                                    x.itemname = x.itemBaseName; //item display name
                                }
                                else if (x.IsSensitive == false && x.IsSensitiveMRP == true)
                                {
                                    x.itemname += " " + x.price + " MRP";//item display name 
                                }
                                x.OfferFreeItemName = ElasticLanguageDatas.Any(y => y.englishtext == x.OfferFreeItemName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.OfferFreeItemName).converttext : x.OfferFreeItemName;
                                x.SellingUnitName = ElasticLanguageDatas.Any(y => y.englishtext == x.SellingUnitName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SellingUnitName).converttext : x.SellingUnitName;
                                x.UOM = ElasticLanguageDatas.Any(y => y.englishtext == x.UOM) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.UOM).converttext : x.UOM;
                            });
                    }

                    if (sortType == "P")
                    {
                        if (direction == "asc")
                        {
                            ItemDataDCs = ItemDataDCs.OrderBy(x => x.UnitPrice).ToList();
                        }
                        else
                        {
                            ItemDataDCs = ItemDataDCs.OrderByDescending(x => x.UnitPrice).ToList();
                        }
                    }



                    res.Status = true;
                    res.Message = "Success";
                    res.ItemMasters = ItemDataDCs;
                }
                else
                {
                    res.Status = false;
                    res.Message = "Failed";
                    return res;
                }

                return res;
            }
        }

        public List<ItemDataDC> GetItem(List<ItemDataDC> ItemDataDCs, CustomerData customerData, string lang, AuthContext context, string Apptype = null)
        {
            int MemberShipHours = AppConstants.MemberShipHours;
            var FlashDealWithItemIds = customerData.CustomerFlashDealWithItems;
            DateTime CurrentDate = !customerData.IsPrimeCustomer ? DateTime.Now.AddHours(-1 * MemberShipHours) : DateTime.Now;
            var inActiveCustomer = customerData != null && (customerData.Active == false || customerData.Deleted == true) ? true : false;
            var offerids = ItemDataDCs.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
            var activeOfferids = new List<int>();
            if (!string.IsNullOrEmpty(Apptype) && Apptype == "Consumer")
            {
                activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferOn == "Item" && (x.OfferAppType == "Consumer")).Select(x => x.OfferId).ToList() : new List<int>();
            }
            else
            {
                activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferOn == "Item" && (x.OfferAppType == "Retailer App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();
            }


            var itemMultiMRPIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
            var PrimeItems = itemMultiMRPIds.Any() ? context.PrimeItemDetails.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.CityId == customerData.CityId && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList() : null;

            List<ItemDataDC> freeItems = null;
            if (activeOfferids.Any() && lang.Trim() == "hi")
            {
                var freeItemIds = ItemDataDCs.Where(x => x.OfferId.HasValue && x.OfferId > 0 && activeOfferids.Contains(x.OfferId.Value)).Select(x => x.OfferFreeItemId).ToList();
                freeItems = context.itemMasters.Where(x => freeItemIds.Contains(x.ItemId)).Select(x => new ItemDataDC
                {
                    ItemId = x.ItemId,
                    itemname = x.itemname,
                    HindiName = x.HindiName,
                    IsSensitive = x.IsSensitive,
                    IsSensitiveMRP = x.IsSensitiveMRP,
                    price = x.price,
                    UnitofQuantity = x.UnitofQuantity,
                    UOM = x.UOM
                }).ToList();

                foreach (var it in freeItems)
                {
                    if (!string.IsNullOrEmpty(it.HindiName))
                    {
                        if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                        {
                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                        }
                        else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                        {
                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                        }
                        else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                        {
                            it.itemname = it.HindiName; //item display name
                        }
                        else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                        {
                            it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                        }
                    }
                }
            }

            List<ItemScheme> ItemSchemes = GetItemScheme(itemMultiMRPIds, customerData.WarehouseId, context);
            AngularJSAuthentication.API.Controllers.BackendOrderController backendOrderController = new AngularJSAuthentication.API.Controllers.BackendOrderController();
            foreach (var it in ItemDataDCs)
            {
                double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, customerData.WarehouseId);
                it.UnitPrice = SkCustomerType.GetPriceFromType(customerData.CustomerType, it.UnitPrice
                                                                  , it.WholeSalePrice ?? 0
                                                                  , it.TradePrice ?? 0, cprice);
                if (PrimeItems != null && PrimeItems.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty))
                {
                    var primeItem = PrimeItems.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty);
                    it.IsPrimeItem = true;
                    it.PrimePrice = primeItem.PrimePercent > 0 ? Convert.ToDecimal(it.UnitPrice - (it.UnitPrice * Convert.ToDouble(primeItem.PrimePercent) / 100)) : primeItem.PrimePrice;
                }

                if (!(it.OfferStartTime <= CurrentDate && it.OfferEndTime >= DateTime.Now))
                {
                    it.IsOffer = false;
                    it.FlashDealSpecialPrice = 0;
                    it.OfferCategory = 0;
                }

                //Condition for offer end
                if (it.IsOffer)
                {
                    if (!inActiveCustomer)
                    {
                        if (it.OfferCategory == 2 && FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                    }
                    else if (it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                    }

                    if (it.OfferCategory == 1 && !(activeOfferids != null && activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId)))
                    {
                        it.IsOffer = false;
                        it.OfferCategory = 0;
                    }

                    if (it.OfferCategory == 1 && it.IsOffer)
                    {
                        if (freeItems != null && freeItems.Any(y => y.ItemId == it.OfferFreeItemId))
                            it.OfferFreeItemName = freeItems.FirstOrDefault(y => y.ItemId == it.OfferFreeItemId).itemname;
                    }
                }
                else
                {
                    it.IsOffer = false;
                    it.FlashDealSpecialPrice = 0;
                    it.OfferCategory = 0;
                }


                it.marginPoint = 0;
                it.dreamPoint = 0;
                if (!it.IsOffer)
                {
                    /// Dream Point Logic && Margin Point
                    int MP = 0, PP = 0;
                    double xPoint = xPointValue * 10;
                    if (it.promoPerItems.HasValue)
                    {
                        PP = it.promoPerItems.Value;
                    }
                    if (it.marginPoint.HasValue)
                    {
                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                    }
                    if (PP > 0 || MP > 0)
                    {
                        int? PP_MP = PP + MP;
                        it.dreamPoint = PP_MP;
                    }
                }

                if (it.price > it.UnitPrice)
                {
                    var unitprice = it.UnitPrice;

                    if (it.OfferCategory == 2 && it.IsOffer && it.FlashDealSpecialPrice.HasValue && it.FlashDealSpecialPrice > 0)
                    {
                        unitprice = it.FlashDealSpecialPrice.Value;
                    }
                    if (!string.IsNullOrEmpty(Apptype) && Apptype == "Consumer")
                    {
                        it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / it.price) : 0;//MP;  we replce marginpoint value by margin for app here 
                    }
                    else
                    {
                        it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / unitprice) : 0;//MP;  we replce marginpoint value by margin for app here 
                    }
                    if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.PTR > 0))
                    {
                        var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId);
                        var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                        var UPMRPMargin = it.marginPoint.Value;
                        if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                            it.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                    }
                }

                if (it.HindiName != null && lang.Trim() == "hi")
                {
                    if (it.IsSensitive == true && it.IsSensitiveMRP == true)
                    {
                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
                    }
                    else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
                    {
                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
                    }

                    else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
                    {
                        it.itemname = it.HindiName; //item display name
                    }
                    else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
                    {
                        it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
                    }
                }

                it.CurrentStartTime = DateTime.Now;
            }

            return ItemDataDCs;
        }
        public CustomerData GetCustomerData(int customerId, AuthContext context)
        {
            CustomerData customerData = new CustomerData();
            List<CustomerFlashDealWithItem> customerFlashDealWithItem = new List<CustomerFlashDealWithItem>();
            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();


            var cmd = context.Database.Connection.CreateCommand();
            cmd.CommandText = "[dbo].[GetCustomerWithFlashDealItem]";
            cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            var reader = cmd.ExecuteReader();
            customerData = ((IObjectContextAdapter)context)
           .ObjectContext
           .Translate<CustomerData>(reader).FirstOrDefault();
            reader.NextResult();
            while (reader.Read())
            {
                customerFlashDealWithItem = ((IObjectContextAdapter)context)
                 .ObjectContext
                 .Translate<CustomerFlashDealWithItem>(reader).ToList();
            }

            if (customerData != null)
            {
                if (customerFlashDealWithItem != null)
                {
                    customerData.CustomerFlashDealWithItems = customerFlashDealWithItem;
                }
                else
                    customerData.CustomerFlashDealWithItems = new List<CustomerFlashDealWithItem>();
            }
            else
            {
                customerData = new CustomerData
                {
                    CustomerFlashDealWithItems = new List<CustomerFlashDealWithItem>()
                };
            }

            return customerData;
        }

        public List<StoreCategorySubCategoryBrand> GetStoreWithDetailNew()
        {
            List<StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = new List<StoreCategorySubCategoryBrand>();
#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            _cacheProvider.Remove(Caching.CacheKeyHelper.StoreCacheKey());
            bool isExist = _cacheProvider.IsInCache(Caching.CacheKeyHelper.StoreCacheKey());
            if (!isExist)
            {
                _cacheProvider.Set(Caching.CacheKeyHelper.StoreCacheKey(), GetStoreWithDetailDb());

            }
            StoreCategorySubCategoryBrands = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.StoreCacheKey(), () => GetStoreWithDetailDb());
#else
            StoreCategorySubCategoryBrands = GetStoreWithDetailDb();
#endif
            return StoreCategorySubCategoryBrands;
        }

        public List<StoreCategorySubCategoryBrand> GetStoreWithDetail()
        {
            List<StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = new List<StoreCategorySubCategoryBrand>();
#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            StoreCategorySubCategoryBrands = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.StoreCacheKey(), () => GetStoreWithDetailDb());
#else
            StoreCategorySubCategoryBrands = GetStoreWithDetailDb();
#endif
            return StoreCategorySubCategoryBrands;
        }

        private List<StoreCategorySubCategoryBrand> GetStoreWithDetailDb()
        {
            List<StoreCategorySubCategoryBrand> storeCategorySubCategoryBrands = new List<StoreCategorySubCategoryBrand>();
            using (var context = new AuthContext())
            {
                storeCategorySubCategoryBrands = context.Database.SqlQuery<StoreCategorySubCategoryBrand>("Exec GetAllStoreWithDetail").ToList();
            }
            return storeCategorySubCategoryBrands;
        }


        public List<ClusterStoreExecutiveDc> GetStoreClusterExecutiveDetail()
        {
            List<ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = new List<ClusterStoreExecutiveDc>();
#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            clusterStoreExecutiveDcs = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.StoreExecutiveCacheKey(), () => GetStoreClusterExecutiveDetailDb());
#else
            clusterStoreExecutiveDcs = GetStoreClusterExecutiveDetailDb();
#endif
            return clusterStoreExecutiveDcs;
        }

        private List<ClusterStoreExecutiveDc> GetStoreClusterExecutiveDetailDb()
        {
            List<ClusterStoreExecutiveDc> custerStoreExecutiveDcs = new List<ClusterStoreExecutiveDc>();
            using (var context = new AuthContext())
            {
                custerStoreExecutiveDcs = context.Database.SqlQuery<ClusterStoreExecutiveDc>("Exec GetAllClusterStoreExecutive").ToList();
            }
            return custerStoreExecutiveDcs;
        }

        public List<ItemScheme> GetItemScheme(List<int> itemMultiMrpIds, int warehouseId, AuthContext context = null, int? cityId = null)
        {
            List<ItemScheme> ItemSchemes = new List<ItemScheme>();

            bool IsContextCreated = false;
            if (context == null)
            {
                IsContextCreated = true;
                context = new AuthContext();
            }
            if (!cityId.HasValue || cityId.Value == 0)
            {
                cityId = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId).Cityid;
            }

#if !DEBUG
            Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
            //_cacheProvider.Remove(Caching.CacheKeyHelper.ItemSchemeCacheKey(cityId.ToString()));
            ItemSchemes = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.ItemSchemeCacheKey(cityId.ToString()), () => 
                GetItemSchemMaster(cityId.Value,context)
            );
#else
            ItemSchemes = context.ItemSchemes.Where(x => x.Cityid == cityId.Value && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
#endif


            ItemSchemes = ItemSchemes.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId) && x.Cityid == cityId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();


            if (IsContextCreated)
            {
                context.Dispose();
            }

            return ItemSchemes;
        }

        private List<ItemScheme> GetItemSchemMaster(int cityId, AuthContext context = null)
        {
            bool IsContextCreated = false;
            if (context == null)
            {
                IsContextCreated = true;
                context = new AuthContext();
            }
            List<ItemScheme> ItemSchemes = context.ItemSchemes.Where(x => x.Cityid == cityId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();

            if (IsContextCreated)
            {
                context.Dispose();
            }
            return ItemSchemes;
        }
        public async Task<CatScatSscatDCs> GetCategories(string lang, int baseCatId, int warehouseid)
        {
            CatScatSscatDCs catScatSscatDCs = new CatScatSscatDCs();
            using (var unitOfWork = new UnitOfWork())
            {
                string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
                DataContracts.KPPApp.customeritem CatSubCatBrands = await unitOfWork.KPPAppRepository.GetRetailCatSubCatAsync(warehouseid, baseCatId, ConsumerApptype);
                if (lang == "hi")
                {
                    foreach (var kk in CatSubCatBrands.Basecats)
                    {
                        if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                        {
                            kk.BaseCategoryName = kk.HindiName;
                        }
                    }

                    foreach (var kk in CatSubCatBrands.Categories)
                    {
                        if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                        {
                            kk.CategoryName = kk.HindiName;
                        }
                    }

                    foreach (var kk in CatSubCatBrands.SubCategories)
                    {
                        if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                        {
                            kk.SubcategoryName = kk.HindiName;
                        }
                    }

                    foreach (var kk in CatSubCatBrands.SubSubCategories)
                    {
                        if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                        {
                            kk.SubSubcategoryName = kk.HindiName;
                        }
                    }
                }
                //catScatSscatDCs.Basecats = CatSubCatBrands.Basecats.Select(x => new BaseCategory { BaseCategoryId = x.BaseCategoryId, BaseCategoryName = x.BaseCategoryName, HindiName = x.HindiName, LogoUrl = x.LogoUrl }).ToList();
                List<CategoryDCs> categoryDC = CatSubCatBrands.Categories.Select(x => new CategoryDCs { Categoryid = x.Categoryid, CategoryName = x.CategoryName, LogoUrl = x.LogoUrl, CategoryImg = x.CategoryImg }).ToList();
                List<SubCategoryDCs> subCategoryDC = CatSubCatBrands.SubCategories.Select(x => new SubCategoryDCs { Categoryid = x.Categoryid, SubCategoryId = x.SubCategoryId, SubcategoryName = x.SubcategoryName, LogoUrl = x.LogoUrl, itemcount = x.itemcount }).ToList();
                List<SubsubCategoryDcs> subsubCategoryDc = CatSubCatBrands.SubSubCategories.Select(x => new SubsubCategoryDcs { SubCategoryId = x.SubCategoryId, SubsubCategoryid = x.SubSubCategoryId, SubsubcategoryName = x.SubSubcategoryName, Categoryid = x.Categoryid, LogoUrl = x.LogoUrl, itemcount = x.itemcount }).ToList();
                catScatSscatDCs.categoryDC = categoryDC;
                catScatSscatDCs.subCategoryDC = subCategoryDC;
                catScatSscatDCs.subsubCategoryDc = subsubCategoryDc;
            }
            return catScatSscatDCs;
        }

        public async Task<CatScatSscatDCs> GetStoreCategories(string lang, int baseCatId, int subCategoryId, int warehouseid)
        {
            CatScatSscatDCs catScatSscatDCs = new CatScatSscatDCs();
            using (var unitOfWork = new UnitOfWork())
            {
                DataContracts.KPPApp.customeritem CatSubCatBrands = await unitOfWork.KPPAppRepository.GetRetailStoreCatSubCatAsync(warehouseid, baseCatId, subCategoryId);
                if (lang == "hi")
                {
                    foreach (var kk in CatSubCatBrands.Basecats)
                    {
                        if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                        {
                            kk.BaseCategoryName = kk.HindiName;
                        }
                    }

                    foreach (var kk in CatSubCatBrands.Categories)
                    {
                        if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                        {
                            kk.CategoryName = kk.HindiName;
                        }
                    }

                    foreach (var kk in CatSubCatBrands.SubCategories)
                    {
                        if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                        {
                            kk.SubcategoryName = kk.HindiName;
                        }
                    }

                    foreach (var kk in CatSubCatBrands.SubSubCategories)
                    {
                        if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                        {
                            kk.SubSubcategoryName = kk.HindiName;
                        }
                    }
                }
                //catScatSscatDCs.Basecats = CatSubCatBrands.Basecats.Select(x => new BaseCategory { BaseCategoryId = x.BaseCategoryId, BaseCategoryName = x.BaseCategoryName, HindiName = x.HindiName, LogoUrl = x.LogoUrl }).ToList();
                List<CategoryDCs> categoryDC = CatSubCatBrands.Categories.Select(x => new CategoryDCs { Categoryid = x.Categoryid, CategoryName = x.CategoryName, LogoUrl = x.LogoUrl, CategoryImg = x.CategoryImg }).ToList();
                List<SubCategoryDCs> subCategoryDC = CatSubCatBrands.SubCategories.Select(x => new SubCategoryDCs { Categoryid = x.Categoryid, SubCategoryId = x.SubCategoryId, SubcategoryName = x.SubcategoryName, LogoUrl = x.LogoUrl, itemcount = x.itemcount }).ToList();
                List<SubsubCategoryDcs> subsubCategoryDc = CatSubCatBrands.SubSubCategories.Select(x => new SubsubCategoryDcs { SubCategoryId = x.SubCategoryId, SubsubCategoryid = x.SubSubCategoryId, SubsubcategoryName = x.SubSubcategoryName, Categoryid = x.Categoryid, LogoUrl = x.LogoUrl, itemcount = x.itemcount }).ToList();
                catScatSscatDCs.categoryDC = categoryDC;
                catScatSscatDCs.subCategoryDC = subCategoryDC;
                catScatSscatDCs.subsubCategoryDc = subsubCategoryDc;
            }
            return catScatSscatDCs;
        }

        public List<BlockBrandDc> GetBlockBrand(int customerType, int appType, int WarehouseId)
        {
            List<BlockBrandDc> blockBrandDcs = new List<BlockBrandDc>();
            MongoDbHelper<BlockBrands> mongoDbHelper = new MongoDbHelper<BlockBrands>();
            var blockBrands = mongoDbHelper.Select(x => (x.CustomerType == customerType || x.CustomerType == 4) && (x.AppType == appType || x.AppType == 3) && x.IsActive && !x.IsDeleted && x.WarehouseId == WarehouseId).ToList();
            blockBrandDcs = Mapper.Map(blockBrands).ToANew<List<BlockBrandDc>>();
            return blockBrandDcs;
        }

        public List<NextETADate> GetNextETADate(int warehouseId, int orderId)
        {
            List<NextETADate> nextETADate = new List<NextETADate>();
            using (var context = new AuthContext())
            {
                nextETADate = context.Database.SqlQuery<NextETADate>("Exec GetInTransitOrderCount " + warehouseId + "," + orderId).ToList();
            }
            return nextETADate;
        }

        public async Task<ItemResponseDc> RetailerGetItembycatesscatidnew(string lang, int customerId, int catid, int scatid, int sscatid, int skip, int take, string sortType, string direction, string Apptype = null)
        {
            var res = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            bool ElasticSearchEnable = AppConstants.ElasticSearchEnable;
            //ItemListDc res = new ItemListDc();
            using (var context = new AuthContext())
            {
                CustomerData customerData = GetCustomerData(customerId, context);
                if (!string.IsNullOrEmpty(Apptype) && Apptype == "Consumer")
                {
                    var defaultadd = context.ConsumerAddressDb.FirstOrDefault(x => x.CustomerId == customerId && x.Default);
                    customerData.WarehouseId = defaultadd.WarehouseId;
                    customerData.CustomerType = Apptype;
                }
                var warehouseId = customerData.WarehouseId;
                List<ItemDataDC> ItemDataDCs = new List<ItemDataDC>();

                if (ElasticSearchEnable)
                {
                    Suggest suggest = null;
                    MongoDbHelper<ElasticSearchQuery> mongoDbHelper = new MongoDbHelper<ElasticSearchQuery>();
                    var searchPredicate = PredicateBuilder.New<ElasticSearchQuery>(x => x.ObjectType == "ItemMaster" && x.QueryType == "RetailerGetItembycatesscatid");
                    var searchQuery = mongoDbHelper.Select(searchPredicate, null, null, null, collectionName: "ElasticSearchQuery").FirstOrDefault();
                    var query = searchQuery.Query.Replace(@"\r", "").Replace(@"\n", "")
                        .Replace("{#warehouseid#}", warehouseId.ToString())
                        .Replace("{#categoryid#}", catid.ToString())
                        .Replace("{#subcategoryid#}", scatid.ToString())
                        .Replace("{#subsubcategoryid#}", sscatid.ToString())
                        .Replace("{#from#}", skip.ToString())
                        .Replace("{#size#}", take.ToString());
                    List<ElasticSearchItem> elasticSearchItems = ElasticSearchHelper.GetItemByElasticSearchQuery(query, out suggest);
                    ItemDataDCs = Mapper.Map(elasticSearchItems).ToANew<List<ItemDataDC>>();
                }
                else
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetItemByCatSubAndSubCat]";
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                    cmd.Parameters.Add(new SqlParameter("@sscatid", sscatid));
                    cmd.Parameters.Add(new SqlParameter("@scatid", scatid));
                    cmd.Parameters.Add(new SqlParameter("@catid", catid));
                    cmd.Parameters.Add(new SqlParameter("@sorttype", sortType));
                    cmd.Parameters.Add(new SqlParameter("@direction", direction));
                    cmd.Parameters.Add(new SqlParameter("@xpoint", xPointValue));

                    cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                    cmd.Parameters.Add(new SqlParameter("@Take", take));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    ItemDataDCs = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ItemDataDC>(reader).ToList();
                    reader.NextResult();
                    if (reader.Read())
                    {
                        res.TotalItem = Convert.ToInt32(reader["itemCount"]);
                    }
                }
                //added by Anurag 04-05-2021
                //RetailerAppManager retailerAppManager = new RetailerAppManager();
                //#region block Barnd
                //var custtype = customerData.IsKPP ? 1 : 2;
                //var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, warehouseId);
                //if (blockBarnds != null && blockBarnds.Any())
                //{
                //    ItemDataDCs = ItemDataDCs.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                //}
                //#endregion
                if (ItemDataDCs != null && ItemDataDCs.Any())
                {

                    var InactiveItems = ItemDataDCs.Where(s => !s.active);
                    InactiveItems = InactiveItems.GroupBy(x => x.ItemMultiMRPId).Select(x => x.FirstOrDefault()).ToList();
                    ItemDataDCs = ItemDataDCs.Where(s => s.active).ToList();
                    ItemDataDCs.AddRange(InactiveItems);
                    if (!string.IsNullOrEmpty(Apptype) && Apptype == "Consumer")
                    {
                        ItemDataDCs = ItemDataDCs.GroupBy(s => new { s.ItemNumber, s.ItemMultiMRPId, s.WarehouseId }).
                             Select(x => new ItemDataDC
                             {
                                 active = x.FirstOrDefault().active,
                                 WarehouseId = x.Key.WarehouseId,
                                 ItemMultiMRPId = x.Key.ItemMultiMRPId,
                                 ItemNumber = x.Key.ItemNumber,
                                 BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                                 BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                                 BillLimitQty = x.FirstOrDefault().BillLimitQty,
                                 Categoryid = x.FirstOrDefault().Categoryid,
                                 Classification = x.FirstOrDefault().Classification,
                                 CompanyId = x.FirstOrDefault().CompanyId,
                                 CurrentStartTime = x.FirstOrDefault().CurrentStartTime,
                                 Deleted = x.FirstOrDefault().Deleted,
                                 Discount = x.FirstOrDefault().Discount,
                                 DistributionPrice = x.FirstOrDefault().DistributionPrice,
                                 DistributorShow = x.FirstOrDefault().DistributorShow,
                                 dreamPoint = x.FirstOrDefault().dreamPoint,
                                 FlashDealMaxQtyPersonCanTake = x.FirstOrDefault().FlashDealMaxQtyPersonCanTake,
                                 FlashDealSpecialPrice = x.FirstOrDefault().FlashDealSpecialPrice,
                                 FreeItemId = x.FirstOrDefault().FreeItemId,
                                 HindiName = x.FirstOrDefault().HindiName,
                                 IsFlashDealStart = x.FirstOrDefault().IsFlashDealStart,
                                 IsFlashDealUsed = x.FirstOrDefault().IsFlashDealUsed,
                                 IsItemLimit = x.FirstOrDefault().IsItemLimit,
                                 IsOffer = x.FirstOrDefault().IsOffer,
                                 IsPrimeItem = x.FirstOrDefault().IsPrimeItem,
                                 IsSensitive = x.FirstOrDefault().IsSensitive,
                                 IsSensitiveMRP = x.FirstOrDefault().IsSensitiveMRP,
                                 ItemAppType = x.FirstOrDefault().ItemAppType,
                                 itemBaseName = x.FirstOrDefault().itemBaseName,
                                 ItemId = x.FirstOrDefault().ItemId,
                                 ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                                 itemname = x.FirstOrDefault().itemname,
                                 Itemtype = x.FirstOrDefault().Itemtype,
                                 LastOrderDate = x.FirstOrDefault().LastOrderDate,
                                 LastOrderDays = x.FirstOrDefault().LastOrderDays,
                                 LastOrderQty = x.FirstOrDefault().LastOrderQty,
                                 LogoUrl = x.FirstOrDefault().LogoUrl,
                                 marginPoint = x.FirstOrDefault().marginPoint,
                                 MinOrderQty = 1,
                                 MRP = x.FirstOrDefault().MRP,
                                 NetPurchasePrice = x.FirstOrDefault().NetPurchasePrice,
                                 NoPrimeOfferStartTime = x.FirstOrDefault().NoPrimeOfferStartTime,
                                 Number = x.FirstOrDefault().Number,
                                 OfferCategory = x.FirstOrDefault().OfferCategory,
                                 OfferEndTime = x.FirstOrDefault().OfferEndTime,
                                 OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                                 OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                                 OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                                 OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                                 OfferId = x.FirstOrDefault().OfferId,
                                 OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                                 OfferPercentage = x.FirstOrDefault().OfferPercentage,
                                 OfferQtyAvaiable = x.FirstOrDefault().OfferQtyAvaiable,
                                 OfferQtyConsumed = x.FirstOrDefault().OfferQtyConsumed,
                                 OfferStartTime = x.FirstOrDefault().OfferStartTime,
                                 OfferType = x.FirstOrDefault().OfferType,
                                 OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                                 price = x.FirstOrDefault().price,
                                 PrimePrice = x.FirstOrDefault().PrimePrice,
                                 promoPerItems = x.FirstOrDefault().promoPerItems,
                                 PurchaseValue = x.FirstOrDefault().PurchaseValue,
                                 Rating = x.FirstOrDefault().Rating,
                                 Scheme = x.FirstOrDefault().Scheme,
                                 SellingSku = x.FirstOrDefault().SellingSku,
                                 SellingUnitName = x.FirstOrDefault().SellingUnitName,
                                 Sequence = x.FirstOrDefault().Sequence,
                                 SubCategoryId = x.FirstOrDefault().SubCategoryId,
                                 SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                                 TotalAmt = x.FirstOrDefault().TotalAmt,
                                 TotalTaxPercentage = x.FirstOrDefault().TotalTaxPercentage,
                                 TradePrice = x.FirstOrDefault().TradePrice,
                                 UnitofQuantity = x.FirstOrDefault().UnitofQuantity,
                                 UnitPrice = x.FirstOrDefault().UnitPrice,
                                 UOM = x.FirstOrDefault().UOM,
                                 VATTax = x.FirstOrDefault().VATTax,
                                 WholeSalePrice = x.FirstOrDefault().WholeSalePrice
                             })
                            .ToList();
                    }
                }

                ItemDataDCs = ItemDataDCs.Where(x => (x.ItemAppType == 0 || x.ItemAppType == 1)).ToList();
                ItemDataDCs = GetItem(ItemDataDCs, customerData, lang, context, Apptype);

                #region ItemNetInventoryCheck
                if (!string.IsNullOrEmpty(Apptype) && Apptype == "Consumer")
                {
                    if (ItemDataDCs != null && ItemDataDCs.Any())
                    {
                        var itemInventory = new DataTable();
                        itemInventory.Columns.Add("ItemMultiMRPId");
                        itemInventory.Columns.Add("WarehouseId");
                        itemInventory.Columns.Add("Qty");
                        itemInventory.Columns.Add("isdispatchedfreestock");

                        foreach (var item in ItemDataDCs)
                        {
                            var dr = itemInventory.NewRow();
                            dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                            dr["WarehouseId"] = item.WarehouseId;
                            dr["Qty"] = 0;
                            dr["isdispatchedfreestock"] = false;
                            itemInventory.Rows.Add(dr);
                        }
                        var parmitemInventory = new SqlParameter
                        {
                            ParameterName = "ItemNetInventory",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.ItemNetInventory",
                            Value = itemInventory
                        };
                        var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        if (ItemNetInventoryDcs != null)
                        {
                            foreach (var item in ItemDataDCs)
                            {
                                var itemInventorys = ItemNetInventoryDcs.FirstOrDefault(x => x.itemmultimrpid == item.ItemMultiMRPId);
                                if (itemInventorys != null)
                                {
                                    item.IsItemLimit = true;
                                    item.ItemlimitQty = itemInventorys.RemainingQty;
                                }
                                else
                                {
                                    item.IsItemLimit = true;
                                    item.ItemlimitQty = 0;
                                }
                            }
                        }
                    }
                }
                #endregion

                if (ItemDataDCs != null && ItemDataDCs.Any())
                {

                    if (EnableOtherLanguage && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
                    {
                        List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                        ElasticLanguageDataRequests = ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.itemBaseName }).ToList();
                        //ElasticLanguageDatas.AddRange(ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = x.itemname }).ToList());
                        ElasticLanguageDataRequests.AddRange(ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.OfferFreeItemName }).ToList());
                        ElasticLanguageDataRequests.AddRange(ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SellingUnitName }).ToList());
                        ElasticLanguageDataRequests.AddRange(ItemDataDCs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.UOM }).ToList());

                        LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                        var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                        AngularJSAuthentication.API.Controllers.BackendOrderController backendOrderController = new AngularJSAuthentication.API.Controllers.BackendOrderController();
                        ItemDataDCs.ForEach(x =>
                        {
                            double cprice = backendOrderController.GetConsumerPrice(context, x.ItemMultiMRPId, x.price, x.UnitPrice, warehouseId);
                            x.UnitPrice = SkCustomerType.GetPriceFromType(customerData.CustomerType, x.UnitPrice, x.WholeSalePrice ?? 0, x.TradePrice ?? 0, cprice);
                            x.itemBaseName = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                            x.itemname = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                            if (x.IsSensitive == true && x.IsSensitiveMRP == true)
                            {
                                x.itemname += " " + x.price + " MRP " + x.UnitofQuantity + " " + x.UOM;
                            }
                            else if (x.IsSensitive == true && x.IsSensitiveMRP == false)
                            {
                                x.itemname += " " + x.UnitofQuantity + " " + x.UOM; //item display name 
                            }

                            else if (x.IsSensitive == false && x.IsSensitiveMRP == false)
                            {
                                x.itemname = x.itemBaseName; //item display name
                            }
                            else if (x.IsSensitive == false && x.IsSensitiveMRP == true)
                            {
                                x.itemname += " " + x.price + " MRP";//item display name 
                            }
                            x.OfferFreeItemName = ElasticLanguageDatas.Any(y => y.englishtext == x.OfferFreeItemName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.OfferFreeItemName).converttext : x.OfferFreeItemName;
                            x.SellingUnitName = ElasticLanguageDatas.Any(y => y.englishtext == x.SellingUnitName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SellingUnitName).converttext : x.SellingUnitName;
                            x.UOM = ElasticLanguageDatas.Any(y => y.englishtext == x.UOM) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.UOM).converttext : x.UOM;
                        });
                    }

                    if (sortType == "P")
                    {
                        if (direction == "asc")
                        {
                            ItemDataDCs = ItemDataDCs.OrderBy(x => x.UnitPrice).ToList();
                        }
                        else
                        {
                            ItemDataDCs = ItemDataDCs.OrderByDescending(x => x.UnitPrice).ToList();
                        }
                    }
                    res.ItemDataDCs = ItemDataDCs;
                }
                else
                {
                    
                    return res;
                }

                return res;
            }
        }
    }
}