using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model;
using NLog;
using System.Data;
using GenricEcommers.Models;

namespace AngularJSAuthentication.API.Helper
{
    public class ItemSchemeHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public async Task<bool> ErrorCheckingAndProcess()
        {
            bool result = false;
            return result;
            //using (AuthContext context = new AuthContext())
            //{
            //    var itemschemList = context.ItemSchemeExcelUploaderMasters.Where(x => x.Status == 0 && x.IsActive == false && x.IsDeleted == false).Include(x => x.ItemSchemeExcelUploaderDetails).OrderBy(x => x.CreatedDate).Skip(0).Take(10).ToList();
            //    if (itemschemList.Count > 0 && itemschemList.Any())
            //    {
            //        var citiesIds = itemschemList.Select(x => x.Cityid).ToList();
            //        var itemCompanyCodelist = itemschemList.SelectMany(x => x.ItemSchemeExcelUploaderDetails.Select(y => y.CompanyCode)).ToList();
            //        var itemStockCodelist = itemschemList.SelectMany(x => x.ItemSchemeExcelUploaderDetails.Select(y => y.CompanyStockCode)).ToList();
            //        var FreeitemCompanyCodelist = itemschemList.SelectMany(x => x.ItemSchemeExcelUploaderDetails.Where(z => !string.IsNullOrEmpty(z.FreeChildItemCompanycode)).Select(y => y.FreeChildItemCompanycode)).ToList();
            //        var FreeitemStockCodelist = itemschemList.SelectMany(x => x.ItemSchemeExcelUploaderDetails.Where(z => !string.IsNullOrEmpty(z.FreeChildItemCompanyStockcode)).Select(y => y.FreeChildItemCompanyStockcode)).ToList();
            //        var CompanyCodes = itemCompanyCodelist.Concat(FreeitemCompanyCodelist);
            //        var StockCode = itemStockCodelist.Concat(FreeitemStockCodelist);
            //        var itemcentrallst = context.ItemMasterCentralDB.Where(x => CompanyCodes.Contains(x.CompanyCode) && x.Deleted == false).ToList();
            //        var itemStocklst = context.ItemMultiMRPDB.Where(x => StockCode.Contains(x.CompanyStockCode) && x.Deleted == false).ToList();
            //        var warehouses = context.Warehouses.Where(x => citiesIds.Contains(x.Cityid) && x.IsKPP == false && x.active == true && x.Deleted == false).Select(x => new { x.WarehouseName, x.WarehouseId }).ToList();
            //        foreach (var Master in itemschemList)
            //        {
            //            try
            //            {
            //                bool AnyError = false; //If any eror then 1 (Pending) else 2 (Success)
            //                foreach (var item in Master.ItemSchemeExcelUploaderDetails)
            //                {
            //                    #region Check Item
            //                    var ItemExists = true;
            //                    var ItemStockExits = true;
            //                    var itemMaster = itemcentrallst.FirstOrDefault(x => x.CompanyCode == item.CompanyCode && x.Deleted == false);
            //                    if (itemMaster == null)
            //                    {
            //                        ItemExists = false;
            //                        item.ErrorMessage = "Item Not Found";
            //                        AnyError = true;
            //                    }
            //                    else
            //                    {
            //                        //if (itemMaster.SubCategoryId == Master.SubCatId)
            //                        //{ 
            //                        //}
            //                        if (itemMaster.SubsubCategoryid != Master.SubSubCatId)
            //                        {
            //                            item.ErrorMessage = "Item Not Found in this Brand";
            //                            AnyError = true;
            //                        }
            //                        #region Check Stock
            //                        ItemStockExits = itemStocklst.Any(x => x.CompanyStockCode == item.CompanyStockCode && x.ItemNumber == itemMaster.Number);
            //                        if (!ItemStockExits)
            //                        {
            //                            item.ErrorMessage = "Item Stock Not Found";
            //                            AnyError = true;
            //                        }
            //                        #endregion
            //                    }
            //                    #endregion
            //                    #region Check Offer

