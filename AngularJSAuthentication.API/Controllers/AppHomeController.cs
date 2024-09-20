using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Http.Description;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Apphome")]
    public class AppHomeController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        /// <summary>
        /// Get Section Detaion for app home page
        /// </summary>
        /// <returns></returns>
        [Route("V2")]
        public IEnumerable<AppHomeDynamic> Get(string lang, int wid)
        {
            using (var db = new AuthContext())
            {
                DateTime CurrentDate = DateTime.Now;
                logger.Info("start AppHomeDynamic: ");
                List<AppHomeDynamic> data = new List<AppHomeDynamic>();
                try
                {
                    AppHomeController h = new AppHomeController();
                    var cachePolicty = new CacheItemPolicy();
                    cachePolicty.AbsoluteExpiration = h.indianTime.AddSeconds(1);
                    AppHomeDynamicitems ibjtosend = new AppHomeDynamicitems();
                    var cache = MemoryCache.Default;
                    if (cache.Get("AppHomeDynamic".ToString()) == null)
                    {
                        cache.Remove("AppHomeDynamic".ToString());
                        data = db.AppHomeDynamicDb.Where(a => a.active == true && a.Wid == wid && a.delete == false).Include("detail").OrderByDescending(a => a.sequenceno).ToList();
                        foreach (AppHomeDynamic a in data)
                        {
                            if (lang == "hi")
                            {
                                if (a.titleshindi != null)
                                {
                                    a.titles = a.titleshindi;
                                }
                            }
                            if (a.SectionType != null)
                            {
                                foreach (AppHomeItem b in a.detail)
                                {
                                    switch (a.SectionType)
                                    {
                                        case "Base Category":
                                            b.Itemcount = 1;//db.itemMasters.Where(q => q.BaseCategoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                            break;
                                        case "Banner Base Category":
                                            b.Itemcount = 1;// db.itemMasters.Where(q => q.BaseCategoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                            break;
                                        case "Category":
                                            b.Itemcount = 1;// db.itemMasters.Where(q => q.Categoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                            break;
                                        case "Banner Category":
                                            b.Itemcount = 1;// db.itemMasters.Where(q => q.Categoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                            break;
                                        case "Brand":
                                            b.Itemcount = 1;//db.itemMasters.Where(q => q.SubsubCategoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                            break;
                                        case "Banner Brand":
                                            b.Itemcount = 1;//db.itemMasters.Where(q => q.SubsubCategoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                            break;
                                        default:
                                            break;
                                    }
                                    if (lang == "hi")
                                    {
                                        if (b.HindiName != null)
                                        {
                                            b.ItemName = b.HindiName;
                                        }
                                    }
                                }
                                if (a.SectionType == "FlashDeal")
                                {
                                    a.detail = a.detail.Where(x => x.StartOfferDate <= CurrentDate
                                    && x.EndOfferDate >= CurrentDate && x.FlashDealQtyAvaiable > 0).ToList();
                                }

                            }
                        }
                        ibjtosend.AppHomeDynamic = data.Where(a => (a.SectionType == "FlashDeal" && a.detail.Any()) || a.SectionType != "FlashDeal").ToList();
                        cache.Add("AppHomeDynamic".ToString(), ibjtosend, cachePolicty);
                    }
                    else
                    {
                        ibjtosend = (AppHomeDynamicitems)cache.Get("AppHomeDynamic".ToString());
                    }
                    logger.Info("End AppHomeDynamic: ");
                    return ibjtosend.AppHomeDynamic;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AppHomeDynamic " + ex.Message);
                    logger.Info("End  AppHomeDynamic: ");
                    return null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="wid"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [Route("V3")]
        public IEnumerable<AppHomeDynamic> GetV3(string lang, int wid, int customerId)
        {
            using (var db = new AuthContext())
            {
                DateTime CurrentDate = DateTime.Now;
                logger.Info("start AppHomeDynamic: ");
                List<AppHomeDynamic> data = new List<AppHomeDynamic>();
                try
                {
                    AppHomeController h = new AppHomeController();
                    var cachePolicty = new CacheItemPolicy();
                    cachePolicty.AbsoluteExpiration = h.indianTime.AddSeconds(1);
                    AppHomeDynamicitems ibjtosend = new AppHomeDynamicitems();
                    var cache = MemoryCache.Default;
                    var inActiveCustomer = db.Customers.Any(x => x.CustomerId == customerId && (x.Active == false || x.Deleted == true));
                    //if (cache.Get("AppHomeDynamic".ToString()) == null)
                    //{
                    //cache.Remove("AppHomeDynamic".ToString());
                    data = db.AppHomeDynamicDb.Where(a => a.active == true && a.Wid == wid && a.delete == false).Include("detail").OrderByDescending(a => a.sequenceno).ToList();
                    var itemIds = new List<int>(); // data.SelectMany(x => x.detail.Select(z => z.ItemId)).ToList();



                    var CatSectionIds = data.Where(x => x.SectionType == "Category" || x.SectionType == "Banner Category").SelectMany(z => z.detail.Select(x => x.ItemId));
                    var baseCatSectionIdsItem = db.Categorys.Where(x => CatSectionIds.Contains(x.Categoryid)).Select(x => new { x.Categoryid, x.BaseCategoryId }).ToList();

                    var brandSectionIds = data.Where(x => x.SectionType == "Brand" || x.SectionType == "Banner Brand").SelectMany(z => z.detail.Select(x => x.ItemId)).ToList();

                    if (data.Any(x => x.SectionType == "Slider" && x.detail.Any(z => z.SliderSectionType == "Brand")))
                    {
                        var ids = data.Where(x => x.SectionType == "Slider").SelectMany(x => x.detail.Where(z => z.SliderSectionType == "Brand").Select(y => y.ItemId)).ToList();
                        brandSectionIds.AddRange(ids);
                    }

                    if (data.Any(x => x.SectionType == "Slider" && x.detail.Any(z => z.SliderSectionType == "Item")))
                        itemIds = data.Where(x => x.SectionType == "Slider").SelectMany(x => x.detail.Where(z => z.SliderSectionType == "Item").Select(y => y.ItemId)).ToList();

                    var itemBaseCats = itemIds.Any() ? db.itemMasters.Where(x => itemIds.Contains(x.ItemId)).Select(x => new { x.ItemId, x.BaseCategoryid, x.Categoryid }).ToList() : null;

                    var baseCatForBrands = (from a in db.SubsubCategorys
                                            join b in db.Categorys on a.Categoryid equals b.Categoryid
                                            where brandSectionIds.Contains(a.SubsubCategoryid)
                                            select (new { a.SubsubCategoryid, b.BaseCategoryId, b.Categoryid })).ToList();
                    //db.SubsubCategorys.Where(x => brandSectionIds.Contains(x.SubsubCategoryid)).Select(x => new { x.SubsubCategoryid, x.BaseCategoryId, x.Categoryid }).ToList();

                    foreach (AppHomeDynamic a in data)
                    {
                        if (lang == "hi")
                        {
                            if (a.titleshindi != null)
                            {
                                a.titles = a.titleshindi;
                            }
                        }
                        if (a.SectionType != null)
                        {
                            foreach (AppHomeItem b in a.detail)
                            {
                                switch (a.SectionType)
                                {
                                    case "Base Category":
                                        b.Itemcount = 1;//db.itemMasters.Where(q => q.BaseCategoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                        b.BaseCategoryid = b.ItemId;
                                        break;
                                    case "Banner Base Category":
                                        b.Itemcount = 1;// db.itemMasters.Where(q => q.BaseCategoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                        b.BaseCategoryid = b.ItemId;
                                        break;
                                    case "Category":
                                        b.Itemcount = 1;// db.itemMasters.Where(q => q.Categoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                        b.CategoryId = b.ItemId;
                                        b.BaseCategoryid = baseCatSectionIdsItem.Any(x => x.Categoryid == b.ItemId) ? baseCatSectionIdsItem.FirstOrDefault(x => x.Categoryid == b.ItemId).BaseCategoryId : 0;
                                        break;
                                    case "Banner Category":
                                        b.Itemcount = 1;// db.itemMasters.Where(q => q.Categoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                        b.CategoryId = b.ItemId;
                                        b.BaseCategoryid = baseCatSectionIdsItem.Any(x => x.Categoryid == b.ItemId) ? baseCatSectionIdsItem.FirstOrDefault(x => x.Categoryid == b.ItemId).BaseCategoryId : 0;
                                        break;
                                    case "Brand":
                                        b.Itemcount = 1;//db.itemMasters.Where(q => q.SubsubCategoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                        b.CategoryId = baseCatForBrands.Any(x => x.SubsubCategoryid == b.ItemId) ? baseCatForBrands.FirstOrDefault(x => x.SubsubCategoryid == b.ItemId).Categoryid : 0;
                                        b.BaseCategoryid = baseCatForBrands.Any(x => x.SubsubCategoryid == b.ItemId) ? baseCatForBrands.FirstOrDefault(x => x.SubsubCategoryid == b.ItemId).BaseCategoryId : 0;
                                        break;
                                    case "Banner Brand":
                                        b.Itemcount = 1;//db.itemMasters.Where(q => q.SubsubCategoryid == b.ItemId && q.Deleted == false && q.active == true && q.WarehouseId == wid).Count();
                                        b.CategoryId = baseCatForBrands.Any(x => x.SubsubCategoryid == b.ItemId) ? baseCatForBrands.FirstOrDefault(x => x.SubsubCategoryid == b.ItemId).Categoryid : 0;
                                        b.BaseCategoryid = baseCatForBrands.Any(x => x.SubsubCategoryid == b.ItemId) ? baseCatForBrands.FirstOrDefault(x => x.SubsubCategoryid == b.ItemId).BaseCategoryId : 0;
                                        break;
                                    case "Slider":
                                        if (b.SliderSectionType == "Brand")
                                        {
                                            b.CategoryId = baseCatForBrands.Any(x => x.SubsubCategoryid == b.ItemId) ? baseCatForBrands.FirstOrDefault(x => x.SubsubCategoryid == b.ItemId).Categoryid : 0;
                                            b.BaseCategoryid = baseCatForBrands.Any(x => x.SubsubCategoryid == b.ItemId) ? baseCatForBrands.FirstOrDefault(x => x.SubsubCategoryid == b.ItemId).BaseCategoryId : 0;
                                        }
                                        else if (b.SliderSectionType == "Item" && itemBaseCats != null && itemBaseCats.Any())
                                        {
                                            var item = itemBaseCats.FirstOrDefault(x => x.ItemId == b.ItemId);
                                            if (item != null && item.BaseCategoryid > 0)
                                            {
                                                b.BaseCategoryid = item.BaseCategoryid;
                                                b.CategoryId = item.Categoryid;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                //var item = itemBaseCats.FirstOrDefault(x => x.ItemId == b.ItemId);
                                //if (item != null && item.BaseCategoryid > 0)
                                //{
                                //    b.BaseCategoryid = item.BaseCategoryid;
                                //    b.CategoryId = item.Categoryid;
                                //}

                                if (lang == "hi")
                                {
                                    if (b.HindiName != null)
                                    {
                                        b.ItemName = b.HindiName;
                                    }
                                }
                            }
                            if (a.SectionType == "FlashDeal")
                            {
                                a.detail = a.detail.Where(x => x.StartOfferDate <= CurrentDate
                                && x.EndOfferDate >= CurrentDate && x.FlashDealQtyAvaiable > 0).ToList();
                            }

                        }
                    }
                    ibjtosend.AppHomeDynamic = data.Where(a => (a.SectionType == "FlashDeal" && a.detail.Any()) || a.SectionType != "FlashDeal").ToList();
                    if (ibjtosend.AppHomeDynamic != null && ibjtosend.AppHomeDynamic.Any())
                    {
                        int i = ibjtosend.AppHomeDynamic.Count - 1;
                        foreach (var item in ibjtosend.AppHomeDynamic)
                        {
                            item.sequenceno = i;
                            i--;
                        }
                    }
                    // cache.Add("AppHomeDynamic".ToString(), ibjtosend, cachePolicty);
                    //}
                    //else
                    //{
                    //    ibjtosend = (AppHomeDynamicitems)cache.Get("AppHomeDynamic".ToString());
                    //}
                    logger.Info("End AppHomeDynamic: ");
                    if (inActiveCustomer)
                    {
                        ibjtosend.AppHomeDynamic = ibjtosend.AppHomeDynamic.Where(x => x.SectionType != "FlashDeal").ToList();
                    }

                    return ibjtosend.AppHomeDynamic;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AppHomeDynamic " + ex.Message);
                    logger.Info("End  AppHomeDynamic: ");
                    return null;
                }
            }
        }


        /// <summary>
        /// Get Section Detaiol for Backend home page
        /// </summary>
        /// <returns></returns>
        [Route("Getsections")]
        public IEnumerable<AppHomeDynamic> Getdetail(int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start AppHomeDynamic: ");
                List<AppHomeDynamic> ass = new List<AppHomeDynamic>();
                try
                {
                    logger.Info("User ID : {0} , Company Id : {1} Get Sactions");
                    ass = db.AppHomeDynamicDb.Where(a => a.delete == false && a.Wid == WarehouseId).Include("detail").ToList();
                    logger.Info("End AppHomeDynamic: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AppHomeDynamic " + ex.Message);
                    logger.Info("End  AppHomeDynamic: ");
                    return null;
                }
            }
        }

        [HttpGet]
        [Route("GetsectionsbyId")]
        public AppHomeDynamic GetsectionsbyId(int sectionid)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start AppHomeDynamic: ");
                AppHomeDynamic ass = new AppHomeDynamic();
                try
                {
                    logger.Info("User ID : {0} , Company Id : {1} Get Sactions");
                    ass = db.AppHomeDynamicDb.Where(a => a.delete == false && a.id == sectionid).Include("detail").SingleOrDefault();
                    ass.TileType = ass.IsPopup ? 2 : (ass.IsTileType ? 1 : 0);
                    logger.Info("End AppHomeDynamic: ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AppHomeDynamic " + ex.Message);
                    logger.Info("End  AppHomeDynamic: ");
                    return null;
                }
            }
        }

        /// <summary>
        /// Post Section for app home page 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(AppHomeDynamic))]
        [Route("")]
        [AcceptVerbs("POST")]
        public AppHomeDynamic addSection(AppHomeDynamic item)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    ItemMaster itemMaster = new ItemMaster();
                    item.active = true;
                    item.IsTileType = item.TileType == 2 ? true : (item.TileType == 1 ? true : false);
                    item.IsPopup = item.TileType == 2 ? true : false;
                    db.AppHomeDynamicDb.Add(item);
                    db.Commit();
                    ICollection<AppHomeItem> appHomeItems = item.detail;
                    foreach (var Offeritem in appHomeItems)
                    {
                        if (Offeritem.IsFlashDeal == true)
                        {
                            itemMaster = db.itemMasters.Where(x => x.ItemId == Offeritem.ItemId && x.WarehouseId == item.Wid && x.active == true && x.Deleted == false).FirstOrDefault();
                            if (itemMaster != null)
                            {
                                itemMaster.OfferCategory = 2;
                                itemMaster.OfferQtyAvaiable = Offeritem.FlashDealQtyAvaiable;
                                itemMaster.OfferMaxQtyPersonCanTake = Offeritem.FlashDealMaxQtyPersonCanTake;
                                itemMaster.IsOffer = Offeritem.IsFlashDeal;
                                itemMaster.OfferType = "FlashDeal";
                                itemMaster.FlashDealSpecialPrice = Offeritem.FlashDealSpecialPrice;
                                itemMaster.OfferStartTime = Convert.ToDateTime(Offeritem.StartOfferDate);
                                itemMaster.OfferEndTime = Convert.ToDateTime(Offeritem.EndOfferDate);
                                //db.itemMasters.Attach(itemMaster);
                                db.Entry(itemMaster).State = EntityState.Modified;
                                db.Commit();
                            }

                        }

                    }


                    return item;
                }
                catch (Exception ee)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Update Home page Section
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(AppHomeDynamic))]
        [Route("PutHome")]
        [AcceptVerbs("PUT")]
        public AppHomeDynamic putSection(AppHomeDynamic item)
        {
            using (var db = new AuthContext())
            {
                ItemMaster itemMaster = new ItemMaster();
                try
                {
                    if (item.SectionType != null)
                    {
                        AppHomeDynamic objitem = db.AppHomeDynamicDb.Where(a => a.id == item.id).Include("detail").SingleOrDefault();
                        objitem.id = item.id;
                        objitem.Wid = item.Wid;
                        objitem.WarehouseName = item.WarehouseName;
                        objitem.titles = item.titles;
                        objitem.tiles = item.tiles;
                        objitem.sequenceno = item.sequenceno;
                        objitem.TileImage = item.TileImage;
                        objitem.TileBackImage = item.TileBackImage;
                        objitem.AppHomeImage = item.AppHomeImage;
                        objitem.titleshindi = item.titleshindi;
                        objitem.IsTileType = item.TileType == 2 ? true : (item.TileType == 1 ? true : false);
                        objitem.AppHomeBackColor = item.AppHomeBackColor;
                        objitem.TileBackgroundColor = item.TileBackgroundColor;
                        objitem.TileText = item.TileText;
                        objitem.IsHorizontalSection = item.IsHorizontalSection;
                        objitem.IsBackgroundImageOrColor = item.IsBackgroundImageOrColor;
                        objitem.IsSectionBackColor = item.IsSectionBackColor;
                        objitem.IsPopup = item.TileType == 2 ? true : false;
                        foreach (AppHomeItem ahomeItem in item.detail)
                        {
                            if (ahomeItem.ItemStatus != null)
                            {
                                if (ahomeItem.ItemStatus.Equals("Added"))
                                {
                                    objitem.detail.Add(ahomeItem);
                                }
                                else if (ahomeItem.ItemStatus.Equals("Deleted"))
                                {
                                    objitem.detail.Remove(objitem.detail.Where(x => x.id == ahomeItem.id).First());
                                }
                            }

                        }
                        //objitem.detail = item.detail;
                        db.Entry(objitem).State = EntityState.Modified;
                        db.Commit();
                        foreach (AppHomeItem ahomeItem in item.detail)
                        {
                            if (ahomeItem.ItemStatus != null)
                            {
                                if (ahomeItem.ItemStatus.Equals("Added"))
                                {
                                    if (ahomeItem.IsFlashDeal == true)
                                    {

                                        itemMaster = db.itemMasters.Where(x => x.ItemId == ahomeItem.ItemId && x.WarehouseId == item.Wid && x.active == true && x.Deleted == false).FirstOrDefault();
                                        if (itemMaster != null)
                                        {
                                            itemMaster.OfferCategory = 2;
                                            itemMaster.OfferQtyAvaiable = ahomeItem.FlashDealQtyAvaiable;
                                            itemMaster.OfferMaxQtyPersonCanTake = ahomeItem.FlashDealMaxQtyPersonCanTake;
                                            itemMaster.IsOffer = ahomeItem.IsFlashDeal;
                                            itemMaster.OfferType = "FlashDeal";
                                            itemMaster.FlashDealSpecialPrice = ahomeItem.FlashDealSpecialPrice;
                                            itemMaster.OfferStartTime = Convert.ToDateTime(ahomeItem.StartOfferDate);
                                            itemMaster.OfferEndTime = Convert.ToDateTime(ahomeItem.EndOfferDate);
                                            //db.itemMasters.Attach(itemMaster);
                                            db.Entry(itemMaster).State = EntityState.Modified;
                                            db.Commit();
                                        }


                                    }
                                }
                                else if (ahomeItem.ItemStatus.Equals("Deleted"))
                                {
                                    if (ahomeItem.IsFlashDeal == true)
                                    {
                                        itemMaster = db.itemMasters.Where(x => x.ItemId == ahomeItem.ItemId && x.WarehouseId == item.Wid && x.active == true && x.Deleted == false).FirstOrDefault();
                                        if (itemMaster != null)
                                        {
                                            itemMaster.OfferCategory = 0;
                                            itemMaster.OfferQtyAvaiable = 0;
                                            itemMaster.OfferMaxQtyPersonCanTake = 0;
                                            itemMaster.IsOffer = false;
                                            itemMaster.OfferType = null;
                                            itemMaster.FlashDealSpecialPrice = 0;
                                            itemMaster.OfferStartTime = null;
                                            itemMaster.OfferEndTime = null;
                                            //db.itemMasters.Attach(itemMaster);
                                            db.Entry(itemMaster).State = EntityState.Modified;
                                            db.Commit();
                                        }

                                    }

                                }
                            }
                        }
                    }
                    return item;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Active Dactive home page section
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(AppHomeDynamic))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public appHomeReturnResult SectionActDact(AppHomeDynamic item)
        {
            using (var db = new AuthContext())
            {
                bool result = false;
                appHomeReturnResult appHomeReturnResult = new appHomeReturnResult();
                try
                {
                    bool isContinue = true;
                    AppHomeDynamic ahd = db.AppHomeDynamicDb.Where(a => a.id == item.id).SingleOrDefault();
                    if (item.active == true)
                    {
                        ahd.active = false;
                        var data = item.detail;
                        foreach (var Offeritem in data)
                        {
                            if (Offeritem.IsFlashDeal == true)
                            {
                                var itemmaster = db.itemMasters.Where(x => x.ItemId == Offeritem.ItemId && x.WarehouseId == item.Wid).FirstOrDefault();
                                itemmaster.OfferCategory = 0;
                                itemmaster.OfferQtyAvaiable = 0;
                                itemmaster.OfferMaxQtyPersonCanTake = 0;
                                itemmaster.IsOffer = false;
                                itemmaster.OfferType = null;
                                itemmaster.FlashDealSpecialPrice = 0;
                                itemmaster.OfferStartTime = null;
                                itemmaster.OfferEndTime = null;
                                //db.itemMasters.Attach(itemmaster);
                                db.Entry(itemmaster).State = EntityState.Modified;
                                db.Commit();
                            }

                        }
                    }
                    else if (item.active == false)
                    {
                        int totalcount = 0;
                        if (ahd.SectionType == "FlashDeal")
                        {
                            totalcount = db.AppHomeDynamicDb.Where(a => a.Wid == ahd.Wid && a.id != ahd.id && a.SectionType == ahd.SectionType && a.active && a.delete == false).Count();
                            isContinue = totalcount == 0;
                        }

                        if (ahd.IsPopup)
                        {
                            totalcount = db.AppHomeDynamicDb.Where(a => a.Wid == ahd.Wid && a.id != ahd.id && a.active && a.delete == false && a.IsPopup).Count();
                            isContinue = totalcount == 0;
                        }
                        appHomeReturnResult.result = isContinue;
                        if (isContinue)
                        {
                            ahd.active = true;
                            var appHomeItems = item.detail;
                            foreach (var Offeritem in appHomeItems)
                            {
                                if (Offeritem.IsFlashDeal == true)
                                {
                                    ItemMaster itemMaster = db.itemMasters.Where(x => x.ItemId == Offeritem.ItemId && x.WarehouseId == item.Wid).FirstOrDefault();
                                    itemMaster.OfferCategory = 2;
                                    itemMaster.OfferQtyAvaiable = Offeritem.FlashDealQtyAvaiable;
                                    itemMaster.OfferMaxQtyPersonCanTake = Offeritem.FlashDealMaxQtyPersonCanTake;
                                    itemMaster.IsOffer = Offeritem.IsFlashDeal;
                                    itemMaster.OfferType = "FlashDeal";
                                    itemMaster.FlashDealSpecialPrice = Offeritem.FlashDealSpecialPrice;
                                    itemMaster.OfferStartTime = Convert.ToDateTime(Offeritem.StartOfferDate);
                                    itemMaster.OfferEndTime = Convert.ToDateTime(Offeritem.EndOfferDate);
                                    db.itemMasters.Attach(itemMaster);
                                    db.Entry(itemMaster).State = EntityState.Modified;
                                    db.Commit();
                                }

                            }
                        }
                        else
                        {
                            appHomeReturnResult.msg = ahd.IsPopup ? "Already active Popup on that Warehouse" : "Already FlashDeal on that Warehouse";
                        }
                    }
                    if (isContinue)
                    {
                        db.AppHomeDynamicDb.Attach(ahd);
                        db.Entry(ahd).State = EntityState.Modified;
                        result = db.Commit() > 0;
                        appHomeReturnResult.result = result;
                        appHomeReturnResult.msg = result ? "" : "Some error occurred during active appHome section.";
                    }
                    return appHomeReturnResult;
                }
                catch
                {
                    appHomeReturnResult.result = false;
                    appHomeReturnResult.msg = "Some error occurred during active appHome section.";
                    return appHomeReturnResult;
                }
            }
        }


        /// <summary>
        /// Delete Home page section
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [ResponseType(typeof(AppHomeDynamic))]
        [Route("Delete")]
        [AcceptVerbs("PUT")]
        public AppHomeDynamic SectionDelete(AppHomeDynamic item)
        {
            using (var db = new AuthContext())
            {
                ItemMaster itemMaster = new ItemMaster();
                try
                {
                    AppHomeDynamic ahd = db.AppHomeDynamicDb.Where(a => a.id == item.id).SingleOrDefault();
                    if (ahd != null)
                    {
                        ahd.delete = true;
                        db.Entry(ahd).State = EntityState.Modified;
                        db.Commit();
                        var data = item.detail;
                        foreach (var Offeritem in data)
                        {
                            if (Offeritem.IsFlashDeal == true)
                            {
                                itemMaster = db.itemMasters.Where(x => x.ItemId == Offeritem.ItemId && x.WarehouseId == item.Wid).FirstOrDefault();
                                itemMaster.OfferCategory = 0;
                                itemMaster.OfferQtyAvaiable = 0;
                                itemMaster.OfferMaxQtyPersonCanTake = 0;
                                itemMaster.IsOffer = false;
                                itemMaster.OfferType = null;
                                itemMaster.FlashDealSpecialPrice = 0;
                                itemMaster.OfferStartTime = null;
                                itemMaster.OfferEndTime = null;
                                //db.itemMasters.Attach(itemMaster);
                                db.Entry(itemMaster).State = EntityState.Modified;
                                db.Commit();
                            }

                        }
                        return item;


                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get Category, subcategory, subsubcategory bases of base category
        /// for App home page saction
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>      
        [Route("HomePageGetCategories/V2")]
        [HttpGet]
        public CatScatSscat GetCategories(string lang, int itemId, int wid)
        {
            using (var db = new AuthContext())
            {
                logger.Info("start GetCategories: ");
                List<Category> Cat = new List<Category>();
                List<SubCategory> Scat = new List<SubCategory>();
                List<SubsubCategory> SsCat = new List<SubsubCategory>();
                try
                {

                    var subCategoryQuery = "select distinct d.Categoryid,  d.CategoryName, a.[SubCategoryId],a.[SubcategoryName] ,a.[Discription],a.[SortOrder]      ,a.[IsPramotional]      ,a.[CreatedDate]      ,a.[UpdatedDate]      ,a.[CreatedBy]      ,a.[UpdateBy] ,a.[Code] ,a.[LogoUrl] ,a.[Deleted] ,a.[IsActive] ,a.[HindiName] ,a.[itemcount] from SubCategories a inner join SubcategoryCategoryMappings b on a.SubCategoryid=b.subCategoryid inner join Categories d on b.Categoryid=d.Categoryid and a.IsActive=1 and b.IsActive =1 and a.Deleted=0 and b.Deleted=0 and d.IsActive=1 and d.Deleted=0";
                    var brandQuery = "select distinct d.SubCategoryId, e.BaseCategoryId, d.SubcategoryName, e.Categoryid, e.CategoryName , a.[SubsubCategoryid],a.[SubsubcategoryName],a.[SortOrder],a.[IsPramotional],a.[Type],a.[Code],a.[CreatedDate],a.[UpdatedDate],a.[CreatedBy],a.[UpdateBy],a.[LogoUrl],a.[IsActive],a.[Deleted],a.[CommisionPercent],a.[IsExclusive],a.[HindiName],a.[itemcount],a.[AgentCommisionPercent]"
                                        + " from SubsubCategories a inner"
                                        + " join BrandCategoryMappings b on a.SubsubCategoryid = b.SubsubCategoryId"
                                        + " inner join SubcategoryCategoryMappings c on c.SubCategoryMappingId = b.SubCategoryMappingId"
                                        + " inner join SubCategories d on d.SubCategoryId = c.SubCategoryId"
                                        + " inner join Categories e on e.Categoryid = c.Categoryid"
                                        + " where a.IsActive = 1 and b.IsActive = 1 and a.Deleted = 0 and b.Deleted = 0 and d.IsActive = 1 and d.Deleted = 0 and c.IsActive = 1 " +
                                        " and c.Deleted = 0 and e.IsActive = 1 and e.Deleted = 0";
                    var Catv = db.Categorys.Where(a => a.BaseCategoryId == itemId && a.IsActive == true && a.Deleted == false).ToList();
                    var Scatv = db.Database.SqlQuery<SubCategory>(subCategoryQuery).ToList();
                    //db.SubCategorys.Where(a => a.IsActive == true && a.Deleted == false).ToList();
                    var SsCatv = db.Database.SqlQuery<SubsubCategory>(brandQuery).ToList();
                    //db.SubsubCategorys.Where(a => a.IsActive == true && a.Deleted == false).ToList();

                    var itemmasters = db.itemMasters.Where(x => x.active == true && x.Deleted == false && x.WarehouseId == wid && x.BaseCategoryid == itemId && (x.ItemAppType == 0 || x.ItemAppType == 1)).ToList();
                    foreach (var kk in Catv)
                    {
                        foreach (var d in itemmasters)
                        {
                            if (kk.Categoryid == d.Categoryid)
                            {
                                if (Cat.Count != 0)
                                {
                                    foreach (var dd in Cat)
                                    {
                                        if (dd.Categoryid == kk.Categoryid)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (lang == "hi" && !string.IsNullOrEmpty(kk.CategoryHindiName))
                                {
                                    kk.CategoryName = kk.CategoryHindiName;
                                }

                                Cat.Add(kk);
                                break;
                            }
                        }

                        if (lang == "hi")
                        {
                            if (kk.HindiName != null)
                            {
                                kk.CategoryName = string.IsNullOrEmpty(kk.HindiName) ? kk.CategoryName : kk.HindiName;
                                kk.CategoryHindiName = string.IsNullOrEmpty(kk.HindiName) ? kk.CategoryName : kk.HindiName;
                            }
                        }
                    }

                    foreach (var kkk in Scatv)
                    {
                        foreach (var d in itemmasters)
                        {
                            if (kkk.SubCategoryId == d.SubCategoryId && kkk.Categoryid == d.Categoryid/*&& !Scat.Any(z => z.SubCategoryId == d.SubCategoryId && z.Categoryid == d.Categoryid)*/)
                            {
                                if (Scat.Count != 0)
                                {
                                    foreach (var dd in Scat)
                                    {
                                        if (dd.SubCategoryId == kkk.SubCategoryId)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (lang == "hi")
                                {
                                    kkk.CategoryName = Cat.Any(x => x.Categoryid == kkk.Categoryid && !string.IsNullOrEmpty(x.CategoryHindiName)) ?
                                                        Cat.FirstOrDefault(x => x.Categoryid == kkk.Categoryid && !string.IsNullOrEmpty(x.CategoryHindiName))?.CategoryHindiName
                                                        : kkk.CategoryName;

                                    if (!string.IsNullOrEmpty(kkk.HindiName))
                                        kkk.SubcategoryName = kkk.HindiName;
                                }
                                Scat.Add(kkk);
                                break;
                            }
                        }
                        if (lang == "hi")
                        {
                            if (kkk.HindiName != null)
                            {
                                kkk.HindiName = string.IsNullOrEmpty(kkk.HindiName) ? kkk.SubcategoryName : kkk.HindiName;
                                kkk.SubcategoryName = string.IsNullOrEmpty(kkk.HindiName) ? kkk.SubcategoryName : kkk.HindiName;

                            }
                        }
                    }

                    foreach (var kkkk in SsCatv)
                    {
                        foreach (var d in itemmasters)
                        {
                            if (kkkk.SubsubCategoryid == d.SubsubCategoryid && kkkk.Categoryid == d.Categoryid && kkkk.SubCategoryId == d.SubCategoryId  /*&& !SsCat.Any(z => z.SubCategoryId == d.SubCategoryId && z.Categoryid == d.Categoryid && z.SubsubCategoryid == d.SubsubCategoryid) */)
                            {
                                if (SsCat.Count != 0)
                                {
                                    foreach (var dd in SsCat)
                                    {
                                        if (dd.SubsubCategoryid == kkkk.SubsubCategoryid)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (lang == "hi")
                                {
                                    kkkk.CategoryName = Scat.Any(x => x.Categoryid == kkkk.Categoryid && !string.IsNullOrEmpty(x.CategoryName))
                                                        ? Scat.FirstOrDefault(x => x.Categoryid == kkkk.Categoryid && !string.IsNullOrEmpty(x.CategoryName)).CategoryName
                                                        : kkkk.CategoryName;

                                    kkkk.SubcategoryName = Scat.Any(x => x.SubCategoryId == kkkk.SubCategoryId && !string.IsNullOrEmpty(x.SubcategoryName))
                                                        ? Scat.FirstOrDefault(x => x.SubCategoryId == kkkk.SubCategoryId && !string.IsNullOrEmpty(x.SubcategoryName)).SubcategoryName
                                                        : kkkk.SubcategoryName;

                                    if (!string.IsNullOrEmpty(kkkk.HindiName))
                                        kkkk.SubsubcategoryName = kkkk.HindiName;
                                }
                                SsCat.Add(kkkk);
                                break;
                            }
                        }

                        if (lang == "hi")
                        {
                            if (kkkk.HindiName != null)
                            {
                                kkkk.HindiName = string.IsNullOrEmpty(kkkk.HindiName) ? kkkk.SubsubcategoryName : kkkk.HindiName;
                                kkkk.SubsubcategoryName = string.IsNullOrEmpty(kkkk.HindiName) ? kkkk.SubsubcategoryName : kkkk.HindiName;
                            }
                        }
                    }

                    if (SsCat != null && SsCat.Any())
                    {
                        var subcategoryids = SsCat.Select(y => y.SubCategoryId);
                        var categoryids = SsCat.Select(y => y.Categoryid);
                        Scat = Scat.Where(x => categoryids.Contains(x.Categoryid) && subcategoryids.Contains(x.SubCategoryId)).ToList();
                    }
                    else
                        Scat = new List<SubCategory>();


                    CatScatSscat CatScatSscat = new CatScatSscat
                    {
                        Categories = Cat,
                        SubCategories = Scat,
                        SubSubCategories = SsCat,
                    };

                    logger.Info("End  GetCategories: ");
                    return CatScatSscat;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in AppHomeDynamic " + ex.Message);
                    logger.Info("End  AppHomeDynamic: ");
                    return null;
                }
            }
        }

        [Route("GetWarehouseFlashoffer")]
        [HttpGet]
        public List<ItemMaster> GetWarehouseFlashoffer(int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    DateTime CurrentDate = DateTime.Now;
                    List<ItemMaster> ahd = db.itemMasters.Where(a => a.WarehouseId == WarehouseId && a.OfferType == "FlashDeal" && a.OfferCategory == 2 && a.OfferEndTime >= CurrentDate).ToList();
                    if (ahd.Count != 0)
                    {
                        return ahd;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        [Route("IsPopupExistForWarehouse")]
        [HttpGet]
        public bool IsPopupExistForWarehouse(int WarehouseId, int? apphomeid)
        {
            using (var db = new AuthContext())
            {
                bool result = false;
                try
                {
                    int PopupItems = 0;
                    if (apphomeid.HasValue)
                        PopupItems = db.AppHomeDynamicDb.Where(a => a.Wid == WarehouseId && a.id != apphomeid.Value && a.active && a.delete == false && a.IsPopup).Count();
                    else
                        PopupItems = db.AppHomeDynamicDb.Where(a => a.Wid == WarehouseId && a.active && a.delete == false && a.IsPopup).Count();

                    result = PopupItems > 0;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in IsPopupExistForWarehouse Method  " + ex.Message);
                }

                return result;
            }
        }

        [Route("AppHomeDetailbyWarehouse")]
        [HttpGet]
        public HttpResponseMessage AppHomeDetailbyWarehouse(int WarehouseId)
        {
            using (var db = new AuthContext())
            {
                appHomeDetail appHomeDetail = new appHomeDetail();
                try
                {
                    List<ItemMaster> itemMasters = new List<ItemMaster>();
                    if (WarehouseId > 0)
                    {
                        logger.Info("start Itemmaster");
                        itemMasters = db.itemMasters.Where(x => x.WarehouseId == WarehouseId && x.Deleted == false && x.active == true).ToList();
                        appHomeDetail.items = itemMasters;
                        if (itemMasters != null)
                        {
                            var groupKeys = itemMasters.GroupBy(x => x.Number).Where(g => g.Count() < 3).Select(x => x.Key).ToList();
                            var Returnitems = itemMasters.Where(x => groupKeys.Contains(x.Number));
                            appHomeDetail.flashDealItem = Returnitems.ToList();
                        }
                        logger.Info("end itemmaster");

                        logger.Info("start Active Offers");
                        appHomeDetail.ActiveOffers = db.GetOfferForSliderAppHome(WarehouseId);
                        logger.Info("end Active Offers");

                        logger.Info("start Subsubategory: ");
                        var Brand = (from j in db.SubsubCategorys
                                     join i in db.itemMasters on j.SubsubCategoryid equals i.SubsubCategoryid
                                     where j.Deleted == false && j.IsActive == true && i.WarehouseId == WarehouseId && i.Deleted == false && i.active == true
                                     group j by j.SubsubCategoryid into uniqueIds
                                     select uniqueIds).Select(x => x.FirstOrDefault()).ToList();
                        appHomeDetail.Brands = Brand;
                        logger.Info("start Subsubategory: ");

                        var appHome = db.AppHomeDynamicDb.Where(a => a.Wid == WarehouseId);
                        if (appHome.FirstOrDefault() != null)
                            appHomeDetail.NextSequenceNo = appHome.Max(x => x.sequenceno) + 1;
                        else
                            appHomeDetail.NextSequenceNo = 1;
                        return Request.CreateResponse(HttpStatusCode.OK, appHomeDetail);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, appHomeDetail);
                    }

                }
                catch (Exception ss)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, appHomeDetail);

                }
            }
        }


        [Route("IsWarehouseSequenceExists")]
        [HttpGet]
        public bool IsWarehouseSequenceExists(int WarehouseId, int? apphomeid, int sequencNo)
        {
            using (var db = new AuthContext())
            {
                bool result = false;
                try
                {
                    int apphome = 0;
                    if (apphomeid.HasValue)
                        apphome = db.AppHomeDynamicDb.Where(a => a.Wid == WarehouseId && a.id != apphomeid.Value && a.active && a.delete == false && a.sequenceno == sequencNo).Count();
                    else
                        apphome = db.AppHomeDynamicDb.Where(a => a.Wid == WarehouseId && a.active && a.delete == false && a.sequenceno == sequencNo).Count();

                    result = apphome > 0;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in IsWarehouseSequenceExists Method  " + ex.Message);
                }

                return result;
            }

        }

    }
    /// <summary>.......
    /// DTO Classes
    /// </summary>
    /// 
    public class CatScatSscat
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<SubCategory> SubCategories { get; set; }
        public IEnumerable<SubsubCategory> SubSubCategories { get; set; }
    }
    //this class for caching
    public class AppHomeDynamicitems
    {
        public List<AppHomeDynamic> AppHomeDynamic { get; set; }
    }

    public class appHomeReturnResult
    {
        public bool result { get; set; }
        public string msg { get; set; }
    }


    public class appHomeDetail
    {
        public List<ItemMaster> flashDealItem { get; set; }
        public List<ItemMaster> items { get; set; }
        public List<GenricEcommers.Models.Offer> ActiveOffers { get; set; }
        public List<SubsubCategory> Brands { get; set; }
        public int NextSequenceNo { get; set; }
    }
}