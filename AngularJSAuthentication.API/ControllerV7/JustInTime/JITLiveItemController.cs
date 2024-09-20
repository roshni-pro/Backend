using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Results;
using AngularJSAuthentication.BusinessLayer.Managers.JustInTime;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.JustInTime;
using AngularJSAuthentication.DataContracts.ROC;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.JustInTime;
using AngularJSAuthentication.Model.SalesApp;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV7.JustInTime
{
    [RoutePrefix("api/JITLiveItem")]
    public class JITLiveItemController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region Get LiveItem/MultiMrp/OpenMoq Lists

        [HttpPost]
        [Route("GetLiveItems")]
        public async Task<List<GetLiveItemListDc>> GetLiveItemListAsync(LiveItemListFilterDc liveItemListFilterDc)
        {
            JITLiveItemManager manager = new JITLiveItemManager();
            return await manager.GetLiveItemListAsync(liveItemListFilterDc);

        }

        [HttpGet]
        [Route("GetMultiMrpList")]
        public async Task<List<ItemMultiMrpDc>> GetMultiMrpListAsync(int WarehouseId, string ItemNumber)
        {
            List<CalculatePurchasePriceDc> calculatePurchasePriceDcs = new List<CalculatePurchasePriceDc>();

            JITLiveItemManager manager = new JITLiveItemManager();
            var list = await manager.GetMultiMrpListAsync(WarehouseId, ItemNumber);
            using (AuthContext context = new AuthContext())
            {
                List<int> ItemMultiMrpIds = list.Select(x => x.ItemMultiMRPId).ToList();
                CalculatePurchasePriceHelper calculatePurchasePriceHelper = new CalculatePurchasePriceHelper();
                calculatePurchasePriceDcs = await calculatePurchasePriceHelper.GetCalculatePurchasePrice(context, WarehouseId, ItemMultiMrpIds);
            }
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            List<ItemWarehouseDc> itemWarehouseDcs = new List<ItemWarehouseDc>();
            var itemWarehouse = list.Select(x => new ItemWarehouseDc { WarehouseId = x.WarehouseId, ItemMultiMRPId = x.ItemMultiMRPId }).ToList();
            var list1 = await tripPlannerHelper.RocTagValueGet(itemWarehouse);
            foreach (var da in list)
            {
                da.Tag = list1.Where(x => x.ItemMultiMRPId == da.ItemMultiMRPId && x.WarehouseId == da.WarehouseId).Select(x => x.Tag).FirstOrDefault();
                da.WeightedPurchasePrice = calculatePurchasePriceDcs.Count > 0 ? calculatePurchasePriceDcs.Where(x => x.ItemMultiMrpId == da.ItemMultiMRPId && x.WarehouseId == da.WarehouseId).Select(x => x.PurchasePrice).FirstOrDefault() : 0;
            }


            return list;
        }

        [HttpPost]
        [Route("GetOpenMoqList")]
        public async Task<List<OpenMoqDc>> GetOpenMoqListAsync(OpenMoqFilterDc openMoqFilterDc)
        {
            List<CalculatePurchasePriceDc> calculatePurchasePriceDcs = new List<CalculatePurchasePriceDc>();

            JITLiveItemManager manager = new JITLiveItemManager();
            var list = await manager.GetOpenMoqListAsync(openMoqFilterDc);

            using (AuthContext context = new AuthContext())
            {
                List<int> ItemMultiMrpIds = list.Select(x => x.ItemMultiMrpId).ToList();
                CalculatePurchasePriceHelper calculatePurchasePriceHelper = new CalculatePurchasePriceHelper();
                calculatePurchasePriceDcs = await calculatePurchasePriceHelper.GetCalculatePurchasePrice(context, openMoqFilterDc.WarehouseId, ItemMultiMrpIds);
            }
            foreach (var da in list)
            {
                da.WeightedPurchasePrice = calculatePurchasePriceDcs.Count > 0 ? calculatePurchasePriceDcs.Where(x => x.ItemMultiMrpId == da.ItemMultiMrpId && x.WarehouseId == openMoqFilterDc.WarehouseId).Select(x => x.PurchasePrice).FirstOrDefault() : 0;
            }
            return list;
        }
        #endregion

        #region Get Brands/Suppliers/Depos

        [HttpGet]
        [Route("GetBrandListByWarehouseId")]
        public async Task<List<BrandListDc>> GetBrandListByWarehouseIdAsync()
        {
            JITLiveItemManager manager = new JITLiveItemManager();
            var list = await manager.GetBrandListByWarehouseIdAsync();
            return list;
        }

        [HttpGet]
        [Route("GetSuppliersByWarehouseId")]
        public List<Supplier> GetSuppliersByWarehouseId(int WarehouseId)
        {
            List<Supplier> ass = new List<Supplier>();
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                ass = db.Suppliers.Where(x => x.Deleted == false && x.CompanyId == compid && x.WarehouseId == WarehouseId).OrderByDescending(s => s.CreatedDate).ToList();
                return ass;

            }
        }

        [HttpGet]
        [Route("GetDeposByWarehouseId")]
        public List<DepoMaster> GetAllDepoByWarehouseId(int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                var depos = db.DepoMasters.Where(x => x.Deleted == false && x.WarehouseId == WarehouseId).ToList();
                return depos;
            }

        }

        #endregion

        #region Update JITLiveItem

        [HttpPost]
        [Route("UpdateJITLiveItem")]
        public async Task<APIResponse> UpdateJITLiveItem(List<UpdateJITLiveItemDc> UpdateJITLiveItems)
        {
            var result = new APIResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            List<string> Roles = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0 && UpdateJITLiveItems != null && UpdateJITLiveItems.Any())
            {
                using (AuthContext context = new AuthContext())
                {
                    var WarehouseId = UpdateJITLiveItems.FirstOrDefault().WarehouseId;
                    var QtyToLive = UpdateJITLiveItems.FirstOrDefault().QtyToLive;


                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Active && x.Deleted == false);
                    if (people == null)
                    {
                        result.Message = "you are not authorized";
                        return result;
                    }


                    int SupplierId = UpdateJITLiveItems.FirstOrDefault().SupplierId;
                    int DepoId = UpdateJITLiveItems.FirstOrDefault().DepoId;

                    var Supplier = context.Suppliers.FirstOrDefault(x => x.SupplierId == SupplierId);
                    if (Supplier == null)
                    {
                        result.Message = "Suplier Can't be null.";
                        return result;
                    }

                    var Depo = context.DepoMasters.FirstOrDefault(x => x.DepoId == DepoId && x.SupplierId == SupplierId);
                    if (Depo == null)
                    {
                        result.Message = "Depo Can't be null.";
                        return result;
                    }
                    var ItemIds = UpdateJITLiveItems.Select(x => x.ItemId).Distinct().ToList();
                    var ItemLists = context.itemMasters.Where(i => ItemIds.Contains(i.ItemId) && i.WarehouseId == WarehouseId && i.Deleted == false && i.IsDisContinued == false).ToList();

                    if (ItemLists == null)
                    {
                        result.Status = false;
                        result.Message = "Item Not Found";
                        return result;
                    }

                    #region ItemIncentiveClassificationMargin
                    var itemmultimrpids = new List<int>();
                    itemmultimrpids.Add(ItemLists.FirstOrDefault().ItemMultiMRPId);
                    JITLiveItemManager jitManager = new JITLiveItemManager();
                    OpenMoqFilterDc openMoqFilterDc = new OpenMoqFilterDc();
                    openMoqFilterDc.WarehouseId = WarehouseId;
                    openMoqFilterDc.ItemMultiMrpId = ItemLists.FirstOrDefault().ItemMultiMRPId;
                    var ItemIncentiveClassificationMargin = await jitManager.GetOpenMoqListAsync(openMoqFilterDc);

                    #endregion

                    var ItemMultiMrpId = UpdateJITLiveItems.FirstOrDefault().ItemMultiMrpId;
                    var ItemMultiMrp = context.ItemMultiMRPDB.FirstOrDefault(x => x.ItemMultiMRPId == ItemMultiMrpId);

                    var SubcatId = ItemLists.FirstOrDefault().SubCategoryId;
                    bool EnableAutoPrice = context.SubCategorys.FirstOrDefault(x => x.SubCategoryId == SubcatId).EnableAutoPrice;
                    var AllItemLists = context.itemMasters.Where(i => i.WarehouseId == WarehouseId && i.ItemMultiMRPId == ItemMultiMrpId && i.Deleted == false && i.IsDisContinued == false).ToList();
                    var activeItem = AllItemLists.Where(x => !ItemIds.Contains(x.ItemId) && x.active).ToList();
                    foreach (var UpdateJITLiveItem in UpdateJITLiveItems)
                    {
                        var item = ItemLists.Where(x => x.ItemId == UpdateJITLiveItem.ItemId).FirstOrDefault();
                        List<ItemMaster> itemMasterList = new List<ItemMaster>();

                        if (item != null)
                        {
                            if (Math.Round(item.PurchasePrice, 4) != UpdateJITLiveItem.PurchasePrice && EnableAutoPrice)
                            {
                                result.Status = false;
                                result.Message = "Purchase Price is different you cannot do the changes reload the page!!";
                                return result;
                            }
                            double unitPrice = item.UnitPrice;
                            double? tradeunitPrice = item.TradePrice;
                            double? wholesellerunitPrice = item.WholeSalePrice;
                            if (ItemIncentiveClassificationMargin != null && ItemIncentiveClassificationMargin.Any(x => UpdateJITLiveItem.ItemId == x.ItemId))
                            {
                                if ((float)UpdateJITLiveItem.Margin < (ItemIncentiveClassificationMargin.Any(x => x.RetailerRMargin > 0 && UpdateJITLiveItem.ItemId == x.ItemId) ? ItemIncentiveClassificationMargin.FirstOrDefault(x => x.RetailerRMargin > 0 && UpdateJITLiveItem.ItemId == x.ItemId).RetailerRMargin : 0))
                                {
                                    result.Status = false;
                                    result.Message = "Margin can't be less than company MinMarginPercent for Retailer Item :" + item.itemname + "  FOR MOQ : " + item.MinOrderQty;
                                    return result;
                                }
                                if ((float)UpdateJITLiveItem.WholesaleMargin < (ItemIncentiveClassificationMargin.Any(x => x.WholesaleRMargin > 0 && UpdateJITLiveItem.ItemId == x.ItemId) ? ItemIncentiveClassificationMargin.FirstOrDefault(x => x.WholesaleRMargin > 0 && UpdateJITLiveItem.ItemId == x.ItemId).WholesaleRMargin : 0))
                                {
                                    result.Status = false;
                                    result.Message = "Wholesale Margin can't be less than company MinMarginPercent for Wholesaler Item  :" + item.itemname + "  FOR MOQ : " + item.MinOrderQty;
                                    return result;
                                }
                                if ((float)UpdateJITLiveItem.TradeMargin < (ItemIncentiveClassificationMargin.Any(x => x.TradeRMargin > 0 && UpdateJITLiveItem.ItemId == x.ItemId) ? ItemIncentiveClassificationMargin.FirstOrDefault(x => x.TradeRMargin > 0 && UpdateJITLiveItem.ItemId == x.ItemId).TradeRMargin : 0))
                                {
                                    result.Status = false;
                                    result.Message = "Trade Margin can't be less than company MinMarginPercent for  Trader   Item :" + item.itemname + "  FOR MOQ : " + item.MinOrderQty;
                                    return result;
                                }
                            }
                            if (item.POPurchasePrice != UpdateJITLiveItem.POPurchasePrice || item.POPurchasePrice == null)
                            {
                                item.POPurchasePrice = UpdateJITLiveItem.POPurchasePrice;
                            }
                            double withouttaxvalue = (UpdateJITLiveItem.PurchasePrice / ((100 + item.TotalTaxPercentage) / 100));
                            double withouttax = Math.Round(withouttaxvalue, 3);
                            double netDiscountAmountvalue = (withouttax * (UpdateJITLiveItem.Discount / 100));
                            double netDiscountAmount = Math.Round(netDiscountAmountvalue, 3);
                            item.NetPurchasePrice = Math.Round((withouttax - netDiscountAmount), 3);// without tax
                            item.WithTaxNetPurchasePrice = Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3);//With tax                                                                                            //Math.Round((withouttax - netDiscountAmount), 3);// with tax
                            item.Discount = UpdateJITLiveItem.Discount;
                            item.Margin = UpdateJITLiveItem.Margin;
                            item.TradeMargin = UpdateJITLiveItem.TradeMargin;
                            item.WholesaleMargin = UpdateJITLiveItem.WholesaleMargin;
                            item.PurchasePrice = UpdateJITLiveItem.PurchasePrice;
                            var value = UpdateJITLiveItem.PurchasePrice + (UpdateJITLiveItem.PurchasePrice * UpdateJITLiveItem.Margin / 100);
                            item.UnitPrice = Math.Round(value, 3);

                            var tradevalue = UpdateJITLiveItem.PurchasePrice + (UpdateJITLiveItem.PurchasePrice * UpdateJITLiveItem.TradeMargin / 100);
                            item.TradePrice = Math.Round(tradevalue, 3);

                            var wholesellervalue = UpdateJITLiveItem.PurchasePrice + (UpdateJITLiveItem.PurchasePrice * UpdateJITLiveItem.WholesaleMargin / 100);
                            item.WholeSalePrice = Math.Round(wholesellervalue, 3);

                            #region item price drop Entry for sales app
                            if (item.UnitPrice > 0 && unitPrice > 0 || item.WholeSalePrice > 0 && wholesellerunitPrice > 0 || item.TradePrice > 0 && tradeunitPrice > 0)
                            {
                                if (item.UnitPrice < unitPrice || item.WholeSalePrice < wholesellerunitPrice || item.TradePrice < tradeunitPrice)
                                {
                                    ItemPriceDrop itemPriceDrop = new ItemPriceDrop();
                                    if (item.UnitPrice < unitPrice)
                                    {
                                        itemPriceDrop.UnitPrice = item.UnitPrice;
                                        itemPriceDrop.OldUnitPrice = unitPrice;
                                    }
                                    if (item.WholeSalePrice > 0 && wholesellerunitPrice > 0)
                                    {
                                        if (item.WholeSalePrice < wholesellerunitPrice)
                                        {
                                            itemPriceDrop.DistributionPrice = item.WholeSalePrice;
                                            itemPriceDrop.OldDistributionPrice = wholesellerunitPrice;
                                        }
                                    }
                                    if (item.TradePrice > 0 && tradeunitPrice > 0)
                                    {
                                        if (item.TradePrice < tradeunitPrice)
                                        {
                                            itemPriceDrop.TradePrice = item.TradePrice;
                                            itemPriceDrop.OldTradePrice = tradeunitPrice;
                                        }
                                    }
                                    itemPriceDrop.WarehouseId = item.WarehouseId;
                                    itemPriceDrop.ItemId = item.ItemId;
                                    itemPriceDrop.IsActive = true;
                                    itemPriceDrop.IsDeleted = false;
                                    itemPriceDrop.CreatedBy = userid;
                                    itemPriceDrop.CreatedDate = DateTime.Now;
                                    context.ItemPriceDrops.Add(itemPriceDrop);
                                }
                            }
                            #endregion

                            item.UnitofQuantity = ItemMultiMrp.UnitofQuantity;
                            item.price = ItemMultiMrp.MRP;
                            item.MRP = ItemMultiMrp.MRP;
                            item.ItemMultiMRPId = ItemMultiMrp.ItemMultiMRPId;

                            if (item.IsSensitive == true && item.IsSensitiveMRP == true)
                            {
                                item.itemname = item.itemBaseName + " " + item.price + " MRP " + item.UnitofQuantity + " " + item.UOM;
                            }
                            else if (item.IsSensitive == true && item.IsSensitiveMRP == false)
                            {
                                item.itemname = item.itemBaseName + " " + item.UnitofQuantity + " " + item.UOM; //item display name 
                            }
                            else if (item.IsSensitive == false && item.IsSensitiveMRP == false)
                            {
                                item.itemname = item.itemBaseName; //item display name
                            }
                            else if (item.IsSensitive == false && item.IsSensitiveMRP == true)
                            {
                                item.itemname = item.itemBaseName + " " + item.price + " MRP";//item display name 
                            }
                            item.SellingUnitName = item.itemname + " " + item.MinOrderQty + "Unit";//item selling unit name
                            item.PurchaseUnitName = item.itemname + " " + item.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name


                            item.UpdatedDate = indianTime;
                            item.userid = userid;
                            item.active = UpdateJITLiveItem.Active;
                            item.UpdatedDate = indianTime;
                            item.SupplierId = Supplier.SupplierId;
                            item.SupplierName = Supplier.Name;
                            item.SUPPLIERCODES = Supplier.SUPPLIERCODES;
                            item.DepoId = Depo.DepoId;
                            item.DepoName = Depo.DepoName;

                            if (!(item.UnitPrice > item.price || item.TradePrice > item.price || item.WholeSalePrice > item.price))
                            {
                                context.Entry(item).State = EntityState.Modified;
                            }
                            //if(itemMasterList.Count > 0)
                            //{
                            //    context.itemMasters.AddRange(itemMasterList);
                            //}  

                        }
                        if (activeItem.Count > 0)
                        {
                            foreach (var acItem in activeItem)
                            {
                                acItem.active = false;
                                acItem.UpdatedDate = indianTime;
                                acItem.userid = userid;
                                context.Entry(acItem).State = EntityState.Modified;
                                //itemMasterList.Add(acItem);

                            }
                        }
                    }
                    result.Status = context.Commit() > 0;
                    result.Message = result.Status ? "record updated successfully." : "Something went wrong.";
                }
            }
            else
            {
                result.Status = false;
                result.Message = "User not found/Something went wrong.";
            }
            return result;
        }


        [HttpPost]
        [Route("UpdateRisk")]
        public async Task<APIResponse> UpdateRisk(List<UpdateRiskDc> UpdateRiskDc)
        {
            var result = new APIResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            List<string> Roles = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0 && UpdateRiskDc != null && UpdateRiskDc.Any())
            {
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Active && x.Deleted == false);
                        if (people == null)
                        {
                            result.Message = "you are not authorized";
                            return result;
                        }

                        var RiskQtyToReduce = new RiskQtyToReduceDc();

                        var multiMrpIdParam = new SqlParameter("@mrpid", UpdateRiskDc.FirstOrDefault().ItemMultiMrpId);
                        var widParam = new SqlParameter("@warehouseid", UpdateRiskDc.FirstOrDefault().WarehouseId);
                        RiskQtyToReduce = context.Database.SqlQuery<RiskQtyToReduceDc>("Sp_Grirdiff @mrpid,@warehouseid", multiMrpIdParam, widParam).FirstOrDefault();
                        if (UpdateRiskDc.Any(x => x.RiskQuantity < 0) && RiskQtyToReduce != null)
                        {
                            foreach (var item in UpdateRiskDc.Where(x => x.RiskQuantity < 0))
                            {
                                if (RiskQtyToReduce.InternalRiskQty > 0 && RiskQtyToReduce.InternalRiskQty < item.RiskQuantity * (-1))
                                {
                                    result.Status = false;
                                    result.Message = "you can change qty upto : " + RiskQtyToReduce.InternalRiskQty + " for  Internal";
                                    return result;
                                }
                                if (RiskQtyToReduce.PORiskQty > 0 && RiskQtyToReduce.PORiskQty < item.RiskQuantity * (-1))
                                {
                                    result.Status = false;
                                    result.Message = "you can change qty upto : " + RiskQtyToReduce.InternalRiskQty + " for  Internal";
                                    return result;
                                }
                            }
                        }

                        if (RiskQtyToReduce.EnableAutoPrice)
                        {
                            CalculatePurchasePriceHelper helper = new CalculatePurchasePriceHelper();
                            result.Status = helper.UpdateRisk(context, UpdateRiskDc, userid);
                        }
                        else
                        {
                            CalculatePurchasePriceHelper helper = new CalculatePurchasePriceHelper();
                            result.Status = helper.InsertUpdateRisk(context, UpdateRiskDc, userid);
                        }

                        if (result.Status)
                        {
                            dbContextTransaction.Complete();
                            result.Message = result.Status ? "record updated successfully." : "Something went wrong.";
                        }

                    }
                }
            }
            else
            {
                result.Status = false;
                result.Message = "User not found/Something went wrong.";
            }
            return result;
        }

        //[HttpPost]
        //[Route("UpdateRisk")]
        //public async Task<APIResponse> UpdateRisk(List<UpdateRiskDc> UpdateRiskDc)
        //{
        //    var result = new APIResponse();
        //    var identity = User.Identity as ClaimsIdentity;
        //    int userid = 0;
        //    List<string> Roles = new List<string>();
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    if (userid > 0 && UpdateRiskDc != null && UpdateRiskDc.Any())
        //    {

        //        CalculatePurchasePriceHelper helper = new CalculatePurchasePriceHelper();
        //        TransactionOptions option = new TransactionOptions();
        //        option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
        //        option.Timeout = TimeSpan.FromSeconds(90);
        //        using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
        //        {
        //            using (AuthContext context = new AuthContext())
        //            {
        //                var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Active && x.Deleted == false);
        //                if (people == null)
        //                {
        //                    result.Message = "you are not authorized";
        //                    return result;
        //                }
        //                //Validate 
        //                if (UpdateRiskDc.Any(x => x.RiskQuantity < 0))
        //                {
        //                    var multiMrpIdParam = new SqlParameter("@mrpid", UpdateRiskDc.FirstOrDefault().ItemMultiMrpId);
        //                    var widParam = new SqlParameter("@warehouseid", UpdateRiskDc.FirstOrDefault().WarehouseId);
        //                    var GRInternalIntransit = context.Database.SqlQuery<GRIRInternalInTransitDc>("Sp_Grirdiff @mrpid,@warehouseid", multiMrpIdParam, widParam).FirstOrDefault();

        //                    if (GRInternalIntransit.EnableAutoPrice)
        //                    {
        //                        if (UpdateRiskDc.Any(x => x.RiskType == 1 && x.RiskQuantity < 0) && GRInternalIntransit.InternalRiskQty < UpdateRiskDc.FirstOrDefault(x => x.RiskType == 1).RiskQuantity)
        //                        {
        //                            int t = UpdateRiskDc.FirstOrDefault(x => x.RiskType == 1).RiskQuantity - GRInternalIntransit.InternalRiskQty;
        //                            if (t < (UpdateRiskDc.FirstOrDefault(x => x.RiskType == 1).RiskQuantity < 0 ? UpdateRiskDc.FirstOrDefault(x => x.RiskType == 1).RiskQuantity * (-1) : UpdateRiskDc.FirstOrDefault(x => x.RiskType == 1).RiskQuantity))
        //                            {
        //                                result.Message = "You can change Internal Risk Quantity equal or less then " + t + " !!";
        //                                return result;
        //                            }
        //                        }
        //                        if (UpdateRiskDc.Any(x => x.RiskType == 0 && x.RiskQuantity < 0) && GRInternalIntransit.PORiskQty < UpdateRiskDc.FirstOrDefault(x => x.RiskType == 0).RiskQuantity)
        //                        {
        //                            int t = UpdateRiskDc.FirstOrDefault(x => x.RiskType == 0).RiskQuantity - GRInternalIntransit.PORiskQty;
        //                            if (t < (UpdateRiskDc.FirstOrDefault(x => x.RiskType == 0).RiskQuantity < 0 ? UpdateRiskDc.FirstOrDefault(x => x.RiskType == 0).RiskQuantity * (-1) : UpdateRiskDc.FirstOrDefault(x => x.RiskType == 0).RiskQuantity))
        //                            {
        //                                result.Message = "You can change PO Risk Quantity equal or less then " + t + " !!";
        //                                return result;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        result.Status = helper.InsertUpdateRisk(context, UpdateRiskDc, userid);
        //                        if (result.Status)
        //                        {
        //                            dbContextTransaction.Complete();
        //                            result.Message = result.Status ? "record updated successfully." : "Something went wrong.";
        //                        }
        //                        return result;
        //                    }
        //                }
        //                    #region InsertUpdateJitRiskQuantity
        //                    result.Status = helper.UpdateRisk(context, UpdateRiskDc, userid);
        //                    if (result.Status)
        //                    {
        //                        dbContextTransaction.Complete();
        //                        result.Message = result.Status ? "record updated successfully." : "Something went wrong.";
        //                    }
        //                    #endregion

        //            }
        //        }
        //    }
        //    else
        //    {
        //        result.Status = false;
        //        result.Message = "User not found/Something went wrong.";
        //    }
        //    return result;
        //}

        public async Task<int> GetGRIRDiff(int WarehouseId, int MultiMrpId)
        {
            using (var myContext = new AuthContext())
            {
                var multiMrpIdParam = new SqlParameter("@mrpid", MultiMrpId);
                var widParam = new SqlParameter("@warehouseid", WarehouseId);

                var result = myContext.Database.SqlQuery<int>("Sp_Grirdiff @mrpid,@warehouseid", multiMrpIdParam, widParam).FirstOrDefault();
                return result;
            }
        }

        [HttpPost]
        [Route("UpdateLimit")]
        public async Task<APIResponse> UpdateLimit(UpdateItemLimitDc UpdateRiskItemDc)
        {
            var result = new APIResponse();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            List<string> Roles = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0 && UpdateRiskItemDc != null)
            {
                using (AuthContext context = new AuthContext())
                {
                    var WarehouseId = UpdateRiskItemDc.WarehouseId;
                    var QtyToLive = UpdateRiskItemDc.QtyToLive;
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Active && x.Deleted == false);
                    if (people == null)
                    {
                        result.Message = "you are not authorized";
                        return result;
                    }
                    int SupplierId = UpdateRiskItemDc.SupplierId;
                    int DepoId = UpdateRiskItemDc.DepoId;

                    var Supplier = context.Suppliers.FirstOrDefault(x => x.SupplierId == SupplierId);
                    if (Supplier == null)
                    {
                        result.Message = "Suplier Can't be null.";
                        return result;
                    }

                    var Depo = context.DepoMasters.FirstOrDefault(x => x.DepoId == DepoId && x.SupplierId == SupplierId);
                    if (Depo == null)
                    {
                        result.Message = "Depo Can't be null.";
                        return result;
                    }
                    var ItemMultiMrpId = UpdateRiskItemDc.ItemMultiMrpId;
                    var ItemMultiMrp = context.ItemMultiMRPDB.FirstOrDefault(x => x.ItemMultiMRPId == ItemMultiMrpId);
                    List<ItemMaster> itemMaster = context.itemMasters.Where(x => x.ItemMultiMRPId == ItemMultiMrpId && x.WarehouseId == UpdateRiskItemDc.WarehouseId && x.IsDisContinued == false && x.Deleted == false).ToList();

                    CalculateItemPurchasePrice calculateItemPurchasePrice = new CalculateItemPurchasePrice();
                    calculateItemPurchasePrice = context.CalculateItemPurchasePrices.Where(x => x.ItemMultiMrpId == ItemMultiMrpId && x.WarehouseId == UpdateRiskItemDc.WarehouseId).OrderByDescending(x => x.Id).FirstOrDefault();
                    bool IspriceChange = false;
                    if (calculateItemPurchasePrice != null)
                    {
                        if (UpdateRiskItemDc.PurchasePrice != calculateItemPurchasePrice.PurchasePrice)
                        {
                            IspriceChange = true;
                            var addCalculateItemPurchasePrice = new CalculateItemPurchasePrice();
                            addCalculateItemPurchasePrice.PurchasePrice = UpdateRiskItemDc.PurchasePrice;
                            addCalculateItemPurchasePrice.WarehouseId = calculateItemPurchasePrice.WarehouseId;
                            addCalculateItemPurchasePrice.ItemMultiMrpId = calculateItemPurchasePrice.ItemMultiMrpId;
                            addCalculateItemPurchasePrice.RiskPurchasePrice = calculateItemPurchasePrice.RiskPurchasePrice;
                            addCalculateItemPurchasePrice.RiskQty = calculateItemPurchasePrice.RiskQty;
                            addCalculateItemPurchasePrice.IRQty = calculateItemPurchasePrice.IRQty;
                            addCalculateItemPurchasePrice.IRPrice = calculateItemPurchasePrice.IRPrice;
                            addCalculateItemPurchasePrice.CurrentStockQty = calculateItemPurchasePrice.CurrentStockQty;
                            addCalculateItemPurchasePrice.CurrentStockPrice = calculateItemPurchasePrice.CurrentStockPrice;
                            addCalculateItemPurchasePrice.CreatedDate = DateTime.Today;
                            addCalculateItemPurchasePrice.CreatedBy = userid;
                            addCalculateItemPurchasePrice.Comment = "Added Purchase Price Manually.";
                            addCalculateItemPurchasePrice.GeneratedDate = indianTime;
                            context.CalculateItemPurchasePrices.Add(addCalculateItemPurchasePrice);
                            //context.Entry(calculateItemPurchasePrice).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        IspriceChange = true;
                        CalculateItemPurchasePrice newCalculateItemPurchasePrice = new CalculateItemPurchasePrice();
                        newCalculateItemPurchasePrice.WarehouseId = UpdateRiskItemDc.WarehouseId;
                        newCalculateItemPurchasePrice.ItemMultiMrpId = UpdateRiskItemDc.ItemMultiMrpId;
                        newCalculateItemPurchasePrice.RiskPurchasePrice = UpdateRiskItemDc.RiskPurchasePrice;
                        newCalculateItemPurchasePrice.RiskQty = UpdateRiskItemDc.RiskQuantity;
                        newCalculateItemPurchasePrice.PurchasePrice = UpdateRiskItemDc.PurchasePrice;
                        newCalculateItemPurchasePrice.IRQty = 0;
                        newCalculateItemPurchasePrice.IRPrice = 0;
                        newCalculateItemPurchasePrice.CurrentStockQty = 0;
                        newCalculateItemPurchasePrice.CurrentStockPrice = 0;
                        newCalculateItemPurchasePrice.CreatedDate = DateTime.Today;
                        newCalculateItemPurchasePrice.CreatedBy = userid;
                        newCalculateItemPurchasePrice.Comment = "Added Purchase Price Manually.";
                        newCalculateItemPurchasePrice.GeneratedDate = indianTime;
                        context.CalculateItemPurchasePrices.Add(newCalculateItemPurchasePrice);
                    }

                    bool ItemValueChange = false;
                    foreach (var item in itemMaster)
                    {
                        ItemValueChange = false;
                        if (item.POPurchasePrice != UpdateRiskItemDc.POPurchasePrice || item.POPurchasePrice == null)
                        {
                            item.POPurchasePrice = UpdateRiskItemDc.POPurchasePrice;
                            ItemValueChange = true;
                        }

                        if (item.PurchasePrice == UpdateRiskItemDc.PurchasePrice)
                        {
                            IspriceChange = false;
                        }
                        else
                        {
                            IspriceChange = true;
                        }

                        if (item.SupplierId != Supplier.SupplierId || item.DepoId != Depo.DepoId)
                        {
                            ItemValueChange = true;
                            item.SupplierId = Supplier.SupplierId;
                            item.SupplierName = Supplier.Name;
                            item.SUPPLIERCODES = Supplier.SUPPLIERCODES;
                            item.DepoId = Depo.DepoId;
                            item.DepoName = Depo.DepoName;
                        }

                        

                        if (IspriceChange && (!(item.UnitPrice > item.price || item.TradePrice > item.price || item.WholeSalePrice > item.price)))
                        {                          
                            ItemValueChange = true;
                            item.PurchasePrice = UpdateRiskItemDc.PurchasePrice;
                            double withouttaxvalue = (item.PurchasePrice / ((100 + item.TotalTaxPercentage) / 100));
                            double withouttax = Math.Round(withouttaxvalue, 3);
                            double netDiscountAmountvalue = (withouttax * (item.Discount / 100));
                            double netDiscountAmount = Math.Round(netDiscountAmountvalue, 3);

                            item.NetPurchasePrice = Math.Round((withouttax - netDiscountAmount), 3);// without tax

                            item.WithTaxNetPurchasePrice = Math.Round(item.NetPurchasePrice * (1 + (item.TotalTaxPercentage / 100)), 3);//With tax                                                                                            //Math.Round((withouttax - netDiscountAmount), 3);// with tax

                            item.UnitPrice = Math.Round(item.PurchasePrice + (item.PurchasePrice * item.Margin / 100), 3);
                            item.TradePrice = Math.Round(item.PurchasePrice + (item.PurchasePrice * (item.TradeMargin ?? 0) / 100), 3);
                            item.WholeSalePrice = Math.Round(item.PurchasePrice + (item.PurchasePrice * (item.WholesaleMargin ?? 0) / 100), 3);
                            item.UpdatedDate = DateTime.Now;
                        }
                        if (ItemValueChange)
                        {
                            item.userid = userid;
                            item.UpdatedDate = indianTime;
                            context.Entry(item).State = EntityState.Modified;
                        }
                    }



                    #region ItemLimit
                    var itemlimit = context.ItemLimitMasterDB.FirstOrDefault(x => x.ItemMultiMRPId == ItemMultiMrpId && x.WarehouseId == WarehouseId);

                    if (itemlimit != null && itemlimit.ItemlimitQty!= QtyToLive && QtyToLive >= 0)
                    {
                        itemlimit.ItemlimitQty = QtyToLive;
                        itemlimit.IsItemLimit = true;
                        itemlimit.UpdateDate = DateTime.Now;
                        context.Entry(itemlimit).State = EntityState.Modified;
                    }
                    else if (itemlimit == null && QtyToLive >= 0)
                    {
                        ItemLimitMaster addlimit = new ItemLimitMaster();
                        addlimit.ItemNumber = ItemMultiMrp.ItemNumber;
                        addlimit.ItemMultiMRPId = ItemMultiMrpId;
                        addlimit.WarehouseId = WarehouseId;
                        addlimit.CreatedDate = indianTime;
                        addlimit.UpdateDate = indianTime;
                        addlimit.ItemlimitQty = QtyToLive;
                        addlimit.IsItemLimit = true;
                        context.ItemLimitMasterDB.Add(addlimit);
                    }
                    #endregion
                    result.Status = context.Commit() > 0;
                    result.Message = result.Status ? "record updated successfully." : "No Record Updated.";

                }
            }
            else
            {
                result.Status = false;
                result.Message = "User not found/Something went wrong.";
            }
            return result;
        }

        [HttpGet]
        [Route("GetRiskList")]
        public async Task<List<GetRiskItemListDc>> GetRiskList(int WarehouseId, int MultiMrpId)
        {
            using (AuthContext context = new AuthContext())
            {
                List<GetRiskItemListDc> getRiskItemListDcs = new List<GetRiskItemListDc>();
                var WarehouseIdParam = new SqlParameter("@WarehouseId", WarehouseId);
                var MultiMrpIdParam = new SqlParameter("@MultiMrpId", MultiMrpId);
                getRiskItemListDcs = context.Database.SqlQuery<GetRiskItemListDc>("GetRiskQtyList @WarehouseId,@MultiMrpId", WarehouseIdParam, MultiMrpIdParam).ToList();
                return getRiskItemListDcs;
            }
        }
        #endregion



        #region Job
        //[HttpGet]
        //[Route("ZeroRiskQuantityJob")]
        //[AllowAnonymous]
        //public bool ZeroRiskQuantityJob()
        //{
        //    using (AuthContext context = new AuthContext())
        //    {
        //        var date = DateTime.Today.AddDays(-3);
        //        var dataList = context.JITRiskQtys.Where(x => x.IsActive == true && x.RiskQuantity > 0 && x.IsDeleted == false && (x.ModifiedDate < date || (x.ModifiedDate == null && x.CreatedDate < date))).ToList();
        //        if (dataList != null && dataList.Any())
        //        {
        //            List<JITRiskQuantityHistory> riskHistList = new List<JITRiskQuantityHistory>();
        //            List<ItemLimitMaster> itemlimit = new List<ItemLimitMaster>();
        //            List<int> wids = null;
        //            List<int> itemMultiMrpIds = null;
        //            itemMultiMrpIds = dataList.Select(x => x.ItemMultiMrpId).ToList();
        //            wids = dataList.Select(x => x.WarehouseId).ToList();
        //            itemlimit = context.ItemLimitMasterDB.Where(x => itemMultiMrpIds.Contains(x.ItemMultiMRPId) && wids.Contains(x.WarehouseId)).ToList();

        //            foreach (var data in dataList)
        //            {
        //                data.RiskPurchasePrice = 0;
        //                data.RiskQuantity = 0;
        //                data.ModifiedDate = DateTime.Now;
        //                data.ModifiedBy = 0;
        //                context.Entry(data).State = EntityState.Modified;

        //                JITRiskQuantityHistory jITRiskQuantityHistory = new JITRiskQuantityHistory();
        //                jITRiskQuantityHistory.WarehouseId = data.WarehouseId;
        //                jITRiskQuantityHistory.ItemMultiMrpId = data.ItemMultiMrpId;
        //                jITRiskQuantityHistory.RiskQuantity = data.RiskQuantity;
        //                jITRiskQuantityHistory.RiskPurchasePrice = data.RiskPurchasePrice;
        //                jITRiskQuantityHistory.CreatedDate = DateTime.Now;
        //                jITRiskQuantityHistory.CreatedBy = 0;
        //                jITRiskQuantityHistory.IsActive = true;
        //                jITRiskQuantityHistory.IsDeleted = false;
        //                jITRiskQuantityHistory.OldItemlimitQty = itemlimit != null ? itemlimit.FirstOrDefault(x => x.ItemMultiMRPId == data.ItemMultiMrpId && x.WarehouseId == data.WarehouseId).ItemlimitQty : 0;
        //                //itemlimit.ItemlimitQty >= 0 ? itemlimit.ItemlimitQty : 0;
        //                riskHistList.Add(jITRiskQuantityHistory);
        //            }
        //            context.JITRiskQuantityHistories.AddRange(riskHistList);
        //            return context.Commit() > 0;
        //        }

        //    }
        //    return true;
        //}
        #endregion

        #region Mail
        [HttpGet]
        [Route("SendMail")]
        [AllowAnonymous]
        public bool AutoItemActivatedData()
        {
            bool isSent = false;
            using (AuthContext context = new AuthContext())
            {
                var dataList = context.Database.SqlQuery<PrepItemMailDataDc>("[JIT].[GetAutoItemActivatedData]").ToList();
                if (dataList != null && dataList.Any())
                {
                    var BuyerGroup = dataList.GroupBy(x => x.BuyerEmail).ToList();
                    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/JIT");
                    if (!Directory.Exists(ExcelSavePath))
                        Directory.CreateDirectory(ExcelSavePath);

                    foreach (var buyer in BuyerGroup)
                    {
                        var excelData = buyer.Select(x => new { x.WarehouseName, x.ItemNumber, x.ItemName, x.ItemMultiMRPId, x.UnitPrice, x.PTR, x.QtyToLive, x.ShowTypes, x.OldliveQTY, x.CurrentInventory, x.Reason }).ToList();

                        DataTable dt = ClassToDataTable.CreateDataTable(excelData);
                        string fileName = $"DailyJITAutoLiveItem_{DateTime.Now.ToString("yyyy-dd-M_HH.mm.ss.fff")}.xlsx";
                        string filePath = Path.Combine(ExcelSavePath, fileName);
                        if (ExcelGenerator.DataTable_To_Excel(dt, "DailyJITAutoLiveItem", filePath))
                        {
                            if (buyer == null || buyer.Key == null)
                            {
                                isSent = EmailHelper.SendMail(ConfigurationManager.AppSettings["MasterEmail"], "surendra.ghanekar@shopkirana.com", "harry@shopkirana.com", "JIT Autolive item update sheet (" + DateTime.Now + ")", " pfa", filePath);
                            }
                            else
                            {
                                isSent = EmailHelper.SendMail(ConfigurationManager.AppSettings["MasterEmail"], buyer.Key, "", "JIT Autolive item update sheet (" + DateTime.Now + ")", " pfa", filePath);
                                if (!isSent) logger.Error("Failed while sending mail to EmailId : " + buyer.Key);
                            }
                        }
                    }



                    var dataTables = new List<DataTable>();
                    DataTable dt1 = ClassToDataTable.CreateDataTable(dataList);
                    dt1.TableName = "JITAutoItemActivatedData";
                    dataTables.Add(dt1);

                    string fileName1 = $"AllDailyJITAutoLiveItem_{DateTime.Now.ToString("yyyy-dd-M_HH.mm.ss.fff")}.xlsx";
                    string filePath1 = Path.Combine(ExcelSavePath, fileName1);
                    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath1))
                    {
                        var query = "Select [To] from EmailRecipients where EmailType='JITAutoItemActivatedData'";
                        var To = context.Database.SqlQuery<string>(query).FirstOrDefault();
                        isSent = EmailHelper.SendMail(ConfigurationManager.AppSettings["MasterEmail"], To, "", " All JIT Autolive item update sheet (" + DateTime.Now + ")", " pfa", filePath1);

                    }
                }

            }
            return isSent;
        }
        #endregion

        #region PrepAutoItemActivate
        [HttpGet]
        [Route("PrepAutoItemActivate")]
        [AllowAnonymous]
        public async Task<ResultViewModel<string>> PrepAutoItemActivate()
        {
            var result = new ResultViewModel<string>();
            #region PrepAutoItemActivate
            ItemMasterManager itemMasterManager = new ItemMasterManager();
            var PrepAutoItemActivateList = await itemMasterManager.getJobLiveItem();
            #endregion
            if (PrepAutoItemActivateList != null && PrepAutoItemActivateList.Any())
            {
                var list = PrepAutoItemActivateList.GroupBy(x => x.WarehouseId).ToList();
                ParallelLoopResult parellelResult = Parallel.ForEach(list, new ParallelOptions { MaxDegreeOfParallelism = 3 }, (x) =>
                {
                    var newList = AsyncContext.Run(() => AutoItemActivate(x.ToList()));
                    try
                    {
                        using (var conn = new SqlConnection(DbConstants.AuthContextDbConnection))
                        {
                            conn.Open();
                            using (SqlBulkCopy copy = new SqlBulkCopy(conn))
                            {
                                copy.BulkCopyTimeout = 3600;
                                copy.BatchSize = 20000;
                                copy.DestinationTableName = "JIT.AutoItemActivate";
                                DataTable table = ClassToDataTable.CreateDataTable(newList);
                                table.TableName = "JIT.AutoItemActivate";
                                copy.WriteToServer(table);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TextFileLogHelper.LogError(" Error In AutoItemActivate for WarehouseId  :" + x.FirstOrDefault().WarehouseId + "  Error : " + ex.ToString());
                    }
                });

                if (parellelResult.IsCompleted)
                {
                    TestController obj = new TestController();
                    var res = obj.InsertAllItemToElastic();
                    return new ResultViewModel<string>
                    {
                        ErrorMessage = "",
                        IsSuceess = true,
                        ResultItem = "",
                        ResultList = null,
                        SuccessMessage = "Success"
                    };
                }
            }
            return result;
        }
        public async Task<List<PrepAutoItemActivateDc>> AutoItemActivate(List<PrepAutoItemActivateDc> PrepAutoItemActivates)
        {

            if (PrepAutoItemActivates.Any())
            {
                var WarehouseId = PrepAutoItemActivates.FirstOrDefault().WarehouseId;

                try
                {

                    using (AuthContext context = new AuthContext())
                    {
                        var ItemIds = PrepAutoItemActivates.Select(x => x.ItemId).Distinct().ToList();
                        var ItemLists = context.itemMasters.Where(i => ItemIds.Contains(i.ItemId) && i.WarehouseId == WarehouseId).ToList();

                        #region ItemIncentiveClassificationMargin
                        ItemMasterManager itemMasterManager = new ItemMasterManager();
                        List<ItemLimitMaster> itemLimitMasters = new List<ItemLimitMaster>();
                        List<ItemMaster> NewItemMasters = new List<ItemMaster>();
                        var numbers = new List<int>();
                        numbers.AddRange(ItemLists.Select(x => x.ItemMultiMRPId).Distinct().ToList());
                        var ItemIncentiveClassificationMargin = await itemMasterManager.GetItemIncentiveClassificationMargin(WarehouseId, numbers);
                        #endregion

                        var ItemMultiMrpIds = PrepAutoItemActivates.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        var ItemMultiMrps = context.ItemMultiMRPDB.Where(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId)).Distinct().ToList();
                        var Itemlimits = context.ItemLimitMasterDB.Where(x => ItemMultiMrpIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == WarehouseId).Distinct().ToList();

                        foreach (var UpdateJITLiveItem in PrepAutoItemActivates.Where(x => x.QtyToLive > 0).OrderBy(x => x.ItemId))
                        {
                            var MRPs = ItemMultiMrps.FirstOrDefault(x => x.ItemMultiMRPId == UpdateJITLiveItem.ItemMultiMRPId);
                            var itemlimit = Itemlimits.Where(x => x.ItemMultiMRPId == UpdateJITLiveItem.ItemMultiMRPId && x.WarehouseId == WarehouseId).FirstOrDefault();
                            var item = ItemLists.FirstOrDefault(x => x.ItemId == UpdateJITLiveItem.ItemId);
                            if (item != null && UpdateJITLiveItem.PTR > 0)
                            {
                                if (ItemIncentiveClassificationMargin != null && ItemIncentiveClassificationMargin.Any(x => x.MinMarginPercent > 0))
                                {
                                    if (item.Margin < ItemIncentiveClassificationMargin.FirstOrDefault(x => x.MinMarginPercent > 0).MinMarginPercent)
                                    {
                                        UpdateJITLiveItem.IsSuccess = false;
                                        UpdateJITLiveItem.ErrorMsg = "Margin can't be less than company MinMarginPercent for Item :" + item.itemname;
                                        continue;
                                    }
                                }
                                bool Isupdate = false;
                                bool IsActivated = false;
                                bool IsUpdateLimit = false;
                                if (itemlimit != null && itemlimit.ItemlimitQty >= UpdateJITLiveItem.QtyToLive && itemlimit.IsItemLimit && item.active)
                                {
                                    UpdateJITLiveItem.IsSuccess = false;
                                    UpdateJITLiveItem.ErrorMsg = "Already Live on limit above the QtyToLive value";
                                }
                                if (itemlimit != null && itemlimit.ItemlimitQty >= UpdateJITLiveItem.QtyToLive && item.active == false)
                                {
                                    itemlimit.ItemlimitQty = UpdateJITLiveItem.QtyToLive;
                                    itemlimit.IsItemLimit = true;
                                    itemlimit.UpdateDate = DateTime.Now;
                                    context.Entry(itemlimit).State = EntityState.Modified;
                                    IsUpdateLimit = true;
                                    Isupdate = true;
                                }
                                else if (itemlimit != null && itemlimit.ItemlimitQty < UpdateJITLiveItem.QtyToLive)
                                {
                                    itemlimit.ItemlimitQty = UpdateJITLiveItem.QtyToLive;
                                    itemlimit.IsItemLimit = true;
                                    itemlimit.UpdateDate = DateTime.Now;
                                    context.Entry(itemlimit).State = EntityState.Modified;
                                    Isupdate = true;
                                    IsUpdateLimit = true;

                                }
                                else if (itemlimit == null)
                                {
                                    ItemLimitMaster addlimit = new ItemLimitMaster();
                                    addlimit.ItemlimitQty = UpdateJITLiveItem.QtyToLive;
                                    addlimit.ItemNumber = item.Number;
                                    addlimit.ItemMultiMRPId = UpdateJITLiveItem.ItemMultiMRPId;
                                    addlimit.WarehouseId = WarehouseId;
                                    addlimit.CreatedDate = indianTime;
                                    addlimit.UpdateDate = indianTime;
                                    addlimit.IsItemLimit = true;
                                    itemLimitMasters.Add(addlimit);
                                    Isupdate = true;
                                    IsUpdateLimit = true;

                                }
                                if (Isupdate)
                                {
                                    if (item.UnitPrice == 0 || item.active == false)
                                    {
                                        item.UnitPrice = Math.Round(MRPs.MRP / UpdateJITLiveItem.PTR, 3);
                                        item.Description = "Due to inactive Or Zero UnitPrice";
                                        UpdateJITLiveItem.ErrorMsg = "Due to inactive Or Zero UnitPrice";
                                    }
                                    IsActivated = item.active ? false : true;
                                    item.active = item.UnitPrice > 0 ? true : false;
                                    item.UpdateBy = "System";
                                    item.UpdatedDate = DateTime.Now;
                                    context.Entry(item).State = EntityState.Modified;

                                    UpdateJITLiveItem.IsSuccess = (IsActivated || IsUpdateLimit) ? true : false;
                                    if (IsActivated && IsUpdateLimit)
                                    {
                                        UpdateJITLiveItem.ErrorMsg = "Item Activate Limit Update";
                                    }
                                    else if (IsActivated)
                                    {
                                        UpdateJITLiveItem.ErrorMsg = "Item Activate";
                                    }
                                    else if (IsUpdateLimit)
                                    {
                                        UpdateJITLiveItem.ErrorMsg = "Limit Update";
                                    }
                                    UpdateJITLiveItem.UnitPrice = item.UnitPrice;

                                }
                            }
                            else if (item == null && UpdateJITLiveItem.PTR > 0)
                            {
                                //create new item
                                var itemmaster = context.itemMasters.Where(x => x.Number == MRPs.ItemNumber && x.IsDisContinued == false && x.WarehouseId == WarehouseId).OrderByDescending(x => x.MinOrderQty).FirstOrDefault();
                                if (itemmaster != null)
                                {
                                    ItemMaster CreateDupicate = new ItemMaster();
                                    CreateDupicate.SupplierId = itemmaster.SupplierId;
                                    CreateDupicate.SupplierName = itemmaster.SupplierName;
                                    CreateDupicate.SUPPLIERCODES = itemmaster.SUPPLIERCODES;
                                    CreateDupicate.DepoId = itemmaster.DepoId;
                                    CreateDupicate.DepoName = itemmaster.DepoName;
                                    CreateDupicate.BuyerName = itemmaster.BuyerName;
                                    CreateDupicate.GruopID = itemmaster.GruopID;
                                    CreateDupicate.TGrpName = itemmaster.TGrpName;
                                    CreateDupicate.DistributionPrice = CreateDupicate.DistributionPrice;
                                    CreateDupicate.TotalTaxPercentage = itemmaster.TotalTaxPercentage;
                                    CreateDupicate.CessGrpID = itemmaster.CessGrpID;
                                    CreateDupicate.CessGrpName = itemmaster.CessGrpName;
                                    CreateDupicate.TotalCessPercentage = itemmaster.TotalCessPercentage;
                                    CreateDupicate.CatLogoUrl = itemmaster.LogoUrl;
                                    CreateDupicate.WarehouseId = itemmaster.WarehouseId;
                                    CreateDupicate.WarehouseName = itemmaster.WarehouseName;
                                    CreateDupicate.BaseCategoryid = itemmaster.BaseCategoryid;
                                    CreateDupicate.LogoUrl = itemmaster.LogoUrl;
                                    CreateDupicate.UpdatedDate = indianTime;
                                    CreateDupicate.CreatedDate = indianTime;
                                    CreateDupicate.CategoryName = itemmaster.CategoryName;
                                    CreateDupicate.Categoryid = itemmaster.Categoryid;
                                    CreateDupicate.SubcategoryName = itemmaster.SubcategoryName;
                                    CreateDupicate.SubCategoryId = itemmaster.SubCategoryId;
                                    CreateDupicate.SubsubcategoryName = itemmaster.SubsubcategoryName;
                                    CreateDupicate.SubsubCategoryid = itemmaster.SubsubCategoryid;
                                    CreateDupicate.SubSubCode = itemmaster.SubSubCode;
                                    CreateDupicate.itemcode = itemmaster.itemcode;
                                    CreateDupicate.marginPoint = itemmaster.marginPoint;
                                    CreateDupicate.Number = itemmaster.Number;
                                    CreateDupicate.PramotionalDiscount = itemmaster.PramotionalDiscount;
                                    CreateDupicate.MinOrderQty = itemmaster.MinOrderQty;
                                    CreateDupicate.NetPurchasePrice = itemmaster.NetPurchasePrice;
                                    CreateDupicate.GeneralPrice = itemmaster.GeneralPrice;
                                    CreateDupicate.promoPerItems = itemmaster.promoPerItems;
                                    CreateDupicate.promoPoint = itemmaster.promoPoint;
                                    CreateDupicate.PurchaseMinOrderQty = itemmaster.PurchaseMinOrderQty;
                                    CreateDupicate.PurchaseSku = itemmaster.PurchaseSku;
                                    CreateDupicate.PurchaseUnitName = itemmaster.PurchaseUnitName;
                                    CreateDupicate.SellingSku = itemmaster.SellingSku;
                                    CreateDupicate.SellingUnitName = itemmaster.SellingUnitName;
                                    CreateDupicate.SizePerUnit = itemmaster.SizePerUnit;
                                    CreateDupicate.VATTax = itemmaster.VATTax;
                                    CreateDupicate.HSNCode = itemmaster.HSNCode;
                                    CreateDupicate.HindiName = itemmaster.HindiName;
                                    CreateDupicate.CompanyId = itemmaster.CompanyId;
                                    CreateDupicate.Reason = itemmaster.Reason;
                                    CreateDupicate.DefaultBaseMargin = itemmaster.DefaultBaseMargin;
                                    CreateDupicate.Deleted = false;
                                    CreateDupicate.Cityid = itemmaster.Cityid;
                                    CreateDupicate.CityName = itemmaster.CityName;
                                    CreateDupicate.UOM = MRPs.UOM;
                                    CreateDupicate.UnitofQuantity = MRPs.UnitofQuantity;
                                    CreateDupicate.PurchasePrice = itemmaster.PurchasePrice;
                                    CreateDupicate.IsSensitive = itemmaster.IsSensitive;
                                    CreateDupicate.IsSensitiveMRP = itemmaster.IsSensitiveMRP;
                                    CreateDupicate.ShelfLife = itemmaster.ShelfLife;
                                    CreateDupicate.IsReplaceable = itemmaster.IsReplaceable;
                                    CreateDupicate.BomId = itemmaster.BomId;
                                    CreateDupicate.Type = itemmaster.Type;
                                    CreateDupicate.CreatedBy = 0;
                                    CreateDupicate.SellerStorePrice = itemmaster.SellerStorePrice; //added
                                    CreateDupicate.IsSellerStoreItem = itemmaster.IsSellerStoreItem;
                                    CreateDupicate.active = true;
                                    CreateDupicate.UnitPrice = Math.Round(MRPs.MRP / UpdateJITLiveItem.PTR, 3);

                                    //CreateDupicate.UnitPrice = Math.Round(MRPs.MRP / (1 + (UpdateJITLiveItem.PTR / Convert.ToDouble(100))), 3);
                                    CreateDupicate.PurchasePrice = CreateDupicate.UnitPrice;
                                    CreateDupicate.POPurchasePrice = CreateDupicate.UnitPrice;
                                    CreateDupicate.price = MRPs.MRP;
                                    CreateDupicate.MRP = MRPs.MRP;
                                    CreateDupicate.ItemMultiMRPId = MRPs.ItemMultiMRPId;
                                    CreateDupicate.itemBaseName = MRPs.itemBaseName;

                                    if (CreateDupicate.IsSensitive == true && CreateDupicate.IsSensitiveMRP == true)
                                    {
                                        CreateDupicate.itemname = CreateDupicate.itemBaseName + " " + CreateDupicate.price + " MRP " + CreateDupicate.UnitofQuantity + " " + CreateDupicate.UOM;
                                    }
                                    else if (CreateDupicate.IsSensitive == true && CreateDupicate.IsSensitiveMRP == false)
                                    {
                                        CreateDupicate.itemname = CreateDupicate.itemBaseName + " " + CreateDupicate.UnitofQuantity + " " + CreateDupicate.UOM; //item display name 
                                    }
                                    else if (CreateDupicate.IsSensitive == false && CreateDupicate.IsSensitiveMRP == false)
                                    {
                                        CreateDupicate.itemname = CreateDupicate.itemBaseName; //item display name
                                    }
                                    else if (CreateDupicate.IsSensitive == false && CreateDupicate.IsSensitiveMRP == true)
                                    {
                                        CreateDupicate.itemname = CreateDupicate.itemBaseName + " " + CreateDupicate.price + " MRP";//item display name 
                                    }
                                    CreateDupicate.SellingUnitName = CreateDupicate.itemname + " " + CreateDupicate.MinOrderQty + "Unit";//item selling unit name
                                    CreateDupicate.PurchaseUnitName = CreateDupicate.itemname + " " + CreateDupicate.PurchaseMinOrderQty + "Unit";//item PurchaseUnitName name
                                    CreateDupicate.CreatedDate = DateTime.Now;
                                    CreateDupicate.UpdatedDate = DateTime.Now;
                                    CreateDupicate.CreatedBy = 0;
                                    CreateDupicate.UpdateBy = "System";
                                    NewItemMasters.Add(CreateDupicate);
                                    UpdateJITLiveItem.ItemId = CreateDupicate.ItemId;
                                    if (itemlimit != null)
                                    {
                                        itemlimit.ItemlimitQty = UpdateJITLiveItem.QtyToLive;
                                        itemlimit.IsItemLimit = true;
                                        itemlimit.UpdateDate = DateTime.Now;
                                        context.Entry(itemlimit).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        ItemLimitMaster addlimit = new ItemLimitMaster();
                                        addlimit.ItemlimitQty = UpdateJITLiveItem.QtyToLive;
                                        addlimit.ItemNumber = CreateDupicate.Number;
                                        addlimit.ItemMultiMRPId = UpdateJITLiveItem.ItemMultiMRPId;
                                        addlimit.WarehouseId = WarehouseId;
                                        addlimit.CreatedDate = indianTime;
                                        addlimit.UpdateDate = indianTime;
                                        addlimit.IsItemLimit = true;
                                        itemLimitMasters.Add(addlimit);
                                    }
                                    UpdateJITLiveItem.IsSuccess = true;
                                    UpdateJITLiveItem.UnitPrice = CreateDupicate.UnitPrice;
                                    UpdateJITLiveItem.ErrorMsg = "New Item Create for ItemMultiMrpId :" + UpdateJITLiveItem.ItemMultiMRPId;
                                }
                                else
                                {
                                    UpdateJITLiveItem.ErrorMsg = "Item DisContinued :" + MRPs.ItemNumber;
                                }
                            }
                            else
                            {
                                UpdateJITLiveItem.IsSuccess = false;
                                UpdateJITLiveItem.ErrorMsg = "PTR not found : " + UpdateJITLiveItem.PTR;
                            }
                        }
                        if (itemLimitMasters != null && itemLimitMasters.Any())
                        {
                            context.ItemLimitMasterDB.AddRange(itemLimitMasters);
                        }
                        if (NewItemMasters != null && NewItemMasters.Any())
                        {
                            context.itemMasters.AddRange(NewItemMasters);
                        }
                        context.Commit();

                        if (PrepAutoItemActivates != null && PrepAutoItemActivates.Any(x => x.ItemId == 0))
                        {
                            foreach (var item in PrepAutoItemActivates.Where(x => x.ItemId == 0 && x.IsSuccess))
                            {
                                item.ItemId = NewItemMasters.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId).ItemId;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TextFileLogHelper.LogError(" during insert or Error In AutoItemActivate for WarehouseId  :" + WarehouseId + "  Error : " + ex.ToString());

                }
            }
            return PrepAutoItemActivates;

        }
        #endregion

        [HttpGet]
        [Route("GetRiskQuantityHistory")]
        public async Task<List<RiskQuantityHistoryDc>> GetRiskQuantityHistoryAsync(int WarehouseId, int ItemMultiMrpId, int skip, int take)
        {
            using (AuthContext context = new AuthContext())
            {
                var param1 = new SqlParameter(parameterName: "@WarehouseId", value: WarehouseId);
                var param2 = new SqlParameter(parameterName: "@ItemMultiMrpId", value: ItemMultiMrpId);
                var param3 = new SqlParameter(parameterName: "@Skip", value: skip);
                var param4 = new SqlParameter(parameterName: "@Take", value: take);
                var data = context.Database.SqlQuery<RiskQuantityHistoryDc>("[JIT].[GetRiskQuantityHistory] @WarehouseId,@ItemMultiMrpId,@Skip,@Take", param1, param2, param3, param4).ToList();
                return data;
            }
        }


        #region ItemPrice Recalculate

        #endregion
        [HttpGet]
        [Route("GetUnitPriceChange")]
        public async Task<List<DailyItemASPDC>> GetUnitPriceChange(int WarehouseId, int BrandId, int Skip, int Take)
        {
            using (var context = new AuthContext())
            {
                var warehouseid = new SqlParameter("@WarehouseId", WarehouseId);
                var brandid = new SqlParameter("@BrandId", BrandId);
                var skip = new SqlParameter("@Skip", Skip);
                var take = new SqlParameter("@Take", Take);
                var data = context.Database.SqlQuery<DailyItemASPDC>("WarehouseWiseItemAsp @WarehouseId,@BrandId,@Skip,@Take", warehouseid, brandid, skip, take).ToList();
                return data;
            }
        }

    }
}
