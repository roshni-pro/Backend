using AngularJSAuthentication.API.Helper.Seller;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Masters.Seller;
using AngularJSAuthentication.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Hangfire;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;


namespace AngularJSAuthentication.API.Controllers.Seller
{
    [RoutePrefix("api/SellerAppHome")]
    public class SellerAppHomeController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        [Route("GetSellerAppHome")]
        [HttpGet]
        public async Task<List<AppHomeSectionsDc>> GetSellerAppHome(string appType, int wId)
        {
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
            List<AppHomeSectionsDc> sectionsResult = new List<AppHomeSectionsDc>();
            if (!string.IsNullOrEmpty(appType) && wId > 0 && SubCatId > 0)
            {
                using (var context = new AuthContext())
                {
                    var datenow = indianTime;
                    List<AppHomeSections> sections = await context.AppHomeSectionsDB.Where(x => x.WarehouseID == wId && x.AppType == appType && x.SubCategoryID == SubCatId && x.Deleted == false).Include("AppItemsList").ToListAsync();
                    foreach (var a in sections)
                    {
                        if (a.SectionType == "Banner")
                            foreach (var ap in a.AppItemsList)
                            {
                                if (ap.OfferEndTime < datenow && ap.Deleted == false)
                                {
                                    ap.Expired = true;
                                    context.Entry(ap).State = EntityState.Modified;
                                    context.Commit();
                                }
                            }
                    }
                    sectionsResult = context.AppHomeSectionsDB.Where(x => x.WarehouseID == wId && x.SubCategoryID == SubCatId && x.AppType == appType && x.Deleted == false).Include(o => o.AppItemsList)
                       .ToList().Select(o => new AppHomeSectionsDc
                       {
                           AppItemsList = o.SectionType == "Banner" || o.SectionType == "PopUp"
                                       ? o.AppItemsList.Where(i => i.Deleted == false && (i.OfferEndTime > datenow || i.OfferEndTime == null)
                                         ).Select(z => new AppHomeSectionItemsDc
                                         {
                                             Active = z.Active,
                                             BannerImage = z.BannerImage,
                                             BannerName = z.BannerName,
                                             BannerActivity = z.BannerActivity,
                                             CreatedDate = z.CreatedDate,
                                             Deleted = z.Deleted,
                                             Expired = z.Expired,
                                             HasOffer = z.HasOffer,
                                             ImageLevel = z.ImageLevel,
                                             OfferEndTime = z.OfferEndTime,
                                             OfferStartTime = z.OfferStartTime,
                                             RedirectionID = z.RedirectionID,
                                             RedirectionType = z.RedirectionType,
                                             RedirectionUrl = z.RedirectionUrl,
                                             SectionItemID = z.SectionItemID,
                                             TileImage = z.TileImage,
                                             TileName = z.TileName,
                                             FlashDealQtyAvaiable = z.FlashDealQtyAvaiable,
                                             FlashDealSpecialPrice = z.FlashDealSpecialPrice,
                                             FlashDealMaxQtyPersonCanTake = z.FlashDealMaxQtyPersonCanTake,
                                             IsFlashDeal = z.IsFlashDeal,
                                             BaseCategoryId = z.BaseCategoryId,
                                             CategoryId = z.CategoryId,
                                             SubCategoryId = z.SubCategoryId,
                                             SubsubCategoryId = z.SubsubCategoryId,
                                             ItemId = z.ItemId,
                                             MOQ = z.MOQ,
                                             UnitPrice = z.UnitPrice,
                                             PurchasePrice = z.PurchasePrice,
                                             UpdatedDate = z.UpdatedDate
                                         }).ToList()
                                       : o.AppItemsList.Select(z => new AppHomeSectionItemsDc
                                       {
                                           Active = z.Active,
                                           BannerImage = z.BannerImage,
                                           BannerName = z.BannerName,
                                           BannerActivity = z.BannerActivity,
                                           CreatedDate = z.CreatedDate,
                                           Deleted = z.Deleted,
                                           Expired = z.Expired,
                                           HasOffer = z.HasOffer,
                                           ImageLevel = z.ImageLevel,
                                           OfferEndTime = z.OfferEndTime,
                                           OfferStartTime = z.OfferStartTime,
                                           RedirectionID = z.RedirectionID,
                                           RedirectionType = z.RedirectionType,
                                           RedirectionUrl = z.RedirectionUrl,
                                           SectionItemID = z.SectionItemID,
                                           TileImage = z.TileImage,
                                           TileSectionBackgroundImage = z.TileSectionBackgroundImage,
                                           TileName = z.TileName,
                                           FlashDealQtyAvaiable = z.FlashDealQtyAvaiable,
                                           FlashDealSpecialPrice = z.FlashDealSpecialPrice,
                                           FlashDealMaxQtyPersonCanTake = z.FlashDealMaxQtyPersonCanTake,
                                           IsFlashDeal = z.IsFlashDeal,
                                           BaseCategoryId = z.BaseCategoryId,
                                           CategoryId = z.CategoryId,
                                           SubCategoryId = z.SubCategoryId,
                                           SubsubCategoryId = z.SubsubCategoryId,
                                           ItemId = z.ItemId,
                                           MOQ = z.MOQ,
                                           UnitPrice = z.UnitPrice,
                                           PurchasePrice = z.PurchasePrice,
                                           UpdatedDate = z.UpdatedDate
                                       }).ToList(),
                           SectionID = o.SectionID,
                           AppType = o.AppType,
                           WarehouseID = o.WarehouseID,
                           SectionName = o.SectionName,
                           SectionHindiName=o.SectionHindiName,
                           SectionType = o.SectionType,
                           SectionSubType = o.SectionSubType,
                           IsTileSlider = o.IsTileSlider,
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
                           TileAreaHeaderBackgroundImage = o.TileAreaHeaderBackgroundImage,
                           sectionBackgroundImage = o.sectionBackgroundImage,
                           HeaderTextColor = o.HeaderTextColor,
                           Deleted = o.Deleted,
                           ViewType = o.ViewType,
                           WebViewUrl = o.WebViewUrl
                       }).ToList();
                    if (sectionsResult != null && sectionsResult.Any())
                    {
                        var itemIds = sectionsResult.SelectMany(x => x.AppItemsList.Where(z => z.ItemId > 0).Select(z => z.ItemId)).Distinct().ToList();
                        var itemsList = context.itemMasters.Where(x => itemIds.Contains(x.ItemId)).Select(x => new { x.ItemId, x.OfferQtyAvaiable }).ToList();

                        sectionsResult.ForEach(x => x.AppItemsList.Where(z => z.ItemId > 0 && z.Deleted == false).ToList().ForEach(y =>
                        {
                            if (y.ItemId > 0)
                            {
                                var item = itemsList.FirstOrDefault(a => a.ItemId == y.ItemId);
                                y.FlashdealRemainingQty = item.OfferQtyAvaiable;
                            }
                            if (!string.IsNullOrEmpty(y.TileImage) && !y.TileImage.Contains("http"))
                            {
                                y.TileImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , y.TileImage);
                            }
                        }));
                    }
                    logger.Info("End GetSections: ");
                    return sectionsResult.OrderBy(x => x.Sequence).ToList();
                }
            }
            else
            {
                return sectionsResult;
            }
        }

        [Route("GetWarehouseItemForAppHome")]
        [HttpGet]
        public HttpResponseMessage GetWarehouseItemWithInactive(int WarehouseId)
        {
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            if (WarehouseId > 0 && SubCatId > 0)
            {
                using (var db = new AuthContext())
                {
                    var Item = db.itemMasters.Where(x => x.WarehouseId == WarehouseId && x.SubCategoryId == SubCatId && x.IsDisContinued == false && x.Deleted == false).Select(x => new
                    {
                        x.ItemId,
                        x.itemname,
                        x.ItemMultiMRPId,
                        x.Number,
                        x.LogoUrl,
                        x.MinOrderQty,
                        x.MRP,
                        x.UnitPrice,
                        x.SellingSku,
                        x.active,
                        x.PurchasePrice,
                        x.price
                    }).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, Item);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Null");
            }

        }

        [Route("AddSection")]
        [HttpPost]
        public AppHomeSectionsres AddSections(AppHomeSections appsection)
        {
            AppHomeSectionsres result = new AppHomeSectionsres();
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                if (appsection == null)
                {
                    throw new ArgumentNullException("state");
                }
                var ar = context.AppHomeSectionsDB.Where(x => x.SectionID == appsection.SectionID && x.SubCategoryID == SubCatId && x.Deleted == false).Include(x => x.AppItemsList).FirstOrDefault();
                if (appsection.AppItemsList != null && appsection.AppItemsList.Any())
                {
                    ar.AppItemsList = ar.AppItemsList.Where(x => x.Deleted == false).ToList();
                    var existingAppItems = ar.AppItemsList;

                    if (existingAppItems.Count > appsection.AppItemsList.Count)
                    {
                        for (var i = 0; i < existingAppItems.Count; i++)
                        {
                            if (i + 1 > appsection.AppItemsList.Count)
                            {
                                existingAppItems[i].Deleted = true;
                                existingAppItems[i].Active = false;
                                existingAppItems[i].UpdatedDate = indianTime;
                                context.Entry(existingAppItems[i]).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                    }
                }
                if (appsection.ViewType == "AppView" && appsection.SectionSubType != "Other" && appsection.SectionSubType != "DynamicHtml")
                {
                    appsection.WebViewUrl = string.Empty;
                }

                if (ar == null)
                {
                    int sequence = context.AppHomeSectionsDB.Where(x => x.WarehouseID == appsection.WarehouseID && x.SubCategoryID == SubCatId && x.Deleted == false && x.AppType == appsection.AppType).Count();
                    appsection.CreatedDate = indianTime;
                    appsection.UpdatedDate = indianTime;
                    appsection.Active = true;
                    appsection.Sequence = sequence + 1;
                    appsection.SubCategoryID = SubCatId;
                    context.AppHomeSectionsDB.Add(appsection);
                    int id = context.Commit();

                    if (appsection.AppItemsList != null && appsection.AppItemsList.Any())
                    {
                        foreach (var o in appsection.AppItemsList)
                        {
                            var appItem = context.AppHomeSectionItemsDB.Where(x => x.SectionItemID == o.SectionItemID && x.Deleted == false).FirstOrDefault();
                            if (appItem == null)
                            {
                                if (o.HasOffer)
                                {
                                    o.OfferStartTime = o.OfferStartTime;
                                    o.OfferEndTime = o.OfferEndTime;
                                }
                                else
                                {
                                    o.OfferStartTime = null;
                                    o.OfferEndTime = null;
                                }
                                o.Active = true;
                                o.apphomesections = ar;
                                o.CreatedDate = indianTime;
                                o.UpdatedDate = indianTime;
                                context.AppHomeSectionItemsDB.Add(o);
                                context.Commit();
                            }
                            else
                            {
                                appItem.RedirectionType = o.RedirectionType;
                                appItem.RedirectionID = o.RedirectionID;
                                appItem.Active = o.Active;
                                appItem.HasOffer = o.HasOffer;
                                if (appItem.HasOffer)
                                {
                                    appItem.OfferStartTime = o.OfferStartTime;
                                    appItem.OfferEndTime = o.OfferEndTime;
                                }
                                else
                                {
                                    appItem.OfferStartTime = null;
                                    appItem.OfferEndTime = null;
                                }
                                appItem.ImageLevel = o.ImageLevel;
                                appItem.TileName = o.TileName;
                                appItem.TileImage = o.TileImage;
                                appItem.BannerName = o.BannerName;
                                appItem.BannerActivity = o.BannerActivity;
                                appItem.RedirectionUrl = o.RedirectionUrl;
                                appItem.BannerImage = o.BannerImage;

                                appItem.UpdatedDate = indianTime;
                                if (o.OfferEndTime.HasValue && o.OfferEndTime.Value <= indianTime)
                                {
                                    appItem.Active = false;
                                    appItem.Deleted = true;
                                }
                                context.Entry(appItem).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                    }
                    appsection = context.AppHomeSectionsDB.Where(x => x.SectionID == appsection.SectionID && x.SubCategoryID == SubCatId && x.Deleted == false).Include("AppItemsList").FirstOrDefault();

                    result.AppHomeSections = appsection;


                    return result;
                }
                else
                {
                    ar.SectionName = appsection.SectionName;
                    ar.SectionHindiName = appsection.SectionHindiName;
                    ar.SectionType = appsection.SectionType;
                    ar.SectionSubType = appsection.SectionSubType;
                    ar.ColumnCount = appsection.ColumnCount;
                    ar.ViewType = appsection.ViewType;
                    ar.WebViewUrl = appsection.WebViewUrl;
                    ar.RowCount = appsection.RowCount;
                    ar.Active = true;
                    ar.HasBackgroundColor = appsection.HasBackgroundColor;
                    ar.HasBackgroundImage = appsection.HasBackgroundImage;
                    ar.TileAreaHeaderBackgroundImage = appsection.TileAreaHeaderBackgroundImage;
                    ar.sectionBackgroundImage = appsection.sectionBackgroundImage;
                    ar.HeaderTextColor = appsection.HeaderTextColor;
                    ar.IsTileSlider = appsection.IsTileSlider;
                    if (ar.HasBackgroundImage == true)
                        ar.TileBackgroundImage = appsection.TileBackgroundImage;
                    else
                        ar.TileBackgroundImage = null;
                    if (ar.HasBackgroundColor == true || appsection.TileBackgroundColor != null)
                        ar.TileBackgroundColor = appsection.TileBackgroundColor;
                    else
                        ar.TileBackgroundColor = null;
                    if (ar.HasBackgroundColor == true)
                        ar.BannerBackgroundColor = appsection.BannerBackgroundColor;
                    else
                        ar.BannerBackgroundColor = null;
                    ar.HasHeaderBackgroundColor = appsection.HasHeaderBackgroundColor;
                    ar.HasHeaderBackgroundImage = appsection.HasHeaderBackgroundImage;
                    if (ar.HasHeaderBackgroundImage == true)
                        ar.TileHeaderBackgroundImage = appsection.TileHeaderBackgroundImage;
                    else
                        ar.TileHeaderBackgroundImage = null;
                    if (ar.HasHeaderBackgroundColor == true)
                        ar.TileHeaderBackgroundColor = appsection.TileHeaderBackgroundColor;
                    else
                        ar.TileHeaderBackgroundColor = null;

                    ar.UpdatedDate = indianTime;
                    context.Entry(ar).State = EntityState.Modified;
                    int id = context.Commit();

                    if (appsection.AppItemsList != null && appsection.AppItemsList.Any())
                    {
                        foreach (var o in appsection.AppItemsList)
                        {
                            var appItem = context.AppHomeSectionItemsDB.Where(x => x.SectionItemID == o.SectionItemID && x.Deleted == false).FirstOrDefault();
                            if (appItem == null)
                            {
                                if (o.HasOffer)
                                {
                                    o.OfferStartTime = o.OfferStartTime;
                                    o.OfferEndTime = o.OfferEndTime;
                                }
                                else
                                {
                                    o.OfferStartTime = null;
                                    o.OfferEndTime = null;
                                }

                                #region Category, BaseCategory,Item,N etc

                                if (appsection.SectionSubType == "Base Category")
                                {
                                    o.apphomesections = ar;
                                    o.CreatedDate = indianTime;
                                    o.UpdatedDate = indianTime;
                                    o.BaseCategoryId = o.RedirectionID;
                                    o.Active = true;
                                    context.AppHomeSectionItemsDB.Add(o);
                                    context.Commit();

                                }
                                else if (appsection.SectionSubType == "Category")
                                {
                                    var itemBaseCats = context.Categorys.Where(x => x.Categoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid }).ToList();

                                    foreach (var Catdata in itemBaseCats)
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.BaseCategoryId = Catdata.BaseCategoryId;
                                        o.CategoryId = Catdata.Categoryid;
                                        o.Active = true;
                                        context.AppHomeSectionItemsDB.Add(o);
                                        context.Commit();
                                    }

                                }
                                else if (appsection.SectionSubType == "SubCategory")
                                {
                                    if (o.RedirectionType == "Item")
                                    {
                                        var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.ItemId = Catdata.ItemId;
                                            o.BaseCategoryId = Catdata.BaseCategoryid;
                                            o.CategoryId = Catdata.Categoryid;
                                            o.SubCategoryId = Catdata.SubCategoryId;
                                            o.SubsubCategoryId = Catdata.SubsubCategoryid;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }

                                    }
                                    else if (o.RedirectionType == "Category")
                                    {
                                        var itemBaseCats = context.Categorys.Where(x => x.Categoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.BaseCategoryId = Catdata.BaseCategoryId;
                                            o.CategoryId = Catdata.Categoryid;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }
                                    }
                                    else if (o.RedirectionType == "Base Category")
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.BaseCategoryId = o.RedirectionID;
                                        o.Active = true;
                                        context.AppHomeSectionItemsDB.Add(o);
                                        context.Commit();
                                    }
                                    else if (o.RedirectionType == "Brand")
                                    {
                                        // var itemBaseCats = context.SubsubCategorys.Where(x => x.SubsubCategoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid, i.SubsubCategoryid, i.SubCategoryId }).ToList();

                                        string query = "select distinct e.BaseCategoryId,e.Categoryid, d.SubCategoryId, a.SubsubCategoryid " +
                                                         " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId " +
                                                         " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId inner " +
                                                         " join SubCategories d on d.SubCategoryId = c.SubCategoryId inner join Categories e on e.Categoryid = c.Categoryid" +
                                                         " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and b.BrandCategoryMappingId=" + o.RedirectionID;
                                        var itemBaseCat = context.Database.SqlQuery<SubsubCategoryDTOM>(query).FirstOrDefault();
                                        if (itemBaseCat != null)
                                        {

                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.BaseCategoryId = itemBaseCat.BaseCategoryId.Value;
                                            o.CategoryId = itemBaseCat.Categoryid;
                                            o.SubCategoryId = itemBaseCat.SubCategoryId;
                                            o.SubsubCategoryId = itemBaseCat.SubsubCategoryid;
                                            o.Active = true;

                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }

                                    }
                                    else if (o.RedirectionType == "SubCategory")
                                    {
                                        var subCats = context.SubCategorys.Where(x => x.SubCategoryId == o.RedirectionID).Select(i => new { i.SubCategoryId }).ToList();

                                        foreach (var Catdata in subCats)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.SubCategoryId = Catdata.SubCategoryId;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }

                                    }

                                }
                                else if (appsection.SectionSubType == "Brand")
                                {

                                    if (o.RedirectionType == "Item")
                                    {
                                        var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.ItemId = Catdata.ItemId;
                                            o.BaseCategoryId = Catdata.BaseCategoryid;
                                            o.CategoryId = Catdata.Categoryid;
                                            o.SubCategoryId = Catdata.SubCategoryId;
                                            o.SubsubCategoryId = Catdata.SubsubCategoryid;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }

                                    }
                                    else if (o.RedirectionType == "Category")
                                    {
                                        var itemBaseCats = context.Categorys.Where(x => x.Categoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.BaseCategoryId = Catdata.BaseCategoryId;
                                            o.CategoryId = Catdata.Categoryid;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }
                                    }
                                    else if (o.RedirectionType == "Base Category")
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.BaseCategoryId = o.RedirectionID;
                                        o.Active = true;
                                        context.AppHomeSectionItemsDB.Add(o);
                                        context.Commit();
                                    }
                                    else if (o.RedirectionType == "Brand")
                                    {
                                        // var itemBaseCats = context.SubsubCategorys.Where(x => x.SubsubCategoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid, i.SubsubCategoryid, i.SubCategoryId }).ToList();

                                        string query = "select distinct e.BaseCategoryId,e.Categoryid, d.SubCategoryId, a.SubsubCategoryid " +
                                                         " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId " +
                                                         " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId inner " +
                                                         " join SubCategories d on d.SubCategoryId = c.SubCategoryId inner join Categories e on e.Categoryid = c.Categoryid" +
                                                         " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and b.BrandCategoryMappingId=" + o.RedirectionID;
                                        var itemBaseCat = context.Database.SqlQuery<SubsubCategoryDTOM>(query).FirstOrDefault();
                                        if (itemBaseCat != null)
                                        {

                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.BaseCategoryId = itemBaseCat.BaseCategoryId.Value;
                                            o.CategoryId = itemBaseCat.Categoryid;
                                            o.SubCategoryId = itemBaseCat.SubCategoryId;
                                            o.SubsubCategoryId = itemBaseCat.SubsubCategoryid;
                                            o.Active = true;

                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }

                                    }
                                    else if (o.RedirectionType == "SubCategory")
                                    {
                                        var subCats = context.SubCategorys.Where(x => x.SubCategoryId == o.RedirectionID).Select(i => new { i.SubCategoryId }).ToList();

                                        foreach (var Catdata in subCats)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.SubCategoryId = Catdata.SubCategoryId;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }

                                    }

                                }
                                else if (appsection.SectionSubType == "Item")
                                {
                                    var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();

                                    foreach (var Catdata in itemBaseCats)
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.ItemId = Catdata.ItemId;
                                        o.BaseCategoryId = Catdata.BaseCategoryid;
                                        o.CategoryId = Catdata.Categoryid;
                                        o.SubCategoryId = Catdata.SubCategoryId;
                                        o.SubsubCategoryId = Catdata.SubsubCategoryid;
                                        o.Active = true;
                                        context.AppHomeSectionItemsDB.Add(o);
                                        context.Commit();
                                    }
                                }
                                else if (appsection.SectionSubType == "Offer")
                                {
                                    var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();

                                    foreach (var Catdata in itemBaseCats)
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.ItemId = Catdata.ItemId;
                                        o.BaseCategoryId = Catdata.BaseCategoryid;
                                        o.CategoryId = Catdata.Categoryid;
                                        o.SubCategoryId = Catdata.SubCategoryId;
                                        o.SubsubCategoryId = Catdata.SubsubCategoryid;
                                        o.Active = true;
                                        context.AppHomeSectionItemsDB.Add(o);
                                        context.Commit();
                                    }
                                }
                                else if (appsection.SectionSubType == "Flash Deal")
                                {

                                    var itemBaseCats = context.itemMasters.Where(x => x.WarehouseId == appsection.WarehouseID && x.ItemId == o.ItemId).ToList();
                                    foreach (var Catdata in itemBaseCats)
                                    {

                                        if (!o.Deleted)
                                        {
                                            bool IsExistresult = context.AppHomeSectionItemsDB.Any(x => x.ItemId == Catdata.ItemId && x.IsFlashDeal == true && x.Active == true
                                                      && x.Deleted == false && (x.OfferEndTime.HasValue && x.OfferEndTime.Value >= o.OfferStartTime) && (x.OfferStartTime.HasValue && x.OfferStartTime.Value <= o.OfferStartTime));
                                            if (IsExistresult)
                                            {
                                                result.AppHomeSections = null;
                                                result.error = true;
                                                result.msg = "Error : flashdeal already active, please deactive Item : " + Catdata.itemname;
                                                return result;
                                            }
                                        }

                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.ItemId = Catdata.ItemId;
                                        o.RedirectionID = Catdata.ItemId;
                                        o.BaseCategoryId = Catdata.BaseCategoryid;
                                        o.CategoryId = Catdata.Categoryid;
                                        o.SubCategoryId = Catdata.SubCategoryId;
                                        o.SubsubCategoryId = Catdata.SubsubCategoryid;
                                        o.FlashDealMaxQtyPersonCanTake = o.FlashDealMaxQtyPersonCanTake;
                                        o.FlashDealQtyAvaiable = o.FlashDealQtyAvaiable;
                                        o.FlashDealSpecialPrice = o.FlashDealSpecialPrice;

                                        if (appsection.Deleted == true)
                                        {
                                            o.IsFlashDeal = false;
                                            o.Active = false;
                                        }
                                        else
                                        {
                                            o.IsFlashDeal = true;
                                            o.Active = true;
                                        }
                                        //o.RedirectionType = "Flash Deal";
                                        context.AppHomeSectionItemsDB.Add(o);
                                        context.Commit();
                                        if (Catdata != null)
                                        {
                                            Catdata.OfferCategory = 2;
                                            Catdata.OfferQtyAvaiable = o.FlashDealQtyAvaiable;
                                            Catdata.OfferMaxQtyPersonCanTake = o.FlashDealMaxQtyPersonCanTake;

                                            if (appsection.Deleted == true)
                                            {
                                                Catdata.IsOffer = false;
                                            }
                                            else
                                            {
                                                Catdata.IsOffer = true;
                                            }
                                            Catdata.OfferType = "FlashDeal";
                                            Catdata.FlashDealSpecialPrice = o.FlashDealSpecialPrice;
                                            Catdata.OfferStartTime = Convert.ToDateTime(o.OfferStartTime);
                                            Catdata.OfferEndTime = Convert.ToDateTime(o.OfferEndTime);
                                            context.Entry(Catdata).State = EntityState.Modified;
                                            context.Commit();
                                        }

                                    }
                                }
                                else if (appsection.SectionSubType == "Slider")
                                {

                                    if (o.RedirectionType == "Item")
                                    {
                                        var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.ItemId = Catdata.ItemId;
                                            o.BaseCategoryId = Catdata.BaseCategoryid;
                                            o.CategoryId = Catdata.Categoryid;
                                            o.SubCategoryId = Catdata.SubCategoryId;
                                            o.SubsubCategoryId = Catdata.SubsubCategoryid;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();

                                            //context.SaveChanges();
                                        }

                                    }
                                    else if (o.RedirectionType == "Category")
                                    {
                                        var itemBaseCats = context.Categorys.Where(x => x.Categoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.BaseCategoryId = Catdata.BaseCategoryId;
                                            o.CategoryId = Catdata.Categoryid;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();

                                            //context.SaveChanges();
                                        }
                                    }
                                    else if (o.RedirectionType == "Base Category")
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.BaseCategoryId = o.RedirectionID;
                                        o.Active = true;
                                        context.AppHomeSectionItemsDB.Add(o);
                                        context.Commit();
                                    }
                                    else if (o.RedirectionType == "Brand")
                                    {
                                        //var itemBaseCats = context.SubsubCategorys.Where(x => x.SubsubCategoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid, i.SubsubCategoryid, i.SubCategoryId }).ToList();
                                        //foreach (var Catdata in itemBaseCats)
                                        //{
                                        string query = "select distinct e.BaseCategoryId,e.Categoryid, d.SubCategoryId, a.SubsubCategoryid " +
                                                         " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId " +
                                                         " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId inner " +
                                                         " join SubCategories d on d.SubCategoryId = c.SubCategoryId inner join Categories e on e.Categoryid = c.Categoryid" +
                                                         " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and b.BrandCategoryMappingId=" + o.RedirectionID;
                                        var itemBaseCat = context.Database.SqlQuery<SubsubCategoryDTOM>(query).FirstOrDefault();
                                        if (itemBaseCat != null)
                                        {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.BaseCategoryId = itemBaseCat.BaseCategoryId.Value;
                                            o.CategoryId = itemBaseCat.Categoryid;
                                            o.SubCategoryId = itemBaseCat.SubCategoryId;
                                            o.SubsubCategoryId = itemBaseCat.SubsubCategoryid;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }

                                    }
                                    else
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.ItemId = 0;
                                        o.BaseCategoryId = 0;
                                        o.CategoryId = 0;
                                        o.SubCategoryId = 0;
                                        o.SubsubCategoryId = 0;
                                        o.Active = true;
                                        context.AppHomeSectionItemsDB.Add(o);
                                        context.Commit();


                                    }

                                }
                                else
                                {
                                    o.apphomesections = ar;
                                    o.CreatedDate = indianTime;
                                    o.UpdatedDate = indianTime;
                                    o.Active = true;
                                    context.AppHomeSectionItemsDB.Add(o);
                                    context.Commit();
                                }
                                #endregion


                            }
                            else
                            {
                                if (o.HasOffer || appItem.IsFlashDeal)
                                {
                                    appItem.OfferStartTime = o.OfferStartTime;
                                    appItem.OfferEndTime = o.OfferEndTime;
                                }
                                else
                                {
                                    appItem.OfferStartTime = null;
                                    appItem.OfferEndTime = null;
                                }
                                appItem.RedirectionType = o.RedirectionType;
                                appItem.RedirectionID = o.RedirectionID;
                                appItem.Active = o.Active;
                                appItem.ImageLevel = o.ImageLevel;
                                appItem.HasOffer = o.HasOffer;

                                if (appItem != null && appItem.OfferEndTime.HasValue && appItem.OfferEndTime.Value <= indianTime)
                                {
                                    appItem.Active = false;
                                    appItem.Deleted = true;
                                }
                                if (o.Deleted)
                                {
                                    appItem.Active = false;
                                    appItem.Deleted = true;
                                }
                                appItem.TileName = o.TileName;
                                appItem.TileImage = o.TileImage;
                                appItem.BannerName = o.BannerName;
                                appItem.BannerActivity = o.BannerActivity;
                                appItem.RedirectionUrl = o.RedirectionUrl;
                                appItem.BannerImage = o.BannerImage;
                                appItem.UpdatedDate = indianTime;
                                appsection.Active = true;

                                #region Category, BaseCategory,Item,N etc
                                if (appsection.SectionSubType == "Base Category")
                                {
                                    appItem.UpdatedDate = indianTime;
                                    appItem.BaseCategoryId = o.RedirectionID;
                                    appItem.Active = true;
                                    appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;

                                }
                                else if (appsection.SectionSubType == "Category")
                                {
                                    var itemBaseCats = context.Categorys.Where(x => x.Categoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid }).ToList();
                                    foreach (var Catdata in itemBaseCats)
                                    {
                                        appItem.UpdatedDate = indianTime;
                                        appItem.BaseCategoryId = Catdata.BaseCategoryId;
                                        appItem.CategoryId = Catdata.Categoryid;
                                        appItem.Active = true;
                                        appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                    }
                                }
                                else if (appsection.SectionSubType == "SubCategory")
                                {
                                    string query = "select distinct e.BaseCategoryId,e.Categoryid, d.SubCategoryId, 0 SubsubCategoryid from SubcategoryCategoryMappings c " +
                                                   "  inner  join SubCategories d on d.SubCategoryId = c.SubCategoryId inner join Categories e on e.Categoryid = c.Categoryid" +
                                                   "  where  d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and d.SubCategoryId=" + o.RedirectionID;
                                    var itemBaseCat = context.Database.SqlQuery<SubsubCategoryDTOM>(query).FirstOrDefault();
                                    if (itemBaseCat != null)
                                    {
                                        appItem.UpdatedDate = indianTime;
                                        appItem.BaseCategoryId = itemBaseCat.BaseCategoryId.Value;
                                        appItem.CategoryId = itemBaseCat.Categoryid;
                                        appItem.SubCategoryId = itemBaseCat.SubCategoryId;
                                        appItem.SubsubCategoryId = 0;
                                        appItem.Active = true;
                                        appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                    }
                                }
                                else if (appsection.SectionSubType == "Brand")
                                {
                                    if (appItem.RedirectionType == "Item")
                                    {
                                        var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            appItem.UpdatedDate = indianTime;
                                            appItem.ItemId = Catdata.ItemId;
                                            appItem.BaseCategoryId = Catdata.BaseCategoryid;
                                            appItem.CategoryId = Catdata.Categoryid;
                                            appItem.SubCategoryId = Catdata.SubCategoryId;
                                            appItem.SubsubCategoryId = Catdata.SubsubCategoryid;
                                            appItem.Active = true;
                                            appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                        }

                                    }
                                    else if (o.RedirectionType == "Category")
                                    {
                                        var itemBaseCats = context.Categorys.Where(x => x.Categoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            appItem.UpdatedDate = indianTime;
                                            appItem.BaseCategoryId = Catdata.BaseCategoryId;
                                            appItem.CategoryId = Catdata.Categoryid;
                                            appItem.Active = true;
                                            appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                        }
                                    }
                                    else if (o.RedirectionType == "Base Category")
                                    {

                                        appItem.UpdatedDate = indianTime;
                                        appItem.BaseCategoryId = o.RedirectionID;
                                        appItem.Active = true;
                                        appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                    }
                                    else if (o.RedirectionType == "Brand")
                                    {
                                        //var itemBaseCats = context.SubsubCategorys.Where(x => x.SubsubCategoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid, i.SubsubCategoryid, i.SubCategoryId }).ToList();
                                        //foreach (var Catdata in itemBaseCats)
                                        //{
                                        string query = "select distinct e.BaseCategoryId,e.Categoryid, d.SubCategoryId, a.SubsubCategoryid " +
                                                        " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId " +
                                                        " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId inner " +
                                                        " join SubCategories d on d.SubCategoryId = c.SubCategoryId inner join Categories e on e.Categoryid = c.Categoryid" +
                                                        " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and b.BrandCategoryMappingId=" + o.RedirectionID;
                                        var itemBaseCat = context.Database.SqlQuery<SubsubCategoryDTOM>(query).FirstOrDefault();
                                        if (itemBaseCat != null)
                                        {
                                            appItem.UpdatedDate = indianTime;
                                            appItem.BaseCategoryId = itemBaseCat.BaseCategoryId.Value;
                                            appItem.CategoryId = itemBaseCat.Categoryid;
                                            appItem.SubCategoryId = itemBaseCat.SubCategoryId;
                                            appItem.SubsubCategoryId = itemBaseCat.SubsubCategoryid;
                                            appItem.Active = true;
                                            appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                        }

                                    }


                                }
                                else if (appsection.SectionSubType == "Item")
                                {
                                    var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();

                                    foreach (var Catdata in itemBaseCats)
                                    {

                                        appItem.UpdatedDate = indianTime;
                                        appItem.ItemId = Catdata.ItemId;
                                        appItem.BaseCategoryId = Catdata.BaseCategoryid;
                                        appItem.CategoryId = Catdata.Categoryid;
                                        appItem.SubCategoryId = Catdata.SubCategoryId;
                                        appItem.SubsubCategoryId = Catdata.SubsubCategoryid;
                                        appItem.Active = true;
                                        appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;

                                    }
                                }
                                else if (appsection.SectionSubType == "Offer")
                                {
                                    var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();

                                    foreach (var Catdata in itemBaseCats)
                                    {
                                        appItem.UpdatedDate = indianTime;
                                        appItem.ItemId = Catdata.ItemId;
                                        appItem.BaseCategoryId = Catdata.BaseCategoryid;
                                        appItem.CategoryId = Catdata.Categoryid;
                                        appItem.SubCategoryId = Catdata.SubCategoryId;
                                        appItem.SubsubCategoryId = Catdata.SubsubCategoryid;
                                        appItem.Active = true;
                                        appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;

                                    }
                                }
                                else if (appsection.SectionSubType == "Flash Deal")
                                {

                                    var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();

                                    foreach (var Catdata in itemBaseCats)
                                    {


                                        appItem.UpdatedDate = indianTime;
                                        appItem.ItemId = Catdata.ItemId;
                                        appItem.BaseCategoryId = Catdata.BaseCategoryid;
                                        appItem.CategoryId = Catdata.Categoryid;
                                        appItem.SubCategoryId = Catdata.SubCategoryId;
                                        appItem.SubsubCategoryId = Catdata.SubsubCategoryid;
                                        appItem.FlashDealMaxQtyPersonCanTake = o.FlashDealMaxQtyPersonCanTake;
                                        appItem.FlashDealQtyAvaiable = o.FlashDealQtyAvaiable;
                                        appItem.FlashDealSpecialPrice = o.FlashDealSpecialPrice;
                                        appItem.IsFlashDeal = true;
                                        appItem.Active = true;
                                        ItemMaster itemMaster = context.itemMasters.Where(x => x.ItemId == Catdata.ItemId && x.WarehouseId == appsection.WarehouseID && x.active == true && x.Deleted == false).FirstOrDefault();
                                        if (itemMaster != null)
                                        {
                                            itemMaster.OfferCategory = 2;
                                            itemMaster.OfferQtyAvaiable = o.FlashDealQtyAvaiable;
                                            itemMaster.OfferMaxQtyPersonCanTake = o.FlashDealMaxQtyPersonCanTake;
                                            itemMaster.IsOffer = true;
                                            itemMaster.OfferType = "FlashDeal";
                                            itemMaster.FlashDealSpecialPrice = o.FlashDealSpecialPrice;
                                            itemMaster.OfferStartTime = Convert.ToDateTime(o.OfferStartTime);
                                            itemMaster.OfferEndTime = Convert.ToDateTime(o.OfferEndTime);
                                            if (!o.Deleted)
                                            {
                                                bool IsExistresult = context.AppHomeSectionItemsDB.Any(x => x.ItemId == Catdata.ItemId && x.SectionItemID != o.SectionItemID && x.IsFlashDeal == true && x.Active == true
                                                          && x.Deleted == false && (x.OfferEndTime.HasValue && x.OfferEndTime.Value >= o.OfferStartTime) && (x.OfferStartTime.HasValue && x.OfferStartTime.Value <= o.OfferStartTime));

                                                if (IsExistresult)
                                                {
                                                    result.AppHomeSections = null;
                                                    result.error = true;
                                                    result.msg = "Error : flashdeal already active, please deactive Item : " + itemMaster.itemname;
                                                    return result;
                                                }
                                            }
                                            context.Entry(itemMaster).State = EntityState.Modified;
                                        }
                                    }


                                }
                                else if (appsection.SectionSubType == "Slider")
                                {

                                    if (o.RedirectionType == "Item")
                                    {
                                        var itemBaseCats = context.itemMasters.Where(x => x.ItemId == o.RedirectionID).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid, x.SubCategoryId, x.SubsubCategoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            appItem.UpdatedDate = indianTime;
                                            appItem.ItemId = Catdata.ItemId;
                                            appItem.BaseCategoryId = Catdata.BaseCategoryid;
                                            appItem.CategoryId = Catdata.Categoryid;
                                            appItem.SubCategoryId = Catdata.SubCategoryId;
                                            appItem.SubsubCategoryId = Catdata.SubsubCategoryid;
                                            appItem.Active = true;
                                            appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                        }

                                    }
                                    else if (o.RedirectionType == "Category")
                                    {
                                        var itemBaseCats = context.Categorys.Where(x => x.Categoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid }).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            appItem.UpdatedDate = indianTime;
                                            appItem.BaseCategoryId = Catdata.BaseCategoryId;
                                            appItem.CategoryId = Catdata.Categoryid;
                                            appItem.Active = true;
                                            appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                        }
                                    }
                                    else if (o.RedirectionType == "Base Category")
                                    {
                                        appItem.UpdatedDate = indianTime;
                                        appItem.BaseCategoryId = o.RedirectionID;
                                        appItem.Active = true;
                                        appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                    }
                                    else if (o.RedirectionType == "Brand")
                                    {
                                        string query = "select distinct e.BaseCategoryId,e.Categoryid, d.SubCategoryId, a.SubsubCategoryid " +
                                                        " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId " +
                                                        " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId inner " +
                                                        " join SubCategories d on d.SubCategoryId = c.SubCategoryId inner join Categories e on e.Categoryid = c.Categoryid" +
                                                        " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and b.BrandCategoryMappingId=" + o.RedirectionID;
                                        var itemBaseCat = context.Database.SqlQuery<SubsubCategoryDTOM>(query).FirstOrDefault();
                                        if (itemBaseCat != null)
                                        {
                                            appItem.apphomesections = ar;
                                            appItem.CreatedDate = indianTime;
                                            appItem.UpdatedDate = indianTime;
                                            appItem.BaseCategoryId = itemBaseCat.BaseCategoryId.Value;
                                            appItem.CategoryId = itemBaseCat.Categoryid;
                                            appItem.SubCategoryId = itemBaseCat.SubCategoryId;
                                            appItem.SubsubCategoryId = itemBaseCat.SubsubCategoryid;
                                            appItem.Active = true;
                                            appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                        }

                                    }
                                    else
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.ItemId = 0;
                                        o.BaseCategoryId = 0;
                                        o.CategoryId = 0;
                                        o.SubCategoryId = 0;
                                        o.SubsubCategoryId = 0;
                                        o.RedirectionID = 0;
                                        appItem.RedirectionID = 0;
                                        appItem.ItemId = 0;
                                        appItem.BaseCategoryId = 0;
                                        appItem.CategoryId = 0;
                                        appItem.SubCategoryId = 0;
                                        appItem.SubsubCategoryId = 0;
                                        o.Active = true;

                                    }


                                }
                                else
                                {
                                    o.apphomesections = ar;
                                    o.CreatedDate = indianTime;
                                    o.UpdatedDate = indianTime;
                                    o.Active = true;
                                    context.AppHomeSectionItemsDB.Add(o);
                                    context.Commit();
                                }
                                #endregion
                                context.Entry(appItem).State = EntityState.Modified;
                                context.Commit();
                            }
                        }
                    }
                    appsection = context.AppHomeSectionsDB.Where(x => x.SectionID == appsection.SectionID && x.SubCategoryID == SubCatId && x.Deleted == false).Include("AppItemsList").FirstOrDefault();
                    if (appsection != null && appsection.AppItemsList.Any())
                    {
                        var datetimenow = indianTime;
                        var data = appsection.AppItemsList[0].OfferEndTime.HasValue;//|| !X.OfferEndTime.HasValue
                        appsection.AppItemsList = appsection.AppItemsList.Where(X => X.Deleted == false && ((X.OfferEndTime.HasValue && X.OfferEndTime.Value >= indianTime) || !X.OfferEndTime.HasValue)).ToList();
                        foreach (var item in appsection.AppItemsList)
                        {
                            item.apphomesections = null;

                            if (!string.IsNullOrEmpty(item.TileImage) && !item.TileImage.Contains("http"))
                            {
                                item.TileImage = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                      , HttpContext.Current.Request.Url.DnsSafeHost
                                                                      , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                      , item.TileImage);
                            }
                        }
                    }

                    result.AppHomeSections = appsection;

                    return result;
                }

            }
        }

        [Route("DeleteSection")]
        [HttpDelete]
        public bool DeleteSection(int SectionID)
        {
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            using (var context = new AuthContext())
            {

                var ara = context.AppHomeSectionsDB.Where(x => x.SectionID == SectionID && x.SubCategoryID == SubCatId && x.Deleted == false).FirstOrDefault();
                List<AppHomeSectionItems> appitem = new List<AppHomeSectionItems>();
                if (ara != null)
                {
                    appitem = context.AppHomeSectionItemsDB.Where(x => x.apphomesections.SectionID == ara.SectionID && x.Deleted == false).ToList();
                    if (appitem.Count > 0)
                    {
                        foreach (var o in appitem)
                        {

                            o.Deleted = true;
                            o.Active = false;
                            o.IsFlashDeal = false;
                            o.UpdatedDate = DateTime.Now;
                            context.Entry(o).State = EntityState.Modified;

                            if (ara.SectionSubType == "Flash Deal")
                            {
                                ItemMaster iteminfo = context.itemMasters.Where(x => x.ItemId == o.ItemId).FirstOrDefault();
                                if (iteminfo != null)
                                {
                                    iteminfo.IsOffer = false;
                                    iteminfo.UpdatedDate = indianTime;
                                    context.Entry(iteminfo).State = EntityState.Modified;
                                }
                            }

                        }
                    }

                    ara.Active = false;
                    ara.Deleted = true;
                    context.Entry(ara).State = EntityState.Modified;
                    if (context.Commit() > 0)
                    {
                        if (ara.SectionSubType == "Flash Deal")
                        {
                            foreach (var o in appitem)
                            {
                                ManulaExpireFlasDeal(o.SectionItemID);
                            }
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        public bool ManulaExpireFlasDeal(int SectionItemID)
        {
            AppHomeSectionItems appHomeSectionitem = new AppHomeSectionItems { SectionItemID = SectionItemID };
            return ExpireFlashDeal(appHomeSectionitem);
        }
        public bool ExpireFlashDeal(AppHomeSectionItems appHomeSectionitem)
        {
            using (var context = new AuthContext())
            {
                var appHomeitem = context.AppHomeSectionItemsDB.Where(x => x.SectionItemID == appHomeSectionitem.SectionItemID).Include(x => x.apphomesections).FirstOrDefault();
                if (appHomeitem != null)
                {
                    appHomeitem.Expired = false;
                    appHomeitem.Active = false;
                    appHomeitem.Deleted = true;
                    appHomeitem.UpdatedDate = DateTime.Now;
                    context.Entry(appHomeitem).State = EntityState.Modified;
                    AppHomeSections apphome = null;
                    if (appHomeitem.apphomesections != null)
                    {
                        apphome = context.AppHomeSectionsDB.Where(x => x.SectionID == appHomeitem.apphomesections.SectionID).Include("AppItemsList").FirstOrDefault();
                        if (apphome.AppItemsList.All(x => !x.Active && x.Deleted))
                        {
                            apphome.Active = false;
                            apphome.Deleted = true;
                            apphome.UpdatedDate = DateTime.Now;
                            context.Entry(apphome).State = EntityState.Modified;
                        }
                    }
                    context.Commit();
                    if (apphome != null)
                    {
                        string apptype = appHomeitem.apphomesections.AppType.Replace(" ", "");
                        var publishedData = context.PublishAppHomeDB.Where(x => x.WarehouseID == appHomeitem.apphomesections.WarehouseID && x.AppType == appHomeitem.apphomesections.AppType && x.Deleted == false).FirstOrDefault();
                        List<AppHomeSections> dbsections = new List<AppHomeSections>();
                        dbsections = JsonConvert.DeserializeObject<List<AppHomeSections>>(publishedData.ApphomeSection);
                        foreach (var item in dbsections.Where(x => x.SectionID == appHomeitem.apphomesections.SectionID))
                        {
                            item.Deleted = apphome.Deleted;
                            item.Active = apphome.Active;
                            if (item.AppItemsList.Any(x => x.SectionItemID == appHomeitem.SectionItemID))
                            {
                                item.AppItemsList.FirstOrDefault(x => x.SectionItemID == appHomeitem.SectionItemID).Expired = true;
                                item.AppItemsList.FirstOrDefault(x => x.SectionItemID == appHomeitem.SectionItemID).Active = false;
                                item.AppItemsList.FirstOrDefault(x => x.SectionItemID == appHomeitem.SectionItemID).Deleted = true;
                            }
                        }
                        dbsections.RemoveAll(x => x.Deleted);

                        string jsonData = JsonConvert.SerializeObject(dbsections);
                        publishedData.UpdatedDate = DateTime.Now;
                        publishedData.ApphomeSection = jsonData;
                        context.Entry(publishedData).State = EntityState.Modified;
                        context.Commit();

                        string appType = "Store";
                        var stringappType = appType + "_" + publishedData.SubCategoryID.ToString();
                        int WarehouseId = publishedData.WarehouseID;

                        var publishData = new PublishAppHome();

#if !DEBUG
                        Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                       // _cacheProvider.Remove(Caching.CacheKeyHelper.APPHomeCacheKey(stringappType.Replace(" ", ""), WarehouseId.ToString()));
                        publishData = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.APPHomeCacheKey(stringappType.Replace(" ", ""), WarehouseId.ToString()), () => GetPublisheddataSeller(appType, WarehouseId, publishData.SubCategoryID));
#else
                        publishData = GetPublisheddataSeller(appType, WarehouseId, publishedData.SubCategoryID);
#endif


                    }

                    var itemmaster = context.itemMasters.FirstOrDefault(x => x.ItemId == appHomeitem.ItemId);
                    if (itemmaster != null)
                    {
                        if (context.itemMasters.Any(x => x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.WarehouseId == itemmaster.WarehouseId && x.IsOffer && x.OfferCategory == 1 && x.OfferStartTime.HasValue && x.OfferEndTime.HasValue && x.OfferStartTime.Value <= indianTime && x.OfferEndTime.Value >= indianTime))
                        {
                            var firstItem = context.itemMasters.FirstOrDefault(x => x.ItemMultiMRPId == itemmaster.ItemMultiMRPId && x.WarehouseId == itemmaster.WarehouseId && x.IsOffer && x.OfferCategory == 1 && x.OfferStartTime.HasValue && x.OfferEndTime.HasValue && x.OfferStartTime.Value <= indianTime && x.OfferEndTime.Value >= indianTime);
                            itemmaster.IsOffer = true;
                            itemmaster.OfferCategory = 1;
                            itemmaster.OfferType = "ItemMaster";
                            itemmaster.OfferStartTime = firstItem.OfferStartTime;
                            itemmaster.OfferEndTime = firstItem.OfferEndTime;
                            itemmaster.UpdatedDate = DateTime.Now;
                        }
                        else
                        {
                            itemmaster.IsOffer = false;
                            itemmaster.OfferCategory = 0;
                            itemmaster.UpdatedDate = DateTime.Now;
                        }
                        context.Entry(itemmaster).State = EntityState.Modified;
                    }
                }
                context.Commit();
            }

            return true;
        }

        [Route("AddCompleteAppHome")]
        [HttpPost]
        public List<AppHomeSections> AddCompleteAppHome(List<AppHomeSections> apphome)
        {
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            using (var context = new AuthContext())
            {
                int WarehouseId = apphome[0].WarehouseID;
                string apptype = apphome[0].AppType;
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    string jsonData = JsonConvert.SerializeObject(apphome);
                    var bsObj = JsonConvert.DeserializeObject<List<AppHomeSections>>(jsonData);

                    List<int> Ids = new List<int>();
                    Ids = apphome.Select(x => x.SectionID).ToList();
                    var APPH = context.AppHomeSectionsDB.Where(x => Ids.Contains(x.SectionID) && x.SubCategoryID == SubCatId && x.Deleted == false).ToList();
                    int i = 1;
                    apphome.ForEach(x => { x.Sequence = i; i++; });
                    APPH.ForEach(x =>
                    {
                        x.Sequence = apphome.Any(y => y.SectionID == x.SectionID) ? apphome.FirstOrDefault(y => y.SectionID == x.SectionID).Sequence : 0;
                        x.WebViewUrl = apphome.Any(y => y.SectionID == x.SectionID) ? apphome.FirstOrDefault(y => y.SectionID == x.SectionID).WebViewUrl : "";
                        context.Entry(x).State = EntityState.Modified;
                    });
                    context.Commit();
                    foreach (var appHomelist in apphome)
                    {
                        foreach (var data in appHomelist.AppItemsList)
                        {
                            data.apphomesections = null;
                        }
                    }
                    dbContextTransaction.Commit();
                    return apphome;
                }
            }
        }

        //Publish App Home Section Live
        [Route("PublishAppHome")]
        [HttpPost]
        public List<AppHomeSections> PublishAppHome(List<AppHomeSections> apphome)
        {
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
            using (var context = new AuthContext())
            {
                PublishAppHome publishData = new PublishAppHome();
                string jsonData = JsonConvert.SerializeObject(apphome);
                bool result = false;
                foreach (var a in apphome)
                {
                    publishData = context.PublishAppHomeDB.Where(x => x.AppType == a.AppType && x.WarehouseID == a.WarehouseID && x.SubCategoryID == SubCatId).FirstOrDefault();
                    if (publishData != null)
                    {
                        publishData.language = "en";
                        publishData.AppType = a.AppType;
                        publishData.WarehouseID = a.WarehouseID;
                        publishData.ApphomeSection = jsonData;
                        publishData.AppType = a.AppType;
                        publishData.WarehouseID = a.WarehouseID;
                        publishData.ApphomeSection = jsonData;
                        publishData.UpdatedDate = indianTime;
                        publishData.Deleted = false;
                        context.Entry(publishData).State = EntityState.Modified;
                        result = context.Commit() > 0;
                    }
                    else
                    {
                        publishData = new PublishAppHome();


                        publishData.language = "en";
                        publishData.AppType = a.AppType;
                        publishData.WarehouseID = a.WarehouseID;
                        publishData.ApphomeSection = jsonData;
                        publishData.SubCategoryID = SubCatId;
                        publishData.CreatedDate = indianTime;
                        publishData.UpdatedDate = indianTime;
                        context.PublishAppHomeDB.Add(publishData);
                        result = context.Commit() > 0;
                    }
                    break;
                }
                apphome.Where(x => x.SectionSubType == "Flash Deal" && x.Active && !x.Deleted).SelectMany(x => x.AppItemsList).ToList().ForEach(x =>
                {
                    if (x.IsFlashDeal && x.Active && x.OfferEndTime.HasValue && x.OfferEndTime.Value > DateTime.Now)
                    {
                        var jobId = BackgroundJob.Schedule(
                                  () => ExpireFlashDeal(x),
                        TimeSpan.FromMinutes((x.OfferEndTime.Value - DateTime.Now).TotalMinutes));
                    }
                }
                );
                if (publishData != null && result)
                {
                    string appType = "Store";
                    var publishedData = new PublishAppHome();
                    var stringappType = appType + "_" + publishData.SubCategoryID.ToString();
                    int WarehouseId = publishData.WarehouseID;
                    logger.Info("start  APPHomeCacheKey stringappType: " + stringappType + "WarehouseId:" + WarehouseId + "SubCatId:" + publishData.SubCategoryID);
#if !DEBUG
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    _cacheProvider.Remove(Caching.CacheKeyHelper.APPHomeCacheKey(stringappType.Replace(" ", ""), WarehouseId.ToString()));
                    publishedData = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.APPHomeCacheKey(stringappType.Replace(" ", ""), WarehouseId.ToString()), () => GetPublisheddataSeller(appType, WarehouseId, publishData.SubCategoryID));
#else
                    publishedData = GetPublisheddataSeller(appType, WarehouseId, publishData.SubCategoryID);
#endif
                }
                return apphome;
            }
        }


        [Route("DeleteAppHome")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage DeleteAppHome(List<AppHomeSections> apphome)
        {
            string result = "";
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
            if (SubCatId > 0 && apphome != null)
            {
                using (var context = new AuthContext())
                {
                    var secionids = apphome.Select(x => x.SectionID).ToList();
                    var AppType = "Store";
                    var WarehouseID = apphome.FirstOrDefault(x => x.WarehouseID > 0).WarehouseID;

                    var publishdata = context.PublishAppHomeDB.Where(x => x.AppType == AppType && x.WarehouseID == WarehouseID && x.SubCategoryID == SubCatId).FirstOrDefault();
                    var AppHomeSectionsList = context.AppHomeSectionsDB.Where(x => secionids.Contains(x.SectionID) && x.SubCategoryID == SubCatId && x.Deleted == false).Include(x => x.AppItemsList).ToList();
                    foreach (var Sectionsitem in AppHomeSectionsList)
                    {

                        Sectionsitem.Deleted = true;
                        Sectionsitem.Active = false;
                        Sectionsitem.UpdatedDate = DateTime.Now;
                        context.Entry(Sectionsitem).State = EntityState.Modified;
                        if (Sectionsitem.SectionSubType == "Flash Deal")
                        {
                            var itemids = Sectionsitem.AppItemsList.Where(x => x.IsFlashDeal == true).Select(x => x.ItemId).ToList();


                            var iteminfo = context.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();
                            foreach (var item in iteminfo)
                            {
                                item.IsOffer = false;
                                context.Entry(item).State = EntityState.Modified;
                            }

                            var SectionsitemAppItemsList = Sectionsitem.AppItemsList.Where(x => x.IsFlashDeal == true).ToList();

                            foreach (var AppItem in SectionsitemAppItemsList)
                            {
                                AppItem.IsFlashDeal = false;
                                AppItem.Active = false;
                            }
                        }
                    }
                    publishdata.Deleted = true;
                    context.Entry(publishdata).State = EntityState.Modified;
                    context.Commit();
                    if (publishdata != null)
                    {
                        string appType = "Store";
                        var publishedData = new PublishAppHome();
                        var stringappType = appType + "_" + publishdata.SubCategoryID.ToString();
                        int WarehouseId = publishdata.WarehouseID;
                        logger.Info("start  APPHomeCacheKey stringappType: " + stringappType + "WarehouseId:" + WarehouseId + "SubCatId:" + publishdata.SubCategoryID);
#if !DEBUG
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    _cacheProvider.Remove(Caching.CacheKeyHelper.APPHomeCacheKey(stringappType.Replace(" ", ""), WarehouseId.ToString()));                    
#endif
                        result = "AppHome Deleted Successfully";
                    }

                }

            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [Route("VerifyImage")]
        [HttpPost]
        [AllowAnonymous]
        public bool VerifyImage()
        {
            logger.Info("start image UploadImageSellerAppHome");

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    //#region Image safe search detection

                    byte[] bytes;
                    using (var stream = new MemoryStream())
                    {
                        httpPostedFile.InputStream.CopyTo(stream);
                        bytes = stream.ToArray();
                    }
                    var base64String = Convert.ToBase64String(bytes);
                    bool IsNotOk = VarifyImageWithVision(base64String);

                    return IsNotOk;

                }
            }

            return false;
        }




        #region upload image
        [Route("UploadImageAppHome")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult UploadImageAppHome()
        {
            string LogoUrl = "", returnPath = "";
            logger.Info("start image UploadImageSellerAppHome");

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    #region Image safe search detection

                    byte[] bytes;
                    using (var stream = new MemoryStream())
                    {
                        httpPostedFile.InputStream.CopyTo(stream);
                        bytes = stream.ToArray();
                    }
                    var base64String = Convert.ToBase64String(bytes);
                    bool IsNotOk = VarifyImageWithVision(base64String);
                    if (IsNotOk)
                    {
                        return Created<string>(LogoUrl, null);
                    }
                    //find the files
                    //var ext = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".gif" };
                    //create service
                    //var credentails = GoogleVisionHelper.CreateCredentials("user_credentials.json");
                    //var service = GoogleVisionHelper.CreateService("MyApplication", credentails);
                    //var task = service.AnnotateAsync(returnPath, "SAFE_SEARCH_DETECTION");
                    //var result = task.Result;
                    //var keywords = result?.SafeSearchAnnotation?.Adult.ToArray();
                    //var words = String.Join(", ", keywords);
                    #endregion



                    string extension = Path.GetExtension(httpPostedFile.FileName);

                    string filename = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                    LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedLogos"), filename);

                    returnPath = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                  , HttpContext.Current.Request.Url.DnsSafeHost
                                                                  , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                  , "/UploadedLogos/" + filename);

                    httpPostedFile.SaveAs(LogoUrl);

                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(filename, "~/UploadedLogos", LogoUrl);

                    if (ConfigurationManager.AppSettings["Environment"] == "Production")
                    {
                        CloudinaryDotNet.Account account = new CloudinaryDotNet.Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);
                        Cloudinary cloudinary = new Cloudinary(account);
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(LogoUrl),
                            PublicId = "AppHome/" + filename,
                            Overwrite = true,
                            Invalidate = true,
                            Backup = false
                        };
                        var uploadResult = cloudinary.Upload(uploadParams);

                        if (uploadResult.SecureUri != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
                        {
                            LogoUrl = uploadResult.SecureUri.ToString();
                            if (System.IO.File.Exists(LogoUrl))
                            {
                                System.IO.File.Delete(LogoUrl);
                            }
                        }
                    }
                    else
                    {
                        LogoUrl = returnPath;
                    }

                }
            }

            return Created<string>(LogoUrl, LogoUrl);
        }



        public bool VarifyImageWithVision(string base64String)
        {
            bool result = false;
            string path = ConfigurationManager.AppSettings["GetvisionUrl"];
            using (var client = new WebClient())
            {
                Mainrequests Mainrequests = new Mainrequests()
                {
                    requests = new List<requests>()
                {
                     new requests()
                {
                     image = new image()
                     {
                     content = base64String
                 },
                 features = new List<features>()
                 {
                     new features()
                     {
                         type = "SAFE_SEARCH_DETECTION",
                     }
                 }
             }
             }
                };
                client.Headers.Add("Content-Type:application/json");
                client.Headers.Add("Accept:application/json");
                var resultjson = client.UploadString(path, JsonConvert.SerializeObject(Mainrequests));
                Root res = JsonConvert.DeserializeObject<Root>(resultjson);
                if (res != null && res.responses.Any())
                {
                    result = res.responses.Any(x => !((x.safeSearchAnnotation.adult == "UNLIKELY" || x.safeSearchAnnotation.adult == "VERY_UNLIKELY")
                    && (x.safeSearchAnnotation.violence == "UNLIKELY" || x.safeSearchAnnotation.violence == "VERY_UNLIKELY")
                    && (x.safeSearchAnnotation.racy == "UNLIKELY" || x.safeSearchAnnotation.racy == "VERY_UNLIKELY" || x.safeSearchAnnotation.racy == "POSSIBLE")
                    //&& (x.safeSearchAnnotation.violence == "UNLIKELY" || x.safeSearchAnnotation.violence == "VERY_UNLIKELY")
                    //&& (x.safeSearchAnnotation.s`poof == "UNLIKELY" || x.safeSearchAnnotation.spoof == "VERY_UNLIKELY")
                    && (x.safeSearchAnnotation.medical == "UNLIKELY" || x.safeSearchAnnotation.medical == "VERY_UNLIKELY")));
                }
                return result;
            }
        }

        #endregion

        #region Get Active all sub sub category


        [Route("ActiveSubSub")]
        [HttpGet]
        public async Task<List<SubsubCategoryDTOM>> GetactiveSubSub()
        {
            logger.Info("start Subsubategory: ");
            List<SubsubCategoryDTOM> result = new List<SubsubCategoryDTOM>();
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
            if (SubCatId > 0)
            {
                using (var db = new AuthContext())
                {
                    string query = "select distinct e.CategoryName, d.SubcategoryName, a.SubsubcategoryName,  b.BrandCategoryMappingId SubsubCategoryid, a.LogoUrl" +
                                  " from SubsubCategories a inner join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId " +
                                  " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId inner " +
                                  " join SubCategories d on d.SubCategoryId = c.SubCategoryId inner join Categories e on e.Categoryid = c.Categoryid" +
                                  " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0 and d.SubCategoryId =" + SubCatId;
                    result = await db.Database.SqlQuery<SubsubCategoryDTOM>(query).ToListAsync();
                }
            }

            return result;

        }
        #endregion
        public PublishAppHome GetPublisheddataSeller(string appType, int wId, int subCategoryId = 0)
        {
            using (var context = new AuthContext())
            {
                var publishedData = context.PublishAppHomeDB.Where(x => x.SubCategoryID == subCategoryId && x.WarehouseID == wId && x.AppType == appType && x.Deleted == false).FirstOrDefault();
                return publishedData;
            }
        }

        [Route("GetItemMOQ")]
        [HttpGet]
        public HttpResponseMessage GetItemMOQ(int ItemId)
        {
            using (AuthContext db = new AuthContext())
            {
                ItemMaster ItemNumber = db.itemMasters.Where(x => x.ItemId == ItemId).SingleOrDefault();
                var Item = db.itemMasters.Where(x => x.Number == ItemNumber.Number && x.WarehouseId == ItemNumber.WarehouseId && x.Deleted == false && x.IsDisContinued == false && x.Deleted == false).Select(x => new
                {
                    x.ItemId,
                    x.itemname,
                    x.ItemMultiMRPId,
                    x.Number,
                    x.LogoUrl,
                    x.MinOrderQty,
                    x.MRP,
                    x.UnitPrice,
                    x.SellingSku,
                    x.active,
                    x.PurchasePrice,
                    x.price
                }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, Item);
            }
        }
    }


    public class AppHomeSectionsres
    {
        public AppHomeSections AppHomeSections { get; set; }
        public bool error { get; set; }
        public string msg { get; set; }

    }
    public class Mainrequests
    {
        public List<requests> requests { get; set; }
    }

    public class requests
    {
        public image image { get; set; }
        public List<features> features { get; set; }
    }

    public class image
    {
        public string content { get; set; }
    }
    public class features
    {
        public string type { get; set; }
    }


    public class SafeSearchAnnotation
    {
        public string adult { get; set; }
        public string spoof { get; set; }
        public string medical { get; set; }
        public string violence { get; set; }
        public string racy { get; set; }
    }

    public class Respons
    {
        public SafeSearchAnnotation safeSearchAnnotation { get; set; }
    }

    public class Root
    {
        public List<Respons> responses { get; set; }
    }


}
//#endregion