            //                    if (!string.IsNullOrEmpty(item.FreeChildItemCompanycode))
            //                    {
            //                        if (!ItemExists && !ItemStockExits)
            //                        {
            //                            foreach (var wh in warehouses)
            //                            {
            //                                var itemMasterids = context.itemMasters.Where(x => x.Number == itemMaster.Number && x.price == item.MRP && x.WarehouseId == wh.WarehouseId).Select(x => x.ItemId).ToList();

            //                                var OfferExits = context.OfferDb.Any(x => itemMasterids.Contains(x.itemId) && x.OfferOn == "Item" && (x.start <= Master.StartDate && x.end >= Master.EndDate) && x.IsActive && !x.IsDeleted);
            //                                if (!ItemStockExits)
            //                                {
            //                                    item.ErrorMessage = "Offer already exists for Item for " + wh.WarehouseName;
            //                                    AnyError = true;
            //                                }
            //                            }
            //                        }
            //                        ItemExists = itemcentrallst.Any(x => x.CompanyCode == item.FreeChildItemCompanycode && x.Deleted == false);
            //                        if (!ItemExists)
            //                        {
            //                            item.ErrorMessage = "Item Not Found for free item : " + item.FreeChildItem;
            //                            AnyError = true;
            //                        }
            //                        ItemStockExits = itemStocklst.Any(x => x.CompanyStockCode == item.FreeChildItemCompanyStockcode);
            //                        if (!ItemStockExits)
            //                        {
            //                            item.ErrorMessage = "Item Stock Not Found for free item : " + item.FreeChildItem;
            //                            AnyError = true;
            //                        }
            //                    }
            //                    #endregion
            //                }
            //                if (AnyError)
            //                {
            //                    Master.Status = 1;//Pending
            //                }
            //                else
            //                {
            //                    bool IsSuccess = await ProceesItemSchem(Master, context, itemcentrallst, itemStocklst);
            //                    if (IsSuccess)
            //                    {
            //                        Master.Status = 2;//Success
            //                        Master.IsApproved = true;
            //                        Master.ApprovedDate = indianTime;
            //                        Master.IsActive = true;
            //                    }
            //                    else
            //                    {
            //                        Master.Status = 1;//Pending
            //                    }
            //                }
            //                Master.ModifiedDate = indianTime;
            //                context.Entry(Master).State = EntityState.Modified;

