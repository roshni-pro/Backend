using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.External.Razorpay;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using AngularJSAuthentication.Model.RazorPay;
using GenricEcommers.Models;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using AngularJSAuthentication.DataContracts.Masters.PaymentRefund;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.API.Helper.PaymentRefund;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;

using System.Configuration;
using System.IO;
using AngularJSAuthentication.API.App_Code.FinBox;
using AngularJSAuthentication.BatchManager.Publishers;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.Transaction;
using System.Data;
using LinqKit;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Helper.Notification;

namespace AngularJSAuthentication.API.Helper
{
    public class ReadyToDispatchHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        public string RTDSingleOrder(OrderDispatchedMaster postOrderDispatch, int compid, int userid)
        {
            var po = postOrderDispatch.orderDetails;
            var dm = postOrderDispatch;
            var result = "";
            double finaltotal = 0;
            double finalTaxAmount = 0;
            double finalSGSTTaxAmount = 0;
            double finalCGSTTaxAmount = 0;
            double finalGrossAmount = 0;
            double finalTotalTaxAmount = 0;
            double finalCessTaxAmount = 0;
            People people = null;
            int stateId = 0;
            OrderMaster omCheck = new OrderMaster();
            List<int> itemids = null;
            List<ItemMaster> itemslist = null;
            int OId = Convert.ToInt32(dm.OrderId);
            // Access claims
            OrderDispatchedMaster ODM = null;
            double? cashAmountRazorPay = 0;
            Warehouse warehouse = null;
            Customer CustomerData;
            using (var authContext = new AuthContext())
            {
                //if (authContext.CompanyDetailsDB.FirstOrDefault().IsAutoRTPRunning)
                //    return "Currently Auto RTP Job is Running... Please try after some time";

                var holdOrderList = authContext.ReadyToPickHoldOrders.Where(x => x.OrderId == postOrderDispatch.OrderId && x.IsActive && x.IsDeleted == false).ToList();
                if (holdOrderList != null && holdOrderList.Any(x => x.ReadyToPickDate < DateTime.Now && x.ReadyToPickDate.Date.AddDays(1).AddHours(19) > DateTime.Now))
                {
                    return "Order can't dispatch it on hold";
                }

                warehouse = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == postOrderDispatch.WarehouseId);
                if (warehouse.IsAutoRTPRunning)
                    return "Currently Auto RTP Job is Running... Please try after some time";
                if (warehouse.IsStopCurrentStockTrans)
                    return "Inventory Transactions are currently disabled for this warehouse... Please try after some time";


                var IsGstRequestPending = authContext.Database.SqlQuery<bool>("Exec CheckGSTRequestPending " + postOrderDispatch.CustomerId).FirstOrDefault();
                var data = authContext.Customers.Where(x => x.CustomerId == postOrderDispatch.CustomerId).FirstOrDefault();
                CustomerData = data;
                if (IsGstRequestPending)
                {
                    result = "Customer (" + data.Skcode + ") GST Request is Inprogress. Please coordinate with customer care.";
                    return result;
                }

                people = authContext.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Deleted == false && x.Active);
                omCheck = authContext.DbOrderMaster.Where(x => x.OrderId == OId && x.Deleted == false).Include(x => x.orderDetails).FirstOrDefault();

