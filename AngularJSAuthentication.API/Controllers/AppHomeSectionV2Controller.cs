using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.FileUpload;
using AngularJSAuthentication.Model;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Hangfire;
using LinqKit;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/AppHomeSectionv2")]
    public class AppHomeSectionv2Controller : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("GetSection")]
        [HttpGet]
        public dynamic GetSections(string appType, int wId)
        {
            using (var context = new AuthContext())
            {

                logger.Info("start GetSections: ");
            
                List<AppHomeSections> sections = new List<AppHomeSections>();
                 List<AppHomeSectionItems> remove = new List<AppHomeSectionItems>();
               
                try
                {
                    var datenow = indianTime;
                    sections = context.AppHomeSectionsDB.Where(x => x.WarehouseID == wId && x.AppType == appType && x.Deleted == false).Include("AppItemsList").ToList();

                    foreach (var a in sections)
                    {
                        if (a.SectionType == "Banner")
                            foreach (var ap in a.AppItemsList)
                            {
                                if (ap.OfferEndTime < datenow && ap.Deleted == false)
                                {
                                    ap.Expired = true;
                                    //context.AppHomeSectionItemsDB.Attach(ap);
                                    context.Entry(ap).State = EntityState.Modified;
                                    context.Commit();
                                    //remove.Add(ap);
                                }
                            }
                        
                    }

                    var sectionsResult = context.AppHomeSectionsDB.Where(x => x.WarehouseID == wId && x.AppType == appType && x.Deleted == false && x.Active==true).Include(o => o.AppItemsList)
                        .ToList().Select(o => new AppHomeSectionsDc
                        {
                            AppItemsList = o.SectionType == "Banner" || o.SectionType == "PopUp"
                                        ? o.AppItemsList.Where(i => i.Deleted == false && (i.OfferEndTime > datenow || i.OfferEndTime == null)
                                         // && (i.OfferStartTime < datenow || i.OfferStartTime == null)
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
                                              DynamicHeaderImage=z.DynamicHeaderImage,
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
                                            DynamicHeaderImage=z.DynamicHeaderImage,
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
                            IsSingleBackgroundImage = o.IsSingleBackgroundImage,
                            Deleted = o.Deleted,
                            ViewType = o.ViewType,
                            WebViewUrl = o.WebViewUrl,
                            SectionHindiName = o.SectionHindiName,
                            Active =o.Active

                        }).ToList();

                    if (sectionsResult != null && sectionsResult.Any())
                    {
                        var itemIds = sectionsResult.SelectMany(x => x.AppItemsList.Where(z => z.ItemId > 0).Select(z => z.ItemId)).Distinct().ToList();
                        var itemsList = context.itemMasters.Where(x => itemIds.Contains(x.ItemId)).Select(x => new { x.ItemId, x.OfferQtyAvaiable }).ToList();
                      //  var activeItems = new List<AppHomeSectionItemsDc>();

                     /*   sectionsResult.ForEach(section =>
                        {


                            if (section.SectionSubType == "Flash Deal")
                            {
                                if (section.AppItemsList.Count > 0)
                                {
                                    section.AppItemsList.ForEach(appItem =>
                                    {
                                        if (appItem.OfferEndTime < datenow)
                                        {
                                            //appItem.Deleted = true;
                                        }
                                    });
                                }

                                
                            }

                            section.AppItemsList = section.AppItemsList.Where(item => item.Deleted == false).ToList();
                            
                            if(section.AppItemsList.Count == 0)
                            {
                               // section.Deleted = true;
                            }
                        }

                        ); */

                        sectionsResult.ForEach(x =>
                        x.AppItemsList.Where(z => z.ItemId > 0 && z.Deleted == false).ToList().ForEach(y =>
                        {
                            if (y.ItemId > 0)
                            {
                                var item = itemsList.FirstOrDefault(a => a.ItemId == y.ItemId);
                                y.FlashdealRemainingQty = item.OfferQtyAvaiable;
                            }
                        }));



                    }
                    sectionsResult.ForEach(x =>
                     x.AppItemsList=x.AppItemsList.Where(z => z.Active == true && z.Deleted == false).ToList()
                     );


                    logger.Info("End GetSections: ");
                    return sectionsResult.OrderBy(x => x.Sequence).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in GetSections " + ex.Message);
                    logger.Info("End GetSections: ");
                    return null;
                }
            }
        }


        [Route("GetConsumerWarehouse")]
        [HttpGet]
        public HttpResponseMessage GetConsumerWarehouse(int storeType)
        {
            using (var context = new AuthContext())
            {
                WarehouseDetailObj warehouses = new WarehouseDetailObj();
                logger.Info("start GetConsumerWarehouse: ");
                var  data = context.Warehouses.Where(x => x.StoreType == 1 && x.active== true && x.Deleted == false).ToList();
                if (data.Count() > 0)
                {
                    warehouses = new WarehouseDetailObj()
                    {
                        Warehouses = data,
                        Status = true,
                        Message = "Warehouse Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, warehouses);
                }
                else
                {

                    warehouses = new WarehouseDetailObj()
                    {
                        Warehouses = data,
                        Status = false,
                        Message = "Warehouse Not Found"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, warehouses);
                }
            }
        }
        



        [Route("AddSection")]
        [AcceptVerbs("POST")]
        public AppHomeSections AddSections(AppHomeSections appsection)
        {    
           using (var context = new AuthContext())
            {
                var userid = GetUserId();
                int count = 0;
                logger.Info("start AppHomeSections: ");
                try
                {
                    if (appsection == null)
                    {
                        throw new ArgumentNullException("state");
                    }

                    var ar = context.AppHomeSectionsDB.Where(x => x.SectionID == appsection.SectionID && x.Deleted == false).Include(x => x.AppItemsList).FirstOrDefault();
                    if (appsection.AppItemsList != null)
                    {

                        var existingAppItems = appsection.AppItemsList.Where(x => x.Deleted == false).ToList();
                        var notexistingAppItems = appsection.AppItemsList.Where(x => x.Deleted == true).ToList();
                        //= appsection.AppItemsList; // context.AppHomeSectionItemsDB.Where(x => x.apphomesections.SectionID == ar.SectionID).ToList();
                        if (existingAppItems.Count > appsection.AppItemsList.Count )
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
                        if (notexistingAppItems.Count > 0)
                        {
                            //for (var i = 0; i < notexistingAppItems.Count; i++)
                            //{
                            notexistingAppItems.ForEach(x => { 
                                x.Deleted = true;
                                x.Active = false;
                                x.UpdatedDate = indianTime;
                                context.Entry(x).State = EntityState.Modified;                                
                            });
                            context.Commit();
                            //}
                        }
                    }
                    if (appsection.ViewType == "AppView" && appsection.SectionSubType != "Other" && appsection.SectionSubType != "DynamicHtml" && appsection.SectionSubType != "Store")
                    {
                        appsection.WebViewUrl = string.Empty;
                    }

                    if (ar == null)
                    {
                        int sequence = context.AppHomeSectionsDB.Where(x => x.WarehouseID == appsection.WarehouseID && x.Deleted == false && x.AppType == appsection.AppType).Count();
                        appsection.CreatedDate = indianTime;
                        appsection.UpdatedDate = indianTime;
                        appsection.Active = true;
                        appsection.Sequence = sequence + 1;
                        context.AppHomeSectionsDB.Add(appsection);
                        int id = context.Commit();

                        if (appsection.AppItemsList != null)
                        {
                            foreach (var o in appsection.AppItemsList)
                            {


                                var appItem = context.AppHomeSectionItemsDB.Where(x => x.SectionItemID == o.SectionItemID && x.Deleted == false).FirstOrDefault();
                                if (appItem == null)
                                {
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
                                    appItem.RedirectionUrl = o.RedirectionUrl;
                                    appItem.BannerImage = o.BannerImage;
                                    appItem.DynamicHeaderImage = o.DynamicHeaderImage;
                                    appItem.OfferStartTime = o.OfferStartTime;
                                    appItem.OfferEndTime = o.OfferEndTime;
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
                        appsection = context.AppHomeSectionsDB.Where(x => x.SectionID == appsection.SectionID && x.Deleted == false).Include("AppItemsList").FirstOrDefault();
                        return appsection;
                    }
                    else
                    {
                        //ar.AppItemsList = appsection.AppItemsList.Where(x => x.Deleted == false).ToList();
                        ar.SectionName = appsection.SectionName;
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
                        ar.IsSingleBackgroundImage = appsection.IsSingleBackgroundImage;
                        ar.HeaderTextColor = appsection.HeaderTextColor;
                        ar.IsTileSlider = appsection.IsTileSlider;
                        if (ar.HasBackgroundImage == true)
                            ar.TileBackgroundImage = appsection.TileBackgroundImage;
                        else
                            ar.TileBackgroundImage = null;
                        if (ar.HasBackgroundColor == true)
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

                        if (appsection.AppItemsList != null)
                        {
                            foreach (var o in appsection.AppItemsList)
                            {



                                var appItem = context.AppHomeSectionItemsDB.Where(x => x.SectionItemID == o.SectionItemID && x.Deleted == false).FirstOrDefault();
                                if (appItem == null)
                                {

                                    #region Category, BaseCategory,Item,N etc

                                    if (o.OfferEndTime.HasValue && o.OfferEndTime.Value <= indianTime)
                                    {
                                        appItem = new AppHomeSectionItems();
                                        appItem.Active = false;
                                        appItem.Deleted = true;
                                    }

                                    if (appsection.SectionSubType == "Base Category")
                                    {

                                        //     ..............if sectionsubtype!=RedirectionType
                                        
                                        if (o.RedirectionType == "Category")
                                        {
                                            appsection.SectionSubType = "Category";
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

                                        else {
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.BaseCategoryId = o.RedirectionID;
                                            o.Active = true;
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                        }
                                       

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

                                    //else if (appsection.SectionSubType == "Video")
                                    //{
                                    //    if (o.RedirectionType == "Category")
                                    //    {
                                    //        var itemBaseCats = context.Categorys.Where(x => x.Categoryid == o.RedirectionID).Select(i => new { i.BaseCategoryId, i.Categoryid }).ToList();
                                    //        foreach (var Catdata in itemBaseCats)
                                    //        {
                                    //            o.apphomesections = ar;
                                    //            o.CreatedDate = indianTime;
                                    //            o.UpdatedDate = indianTime;
                                    //            o.BaseCategoryId = Catdata.BaseCategoryId;
                                    //            o.CategoryId = Catdata.Categoryid;
                                    //            o.Active = true;
                                    //            context.AppHomeSectionItemsDB.Add(o);
                                    //            context.Commit();
                                    //        }
                                    //    }

                                    //}



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
                                        
                                        var itemBaseCats = context.itemMasters.Where(x => x.WarehouseId == appsection.WarehouseID && x.SellingSku == o.SellingSku && x.MinOrderQty == o.MOQ && x.price == o.MRP).ToList();
                                        foreach (var Catdata in itemBaseCats)
                                        {
                                            if (!o.Deleted)
                                            {
                                                bool IsExistresult = context.AppHomeSectionItemsDB.Any(x => x.ItemId == Catdata.ItemId && x.IsFlashDeal == true && x.Active == true
                                                          && x.Deleted == false && (x.OfferEndTime.HasValue && x.OfferEndTime.Value >= o.OfferStartTime) && (x.OfferStartTime.HasValue && x.OfferStartTime.Value <= o.OfferStartTime));
                                                if (IsExistresult)
                                                {

                                                    return null;

                                                }
                                            }
                                            o.apphomesections = ar;
                                            o.CreatedDate = indianTime;
                                            o.UpdatedDate = indianTime;
                                            o.ItemId = Catdata.ItemId;
                                            o.BaseCategoryId = Catdata.BaseCategoryid;
                                            o.CategoryId = Catdata.Categoryid;
                                            o.SubCategoryId = Catdata.SubCategoryId;
                                            o.SubsubCategoryId = Catdata.SubsubCategoryid;
                                            o.FlashDealMaxQtyPersonCanTake = o.FlashDealMaxQtyPersonCanTake;
                                            o.FlashDealQtyAvaiable = o.FlashDealQtyAvaiable;
                                            o.FlashDealSpecialPrice = o.FlashDealSpecialPrice;
                                            o.SellingSku = Catdata.SellingSku;
                                            o.MOQ = Catdata.MinOrderQty;
                                            o.RedirectionType = "Flash Deal";
                                            o.RedirectionID = Catdata.ItemId;
                                            o.IsFlashDeal = true;
                                            o.Active = true;
                                            o.HasOffer = true;
                                            o.UnitPrice = Catdata.UnitPrice;
                                            o.PurchasePrice = Catdata.UnitPrice;                                          
                                            //if (o.HasOffer)
                                            //{
                                            //    o.OfferStartTime = Convert.ToDateTime(o.OfferStartTime);
                                            //    o.OfferEndTime = Convert.ToDateTime(o.OfferStartTime);
                                            //}
                                            //else
                                            //{
                                            //    o.OfferStartTime = null;
                                            //    o.OfferEndTime = null;
                                            //}
                                            //if (o.OfferEndTime.HasValue && o.OfferEndTime.Value <= indianTime)
                                            //{
                                            //    o.Active = false;
                                            //    o.Deleted = true;
                                            //}
                                            o.TileName = Catdata.itemname;
                                            o.TileImage = Catdata.LogoUrl;
                                          
                                            context.AppHomeSectionItemsDB.Add(o);
                                            context.Commit();
                                            // ItemMaster itemMaster = context.itemMasters.Where(x => x.ItemId == Catdata.ItemId && x.WarehouseId == appsection.WarehouseID && x.active == true && x.Deleted == false && x.SellingSku == o.SellingSku).FirstOrDefault();
                                            if (Catdata != null)
                                            {                                                
                                                Catdata.OfferCategory = 2;
                                                Catdata.OfferQtyAvaiable = o.FlashDealQtyAvaiable;
                                                Catdata.OfferMaxQtyPersonCanTake = o.FlashDealMaxQtyPersonCanTake;
                                                Catdata.IsOffer = true;
                                                Catdata.OfferType = "FlashDeal";
                                                Catdata.FlashDealSpecialPrice = o.FlashDealSpecialPrice;
                                                Catdata.OfferStartTime = Convert.ToDateTime(o.OfferStartTime);
                                                Catdata.OfferEndTime = Convert.ToDateTime(o.OfferEndTime);
                                                //db.itemMasters.Attach(itemMaster);
                                                context.Entry(Catdata).State = EntityState.Modified;
                                                context.Commit();
                                            }
                                        }
                                    }
                                    else if (appsection.SectionSubType == "Slider")
                                    {

                                           if (o.RedirectionType == "SubCategory")
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
                                        //else if (o.RedirectionType == "Base Category")
                                        //{
                                        //    o.apphomesections = ar;
                                        //    o.CreatedDate = indianTime;
                                        //    o.UpdatedDate = indianTime;
                                        //    o.BaseCategoryId = o.RedirectionID;
                                        //    o.Active = true;
                                        //    context.AppHomeSectionItemsDB.Add(o);
                                        //    context.Commit();

                                        //}
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
                                        //video
                                        




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

                                    appItem.RedirectionType = o.RedirectionType;
                                    appItem.RedirectionID = o.RedirectionID;
                                    appItem.Active = o.Active;
                                    appItem.ImageLevel = o.ImageLevel;
                                    appItem.HasOffer = o.HasOffer;
                                    appItem.BannerActivity = o.BannerActivity;
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
                                    if (o.OfferEndTime.HasValue && o.OfferEndTime.Value <= indianTime)
                                    {
                                        appItem.Active = false;
                                        appItem.Deleted = true;
                                    }
                                    appItem.TileName = o.TileName;
                                    appItem.TileImage = o.TileImage;
                                    appItem.BannerName = o.BannerName;
                                    appItem.RedirectionUrl = o.RedirectionUrl;
                                    appItem.BannerImage = o.BannerImage;
                                    appItem.DynamicHeaderImage = o.DynamicHeaderImage;

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
                                            //appItem.BaseCategoryId = itemBaseCat.BaseCategoryId.Value;
                                            //appItem.CategoryId = itemBaseCat.Categoryid;
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
                                            appsection.Active = true;
                                            //context.AppHomeSectionItemsDB.Add(o);
                                            //context.Commit();
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
                                            if (!o.Deleted)
                                            {
                                                bool IsExistresult = context.AppHomeSectionItemsDB.Any(x => x.ItemId == Catdata.ItemId && x.SectionItemID != o.SectionItemID && x.IsFlashDeal == true && x.Active == true
                                                          && x.Deleted == false && (x.OfferEndTime.HasValue && x.OfferEndTime.Value >= o.OfferStartTime) && (x.OfferStartTime.HasValue && x.OfferStartTime.Value <= o.OfferStartTime));
                                                if (IsExistresult)
                                                {
                                                    return null;
                                                }
                                            }
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
                                                itemMaster.OfferQtyAvaiable = o.FlashDealQtyAvaiable;
                                                itemMaster.OfferMaxQtyPersonCanTake = o.FlashDealMaxQtyPersonCanTake;
                                                itemMaster.IsOffer = true;
                                                itemMaster.OfferType = "FlashDeal";
                                                itemMaster.FlashDealSpecialPrice = o.FlashDealSpecialPrice;
                                                itemMaster.OfferStartTime = Convert.ToDateTime(o.OfferStartTime);
                                                itemMaster.OfferEndTime = Convert.ToDateTime(o.OfferEndTime);
                                                //db.itemMasters.Attach(itemMaster);
                                                context.Entry(itemMaster).State = EntityState.Modified;
                                                context.SaveChanges();
                                            }
                                        }
                                    }
                                    else if (appsection.SectionSubType == "Slider")
                                    {
                                        if (o.RedirectionType == "SubCategory")
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
                                        else if (o.RedirectionType == "Item")
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
                                        //else if (o.RedirectionType == "Base Category")
                                        //{
                                        //    appItem.UpdatedDate = indianTime;
                                        //    appItem.BaseCategoryId = o.RedirectionID;
                                        //    appItem.Active = true;
                                        //    appItem.TileSectionBackgroundImage = o.TileSectionBackgroundImage;
                                        //}
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
                                   /* else
                                    {
                                        o.apphomesections = ar;
                                        o.CreatedDate = indianTime;
                                        o.UpdatedDate = indianTime;
                                        o.Active = true;
                                        //context.AppHomeSectionItemsDB.Add(o);
                                        //context.Commit();
                                        *//*context.Entry(o).State = EntityState.Modified;
                                        context.SaveChanges();*//*
                                    }*/
                                    #endregion
                                    //context.AppHomeSectionItemsDB.Attach(appItem);
                                    context.Entry(appItem).State = EntityState.Modified;
                                    context.Commit();
                                }
                            }
                        }
                        appsection = context.AppHomeSectionsDB.Where(x => x.SectionID == appsection.SectionID && x.Deleted == false).Include("AppItemsList").FirstOrDefault();
                        if (appsection != null && appsection.AppItemsList.Any())
                        {
                            var datetimenow = indianTime;
                            var data = appsection.AppItemsList[0].OfferEndTime.HasValue;//|| !X.OfferEndTime.HasValue
                            appsection.AppItemsList = appsection.AppItemsList.Where(X => X.Deleted == false && ((X.OfferEndTime.HasValue && X.OfferEndTime.Value >= indianTime) || !X.OfferEndTime.HasValue)).ToList();
                            foreach (var item in appsection.AppItemsList)
                            {
                                item.apphomesections = null;
                            }
                        }
                        return appsection;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AppHomeSections" + ex.Message);
                    logger.Info("End  Add AppHomeSections: ");
                    return null;
                }
            }
        }


        [Route("AddCompleteAppHome")]
        [AcceptVerbs("POST")]
        public List<AppHomeSections> AddCompleteAppHome(List<AppHomeSections> apphome)
        {
            using (var context = new AuthContext())
            {
                int WarehouseId = apphome[0].WarehouseID;
                string apptype = apphome[0].AppType;
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {



                    try
                    {

                        List<AppHomeSections> Ids = new List<AppHomeSections>();
                        Ids = apphome.Where(x => x.Deleted == true).ToList();
                        int i = 1;
                        Ids.ForEach(x =>
                        {

                            this.DeleteSection(x.SectionID);

                        });

                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    try
                    {
                        string jsonData = JsonConvert.SerializeObject(apphome);
                        var bsObj = JsonConvert.DeserializeObject<List<AppHomeSections>>(jsonData);

                        List<int> Ids = new List<int>();
                        Ids = apphome.Select(x => x.SectionID).ToList();

                        //var upadateappitem = new List<AppHomeSectionItems>();
                        //var upadateAppHomeSections = new List<AppHomeSections>();

                        var APPH = context.AppHomeSectionsDB.Where(x => Ids.Contains(x.SectionID) && x.Deleted == false).ToList();
                        int i = 1;
                        apphome.ForEach(x => { x.Sequence = i; i++; });
                        APPH.ForEach(x =>
                        {
                            x.Sequence = apphome.Any(y => y.SectionID == x.SectionID) ? apphome.FirstOrDefault(y => y.SectionID == x.SectionID).Sequence : 0;
                            x.WebViewUrl = apphome.Any(y => y.SectionID == x.SectionID) ? apphome.FirstOrDefault(y => y.SectionID == x.SectionID).WebViewUrl : "";

                            context.Entry(x).State = EntityState.Modified;
                        });
                        context.Commit();

                        /* List<AppHomeSections> appSectionsList = new List<AppHomeSections>();
                        appSectionsList = apphome;
                        appSectionsList.ForEach(x =>
                        {
                            if (x.Deleted != true || x.Deleted == false)
                            {
                                this.AddSections(x);
                            }
                        }); */


                        //context.Commit();

                        //foreach (var a in apphome)
                        //{
                        //    var ara = APPH.Where(x => x.SectionID == a.SectionID && x.Deleted == false).FirstOrDefault();

                        //    if (ara != null)
                        //    {
                        //        var appitem = context.AppHomeSectionItemsDB.Where(x => x.apphomesections.SectionID == ara.SectionID && x.Deleted == false).ToList();
                        //        if (appitem.Count > 0)
                        //        {
                        //            foreach (var o in appitem)
                        //            {
                        //                o.Deleted = true;
                        //                o.Active = false;
                        //                o.UpdatedDate = indianTime;
                        //                //context.Entry(o).State = EntityState.Modified;
                        //                //context.SaveChanges();
                        //                upadateappitem.Add(o);
                        //            }
                        //        }
                        //        ara.Deleted = true;
                        //        ara.Active = false;
                        //        ara.UpdatedDate = indianTime;
                        //        //context.Entry(ara).State = EntityState.Modified;
                        //        // context.SaveChanges();
                        //        upadateAppHomeSections.Add(ara);
                        //    }

                        //}



                        //foreach (var item in upadateAppHomeSections)
                        //{
                        //    context.Entry(item).State = EntityState.Modified;
                        //}
                        //foreach (var fitem in upadateappitem)
                        //{
                        //    context.Entry(fitem).State = EntityState.Modified;
                        //}
                        //context.SaveChanges();
                        //var APPHH = context.AppHomeSectionsDB.Where(x => Ids.Contains(x.SectionID) && !x.Deleted).ToList();






                        //foreach (var a in apphome)
                        //    {
                        //        var ar = APPHH.Where(x => x.SectionID == a.SectionID && !x.Deleted).FirstOrDefault();

                        //        if (ar == null)
                        //        {

                        //            if (a.AppItemsList != null)
                        //            {
                        //                foreach (var o in a.AppItemsList)
                        //                {
                        //                    var appItem = context.AppHomeSectionItemsDB.Where(x => x.SectionItemID == o.SectionItemID && x.Deleted == false).FirstOrDefault();
                        //                    if (appItem == null)
                        //                    {
                        //                        o.SectionItemID = 0;
                        //                        o.apphomesections = null;
                        //                        o.CreatedDate = indianTime;
                        //                        o.UpdatedDate = indianTime;
                        //                    }
                        //                    else
                        //                    {

                        //                    }
                        //                }
                        //            }

                        //            if (a.HasBackgroundImage == true)
                        //                a.TileBackgroundImage = a.TileBackgroundImage;
                        //            else
                        //                a.TileBackgroundImage = null;

                        //            if (a.HasBackgroundColor == true)
                        //                a.TileBackgroundColor = a.TileBackgroundColor;
                        //            else
                        //                a.TileBackgroundColor = null;
                        //            if (a.HasBackgroundColor == true)
                        //                a.BannerBackgroundColor = a.BannerBackgroundColor;
                        //            else
                        //                a.BannerBackgroundColor = null;

                        //            if (a.HasHeaderBackgroundImage == true)
                        //                a.TileHeaderBackgroundImage = a.TileHeaderBackgroundImage;
                        //            else
                        //                a.TileHeaderBackgroundImage = null;
                        //            if (a.HasHeaderBackgroundColor == true)
                        //                a.TileHeaderBackgroundColor = a.TileHeaderBackgroundColor;
                        //            else
                        //                a.TileHeaderBackgroundColor = null;


                        //            a.CreatedDate = indianTime;
                        //            a.UpdatedDate = indianTime;
                        //            a.Deleted = false;
                        //            a.Active = true;
                        //           context.AppHomeSectionsDB.Add(a);
                        //           context.SaveChanges();



                        //        }
                        //    }



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
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return null;
                    }
                }
            }
        }

        //Clone App Home to another App Type
        [Route("CloneAppHome")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CloneAppHome([FromBody]JObject data)
        {
            string appTypefrom = data["appTypefrom"].ToString();
            int wIdfrom = Convert.ToInt32(data["wIdfrom"]);
            string appType = data["appType"].ToString();
            int wId = Convert.ToInt32(data["wId"]);

            //string appTypefrom, int wIdfrom, string appType, int wId

            using (var context = new AuthContext())
            {
                try
                {
                    //var apphomeDelete = GetSections(appType, wId) as List<AppHomeSections>; 
                    //var apphome = GetSections(appTypefrom, wIdfrom) as List<AppHomeSections>; 
                    var apphomeDelete = context.AppHomeSectionsDB.Where(x => x.AppType == appType && x.WarehouseID == wId && x.Deleted == false).Include(x => x.AppItemsList).Where(x => x.WarehouseID == wId && x.AppType == appType && x.Deleted == false).ToList();
                    var apphome = context.AppHomeSectionsDB.Where(x => x.AppType == appTypefrom && x.WarehouseID == wIdfrom && x.Deleted == false).Include(x => x.AppItemsList).Where(x => x.WarehouseID == wIdfrom && x.AppType == appTypefrom && x.Deleted == false).ToList();

                    var apphomeClone = apphome;
                    foreach (var a in apphomeDelete)
                    {
                        var ara = context.AppHomeSectionsDB.Where(x => x.SectionID == a.SectionID).FirstOrDefault();

                        if (ara != null)
                        {
                            var appitem = context.AppHomeSectionItemsDB.Where(x => x.apphomesections.SectionID == ara.SectionID).ToList();
                            if (appitem.Count > 0)
                            {
                                foreach (var o in appitem)
                                {
                                    o.Deleted = true;
                                    context.Entry(o).State = EntityState.Modified;
                                    context.Commit();
                                    //context.Entry(o).State = EntityState.Deleted;
                                    //context.AppHomeSectionItemsDB.Remove(o);

                                }
                            }
                            //context.AppHomeSectionsDB.Remove(ara);
                            //context.Entry(ara).State = EntityState.Deleted;
                            ara.Deleted = true;
                            context.Entry(ara).State = EntityState.Modified;
                            context.Commit();
                        }
                    }

                    AppHomeSections clonedAppHome = new AppHomeSections();

                    List<AppHomeSections> clonedAppHomelst = new List<AppHomeSections>();
                    List<AppHomeSectionItems> appItemlst = new List<AppHomeSectionItems>();
                    foreach (var a in apphome)
                    {
                        a.AppType = appType;
                        a.WarehouseID = wId;
                        a.CreatedDate = indianTime;
                        a.UpdatedDate = indianTime;
                        clonedAppHome = a;
                        if (a.AppItemsList.Count > 0 || a.AppItemsList != null)
                        {
                            foreach (var o in a.AppItemsList)
                            {
                                AppHomeSectionItems appItem = new AppHomeSectionItems();
                                appItem.ItemId = o.ItemId;
                                appItem.BaseCategoryId = o.BaseCategoryId;
                                appItem.CategoryId = o.CategoryId;
                                appItem.SubCategoryId = o.SubCategoryId;
                                appItem.SubsubCategoryId = o.SubsubCategoryId;
                                appItem.IsFlashDeal = o.IsFlashDeal;
                                appItem.FlashDealMaxQtyPersonCanTake = o.FlashDealMaxQtyPersonCanTake;
                                appItem.FlashDealQtyAvaiable = o.FlashDealQtyAvaiable;
                                appItem.FlashDealSpecialPrice = o.FlashDealSpecialPrice;
                                appItem.RedirectionType = o.RedirectionType;
                                appItem.RedirectionID = o.RedirectionID;
                                appItem.Active = o.Active;
                                appItem.ImageLevel = o.ImageLevel;
                                appItem.HasOffer = o.HasOffer;
                                appItem.DynamicHeaderImage = o.DynamicHeaderImage;
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
                                appItem.TileName = o.TileName;
                                appItem.TileImage = o.TileImage;
                                appItem.BannerName = o.BannerName;
                                appItem.RedirectionUrl = o.RedirectionUrl;
                                appItem.BannerImage = o.BannerImage;
                                o.CreatedDate = indianTime;
                                o.UpdatedDate = indianTime;
                                appItemlst.Add(appItem);

                            }
                            clonedAppHome.AppItemsList = appItemlst;
                            appItemlst = new List<AppHomeSectionItems>();

                        }

                        clonedAppHomelst.Add(clonedAppHome);
                        clonedAppHome = new AppHomeSections();
                    }

                    context.AppHomeSectionsDB.AddRange(clonedAppHomelst);
                    context.Commit();


                    return Request.CreateResponse(HttpStatusCode.OK, "Cloned");
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
                }
            }
        }


        [Route("DeleteAppHome")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage DeleteAppHome(List<AppHomeSections> apphome)
        {
            using (var context = new AuthContext())
            {
                try
                {

                    foreach (var a in apphome)
                    {
                        var publishdata = context.PublishAppHomeDB.Where(x => x.AppType == a.AppType && x.WarehouseID == a.WarehouseID).FirstOrDefault();

                        var ara = context.AppHomeSectionsDB.Where(x => x.SectionID == a.SectionID && a.Deleted == false).FirstOrDefault();
                        if (ara != null)
                        {
                            var appitem = context.AppHomeSectionItemsDB.Where(x => x.apphomesections.SectionID == ara.SectionID && ara.Deleted == false).ToList();
                            if (appitem.Count > 0)
                            {
                                foreach (var o in appitem)
                                {
                                    o.Deleted = true;
                                    o.UpdatedDate = DateTime.Now;
                                    //context.AppHomeSectionItemsDB.Attach(o);
                                    context.Entry(o).State = EntityState.Modified;
                                    context.Commit();
                                }
                            }
                            if (ara.SectionSubType == "Flash Deal")
                            {
                                var itemdata = context.AppHomeSectionItemsDB.Where(x => x.apphomesections.SectionID == ara.SectionID && ara.Deleted == false).ToList();
                                if (itemdata.Count > 0)
                                {
                                    foreach (var o in itemdata)
                                    {
                                        ItemMaster iteminfo = context.itemMasters.Where(x => x.ItemId == o.ItemId).FirstOrDefault();
                                        iteminfo.IsOffer = false;
                                        context.Entry(iteminfo).State = EntityState.Modified;
                                        context.Commit();

                                    }
                                }

                            }
                            ara.Deleted = true;
                            ara.UpdatedDate = DateTime.Now;
                            //context.AppHomeSectionsDB.Attach(ara);
                            context.Entry(ara).State = EntityState.Modified;
                            context.Commit();


                        }
                        else
                        {

                        }
                        if (publishdata != null)
                        {
                            publishdata.Deleted = true;
                            context.Entry(publishdata).State = EntityState.Modified;
                            context.Commit();
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, " App Home Deleted");
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
                }
            }
        }


        [Route("DeleteItem")]
        [AcceptVerbs("Delete")]
        public bool DeleteItem(int SectionID, int SectionItemID)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var ara = context.AppHomeSectionItemsDB.Where(x => x.apphomesections.SectionID == SectionID && x.SectionItemID == SectionItemID && x.Deleted == false).FirstOrDefault();
                    var ara2 = context.AppHomeSectionsDB.Where(x => x.SectionID == SectionID && x.Deleted == false).FirstOrDefault();
                    if (ara2.SectionSubType == "Flash Deal")
                    {
                        ItemMaster iteminfo = context.itemMasters.Where(x => x.ItemId == ara.ItemId).FirstOrDefault();
                        if (iteminfo != null)
                        {
                            iteminfo.IsOffer = false;
                            iteminfo.OfferCategory = 0;
                            iteminfo.UpdatedDate = indianTime;
                            context.Entry(iteminfo).State = EntityState.Modified;
                        }
                    }



                    if (ara != null)
                    {
                        ara.Active = false;
                        ara.Deleted = true;
                        ara.UpdatedDate = DateTime.Now;
                        context.Entry(ara).State = EntityState.Modified;
                        context.Commit();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        [Route("DeleteSection")]
        [AcceptVerbs("Delete")]
        public bool DeleteSection(int SectionID)
        {
            using (var context = new AuthContext())
            {

                var ara = context.AppHomeSectionsDB.Where(x => x.SectionID == SectionID && x.Deleted == false).FirstOrDefault();
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
                            o.UpdatedDate = DateTime.Now;
                            context.Entry(o).State = EntityState.Modified;

                            if (ara.SectionSubType == "Flash Deal")
                            {
                                ItemMaster iteminfo = context.itemMasters.Where(x => x.ItemId == o.ItemId).FirstOrDefault();
                                if (iteminfo != null)
                                {
                                    iteminfo.IsOffer = false;
                                    iteminfo.OfferCategory = 0;
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
        [Route("ExpireFlashDeal")]
        [HttpGet]
        [AllowAnonymous]
        public bool ManulaExpireFlasDeal(int SectionItemID)
        {
            AppHomeSectionItems appHomeSectionitem = new AppHomeSectionItems { SectionItemID = SectionItemID };
            return ExpireFlashDeal(appHomeSectionitem);
        }

        //Publish App Home Section Live
        [Route("PublishAppHome")]
        [AcceptVerbs("POST")]
        public List<AppHomeSections> PublishAppHome(List<AppHomeSections> apphome)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    PublishAppHome publishData = new PublishAppHome();
                    string jsonData = JsonConvert.SerializeObject(apphome);
                    //var bsObj = JsonConvert.DeserializeObject<List<AppHomeSections>>(jsonData);
                    bool result = false;

                    foreach (var a in apphome)
                    {
                        var ara = context.PublishAppHomeDB.Where(x => x.AppType == a.AppType && x.WarehouseID == a.WarehouseID).FirstOrDefault();
                        publishData.language = "en";
                        publishData.AppType = a.AppType;
                        publishData.WarehouseID = a.WarehouseID;
                        publishData.ApphomeSection = jsonData;

                        if (ara != null)
                        {
                            ara.AppType = a.AppType;
                            ara.WarehouseID = a.WarehouseID;
                            ara.ApphomeSection = jsonData;
                            ara.UpdatedDate = indianTime;
                            ara.Deleted = false;
                            context.PublishAppHomeDB.Attach(ara);
                            context.Entry(ara).State = EntityState.Modified;
                            result = context.Commit() > 0;
                        }
                        else
                        {
                            publishData.CreatedDate = indianTime;
                            publishData.UpdatedDate = indianTime;
                            context.PublishAppHomeDB.Add(publishData);
                            result = context.Commit() > 0;
                        }
                        break;
                    }
                    //Add scheduler to fales flash deal
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
#if !DEBUG
                        Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                        string apptype = apphome.FirstOrDefault().AppType.Replace(" ", "");
                        _cacheProvider.Remove(Caching.CacheKeyHelper.APPHomeCacheKey(apptype, apphome.FirstOrDefault().WarehouseID.ToString()));
                        var apphomeresult = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.APPHomeCacheKey(apphome.FirstOrDefault().AppType, apphome.FirstOrDefault().WarehouseID.ToString()), () => publishData);
#endif


                        //MongoDbHelper<PublishAppHomeMongo> mongoDbHelper = new MongoDbHelper<PublishAppHomeMongo>();
                        //var appHomePredicate = PredicateBuilder.New<PublishAppHomeMongo>(x => x.AppType == apphome.FirstOrDefault().AppType && x.WarehouseID == apphome.FirstOrDefault().WarehouseID && !x.Deleted);
                        //var publishAppHomeMongo = mongoDbHelper.Select(appHomePredicate).FirstOrDefault();
                        //if (publishAppHomeMongo != null)
                        //{
                        //    publishAppHomeMongo.UpdatedDate = indianTime;
                        //    publishAppHomeMongo.ApphomeSection = jsonData;
                        //    result = mongoDbHelper.ReplaceWithoutFind(publishAppHomeMongo.Id, publishAppHomeMongo);
                        //}
                        //else
                        //{
                        //    publishAppHomeMongo = new PublishAppHomeMongo
                        //    {
                        //        ApphomeSection = jsonData,
                        //        AppType = publishData.AppType,
                        //        CreatedDate = indianTime,
                        //        Deleted = false,
                        //        language = "en",
                        //        WarehouseID = publishData.WarehouseID,
                        //        PublishID = publishData.PublishID
                        //    };

                        //    result = mongoDbHelper.Insert(publishAppHomeMongo);
                        //}
                    }


                    return apphome;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public bool ExpireFlashDeal(AppHomeSectionItems appHomeSectionitem)
        {

            //using (var context = new AuthContext())
            //{
            //    var appHomeitem = context.AppHomeSectionItemsDB.Where(x => x.SectionItemID == appHomeSectionitem.SectionItemID).Include(x => x.apphomesections).FirstOrDefault();


            //    if (appHomeitem != null)
            //    {
            //        appHomeitem.Expired = false;
            //        appHomeitem.Active = false;
            //        appHomeitem.Deleted = true;
            //        appHomeitem.UpdatedDate = DateTime.Now;
            //        context.Entry(appHomeitem).State = EntityState.Modified;
            //        context.Commit();
            //        if (appHomeitem.apphomesections != null)
            //        {
            //            var apphome = context.AppHomeSectionsDB.Where(x => x.SectionID == appHomeitem.apphomesections.SectionID).Include("AppItemsList").FirstOrDefault();
            //            if (apphome.AppItemsList.All(x => !x.Active && x.Deleted))
            //            {
            //                apphome.Active = false;
            //                apphome.Deleted = true;
            //                apphome.UpdatedDate = DateTime.Now;
            //                context.Entry(apphome).State = EntityState.Modified;
            //            }
            //        }

            //        var itemmaster = context.itemMasters.FirstOrDefault(x => x.ItemId == appHomeSectionitem.ItemId);
            //        if (itemmaster != null)
            //        {
            //            itemmaster.IsOffer = false;
            //            itemmaster.OfferCategory = 0;
            //            itemmaster.UpdatedDate = DateTime.Now;
            //            context.Entry(itemmaster).State = EntityState.Modified;
            //        }
            //    }
            //    context.Commit();
            //}

            //return true;



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
                        Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                        _cacheProvider.Remove(Caching.CacheKeyHelper.APPHomeCacheKey(apptype, appHomeitem.apphomesections.WarehouseID.ToString()));
                        _cacheProvider.Set(Caching.CacheKeyHelper.APPHomeCacheKey(apptype, appHomeitem.apphomesections.WarehouseID.ToString()), publishedData);
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


        // 2022 code /* simran*/


        string msgitemname, msg1;
        //string msg1;
        string strJSON = null;
        string col0, col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15;
        [Authorize]
        [HttpPost]
        public IHttpActionResult UploadFile(int SectionId)
        {

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {

                var formData1 = HttpContext.Current.Request.Form["compid"];
                logger.Info("start Transfer Order Upload Exel File: ");

                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;


                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                // Get the uploaded image from the Files collection
                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                    // Validate the uploaded image(optional)
                    httpPostedFile.SaveAs(FileUrl);

                    byte[] buffer = new byte[httpPostedFile.ContentLength];

                    using (BinaryReader br = new BinaryReader(File.OpenRead(FileUrl)))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;
                    //   XSSFWorkbook workbook1;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                    }

                    var uploader = new List<Uploader> { new Uploader() };
                    uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                    uploader.FirstOrDefault().RelativePath = "~/UploadedFiles";


                    uploader.FirstOrDefault().SaveFileURL = FileUrl;

                    uploader = AsyncContext.Run(() => FileUploadHelper.UploadFileToOtherApi(uploader));


                    return ReadFlashDealUploadFile(hssfwb, userid, SectionId);


                }
            }

            return Created("Error", "Error");
        }

        public IHttpActionResult ReadFlashDealUploadFile(XSSFWorkbook hssfwb, int userid, int SectionId)
        {
            string sSheetName = hssfwb.GetSheetName(0);
            ISheet sheet = hssfwb.GetSheet(sSheetName);

            IRow rowData;
            ICell cellData = null;
            List<FlashDealUploadDc> flashdeallist = new List<FlashDealUploadDc>();
            int? SellingSku = null;
            int? OfferStartTime = null;
            int? OfferEndTime = null;
            int? FlashDealQtyAvaiable = 0;
            int? FlashDealMaxQtyPersonCanTake = 0;
            int? FlashDealSpecialPrice = 0;
            double? MRP = 0;

            for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
            {
                if (iRowIdx == 0)
                {
                    rowData = sheet.GetRow(iRowIdx);

                    if (rowData != null)
                    {


                        SellingSku = rowData.Cells.Any(x => x.ToString().Trim() == "SellingSku") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SellingSku").ColumnIndex : (int?)null;
                        if (!SellingSku.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("SellingSku does not exist..try again");
                            return Created(strJSON, strJSON);
                        }

                        MRP = rowData.Cells.Any(x => x.ToString().Trim() == "MRP") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "MRP").ColumnIndex : (double?)null;
                        if (!SellingSku.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("MRP does not exist..try again");
                            return Created(strJSON, strJSON);
                        }
                        OfferStartTime = rowData.Cells.Any(x => x.ToString().Trim() == "OfferStartTime") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "OfferStartTime").ColumnIndex : (int?)null;
                        if (!OfferStartTime.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("OfferStartTime does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }

                        OfferEndTime = rowData.Cells.Any(x => x.ToString().Trim() == "OfferEndTime") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "OfferEndTime").ColumnIndex : (int?)null;
                        if (!OfferEndTime.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Offer EndTime does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }


                        FlashDealQtyAvaiable = rowData.Cells.Any(x => x.ToString().Trim() == "FlashDealQtyAvaiable") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "FlashDealQtyAvaiable").ColumnIndex : (int?)null;
                        if (!FlashDealQtyAvaiable.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealQtyAvaiable does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }

                        FlashDealMaxQtyPersonCanTake = rowData.Cells.Any(x => x.ToString().Trim() == "FlashDealMaxQtyPersonCanTake") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "FlashDealMaxQtyPersonCanTake").ColumnIndex : (int?)null;
                        if (!FlashDealMaxQtyPersonCanTake.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealMaxQtyPersonCanTake does not exist..try again");
                            return Created(strJSON, strJSON); ;
                        }


                        FlashDealSpecialPrice = rowData.Cells.Any(x => x.ToString().Trim() == "FlashDealSpecialPrice") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "FlashDealSpecialPrice").ColumnIndex : (int?)null;
                        if (!FlashDealSpecialPrice.HasValue)
                        {
                            JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("FlashDealSpecialPrice NO does not exist..try again");
                            return Created(strJSON, strJSON);
                        }


                    }
                }
                else
                {
                    rowData = sheet.GetRow(iRowIdx);
                    DateTime datetoday = DateTime.Today;
                    cellData = rowData.GetCell(0);
                    rowData = sheet.GetRow(iRowIdx);
                    if (rowData != null)
                    {
                        FlashDealUploadDc exceluplaod = new FlashDealUploadDc();

                        cellData = rowData.GetCell(0);
                        col0 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.SellingSku = Convert.ToString(col0);

                        cellData = rowData.GetCell(1);
                        col1 = cellData == null ? "" : cellData.ToString();

                        DateTime? dtst = DateTimeHelper.ConvertToDateTime(col1);
                        if (dtst.HasValue)
                            exceluplaod.OfferStartTime = dtst;//Convert.ToDateTime(col3);//Convert.ToDateTime(col1);

                        if (exceluplaod.OfferStartTime < datetoday)
                        {
                            continue;

                        }


                        cellData = rowData.GetCell(2);
                        col2 = cellData == null ? "" : cellData.ToString();

                        DateTime? dtendt = DateTimeHelper.ConvertToDateTime(col2);
                        if (dtendt.HasValue)
                            exceluplaod.OfferEndTime = dtendt;//Convert.ToDateTime(col3);//Convert.ToDateTime(col1);
                                                              //exceluplaod.OfferEndTime = DateTime.Parse(col2);
                        if (exceluplaod.OfferEndTime < datetoday)
                        {
                            continue;

                        }


                        cellData = rowData.GetCell(3);
                        col3 = cellData == null ? "" : cellData.ToString();

                        exceluplaod.FlashDealQtyAvaiable = Convert.ToInt32(col3); ;


                        cellData = rowData.GetCell(4);
                        col4 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.FlashDealMaxQtyPersonCanTake = Convert.ToInt32(col4);


                        cellData = rowData.GetCell(5);
                        col5 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.FlashDealSpecialPrice = Convert.ToDouble(col5);


                        cellData = rowData.GetCell(6);
                        col6 = cellData == null ? "" : cellData.ToString();
                        exceluplaod.MRP = Convert.ToDouble(col6);

                        exceluplaod.userid = userid;
                        flashdeallist.Add(exceluplaod);


                    }
                }
            }


            ///
           // Validation of item in flashdeal
            //
            List<AppHomeSectionItems> appHomeSectionItems = AddFlashDealUpload(flashdeallist, userid, SectionId);

            if (appHomeSectionItems.Count == flashdeallist.Count)
            {
                if (appHomeSectionItems.Count > 0)
                {
                    return Created("Success", "Success");
                }
                else
                {
                    return Created("Error", "Error");
                }
            }
            else
            {
                List<string> str = new List<string>();
                if (appHomeSectionItems.Count == 0)
                {
                    foreach (var flashdealnot in flashdeallist)
                    {
                        str.Add("Selling sku:-" + flashdealnot.SellingSku + "  " + "and Price:-" + flashdealnot.FlashDealSpecialPrice);
                    }
                }
                else
                {
                    List<string> appHomeSectionItemsstr = new List<string>();
                    foreach (var flashdealsuccessdata in appHomeSectionItems)
                    {
                        appHomeSectionItemsstr.Add(flashdealsuccessdata.SellingSku);
                    }
                    foreach (var flashdealnot in flashdeallist)
                    {
                        if (!appHomeSectionItemsstr.Contains(flashdealnot.SellingSku))
                        {
                            if (!str.Contains(flashdealnot.SellingSku))
                            {
                                str.Add("Selling sku:-" + flashdealnot.SellingSku + "  " + "and Price:" + flashdealnot.FlashDealSpecialPrice);

                            }

                            //msg += flashdealnot.SellingSku + ",";
                        }
                    }


                }

                return Created("Error", str);

            }

        }

        public List<AppHomeSectionItems> AddFlashDealUpload(List<FlashDealUploadDc> Collection, int userId, int SectionId)
        {

            List<FlashDealUploadDc> flashdealList = new List<FlashDealUploadDc>();
            using (var context = new AuthContext())
            {
                List<AppHomeSectionItems> sectionlist = new List<AppHomeSectionItems>();
                List<ItemMaster> itemList = new List<ItemMaster>();
                AppHomeSections ar = context.AppHomeSectionsDB.Where(x => x.SectionID == SectionId && x.SectionSubType == "Flash Deal" && x.Deleted == false && x.Active == true).FirstOrDefault();
                if (Collection.Count > 0)
                {
                    ar.RowCount = Collection.Count;
                    ar.ColumnCount = 1;

                    foreach (var flashdeal in Collection)
                    {
                        AppHomeSectionItems flashDealItemList = new AppHomeSectionItems();
                        //  ItemMaster iteminfo = context.itemMasters.Where(x => x.WarehouseId == ar.WarehouseID && x.SellingSku == flashdeal.SellingSku && x.active == true && x.Deleted == false).FirstOrDefault();
                        ItemMaster iteminfo = context.itemMasters.Where(x => x.WarehouseId == ar.WarehouseID && x.SellingSku == flashdeal.SellingSku && x.Deleted == false && x.MRP == flashdeal.MRP).FirstOrDefault();

                        {

                            if (iteminfo != null)
                            {
                                if (iteminfo.UnitPrice > flashdeal.FlashDealSpecialPrice)
                                {

                                    flashDealItemList.apphomesections = ar;
                                    flashDealItemList.CreatedDate = indianTime;
                                    flashDealItemList.UpdatedDate = indianTime;
                                    flashDealItemList.ItemId = iteminfo.ItemId;
                                    flashDealItemList.BaseCategoryId = iteminfo.BaseCategoryid;
                                    flashDealItemList.CategoryId = iteminfo.Categoryid;
                                    flashDealItemList.SubCategoryId = iteminfo.SubCategoryId;
                                    flashDealItemList.SubsubCategoryId = iteminfo.SubsubCategoryid;
                                    flashDealItemList.FlashDealMaxQtyPersonCanTake = flashdeal.FlashDealMaxQtyPersonCanTake;
                                    flashDealItemList.FlashDealQtyAvaiable = flashdeal.FlashDealQtyAvaiable;
                                    flashDealItemList.FlashDealSpecialPrice = flashdeal.FlashDealSpecialPrice;
                                    flashDealItemList.IsFlashDeal = true;
                                    flashDealItemList.Active = true;
                                    flashDealItemList.RedirectionType = "Flash Deal";
                                    flashDealItemList.RedirectionID = iteminfo.ItemId;
                                    flashDealItemList.ImageLevel = 5;
                                    flashDealItemList.HasOffer = true;
                                    flashDealItemList.MOQ = iteminfo.MinOrderQty;
                                    flashDealItemList.UnitPrice = iteminfo.UnitPrice;
                                    flashDealItemList.PurchasePrice = iteminfo.UnitPrice;
                                    flashDealItemList.SellingSku = iteminfo.SellingSku;
                                    if (flashDealItemList.HasOffer)
                                    {
                                        flashDealItemList.OfferStartTime = flashdeal.OfferStartTime;
                                        flashDealItemList.OfferEndTime = flashdeal.OfferEndTime;
                                    }
                                    else
                                    {
                                        flashDealItemList.OfferStartTime = null;
                                        flashDealItemList.OfferEndTime = null;
                                    }
                                    if (flashdeal.OfferEndTime.HasValue && flashdeal.OfferEndTime.Value <= indianTime)
                                    {
                                        flashDealItemList.Active = false;
                                        flashDealItemList.Deleted = true;
                                    }
                                    flashDealItemList.TileName = iteminfo.itemname;
                                    flashDealItemList.TileImage = iteminfo.LogoUrl;
                                    sectionlist.Add(flashDealItemList);

                                    iteminfo.OfferCategory = 2;
                                    iteminfo.OfferQtyAvaiable = flashdeal.FlashDealQtyAvaiable;
                                    iteminfo.OfferMaxQtyPersonCanTake = flashdeal.FlashDealMaxQtyPersonCanTake;
                                    iteminfo.IsOffer = true;
                                    iteminfo.OfferType = "FlashDeal";
                                    iteminfo.FlashDealSpecialPrice = flashdeal.FlashDealSpecialPrice;
                                    iteminfo.OfferStartTime = (flashdeal.OfferStartTime);
                                    iteminfo.OfferEndTime = (flashdeal.OfferEndTime);
                                    //db.itemMasters.Attach(itemMaster);
                                    //context.Entry(Catdata).State = EntityState.Modified;
                                    itemList.Add(iteminfo);
                                }

                            }
                        }

                    }
                    if (sectionlist.Count == Collection.Count)
                    {
                        context.AppHomeSectionItemsDB.AddRange(sectionlist);

                        foreach (var item in itemList)
                        {
                            context.Entry(item).State = EntityState.Modified;

                        }
                        context.Entry(ar).State = EntityState.Modified;
                        context.Commit();
                    }
                }
                return sectionlist;
            }

        }



        //2022

        [Route("AddImage")]
        [HttpPost]
        public AppHomeSections AddFlashDealcolorImage(FlashDealExcelImageDC flashDealExcelImageDC)
        {
            using (AuthContext context = new AuthContext())
            {

                AppHomeSections apphome = context.AppHomeSectionsDB.Where(x => x.SectionID == flashDealExcelImageDC.SectionId).FirstOrDefault();
                apphome.IsTile = flashDealExcelImageDC.IsTile;
                apphome.HasBackgroundColor = flashDealExcelImageDC.HasBackgroundColor;
                apphome.HasBackgroundImage = flashDealExcelImageDC.HasBackgroundImage;
                apphome.HasHeaderBackgroundColor = flashDealExcelImageDC.HasHeaderBackgroundColor;
                apphome.HasHeaderBackgroundImage = flashDealExcelImageDC.HasHeaderBackgroundImage;
                apphome.TileBackgroundColor = flashDealExcelImageDC.TileBackgroundColor;
                apphome.BannerBackgroundColor = flashDealExcelImageDC.BannerBackgroundColor;
                apphome.HeaderTextColor = flashDealExcelImageDC.HeaderTextColor;
                apphome.sectionBackgroundImage = flashDealExcelImageDC.sectionBackgroundImage;
                apphome.TileAreaHeaderBackgroundImage = flashDealExcelImageDC.TileAreaHeaderBackgroundImage;
                apphome.TileBackgroundImage = flashDealExcelImageDC.TileBackgroundImage;
                apphome.TileHeaderBackgroundColor = flashDealExcelImageDC.TileHeaderBackgroundColor;
                apphome.TileHeaderBackgroundImage = flashDealExcelImageDC.TileHeaderBackgroundImage;
                context.Entry(apphome).State = EntityState.Modified;
                context.Commit();

                return apphome;
            }

        }









        //Get Published Code
        [Route("GetPublishedSection")]
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<AppHomeSections> GetPublishedSection(string appType, int wId, string lang, int CustomerId)
        {
            logger.Info("start GetPublishedSection: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var inActiveCustomer = context.Customers.Any(x => x.CustomerId == CustomerId && (x.Active == false || x.Deleted == true));
                    List<AppHomeSections> sections = new List<AppHomeSections>();
                    var datenow = indianTime;
                    //var publishedData = context.PublishAppHomeDB.Where(x => x.WarehouseID == wId && x.AppType == appType && x.Deleted == false).FirstOrDefault();
                    //MongoDbHelper<PublishAppHomeMongo> mongoDbHelper = new MongoDbHelper<PublishAppHomeMongo>();
                    //var appHomePredicate = PredicateBuilder.New<PublishAppHomeMongo>(x => x.AppType == appType && x.WarehouseID == wId && !x.Deleted);
                    //var publishedData = mongoDbHelper.Select(appHomePredicate).FirstOrDefault();
                    Caching.ICacheProvider _cacheProvider = new Caching.RedisCacheProvider();
                    var publishedData = _cacheProvider.GetOrSet(Caching.CacheKeyHelper.APPHomeCacheKey(appType.Replace(" ", ""), wId.ToString()), () => GetPublisheddata(appType, wId));

                    if (publishedData == null)
                    {

                    }
                    else
                    {
                        sections = JsonConvert.DeserializeObject<List<AppHomeSections>>(publishedData.ApphomeSection);

                        foreach (var a in sections)
                        {
                            if (a.SectionType == "Banner")
                                foreach (var ap in a.AppItemsList)
                                {
                                    if (ap.OfferEndTime < datenow)
                                    {
                                        ap.Expired = true;
                                        //context.AppHomeSectionItemsDB.Attach(ap);
                                        context.Entry(ap).State = EntityState.Modified;
                                        context.Commit();
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

                    sections = sections.Where(x => x.AppItemsList.Count > 0).ToList();
                    if (inActiveCustomer)
                    {
                        sections = sections.Where(x => x.SectionSubType != "Flash Deal").ToList();
                    }
                    logger.Info("End GetPublishedSection: ");

                    if (!string.IsNullOrEmpty(lang) && lang.ToLower() == "hi")
                    {

                        var BaseCategoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.BaseCategoryId)).Distinct().ToList();
                        var Categoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.CategoryId)).Distinct().ToList();
                        var Brandids = sections.SelectMany(x => x.AppItemsList.Select(y => y.SubsubCategoryId)).Distinct().ToList();
                        var SubCategoryids = sections.SelectMany(x => x.AppItemsList.Select(y => y.SubCategoryId)).Distinct().ToList();

                        var CatNames = Categoryids.Any() ? context.Categorys.Where(x => Categoryids.Contains(x.Categoryid)).Select(x => new { x.Categoryid, x.CategoryName, x.HindiName }).ToList() : null;
                        var BaseCatNames = BaseCategoryids.Any() ? context.BaseCategoryDb.Where(x => BaseCategoryids.Contains(x.BaseCategoryId)).Select(x => new { x.BaseCategoryId, x.BaseCategoryName, x.HindiName }).ToList() : null;
                        var SubCatNames = SubCategoryids.Any() ? context.SubCategorys.Where(x => SubCategoryids.Contains(x.SubCategoryId)).Select(x => new { x.SubCategoryId, x.SubcategoryName, x.HindiName }).ToList() : null;
                        var Subsubcatnames = Brandids.Any() ? context.SubsubCategorys.Where(x => Brandids.Contains(x.SubsubCategoryid)).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName, x.HindiName }).ToList() : null;

                        sections.ForEach(x =>
                        {

                            string SectionName = !string.IsNullOrEmpty(x.SectionHindiName) ? x.SectionHindiName : x.SectionName;
                            x.SectionName = SectionName;
                            x.AppItemsList.ForEach(y =>
                            {
                                if (x.SectionSubType == "Base Category")
                                {
                                    var basecat = BaseCatNames != null && BaseCatNames.Any(s => s.BaseCategoryId == y.BaseCategoryId) ? BaseCatNames.FirstOrDefault(s => s.BaseCategoryId == y.BaseCategoryId) : null;
                                    if (basecat != null)
                                    {
                                        string tileName = !string.IsNullOrEmpty(basecat.HindiName) ? basecat.HindiName : basecat.BaseCategoryName;
                                        y.TileName = tileName;
                                    }
                                }
                                else if (x.SectionSubType == "Category")
                                {
                                    var catdata = CatNames != null && CatNames.Any(s => s.Categoryid == y.CategoryId) ? CatNames.FirstOrDefault(s => s.Categoryid == y.CategoryId) : null;
                                    if (catdata != null)
                                    {
                                        string tileName = !string.IsNullOrEmpty(catdata.HindiName) ? catdata.HindiName : catdata.CategoryName;
                                        y.TileName = tileName;
                                    }
                                }
                                else if (x.SectionSubType == "Sub Category")
                                {
                                    var subcat = SubCatNames != null && SubCatNames.Any(s => s.SubCategoryId == y.SubCategoryId) ? SubCatNames.FirstOrDefault(s => s.SubCategoryId == y.SubCategoryId) : null;
                                    if (subcat != null)
                                    {
                                        string tileName = !string.IsNullOrEmpty(subcat.HindiName) ? subcat.HindiName : subcat.SubcategoryName;
                                        y.TileName = tileName;
                                    }
                                }
                                else if (x.SectionSubType == "Brand")
                                {
                                    var subsubcat = Subsubcatnames != null && Subsubcatnames.Any(s => s.SubsubCategoryid == y.SubsubCategoryId) ? Subsubcatnames.FirstOrDefault(s => s.SubsubCategoryid == y.SubsubCategoryId) : null;
                                    if (subsubcat != null)
                                    {
                                        string tileName = !string.IsNullOrEmpty(subsubcat.HindiName) ? subsubcat.HindiName : subsubcat.SubsubcategoryName;
                                        y.TileName = tileName;
                                    }
                                }

                            });
                        });

                    }


                    return sections.OrderBy(x => x.Sequence).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in GetPublishedSection " + ex.Message);
                    logger.Info("End GetPublishedSection: ");
                    return null;
                }
            }
        }

        public PublishAppHome GetPublisheddata(string appType, int wId)
        {
            using (var context = new AuthContext())
            {
                var publishedData = context.PublishAppHomeDB.Where(x => x.WarehouseID == wId && x.AppType == appType && x.Deleted == false).FirstOrDefault();
                return publishedData;
            }
        }

        public claimdata GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0, Warehouse_id = 0;
            //Access claims
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
                if (claim.Type == "Warehouseid")
                {
                    Warehouse_id = int.Parse(claim.Value);
                }
            }
            claimdata claimdata = new claimdata
            {
                UserId = userid,
                CompId = compid,
                WarehouseId = Warehouse_id
            };
            return claimdata;
        }


#region cloudinary image deleted
        /// <summary>
        /// Created date:23/09/2019
        /// Created by Raj
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string cloudinaryimage(string url)
        {
            try
            {
                string str = url.Substring(1, url.LastIndexOf('/'));
                using (WebClient client = new WebClient())
                {
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/Inactiveapphomeimage")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/Inactiveapphomeimage"));

                    client.DownloadFile(new Uri(url), @"~/CurrencySettlementImage0");

                    Account account = new Account(Startup.CloudName, Startup.APIKey, Startup.APISecret);

                    Cloudinary cloudinary = new Cloudinary(account);

                    var delResParams = new DelResParams()
                    {
                        PublicIds = new List<string> { "AppHome/flash%20deal_god1.jpg.jpg" }
                    };
                    cloudinary.DeleteResources(delResParams);
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
#endregion

#region Today Flash Deal Dashboard (21-11-2019)
        /// <summary>
        /// Today Flash Deal Dashboard
        /// </summary>
        /// <param name="FlashDeal"></param>
        /// <returns></returns>

        [Route("GetTodayFlashDeal")]
        [HttpGet]
        public List<TodayFlashDealDTO> TodayFlashDeal(int WarehouseId, string itemname, DateTime? StartDate, DateTime? EndDate)
        {
            using (var context = new AuthContext())
            {
                if (itemname == "undefined")
                {
                    itemname = "";
                }
                var warehouseid = new SqlParameter("WarehouseId", WarehouseId);
                var Itemname = new SqlParameter("itemname", itemname);
                var startDate = new SqlParameter("StartDate", StartDate);
                var endDate = new SqlParameter("EndDate", EndDate);
                var result = context.Database.SqlQuery<TodayFlashDealDTO>("exec GetTodayFlashdeal @WarehouseId,@itemname,@StartDate,@EndDate", warehouseid, Itemname, startDate, endDate).ToList();
                return result;


            }
        }
        #endregion


        public class WarehouseDetailObj
        {
            public List<Warehouse> Warehouses { get; set; }

            public bool Status { get; set; }
            public string Message { get; set; }
        }


        public class claimdata
        {
            public int UserId { get; set; }
            public int CompId { get; set; }
            public int WarehouseId { get; set; }
        }

        //=======================Function to get claim ID(of loggged in person) End========================//


        public class categoryDTO
        {
            public int ItemId { get; set; }
            public int BaseCategoryId { get; set; }
            public int Categoryid { get; set; }
            public int SubCategoryId { get; set; }
            public int SubsubCategoryid { get; set; }


        }

        public class TodayFlashDealDTO
        {
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public string itemname { get; set; }
            public double? OfferQtyAvaiable { get; set; }
            public double? FlashDealSpecialPrice { get; set; }
            public DateTime? OfferStartTime { get; set; }
            public DateTime? OfferEndTime { get; set; }
            public bool Active { get; set; }
            public bool IsOffer { get; set; }

        }




    }
}
