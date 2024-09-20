using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.Model;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using AngularJSAuthentication.API.Controllers.AgentCommission;
using Nito.AspNetBackgroundTasks;
using AngularJSAuthentication.API.Models;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using System.Runtime.Caching;
using Newtonsoft.Json;
using AngularJSAuthentication.DataLayer.Infrastructure;
using AngularJSAuthentication.Common.Constants;
using System.Web;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model.Store;
using SqlBulkTools;
using AgileObjects.AgileMapper;
using System.ComponentModel.DataAnnotations;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.APIParams;
using System.Device.Location;
using System.Collections.Concurrent;
using AngularJSAuthentication.Model.Forecasting;
using System.Configuration;
using AngularJSAuthentication.BusinessLayer.Helpers.ElasticDataHelper;
using Nito.AsyncEx;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using LinqKit;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using Nest;
using AngularJSAuthentication.Model.CustomerShoppingCart;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.Model.SalesApp;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.Masters;
using System.Text.RegularExpressions;
using System.IO;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Model.Item;
using System.Security.Claims;
using static AngularJSAuthentication.API.Controllers.BackendOrderController;

namespace AngularJSAuthentication.API.Controllers.External.SalesManApp
{
    [RoutePrefix("api/SalesAppItem")]
    public class SalesAppItemController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public double xPointValue = AppConstants.xPoint;
        /// <summary>
        /// Section base item on agent app
        /// 16/07/2019
        /// Created by 
        /// </summary>
        /// <param name="Warehouseid"></param>
        /// <param name="sectionid"></param>       
        /// <returns></returns>
        [Route("GetItemBySectionForAgent")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetItemBySectionForAgent(int PeopleId, int warehouseid, int sectionid, string lang)
        {

            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
            List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
            List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
            List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

            using (var context = new AuthContext())
            {
                DateTime CurrentDate = DateTime.Now;
                var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                if (data != null)
                {
                    List<int> Itemids = data.Select(x => x.ItemId).ToList();

                    WRSITEM item = new WRSITEM();

                    var newdata = (from a in context.itemMasters
                                   where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && Itemids.Contains(a.ItemId) && (a.ItemAppType == 0 || a.ItemAppType == 1)
                                   && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       CompanyId = a.CompanyId,
                                       Categoryid = a.Categoryid,
                                       Discount = a.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       itemname = a.itemname,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = a.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName,
                                       VATTax = a.VATTax,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       NetPurchasePrice = a.NetPurchasePrice,
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
                                       BillLimitQty = a.BillLimitQty,
                                       ItemAppType = a.ItemAppType,
                                       ItemMultiMRPId = a.ItemMultiMRPId
                                   }).ToList();

                    newdata = newdata.OrderByDescending(x => x.ItemNumber).ToList();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd
                    var custtype = 4;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                    }
                    #endregion
                    var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                    foreach (var it in newdata)
                    {
                        if (!it.OfferId.HasValue || it.OfferId.Value == 0)
                        {
                            it.IsOffer = false;
                        }
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;

                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }
                        }


                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax)  By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }

                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;

                            }
                            else
                            {

                                it.dreamPoint = 0;
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                        }
                        catch { }

                        if (lang.Trim() == "hi")
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
                        item.ItemMasters.Add(it);
                    }
                    if (item.ItemMasters != null)
                    {
                        var res = new
                        {
                            ItemMasters = item.ItemMasters,
                            Status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {
                        Array[] l = new Array[] { };
                        var res = new
                        {
                            ItemMasters = l,
                            Status = false,
                            Message = "fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    Array[] l = new Array[] { };
                    var res = new
                    {
                        ItemMasters = l,
                        Status = false,
                        Message = "Item Not found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }


        [Route("GetCompanyTopMarginItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetCompanyTopMarginItem(int PeopleId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                var brandids = string.Join(",", SubSubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable brandidDt = new DataTable();
                brandidDt.Columns.Add("IntValue");
                foreach (var item in brandids)
                {
                    DataRow dr = brandidDt.NewRow();
                    dr[0] = item;
                    brandidDt.Rows.Add(dr);
                }
                var SubSubCategoryIds = new SqlParameter("SubSubCategoryIds", brandidDt);
                SubSubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubSubCategoryIds.TypeName = "dbo.IntValues";


                var Subcatids = string.Join(",", SubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable SubCatidDt = new DataTable();
                SubCatidDt.Columns.Add("IntValue");
                foreach (var item in Subcatids)
                {
                    DataRow dr = SubCatidDt.NewRow();
                    dr[0] = item;
                    SubCatidDt.Rows.Add(dr);
                }
                var SubCategoryIds = new SqlParameter("SubCategoryIds", SubCatidDt);
                SubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubCategoryIds.TypeName = "dbo.IntValues";

                var Categoryid = string.Join(",", CatIds).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable CatIdDt = new DataTable();
                CatIdDt.Columns.Add("IntValue");
                foreach (var item in Categoryid)
                {
                    DataRow dr = CatIdDt.NewRow();
                    dr[0] = item;
                    CatIdDt.Rows.Add(dr);
                }
                var CategoryIds = new SqlParameter("CategoryIds", CatIdDt);
                CategoryIds.SqlDbType = SqlDbType.Structured;
                CategoryIds.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetCompanyTopMarginItemSalesApp]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(SubSubCategoryIds);
                cmd.Parameters.Add(SubCategoryIds);
                cmd.Parameters.Add(CategoryIds);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                while (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    ItemData = ItemData.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (ItemData != null && ItemData.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                }

                foreach (var it in ItemData.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1) && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)))
                {

                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;

                    //Condition for offer end

                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                        else if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }

                    }
                    else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;

                    }

                    if (it.OfferCategory == 1)
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }

                    try
                    {
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //Customer (0.2 * 10=1)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }
                        }
                        else { it.dreamPoint = 0; }

                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }
                    catch { }

                    if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                    ItemDataDCs.Add(it);
                }

                itemResponseDc.ItemDataDCs = ItemDataDCs;
            }
            return itemResponseDc;
        }

        [Route("GetABCategoryItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetABCategoryItem(int PeopleId, int warehouseId, int skip, int take, string lang)
        {
            skip = skip * take;
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                //if (take == 4)
                //    take = 100;

                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);

                if (blockBarnds != null && blockBarnds.Any())
                {
                    StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
                }

                #endregion


                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();



                var brandids = string.Join(",", SubSubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable brandidDt = new DataTable();
                brandidDt.Columns.Add("IntValue");
                foreach (var item in brandids)
                {
                    DataRow dr = brandidDt.NewRow();
                    dr[0] = item;
                    brandidDt.Rows.Add(dr);
                }
                var SubSubCategoryIds = new SqlParameter("SubSubCategoryIds", brandidDt);
                SubSubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubSubCategoryIds.TypeName = "dbo.IntValues";


                var Subcatids = string.Join(",", SubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable SubCatidDt = new DataTable();
                SubCatidDt.Columns.Add("IntValue");
                foreach (var item in Subcatids)
                {
                    DataRow dr = SubCatidDt.NewRow();
                    dr[0] = item;
                    SubCatidDt.Rows.Add(dr);
                }
                var SubCategoryIds = new SqlParameter("SubCategoryIds", SubCatidDt);
                SubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubCategoryIds.TypeName = "dbo.IntValues";

                var Categoryid = string.Join(",", CatIds).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable CatIdDt = new DataTable();
                CatIdDt.Columns.Add("IntValue");
                foreach (var item in Categoryid)
                {
                    DataRow dr = CatIdDt.NewRow();
                    dr[0] = item;
                    CatIdDt.Rows.Add(dr);
                }
                var CategoryIds = new SqlParameter("CategoryIds", CatIdDt);
                CategoryIds.SqlDbType = SqlDbType.Structured;
                CategoryIds.TypeName = "dbo.IntValues";
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetABCategoryItemSalesApp]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(SubSubCategoryIds);
                cmd.Parameters.Add(SubCategoryIds);
                cmd.Parameters.Add(CategoryIds);
                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                while (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }



                var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (ItemData != null && ItemData.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                }


                foreach (var it in ItemData.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1)))
                {
                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;


                    //Condition for offer end

                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                        else if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }

                    }
                    else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;

                    }

                    if (it.OfferCategory == 1)
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }


                    try
                    {
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //Customer (0.2 * 10=1)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }
                        }
                        else { it.dreamPoint = 0; }

                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }
                    catch { }

                    if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                    ItemDataDCs.Add(it);
                }

                itemResponseDc.ItemDataDCs = ItemDataDCs;
            }

            return itemResponseDc;
        }

        #region GetLastPurchaseItem by anurag 13-02-2023
        [Route("GetLastPurchaseItem")]
        [HttpGet]
        public async Task<ItemResponseDc> GetLastPurchaseItem(int PeopleId, int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);

                if (blockBarnds != null && blockBarnds.Any())
                {
                    StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
                }

                #endregion



                //param = new SqlParameter("CatCompanyBrand", IdDt);

                //List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                //List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                //List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                //var brandids = string.Join(",", SubSubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                //DataTable brandidDt = new DataTable();
                //brandidDt.Columns.Add("IntValue");
                //foreach (var item in brandids)
                //{
                //    DataRow dr = brandidDt.NewRow();
                //    dr[0] = item;
                //    brandidDt.Rows.Add(dr);
                //}
                //var SubSubCategoryIds = new SqlParameter("SubSubCategoryIds", brandidDt);
                //SubSubCategoryIds.SqlDbType = SqlDbType.Structured;
                //SubSubCategoryIds.TypeName = "dbo.IntValues";


                //var Subcatids = string.Join(",", SubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                //DataTable SubCatidDt = new DataTable();
                //SubCatidDt.Columns.Add("IntValue");
                //foreach (var item in Subcatids)
                //{
                //    DataRow dr = SubCatidDt.NewRow();
                //    dr[0] = item;
                //    SubCatidDt.Rows.Add(dr);
                //}
                //var SubCategoryIds = new SqlParameter("SubCategoryIds", SubCatidDt);
                //SubCategoryIds.SqlDbType = SqlDbType.Structured;
                //SubCategoryIds.TypeName = "dbo.IntValues";

                //var Categoryid = string.Join(",", CatIds).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                //DataTable CatIdDt = new DataTable();
                //CatIdDt.Columns.Add("IntValue");
                //foreach (var item in Categoryid)
                //{
                //    DataRow dr = CatIdDt.NewRow();
                //    dr[0] = item;
                //    CatIdDt.Rows.Add(dr);
                //}
                //var CategoryIds = new SqlParameter("CategoryIds", CatIdDt);
                //CategoryIds.SqlDbType = SqlDbType.Structured;
                //CategoryIds.TypeName = "dbo.IntValues";

                var IdDt = new DataTable();
                SqlParameter param = null;

                IdDt = new DataTable();
                IdDt.Columns.Add("categoryId");
                IdDt.Columns.Add("companyId");
                IdDt.Columns.Add("brandId");
                foreach (var item in StoreCategorySubCategoryBrands)
                {
                    var dr = IdDt.NewRow();
                    dr["categoryId"] = item.Categoryid;
                    dr["companyId"] = item.SubCategoryId;
                    dr["brandId"] = item.BrandId;
                    IdDt.Rows.Add(dr);
                }

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                param = new SqlParameter("CatCompanyBrand", IdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.CatCompanyBrand";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetCustomerLastMonthPurchaseItemByPeopleID]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemcount"]);
                }

                var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                foreach (var it in ItemData)
                {
                    //Condition for offer end
                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                        else if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }

                    }
                    else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;

                    }

                    if (it.OfferCategory == 1)
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }

                    try
                    {
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //Customer (0.2 * 10=1)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }
                        }
                        else { it.dreamPoint = 0; }

                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }
                    catch { }

                    if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                    it.Itemtype = 2;
                    //it.Sequence = suggestedIndex;
                    ItemDataDCs.Add(it);
                }
            }

            return itemResponseDc;
        }

        [Route("GetLastPurchaseItemNew")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ItemResponseDc> GetLastPurchaseItemAsyncNew(int PeopleId, int customerId, int warehouseId, int skip, int take, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            List<DataContracts.External.MobileExecutiveDC.ItemDataDC> ItemDataDCs = new List<DataContracts.External.MobileExecutiveDC.ItemDataDC>();
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);

                if (blockBarnds != null && blockBarnds.Any())
                {
                    StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
                }

                #endregion

                var categoryIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                var companyIds = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                var brandIds = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                var enddate = DateTime.Now;
                var startDate = enddate.AddMonths(-9);
                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted
                                                    && x.CustomerId == customerId && x.WarehouseId == warehouseId);

                var platformIdxName = "skorderdata_" + AppConstants.Environment;
                //ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemNumber> elasticSqlHelper = new ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemNumber>();

                // var orderdatas = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync($"SELECT top 100 itemnumber,itemname,createddate,ordqty,itemmultimrpid from {platformIdxName} where custid='{customerId}' and whid='{warehouseId}' order by createddate desc,ordqty desc"))).ToList();

                string query = $" SELECT top 10 orderid from skorderdata_{AppConstants.Environment} where custid={customerId} and whid='{warehouseId}'  and catid in ({ string.Join(",", categoryIds) }) and compid in ({ string.Join(",", companyIds) }) and brandid in ({ string.Join(",", brandIds) })  group by orderid order by orderid desc";

                ElasticSqlHelper<DataContracts.External.LastPOOrderData> elasticSqlHelperData = new ElasticSqlHelper<DataContracts.External.LastPOOrderData>();

                var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(query)).ToList());

                if (orderdetails != null && orderdetails.Any())
                {
                    var orderIdList = orderdetails.Select(x => x.OrderId).ToList();
                    List<DataContracts.External.MobileExecutiveDC.ItemDataDC> ItemData = new List<DataContracts.External.MobileExecutiveDC.ItemDataDC>();
                    string queryItem = $" SELECT itemnumber from skorderdata_{AppConstants.Environment} where custid={customerId} and whid='{warehouseId}'  and catid in ({ string.Join(",", categoryIds) }) and compid in ({ string.Join(",", companyIds) }) and brandid in ({ string.Join(",", brandIds) }) and orderid in ({ string.Join(",", orderIdList) })  group by itemnumber";

                    ElasticSqlHelper<DataContracts.External.LastPOOrderItemNumberData> elasticSqlHelperDataNew = new ElasticSqlHelper<DataContracts.External.LastPOOrderItemNumberData>();

                    var OrderItemDetails = AsyncContext.Run(async () => (await elasticSqlHelperDataNew.GetListAsync(queryItem)).ToList());

                    var ItemNumberList = OrderItemDetails.Select(x => x.ItemNumber).ToList();

                    ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                    var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesCatelogElasticData(warehouseId, StoreCategorySubCategoryBrands, ItemNumberList, "", -1, -1, (skip * take), take, "DESC", true, null));
                    ItemData = data.ItemMasters;
                    itemResponseDc.TotalItem = data.TotalItem;

                    var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                    List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                    if (ItemData != null && ItemData.Any())
                    {
                        ItemMasterManager itemMasterManager = new ItemMasterManager();
                        itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                    }

                    foreach (var it in ItemData)
                    {
                        it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                        it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;

                        //Condition for offer end
                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                            else if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }
                        else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                        }

                        if (it.OfferCategory == 1)
                        {
                            if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                it.IsOffer = true;
                            else
                                it.IsOffer = false;
                        }

                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }
                            }
                            else { it.dreamPoint = 0; }

                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }

                        if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                        it.Itemtype = 1;
                        ItemDataDCs.Add(it);
                    }
                }
            }
            itemResponseDc.ItemDataDCs = Mapper.Map(ItemDataDCs).ToANew<List<DataContracts.External.ItemDataDC>>();
            return itemResponseDc;
        }

        #endregion
        //For Mobile API Showing item in that there are current offer.
        [Route("GetOfferItemForAgent")]
        [HttpGet]
        public HttpResponseMessage GetOfferItemForAgent(int PeopleId, int WarehouseId, string lang)
        {

            using (var context = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                WROFFERTEM item = new WROFFERTEM();
                List<factoryItemdata> itemMasters = new List<factoryItemdata>();
                itemMasters = (from a in context.itemMasters
                               where (a.WarehouseId == WarehouseId && a.OfferStartTime <= indianTime
                               && a.OfferEndTime >= indianTime && a.OfferCategory == 1 && a.active == true && a.Deleted == false
                               && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)
                               && (a.ItemAppType == 0 || a.ItemAppType == 1))
                               //join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                               join c in context.OfferDb on a.OfferId equals c.OfferId
                               where (c.IsActive == true && c.IsDeleted == false && (c.OfferAppType == "Sales App" || c.OfferAppType == "Both"))
                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                               select new factoryItemdata
                               {
                                   WarehouseId = a.WarehouseId,
                                   IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                   ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                   CompanyId = a.CompanyId,
                                   Categoryid = a.Categoryid,
                                   Discount = a.Discount,
                                   ItemId = a.ItemId,
                                   ItemNumber = a.Number,
                                   //itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                   itemname = a.itemname,
                                   LogoUrl = a.LogoUrl,
                                   MinOrderQty = a.MinOrderQty,
                                   price = a.price,
                                   SubCategoryId = a.SubCategoryId,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   TotalTaxPercentage = a.TotalTaxPercentage,
                                   SellingUnitName = a.SellingUnitName,
                                   SellingSku = a.SellingSku,
                                   UnitPrice = a.UnitPrice,
                                   HindiName = a.HindiName != null ? a.HindiName : a.itemname,
                                   VATTax = a.VATTax,
                                   active = a.active,
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
                                   BillLimitQty = a.BillLimitQty,
                                   ItemMultiMRPId = a.ItemMultiMRPId
                               }).OrderByDescending(x => x.ItemNumber).ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    itemMasters = itemMasters.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion


                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (itemMasters != null && itemMasters.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = AsyncContext.Run(() => itemMasterManager.GetItemIncentiveClassification(WarehouseId, itemMasters.Select(s => s.ItemMultiMRPId).Distinct().ToList()));

                }



                foreach (var it in itemMasters)
                {
                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;

                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<factoryItemdata>();
                    }
                    try
                    {/// Dream Point Logic && Margin Point
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //salesman 0.2=(0.02 * 10=0.2)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }

                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }
                    catch { }
                    if (lang.Trim() == "hi")
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
                    item.ItemMasters.Add(it);
                }
                if (itemMasters.Count() != 0)
                {
                    var res = new
                    {
                        offerData = itemMasters,
                        Status = true,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        offerData = itemMasters,
                        Status = false,
                        Message = "Item Not found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }


        [Route("ItemListForAgent")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemListForAgent> ItemListForAgent(int WarehouseId, string lang, int PeopleId, int Skip, int Take)
        {

            using (var context = new AuthContext())
            {
                ItemListForAgent item = new ItemListForAgent();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemForAgentApp]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                cmd.Parameters.Add(new SqlParameter("@Skip", Skip));
                cmd.Parameters.Add(new SqlParameter("@Take", Take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = cmd.ExecuteReader();
                var newdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<Itemdata>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    item.TotalItem = Convert.ToInt32(reader["TotalItem"]);
                }


                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                var offerids = newdata.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (newdata != null && newdata.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(WarehouseId, newdata.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                }

                foreach (var it in newdata.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1) && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)))
                {

                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;


                    if (!it.OfferId.HasValue || it.OfferId.Value == 0)
                    {
                        it.IsOffer = false;
                    }
                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                        it.IsOffer = true;
                    else
                        it.IsOffer = false;

                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }
                    }


                    if (it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                    }
                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<Itemdata>();
                    }
                    try
                    {/// Dream Point Logic && Margin Point
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //salesman 0.2=(0.02 * 10=0.2)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }

                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }

                    }
                    catch { }
                    if (lang.Trim() == "hi")
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
                    if (it.OfferType != "FlashDeal")
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }
                    item.ItemMasters.Add(it);
                }
                if (item.ItemMasters != null && item.ItemMasters.Any())
                {
                    item.Message = "Success";
                    item.Status = true;
                    item.ItemMasters.Where(x => !x.marginPoint.HasValue).ToList().ForEach(x => x.marginPoint = 0);
                    item.ItemMasters = item.ItemMasters.OrderByDescending(x => x.marginPoint).ToList();
                    return item;
                }
                else
                {
                    item.Message = "Item Not found";
                    item.Status = false;
                    return item;
                }



            }


        }


        /// <summary>
        /// Updated by 05/09/2019
        /// </summary>
        /// <param name="itemname"></param>
        /// <param name="PeopleID"></param>
        /// <returns></returns>
        [Route("Search")]
        [HttpGet]
        public HttpResponseMessage Search(string lang, string itemname, int PeopleId, int warehouseId, int customerId)
        {
            using (var db = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                WRSITEM item = new WRSITEM();
                var newdata = (from a in db.itemMasters
                               where (a.WarehouseId == warehouseId &&
                               a.itemname.Contains(itemname) && a.Deleted == false && a.active == true && (a.ItemAppType == 0 || a.ItemAppType == 1)
                               && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                               let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                               select new factoryItemdata
                               {
                                   WarehouseId = a.WarehouseId,
                                   IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                   ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                   CompanyId = a.CompanyId,
                                   Categoryid = a.Categoryid,
                                   Discount = a.Discount,
                                   ItemId = a.ItemId,
                                   ItemNumber = a.Number,
                                   itemname = a.itemname,
                                   LogoUrl = a.LogoUrl,
                                   MinOrderQty = a.MinOrderQty,
                                   IsSensitive = a.IsSensitive,
                                   IsSensitiveMRP = a.IsSensitiveMRP,
                                   UnitofQuantity = a.UnitofQuantity,
                                   UOM = a.UOM,
                                   price = a.price,
                                   SubCategoryId = a.SubCategoryId,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   TotalTaxPercentage = a.TotalTaxPercentage,
                                   SellingUnitName = a.SellingUnitName,
                                   SellingSku = a.SellingSku,
                                   UnitPrice = a.UnitPrice,
                                   HindiName = a.HindiName,
                                   VATTax = a.VATTax,
                                   active = a.active,
                                   marginPoint = a.marginPoint,
                                   promoPerItems = a.promoPerItems,
                                   NetPurchasePrice = a.NetPurchasePrice,
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
                                   FreeItemId = a.OfferFreeItemId,
                                   ItemMultiMRPId = a.ItemMultiMRPId,
                                   BillLimitQty = a.BillLimitQty,
                                   itemBaseName = a.itemBaseName
                               }).OrderByDescending(x => x.ItemNumber).ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (newdata != null && newdata.Any())
                {
                    var itemmultiMrpIds = newdata.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    List<DataContracts.External.orderMrpData> orderdetails = new List<DataContracts.External.orderMrpData>();
                    ParallelLoopResult parellelResult = Parallel.ForEach(itemmultiMrpIds, (mrpid) =>
                    {
                        //    foreach (var mrpid in itemmultiMrpIds)
                        //{
                        string query = $"SELECT top 1 itemmultimrpid,createddate createddate, ordqty Qty from skorderdata_{AppConstants.Environment} where itemmultimrpid in ({ mrpid })   and whid={warehouseId} and custid={customerId}  order by createddate desc";

                        ElasticSqlHelper<DataContracts.External.orderMrpData> elasticSqlHelper = new ElasticSqlHelper<DataContracts.External.orderMrpData>();
                        var order = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).FirstOrDefault());
                        if (order != null)
                            orderdetails.Add(order);
                    });

                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = AsyncContext.Run(() => itemMasterManager.GetItemIncentiveClassification(warehouseId, newdata.Select(s => s.ItemMultiMRPId).Distinct().ToList()));

                    var itemMultiMRPIds = newdata.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                    using (var context = new AuthContext())
                    {
                        ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, warehouseId, context);
                    }
                    foreach (var itemdata in newdata)
                    {

                        if (orderdetails != null && orderdetails.Any(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId))
                        {
                            itemdata.LastOrderDate = orderdetails.Where(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate;
                            itemdata.LastOrderQty = orderdetails.Where(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().Qty;
                            itemdata.LastOrderDays = (DateTime.Today - itemdata.LastOrderDate).Value.Days;
                        }

                        if (itemdata.price > itemdata.UnitPrice)
                        {
                            itemdata.marginPoint = itemdata.UnitPrice > 0 ? (((itemdata.price - itemdata.UnitPrice) * 100) / itemdata.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                            if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId && x.PTR > 0))
                            {
                                var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId);
                                var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                                var UPMRPMargin = itemdata.marginPoint.Value;
                                if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                    itemdata.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                            }

                        }
                        else
                        {
                            itemdata.marginPoint = 0;
                        }
                    }
                }


                foreach (var it in newdata)
                {
                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;


                    if (!it.OfferId.HasValue || it.OfferId.Value == 0)
                    {
                        it.IsOffer = false;
                    }
                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                        it.IsOffer = true;
                    else
                        it.IsOffer = false;

                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }
                    }
                    if (it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                    }



                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<factoryItemdata>();
                    }
                    try
                    {/// Dream Point Logic && Margin Point
                        int? MP, PP;
                        double xPoint = xPointValue * 10;
                        //Customer (0.02 * 10=0.2)
                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                        {
                            PP = 0;
                        }
                        else
                        {
                            PP = it.promoPerItems;
                        }
                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                        {
                            MP = 0;
                        }
                        else
                        {
                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                        }
                        if (PP > 0 && MP > 0)
                        {
                            int? PP_MP = PP + MP;
                            it.dreamPoint = PP_MP;
                        }
                        else if (MP > 0)
                        {
                            it.dreamPoint = MP;
                        }
                        else if (PP > 0)
                        {
                            it.dreamPoint = PP;
                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                        //// by sudhir 22-08-2019
                        if (lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }
                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                        //end
                    }
                    catch { }

                    if (it.OfferType != "FlashDeal")
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }

                    item.ItemMasters.Add(it);
                }
                if (itemname != null || itemname != "")
                {
                    BackgroundTaskManager.Run(() =>
                    {
                        MongoDbHelper<ExecutiveProductSearch> mongoDbHelper = new MongoDbHelper<ExecutiveProductSearch>();
                        ExecutiveProductSearch executiveProductSearch = new ExecutiveProductSearch
                        {
                            CreatedDate = indianTime,
                            PeopleId = PeopleId,
                            keyword = itemname,
                            IsDeleted = false
                        };
                        mongoDbHelper.Insert(executiveProductSearch);
                    });
                }
                if (item.ItemMasters != null)
                {
                    item.Message = true;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                else
                {
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }

            }
        }


        /// <summary>
        /// # Version 2
        /// Created by 19/12/2018 
        /// </summary>
        /// <param name="spid"></param>
        /// <param name="sscatid"></param>
        /// <returns></returns>

        [Route("getItembysscatid")]
        [HttpGet]
        public HttpResponseMessage salesgetbysscatidv3(string lang, int PeopleId, int warehouseid, int catid, int scatid, int sscatid)
        {
            using (var context = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                WRSITEM item = new WRSITEM();
                //Increase some parameter For offer
                var newdata = (from a in context.itemMasters
                               where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true
                               && ((sscatid > 0 && a.SubsubCategoryid == sscatid) || (sscatid == 0 && a.SubsubCategoryid == a.SubsubCategoryid))
                               && ((scatid > 0 && a.SubCategoryId == scatid) || (scatid == 0 && a.SubCategoryId == a.SubCategoryId))
                               && a.Categoryid == catid && (a.ItemAppType == 0 || a.ItemAppType == 1)
                               && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                               select new factoryItemdata
                               {
                                   WarehouseId = a.WarehouseId,
                                   IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                   ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                   CompanyId = a.CompanyId,
                                   Categoryid = a.Categoryid,
                                   Discount = a.Discount,
                                   ItemId = a.ItemId,
                                   ItemNumber = a.Number,
                                   itemname = a.itemname,
                                   IsSensitive = a.IsSensitive,
                                   IsSensitiveMRP = a.IsSensitiveMRP,
                                   UnitofQuantity = a.UnitofQuantity,
                                   UOM = a.UOM,
                                   LogoUrl = a.LogoUrl,
                                   MinOrderQty = a.MinOrderQty,
                                   price = a.price,
                                   SubCategoryId = a.SubCategoryId,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   TotalTaxPercentage = a.TotalTaxPercentage,
                                   SellingUnitName = a.SellingUnitName,
                                   SellingSku = a.SellingSku,
                                   UnitPrice = a.UnitPrice,
                                   HindiName = a.HindiName,
                                   VATTax = a.VATTax,
                                   active = a.active,
                                   NetPurchasePrice = a.NetPurchasePrice,
                                   marginPoint = a.marginPoint,
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
                                   BillLimitQty = a.BillLimitQty,
                                   ItemMultiMRPId = a.ItemMultiMRPId
                               }).OrderByDescending(x => x.ItemNumber).ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                foreach (var it in newdata)
                {
                    if (it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                    }
                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }

                    }

                    if (it.OfferType != "FlashDeal")
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }


                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<factoryItemdata>();
                    }
                    try
                    {
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //salesman 0.2=(0.02 * 10=0.2)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }

                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }

                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }
                    catch (Exception ds) { }
                    //// by sudhir 22-08-2019
                    if (lang.Trim() == "hi")
                    {
                        if (it.IsSensitive == true)
                        {
                            if (it.IsSensitiveMRP == false)
                            {
                                it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                            }
                            else
                            {
                                it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                            }
                        }
                        else
                        {
                            it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                        }
                    }
                    //end
                    item.ItemMasters.Add(it);
                }

                if (item.ItemMasters != null)
                {
                    item.Message = true;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                else
                {
                    item.Message = true;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }

            }
        }


        [Route("getSaleIntentItembysscatid")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getSaleIntentItembysscatid(string lang, int PeopleId, int warehouseid, int catid, int scatid, int sscatid, int Skip, int Take)
        {
            using (var context = new AuthContext())

            {
                //string lang="hi";
                List<SalesIntentItemResponse> item = new List<SalesIntentItemResponse>();
                //  var param= new SqlParameter("@lang", lang);
                // var param1= new SqlParameter("@PeopleId", PeopleId) ;
                // var param2= new SqlParameter("@warehouseid", warehouseid);
                var param3 = new SqlParameter("@catid", catid);
                var param4 = new SqlParameter("@scatid", scatid);
                var param5 = new SqlParameter("@sscatid", sscatid);
                var param8 = new SqlParameter("@warehouseId", warehouseid);
                var param6 = new SqlParameter("@Skip", Skip);
                var param7 = new SqlParameter("@Take", Take);



                var dataSP = context.Database.SqlQuery<SalesIntentHistoryDC>("exec[SpSalesIndentItemNew] @catid,@scatid,@sscatid,@warehouseId,@Skip,@Take", param3, param4, param5, param8, param6, param7).ToList();
                //  var dataSP = context.Database.SqlQuery<SalesIntentHistoryDC>("exec[SpSalesIndentItemNew] @catid,@scatid,@sscatid,@Skip,@Tag", param3,param4,param5,param7).ToList();


                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                //  List<SalesIntentItemResponse> item = new List<SalesIntentItemResponse>();
                // Increase some parameter For offer
                //dataSP =from a in dataSP.where(a.WarehouseId == warehouseid && a.Deleted == false && a.IsDisContinued == false
                //                && ((sscatid > 0 && a.SubsubCategoryid == sscatid) || (sscatid == 0 && a.SubsubCategoryid == a.SubsubCategoryid))
                //                && ((scatid > 0 && a.SubCategoryId == scatid) || (scatid == 0 && a.SubCategoryId == a.SubCategoryId))
                //                && a.Categoryid == catid
                //                && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                //                join i in context.ItemMultiMRPDB on a.Number equals i.ItemNumber
                //                join m in context.ItemForecastDetailDb on a.ItemMultiMRPId equals m.ItemMultiMRPIdsp


                var newdata = (from a in dataSP
                               where CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)


                               //                               CatIds.Contains(a.Categoryid) = (
                               //CASE WHEN a.SubCategoryId = 0 and a.SubsubCategoryid = 0 THEN a.Categoryid END)
                               //or
                               //SubsubCategoryid = @sscatid and SubCategoryId = @scatid and Categoryid = @catid



                               // join i in context.ItemMultiMRPDB on a.Number equals i.ItemNumber
                               //join m in context.ItemForecastDetailDb on a.ItemMultiMRPId equals m.ItemMultiMRPId
                               //into gj from ItemForecastDetailDb in gj.DefaultIfEmpty()
                               //  into ItemForecastDetailDb
                               //from m in ItemForecastDetailDb.DefaultIfEmpty()

                               select new
                               {
                                   ItemNumber = a.Number,
                                   itemname = a.itemBaseName,
                                   IsSensitive = a.IsSensitive,
                                   IsSensitiveMRP = a.IsSensitiveMRP,
                                   UnitofQuantity = a.UnitofQuantity,
                                   UOM = a.UOM,
                                   LogoUrl = a.LogoUrl,
                                   MRP = a.MRP,
                                   HindiName = a.HindiName,
                                   ItemMultiMRPId = a.ItemMultiMRPId,
                                   SubCategoryId = a.SubCategoryId,
                                   Categoryid = a.Categoryid,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   WarehouseId = a.Warehouseid,
                                   SystemSuggestedQty = a.SystemSuggestedQty

                               }).ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    dataSP = dataSP.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();

                }

                #endregion

                foreach (var si in newdata)
                {
                    SalesIntentItemResponse it = new SalesIntentItemResponse();
                    it.IsSensitive = si.IsSensitive;
                    it.IsSensitiveMRP = si.IsSensitiveMRP;
                    it.ItemMultiMRPId = si.ItemMultiMRPId;
                    it.Itemname = si.itemname;
                    it.LogoUrl = si.LogoUrl;
                    it.MRP = si.MRP;
                    it.SystemForecastQty = si.SystemSuggestedQty;
                    it.WarehouseId = si.WarehouseId;
                    if (it.IsSensitive == true)
                    {
                        if (it.IsSensitiveMRP == false)
                        {
                            it.Itemname = si.itemname + " " + si.UnitofQuantity + " " + si.UOM; //item display name   
                        }
                        else
                        {
                            it.Itemname = si.itemname + " " + si.MRP + " MRP " + si.UnitofQuantity + " " + si.UOM; //item display name                               
                        }
                    }
                    else
                    {
                        it.Itemname = si.itemname + " " + si.MRP + " MRP "; //item display name                               
                    }
                    if (lang.Trim() == "hi")
                    {
                        if (it.IsSensitive == true)
                        {
                            if (it.IsSensitiveMRP == false)
                            {
                                it.Itemname = si.HindiName + " " + si.UnitofQuantity + " " + si.UOM; //item display name   
                            }
                            else
                            {
                                it.Itemname = si.HindiName + " " + si.MRP + " MRP " + si.UnitofQuantity + " " + si.UOM; //item display name                               
                            }
                        }
                        else
                        {
                            it.Itemname = si.HindiName + " " + si.MRP + " MRP "; //item display name                               
                        }
                    }
                    item.Add(it);
                }

                return Request.CreateResponse(HttpStatusCode.OK, item);
            }
        }
        //new Search
        [Route("SearchSaleIntentItembysscatid")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage SearchSaleIntentItembysscatid(string lang, int PeopleId, int warehouseid, string itemName)
        {
            using (var context = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                List<SalesIntentItemResponse> item = new List<SalesIntentItemResponse>();
                //Increase some parameter For offer
                var newdata = (from a in context.itemMasters
                               where (a.WarehouseId == warehouseid && a.Deleted == false && a.IsDisContinued == false
                               //&& ((sscatid > 0 && a.SubsubCategoryid == sscatid) || (sscatid == 0 && a.SubsubCategoryid == a.SubsubCategoryid))
                               //&& ((scatid > 0 && a.SubCategoryId == scatid) || (scatid == 0 && a.SubCategoryId == a.SubCategoryId))
                               //&& a.Categoryid == catid
                               && a.itemname.Contains(itemName)
                               && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                               join i in context.ItemMultiMRPDB on a.Number equals i.ItemNumber //where a.ItemMultiMRPId==i.ItemMultiMRPId 
                               select new
                               {
                                   ItemNumber = a.Number,
                                   itemname = a.itemBaseName,
                                   IsSensitive = a.IsSensitive,
                                   IsSensitiveMRP = a.IsSensitiveMRP,
                                   UnitofQuantity = i.UnitofQuantity,
                                   UOM = i.UOM,
                                   LogoUrl = a.LogoUrl,
                                   MRP = i.MRP,
                                   HindiName = a.HindiName,
                                   ItemMultiMRPId = i.ItemMultiMRPId,
                                   SubCategoryId = a.SubCategoryId,
                                   Categoryid = a.Categoryid,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   WarehouseId = a.WarehouseId
                               }).Distinct().ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                foreach (var si in newdata)
                {
                    SalesIntentItemResponse it = new SalesIntentItemResponse();
                    it.IsSensitive = si.IsSensitive;
                    it.IsSensitiveMRP = si.IsSensitiveMRP;
                    it.ItemMultiMRPId = si.ItemMultiMRPId;
                    it.Itemname = si.itemname;
                    it.LogoUrl = si.LogoUrl;
                    it.MRP = si.MRP;
                    it.SystemForecastQty = 0;
                    it.WarehouseId = si.WarehouseId;
                    if (it.IsSensitive == true)
                    {
                        if (it.IsSensitiveMRP == false)
                        {
                            it.Itemname = si.itemname + " " + si.UnitofQuantity + " " + si.UOM; //item display name   
                        }
                        else
                        {
                            it.Itemname = si.itemname + " " + si.MRP + " MRP " + si.UnitofQuantity + " " + si.UOM; //item display name                               
                        }
                    }
                    else
                    {
                        it.Itemname = si.itemname + " " + si.MRP + " MRP "; //item display name                               
                    }
                    if (lang.Trim() == "hi")
                    {
                        if (it.IsSensitive == true)
                        {
                            if (it.IsSensitiveMRP == false)
                            {
                                it.Itemname = si.HindiName + " " + si.UnitofQuantity + " " + si.UOM; //item display name   
                            }
                            else
                            {
                                it.Itemname = si.HindiName + " " + si.MRP + " MRP " + si.UnitofQuantity + " " + si.UOM; //item display name                               
                            }
                        }
                        else
                        {
                            it.Itemname = si.HindiName + " " + si.MRP + " MRP "; //item display name                               
                        }
                    }
                    item.Add(it);
                }

                return Request.CreateResponse(HttpStatusCode.OK, item);
            }

        }

        //EndSearch

        // add data mbd date 01 June 2022
        [Route("AddSalesIntent")]
        [HttpPost]
        public bool AddSalesIR(SalesIntentRequestDC SForecast)
        {
            bool status = false;
            try
            {
                //  bool status = false;
                using (AuthContext context = new AuthContext())
                {
                    var Rdata = context.SalesIntentRequestDb.Where(x => x.Warehouseid == SForecast.Warehouseid && x.ItemMultiMRPId == SForecast.ItemMultiMRPId).FirstOrDefault();
                    // if (Rdata == null)
                    //{
                    SalesIntentRequest k = new SalesIntentRequest();
                    k.PeopleId = SForecast.PeopleId;
                    k.CreatedBy = SForecast.PeopleId;
                    k.ItemMultiMRPId = SForecast.ItemMultiMRPId;
                    k.RequestQty = SForecast.RequestQty;
                    k.RequestPrice = SForecast.RequestPrice;
                    k.BuyerApproveID = SForecast.BuyerApproveID;
                    k.SalesLeadApproveID = SForecast.SalesLeadApproveID;
                    k.Status = SForecast.Status;
                    k.Warehouseid = SForecast.Warehouseid;
                    k.SalesApprovedDate = SForecast.SalesApprovedDate;
                    k.BuyerApprovedDate = SForecast.BuyerApprovedDate;
                    k.IsActive = true;
                    k.IsDeleted = false;
                    k.isReject = 0;
                    k.CreatedDate = DateTime.Now;
                    k.ModifiedDate = DateTime.Now;
                    k.ETADate = Convert.ToDateTime(SForecast.ETADate);// new add
                    k.NoOfSet = SForecast.NoOfSet;//new add
                    k.MinOrderQty = SForecast.MinOrderQty;//new add
                    context.SalesIntentRequestDb.Add(k);
                    status = context.Commit() > 0;

                    //}                 
                }
                return status;
            }
            catch (Exception ex)
            {
                return status;
            }
        }
        // end add data mbd 02/06/2022
        //New History
        [Route("GetHistorySI")]
        [HttpGet]
        [AllowAnonymous]
        public List<SalesIntentHistoryDC> GetHistorySI(int PeopleId)
        {
            using (var context = new AuthContext())
            {
                List<SalesIntentHistoryDC> item = new List<SalesIntentHistoryDC>();

                var param = new SqlParameter("@PeopleId", PeopleId);
                var data = context.Database.SqlQuery<SalesIntentHistoryDC>("exec[SpSalesIndanteHistory] @PeopleId", param).ToList();


                return data;
            }
        }
        //End History

        /// <summary>
        /// Updated by 09/11/2019
        /// Get item by barcode
        /// </summary>
        /// <param name="customerid"></param>
        /// <param name="barcode"></param>
        /// <returns></returns>
        [Route("GetItemByBarcode")]
        [HttpGet]
        public HttpResponseMessage GetItemByBarcode(int PeopleId, int WarehouseId, string barcode, string Lang)
        {
            using (var db = new AuthContext())
            {
                WRSITEM item = new WRSITEM();
                var barcodeitem = db.ItemBarcodes.FirstOrDefault(i => i.Barcode == barcode && i.IsDeleted == false && i.IsActive);
                if (barcodeitem != null)
                {
                    List<ItemMaster> itemList = new List<ItemMaster>();
                    List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                    List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                    List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                    List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                    var newdata = (from a in db.itemMasters
                                   where a.WarehouseId == WarehouseId && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)
                                   && a.Deleted == false && a.active == true && a.Number.Trim().ToLower().Equals(barcodeitem.ItemNumber.Trim().ToLower())
                                   let limit = db.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new factoryItemdata
                                   {
                                       BaseCategoryId = a.BaseCategoryid,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       Categoryid = a.Categoryid,
                                       Discount = a.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       HindiName = a.HindiName,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UnitofQuantity = a.UnitofQuantity,
                                       UOM = a.UOM,
                                       itemname = a.itemname,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = a.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       VATTax = a.VATTax,
                                       itemBaseName = a.itemBaseName,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       promoPerItems = a.promoPerItems,
                                       NetPurchasePrice = a.NetPurchasePrice,
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
                                       FreeItemId = a.OfferFreeItemId,
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       BillLimitQty = a.BillLimitQty
                                   }).OrderByDescending(x => x.ItemNumber).ToList();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd
                    var custtype = 4;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                    }
                    #endregion

                    var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both" || x.OfferAppType == "Sales App").Select(x => x.OfferId).ToList() : new List<int>();
                    foreach (var it in newdata)
                    {
                        if (it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }

                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }
                        }

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;

                                //Customer (0.2 * 10=1)
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }

                            }
                            else { it.dreamPoint = 0; }

                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }
                        if (Lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }

                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                        item.ItemMasters.Add(it);
                    }
                    if (item.ItemMasters != null)
                    {
                        item.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        item.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                }
                else
                {
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
            }
        }

        /// <summary>
        /// get all Brand item By ID # version 2
        /// </summary>
        /// <param name="warid"></param>
        /// <param name="brandName"></param>
        /// <returns></returns>
        [Route("GetAllItemByBrand")]
        [HttpGet]
        public HttpResponseMessage GetAllItemByBrand(string lang, int PeopleId, int warehouseid, int subSubCategoryId)
        {
            using (var context = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                WRSITEM item = new WRSITEM();
                if (lang.Trim() == "hi")
                {
                    //Increase some parameter For offer
                    var newdatahi = (from a in context.itemMasters
                                     where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.SubsubCategoryid == subSubCategoryId && (a.ItemAppType == 0 || a.ItemAppType == 1)
                                       && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                                     let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                     select new factoryItemdata
                                     {
                                         WarehouseId = a.WarehouseId,
                                         IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                         ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                         CompanyId = a.CompanyId,
                                         Categoryid = a.Categoryid,
                                         Discount = a.Discount,
                                         ItemId = a.ItemId,
                                         ItemNumber = a.Number,
                                         itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                         LogoUrl = a.LogoUrl,
                                         MinOrderQty = a.MinOrderQty,
                                         price = a.price,
                                         SubCategoryId = a.SubCategoryId,
                                         SubsubCategoryid = a.SubsubCategoryid,
                                         TotalTaxPercentage = a.TotalTaxPercentage,
                                         SellingUnitName = a.SellingUnitName,
                                         SellingSku = a.SellingSku,
                                         UnitPrice = a.UnitPrice,
                                         HindiName = a.HindiName,
                                         VATTax = a.VATTax,
                                         active = a.active,
                                         dreamPoint = 0,
                                         marginPoint = a.marginPoint,
                                         NetPurchasePrice = a.NetPurchasePrice,
                                         promoPerItems = a.promoPerItems,
                                         IsOffer = a.IsOffer,
                                         Deleted = a.Deleted,
                                         IsSensitive = a.IsSensitive,
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
                                         itemBaseName = a.itemBaseName,
                                         IsSensitiveMRP = a.IsSensitiveMRP,
                                         UOM = a.UOM,
                                         UnitofQuantity = a.UnitofQuantity
                                     }).OrderByDescending(x => x.ItemNumber).ToList();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd
                    var custtype = 4;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        newdatahi = newdatahi.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                    }
                    #endregion

                    var offerids = newdatahi.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();


                    foreach (factoryItemdata it in newdatahi)
                    {
                        if (it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }

                        if (it.OfferType != "FlashDeal")
                        {

                            if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                it.IsOffer = true;
                            else
                                it.IsOffer = false;
                        }

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }

                            }
                            else
                                it.dreamPoint = 0;
                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch (Exception es)
                        {

                        }

                        if (lang.Trim() == "hi")
                        {
                            if (!string.IsNullOrEmpty(it.HindiName))
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }
                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }

                        item.ItemMasters.Add(it);
                    }

                }
                else
                {
                    //Increase some parameter For offer
                    var newdata = (from a in context.itemMasters
                                   where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.SubsubCategoryid == subSubCategoryId && (a.ItemAppType == 0 || a.ItemAppType == 1)
                                   && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       CompanyId = a.CompanyId,
                                       Categoryid = a.Categoryid,
                                       Discount = a.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       itemname = a.itemname,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = a.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName,
                                       VATTax = a.VATTax,
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
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UOM = a.UOM,
                                       UnitofQuantity = a.UnitofQuantity
                                   }).OrderByDescending(x => x.ItemNumber).ToList();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd
                    var custtype = 4;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                    }
                    #endregion

                    var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();


                    foreach (factoryItemdata it in newdata)
                    {
                        if (it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }


                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;

                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }

                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }

                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch (Exception es)
                        {

                        }
                        item.ItemMasters.Add(it);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new WRSITEM() { ItemMasters = item.ItemMasters, Message = true });

            }
        }

        #region SPA: for sales person app Get Sub cat item by spid Subsub id at app on home page
        /// <summary>
        /// Created by 19/12/2018 
        /// </summary>
        /// <param name="spid"></param>
        /// <param name="sscatid"></param>
        /// <returns></returns>

        [Route("getbysscatid")]
        [HttpGet]
        public HttpResponseMessage salesgetbysscatid(int PeopleId, int WarehouseId, int sscatid)
        {
            using (var context = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                WRSITEM item = new WRSITEM();
                var newdata = (from a in context.itemMasters
                               where (a.WarehouseId == WarehouseId && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid && (a.ItemAppType == 0 || a.ItemAppType == 1)
                                && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                               select new factoryItemdata
                               {
                                   WarehouseId = a.WarehouseId,
                                   CompanyId = a.CompanyId,
                                   IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                   ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                   Categoryid = a.Categoryid,
                                   Discount = a.Discount,
                                   ItemId = a.ItemId,
                                   ItemNumber = a.Number,
                                   itemname = a.itemname,
                                   LogoUrl = a.LogoUrl,
                                   MinOrderQty = a.MinOrderQty,
                                   price = a.price,
                                   SubCategoryId = a.SubCategoryId,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   TotalTaxPercentage = a.TotalTaxPercentage,
                                   SellingUnitName = a.SellingUnitName,
                                   SellingSku = a.SellingSku,
                                   UnitPrice = a.UnitPrice,
                                   HindiName = a.HindiName,
                                   VATTax = a.VATTax,
                                   active = a.active,
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
                                   IsSensitive = a.IsSensitive,
                                   IsSensitiveMRP = a.IsSensitiveMRP,
                                   UOM = a.UOM,
                                   UnitofQuantity = a.UnitofQuantity
                               }).OrderByDescending(x => x.ItemNumber).ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                foreach (var it in newdata)
                {
                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<factoryItemdata>();
                    }
                    try
                    {

                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //salesman 0.2=(0.02 * 10=0.2)
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }
                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                        }
                        catch (Exception ds) { }
                    }
                    catch { }
                    item.ItemMasters.Add(it);
                }

                if (item.ItemMasters != null)
                {
                    item.Message = true;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                else
                {
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }

            }
        }
        [Route("GetCompanyAllItem")]
        [HttpGet]
        public HttpResponseMessage GetCompanyAllItem(int PeopleId, int WarehouseId, int scatid, string lang)
        {
            using (var context = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                WRSITEM item = new WRSITEM();
                if (lang.Trim() == "hi")
                {
                    var newdata = (from a in context.itemMasters
                                   where (a.WarehouseId == WarehouseId && a.Deleted == false && a.active == true && a.SubCategoryId == scatid && (a.ItemAppType == 0 || a.ItemAppType == 1)
                                    && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       Categoryid = a.Categoryid,
                                       Discount = a.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = a.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName,
                                       VATTax = a.VATTax,
                                       active = a.active,
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
                                       itemBaseName = a.itemBaseName,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UOM = a.UOM,
                                       UnitofQuantity = a.UnitofQuantity
                                   }).OrderByDescending(x => x.ItemNumber).ToList();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd
                    var custtype = 4;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                    }
                    #endregion

                    foreach (var it in newdata)
                    {
                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //salesman 0.2=(0.02 * 10=0.2)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                    // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                    if (it.price > it.UnitPrice)
                                    {
                                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                            }
                            catch (Exception ds) { }
                        }
                        catch { }
                        if (lang.Trim() == "hi")
                        {
                            if (!string.IsNullOrEmpty(it.HindiName))
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }
                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                        item.ItemMasters.Add(it);
                    }

                }
                else
                {
                    var newdata = (from a in context.itemMasters
                                   where (a.WarehouseId == WarehouseId && a.Deleted == false && a.active == true && a.SubCategoryId == scatid && (a.ItemAppType == 0 || a.ItemAppType == 1)
                                    && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid))
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       Categoryid = a.Categoryid,
                                       Discount = a.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       itemname = a.itemname,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = a.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName,
                                       VATTax = a.VATTax,
                                       active = a.active,
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
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       UOM = a.UOM,
                                       UnitofQuantity = a.UnitofQuantity
                                   }).OrderByDescending(x => x.ItemNumber).ToList();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd
                    var custtype = 4;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                    }
                    #endregion

                    foreach (var it in newdata)
                    {
                        if (item.ItemMasters == null)
                        {
                            item.ItemMasters = new List<factoryItemdata>();
                        }
                        try
                        {

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //salesman 0.2=(0.02 * 10=0.2)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                    // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                    if (it.price > it.UnitPrice)
                                    {
                                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                            }
                            catch (Exception ds) { }
                        }
                        catch { }
                        item.ItemMasters.Add(it);
                    }
                }
                if (item.ItemMasters != null)
                {
                    item.Message = true;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                else
                {
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }

            }
        }
        #endregion

        /// <summary>
        ///for Brand All base by WarehouseId ,PeopleId
        /// </summary>
        /// <param name="WarehouseId"></param>    
        /// <param name="PeopleId"></param>       
        /// <returns>SubsubCategoryDTO</returns>
        [Route("GetAllBrand")]
        [HttpGet]
        public dynamic GetBrandWarehouseId(int PeopleId, int WarehouseId, string lang)
        {
            List<SubsubCategoryDTOM> ass = new List<SubsubCategoryDTOM>();
            try
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                // var CatIdstr = String.Join(",", CatIds);
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                // var SubCatsstr = String.Join(",", SubCats);
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                //var SubSubCatsstr = String.Join(",", SubSubCats);
                using (var authContext = new AuthContext())
                {
                    if (authContext.Database.Connection.State != ConnectionState.Open)
                        authContext.Database.Connection.Open();

                    var CatIdDt = new DataTable();
                    CatIdDt.Columns.Add("IntValue");
                    foreach (var item in CatIds)
                    {
                        var dr = CatIdDt.NewRow();
                        dr["IntValue"] = item;
                        CatIdDt.Rows.Add(dr);
                    }
                    var SubCatIdDt = new DataTable();
                    SubCatIdDt.Columns.Add("IntValue");
                    foreach (var item in SubCats)
                    {
                        var dr = SubCatIdDt.NewRow();
                        dr["IntValue"] = item;
                        SubCatIdDt.Rows.Add(dr);
                    }
                    var BrandIdDt = new DataTable();
                    BrandIdDt.Columns.Add("IntValue");
                    foreach (var item in SubSubCats)
                    {
                        var dr = BrandIdDt.NewRow();
                        dr["IntValue"] = item;
                        BrandIdDt.Rows.Add(dr);
                    }

                    var Catparam = new SqlParameter("categoryIds", CatIdDt);
                    Catparam.SqlDbType = SqlDbType.Structured;
                    Catparam.TypeName = "dbo.IntValues";
                    var SubCatparam = new SqlParameter("subCategoryIds", SubCatIdDt);
                    SubCatparam.SqlDbType = SqlDbType.Structured;
                    SubCatparam.TypeName = "dbo.IntValues";
                    var Brandparam = new SqlParameter("brandIds", BrandIdDt);
                    Brandparam.SqlDbType = SqlDbType.Structured;
                    Brandparam.TypeName = "dbo.IntValues";
                    var cmd = authContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetSalesAllBrand]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                    cmd.Parameters.Add(Catparam);
                    cmd.Parameters.Add(SubCatparam);
                    cmd.Parameters.Add(Brandparam);

                    // Run the sproc
                    var reader1 = cmd.ExecuteReader();
                    ass = ((IObjectContextAdapter)authContext)
                    .ObjectContext
                    .Translate<SubsubCategoryDTOM>(reader1).ToList();

                    ass.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi" && !string.IsNullOrEmpty(x.HindiName))
                        {
                            x.SubsubcategoryName = x.HindiName;
                        }
                    });
                    //string query = "select b.SubsubCategoryid,b.SubsubcategoryName,b.LogoUrl,b.HindiName,b.Categoryid,b.SubCategoryId,b.SubcategoryName SubcategoryName from ItemMasters a with(nolock) inner join SubsubCategories b with(nolock) on a.SubsubCategoryid=b.SubsubCategoryid " +
                    //              "and a.Deleted = 0  and a.active = 1  and a.WarehouseId = " + WarehouseId + "and b.Deleted =0 and b.IsActive =1 and a.SubsubCategoryid in(" + SubSubCatsstr + ") and a.SubCategoryId in(" + SubCatsstr + ")  and a.Categoryid in(" + CatIdstr + ")  group by b.SubsubCategoryid,b.SubsubcategoryName,b.LogoUrl,b.HindiName,b.Categoryid,b.SubCategoryId,b.SubcategoryName ";
                    //ass = db.Database.SqlQuery<SubsubCategoryDTOM>(query).ToList();
                    return ass;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GetAllBrand " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Created by 01/02/2019
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [Route("Category")]
        [HttpGet]
        public CategorySalesAppDc GetCategory(int PeopleId, int warehouseid, string lang)
        {
            CategorySalesAppDc ibjtosend = Categories(PeopleId, 0, warehouseid, lang);
            return ibjtosend;
        }


        [Route("GetCatelog")]
        [HttpGet]
        public CalelogDc GetCatelog(int PeopleId, int warehouseid, string lang)
        {
            CalelogDc objtosend = new CalelogDc();
            objtosend.SalesCategories = new List<SalesCategory>();
            objtosend.SalesCompanies = new List<SalesCompany>();
            objtosend.SalesBrands = new List<SalesBrand>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);

            #region block Barnd
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            var custtype = 4;
            var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
            if (blockBarnds != null && blockBarnds.Any())
            {
                StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
            }
            #endregion
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any())
                {
                    var IdDt = new DataTable();
                    SqlParameter param = null;

                    IdDt = new DataTable();
                    IdDt.Columns.Add("categoryId");
                    IdDt.Columns.Add("companyId");
                    IdDt.Columns.Add("brandId");
                    foreach (var item in StoreCategorySubCategoryBrands)
                    {
                        var dr = IdDt.NewRow();
                        dr["categoryId"] = item.Categoryid;
                        dr["companyId"] = item.SubCategoryId;
                        dr["brandId"] = item.BrandId;
                        IdDt.Rows.Add(dr);
                    }

                    param = new SqlParameter("CatCompanyBrand", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.CatCompanyBrand";

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetCatCompanyBrandforSalesApp]";
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseid));
                    cmd.Parameters.Add(param);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    objtosend.SalesCategories = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<SalesCategory>(reader).ToList();
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        objtosend.SalesCompanies = ((IObjectContextAdapter)context)
                                                    .ObjectContext
                                                    .Translate<SalesCompany>(reader).ToList();
                    }
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        objtosend.SalesBrands = ((IObjectContextAdapter)context)
                                                    .ObjectContext
                                                    .Translate<SalesBrand>(reader).ToList();
                    }

                }
            }

            if (lang == "hi")
            {
                foreach (var kk in objtosend.SalesCategories)
                {
                    if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                    {
                        kk.CategoryName = kk.HindiName;
                    }
                }

                foreach (var kk in objtosend.SalesCompanies)
                {
                    if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                    {
                        kk.SubcategoryName = kk.HindiName;
                    }
                }

                foreach (var kk in objtosend.SalesBrands)
                {
                    if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                    {
                        kk.SubsubcategoryName = kk.HindiName;
                    }
                }
            }
            objtosend.SalesCategories = objtosend.SalesCategories.OrderBy(x => x.CategoryName).ToList();
            objtosend.SalesBrands = objtosend.SalesBrands.OrderBy(x => x.SubsubcategoryName).ToList();
            objtosend.BrandCompanies = objtosend.SalesCompanies.Any() ? objtosend.SalesCompanies.GroupBy(x => x.SubCategoryId).Select(x => new SalesCompany
            {
                Categoryid = 0,
                HindiName = x.FirstOrDefault().HindiName,
                itemcount = x.Sum(y => y.itemcount),
                LogoUrl = x.FirstOrDefault().LogoUrl,
                Sequence = x.FirstOrDefault().Sequence,
                SubCategoryId = x.Key,
                SubcategoryName = x.FirstOrDefault().SubcategoryName
            }).OrderBy(x => x.Sequence).OrderBy(x => x.SubcategoryName).ToList() : new List<SalesCompany>();

            objtosend.Brands = objtosend.SalesBrands.Any() ? objtosend.SalesBrands.GroupBy(x => new { x.SubsubCategoryid, x.SubCategoryId }).Select(x => new SalesBrand
            {
                Categoryid = 0,
                HindiName = x.FirstOrDefault().HindiName,
                itemcount = x.Sum(y => y.itemcount),
                LogoUrl = x.FirstOrDefault().LogoUrl,
                SubCategoryId = x.Key.SubCategoryId,
                SubsubCategoryid = x.Key.SubsubCategoryid,
                SubsubcategoryName = x.FirstOrDefault().SubsubcategoryName
            }).OrderBy(x => x.SubsubcategoryName).ToList() : new List<SalesBrand>();



            return objtosend;
        }


        [Route("GetBrandItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<DataContracts.External.SalesItemResponseDc> GetBrandItem(int PeopleId, int customerId, int warehouseId, int companyId, int brandId, int skip, int take, string lang, string IncentiveClassifications)
        {
            List<string> IncentiveClassification = new List<string>();
            IncentiveClassification = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
            skip = skip / take;
            Customer ActiveCustomer = new Customer();

            #region variables
            var taskList = new List<Task>();
            int baseskip = 0, basetake = 0, suggestedskip = 0, suggestedTake = 0, promotionalskip = 0, promotionaltake = 0;
            string baseorderby = "ASC", suggestedorderby = "ASC", promotionalorderby = "ASC";
            int basescorefrom = -1, basescoreto = -1, suggestedscorefrom = -1, suggestedscoreto = -1, promotionalscorefrom = -1, promotionalscoreto = -1,
            baseIndex = 1, suggestedIndex = 2, promotionalIndex = 2, basedefaultitem = 0;
            bool isSuggestShow = false, isPromotionalShow = false, promotionalNewLaunch = false, promotionalItem = false, suggestedUnbilled = false;
            #endregion
            var itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.SalesAppItemDataDC>() };


            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStoresNewSales(PeopleId);

            #region block Barnd
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            var custtype = 4;
            var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
            if (blockBarnds != null && blockBarnds.Any())
            {
                StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
            }
            #endregion


            var PromotionalStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands;
            var SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands;//.Where(x => x.SubCategoryId == companyId && x.BrandId != brandId).ToList();
            StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.SubCategoryId == companyId && ((brandId > 0 && x.BrandId == brandId) || (brandId == 0 && x.BrandId == x.BrandId))).ToList();
            //var SuggestedStoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            if (brandId == 0)
            {
                PromotionalStoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
                SuggestedStoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            }
            else if (companyId > 0 && brandId == 0)
                SuggestedStoreCategorySubCategoryBrands = SuggestedStoreCategorySubCategoryBrands.Where(x => x.SubCategoryId != companyId).ToList();
            else if (companyId > 0 && brandId > 0)
                SuggestedStoreCategorySubCategoryBrands = SuggestedStoreCategorySubCategoryBrands.Where(x => x.SubCategoryId == companyId && brandId != x.BrandId).ToList();

            using (var context = new AuthContext())
            {
                ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                #region City wise configuration working
                List<CatelogConfig> categlogconfigs = new List<CatelogConfig>();
                var cityId = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId && x.active && !x.Deleted)?.Cityid;
                var Cityconfigs = context.CatelogConfigs.Where(x => x.IsActive && x.CityId == cityId).ToList();

                if (Cityconfigs != null && Cityconfigs.Any())
                {
                    categlogconfigs = Cityconfigs.ToList();
                }
                else
                {
                    categlogconfigs = context.CatelogConfigs.Where(x => x.IsActive && x.CityId == 0).ToList();
                }
                #endregion
                if (categlogconfigs != null && categlogconfigs.Any(x => x.ConfigName == "BaseListing"))
                {
                    baseIndex = categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").Sequence;
                    baseskip = skip;
                    basetake = categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").ItemCount;
                    basedefaultitem = basetake;
                    baseorderby = categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").Sort;
                    baseorderby = !string.IsNullOrEmpty(baseorderby) && baseorderby.ToLower() == "leastsold" ? "ASC" : "DESC";
                    basescorefrom = Convert.ToInt16(categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").ScoreFrom ?? -1);
                    basescoreto = Convert.ToInt16(categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").ScoreTo ?? -1);
                }
                if (categlogconfigs != null && categlogconfigs.Any(x => x.ConfigName == "SuggestedItems" && x.Status == true))
                {
                    suggestedIndex = categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").Sequence;
                    suggestedskip = skip;
                    suggestedTake = categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").ItemCount;
                    suggestedorderby = categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").Sort;
                    //suggestedorderby = !string.IsNullOrEmpty(suggestedorderby) && suggestedorderby.ToLower() == "leastsold" ? "ASC" : "DESC";
                    suggestedorderby = !string.IsNullOrEmpty(suggestedorderby) && suggestedorderby.ToLower() == "random"
                           ? suggestedorderby.ToUpper() : !string.IsNullOrEmpty(suggestedorderby) && suggestedorderby.ToLower() == "leastsold" ? "ASC" : "DESC";
                    suggestedscorefrom = Convert.ToInt16(categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").ScoreFrom ?? -1);
                    suggestedscoreto = Convert.ToInt16(categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").ScoreTo ?? -1);
                    suggestedUnbilled = categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").Unbilled != null ? true : false;
                    isSuggestShow = suggestedTake > 0 ? true : false;
                }
                if (categlogconfigs != null && categlogconfigs.Any(x => x.ConfigName == "PromotionalItems" && x.Status == true))
                {
                    promotionalIndex = categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").Sequence;
                    promotionalskip = skip;
                    promotionaltake = categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").ItemCount;
                    promotionalorderby = categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").Sort;
                    promotionalorderby = !string.IsNullOrEmpty(promotionalorderby) && promotionalorderby.ToLower() == "custom" ? "Custom" : "Random";
                    promotionalNewLaunch = (bool)categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").NewLaunch;
                    promotionalItem = (bool)categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").PromotionalItems;
                    isPromotionalShow = promotionaltake > 0 ? true : false;
                }

            }

            List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

            //--Suggested Item
            if (isSuggestShow && suggestedTake > 0)
            {
                //var tasksuggestedItem = Task.Factory.StartNew(() =>
                //{
                if (SuggestedStoreCategorySubCategoryBrands != null && SuggestedStoreCategorySubCategoryBrands.Any())
                {
                    using (var context = new AuthContext())
                    {
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();


                        List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                        if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                        {
                            var IdDt = new DataTable();
                            SqlParameter param = null;

                            IdDt = new DataTable();
                            IdDt.Columns.Add("categoryId");
                            IdDt.Columns.Add("companyId");
                            IdDt.Columns.Add("brandId");
                            foreach (var item in SuggestedStoreCategorySubCategoryBrands)
                            {
                                var dr = IdDt.NewRow();
                                dr["categoryId"] = item.Categoryid;
                                dr["companyId"] = item.SubCategoryId;
                                dr["brandId"] = item.BrandId;
                                IdDt.Rows.Add(dr);
                            }

                            param = new SqlParameter("CatCompanyBrand", IdDt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.CatCompanyBrand";


                            var IcDt = new DataTable();
                            SqlParameter param1 = null;

                            IcDt = new DataTable();
                            IcDt.Columns.Add("stringValue");
                            if (IncentiveClassification.Count > 0)
                            {
                                foreach (var item in IncentiveClassification)
                                {
                                    var dr = IcDt.NewRow();
                                    dr["stringValue"] = item;
                                    IcDt.Rows.Add(dr);
                                }
                            }

                            param1 = new SqlParameter("Classification", IcDt);
                            param1.SqlDbType = SqlDbType.Structured;
                            param1.TypeName = "dbo.stringValues";

                            var cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[GetItemforSalesApp]";
                            cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                            cmd.Parameters.Add(param);
                            cmd.Parameters.Add(param1);
                            cmd.Parameters.Add(new SqlParameter("@SearchKeyWord", ""));
                            cmd.Parameters.Add(new SqlParameter("@ScoreFrom", suggestedscorefrom));
                            cmd.Parameters.Add(new SqlParameter("@ScoreTo", suggestedscoreto));
                            cmd.Parameters.Add(new SqlParameter("@skip", suggestedskip * suggestedTake));
                            cmd.Parameters.Add(new SqlParameter("@take", suggestedTake));
                            cmd.Parameters.Add(new SqlParameter("@orderby", suggestedorderby));
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            // Run the sproc
                            var reader = cmd.ExecuteReader();
                            ItemData = ((IObjectContextAdapter)context)
                            .ObjectContext
                            .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                            reader.NextResult();
                            //if (reader.Read())
                            //{
                            //    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                            //}
                        }
                        else
                        {
                            ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                            var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesCatelogElasticData(warehouseId, SuggestedStoreCategorySubCategoryBrands, null, "", suggestedscorefrom, suggestedscoreto, (suggestedskip * suggestedTake), suggestedTake, suggestedorderby, true, IncentiveClassification));
                            ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                            // itemResponseDc.TotalItem = data.TotalItem;

                        }

                        if (ItemData.Any())
                        {
                            basetake = take - (ItemData.Count == suggestedTake ? suggestedTake : ItemData.Count);
                        }

                        var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();
                        BackendOrderController backendOrderController = new BackendOrderController();
                        foreach (var it in ItemData)
                        {
                            double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price,it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                            it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);

                            //Condition for offer end
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;

                            }

                            if (it.OfferCategory == 1)
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //Customer (0.2 * 10=1)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }

                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }

                            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                            it.Itemtype = 2;
                            it.Sequence = suggestedIndex;
                            ItemDataDCs.Add(it);
                        }

                        //itemResponseDc.ItemDataDCs.AddRange(ItemDataDCs);

                    }

                }
                //});
                //taskList.Add(tasksuggestedItem);
            }
            //--promotional Item
            if (isPromotionalShow && promotionaltake > 0)
            {
                //var taskpromotionalItem = Task.Factory.StartNew(() =>
                //{
                if (brandId != 0 && PromotionalStoreCategorySubCategoryBrands != null && PromotionalStoreCategorySubCategoryBrands.Any())
                {
                    using (var context = new AuthContext())
                    {
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        if (PromotionalStoreCategorySubCategoryBrands != null && PromotionalStoreCategorySubCategoryBrands.Any())
                        {
                            List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                            if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                            {
                                var IdDt = new DataTable();
                                SqlParameter param = null;

                                IdDt = new DataTable();
                                IdDt.Columns.Add("categoryId");
                                IdDt.Columns.Add("companyId");
                                IdDt.Columns.Add("brandId");
                                foreach (var item in PromotionalStoreCategorySubCategoryBrands)
                                {
                                    var dr = IdDt.NewRow();
                                    dr["categoryId"] = item.Categoryid;
                                    dr["companyId"] = item.SubCategoryId;
                                    dr["brandId"] = item.BrandId;
                                    IdDt.Rows.Add(dr);
                                }

                                param = new SqlParameter("CatCompanyBrand", IdDt);
                                param.SqlDbType = SqlDbType.Structured;
                                param.TypeName = "dbo.CatCompanyBrand";

                                var IcDt = new DataTable();
                                SqlParameter param1 = null;

                                IcDt = new DataTable();
                                IcDt.Columns.Add("stringValue");
                                if (IncentiveClassification.Count > 0)
                                {
                                    foreach (var item in IncentiveClassification)
                                    {
                                        var dr = IcDt.NewRow();
                                        dr["stringValue"] = item;
                                        IcDt.Rows.Add(dr);
                                    }
                                }

                                param1 = new SqlParameter("Classification", IcDt);
                                param1.SqlDbType = SqlDbType.Structured;
                                param1.TypeName = "dbo.stringValues";

                                var cmd = context.Database.Connection.CreateCommand();
                                cmd.CommandText = "[dbo].[GetPromotionalItemforSalesApp]";
                                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                                cmd.Parameters.Add(param);
                                cmd.Parameters.Add(param1);
                                cmd.Parameters.Add(new SqlParameter("@SearchKeyWord", ""));
                                cmd.Parameters.Add(new SqlParameter("@promotionalNewLaunch", promotionalNewLaunch));
                                cmd.Parameters.Add(new SqlParameter("@promotionalItems", promotionalItem));
                                cmd.Parameters.Add(new SqlParameter("@skip", promotionalskip * promotionaltake));
                                cmd.Parameters.Add(new SqlParameter("@take", promotionaltake));
                                cmd.Parameters.Add(new SqlParameter("@orderby", promotionalorderby));
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                                // Run the sproc
                                var reader = cmd.ExecuteReader();
                                ItemData = ((IObjectContextAdapter)context)
                                .ObjectContext
                                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                                reader.NextResult();
                                //if (reader.Read())
                                //{
                                //    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                                //}
                            }
                            else
                            {
                                var ItemNumbersList = new List<string>();
                                if (promotionalNewLaunch)
                                {
                                    string query = "Exec GetNewLaunchItemNumbers";
                                    var NewLaunchItems = context.Database.SqlQuery<string>(query).ToList();
                                    ItemNumbersList.AddRange(NewLaunchItems);
                                }
                                if (promotionalItem || (!promotionalNewLaunch && !promotionalItem))
                                {
                                    string query = "Exec GetPromotionalItemNumber " + PeopleId + "," + warehouseId + ",0";
                                    var promotionalItems = context.Database.SqlQuery<string>(query).ToList();
                                    ItemNumbersList.AddRange(promotionalItems);
                                }
                                ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                                var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesPromotionalCatelogElasticData(warehouseId, PromotionalStoreCategorySubCategoryBrands, ItemNumbersList, "", (promotionalskip * promotionaltake), promotionaltake, promotionalorderby, true, IncentiveClassification));
                                ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                                // itemResponseDc.TotalItem = data.TotalItem;
                            }
                            if (ItemData.Any() && isSuggestShow && suggestedTake > 0 && ItemDataDCs.Count > 0)
                                basetake = basetake - (ItemData.Count == promotionaltake ? promotionaltake : ItemData.Count);
                            else if (ItemData.Any() && ItemData.Count > 0)
                                basetake = take - (ItemData.Count == promotionaltake ? promotionaltake : ItemData.Count);


                            var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                            var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();
                            BackendOrderController backendOrderController = new BackendOrderController();
                            foreach (var it in ItemData)
                            {
                                double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                                it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);

                                //Condition for offer end
                                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                {
                                    if (it.OfferCategory == 2)
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                    else if (it.OfferCategory == 1)
                                    {
                                        it.IsOffer = false;
                                        it.OfferCategory = 0;
                                    }

                                }
                                else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;

                                }

                                if (it.OfferCategory == 1)
                                {
                                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                        it.IsOffer = true;
                                    else
                                        it.IsOffer = false;
                                }

                                try
                                {
                                    if (!it.IsOffer)
                                    {
                                        /// Dream Point Logic && Margin Point
                                        int? MP, PP;
                                        double xPoint = xPointValue * 10;
                                        //Customer (0.2 * 10=1)
                                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                        {
                                            PP = 0;
                                        }
                                        else
                                        {
                                            PP = it.promoPerItems;
                                        }
                                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                        {
                                            MP = 0;
                                        }
                                        else
                                        {
                                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                        }
                                        if (PP > 0 && MP > 0)
                                        {
                                            int? PP_MP = PP + MP;
                                            it.dreamPoint = PP_MP;
                                        }
                                        else if (MP > 0)
                                        {
                                            it.dreamPoint = MP;
                                        }
                                        else if (PP > 0)
                                        {
                                            it.dreamPoint = PP;
                                        }
                                        else
                                        {
                                            it.dreamPoint = 0;
                                        }
                                    }
                                    else { it.dreamPoint = 0; }

                                    // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                    if (it.price > it.UnitPrice)
                                    {
                                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                                catch { }

                                if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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
                                it.Itemtype = 3;
                                it.Sequence = promotionalIndex;
                                ItemDataDCs.Add(it);
                            }

                            // itemResponseDc.ItemDataDCs.AddRange(ItemDataDCs);
                        }

                    }

                }
                //});
                //taskList.Add(taskpromotionalItem);
            }
            //Task.WaitAll(taskList.ToArray());
            //--- Base Item
            //var taskbaseItem = Task.Factory.StartNew(() =>
            //{
            if ((basetake == basedefaultitem) && ItemDataDCs.Count == 0)
            {
                basetake = take;
            }
            /*else
            {
                basetake = basedefaultitem;
            }*/
            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any() && basetake > 0)
            {
                using (var context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();


                    List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                    if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                    {
                        var IdDt = new DataTable();
                        SqlParameter param = null;

                        IdDt = new DataTable();
                        IdDt.Columns.Add("categoryId");
                        IdDt.Columns.Add("companyId");
                        IdDt.Columns.Add("brandId");
                        foreach (var item in StoreCategorySubCategoryBrands)
                        {
                            var dr = IdDt.NewRow();
                            dr["categoryId"] = item.Categoryid;
                            dr["companyId"] = item.SubCategoryId;
                            dr["brandId"] = item.BrandId;
                            IdDt.Rows.Add(dr);
                        }

                        param = new SqlParameter("CatCompanyBrand", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.CatCompanyBrand";

                        var IcDt = new DataTable();
                        SqlParameter param1 = null;

                        IcDt = new DataTable();
                        IcDt.Columns.Add("stringValue");
                        if (IncentiveClassification.Count > 0)
                        {
                            foreach (var item in IncentiveClassification)
                            {
                                var dr = IcDt.NewRow();
                                dr["stringValue"] = item;
                                IcDt.Rows.Add(dr);
                            }
                        }

                        param1 = new SqlParameter("Classification", IcDt);
                        param1.SqlDbType = SqlDbType.Structured;
                        param1.TypeName = "dbo.stringValues";

                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetItemforSalesApp]";
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(param1);
                        cmd.Parameters.Add(new SqlParameter("@SearchKeyWord", ""));
                        cmd.Parameters.Add(new SqlParameter("@ScoreFrom", basescorefrom));
                        cmd.Parameters.Add(new SqlParameter("@ScoreTo", basescoreto));
                        cmd.Parameters.Add(new SqlParameter("@skip", (baseskip * basetake)));
                        cmd.Parameters.Add(new SqlParameter("@take", basetake));
                        cmd.Parameters.Add(new SqlParameter("@orderby", baseorderby));

                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        ItemData = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                        reader.NextResult();
                        if (reader.Read())
                        {
                            itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                        }
                    }
                    else
                    {
                        ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                        var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesBaseCatelogElasticData(warehouseId, StoreCategorySubCategoryBrands, null, "", (baseskip * basetake), basetake, baseorderby, true, IncentiveClassification));
                        ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                        itemResponseDc.TotalItem = data.TotalItem;
                    }

                    var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                    if (ItemData != null && ItemData.Any())
                    {
                        BackendOrderController backendOrderController = new BackendOrderController();
                        foreach (var it in ItemData)
                        {
                            double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                            it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);

                            //Condition for offer end
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;

                            }

                            if (it.OfferCategory == 1)
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //Customer (0.2 * 10=1)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }

                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }

                            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                            it.Itemtype = 1;
                            it.Sequence = baseIndex;
                            ItemDataDCs.Add(it);
                        }
                    }
                    /* else
                     {
                         ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                     }*/

                }

            }

            //});

            //taskList.Add(taskbaseItem);

            if (ItemDataDCs != null && ItemDataDCs.Any())
            {
                var enddate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                var startDate = DateTime.Now.AddMonths(-9).Date.ToString("yyyy-MM-dd");
                var itemmultiMrpIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                List<DataContracts.External.orderMrpData> orderdetails = new List<DataContracts.External.orderMrpData>();
                ParallelLoopResult parellelResult = Parallel.ForEach(itemmultiMrpIds, (mrpid) =>
                {
                    //    foreach (var mrpid in itemmultiMrpIds)
                    //{
                    string query = $"SELECT top 1 itemmultimrpid,createddate createddate, ordqty Qty from skorderdata_{AppConstants.Environment} where itemmultimrpid in ({ mrpid })   and whid={warehouseId} and custid={customerId}  and createddate>='{startDate}' and createddate <= '{enddate}'  order by createddate desc";

                    ElasticSqlHelper<DataContracts.External.orderMrpData> elasticSqlHelper = new ElasticSqlHelper<DataContracts.External.orderMrpData>();
                    var order = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).FirstOrDefault());
                    if (order != null)
                        orderdetails.Add(order);
                });


                /*MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
               
                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted
                                                    && x.CustomerId == customerId && x.WarehouseId == warehouseId
                                                    // && x.orderDetails.Any(y => itemmultiMrpIds.Contains(y.ItemMultiMRPId)
                                                    // && x.CreatedDate >= startDate && x.CreatedDate <= enddate
                                                    );
                var ordercollection = mongoDbHelper.mongoDatabase.GetCollection<MongoOrderMaster>("MongoOrderMaster").AsQueryable();
                var orderdetails = ordercollection.Where(orderPredicate)
                                    .SelectMany(t => t.orderDetails, (t, a) => new
                                    {
                                        CreatedDate = t.CreatedDate,
                                        ItemMultiMRPId = a.ItemMultiMRPId,
                                        Qty = a.qty
                                    }).Where(x => itemmultiMrpIds.Contains(x.ItemMultiMRPId))
                                    .ToList();
*/
                var itemMultiMRPIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                using (var context = new AuthContext())
                {
                    ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, warehouseId, context);
                }
                if (parellelResult.IsCompleted)
                {
                    List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                    if (ItemDataDCs != null && ItemDataDCs.Any())
                    {
                        ItemMasterManager itemMasterManager = new ItemMasterManager();
                        itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemDataDCs.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                    }

                    foreach (var item in ItemDataDCs)
                    {
                        item.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == item.ItemMultiMRPId)?.Classification;
                        item.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == item.ItemMultiMRPId)?.BackgroundRgbColor;


                        if (orderdetails != null && orderdetails.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId))
                        {
                            item.LastOrderDate = orderdetails.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate;
                            item.LastOrderQty = orderdetails.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().Qty;
                            item.LastOrderDays = (DateTime.Today - item.LastOrderDate).Value.Days;
                        }

                        if (item.price > item.UnitPrice)
                        {
                            item.marginPoint = item.UnitPrice > 0 ? (((item.price - item.UnitPrice) * 100) / item.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                            if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.PTR > 0))
                            {
                                var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                                var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                                var UPMRPMargin = item.marginPoint.Value;
                                if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                    item.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                            }

                        }
                        else
                        {
                            item.marginPoint = 0;
                        }
                    }
                }
                itemResponseDc.ItemDataDCs = ItemDataDCs.GroupBy(x => new { x.ItemNumber, x.Itemtype }).Select(x => new DataContracts.External.SalesAppItemDataDC
                {
                    BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                    BillLimitQty = x.FirstOrDefault().BillLimitQty,
                    Categoryid = x.FirstOrDefault().Categoryid,
                    CompanyId = x.FirstOrDefault().CompanyId,
                    dreamPoint = x.FirstOrDefault().dreamPoint,
                    HindiName = x.FirstOrDefault().HindiName,
                    IsItemLimit = x.FirstOrDefault().IsItemLimit,
                    IsOffer = x.FirstOrDefault().IsOffer,
                    ItemId = x.FirstOrDefault().ItemId,
                    ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                    ItemMultiMRPId = x.FirstOrDefault().ItemMultiMRPId,
                    itemname = x.FirstOrDefault().itemname,
                    ItemNumber = x.FirstOrDefault().ItemNumber,
                    Itemtype = x.FirstOrDefault().Itemtype,
                    LastOrderDate = x.FirstOrDefault().LastOrderDate,
                    LastOrderDays = x.FirstOrDefault().LastOrderDays,
                    LastOrderQty = x.FirstOrDefault().LastOrderQty,
                    LogoUrl = x.FirstOrDefault().LogoUrl,
                    marginPoint = x.FirstOrDefault().marginPoint,
                    MinOrderQty = x.FirstOrDefault().MinOrderQty,
                    OfferCategory = x.FirstOrDefault().OfferCategory,
                    OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                    OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                    OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                    OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                    OfferId = x.FirstOrDefault().OfferId,
                    OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                    OfferType = x.FirstOrDefault().OfferType,
                    OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                    price = x.FirstOrDefault().price,
                    Sequence = x.FirstOrDefault().Sequence,
                    SubCategoryId = x.FirstOrDefault().SubCategoryId,
                    SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                    UnitPrice = x.FirstOrDefault().UnitPrice,
                    WarehouseId = x.FirstOrDefault().WarehouseId,
                    Classification = x.FirstOrDefault().Classification,
                    BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                    moqList = x.Count() > 1 ? x.Select(y => new DataContracts.External.SalesAppItemDataDC
                    {
                        isChecked = (y.ItemMultiMRPId == x.FirstOrDefault().ItemMultiMRPId && y.MinOrderQty == x.FirstOrDefault().MinOrderQty),
                        BaseCategoryId = y.BaseCategoryId,
                        BillLimitQty = y.BillLimitQty,
                        Categoryid = y.Categoryid,
                        CompanyId = y.CompanyId,
                        dreamPoint = y.dreamPoint,
                        HindiName = y.HindiName,
                        IsItemLimit = y.IsItemLimit,
                        IsOffer = y.IsOffer,
                        ItemId = y.ItemId,
                        ItemlimitQty = y.ItemlimitQty,
                        ItemMultiMRPId = y.ItemMultiMRPId,
                        itemname = y.itemname,
                        ItemNumber = y.ItemNumber,
                        Itemtype = y.Itemtype,
                        LastOrderDate = y.LastOrderDate,
                        LastOrderDays = y.LastOrderDays,
                        LastOrderQty = y.LastOrderQty,
                        LogoUrl = y.LogoUrl,
                        marginPoint = y.marginPoint,
                        MinOrderQty = y.MinOrderQty,
                        OfferCategory = y.OfferCategory,
                        OfferFreeItemId = y.OfferFreeItemId,
                        OfferFreeItemImage = y.OfferFreeItemImage,
                        OfferFreeItemName = y.OfferFreeItemName,
                        OfferFreeItemQuantity = y.OfferFreeItemQuantity,
                        OfferId = y.OfferId,
                        OfferMinimumQty = y.OfferMinimumQty,
                        OfferType = y.OfferType,
                        OfferWalletPoint = y.OfferWalletPoint,
                        price = y.price,
                        Sequence = y.Sequence,
                        SubCategoryId = y.SubCategoryId,
                        SubsubCategoryid = y.SubsubCategoryid,
                        UnitPrice = y.UnitPrice,
                        WarehouseId = y.WarehouseId,
                        Classification = x.FirstOrDefault().Classification,
                        BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                    }).ToList() : new List<DataContracts.External.SalesAppItemDataDC>()
                }).OrderBy(x => x.Sequence).ToList();

            }
            return itemResponseDc;
        }

        [Route("GetCategoryItem")]
        [HttpGet]
        public async Task<DataContracts.External.SalesItemResponseDc> GetCategoryItem(int PeopleId, int customerId, int warehouseId, int categoryId, int companyId, int brandId, int skip, int take, string lang, string IncentiveClassifications)
        {
            List<string> IncentiveClassification = new List<string>();
            IncentiveClassification = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
            skip = skip / take;
            Customer ActiveCustomer = new Customer();
            var itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.SalesAppItemDataDC>() };


            #region variables
            var taskList = new List<Task>();
            int baseskip = 0, basetake = 0, suggestedskip = 0, suggestedTake = 0, promotionalskip = 0, promotionaltake = 0;
            string baseorderby = "ASC", suggestedorderby = "ASC", promotionalorderby = "ASC";
            int basescorefrom = -1, basescoreto = -1, suggestedscorefrom = -1, suggestedscoreto = -1,
                baseIndex = 1, suggestedIndex = 2, promotionalIndex = 2, basedefaultitem = 0;
            bool isSuggestShow = false, isPromotionalShow = false, suggestedUnbilled = false, basesUnbilled = false
                , promotionalNewLaunch = false, promotionalItem = false;


            #endregion

            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStoresNewSales(PeopleId);

            #region block Barnd
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            var custtype = 4;
            var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
            if (blockBarnds != null && blockBarnds.Any())
            {
                StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
            }
            #endregion

            var PromotionalStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands;

            //var SuggestedStoreCategorySubCategoryBrands = companyId==0 && brandId==0 ? null:  StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
            //                                                                 && (companyId > 0 && brandId==0 && companyId != x.SubCategoryId)
            //                                                                 && (companyId >=0 && brandId >0 && x.BrandId != brandId)).ToList();

            var SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands;
            if (companyId == 0 && brandId == 0)
            {
                int suggestedCategoryId = await GetSuggestedCategoryId(categoryId);
                if (suggestedCategoryId > 0)
                    SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == suggestedCategoryId).ToList();
                else
                    SuggestedStoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
                //PromotionalStoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            }
            else if (companyId > 0 && brandId == 0)
                SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
                                                                              && (x.SubCategoryId != companyId)).ToList();
            else if (companyId == 0 && brandId > 0)
                SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
                                                                              && (x.BrandId != brandId)).ToList();
            else if (companyId > 0 && brandId > 0)
                SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
                                                                              && (x.SubCategoryId == companyId && x.BrandId != brandId)).ToList();

            StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
                                                                            && ((brandId > 0 && x.BrandId == brandId) || (brandId == 0 && x.BrandId == x.BrandId))
                                                                            && ((companyId > 0 && x.SubCategoryId == companyId) || (companyId == 0 && x.SubCategoryId == x.SubCategoryId))).ToList();


            using (var context = new AuthContext())
            {
                ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                #region City wise configuration working
                List<CatelogConfig> categlogconfigs = new List<CatelogConfig>();
                var cityId = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId && x.active && !x.Deleted)?.Cityid;
                var Cityconfigs = context.CatelogConfigs.Where(x => x.IsActive && x.CityId == cityId).ToList();

                if (Cityconfigs != null && Cityconfigs.Any())
                {
                    categlogconfigs = Cityconfigs.ToList();
                }
                else
                {
                    categlogconfigs = context.CatelogConfigs.Where(x => x.IsActive && x.CityId == 0).ToList();
                }
                #endregion
                //var categlogconfigs = context.CatelogConfigs.Where(x => x.IsActive).ToList();
                if (categlogconfigs != null && categlogconfigs.Any(x => x.ConfigName == "BaseListing"))
                {
                    baseIndex = categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").Sequence;
                    basetake = categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").ItemCount;
                    baseskip = skip;
                    basedefaultitem = basetake;
                    baseorderby = categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").Sort;
                    baseorderby = !string.IsNullOrEmpty(baseorderby) && baseorderby.ToLower() == "leastsold" ? "ASC" : "DESC";
                    basescorefrom = Convert.ToInt16(categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").ScoreFrom ?? -1);
                    basescoreto = Convert.ToInt16(categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").ScoreTo ?? -1);
                    basesUnbilled = categlogconfigs.FirstOrDefault(x => x.ConfigName == "BaseListing").Unbilled != null ? true : false;
                }
                if (categlogconfigs != null && categlogconfigs.Any(x => x.ConfigName == "SuggestedItems" && x.Status == true))
                {

                    suggestedIndex = categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").Sequence;
                    suggestedTake = categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").ItemCount;
                    suggestedskip = skip;
                    suggestedorderby = categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").Sort;
                    //suggestedorderby=!string.IsNullOrEmpty(suggestedorderby) && suggestedorderby.ToLower() == "leastsold" ? "ASC" : "DESC";
                    suggestedorderby = !string.IsNullOrEmpty(suggestedorderby) && suggestedorderby.ToLower() == "random"
                           ? suggestedorderby.ToUpper() : !string.IsNullOrEmpty(suggestedorderby) && suggestedorderby.ToLower() == "leastsold" ? "ASC" : "DESC";
                    suggestedscorefrom = Convert.ToInt16(categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").ScoreFrom ?? -1);
                    suggestedscoreto = Convert.ToInt16(categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").ScoreTo ?? -1);
                    suggestedUnbilled = categlogconfigs.FirstOrDefault(x => x.ConfigName == "SuggestedItems").Unbilled != null ? true : false;
                    isSuggestShow = suggestedTake > 0 ? true : false;

                }
                if (categlogconfigs != null && categlogconfigs.Any(x => x.ConfigName == "PromotionalItems" && x.Status == true))
                {

                    promotionalIndex = categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").Sequence;
                    promotionaltake = categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").ItemCount;
                    promotionalskip = skip;
                    promotionalorderby = categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").Sort;
                    promotionalorderby = !string.IsNullOrEmpty(promotionalorderby) && promotionalorderby.ToLower() == "custom" ? "Custom" : "Random";
                    promotionalNewLaunch = (bool)categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").NewLaunch;
                    promotionalItem = (bool)categlogconfigs.FirstOrDefault(x => x.ConfigName == "PromotionalItems").PromotionalItems;

                    isPromotionalShow = promotionaltake > 0 ? true : false;
                }



            }

            List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

            //--Suggested Item            
            if (isSuggestShow)
            {
                //var tasksuggestedItem = Task.Factory.StartNew(() =>
                //{
                if (SuggestedStoreCategorySubCategoryBrands != null && SuggestedStoreCategorySubCategoryBrands.Any())
                {
                    using (var context = new AuthContext())
                    {
                        List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                        if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                        {
                            if (context.Database.Connection.State != ConnectionState.Open)
                                context.Database.Connection.Open();

                            var IdDt = new DataTable();
                            SqlParameter param = null;

                            IdDt = new DataTable();
                            IdDt.Columns.Add("categoryId");
                            IdDt.Columns.Add("companyId");
                            IdDt.Columns.Add("brandId");
                            foreach (var item in SuggestedStoreCategorySubCategoryBrands)
                            {
                                var dr = IdDt.NewRow();
                                dr["categoryId"] = item.Categoryid;
                                dr["companyId"] = item.SubCategoryId;
                                dr["brandId"] = item.BrandId;
                                IdDt.Rows.Add(dr);
                            }

                            param = new SqlParameter("CatCompanyBrand", IdDt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.CatCompanyBrand";

                            var IcDt = new DataTable();
                            SqlParameter param1 = null;

                            IcDt = new DataTable();
                            IcDt.Columns.Add("stringValue");
                            if (IncentiveClassification.Count > 0)
                            {
                                foreach (var item in IncentiveClassification)
                                {
                                    var dr = IcDt.NewRow();
                                    dr["stringValue"] = item;
                                    IcDt.Rows.Add(dr);
                                }
                            }

                            param1 = new SqlParameter("Classification", IcDt);
                            param1.SqlDbType = SqlDbType.Structured;
                            param1.TypeName = "dbo.stringValues";

                            var cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[GetItemforSalesApp]";
                            cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                            cmd.Parameters.Add(param);
                            cmd.Parameters.Add(param1);
                            cmd.Parameters.Add(new SqlParameter("@SearchKeyWord", ""));
                            cmd.Parameters.Add(new SqlParameter("@ScoreFrom", suggestedscorefrom));
                            cmd.Parameters.Add(new SqlParameter("@ScoreTo", suggestedscoreto));
                            cmd.Parameters.Add(new SqlParameter("@skip", (suggestedskip * suggestedTake)));
                            cmd.Parameters.Add(new SqlParameter("@take", suggestedTake));
                            //cmd.Parameters.Add(new SqlParameter("@Unbilled", suggestedUnbilled));
                            cmd.Parameters.Add(new SqlParameter("@orderby", suggestedorderby));

                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            // Run the sproc
                            var reader = cmd.ExecuteReader();
                            ItemData = ((IObjectContextAdapter)context)
                            .ObjectContext
                            .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                            reader.NextResult();
                            //if (reader.Read())
                            //{
                            //    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                            //}
                        }
                        else
                        {
                            ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                            var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesSuggestedCatelogElasticData(warehouseId, SuggestedStoreCategorySubCategoryBrands, null, "", suggestedscorefrom, suggestedscoreto, (suggestedskip * suggestedTake), suggestedTake, suggestedorderby, true, IncentiveClassification));
                            ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                            // itemResponseDc.TotalItem = data.TotalItem;

                        }
                        if (ItemData.Any())
                        {
                            int ItemDataCount = ItemData.GroupBy(x => new { x.ItemNumber, x.WarehouseId, x.Rating }).Count();
                            basetake = take - (ItemDataCount == suggestedTake ? suggestedTake : ItemDataCount);
                        }

                        var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                        List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                        if (ItemData != null && ItemData.Any())
                        {
                            ItemMasterManager itemMasterManager = new ItemMasterManager();
                            itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                        }

                        BackendOrderController backendOrderController = new BackendOrderController();
                        foreach (var it in ItemData)
                        {

                            it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                            it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;
                            double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                            it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);

                            //Condition for offer end
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;

                            }

                            if (it.OfferCategory == 1)
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //Customer (0.2 * 10=1)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }

                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }

                            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                            it.Itemtype = 2;
                            it.Sequence = suggestedIndex;
                            ItemDataDCs.Add(it);
                        }

                        //itemResponseDc.ItemDataDCs.AddRange(ItemDataDCs);

                    }

                }
                //});
                //taskList.Add(tasksuggestedItem);
            }
            //--promotional Item
            if (isPromotionalShow)
            {
                //var taskpromotionalItem = Task.Factory.StartNew(() =>
                //{
                if (PromotionalStoreCategorySubCategoryBrands != null && PromotionalStoreCategorySubCategoryBrands.Any())
                {
                    using (var context = new AuthContext())
                    {
                        List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                        if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                        {
                            if (context.Database.Connection.State != ConnectionState.Open)
                                context.Database.Connection.Open();

                            var IdDt = new DataTable();
                            SqlParameter param = null;

                            IdDt = new DataTable();
                            IdDt.Columns.Add("categoryId");
                            IdDt.Columns.Add("companyId");
                            IdDt.Columns.Add("brandId");
                            foreach (var item in PromotionalStoreCategorySubCategoryBrands)
                            {
                                var dr = IdDt.NewRow();
                                dr["categoryId"] = item.Categoryid;
                                dr["companyId"] = item.SubCategoryId;
                                dr["brandId"] = item.BrandId;
                                IdDt.Rows.Add(dr);
                            }

                            param = new SqlParameter("CatCompanyBrand", IdDt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.CatCompanyBrand";

                            var IcDt = new DataTable();
                            SqlParameter param1 = null;

                            IcDt = new DataTable();
                            IcDt.Columns.Add("stringValue");
                            if (IncentiveClassification.Count > 0)
                            {
                                foreach (var item in IncentiveClassification)
                                {
                                    var dr = IcDt.NewRow();
                                    dr["stringValue"] = item;
                                    IcDt.Rows.Add(dr);
                                }
                            }

                            param1 = new SqlParameter("Classification", IcDt);
                            param1.SqlDbType = SqlDbType.Structured;
                            param1.TypeName = "dbo.stringValues";

                            var cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[GetPromotionalItemforSalesApp]";
                            cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                            cmd.Parameters.Add(param);
                            cmd.Parameters.Add(param1);
                            cmd.Parameters.Add(new SqlParameter("@SearchKeyWord", ""));
                            cmd.Parameters.Add(new SqlParameter("@promotionalNewLaunch", promotionalNewLaunch));
                            cmd.Parameters.Add(new SqlParameter("@promotionalItems", promotionalItem));
                            cmd.Parameters.Add(new SqlParameter("@skip", (promotionalskip * promotionaltake)));
                            cmd.Parameters.Add(new SqlParameter("@take", promotionaltake));
                            cmd.Parameters.Add(new SqlParameter("@orderby", promotionalorderby));
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;

                            // Run the sproc
                            var reader = cmd.ExecuteReader();
                            ItemData = ((IObjectContextAdapter)context)
                            .ObjectContext
                            .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                            reader.NextResult();
                            //if (reader.Read())
                            //{
                            //    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                            //}

                        }
                        else
                        {
                            var ItemNumbersList = new List<string>();
                            if (promotionalNewLaunch)
                            {
                                string query = "Exec GetNewLaunchItemNumbers";
                                var NewLaunchItems = context.Database.SqlQuery<string>(query).ToList();
                                ItemNumbersList.AddRange(NewLaunchItems);
                            }
                            if (promotionalItem || (!promotionalNewLaunch && !promotionalItem))
                            {
                                string query = "Exec GetPromotionalItemNumber " + PeopleId + "," + warehouseId + ",0";
                                var promotionalItems = context.Database.SqlQuery<string>(query).ToList();
                                ItemNumbersList.AddRange(promotionalItems);
                            }
                            ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                            var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesPromotionalCatelogElasticData(warehouseId, PromotionalStoreCategorySubCategoryBrands, ItemNumbersList, "", (promotionalskip * promotionaltake), promotionaltake, promotionalorderby, true, IncentiveClassification));
                            ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                            // itemResponseDc.TotalItem = data.TotalItem;
                        }

                        if (ItemData.Any() && isSuggestShow && suggestedTake > 0 && ItemDataDCs.Count > 0)
                        {
                            int ItemDataCount = ItemData.GroupBy(x => new { x.ItemNumber, x.WarehouseId, x.Rating }).Count();
                            basetake = basetake - (ItemDataCount == promotionaltake ? promotionaltake : ItemDataCount);
                        }
                        else if (ItemData.Any() && ItemData.Count > 0)
                        {
                            int ItemDataCount = ItemData.GroupBy(x => new { x.ItemNumber, x.WarehouseId, x.Rating }).Count();
                            basetake = take - (ItemDataCount == promotionaltake ? promotionaltake : ItemDataCount);
                        }

                        var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                        List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                        if (ItemData != null && ItemData.Any())
                        {
                            ItemMasterManager itemMasterManager = new ItemMasterManager();
                            itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                        }

                        BackendOrderController backendOrderController = new BackendOrderController();
                        foreach (var it in ItemData)
                        {

                            it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                            it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;
                            double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                            it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);

                            //Condition for offer end
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;

                            }

                            if (it.OfferCategory == 1)
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //Customer (0.2 * 10=1)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }

                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }

                            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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
                            it.Itemtype = 3;
                            it.Sequence = promotionalIndex;
                            ItemDataDCs.Add(it);
                        }

                        // itemResponseDc.ItemDataDCs.AddRange(ItemDataDCs);

                    }

                }
                //});
                //taskList.Add(taskpromotionalItem);
            }

            //Task.WaitAll(taskList.ToArray());
            //--- Base Item
            //var taskbaseItem = Task.Factory.StartNew(() =>
            //{
            if ((basetake == basedefaultitem) && ItemDataDCs.Count == 0)
            {
                basetake = take;
            }
            //else
            //{
            //    basetake = basedefaultitem;
            //}
            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any())
            {
                using (var context = new AuthContext())
                {
                    List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                    if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                    {
                        if (context.Database.Connection.State != ConnectionState.Open)

                            context.Database.Connection.Open();
                        var IdDt = new DataTable();
                        SqlParameter param = null;

                        IdDt = new DataTable();
                        IdDt.Columns.Add("categoryId");
                        IdDt.Columns.Add("companyId");
                        IdDt.Columns.Add("brandId");
                        foreach (var item in StoreCategorySubCategoryBrands)
                        {
                            var dr = IdDt.NewRow();
                            dr["categoryId"] = item.Categoryid;
                            dr["companyId"] = item.SubCategoryId;
                            dr["brandId"] = item.BrandId;
                            IdDt.Rows.Add(dr);
                        }

                        param = new SqlParameter("CatCompanyBrand", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.CatCompanyBrand";

                        var IcDt = new DataTable();
                        SqlParameter param1 = null;

                        IcDt = new DataTable();
                        IcDt.Columns.Add("stringValue");
                        if (IncentiveClassification.Count > 0)
                        {
                            foreach (var item in IncentiveClassification)
                            {
                                var dr = IcDt.NewRow();
                                dr["stringValue"] = item;
                                IcDt.Rows.Add(dr);
                            }
                        }

                        param1 = new SqlParameter("Classification", IcDt);
                        param1.SqlDbType = SqlDbType.Structured;
                        param1.TypeName = "dbo.stringValues";

                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetItemforSalesApp]";
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(param1);
                        cmd.Parameters.Add(new SqlParameter("@SearchKeyWord", ""));
                        cmd.Parameters.Add(new SqlParameter("@ScoreFrom", basescorefrom));
                        cmd.Parameters.Add(new SqlParameter("@ScoreTo", basescoreto));
                        cmd.Parameters.Add(new SqlParameter("@skip", (baseskip * basetake)));
                        cmd.Parameters.Add(new SqlParameter("@take", basetake));
                        //cmd.Parameters.Add(new SqlParameter("@Unbilled", basesUnbilled));
                        cmd.Parameters.Add(new SqlParameter("@orderby", baseorderby));

                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        ItemData = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                        reader.NextResult();
                        if (reader.Read())
                        {
                            itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                        }
                    }
                    else
                    {
                        ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                        var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesBaseCatelogElasticData(warehouseId, StoreCategorySubCategoryBrands, null, "", (baseskip * basetake), basetake, baseorderby, true, IncentiveClassification));
                        ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                        itemResponseDc.TotalItem = data.TotalItem;
                    }

                    var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();



                    if (ItemData != null && ItemData.Any())
                    {

                        List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();
                        ItemMasterManager itemMasterManager = new ItemMasterManager();
                        itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                        BackendOrderController backendOrderController = new BackendOrderController();
                        foreach (var it in ItemData)
                        {
                            it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                            it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;
                            double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                            it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);

                            //Condition for offer end
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;

                            }

                            if (it.OfferCategory == 1)
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //Customer (0.2 * 10=1)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }

                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }

                            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                            it.Itemtype = 1;
                            it.Sequence = baseIndex;
                            ItemDataDCs.Add(it);
                        }
                    }
                    //--If Base Item is null then remove all suggested and Promotional Item
                    /*else
                    {
                        ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                    }*/

                }
            }

            //});

            //taskList.Add(taskbaseItem);

            if (ItemDataDCs != null && ItemDataDCs.Any())
            {
                var enddate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                var startDate = DateTime.Now.AddMonths(-9).Date.ToString("yyyy-MM-dd");
                var itemmultiMrpIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();


                string query = $"SELECT  itemmultimrpid,max(createddate) createddate, first(ordqty) Qty from skorderdata_{AppConstants.Environment} where itemmultimrpid in ({ string.Join(",", itemmultiMrpIds) })   and whid={warehouseId} and custid={customerId}  and createddate>='{startDate}' and createddate <= '{enddate}'  group by itemmultimrpid";

                ElasticSqlHelper<DataContracts.External.orderMrpData> elasticSqlHelper = new ElasticSqlHelper<DataContracts.External.orderMrpData>();

                var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).ToList());


                /*var itemmultiMrpIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                var enddate = DateTime.Now;
                var startDate = enddate.AddMonths(-9);
                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted
                                                    && x.CustomerId == customerId && x.WarehouseId == warehouseId
                                                    // && x.orderDetails.Any(y => itemmultiMrpIds.Contains(y.ItemMultiMRPId)
                                                    // && x.CreatedDate >= startDate && x.CreatedDate <= enddate
                                                    );
                var ordercollection = mongoDbHelper.mongoDatabase.GetCollection<MongoOrderMaster>("MongoOrderMaster").AsQueryable();
                var orderdetails = ordercollection.Where(orderPredicate)
                                    .SelectMany(t => t.orderDetails, (t, a) => new
                                    {
                                        CreatedDate = t.CreatedDate,
                                        ItemMultiMRPId = a.ItemMultiMRPId,
                                        Qty = a.qty
                                    }).Where(x => itemmultiMrpIds.Contains(x.ItemMultiMRPId))
                                    .ToList();
    */
                var itemMultiMRPIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                using (var context = new AuthContext())
                {
                    ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, warehouseId, context);
                }
                foreach (var item in ItemDataDCs)
                {
                    if (orderdetails != null && orderdetails.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId))
                    {
                        item.LastOrderDate = orderdetails.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate;
                        item.LastOrderQty = orderdetails.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().Qty;
                        item.LastOrderDays = (DateTime.Today - item.LastOrderDate).Value.Days;
                    }
                    if (item.price > item.UnitPrice)
                    {
                        item.marginPoint = item.UnitPrice > 0 ? (((item.price - item.UnitPrice) * 100) / item.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                        if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.PTR > 0))
                        {
                            var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                            var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                            var UPMRPMargin = item.marginPoint.Value;
                            if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                item.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                        }

                    }
                    else
                    {
                        item.marginPoint = 0;
                    }
                }

                itemResponseDc.ItemDataDCs = ItemDataDCs.GroupBy(x => new { x.ItemNumber, x.Itemtype }).Select(x => new DataContracts.External.SalesAppItemDataDC
                {
                    BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                    BillLimitQty = x.FirstOrDefault().BillLimitQty,
                    Categoryid = x.FirstOrDefault().Categoryid,
                    CompanyId = x.FirstOrDefault().CompanyId,
                    dreamPoint = x.FirstOrDefault().dreamPoint,
                    HindiName = x.FirstOrDefault().HindiName,
                    IsItemLimit = x.FirstOrDefault().IsItemLimit,
                    IsOffer = x.FirstOrDefault().IsOffer,
                    ItemId = x.FirstOrDefault().ItemId,
                    ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                    ItemMultiMRPId = x.FirstOrDefault().ItemMultiMRPId,
                    itemname = x.FirstOrDefault().itemname,
                    ItemNumber = x.FirstOrDefault().ItemNumber,
                    Itemtype = x.FirstOrDefault().Itemtype,
                    LastOrderDate = x.FirstOrDefault().LastOrderDate,
                    LastOrderDays = x.FirstOrDefault().LastOrderDays,
                    LastOrderQty = x.FirstOrDefault().LastOrderQty,
                    LogoUrl = x.FirstOrDefault().LogoUrl,
                    marginPoint = x.FirstOrDefault().marginPoint,
                    MinOrderQty = x.FirstOrDefault().MinOrderQty,
                    OfferCategory = x.FirstOrDefault().OfferCategory,
                    OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                    OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                    OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                    OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                    OfferId = x.FirstOrDefault().OfferId,
                    OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                    OfferType = x.FirstOrDefault().OfferType,
                    OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                    price = x.FirstOrDefault().price,
                    Sequence = x.FirstOrDefault().Sequence,
                    SubCategoryId = x.FirstOrDefault().SubCategoryId,
                    SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                    UnitPrice = x.FirstOrDefault().UnitPrice,
                    WarehouseId = x.FirstOrDefault().WarehouseId,
                    Scheme = x.FirstOrDefault().Scheme,
                    Classification = x.FirstOrDefault().Classification,
                    BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                    moqList = x.Count() > 1 ? x.Select(y => new DataContracts.External.SalesAppItemDataDC
                    {
                        isChecked = (y.ItemMultiMRPId == x.FirstOrDefault().ItemMultiMRPId && y.MinOrderQty == x.FirstOrDefault().MinOrderQty),
                        Scheme = y.Scheme,
                        BaseCategoryId = y.BaseCategoryId,
                        BillLimitQty = y.BillLimitQty,
                        Categoryid = y.Categoryid,
                        CompanyId = y.CompanyId,
                        dreamPoint = y.dreamPoint,
                        HindiName = y.HindiName,
                        IsItemLimit = y.IsItemLimit,
                        IsOffer = y.IsOffer,
                        ItemId = y.ItemId,
                        ItemlimitQty = y.ItemlimitQty,
                        ItemMultiMRPId = y.ItemMultiMRPId,
                        itemname = y.itemname,
                        ItemNumber = y.ItemNumber,
                        Itemtype = y.Itemtype,
                        LastOrderDate = y.LastOrderDate,
                        LastOrderDays = y.LastOrderDays,
                        LastOrderQty = y.LastOrderQty,
                        LogoUrl = y.LogoUrl,
                        marginPoint = y.marginPoint,
                        MinOrderQty = y.MinOrderQty,
                        OfferCategory = y.OfferCategory,
                        OfferFreeItemId = y.OfferFreeItemId,
                        OfferFreeItemImage = y.OfferFreeItemImage,
                        OfferFreeItemName = y.OfferFreeItemName,
                        OfferFreeItemQuantity = y.OfferFreeItemQuantity,
                        OfferId = y.OfferId,
                        OfferMinimumQty = y.OfferMinimumQty,
                        OfferType = y.OfferType,
                        OfferWalletPoint = y.OfferWalletPoint,
                        price = y.price,
                        Sequence = y.Sequence,
                        SubCategoryId = y.SubCategoryId,
                        SubsubCategoryid = y.SubsubCategoryid,
                        UnitPrice = y.UnitPrice,
                        WarehouseId = y.WarehouseId,
                        Classification = x.FirstOrDefault().Classification,
                        BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                    }).ToList() : new List<DataContracts.External.SalesAppItemDataDC>()
                }).OrderBy(x => x.Sequence).ToList();

            }
            return itemResponseDc;
        }


        public async Task<int> GetSuggestedCategoryId(int CategoryId)
        {
            int suggestedCategoryId = 0;
            using (var context = new AuthContext())
            {
                var suggestedCategoryDetail = context.SalesSuggestedCategoryMappings.Where(x => x.CategoryId == CategoryId || x.SuggestedCategoryId == CategoryId).FirstOrDefault();
                if (suggestedCategoryDetail != null && suggestedCategoryDetail.CategoryId > 0 && suggestedCategoryDetail.SuggestedCategoryId > 0)
                {
                    suggestedCategoryId = suggestedCategoryDetail.CategoryId == CategoryId ? suggestedCategoryDetail.SuggestedCategoryId : suggestedCategoryDetail.CategoryId;
                }
            }
            return suggestedCategoryId;
        }
        [Route("getcateglogConfig")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<dynamic> getcateglogConfig()
        {
            using (var context = new AuthContext())
            {
                var categlogconfigs = context.CatelogConfigs.Where(x => x.IsActive).GroupBy(x => new { x.Id, x.ConfigName, x.Sequence })
                    .Select(x => new { x.Key.ConfigName, x.Key.Sequence }).ToList();
                return categlogconfigs;
            }
        }
        [Route("HomePageGetCategories")]
        [AllowAnonymous]
        [HttpGet]
        public OnBaseSalesAppDc GetCategories(int PeopleId, int BaseCategoryId, int warehouseid, string lang)
        {
            OnBaseSalesAppDc ibjtosend = new OnBaseSalesAppDc();
            ibjtosend.Categorys = new List<Category>();
            ibjtosend.SubCategorys = new List<SubCategory>();
            ibjtosend.SubsubCategorys = new List<SubsubCategory>();
            CategorySalesAppDc CategorySalesApp = Categories(PeopleId, BaseCategoryId, warehouseid, lang);

            ibjtosend.Categorys = CategorySalesApp.Categories.Select(x => new Category
            {
                BaseCategoryId = x.BaseCategoryId,
                HindiName = x.HindiName,
                Categoryid = x.Categoryid,
                CategoryName = x.CategoryName,
                CategoryHindiName = x.HindiName,
                LogoUrl = x.LogoUrl,
                CategoryImg = x.CategoryImg
            }).ToList();
            ibjtosend.SubCategorys = CategorySalesApp.SubCategories.Select(x => new SubCategory
            {
                Categoryid = x.Categoryid,
                HindiName = x.HindiName,
                SubCategoryId = x.SubCategoryId,
                SubcategoryName = x.SubcategoryName,
                LogoUrl = x.LogoUrl,
                itemcount = x.itemcount
            }).ToList();
            ibjtosend.SubsubCategorys = CategorySalesApp.SubSubCategories.Select(x => new SubsubCategory
            {
                SubCategoryId = x.SubCategoryId,
                Categoryid = x.Categoryid,
                HindiName = x.HindiName,
                SubsubCategoryid = x.SubSubCategoryId,
                SubsubcategoryName = x.SubSubcategoryName,
                LogoUrl = x.LogoUrl,
                itemcount = x.itemcount
            }).ToList();

            if (ibjtosend.SubsubCategorys != null && ibjtosend.SubsubCategorys.Any())
            {
                using (var db = new AuthContext())
                {
                    //var ActiveCustomer = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.IsKPP }).FirstOrDefault();

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    #region block Barnd

                    var custtype = 4;
                    var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                    if (blockBarnds != null && blockBarnds.Any())
                    {
                        //ibjtosend.SubsubCategorys = ibjtosend.SubsubCategorys.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                        //ibjtosend.SubCategorys = ibjtosend.SubCategorys.Where(x => ibjtosend.SubsubCategorys.Select(y => y.SubCategoryId).Contains(x.SubCategoryId)).ToList();
                        //ibjtosend.Categorys = ibjtosend.Categorys.Where(x => ibjtosend.SubCategorys.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();

                        ibjtosend.SubsubCategorys = ibjtosend.SubsubCategorys.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.SubsubCategoryid))).ToList();
                        ibjtosend.SubCategorys = ibjtosend.SubCategorys.Where(x => ibjtosend.SubsubCategorys.Select(y => y.Categoryid + " " + y.SubCategoryId).Contains(x.Categoryid + " " + x.SubCategoryId)).ToList();
                        ibjtosend.Categorys = ibjtosend.Categorys.Where(x => ibjtosend.SubCategorys.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();
                    }
                    #endregion
                }
            }
            return ibjtosend;
        }

        [Route("GetHighesPOForReviewBasket")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ItemResponseDc> GetHighesPOForReviewBasketAsync(int PeopleId, int CustomerId, int WarehouseId, string lang)
        {
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };

            List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == CustomerId && x.WarehouseId == WarehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (PeopleId > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == PeopleId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            List<int> AddedItemMrpIdLst = new List<int>();
            if (customerShoppingCart != null && customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any())
            {
                AddedItemMrpIdLst = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && x.IsDeleted != true).ToList().Select(x => x.ItemMultiMRPId).ToList();
            }

            var enddate = DateTime.Now;
            var startDate = enddate.AddMonths(-9);
            var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted
                                                && x.CustomerId == CustomerId && x.WarehouseId == WarehouseId);
            /*
            var indexName = "skorderdata_" + AppConstants.Environment;
            var client = ElasticNestHelper<DataContracts.ElasticSearch.skorderdata>.CreateElaticConfig(indexName);
            var filters = new List<Func<QueryContainerDescriptor<DataContracts.ElasticSearch.skorderdata>, QueryContainer>>();

            filters.Add(fq => fq.Term(x => x.Field("whid").Value(WarehouseId)));//---TODO--Remove on prod
            filters.Add(fq => fq.Term(x => x.Field("custid").Value(CustomerId)));
            if (AddedItemMrpIdLst.Any())
                filters.Add(fq => fq.Bool(a => a.MustNot(z => z.Terms(t => t.Field("itemmultimrpid").Terms(AddedItemMrpIdLst)))));
            var orderdatas = (client.Search<DataContracts.ElasticSearch.skorderdata>(x => x.Query(q =>
                       q.Bool(b => b.Filter(filters))))).Documents.OrderByDescending(x => x.createddate).OrderByDescending(x=>x.ordqty).Take(10).ToList();

            */
            var platformIdxName = "skorderdata_" + AppConstants.Environment;

            string query = $" SELECT top 10 orderid from skorderdata_{AppConstants.Environment} where custid={CustomerId} and whid='{WarehouseId}' group by orderid order by orderid desc";

            ElasticSqlHelper<DataContracts.External.LastPOOrderData> elasticSqlHelperData = new ElasticSqlHelper<DataContracts.External.LastPOOrderData>();

            var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(query)).ToList());

            if (orderdetails != null && orderdetails.Any())
            {
                var orderIdList = orderdetails.Select(x => x.OrderId).ToList();
                //List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                //string queryItem = $" SELECT itemnumber from skorderdata_{AppConstants.Environment} where custid={customerId} and whid='{warehouseId}'  and catid in ({ string.Join(",", categoryIds) }) and compid in ({ string.Join(",", companyIds) }) and brandid in ({ string.Join(",", brandIds) }) and orderid in ({ string.Join(",", orderIdList) })  group by itemnumber";

                ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemNumber> elasticSqlHelper = new ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemNumber>();

                var orderdatas = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync($"SELECT itemnumber,itemname,createddate,ordqty,itemmultimrpid from {platformIdxName} where custid='{CustomerId}' and whid='{WarehouseId}' and orderid in ({ string.Join(",", orderIdList) })  order by createddate desc"))).ToList();
                //select sum(dispatchqty * price) as val from {platformIdxName} where status='Post Order Canceled' and updateddate>='{sDate}' and updateddate<'{eDate}' {whereCond.ToString()}")).FirstOrDefault());


                var ItemNumberLists = AddedItemMrpIdLst != null && AddedItemMrpIdLst.Any() ? orderdatas.Where(x => !AddedItemMrpIdLst.Contains(x.itemmultimrpid)).ToList().Select(x => new
                {
                    itemmultimrpid = x.itemmultimrpid,
                    itemnumber = x.itemnumber,
                    itemname = x.itemname,
                    createddate = x.createddate,
                    ordqty = x.ordqty
                }).ToList() : orderdatas.Select(x => new
                {
                    itemmultimrpid = x.itemmultimrpid,
                    itemnumber = x.itemnumber,
                    itemname = x.itemname,
                    createddate = x.createddate,
                    ordqty = x.ordqty
                }).ToList();


                var ItemNumberList = ItemNumberLists.Select(x =>
                            new
                            {
                                x.itemnumber,
                                x.createddate,
                                x.ordqty
                            }
                           ).GroupBy(x => x.itemnumber).Select(x => new
                           {
                               ItemNumber = x.Key,
                               x.OrderByDescending(y => y.createddate).FirstOrDefault().ordqty,
                               x.OrderByDescending(y => y.createddate).FirstOrDefault().createddate
                           }).OrderByDescending(x => x.ordqty).ToList();

                //var ItemNumberList = ItemNumberLists.Select(x => x.itemnumber).ToList();

                /*
                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                var enddate = DateTime.Now;
                var startDate = enddate.AddMonths(-9);
                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted
                                                    && x.CustomerId == CustomerId && x.WarehouseId == WarehouseId);
                // && x.CreatedDate >= startDate && x.CreatedDate <= enddate);
                var ordercollection = mongoDbHelper.mongoDatabase.GetCollection<MongoOrderMaster>("MongoOrderMaster").AsQueryable();
                var itemList = ordercollection.Where(orderPredicate)
                                    .SelectMany(t => t.orderDetails, (t, a) => new
                                    {
                                        CreatedDate = t.CreatedDate,
                                        Qty = a.qty,
                                        ItemAmount = a.TotalAmt,
                                        ItemNumber = a.itemNumber
                                    }).ToList();
                var ItemNumberList = itemList.GroupBy(x => x.ItemNumber)
                                    .Select(y => new
                                    {
                                        ItemNumber = y.Key,
                                        Qty = y.FirstOrDefault().Qty,
                                        CreatedDate = y.FirstOrDefault().CreatedDate,
                                        ItemAmount = y.FirstOrDefault().ItemAmount
                                    }).ToList();
                */
                if (ItemNumberList != null && ItemNumberList.Any())
                {
                    List<string> ItemNumberLst = ItemNumberList.Select(x => x.ItemNumber).Distinct().ToList();
                    //string ItemNumbers = string.Join(",", ItemNumberLst);

                    List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);

                    if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any())
                    {
                        using (var context = new AuthContext())
                        {
                            var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);
                            if (context.Database.Connection.State != ConnectionState.Open)
                                context.Database.Connection.Open();

                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any())
                            {
                                List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                                if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                                {
                                    var IdDt = new DataTable();
                                    SqlParameter param = null;

                                    IdDt = new DataTable();
                                    IdDt.Columns.Add("categoryId");
                                    IdDt.Columns.Add("companyId");
                                    IdDt.Columns.Add("brandId");
                                    foreach (var item in StoreCategorySubCategoryBrands)
                                    {
                                        var dr = IdDt.NewRow();
                                        dr["categoryId"] = item.Categoryid;
                                        dr["companyId"] = item.SubCategoryId;
                                        dr["brandId"] = item.BrandId;
                                        IdDt.Rows.Add(dr);
                                    }

                                    var ItemNumberDT = new DataTable();
                                    ItemNumberDT.Columns.Add("StringValue");
                                    foreach (var item in ItemNumberLst)
                                    {
                                        var dr = ItemNumberDT.NewRow();
                                        dr["StringValue"] = item;
                                        ItemNumberDT.Rows.Add(dr);
                                    }
                                    var paramItemNumber = new SqlParameter("@ItemNumbers", ItemNumberDT);
                                    paramItemNumber.SqlDbType = System.Data.SqlDbType.Structured;
                                    paramItemNumber.TypeName = "dbo.stringValues";

                                    param = new SqlParameter("CatCompanyBrand", IdDt);
                                    param.SqlDbType = SqlDbType.Structured;
                                    param.TypeName = "dbo.CatCompanyBrand";

                                    var cmd = context.Database.Connection.CreateCommand();
                                    cmd.CommandText = "[dbo].[GetHighestPOItemforSalesAppN]";
                                    cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                                    cmd.Parameters.Add(param);
                                    cmd.Parameters.Add(paramItemNumber);


                                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                                    // Run the sproc
                                    var reader = cmd.ExecuteReader();
                                    ItemData = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                                    reader.NextResult();
                                    if (reader.Read())
                                    {
                                        itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                                    }
                                }
                                else
                                {
                                    ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                                    var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesCatelogElasticData(WarehouseId, StoreCategorySubCategoryBrands, ItemNumberLst, "", -1, -1, 0, 1000, "DESC", true, null));
                                    ItemData = Mapper.Map(data.ItemMasters.ToList()).ToANew<List<DataContracts.External.ItemDataDC>>();//OrderByDescending(y => y.LastOrderQty)
                                    itemResponseDc.TotalItem = data.TotalItem;
                                }

                                var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();
                                BackendOrderController backendOrderController = new BackendOrderController();
                                foreach (var it in ItemData)
                                {
                                    double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                                    it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);
                                    //Condition for offer end
                                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                                    {
                                        if (it.OfferCategory == 2)
                                        {
                                            it.IsOffer = false;
                                            it.FlashDealSpecialPrice = 0;
                                            it.OfferCategory = 0;
                                        }
                                        else if (it.OfferCategory == 1)
                                        {
                                            it.IsOffer = false;
                                            it.OfferCategory = 0;
                                        }

                                    }
                                    else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;

                                    }

                                    if (it.OfferCategory == 1)
                                    {
                                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                            it.IsOffer = true;
                                        else
                                            it.IsOffer = false;
                                    }

                                    try
                                    {
                                        if (!it.IsOffer)
                                        {
                                            /// Dream Point Logic && Margin Point
                                            int? MP, PP;
                                            double xPoint = xPointValue * 10;
                                            //Customer (0.2 * 10=1)
                                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                            {
                                                PP = 0;
                                            }
                                            else
                                            {
                                                PP = it.promoPerItems;
                                            }
                                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                            {
                                                MP = 0;
                                            }
                                            else
                                            {
                                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                            }
                                            if (PP > 0 && MP > 0)
                                            {
                                                int? PP_MP = PP + MP;
                                                it.dreamPoint = PP_MP;
                                            }
                                            else if (MP > 0)
                                            {
                                                it.dreamPoint = MP;
                                            }
                                            else if (PP > 0)
                                            {
                                                it.dreamPoint = PP;
                                            }
                                            else
                                            {
                                                it.dreamPoint = 0;
                                            }
                                        }
                                        else { it.dreamPoint = 0; }

                                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                        if (it.price > it.UnitPrice)
                                        {
                                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                        }
                                        else
                                        {
                                            it.marginPoint = 0;
                                        }
                                    }
                                    catch { }

                                    if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                                    it.Itemtype = 1;
                                    ItemDataDCs.Add(it);
                                }

                            }
                        }
                    }
                }
                var itemMultiMRPIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                using (var context = new AuthContext())
                {
                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, WarehouseId, context);
                }

                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (ItemDataDCs != null && ItemDataDCs.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(WarehouseId, ItemDataDCs.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                }



                foreach (var item in ItemDataDCs)
                {
                    item.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == item.ItemMultiMRPId)?.Classification;
                    item.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == item.ItemMultiMRPId)?.BackgroundRgbColor;


                    if (ItemNumberList != null && ItemNumberList.Any(x => x.ItemNumber == item.ItemNumber))
                    {
                        item.LastOrderDate = ItemNumberList.Where(x => x.ItemNumber == item.ItemNumber).OrderByDescending(x => x.createddate).FirstOrDefault().createddate;
                        item.LastOrderQty = ItemNumberList.Where(x => x.ItemNumber == item.ItemNumber).OrderByDescending(x => x.createddate).FirstOrDefault().ordqty;
                        item.LastOrderDays = (DateTime.Today - item.LastOrderDate).Value.Days;
                    }

                    if (item.price > item.UnitPrice)
                    {
                        item.marginPoint = item.UnitPrice > 0 ? (((item.price - item.UnitPrice) * 100) / item.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                        if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.PTR > 0))
                        {
                            var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                            var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                            var UPMRPMargin = item.marginPoint.Value;
                            if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                item.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                        }

                    }
                    else
                    {
                        item.marginPoint = 0;
                    }
                }
            }
            itemResponseDc.ItemDataDCs = ItemDataDCs.OrderByDescending(y => y.PurchaseValue).ToList();
            return itemResponseDc;
        }

        [Route("GetSalesAppItemNotification")]
        [HttpGet]
        public async Task<ItemResponseDc> GetNewLaunchNotificationAysnc(int PeopleId, int WarehouseId, string ItemType, int Skip, int Take, string Lang)
        {
            List<SalesItemForNotificationDC> salesItemForNotificationList = new List<SalesItemForNotificationDC>();
            List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
            // var itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };

            #region block Barnd
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            var custtype = 4;
            var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
            if (blockBarnds != null && blockBarnds.Any())
            {
                StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
            }
            #endregion

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesItemNotification]";
                cmd.Parameters.Add(new SqlParameter("@PeopleId", PeopleId));
                cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                cmd.Parameters.Add(new SqlParameter("@ItemType", ItemType));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                salesItemForNotificationList = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<SalesItemForNotificationDC>(reader).ToList();
                reader.NextResult();

                List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                if ((StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any()) && salesItemForNotificationList != null && salesItemForNotificationList.Any())
                {
                    if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                    {
                        var CatSubCatTable = new DataTable();
                        SqlParameter paramCatSubCat = null;
                        CatSubCatTable = new DataTable();
                        CatSubCatTable.Columns.Add("categoryId");
                        CatSubCatTable.Columns.Add("companyId");
                        CatSubCatTable.Columns.Add("brandId");
                        foreach (var item in StoreCategorySubCategoryBrands)
                        {
                            var dr = CatSubCatTable.NewRow();
                            dr["categoryId"] = item.Categoryid;
                            dr["companyId"] = item.SubCategoryId;
                            dr["brandId"] = item.BrandId;
                            CatSubCatTable.Rows.Add(dr);
                        }

                        paramCatSubCat = new SqlParameter("CatCompanyBrand", CatSubCatTable);
                        paramCatSubCat.SqlDbType = SqlDbType.Structured;
                        paramCatSubCat.TypeName = "dbo.CatCompanyBrand";

                        var ItemMultiMrpIdsTable = new DataTable();
                        ItemMultiMrpIdsTable = new DataTable();
                        SqlParameter paramItemMultiMrpIds = null;
                        ItemMultiMrpIdsTable.Columns.Add("IntValue");
                        foreach (var item in salesItemForNotificationList)
                        {
                            var drItemMultiMrp = ItemMultiMrpIdsTable.NewRow();
                            drItemMultiMrp[0] = item.ItemMultiMRPId;
                            ItemMultiMrpIdsTable.Rows.Add(drItemMultiMrp);

                        }

                        paramItemMultiMrpIds = new SqlParameter("@ItemMultiMrpIds", ItemMultiMrpIdsTable);
                        paramItemMultiMrpIds.SqlDbType = SqlDbType.Structured;
                        paramItemMultiMrpIds.TypeName = "dbo.Intvalues";

                        var cmdItem = context.Database.Connection.CreateCommand();
                        cmdItem.CommandText = "[dbo].[GetItemForPriceNotifications]";
                        cmdItem.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                        cmdItem.Parameters.Add(paramCatSubCat);
                        cmdItem.Parameters.Add(paramItemMultiMrpIds);
                        cmdItem.Parameters.Add(new SqlParameter("@skip", (Skip * Take)));
                        cmdItem.Parameters.Add(new SqlParameter("@take", Take));


                        cmdItem.CommandType = System.Data.CommandType.StoredProcedure;

                        // Run the sproc
                        var readerItem = cmdItem.ExecuteReader();
                        ItemData = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<DataContracts.External.ItemDataDC>(readerItem).ToList();
                        readerItem.NextResult();
                        if (readerItem.Read())
                        {
                            itemResponseDc.TotalItem = Convert.ToInt32(readerItem["itemCount"]);
                        }

                    }
                    else
                    {
                        ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                        var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesCatelogElasticData(WarehouseId, StoreCategorySubCategoryBrands, null, "", -1, -1, (Skip * Take), Take, "DESC", true, null));
                        ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                        itemResponseDc.TotalItem = data.TotalItem;
                    }
                }

                if (ItemData != null && ItemData.Any())
                {
                    foreach (var it in ItemData)
                    {
                        //Condition for offer end
                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                            else if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }
                        else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                        }



                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }
                            }
                            else { it.dreamPoint = 0; }

                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }

                        if (it.HindiName != null && !string.IsNullOrEmpty(Lang) && Lang == "hi")
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

                        it.Itemtype = 1;

                        ItemDataDCs.Add(it);
                    }
                }
                //--If Base Item is null then remove all suggested and Promotional Item
                else
                {
                    ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                }
            }
            itemResponseDc.ItemDataDCs = ItemDataDCs;
            return itemResponseDc;
        }



        //Get Published Code
        [Route("GetPublishedSection")]
        [HttpGet]
        public IEnumerable<AppHomeSections> GetPublishedSectionSalesApp(string appType, int WarehouseId, string lang, int PeopleId)
        {
            using (var context = new AuthContext())
            {
                CategorySalesAppDc CategorySalesApp = Categories(PeopleId, 0, WarehouseId, lang);
                List<AppHomeSections> sections = new List<AppHomeSections>();

                var datenow = indianTime;

                Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                var publishedData = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.APPHomeCacheKey(appType.Replace(" ", ""), WarehouseId.ToString()), () => GetSalesAppPublisheddata(appType, WarehouseId));

                //var publishedData = GetSalesAppPublisheddata(appType, WarehouseId);

                if (publishedData == null)
                {
                }
                else
                {
                    sections = JsonConvert.DeserializeObject<List<AppHomeSections>>(publishedData.ApphomeSection);
                    foreach (var a in sections)
                    {
                        if (a.SectionType == "Banner" || a.SectionSubType == "Flash Deal")
                        {
                            foreach (var ap in a.AppItemsList)
                            {
                                //if (a.SectionSubType == "Base Category")
                                //{
                                //    ap.BaseCategoryId = ap.RedirectionID;
                                //}
                                //else if (a.SectionSubType == "Category")
                                //{
                                //    ap.CategoryId = ap.RedirectionID;
                                //}
                                //else if (a.SectionSubType == "Sub Category")
                                //{
                                //    ap.SubCategoryId = ap.RedirectionID;
                                //}
                                //else if (a.SectionSubType == "Brand")
                                //{
                                //    ap.SubsubCategoryId = ap.RedirectionID;
                                //}

                                if (ap.OfferEndTime < datenow)
                                {
                                    ap.Expired = true;
                                    ap.Deleted = true;
                                    ap.Active = false;
                                }
                            }
                            if (a.AppItemsList.All(x => !x.Active && x.Deleted))
                            {
                                a.Deleted = true;
                                a.Active = false;
                            }
                        }
                    }
                    sections = sections.ToList().Select(o => new AppHomeSections
                    {
                        AppItemsList = o.Deleted == false && (o.SectionType == "Banner" || o.SectionType == "PopUp" || o.SectionSubType == "Flash Deal") ? o.AppItemsList.Where(i => i.Deleted == false && (i.OfferEndTime > datenow || i.OfferEndTime == null) && (i.OfferStartTime < datenow || i.OfferStartTime == null)).ToList() : o.AppItemsList.Where(x => x.Deleted == false).ToList(),
                        SectionID = o.SectionID,
                        AppType = o.AppType,
                        WarehouseID = o.WarehouseID,
                        SectionName = o.SectionName,
                        SectionHindiName = o.SectionHindiName,
                        SectionType = o.SectionType,
                        SectionSubType = o.SectionSubType,
                        CreatedDate = o.CreatedDate,
                        UpdatedDate = indianTime,
                        IsTile = o.IsTile,
                        IsBanner = o.IsBanner,
                        IsPopUp = o.IsPopUp,
                        Sequence = o.Sequence,
                        RowCount = o.RowCount,
                        ColumnCount = o.ColumnCount,
                        HasBackgroundColor = o.HasBackgroundColor,
                        TileBackgroundColor = o.TileBackgroundColor,
                        BannerBackgroundColor = o.BannerBackgroundColor,
                        HasHeaderBackgroundColor = o.HasHeaderBackgroundColor,
                        TileHeaderBackgroundColor = o.TileHeaderBackgroundColor,
                        HasBackgroundImage = o.HasBackgroundImage,
                        TileBackgroundImage = o.TileBackgroundImage,
                        HasHeaderBackgroundImage = o.HasHeaderBackgroundImage,
                        TileHeaderBackgroundImage = o.TileHeaderBackgroundImage,
                        IsTileSlider = o.IsTileSlider,
                        TileAreaHeaderBackgroundImage = o.TileAreaHeaderBackgroundImage,
                        HeaderTextColor = o.HeaderTextColor,
                        sectionBackgroundImage = o.sectionBackgroundImage,
                        Deleted = o.Deleted,
                        ViewType = o.ViewType,
                        WebViewUrl = o.WebViewUrl
                    }).ToList();
                }

                #region block Barnd
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                #endregion

                if (!string.IsNullOrEmpty(lang) && lang.ToLower() == "hi")
                {
                    sections.ForEach(x =>
                    {
                        string SectionName = !string.IsNullOrEmpty(x.SectionHindiName) ? x.SectionHindiName : x.SectionName;
                        x.SectionName = SectionName;
                        x.AppItemsList.ForEach(y =>
                        {
                            if (x.SectionSubType == "Base Category")
                            {
                                var basecat = CategorySalesApp.Basecats.Any(s => s.BaseCategoryId == y.BaseCategoryId) ? CategorySalesApp.Basecats.FirstOrDefault(s => s.BaseCategoryId == y.BaseCategoryId) : null;
                                y.Active = CategorySalesApp.Basecats.Any(c => c.BaseCategoryId == y.BaseCategoryId);
                                if (basecat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(basecat.HindiName) ? basecat.HindiName : basecat.BaseCategoryName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Category")
                            {
                                var catdata = CategorySalesApp.Categories.Any(s => s.Categoryid == y.CategoryId) ? CategorySalesApp.Categories.FirstOrDefault(s => s.Categoryid == y.CategoryId) : null;
                                y.Active = CategorySalesApp.Categories.Any(c => c.Categoryid == y.CategoryId);
                                if (catdata != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(catdata.HindiName) ? catdata.HindiName : catdata.CategoryName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Sub Category")
                            {
                                var subcat = CategorySalesApp.SubCategories.Any(s => s.SubCategoryId == y.SubCategoryId) ? CategorySalesApp.SubCategories.FirstOrDefault(s => s.SubCategoryId == y.SubCategoryId) : null;
                                y.Active = CategorySalesApp.SubCategories.Any(c => c.SubCategoryId == y.SubCategoryId);
                                if (subcat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(subcat.HindiName) ? subcat.HindiName : subcat.SubcategoryName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Brand")
                            {
                                var subsubcat = CategorySalesApp.SubSubCategories.Any(s => s.SubSubCategoryId == y.SubsubCategoryId) ? CategorySalesApp.SubSubCategories.FirstOrDefault(s => s.SubSubCategoryId == y.SubsubCategoryId) : null;
                                y.Active = CategorySalesApp.SubSubCategories.Any(c => c.SubSubCategoryId == y.SubsubCategoryId);
                                if (subsubcat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(subsubcat.HindiName) ? subsubcat.HindiName : subsubcat.SubSubcategoryName;
                                    y.TileName = tileName;
                                }
                            }

                            if (!string.IsNullOrEmpty(y.BannerImage) && !y.BannerImage.Contains("http"))
                            {
                                y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.BannerImage);
                            }
                            if (!string.IsNullOrEmpty(y.TileImage) && !y.TileImage.Contains("http"))
                            {
                                y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileImage);
                            }
                            if (!string.IsNullOrEmpty(y.TileSectionBackgroundImage) && !y.TileSectionBackgroundImage.Contains("http"))
                            {
                                y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileSectionBackgroundImage);
                            }
                            if (!string.IsNullOrEmpty(x.TileHeaderBackgroundImage) && !x.TileHeaderBackgroundImage.Contains("http"))
                            {
                                x.TileHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.TileHeaderBackgroundImage);
                            }
                        });

                        if (x.SectionSubType == "Brand")
                        {
                            if (blockBarnds != null && blockBarnds.Any())
                            {
                                x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                            }
                        }
                        else if (x.SectionSubType == "Item")
                        {
                            if (blockBarnds != null && blockBarnds.Any())
                            {
                                x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.CatId).Contains(t.CategoryId) && blockBarnds.Select(z => z.SubCatId).Contains(t.SubCategoryId) && blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                            }
                        }

                        x.AppItemsList = x.AppItemsList.Where(y => y.Active).ToList();
                        x.Active = x.SectionSubType != "Other" ? x.AppItemsList.Any() : true;
                    });
                }
                else
                {
                    sections.ForEach(x =>
                    {
                        string SectionName = !string.IsNullOrEmpty(x.SectionHindiName) ? x.SectionHindiName : x.SectionName;
                        x.SectionName = SectionName;
                        x.AppItemsList.ForEach(y =>
                        {
                            if (x.SectionSubType == "Base Category")
                            {
                                var basecat = CategorySalesApp.Basecats.Any(s => s.BaseCategoryId == y.BaseCategoryId) ? CategorySalesApp.Basecats.FirstOrDefault(s => s.BaseCategoryId == y.BaseCategoryId) : null;
                                y.Active = CategorySalesApp.Basecats.Any(c => c.BaseCategoryId == y.BaseCategoryId);
                                if (basecat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(basecat.BaseCategoryName) ? basecat.BaseCategoryName : basecat.HindiName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Category")
                            {
                                var catdata = CategorySalesApp.Categories.Any(s => s.Categoryid == y.CategoryId) ? CategorySalesApp.Categories.FirstOrDefault(s => s.Categoryid == y.CategoryId) : null;
                                y.Active = CategorySalesApp.Categories.Any(c => c.Categoryid == y.CategoryId);
                                if (catdata != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(catdata.CategoryName) ? catdata.CategoryName : catdata.HindiName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Sub Category")
                            {
                                var subcat = CategorySalesApp.SubCategories.Any(s => s.SubCategoryId == y.SubCategoryId) ? CategorySalesApp.SubCategories.FirstOrDefault(s => s.SubCategoryId == y.SubCategoryId) : null;
                                y.Active = CategorySalesApp.SubCategories.Any(c => c.SubCategoryId == y.SubCategoryId);
                                if (subcat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(subcat.SubcategoryName) ? subcat.SubcategoryName : subcat.HindiName;
                                    y.TileName = tileName;
                                }
                            }
                            else if (x.SectionSubType == "Brand")
                            {
                                var subsubcat = CategorySalesApp.SubSubCategories.Any(s => s.SubSubCategoryId == y.SubsubCategoryId) ? CategorySalesApp.SubSubCategories.FirstOrDefault(s => s.SubSubCategoryId == y.SubsubCategoryId) : null;
                                y.Active = CategorySalesApp.SubSubCategories.Any(c => c.SubSubCategoryId == y.SubsubCategoryId);
                                if (subsubcat != null)
                                {
                                    string tileName = !string.IsNullOrEmpty(subsubcat.SubSubcategoryName) ? subsubcat.SubSubcategoryName : subsubcat.HindiName;
                                    y.TileName = tileName;
                                }
                            }

                            if (x.SectionType == "Banner")
                            {
                                if (y.RedirectionType == "Base Category")
                                {
                                    var basecat = CategorySalesApp.Basecats.Any(s => s.BaseCategoryId == y.BaseCategoryId) ? CategorySalesApp.Basecats.FirstOrDefault(s => s.BaseCategoryId == y.BaseCategoryId) : null;
                                    y.Active = CategorySalesApp.Basecats.Any(c => c.BaseCategoryId == y.BaseCategoryId);
                                }
                                else if (y.RedirectionType == "Category")
                                {
                                    var catdata = CategorySalesApp.Categories.Any(s => s.Categoryid == y.CategoryId) ? CategorySalesApp.Categories.FirstOrDefault(s => s.Categoryid == y.CategoryId) : null;
                                    y.Active = CategorySalesApp.Categories.Any(c => c.Categoryid == y.CategoryId);
                                }
                                else if (y.RedirectionType == "Sub Category")
                                {
                                    var subcat = CategorySalesApp.SubCategories.Any(s => s.SubCategoryId == y.SubCategoryId) ? CategorySalesApp.SubCategories.FirstOrDefault(s => s.SubCategoryId == y.SubCategoryId) : null;
                                    y.Active = CategorySalesApp.SubCategories.Any(c => c.SubCategoryId == y.SubCategoryId);
                                }
                                else if (y.RedirectionType == "Brand")
                                {
                                    var subsubcat = CategorySalesApp.SubSubCategories.Any(s => s.SubSubCategoryId == y.SubsubCategoryId) ? CategorySalesApp.SubSubCategories.FirstOrDefault(s => s.SubSubCategoryId == y.SubsubCategoryId) : null;
                                    y.Active = CategorySalesApp.SubSubCategories.Any(c => c.SubSubCategoryId == y.SubsubCategoryId);
                                }
                            }
                            if (!string.IsNullOrEmpty(y.BannerImage) && !y.BannerImage.Contains("http"))
                            {
                                y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.BannerImage);
                            }
                            if (!string.IsNullOrEmpty(y.TileImage) && !y.TileImage.Contains("http"))
                            {
                                y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileImage);
                            }
                            if (!string.IsNullOrEmpty(y.TileSectionBackgroundImage) && !y.TileSectionBackgroundImage.Contains("http"))
                            {
                                y.BannerImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileSectionBackgroundImage);
                            }
                            if (!string.IsNullOrEmpty(x.TileHeaderBackgroundImage) && !x.TileHeaderBackgroundImage.Contains("http"))
                            {
                                x.TileHeaderBackgroundImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , x.TileHeaderBackgroundImage);
                            }
                        });
                        if (x.SectionSubType == "Brand")
                        {
                            if (blockBarnds != null && blockBarnds.Any())
                            {
                                x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                            }
                        }
                        else if (x.SectionSubType == "Item")
                        {
                            if (blockBarnds != null && blockBarnds.Any())
                            {
                                x.AppItemsList = x.AppItemsList.Where(t => !(blockBarnds.Select(z => z.CatId).Contains(t.CategoryId) && blockBarnds.Select(z => z.SubCatId).Contains(t.SubCategoryId) && blockBarnds.Select(z => z.SubSubCatId).Contains(t.SubsubCategoryId))).ToList();
                            }
                        }
                        x.AppItemsList = x.AppItemsList.Where(y => y.Active).ToList();
                        x.Active = x.SectionSubType != "Other" && x.SectionSubType != "DynamicHtml" ? x.AppItemsList.Any() : true;
                    });

                }
                return sections.Where(x => x.Active).OrderBy(x => x.Sequence).ToList();

            }
        }

        public PublishAppHome GetSalesAppPublisheddata(string appType, int wId)
        {
            using (var context = new AuthContext())
            {
                var publishedData = context.PublishAppHomeDB.Where(x => x.WarehouseID == wId && x.AppType == appType && x.Deleted == false).FirstOrDefault();
                return publishedData;
            }
        }

        public CategorySalesAppDc Categories(int PeopleId, int baseCatId, int warehouseid, string lang)
        {
            //List<Category> categories = new List<Category>();
            //List<SubCategories> subcategories = new List<SubCategories>();
            //List<SubSubCategories> subsubcategories = new List<SubSubCategories>();
            CategorySalesAppDc ibjtosend = new CategorySalesAppDc();
            ibjtosend.Basecats = new List<BaseCategory>();
            ibjtosend.Categories = new List<Category>();
            ibjtosend.SubCategories = new List<SubCategories>();
            ibjtosend.SubSubCategories = new List<SubSubCategories>();
            DataContracts.KPPApp.customeritem ds = new DataContracts.KPPApp.customeritem();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
            List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
            List<string> SubCats = StoreCategorySubCategoryBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId).Distinct().ToList();
            List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
            List<string> strCondition = StoreCategorySubCategoryBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.BrandId).Distinct().ToList();
            using (var unitOfWork = new UnitOfWork())
            {
                DataContracts.KPPApp.customeritem CatSubCatBrands = unitOfWork.KPPAppRepository.GetSalesCatSubCat(warehouseid, baseCatId);
                CatSubCatBrands.SubSubCategories = CatSubCatBrands.SubSubCategories.Where(x => strCondition.Contains(x.Categoryid + "-" + x.SubCategoryId + "-" + x.SubSubCategoryId)).ToList();
                SubCats = CatSubCatBrands.SubSubCategories.Select(x => x.Categoryid + "-" + x.SubCategoryId).Distinct().ToList();
                CatSubCatBrands.SubCategories = CatSubCatBrands.SubCategories.Where(x => SubCats.Contains(x.Categoryid + "-" + x.SubCategoryId)).ToList();

                CatIds = CatSubCatBrands.SubCategories.Select(x => x.Categoryid).ToList();
                CatSubCatBrands.Categories = CatSubCatBrands.Categories.Where(x => CatIds.Contains(x.Categoryid)).ToList();
                List<int> BaseCatIds = CatSubCatBrands.Categories.Select(x => x.BaseCategoryId).ToList();
                CatSubCatBrands.Basecats = CatSubCatBrands.Basecats.Where(x => BaseCatIds.Contains(x.BaseCategoryId)).ToList();


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
                ibjtosend.Basecats = CatSubCatBrands.Basecats.Select(x => new BaseCategory { BaseCategoryId = x.BaseCategoryId, BaseCategoryName = x.BaseCategoryName, HindiName = x.HindiName, LogoUrl = x.LogoUrl }).ToList();
                ibjtosend.Categories = CatSubCatBrands.Categories.Select(x => new Category { BaseCategoryId = x.BaseCategoryId, HindiName = x.HindiName, Categoryid = x.Categoryid, CategoryName = x.CategoryName, CategoryHindiName = x.HindiName, LogoUrl = x.LogoUrl, CategoryImg = x.CategoryImg }).ToList();
                ibjtosend.SubCategories = CatSubCatBrands.SubCategories.Select(x => new SubCategories { Categoryid = x.Categoryid, HindiName = x.HindiName, SubCategoryId = x.SubCategoryId, SubcategoryName = x.SubcategoryName, LogoUrl = x.LogoUrl, itemcount = x.itemcount }).ToList();
                ibjtosend.SubSubCategories = CatSubCatBrands.SubSubCategories.Select(x => new SubSubCategories { SubCategoryId = x.SubCategoryId, Categoryid = x.Categoryid, HindiName = x.HindiName, SubSubCategoryId = x.SubSubCategoryId, SubSubcategoryName = x.SubSubcategoryName, LogoUrl = x.LogoUrl, itemcount = x.itemcount }).ToList();

                if (ibjtosend.SubSubCategories != null && ibjtosend.SubSubCategories.Any())
                {
                    using (var db = new AuthContext())
                    {
                        //var ActiveCustomer = db.Customers.Where(x => x.CustomerId == customerId).Select(x => new { x.IsKPP }).FirstOrDefault();

                        RetailerAppManager retailerAppManager = new RetailerAppManager();
                        #region block Barnd

                        var custtype = 4;
                        var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                        if (blockBarnds != null && blockBarnds.Any())
                        {
                            //ibjtosend.SubSubCategories = ibjtosend.SubSubCategories.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubSubCategoryId))).ToList();
                            //ibjtosend.SubCategories = ibjtosend.SubCategories.Where(x => ibjtosend.SubSubCategories.Select(y => y.SubCategoryId).Contains(x.SubCategoryId)).ToList();
                            //ibjtosend.Categories = ibjtosend.Categories.Where(x => ibjtosend.SubCategories.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();

                            ibjtosend.SubSubCategories = ibjtosend.SubSubCategories.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.SubSubCategoryId))).ToList();
                            ibjtosend.SubCategories = ibjtosend.SubCategories.Where(x => ibjtosend.SubSubCategories.Select(y => y.Categoryid + " " + y.SubCategoryId).Contains(x.Categoryid + " " + x.SubCategoryId)).ToList();
                            ibjtosend.Categories = ibjtosend.Categories.Where(x => ibjtosend.SubCategories.Select(y => y.Categoryid).Contains(x.Categoryid)).ToList();
                        }
                        #endregion
                    }
                }
            }
            return ibjtosend;
        }
        public List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> GetCatSubCatwithStores(int peopleid)
        {
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> results = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            using (var context = new AuthContext())
            {
                var query = string.Format("exec IsSalesAppLead {0}", peopleid);
                var isSalesLead = context.Database.SqlQuery<int>(query).FirstOrDefault();
                List<long> storeids = new List<long>();
                if (isSalesLead > 0)
                    storeids = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.Id).ToList();
                else
                {
                    storeids = context.ClusterStoreExecutives.Where(x => x.ExecutiveId == peopleid && x.IsDeleted == false && x.IsActive).Select(x => x.StoreId).Distinct().ToList();

                    if (context.StoreDB.Any(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.IsUniversal))
                        storeids.AddRange(context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.IsUniversal).Select(x => x.Id).ToList());

                    storeids = storeids.Distinct().ToList();
                }


                RetailerAppManager retailerAppManager = new RetailerAppManager();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();

                results = StoreCategorySubCategoryBrands.Where(x => storeids.Contains(x.StoreId)).ToList();
            }
            return results;
        }

        public List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> GetCatSubCatwithStoresNewSales(int peopleid)
        {
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> results = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            using (var context = new AuthContext())
            {
                var query = string.Format("exec IsSalesAppLead {0}", peopleid);
                var isSalesLead = context.Database.SqlQuery<int>(query).FirstOrDefault();
                List<long> storeids = new List<long>();
                if (isSalesLead > 0)
                    storeids = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.Id).ToList();
                else
                {
                    storeids = context.ClusterStoreExecutives.Where(x => x.ExecutiveId == peopleid && x.IsDeleted == false && x.IsActive).Select(x => x.StoreId).Distinct().ToList();

                    if (context.StoreDB.Any(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.IsUniversal))
                        storeids.AddRange(context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.IsUniversal).Select(x => x.Id).ToList());

                    storeids = storeids.Distinct().ToList();
                }


                RetailerAppManager retailerAppManager = new RetailerAppManager();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetailNew();

                results = StoreCategorySubCategoryBrands.Where(x => storeids.Contains(x.StoreId)).ToList();
            }
            return results;
        }

        private async Task<List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>> GetCatSubCatwithStoresNew(int peopleid)
        {
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> results = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            using (var context = new AuthContext())
            {
                var subCategoryQuery = "Exec GetCatSubCatwithStores " + peopleid;
                results = await context.Database.SqlQuery<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>(subCategoryQuery).ToListAsync();

            }
            return results;
        }

        [Route("GetBestPriceSubCategory")]
        [HttpGet]
        public async Task<List<SubCategories>> GetBestPriceSubCategory(int PeopleId, int warehouseId, string lang)
        {
            List<SubCategories> lstSubCategories = new List<SubCategories>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
            using (var db = new AuthContext())
            {
                var subCategoryQuery = "Exec GetBestPriceSubCategory " + warehouseId + ",0,20";
                lstSubCategories = await db.Database.SqlQuery<SubCategories>(subCategoryQuery).ToListAsync();

                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                if (lstSubCategories != null)
                {
                    lstSubCategories = lstSubCategories.Where(x => SubCats.Contains(x.SubCategoryId)).ToList();
                    lstSubCategories.ForEach(x =>
                     {
                         if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi" && !string.IsNullOrEmpty(x.HindiName))
                         {
                             x.SubcategoryName = x.HindiName;
                         }
                     });
                }

            }
            return lstSubCategories;
        }


        [Route("GetBestSellingSubCategory")]
        [HttpGet]
        public async Task<List<BestSellingSubCategory>> GetBestSellingSubCategory(int PeopleId, int warehouseId, string lang)
        {
            List<BestSellingSubCategory> bestSellingSubCategories = new List<BestSellingSubCategory>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);

            using (var db = new AuthContext())
            {
                var subCategoryQuery = "Exec GetSalesAppDeshboardSubCategory " + warehouseId + "," + PeopleId + ",0,20";
                bestSellingSubCategories = await db.Database.SqlQuery<BestSellingSubCategory>(subCategoryQuery).ToListAsync();

                List<string> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryName).Distinct().ToList();
                if (bestSellingSubCategories != null)
                {
                    bestSellingSubCategories = bestSellingSubCategories.Where(x => SubCats.Contains(x.SubCategoryName)).ToList();
                    bestSellingSubCategories.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi" && !string.IsNullOrEmpty(x.HindiName))
                        {
                            x.SubCategoryName = x.HindiName;
                        }
                    });
                }
            }
            return bestSellingSubCategories;
        }

        [Route("GetBestPriceSubCategoryItem")]
        [HttpGet]
        public async Task<List<factoryItemdata>> GetBestPriceSubCategoryItem(int PeopleId, int warehouseId, int subCategoryId, int skip, int take, string lang)
        {
            List<factoryItemdata> newdata = new List<factoryItemdata>();
            using (var db = new AuthContext())
            {
                var subCategoryQuery = "Exec GetBestPriceSubCategoryItem " + warehouseId + "," + subCategoryId + "," + skip + "," + take;
                newdata = await db.Database.SqlQuery<factoryItemdata>(subCategoryQuery).ToListAsync();
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                foreach (var it in newdata)
                {
                    it.dreamPoint = it.dreamPoint.HasValue ? it.dreamPoint : 0;
                    it.marginPoint = it.marginPoint.HasValue ? it.marginPoint : 0;
                    if (!it.IsOffer)
                    {
                        /// Dream Point Logic && Margin Point
                        int? MP, PP;
                        double xPoint = xPointValue * 10;
                        //salesman 0.2=(0.02 * 10=0.2)
                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                        {
                            PP = 0;
                        }
                        else
                        {
                            PP = it.promoPerItems;
                        }
                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                        {
                            MP = 0;
                        }
                        else
                        {
                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                        }
                        if (PP > 0 && MP > 0)
                        {
                            int? PP_MP = PP + MP;
                            it.dreamPoint = PP_MP;
                        }
                        else if (MP > 0)
                        {
                            it.dreamPoint = MP;
                        }
                        else if (PP > 0)
                        {
                            it.dreamPoint = PP;
                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }

                    if (lang.Trim() == "hi")
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

            }
            return newdata;
        }

        [Route("GetCustomerTopBrand")]
        [HttpGet]
        public async Task<List<SubSubCategories>> GetCustomerTopBrand(int PeopleId, int warehouseId, int customerId, string lang)
        {
            List<SubSubCategories> lstSubCategories = new List<SubSubCategories>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);

            using (var db = new AuthContext())
            {
                var subCategoryQuery = "Exec GetCustomerTopBrand " + warehouseId + "," + customerId + ",0,20";
                lstSubCategories = await db.Database.SqlQuery<SubSubCategories>(subCategoryQuery).ToListAsync();

                List<string> strCondition = StoreCategorySubCategoryBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.BrandId).Distinct().ToList();
                if (lstSubCategories != null)
                {
                    lstSubCategories = lstSubCategories.Where(x => strCondition.Contains(x.Categoryid + "-" + x.SubCategoryId + "-" + x.SubSubCategoryId)).ToList();
                    lstSubCategories.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi" && !string.IsNullOrEmpty(x.HindiName))
                        {
                            x.SubSubcategoryName = x.HindiName;
                        }
                    });
                }
            }
            return lstSubCategories;
        }

        [Route("GetCustomerTopBrandItem")]
        [HttpGet]
        public async Task<List<factoryItemdata>> GetCustomerTopBrandItem(int warehouseId, int customerId, int subSubCategoryId, int skip, int take, string lang)
        {
            List<factoryItemdata> newdata = new List<factoryItemdata>();
            using (var db = new AuthContext())
            {
                var subCategoryQuery = "Exec GetCustomerTopBrandItem " + warehouseId + "," + customerId + "," + subSubCategoryId + "," + skip + "," + take;
                newdata = await db.Database.SqlQuery<factoryItemdata>(subCategoryQuery).ToListAsync();
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                foreach (var it in newdata)
                {
                    it.dreamPoint = it.dreamPoint.HasValue ? it.dreamPoint : 0;
                    it.marginPoint = it.marginPoint.HasValue ? it.marginPoint : 0;
                    if (!it.IsOffer)
                    {
                        /// Dream Point Logic && Margin Point
                        int? MP, PP;
                        double xPoint = xPointValue * 10;
                        //salesman 0.2=(0.02 * 10=0.2)
                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                        {
                            PP = 0;
                        }
                        else
                        {
                            PP = it.promoPerItems;
                        }
                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                        {
                            MP = 0;
                        }
                        else
                        {
                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                        }
                        if (PP > 0 && MP > 0)
                        {
                            int? PP_MP = PP + MP;
                            it.dreamPoint = PP_MP;
                        }
                        else if (MP > 0)
                        {
                            it.dreamPoint = MP;
                        }
                        else if (PP > 0)
                        {
                            it.dreamPoint = PP;
                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }

                    if (lang.Trim() == "hi")
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
            }
            return newdata;
        }

        [Route("GetCompanyBrand")]
        [HttpGet]
        public async Task<List<SubSubCategories>> GetCompanyBrand(int PeopleId, int warehouseId, int subcategoryId, string lang)
        {
            List<SubSubCategories> lstSubCategories = new List<SubSubCategories>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);

            using (var db = new AuthContext())
            {
                var brandQuery = "Exec GetSalesBrandBySubCategoryId " + warehouseId + "," + subcategoryId;
                lstSubCategories = db.Database.SqlQuery<SubSubCategories>(brandQuery).ToList();

                List<string> strCondition = StoreCategorySubCategoryBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.BrandId).Distinct().ToList();
                if (lstSubCategories != null)
                {
                    lstSubCategories = lstSubCategories.Where(x => strCondition.Contains(x.Categoryid + "-" + x.SubCategoryId + "-" + x.SubSubCategoryId)).ToList();
                    lstSubCategories = lstSubCategories.GroupBy(s => new { s.SubCategoryId, s.SubSubCategoryId, s.SubSubcategoryName, s.LogoUrl, s.HindiName }).Select(x => new SubSubCategories
                    {
                        Categoryid = 0,
                        HindiName = x.Key.HindiName,
                        itemcount = x.Sum(u => u.itemcount),
                        LogoUrl = x.Key.LogoUrl,
                        SubCategoryId = x.Key.SubCategoryId,
                        SubSubCategoryId = x.Key.SubSubCategoryId,
                        SubSubcategoryName = !string.IsNullOrEmpty(lang) && lang.Trim() == "hi" && !string.IsNullOrEmpty(x.Key.HindiName) ? x.Key.HindiName : x.Key.SubSubcategoryName
                    }).ToList();
                }
            }
            return lstSubCategories;
        }

        static DateTime GetDayDate(DateTime source, int dayOfWeek)
        {
            return source.AddDays(dayOfWeek - (int)source.DayOfWeek);
        }
        #region  CreateExecutiveBeat
        [Route("CreateExecutiveBeat")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> CreateExecutiveBeat(int numCluster, int warehouseId = 0)
        {
            bool result = false;
            ConcurrentBag<CustomerExecutiveMapping> customerExecutiveMappings = new ConcurrentBag<CustomerExecutiveMapping>();

            ParallelLoopResult parellelResult = new ParallelLoopResult();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var stores = await context.StoreDB.Where(x => x.IsActive && x.IsDeleted == false).ToListAsync();
                List<Warehouse> whs = new List<Warehouse>();
                if (warehouseId > 0)
                {
                    whs = await context.Warehouses.Where(x => x.WarehouseId == warehouseId).ToListAsync();
                }
                else
                {
                    whs = await context.Warehouses.Where(x => x.active == true && x.Deleted == false && x.IsKPP == false).ToListAsync();
                }
                foreach (var warehouse in whs)
                {
                    var clusterIds = new List<int>();
                    clusterIds = await context.Clusters.Where(x => x.Active && x.WarehouseId == warehouse.WarehouseId).Select(x => x.ClusterId).ToListAsync();
                    if (clusterIds.Any())
                    {
                        var clusterIdDt = new DataTable();
                        clusterIdDt.Columns.Add("IntValue");
                        foreach (var item in clusterIds)
                        {
                            var dr = clusterIdDt.NewRow();
                            dr["IntValue"] = item;
                            clusterIdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("clusterIds", clusterIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetClusterCustomers]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.Add(param);
                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var ClusterDatas = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<ClusterData>(reader).ToList();
                        ClusterHelper clusterHelper = new ClusterHelper();
                        parellelResult = Parallel.ForEach(stores, (store) =>
                        {
                            foreach (var item in ClusterDatas.GroupBy(x => x.ClusterId).ToList())
                            {
                                var customerData = item.ToList();
                                int clusterSize = customerData.Count > 30 ? (customerData.Count / numCluster) + 1 : customerData.Count;
                                var clusters = clusterHelper.Cluster(customerData, warehouse.latitude, warehouse.longitude, numCluster, clusterSize);
                                if (clusters.Any())
                                {
                                    var Exectivecusts = clusters.SelectMany(x => x.Customers.Select(y =>
                                                                          new CustomerExecutiveMapping
                                                                          {
                                                                              CreatedBy = 0,
                                                                              CreatedDate = DateTime.Now,
                                                                              CustomerId = y.ObjectId,
                                                                              // ExecutiveId = 0,// salesPerson.ExecutiveId,
                                                                              IsActive = true,
                                                                              IsDeleted = false,
                                                                              ModifiedBy = 0,
                                                                              StoreId = store.Id,
                                                                              StartDate = GetDayDate(DateTime.Now, (int)(DayOfWeek)(x.Clusterid == 7 ? 0 : x.Clusterid)),
                                                                              ModifiedDate = DateTime.Now,
                                                                              Day = ((DayOfWeek)(x.Clusterid == 7 ? 0 : x.Clusterid)).ToString(),
                                                                              Beat = x.Customers.IndexOf(y) + 1
                                                                          }).ToList()); ; ;
                                    foreach (var cust in Exectivecusts)
                                    {
                                        customerExecutiveMappings.Add(cust);
                                    }
                                }
                            }
                        });
                    }
                }
                if (parellelResult.IsCompleted && customerExecutiveMappings.Any())
                {
                    var bulkInsert = new BulkOperations();

                    var tblCustomerExecutiveMappings = Mapper.Map(customerExecutiveMappings).ToANew<List<tblCustomerExecutiveMapping>>();
                    bulkInsert.Setup<tblCustomerExecutiveMapping>(x => x.ForCollection(tblCustomerExecutiveMappings))
                        .WithTable("CustomerExecutiveMappings")
                        .WithBulkCopyBatchSize(4000)
                        .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                        .WithSqlCommandTimeout(720) // Default is 600 seconds
                        .AddAllColumns()
                        .BulkInsert();
                    bulkInsert.CommitTransaction("AuthContext");
                }
            }

            return result;
        }

        //[Route("CreateExecutiveBeat")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<bool> CreateExecutiveBeat(int numCluster)
        //{

        //    bool result = false;
        //    //List<CustomerExecutiveMapping> customerExecutiveMappings = new List<CustomerExecutiveMapping>();
        //    ConcurrentBag<CustomerExecutiveMapping> customerExecutiveMappings = new ConcurrentBag<CustomerExecutiveMapping>();
        //    ParallelLoopResult parellelResult = new ParallelLoopResult();
        //    using (var context = new AuthContext())
        //    {
        //        if (context.Database.Connection.State != ConnectionState.Open)
        //            context.Database.Connection.Open();

        //        var clusterExecutive = await context.Database.SqlQuery<ExecuteCluster>("Exec GetExecuteCluster").ToListAsync();

        //        var clusterIds = clusterExecutive.GroupBy(x => x.ClusterId).Select(x => x.Key).ToList();


        //        var clusterIdDt = new DataTable();
        //        clusterIdDt.Columns.Add("IntValue");
        //        foreach (var item in clusterIds)
        //        {
        //            var dr = clusterIdDt.NewRow();
        //            dr["IntValue"] = item;
        //            clusterIdDt.Rows.Add(dr);
        //        }


        //        var param = new SqlParameter("clusterIds", clusterIdDt);
        //        param.SqlDbType = SqlDbType.Structured;
        //        param.TypeName = "dbo.IntValues";
        //        var cmd = context.Database.Connection.CreateCommand();
        //        cmd.CommandText = "[dbo].[GetClusterCustomers]";
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //        cmd.CommandTimeout = 600;
        //        cmd.Parameters.Add(param);

        //        // Run the sproc
        //        var reader = cmd.ExecuteReader();
        //        var ClusterDatas = ((IObjectContextAdapter)context)
        //        .ObjectContext
        //        .Translate<ClusterData>(reader).ToList();

        //        var executiveIds = clusterExecutive.Select(x => x.ExecutiveId).ToList();
        //        if (executiveIds != null && executiveIds.Any())
        //        {
        //            var executiveCustomers = context.CustomerExecutiveMappings.Where(x => executiveIds.Contains(x.ExecutiveId) && x.IsActive).Select(x => new { x.ExecutiveId, x.CustomerId, x.Day }).ToList();
        //            ClusterHelper clusterHelper = new ClusterHelper();
        //            //foreach (var salesPerson in clusterExecutive)
        //            parellelResult = Parallel.ForEach(clusterExecutive, (salesPerson) =>
        //            {
        //                if (executiveCustomers.Any(x => x.ExecutiveId == salesPerson.ExecutiveId))
        //                {
        //                    var beat = executiveCustomers.Where(x => x.ExecutiveId == salesPerson.ExecutiveId).GroupBy(x => x.Day).Select(x => new { x.Key, x }).ToList();
        //                    var lstCustomerids = executiveCustomers.Where(x => x.ExecutiveId == salesPerson.ExecutiveId).Select(x => x.CustomerId).ToList();
        //                    var BeatCustomers = context.Customers.Where(x => lstCustomerids.Contains(x.CustomerId) /*&& x.ClusterId == salesPerson.ClusterId*/).Select(x => new { x.CustomerId, x.lat, x.lg }).ToList();
        //                    var BeatWithMeanLatLgs = new List<BeatWithMeanLatLg>();
        //                    var beatnotassignCustomers = ClusterDatas.Where(x => !lstCustomerids.Contains(x.ObjectId) && x.ClusterId == salesPerson.ClusterId).ToList();
        //                    if (beatnotassignCustomers.Any())
        //                    {
        //                        foreach (var item in beat)
        //                        {
        //                            var customerIds = item.x.Select(y => y.CustomerId).ToList();
        //                            var latlng = BeatCustomers.Where(x => customerIds.Contains(x.CustomerId)).Select(x => new GeoCoordinate { Latitude = x.lat, Longitude = x.lg }).ToList();
        //                            var centerpoint = GetCentralGeoCoordinate(latlng);
        //                            BeatWithMeanLatLgs.Add(new BeatWithMeanLatLg { Lat = centerpoint.Latitude, lg = centerpoint.Longitude, Day = item.Key });
        //                        }
        //                        foreach (var cust in beatnotassignCustomers)
        //                        {
        //                            string day = "";
        //                            int i = 0;
        //                            double lastdistance = -1;
        //                            do
        //                            {
        //                                var sCoord = new GeoCoordinate(BeatWithMeanLatLgs[i].Lat, BeatWithMeanLatLgs[i].lg);
        //                                var eCoord = new GeoCoordinate(cust.LATITUDE, cust.LONGITUDE);
        //                                var distance = sCoord.GetDistanceTo(eCoord);
        //                                if (lastdistance == -1)
        //                                {
        //                                    lastdistance = distance;
        //                                    day = BeatWithMeanLatLgs[i].Day;
        //                                }
        //                                else if (lastdistance > distance)
        //                                {
        //                                    lastdistance = distance;
        //                                    day = BeatWithMeanLatLgs[i].Day;
        //                                }
        //                                i++;
        //                            }
        //                            while (i < BeatWithMeanLatLgs.Count);
        //                            customerExecutiveMappings.Add(
        //                                                           new CustomerExecutiveMapping
        //                                                           {
        //                                                               CreatedBy = 0,
        //                                                               CreatedDate = DateTime.Now,
        //                                                               CustomerId = cust.ObjectId,
        //                                                               ExecutiveId = salesPerson.ExecutiveId,
        //                                                               IsActive = true,
        //                                                               IsDeleted = false,
        //                                                               ModifiedBy = 0,
        //                                                               ModifiedDate = DateTime.Now,
        //                                                               Day = day,
        //                                                               Beat = executiveCustomers.Count(x => x.Day == day) + 1
        //                                                           });

        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    foreach (var item in ClusterDatas.Where(x => x.ClusterId == salesPerson.ClusterId).GroupBy(x => x.ClusterId).ToList())
        //                    {
        //                        var customerData = item.ToList();
        //                        int clusterSize = customerData.Count > 30 ? (customerData.Count / numCluster) + 1 : customerData.Count;
        //                        var clusters = clusterHelper.Cluster(customerData, salesPerson.latitude, salesPerson.longitude, numCluster, clusterSize);

        //                        if (clusters.Any())
        //                        {
        //                            var Exectivecusts = clusters.SelectMany(x => x.Customers.Select(y =>
        //                                                                  new CustomerExecutiveMapping
        //                                                                  {
        //                                                                      CreatedBy = 0,
        //                                                                      CreatedDate = DateTime.Now,
        //                                                                      CustomerId = y.ObjectId,
        //                                                                      ExecutiveId = salesPerson.ExecutiveId,
        //                                                                      IsActive = true,
        //                                                                      IsDeleted = false,
        //                                                                      ModifiedBy = 0,
        //                                                                      ModifiedDate = DateTime.Now,
        //                                                                      Day = ((DayOfWeek)(x.Clusterid == 7 ? 0 : x.Clusterid)).ToString(),
        //                                                                      Beat = x.Customers.IndexOf(y) + 1
        //                                                                  }).ToList());
        //                            foreach (var cust in Exectivecusts)
        //                            {
        //                                customerExecutiveMappings.Add(cust);
        //                            }

        //                        }
        //                    }
        //                }
        //            });
        //        }
        //    }

        //    if (parellelResult.IsCompleted && customerExecutiveMappings.Any())
        //    {
        //        var TargetCustomers = new BulkOperations();
        //        var tblCustomerExecutiveMappings = Mapper.Map(customerExecutiveMappings).ToANew<List<tblCustomerExecutiveMapping>>();
        //        TargetCustomers.Setup<tblCustomerExecutiveMapping>(x => x.ForCollection(tblCustomerExecutiveMappings))
        //            .WithTable("CustomerExecutiveMappings")
        //            .WithBulkCopyBatchSize(4000)
        //            .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
        //            .WithSqlCommandTimeout(720) // Default is 600 seconds
        //            .AddAllColumns()
        //            .BulkInsert();
        //        TargetCustomers.CommitTransaction("AuthContext");

        //    }
        //    return result;
        //}

        #endregion
        private System.Device.Location.GeoCoordinate GetCentralGeoCoordinate(
        IList<System.Device.Location.GeoCoordinate> geoCoordinates)
        {
            if (geoCoordinates.Count == 1)
            {
                return geoCoordinates.Single();
            }

            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var geoCoordinate in geoCoordinates)
            {
                var latitude = geoCoordinate.Latitude * Math.PI / 180;
                var longitude = geoCoordinate.Longitude * Math.PI / 180;

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }

            var total = geoCoordinates.Count;

            x = x / total;
            y = y / total;
            z = z / total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return new System.Device.Location.GeoCoordinate(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
        }


        [Route("CustomerOrderHistory")]
        [HttpGet]
        public async Task<SalesCustomerOrders> GetCustomerOrderHistory(int customerId)
        {
            SalesCustomerOrders customerOrders = new SalesCustomerOrders();
            List<CusterOrderHistory> custerOrderHistories = new List<CusterOrderHistory>();
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.Parameters.Add(new SqlParameter("@customerId", @customerId));
                cmd.CommandText = "[dbo].[GetCustomerOrderHistoryForSalesApp]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;

                var reader = cmd.ExecuteReader();
                custerOrderHistories = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<CusterOrderHistory>(reader).ToList();
                custerOrderHistories = custerOrderHistories.OrderBy(x => x.SubCategoryName).ThenBy(x => x.BrandName).ThenBy(x => x.ItemName).ToList();
            }

            if (custerOrderHistories != null && custerOrderHistories.Any())
            {
                customerOrders.CusterOrderHistories = custerOrderHistories;
                customerOrders.Brands = custerOrderHistories.Select(x => x.BrandName).Distinct().ToList();
                customerOrders.Subcategories = custerOrderHistories.Select(x => x.SubCategoryName).Distinct().ToList();
            }

            return customerOrders;
        }

        [Route("InsertSalesBeatTarget")]
        [HttpGet]
        public async Task<bool> InsertSalesClusterTarget()
        {
            MongoDbHelper<ExecuteBeatTarget> mongoDbHelper = new MongoDbHelper<ExecuteBeatTarget>();
            await mongoDbHelper.InsertAsync(new ExecuteBeatTarget
            {
                AvgLineItem = 3,
                AvgOrderAmount = 2000,
                CityId = 0,
                ClusterId = 46,
                ConversionPercent = 50,
                StartDate = DateTime.Now,
                EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddMilliseconds(-1),
                OrderPercent = 60,
                VisitedPercent = 75,
            });

            return true;
        }




        [Route("getSaleIntentItem")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getSaleIntentItem(string lang, int PeopleId, int warehouseid, int catid, int scatid, int sscatid, int Skip, int Take)
        {
            using (var context = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                var newdata = (from a in context.ItemMultiMRPDB
                               join b in context.ItemMasterCentralDB on a.ItemNumber equals b.Number

                               let forcast = context.ItemForecastDetailDb.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && p2.WarehouseId == warehouseid).FirstOrDefault()
                               where a.Deleted == false && b.Deleted == false
                               && ((catid > 0 && b.Categoryid == catid) || (catid == 0 && b.Categoryid == b.Categoryid))
                               && ((scatid > 0 && b.SubCategoryId == scatid) || (scatid == 0 && b.SubCategoryId == b.SubCategoryId))
                               && ((sscatid > 0 && b.SubsubCategoryid == sscatid) || (sscatid == 0 && b.SubsubCategoryid == b.SubsubCategoryid))
                               && CatIds.Contains(b.Categoryid) && SubCats.Contains(b.SubCategoryId) && SubSubCats.Contains(b.SubsubCategoryid)
                               select new
                               {
                                   Number = a.ItemNumber,
                                   itemBaseName = b.itemBaseName,
                                   IsSensitive = b.IsSensitive,
                                   IsSensitiveMRP = b.IsSensitiveMRP,
                                   UnitofQuantity = a.UnitofQuantity,
                                   UOM = a.UOM,
                                   LogoUrl = b.LogoUrl,
                                   MRP = a.MRP,
                                   HindiName = b.HindiName,
                                   ItemMultiMRPId = a.ItemMultiMRPId,
                                   SubCategoryId = b.SubCategoryId,
                                   Categoryid = b.Categoryid,
                                   SubsubCategoryid = b.SubsubCategoryid,
                                   Warehouseid = warehouseid,
                                   SystemSuggestedQty = forcast == null ? 0 : forcast.SystemSuggestedQty,

                               }).Distinct().OrderBy(x => x.itemBaseName).Skip(Skip).Take(Take).ToList();

                var itemnumberlist = newdata.Select(x => x.Number).ToList();

                var res = context.itemMasters.Where(x => itemnumberlist.Contains(x.Number) && x.WarehouseId == warehouseid && x.Deleted == false).Select(y => new { y.PurchaseMinOrderQty, y.Number }).Distinct().ToList();



                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion
                List<SalesIntentItemResponse> item = new List<SalesIntentItemResponse>();
                foreach (var si in newdata)
                {
                    SalesIntentItemResponse it = new SalesIntentItemResponse();
                    it.IsSensitive = si.IsSensitive;
                    it.IsSensitiveMRP = si.IsSensitiveMRP;
                    it.ItemMultiMRPId = si.ItemMultiMRPId;
                    it.Itemname = si.itemBaseName;
                    it.LogoUrl = si.LogoUrl;
                    it.MRP = si.MRP;
                    it.SystemForecastQty = si.SystemSuggestedQty;
                    it.WarehouseId = si.Warehouseid;
                    it.PurchaseMOQList = res.Any(x => x.Number == si.Number) ? res.Where(x => x.Number == si.Number).Select(x => x.PurchaseMinOrderQty).ToList() : new List<int>();
                    if (it.IsSensitive == true)
                    {
                        if (it.IsSensitiveMRP == false)
                        {
                            it.Itemname = si.itemBaseName + " " + si.UnitofQuantity + " " + si.UOM; //item display name   
                        }
                        else
                        {
                            it.Itemname = si.itemBaseName + " " + si.MRP + " MRP " + si.UnitofQuantity + " " + si.UOM; //item display name                               
                        }
                    }
                    else
                    {
                        it.Itemname = si.itemBaseName + " " + si.MRP + " MRP "; //item display name                               
                    }
                    if (lang.Trim() == "hi")
                    {
                        if (it.IsSensitive == true)
                        {
                            if (it.IsSensitiveMRP == false)
                            {
                                it.Itemname = (string.IsNullOrEmpty(si.HindiName) ? si.itemBaseName : si.HindiName) + " " + si.UnitofQuantity + " " + si.UOM; //item display name   
                            }
                            else
                            {
                                it.Itemname = (string.IsNullOrEmpty(si.HindiName) ? si.itemBaseName : si.HindiName) + " " + si.MRP + " MRP " + si.UnitofQuantity + " " + si.UOM; //item display name                               
                            }
                        }
                        else
                        {
                            it.Itemname = (string.IsNullOrEmpty(si.HindiName) ? si.itemBaseName : si.HindiName) + " " + si.MRP + " MRP "; //item display name                               
                        }
                    }
                    item.Add(it);
                }

                return Request.CreateResponse(HttpStatusCode.OK, item);
            }
        }
        //new Search
        [Route("getSaleIntentItemSearch")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getSaleIntentItemSearch(string lang, int PeopleId, int warehouseid, string itemName)
        {
            using (var context = new AuthContext())
            {
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                List<SalesIntentItemResponse> item = new List<SalesIntentItemResponse>();
                //Increase some parameter For offer
                var newdata = (from a in context.ItemMultiMRPDB
                               join b in context.ItemMasterCentralDB on a.ItemNumber equals b.Number
                               let forcast = context.ItemForecastDetailDb.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && p2.WarehouseId == warehouseid).FirstOrDefault()
                               where a.Deleted == false && b.Deleted == false  // New Add
                                   && a.itemname.Contains(itemName)
                                   && CatIds.Contains(b.Categoryid) && SubCats.Contains(b.SubCategoryId) && SubSubCats.Contains(b.SubsubCategoryid)

                               select new
                               {
                                   ItemNumber = a.ItemNumber,
                                   itemname = b.itemBaseName,
                                   IsSensitive = b.IsSensitive,
                                   IsSensitiveMRP = b.IsSensitiveMRP,
                                   UnitofQuantity = a.UnitofQuantity,
                                   UOM = a.UOM,
                                   LogoUrl = b.LogoUrl,
                                   MRP = a.MRP,
                                   HindiName = b.HindiName,
                                   ItemMultiMRPId = a.ItemMultiMRPId,
                                   SubCategoryId = b.SubCategoryId,
                                   Categoryid = b.Categoryid,
                                   SubsubCategoryid = b.SubsubCategoryid,
                                   WarehouseId = warehouseid,
                                   SystemSuggestedQty = forcast == null ? 0 : forcast.SystemSuggestedQty
                               }).Distinct().ToList();

                var itemnumberlist = newdata.Select(x => x.ItemNumber).ToList();

                var res = context.itemMasters.Where(x => itemnumberlist.Contains(x.Number) && x.WarehouseId == warehouseid && x.Deleted == false).Select(y => new { y.PurchaseMinOrderQty, y.Number }).Distinct().ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                foreach (var si in newdata)
                {
                    SalesIntentItemResponse it = new SalesIntentItemResponse();
                    it.IsSensitive = si.IsSensitive;
                    it.IsSensitiveMRP = si.IsSensitiveMRP;
                    it.ItemMultiMRPId = si.ItemMultiMRPId;
                    it.Itemname = si.itemname;
                    it.LogoUrl = si.LogoUrl;
                    it.MRP = si.MRP;
                    it.SystemForecastQty = si.SystemSuggestedQty;
                    it.WarehouseId = si.WarehouseId;
                    it.PurchaseMOQList = res.Any(x => x.Number == si.ItemNumber) ? res.Where(x => x.Number == si.ItemNumber).Select(x => x.PurchaseMinOrderQty).ToList() : new List<int>();  // TO DO new Add
                    if (it.IsSensitive == true)
                    {
                        if (it.IsSensitiveMRP == false)
                        {
                            it.Itemname = si.itemname + " " + si.UnitofQuantity + " " + si.UOM; //item display name   
                        }
                        else
                        {
                            it.Itemname = si.itemname + " " + si.MRP + " MRP " + si.UnitofQuantity + " " + si.UOM; //item display name                               
                        }
                    }
                    else
                    {
                        it.Itemname = si.itemname + " " + si.MRP + " MRP "; //item display name                               
                    }
                    if (lang.Trim() == "hi")
                    {
                        if (it.IsSensitive == true)
                        {
                            if (it.IsSensitiveMRP == false)
                            {
                                it.Itemname = (string.IsNullOrEmpty(si.HindiName) ? si.itemname : si.HindiName) + " " + si.UnitofQuantity + " " + si.UOM; //item display name   
                            }
                            else
                            {
                                it.Itemname = (string.IsNullOrEmpty(si.HindiName) ? si.itemname : si.HindiName) + " " + si.MRP + " MRP " + si.UnitofQuantity + " " + si.UOM; //item display name                               
                            }
                        }
                        else
                        {
                            it.Itemname = (string.IsNullOrEmpty(si.HindiName) ? si.itemname : si.HindiName) + " " + si.MRP + " MRP "; //item display name                               
                        }
                    }
                    item.Add(it);
                }

                return Request.CreateResponse(HttpStatusCode.OK, item);
            }

        }


        [HttpPost]
        [Route("SearchItemPrice")]
        public async Task<SearchItemResponse> SearchItemPrice(SearchItemRequest searchItemRequest)
        {
            SearchItemResponse searchItemResponse = new SearchItemResponse();
            searchItemResponse.Suggestion = new List<string>();
            if (string.IsNullOrEmpty(searchItemRequest.Keyword) && searchItemRequest.BrandId == 0 && searchItemRequest.CategoryId == 0 && searchItemRequest.CompanyId == 0 && searchItemRequest.ItemId == 0)
            {
                searchItemResponse.Items = new List<SearchData>();
                searchItemResponse.IdField = "";
                searchItemResponse.IsStopSearch = true;
                searchItemResponse.message = "छमा करे हमें आप की तरफ से कोई इनपुट प्राप्त नहीं हुआ। आप कोई और आइटम की प्राइस जानना चाहते हो।";
                return searchItemResponse;
            }
            if (searchItemRequest.IsRequiredSuggestion && searchItemRequest.BrandId == 0 && searchItemRequest.CategoryId == 0 && searchItemRequest.CompanyId == 0 && searchItemRequest.ItemId == 0)
            {
                Spelling spelling = new Spelling();
                string suggestion = spelling.Correct(searchItemRequest.Keyword);
                if (searchItemRequest.Keyword.ToLower() != suggestion)
                {
                    searchItemResponse.Items = new List<SearchData>();
                    searchItemResponse.Suggestion = new List<string> { suggestion };
                    searchItemResponse.IdField = "";
                    searchItemResponse.IsStopSearch = true;
                    searchItemResponse.message = "क्या आप नीचे दिये गये सजेसन से सर्च करना चाहते हो।";
                    return searchItemResponse;
                }
            }

            ElasticSqlHelper<SearchItem> elasticSqlHelper = new ElasticSqlHelper<SearchItem>();
            string itemIndex = ConfigurationManager.AppSettings["ElasticSearchIndexName"];
            string columns = "itemid,logourl,itemname,itemmultimrpid,categoryid,categoryname,subcategoryid,subcategoryname,subsubcategoryid,subsubcategoryname,price,unitprice,minorderqty";
            string query = $"SELECT {columns}  from {itemIndex} where warehouseid={searchItemRequest.WarehouseId}  and active=true and deleted=false and  isdiscontinued=false and (isitemlimit=false or (isitemlimit=true and itemlimitqty>0 and itemlimitqty-minorderqty>0 )) and (itemapptype=0 or itemapptype=1)";
            string SearchFrom = "";
            string condition = "";
            var itemdetails = new List<SearchItem>();
            if (searchItemRequest.BrandId == 0 && searchItemRequest.CategoryId == 0 && searchItemRequest.CompanyId == 0 && searchItemRequest.ItemId == 0)
            {
                SearchFrom = "Category";
                condition = query + $" and ucase(categoryname) like '%{searchItemRequest.Keyword.ToUpper()}%'";

                itemdetails = (await elasticSqlHelper.GetListAsync(condition)).ToList();
            }
            else if (searchItemRequest.CategoryId > 0)
            {
                SearchFrom = "WithCategory";
                query += $" and categoryid =  {searchItemRequest.CategoryId} ";
            }


            if (!itemdetails.Any() && searchItemRequest.CompanyId == 0 && searchItemRequest.BrandId == 0 && searchItemRequest.ItemId == 0)
            {
                SearchFrom = "Company";
                condition = query + $" and ucase(subcategoryname) like '%{searchItemRequest.Keyword.ToUpper()}%'";

                itemdetails = (await elasticSqlHelper.GetListAsync(condition)).ToList();
            }
            else if (searchItemRequest.CompanyId > 0)
            {
                SearchFrom = "WithCompany";
                query += $" and subcategoryid =  {searchItemRequest.CompanyId} ";
            }

            if (!itemdetails.Any() && searchItemRequest.BrandId == 0 && searchItemRequest.ItemId == 0 && !SearchFrom.Contains("With"))
            {
                SearchFrom = "Brand";
                condition = query + $" and ucase(subsubcategoryname) like '%{searchItemRequest.Keyword.ToUpper()}%'";

                itemdetails = (await elasticSqlHelper.GetListAsync(condition)).ToList();
            }
            else if (searchItemRequest.BrandId > 0)
            {
                SearchFrom = "WithBrand";
                query += $" and subsubcategoryid =  {searchItemRequest.BrandId} ";
            }

            if (!itemdetails.Any() && searchItemRequest.ItemId == 0 && !SearchFrom.Contains("With"))
            {
                SearchFrom = "Item";
                condition = query + $" and ucase(itemname) like '%{searchItemRequest.Keyword.ToUpper()}%'";

                itemdetails = (await elasticSqlHelper.GetListAsync(condition)).ToList();
            }
            else if (searchItemRequest.ItemId > 0)
            {
                SearchFrom = "Price";
                query += $" and itemid =  {searchItemRequest.ItemId} ";
            }

            if (!itemdetails.Any() && (SearchFrom == "Price" || SearchFrom.Contains("With")))
            {
                itemdetails = (await elasticSqlHelper.GetListAsync(query)).ToList();
            }

            if (itemdetails.Any())
            {

                switch (SearchFrom)
                {
                    case "Category":
                        {
                            searchItemResponse.Items = itemdetails.GroupBy(x => new { subcategoryid = x.subcategoryid, subcategoryname = x.subcategoryname }).Select(x => new SearchData
                            {
                                Id = x.Key.subcategoryid,
                                Name = x.Key.subcategoryname
                            }).ToList();
                            searchItemResponse.IdField = "CompanyId";
                            searchItemResponse.message = "यह केटेगरी निम्न कंपनी में उपलब्ध है। कृपया अधिक जानकारी के लिए ,इनमे से एक कंपनी पर क्लिक करे। ";
                            break;
                        }
                    case "Company":
                        {
                            searchItemResponse.Items = itemdetails.GroupBy(x => new { subsubcategoryid = x.subsubcategoryid, subsubcategoryname = x.subsubcategoryname }).Select(x => new SearchData
                            {
                                Id = x.Key.subsubcategoryid,
                                Name = x.Key.subsubcategoryname
                            }).ToList();
                            searchItemResponse.IdField = "BrandId";
                            searchItemResponse.message = "यह कंपनी निम्न ब्रांड में उपलब्ध है। कृपया अधिक जानकारी के लिए ,इनमे से एक ब्रांड पर क्लिक करे। ";
                            break;
                        }
                    case "Brand":
                        {
                            searchItemResponse.Items = itemdetails.Select(x => new SearchData
                            {
                                Id = x.itemid,
                                Name = x.itemname
                            }).ToList();

                            searchItemResponse.IdField = "ItemId";
                            searchItemResponse.message = "यह ब्रांड निम्न आइटम में उपलब्ध है। कृपया अधिक जानकारी के लिए ,इनमे से एक आइटम पर क्लिक करे। ";

                            break;
                        }
                    case "WithCategory":
                        {
                            searchItemResponse.Items = itemdetails.GroupBy(x => new { subcategoryid = x.subcategoryid, subcategoryname = x.subcategoryname }).Select(x => new SearchData
                            {
                                Id = x.Key.subcategoryid,
                                Name = x.Key.subcategoryname
                            }).ToList();
                            searchItemResponse.IdField = "CompanyId";
                            searchItemResponse.message = "सिलेक्टेड केटेगरी की निम्न कंपनी उपलब्ध है। कृपया अधिक जानकारी के लिए ,इनमे से एक कंपनी पर क्लिक करे। ";
                            break;
                        }
                    case "WithCompany":
                        {
                            searchItemResponse.Items = itemdetails.GroupBy(x => new { subsubcategoryid = x.subsubcategoryid, subsubcategoryname = x.subsubcategoryname }).Select(x => new SearchData
                            {
                                Id = x.Key.subsubcategoryid,
                                Name = x.Key.subsubcategoryname
                            }).ToList();
                            searchItemResponse.IdField = "BrandId";
                            searchItemResponse.message = "सिलेक्टेड कंपनी की निम्न ब्रांड उपलब्ध है। कृपया अधिक जानकारी के लिए ,इनमे से एक ब्रांड पर क्लिक करे। ";
                            break;
                        }
                    case "WithBrand":
                        {
                            searchItemResponse.Items = itemdetails.GroupBy(x => new { itemid = x.itemid, itemname = x.itemname, MOQ = x.minorderqty }).Select(x => new SearchData
                            {
                                Id = x.Key.itemid,
                                Name = x.Key.itemname + "(MOQ:" + x.Key.MOQ + ")",
                                MOQ = x.Key.MOQ
                            }).ToList();

                            searchItemResponse.IdField = "ItemId";
                            searchItemResponse.message = "सिलेक्टेड ब्रांड के निम्न आइटम उपलब्ध है। कृपया अधिक जानकारी के लिए ,इनमे से एक आइटम पर क्लिक करे। ";


                            break;
                        }

                    case "Item":
                        {
                            searchItemResponse.Items = itemdetails.Select(x => new SearchData
                            {
                                Id = x.itemid,
                                Name = x.itemname + "(MOQ:" + x.minorderqty + ")",
                                MOQ = x.minorderqty
                            }).ToList();
                            if (searchItemResponse.Items.Count() == 1)
                            {
                                searchItemResponse.message = $"इस आइटम की हमारे यहाँ ₹{ itemdetails.FirstOrDefault().unitprice }/- प्राइस है। आप कोई और आइटम की प्राइस जानना चाहते हो।";
                                searchItemResponse.IsStopSearch = true;
                                searchItemResponse.IdField = "";
                            }
                            else
                            {
                                searchItemResponse.IdField = "ItemId";
                                searchItemResponse.message = "यह आइटम उपलब्ध है। कृपया अधिक जानकारी के लिए ,इनमे से एक आइटम पर क्लिक करे। ";
                            }
                            break;
                        }
                    case "Price":
                        {
                            searchItemResponse.Items = new List<SearchData>();
                            searchItemResponse.IsStopSearch = true;
                            searchItemResponse.IdField = "";
                            searchItemResponse.message = $"इस आइटम की हमारे यहाँ ₹{ itemdetails.FirstOrDefault().unitprice }/- प्राइस है। आप कोई और आइटम की प्राइस जानना चाहते हो।";
                            break;
                        }
                }
            }
            else
            {
                searchItemResponse.Items = new List<SearchData>();
                searchItemResponse.IdField = "";
                searchItemResponse.IsStopSearch = true;
                searchItemResponse.message = "छमा करे सर्च कीवर्ड से हमारे यहाँ कोई भी आइटम लाइव नहीं है। आप कोई और आइटम की प्राइस जानना चाहते हो।";
            }

            return searchItemResponse;
        }

        [HttpGet]
        [Route("AddItemToCart")]
        public async Task<AddCartResponse> AddItemToCart(int peopleId, int warehouseId, int customerId, int itemId, int qty)
        {
            AddCartResponse addCartResponse = new AddCartResponse { result = false, message = "" };
            double unitPrice = 0;
            using (var context = new AuthContext())
            {
                unitPrice = context.itemMasters.FirstOrDefault(x => x.ItemId == itemId).UnitPrice;
            }
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

            cartPredicate = cartPredicate.And(x => x.PeopleId == peopleId);

            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate)).FirstOrDefault();

            if (customerShoppingCart != null)
            {
                UpdateResult result = null;
                var _collection = mongoDbHelper.mongoDatabase.GetCollection<CustomerShoppingCart>("CustomerShoppingCart");

                if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == itemId && x.IsFreeItem == false))
                {
                    var Id = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId && x.IsFreeItem == false).Id;
                    var filter = Builders<CustomerShoppingCart>.Filter.Where(x => x.Id == customerShoppingCart.Id
                                                                  && x.ShoppingCartItems.Any(i => i.ItemId == itemId && i.IsFreeItem == false));

                    var update = Builders<CustomerShoppingCart>.Update.Set(x => x.ShoppingCartItems.FirstMatchingElement().IsActive, true)
                                                                      .Set(x => x.ShoppingCartItems.FirstMatchingElement().IsDeleted, false)
                                                                      .Set(x => x.ShoppingCartItems.FirstMatchingElement().qty, qty)
                                                                      .Set(x => x.ShoppingCartItems.FirstMatchingElement().UnitPrice, unitPrice)
                                                                      .Set(x => x.ShoppingCartItems.FirstMatchingElement().IsPrimeItem, false)
                                                                      .Set(x => x.ShoppingCartItems.FirstMatchingElement().ModifiedDate, DateTime.Now)
                                                                      .Set(x => x.ShoppingCartItems.FirstMatchingElement().ModifiedBy, customerId)
                                                                      .Set(x => x.ShoppingCartItems.FirstMatchingElement().IsFreeItem, false)
                                                                      .Set(x => x.ShoppingCartItems.FirstMatchingElement().IsDealItem, false)
                                                                      .Set(x => x.ModifiedDate, DateTime.Now)
                                                                      .Set(x => x.ModifiedBy, customerId);
                    result = _collection.UpdateOneAsync(filter, update).Result;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).IsActive = true;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).qty = qty;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).UnitPrice = unitPrice;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).IsPrimeItem = false;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).IsDeleted = false;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).ModifiedDate = DateTime.Now;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).ModifiedBy = customerId;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).IsFreeItem = false;
                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == itemId).IsDealItem = false;
                }
                else
                {
                    var shoppingCartItem = new ShoppingCartItem
                    {
                        CreatedBy = customerId,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        IsFreeItem = false,
                        ModifiedBy = customerId,
                        ItemId = itemId,
                        qty = qty,
                        UnitPrice = unitPrice,
                        IsPrimeItem = false,
                        IsDealItem = false,
                        TaxAmount = 0
                    };
                    var filter = Builders<CustomerShoppingCart>.Filter.Where(x => x.Id == customerShoppingCart.Id);

                    var update = Builders<CustomerShoppingCart>.Update.Push(x => x.ShoppingCartItems, shoppingCartItem)
                                                                      .Set(x => x.ModifiedDate, DateTime.Now)
                                                                      .Set(x => x.ModifiedBy, customerId);
                    result = await _collection.UpdateOneAsync(filter, update);

                    customerShoppingCart.ShoppingCartItems.Add(new ShoppingCartItem
                    {
                        CreatedBy = customerId,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        IsFreeItem = false,
                        ModifiedBy = customerId,
                        ItemId = itemId,
                        qty = qty,
                        UnitPrice = unitPrice,
                        IsPrimeItem = false,
                        TaxAmount = 0,
                        IsDealItem = false
                    });
                }
                addCartResponse.result = result.IsAcknowledged && result.ModifiedCount > 0; //await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart);
            }
            else
            {
                customerShoppingCart = new CustomerShoppingCart
                {
                    IsActive = true,
                    CartTotalAmt = 0,
                    CreatedBy = customerId,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    CustomerId = customerId,
                    PeopleId = peopleId,
                    DeamPoint = 0,
                    DeliveryCharges = 0,
                    GrossTotalAmt = 0,
                    IsDeleted = false,
                    TotalDiscountAmt = 0,
                    TotalTaxAmount = 0,
                    WalletAmount = 0,
                    WarehouseId = warehouseId,
                    ShoppingCartItems = new List<ShoppingCartItem> {
                     new ShoppingCartItem {
                         CreatedBy = customerId,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        IsFreeItem = false,
                        ModifiedBy = customerId,
                        ItemId = itemId,
                        qty = qty,
                        UnitPrice = unitPrice,
                        IsPrimeItem=false,
                        IsDealItem=false,
                        //TaxPercentage = itemmaster.TotalTaxPercentage,
                       TaxAmount= 0//itemmaster.TotalTaxPercentage>0? (cartItemDc.qty * cartItemDc.UnitPrice) * itemmaster.TotalTaxPercentage/100:0
                     }
                    }
                };
                addCartResponse.result = await mongoDbHelper.InsertAsync(customerShoppingCart);
            }

            if (addCartResponse.result)
                addCartResponse.message = "आप का आइटम कार्ट मे ऐड हो गया है क्या आप चेकआउट करना चाहते हो। ";
            else
                addCartResponse.message = "आइटम को कार्ट मे ऐड करने मे कोई प्रॉब्लम आ रही है थोड़ी दर से फिर प्रयास करे।";

            return addCartResponse;
        }

        [HttpGet]
        [Route("FileCreateSearchItemWord")]
        public void UpdateItemSearchFile()
        {
            using (var context = new AuthContext())
            {
                var data = context.ItemMasterCentralDB.Where(x => x.Deleted == false).Select(x => new
                {
                    x.BaseCategoryName,
                    x.CategoryName,
                    x.SubcategoryName,
                    x.SubsubcategoryName,
                    x.itemBaseName
                });

                var filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Templates"), "SearchItemWord.txt");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (StreamWriter sw = File.CreateText(filePath))
                //    using (TextWriter tw = new StreamWriter(filePath))
                {

                    foreach (var item in data.GroupBy(x => x.CategoryName))
                    {
                        sw.WriteLine(item.Key);
                        foreach (var splititem in item.Key.Split(' '))
                        {
                            sw.WriteLine(splititem);
                        }
                    }
                    foreach (var item in data.GroupBy(x => x.SubcategoryName))
                    {
                        sw.WriteLine(item.Key);
                        foreach (var splititem in item.Key.Split(' '))
                        {
                            sw.WriteLine(splititem);
                        }
                    }
                    foreach (var item in data.GroupBy(x => x.SubsubcategoryName))
                    {
                        sw.WriteLine(item.Key);
                        foreach (var splititem in item.Key.Split(' '))
                        {
                            sw.WriteLine(splititem);
                        }
                    }
                    foreach (var item in data.GroupBy(x => x.itemBaseName))
                    {
                        sw.WriteLine(item.Key);
                    }
                }
            }

        }

        #region Filter Api V2

        [Route("ItemListForAgentV2")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ItemListForAgent> ItemListForAgentV2(int WarehouseId, string lang, int PeopleId, int Skip, int Take, string IncentiveClassifications, int customerId = 0)
        {
            List<string> IncentiveClassificationList = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                ItemListForAgent item = new ItemListForAgent();

                var IdDt1 = new DataTable();
                SqlParameter param1 = null;

                IdDt1 = new DataTable();
                IdDt1.Columns.Add("stringValue");
                foreach (var ic in IncentiveClassificationList)
                {
                    var dr = IdDt1.NewRow();
                    dr["stringValue"] = ic;
                    IdDt1.Rows.Add(dr);
                }
                param1 = new SqlParameter("Classification", IdDt1);
                param1.SqlDbType = SqlDbType.Structured;
                param1.TypeName = "dbo.stringValues";

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemForAgentAppV2]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                cmd.Parameters.Add(new SqlParameter("@customerId", customerId));
                cmd.Parameters.Add(new SqlParameter("@Skip", Skip));
                cmd.Parameters.Add(new SqlParameter("@Take", Take));
                cmd.Parameters.Add(param1);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = cmd.ExecuteReader();
                var newdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<Itemdata>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    item.TotalItem = Convert.ToInt32(reader["TotalItem"]);
                }


                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                var offerids = newdata.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (newdata != null && newdata.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(WarehouseId, newdata.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                }
                BackendOrderController backendOrderController = new BackendOrderController();
                foreach (var it in newdata.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1) && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)))
                {

                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;
                    if (customerId > 0)
                    {
                        double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                        it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);
                    }
                        

                    if (!it.OfferId.HasValue || it.OfferId.Value == 0)
                    {
                        it.IsOffer = false;
                    }
                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                        it.IsOffer = true;
                    else
                        it.IsOffer = false;

                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }
                    }


                    if (it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                    }
                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<Itemdata>();
                    }
                    try
                    {/// Dream Point Logic && Margin Point
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //salesman 0.2=(0.02 * 10=0.2)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }

                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }

                    }
                    catch { }
                    if (lang.Trim() == "hi")
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
                    if (it.OfferType != "FlashDeal")
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }
                    item.ItemMasters.Add(it);
                }
                if (item.ItemMasters != null && item.ItemMasters.Any())
                {
                    item.Message = "Success";
                    item.Status = true;
                    item.ItemMasters.Where(x => !x.marginPoint.HasValue).ToList().ForEach(x => x.marginPoint = 0);
                    item.ItemMasters = item.ItemMasters.OrderByDescending(x => x.marginPoint).ToList();
                    return item;
                }
                else
                {
                    item.Message = "Item Not found";
                    item.Status = false;
                    return item;
                }



            }


        }

        [Route("GetCompanyTopMarginItemV2")]
        [HttpGet]
        public async Task<ItemResponseDc> GetCompanyTopMarginItemV2(int PeopleId, int warehouseId, int skip, int take, string lang, string IncentiveClassifications, int customerId = 0)
        {
            List<string> IncentiveClassificationList = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                var brandids = string.Join(",", SubSubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();

                var IdDt1 = new DataTable();
                SqlParameter param1 = null;

                IdDt1 = new DataTable();
                IdDt1.Columns.Add("stringValue");
                foreach (var ic in IncentiveClassificationList)
                {
                    var dr = IdDt1.NewRow();
                    dr["stringValue"] = ic;
                    IdDt1.Rows.Add(dr);
                }
                param1 = new SqlParameter("Classification", IdDt1);
                param1.SqlDbType = SqlDbType.Structured;
                param1.TypeName = "dbo.stringValues";

                DataTable brandidDt = new DataTable();
                brandidDt.Columns.Add("IntValue");
                foreach (var item in brandids)
                {
                    DataRow dr = brandidDt.NewRow();
                    dr[0] = item;
                    brandidDt.Rows.Add(dr);
                }
                var SubSubCategoryIds = new SqlParameter("SubSubCategoryIds", brandidDt);
                SubSubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubSubCategoryIds.TypeName = "dbo.IntValues";


                var Subcatids = string.Join(",", SubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable SubCatidDt = new DataTable();
                SubCatidDt.Columns.Add("IntValue");
                foreach (var item in Subcatids)
                {
                    DataRow dr = SubCatidDt.NewRow();
                    dr[0] = item;
                    SubCatidDt.Rows.Add(dr);
                }
                var SubCategoryIds = new SqlParameter("SubCategoryIds", SubCatidDt);
                SubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubCategoryIds.TypeName = "dbo.IntValues";

                var Categoryid = string.Join(",", CatIds).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable CatIdDt = new DataTable();
                CatIdDt.Columns.Add("IntValue");
                foreach (var item in Categoryid)
                {
                    DataRow dr = CatIdDt.NewRow();
                    dr[0] = item;
                    CatIdDt.Rows.Add(dr);
                }
                var CategoryIds = new SqlParameter("CategoryIds", CatIdDt);
                CategoryIds.SqlDbType = SqlDbType.Structured;
                CategoryIds.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetCompanyTopMarginItemSalesAppV2]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@Skip", skip));
                cmd.Parameters.Add(new SqlParameter("@Take", take));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(SubSubCategoryIds);
                cmd.Parameters.Add(SubCategoryIds);
                cmd.Parameters.Add(CategoryIds);
                cmd.Parameters.Add(param1);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var ItemData = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                reader.NextResult();
                while (reader.Read())
                {
                    itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                }

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    ItemData = ItemData.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (ItemData != null && ItemData.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                }
                BackendOrderController backendOrderController = new BackendOrderController();
                foreach (var it in ItemData.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1) && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)))
                {

                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;
                    if (customerId > 0)
                    {
                        double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                        it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);
                    }
                        
                    //Condition for offer end

                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                        else if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }

                    }
                    else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;

                    }

                    if (it.OfferCategory == 1)
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }

                    try
                    {
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //Customer (0.2 * 10=1)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }
                        }
                        else { it.dreamPoint = 0; }

                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }
                    catch { }

                    if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                    ItemDataDCs.Add(it);
                }

                itemResponseDc.ItemDataDCs = ItemDataDCs;
            }
            return itemResponseDc;
        }

        [Route("GetLastPurchaseItemNewV2")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ItemResponseDc> GetLastPurchaseItemAsyncNewV2(int PeopleId, int customerId, int warehouseId, int skip, int take, string lang, string IncentiveClassifications)
        {
            List<string> IncentiveClassificationList = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);

                if (blockBarnds != null && blockBarnds.Any())
                {
                    StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
                }

                #endregion

                var categoryIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                var companyIds = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                var brandIds = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();

                var enddate = DateTime.Now;
                var startDate = enddate.AddMonths(-9);
                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted
                                                    && x.CustomerId == customerId && x.WarehouseId == warehouseId);

                var platformIdxName = "skorderdata_" + AppConstants.Environment;
                //ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemNumber> elasticSqlHelper = new ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemNumber>();

                // var orderdatas = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync($"SELECT top 100 itemnumber,itemname,createddate,ordqty,itemmultimrpid from {platformIdxName} where custid='{customerId}' and whid='{warehouseId}' order by createddate desc,ordqty desc"))).ToList();

                string query = $" SELECT top 10 orderid from skorderdata_{AppConstants.Environment} where custid={customerId} and whid='{warehouseId}'  and catid in ({ string.Join(",", categoryIds) }) and compid in ({ string.Join(",", companyIds) }) and brandid in ({ string.Join(",", brandIds) })  group by orderid order by orderid desc";

                ElasticSqlHelper<DataContracts.External.LastPOOrderData> elasticSqlHelperData = new ElasticSqlHelper<DataContracts.External.LastPOOrderData>();

                var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(query)).ToList());

                if (orderdetails != null && orderdetails.Any())
                {
                    var orderIdList = orderdetails.Select(x => x.OrderId).ToList();
                    List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                    string queryItem = $" SELECT itemnumber from skorderdata_{AppConstants.Environment} where custid={customerId} and whid='{warehouseId}'  and catid in ({ string.Join(",", categoryIds) }) and compid in ({ string.Join(",", companyIds) }) and brandid in ({ string.Join(",", brandIds) }) and orderid in ({ string.Join(",", orderIdList) })  group by itemnumber";

                    ElasticSqlHelper<DataContracts.External.LastPOOrderItemNumberData> elasticSqlHelperDataNew = new ElasticSqlHelper<DataContracts.External.LastPOOrderItemNumberData>();

                    var OrderItemDetails = AsyncContext.Run(async () => (await elasticSqlHelperDataNew.GetListAsync(queryItem)).ToList());

                    var ItemNumberList = OrderItemDetails.Select(x => x.ItemNumber).ToList();

                    ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                    var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesCatelogElasticData(warehouseId, StoreCategorySubCategoryBrands, ItemNumberList, "", -1, -1, (skip * take), take, "DESC", true, IncentiveClassificationList));
                    ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                    itemResponseDc.TotalItem = data.TotalItem;

                    var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                    List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                    if (ItemData != null && ItemData.Any())
                    {
                        ItemMasterManager itemMasterManager = new ItemMasterManager();
                        itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());

                    }
                    BackendOrderController backendOrderController = new BackendOrderController();
                    foreach (var it in ItemData)
                    {
                        it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                        it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;
                        double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                        it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);

                        //Condition for offer end
                        if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                        {
                            if (it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                            else if (it.OfferCategory == 1)
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }

                        }
                        else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;

                        }

                        if (it.OfferCategory == 1)
                        {
                            if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                it.IsOffer = true;
                            else
                                it.IsOffer = false;
                        }

                        try
                        {
                            if (!it.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point
                                int? MP, PP;
                                double xPoint = xPointValue * 10;
                                //Customer (0.2 * 10=1)
                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = it.promoPerItems;
                                }
                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    it.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    it.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    it.dreamPoint = PP;
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                }
                            }
                            else { it.dreamPoint = 0; }

                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                            if (it.price > it.UnitPrice)
                            {
                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }
                        }
                        catch { }

                        if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                        it.Itemtype = 1;
                        ItemDataDCs.Add(it);
                    }
                }
            }
            itemResponseDc.ItemDataDCs = ItemDataDCs;
            return itemResponseDc;
        }

        [Route("GetOfferItemForAgentV2")]
        [HttpGet]
        public HttpResponseMessage GetOfferItemForAgentV2(int PeopleId, int WarehouseId, string lang, string IncentiveClassifications, int customerId = 0)
        {
            List<string> IncentiveClassificationList = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
            using (var context = new AuthContext())
            {
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                WROFFERTEM item = new WROFFERTEM();
                List<factoryItemdata> itemMasters = new List<factoryItemdata>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var IdDt1 = new DataTable();
                SqlParameter param1 = null;

                IdDt1 = new DataTable();
                IdDt1.Columns.Add("stringValue");
                foreach (var ic in IncentiveClassificationList)
                {
                    var dr = IdDt1.NewRow();
                    dr["stringValue"] = ic;
                    IdDt1.Rows.Add(dr);
                }
                param1 = new SqlParameter("Classification", IdDt1);
                param1.SqlDbType = SqlDbType.Structured;
                param1.TypeName = "dbo.stringValues";

                DataTable brandidDt = new DataTable();
                brandidDt.Columns.Add("IntValue");
                foreach (var ss in SubSubCats)
                {
                    DataRow dr = brandidDt.NewRow();
                    dr[0] = ss;
                    brandidDt.Rows.Add(dr);
                }
                var SubSubCategoryIds = new SqlParameter("SubSubCategoryIds", brandidDt);
                SubSubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubSubCategoryIds.TypeName = "dbo.IntValues";


                //var Subcatids = string.Join(",", SubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable SubCatidDt = new DataTable();
                SubCatidDt.Columns.Add("IntValue");
                foreach (var ss in SubCats)
                {
                    DataRow dr = SubCatidDt.NewRow();
                    dr[0] = ss;
                    SubCatidDt.Rows.Add(dr);
                }
                var SubCategoryIds = new SqlParameter("SubCategoryIds", SubCatidDt);
                SubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubCategoryIds.TypeName = "dbo.IntValues";

                //var Categoryid = string.Join(",", CatIds).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable CatIdDt = new DataTable();
                CatIdDt.Columns.Add("IntValue");
                foreach (var id in CatIds)
                {
                    DataRow dr = CatIdDt.NewRow();
                    dr[0] = id;
                    CatIdDt.Rows.Add(dr);
                }
                var CategoryIds = new SqlParameter("CategoryIds", CatIdDt);
                CategoryIds.SqlDbType = SqlDbType.Structured;
                CategoryIds.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetOfferItemForAgentV2]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));
                cmd.Parameters.Add(SubSubCategoryIds);
                cmd.Parameters.Add(SubCategoryIds);
                cmd.Parameters.Add(CategoryIds);
                cmd.Parameters.Add(param1);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                itemMasters = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<factoryItemdata>(reader).ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, WarehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    itemMasters = itemMasters.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion


                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (itemMasters != null && itemMasters.Any())
                {
                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = AsyncContext.Run(() => itemMasterManager.GetItemIncentiveClassification(WarehouseId, itemMasters.Select(s => s.ItemMultiMRPId).Distinct().ToList()));

                }


                BackendOrderController backendOrderController = new BackendOrderController();
                foreach (var it in itemMasters)
                {
                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;
                    if (customerId > 0)
                    {
                        double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                        it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice, it.WholeSalePrice ?? 0, it.TradePrice ?? 0,cprice);
                    }
                        
                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<factoryItemdata>();
                    }
                    try
                    {/// Dream Point Logic && Margin Point
                        if (!it.IsOffer)
                        {
                            /// Dream Point Logic && Margin Point
                            int? MP, PP;
                            double xPoint = xPointValue * 10;
                            //salesman 0.2=(0.02 * 10=0.2)
                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                            {
                                PP = 0;
                            }
                            else
                            {
                                PP = it.promoPerItems;
                            }
                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                            {
                                MP = 0;
                            }
                            else
                            {
                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            }
                            if (PP > 0 && MP > 0)
                            {
                                int? PP_MP = PP + MP;
                                it.dreamPoint = PP_MP;
                            }
                            else if (MP > 0)
                            {
                                it.dreamPoint = MP;
                            }
                            else if (PP > 0)
                            {
                                it.dreamPoint = PP;
                            }
                            else
                            {
                                it.dreamPoint = 0;
                            }

                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                    }
                    catch { }
                    if (lang.Trim() == "hi")
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
                    item.ItemMasters.Add(it);
                }
                if (itemMasters.Count() != 0)
                {
                    var res = new
                    {
                        offerData = itemMasters,
                        Status = true,
                        Message = "Success."
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    var res = new
                    {
                        offerData = itemMasters,
                        Status = false,
                        Message = "Item Not found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
        }

        [Route("SearchV2")]
        [HttpGet]
        public HttpResponseMessage SearchV2(string lang, string itemname, int PeopleId, int warehouseId, int customerId, string IncentiveClassifications)
        {
            using (var db = new AuthContext())
            {
                var ActiveCustomer = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                List<string> IncentiveClassificationList = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
                List<int> CatIds = StoreCategorySubCategoryBrands.Select(x => x.Categoryid).Distinct().ToList();
                List<int> SubCats = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
                List<int> SubSubCats = StoreCategorySubCategoryBrands.Select(x => x.BrandId).Distinct().ToList();
                WRSITEM item = new WRSITEM();


                if (db.Database.Connection.State != ConnectionState.Open)
                    db.Database.Connection.Open();

                var IdDt1 = new DataTable();
                SqlParameter param1 = null;

                IdDt1 = new DataTable();
                IdDt1.Columns.Add("stringValue");
                foreach (var ic in IncentiveClassificationList)
                {
                    var dr = IdDt1.NewRow();
                    dr["stringValue"] = ic;
                    IdDt1.Rows.Add(dr);
                }
                param1 = new SqlParameter("Classification", IdDt1);
                param1.SqlDbType = SqlDbType.Structured;
                param1.TypeName = "dbo.stringValues";

                DataTable brandidDt = new DataTable();
                brandidDt.Columns.Add("IntValue");
                foreach (var ss in SubSubCats)
                {
                    DataRow dr = brandidDt.NewRow();
                    dr[0] = ss;
                    brandidDt.Rows.Add(dr);
                }
                var SubSubCategoryIds = new SqlParameter("SubSubCategoryIds", brandidDt);
                SubSubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubSubCategoryIds.TypeName = "dbo.IntValues";


                //var Subcatids = string.Join(",", SubCats).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable SubCatidDt = new DataTable();
                SubCatidDt.Columns.Add("IntValue");
                foreach (var ss in SubCats)
                {
                    DataRow dr = SubCatidDt.NewRow();
                    dr[0] = ss;
                    SubCatidDt.Rows.Add(dr);
                }
                var SubCategoryIds = new SqlParameter("SubCategoryIds", SubCatidDt);
                SubCategoryIds.SqlDbType = SqlDbType.Structured;
                SubCategoryIds.TypeName = "dbo.IntValues";

                //var Categoryid = string.Join(",", CatIds).Split(',').Select(x => Convert.ToInt32(x)).ToList();
                DataTable CatIdDt = new DataTable();
                CatIdDt.Columns.Add("IntValue");
                foreach (var id in CatIds)
                {
                    DataRow dr = CatIdDt.NewRow();
                    dr[0] = id;
                    CatIdDt.Rows.Add(dr);
                }
                var CategoryIds = new SqlParameter("CategoryIds", CatIdDt);
                CategoryIds.SqlDbType = SqlDbType.Structured;
                CategoryIds.TypeName = "dbo.IntValues";

                var cmd = db.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSearchItemsV2]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@ItemName", itemname));
                cmd.Parameters.Add(SubSubCategoryIds);
                cmd.Parameters.Add(SubCategoryIds);
                cmd.Parameters.Add(CategoryIds);
                cmd.Parameters.Add(param1);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var newdata = ((IObjectContextAdapter)db)
                .ObjectContext
                .Translate<factoryItemdata>(reader).ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                #region block Barnd
                var custtype = 4;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion

                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();

                if (newdata != null && newdata.Any())
                {
                    var itemmultiMrpIds = newdata.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    List<DataContracts.External.orderMrpData> orderdetails = new List<DataContracts.External.orderMrpData>();
                    ParallelLoopResult parellelResult = Parallel.ForEach(itemmultiMrpIds, (mrpid) =>
                    {
                        //    foreach (var mrpid in itemmultiMrpIds)
                        //{
                        string query = $"SELECT top 1 itemmultimrpid,createddate createddate, ordqty Qty from skorderdata_{AppConstants.Environment} where itemmultimrpid in ({ mrpid })   and whid={warehouseId} and custid={customerId}  order by createddate desc";

                        ElasticSqlHelper<DataContracts.External.orderMrpData> elasticSqlHelper = new ElasticSqlHelper<DataContracts.External.orderMrpData>();
                        var order = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).FirstOrDefault());
                        if (order != null)
                            orderdetails.Add(order);
                    });

                    ItemMasterManager itemMasterManager = new ItemMasterManager();
                    itemsIncentiveClassification = AsyncContext.Run(() => itemMasterManager.GetItemIncentiveClassification(warehouseId, newdata.Select(s => s.ItemMultiMRPId).Distinct().ToList()));

                    var itemMultiMRPIds = newdata.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                    using (var context = new AuthContext())
                    {
                        ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, warehouseId, context);
                    }
                    BackendOrderController backendOrderController = new BackendOrderController();
                    foreach (var itemdata in newdata)
                    {
                        double cprice = backendOrderController.GetConsumerPrice(db, itemdata.ItemMultiMRPId, itemdata.price, itemdata.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                        itemdata.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, itemdata.UnitPrice, itemdata.WholeSalePrice ?? 0, itemdata.TradePrice ?? 0,cprice);
                        if (orderdetails != null && orderdetails.Any(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId))
                        {
                            itemdata.LastOrderDate = orderdetails.Where(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate;
                            itemdata.LastOrderQty = orderdetails.Where(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().Qty;
                            itemdata.LastOrderDays = (DateTime.Today - itemdata.LastOrderDate).Value.Days;
                        }

                        if (itemdata.price > itemdata.UnitPrice)
                        {
                            itemdata.marginPoint = itemdata.UnitPrice > 0 ? (((itemdata.price - itemdata.UnitPrice) * 100) / itemdata.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                            if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId && x.PTR > 0))
                            {
                                var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == itemdata.ItemMultiMRPId);
                                var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                                var UPMRPMargin = itemdata.marginPoint.Value;
                                if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                    itemdata.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                            }

                        }
                        else
                        {
                            itemdata.marginPoint = 0;
                        }
                    }
                }


                foreach (var it in newdata)
                {
                    it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                    it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;

                    if (!it.OfferId.HasValue || it.OfferId.Value == 0)
                    {
                        it.IsOffer = false;
                    }
                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                        it.IsOffer = true;
                    else
                        it.IsOffer = false;

                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                    {
                        if (it.OfferCategory == 1)
                        {
                            it.IsOffer = false;
                            it.OfferCategory = 0;
                        }
                    }
                    if (it.OfferCategory == 2)
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                    }



                    if (item.ItemMasters == null)
                    {
                        item.ItemMasters = new List<factoryItemdata>();
                    }
                    try
                    {/// Dream Point Logic && Margin Point
                        int? MP, PP;
                        double xPoint = xPointValue * 10;
                        //Customer (0.02 * 10=0.2)
                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                        {
                            PP = 0;
                        }
                        else
                        {
                            PP = it.promoPerItems;
                        }
                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                        {
                            MP = 0;
                        }
                        else
                        {
                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                        }
                        if (PP > 0 && MP > 0)
                        {
                            int? PP_MP = PP + MP;
                            it.dreamPoint = PP_MP;
                        }
                        else if (MP > 0)
                        {
                            it.dreamPoint = MP;
                        }
                        else if (PP > 0)
                        {
                            it.dreamPoint = PP;
                        }
                        else
                        {
                            it.dreamPoint = 0;
                        }
                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                        if (it.price > it.UnitPrice)
                        {
                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }
                        //// by sudhir 22-08-2019
                        if (lang.Trim() == "hi")
                        {
                            if (it.HindiName != null)
                            {
                                if (it.IsSensitive == true)
                                {
                                    if (it.IsSensitiveMRP == false)
                                    {
                                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
                                    }
                                    else
                                    {
                                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
                                    }
                                }
                                else
                                {
                                    it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
                                }
                            }
                            else
                            {
                                it.itemname = it.itemBaseName + " " + it.price + " MRP ";
                            }
                        }
                        //end
                    }
                    catch { }

                    if (it.OfferType != "FlashDeal")
                    {
                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            it.IsOffer = true;
                        else
                            it.IsOffer = false;
                    }

                    item.ItemMasters.Add(it);
                }
                if (itemname != null || itemname != "")
                {
                    BackgroundTaskManager.Run(() =>
                    {
                        MongoDbHelper<ExecutiveProductSearch> mongoDbHelper = new MongoDbHelper<ExecutiveProductSearch>();
                        ExecutiveProductSearch executiveProductSearch = new ExecutiveProductSearch
                        {
                            CreatedDate = indianTime,
                            PeopleId = PeopleId,
                            keyword = itemname,
                            IsDeleted = false
                        };
                        mongoDbHelper.Insert(executiveProductSearch);
                    });
                }
                if (item.ItemMasters != null)
                {
                    item.Message = true;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }
                else
                {
                    item.Message = false;
                    return Request.CreateResponse(HttpStatusCode.OK, item);
                }

            }
        }
        [Route("GetCatelogV2")]
        [HttpGet]
        public CalelogDc GetCatelogV2(int PeopleId, int warehouseid, string lang, string IncentiveClassifications)
        {
            List<string> IncentiveClassificationList = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
            CalelogDc objtosend = new CalelogDc();
            objtosend.SalesCategories = new List<SalesCategory>();
            objtosend.SalesCompanies = new List<SalesCompany>();
            objtosend.SalesBrands = new List<SalesBrand>();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);

            #region block Barnd
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            var custtype = 4;
            var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseid);
            if (blockBarnds != null && blockBarnds.Any())
            {
                StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
            }
            #endregion
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any())
                {
                    var IdDt = new DataTable();
                    SqlParameter param = null;

                    IdDt = new DataTable();
                    IdDt.Columns.Add("categoryId");
                    IdDt.Columns.Add("companyId");
                    IdDt.Columns.Add("brandId");
                    foreach (var item in StoreCategorySubCategoryBrands)
                    {
                        var dr = IdDt.NewRow();
                        dr["categoryId"] = item.Categoryid;
                        dr["companyId"] = item.SubCategoryId;
                        dr["brandId"] = item.BrandId;
                        IdDt.Rows.Add(dr);
                    }

                    param = new SqlParameter("CatCompanyBrand", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.CatCompanyBrand";

                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetCatCompanyBrandforSalesApp]";
                    cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseid));
                    cmd.Parameters.Add(param);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    objtosend.SalesCategories = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<SalesCategory>(reader).ToList();
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        objtosend.SalesCompanies = ((IObjectContextAdapter)context)
                                                    .ObjectContext
                                                    .Translate<SalesCompany>(reader).ToList();
                    }
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        objtosend.SalesBrands = ((IObjectContextAdapter)context)
                                                    .ObjectContext
                                                    .Translate<SalesBrand>(reader).ToList();
                    }

                }
            }

            if (lang == "hi")
            {
                foreach (var kk in objtosend.SalesCategories)
                {
                    if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                    {
                        kk.CategoryName = kk.HindiName;
                    }
                }

                foreach (var kk in objtosend.SalesCompanies)
                {
                    if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                    {
                        kk.SubcategoryName = kk.HindiName;
                    }
                }

                foreach (var kk in objtosend.SalesBrands)
                {
                    if (kk.HindiName != null && kk.HindiName != "{nan}" && kk.HindiName != "")
                    {
                        kk.SubsubcategoryName = kk.HindiName;
                    }
                }
            }
            objtosend.SalesCategories = objtosend.SalesCategories.OrderBy(x => x.CategoryName).ToList();
            objtosend.SalesBrands = objtosend.SalesBrands.OrderBy(x => x.SubsubcategoryName).ToList();
            objtosend.BrandCompanies = objtosend.SalesCompanies.Any() ? objtosend.SalesCompanies.GroupBy(x => x.SubCategoryId).Select(x => new SalesCompany
            {
                Categoryid = 0,
                HindiName = x.FirstOrDefault().HindiName,
                itemcount = x.Sum(y => y.itemcount),
                LogoUrl = x.FirstOrDefault().LogoUrl,
                Sequence = x.FirstOrDefault().Sequence,
                SubCategoryId = x.Key,
                SubcategoryName = x.FirstOrDefault().SubcategoryName
            }).OrderBy(x => x.Sequence).OrderBy(x => x.SubcategoryName).ToList() : new List<SalesCompany>();

            objtosend.Brands = objtosend.SalesBrands.Any() ? objtosend.SalesBrands.GroupBy(x => new { x.SubsubCategoryid, x.SubCategoryId }).Select(x => new SalesBrand
            {
                Categoryid = 0,
                HindiName = x.FirstOrDefault().HindiName,
                itemcount = x.Sum(y => y.itemcount),
                LogoUrl = x.FirstOrDefault().LogoUrl,
                SubCategoryId = x.Key.SubCategoryId,
                SubsubCategoryid = x.Key.SubsubCategoryid,
                SubsubcategoryName = x.FirstOrDefault().SubsubcategoryName
            }).OrderBy(x => x.SubsubcategoryName).ToList() : new List<SalesBrand>();



            return objtosend;
        }
        #endregion

        [Route("GetQuadrantItemList")]
        [HttpGet]
        public async Task<DataContracts.External.SalesItemResponseDc> GetQuadrantItemList(int PeopleId, int warehouseId, int skip, int take, string lang, string IncentiveClassifications)
        {
            List<string> IncentiveClassification = new List<string>();
            IncentiveClassification = IncentiveClassifications != null && IncentiveClassifications != "" ? IncentiveClassifications.Split(',').ToList() : new List<string>();
            skip = skip / take;
            var itemResponseDc = new DataContracts.External.SalesItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.SalesAppItemDataDC>() };

            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStoresNewSales(PeopleId);

            #region block Barnd
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            var custtype = 4;
            var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
            if (blockBarnds != null && blockBarnds.Any())
            {
                StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => !(blockBarnds.Select(y => y.CatId + " " + y.SubCatId + " " + y.SubSubCatId).Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.BrandId))).ToList();
            }
            #endregion

            var PromotionalStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands;

            //var SuggestedStoreCategorySubCategoryBrands = companyId==0 && brandId==0 ? null:  StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
            //                                                                 && (companyId > 0 && brandId==0 && companyId != x.SubCategoryId)
            //                                                                 && (companyId >=0 && brandId >0 && x.BrandId != brandId)).ToList();

            var SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands;
            //if (companyId == 0 && brandId == 0)
            //{
            //    int suggestedCategoryId = await GetSuggestedCategoryId(categoryId);
            //    if (suggestedCategoryId > 0)
            //        SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == suggestedCategoryId).ToList();
            //    else
            //        SuggestedStoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            //    //PromotionalStoreCategorySubCategoryBrands = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
            //}
            //else if (companyId > 0 && brandId == 0)
            //    SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
            //                                                                  && (x.SubCategoryId != companyId)).ToList();
            //else if (companyId == 0 && brandId > 0)
            //    SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
            //                                                                  && (x.BrandId != brandId)).ToList();
            //else if (companyId > 0 && brandId > 0)
            //    SuggestedStoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
            //                                                                  && (x.SubCategoryId == companyId && x.BrandId != brandId)).ToList();

            //StoreCategorySubCategoryBrands = StoreCategorySubCategoryBrands.Where(x => x.Categoryid == categoryId
            //                                                                && ((brandId > 0 && x.BrandId == brandId) || (brandId == 0 && x.BrandId == x.BrandId))
            //                                                                && ((companyId > 0 && x.SubCategoryId == companyId) || (companyId == 0 && x.SubCategoryId == x.SubCategoryId))).ToList();


            using (var context = new AuthContext())
            {
                #region City wise configuration working
                List<CatelogConfig> categlogconfigs = new List<CatelogConfig>();
                var cityId = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId && x.active && !x.Deleted)?.Cityid;
                var Cityconfigs = context.CatelogConfigs.Where(x => x.IsActive && x.CityId == cityId).ToList();

                if (Cityconfigs != null && Cityconfigs.Any())
                {
                    categlogconfigs = Cityconfigs.ToList();
                }
                else
                {
                    categlogconfigs = context.CatelogConfigs.Where(x => x.IsActive && x.CityId == 0).ToList();
                }
                #endregion
            }

            List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any())
            {
                using (var context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();


                    List<DataContracts.External.ItemDataDC> ItemData = new List<DataContracts.External.ItemDataDC>();
                    if (!Convert.ToBoolean(ConfigurationManager.AppSettings["salesAppElasticData"]))
                    {
                        var IdDt = new DataTable();
                        SqlParameter param = null;

                        IdDt = new DataTable();
                        IdDt.Columns.Add("categoryId");
                        IdDt.Columns.Add("companyId");
                        IdDt.Columns.Add("brandId");
                        foreach (var item in StoreCategorySubCategoryBrands)
                        {
                            var dr = IdDt.NewRow();
                            dr["categoryId"] = item.Categoryid;
                            dr["companyId"] = item.SubCategoryId;
                            dr["brandId"] = item.BrandId;
                            IdDt.Rows.Add(dr);
                        }

                        param = new SqlParameter("CatCompanyBrand", IdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.CatCompanyBrand";

                        var IcDt = new DataTable();
                        SqlParameter param1 = null;

                        IcDt = new DataTable();
                        IcDt.Columns.Add("stringValue");
                        if (IncentiveClassification.Count > 0)
                        {
                            foreach (var item in IncentiveClassification)
                            {
                                var dr = IcDt.NewRow();
                                dr["stringValue"] = item;
                                IcDt.Rows.Add(dr);
                            }
                        }

                        param1 = new SqlParameter("Classification", IcDt);
                        param1.SqlDbType = SqlDbType.Structured;
                        param1.TypeName = "dbo.stringValues";

                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetQuadrantItemforSalesApp]";
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(param);
                        cmd.Parameters.Add(param1);
                        cmd.Parameters.Add(new SqlParameter("@skip", skip));
                        cmd.Parameters.Add(new SqlParameter("@take", take));
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        ItemData = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<DataContracts.External.ItemDataDC>(reader).ToList();
                        reader.NextResult();
                        if (reader.Read())
                        {
                            itemResponseDc.TotalItem = Convert.ToInt32(reader["itemCount"]);
                        }
                    }
                    else
                    {
                        ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
                        var data = AsyncContext.Run(() => elasticSalesAppClusterItem.GetSalesBaseCatelogElasticData(warehouseId, StoreCategorySubCategoryBrands, null, "", skip, take, "ASC", true, IncentiveClassification));
                        ItemData = Mapper.Map(data.ItemMasters).ToANew<List<DataContracts.External.ItemDataDC>>();
                        itemResponseDc.TotalItem = data.TotalItem;
                    }

                    var offerids = ItemData.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                    var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Sales App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();



                    if (ItemData != null && ItemData.Any())
                    {

                        List<ItemIncentiveClassification> itemsIncentiveClassification = new List<ItemIncentiveClassification>();
                        ItemMasterManager itemMasterManager = new ItemMasterManager();
                        itemsIncentiveClassification = await itemMasterManager.GetItemIncentiveClassification(warehouseId, ItemData.Select(s => s.ItemMultiMRPId).Distinct().ToList());


                        foreach (var it in ItemData)
                        {
                            it.Classification = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.Classification;
                            it.BackgroundRgbColor = itemsIncentiveClassification.FirstOrDefault(s => s.ItemMultiMrpId == it.ItemMultiMRPId)?.BackgroundRgbColor;


                            //Condition for offer end
                            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
                            {
                                if (it.OfferCategory == 2)
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                                else if (it.OfferCategory == 1)
                                {
                                    it.IsOffer = false;
                                    it.OfferCategory = 0;
                                }

                            }
                            else if ((it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;

                            }

                            if (it.OfferCategory == 1)
                            {
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                            try
                            {
                                if (!it.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point
                                    int? MP, PP;
                                    double xPoint = xPointValue * 10;
                                    //Customer (0.2 * 10=1)
                                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = it.promoPerItems;
                                    }
                                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        it.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        it.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        it.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        it.dreamPoint = 0;
                                    }
                                }
                                else { it.dreamPoint = 0; }

                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                if (it.price > it.UnitPrice)
                                {
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                }
                                else
                                {
                                    it.marginPoint = 0;
                                }
                            }
                            catch { }

                            if (it.HindiName != null && !string.IsNullOrEmpty(lang) && lang == "hi")
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

                            it.Itemtype = 1;
                            ItemDataDCs.Add(it);
                        }
                    }
                    //--If Base Item is null then remove all suggested and Promotional Item
                    /*else
                    {
                        ItemDataDCs = new List<DataContracts.External.ItemDataDC>();
                    }*/

                }
            }

            //});

            //taskList.Add(taskbaseItem);

            if (ItemDataDCs != null && ItemDataDCs.Any())
            {
                var enddate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                var startDate = DateTime.Now.AddMonths(-9).Date.ToString("yyyy-MM-dd");
                var itemmultiMrpIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();


                string query = $"SELECT  itemmultimrpid,max(createddate) createddate, first(ordqty) Qty from skorderdata_{AppConstants.Environment} where itemmultimrpid in ({ string.Join(",", itemmultiMrpIds) })   and whid={warehouseId}  and createddate>='{startDate}' and createddate <= '{enddate}'  group by itemmultimrpid";

                ElasticSqlHelper<DataContracts.External.orderMrpData> elasticSqlHelper = new ElasticSqlHelper<DataContracts.External.orderMrpData>();

                var orderdetails = AsyncContext.Run(async () => (await elasticSqlHelper.GetListAsync(query)).ToList());


                /*var itemmultiMrpIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                var enddate = DateTime.Now;
                var startDate = enddate.AddMonths(-9);
                var orderPredicate = PredicateBuilder.New<MongoOrderMaster>(x => x.Status != "Inactive" && x.Status != "Dummy Order Cancelled" && !x.Deleted
                                                    && x.CustomerId == customerId && x.WarehouseId == warehouseId
                                                    // && x.orderDetails.Any(y => itemmultiMrpIds.Contains(y.ItemMultiMRPId)
                                                    // && x.CreatedDate >= startDate && x.CreatedDate <= enddate
                                                    );
                var ordercollection = mongoDbHelper.mongoDatabase.GetCollection<MongoOrderMaster>("MongoOrderMaster").AsQueryable();
                var orderdetails = ordercollection.Where(orderPredicate)
                                    .SelectMany(t => t.orderDetails, (t, a) => new
                                    {
                                        CreatedDate = t.CreatedDate,
                                        ItemMultiMRPId = a.ItemMultiMRPId,
                                        Qty = a.qty
                                    }).Where(x => itemmultiMrpIds.Contains(x.ItemMultiMRPId))
                                    .ToList();
    */
                var itemMultiMRPIds = ItemDataDCs.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                List<ItemScheme> ItemSchemes = new List<ItemScheme>();
                using (var context = new AuthContext())
                {
                    ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, warehouseId, context);
                }
                foreach (var item in ItemDataDCs)
                {
                    if (orderdetails != null && orderdetails.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId))
                    {
                        item.LastOrderDate = orderdetails.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate;
                        item.LastOrderQty = orderdetails.Where(x => x.ItemMultiMRPId == item.ItemMultiMRPId).OrderByDescending(x => x.CreatedDate).FirstOrDefault().Qty;
                        item.LastOrderDays = (DateTime.Today - item.LastOrderDate).Value.Days;
                    }
                    if (item.price > item.UnitPrice)
                    {
                        item.marginPoint = item.UnitPrice > 0 ? (((item.price - item.UnitPrice) * 100) / item.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                        if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.PTR > 0))
                        {
                            var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId);
                            var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                            var UPMRPMargin = item.marginPoint.Value;
                            if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                item.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                        }

                    }
                    else
                    {
                        item.marginPoint = 0;
                    }
                }

                itemResponseDc.ItemDataDCs = ItemDataDCs.GroupBy(x => new { x.ItemNumber, x.Itemtype }).Select(x => new DataContracts.External.SalesAppItemDataDC
                {
                    BaseCategoryId = x.FirstOrDefault().BaseCategoryId,
                    BillLimitQty = x.FirstOrDefault().BillLimitQty,
                    Categoryid = x.FirstOrDefault().Categoryid,
                    CompanyId = x.FirstOrDefault().CompanyId,
                    dreamPoint = x.FirstOrDefault().dreamPoint,
                    HindiName = x.FirstOrDefault().HindiName,
                    IsItemLimit = x.FirstOrDefault().IsItemLimit,
                    IsOffer = x.FirstOrDefault().IsOffer,
                    ItemId = x.FirstOrDefault().ItemId,
                    ItemlimitQty = x.FirstOrDefault().ItemlimitQty,
                    ItemMultiMRPId = x.FirstOrDefault().ItemMultiMRPId,
                    itemname = x.FirstOrDefault().itemname,
                    ItemNumber = x.FirstOrDefault().ItemNumber,
                    Itemtype = x.FirstOrDefault().Itemtype,
                    LastOrderDate = x.FirstOrDefault().LastOrderDate,
                    LastOrderDays = x.FirstOrDefault().LastOrderDays,
                    LastOrderQty = x.FirstOrDefault().LastOrderQty,
                    LogoUrl = x.FirstOrDefault().LogoUrl,
                    marginPoint = x.FirstOrDefault().marginPoint,
                    MinOrderQty = x.FirstOrDefault().MinOrderQty,
                    OfferCategory = x.FirstOrDefault().OfferCategory,
                    OfferFreeItemId = x.FirstOrDefault().OfferFreeItemId,
                    OfferFreeItemImage = x.FirstOrDefault().OfferFreeItemImage,
                    OfferFreeItemName = x.FirstOrDefault().OfferFreeItemName,
                    OfferFreeItemQuantity = x.FirstOrDefault().OfferFreeItemQuantity,
                    OfferId = x.FirstOrDefault().OfferId,
                    OfferMinimumQty = x.FirstOrDefault().OfferMinimumQty,
                    OfferType = x.FirstOrDefault().OfferType,
                    OfferWalletPoint = x.FirstOrDefault().OfferWalletPoint,
                    price = x.FirstOrDefault().price,
                    Sequence = x.FirstOrDefault().Sequence,
                    SubCategoryId = x.FirstOrDefault().SubCategoryId,
                    SubsubCategoryid = x.FirstOrDefault().SubsubCategoryid,
                    UnitPrice = x.FirstOrDefault().UnitPrice,
                    WarehouseId = x.FirstOrDefault().WarehouseId,
                    Scheme = x.FirstOrDefault().Scheme,
                    Classification = x.FirstOrDefault().Classification,
                    BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                    moqList = x.Count() > 1 ? x.Select(y => new DataContracts.External.SalesAppItemDataDC
                    {
                        isChecked = (y.ItemMultiMRPId == x.FirstOrDefault().ItemMultiMRPId && y.MinOrderQty == x.FirstOrDefault().MinOrderQty),
                        Scheme = y.Scheme,
                        BaseCategoryId = y.BaseCategoryId,
                        BillLimitQty = y.BillLimitQty,
                        Categoryid = y.Categoryid,
                        CompanyId = y.CompanyId,
                        dreamPoint = y.dreamPoint,
                        HindiName = y.HindiName,
                        IsItemLimit = y.IsItemLimit,
                        IsOffer = y.IsOffer,
                        ItemId = y.ItemId,
                        ItemlimitQty = y.ItemlimitQty,
                        ItemMultiMRPId = y.ItemMultiMRPId,
                        itemname = y.itemname,
                        ItemNumber = y.ItemNumber,
                        Itemtype = y.Itemtype,
                        LastOrderDate = y.LastOrderDate,
                        LastOrderDays = y.LastOrderDays,
                        LastOrderQty = y.LastOrderQty,
                        LogoUrl = y.LogoUrl,
                        marginPoint = y.marginPoint,
                        MinOrderQty = y.MinOrderQty,
                        OfferCategory = y.OfferCategory,
                        OfferFreeItemId = y.OfferFreeItemId,
                        OfferFreeItemImage = y.OfferFreeItemImage,
                        OfferFreeItemName = y.OfferFreeItemName,
                        OfferFreeItemQuantity = y.OfferFreeItemQuantity,
                        OfferId = y.OfferId,
                        OfferMinimumQty = y.OfferMinimumQty,
                        OfferType = y.OfferType,
                        OfferWalletPoint = y.OfferWalletPoint,
                        price = y.price,
                        Sequence = y.Sequence,
                        SubCategoryId = y.SubCategoryId,
                        SubsubCategoryid = y.SubsubCategoryid,
                        UnitPrice = y.UnitPrice,
                        WarehouseId = y.WarehouseId,
                        Classification = x.FirstOrDefault().Classification,
                        BackgroundRgbColor = x.FirstOrDefault().BackgroundRgbColor,
                    }).ToList() : new List<DataContracts.External.SalesAppItemDataDC>()
                }).OrderBy(x => x.Sequence).ToList();

            }
            return itemResponseDc;
        }


        #region Warehouse Quadrant Margin Upload APIs By Kapil
        [Route("GetWarehouseQuadrantByCustomerType")]
        
        [HttpPost]
        public List<SearchWarehouseQuadrantCustomerTypeDC> GetWarehouseQuadrantByCustomerType(WarehouseQuadrantCustomerTypeDC Obj)
        {
            List<SearchWarehouseQuadrantCustomerTypeDC> WarehouseQuadrantCustomerTypeData = new List<SearchWarehouseQuadrantCustomerTypeDC>();

            if (Obj != null)
            {
                using (var context = new AuthContext())
                {
                    var IdDt = new DataTable();
                    IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");

                    foreach (var item in Obj.WarehouseIDs)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@WarehouseIDs", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.intValues";

                    var param2 = new SqlParameter("@Quadrant", Obj.Quadrant != null ? (object)Obj.Quadrant : (DBNull.Value));
                    var param3 = new SqlParameter("@CustomerType", Obj.CustomerType != null ? (object)Obj.CustomerType : (DBNull.Value));
                    var param4 = new SqlParameter("@skip", Obj.skip);
                    var param5 = new SqlParameter("@take", Obj.take);


                    IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in Obj.StoreIDs)
                    {
                        var dr = IdDt.NewRow(); 
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param7 = new SqlParameter("@StoreIDs", IdDt);
                    param7.SqlDbType = SqlDbType.Structured;
                    param7.TypeName = "dbo.IntValues";
                    WarehouseQuadrantCustomerTypeData = context.Database.SqlQuery<SearchWarehouseQuadrantCustomerTypeDC>("Exec WarehouseQuadrantMarginSearch  @WarehouseIDs,@StoreIDs,@Quadrant,@CustomerType,@skip,@take", param, param7, param2, param3, param4, param5).ToList();

                }
            }
            return WarehouseQuadrantCustomerTypeData;
        }

        [Route("WarehouseQuadrantMarginExport")]
        [HttpPost]
        public List<WarehouseQuadrantMarginExport> WarehouseQuadrantMarginExport(WarehouseQuadrantCustomerTypeDC Obj)
        {
            List<WarehouseQuadrantMarginExport> WarehouseQuadrantMarginExport = new List<WarehouseQuadrantMarginExport>();

            if (Obj != null)
            {
                using (var context = new AuthContext())
                {
                    var IdDt = new DataTable();
                    IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");

                    foreach (var item in Obj.WarehouseIDs)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@WarehouseIDs", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.intValues";

                    IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in Obj.StoreIDs)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param4 = new SqlParameter("@StoreIDs", IdDt);
                    param4.SqlDbType = SqlDbType.Structured;
                    param4.TypeName = "dbo.intValues";

                    var param2 = new SqlParameter("@Quadrant", Obj.Quadrant != null ? (object)Obj.Quadrant : (DBNull.Value));
                    var param3 = new SqlParameter("@CustomerType", Obj.CustomerType != null ? (object)Obj.CustomerType : (DBNull.Value));

                    WarehouseQuadrantMarginExport = context.Database.SqlQuery<WarehouseQuadrantMarginExport>("Exec WarehouseQuadrantMarginExport  @WarehouseIDs,@StoreIDs,@Quadrant,@CustomerType", param,param4, param2, param3).ToList();
                }
            }
            return WarehouseQuadrantMarginExport;
        }


        [Route("UpdateWarehouseQuadrantByCustomerType")]
        [HttpPost]
        public bool UpdateWarehouseQuadrantByCustomerType(WarehouseQuadrantCustomerTypeDC Obj)
        {
            bool result = false;
            if (Obj != null)
            {
                using (var context = new AuthContext())
                {
                    var IdDt = new DataTable();
                    IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");

                    foreach (var item in Obj.WarehouseIDs)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@WarehouseIDs", IdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.intValues";

                    var param2 = new SqlParameter("@Quadrant", Obj.Quadrant != null ? (object)Obj.Quadrant : (DBNull.Value));
                    var param3 = new SqlParameter("@CustomerType", Obj.CustomerType != null ? (object)Obj.CustomerType : (DBNull.Value));
                    var param4 = new SqlParameter("@Margin", Obj.Margin);

                    IdDt = new DataTable();
                    IdDt.Columns.Add("IntValue");
                    foreach (var item in Obj.StoreIDs)
                    {
                        var dr = IdDt.NewRow();
                        dr["IntValue"] = item;
                        IdDt.Rows.Add(dr);
                    }
                    var param5 = new SqlParameter("@storeIds", IdDt);
                    param5.SqlDbType = SqlDbType.Structured;
                    param5.TypeName = "dbo.IntValues";
                    var res = context.Database.ExecuteSqlCommand("Exec UpdateWarehouseQuadrantMargin  @WarehouseIDs,@storeIds,@Quadrant,@CustomerType,@Margin", param, param5, param2, param3, param4);
                    if (res > 0)
                    {
                        return result = true;
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            else
            {
                return result;
            }
        }

        [Route("UpdateStoreQuadrantMargin")]
        [HttpGet]
        public string UpdateStoreQuadrantMargin(long id, float Margin)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string result = "";
            if (id > 0 && Margin > 0)
            {
                using (var context = new AuthContext())
                {
                    WarehouseBasedQuadarant obj = new WarehouseBasedQuadarant();
                    var Data = context.WarehouseBasedQuadarants.Where(x => x.Id == id).FirstOrDefault();
                    if (Data != null)
                    {
                        Data.IsActive = false;
                        Data.IsDeleted = true;
                        context.Entry(Data).State = EntityState.Modified;

                        obj.MinMarginPercent = Margin;
                        obj.WarehouseId = Data.WarehouseId;
                        obj.CreatedDate = DateTime.Now;
                        obj.ModifiedDate = null;
                        obj.IsActive = true;
                        obj.IsDeleted = false;
                        obj.CreatedBy = userid;
                        obj.ModifiedBy = 0;
                        obj.ClassificationMasterId = Data.ClassificationMasterId;
                        obj.CustomerType = Data.CustomerType;
                        context.WarehouseBasedQuadarants.Add(obj);

                        if (context.Commit() > 0)
                        {
                            result = "Updated Succesfully";
                        }
                        else
                        {
                            result = "Something went wrong";
                        }
                    }
                    else
                    {
                        result = "Data not found";
                    }

                }
            }
            else
            {
                result = "Not Updated";
            }
            return result;
        }

        [Route("getStoreQuadrantWise")]
        [HttpGet]

        public  List<StoreListDTO> getStoreQuadrantWise(string QuadrantId)
        {
            List<StoreListDTO> StoreList = new List<StoreListDTO>();
            using (var context = new AuthContext())
            {
                var QuadrantIds = new SqlParameter("@QuadrantId", QuadrantId);
                StoreList = context.Database.SqlQuery<StoreListDTO>("exec getStoreQuadrantWise @QuadrantId", QuadrantIds).ToList();
            }
                return StoreList;
        }
        #endregion

    }


    public class Spelling
    {
        private Dictionary<String, int> _dictionary = new Dictionary<String, int>();
        private static Regex _wordRegex = new Regex("[a-z]+", RegexOptions.Compiled);

        public Spelling()
        {
            var filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/Templates"), "SearchItemWord.txt");

            string fileContent = File.ReadAllText(filePath);
            List<string> wordList = fileContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var word in wordList)
            {
                string trimmedWord = word.Trim().ToLower();
                if (_wordRegex.IsMatch(trimmedWord))
                {
                    if (_dictionary.ContainsKey(trimmedWord))
                        _dictionary[trimmedWord]++;
                    else
                        _dictionary.Add(trimmedWord, 1);
                }
            }
        }

        public string Correct(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            word = word.ToLower();

            // known()
            if (_dictionary.ContainsKey(word))
                return word;

            List<String> list = Edits(word);
            Dictionary<string, int> candidates = new Dictionary<string, int>();

            foreach (string wordVariation in list)
            {
                if (_dictionary.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
                    candidates.Add(wordVariation, _dictionary[wordVariation]);
            }

            if (candidates.Count > 0)
                return candidates.OrderByDescending(x => x.Value).First().Key;

            // known_edits2()
            foreach (string item in list)
            {
                foreach (string wordVariation in Edits(item))
                {
                    if (_dictionary.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
                        candidates.Add(wordVariation, _dictionary[wordVariation]);
                }
            }

            return (candidates.Count > 0) ? candidates.OrderByDescending(x => x.Value).First().Key : word;
        }

        private List<string> Edits(string word)
        {
            var splits = new List<Tuple<string, string>>();
            var transposes = new List<string>();
            var deletes = new List<string>();
            var replaces = new List<string>();
            var inserts = new List<string>();

            // Splits
            for (int i = 0; i < word.Length; i++)
            {
                var tuple = new Tuple<string, string>(word.Substring(0, i), word.Substring(i));
                splits.Add(tuple);
            }

            // Deletes
            for (int i = 0; i < splits.Count; i++)
            {
                string a = splits[i].Item1;
                string b = splits[i].Item2;
                if (!string.IsNullOrEmpty(b))
                {
                    deletes.Add(a + b.Substring(1));
                }
            }

            // Transposes
            for (int i = 0; i < splits.Count; i++)
            {
                string a = splits[i].Item1;
                string b = splits[i].Item2;
                if (b.Length > 1)
                {
                    transposes.Add(a + b[1] + b[0] + b.Substring(2));
                }
            }

            // Replaces
            for (int i = 0; i < splits.Count; i++)
            {
                string a = splits[i].Item1;
                string b = splits[i].Item2;
                if (!string.IsNullOrEmpty(b))
                {
                    for (char c = 'a'; c <= 'z'; c++)
                    {
                        replaces.Add(a + c + b.Substring(1));
                    }
                }
            }

            // Inserts
            for (int i = 0; i < splits.Count; i++)
            {
                string a = splits[i].Item1;
                string b = splits[i].Item2;
                for (char c = 'a'; c <= 'z'; c++)
                {
                    inserts.Add(a + c + b);
                }
            }

            return deletes.Union(transposes).Union(replaces).Union(inserts).ToList();
        }
    }

    public class SearchItemRequest
    {
        public int PeopleId { get; set; }
        public int WarehouseId { get; set; }
        public string Keyword { get; set; }
        public bool IsRequiredSuggestion { get; set; }
        public int CategoryId { get; set; }
        public int CompanyId { get; set; }
        public int BrandId { get; set; }
        public int ItemId { get; set; }
    }


    public class SearchItemResponse
    {
        public string message { get; set; }
        public List<string> Suggestion { get; set; }
        public bool IsStopSearch { get; set; }
        public String IdField { get; set; }
        public List<SearchData> Items { get; set; }
    }

    public class SearchData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MOQ { get; set; }
    }
    public class SearchItem
    {
        public int itemid { get; set; }
        public string logourl { get; set; }
        public string itemname { get; set; }
        public string itemmultimrpid { get; set; }
        public int categoryid { get; set; }
        public string categoryname { get; set; }
        public int subcategoryid { get; set; }
        public string subcategoryname { get; set; }
        public int subsubcategoryid { get; set; }
        public string subsubcategoryname { get; set; }
        public double price { get; set; }
        public double unitprice { get; set; }
        public int minorderqty { get; set; }
    }

    public class AddCartResponse
    {
        public bool result { get; set; }
        public string message { get; set; }
    }


    public class WarehouseQuadrantCustomerTypeDC
    {
        public List<int> WarehouseIDs { get; set; }
        public string Quadrant { get; set; }
        public List<int> StoreIDs { get; set; }
        public string CustomerType { get; set; }
        public float? Margin { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }
    public class SearchWarehouseQuadrantCustomerTypeDC
    {
        public long Id { get; set; }
        public int WarehouseId { get; set; }
        public float MinMarginPercent { get; set; }
        public string WarehouseName { get; set; }
        public string Quadrant { get; set; }
        public string StoreName { get; set; }
        public string customerType { get; set; }
        public int TotalRecords { get; set; }
    }
    public class WarehouseQuadrantMarginExport
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public float MinMarginPercent { get; set; }

        public string Quadrant { get; set; }
        public string StoreName { get; set; }
        public string customerType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StoreListDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

    }
}
