using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.PlaceOrder;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Description;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderDispatchedDetails")]
    public class OrderDispatchedDetailsController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        #region   Ready To Dispatched Process
        [Route("V1")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage addV1ReadyToDispatched(OrderDispatchedMaster postOrderDispatch)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            var result = "";

            if (postOrderDispatch != null && postOrderDispatch.DboyMobileNo != null && postOrderDispatch.OrderId > 0)
            {            
                using (var context = new AuthContext())
                {
                            ReadyToDispatchHelper helper = new ReadyToDispatchHelper();
                            result = helper.RTDSingleOrder(postOrderDispatch, compid, userid);                                      
                }
            }
            else
            {
                result = "Post Data is null";
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        //Removed by HArry, Raviji and pratik on 26dec2022
        //[Route("AutoOrderProcess")]
        //[HttpPost]
        //[Authorize]
        //public async Task<string> AutoOrderProcess(AutomateOrderProcessRequest param)
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    int compid = 0, userid = 0;
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //    var Msg = "";
        //    var postOrderDispatch = param.OrderDispatchs;
        //    People Dboyinfo = new People();
        //    using (var context = new AuthContext())
        //    {
        //        Dboyinfo = context.Peoples.Where(x => x.PeopleID == param.DeliveryBoyId).FirstOrDefault();
        //        if (Dboyinfo == null)
        //        {
        //            Msg = " Please select Delivery Boy ";
        //            return Msg;
        //        }
        //    }
        //    ReadyToDispatchHelper helper = new ReadyToDispatchHelper();
        //    List<int> InvalidOrderId = new List<int>();
        //    if (Dboyinfo != null)
        //    {
        //        ConcurrentBag<int> bag = new ConcurrentBag<int>();


        //        ParallelLoopResult parellelResult = Parallel.ForEach(postOrderDispatch, (order) =>
        //       {
        //           #region assign Dboy info
        //           order.DBoyId = param.DeliveryBoyId;
        //           order.DboyName = Dboyinfo.DisplayName;
        //           order.DboyMobileNo = Dboyinfo.Mobile;
        //           #endregion

        //           string result = helper.RTDSingleOrder(order, compid, userid);
        //           if (result == "Can't Dispatched, Something went wrong")
        //           {
        //               bag.Add(order.OrderId);
        //           }

        //       });



        //        if (parellelResult.IsCompleted && bag != null && bag.Any())
        //        {
        //            Msg = "following OrderIds : " + string.Join(", ", bag.ToList()) + " not dispatched";

        //        }
        //        else { Msg = "All order Processed successfully"; }
        //    }
        //    return Msg;
        //}

        #endregion

        #region  old code   addV1ReadyToDispatched

        ////Code Done By Atish & Harry
        //[Route("V1")]
        //[HttpPost]
        //[Authorize]
        //public HttpResponseMessage addV1ReadyToDispatched(OrderDispatchedMaster postOrderDispatch)
        //{
        //    EntitySerialManager entitySerialManager = new EntitySerialManager();
        //    var po = postOrderDispatch.orderDetails;
        //    var dm = postOrderDispatch;
        //    var result = "";

        //    // Access claims
        //    var identity = User.Identity as ClaimsIdentity;
        //    int compid = 0, userid = 0;

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
        //    //using (var dbContextTransaction = new TransactionScope())
        //    //{
        //    using (var authContext = new AuthContext())
        //    {
        //        var people = authContext.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false && x.Active).SingleOrDefault();
        //        logger.Info("addV1ReadyToDispatched");
        //        OrderMaster omCheck = new OrderMaster();
        //        int OId = Convert.ToInt32(dm.OrderId);
        //        omCheck = authContext.DbOrderMaster.Where(x => x.OrderId == OId && x.Deleted == false).Include(x => x.orderDetails).FirstOrDefault();
        //        //var Od = authContext.DbOrderDetails.Where(x=> x.i)
        //        OrderDispatchedMaster ODM = authContext.OrderDispatchedMasters.Where(x => x.OrderId == OId).FirstOrDefault();

        //        if (omCheck.Status == "Pending" && (ODM == null || ODM.OrderDispatchedMasterId == 0) && po.Sum(x => x.qty) > 0) //if order in Pending status
        //        {
        //            // int stateId = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == omCheck.WarehouseId).Stateid;
        //            dm.Status = "Ready to Dispatch";
        //            dm.CreatedDate = indianTime;
        //            dm.OrderedDate = omCheck.CreatedDate;

        //            //var invoiceNumber = entitySerialManager.GetCurrentEntityNumber(stateId, "Invoice");

        //            //dm.invoice_no = invoiceNumber;

        //            // bool isDiscount = false;
        //            double finaltotal = 0;
        //            double finalTaxAmount = 0;
        //            double finalSGSTTaxAmount = 0;
        //            double finalCGSTTaxAmount = 0;
        //            double finalGrossAmount = 0;
        //            double finalTotalTaxAmount = 0;
        //            double finalCessTaxAmount = 0;

        //            // remove c.IsFreeItem == true, Mean Freestock , False not fresss
        //            var itemids = po.Where(c => !c.IsDispatchedFreeStock).Select(x => x.ItemId).ToList();

        //            var itemslist = authContext.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();

        //            var orderdispatchQty = po.Sum(z => z.qty);//

        //            var orderDetailQtys = omCheck.orderDetails.Sum(z => z.qty);//total order qty

        //            bool isQtyNotChanged = orderDetailQtys == orderdispatchQty;

        //            var inventoryManager = new InventoryManager();
        //            var updateStockList = new List<CurrentStock>();
        //            var stockHistoryToAdd = new List<CurrentStockHistory>();


        //            //for freestock 
        //            var FreestockItemIds = po.Where(c => c.IsFreeItem && c.IsDispatchedFreeStock).Select(x => x.ItemMultiMRPId).ToList();// App Freebies  Freestock
        //            var FreestockList = authContext.FreeStockDB.Where(x => FreestockItemIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == dm.WarehouseId && x.Deleted == false).ToList(); //App Freebies  Freestock

        //            var updateFreeStockList = new List<FreeStock>();
        //            var FreestockHistoryToAdd = new List<FreeStockHistory>();


        //            //for stock 
        //            var MultiItemIds = po.Where(c =>!c.IsDispatchedFreeStock).Select(x => x.ItemMultiMRPId).ToList();////for stock
        //            var stockList = authContext.DbCurrentStock.Where(x => MultiItemIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == dm.WarehouseId && x.Deleted == false).ToList(); //for stock

        //            // var dispatchDetailsList = new List<OrderDispatchedDetails>();
        //            //orderdetail calculation





        //            foreach (OrderDispatchedDetails pc in po)
        //            {
        //                #region calculate free item qty
        //                if (pc.IsFreeItem)
        //                {
        //                    int freeitemqty = authContext.getfreebiesitem(postOrderDispatch.OrderId, pc.ItemId, po);
        //                    pc.qty = freeitemqty;
        //                }
        //                #endregion

        //                OrderItemHistory h2 = new OrderItemHistory();
        //                h2.orderid = pc.OrderId;
        //                h2.ItemId = pc.ItemId;
        //                h2.ItemName = pc.itemname;
        //                h2.Reasoncancel = pc.QtyChangeReason;
        //                h2.qty = pc.qty;
        //                h2.oldqty = pc.Noqty;
        //                authContext.OrderItemHistoryDB.Add(h2);

        //                ItemMaster items = new ItemMaster();

        //                //freestock
        //                if (pc.IsFreeItem && pc.IsDispatchedFreeStock && pc.qty > 0)
        //                {
        //                    ////write code for App FreeBies by RajKumar
        //                    FreeStock freeItem = FreestockList.FirstOrDefault(t => t.ItemNumber == pc.itemNumber && t.ItemMultiMRPId == pc.ItemMultiMRPId && t.CurrentInventory > 0);
        //                    if (freeItem != null)
        //                    {
        //                        FreeStockHistory itemHistory = new FreeStockHistory();
        //                        itemHistory.FreeStockId = freeItem.FreeStockId;
        //                        itemHistory.WarehouseId = freeItem.WarehouseId;
        //                        itemHistory.ItemMultiMRPId = freeItem.ItemMultiMRPId;
        //                        itemHistory.ItemNumber = freeItem.ItemNumber;
        //                        itemHistory.OdOrPoId = pc.OrderId;
        //                        itemHistory.ManualReason = "Dispatched on Order";
        //                        itemHistory.InventoryOut = Convert.ToInt32(pc.qty);
        //                        itemHistory.TotalInventory = Convert.ToInt32(freeItem.CurrentInventory - pc.qty);
        //                        itemHistory.itemname = freeItem.itemname;
        //                        itemHistory.MRP = freeItem.MRP;
        //                        itemHistory.Deleted = false;
        //                        itemHistory.CreationDate = indianTime;
        //                        itemHistory.userid = userid;
        //                        FreestockHistoryToAdd.Add(itemHistory);

        //                        freeItem.CurrentInventory = freeItem.CurrentInventory - pc.qty;
        //                        updateFreeStockList.Add(freeItem);
        //                    }
        //                }
        //                else
        //                {
        //                    items = itemslist.Where(x => x.ItemId == pc.ItemId).FirstOrDefault();
        //                    if (pc.qty > 0)
        //                    {
        //                        CurrentStock item = new CurrentStock();
        //                        item = stockList.Where(x =>x.WarehouseId == pc.WarehouseId && x.ItemMultiMRPId == pc.ItemMultiMRPId).FirstOrDefault();

        //                        if (item != null && item.CurrentInventory > 0 && pc.qty > 0 && item.CurrentInventory >= pc.qty)
        //                        {
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (item != null)
        //                            {
        //                                Oss.StockId = item.StockId;
        //                                Oss.ItemNumber = item.ItemNumber;
        //                                Oss.itemname = item.itemname;
        //                                Oss.OdOrPoId = pc.OrderId;
        //                                Oss.CurrentInventory = item.CurrentInventory;
        //                                Oss.InventoryOut = Convert.ToInt32(pc.qty);
        //                                Oss.TotalInventory = Convert.ToInt32(item.CurrentInventory - pc.qty);
        //                                Oss.WarehouseName = item.WarehouseName;
        //                                Oss.Warehouseid = item.WarehouseId;
        //                                Oss.CompanyId = item.CompanyId;
        //                                Oss.userid = people.PeopleID;
        //                                Oss.UserName = people.DisplayName;

        //                                Oss.ItemMultiMRPId = pc.ItemMultiMRPId;
        //                                Oss.CreationDate = indianTime;
        //                                //authContext.CurrentStockHistoryDb.Add(Oss);
        //                                stockHistoryToAdd.Add(Oss);
        //                            }
        //                            item.CurrentInventory = Convert.ToInt32(item.CurrentInventory - pc.qty);
        //                            updateStockList.Add(item);
        //                            // context.UpdateCurrentStock(item);
        //                        }
        //                        else
        //                        {
        //                            throw new Exception("Order can't dispatch due to Dispatched inventory is not available for Item: " + items.itemname);
        //                            //pc.qty = 0; //if no more inventory then set it zero qty
        //                        }
        //                    }

        //                }

        //                //if (pc.DiscountPercentage == 0)
        //                //{
        //                //    isDiscount = false;
        //                //}
        //                //else
        //                //{
        //                //    isDiscount = true;
        //                //}

        //                int MOQ = pc.MinOrderQty;
        //                pc.MinOrderQtyPrice = MOQ * pc.UnitPrice;

        //                pc.TaxPercentage = items.TotalTaxPercentage;
        //                pc.TotalCessPercentage = items.TotalCessPercentage;

        //                if (pc.TaxPercentage >= 0)
        //                {
        //                    pc.SGSTTaxPercentage = pc.TaxPercentage / 2;
        //                    pc.CGSTTaxPercentage = pc.TaxPercentage / 2;
        //                }

        //                pc.HSNCode = items.HSNCode;


        //                pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
        //                pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
        //                //if there is cess for that item

        //                if (pc.TotalCessPercentage > 0)
        //                {
        //                    pc.TotalCessPercentage = items.TotalCessPercentage;
        //                    double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

        //                    pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge / 100)) / 100;
        //                    pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
        //                    pc.CessTaxAmount = (pc.AmtWithoutAfterTaxDisc * pc.TotalCessPercentage) / 100;
        //                }


        //                double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;
        //                pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge2 / 100)) / 100;
        //                pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + pc.DiscountPercentage);
        //                pc.TaxAmmount = (pc.AmtWithoutAfterTaxDisc * pc.TaxPercentage) / 100;
        //                if (pc.TaxAmmount >= 0)
        //                {
        //                    pc.SGSTTaxAmmount = pc.TaxAmmount / 2;
        //                    pc.CGSTTaxAmmount = pc.TaxAmmount / 2;
        //                }

        //                //for cess
        //                if (pc.CessTaxAmount > 0)
        //                {
        //                    //double temp = pc.TaxPercentage + pc.TotalCessPercentage;
        //                    double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
        //                    pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
        //                    pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
        //                    pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;

        //                }
        //                else
        //                {
        //                    pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
        //                }


        //                //pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
        //                finalGrossAmount = finalGrossAmount + pc.TotalAmountAfterTaxDisc;
        //                finalTotalTaxAmount = finalTotalTaxAmount + pc.TotalAmountAfterTaxDisc;


        //                pc.DiscountAmmount = 0;
        //                pc.NetAmtAfterDis = 0;
        //                pc.Purchaseprice = items.price;
        //                pc.CreatedDate = indianTime;
        //                pc.UpdatedDate = indianTime;
        //                pc.Deleted = false;
        //                //pc.Status = "Ready to Dispatch";
        //                //dispatchDetailsList.Add(od);

        //                finaltotal = finaltotal + pc.TotalAmt;

        //                if (pc.CessTaxAmount > 0)
        //                {
        //                    finalCessTaxAmount = finalCessTaxAmount + pc.CessTaxAmount;
        //                    finalTaxAmount = finalTaxAmount + pc.TaxAmmount + pc.CessTaxAmount;
        //                }
        //                else
        //                {
        //                    finalTaxAmount = finalTaxAmount + pc.TaxAmmount;
        //                }
        //                finalSGSTTaxAmount = finalSGSTTaxAmount + pc.SGSTTaxAmmount;
        //                finalCGSTTaxAmount = finalCGSTTaxAmount + pc.CGSTTaxAmmount;

        //                var ord = authContext.DbOrderDetails.Where(r => r.OrderDetailsId == pc.OrderDetailsId).FirstOrDefault();
        //                ord.Status = "Ready to Dispatch";
        //                ord.UpdatedDate = indianTime;
        //                authContext.Entry(ord).State = EntityState.Modified;
        //            }
        //            //omCheck.invoice_no = invoiceNumber;
        //            omCheck.Status = "Ready to Dispatch";
        //            omCheck.ReadytoDispatchedDate = indianTime;
        //            omCheck.UpdatedDate = indianTime;
        //            authContext.Entry(omCheck).State = EntityState.Modified;

        //            //update OrderDispatchedMasters

        //            finaltotal = finaltotal + dm.deliveryCharge;
        //            finalGrossAmount = finalGrossAmount + dm.deliveryCharge;
        //            if (dm.WalletAmount > 0)
        //            {
        //                dm.TotalAmount = Math.Round(finaltotal, 2) - dm.WalletAmount.GetValueOrDefault();
        //                dm.TaxAmount = Math.Round(finalTaxAmount, 2);
        //                dm.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
        //                dm.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
        //                dm.GrossAmount = Math.Round((Convert.ToInt32(finalGrossAmount) - dm.WalletAmount.GetValueOrDefault()), 0);
        //            }
        //            else
        //            {
        //                dm.TotalAmount = Math.Round(finaltotal, 2);
        //                dm.TaxAmount = Math.Round(finalTaxAmount, 2);
        //                dm.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
        //                dm.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
        //                dm.GrossAmount = Math.Round((finalGrossAmount), 0);
        //            }
        //            //if (isDiscount == true)
        //            //{
        //            //    dm.DiscountAmount = finalTotalTaxAmount - finaltotal;
        //            //}
        //            //else
        //            //{
        //            //    dm.DiscountAmount = 0;
        //            //}
        //            //authContext.Entry(ODM).State = EntityState.Modified;

        //            #region Update Inventory and add Stock History In DB
        //            inventoryManager.UpdateInventory(authContext, updateStockList, stockHistoryToAdd, updateFreeStockList, FreestockHistoryToAdd);
        //            #endregion


        //            #region Order History
        //            OrderMasterHistories h1 = new OrderMasterHistories();
        //            h1.orderid = omCheck.OrderId;
        //            h1.Status = omCheck.Status;
        //            h1.Reasoncancel = po.FirstOrDefault().QtyChangeReason;
        //            h1.Warehousename = omCheck.WarehouseName;
        //            if (people.DisplayName != null)
        //            {
        //                h1.username = people.DisplayName;
        //            }
        //            else
        //            {
        //                h1.username = people.PeopleFirstName;

        //            }
        //            h1.userid = userid;
        //            h1.CreatedDate = DateTime.Now;
        //            authContext.OrderMasterHistoriesDB.Add(h1);
        //            #endregion

        //            #region Billdiscountamount

        //            double offerDiscountAmount = 0;
        //            if (!isQtyNotChanged)
        //            {
        //                var billdiscount = authContext.BillDiscountDb.Where(x => x.OrderId == omCheck.OrderId).ToList();
        //                var offerIds = billdiscount.Select(x => x.OfferId).ToList();
        //                var offers = authContext.OfferDb.Where(x => offerIds.Contains(x.OfferId)).ToList();
        //                List<int> flashdealItems = authContext.FlashDealItemConsumedDB.Where(x => x.OrderId == omCheck.OrderId).Select(x => x.ItemId).ToList();
        //                var billdiscountofferids = billdiscount.Select(x => x.OfferId).ToList();
        //                List<Offer> Offers = authContext.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.OfferItemsBillDiscounts).Include(x => x.BillDiscountOfferSections).ToList();

        //                foreach (var BillDiscount in billdiscount)
        //                {

        //                    var Offer = offers.FirstOrDefault(z => z.OfferId == BillDiscount.OfferId);

        //                    double totalamount = 0;
        //                    int OrderLineItems = 0;

        //                    if (Offer.OfferOn != "ScratchBillDiscount")
        //                    {
        //                        List<int> Itemids = new List<int>();
        //                        if (Offer.BillDiscountType == "category")
        //                        {
        //                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
        //                            var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
        //                            Itemids = itemslist.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

        //                            totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
        //                            OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
        //                        }
        //                        else if (Offer.BillDiscountType == "subcategory")
        //                        {
        //                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
        //                            AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
        //                            List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

        //                            Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

        //                            totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
        //                            OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
        //                        }
        //                        else if (Offer.BillDiscountType == "brand")
        //                        {
        //                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
        //                            AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
        //                            List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

        //                            Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

        //                            totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
        //                            OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
        //                        }
        //                        else if (Offer.BillDiscountType == "items")
        //                        {
        //                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
        //                            if (Offer.OfferItemsBillDiscounts.FirstOrDefault().IsInclude)
        //                            {
        //                                Itemids = itemslist.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
        //                            }
        //                            totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
        //                            OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
        //                        }
        //                        else
        //                        {
        //                            var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
        //                            Itemids = itemslist.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();

        //                            totalamount = Itemids.Any() ? dm.orderDetails.Where(x => !Itemids.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : dm.orderDetails.Where(x => !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
        //                            OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => !Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
        //                        }

        //                        if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
        //                        {
        //                            totalamount = Offer.MaxBillAmount;
        //                        }

        //                    }
        //                    else
        //                    {
        //                        totalamount = dm.orderDetails.Sum(x => x.qty * x.UnitPrice);
        //                        if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
        //                        {
        //                            totalamount = Offer.MaxBillAmount;
        //                        }

        //                    }

        //                    if (Offer.BillDiscountOfferOn == "Percentage")
        //                    {
        //                        BillDiscount.BillDiscountTypeValue = Offer.DiscountPercentage;
        //                        BillDiscount.BillDiscountAmount = totalamount * Offer.DiscountPercentage / 100;
        //                    }
        //                    else
        //                    {
        //                        int WalletPoint = 0;
        //                        if (Offer.WalletType == "WalletPercentage")
        //                        {
        //                            WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * ((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0) / 100)));
        //                            WalletPoint = WalletPoint * 10;
        //                        }
        //                        else
        //                        {
        //                            WalletPoint = Convert.ToInt32((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0));
        //                        }
        //                        if (Offer.ApplyOn == "PostOffer")
        //                        {
        //                            BillDiscount.BillDiscountTypeValue = WalletPoint;
        //                            BillDiscount.BillDiscountAmount = 0;
        //                            BillDiscount.IsUsedNextOrder = true;
        //                        }
        //                        else
        //                        {
        //                            BillDiscount.BillDiscountTypeValue = Offer.BillDiscountWallet;
        //                            BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10);
        //                            BillDiscount.IsUsedNextOrder = false;
        //                        }
        //                    }
        //                    if (Offer.MaxDiscount > 0)
        //                    {
        //                        var walletmultipler = 1;

        //                        if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage")
        //                        {
        //                            walletmultipler = 10;
        //                        }
        //                        if (Offer.MaxDiscount < BillDiscount.BillDiscountAmount)
        //                        {
        //                            BillDiscount.BillDiscountAmount = Offer.MaxDiscount * walletmultipler;
        //                        }
        //                        if (Offer.MaxDiscount < BillDiscount.BillDiscountTypeValue)
        //                        {
        //                            BillDiscount.BillDiscountTypeValue = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
        //                        }
        //                    }

        //                    BillDiscount.IsAddNextOrderWallet = false;
        //                    BillDiscount.ModifiedDate = indianTime;
        //                    BillDiscount.ModifiedBy = userid;
        //                    authContext.Entry(BillDiscount).State = EntityState.Modified;

        //                    offerDiscountAmount += BillDiscount.BillDiscountAmount.Value;
        //                }
        //            }
        //            else
        //                offerDiscountAmount = omCheck.BillDiscountAmount ?? 0;
        //            dm.TotalAmount = dm.TotalAmount - offerDiscountAmount;
        //            dm.BillDiscountAmount = offerDiscountAmount;
        //            dm.GrossAmount = System.Math.Round(dm.TotalAmount, 0);

        //            #endregion

        //            #region  //if Gross amount is negative due wallet amount more then dispatched amount 

        //            if ((dm.GrossAmount < 0 && dm.WalletAmount > dm.GrossAmount))
        //            {

        //                double _RefundDiffValue = Math.Abs(dm.GrossAmount);//Convert to positive
        //                var wallet = authContext.WalletDb.Where(c => c.CustomerId == dm.CustomerId).SingleOrDefault();
        //                if (_RefundDiffValue > 0 && _RefundDiffValue < dm.WalletAmount)
        //                {
        //                    double _RefundPoint = System.Math.Round((_RefundDiffValue * 10), 0);//convert to point

        //                    CustomerWalletHistory CWH = new CustomerWalletHistory();
        //                    CWH.WarehouseId = dm.WarehouseId;
        //                    CWH.CompanyId = dm.CompanyId;
        //                    CWH.CustomerId = wallet.CustomerId;
        //                    CWH.Through = "Addtional Wallet point Refunded, due to Walletamount > OrderAmount ";
        //                    CWH.NewAddedWAmount = _RefundPoint;
        //                    CWH.TotalWalletAmount = wallet.TotalAmount + _RefundPoint;
        //                    CWH.CreatedDate = indianTime;
        //                    CWH.UpdatedDate = indianTime;
        //                    CWH.OrderId = dm.OrderId;
        //                    authContext.CustomerWalletHistoryDb.Add(CWH);

        //                    //update in wallet
        //                    wallet.TotalAmount += _RefundPoint;
        //                    wallet.TransactionDate = indianTime;
        //                    authContext.Entry(wallet).State = EntityState.Modified;

        //                    dm.WalletAmount = dm.WalletAmount - _RefundDiffValue;//amount

        //                    dm.TotalAmount = 0;//

        //                    dm.GrossAmount = System.Math.Round(dm.TotalAmount, 0);
        //                }
        //            }
        //            #endregion


        //            //if there is no barcode then genearte barcode in dispatched 
        //            if (dm.InvoiceBarcodeImage == null) //byte value
        //            {
        //                string Borderid = Convert.ToString(dm.OrderId);
        //                string BorderCodeId = Borderid.PadLeft(11, '0');
        //                temOrderQBcode code = authContext.GetBarcode(BorderCodeId);
        //                dm.InvoiceBarcodeImage = code.BarcodeImage;
        //            }
        //            authContext.OrderDispatchedMasters.Add(dm);
        //            //authContext.OrderDispatchedDetailss.AddRange(dispatchDetailsList);



        //            var otherMode = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom != "Cash" && z.status == "Success").ToList();
        //            double otherModeAmt = 0;
        //            if (otherMode.Count > 0)
        //            {
        //                otherModeAmt = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom != "Cash" && z.status == "Success").Sum(x => x.amount);
        //            }




        //            if (omCheck.GrossAmount != dm.GrossAmount && otherModeAmt == 0)
        //            {
        //                var cashOldEntries = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom == "Cash"
        //                                          && z.status == "Success").ToList();

        //                if (cashOldEntries != null && cashOldEntries.Any())
        //                {
        //                    foreach (var cash in cashOldEntries)
        //                    {
        //                        cash.status = "Failed";
        //                        cash.statusDesc = "Due to Items cut when Ready to Dispatch";
        //                        authContext.Entry(cash).State = EntityState.Modified;
        //                    }
        //                }

        //                var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
        //                {
        //                    amount = dm.GrossAmount,
        //                    CreatedDate = DateTime.Now,
        //                    currencyCode = "INR",
        //                    OrderId = dm.OrderId,
        //                    PaymentFrom = "Cash",
        //                    status = "Success",
        //                    UpdatedDate = DateTime.Now,
        //                    IsRefund = false,
        //                    statusDesc = "Due to Items cut when Ready to Dispatch"
        //                };
        //                authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);

        //            }
        //            else if (omCheck.GrossAmount != dm.GrossAmount && otherModeAmt > 0)
        //            {
        //                if (omCheck.OrderType == 4)
        //                {
        //                    var gullakOldEntries = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom == "Gullak"
        //                                                  && z.status == "Success").ToList();



        //                    if (gullakOldEntries != null && gullakOldEntries.Any())
        //                    {
        //                        foreach (var cash in gullakOldEntries)
        //                        {
        //                            cash.status = "Failed";
        //                            cash.statusDesc = "Due to Items cut when Ready to Dispatch";
        //                            authContext.Entry(cash).State = EntityState.Modified;
        //                        }
        //                    }


        //                    if (omCheck.paymentMode == "Gullak")
        //                    {
        //                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
        //                        {
        //                            amount = dm.GrossAmount,
        //                            CreatedDate = DateTime.Now,
        //                            currencyCode = "INR",
        //                            OrderId = dm.OrderId,
        //                            PaymentFrom = "Gullak",
        //                            status = "Success",
        //                            UpdatedDate = DateTime.Now,
        //                            IsRefund = false,
        //                            statusDesc = "Due to Items cut when Ready to Dispatch"
        //                        };
        //                        authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);

        //                        var customerGullak = authContext.GullakDB.FirstOrDefault(x => x.CustomerId == dm.CustomerId);
        //                        if (customerGullak != null)
        //                        {
        //                            authContext.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
        //                            {
        //                                CreatedDate = indianTime,
        //                                CreatedBy = dm.CustomerId,
        //                                Comment = "Due to Items cut when Ready to Dispatch",
        //                                Amount = omCheck.GrossAmount - dm.GrossAmount,
        //                                GullakId = customerGullak.Id,
        //                                CustomerId = dm.CustomerId,
        //                                IsActive = true,
        //                                IsDeleted = false,
        //                                ObjectId = omCheck.OrderId.ToString(),
        //                                ObjectType = "Order"
        //                            });

        //                            customerGullak.TotalAmount += (omCheck.GrossAmount - dm.GrossAmount);
        //                            customerGullak.ModifiedBy = customerGullak.CustomerId;
        //                            customerGullak.ModifiedDate = indianTime;
        //                            authContext.Entry(customerGullak).State = EntityState.Modified;
        //                        }
        //                    }
        //                    else {

        //                        var cashOldEntries = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom == "Cash"
        //                                             && z.status == "Success").ToList();

        //                        if (cashOldEntries != null && cashOldEntries.Any())
        //                        {
        //                            foreach (var cash in cashOldEntries)
        //                            {
        //                                cash.status = "Failed";
        //                                cash.statusDesc = "Due to Items cut when Ready to Dispatch";
        //                                authContext.Entry(cash).State = EntityState.Modified;
        //                            }
        //                        }
        //                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
        //                        {
        //                            amount = dm.GrossAmount,
        //                            CreatedDate = DateTime.Now,
        //                            currencyCode = "INR",
        //                            OrderId = dm.OrderId,
        //                            PaymentFrom = "Cash",
        //                            status = "Success",
        //                            UpdatedDate = DateTime.Now,
        //                            IsRefund = false,
        //                            statusDesc = "Due to Items cut when Ready to Dispatch"
        //                        };
        //                        authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
        //                    }
        //                }
        //                else
        //                {

        //                    throw new Exception("Order amount and dispatch amount is different. It is not allowed in online payment.");
        //                }
        //            }


        //            if (authContext.Commit() > 0)
        //            {
        //                // dbContextTransaction.Complete();
        //                CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
        //                helper.OnDispatch(dm.OrderId, dm.CustomerId, userid);
        //            }

        //            result = "Order Ready to Dispatched Successfully";
        //        }
        //        else
        //        {
        //            // dbContextTransaction.Dispose();
        //            result = "Zero Qty Order Can't Dispatched";

        //            return Request.CreateResponse(HttpStatusCode.OK, result);
        //        }
        //        logger.Info("Dispatch in Mongo Finished ");
        //        return Request.CreateResponse(HttpStatusCode.OK, result);
        //    }
        //    //}
        //}


        ///// <summary>
        ///// Process Order Automation
        ///// </summary>
        ///// <param name="param"></param>
        ///// <returns>bool</returns>
        //[Route("AutoOrderProcess")]
        //[HttpPost]
        //[Authorize]
        //public async Task<bool> AutoOrderProcess(AutomateOrderProcessRequest param)
        //{
        //    var postOrderDispatch = param.OrderDispatchs;
        //    var identity = User.Identity as ClaimsIdentity;
        //    int compid = 0, userid = 0;
        //    List<int> InvalidOrderId = new List<int>();
        //    // Access claims
        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);



        //    bool isQtyNotChanged = true;

        //    using (var myContext = new AuthContext())
        //    {
        //        var postOrderIds = postOrderDispatch.Select(x => x.OrderId).ToList();
        //        var orderMasters = await myContext.DbOrderMaster.Where(x => postOrderIds.Contains(x.OrderId) && !x.Deleted && x.Status == "Pending").Include(x => x.orderDetails).ToListAsync();
        //        var orderDispatchMasters = myContext.OrderDispatchedMasters.Where(x => postOrderIds.Contains(x.OrderId)).Select(x => x.OrderId).ToList();
        //        var validOrders = orderDispatchMasters == null || !orderDispatchMasters.Any() ? orderMasters : orderMasters.Where(x => !orderDispatchMasters.Contains(x.OrderId)).ToList();
        //        var people = myContext.Peoples.FirstOrDefault(x => x.PeopleID == param.DeliveryBoyId);

        //        if (validOrders != null && validOrders.Any() && people != null && people.PeopleID > 0)
        //        {
        //            var validOrderDispatch = postOrderDispatch.Where(x => validOrders.Select(z => z.OrderId).Contains(x.OrderId)).ToList();
        //            var validOrderIds = validOrderDispatch.Select(x => x.OrderId).ToList();

        //            var orderdispatchQty = validOrderDispatch.Sum(x => x.orderDetails.Sum(z => z.qty));
        //            var orderDetailQtys = validOrders.Sum(x => x.orderDetails.Sum(z => z.qty));//total order qty
        //            isQtyNotChanged = orderDetailQtys == orderdispatchQty;
        //            List<int> customerIds = validOrders.Select(x => x.CustomerId).ToList();
        //            var customerGullaks = myContext.GullakDB.Where(x => customerIds.Contains(x.CustomerId));


        //            var itemIds = validOrders.SelectMany(x => x.orderDetails.Select(z => z.ItemId)).ToList();
        //            var itemslist = await myContext.itemMasters.Where(x => itemIds.Contains(x.ItemId)).ToListAsync();

        //            var result = new ConcurrentBag<OrderDispatchedMaster>();
        //            List<OrderDispatchedMaster> ordersToProcess = new List<OrderDispatchedMaster>();
        //            List<PaymentResponseRetailerApp> paymentsToInsert = new List<PaymentResponseRetailerApp>();
        //            List<Model.Gullak.GullakTransaction> gullakTransactions = new List<Model.Gullak.GullakTransaction>();

        //            var orderPaymentsToUpdate = new List<int>();

        //            validOrderDispatch.ForEach(x =>
        //            {
        //                var orderGrossAmount = orderMasters.FirstOrDefault(z => z.OrderId == x.OrderId).GrossAmount;
        //                if (orderGrossAmount != x.GrossAmount)
        //                    orderPaymentsToUpdate.Add(x.OrderId);
        //            });

        //            //List<PaymentResponseRetailerApp> paymentsToUpdate = orderPaymentsToUpdate.Any() ?
        //            //                        myContext.PaymentResponseRetailerAppDb.Where(z => orderPaymentsToUpdate.Contains(z.OrderId) && z.PaymentFrom == "Cash"
        //            //                                  && z.status == "Success").ToList() : null;

        //            var paymentsToUpdate = orderPaymentsToUpdate.Any()
        //                                    ? myContext.PaymentResponseRetailerAppDb.Where(z => orderPaymentsToUpdate.Contains(z.OrderId) && z.status == "Success").ToList()
        //                                    : null;

        //            var ordersToUpdate = orderMasters.Where(x => validOrders.Select(z => z.OrderId).Contains(x.OrderId)).ToList();

        //            var UserName = myContext.Peoples.Where(x => x.PeopleID == userid).Select(x => new { x.DisplayName, x.PeopleFirstName }).FirstOrDefault();

        //            //ParallelLoopResult loopResult = Parallel.ForEach(validOrderDispatch, (orderMaster) =>
        //            foreach (var orderMaster in validOrderDispatch)
        //            {
        //                //var order = ordersToUpdate.FirstOrDefault(x => x.OrderId == orderMaster.OrderId);

        //                orderMaster.active = true;
        //                orderMaster.Status = "Ready to Dispatch";
        //                //order.Status = "Ready to Dispatch";
        //                //order.UpdatedDate = indianTime;
        //                orderMaster.CreatedDate = indianTime;
        //                orderMaster.OrderMaster = orderMasters.FirstOrDefault(r => r.OrderId == orderMaster.OrderId);
        //                orderMaster.OrderedDate = orderMaster.OrderMaster.CreatedDate;
        //                orderMaster.DBoyId = people.PeopleID;
        //                orderMaster.DboyMobileNo = people.Mobile;
        //                orderMaster.DboyName = people.DisplayName;


        //                #region Update Payment


        //                var otherMode = paymentsToUpdate != null ? paymentsToUpdate.Where(z => z.OrderId == orderMaster.OrderId && z.PaymentFrom != "Cash" && z.status == "Success").ToList() : null;
        //                double otherModeAmt = 0;
        //                if (otherMode != null && otherMode.Count > 0)
        //                {
        //                    otherModeAmt = paymentsToUpdate.Where(z => z.OrderId == orderMaster.OrderId && z.PaymentFrom != "Cash" && z.status == "Success").Sum(x => x.amount);
        //                }


        //                if (paymentsToUpdate != null && paymentsToUpdate.Any(x => x.OrderId == orderMaster.OrderId) && orderMaster.GrossAmount != orderMaster.OrderMaster.GrossAmount && otherModeAmt == 0)
        //                {
        //                    var cashOldEntries = paymentsToUpdate.Where(z => z.OrderId == orderMaster.OrderId && z.PaymentFrom == "Cash" && z.status == "Success").ToList();

        //                    if (cashOldEntries != null && cashOldEntries.Any())
        //                    {
        //                        foreach (var cash in cashOldEntries)
        //                        {
        //                            cash.status = "Failed";
        //                            cash.statusDesc = "Due to Items cut when Ready to Dispatch from Bulk Process";
        //                            myContext.Entry(cash).State = EntityState.Modified;
        //                        }
        //                    }
        //                    var NotcashOldEntries = paymentsToUpdate.Any(z => z.PaymentFrom != "Cash" && z.status == "Success") ? paymentsToUpdate.Where(z => z.PaymentFrom != "Cash" && z.status == "Success").Sum(x => x.amount) : 0;

        //                    var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
        //                    {
        //                        amount = orderMaster.GrossAmount - NotcashOldEntries,
        //                        CreatedDate = DateTime.Now,
        //                        currencyCode = "INR",
        //                        OrderId = orderMaster.OrderId,
        //                        PaymentFrom = "Cash",
        //                        status = "Success",
        //                        UpdatedDate = DateTime.Now,
        //                        statusDesc = "Due to Items cut when Ready to Dispatch from Bulk Process"
        //                    };


        //                    paymentsToInsert.Add(PaymentResponseRetailerAppDb);
        //                }
        //                else if (orderMaster.GrossAmount != orderMaster.OrderMaster.GrossAmount && otherModeAmt > 0)
        //                {
        //                    if (orderMasters.Any(o => o.OrderId == orderMaster.OrderId) && orderMasters.FirstOrDefault(o => o.OrderId == orderMaster.OrderId).OrderType == 4)
        //                    {
        //                        if (orderMasters.FirstOrDefault(o => o.OrderId == orderMaster.OrderId).paymentMode == "Gullak")
        //                        {
        //                            var cashOldEntries = paymentsToUpdate.Where(z => z.OrderId == orderMaster.OrderId && z.PaymentFrom == "Gullak"
        //                                                      && z.status == "Success").ToList();

        //                            if (cashOldEntries != null && cashOldEntries.Any())
        //                            {
        //                                foreach (var cash in cashOldEntries)
        //                                {
        //                                    cash.status = "Failed";
        //                                    cash.statusDesc = "Due to Items cut when Ready to Dispatch from Bulk Process";
        //                                    myContext.Entry(cash).State = EntityState.Modified;
        //                                }
        //                            }

        //                            var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
        //                            {
        //                                amount = orderMaster.OrderMaster.GrossAmount,
        //                                CreatedDate = DateTime.Now,
        //                                currencyCode = "INR",
        //                                OrderId = orderMaster.OrderId,
        //                                PaymentFrom = "Gullak",
        //                                status = "Success",
        //                                UpdatedDate = DateTime.Now,
        //                                IsRefund = false,
        //                                statusDesc = "Due to Items cut when Ready to Dispatch from Bulk Process"
        //                            };
        //                            paymentsToInsert.Add(PaymentResponseRetailerAppDb);

        //                            var customerGullak = customerGullaks.FirstOrDefault(x => x.CustomerId == orderMaster.OrderMaster.CustomerId);
        //                            if (customerGullak != null)
        //                            {
        //                                gullakTransactions.Add(new Model.Gullak.GullakTransaction
        //                                {
        //                                    CreatedDate = indianTime,
        //                                    CreatedBy = orderMaster.OrderMaster.CustomerId,
        //                                    Comment = "Due to Items cut when Ready to Dispatch",
        //                                    Amount = orderMaster.GrossAmount - orderMaster.OrderMaster.GrossAmount,
        //                                    GullakId = customerGullak.Id,
        //                                    CustomerId = orderMaster.OrderMaster.CustomerId,
        //                                    IsActive = true,
        //                                    IsDeleted = false,
        //                                    ObjectId = orderMaster.OrderId.ToString(),
        //                                    ObjectType = "Order"
        //                                });

        //                                customerGullak.TotalAmount += (orderMaster.GrossAmount - orderMaster.OrderMaster.GrossAmount);
        //                                customerGullak.ModifiedBy = customerGullak.CustomerId;
        //                                customerGullak.ModifiedDate = indianTime;
        //                                myContext.Entry(customerGullak).State = EntityState.Modified;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var cashOldEntries = paymentsToUpdate.Where(z => z.OrderId == orderMaster.OrderId && z.PaymentFrom == "Cash" && z.status == "Success").ToList();

        //                            if (cashOldEntries != null && cashOldEntries.Any())
        //                            {
        //                                foreach (var cash in cashOldEntries)
        //                                {
        //                                    cash.status = "Failed";
        //                                    cash.statusDesc = "Due to Items cut when Ready to Dispatch from Bulk Process";
        //                                    myContext.Entry(cash).State = EntityState.Modified;
        //                                }
        //                            }

        //                            var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
        //                            {
        //                                amount = orderMaster.GrossAmount ,
        //                                CreatedDate = DateTime.Now,
        //                                currencyCode = "INR",
        //                                OrderId = orderMaster.OrderId,
        //                                PaymentFrom = "Cash",
        //                                status = "Success",
        //                                UpdatedDate = DateTime.Now,
        //                                statusDesc = "Due to Items cut when Ready to Dispatch from Bulk Process"
        //                            };


        //                            paymentsToInsert.Add(PaymentResponseRetailerAppDb);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        InvalidOrderId.Add(orderMaster.OrderId);
        //                    }
        //                }




        //                #endregion

        //                // bool isDiscount = false;
        //                double finaltotal = 0;
        //                double finalTaxAmount = 0;
        //                double finalSGSTTaxAmount = 0;
        //                double finalCGSTTaxAmount = 0;
        //                double finalGrossAmount = 0;
        //                double finalTotalTaxAmount = 0;
        //                double finalCessTaxAmount = 0;


        //                //for freestock 
        //                var FreestockItemIds = orderMaster.orderDetails.Where(c => c.IsFreeItem && c.IsDispatchedFreeStock).Select(x => x.ItemMultiMRPId).ToList();// App Freebies  Freestock
        //                var FreestockList = myContext.FreeStockDB.Where(x => FreestockItemIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == orderMaster.WarehouseId && x.Deleted == false).ToList(); //App Freebies  Freestock




        //                foreach (var pc in orderMaster.orderDetails)
        //                {
        //                    #region calculate free item qty
        //                    if (pc.IsFreeItem)
        //                    {
        //                        int  freeitemqty = myContext.getfreebiesitem(orderMaster.OrderId, pc.ItemId, orderMaster.orderDetails);
        //                        pc.qty = freeitemqty;
        //                    }
        //                    #endregion

        //                    OrderItemHistory h2 = new OrderItemHistory();
        //                    h2.orderid = orderMaster.OrderId;
        //                    h2.ItemId = pc.ItemId;
        //                    h2.ItemName = pc.itemname;
        //                    h2.Reasoncancel = pc.QtyChangeReason;
        //                    h2.qty = pc.qty;
        //                    h2.oldqty = pc.Noqty;
        //                    pc.OrderItemHistory = h2;



        //                        var items = itemslist.Where(x => x.ItemId == pc.ItemId).FirstOrDefault();
        //                    //freestock
        //                    if (pc.IsFreeItem && pc.IsDispatchedFreeStock && pc.qty > 0)
        //                    {
        //                        ////write code for App FreeBies by RajKumar
        //                        FreeStock freeItem = FreestockList.FirstOrDefault(t => t.ItemNumber == pc.itemNumber && t.ItemMultiMRPId == pc.ItemMultiMRPId && t.CurrentInventory > 0);
        //                        if (freeItem != null)
        //                        {
        //                            FreeStockHistory itemHistory = new FreeStockHistory();
        //                            itemHistory.FreeStockId = freeItem.FreeStockId;
        //                            itemHistory.WarehouseId = freeItem.WarehouseId;
        //                            itemHistory.ItemMultiMRPId = freeItem.ItemMultiMRPId;
        //                            itemHistory.ItemNumber = freeItem.ItemNumber;
        //                            itemHistory.OdOrPoId = pc.OrderId;
        //                            itemHistory.ManualReason = "Dispatched on Order";
        //                            itemHistory.InventoryOut = Convert.ToInt32(pc.qty);
        //                            itemHistory.TotalInventory = Convert.ToInt32(freeItem.CurrentInventory - pc.qty);
        //                            itemHistory.itemname = freeItem.itemname;
        //                            itemHistory.MRP = freeItem.MRP;
        //                            itemHistory.Deleted = false;
        //                            itemHistory.CreationDate = indianTime;
        //                            itemHistory.userid = userid;
        //                            pc.FreeCurrentStockHistory = itemHistory;
        //                            //FreestockHistoryToAdd.Add(itemHistory);

        //                            freeItem.CurrentInventory = freeItem.CurrentInventory - pc.qty;
        //                            pc.FreeStockToUpdate = freeItem;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //Move before Loop
        //                        //CurrentStock stock = new CurrentStock();

        //                        pc.StockToUpdate = myContext.DbCurrentStock.FirstOrDefault(x => x.ItemNumber == pc.itemNumber && x.WarehouseId == pc.WarehouseId && x.ItemMultiMRPId == pc.ItemMultiMRPId);

        //                        if (pc.StockToUpdate != null && pc.StockToUpdate.CurrentInventory > 0 && pc.qty > 0 && pc.StockToUpdate.CurrentInventory >= pc.qty)
        //                        {
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (pc.StockToUpdate != null)
        //                            {
        //                                Oss.StockId = pc.StockToUpdate.StockId;
        //                                Oss.ItemNumber = pc.StockToUpdate.ItemNumber;
        //                                Oss.itemname = pc.StockToUpdate.itemname;
        //                                Oss.OdOrPoId = pc.OrderId;
        //                                Oss.CurrentInventory = pc.StockToUpdate.CurrentInventory;
        //                                Oss.InventoryOut = Convert.ToInt32(pc.qty);
        //                                Oss.TotalInventory = Convert.ToInt32(pc.StockToUpdate.CurrentInventory - pc.qty);
        //                                Oss.WarehouseName = pc.StockToUpdate.WarehouseName;
        //                                Oss.Warehouseid = pc.StockToUpdate.WarehouseId;
        //                                Oss.CompanyId = pc.StockToUpdate.CompanyId;
        //                                Oss.userid = people.PeopleID;
        //                                Oss.UserName = people.DisplayName;
        //                                Oss.ItemMultiMRPId = pc.ItemMultiMRPId;
        //                                Oss.CreationDate = indianTime;
        //                                pc.CurrentStockHistory = Oss;
        //                            }
        //                            pc.StockToUpdate.CurrentInventory = Convert.ToInt32(pc.StockToUpdate.CurrentInventory - pc.qty);

        //                        }
        //                        else
        //                        {
        //                            //pc.qty = 0; //if no more inventory then set it zero qty
        //                            InvalidOrderId.Add(orderMaster.OrderId);

        //                        }
        //                    }
        //                    //if (pc.DiscountPercentage == 0)
        //                    //{
        //                    //    isDiscount = false;
        //                    //}
        //                    //else
        //                    //{
        //                    //    isDiscount = true;
        //                    //}

        //                    int MOQ = pc.MinOrderQty;
        //                    pc.MinOrderQtyPrice = MOQ * pc.UnitPrice;

        //                    pc.TaxPercentage = items.TotalTaxPercentage;
        //                    pc.TotalCessPercentage = items.TotalCessPercentage;

        //                    if (pc.TaxPercentage >= 0)
        //                    {
        //                        pc.SGSTTaxPercentage = pc.TaxPercentage / 2;
        //                        pc.CGSTTaxPercentage = pc.TaxPercentage / 2;
        //                    }

        //                    pc.HSNCode = items.HSNCode;


        //                    pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
        //                    pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
        //                    //if there is cess for that item

        //                    if (pc.TotalCessPercentage > 0)
        //                    {
        //                        pc.TotalCessPercentage = items.TotalCessPercentage;
        //                        double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

        //                        pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge / 100)) / 100;
        //                        pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
        //                        pc.CessTaxAmount = (pc.AmtWithoutAfterTaxDisc * pc.TotalCessPercentage) / 100;
        //                    }


        //                    double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;
        //                    pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge2 / 100)) / 100;
        //                    pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + pc.DiscountPercentage);
        //                    pc.TaxAmmount = (pc.AmtWithoutAfterTaxDisc * pc.TaxPercentage) / 100;
        //                    if (pc.TaxAmmount >= 0)
        //                    {
        //                        pc.SGSTTaxAmmount = pc.TaxAmmount / 2;
        //                        pc.CGSTTaxAmmount = pc.TaxAmmount / 2;
        //                    }

        //                    //for cess
        //                    if (pc.CessTaxAmount > 0)
        //                    {
        //                        //double temp = pc.TaxPercentage + pc.TotalCessPercentage;
        //                        double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
        //                        pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
        //                        pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
        //                        pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;

        //                    }
        //                    else
        //                    {
        //                        pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
        //                    }


        //                    //pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
        //                    finalGrossAmount = finalGrossAmount + pc.TotalAmountAfterTaxDisc;
        //                    finalTotalTaxAmount = finalTotalTaxAmount + pc.TotalAmountAfterTaxDisc;


        //                    pc.DiscountAmmount = 0;
        //                    pc.NetAmtAfterDis = 0;
        //                    pc.Purchaseprice = items.price;
        //                    pc.CreatedDate = indianTime;
        //                    pc.UpdatedDate = indianTime;
        //                    pc.Deleted = false;
        //                    //pc.Status = "Ready to Dispatch";
        //                    //dispatchDetailsList.Add(od);

        //                    finaltotal = finaltotal + pc.TotalAmt;

        //                    if (pc.CessTaxAmount > 0)
        //                    {
        //                        finalCessTaxAmount = finalCessTaxAmount + pc.CessTaxAmount;
        //                        finalTaxAmount = finalTaxAmount + pc.TaxAmmount + pc.CessTaxAmount;
        //                    }
        //                    else
        //                    {
        //                        finalTaxAmount = finalTaxAmount + pc.TaxAmmount;
        //                    }
        //                    finalSGSTTaxAmount = finalSGSTTaxAmount + pc.SGSTTaxAmmount;
        //                    finalCGSTTaxAmount = finalCGSTTaxAmount + pc.CGSTTaxAmmount;

        //                    var ordDet = orderMaster.OrderMaster.orderDetails.FirstOrDefault(x => x.OrderDetailsId == pc.OrderDetailsId);
        //                    ordDet.Status = "Ready to Dispatch";
        //                    ordDet.UpdatedDate = indianTime;
        //                    //authContext.Entry(ord).State = EntityState.Modified;
        //                }

        //                orderMaster.OrderMaster.Status = "Ready to Dispatch";
        //                orderMaster.OrderMaster.ReadytoDispatchedDate = indianTime;
        //                orderMaster.OrderMaster.UpdatedDate = indianTime;
        //                //authContext.Entry(omCheck).State = EntityState.Modified;

        //                //update OrderDispatchedMasters

        //                finaltotal = finaltotal + orderMaster.deliveryCharge;
        //                finalGrossAmount = finalGrossAmount + orderMaster.deliveryCharge;

        //                if (orderMaster.WalletAmount > 0)
        //                {
        //                    orderMaster.TotalAmount = Math.Round(finaltotal, 2) - orderMaster.WalletAmount.GetValueOrDefault();
        //                    orderMaster.TaxAmount = Math.Round(finalTaxAmount, 2);
        //                    orderMaster.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
        //                    orderMaster.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
        //                    orderMaster.GrossAmount = Math.Round((Convert.ToInt32(finalGrossAmount) - orderMaster.WalletAmount.GetValueOrDefault()), 0);
        //                }
        //                else
        //                {
        //                    orderMaster.TotalAmount = Math.Round(finaltotal, 2);
        //                    orderMaster.TaxAmount = Math.Round(finalTaxAmount, 2);
        //                    orderMaster.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
        //                    orderMaster.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
        //                    orderMaster.GrossAmount = Math.Round((finalGrossAmount), 0);
        //                }
        //                //if (isDiscount == true)
        //                //{
        //                //    orderMaster.DiscountAmount = finalTotalTaxAmount - finaltotal;
        //                //}
        //                //else
        //                //{
        //                //    orderMaster.DiscountAmount = 0;
        //                //}


        //                orderMaster.OrderHistory = new OrderMasterHistories();
        //                orderMaster.OrderHistory.orderid = orderMaster.OrderId;
        //                orderMaster.OrderHistory.Status = orderMaster.OrderMaster.Status;
        //                orderMaster.OrderHistory.Reasoncancel = orderMaster.orderDetails.FirstOrDefault().QtyChangeReason;
        //                orderMaster.OrderHistory.Warehousename = orderMaster.OrderMaster.WarehouseName;
        //                if (UserName.DisplayName != null)
        //                {
        //                    orderMaster.OrderHistory.username = UserName.DisplayName;
        //                }
        //                else
        //                {
        //                    orderMaster.OrderHistory.username = UserName.PeopleFirstName;

        //                }
        //                orderMaster.OrderHistory.userid = userid;
        //                orderMaster.OrderHistory.CreatedDate = DateTime.Now;

        //                double offerDiscountAmount = 0;
        //                if (!isQtyNotChanged)
        //                {
        //                    var billdiscounts = await myContext.BillDiscountDb.Where(x => validOrderIds.Contains(x.OrderId)).ToListAsync();
        //                    var offerIds = billdiscounts.Select(x => x.OfferId).ToList();
        //                    var offers = await myContext.OfferDb.Where(x => offerIds.Contains(x.OfferId)).ToListAsync();
        //                    var billdiscount = billdiscounts.Where(x => x.OrderId == orderMaster.OrderId).ToList();
        //                    List<int> flashdealItems = myContext.FlashDealItemConsumedDB.Where(x => x.OrderId == orderMaster.OrderId).Select(x => x.ItemId).ToList();
        //                    var billdiscountofferids = billdiscount.Select(x => x.OfferId).ToList();
        //                    List<Offer> Offers = myContext.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.OfferItemsBillDiscounts).Include(x => x.BillDiscountOfferSections).ToList();

        //                    foreach (var BillDiscount in billdiscount)
        //                    {

        //                        var Offer = offers.FirstOrDefault(z => z.OfferId == BillDiscount.OfferId);

        //                        double totalamount = 0;
        //                        int OrderLineItems = 0;
        //                        if (Offer.OfferOn != "ScratchBillDiscount")
        //                        {
        //                            List<int> Itemids = new List<int>();
        //                            if (Offer.BillDiscountType == "category")
        //                            {
        //                                var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
        //                                var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
        //                                Itemids = itemslist.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

        //                                totalamount = Itemids.Any() ? orderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
        //                                OrderLineItems = Itemids.Any() ? orderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : orderMaster.orderDetails.Count();
        //                            }
        //                            else if (Offer.BillDiscountType == "subcategory")
        //                            {
        //                                var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
        //                                AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
        //                                List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

        //                                Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

        //                                totalamount = Itemids.Any() ? orderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
        //                                OrderLineItems = Itemids.Any() ? orderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : orderMaster.orderDetails.Count();
        //                            }
        //                            else if (Offer.BillDiscountType == "brand")
        //                            {
        //                                var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
        //                                AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
        //                                List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

        //                                Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

        //                                totalamount = Itemids.Any() ? orderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
        //                                OrderLineItems = Itemids.Any() ? orderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : orderMaster.orderDetails.Count();
        //                            }
        //                            else if (Offer.BillDiscountType == "items")
        //                            {
        //                                var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
        //                                if (Offer.OfferItemsBillDiscounts.FirstOrDefault().IsInclude)
        //                                {
        //                                    Itemids = itemslist.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
        //                                }

        //                                totalamount = Itemids.Any() ? orderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
        //                                OrderLineItems = Itemids.Any() ? orderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : orderMaster.orderDetails.Count();
        //                            }
        //                            else
        //                            {
        //                                var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
        //                                Itemids = itemslist.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();

        //                                totalamount = Itemids.Any() ? orderMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : orderMaster.orderDetails.Where(x => !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
        //                                OrderLineItems = Itemids.Any() ? orderMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId)).Count() : orderMaster.orderDetails.Count();
        //                            }


        //                            if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
        //                            {
        //                                totalamount = Offer.MaxBillAmount;
        //                            }

        //                        }
        //                        else
        //                        {
        //                            totalamount = orderMaster.orderDetails.Sum(x => x.qty * x.UnitPrice);
        //                            if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
        //                            {
        //                                totalamount = Offer.MaxBillAmount;
        //                            }
        //                        }

        //                        if (Offer.BillDiscountOfferOn == "Percentage")
        //                        {
        //                            BillDiscount.BillDiscountTypeValue = Offer.DiscountPercentage;
        //                            BillDiscount.BillDiscountAmount = totalamount * Offer.DiscountPercentage / 100;
        //                        }
        //                        else
        //                        {
        //                            int WalletPoint = 0;
        //                            if (Offer.WalletType == "WalletPercentage")
        //                            {
        //                                WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * ((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0) / 100)));
        //                                WalletPoint = WalletPoint * 10;
        //                            }
        //                            else
        //                            {
        //                                WalletPoint = Convert.ToInt32((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0));
        //                            }
        //                            if (Offer.ApplyOn == "PostOffer")
        //                            {
        //                                BillDiscount.BillDiscountTypeValue = WalletPoint;
        //                                BillDiscount.BillDiscountAmount = 0;
        //                                BillDiscount.IsUsedNextOrder = true;
        //                            }
        //                            else
        //                            {
        //                                BillDiscount.BillDiscountTypeValue = Offer.BillDiscountWallet;
        //                                BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10);
        //                                BillDiscount.IsUsedNextOrder = false;
        //                            }
        //                        }
        //                        if (Offer.MaxDiscount > 0)
        //                        {
        //                            var walletmultipler = 1;

        //                            if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage")
        //                            {
        //                                walletmultipler = 10;
        //                            }
        //                            if (Offer.MaxDiscount < BillDiscount.BillDiscountAmount)
        //                            {
        //                                BillDiscount.BillDiscountAmount = Offer.MaxDiscount * walletmultipler;
        //                            }
        //                            if (Offer.MaxDiscount < BillDiscount.BillDiscountTypeValue)
        //                            {
        //                                BillDiscount.BillDiscountTypeValue = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
        //                            }
        //                        }

        //                        BillDiscount.IsAddNextOrderWallet = false;
        //                        BillDiscount.ModifiedDate = indianTime;
        //                        BillDiscount.ModifiedBy = userid;
        //                        myContext.Entry(BillDiscount).State = EntityState.Modified;

        //                        offerDiscountAmount += BillDiscount.BillDiscountAmount.Value;
        //                    }
        //                }
        //                else
        //                    offerDiscountAmount = orderMaster.BillDiscountAmount ?? 0;

        //                orderMaster.TotalAmount = orderMaster.TotalAmount - offerDiscountAmount;
        //                orderMaster.BillDiscountAmount = offerDiscountAmount;
        //                orderMaster.GrossAmount = System.Math.Round(orderMaster.TotalAmount, 0);
        //                ordersToProcess.Add(orderMaster);

        //            }//);

        //            //if (loopResult.IsCompleted)
        //            //    ordersToProcess = result.ToList();

        //            if (InvalidOrderId.Any())
        //                ordersToProcess = ordersToProcess.Where(x => !InvalidOrderId.Contains(x.OrderId)).ToList();

        //            #region Update Inventory and add Stock History In DB

        //            var inventoryManager = new InventoryManager();
        //            var updateStockList = ordersToProcess.SelectMany(x => x.orderDetails.Where(z => z.StockToUpdate != null).Select(z => z.StockToUpdate)).ToList();
        //            var stockHistoryToAdd = ordersToProcess.SelectMany(x => x.orderDetails.Where(z => z.CurrentStockHistory != null).Select(z => z.CurrentStockHistory)).ToList();
        //            //free stock
        //            var updateFreeStockList = ordersToProcess.SelectMany(x => x.orderDetails.Where(z => z.FreeStockToUpdate != null).Select(z => z.FreeStockToUpdate)).ToList();
        //            var FreestockHistoryToAdd = ordersToProcess.SelectMany(x => x.orderDetails.Where(z => z.FreeCurrentStockHistory != null).Select(z => z.FreeCurrentStockHistory)).ToList();

        //            inventoryManager.UpdateInventory(myContext, updateStockList, stockHistoryToAdd, updateFreeStockList, FreestockHistoryToAdd);
        //            #endregion

        //            #region Order History
        //            myContext.OrderMasterHistoriesDB.AddRange(ordersToProcess.Select(x => x.OrderHistory));
        //            var orderItemHistories = ordersToProcess.SelectMany(x => x.orderDetails.Select(z => z.OrderItemHistory)).ToList();
        //            if (orderItemHistories != null && orderItemHistories.Any())
        //                myContext.OrderItemHistoryDB.AddRange(orderItemHistories);
        //            #endregion

        //            #region OrderMaster & Details Update
        //            foreach (var item in ordersToProcess.Select(x => x.OrderMaster))
        //            {
        //                foreach (var detail in item.orderDetails)
        //                {
        //                    myContext.Entry(detail).State = EntityState.Modified;
        //                }

        //                myContext.Entry(item).State = EntityState.Modified;
        //            }


        //            #endregion

        //            #region Insert Payment if payment is Cash and Items has been changed
        //            if (paymentsToInsert != null && paymentsToInsert.Any())
        //            {
        //                myContext.PaymentResponseRetailerAppDb.AddRange(paymentsToInsert);
        //            }
        //            #endregion

        //            #region Insert Gullak Transaction Amount
        //            if (gullakTransactions != null && gullakTransactions.Any())
        //            {
        //                myContext.GullakTransactionDB.AddRange(gullakTransactions);
        //            }
        //            #endregion

        //            myContext.OrderDispatchedMasters.AddRange(ordersToProcess);


        //            if (myContext.Commit() > 0)
        //            {
        //                if (ordersToProcess.Count > 0)
        //                {

        //                    foreach (var order in ordersToProcess)
        //                    {
        //                        CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
        //                        helper.OnDispatch(order.OrderId, order.CustomerId, userid);
        //                    }
        //                }
        //            }

        //            if (InvalidOrderId.Any())
        //                throw new Exception("Order amount and dispatch amount is different. It is not allowed in online payment for OrderIds: " + string.Join(", ", InvalidOrderId));
        //        }

        //    }



        //    return true;
        //}

        #endregion


        [Authorize]
        [Route("")]
        public async Task<IEnumerable<OrderDispatchedDetails>> GetalldispatchDetailbyId(string id)
        {
            List<OrderDispatchedDetails> ass = new List<OrderDispatchedDetails>();
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    // Access claims
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

                    int idd = Int32.Parse(id);
                    ass = context.AllPOrderDispatchedDetails(idd, compid).ToList();
                    //var orderDetails = context.DbOrderDetails.Where(y => y.OrderId == idd).ToList();
                    var orderData = context.DbOrderMaster.Where(x => x.OrderId == idd).Include("orderDetails").FirstOrDefault();
                    var orderDetails = orderData.orderDetails.ToList();

                    List<Model.Store.Store> stores = new List<Model.Store.Store>();
                    if (orderDetails != null && orderDetails.Any())
                    {
                        var storeids = orderDetails.Select(x => x.StoreId).Distinct().ToList();
                        stores = context.StoreDB.Where(x => storeids.Contains(x.Id)).ToList();
                    }
                    foreach (var item in ass)
                    {
                        var orderdetail = orderDetails.FirstOrDefault(y => y.OrderDetailsId == item.OrderDetailsId);
                        item.PTR = orderdetail.PTR;
                        item.IsFreeItem = orderdetail.IsFreeItem;
                        item.Category = orderdetail.ABCClassification;
                        item.StoreName = stores.Any(x => x.Id == orderdetail.StoreId) ? stores.FirstOrDefault(x => x.Id == orderdetail.StoreId).Name : "Other";
                    }

                    if(orderData.OrderType == 10)
                    {
                        foreach (var item in ass)
                        {
                            item.UnitPrice = 0;
                        }
                    }
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderDetail " + ex.Message);
                    logger.Info("End  PurchaseOrderDetail: ");
                    return null;
                }
            }
        }

        private async Task<bool> ForNotificationCrderCancelledBeforeDispatched(int CustomerId, double? walletPointUsed)
        {
            bool res = false;
            using (AuthContext db = new AuthContext())
            {
                //Notification notification = new Notification();
                //notification.title = "Order Cancelled";
                //notification.Message = "On Order Cancelled " + walletPointUsed + " Point is added to your Wallet";
                //notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";
                //string id11 = ConfigurationManager.AppSettings["FcmApiId"];
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";

                //var objNotification = new
                //{
                //    to = customers.fcmId,
                //    notification = new
                //    {
                //        title = notification.title,
                //        body = notification.Message,
                //        icon = notification.Pic
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
                //                NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationController.FCMResponse>(responseFromFirebaseServer);
                //            }
                //        }
                //    }
                //}
                var data = new FCMData
                {
                    title = "Order Cancelled",
                    body = "On Order Cancelled " + walletPointUsed + " Point is added to your Wallet",
                    image_url = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png",
                    
                };
                var customers = db.Customers.Where(x => x.fcmId != null && x.CustomerId == CustomerId).SingleOrDefault();
                //AddNotification(notification);
                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                var result = await firebaseService.SendNotificationForApprovalAsync(customers.fcmId, data);
                if (result != null)
                {
                    return res= true;
                }
                else
                {
                    return res = false;
                }
            }
        }
    }

    public class PostOrderDispatch
    {
        public List<OrderDispatchedDetails> po { get; set; }
        public OrderDispatchedMaster dm { get; set; }
    }
}