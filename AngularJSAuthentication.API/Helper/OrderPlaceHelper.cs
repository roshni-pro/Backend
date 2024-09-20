using AngularJSAuthentication.API.App_Code.FinBox;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.DataContract;
using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.BatchManager.Publishers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using AngularJSAuthentication.Model.Item;
using AngularJSAuthentication.Model.PlaceOrder;
using BarcodeLib;
using GenricEcommers.Models;
using LinqKit;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.OrderMastersAPIController;

namespace AngularJSAuthentication.API.Helper
{
    public class OrderPlaceHelper
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public double xPointValue = AppConstants.xPoint;

        public async Task<CustomerCODLimitDc> GetConsumerCODLimit(int CustomerId)
        {
            var result = new CustomerCODLimitDc();
            if (CustomerId > 0)
            {
                using (AuthContext db = new AuthContext())
                {
                    result = await db.Database.SqlQuery<CustomerCODLimitDc>("exec GetCustomerCODLimit {0}", CustomerId).FirstOrDefaultAsync();
                }
            }
            return result;
        }
        public async Task<PlaceOrderResponse> PushOrderMasterV6(ShoppingCart sc, AuthContext context)
        {

            Guid guid = Guid.NewGuid();
            string LimitCheck_GUID = guid.ToString();
            bool Exist = false;

            var placeOrderResponse = new PlaceOrderResponse();
            string MemberShipName = Common.Constants.AppConstants.MemberShipName;
            int MemberShipHours = Common.Constants.AppConstants.MemberShipHours;

            Customer cust = new Customer();
            var rsaKey = string.Empty;
            var hdfcOrderId = string.Empty;
            var eplOrderId = string.Empty;

            OrderMaster objOrderMaster = new OrderMaster();
            List<BillDiscount> BillDiscounts = new List<BillDiscount>();


            cust = context.Customers.Where(c => !c.Deleted && c.CustomerId == sc.CustomerId).Include(x => x.ConsumerAddress).FirstOrDefault();

            if (cust == null)
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = "Customer not found.";
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }
            Warehouse warehouse = null;
            Customer baseCustomer = new Customer
            {
                Warehouseid = cust.Warehouseid,
                ClusterId = cust.ClusterId,
                WarehouseName = cust.WarehouseName,
                ShippingAddress = cust.ShippingAddress,
                CustomerType = cust.CustomerType,
                Name = cust.Name,
                ShopName = cust.ShopName
            };
            if (cust.ConsumerAddress != null && cust.ConsumerAddress.Any(x => x.CompleteAddress == sc.ShippingAddress))
            {

                var defaultAddress = cust.ConsumerAddress.FirstOrDefault(x => x.CompleteAddress == sc.ShippingAddress);
                cust.Warehouseid = defaultAddress.WarehouseId;
                cust.ClusterId = context.Clusters.FirstOrDefault(x => x.WarehouseId == defaultAddress.WarehouseId)?.ClusterId;
                warehouse = await context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == cust.Warehouseid);
                cust.WarehouseName = warehouse.WarehouseName;
                string Address = "";
                if (!string.IsNullOrEmpty(defaultAddress.Address1))
                    Address = defaultAddress.Address1;