            //            }
            //            catch (Exception ex)
            //            {
            //                TextFileLogHelper.TraceLog("Error occures In ErrorChecking : " + ex.Message.ToString());
            //            }
            //        }
            //        context.Commit();
            //    }
            //}
           // return result;
        }
        public async Task<bool> ProceesItemSchem(ItemSchemeExcelUploaderMaster ItemSchemeUploader, AuthContext context, List<ItemMasterCentral> itemcentrallst, List<ItemMultiMRP> itemStocklst)
        {
            bool result = false;
            if (ItemSchemeUploader != null)
            {
                ItemSchemeMaster AddItem = new ItemSchemeMaster();
                AddItem.SubSubCatId = ItemSchemeUploader.SubSubCatId;
                AddItem.Cityid = ItemSchemeUploader.Cityid;
                AddItem.StartDate = ItemSchemeUploader.StartDate;
                AddItem.EndDate = (DateTime)ItemSchemeUploader.EndDate;
                AddItem.UploadedSheetUrl = ItemSchemeUploader.UploadedSheetUrl;
                AddItem.ItemSchemeDetails = new List<ItemSchemeDetail>();
                foreach (var ItemDetails in ItemSchemeUploader.ItemSchemeExcelUploaderDetails)
                {
                    ItemSchemeDetail item = new ItemSchemeDetail();
                    item.Slabs = new List<ItemSchemeSlab>();
                    item.ItemSchemeFreebiess = new List<ItemSchemeFreebies>();
                    item.BaseScheme = ItemDetails.BaseScheme;
                    item.CompanyStockCode = ItemDetails.CompanyStockCode;
                    item.CompanyCode = ItemDetails.CompanyCode;
                    item.ItemMultiMRPId = itemStocklst.FirstOrDefault(x => x.CompanyStockCode == ItemDetails.CompanyStockCode).ItemMultiMRPId;
                    item.ItemNumber = itemcentrallst.FirstOrDefault(x => x.CompanyCode == ItemDetails.CompanyCode).Number;
                    item.ItemName = ItemDetails.ItemName;
                    item.MRP = ItemDetails.MRP;
                    item.PTR = ItemDetails.PTR;
                    item.BaseScheme = ItemDetails.BaseScheme;
                    item.IsIncludeOnInvoiceMarginPOPrice = ItemDetails.IsIncludeOnInvoiceMarginPOPrice;
                    item.IsIncludeMaxSlabPOPrice = ItemDetails.IsIncludeMaxSlabPOPrice;
                    item.IsIncludeBaseSchmePOPrice = ItemDetails.IsIncludeBaseSchmePOPrice;
                    item.onvoiceMargin = ItemDetails.onvoiceMargin;
                    item.offinvoicemargin = ItemDetails.offinvoicemargin;
                    item.IsActive = false;

                    ItemDetails.ErrorMessage = null;

                    if (!string.IsNullOrEmpty(ItemDetails.FreeChildItem) && ItemDetails.FreeChildItem!="0" && ItemDetails.FreeBaseItemQty>0 && ItemDetails.FreeItemStockQty>0)
                    {
                        var Freeitem = itemStocklst.FirstOrDefault(x => x.CompanyStockCode == ItemDetails.FreeChildItemCompanyStockcode);
                        ItemSchemeFreebies fitem = new ItemSchemeFreebies();
                        fitem.BaseItemQty = ItemDetails.FreeBaseItemQty;
                        fitem.ItemName = ItemDetails.FreeChildItem;
                        fitem.ItemCompanyCode = ItemDetails.CompanyCode;
                        fitem.ItemCompanyStockCode = Freeitem.CompanyStockCode;
                        fitem.MRP = Freeitem.MRP;
                        fitem.Qty = ItemDetails.FreeItemQty;
                        fitem.StockQty = ItemDetails.FreeItemStockQty;
                        fitem.IsFreeStock = ItemDetails.IsFreeStock;
                        item.ItemSchemeFreebiess.Add(fitem);
                    }
                    if (ItemDetails.SlabPurchaseQTY1 > 0)
                    {
                        ItemSchemeSlab slab1 = new ItemSchemeSlab();
                        slab1.SlabPurchaseQTY = ItemDetails.SlabPurchaseQTY1;
                        slab1.SlabScheme = ItemDetails.SlabScheme1;
                        item.Slabs.Add(slab1);
                        if (ItemDetails.SlabPurchaseQTY2 > 0)
                        {
                            ItemSchemeSlab slab2 = new ItemSchemeSlab();
                            slab2.SlabPurchaseQTY = ItemDetails.SlabPurchaseQTY2;
                            slab2.SlabScheme = ItemDetails.SlabScheme2;
                            item.Slabs.Add(slab2);
                        }
                        if (ItemDetails.SlabPurchaseQTY3 > 0)
                        {
                            ItemSchemeSlab slab3 = new ItemSchemeSlab();
                            slab3.SlabPurchaseQTY = ItemDetails.SlabPurchaseQTY3;
                            slab3.SlabScheme = ItemDetails.SlabScheme3;
                            item.Slabs.Add(slab3);
                        }
                        if (ItemDetails.SlabPurchaseQTY4 > 0)
                        {
                            ItemSchemeSlab slab4 = new ItemSchemeSlab();
                            slab4.SlabPurchaseQTY = ItemDetails.SlabPurchaseQTY4;
                            slab4.SlabScheme = ItemDetails.SlabScheme4;
                            item.Slabs.Add(slab4);
                        }
                    }
                    AddItem.ItemSchemeDetails.Add(item);
                }
                AddItem.CreatedDate = indianTime;
                AddItem.IsDeleted = false;
                AddItem.IsActive = false;
                AddItem.IsApproved = false;
                AddItem.CreatedBy = ItemSchemeUploader.CreatedBy;
                context.ItemSchemeMasters.Add(AddItem);
                if (context.Commit() > 0)
                {
                    ItemSchemeUploader.ItemSchemeMasterId = AddItem.Id;
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
        public async Task<bool> DeactiveItemScheme()
        {
            bool result = false;
            List<ItemSchemeMaster> itemschemList = null;
            using (AuthContext context = new AuthContext())
            {
                itemschemList = context.ItemSchemeMasters.Where(x => x.IsActive == true && x.IsDeleted == false && x.EndDate < indianTime).OrderBy(x => x.EndDate).Skip(0).Take(5).ToList();
                if (itemschemList.Count > 0 && itemschemList.Any())
                {
                    foreach (var item in itemschemList)
                    {
                        await DeActivateItemSchemeMasterById(item.Id, 1);
                    }
                }
            }
            return result;
        }

        public async Task<string> DeActivateItemSchemeMasterById(long MasterId, int userid)
        {
            string result = "";
            using (AuthContext context = new AuthContext())
            {
                var ItemSchemeMasters = await context.ItemSchemeMasters.Where(x => x.IsDeleted == false && x.Id == MasterId && x.IsApproved == true)
                                  .Include(c => c.ItemSchemeDetails)
                                  .Include("ItemSchemeDetails.Slabs")
                                  .Include("ItemSchemeDetails.ItemSchemeFreebiess")
                                  .FirstAsync();
                if (ItemSchemeMasters != null && ItemSchemeMasters.ItemSchemeDetails.Any() && ItemSchemeMasters.IsActive == true)
                {
                    var UpdateItemmasters = new List<ItemMaster>();
                    var UpdateOffer = new List<Offer>();
                    foreach (var ItemSchemeDetail in ItemSchemeMasters.ItemSchemeDetails.Where(x => x.IsActive == true))
                    {
                        List<ItemMaster> itemlist = null;
                        List<Offer> OfferList = null;
                        if (ItemSchemeDetail.ItemIds != null)
                        {
                            int[] itemids = ItemSchemeDetail.ItemIds.Split(',').Select(x => int.Parse(x)).ToArray();
                            itemlist = context.itemMasters.Where(x => itemids.Contains(x.ItemId) && x.active == true).ToList();

                        }
                        if (ItemSchemeDetail.OfferIds != null)
                        {
                            int[] OfferIds = ItemSchemeDetail.OfferIds.Split(',').Select(x => int.Parse(x)).ToArray();
                            OfferList = context.OfferDb.Where(x => OfferIds.Contains(x.OfferId) && x.IsActive == true).ToList();

                        }

                        if (itemlist!=null && itemlist.Count() > 0 && itemlist.Any())
                        {
                            UpdateItemmasters.AddRange(itemlist);
                        }
                        if (OfferList!=null && OfferList.Count() > 0 && OfferList.Any())
                        {
                            UpdateOffer.AddRange(OfferList);
                        }
                        ItemSchemeDetail.IsActive = false;
                    }
                    foreach (var offer in UpdateOffer)
                    {
                        offer.IsActive = false;
                        offer.UpdateDate = indianTime;
                        context.Entry(offer).State = EntityState.Modified;
                        if (offer.OfferOn == "Item")
                        {
                            var offerItem = context.itemMasters.FirstOrDefault(x => x.ItemId == offer.itemId);
                            if (offer.OfferAppType != "Distributor App" && offerItem != null)
                            {
                                var itemnumbers = context.itemMasters.Where(x => x.Number == offerItem.Number && x.ItemMultiMRPId == offerItem.ItemMultiMRPId && x.WarehouseId == offerItem.WarehouseId && x.Deleted == false).ToList();
                                if (itemnumbers.Count != 0)
                                {
                                    foreach (var item in itemnumbers)
                                    {
                                        item.IsOffer = false;
                                        item.OfferCategory = 0;
                                        item.UpdatedDate = indianTime;
                                        context.Entry(offer).State = EntityState.Modified;
                                    }
                                }
                            }

                        }
                    }
                    foreach (var item in UpdateItemmasters)
                    {
                        item.active = false;
                        item.ModifiedBy = userid;
                        item.UpdatedDate = indianTime;
                        context.Entry(item).State = EntityState.Modified;
                    }

                    ItemSchemeMasters.ModifiedBy = userid;
                    ItemSchemeMasters.IsActive = false;
                    ItemSchemeMasters.ModifiedDate = indianTime;
                    if (context.Commit() > 0) { result = "Deactivated Successfully"; } else { result = "Something went wrong"; }
                }
                else if (ItemSchemeMasters.IsActive == false)
                {
                    result = "Already Deactivated";
                }
                else { result = "Something went wrong"; }
            }
            return result;
        }

        public async Task<string> DeActivateItemSchemeOnDetailById(long MasterId, long DetailId, int userid)
        {
            string result = "";
            using (AuthContext context = new AuthContext())
            {
                var ItemSchemeMasters = await context.ItemSchemeMasters.Where(x => x.IsDeleted == false && x.Id == MasterId && x.IsActive == true && x.IsApproved == true)
                                  .Include(c => c.ItemSchemeDetails)
                                  .Include("ItemSchemeDetails.Slabs")
                                  .Include("ItemSchemeDetails.ItemSchemeFreebiess")
                                  .FirstAsync();
                if (ItemSchemeMasters != null && ItemSchemeMasters.ItemSchemeDetails.Any())
                {
                    var UpdateItemmasters = new List<ItemMaster>();
                    var UpdateOffer = new List<Offer>();
                    foreach (var ItemSchemeDetail in ItemSchemeMasters.ItemSchemeDetails.Where(x => x.IsActive == true && x.Id == DetailId))
                    {
                        List<ItemMaster> itemlist = null;

                        List<Offer> OfferList = null;
                        if (ItemSchemeDetail.ItemIds != null)
                        {
                            int[] itemids = ItemSchemeDetail.ItemIds.Split(',').Select(x => int.Parse(x)).ToArray();
                            itemlist = context.itemMasters.Where(x => itemids.Contains(x.ItemId) && x.active == true).ToList();

                        }
                        if (ItemSchemeDetail.OfferIds != null)
                        {
                            int[] OfferIds = ItemSchemeDetail.OfferIds.Split(',').Select(x => int.Parse(x)).ToArray();
                            OfferList = context.OfferDb.Where(x => OfferIds.Contains(x.OfferId) && x.IsActive == true).ToList();

                        }

                        if (itemlist!=null  && itemlist.Count() > 0 && itemlist.Any())
                        {
                            UpdateItemmasters.AddRange(itemlist);
                        }
                        if (OfferList!=null &&  OfferList.Count() > 0 && OfferList.Any())
                        {
                            UpdateOffer.AddRange(OfferList);
                        }
                        ItemSchemeDetail.IsActive = false;
                    }
                    if (UpdateItemmasters.Count > 0 && UpdateItemmasters.Any())
                    {
                        foreach (var item in UpdateItemmasters)
                        {
                            item.active = false;
                            item.ModifiedBy = userid;
                            item.UpdatedDate = indianTime;
                            context.Entry(item).State = EntityState.Modified;
                        }
                    }
                    if (UpdateOffer.Count > 0 && UpdateOffer.Any())
                    {
                        foreach (var offer in UpdateOffer)
                        {
                            offer.IsActive = false;
                            offer.UpdateDate = indianTime;
                            context.Entry(offer).State = EntityState.Modified;
                            if (offer.OfferOn == "Item")
                            {
                                var offerItem = context.itemMasters.FirstOrDefault(x => x.ItemId == offer.itemId);
                                if (offer.OfferAppType != "Distributor App" && offerItem != null)
                                {
                                    var itemnumbers = context.itemMasters.Where(x => x.Number == offerItem.Number && x.ItemMultiMRPId == offerItem.ItemMultiMRPId && x.WarehouseId == offerItem.WarehouseId && x.Deleted == false).ToList();
                                    if (itemnumbers.Count != 0)
                                    {
                                        foreach (var item in itemnumbers)
                                        {
                                            item.IsOffer = false;
                                            item.OfferCategory = 0;
                                            item.UpdatedDate = indianTime;
                                            context.Entry(offer).State = EntityState.Modified;
                                        }
                                    }
                                }

                            }
                        }
                    }
                    ItemSchemeMasters.ModifiedBy = userid;
                    ItemSchemeMasters.ModifiedDate = indianTime;
                    if (context.Commit() > 0) { result = "Updated Successfully"; } else { result = "Something went wrong"; }
                }
            }
            return result;
        }




        public async Task<string> DeActivateFreebiesByDetailId(long MasterId, long DetailId, int userid)
        {
            string result = "";
            using (AuthContext context = new AuthContext())
            {
                var ItemSchemeMasters = await context.ItemSchemeMasters.Where(x => x.IsDeleted == false && x.Id == MasterId && x.IsActive == true && x.IsApproved == true)
                                  .Include(c => c.ItemSchemeDetails)
                                  .Include("ItemSchemeDetails.Slabs")
                                  .Include("ItemSchemeDetails.ItemSchemeFreebiess")
                                  .FirstAsync();
                if (ItemSchemeMasters != null && ItemSchemeMasters.ItemSchemeDetails.Any())
                {
                    var UpdateItemmasters = new List<ItemMaster>();
                    var UpdateOffer = new List<Offer>();
                    var itemSchem = ItemSchemeMasters.ItemSchemeDetails.FirstOrDefault(x => x.Id == DetailId);

                    if (itemSchem != null && itemSchem.ItemSchemeFreebiess.Any())
                    {
                        int[] OfferIds = itemSchem.OfferIds.Split(',').Select(x => int.Parse(x)).ToArray();
                        var OfferList = context.OfferDb.Where(x => OfferIds.Contains(x.OfferId) && x.IsActive == true).ToList();
                        if (OfferList.Count() > 0 && OfferList.Any())
                        {
                            UpdateOffer.AddRange(OfferList);
                        }
                    }
                    if (UpdateOffer.Count > 0 && UpdateOffer.Any())
                    {
                        foreach (var offer in UpdateOffer)
                        {
                            offer.IsActive = false;
                            offer.UpdateDate = indianTime;
                            context.Entry(offer).State = EntityState.Modified;
                            if (offer.OfferOn == "Item")
                            {
                                var offerItem = context.itemMasters.FirstOrDefault(x => x.ItemId == offer.itemId);
                                if (offer.OfferAppType != "Distributor App" && offerItem != null)
                                {
                                    var itemnumbers = context.itemMasters.Where(x => x.Number == offerItem.Number && x.ItemMultiMRPId == offerItem.ItemMultiMRPId && x.WarehouseId == offerItem.WarehouseId && x.Deleted == false).ToList();
                                    if (itemnumbers.Count != 0)
                                    {
                                        foreach (var item in itemnumbers)
                                        {
                                            item.IsOffer = false;
                                            item.OfferCategory = 0;
                                            item.UpdatedDate = indianTime;
                                            context.Entry(offer).State = EntityState.Modified;
                                        }
                                    }
                                }

                            }
                        }
                        ItemSchemeMasters.ModifiedBy = userid;
                        ItemSchemeMasters.ModifiedDate = indianTime;
                        if (context.Commit() > 0) { result = "Updated Successfully"; } else { result = "Something went wrong"; }
                    }
                    else { result = result = "Already Deactivated"; }

                }
            }
            return result;
        }
    }
}
