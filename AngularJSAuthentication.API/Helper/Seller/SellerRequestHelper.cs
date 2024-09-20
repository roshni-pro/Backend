
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Seller;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace AngularJSAuthentication.API.Helper
{
    public class SellerRequestHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public int ApproveAppBannerRequests(AppBannerRequest Request, int? SequenceNo)
        {
            int objectId = 0;
            using (var context = new AuthContext())
            {
                int sequence = 0;
                if (!SequenceNo.HasValue || SequenceNo == 0)
                {
                    sequence = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Request.WarehouseId && x.Deleted == false && x.AppType == "Retailer App").Count();
                    sequence += 1;
                }
                else
                    sequence = SequenceNo.Value;

                AppHomeSections appsection = new AppHomeSections
                {
                    Active = true,
                    AppType = "Retailer App",
                    WarehouseID = Request.WarehouseId,
                    SectionType = "Banner",
                    SectionSubType = "SubCategory",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsTile = false,
                    IsBanner = true,
                    IsPopUp = false,
                    Sequence = sequence,
                    RowCount = 0,
                    ColumnCount = 0,
                    HasBackgroundColor = true,
                    HasHeaderBackgroundColor = true,
                    HasBackgroundImage = false,
                    HasHeaderBackgroundImage = false,
                    Deleted = false,
                    ViewType = "AppView",
                    IsTileSlider = false,
                    AppItemsList = new System.Collections.Generic.List<AppHomeSectionItems> {
                  new AppHomeSectionItems {
                    CreatedDate=DateTime.Now,
                    UpdatedDate=DateTime.Now,
                    BannerImage=Request.ImageUrl,
                    RedirectionType="SubCategory",
                    RedirectionID=Request.SubCatId,
                    BaseCategoryId=0,
                    SubCategoryId=Request.SubCatId,
                    SubsubCategoryId=0,
                    ItemId=0,
                    ImageLevel=2,
                    HasOffer=true,
                    OfferStartTime=Request.StartDate,
                    OfferEndTime=Request.EndDate,
                    Deleted=false,
                    Expired=false,
                    Active=true,
                    IsFlashDeal=false,
                    FlashDealQtyAvaiable=0,
                    FlashDealMaxQtyPersonCanTake=0,
                    FlashDealSpecialPrice=0,
                    MOQ=0,
                    UnitPrice=0,
                    PurchasePrice=0,
                    SellingSku=null,
                  }
                }
                };
                context.AppHomeSectionsDB.Add(appsection);
                context.Commit();
                objectId = appsection.SectionID;

            }
            return objectId;
        }
        public int ApproveBrandStoreReq(BrandStoreRequest Request)
        {
            return 0;
        }
        public int ApproveFlashDealRequest(FlashDealRequest Request, int? SequenceNo)
        {
            int objectId = 0;
            using (var context = new AuthContext())
            {

                int sequence = 0;
                //if (!SequenceNo.HasValue || SequenceNo == 0)
                //{
                sequence = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Request.WarehouseId && x.Deleted == false && x.AppType == "Retailer App").Count();
                sequence += 1;
                //}
                //else
                //    sequence = SequenceNo.Value;

                var AppendFlashdeal = context.AppHomeSectionsDB.Where(x => x.WarehouseID == Request.WarehouseId && x.SectionSubType == "Flash Deal" && x.Active == true && x.Deleted == false && x.AppType == "Retailer App").Include(x => x.AppItemsList).FirstOrDefault();
                if (Request.FlashDealRequestItems.Count > 0)
                {
                    if (AppendFlashdeal == null)
                    {
                        AppHomeSections AddSection = new AppHomeSections();
                        AddSection.Active = true;
                        AddSection.AppType = "Retailer App";
                        AddSection.WarehouseID = Request.WarehouseId;
                        AddSection.SectionType = "Tile";
                        AddSection.SectionSubType = "Flash Deal";
                        AddSection.CreatedDate = DateTime.Now;
                        AddSection.UpdatedDate = DateTime.Now;
                        AddSection.IsTile = true;
                        AddSection.IsBanner = false;
                        AddSection.IsPopUp = false;
                        AddSection.Sequence = sequence;
                        AddSection.RowCount = Request.FlashDealRequestItems.Count;
                        AddSection.ColumnCount = 1;
                        AddSection.HasBackgroundColor = true;
                        AddSection.HasHeaderBackgroundColor = true;
                        AddSection.HasBackgroundImage = false;
                        AddSection.HasHeaderBackgroundImage = false;
                        AddSection.Deleted = false;
                        AddSection.ViewType = "AppView";
                        AddSection.IsTileSlider = false;
                        AddSection.AppItemsList = new List<AppHomeSectionItems>();

                        foreach (var item in Request.FlashDealRequestItems)
                        {
                            ItemMaster iteminfo = context.itemMasters.Where(x => x.WarehouseId == Request.WarehouseId && x.ItemMultiMRPId == item.ItemMultiMrpId && x.MinOrderQty == item.Moq && x.Deleted == false).FirstOrDefault();
                            if (iteminfo != null)
                            {
                                iteminfo.OfferCategory = 2;
                                iteminfo.OfferQtyAvaiable = item.AvailableQty;
                                iteminfo.OfferMaxQtyPersonCanTake = item.MaxQty;
                                iteminfo.IsOffer = true;
                                iteminfo.OfferType = "FlashDeal";
                                iteminfo.FlashDealSpecialPrice = item.FlashDealPrice;
                                iteminfo.OfferStartTime = Convert.ToDateTime(item.StartDate);
                                iteminfo.OfferEndTime = Convert.ToDateTime(item.EndDate);

                                AppHomeSectionItems addii = new AppHomeSectionItems();
                                addii.CreatedDate = DateTime.Now;
                                addii.UpdatedDate = DateTime.Now;
                                addii.TileName = iteminfo.itemname;
                                addii.BannerImage = Request.ImageUrl;
                                addii.RedirectionType = "Flash Deal";
                                addii.RedirectionID = iteminfo.ItemId;
                                addii.BaseCategoryId = iteminfo.BaseCategoryid;
                                addii.CategoryId = iteminfo.Categoryid;
                                addii.SubCategoryId = iteminfo.SubCategoryId;
                                addii.SubsubCategoryId = iteminfo.SubsubCategoryid;
                                addii.ItemId = iteminfo.ItemId;
                                addii.ImageLevel = 2;
                                addii.HasOffer = false;
                                addii.OfferStartTime = item.StartDate;
                                addii.OfferEndTime = item.EndDate;
                                addii.Deleted = false;
                                addii.Expired = false;
                                addii.Active = true;
                                addii.IsFlashDeal = true;
                                addii.FlashDealQtyAvaiable = item.AvailableQty;
                                addii.FlashDealMaxQtyPersonCanTake = item.MaxQty;
                                addii.FlashDealSpecialPrice = item.FlashDealPrice;
                                addii.MOQ = iteminfo.MinOrderQty;
                                addii.UnitPrice = iteminfo.UnitPrice;
                                addii.PurchasePrice = iteminfo.PurchasePrice;
                                addii.SellingSku = iteminfo.SellingSku;
                                AddSection.AppItemsList.Add(addii);
                                context.Entry(iteminfo).State = EntityState.Modified;
                            }
                            context.AppHomeSectionsDB.Add(AddSection);
                        }
                        context.Commit();
                        objectId = AddSection.SectionID;
                    }
                    else
                    {
                        foreach (var item in Request.FlashDealRequestItems)
                        {
                            ItemMaster iteminfo = context.itemMasters.Where(x => x.WarehouseId == Request.WarehouseId && x.ItemMultiMRPId == item.ItemMultiMrpId && x.MinOrderQty == item.Moq && x.Deleted == false).FirstOrDefault();
                            if (iteminfo != null)
                            {
                                iteminfo.OfferCategory = 2;
                                iteminfo.OfferQtyAvaiable = item.AvailableQty;
                                iteminfo.OfferMaxQtyPersonCanTake = item.MaxQty;
                                iteminfo.IsOffer = true;
                                iteminfo.OfferType = "FlashDeal";
                                iteminfo.FlashDealSpecialPrice = item.FlashDealPrice;
                                iteminfo.OfferStartTime = Convert.ToDateTime(item.StartDate);
                                iteminfo.OfferEndTime = Convert.ToDateTime(item.EndDate);
                                AppHomeSectionItems addii = new AppHomeSectionItems();
                                addii.CreatedDate = DateTime.Now;
                                addii.UpdatedDate = DateTime.Now;
                                addii.TileName = iteminfo.itemname;
                                addii.BannerImage = Request.ImageUrl;
                                addii.RedirectionType = "Flash Deal";
                                addii.RedirectionID = iteminfo.ItemId;
                                addii.BaseCategoryId = iteminfo.BaseCategoryid;
                                addii.CategoryId = iteminfo.Categoryid;
                                addii.SubCategoryId = iteminfo.SubCategoryId;
                                addii.SubsubCategoryId = iteminfo.SubsubCategoryid;
                                addii.ItemId = iteminfo.ItemId;
                                addii.ImageLevel = 2;
                                addii.HasOffer = false;
                                addii.OfferStartTime = item.StartDate;
                                addii.OfferEndTime = item.EndDate;
                                addii.Deleted = false;
                                addii.Expired = false;
                                addii.Active = true;
                                addii.IsFlashDeal = true;
                                addii.FlashDealQtyAvaiable = item.AvailableQty;
                                addii.FlashDealMaxQtyPersonCanTake = item.MaxQty;
                                addii.FlashDealSpecialPrice = item.FlashDealPrice;
                                addii.MOQ = iteminfo.MinOrderQty;
                                addii.UnitPrice = iteminfo.UnitPrice;
                                addii.PurchasePrice = iteminfo.PurchasePrice;
                                addii.SellingSku = iteminfo.SellingSku;
                                AppendFlashdeal.AppItemsList.Add(addii);

                                context.Entry(iteminfo).State = EntityState.Modified;
                            }
                            AppendFlashdeal.RowCount = AppendFlashdeal.RowCount + 1;
                            context.Entry(AppendFlashdeal).State = EntityState.Modified;
                            context.Commit();
                            objectId = AppendFlashdeal.SectionID;
                        }
                    }
                }
                else
                {
                    if (AppendFlashdeal == null)
                    {
                        ItemMaster iteminfo = context.itemMasters.Where(x => x.WarehouseId == Request.WarehouseId && x.ItemMultiMRPId == Request.ItemMultiMrpId && x.MinOrderQty == Request.Moq && x.Deleted == false).FirstOrDefault();
                        if (iteminfo != null)
                        {
                            iteminfo.OfferCategory = 2;
                            iteminfo.OfferQtyAvaiable = Request.AvailableQty;
                            iteminfo.OfferMaxQtyPersonCanTake = Request.MaxQty;
                            iteminfo.IsOffer = true;
                            iteminfo.OfferType = "FlashDeal";
                            iteminfo.FlashDealSpecialPrice = Request.FlashDealPrice;
                            iteminfo.OfferStartTime = Convert.ToDateTime(Request.StartDate);
                            iteminfo.OfferEndTime = Convert.ToDateTime(Request.EndDate);
                            AppHomeSections appsection = new AppHomeSections
                            {
                                Active = true,
                                AppType = "Retailer App",
                                WarehouseID = Request.WarehouseId,
                                SectionType = "Tile",
                                SectionSubType = "Flash Deal",
                                CreatedDate = DateTime.Now,
                                UpdatedDate = DateTime.Now,
                                IsTile = true,
                                IsBanner = false,
                                IsPopUp = false,
                                Sequence = sequence,
                                RowCount = 1,
                                ColumnCount = 1,
                                HasBackgroundColor = true,
                                HasHeaderBackgroundColor = true,
                                HasBackgroundImage = false,
                                HasHeaderBackgroundImage = false,
                                Deleted = false,
                                ViewType = "AppView",
                                IsTileSlider = false,
                                AppItemsList = new System.Collections.Generic.List<AppHomeSectionItems> {
                                       new AppHomeSectionItems {
                        CreatedDate=DateTime.Now,
                        UpdatedDate=DateTime.Now,
                        TileName=iteminfo.itemname,
                        BannerImage=Request.ImageUrl,
                        RedirectionType="Flash Deal",
                        RedirectionID=iteminfo.ItemId,
                        BaseCategoryId=iteminfo.BaseCategoryid,
                        CategoryId=iteminfo.Categoryid,
                        SubCategoryId=iteminfo.SubCategoryId,
                        SubsubCategoryId=iteminfo.SubsubCategoryid,
                        ItemId=iteminfo.ItemId,
                        ImageLevel=2,
                        HasOffer=false,
                        OfferStartTime=Request.StartDate,
                        OfferEndTime=Request.EndDate,
                        Deleted=false,
                        Expired=false,
                        Active=true,
                        IsFlashDeal=true,
                        FlashDealQtyAvaiable=Request.AvailableQty,
                        FlashDealMaxQtyPersonCanTake=Request.MaxQty,
                        FlashDealSpecialPrice=Request.FlashDealPrice,
                        MOQ=iteminfo.MinOrderQty,
                        UnitPrice=iteminfo.UnitPrice,
                        PurchasePrice=iteminfo.PurchasePrice,
                        SellingSku=iteminfo.SellingSku,
                      }
                        }
                            };
                            context.AppHomeSectionsDB.Add(appsection);
                            context.Entry(iteminfo).State = EntityState.Modified;
                            context.Commit();
                            objectId = appsection.SectionID;
                        }
                    }
                    else
                    {
                        ItemMaster iteminfo = context.itemMasters.Where(x => x.WarehouseId == Request.WarehouseId && x.ItemMultiMRPId == Request.ItemMultiMrpId && x.MinOrderQty == Request.Moq && x.Deleted == false).FirstOrDefault();
                        if (iteminfo != null)
                        {
                            iteminfo.OfferCategory = 2;
                            iteminfo.OfferQtyAvaiable = Request.AvailableQty;
                            iteminfo.OfferMaxQtyPersonCanTake = Request.MaxQty;
                            iteminfo.IsOffer = true;
                            iteminfo.OfferType = "FlashDeal";
                            iteminfo.FlashDealSpecialPrice = Request.FlashDealPrice;
                            iteminfo.OfferStartTime = Convert.ToDateTime(Request.StartDate);
                            iteminfo.OfferEndTime = Convert.ToDateTime(Request.EndDate);

                            AppHomeSectionItems addii = new AppHomeSectionItems();
                            addii.CreatedDate = DateTime.Now;
                            addii.UpdatedDate = DateTime.Now;
                            addii.TileName = iteminfo.itemname;
                            addii.BannerImage = Request.ImageUrl;
                            addii.RedirectionType = "Flash Deal";
                            addii.RedirectionID = iteminfo.ItemId;
                            addii.BaseCategoryId = iteminfo.BaseCategoryid;
                            addii.CategoryId = iteminfo.Categoryid;
                            addii.SubCategoryId = iteminfo.SubCategoryId;
                            addii.SubsubCategoryId = iteminfo.SubsubCategoryid;
                            addii.ItemId = iteminfo.ItemId;
                            addii.ImageLevel = 2;
                            addii.HasOffer = false;
                            addii.OfferStartTime = Request.StartDate;
                            addii.OfferEndTime = Request.EndDate;
                            addii.Deleted = false;
                            addii.Expired = false;
                            addii.Active = true;
                            addii.IsFlashDeal = true;
                            addii.FlashDealQtyAvaiable = Request.AvailableQty;
                            addii.FlashDealMaxQtyPersonCanTake = Request.MaxQty;
                            addii.FlashDealSpecialPrice = Request.FlashDealPrice;
                            addii.MOQ = iteminfo.MinOrderQty;
                            addii.UnitPrice = iteminfo.UnitPrice;
                            addii.PurchasePrice = iteminfo.PurchasePrice;
                            addii.SellingSku = iteminfo.SellingSku;
                            AppendFlashdeal.RowCount = AppendFlashdeal.RowCount + 1;
                            AppendFlashdeal.AppItemsList.Add(addii);
                            context.Entry(iteminfo).State = EntityState.Modified;
                        }
                    }
                    context.Entry(AppendFlashdeal).State = EntityState.Modified;
                    context.Commit();
                    objectId = AppendFlashdeal.SectionID;
                }
            }
            return objectId;
        }
        public int ApproveMurliRequest(MurliRequest Request)
        {
            int objectId = 0;
            using (var context = new AuthContext())
            {
                int cityId = context.Warehouses.FirstOrDefault(x => x.WarehouseId == Request.WarehouseId).Cityid;
                var people = context.Peoples.Where(x => x.PeopleID == Request.ApprovedBy.Value).Select(x => new { x.DisplayName, x.PeopleID }).FirstOrDefault();
                string SubCatName = context.SubCategorys.FirstOrDefault(x => x.SubCategoryId == Request.SubCatId).SubcategoryName;

                NotificationUpdated notification = new NotificationUpdated
                {
                    CompanyId = 1,
                    title = Request.MurliNotificationTitle,
                    Message = Request.MurliNotificationMsg,
                    Pic = Request.MurliFile,
                    NotifiedTo = "All",
                    WarehouseID = Request.WarehouseId,
                    WarehouseName = "",
                    GroupID = 0,
                    GroupName = "All",
                    GroupAssociation = "Retailer",
                    NotificationType = "Non-Actionable",
                    NotificationName = SubCatName + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                    SentVia = "Shopkirana App",
                    TotalAction = 0,
                    TotalReceived = 0,
                    TotalSent = 0,
                    TotalViews = 0,
                    NotificationTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    From = Request.StartDate,
                    TO = Request.EndDate,
                    Deleted = false,
                    ItemCode = 0,
                    ItemName = "",
                    BrandCode = 0,
                    BrandName = "",
                    NotificationCategory = null,
                    Sent = false,
                    NotificationMediaType = "Audio",
                    NotificationDisplayType = "murli",
                    CityId = cityId,
                    IsMultiSchedule = false,
                    CreatedBy = Request.ApprovedBy.Value,
                    CreatedByName = people.DisplayName
                };
                context.NotificationUpdatedDb.Add(notification);
                context.Commit();
                objectId = notification.Id;

            }
            return objectId;

        }
        public int ApproveNotificationRequest(NotificationRequest Request)
        {
            int objectId = 0;
            using (var context = new AuthContext())
            {
                int cityId = context.Warehouses.FirstOrDefault(x => x.WarehouseId == Request.WarehouseId).Cityid;
                var people = context.Peoples.Where(x => x.PeopleID == Request.ApprovedBy.Value).Select(x => new { x.DisplayName, x.PeopleID }).FirstOrDefault();
                string SubCatName = context.SubCategorys.FirstOrDefault(x => x.SubCategoryId == Request.SubCatId).SubcategoryName;
                NotificationUpdated notification = new NotificationUpdated
                {
                    CompanyId = 1,
                    title = Request.NotificationTitle,
                    Message = Request.NotificationDescription,
                    Pic = Request.NotificationImage,
                    NotifiedTo = "All",
                    WarehouseID = Request.WarehouseId,
                    WarehouseName = "",
                    GroupID = 0,
                    GroupName = "All",
                    GroupAssociation = "Retailer",
                    NotificationType = "Actionable",
                    NotificationName = SubCatName + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss"),
                    SentVia = "Shopkirana App",
                    TotalAction = 0,
                    TotalReceived = 0,
                    TotalSent = 0,
                    TotalViews = 0,
                    NotificationTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    From = Request.StartDate,
                    TO = Request.EndDate,
                    Deleted = false,
                    ItemCode = Request.SubCatId,
                    ItemName = SubCatName,
                    BrandCode = 0,
                    BrandName = "",
                    NotificationCategory = "SubCategory",
                    Sent = false,
                    NotificationMediaType = "Image",
                    NotificationDisplayType = "",
                    CityId = cityId,
                    IsMultiSchedule = false,
                    CreatedBy = Request.ApprovedBy.Value,
                    CreatedByName = people.DisplayName
                };
                context.NotificationUpdatedDb.Add(notification);
                context.Commit();
                objectId = notification.Id;
            }
            return objectId;

        }

        public List<AppHomeSectionsDc> GetSectionpreview(int requestId, string RequestType)
        {
            List<AppHomeSectionsDc> sectionsResult = new List<AppHomeSectionsDc>();
            string appType = "Retailer App"; int wId = 0;
            using (var context = new AuthContext())
            {
                AppHomeSectionsDc appsection = new AppHomeSectionsDc();
                if (RequestType == "Banner")
                {
                    var Request = context.AppBannerRequests.FirstOrDefault(x => x.Id == requestId);
                    wId = Request.WarehouseId;
                    //Banner
                    appsection = new AppHomeSectionsDc
                    {
                        Active = true,
                        AppType = "Retailer App",
                        WarehouseID = Request.WarehouseId,
                        SectionType = "Banner",
                        SectionSubType = "SubCategory",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        IsTile = false,
                        IsBanner = true,
                        IsPopUp = false,
                        Sequence = Request.SequenceNo ?? 0,
                        RowCount = 0,
                        ColumnCount = 0,
                        HasBackgroundColor = true,
                        HasHeaderBackgroundColor = true,
                        HasBackgroundImage = false,
                        HasHeaderBackgroundImage = false,
                        Deleted = false,
                        ViewType = "AppView",
                        IsTileSlider = false,
                        AppItemsList = new System.Collections.Generic.List<AppHomeSectionItemsDc> {
                  new AppHomeSectionItemsDc {
                    CreatedDate=DateTime.Now,
                    UpdatedDate=DateTime.Now,
                    BannerImage=Request.ImageUrl,
                    RedirectionType="SubCategory",
                    RedirectionID=Request.SubCatId,
                    BaseCategoryId=0,
                    SubCategoryId=Request.SubCatId,
                    SubsubCategoryId=0,
                    ItemId=0,
                    ImageLevel=2,
                    HasOffer=true,
                    OfferStartTime=Request.StartDate,
                    OfferEndTime=Request.EndDate,
                    Deleted=false,
                    Expired=false,
                    Active=true,
                    IsFlashDeal=false,
                    FlashDealQtyAvaiable=0,
                    FlashDealMaxQtyPersonCanTake=0,
                    FlashDealSpecialPrice=0,
                    MOQ=0,
                    UnitPrice=0,
                    PurchasePrice=0,
                    SellingSku=null,
                  }
                }
                    };
                }
                else
                {
                    //Flashdeal
                    var Request = context.FlashDealRequests.FirstOrDefault(x => x.Id == requestId);
                    ItemMaster iteminfo = context.itemMasters.Where(x => x.WarehouseId == Request.WarehouseId && x.ItemMultiMRPId == Request.ItemMultiMrpId && x.MinOrderQty == Request.Moq && x.Deleted == false).FirstOrDefault();

                    wId = Request.WarehouseId;

                    appsection = new AppHomeSectionsDc
                    {
                        Active = true,
                        AppType = "Retailer App",
                        WarehouseID = Request.WarehouseId,
                        SectionType = "Tile",
                        SectionSubType = "Flash Deal",
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        IsTile = true,
                        IsBanner = false,
                        IsPopUp = false,
                        Sequence = Request.SequenceNo,
                        RowCount = 1,
                        ColumnCount = 1,
                        HasBackgroundColor = true,
                        HasHeaderBackgroundColor = true,
                        HasBackgroundImage = false,
                        HasHeaderBackgroundImage = false,
                        Deleted = false,
                        ViewType = "AppView",
                        IsTileSlider = false,
                        AppItemsList = new System.Collections.Generic.List<AppHomeSectionItemsDc> {
                                   new AppHomeSectionItemsDc {
                    CreatedDate=DateTime.Now,
                    UpdatedDate=DateTime.Now,
                    TileName=iteminfo.itemname,
                    BannerImage=Request.ImageUrl,
                    RedirectionType="Flash Deal",
                    RedirectionID=iteminfo.ItemId,
                    BaseCategoryId=iteminfo.BaseCategoryid,
                    CategoryId=iteminfo.Categoryid,
                    SubCategoryId=iteminfo.SubCategoryId,
                    SubsubCategoryId=iteminfo.SubsubCategoryid,
                    ItemId=iteminfo.ItemId,
                    ImageLevel=2,
                    HasOffer=false,
                    OfferStartTime=Request.StartDate,
                    OfferEndTime=Request.EndDate,
                    Deleted=false,
                    Expired=false,
                    Active=true,
                    IsFlashDeal=false,
                    FlashDealQtyAvaiable=Request.AvailableQty,
                    FlashDealMaxQtyPersonCanTake=Request.MaxQty,
                    FlashDealSpecialPrice=Request.FlashDealPrice,
                    MOQ=iteminfo.MinOrderQty,
                    UnitPrice=iteminfo.UnitPrice,
                    PurchasePrice=iteminfo.PurchasePrice,
                    SellingSku=iteminfo.SellingSku,
                  }
                    }
                    };

                }
                if (!string.IsNullOrEmpty(appType) && wId > 0)
                {
                    var datenow = indianTime;
                    sectionsResult = context.AppHomeSectionsDB.Where(x => x.WarehouseID == wId && x.AppType == appType && x.Deleted == false).Include(o => o.AppItemsList)
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
                        }));
                    }
                    sectionsResult.Add(appsection);
                    return sectionsResult.OrderBy(x => x.Sequence).ToList();
                }
                else
                {
                    return sectionsResult;
                }

            }
        }

    }
}