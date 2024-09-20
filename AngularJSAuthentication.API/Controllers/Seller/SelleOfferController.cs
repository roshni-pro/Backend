using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Masters.Seller;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Seller;
using GenricEcommers.Models;
using Hangfire;
using LinqKit;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.Seller
{

    [RoutePrefix("api/SelleOffer")]
    public class SelleOfferController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("CreateOffer")]
        [HttpPost]
        public OfferResponseDC add(SellerOfferDc AddOffer)
        {
            OfferResponseDC offerResponseDC = new OfferResponseDC { status = true, msg = "", Offer = null, ShowValidationSkipmsg = false };
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
            using (AuthContext context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                List<Offer> offers = new List<Offer>();
                DateTime date1 = AddOffer.start;
                DateTime date2 = AddOffer.end;
                bool data = false;
                List<int> OfferIds = null;
                List<int> SubCategoryMappingids = new List<int>();
                var itemmaster =new List<ItemMaster>();
                List<FreebiesUploader> GeneretFreebies = new List<FreebiesUploader>();

                if (AddOffer.CityIds.Any() && userid > 0)
                {
                    List<Warehousedto> warehouses = context.Warehouses.Where(x => AddOffer.CityIds.Contains(x.Cityid) && x.active && !x.Deleted && !x.IsKPP).Select(y => new Warehousedto { WarehouseId = y.WarehouseId, WarehouseName = y.WarehouseName, CityId = y.Cityid }).ToList();
                    if (AddOffer.OfferOn == "Item")
                    {
                        List<int> itemids = new List<int>();
                        itemids.Add(AddOffer.ItemId);
                        itemids.Add(AddOffer.FreeItemId);
                        itemmaster = context.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();
                    }
                    foreach (var warehouse in warehouses)
                    {
                        var predicate = PredicateBuilder.True<Offer>();
                        if (AddOffer.OfferOn == "BillDiscount")
                        {
                            predicate = predicate.And(x => x.IsDeleted == false && x.IsActive == true && x.BillAmount == AddOffer.BillAmount && x.MaxDiscount == AddOffer.MaxDiscount && x.LineItem == AddOffer.LineItem && x.WarehouseId == warehouse.WarehouseId && x.OfferOn == AddOffer.OfferOn && x.start <= date1 && x.end >= date2);
                            SubCategoryMappingids = context.SubcategoryCategoryMappingDb.Where(x => x.SubCategoryId == SubCatId).Select(x => x.SubCategoryMappingId).Distinct().ToList();
                            predicate = predicate.And(x => x.BillDiscountOfferSections.Any(y => SubCategoryMappingids.Contains(y.ObjId)));
                            if (AddOffer.BillDiscountOfferOn == "Percentage")
                            {
                                predicate = predicate.And(x => x.BillDiscountOfferOn == "Percentage");
                            }
                            else
                            {
                                predicate = predicate.And(x => x.BillDiscountOfferOn == "WalletPoint");
                            }
                            OfferIds = context.OfferDb.Where(predicate).Select(x => x.OfferId).ToList();
                        }
                        if (AddOffer.OfferOn == "Item")
                        {
                            var sku = itemmaster.FirstOrDefault(x => x.ItemId == AddOffer.ItemId).SellingSku;
                            var warehouseitem = context.itemMasters.Where(x => x.SellingSku == sku && x.WarehouseId == warehouse.WarehouseId).FirstOrDefault();
                            if (warehouseitem != null)
                            {
                                predicate = predicate.And(x => x.IsDeleted == false && x.itemId == warehouseitem.ItemId && x.IsDeleted == false && x.start <= date1 && x.end >= date2 && x.WarehouseId == warehouse.WarehouseId && x.OfferOn == "Item" && x.IsActive == true);
                                OfferIds = context.OfferDb.Where(predicate).Select(x => x.OfferId).ToList();
                            }
                        }
                        
                        data = OfferIds.Any();
                        if (data)
                        {
                            offerResponseDC.status = false;
                            offerResponseDC.ShowValidationSkipmsg = true;
                            offerResponseDC.msg += (string.IsNullOrEmpty(offerResponseDC.msg) ? "" : "\n") + "This " + AddOffer.OfferOn + " Offer already exist for " + warehouse.WarehouseName + ". Please first inactive previous offer.";
                        }
                    }
                    if (offerResponseDC.status)
                    {
                        if (AddOffer.OfferOn == "Item")
                        {
                            foreach (var warehouse in warehouses)
                            {
                                FreebiesUploader fre = new FreebiesUploader();
                                fre.CityId = warehouse.CityId??0;
                                fre.WarehouseId = warehouse.WarehouseId;
                                fre.Itemnumber = itemmaster.FirstOrDefault(x => x.ItemId == AddOffer.ItemId).Number;
                                fre.MRP = itemmaster.FirstOrDefault(x => x.ItemId == AddOffer.ItemId).price;
                                fre.FreeItemnumber = itemmaster.FirstOrDefault(x => x.ItemId == AddOffer.FreeItemId).Number;
                                fre.FreeitemMRP =itemmaster.FirstOrDefault(x => x.ItemId == AddOffer.FreeItemId).price;
                                fre.StartDate = AddOffer.start;
                                fre.EndDate = AddOffer.end;
                                fre.MinimumOrderQty = AddOffer.MinOrderQuantity;
                                fre.NoOfFreeitemQty = AddOffer.NoOffreeQuantity;
                                fre.QtyAvaiable = AddOffer.FreeItemLimit;
                                fre.IsFreeStock = AddOffer.IsDispatchedFreeStock;
                                fre.OfferName = AddOffer.OfferName;
                                fre.OfferAppType = AddOffer.OfferAppType;
                                GeneretFreebies.Add(fre);
                            }
                            OfferController CreateFree = new OfferController();
                            List<FreebiesUploader> FreeBiesCreated = CreateFree.AddFreebiesDuringItemScheme(GeneretFreebies, userid, context,SubCatId);

                            if (FreeBiesCreated.Any(x => !string.IsNullOrEmpty(x.Error)))
                            {
                                offerResponseDC.msg = "offer not created for Free ItemNumber: " + string.Join(",", FreeBiesCreated.Where(x => x.Error != null).Select(x => x.FreeItemnumber).Distinct());
                            }
                            else
                            {
                                offerResponseDC.Offer = null;
                                offerResponseDC.status = true;
                                offerResponseDC.msg = "Offer Added Successfully." + FreeBiesCreated.FirstOrDefault(x => x.OfferId != null).OfferId;
                            }
                        }
                        else
                        {
                            List<KeyValuePair<int, int>> items = new List<KeyValuePair<int, int>>();
                            if (AddOffer.itemIds != null && AddOffer.itemIds.Any())
                            {
                                var billDiscountItemids = AddOffer.itemIds;
                                var itemSellingSkus = context.itemMasters.Where(x => billDiscountItemids.Contains(x.ItemId)).Select(x => x.SellingSku).ToList();

                                var warehouseids = warehouses.Select(x => x.WarehouseId).ToList();

                                var Dbitems = context.itemMasters.Where(x => itemSellingSkus.Contains(x.SellingSku) && warehouseids.Contains(x.WarehouseId) && x.Deleted == false).Select(x => new { x.ItemId, x.WarehouseId }).ToList();
                                foreach (var item in Dbitems)
                                {
                                    items.Add(new KeyValuePair<int, int>(item.WarehouseId, item.ItemId));
                                }
                            }
                            foreach (var warehouse in warehouses)
                            {
                                Offer Newoffer = new Offer();
                                Newoffer.CompanyId = 1;
                                Newoffer.StoreId = 0;// offer.StoreId;
                                Newoffer.userid = userid;
                                Newoffer.WarehouseId = warehouse.WarehouseId;
                                Newoffer.ApplyType = "Warehouse";
                                Newoffer.start = date1;
                                Newoffer.end = date2;
                                Newoffer.itemId = 0;
                                Newoffer.IsDeleted = false;
                                Newoffer.IsActive = AddOffer.IsActive;
                                Newoffer.OfferLogoUrl = "";//offer.OfferLogoUrl;
                                Newoffer.CreatedDate = indianTime;
                                Newoffer.UpdateDate = indianTime;
                                Newoffer.OfferCode = AddOffer.OfferCode;
                                Newoffer.CityId = warehouse.CityId ?? 0;
                                Newoffer.Description = AddOffer.Description;
                                Newoffer.DiscountPercentage = AddOffer.DiscountPercentage;
                                Newoffer.OfferName = AddOffer.OfferName;
                                Newoffer.BillDiscountOfferOn = AddOffer.BillDiscountOfferOn;
                                Newoffer.BillDiscountWallet = AddOffer.BillDiscountWallet;
                                if (AddOffer.OfferUseCount > 0)
                                {
                                    Newoffer.OfferUseCount = AddOffer.OfferUseCount;
                                }
                                else
                                {
                                    Newoffer.OfferUseCount = null;
                                }
                                Newoffer.IsMultiTimeUse = AddOffer.IsMultiTimeUse;
                                Newoffer.IsUseOtherOffer = AddOffer.IsUseOtherOffer;
                                Newoffer.OfferOn = AddOffer.OfferOn;
                                //Newoffer.Category = " ";
                                //Newoffer.subCategory = AddOffer.SubCatId;
                                //Newoffer.subSubCategory = offer.subSubCategory;
                                Newoffer.OfferAppType = AddOffer.OfferAppType;
                                Newoffer.ApplyOn = "PreOffer";
                                Newoffer.WalletType = AddOffer.WalletType;
                                Newoffer.BillAmount = AddOffer.BillAmount;
                                //Newoffer.BillDiscountFreeItems = offer.BillDiscountFreeItems;
                                Newoffer.BillDiscountType = "subcategory";
                                Newoffer.FreeOfferType = "BillDiscount";
                                Newoffer.MaxBillAmount = AddOffer.MaxBillAmount;
                                Newoffer.MaxDiscount = AddOffer.MaxDiscount;
                                Newoffer.OfferCategory = "Offer";
                                Newoffer.LineItem = AddOffer.LineItem;
                                offers.Add(Newoffer);
                            }
                            context.OfferDb.AddRange(offers);
                            if (context.Commit() > 0)
                            {
                                var offerbilldiscountitems = new List<OfferItemsBillDiscount>();
                                List<BillDiscountOfferSection> billDiscountOfferSections = new List<BillDiscountOfferSection>();
                                List<SellerSubCatOffer> AddSellerSubCatOffer = new List<SellerSubCatOffer>();


                                foreach (var offer in offers)
                                {
                                    // add mapping of seller subcat Offer
                                    AddSellerSubCatOffer.Add(new SellerSubCatOffer
                                    {
                                        SubCatId = SubCatId,
                                        CityId = offer.CityId,
                                        OfferId = offer.OfferId
                                    });

                                    string code = "";
                                    if (offer.OfferOn == "BillDiscount")
                                    {
                                        code = "BD_";
                                    }
                                    if (string.IsNullOrEmpty(AddOffer.OfferCode))
                                    {
                                        string offerCode = code + offer.OfferId;
                                        offer.OfferCode = offerCode;
                                    }
                                    context.Entry(offer).State = EntityState.Modified;

                                    if (SubCategoryMappingids != null && SubCategoryMappingids.Any())
                                    {
                                        foreach (var item in SubCategoryMappingids)
                                        {
                                            billDiscountOfferSections.Add(new BillDiscountOfferSection
                                            {
                                                IsInclude = true,
                                                ObjId = item,
                                                OfferId = offer.OfferId
                                            });
                                        }
                                    }
                                    if (items != null && items.Any(x => x.Key == offer.WarehouseId))
                                    {
                                        foreach (var item in items.Where(x => x.Key == offer.WarehouseId))
                                        {
                                            offerbilldiscountitems.Add(new OfferItemsBillDiscount
                                            {
                                                OfferId = offer.OfferId,
                                                itemId = item.Value,
                                                IsInclude = offer.BillDiscountType == "items"
                                            });
                                        }
                                    }
                                }


                                if (AddSellerSubCatOffer.Any())
                                    context.SellerSubCatOffers.AddRange(AddSellerSubCatOffer);
                                if (offerbilldiscountitems.Any())
                                    context.OfferItemsBillDiscountDB.AddRange(offerbilldiscountitems);
                                if (billDiscountOfferSections.Any())
                                    context.BillDiscountOfferSectionDB.AddRange(billDiscountOfferSections);

                                if (context.Commit() > 0)
                                {
                                    offerResponseDC.Offer = null;
                                    offerResponseDC.status = true;
                                    offerResponseDC.msg = "Offer Added Successfully." + string.Join(",", offers.Where(x => x.OfferId > 0).Select(x => x.OfferId));
                                }
                                else
                                {
                                    foreach (var offer in offers)
                                    {
                                        offer.IsActive = false;
                                        offer.IsDeleted = true;
                                        context.Entry(offer).State = EntityState.Modified;
                                    }
                                    offerResponseDC.Offer = null;
                                    offerResponseDC.status = false;
                                    offerResponseDC.msg = "Offer not added.";
                                }
                            }
                            else
                            {
                                offerResponseDC.Offer = null;
                                offerResponseDC.status = false;
                                offerResponseDC.msg = "Offer not added.";
                            }
                        }
                    }
                }
                else
                {
                    offerResponseDC.status = false;
                    offerResponseDC.msg = "Please select atleast one warehouse to add offer";
                }
                //#if !DEBUG
                //if (offerResponseDC != null && offerResponseDC.status)
                //{
                //    foreach (var Offer in offers)
                //    {
                //        if (Offer.IsActive && Offer.end > DateTime.Now)
                //        {  
                //            var jobId = BackgroundJob.Schedule(
                //                                      () => ActiveDeativeofferByJob(Offer.OfferId),
                //                            TimeSpan.FromMinutes((Offer.end - DateTime.Now).TotalMinutes));
                //            MongoDbHelper<ScheduleJobHistory> mongoDbHelper = new MongoDbHelper<ScheduleJobHistory>();
                //            ScheduleJobHistory scheduleJobHistory = new ScheduleJobHistory
                //            {
                //                CreatedDate = DateTime.Now,
                //                isActive = true,
                //                jobid = jobId,
                //                ObjectId = Offer.OfferId.ToString(),
                //                ObjectType = "Offer"
                //            };
                //            mongoDbHelper.Insert(scheduleJobHistory);
                //        }
                //    }
                //}
                //#endif

            }
            return offerResponseDC;
        }


        [Route("Get")]
        [HttpGet]
        public OfferPaggingData GetSellerOffer(int CityId, int skip, int take)
        {
            logger.Info("start Offer: ");
            OfferPaggingData OfferPaggingData = new OfferPaggingData();
            List<OfferList> OfferList = new List<OfferList>();
            int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());

            if (SubCatId > 0 && CityId > 0)
            {
                using (var context = new AuthContext())
                {
                    var predicatesellerOffer = PredicateBuilder.True<SellerSubCatOffer>();
                    predicatesellerOffer = predicatesellerOffer.And(x => x.SubCatId == SubCatId && x.CityId == CityId);
                    int totalcount = context.SellerSubCatOffers.Where(predicatesellerOffer).Count();
                    var sellerOfferquery = "select distinct OfferId from SellerSubCatOffers where SubCatId=" + SubCatId + " and CityId=" + CityId + " order by OfferId desc offset " + (@skip - 1) + " rows fetch next " + @take + " rows only";
                    var sellerOfferids = context.Database.SqlQuery<int>(sellerOfferquery).ToList();
                    var predicate = PredicateBuilder.True<OfferList>();
                    if (sellerOfferids != null && sellerOfferids.Any())
                    {
                        predicate = predicate.And(x => sellerOfferids.Contains(x.OfferId));
                    }
                    else
                    {
                        return OfferPaggingData;
                    }
                    predicate = predicate.And(x => !x.IsDeleted);
                    var query = from ofr in context.OfferDb
                                join wr in context.Warehouses
                                on ofr.WarehouseId equals wr.WarehouseId
                                select new OfferList
                                {
                                    OfferId = ofr.OfferId,
                                    WarehouseId = ofr.WarehouseId,
                                    WarehouseName = wr.WarehouseName + " " + wr.CityName,
                                    OfferOn = ofr.OfferOn,
                                    OfferName = ofr.OfferName,
                                    OfferCategory = ofr.OfferCategory,
                                    FreeOfferType = ofr.FreeOfferType,
                                    Description = ofr.Description,
                                    itemname = ofr.itemname,
                                    ItemId = ofr.itemId,
                                    MinOrderQuantity = ofr.MinOrderQuantity,
                                    FreeItemName = ofr.FreeItemName,
                                    NoOffreeQuantity = ofr.NoOffreeQuantity,
                                    start = ofr.start,
                                    end = ofr.end,
                                    FreeWalletPoint = ofr.FreeWalletPoint,
                                    DiscountPercentage = ofr.DiscountPercentage,
                                    FreeItemId = ofr.FreeItemId,
                                    IsActive = ofr.IsActive,
                                    QtyAvaiable = ofr.QtyAvaiable,
                                    QtyConsumed = ofr.QtyConsumed,
                                    MaxQtyPersonCanTake = ofr.MaxQtyPersonCanTake,
                                    OfferWithOtherOffer = ofr.OfferWithOtherOffer,
                                    OfferVolume = ofr.OfferVolume,
                                    FreeItemMRP = ofr.FreeItemMRP,
                                    IsDeleted = ofr.IsDeleted,
                                    CreatedDate = ofr.CreatedDate,
                                    UpdateDate = ofr.UpdateDate,
                                    OfferCode = ofr.OfferCode,
                                    BillDiscountOfferOn = ofr.BillDiscountOfferOn,
                                    BillDiscountWallet = ofr.BillDiscountWallet,
                                    BillAmount = ofr.BillAmount,
                                    FreeItemLimit = ofr.FreeItemLimit,
                                    OfferAppType = ofr.OfferAppType,
                                    ApplyType = ofr.ApplyType,
                                    LineItem = ofr.LineItem,
                                    IsDispatchedFreeStock = ofr.IsDispatchedFreeStock
                                };
                    OfferPaggingData.total_count = totalcount;
                    OfferPaggingData.OfferListDTO = query.Where(predicate).OrderByDescending(x => x.OfferId).ToList();



                }
            }
            return OfferPaggingData;

        }



        [Route("FirstHubItem")]
        [HttpGet]
        public List<OfferDetail> GetFirstHubItem(int CityId)
        {
            List<OfferDetail> OfferDetails = new List<OfferDetail>();
            using (var context = new AuthContext())
            {
                int warehouseid = context.Warehouses.FirstOrDefault(x => x.Cityid == CityId && x.active==true && !x.Deleted && !x.IsKPP).WarehouseId;
                int SubCatId = Convert.ToInt32(Request.Headers.GetValues("SubCatId").First());
                OfferDetails = context.itemMasters.Where(y => y.SubCategoryId == SubCatId && y.WarehouseId == warehouseid && y.Deleted == false && y.ItemMultiMRPId>0).Select(x => new OfferDetail
                {
                    Id = x.ItemId,
                    MinOrderQty = x.MinOrderQty,
                    UnitPrice = x.UnitPrice,
                    MRP=x.price,
                    ItemMultiMRPId = x.ItemMultiMRPId,
                    Name = x.itemname + " => (" + x.SellingSku + ") "+ " => MOQ (" + x.MinOrderQty + ")" 
                }).ToList().OrderBy(x => x.Name).ToList();

                return OfferDetails;
            }
        }

        [Route("ActiveOrDeactiveOffer")]
        [HttpGet]
        public async Task<string> ActiveOrDeactiveOffer(int OfferId, bool Isactive) 
        {
            var result = "";
            using (var db = new AuthContext()) 
            {
                var offer = db.OfferDb.Where(x => x.OfferId == OfferId).FirstOrDefault();
                if (offer != null)
                {
                    offer.IsActive = Isactive;
                    offer.UpdateDate = DateTime.Now;
                    db.Entry(offer).State = EntityState.Modified;
                    if (db.Commit() > 0)
                    {
                        result = "Offer updated";
                    }
                    
                }
                else
                {
                    result = "Offer not found";
                }
            }
            return result;
        }
    }

}