                stateId = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == omCheck.WarehouseId).Stateid;
                logger.Info("addV1ReadyToDispatched");
                ODM = authContext.OrderDispatchedMasters.Where(x => x.OrderId == OId).FirstOrDefault();

                if ((/*omCheck.Status == "ReadyToPick" ||*/ omCheck.Status == "Pending") && ODM == null && po != null && po.Where(x => !x.IsFreeItem).Sum(x => x.qty) > 0) //if order in Pending status
                {
                    itemids = po.Where(c => c.ItemId > 0).Select(x => x.ItemId).Distinct().ToList();
                    itemslist = authContext.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();
                }
                else
                {
                    if (po.Where(x => !x.IsFreeItem).Sum(x => x.qty) == 0)
                    {
                        result = "Zero Qty Order Can't Dispatched";

                    }
                    else
                    {
                        result = "Order Can't Dispatched due order status in : " + omCheck.Status;

                    }
                    return result;
                }
            }

            OrderOutPublisher Publisher = new OrderOutPublisher();
            List<BatchCodeSubjectDc> PublisherrtdStockList = new List<BatchCodeSubjectDc>();
            List<BatchCodeSubjectDc> OrderInvoiceQueue = new List<BatchCodeSubjectDc>();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(120);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {

                //IsolationLevel isolationLevel = Transaction.Current.IsolationLevel;
                //TimeSpan defaultTimeout = TransactionManager.DefaultTimeout;
                //TimeSpan maximumTimeout = TransactionManager.MaximumTimeout;

                using (var authContext = new AuthContext())
                {
                    int? prevto = authContext.Database.CommandTimeout;
                    authContext.Database.CommandTimeout = 120;

                    if ((/*omCheck.Status == "ReadyToPick" ||*/ omCheck.Status == "Pending") && (ODM == null) && po.Sum(x => x.qty) > 0) //if order in Pending status
                    {
                        dm.Status = "";
                        dm.CreatedDate = indianTime;
                        dm.UpdatedDate = indianTime;
                        dm.OrderedDate = omCheck.CreatedDate;
                        dm.InvoiceBarcodeImage = omCheck.InvoiceBarcodeImage;

                        var orderdispatchQty = po.Sum(z => z.qty);//
                        var orderDetailQtys = omCheck.orderDetails.Sum(z => z.qty);//total order qty
                        bool isQtyNotChanged = orderDetailQtys == orderdispatchQty;

                        List<ReadyToDispatchHelper.FreeBillItems> freeBillItems = dm.orderDetails.Select(x => new ReadyToDispatchHelper.FreeBillItems
                        {
                            ItemId = x.ItemId,
                            ItemNumber = x.itemNumber,
                            OrderdetailId = x.OrderDetailsId,
                            Qty = x.qty,
                            UnitPrice = x.UnitPrice,
                            IsFreeitem = x.IsFreeItem,
                            OfferId = omCheck.orderDetails.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? omCheck.orderDetails.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).OfferId : null
                        }).ToList();
                        foreach (OrderDispatchedDetails pc in dm.orderDetails)
                        {

                            #region calculate free item qty
                            if (!isQtyNotChanged && pc.IsFreeItem)
                            {
                                int ParentItemId = omCheck.orderDetails.FirstOrDefault(x => x.OrderDetailsId == pc.OrderDetailsId).FreeWithParentItemId ?? 0;

                                if (ParentItemId >= 0)
                                {
                                    var Parent = dm.orderDetails.FirstOrDefault(x => x.ItemId == ParentItemId && x.OrderDetailsId != pc.OrderDetailsId && !x.IsFreeItem);

                                    int TotalParentQty = Parent != null ? Parent.qty : 0;
                                    string itemnum = Parent != null ? Parent.itemNumber : "";
                                    int freeitemqty = getfreebiesitem(postOrderDispatch.OrderId, pc.ItemId, authContext, TotalParentQty, pc.OrderDetailsId, itemnum, freeBillItems);
                                    pc.qty = freeitemqty;
                                }
                            }
                            #endregion

                            int MOQ = pc.MinOrderQty;
                            pc.MinOrderQtyPrice = MOQ * pc.UnitPrice;
                            pc.TaxPercentage = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).TotalTaxPercentage : 0;
                            pc.TotalCessPercentage = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).TotalCessPercentage : 0;// items.TotalCessPercentage;
                            if (pc.TaxPercentage >= 0)
                            {
                                pc.SGSTTaxPercentage = pc.TaxPercentage / 2;
                                pc.CGSTTaxPercentage = pc.TaxPercentage / 2;
                            }
                            pc.HSNCode = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).HSNCode : null;//items.HSNCode;
                            pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
                            pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
                            //if there is cess for that item

                            if (pc.TotalCessPercentage > 0)
                            {
                                pc.TotalCessPercentage = pc.TotalCessPercentage;
                                double tempPercentagge = pc.TotalCessPercentage + pc.TaxPercentage;

                                pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge / 100)) / 100;
                                pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
                                pc.CessTaxAmount = (pc.AmtWithoutAfterTaxDisc * pc.TotalCessPercentage) / 100;
                            }
                            double tempPercentagge2 = pc.TotalCessPercentage + pc.TaxPercentage;
                            pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge2 / 100)) / 100;
                            pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + pc.DiscountPercentage);
                            pc.TaxAmmount = (pc.AmtWithoutAfterTaxDisc * pc.TaxPercentage) / 100;
                            if (pc.TaxAmmount >= 0)
                            {
                                pc.SGSTTaxAmmount = pc.TaxAmmount / 2;
                                pc.CGSTTaxAmmount = pc.TaxAmmount / 2;
                            }

                            //for cess
                            if (pc.CessTaxAmount > 0)
                            {
                                double tempPercentagge3 = pc.TotalCessPercentage + pc.TaxPercentage;
                                pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
                                pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;

                            }
                            else
                            {
                                pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
                            }


                            finalGrossAmount = finalGrossAmount + pc.TotalAmountAfterTaxDisc;
                            finalTotalTaxAmount = finalTotalTaxAmount + pc.TotalAmountAfterTaxDisc;

                            pc.DiscountAmmount = 0;
                            pc.NetAmtAfterDis = 0;
                            pc.Purchaseprice = pc.Purchaseprice;
                            pc.CreatedDate = indianTime;
                            pc.UpdatedDate = indianTime;
                            pc.Deleted = false;

                            finaltotal = finaltotal + pc.TotalAmt;

                            if (pc.CessTaxAmount > 0)
                            {
                                finalCessTaxAmount = finalCessTaxAmount + pc.CessTaxAmount;
                                finalTaxAmount = finalTaxAmount + pc.TaxAmmount + pc.CessTaxAmount;
                            }
                            else
                            {
                                finalTaxAmount = finalTaxAmount + pc.TaxAmmount;
                            }
                            finalSGSTTaxAmount = finalSGSTTaxAmount + pc.SGSTTaxAmmount;
                            finalCGSTTaxAmount = finalCGSTTaxAmount + pc.CGSTTaxAmmount;
                        }

                        foreach (var od in omCheck.orderDetails)
                        {
                            od.Status = "Ready to Dispatch";
                            od.UpdatedDate = indianTime;
                        }

                        authContext.OrderDispatchedMasters.Add(dm);
                        TextFileLogHelper.TraceLog("before 1st commit: Order id : " + dm.OrderId.ToString());
                        authContext.Commit();
                        TextFileLogHelper.TraceLog("after 1st commit: Order id : " + dm.OrderId.ToString());
                        #region stock Hit
                        //for currentstock
                        MultiStockHelper<RTDStockEntryDc> MultiStockHelpers = new MultiStockHelper<RTDStockEntryDc>();
                        List<RTDStockEntryDc> rtdStockList = new List<RTDStockEntryDc>();
                        foreach (var StockHit in dm.orderDetails.Where(x => !x.IsDispatchedFreeStock && x.qty > 0))
                        {
                            rtdStockList.Add(new RTDStockEntryDc
                            {
                                ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                OrderId = StockHit.OrderId,
                                Qty = StockHit.qty,
                                UserId = people.PeopleID,
                                WarehouseId = StockHit.WarehouseId,
                                IsFreeStock = false,
                                IsDispatchFromPlannedStock = false,
                                RefStockCode = (omCheck.OrderType == 8) ? "CL" : "C"
                            }); ;
                        }

                        if (rtdStockList.Any())
                        {
                            //bool res = MultiStockHelpers.MakeEntryDependableTransaction(rtdStockList, "Stock_OnRTD", Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete));

                            bool res = MultiStockHelpers.MakeEntry(rtdStockList, "Stock_OnRTD", authContext, dbContextTransaction);
                            if (!res)
                            {
                                TextFileLogHelper.TraceLog("before disposing and rollback : " + dm.OrderId.ToString());
                                //dbContextTransaction.Dispose();
                                TextFileLogHelper.TraceLog("after disposing and rollback : " + dm.OrderId.ToString());
                                result = "Can't Dispatched, Something went wrong";
                                return result;
                                // return Request.CreateResponse(HttpStatusCode.OK, result);
                            }

                            #region BatchCode
                            foreach (var s in dm.orderDetails.Where(x => !x.IsDispatchedFreeStock && x.qty > 0))
                            {
                                PublisherrtdStockList.Add(new BatchCodeSubjectDc
                                {
                                    ObjectDetailId = s.OrderDetailsId,
                                    ObjectId = s.OrderId,
                                    StockType = (omCheck.OrderType == 8) ? "CL" : "C",
                                    Quantity = s.qty,
                                    WarehouseId = s.WarehouseId,
                                    ItemMultiMrpId = s.ItemMultiMRPId
                                });
                            }
                            #endregion

                        }

                        //for freestock pc.IsFreeItem && pc.IsDispatchedFreeStock && pc.qty > 0
                        MultiStockHelper<RTDStockEntryDc> FreeMultiStockHelpers = new MultiStockHelper<RTDStockEntryDc>();
                        List<RTDStockEntryDc> rtdFreeStockList = new List<RTDStockEntryDc>();
                        foreach (var StockHit in dm.orderDetails.Where(x => x.IsDispatchedFreeStock && x.IsFreeItem && x.qty > 0))
                        {
                            rtdFreeStockList.Add(new RTDStockEntryDc
                            {
                                ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                OrderId = StockHit.OrderId,
                                Qty = StockHit.qty,
                                UserId = people.PeopleID,
                                WarehouseId = StockHit.WarehouseId,
                                IsFreeStock = StockHit.IsFreeItem,
                                IsDispatchFromPlannedStock = false,
                                RefStockCode = "F"
                            }); ;
                        }

                        if (rtdFreeStockList.Any())
                        {
                            bool resFreeitems = FreeMultiStockHelpers.MakeEntry(rtdFreeStockList, "Stock_OnRTD", authContext, dbContextTransaction);
                            if (!resFreeitems)
                            {
                                //dbContextTransaction.Dispose();
                                //dbContextTransaction.Dispose();
                                result = "Can't Dispatched, Something went wrong";
                                return result;
                                //return Request.CreateResponse(HttpStatusCode.OK, result);
                            }

                            #region BatchCode
                            foreach (var s in dm.orderDetails.Where(x => x.IsDispatchedFreeStock && x.IsFreeItem && x.qty > 0))
                            {
                                PublisherrtdStockList.Add(new BatchCodeSubjectDc
                                {
                                    ObjectDetailId = s.OrderDetailsId,
                                    ObjectId = s.OrderId,
                                    StockType = "F",
                                    Quantity = s.qty,
                                    WarehouseId = s.WarehouseId,
                                    ItemMultiMrpId = s.ItemMultiMRPId
                                });
                            }
                            #endregion

                        }
                        #endregion
                        //update OrderDispatchedMasters
                        string invoiceNumber = " ";
                        if (omCheck.WarehouseId != 67 && omCheck.WarehouseId != 80)
                        {
                            //PublishOrderInvoiceQueue
                            OrderInvoiceQueue.Add(new BatchCodeSubjectDc
                            {
                                ObjectDetailId = stateId,
                                ObjectId = omCheck.OrderId,
                                StockType = "",
                                Quantity = 0,
                                WarehouseId = 0,
                                ItemMultiMrpId = 0
                            });
                            //invoiceNumber = authContext.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'Invoice', " + stateId).FirstOrDefault();
                        }

                        omCheck.Status = "Ready to Dispatch";
                        omCheck.ReadytoDispatchedDate = indianTime;
                        omCheck.UpdatedDate = indianTime;
                        omCheck.invoice_no = invoiceNumber;
                        dm.invoice_no = invoiceNumber;
                        authContext.Entry(omCheck).State = EntityState.Modified;


                        finaltotal = finaltotal + dm.deliveryCharge;
                        finalGrossAmount = finalGrossAmount + dm.deliveryCharge;
                        if (dm.WalletAmount > 0)
                        {
                            dm.TotalAmount = Math.Round(finaltotal, 2) - dm.WalletAmount.GetValueOrDefault();
                            dm.TaxAmount = Math.Round(finalTaxAmount, 2);
                            dm.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
                            dm.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
                            dm.GrossAmount = Math.Round((Convert.ToInt32(finalGrossAmount) - dm.WalletAmount.GetValueOrDefault()), 0, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            dm.TotalAmount = Math.Round(finaltotal, 2);
                            dm.TaxAmount = Math.Round(finalTaxAmount, 2);
                            dm.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
                            dm.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
                            dm.GrossAmount = Math.Round((finalGrossAmount), 0, MidpointRounding.AwayFromZero);
                        }
                        #region Order History
                        OrderMasterHistories h1 = new OrderMasterHistories();
                        h1.orderid = omCheck.OrderId;
                        h1.Status = omCheck.Status;
                        h1.Reasoncancel = po.FirstOrDefault().QtyChangeReason;
                        h1.Warehousename = omCheck.WarehouseName;
                        if (people.DisplayName != null)
                        {
                            h1.username = people.DisplayName;
                        }
                        else
                        {
                            h1.username = people.PeopleFirstName;

                        }
                        h1.userid = userid;
                        h1.CreatedDate = indianTime;
                        authContext.OrderMasterHistoriesDB.Add(h1);
                        #endregion

                        #region Billdiscountamount

                        double offerDiscountAmount = 0;
                        if (!isQtyNotChanged)
                        {
                            var billdiscount = authContext.BillDiscountDb.Where(x => x.OrderId == omCheck.OrderId && x.CustomerId == omCheck.CustomerId).ToList();
                            var offerIds = billdiscount.Select(x => x.OfferId).ToList();
                            var offers = authContext.OfferDb.Where(x => offerIds.Contains(x.OfferId)).ToList();
                            List<int> flashdealItems = authContext.FlashDealItemConsumedDB.Where(x => x.OrderId == omCheck.OrderId).Select(x => x.ItemId).ToList();
                            var billdiscountofferids = billdiscount.Select(x => x.OfferId).ToList();
                            List<Offer> Offers = authContext.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.OfferItemsBillDiscounts).Include(x => x.BillDiscountOfferSections).Include(y => y.BillDiscountFreeItems).ToList();

                            foreach (var BillDiscount in billdiscount)
                            {

                                var Offer = offers.FirstOrDefault(z => z.OfferId == BillDiscount.OfferId);

                                double totalamount = 0;
                                int OrderLineItems = 0;
                                double BillDiscountamount = 0;
                                if (Offer.OfferOn != "ScratchBillDiscount" && Offer.OfferOn != "ItemMarkDown")
                                {
                                    List<int> Itemids = new List<int>();
                                    if (Offer.BillDiscountType == "category")
                                    {
                                        var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                        var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                        var ids = Offer.BillDiscountOfferSections.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                        var notids = Offer.BillDiscountOfferSections.Where(x => !x.IsInclude).Select(x => x.Id).ToList();
                                        Itemids = itemslist.Where(x => (!ids.Any() || ids.Contains(x.Categoryid)) && !notids.Contains(x.Categoryid)
                                        && !itemoutofferlist.Contains(x.ItemId)
                                        && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                        && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();


                                        //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                        //var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
                                        //Itemids = itemslist.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
                                    }
                                    else if (Offer.BillDiscountType == "subcategory")
                                    {
                                        //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                        var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                        var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                                        List<OfferBillDiscountItemDc> offerCatSubCats = authContext.Database.SqlQuery<OfferBillDiscountItemDc>("EXEC GetOfferSectionById " + Offer.OfferId).ToList();

                                        Itemids = itemslist.Where(x =>
                                                     (!offerCatSubCats.Where(y => y.IsInclude).Any() || offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                                                      && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                                                     && !itemoutofferlist.Contains(x.ItemId)
                                                     && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                                     && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        //Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
                                    }
                                    else if (Offer.BillDiscountType == "brand")
                                    {
                                        //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                        var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                        var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                                        List<OfferBillDiscountItemDc> offerCatSubCats = authContext.Database.SqlQuery<OfferBillDiscountItemDc>("EXEC GetOfferSectionById " + Offer.OfferId).ToList();


                                        Itemids = itemslist.Where(x =>
                                (
                                 !offerCatSubCats.Where(y => y.IsInclude).Any() ||
                                offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                )
                                && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();


                                        //Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
                                    }
                                    else if (Offer.BillDiscountType == "items")
                                    {
                                        //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                        //if (Offer.OfferItemsBillDiscounts.FirstOrDefault().IsInclude)
                                        //{
                                        //    Itemids = itemslist.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                        //}
                                        var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                        var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                                        Itemids = itemslist.Where(x => (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                           && !itemoutofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)
                                           ).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
                                    }
                                    else
                                    {
                                        //var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
                                        //Itemids = itemslist.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();

                                        var catIdoutofferlist = Offer.BillDiscountOfferSections.Where(x => !x.IsInclude).Select(x => x.ObjId).ToList();
                                        var catIdinofferlist = Offer.BillDiscountOfferSections.Where(x => x.IsInclude).Select(x => x.ObjId).ToList();

                                        Itemids = itemslist.Where(x => (catIdinofferlist.Any() || catIdinofferlist.Contains(x.Categoryid)) && !catIdoutofferlist.Contains(x.Categoryid) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? dm.orderDetails.Where(x => !Itemids.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : dm.orderDetails.Where(x => !flashdealItems.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                        OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => !Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
                                    }

                                    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                                    {
                                        totalamount = Offer.MaxBillAmount;
                                    }

                                    if (Offer.BillDiscountOfferOn == "FreeItem")
                                    {
                                        if (Offer.BillAmount > totalamount)
                                        {
                                            totalamount = 0;
                                        }
                                        if (Offer.LineItem > 0 && Offer.LineItem > OrderLineItems)
                                        {
                                            totalamount = 0;
                                        }
                                    }

                                }
                                else if (Offer.OfferOn == "ItemMarkDown")
                                {
                                    List<int> Itemids = new List<int>();
                                    if (Offer.BillDiscountType == "category")
                                    {
                                        //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                        //var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
                                        //Itemids = itemslist.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                        var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                                        var ids = Offer.BillDiscountOfferSections.Where(x => x.IsInclude).Select(x => x.Id).ToList();
                                        var notids = Offer.BillDiscountOfferSections.Where(x => !x.IsInclude).Select(x => x.Id).ToList();

                                        Itemids = itemslist.Where(x =>
                                        (!ids.Any() || ids.Contains(x.Categoryid)) && !notids.Contains(x.Categoryid)
                                             && !itemoutofferlist.Contains(x.ItemId)
                                             && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                        && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
                                        BillDiscountamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                        BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                    }
                                    else if (Offer.BillDiscountType == "subcategory")
                                    {
                                        //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                        List<OfferBillDiscountItemDc> offerCatSubCats = authContext.Database.SqlQuery<OfferBillDiscountItemDc>("EXEC GetOfferSectionById " + Offer.OfferId).ToList();
                                        // AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                                        //List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

                                        //Itemids = itemslist.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                        var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                                        Itemids = itemslist.Where(x =>
                                         (!offerCatSubCats.Where(y => y.IsInclude).Any() || offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                                         && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                                         && !itemoutofferlist.Contains(x.ItemId)
                                         && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                         && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
                                        BillDiscountamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                        BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                    }
                                    else if (Offer.BillDiscountType == "brand")
                                    {
                                        //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();

                                        var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                                        var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                                        List<OfferBillDiscountItemDc> offerCatSubCats = authContext.Database.SqlQuery<OfferBillDiscountItemDc>("EXEC GetOfferSectionById " + Offer.OfferId).ToList();
                                        //AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                                        //List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

                                        Itemids = itemslist.Where(x => (
                                 !offerCatSubCats.Where(y => y.IsInclude).Any() ||
                                offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                )
                                && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                                && !itemoutofferlist.Contains(x.ItemId)
                                && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                                        && !flashdealItems.Contains(x.ItemId)).Select(x => x.ItemId).ToList();

                                        totalamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                        OrderLineItems = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : dm.orderDetails.Count();
                                        BillDiscountamount = Itemids.Any() ? dm.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * (x.UnitPrice / ((100 + Convert.ToDouble(Offer.DistributorDiscountPercentage)) / 100))) : 0;
                                        BillDiscountamount = BillDiscountamount > 0 ? totalamount - BillDiscountamount : 0;
                                    }

                                }
                                else if (Offer.OfferOn == "ScratchBillDiscount" && Offer.BillDiscountOfferOn == "DynamicAmount")
                                {
                                    totalamount = dm.orderDetails.Sum(x => x.qty * x.UnitPrice);
                                    if (BillDiscount.MaxOrderAmount > 0 && totalamount > BillDiscount.MaxOrderAmount)
                                    {
                                        totalamount = BillDiscount.MaxOrderAmount;
                                    }
                                }
                                else
                                {
                                    totalamount = dm.orderDetails.Sum(x => x.qty * x.UnitPrice);
                                    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                                    {
                                        totalamount = Offer.MaxBillAmount;
                                    }

                                }

                                if (Offer.OfferOn != "ItemMarkDown")
                                {
                                    if (Offer.BillDiscountOfferOn == "Percentage")
                                    {
                                        BillDiscount.BillDiscountTypeValue = Offer.DiscountPercentage;
                                        BillDiscount.BillDiscountAmount = totalamount * Offer.DiscountPercentage / 100;
                                    }
                                    else if (Offer.BillDiscountOfferOn == "FreeItem" && totalamount == 0)
                                    {
                                        // BillDiscount.IsActive = false;
                                        //var freeItemIds = Offer.BillDiscountFreeItems.Select(x => x.ItemId).ToList();
                                        //var orderDetailIds = omCheck.orderDetails.Where(x => freeItemIds.Contains(x.ItemId) && x.IsFreeItem && !x.FreeWithParentItemId.HasValue).Select(x => x.OrderDetailsId).ToList();

                                        /////Return Free Item Stock to inventory
                                        // #region Reverse stock Hit
                                        //MultiStockHelper<RTDStockEntryDc> rtdReverseStockHelpers = new MultiStockHelper<RTDStockEntryDc>();
                                        //List<RTDStockEntryDc> rtdReverseStockList = new List<RTDStockEntryDc>();
                                        //foreach (var item in dm.orderDetails.Where(x => orderDetailIds.Contains(x.OrderDetailsId)).ToList())
                                        //{
                                        //    bool isfree = false;
                                        //    string RefStockCode = "C";
                                        //    var od = omCheck.orderDetails.Where(x => x.OrderDetailsId == item.OrderDetailsId).FirstOrDefault();

                                        //    if (od.IsFreeItem && od.IsDispatchedFreeStock)
                                        //    {
                                        //        RefStockCode = "F";
                                        //        isfree = true;
                                        //    }
                                        //    rtdReverseStockList.Add(new RTDStockEntryDc
                                        //    {
                                        //        ItemMultiMRPId = item.ItemMultiMRPId,
                                        //        OrderDispatchedDetailsId = item.OrderDispatchedDetailsId,
                                        //        OrderId = item.OrderId,
                                        //        Qty = item.qty,
                                        //        UserId = people.PeopleID,
                                        //        WarehouseId = item.WarehouseId,
                                        //        IsFreeStock = isfree,
                                        //        IsDispatchFromPlannedStock = false,
                                        //        RefStockCode = RefStockCode
                                        //    });
                                        //    item.qty = 0;
                                        //    item.Noqty = 0;
                                        //}
                                        //if (rtdReverseStockList.Any())
                                        //{
                                        //    bool res = rtdReverseStockHelpers.MakeEntry(rtdReverseStockList, "Stock_OnRTDCancel", authContext, dbContextTransaction);
                                        //    if (!res)
                                        //    {
                                        //        result = "Can't Dispatched, during reverse stock";
                                        //        return result;
                                        //    }
                                        //}
                                        //#endregion
                                    }
                                    else if (Offer.BillDiscountOfferOn == "DynamicAmount")
                                    {
                                        BillDiscount.BillDiscountAmount = BillDiscount.BillDiscountAmount;
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
                                            WalletPoint = Convert.ToInt32((BillDiscount.BillDiscountAmount.HasValue) ? BillDiscount.BillDiscountAmount : (Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0));
                                            WalletPoint = WalletPoint * 10;
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
                                            BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10);
                                            BillDiscount.IsUsedNextOrder = false;
                                        }
                                    }
                                    if (Offer.MaxDiscount > 0)
                                    {
                                        var walletmultipler = 1;

                                        if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage" && Offer.BillDiscountOfferOn != "DynamicAmount")
                                        {
                                            walletmultipler = 10;
                                        }
                                        if (Offer.BillDiscountOfferOn != "DynamicAmount")
                                        {
                                            if (Offer.MaxDiscount < BillDiscount.BillDiscountAmount)
                                            {
                                                BillDiscount.BillDiscountAmount = Offer.MaxDiscount * walletmultipler;
                                            }
                                            if (Offer.MaxDiscount < BillDiscount.BillDiscountTypeValue)
                                            {
                                                BillDiscount.BillDiscountTypeValue = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
                                            }
                                        }
                                    }
                                }
                                else if (Offer.OfferOn == "ItemMarkDown" && BillDiscountamount > 0)
                                {
                                    BillDiscount.BillDiscountTypeValue = Convert.ToDouble(Offer.DistributorDiscountPercentage);
                                    BillDiscount.BillDiscountAmount = BillDiscountamount;
                                }
                                BillDiscount.IsAddNextOrderWallet = false;
                                BillDiscount.ModifiedDate = indianTime;
                                BillDiscount.ModifiedBy = userid;
                                authContext.Entry(BillDiscount).State = EntityState.Modified;
                                offerDiscountAmount += BillDiscount.BillDiscountAmount.Value;
                            }
                        }
                        else
                            offerDiscountAmount = omCheck.BillDiscountAmount ?? 0;

                        dm.TCSAmount = omCheck.TCSAmount;
                        var Orderpayments = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId && z.status == "Success").ToList();

                        if (!isQtyNotChanged)
                        {
                            var CODcnt = Orderpayments.Count(z => z.OrderId == omCheck.OrderId && !z.IsOnline && z.status == "Success");
                            bool IsConsumer = (CustomerData.CustomerType != null && CustomerData.CustomerType.ToLower() == "consumer") ? true : false;
                            if (CODcnt > 0 && !IsConsumer)
                            {

                                //#region TCS Calculate
                                //string fy = (indianTime.Month >= 4 ? indianTime.Year + 1 : indianTime.Year).ToString();
                                //MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
                                //var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();
                                //if (tcsConfig != null)
                                //{
                                //    MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                                //    var tcsCustomer = mHelper.Select(x => x.CustomerId == omCheck.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                                //    var percent = string.IsNullOrEmpty(CustomerData.PanNo) ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                                //    dm.TCSAmount = (dm.TotalAmount - offerDiscountAmount) * percent / 100;
                                //}
                                //#endregion

                                #region TCS Calculate
                                GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                                var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(CustomerData.CustomerId, CustomerData.PanNo, authContext);
                                if (tcsConfig != null && !CustomerData.IsTCSExemption)
                                {
                                    var percent = !CustomerData.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                                    double totalamount = (tcsConfig.TotalPurchase + (tcsConfig.PendingOrderAmount - omCheck.TotalAmount) + dm.TotalAmount) - (offerDiscountAmount);
                                    if (tcsConfig.IsAlreadyTcsUsed == true)
                                    {
                                        dm.TCSAmount = (dm.TotalAmount - offerDiscountAmount) * percent / 100;
                                    }
                                    else if (totalamount > tcsConfig.TCSAmountLimit)
                                    {
                                        if (tcsConfig.TotalPurchase > tcsConfig.TCSAmountLimit)
                                        {
                                            dm.TCSAmount = (dm.TotalAmount - offerDiscountAmount) * percent / 100;
                                        }
                                        else if (tcsConfig.TotalPurchase + (tcsConfig.PendingOrderAmount - omCheck.TotalAmount) > tcsConfig.TCSAmountLimit)
                                        {
                                            dm.TCSAmount = (dm.TotalAmount - offerDiscountAmount) * percent / 100;
                                        }
                                        else
                                        {
                                            var TCSCalculatedAMT = totalamount - tcsConfig.TCSAmountLimit;
                                            if (TCSCalculatedAMT > 0)
                                            {
                                                dm.TCSAmount = (dm.TotalAmount - offerDiscountAmount) * percent / 100;
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                        }

                        dm.TotalAmount = dm.TotalAmount - offerDiscountAmount + dm.TCSAmount;
                        dm.BillDiscountAmount = offerDiscountAmount;
                        dm.GrossAmount = System.Math.Round(dm.TotalAmount, 0, MidpointRounding.AwayFromZero);

                        #endregion

                        #region  //if Gross amount is negative due wallet amount more then dispatched amount 

                        if ((dm.GrossAmount < 0 && dm.WalletAmount > dm.GrossAmount))
                        {

                            double _RefundDiffValue = Math.Abs(dm.GrossAmount);//Convert to positive
                            var wallet = authContext.WalletDb.Where(c => c.CustomerId == dm.CustomerId).SingleOrDefault();
                            if (_RefundDiffValue > 0 && _RefundDiffValue < dm.WalletAmount)
                            {
                                double _RefundPoint = System.Math.Round((_RefundDiffValue * 10), 0);//convert to point

                                CustomerWalletHistory CWH = new CustomerWalletHistory();
                                CWH.WarehouseId = dm.WarehouseId;
                                CWH.CompanyId = dm.CompanyId;
                                CWH.CustomerId = wallet.CustomerId;
                                CWH.Through = "Addtional Wallet point Refunded, due to Walletamount > OrderAmount ";
                                CWH.NewAddedWAmount = _RefundPoint;
                                CWH.TotalWalletAmount = wallet.TotalAmount + _RefundPoint;
                                CWH.CreatedDate = indianTime;
                                CWH.UpdatedDate = indianTime;
                                CWH.OrderId = dm.OrderId;
                                authContext.CustomerWalletHistoryDb.Add(CWH);

                                //update in wallet
                                wallet.TotalAmount += _RefundPoint;
                                wallet.TransactionDate = indianTime;
                                authContext.Entry(wallet).State = EntityState.Modified;

                                dm.WalletAmount = dm.WalletAmount - _RefundDiffValue;//amount

                                dm.TotalAmount = 0;//

                                dm.GrossAmount = System.Math.Round(dm.TotalAmount, 0, MidpointRounding.AwayFromZero);
                            }
                        }
                        #endregion
                        //if there is no barcode then genearte barcode in dispatched 
                        if (dm.InvoiceBarcodeImage == null) //byte value
                        {
                            string Borderid = Convert.ToString(dm.OrderId);
                            string BorderCodeId = Borderid.PadLeft(11, '0');
                            temOrderQBcode code = authContext.GetBarcode(BorderCodeId);
                            dm.InvoiceBarcodeImage = code.BarcodeImage;
                        }


                        double otherModeAmt = 0;
                        var otherMode = Orderpayments.Where(z => z.OrderId == omCheck.OrderId && z.IsOnline && z.status == "Success").ToList();
                        if (otherMode.Count > 0)
                        {
                            otherModeAmt = otherMode.Sum(x => x.amount);
                        }


                        if (omCheck.GrossAmount != dm.GrossAmount && otherModeAmt == 0)
                        {
                            var cashOldEntries = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId && !z.IsOnline
                                                    && z.status == "Success").ToList();

                            if (cashOldEntries != null && cashOldEntries.Any())
                            {
                                foreach (var cash in cashOldEntries)
                                {
                                    cash.status = "Failed";
                                    cash.statusDesc = "Due to Items cut when Ready to Dispatch";
                                    authContext.Entry(cash).State = EntityState.Modified;
                                }
                            }
                            var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp();
                            if (!cashOldEntries.Any(x => x.PaymentFrom == "RTGS/NEFT"))
                            {
                                PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                {
                                    amount = dm.GrossAmount,
                                    CreatedDate = indianTime,
                                    currencyCode = "INR",
                                    OrderId = dm.OrderId,
                                    PaymentFrom = "Cash",
                                    status = "Success",
                                    UpdatedDate = indianTime,
                                    IsRefund = false,
                                    statusDesc = "Due to Items cut when Ready to Dispatch"
                                };
                            }
                            else
                            {
                                PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                {
                                    amount = dm.GrossAmount,
                                    CreatedDate = indianTime,
                                    currencyCode = "INR",
                                    OrderId = dm.OrderId,
                                    PaymentFrom = "RTGS/NEFT",
                                    status = "Success",
                                    UpdatedDate = indianTime,
                                    IsRefund = false,
                                    statusDesc = "Due to Items cut when Ready to Dispatch"
                                };
                            }
                            authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                            cashAmountRazorPay = dm.GrossAmount;
                        }
                        else if (omCheck.GrossAmount != dm.GrossAmount && otherModeAmt > 0)
                        {
                            if (omCheck.paymentThrough.Trim().ToLower() == "paylater")
                            {
                                var paymentresponse = authContext.PaymentResponseRetailerAppDb.Where(x => x.OrderId == omCheck.OrderId && x.statusDesc == "Order Place").FirstOrDefault();
                                var paylaterdata = authContext.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == omCheck.OrderId);
                                if (paylaterdata != null)
                                {
                                    var PaymentResponseRetailerAppdb = new PaymentResponseRetailerApp
                                    {
                                        amount = (-1) * (paylaterdata.Amount - dm.GrossAmount),
                                        CreatedDate = DateTime.Now,
                                        currencyCode = "INR",
                                        OrderId = paylaterdata.OrderId,
                                        PaymentFrom = "PayLater",
                                        GatewayTransId = paymentresponse != null ? paymentresponse.GatewayTransId : "",
                                        GatewayOrderId = Convert.ToString(paylaterdata.OrderId),
                                        status = "Success",
                                        UpdatedDate = DateTime.Now,
                                        IsRefund = false,
                                        IsOnline = true,
                                        statusDesc = "Manual Cut & Dispatch"
                                    };
                                    authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppdb);

                                    paylaterdata.Amount = dm.GrossAmount;
                                    paylaterdata.ModifiedBy = userid;
                                    paylaterdata.ModifiedDate = DateTime.Now;
                                    authContext.Entry(paylaterdata).State = EntityState.Modified;

                                    var retailpay = authContext.PayLaterCollectionHistoryDb.FirstOrDefault(x => x.PayLaterCollectionId == paylaterdata.Id && x.IsActive == true && x.IsDeleted == false && x.PaymentStatus == 1 && x.Comment == "Retailer Pay Now");
                                    if (retailpay != null)
                                    {
                                        if (retailpay.Amount > paylaterdata.Amount)
                                        {
                                            double refundamount = 0;
                                            refundamount = retailpay.Amount - paylaterdata.Amount;
                                            PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                            var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                            {
                                                amount = (-1) * (refundamount),
                                                CreatedDate = DateTime.Now,
                                                currencyCode = "INR",
                                                OrderId = paylaterdata.OrderId,
                                                PaymentFrom = retailpay.PaymentMode,
                                                GatewayTransId = retailpay.RefNo,
                                                GatewayOrderId = Convert.ToString(paylaterdata.OrderId),
                                                status = "Success",
                                                UpdatedDate = DateTime.Now,
                                                IsRefund = false,
                                                IsOnline = true,
                                                statusDesc = "Refund Initiated"
                                            };
                                            authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                            var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                            {
                                                Amount = (refundamount),
                                                OrderId = PaymentResponseRetailerAppDb.OrderId,
                                                Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                                ReqGatewayTransId = PaymentResponseRetailerAppDb.GatewayTransId,
                                                Status = (int)PaymentRefundEnum.Initiated,
                                                CreatedBy = userid,
                                                CreatedDate = DateTime.Now,
                                                IsActive = true,
                                                IsDeleted = false,
                                                ModifiedBy = userid,
                                                ModifiedDate = DateTime.Now,
                                                PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                            };
                                            bool IsInserted = PRHelper.InsertPaymentRefundRequest(authContext, PaymentRefundRequestDc);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (warehouse.IsOnlineRefundEnabled)
                                {
                                    #region Cut line item Payment refund  -- April2022
                                    var RefundDays = authContext.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();
                                    // case 1:  Online payment is more than order payment for payment mode :  (chqbook)
                                    if (dm.GrossAmount < Orderpayments.Where(z => z.OrderId == omCheck.OrderId && (z.PaymentFrom == "chqbook") && z.IsOnline && z.status == "Success").Sum(z => z.amount))
                                    {
                                        //result = "Can't dispatched due to online payment is more than dispatched amount for Order  : " + omCheck.OrderId;
                                        result = "Partial cut is not allowed in Chqbook payment order : " + omCheck.OrderId;
                                        return result;
                                    }
                                    // case 2 : failed cash payment if exists
                                    var OldCashEntries = Orderpayments.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom == "Cash" && z.status == "Success").ToList();
                                    if (OldCashEntries != null && OldCashEntries.Any())
                                    {
                                        foreach (var cash in OldCashEntries)
                                        {
                                            cash.status = "Failed";
                                            cash.statusDesc = "Due to Items cut when Ready to Dispatch";
                                            authContext.Entry(cash).State = EntityState.Modified;
                                        }
                                    }
                                    //case 3 : online payment list
                                    var OnlineEntries = Orderpayments.Where(z => z.OrderId == omCheck.OrderId && z.IsOnline && z.status == "Success").ToList();
                                    if (OnlineEntries != null && OnlineEntries.Any() && OnlineEntries.Sum(x => x.amount) > dm.GrossAmount)
                                    {
                                        double NetRefundAmount = OnlineEntries.Sum(x => x.amount) - dm.GrossAmount;// Calculate Net total refund amount

                                        PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                        foreach (var item in OnlineEntries.OrderBy(c => c.RefundPriority).OrderByDescending(c => c.id))
                                        {
                                            var PaymentRefundDays = RefundDays.FirstOrDefault(x => x.ApiName.Trim().ToLower() == item.PaymentFrom.Trim().ToLower());
                                            if (PaymentRefundDays != null && PaymentRefundDays.DaysForRefundEligible > 0 && item.CreatedDate.AddDays(PaymentRefundDays.DaysForRefundEligible) < indianTime)
                                            {
                                                result = "Can't dispatch cut item  order , because online payment refund days expired for Order  : " + omCheck.OrderId;
                                                dbContextTransaction.Dispose();
                                                return result;
                                            }
                                            else if (PaymentRefundDays == null && item.PaymentFrom.Trim().ToLower() != "gullak")
                                            {
                                                result = "refund apis or refund days not configured for payment mode " + item.PaymentFrom;
                                                return result;
                                            }

                                            double sourceAmount = item.amount;
                                            double RefundAmount = NetRefundAmount - sourceAmount > 0 ? sourceAmount : NetRefundAmount;
                                            if (RefundAmount > 0 && NetRefundAmount > 0)
                                            {
                                                var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                                {
                                                    amount = (-1) * RefundAmount,
                                                    CreatedDate = indianTime,
                                                    currencyCode = "INR",
                                                    OrderId = dm.OrderId,
                                                    PaymentFrom = item.PaymentFrom,
                                                    GatewayTransId = item.GatewayTransId,
                                                    GatewayOrderId = item.GatewayOrderId,
                                                    status = "Success",
                                                    UpdatedDate = indianTime,
                                                    IsRefund = false,
                                                    IsOnline = true,
                                                    statusDesc = "Refund Initiated"
                                                };
                                                authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                                authContext.Commit();
                                                // addd Refund request
                                                var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                                {
                                                    Amount = RefundAmount,
                                                    OrderId = PaymentResponseRetailerAppDb.OrderId,
                                                    Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                                    Status = (int)PaymentRefundEnum.Initiated,
                                                    ReqGatewayTransId = item.GatewayTransId,
                                                    CreatedBy = userid,
                                                    CreatedDate = indianTime,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    ModifiedBy = userid,
                                                    ModifiedDate = indianTime,
                                                    RefundType = (int)RefundTypeEnum.Auto,
                                                    PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                                };
                                                bool IsInserted = PRHelper.InsertPaymentRefundRequest(authContext, PaymentRefundRequestDc);
                                                NetRefundAmount -= RefundAmount;
                                            }
                                        }

                                    }

                                    //case 2 : add remaing amount in cash payment if online amount is less than dispatchedAmount 
                                    if (OnlineEntries != null && OnlineEntries.Sum(x => x.amount) < dm.GrossAmount)
                                    {
                                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                        {
                                            amount = dm.GrossAmount - OnlineEntries.Sum(x => x.amount),
                                            CreatedDate = indianTime,
                                            currencyCode = "INR",
                                            OrderId = dm.OrderId,
                                            PaymentFrom = "Cash",
                                            status = "Success",
                                            UpdatedDate = indianTime,
                                            IsRefund = false,
                                            statusDesc = "Due to Items cut when Ready to Dispatch"
                                        };
                                        authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region old code and Gullak
                                    if (otherMode.Any(x => x.PaymentFrom == "Gullak"))
                                    {
                                        var gullakOldEntries = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId //&& z.PaymentFrom == "Gullak"
                                                                      && z.status == "Success").ToList();
                                        if (gullakOldEntries != null && gullakOldEntries.Any())
                                        {
                                            foreach (var cash in gullakOldEntries)
                                            {
                                                cash.status = "Failed";
                                                cash.statusDesc = "Due to Items cut when Ready to Dispatch";
                                                authContext.Entry(cash).State = EntityState.Modified;
                                            }
                                        }
                                        List<PaymentResponseRetailerApp> GullakPaymentResponseRetailerAppDbs = new List<PaymentResponseRetailerApp>();
                                        var oldgullak = gullakOldEntries.FirstOrDefault(z => z.PaymentFrom == "Gullak");
                                        double RemainingGullakAmount = 0;
                                        double gullakAmount = 0;
                                        double cashAmount = 0;
                                        if (oldgullak != null)
                                        {
                                            oldgullak.amount = gullakOldEntries.Where(z => z.PaymentFrom == "Gullak").Sum(x => x.amount);
                                            if (dm.GrossAmount >= oldgullak.amount)
                                            {
                                                RemainingGullakAmount = 0;
                                                gullakAmount = oldgullak.amount;
                                                cashAmount = dm.GrossAmount - gullakAmount;
                                            }
                                            else if (dm.GrossAmount < oldgullak.amount)
                                            {
                                                RemainingGullakAmount = oldgullak.amount - dm.GrossAmount;
                                                gullakAmount = dm.GrossAmount;
                                            }
                                            //GullakPaymentResponseRetailerAppDbs = new List<PaymentResponseRetailerApp>();
                                            GullakPaymentResponseRetailerAppDbs.Add(
                                                 new PaymentResponseRetailerApp
                                                 {
                                                     amount = gullakAmount,
                                                     CreatedDate = indianTime,
                                                     currencyCode = "INR",
                                                     OrderId = dm.OrderId,
                                                     PaymentFrom = "Gullak",
                                                     status = "Success",
                                                     UpdatedDate = indianTime,
                                                     IsRefund = false,
                                                     IsOnline = true,
                                                     statusDesc = "Due to Items cut when Ready to Dispatch"
                                                 });
                                        }
                                        else
                                        {
                                            cashAmount = dm.GrossAmount;
                                        }
                                        if (cashAmount > 0)
                                        {
                                            GullakPaymentResponseRetailerAppDbs.Add(
                                                new PaymentResponseRetailerApp
                                                {
                                                    amount = cashAmount,
                                                    CreatedDate = indianTime,
                                                    currencyCode = "INR",
                                                    OrderId = dm.OrderId,
                                                    PaymentFrom = "Cash",
                                                    status = "Success",
                                                    UpdatedDate = indianTime,
                                                    IsRefund = false,
                                                    statusDesc = "Due to Items cut when Ready to Dispatch"
                                                });
                                        }
                                        authContext.PaymentResponseRetailerAppDb.AddRange(GullakPaymentResponseRetailerAppDbs);
                                        if (RemainingGullakAmount > 0)
                                        {
                                            var customerGullak = authContext.GullakDB.FirstOrDefault(x => x.CustomerId == dm.CustomerId);
                                            if (customerGullak != null)
                                            {
                                                authContext.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                                                {
                                                    CreatedDate = indianTime,
                                                    CreatedBy = dm.CustomerId,
                                                    Comment = "Items cut : " + omCheck.OrderId.ToString(),
                                                    Amount = RemainingGullakAmount,
                                                    GullakId = customerGullak.Id,
                                                    CustomerId = dm.CustomerId,
                                                    IsActive = true,
                                                    IsDeleted = false,
                                                    ObjectId = omCheck.OrderId.ToString(),
                                                    ObjectType = "Order"
                                                });
                                                customerGullak.TotalAmount += RemainingGullakAmount;
                                                customerGullak.ModifiedBy = customerGullak.CustomerId;
                                                customerGullak.ModifiedDate = indianTime;
                                                authContext.Entry(customerGullak).State = EntityState.Modified;
                                            }
                                        }
                                    }
                                    else if (otherMode.All(x => !x.IsOnline))
                                    {
                                        var cashOldEntries = authContext.PaymentResponseRetailerAppDb.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom == "Cash"
                                                             && z.status == "Success").ToList();
                                        if (cashOldEntries != null && cashOldEntries.Any())
                                        {
                                            foreach (var cash in cashOldEntries)
                                            {
                                                cash.status = "Failed";
                                                cash.statusDesc = "Due to Items cut when Ready to Dispatch";
                                                authContext.Entry(cash).State = EntityState.Modified;
                                            }
                                        }
                                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                        {
                                            amount = dm.GrossAmount,
                                            CreatedDate = indianTime,
                                            currencyCode = "INR",
                                            OrderId = dm.OrderId,
                                            PaymentFrom = "Cash",
                                            status = "Success",
                                            UpdatedDate = indianTime,
                                            IsRefund = false,
                                            statusDesc = "Due to Items cut when Ready to Dispatch"
                                        };
                                        authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                    }
                                    else
                                    {
                                        result = "Order amount and dispatch amount is different.It is not allowed in online payment.";
                                        return result;
                                    }
                                    #endregion
                                }
                            }

                        }

                        ///chanage for ledger purpose
                        dm.Status = "Ready to Dispatch";
                        dm.invoice_no = invoiceNumber;
                        dm.UpdatedDate = indianTime;

                        #region Update IRN Check 
                        IRNHelper irnHelper = new IRNHelper();

                        if (irnHelper.IsGenerateIRN(authContext, dm.CustomerId))
                        {
                            dm.IsGenerateIRN = true;

                            #region ClearTaxIntegrations
                            ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration();
                            clearTaxIntegration.OrderId = dm.OrderId;
                            clearTaxIntegration.IsActive = true;
                            clearTaxIntegration.CreateDate = indianTime;
                            clearTaxIntegration.IsProcessed = false;
                            clearTaxIntegration.APIType = "GenerateIRN";
                            authContext.ClearTaxIntegrations.Add(clearTaxIntegration);
                            #endregion

                        }

                        #endregion

                        if (dm.GrossAmount <= 0)
                        {
                            result = "Order Can't Dispatched due to invoice value on dispatched will be zero or negative";
                            return result;
                        }
                        authContext.Entry(dm).State = EntityState.Modified;

                        #region if no entry of payment then insert entry in cash 
                        if (!authContext.PaymentResponseRetailerAppDb.Any(x => x.OrderId == dm.OrderId && x.status == "Success"))
                        {
                            var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                            {
                                amount = dm.GrossAmount,
                                CreatedDate = indianTime,
                                currencyCode = "INR",
                                OrderId = dm.OrderId,
                                PaymentFrom = "Cash",
                                status = "Success",
                                UpdatedDate = indianTime,
                                IsRefund = false,
                                statusDesc = "OnRTD"
                            };
                            authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                        }
                        #endregion


                        if (authContext.Commit() > 0)
                        {
                            TextFileLogHelper.TraceLog("after 2nd commit  : " + dm.OrderId.ToString());
                            CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
                            helper.OnDispatch(dm.OrderId, dm.CustomerId, userid);
                            try
                            {
                                PickerCustomerDc cust = authContext.Customers.Where(v => v.CustomerId == dm.CustomerId).Select(v => new PickerCustomerDc { CustomerId = v.CustomerId, Skcode = v.Skcode, IsGstRequestPending = false, FcmId = v.fcmId }).FirstOrDefault();

                                if (authContext.PaymentResponseRetailerAppDb.Any(x => x.OrderId == dm.OrderId && x.status == "Success" && x.PaymentFrom == "RTGS/NEFT" && string.IsNullOrEmpty(x.GatewayTransId)) && cust != null)
                                {
                                    #region VAN notification
                                    VANForNotification(dm.OrderId, dm.GrossAmount, cust);
                                }
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message);
                            }


                            dbContextTransaction.Complete();




                            TextFileLogHelper.TraceLog("after trans complete  : " + dm.OrderId.ToString());
                            result = "Order Ready to Dispatched Successfully";
                            #region FY Parchase Calculate
                            MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                            string fy = (indianTime.Month >= 4 ? indianTime.Year + 1 : indianTime.Year).ToString();
                            var tcsCustomer = mHelper.Select(x => x.CustomerId == dm.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                            if (tcsCustomer != null)
                            {
                                tcsCustomer.TotalPurchase += dm.GrossAmount;
                                tcsCustomer.LastUpdatedDate = indianTime;
                                mHelper.ReplaceWithoutFind(tcsCustomer.Id, tcsCustomer, "TCSCustomer");
                            }
                            else
                            {
                                tcsCustomer = new TCSCustomer
                                {
                                    CustomerId = dm.CustomerId,
                                    FinancialYear = fy,
                                    LastUpdatedDate = indianTime,
                                    TotalPurchase = dm.GrossAmount
                                };
                                mHelper.Insert(tcsCustomer);
                            }

                            #endregion

                            #region Insert in FIFO

                            if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                            {
                                List<OutDc> items = dm.orderDetails.Where(x => x.qty > 0).Select(x => new OutDc
                                {
                                    ItemMultiMrpId = x.ItemMultiMRPId,
                                    WarehouseId = dm.WarehouseId,
                                    Destination = "Sale",
                                    CreatedDate = indianTime,
                                    ObjectId = x.OrderId,
                                    Qty = x.qty,
                                    SellingPrice = x.UnitPrice,

                                }).ToList();

                                foreach (var item in items)
                                {
                                    RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                    rabbitMqHelper.Publish("RTD", item);
                                }
                            }

                            #endregion
                        }
                    }
                    else
                    {

                        //dbContextTransaction.Dispose();
                        result = "Zero Qty Order Can't Dispatched";
                        return result;
                    }

                }
            }
            if (PublisherrtdStockList != null && PublisherrtdStockList.Any() && result == "Order Ready to Dispatched Successfully")
            {
                //AsyncContext.Run(() => Publisher.PublishOrderOut(PublisherrtdStockList));
                Publisher.PublishOrderInvoiceQueue(OrderInvoiceQueue);
                Publisher.PublishOrderOut(PublisherrtdStockList);

            }

            logger.Info("Dispatch in Mongo Finished ");
            return result;

        }

        public int getfreebiesitem(int OrderId, int ItemId, AuthContext authContext, int ParentTotalQty, int OrderDetailsId, string itemnumber, List<FreeBillItems> freeBillItems)
        {
            var offerdata = (from offitem in authContext.OfferItemDb
                             join off in authContext.OfferDb
                             on offitem.ReferOfferId equals off.OfferId
                             where offitem.OrderId == OrderId && offitem.FreeItemId == ItemId && offitem.OrderDetailsId == OrderDetailsId
                             select new GetFreebiesInfo
                             {
                                 itemId = offitem.itemId,
                                 MinOrderQuantity = offitem.MinOrderQuantity,
                                 FreeItemId = offitem.FreeItemId,
                                 NoOffreeQuantity = off.NoOffreeQuantity,
                                 OfferType = offitem.OfferType,
                                 BillDisountNoOffreeQuantity = offitem.NoOffreeQuantity,
                                 IsBillDiscountFreebiesItem = off.IsBillDiscountFreebiesItem,
                                 OfferId = off.OfferId,
                                 IsBillDiscountFreebiesValue = off.IsBillDiscountFreebiesValue,
                                 billamount = off.BillAmount,
                                 MaxBillAmount = off.MaxBillAmount,
                                 OfferOn = off.OfferOn
                             }).FirstOrDefault();
            if (offerdata != null && offerdata.OfferType != "BillDiscount_FreeItem")
            {
                int multiply = Convert.ToInt32(ParentTotalQty / offerdata.MinOrderQuantity);
                int totalquantity = multiply * offerdata.NoOffreeQuantity;
                return totalquantity;
            }
            else if (offerdata != null && offerdata.OfferType == "BillDiscount_FreeItem")
            {

                if (offerdata.IsBillDiscountFreebiesItem)
                {
                    if (offerdata.OfferId > 0)
                    {
                        if (freeBillItems != null && freeBillItems.Any())
                        {
                            int totalQty = freeBillItems.Where(x => x.ItemNumber == itemnumber && x.OrderdetailId != OrderDetailsId && (!x.OfferId.HasValue || x.OfferId <= 0)).Sum(x => x.Qty);
                            var minorderdata = authContext.OfferDb.Where(x => x.OfferId == offerdata.OfferId).FirstOrDefault();
                            var freequantitydata = authContext.BillDiscountFreeItem.Where(x => x.offerId == offerdata.OfferId && x.ItemId == ItemId).FirstOrDefault();
                            if (minorderdata != null && minorderdata.MinOrderQuantity > 0 && freequantitydata != null && freequantitydata.Qty > 0)
                            {
                                int multiply = Convert.ToInt32(totalQty / minorderdata.MinOrderQuantity);
                                int totalquantity = multiply * freequantitydata.Qty;
                                return totalquantity;
                            }
                        }

                        return offerdata.BillDisountNoOffreeQuantity;
                    }
                    return offerdata.BillDisountNoOffreeQuantity;
                }
                else if (offerdata.IsBillDiscountFreebiesValue)
                {
                    var freequantitydata = authContext.BillDiscountFreeItem.Where(x => x.offerId == offerdata.OfferId && x.ItemId == ItemId).FirstOrDefault();
                    if (offerdata.OfferId > 0)
                    {
                        double billDisitemValue = 0;
                        var offerId = new SqlParameter
                        {
                            ParameterName = "offerId",
                            Value = offerdata.OfferId
                        };

                        List<int> itemids = authContext.Database.SqlQuery<int>("exec GetOfferforbilldiscount  @offerId", offerId).ToList();
                        int freeqty = 0;
                        if (freeBillItems != null && freeBillItems.Any())
                        {
                            billDisitemValue = freeBillItems.Where(x => itemids.Contains(x.ItemId) && !x.IsFreeitem).Sum(x => x.Qty * x.UnitPrice);
                            if (billDisitemValue >= offerdata.MaxBillAmount && offerdata.MaxBillAmount > 0)
                            {
                                billDisitemValue = offerdata.MaxBillAmount;
                            }
                            int multiple = Convert.ToInt32(Convert.ToInt32(billDisitemValue) / Convert.ToInt32(offerdata.billamount));
                            freeqty = multiple * freequantitydata.Qty;
                            return freeqty;
                        }
                    }
                    return offerdata.BillDisountNoOffreeQuantity;
                }
                else
                {
                    return offerdata.BillDisountNoOffreeQuantity;
                }

            }
            else
            {
                return 0;
            }

        }

        //public int getBillDiscountfreebiesitem(int OrderId, int ItemId, AuthContext authContext, int OrderDetailsId, string itemnumber, List<FreeBillItems> freeBillItems)
        //{
        //    var offerdata = (from offitem in authContext.OfferItemDb
        //                     join off in authContext.OfferDb
        //                     on offitem.ReferOfferId equals off.OfferId
        //                     where offitem.OrderId == OrderId && offitem.FreeItemId == ItemId && offitem.OrderDetailsId == OrderDetailsId
        //                     select new GetFreebiesInfo
        //                     {
        //                         itemId = offitem.itemId,
        //                         MinOrderQuantity = offitem.MinOrderQuantity,
        //                         FreeItemId = offitem.FreeItemId,
        //                         NoOffreeQuantity = off.NoOffreeQuantity,
        //                         OfferType = offitem.OfferType,
        //                         BillDisountNoOffreeQuantity = offitem.NoOffreeQuantity,
        //                         IsBillDiscountFreebiesItem = off.IsBillDiscountFreebiesItem,
        //                         OfferId = off.OfferId,
        //                         IsBillDiscountFreebiesValue = off.IsBillDiscountFreebiesValue,
        //                         billamount = off.BillAmount,
        //                         MaxBillAmount = off.MaxBillAmount,
        //                         OfferOn = off.OfferOn
        //                     }).FirstOrDefault();

        //    if (offerdata != null && offerdata.OfferType == "BillDiscount_FreeItem")
        //    {
        //        if (offerdata.IsBillDiscountFreebiesItem)
        //        {
        //            if (offerdata.OfferId > 0)
        //            {
        //                var totalQty = freeBillItems.Where(x => x.ItemNumber == itemnumber && x.OrderdetailId != OrderDetailsId).Sum(x => x.Qty);
        //                var minorderdata = authContext.OfferDb.Where(x => x.OfferId == offerdata.OfferId).FirstOrDefault();
        //                var freequantitydata = authContext.BillDiscountFreeItem.Where(x => x.offerId == offerdata.OfferId && x.ItemId == ItemId).FirstOrDefault();
        //                if (minorderdata != null && minorderdata.MinOrderQuantity > 0 && freequantitydata != null && freequantitydata.Qty > 0)
        //                {
        //                    int multiply = Convert.ToInt32(totalQty / minorderdata.MinOrderQuantity);
        //                    int totalquantity = multiply * freequantitydata.Qty;
        //                    return totalquantity;
        //                }
        //                return offerdata.BillDisountNoOffreeQuantity;
        //            }
        //            return offerdata.BillDisountNoOffreeQuantity;
        //        }
        //        else if (offerdata.IsBillDiscountFreebiesValue)
        //        {
        //            var freequantitydata = authContext.BillDiscountFreeItem.Where(x => x.offerId == offerdata.OfferId && x.ItemId == ItemId).FirstOrDefault();
        //            if (offerdata.OfferId > 0)
        //            {
        //                double billDisitemValue = 0;
        //                var offerId = new SqlParameter
        //                {
        //                    ParameterName = "offerId",
        //                    Value = offerdata.OfferId
        //                };
        //                var SubCategoryId = new SqlParameter
        //                {
        //                    ParameterName = "SubCategoryId",
        //                    Value = 0
        //                };
        //                var BrandId = new SqlParameter
        //                {
        //                    ParameterName = "BrandId",
        //                    Value = 0
        //                };
        //                List<int> itemids = authContext.Database.SqlQuery<int>("exec GetOfferforbilldiscount  @offerId,@SubCategoryId,@BrandId", offerId, SubCategoryId, BrandId).ToList();
        //                //List<int> itemids = authContext.Database.SqlQuery<int>("exec GetOfferforbilldiscount  @offerId,@SubCategoryId,@BrandId", offerdata.OfferId, 0, 0).ToList();
        //                //if (itemids.Any() && itemids.Count() > 0 && itemids != null)
        //                //{
        //                //    billDisitemValue = freeBillItems.Where(x => itemids.Contains(x.ItemId) && !x.IsFreeitem).Sum(x => x.Qty * x.UnitPrice);
        //                //}
        //                int freeqty = 0;
        //                //if (offerdata.OfferOn == "BillDiscount")
        //                //    billDisitemValue = freeBillItems.Where(x => !x.IsFreeitem).Sum(x => x.Qty * x.UnitPrice);
        //                //else
        //                //    billDisitemValue = freeBillItems.Where(x => x.ItemNumber == itemnumber).Sum(x => x.Qty * x.UnitPrice);
        //                billDisitemValue = freeBillItems.Where(x => itemids.Contains(x.ItemId) && !x.IsFreeitem).Sum(x => x.Qty * x.UnitPrice);
        //                if (billDisitemValue >= offerdata.MaxBillAmount && offerdata.MaxBillAmount > 0)
        //                {
        //                    billDisitemValue = offerdata.MaxBillAmount;
        //                }
        //                int multiple = (int)(billDisitemValue / offerdata.billamount);
        //                freeqty = multiple * freequantitydata.Qty;
        //                return freeqty;
        //            }
        //            return offerdata.BillDisountNoOffreeQuantity;
        //        }
        //        else
        //        {
        //            return offerdata.BillDisountNoOffreeQuantity;
        //        }

        //    }
        //    else
        //    {
        //        return 0;
        //    }

        //}

        public async Task<int> getfreebiesitemasync(int OrderId, int ItemId, AuthContext authContext, int ParentTotalQty, int OrderDetailsId)
        {
            var offerdata = await (from offitem in authContext.OfferItemDb
                                   join off in authContext.OfferDb
                                   on offitem.ReferOfferId equals off.OfferId
                                   where offitem.OrderId == OrderId && offitem.FreeItemId == ItemId && offitem.OrderDetailsId == OrderDetailsId
                                   select new GetFreebiesInfo
                                   {
                                       itemId = offitem.itemId,
                                       MinOrderQuantity = offitem.MinOrderQuantity,
                                       FreeItemId = offitem.FreeItemId,
                                       NoOffreeQuantity = off.NoOffreeQuantity,
                                       OfferType = offitem.OfferType,
                                       BillDisountNoOffreeQuantity = offitem.NoOffreeQuantity
                                   }).FirstOrDefaultAsync();
            if (offerdata != null && offerdata.OfferType != "BillDiscount_FreeItem")
            {
                int multiply = Convert.ToInt32(ParentTotalQty / offerdata.MinOrderQuantity);
                int totalquantity = multiply * offerdata.NoOffreeQuantity;
                return totalquantity;
            }
            else if (offerdata != null && offerdata.OfferType == "BillDiscount_FreeItem")
            {
                return offerdata.BillDisountNoOffreeQuantity;
            }
            else
            {
                return 0;
            }

        }

        public string GenerateRazorpayQrCode(int orderId, int customerId, int cashAmount)
        {
            string qrCodeUrl = "";
            //bool iscommit = false;
            using (var authContext = new AuthContext())
            {
                var cashAmountRazorPay = cashAmount;
                var virtualAccountDb = authContext.RazorpayVirtualAccounts.FirstOrDefault(x => x.OrderId == orderId && x.IsActive);
                if (cashAmountRazorPay > 0)
                {
                    if (virtualAccountDb != null && virtualAccountDb.Amount == cashAmount)
                        qrCodeUrl = virtualAccountDb.QrCodeUrl;
                    else
                    {
                        if (virtualAccountDb != null && virtualAccountDb.Amount != cashAmount)
                        {
                            virtualAccountDb.IsActive = false;
                            virtualAccountDb.UpdatedDate = indianTime;
                            authContext.Entry(virtualAccountDb).State = EntityState.Modified;
                            //iscommit = true;
                        }

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        try
                        {
                            var companyDetail = authContext.CompanyDetailsDB.FirstOrDefault();

                            if (companyDetail.IsRazorpayEnable && !string.IsNullOrEmpty(companyDetail.RazorpayBaseUrl)
                                    && !string.IsNullOrEmpty(companyDetail.RazorpayApiKeyId)
                                    && !string.IsNullOrEmpty(companyDetail.RazorpayApiKeySecret)
                                )
                            {
                                var cust = authContext.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                                var razorpayCustomerId = cust.RazorpayCustomerId;

                                var byteArray = Encoding.ASCII.GetBytes($"{companyDetail.RazorpayApiKeyId}:{companyDetail.RazorpayApiKeySecret}");

                                if (string.IsNullOrEmpty(razorpayCustomerId))
                                {
                                    CustomerCreateRequest custReq = new CustomerCreateRequest
                                    {
                                        contact = cust.Mobile,
                                        //email = cust.Emailid,
                                        fail_existing = "0",
                                        name = !string.IsNullOrEmpty(cust.Name) ? cust.Name :
                                                !string.IsNullOrEmpty(cust.ShopName) ? cust.ShopName :
                                                cust.Skcode,
                                        notes = new CreateCustNotes
                                        {
                                            notes_key_1 = "Create this customer",
                                            notes_key_2 = ""
                                        }
                                    };

                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                                        //client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)) );

                                        var jsonRequestParam = JsonConvert.SerializeObject(custReq);
                                        var webhookDb = new RazorpayWebhookRequest
                                        {
                                            CreatedDate = indianTime,
                                            RequestJson = jsonRequestParam,
                                            Customerid = cust.CustomerId,
                                            CallType = 3,
                                            Url = companyDetail.RazorpayBaseUrl + "customers"
                                        };

                                        using (var content = new StringContent(jsonRequestParam, Encoding.UTF8, "application/json"))
                                        {
                                            try
                                            {
                                                var response = AsyncContext.Run(() => client.PostAsync(companyDetail.RazorpayBaseUrl + "customers", content));
                                                response.EnsureSuccessStatusCode();
                                                string responseBody = response.Content.ReadAsStringAsync().Result;
                                                var createCustResponse = JsonConvert.DeserializeObject<CustomerCreateResponse>(responseBody);

                                                if (createCustResponse != null)
                                                {
                                                    RazorPayCustomerReqResponse responseReq = new RazorPayCustomerReqResponse
                                                    {
                                                        CreatedDate = indianTime,
                                                        RequestJson = jsonRequestParam,
                                                        RequestType = 1,
                                                        ResponseJson = JsonConvert.SerializeObject(createCustResponse)
                                                    };
                                                    authContext.RazorPayCustomerReqResponse.Add(responseReq);
                                                    //iscommit = true;

                                                    webhookDb.ResponseJson = JsonConvert.SerializeObject(createCustResponse);

                                                    if (createCustResponse.error == null && !string.IsNullOrEmpty(createCustResponse.id))
                                                    {
                                                        cust.RazorpayCustomerId = createCustResponse.id;
                                                        authContext.Entry(cust).State = EntityState.Modified;
                                                        razorpayCustomerId = createCustResponse.id;
                                                    }
                                                }
                                            }
                                            catch (Exception exe)
                                            {
                                                webhookDb.ResponseJson = JsonConvert.SerializeObject(exe);
                                            }

                                            authContext.RazorpayWebhookRequest.Add(webhookDb);

                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(razorpayCustomerId))
                                {
                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                                        //client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(authString)));

                                        var request = new VirtualAccountRequest
                                        {
                                            amount_expected = Convert.ToInt32(cashAmountRazorPay) * 100,
                                            customer_id = razorpayCustomerId,
                                            description = "Cash Order Amount of Order: " + orderId,
                                            notes = new BharatQrNotes
                                            {
                                                reference_key = orderId.ToString()
                                            },
                                            receivers = new VirtualAccountRequestReceivers
                                            {
                                                //qr_code = new QrCodeType
                                                //{
                                                //    method = new Method
                                                //    {
                                                //        card = false,
                                                //        upi = true
                                                //    }
                                                //},
                                                types = new string[] { "qr_code" }
                                            },

                                        };

                                        var jsonRequestParam = JsonConvert.SerializeObject(request);

                                        var webhookDb = new RazorpayWebhookRequest
                                        {
                                            CreatedDate = indianTime,
                                            RequestJson = jsonRequestParam,
                                            OrderId = orderId,
                                            CallType = 4,
                                            Url = companyDetail.RazorpayBaseUrl + "virtual_accounts"
                                        };

                                        try
                                        {
                                            using (var content = new StringContent(jsonRequestParam, Encoding.UTF8, "application/json"))
                                            {
                                                var response = AsyncContext.Run(() => client.PostAsync(companyDetail.RazorpayBaseUrl + "virtual_accounts", content));
                                                response.EnsureSuccessStatusCode();
                                                string responseBody = response.Content.ReadAsStringAsync().Result;
                                                var virtualAccountResponse = JsonConvert.DeserializeObject<VirtualAccountResponse>(responseBody);
                                                webhookDb.ResponseJson = JsonConvert.SerializeObject(virtualAccountResponse);
                                                if (virtualAccountResponse != null && virtualAccountResponse.error == null)
                                                {
                                                    RazorpayVirtualAccounts virtualAccount = new RazorpayVirtualAccounts
                                                    {
                                                        Amount = Convert.ToInt32(cashAmountRazorPay),
                                                        CreatedDate = indianTime,
                                                        OrderId = orderId,
                                                        QrCodeUrl = virtualAccountResponse.receivers?.FirstOrDefault()?.short_url,
                                                        RazorpayCustomerId = razorpayCustomerId,
                                                        RequestJson = jsonRequestParam,
                                                        ResponseJson = JsonConvert.SerializeObject(virtualAccountResponse),
                                                        VirtualAccountId = virtualAccountResponse.id,
                                                        IsActive = true
                                                    };
                                                    authContext.RazorpayVirtualAccounts.Add(virtualAccount);
                                                    //iscommit = true;
                                                    qrCodeUrl = virtualAccount.QrCodeUrl;
                                                }
                                            }
                                        }
                                        catch (Exception exe)
                                        {
                                            webhookDb.ResponseJson = JsonConvert.SerializeObject(exe);
                                        }

                                        authContext.RazorpayWebhookRequest.Add(webhookDb);
                                    }

                                }

                            }
                        }
                        catch (Exception exe)
                        {
                            TextFileLogHelper.LogError("Razorpay for OrderId: " + orderId + Environment.NewLine + exe.ToString());
                        }


                        authContext.Commit();

                    }
                }
            }

            return qrCodeUrl;
        }

        public WebHookDbResponse CaptureWebHookQRPayment(RazorPayWebHookResponse webHookResponse)
        {
            var result = new WebHookDbResponse();
            using (var authContext = new AuthContext())
            {
                var virtualAccountId = webHookResponse.payload.virtual_account.entity.id;
                var webhookDb = new RazorpayWebhookRequest
                {
                    CreatedDate = indianTime,
                    RequestJson = JsonConvert.SerializeObject(webHookResponse),
                    VirtualAccountId = virtualAccountId,
                    CallType = 1
                };

                var virtualAccountDb = authContext.RazorpayVirtualAccounts.FirstOrDefault(x => x.VirtualAccountId == virtualAccountId && x.IsActive);

                if (virtualAccountDb != null)
                {
                    webhookDb.OrderId = virtualAccountDb.OrderId;
                    var cashPayments = authContext.PaymentResponseRetailerAppDb.Where(x => x.OrderId == virtualAccountDb.OrderId
                                            && x.status == "Success" && !x.IsOnline).ToList();

                    if (cashPayments != null && cashPayments.Any())
                    {
                        foreach (var item in cashPayments)
                        {
                            item.status = "Failed";
                            item.statusDesc = "Due to UPI from DeliveryApp";
                            authContext.Entry(item).State = EntityState.Modified;
                        }
                    }

                    authContext.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                    {
                        OrderId = virtualAccountDb.OrderId,
                        status = "Success",
                        CreatedDate = indianTime,
                        UpdatedDate = indianTime,
                        PaymentFrom = "Razorpay QR",
                        statusDesc = "Due to Delivery",
                        amount = webHookResponse.payload.payment.entity.amount / 100,
                        GatewayTransId = webHookResponse.payload.payment.entity.id,
                        IsOnline = true
                    });
                    if (AppConstants.IsUsingLedgerHitOnOnlinePayment)
                    {
                        if (authContext.OnlinePaymentDtlsForLedgerDB.FirstOrDefault(z => z.OrderId == virtualAccountDb.OrderId && z.TransactionId == Convert.ToString(webHookResponse.payload.payment.entity.id)) == null)
                        {
                            var Customers = authContext.Customers.Where(x => x.RazorpayCustomerId == virtualAccountDb.RazorpayCustomerId).FirstOrDefault();
                            OnlinePaymentDtlsForLedger Opdl = new OnlinePaymentDtlsForLedger();
                            Opdl.OrderId = virtualAccountDb.OrderId;
                            Opdl.IsPaymentSuccess = true;
                            Opdl.IsLedgerAffected = "Yes";
                            Opdl.PaymentDate = indianTime;
                            Opdl.TransactionId = Convert.ToString(webHookResponse.payload.payment.entity.id);
                            Opdl.IsActive = true;
                            Opdl.CustomerId = Customers.CustomerId;
                            authContext.OnlinePaymentDtlsForLedgerDB.Add(Opdl);
                        }
                    }

                    virtualAccountDb.UpdatedDate = indianTime;
                    virtualAccountDb.IsProcessed = true;
                    virtualAccountDb.IsActive = false;

                    authContext.Entry(virtualAccountDb).State = EntityState.Modified;

                    result.IsCaptured = true;
                    result.OrderId = virtualAccountDb.OrderId;
                    result.AmountCaptured = webHookResponse.payload.payment.entity.amount / 100;

                    var dboyMobileNo = authContext.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == virtualAccountDb.OrderId).DboyMobileNo;
                    result.DeliveryBoyFcmId = authContext.Peoples.FirstOrDefault(x => x.Mobile == dboyMobileNo).FcmId;

                }

                authContext.RazorpayWebhookRequest.Add(webhookDb);
                authContext.Commit();
            }

            return result;
        }
        internal async Task<bool> VANForNotification(int OrderId, double OrderAmount, PickerCustomerDc cust)
        {
            bool Result = false;
            try
            {
               

                AngularJSAuthentication.Model.Notification notification = new AngularJSAuthentication.Model.Notification();
                notification.title = "RTGS Payment Request";
                notification.Message = "Your Order " + OrderId + " of amount Rs." + OrderAmount + " is out for delivery. Please add balance to your RTGS account now for smooth and easy transaction.Ignore in case you have already added the amount";
                notification.Pic = "";//"https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";

                //string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                //string id11 = ConfigurationManager.AppSettings["FcmApiId"];
                //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                //tRequest.Method = "post";


                //var objNotification = new
                //{
                //    to = cust.FcmId,
                //    CustId = cust.CustomerId,
                //    data = new
                //    {
                //        title = notification.title,
                //        body = notification.Message,
                //        icon = notification.Pic,
                //        typeId = cust.CustomerId,
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

                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                var data = new FCMData
                {
                    title = notification.title,
                    body = notification.Message,
                    icon = notification.Pic,
                    typeId = cust.CustomerId,
                    notificationCategory = "",
                    notificationType = "Non-Actionable"
                };
                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                var result = await firebaseService.SendNotificationForApprovalAsync(cust.FcmId, data);
                if (result != null)
                {
                    Result = true;
                }
                else
                {
                    Result = false;
                }
            }
            catch (Exception ds)
            {
                logger.Error("Error during customer order notification: " + ds.ToString());
            }
            return  Result;
        }
        public class GetFreebiesInfo
        {
            public int itemId { get; set; }
            public int MinOrderQuantity { get; set; }
            public int FreeItemId { get; set; }
            public int NoOffreeQuantity { get; set; }
            public string OfferType { get; set; }
            public int BillDisountNoOffreeQuantity { get; set; }
            public bool IsBillDiscountFreebiesItem { get; set; }
            public int OfferId { get; set; }
            public bool IsBillDiscountFreebiesValue { get; set; }
            public double billamount { get; set; }
            public double MaxBillAmount { get; set; }
            public string OfferOn { get; set; }
        }

        public class FreeBillItems
        {
            public int ItemId { get; set; }
            public string ItemNumber { get; set; }
            public int Qty { get; set; }
            public int OrderdetailId { get; set; }
            public double UnitPrice { get; set; }
            public bool IsFreeitem { get; set; }
            public int? OfferId { get; set; }
        }

    }
}