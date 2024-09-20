using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.ShoppingCart;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.CustomerShoppingCart;
using AngularJSAuthentication.API.Helper;
using Hangfire;
using LinqKit;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.DataContracts.Masters;
using System.Configuration;
using AngularJSAuthentication.Common.Helpers;
using Nito.AsyncEx;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using System.Web;

namespace AngularJSAuthentication.API.Controllers.External.RetailerAPP
{
    [RoutePrefix("api/ShoppingCart")]
    public class ShoppingCartController : BaseAuthController
    {
        public string MemberShipName = AppConstants.MemberShipName;
        public int MemberShipHours = AppConstants.MemberShipHours;
        public static List<int> ItemToProcess = new List<int>();
        public double xPointValue = AppConstants.xPoint;
        public bool EnableOtherLanguage = false;
        [Route("AddItem")]
        [HttpPost]
        public async Task<ReturnShoppingCart> InsertCartItem(CartItemDc cartItemDc)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();

            if (!ItemToProcess.Any(x => x == cartItemDc.ItemId))
            {
                ItemToProcess.Add(cartItemDc.ItemId);
                using (var context = new AuthContext())
                {
                    MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
                    var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == cartItemDc.CustomerId && x.WarehouseId == cartItemDc.WarehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    if (cartItemDc.PeopleId > 0)
                    {
                        cartPredicate = cartPredicate.And(x => x.PeopleId == cartItemDc.PeopleId);
                    }
                    else
                    {
                        cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
                    }
                    var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate)).FirstOrDefault();

                    //var itemmaster = await context.itemMasters.FirstOrDefaultAsync(x => x.ItemId == cartItemDc.ItemId);
                    if (customerShoppingCart != null)
                    {
                        UpdateResult result = null;
                        var _collection = mongoDbHelper.mongoDatabase.GetCollection<CustomerShoppingCart>("CustomerShoppingCart");

                        if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == cartItemDc.ItemId && x.IsFreeItem == cartItemDc.IsFreeItem))
                        {
                            var Id = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId && x.IsFreeItem == cartItemDc.IsFreeItem).Id;
                            var filter = Builders<CustomerShoppingCart>.Filter.Where(x => x.Id == customerShoppingCart.Id
                                                                          && x.ShoppingCartItems.Any(i => i.ItemId == cartItemDc.ItemId && i.IsFreeItem == cartItemDc.IsFreeItem));

