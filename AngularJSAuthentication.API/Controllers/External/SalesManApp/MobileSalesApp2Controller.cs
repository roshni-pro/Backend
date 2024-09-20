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
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
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
using System.Security.Claims;
using LinqKit;
using MongoDB.Driver;
using MongoDB.Bson;
using AngularJSAuthentication.Model.Seller;
using System.IO;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.Model.Salescommission;
using System.Configuration;
using static AngularJSAuthentication.API.Controllers.CustomersController;
using static AngularJSAuthentication.API.Controllers.DeliverychargeController;
using static AngularJSAuthentication.API.Controllers.SalesAppCounterController;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.DataContracts.PeopleNotification;
using System.Text;
using AngularJSAuthentication.DataContracts.CustomerReferralDc;


namespace AngularJSAuthentication.API.Controllers.External.SalesManApp
{
    [RoutePrefix("api/MobileSalesApp")]
    public class MobileSalesApp2Controller : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public double xPointValue = AppConstants.xPoint;
        public int MemberShipHours = AppConstants.MemberShipHours;
        public bool ElasticSearchEnable = AppConstants.ElasticSearchEnable;

        #region SalesAppItem controller


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
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Retailer App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                foreach (var it in ItemData.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1) && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)))
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
            var itemResponseDc = new ItemResponseDc { TotalItem = 0, ItemDataDCs = new List<DataContracts.External.ItemDataDC>() };
            using (var context = new AuthContext())
            {
                List<DataContracts.External.ItemDataDC> ItemDataDCs = new List<DataContracts.External.ItemDataDC>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                //if (take == 4)
                //    take = 100;

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
                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == "Retailer App" || x.OfferAppType == "Both")).Select(x => x.OfferId).ToList() : new List<int>();

                foreach (var it in ItemData.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1)))
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

                    ItemDataDCs.Add(it);
                }

                itemResponseDc.ItemDataDCs = ItemDataDCs;
            }

            return itemResponseDc;
        }


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

                foreach (var it in itemMasters)
                {
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
        public async Task<ItemListForAgent> ItemListForAgent(int WarehouseId, string lang, int PeopleId)
        {

            using (var context = new AuthContext())
            {
                ItemListForAgent item = new ItemListForAgent();
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetItemForAgentApp]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = cmd.ExecuteReader();
                var newdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<Itemdata>(reader).ToList();

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
                foreach (var it in newdata.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1) && CatIds.Contains(a.Categoryid) && SubCats.Contains(a.SubCategoryId) && SubSubCats.Contains(a.SubsubCategoryid)))
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
        public HttpResponseMessage Search(string lang, string itemname, int PeopleId, int warehouseId)
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
        public HttpResponseMessage getSaleIntentItembysscatid(string lang, int PeopleId, int warehouseid, int catid, int scatid, int sscatid)
        {
            using (var context = new AuthContext())

            {
                //string lang="hi";
                List<SalesIntentItemResponse> item = new List<SalesIntentItemResponse>();
                //  var param= new SqlParameter("@lang", lang);
                // var param1= new SqlParameter("@PeopleId", PeopleId) ;
                //var param2= new SqlParameter("@warehouseid", warehouseid);
                var param3 = new SqlParameter("@catid", catid);
                var param4 = new SqlParameter("@scatid", scatid);
                var param5 = new SqlParameter("@sscatid", sscatid);



                var dataSP = context.Database.SqlQuery<SalesIntentHistoryDC>("exec[SpSalesIndentItemNew] @catid,@scatid,@sscatid", param3, param4, param5).ToList();


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
                               join i in context.ItemMultiMRPDB on a.Number equals i.ItemNumber
                               select new
                               {
                                   ItemNumber = a.Number,
                                   itemname = a.itemBaseName,
                                   IsSensitive = a.IsSensitive,
                                   IsSensitiveMRP = a.IsSensitiveMRP,
                                   UnitofQuantity = i.UnitofQuantity,
                                   UOM = i.UOM,
                                   LogoUrl = a.LogoUrl,
                                   MRP = a.MRP,
                                   HindiName = a.HindiName,
                                   ItemMultiMRPId = i.ItemMultiMRPId,
                                   SubCategoryId = a.SubCategoryId,
                                   Categoryid = a.Categoryid,
                                   SubsubCategoryid = a.SubsubCategoryid,
                                   WarehouseId = a.WarehouseId
                               }).ToList();

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
                    // k.ItemForecastDetailId = 418;
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
                    k.CreatedDate = DateTime.Now;
                    k.ModifiedDate = DateTime.Now;
                    context.SalesIntentRequestDb.Add(k);
                    context.Commit();
                    status = true;
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
                var barcodeitem = db.ItemBarcodes.FirstOrDefault(i => i.Barcode == barcode && i.IsDeleted == false && i.IsActive == true);
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
        private List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> GetCatSubCatwithStores(int peopleid)
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
                    storeids = context.ClusterStoreExecutives.Where(x => x.ExecutiveId == peopleid && x.IsDeleted == false && x.IsActive).Select(x => x.StoreId).Distinct().ToList();

                RetailerAppManager retailerAppManager = new RetailerAppManager();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();

                results = StoreCategorySubCategoryBrands.Where(x => storeids.Contains(x.StoreId)).ToList();
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
        private GeoCoordinate GetCentralGeoCoordinate(
        IList<GeoCoordinate> geoCoordinates)
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

            return new GeoCoordinate(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
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

        #endregion

        #region SalesAppReport Controller

        [Route("BrandWise")]
        [HttpGet]
        public dynamic Getdata(string day, int PeopleId)
        {

            using (var db = new AuthContext())
            {
                var people = db.Peoples.Where(x => x.PeopleID == PeopleId && x.Deleted == false).SingleOrDefault();

                if (day != null && PeopleId > 0)
                {
                    List<Target> item = new List<Target>();
                    //item = db.TargetDb.Where(x => x.WarehouseId == people.WarehouseId).ToList();
                    var date = indianTime;
                    var sDate = indianTime.Date;
                    if (day == "1Month")
                    {
                        sDate = indianTime.AddMonths(-1).Date;
                    }
                    else if (day == "3Month")
                    {
                        sDate = indianTime.AddMonths(-3).Date;
                    }
                    var list = (from i in db.DbOrderDetails
                                where i.CreatedDate > sDate && i.CreatedDate <= date && i.WarehouseId == people.WarehouseId && i.ExecutiveId == PeopleId
                                join k in db.itemMasters on i.ItemId equals k.ItemId
                                join l in db.SubsubCategorys on k.SubsubCategoryid equals l.SubsubCategoryid
                                select new SaleDC
                                {
                                    Sale = i.TotalAmt,
                                    SubsubcategoryName = l.SubsubcategoryName,
                                }).ToList();


                    var result = list.GroupBy(d => d.SubsubcategoryName)
                        .Select(
                            g => new
                            {
                                Sale = g.Sum(s => s.Sale),
                                BrandName = g.First().SubsubcategoryName,
                            });
                    return result;
                }

                else
                {
                    return null;
                }
            }

        }


        [Route("SalesManAppNew")]
        [HttpGet]
        public HttpResponseMessage MobileAppV1(DateTime? datefrom, DateTime? dateto, int id)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    var orderQuery = string.Format("select distinct sum(totalamt) over(partition by  o.executiveid ) sale ," +
                                                    "sum(totalamt) over(partition by  o.executiveid, o.storeid) Storesale," +
                        " max(isnull(s.name, 'Other'))  over(partition by  o.storeid) StoreName," +
                        " dense_rank() over(partition by  o.executiveid order by od.orderid) + dense_rank() over(partition by  o.executiveid order by od.orderid desc) - 1 OrderCount, " +
                        " dense_rank() over(partition by  o.executiveid order by od.CustomerId) + dense_rank() over(partition by  o.executiveid order by od.CustomerId desc) - 1 OrderCustomerCount, " +
                        "  dense_rank() over(partition by  o.executiveid, o.storeid  order by od.orderid) + dense_rank() over(partition by  o.executiveid, o.storeid  order by od.orderid desc) - 1 StoreOrderCount" +
                        " from OrderDetails o with(nolock)" +
                       " inner join OrderMasters od  with(nolock) on o.OrderId = od.OrderId and o.ExecutiveId = {0}" +
                       " left join stores s with(nolock) on o.StoreId = s.Id", id);

                    int ActiveCustomercount = 0, TotalCustomercount = 0;

                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();
                    var clusterIds = clusterStoreExecutiveDcs.Where(x => x.ExecutiveId == id).Select(x => x.ClusterId).Distinct().ToList();
                    var predicate = PredicateBuilder.New<Customer>();
                    predicate = predicate.And(x => x.ClusterId.HasValue && clusterIds.Contains(x.ClusterId.Value) && x.Deleted == false);

                    //var builder = Builders<BsonDocument>.Filter;
                    //var filter = builder.Eq("orderDetails.ExecutiveId", id) & builder.Eq("active", true) & builder.Eq("Deleted", false);
                    if (datefrom != null && dateto != null)
                    {
                        //filter = filter & builder.Gte("CreatedDate", datefrom.Value) & builder.Lte("CreatedDate", dateto.Value);
                        predicate = predicate.And(x => x.CreatedDate >= datefrom && x.CreatedDate <= dateto);
                        orderQuery += string.Format(" where od.CreatedDate between '{0}' and '{1}'", datefrom.Value, dateto.Value);
                    }
                    //MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                    TotalCustomercount = db.Customers.Count(predicate);
                    predicate = predicate.And(x => x.Active);
                    ActiveCustomercount = db.Customers.Count(predicate);
                    ////IMongoDatabase mogodb = mongoDbHelper.dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                    //var collection = mongoDbHelper.mongoDatabase.GetCollection<BsonDocument>("OrderMaster");
                    //var aggTotal = collection.Aggregate().Match(filter)
                    //.Group(new BsonDocument
                    //                {
                    //                    {
                    //                      "_id", "$orderDetails.ExecutiveId"
                    //                    },
                    //                    {
                    //                        "total", new BsonDocument
                    //                                     {
                    //                                         {
                    //                                             "$sum", "$orderDetails.TotalAmt"
                    //                                         }
                    //                                     }
                    //                    },
                    //                    {

                    //                         "count", new BsonDocument
                    //                                     {
                    //                                       {
                    //                                           "$sum", 1
                    //                                       }
                    //                                    }
                    //                    }
                    //                }).Project(new BsonDocument
                    //                {
                    //                    {"_id", 0},
                    //                    {"total", 1},
                    //                    {"count", 2},
                    //                });


                    //var doc = aggTotal.FirstOrDefault();

                    var orderData = db.Database.SqlQuery<orderDataDC>(orderQuery).ToList();

                    var res = new
                    {
                        Customercountdata = TotalCustomercount,
                        ActiveCustomer = ActiveCustomercount,
                        OrderCountdata = orderData != null && orderData.Any() ? orderData.FirstOrDefault().OrderCount : 0,
                        TotalOrderAmount = orderData != null && orderData.Any() ? orderData.FirstOrDefault().sale : 0,
                        OrderCustomerCount = orderData != null && orderData.Any() ? orderData.FirstOrDefault().OrderCustomerCount : 0,
                        StoreDetail = orderData != null && orderData.Any() ? orderData.Select(x => new StoreSalesDc { StoreName = x.StoreName, Storesale = x.Storesale, StoreOrderCount = x.StoreOrderCount }).ToList() : new List<StoreSalesDc>()
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                catch (Exception ex)
                {
                    var storedetail = new { StoreName = "", Storesale = 0, StoreOrderCount = 0 };
                    return Request.CreateResponse(HttpStatusCode.OK,

                        new
                        {
                            Customercountdata = 0,
                            ActiveCustomer = 0,
                            OrderCountdata = 0,
                            TotalOrderAmount = 0,
                            OrderCustomerCount = 0,
                            StoreDetail = new List<StoreSalesDc>()
                        });

                }
            }
        }


        [Route("OrderSummary")]
        [HttpGet]
        public HttpResponseMessage OrderSummary(int PeopleId)
        {
            var result = new OrderSummaryDc();
            using (var context = new AuthContext())
            {
                if (PeopleId > 0)
                {
                    var param = new SqlParameter("@PeopleId", PeopleId);
                    result = context.Database.SqlQuery<OrderSummaryDc>("exec [SalesAppOrderSummary] @PeopleId", param).FirstOrDefault();
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [Route("ExecuteSalesTarget")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<SalesTargetResponse> ExecuteSalesTarget(int peopleId, int CustomerId, int skip, int take, string itemName = null)
        {
            SalesTargetResponse response = new SalesTargetResponse();
            List<SalesTargetCustomerItem> result = new List<SalesTargetCustomerItem>();
            using (var context = new AuthContext())
            {
                DateTime startDate, endDate;
                DateTime now = indianTime;
                itemName = itemName == null ? "" : itemName;
                //if (Month == 0)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //}
                //else if (Month == 1)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-1);
                //    startDate = new DateTime(now.Year, now.Month, 1);

                //}
                //else
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-2);
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //}

                var subcatid = 0;
                //DateTime date = indianTime.AddMonths(Month);
                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.SalesTargetByCustomerId";
                parameters.Add(new SqlParameter("@CustomerId", CustomerId));
                parameters.Add(new SqlParameter("@PeopleId", peopleId));
                parameters.Add(new SqlParameter("@SubCategoryId", subcatid));
                parameters.Add(new SqlParameter("@itemName", itemName));
                //parameters.Add(new SqlParameter("@StartDate", startDate));
                //parameters.Add(new SqlParameter("@EndDate", endDate));
                parameters.Add(new SqlParameter("@skip", skip));
                parameters.Add(new SqlParameter("@take", take));
                sqlquery = sqlquery + " @CustomerId, @PeopleId,@SubCategoryId,@itemName, @skip,@take";
                result = await context.Database.SqlQuery<SalesTargetCustomerItem>(sqlquery, parameters.ToArray()).ToListAsync();

                if (skip == 0)
                {
                    response.AchivePercent = await context.Database.SqlQuery<double>("exec GetAchiveSalesTargetByPeopleId " + peopleId).FirstOrDefaultAsync();
                }
                response.SalesTargetCustomerItems = result;

            }
            return response;
        }

        [Route("CustomerSalesTargetbyBrand")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<SalesTargetCustomerItem>> CustomerSalesTargetbyBrand(int peopleId, int CustomerId, int subCategoryId)
        {
            List<SalesTargetCustomerItem> result = new List<SalesTargetCustomerItem>();
            using (var context = new AuthContext())
            {
                DateTime startDate, endDate;
                DateTime now = indianTime;
                string itemName = "";
                //if (Month == 0)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //}
                //else if (Month == 1)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-1);
                //    startDate = new DateTime(now.Year, now.Month, 1);

                //}
                //else
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-2);
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //}

                if (CustomerId > 0)
                {
                    int skip = 0; int take = 100;
                    //  DateTime date = indianTime.AddMonths(Month);
                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.SalesTargetByCustomerId";
                    parameters.Add(new SqlParameter("@CustomerId", CustomerId));
                    parameters.Add(new SqlParameter("@PeopleId", peopleId));
                    parameters.Add(new SqlParameter("@SubCategoryId", subCategoryId));
                    parameters.Add(new SqlParameter("@itemName", itemName));
                    //parameters.Add(new SqlParameter("@StartDate", startDate));
                    //parameters.Add(new SqlParameter("@EndDate", endDate));
                    parameters.Add(new SqlParameter("@skip", skip));
                    parameters.Add(new SqlParameter("@take", take));
                    sqlquery = sqlquery + " @CustomerId, @PeopleId, @SubCategoryId,@itemName, @skip, @take";
                    result = await context.Database.SqlQuery<SalesTargetCustomerItem>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return result;
        }

        [Route("BrandWiseCustomerSalesTarget")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CompanySalesTargetCustomer>> BrandWiseCustomerSalesTarget(int peopleId, int CustomerId, int skip, int take)
        {
            List<CompanySalesTargetCustomer> result = new List<CompanySalesTargetCustomer>();
            using (var context = new AuthContext())
            {
                DateTime startDate, endDate;
                DateTime now = indianTime;

                //if (Month == 0)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //}
                //else if (Month == 1)
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-1);
                //    startDate = new DateTime(now.Year, now.Month, 1);

                //}
                //else
                //{
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //    endDate = startDate.AddMonths(1).AddDays(-1).AddHours(now.Hour).AddMinutes(now.Minute);
                //    now = now.AddMonths(-2);
                //    startDate = new DateTime(now.Year, now.Month, 1);
                //}

                if (CustomerId > 0)
                {
                    //DateTime date = indianTime.AddMonths(Month);
                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.SalesTargetByCustomerBrand";
                    parameters.Add(new SqlParameter("@CustomerId", CustomerId));
                    parameters.Add(new SqlParameter("@PeopleId", peopleId));
                    //parameters.Add(new SqlParameter("@StartDate", startDate));
                    //parameters.Add(new SqlParameter("@EndDate", endDate));
                    parameters.Add(new SqlParameter("@skip", skip));
                    parameters.Add(new SqlParameter("@take", take));
                    sqlquery = sqlquery + " @CustomerId, @PeopleId,  @skip,@take";
                    result = await context.Database.SqlQuery<CompanySalesTargetCustomer>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return result;
        }


        #region Sales Target page

        [Route("UniqueItemByNumber")]
        [HttpGet]
        public async Task<SalesTargetItemDc> SearchUniqueItemByNumber(string ItemNumber)
        {
            SalesTargetItemDc result = new SalesTargetItemDc();
            using (var context = new AuthContext())
            {
                result = await context.ItemMasterCentralDB.Where(y => y.Number == ItemNumber && y.Deleted == false).Select(x => new SalesTargetItemDc
                {
                    ItemName = x.itemBaseName,
                    ItemNumber = x.Number
                }).FirstOrDefaultAsync();
            }
            return result;
        }

        [Route("InsertUpdateSalesTarget")]
        [HttpPost]
        public async Task<string> InsertUpdateSalesTargets(PostSalesTargetItemDc PostSalesTargetItem)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            string result = "";
            using (var context = new AuthContext())
            {
                if (PostSalesTargetItem != null && PostSalesTargetItem.Id > 0)
                {
                    var item = context.SalesTargets.Where(x => x.Id == PostSalesTargetItem.Id).FirstOrDefault();
                    if (item != null && PostSalesTargetItem.BaseQty >= 0)
                    {
                        item.BaseQty = PostSalesTargetItem.BaseQty;
                        context.Entry(item).State = EntityState.Modified;
                        context.Commit();
                        result = "Record updated successfully";

                    }
                    else { result = "No Record exits"; }
                }
                else
                {
                    bool isexist = context.SalesTargets.Any(x => x.ItemNumber == PostSalesTargetItem.ItemNumber && x.ItemMultiMrpId == PostSalesTargetItem.ItemMultiMrpId && x.StoreId == PostSalesTargetItem.StoreId);
                    if (!isexist)
                    {
                        AngularJSAuthentication.Model.Seller.SalesTarget item = new AngularJSAuthentication.Model.Seller.SalesTarget();
                        item.IsActive = true;
                        item.IsDeleted = false;
                        item.BaseQty = PostSalesTargetItem.BaseQty;
                        item.ItemMultiMrpId = PostSalesTargetItem.ItemMultiMrpId;
                        item.ItemNumber = PostSalesTargetItem.ItemNumber;
                        item.StoreId = PostSalesTargetItem.StoreId;
                        item.CreatedBy = userid;
                        item.CreatedDate = indianTime;
                        item.ModifiedBy = userid;
                        item.ModifiedDate = indianTime;
                        context.SalesTargets.Add(item);
                        context.Commit();
                        result = "Record added successfully";
                    }
                    else { result = "Record already exits"; }

                }

            }
            return result;
        }


        [Route("SalesTargetListByStoreId")]
        [HttpGet]
        public async Task<List<SalesTargetListItemDc>> SalesTargetListByStoreId(long StoreId)
        {
            List<SalesTargetListItemDc> result = new List<SalesTargetListItemDc>();
            using (var context = new AuthContext())
            {
                if (StoreId > 0)
                {
                    //int skip = 0; int take = 100;
                    List<Object> parameters = new List<object>();
                    string sqlquery = "exec Seller.GetSalesTargetByStoreId";
                    parameters.Add(new SqlParameter("@StoreId", StoreId));

                    //parameters.Add(new SqlParameter("@skip", skip));
                    //parameters.Add(new SqlParameter("@take", take));
                    sqlquery = sqlquery + " @StoreId";
                    result = await context.Database.SqlQuery<SalesTargetListItemDc>(sqlquery, parameters.ToArray()).ToListAsync();
                }
            }
            return result;
        }

        [Route("SaleTargetReport")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<SalesTargetCustomerItem>> SaleTargetReportBySKcode(int peopleId)
        {
            List<SalesTargetCustomerItem> result = new List<SalesTargetCustomerItem>();
            using (var context = new AuthContext())
            {

                List<Object> parameters = new List<object>();
                string sqlquery = "exec Seller.SaleTargetReportBySKcode";
                parameters.Add(new SqlParameter("@PeopleId", peopleId));
                sqlquery = sqlquery + "  @PeopleId";
                result = await context.Database.SqlQuery<SalesTargetCustomerItem>(sqlquery, parameters.ToArray()).ToListAsync();
            }
            return result;
        }


        [Route("SaleCustomerRetentionTarget")]
        [HttpGet]
        public async Task<string> SaleCustomerRetentionTarget(int peopleId)
        {
            string expiredHtml = string.Empty;
            List<ExecutiveRetailer> executiveRetailers = new List<ExecutiveRetailer>();
            List<ExecutiveBrandPurchaseRetailer> executiveBrandPurchaseRetailers = new List<ExecutiveBrandPurchaseRetailer>();
            using (var context = new AuthContext())
            {

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();


                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesPersonData]";
                cmd.Parameters.Add(new SqlParameter("@peopleId", peopleId));
                cmd.CommandType = CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                executiveRetailers = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<ExecutiveRetailer>(reader).ToList();
                reader.NextResult();
                while (reader.Read())
                {
                    executiveBrandPurchaseRetailers = ((IObjectContextAdapter)context)
                                                       .ObjectContext
                                                       .Translate<ExecutiveBrandPurchaseRetailer>(reader).ToList();
                }

            }

            if (executiveRetailers != null && executiveRetailers.Any())
            {
                int minCustomer = executiveRetailers.Select(x => new
                {
                    Date = new DateTime(x.year, x.month, 1),
                    x.CustomerCount
                }).OrderBy(x => x.Date).FirstOrDefault().CustomerCount;

                int maxCustomer = executiveRetailers.Select(x => new
                {
                    Date = new DateTime(x.year, x.month, 1),
                    x.CustomerCount
                }).OrderByDescending(x => x.Date).FirstOrDefault().CustomerCount;


                maxCustomer = maxCustomer > minCustomer ? minCustomer : maxCustomer;
                int parcent = maxCustomer * 100 / minCustomer;

                MongoDbHelper<ExecutiveCompanyTarget> mongoDbHelper = new MongoDbHelper<ExecutiveCompanyTarget>();
                List<ExecutiveCompanyTarget> ExecutiveCompanyTargets = mongoDbHelper.Select(x => x.SubCategoryId > 0).ToList();
                string pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/SalesAPPBrand.html";
                string content = File.ReadAllText(pathToHTMLFile);
                if (!string.IsNullOrEmpty(content))
                {
                    string html = "<div class='row'><div class='col-xs-3 nopadding' ><b>[name]</b></div><div class='col-xs-9 nopadding'><div class='progress'><div class='progress-bar progress-bar-striped active' role='progressbar' aria-valuenow='[currentvalue]' aria-valuemin='0' aria-valuemax='100' style='background-color: #[color];width:[currentvalue]%'> [currentvalue]%</div></div></div></div>";
                    string retailer = html.Replace("[name]", "Total Retailer").Replace("[currentvalue]", parcent.ToString()).Replace("[color]", "5cb85c");
                    string Brandretailer = "";
                    Random rnd = new Random();
                    foreach (var item in ExecutiveCompanyTargets)
                    {
                        var color = string.IsNullOrEmpty(item.Color) ? String.Format("{0:X6}", rnd.Next(0x1000000)) : item.Color;
                        int customercount = 0;
                        if (executiveBrandPurchaseRetailers != null && executiveBrandPurchaseRetailers.Any(x => x.SubcategoryName == item.SubCategoryName))
                        {
                            var data = executiveBrandPurchaseRetailers.FirstOrDefault(x => x.SubcategoryName == item.SubCategoryName);
                            customercount = data.CustomerCount > item.CustomerCount ? item.CustomerCount : data.CustomerCount;
                        }
                        parcent = customercount * 100 / item.CustomerCount;
                        Brandretailer += html.Replace("[name]", item.SubCategoryName).Replace("[currentvalue]", parcent.ToString()).Replace("[color]", color);
                    }
                    expiredHtml = content.Replace("[ExecutiveRetailer]", retailer).Replace("[ExecutiveBrandRetailer]", Brandretailer);
                }
            }
            return expiredHtml;
        }


        #endregion

        [Route("GetSalesPersonCommission")]
        [HttpGet]
        public async Task<SalesPersonCommission> GetSalesPersonCommission(int peopleId, int warehouseId, int month, int year = 2021)
        {
            SalesPersonCommission salesPersonCommission = new SalesPersonCommission();
            using (var context = new AuthContext())
            {

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                DataTable peopleidDt = new DataTable();
                peopleidDt.Columns.Add("IntValue");
                DataRow dr = peopleidDt.NewRow();
                dr[0] = peopleId;
                peopleidDt.Rows.Add(dr);

                var executiveIds = new SqlParameter("executiveIds", peopleidDt);
                executiveIds.SqlDbType = SqlDbType.Structured;
                executiveIds.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesCommission]";
                cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                cmd.Parameters.Add(new SqlParameter("@Month", month));
                cmd.Parameters.Add(new SqlParameter("@Year", year));
                cmd.Parameters.Add(executiveIds);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var SalesPersonCommissionData = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<SalesPersonCommissionData>(reader).ToList();

                if (SalesPersonCommissionData != null && SalesPersonCommissionData.Any())
                {
                    salesPersonCommission = SalesPersonCommissionData.GroupBy(x => x.Name).Select(x => new SalesPersonCommission
                    {
                        Name = x.Key,
                        CategoryCommissions = x.GroupBy(y => new { y.CategoryName, y.ShowColumnWithValueField }).Select(z =>
                               new CategoryCommission
                               {
                                   CategoryName = z.Key.CategoryName,
                                   ShowColumnWithValueField = JsonConvert.DeserializeObject<Dictionary<string, string>>(z.Key.ShowColumnWithValueField),
                                   EventCommissions = z.Select(p => new EventCommission
                                   {
                                       Id = p.Id,
                                       BookedValue = Convert.ToInt32(Math.Round(p.BookedValue, 0)),
                                       EventCatName = p.EventCatName,
                                       EventName = p.EventName,
                                       IncentiveType = p.IncentiveType,
                                       IncentiveValue = p.IncentiveValue,
                                       ReqBookedValue = Convert.ToInt32(Math.Round(p.ReqBookedValue, 0)),
                                       EarnValue = Convert.ToInt32(Math.Round(p.EarnValue, 0)),
                                       EndDate = p.EndDate,
                                       StartDate = p.StartDate
                                   }
                                  ).ToList()
                               }
                        ).ToList()
                    }).FirstOrDefault();
                }
                else
                {
                    string Name = context.Peoples.FirstOrDefault(x => x.PeopleID == peopleId).DisplayName;
                    salesPersonCommission = new SalesPersonCommission
                    {
                        Name = Name
                    };
                }
            }

            return salesPersonCommission;
        }
        [Route("GetBrandsWiseItemList/{Warehouseid}")]
        [HttpPost]
        public IEnumerable<ItemMasterSalesDc> GetBrandsWiseItemList([FromUri] int Warehouseid, [FromBody] List<int> BrandId)
        {
            using (var db = new AuthContext())
            {
                //  List<ItemMaster> ass = new List<ItemMaster>();
                List<ItemMasterSalesDc> result = new List<ItemMasterSalesDc>();
                using (var context = new AuthContext())
                {
                    try
                    {
                        if (Warehouseid > 0)
                        {
                            List<ItemMasterSalesDc> ass = context.itemMasters.Where(t => BrandId.Contains(t.SubsubCategoryid) && t.WarehouseId == Warehouseid && t.Deleted == false && t.active == true).Select(t => new ItemMasterSalesDc { ItemId = t.ItemId, SellingSku = t.SellingSku, itemname = t.itemname }).ToList();
                            foreach (var item in ass.GroupBy(x => x.SellingSku))
                            {
                                result.Add(item.ToList().FirstOrDefault());
                            }
                            result.Insert(0, new ItemMasterSalesDc
                            {
                                itemname = "All Item",
                                ItemId = 0,
                                SellingSku = ""
                            });
                            var results = result.OrderBy(x => x.ItemId);
                            return results;
                        }

                        else
                        {
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in Item Master " + ex.Message);
                        return null;
                    }
                }
            }
        }

        [Route("GetsalesCommissionCatMaster")]
        [HttpGet]
        public List<SalesCommissionCatMaster> GetsalesCommissionCatMaster()
        {
            using (var db = new AuthContext())
            {
                List<SalesCommissionCatMaster> list = db.SalesCommissionCatMasters.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                return list;
            }
        }
        [Route("GetSalesCommissionEventMasterList")]
        [HttpGet]
        public List<SalesCommissionEventMaster> GetSalesCommissionEventMasterList(long CommissionCatMasterId)
        {
            using (var db = new AuthContext())
            {
                List<SalesCommissionEventMaster> list = db.SalesCommissionEventMasters.Where(x => x.CommissionCatMasterId == CommissionCatMasterId && x.IsActive == true && x.IsDeleted == false).ToList();
                return list;
            }
        }
        [Route("GetExecutiveList/{warehouseId}")]
        [HttpGet]
        public List<ClusterExecutive> GetExecutiveList(int warehouseId)
        {
            List<ClusterExecutive> result = null;
            using (var authContext = new AuthContext())
            {
                var idParam = new SqlParameter("WarehouseId", SqlDbType.Int);
                idParam.Value = warehouseId;
                result = authContext.Database.SqlQuery<ClusterExecutive>("exec Store_GetDistinctClusterExecutiveByWarehouseId @WarehouseId", idParam).ToList();
                return result;
            }
        }
        #endregion

        #region MyRegion

        #endregion
        //#region SalesApp controller

        //#region:SalesAppCounter
        //[Route("")]
        //[AllowAnonymous]
        //[HttpPost]
        //public HttpResponseMessage add(SalesAppCounterDc sale)
        //{
        //    try
        //    {
        //        SalesAppCounter sales = new SalesAppCounter
        //        {
        //            Date = indianTime,
        //            Deleted = false,
        //            lat = sale.lat,
        //            Long = sale.Long,
        //            SalesPersonId = sale.SalesPersonId
        //        };
        //        using (var db = new AuthContext())
        //        {
        //            db.SalesAppCounterDB.Add(sales);
        //            db.Commit();
        //            SalesAppCounterDTO MUData = new SalesAppCounterDTO()
        //            {
        //                MUget = sales,
        //                Status = true,
        //                Message = " Added suscessfully."
        //            };
        //            //var query = @"select p.peopleId as SalesPersonId, p.Mobile, p.PeopleFirstName, p.PeopleLastName, p.Email, w.WarehouseName, w.WarehouseId from people p inner join Warehouses w on w.WarehouseId = p.WarehouseId where p.peopleId=#salesPersonID#";
        //            //query = query.Replace("#salesPersonID#", sale.SalesPersonId.ToString());
        //            //InitialPoint initialPoint = new InitialPoint()
        //            //{
        //            //    lat = sale.lat,
        //            //    Long = sale.Long,
        //            //    Mobile = sale.Mobile,
        //            //    PeopleFirstName = sale.PeopleFirstName,
        //            //    PeopleLastName = sale.PeopleLastName,
        //            //    WarehouseId = sale.WarehouseId,
        //            //    WarehouseName = sale.WarehouseName,
        //            //    SalesPersonId = sale.SalesPersonId
        //            //};
        //            //var client = new SignalRMasterClient(DbConstants.URL + "signalr");
        //            //// Send message to server.
        //            //string message = JsonConvert.SerializeObject(initialPoint);
        //            //client.SayHello(message, initialPoint.WarehouseId.ToString());
        //            //client.Stop();
        //            //string message = JsonConvert.SerializeObject(initialPoint);
        //            //ChatFeed.SendChatMessage(message, initialPoint.WarehouseId.ToString());
        //            return Request.CreateResponse(HttpStatusCode.OK, MUData);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SalesAppCounterDTO MUData = new SalesAppCounterDTO()
        //        {
        //            MUget = null,
        //            Status = false,
        //            Message = "Something Went Wrong."
        //        };
        //        logger.Error("Error in Add data salesperson " + ex.Message);
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, MUData);
        //    }
        //}
        //#endregion

        ///// <summary>
        ///// Delivery charge new API  for sales app
        ///// </summary>
        ///// <param name="WarehouseId"></param>
        ///// <param name="PeopleId"></param>
        ///// <returns></returns>
        //[Route("DeliveryCharge")]
        //[HttpGet]
        //public async Task<DeliveryChageDC> GetWarehouseDeliveryCharge(int WarehouseId, int PeopleId)
        //{
        //    DeliveryChageDC Commission = new DeliveryChageDC();
        //    if (WarehouseId > 0 && PeopleId > 0)
        //    {
        //        using (AuthContext context = new AuthContext())
        //        {
        //            var query = "Select delcharge.*,sum(agentcom.Amount) as CommissionAmt from DeliveryCharges delcharge inner join Customers cust on delcharge.WarehouseId = cust.Warehouseid inner join AgentCommissionforCities agentcom on agentcom.CustomerId = cust.CustomerId where delcharge.WarehouseId = " + WarehouseId + " and agentcom.PeopleId = " + PeopleId + " and delcharge.IsActive = 1 and delcharge.isDeleted = 0 group by delcharge.[id],delcharge.[CompanyId],delcharge.[min_Amount],delcharge.[max_Amount] ,delcharge.[del_Charge],delcharge.[WarehouseId],delcharge.[cluster_Id],delcharge.[warhouse_Name],delcharge.[cluster_Name],delcharge.[IsActive],delcharge.[isDeleted],delcharge.IsDistributor";
        //            Commission = context.Database.SqlQuery<DeliveryChageDC>(query).FirstOrDefault();
        //        }
        //    }
        //    return Commission;
        //}


        ///// <summary>
        ///// created by 19/01/2019
        ///// get people profile
        ///// </summary>
        ///// <param name="PeopleId"></param>
        ///// <returns></returns>
        //[Route("Profile")]
        //[HttpGet]
        //public HttpResponseMessage GetProfile(int PeopleId)
        //{
        //    Peopleresponse res;
        //    People person = new People();
        //    if (PeopleId > 0)
        //    {
        //        using (var db = new AuthContext())
        //        {
        //            person = db.Peoples.Where(u => u.PeopleID == PeopleId).SingleOrDefault();
        //            if (person != null)
        //            {

        //                if (person.IsLocation == null)
        //                {
        //                    person.IsLocation = false;
        //                }
        //                if (person.IsRecording == null)
        //                {
        //                    person.IsRecording = false;

        //                }
        //                if (person.LocationTimer == null)
        //                {
        //                    person.LocationTimer = 0;
        //                }
        //                string query = "select distinct r.Name as Role from People p inner join AspNetUsers u on p.Email=u.Email inner join AspNetUserRoles ur on u.Id=ur.UserId inner join AspNetRoles r on ur.RoleId=r.Id where PeopleID='" + PeopleId + "'and ur.isActive=1 and p.Active=1 and p.Deleted=0";
        //                var role = db.Database.SqlQuery<string>(query).ToList();
        //                var IsRole = role.Any(x => x.Contains("Hub sales lead"));
        //                if (IsRole)
        //                {
        //                    person.Role = "Hub sales lead";
        //                }
        //                else
        //                {
        //                    person.Role = "";
        //                }

        //                var data = db.LocationResumeDetails.Where(z => z.PeopleId == PeopleId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
        //                if (data != null)
        //                {

        //                    person.Status = data.Status;
        //                }

        //                var list =

        //                res = new Peopleresponse()
        //                {
        //                    people = person,
        //                    Status = true,
        //                    message = "Success."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            else
        //            {
        //                res = new Peopleresponse()
        //                {
        //                    people = person,
        //                    Status = false,
        //                    message = "People not exist."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //        }
        //    }
        //    res = new Peopleresponse()
        //    {
        //        people = person,
        //        Status = false,
        //        message = "Something went wrong."
        //    };
        //    return Request.CreateResponse(HttpStatusCode.OK, res);
        //}

        //[Route("UpdateExectiveStartAddress")]
        //[HttpGet]
        //public HttpResponseMessage UpdateExectiveStartAddress(int peopleId, double lat, double lng)
        //{
        //    if (peopleId > 0)
        //    {
        //        using (var db = new AuthContext())
        //        {
        //            var person = db.Peoples.Where(u => u.PeopleID == peopleId).SingleOrDefault();
        //            if (person != null)
        //            {
        //                person.StartLat = lat;
        //                person.StartLng = lng;
        //                person.UpdatedDate = DateTime.Now;
        //                db.Entry(person).State = EntityState.Modified;
        //                db.Commit();
        //                var res = new Peopleresponse()
        //                {
        //                    Status = true,
        //                    message = "Success."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //            else
        //            {
        //                var res = new Peopleresponse()
        //                {
        //                    Status = false,
        //                    message = "People not exist."
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var res = new Peopleresponse()
        //        {
        //            Status = false,
        //            message = "People not exist."
        //        };
        //        return Request.CreateResponse(HttpStatusCode.OK, res);
        //    }
        //}


        //#region Global Search for sales app
        ///// <summary>
        ///// Created Raj by 25/02/2020
        ///// Global customer Serach(Skcode/ShopName/Mobile)  
        ///// </summary>
        ///// <param name="WarehouseId"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("GlobalSearch")]
        //[AllowAnonymous]
        //public HttpResponseMessage GlobalsearchV1(int PeopleId, int WarehouseId, string Globalkey)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {

        //        GlobalcustomerDetail obj = new GlobalcustomerDetail();

        //        var customer = new List<SalespDTO>();
        //        if (!string.IsNullOrEmpty(Globalkey) && Globalkey.Length > 5)
        //        {

        //            var Warehouseid = new SqlParameter
        //            {
        //                ParameterName = "WarehouseId",
        //                Value = WarehouseId,

        //            };
        //            var ParamPeopleId = new SqlParameter
        //            {
        //                ParameterName = "PeopleId",
        //                Value = PeopleId,

        //            };
        //            var GlobalKey = new SqlParameter
        //            {
        //                ParameterName = "Globalkey",
        //                Value = Globalkey,

        //            };
        //            customer = db.Database.SqlQuery<SalespDTO>("CustomerGlobalSearch @WarehouseId,@PeopleId,@Globalkey", Warehouseid, ParamPeopleId, GlobalKey).ToList();
        //        }
        //        if (customer.Count() > 0)
        //        {
        //            obj = new GlobalcustomerDetail()
        //            {
        //                customers = customer,
        //                Status = true,
        //                Message = "Customer Found"
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, obj);
        //        }
        //        else
        //        {
        //            obj = new GlobalcustomerDetail()
        //            {
        //                customers = customer,
        //                Status = false,
        //                Message = "No Customer found"
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, obj);
        //        }

        //    }
        //}
        //#endregion

        //[HttpGet]
        //[Route("Search")]
        //public HttpResponseMessage Search(int PeopleId, string key)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        var customer = db.Customers.Where(c => (c.Skcode.Contains(key) || c.ShopName.Contains(key) || c.Mobile.Contains(key)) && c.Deleted == false).ToList();
        //        var warehouseIds = customer.Select(x => x.Warehouseid).Distinct().ToList();
        //        var warehouses = db.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
        //        var customerids = customer.Select(x => x.CustomerId).ToList();
        //        var custDocs = db.CustomerDocs.Where(x => customerids.Contains(x.CustomerId) && x.IsActive).ToList();
        //        var gstDocttypeid = db.CustomerDocTypeMasters.FirstOrDefault(x => x.IsActive && x.DocType == "GST")?.Id;
        //        var clusterIds = customer.Select(x => x.ClusterId).ToList();
        //        var clusterExecutive = db.ClusterStoreExecutives.Where(x => clusterIds.Contains(x.ClusterId) && x.IsActive && !x.IsDeleted.Value).Select(x => new { x.ClusterId, x.ExecutiveId }).ToList();

        //        customer.ForEach(x =>
        //        {
        //            x.ExecutiveId = clusterExecutive.Any(y => y.ClusterId == x.ClusterId && y.ExecutiveId == PeopleId) ? clusterExecutive.FirstOrDefault(y => y.ClusterId == x.ClusterId && y.ExecutiveId == PeopleId).ExecutiveId : 0;
        //            x.WarehouseName = x.Warehouseid.HasValue ? warehouses.FirstOrDefault(z => z.WarehouseId == x.Warehouseid.Value)?.WarehouseName : "";
        //            if (gstDocttypeid.HasValue && custDocs.Any(y => y.CustomerDocTypeMasterId != gstDocttypeid.Value))
        //            {
        //                x.CustomerDocTypeMasterId = custDocs.FirstOrDefault(y => y.CustomerDocTypeMasterId != gstDocttypeid.Value).CustomerDocTypeMasterId;
        //            }

        //        });

        //        return Request.CreateResponse(HttpStatusCode.OK, customer);
        //    }
        //}

        //[Route("GetCustomerDocType")]
        //[HttpGet]
        //public async Task<dynamic> GetCustomerDocType(int warehouseId, int PeopleId)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        var CustomerDocTypes = await db.CustomerDocTypeMasters.Where(x => x.IsActive).ToListAsync();
        //        return Request.CreateResponse(HttpStatusCode.OK, CustomerDocTypes);
        //    }

        //}


        //[Route("GetMinOrderAmount")]
        //[HttpGet]
        //public async Task<dynamic> GetRetailerMinOrderAmountSalesAPP(int warehouseId, int PeopleId)
        //{
        //    int minOrderValue = Convert.ToInt32(ConfigurationManager.AppSettings["MinOrderValue"]);
        //    int NoOfLineItemSales = 0;
        //    List<StoreMinOrder> storeMinOrder = new List<StoreMinOrder>();
        //    using (var context = new AuthContext())
        //    {
        //        NoOfLineItemSales = context.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x =>
        //            x.NoOfLineItemSales
        //        ).FirstOrDefault();
        //        var warehouse = await context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId);
        //        if (warehouse != null && warehouse.Cityid > 0)
        //        {
        //            MongoDbHelper<DataContracts.Mongo.RetailerMinOrder> mongoDbHelper = new MongoDbHelper<DataContracts.Mongo.RetailerMinOrder>();
        //            var cartPredicate = PredicateBuilder.New<DataContracts.Mongo.RetailerMinOrder>(x => x.CityId == warehouse.Cityid);
        //            var retailerMinOrder = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "RetailerMinOrder").FirstOrDefault();
        //            if (retailerMinOrder != null)
        //            {
        //                minOrderValue = retailerMinOrder.MinOrderValue;
        //            }
        //            else
        //            {
        //                DataContracts.Mongo.RetailerMinOrder newRetailerMinOrder = new DataContracts.Mongo.RetailerMinOrder
        //                {
        //                    CityId = warehouse.Cityid,
        //                    WarehouseId = warehouse.WarehouseId,
        //                    MinOrderValue = minOrderValue
        //                };
        //                var result = mongoDbHelper.Insert(newRetailerMinOrder);
        //            }
        //        }
        //        MongoDbHelper<StoreMinOrder> mHelperStore = new MongoDbHelper<StoreMinOrder>();
        //        storeMinOrder = mHelperStore.Select(x => x.StoreId > 0 && (x.CityId == 0 || x.CityId == warehouse.Cityid)).ToList();
        //        storeMinOrder = storeMinOrder.GroupBy(x => new { x.CityId, x.StoreId }).Select(x => new StoreMinOrder { CityId = x.Key.CityId, StoreId = x.Key.StoreId, MinOrderValue = x.FirstOrDefault().MinOrderValue }).ToList();
        //    }

        //    return new { minOrderValue = minOrderValue, StoreMinOrder = storeMinOrder, NoOfLineItem = NoOfLineItemSales };

        //}


        //#region Beats
        ///// <summary>
        ///// Created By Raj 
        ///// Date:25/02/2020
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="day"></param>
        ///// <returns></returns>
        //[Route("customer/V3")]
        //[AllowAnonymous]
        //public HttpResponseMessage GetBeatDataV3(int id, string day)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        GlobalcustomerDetail obj = new GlobalcustomerDetail();

        //        MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
        //        var today = DateTime.Now.Date;
        //        var todayBeats = mongoDbHelper.Select(x => x.PeopleId == id && x.AssignmentDate == today);


        //        if (todayBeats != null && todayBeats.Any())
        //        {
        //            if (!string.IsNullOrEmpty(day) && day != "undefined")
        //            {
        //                if (day != DateTime.Now.DayOfWeek.ToString())
        //                {
        //                    var executiveBeats = db.Database.SqlQuery<SalespDTO>(string.Format("exec GetExcutiveBeatCustomerexceptToday {0}", id)).ToList();
        //                    var OtherDayPlannedCustomers = executiveBeats.Select(i => new PlannedRoute
        //                    {
        //                        CustomerId = i.CustomerId,
        //                        CompanyId = i.CompanyId,
        //                        Active = i.Active,
        //                        CustomerVerify = i.CustomerVerify,
        //                        City = i.City,
        //                        WarehouseId = i.WarehouseId,
        //                        WarehouseName = i.WarehouseName,
        //                        lat = i.lat,
        //                        lg = i.lg,
        //                        ExecutiveId = i.ExecutiveId,
        //                        BeatNumber = i.BeatNumber,
        //                        Day = i.Day,
        //                        Skcode = i.Skcode,
        //                        Mobile = i.Mobile,
        //                        ShopName = i.ShopName,
        //                        BillingAddress = i.BillingAddress,
        //                        ShippingAddress = i.ShippingAddress,
        //                        Name = i.Name,
        //                        Emailid = i.Emailid,
        //                        RefNo = i.RefNo,
        //                        Password = i.Password,
        //                        UploadRegistration = i.UploadRegistration,
        //                        ResidenceAddressProof = i.ResidenceAddressProof,
        //                        DOB = i.DOB,
        //                        MaxOrderCount = i.MaxOrderCount,
        //                        IsKPP = i.IsKPP,
        //                        ClusterId = i.ClusterId,
        //                        ClusterName = i.ClusterName,
        //                        CustomerType = i.CustomerType
        //                    }).ToList();
        //                    todayBeats.ForEach(x =>
        //                    {
        //                        x.PlannedRoutes.AddRange(OtherDayPlannedCustomers);
        //                    });
        //                }

        //                todayBeats.ForEach(x =>
        //                {
        //                    x.PlannedRoutes = x.PlannedRoutes.Where(s => !string.IsNullOrEmpty(s.Day) && s.Day == day).ToList();
        //                });
        //            }
        //            else
        //            {
        //                var executiveBeats = db.Database.SqlQuery<SalespDTO>(string.Format("exec GetExcutiveBeatCustomerexceptToday {0}", id)).ToList();
        //                var OtherDayPlannedCustomers = executiveBeats.Select(i => new PlannedRoute
        //                {
        //                    CustomerId = i.CustomerId,
        //                    CompanyId = i.CompanyId,
        //                    Active = i.Active,
        //                    CustomerVerify = i.CustomerVerify,
        //                    City = i.City,
        //                    WarehouseId = i.WarehouseId,
        //                    WarehouseName = i.WarehouseName,
        //                    lat = i.lat,
        //                    lg = i.lg,
        //                    ExecutiveId = i.ExecutiveId,
        //                    BeatNumber = i.BeatNumber,
        //                    Day = i.Day,
        //                    Skcode = i.Skcode,
        //                    Mobile = i.Mobile,
        //                    ShopName = i.ShopName,
        //                    BillingAddress = i.BillingAddress,
        //                    ShippingAddress = i.ShippingAddress,
        //                    Name = i.Name,
        //                    Emailid = i.Emailid,
        //                    RefNo = i.RefNo,
        //                    Password = i.Password,
        //                    UploadRegistration = i.UploadRegistration,
        //                    ResidenceAddressProof = i.ResidenceAddressProof,
        //                    DOB = i.DOB,
        //                    MaxOrderCount = i.MaxOrderCount,
        //                    IsKPP = i.IsKPP,
        //                    ClusterId = i.ClusterId,
        //                    ClusterName = i.ClusterName,
        //                    CustomerType = i.CustomerType
        //                }).ToList();
        //                todayBeats.ForEach(x =>
        //                {
        //                    x.PlannedRoutes.AddRange(OtherDayPlannedCustomers);
        //                });
        //            }

        //        }


        //        if (todayBeats != null && todayBeats.Any())
        //        {
        //            //var existingActualRoute = todayBeats != null
        //            //                    ? todayBeats.Where(s => s.ActualRoutes != null && s.ActualRoutes.Any()).SelectMany(z => z.ActualRoutes)
        //            //                    : null;

        //            //if (existingActualRoute != null && existingActualRoute.Any())
        //            //    todayBeats.ForEach(s => s.PlannedRoutes.RemoveAll(x => existingActualRoute.Select(z => z.CustomerId).Contains(x.CustomerId)));

        //            return Request.CreateResponse(HttpStatusCode.OK, new
        //            {
        //                customers = todayBeats,
        //                Status = true,
        //                Message = "Customer Found"
        //            });
        //        }
        //        else
        //            return Request.CreateResponse(HttpStatusCode.OK, new
        //            {
        //                customers = todayBeats,
        //                Status = false,
        //                Message = "No Customer found"
        //            });


        //    }
        //}

        //[Route("GetSalesDashboardData")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<SalesDeshboardData> GetSalesDashboardData(int peopleId)
        //{
        //    SalesDeshboardData salesDeshboardData = new SalesDeshboardData();
        //    string completeTargetColor = "#FF6161", IncompleteTargetColor = "#4FFF6C";
        //    using (var context = new AuthContext())
        //    {
        //        salesDeshboardData.ShowTarget = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowSalesAppDesbaordTarget"]);
        //        MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
        //        MongoDbHelper<ExecuteBeatTarget> mongoDbTargetHelper = new MongoDbHelper<ExecuteBeatTarget>();
        //        var today = DateTime.Now.Date;
        //        var people = (await context.Peoples.FirstOrDefaultAsync(x => x.PeopleID == peopleId));
        //        int cityId = people.Cityid ?? 0;
        //        int? clusterId = (await context.ClusterStoreExecutives.Where(x => x.ExecutiveId == peopleId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ClusterId).FirstOrDefaultAsync());
        //        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        //        var MonthBeat = await mongoDbHelper.SelectAsync(x => x.PeopleId == peopleId && x.AssignmentDate >= firstDayOfMonth && x.AssignmentDate <= today);
        //        var todayBeat = MonthBeat.FirstOrDefault(x => x.AssignmentDate == today);
        //        var MonthCustomers = await context.Customers.Where(x => x.ExecutiveId == peopleId && x.CreatedDate >= firstDayOfMonth).Select(x => new { x.CustomerId, x.CreatedDate }).ToListAsync();
        //        if (todayBeat != null && todayBeat.PlannedRoutes != null && todayBeat.PlannedRoutes.Any())
        //        {
        //            MongoDbHelper<NextDayBeatPlan> mongoDbCustomBeatPlanHelper = new MongoDbHelper<NextDayBeatPlan>();
        //            var CustomBeatPlans = (await mongoDbCustomBeatPlanHelper.SelectAsync(x => x.CreatedDate <= DateTime.Now && x.CreatedDate >= today && x.ExecutiveId == peopleId)).ToList();

        //            var beatTargets = clusterId.HasValue ? mongoDbTargetHelper.Select(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.ClusterId == clusterId).ToList() : null;
        //            var beatTarget = beatTargets.FirstOrDefault();

        //            #region BeatCustomerOrder
        //            var beatCustomerids = MonthBeat.SelectMany(y => y.PlannedRoutes.Select(x => x.CustomerId)).ToList();
        //            if (MonthBeat.Select(x => x.ActualRoutes).Any())
        //            {
        //                if (beatCustomerids == null)
        //                    beatCustomerids = new List<int>();

        //                beatCustomerids.AddRange(MonthBeat.Select(x => x.ActualRoutes).Where(x => x != null).SelectMany(x => x.Select(y => y.CustomerId)).ToList());
        //            }
        //            beatCustomerids = beatCustomerids.Distinct().ToList();

        //            if (context.Database.Connection.State != ConnectionState.Open)
        //                context.Database.Connection.Open();
        //            var customerIdDt = new DataTable();
        //            customerIdDt.Columns.Add("IntValue");
        //            foreach (var item in beatCustomerids)
        //            {
        //                var dr = customerIdDt.NewRow();
        //                dr["IntValue"] = item;
        //                customerIdDt.Rows.Add(dr);
        //            }
        //            var param = new SqlParameter("customerId", customerIdDt);
        //            param.SqlDbType = SqlDbType.Structured;
        //            param.TypeName = "dbo.IntValues";
        //            var cmd = context.Database.Connection.CreateCommand();
        //            cmd.Parameters.Add(new SqlParameter("@ExectiveId", peopleId));
        //            cmd.CommandText = "[dbo].[GetBeatCustomerOrder]";
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //            cmd.CommandTimeout = 600;
        //            cmd.Parameters.Add(param);

        //            // Run the sproc
        //            var reader = cmd.ExecuteReader();
        //            var beatCustomerOrders = ((IObjectContextAdapter)context)
        //            .ObjectContext
        //            .Translate<BeatCustomerOrder>(reader).ToList();
        //            #endregion


        //            var BeatCustomers = todayBeat.PlannedRoutes.Select(x => new BeatCustomer
        //            {
        //                CustomerId = x.CustomerId,
        //                Active = x.Active,
        //                TravalStart = x.TravalStart,
        //                BillingAddress = x.BillingAddress,
        //                IsVisited = x.IsVisited,
        //                BeatNumber = x.BeatNumber.HasValue ? x.BeatNumber.Value : todayBeat.PlannedRoutes.Count + 1,
        //                lat = x.lat,
        //                AreaName = x.AreaName,
        //                lg = x.lg,
        //                Mobile = x.Mobile,
        //                Name = x.Name,
        //                IsKPP = x.IsKPP,
        //                ShippingAddress = x.ShippingAddress,
        //                ShopName = x.ShopName,
        //                Skcode = x.Skcode,
        //                WarehouseId = x.WarehouseId,
        //                WarehouseName = x.WarehouseName,
        //                MaxOrderCount = x.MaxOrderCount,
        //                WtAvgAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId) ? beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId).FirstOrDefault().weightAvgAmount : 0,
        //                WtAvgOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId) ? beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId).FirstOrDefault().weightAvgOrder : 0,
        //                AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today).Sum(z => z.lineItem) / beatCustomerOrders.Count(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today)) : 0,
        //                TotalLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today).Sum(z => z.lineItem)) : 0,
        //                TotalOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ? beatCustomerOrders.Count(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) : 0,
        //                TotalOrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today).Sum(z => z.GrossAmount), 0)) : 0,
        //                Comment = todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId) ? todayBeat.ActualRoutes.FirstOrDefault(y => y.CustomerId == x.CustomerId).Comment : "",
        //                Status = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today) ?
        //                           "Ordered" : (!x.IsVisited ? "Not Visited" : (todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.ShopCloseImage))
        //                           ? "Shop Closed - Skip" : ((CustomBeatPlans != null && CustomBeatPlans.Any(y => y.CustomerId == x.CustomerId)) ? "Reschedule" : (todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.Comment) && y.Comment.Contains("Not Visited")) ? "Not Visited" : "Visited"))))
        //            }).ToList();


        //            salesDeshboardData.MyBeat = new MyBeat
        //            {
        //                AreaName = todayBeat != null && todayBeat.PlannedRoutes != null ? todayBeat.PlannedRoutes.FirstOrDefault(x => !string.IsNullOrEmpty(x.ClusterName)).ClusterName + "-" + todayBeat.AssignmentDate.DayOfWeek.ToString() : "",
        //                BeatCustomers = BeatCustomers,
        //                TodayVisit = BeatCustomers != null ? BeatCustomers.Count : 0,
        //                Visited = BeatCustomers != null && BeatCustomers.Any(x => x.IsVisited) ? BeatCustomers.Count(x => x.IsVisited) : 0,
        //                AvgLineItem = BeatCustomers != null && BeatCustomers.Sum(x => x.TotalOrder) > 0 ? BeatCustomers.Sum(z => z.TotalLineItem) / BeatCustomers.Sum(x => x.TotalOrder) : 0,
        //                BeatAmount = BeatCustomers != null ? BeatCustomers.Sum(z => z.TotalOrderAmount) : 0,
        //                BeatOrder = BeatCustomers != null ? BeatCustomers.Sum(y => y.TotalOrder) : 0,
        //                Conversion = BeatCustomers != null ? BeatCustomers.Where(y => y.TotalOrder > 0).Select(z => z.CustomerId).Distinct().Count() : 0,
        //                AvgLineItemColor = "#ffffff",
        //                BeatAmountColor = "#ffffff",
        //                BeatOrderColor = "#ffffff",
        //                ConversionColor = "#ffffff",
        //                VisitedColor = "#ffffff",
        //            };
        //            if (beatTarget != null)
        //            {
        //                salesDeshboardData.MyBeat.VisitedColor = salesDeshboardData.MyBeat.Visited <= (salesDeshboardData.MyBeat.TodayVisit * beatTarget.VisitedPercent / 100) ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.MyBeat.ConversionColor = salesDeshboardData.MyBeat.Conversion <= (salesDeshboardData.MyBeat.TodayVisit * beatTarget.ConversionPercent / 100) ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.MyBeat.BeatOrderColor = salesDeshboardData.MyBeat.BeatOrder <= (salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.MyBeat.BeatAmountColor = salesDeshboardData.MyBeat.BeatAmount <= (salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.MyBeat.AvgLineItemColor = salesDeshboardData.MyBeat.AvgLineItem <= beatTarget.AvgLineItem ? completeTargetColor : IncompleteTargetColor;


        //                salesDeshboardData.BeatTarget = new BeatTarget
        //                {
        //                    AvgLineItem = beatTarget.AvgLineItem,
        //                    Conversion = Convert.ToInt32(salesDeshboardData.MyBeat.TodayVisit * beatTarget.ConversionPercent / 100),
        //                    CustomerCount = Convert.ToInt32(salesDeshboardData.MyBeat.TodayVisit * beatTarget.CustomerPercent / 100),
        //                    OrderCount = Convert.ToInt32((salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100)),
        //                    OrderAmount = Convert.ToInt32((salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount),
        //                    Visited = Convert.ToInt32(salesDeshboardData.MyBeat.TodayVisit * beatTarget.VisitedPercent / 100),
        //                };
        //            }
        //            else
        //            {
        //                salesDeshboardData.BeatTarget = new BeatTarget
        //                {
        //                    AvgLineItem = 0,
        //                    Conversion = 0,
        //                    CustomerCount = 0,
        //                    OrderCount = 0,
        //                    OrderAmount = 0,
        //                    Visited = 0,
        //                };
        //            }


        //            salesDeshboardData.SalesMetricsDaily = new BeatSale
        //            {
        //                CustomerCount = beatCustomerOrders != null && beatCustomerOrders.Any(x => x.CreatedDate.Date == today) ? beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Select(x => x.CustomerId).Distinct().Count() : 0,
        //                TotalOrders = beatCustomerOrders != null && beatCustomerOrders.Any(x => x.CreatedDate.Date == today) ? beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Count() : 0,
        //                AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(x => x.CreatedDate.Date == today) ? Convert.ToDecimal(beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Sum(x => x.lineItem)) / beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Count() : 0,
        //                TotalAmount = beatCustomerOrders != null && beatCustomerOrders.Any(x => x.CreatedDate.Date == today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(x => x.CreatedDate.Date == today).Sum(x => x.GrossAmount), 0)) : 0,
        //                AvgLineItemColor = "#ffffff",
        //                TotalAmountColor = "#ffffff",
        //                TotalOrdersColor = "#ffffff",
        //                CustomerCountColor = "#ffffff",
        //            };
        //            if (beatTarget != null)
        //            {
        //                salesDeshboardData.SalesMetricsDaily.AvgLineItemColor = salesDeshboardData.SalesMetricsDaily.AvgLineItem < beatTarget.AvgLineItem ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SalesMetricsDaily.TotalOrdersColor = salesDeshboardData.SalesMetricsDaily.TotalOrders < (salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SalesMetricsDaily.CustomerCountColor = salesDeshboardData.SalesMetricsDaily.CustomerCount < (salesDeshboardData.MyBeat.TodayVisit * beatTarget.CustomerPercent / 100) ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SalesMetricsDaily.TotalAmountColor = salesDeshboardData.SalesMetricsDaily.TotalOrders < (salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount ? completeTargetColor : IncompleteTargetColor;

        //                salesDeshboardData.SalesTarget = new SalesTarget
        //                {
        //                    AvgLineItem = beatTarget.AvgLineItem,
        //                    CustomerCount = Convert.ToInt32(salesDeshboardData.MyBeat.TodayVisit * beatTarget.CustomerPercent / 100),
        //                    OrderCount = Convert.ToInt32((salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100)),
        //                    OrderAmount = Convert.ToInt32((salesDeshboardData.MyBeat.TodayVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount),
        //                };
        //            }
        //            else
        //            {
        //                salesDeshboardData.SalesTarget = new SalesTarget
        //                {
        //                    AvgLineItem = 0,
        //                    CustomerCount = 0,
        //                    OrderCount = 0,
        //                    OrderAmount = 0,
        //                };
        //            }



        //            var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
        //            var weekBeats = MonthBeat.Where(x => x.AssignmentDate >= thisWeekStart && x.AssignmentDate <= today).SelectMany(p => p.PlannedRoutes.Select(x => new BeatCustomer
        //            {
        //                CustomerId = x.CustomerId,
        //                Active = x.Active,
        //                BillingAddress = x.BillingAddress,
        //                IsVisited = x.IsVisited,
        //                BeatNumber = x.BeatNumber.HasValue ? x.BeatNumber.Value : todayBeat.PlannedRoutes.Count + 1,
        //                lat = x.lat,
        //                lg = x.lg,
        //                Mobile = x.Mobile,
        //                Name = x.Name,
        //                IsKPP = x.IsKPP,
        //                ShippingAddress = x.ShippingAddress,
        //                ShopName = x.ShopName,
        //                Skcode = x.Skcode,
        //                WarehouseId = x.WarehouseId,
        //                WarehouseName = x.WarehouseName,
        //                AreaName = x.AreaName,
        //                MaxOrderCount = x.MaxOrderCount,
        //                WtAvgAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId) ? beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId).FirstOrDefault().weightAvgAmount : 0,
        //                WtAvgOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId) ? beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId).FirstOrDefault().weightAvgOrder : 0,
        //                AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Average(z => z.lineItem)) : 0,
        //                TotalLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date == today).Sum(z => z.lineItem)) : 0,
        //                TotalOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Count(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) : 0,
        //                TotalOrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Sum(z => z.GrossAmount), 0)) : 0,
        //                //Comment = p.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId) ? todayBeat.ActualRoutes.FirstOrDefault(y => y.CustomerId == x.CustomerId).Comment : "",
        //                Status = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ?
        //                            //"Ordered" : (!x.IsVisited ? "Not Visited" : (p.ActualRoutes != null && p.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.ShopCloseImage))
        //                            //? "Shop Closed - Skip" : "Visited"))
        //                            "Ordered" : (!x.IsVisited ? "Not Visited" : (todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.ShopCloseImage))
        //                           ? "Shop Closed - Skip" : ((CustomBeatPlans != null && CustomBeatPlans.Any(y => y.CustomerId == x.CustomerId)) ? "Reschedule" : (todayBeat.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.Comment) && y.Comment.Contains("Not Visited")) ? "Not Visited" : "Visited"))))
        //            }).ToList()).ToList();


        //            salesDeshboardData.SaleMetricsWeekly = new BeatSaleWeekly
        //            {
        //                BeatCustomers = weekBeats,
        //                PlannedVisit = weekBeats.Count(),
        //                Visited = weekBeats.Where(x => x.IsVisited).Count(),
        //                NotVisited = weekBeats.Where(x => !x.IsVisited).Count(),
        //                CustomerCount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Select(x => x.CustomerId).Distinct().Count() : 0,
        //                AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Sum(z => z.lineItem)) / beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Count() : 0,
        //                TotalAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Sum(z => z.GrossAmount), 0)) : 0,
        //                TotalOrders = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Count(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) : 0,
        //                Conversion = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Where(y => y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Select(z => z.CustomerId).Distinct().Count() : 0,
        //                AvgLineItemColor = "#ffffff",
        //                ConversionColor = "#ffffff",
        //                CustomerCountColor = "#ffffff",
        //                NotVisitedColor = "#ffffff",
        //                TotalAmountColor = "#ffffff",
        //                TotalOrdersColor = "#ffffff",
        //                VisitedColor = "#ffffff",
        //            };

        //            if (beatTarget != null)
        //            {
        //                salesDeshboardData.SaleMetricsWeekly.AvgLineItemColor = salesDeshboardData.SaleMetricsWeekly.AvgLineItem < beatTarget.AvgLineItem ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SaleMetricsWeekly.TotalOrdersColor = salesDeshboardData.SaleMetricsWeekly.TotalOrders < (salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.OrderPercent / 100) ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SaleMetricsWeekly.ConversionColor = salesDeshboardData.SaleMetricsWeekly.Conversion < (salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.ConversionPercent / 100) ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SaleMetricsWeekly.TotalAmountColor = salesDeshboardData.SaleMetricsWeekly.TotalAmount < (salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SaleMetricsWeekly.VisitedColor = salesDeshboardData.SaleMetricsWeekly.Visited < (salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.VisitedPercent / 100) ? completeTargetColor : IncompleteTargetColor;

        //                salesDeshboardData.SalesWeeklyTarget = new BeatTarget
        //                {
        //                    AvgLineItem = beatTarget.AvgLineItem,
        //                    Conversion = Convert.ToInt32(salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.ConversionPercent / 100),
        //                    CustomerCount = Convert.ToInt32(salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.CustomerPercent / 100),
        //                    OrderCount = Convert.ToInt32((salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.OrderPercent / 100)),
        //                    OrderAmount = Convert.ToInt32((salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount),
        //                    Visited = Convert.ToInt32(salesDeshboardData.SaleMetricsWeekly.PlannedVisit * beatTarget.VisitedPercent / 100),
        //                };
        //            }
        //            else
        //            {
        //                salesDeshboardData.SalesWeeklyTarget = new BeatTarget
        //                {
        //                    AvgLineItem = 0,
        //                    Conversion = 0,
        //                    CustomerCount = 0,
        //                    OrderCount = 0,
        //                    OrderAmount = 0,
        //                    Visited = 0,
        //                };
        //            }

        //            ClusterPareto clusterPareto = new ClusterPareto();

        //            if (clusterId.HasValue)
        //            {
        //                var cmd1 = context.Database.Connection.CreateCommand();
        //                cmd1.Parameters.Add(new SqlParameter("@warehouseid", people.WarehouseId));
        //                cmd1.Parameters.Add(new SqlParameter("@clusterid", clusterId));
        //                cmd1.CommandText = "[dbo].[GetCustomerItemPareto]";
        //                cmd1.CommandType = System.Data.CommandType.StoredProcedure;
        //                cmd1.CommandTimeout = 600;


        //                // Run the sproc
        //                var reader1 = cmd1.ExecuteReader();
        //                clusterPareto = ((IObjectContextAdapter)context)
        //                .ObjectContext
        //                .Translate<ClusterPareto>(reader1).FirstOrDefault();
        //            }

        //            var monthBeats = MonthBeat.SelectMany(p => p.PlannedRoutes.Select(x => new BeatCustomer
        //            {
        //                CustomerId = x.CustomerId,
        //                Active = x.Active,
        //                BillingAddress = x.BillingAddress,
        //                IsVisited = x.IsVisited,
        //                BeatNumber = x.BeatNumber.HasValue ? x.BeatNumber.Value : todayBeat.PlannedRoutes.Count + 1,
        //                lat = x.lat,
        //                lg = x.lg,
        //                Mobile = x.Mobile,
        //                Name = x.Name,
        //                IsKPP = x.IsKPP,
        //                ShippingAddress = x.ShippingAddress,
        //                ShopName = x.ShopName,
        //                Skcode = x.Skcode,
        //                WarehouseId = x.WarehouseId,
        //                WarehouseName = x.WarehouseName,
        //                AreaName = x.AreaName,
        //                MaxOrderCount = x.MaxOrderCount,
        //                AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToDecimal(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Average(z => z.lineItem)) : 0,
        //                TotalOrder = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? beatCustomerOrders.Count(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) : 0,
        //                TotalOrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today).Sum(z => z.GrossAmount), 0)) : 0,
        //                //Comment = p.ActualRoutes != null && todayBeat.ActualRoutes.Any(y => y.CustomerId == x.CustomerId) ? todayBeat.ActualRoutes.FirstOrDefault(y => y.CustomerId == x.CustomerId).Comment : "",
        //                Status = beatCustomerOrders != null && beatCustomerOrders.Any(y => y.CustomerId == x.CustomerId && y.CreatedDate.Date >= thisWeekStart && y.CreatedDate.Date <= today) ?
        //                           "Ordered" : (!x.IsVisited ? "Not Visited" : (p.ActualRoutes != null && p.ActualRoutes.Any(y => y.CustomerId == x.CustomerId && !string.IsNullOrEmpty(y.ShopCloseImage))
        //                           ? "Shop Closed - Skip" : "Visited"))
        //            }).ToList()).ToList();

        //            salesDeshboardData.SaleMetricsMonthly = new BeatSaleMonthly
        //            {
        //                //BeatCustomers= monthBeats,
        //                CustomerCount = beatCustomerOrders != null && beatCustomerOrders.Any() ? beatCustomerOrders.Select(x => x.CustomerId).Distinct().Count() : 0,
        //                TotalAmount = beatCustomerOrders != null && beatCustomerOrders.Any() ? Convert.ToInt32(Math.Round(beatCustomerOrders.Sum(z => z.GrossAmount), 0)) : 0,
        //                TotalOrders = beatCustomerOrders != null && beatCustomerOrders.Any() ? beatCustomerOrders.Count() : 0,
        //                AvgLineItem = beatCustomerOrders != null && beatCustomerOrders.Any() ? Convert.ToDecimal(beatCustomerOrders.Sum(z => z.lineItem)) / beatCustomerOrders.Count() : 0,
        //                AvgLineItemColor = "#ffffff",
        //                CustomerCountColor = "#ffffff",
        //                TotalAmountColor = "#ffffff",
        //                TotalOrdersColor = "#ffffff",
        //                CustomerPareto = clusterPareto.CustomerPareto,
        //                ProductPareto = clusterPareto.ItemPareto
        //            };


        //            if (beatTarget != null)
        //            {
        //                salesDeshboardData.SaleMetricsMonthly.AvgLineItemColor = salesDeshboardData.SaleMetricsMonthly.AvgLineItem < beatTarget.AvgLineItem ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SaleMetricsMonthly.TotalOrdersColor = salesDeshboardData.SaleMetricsMonthly.TotalOrders < (monthBeats.Count() * beatTarget.OrderPercent / 100) ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SaleMetricsMonthly.TotalAmountColor = salesDeshboardData.SaleMetricsMonthly.TotalAmount < (monthBeats.Count() * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount ? completeTargetColor : IncompleteTargetColor;
        //                salesDeshboardData.SaleMetricsMonthly.CustomerCountColor = salesDeshboardData.SaleMetricsMonthly.CustomerCount < (monthBeats.Count() * beatTarget.CustomerPercent / 100) ? completeTargetColor : IncompleteTargetColor;

        //                salesDeshboardData.SalesMonthlyTarget = new SalesMonthlyTarget
        //                {
        //                    AvgLineItem = beatTarget.AvgLineItem,
        //                    CustomerCount = Convert.ToInt32(monthBeats.Count() * beatTarget.CustomerPercent / 100),
        //                    OrderCount = Convert.ToInt32(monthBeats.Count() * beatTarget.OrderPercent / 100),
        //                    OrderAmount = Convert.ToInt32(monthBeats.Count() * beatTarget.OrderPercent / 100) * beatTarget.AvgOrderAmount,
        //                    CustomerPareto = beatTarget.CustomerPareto,
        //                    ProductPareto = beatTarget.ProductPareto
        //                };

        //            }
        //            else
        //            {
        //                salesDeshboardData.SalesMonthlyTarget = new SalesMonthlyTarget
        //                {
        //                    AvgLineItem = 0,
        //                    CustomerCount = 0,
        //                    OrderCount = 0,
        //                    OrderAmount = 0,
        //                    CustomerPareto = 0,
        //                    ProductPareto = 0
        //                };
        //            }

        //        }
        //        else
        //        {
        //            salesDeshboardData.MyBeat = new MyBeat
        //            {
        //                AvgLineItemColor = "#ffffff",
        //                BeatAmountColor = "#ffffff",
        //                BeatOrderColor = "#ffffff",
        //                ConversionColor = "#ffffff",
        //                VisitedColor = "#ffffff",
        //            };
        //            salesDeshboardData.SaleMetricsWeekly = new BeatSaleWeekly
        //            {

        //                AvgLineItemColor = "#ffffff",
        //                ConversionColor = "#ffffff",
        //                CustomerCountColor = "#ffffff",
        //                NotVisitedColor = "#ffffff",
        //                TotalAmountColor = "#ffffff",
        //                TotalOrdersColor = "#ffffff",
        //                VisitedColor = "#ffffff",
        //            };
        //            salesDeshboardData.SaleMetricsMonthly = new BeatSaleMonthly
        //            {
        //                AvgLineItemColor = "#ffffff",
        //                CustomerCountColor = "#ffffff",
        //                TotalAmountColor = "#ffffff",
        //                TotalOrdersColor = "#ffffff",
        //            };
        //            salesDeshboardData.SalesMetricsDaily = new BeatSale
        //            {
        //                AvgLineItemColor = "#ffffff",
        //                TotalAmountColor = "#ffffff",
        //                TotalOrdersColor = "#ffffff",
        //                CustomerCountColor = "#ffffff",
        //            };
        //        }

        //        if (MonthCustomers != null && MonthCustomers.Any())
        //        {
        //            var customerIds = MonthCustomers.Select(x => x.CustomerId).ToList();
        //            var newCustSale = context.DbOrderMaster.Where(x => customerIds.Contains(x.CustomerId) && x.OrderTakenSalesPersonId == peopleId).Select(x => new { x.CustomerId, x.OrderId, x.GrossAmount }).ToList();
        //            salesDeshboardData.CustomerAcquisitionMonthly = new BeatSale
        //            {
        //                CustomerCount = MonthCustomers.Count(),
        //                TotalAmount = newCustSale != null && newCustSale.Any() ? Convert.ToInt32(Math.Round(newCustSale.Sum(x => x.GrossAmount), 0)) : 0,
        //                TotalOrders = newCustSale != null && newCustSale.Any() ? newCustSale.Count() : 0,
        //            };
        //        }
        //        else
        //        {
        //            salesDeshboardData.CustomerAcquisitionMonthly = new BeatSale();
        //        }
        //        try
        //        {
        //            #region  CancellationReportResDc
        //            CancellationReportResDc result = new CancellationReportResDc();
        //            if (context.Database.Connection.State != ConnectionState.Open)
        //                context.Database.Connection.Open();
        //            var cmdCancellation = context.Database.Connection.CreateCommand();
        //            cmdCancellation.CommandText = "[Cancellation].[SalesManReport]";
        //            cmdCancellation.CommandType = System.Data.CommandType.StoredProcedure;
        //            cmdCancellation.Parameters.Add(new SqlParameter("@peopleId", peopleId));
        //            var readerCancellation = cmdCancellation.ExecuteReader();
        //            var CancellationReport = ((IObjectContextAdapter)context)
        //             .ObjectContext
        //             .Translate<CancellationReportDc>(readerCancellation).FirstOrDefault();
        //            if (CancellationReport != null)
        //            {
        //                //on amount Cancellation
        //                result.CancelAmount = Math.Round(CancellationReport.CurrentMonthCancelValue, 2);  //Current Month Cancellation amount   

        //                // on count Cancellation
        //                result.CancelCount = CancellationReport.CurrentMonthCancelCount; // Current month Cancellation count
        //                result.CancelCountDiff = result.CancelCount - CancellationReport.LastMonthCancelCount;

        //                double currentCancelCountPercent = Math.Round(CancellationReport.CurrentMonthCancelCount > 0 ? Convert.ToDouble(CancellationReport.CurrentMonthCancelCount) / CancellationReport.CurrentMonthTotalCount * 100 : 0, 2);
        //                double lastCancelCountPercent = Math.Round(CancellationReport.LastMonthCancelCount > 0 ? Convert.ToDouble(CancellationReport.LastMonthCancelCount) / CancellationReport.LastMonthTotalCount * 100 : 0, 2);
        //                result.CompareCountPercent = Math.Round(currentCancelCountPercent - lastCancelCountPercent, 2);

        //                //Cancellation  Percent on value
        //                result.CancellationPercant = Math.Round(CancellationReport.CurrentMonthCancelValue > 0 ? Convert.ToDouble(CancellationReport.CurrentMonthCancelValue) / CancellationReport.CurrentMonthTotalValue * 100 : 0, 2);
        //                double lastCancellationPercant = CancellationReport.LastMonthCancelValue > 0 ? Convert.ToDouble(CancellationReport.LastMonthCancelValue) / CancellationReport.LastMonthTotalValue * 100 : 0;
        //                result.CompareCancellationPercant = Math.Round(result.CancellationPercant - lastCancellationPercant, 2);
        //                if (result.CancellationPercant >= 0 && result.CancellationPercant <= 5)
        //                {
        //                    result.Backgroundcolor = "#FFFFFF";
        //                    result.WarningCount = 0;
        //                }
        //                else if (result.CancellationPercant > 5 && result.CancellationPercant < 10)
        //                {
        //                    result.Backgroundcolor = "#FFFF00"; result.WarningCount = 0;
        //                }
        //                else
        //                {
        //                    result.Backgroundcolor = "#FF0000"; //red
        //                    result.WarningCount = Convert.ToInt32(result.CancellationPercant / 10);
        //                }

        //            }
        //            salesDeshboardData.CancellationReports = result;
        //            #endregion
        //        }
        //        catch (Exception s) { }

        //    }
        //    return salesDeshboardData;
        //}

        //[Route("GetExecutiveRoute")]
        //[AllowAnonymous]
        //public HttpResponseMessage GetExecutiveRoute(int peopleId, DateTime date)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        GlobalcustomerDetail obj = new GlobalcustomerDetail();

        //        MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
        //        var today = date.Date;
        //        var todayBeats = mongoDbHelper.Select(x => x.PeopleId == peopleId && x.AssignmentDate == today);

        //        if (todayBeats != null && todayBeats.Any())
        //        {

        //            todayBeats.ForEach(s => s.PlannedRoutes = s.PlannedRoutes.Where(x => string.IsNullOrEmpty(x.Day) || (!string.IsNullOrEmpty(x.Day) && x.Day == date.DayOfWeek.ToString())).ToList());

        //            //todayBeats.ForEach(s => s.PlannedRoutes.ForEach(x => { 
        //            //    x.IsVisited = s.ActualRoutes.Any(y => y.Day == x.Day);
        //            //    if(s.ActualRoutes.Any(y => y.Day == x.Day))
        //            //        x.VisitedOn = s.ActualRoutes.FirstOrDefault(y => y.Day == x.Day).CreatedDate;
        //            //})) ;

        //            //var existingActualRoute = todayBeats != null
        //            //                    ? todayBeats.Where(s => s.ActualRoutes != null && s.ActualRoutes.Any()).SelectMany(z => z.ActualRoutes)
        //            //                    : null;

        //            //if (existingActualRoute != null && existingActualRoute.Any())
        //            //    todayBeats.ForEach(s => s.PlannedRoutes.RemoveAll(x => existingActualRoute.Select(z => z.CustomerId).Contains(x.CustomerId)));

        //            return Request.CreateResponse(HttpStatusCode.OK, new
        //            {
        //                customers = todayBeats,
        //                Status = true,
        //                Message = "Customer Found"
        //            });
        //        }
        //        else
        //            return Request.CreateResponse(HttpStatusCode.OK, new
        //            {
        //                customers = todayBeats,
        //                Status = false,
        //                Message = "No Customer found"
        //            });


        //    }
        //}


        //#region Beat Plan



        //[Route("StartDay")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<bool> StartDay(DayStartParams param)
        //{
        //    BeatsManager manager = new BeatsManager();
        //    return await manager.InsertBeatInMongo(param.PeopleId, param.lat, param.lng, param.DayStartAddress);
        //}


        //[Route("IsDayStarted/{peopleId}")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<bool> IsDayStarted(int PeopleId)
        //{
        //    BeatsManager manager = new BeatsManager();
        //    return await manager.IsDayStarted(PeopleId);
        //}

        //[Route("BeatStart/{peopleId}/{customerId}")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<DateTime?> BeatStart(int peopleId, int customerId)
        //{
        //    BeatsManager manager = new BeatsManager();
        //    return await manager.BeatStart(peopleId, customerId);
        //}


        //#endregion

        //[Route("InactiveCustOrderCount/{customerid}")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<int> InactiveCustOrderCount(int customerid)
        //{
        //    BeatsManager manager = new BeatsManager();
        //    return await manager.InactiveCustOrderCount(customerid);
        //}


        //[Route("PlanNextDayBeat")]
        //[HttpGet]
        //public async Task<bool> PlanNextDayBeat(int customerId, int peopleId, DateTime planDate, bool isAddOnBeat = false)
        //{
        //    if (!isAddOnBeat)
        //    {
        //        MongoDbHelper<NextDayBeatPlan> mongoDbHelper = new MongoDbHelper<NextDayBeatPlan>();
        //        bool result = await mongoDbHelper.InsertAsync(new NextDayBeatPlan
        //        {
        //            CustomerId = customerId,
        //            ExecutiveId = peopleId,
        //            PlanDate = planDate.Date,
        //            CreatedDate = DateTime.Now
        //        });
        //        return result;
        //    }
        //    else
        //    {
        //        //using (var context = new AuthContext())
        //        //{
        //        //    string day = planDate.DayOfWeek.ToString();
        //        //    if (!context.CustomerExecutiveMappings.Any(x => x.ExecutiveId == peopleId && x.CustomerId == customerId && x.Day == day && x.IsActive))
        //        //    {
        //        //        int beat = context.CustomerExecutiveMappings.Count(x => x.ExecutiveId == peopleId && x.Day == day && x.IsActive);
        //        //        context.CustomerExecutiveMappings.Add(new Model.Store.CustomerExecutiveMapping
        //        //        {
        //        //            CustomerId = customerId,
        //        //            CreatedDate = DateTime.Now,
        //        //            CreatedBy = peopleId,
        //        //            Beat = beat,
        //        //            Day = day,
        //        //            ExecutiveId = peopleId,
        //        //            IsActive = true,
        //        //            IsDeleted = false
        //        //        });
        //        //        context.Commit();
        //        //    }
        //        //}
        //        return true;
        //    }
        //}

        ////[Route("CheckCustomeronBeat")]
        ////[HttpGet]
        ////public async Task<string> CheckCustomerOnBeat(int customerId, int peopleId)
        ////{
        ////    string day = string.Empty;
        ////    using (var context = new AuthContext())
        ////    {
        ////        var custmapping = await context.CustomerExecutiveMappings.FirstOrDefaultAsync(x => x.ExecutiveId == peopleId && x.CustomerId == customerId && x.IsActive);
        ////        if (custmapping != null)
        ////            day = custmapping.Day;
        ////    }
        ////    return day;
        ////}

        //[Route("UpdateActualRoute")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<bool> UpdateActualRoute(List<SalesAppRouteParam> param)
        //{
        //    BeatsManager manager = new BeatsManager();
        //    return await manager.UpdateActualRoute(param);
        //}
        //[Route("UpdateActualRouteForSkip")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<bool> UpdateActualRouteForSkip(SalesAppRouteParam param)
        //{
        //    BeatsManager manager = new BeatsManager();
        //    return await manager.UpdateActualRouteForSkip(param);
        //}


        //[Route("CustomerAddressUpdateRequest")]
        //[HttpPost]
        //public async Task<HttpResponseMessage> CustomerAddressUpdateRequest(CustomerUpdateRequest customerUpdateRequest)
        //{
        //    var Customer = new Customer();
        //    if (customerUpdateRequest.CustomerId > 0)
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            Customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerUpdateRequest.CustomerId);
        //        }
        //    }
        //    MongoDbHelper<CustomerUpdateRequest> mongoDbHelper = new MongoDbHelper<CustomerUpdateRequest>();
        //    int count = mongoDbHelper.Count(x => x.CustomerId == customerUpdateRequest.CustomerId && x.RequestBy == customerUpdateRequest.RequestBy && x.Status == 0);
        //    if (count == 0)
        //    {
        //        customerUpdateRequest.CreatedDate = DateTime.Now;
        //        customerUpdateRequest.Status = 0;
        //        customerUpdateRequest.UpdatedDate = DateTime.Now;
        //        customerUpdateRequest.WarehouseId = Customer.Warehouseid ?? 0;
        //        customerUpdateRequest.SkCode = Customer.Skcode;
        //        customerUpdateRequest.MobileNo = Customer.Mobile;
        //        bool result = await mongoDbHelper.InsertAsync(customerUpdateRequest);
        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            Status = result,
        //            Message = result ? "Updated request save successfully." : "Some issue occurred please try after some time."
        //        });

        //    }
        //    else
        //    {

        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            Status = false,
        //            Message = "Already one updated request pending for this customer."
        //        });
        //    }
        //}

        //#endregion


        //[Route("GetDefaultCustomerid")]
        //[HttpGet]
        //public SalesAppDefaultCustomersDC GetDefaultCustomerid(int WarehouseId)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        SalesAppDefaultCustomersDC res;
        //        var companydetails = db.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
        //        if (companydetails != null)
        //        {
        //            if (WarehouseId > 0)
        //            {
        //                MongoDbHelper<SalesAppDefaultCustomers> SalesAppmongoDbHelper = new MongoDbHelper<SalesAppDefaultCustomers>();
        //                var defaultCustomer = SalesAppmongoDbHelper.Select(x => x.WarehouseId == WarehouseId).FirstOrDefault();
        //                if (defaultCustomer != null)
        //                {
        //                    companydetails.DefaultSalesSCcustomerId = defaultCustomer.CustomerId;
        //                    res = new SalesAppDefaultCustomersDC
        //                    {
        //                        DefaultSalesSCcustomerId = companydetails.DefaultSalesSCcustomerId,
        //                        Status = true,
        //                        Message = "Success!!"
        //                    };
        //                    return res;
        //                }
        //                else
        //                {
        //                    res = new SalesAppDefaultCustomersDC
        //                    {
        //                        DefaultSalesSCcustomerId = 0,
        //                        Status = false,
        //                        Message = "No Data Found!!"
        //                    };
        //                    return res;
        //                }
        //            }
        //            else
        //            {
        //                res = new SalesAppDefaultCustomersDC
        //                {
        //                    DefaultSalesSCcustomerId = 0,
        //                    Status = false,
        //                    Message = "No Data Found!!"
        //                };
        //                return res;
        //            }
        //        }
        //        else
        //        {
        //            res = new SalesAppDefaultCustomersDC
        //            {
        //                DefaultSalesSCcustomerId = 0,
        //                Status = false,
        //                Message = "No Data Found!!"
        //            };
        //            return res;
        //        }
        //    }

        //}

        //[Route("InsertMissExecutiveBeatJob")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<bool> InsertMissExecutiveBeatJob()
        //{
        //    List<int> executiveIds = new List<int>();
        //    using (var context = new AuthContext())
        //    {
        //        executiveIds = await context.ClusterStoreExecutives.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ExecutiveId).Distinct().ToListAsync();
        //    }
        //    if (executiveIds.Any())
        //    {
        //        BeatsManager manager = new BeatsManager();
        //        await manager.InsertNotStartExecutiveBeatInMongo(executiveIds);
        //    }
        //    return true;
        //}


        //[Route("GetAddressUpdateRequest")]
        //[HttpPost]
        //public dynamic GetAddressUpdateRequestList(AddressUpdatepaginationDTO filterOrderDTO)
        //{
        //    int Skiplist = (filterOrderDTO.Skip - 1) * filterOrderDTO.Take;
        //    AddressUpdateResDTO paggingData = new AddressUpdateResDTO();

        //    MongoDbHelper<CustomerUpdateRequest> mongoDbHelper = new MongoDbHelper<CustomerUpdateRequest>();
        //    var orderPredicate = PredicateBuilder.New<CustomerUpdateRequest>();
        //    if (filterOrderDTO.WarehouseId > 0)
        //    {
        //        if (filterOrderDTO.WarehouseId > 0)
        //            orderPredicate.And(x => x.WarehouseId == filterOrderDTO.WarehouseId);

        //        if (!string.IsNullOrEmpty(filterOrderDTO.Keyword))
        //            orderPredicate.And(x => x.SkCode.Contains(filterOrderDTO.Keyword) || x.MobileNo.Contains(filterOrderDTO.Keyword));

        //        if (filterOrderDTO.status >= 0)
        //            orderPredicate.And(x => x.Status == filterOrderDTO.status);

        //        if (filterOrderDTO.FromDate.HasValue && filterOrderDTO.ToDate.HasValue)
        //            orderPredicate.And(x => x.CreatedDate >= filterOrderDTO.FromDate.Value && x.CreatedDate <= filterOrderDTO.ToDate.Value);

        //        int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "CustomerUpdateRequest");

        //        var result = new List<CustomerUpdateRequest>();
        //        result = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), Skiplist, filterOrderDTO.Take, collectionName: "CustomerUpdateRequest").ToList();

        //        var warehouseIds = result.Select(x => x.WarehouseId).Distinct().ToList();
        //        using (var context = new AuthContext())
        //        {
        //            var WarehouseName = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
        //            result.ForEach(x =>
        //            {
        //                x.WarehouseName = WarehouseName.Where(y => y.WarehouseId == x.WarehouseId).Select(y => y.WarehouseName).FirstOrDefault();
        //                if (x.Status == 0)
        //                {
        //                    x.status = "Pending";
        //                }
        //                if (x.Status == 1)
        //                {
        //                    x.status = "Approved";
        //                }
        //                if (x.Status == 2)
        //                {
        //                    x.status = "Reject";
        //                }
        //            });
        //        }
        //        paggingData.totalcount = 0;
        //        if (result != null && result.Any())
        //        {
        //            paggingData.totalcount = dataCount;
        //            paggingData.result = result;
        //        }
        //        return paggingData;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //[Route("UpdateAddressRequest")]
        //[HttpGet]
        //public async Task<bool> UpdateAddressRequest(string ObjId, int Status)
        //{
        //    bool result = false;

        //    ObjectId id = ObjectId.Parse(ObjId);
        //    MongoDbHelper<CustomerUpdateRequest> mongoDbHelper = new MongoDbHelper<CustomerUpdateRequest>();
        //    ExpressionStarter<CustomerUpdateRequest> Predicate = PredicateBuilder.New<CustomerUpdateRequest>(x => (x.Id == id));
        //    CustomerUpdateRequest CustomerUpdateRequest = mongoDbHelper.Select(Predicate, null).FirstOrDefault();

        //    if (CustomerUpdateRequest != null && CustomerUpdateRequest.CustomerId > 0 && Status > 0)
        //    {
        //        if (Status == 1)
        //        {  //1 mean update in customer
        //            using (var context = new AuthContext())
        //            {
        //                var cust = context.Customers.FirstOrDefault(c => c.CustomerId == CustomerUpdateRequest.CustomerId);
        //                cust.lat = CustomerUpdateRequest.UpdatedLat;
        //                cust.lg = CustomerUpdateRequest.UpdatedLng;
        //                cust.ShippingAddress = CustomerUpdateRequest.UpdateedAddress;
        //                cust.UpdatedDate = indianTime;
        //                context.Entry(cust).State = EntityState.Modified;
        //                context.Commit();
        //            }
        //        }
        //        CustomerUpdateRequest.Status = Status;
        //        await mongoDbHelper.ReplaceAsync(CustomerUpdateRequest.Id, CustomerUpdateRequest);
        //        result = true;
        //    }
        //    return result;
        //}

        //[Route("ExportAddressUpdateRequest")]
        //[HttpPost]
        //public dynamic ExportAddressUpdateRequest(AddressUpdatepaginationDTO filterOrderDTO)
        //{
        //    //int Skiplist = (filterOrderDTO.Skip - 1) * filterOrderDTO.Take;
        //    AddressUpdateResDTO paggingData = new AddressUpdateResDTO();

        //    MongoDbHelper<ExportAddressUpdateRequest> mongoDbHelper = new MongoDbHelper<ExportAddressUpdateRequest>();
        //    var orderPredicate = PredicateBuilder.New<ExportAddressUpdateRequest>();

        //    if (filterOrderDTO.WarehouseId > 0)
        //        orderPredicate.And(x => x.WarehouseId == filterOrderDTO.WarehouseId);

        //    if (!string.IsNullOrEmpty(filterOrderDTO.Keyword))
        //        orderPredicate.And(x => x.SkCode.Contains(filterOrderDTO.Keyword) || x.MobileNo.Contains(filterOrderDTO.Keyword));

        //    if (filterOrderDTO.status > 0)
        //        orderPredicate.And(x => x.Status == filterOrderDTO.status);

        //    if (filterOrderDTO.FromDate.HasValue && filterOrderDTO.ToDate.HasValue)
        //        orderPredicate.And(x => x.CreatedDate >= filterOrderDTO.FromDate.Value && x.CreatedDate <= filterOrderDTO.ToDate.Value);

        //    //int dataCount = mongoDbHelper.Count(orderPredicate, collectionName: "CustomerUpdateRequest");
        //    var result = new List<ExportAddressUpdateRequest>();
        //    result = mongoDbHelper.Select(orderPredicate, x => x.OrderByDescending(z => z.CreatedDate), null, null, collectionName: "CustomerUpdateRequest").ToList();
        //    var warehouseIds = result.Select(x => x.WarehouseId).Distinct().ToList();
        //    using (var context = new AuthContext())
        //    {
        //        var WarehouseName = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
        //        result.ForEach(x =>
        //        {
        //            x.WarehouseName = WarehouseName.Where(y => y.WarehouseId == x.WarehouseId).Select(y => y.WarehouseName).FirstOrDefault();
        //            if (x.Status == 0)
        //            {
        //                x.status = "Pending";
        //            }
        //            if (x.Status == 1)
        //            {
        //                x.status = "Approved";
        //            }
        //            if (x.Status == 2)
        //            {
        //                x.status = "Reject";
        //            }
        //        });
        //    }

        //    paggingData.totalcount = 0;
        //    if (result != null && result.Any())
        //    {
        //        paggingData.result = result;
        //    }
        //    return paggingData;
        //}

        ///// <summary>
        ///// Get Sales Dashboard Report on saral
        ///// </summary>
        ///// <param name="peopleId"></param>
        ///// <returns></returns>
        //[Route("SalesDashboardReport")]
        //[HttpPost]
        //public async Task<List<SalesDashboardReportDc>> GetSalesDashboardReport(SalesDashboardReportReqDc req)
        //{
        //    List<string> WeekDays = new List<string>();
        //    var count = (req.EndDate.AddDays(1) - req.StartDate).TotalDays;
        //    if (count >= 7)
        //    {
        //        WeekDays.Add("Monday");
        //        WeekDays.Add("Tuesday");
        //        WeekDays.Add("Wednesday");
        //        WeekDays.Add("Thursday");
        //        WeekDays.Add("Friday");
        //        WeekDays.Add("Saturday");
        //        WeekDays.Add("Sunday");
        //    }
        //    else
        //    {
        //        for (var date = req.StartDate; date <= req.EndDate; date = date.AddDays(1))
        //        {
        //            WeekDays.Add(date.DayOfWeek.ToString());
        //        }
        //        WeekDays.ToArray();
        //    }
        //    List<SalesDashboardReportDc> result = new List<SalesDashboardReportDc>();
        //    if (req != null && req.WarehouseId > 0 && req.PeopleIds.Any())
        //    {
        //        using (var context = new AuthContext())
        //        {
        //            MongoDbHelper<ExecutiveBeats> mongoDbHelper = new MongoDbHelper<ExecutiveBeats>();
        //            var MonthBeat = (await mongoDbHelper.SelectAsync(x => req.PeopleIds.Contains(x.PeopleId) && x.PlannedRoutes != null && x.PlannedRoutes.Count() > 0 && x.AssignmentDate >= req.StartDate && x.AssignmentDate <= req.EndDate)).ToList();

        //            if (MonthBeat != null && MonthBeat.Any())
        //            {
        //                #region GetPeopleBeatCustomerOrder
        //                var beatCustomerids = MonthBeat.Where(x => x.PlannedRoutes != null).SelectMany(x => x.PlannedRoutes.Select(y => y.CustomerId)).ToList();
        //                var actualCustiomerids = MonthBeat.Where(x => x.ActualRoutes != null).SelectMany(x => x.ActualRoutes).Any() ? beatCustomerids : new List<int>();
        //                if (actualCustiomerids != null && actualCustiomerids.Any())
        //                {
        //                    beatCustomerids.AddRange(actualCustiomerids);
        //                }
        //                beatCustomerids = beatCustomerids.Distinct().ToList();

        //                if (context.Database.Connection.State != ConnectionState.Open)
        //                    context.Database.Connection.Open();
        //                //CustomerIds
        //                var customerIdDt = new DataTable();
        //                customerIdDt.Columns.Add("IntValue");
        //                foreach (var item in beatCustomerids)
        //                {
        //                    var dr = customerIdDt.NewRow();
        //                    dr["IntValue"] = item;
        //                    customerIdDt.Rows.Add(dr);
        //                }
        //                var param = new SqlParameter("customerId", customerIdDt);
        //                param.SqlDbType = SqlDbType.Structured;
        //                param.TypeName = "dbo.IntValues";

        //                //PeopleIDs
        //                var peopleIdDt = new DataTable();
        //                peopleIdDt.Columns.Add("IntValue");
        //                foreach (var item in req.PeopleIds)
        //                {
        //                    var dr = peopleIdDt.NewRow();
        //                    dr["IntValue"] = item;
        //                    peopleIdDt.Rows.Add(dr);
        //                }
        //                var param1 = new SqlParameter("ExectiveIds", peopleIdDt);
        //                param1.SqlDbType = SqlDbType.Structured;
        //                param1.TypeName = "dbo.IntValues";

        //                var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = "[dbo].[GetPeopleBeatCustomerOrder]";
        //                cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //                cmd.CommandTimeout = 600;
        //                cmd.Parameters.Add(param);
        //                cmd.Parameters.Add(param1);


        //                // Run the sproc
        //                var reader = cmd.ExecuteReader();
        //                var beatCustomerOrders = ((IObjectContextAdapter)context)
        //                .ObjectContext
        //                .Translate<PeopleBeatCustomerOrder>(reader).ToList();
        //                #endregion

        //                #region GetPeopleBeatCustomers

        //                if (context.Database.Connection.State != ConnectionState.Open)
        //                    context.Database.Connection.Open();

        //                var DaysIdDt = new DataTable();
        //                DaysIdDt.Columns.Add("stringValues");
        //                foreach (var item in WeekDays)
        //                {
        //                    var dr = DaysIdDt.NewRow();
        //                    dr["stringValues"] = item;
        //                    DaysIdDt.Rows.Add(dr);
        //                }
        //                var param2 = new SqlParameter("Days", DaysIdDt);
        //                param2.SqlDbType = SqlDbType.Structured;
        //                param2.TypeName = "dbo.stringValues";

        //                //PeopleIDs
        //                var peopleIdDts = new DataTable();
        //                peopleIdDts.Columns.Add("IntValue");
        //                foreach (var item in req.PeopleIds)
        //                {
        //                    var dr = peopleIdDts.NewRow();
        //                    dr["IntValue"] = item;
        //                    peopleIdDts.Rows.Add(dr);
        //                }
        //                var param3 = new SqlParameter("PeopleIds", peopleIdDts);
        //                param3.SqlDbType = SqlDbType.Structured;
        //                param3.TypeName = "dbo.IntValues";

        //                var cmd1 = context.Database.Connection.CreateCommand();
        //                cmd1.CommandText = "[dbo].[GetPeopleBeatCustomers]";
        //                cmd1.CommandType = System.Data.CommandType.StoredProcedure;
        //                cmd1.CommandTimeout = 600;
        //                cmd1.Parameters.Add(param2);
        //                cmd1.Parameters.Add(param3);
        //                // Run the sproc
        //                var reader1 = cmd1.ExecuteReader();
        //                var PeopleBeatCustomers = ((IObjectContextAdapter)context)
        //                .ObjectContext
        //                .Translate<PeopleBeatCustomers>(reader1).ToList();
        //                #endregion

        //                result = MonthBeat.GroupBy(x => x.PeopleId).Select(x => new SalesDashboardReportDc
        //                {
        //                    ExectiveId = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.Where(q => q.PeopleId == x.Key).Select(w => w.PeopleId).FirstOrDefault() : 0,
        //                    ExectiveName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).ExectiveName : " ",
        //                    WarehouseName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).WarehouseName : " ",
        //                    ClusterName = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).ClusterName : " ",
        //                    TotalCustomer = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).TotalCustomer : 0,
        //                    TotalBeat = (PeopleBeatCustomers != null && PeopleBeatCustomers.Count() > 0) ? PeopleBeatCustomers.FirstOrDefault(q => q.PeopleId == x.Key).TotalBeat : 0,
        //                    CustomerPlann = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.Where(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0).Sum(u => u.PlannedRoutes.Count()) : 0,
        //                    Visited = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.Where(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0).Sum(u => u.PlannedRoutes.Where(e => e.IsVisited == true).Count()) : 0,
        //                    Ordercount = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToInt32(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Count()) : 0,
        //                    OrderAmount = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToInt32(Math.Round(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Sum(z => z.GrossAmount), 0)) : 0,
        //                    AvgLine = beatCustomerOrders != null && beatCustomerOrders.Any(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key) ? Convert.ToDecimal(beatCustomerOrders.Where(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key).Sum(z => z.lineItem) / beatCustomerOrders.Count(c => x.SelectMany(y => y.PlannedRoutes.Select(z => z.CustomerId)).Contains(c.CustomerId) || c.PeopleId == x.Key)) : 0,
        //                    Conversion = beatCustomerOrders != null ? beatCustomerOrders.Where(y => y.OrderId > 0).Select(z => z.CustomerId).Distinct().Count() : 0,
        //                    VisitDetails = x.Any(u => u.PlannedRoutes != null && u.PlannedRoutes.Count() > 0) ? x.SelectMany(i => i.PlannedRoutes.Where(p => p.IsVisited).Select(y => new CustomerVisitDc
        //                    {
        //                        Address = y.ShippingAddress,
        //                        SKcode = y.Skcode,
        //                        Date = y.VisitedOn,
        //                        ShopName = y.ShopName
        //                    })).ToList() : null
        //                }).ToList();
        //            }
        //        }
        //    }
        //    return result;
        //}


        //#region SalesApp Company Apphome 

        //[Route("GetAllSalesStore")]
        //[HttpGet]
        //public async Task<List<RetailerStore>> GetAllSalesStore(int PeopleId, int warehouseId, string lang)
        //{
        //    List<RetailerStore> retailerStore = new List<RetailerStore>();
        //    using (var context = new AuthContext())
        //    {
        //        if (context.Database.Connection.State != ConnectionState.Open)
        //            context.Database.Connection.Open();


        //        var cmd = context.Database.Connection.CreateCommand();
        //        cmd.CommandText = "[dbo].[GetAllStore]";
        //        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
        //        cmd.Parameters.Add(new SqlParameter("@lang", lang));
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //        // Run the sproc
        //        var reader = cmd.ExecuteReader();
        //        retailerStore = ((IObjectContextAdapter)context)
        //        .ObjectContext
        //        .Translate<RetailerStore>(reader).ToList();

        //        #region Mappedstore
        //        List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
        //        if (retailerStore != null && retailerStore.Any())
        //        {
        //            List<int> Subcatids = StoreCategorySubCategoryBrands.Select(x => x.SubCategoryId).Distinct().ToList();
        //            retailerStore = retailerStore.Where(x => Subcatids.Contains(x.SubCategoryId)).ToList();
        //        }
        //        #endregion
        //        #region block Barnd
        //        RetailerAppManager retailerAppManager = new RetailerAppManager();
        //        var custtype = 4;
        //        var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
        //        if (retailerStore != null && retailerStore.Any() && blockBarnds != null && blockBarnds.Any())
        //        {
        //            retailerStore = retailerStore.Where(x => !(blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId))).ToList();
        //        }
        //        #endregion
        //    }
        //    return retailerStore;
        //}

        //[Route("SalesSubCategoryOffer")]
        //[HttpGet]
        //public async Task<OfferdataDc> SalesSubCategoryOffer(int customerId, int PeopleId, int SubCategoryId)
        //{
        //    List<OfferDc> FinalBillDiscount = new List<OfferDc>();
        //    List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
        //    OfferdataDc res;
        //    using (AuthContext context = new AuthContext())
        //    {
        //        CustomersManager manager = new CustomersManager();

        //        List<BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(customerId, "Retailer App");
        //        if (billDiscountOfferDcs.Any())
        //        {
        //            foreach (var billDiscountOfferDc in billDiscountOfferDcs.Where(x => x.BillDiscountType == "subcategory" && x.OfferBillDiscountItems.Any(y => y.Id == SubCategoryId)))
        //            {

        //                var bdcheck = new OfferDc
        //                {
        //                    OfferId = billDiscountOfferDc.OfferId,

        //                    OfferName = billDiscountOfferDc.OfferName,
        //                    OfferCode = billDiscountOfferDc.OfferCode,
        //                    OfferCategory = billDiscountOfferDc.OfferCategory,
        //                    OfferOn = billDiscountOfferDc.OfferOn,
        //                    start = billDiscountOfferDc.start,
        //                    end = billDiscountOfferDc.end,
        //                    DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
        //                    BillAmount = billDiscountOfferDc.BillAmount,
        //                    LineItem = billDiscountOfferDc.LineItem,
        //                    Description = billDiscountOfferDc.Description,
        //                    BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
        //                    BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
        //                    IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
        //                    IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
        //                    IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
        //                    BillDiscountType = billDiscountOfferDc.BillDiscountType,
        //                    OfferAppType = billDiscountOfferDc.OfferAppType,
        //                    ApplyOn = billDiscountOfferDc.ApplyOn,
        //                    WalletType = billDiscountOfferDc.WalletType,
        //                    OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new OfferBillDiscountItemDc
        //                    {
        //                        CategoryId = y.CategoryId,
        //                        Id = y.Id,
        //                        IsInclude = y.IsInclude,
        //                        SubCategoryId = y.SubCategoryId
        //                    }).ToList(),
        //                    OfferItems = billDiscountOfferDc.OfferItems.Select(y => new OfferItemdc
        //                    {
        //                        IsInclude = y.IsInclude,
        //                        itemId = y.itemId
        //                    }).ToList(),
        //                    RetailerBillDiscountFreeItemDcs = null
        //                };

        //                FinalBillDiscount.Add(bdcheck);
        //            }
        //        }
        //        res = new OfferdataDc()
        //        {
        //            offer = FinalBillDiscount,
        //            Status = true,
        //            Message = "Success"
        //        };
        //        return res;
        //    }

        //}
        //[Route("SalesHomePageGetSubSubCategories")]
        //[HttpGet]
        //public async Task<CatScatSscatDCs> SalesHomePageGetSubSubCategories(string lang, int subCategoryId, int PeopleId, int warehouseId)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        RetailerAppManager retailerAppManager = new RetailerAppManager();
        //        List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = GetCatSubCatwithStores(PeopleId);
        //        List<Category> Cat = new List<Category>();
        //        List<SubCategory> Scat = new List<SubCategory>();
        //        List<SubsubCategory> SsCat = new List<SubsubCategory>();
        //        try
        //        {
        //            var subCategoryQuery = "select SubCategoryId, 0 Categoryid,  '' CategoryName, [SubCategoryId],  (Case when '" + lang + "'='hi' and ( HindiName is not null or HindiName='') then HindiName else SubcategoryName end) SubcategoryName ,[LogoUrl],[itemcount],StoreBanner from SubCategories where IsActive=1 and Deleted=0 and SubCategoryId=" + subCategoryId;

        //            var brandQuery = "Exec GetRetailerBrandBySubCategoryId " + warehouseId + "," + subCategoryId + "," + lang;
        //            var Scatv = db.Database.SqlQuery<SubCategoryDCs>(subCategoryQuery).ToList();
        //            var SsCatv = db.Database.SqlQuery<SubsubCategoryDcs>(brandQuery).ToList();

        //            CatScatSscatDCs CatScatSscatcdc = new CatScatSscatDCs
        //            {
        //                subCategoryDC = Mapper.Map(Scatv).ToANew<List<SubCategoryDCs>>(),
        //                subsubCategoryDc = Mapper.Map(SsCatv).ToANew<List<SubsubCategoryDcs>>(),
        //            };


        //            #region block Barnd
        //            var custtype = 4;
        //            var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
        //            if (blockBarnds != null && blockBarnds.Any())
        //            {
        //                CatScatSscatcdc.subsubCategoryDc = CatScatSscatcdc.subsubCategoryDc.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
        //                CatScatSscatcdc.subCategoryDC = CatScatSscatcdc.subCategoryDC.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId))).ToList();
        //            }
        //            #endregion


        //            if (CatScatSscatcdc.subsubCategoryDc != null && CatScatSscatcdc.subsubCategoryDc.Any())
        //            {

        //                List<string> strCondition = StoreCategorySubCategoryBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId + "-" + x.BrandId).Distinct().ToList();
        //                List<string> companyStrCondition = StoreCategorySubCategoryBrands.Select(x => x.Categoryid + "-" + x.SubCategoryId).Distinct().ToList();


        //                CatScatSscatcdc.subsubCategoryDc = CatScatSscatcdc.subsubCategoryDc.Where(x => !(strCondition.Contains(x.Categoryid + " " + x.SubCategoryId + " " + x.SubsubCategoryid))).ToList();
        //                CatScatSscatcdc.subCategoryDC = CatScatSscatcdc.subCategoryDC.Where(x => !(companyStrCondition.Contains(x.Categoryid + " " + x.SubCategoryId))).ToList();

        //            }

        //            return CatScatSscatcdc;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in SalesHomePageGetSubSubCategories " + ex.Message);
        //            return null;
        //        }
        //    }
        //}
        //[Route("SalesGetItembySubCatAndBrand")]
        //[HttpGet]
        //public async Task<ItemListDc> SalesGetItembySubCatAndBrand(string lang, int PeopleId, int warehouseId, int scatid, int sscatid)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        List<ItemListDc> brandItem = new List<ItemListDc>();
        //        ItemListDc item = new ItemListDc();
        //        List<ItemDataDC> newdata = new List<ItemDataDC>();

        //        if (context.Database.Connection.State != ConnectionState.Open)
        //            context.Database.Connection.Open();


        //        var cmd = context.Database.Connection.CreateCommand();
        //        cmd.CommandText = "[dbo].[GetItemBySubCatAndBrand]";
        //        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
        //        cmd.Parameters.Add(new SqlParameter("@sscatid", sscatid));
        //        cmd.Parameters.Add(new SqlParameter("@scatid", scatid));
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //        // Run the sproc
        //        var reader = cmd.ExecuteReader();
        //        newdata = ((IObjectContextAdapter)context)
        //        .ObjectContext
        //        .Translate<ItemDataDC>(reader).ToList();


        //        RetailerAppManager retailerAppManager = new RetailerAppManager();
        //        #region block Barnd
        //        var custtype = 4;
        //        var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 2, warehouseId);
        //        if (blockBarnds != null && blockBarnds.Any())
        //        {
        //            newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
        //        }
        //        #endregion

        //        foreach (var it in newdata)
        //        {
        //            it.dreamPoint = it.dreamPoint.HasValue ? it.dreamPoint : 0;
        //            it.marginPoint = it.marginPoint.HasValue ? it.marginPoint : 0;
        //            if (!it.IsOffer)
        //            {
        //                /// Dream Point Logic && Margin Point
        //                int? MP, PP;
        //                double xPoint = xPointValue * 10;
        //                //salesman 0.2=(0.02 * 10=0.2)
        //                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                {
        //                    PP = 0;
        //                }
        //                else
        //                {
        //                    PP = it.promoPerItems;
        //                }
        //                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                {
        //                    MP = 0;
        //                }
        //                else
        //                {
        //                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                }
        //                if (PP > 0 && MP > 0)
        //                {
        //                    int? PP_MP = PP + MP;
        //                    it.dreamPoint = PP_MP;
        //                }
        //                else if (MP > 0)
        //                {
        //                    it.dreamPoint = MP;
        //                }
        //                else if (PP > 0)
        //                {
        //                    it.dreamPoint = PP;
        //                }
        //                else
        //                {
        //                    it.dreamPoint = 0;
        //                }
        //                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                if (it.price > it.UnitPrice)
        //                {
        //                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                }
        //                else
        //                {
        //                    it.marginPoint = 0;
        //                }
        //            }

        //            if (lang.Trim() == "hi")
        //            {
        //                if (!string.IsNullOrEmpty(it.HindiName))
        //                {
        //                    if (it.IsSensitive == true && it.IsSensitiveMRP == true)
        //                    {
        //                        it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM;
        //                    }
        //                    else if (it.IsSensitive == true && it.IsSensitiveMRP == false)
        //                    {
        //                        it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name 
        //                    }

        //                    else if (it.IsSensitive == false && it.IsSensitiveMRP == false)
        //                    {
        //                        it.itemname = it.HindiName; //item display name
        //                    }
        //                    else if (it.IsSensitive == false && it.IsSensitiveMRP == true)
        //                    {
        //                        it.itemname = it.HindiName + " " + it.price + " MRP";//item display name 
        //                    }
        //                }
        //            }
        //        }

        //        item.ItemMasters = new List<ItemDataDC>();
        //        item.ItemMasters.AddRange(newdata);

        //        ItemListDc res = new ItemListDc();
        //        if (item.ItemMasters != null && item.ItemMasters.Any())
        //        {
        //            res.Status = true;
        //            res.Message = "Success";
        //            res.ItemMasters = item.ItemMasters;
        //            return res;
        //        }
        //        else
        //        {
        //            res.Status = false;
        //            res.Message = "Failed";
        //            return res;
        //        }
        //    }
        //}
        //private List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> GetCatSubCatwithStores(int peopleid)
        //{
        //    List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> results = new List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand>();
        //    using (var context = new AuthContext())
        //    {
        //        var query = string.Format("select Count(distinct p.PeopleID) from People p inner join AspNetUsers a on a.Email = p.Email and p.Active=1 and p.Deleted=0 inner join AspNetUserRoles ur on a.id = ur.UserId and ur.isActive=1 inner join AspNetRoles r on ur.RoleId=r.Id and r.Name like '%sales lead%' and p.PeopleID={0}", peopleid);
        //        var isSalesLead = context.Database.SqlQuery<int>(query).FirstOrDefault();
        //        List<long> storeids = new List<long>();
        //        if (isSalesLead > 0)
        //            storeids = context.StoreDB.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.Id).ToList();
        //        else
        //            storeids = context.ClusterStoreExecutives.Where(x => x.ExecutiveId == peopleid && x.IsDeleted == false && x.IsActive).Select(x => x.StoreId).Distinct().ToList();

        //        RetailerAppManager retailerAppManager = new RetailerAppManager();
        //        List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();

        //        results = StoreCategorySubCategoryBrands.Where(x => storeids.Contains(x.StoreId)).ToList();
        //    }
        //    return results;
        //}
        //#endregion



        //[Route("UpdateProfileImage")]
        //[HttpPost]
        //public async Task<bool> UpdateProfileImage(UpdateSalesManProfileImageDc obj)
        //{
        //    bool result = false;
        //    if (obj != null && obj.PeopleId > 0 && obj.ProfilePic != null)
        //    {
        //        using (var db = new AuthContext())
        //        {
        //            var person = await db.Peoples.Where(u => u.PeopleID == obj.PeopleId).FirstOrDefaultAsync();
        //            person.ProfilePic = obj.ProfilePic;
        //            person.UpdatedDate = DateTime.Now;
        //            db.Entry(person).State = EntityState.Modified;
        //            result = db.Commit() > 0;
        //        }
        //    }
        //    return result;
        //}

        //[Route("GetallNotification")]
        //[HttpGet]
        //public PaggingDatas GetallNotification(int skip, int take, int PeopleId)
        //{
        //    int ApproveTimeLeft = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);

        //    using (var context = new AuthContext())
        //    {
        //        DateTime dt1 = DateTime.Now;
        //        PaggingDatas data = new PaggingDatas();
        //        context.Database.CommandTimeout = 600;
        //        SalesAppManager manager = new SalesAppManager();
        //        skip = (take - 1) * skip;
        //        var PeopleSentNotificationDc = manager.GetPeopleSentNotificationDetail(skip, take, PeopleId);
        //        //    var query = "[Operation].[GetPeopleNotification] " + PeopleId.ToString() + "," + ((take - 1) * skip).ToString() + "," + take;
        //        //    var PeopleSentNotificationDc = context.Database.SqlQuery<PeopleSentNotificationDc>(query).ToList();
        //        PeopleSentNotificationDc.ForEach(x =>
        //        {
        //            x.TimeLeft = x.TimeLeft.AddMinutes(ApproveTimeLeft); // from Create date

        //                if (!string.IsNullOrEmpty(x.Shopimage) && !x.Shopimage.Contains("http"))
        //            {
        //                x.Shopimage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
        //                                                      , HttpContext.Current.Request.Url.DnsSafeHost
        //                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
        //                                                      , x.Shopimage);
        //            }
        //        });
        //        data.notificationmaster = PeopleSentNotificationDc;
        //        data.total_count = PeopleSentNotificationDc != null && PeopleSentNotificationDc.Any() ? PeopleSentNotificationDc.FirstOrDefault().TotalCount : 0;
        //        return data;
        //    }
        //}

        //[Route("NotificationApprove")]
        //[HttpGet]
        //public async Task<bool> NotificationApprove(int Id, int PeopleId, bool IsNotificationApproved)
        //{

        //    using (var context = new AuthContext())
        //    {
        //        ConfigureNotifyHelper helepr = new ConfigureNotifyHelper();
        //        bool IsUpdate = await helepr.IsNotificationApproved(Id, PeopleId, IsNotificationApproved, context);

        //        //Action CallList pending 
        //        return IsUpdate;
        //    }
        //}

        //[Route("NotApprovedNotification")]
        //[HttpGet]
        //public async Task<List<PeopleSentNotificationDc>> NotApprovedNotification(int PeopleId)
        //{
        //    int ApproveTimeLeft = Convert.ToInt32(ConfigurationManager.AppSettings["ApproveNotifyTimeLeftInMinute"]);

        //    DateTime dt1 = DateTime.Now;
        //    using (var context = new AuthContext())
        //    {
        //        //var param = new SqlParameter("PeopleId", PeopleId);
        //        //var NotApprovedList = await context.Database.SqlQuery<PeopleSentNotificationDc>("exec Operation.NotApprovedNotification @PeopleId", param).ToListAsync();
        //        SalesAppManager manager = new SalesAppManager();
        //        var NotApprovedList = manager.NotApprovedNotificationManager(PeopleId);
        //        NotApprovedList.ForEach(x =>
        //        {
        //            x.TimeLeft = x.TimeLeft.AddMinutes(ApproveTimeLeft); // from Create date
        //                if (!string.IsNullOrEmpty(x.Shopimage) && !x.Shopimage.Contains("http"))
        //            {
        //                x.Shopimage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
        //                                                      , HttpContext.Current.Request.Url.DnsSafeHost
        //                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
        //                                                      , x.Shopimage);
        //            }
        //        });
        //        return NotApprovedList;
        //    }
        //}


        //private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        string sOTP = String.Empty;
        //        string sTempChars = String.Empty;
        //        Random rand = new Random();

        //        for (int i = 0; i < iOTPLength; i++)
        //        {
        //            int p = rand.Next(0, saAllowedCharacters.Length);
        //            sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
        //            sOTP += sTempChars;
        //        }
        //        return sOTP;
        //    }
        //}
        //[Route("Genotp")]
        //[HttpGet]
        //[AllowAnonymous]

        //public OTP Getotp(string MobileNumber, bool type, string mode = "")
        //{
        //    string Apphash = "";
        //    bool TestUser = false;
        //    OTP b = new OTP();
        //    List<string> CustomerStatus = new List<string>();
        //    CustomerStatus.Add("Not Verified");
        //    CustomerStatus.Add("Pending For Submitted");
        //    CustomerStatus.Add("Pending For Activation");
        //    CustomerStatus.Add("Temporary Active");
        //    using (var context = new AuthContext())
        //    {
        //        if (!type)
        //        {
        //            Customer cust = context.Customers.Where(c => c.Mobile.Trim().Equals(MobileNumber.Trim()) && !c.Deleted).FirstOrDefault();
        //            if (cust != null)
        //            {
        //                TestUser = cust.CustomerCategoryId.HasValue && cust.CustomerCategoryId.Value == 0;
        //                b = new OTP()
        //                {
        //                    Status = false,
        //                    Message = "This mobile no already registered."
        //                };
        //                return b;
        //            }
        //        }
        //    }
        //    string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        //    string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
        //    //string OtpMessage = " is Your Shopkirana Verification Code. :)";

        //    if (string.IsNullOrEmpty(Apphash))
        //    {
        //        Apphash = ConfigurationManager.AppSettings["Apphash"];
        //    }

        //    string OtpMessage = string.Format("<#> {0} : is Your Shopkirana Verification Code for complete process.{1}{2} Shopkirana", sRandomOTP, Environment.NewLine, Apphash);
        //    //string message = sRandomOTP + " :" + OtpMessage;
        //    string message = OtpMessage;
        //    var status = Common.Helpers.SendSMSHelper.SendSMS(MobileNumber, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString());
        //    //if (status)
        //    //{
        //    MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
        //    Model.CustomerOTP.RetailerCustomerOTP CustomerOTP = new Model.CustomerOTP.RetailerCustomerOTP
        //    {
        //        CreatedDate = DateTime.Now,
        //        DeviceId = "",
        //        IsActive = true,
        //        Mobile = MobileNumber,
        //        Otp = sRandomOTP
        //    };
        //    mongoDbHelper.Insert(CustomerOTP);
        //    //}


        //    OTP a = new OTP()
        //    {
        //        OtpNo = TestUser || (!string.IsNullOrEmpty(mode) && mode == "debug") ? sRandomOTP : "",
        //        Status = true,
        //        Message = "Successfully sent OTP."
        //    };
        //    return a;
        //}



        //[Route("CheckOTP")]
        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<HttpResponseMessage> CheckOTP(SalesCustomerRegistor otpCheckDc)
        //{
        //    MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP> mongoDbHelper = new MongoDbHelper<Model.CustomerOTP.RetailerCustomerOTP>();
        //    var cartPredicate = PredicateBuilder.New<Model.CustomerOTP.RetailerCustomerOTP>(x => x.Mobile == otpCheckDc.MobileNumber);

        //    var CustomerOTPs = mongoDbHelper.Select(cartPredicate).ToList();
        //    if (CustomerOTPs != null && CustomerOTPs.Any(x => x.Otp == otpCheckDc.Otp))
        //    {
        //        foreach (var item in CustomerOTPs)
        //        {
        //            await mongoDbHelper.DeleteAsync(item.Id);
        //        }

        //        using (var context = new AuthContext())
        //        {
        //            People people = context.Peoples.Where(q => q.PeopleID == otpCheckDc.PeopleId).FirstOrDefault();
        //            var cust = context.Customers.Where(x => x.Deleted == false && x.Mobile == otpCheckDc.MobileNumber).FirstOrDefault();
        //            Cluster dd = null;
        //            if (cust != null)
        //            {
        //                cust.Skcode = skcode();
        //                cust.ShopName = otpCheckDc.ShopName;
        //                cust.Shopimage = otpCheckDc.Shopimage;
        //                cust.Mobile = otpCheckDc.MobileNumber;
        //                cust.Active = false;
        //                cust.Deleted = false;
        //                cust.CreatedBy = people.DisplayName;
        //                cust.CreatedDate = indianTime;
        //                cust.lat = otpCheckDc.lat;
        //                cust.lg = otpCheckDc.lg;
        //                cust.Shoplat = otpCheckDc.lat;
        //                cust.Shoplg = otpCheckDc.lg;
        //                #region to assign cluster ID and determine if it is in cluster or not.

        //                if (cust.lat != 0 && cust.lg != 0)
        //                {
        //                    var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(cust.lat).Append("', '").Append(cust.lg).Append("')");
        //                    var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
        //                    if (!clusterId.HasValue)
        //                    {
        //                        cust.InRegion = false;
        //                    }
        //                    else
        //                    {
        //                        var agent = context.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active);

        //                        if (agent != null && agent.AgentId > 0)
        //                            cust.AgentCode = Convert.ToString(agent.AgentId);


        //                        cust.ClusterId = clusterId;
        //                        dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
        //                        cust.ClusterName = dd.ClusterName;
        //                        cust.InRegion = true;
        //                    }
        //                }
        //                #endregion

        //                if (dd != null)
        //                {
        //                    cust.Warehouseid = dd.WarehouseId;
        //                    cust.WarehouseName = dd.WarehouseName;
        //                    cust.ClusterId = dd.ClusterId;
        //                    cust.ClusterName = dd.ClusterName;
        //                    cust.Cityid = dd.CityId;
        //                    cust.City = dd.CityName;
        //                    cust.ShippingCity = dd.CityName;
        //                    cust.IsCityVerified = true;
        //                }
        //                context.Entry(cust).State = EntityState.Modified;
        //                context.Commit();
        //            }
        //            else
        //            {
        //                cust = new Customer();
        //                cust.Skcode = skcode();
        //                cust.ShopName = otpCheckDc.ShopName;
        //                cust.Shopimage = otpCheckDc.Shopimage;
        //                cust.Mobile = otpCheckDc.MobileNumber;
        //                cust.Active = false;
        //                cust.Deleted = false;
        //                cust.CreatedBy = people.DisplayName;
        //                cust.CreatedDate = indianTime;
        //                cust.UpdatedDate = indianTime;
        //                cust.lat = otpCheckDc.lat;
        //                cust.lg = otpCheckDc.lg;
        //                cust.Shoplat = otpCheckDc.lat;
        //                cust.Shoplg = otpCheckDc.lg;
        //                cust.CompanyId = 1;
        //                #region to assign cluster ID and determine if it is in cluster or not.

        //                if (cust.lat != 0 && cust.lg != 0)
        //                {
        //                    var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(cust.lat).Append("', '").Append(cust.lg).Append("')");
        //                    var clusterId = context.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
        //                    if (!clusterId.HasValue)
        //                    {
        //                        cust.InRegion = false;
        //                    }
        //                    else
        //                    {
        //                        var agent = context.ClusterAgent.FirstOrDefault(x => x.ClusterId == clusterId && x.active);

        //                        if (agent != null && agent.AgentId > 0)
        //                            cust.AgentCode = Convert.ToString(agent.AgentId);


        //                        cust.ClusterId = clusterId;
        //                        dd = context.Clusters.Where(x => x.ClusterId == clusterId).FirstOrDefault();
        //                        cust.ClusterName = dd.ClusterName;
        //                        cust.InRegion = true;
        //                    }
        //                }
        //                #endregion

        //                if (dd != null)
        //                {
        //                    cust.Warehouseid = dd.WarehouseId;
        //                    cust.WarehouseName = dd.WarehouseName;
        //                    cust.ClusterId = dd.ClusterId;
        //                    cust.ClusterName = dd.ClusterName;

        //                    cust.Cityid = dd.CityId;
        //                    cust.City = dd.CityName;
        //                    cust.ShippingCity = dd.CityName;
        //                    cust.IsCityVerified = true;
        //                }
        //                context.Customers.Add(cust);
        //            }
        //            context.Commit();

        //            var res = new
        //            {
        //                SkCode = cust.Skcode,
        //                Status = true,
        //                Message = "OTP Verify Successfully."
        //            };
        //            return Request.CreateResponse(HttpStatusCode.OK, res);
        //        }
        //    }
        //    else
        //    {
        //        var res = new
        //        {
        //            SkCode = "",
        //            Status = false,
        //            Message = "Please enter correct OTP."
        //        };
        //        return Request.CreateResponse(HttpStatusCode.OK, res);
        //    }
        //}

        //#region SKCode genrate Function.
        ///// <summary>
        ///// Created by 19/12/2018 
        ///// Get New Skcode function
        ///// </summary>
        ///// <returns></returns>
        //public string skcode()
        //{
        //    using (AuthContext db = new AuthContext())
        //    {
        //        var query = "select max(cast(replace(skcode,'SK','') as bigint)) from customers ";
        //        var intSkCode = db.Database.SqlQuery<long>(query).FirstOrDefault();
        //        var skcode = "SK" + (intSkCode + 1);
        //        bool flag = false;
        //        while (flag == false)
        //        {
        //            var check = db.Customers.Any(s => s.Skcode.Trim().ToLower() == skcode.Trim().ToLower());

        //            if (!check)
        //            {
        //                flag = true;
        //                return skcode;
        //            }
        //            else
        //            {
        //                intSkCode += 1;
        //                skcode = "SK" + intSkCode;
        //            }
        //        }

        //        return skcode;
        //    }
        //}
        //#endregion
        //[Route("GetPeopleReferralConfigurations")]
        //[HttpGet]
        //public List<GetCustReferralConfigDc> GetPeopleReferralConfigurations(int CityId)
        //{
        //    List<GetCustReferralConfigDc> custReferralConfigList = new List<GetCustReferralConfigDc>();
        //    using (var db = new AuthContext())
        //    {
        //        custReferralConfigList = db.CustomerReferralConfigurationDb.Where(x => x.CityId == CityId && x.ReferralType == 2 && x.IsActive == true && x.IsDeleted == false)
        //             .Select(x => new GetCustReferralConfigDc
        //             {
        //                 OnOrder = x.OnOrder,
        //                 ReferralWalletPoint = x.ReferralWalletPoint,
        //                 CustomerWalletPoint = x.CustomerWalletPoint,
        //                 OnDeliverd = x.OnDeliverd
        //             }).ToList();
        //        var statusids = custReferralConfigList.Select(x => x.OnDeliverd).Distinct().ToList();
        //        var customerReferralStatus = db.CustomerReferralStatusDb.Where(x => statusids.Contains((int)x.Id) && x.IsActive == true && x.IsDeleted == false).ToList();
        //        custReferralConfigList.ForEach(x =>
        //        {
        //            x.OrderCount = x.OnOrder + " Order";
        //            x.orderStatus = customerReferralStatus != null ? customerReferralStatus.FirstOrDefault(y => y.Id == x.OnDeliverd).OrderStatus : "NA";
        //        });
        //        return custReferralConfigList;
        //    }
        //}
        //[Route("GetPeopleReferralOrderList")]
        //[HttpGet]
        //public List<GetPeopleReferralOrderListDc> GetPeopleReferralOrderList(int PeopleId)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        var peopleId = new SqlParameter("@PeopleId", PeopleId);
        //        List<GetPeopleReferralOrderListDc> PeopleReferralList = context.Database.SqlQuery<GetPeopleReferralOrderListDc>("exec GetPeopleReferralOrderList @PeopleId", peopleId).ToList();
        //        return PeopleReferralList;
        //    }
        //}
        //#endregion
    }

    #region SalesAppItem controller class

    public class tblCustomerExecutiveMapping
    {
        [Key]
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public int CustomerId { get; set; }
        //public int ExecutiveId { get; set; }
        public int? Beat { get; set; }
        public string Day { get; set; }
        public long StoreId { get; set; }
        public int SkipDays { get; set; }
        public int SkipWeeks { get; set; }
        public DateTime StartDate { get; set; }
        public string EvenOrOddWeek { get; set; }

    }

    public class CategorySalesAppDc
    {
        public List<BaseCategory> Basecats { get; set; }
        public List<Category> Categories { get; set; }
        public List<SubCategories> SubCategories { get; set; }
        public List<SubSubCategories> SubSubCategories { get; set; }
    }
    public class OnBaseSalesAppDc
    {
        public List<Category> Categorys { get; set; }
        public List<SubsubCategory> SubsubCategorys { get; set; }
        public List<SubCategory> SubCategorys { get; set; }
    }


    public class CalelogDc
    {
        public List<SalesCategory> SalesCategories { get; set; }
        public List<SalesCompany> SalesCompanies { get; set; }
        public List<SalesBrand> SalesBrands { get; set; }
        public List<SalesCompany> BrandCompanies { get; set; }
        public List<SalesBrand> Brands { get; set; }
    }

    public class SalesCategory
    {
        public int BaseCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string CategoryName { get; set; }
        public string LogoUrl { get; set; }
        public string HindiName { get; set; }
        public string CategoryImg { get; set; }
        public int itemcount { get; set; }
    }


    public class SalesCompany
    {
        public int SubCategoryId { get; set; }
        public int Categoryid { get; set; }
        public string SubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }
        public int Sequence { get; set; }
        public int itemcount { get; set; }
    }

    public class SalesBrand
    {
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }
        public int Categoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public string HindiName { get; set; }
        public string LogoUrl { get; set; }       
        public int itemcount { get; set; }
    }

    public class ExecuteCluster
    {
        public int ClusterId { get; set; }
        public int ExecutiveId { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int StoreId { get; set; }

    }
    //new dc  MBD 
    public class SalesIntentRequestDC
    {
        public long ItemForecastDetailId { get; set; }

        public ItemForecastDetail ItemForecastDetail { get; set; }

        public int PeopleId { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int RequestQty { get; set; }
        public double RequestPrice { get; set; }
        public int SalesLeadApproveID { get; set; }
        public DateTime? SalesApprovedDate { get; set; }
        public int BuyerApproveID { get; set; }
        public DateTime? BuyerApprovedDate { get; set; }
        public int Status { get; set; }  // Pending for Lead = 0, Pending for buyer = 1, Rejected = 2, Approved = 3  
        public int Warehouseid { get; set; }
        public int CreatedBy { get; set; }
        public int? MinOrderQty { get; set; } //New Change
        public int? NoOfSet { get; set; } //New Change
        public string ETADate { get; set; } //New Change


    }
    //end DC MBD

    //new dc MBD2 
    public class SalesIntentHistoryDC
    {
        public string itemBaseName { get; set; }

        public bool IsSensitive { get; set; }

        public bool IsSensitiveMRP { get; set; }

        public string LogoUrl { get; set; }

        public double MRP { get; set; }
        public int ItemMultiMRPId { get; set; }
        public int RequestQty { get; set; }
        public double RequestPrice { get; set; }
        public int SystemSuggestedQty { get; set; }
        public int Warehouseid { get; set; }
        public int Status { get; set; }

        public bool Deleted { get; set; }

        public bool IsDisContinued { get; set; }

        public int SubsubCategoryid { get; set; }

        public int SubCategoryId { get; set; }

        public int Categoryid { get; set; }


        public string Number { get; set; }

        public string UnitofQuantity { get; set; }

        public string UOM { get; set; }

        public string HindiName { get; set; }

    }
    //end DC MBD2
    #endregion

    #region SalesAppReports class

    public class StoreSalesDc
    {
        public double Storesale { get; set; }
        public string StoreName { get; set; }
        public long StoreOrderCount { get; set; }
    }
    internal class orderDataDC
    {
        public double sale { get; set; }
        public long OrderCount { get; set; }
        public long OrderCustomerCount { get; set; }
        public double Storesale { get; set; }
        public string StoreName { get; set; }
        public long StoreOrderCount { get; set; }
    }

    internal class SaleDC
    {
        public double Sale { get; set; }
        public string SubsubcategoryName { get; set; }

    }
    internal class SubCatgeoryorderDataDC
    {
        public double Sale { get; set; }
        public string BrandName { get; set; }

    }


    internal class OrderSummaryDc
    {
        public int PendingCount { get; set; }
        public double PendingAmount { get; set; }
        public int InProcessCount { get; set; }
        public double InProcessAmount { get; set; }
        public int CanceledCount { get; set; }
        public double CanceledAmount { get; set; }
        public int DeliveredCount { get; set; }
        public double DeliveredAmount { get; set; }
    }

    public class SalesTargetResponse
    {
        public double AchivePercent { get; set; }
        public List<SalesTargetCustomerItem> SalesTargetCustomerItems { get; set; }
    }

    public class SalesTargetCustomerItem
    {
        public int Achieveqty { get; set; }
        public int TargetQty { get; set; }

        public double Percent
        {
            get
            {

                return TargetQty > 0 ? Achieveqty * 100 / TargetQty : 0;
            }
        }

        public string ItemName { get; set; }
        public double price { get; set; }
        public int SubCategoryId { get; set; }
        public string SubcategoryName { get; set; }
    }


    public class CompanySalesTargetCustomer
    {
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public int TargetQty { get; set; }
        public int Achieveqty { get; set; }
    }

    public class SalesTargetItemDc
    {
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
    }
    public class PostSalesTargetItemDc
    {
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public long StoreId { get; set; }
        public int BaseQty { get; set; }
        public int? Id { get; set; } //use for Update case

    }

    public class SalesTargetListItemDc
    {
        public double MRP { get; set; }
        public string ItemName { get; set; }
        public string ItemNumber { get; set; }
        public int ItemMultiMrpId { get; set; }
        public long StoreId { get; set; }
        public int BaseQty { get; set; }
        public long Id { get; set; }

    }

    public class ExecutiveRetailer
    {
        public int month { get; set; }
        public int year { get; set; }
        public int CustomerCount { get; set; }
        public int RemainingCustomerCount { get; set; }
    }

    public class ExecutiveBrandPurchaseRetailer
    {
        public string SubcategoryName { get; set; }
        public int CustomerCount { get; set; }
        public int TargetCustomerCount { get; set; }
    }
    public class ItemMasterSalesDc
    {
        public int ItemId { get; set; }
        public string itemname { get; set; }
        public string SellingSku { get; set; }
    }
    #endregion

    //#region SalesApp controller Class
    //public class GetPeopleReferralOrderListDc
    //{
    //    public string PeopleName { get; set; }
    //    public string ShopName { get; set; }
    //    public string SkCode { get; set; }
    //    public string ReferralSkCode { get; set; }
    //    public double ReferralWalletPoint { get; set; }
    //    public double CustomerWalletPoint { get; set; }
    //    public int OnOrder { get; set; }
    //    public int OrderId { get; set; }
    //    public string Status { get; set; }
    //    public int IsUsed { get; set; }
    //}
    //public class UpdateSalesManProfileImageDc
    //{
    //    public int PeopleId { get; set; }
    //    public string ProfilePic { get; set; }
    //}
    //public class SalesCustomerRegistor
    //{
    //    public string MobileNumber { get; set; }
    //    public int PeopleId { get; set; }
    //    public string Otp { get; set; }
    //    public string ShopName { get; set; }
    //    public string Shopimage { get; set; }
    //    public double lat { get; set; }
    //    public double lg { get; set; }
    //}

    //#endregion
}
