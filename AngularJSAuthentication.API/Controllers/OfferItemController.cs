using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Common.Constants;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.ControllerV1
{
    [RoutePrefix("api/offeritem")]
    public class OfferItemController : BaseAuthController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public double xPointValue = AppConstants.xPoint;



        #region RA V2:  for customer app get item by cat id # Version 2
        /// <summary>
        ///  # Version 2
        ///  for customer app get item by cat id 
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="customerId"></param>
        /// <param name="catid"></param>
        /// <returns></returns>
        [Route("itemsgetbyoffer")]
        [HttpGet]
        public HttpResponseMessage itemsgetbyoffer(int Warehouseid, int sectionid, int CustomerId)
        {
            using (var context = new AuthContext())
                try
                {

                    //List<string> itemids = new List<string>(new string[] { "AE1011A",  "AE1013A" });
                    DateTime CurrentDate = indianTime;
                    var data = context.AppHomeDynamicDb.Where(x => x.Wid == Warehouseid && x.delete == false && x.id == sectionid).Include("detail").SelectMany(x => x.detail.Select(y => new { y.ItemId, y.id })).ToList();
                    if (data != null)
                    {
                        List<int> ids = data.Select(x => x.id.Value).ToList();
                        //var FlashDealWithItemIds = (from c in context.FlashDealItemConsumedDB.Where(x => x.CustomerId == CustomerId)
                        //                            join p in context.AppHomeDynamicDb.Where(x => x.Wid == Warehouseid && x.active && !x.delete).SelectMany(x => x.detail.Select(y => new { id = y.id.Value, ItemId = y.ItemId }))
                        //                            on c.FlashDealId equals p.id into ps
                        //                            select new
                        //                            { c.FlashDealId, c.ItemId }).ToList();
                        //var FlashDealWithItemIds = context.FlashDealItemConsumedDB.Where(x => x.CustomerId == CustomerId && ids.Contains(x.FlashDealId)).Select(x => new { x.FlashDealId, x.ItemId });
                        string sqlquery = "SELECT a.[FlashDealId] AS[FlashDealId], a.[ItemId] AS[ItemId] FROM[dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.[Deleted]= 0 and b.Active=1 and b.WarehouseID=" + Warehouseid +
                                     " WHERE a.[CustomerId]=" + CustomerId;
                        var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();
                        List<WRSITEM> brandItem = new List<WRSITEM>();
                        WRSITEM item = new WRSITEM();
                        var itemids = data.Select(x => x.ItemId);
                        // var FlashDealIitemids = FlashDealIds != null && FlashDealIds.Any() ? data.Where(s => !FlashDealIds.Contains(s.id.Value)).Select(x => x.ItemId) : new List<int>();
                        //foreach (var itemid in data)
                        //{
                        //Increase some Parameter for Flash Deal 
                        //context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId) && a.OfferType == "FlashDeal" && a.OfferStartTime <= CurrentDate
                                             && a.OfferEndTime >= CurrentDate && a.OfferQtyAvaiable > 0)
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
                            it.IsFlashDealUsed = FlashDealWithItemIds != null ? FlashDealWithItemIds.Select(x => x.ItemId).Contains(it.ItemId) : false;
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
                                    if (it.price > it.FlashDealSpecialPrice)
                                    {
                                        it.marginPoint = ((it.price - it.FlashDealSpecialPrice) * 100) / it.FlashDealSpecialPrice;//MP;  we replce marginpoint value by margin for app here 
                                        double IsInfinity = (double)it.marginPoint;
                                        if (Double.IsInfinity(IsInfinity))
                                        {
                                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                        }
                                    }
                                    else
                                    {
                                        it.marginPoint = 0;
                                    }
                                }
                                else
                                {
                                    it.dreamPoint = 0;
                                    if (it.price > it.FlashDealSpecialPrice)
                                    {
                                        it.marginPoint = it.FlashDealSpecialPrice > 0 ? (((it.price - it.FlashDealSpecialPrice) * 100) / it.FlashDealSpecialPrice) : (((it.price - it.UnitPrice) * 100) / it.UnitPrice);//MP;  we replce marginpoint value by margin for app here 
                                    }
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
                        //}
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
                            Message = "fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                catch (Exception ee)
                {
                    throw;
                }
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Warehouseid"></param>
        /// <param name="sectionid"></param>
        /// <returns></returns>
        [Route("GetItemBySection")]
        [HttpGet]
        public HttpResponseMessage GetItemBySection(int Warehouseid, int sectionid)
        {
            using (var context = new AuthContext())
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
                    //List<string> itemids = new List<string>(new string[] { "AE1011A",  "AE1013A" });
                    DateTime CurrentDate = DateTime.Now;
                    var data = context.AppHomeDynamicDb.Where(x => x.Wid == Warehouseid && x.delete == false && x.id == sectionid).Include("detail").SelectMany(x => x.detail.Select(y => new { y.ItemId, y.id })).ToList();
                    if (data != null)
                    {
                        var FlashDealWithItemIds = context.FlashDealItemConsumedDB.Where(x => x.CustomerId == userid && data.Select(y => y.id).Contains(x.FlashDealId)).Select(x => new { x.FlashDealId, x.ItemId });
                        List<WRSITEM> brandItem = new List<WRSITEM>();
                        WRSITEM item = new WRSITEM();
                        foreach (var itemid in data)
                        {
                            var newdata = (from a in context.itemMasters
                                           where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && a.ItemId == itemid.ItemId)
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
                        // }
                        //}
                        //else
                        //{
                        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                        //}
                        //}
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
                            Message = "fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                catch (Exception ee)
                {
                    throw;
                }
        }


        /// <summary>
        /// Not show offer and flashdeal on Inactive customer
        /// 28/05/2019
        /// Sudeep Solanki
        /// </summary>
        /// <param name="Warehouseid"></param>
        /// <param name="sectionid"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("GetItemBySectionV1")]
        [HttpGet]
        public HttpResponseMessage GetItemBySection(int Warehouseid, int sectionid, int customerId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    //List<string> itemids = new List<string>(new string[] { "AE1011A",  "AE1013A" });
                    DateTime CurrentDate = DateTime.Now;
                    var data = context.AppHomeDynamicDb.Where(x => x.Wid == Warehouseid && x.delete == false && x.id == sectionid).Include("detail").SelectMany(x => x.detail.Select(y => new { y.ItemId, y.id })).ToList();
                    if (data != null)
                    {
                        List<int> ids = data.Select(x => x.id.Value).ToList();
                        var FlashDealWithItemIds = context.FlashDealItemConsumedDB.Where(x => x.CustomerId == customerId && ids.Contains(x.FlashDealId)).Select(x => new { x.FlashDealId, x.ItemId });

                        var inActiveCustomer = context.Customers.Any(x => x.CustomerId == customerId && (x.Active == false || x.Deleted == true));
                        List<WRSITEM> brandItem = new List<WRSITEM>();
                        WRSITEM item = new WRSITEM();
                        foreach (var itemid in data)
                        {
                            var newdata = (from a in context.itemMasters
                                           where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && a.ItemId == itemid.ItemId)
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
                                //Condition for offer end.
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
                                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                        if (it.price > it.FlashDealSpecialPrice)
                                        {
                                            it.marginPoint = ((it.price - it.FlashDealSpecialPrice) * 100) / it.FlashDealSpecialPrice;//MP;  we replce marginpoint value by margin for app here 
                                            double IsInfinity = (double)it.marginPoint;
                                            if (Double.IsInfinity(IsInfinity))
                                            {
                                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;// In infinity case
                                            }
                                        }
                                        else
                                        {
                                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;
                                        }
                                    }
                                    else
                                    {

                                        it.dreamPoint = 0;
                                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                    }
                                }
                                catch { }
                                item.ItemMasters.Add(it);
                            }
                        }
                        // }
                        //}
                        //else
                        //{
                        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                        //}
                        //}
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
                            Message = "fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                catch (Exception ee)
                {
                    throw;
                }
            }
        }

        #region GetItemBySection for new app home 
        /// <summary>
        /// Not show offer and flashdeal on Inactive customer
        /// 16/07/2019
        /// Created by Raj
        /// </summary>
        /// <param name="Warehouseid"></param>
        /// <param name="sectionid"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        [Route("GetItemBySectionV2")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetItemBySectionV2(int Warehouseid, int sectionid, int customerId)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    //List<string> itemids = new List<string>(new string[] { "AE1011A",  "AE1013A" });
                    DateTime CurrentDate = DateTime.Now;
                    var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                    if (data != null)
                    {
                        List<int> ids = data.Select(x => x.SectionItemID).ToList();
                        var FlashDealWithItemIds = context.FlashDealItemConsumedDB.Where(x => x.CustomerId == customerId && ids.Contains(x.FlashDealId)).Select(x => new { x.FlashDealId, x.ItemId });

                        var inActiveCustomer = context.Customers.Any(x => x.CustomerId == customerId && (x.Active == false || x.Deleted == true));
                        List<WRSITEM> brandItem = new List<WRSITEM>();
                        WRSITEM item = new WRSITEM();
                        //foreach (var itemid in data)

                        var itemids = data.Select(x => x.ItemId).ToList();
                        if (itemids != null && itemids.Any())
                        {
                            var newdata = (from a in context.itemMasters
                                           where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId))
                                           //join b in context.ItemMasterCentralDB on a.SellingSku equals b.SellingSku
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
                                               ItemMultiMRPId = a.ItemMultiMRPId,
                                               BillLimitQty = a.BillLimitQty
                                           }).ToList();

                            newdata = newdata.OrderByDescending(x => x.ItemNumber).ToList();


                            var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                            var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Retailer App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();


                            foreach (var it in newdata)
                            {
                                //Condition for offer end.
                                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;

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
                                        // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
                                        if (it.price > it.FlashDealSpecialPrice)
                                        {
                                            it.marginPoint = ((it.price - it.FlashDealSpecialPrice) * 100) / it.FlashDealSpecialPrice;//MP;  we replce marginpoint value by margin for app here 
                                            double IsInfinity = (double)it.marginPoint;
                                            if (Double.IsInfinity(IsInfinity))
                                            {
                                                it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                            }
                                        }
                                        else
                                        {
                                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;
                                        }
                                    }
                                    else
                                    {

                                        it.dreamPoint = 0;
                                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                    }
                                }
                                catch { }
                                item.ItemMasters.Add(it);
                            }
                        }
                        // }
                        //}
                        //else
                        //{
                        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "false");
                        //}
                        //}
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
                            Message = "fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                catch (Exception ee)
                {
                    throw;
                }
            }
        }



        ///// <summary>
        ///// Section base item on agent app
        ///// 16/07/2019
        ///// Created by Raj
        ///// </summary>
        ///// <param name="Warehouseid"></param>
        ///// <param name="sectionid"></param>       
        ///// <returns></returns>
        //[Route("GetItemBySectionForAgent")]
        //[HttpGet]
        //[AllowAnonymous]
        //public HttpResponseMessage GetItemBySectionForAgent(int peopleid, int warehouseid, int sectionid)
        //{
        //    using (var context = new AuthContext())
        //    {
        //        try
        //        {
        //            DateTime CurrentDate = DateTime.Now;
        //            var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();
        //            if (data != null)
        //            {
        //                List<int> Itemids = data.Select(x => x.ItemId).ToList();

        //                WRSITEM item = new WRSITEM();

        //                var newdata = (from a in context.itemMasters
        //                               where (a.WarehouseId == warehouseid && a.Deleted == false && a.active == true && Itemids.Contains(a.ItemId) && (a.ItemAppType == 0 || a.ItemAppType == 1))
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
        //                                   SellingUnitName = b.SellingUnitName,
        //                                   SellingSku = b.SellingSku,
        //                                   UnitPrice = a.UnitPrice,
        //                                   HindiName = b.HindiName,
        //                                   VATTax = b.VATTax,
        //                                   active = a.active,
        //                                   marginPoint = a.marginPoint,
        //                                   promoPerItems = a.promoPerItems,
        //                                   NetPurchasePrice = a.NetPurchasePrice,
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
        //                                   BillLimitQty = a.BillLimitQty,
        //                                   ItemAppType=a.ItemAppType,
        //                               }).ToList();

        //                newdata = newdata.OrderByDescending(x => x.ItemNumber).ToList();

        //                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
        //                var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Sales App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();

        //                foreach (var it in newdata)
        //                {
        //                    if (!it.OfferId.HasValue || it.OfferId.Value == 0)
        //                    {
        //                        it.IsOffer = false;
        //                    }
        //                    if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
        //                        it.IsOffer = true;
        //                    else
        //                        it.IsOffer = false;

        //                    if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
        //                    {
        //                        if (it.OfferCategory == 1)
        //                        {
        //                            it.IsOffer = false;
        //                            it.OfferCategory = 0;
        //                        }
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
        //                                MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax)  By xpoint 
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

        //                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;

        //                        }
        //                        else
        //                        {

        //                            it.dreamPoint = 0;
        //                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                        }
        //                    }
        //                    catch { }
        //                    item.ItemMasters.Add(it);
        //                }


        //                if (item.ItemMasters != null)
        //                {
        //                    var res = new
        //                    {
        //                        ItemMasters = item.ItemMasters,
        //                        Status = true,
        //                        Message = "Success."
        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, res);
        //                }
        //                else
        //                {
        //                    Array[] l = new Array[] { };
        //                    var res = new
        //                    {
        //                        ItemMasters = l,
        //                        Status = false,
        //                        Message = "fail"
        //                    };
        //                    return Request.CreateResponse(HttpStatusCode.OK, res);
        //                }
        //            }
        //            else
        //            {
        //                Array[] l = new Array[] { };
        //                var res = new
        //                {
        //                    ItemMasters = l,
        //                    Status = false,
        //                    Message = "fail"
        //                };
        //                return Request.CreateResponse(HttpStatusCode.OK, res);
        //            }
        //        }

        //        catch (Exception ee)
        //        {
        //            throw;
        //        }
        //    }
        //}

        #endregion

        #region RA V3:  for customer app get item by cat id # Version 3
        /// <summary>
        ///  # Version 3
        ///  for customer app get item by cat id 
        /// </summary>
        /// Created Date:16/072019
        /// Created by Raj
        /// <param name="lang"></param>
        /// <param name="customerId"></param>
        /// <param name="catid"></param>
        /// <returns></returns>
        [Route("itemsgetbyofferV1")]
        [HttpGet]
        public HttpResponseMessage itemsgetbyofferV1(int Warehouseid, int sectionid, int CustomerId)
        {
            using (var context = new AuthContext())
            {
                try
                {

                    //List<string> itemids = new List<string>(new string[] { "AE1011A",  "AE1013A" });
                    DateTime CurrentDate = DateTime.Now;
                    var data = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Warehouseid && x.Deleted == false && x.SectionID == sectionid).Include("detail").SelectMany(x => x.AppItemsList.Select(y => new { y.ItemId, y.SectionItemID })).ToList();
                    if (data != null)
                    {
                        List<int> ids = data.Select(x => x.SectionItemID).ToList();
                        string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + Warehouseid +
                                        " WHERE a.[CustomerId]=" + CustomerId;
                        var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                        List<WRSITEM> brandItem = new List<WRSITEM>();
                        WRSITEM item = new WRSITEM();
                        var itemids = data.Select(x => x.ItemId);
                        var newdata = (from a in context.itemMasters
                                       where (a.WarehouseId == Warehouseid && a.Deleted == false && a.active == true && itemids.Contains(a.ItemId) && a.OfferType == "FlashDeal" && a.OfferStartTime <= CurrentDate
                                             && a.OfferEndTime >= CurrentDate && a.OfferQtyAvaiable > 0)
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
                                           FlashDealMaxQtyPersonCanTake = a.OfferMaxQtyPersonCanTake,
                                           ItemMultiMRPId = a.ItemMultiMRPId,
                                           BillLimitQty = a.BillLimitQty
                                       }).OrderByDescending(x => x.ItemNumber).ToList();
                        foreach (var it in newdata)
                        {
                            it.IsFlashDealUsed = FlashDealWithItemIds != null ? FlashDealWithItemIds.Select(x => x.ItemId).Contains(it.ItemId) : false;
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
                                    if (it.price > it.FlashDealSpecialPrice)
                                    {
                                        it.marginPoint = ((it.price - it.FlashDealSpecialPrice) * 100) / it.FlashDealSpecialPrice;//MP;  we replce marginpoint value by margin for app here 
                                        double IsInfinity = (double)it.marginPoint;
                                        if (Double.IsInfinity(IsInfinity))
                                        {
                                            it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
                                        }
                                    }
                                    else
                                    {
                                        it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;
                                    }
                                }
                                else
                                {

                                    it.dreamPoint = 0;
                                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
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
                        //}
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
                            Message = "fail"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }

                catch (Exception ee)
                {
                    throw;
                }
            }
            #endregion






        }
    }
}
