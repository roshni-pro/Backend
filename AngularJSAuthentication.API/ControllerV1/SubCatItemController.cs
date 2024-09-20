using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/ssitem")]
    public class SubCatItemController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public double xPointValue = AppConstants.xPoint;
        [Route("")]
        [HttpGet]
        public HttpResponseMessage getbyCustscatid(int customerId, int subcatid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


                    //var cachePolicty = new CacheItemPolicy();
                    //cachePolicty.AbsoluteExpiration = indianTime.AddMonths(25);
                    List<SSITEM> brandItem = new List<SSITEM>();
                    //var cache = MemoryCache.Default;

                    var custBrand = context.Customers.Where(x => x.CustomerId == customerId && x.Deleted == false).ToList();

                    foreach (var cw in custBrand)
                    {
                        SSITEM item = new SSITEM();
                        //if (cache.Get("Brand" + cw.Warehouseid.ToString() + subcatid.ToString()) == null)
                        //{
                        var subsubcategory = (from i in context.DbWarehousesubsubcats
                                              where i.Deleted == false && i.WarehouseId == cw.Warehouseid && i.SubsubCategoryid == subcatid
                                              join j in context.SubsubCategorys on i.SubsubCategoryid equals j.SubsubCategoryid
                                              select new factorySubSubCategory
                                              {
                                                  Categoryid = j.Categoryid,
                                                  SubCategoryId = j.SubCategoryId,
                                                  SubsubCategoryid = j.SubsubCategoryid,
                                                  SubsubcategoryName = j.SubsubcategoryName,
                                                  ItemMasters = context.itemMasters.Where(x => x.active == true && x.Deleted == false && x.SubsubCategoryid == i.SubsubCategoryid && x.WarehouseId == cw.Warehouseid).Select(x => new factoryItemdata()
                                                  {
                                                      WarehouseId = i.WarehouseId,
                                                      CompanyId = i.CompanyId,
                                                      Categoryid = x.Categoryid,
                                                      Discount = x.Discount,
                                                      ItemId = x.ItemId,
                                                      itemname = x.itemname,
                                                      LogoUrl = x.SellingSku,
                                                      MinOrderQty = x.MinOrderQty,
                                                      price = x.price,
                                                      SubCategoryId = x.SubCategoryId,
                                                      SubsubCategoryid = x.SubsubCategoryid,
                                                      TotalTaxPercentage = x.TotalTaxPercentage,
                                                      SellingUnitName = x.SellingUnitName,
                                                      SellingSku = x.SellingSku,
                                                      UnitPrice = x.UnitPrice,
                                                      HindiName = x.HindiName,
                                                      VATTax = x.VATTax,
                                                      marginPoint = x.marginPoint,
                                                      ItemNumber = x.Number,
                                                      promoPerItems = x.promoPerItems
                                                  }).ToList()
                                              }).ToList();
                        item.SubsubCategories = subsubcategory;
                        if (item.SubsubCategories.Count > 0)
                        {
                            //cache.Add("Brand" + cw.Warehouseid.ToString() + subcatid.ToString(), item, cachePolicty);
                            brandItem.Add(item);
                        }
                        //}
                        //else
                        //{
                        //    item = (SSITEM)cache.Get("Brand" + cw.Warehouseid.ToString() + subcatid.ToString());
                        //    brandItem.Add(item);
                        //}
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, brandItem);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage getdailyessential(int warid)
        {
            using (var context = new AuthContext())
            {
                try
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    foreach (Claim claim in identity.Claims)
                    {
                        if (claim.Type == "compid")
                        {
                            compid = int.Parse(claim.Value);
                        }
                        if (claim.Type == "userid")
                        {
                            userid = int.Parse(claim.Value);
                        }
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);


                    var ItemMasters = context.itemMasters.Where(x => x.WarehouseId == warid && x.IsDailyEssential == true && x.CompanyId == compid).Select(x => new factoryItemdata()
                    {
                        Categoryid = x.Categoryid,
                        Discount = x.Discount,
                        ItemId = x.ItemId,
                        itemname = x.itemname,
                        LogoUrl = x.LogoUrl,
                        MinOrderQty = x.MinOrderQty,
                        price = x.price,
                        SubCategoryId = x.SubCategoryId,
                        SubsubCategoryid = x.SubsubCategoryid,
                        TotalTaxPercentage = x.TotalTaxPercentage,
                        SellingUnitName = x.SellingUnitName,
                        SellingSku = x.SellingSku,
                        UnitPrice = x.UnitPrice,
                        HindiName = x.HindiName,
                        VATTax = x.VATTax,
                        marginPoint = x.marginPoint,
                        promoPerItems = x.promoPerItems
                    }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, new SSITEM()
                    {
                        //ItemMasters = ItemMasters
                    });
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Item Master " + ex.Message);
                    return null;
                }
            }
        }

        #region RA V2: for customer app get item by cat id
        [Route("V2")]
        [HttpGet]
        public HttpResponseMessage ItemgetbyCategoryId(int customerId, int catid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var custBrand = context.Customers.Where(x => x.CustomerId == customerId && x.Deleted == false).ToList();
                    WRSITEM item = new WRSITEM();
                    foreach (var cw in custBrand)
                    {
                        //// By Sachin For Offer Inactive
                        //try
                        //{
                        //    var data = context.OfferDb.Where(x => x.IsDeleted == false && x.IsActive == true && x.start <= DateTime.Today && x.end <= DateTime.Today).ToList();
                        //    if (data.Count > 0)
                        //    {
                        //        foreach (var offer in data)
                        //        {
                        //            var item1 = context.itemMasters.Where(x => x.IsOffer == true && x.ItemId == offer.itemId && x.WarehouseId == cw.WarehouseId && x.Deleted == false).FirstOrDefault();
                        //            if (item1 != null)
                        //            {
                        //                var data5 = context.itemMasters.Where(x => x.Number == item1.Number && x.WarehouseId == cw.WarehouseId).ToList();
                        //                foreach (var itemnumber in data5)
                        //                {
                        //                    var item5 = context.itemMasters.Where(x => x.ItemId == itemnumber.ItemId && x.WarehouseId == cw.WarehouseId).SingleOrDefault();
                        //                    item5.IsOffer = false;
                        //                    context.itemMasters.Attach(item5);
                        //                    context.Entry(item5).State = EntityState.Modified;
                        //                    context.SaveChanges();
                        //                }
                        //            }
                        //            var kk = context.OfferDb.Where(x => x.OfferId == offer.OfferId).SingleOrDefault();
                        //            kk.IsActive = false;
                        //            context.OfferDb.Attach(kk);
                        //            context.Entry(kk).State = EntityState.Modified;
                        //            context.SaveChanges();
                        //        }
                        //    }
                        //}
                        //catch (Exception ex) { logger.Error(ex.Message); }
                        //List<SubsubCategory> st = new List<SubsubCategory>();
                        //var scategory = context.SubsubCategorys.Where(x => x.IsActive == true && x.Categoryid == catid && x.Deleted == false).ToList();
                        //if (scategory != null)
                        //{
                        //foreach (var a in scategory)
                        //{
                        //    if (a != null)
                        //    {
                        //        bool ItemFound = context.itemMasters.Any(x => x.active == true && x.Deleted == false && x.SubsubCategoryid == a.SubsubCategoryid && x.WarehouseId == cw.WarehouseId);
                        //        if (ItemFound == true)
                        //        {
                        //            st.Add(a);
                        //        }
                        //    }
                        //}
                        //foreach (var ab in st)
                        //{
                        //    var subsubcategory = context.SubsubCategorys.Where(x => x.IsActive == true && x.SubsubCategoryid == ab.SubsubCategoryid && x.Deleted == false).Select(x => new factorySubSubCategory()
                        //    {
                        //        Categoryid = x.Categoryid,
                        //        SubCategoryId = x.SubCategoryId,
                        //        SubsubCategoryid = x.SubsubCategoryid,
                        //        SubsubcategoryName = x.SubsubcategoryName
                        //    }).ToList();
                        //    foreach (var sub in subsubcategory)
                        //    {
                        //        if (item.SubsubCategories == null)
                        //        {
                        //            item.SubsubCategories = new List<factorySubSubCategory>();
                        //            item.SubsubCategories.Add(sub);
                        //        }
                        //        else
                        //        {
                        //            var data = item.SubsubCategories.Where(x => x.SubsubCategoryid == sub.SubsubCategoryid).FirstOrDefault();
                        //            if (data == null)
                        //            {
                        //                item.SubsubCategories.Add(sub);
                        //            }
                        //        }
                        //    }




                        //Increase some parameter For offer 
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == cw.Warehouseid && a.Deleted == false && a.active == true && a.Categoryid == catid)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = b.IsOffer,
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
                                           FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
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
                            catch { }
                            item.ItemMasters.Add(it);
                        }
                        //    }
                        //}
                        //else
                        //{
                        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                        //}
                    }
                    if (item.ItemMasters != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        #endregion

        #region RA V2:  for customer app get item by cat id # Version 2
        /// <summary>
        ///  # Version 2
        ///  for customer app get item by cat id 
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="customerId"></param>
        /// <param name="catid"></param>
        /// <returns></returns>
        [Route("V2")]
        [HttpGet]
        public HttpResponseMessage ItemgetbyCategoryIdv2(string lang, int customerId, int catid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var custBrand = context.Customers.Where(x => x.CustomerId == customerId && x.Deleted == false).ToList();
                    WRSITEM item = new WRSITEM();
                    foreach (var cw in custBrand)
                    {

                        //Increase some parameter For offer 
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == cw.Warehouseid && a.Deleted == false && a.active == true && a.Categoryid == catid)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           IsSensitive = a.IsSensitive,
                                           IsSensitiveMRP = a.IsSensitiveMRP,
                                           UnitofQuantity = a.UnitofQuantity,
                                           UOM = a.UOM,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = b.IsOffer,
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
                                           FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
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
                                }
                            }
                            catch { }
                            item.ItemMasters.Add(it);
                        }
                        // }
                        //}
                        //else
                        //{
                        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                        //}
                    }
                    if (item.ItemMasters != null)
                    {
                        item.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        #endregion

        //#region SPA:  for Salesperson app get item by cat id
        //[Route("")]
        //[HttpGet]
        //public HttpResponseMessage salesItemgetbyCategoryId(int spid, int catid)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            List<WRSITEM> brandItem = new List<WRSITEM>();
        //            var custBrand = context.Peoples.Where(x => x.PeopleID == spid && x.Deleted == false && x.WarehouseId != 0 && x.Active == true).ToList();
        //            WRSITEM item = new WRSITEM();
        //            foreach (var cw in custBrand)
        //            {
        //                // By Sachin For Offer Inactive
        //                //try
        //                //{
        //                //    var data = context.OfferDb.Where(x => x.IsDeleted == false && x.IsActive == true && x.start <= DateTime.Today && x.end <= DateTime.Today).ToList();
        //                //    if (data.Count > 0)
        //                //    {
        //                //        foreach (var offer in data)
        //                //        {
        //                //            var item1 = context.itemMasters.Where(x => x.IsOffer == true && x.ItemId == offer.itemId && x.WarehouseId == cw.WarehouseId && x.Deleted == false).FirstOrDefault();
        //                //            if (item1 != null)
        //                //            {
        //                //                var data5 = context.itemMasters.Where(x => x.Number == item1.Number && x.WarehouseId == cw.WarehouseId).ToList();
        //                //                foreach (var itemnumber in data5)
        //                //                {
        //                //                    var item5 = context.itemMasters.Where(x => x.ItemId == itemnumber.ItemId && x.WarehouseId == cw.WarehouseId).SingleOrDefault();
        //                //                    item5.IsOffer = false;
        //                //                    context.itemMasters.Attach(item5);
        //                //                    context.Entry(item5).State = EntityState.Modified;
        //                //                    context.SaveChanges();
        //                //                }
        //                //            }
        //                //            var kk = context.OfferDb.Where(x => x.OfferId == offer.OfferId).SingleOrDefault();
        //                //            kk.IsActive = false;
        //                //            context.OfferDb.Attach(kk);
        //                //            context.Entry(kk).State = EntityState.Modified;
        //                //            context.SaveChanges();
        //                //        }
        //                //    }
        //                //}
        //                //catch (Exception ex) { logger.Error(ex.Message); }
        //                //List<SubsubCategory> st = new List<SubsubCategory>();
        //                //var scategory = context.SubsubCategorys.Where(x => x.IsActive == true && x.Categoryid == catid && x.Deleted == false).ToList();
        //                //if (scategory != null)
        //                //{
        //                //    foreach (var a in scategory)
        //                //    {
        //                //        if (a != null)
        //                //        {
        //                //            bool ItemFound = context.itemMasters.Any(x => x.active == true && x.Deleted == false && x.SubsubCategoryid == a.SubsubCategoryid && x.WarehouseId == cw.WarehouseId);
        //                //            if (ItemFound == true)
        //                //            {
        //                //                st.Add(a);
        //                //            }
        //                //        }
        //                //    }
        //                //    foreach (var ab in st)
        //                //    {
        //                //        var subsubcategory = context.SubsubCategorys.Where(x => x.IsActive == true && x.SubsubCategoryid == ab.SubsubCategoryid && x.Deleted == false).Select(x => new factorySubSubCategory()
        //                //        {
        //                //            Categoryid = x.Categoryid,
        //                //            SubCategoryId = x.SubCategoryId,
        //                //            SubsubCategoryid = x.SubsubCategoryid,
        //                //            SubsubcategoryName = x.SubsubcategoryName
        //                //        }).ToList();
        //                //        foreach (var sub in subsubcategory)
        //                //        {
        //                //            if (item.SubsubCategories == null)
        //                //            {
        //                //                item.SubsubCategories = new List<factorySubSubCategory>();
        //                //                item.SubsubCategories.Add(sub);
        //                //            }
        //                //            else
        //                //            {
        //                //                var data = item.SubsubCategories.Where(x => x.SubsubCategoryid == sub.SubsubCategoryid).FirstOrDefault();
        //                //                if (data == null)
        //                //                {
        //                //                    item.SubsubCategories.Add(sub);
        //                //                }
        //                //            }
        //                //        }
        //                //Increase some parameter For offer
        //                var newdata = (from a in context.itemMasters
        //                               where (a.WarehouseId == cw.WarehouseId && a.Deleted == false && a.active == true && a.Categoryid == catid)
        //                               join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
        //                               select new factoryItemdata
        //                               {
        //                                   WarehouseId = a.WarehouseId,
        //                                   CompanyId = a.CompanyId,
        //                                   Categoryid = b.Categoryid,
        //                                   Discount = b.Discount,
        //                                   ItemId = a.ItemId,
        //                                   ItemNumber = b.Number,
        //                                   itemname = a.itemname,
        //                                   LogoUrl = b.LogoUrl,
        //                                   MinOrderQty = b.MinOrderQty,
        //                                   price = a.price,
        //                                   SubCategoryId = b.SubCategoryId,
        //                                   SubsubCategoryid = b.SubsubCategoryid,
        //                                   TotalTaxPercentage = b.TotalTaxPercentage,
        //                                   SellingUnitName = b.SellingUnitName,
        //                                   SellingSku = b.SellingSku,
        //                                   UnitPrice = a.UnitPrice,
        //                                   HindiName = a.HindiName,
        //                                   VATTax = b.VATTax,
        //                                   active = a.active,
        //                                   marginPoint = a.marginPoint,
        //                                   promoPerItems = a.promoPerItems,
        //                                   NetPurchasePrice = a.NetPurchasePrice,
        //                                   IsOffer = b.IsOffer,
        //                                   Deleted = a.Deleted,
        //                                   OfferCategory = a.OfferCategory,
        //                                   OfferStartTime = a.OfferStartTime,
        //                                   OfferEndTime = a.OfferEndTime,
        //                                   OfferQtyAvaiable = a.OfferQtyAvaiable,
        //                                   OfferQtyConsumed = a.OfferQtyConsumed,
        //                                   OfferId = a.OfferId,
        //                                   OfferType = a.OfferType,
        //                                   OfferWalletPoint = a.OfferWalletPoint,
        //                                   OfferFreeItemId = a.OfferFreeItemId,
        //                                   OfferPercentage = a.OfferPercentage,
        //                                   OfferFreeItemName = a.OfferFreeItemName,
        //                                   OfferFreeItemImage = a.OfferFreeItemImage,
        //                                   OfferFreeItemQuantity = a.OfferFreeItemQuantity,
        //                                   OfferMinimumQty = a.OfferMinimumQty,
        //                                   FlashDealSpecialPrice = a.FlashDealSpecialPrice,
        //                                   FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
        //                               }).OrderByDescending(x => x.ItemNumber).ToList();
        //                foreach (var it in newdata)
        //                {
        //                    if (item.ItemMasters == null)
        //                    {
        //                        item.ItemMasters = new List<factoryItemdata>();
        //                    }
        //                    try
        //                    {
        //                        if (!it.IsOffer)
        //                        {
        //                            /// Dream Point Logic && Margin Point
        //                            int? MP, PP;
        //                            double xPoint = xPointValue * 10;
        //                            //Customer (0.2 * 10=1)
        //                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                            {
        //                                PP = 0;
        //                            }
        //                            else
        //                            {
        //                                PP = it.promoPerItems;
        //                            }
        //                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                            {
        //                                MP = 0;
        //                            }
        //                            else
        //                            {
        //                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                            }
        //                            if (PP > 0 && MP > 0)
        //                            {
        //                                int? PP_MP = PP + MP;
        //                                it.dreamPoint = PP_MP;
        //                            }
        //                            else if (MP > 0)
        //                            {
        //                                it.dreamPoint = MP;
        //                            }
        //                            else if (PP > 0)
        //                            {
        //                                it.dreamPoint = PP;
        //                            }
        //                            else
        //                            {
        //                                it.dreamPoint = 0;
        //                            }
        //                            // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                            if (it.price > it.UnitPrice)
        //                            {
        //                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                            }
        //                            else
        //                            {
        //                                it.marginPoint = 0;
        //                            }
        //                        }
        //                    }
        //                    catch { }
        //                    item.ItemMasters.Add(it);
        //                }
        //                //    }
        //                //}
        //                //else
        //                //{
        //                //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
        //                //}
        //            }
        //            if (item.ItemMasters != null)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, item);
        //            }
        //            else
        //            {
        //                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }
        //    }
        //}
        //#endregion

        #region SPA V2:  for Salesperson app get item by cat id # version 2
        /// <summary>
        /// Version 2
        /// for Salesperson app get item by cat id # version 2
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="spid"></param>
        /// <param name="catid"></param>
        /// <returns></returns>
        [Route("V2")]
        [HttpGet]
        public HttpResponseMessage salesItemgetbyCategoryIdv2(string lang, int spid, int catid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var custBrand = context.Peoples.Where(x => x.PeopleID == spid && x.Deleted == false && x.WarehouseId != 0 && x.Active == true).ToList();
                    WRSITEM item = new WRSITEM();
                    foreach (var cw in custBrand)
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == cw.WarehouseId && a.Deleted == false && a.active == true && a.Categoryid == catid)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = b.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
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
                                           FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
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
                                }
                            }
                            catch { }
                            item.ItemMasters.Add(it);
                        }
                        //    }
                        //}
                        //else
                        //{
                        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                        //}
                    }
                    if (item.ItemMasters != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        #endregion

        #region SPA: for Salesperson app get item by Cat & Excutive id  
        /// <summary>
        /// note use this api... 
        /// </summary>
        /// <param name="ExecutiveId"></param>
        /// <param name="catid"></param>
        /// <returns></returns>
        [Route("V2")]
        [HttpGet]
        public HttpResponseMessage ItemgetbysalespId(int ExecutiveId, int catid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    //  var custBrand = context.CustWarehouseDB.Where(x => x.ExecutiveId == ExecutiveId).FirstOrDefault();
                    var custBrand = context.Peoples.Where(x => x.PeopleID == ExecutiveId && x.Deleted == false && x.WarehouseId != 0 && x.Active == true).FirstOrDefault();

                    WRSITEM item = new WRSITEM();
                    var scategory = context.SubsubCategorys.Where(x => x.IsActive == true && x.Categoryid == catid && x.Deleted == false).ToList();
                    List<SubsubCategory> sst = new List<SubsubCategory>();
                    foreach (var sac in scategory)
                    {
                        bool ItemFound = context.itemMasters.Any(x => x.active == true && x.Deleted == false && x.SubsubCategoryid == sac.SubsubCategoryid && x.WarehouseId == custBrand.WarehouseId);
                        if (ItemFound == true)
                        {
                            sst.Add(sac);
                        }
                    }
                    foreach (var ab in sst)
                    {
                        var subsubcategory = context.SubsubCategorys.Where(x => x.IsActive == true && x.SubsubCategoryid == ab.SubsubCategoryid && x.Deleted == false).Select(x => new factorySubSubCategory()
                        {
                            Categoryid = x.Categoryid,
                            SubCategoryId = x.SubCategoryId,
                            SubsubCategoryid = x.SubsubCategoryid,
                            SubsubcategoryName = x.SubsubcategoryName
                        }).ToList();
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == custBrand.WarehouseId && a.Deleted == false && a.active == true && a.SubsubCategoryid == ab.SubsubCategoryid)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
                                           active = a.active,
                                           marginPoint = a.marginPoint,
                                           promoPerItems = a.promoPerItems,
                                           NetPurchasePrice = a.NetPurchasePrice,
                                           IsOffer = b.IsOffer,
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
                                           FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var sub in subsubcategory)
                        {
                            if (item.SubsubCategories == null)
                            {
                                item.SubsubCategories = new List<factorySubSubCategory>();
                                item.SubsubCategories.Add(sub);
                            }
                            else
                            {
                                var data = item.SubsubCategories.Where(x => x.SubsubCategoryid == sub.SubsubCategoryid).FirstOrDefault();
                                if (data == null)
                                {
                                    item.SubsubCategories.Add(sub);
                                }
                            }
                        }
                        foreach (var it in newdata)
                        {
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
                            catch { }

                            item.ItemMasters.Add(it);
                        }
                    }
                    if (item.ItemMasters != null && item.ItemMasters.Count > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Item Master " + ex.Message);
                    return null;
                }
            }
        }

        #endregion

        #region RA & SPA V2:  get all Brand item By ID # version 2
        /// <summary>
        /// get all Brand item By ID # version 2
        /// </summary>
        /// <param name="warid"></param>
        /// <param name="brandName"></param>
        /// <returns></returns>
        [Route("GetAllItemByBrand/V2")]
        [HttpGet]
        public HttpResponseMessage getItemOnBrandv2(string lang, int warid, string brandName)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    WRSITEM item = new WRSITEM();
                    //  factoryItemdata item = new factoryItemdata();
                    if (lang.Trim() == "hi")
                    {

                        //Increase some parameter For offer
                        var newdatahi = (from a in context.itemMasters
                                         where (a.WarehouseId == warid && a.Deleted == false && a.active == true && a.SubsubcategoryName.Trim().ToLower() == brandName.Trim().ToLower() && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                         join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                         select new factoryItemdata
                                         {
                                             WarehouseId = a.WarehouseId,
                                             CompanyId = a.CompanyId,
                                             Categoryid = b.Categoryid,
                                             Discount = b.Discount,
                                             ItemId = a.ItemId,
                                             ItemNumber = b.Number,
                                             itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                             LogoUrl = b.LogoUrl,
                                             MinOrderQty = b.MinOrderQty,
                                             price = a.price,
                                             SubCategoryId = b.SubCategoryId,
                                             SubsubCategoryid = b.SubsubCategoryid,
                                             TotalTaxPercentage = b.TotalTaxPercentage,
                                             SellingUnitName = a.SellingUnitName,
                                             SellingSku = b.SellingSku,
                                             UnitPrice = a.UnitPrice,
                                             HindiName = a.HindiName,
                                             VATTax = b.VATTax,
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
                                             BillLimitQty = a.BillLimitQty,
                                             
                                         }).OrderByDescending(x => x.ItemNumber).ToList();

                        foreach (factoryItemdata it in newdatahi)
                        {
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
                            catch (Exception es)
                            {

                            }

                            if (it.HindiName != null)
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
                            item.ItemMasters.Add(it);
                        }

                    }
                    else
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == warid && a.Deleted == false && a.active == true && a.SubsubcategoryName.Trim().ToLower() == brandName.Trim().ToLower())
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
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
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();

                        foreach (factoryItemdata it in newdata)
                        {
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
                            catch (Exception es)
                            {

                            }
                            item.ItemMasters.Add(it);
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new WRSITEM() { ItemMasters = item.ItemMasters, Message = true });
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                }
            }
        }


        ///// <summary>
        ///// get all Brand item By ID # version 2
        ///// </summary>
        ///// <param name="warid"></param>
        ///// <param name="brandName"></param>
        ///// <returns></returns>
        //[Route("GetAllItemByBrand/V3")]
        //[HttpGet]
        //public HttpResponseMessage getPeopleItemOnBrandv3(string lang, int peopleid, int warehouseid, string brandName)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            WRSITEM item = new WRSITEM();
        //            //  factoryItemdata item = new factoryItemdata();
        //            if (lang.Trim() == "hi")
        //            {
        //                //Increase some parameter For offer
        //                var newdatahi = (from a in context.itemMasters
        //                                 where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.SubsubcategoryName.Trim().ToLower() == brandName.Trim().ToLower() && (a.ItemAppType == 0 || a.ItemAppType == 1))
        //                                 join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
        //                                 let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

        //                                 select new factoryItemdata
        //                                 {
        //                                     WarehouseId = a.WarehouseId,
        //                                     IsItemLimit = limit != null ? limit.IsItemLimit : false,
        //                                     ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
        //                                     CompanyId = a.CompanyId,
        //                                     Categoryid = b.Categoryid,
        //                                     Discount = b.Discount,
        //                                     ItemId = a.ItemId,
        //                                     ItemNumber = b.Number,
        //                                     itemname = a.HindiName != null ? a.HindiName : a.itemname,
        //                                     LogoUrl = b.LogoUrl,
        //                                     MinOrderQty = b.MinOrderQty,
        //                                     price = a.price,
        //                                     SubCategoryId = b.SubCategoryId,
        //                                     SubsubCategoryid = b.SubsubCategoryid,
        //                                     TotalTaxPercentage = b.TotalTaxPercentage,
        //                                     SellingUnitName = a.SellingUnitName,
        //                                     SellingSku = b.SellingSku,
        //                                     UnitPrice = a.UnitPrice,
        //                                     HindiName = a.HindiName,
        //                                     VATTax = b.VATTax,
        //                                     active = a.active,
        //                                     dreamPoint = 0,
        //                                     marginPoint = a.marginPoint,
        //                                     NetPurchasePrice = a.NetPurchasePrice,
        //                                     promoPerItems = a.promoPerItems,
        //                                     IsOffer = a.IsOffer,
        //                                     Deleted = a.Deleted,
        //                                     IsSensitive = b.IsSensitive,
        //                                     OfferCategory = a.OfferCategory,
        //                                     OfferStartTime = a.OfferStartTime,
        //                                     OfferEndTime = a.OfferEndTime,
        //                                     OfferQtyAvaiable = a.OfferQtyAvaiable,
        //                                     OfferQtyConsumed = a.OfferQtyConsumed,
        //                                     OfferId = a.OfferId,
        //                                     OfferType = a.OfferType,
        //                                     OfferWalletPoint = a.OfferWalletPoint,
        //                                     OfferFreeItemId = a.OfferFreeItemId,
        //                                     OfferPercentage = a.OfferPercentage,
        //                                     OfferFreeItemName = a.OfferFreeItemName,
        //                                     OfferFreeItemImage = a.OfferFreeItemImage,
        //                                     OfferFreeItemQuantity = a.OfferFreeItemQuantity,
        //                                     OfferMinimumQty = a.OfferMinimumQty,
        //                                     FlashDealSpecialPrice = a.FlashDealSpecialPrice,
        //                                     FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
        //                                     ItemMultiMRPId = a.ItemMultiMRPId,
        //                                     BillLimitQty = a.BillLimitQty
        //                                 }).OrderByDescending(x => x.ItemNumber).ToList();

        //                var offerids = newdatahi.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
        //                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Sales App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();


        //                foreach (factoryItemdata it in newdatahi)
        //                {
        //                    if (it.OfferCategory == 2)
        //                    {
        //                        it.IsOffer = false;
        //                        it.FlashDealSpecialPrice = 0;
        //                        it.OfferCategory = 0;
        //                    }
        //                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
        //                    {
        //                        if (it.OfferCategory == 1)
        //                        {
        //                            it.IsOffer = false;
        //                            it.OfferCategory = 0;
        //                        }

        //                    }

        //                    if (it.OfferType != "FlashDeal")
        //                    {

        //                        if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
        //                            it.IsOffer = true;
        //                        else
        //                            it.IsOffer = false;
        //                    }

        //                    if (item.ItemMasters == null)
        //                    {
        //                        item.ItemMasters = new List<factoryItemdata>();
        //                    }
        //                    try
        //                    {
        //                        if (!it.IsOffer)
        //                        {
        //                            /// Dream Point Logic && Margin Point
        //                            int? MP, PP;
        //                            double xPoint = xPointValue * 10;
        //                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                            {
        //                                PP = 0;
        //                            }
        //                            else
        //                            {
        //                                PP = it.promoPerItems;
        //                            }
        //                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                            {
        //                                MP = 0;
        //                            }
        //                            else
        //                            {
        //                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                            }
        //                            if (PP > 0 && MP > 0)
        //                            {
        //                                int? PP_MP = PP + MP;
        //                                it.dreamPoint = PP_MP;
        //                            }
        //                            else if (MP > 0)
        //                            {
        //                                it.dreamPoint = MP;
        //                            }
        //                            else if (PP > 0)
        //                            {
        //                                it.dreamPoint = PP;
        //                            }
        //                            else
        //                            {
        //                                it.dreamPoint = 0;
        //                            }

        //                        }
        //                        else
        //                            it.dreamPoint = 0;
        //                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                        if (it.price > it.UnitPrice)
        //                        {
        //                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                        }
        //                        else
        //                        {
        //                            it.marginPoint = 0;
        //                        }
        //                    }
        //                    catch (Exception es)
        //                    {

        //                    }

        //                    if (lang.Trim() == "hi")
        //                    {
        //                        if (it.IsSensitive == true)
        //                        {
        //                            if (it.IsSensitiveMRP == false)
        //                            {
        //                                it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
        //                            }
        //                            else
        //                            {
        //                                it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
        //                            }
        //                        }
        //                        else
        //                        {
        //                            it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
        //                        }
        //                    }

        //                    item.ItemMasters.Add(it);
        //                }

        //            }
        //            else
        //            {
        //                //Increase some parameter For offer
        //                var newdata = (from a in context.itemMasters
        //                               where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.SubsubcategoryName.Trim().ToLower() == brandName.Trim().ToLower() && (a.ItemAppType == 0 || a.ItemAppType == 1))
        //                               join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
        //                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
        //                               select new factoryItemdata
        //                               {
        //                                   WarehouseId = a.WarehouseId,
        //                                   IsItemLimit = limit != null ? limit.IsItemLimit : false,
        //                                   ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
        //                                   CompanyId = a.CompanyId,
        //                                   Categoryid = b.Categoryid,
        //                                   Discount = b.Discount,
        //                                   ItemId = a.ItemId,
        //                                   ItemNumber = b.Number,
        //                                   itemname = a.itemname,
        //                                   LogoUrl = b.LogoUrl,
        //                                   MinOrderQty = b.MinOrderQty,
        //                                   price = a.price,
        //                                   SubCategoryId = b.SubCategoryId,
        //                                   SubsubCategoryid = b.SubsubCategoryid,
        //                                   TotalTaxPercentage = b.TotalTaxPercentage,
        //                                   SellingUnitName = a.SellingUnitName,
        //                                   SellingSku = b.SellingSku,
        //                                   UnitPrice = a.UnitPrice,
        //                                   HindiName = a.HindiName,
        //                                   VATTax = b.VATTax,
        //                                   active = a.active,
        //                                   dreamPoint = 0,
        //                                   marginPoint = a.marginPoint,
        //                                   NetPurchasePrice = a.NetPurchasePrice,
        //                                   promoPerItems = a.promoPerItems,
        //                                   IsOffer = a.IsOffer,
        //                                   Deleted = a.Deleted,
        //                                   OfferCategory = a.OfferCategory,
        //                                   OfferStartTime = a.OfferStartTime,
        //                                   OfferEndTime = a.OfferEndTime,
        //                                   OfferQtyAvaiable = a.OfferQtyAvaiable,
        //                                   OfferQtyConsumed = a.OfferQtyConsumed,
        //                                   OfferId = a.OfferId,
        //                                   OfferType = a.OfferType,
        //                                   OfferWalletPoint = a.OfferWalletPoint,
        //                                   OfferFreeItemId = a.OfferFreeItemId,
        //                                   OfferPercentage = a.OfferPercentage,
        //                                   OfferFreeItemName = a.OfferFreeItemName,
        //                                   OfferFreeItemImage = a.OfferFreeItemImage,
        //                                   OfferFreeItemQuantity = a.OfferFreeItemQuantity,
        //                                   OfferMinimumQty = a.OfferMinimumQty,
        //                                   FlashDealSpecialPrice = a.FlashDealSpecialPrice,
        //                                   FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
        //                                   ItemMultiMRPId = a.ItemMultiMRPId,
        //                                   BillLimitQty = a.BillLimitQty
        //                               }).OrderByDescending(x => x.ItemNumber).ToList();

        //                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
        //                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Sales App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();


        //                foreach (factoryItemdata it in newdata)
        //                {
        //                    if (it.OfferCategory == 2)
        //                    {
        //                        it.IsOffer = false;
        //                        it.FlashDealSpecialPrice = 0;
        //                        it.OfferCategory = 0;
        //                    }
        //                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
        //                    {
        //                        if (it.OfferCategory == 1)
        //                        {
        //                            it.IsOffer = false;
        //                            it.OfferCategory = 0;
        //                        }

        //                    }


        //                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
        //                        it.IsOffer = true;
        //                    else
        //                        it.IsOffer = false;

        //                    if (item.ItemMasters == null)
        //                    {
        //                        item.ItemMasters = new List<factoryItemdata>();
        //                    }
        //                    try
        //                    {
        //                        if (!it.IsOffer)
        //                        {
        //                            /// Dream Point Logic && Margin Point
        //                            int? MP, PP;
        //                            double xPoint = xPointValue * 10;
        //                            //Customer (0.2 * 10=1)
        //                            if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                            {
        //                                PP = 0;
        //                            }
        //                            else
        //                            {
        //                                PP = it.promoPerItems;
        //                            }
        //                            if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                            {
        //                                MP = 0;
        //                            }
        //                            else
        //                            {
        //                                double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                            }
        //                            if (PP > 0 && MP > 0)
        //                            {
        //                                int? PP_MP = PP + MP;
        //                                it.dreamPoint = PP_MP;
        //                            }
        //                            else if (MP > 0)
        //                            {
        //                                it.dreamPoint = MP;
        //                            }
        //                            else if (PP > 0)
        //                            {
        //                                it.dreamPoint = PP;
        //                            }
        //                            else
        //                            {
        //                                it.dreamPoint = 0;
        //                            }

        //                        }
        //                        else
        //                        {
        //                            it.dreamPoint = 0;
        //                        }

        //                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                        if (it.price > it.UnitPrice)
        //                        {
        //                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                        }
        //                        else
        //                        {
        //                            it.marginPoint = 0;
        //                        }
        //                    }
        //                    catch (Exception es)
        //                    {

        //                    }
        //                    item.ItemMasters.Add(it);
        //                }
        //            }


        //            return Request.CreateResponse(HttpStatusCode.OK, new WRSITEM() { ItemMasters = item.ItemMasters, Message = true });
        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
        //        }
        //    }
        //}
        #endregion

        #region RA: Get Sub cat item by custId Subsub id at app on home page
        /// <summary>
        /// Created by 19/12/2018 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="sscatid"></param>
        /// <returns></returns>

        [Route("getbysscatid")]
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage getbysscatid(int customerId, int sscatid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var custBrand = context.Customers.Where(x => x.CustomerId == customerId && x.Deleted == false).ToList();

                    WRSITEM item = new WRSITEM();
                    foreach (var cw in custBrand)
                    {

                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == cw.Warehouseid && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       join c in context.ItemLimitMasterDB on a.Number equals c.ItemNumber
                                       where (a.ItemMultiMRPId == c.ItemMultiMRPId && a.WarehouseId == c.WarehouseId)


                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           IsItemLimit = c.IsItemLimit,
                                           ItemlimitQty = c.ItemlimitQty,
                                           ItemMultiMRPId = a.ItemMultiMRPId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
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
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
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
                                else { it.dreamPoint = 0; }
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
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        #endregion

        #region RA V2: Get Sub cat item by custId Subsub id at app on home page
        /// <summary>
        /// # Version 2
        /// Created by 31/01/2019 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="sscatid"></param>
        /// <returns></returns>

        [Route("getbysscatid/V2")]
        [HttpGet]
        public HttpResponseMessage getbysscatidv2(string lang, int customerId, int sscatid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    //var custBrand = context.CustWarehouseDB.Where(x => x.CustomerId == customerId && x.Deleted == false).ToList();
                    var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    var warehouseid = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;
                    //var FlashDealWithItemIds = (from c in context.FlashDealItemConsumedDB.Where(x => x.CustomerId == customerId)
                    //                            join p in context.AppHomeDynamicDb.Where(x => x.Wid == warehouseid && x.active && !x.delete).SelectMany(x => x.detail.Select(y => new { id = y.id.Value, ItemId = y.ItemId }))
                    //                            on c.FlashDealId equals p.id into ps
                    //                            select new
                    //                            { c.FlashDealId, c.ItemId }).ToList();
                    //var FlashDealWithItemIds = context.FlashDealItemConsumedDB.Where(x => x.CustomerId == customerId).Select(x => new { x.FlashDealId, x.ItemId });
                    string sqlquery = "SELECT a.[FlashDealId] AS[FlashDealId], a.[ItemId] AS[ItemId] FROM[dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.[Deleted]= 0 and b.Active=1 and b.WarehouseID=" + warehouseid +
                                     " WHERE a.[CustomerId]=" + customerId;
                    var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();
                    WRSITEM item = new WRSITEM();
                    //foreach (var cw in custBrand)
                    //{
                    //// By Sachin For Offer Inactive
                    //try
                    //{
                    //    var data = context.OfferDb.Where(x => x.IsDeleted == false && x.IsActive == true && x.start <= DateTime.Today && x.end <= DateTime.Today).ToList();
                    //    if (data.Count > 0)
                    //    {
                    //        foreach (var offer in data)
                    //        {
                    //            var item1 = context.itemMasters.Where(x => x.IsOffer == true && x.ItemId == offer.itemId && x.Deleted == false && x.WarehouseId == cw.WarehouseId).FirstOrDefault();
                    //            if (item1 != null)
                    //            {
                    //                var data5 = context.itemMasters.Where(x => x.Number == item1.Number && x.WarehouseId == cw.WarehouseId).ToList();
                    //                foreach (var itemnumber in data5)
                    //                {
                    //                    var item5 = context.itemMasters.Where(x => x.ItemId == itemnumber.ItemId && x.WarehouseId == cw.WarehouseId).SingleOrDefault();
                    //                    item5.IsOffer = false;
                    //                    context.itemMasters.Attach(item5);
                    //                    context.Entry(item5).State = EntityState.Modified;
                    //                    context.SaveChanges();
                    //                }
                    //            }
                    //            var kk = context.OfferDb.Where(x => x.OfferId == offer.OfferId).SingleOrDefault();
                    //            kk.IsActive = false;
                    //            context.OfferDb.Attach(kk);
                    //            context.Entry(kk).State = EntityState.Modified;
                    //            context.SaveChanges();
                    //        }
                    //    }
                    //}
                    //catch (Exception ex) { logger.Error(ex.Message); }

                    //Increase some parameter For offer
                    if (lang.Trim() == "hi")
                    {
                        var newdatahi = (from a in context.itemMasters
                                         where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid)
                                         join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                         select new factoryItemdata
                                         {
                                             WarehouseId = a.WarehouseId,
                                             CompanyId = a.CompanyId,
                                             Categoryid = b.Categoryid,
                                             Discount = b.Discount,
                                             ItemId = a.ItemId,
                                             ItemNumber = b.Number,
                                             itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                             LogoUrl = b.LogoUrl,
                                             MinOrderQty = b.MinOrderQty,
                                             price = a.price,
                                             SubCategoryId = b.SubCategoryId,
                                             SubsubCategoryid = b.SubsubCategoryid,
                                             TotalTaxPercentage = b.TotalTaxPercentage,
                                             SellingUnitName = b.SellingUnitName,
                                             SellingSku = b.SellingSku,
                                             UnitPrice = a.UnitPrice,
                                             HindiName = a.HindiName,
                                             VATTax = b.VATTax,
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
                                             ItemMultiMRPId = a.ItemMultiMRPId
                                         }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdatahi)
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
                                if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                            }
                            if (item.ItemMasters == null)
                            {
                                item.ItemMasters = new List<factoryItemdata>();
                            }
                            try
                            {  /// Dream Point Logic && Margin Point
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

                            if (it.HindiName != null)
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

                            item.ItemMasters.Add(it);
                        }
                    }
                    else
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = b.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
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
                                           ItemMultiMRPId = a.ItemMultiMRPId
                                       }).OrderByDescending(x => x.ItemNumber).ToList();

                        foreach (var it in newdata)
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
                                if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                            }
                            if (item.ItemMasters == null)
                            {
                                item.ItemMasters = new List<factoryItemdata>();
                            }
                            try
                            {  /// Dream Point Logic && Margin Point
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
                                // Margin logic
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

                            item.ItemMasters.Add(it);
                        }
                    }
                    //}
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
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Not show offer and flashdeal on Inactive customer
        /// 28/05/2019
        /// Sudeep Solanki
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="customerId"></param>
        /// <param name="sscatid"></param>
        /// <returns></returns>
        [Route("getbysscatid/V3")]
        [HttpGet]
        public HttpResponseMessage getbysscatidv3(string lang, int customerId, int sscatid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                    var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;


                    string sqlquery = "SELECT a.[FlashDealId] AS[FlashDealId], a.[ItemId] AS[ItemId] FROM[dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.[Deleted]= 0 and b.Active=1 and b.WarehouseID=" + warehouseId +
                                     " WHERE a.[CustomerId]=" + customerId;
                    var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();
                    WRSITEM item = new WRSITEM();
                    //foreach (var cw in custBrand)
                    //{

                    //Increase some parameter For offer
                    if (lang.Trim() == "hi")
                    {
                        var newdatahi = (from a in context.itemMasters
                                         where (a.WarehouseId == warehouseId && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid)
                                         join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                         let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                                         select new factoryItemdata
                                         {
                                             WarehouseId = a.WarehouseId,
                                             IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                             ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                             CompanyId = a.CompanyId,
                                             Categoryid = b.Categoryid,
                                             Discount = b.Discount,
                                             ItemId = a.ItemId,
                                             ItemNumber = b.Number,
                                             itemname = !string.IsNullOrEmpty(a.HindiName) ? a.HindiName : a.itemname,
                                             LogoUrl = b.LogoUrl,
                                             MinOrderQty = b.MinOrderQty,
                                             price = a.price,
                                             SubCategoryId = b.SubCategoryId,
                                             SubsubCategoryid = b.SubsubCategoryid,
                                             TotalTaxPercentage = b.TotalTaxPercentage,
                                             SellingUnitName = b.SellingUnitName,
                                             SellingSku = b.SellingSku,
                                             UnitPrice = a.UnitPrice,
                                             HindiName = a.HindiName,
                                             itemBaseName = a.itemBaseName,
                                             IsSensitive = a.IsSensitive,
                                             IsSensitiveMRP = a.IsSensitiveMRP,
                                             VATTax = b.VATTax,
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
                                             ItemMultiMRPId = a.ItemMultiMRPId,
                                             BillLimitQty = a.BillLimitQty
                                         }).OrderByDescending(x => x.ItemNumber).ToList();

                        var offerids = newdatahi.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both" || x.OfferAppType == "Sales App").Select(x => x.OfferId).ToList() : new List<int>();


                        foreach (var it in newdatahi)
                        {
                            //Condition for offer end
                            if (!inActiveCustomer)
                            {
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
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
                                        it.OfferCategory = 0;
                                    }
                                }

                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }
                            else
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
                                        it.itemname = it.itemBaseName + " " + it.price + " MRP "; //item display name                               
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

                            if (it.HindiName != null)
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


                            item.ItemMasters.Add(it);
                        }
                    }
                    else
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == warehouseId && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                           ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = b.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
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
                                           ItemMultiMRPId = a.ItemMultiMRPId,
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();

                        var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();


                        foreach (var it in newdata)
                        {
                            if (!inActiveCustomer)
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
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
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
                            }
                            else
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
                                // Margin logic
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

                            item.ItemMasters.Add(it);
                        }
                    }
                    //}
                    if (item.ItemMasters != null && item.ItemMasters.Any())
                    {
                        item.Message = true;
                        item.ItemMasters.Where(x => !x.marginPoint.HasValue).ToList().ForEach(x => x.marginPoint = 0);
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        item.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// /// 28/05/2019
        /// Sudeep Solanki
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="customerId"></param>
        /// <param name="catid"></param>
        /// <param name="scatid"></param>
        /// <param name="sscatid"></param>
        /// <returns></returns>
        [Route("getItembycatesscatid")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getitembysscatid(string lang, int customerId, int catid, int scatid, int sscatid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                    var warehouseId = ActiveCustomer != null ? ActiveCustomer.Warehouseid : 0;


                    
                    WRSITEM item = new WRSITEM();
                   
                    //Increase some parameter For offer
                    if (lang.Trim() == "hi")
                    {
                        
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();


                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetItemByCatSubAndSubCat]";
                        cmd.Parameters.Add(new SqlParameter("@warehouseId", warehouseId));
                        cmd.Parameters.Add(new SqlParameter("@sscatid", sscatid));
                        cmd.Parameters.Add(new SqlParameter("@scatid", scatid));
                        cmd.Parameters.Add(new SqlParameter("@catid", catid));
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var newdatahi = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<factoryItemdata>(reader).ToList();

                        var offerids = newdatahi.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();


                        foreach (var it in newdatahi.Where(a=> (a.ItemAppType == 0 || a.ItemAppType == 1)))
                        {
                            //Condition for offer end
                            if (!inActiveCustomer)
                            {
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
                                
                                if (activeOfferids!=null && activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }
                            else
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

                            if (it.HindiName != null)
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

                            item.ItemMasters.Add(it);
                        }
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
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var newdata = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<factoryItemdata>(reader).ToList();

                        var offerids = newdata.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();


                        foreach (var it in newdata.Where(a => (a.ItemAppType == 0 || a.ItemAppType == 1)))
                        {
                            if (!inActiveCustomer)
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

                                if (activeOfferids != null && activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }
                            else
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
                                // Margin logic
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

                            item.ItemMasters.Add(it);
                        }
                    }
                    //}
                    if (item.ItemMasters != null && item.ItemMasters.Any())
                    {
                        item.Message = true;
                        item.ItemMasters.Where(x => !x.marginPoint.HasValue).ToList().ForEach(x => x.marginPoint = 0);
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        item.Message = false;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        #endregion


        //#region SPA: for sales person app Get Sub cat item by spid Subsub id at app on home page
        ///// <summary>
        ///// Created by 19/12/2018 
        ///// </summary>
        ///// <param name="spid"></param>
        ///// <param name="sscatid"></param>
        ///// <returns></returns>

        //[Route("getbysscatid")]
        //[HttpGet]
        //public HttpResponseMessage salesgetbysscatid(int spid, int sscatid)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            List<WRSITEM> brandItem = new List<WRSITEM>();
        //            var custBrand = context.Peoples.Where(x => x.PeopleID == spid && x.Deleted == false && x.WarehouseId != 0 && x.Active == true).ToList();

        //            WRSITEM item = new WRSITEM();
        //            foreach (var cw in custBrand)
        //            {
        //                //Increase some parameter For offer
        //                var newdata = (from a in context.itemMasters
        //                               where (a.WarehouseId == cw.WarehouseId && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid && (a.ItemAppType == 0 || a.ItemAppType == 1))
        //                               join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
        //                               let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
        //                               select new factoryItemdata
        //                               {
        //                                   WarehouseId = a.WarehouseId,
        //                                   CompanyId = a.CompanyId,
        //                                   IsItemLimit = limit != null ? limit.IsItemLimit : false,
        //                                   ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
        //                                   Categoryid = b.Categoryid,
        //                                   Discount = b.Discount,
        //                                   ItemId = a.ItemId,
        //                                   ItemNumber = b.Number,
        //                                   itemname = a.itemname,
        //                                   LogoUrl = b.LogoUrl,
        //                                   MinOrderQty = b.MinOrderQty,
        //                                   price = a.price,
        //                                   SubCategoryId = b.SubCategoryId,
        //                                   SubsubCategoryid = b.SubsubCategoryid,
        //                                   TotalTaxPercentage = b.TotalTaxPercentage,
        //                                   SellingUnitName = a.SellingUnitName,
        //                                   SellingSku = b.SellingSku,
        //                                   UnitPrice = a.UnitPrice,
        //                                   HindiName = a.HindiName,
        //                                   VATTax = b.VATTax,
        //                                   active = a.active,
        //                                   marginPoint = a.marginPoint,
        //                                   NetPurchasePrice = a.NetPurchasePrice,
        //                                   promoPerItems = a.promoPerItems,
        //                                   IsOffer = a.IsOffer,
        //                                   Deleted = a.Deleted,
        //                                   OfferCategory = a.OfferCategory,
        //                                   OfferStartTime = a.OfferStartTime,
        //                                   OfferEndTime = a.OfferEndTime,
        //                                   OfferQtyAvaiable = a.OfferQtyAvaiable,
        //                                   OfferQtyConsumed = a.OfferQtyConsumed,
        //                                   OfferId = a.OfferId,
        //                                   OfferType = a.OfferType,
        //                                   OfferWalletPoint = a.OfferWalletPoint,
        //                                   OfferFreeItemId = a.OfferFreeItemId,
        //                                   OfferPercentage = a.OfferPercentage,
        //                                   OfferFreeItemName = a.OfferFreeItemName,
        //                                   OfferFreeItemImage = a.OfferFreeItemImage,
        //                                   OfferFreeItemQuantity = a.OfferFreeItemQuantity,
        //                                   OfferMinimumQty = a.OfferMinimumQty,
        //                                   FlashDealSpecialPrice = a.FlashDealSpecialPrice,
        //                                   FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake
        //                               }).OrderByDescending(x => x.ItemNumber).ToList();
        //                foreach (var it in newdata)
        //                {
        //                    if (item.ItemMasters == null)
        //                    {
        //                        item.ItemMasters = new List<factoryItemdata>();
        //                    }
        //                    try
        //                    {

        //                        try
        //                        {
        //                            if (!it.IsOffer)
        //                            {
        //                                /// Dream Point Logic && Margin Point
        //                                int? MP, PP;
        //                                double xPoint = xPointValue * 10;
        //                                //salesman 0.2=(0.02 * 10=0.2)
        //                                if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                                {
        //                                    PP = 0;
        //                                }
        //                                else
        //                                {
        //                                    PP = it.promoPerItems;
        //                                }
        //                                if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                                {
        //                                    MP = 0;
        //                                }
        //                                else
        //                                {
        //                                    double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                                    MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                                }
        //                                if (PP > 0 && MP > 0)
        //                                {
        //                                    int? PP_MP = PP + MP;
        //                                    it.dreamPoint = PP_MP;
        //                                }
        //                                else if (MP > 0)
        //                                {
        //                                    it.dreamPoint = MP;
        //                                }
        //                                else if (PP > 0)
        //                                {
        //                                    it.dreamPoint = PP;
        //                                }
        //                                else
        //                                {
        //                                    it.dreamPoint = 0;
        //                                }
        //                                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                                if (it.price > it.UnitPrice)
        //                                {
        //                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                                }
        //                                else
        //                                {
        //                                    it.marginPoint = 0;
        //                                }
        //                            }
        //                        }
        //                        catch (Exception ds) { }
        //                    }
        //                    catch { }
        //                    item.ItemMasters.Add(it);
        //                }
        //            }
        //            if (item.ItemMasters != null)
        //            {
        //                item.Message = true;
        //                return Request.CreateResponse(HttpStatusCode.OK, item);
        //            }
        //            else
        //            {
        //                item.Message = false;
        //                return Request.CreateResponse(HttpStatusCode.OK, item);
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }
        //    }
        //}
        //#endregion

        #region SPA V2: for sales person app Get Sub cat item by spid Subsub id at app on home page
        /// <summary>
        /// # Version 2
        /// Created by 19/12/2018 
        /// </summary>
        /// <param name="spid"></param>
        /// <param name="sscatid"></param>
        /// <returns></returns>

        [Route("getbysscatid/V2")]
        [HttpGet]
        public HttpResponseMessage salesgetbysscatidv2(string lang, int spid, int sscatid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var custBrand = context.Peoples.Where(x => x.PeopleID == spid && x.Deleted == false && x.WarehouseId != 0 && x.Active == true).ToList();

                    WRSITEM item = new WRSITEM();
                    foreach (var cw in custBrand)
                    {
                        // By Sachin For Offer Inactive
                        //try
                        //{
                        //var data = context.OfferDb.Where(x => x.IsDeleted == false && x.IsActive == true && x.start <= DateTime.Today && x.end <= DateTime.Today).ToList();
                        //if (data.Count > 0)
                        //{
                        //    foreach (var offer in data)
                        //    {
                        //        var item1 = context.itemMasters.Where(x => x.IsOffer == true && x.ItemId == offer.itemId && x.Deleted == false && x.WarehouseId == cw.WarehouseId).FirstOrDefault();
                        //        if (item1 != null)
                        //        {
                        //            var data5 = context.itemMasters.Where(x => x.Number == item1.Number && x.WarehouseId == cw.WarehouseId).ToList();
                        //            foreach (var itemnumber in data5)
                        //            {
                        //                var item5 = context.itemMasters.Where(x => x.ItemId == itemnumber.ItemId && x.WarehouseId == cw.WarehouseId).SingleOrDefault();
                        //                item5.IsOffer = false;
                        //                context.itemMasters.Attach(item5);
                        //                context.Entry(item5).State = EntityState.Modified;
                        //                context.SaveChanges();
                        //            }
                        //        }
                        //        var kk = context.OfferDb.Where(x => x.OfferId == offer.OfferId).SingleOrDefault();
                        //        kk.IsActive = false;
                        //        context.OfferDb.Attach(kk);
                        //        context.Entry(kk).State = EntityState.Modified;
                        //        context.SaveChanges();
                        //    }
                        //}
                        //}
                        //catch (Exception ex) { logger.Error(ex.Message); }

                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == cw.WarehouseId && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           IsSensitive = a.IsSensitive,
                                           IsSensitiveMRP = a.IsSensitiveMRP,
                                           UnitofQuantity = a.UnitofQuantity,
                                           UOM = a.UOM,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
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
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {

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
                    }
                    if (item.ItemMasters != null)
                    {
                        item.Message = true;
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        ///// <summary>
        ///// # Version 2
        ///// Created by 19/12/2018 
        ///// </summary>
        ///// <param name="spid"></param>
        ///// <param name="sscatid"></param>
        ///// <returns></returns>

        //[Route("getItembysscatid")]
        //[HttpGet]
        //public HttpResponseMessage salesgetbysscatidv3(string lang, int peopleid, int warehouseid, int catid, int scatid, int sscatid)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            WRSITEM item = new WRSITEM();
        //            //Increase some parameter For offer
        //            var newdata = (from a in context.itemMasters
        //                           where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && a.SubsubCategoryid == sscatid && a.SubCategoryId == scatid && a.Categoryid == catid && (a.ItemAppType == 0 || a.ItemAppType == 1))
        //                           //join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
        //                           let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
        //                           select new factoryItemdata
        //                           {
        //                               WarehouseId = a.WarehouseId,
        //                               IsItemLimit = limit != null ? limit.IsItemLimit : false,
        //                               ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
        //                               CompanyId = a.CompanyId,
        //                               Categoryid = a.Categoryid,
        //                               Discount = a.Discount,
        //                               ItemId = a.ItemId,
        //                               ItemNumber = a.Number,
        //                               itemname = a.itemname,
        //                               IsSensitive = a.IsSensitive,
        //                               IsSensitiveMRP = a.IsSensitiveMRP,
        //                               UnitofQuantity = a.UnitofQuantity,
        //                               UOM = a.UOM,
        //                               LogoUrl = a.LogoUrl,
        //                               MinOrderQty = a.MinOrderQty,
        //                               price = a.price,
        //                               SubCategoryId = a.SubCategoryId,
        //                               SubsubCategoryid = a.SubsubCategoryid,
        //                               TotalTaxPercentage = a.TotalTaxPercentage,
        //                               SellingUnitName = a.SellingUnitName,
        //                               SellingSku = a.SellingSku,
        //                               UnitPrice = a.UnitPrice,
        //                               HindiName = a.HindiName,
        //                               VATTax = a.VATTax,
        //                               active = a.active,
        //                               NetPurchasePrice = a.NetPurchasePrice,
        //                               marginPoint = a.marginPoint,
        //                               promoPerItems = a.promoPerItems,
        //                               IsOffer = a.IsOffer,
        //                               Deleted = a.Deleted,
        //                               OfferCategory = a.OfferCategory,
        //                               OfferStartTime = a.OfferStartTime,
        //                               OfferEndTime = a.OfferEndTime,
        //                               OfferQtyAvaiable = a.OfferQtyAvaiable,
        //                               OfferQtyConsumed = a.OfferQtyConsumed,
        //                               OfferId = a.OfferId,
        //                               OfferType = a.OfferType,
        //                               OfferWalletPoint = a.OfferWalletPoint,
        //                               OfferFreeItemId = a.OfferFreeItemId,
        //                               OfferPercentage = a.OfferPercentage,
        //                               OfferFreeItemName = a.OfferFreeItemName,
        //                               OfferFreeItemImage = a.OfferFreeItemImage,
        //                               OfferFreeItemQuantity = a.OfferFreeItemQuantity,
        //                               OfferMinimumQty = a.OfferMinimumQty,
        //                               FlashDealSpecialPrice = a.FlashDealSpecialPrice,
        //                               FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
        //                               BillLimitQty = a.BillLimitQty
        //                           }).OrderByDescending(x => x.ItemNumber).ToList();

        //            var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
        //            var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Sales App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();

        //            foreach (var it in newdata)
        //            {
        //                if (it.OfferCategory == 2)
        //                {
        //                    it.IsOffer = false;
        //                    it.FlashDealSpecialPrice = 0;
        //                    it.OfferCategory = 0;
        //                }
        //                if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
        //                {
        //                    if (it.OfferCategory == 1)
        //                    {
        //                        it.IsOffer = false;
        //                        it.OfferCategory = 0;
        //                    }

        //                }

        //                if (it.OfferType != "FlashDeal")
        //                {
        //                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
        //                        it.IsOffer = true;
        //                    else
        //                        it.IsOffer = false;
        //                }


        //                if (item.ItemMasters == null)
        //                {
        //                    item.ItemMasters = new List<factoryItemdata>();
        //                }
        //                try
        //                {
        //                    if (!it.IsOffer)
        //                    {
        //                        /// Dream Point Logic && Margin Point
        //                        int? MP, PP;
        //                        double xPoint = xPointValue * 10;
        //                        //salesman 0.2=(0.02 * 10=0.2)
        //                        if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                        {
        //                            PP = 0;
        //                        }
        //                        else
        //                        {
        //                            PP = it.promoPerItems;
        //                        }
        //                        if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                        {
        //                            MP = 0;
        //                        }
        //                        else
        //                        {
        //                            double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                            MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                        }
        //                        if (PP > 0 && MP > 0)
        //                        {
        //                            int? PP_MP = PP + MP;
        //                            it.dreamPoint = PP_MP;
        //                        }
        //                        else if (MP > 0)
        //                        {
        //                            it.dreamPoint = MP;
        //                        }
        //                        else if (PP > 0)
        //                        {
        //                            it.dreamPoint = PP;
        //                        }
        //                        else
        //                        {
        //                            it.dreamPoint = 0;
        //                        }

        //                    }
        //                    else
        //                    {
        //                        it.dreamPoint = 0;
        //                    }

        //                    // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                    if (it.price > it.UnitPrice)
        //                    {
        //                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                    }
        //                    else
        //                    {
        //                        it.marginPoint = 0;
        //                    }
        //                }
        //                catch (Exception ds) { }
        //                //// by sudhir 22-08-2019
        //                if (lang.Trim() == "hi")
        //                {
        //                    if (it.IsSensitive == true)
        //                    {
        //                        if (it.IsSensitiveMRP == false)
        //                        {
        //                            it.itemname = it.HindiName + " " + it.UnitofQuantity + " " + it.UOM; //item display name   
        //                        }
        //                        else
        //                        {
        //                            it.itemname = it.HindiName + " " + it.price + " MRP " + it.UnitofQuantity + " " + it.UOM; //item display name                               
        //                        }
        //                    }
        //                    else
        //                    {
        //                        it.itemname = it.HindiName + " " + it.price + " MRP "; //item display name                               
        //                    }
        //                }
        //                //end
        //                item.ItemMasters.Add(it);
        //            }

        //            if (item.ItemMasters != null)
        //            {
        //                item.Message = true;
        //                return Request.CreateResponse(HttpStatusCode.OK, item);
        //            }
        //            else
        //            {
        //                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }
        //    }
        //}

        #endregion

        #region RA: Get All item of customer
        /// <summary>
        /// Get All item of customer
        /// </summary>
        /// <param name="customerId"></param>       
        /// <returns>DTO Object</returns>
        [Route("getAllitem")]
        [HttpGet]
        public HttpResponseMessage getAllitem(int customerId)
        {
            WRSITEM item = new WRSITEM();
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var custBrand = context.Customers.Where(x => x.CustomerId == customerId && x.Deleted == false).ToList();
                    foreach (var cw in custBrand)
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == cw.Warehouseid && a.Deleted == false && a.active == true)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = b.HindiName,
                                           VATTax = b.VATTax,
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
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
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
                catch (Exception)
                {
                    throw;
                }
            }
        }
        #endregion

        #region SPA: Get All item of customer for sales person
        /// <summary>
        /// Get All item of customer for sales person app
        /// </summary>
        /// <param name="spid"></param>       
        /// <returns>DTO Object</returns>
        [Route("getAllitem")]
        [HttpGet]
        public HttpResponseMessage salesgetAllitem(int spid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<WRSITEM> brandItem = new List<WRSITEM>();
                    var custBrand = context.Peoples.Where(x => x.PeopleID == spid && x.Deleted == false && x.WarehouseId != 0 && x.Active == true).ToList();

                    WRSITEM item = new WRSITEM();
                    foreach (var cw in custBrand)
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == cw.WarehouseId && a.Deleted == false && a.active == true)
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
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
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
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
                            catch { }
                            item.ItemMasters.Add(it);
                        }
                    }
                    if (item.ItemMasters != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, item);
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        #endregion

        #region  get daily Essential item
        [Route("V2")]
        [HttpGet]
        public HttpResponseMessage getdailyessentialv2(int warid)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    WRSITEM item = new WRSITEM();
                    //Increase some parameter For offer
                    var newdata = (from a in context.itemMasters
                                   where (a.WarehouseId == warid && a.Deleted == false && a.active == true && a.IsDailyEssential == true)
                                   join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                   select new factoryItemdata
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       Categoryid = b.Categoryid,
                                       Discount = b.Discount,
                                       ItemId = a.ItemId,
                                       ItemNumber = b.Number,
                                       itemname = a.itemname,
                                       LogoUrl = b.LogoUrl,
                                       MinOrderQty = b.MinOrderQty,
                                       price = a.price,
                                       SubCategoryId = b.SubCategoryId,
                                       SubsubCategoryid = b.SubsubCategoryid,
                                       TotalTaxPercentage = b.TotalTaxPercentage,
                                       SellingUnitName = a.SellingUnitName,
                                       SellingSku = b.SellingSku,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = b.HindiName,
                                       VATTax = b.VATTax,
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
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       BillLimitQty = a.BillLimitQty
                                   }).OrderByDescending(x => x.ItemNumber).ToList();
                    foreach (factoryItemdata it in newdata)
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
                        catch { }


                        item.ItemMasters.Add(it);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new WRSITEM() { ItemMasters = item.ItemMasters });
                }
                catch (Exception ex)
                {
                    logger.Error("Error in delete Item Master " + ex.Message);
                    return null;
                }
            }
        }
        #endregion

        #region DTO Model



        public class WRSITEM
        {
            public List<factoryItemdata> ItemMasters { get; set; }
            public List<factorySubSubCategory> SubsubCategories { get; set; }
            public bool Message { get; set; }
        }


        public class factoryItemdata
        {
            internal bool active;
            public int ItemId { get; set; }
            public bool IsItemLimit { get; set; }
            public int ItemlimitQty { get; set; }
            public int ItemMultiMRPId { get; set; }
           
            public string ItemNumber { get; set; }
            public int CompanyId { get; set; }
            public int WarehouseId { get; set; }
            public int Categoryid { get; set; }
            public int SubCategoryId { get; set; }
            public int SubsubCategoryid { get; set; }
            public string itemname { get; set; }
            public string HindiName { get; set; }
            public double price { get; set; }
            public string SellingUnitName { get; set; }
            public string SellingSku { get; set; }
            public double UnitPrice { get; set; }
            public double VATTax { get; set; }
            public string LogoUrl { get; set; }
            public int MinOrderQty { get; set; }
            public double Discount { get; set; }
            public double TotalTaxPercentage { get; set; }
            public double? marginPoint { get; internal set; }
            public int? promoPerItems { get; internal set; }
            public int? dreamPoint { get; internal set; }
            public bool IsOffer { get; set; }

            //by sachin (Date 14-05-2019)
            // public bool Isoffer { get; set; }
            public bool Deleted { get; internal set; }
            public double NetPurchasePrice { get; set; }
            public bool IsSensitive { get; set; }//sudhir
            public bool IsSensitiveMRP { get; set; }//sudhir
            public string UnitofQuantity { get; set; }//sudhir
            public string UOM { get; set; }//sudhir
            public string itemBaseName { get; set; }//sudhir

            public int? OfferCategory
            {
                get; set;
            }
            public DateTime? OfferStartTime
            {
                get; set;
            }
            public DateTime? OfferEndTime
            {
                get; set;
            }
            public double? OfferQtyAvaiable
            {
                get; set;
            }

            public double? OfferQtyConsumed
            {
                get; set;
            }

            public int? OfferId
            {
                get; set;
            }

            public string OfferType
            {
                get; set;
            }

            public double? OfferWalletPoint
            {
                get; set;
            }
            public int? OfferFreeItemId
            {
                get; set;
            }

            public double? OfferPercentage
            {
                get; set;
            }

            public string OfferFreeItemName
            {
                get; set;
            }

            public string OfferFreeItemImage
            {
                get; set;
            }
            public int? OfferFreeItemQuantity
            {
                get; set;
            }
            public int? OfferMinimumQty
            {
                get; set;
            }
            public double? FlashDealSpecialPrice
            {
                get; set;
            }
            public int? FlashDealMaxQtyPersonCanTake
            {
                get; set;
            }

            public int BillLimitQty { get; set; }
            public int ItemAppType { get; set; }

        }

        public class factorySubSubCategory
        {
            public int SubsubCategoryid { get; set; }
            public string SubsubcategoryName { get; set; }
            public int Categoryid { get; set; }
            public int SubCategoryId { get; set; }
            public List<factoryItemdata> ItemMasters { get; set; }
        }
        public class SSITEM
        {
            //public List<factoryItemdata> ItemMasters { get; set; }
            public List<factorySubSubCategory> SubsubCategories { get; set; }
        }
        #endregion

        #region For old app dto
        public class WRSITEM2
        {
            public List<factoryItemdata2> ItemMasters { get; set; }
            public List<factorySubSubCategory> SubsubCategories { get; set; }
            public bool Message { get; set; }
        }

        public class factoryItemdata2
        {
            internal bool active;
            public int ItemId { get; set; }
            public string ItemNumber { get; set; }
            public int CompanyId { get; set; }
            public int WarehouseId { get; set; }
            public int Categoryid { get; set; }
            public int SubCategoryId { get; set; }
            public int SubsubCategoryid { get; set; }
            public string itemname { get; set; }
            public string HindiName { get; set; }
            public double price { get; set; }
            public string SellingUnitName { get; set; }
            public string SellingSku { get; set; }
            public double UnitPrice { get; set; }
            public double VATTax { get; set; }
            public string LogoUrl { get; set; }
            public int MinOrderQty { get; set; }
            public double Discount { get; set; }
            public double TotalTaxPercentage { get; set; }
            public string marginPoint { get; internal set; }
            public int? promoPerItems { get; internal set; }
            public int? dreamPoint { get; internal set; }
            public bool IsOffer { get; set; }
            public bool Deleted { get; internal set; }
            public double NetPurchasePrice { get; set; }
        }

        #endregion

        #region flash deal 
        /// <summary>
        /// Not show offer and flashdeal on Inactive customer
        /// 28/05/2019
        /// Sudeep Solanki
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="warid"></param>
        /// <param name="CustomerId"></param>
        /// <param name="brandName"></param>
        /// <returns></returns>
        [Route("GetAllItemByBrand/V3")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage getItemOnBrandv3(string lang, int warid, int customerId, string brandName)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    WRSITEM item = new WRSITEM();

                    var customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                    var inActiveCustomer = customer.Active == false || customer.Deleted == true;
                    var data = context.AppHomeDynamicDb.Where(x => x.Wid == customer.Warehouseid && x.delete == false && x.SectionType == "FlashDeal").Include("detail").SelectMany(x => x.detail.Select(y => new { y.ItemId, y.id })).ToList();
                    List<int> ids = data.Select(x => x.id.Value).ToList();
                    var warehouseid = customer != null ? customer.Warehouseid : 0;

                    string sqlquery = "SELECT a.[FlashDealId] AS[FlashDealId], a.[ItemId] AS[ItemId] FROM[dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.[Deleted]= 0 and b.Active=1 and b.WarehouseID=" + warehouseid +
                                      " WHERE a.[CustomerId]=" + customerId;
                    var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                    if (lang.Trim() == "hi")
                    {

                        var newdatahi = (from a in context.itemMasters
                                         where (a.WarehouseId == warid && a.Deleted == false && a.active == true && a.SubsubcategoryName.Trim().ToLower() == brandName.Trim().ToLower() && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                         join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                         let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()

                                         select new factoryItemdata
                                         {
                                             WarehouseId = a.WarehouseId,
                                             IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                             ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                             CompanyId = a.CompanyId,
                                             Categoryid = b.Categoryid,
                                             Discount = b.Discount,
                                             ItemId = a.ItemId,
                                             ItemNumber = b.Number,
                                             itemname = !string.IsNullOrEmpty(a.HindiName) ? a.HindiName : a.itemname,
                                             LogoUrl = b.LogoUrl,
                                             MinOrderQty = b.MinOrderQty,
                                             price = a.price,
                                             SubCategoryId = b.SubCategoryId,
                                             SubsubCategoryid = b.SubsubCategoryid,
                                             TotalTaxPercentage = b.TotalTaxPercentage,
                                             SellingUnitName = a.SellingUnitName,
                                             SellingSku = b.SellingSku,
                                             UnitPrice = a.UnitPrice,
                                             HindiName = a.HindiName,
                                             VATTax = b.VATTax,
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
                                             BillLimitQty = a.BillLimitQty,
                                             ItemAppType=a.ItemAppType,
                                         }).OrderByDescending(x => x.ItemNumber).ToList();

                        var offerids = newdatahi.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();

                        foreach (factoryItemdata it in newdatahi)
                        {
                            if (!inActiveCustomer)
                            {
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
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
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
                            }
                            else
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

                            if (it.HindiName != null)
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

                            item.ItemMasters.Add(it);
                        }

                    }
                    else
                    {
                        //Increase some parameter For offer
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == warid && a.Deleted == false && a.active == true && a.SubsubcategoryName.Trim().ToLower() == brandName.Trim().ToLower() && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                       join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
                                       let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.Number == p2.ItemNumber && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                       select new factoryItemdata
                                       {
                                           WarehouseId = a.WarehouseId,
                                           CompanyId = a.CompanyId,
                                           Categoryid = b.Categoryid,
                                           Discount = b.Discount,
                                           ItemId = a.ItemId,
                                           ItemNumber = b.Number,
                                           itemname = a.itemname,
                                           LogoUrl = b.LogoUrl,
                                           MinOrderQty = b.MinOrderQty,
                                           price = a.price,
                                           SubCategoryId = b.SubCategoryId,
                                           SubsubCategoryid = b.SubsubCategoryid,
                                           TotalTaxPercentage = b.TotalTaxPercentage,
                                           SellingUnitName = a.SellingUnitName,
                                           SellingSku = b.SellingSku,
                                           UnitPrice = a.UnitPrice,
                                           HindiName = a.HindiName,
                                           VATTax = b.VATTax,
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
                                           ItemAppType = a.ItemAppType,
                                           IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                           ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0
                                       }).OrderByDescending(x => x.ItemNumber).ToList();

                        var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();


                        foreach (factoryItemdata it in newdata)
                        {
                            if (!inActiveCustomer)
                            {
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
                                    if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                                    {
                                        it.IsOffer = false;
                                        it.FlashDealSpecialPrice = 0;
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
                            }
                            else
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
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                }
            }
        }
        #endregion




        [HttpGet]
        [Route("GetCategoryBySubCategory")]
        public List<OfferDetail> GetCategoryBySubCategory(int? subcategoryId)
        {
            List<OfferDetail> OfferDetail = new List<OfferDetail>();
            using (var context = new AuthContext())
            {
                if (subcategoryId.HasValue)
                {
                    var subCategoryQuery = "select distinct a.Categoryid Id,a.CategoryName Name, Cast((case when b.SubCategoryId is null then 0 else 1 end) as bit) Selected from Categories a  left join  SubcategoryCategoryMappings b on  a.Categoryid=b.Categoryid and b.SubCategoryId=" + subcategoryId + " and b.IsActive=1 and b.Deleted=0 where a.IsActive=1 and a.Deleted=0";
                    OfferDetail = context.Database.SqlQuery<OfferDetail>(subCategoryQuery).ToList().OrderBy(x => x.Name).ToList();
                }
                else
                {
                    var subCategoryQuery = "select distinct a.Categoryid Id,a.CategoryName Name, Cast((case when b.SubCategoryId is null then 0 else 1 end) as bit) Selected from Categories a  left join  SubcategoryCategoryMappings b on  a.Categoryid=b.Categoryid and b.IsActive=1 and b.Deleted=0 where a.IsActive=1 and a.Deleted=0";
                    OfferDetail = context.Database.SqlQuery<OfferDetail>(subCategoryQuery).ToList().OrderBy(x => x.Name).ToList();
                }

            }

            return OfferDetail;
        }

        [HttpGet]
        [Route("GetAllBrands")]
        public List<BrandsDTO> GetAllBrands(int? CustomerId)
        {
            List<BrandsDTO> BrandsList = new List<BrandsDTO>();
            using (var context = new AuthContext())
            {
                if (CustomerId.HasValue)
                {
                    var subSubCategoryQuery = "select distinct ssc.SubsubCategoryid,ssc.SubsubcategoryName,Cast((case when CBA.CustomerId is null then 0 else 1 end) as bit) Selected "+
                                           "   from SubsubCategories ssc "+
                                           "   left join CustomerBrandAcesses CBA on CBA.BrandId = ssc.SubsubCategoryid and CBA.IsActive = 1  and CBA.CustomerId = "+ CustomerId+"  "+
                                           "   where  ssc.IsActive = 1 and ssc.Deleted = 0 ";
                         BrandsList = context.Database.SqlQuery<BrandsDTO>(subSubCategoryQuery).ToList().OrderBy(x => x.SubsubcategoryName).ToList();
                }
                else
                {
                    var subSubCategoryQuery = "select distinct ssc.SubsubCategoryid,ssc.SubsubcategoryName,Cast((case when CBA.CustomerId is null then 0 else 1 end) as bit) Selected " +
                                           "   from SubsubCategories ssc " +
                                           "   left join CustomerBrandAcesses CBA on CBA.BrandId = ssc.SubsubCategoryid and CBA.IsActive = 1   " +
                                           "   where  ssc.IsActive = 1 and ssc.Deleted = 0 ";
                    BrandsList = context.Database.SqlQuery<BrandsDTO>(subSubCategoryQuery).ToList().OrderBy(x => x.SubsubcategoryName).ToList();
                }

            }

            return BrandsList;
        }

        [HttpGet]
        [Route("GetCategoryByBrand")]
        public List<OfferDetail> GetCategoryByBrand(int subsubcategoryId, int subCategoryId)
        {
            List<OfferDetail> OfferDetail = new List<OfferDetail>();
            using (var context = new AuthContext())
            {
                // var subCategoryQuery = "select distinct a.Categoryid Id,a.CategoryName Name, cast((case when c.BrandCategoryMappingId is null then 0 else 1 end) as bit) Selected,cast((case when c.BrandCategoryMappingId is null then 0 else c.AgentCommisionPercent end) as float) AgentCommisionPercent  from Categories a  Inner join  SubcategoryCategoryMappings b on  a.Categoryid=b.Categoryid and b.IsActive=1 and b.Deleted=0 and b.subcategoryid=" + subCategoryId + " left join BrandCategoryMappings c on b.SubCategoryMappingId = c.SubCategoryMappingId and c.IsActive=1 and c.Deleted=0 and c.SubsubCategoryId=" + subsubcategoryId +" where a.IsActive=1 and a.Deleted=0 ";
                var subCategoryQuery = " exec GetBrandCategoryWithAgentCommission " + subsubcategoryId;
                OfferDetail = context.Database.SqlQuery<OfferDetail>(subCategoryQuery).ToList().OrderBy(x => x.Name).ToList();
            }

            return OfferDetail;
        }


    }

    public class BrandsDTO
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public bool Selected { get; set; }
    }
}