                cust.ShippingAddress = !string.IsNullOrEmpty(Address) ? (Address + " " + defaultAddress.CompleteAddress) : defaultAddress.CompleteAddress;
                cust.Name = defaultAddress.PersonName;
                cust.ShopName = defaultAddress.PersonName;
                cust.CustomerType = "Consumer";
            }
            else
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = "We are not serving select address";
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }

            MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart>(); //!x.GeneratedOrderId.HasValue
            var cartPredicate = PredicateBuilder.New<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart>(x => x.CustomerId == cust.CustomerId && x.WarehouseId == cust.Warehouseid && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (sc.SalesPersonId.HasValue && sc.SalesPersonId.Value > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == sc.SalesPersonId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerShoppingCart = (await mongoDbHelper.SelectAsync(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart")).FirstOrDefault();

            if ((sc.APPType == "SalesApp" || sc.APPType == "Retailer") && customerShoppingCart == null)
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = "आपका आर्डर प्राप्त हो चुका है | कृपया My Order में चेक करें |";
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }

            int minLineItem = Convert.ToInt32(ConfigurationManager.AppSettings["ConsumerFlashDealMinLineItem"]);
            if (minLineItem > 0 && sc.itemDetails.Any(x => x.OfferCategory == 2) && sc.itemDetails.Count() < minLineItem)
            {

                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = string.Format("फ़्लैश डील में आर्डर के लिए आपको कम से कम {0} लाइन आइटम लेना अनिवार्य है।", minLineItem);
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }
            var itemIds = sc.itemDetails.Select(x => x.ItemId).Distinct().ToList();

            if (cust.CustomerType.ToLower() != "consumer" || (cust.IsKPP && context.DistributorVerificationDB.Any(x => x.CustomerID == cust.CustomerId && x.IsActive)))
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = "आप इस ऐप पर ऑर्डर देने के लिए अधिकृत नहीं हैं|";
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }


            List<ItemMaster> itemMastersList = context.itemMasters.Where(x => itemIds.Contains(x.ItemId)).Distinct().ToList();

            var minOrderValue = GetRetailerMinOrder(cust, context);

            MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.WalletHundredPercentUse> mongoDbHelper_W = new MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.WalletHundredPercentUse>(); //!x.GeneratedOrderId.HasValue
            var WalletHPer = mongoDbHelper_W.Select(x => x.WarehouseId == cust.Warehouseid).FirstOrDefault();
            double ConfigWalletUsePercent = WalletHPer != null && WalletHPer.WalletPer.HasValue ? WalletHPer.WalletPer.Value : Convert.ToDouble(ConfigurationManager.AppSettings["ConsumerWalletUseOfOrderValue"]);
            // check for percentage WalletUseOfOrderValue
            if (sc.walletPointUsed > 0)
            {
                double CalculateWalletPoints = Convert.ToInt32((sc.TotalAmount * ConfigWalletUsePercent / 100) * 10);
                if (CalculateWalletPoints < sc.walletPointUsed)
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = string.Format("आप इस आर्डर पर {0} वॉलेट पॉइंट से ज्यादा के वॉलेट पॉइंट यूज़ नहीं कर पावोगे। असुविधा के लिए खेद है।", CalculateWalletPoints);
                    placeOrderResponse.cart = null;
                    return placeOrderResponse;
                }
            }

            if (sc.TotalAmount + (sc.BillDiscountAmount ?? 0) + (sc.WalletAmount) < minOrderValue)
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = string.Format("हम आज केवल {0} / - से अधिक की राशि के आर्डर को स्वीकार कर रहे हैं। असुविधा के लिए खेद है।", minOrderValue);
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }




            var customerGullak = await context.GullakDB.FirstOrDefaultAsync(x => x.CustomerId == cust.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

            if (sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
            {
                MongoDbHelper<BlockGullakAmount> mongogullakDbHelper = new MongoDbHelper<BlockGullakAmount>();
                var gullkPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == sc.CustomerId && x.Guid != sc.BlockGullakAmountGuid && x.IsActive);
                var blockGullakAmount = mongogullakDbHelper.Select(gullkPredicate).ToList().Sum(x => x.Amount);

                if (customerGullak == null || Math.Round((customerGullak.TotalAmount - blockGullakAmount), 2) < sc.GulkAmount)
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Insufficient fund in your gullak please add money to your gullak.";
                    placeOrderResponse.cart = null;
                    return placeOrderResponse;
                }
            }




            var currentappVersion = await context.ConsumerAppVersionDb.FirstOrDefaultAsync(x => x.IsActive);
            if (string.IsNullOrEmpty(sc.APPVersion) && currentappVersion != null && currentappVersion.App_version != sc.APPVersion)
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = "Please update you App. before placing order.";
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }


            var cluster = cust.ClusterId.HasValue ? await context.Clusters.FirstOrDefaultAsync(x => x.ClusterId == cust.ClusterId.Value) : null;

            List<int> offerItemId = new List<int>();
            List<int> FlashDealOrderId = new List<int>();




            var isWareHouseLive = (await context.GMWarehouseProgressDB.FirstOrDefaultAsync(x => x.WarehouseID == warehouse.WarehouseId))?.IsLaunched;

            if (isWareHouseLive.HasValue && !isWareHouseLive.Value)
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = "We are coming soon to your location.";
                placeOrderResponse.cart = null;
                return placeOrderResponse;
            }

            placeOrderResponse.NotServicing = cluster == null || !cluster.Active;
            if (placeOrderResponse.NotServicing)
            {
                placeOrderResponse.Message = "We are currently not servicing in your area. Our team will contact you soon.";
                return placeOrderResponse;
            }

            var primeCustomer = await context.PrimeCustomers.FirstOrDefaultAsync(x => x.CustomerId == cust.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

            cust.IsPrimeCustomer = primeCustomer != null && primeCustomer.StartDate <= indianTime && primeCustomer.EndDate >= indianTime;

            DateTime CurrentDate = cust.IsPrimeCustomer ? indianTime.AddHours(MemberShipHours) : indianTime;

            var appHome = await context.AppHomeSectionsDB.Where(x => x.WarehouseID == warehouse.WarehouseId && x.Deleted == false && x.Active == true && x.AppItemsList.Any(y => y.IsFlashDeal == true && y.Active == true && y.OfferStartTime <= CurrentDate
                                          && y.OfferEndTime >= indianTime)).Include(x => x.AppItemsList).ToListAsync();
            List<AppHomeItem> appHomeItems = appHome.SelectMany(x => x.AppItemsList.Select(y => new AppHomeItem
            {
                active = y.Active,
                ItemId = y.ItemId,
                id = y.SectionItemID,
                IsFlashDeal = y.IsFlashDeal
            })).ToList();

            List<int> appHomeItemids = appHomeItems.Where(x => x.active == true).Select(y => y.ItemId).ToList();
            var cartItemIds = sc.itemDetails.Where(x => x.OfferCategory == 2 && appHomeItemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
            var dealItemIds = sc.itemDetails.Where(x => x.OfferCategory == 3).Select(x => x.ItemId).ToList();

            #region IncentiveClassification by Sudhir 29-06-2023
            var itemId = new DataTable();
            itemId.Columns.Add("IntValue");
            foreach (var item in itemIds)
            {
                var dr = itemId.NewRow();
                dr["IntValue"] = item;
                itemId.Rows.Add(dr);
            }
            var allitemIds = new SqlParameter
            {
                ParameterName = "ItemIds",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = itemId
            };
            var incentiveClassificationList = context.Database.SqlQuery<IncentiveClassificationDc>("exec GetItemWiseIncentiveClassification @ItemIds", allitemIds).ToList();
            itemMastersList.ForEach(x =>
            {
                x.IncentiveClassification = incentiveClassificationList.Any() && incentiveClassificationList.Any(y => y.ItemId == x.ItemId) ? incentiveClassificationList.FirstOrDefault(y => y.ItemId == x.ItemId).IncentiveClassification : "General";
            });
            #endregion


            var freeItemIds = sc.itemDetails.Where(x => x.FreeItemId > 0 && x.FreeItemqty > 0).Select(x => x.FreeItemId).ToList();
            var FreeitemsList = context.itemMasters.Where(x => freeItemIds.Contains(x.ItemId)).Select(x => x).ToList();
            var FreeItemMultiMRPIds = FreeitemsList.Select(x => x.ItemMultiMRPId).Distinct().ToList();

            //Prime code
            var primeItemIds = sc.itemDetails.Where(x => x.IsPrimeItem).Select(x => x.ItemId);
            var itemMultiMRPIds = itemMastersList.Where(x => primeItemIds.Contains(x.ItemId)).Select(x => x.ItemMultiMRPId).Distinct().ToList();
            var PrimeItems = itemMultiMRPIds.Any() && cust.IsB2CApp ? context.PrimeItemDetails.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.CityId == cust.Cityid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList() : null;
            var dealItemMultiMRPIds = itemMastersList.Where(x => dealItemIds.Contains(x.ItemId)).Select(x => x.ItemMultiMRPId).Distinct().ToList();
            var dbdealItems = dealItemMultiMRPIds.Any() ? context.DealItems.Where(x => dealItemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == cust.Warehouseid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList() : null;

            List<ItemLimitMaster> itemLimits = new List<ItemLimitMaster>();
            List<int> Deactiavteofferids = new List<int>();
            var OrderItemMultiMRPIds = itemMastersList.Select(x => x.ItemMultiMRPId).Distinct().ToList();
            itemLimits.AddRange(context.ItemLimitMasterDB.Where(z => OrderItemMultiMRPIds.Contains(z.ItemMultiMRPId) && z.WarehouseId == cust.Warehouseid && z.IsItemLimit).ToList());


            bool IsStopActive = true;
            if (FreeItemMultiMRPIds != null)
            {
                OrderItemMultiMRPIds.AddRange(FreeItemMultiMRPIds);
            }
            RetailerAppManager retailerAppManagers = new RetailerAppManager();
            List<ItemScheme> itemPTR = retailerAppManagers.GetItemScheme(OrderItemMultiMRPIds, warehouse.WarehouseId);

            List<BillDiscountFreebiesItemQtyDC> FreeQtyList = new List<BillDiscountFreebiesItemQtyDC>();
            placeOrderResponse = ValidateShoppingCartHDFC(context, sc, warehouse, cust, appHomeItems, cartItemIds, itemMastersList, FreeitemsList, itemLimits, PrimeItems, dbdealItems, itemPTR, LimitCheck_GUID, out objOrderMaster, out BillDiscounts, out Deactiavteofferids, out IsStopActive, out FreeQtyList);
            List<NextETADate> NextETADate = null;
            if (placeOrderResponse.IsSuccess)
            {
                var billOffers = new List<Offer>();
                List<FlashDealItemConsumed> flashDealItemConsumedList = new List<FlashDealItemConsumed>();
                List<OfferItem> offerItemsList = new List<OfferItem>();
                var OfferUpdate = new List<Offer>();
                double offerWalletPoint = 0;
                var itemofferids = itemMastersList.Select(x => x.OfferId).Distinct().ToList();
                var offerItems = context.OfferDb.Where(x => itemofferids.Contains(x.OfferId) && x.IsDeleted == false && x.IsActive == true && x.WarehouseId == cust.Warehouseid).ToList();

                if (cust.IsB2CApp)
                {

                    foreach (var i in placeOrderResponse.cart.itemDetails)
                    {
                        var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
                        #region flashdeal
                        ItemLimitFreebiesDc ItemLimitFreebiesconsume = new ItemLimitFreebiesDc();


                        if (i.OfferCategory == 2 && i.IsOffer)
                        {
                            AppHomeSectionItems appHomeItem = appHome.SelectMany(x => x.AppItemsList).Where(x => x.ItemId == i.ItemId && !x.Deleted && x.Active && x.IsFlashDeal == true && x.IsFlashDeal == true && x.OfferStartTime <= CurrentDate
                                    && x.OfferEndTime >= indianTime).FirstOrDefault();

                            if (appHomeItem != null)
                            {

                                items.OfferQtyAvaiable = items.OfferQtyAvaiable - i.qty;
                                items.OfferQtyConsumed = items.OfferQtyConsumed + i.qty;
                                if (items.OfferQtyAvaiable.Value <= 0)
                                {
                                    items.IsOffer = false;
                                }
                                context.Entry(items).State = EntityState.Modified;

                                appHomeItem.FlashDealQtyAvaiable = appHomeItem.FlashDealQtyAvaiable - i.qty;
                                context.Entry(appHomeItem).State = EntityState.Modified;
                            }
                            //Insert in flashdealitemconsumed for functionilty that an customer take only one time flash deal.
                            FlashDealItemConsumed flashDealItemConsumed = new FlashDealItemConsumed();
                            flashDealItemConsumed.FlashDealId = appHomeItem != null ? Convert.ToInt32(appHomeItem.SectionItemID) : 0;
                            flashDealItemConsumed.ItemId = i.ItemId;
                            flashDealItemConsumed.WarehouseId = i.WarehouseId;
                            flashDealItemConsumed.CompanyId = i.CompanyId;
                            flashDealItemConsumed.CustomerId = cust.CustomerId;
                            flashDealItemConsumed.CreatedDate = indianTime;
                            flashDealItemConsumed.UpdatedDate = indianTime;
                            flashDealItemConsumedList.Add(flashDealItemConsumed);
                            //this.SaveChanges();
                            FlashDealOrderId.Add(flashDealItemConsumed.FlashDealItemConsumedId);

                        }

                        #endregion

                        #region Free Item and offer Walletpoint

                        if (i.IsOffer == true && i.FreeItemId > 0 && i.FreeItemqty > 0)
                        {
                            #region  validated freebise offer
                            var offer = offerItems.FirstOrDefault(x => x.OfferId == items.OfferId);

                            //to consume qty of freebiese if stock hit from currentstock in offer
                            if (offer != null && !offer.IsDispatchedFreeStock)
                            {
                                ItemLimitFreebiesconsume.ItemMultiMrpId = FreeitemsList.FirstOrDefault(f => f.ItemId == i.FreeItemId).ItemMultiMRPId;
                                ItemLimitFreebiesconsume.Qty = i.FreeItemqty;
                            }

                            //freesqtylimit
                            if (offer != null && i.FreeItemqty <= offer.FreeItemLimit)
                            {
                                offer.QtyAvaiable = offer.QtyAvaiable - i.FreeItemqty;
                                offer.QtyConsumed = offer.QtyConsumed + i.FreeItemqty;
                                if (offer.QtyAvaiable <= 0)
                                {
                                    offer.IsActive = false;
                                }
                                OfferUpdate.Add(offer);
                            }

                            OfferItem ff = new OfferItem();
                            ff.CompanyId = i.CompanyId;
                            ff.WarehouseId = i.WarehouseId;
                            ff.itemId = items.ItemId;
                            ff.itemname = items.itemname;
                            ff.MinOrderQuantity = offer.MinOrderQuantity;
                            ff.NoOffreeQuantity = i.FreeItemqty;
                            ff.FreeItemId = offer.FreeItemId;
                            ff.FreeItemName = offer.FreeItemName;
                            ff.FreeItemMRP = offer.FreeItemMRP;
                            ff.IsDeleted = false;
                            ff.CreatedDate = indianTime;
                            ff.UpdateDate = indianTime;
                            ff.CustomerId = cust.CustomerId;
                            //ff.OrderId = placeOrderResponse.OrderMaster.OrderId;
                            ff.OfferType = "ItemMaster";
                            ff.ReferOfferId = offer.OfferId;
                            //offerItemId.Add(ff.OfferId);
                            offerItemsList.Add(ff);

                            #endregion
                        }


                        if (i.IsOffer == true && i.OfferWalletPoint > 0)
                        {
                            //If offer is on wallet point then update is wallet point.
                            offerWalletPoint = offerWalletPoint + i.OfferWalletPoint;
                            var offerdata = offerItems.FirstOrDefault(x => x.OfferId == items.OfferId);
                            OfferItem offerItem = new OfferItem();

                            offerItem.CompanyId = i.CompanyId;
                            offerItem.WarehouseId = i.WarehouseId;
                            offerItem.itemId = items.ItemId;
                            offerItem.itemname = items.itemname;
                            offerItem.MinOrderQuantity = offerdata.MinOrderQuantity;
                            offerItem.NoOffreeQuantity = i.FreeItemqty;
                            offerItem.FreeItemId = offerdata.FreeItemId;
                            offerItem.FreeItemName = offerdata.FreeItemName;
                            offerItem.FreeItemMRP = offerdata.FreeItemMRP;
                            offerItem.IsDeleted = false;
                            offerItem.CreatedDate = indianTime;
                            offerItem.UpdateDate = indianTime;
                            offerItem.CustomerId = cust.CustomerId;
                            //offerItem.OrderId = objOrderMaster.OrderId;
                            offerItem.WallentPoint = Convert.ToInt32(i.OfferWalletPoint);
                            offerItem.ReferOfferId = offerdata.OfferId;
                            offerItem.OfferType = "WalletPoint";
                            offerItemsList.Add(offerItem);
                            //offerItemId.Add(offerItem.OfferId);

                        }


                        #endregion

                        //For Item Deactive
                        #region Validated item Limit
                        ItemLimitMaster ItemLimitMaster = itemLimits.Where(x => x.WarehouseId == items.WarehouseId && x.ItemMultiMRPId == items.ItemMultiMRPId).FirstOrDefault();
                        if (ItemLimitMaster != null && ItemLimitMaster.IsItemLimit == true)
                        {
                            #region to consume qty of freebiese if stock hit from currentstock in offer

                            if (ItemLimitFreebiesconsume != null)
                            {
                                if (ItemLimitFreebiesconsume.ItemMultiMrpId == ItemLimitMaster.ItemMultiMRPId)
                                {
                                    i.qty += ItemLimitFreebiesconsume.Qty;
                                }
                            }
                            #endregion


                            if (i.qty < ItemLimitMaster.ItemlimitQty || i.qty == 0)
                            {

                                ItemLimitMaster.ItemlimitQty = ItemLimitMaster.ItemlimitQty - i.qty;
                                ItemLimitMaster.ItemLimitSaleQty = ItemLimitMaster.ItemLimitSaleQty + i.qty;

                                context.Entry(ItemLimitMaster).State = EntityState.Modified;
                            }
                            else
                            {

                                ItemLimitMaster.ItemlimitQty = ItemLimitMaster.ItemlimitQty - i.qty;
                                ItemLimitMaster.ItemLimitSaleQty = ItemLimitMaster.ItemLimitSaleQty + i.qty;
                                ItemLimitMaster.IsItemLimit = false;
                                context.Entry(ItemLimitMaster).State = EntityState.Modified;

                                if (ItemLimitMaster.ItemlimitQty <= 0 || items.MinOrderQty > ItemLimitMaster.ItemlimitQty)
                                {
                                    //deactive
                                    List<ItemMaster> itemsDeactive = context.itemMasters.Where(x => x.Number == ItemLimitMaster.ItemNumber && x.active == true && x.WarehouseId == ItemLimitMaster.WarehouseId && x.ItemMultiMRPId == ItemLimitMaster.ItemMultiMRPId).ToList();
                                    foreach (var Ditem in itemsDeactive)
                                    {
                                        Ditem.active = false;
                                        Ditem.UpdatedDate = indianTime;
                                        Ditem.UpdateBy = "Auto Dective";
                                        Ditem.Reason = "Auto Dective due remaining limit is less than MOQ  or zero:" + ItemLimitMaster.ItemlimitQty;
                                        context.Entry(Ditem).State = EntityState.Modified;
                                    }
                                }
                            }
                        }

                        ItemLimitMaster freeItemLimitMaster = context.ItemLimitMasterDB.FirstOrDefault(x => x.WarehouseId == items.WarehouseId && x.ItemMultiMRPId == ItemLimitFreebiesconsume.ItemMultiMrpId && x.ItemMultiMRPId != items.ItemMultiMRPId);
                        if (freeItemLimitMaster != null && freeItemLimitMaster.IsItemLimit == true && ItemLimitFreebiesconsume != null)
                        {
                            freeItemLimitMaster.ItemlimitQty = freeItemLimitMaster.ItemlimitQty - ItemLimitFreebiesconsume.Qty;
                            freeItemLimitMaster.ItemLimitSaleQty = freeItemLimitMaster.ItemLimitSaleQty + ItemLimitFreebiesconsume.Qty;
                            context.Entry(freeItemLimitMaster).State = EntityState.Modified;
                        }
                        #endregion
                    }
                    #region BillDiscount Free Item Update Qty
                    if (BillDiscounts.Any())
                    {
                        if (OfferUpdate == null)
                            OfferUpdate = new List<Offer>();
                        var offerids = BillDiscounts.Select(y => y.OfferId).ToList();
                        billOffers = context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.BillDiscountOfferOn == "FreeItem").Include(x => x.BillDiscountFreeItems).ToList();
                        var BillDiscountFreeItemlst = billOffers.SelectMany(x => x.BillDiscountFreeItems).ToList();
                        foreach (var billOffer in billOffers)
                        {
                            var BillDiscountFreeItems = BillDiscountFreeItemlst.Where(x => offerids.Contains(x.offerId)).ToList();
                            bool inactiveoffer = false;
                            int offerqty = 0;
                            foreach (var item in BillDiscountFreeItems.Where(x => x.offerId == billOffer.OfferId).ToList())
                            {
                                var freeQty = item.Qty;
                                if (billOffer.IsBillDiscountFreebiesItem)
                                {
                                    var itemdata = FreeQtyList.FirstOrDefault(x => x.Offerid == billOffer.OfferId);
                                    if (itemdata != null && itemdata.BillDiscountItemQty > 0)
                                    {
                                        freeQty = itemdata.BillDiscountItemQty * item.Qty;
                                    }
                                }
                                else if (billOffer.IsBillDiscountFreebiesValue)
                                {
                                    var itemdata = FreeQtyList.FirstOrDefault(x => x.Offerid == billOffer.OfferId);
                                    if (itemdata != null && itemdata.BillDiscountValueQty > 0)
                                    {
                                        freeQty = itemdata.BillDiscountValueQty * item.Qty;
                                    }
                                }

                                if (freeQty > 0)
                                {
                                    item.RemainingOfferStockQty += freeQty;
                                    context.Entry(item).State = EntityState.Modified;
                                    if (item.RemainingOfferStockQty >= item.OfferStockQty)
                                        inactiveoffer = true;

                                    //if(!billOffer.IsDispatchedFreeStock) 
                                    //Itemid, itemmrpid,qty 

                                    offerqty += freeQty;
                                    OfferItem ff = new OfferItem();
                                    ff.CompanyId = billOffer.CompanyId;
                                    ff.WarehouseId = billOffer.WarehouseId;
                                    ff.itemId = item.ItemId;///we are using free item id 
                                    ff.itemname = item.ItemName;
                                    ff.MinOrderQuantity = item.Qty;
                                    ff.NoOffreeQuantity = freeQty;
                                    ff.FreeItemId = item.ItemId;
                                    ff.FreeItemName = item.ItemName;
                                    ff.FreeItemMRP = item.MRP;
                                    ff.IsDeleted = false;
                                    ff.CreatedDate = indianTime;
                                    ff.UpdateDate = indianTime;
                                    ff.CustomerId = cust.CustomerId;
                                    ff.OfferType = "BillDiscount_FreeItem";
                                    ff.ReferOfferId = billOffer.OfferId;
                                    offerItemsList.Add(ff);
                                }

                            }

                            billOffer.QtyAvaiable = billOffer.QtyAvaiable - offerqty;
                            billOffer.QtyConsumed = billOffer.QtyConsumed + offerqty;


                            if (inactiveoffer)
                            {
                                billOffer.IsActive = false;
                            }
                            OfferUpdate.Add(billOffer);
                        }

                    }
                    #endregion



                }
                else
                {
                    foreach (var i in placeOrderResponse.cart.itemDetails)
                    {
                        var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
                        if (i.IsOffer == true && i.FreeItemId > 0 && i.FreeItemqty > 0)
                        {
                            #region Add if validated
                            var offer = offerItems.Where(x => x.OfferId == items.OfferId).SingleOrDefault();
                            //freesqtylimit not 

                            OfferItem ff = new OfferItem();
                            ff.CompanyId = i.CompanyId;
                            ff.WarehouseId = i.WarehouseId;
                            ff.itemId = items.ItemId;
                            ff.itemname = items.itemname;
                            ff.MinOrderQuantity = offer.MinOrderQuantity;
                            ff.NoOffreeQuantity = i.FreeItemqty;
                            ff.FreeItemId = offer.FreeItemId;
                            ff.FreeItemName = offer.FreeItemName;
                            ff.FreeItemMRP = offer.FreeItemMRP;
                            ff.IsDeleted = false;
                            ff.CreatedDate = indianTime;
                            ff.UpdateDate = indianTime;
                            ff.CustomerId = cust.CustomerId;
                            //ff.OrderId = placeOrderResponse.OrderMaster.OrderId;
                            ff.OfferType = "ItemMaster";
                            ff.ReferOfferId = offer.OfferId;
                            //offerItemId.Add(ff.OfferId);
                            offerItemsList.Add(ff);

                            #endregion
                        }
                    }
                }

                #region Rewards, Offers, FlashDeals, Wallet etc....

                double rewardpoint = (double)objOrderMaster.orderDetails.Sum(x => x.marginPoint);

                objOrderMaster.deliveryCharge = sc.deliveryCharge;

                objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt) + objOrderMaster.deliveryCharge.Value, 2);
                objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
                objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
                objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
                objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero);
                objOrderMaster.DiscountAmount = 0;//System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmountAfterTaxDisc), 2);
                objOrderMaster.OrderType = sc.CustomerType.ToLower() == "consumer" ? 11 : (sc.status == "Replace" ? 3 : (cust.IsFranchise ? 7 : 1));
                //add cluster to ordermaster
                objOrderMaster.ClusterId = cust.ClusterId ?? 0;
                objOrderMaster.ClusterName = cust.ClusterName;
                objOrderMaster.IsPrimeCustomer = cust.IsPrimeCustomer;
                var walletUsedPoint1 = sc.walletPointUsed;
                var walletAmount1 = sc.WalletAmount;
                CashConversion cash = context.CashConversionDb.FirstOrDefault(x => x.IsConsumer == true);

                double rewPoint = 0;
                double rewAmount = 0;

                var customerSegments = context.CustomerSegmentDb.Where(x => x.CustomerId == sc.CustomerId && x.IsActive && x.IsDeleted == false).ToList();
                var customerSegment = customerSegments.OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                if (customerSegment != null)
                {
                    switch (customerSegment.Segment)
                    {
                        case 1:
                            objOrderMaster.CustomerClass = "Platinum";
                            break;
                        case 2:
                            objOrderMaster.CustomerClass = "Gold";
                            break;
                        case 3:
                            objOrderMaster.CustomerClass = "Silver";
                            break;
                        case 4:
                            objOrderMaster.CustomerClass = "GT";
                            break;
                        case 5:
                            switch (customerSegment.PotentialSegment)
                            {
                                case 1:
                                    objOrderMaster.CustomerClass = "Potential Platinum";
                                    break;
                                case 2:
                                    objOrderMaster.CustomerClass = "Potential Gold";
                                    break;
                                case 3:
                                    objOrderMaster.CustomerClass = "Potential Silver";
                                    break;
                                case 4:
                                    objOrderMaster.CustomerClass = "Potential GT";
                                    break;
                                case 5:
                                    objOrderMaster.CustomerClass = "Digital";
                                    break;
                            }
                            break;
                    }
                }




                context.DbOrderMaster.Add(objOrderMaster);
                context.Commit();

                objOrderMaster = RewardAndWalletPointForPlacedOrder(context, placeOrderResponse.cart, offerWalletPoint, objOrderMaster, rewardpoint, cust, walletUsedPoint1, rewPoint, rewAmount, cash);


                if (OfferUpdate != null && OfferUpdate.Any())
                {
                    foreach (var Offers in OfferUpdate)
                    {
                        context.Entry(Offers).State = EntityState.Modified;
                    }
                }

                if (cust != null)
                {
                    cust.Warehouseid = baseCustomer.Warehouseid;
                    cust.ClusterId = baseCustomer.ClusterId;
                    warehouse = await context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == baseCustomer.Warehouseid);
                    cust.WarehouseName = warehouse.WarehouseName;
                    cust.ShippingAddress = baseCustomer.ShippingAddress;
                    cust.Name = baseCustomer.Name;
                    cust.ShopName = baseCustomer.ShopName;
                    cust.CustomerType = baseCustomer.CustomerType;
                    cust.ordercount = cust.ordercount + 1;
                    cust.MonthlyTurnOver = cust.MonthlyTurnOver + objOrderMaster.GrossAmount;
                    context.Entry(cust).State = EntityState.Modified;
                }


                //for first order

                #endregion

                #region payment mode with status
                if (!string.IsNullOrEmpty(sc.paymentThrough) && (sc.paymentThrough.ToLower().Contains("hdfc") || sc.paymentThrough.ToLower().Contains("truepay") || sc.paymentThrough.ToLower().Contains("epaylater") || sc.paymentThrough.ToLower().Contains("chqbook") || sc.paymentThrough.ToLower().Contains("directudhar") || sc.paymentThrough.ToLower().Contains("scaleup") || sc.paymentThrough.ToLower().Contains("razorpay")))//by Ashwin
                {
                    objOrderMaster.Status = "Payment Pending";
                    objOrderMaster.paymentThrough = sc.paymentThrough;
                    objOrderMaster.paymentMode = "Online";
                }
                else if (sc.paymentThrough.ToLower().Contains("gullak") || sc.paymentThrough.ToLower().Contains("paylater"))
                {
                    objOrderMaster.Status = "Pending";
                    objOrderMaster.paymentThrough = sc.paymentThrough;
                    objOrderMaster.paymentMode = "Online";
                }
                else
                {
                    objOrderMaster.paymentMode = "COD";
                    objOrderMaster.paymentThrough = sc.paymentThrough;
                    objOrderMaster.Status = "Pending";
                }

                objOrderMaster.Status = (cust.IsB2CApp && cluster != null && cluster.Active) ? objOrderMaster.Status : (!cust.IsB2CApp && IsStopActive) ? objOrderMaster.Status : "Inactive";

                #endregion

                #region Bill Discount
                if (!string.IsNullOrEmpty(sc.BillDiscountOfferId))
                {
                    List<int> billdiscountofferids = BillDiscounts.Select(x => x.OfferId).ToList();
                    List<Offer> Offers = context.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.BillDiscountFreeItems).ToList();
                    List<BillDiscount> dbBillDiscounts = context.BillDiscountDb.Where(x => billdiscountofferids.Contains(x.OfferId) && x.CustomerId == cust.CustomerId && x.OrderId == 0 && x.IsActive).ToList();

                    #region BillDiscount Free Item

                    if (Offers != null)
                    {
                        if (Offers.Any(x => x.FreeItemLimit.HasValue && x.FreeItemLimit.Value > 0 && (x.OfferOn == "ScratchBillDiscount" || x.OfferOn == "BillDiscount")))
                        {
                            var limitofferids = Offers.Where(x => x.FreeItemLimit.HasValue && x.FreeItemLimit.Value > 0 && (x.OfferOn == "ScratchBillDiscount" || x.OfferOn == "BillDiscount")).Select(x => x.OfferId);
                            var offerTakingCount = context.BillDiscountDb.Where(x => limitofferids.Contains(x.OfferId)).GroupBy(x => x.OfferId).Select(x => new { offerid = x.Key, totalCount = x.Count() }).ToList();
                            if (offerTakingCount != null && offerTakingCount.Any())
                            {
                                foreach (var item in Offers.Where(x => limitofferids.Contains(x.OfferId)))
                                {
                                    var offertaking = offerTakingCount.FirstOrDefault(x => x.offerid == item.OfferId);
                                    if (offertaking != null && item.FreeItemLimit <= offertaking.totalCount + 1)
                                    {
                                        Deactiavteofferids.Add(item.OfferId);
                                    }
                                    else if (item.FreeItemLimit == 1)
                                    {
                                        Deactiavteofferids.Add(item.OfferId);
                                    }
                                }
                            }
                        }


                    }
                    #endregion
                    double? RecalculateBillDiscountAmount = 0;
                    foreach (var offer in BillDiscounts)
                    {
                        var scritchcartoffer = !Offers.Any(x => x.OfferId == offer.OfferId && x.BillDiscountOfferOn == "DynamicAmount") ?
                            dbBillDiscounts.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == cust.CustomerId && x.OrderId == 0)
                            : dbBillDiscounts.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == cust.CustomerId && x.BillDiscountAmount == offer.BillDiscountAmount && x.OrderId == 0);

                        offer.OrderId = objOrderMaster.OrderId;
                        if (scritchcartoffer == null && Offers != null && Offers.Any(x => x.IsCRMOffer == false && x.OfferId == offer.OfferId))
                        {
                            RecalculateBillDiscountAmount += offer.BillDiscountAmount;
                            context.BillDiscountDb.Add(offer);
                        }
                        else
                        {
                            if (Offers != null && Offers.Any(x => x.IsCRMOffer && x.OfferId == offer.OfferId))
                            {
                                var scritchcartoffercrm = dbBillDiscounts.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == cust.CustomerId && x.OrderId == 0 && x.IsDeleted == false && x.IsActive == true);
                                if (scritchcartoffercrm != null)
                                {
                                    scritchcartoffercrm.BillDiscountTypeValue = offer.BillDiscountTypeValue;
                                    scritchcartoffercrm.IsUsedNextOrder = offer.IsUsedNextOrder;
                                    scritchcartoffercrm.OrderId = objOrderMaster.OrderId;
                                    scritchcartoffercrm.BillDiscountAmount = offer.BillDiscountAmount;
                                    scritchcartoffercrm.ModifiedBy = cust.CustomerId;
                                    scritchcartoffercrm.ModifiedDate = indianTime;
                                    context.Entry(scritchcartoffercrm).State = EntityState.Modified;

                                    RecalculateBillDiscountAmount += offer.BillDiscountAmount;
                                }
                            }
                            else
                            {
                                scritchcartoffer.BillDiscountTypeValue = offer.BillDiscountTypeValue;
                                scritchcartoffer.IsUsedNextOrder = offer.IsUsedNextOrder;
                                scritchcartoffer.OrderId = objOrderMaster.OrderId;
                                scritchcartoffer.BillDiscountAmount = offer.BillDiscountAmount;
                                scritchcartoffer.ModifiedBy = cust.CustomerId;
                                scritchcartoffer.ModifiedDate = indianTime;
                                context.Entry(scritchcartoffer).State = EntityState.Modified;

                                RecalculateBillDiscountAmount += offer.BillDiscountAmount;
                            }
                        }
                    }
                    //sc.BillDiscountAmount = BillDiscounts.Sum(x => x.BillDiscountAmount);
                    sc.BillDiscountAmount = RecalculateBillDiscountAmount;
                }


                // 08Dec2021 Offer bies changes
                if (Deactiavteofferids.Any())
                {
                    var Deactiavteoffers = context.OfferDb.Where(x => Deactiavteofferids.Contains(x.OfferId) && x.IsActive).ToList();
                    foreach (var offer in Deactiavteoffers)
                    {
                        offer.UpdateDate = indianTime;
                        offer.IsActive = false;
                        context.Entry(offer).State = EntityState.Modified;
                    }
                }


                #endregion

                //#region TCS Calculate
                //GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();

                //var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(cust.CustomerId, cust.PanNo, context);

                //if (tcsConfig != null && !cust.IsTCSExemption)
                //{
                //    if ((tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + objOrderMaster.TotalAmount) > tcsConfig.TCSAmountLimit)
                //    {
                //        var percent = !cust.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                //        if (tcsConfig.TotalPurchase > tcsConfig.TCSAmountLimit)
                //        {
                //            objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0)) * percent / 100;
                //        }
                //        else if (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount > tcsConfig.TCSAmountLimit)
                //        {
                //            objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0)) * percent / 100;
                //        }
                //        else
                //        {
                //            var TCSCalculatedAMT = (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + objOrderMaster.TotalAmount) - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0) - tcsConfig.TCSAmountLimit;
                //            if (TCSCalculatedAMT > 0)
                //            {
                //                objOrderMaster.TCSAmount = TCSCalculatedAMT * percent / 100;
                //            }
                //        }
                //    }
                //}
                //#endregion

                objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0) + objOrderMaster.TCSAmount;
                objOrderMaster.BillDiscountAmount = sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0;
                objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero);

                bool sendNotification = false;
                var GtxnId = "";

                if (objOrderMaster.OrderId != 0)
                {
                    if (customerShoppingCart != null)
                    {
                        customerShoppingCart.GeneratedOrderId = objOrderMaster.OrderId;
                        bool status = mongoDbHelper.Replace(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                    }

                    #region BillDiscount_FreeItem  and Flash Deal

                    if (offerItemsList != null && offerItemsList.Any())
                    {
                        foreach (var data in offerItemsList)
                        {
                            var offerdata = billOffers.FirstOrDefault(y => y.OfferId == data.ReferOfferId);
                            data.OrderId = objOrderMaster.OrderId;
                            int OrderDetailsId = 0;
                            if (data.OfferType != "BillDiscount_FreeItem")
                            {
                                OrderDetailsId = objOrderMaster.orderDetails.Any(x => x.FreeWithParentItemId > 0 && x.FreeWithParentItemId == data.itemId) ?
                                    objOrderMaster.orderDetails.FirstOrDefault(x => x.FreeWithParentItemId > 0 && x.FreeWithParentItemId == data.itemId).OrderDetailsId : 0;
                            }
                            else
                            {
                                if (offerdata != null && (offerdata.IsBillDiscountFreebiesItem || offerdata.IsBillDiscountFreebiesValue))
                                {
                                    OrderDetailsId = objOrderMaster.orderDetails.FirstOrDefault(x => x.ItemId == data.FreeItemId && x.UnitPrice == 0.0001 && x.OfferId.HasValue && x.OfferId.Value == data.ReferOfferId && x.IsFreeItem == true).OrderDetailsId;
                                }
                                else
                                {
                                    OrderDetailsId = objOrderMaster.orderDetails.FirstOrDefault(x => x.ItemId == data.FreeItemId && x.UnitPrice == 0.0001).OrderDetailsId;
                                }
                            }

                            data.OrderDetailsId = OrderDetailsId;


                        }

                        context.OfferItemDb.AddRange(offerItemsList);
                    }



                    //Update OrderId in FlashDealItemConsumedDB
                    if (flashDealItemConsumedList != null && flashDealItemConsumedList.Any())
                    {
                        // FlashDealOrderId = flashDealItemConsumedList.Select(x => x.FlashDealItemConsumedId).ToList();
                        foreach (var FlashDealOrderIdData in flashDealItemConsumedList)
                        {
                            FlashDealOrderIdData.OrderId = objOrderMaster.OrderId;
                        }
                        context.FlashDealItemConsumedDB.AddRange(flashDealItemConsumedList);
                    }

                    #endregion

                    #region Deal Item Update
                    if (sc.itemDetails.Any(x => x.OfferCategory == 3) && dbdealItems != null && dbdealItems.Any())
                    {
                        var minOrderqtys = itemMastersList.Where(x => dealItemIds.Contains(x.ItemId)).Select(x => x.MinOrderQty).ToList();
                        var dealitemdetails = itemMastersList.Where(x => dealItemIds.Contains(x.ItemId)).Select(x => new { x.MinOrderQty, x.ItemId, x.ItemMultiMRPId }).ToList();
                        var UpdateDealItem = dbdealItems.Where(x => dealItemMultiMRPIds.Contains(x.ItemMultiMRPId) && minOrderqtys.Contains(x.MinOrderQty)).ToList();

                        foreach (var item in UpdateDealItem)
                        {
                            var id = dealitemdetails.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty)?.ItemId;
                            if (id.HasValue)
                            {
                                var dealitem = sc.itemDetails.FirstOrDefault(x => x.OfferCategory == 3 && x.ItemId == id.Value);
                                item.TotalConsume += dealitem.qty;
                                item.ModifiedDate = indianTime;
                                context.Entry(item).State = EntityState.Modified;
                            }

                        }
                    }
                    #endregion

                    #region PaymentMode entry

                    if (!string.IsNullOrEmpty(sc.paymentThrough))
                    {
                        var paymentThroughs = sc.paymentThrough.Split(',').ToList();

                        if (paymentThroughs.Count == 1 && paymentThroughs.Any(x => x.ToLower() == "cash" || x.ToLower() == "rtgs/neft"))
                        {
                            string paymode = "RTGS/NEFT";
                            if (paymentThroughs.Any(x => x.ToLower() == "cash"))
                                paymode = "Cash";

                            sendNotification = true;
                            context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                            {
                                amount = Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero),
                                CreatedDate = indianTime,
                                currencyCode = "INR",
                                OrderId = objOrderMaster.OrderId,
                                PaymentFrom = paymode,
                                status = "Success",
                                statusDesc = "Order Place",
                                UpdatedDate = indianTime,
                                IsRefund = false
                            });
                        }
                        if ((paymentThroughs.Count == 2
                            && paymentThroughs.Any(x => x.ToLower() == "cash")
                            && paymentThroughs.Any(x => x.ToLower() == "paylater"))
                            || (sc.PaylaterAmount > 0 && paymentThroughs.Any(x => x.ToLower() == "paylater")))
                        {

                            paymentThroughs.ForEach(paylimit =>
                            {
                                string payLatertxnId = "";
                                string paymode = "PayLater";
                                payLatertxnId = "P" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                                if ("cash" == paylimit.ToLower())
                                    paymode = "Cash";

                                PaymentResponseRetailerApp paylimitAdd = new PaymentResponseRetailerApp
                                {
                                    amount = Math.Round(paymode == "Cash" ? sc.CODAmount : sc.PaylaterAmount, 0, MidpointRounding.AwayFromZero),
                                    CreatedDate = indianTime,
                                    currencyCode = "INR",
                                    OrderId = objOrderMaster.OrderId,
                                    GatewayTransId = paymode == "PayLater" ? payLatertxnId : "",
                                    PaymentFrom = paymode,
                                    status = "Success",
                                    statusDesc = "Order Place",
                                    UpdatedDate = indianTime,
                                    IsRefund = false,
                                    IsOnline = paymode == "Cash" ? false : true
                                };
                                context.PaymentResponseRetailerAppDb.Add(paylimitAdd);
                                context.Commit();
                                if (paymode == "PayLater")
                                {
                                    context.PayLaterCollectionDb.Add(new Model.CashManagement.PayLaterCollection
                                    {
                                        PaymentResponseRetailerAppId = paylimitAdd.id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Amount = Math.Round(paymode == "Cash" ? sc.CODAmount : sc.PaylaterAmount, 0, MidpointRounding.AwayFromZero),
                                        OrderId = objOrderMaster.OrderId,
                                        CreatedBy = cust.CustomerId,
                                        CreatedDate = indianTime,
                                        Status = Convert.ToInt32(PayCollectionEnum.Pending),
                                        StoreId = objOrderMaster.orderDetails.Distinct().FirstOrDefault().StoreId
                                    });
                                }
                            });
                        }

                    }

                    if (string.IsNullOrEmpty(sc.paymentThrough))
                    {
                        sendNotification = true;
                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                        {
                            amount = Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero),
                            CreatedDate = indianTime,
                            currencyCode = "INR",
                            OrderId = objOrderMaster.OrderId,
                            PaymentFrom = "Cash",
                            status = "Success",
                            statusDesc = "Order Place",
                            UpdatedDate = indianTime,
                            IsRefund = false
                        });


                    }

                    if (sc.paymentThrough == "Gullak" && sc.GulkAmount > 0)
                    {
                        GtxnId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                        sc.GulkAmount = objOrderMaster.GrossAmount;
                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                        {
                            amount = Math.Round(sc.GulkAmount, 0),
                            CreatedDate = indianTime,
                            currencyCode = "INR",
                            OrderId = objOrderMaster.OrderId,
                            PaymentFrom = "Gullak",
                            status = "Success",
                            statusDesc = "Order Place",
                            UpdatedDate = indianTime,
                            IsRefund = false,
                            IsOnline = true,
                            GatewayTransId = GtxnId,
                            GatewayOrderId = customerGullak.Id.ToString()
                        });

                        context.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                        {
                            CreatedDate = indianTime,
                            CreatedBy = customerGullak.CustomerId,
                            Comment = "Order Placed : " + objOrderMaster.OrderId.ToString(),
                            Amount = (-1) * Math.Round(sc.GulkAmount, 0),
                            GullakId = customerGullak.Id,
                            CustomerId = customerGullak.CustomerId,
                            IsActive = true,
                            IsDeleted = false,
                            ObjectId = objOrderMaster.OrderId.ToString(),
                            ObjectType = "Order"
                        });

                        customerGullak = context.GullakDB.FirstOrDefault(x => x.CustomerId == cust.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                        customerGullak.TotalAmount -= Math.Round(sc.GulkAmount);
                        customerGullak.ModifiedBy = customerGullak.CustomerId;
                        customerGullak.ModifiedDate = indianTime;

                        context.Entry(customerGullak).State = EntityState.Modified;


                    }


                    #endregion


                    #region  Barcode entry
                    string Borderid = Convert.ToString(objOrderMaster.OrderId);
                    string BorderCodeId = Borderid.PadLeft(11, '0');
                    temOrderQBcode code = GetBarcode(BorderCodeId);
                    objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;
                    #endregion


                    //IsCustFirstOrder
                    if (objOrderMaster.Status == "Pending")
                    {
                        if (IsCustFirstOrder(objOrderMaster.CustomerId, context))
                        {
                            objOrderMaster.IsFirstOrder = true;
                        };
                    }

                    //ETA Date Calculation
                    RetailerAppManager retailerApp = new RetailerAppManager();

                    CRMManager mg = new CRMManager();
                    objOrderMaster.IsDigitalOrder = (await mg.IsDigitalCustomer(cust.Skcode));


                    if (objOrderMaster.IsDigitalOrder.Value)
                    {
                        objOrderMaster.Deliverydate = DateTime.Now.Date.AddDays(1);
                    }
                    else
                    {
                        NextETADate = retailerApp.GetNextETADate(objOrderMaster.WarehouseId, objOrderMaster.OrderId);
                        if (NextETADate != null)
                        {

                            objOrderMaster.Deliverydate = new OrderPlaceHelper().GetConsumerETADate(objOrderMaster.WarehouseId); //NextETADate.Min(x => x.NextDeliveryDate);
                        }
                    }
                    #region For IsNextDayDelivery 
                    if (warehouse.IsNextDayDelivery)
                    {
                        if (objOrderMaster.CreatedDate.Hour <= 17)
                        {
                            objOrderMaster.Deliverydate = objOrderMaster.CreatedDate.Date.AddDays(1);
                        }
                        else
                        {
                            objOrderMaster.Deliverydate = objOrderMaster.CreatedDate.Date.AddDays(2);
                        }

                    }
                    #endregion

                    context.Entry(objOrderMaster).State = EntityState.Modified;



                    if (AppConstants.IsUsingLedgerHitOnOnlinePayment && sc.paymentThrough == "Gullak" && sc.GulkAmount > 0 && GtxnId != "")
                    {
                        if (context.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == objOrderMaster.OrderId && z.TransactionId == GtxnId) == null)
                        {
                            OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                            Opdl.OrderId = objOrderMaster.OrderId;
                            Opdl.IsPaymentSuccess = true;
                            Opdl.IsLedgerAffected = "Yes";
                            Opdl.PaymentDate = DateTime.Now;
                            Opdl.TransactionId = GtxnId;
                            Opdl.IsActive = true;
                            Opdl.CustomerId = objOrderMaster.CustomerId;
                            context.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                        }
                    }
                    context.Commit();

                    if (sendNotification)
                    {
                        try
                        {
                            #region for first order
                            if (cust.ordercount == 1)//if this is customer first order
                            {
                                FirstCustomerOrder(cust, objOrderMaster, context);
                            }
                            #endregion

                            if (cust.ordercount > 1)
                            {
                                ForNotification(cust.CustomerId, objOrderMaster.GrossAmount, context);
                            }

                            //If Order Placed By Sales Man then send notification to customer
                            if (objOrderMaster.OrderTakenSalesPersonId > 1)
                            {
                                ForSalesManOrderPlaced(cust, objOrderMaster, context);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);
                        }
                    }
                }
            }

            OrderOutPublisher Publisher = new OrderOutPublisher();
            List<BatchCodeSubjectDc> ZilaOrderQueue = new List<BatchCodeSubjectDc>();
            if (placeOrderResponse.IsSuccess && objOrderMaster.OrderId > 0)
            {
                //PublishZilaOrderQueue
                ZilaOrderQueue.Add(new BatchCodeSubjectDc
                {
                    ObjectDetailId = warehouse.Stateid,
                    ObjectId = Convert.ToInt64(objOrderMaster.OrderId),
                    StockType = "",
                    Quantity = 0,
                    WarehouseId = objOrderMaster.WarehouseId,
                    ItemMultiMrpId = 0
                });

                AngularJSAuthentication.Model.PlaceOrder.PlacedOrderMasterDTM order = new AngularJSAuthentication.Model.PlaceOrder.PlacedOrderMasterDTM();
                order.OrderId = objOrderMaster.OrderId;
                order.CustomerId = objOrderMaster.CustomerId;
                order.Skcode = objOrderMaster.Skcode;
                order.WarehouseId = objOrderMaster.WarehouseId;
                order.TotalAmount = objOrderMaster.TotalAmount;
                var totalamt = objOrderMaster.TotalAmount + objOrderMaster.BillDiscountAmount ?? 0 + objOrderMaster.WalletAmount ?? 0;
                var wheelConfig = context.CompanyWheelConfiguration.FirstOrDefault(x => x.IsStore == true);
                double wheelAmount = 0;
                int lineItemcount = objOrderMaster.orderDetails.Where(x => !x.IsFreeItem).ToList().Count;
                if ((wheelConfig.IsKPPRequiredWheel && cust.IsKPP) || !cust.IsKPP)
                {
                    if (wheelConfig != null && wheelConfig.OrderAmount > 0 && wheelConfig.LineItemCount > 0)
                    {
                        if (lineItemcount >= wheelConfig.LineItemCount)
                        {
                            wheelAmount = Convert.ToInt32(wheelConfig.OrderAmount);
                            order.WheelCount = Convert.ToInt32(Math.Floor(totalamt / wheelAmount));
                        }
                    }
                    else if (wheelConfig != null && wheelConfig.OrderAmount > 0)
                    {
                        wheelAmount = Convert.ToInt32(wheelConfig.OrderAmount);
                        order.WheelCount = Convert.ToInt32(Math.Floor(totalamt / wheelAmount));
                    }
                    else if (wheelConfig != null && wheelConfig.LineItemCount > 0 && lineItemcount >= wheelConfig.LineItemCount)
                    {
                        wheelAmount = 0;
                        order.WheelCount = Convert.ToInt32(Math.Floor(Convert.ToDecimal(lineItemcount) / wheelConfig.LineItemCount));
                    }
                }
                order.WheelList = new List<int>();
                if (order.WheelCount > 0)
                {
                    var companyWheel = context.WheelPointWeightPercentConfig.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                    order.WheelList = NextWheelNumber(companyWheel, order.WheelCount);

                    DialValuePoint DialValue = new DialValuePoint();
                    DialValue.EarnWheelCount = order.WheelCount;
                    DialValue.EarnWheelList = string.Join(",", order.WheelList);
                    DialValue.point = 0;
                    DialValue.OrderId = objOrderMaster.OrderId;
                    DialValue.Skcode = objOrderMaster.Skcode;
                    DialValue.CustomerId = objOrderMaster.CustomerId;
                    DialValue.ShopName = objOrderMaster.ShopName;
                    DialValue.OrderAmount = objOrderMaster.GrossAmount;
                    DialValue.CreatedDate = indianTime;
                    context.DialValuePointDB.Add(DialValue);
                    context.Commit();

                }
                order.WheelAmountLimit = wheelAmount;
                order.DialEarnigPoint = 0;
                decimal KisanDaanAmount = 0;
                if (objOrderMaster.orderDetails != null && objOrderMaster.orderDetails.Any(x => !string.IsNullOrEmpty(x.SubcategoryName) && x.SubcategoryName.ToLower() == "kisan kirana"))
                {
                    var KKAmount = Convert.ToDecimal(objOrderMaster.orderDetails.Where(x => !x.IsFreeItem && !string.IsNullOrEmpty(x.SubcategoryName) && x.SubcategoryName.ToLower() == "kisan kirana").Sum(x => x.qty * x.UnitPrice));
                    if (KKAmount > 0)
                    {
                        var KisanDaan = context.kisanDanMaster.FirstOrDefault(x => (x.OrderFromAmount <= KKAmount && x.OrderToAmount >= KKAmount) && x.IsActive);
                        if (KisanDaan != null)
                        {
                            KisanDaanAmount = KKAmount * KisanDaan.KisanDanPrecentage / 100;
                        }
                    }
                }


                if (!string.IsNullOrEmpty(sc.paymentThrough))//by sudhir
                {
                    order.RSAKey = rsaKey;
                    order.HDFCOrderId = hdfcOrderId;
                    order.eplOrderId = eplOrderId;
                }


                order.ExpectedETADate = objOrderMaster.Deliverydate; //ETA Date                
                order.ETADates = new List<ETADatesDc>();

                #region CommodityLimit
                var companydetail = context.CompanyDetailsDB.Where(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                var orderItemids = itemMastersList.Where(x => itemIds.Contains(x.ItemId) && x.IsCommodity).Select(x => x.ItemId).ToList();
                var orderCommodityAmount = objOrderMaster.orderDetails.Where(x => orderItemids.Contains(x.ItemId)).Sum(x => x.TotalAmt);
                bool isCommodityLimit = false;
                if (companydetail.CommodityLimit > 0 && companydetail.CommodityLimit < orderCommodityAmount)
                {
                    isCommodityLimit = true;
                }
                #endregion
                if (!isCommodityLimit)
                {
                    if (NextETADate != null)
                    {
                        order.ETADates.AddRange(NextETADate.Where(x => x.NextDeliveryDate != objOrderMaster.Deliverydate).Select(x => new ETADatesDc { ETADate = x.NextDeliveryDate }).ToList());
                        order.IsDefaultDeliveryChange = NextETADate.FirstOrDefault().IsDefaultDeliveryChange;
                    }
                    else
                    {
                        order.ETADates.Add(new ETADatesDc { ETADate = order.ExpectedETADate.AddDays(1) });
                        order.ETADates.Add(new ETADatesDc { ETADate = order.ExpectedETADate.AddDays(2) });
                        order.ETADates.Add(new ETADatesDc { ETADate = order.ExpectedETADate.AddDays(3) });
                    }
                }

                #region For IsNextDayDelivery return
                if (warehouse.IsNextDayDelivery)
                {
                    order.ETADates = new List<ETADatesDc>();
                    order.ExpectedETADate = objOrderMaster.Deliverydate;
                }
                #endregion

                placeOrderResponse.OrderMaster = order;
                placeOrderResponse.EarnWalletPoint = BillDiscounts != null && BillDiscounts.Any(x => x.IsUsedNextOrder) ? Convert.ToInt32(BillDiscounts.Where(x => x.IsUsedNextOrder).Sum(x => x.BillDiscountTypeValue)) : 0;
                placeOrderResponse.KisanDaanAmount = KisanDaanAmount;

                if (ZilaOrderQueue != null && ZilaOrderQueue.Any())
                {
                    AsyncContext.Run(() => Publisher.PublishZilaOrderQueue(ZilaOrderQueue));
                }
            }

            return placeOrderResponse;
        }
        private List<int> NextWheelNumber(List<WheelPointWeightPercentConfig> companyWheels, int wheelCount)
        {
            List<int> wheelPoint = new List<int>();
            if (companyWheels != null && companyWheels.Any())
            {
                WeightedRandomBag<int> itemDrops = new WeightedRandomBag<int>();
                foreach (var item in companyWheels)
                {
                    itemDrops.AddEntry(item.WheelPoint, item.WheelWeightPercent);
                }
                for (int i = 0; i < wheelCount; i++)
                {
                    wheelPoint.Add(itemDrops.GetRandom());
                }
            }
            else
                wheelPoint.Add(10);

            return wheelPoint;
        }

        public OrderMaster RewardAndWalletPointForPlacedOrder(AuthContext context, ShoppingCart sc, double offerWalletPoint, OrderMaster objOrderMaster, double rewardpoint, Customer cust, double walletUsedPoint1, double rewPoint, double rewAmount, CashConversion cash)
        {

            #region RewardPoint  calculation 

            if (sc.DialEarnigPoint > 0)
            {
                //rewardpoint = rewardpoint + sc.DialEarnigPoint; user in after add order puch api
                if (offerWalletPoint > 0)
                {
                    objOrderMaster.RewardPoint = rewardpoint + offerWalletPoint;
                }
                else
                {
                    objOrderMaster.RewardPoint = rewardpoint;
                }

            }
            else
            {
                if (offerWalletPoint > 0)
                {
                    objOrderMaster.RewardPoint = rewardpoint + offerWalletPoint;
                }
                else
                {
                    objOrderMaster.RewardPoint = rewardpoint;
                }

            }

            var rpoint = context.RewardPointDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();
            if (rpoint != null)
            {
                if (rewardpoint > 0)
                {

                    rpoint.EarningPoint += rewardpoint;
                    rpoint.UpdatedDate = indianTime;
                    context.Entry(rpoint).State = EntityState.Modified;
                }
            }
            else
            {
                RewardPoint point = new RewardPoint();
                point.CustomerId = cust.CustomerId;
                if (rewardpoint > 0)
                    point.EarningPoint = rewardpoint;
                else
                    point.EarningPoint = 0;
                point.TotalPoint = 0;
                point.UsedPoint = 0;
                point.MilestonePoint = 0;
                point.CreatedDate = indianTime;
                point.UpdatedDate = indianTime;
                point.Deleted = false;
                context.RewardPointDb.Add(point);
            }
            #endregion

            Wallet wallet = context.WalletDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();

            if (objOrderMaster.RewardPoint > 0 || walletUsedPoint1 > 0)
            {
                var rpointWarehouse = context.WarehousePointDB.Where(c => c.WarehouseId == objOrderMaster.WarehouseId).SingleOrDefault();
                int fnlAmount = Convert.ToInt32((objOrderMaster.GrossAmount / cash.rupee) * cash.point);
                if (rpointWarehouse != null)
                {


                    if (walletUsedPoint1 > 0 && wallet != null && wallet.TotalAmount > 0 && wallet.TotalAmount >= sc.walletPointUsed)
                    {

                        if (fnlAmount > walletUsedPoint1)
                        {
                            rpointWarehouse.availablePoint -= walletUsedPoint1;
                            rpointWarehouse.UsedPoint += walletUsedPoint1;
                            rewPoint = walletUsedPoint1;
                            walletUsedPoint1 = 0;
                        }
                        else
                        {
                            rpointWarehouse.availablePoint -= rewPoint;
                            rpointWarehouse.UsedPoint += rewPoint;
                            walletUsedPoint1 -= fnlAmount;
                            rewPoint = fnlAmount;
                        }

                        objOrderMaster.walletPointUsed = rewPoint;
                        try

                        {
                            rewAmount = ((rewPoint / cash.point) * cash.rupee);
                            objOrderMaster.WalletAmount = rewAmount;
                        }
                        catch (Exception e)
                        {

                            objOrderMaster.WalletAmount = 0;
                        }
                    }
                    else
                    {
                        objOrderMaster.WalletAmount = 0;
                        objOrderMaster.walletPointUsed = 0;
                    }

                    if (objOrderMaster.RewardPoint > 0)
                    {
                        rpointWarehouse.availablePoint += objOrderMaster.RewardPoint.GetValueOrDefault();
                        rpointWarehouse.TotalPoint += objOrderMaster.RewardPoint.GetValueOrDefault();
                    }
                    //WarehousePointDB.Attach(rpointWarehouse);
                    context.Entry(rpointWarehouse).State = EntityState.Modified;
                    //this.SaveChanges();
                }
                else
                {
                    objOrderMaster.WalletAmount = 0;
                    objOrderMaster.walletPointUsed = 0;
                    WarehousePoint wPoint = new WarehousePoint();
                    if (objOrderMaster.RewardPoint > 0)
                    {
                        wPoint.availablePoint += objOrderMaster.RewardPoint.GetValueOrDefault();
                        wPoint.TotalPoint += objOrderMaster.RewardPoint.GetValueOrDefault();
                    }
                    wPoint.WarehouseId = objOrderMaster.WarehouseId;
                    wPoint.CompanyId = objOrderMaster.CompanyId;
                    wPoint.UsedPoint = 0;
                    context.WarehousePointDB.Add(wPoint);
                    //this.SaveChanges();
                }
            }


            objOrderMaster.GrossAmount = System.Math.Round((objOrderMaster.GrossAmount - rewAmount), 0);
            objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - rewAmount;

            if (sc.walletPointUsed > 0)
            {

                var rpoint1 = context.RewardPointDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();
                //var WData = WalletDb.Where(x => x.CustomerId == cust.CustomerId).SingleOrDefault();
                if (rpoint1 != null)
                {
                    if (wallet != null && wallet.TotalAmount > 0 && wallet.TotalAmount >= sc.walletPointUsed)
                    {
                        rpoint1.UsedPoint += sc.walletPointUsed;
                        rpoint1.UpdatedDate = indianTime;
                        context.Entry(rpoint1).State = EntityState.Modified;
                    }
                }

                CustomerWalletHistory CWH = new CustomerWalletHistory();
                if (wallet != null && wallet.TotalAmount > 0 && wallet.TotalAmount >= sc.walletPointUsed)
                {
                    CWH.WarehouseId = cust.Warehouseid ?? 0;
                    CWH.CompanyId = cust.CompanyId ?? 0;
                    CWH.CustomerId = wallet.CustomerId;
                    CWH.Through = "Used On Order";
                    CWH.NewOutWAmount = sc.walletPointUsed;
                    CWH.TotalWalletAmount = wallet.TotalAmount - sc.walletPointUsed;
                    CWH.TotalEarningPoint = rpoint1.EarningPoint;
                    CWH.CreatedDate = indianTime;
                    CWH.UpdatedDate = indianTime;
                    CWH.OrderId = objOrderMaster.OrderId;
                    context.CustomerWalletHistoryDb.Add(CWH);

                    //update in wallet
                    wallet.TotalAmount -= sc.walletPointUsed;
                    wallet.TransactionDate = indianTime;
                    context.Entry(wallet).State = EntityState.Modified;

                }
            }

            return objOrderMaster;
        }

        private OrderMaster PrepareOrderMasterToInsertHDFC(AuthContext context, Warehouse warehouse, ShoppingCart sc, Customer cust)
        {
            OrderMaster objOrderMaster = new OrderMaster();
            objOrderMaster.CompanyId = warehouse.CompanyId;
            objOrderMaster.WarehouseId = warehouse.WarehouseId;
            objOrderMaster.WarehouseName = warehouse.WarehouseName;
            objOrderMaster.CustomerCategoryId = 2;
            objOrderMaster.Status = sc.paymentThrough.Trim().ToLower() == "cash" ? "Pending" : "Payment Pending";

            objOrderMaster.CustomerName = cust.Name;
            objOrderMaster.ShopName = cust.ShopName;
            objOrderMaster.LandMark = cust.LandMark;
            objOrderMaster.Customerphonenum = cust.Mobile;
            objOrderMaster.Lat = sc.Lat;
            objOrderMaster.Lng = sc.Lng;

            if (cust.BillingAddress == null && cust.ShippingAddress != null)
            {
                objOrderMaster.BillingAddress = cust.ShippingAddress;
            }
            else
            {
                objOrderMaster.BillingAddress = cust.BillingAddress;
            }
            if (cust.ShippingAddress == null && cust.BillingAddress != null)
            {
                objOrderMaster.ShippingAddress = cust.BillingAddress;
            }
            else
            {
                objOrderMaster.ShippingAddress = cust.ShippingAddress;
            }

            objOrderMaster.Skcode = cust.Skcode;
            objOrderMaster.Tin_No = cust.RefNo;
            objOrderMaster.CustomerType = cust.CustomerType;
            objOrderMaster.CustomerId = cust.CustomerId;
            objOrderMaster.CityId = warehouse.Cityid;

            // MRP-Actual Price
            objOrderMaster.Savingamount = System.Math.Round(sc.Savingamount, 2);
            objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
            objOrderMaster.OnlineServiceTax = sc.OnlineServiceTax;
            var clstr = context.Clusters.Where(x => x.ClusterId == cust.ClusterId).SingleOrDefault();
            if (clstr != null)
            {
                objOrderMaster.ClusterName = clstr.ClusterName;
            }
            People p = new People();
            if (sc.SalesPersonId == 0)
            {
                objOrderMaster.OrderTakenSalesPersonId = 0;
                objOrderMaster.OrderTakenSalesPerson = "Self";
            }
            else
            {
                p = context.Peoples.Where(x => x.PeopleID == sc.SalesPersonId && x.Deleted == false && x.Active == true).SingleOrDefault();
                if (p != null)
                {
                    objOrderMaster.OrderTakenSalesPersonId = p.PeopleID;
                    objOrderMaster.OrderTakenSalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                }

                else
                {
                    objOrderMaster.OrderTakenSalesPersonId = 0;
                    objOrderMaster.OrderTakenSalesPerson = "Self";
                }
            }
            objOrderMaster.active = true;
            objOrderMaster.CreatedDate = indianTime;
            //ETADate 
            objOrderMaster.Deliverydate = indianTime.AddHours(48);

            objOrderMaster.UpdatedDate = indianTime;
            objOrderMaster.Deleted = false;
            return objOrderMaster;
        }

        private PlaceOrderResponse ValidateShoppingCartHDFC(AuthContext context, ShoppingCart cart, Warehouse warehouse, Customer cust, List<AppHomeItem> appHomeItems, List<int> cartItemIds, List<ItemMaster> itemMastersList, List<ItemMaster> FreeitemsList, List<ItemLimitMaster> itemLimits, List<PrimeItemDetail> PrimeItemDetails, List<DealItem> DealItems, List<ItemScheme> itemPTR, string LimitCheck_GUID, out OrderMaster objOrderMaster, out List<BillDiscount> BillDiscounts, out List<int> ExpireofferId, out bool IsStopActive, out List<BillDiscountFreebiesItemQtyDC> FreeQtyList)
        {
            string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
            var CashConversion = context.CashConversionDb.Where(x => x.IsConsumer == true).FirstOrDefault();
            ExpireofferId = new List<int>();
            string MemberShipName = Common.Constants.AppConstants.MemberShipName;
            int MemberShipHours = Common.Constants.AppConstants.MemberShipHours;
            var placeOrderResponse = new PlaceOrderResponse { IsSuccess = true, Message = string.Empty, cart = cart };
            objOrderMaster = new OrderMaster();
            objOrderMaster = PrepareOrderMasterToInsertHDFC(context, warehouse, cart, cust);
            objOrderMaster.orderDetails = new List<OrderDetails>();
            BillDiscounts = new List<BillDiscount>();
            FreeQtyList = new List<BillDiscountFreebiesItemQtyDC>();
            double finaltotal = 0;
            double finalTaxAmount = 0;
            double finalSGSTTaxAmount = 0;
            double finalCGSTTaxAmount = 0;
            double finalGrossAmount = 0;
            double finalTotalTaxAmount = 0;
            //cess 
            double finalCessTaxAmount = 0;
            List<FlashDealItemConsumed> FlashDealItemConsumed = null;
            List<int> itemids = itemMastersList.Select(x => x.ItemId).ToList();
            var apphomeitem = appHomeItems.Where(x => itemids.Contains(x.ItemId) && x.IsFlashDeal == true).FirstOrDefault();
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
            IsStopActive = false;
            if (!cust.Active)
            {
                var Ordersubcatids = itemMastersList.Select(x => x.SubCategoryId).Distinct().ToList();
                var StoreCategorySubCategoryBrandsAllow = StoreCategorySubCategoryBrands.Where(c => Ordersubcatids.Contains(c.SubCategoryId)).ToList();

                IsStopActive = StoreCategorySubCategoryBrandsAllow.All(x => Ordersubcatids.Contains(x.SubCategoryId) && x.AllowInactiveOrderToPending == true);
            }

            if (apphomeitem != null)
            {
                FlashDealItemConsumed = context.FlashDealItemConsumedDB.Where(x => itemids.Contains(x.ItemId) && x.FlashDealId == apphomeitem.id && x.CompanyId == cust.CustomerId).ToList();
            }

            var rewardpoint = 0;
            double unitPrice = 0;
            List<int> offerItemId = new List<int>();
            List<int> FlashDealOrderId = new List<int>();

            List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();
            #region Supplier PO Check item Code
            var OrderItemMultiMRPIds = itemMastersList.Select(x => x.ItemMultiMRPId).Distinct().ToList();
            var itemmultimrp = new DataTable();
            itemmultimrp.Columns.Add("IntValue");
            foreach (var item in OrderItemMultiMRPIds)
            {
                var dr = itemmultimrp.NewRow();
                dr["IntValue"] = item;
                itemmultimrp.Rows.Add(dr);
            }
            var itemmultimrpids = new SqlParameter
            {
                ParameterName = "itemmultiMrpIds",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = itemmultimrp
            };
            var paramStatus = new SqlParameter()
            {
                ParameterName = "@customerId",
                Value = cust.CustomerId
            };

            var supplierItemlist = context.Database.SqlQuery<int>("exec GetSupplierItemForRetailer @customerId , @itemmultiMrpIds", paramStatus, itemmultimrpids).ToList();
            #endregion




            var CustomerChannels = context.CustomerChannelMappings.Where(x => x.CustomerId == cust.CustomerId && x.IsActive == true).ToList(); // by kapil
            Dictionary<string, int> itemclasslst = itemMastersList.Select(x => new { x.Number, WId = warehouse.WarehouseId }).Distinct().ToDictionary(kvp => kvp.Number, kvp => kvp.WId);

            foreach (var item in FreeitemsList)
            {
                if (!itemclasslst.Any(z => z.Key == item.Number && z.Value == warehouse.WarehouseId))
                    itemclasslst.Add(item.Number, warehouse.WarehouseId);
            }

            var ItemClassificationslst = GetItemClassifications(itemclasslst);
            var itemofferids = itemMastersList.Select(x => x.OfferId).Distinct().ToList();
            var offerItems = context.OfferDb.Where(x => itemofferids.Contains(x.OfferId) && x.IsDeleted == false && x.IsActive == true && x.WarehouseId == cust.Warehouseid).ToList();

            foreach (var i in placeOrderResponse.cart.itemDetails.Where(x => x.OfferCategory != 2).Select(x => x))
            {
                unitPrice = 0;
                //try
                //{

                i.IsSuccess = true;
                if (i.qty <= 0)
                {

                    i.IsSuccess = false;
                    i.Message = "Quantity is 0.";
                }
                else if (i.qty != 0 && i.qty > 0)
                {

                    var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();

                    if (items == null)
                    {
                        i.IsSuccess = false;
                        i.Message = "Item is not found.";
                    }
                    else
                    {
                        unitPrice = items.UnitPrice;
                        BackendOrderController backendOrderController = new BackendOrderController();

                        double cprice = backendOrderController.GetConsumerPrice(context, items.ItemMultiMRPId, items.price, items.UnitPrice, warehouse.WarehouseId);
                        items.UnitPrice = SkCustomerType.GetPriceFromType(cust.CustomerType, items.UnitPrice
                                                                    , items.WholeSalePrice ?? 0
                                                                    , items.TradePrice ?? 0, cprice);

                        bool isOffer = items.IsOffer;

                        if (FlashDealItemConsumed != null && FlashDealItemConsumed.Any(x => x.ItemId == items.ItemId) && items.IsOffer)
                        {
                            items.IsOffer = false;
                        }

                        if (!items.active || items.Deleted)
                        {
                            i.IsSuccess = false;
                            i.Message = "Item is not Active.";
                        }

                        if (supplierItemlist != null && supplierItemlist.Any(x => x == items.ItemMultiMRPId))
                        {
                            i.IsSuccess = false;
                            i.Message = "Supplier not eligible to purchase this item!!";
                        }

                        var limit = itemLimits.FirstOrDefault(x => x.ItemNumber == items.ItemNumber && x.ItemMultiMRPId == items.ItemMultiMRPId);

                        if (limit != null && limit.ItemlimitQty < i.qty)
                        {
                            i.IsSuccess = false;
                            i.Message = "Item is not Active.";
                        }
                        if (ConsumerApptype.ToLower() != "consumer")
                        {
                            var mod = Convert.ToDecimal(i.qty) % items.MinOrderQty;
                            if (mod != 0)
                            {
                                i.IsSuccess = false;
                                i.Message = "Item qty is not multiples of min order qty.";
                            }
                        }
                        if (i.IsSuccess && cart.status != "Replace")
                        {
                            var primeitem = PrimeItemDetails != null && PrimeItemDetails.Any(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) ? PrimeItemDetails.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) : null;
                            var dealItem = DealItems != null && i.OfferCategory == 3 && DealItems.Any(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) ? DealItems.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) : null;
                            if (cust.IsPrimeCustomer && primeitem != null)
                            {
                                primeitem.PrimePrice = primeitem.PrimePercent > 0 ? Convert.ToDecimal(items.UnitPrice - (items.UnitPrice * Convert.ToDouble(primeitem.PrimePercent) / 100)) : primeitem.PrimePrice;

                                if (i.UnitPrice != Convert.ToDouble(primeitem.PrimePrice))
                                {
                                    i.IsSuccess = false;
                                    i.Message = "Item " + MemberShipName + " Unit Price has changed.";
                                    i.NewUnitPrice = Convert.ToDouble(primeitem.PrimePrice);
                                }
                            }
                            else if (i.OfferCategory == 3 && dealItem == null)
                            {
                                i.IsSuccess = false;
                                i.Message = "Deal Item has expired.";
                                i.NewUnitPrice = Convert.ToDouble(dealItem.DealPrice);
                            }
                            else if (i.OfferCategory == 3 && dealItem != null)
                            {
                                if (i.UnitPrice != Convert.ToDouble(dealItem.DealPrice))
                                {
                                    i.IsSuccess = false;
                                    i.Message = "Item Deal Price has changed.";
                                    i.NewUnitPrice = Convert.ToDouble(dealItem.DealPrice);
                                }
                            }
                            else if (i.UnitPrice != items.UnitPrice)
                            {
                                i.IsSuccess = false;
                                i.Message = "Item Unit Price has changed.";
                                i.NewUnitPrice = items.UnitPrice;
                            }
                        }

                        if (i.IsSuccess && cart.status != "Replace")
                        {

                            OrderDetails od = new OrderDetails();
                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid))
                            {
                                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid);
                                od.StoreId = store.StoreId;
                                if (CustomerChannels != null && CustomerChannels.Any(x => x.StoreId == od.StoreId))
                                    od.ChannelMasterId = CustomerChannels.FirstOrDefault(x => x.StoreId == store.StoreId).ChannelMasterId;

                                if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId) && CustomerChannels != null)
                                {
                                    if (CustomerChannels.Any(x => x.StoreId == od.StoreId))
                                    {
                                        var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId && a.ChannelMasterId == od.ChannelMasterId);
                                        if (clusterStoreExecutiveDc != null && cust.CustomerType.ToLower() != "consumer")
                                        {
                                            od.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
                                            od.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
                                        }

                                    }

                                }

                            }
                            else
                            {
                                od.StoreId = 0;
                                od.ExecutiveId = 0;
                                od.ExecutiveName = "";
                            }
                            od.CustomerId = cust.CustomerId;
                            od.CustomerName = cust.Name;
                            od.CityId = cust.Cityid;
                            od.Mobile = cust.Mobile;
                            od.OrderDate = indianTime;
                            od.Status = cust.Active ? "Pending" : "Inactive";
                            od.CompanyId = warehouse.CompanyId;
                            od.WarehouseId = warehouse.WarehouseId;
                            od.WarehouseName = warehouse.WarehouseName;
                            od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                            od.ItemId = items.ItemId;
                            od.ItemMultiMRPId = items.ItemMultiMRPId;
                            od.Itempic = items.LogoUrl;
                            od.itemname = items.itemname;
                            od.SupplierName = items.SupplierName;
                            od.SellingUnitName = items.SellingUnitName;
                            od.CategoryName = items.CategoryName;
                            od.SubsubcategoryName = items.SubsubcategoryName;
                            od.SubcategoryName = items.SubcategoryName;
                            od.SellingSku = items.SellingSku;
                            od.City = items.CityName;
                            od.itemcode = items.itemcode;
                            od.HSNCode = items.HSNCode;
                            od.itemNumber = items.Number;
                            od.Barcode = items.itemcode;
                            var primeitem = PrimeItemDetails != null && PrimeItemDetails.Any(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) ? PrimeItemDetails.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) : null;
                            var dealItem = DealItems != null && i.OfferCategory == 3 && DealItems.Any(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) ? DealItems.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) : null;
                            if (cust.IsPrimeCustomer && primeitem != null)
                                od.UnitPrice = Convert.ToDouble(primeitem.PrimePercent > 0 ? Convert.ToDecimal(items.UnitPrice - (items.UnitPrice * Convert.ToDouble(primeitem.PrimePercent) / 100)) : primeitem.PrimePrice);
                            else if (i.OfferCategory == 3 && dealItem != null)
                            {
                                od.UnitPrice = Convert.ToDouble(dealItem.DealPrice);
                            }
                            else
                                od.UnitPrice = items.UnitPrice;

                            od.price = items.price;
                            od.ActualUnitPrice = items.UnitPrice;
                            od.MinOrderQty = !string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer" ? 1 : items.MinOrderQty;
                            od.MinOrderQtyPrice = (od.MinOrderQty * od.UnitPrice);
                            od.qty = Convert.ToInt32(i.qty);
                            od.SizePerUnit = items.SizePerUnit;
                            od.TaxPercentage = items.TotalTaxPercentage;
                            if (od.TaxPercentage >= 0)
                            {
                                od.SGSTTaxPercentage = od.TaxPercentage / 2;
                                od.CGSTTaxPercentage = od.TaxPercentage / 2;
                            }
                            od.Noqty = od.qty; // for total qty (no of items)    
                            od.TotalAmt = System.Math.Round(od.UnitPrice * od.qty, 2);

                            if (items.TotalCessPercentage > 0)
                            {
                                od.TotalCessPercentage = items.TotalCessPercentage;
                                double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

                                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge / 100)) / 100;


                                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                od.CessTaxAmount = (od.AmtWithoutAfterTaxDisc * od.TotalCessPercentage) / 100;
                            }


                            double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;

                            od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge2 / 100)) / 100;
                            od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                            od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
                            if (od.TaxAmmount >= 0)
                            {
                                od.SGSTTaxAmmount = od.TaxAmmount / 2;
                                od.CGSTTaxAmmount = od.TaxAmmount / 2;
                            }
                            //for cess
                            if (od.CessTaxAmount > 0)
                            {
                                double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                //double temp = od.TaxPercentage + od.TotalCessPercentage;
                                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.CessTaxAmount + od.TaxAmmount;
                            }
                            else
                            {
                                od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                            }
                            od.DiscountPercentage = 0;// items.PramotionalDiscount;
                            od.DiscountAmmount = 0;// (od.NetAmmount * items.PramotionalDiscount) / 100;
                            double DiscountAmmount = od.DiscountAmmount;
                            double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            double TaxAmmount = od.TaxAmmount;
                            od.Purchaseprice = items.PurchasePrice;
                            od.CreatedDate = indianTime;
                            od.UpdatedDate = indianTime;
                            od.Deleted = false;
                            var schemeptr = itemPTR.Any(y => y.ItemMultiMRPId == items.ItemMultiMRPId) ? itemPTR.FirstOrDefault(y => y.ItemMultiMRPId == items.ItemMultiMRPId).PTR : 0;
                            if (schemeptr > 0)
                            {
                                od.PTR = Math.Round((schemeptr - 1) * 100, 2); //percent
                            }
                            //////////////////////////////////////////////////////////////////////////////////////////////
                            if (!items.IsOffer)
                            {
                                /// Dream Point Logic && Margin Point relogic from 22April2019
                                int? MP, PP;
                                double xPoint = 0;

                                if (cart.SalesPersonId == 0)
                                {
                                    xPoint = xPointValue * 10; //Customer (0.2 * 10=1)
                                }
                                else
                                {
                                    xPoint = xPointValue * 10; //Salesman (0.2 * 10=1)
                                }

                                if (items.promoPerItems.Equals(null) && items.promoPerItems == null)
                                {
                                    PP = 0;
                                }
                                else
                                {
                                    PP = items.promoPerItems;
                                }
                                if (items.marginPoint.Equals(null) && items.promoPerItems == null)
                                {
                                    MP = 0;
                                }
                                else
                                {
                                    double WithTaxNetPurchasePrice = Math.Round(items.NetPurchasePrice * (1 + (items.TotalTaxPercentage / 100)), 3);//With tax
                                    MP = Convert.ToInt32((items.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                }
                                if (PP > 0 && MP > 0)
                                {
                                    int? PP_MP = PP + MP;
                                    items.dreamPoint = PP_MP;
                                }
                                else if (MP > 0)
                                {
                                    items.dreamPoint = MP;
                                }
                                else if (PP > 0)
                                {
                                    items.dreamPoint = PP;
                                }
                                else
                                {
                                    items.dreamPoint = 0;
                                }
                            }
                            od.marginPoint = items.dreamPoint * od.qty;//dp point multiplyby order qty
                            rewardpoint += od.marginPoint.GetValueOrDefault();


                            ItemClassificationDC objclassificationDc = new ItemClassificationDC();
                            objclassificationDc = ItemClassificationslst.FirstOrDefault(x => x.ItemNumber == items.ItemNumber);
                            od.ABCClassification = objclassificationDc != null ? objclassificationDc.Category : "D";

                            objOrderMaster.orderDetails.Add(od);
                            if (od.CessTaxAmount > 0)
                            {
                                finalCessTaxAmount = finalCessTaxAmount + od.CessTaxAmount;
                                finalTaxAmount = finalTaxAmount + od.TaxAmmount + od.CessTaxAmount;
                            }
                            else
                            {
                                finalTaxAmount = finalTaxAmount + od.TaxAmmount;
                            }
                            finaltotal = finaltotal + od.TotalAmt;
                            finalSGSTTaxAmount = finalSGSTTaxAmount + od.SGSTTaxAmmount;
                            finalCGSTTaxAmount = finalCGSTTaxAmount + od.CGSTTaxAmmount;
                            finalGrossAmount = finalGrossAmount + od.TotalAmountAfterTaxDisc;
                            finalTotalTaxAmount = finalTotalTaxAmount + od.TotalAmountAfterTaxDisc;
                            //If there is any offer then it act as item but all thing will be 0
                            if (i.IsOffer == true && i.FreeItemId > 0 && i.FreeItemqty > 0)
                            {
                                //When there is a free item then we add this item in order detail
                                //Calculate its unit price as 0.
                                ItemMaster Freeitem = FreeitemsList.Where(x => x.ItemId == i.FreeItemId).FirstOrDefault();
                                var freeItemOffer = offerItems.FirstOrDefault(x => x.OfferId == items.OfferId);

                                if (freeItemOffer == null || Freeitem == null)
                                {
                                    i.IsSuccess = false;
                                    i.Message = "Item is not found.";
                                }
                                else
                                {
                                    if (Freeitem.Deleted || freeItemOffer.start > indianTime || freeItemOffer.end < indianTime || !freeItemOffer.IsActive)
                                    {
                                        i.IsSuccess = false;
                                        i.Message = "Free Item expired.";
                                    } // Also check stock
                                    else
                                    {
                                        int? FreeOrderqty = i.FreeItemqty;
                                        if (freeItemOffer.QtyAvaiable < FreeOrderqty)
                                        {
                                            i.IsSuccess = false;
                                            i.Message = "Free Item expired.";
                                        }


                                        if (i.IsSuccess)
                                        {

                                            OrderDetails od1 = new OrderDetails();
                                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == Freeitem.Categoryid && x.SubCategoryId == Freeitem.SubCategoryId && x.BrandId == Freeitem.SubsubCategoryid))
                                            {
                                                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == Freeitem.Categoryid && x.SubCategoryId == Freeitem.SubCategoryId && x.BrandId == Freeitem.SubsubCategoryid);
                                                od1.StoreId = store.StoreId;

                                                if (CustomerChannels != null && CustomerChannels.Any(x => x.StoreId == od1.StoreId))
                                                    od1.ChannelMasterId = CustomerChannels.FirstOrDefault(x => x.StoreId == store.StoreId).ChannelMasterId;

                                                if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId) && CustomerChannels != null)
                                                {
                                                    if (CustomerChannels.Any(x => x.StoreId == od1.StoreId))
                                                    {
                                                        var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId && a.ChannelMasterId == od1.ChannelMasterId);
                                                        if (clusterStoreExecutiveDc != null && cust.CustomerType.ToLower() != "consumer")
                                                        {
                                                            od1.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
                                                            od1.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
                                                        }

                                                    }

                                                }
                                            }
                                            else
                                            {
                                                od1.StoreId = 0;
                                                od1.ExecutiveId = 0;
                                                od1.ExecutiveName = "";
                                            }
                                            od1.CustomerId = cust.CustomerId;
                                            od1.CustomerName = cust.Name;
                                            od1.CityId = cust.Cityid;
                                            od1.Mobile = cust.Mobile;
                                            od1.OrderDate = indianTime;
                                            od1.Status = cust.Active ? "Pending" : "Inactive";
                                            od1.CompanyId = warehouse.CompanyId;
                                            od1.WarehouseId = warehouse.WarehouseId;
                                            od1.WarehouseName = warehouse.WarehouseName;
                                            od1.NetPurchasePrice = Freeitem.NetPurchasePrice + ((Freeitem.NetPurchasePrice * Freeitem.TotalTaxPercentage) / 100);
                                            od1.ItemId = Freeitem.ItemId;
                                            od1.ItemMultiMRPId = Freeitem.ItemMultiMRPId;
                                            od1.Itempic = Freeitem.LogoUrl;
                                            od1.itemname = Freeitem.itemname;
                                            od1.SupplierName = Freeitem.SupplierName;
                                            od1.SellingUnitName = Freeitem.SellingUnitName;
                                            od1.CategoryName = Freeitem.CategoryName;
                                            od1.SubsubcategoryName = Freeitem.SubsubcategoryName;
                                            od1.SubcategoryName = Freeitem.SubcategoryName;
                                            od1.SellingSku = Freeitem.SellingSku;
                                            od1.City = Freeitem.CityName;
                                            od1.itemcode = Freeitem.itemcode;
                                            od1.HSNCode = Freeitem.HSNCode;
                                            od1.itemNumber = Freeitem.Number;
                                            od1.Barcode = Freeitem.itemcode;
                                            od1.MinOrderQty = !string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer" ? 1 : Freeitem.MinOrderQty;
                                            od1.UnitPrice = 0.0001;
                                            od1.price = Freeitem.price;
                                            od1.MinOrderQtyPrice = (od1.MinOrderQty * od1.UnitPrice);
                                            od1.qty = Convert.ToInt32(i.FreeItemqty);
                                            od1.Noqty = od1.qty;
                                            od1.SizePerUnit = items.SizePerUnit;
                                            od1.TaxPercentage = Freeitem.TotalTaxPercentage;
                                            od1.IsFreeItem = true;
                                            od1.FreeWithParentItemId = i.ItemId;
                                            od1.IsDispatchedFreeStock = freeItemOffer.IsDispatchedFreeStock;//true mean stock hit from Freestock
                                            od1.CreatedDate = indianTime;
                                            od1.UpdatedDate = indianTime;
                                            od1.Deleted = false;
                                            od1.marginPoint = 0;
                                            od1.ActualUnitPrice = Freeitem.UnitPrice;
                                            var freeschemeptr = itemPTR.Any(y => y.ItemMultiMRPId == Freeitem.ItemMultiMRPId) ? itemPTR.FirstOrDefault(y => y.ItemMultiMRPId == Freeitem.ItemMultiMRPId).PTR : 0;
                                            if (freeschemeptr > 0)
                                            {
                                                od1.PTR = Math.Round((freeschemeptr - 1) * 100, 2); //percent
                                            }
                                            if (od1.TaxPercentage >= 0)
                                            {
                                                od1.SGSTTaxPercentage = od1.TaxPercentage / 2;
                                                od1.CGSTTaxPercentage = od1.TaxPercentage / 2;
                                            }
                                            od1.Noqty = od1.qty; // for total qty (no of items)    
                                            od1.TotalAmt = System.Math.Round(od1.UnitPrice * od1.qty, 2);

                                            if (Freeitem.TotalCessPercentage > 0)
                                            {
                                                od1.TotalCessPercentage = Freeitem.TotalCessPercentage;
                                                double tempPercentagge = Freeitem.TotalCessPercentage + Freeitem.TotalTaxPercentage;

                                                od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge / 100)) / 100;


                                                od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                                od1.CessTaxAmount = (od1.AmtWithoutAfterTaxDisc * od1.TotalCessPercentage) / 100;
                                            }


                                            double tempPercentagge2f = Freeitem.TotalCessPercentage + Freeitem.TotalTaxPercentage;

                                            od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge2f / 100)) / 100;
                                            od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                            od1.TaxAmmount = (od1.AmtWithoutAfterTaxDisc * od1.TaxPercentage) / 100;
                                            if (od1.TaxAmmount >= 0)
                                            {
                                                od1.SGSTTaxAmmount = od1.TaxAmmount / 2;
                                                od1.CGSTTaxAmmount = od1.TaxAmmount / 2;
                                            }
                                            //for cess
                                            if (od1.CessTaxAmount > 0)
                                            {
                                                double tempPercentagge3 = Freeitem.TotalCessPercentage + Freeitem.TotalTaxPercentage;
                                                od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                                od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                                od1.TotalAmountAfterTaxDisc = od1.AmtWithoutAfterTaxDisc + od1.CessTaxAmount + od1.TaxAmmount;
                                            }
                                            else
                                            {
                                                od1.TotalAmountAfterTaxDisc = od1.AmtWithoutAfterTaxDisc + od1.TaxAmmount;
                                            }
                                            od1.DiscountPercentage = 0;// 
                                            od1.DiscountAmmount = 0;// 

                                            od1.NetAmtAfterDis = (od1.NetAmmount - od1.DiscountAmmount);
                                            od1.Purchaseprice = 0;


                                            objclassificationDc = ItemClassificationslst.FirstOrDefault(x => x.ItemNumber == Freeitem.ItemNumber);
                                            od1.ABCClassification = objclassificationDc != null ? objclassificationDc.Category : "D";
                                            objOrderMaster.orderDetails.Add(od1);
                                            if (od1.CessTaxAmount > 0)
                                            {
                                                finalCessTaxAmount = finalCessTaxAmount + od1.CessTaxAmount;
                                                finalTaxAmount = finalTaxAmount + od1.TaxAmmount + od1.CessTaxAmount;
                                            }
                                            else
                                            {
                                                finalTaxAmount = finalTaxAmount + od1.TaxAmmount;
                                            }
                                            finaltotal = finaltotal + od1.TotalAmt;
                                            finalSGSTTaxAmount = finalSGSTTaxAmount + od1.SGSTTaxAmmount;
                                            finalCGSTTaxAmount = finalCGSTTaxAmount + od1.CGSTTaxAmmount;
                                            finalGrossAmount = finalGrossAmount + od1.TotalAmountAfterTaxDisc;
                                            finalTotalTaxAmount = finalTotalTaxAmount + od1.TotalAmountAfterTaxDisc;


                                        }

                                    }
                                }
                            }
                        }

                        items.IsOffer = isOffer;
                        items.UnitPrice = unitPrice;
                    }
                }
            }

            foreach (var i in placeOrderResponse.cart.itemDetails.Where(x => x.OfferCategory == 2).Select(x => x))
            {
                unitPrice = 0;
                //try
                //{

                i.IsSuccess = true;
                if (cartItemIds.Contains(i.ItemId))
                {
                    if (i.qty <= 0)
                    {

                        i.IsSuccess = false;
                        i.Message = "Quantity is 0.";
                    }
                    else if (i.qty != 0 && i.qty > 0)
                    {
                        var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
                        if (items == null)
                        {
                            i.IsSuccess = false;
                            i.Message = "Item is not found.";
                        }
                        else
                        {
                            unitPrice = items.UnitPrice;
                            bool isOffer = items.IsOffer;
                            BackendOrderController backendOrderController = new BackendOrderController();
                            double cprice = backendOrderController.GetConsumerPrice(context, items.ItemMultiMRPId, items.price, items.UnitPrice, warehouse.WarehouseId);
                            items.UnitPrice = SkCustomerType.GetPriceFromType(cust.CustomerType, items.UnitPrice
                                                                        , items.WholeSalePrice ?? 0
                                                                        , items.TradePrice ?? 0, cprice);
                            if (!items.active || items.Deleted)
                            {
                                i.IsSuccess = false;
                                i.Message = "Item is not Active.";
                            }
                            if (supplierItemlist != null && supplierItemlist.Any(x => x == items.ItemMultiMRPId))
                            {
                                i.IsSuccess = false;
                                i.Message = "Supplier not eligible to purchase this item!!";
                            }
                            var limit = itemLimits.FirstOrDefault(x => x.ItemNumber == items.ItemNumber && x.ItemMultiMRPId == items.ItemMultiMRPId);

                            if (limit != null && limit.ItemlimitQty < i.qty)
                            {
                                i.IsSuccess = false;
                                i.Message = "Item is not Active.";
                            }
                            if (ConsumerApptype.ToLower() != "consumer")
                            {
                                var mod = Convert.ToDecimal(i.qty) % items.MinOrderQty;
                                if (mod != 0)
                                {
                                    i.IsSuccess = false;
                                    i.Message = "Item qty is not multiples of min order qty.";
                                }
                            }

                            if (i.IsSuccess && i.UnitPrice != items.UnitPrice)
                            {
                                i.IsSuccess = false;
                                i.Message = "Item Unit Price has changed.";
                                i.NewUnitPrice = items.UnitPrice;
                            }
                            else
                            {
                                OrderDetails od = new OrderDetails();
                                if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid))
                                {
                                    var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid);
                                    od.StoreId = store.StoreId;
                                    if (CustomerChannels != null && CustomerChannels.Any(x => x.StoreId == od.StoreId))
                                        od.ChannelMasterId = CustomerChannels.FirstOrDefault(x => x.StoreId == store.StoreId).ChannelMasterId;
                                    if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId) && CustomerChannels != null)
                                    {
                                        if (CustomerChannels.Any(x => x.StoreId == od.StoreId))
                                        {
                                            var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId && a.ChannelMasterId == od.ChannelMasterId);
                                            if (clusterStoreExecutiveDc != null && cust.CustomerType.ToLower() != "consumer")
                                            {
                                                od.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
                                                od.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
                                            }

                                        }

                                    }
                                }
                                else
                                {
                                    od.StoreId = 0;
                                    od.ExecutiveId = 0;
                                    od.ExecutiveName = "";
                                }
                                od.CustomerId = cust.CustomerId;
                                od.CustomerName = cust.Name;
                                od.CityId = cust.Cityid;
                                od.Mobile = cust.Mobile;
                                od.OrderDate = indianTime;
                                od.Status = cust.Active ? "Pending" : "Inactive";
                                od.CompanyId = warehouse.CompanyId;
                                od.WarehouseId = warehouse.WarehouseId;
                                od.WarehouseName = warehouse.WarehouseName;
                                od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                                od.ItemId = items.ItemId;
                                od.ItemMultiMRPId = items.ItemMultiMRPId;
                                od.Itempic = items.LogoUrl;
                                od.itemname = items.itemname;
                                od.SupplierName = items.SupplierName;
                                od.SellingUnitName = items.SellingUnitName;
                                od.CategoryName = items.CategoryName;
                                od.SubsubcategoryName = items.SubsubcategoryName;
                                od.SubcategoryName = items.SubcategoryName;
                                od.SellingSku = items.SellingSku;
                                od.City = items.CityName;
                                od.itemcode = items.itemcode;
                                od.HSNCode = items.HSNCode;
                                od.itemNumber = items.Number;
                                od.Barcode = items.itemcode;

                                od.UnitPrice = items.FlashDealSpecialPrice ?? items.UnitPrice;
                                od.ActualUnitPrice = items.UnitPrice;
                                var schemeptr = itemPTR.Any(y => y.ItemMultiMRPId == items.ItemMultiMRPId) ? itemPTR.FirstOrDefault(y => y.ItemMultiMRPId == items.ItemMultiMRPId).PTR : 0;
                                if (schemeptr > 0)
                                {
                                    od.PTR = Math.Round((schemeptr - 1) * 100, 2); //percent
                                }

                                od.price = items.price;
                                od.MinOrderQty = !string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer" ? 1 : items.MinOrderQty;
                                od.MinOrderQtyPrice = (od.MinOrderQty * items.UnitPrice);
                                od.qty = Convert.ToInt32(i.qty);
                                od.SizePerUnit = items.SizePerUnit;
                                od.TaxPercentage = items.TotalTaxPercentage;
                                if (od.TaxPercentage >= 0)
                                {
                                    od.SGSTTaxPercentage = od.TaxPercentage / 2;
                                    od.CGSTTaxPercentage = od.TaxPercentage / 2;
                                }
                                od.Noqty = od.qty; // for total qty (no of items)    
                                od.TotalAmt = System.Math.Round(od.UnitPrice * od.qty, 2);

                                if (items.TotalCessPercentage > 0)
                                {
                                    od.TotalCessPercentage = items.TotalCessPercentage;
                                    double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

                                    od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge / 100)) / 100;


                                    od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                    od.CessTaxAmount = (od.AmtWithoutAfterTaxDisc * od.TotalCessPercentage) / 100;
                                }


                                double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;

                                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge2 / 100)) / 100;
                                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
                                if (od.TaxAmmount >= 0)
                                {
                                    od.SGSTTaxAmmount = od.TaxAmmount / 2;
                                    od.CGSTTaxAmmount = od.TaxAmmount / 2;
                                }
                                //for cess
                                if (od.CessTaxAmount > 0)
                                {
                                    double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                    //double temp = od.TaxPercentage + od.TotalCessPercentage;
                                    od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                    od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
                                    od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.CessTaxAmount + od.TaxAmmount;
                                }
                                else
                                {
                                    od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                                }
                                od.DiscountPercentage = 0;// items.PramotionalDiscount;
                                od.DiscountAmmount = 0;// (od.NetAmmount * items.PramotionalDiscount) / 100;
                                double DiscountAmmount = od.DiscountAmmount;
                                double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                                od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                                double TaxAmmount = od.TaxAmmount;
                                od.Purchaseprice = items.PurchasePrice;
                                od.CreatedDate = indianTime;
                                od.UpdatedDate = indianTime;
                                od.Deleted = false;

                                //////////////////////////////////////////////////////////////////////////////////////////////
                                if (!items.IsOffer)
                                {
                                    /// Dream Point Logic && Margin Point relogic from 22April2019
                                    int? MP, PP;
                                    double xPoint = 0;

                                    if (cart.SalesPersonId == 0)
                                    {
                                        xPoint = xPointValue * 10; //Customer (0.2 * 10=1)
                                    }
                                    else
                                    {
                                        xPoint = xPointValue * 10; //Salesman (0.2 * 10=1)
                                    }

                                    if (items.promoPerItems.Equals(null) && items.promoPerItems == null)
                                    {
                                        PP = 0;
                                    }
                                    else
                                    {
                                        PP = items.promoPerItems;
                                    }
                                    if (items.marginPoint.Equals(null) && items.promoPerItems == null)
                                    {
                                        MP = 0;
                                    }
                                    else
                                    {
                                        double WithTaxNetPurchasePrice = Math.Round(items.NetPurchasePrice * (1 + (items.TotalTaxPercentage / 100)), 3);//With tax
                                        MP = Convert.ToInt32((items.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                                    }
                                    if (PP > 0 && MP > 0)
                                    {
                                        int? PP_MP = PP + MP;
                                        items.dreamPoint = PP_MP;
                                    }
                                    else if (MP > 0)
                                    {
                                        items.dreamPoint = MP;
                                    }
                                    else if (PP > 0)
                                    {
                                        items.dreamPoint = PP;
                                    }
                                    else
                                    {
                                        items.dreamPoint = 0;
                                    }
                                }
                                od.marginPoint = items.dreamPoint * od.qty;//dp point multiplyby order qty
                                rewardpoint += od.marginPoint.GetValueOrDefault();
                                ItemClassificationDC objclassificationDc = new ItemClassificationDC();
                                objclassificationDc = ItemClassificationslst.FirstOrDefault(x => x.ItemNumber == items.ItemNumber);
                                od.ABCClassification = objclassificationDc != null ? objclassificationDc.Category : "D";
                                objOrderMaster.orderDetails.Add(od);
                                if (od.CessTaxAmount > 0)
                                {
                                    finalCessTaxAmount = finalCessTaxAmount + od.CessTaxAmount;
                                    finalTaxAmount = finalTaxAmount + od.TaxAmmount + od.CessTaxAmount;
                                }
                                else
                                {
                                    finalTaxAmount = finalTaxAmount + od.TaxAmmount;
                                }
                                finaltotal = finaltotal + od.TotalAmt;
                                finalSGSTTaxAmount = finalSGSTTaxAmount + od.SGSTTaxAmmount;
                                finalCGSTTaxAmount = finalCGSTTaxAmount + od.CGSTTaxAmmount;
                                finalGrossAmount = finalGrossAmount + od.TotalAmountAfterTaxDisc;
                                finalTotalTaxAmount = finalTotalTaxAmount + od.TotalAmountAfterTaxDisc;

                            }

                            items.IsOffer = isOffer;
                            items.UnitPrice = unitPrice;
                        }

                    }
                }
                else
                {
                    i.IsSuccess = false;
                    i.Message = "Flash Deal Expired!";
                }
            }


            if (!string.IsNullOrEmpty(cart.BillDiscountOfferId))
            {
                List<int> billdiscountofferids = cart.BillDiscountOfferId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                List<Offer> Offers = context.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId) && x.IsDeleted == false && x.IsActive == true && x.end > indianTime).Include(x => x.OfferItemsBillDiscounts).Include(x => x.BillDiscountOfferSections).Include(x => x.OfferBillDiscountRequiredItems).Include(x => x.OfferLineItemValues).ToList();
                List<BillDiscount> offerbilldiscounts = null;
                if (Offers != null && Offers.Count > 0)
                {
                    if (Offers.Any(x => !billdiscountofferids.Contains(x.OfferId)))
                    {
                        List<int> offerids = billdiscountofferids.Where(y => !Offers.Select(x => x.OfferId).Contains(y)).ToList();
                        List<string> offernames = Offers.Where(x => offerids.Contains(x.OfferId)).Select(x => x.OfferName).ToList();
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "following offer expired :" + string.Join(",", offernames);
                        return placeOrderResponse;
                    }

                    if (Offers.Count > 1 && Offers.Any(x => !x.IsUseOtherOffer))
                    {
                        var offernames = Offers.Where(x => !x.IsUseOtherOffer).Select(x => x.OfferName).ToList();
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "following offer can't use with other offers :" + string.Join(",", offernames);
                        return placeOrderResponse;
                    }


                    if (Offers.Count > 0)
                    {
                        offerbilldiscounts = context.BillDiscountDb.Where(x => billdiscountofferids.Contains(x.OfferId) && x.CustomerId == cust.CustomerId /*&& x.OrderId > 0 */&& x.IsActive).ToList();
                        foreach (var item in Offers)
                        {
                            if (!item.OfferUseCount.HasValue)
                                item.OfferUseCount = 1000;

                            if (!item.IsMultiTimeUse && offerbilldiscounts.Count > 0 && offerbilldiscounts.All(x => x.OfferId == item.OfferId && x.OrderId > 0))
                            {
                                var offernames = Offers.Where(x => !x.IsUseOtherOffer).Select(x => x.OfferName).ToList();
                                placeOrderResponse.IsSuccess = false;
                                placeOrderResponse.Message = "following offer can't use multiple :" + string.Join(",", offernames);
                                return placeOrderResponse;
                            }
                            if (item.IsMultiTimeUse && !item.IsCRMOffer && offerbilldiscounts.Count > 0 && offerbilldiscounts.Count(x => x.OfferId == item.OfferId) >= item.OfferUseCount.Value)
                            {
                                var offernames = Offers.Where(x => !x.IsUseOtherOffer).Select(x => x.OfferName).ToList();
                                placeOrderResponse.IsSuccess = false;
                                placeOrderResponse.Message = "following offer can't use more then " + item.OfferUseCount.ToString() + " :" + string.Join(",", offernames);
                                return placeOrderResponse;
                            }
                        }
                    }
                }
                else
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "following offer expired :" + string.Join(",", billdiscountofferids);
                    return placeOrderResponse;
                }

                if (Offers.Any(x => x.OfferOn == "ScratchBillDiscount"))
                {
                    string offeralreadyuse = "";
                    foreach (var item in Offers.Where(x => x.OfferOn == "ScratchBillDiscount"))
                    {
                        if (context.BillDiscountDb.All(x => x.OfferId == item.OfferId && x.CustomerId == cust.CustomerId && x.OrderId > 0))
                        {
                            if (string.IsNullOrEmpty(offeralreadyuse))
                                offeralreadyuse = item.OfferName;
                            else
                                offeralreadyuse += "," + item.OfferName;
                        }
                    }

                    if (!string.IsNullOrEmpty(offeralreadyuse))
                    {
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "You have already used following scratch card " + offeralreadyuse;
                        return placeOrderResponse;
                    }
                }

                if (placeOrderResponse.cart.itemDetails.All(x => x.IsSuccess) && placeOrderResponse.IsSuccess)
                {
                    //Due to assign cart offer and category on item
                    var PreItem = itemMastersList.Select(x => new { x.ItemId, x.IsOffer, x.OfferCategory }).ToList();
                    foreach (var item in itemMastersList)
                    {
                        var cartitem = placeOrderResponse.cart.itemDetails.FirstOrDefault(p => p.ItemId == item.ItemId);
                        if (cartitem != null)
                        {
                            item.IsOffer = cartitem.IsOffer;
                            item.OfferCategory = cartitem.OfferCategory;
                        }
                    }

                    #region BillDiscount Free Item
                    var freeItemofferId = Offers.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId);

                    List<BillDiscountFreeItem> BillDiscountFreeItems = new List<BillDiscountFreeItem>();
                    List<ItemMaster> BillDiscountOfferFreeitems = new List<ItemMaster>();
                    if (freeItemofferId != null && freeItemofferId.Any())
                    {
                        BillDiscountFreeItems = context.BillDiscountFreeItem.Where(x => freeItemofferId.Contains(x.offerId) && x.RemainingOfferStockQty < x.OfferStockQty).ToList();
                        if (BillDiscountFreeItems != null && BillDiscountFreeItems.Any())
                        {
                            var freeitemids = BillDiscountFreeItems.Select(x => x.ItemId).Distinct().ToList();
                            BillDiscountOfferFreeitems = context.itemMasters.Where(x => freeitemids.Contains(x.ItemId)).ToList();
                        }
                    }
                    #endregion

                    foreach (var Offer in Offers)
                    {
                        var BillDiscount = new BillDiscount();
                        BillDiscount.CustomerId = cust.CustomerId;
                        BillDiscount.OfferId = Offer.OfferId;
                        BillDiscount.BillDiscountType = Offer.OfferOn;
                        double totalamount = 0;
                        var OrderLineItems = 0;

                        var CItemIds = itemMastersList.Select(x => x.ItemId).ToList();
                        if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                        {
                            var classifications = Offer.IncentiveClassification.Split(',').ToList();
                            CItemIds = itemMastersList.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                        }
                        List<int> Itemids = new List<int>();
                        if (Offer.BillDiscountType == "category" && Offer.BillDiscountOfferSections.Any())
                        {

                            var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                            var ids = Offer.BillDiscountOfferSections.Where(x => x.IsInclude).Select(x => x.ObjId).ToList();
                            var notids = Offer.BillDiscountOfferSections.Where(x => !x.IsInclude).Select(x => x.ObjId).ToList();

                            if (cart.APPType == "SalesApp")
                            {
                                Itemids = itemMastersList.Where(x =>
                                (!ids.Any() || ids.Contains(x.Categoryid)) && !notids.Contains(x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                ).Select(x => x.ItemId).ToList();
                            }
                            else
                            {
                                Itemids = itemMastersList.Where(x => (!ids.Any() || ids.Contains(x.Categoryid)) && !notids.Contains(x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                            }
                            if (CItemIds.Any())
                            {
                                Itemids = itemMastersList.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                            }
                            totalamount = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                            var cartItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).ToList() : null;

                            if (Offer.OfferLineItemValues != null && Offer.OfferLineItemValues.Any(x => x.itemValue > 0))
                            {
                                List<int> lineItemValueItemExists = new List<int>();
                                foreach (var item in Offer.OfferLineItemValues.Where(x => x.itemValue > 0))
                                {
                                    int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                    if (ItemId == 0)
                                    {
                                        totalamount = 0;
                                        break;
                                    }
                                    else
                                        lineItemValueItemExists.Add(ItemId);
                                }
                            }

                        }
                        else if (Offer.BillDiscountType == "subcategory" && Offer.BillDiscountOfferSections.Any())
                        {
                            AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                            List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

                            //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                            var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                            if (cart.APPType == "SalesApp")
                            {
                                Itemids = itemMastersList.Where(x =>
                                 (!offerCatSubCats.Where(y => y.IsInclude).Any() || offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                                 && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                ).Select(x => x.ItemId).ToList();
                            }
                            else
                            {
                                Itemids = itemMastersList.Where(x =>
                                (!offerCatSubCats.Where(y => y.IsInclude).Any() || offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                                 && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                            }
                            if (CItemIds.Any())
                            {
                                Itemids = itemMastersList.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                            }
                            totalamount = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                            var cartItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).ToList() : null;

                            if (Offer.OfferLineItemValues != null && Offer.OfferLineItemValues.Any(x => x.itemValue > 0))
                            {
                                List<int> lineItemValueItemExists = new List<int>();
                                foreach (var item in Offer.OfferLineItemValues.Where(x => x.itemValue > 0))
                                {
                                    int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                    if (ItemId == 0)
                                    {
                                        totalamount = 0;
                                        break;
                                    }
                                    else
                                        lineItemValueItemExists.Add(ItemId);
                                }
                            }
                        }
                        else if (Offer.BillDiscountType == "brand" && Offer.BillDiscountOfferSections.Any())
                        {
                            // var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                            var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                            AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                            List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);
                            if (cart.APPType == "SalesApp")
                            {
                                Itemids = itemMastersList.Where(x =>
                                (
                                 !offerCatSubCats.Where(y => y.IsInclude).Any() ||
                                offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                )
                                && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                ).Select(x => x.ItemId).ToList();
                            }
                            else
                            {
                                Itemids = itemMastersList.Where(x =>
                                (
                                 !offerCatSubCats.Where(y => y.IsInclude).Any() ||
                                offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                )
                                && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                            }
                            if (CItemIds.Any())
                            {
                                Itemids = itemMastersList.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                            }
                            totalamount = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                            var cartItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).ToList() : null;

                            if (Offer.OfferLineItemValues != null && Offer.OfferLineItemValues.Any(x => x.itemValue > 0))
                            {
                                List<int> lineItemValueItemExists = new List<int>();
                                foreach (var item in Offer.OfferLineItemValues.Where(x => x.itemValue > 0))
                                {
                                    int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                    if (ItemId == 0)
                                    {
                                        totalamount = 0;
                                        break;
                                    }
                                    else
                                        lineItemValueItemExists.Add(ItemId);
                                }
                            }
                        }
                        else if (Offer.BillDiscountType == "items")
                        {
                            // var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                            var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                            //if (Offer.OfferItemsBillDiscounts.FirstOrDefault().IsInclude)
                            //{
                            //    Itemids = itemMastersList.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                            //}

                            Itemids = itemMastersList.Where(x => (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                               && !itemoutofferlist.Contains(x.ItemId)
                               ).Select(x => x.ItemId).ToList();

                            List<int> incluseItemIds = new List<int>();
                            if (cart.APPType == "SalesApp")
                            {
                                incluseItemIds = itemMastersList.Select(x => x.ItemId).ToList();
                            }
                            else
                            {
                                incluseItemIds = itemMastersList.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                            }
                            if (CItemIds.Any())
                            {
                                Itemids = itemMastersList.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                            }
                            totalamount = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                            var cartItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;

                            if (Offer.OfferLineItemValues != null && Offer.OfferLineItemValues.Any(x => x.itemValue > 0))
                            {
                                List<int> lineItemValueItemExists = new List<int>();
                                foreach (var item in Offer.OfferLineItemValues.Where(x => x.itemValue > 0))
                                {
                                    int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                    if (ItemId == 0)
                                    {
                                        totalamount = 0;
                                        break;
                                    }
                                    else
                                        lineItemValueItemExists.Add(ItemId);
                                }
                            }
                        }
                        else
                        {
                            var catIdoutofferlist = Offer.BillDiscountOfferSections.Where(x => !x.IsInclude).Select(x => x.ObjId).ToList();
                            var catIdinofferlist = Offer.BillDiscountOfferSections.Where(x => x.IsInclude).Select(x => x.ObjId).ToList();

                            // var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
                            //  Itemids = itemMastersList.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();
                            Itemids = itemMastersList.Where(x => (catIdinofferlist.Any() || catIdinofferlist.Contains(x.Categoryid))
                            && !catIdoutofferlist.Contains(x.Categoryid)
                            ).Select(x => x.ItemId).ToList();

                            List<int> incluseItemIds = new List<int>();
                            if (cart.APPType == "SalesApp")
                            {
                                incluseItemIds = itemMastersList.Select(x => x.ItemId).ToList();
                            }
                            else
                            {
                                incluseItemIds = itemMastersList.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                            }

                            if (catIdoutofferlist.Any())
                                incluseItemIds = itemMastersList.Where(x => !catIdoutofferlist.Contains(x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                            if (CItemIds.Any())
                            {
                                incluseItemIds = itemMastersList.Where(x => CItemIds.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)
                                  ).Select(x => x.ItemId).ToList();
                            }
                            totalamount = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : objOrderMaster.orderDetails.Where(x => incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                            OrderLineItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                            var cartItems = Itemids.Any() && CItemIds.Any() ? objOrderMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : objOrderMaster.orderDetails.Where(x => incluseItemIds.Contains(x.ItemId)).ToList();

                            if (cartItems != null && Offer.OfferLineItemValues != null && Offer.OfferLineItemValues.Any(x => x.itemValue > 0))
                            {
                                List<int> lineItemValueItemExists = new List<int>();
                                foreach (var item in Offer.OfferLineItemValues.Where(x => x.itemValue > 0))
                                {
                                    int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
                                    if (ItemId == 0)
                                    {
                                        totalamount = 0;
                                        break;
                                    }
                                    else
                                        lineItemValueItemExists.Add(ItemId);
                                }
                            }
                        }


                        if (Offer.OfferBillDiscountRequiredItems != null && Offer.OfferBillDiscountRequiredItems.Any())
                        {
                            List<BillDiscountRequiredItemDc> BillDiscountRequiredItems = AgileObjects.AgileMapper.Mapper.Map(Offer.OfferBillDiscountRequiredItems).ToANew<List<BillDiscountRequiredItemDc>>();
                            if (BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                            {
                                var ids = BillDiscountRequiredItems.Where(x => x.ObjectType == "brand").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).ToList();
                                AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                                List<BrandCategorySubCategory> BrandCategorySubCategorys = manager.GetCatSubCatByMappingId(ids);
                                foreach (var item in BillDiscountRequiredItems.Where(x => x.ObjectType == "brand"))
                                {
                                    var mappingIds = item.ObjectId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                                    if (BrandCategorySubCategorys.Any(x => mappingIds.Contains(x.BrandCategoryMappingId)))
                                    {
                                        item.ObjectId = string.Join(",", BrandCategorySubCategorys.Where(x => mappingIds.Contains(x.BrandCategoryMappingId)).Select(y => y.SubsubCategoryid + " " + y.SubCategoryId + " " + y.Categoryid).ToList());
                                        //item.SubCategoryId = BrandCategorySubCategorys.FirstOrDefault(x => x.BrandCategoryMappingId == item.ObjectId).SubCategoryId;
                                        //item.CategoryId = BrandCategorySubCategorys.FirstOrDefault(x => x.BrandCategoryMappingId == item.ObjectId).Categoryid;
                                        //item.ObjectId = BrandCategorySubCategorys.FirstOrDefault(x => x.BrandCategoryMappingId == item.ObjectId).SubsubCategoryid;

                                    }
                                }
                            }
                            var objectIds = BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(z => Convert.ToInt32(z))).Distinct().ToList();

                            if (BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                            {
                                objectIds.AddRange(itemMastersList.Where(x => BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                            }
                            bool IsRequiredItemExists = true;
                            var cartrequiredItems = objOrderMaster.orderDetails.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0);
                            foreach (var reqitem in BillDiscountRequiredItems)
                            {
                                if (reqitem.ObjectType == "Item")
                                {
                                    var mrpIds = reqitem.ObjectId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                                    var cartitem = cartrequiredItems.Where(x => mrpIds.Contains(x.ItemMultiMRPId));
                                    if (cartitem != null && cartitem.Any())
                                    {
                                        if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitem.Sum(x => x.qty))
                                        {
                                            IsRequiredItemExists = false;
                                            break;
                                        }
                                        else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitem.Sum(x => x.qty * x.UnitPrice))
                                        {
                                            IsRequiredItemExists = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        IsRequiredItemExists = false;
                                        break;
                                    }
                                }
                                else if (reqitem.ObjectType == "brand")
                                {
                                    var objIds = reqitem.ObjectId.Split(',').Select(x => x).ToList();
                                    var multiMrpIds = itemMastersList.Where(x => objIds.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
                                    var cartitems = cartrequiredItems.Where(x => multiMrpIds.Contains(x.ItemMultiMRPId));
                                    if (cartitems != null && cartitems.Any())
                                    {
                                        if (reqitem.ValueType.ToLower() == "qty" && reqitem.ObjectValue > cartitems.Sum(x => x.qty))
                                        {
                                            IsRequiredItemExists = false;
                                            break;
                                        }
                                        else if (reqitem.ValueType.ToLower() == "value" && reqitem.ObjectValue > cartitems.Sum(x => x.qty * x.UnitPrice))
                                        {
                                            IsRequiredItemExists = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        IsRequiredItemExists = false;
                                        break;
                                    }

                                }
                            }
                            if (!IsRequiredItemExists)
                            {
                                totalamount = 0;
                            }
                        }

                        double MaxBillAmount = Offer.MaxBillAmount;
                        double MinBillAmount = Offer.BillAmount;
                        if (Offer.OfferOn == "ScratchBillDiscount" && Offer.BillDiscountOfferOn == "DynamicAmount" && offerbilldiscounts.Any(x => x.OfferId == Offer.OfferId && x.OrderId == 0))
                        {
                            MaxBillAmount = offerbilldiscounts.Where(x => x.OfferId == Offer.OfferId && x.OrderId == 0).OrderBy(x => x.Id).FirstOrDefault().MaxOrderAmount;
                            MinBillAmount = offerbilldiscounts.Where(x => x.OfferId == Offer.OfferId && x.OrderId == 0).OrderBy(x => x.Id).FirstOrDefault().MinOrderAmount;
                        }

                        if (MaxBillAmount > 0 && totalamount > MaxBillAmount)
                        {
                            totalamount = Offer.MaxBillAmount;
                        }
                        else if (MinBillAmount > totalamount)
                        {
                            totalamount = 0;
                        }

                        if (Offer.LineItem > 0 && Offer.LineItem > OrderLineItems)
                        {
                            totalamount = 0;
                        }

                        if (Offer.BillDiscountOfferOn == "Percentage")
                        {
                            BillDiscount.BillDiscountTypeValue = Offer.DiscountPercentage;
                            BillDiscount.BillDiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                        }
                        else if (Offer.BillDiscountOfferOn == "FreeItem")
                        {
                            #region BillDiscount Free Item
                            BillDiscount.BillDiscountAmount = 0;
                            int FreeWithParentItemId = 0;
                            if (BillDiscountFreeItems.Any(x => x.offerId == Offer.OfferId))
                            {
                                int multiple = 1;
                                if (Offer.IsBillDiscountFreebiesItem)
                                {
                                    int billDisitemQtys = 0;
                                    string freeMainItemNumber = string.Empty;

                                    freeMainItemNumber = context.itemMasters.Where(x => x.ItemId == Offer.itemId).Select(x => x.Number).FirstOrDefault();
                                    billDisitemQtys = objOrderMaster.orderDetails.Where(x => x.itemNumber == freeMainItemNumber && (!x.OfferId.HasValue || x.OfferId <= 0)).Sum(x => x.qty);
                                    multiple = Convert.ToInt32(billDisitemQtys / Offer.MinOrderQuantity);
                                    FreeWithParentItemId = objOrderMaster.orderDetails.FirstOrDefault(x => x.itemNumber == freeMainItemNumber).ItemId;
                                    FreeQtyList.Add(new BillDiscountFreebiesItemQtyDC
                                    {
                                        Offerid = Offer.OfferId,
                                        BillDiscountItemQty = multiple,
                                        BillDiscountValueQty = 0
                                    });
                                }
                                else if (Offer.IsBillDiscountFreebiesValue)
                                {
                                    double billDisitemValue = 0;
                                    var offerId = new SqlParameter
                                    {
                                        ParameterName = "offerId",
                                        Value = Offer.OfferId
                                    };
                                    List<int> valueofferitemids = context.Database.SqlQuery<int>("exec GetOfferforbilldiscount  @offerId", offerId).ToList();
                                    if (valueofferitemids != null && valueofferitemids.Any())
                                    {
                                        billDisitemValue = objOrderMaster.orderDetails.Where(x => valueofferitemids.Contains(x.ItemId) && !x.IsFreeItem).Sum(x => x.qty * x.UnitPrice);
                                        if (billDisitemValue >= Offer.MaxBillAmount && Offer.MaxBillAmount > 0)
                                        {
                                            billDisitemValue = Offer.MaxBillAmount;
                                        }

                                        multiple = Convert.ToInt32(Convert.ToInt32(billDisitemValue) / Convert.ToInt32(Offer.BillAmount));
                                        FreeQtyList.Add(new BillDiscountFreebiesItemQtyDC
                                        {
                                            Offerid = Offer.OfferId,
                                            BillDiscountItemQty = 0,
                                            BillDiscountValueQty = multiple
                                        });

                                    }

                                }

                                var BillDiscountFreeItem = BillDiscountFreeItems.Where(x => x.offerId == Offer.OfferId).ToList();
                                if (BillDiscountFreeItem != null && BillDiscountFreeItem.Any())
                                {
                                    var freeItems = BillDiscountOfferFreeitems.Where(x => BillDiscountFreeItem.Select(y => y.ItemId).Contains(x.ItemId));
                                    Dictionary<string, int> Freeitemclasslst = freeItems.Select(x => new { x.Number, WId = warehouse.WarehouseId }).ToDictionary(kvp => kvp.Number, kvp => kvp.WId);

                                    var FreeItemClassificationslst = GetItemClassifications(Freeitemclasslst);
                                    if (freeItems != null && freeItems.Any())
                                    {
                                        OrderDetails od1 = null;
                                        foreach (var Freeitem in freeItems)
                                        {
                                            od1 = new OrderDetails();
                                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == Freeitem.Categoryid && x.SubCategoryId == Freeitem.SubCategoryId && x.BrandId == Freeitem.SubsubCategoryid))
                                            {
                                                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == Freeitem.Categoryid && x.SubCategoryId == Freeitem.SubCategoryId && x.BrandId == Freeitem.SubsubCategoryid);
                                                od1.StoreId = store.StoreId;
                                                if (CustomerChannels != null && CustomerChannels.Any(x => x.StoreId == od1.StoreId))
                                                    od1.ChannelMasterId = CustomerChannels.FirstOrDefault(x => x.StoreId == store.StoreId).ChannelMasterId;
                                                if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId) && CustomerChannels != null)
                                                {
                                                    if (CustomerChannels.Any(x => x.StoreId == od1.StoreId))
                                                    {
                                                        var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId && a.ChannelMasterId == od1.ChannelMasterId);
                                                        if (clusterStoreExecutiveDc != null && cust.CustomerType.ToLower() != "consumer")
                                                        {
                                                            od1.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
                                                            od1.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
                                                        }

                                                    }

                                                }
                                            }
                                            else
                                            {
                                                od1.StoreId = 0;
                                                od1.ExecutiveId = 0;
                                                od1.ExecutiveName = "";
                                            }
                                            od1.OfferId = Offer.OfferId;
                                            od1.FreeWithParentItemId = FreeWithParentItemId;
                                            od1.CustomerId = cust.CustomerId;
                                            od1.CustomerName = cust.Name;
                                            od1.CityId = cust.Cityid;
                                            od1.Mobile = cust.Mobile;
                                            od1.OrderDate = indianTime;
                                            od1.Status = cust.Active ? "Pending" : "Inactive";
                                            od1.CompanyId = warehouse.CompanyId;
                                            od1.WarehouseId = warehouse.WarehouseId;
                                            od1.WarehouseName = warehouse.WarehouseName;
                                            od1.NetPurchasePrice = Freeitem.NetPurchasePrice + ((Freeitem.NetPurchasePrice * Freeitem.TotalTaxPercentage) / 100);
                                            od1.ItemId = Freeitem.ItemId;
                                            od1.ItemMultiMRPId = Freeitem.ItemMultiMRPId;
                                            od1.Itempic = Freeitem.LogoUrl;
                                            od1.itemname = Freeitem.itemname;
                                            od1.SupplierName = Freeitem.SupplierName;
                                            od1.SellingUnitName = Freeitem.SellingUnitName;
                                            od1.CategoryName = Freeitem.CategoryName;
                                            od1.SubsubcategoryName = Freeitem.SubsubcategoryName;
                                            od1.SubcategoryName = Freeitem.SubcategoryName;
                                            od1.SellingSku = Freeitem.SellingSku;
                                            od1.City = Freeitem.CityName;
                                            od1.itemcode = Freeitem.itemcode;
                                            od1.HSNCode = Freeitem.HSNCode;
                                            od1.itemNumber = Freeitem.Number;
                                            od1.ActualUnitPrice = Freeitem.UnitPrice;
                                            od1.IsFreeItem = true;
                                            od1.IsDispatchedFreeStock = BillDiscountFreeItem.FirstOrDefault(x => x.ItemId == Freeitem.ItemId).StockType == 2;//true mean stock hit from Freestock
                                            od1.UnitPrice = 0.0001;
                                            od1.price = Freeitem.price;
                                            od1.MinOrderQty = !string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer" ? 1 : Freeitem.MinOrderQty;
                                            od1.MinOrderQtyPrice = od1.MinOrderQty * od1.UnitPrice;
                                            od1.qty = multiple * Convert.ToInt32(BillDiscountFreeItem.FirstOrDefault(x => x.ItemId == Freeitem.ItemId).Qty);
                                            od1.SizePerUnit = 0; //Offer.IsBillDiscountFreebiesItem==true?Offer.OfferId:
                                            od1.TaxPercentage = Freeitem.TotalTaxPercentage;
                                            od1.SGSTTaxPercentage = 0;
                                            od1.CGSTTaxPercentage = 0;
                                            od1.Noqty = od1.qty; // for total qty (no of items)    
                                            od1.TotalAmt = 0;
                                            od1.TotalCessPercentage = 0;
                                            od1.AmtWithoutTaxDisc = 0;
                                            od1.AmtWithoutAfterTaxDisc = 0;
                                            od1.CessTaxAmount = 0;
                                            od1.AmtWithoutTaxDisc = 0;
                                            od1.AmtWithoutAfterTaxDisc = 0;
                                            od1.TaxAmmount = 0;

                                            od1.MinOrderQty = 1;
                                            od1.MinOrderQtyPrice = (od1.MinOrderQty * od1.UnitPrice);

                                            od1.DiscountPercentage = 0;
                                            od1.DiscountAmmount = 0;
                                            od1.NetAmtAfterDis = 0;
                                            od1.Purchaseprice = 0;
                                            od1.CreatedDate = indianTime;
                                            od1.UpdatedDate = indianTime;
                                            od1.Deleted = false;
                                            od1.marginPoint = 0;

                                            od1.TaxPercentage = Freeitem.TotalTaxPercentage;
                                            od1.TotalCessPercentage = Freeitem.TotalCessPercentage;

                                            if (od1.TaxPercentage >= 0)
                                            {
                                                od1.SGSTTaxPercentage = od1.TaxPercentage / 2;
                                                od1.CGSTTaxPercentage = od1.TaxPercentage / 2;
                                            }
                                            od1.Noqty = od1.qty; // for total qty (no of items)    
                                            od1.TotalAmt = System.Math.Round(od1.UnitPrice * od1.qty, 2);

                                            if (Freeitem.TotalCessPercentage > 0)
                                            {
                                                od1.TotalCessPercentage = Freeitem.TotalCessPercentage;
                                                double tempPercentagge = Freeitem.TotalCessPercentage + Freeitem.TotalTaxPercentage;
                                                od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge / 100)) / 100;
                                                od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + Freeitem.PramotionalDiscount);
                                                od1.CessTaxAmount = (od1.AmtWithoutAfterTaxDisc * od1.TotalCessPercentage) / 100;
                                            }
                                            double tempPercentagge2f = Freeitem.TotalCessPercentage + Freeitem.TotalTaxPercentage;

                                            od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge2f / 100)) / 100;
                                            od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + Freeitem.PramotionalDiscount);
                                            od1.TaxAmmount = (od1.AmtWithoutAfterTaxDisc * od1.TaxPercentage) / 100;
                                            if (od1.TaxAmmount >= 0)
                                            {
                                                od1.SGSTTaxAmmount = od1.TaxAmmount / 2;
                                                od1.CGSTTaxAmmount = od1.TaxAmmount / 2;
                                            }
                                            //for cess
                                            if (od1.CessTaxAmount > 0)
                                            {
                                                double tempPercentagge3 = Freeitem.TotalCessPercentage + Freeitem.TotalTaxPercentage;
                                                od1.AmtWithoutTaxDisc = ((100 * od1.UnitPrice * od1.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                                od1.AmtWithoutAfterTaxDisc = (100 * od1.AmtWithoutTaxDisc) / (100 + Freeitem.PramotionalDiscount);
                                                od1.TotalAmountAfterTaxDisc = od1.AmtWithoutAfterTaxDisc + od1.CessTaxAmount + od1.TaxAmmount;
                                            }
                                            else
                                            {
                                                od1.TotalAmountAfterTaxDisc = od1.AmtWithoutAfterTaxDisc + od1.TaxAmmount;
                                            }
                                            od1.DiscountPercentage = 0;// 
                                            od1.DiscountAmmount = 0;// 

                                            od1.NetAmtAfterDis = (od1.NetAmmount - od1.DiscountAmmount);
                                            od1.Purchaseprice = 0;

                                            var objclassificationDc = FreeItemClassificationslst.FirstOrDefault(x => x.ItemNumber == Freeitem.Number);
                                            od1.ABCClassification = objclassificationDc != null ? objclassificationDc.Category : "D";
                                            objOrderMaster.orderDetails.Add(od1);
                                            if (od1.CessTaxAmount > 0)
                                            {
                                                finalCessTaxAmount = finalCessTaxAmount + od1.CessTaxAmount;
                                                finalTaxAmount = finalTaxAmount + od1.TaxAmmount + od1.CessTaxAmount;
                                            }
                                            else
                                            {
                                                finalTaxAmount = finalTaxAmount + od1.TaxAmmount;
                                            }
                                            finaltotal = finaltotal + od1.TotalAmt;
                                            finalSGSTTaxAmount = finalSGSTTaxAmount + od1.SGSTTaxAmmount;
                                            finalCGSTTaxAmount = finalCGSTTaxAmount + od1.CGSTTaxAmmount;
                                            finalGrossAmount = finalGrossAmount + od1.TotalAmountAfterTaxDisc;
                                            finalTotalTaxAmount = finalTotalTaxAmount + od1.TotalAmountAfterTaxDisc;
                                            objOrderMaster.orderDetails.Add(od1);
                                        }
                                    }
                                }
                                //}

                            }
                            else
                            {

                                placeOrderResponse.IsSuccess = false;
                                placeOrderResponse.Message = Offer.OfferName + " Offer Expired.";
                                return placeOrderResponse;
                            }
                            #endregion

                        }
                        else if (Offer.BillDiscountOfferOn == "DynamicAmount")
                        {
                            BillDiscount.BillDiscountAmount = offerbilldiscounts.Where(x => x.OfferId == Offer.OfferId && x.OrderId == 0).OrderBy(x => x.Id).FirstOrDefault().BillDiscountAmount;
                            BillDiscount.BillDiscountTypeValue = BillDiscount.BillDiscountAmount;
                        }
                        else if (Offer.BillDiscountOfferOn == "DynamicWalletPoint")
                        {
                            BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble((offerbilldiscounts.FirstOrDefault(x => x.OfferId == Offer.OfferId && x.OrderId == 0).BillDiscountTypeValue) / (CashConversion != null ? CashConversion.point : 10));
                        }
                        else
                        {
                            int WalletPoint = 0;
                            if (Offer.WalletType == "WalletPercentage")
                            {
                                WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * ((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0) / 100)));
                                WalletPoint = WalletPoint * 10;
                            }
                            else
                            {
                                WalletPoint = Convert.ToInt32(Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0);
                            }
                            if (Offer.ApplyOn == "PostOffer")
                            {
                                BillDiscount.BillDiscountTypeValue = WalletPoint;
                                BillDiscount.BillDiscountAmount = 0;
                                BillDiscount.IsUsedNextOrder = true;
                            }
                            else
                            {
                                BillDiscount.BillDiscountTypeValue = Offer.BillDiscountWallet;
                                BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / (CashConversion != null ? CashConversion.point : 10));
                                BillDiscount.IsUsedNextOrder = false;
                            }
                        }
                        if (Offer.MaxDiscount > 0)
                        {
                            var walletmultipler = 1;

                            if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && (Offer.BillDiscountOfferOn != "Percentage" && Offer.BillDiscountOfferOn != "DynamicAmount"))
                            {
                                walletmultipler = 10;
                            }
                            if (Offer.BillDiscountOfferOn != "DynamicAmount")
                            {
                                if (Offer.MaxDiscount * walletmultipler < BillDiscount.BillDiscountAmount)
                                {
                                    BillDiscount.BillDiscountAmount = Offer.MaxDiscount * walletmultipler;
                                }
                                if (Offer.MaxDiscount * walletmultipler < BillDiscount.BillDiscountTypeValue)
                                {
                                    BillDiscount.BillDiscountTypeValue = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                                }
                            }
                        }



                        BillDiscount.IsAddNextOrderWallet = false;
                        BillDiscount.IsMultiTimeUse = Offer.IsMultiTimeUse;
                        BillDiscount.IsUseOtherOffer = Offer.IsUseOtherOffer;
                        BillDiscount.CreatedDate = indianTime;
                        BillDiscount.ModifiedDate = indianTime;
                        BillDiscount.IsActive = Offer.IsActive;
                        BillDiscount.IsDeleted = false;
                        BillDiscount.CreatedBy = cust.CustomerId;
                        BillDiscount.ModifiedBy = cust.CustomerId;
                        BillDiscounts.Add(BillDiscount);
                    }

                    //Due to Re assign db offer and category on item                    
                    foreach (var item in itemMastersList)
                    {
                        var cartitem = PreItem.FirstOrDefault(p => p.ItemId == item.ItemId);
                        if (cartitem != null)
                        {
                            item.IsOffer = cartitem.IsOffer;
                            item.OfferCategory = cartitem.OfferCategory;
                        }
                    }
                }

            }
            if (placeOrderResponse.cart.itemDetails.Any(x => !x.IsSuccess))
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = string.Join(", ", placeOrderResponse.cart.itemDetails.Where(x => !x.IsSuccess).Select(x => x.Message).Distinct());
            }
            else
            {

                #region ItemNetInventoryCheck
                if (objOrderMaster.orderDetails != null && objOrderMaster.orderDetails.Any())
                {
                    var itemInventory = new DataTable();
                    itemInventory.Columns.Add("ItemMultiMRPId");
                    itemInventory.Columns.Add("WarehouseId");
                    itemInventory.Columns.Add("Qty");
                    itemInventory.Columns.Add("isdispatchedfreestock");

                    foreach (var item in objOrderMaster.orderDetails)
                    {
                        var dr = itemInventory.NewRow();
                        dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                        dr["WarehouseId"] = objOrderMaster.WarehouseId;
                        dr["Qty"] = item.qty;
                        dr["isdispatchedfreestock"] = item.IsDispatchedFreeStock;
                        itemInventory.Rows.Add(dr);
                    }
                    var parmitemInventory = new SqlParameter
                    {
                        ParameterName = "ItemNetInventory",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.ItemNetInventory",
                        Value = itemInventory
                    };
                    var ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                    if (ItemNetInventoryDcs != null && ItemNetInventoryDcs.Any(x => !x.isavailable))
                    {
                        var itemmultimrpIds = ItemNetInventoryDcs.Where(x => !x.isavailable).Select(x => x.itemmultimrpid);
                        var itemIds = objOrderMaster.orderDetails.Where(x => itemmultimrpIds.Contains(x.ItemMultiMRPId)).Select(x => x.ItemId).ToList();
                        placeOrderResponse.cart.itemDetails.Where(x => itemIds.Contains(x.ItemId)).ToList().ForEach(x =>
                        {
                            x.IsSuccess = false;
                            x.Message = "Item Out of Stock";
                        });
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "Item Out of Stock";
                    }
                }
                #endregion

            }

            return placeOrderResponse;
        }

        public temOrderQBcode GetBarcode(string OrderId)
        {
            temOrderQBcode obj = new temOrderQBcode();
            try
            {

                string barcode = OrderId;

                //Barcode image into your system
                var barcodeLib = new BarcodeLib.Barcode(barcode);
                barcodeLib.Height = 120;
                barcodeLib.Width = 245;
                barcodeLib.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;//
                barcodeLib.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;//
                System.Drawing.Font font = new System.Drawing.Font("verdana", 12f);//
                barcodeLib.LabelFont = font;
                barcodeLib.IncludeLabel = true;
                barcodeLib.Alignment = BarcodeLib.AlignmentPositions.CENTER;
                barcodeLib.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;//
                Image imeg = barcodeLib.Encode(TYPE.CODE128, barcode);//bytestream
                obj.BarcodeImage = (byte[])(new ImageConverter()).ConvertTo(imeg, typeof(byte[]));

                return obj;
            }

            catch (Exception err)
            {
                return obj;
            }
        }
        public int GetRetailerMinOrder(Customer customer, AuthContext context)
        {
            string ConsumerApptype = GetCustomerAppType();
            int minOrderValue = Convert.ToInt32(ConfigurationManager.AppSettings["MinOrderValue"]);
            if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
            {
                var defaultadd = context.ConsumerAddressDb.FirstOrDefault(x => x.CustomerId == customer.CustomerId && x.Default);
                if (defaultadd != null)
                {
                    customer.Cityid = defaultadd.Cityid;
                    customer.Warehouseid = defaultadd.WarehouseId;
                }
            }

            if (customer != null && customer.Cityid.HasValue && customer.Warehouseid.HasValue)
            {
                MongoDbHelper<RetailerMinOrder> mongoDbHelper = new MongoDbHelper<RetailerMinOrder>();
                var cartPredicate = PredicateBuilder.New<RetailerMinOrder>(x => x.CityId == customer.Cityid && x.WarehouseId == customer.Warehouseid.Value);
                var retailerMinOrder = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "RetailerMinOrder").FirstOrDefault();
                if (retailerMinOrder != null)
                {
                    minOrderValue = retailerMinOrder.MinOrderValue;
                }
                else
                {
                    RetailerMinOrder newRetailerMinOrder = new RetailerMinOrder
                    {
                        CityId = customer.Cityid.Value,
                        WarehouseId = customer.Warehouseid.Value,
                        MinOrderValue = minOrderValue
                    };
                    var result = mongoDbHelper.Insert(newRetailerMinOrder);
                }
            }

            return minOrderValue;
        }
        public bool IsCustFirstOrder(int CustomerId, AuthContext context)
        {
            bool result = false;
            var Custparam = new SqlParameter("@CustomerId", CustomerId);
            int Count = context.Database.SqlQuery<int>("exec IsCustFirstOrder @CustomerId", Custparam).FirstOrDefault();
            if (Count == 0) { result = true; }
            return result;
        }

        private List<ItemClassificationDC> GetItemClassifications(Dictionary<string, int> itemlst)
        {
            using (AuthContext context = new AuthContext())
            {

                DataTable dt = new DataTable();
                dt.Columns.Add("ItemNumber");
                dt.Columns.Add("WarehouseId");

                foreach (var item in itemlst)
                {
                    DataRow dr = dt.NewRow();
                    dr["ItemNumber"] = item.Key;
                    dr["WarehouseId"] = item.Value;
                    dt.Rows.Add(dr);
                }

                var param = new SqlParameter
                {
                    TypeName = "dbo.ItemNumberType",
                    ParameterName = "items",
                    Value = dt
                };

                List<ItemClassificationDC> _result = context.Database.SqlQuery<ItemClassificationDC>("exec [GetItemsClassification] @items", param).ToList();
                return _result;
            }

        }
        public void FirstCustomerOrder(Customer cust, OrderMaster objOrderMaster, AuthContext context)
        {
            double rewardPoints = 150;
            var walt = context.WalletDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();
            if (walt != null)
            {
                var custRewardCityBased = context.CityBaseCustomerRewards.FirstOrDefault(x => x.CityId == cust.Cityid && x.IsActive && !x.IsDeleted && x.StartDate <= indianTime && x.EndDate >= indianTime
                                            && x.RewardType == "FirstOrder");
                rewardPoints = Convert.ToDouble(custRewardCityBased?.Point ?? 150);

                CustomerWalletHistory od = new CustomerWalletHistory();
                od.CustomerId = walt.CustomerId;
                //CustWarehouse custWarehouse = CustWarehouseDB.Where(c => c.CustomerId == walt.CustomerId).SingleOrDefault();
                Customer Customer = context.Customers.Where(c => c.CustomerId == walt.CustomerId).SingleOrDefault();
                od.WarehouseId = cust.Warehouseid ?? 0;
                od.CompanyId = cust.CompanyId ?? 0;
                od.OrderId = objOrderMaster.OrderId;
                od.NewAddedWAmount = rewardPoints;
                od.TotalWalletAmount = walt.TotalAmount + od.NewAddedWAmount;
                od.UpdatedDate = indianTime;
                od.Through = "1st Order";
                od.TransactionDate = indianTime;
                od.CreatedDate = indianTime;
                context.CustomerWalletHistoryDb.Add(od);
                // this.SaveChanges();

                walt.CustomerId = walt.CustomerId;
                if (walt.TotalAmount == 0)
                {
                    walt.TotalAmount = rewardPoints;
                }
                else
                {
                    walt.TotalAmount = walt.TotalAmount + rewardPoints;
                }
                walt.UpdatedDate = indianTime;

                if (custRewardCityBased != null)
                {
                    var rewardedCustomer = new RewardedCustomer
                    {
                        CustomerId = walt.CustomerId,
                        CityBaseCustomerRewardId = custRewardCityBased.Id,
                        CreatedBy = 0,
                        UpdateBy = 0,
                        IsDeleted = false,
                        CreatedDate = indianTime
                    };
                    context.RewardedCustomers.Add(rewardedCustomer);
                }
                context.Entry(walt).State = EntityState.Modified;

                context.Commit();
            }

            BackgroundTaskManager.Run(() => FirstOrderNotification(cust.CustomerId, Convert.ToInt32(rewardPoints), context));
        }
        private  async Task<bool> FirstOrderNotification(int CustomerId, int rewardPoint, AuthContext context)
        {
            bool Result = false;
            try
            {
                AngularJSAuthentication.Model.Notification notification = new AngularJSAuthentication.Model.Notification();
                notification.title = "बधाई हो ! ";
                notification.Message = "शॉपकिराना से जुड़ने के लिए धन्यवाद् ! आपको मिले हैं " + rewardPoint + " ड्रीम पॉइंट्स फ्री ! अब सारे ब्रांड एक जगह और फ्री डिलीवरी 24X7 आपके द्वार ";
                notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";
                var customers = context.Customers.Where(x => x.fcmId != null && x.CustomerId == CustomerId).SingleOrDefault();
                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                //string id11 = ConfigurationManager.AppSettings["FcmApiId"];
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";
                //var objNotification = new
                //{
                //    to = customers.fcmId,
                //    CustId = customers.CustomerId,
                //    data = new
                //    {
                //        title = notification.title,
                //        body = notification.Message,
                //        icon = notification.Pic,
                //        typeId = customers.CustomerId,
                //        notificationCategory = "",
                //        notificationType = "Non-Actionable"
                //    }
                //};
                //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                //tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                //tRequest.ContentLength = byteArray.Length;
                //tRequest.ContentType = "application/json";
                //using (Stream dataStream = tRequest.GetRequestStream())
                //{
                //    dataStream.Write(byteArray, 0, byteArray.Length);
                //    using (WebResponse tResponse = tRequest.GetResponse())
                //    {
                //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                //        {
                //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                //            {
                //                String responseFromFirebaseServer = tReader.ReadToEnd();
                //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                //                if (response.success == 1)
                //                {
                //                    Console.Write(response);
                //                }
                //                else if (response.failure == 1)
                //                {
                //                    Console.Write(response);
                //                }
                //            }
                //        }
                //    }
                //}
                var data = new FCMData
                {
                    title = notification.title,
                    body = notification.Message,
                    icon = notification.Pic,
                    typeId = customers.CustomerId,
                    notificationCategory = "",
                    notificationType = "Non-Actionable"
                };
                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                var result = await firebaseService.SendNotificationForApprovalAsync(customers.fcmId, data);
                if (result != null)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("FirstOrderNotification : " + ex.ToString());
            }
            return Result;
        }
        internal async Task<bool> ForNotification(int CustomerId, double amount, AuthContext context)
        {
            bool Result = false;
            try
            {
                AngularJSAuthentication.Model.Notification notification = new AngularJSAuthentication.Model.Notification();
                notification.title = "Order Placed Successfully";
                notification.Message = "Dear Customer Your Order Placed of Total Amount=Rs." + amount;
                notification.Pic = "";//"https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";

                var customers = context.Customers.Where(x => x.fcmId != null && x.CustomerId == CustomerId).SingleOrDefault();

                //AddNotification(notification);

                if (customers != null)
                {
                    string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                    //string id11 = ConfigurationManager.AppSettings["FcmApiId"];


                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                    //tRequest.Method = "post";


                    //var objNotification = new
                    //{
                    //    to = customers.fcmId,
                    //    CustId = customers.CustomerId,
                    //    data = new
                    //    {
                    //        title = notification.title,
                    //        body = notification.Message,
                    //        icon = notification.Pic,
                    //        typeId = customers.CustomerId,
                    //        notificationCategory = "",
                    //        notificationType = "Non-Actionable"
                    //    }
                    //};

                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                    //tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
                    //tRequest.ContentLength = byteArray.Length;
                    //tRequest.ContentType = "application/json";
                    //using (Stream dataStream = tRequest.GetRequestStream())
                    //{
                    //    dataStream.Write(byteArray, 0, byteArray.Length);
                    //    using (WebResponse tResponse = tRequest.GetResponse())
                    //    {
                    //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    //        {
                    //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                    //            {
                    //                String responseFromFirebaseServer = tReader.ReadToEnd();
                    //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                    //                if (response.success == 1)
                    //                {
                    //                    Console.Write(response);
                    //                }
                    //                else if (response.failure == 1)
                    //                {
                    //                    Console.Write(response);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    var data = new FCMData
                    {
                        title = notification.title,
                        body = notification.Message,
                        icon = notification.Pic,
                        typeId = customers.CustomerId,
                        notificationCategory = "",
                        notificationType = "Non-Actionable"
                    };
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    var result = await firebaseService.SendNotificationForApprovalAsync(customers.fcmId, data);
                    if (result != null)
                    {
                        Console.Write(result);
                    }
                    else
                    {
                        Console.Write(result);
                    }
                }
            }
            catch (Exception ds)
            {
                logger.Error("Error during customer order notification: " + ds.ToString());
            }
            return Result;
        }
        internal async Task<bool> ForSalesManOrderPlaced(Customer cust, OrderMaster order, AuthContext context)
        {
            bool Result = false;
            try
            {
                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                if (cust != null && cust.fcmId != null)
                {
                    //FCMRequest objNotification = new FCMRequest();
                    //objNotification.to = cust.fcmId;
                    //objNotification.CustId = cust.CustomerId;
                    //objNotification.MessageId = "";
                    var data = new FCMData
                    {
                        title = "Rate our executive !!",
                        body = "Dear Customer please rate our executive. ",
                        icon = "",
                        typeId = 1,   // typeid 1 salesman rating
                        notificationCategory = "Rating",
                        notificationType = "",
                        notificationId = 0,
                        notify_type = "ApphomeBottomCall",
                        url = "api/RetailerApp/GetSalesManRatingOrder/" + order.OrderId
                    };
                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                    //tRequest.Method = "post";
                    //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
                    //Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
                    //tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
                    //tRequest.ContentLength = byteArray.Length;
                    //tRequest.ContentType = "application/json";
                    //using (Stream dataStream = tRequest.GetRequestStream())
                    //{
                    //    dataStream.Write(byteArray, 0, byteArray.Length);
                    //    using (WebResponse tResponse = tRequest.GetResponse())
                    //    {
                    //        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    //        {
                    //            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                    //            {
                    //                String responseFromFirebaseServer = tReader.ReadToEnd();
                    //                FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                    //                if (response.success == 1 && response.results != null && response.results.Any() && !string.IsNullOrEmpty(response.results.FirstOrDefault().message_id))
                    //                {
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    var result = await firebaseService.SendNotificationForApprovalAsync(cust.fcmId, data);
                    if (result != null)
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                }
            }
            catch (Exception ds)
            {
                logger.Error("Error during ForSalesManOrderPlaced: " + ds.ToString());
            }
            return Result;
        }
        public string GetCustomerAppType()
        {
            return HttpContext.Current.Request.Headers.GetValues("customerType") != null ? HttpContext.Current.Request.Headers.GetValues("customerType").FirstOrDefault() : "";//new OrderPlaceHelper().GetCustomerAppType();
        }

        //public static string CustomerAppType
        //{
        //    get
        //    {
        //        if (HttpContext.Current.Request.Headers.AllKeys.Any(x => x == "customerType"))
        //            return HttpContext.Current.Request.Headers.GetValues("customerType").FirstOrDefault();
        //        else
        //            return "";
        //    }
        //}
        public DateTime GetConsumerETADate(int WarehouseId)
        {
            DateTime ETAdate;
            using (var db = new AuthContext())
            {
                var storeWarehouseOpeningDetails = db.StoreWarehouseOpeningDetailDb.FirstOrDefault(x => x.WarehouseId == WarehouseId && x.DeliveryHours > 0 && x.IsActive == true && x.IsDeleted == false);
                if (storeWarehouseOpeningDetails != null)
                {
                    if (DateTime.Now.TimeOfDay >= storeWarehouseOpeningDetails.OpenDate.TimeOfDay
                        && DateTime.Now.TimeOfDay <= storeWarehouseOpeningDetails.CloseDate.TimeOfDay)
                    {
                        ETAdate = DateTime.Now.AddHours(storeWarehouseOpeningDetails.DeliveryHours);
                    }
                    else
                    {
                        var date = DateTime.Now.AddDays(1);
                        ETAdate = new DateTime(date.Year, date.Month, date.Day,
                            storeWarehouseOpeningDetails.OpenDate.Hour,
                            storeWarehouseOpeningDetails.OpenDate.Minute,
                            storeWarehouseOpeningDetails.OpenDate.Second);
                    }
                }
                else
                {
                    ETAdate = DateTime.Now.AddDays(1);
                }
            }
            return ETAdate;
        }
    }
}