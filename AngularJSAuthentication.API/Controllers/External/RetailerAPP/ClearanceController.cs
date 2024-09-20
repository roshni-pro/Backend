using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.ClearanceStockNonSaleable;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Stocks;
using GenricEcommers.Models;
using LinqKit;
using MongoDB.Bson.IO;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.External.RetailerAPP
{
    [RoutePrefix("api/Clearance")]
    [Authorize]
    public class ClearanceController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region ClearanceLiveItem   

        [Route("GetClearanceLiveItemCategory/{WarehouseId}/{Customerid}/{lang}")]
        [HttpGet]
        public async Task<List<CategoryDc>> GetClearanceLiveItemCategory(int WarehouseId, int Customerid, string lang)
        {
            var result = new List<CategoryDc>();
            if (WarehouseId > 0)
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("@WarehouseId", WarehouseId);
                    var CustomerId = new SqlParameter("@Customerid", Customerid);
                    var Language = new SqlParameter("@Language", lang);
                    result = context.Database.SqlQuery<CategoryDc>("exec Clearance.GetClearanceLiveItemCategory @WarehouseId,@Customerid,@Language", param, CustomerId, Language).ToList();
                }
            }
            return result;
        }

        [Route("ClearanceLiveItemToSale")]
        [HttpPost]
        public async Task<List<ClearanceLiveItemDc>> GetClearanceLiveItemToSale(SearchClearanceLiveItemDc SearchClearanceLiveItem)
        {
            var result = new List<ClearanceLiveItemDc>();
            if (SearchClearanceLiveItem.WarehouseId > 0)
            {
                using (var context = new AuthContext())
                {
                    var paramSkip = new SqlParameter("@Skip", SearchClearanceLiveItem.Skip);
                    var paramtake = new SqlParameter("@take", SearchClearanceLiveItem.take);
                    var paramkeyword = new SqlParameter("@keyword", SearchClearanceLiveItem.keyword);
                    var paramWarehouseId = new SqlParameter("@WarehouseId", SearchClearanceLiveItem.WarehouseId);
                    var paramCategoryId = new SqlParameter("@CategoryId", SearchClearanceLiveItem.CategoryId);
                    var paramCustomerId = new SqlParameter("@Customerid", SearchClearanceLiveItem.Customerid);
                    var Language = new SqlParameter("@Language", (SearchClearanceLiveItem.lang == null ? "en" : SearchClearanceLiveItem.lang));
                    result = context.Database.SqlQuery<ClearanceLiveItemDc>("exec Clearance.GetClearanceLiveItemToSale @Skip, @take , @keyword, @WarehouseId, @CategoryId,@Customerid,@Language", paramSkip, paramtake, paramkeyword, paramWarehouseId, paramCategoryId, paramCustomerId, Language).ToList();
                }
            }
            return result;
        }

        [Route("GetClearanceLiveItemCategory/{WarehouseId}/{Customerid}")]
        [HttpGet]
        public async Task<List<CategoryDc>> GetClearanceLiveItemCategory(int WarehouseId, int Customerid)
        {
            var result = new List<CategoryDc>();
            if (WarehouseId > 0)
            {
                using (var context = new AuthContext())
                {
                    var param = new SqlParameter("@WarehouseId", WarehouseId);
                    var CustomerId = new SqlParameter("@Customerid", Customerid);
                    var Language = new SqlParameter("@Language", "en");
                    result = context.Database.SqlQuery<CategoryDc>("exec Clearance.GetClearanceLiveItemCategory @WarehouseId,@Customerid,@Language", param, CustomerId, Language).ToList();
                }
            }
            return result;
        }


        [Route("GenerateClearanceOrder")]
        [HttpPost]
        public async Task<GenerateClearanceOrderDc> GenerateClearanceOrder(ClearanceShoppingCart ClearanceShoppingCart)
        {
            var placeOrderResponse = new GenerateClearanceOrderDc();
            using (var context = new AuthContext())
            {
                MongoDbHelper<BlockGullakAmount> mongoDbHelper = new MongoDbHelper<BlockGullakAmount>();
                BlockGullakAmount blockGullakAmount = null;

                var customer = context.Customers.FirstOrDefault(x => x.CustomerId == ClearanceShoppingCart.CustomerId && x.Deleted == false);
                if (customer == null)
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Customer not found.";
                    placeOrderResponse.cart = null;
                    if (ClearanceShoppingCart.GullakAmount > 0)
                    {
                        var cartPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == ClearanceShoppingCart.CustomerId && x.Guid == blockGullakAmount.Guid);
                        var blockGullakAmountdb = mongoDbHelper.Select(cartPredicate).FirstOrDefault();
                        blockGullakAmountdb.IsActive = false;
                        var result = await mongoDbHelper.ReplaceAsync(blockGullakAmountdb.Id, blockGullakAmountdb);
                    }
                    return placeOrderResponse;
                }
                if (!customer.Active || customer.Deleted || customer.Warehouseid == 0 || customer.Warehouseid == null)
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Dear Customer you are not authorized to place order";
                    placeOrderResponse.cart = null;
                    if (ClearanceShoppingCart.GullakAmount > 0)
                    {
                        var cartPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == ClearanceShoppingCart.CustomerId && x.Guid == blockGullakAmount.Guid);
                        var blockGullakAmountdb = mongoDbHelper.Select(cartPredicate).FirstOrDefault();
                        blockGullakAmountdb.IsActive = false;
                        var result = await mongoDbHelper.ReplaceAsync(blockGullakAmountdb.Id, blockGullakAmountdb);
                    }
                    return placeOrderResponse;
                }

                #region Stop Order OverDue
                if (customer.UdharDueDays > 0)
                {
                    MongoDbHelper<UdharOverDueDayValidation> UdharOverDueDay = new MongoDbHelper<UdharOverDueDayValidation>();
                    var DueAmt = UdharOverDueDay.GetAll();
                    if (DueAmt != null && DueAmt.Any() && ClearanceShoppingCart.CustomerId > 0 && DueAmt.Max(x => x.MaxOverDueDay) > 0)
                    {
                        CheckDueAmtDc UDData = new CheckDueAmtDc();
                        var maxDay = DueAmt.Max(x => x.MaxOverDueDay);
                        var param1 = new SqlParameter("@CustomerId", ClearanceShoppingCart.CustomerId);
                        UDData = context.Database.SqlQuery<CheckDueAmtDc>("Exec CheckDueAmt @CustomerId ", param1).FirstOrDefault();

                        if (UDData != null && customer.UdharDueDays > maxDay && UDData.Amount >= 1)
                        {
                            placeOrderResponse.IsSuccess = false;
                            placeOrderResponse.Message = "Kindly clear your Udhaar overdue amount of Rs." + UDData.Amount + " before placing the order.";
                            placeOrderResponse.cart = null;
                            return placeOrderResponse;
                        }
                    }
                }
                #endregion





                if (ClearanceShoppingCart.GullakAmount > 0)
                {
                    if (ClearanceShoppingCart.GullakAmount > 0)
                    {
                        blockGullakAmount = new BlockGullakAmount
                        {
                            CreatedDate = DateTime.Now,
                            CustomerId = ClearanceShoppingCart.CustomerId,
                            Guid = Guid.NewGuid().ToString(),
                            Amount = ClearanceShoppingCart.GullakAmount,
                            IsActive = true
                        };
                        await mongoDbHelper.InsertAsync(blockGullakAmount);
                        ClearanceShoppingCart.BlockGullakAmountGuid = blockGullakAmount.Guid;
                    }
                }




                var customerGullak = context.GullakDB.FirstOrDefault(x => x.CustomerId == customer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                if (ClearanceShoppingCart.GullakAmount > 0)
                {
                    MongoDbHelper<BlockGullakAmount> mongogullakDbHelper = new MongoDbHelper<BlockGullakAmount>();
                    var gullkPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == customer.CustomerId && x.Guid != ClearanceShoppingCart.BlockGullakAmountGuid && x.IsActive);
                    var ablockGullakAmount = mongogullakDbHelper.Select(gullkPredicate).ToList().Sum(x => x.Amount);
                    if (customerGullak == null || Math.Round((customerGullak.TotalAmount - ablockGullakAmount), 2) < ClearanceShoppingCart.TotalAmount)
                    {
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "Insufficient fund in your gullak please add money to your gullak.";
                        placeOrderResponse.cart = null;
                        if (ClearanceShoppingCart.GullakAmount > 0)
                        {
                            var cartPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == ClearanceShoppingCart.CustomerId && x.Guid == blockGullakAmount.Guid);
                            var blockGullakAmountdb = mongoDbHelper.Select(cartPredicate).FirstOrDefault();
                            blockGullakAmountdb.IsActive = false;
                            var result = await mongoDbHelper.ReplaceAsync(blockGullakAmountdb.Id, blockGullakAmountdb);
                        }
                        return placeOrderResponse;
                    }
                }
                var objOrderMaster = new OrderMaster();
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                objOrderMaster.orderDetails = new List<OrderDetails>();
                if (ClearanceShoppingCart != null && ClearanceShoppingCart.itemDetails != null && ClearanceShoppingCart.itemDetails.Any())
                {
                    var cids = ClearanceShoppingCart.itemDetails.Select(x => x.Id).Distinct().ToList();
                    var citemlist = context.ClearanceLiveItemDB.Where(x => cids.Contains(x.Id)).ToList();

                    #region GetItemScheme
                    var ItemMultiMrpIds = citemlist.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    List<ItemScheme> itemPTR = retailerAppManager.GetItemScheme(ItemMultiMrpIds, customer.Warehouseid ?? 0);

                    #endregion
                    #region Default 1 Itemaster for number

                    var idlist = new DataTable();
                    idlist.Columns.Add("IntValue");
                    foreach (var item in ItemMultiMrpIds)
                    {
                        var dr = idlist.NewRow();
                        dr["IntValue"] = item;
                        idlist.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@ItemMultiMrpIds", idlist);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";

                    var paramWarehouseid = new SqlParameter("@WarehouseId", customer.Warehouseid);
                    var itemMasterlist = context.Database.SqlQuery<ClearanceOrderItemMasterDc>("exec [Clearance].[ClearanceOrderItemMaster] @ItemMultiMrpIds, @WarehouseId ", param, paramWarehouseid).ToList();

                    #endregion

                    #region store
                    List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();
                    List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
                    List<long> storeIds = new List<long>();
                    string storeName = "";
                    foreach (var item in itemMasterlist)
                    {
                        if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid))
                        {
                            var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == item.Categoryid && x.SubCategoryId == item.SubCategoryId && x.BrandId == item.SubsubCategoryid);
                            storeIds.Add(store.StoreId);
                            storeName = store.StoreName;
                        }
                    }
                    #endregion

                    var warehouses = context.Warehouses.Where(x => x.WarehouseId == customer.Warehouseid).FirstOrDefault();
                    foreach (var i in ClearanceShoppingCart.itemDetails.Where(x => x.Id > 0 && x.UnitPrice > 0))
                    {
                        i.IsSuccess = true;
                        var citem = citemlist.FirstOrDefault(z => z.Id == i.Id);
                        if (i.qty <= 0)
                        {
                            i.IsSuccess = false;
                            i.Message = "Quantity is 0.";
                        }
                        else if (!citem.IsActive || citem.IsDeleted == true)
                        {
                            i.IsSuccess = false;
                            i.Message = "Item is not Active.";
                        }
                        else if (i.UnitPrice != citem.UnitPrice)
                        {
                            i.IsSuccess = false;
                            i.Message = " Unit Price has changed.";
                            i.NewUnitPrice = citem.UnitPrice;
                        }
                        else if (i.qty > citem.RemainingStockQty)
                        {
                            i.IsSuccess = false;
                            i.Message = " stock qty changed";
                            i.NewRemainingStockQty = citem.RemainingStockQty;
                        }
                        else
                        {

                            citem.RemainingStockQty -= i.qty;
                            citem.ModifiedDate = indianTime;
                            citem.ModifiedBy = customer.CustomerId;
                            if (citem.RemainingStockQty == 0)
                            {
                                citem.IsActive = false;

                            }
                            context.Entry(citem).State = EntityState.Modified;

                            var items = itemMasterlist.FirstOrDefault(x => x.ItemMultiMprId == citem.ItemMultiMRPId);
                            OrderDetails od = new OrderDetails();
                            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid))
                            {
                                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid);
                                od.StoreId = store.StoreId;
                                if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == customer.ClusterId))
                                {
                                    var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == customer.ClusterId);
                                    od.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
                                    od.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
                                }
                            }
                            else
                            {
                                od.StoreId = 0;
                                od.ExecutiveId = 0;
                                od.ExecutiveName = "";
                            }
                            od.UnitPrice = i.UnitPrice;
                            od.CustomerId = customer.CustomerId;
                            od.CustomerName = customer.Name;
                            od.CityId = customer.Cityid;
                            od.Mobile = customer.Mobile;
                            od.OrderDate = DateTime.Now;
                            od.Status = ClearanceShoppingCart.GullakAmount > 0 ? "Pending" : "Payment Pending";
                            od.CompanyId = 1;
                            od.WarehouseId = warehouses.WarehouseId;
                            od.WarehouseName = warehouses.WarehouseName;
                            od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
                            od.ItemId = items.ItemId;
                            od.ItemMultiMRPId = items.ItemMultiMprId;
                            od.Itempic = items.LogoUrl;
                            od.itemname = items.itemname;
                            od.SupplierName = items.SupplierName;
                            od.SellingUnitName = items.itemname;
                            od.CategoryName = items.CategoryName;
                            od.SubsubcategoryName = items.SubsubcategoryName;
                            od.SubcategoryName = items.SubcategoryName;
                            od.SellingSku = items.SellingSku;
                            od.City = items.CityName;
                            od.itemcode = items.itemcode;
                            od.HSNCode = items.HSNCode;
                            od.itemNumber = items.Number;
                            od.Barcode = items.itemcode;
                            od.ActualUnitPrice = i.UnitPrice;
                            var schemeptr = itemPTR.Any(y => y.ItemMultiMRPId == items.ItemMultiMprId) ? itemPTR.FirstOrDefault(y => y.ItemMultiMRPId == items.ItemMultiMprId).PTR : 0;
                            if (schemeptr > 0)
                            {
                                od.PTR = Math.Round((schemeptr - 1) * 100, 2); //percent
                            }
                            od.price = items.MRP;
                            od.MinOrderQty = (citem.RemainingStockQty + i.qty) > items.MOQ ? items.MOQ : 1;
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
                                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + 0);
                                od.CessTaxAmount = (od.AmtWithoutAfterTaxDisc * od.TotalCessPercentage) / 100;
                            }
                            double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;
                            od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge2 / 100)) / 100;
                            od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + 0);
                            od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
                            if (od.TaxAmmount >= 0)
                            {
                                od.SGSTTaxAmmount = od.TaxAmmount / 2;
                                od.CGSTTaxAmmount = od.TaxAmmount / 2;
                            }
                            if (od.CessTaxAmount > 0)
                            {
                                double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
                                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + 0);
                                od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.CessTaxAmount + od.TaxAmmount;
                            }
                            else
                            {
                                od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
                            }
                            od.DiscountPercentage = 0;
                            od.DiscountAmmount = 0;
                            double DiscountAmmount = od.DiscountAmmount;
                            double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
                            double TaxAmmount = od.TaxAmmount;
                            od.Purchaseprice = items.PurchasePrice;
                            od.CreatedDate = DateTime.Now;
                            od.UpdatedDate = DateTime.Now;
                            od.Deleted = false;
                            od.marginPoint = 0;
                            od.ABCClassification = items.ABCClassification == null ? items.ABCClassification = null : "D";
                            od.ClearanceLiveItemId = i.Id;

                            objOrderMaster.orderDetails.Add(od);
                        }
                        if (!i.IsSuccess)
                        {
                            placeOrderResponse.IsSuccess = i.IsSuccess;
                            placeOrderResponse.Message = i.Message;
                            placeOrderResponse.cart = ClearanceShoppingCart;
                            if (ClearanceShoppingCart.GullakAmount > 0)
                            {
                                var cartPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == ClearanceShoppingCart.CustomerId && x.Guid == blockGullakAmount.Guid);
                                var blockGullakAmountdb = mongoDbHelper.Select(cartPredicate).FirstOrDefault();
                                blockGullakAmountdb.IsActive = false;
                                var result = await mongoDbHelper.ReplaceAsync(blockGullakAmountdb.Id, blockGullakAmountdb);
                            }

                            return placeOrderResponse;
                        }
                    }


                    #region billdiscount

                    List<BillDiscountOfferDc> billDiscountOfferDcs = new List<BillDiscountOfferDc>();
                    List<Model.BillDiscount.BillDiscount> ShoppingCartDiscounts = new List<Model.BillDiscount.BillDiscount>();
                    BusinessLayer.Managers.Masters.CustomersManager manager = new BusinessLayer.Managers.Masters.CustomersManager();
                    List<int> offerIds = ClearanceShoppingCart.BillDiscountOfferId != null ? ClearanceShoppingCart.BillDiscountOfferId.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();


                    billDiscountOfferDcs = offerIds.Any() ? manager.GetApplyBillDiscountById(offerIds, customer.CustomerId) : new List<DataContracts.Masters.BillDiscountOfferDc>();

                    if (billDiscountOfferDcs.Any())
                    {

                        var cartItemDetails = (from ClLive in citemlist
                                               join cart in ClearanceShoppingCart.itemDetails on ClLive.Id equals cart.Id
                                               join item in itemMasterlist on ClLive.ItemMultiMRPId equals item.ItemMultiMprId
                                               select new
                                               {
                                                   ItemMultiMRPId = ClLive.ItemMultiMRPId,
                                                   UnitPrice = ClLive.UnitPrice,
                                                   qty = cart.qty,
                                                   Id = cart.Id,
                                                   Categoryid = item.Categoryid,
                                                   SubCategoryId = item.SubCategoryId,
                                                   SubsubCategoryid = item.SubsubCategoryid
                                               }).Distinct().ToList();
                        foreach (var Offer in billDiscountOfferDcs.OrderByDescending(x => x.IsUseOtherOffer))
                        {
                            var BillDiscount = new Model.BillDiscount.BillDiscount();
                            BillDiscount.CustomerId = customer.CustomerId;
                            BillDiscount.OfferId = Offer.OfferId;
                            BillDiscount.BillDiscountType = Offer.OfferOn;
                            BillDiscount.IsAddNextOrderWallet = false;
                            BillDiscount.IsMultiTimeUse = Offer.IsMultiTimeUse;
                            BillDiscount.IsUseOtherOffer = Offer.IsUseOtherOffer;
                            BillDiscount.IsScratchBDCode = false;
                            BillDiscount.IsUsedNextOrder = false;
                            BillDiscount.IsAddNextOrderWallet = false;
                            BillDiscount.CreatedDate = indianTime;
                            BillDiscount.ModifiedDate = indianTime;
                            BillDiscount.IsActive = Offer.IsActive;
                            BillDiscount.IsDeleted = false;
                            BillDiscount.CreatedBy = customer.CustomerId;
                            BillDiscount.ModifiedBy = customer.CustomerId;
                            double totalamount = 0;
                            var OrderLineItems = 0;
                            List<int> ItemMultiMprids = new List<int>();
                            if (Offer.BillDiscountType == "ClearanceStock")
                            {
                                var itemoutofferlist = Offer.OfferBillDiscountItems.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                var iteminofferlist = Offer.OfferBillDiscountItems.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                ItemMultiMprids = iteminofferlist;
                                totalamount = ItemMultiMprids.Any() ? cartItemDetails.Where(x => ItemMultiMprids.Contains(x.ItemMultiMRPId)).Sum(x => x.qty * x.UnitPrice) : cartItemDetails.Sum(x => x.qty * x.UnitPrice);
                                OrderLineItems = ItemMultiMprids.Any() ? cartItemDetails.Where(x => ItemMultiMprids.Contains(x.ItemMultiMRPId)).Count() : cartItemDetails.Count();
                                var cartItems = ItemMultiMprids.Any() ? cartItemDetails.Where(x => ItemMultiMprids.Contains(x.ItemMultiMRPId)).ToList() : cartItemDetails;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemMultiMRPId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemMultiMRPId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemMultiMRPId).FirstOrDefault();
                                        if (ItemMultiMRPId == 0)
                                        {
                                            totalamount = 0;
                                            break;
                                        }
                                        else
                                            lineItemValueItemExists.Add(ItemMultiMRPId);
                                    }
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
                            bool IsUsed = true;
                            if (!Offer.IsUseOtherOffer && ShoppingCartDiscounts.Any())
                                IsUsed = false;

                            if (IsUsed && totalamount > 0)
                            {
                                if (Offer.BillDiscountOfferOn == "Percentage")
                                {
                                    BillDiscount.BillDiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                }
                                if (Offer.MaxDiscount > 0)
                                {
                                    if (Offer.MaxDiscount < BillDiscount.BillDiscountAmount)
                                    {
                                        BillDiscount.BillDiscountAmount = Offer.MaxDiscount;
                                    }
                                }

                                ShoppingCartDiscounts.Add(BillDiscount);
                            }

                        }

                    }
                    #endregion
                    objOrderMaster.CompanyId = warehouses.CompanyId;
                    objOrderMaster.WarehouseId = warehouses.WarehouseId;
                    objOrderMaster.WarehouseName = warehouses.WarehouseName;
                    objOrderMaster.CustomerCategoryId = 2;
                    objOrderMaster.CustomerName = customer.Name;
                    objOrderMaster.ShopName = customer.ShopName;
                    objOrderMaster.LandMark = customer.LandMark;
                    objOrderMaster.Skcode = customer.Skcode;
                    objOrderMaster.Tin_No = customer.RefNo;
                    objOrderMaster.CustomerType = customer.CustomerType;
                    objOrderMaster.CustomerId = customer.CustomerId;
                    objOrderMaster.CityId = customer.Cityid;
                    objOrderMaster.Customerphonenum = (customer.Mobile);
                    objOrderMaster.BillingAddress = customer.BillingAddress;
                    objOrderMaster.ShippingAddress = customer.ShippingAddress;
                    objOrderMaster.active = true;
                    objOrderMaster.deliveryCharge = 0;
                    if (ShoppingCartDiscounts != null && ShoppingCartDiscounts.Any())
                    {
                        objOrderMaster.BillDiscountAmount = ShoppingCartDiscounts.Sum(x => x.BillDiscountAmount);
                    }

                    objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt) - (objOrderMaster.BillDiscountAmount ?? 0) + objOrderMaster.deliveryCharge.Value, 2);
                    objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
                    objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
                    objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
                    objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero);
                    objOrderMaster.DiscountAmount = 0;
                    objOrderMaster.OrderType = 8; // Clearence Order Type
                    objOrderMaster.ClusterId = customer.ClusterId ?? 0;
                    objOrderMaster.ClusterName = customer.ClusterName;
                    objOrderMaster.IsPrimeCustomer = customer.IsPrimeCustomer;
                    objOrderMaster.Lat = ClearanceShoppingCart.Lat;
                    objOrderMaster.Lng = ClearanceShoppingCart.Lng;
                    objOrderMaster.CreatedDate = DateTime.Now;
                    objOrderMaster.Deliverydate = DateTime.Now.AddDays(1);
                    objOrderMaster.UpdatedDate = DateTime.Now;
                    objOrderMaster.paymentMode = ClearanceShoppingCart.MOP;
                    context.DbOrderMaster.Add(objOrderMaster);
                    context.Commit();
                    if (ShoppingCartDiscounts != null && ShoppingCartDiscounts.Any())
                    {
                        foreach (var item in ShoppingCartDiscounts)
                        {
                            item.OrderId = objOrderMaster.OrderId;
                            context.BillDiscountDb.Add(item);
                        }
                    }
                    List<ClearanceOrderDetail> addClearanceOrderDetails = new List<ClearanceOrderDetail>();
                    foreach (var item in objOrderMaster.orderDetails)
                    {
                        addClearanceOrderDetails.Add(new ClearanceOrderDetail { ClearanceLiveItemId = item.ClearanceLiveItemId, OrderDetailsId = item.OrderDetailsId });
                    }


                    if (customer != null)
                    {
                        customer.ordercount = customer.ordercount + 1;
                        customer.MonthlyTurnOver = customer.MonthlyTurnOver + objOrderMaster.GrossAmount;
                        context.Entry(customer).State = EntityState.Modified;
                    }
                    objOrderMaster.Status = (ClearanceShoppingCart.GullakAmount > 0 || objOrderMaster.paymentMode.Trim().ToUpper() == "COD") ? "Pending" : "Payment Pending";
                    objOrderMaster.OrderTakenSalesPersonId = ClearanceShoppingCart.PeopleId;
                    if (objOrderMaster.OrderTakenSalesPersonId > 0)
                    {
                        objOrderMaster.OrderTakenSalesPerson = context.Peoples.FirstOrDefault(x => x.PeopleID == ClearanceShoppingCart.PeopleId).DisplayName;
                    }

                    //#region TCS Calculate
                    //string fy = (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString();
                    //MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
                    //var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();
                    //objOrderMaster.TCSAmount = 0;
                    //if (tcsConfig != null)
                    //{
                    //    MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();

                    //    var tcsCustomer = mHelper.Select(x => x.CustomerId == customer.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                    //    if (tcsCustomer != null && tcsCustomer.TotalPurchase >= tcsConfig.TCSAmountLimit)
                    //    {
                    //        var percent = string.IsNullOrEmpty(customer.PanNo) ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                    //        objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount) * percent / 100;
                    //    }
                    //}
                    //#endregion
                    #region TCS Calculate
                    GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();

                    var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(customer.CustomerId, customer.PanNo, context);

                    if (tcsConfig != null && !customer.IsTCSExemption)
                    {
                        var percent = !customer.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;

                        if (tcsConfig.IsAlreadyTcsUsed == true)
                        {
                            objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount) * percent / 100;
                        }
                        //else if ((tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + objOrderMaster.TotalAmount) > tcsConfig.TCSAmountLimit)
                        //{
                        //    if (tcsConfig.TotalPurchase > tcsConfig.TCSAmountLimit)
                        //    {
                        //        objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount ) * percent / 100;
                        //    }
                        //    else if (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount > tcsConfig.TCSAmountLimit)
                        //    {
                        //        objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount) * percent / 100;
                        //    }
                        //    else
                        //    {
                        //        var TCSCalculatedAMT = (tcsConfig.TotalPurchase + tcsConfig.PendingOrderAmount + objOrderMaster.TotalAmount) - tcsConfig.TCSAmountLimit;
                        //        if (TCSCalculatedAMT > 0)
                        //        {
                        //            objOrderMaster.TCSAmount = (objOrderMaster.TotalAmount ) * percent / 100;
                        //        }
                        //    }
                        //}
                    }
                    #endregion
                    objOrderMaster.TotalAmount = objOrderMaster.TotalAmount + objOrderMaster.TCSAmount;
                    objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero);
                    if (ClearanceShoppingCart.GullakAmount > 0)
                    {
                        var GtxnId = "G" + DateTime.Now.ToString("ddMMyyyyHHmmss");
                        context.PaymentResponseRetailerAppDb.Add(new GenricEcommers.Models.PaymentResponseRetailerApp
                        {
                            amount = Math.Round(ClearanceShoppingCart.GullakAmount, 0),
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
                            Amount = (-1) * Math.Round(ClearanceShoppingCart.GullakAmount, 0),
                            GullakId = customerGullak.Id,
                            CustomerId = customerGullak.CustomerId,
                            IsActive = true,
                            IsDeleted = false,
                            ObjectId = objOrderMaster.OrderId.ToString(),
                            ObjectType = "Order"
                        });
                        customerGullak.TotalAmount -= Math.Round(ClearanceShoppingCart.GullakAmount);
                        customerGullak.ModifiedBy = customerGullak.CustomerId;
                        customerGullak.ModifiedDate = indianTime;
                        context.Entry(customerGullak).State = EntityState.Modified;
                    }
                    if (objOrderMaster.OrderId != 0)
                    {
                        string Borderid = Convert.ToString(objOrderMaster.OrderId);
                        string BorderCodeId = Borderid.PadLeft(11, '0');
                        temOrderQBcode code = context.GetBarcode(BorderCodeId);
                        objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;

                    }
                    context.Entry(objOrderMaster).State = EntityState.Modified;
                    if (objOrderMaster.paymentMode.Trim().ToUpper() == "COD")
                    {
                        context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                        {
                            amount = objOrderMaster.GrossAmount,
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
                    if (addClearanceOrderDetails.Any()) { context.ClearanceOrderDetails.AddRange(addClearanceOrderDetails); }
                    context.Commit();

                    if (ClearanceShoppingCart.GullakAmount > 0)
                    {
                        var cartPredicate = PredicateBuilder.New<BlockGullakAmount>(x => x.CustomerId == ClearanceShoppingCart.CustomerId && x.Guid == blockGullakAmount.Guid);
                        var blockGullakAmountdb = mongoDbHelper.Select(cartPredicate).FirstOrDefault();
                        blockGullakAmountdb.IsActive = false;
                        var result = await mongoDbHelper.ReplaceAsync(blockGullakAmountdb.Id, blockGullakAmountdb);
                    }

                }
                if (objOrderMaster.OrderId > 0)
                {
                    placeOrderResponse.IsSuccess = true;
                    placeOrderResponse.Message = "Order Place Successfully!!";
                    placeOrderResponse.cart = null;
                    placeOrderResponse.OrderId = objOrderMaster.OrderId;
                    placeOrderResponse.GrossAmount = objOrderMaster.GrossAmount;
                    placeOrderResponse.CreatedDate = objOrderMaster.CreatedDate;
                    placeOrderResponse.ETADate = objOrderMaster.Deliverydate;
                }
                return placeOrderResponse;
            }
        }


        //SalesApp On Checkoutbackpressed
        [Route("UpdateClPaymentFailed/{OrderId}")]
        [HttpGet]
        public async Task<ClFailedRes> UpdateClPaymentFailed(int OrderId)
        {
            var result = new ClFailedRes();

            if (OrderId > 0)
            {
                using (var authContext = new AuthContext())
                {
                    string oldstatus = "";
                    var order = await authContext.DbOrderMaster.FirstOrDefaultAsync(x => x.OrderId == OrderId);
                    if (order.Deleted)
                    {
                        result.status = true;
                        return result;
                    }
                    if (order.Status == "Pending")
                    {
                        var UpiTxn = await authContext.UPITransactions.FirstOrDefaultAsync(x => x.OrderId == OrderId && x.IsActive && x.IsSuccess);
                        result.status = UpiTxn != null ? true : false;
                        result.UPITxnId = UpiTxn != null ? UpiTxn.UPITxnID : null;
                        return result;
                    }
                    oldstatus = order.Status;
                    #region Clearence (reverse the stock to live stock) 
                    if (order.OrderType == 8 && order.Status == "Payment Pending" && oldstatus != "Failed" && oldstatus != "Order Canceled")
                    {
                        order.Deleted = true;
                        order.active = false;
                        var param = new SqlParameter("@OrderId", order.OrderId);
                        var Isupdate = await authContext.Database.ExecuteSqlCommandAsync("exec [Clearance].[UpdateClPaymentFailed] @OrderId", param);
                        authContext.Entry(order).State = EntityState.Modified;
                        await authContext.CommitAsync();
                        result.status = true;
                        return result;
                    }
                    else
                    {
                        result.Message = "Something went wrong.";
                    }
                    #endregion
                }
            }
            else
            {
                result.Message = "Something went wrong.";
            }
            return result;
        }


        [Route("GetTCSpercent/{CustomerId}")]
        [HttpGet]
        public async Task<double> GetTCSpercent(int CustomerId)
        {
            double TCSpercent = 0;
            var customer = new Customer();
            using (var context = new AuthContext())
            {
                customer = context.Customers.FirstOrDefault(x => x.CustomerId == CustomerId);

                //#region TCS Calculate
                //string fy = (DateTime.Now.Month >= 4 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString();
                //MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
                //var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();
                //if (tcsConfig != null)
                //{
                //    MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                //    var tcsCustomer = mHelper.Select(x => x.CustomerId == customer.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                //    if (tcsCustomer != null && tcsCustomer.TotalPurchase >= tcsConfig.TCSAmountLimit)
                //    {
                //        var percent = string.IsNullOrEmpty(customer.RefNo) ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                //        TCSpercent = percent;
                //    }
                //}
                //return TCSpercent;

                //#endregion

                #region TCS Calculate
                bool IsConsumer = (customer.CustomerType != null && customer.CustomerType.ToLower() == "consumer") ? true : false;
                if (!IsConsumer)
                {
                    GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                    var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(customer.CustomerId, customer.PanNo, context);
                    if (!customer.IsTCSExemption && tcsConfig != null && tcsConfig.IsAlreadyTcsUsed == true)
                    {
                        TCSpercent = !customer.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                    }
                }
                return TCSpercent;
            }
            #endregion
        }

        [Route("UpdateClFailedOrderRemngQtyJob")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> UpdateClFailedOrderRemngQtyJob()
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                if (await context.Database.ExecuteSqlCommandAsync("exec [Clearance].[UpdateClFailedOrderRemngQty]") > 0) { result = true; }
            }
            return result;
        }



        [Route("UpdateClOfferEndDateJob")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> UpdateClOfferEndDateJob()
        {
            bool result = false;
            using (var context = new AuthContext())
            {
                if (await context.Database.ExecuteSqlCommandAsync("exec [Clearance].[UpdateClOfferEndDateJob]") > 0) { result = true; }
            }
            return result;
        }



        [Route("OrderValidateForPayment/{OrderId}")]
        [HttpGet]
        public async Task<bool> OrderValidateForPayment(int OrderId)
        {
            bool result = false;
            using (var db = new AuthContext())
            {
                var Query = "exec Clearance.IsOrderValidateForPayment " + OrderId;
                result = await db.Database.SqlQuery<bool>(Query).FirstOrDefaultAsync();

            }
            return result;
        }

        [Route("ApplyOffer")]
        [HttpPost]
        public async Task<ApplyOfferResponse> ApplyOffer(ClearanceShoppingCart ClearanceShoppingCart, bool IsApply, int PeopleId = 0)
        {
            ApplyOfferResponse applyOfferResponse = new ApplyOfferResponse { cart = ClearanceShoppingCart.itemDetails, IsSuccess = false, BillDiscount = 0 };
            List<ClearanceDiscount> ShoppingCartDiscounts = new List<ClearanceDiscount>();
            using (var context = new AuthContext())
            {
                var customer = context.Customers.FirstOrDefault(x => x.CustomerId == ClearanceShoppingCart.CustomerId && x.Deleted == false);

                if (ClearanceShoppingCart != null && ClearanceShoppingCart.itemDetails != null && ClearanceShoppingCart.itemDetails.Any())
                {
                    var cids = ClearanceShoppingCart.itemDetails.Select(x => x.Id).Distinct().ToList();
                    var citemlist = context.ClearanceLiveItemDB.Where(x => cids.Contains(x.Id)).ToList();


                    var ItemMultiMrpIds = citemlist.Select(x => x.ItemMultiMRPId).Distinct().ToList();

                    #region Default 1 Itemaster for number

                    var idlist = new DataTable();
                    idlist.Columns.Add("IntValue");
                    foreach (var item in ItemMultiMrpIds)
                    {
                        var dr = idlist.NewRow();
                        dr["IntValue"] = item;
                        idlist.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@ItemMultiMrpIds", idlist);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    var paramWarehouseid = new SqlParameter("@WarehouseId", customer.Warehouseid);
                    var itemMasterlist = context.Database.SqlQuery<ClearanceOrderItemMasterDc>("exec [Clearance].[ClearanceOrderItemMaster] @ItemMultiMrpIds, @WarehouseId ", param, paramWarehouseid).ToList();

                    #endregion


                    List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = new List<DataContracts.Masters.BillDiscountOfferDc>();

                    BusinessLayer.Managers.Masters.CustomersManager manager = new BusinessLayer.Managers.Masters.CustomersManager();
                    List<int> offerIds = ClearanceShoppingCart.BillDiscountOfferId != null ? ClearanceShoppingCart.BillDiscountOfferId.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();

                    foreach (var i in ClearanceShoppingCart.itemDetails.Where(x => x.Id > 0 && x.UnitPrice > 0))
                    {
                        i.IsSuccess = true;
                        var citem = citemlist.FirstOrDefault(z => z.Id == i.Id);
                        if (i.qty <= 0)
                        {
                            i.IsSuccess = false;
                            i.Message = "Quantity is 0.";
                        }
                        else if (!citem.IsActive || citem.IsDeleted == true)

                        {
                            i.IsSuccess = false;
                            i.Message = "Item is not Active.";
                        }
                        else if (i.UnitPrice != citem.UnitPrice)
                        {
                            i.IsSuccess = false;
                            i.Message = " Unit Price has changed.";
                            i.NewUnitPrice = citem.UnitPrice;
                        }
                        else if (i.qty > citem.RemainingStockQty)
                        {
                            i.IsSuccess = false;
                            i.Message = " stock qty changed";
                            i.NewRemainingStockQty = citem.RemainingStockQty;
                        }

                        if (!i.IsSuccess)
                        {
                            applyOfferResponse.IsSuccess = i.IsSuccess;
                            applyOfferResponse.Message = i.Message;
                            applyOfferResponse.cart = ClearanceShoppingCart.itemDetails;
                            return applyOfferResponse;
                        }
                    }

                    billDiscountOfferDcs = offerIds.Any() ? manager.GetApplyBillDiscountById(offerIds, customer.CustomerId) : new List<DataContracts.Masters.BillDiscountOfferDc>();

                    if (billDiscountOfferDcs.Any())
                    {

                        var cartItemDetails = (from ClLive in citemlist
                                               join cart in ClearanceShoppingCart.itemDetails on ClLive.Id equals cart.Id
                                               join item in itemMasterlist on ClLive.ItemMultiMRPId equals item.ItemMultiMprId
                                               select new
                                               {
                                                   ItemMultiMRPId = ClLive.ItemMultiMRPId,
                                                   UnitPrice = ClLive.UnitPrice,
                                                   qty = cart.qty,
                                                   Id = cart.Id,
                                                   Categoryid = item.Categoryid,
                                                   SubCategoryId = item.SubCategoryId,
                                                   SubsubCategoryid = item.SubsubCategoryid
                                               }).Distinct().ToList();
                        foreach (var Offer in billDiscountOfferDcs.OrderByDescending(x => x.IsUseOtherOffer))
                        {
                            ClearanceDiscount ShoppingCartDiscount = new ClearanceDiscount();
                            double totalamount = 0;
                            var OrderLineItems = 0;
                            //if (Offer.OfferOn != "ScratchBillDiscount")
                            //{
                            List<int> ItemMultiMprids = new List<int>();
                            if (Offer.BillDiscountType == "ClearanceStock")
                            {

                                var itemoutofferlist = Offer.OfferBillDiscountItems.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                var iteminofferlist = Offer.OfferBillDiscountItems.Where(x => x.IsInclude).Select(x => x.Id).ToList();


                                //ItemMultiMprids = itemMasterlist.Where(x => (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemMultiMprId))
                                //&& !itemoutofferlist.Contains(x.ItemMultiMprId)
                                //).Select(x => x.ItemMultiMprId).ToList();
                                ItemMultiMprids = iteminofferlist;

                                totalamount = ItemMultiMprids.Any() ? cartItemDetails.Where(x => ItemMultiMprids.Contains(x.ItemMultiMRPId)).Sum(x => x.qty * x.UnitPrice) : cartItemDetails.Sum(x => x.qty * x.UnitPrice);
                                OrderLineItems = ItemMultiMprids.Any() ? cartItemDetails.Where(x => ItemMultiMprids.Contains(x.ItemMultiMRPId)).Count() : cartItemDetails.Count();
                                var cartItems = ItemMultiMprids.Any() ? cartItemDetails.Where(x => ItemMultiMprids.Contains(x.ItemMultiMRPId)).ToList() : cartItemDetails;
                                if (cartItems != null && cartItems.Any() && Offer.OfferLineItemValueDcs != null && Offer.OfferLineItemValueDcs.Any(x => x.itemValue > 0))
                                {
                                    List<int> lineItemValueItemExists = new List<int>();
                                    foreach (var item in Offer.OfferLineItemValueDcs.Where(x => x.itemValue > 0))
                                    {
                                        int ItemMultiMRPId = cartItems.Where(x => !lineItemValueItemExists.Contains(x.ItemMultiMRPId) && (x.qty * x.UnitPrice) > item.itemValue).OrderBy(x => (x.qty * x.UnitPrice)).Select(x => x.ItemMultiMRPId).FirstOrDefault();
                                        if (ItemMultiMRPId == 0)
                                        {
                                            totalamount = 0;
                                            break;
                                        }
                                        else
                                            lineItemValueItemExists.Add(ItemMultiMRPId);
                                    }
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



                            bool IsUsed = true;
                            if (!Offer.IsUseOtherOffer && ShoppingCartDiscounts.Any())
                                IsUsed = false;



                            if (IsUsed && totalamount > 0)
                            {
                                if (Offer.BillDiscountOfferOn == "Percentage")
                                {
                                    ShoppingCartDiscount.DiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                }
                                if (Offer.MaxDiscount > 0)
                                {
                                    if (Offer.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                    {
                                        ShoppingCartDiscount.DiscountAmount = Offer.MaxDiscount;
                                    }
                                }

                                ShoppingCartDiscount.OfferId = Offer.OfferId;
                                ShoppingCartDiscounts.Add(ShoppingCartDiscount);
                            }

                        }

                        if (ShoppingCartDiscounts != null && ShoppingCartDiscounts.Any())
                        {
                            applyOfferResponse.BillDiscount = ShoppingCartDiscounts.Sum(x => x.DiscountAmount);
                            applyOfferResponse.BillDiscountOffers = ShoppingCartDiscounts;
                        }
                    }

                    if (IsApply)
                    {
                        if (ClearanceShoppingCart.BillDiscountOfferId.Any())
                        {
                            foreach (var offerid in ClearanceShoppingCart.BillDiscountOfferId)
                            {
                                if (applyOfferResponse.BillDiscountOffers != null && applyOfferResponse.BillDiscountOffers.Select(x => Convert.ToInt64(x.OfferId)).Contains(offerid))
                                {
                                    applyOfferResponse.IsSuccess = true;
                                    applyOfferResponse.Message = "Offer apply successfully.";
                                }
                                else
                                {
                                    applyOfferResponse.IsSuccess = false;
                                    applyOfferResponse.Message = "You are not eligible of this offer.";
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        applyOfferResponse.IsSuccess = true;
                        applyOfferResponse.Message = "Offer removed successfully";
                    }
                }
                else
                {
                    applyOfferResponse.IsSuccess = false;
                    applyOfferResponse.Message = "Cart is empty";
                }
            }

            return applyOfferResponse;
        }


        //-------------------------------------------------------//ClearanceAprprover Live Item-------------------------------------------------------------------//


        #region Clearance Item Status Update Page
        [Route("GetClearanceLiveItem")]
        [HttpPost]
        public async Task<ClearanceLiveItemList> GetClearanceLiveItem(SearchClearanceLiveItemDc SearchClearanceLiveItem)
        {
            var identity = User.Identity as ClaimsIdentity;
            ClearanceLiveItemList res = new ClearanceLiveItemList();
            var result = new List<ClearanceLiveItemListDc>();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            var HQlogin = roleNames.Where(x => x == "HQ Master login").FirstOrDefault();
            if (roleNames != null && roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && userid > 0 && SearchClearanceLiveItem.WarehouseId > 0)
            {
                userid = roleNames.Any(x => (x == "Buyer" || x == "Sourcing Executive") && HQlogin != "HQ Master login") ? userid : 0;

                using (var context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[GetClearanceLiveItem]";
                    cmd.Parameters.Add(new SqlParameter("@WarehouseId", SearchClearanceLiveItem.WarehouseId));
                    cmd.Parameters.Add(new SqlParameter("@skip", SearchClearanceLiveItem.Skip));
                    cmd.Parameters.Add(new SqlParameter("@take", SearchClearanceLiveItem.take));
                    cmd.Parameters.Add(new SqlParameter("@keyword", SearchClearanceLiveItem.keyword));
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userid });
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    var newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ClearanceLiveItemListDc>(reader).ToList();

                    reader.NextResult();
                    while (reader.Read())
                    {
                        res.ClearanceLiveItemLists = newdata;
                        res.TotalRecords = Convert.ToInt32(reader["itemCount"]);
                    }
                }

            }
            return res;


        }


        [Route("GetClearanceLiveItemExport")]
        [HttpPost]
        public async Task<List<ClearanceLiveItemListDc>> GetClearanceLiveItemExport(ExportClearanceLiveItemFilterDc exportClearanceLiveItemFilterDc)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            var result = new List<ClearanceLiveItemListDc>();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (roleNames != null && roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && userid > 0)
            {
                if (exportClearanceLiveItemFilterDc.WarehouseId > 0)
                {
                    using (var context = new AuthContext())
                    {
                        var widparam = new SqlParameter("@WarehouseId", exportClearanceLiveItemFilterDc.WarehouseId);
                        var keywordparam = new SqlParameter("@Keyword", exportClearanceLiveItemFilterDc.keyword);
                        result = context.Database.SqlQuery<ClearanceLiveItemListDc>("exec Clearance.GetClearanceLiveItemExport @WarehouseId, @Keyword", widparam, keywordparam).ToList();
                    }
                }
            }
            return result;
        }


        [Route("ActiveInactive")]
        [HttpGet]
        public async Task<ResponseMsg> ActiveInactive(long ClearanceId, bool isActive)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            ResponseMsg result = new ResponseMsg();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (roleNames != null && roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && userid > 0)
            {
                DateTime currentTime = DateTime.Now;
                using (var context = new AuthContext())
                {
                    var clearanceData = context.ClearanceLiveItemDB.FirstOrDefault(x => x.Id == ClearanceId);

                    if (clearanceData != null)
                    {
                        clearanceData.IsActive = isActive;
                        clearanceData.ModifiedBy = userid;
                        clearanceData.ModifiedDate = currentTime;
                        if (context.Commit() > 0)
                        {
                            result.Status = true;
                            result.Message = " Successfully Updated";
                        }

                    }
                }
                return result;
            }
            else
            {
                result.Status = false;
                result.Message = " You are not authorized ";
                return result;
            }

        }

        #endregion



        #region Clearance Live Item Edit Page
        [Route("GetGroupNameList")]
        [HttpGet]
        public List<GetGroupNameDc> GetGroupNameList()
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            var result = new List<GetGroupNameDc>();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (roleNames != null && roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && userid > 0)
            {
                using (var context = new AuthContext())
                {
                    result = context.Database.SqlQuery<GetGroupNameDc>("exec GetGroupName").ToList();
                }

            }
            return result;
        }

        [Route("GetItemList")]
        [HttpGet]
        public List<ItemListsDc> GetItemList(int Categoryid, int SubCategoryid, int Brandid)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;
            var result = new List<ItemListsDc>();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (roleNames != null && roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && userid > 0)
            {

                using (var context = new AuthContext())
                {
                    var param1 = new SqlParameter("@Categoryid", Categoryid);
                    var param2 = new SqlParameter("@SubCategoryid", SubCategoryid);
                    var param3 = new SqlParameter("@Brandid", Brandid);
                    result = context.Database.SqlQuery<ItemListsDc>("exec Sp_ItemList @CategoryId, @SubCategoryId,@BrandId", param1, param2, param3).ToList();
                }

            }
            return result;
        }


        [Route("GetClearanceStockList")]
        [HttpPost]
        public ClearanceStockLists GetClearanceStockList(SearchClearanceStockDc SearchClearanceStockDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            ClearanceStockLists result = new ClearanceStockLists();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            var HQlogin = roleNames.Where(x => x == "HQ Master login").FirstOrDefault();
            if (roleNames != null && roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && userid > 0 && SearchClearanceStockDc.WarehouseId > 0)
            {
                userid = roleNames.Any(x => (x == "Buyer" || x == "Sourcing Executive") && HQlogin != "HQ Master login") ? userid : 0;

                using (var context = new AuthContext())
                {

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[GetClearanceStockList]";
                    cmd.Parameters.Add(new SqlParameter("@WarehouseId", SearchClearanceStockDc.WarehouseId));
                    cmd.Parameters.Add(new SqlParameter("@Keyword", SearchClearanceStockDc.keyword));
                    cmd.Parameters.Add(new SqlParameter("@Skip", SearchClearanceStockDc.Skip));
                    cmd.Parameters.Add(new SqlParameter("@Take", SearchClearanceStockDc.take));
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userid });

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    var newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ClearanceStockListDc>(reader).ToList();

                    reader.NextResult();
                    while (reader.Read())
                    {
                        result.ClearanceStockListDc = newdata;

                        result.TotalRecords = Convert.ToInt32(reader["Totalrecords"]);
                    }
                }


            }
            return result;
        }



        [Route("GetClearanceStockListExport")]
        [HttpPost]
        public ClearanceStockLists GetClearanceStockListExport(SearchClearanceStockDc SearchClearanceStockDc)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            ClearanceStockLists result = new ClearanceStockLists();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            var HQlogin = roleNames.Where(x => x == "HQ Master login").FirstOrDefault();
            if (roleNames != null && roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && userid > 0 && SearchClearanceStockDc.WarehouseId > 0)
            {
                userid = roleNames.Any(x => (x == "Buyer" || x == "Sourcing Executive") && HQlogin != "HQ Master login") ? userid : 0;

                using (var context = new AuthContext())
                {

                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[GetClearanceStockList]";
                    cmd.Parameters.Add(new SqlParameter("@WarehouseId", SearchClearanceStockDc.WarehouseId));
                    cmd.Parameters.Add(new SqlParameter("@Keyword", SearchClearanceStockDc.keyword));
                    cmd.Parameters.Add(new SqlParameter("@IsExport", SearchClearanceStockDc.IsExport));
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userid });

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    var newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<ClearanceStockListDc>(reader).ToList();
                    result.ClearanceStockListDc = newdata;

                }


            }
            return result;
        }


        [Route("ClearanceLiveItemsAllExport")]
        [HttpPost]
        public List<ClearanceStockListDc> ClearanceLiveItemsAllExport(ClearanceLiveItemsAllExportDC WarehouseIds)
        {

            var result = new List<ClearanceStockListDc>();
            using (var context = new AuthContext())
            {
                if (WarehouseIds != null)
                {
                    var idlist = new DataTable();
                    idlist.Columns.Add("IntValue");
                    foreach (var item in WarehouseIds.WarehouseIds)
                    {
                        var dr = idlist.NewRow();
                        dr["IntValue"] = item;
                        idlist.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@WarehouseIds", idlist);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";

                    result = context.Database.SqlQuery<ClearanceStockListDc>("Exec ClearanceLiveItemsAllExport @WarehouseIds", param).ToList();


                }
            }
            return result;
        }


        [Route("UpdateClearanceStockLiveItem")]
        [HttpPost]
        public async Task<ResponseMsg> UpdateClearanceStockLiveItem(List<UpdateClearanceStockDc> updateClearanceStockDc)
        {
            bool Isupdated = false;
            ResponseMsg result = new ResponseMsg();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            List<Offer> list = new List<Offer>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            ClearanceLiveItem clearanceLiveItem = new ClearanceLiveItem();
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(120);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                if (roleNames.Any(x => x == "Buyer" || x == "Sourcing Executive" || x == "HQ Master login") && updateClearanceStockDc != null && updateClearanceStockDc.Any())
                {
                    using (var context = new AuthContext())
                    {
                        var wid = updateClearanceStockDc.FirstOrDefault().WarehouseId;
                        var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseId == wid);
                        foreach (var item in updateClearanceStockDc)
                        {
                            var param1 = new SqlParameter("Stockid", item.ClearanceStockBatchMasterId);
                            var param2 = new SqlParameter("WarehouseId", warehouse.WarehouseId);
                            var StockQuantity = context.Database.SqlQuery<int>("exec Clearance.StockQty @Stockid,@WarehouseId", param1, param2).FirstOrDefault();
                            if (StockQuantity < item.AvailableQty)
                            {
                                result.Status = false;
                                result.Message = "Can't Live Qty, Clearance Remaining Qty is less for Batch Item :" + item.BatchCode;
                                return result;
                            }

                            var clearanceLiveItems = context.ClearanceLiveItemDB.FirstOrDefault(a => a.WarehouseId == item.WarehouseId && a.ClearanceStockBatchMasterId == item.ClearanceStockBatchMasterId && a.IsDeleted == false);

                            if (clearanceLiveItems == null)
                            {
                                //Offer offers = new Offer();
                                //if (!string.IsNullOrEmpty(item.ApplyType) && item.IsOfferGenerated)
                                //{
                                //    offers.ApplyOn = "PreOffer";
                                //    offers.ApplyType = item.ApplyType;
                                //    offers.BillAmount = 10;
                                //    offers.BillDiscountOfferOn = item.DiscountType;
                                //    offers.BillDiscountType = "ClearanceStock";
                                //    offers.CityId = warehouse.Cityid;
                                //    offers.CompanyId = 1;
                                //    offers.CreatedDate = DateTime.Now;
                                //    offers.DiscountPercentage = item.DiscountType == "Percentage" ? item.Discount : 0;
                                //    offers.start = DateTime.Now;
                                //    offers.end = DateTime.Now.AddDays(7);
                                //    offers.GroupId = item.GroupId;
                                //    offers.IsCRMOffer = false;
                                //    offers.IsDispatchedFreeStock = false;
                                //    offers.IsMultiTimeUse = true;
                                //    offers.IsUseOtherOffer = true;
                                //    offers.IsAutoApply = true;
                                //    offers.IsActive = true;
                                //    offers.IsDeleted = false;
                                //    offers.itemId = 0;
                                //    offers.itemname = item.itemname;
                                //    offers.LineItem = 1;
                                //    offers.MaxBillAmount = 200000;
                                //    offers.MaxQtyPersonCanTake = 0;
                                //    offers.MaxDiscount = 100000;
                                //    offers.MinOrderQuantity = 1;
                                //    offers.OfferAppType = "Both";
                                //    offers.OfferCategory = "Offer";
                                //    offers.OfferName = "ClearanceStockOffer on " + item.itemname;
                                //    offers.OfferOn = "BillDiscount";
                                //    offers.Description = "";
                                //    offers.WarehouseId = item.WarehouseId;
                                //    context.OfferDb.Add(offers);
                                //}
                                clearanceLiveItem.ItemMultiMRPId = item.ItemMultiMRPId;
                                clearanceLiveItem.LiveStockQty = item.AvailableQty;
                                clearanceLiveItem.ClearanceStockBatchMasterId = item.ClearanceStockBatchMasterId;
                                clearanceLiveItem.RemainingStockQty = item.AvailableQty;
                                clearanceLiveItem.DefaultUnitPrice = item.UnitPrice;
                                clearanceLiveItem.UnitPrice = item.IsOfferGenerated ? item.UnitPrice : item.ClPrice;
                                clearanceLiveItem.DiscountType = item.DiscountType;
                                clearanceLiveItem.Discount = item.Discount;
                                clearanceLiveItem.WarehouseId = item.WarehouseId;
                                clearanceLiveItem.GroupId = item.GroupId;
                                clearanceLiveItem.ApplyType = item.ApplyType;
                                clearanceLiveItem.IsActive = true;
                                clearanceLiveItem.IsDeleted = false;
                                clearanceLiveItem.CreatedDate = DateTime.Now;
                                clearanceLiveItem.CreatedBy = userid;
                                context.ClearanceLiveItemDB.Add(clearanceLiveItem);
                                context.Commit();
                                //if (!string.IsNullOrEmpty(item.ApplyType) && item.IsOfferGenerated)
                                //{
                                //    offers.OfferCode = "CL_" + offers.OfferId;
                                //    context.Entry(offers).State = EntityState.Modified;
                                //    BillDiscountOfferSection billDiscountOfferSection = new BillDiscountOfferSection
                                //    {
                                //        IsInclude = true,
                                //        ObjId = Convert.ToInt32(clearanceLiveItems.Id),
                                //        OfferId = offers.OfferId,
                                //    };
                                //    context.BillDiscountOfferSectionDB.Add(billDiscountOfferSection);

                                //    ClearanceLiveItemOffer clearanceLiveItemOffer = new ClearanceLiveItemOffer
                                //    {
                                //        ClearanceLiveItemId = Convert.ToInt32(clearanceLiveItems.Id),
                                //        OfferId = offers.OfferId,


                                //        IsActive = true,
                                //        IsDeleted = false,
                                //        CreatedDate = DateTime.Now,
                                //        CreatedBy = userid,
                                //    };
                                //    context.ClearanceLiveItemOffers.Add(clearanceLiveItemOffer);
                                //    context.Commit();
                                //    Isupdated = true;
                                //}
                                //if (string.IsNullOrEmpty(item.ApplyType) && item.IsOfferGenerated == false)
                                //{
                                Isupdated = true;
                                //}
                            }
                            else
                            {
                                //var Existoffer = context.ClearanceLiveItemOffers.OrderByDescending(x => x.CreatedDate).FirstOrDefault(x => x.ClearanceLiveItemId == clearanceLiveItems.Id);
                                //if (Existoffer != null)
                                //{
                                //    var offer = context.OfferDb.FirstOrDefault(x => x.OfferId == Existoffer.OfferId);
                                //    if (offer.GroupId == item.GroupId && item.GroupId > 0 && offer.GroupId > 0 && item.IsOfferGenerated)
                                //    {
                                //        offer.end = DateTime.Now.AddDays(7);
                                //        offer.IsActive = true;
                                //        Existoffer.IsActive = true;
                                //    }
                                //    else if (item.IsOfferGenerated)
                                //    {
                                //        offer.GroupId = item.GroupId;
                                //        offer.end = DateTime.Now.AddDays(7);
                                //        offer.IsActive = true;
                                //        Existoffer.IsActive = true;
                                //    }

                                //    else
                                //    {
                                //        offer.IsActive = false;
                                //        Existoffer.IsActive = false;
                                //    }


                                //}
                                //else if (Existoffer == null && item.IsOfferGenerated)
                                //{
                                //    Offer offers = new Offer();
                                //    if (!string.IsNullOrEmpty(item.ApplyType) && item.IsOfferGenerated)
                                //    {
                                //        offers.ApplyOn = "PreOffer";
                                //        offers.ApplyType = item.ApplyType;
                                //        offers.BillAmount = 10;
                                //        offers.BillDiscountOfferOn = item.DiscountType;
                                //        offers.BillDiscountType = "ClearanceStock";
                                //        offers.CityId = warehouse.Cityid;
                                //        offers.CompanyId = 1;
                                //        offers.CreatedDate = DateTime.Now;
                                //        offers.DiscountPercentage = item.DiscountType == "Percentage" ? item.Discount : 0;
                                //        offers.start = DateTime.Now;
                                //        offers.end = DateTime.Now.AddDays(7);
                                //        offers.GroupId = item.GroupId;
                                //        offers.IsCRMOffer = false;
                                //        offers.IsDispatchedFreeStock = false;
                                //        offers.IsMultiTimeUse = true;
                                //        offers.IsUseOtherOffer = true;
                                //        offers.IsActive = true;
                                //        offers.IsAutoApply = true;
                                //        offers.IsDeleted = false;
                                //        offers.itemId = 0;
                                //        offers.itemname = item.itemname;
                                //        offers.LineItem = 1;
                                //        offers.MaxBillAmount = 200000;
                                //        offers.MaxQtyPersonCanTake = 0;
                                //        offers.MaxDiscount = 100000;
                                //        offers.MinOrderQuantity = 1;
                                //        offers.OfferAppType = "Both";
                                //        offers.OfferCategory = "Offer";
                                //        offers.OfferName = "ClearanceStockOffer on " + item.itemname;
                                //        offers.OfferOn = "BillDiscount";
                                //        offers.WarehouseId = item.WarehouseId;
                                //        offers.Description = "";
                                //        context.OfferDb.Add(offers);
                                //        context.Commit();
                                //        offers.OfferCode = "CL_" + offers.OfferId;
                                //        context.Entry(offers).State = EntityState.Modified;
                                //        BillDiscountOfferSection billDiscountOfferSection = new BillDiscountOfferSection
                                //        {
                                //            IsInclude = true,
                                //            ObjId = Convert.ToInt32(clearanceLiveItems.Id),
                                //            OfferId = offers.OfferId,
                                //        };
                                //        context.BillDiscountOfferSectionDB.Add(billDiscountOfferSection);
                                //        ClearanceLiveItemOffer clearanceLiveItemOffer = new ClearanceLiveItemOffer
                                //        {
                                //            ClearanceLiveItemId = clearanceLiveItems.Id,
                                //            OfferId = offers.OfferId,
                                //            IsActive = true,
                                //            IsDeleted = false,
                                //            CreatedDate = DateTime.Now,
                                //            CreatedBy = userid,
                                //        };
                                //        context.ClearanceLiveItemOffers.Add(clearanceLiveItemOffer);
                                //    }

                                clearanceLiveItems.LiveStockQty += item.AvailableQty > 0 ? item.AvailableQty : 0;
                                clearanceLiveItems.RemainingStockQty += item.AvailableQty > 0 ? item.AvailableQty : 0;
                                clearanceLiveItems.DefaultUnitPrice = item.UnitPrice;
                                clearanceLiveItems.UnitPrice = item.IsOfferGenerated ? item.UnitPrice : item.ClPrice;
                                clearanceLiveItems.DiscountType = item.DiscountType;
                                clearanceLiveItems.Discount = item.Discount;
                                clearanceLiveItems.WarehouseId = item.WarehouseId;
                                clearanceLiveItems.GroupId = item.GroupId;
                                clearanceLiveItems.ApplyType = item.ApplyType;
                                clearanceLiveItems.ModifiedDate = DateTime.Now;
                                clearanceLiveItems.ModifiedBy = userid;
                                clearanceLiveItems.IsActive = true;
                                clearanceLiveItems.IsDeleted = false;
                                context.Entry(clearanceLiveItems).State = EntityState.Modified;
                                context.Commit();
                                Isupdated = true;
                            }
                        }
                    }
                    if (Isupdated)
                    {
                        dbContextTransaction.Complete();
                        result.Status = true;
                        result.Message = "Successfully";
                        return result;
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "something went wrong.";
                        return result;
                    }
                }
                else
                {
                    result.Status = false;
                    result.Message = " You are not authorized ";
                    return result;
                }
            }
        }

        #endregion

        [Route("AvailableItemForClNSOrder")]
        [HttpGet]
        [AllowAnonymous]
        public AvailableItemForClNSOrderList AvailableItemForClNSOrder(int WarehouseId, int skip, int take)
        {
            AvailableItemForClNSOrderList res = new AvailableItemForClNSOrderList();
            if (WarehouseId > 0 && skip >= 0 && take > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    if (context.Database.Connection.State != ConnectionState.Open)
                        context.Database.Connection.Open();
                    var cmd = context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 900;
                    cmd.CommandText = "[Clearance].[AvailableItemForClNSOrder]";
                    cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                    cmd.Parameters.Add(new SqlParameter("@skip", skip));
                    cmd.Parameters.Add(new SqlParameter("@take", take));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    var reader = cmd.ExecuteReader();
                    var newdata = ((IObjectContextAdapter)context)
                    .ObjectContext
                    .Translate<AvailableItemForClNSOrderDC>(reader).ToList();

                    reader.NextResult();
                    while (reader.Read())
                    {
                        res.AvailableItemForClNSOrderDC = newdata;
                        res.TotalRecords = Convert.ToInt32(reader["Totalrecords"]);
                    }
                }
            }
            return res;
        }

        [Route("MailToBuyerForItems")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> MailToBuyerForItems()
        {
            using (AuthContext context = new AuthContext())
            {
                string From = "";
                From = AppConstants.MasterEmail;
                string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
                var res = context.Database.SqlQuery<GetBuyerMailsForItems>("EXEC dbo.getBuyerMailsForItems ").ToList();
                if (res != null && res.Any())
                {
                    var email = res.Select(t => t.Email).Distinct().ToList();
                    foreach (var r in email)
                    {
                        DataTable table = new DataTable();
                        List<GetBuyerMailsForItems> data = new List<GetBuyerMailsForItems>();
                        var datatables = new List<DataTable>();
                        if (!Directory.Exists(ExcelSavePath))
                            Directory.CreateDirectory(ExcelSavePath);

                        var x = res.Where(y => y.Email == r).ToList();
                        //int i = 1;
                        foreach (var e in x)
                        {


                            data.Add(new GetBuyerMailsForItems
                            {
                                BuyerName = e.BuyerName,
                                Email = e.Email,
                                WarehouseName = e.WarehouseName,
                                ItemName = e.ItemName,
                                ItemMultiMRPId = e.ItemMultiMRPId,
                                Number = e.Number,
                                BatchCode = e.BatchCode,
                                ExpiryDate = e.ExpiryDate,
                                APP = e.APP,
                                StockQty = e.StockQty,
                                LiveQty = e.LiveQty,
                                RemainingQty = e.RemainingQty,
                                RemainingQtyValue = e.RemainingQtyValue,

                            });

                        }
                        table = ClassToDataTable.CreateDataTable(data);
                        table.TableName = "LiveItemList";
                        datatables.Add(table);
                        //i++;

                        string filePath = ExcelSavePath + "InactiveLiveItemList" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";
                        if (ExcelGenerator.DataTable_To_Excel(datatables, filePath))
                        {

                            var sub = "Please live this clearance Inactive items";
                            var msg = "Live this Inactive items";
                            EmailHelper.SendMail(From, x[0].Email, "", sub, msg, filePath);
                        }
                    }

                }
            }
            return true;

        }


        #region Clearance DashBorad

        [Route("GetClearanceDashboardBrandList")]
        [HttpPost]
        public List<GetClearanceDashboardBrandListDc> GetClearanceDashboardBrandList(ClearanceDashboardBrandPayLoad Wids)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;

            var result = new List<GetClearanceDashboardBrandListDc>();

            List<string> roleNames = new List<string>();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();
            using (var context = new AuthContext())
            {
                if (userid > 0 && Wids != null)
                {

                    var idlist = new DataTable();
                    idlist.Columns.Add("IntValue");
                    foreach (var item in Wids.WarehouseId)
                    {

                        var dr = idlist.NewRow();
                        dr["IntValue"] = item;
                        idlist.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@Warehouseids", idlist);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";

                    result = context.Database.SqlQuery<GetClearanceDashboardBrandListDc>("Exec GetClearanceDashboardBrandList @Warehouseids", param).ToList();

                }
            }
            return result;
        }

        [Route("GetClearanceDashboardData")]
        [HttpPost]
        public List<GetClearanceDashboardDataDc> GetClearanceDashboardData(GetClearanceDashboardPayLoadDc item)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;

            var result = new List<GetClearanceDashboardDataDc>();

            List<string> roleNames = new List<string>();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            using (var context = new AuthContext())
            {
                if (userid > 0 && item != null)
                {

                    var idlist = new DataTable();
                    idlist.Columns.Add("IntValue");
                    foreach (var Data in item.BrandIds)
                    {
                        var dr = idlist.NewRow();
                        dr["IntValue"] = Data;
                        idlist.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@BrandIds", idlist);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";


                    var Widlist = new DataTable();
                    Widlist.Columns.Add("IntValue");
                    foreach (var obj in item.WarehouseIds)
                    {
                        var dr = Widlist.NewRow();
                        dr["IntValue"] = obj;
                        Widlist.Rows.Add(dr);
                    }
                    var param1 = new SqlParameter("@WarehouseIds", Widlist);
                    param1.SqlDbType = SqlDbType.Structured;
                    param1.TypeName = "dbo.IntValues";

                    var param2 = new SqlParameter("@StartDate", item.Date);
                    var param3 = new SqlParameter("@Year", item.Value);

                    context.Database.CommandTimeout = 90000;
                    result = context.Database.SqlQuery<GetClearanceDashboardDataDc>("exec GetClearanceDashboardData @WarehouseIds, @StartDate,@Year,@BrandIds", param1, param2, param3, param).ToList();


                }
            }
            return result;
        }

        [Route("ExportClearanceDashboardData")]
        [HttpPost]
        public AllExportClearanceDashboardDataDc ExportClearanceDashboardData(GetClearanceDashboardPayLoadDc obj)
        {
            int userid = 0;
            var identity = User.Identity as ClaimsIdentity;

            AllExportClearanceDashboardDataDc Result = new AllExportClearanceDashboardDataDc();

            List<string> roleNames = new List<string>();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            using (var context = new AuthContext())
            {
                if (userid > 0 && obj != null)
                {

                    var idlist = new DataTable();
                    idlist.Columns.Add("IntValue");
                    foreach (var Data in obj.BrandIds)
                    {
                        var dr = idlist.NewRow();
                        dr["IntValue"] = Data;
                        idlist.Rows.Add(dr);
                    }
                    var param = new SqlParameter("@BrandIds", idlist);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";


                    var Widlist = new DataTable();
                    Widlist.Columns.Add("IntValue");
                    foreach (var Data in obj.WarehouseIds)
                    {
                        var dr = Widlist.NewRow();
                        dr["IntValue"] = Data;
                        Widlist.Rows.Add(dr);
                    }
                    var param1 = new SqlParameter("@WarehouseIds", Widlist);
                    param1.SqlDbType = SqlDbType.Structured;
                    param1.TypeName = "dbo.IntValues";


                    var param2 = new SqlParameter("@StartDate", obj.Date);
                    var param3 = new SqlParameter("@Year", obj.Value);
                    var param4 = new SqlParameter("@Status", obj.Status);

                    if (obj.Status == "PrepareItem")
                    {
                        Result.PrepareItemExportClearanceDashboardDataDcs = context.Database.SqlQuery<PrepareItemExportClearanceDashboardDataDc>("exec ExportClearanceDashboardData @WarehouseIds, @StartDate,@Year,@Status,@BrandIds", param1, param2, param3, param4, param).ToList();

                    }
                    else if (obj.Status == "Pending")
                    {
                        Result.PendingExportClearanceDashboardDataDcs = context.Database.SqlQuery<PendingExportClearanceDashboardDataDc>("exec ExportClearanceDashboardData @WarehouseIds, @StartDate,@Year,@Status,@BrandIds", param1, param2, param3, param4, param).ToList();

                    }
                    else if (obj.Status == "Approved")
                    {
                        Result.ApprovedExportClearanceDashboardDataDcs = context.Database.SqlQuery<ApprovedExportClearanceDashboardDataDc>("exec ExportClearanceDashboardData @WarehouseIds, @StartDate,@Year,@Status,@BrandIds", param1, param2, param3, param4, param).ToList();

                    }
                    else // "Physically Moved"
                    {
                        Result.PhysicallMovedExportClearanceDashboardDataDcs = context.Database.SqlQuery<PhysicallMovedExportClearanceDashboardDataDc>("exec ExportClearanceDashboardData @WarehouseIds, @StartDate,@Year,@Status,@BrandIds", param1, param2, param3, param4, param).ToList();

                    }


                }
            }

            return Result;
        }
        #endregion

        

        

        #endregion


    }

}