                            var update = Builders<CustomerShoppingCart>.Update.Set(x => x.ShoppingCartItems.FirstMatchingElement().IsActive, true)
                                                                                         .Set(x => x.ShoppingCartItems.FirstMatchingElement().IsDeleted, false)
                                                                                         .Set(x => x.ShoppingCartItems.FirstMatchingElement().qty, cartItemDc.qty)
                                                                                         .Set(x => x.ShoppingCartItems.FirstMatchingElement().UnitPrice, cartItemDc.UnitPrice)
                                                                                         .Set(x => x.ShoppingCartItems.FirstMatchingElement().IsPrimeItem, cartItemDc.IsPrimeItem)
                                                                                         .Set(x => x.ShoppingCartItems.FirstMatchingElement().ModifiedDate, DateTime.Now)
                                                                                         .Set(x => x.ShoppingCartItems.FirstMatchingElement().ModifiedBy, cartItemDc.CustomerId)
                                                                                         .Set(x => x.ShoppingCartItems.FirstMatchingElement().IsFreeItem, cartItemDc.IsFreeItem)
                                                                                         .Set(x => x.ShoppingCartItems.FirstMatchingElement().IsDealItem, cartItemDc.IsDealItem)
                                                                                         .Set(x => x.ModifiedDate, DateTime.Now)
                                                                                         .Set(x => x.ModifiedBy, cartItemDc.CustomerId);
                            result = _collection.UpdateOneAsync(filter, update).Result;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsActive = true;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).qty = cartItemDc.qty;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).UnitPrice = cartItemDc.UnitPrice;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsPrimeItem = cartItemDc.IsPrimeItem;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsDeleted = false;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).ModifiedDate = DateTime.Now;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).ModifiedBy = cartItemDc.CustomerId;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsFreeItem = cartItemDc.IsFreeItem;
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsDealItem = cartItemDc.IsDealItem;
                        }
                        else
                        {
                            var shoppingCartItem = new ShoppingCartItem
                            {
                                CreatedBy = cartItemDc.CustomerId,
                                CreatedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                IsFreeItem = cartItemDc.IsFreeItem,
                                ModifiedBy = cartItemDc.CustomerId,
                                ItemId = cartItemDc.ItemId,
                                qty = cartItemDc.qty,
                                UnitPrice = cartItemDc.UnitPrice,
                                IsPrimeItem = cartItemDc.IsPrimeItem,
                                IsDealItem = cartItemDc.IsDealItem,
                                TaxAmount = 0
                            };
                            var filter = Builders<CustomerShoppingCart>.Filter.Where(x => x.Id == customerShoppingCart.Id);

                            var update = Builders<CustomerShoppingCart>.Update.Push(x => x.ShoppingCartItems, shoppingCartItem)
                                                                              .Set(x => x.ModifiedDate, DateTime.Now)
                                                                              .Set(x => x.ModifiedBy, cartItemDc.CustomerId);
                            result = await _collection.UpdateOneAsync(filter, update);

                            customerShoppingCart.ShoppingCartItems.Add(new ShoppingCartItem
                            {
                                CreatedBy = cartItemDc.CustomerId,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                                IsFreeItem = cartItemDc.IsFreeItem,
                                ModifiedBy = cartItemDc.CustomerId,
                                ItemId = cartItemDc.ItemId,
                                qty = cartItemDc.qty,
                                UnitPrice = cartItemDc.UnitPrice,
                                IsPrimeItem = cartItemDc.IsPrimeItem,
                                TaxAmount = 0,
                                IsDealItem = cartItemDc.IsDealItem
                                //TaxPercentage = itemmaster.TotalTaxPercentage,
                                //TaxAmount = itemmaster.TotalTaxPercentage > 0 ? (cartItemDc.qty * cartItemDc.UnitPrice) * itemmaster.TotalTaxPercentage / 100 : 0
                            });
                        }
                        returnShoppingCart.Status = result.IsAcknowledged && result.ModifiedCount > 0; //await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart);
                    }
                    else
                    {
                        customerShoppingCart = new CustomerShoppingCart
                        {
                            IsActive = true,
                            CartTotalAmt = 0,
                            CreatedBy = cartItemDc.CustomerId,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            CustomerId = cartItemDc.CustomerId,
                            PeopleId = cartItemDc.PeopleId,
                            DeamPoint = 0,
                            DeliveryCharges = 0,
                            GrossTotalAmt = 0,
                            IsDeleted = false,
                            TotalDiscountAmt = 0,
                            TotalTaxAmount = 0,
                            WalletAmount = 0,
                            WarehouseId = cartItemDc.WarehouseId,
                            ShoppingCartItems = new List<ShoppingCartItem> {
                     new ShoppingCartItem {
                         CreatedBy = cartItemDc.CustomerId,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        IsFreeItem = cartItemDc.IsFreeItem,
                        ModifiedBy = cartItemDc.CustomerId,
                        ItemId = cartItemDc.ItemId,
                        qty = cartItemDc.qty,
                        UnitPrice = cartItemDc.UnitPrice,
                        IsPrimeItem=cartItemDc.IsPrimeItem,
                        IsDealItem=cartItemDc.IsDealItem,
                        //TaxPercentage = itemmaster.TotalTaxPercentage,
                       TaxAmount= 0//itemmaster.TotalTaxPercentage>0? (cartItemDc.qty * cartItemDc.UnitPrice) * itemmaster.TotalTaxPercentage/100:0
                     }
                    }
                        };
                        returnShoppingCart.Status = await mongoDbHelper.InsertAsync(customerShoppingCart);
                    }
                    if (cartItemDc.IsCartRequire)
                    {
                        customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, cartItemDc.lang);
                    }
                    else
                    {
                        customerShoppingCartDc = null;
                        // BackgroundJob.Enqueue(() => RefereshCartSync(customerShoppingCart, cartItemDc.lang));
                    }
                }
                returnShoppingCart.Cart = customerShoppingCartDc;

            }

            ItemToProcess.RemoveAll(x => x == cartItemDc.ItemId);

            return returnShoppingCart;
        }

        private async Task<CustomerShoppingCartDc> RefereshCart(CustomerShoppingCart customerShoppingCart, AuthContext context, string lang)
        {
            string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            List<ShoppingCartItemDc> ShoppingCartItemDcs = new List<ShoppingCartItemDc>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            List<int> BillDiscountItemOfferIds = new List<int>();

            int walletPoint = 0;
            if (customerShoppingCart != null)
            {

                customerShoppingCartDc = new CustomerShoppingCartDc
                {
                    CartTotalAmt = 0,
                    CustomerId = customerShoppingCart.CustomerId,
                    DeamPoint = 0,
                    DeliveryCharges = 0,
                    GrossTotalAmt = 0,
                    TotalDiscountAmt = 0,
                    TotalTaxAmount = 0,
                    WalletAmount = 0,
                    WarehouseId = customerShoppingCart.WarehouseId,
                };
                if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any())
                {
                    var ActiveCustomer = context.Customers.Where(x => x.CustomerId == customerShoppingCart.CustomerId).Include(x => x.ConsumerAddress).FirstOrDefault();
                    if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                    {

                        ActiveCustomer.Active = ActiveCustomer.IsB2CApp;
                        var defaultadd = ActiveCustomer.ConsumerAddress.FirstOrDefault(x => x.Default);
                        var cluster = context.Clusters.FirstOrDefault(x => x.WarehouseId == defaultadd.WarehouseId);
                        ActiveCustomer.ShippingAddress = defaultadd.CompleteAddress;
                        ActiveCustomer.Warehouseid = defaultadd.WarehouseId;
                        ActiveCustomer.City = defaultadd.CityName;
                        ActiveCustomer.Cityid = defaultadd.Cityid;
                        ActiveCustomer.State = defaultadd.StateName;
                        ActiveCustomer.ClusterId = cluster.ClusterId;
                        ActiveCustomer.ZipCode = defaultadd.ZipCode;
                        ActiveCustomer.lat = defaultadd.lat;
                        ActiveCustomer.lg = defaultadd.lng;
                        ActiveCustomer.LandMark = defaultadd.LandMark;
                        ActiveCustomer.ShopName = defaultadd.PersonName;
                        ActiveCustomer.Name = defaultadd.PersonName;
                        ActiveCustomer.CustomerType = ConsumerApptype;
                    }
                    var primeCustomer = context.PrimeCustomers.FirstOrDefault(x => x.CustomerId == ActiveCustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    ActiveCustomer.IsPrimeCustomer = primeCustomer != null && primeCustomer.StartDate <= DateTime.Now && primeCustomer.EndDate >= DateTime.Now;

                    var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == customerShoppingCart.WarehouseId && x.isDeleted == false && x.IsActive && !x.IsDistributor).ToList();

                    var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.Active == false || ActiveCustomer.Deleted == true) ? true : false;
                    List<int> itemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();
                    itemids = context.itemMasters.Where(x => itemids.Contains(x.ItemId) && x.WarehouseId == ActiveCustomer.Warehouseid).Select(x => x.ItemId).ToList();
                    string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + customerShoppingCart.WarehouseId +
                                      " WHERE a.[CustomerId]=" + customerShoppingCart.CustomerId;
                    var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                    string itemIndex = ConfigurationManager.AppSettings["ElasticSearchIndexName"];
                    string query = $"SELECT itemid,active,basecategoryid,basecategoryname,categoryid,categoryname,subcategoryid,subcategoryname,subsubcategoryid,subsubcategoryname,itemlimitqty," +
                                $"isitemlimit,itemnumber,companyid,warehouseid,itemname,price,unitprice,logourl,minorderqty,totaltaxpercentage,isnull(marginpoint,0) marginpoint ,isnull(dreampoint,0) dreampoint,isoffer,unitofquantity," +
                                $"uom,itembasename,deleted,isflashdealused,itemmultimrpid,netpurchaseprice,billlimitqty,issensitive,issensitivemrp,hindiname,isnull(offercategory,0) offercategory,offerstarttime,offerendtime," +
                                $"isnull(offerqtyavaiable,0) offerqtyavaiable,offerqtyconsumed,offerid,offertype,offerwalletpoint,offerfreeitemid,freeitemid,offerpercentage,offerfreeitemname,offerfreeitemimage," +
                                $"offerfreeitemquantity,offerminimumqty,flashdealspecialprice,flashdealmaxqtypersoncantake,distributionprice,distributorshow,itemapptype,isflashdealstart,warehousename," +
                                $"cityid,cityname,rating,margin,dreamPointCal" +
                                $" from {itemIndex} where " +
                                $"  active=true and deleted=false and  isdiscontinued=false and (isitemlimit=false or (isitemlimit=true and itemlimitqty>0 and itemlimitqty-minorderqty>0 )) and (itemapptype=0 or itemapptype=1)";

                    if (itemids != null && itemids.Any())
                    {
                        query += $" and itemid in ({string.Join(",", itemids)})";
                    }


                    // ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemResponse> elasticSqlHelper1 = new ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemResponse>();

                    //var dbItems = AsyncContext.Run(async () => (await elasticSqlHelper1.GetListAsync(query)).ToList());

                    //var newdata = dbItems.Select(x => new factoryItemdata
                    //{
                    //    active = x.active,
                    //    BaseCategoryId = x.basecategoryid,
                    //    BillLimitQty = x.billlimitqty,
                    //    Categoryid = x.categoryid,
                    //    CompanyId = x.companyid,                       
                    //    Deleted = x.deleted,
                    //    DistributionPrice = x.distributionprice,
                    //    DistributorShow = x.distributorshow,
                    //    marginPoint = x.marginpoint,
                    //    dreamPoint = x.dreampoint,
                    //    FlashDealMaxQtyPersonCanTake = x.flashdealmaxqtypersoncantake,
                    //    FlashDealSpecialPrice = x.flashdealspecialprice,
                    //    FreeItemId = x.freeitemid,
                    //    HindiName = x.hindiname,
                    //    IsFlashDealUsed = x.isflashdealused,
                    //    IsItemLimit = x.isitemlimit,
                    //    IsOffer = x.isoffer,
                    //    IsSensitive = x.issensitive,
                    //    IsSensitiveMRP = x.issensitivemrp,
                    //    ItemAppType = x.itemapptype,
                    //    itemBaseName = x.itembasename,
                    //    ItemId = x.itemid,
                    //    ItemlimitQty = x.itemlimitqty,
                    //    ItemMultiMRPId = x.itemmultimrpid,
                    //    itemname = x.itemname,
                    //    ItemNumber = x.itemnumber,
                    //    LogoUrl = x.logourl,
                    //    MinOrderQty = x.minorderqty,
                    //    NetPurchasePrice = x.netpurchaseprice,
                    //    OfferCategory = x.offercategory,
                    //    OfferEndTime = x.offerendtime,
                    //    OfferFreeItemId = x.offerfreeitemid,
                    //    OfferFreeItemImage = x.offerfreeitemimage,
                    //    OfferFreeItemName = x.offerfreeitemname,
                    //    OfferFreeItemQuantity = x.offerfreeitemquantity,
                    //    OfferId = x.offerid,
                    //    OfferMinimumQty = x.offerminimumqty,
                    //    OfferPercentage = x.offerpercentage,
                    //    OfferQtyAvaiable = x.offerqtyavaiable,
                    //    OfferQtyConsumed = x.offerqtyconsumed,
                    //    OfferStartTime = x.offerstarttime,
                    //    OfferType = x.offertype,
                    //    OfferWalletPoint = x.offerwalletpoint,
                    //    price = x.price,
                    //    UnitofQuantity = x.unitofquantity,
                    //    UOM = x.uom,
                    //    SubCategoryId = x.subcategoryid,
                    //    SubsubCategoryid = x.subsubcategoryid,
                    //    WarehouseId = x.warehouseid,
                    //    UnitPrice = x.unitprice,
                    //    TotalTaxPercentage = x.totaltaxpercentage
                    //}).ToList();



                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();



                    var orderIdDt = new DataTable();
                    orderIdDt.Columns.Add("IntValue");
                    foreach (var item in itemids)
                    {
                        var dr = orderIdDt.NewRow();
                        dr["IntValue"] = item;
                        orderIdDt.Rows.Add(dr);
                    }
                    var param = new SqlParameter("ItemIds", orderIdDt);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[GetShoppingCardItem]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(param);

                    // Run the sproc
                    var reader = cmd.ExecuteReader();
                    var newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<factoryItemdata>(reader).ToList();

                    string apptype = customerShoppingCart.PeopleId > 0 ? "Sales App" : "Retailer App";
                    if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                        apptype = ConsumerApptype;

                    var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                    List<OrderOfferDc> activeOfferids = new List<OrderOfferDc>();
                    if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                        activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == apptype)).Select(x => new OrderOfferDc { OfferId = x.OfferId, QtyAvaiable = x.QtyAvaiable, IsDispatchedFreeStock = x.IsDispatchedFreeStock, MinOrderQuantity = x.MinOrderQuantity, NoOffreeQuantity = x.NoOffreeQuantity }).ToList() : null;
                    else
                        activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == apptype || x.OfferAppType == "Both")).Select(x => new OrderOfferDc { OfferId = x.OfferId, QtyAvaiable = x.QtyAvaiable, IsDispatchedFreeStock = x.IsDispatchedFreeStock, MinOrderQuantity = x.MinOrderQuantity, NoOffreeQuantity = x.NoOffreeQuantity }).ToList() : null;

                    List<int> Primeitemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsPrimeItem.HasValue && x.IsPrimeItem.Value && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();

                    var itemMultiMRPIds = newdata.Where(x => Primeitemids.Contains(x.ItemId)).Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    var PrimeItems = itemMultiMRPIds.Any() && ActiveCustomer.IsPrimeCustomer ? context.PrimeItemDetails.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.CityId == ActiveCustomer.Cityid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList() : null;
                    DateTime CurrentDate = !ActiveCustomer.IsPrimeCustomer ? DateTime.Now.AddHours(MemberShipHours) : DateTime.Now;
                    int hrs = !ActiveCustomer.IsPrimeCustomer ? MemberShipHours : 0;
                    itemMultiMRPIds = newdata.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    RetailerAppManager retailerAppManager = new RetailerAppManager();
                    List<ItemScheme> ItemSchemes = retailerAppManager.GetItemScheme(itemMultiMRPIds, ActiveCustomer.Warehouseid.Value, context);

                    var dealItems = context.DealItems.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == ActiveCustomer.Warehouseid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList();
                    BackendOrderController backendOrderController = new BackendOrderController();
                    foreach (var it in newdata)
                    {

                        double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                        it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice
                                                                    , it.WholeSalePrice ?? 0
                                                                    , it.TradePrice ?? 0, cprice);

                        if (PrimeItems != null && PrimeItems.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty))
                        {
                            var primeItem = PrimeItems.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty);
                            it.IsPrimeItem = true;
                            it.PrimePrice = primeItem.PrimePercent > 0 ? Convert.ToDecimal(it.UnitPrice - (it.UnitPrice * Convert.ToDouble(primeItem.PrimePercent) / 100)) : primeItem.PrimePrice;
                        }
                        //Condition for offer end.
                        if (!inActiveCustomer)
                        {
                            if (!(it.OfferStartTime.HasValue && it.OfferStartTime.Value.AddHours(hrs) <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
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
                            else if (it.OfferStartTime.HasValue && (it.OfferStartTime.Value.AddHours(hrs) <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
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
                                if (activeOfferids != null && activeOfferids.Any() && activeOfferids.Any(x => x.OfferId == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                            if (customerShoppingCart.PeopleId > 0 && it.OfferType == "FlashDeal")
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }
                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }

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
                            var unitprice = it.UnitPrice;

                            if (it.OfferCategory == 2 && it.IsOffer && it.FlashDealSpecialPrice.HasValue && it.FlashDealSpecialPrice > 0)
                            {
                                unitprice = it.FlashDealSpecialPrice.Value;
                            }
                            if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                                it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / it.price) : 0;
                            else
                                it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / unitprice) : 0;//MP;  we replce marginpoint value by margin for app here 


                            if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.PTR > 0))
                            {
                                var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId);
                                var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                                var UPMRPMargin = it.marginPoint.Value;
                                if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                                    it.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                            }
                        }
                        else
                        {
                            it.marginPoint = 0;
                        }

                        if (it.IsOffer && it.OfferCategory.HasValue && it.OfferCategory.Value == 1)
                        {
                            if (it.OfferType == "WalletPoint" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferWalletPoint.HasValue && it.OfferWalletPoint.Value > 0)
                            {
                                var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                if (item.qty >= it.OfferMinimumQty)
                                {
                                    var FreeWalletPoint = it.OfferWalletPoint.Value;
                                    int calfreeItemQty = item.qty / it.OfferMinimumQty.Value;
                                    FreeWalletPoint *= calfreeItemQty;
                                    item.TotalFreeWalletPoint = FreeWalletPoint;
                                    walletPoint += Convert.ToInt32(FreeWalletPoint);
                                }

                            }
                            else if (it.OfferType == "ItemMaster" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferFreeItemQuantity.HasValue && it.OfferFreeItemQuantity.Value > 0)
                            {
                                var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                if (item.qty >= it.OfferMinimumQty)
                                {
                                    var cartqty = it.IsItemLimit && item.qty > it.ItemlimitQty ? it.ItemlimitQty : item.qty;
                                    var FreeItemQuantity = it.OfferFreeItemQuantity.Value;
                                    int calfreeItemQty = Convert.ToInt32(cartqty / it.OfferMinimumQty);
                                    FreeItemQuantity *= calfreeItemQty;
                                    if (FreeItemQuantity > 0)
                                    {
                                        item.FreeItemqty = FreeItemQuantity;
                                    }
                                    else
                                    {
                                        item.FreeItemqty = 0;
                                    }
                                }
                                else
                                {
                                    item.FreeItemqty = 0;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi")
                        {
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
                        }
                    }


                    customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList().ForEach(x =>
                    {
                        if (newdata.Any(y => y.ItemId == x.ItemId))
                        {

                            var item = newdata.FirstOrDefault(y => y.ItemId == x.ItemId && y.OfferCategory == 2 && y.IsOffer && y.OfferStartTime.Value.AddHours(hrs) <= DateTime.Now);
                            if (item != null && (!ActiveCustomer.IsPrimeCustomer ? item.OfferStartTime.Value.AddHours(MemberShipHours) : item.OfferStartTime) > (x.ModifiedDate.HasValue ? x.ModifiedDate.Value : x.CreatedDate))
                            {
                                x.IsActive = false;
                                x.IsDeleted = true;
                                item.active = false;
                            }

                            var itemActive = newdata.FirstOrDefault(y => y.ItemId == x.ItemId);
                            if (itemActive != null)
                            {
                                x.IsActive = itemActive.active;
                                x.IsDeleted = !itemActive.active;
                            }
                            else
                            {
                                x.IsActive = false;
                                x.IsDeleted = true;
                            }
                            x.ItemMultiMRPId = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemMultiMRPId;
                            x.ItemNumber = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemNumber;
                            x.ItemName = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).itemname;
                            if (newdata.Any(y => y.ItemId == x.ItemId && y.OfferCategory == 2 && y.IsOffer && y.OfferStartTime.Value.AddHours(hrs) <= DateTime.Now))
                            {
                                x.UnitPrice = x.UnitPrice;
                            }
                            else
                            {
                                var dbitem = newdata.FirstOrDefault(y => y.ItemId == x.ItemId);
                                if (x.IsPrimeItem.HasValue && x.IsPrimeItem.Value && newdata.FirstOrDefault(y => y.ItemId == x.ItemId).IsPrimeItem)
                                {
                                    x.UnitPrice = Convert.ToDouble(newdata.FirstOrDefault(y => y.ItemId == x.ItemId).PrimePrice);
                                }
                                else if (x.IsDealItem.HasValue && x.IsDealItem.Value && dealItems.Any(y => y.ItemMultiMRPId == dbitem.ItemMultiMRPId && y.MinOrderQty == dbitem.MinOrderQty))
                                {
                                    x.UnitPrice = dealItems.FirstOrDefault(y => y.ItemMultiMRPId == dbitem.ItemMultiMRPId && y.MinOrderQty == dbitem.MinOrderQty).DealPrice;
                                }
                                else
                                {
                                    x.UnitPrice = x.UnitPrice;
                                }
                            }
                        }
                    });


                    CustomersManager manager = new CustomersManager();
                    List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(customerShoppingCart.CustomerId, apptype);
                    if ((billDiscountOfferDcs.Any() && customerShoppingCart.ShoppingCartDiscounts != null && customerShoppingCart.ShoppingCartDiscounts.Any()) || billDiscountOfferDcs.Any(x => x.IsAutoApply))
                    {
                        List<int> applyedOfferIds = customerShoppingCart.ShoppingCartDiscounts != null ? customerShoppingCart.ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList() : new List<int>();
                        int MaxSingleUsedOfferId = 0;

                        #region old code
                        //double MaxDiscountAmount = 0;
                        //bool IsUsedWithOfferApply = false;
                        //foreach (var Offer in billDiscountOfferDcs.Where(x => applyedOfferIds.Contains(x.OfferId)|| x.IsAutoApply))
                        //{
                        //    ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                        //    double totalamount = 0;
                        //    var OrderLineItems = 0;
                        //    //if (Offer.OfferOn != "ScratchBillDiscount")
                        //    //{
                        //    List<int> Itemids = new List<int>();
                        //    if (Offer.BillDiscountType == "category" && Offer.OfferBillDiscountItems.Any())
                        //    {
                        //        var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                        //        var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                        //        Itemids = newdata.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                        //        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                        //        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();

                        //    }
                        //    else if (Offer.BillDiscountType == "subcategory" && Offer.OfferBillDiscountItems.Any())
                        //    {
                        //        var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                        //        Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                        //        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                        //        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                        //    }
                        //    else if (Offer.BillDiscountType == "brand" && Offer.OfferBillDiscountItems.Any())
                        //    {
                        //        var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();

                        //        Itemids = newdata.Where(x => Offer.OfferBillDiscountItems.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                        //        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                        //        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                        //    }
                        //    else if (Offer.BillDiscountType == "items")
                        //    {
                        //        var iteminofferlist = Offer.OfferItems.Select(x => x.itemId).ToList();
                        //        if (Offer.OfferItems.FirstOrDefault().IsInclude)
                        //        {
                        //            Itemids = newdata.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                        //        }
                        //        var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                        //        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                        //        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                        //    }
                        //    else
                        //    {
                        //        var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                        //        Itemids = newdata.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();
                        //        var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                        //        totalamount = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                        //        OrderLineItems = Itemids.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                        //    }

                        //    if (Offer.BillDiscountRequiredItems != null && Offer.BillDiscountRequiredItems.Any())
                        //    {
                        //        bool IsRequiredItemExists = true;
                        //        var objectIds = Offer.BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                        //        if (Offer.BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                        //        {
                        //            objectIds.AddRange(newdata.Where(x => Offer.BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                        //        }
                        //        var cartrequiredItems = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                        //        foreach (var reqitem in Offer.BillDiscountRequiredItems)
                        //        {
                        //            if (reqitem.ObjectType == "Item")
                        //            {
                        //                var reqobjectids = reqitem.ObjectId.Split(',').Select(z => Convert.ToInt32(z)).ToList();
                        //                var cartitem = cartrequiredItems.Where(x => reqobjectids.Contains(x.ItemMultiMRPId));
                        //                if (cartitem != null && cartitem.Any())
                        //                {
                        //                    if (reqitem.ValueType == "Qty" && reqitem.ObjectValue > cartitem.Sum(x => x.qty))
                        //                    {
                        //                        IsRequiredItemExists = false;
                        //                        break;
                        //                    }
                        //                    else if (reqitem.ValueType == "Value" && reqitem.ObjectValue > cartitem.Sum(x => x.qty * x.UnitPrice))
                        //                    {
                        //                        IsRequiredItemExists = false;
                        //                        break;
                        //                    }
                        //                }
                        //                else
                        //                {
                        //                    IsRequiredItemExists = false;
                        //                    break;
                        //                }
                        //            }
                        //            else if (reqitem.ObjectType == "brand")
                        //            {
                        //                var reqobjectids = reqitem.ObjectId.Split(',').Select(z => z).ToList();
                        //                var multiMrpIds = newdata.Where(x => reqobjectids.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
                        //                var cartitems = cartrequiredItems.Where(x => multiMrpIds.Contains(x.ItemMultiMRPId));
                        //                if (cartitems != null && cartitems.Any())
                        //                {
                        //                    if (reqitem.ValueType == "Qty" && reqitem.ObjectValue > cartitems.Sum(x => x.qty))
                        //                    {
                        //                        IsRequiredItemExists = false;
                        //                        break;
                        //                    }
                        //                    else if (reqitem.ValueType == "Value" && reqitem.ObjectValue > cartitems.Sum(x => x.qty * x.UnitPrice))
                        //                    {
                        //                        IsRequiredItemExists = false;
                        //                        break;
                        //                    }
                        //                }
                        //                else
                        //                {
                        //                    IsRequiredItemExists = false;
                        //                    break;
                        //                }

                        //            }
                        //        }
                        //        if (!IsRequiredItemExists)
                        //        {
                        //            totalamount = 0;
                        //        }
                        //    }


                        //    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                        //    {
                        //        totalamount = Offer.MaxBillAmount;
                        //    }
                        //    else if (Offer.BillAmount > totalamount)
                        //    {
                        //        totalamount = 0;
                        //    }

                        //    if (Offer.LineItem > 0 && Offer.LineItem > OrderLineItems)
                        //    {
                        //        totalamount = 0;
                        //    }

                        //    if (Offer.OfferOn == "ScratchBillDiscount" && !Offer.IsScratchBDCode)
                        //    {
                        //        totalamount = 0;
                        //    }

                        //    //}
                        //    //else
                        //    //{
                        //    //    var Itemids = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                        //    //    totalamount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && Itemids.Contains(x.ItemId) && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice);

                        //    //    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                        //    //    {
                        //    //        totalamount = Offer.MaxBillAmount;
                        //    //    }
                        //    //    else if (Offer.BillAmount > totalamount)
                        //    //    {
                        //    //        totalamount = 0;
                        //    //    }

                        //    //    if (!Offer.IsScratchBDCode)
                        //    //        totalamount = 0;
                        //    //}

                        //    if (totalamount > 0)
                        //    {
                        //        if (Offer.BillDiscountOfferOn == "Percentage")
                        //        {
                        //            ShoppingCartDiscount.DiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                        //        }
                        //        else if (Offer.BillDiscountOfferOn == "FreeItem")
                        //        {
                        //            ShoppingCartDiscount.DiscountAmount = 0;
                        //        }
                        //        else
                        //        {
                        //            int WalletPoint = 0;
                        //            if (Offer.WalletType == "WalletPercentage")
                        //            {
                        //                WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * (Offer.BillDiscountWallet ?? 0) / 100));
                        //                WalletPoint = WalletPoint * 10;
                        //            }
                        //            else
                        //            {
                        //                WalletPoint = Convert.ToInt32(Offer.BillDiscountWallet ?? 0);
                        //            }
                        //            if (Offer.ApplyOn == "PostOffer")
                        //            {
                        //                ShoppingCartDiscount.DiscountAmount = 0;
                        //                ShoppingCartDiscount.NewBillingWalletPoint = WalletPoint;
                        //            }
                        //            else
                        //            {
                        //                ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10); ;
                        //                ShoppingCartDiscount.NewBillingWalletPoint = 0;
                        //            }
                        //        }
                        //        if (Offer.MaxDiscount > 0)
                        //        {
                        //            var walletmultipler = 1;

                        //            if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage")
                        //            {
                        //                walletmultipler = 10;
                        //            }
                        //            if (Offer.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                        //            {
                        //                ShoppingCartDiscount.DiscountAmount = Offer.MaxDiscount * walletmultipler;
                        //            }
                        //            if (Offer.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint / 10)
                        //            {
                        //                ShoppingCartDiscount.NewBillingWalletPoint = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                        //            }
                        //        }

                        //        if (ShoppingCartDiscount.NewBillingWalletPoint > 0)
                        //        {
                        //            ShoppingCartDiscount.DiscountAmount = ShoppingCartDiscount.NewBillingWalletPoint / 10;
                        //        }

                        //        if (ShoppingCartDiscount.DiscountAmount > 0 && Offer.IsUseOtherOffer)
                        //        {
                        //            IsUsedWithOfferApply = true;
                        //        }

                        //        if (MaxDiscountAmount < ShoppingCartDiscount.DiscountAmount)
                        //        {
                        //            MaxDiscountAmount = ShoppingCartDiscount.DiscountAmount;
                        //            MaxSingleUsedOfferId = Offer.OfferId;
                        //        }
                        //        if (IsUsedWithOfferApply)
                        //            MaxSingleUsedOfferId = 0;
                        //    }
                        //}

                        #endregion

                        if (applyedOfferIds != null && applyedOfferIds.Any())
                        {
                            billDiscountOfferDcs.ForEach(x => x.IsAppliedOffer = applyedOfferIds.Contains(x.OfferId));
                        }

                        foreach (var Offer1 in billDiscountOfferDcs.Where(x => applyedOfferIds.Contains(x.OfferId) || x.IsAutoApply).OrderByDescending(x => x.IsAppliedOffer).ThenByDescending(x => x.IsUseOtherOffer).ToList())
                        {
                            ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                            double totalamount = 0;
                            var OrderLineItems = 0;
                            //if (Offer1.OfferOn != "ScratchBillDiscount")
                            //{
                            List<int> Itemids = new List<int>();
                            if (Offer1.BillDiscountType == "category" && Offer1.OfferBillDiscountItems.Any())
                            {
                                var itemoutofferlist = Offer1.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer1.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                var ids = Offer1.OfferBillDiscountItems.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                var notids = Offer1.OfferBillDiscountItems.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer1.IncentiveClassification))
                                {
                                    var classifications = Offer1.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x => (!ids.Any() || ids.Contains(x.Categoryid))
                               && !notids.Contains(x.Categoryid)
                               && !itemoutofferlist.Contains(x.ItemId)
                               && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                               && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer1.OfferLineItemValueDcs != null && Offer1.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer1.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                            else if (Offer1.BillDiscountType == "subcategory" && Offer1.OfferBillDiscountItems.Any())
                            {
                                var itemoutofferlist = Offer1.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer1.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer1.IncentiveClassification))
                                {
                                    var classifications = Offer1.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x =>
                                 (!Offer1.OfferBillDiscountItems.Where(y => y.IsInclude).Any() || Offer1.OfferBillDiscountItems.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                                 && !Offer1.OfferBillDiscountItems.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer1.OfferLineItemValueDcs != null && Offer1.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer1.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                            else if (Offer1.BillDiscountType == "brand" && Offer1.OfferBillDiscountItems.Any())
                            {
                                var itemoutofferlist = Offer1.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer1.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer1.IncentiveClassification))
                                {
                                    var classifications = Offer1.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x =>
                                (
                                 !Offer1.OfferBillDiscountItems.Where(y => y.IsInclude).Any() ||
                                Offer1.OfferBillDiscountItems.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                )
                                && !Offer1.OfferBillDiscountItems.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer1.OfferLineItemValueDcs != null && Offer1.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer1.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                            else if (Offer1.BillDiscountType == "items" && Offer1.IsBillDiscountFreebiesItem)
                            {
                                var itemoutofferlist = Offer1.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer1.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                var itemnumbermrps = context.itemMasters.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => new DataContracts.BillDiscount.offerItemMRP { ItemNumber = x.Number, ItemMultiMRPId = x.ItemMultiMRPId }).ToList();

                                var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer1.IncentiveClassification))
                                {
                                    var classifications = Offer1.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x => (!iteminofferlist.Any() || itemnumbermrps.Select(y => y.ItemNumber + "" + y.ItemMultiMRPId).Contains(x.ItemNumber + "" + x.ItemMultiMRPId))
                                && !itemoutofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer1.OfferLineItemValueDcs != null && Offer1.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer1.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                            else if (Offer1.BillDiscountType == "items")
                            {
                                var itemoutofferlist = Offer1.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer1.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                                //if (Offer.OfferItems.FirstOrDefault().IsInclude)
                                //{
                                //    Itemids = newdata.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                //}
                                var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer1.IncentiveClassification))
                                {
                                    var classifications = Offer1.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x => (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !itemoutofferlist.Contains(x.ItemId)
                                ).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer1.OfferLineItemValueDcs != null && Offer1.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer1.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                                var catIdoutofferlist = Offer1.OfferBillDiscountItems.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                var catIdinofferlist = Offer1.OfferBillDiscountItems.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                // var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer1.IncentiveClassification))
                                {
                                    var classifications = Offer1.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x => (catIdinofferlist.Any() || catIdinofferlist.Contains(x.Categoryid))
                                && !catIdoutofferlist.Contains(x.Categoryid)
                                ).Select(x => x.ItemId).ToList();
                                var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                if (catIdoutofferlist.Any())
                                    incluseItemIds = newdata.Where(x => !catIdoutofferlist.Contains(x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    incluseItemIds = newdata.Where(x => CItemIds.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)
                                  ).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).ToList();
                                if (cartItems != null && cartItems.Any() && Offer1.OfferLineItemValueDcs != null && Offer1.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer1.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) >= item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
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

                            if (Offer1.BillDiscountRequiredItems != null && Offer1.BillDiscountRequiredItems.Any())
                            {
                                bool IsRequiredItemExists = true;
                                var objectIds = Offer1.BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                                if (Offer1.BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                                {
                                    objectIds.AddRange(newdata.Where(x => Offer1.BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                                }
                                var cartrequiredItems = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                foreach (var reqitem in Offer1.BillDiscountRequiredItems)
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
                                        var multiMrpIds = newdata.Where(x => objIds.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
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

                            if (Offer1.MaxBillAmount > 0 && totalamount > Offer1.MaxBillAmount)
                            {
                                totalamount = Offer1.MaxBillAmount;
                            }
                            else if (Offer1.BillAmount > totalamount)
                            {
                                totalamount = 0;
                            }

                            if (Offer1.LineItem > 0 && Offer1.LineItem > OrderLineItems)
                            {
                                totalamount = 0;
                            }

                            if (Offer1.OfferOn == "ScratchBillDiscount" && !Offer1.IsScratchBDCode)
                                totalamount = 0;


                            bool IsUsed = true;
                            if (!Offer1.IsUseOtherOffer && (ShoppingCartDiscounts.Any() || (MaxSingleUsedOfferId != 0 && MaxSingleUsedOfferId != Offer1.OfferId)))
                                IsUsed = false;


                            if (ShoppingCartDiscounts.Any() && Offer1.IsUseOtherOffer)
                            {
                                IsUsed = billDiscountOfferDcs.Any(y => ShoppingCartDiscounts.Select(x => x.OfferId).ToList().Contains(y.OfferId) && y.IsUseOtherOffer);
                            }
                            //new enhancement
                            if (IsUsed)
                            {
                                //List<int> customershoppingcartofferids = applyedOfferIds;
                                //if (billDiscountOfferDcs.Any(x => x.IsAutoApply))
                                //{
                                //    List<int> offeriddata = billDiscountOfferDcs.Where(x => x.IsAutoApply).Select(y => y.OfferId).ToList();
                                //    customershoppingcartofferids.AddRange(offeriddata);
                                //}
                                //var billdiscounts = billDiscountOfferDcs.Where(x => customershoppingcartofferids.Distinct().ToList().Contains(x.OfferId)).ToList();

                                List<long> combinedgroupids = billDiscountOfferDcs.Where(x => (applyedOfferIds.Contains(x.OfferId) || x.IsAutoApply) && x.CombinedGroupId > 0).Select(y => y.CombinedGroupId).Distinct().ToList();
                                if (combinedgroupids != null && combinedgroupids.Any())
                                {
                                    if (Offer1.CombinedGroupId > 0 && Offer1.IsAutoApply && !applyedOfferIds.Contains(Offer1.OfferId))
                                    {
                                        if (combinedgroupids.Any(x => x != Offer1.CombinedGroupId))
                                        {
                                            IsUsed = false;
                                        }
                                    }
                                }
                            }

                            if (IsUsed && totalamount > 0)
                            {
                                if (Offer1.BillDiscountOfferOn == "Percentage")
                                {
                                    ShoppingCartDiscount.DiscountAmount = totalamount * Offer1.DiscountPercentage / 100;
                                }
                                else if (Offer1.BillDiscountOfferOn == "FreeItem")
                                {
                                    ShoppingCartDiscount.DiscountAmount = 0;
                                }
                                else if (Offer1.BillDiscountOfferOn == "DynamicAmount")
                                {
                                    ShoppingCartDiscount.DiscountAmount = Offer1.BillDiscountWallet.Value;
                                }
                                else
                                {
                                    int WalletPoint = 0;
                                    if (Offer1.WalletType == "WalletPercentage")
                                    {
                                        WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * (Offer1.BillDiscountWallet ?? 0) / 100));
                                        WalletPoint = WalletPoint * 10;
                                    }
                                    else
                                    {
                                        WalletPoint = Convert.ToInt32(Offer1.BillDiscountWallet ?? 0);
                                    }
                                    if (Offer1.ApplyOn == "PostOffer")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = 0;
                                        ShoppingCartDiscount.NewBillingWalletPoint = WalletPoint;
                                    }
                                    else
                                    {
                                        if (ActiveCustomer.CustomerType.ToLower() == "consumer")
                                        {
                                            var Conversion = context.CashConversionDb.FirstOrDefault(x => x.IsConsumer == true);
                                            if (Conversion != null)
                                            {
                                                ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / Conversion.point); ;
                                            }
                                        }
                                        else
                                        {
                                            var Conversion = context.CashConversionDb.FirstOrDefault(x => x.IsConsumer == false);
                                            if (Conversion != null)
                                            {
                                                ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / Conversion.point); ;
                                            }
                                        }
                                        ShoppingCartDiscount.NewBillingWalletPoint = 0;
                                    }
                                }
                                if (Offer1.MaxDiscount > 0)
                                {

                                    var walletmultipler = 1;

                                    if (!string.IsNullOrEmpty(Offer1.BillDiscountOfferOn) && (Offer1.BillDiscountOfferOn != "Percentage" && Offer1.BillDiscountOfferOn != "DynamicAmount"))
                                    {
                                        walletmultipler = 10;
                                    }
                                    if (Offer1.BillDiscountOfferOn != "DynamicAmount")
                                    {
                                        if (Offer1.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                        {
                                            ShoppingCartDiscount.DiscountAmount = Offer1.MaxDiscount;
                                        }
                                        if (Offer1.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint / 10)
                                        {
                                            ShoppingCartDiscount.NewBillingWalletPoint = Convert.ToInt32(Offer1.MaxDiscount * walletmultipler);
                                        }
                                    }
                                }

                                if (Offer1.OfferOn == "BillDiscount" && Offer1.BillDiscountOfferOn == "FreeItem")
                                {
                                    BillDiscountItemOfferIds.Add(Offer1.OfferId);
                                }

                                ShoppingCartDiscount.OfferId = Offer1.OfferId;
                                ShoppingCartDiscount.CreatedDate = DateTime.Now;
                                ShoppingCartDiscount.IsActive = Offer1.IsActive;
                                ShoppingCartDiscount.IsDeleted = false;
                                ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                            }
                        }
                    }
                    customerShoppingCart.ShoppingCartDiscounts = ShoppingCartDiscounts;


                    List<StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();

                    ShoppingCartItemDcs = newdata.Where(x => x.active).Select(a => new ShoppingCartItemDc
                    {
                        BaseCategoryId = a.BaseCategoryId,
                        IsItemLimit = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? true : a.IsItemLimit,
                        ItemlimitQty = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) && dealItems.Any(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty) ? (dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).OrderLimit < dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalLimit - dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalConsume ? dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).OrderLimit : dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalLimit - dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalConsume) : a.ItemlimitQty,
                        BillLimitQty = a.BillLimitQty,
                        WarehouseId = a.WarehouseId,
                        CompanyId = a.CompanyId,
                        Categoryid = a.Categoryid,
                        Discount = a.Discount,
                        ItemId = a.ItemId,
                        ItemNumber = a.ItemNumber,
                        HindiName = a.HindiName,
                        IsSensitive = a.IsSensitive,
                        IsSensitiveMRP = a.IsSensitiveMRP,
                        UnitofQuantity = a.UnitofQuantity,
                        UOM = a.UOM,
                        itemname = a.itemname,
                        LogoUrl = a.LogoUrl,
                        MinOrderQty = !string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer" ? 1 : a.MinOrderQty,
                        price = a.price,
                        SubCategoryId = a.SubCategoryId,
                        SubsubCategoryid = a.SubsubCategoryid,
                        TotalTaxPercentage = a.TotalTaxPercentage,
                        SellingUnitName = a.SellingUnitName,
                        SellingSku = a.SellingSku,
                        UnitPrice = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value).UnitPrice : a.UnitPrice,
                        ItemMultiMRPId = a.ItemMultiMRPId,
                        VATTax = a.VATTax,
                        itemBaseName = a.itemBaseName,
                        active = a.active,
                        marginPoint = a.marginPoint.HasValue ? a.marginPoint.Value : 0,
                        promoPerItems = a.promoPerItems.HasValue ? a.promoPerItems.Value : 0,
                        NetPurchasePrice = a.NetPurchasePrice,
                        IsOffer = a.IsOffer,
                        Deleted = a.Deleted,
                        OfferCategory = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? 3 : (a.OfferCategory.HasValue ? a.OfferCategory.Value : 0),
                        OfferStartTime = a.OfferStartTime,
                        OfferEndTime = a.OfferEndTime,
                        OfferQtyAvaiable = a.OfferQtyAvaiable.HasValue ? a.OfferQtyAvaiable.Value : 0,
                        OfferQtyConsumed = a.OfferQtyConsumed.HasValue ? a.OfferQtyConsumed.Value : 0,
                        OfferId = a.OfferId.HasValue ? a.OfferId.Value : 0,
                        OfferType = a.OfferType,
                        dreamPoint = a.dreamPoint.HasValue ? a.dreamPoint.Value : 0,
                        OfferWalletPoint = a.OfferWalletPoint.HasValue ? a.OfferWalletPoint.Value : 0,
                        OfferFreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                        OfferPercentage = a.OfferPercentage.HasValue ? a.OfferPercentage.Value : 0,
                        OfferFreeItemName = a.OfferFreeItemName,
                        OfferFreeItemImage = a.OfferFreeItemImage,
                        OfferFreeItemQuantity = a.OfferFreeItemQuantity.HasValue ? a.OfferFreeItemQuantity.Value : 0,
                        OfferMinimumQty = a.OfferMinimumQty.HasValue ? a.OfferMinimumQty.Value : 0,
                        FlashDealSpecialPrice = a.FlashDealSpecialPrice.HasValue ? a.FlashDealSpecialPrice.Value : 0,
                        FlashDealMaxQtyPersonCanTake = a.FlashDealMaxQtyPersonCanTake.HasValue ? a.FlashDealMaxQtyPersonCanTake.Value : 0,
                        FreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                        IsPrimeItem = a.IsPrimeItem,
                        ItemAppType = a.ItemAppType,
                        PrimePrice = Convert.ToDouble(a.PrimePrice),
                        IsDealItem = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).IsDealItem,
                        qty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).qty,
                        CartUnitPrice = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).UnitPrice,
                        TotalFreeItemQty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).FreeItemqty,
                        TotalFreeWalletPoint = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).TotalFreeWalletPoint,
                        Scheme = a.Scheme,

                    }).ToList();
                    #region Supplier PO Check item Code                    
                    var itemmultimrp = new DataTable();
                    itemmultimrp.Columns.Add("IntValue");
                    foreach (var item in itemMultiMRPIds)
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
                        Value = customerShoppingCart.CustomerId
                    };

                    var supplierItemlist = context.Database.SqlQuery<int>("exec GetSupplierItemForRetailer @customerId , @itemmultiMrpIds", paramStatus, itemmultimrpids).ToList();
                    #endregion

                    List<DataContracts.Consumer.ItemNetInventoryDc> ItemNetInventoryDcs = null;
                    #region ItemNetInventoryCheck
                    if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                    {
                        if (ShoppingCartItemDcs != null && ShoppingCartItemDcs.Any())
                        {
                            var itemInventory = new DataTable();
                            itemInventory.Columns.Add("ItemMultiMRPId");
                            itemInventory.Columns.Add("WarehouseId");
                            itemInventory.Columns.Add("Qty");
                            itemInventory.Columns.Add("isdispatchedfreestock");

                            foreach (var item in ShoppingCartItemDcs)
                            {
                                var dr = itemInventory.NewRow();
                                dr["ItemMultiMRPId"] = item.ItemMultiMRPId;
                                dr["WarehouseId"] = item.WarehouseId;
                                dr["Qty"] = 0;
                                dr["isdispatchedfreestock"] = false;
                                itemInventory.Rows.Add(dr);
                            }
                            var parmitemInventory = new SqlParameter
                            {
                                ParameterName = "ItemNetInventory",
                                SqlDbType = SqlDbType.Structured,
                                TypeName = "dbo.ItemNetInventory",
                                Value = itemInventory
                            };
                            ItemNetInventoryDcs = context.Database.SqlQuery<DataContracts.Consumer.ItemNetInventoryDc>("exec CheckItemNetInventory  @ItemNetInventory", parmitemInventory).ToList();

                        }
                    }
                    #endregion

                    foreach (var item in ShoppingCartItemDcs)
                    {

                        if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid))
                        {
                            var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid);
                            item.StoreId = store.StoreId;
                            item.StoreName = store.StoreName;
                            item.StoreLogo = string.IsNullOrEmpty(store.StoreLogo) ? "" : store.StoreLogo;
                        }
                        else
                        {
                            item.StoreId = 0;
                            item.StoreName = "Other";
                            item.StoreLogo = "";
                        }
                        item.IsSuccess = true;
                        bool valid = true;
                        if (!item.active || item.Deleted)
                        {
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "ItemNotActive"; //!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम एक्टिव नहीं है " : "Item is not Active";
                        }
                        if (supplierItemlist != null && supplierItemlist.Any(x => x == item.ItemMultiMRPId))
                        {
                            item.IsSuccess = false;
                            item.Message = "SupplierNotEligible";//to purchase this item!!
                        }
                        if (valid && !(item.ItemAppType == 1 || item.ItemAppType == 0))
                        {
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "ItemNotActive";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम एक्टिव नहीं है " : "Item is not Active";
                        }
                        if (ConsumerApptype.ToLower() != "consumer")//by sudhir 15-04-2024
                        {
                            var mod = Convert.ToDecimal(item.qty) % item.MinOrderQty;
                            if (mod != 0 || (item.ItemlimitQty > 0 && item.qty > 0 && item.MinOrderQty > item.ItemlimitQty))
                            {
                                valid = false;
                                item.IsSuccess = false;
                                item.qty = 0;
                                item.Message = "multiplesMinOrderQty";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की क्वांटिटी मल्टीप्ल नहीं है मिनिमम आर्डर क्वांटिटी के " : "Item qty is not multiples of min order qty.";
                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                {
                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                                }
                            }
                        }
                        else
                        {
                            if (ItemNetInventoryDcs != null && ItemNetInventoryDcs.Any(x => item.ItemMultiMRPId == x.itemmultimrpid))
                            {
                                var itemInventory = ItemNetInventoryDcs.FirstOrDefault(x => item.ItemMultiMRPId == x.itemmultimrpid);
                                if (!itemInventory.isavailable)
                                {
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.qty = 0;
                                    item.Message = "OutOfStock";
                                }
                                item.IsItemLimit = true;
                                item.ItemlimitQty = itemInventory.RemainingQty;
                            }
                        }
                        if (valid)
                        {
                            if (item.IsOffer && item.OfferType == "FlashDeal")
                            {
                                if (item.FlashDealSpecialPrice.HasValue && item.CartUnitPrice != item.FlashDealSpecialPrice.Value)
                                {
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.Message = "UnitPriceChange";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की यूनिट प्राइस चेंज हो गई है" : "Item Unit Price has changed";
                                    item.NewUnitPrice = item.FlashDealSpecialPrice.Value;
                                    if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                    {
                                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.FlashDealSpecialPrice.Value;
                                    }
                                }
                            }
                            else
                            {
                                if (ActiveCustomer.IsPrimeCustomer && item.IsPrimeItem)
                                {
                                    if (item.PrimePrice != item.CartUnitPrice)
                                    {
                                        valid = false;
                                        item.IsSuccess = false;
                                        item.Message = "MemberUnitPriceChange"; //"Item " + MemberShipName + " Price has changed";
                                        item.NewUnitPrice = item.PrimePrice;
                                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                        {
                                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.PrimePrice;
                                        }
                                    }
                                }
                                else if (item.IsDealItem.HasValue && item.IsDealItem.Value)
                                {
                                    if (dealItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty))
                                    {
                                        var dealitem = dealItems.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty);
                                        if (dealitem.DealPrice != item.CartUnitPrice)
                                        {
                                            valid = false;
                                            item.IsSuccess = false;
                                            item.Message = "UnitPriceChange";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की यूनिट प्राइस चेंज हो गई है" : "Item Unit Price has changed";
                                            item.NewUnitPrice = dealitem.DealPrice;
                                            if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                            {
                                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = dealitem.DealPrice;
                                            }
                                        }
                                        item.ItemlimitQty = dealitem.OrderLimit;
                                    }
                                    else
                                    {
                                        valid = false;
                                        item.IsSuccess = false;
                                        item.Message = "ItemNotActive";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम एक्टिव नहीं है" : "Item is not Active";
                                    }
                                }
                                else if (item.UnitPrice != item.CartUnitPrice)
                                {
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.Message = "UnitPriceChange";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की यूनिट प्राइस चेंज हो गई है" : "Item Unit Price has changed";
                                    item.NewUnitPrice = item.UnitPrice;
                                    if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                    {
                                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.UnitPrice;
                                    }
                                }
                            }
                        }
                        if (!(item.IsOffer && item.OfferType == "FlashDeal"))
                        {
                            if (valid && item.IsItemLimit && item.ItemlimitQty < item.qty)
                            {
                                item.qty = item.qty > item.ItemlimitQty ? item.ItemlimitQty : (item.ItemlimitQty - item.qty);
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "ItemLimitReach";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की लिमिट ख़तम हो गई है" : "Item Limit Exceeded";
                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                {
                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                                }
                            }
                            if (valid && item.BillLimitQty > 0 && item.BillLimitQty < item.qty)
                            {
                                item.qty = item.qty > item.BillLimitQty ? item.BillLimitQty : (item.BillLimitQty - item.qty);
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "BillLimitReach";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की बिलिंग लिमिट ख़तम हो गई है" : "Item Bill Limit Exceeded";
                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                {
                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                                }
                            }
                        }
                        else
                        {
                            item.BillLimitQty = 0;
                        }

                        if (valid && activeOfferids != null && item.IsOffer && item.OfferFreeItemId > 0 && item.TotalFreeItemQty > 0)
                        {
                            var offer = activeOfferids.FirstOrDefault(x => x.OfferId == item.OfferId);
                            if (offer != null && (offer.QtyAvaiable < item.TotalFreeItemQty))
                            {
                                item.OfferCategory = 0;
                                item.IsOffer = false;
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "FreeItemExpired";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "फ्री आइटम ऑफर ख़तम हो गया है" : "Free Item expired";
                            }

                            #region old free bies
                            //else if (!offer.IsDispatchedFreeStock && item.ItemId == item.FreeItemId && item.IsItemLimit && (item.ItemlimitQty - item.qty) < item.TotalFreeItemQty)
                            //{
                            //    int multiply = Convert.ToInt32(item.qty / offer.MinOrderQuantity);
                            //    int maxFreeItemqty = (multiply * offer.NoOffreeQuantity) > (item.ItemlimitQty - item.qty) ? (item.ItemlimitQty - item.qty) : multiply * offer.NoOffreeQuantity;
                            //    int maxMainItemqty = maxFreeItemqty * offer.MinOrderQuantity;

                            //    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = maxMainItemqty;
                            //    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).FreeItemqty = maxFreeItemqty;
                            //    item.qty = maxMainItemqty;
                            //    item.TotalFreeItemQty = maxFreeItemqty;
                            //    valid = false;
                            //    item.IsSuccess = false;
                            //    item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आप को फ्री आइटम " + maxFreeItemqty + " क्वांटिटी से ज्यादा नहीं मिलेंगे।" : "You will not get more than " + maxFreeItemqty + " quantity of free items.";
                            //}
                            //else if (!offer.IsDispatchedFreeStock && item.ItemId != item.OfferFreeItemId)
                            //{
                            //    var FreeItem = context.itemMasters.Where(x => x.ItemId == item.OfferFreeItemId).Select(x => new { x.WarehouseId, x.ItemMultiMRPId }).FirstOrDefault();
                            //    var freelimit = context.ItemLimitMasterDB.FirstOrDefault(x => x.WarehouseId == FreeItem.WarehouseId && x.ItemMultiMRPId == FreeItem.ItemMultiMRPId);

                            //    if (freelimit != null && freelimit.IsItemLimit && (freelimit.ItemlimitQty) < item.TotalFreeItemQty)
                            //    {
                            //        int multiply = Convert.ToInt32(item.qty / offer.MinOrderQuantity);
                            //        int maxFreeItemqty = (multiply * offer.NoOffreeQuantity) > freelimit.ItemlimitQty ? freelimit.ItemlimitQty : multiply * offer.NoOffreeQuantity;
                            //        int maxMainItemqty = maxFreeItemqty * offer.MinOrderQuantity;
                            //        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).FreeItemqty = maxFreeItemqty;
                            //        //customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = maxMainItemqty;
                            //        // item.qty = maxMainItemqty;
                            //        item.TotalFreeItemQty = maxFreeItemqty;
                            //        valid = false;
                            //        item.IsSuccess = false;
                            //        item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आप को फ्री आइटम " + maxFreeItemqty + " क्वांटिटी से ज्यादा नहीं मिलेंगे।" : "You will not get more than " + maxFreeItemqty + " quantity of free items.";
                            //    }
                            //}
                            #endregion
                        }

                        if (valid && customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).FreeItemqty > 0)
                        {
                            if (activeOfferids == null || !activeOfferids.Any(x => x.OfferId == item.OfferId))
                            {
                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).IsFreeItem = false;
                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).FreeItemqty = 0;
                                item.OfferCategory = 0;
                                item.IsOffer = false;
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "FreeItemExpired";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "फ्री आइटम ऑफर ख़तम हो गया है" : "Free Item expired";
                            }
                        }


                    }

                    if (ShoppingCartItemDcs != null && activeOfferids != null && ShoppingCartItemDcs.Any(x => x.FreeItemId > 0 && x.TotalFreeItemQty > 0))
                    {
                        foreach (var item in ShoppingCartItemDcs.GroupBy(x => new { x.FreeItemId, x.ItemNumber, x.OfferId, x.ItemMultiMRPId }))
                        {
                            if (item.Sum(x => x.TotalFreeItemQty) > 0)
                            {
                                var freeItemoffer = activeOfferids.FirstOrDefault(x => x.OfferId == item.Key.OfferId);
                                if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.Sum(x => x.TotalFreeItemQty))
                                {
                                    var qtyAvailable = Convert.ToInt32(freeItemoffer.QtyAvaiable);
                                    foreach (var shoppingCart in ShoppingCartItemDcs.Where(x => x.ItemNumber == item.Key.ItemNumber))
                                    {
                                        if (shoppingCart.TotalFreeItemQty > qtyAvailable)
                                        {
                                            shoppingCart.OfferCategory = 0;
                                            shoppingCart.IsOffer = false;
                                            shoppingCart.IsSuccess = false;
                                            shoppingCart.Message = "FreeItemExpired";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "फ्री आइटम ऑफर ख़तम हो गया है" : "Free Item expired";
                                        }
                                        else
                                        {
                                            qtyAvailable = qtyAvailable - shoppingCart.TotalFreeItemQty;
                                        }

                                    }
                                }
                            }
                        }
                    }



                    customerShoppingCart.TotalDiscountAmt = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.DiscountAmount) : 0;
                    customerShoppingCart.NewBillingWalletPoint = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.NewBillingWalletPoint) : 0;
                    customerShoppingCart.DeamPoint = newdata.Where(x => x.dreamPoint.HasValue && x.active).Sum(x => x.dreamPoint.Value * customerShoppingCart.ShoppingCartItems.FirstOrDefault(y => y.ItemId == x.ItemId).qty);
                    customerShoppingCart.CartTotalAmt = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice) - customerShoppingCart.TotalDiscountAmt;

                    customerShoppingCart.TotalTaxAmount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.TaxAmount);
                    customerShoppingCart.GrossTotalAmt = Math.Round(customerShoppingCart.CartTotalAmt, 0, MidpointRounding.AwayFromZero);

                    customerShoppingCart.WalletAmount = walletPoint;

                    customerShoppingCart.TotalSavingAmt = ShoppingCartItemDcs.Sum(x => (x.qty * x.price) - (x.qty * x.CartUnitPrice));
                    customerShoppingCart.TotalQty = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                    int lineItemCount = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && !x.IsFreeItem && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                    #region old code
                    //var wheelConfig = context.CompanyWheelConfiguration.FirstOrDefault();
                    #endregion

                    #region new code for consumer type
                    CompanyWheelConfiguration wheelConfig = new CompanyWheelConfiguration();
                    if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer")
                    {
                        wheelConfig = context.CompanyWheelConfiguration.FirstOrDefault(x => x.IsStore);
                    }
                    else
                    {
                        wheelConfig = context.CompanyWheelConfiguration.FirstOrDefault();
                    }
                    #endregion

                    decimal wheelAmount = 0;
                    customerShoppingCart.WheelCount = 0;
                    int totalamt = Convert.ToInt32(Math.Truncate(customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt));
                    if ((wheelConfig.IsKPPRequiredWheel && ActiveCustomer.IsKPP) || !ActiveCustomer.IsKPP)
                    {
                        if (wheelConfig != null && wheelConfig.OrderAmount > 0 && wheelConfig.LineItemCount > 0)
                        {
                            if (lineItemCount >= wheelConfig.LineItemCount)
                            {
                                wheelAmount = wheelConfig.OrderAmount;
                                customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(totalamt / wheelAmount));
                            }
                        }
                        else if (wheelConfig != null && wheelConfig.OrderAmount > 0)
                        {
                            wheelAmount = wheelConfig.OrderAmount;
                            customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(totalamt / wheelAmount));
                        }
                        else if (wheelConfig != null && wheelConfig.LineItemCount > 0 && lineItemCount >= wheelConfig.LineItemCount)
                        {
                            wheelAmount = 0;
                            customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(Convert.ToDecimal(lineItemCount) / wheelConfig.LineItemCount));
                        }
                    }
                    //customerShoppingCart.WheelCount = Convert.ToInt32(Math.Truncate((customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt) / 4000));
                    customerShoppingCart.SkCode = ActiveCustomer != null ? ActiveCustomer.Skcode : "";
                    customerShoppingCart.Mobile = ActiveCustomer != null ? ActiveCustomer.Mobile : "";
                    customerShoppingCart.ShopName = ActiveCustomer != null ? ActiveCustomer.ShopName : "";
                    customerShoppingCart.City = ActiveCustomer != null ? ActiveCustomer.City : "";
                    double TotalAmount = customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt;
                    double DeliveryAmount = 0;
                    var storeIds = ShoppingCartItemDcs.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();
                    var itemNumbers = ShoppingCartItemDcs.Where(x => x.qty > 0).Select(x => x.ItemNumber).Distinct().ToList();


                    customerShoppingCartDc.IsPayLater = false;
                    #region storePayLaterLimit
                    if (storeIds.Count() == 1)
                    {

                        System.Data.DataTable IdDt = new System.Data.DataTable();
                        IdDt.Columns.Add("stringValue");
                        foreach (var item in itemNumbers)
                        {
                            var dr = IdDt.NewRow();
                            dr["stringValue"] = item;
                            IdDt.Rows.Add(dr);
                        }

                        var strParam = new SqlParameter("itemnumbers", IdDt);
                        strParam.SqlDbType = System.Data.SqlDbType.Structured;
                        strParam.TypeName = "dbo.stringValues";

                        var customerId = new SqlParameter()
                        {
                            ParameterName = "@CustomerId",
                            Value = customerShoppingCart.CustomerId
                        };
                        var storeId = new SqlParameter()
                        {
                            ParameterName = "@StoreId",
                            Value = storeIds.FirstOrDefault()
                        };
                        var storePayLaterLimit = context.Database.SqlQuery<StorePayLaterLimitDc>("exec Sp_GetstorecreditLimit @CustomerId , @StoreId, @itemnumbers", customerId, storeId, strParam).FirstOrDefault();
                        if (storePayLaterLimit != null && storePayLaterLimit.CreditLimit > 0)
                        {
                            customerShoppingCartDc.IsPayLater = true;
                            customerShoppingCartDc.PayLaterLimit = storePayLaterLimit.CreditLimit;
                        }
                    }
                    #endregion
                    if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount))
                    {
                        if (storeIds.All(x => x == storeIds.Max(y => y))
                            && deliveryCharges.Any(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount)
                            )
                            DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));
                        else
                            DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (!x.storeId.HasValue || x.storeId.Value == 0) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));

                        if (ActiveCustomer.IsPrimeCustomer)
                            DeliveryAmount = 0;
                    }
                    customerShoppingCart.DeliveryCharges = DeliveryAmount;


                    #region TCS Calculate
                    bool IsConsumer = (ActiveCustomer.CustomerType != null && ActiveCustomer.CustomerType.ToLower() == "consumer") ? true : false;
                    if (!IsConsumer)
                    {
                        GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                        var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(ActiveCustomer.CustomerId, ActiveCustomer.PanNo, context);

                        if (!ActiveCustomer.IsTCSExemption && tcsConfig != null && (tcsConfig.IsAlreadyTcsUsed == true || (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + customerShoppingCart.CartTotalAmt > tcsConfig.TCSAmountLimit)))
                        {
                            customerShoppingCart.TCSPercent = !ActiveCustomer.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                            customerShoppingCart.PreTotalDispatched = tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount;
                            customerShoppingCart.TCSLimit = tcsConfig.TCSAmountLimit;
                        }
                        else
                        {
                            customerShoppingCart.TCSPercent = 0;
                            customerShoppingCart.PreTotalDispatched = 0;
                            customerShoppingCart.TCSLimit = 0;
                        }
                    }
                    #endregion

                }
                customerShoppingCart.ModifiedDate = DateTime.Now;

                var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart);
                customerShoppingCartDc.TCSPercent = customerShoppingCart.TCSPercent;
                customerShoppingCartDc.TCSLimit = customerShoppingCart.TCSLimit;
                customerShoppingCartDc.PreTotalDispatched = customerShoppingCart.PreTotalDispatched;
                customerShoppingCartDc.ApplyOfferId = string.Join(",", ShoppingCartDiscounts.Where(x => (x.DiscountAmount > 0 || x.NewBillingWalletPoint > 0 || BillDiscountItemOfferIds.Contains(x.OfferId.Value)) && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId).ToList());
                customerShoppingCartDc.CartTotalAmt = customerShoppingCart.CartTotalAmt;
                customerShoppingCartDc.CustomerId = customerShoppingCart.CustomerId;
                customerShoppingCartDc.DeamPoint = customerShoppingCart.DeamPoint;
                customerShoppingCartDc.DeliveryCharges = customerShoppingCart.DeliveryCharges;
                customerShoppingCartDc.GeneratedOrderId = customerShoppingCart.GeneratedOrderId;
                customerShoppingCartDc.GrossTotalAmt = customerShoppingCart.GrossTotalAmt;
                customerShoppingCartDc.TotalDiscountAmt = customerShoppingCart.TotalDiscountAmt;
                customerShoppingCartDc.TotalTaxAmount = customerShoppingCart.TotalTaxAmount;
                customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
                customerShoppingCartDc.WarehouseId = customerShoppingCart.WarehouseId;
                customerShoppingCartDc.TotalSavingAmt = customerShoppingCart.TotalSavingAmt;
                customerShoppingCartDc.TotalQty = customerShoppingCart.TotalQty;
                customerShoppingCartDc.WheelCount = customerShoppingCart.WheelCount;
                customerShoppingCartDc.NewBillingWalletPoint = customerShoppingCart.NewBillingWalletPoint;
                customerShoppingCartDc.ShoppingCartItemDcs = ShoppingCartItemDcs;
                customerShoppingCartDc.PeopleId = customerShoppingCart.PeopleId;
                if (customerShoppingCart != null && customerShoppingCart.ShoppingCartDiscounts != null && customerShoppingCart.ShoppingCartDiscounts.Any())
                {
                    customerShoppingCartDc.DiscountDetails = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))
                                                            .Select(x => new DiscountDetail
                                                            {
                                                                DiscountAmount = x.DiscountAmount,
                                                                OfferId = x.OfferId
                                                            }).ToList();
                }

            }


            if (EnableOtherLanguage && customerShoppingCartDc != null && customerShoppingCartDc.ShoppingCartItemDcs != null && customerShoppingCartDc.ShoppingCartItemDcs.Any() && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
            {
                LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();

                ElasticLanguageDataRequests = customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.itemBaseName }).ToList();
                //ElasticLanguageDatas.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = x.itemname }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.OfferFreeItemName }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.Scheme }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.UOM }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SellingUnitName }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.StoreName }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.UnitofQuantity }).ToList());
                //ElasticLanguageDatas.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = x.Message }).ToList());

                if (customerShoppingCartDc.ShoppingCartItemDcs.Any(x => x.IsSuccess == false))
                {
                    //List<DataContracts.ElasticLanguageSearch.ElasticLanguageData> msgElasticLanguageDatas = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageData>();

                    foreach (var cartItem in customerShoppingCartDc.ShoppingCartItemDcs.Where(x => x.IsSuccess == false).ToList())
                    {
                        cartItem.MessageKey = cartItem.Message;
                        cartItem.Message = SCMsgEnum.SCEnum[cartItem.Message].Value;
                        // msgElasticLanguageDatas.Add(new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = cartItem.Message });
                        ElasticLanguageDataRequests.Add(new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = cartItem.Message, IsTranslate = true });
                    }
                    // LanguageConvertHelper.CheckConvertLanguageData(msgElasticLanguageDatas.Distinct().ToList(), lang.ToLower());

                }


                var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                customerShoppingCartDc.ShoppingCartItemDcs.ForEach(x =>
                {
                    x.itemBaseName = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                    x.itemname = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                    if (x.IsSensitive == true && x.IsSensitiveMRP == true)
                    {
                        x.itemname += " " + x.price + " MRP " + x.UnitofQuantity + " " + x.UOM;
                    }
                    else if (x.IsSensitive == true && x.IsSensitiveMRP == false)
                    {
                        x.itemname += " " + x.UnitofQuantity + " " + x.UOM; //item display name 
                    }
                    else if (x.IsSensitive == false && x.IsSensitiveMRP == false)
                    {
                        x.itemname = x.itemBaseName; //item display name
                    }
                    else if (x.IsSensitive == false && x.IsSensitiveMRP == true)
                    {
                        x.itemname += " " + x.price + " MRP";//item display name 
                    }
                    x.OfferFreeItemName = ElasticLanguageDatas.Any(y => y.englishtext == x.OfferFreeItemName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.OfferFreeItemName).converttext : x.OfferFreeItemName;
                    x.SellingUnitName = ElasticLanguageDatas.Any(y => y.englishtext == x.SellingUnitName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SellingUnitName).converttext : x.SellingUnitName;
                    x.UOM = ElasticLanguageDatas.Any(y => y.englishtext == x.UOM) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.UOM).converttext : x.UOM;
                    x.Scheme = ElasticLanguageDatas.Any(y => y.englishtext == x.Scheme) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.Scheme).converttext : x.Scheme;
                    x.UnitofQuantity = ElasticLanguageDatas.Any(y => y.englishtext == x.UnitofQuantity) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.UnitofQuantity).converttext : x.UnitofQuantity;
                    x.StoreName = ElasticLanguageDatas.Any(y => y.englishtext == x.StoreName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.StoreName).converttext : x.StoreName;
                    x.Message = ElasticLanguageDatas.Any(y => y.englishtext == x.Message) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.Message).converttext : x.Message;
                });
            }
            else
            {
                if (customerShoppingCartDc.ShoppingCartItemDcs != null && customerShoppingCartDc.ShoppingCartItemDcs.Any(x => x.IsSuccess == false))
                {
                    foreach (var cartItem in customerShoppingCartDc.ShoppingCartItemDcs.Where(x => x.IsSuccess == false).ToList())
                    {
                        cartItem.MessageKey = cartItem.Message;
                        cartItem.Message = SCMsgEnum.SCEnum[cartItem.Message].Key;
                    }

                }

            }
            return customerShoppingCartDc;
        }


        [AutomaticRetry(Attempts = 0)]
        public CustomerShoppingCartDc RefereshCartSync(CustomerShoppingCart customerShoppingCart, string lang)
        {
            string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            List<ShoppingCartItemDc> ShoppingCartItemDcs = new List<ShoppingCartItemDc>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            int walletPoint = 0;
            if (customerShoppingCart != null)
            {
                customerShoppingCartDc = new CustomerShoppingCartDc
                {
                    CartTotalAmt = 0,
                    CustomerId = customerShoppingCart.CustomerId,
                    DeamPoint = 0,
                    DeliveryCharges = 0,
                    GrossTotalAmt = 0,
                    TotalDiscountAmt = 0,
                    TotalTaxAmount = 0,
                    WalletAmount = 0,
                    WarehouseId = customerShoppingCart.WarehouseId,
                };
                using (var context = new AuthContext())
                {
                    if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any())
                    {
                        var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == customerShoppingCart.WarehouseId && x.isDeleted == false && x.IsActive && !x.IsDistributor).ToList();
                        var ActiveCustomer = context.Customers.Where(x => x.CustomerId == customerShoppingCart.CustomerId).Include(x => x.ConsumerAddress).FirstOrDefault();
                        if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                        {
                            ActiveCustomer.Active = ActiveCustomer.IsB2CApp;
                            var defaultadd = ActiveCustomer.ConsumerAddress.FirstOrDefault(x => x.Default);
                            var cluster = context.Clusters.FirstOrDefault(x => x.WarehouseId == defaultadd.WarehouseId);
                            ActiveCustomer.ShippingAddress = defaultadd.CompleteAddress;
                            ActiveCustomer.Warehouseid = defaultadd.WarehouseId;
                            ActiveCustomer.City = defaultadd.CityName;
                            ActiveCustomer.Cityid = defaultadd.Cityid;
                            ActiveCustomer.State = defaultadd.StateName;
                            ActiveCustomer.ClusterId = cluster.ClusterId;
                            ActiveCustomer.ZipCode = defaultadd.ZipCode;
                            ActiveCustomer.lat = defaultadd.lat;
                            ActiveCustomer.lg = defaultadd.lng;
                            ActiveCustomer.LandMark = defaultadd.LandMark;
                            ActiveCustomer.ShopName = defaultadd.PersonName;
                            ActiveCustomer.Name = defaultadd.PersonName;
                        }
                        var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                        List<int> itemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();
                        itemids = context.itemMasters.Where(x => itemids.Contains(x.ItemId) && x.WarehouseId == ActiveCustomer.Warehouseid).Select(x => x.ItemId).ToList();
                        string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + customerShoppingCart.WarehouseId +
                                          " WHERE a.[CustomerId]=" + customerShoppingCart.CustomerId;
                        var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();


                        string itemIndex = ConfigurationManager.AppSettings["ElasticSearchIndexName"];
                        string query = $"SELECT itemid,active,basecategoryid,basecategoryname,categoryid,categoryname,subcategoryid,subcategoryname,subsubcategoryid,subsubcategoryname,itemlimitqty," +
                               $"isitemlimit,itemnumber,companyid,warehouseid,itemname,price,unitprice,logourl,minorderqty,totaltaxpercentage,marginpoint,dreampoint,isoffer,unitofquantity," +
                               $"uom,itembasename,deleted,isflashdealused,itemmultimrpid,netpurchaseprice,billlimitqty,issensitive,issensitivemrp,hindiname,offercategory,offerstarttime,offerendtime," +
                               $"offerqtyavaiable,offerqtyconsumed,offerid,offertype,offerwalletpoint,offerfreeitemid,freeitemid,offerpercentage,offerfreeitemname,offerfreeitemimage," +
                               $"offerfreeitemquantity,offerminimumqty,flashdealspecialprice,flashdealmaxqtypersoncantake,distributionprice,distributorshow,itemapptype,isflashdealstart,warehousename," +
                               $"cityid,cityname,Rating,Margin,DreamPointCal" +
                               $" from {itemIndex} where " +
                               $"  active=true and deleted=false and  isdiscontinued=false and (isitemlimit=false or (isitemlimit=true and itemlimitqty>0 and itemlimitqty-minorderqty>0 )) and (itemapptype=0 or itemapptype=1)";

                        if (itemids != null && itemids.Any())
                        {
                            query += $" and itemid in ({string.Join(",", itemids)})";
                        }


                        // ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemResponse> elasticSqlHelper1 = new ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemResponse>();

                        //var dbItems = AsyncContext.Run(async () => (await elasticSqlHelper1.GetListAsync(query)).ToList());

                        //var newdata = dbItems.Select(x => new factoryItemdata
                        //{
                        //    active = x.active,
                        //    BaseCategoryId = x.basecategoryid,
                        //    BillLimitQty = x.billlimitqty,
                        //    Categoryid = x.categoryid,
                        //    CompanyId = x.companyid,
                        //    //CurrentStartTime = x.currentstarttime,
                        //    Deleted = x.deleted,
                        //    DistributionPrice = x.distributionprice,
                        //    DistributorShow = x.distributorshow,
                        //    marginPoint = x.marginpoint,
                        //    dreamPoint = x.dreampoint,
                        //    FlashDealMaxQtyPersonCanTake = x.flashdealmaxqtypersoncantake,
                        //    FlashDealSpecialPrice = x.flashdealspecialprice,
                        //    FreeItemId = x.freeitemid,
                        //    HindiName = x.hindiname,
                        //    IsFlashDealUsed = x.isflashdealused,
                        //    IsItemLimit = x.isitemlimit,
                        //    IsOffer = x.isoffer,
                        //    //IsPrimeItem = x.isprimeitem,
                        //    IsSensitive = x.issensitive,
                        //    IsSensitiveMRP = x.issensitivemrp,
                        //    ItemAppType = x.itemapptype,
                        //    itemBaseName = x.itembasename,
                        //    ItemId = x.itemid,
                        //    ItemlimitQty = x.itemlimitqty,
                        //    ItemMultiMRPId = x.itemmultimrpid,
                        //    itemname = x.itemname,
                        //    ItemNumber = x.itemnumber,
                        //    LogoUrl = x.logourl,
                        //    MinOrderQty = x.minorderqty,
                        //    NetPurchasePrice = x.netpurchaseprice,
                        //    OfferCategory = x.offercategory,
                        //    OfferEndTime = x.offerendtime,
                        //    OfferFreeItemId = x.offerfreeitemid,
                        //    OfferFreeItemImage = x.offerfreeitemimage,
                        //    OfferFreeItemName = x.offerfreeitemname,
                        //    OfferFreeItemQuantity = x.offerfreeitemquantity,
                        //    OfferId = x.offerid,
                        //    OfferMinimumQty = x.offerminimumqty,
                        //    OfferPercentage = x.offerpercentage,
                        //    OfferQtyAvaiable = x.offerqtyavaiable,
                        //    OfferQtyConsumed = x.offerqtyconsumed,
                        //    OfferStartTime = x.offerstarttime,
                        //    OfferType = x.offertype,
                        //    OfferWalletPoint = x.offerwalletpoint,
                        //    price = x.price,
                        //    // PrimePrice = x.primeprice,
                        //    // promoPerItems = x.promoperitems,
                        //    //  Scheme = x.scheme,
                        //    UnitofQuantity = x.unitofquantity,
                        //    UOM = x.uom,
                        //    SubCategoryId = x.subcategoryid,
                        //    SubsubCategoryid = x.subsubcategoryid,
                        //    WarehouseId = x.warehouseid,
                        //    UnitPrice = x.unitprice,
                        //    TotalTaxPercentage = x.totaltaxpercentage
                        //}).ToList();


                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();


                        var orderIdDt = new DataTable();
                        orderIdDt.Columns.Add("IntValue");
                        foreach (var item in itemids)
                        {
                            var dr = orderIdDt.NewRow();
                            dr["IntValue"] = item;
                            orderIdDt.Rows.Add(dr);
                        }
                        var param = new SqlParameter("ItemIds", orderIdDt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandText = "[dbo].[GetShoppingCardItem]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(param);

                        // Run the sproc
                        var reader = cmd.ExecuteReader();
                        var newdata = ((IObjectContextAdapter)context)
                        .ObjectContext
                        .Translate<factoryItemdata>(reader).ToList();

                        string apptype = customerShoppingCart.PeopleId > 0 ? "Sales App" : "Retailer App";
                        if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                            apptype = ConsumerApptype;

                        //string apptype = customerShoppingCart.PeopleId > 0 ? "Sales App" : "Retailer App";
                        var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                        List<OrderOfferDc> activeOfferids = new List<OrderOfferDc>();
                        if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                            activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == apptype)).Select(x => new OrderOfferDc { OfferId = x.OfferId, QtyAvaiable = x.QtyAvaiable, IsDispatchedFreeStock = x.IsDispatchedFreeStock, MinOrderQuantity = x.MinOrderQuantity, NoOffreeQuantity = x.NoOffreeQuantity }).ToList() : null;
                        else
                            activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == apptype || x.OfferAppType == "Both")).Select(x => new OrderOfferDc { OfferId = x.OfferId, QtyAvaiable = x.QtyAvaiable, IsDispatchedFreeStock = x.IsDispatchedFreeStock, MinOrderQuantity = x.MinOrderQuantity, NoOffreeQuantity = x.NoOffreeQuantity }).ToList() : null;

                        List<int> Primeitemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsPrimeItem.HasValue && x.IsPrimeItem.Value && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();

                        var itemMultiMRPIds = newdata.Where(x => Primeitemids.Contains(x.ItemId)).Select(x => x.ItemMultiMRPId).Distinct().ToList();
                        var PrimeItems = itemMultiMRPIds.Any() ? context.PrimeItemDetails.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.CityId == ActiveCustomer.Cityid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList() : null;
                        var primeCustomer = context.PrimeCustomers.FirstOrDefault(x => x.CustomerId == ActiveCustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                        ActiveCustomer.IsPrimeCustomer = primeCustomer != null && primeCustomer.StartDate <= DateTime.Now && primeCustomer.EndDate >= DateTime.Now;
                        var dealItems = context.DealItems.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == ActiveCustomer.Warehouseid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList();

                        DateTime CurrentDate = !ActiveCustomer.IsPrimeCustomer ? DateTime.Now.AddHours(MemberShipHours) : DateTime.Now;
                        int hrs = !ActiveCustomer.IsPrimeCustomer ? MemberShipHours : 0;
                        BackendOrderController backendOrderController = new BackendOrderController();
                        foreach (var it in newdata)
                        {
                            double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                            it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice
                                                                   , it.WholeSalePrice ?? 0
                                                                   , it.TradePrice ?? 0, cprice);
                            if (PrimeItems != null && PrimeItems.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty))
                            {
                                var primeItem = PrimeItems.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty);
                                it.IsPrimeItem = true;
                                it.PrimePrice = primeItem.PrimePercent > 0 ? Convert.ToDecimal(it.UnitPrice - (it.UnitPrice * Convert.ToDouble(primeItem.PrimePercent) / 100)) : primeItem.PrimePrice;
                            }
                            //Condition for offer end.
                            if (!inActiveCustomer)
                            {
                                if (!(it.OfferStartTime.HasValue && it.OfferStartTime.Value.AddHours(hrs) <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
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
                                else if (it.OfferStartTime.HasValue && (it.OfferStartTime.Value.AddHours(hrs) <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
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
                                    if (activeOfferids != null && activeOfferids.Any() && activeOfferids.Any(x => x.OfferId == it.OfferId) && it.IsOffer)
                                        it.IsOffer = true;
                                    else
                                        it.IsOffer = false;
                                }

                                if (customerShoppingCart.PeopleId > 0 && it.OfferType == "FlashDeal")
                                {
                                    it.IsOffer = false;
                                    it.FlashDealSpecialPrice = 0;
                                    it.OfferCategory = 0;
                                }
                            }
                            else
                            {
                                it.IsOffer = false;
                                it.FlashDealSpecialPrice = 0;
                                it.OfferCategory = 0;
                            }

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
                                if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                                    it.marginPoint = it.UnitPrice > 0 ? (((it.price - it.UnitPrice) * 100) / it.price) : 0;
                                else
                                    it.marginPoint = it.UnitPrice > 0 ? (((it.price - it.UnitPrice) * 100) / it.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 
                            }
                            else
                            {
                                it.marginPoint = 0;
                            }

                            if (it.IsOffer && it.OfferCategory.HasValue && it.OfferCategory.Value == 1)
                            {
                                if (it.OfferType == "WalletPoint" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferWalletPoint.HasValue && it.OfferWalletPoint.Value > 0)
                                {
                                    var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                    if (item.qty >= it.OfferMinimumQty)
                                    {
                                        var FreeWalletPoint = it.OfferWalletPoint.Value;
                                        int calfreeItemQty = item.qty / it.OfferMinimumQty.Value;
                                        FreeWalletPoint *= calfreeItemQty;
                                        item.TotalFreeWalletPoint = FreeWalletPoint;
                                        walletPoint += Convert.ToInt32(FreeWalletPoint);
                                    }

                                }
                                else if (it.OfferType == "ItemMaster" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferFreeItemQuantity.HasValue && it.OfferFreeItemQuantity.Value > 0)
                                {
                                    var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                    if (item.qty >= it.OfferMinimumQty)
                                    {
                                        var cartqty = it.IsItemLimit && item.qty > it.ItemlimitQty ? it.ItemlimitQty : item.qty;
                                        var FreeItemQuantity = it.OfferFreeItemQuantity.Value;
                                        int calfreeItemQty = Convert.ToInt32(cartqty / it.OfferMinimumQty);
                                        FreeItemQuantity *= calfreeItemQty;
                                        item.FreeItemqty = 0;
                                        if (FreeItemQuantity > 0)
                                        {
                                            item.FreeItemqty = FreeItemQuantity;
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi")
                            {
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
                            }
                        }

                        CustomersManager manager = new CustomersManager();
                        List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetCustomerBillDiscount(customerShoppingCart.CustomerId, apptype);
                        List<int> applyedOfferIds = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList();
                        if ((billDiscountOfferDcs.Any() && applyedOfferIds != null && applyedOfferIds.Any()) || billDiscountOfferDcs.Any(x => x.IsAutoApply))
                        {
                            foreach (var Offer in billDiscountOfferDcs.Where(x => applyedOfferIds.Contains(x.OfferId) || x.IsAutoApply))
                            {
                                ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                                double totalamount = 0;
                                var OrderLineItems = 0;
                                //if (Offer.OfferOn != "ScratchBillDiscount")
                                //{
                                List<int> Itemids = new List<int>();
                                if (Offer.BillDiscountType == "category" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                    var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                    var ids = Offer.OfferBillDiscountItems.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                    var notids = Offer.OfferBillDiscountItems.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                    var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                    if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                    {
                                        var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                        CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                    }
                                    Itemids = newdata.Where(x => (!ids.Any() || ids.Contains(x.Categoryid)) && !notids.Contains(x.Categoryid)
                                    && !itemoutofferlist.Contains(x.ItemId)
                                    && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                    && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    if (CItemIds.Any())
                                    {
                                        itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    }
                                    totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                                else if (Offer.BillDiscountType == "subcategory" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                    var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                    var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                    if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                    {
                                        var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                        CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                    }
                                    Itemids = newdata.Where(x =>
                                     (!Offer.OfferBillDiscountItems.Where(y => y.IsInclude).Any() || Offer.OfferBillDiscountItems.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                                     && !Offer.OfferBillDiscountItems.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                                    && !itemoutofferlist.Contains(x.ItemId)
                                    && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                    && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    if (CItemIds.Any())
                                    {
                                        itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    }
                                    totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                                else if (Offer.BillDiscountType == "brand" && Offer.OfferBillDiscountItems.Any())
                                {
                                    var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                    var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                    var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                    if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                    {
                                        var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                        CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                    }
                                    Itemids = newdata.Where(x =>
                                    (
                                     !Offer.OfferBillDiscountItems.Where(y => y.IsInclude).Any() ||
                                    Offer.OfferBillDiscountItems.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                    )
                                    && !Offer.OfferBillDiscountItems.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                    && !itemoutofferlist.Contains(x.ItemId)
                                    && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                    && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    if (CItemIds.Any())
                                    {
                                        itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    }
                                    totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                                else if (Offer.BillDiscountType == "items" && Offer.IsBillDiscountFreebiesItem)
                                {
                                    var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                    var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                    var itemnumbermrps = context.itemMasters.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => new DataContracts.BillDiscount.offerItemMRP { ItemNumber = x.Number, ItemMultiMRPId = x.ItemMultiMRPId }).ToList();

                                    var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                    if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                    {
                                        var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                        CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                    }
                                    Itemids = newdata.Where(x => (!iteminofferlist.Any() || itemnumbermrps.Select(y => y.ItemNumber + "" + y.ItemMultiMRPId).Contains(x.ItemNumber + "" + x.ItemMultiMRPId))
                                      && !itemoutofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    if (CItemIds.Any())
                                    {
                                        Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    }
                                    totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                                    var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                    var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                                    //if (Offer.OfferItems.FirstOrDefault().IsInclude)
                                    //{
                                    //    Itemids = newdata.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    //}
                                    var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                    if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                    {
                                        var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                        CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                    }
                                    Itemids = newdata.Where(x => (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                    && !itemoutofferlist.Contains(x.ItemId)
                                    ).Select(x => x.ItemId).ToList();
                                    if (CItemIds.Any())
                                    {
                                        Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    }
                                    totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                                    var catIdoutofferlist = Offer.OfferBillDiscountItems.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                    var catIdinofferlist = Offer.OfferBillDiscountItems.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                    // var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                    var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                    if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                    {
                                        var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                        CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                    }
                                    Itemids = newdata.Where(x => (catIdinofferlist.Any() || catIdinofferlist.Contains(x.Categoryid))
                                    && !catIdoutofferlist.Contains(x.Categoryid)
                                    ).Select(x => x.ItemId).ToList();
                                    var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    if (catIdoutofferlist.Any())
                                        incluseItemIds = newdata.Where(x => !catIdoutofferlist.Contains(x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    if (CItemIds.Any())
                                    {
                                        incluseItemIds = newdata.Where(x => CItemIds.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)
                                  ).Select(x => x.ItemId).ToList();
                                    }
                                    totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                    OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                    var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).ToList();
                                    if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                    {
                                        List<int> lineItemValueItemExists = new List<int>();
                                        foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                        {
                                            int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) >= item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
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

                                if (Offer.BillDiscountRequiredItems != null && Offer.BillDiscountRequiredItems.Any())
                                {
                                    bool IsRequiredItemExists = true;
                                    var objectIds = Offer.BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                                    if (Offer.BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                                    {
                                        objectIds.AddRange(newdata.Where(x => Offer.BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                                    }
                                    var cartrequiredItems = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                    foreach (var reqitem in Offer.BillDiscountRequiredItems)
                                    {
                                        if (reqitem.ObjectType == "Item")
                                        {
                                            var reqobjectids = reqitem.ObjectId.Split(',').Select(z => Convert.ToInt32(z)).ToList();
                                            var cartitem = cartrequiredItems.Where(x => reqobjectids.Contains(x.ItemMultiMRPId));
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
                                            var reqobjectids = reqitem.ObjectId.Split(',').Select(z => z).ToList();
                                            var multiMrpIds = newdata.Where(x => reqobjectids.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
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


                                if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                                {
                                    totalamount = Offer.MaxBillAmount;
                                }
                                else if (Offer.BillAmount > totalamount)
                                {
                                    totalamount = 0;
                                }

                                if (Offer.LineItem > 0 && Offer.LineItem > OrderLineItems)
                                {
                                    totalamount = 0;
                                }

                                if (Offer.OfferOn == "ScratchBillDiscount" && !Offer.IsScratchBDCode)
                                {
                                    totalamount = 0;
                                }


                                bool IsUsed = true;
                                if (!Offer.IsUseOtherOffer && ShoppingCartDiscounts.Any())
                                    IsUsed = false;

                                if (IsUsed && totalamount > 0)
                                {
                                    if (Offer.BillDiscountOfferOn == "Percentage")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                    }
                                    else if (Offer.BillDiscountOfferOn == "FreeItem")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = 0;
                                    }
                                    else if (Offer.BillDiscountOfferOn == "DynamicAmount")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = Offer.BillDiscountWallet.Value;
                                    }
                                    else
                                    {
                                        int WalletPoint = 0;
                                        if (Offer.WalletType == "WalletPercentage")
                                        {
                                            WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * (Offer.BillDiscountWallet ?? 0) / 100));
                                            WalletPoint = WalletPoint * 10;
                                        }
                                        else
                                        {
                                            WalletPoint = Convert.ToInt32(Offer.BillDiscountWallet.Value);
                                        }
                                        if (Offer.ApplyOn == "PostOffer")
                                        {
                                            ShoppingCartDiscount.DiscountAmount = 0;
                                            ShoppingCartDiscount.NewBillingWalletPoint = WalletPoint;
                                        }
                                        else
                                        {
                                            ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10); ;
                                            ShoppingCartDiscount.NewBillingWalletPoint = 0;
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
                                            if (Offer.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                            {
                                                ShoppingCartDiscount.DiscountAmount = Offer.MaxDiscount * walletmultipler;
                                            }
                                            if (Offer.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint)
                                            {
                                                ShoppingCartDiscount.NewBillingWalletPoint = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                                            }
                                        }
                                    }

                                    ShoppingCartDiscount.OfferId = Offer.OfferId;
                                    ShoppingCartDiscount.CreatedDate = DateTime.Now;
                                    ShoppingCartDiscount.IsActive = Offer.IsActive;
                                    ShoppingCartDiscount.IsDeleted = false;
                                    ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                                }
                            }
                        }
                        customerShoppingCart.ShoppingCartDiscounts = ShoppingCartDiscounts;

                        customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList().ForEach(x =>
                        {
                            if (newdata.Any(y => y.ItemId == x.ItemId))
                            {
                                var item = newdata.FirstOrDefault(y => y.ItemId == x.ItemId);
                                x.ItemMultiMRPId = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemMultiMRPId;
                                x.ItemNumber = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).ItemNumber;
                                x.ItemName = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).itemname;
                                if (newdata.Any(y => y.ItemId == x.ItemId && y.OfferCategory == 2 && y.IsOffer))
                                {
                                    x.UnitPrice = x.UnitPrice;
                                }
                                else if (x.IsDealItem.HasValue && x.IsDealItem.Value && dealItems.Any(y => y.ItemMultiMRPId == item.ItemMultiMRPId && y.MinOrderQty == item.MinOrderQty))
                                {
                                    x.UnitPrice = dealItems.FirstOrDefault(y => y.ItemMultiMRPId == item.ItemMultiMRPId && y.MinOrderQty == item.MinOrderQty).DealPrice;
                                }
                                else
                                {
                                    x.UnitPrice = x.IsPrimeItem.HasValue && x.IsPrimeItem.Value && newdata.FirstOrDefault(y => y.ItemId == x.ItemId).IsPrimeItem ? Convert.ToDouble(newdata.FirstOrDefault(y => y.ItemId == x.ItemId).PrimePrice) : x.UnitPrice;
                                }
                            }
                        });

                        ShoppingCartItemDcs = newdata.Select(a => new ShoppingCartItemDc
                        {
                            BaseCategoryId = a.BaseCategoryId,
                            IsItemLimit = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? true : a.IsItemLimit,
                            ItemlimitQty = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) && dealItems.Any(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty) ? (dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).OrderLimit < dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalLimit - dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalConsume ? dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).OrderLimit : dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalLimit - dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalConsume) : a.ItemlimitQty,
                            BillLimitQty = a.BillLimitQty,
                            WarehouseId = a.WarehouseId,
                            CompanyId = a.CompanyId,
                            Categoryid = a.Categoryid,
                            Discount = a.Discount,
                            ItemId = a.ItemId,
                            ItemNumber = a.ItemNumber,
                            HindiName = a.HindiName,
                            IsSensitive = a.IsSensitive,
                            IsSensitiveMRP = a.IsSensitiveMRP,
                            UnitofQuantity = a.UnitofQuantity,
                            UOM = a.UOM,
                            itemname = a.itemname,
                            LogoUrl = a.LogoUrl,
                            MinOrderQty = !string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer" ? 1 : a.MinOrderQty,
                            price = a.price,
                            SubCategoryId = a.SubCategoryId,
                            SubsubCategoryid = a.SubsubCategoryid,
                            TotalTaxPercentage = a.TotalTaxPercentage,
                            SellingUnitName = a.SellingUnitName,
                            SellingSku = a.SellingSku,
                            UnitPrice = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value).UnitPrice : a.UnitPrice,
                            VATTax = a.VATTax,
                            itemBaseName = a.itemBaseName,
                            active = a.active,
                            marginPoint = a.marginPoint.HasValue ? a.marginPoint.Value : 0,
                            promoPerItems = a.promoPerItems.HasValue ? a.promoPerItems.Value : 0,
                            NetPurchasePrice = a.NetPurchasePrice,
                            IsOffer = a.IsOffer,
                            Deleted = a.Deleted,
                            OfferCategory = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? 3 : (a.OfferCategory.HasValue ? a.OfferCategory.Value : 0),
                            OfferStartTime = a.OfferStartTime,
                            OfferEndTime = a.OfferEndTime,
                            OfferQtyAvaiable = a.OfferQtyAvaiable.HasValue ? a.OfferQtyAvaiable.Value : 0,
                            OfferQtyConsumed = a.OfferQtyConsumed.HasValue ? a.OfferQtyConsumed.Value : 0,
                            OfferId = a.OfferId.HasValue ? a.OfferId.Value : 0,
                            OfferType = a.OfferType,
                            dreamPoint = a.dreamPoint.HasValue ? a.dreamPoint.Value : 0,
                            OfferWalletPoint = a.OfferWalletPoint.HasValue ? a.OfferWalletPoint.Value : 0,
                            OfferFreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                            OfferPercentage = a.OfferPercentage.HasValue ? a.OfferPercentage.Value : 0,
                            OfferFreeItemName = a.OfferFreeItemName,
                            OfferFreeItemImage = a.OfferFreeItemImage,
                            OfferFreeItemQuantity = a.OfferFreeItemQuantity.HasValue ? a.OfferFreeItemQuantity.Value : 0,
                            OfferMinimumQty = a.OfferMinimumQty.HasValue ? a.OfferMinimumQty.Value : 0,
                            FlashDealSpecialPrice = a.FlashDealSpecialPrice.HasValue ? a.FlashDealSpecialPrice.Value : 0,
                            FlashDealMaxQtyPersonCanTake = a.FlashDealMaxQtyPersonCanTake.HasValue ? a.FlashDealMaxQtyPersonCanTake.Value : 0,
                            FreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                            IsPrimeItem = a.IsPrimeItem,
                            PrimePrice = Convert.ToDouble(a.PrimePrice),
                            IsDealItem = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).IsDealItem,
                            qty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).qty,
                            CartUnitPrice = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).UnitPrice,
                            TotalFreeItemQty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).FreeItemqty,
                            TotalFreeWalletPoint = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).TotalFreeWalletPoint,
                        }).ToList();
                        foreach (var item in ShoppingCartItemDcs)
                        {
                            item.IsSuccess = true;
                            bool valid = true;
                            if (!item.active || item.Deleted)
                            {
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम एक्टिव नहीं है" : "Item is not Active.";
                            }

                            var mod = Convert.ToDecimal(item.qty) % item.MinOrderQty;
                            if (mod != 0)
                            {
                                valid = false;
                                item.IsSuccess = false;
                                item.qty = 0;
                                item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की क्वांटिटी मल्टीप्ल नहीं है मिनिमम आर्डर क्वांटिटी के " : "Item qty is not multiples of min order qty.";
                            }

                            if (valid)
                            {
                                if (item.IsOffer && item.OfferType == "FlashDeal")
                                {
                                    if (item.FlashDealSpecialPrice.HasValue && item.CartUnitPrice != item.FlashDealSpecialPrice.Value)
                                    {
                                        valid = false;
                                        item.IsSuccess = false;
                                        item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की यूनिट प्राइस चेंज हो गई है" : "Item Unit Price has changed.";
                                        item.NewUnitPrice = item.FlashDealSpecialPrice.Value;
                                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                        {
                                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.FlashDealSpecialPrice.Value;
                                        }
                                    }
                                }
                                else
                                {
                                    if (ActiveCustomer.IsPrimeCustomer && item.IsPrimeItem)
                                    {
                                        if (item.PrimePrice != item.CartUnitPrice)
                                        {
                                            valid = false;
                                            item.IsSuccess = false;
                                            item.Message = "Item " + MemberShipName + " Price has changed";
                                            item.NewUnitPrice = item.PrimePrice;
                                            if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                            {
                                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.PrimePrice;
                                            }
                                        }
                                    }
                                    else if (item.IsDealItem.HasValue && item.IsDealItem.Value)
                                    {
                                        if (dealItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty))
                                        {
                                            var dealitem = dealItems.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty);
                                            if (dealitem.DealPrice != item.CartUnitPrice)
                                            {
                                                valid = false;
                                                item.IsSuccess = false;
                                                item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की यूनिट प्राइस चेंज हो गई है" : "Item Unit Price has changed.";
                                                item.NewUnitPrice = dealitem.DealPrice;
                                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                                {
                                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = dealitem.DealPrice;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            valid = false;
                                            item.IsSuccess = false;
                                            item.Message = "Item is not Active";
                                        }
                                    }
                                    else if (item.UnitPrice != item.CartUnitPrice)
                                    {
                                        valid = false;
                                        item.IsSuccess = false;
                                        item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम एक्टिव नहीं है" : "Item Unit Price has changed.";
                                        item.NewUnitPrice = item.UnitPrice;
                                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                        {
                                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.UnitPrice;
                                        }
                                    }
                                }
                            }
                            if (!(item.IsOffer && item.OfferType == "FlashDeal"))
                            {
                                if (valid && item.IsItemLimit && item.ItemlimitQty < item.qty)
                                {
                                    item.qty = item.qty > item.ItemlimitQty ? item.ItemlimitQty : (item.ItemlimitQty - item.qty);
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की लिमिट ख़तम हो गई है" : "Item Limit Exceeded.";
                                    if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                    {
                                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                                    }
                                }
                                if (valid && item.BillLimitQty > 0 && item.BillLimitQty < item.qty)
                                {
                                    item.qty = item.qty > item.BillLimitQty ? item.BillLimitQty : (item.ItemlimitQty - item.qty);
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की बिल लिमिट ख़तम हो गई है" : "Item Bill Limit Exceeded.";
                                    if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                    {
                                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                                    }
                                }
                            }
                            else
                            {
                                item.BillLimitQty = 0;
                            }

                            if (valid && activeOfferids != null && item.OfferFreeItemId > 0 && item.OfferFreeItemQuantity > 0)
                            {
                                var offer = activeOfferids.FirstOrDefault(x => x.OfferId == item.OfferId);
                                if (offer.QtyAvaiable < item.TotalFreeItemQty)
                                {
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "फ्री आइटम ऑफर ख़तम हो गया है" : "Free Item expired.";
                                }

                                #region 8dec2021 Freebies Changes
                                //else if (!offer.IsDispatchedFreeStock && item.ItemId == item.FreeItemId && item.IsItemLimit && (item.ItemlimitQty - item.qty) < item.TotalFreeItemQty)
                                //{
                                //    int multiply = Convert.ToInt32(item.qty / offer.MinOrderQuantity);
                                //    int maxFreeItemqty = (multiply * offer.NoOffreeQuantity) > (item.ItemlimitQty - item.qty) ? (item.ItemlimitQty - item.qty) : multiply * offer.NoOffreeQuantity;
                                //    int maxMainItemqty = maxFreeItemqty * offer.MinOrderQuantity;

                                //    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = maxMainItemqty;
                                //    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).FreeItemqty = maxFreeItemqty;
                                //    item.qty = maxMainItemqty;
                                //    item.TotalFreeItemQty = maxFreeItemqty;
                                //    valid = false;
                                //    item.IsSuccess = false;
                                //    item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आप को फ्री आइटम " + maxFreeItemqty + " क्वांटिटी से ज्यादा नहीं मिलेंगे।" : "You will not get more than " + maxFreeItemqty + " quantity of free items.";
                                //}
                                //else if (!offer.IsDispatchedFreeStock && item.ItemId != item.OfferFreeItemId)
                                //{
                                //    var FreeItem = context.itemMasters.Where(x => x.ItemId == item.OfferFreeItemId).Select(x => new { x.WarehouseId, x.ItemMultiMRPId }).FirstOrDefault();
                                //    var freelimit = context.ItemLimitMasterDB.FirstOrDefault(x => x.WarehouseId == FreeItem.WarehouseId && x.ItemMultiMRPId == FreeItem.ItemMultiMRPId);

                                //    if (freelimit != null && freelimit.IsItemLimit && (freelimit.ItemlimitQty) < item.TotalFreeItemQty)
                                //    {
                                //        int multiply = Convert.ToInt32(item.qty / offer.MinOrderQuantity);
                                //        int maxFreeItemqty = (multiply * offer.NoOffreeQuantity) > freelimit.ItemlimitQty ? freelimit.ItemlimitQty : multiply * offer.NoOffreeQuantity;
                                //        int maxMainItemqty = maxFreeItemqty * offer.MinOrderQuantity;
                                //        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).FreeItemqty = maxFreeItemqty;
                                //        //customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = maxMainItemqty;
                                //        // item.qty = maxMainItemqty;
                                //        item.TotalFreeItemQty = maxFreeItemqty;
                                //        valid = false;
                                //        item.IsSuccess = false;
                                //        item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आप को फ्री आइटम " + maxFreeItemqty + " क्वांटिटी से ज्यादा नहीं मिलेंगे।" : "You will not get more than " + maxFreeItemqty + " quantity of free items.";
                                //    }
                                //}
                                #endregion
                            }


                        }



                        ////need to confirm from sudeep sir
                        if (ShoppingCartItemDcs != null && activeOfferids != null && ShoppingCartItemDcs.Any(x => x.FreeItemId > 0 && x.OfferFreeItemQuantity > 0))
                        {
                            foreach (var item in ShoppingCartItemDcs.GroupBy(x => new { x.FreeItemId, x.ItemNumber, x.OfferId, x.ItemMultiMRPId }))
                            {
                                if (item.Sum(x => x.OfferFreeItemQuantity) > 0)
                                {
                                    var freeItemoffer = activeOfferids.FirstOrDefault(x => x.OfferId == item.Key.OfferId);
                                    if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.Sum(x => x.TotalFreeItemQty))
                                    {
                                        var qtyAvailable = Convert.ToInt32(freeItemoffer.QtyAvaiable);
                                        foreach (var shoppingCart in ShoppingCartItemDcs.Where(x => x.ItemNumber == item.Key.ItemNumber))
                                        {
                                            if (shoppingCart.TotalFreeItemQty > qtyAvailable)
                                            {
                                                shoppingCart.IsSuccess = false;
                                                shoppingCart.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "फ्री आइटम ऑफर ख़तम हो गया है" : "Free Item expired.";
                                            }
                                            else
                                            {
                                                qtyAvailable = qtyAvailable - shoppingCart.TotalFreeItemQty;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        #region TCS Calculate
                        string fy = (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString();
                        MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
                        var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();
                        customerShoppingCart.TCSPercent = 0;
                        if (tcsConfig != null)
                        {
                            MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                            var tcsCustomer = mHelper.Select(x => x.CustomerId == ActiveCustomer.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                            if (tcsCustomer != null && tcsCustomer.TotalPurchase >= tcsConfig.TCSAmountLimit)
                            {
                                customerShoppingCart.TCSPercent = string.IsNullOrEmpty(ActiveCustomer.PanNo) ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                            }
                        }
                        #endregion
                        customerShoppingCart.TotalDiscountAmt = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.DiscountAmount) : 0;
                        customerShoppingCart.NewBillingWalletPoint = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.NewBillingWalletPoint) : 0;
                        customerShoppingCart.DeamPoint = newdata.Where(x => x.dreamPoint.HasValue).Sum(x => x.dreamPoint.Value * customerShoppingCart.ShoppingCartItems.FirstOrDefault(y => y.ItemId == x.ItemId).qty);
                        customerShoppingCart.CartTotalAmt = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice) - customerShoppingCart.TotalDiscountAmt;
                        customerShoppingCart.TotalTaxAmount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.TaxAmount);

                        customerShoppingCart.GrossTotalAmt = Math.Round(customerShoppingCart.CartTotalAmt, 0, MidpointRounding.AwayFromZero);
                        customerShoppingCart.WalletAmount = walletPoint;
                        customerShoppingCart.TotalSavingAmt = ShoppingCartItemDcs.Sum(x => (x.qty * x.price) - (x.qty * x.CartUnitPrice));
                        customerShoppingCart.TotalQty = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                        var wheelConfig = context.CompanyWheelConfiguration.FirstOrDefault();
                        int lineItemCount = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && !x.IsFreeItem && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                        decimal wheelAmount = 0;
                        customerShoppingCart.WheelCount = 0;
                        int totalamt = Convert.ToInt32(Math.Truncate(customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt));
                        if ((wheelConfig.IsKPPRequiredWheel && ActiveCustomer.IsKPP) || !ActiveCustomer.IsKPP)
                        {
                            if (wheelConfig != null && wheelConfig.OrderAmount > 0 && wheelConfig.LineItemCount > 0)
                            {
                                if (lineItemCount >= wheelConfig.LineItemCount)
                                {
                                    wheelAmount = wheelConfig.OrderAmount;
                                    customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(totalamt / wheelAmount));
                                }
                            }
                            else if (wheelConfig != null && wheelConfig.OrderAmount > 0)
                            {
                                wheelAmount = wheelConfig.OrderAmount;
                                customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(totalamt / wheelAmount));
                            }
                            else if (wheelConfig != null && wheelConfig.LineItemCount > 0 && lineItemCount >= wheelConfig.LineItemCount)
                            {
                                wheelAmount = 0;
                                customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(Convert.ToDecimal(lineItemCount) / wheelConfig.LineItemCount));
                            }
                        }
                        //customerShoppingCart.WheelCount = Convert.ToInt32(Math.Truncate((customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt) / 4000));
                        customerShoppingCart.SkCode = ActiveCustomer != null ? ActiveCustomer.Skcode : "";
                        customerShoppingCart.Mobile = ActiveCustomer != null ? ActiveCustomer.Mobile : "";
                        customerShoppingCart.ShopName = ActiveCustomer != null ? ActiveCustomer.ShopName : "";
                        customerShoppingCart.City = ActiveCustomer != null ? ActiveCustomer.City : "";
                        double TotalAmount = customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt;
                        double DeliveryAmount = 0;
                        var storeIds = ShoppingCartItemDcs.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();
                        if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount))
                        {
                            if (storeIds.All(x => x == storeIds.Max(y => y))
                                && deliveryCharges.Any(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount)
                            )
                                DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));
                            else
                                DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (!x.storeId.HasValue || x.storeId.Value == 0) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));

                            if (ActiveCustomer.IsPrimeCustomer)
                                DeliveryAmount = 0;
                        }
                        customerShoppingCart.DeliveryCharges = DeliveryAmount;

                    }
                    var result = mongoDbHelper.ReplaceWithoutFind(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                }
                customerShoppingCartDc.TCSPercent = customerShoppingCart.TCSPercent;
                customerShoppingCartDc.ApplyOfferId = string.Join(",", ShoppingCartDiscounts.Where(x => (x.DiscountAmount > 0 || x.NewBillingWalletPoint > 0) && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId).ToList());
                customerShoppingCartDc.CartTotalAmt = customerShoppingCart.CartTotalAmt;
                customerShoppingCartDc.CustomerId = customerShoppingCart.CustomerId;
                customerShoppingCartDc.DeamPoint = customerShoppingCart.DeamPoint;
                customerShoppingCartDc.DeliveryCharges = customerShoppingCart.DeliveryCharges;
                customerShoppingCartDc.GeneratedOrderId = customerShoppingCart.GeneratedOrderId;
                customerShoppingCartDc.GrossTotalAmt = customerShoppingCart.GrossTotalAmt;
                customerShoppingCartDc.TotalDiscountAmt = customerShoppingCart.TotalDiscountAmt;
                customerShoppingCartDc.TotalTaxAmount = customerShoppingCart.TotalTaxAmount;
                customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
                customerShoppingCartDc.WarehouseId = customerShoppingCart.WarehouseId;
                customerShoppingCartDc.TotalSavingAmt = customerShoppingCart.TotalSavingAmt;
                customerShoppingCartDc.TotalQty = customerShoppingCart.TotalQty;
                customerShoppingCartDc.WheelCount = customerShoppingCart.WheelCount;
                customerShoppingCartDc.NewBillingWalletPoint = customerShoppingCart.NewBillingWalletPoint;
                customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
                customerShoppingCartDc.ShoppingCartItemDcs = ShoppingCartItemDcs;
            }
            return customerShoppingCartDc;
        }

        [Route("DeleteItem")]
        [HttpPost]
        public async Task<ReturnShoppingCart> DeleteCartItem(CartItemDc cartItemDc)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == cartItemDc.CustomerId && x.WarehouseId == cartItemDc.WarehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (cartItemDc.PeopleId > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == cartItemDc.PeopleId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            using (var context = new AuthContext())
            {
                if (customerShoppingCart != null)
                {
                    customerShoppingCart.ModifiedDate = DateTime.Now;
                    customerShoppingCart.ModifiedBy = cartItemDc.CustomerId;
                    if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == cartItemDc.ItemId && x.IsFreeItem == cartItemDc.IsFreeItem))
                    {
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsActive = false;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).IsDeleted = true;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).ModifiedDate = DateTime.Now;
                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == cartItemDc.ItemId).ModifiedBy = cartItemDc.CustomerId;
                    }

                    returnShoppingCart.Status = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                }
                if (cartItemDc.IsCartRequire)
                {
                    customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, cartItemDc.lang);
                }
                else
                {
                    customerShoppingCartDc = null;
                    //BackgroundJob.Enqueue(() => RefereshCartSync(customerShoppingCart, cartItemDc.lang));
                }

            }
            returnShoppingCart.Cart = customerShoppingCartDc;
            return returnShoppingCart;
        }

        [Route("ClearCart")]
        [HttpGet]
        public async Task<ReturnShoppingCart> ClearCart(int customerId, int warehouseId, int peopleId = 0)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (peopleId > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == peopleId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            using (var context = new AuthContext())
            {
                if (customerShoppingCart != null)
                {
                    if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                    {
                        foreach (var item in customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                        {
                            item.IsActive = false;
                            item.IsDeleted = true;
                            item.ModifiedDate = DateTime.Now;
                            item.ModifiedBy = customerId;
                        }
                    }
                    var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                }

                customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, "en");
            }

            returnShoppingCart.Status = true;
            returnShoppingCart.Cart = customerShoppingCartDc;
            return returnShoppingCart;
        }

        [Route("ClearPeopleCart")]
        [HttpGet]
        public async Task<ReturnShoppingCart> ClearPeopleCart(int warehouseId, int peopleId)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.PeopleId == peopleId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

            var customerShoppingCarts = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").ToList();
            using (var context = new AuthContext())
            {
                if (customerShoppingCarts != null)
                {
                    foreach (var customerShoppingCart in customerShoppingCarts)
                    {
                        if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                        {
                            foreach (var item in customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)))
                            {
                                item.IsActive = false;
                                item.IsDeleted = true;
                                item.ModifiedDate = DateTime.Now;
                                item.ModifiedBy = peopleId;
                            }
                        }
                        customerShoppingCart.IsActive = false;
                        customerShoppingCart.IsDeleted = false;
                        var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                    }
                }

            }

            returnShoppingCart.Status = true;
            returnShoppingCart.Cart = null;
            return returnShoppingCart;
        }

        [Route("GetCustomerCart")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ReturnShoppingCart> GetCustomerCart(int customerId, int warehouseId, string lang, int peopleId = 0)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (peopleId > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == peopleId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            using (var context = new AuthContext())
            {
                customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, lang);
            }

            returnShoppingCart.Status = customerShoppingCart == null ? false : true;
            returnShoppingCart.Cart = customerShoppingCartDc;
            return returnShoppingCart;
        }

        [Route("ApplyNewOffer")]
        [HttpGet]
        public async Task<ReturnShoppingCart> ApplyOffer(int customerId, int warehouseId, int offerId, bool IsApply, string lang, int peopleId = 0)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (peopleId > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == peopleId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            using (var context = new AuthContext())
            {
                if (customerShoppingCart != null)
                {
                    if (IsApply)
                    {
                        if (customerShoppingCart.ShoppingCartDiscounts == null)
                        {
                            customerShoppingCart.ShoppingCartDiscounts = new List<ShoppingCartDiscount> {
                            new ShoppingCartDiscount {

                              OfferId =offerId,
                              CreatedBy=customerId,
                              CreatedDate=DateTime.Now,
                              IsActive=true,
                              IsDeleted=false
                            }};

                        }
                        else if (customerShoppingCart.ShoppingCartDiscounts.Any(x => x.OfferId == offerId))
                        {
                            List<int> ExistingOfferIds = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.OfferId != offerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList();
                            bool Isvalid = ExistingOfferIds.Any() ? await IsValidOffer(ExistingOfferIds, offerId) : true;
                            if (Isvalid)
                            {
                                customerShoppingCart.ShoppingCartDiscounts.FirstOrDefault(x => x.OfferId == offerId).IsActive = true;
                                customerShoppingCart.ShoppingCartDiscounts.FirstOrDefault(x => x.OfferId == offerId).IsDeleted = false;
                            }
                        }
                        else
                        {
                            List<int> ExistingOfferIds = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.OfferId != offerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList();
                            bool Isvalid = ExistingOfferIds.Any() ? await IsValidOffer(ExistingOfferIds, offerId) : true;
                            if (Isvalid)
                            {
                                customerShoppingCart.ShoppingCartDiscounts.Add(new ShoppingCartDiscount
                                {

                                    OfferId = offerId,
                                    CreatedBy = customerId,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    IsDeleted = false
                                });
                            }
                        }
                    }
                    else
                    {
                        if (customerShoppingCart.ShoppingCartDiscounts != null && customerShoppingCart.ShoppingCartDiscounts.Any(x => x.OfferId == offerId))
                        {
                            customerShoppingCart.ShoppingCartDiscounts.FirstOrDefault(x => x.OfferId == offerId).IsActive = false;
                            customerShoppingCart.ShoppingCartDiscounts.FirstOrDefault(x => x.OfferId == offerId).IsDeleted = true;
                        }
                    }
                    returnShoppingCart.Status = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");

                    customerShoppingCartDc = await RefereshCartWithOutAutoApplyDis(customerShoppingCart, context, lang, peopleId);

                    if (IsApply && customerShoppingCartDc != null && !customerShoppingCartDc.ApplyOfferId.Contains(offerId.ToString()))
                    {
                        string offermsg = "OfferNotEligible";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आप इस ऑफर के लिए एलिजिबल नहीं हो!" : "You are not eligible for this offer!";
                        if (customerShoppingCartDc.DiscountDetails != null && customerShoppingCartDc.DiscountDetails.Any(x => !customerShoppingCartDc.ApplyOfferId.Contains(offerId.ToString())))
                            offermsg = "OfferNotEligibleUserWithOther";
                        returnShoppingCart.Status = false;
                        returnShoppingCart.Message = customerShoppingCartDc.NotEligiblePrimeOffer ? "MemberOfferNotEligible" : offermsg;//"You are not eligible for " + MemberShipName + " offer." : offermsg;

                        if (lang != "en" && EnableOtherLanguage)
                        {
                            LanguageConvertHelper languageConvertHelper = new LanguageConvertHelper();
                            List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> msgElasticLanguageDatas = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                            msgElasticLanguageDatas.Add(new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = SCMsgEnum.SCEnum[returnShoppingCart.Message].Value, IsTranslate = true });
                            //if (languageConvertHelper.CheckConvertLanguageData(msgElasticLanguageDatas.Distinct().ToList(), lang.ToLower()))
                            //{
                            var ElasticLanguageDatas = languageConvertHelper.GetConvertLanguageData(msgElasticLanguageDatas.Distinct().ToList(), lang.ToLower());
                            returnShoppingCart.Message = ElasticLanguageDatas.Any(y => y.englishtext == SCMsgEnum.SCEnum[returnShoppingCart.Message].Value) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == SCMsgEnum.SCEnum[returnShoppingCart.Message].Value).converttext : SCMsgEnum.SCEnum[returnShoppingCart.Message].Value;
                            //}
                        }
                        else
                        {
                            returnShoppingCart.Message = SCMsgEnum.SCEnum[returnShoppingCart.Message].Key;
                        }
                    }
                    returnShoppingCart.Cart = customerShoppingCartDc;
                }


            }
            return returnShoppingCart;

        }

        [Route("UpdatePeopleCustomer")]
        [HttpGet]
        public async Task<ReturnShoppingCart> UpdatePeopleCustomer(int peopleId, int customerId, int newCustomerId, string lang)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (peopleId > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == peopleId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
            if (customerShoppingCart != null)
            {
                customerShoppingCart.CustomerId = newCustomerId;
                if (customerShoppingCart.ShoppingCartDiscounts != null && customerShoppingCart.ShoppingCartDiscounts.Any())
                    customerShoppingCart.ShoppingCartDiscounts.ForEach(x => x.IsActive = false);
                var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                //using (var context = new AuthContext())
                //{
                //    customerShoppingCartDc = await RefereshCart(customerShoppingCart, context, lang);
                //}
            }
            returnShoppingCart.Status = customerShoppingCart == null ? false : true;
            returnShoppingCart.Cart = customerShoppingCartDc;
            return returnShoppingCart;
        }

        [Route("UpdatePeopleFirstCall")]
        [HttpGet]
        public async Task<bool> UpdatePeopleCartFirstCall(int peopleId, int customerId)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
            if (peopleId > 0)
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == peopleId);
            }
            else
            {
                cartPredicate = cartPredicate.And(x => x.PeopleId == 0);
            }
            var customerMultiCart = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart").ToList();
            if (customerMultiCart != null && customerMultiCart.Any())
            {
                var customerShoppingCart = customerMultiCart.OrderByDescending(x => x.ModifiedDate).FirstOrDefault();
                if (customerMultiCart.Count() > 1)
                {
                    foreach (var item in customerMultiCart)
                    {
                        if (item.Id != customerShoppingCart.Id)
                        {
                            item.IsActive = false;
                            item.ModifiedDate = DateTime.Now;
                            await mongoDbHelper.ReplaceWithoutFindAsync(item.Id, item, "CustomerShoppingCart");
                        }
                    }
                }

                if (customerShoppingCart != null)
                {
                    customerShoppingCart.CustomerId = customerId;
                    if (customerShoppingCart.ShoppingCartDiscounts != null && customerShoppingCart.ShoppingCartDiscounts.Any())
                        customerShoppingCart.ShoppingCartDiscounts.ForEach(x => x.IsActive = false);
                    var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                }
            }
            return true;
        }
        private async Task<CustomerShoppingCartDc> RefereshCartWithOutAutoApplyDis(CustomerShoppingCart customerShoppingCart, AuthContext context, string lang, int peopleId)
        {
            string ConsumerApptype = new OrderPlaceHelper().GetCustomerAppType();
            List<ShoppingCartItemDc> ShoppingCartItemDcs = new List<ShoppingCartItemDc>();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = new List<DataContracts.Masters.BillDiscountOfferDc>();
            int walletPoint = 0;
            customerShoppingCartDc = new CustomerShoppingCartDc
            {
                CartTotalAmt = 0,
                CustomerId = customerShoppingCart.CustomerId,
                DeamPoint = 0,
                DeliveryCharges = 0,
                GrossTotalAmt = 0,
                TotalDiscountAmt = 0,
                TotalTaxAmount = 0,
                WalletAmount = 0,
                WarehouseId = customerShoppingCart.WarehouseId,
            };
            if (customerShoppingCart.ShoppingCartItems != null && customerShoppingCart.ShoppingCartItems.Any())
            {
                var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == customerShoppingCart.WarehouseId && x.isDeleted == false && x.IsActive && !x.IsDistributor).ToList();
                var ActiveCustomer = context.Customers.Where(x => x.CustomerId == customerShoppingCart.CustomerId).Include(x => x.ConsumerAddress).FirstOrDefault();
                if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                {
                    ActiveCustomer.Active = ActiveCustomer.IsB2CApp;
                    var defaultadd = ActiveCustomer.ConsumerAddress.FirstOrDefault(x => x.Default);
                    var cluster = context.Clusters.FirstOrDefault(x => x.WarehouseId == defaultadd.WarehouseId);
                    ActiveCustomer.ShippingAddress = defaultadd.CompleteAddress;
                    ActiveCustomer.Warehouseid = defaultadd.WarehouseId;
                    ActiveCustomer.City = defaultadd.CityName;
                    ActiveCustomer.Cityid = defaultadd.Cityid;
                    ActiveCustomer.State = defaultadd.StateName;
                    ActiveCustomer.ClusterId = cluster.ClusterId;
                    ActiveCustomer.ZipCode = defaultadd.ZipCode;
                    ActiveCustomer.lat = defaultadd.lat;
                    ActiveCustomer.lg = defaultadd.lng;
                    ActiveCustomer.LandMark = defaultadd.LandMark;
                    ActiveCustomer.ShopName = defaultadd.PersonName;
                    ActiveCustomer.Name = defaultadd.PersonName;
                    ActiveCustomer.CustomerType = ConsumerApptype;
                }
                var inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                List<int> itemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();
                itemids = context.itemMasters.Where(x => itemids.Contains(x.ItemId) && x.WarehouseId == ActiveCustomer.Warehouseid).Select(x => x.ItemId).ToList();
                string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + customerShoppingCart.WarehouseId +
                                  " WHERE a.[CustomerId]=" + customerShoppingCart.CustomerId;
                var FlashDealWithItemIds = context.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();

                string itemIndex = ConfigurationManager.AppSettings["ElasticSearchIndexName"];
                string query = $"SELECT itemid,active,basecategoryid,basecategoryname,categoryid,categoryname,subcategoryid,subcategoryname,subsubcategoryid" +
                                $", subsubcategoryname, itemlimitqty, isitemlimit, itemnumber, companyid, warehouseid, itemname, price, unitprice, logourl" +
                                $", minorderqty, totaltaxpercentage, marginpoint, promoperitems, dreampoint, isoffer, unitofquantity, uom, itembasename" +
                                $", deleted, isflashdealused, itemmultimrpid, netpurchaseprice, billlimitqty, issensitive, issensitivemrp, hindiname" +
                                $", offercategory, offerstarttime, offerendtime, offerqtyavaiable, offerqtyconsumed, offerid, offertype, offerwalletpoint" +
                                $", offerfreeitemid, freeitemid, offerpercentage, offerfreeitemname, offerfreeitemimage, offerfreeitemquantity" +
                                $", offerminimumqty, flashdealspecialprice, flashdealmaxqtypersoncantake, distributionprice, distributorshow, itemapptype" +
                                $", mrp, isprimeitem, primeprice, noprimeofferstarttime, currentstarttime, isflashdealstart, scheme, warehousename" +
                                $", cityid, cityname, isdiscontinued from {itemIndex} where " +
                        $"  active=true and deleted=false and  isdiscontinued=false and (isitemlimit=false or (isitemlimit=true and itemlimitqty>0 and itemlimitqty-minorderqty>0 )) and (itemapptype=0 or itemapptype=1)";

                if (itemids != null && itemids.Any())
                {
                    query += $" and itemid in ({string.Join(",", itemids)})";
                }


                /*   ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemResponse> elasticSqlHelper1 = new ElasticSqlHelper<DataContracts.ElasticSearch.ElasticItemResponse>();

                   var dbItems = AsyncContext.Run(async () => (await elasticSqlHelper1.GetListAsync(query)).ToList());

                   var newdata = dbItems.Select(x => new factoryItemdata
                   {
                       active = x.active,
                       BaseCategoryId = x.basecategoryid,
                       BillLimitQty = x.billlimitqty,
                       Categoryid = x.categoryid,
                       CompanyId = x.companyid,
                       //  CurrentStartTime = x.currentstarttime,
                       Deleted = x.deleted,
                       DistributionPrice = x.distributionprice,
                       DistributorShow = x.distributorshow,
                       marginPoint = x.marginpoint,
                       dreamPoint = x.dreampoint,
                       FlashDealMaxQtyPersonCanTake = x.flashdealmaxqtypersoncantake,
                       FlashDealSpecialPrice = x.flashdealspecialprice,
                       FreeItemId = x.freeitemid,
                       HindiName = x.hindiname,
                       IsFlashDealUsed = x.isflashdealused,
                       IsItemLimit = x.isitemlimit,
                       IsOffer = x.isoffer,
                       //IsPrimeItem = x.isprimeitem,
                       IsSensitive = x.issensitive,
                       IsSensitiveMRP = x.issensitivemrp,
                       ItemAppType = x.itemapptype,
                       itemBaseName = x.itembasename,
                       ItemId = x.itemid,
                       ItemlimitQty = x.itemlimitqty,
                       ItemMultiMRPId = x.itemmultimrpid,
                       itemname = x.itemname,
                       ItemNumber = x.itemnumber,
                       LogoUrl = x.logourl,
                       MinOrderQty = x.minorderqty,
                       NetPurchasePrice = x.netpurchaseprice,
                       OfferCategory = x.offercategory,
                       OfferEndTime = x.offerendtime,
                       OfferFreeItemId = x.offerfreeitemid,
                       OfferFreeItemImage = x.offerfreeitemimage,
                       OfferFreeItemName = x.offerfreeitemname,
                       OfferFreeItemQuantity = x.offerfreeitemquantity,
                       OfferId = x.offerid,
                       OfferMinimumQty = x.offerminimumqty,
                       OfferPercentage = x.offerpercentage,
                       OfferQtyAvaiable = x.offerqtyavaiable,
                       OfferQtyConsumed = x.offerqtyconsumed,
                       OfferStartTime = x.offerstarttime,
                       OfferType = x.offertype,
                       OfferWalletPoint = x.offerwalletpoint,
                       price = x.price,
                       //PrimePrice = x.primeprice,
                       //promoPerItems = x.promoperitems,
                       //Scheme = x.scheme,
                       UnitofQuantity = x.unitofquantity,
                       UOM = x.uom,
                       SubCategoryId = x.subcategoryid,
                       SubsubCategoryid = x.subsubcategoryid,
                       WarehouseId = x.warehouseid,
                       UnitPrice = x.unitprice,
                       TotalTaxPercentage = x.totaltaxpercentage
                   }).ToList();
                */


                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var orderIdDt = new DataTable();
                orderIdDt.Columns.Add("IntValue");
                foreach (var item in itemids)
                {
                    var dr = orderIdDt.NewRow();
                    dr["IntValue"] = item;
                    orderIdDt.Rows.Add(dr);
                }
                var param = new SqlParameter("ItemIds", orderIdDt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetShoppingCardItem]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);

                // Run the sproc
                var reader = cmd.ExecuteReader();
                var newdata = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<factoryItemdata>(reader).ToList();

                string apptype = customerShoppingCart.PeopleId > 0 ? "Sales App" : "Retailer App";
                if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                    apptype = ConsumerApptype;

                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                List<OrderOfferDc> activeOfferids = new List<OrderOfferDc>();
                if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                    activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == apptype)).Select(x => new OrderOfferDc { OfferId = x.OfferId, QtyAvaiable = x.QtyAvaiable, IsDispatchedFreeStock = x.IsDispatchedFreeStock, MinOrderQuantity = x.MinOrderQuantity, NoOffreeQuantity = x.NoOffreeQuantity }).ToList() : null;
                else
                    activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == apptype || x.OfferAppType == "Both")).Select(x => new OrderOfferDc { OfferId = x.OfferId, QtyAvaiable = x.QtyAvaiable, IsDispatchedFreeStock = x.IsDispatchedFreeStock, MinOrderQuantity = x.MinOrderQuantity, NoOffreeQuantity = x.NoOffreeQuantity }).ToList() : null;

                List<int> Primeitemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsPrimeItem.HasValue && x.IsPrimeItem.Value && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();

                var itemMultiMRPIds = newdata.Where(x => Primeitemids.Contains(x.ItemId)).Select(x => x.ItemMultiMRPId).Distinct().ToList();
                var PrimeItems = itemMultiMRPIds.Any() ? context.PrimeItemDetails.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.CityId == ActiveCustomer.Cityid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList() : null;
                var primeCustomer = context.PrimeCustomers.FirstOrDefault(x => x.CustomerId == ActiveCustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));

                ActiveCustomer.IsPrimeCustomer = primeCustomer != null && primeCustomer.StartDate <= DateTime.Now && primeCustomer.EndDate >= DateTime.Now;
                var dealItems = context.DealItems.Where(x => itemMultiMRPIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == ActiveCustomer.Warehouseid && (x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))).ToList();


                DateTime CurrentDate = !ActiveCustomer.IsPrimeCustomer ? DateTime.Now.AddHours(MemberShipHours) : DateTime.Now;
                int hrs = !ActiveCustomer.IsPrimeCustomer ? MemberShipHours : 0;
                BackendOrderController backendOrderController = new BackendOrderController();
                foreach (var it in newdata)
                {
                    double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ActiveCustomer.Warehouseid));
                    it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice
                                                                   , it.WholeSalePrice ?? 0
                                                                   , it.TradePrice ?? 0, cprice);
                    if (PrimeItems != null && PrimeItems.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty))
                    {
                        var primeItem = PrimeItems.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.MinOrderQty == it.MinOrderQty);
                        it.IsPrimeItem = true;
                        it.PrimePrice = primeItem.PrimePercent > 0 ? Convert.ToDecimal(it.UnitPrice - (it.UnitPrice * Convert.ToDouble(primeItem.PrimePercent) / 100)) : primeItem.PrimePrice;
                    }
                    //Condition for offer end.
                    if (!inActiveCustomer)
                    {
                        if (!(it.OfferStartTime.HasValue && it.OfferStartTime.Value.AddHours(hrs) <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
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
                        else if (it.OfferStartTime.HasValue && (it.OfferStartTime.Value.AddHours(hrs) <= DateTime.Now && it.OfferEndTime >= DateTime.Now) && it.OfferCategory == 2)
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
                            if (activeOfferids != null && activeOfferids.Any() && activeOfferids.Any(x => x.OfferId == it.OfferId) && it.IsOffer)
                                it.IsOffer = true;
                            else
                                it.IsOffer = false;
                        }

                        if (customerShoppingCart.PeopleId > 0 && it.OfferType == "FlashDeal")
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                    }
                    else
                    {
                        it.IsOffer = false;
                        it.FlashDealSpecialPrice = 0;
                        it.OfferCategory = 0;
                    }

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
                        if (!string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer")
                            it.marginPoint = it.UnitPrice > 0 ? (((it.price - it.UnitPrice) * 100) / it.price) : 0;
                        else
                            it.marginPoint = it.UnitPrice > 0 ? (((it.price - it.UnitPrice) * 100) / it.UnitPrice) : 0;//MP;  we replce marginpoint value by margin for app here 

                    }
                    else
                    {
                        it.marginPoint = 0;
                    }

                    if (it.IsOffer && it.OfferCategory.HasValue && it.OfferCategory.Value == 1)
                    {
                        if (it.OfferType == "WalletPoint" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferWalletPoint.HasValue && it.OfferWalletPoint.Value > 0)
                        {
                            var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                            if (item.qty >= it.OfferMinimumQty)
                            {
                                var FreeWalletPoint = it.OfferWalletPoint.Value;
                                int calfreeItemQty = item.qty / it.OfferMinimumQty.Value;
                                FreeWalletPoint *= calfreeItemQty;
                                item.TotalFreeWalletPoint = FreeWalletPoint;
                                walletPoint += Convert.ToInt32(FreeWalletPoint);
                            }

                        }
                        else if (it.OfferType == "ItemMaster" && it.OfferMinimumQty.HasValue && it.OfferMinimumQty.Value != 0 && it.OfferFreeItemQuantity.HasValue && it.OfferFreeItemQuantity.Value > 0)
                        {
                            var item = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId && !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                            if (item.qty >= it.OfferMinimumQty)
                            {
                                var cartqty = it.IsItemLimit && item.qty > it.ItemlimitQty ? it.ItemlimitQty : item.qty;
                                var FreeItemQuantity = it.OfferFreeItemQuantity.Value;
                                int calfreeItemQty = Convert.ToInt32(cartqty / it.OfferMinimumQty);
                                FreeItemQuantity *= calfreeItemQty;
                                if (FreeItemQuantity > 0)
                                {
                                    item.FreeItemqty = FreeItemQuantity;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(lang) && lang.Trim() == "hi")
                    {
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
                    }
                }
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                CustomersManager manager = new CustomersManager();
                List<int> offerIds = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId.Value).ToList();
                billDiscountOfferDcs = offerIds.Any() ? manager.GetApplyBillDiscountById(offerIds, customerShoppingCart.CustomerId) : new List<DataContracts.Masters.BillDiscountOfferDc>();
                if (billDiscountOfferDcs.Any())
                {
                    if (peopleId > 0)
                        billDiscountOfferDcs = billDiscountOfferDcs.Where(x => x.BillDiscountType != "ClearanceStock").ToList();

                    foreach (var Offer in billDiscountOfferDcs.OrderByDescending(x => x.IsUseOtherOffer))
                    {

                        bool isEligable = true;
                        if (Offer.ApplyType == "PrimeCustomer" && !ActiveCustomer.IsPrimeCustomer)
                        {
                            customerShoppingCartDc.NotEligiblePrimeOffer = true;
                            isEligable = false;
                        }

                        if (isEligable)
                        {
                            ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                            double totalamount = 0;
                            var OrderLineItems = 0;
                            //if (Offer.OfferOn != "ScratchBillDiscount")
                            //{
                            List<int> Itemids = new List<int>();
                            if (Offer.BillDiscountType == "category" && Offer.OfferBillDiscountItems.Any())
                            {
                                var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                var ids = Offer.OfferBillDiscountItems.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                var notids = Offer.OfferBillDiscountItems.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                {
                                    var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x => (!ids.Any() || ids.Contains(x.Categoryid)) && !notids.Contains(x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                            else if (Offer.BillDiscountType == "subcategory" && Offer.OfferBillDiscountItems.Any())
                            {
                                var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                {
                                    var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x =>
                                 (!Offer.OfferBillDiscountItems.Where(y => y.IsInclude).Any() || Offer.OfferBillDiscountItems.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                                 && !Offer.OfferBillDiscountItems.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                            else if (Offer.BillDiscountType == "brand" && Offer.OfferBillDiscountItems.Any())
                            {
                                var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                {
                                    var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x =>
                                (
                                 !Offer.OfferBillDiscountItems.Where(y => y.IsInclude).Any() ||
                                Offer.OfferBillDiscountItems.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                )
                                && !Offer.OfferBillDiscountItems.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                            else if (Offer.BillDiscountType == "items" && Offer.IsBillDiscountFreebiesItem)
                            {
                                var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                var itemnumbermrps = context.itemMasters.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => new DataContracts.BillDiscount.offerItemMRP { ItemNumber = x.Number, ItemMultiMRPId = x.ItemMultiMRPId }).ToList();

                                var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                {
                                    var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x => (!iteminofferlist.Any() || itemnumbermrps.Select(y => y.ItemNumber + "" + y.ItemMultiMRPId).Contains(x.ItemNumber + "" + x.ItemMultiMRPId))
                                               && !itemoutofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                                var itemoutofferlist = Offer.OfferItems.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                var iteminofferlist = Offer.OfferItems.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                                //if (Offer.OfferItems.FirstOrDefault().IsInclude)
                                //{
                                //    Itemids = newdata.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                //}
                                var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                {
                                    var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x => (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !itemoutofferlist.Contains(x.ItemId)
                                ).Select(x => x.ItemId).ToList();
                                if (CItemIds.Any())
                                {
                                    Itemids = newdata.Where(x => CItemIds.Contains(x.ItemId) && Itemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : null;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
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
                                var catIdoutofferlist = Offer.OfferBillDiscountItems.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                var catIdinofferlist = Offer.OfferBillDiscountItems.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                // var ids = Offer.OfferBillDiscountItems.Select(x => x.Id).ToList();
                                var CItemIds = newdata.Select(x => x.ItemId).ToList();
                                if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                                {
                                    var classifications = Offer.IncentiveClassification.Split(',').ToList();
                                    CItemIds = newdata.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                                }
                                Itemids = newdata.Where(x => (catIdinofferlist.Any() || catIdinofferlist.Contains(x.Categoryid)) &&
                                !catIdoutofferlist.Contains(x.Categoryid)
                                ).Select(x => x.ItemId).ToList();
                                var incluseItemIds = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                if (catIdoutofferlist.Any())
                                    incluseItemIds = newdata.Where(x => !catIdoutofferlist.Contains(x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                if (CItemIds.Any())
                                {
                                    incluseItemIds = newdata.Where(x => CItemIds.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)
                                  ).Select(x => x.ItemId).ToList();
                                }
                                totalamount = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                OrderLineItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId)).Count() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                                var cartItems = Itemids.Any() && CItemIds.Any() ? customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).ToList() : customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && x.qty > 0 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && incluseItemIds.Contains(x.ItemId)).ToList();
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemId) && (x.qty * x.UnitPrice) >= item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemId).FirstOrDefault();
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


                            if (Offer.BillDiscountRequiredItems != null && Offer.BillDiscountRequiredItems.Any())
                            {
                                bool IsRequiredItemExists = true;
                                var objectIds = Offer.BillDiscountRequiredItems.Where(x => x.ObjectType == "Item").SelectMany(x => x.ObjectId.Split(',').Select(y => Convert.ToInt32(y))).Distinct().ToList();
                                if (Offer.BillDiscountRequiredItems.Any(x => x.ObjectType == "brand"))
                                {
                                    objectIds.AddRange(newdata.Where(x => Offer.BillDiscountRequiredItems.Where(y => y.ObjectType == "brand").SelectMany(y => y.ObjectId.Split(',').Select(z => z)).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList());
                                }
                                var cartrequiredItems = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && objectIds.Contains(x.ItemMultiMRPId) && x.qty > 0 && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                                foreach (var reqitem in Offer.BillDiscountRequiredItems)
                                {
                                    if (reqitem.ObjectType == "Item")
                                    {
                                        var reqobjectids = reqitem.ObjectId.Split(',').Select(z => Convert.ToInt32(z)).ToList();
                                        var cartitem = cartrequiredItems.Where(x => reqobjectids.Contains(x.ItemMultiMRPId));
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
                                        var reqobjectids = reqitem.ObjectId.Split(',').Select(z => z).ToList();
                                        var multiMrpIds = newdata.Where(x => reqobjectids.Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemMultiMRPId).ToList();
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


                            if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                            {
                                totalamount = Offer.MaxBillAmount;
                            }
                            else if (Offer.BillAmount > totalamount)
                            {
                                totalamount = 0;
                            }

                            if (Offer.LineItem > 0 && Offer.LineItem > OrderLineItems)
                            {
                                totalamount = 0;
                            }
                            if (Offer.OfferOn == "ScratchBillDiscount")
                            {
                                var billdiscount = context.BillDiscountDb.FirstOrDefault(x => x.OfferId == Offer.OfferId && x.CustomerId == customerShoppingCart.CustomerId && x.OrderId == 0);
                                Offer.IsScratchBDCode = false;
                                if (billdiscount != null)
                                    Offer.IsScratchBDCode = billdiscount.OrderId == 0 ? billdiscount.IsScratchBDCode : false;

                                if (!Offer.IsScratchBDCode)
                                    totalamount = 0;

                                //if (billdiscount!=null &&( Offer.BillDiscountOfferOn == "DynamicWalletPoint" || Offer.BillDiscountOfferOn == "DynamicAmount"))
                                //{
                                //    Offer.BillDiscountWallet = billdiscount.BillDiscountTypeValue;
                                //}






                            }
                            //}
                            //    else
                            //    {
                            //        var Itemids = newdata.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                            //        totalamount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.qty > 0 && x.IsActive && Itemids.Contains(x.ItemId) && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice);
                            //        var billdiscount = context.BillDiscountDb.FirstOrDefault(x => x.OfferId == Offer.OfferId && x.CustomerId == customerShoppingCart.CustomerId);
                            //        Offer.IsScratchBDCode = false;
                            //        if (billdiscount != null)
                            //            Offer.IsScratchBDCode = billdiscount.OrderId == 0 ? billdiscount.IsScratchBDCode : false;

                            //        if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                            //        {
                            //            totalamount = Offer.MaxBillAmount;
                            //        }
                            //        else if (Offer.BillAmount > totalamount)
                            //        {
                            //            totalamount = 0;
                            //        }

                            //        if (Offer.BillDiscountOfferOn == "DynamicWalletPoint")
                            //        {
                            //            Offer.BillDiscountWallet = billdiscount.BillDiscountTypeValue;
                            //        }

                            //        if (!Offer.IsScratchBDCode)
                            //            totalamount = 0;
                            //    }


                            bool IsUsed = true;
                            if (!Offer.IsUseOtherOffer && ShoppingCartDiscounts.Any())
                                IsUsed = false;



                            if (IsUsed && totalamount > 0)
                            {
                                if (Offer.BillDiscountOfferOn == "Percentage")
                                {
                                    ShoppingCartDiscount.DiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                }
                                else if (Offer.BillDiscountOfferOn == "FreeItem")
                                {
                                    ShoppingCartDiscount.DiscountAmount = 0;
                                }
                                else if (Offer.BillDiscountOfferOn == "DynamicAmount")
                                {
                                    ShoppingCartDiscount.DiscountAmount = Offer.BillDiscountWallet.Value;
                                }
                                else if (Offer.BillDiscountOfferOn == "DynamicWalletPoint")
                                {
                                    ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(Offer.BillDiscountWallet / 10);
                                }
                                else
                                {
                                    int WalletPoint = 0;
                                    if (Offer.WalletType == "WalletPercentage")
                                    {
                                        WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * (Offer.BillDiscountWallet ?? 0) / 100));
                                        WalletPoint = WalletPoint * 10;
                                    }
                                    else
                                    {
                                        WalletPoint = Convert.ToInt32(Offer.BillDiscountWallet ?? 0);
                                    }
                                    if (Offer.ApplyOn == "PostOffer")
                                    {
                                        ShoppingCartDiscount.DiscountAmount = 0;
                                        ShoppingCartDiscount.NewBillingWalletPoint = WalletPoint;
                                    }
                                    else
                                    {
                                        if (ActiveCustomer.CustomerType.ToLower() == "consumer")
                                        {
                                            var Conversion = context.CashConversionDb.FirstOrDefault(x => x.IsConsumer == true);
                                            if (Conversion != null)
                                            {
                                                ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / Conversion.point); ;
                                            }
                                        }
                                        else
                                        {
                                            var Conversion = context.CashConversionDb.FirstOrDefault(x => x.IsConsumer == false);
                                            if (Conversion != null)
                                            {
                                                ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / Conversion.point); ;
                                            }
                                        }
                                        ShoppingCartDiscount.NewBillingWalletPoint = 0;
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
                                        if (Offer.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                        {
                                            ShoppingCartDiscount.DiscountAmount = Offer.MaxDiscount;
                                        }
                                        if (Offer.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint / 10)
                                        {
                                            ShoppingCartDiscount.NewBillingWalletPoint = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                                        }
                                    }
                                }

                                ShoppingCartDiscount.OfferId = Offer.OfferId;
                                ShoppingCartDiscount.CreatedDate = DateTime.Now;
                                ShoppingCartDiscount.IsActive = Offer.IsActive;
                                ShoppingCartDiscount.IsDeleted = false;
                                ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                            }
                        }
                    }
                }
                customerShoppingCart.ShoppingCartDiscounts = ShoppingCartDiscounts;

                customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList().ForEach(x =>
                {
                    if (newdata.Any(y => y.ItemId == x.ItemId))
                    {
                        var item = newdata.FirstOrDefault(y => y.ItemId == x.ItemId);
                        x.ItemMultiMRPId = item.ItemMultiMRPId;
                        x.ItemNumber = item.ItemNumber;
                        x.ItemName = item.itemname;
                        if (newdata.Any(y => y.ItemId == x.ItemId && y.OfferCategory == 2 && y.IsOffer))
                        {
                            x.UnitPrice = x.UnitPrice;
                        }
                        else if (x.IsDealItem.HasValue && x.IsDealItem.Value && dealItems.Any(y => y.ItemMultiMRPId == item.ItemMultiMRPId && y.MinOrderQty == item.MinOrderQty))
                        {
                            x.UnitPrice = dealItems.FirstOrDefault(y => y.ItemMultiMRPId == item.ItemMultiMRPId && y.MinOrderQty == item.MinOrderQty).DealPrice;
                        }
                        else
                        {
                            x.UnitPrice = x.IsPrimeItem.HasValue && x.IsPrimeItem.Value && newdata.FirstOrDefault(y => y.ItemId == x.ItemId).IsPrimeItem ? Convert.ToDouble(newdata.FirstOrDefault(y => y.ItemId == x.ItemId).PrimePrice) : x.UnitPrice;
                        }
                    }
                });
                ShoppingCartItemDcs = newdata.Select(a => new ShoppingCartItemDc
                {
                    BaseCategoryId = a.BaseCategoryId,
                    IsItemLimit = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? true : a.IsItemLimit,
                    ItemlimitQty = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) && dealItems.Any(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty) ? (dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).OrderLimit < dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalLimit - dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalConsume ? dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).OrderLimit : dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalLimit - dealItems.FirstOrDefault(x => x.ItemMultiMRPId == a.ItemMultiMRPId && x.MinOrderQty == a.MinOrderQty).TotalConsume) : a.ItemlimitQty,
                    BillLimitQty = a.BillLimitQty,
                    WarehouseId = a.WarehouseId,
                    CompanyId = a.CompanyId,
                    Categoryid = a.Categoryid,
                    Discount = a.Discount,
                    ItemId = a.ItemId,
                    IsPrimeItem = a.IsPrimeItem,
                    PrimePrice = Convert.ToDouble(a.PrimePrice),
                    ItemNumber = a.ItemNumber,
                    HindiName = a.HindiName,
                    IsSensitive = a.IsSensitive,
                    IsSensitiveMRP = a.IsSensitiveMRP,
                    UnitofQuantity = a.UnitofQuantity,
                    UOM = a.UOM,
                    itemname = a.itemname,
                    LogoUrl = a.LogoUrl,
                    MinOrderQty = !string.IsNullOrEmpty(ConsumerApptype) && ConsumerApptype.ToLower() == "consumer" ? 1 : a.MinOrderQty,
                    price = a.price,
                    SubCategoryId = a.SubCategoryId,
                    SubsubCategoryid = a.SubsubCategoryid,
                    TotalTaxPercentage = a.TotalTaxPercentage,
                    SellingUnitName = a.SellingUnitName,
                    SellingSku = a.SellingSku,
                    UnitPrice = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value).UnitPrice : a.UnitPrice,
                    VATTax = a.VATTax,
                    itemBaseName = a.itemBaseName,
                    active = a.active,
                    marginPoint = a.marginPoint.HasValue ? a.marginPoint.Value : 0,
                    promoPerItems = a.promoPerItems.HasValue ? a.promoPerItems.Value : 0,
                    NetPurchasePrice = a.NetPurchasePrice,
                    IsOffer = a.IsOffer,
                    Deleted = a.Deleted,
                    OfferCategory = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? 3 : (a.OfferCategory.HasValue ? a.OfferCategory.Value : 0),
                    OfferStartTime = a.OfferStartTime,
                    OfferEndTime = a.OfferEndTime,
                    OfferQtyAvaiable = a.OfferQtyAvaiable.HasValue ? a.OfferQtyAvaiable.Value : 0,
                    OfferQtyConsumed = a.OfferQtyConsumed.HasValue ? a.OfferQtyConsumed.Value : 0,
                    OfferId = a.OfferId.HasValue ? a.OfferId.Value : 0,
                    OfferType = a.OfferType,
                    dreamPoint = a.dreamPoint.HasValue ? a.dreamPoint.Value : 0,
                    OfferWalletPoint = a.OfferWalletPoint.HasValue ? a.OfferWalletPoint.Value : 0,
                    OfferFreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                    OfferPercentage = a.OfferPercentage.HasValue ? a.OfferPercentage.Value : 0,
                    OfferFreeItemName = a.OfferFreeItemName,
                    OfferFreeItemImage = a.OfferFreeItemImage,
                    OfferFreeItemQuantity = a.OfferFreeItemQuantity.HasValue ? a.OfferFreeItemQuantity.Value : 0,
                    OfferMinimumQty = a.OfferMinimumQty.HasValue ? a.OfferMinimumQty.Value : 0,
                    FlashDealSpecialPrice = a.FlashDealSpecialPrice.HasValue ? a.FlashDealSpecialPrice.Value : 0,
                    FlashDealMaxQtyPersonCanTake = a.FlashDealMaxQtyPersonCanTake.HasValue ? a.FlashDealMaxQtyPersonCanTake.Value : 0,
                    FreeItemId = a.OfferFreeItemId.HasValue ? a.OfferFreeItemId.Value : 0,
                    IsDealItem = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).IsDealItem,
                    qty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).qty,
                    CartUnitPrice = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).UnitPrice,
                    TotalFreeItemQty = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).FreeItemqty,
                    TotalFreeWalletPoint = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == a.ItemId).TotalFreeWalletPoint,
                }).ToList();
                List<StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();

                foreach (var item in ShoppingCartItemDcs)
                {
                    if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid))
                    {
                        var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid);
                        item.StoreId = store.StoreId;
                        item.StoreName = store.StoreName;
                        item.StoreLogo = string.IsNullOrEmpty(store.StoreLogo) ? "" : store.StoreLogo;
                    }
                    else
                    {
                        item.StoreId = 0;
                        item.StoreName = "Other";
                        item.StoreLogo = "";
                    }
                    item.IsSuccess = true;
                    bool valid = true;
                    if (!item.active || item.Deleted)
                    {
                        valid = false;
                        item.IsSuccess = false;
                        item.Message = "ItemNotActive"; // !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम एक्टिव नहीं है" : "Item is not Active.";
                    }

                    if (valid && !(item.ItemAppType == 1 || item.ItemAppType == 0))
                    {
                        valid = false;
                        item.IsSuccess = false;
                        item.Message = "ItemNotActive";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम एक्टिव नहीं है " : "Item is not Active";
                    }

                    var mod = Convert.ToDecimal(item.qty) % item.MinOrderQty;
                    if (mod != 0)
                    {
                        valid = false;
                        item.IsSuccess = false;
                        item.Message = "multiplesMinOrderQty";// !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की क्वांटिटी मल्टीप्ल नहीं है मिनिमम आर्डर क्वांटिटी के " : "Item qty is less then min order qty.";
                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                        {
                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                        }
                    }

                    if (valid)
                    {
                        if (item.IsOffer && item.OfferType == "FlashDeal")
                        {
                            if (item.FlashDealSpecialPrice.HasValue && item.CartUnitPrice != item.FlashDealSpecialPrice.Value)
                            {
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "UnitPriceChange";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की यूनिट प्राइस चेंज हो गई है" : "Item Unit Price has changed.";
                                item.NewUnitPrice = item.FlashDealSpecialPrice.Value;
                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                {
                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.FlashDealSpecialPrice.Value;
                                }
                            }
                        }
                        else
                        {
                            if (ActiveCustomer.IsPrimeCustomer && item.IsPrimeItem)
                            {
                                if (item.PrimePrice != item.CartUnitPrice)
                                {
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.Message = "MemberUnitPriceChange"; //"Item " + MemberShipName + " Price has changed.";
                                    item.NewUnitPrice = item.PrimePrice;
                                    if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                    {
                                        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.PrimePrice;
                                    }
                                }
                            }
                            else if (item.IsDealItem.HasValue && item.IsDealItem.Value)
                            {
                                if (dealItems.Any(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty))
                                {
                                    var dealitem = dealItems.FirstOrDefault(x => x.ItemMultiMRPId == item.ItemMultiMRPId && x.MinOrderQty == item.MinOrderQty);
                                    if (dealitem.DealPrice != item.CartUnitPrice)
                                    {
                                        valid = false;
                                        item.IsSuccess = false;
                                        item.Message = "UnitPriceChange";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की यूनिट प्राइस चेंज हो गई है" : "Item Unit Price has changed.";
                                        item.NewUnitPrice = dealitem.DealPrice;
                                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                        {
                                            customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = dealitem.DealPrice;
                                        }
                                    }
                                }
                                else
                                {
                                    valid = false;
                                    item.IsSuccess = false;
                                    item.Message = "ItemNotActive";// !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम एक्टिव नहीं है" : "Item is not Active.";
                                }
                            }
                            else if (item.UnitPrice != item.CartUnitPrice)
                            {
                                valid = false;
                                item.IsSuccess = false;
                                item.Message = "UnitPriceChange";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की यूनिट प्राइस चेंज हो गई है" : "Item Unit Price has changed.";
                                item.NewUnitPrice = item.UnitPrice;
                                if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                                {
                                    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).UnitPrice = item.UnitPrice;
                                }
                            }
                        }
                    }

                    if (!(item.IsOffer && item.OfferType == "FlashDeal"))
                    {
                        if (valid && item.IsItemLimit && item.ItemlimitQty < item.qty)
                        {
                            item.qty = item.qty > item.ItemlimitQty ? item.ItemlimitQty : (item.ItemlimitQty - item.qty);
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "ItemLimitReach";// !string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की लिमिट ख़तम हो गई है" : "Item Limit Exceeded.";
                            if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                            {
                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                            }
                        }
                        if (valid && item.BillLimitQty > 0 && item.BillLimitQty < item.qty)
                        {
                            item.qty = item.qty > item.BillLimitQty ? item.BillLimitQty : (item.BillLimitQty - item.qty);
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "BillLimitReach";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आइटम की बिल लिमिट ख़तम हो गई है" : "Item Bill Limit Exceeded.";
                            if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == item.ItemId))
                            {
                                customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = item.qty;
                            }
                        }
                    }
                    else
                    {
                        item.BillLimitQty = 0;
                    }

                    if (valid && activeOfferids != null && item.IsOffer && item.OfferFreeItemId > 0 && item.TotalFreeItemQty > 0)
                    {
                        var offer = activeOfferids.FirstOrDefault(x => x.OfferId == item.OfferId);
                        if (offer != null && (offer.QtyAvaiable < item.TotalFreeItemQty))
                        {
                            item.OfferCategory = 0;
                            item.IsOffer = false;
                            valid = false;
                            item.IsSuccess = false;
                            item.Message = "FreeItemExpired";// !string.IsNullOrEmpty(lang) && lang == "hi" ? "फ्री आइटम ऑफर ख़तम हो गया है" : "Free Item expired.";
                        }
                        #region 8dec2021 Freebies Changes
                        //else if (!offer.IsDispatchedFreeStock && item.ItemId == item.FreeItemId && item.IsItemLimit && (item.ItemlimitQty - item.qty) < item.TotalFreeItemQty)
                        //{
                        //    int multiply = Convert.ToInt32(item.qty / offer.MinOrderQuantity);
                        //    int maxFreeItemqty = (multiply * offer.NoOffreeQuantity) > (item.ItemlimitQty - item.qty) ? (item.ItemlimitQty - item.qty) : multiply * offer.NoOffreeQuantity;
                        //    int maxMainItemqty = maxFreeItemqty * offer.MinOrderQuantity;

                        //    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = maxMainItemqty;
                        //    customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).FreeItemqty = maxFreeItemqty;
                        //    item.qty = maxMainItemqty;
                        //    item.TotalFreeItemQty = maxFreeItemqty;
                        //    valid = false;
                        //    item.IsSuccess = false;
                        //    item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आप को फ्री आइटम " + maxFreeItemqty + " क्वांटिटी से ज्यादा नहीं मिलेंगे।" : "You will not get more than " + maxFreeItemqty + " quantity of free items.";
                        //}
                        //else if (!offer.IsDispatchedFreeStock && item.ItemId != item.OfferFreeItemId)
                        //{
                        //    var FreeItem = context.itemMasters.Where(x => x.ItemId == item.OfferFreeItemId).Select(x => new { x.WarehouseId, x.ItemMultiMRPId }).FirstOrDefault();
                        //    var freelimit = context.ItemLimitMasterDB.FirstOrDefault(x => x.WarehouseId == FreeItem.WarehouseId && x.ItemMultiMRPId == FreeItem.ItemMultiMRPId);

                        //    if (freelimit != null && freelimit.IsItemLimit && (freelimit.ItemlimitQty) < item.TotalFreeItemQty)
                        //    {
                        //        int multiply = Convert.ToInt32(item.qty / offer.MinOrderQuantity);
                        //        int maxFreeItemqty = (multiply * offer.NoOffreeQuantity) > freelimit.ItemlimitQty ? freelimit.ItemlimitQty : multiply * offer.NoOffreeQuantity;
                        //        int maxMainItemqty = maxFreeItemqty * offer.MinOrderQuantity;
                        //        customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).FreeItemqty = maxFreeItemqty;
                        //        //customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == item.ItemId).qty = maxMainItemqty;
                        //        // item.qty = maxMainItemqty;
                        //        item.TotalFreeItemQty = maxFreeItemqty;
                        //        valid = false;
                        //        item.IsSuccess = false;
                        //        item.Message = !string.IsNullOrEmpty(lang) && lang == "hi" ? "आप को फ्री आइटम " + maxFreeItemqty + " क्वांटिटी से ज्यादा नहीं मिलेंगे।" : "You will not get more than " + maxFreeItemqty + " quantity of free items.";
                        //    }
                        //}
                        #endregion
                    }
                }



                if (ShoppingCartItemDcs != null && activeOfferids != null && ShoppingCartItemDcs.Any(x => x.FreeItemId > 0 && x.TotalFreeItemQty > 0))
                {
                    foreach (var item in ShoppingCartItemDcs.GroupBy(x => new { x.FreeItemId, x.ItemNumber, x.OfferId, x.ItemMultiMRPId }))
                    {
                        if (item.Sum(x => x.TotalFreeItemQty) > 0)
                        {
                            var freeItemoffer = activeOfferids.FirstOrDefault(x => x.OfferId == item.Key.OfferId);
                            if (freeItemoffer != null && freeItemoffer.QtyAvaiable < item.Sum(x => x.TotalFreeItemQty))
                            {
                                var qtyAvailable = Convert.ToInt32(freeItemoffer.QtyAvaiable);
                                foreach (var shoppingCart in ShoppingCartItemDcs.Where(x => x.ItemNumber == item.Key.ItemNumber))
                                {
                                    if (shoppingCart.TotalFreeItemQty > qtyAvailable)
                                    {
                                        shoppingCart.OfferCategory = 0;
                                        shoppingCart.IsOffer = false;
                                        shoppingCart.IsSuccess = false;
                                        shoppingCart.Message = "FreeItemExpired";// !string.IsNullOrEmpty(lang) && lang == "hi" ? "फ्री आइटम ऑफर ख़तम हो गया है" : "Free Item expired.";
                                    }
                                    else
                                    {
                                        qtyAvailable = qtyAvailable - shoppingCart.TotalFreeItemQty;
                                    }

                                }
                            }
                        }
                    }
                }



                customerShoppingCart.TotalDiscountAmt = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.DiscountAmount) : 0;

                customerShoppingCart.NewBillingWalletPoint = ShoppingCartDiscounts.Any() ? ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.NewBillingWalletPoint) : 0;
                customerShoppingCart.DeamPoint = newdata.Where(x => x.dreamPoint.HasValue).Sum(x => x.dreamPoint.Value * customerShoppingCart.ShoppingCartItems.FirstOrDefault(y => y.ItemId == x.ItemId).qty);
                customerShoppingCart.CartTotalAmt = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.qty * x.UnitPrice) - customerShoppingCart.TotalDiscountAmt;
                customerShoppingCart.TotalTaxAmount = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Sum(x => x.TaxAmount);

                customerShoppingCart.GrossTotalAmt = Math.Round(customerShoppingCart.CartTotalAmt, 0, MidpointRounding.AwayFromZero);
                customerShoppingCart.WalletAmount = walletPoint;
                customerShoppingCart.TotalSavingAmt = ShoppingCartItemDcs.Sum(x => (x.qty * x.price) - (x.qty * x.CartUnitPrice));
                customerShoppingCart.TotalQty = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                int lineItemCount = customerShoppingCart.ShoppingCartItems.Where(x => x.IsActive && !x.IsFreeItem && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Count();
                var wheelConfig = context.CompanyWheelConfiguration.FirstOrDefault();
                decimal wheelAmount = 0;
                int totalamt = Convert.ToInt32(Math.Truncate(customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt));
                customerShoppingCart.WheelCount = 0;
                if ((wheelConfig.IsKPPRequiredWheel && ActiveCustomer.IsKPP) || !ActiveCustomer.IsKPP)
                {
                    if (wheelConfig != null && wheelConfig.OrderAmount > 0 && wheelConfig.LineItemCount > 0)
                    {
                        if (lineItemCount >= wheelConfig.LineItemCount)
                        {
                            wheelAmount = wheelConfig.OrderAmount;
                            customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(totalamt / wheelAmount));
                        }
                    }
                    else if (wheelConfig != null && wheelConfig.OrderAmount > 0)
                    {
                        wheelAmount = wheelConfig.OrderAmount;
                        customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(totalamt / wheelAmount));
                    }
                    else if (wheelConfig != null && wheelConfig.LineItemCount > 0 && lineItemCount >= wheelConfig.LineItemCount)
                    {
                        wheelAmount = 0;
                        customerShoppingCart.WheelCount = Convert.ToInt32(Math.Floor(Convert.ToDecimal(lineItemCount) / wheelConfig.LineItemCount));
                    }
                }
                //customerShoppingCart.WheelCount = Convert.ToInt32(Math.Truncate((customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt) / 4000));
                customerShoppingCart.SkCode = ActiveCustomer != null ? ActiveCustomer.Skcode : "";
                customerShoppingCart.Mobile = ActiveCustomer != null ? ActiveCustomer.Mobile : "";
                customerShoppingCart.ShopName = ActiveCustomer != null ? ActiveCustomer.ShopName : "";
                customerShoppingCart.City = ActiveCustomer != null ? ActiveCustomer.City : "";
                double TotalAmount = customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt;
                double DeliveryAmount = 0;
                var storeIds = ShoppingCartItemDcs.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();
                if (deliveryCharges != null && deliveryCharges.Any(x => x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount))
                {
                    if (storeIds.All(x => x == storeIds.Max(y => y))
                         && deliveryCharges.Any(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount)
                            )
                        DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (x.storeId.HasValue && storeIds.Contains(x.storeId.Value)) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));
                    else
                        DeliveryAmount = Convert.ToDouble(deliveryCharges.Where(x => (!x.storeId.HasValue || x.storeId.Value == 0) && x.max_Amount >= TotalAmount && x.min_Amount <= TotalAmount).Max(x => x.del_Charge));

                    if (ActiveCustomer.IsPrimeCustomer)
                        DeliveryAmount = 0;
                }
                customerShoppingCart.DeliveryCharges = DeliveryAmount;

                #region TCS Calculate
                if (ActiveCustomer.CustomerType.ToLower() != "consumer")
                {
                    GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                    var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(ActiveCustomer.CustomerId, ActiveCustomer.PanNo, context);
                    if (!ActiveCustomer.IsTCSExemption && tcsConfig != null && (tcsConfig.IsAlreadyTcsUsed == true || (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + customerShoppingCart.CartTotalAmt > tcsConfig.TCSAmountLimit)))
                    {
                        customerShoppingCart.TCSPercent = !ActiveCustomer.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                        customerShoppingCart.PreTotalDispatched = tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount;
                        customerShoppingCart.TCSLimit = tcsConfig.TCSAmountLimit;
                    }
                    else
                    {
                        customerShoppingCart.TCSPercent = 0;
                        customerShoppingCart.PreTotalDispatched = 0;
                        customerShoppingCart.TCSLimit = 0;
                    }
                }
                #endregion
            }
            var result = await mongoDbHelper.ReplaceWithoutFindAsync(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
            var freeofferids = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();
            customerShoppingCartDc.ApplyOfferId = string.Join(",", ShoppingCartDiscounts.Where(x => (x.DiscountAmount > 0 || x.NewBillingWalletPoint > 0 || freeofferids.Contains(x.OfferId.Value)) && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.OfferId).ToList());
            customerShoppingCartDc.CartTotalAmt = customerShoppingCart.CartTotalAmt;
            customerShoppingCartDc.CustomerId = customerShoppingCart.CustomerId;
            customerShoppingCartDc.TCSPercent = customerShoppingCart.TCSPercent;
            customerShoppingCartDc.TCSLimit = customerShoppingCart.TCSLimit;
            customerShoppingCartDc.PreTotalDispatched = customerShoppingCart.PreTotalDispatched;
            customerShoppingCartDc.DeamPoint = customerShoppingCart.DeamPoint;
            customerShoppingCartDc.DeliveryCharges = customerShoppingCart.DeliveryCharges;
            customerShoppingCartDc.GeneratedOrderId = customerShoppingCart.GeneratedOrderId;
            customerShoppingCartDc.GrossTotalAmt = customerShoppingCart.GrossTotalAmt;
            customerShoppingCartDc.TotalDiscountAmt = customerShoppingCart.TotalDiscountAmt;
            customerShoppingCartDc.TotalTaxAmount = customerShoppingCart.TotalTaxAmount;
            customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
            customerShoppingCartDc.WarehouseId = customerShoppingCart.WarehouseId;
            customerShoppingCartDc.TotalSavingAmt = customerShoppingCart.TotalSavingAmt;
            customerShoppingCartDc.TotalQty = customerShoppingCart.TotalQty;
            customerShoppingCartDc.WheelCount = customerShoppingCart.WheelCount;
            customerShoppingCartDc.NewBillingWalletPoint = customerShoppingCart.NewBillingWalletPoint;
            customerShoppingCartDc.WalletAmount = customerShoppingCart.WalletAmount;
            customerShoppingCartDc.ShoppingCartItemDcs = ShoppingCartItemDcs;
            if (customerShoppingCart != null && customerShoppingCart.ShoppingCartDiscounts != null && customerShoppingCart.ShoppingCartDiscounts.Any())
            {
                customerShoppingCartDc.DiscountDetails = customerShoppingCart.ShoppingCartDiscounts.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value))
                                                        .Select(x => new DiscountDetail
                                                        {
                                                            DiscountAmount = x.DiscountAmount,
                                                            OfferId = x.OfferId
                                                        }).ToList();
            }

            if (EnableOtherLanguage && customerShoppingCartDc != null && customerShoppingCartDc.ShoppingCartItemDcs != null && customerShoppingCartDc.ShoppingCartItemDcs.Any() && !string.IsNullOrEmpty(lang) && lang.ToLower() != "hi" && lang.ToLower() != "en")
            {
                LanguageConvertHelper LanguageConvertHelper = new LanguageConvertHelper();
                List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest> ElasticLanguageDataRequests = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest>();
                // customerShoppingCartDc.ShoppingCartItemDcs
                ElasticLanguageDataRequests = customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.itemBaseName }).ToList();
                //ElasticLanguageDatas.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = x.itemname }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.OfferFreeItemName }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.Scheme }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.UOM }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.SellingUnitName }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.StoreName }).ToList());
                ElasticLanguageDataRequests.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = x.UnitofQuantity }).ToList());
                //ElasticLanguageDatas.AddRange(customerShoppingCartDc.ShoppingCartItemDcs.Select(x => new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = x.Message }).ToList());

                if (customerShoppingCartDc.ShoppingCartItemDcs.Any(x => x.IsSuccess == false))
                {
                    //List<DataContracts.ElasticLanguageSearch.ElasticLanguageData> msgElasticLanguageDatas = new List<DataContracts.ElasticLanguageSearch.ElasticLanguageData>();

                    foreach (var cartItem in customerShoppingCartDc.ShoppingCartItemDcs.Where(x => x.IsSuccess == false).ToList())
                    {
                        cartItem.MessageKey = cartItem.Message;
                        cartItem.Message = SCMsgEnum.SCEnum[cartItem.Message].Value;
                        // msgElasticLanguageDatas.Add(new DataContracts.ElasticLanguageSearch.ElasticLanguageData { englishtext = cartItem.Message });
                        ElasticLanguageDataRequests.Add(new DataContracts.ElasticLanguageSearch.ElasticLanguageDataRequest { englishtext = cartItem.Message });
                    }
                    //LanguageConvertHelper.CheckConvertLanguageData(msgElasticLanguageDatas.Distinct().ToList(), lang.ToLower());

                }

                var ElasticLanguageDatas = LanguageConvertHelper.GetConvertLanguageData(ElasticLanguageDataRequests.Distinct().ToList(), lang.ToLower());
                customerShoppingCartDc.ShoppingCartItemDcs.ForEach(x =>
                {
                    x.itemBaseName = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                    x.itemname = ElasticLanguageDatas.Any(y => y.englishtext == x.itemBaseName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.itemBaseName).converttext : x.itemBaseName;
                    if (x.IsSensitive == true && x.IsSensitiveMRP == true)
                    {
                        x.itemname += " " + x.price + " MRP " + x.UnitofQuantity + " " + x.UOM;
                    }
                    else if (x.IsSensitive == true && x.IsSensitiveMRP == false)
                    {
                        x.itemname += " " + x.UnitofQuantity + " " + x.UOM; //item display name 
                    }

                    else if (x.IsSensitive == false && x.IsSensitiveMRP == false)
                    {
                        x.itemname = x.itemBaseName; //item display name
                    }
                    else if (x.IsSensitive == false && x.IsSensitiveMRP == true)
                    {
                        x.itemname += " " + x.price + " MRP";//item display name 
                    }
                    x.OfferFreeItemName = ElasticLanguageDatas.Any(y => y.englishtext == x.OfferFreeItemName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.OfferFreeItemName).converttext : x.OfferFreeItemName;
                    x.SellingUnitName = ElasticLanguageDatas.Any(y => y.englishtext == x.SellingUnitName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.SellingUnitName).converttext : x.SellingUnitName;
                    x.UOM = ElasticLanguageDatas.Any(y => y.englishtext == x.UOM) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.UOM).converttext : x.UOM;
                    x.Scheme = ElasticLanguageDatas.Any(y => y.englishtext == x.Scheme) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.Scheme).converttext : x.Scheme;
                    x.UnitofQuantity = ElasticLanguageDatas.Any(y => y.englishtext == x.UnitofQuantity) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.UnitofQuantity).converttext : x.UnitofQuantity;
                    x.StoreName = ElasticLanguageDatas.Any(y => y.englishtext == x.StoreName) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.StoreName).converttext : x.StoreName;
                    x.Message = ElasticLanguageDatas.Any(y => y.englishtext == x.Message) ? ElasticLanguageDatas.FirstOrDefault(y => y.englishtext == x.Message).converttext : x.Message;
                });
            }
            else
            {
                if (customerShoppingCartDc.ShoppingCartItemDcs != null && customerShoppingCartDc.ShoppingCartItemDcs.Any(x => x.IsSuccess == false))
                {
                    foreach (var cartItem in customerShoppingCartDc.ShoppingCartItemDcs.Where(x => x.IsSuccess == false).ToList())
                    {
                        cartItem.MessageKey = cartItem.Message;
                        cartItem.Message = SCMsgEnum.SCEnum[cartItem.Message].Key;
                    }

                }

            }


            return customerShoppingCartDc;
        }

        //[Route("GetCartAmount")]
        //[HttpGet]
        //public async Task<double> GetCartAmount(int customerId, int warehouseId)
        //{
        //    double cartAmount = 0;
        //    MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
        //    var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.CustomerId == customerId && x.WarehouseId == warehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
        //    var customerShoppingCart = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
        //    if (customerShoppingCart != null)
        //    {
        //        cartAmount = customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt;
        //    }
        //    return cartAmount;
        //}

        private async Task<bool> IsValidOffer(List<int> offerIds, int applyofferId)
        {
            bool isvalid = false;
            using (var context = new AuthContext())
            {
                //var allOfferlist = await context.OfferDb.Where(x => offerIds.Contains(x.OfferId)).FirstOrDefaultAsync();
                var offerusedWithother = await context.OfferDb.Where(x => offerIds.Contains(x.OfferId)).ToListAsync();
                var ApplyofferusedWithOther = (await context.OfferDb.FirstOrDefaultAsync(x => x.OfferId == applyofferId));
                if (offerusedWithother.All(x => x.IsUseOtherOffer) && ApplyofferusedWithOther.IsUseOtherOffer)
                {
                    isvalid = true;
                }
                if ((offerusedWithother.Any(x => x.CombinedGroupId.Value > 0) || ApplyofferusedWithOther.CombinedGroupId.Value > 0) && isvalid)
                {
                    if ((offerusedWithother.Any(x => x.CombinedGroupId.Value == ApplyofferusedWithOther.CombinedGroupId.Value)))
                    {
                        isvalid = true;
                    }
                    else if (offerusedWithother.Any(x => x.CombinedGroupId.Value > 0) && ApplyofferusedWithOther.CombinedGroupId.Value > 0)
                    {
                        if ((offerusedWithother.Any(x => x.CombinedGroupId.Value > 0 && x.CombinedGroupId.Value != ApplyofferusedWithOther.CombinedGroupId.Value)))
                        {
                            isvalid = false;
                        }

                    }
                }
            }

            return isvalid;
        }

        [AllowAnonymous]
        [Route("SearchList")]
        [HttpPost]
        public async Task<SearchByCustomerDc> SearchListAsync(CustomersCartFilters customersCartFilters)
        {

            SearchByCustomerDc searchByCustomerDc = new SearchByCustomerDc();
            using (var authContext = new AuthContext())
            {
                List<CustomerShoppingCart> cartdata = new List<CustomerShoppingCart>();
                SearchByCustomerDc searchShoppingCart = new SearchByCustomerDc();
                List<CustomersCart> customersCart = new List<CustomersCart>();
                CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
                MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
                List<CustomerShoppingCart> CustomerShoppingCartList = new List<CustomerShoppingCart>();
                var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                if (!string.IsNullOrEmpty(customersCartFilters.keyword))
                {
                    cartPredicate = cartPredicate.And(x => x.SkCode.ToLower().Contains(customersCartFilters.keyword) || x.Mobile.ToLower().Contains(customersCartFilters.keyword));
                }
                if (customersCartFilters.WarehouseIds != null && customersCartFilters.WarehouseIds.Any())
                {
                    cartPredicate = cartPredicate.And(x => customersCartFilters.WarehouseIds.Contains(x.WarehouseId));
                }

                if (customersCartFilters.IsOrderNotPlaced)
                {
                    cartPredicate = cartPredicate.And(x => !x.GeneratedOrderId.HasValue || x.GeneratedOrderId.Value == 0);
                }

                var datacount = mongoDbHelper.Count(cartPredicate, collectionName: "CustomerShoppingCart");
                int skip = customersCartFilters.totalitem * customersCartFilters.page;
                var data = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), skip, customersCartFilters.totalitem, collectionName: "CustomerShoppingCart").ToList();

                //var customerShoppingCart = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), customersCartFilters.totalitem * customersCartFilters.page, customersCartFilters.totalitem, collectionName: "CustomerShoppingCart").ToList();

                List<CustomersCart> newdata = new List<CustomersCart>();
                newdata = data.Select(x => new AngularJSAuthentication.DataContracts.Transaction.ShoppingCart.CustomersCart
                {
                    CreatedDate = x.CreatedDate,
                    Mobile = x.Mobile,
                    ModifiedDate = x.ModifiedDate,
                    ShopName = x.ShopName,
                    SkCode = x.SkCode,
                    WarehouseId = x.WarehouseId,
                    GrossAmount = x.GrossTotalAmt,
                    OrderId = x.GeneratedOrderId.HasValue ? x.GeneratedOrderId.Value : 0,
                    ItemList = x.ShoppingCartItems.Select(y => new ItemCart
                    {
                        IsActive = y.IsActive,
                        ItemId = y.ItemId,
                        ItemMultiMRPId = y.ItemMultiMRPId,
                        ItemName = y.ItemName,
                        ItemNumber = y.ItemNumber,
                        qty = y.qty,
                        UnitPrice = y.UnitPrice
                    }).ToList()
                }).ToList();

                var manager = new ItemLedgerManager();
                List<ItemClassificationDC> objItemClassificationDClist = new List<ItemClassificationDC>();
                foreach (var d in newdata)

                {
                    foreach (var itemlist in d.ItemList)
                    {
                        ItemClassificationDC obj = new ItemClassificationDC();
                        obj.WarehouseId = d.WarehouseId;
                        obj.ItemNumber = itemlist.ItemNumber;
                        objItemClassificationDClist.Add(obj);
                    }

                }
                List<ItemClassificationDC> _objItemClassificationDClist = await manager.GetItemClassificationsAsync(objItemClassificationDClist);

                var warehousids = newdata.Select(x => x.WarehouseId).ToList();
                if (warehousids != null && warehousids.Any())
                {
                    using (var context = new AuthContext())
                    {
                        var warehouses = context.Warehouses.Where(x => warehousids.Contains(x.WarehouseId)).Select(x => new { Warehouseid = x.WarehouseId, WarehouseName = x.WarehouseName + " " + x.CityName }).ToList();
                        newdata.ForEach(x =>
                        {
                            x.WarehouseName = warehouses.Any(y => y.Warehouseid == x.WarehouseId) ? warehouses.FirstOrDefault(y => y.Warehouseid == x.WarehouseId).WarehouseName : "";

                        });
                    }
                }
                //if(_objItemClassificationDClist.Any())
                //{
                newdata.ForEach(x =>
                {
                    x.ItemList.ForEach(y =>
                    {
                        y.ABCClassification = _objItemClassificationDClist.Where(z => z.ItemNumber == y.ItemNumber).Select(z => z.Category).FirstOrDefault();
                        y.ABCClassification = y.ABCClassification == null ? "D" : y.ABCClassification;
                    });

                });
                //}

                searchByCustomerDc.total_count = datacount;
                searchByCustomerDc.Carts = newdata;
                return searchByCustomerDc;
            }
        }


        [Route("ExportShoppingCart")]
        [HttpPost]   // changed this to post from get -- ridhima 
        public List<CustomersCart> ExportShoppingCart(CustomersCartFilters customersCartFilters)
        {
            using (var authContext = new AuthContext())
            {
                List<CustomerShoppingCart> cartdata = new List<CustomerShoppingCart>();
                SearchByCustomerDc searchShoppingCart = new SearchByCustomerDc();
                List<CustomersCart> customersCart = new List<CustomersCart>();
                CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
                MongoDbHelper<CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<CustomerShoppingCart>();
                List<CustomerShoppingCart> CustomerShoppingCartList = new List<CustomerShoppingCart>();
                var cartPredicate = PredicateBuilder.New<CustomerShoppingCart>(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                if (!string.IsNullOrEmpty(customersCartFilters.keyword))
                {
                    cartPredicate = cartPredicate.And(x => x.SkCode.ToLower().Contains(customersCartFilters.keyword) || x.Mobile.ToLower().Contains(customersCartFilters.keyword));
                }
                if (customersCartFilters.WarehouseIds != null && customersCartFilters.WarehouseIds.Any())
                {
                    cartPredicate = cartPredicate.And(x => customersCartFilters.WarehouseIds.Contains(x.WarehouseId));
                }

                if (customersCartFilters.IsOrderNotPlaced)
                {
                    cartPredicate = cartPredicate.And(x => !x.GeneratedOrderId.HasValue || x.GeneratedOrderId.Value == 0);
                }

                var datacount = mongoDbHelper.Count(cartPredicate, collectionName: "CustomerShoppingCart");
                int skip = customersCartFilters.totalitem * customersCartFilters.page;
                var data = mongoDbHelper.Select(cartPredicate, x => x.OrderByDescending(y => y.ModifiedDate), null, null, collectionName: "CustomerShoppingCart").ToList();

                List<CustomersCart> newdata = new List<CustomersCart>();
                newdata = data.Select(x => new CustomersCart
                {
                    CreatedDate = x.CreatedDate,
                    Mobile = x.Mobile,
                    ModifiedDate = x.ModifiedDate,
                    ShopName = x.ShopName,
                    SkCode = x.SkCode,
                    WarehouseId = x.WarehouseId,
                    GrossAmount = x.GrossTotalAmt,
                    OrderId = x.GeneratedOrderId.HasValue ? x.GeneratedOrderId.Value : 0,
                    ItemList = x.ShoppingCartItems.Select(y => new ItemCart
                    {
                        IsActive = y.IsActive,
                        ItemId = y.ItemId,
                        ItemMultiMRPId = y.ItemMultiMRPId,
                        ItemName = y.ItemName,
                        ItemNumber = y.ItemNumber,
                        qty = y.qty,
                        UnitPrice = y.UnitPrice
                    }).ToList()
                }).ToList();
                var warehousids = newdata.Select(x => x.WarehouseId).ToList();
                if (warehousids != null && warehousids.Any())
                {
                    using (var context = new AuthContext())
                    {
                        var warehouses = context.Warehouses.Where(x => warehousids.Contains(x.WarehouseId)).Select(x => new { Warehouseid = x.WarehouseId, WarehouseName = x.WarehouseName + " " + x.CityName }).ToList();
                        newdata.ForEach(x =>
                        {
                            x.WarehouseName = warehouses.Any(y => y.Warehouseid == x.WarehouseId) ? warehouses.FirstOrDefault(y => y.Warehouseid == x.WarehouseId).WarehouseName : "";
                        });
                    }
                }

                return newdata;
            }
        }

        [Route("InsertDefaultNotificationMessage")]
        [HttpGet]
        public bool InsertDefaultNotificationMessage()
        {
            bool result = true;
            MongoDbHelper<DefaultNotificationMessage> mongoDbHelper = new MongoDbHelper<DefaultNotificationMessage>();
            List<DefaultNotificationMessage> DefaultNotificationMessages = new List<DefaultNotificationMessage>();
            //DefaultNotificationMessage DefaultNotificationMessage = new DefaultNotificationMessage
            //{
            //    CreatedDate = DateTime.Now,
            //    NotificationMsg = "Hi [CustomerName], You Have Left Something in Your Cart, complete your Purchase with ShopKirana on Sigle Click.",
            //    NotificationMsgType="ShopingCart"
            //};
            //DefaultNotificationMessages.Add(DefaultNotificationMessage);
            //var DefaultNotificationMessage1 = new DefaultNotificationMessage
            //{
            //    CreatedDate = DateTime.Now,
            //    NotificationMsg = "Dear [CustomerName], you have archived [Parcent]% of your monthly target, lets finish to get your best rewards.",
            //    NotificationMsgType = "Claim70"
            //};
            //DefaultNotificationMessages.Add(DefaultNotificationMessage1);
            var DefaultNotificationMessage2 = new DefaultNotificationMessage
            {
                CreatedDate = DateTime.Now,
                NotificationMsg = "Congratulations Dear [CustomerName], you have completed your monthly. Please click on CLAIM button to grab your monthly reward now.",
                NotificationMsgType = "Claim100"
            };
            DefaultNotificationMessages.Add(DefaultNotificationMessage2);
            mongoDbHelper.InsertMany(DefaultNotificationMessages);
            return result;
        }

        [Route("TestNotification")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> TestNotification()

        {
            // AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
            //return autoNotificationManager.ShoppingCartNotification();            
            //return autoNotificationManager.CustomerTargetNotification();
            // OrderMasterChangeDetectManager.SyncOrdersInMongo();
            Common.Helpers.SendSMSHelper.SendSMS("9926303404", "Hello Text msg", ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), "");

            return true;
        }
    }
}
