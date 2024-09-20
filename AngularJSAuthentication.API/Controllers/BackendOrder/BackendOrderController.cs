using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.External.RetailerAPP;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BatchManager.Publishers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.BackEndItem;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.BackendOrder;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BackendOrder;
using AngularJSAuthentication.Model.ClearTax;
using AngularJSAuthentication.Model.PlaceOrder;
using AngularJSAuthentication.Model.Store;
using GenricEcommers.Models;
using LinqKit;
using NLog;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using static AngularJSAuthentication.API.Controllers.InActiveCustOrderMasterController;
using static AngularJSAuthentication.API.Controllers.OrderMasterrController;
using static AngularJSAuthentication.API.Controllers.OrderMastersAPIController;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.Model.BillDiscount;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.API.Controllers.KKReturnReplace;
using AngularJSAuthentication.Model.Consumer;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.Model.RazorPay;
using static AngularJSAuthentication.API.Controllers.OrderDispatchedMasterController;
using System.Text;


//using ArticPolar.Dev.IsGdAPI;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/BackendOrder")]
    public class BackendOrderController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        public double xPointValue = AppConstants.xPoint;
        public int MemberShipHours = AppConstants.MemberShipHours;


        [Route("CreateBO")]
        [HttpPost]
        public async Task<BackendOrderResponseMsg> CreateBO(ShoppingCartBODc sc)
        {
            BackendOrderResponseMsg response = new BackendOrderResponseMsg();
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    var ShoppingCarts = new ShoppingCartBODc();
                    ShoppingCarts.CreditNoteNumber = sc.CreditNoteNumber;
                    int wid = sc.itemDetails.Select(y => y.WarehouseId).FirstOrDefault();
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.WarehouseId == wid && x.Deleted == false && x.Active);
                    if (people == null)
                    {
                        response.Message = "You are not authorized to generate Order.";
                        return response;
                    }
                    if (sc.itemDetails.Any() && sc.itemDetails.Any(x => x.WarehouseId > 0) && sc.MobileNo != null && sc.itemDetails.Any(x => x.UnitPrice > 0))
                    {
                        int WarehouseId = sc.itemDetails != null ? sc.itemDetails.Select(x => x.WarehouseId).FirstOrDefault() : people.WarehouseId;
                        Warehouse warehouse = context.Warehouses.FirstOrDefault(w => w.WarehouseId == WarehouseId);
                        Cluster cluster = context.Clusters.FirstOrDefault(x => x.WarehouseId == WarehouseId && x.CityId == warehouse.Cityid && x.Active);
                        if (warehouse != null && !warehouse.IsStore)
                        {
                            response.Message = "This warehouse is not eligible for generate order.";
                            return response;
                        }
                        var cust = context.Customers.FirstOrDefault(c => c.Mobile.Trim().Equals(sc.MobileNo.Trim()));
                        //if (cust != null && warehouse.WarehouseId != WarehouseId)
                        //{
                        //    response.Status = false;
                        //    response.Message = "Customer Warehouse Mismatch";
                        //    return response;
                        //}
                        if (cust != null && string.IsNullOrEmpty(cust.RefNo))
                        {
                            cust.RefNo = sc.RefNo;
                            context.Entry(cust).State = EntityState.Modified;
                        }
                        if(cust != null && !string.IsNullOrEmpty(sc.CustomerName))
                        {
                            if (string.IsNullOrEmpty(cust.Name))
                            {
                                cust.Name = sc.CustomerName;                                
                            }
                            if (string.IsNullOrEmpty(cust.ShopName))
                            {
                                cust.ShopName = sc.CustomerName;
                            }
                            if (warehouse.StoreType == 1)
                            {
                              var consumerAddrs=  context.ConsumerAddressDb.Where(x => x.CustomerId == cust.CustomerId);
                                if (consumerAddrs != null)
                                {
                                    foreach (var addr in consumerAddrs)
                                    {
                                        addr.Default = false;
                                        context.Entry(addr).State = EntityState.Modified;
                                    }
                                    if (consumerAddrs.Any(x => x.CompleteAddress == sc.ShippingAddress))
                                    {
                                        var addr = consumerAddrs.FirstOrDefault(x => x.CompleteAddress == sc.ShippingAddress);
                                        addr.IsActive = true;
                                        addr.IsDeleted = false;
                                        addr.ModifiedBy = people.PeopleID;
                                        addr.ModifiedDate = DateTime.Now;
                                        addr.PersonName = sc.CustomerName;
                                        context.Entry(addr).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        var addr = new ConsumerAddress
                                        {
                                            Cityid = warehouse.Cityid,
                                            CityName = warehouse.CityName,
                                            WarehouseId = warehouse.WarehouseId,
                                            StateId = warehouse.Stateid,
                                            StateName = warehouse.StateName,
                                            Address1 = sc.ShippingAddress,
                                            CompleteAddress = sc.ShippingAddress,
                                            Default = false,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CreatedBy = people.PeopleID,
                                            CreatedDate = DateTime.Now,
                                            CustomerId = cust.CustomerId,
                                            lat = warehouse.latitude,
                                            lng = warehouse.longitude,
                                            PersonName = sc.CustomerName
                                        };
                                        context.ConsumerAddressDb.Add(addr);
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(cust.ShippingAddress))
                            {
                                cust.ShippingAddress = sc.ShippingAddress;
                            }
                            context.Entry(cust).State = EntityState.Modified;
                        }

                        if (cust == null)
                        {
                            response.Status = false;
                            response.Message = "Please Add Customer First";
                            return response;
                        }



                        if (cust != null)
                        {
                            Wallet wallets = context.WalletDb.Where(x => x.CustomerId == cust.CustomerId).FirstOrDefault();
                            if (wallets == null)
                            {
                                Wallet wallet = new Wallet();
                                wallet.CustomerId = cust.CustomerId;
                                wallet.TotalAmount = 0;
                                wallet.CreatedDate = DateTime.Now;
                                wallet.Deleted = false;
                                wallet.IsNotExpirable = true;
                                context.WalletDb.Add(wallet);
                                context.Commit();
                            }
                        }

                        ShoppingCarts.itemDetails = sc.itemDetails.Select(x => new IBODetail
                        {
                            qty = x.qty,
                            ItemId = x.ItemId,
                            WarehouseId = x.WarehouseId,
                            UnitPrice = x.UnitPrice,
                            batchdetails = x.batchdetails
                        }).ToList();
                        ShoppingCarts.CustomerName = sc.CustomerName;
                        ShoppingCarts.ShippingAddress = sc.ShippingAddress;
                        ShoppingCarts.BillDiscountAmount = sc.BillDiscountAmount;
                        ShoppingCarts.UsedWalletAmount = sc.UsedWalletAmount;
                        ShoppingCarts.RefNo = sc.RefNo;

                        var minOrderValue = GetRetailerMinOrder(warehouse.Cityid, WarehouseId);
                        if ((sc.NetAmount + (sc.BillDiscountAmount ?? 0) + (sc.UsedWalletAmount)) < minOrderValue)
                        {
                            response.Status = false;
                            response.Message = string.Format("हम आज केवल {0} / - से अधिक की राशि के आर्डर को स्वीकार कर रहे हैं। असुविधा के लिए खेद है।", minOrderValue);
                            return response;
                        }

                        response = await BackendPushBOOrder(ShoppingCarts, context, cust, people, warehouse, scope, sc.OfferIds);



                        if (response != null && response.Data != null && Convert.ToInt64(response.Data.OrderId) > 0)
                        {
                            scope.Complete();
                            response.Message = "Order generated Successfully.";
                            response.Status = true;
                        }
                        else
                        {
                            scope.Dispose();
                            response.Status = false;
                        }

                    }
                    else
                    {
                        response.Message = "Something went wrong.";
                        return response;
                    }
                }
            }
            return response;

        }


        private int GetRetailerMinOrder(int? Cityid, int Warehouseid)
        {
            int minOrderValue = Convert.ToInt32(ConfigurationManager.AppSettings["MinOrderValue"]);

            if (Cityid.HasValue && Warehouseid > 0)
            {
                MongoDbHelper<RetailerMinOrder> mongoDbHelper = new MongoDbHelper<RetailerMinOrder>();
                var cartPredicate = PredicateBuilder.New<RetailerMinOrder>(x => x.CityId == Cityid && x.WarehouseId == Warehouseid);
                var retailerMinOrder = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "RetailerMinOrder").FirstOrDefault();
                if (retailerMinOrder != null)
                {
                    minOrderValue = retailerMinOrder.MinOrderValue;
                }
                else
                {
                    RetailerMinOrder newRetailerMinOrder = new RetailerMinOrder
                    {
                        CityId = Cityid.Value,
                        WarehouseId = Warehouseid,
                        MinOrderValue = minOrderValue
                    };
                    var result = mongoDbHelper.Insert(newRetailerMinOrder);
                }
            }

            return minOrderValue;
        }

        [Route("UpdatePaymentStatus")]
        [HttpPost]
        public BackendOrderResponseMsg UpdatePaymentStatus(UpdateBOPaymentDc UpdateBOPayment)
        {
            BackendOrderResponseMsg response = new BackendOrderResponseMsg();
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (var context = new AuthContext())
                {
                    List<BatchCodeSubjectDc> OrderInvoiceQueue = new List<BatchCodeSubjectDc>();
                    List<BatchCodeSubjectDc> BackendOrderQueue = new List<BatchCodeSubjectDc>();
                    List<BatchCodeSubjectDc> PublisherPickerRejectStockList = new List<BatchCodeSubjectDc>();

                    OrderOutPublisher Publisher = new OrderOutPublisher();
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Deleted == false && x.Active);
                    var IsGenerateIRN = context.ClearTaxIntegrations.FirstOrDefault(x => x.OrderId == UpdateBOPayment.OrderId && x.APIType == "GenerateIRN" && x.IsProcessed && x.IsActive);
                    if (people == null)
                    {
                        response.Message = "You are not authorized to update Order.";
                        return response;
                    }
                    var Ordermaster = context.DbOrderMaster.Where(x => x.OrderId == UpdateBOPayment.OrderId && x.Status == "Payment Pending").Include(x => x.orderDetails).FirstOrDefault();
                    var OrderAmount = UpdateBOPayment.BOPayments.Sum(x => x.Amount);
                    if (Ordermaster != null)
                    {
                        if (Ordermaster.Status == "Payment Pending" && UpdateBOPayment.Status == "PaymentSuccess" && Ordermaster.GrossAmount == OrderAmount)
                        {
                            Warehouse warehouse = context.Warehouses.FirstOrDefault(w => w.WarehouseId == Ordermaster.WarehouseId);
                            var billdiscount = context.BillDiscountDb.Where(x => x.OrderId == UpdateBOPayment.OrderId && x.IsUsedNextOrder == true && x.BillDiscountTypeValue > 0).ToList();
                            foreach (var item in UpdateBOPayment.BOPayments)
                            {
                                context.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                {
                                    amount = Math.Round(item.Amount, 0),
                                    CreatedDate = indianTime,
                                    currencyCode = "INR",
                                    OrderId = Ordermaster.OrderId,
                                    PaymentFrom = item.PaymentMode,
                                    GatewayTransId = item.PaymentRefNo,
                                    status = "Success",
                                    statusDesc = "Order Place",
                                    UpdatedDate = indianTime,
                                    IsRefund = false
                                });
                            }

                            Ordermaster.Status = "Delivered";
                            Ordermaster.DeliveredDate = DateTime.Now;
                            OrderDispatchedMaster postOrderDispatch = new OrderDispatchedMaster();
                            postOrderDispatch = Mapper.Map(Ordermaster).ToANew<OrderDispatchedMaster>();
                            postOrderDispatch.DboyMobileNo = people.Mobile;
                            postOrderDispatch.DboyName = people.DisplayName;
                            postOrderDispatch.DBoyId = people.PeopleID;
                            postOrderDispatch.Status = "Delivered";
                            postOrderDispatch.invoice_no = "";
                            postOrderDispatch.CreatedDate = indianTime;
                            postOrderDispatch.OrderedDate = indianTime;
                            postOrderDispatch.Deliverydate = indianTime;
                            postOrderDispatch.UpdatedDate = indianTime;
                            if (IsGenerateIRN != null)
                            {
                                postOrderDispatch.IsGenerateIRN = true;
                            }
                            context.OrderDispatchedMasters.Add(postOrderDispatch);

                            OrderMasterHistories h1 = new OrderMasterHistories();
                            h1.orderid = Ordermaster.OrderId;
                            h1.Status = "Delivered";
                            h1.Description = "BO";
                            h1.Reasoncancel = null;
                            h1.Warehousename = warehouse.WarehouseName;
                            h1.username = people.DisplayName;
                            h1.userid = people.PeopleID;
                            h1.CreatedDate = DateTime.Now;
                            context.OrderMasterHistoriesDB.Add(h1);
                            context.Entry(Ordermaster).State = EntityState.Modified;
                            Wallet wallet = context.WalletDb.Where(x => x.CustomerId == Ordermaster.CustomerId).FirstOrDefault();
                            //#region Wallet Point for Post Offer
                            //if (billdiscount != null && billdiscount.Any())
                            //{
                            //    double walletamount = billdiscount.Sum(x => Convert.ToDouble(x.BillDiscountTypeValue));
                            //    if (wallet != null)
                            //    {
                            //        wallet.TotalAmount = wallet.TotalAmount + walletamount;
                            //        CustomerWalletHistory CWH = new CustomerWalletHistory();
                            //        CWH.PeopleId = people.PeopleID;
                            //        CWH.PeopleName = people.DisplayName;
                            //        CWH.OrderId = Ordermaster.OrderId;
                            //        //op by Cust
                            //        CWH.WarehouseId = Ordermaster.WarehouseId;
                            //        CWH.CompanyId = 1;
                            //        CWH.CustomerId = wallet.CustomerId;
                            //        CWH.NewAddedWAmount = walletamount;
                            //        CWH.TotalWalletAmount = wallet.TotalAmount;
                            //        CWH.Through = "Due To Order " + Ordermaster.OrderId + " Delivered Successfully.";
                            //        CWH.CreatedDate = indianTime;
                            //        CWH.UpdatedDate = indianTime;
                            //        context.CustomerWalletHistoryDb.Add(CWH);
                            //    }
                            //}
                            //#endregion

                            #region WalletPoint For Reward Point
                            if (Ordermaster.RewardPoint > 0)
                            {
                                if (wallet != null)
                                {
                                    wallet.TotalAmount = wallet.TotalAmount + Ordermaster.RewardPoint;
                                    CustomerWalletHistory CWH = new CustomerWalletHistory();
                                    CWH.PeopleId = people.PeopleID;
                                    CWH.PeopleName = people.DisplayName;
                                    CWH.OrderId = Ordermaster.OrderId;
                                    //op by Cust
                                    CWH.WarehouseId = Ordermaster.WarehouseId;
                                    CWH.CompanyId = 1;
                                    CWH.CustomerId = wallet.CustomerId;
                                    CWH.NewAddedWAmount = Ordermaster.RewardPoint;
                                    CWH.TotalWalletAmount = wallet.TotalAmount;
                                    CWH.Through = "From Order Delivered Successfully.";
                                    CWH.CreatedDate = indianTime;
                                    CWH.UpdatedDate = indianTime;
                                    context.CustomerWalletHistoryDb.Add(CWH);
                                }
                            }
                            #endregion

                            context.Entry(wallet).State = EntityState.Modified;


                            if (context.Commit() > 0)
                            {
                                MultiStockHelper<RTDOnPickedDc> MultiStockHelpers = new MultiStockHelper<RTDOnPickedDc>();
                                List<RTDOnPickedDc> RTDOnPickedList = new List<RTDOnPickedDc>();
                                List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
                                foreach (var StockHit in postOrderDispatch.orderDetails.Where(x => x.qty > 0))
                                {
                                    bool isfreestock = Ordermaster.orderDetails.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).IsFreeItem;
                                    string refstock = Ordermaster.orderDetails.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).IsDispatchedFreeStock == false ? "C" : "F";
                                    RTDOnPickedList.Add(new RTDOnPickedDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = people.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        IsFreeStock = isfreestock == true && refstock == "F" ? true : false,//false,
                                        IsDispatchFromPlannedStock = true,
                                        RefStockCode = refstock,//"C"
                                    });
                                }
                                if (RTDOnPickedList.Any())
                                {

                                    bool ress = MultiStockHelpers.MakeBulkEntryOnPicker(RTDOnPickedList, context);
                                    if (!ress)
                                    {
                                        response.Message = "Can't Dispatched Due to stock qty not available!!";
                                        return response;
                                    }

                                    //var param = new SqlParameter("@OrderId", Ordermaster.OrderId);
                                    //var param2 = new SqlParameter("@PeopleId", people.PeopleID);
                                    //var StockHit = context.Database.SqlQuery<int>("Exec BackendOrderProcess @OrderId,@PeopleId", param, param2).ToList();

                                    //PublishOrderInvoiceQueue
                                    OrderInvoiceQueue.Add(new BatchCodeSubjectDc
                                    {
                                        ObjectDetailId = warehouse.Stateid,
                                        ObjectId = Convert.ToInt64(Ordermaster.OrderId),
                                        StockType = "",
                                        Quantity = 0,
                                        WarehouseId = 0,
                                        ItemMultiMrpId = 0
                                    });

                                    //PublishBackendOrderQueue
                                    BackendOrderQueue.Add(new BatchCodeSubjectDc
                                    {
                                        ObjectDetailId = Convert.ToInt64(people.PeopleID),
                                        ObjectId = Convert.ToInt64(Ordermaster.OrderId),
                                        StockType = "",
                                        Quantity = 0,
                                        WarehouseId = 0,
                                        ItemMultiMrpId = 0
                                    });

                                    scope.Complete();
                                    if (OrderInvoiceQueue != null && OrderInvoiceQueue.Any())
                                    {
                                        Publisher.PublishOrderInvoiceQueue(OrderInvoiceQueue);
                                        Publisher.PublishBackendOrderQueue(BackendOrderQueue);
                                    }
                                    response.Status = true;
                                    response.Data = Ordermaster;
                                    response.Message = "Payment Updated Successfully.";
                                }
                            }
                            else
                            {
                                scope.Dispose();
                                response.Message = "Something went wrong";
                                response.Status = false;
                                response.Data = null;
                                return response;
                            }
                        }
                        else if (Ordermaster.Status == "Payment Pending" && UpdateBOPayment.Status == "Order Cancel")
                        {
                            //#region Update Item Limit qty 
                            //var IteMultiMRPIds = Ordermaster.orderDetails.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                            //var ItemLimits = context.ItemLimitMasterDB.Where(x => IteMultiMRPIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == Ordermaster.WarehouseId && x.IsItemLimit).ToList();
                            //if (ItemLimits != null && ItemLimits.Count()> 0)
                            //{
                            //    foreach (var items in Ordermaster.orderDetails)
                            //    {
                            //        ItemLimitMaster limit = ItemLimits.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.WarehouseId == Ordermaster.WarehouseId);
                            //        if (limit != null && items.qty > 0)
                            //        {
                            //            limit.ItemlimitQty = limit.ItemlimitQty + items.qty;
                            //            context.Entry(limit).State = EntityState.Modified;
                            //        }
                            //    }
                            //}
                            //#endregion

                            var BillDiscount = context.BackendOrderBillDiscountDetails.FirstOrDefault(x => x.Orderid == UpdateBOPayment.OrderId);
                            if (BillDiscount != null)
                            {
                                BillDiscount.BillDiscount = 0;
                                context.Entry(BillDiscount).State = EntityState.Modified;
                            }

                            foreach (var item in Ordermaster.orderDetails)
                            {
                                item.Status = "Order Canceled";
                                item.UpdatedDate = indianTime;
                            }
                            Wallet wallet = context.WalletDb.FirstOrDefault(x => x.CustomerId == Ordermaster.CustomerId);

                            if (Ordermaster.walletPointUsed > 0 && Ordermaster.walletPointUsed != null)
                            {
                                wallet.TotalAmount = wallet.TotalAmount + Ordermaster.walletPointUsed;
                                context.Entry(wallet).State = EntityState.Modified;
                                CustomerWalletHistory CWH = new CustomerWalletHistory();
                                CWH.PeopleId = people.PeopleID;
                                CWH.PeopleName = people.DisplayName;
                                CWH.OrderId = Ordermaster.OrderId;
                                //op by Cust
                                CWH.WarehouseId = Ordermaster.WarehouseId;
                                CWH.CompanyId = 1;
                                CWH.CustomerId = wallet.CustomerId;
                                CWH.NewAddedWAmount = Ordermaster.walletPointUsed;
                                CWH.TotalWalletAmount = wallet.TotalAmount;
                                CWH.Through = "From post order cancellation";
                                CWH.CreatedDate = indianTime;
                                CWH.UpdatedDate = indianTime;
                                context.CustomerWalletHistoryDb.Add(CWH);
                            }
                            Ordermaster.Status = "Order Canceled";
                            Ordermaster.UpdatedDate = indianTime;

                            OrderMasterHistories h1 = new OrderMasterHistories();
                            h1.orderid = Ordermaster.OrderId;
                            h1.Status = Ordermaster.Status;
                            h1.Reasoncancel = null;
                            h1.Warehousename = Ordermaster.WarehouseName;
                            h1.Description = "BO";
                            if (people.DisplayName != null)
                            {
                                h1.username = people.DisplayName;
                            }
                            else
                            {
                                h1.username = people.PeopleFirstName;
                            }
                            h1.userid = userid;
                            h1.CreatedDate = DateTime.Now;
                            MultiStockHelper<OnPickedCancelDc> MultiStockHelpers = new MultiStockHelper<OnPickedCancelDc>();
                            List<OnPickedCancelDc> RTDOnPickedCancelList = new List<OnPickedCancelDc>();
                            foreach (var StockHit in Ordermaster.orderDetails.Where(x => x.qty > 0))
                            {
                                bool isfreestock = Ordermaster.orderDetails.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).IsFreeItem;
                                string refstock = Ordermaster.orderDetails.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).IsDispatchedFreeStock == false ? "C" : "F";
                                RTDOnPickedCancelList.Add(new OnPickedCancelDc
                                {
                                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                    OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                    OrderId = StockHit.OrderId,
                                    Qty = StockHit.qty,
                                    UserId = userid,
                                    WarehouseId = StockHit.WarehouseId,
                                    IsFreeStock = isfreestock == true && refstock == "F" ? true : false,//isfreestock,//false,
                                    RefStockCode = refstock//"C"
                                });
                            }
                            if (RTDOnPickedCancelList.Any())
                            {

                                bool res = MultiStockHelpers.MakeEntry(RTDOnPickedCancelList, "Stock_OnPickedCancel", context, scope);
                                if (!res)
                                {
                                    scope.Dispose();
                                    response.Message = "Inventory not reverted on Canceled Picker";
                                    return response;
                                }
                                foreach (var s in RTDOnPickedCancelList.Where(x => x.Qty > 0))
                                {
                                    PublisherPickerRejectStockList.Add(new BatchCodeSubjectDc
                                    {
                                        ObjectDetailId = s.OrderDispatchedDetailsId,  // its OrderDetailsId
                                        ObjectId = s.OrderId,
                                        StockType = s.RefStockCode,
                                        Quantity = s.Qty,
                                        WarehouseId = s.WarehouseId,
                                        ItemMultiMrpId = s.ItemMultiMRPId
                                    });
                                }
                                context.Entry(Ordermaster).State = EntityState.Modified;
                                context.OrderMasterHistoriesDB.Add(h1);
                                context.Commit();
                                scope.Complete();
                                response.Status = true;
                                response.Data = Ordermaster;
                                response.Message = "Order Canceled Successfully.";
                                if (PublisherPickerRejectStockList != null && PublisherPickerRejectStockList.Any())
                                {
                                    Publisher.PlannedRejectPublish(PublisherPickerRejectStockList);
                                }
                            }
                        }
                        else
                        {
                            response.Message = "Order Already " + Ordermaster.Status;
                            response.Status = false;
                            response.Data = null;
                            return response;
                        }
                    }
                    else
                    {
                        response.Message = "Amount mismatch.";
                        response.Status = false;
                        response.Data = null;
                        return response;
                    }
                }
            }
            return response;
        }

        [Route("BOBillDiscount")]
        [HttpGet]
        public BackendOrderResponseMsg UpdatePaymentStatus(double TotalAmount, double discount, int wareid)
        {
            BackendOrderResponseMsg response = new BackendOrderResponseMsg();
            using (var context = new AuthContext())
            {
                int month = DateTime.Now.Month;
                var BillDiscount = context.BackendOrderBillDiscountConfigurations.FirstOrDefault(x => x.WarehouseId == wareid && x.CreatedDate.Month == month);
                if (BillDiscount != null && discount > 0)
                {
                    var BillDiscountdetails = context.BackendOrderBillDiscountDetails.Where(x => x.BackendOrderBillDiscountMasterId == BillDiscount.Id).ToList();
                    if (BillDiscountdetails.Any() && BillDiscountdetails.Count > 0)
                    {
                        var BillDiscountUsed = BillDiscountdetails.Sum(x => x.BillDiscount);
                        var amount = (BillDiscount.Percentage * TotalAmount) / 100;
                        if ((BillDiscountUsed + discount) >= BillDiscount.Amount)
                        {
                            response.Status = false;
                            response.Message = "Bill discount limit has exceed ";
                            response.Data = null;
                            return response;
                        }

                        if (discount > Math.Round(amount, 2))
                        {
                            response.Status = false;
                            response.Message = "Maximum Discount Value is :" + Math.Round(amount, 2);
                            response.Data = null;
                            return response;
                        }
                        else
                        {
                            response.Status = true;
                            response.Message = "Bill Discount applied Successfully";
                            response.Data = null;
                            return response;
                        }
                    }
                    else
                    {
                        var amount = (BillDiscount.Percentage * TotalAmount) / 100;
                        if (discount > Math.Round(amount, 2) || discount > BillDiscount.Amount)
                        {
                            response.Status = false;
                            response.Message = "Maximum Discount Value is :" + Math.Round(amount, 2);
                            response.Data = null;
                            return response;
                        }
                        else
                        {
                            response.Status = true;
                            response.Message = "Bill Discount applied Successfully";
                            response.Data = null;
                            return response;
                        }
                    }
                }
                else
                {
                    response.Status = true;
                    response.Message = "";
                    response.Data = null;
                    return response;
                }

            }
            return response;
        }

        [Route("AssignMentSubmit")]
        [HttpGet]
        public BackendOrderResponseMsg AssignMentSubmit(long AssignmentId)
        {
            BackendOrderResponseMsg response = new BackendOrderResponseMsg();
            using (var context = new AuthContext())
            {
                var Assignmemt = context.DeliveryIssuanceDb.FirstOrDefault(x => x.DeliveryIssuanceId == AssignmentId && x.Status == "Pending");
                if (Assignmemt != null)
                {
                    Assignmemt.Status = "Submitted";
                    Assignmemt.UpdatedDate = DateTime.Now;
                    context.Entry(Assignmemt).State = EntityState.Modified;
                    if (context.Commit() > 0)
                    {
                        response.Status = true;
                        response.Message = "Submitted Successfully";
                        response.Data = null;
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Failed";
                        response.Data = null;
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Data not found";
                    response.Data = null;
                }
            }
            return response;
        }

        [HttpGet]
        [Route("GetItemList")]
        public async Task<List<BackendItem>> GetItemList(string keyword, int warehouseId, string MobileNo)
        {
            List<BackendItem> FinalItemList = new List<BackendItem>();

            List<ItemDataDC> freebiesitemmasterlist = new List<ItemDataDC>();
            List<ItemDataDC> itemMasters = new List<ItemDataDC>();
            BackendOrderResponseMsg response = new BackendOrderResponseMsg();
            DateTime CurrentDate = indianTime;
            var identity = User.Identity as ClaimsIdentity;
            int warehouseid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);

            using (var context = new AuthContext())
            {
                string CustomerType = "";
                var Cust = context.Customers.FirstOrDefault(x => x.Mobile == MobileNo && x.Deleted == false);
                if (Cust != null && !string.IsNullOrEmpty(Cust.CustomerType))
                {
                    CustomerType = Cust.CustomerType;
                }
                var ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == Cust.CustomerId);
                var warehousedata = context.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);
                if(warehousedata.StoreType==1)
                    CustomerType = "Consumer";
                if (CustomerType == "Consumer")
                {
                    #region freebies section

                    itemMasters = (from a in context.itemMasters
                                   where (a.WarehouseId == warehouseId && a.OfferStartTime <= CurrentDate
                                   && a.OfferEndTime >= indianTime && a.OfferCategory == 1 && a.active == true && a.Deleted == false && (a.ItemAppType == 0 || a.ItemAppType == 1))
                                   join c in context.OfferDb on a.OfferId equals c.OfferId
                                   where (c.IsActive == true && c.IsDeleted == false && (c.OfferAppType == "BackendStore"))
                                   let limit = context.ItemLimitMasterDB.Where(p2 => a.ItemMultiMRPId == p2.ItemMultiMRPId && a.WarehouseId == p2.WarehouseId).FirstOrDefault()
                                   select new ItemDataDC
                                   {
                                       WarehouseId = a.WarehouseId,
                                       CompanyId = a.CompanyId,
                                       IsItemLimit = limit != null ? limit.IsItemLimit : false,
                                       ItemlimitQty = limit != null && limit.IsItemLimit ? limit.ItemlimitQty : 0,
                                       ItemId = a.ItemId,
                                       ItemNumber = a.Number,
                                       itemBaseName = a.itemBaseName,
                                       //itemname = a.HindiName != null ? a.HindiName : a.itemname,
                                       itemname = a.itemname,
                                       LogoUrl = a.LogoUrl,
                                       MinOrderQty = a.MinOrderQty,
                                       price = a.price,
                                       TotalTaxPercentage = a.TotalTaxPercentage,
                                       UnitPrice = a.UnitPrice,
                                       HindiName = a.HindiName != null ? a.HindiName : a.itemname,
                                       active = a.active,
                                       marginPoint = a.marginPoint,
                                       NetPurchasePrice = a.NetPurchasePrice,
                                       promoPerItems = a.promoPerItems,
                                       IsOffer = a.IsOffer,
                                       Deleted = a.Deleted,
                                       OfferCategory = a.OfferCategory,
                                       OfferStartTime = a.OfferStartTime,
                                       OfferEndTime = a.OfferEndTime,
                                       OfferQtyAvaiable = a.OfferQtyAvaiable,
                                       OfferQtyConsumed = a.OfferQtyConsumed,
                                       OfferId = a.OfferId,
                                       OfferType = a.OfferType,
                                       OfferWalletPoint = a.OfferWalletPoint,
                                       OfferFreeItemId = a.OfferFreeItemId,
                                       OfferPercentage = a.OfferPercentage,
                                       OfferFreeItemName = a.OfferFreeItemName,
                                       OfferFreeItemImage = a.OfferFreeItemImage,
                                       OfferFreeItemQuantity = a.OfferFreeItemQuantity,
                                       OfferMinimumQty = a.OfferMinimumQty,
                                       FlashDealSpecialPrice = a.FlashDealSpecialPrice,
                                       ItemMultiMRPId = a.ItemMultiMRPId,
                                       BillLimitQty = a.BillLimitQty,
                                       ItemAppType = a.ItemAppType,
                                       Categoryid = a.Categoryid,
                                       SubCategoryId = a.SubCategoryId,
                                       SubsubCategoryid = a.SubsubCategoryid,
                                       BaseCategoryId = a.BaseCategoryid,
                                       SellingSku = a.SellingSku,
                                       SellingUnitName = a.SellingUnitName,
                                       IsSensitive = a.IsSensitive,
                                       IsSensitiveMRP = a.IsSensitiveMRP,
                                       TradePrice = a.TradePrice,
                                       WholeSalePrice = a.WholeSalePrice
                                   }).OrderByDescending(x => x.ItemNumber).ToList();
                    #endregion

                    var formatedData = await ItemValidate(itemMasters, ActiveCustomer, context);
                    freebiesitemmasterlist.AddRange(formatedData);
                }


                List<BackendItem> ItemList = new List<BackendItem>();
                List<BackendItemBatchs> backenditembaches = new List<BackendItemBatchs>();
                List<MoqItem> moqitems = new List<MoqItem>();

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                var param = new SqlParameter("@WarehouseId", warehouseId > 0 ? warehouseId : warehouseid);
                var param1 = new SqlParameter("@Keyword", keyword);

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[BackendOrderItemGet]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(param);
                cmd.Parameters.Add(param1);
                cmd.CommandTimeout = 1200;
                var reader = cmd.ExecuteReader();
                ItemList = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<BackendItem>(reader).ToList();
                reader.NextResult();
                backenditembaches = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<BackendItemBatchs>(reader).ToList();
                reader.NextResult();
                moqitems = ((IObjectContextAdapter)context)
                .ObjectContext
                .Translate<MoqItem>(reader).ToList();

                foreach (var item in ItemList)
                {
                    item.Consumerprice = GetConsumerPrice(context, item.ItemMultiMRPId, item.MRP, item.UnitPrice, warehouseId);
                }

                FinalItemList = ItemList.Select(x => new BackendItem
                {
                    ItemId = x.ItemId,
                    WarehouseId = x.WarehouseId,
                    ItemMultiMRPId = x.ItemMultiMRPId,
                    MinOrderQty = x.MinOrderQty,
                    UnitPrice = SkCustomerType.GetPriceFromType(CustomerType, x.UnitPrice
                                                                , x.WholeSalePrice
                                                                , x.TradePrice, x.Consumerprice),
                    WholeSalePrice = x.WholeSalePrice,
                    TradePrice = x.TradePrice,
                    MRP = x.MRP,
                    ItemName = x.ItemName,
                    CurrentInventory = x.CurrentInventory,
                    ItemNumber = x.ItemNumber,
                    BackendItemBatchs = backenditembaches.Where(y => y.itemmultimrpid == x.ItemMultiMRPId).ToList(),
                    MoqItems = moqitems.Where(z => z.Number == x.ItemNumber).OrderBy(y => y.MinOrderQty).ToList(),
                    itemDataDCs = freebiesitemmasterlist.Where(y => y.ItemId == x.ItemId).ToList()
                }).ToList();




            }
            return FinalItemList;
        }

        [HttpGet]
        [Route("GetWarehouseCashCustomer/{warehouseId}/{key}")]
        public Customer GetWarehouseCashCustomer(int warehouseId, string key)
        {
            string backendCustomerName = ConfigurationManager.AppSettings["BackendCustomerName"].ToString();
            using (var context = new AuthContext())
            {
                var customer = new Customer();
                if (key != null && key != "undefined")
                {
                    customer = context.Customers.FirstOrDefault(x => x.Skcode.ToLower().Contains(key.ToLower()) && x.Active == true && x.Deleted == false && x.Warehouseid == warehouseId);
                }
                else
                {
                    customer = context.Customers.FirstOrDefault(x => x.Name == backendCustomerName && x.Deleted == false && x.Warehouseid == warehouseId);
                }
                return customer;
            }
        }

        //public async Task<BackendOrderResponseMsg> BackendPushBOOrder(ShoppingCartBODc sc, AuthContext context, Customer Customers, People people, Warehouse warehouse, TransactionScope scope, List<int> ExistingOfferIds)
        //{
        //    int salespersonid = 0;
        //    int wareid = sc.itemDetails.Select(x => x.WarehouseId).FirstOrDefault();
        //    var salespersondata = context.Peoples.FirstOrDefault(x => x.WarehouseId == wareid && x.Department == "Sales" && x.Active == true);
        //    if (salespersondata != null) { salespersonid = salespersondata.PeopleID; }

        //    BackendOrderResponseMsg result = new BackendOrderResponseMsg();
        //    OrderMaster objOrderMaster = new OrderMaster();
        //    objOrderMaster.orderDetails = new List<OrderDetails>();
        //    var cust = Customers;
        //    #region Prepair order 
        //    objOrderMaster.CompanyId = warehouse.CompanyId;
        //    objOrderMaster.WarehouseId = warehouse.WarehouseId;
        //    objOrderMaster.WarehouseName = warehouse.WarehouseName;
        //    objOrderMaster.CustomerCategoryId = 2;
        //    objOrderMaster.Status = "Payment Pending";
        //    objOrderMaster.CustomerName = sc.CustomerName;
        //    objOrderMaster.ShopName = Customers.ShopName;
        //    objOrderMaster.BillingAddress = sc.ShippingAddress;
        //    objOrderMaster.ShippingAddress = sc.ShippingAddress;
        //    objOrderMaster.Customerphonenum = Customers.Mobile;
        //    objOrderMaster.LandMark = cust.LandMark;
        //    objOrderMaster.Skcode = cust.Skcode;
        //    objOrderMaster.CustomerType = cust.CustomerType;
        //    objOrderMaster.CustomerId = cust.CustomerId;
        //    objOrderMaster.CityId = cust.Cityid;
        //    objOrderMaster.ShopName = cust.ShopName;
        //    objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
        //    objOrderMaster.ClusterName = cust.ClusterName;
        //    objOrderMaster.OrderTakenSalesPerson = "Self";
        //    objOrderMaster.comments = "BO";
        //    objOrderMaster.active = true;
        //    objOrderMaster.CreatedDate = indianTime;
        //    objOrderMaster.Tin_No = sc.RefNo;
        //    objOrderMaster.Deliverydate = indianTime;
        //    objOrderMaster.UpdatedDate = indianTime;
        //    objOrderMaster.Deleted = false;
        //    objOrderMaster.OrderTakenSalesPersonId = salespersonid;
        //    #endregion
        //    var itemIds = sc.itemDetails.Select(x => x.ItemId).ToList();
        //    var itemMastersList = context.itemMasters.Where(x => itemIds.Contains(x.ItemId) && x.WarehouseId == cust.Warehouseid).ToList();

        //    RetailerAppManager retailerAppManager = new RetailerAppManager();
        //    List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
        //    List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();

        //    //List<ItemMultiMRP> mrplistdata = context.ItemMultiMRPDB.Where(x => itemMastersList.Select(y => y.ItemMultiMRPId).ToList().Contains(x.ItemMultiMRPId) && x.Deleted == false).ToList();
        //    var IteMultiMRPIds = itemMastersList.Select(x => x.ItemMultiMRPId).Distinct().ToList();
        //    var ItemLimits = context.ItemLimitMasterDB.Where(x => IteMultiMRPIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == warehouse.WarehouseId && x.IsItemLimit).ToList();

        //    foreach (var combo in sc.itemDetails)
        //    {
        //        var items = itemMastersList.Where(x => x.ItemId == combo.ItemId).FirstOrDefault();

        //        double cprice = GetConsumerPrice(context, items.ItemMultiMRPId, items.price, items.UnitPrice, warehouse.WarehouseId);

        //        double UnitPrice = SkCustomerType.GetPriceFromType(cust.CustomerType, items.UnitPrice
        //                                                             , items.WholeSalePrice ?? 0
        //                                                             , items.TradePrice ?? 0, cprice);


        //        if (items.active && items.UnitPrice > 0)
        //        {
        //            #region Update Item Limit qty 
        //            if (ItemLimits != null && ItemLimits.Count() > 0)
        //            {
        //                ItemLimitMaster limit = ItemLimits.FirstOrDefault(x => x.ItemNumber == items.ItemNumber && x.ItemMultiMRPId == items.ItemMultiMRPId && x.WarehouseId == warehouse.WarehouseId && x.IsItemLimit);
        //                if (limit != null && limit.ItemlimitQty >= combo.qty)
        //                {
        //                    limit.ItemlimitQty = limit.ItemlimitQty - combo.qty;
        //                    limit.ItemLimitSaleQty = limit.ItemLimitSaleQty + combo.qty;
        //                    context.Entry(limit).State = EntityState.Modified;

        //                    if (limit.ItemlimitQty == 0)
        //                    {
        //                        items.active = false;
        //                        items.UpdatedDate = indianTime;
        //                        items.UpdateBy = "Auto Dective";
        //                        items.Reason = "Auto Dective due remaining limit is less than Order Qty  or zero:" + limit.ItemlimitQty;
        //                        context.Entry(items).State = EntityState.Modified;
        //                    }
        //                }
        //                else
        //                {
        //                    if (limit != null && (limit.ItemlimitQty <= 0 || limit.ItemlimitQty < combo.qty))
        //                    {
        //                        items.active = false;
        //                        items.UpdatedDate = indianTime;
        //                        items.UpdateBy = "Auto Dective";
        //                        items.Reason = "Auto Dective due remaining limit is less than Order Qty  or zero:" + limit.ItemlimitQty;
        //                        context.Entry(items).State = EntityState.Modified;

        //                        result.Message = "The available item limit of itemname-(" + items.itemname + "), is less than the ordered quantity.";
        //                        return result;
        //                    }
        //                }
        //            }
        //            #endregion

        //            OrderDetails od = new OrderDetails();
        //            if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid))
        //            {
        //                var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid);
        //                od.StoreId = store.StoreId;
        //                if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId))
        //                {
        //                    var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId);
        //                    od.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
        //                    od.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
        //                }
        //            }
        //            else
        //            {
        //                od.StoreId = 0;
        //                od.ExecutiveId = 0;
        //                od.ExecutiveName = "";
        //            }

        //            od.ActualUnitPrice = UnitPrice;
        //            od.CustomerName = sc.CustomerName;
        //            od.Mobile = Customers.Mobile;
        //            od.CustomerId = cust.CustomerId;
        //            od.CityId = cust.Cityid;
        //            od.OrderDate = indianTime;
        //            od.Status = "Payment Pending";
        //            od.CompanyId = warehouse.CompanyId;
        //            od.WarehouseId = warehouse.WarehouseId;
        //            od.WarehouseName = warehouse.WarehouseName;
        //            od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
        //            od.ItemId = items.ItemId;
        //            od.ItemMultiMRPId = items.ItemMultiMRPId;
        //            od.Itempic = items.LogoUrl;
        //            od.itemname = items.itemname;
        //            od.SupplierName = items.SupplierName;
        //            od.SellingUnitName = items.SellingUnitName;
        //            od.CategoryName = items.CategoryName;
        //            od.SubsubcategoryName = items.SubsubcategoryName;
        //            od.SubcategoryName = items.SubcategoryName;
        //            od.SellingSku = items.SellingSku;
        //            od.City = items.CityName;
        //            od.itemcode = items.itemcode;
        //            od.HSNCode = items.HSNCode;
        //            od.itemNumber = items.Number;
        //            od.Barcode = items.itemcode;
        //            od.UnitPrice = combo.UnitPrice;
        //            od.price = items.price;
        //            od.MinOrderQty = items.MinOrderQty;
        //            od.MinOrderQtyPrice = (od.MinOrderQty * od.UnitPrice);
        //            od.qty = Convert.ToInt32(combo.qty);
        //            od.SizePerUnit = items.SizePerUnit;
        //            od.TaxPercentage = items.TotalTaxPercentage;
        //            if (od.TaxPercentage >= 0)
        //            {
        //                od.SGSTTaxPercentage = od.TaxPercentage / 2;
        //                od.CGSTTaxPercentage = od.TaxPercentage / 2;
        //            }
        //            od.Noqty = od.qty;
        //            od.TotalAmt = System.Math.Round(od.UnitPrice * od.qty, 2);

        //            if (items.TotalCessPercentage > 0)
        //            {
        //                od.TotalCessPercentage = items.TotalCessPercentage;
        //                double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

        //                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge / 100)) / 100;

        //                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
        //                od.CessTaxAmount = (od.AmtWithoutAfterTaxDisc * od.TotalCessPercentage) / 100;
        //            }
        //            double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;
        //            od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge2 / 100)) / 100;
        //            od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
        //            od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
        //            if (od.TaxAmmount >= 0)
        //            {
        //                od.SGSTTaxAmmount = od.TaxAmmount / 2;
        //                od.CGSTTaxAmmount = od.TaxAmmount / 2;
        //            }
        //            if (od.CessTaxAmount > 0)
        //            {
        //                double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
        //                od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge3 / 100)) / 100;
        //                od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
        //                od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.CessTaxAmount + od.TaxAmmount;
        //            }
        //            else
        //            {
        //                od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
        //            }
        //            od.DiscountPercentage = 0;
        //            od.DiscountAmmount = 0;
        //            double DiscountAmmount = od.DiscountAmmount;
        //            double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
        //            od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
        //            double TaxAmmount = od.TaxAmmount;
        //            od.Purchaseprice = items.PurchasePrice;
        //            od.CreatedDate = indianTime;
        //            od.UpdatedDate = indianTime;
        //            od.Deleted = false;
        //            objOrderMaster.orderDetails.Add(od);
        //        }
        //        else
        //        {
        //            result.Message = "Item is inactive or rate change";

        //            return result;
        //        }
        //    }

        //    var rewardpoint = (double)objOrderMaster.orderDetails.Sum(x => x.marginPoint);
        //    objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt));
        //    objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
        //    objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
        //    objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
        //    objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0);
        //    objOrderMaster.DiscountAmount = 0;
        //    objOrderMaster.ClusterId = cust.ClusterId ?? 0;
        //    objOrderMaster.ClusterName = cust.ClusterName;
        //    objOrderMaster.OrderType = !string.IsNullOrEmpty(cust.CustomerType) && cust.CustomerType == "Consumer" ? 11 : 1;
        //    double DeliveryAmount = 0;
        //    objOrderMaster.deliveryCharge = DeliveryAmount;
        //    Wallet wallet = context.WalletDb.FirstOrDefault(c => c.CustomerId == cust.CustomerId);
        //    if (wallet != null && wallet.TotalAmount > 0 && wallet.TotalAmount >= sc.UsedWalletAmount && sc.UsedWalletAmount > 0)
        //    {
        //        CustomerWalletHistory CWH = new CustomerWalletHistory();

        //        CWH.WarehouseId = cust.Warehouseid ?? 0;
        //        CWH.CompanyId = cust.CompanyId ?? 0;
        //        CWH.CustomerId = wallet.CustomerId;
        //        CWH.Through = "Used On Order";
        //        CWH.NewOutWAmount = sc.UsedWalletAmount * 10;
        //        CWH.TotalWalletAmount = wallet.TotalAmount - CWH.NewOutWAmount;
        //        CWH.TotalEarningPoint = 0;
        //        CWH.CreatedDate = indianTime;
        //        CWH.UpdatedDate = indianTime;
        //        CWH.OrderId = objOrderMaster.OrderId;
        //        context.CustomerWalletHistoryDb.Add(CWH);
        //        //update in wallet
        //        wallet.TotalAmount -= CWH.NewOutWAmount;
        //        wallet.TransactionDate = indianTime;
        //        context.Entry(wallet).State = EntityState.Modified;

        //        objOrderMaster.walletPointUsed = sc.UsedWalletAmount * 10;
        //        objOrderMaster.WalletAmount = sc.UsedWalletAmount;
        //    }

        //    objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0) + objOrderMaster.TCSAmount - (sc.UsedWalletAmount);
        //    objOrderMaster.BillDiscountAmount = sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0;

        //    objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero);
        //    context.DbOrderMaster.Add(objOrderMaster);
        //    context.Commit();
        //    if (objOrderMaster.OrderId != 0)
        //    {

        //        string Borderid = Convert.ToString(objOrderMaster.OrderId);
        //        string BorderCodeId = Borderid.PadLeft(11, '0');
        //        temOrderQBcode code = context.GetBarcode(BorderCodeId);
        //        objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;
        //        objOrderMaster.Status = "Payment Pending";

        //        context.Commit();

        //        OrderMasterHistories h1 = new OrderMasterHistories();
        //        h1.orderid = objOrderMaster.OrderId;
        //        h1.Status = "Payment Pending";
        //        h1.Description = "BO";
        //        h1.Reasoncancel = null;
        //        h1.Warehousename = warehouse.WarehouseName;
        //        h1.username = people.DisplayName;
        //        h1.userid = people.PeopleID;
        //        h1.CreatedDate = DateTime.Now;
        //        context.OrderMasterHistoriesDB.Add(h1);
        //        #region
        //        var BillDiscount = context.BackendOrderBillDiscountConfigurations.FirstOrDefault(x => x.WarehouseId == wareid && x.CreatedDate.Month == DateTime.Now.Month);
        //        if (BillDiscount != null && sc.BillDiscountAmount.Value > 0)
        //        {
        //            BackendOrderBillDiscountDetails bo = new BackendOrderBillDiscountDetails();
        //            bo.Orderid = objOrderMaster.OrderId;
        //            bo.BackendOrderBillDiscountMasterId = BillDiscount.Id;
        //            bo.BillDiscount = sc.BillDiscountAmount.Value;
        //            bo.CreatedDate = DateTime.Now;
        //            context.BackendOrderBillDiscountDetails.Add(bo);
        //        }
        //        #endregion

        //        #region Update IRN Check 
        //        IRNHelper irnHelper = new IRNHelper();

        //        if (irnHelper.IsGenerateIRN(context, cust.CustomerId))
        //        {
        //            #region ClearTaxIntegrations
        //            ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration();
        //            clearTaxIntegration.OrderId = objOrderMaster.OrderId;
        //            clearTaxIntegration.IsActive = true;
        //            clearTaxIntegration.CreateDate = DateTime.Now;
        //            clearTaxIntegration.IsProcessed = false;
        //            clearTaxIntegration.APIType = "GenerateIRN";
        //            context.ClearTaxIntegrations.Add(clearTaxIntegration);
        //            #endregion

        //        }
        //        #endregion


        //        if (context.Commit() > 0)
        //        {

        //            MultiStockHelper<Stock_OnPickedDc> MultiStockHelpers = new MultiStockHelper<Stock_OnPickedDc>();

        //            List<Stock_OnPickedDc> rtdStockList = new List<Stock_OnPickedDc>();

        //            List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
        //            foreach (var StockHit in objOrderMaster.orderDetails.Where(x => x.qty > 0))
        //            {
        //                rtdStockList.Add(new Stock_OnPickedDc
        //                {
        //                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
        //                    OrderDispatchedDetailsId = StockHit.OrderDetailsId,
        //                    OrderId = StockHit.OrderId,
        //                    Qty = StockHit.qty,
        //                    UserId = people.PeopleID,
        //                    WarehouseId = StockHit.WarehouseId,
        //                    IsFreeStock = false,
        //                    RefStockCode = "C"
        //                });
        //                foreach (var Batchlist in sc.itemDetails.Where(x => x.ItemId == StockHit.ItemId))
        //                {
        //                    foreach (var bitem in Batchlist.batchdetails)
        //                    {
        //                        TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
        //                        {

        //                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
        //                            Qty = bitem.qty * (-1),
        //                            StockBatchMasterId = bitem.StockBatchMasterId,
        //                            WarehouseId = warehouse.WarehouseId,
        //                            ObjectId = StockHit.OrderId,
        //                            ObjectIdDetailId = StockHit.OrderDetailsId
        //                        });
        //                    }
        //                }
        //            }

        //            if (rtdStockList.Any())
        //            {
        //                BatchMasterManager batchMasterManager = new BatchMasterManager();

        //                bool ress = MultiStockHelpers.MakeEntry(rtdStockList, "Stock_OnPicked", context, scope);

        //                var StockTxnType = context.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "OrderPlannedOutCurrent" && x.IsDeleted == false);

        //                bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMasterList, context, people.PeopleID, StockTxnType.Id);
        //                if (!ress || !batchRes)
        //                {
        //                    if (!batchRes)
        //                    {
        //                        result.Message = "Can't Dispatched Due to batchstock qty not available!!";
        //                        return result;
        //                    }
        //                    else
        //                    {
        //                        result.Message = "Can't Dispatched, Due to qty";
        //                        return result;
        //                    }
        //                }
        //                else
        //                {
        //                    context.Commit();
        //                    result.Status = true;
        //                    result.Data = objOrderMaster;
        //                }
        //            }
        //        }
        //    }
        //    return result;
        //}

        [HttpGet]
        [Route("GetDelieveryBoy/{WarehouseId}")]
        public HttpResponseMessage GetDelieveryBoy(int WarehouseId)
        {
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var data = context.AllDBoyWid(GetCompanyId(), WarehouseId).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.GetBaseException().Message.ToString());

            }
        }

        [HttpGet]
        [Route("GetCustomerByMobile")]
        public CustomerDetailsDC GetCutmerDetail(string MobileNo, int WarehouseId)
        {
            CustomerDetailsDC result = new CustomerDetailsDC();
            GetCutomerDetailsDC FinalList = new GetCutomerDetailsDC();
            GetCutomerDetailsDC warehousemiscustomer = new GetCutomerDetailsDC();
            // string backendCustomerName = ConfigurationManager.AppSettings["BackendCustomerName"].ToString();
            using (var context = new AuthContext())
            {
                var customer = new Customer();
                if (MobileNo != null)
                {
                    var warehousedata = context.Warehouses.FirstOrDefault(x => x.WarehouseId == WarehouseId);
                    var MNo = new SqlParameter("@MobileNo", MobileNo);
                    FinalList = context.Database.SqlQuery<GetCutomerDetailsDC>("EXEC getCustomerbyMobile @MobileNo", MNo).FirstOrDefault();
                    if (FinalList != null)
                    {
                        if (FinalList.WarehouseId == WarehouseId)
                        {
                            result.Status = true;
                            result.Message = "Customer Found";
                            result.getCutomerDetailsDC = FinalList;
                        }
                        else
                        {
                            result.Status = true;
                            result.Message = "Customer Found";
                            warehousemiscustomer = Mapper.Map(FinalList).ToANew<GetCutomerDetailsDC>();
                            warehousemiscustomer.WarehouseId = WarehouseId;
                            result.getCutomerDetailsDC = warehousemiscustomer;
                        }

                        if (warehousedata.StoreType == 1 && result.getCutomerDetailsDC != null)
                            result.getCutomerDetailsDC.CustomerType = "Consumer";

                    }
                    else
                    {
                        result.Status = false;
                        result.Message = "New Customer ";
                        result.getCutomerDetailsDC = new GetCutomerDetailsDC();
                    }
                }
            }
            return result;
        }

        [HttpPost]
        [Route("GetOrderDetailByWarehouse")]
        public List<GetOrderByWarehouseDetail> GetOrderDetailByWarehouse(OrderDetailByWarehouse obj)
        {
            List<GetOrderByWarehouseDetail> FinalWareList = new List<GetOrderByWarehouseDetail>();
            // string backendCustomerName = ConfigurationManager.AppSettings["BackendCustomerName"].ToString();
            using (var context = new AuthContext())
            {
                OrderMaster orm = new OrderMaster();

                var WNo = new SqlParameter("@WarehouseId", obj.WarehouseId);
                var KeyValue1 = new SqlParameter("@KeyValue", obj.KeyValue == null ? DBNull.Value : (object)obj.KeyValue);
                var st = new SqlParameter("@Status", obj.Status == null ? DBNull.Value : (object)obj.Status);
                var fromdt = new SqlParameter("@Fromdate", obj.Fromdate == null ? DBNull.Value : (object)obj.Fromdate);
                var todt = new SqlParameter("@Todate", obj.Todate == null ? DBNull.Value : (object)obj.Todate);
                var skip1 = new SqlParameter("@skip", obj.skip);
                var take1 = new SqlParameter("@take", obj.take);
                context.Database.CommandTimeout = 300;
                FinalWareList = context.Database.SqlQuery<GetOrderByWarehouseDetail>("EXEC SP_GetOrderDetailByWarehouse @WarehouseId,@KeyValue,@Status,@Fromdate,@Todate, @skip, @take", WNo, KeyValue1, st, fromdt, todt, skip1, take1).ToList();

            }
            return FinalWareList;
        }


        [Route("GetBackendOrderInvoiceHtml")]
        [HttpPost]
        public async Task<string> GetOrderInvoiceHtml(int id, string Mobile)
        {
            OrderInvoice invoice = new OrderInvoice();
            string expiredHtml = string.Empty;
            string pathToHTMLFile = string.Empty; //HttpContext.Current.Server.MapPath("~/Templates") + "/CustomerBackendOrder.html";
            string content = string.Empty;//File.ReadAllText(pathToHTMLFile);
            int warehouseId, customerId = 0;
            using (var db = new AuthContext())
            {
                string filenamess = id + ".pdf";
                string ExcelSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/BO"), filenamess);

                if (!File.Exists(ExcelSavePath))
                {
                    OrderDispatchedMaster odm = new OrderDispatchedMaster();
                    var ordermaster = db.DbOrderMaster.Where(orm => orm.OrderId == id).Include(x => x.orderDetails).FirstOrDefault();
                    var warehousedata = db.Warehouses.FirstOrDefault(x => x.WarehouseId == ordermaster.WarehouseId);
                    if (warehousedata.StoreType != 1)
                    {
                        //if (ordermaster.OrderType == 11)
                        //{
                        //    pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/ConsumerTypeInvoice.html";
                        //    content = File.ReadAllText(pathToHTMLFile);
                        //}
                        //else
                        //{
                        pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/CustomerBackendOrder.html";
                        content = File.ReadAllText(pathToHTMLFile);
                        //}

                        odm = db.OrderDispatchedMasters.Where(x => x.OrderId == id).Include(x => x.orderDetails).FirstOrDefault();
                        customerId = odm.CustomerId;
                        warehouseId = odm.WarehouseId;
                        var warehouseDetail = db.Warehouses.FirstOrDefault(x => x.WarehouseId == warehouseId);
                        var Query = "exec GetOrderpayment " + id;
                        var paymentdetail = db.Database.SqlQuery<PaymentResponseRetailerAppDc>(Query).ToList();

                        var cust = db.Customers.FirstOrDefault(x => x.CustomerId == customerId);

                        var invoiceOrderOffer = new InvoiceOrderOffer();
                        List<InvoiceOrderOffer> invoiceOrderOffers = new List<InvoiceOrderOffer>();
                        var query = " select a.OrderId,b.OfferCode,b.ApplyOn,a.BillDiscountTypeValue,a.BillDiscountAmount from  BillDiscounts a  inner join Offers b on a.OfferId = b.OfferId  where a.orderid =  " + id + " and b.ApplyOn = 'PostOffer' Union all select orderid,'Flash Deal','',0,0 from FlashDealItemConsumeds a where a.orderid = " + id + " group by orderid";
                        invoiceOrderOffers = db.Database.SqlQuery<InvoiceOrderOffer>(query).ToList();
                        if (invoiceOrderOffers != null && invoiceOrderOffers.Any())
                        {
                            var offerCodes = invoiceOrderOffers.Select(x => x.OfferCode).ToList();
                            invoiceOrderOffer.OfferCode = string.Join(",", offerCodes);
                            double totalBillDicount = 0;
                            foreach (var item in invoiceOrderOffers)
                            {
                                if (item.BillDiscountAmount > 0)
                                    totalBillDicount += item.BillDiscountAmount;
                                else
                                    totalBillDicount += item.BillDiscountTypeValue;
                            }
                            invoiceOrderOffer.BillDiscountAmount = totalBillDicount > 0 ? totalBillDicount / 10 : 0;
                        }
                        // var CustomerCount = new OrderCountInfo();
                        var CustomerCount = (from i in db.Customers
                                             join k in db.DbOrderMaster on i.CustomerId equals k.CustomerId
                                             join com in db.Companies on i.CompanyId equals com.Id
                                             where k.OrderId == id && i.CustomerVerify == "Temporary Active"
                                             select new OrderCountInfo
                                             {
                                                 OrderCount = (db.DbOrderMaster.Where(x => x.CustomerId == i.CustomerId && x.CreatedDate >= com.InActiveCustomerCountDate && x.CreatedDate <= k.CreatedDate).Count()),
                                                 MaxOrderLimit = com.InActiveCustomerCount ?? 0,
                                             }).FirstOrDefault();
                        string KYCNote = "";

                        if (CustomerCount != null && CustomerCount.OrderCount > 0)
                        {
                            KYCNote = " Note:-Please complete your KYC.You are eligible for " + (CustomerCount.MaxOrderLimit - CustomerCount.OrderCount) + " more orders. Current Order Count = " + CustomerCount.OrderCount / CustomerCount.MaxOrderLimit;
                        }


                        var AddDatalists = odm.orderDetails.Where(z => z.OrderId == id).GroupBy(x => new { x.HSNCode }).Select(x => new getSuminvoiceHSNCodeDataDC
                        {
                            HSNCode = x.Key.HSNCode,
                            AmtWithoutTaxDisc = x.Sum(y => y.AmtWithoutTaxDisc),
                            SGSTTaxAmmount = x.Sum(y => y.SGSTTaxAmmount),
                            CGSTTaxAmmount = x.Sum(y => y.CGSTTaxAmmount),
                            TaxAmmount = x.Sum(y => y.TaxAmmount),
                            CessTaxAmount = x.Sum(y => y.CessTaxAmount),
                            TotalSum = x.Sum(y => y.AmtWithoutTaxDisc + y.SGSTTaxAmmount + y.CGSTTaxAmmount)
                        }).ToList();
                        string SumDataHSNDetailRows = "";
                        if (AddDatalists != null && AddDatalists.Count() > 0)
                        {
                            int rowNumber = 1;
                            foreach (var SumDataHSNDetail in AddDatalists)
                            {
                                SumDataHSNDetailRows += @"<tr>"
                                        //+ "<td>" + rowNumber.ToString() + "</td>"
                                        + "<td>" + SumDataHSNDetail.HSNCode + "</td>"
                                        + "<td>" + Math.Round(SumDataHSNDetail.AmtWithoutTaxDisc, 2) + "</td>"
                                        + "<td>" + (odm.IsIgstInvoice == false ? Math.Round(SumDataHSNDetail.CGSTTaxAmmount, 2) : 0) + "</td>"
                                        + "<td>" + (odm.IsIgstInvoice == false ? Math.Round(SumDataHSNDetail.SGSTTaxAmmount, 2) : 0) + "</td>"
                                        //+ "<td>" + (odm.IsIgstInvoice == true ? Math.Round((SumDataHSNDetail.TaxAmmount + SumDataHSNDetail.CessTaxAmount),2) : 0) + "</td>"
                                        //+ "<td>" + (SumDataHSNDetail.CessTaxAmount > 0 && odm.IsIgstInvoice == false ? Math.Round((SumDataHSNDetail.CessTaxAmount),2) : 0) + "</td>"
                                        + "<td>" + (Math.Round(SumDataHSNDetail.AmtWithoutTaxDisc, 2) + Math.Round(SumDataHSNDetail.SGSTTaxAmmount, 2) + Math.Round(SumDataHSNDetail.CGSTTaxAmmount, 2) + SumDataHSNDetail.CessTaxAmount) + "</td>"
                                    + "</tr>";

                                rowNumber++;
                            }
                        }
                        else
                        {
                            //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
                        }
                        string result = "";

                        bool Ainfo = true, Binfo = true;
                        if (cust.lat == 0 || cust.lg == 0)
                        {
                            Ainfo = false;
                        }
                        if (string.IsNullOrEmpty(cust.RefNo) && (string.IsNullOrEmpty(cust.UploadGSTPicture) || string.IsNullOrEmpty(cust.UploadLicensePicture)))
                        {
                            Binfo = false;
                        }
                        if (!Ainfo || !Binfo)
                            result = "Your Critical info " + (!Ainfo ? "GPS" : "") + (!Ainfo && !Binfo ? " & " : "") + (!Binfo ? "Shop Licence/GST#" : "") + " is Missing. Your account can be blocked anytime.";


                        odm.IsPrimeCustomer = ordermaster.IsPrimeCustomer;
                        odm.IsFirstOrder = ordermaster.IsFirstOrder;

                        #region offerdiscounttype
                        if (odm.BillDiscountAmount > 0)
                        {
                            var billdiscountOfferId = db.BillDiscountDb.Where(x => x.OrderId == odm.OrderId && x.CustomerId == odm.CustomerId).Select(z => z.OfferId).ToList();
                            if (billdiscountOfferId.Count > 0)
                            {
                                List<string> offeron = db.OfferDb.Where(x => billdiscountOfferId.Contains(x.OfferId)).Select(x => new { x.OfferOn, x.OfferCode }).ToList().Select(x => (!string.IsNullOrEmpty(x.OfferCode) ? x.OfferCode : x.OfferOn)).ToList();
                                odm.offertype = string.Join(",", offeron);
                            }
                        }
                        #endregion
                        //for igst case if true then apply condion to hide column of cgst sgst cess
                        if (!string.IsNullOrEmpty(odm.Tin_No) && odm.Tin_No.Length >= 11)
                        {
                            string CustTin_No = odm.Tin_No.Substring(0, 2);

                            //if (!CustTin_No.StartsWith("0"))
                            //{
                            odm.IsIgstInvoice = !db.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == odm.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No);
                            //}

                        }
                        if (odm != null)
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("IntValue");
                            var dr = dt.NewRow();
                            dr["IntValue"] = odm.CustomerId;
                            dt.Rows.Add(dr);
                            var param = new SqlParameter("CustomerId", dt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.IntValues";

                            var GetStateCodeList = db.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByCustomerId @CustomerId", param).FirstOrDefault();

                            if (GetStateCodeList != null)
                            {
                                odm.shippingStateName = GetStateCodeList.shippingStateName != null ? GetStateCodeList.shippingStateName : " ";
                                odm.shippingStateCode = GetStateCodeList.shippingStateCode != null ? GetStateCodeList.shippingStateCode : " "; ;
                                odm.BillingStateName = GetStateCodeList.BillingStateName != null ? GetStateCodeList.BillingStateName : " "; ;
                                odm.BillingStateCode = GetStateCodeList.BillingStateCode != null ? GetStateCodeList.BillingStateCode : " "; ;
                            }
                        }


                        if (odm != null)
                        {

                            odm.WalletAmount = odm.WalletAmount > 0 ? odm.WalletAmount : 0;
                            odm.offertype = odm.offertype != null ? odm.offertype : "";
                            odm.EwayBillNumber = odm.EwayBillNumber != null ? odm.EwayBillNumber : "";
                            odm.IRNQRCodeUrl = odm.IRNQRCodeUrl != null ? odm.IRNQRCodeUrl : "";
                            odm.POCIRNQRCodeURL = odm.POCIRNQRCodeURL != null ? odm.POCIRNQRCodeURL : "";
                            odm.IRNNo = odm.IRNNo != null ? odm.IRNNo : "";
                            odm.POCIRNNo = odm.POCIRNNo != null ? odm.POCIRNNo : "";
                            odm.PocCreditNoteNumber = odm.PocCreditNoteNumber != null ? odm.PocCreditNoteNumber : "";
                            odm.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee((decimal)odm.GrossAmount);
                        }
                        if (invoiceOrderOffer.OfferCode == null)
                        {
                            invoiceOrderOffer.OfferCode = "";
                        }
                        if (CustomerCount != null)
                        {
                            expiredHtml = content.Replace("[CustomerCount.MaxOrderLimit]", CustomerCount.MaxOrderLimit.ToString()).Replace("[CustomerCount.OrderCount]", CustomerCount.OrderCount.ToString());
                        }
                        odm.ShopName = odm.ShopName != null ? odm.ShopName : "";
                        odm.Tin_No = odm.Tin_No != null ? odm.Tin_No : "";
                        odm.invoice_no = odm.invoice_no != null ? odm.invoice_no : "";
                        odm.SalesMobile = odm.SalesMobile != null ? odm.SalesMobile : "";
                        odm.SalesPerson = odm.SalesPerson != null ? odm.SalesPerson : "";
                        odm.DeliveryIssuanceIdOrderDeliveryMaster = odm.DeliveryIssuanceIdOrderDeliveryMaster != null ? odm.DeliveryIssuanceIdOrderDeliveryMaster : 0;
                        odm.paymentThrough = odm.paymentThrough != null ? odm.paymentThrough : " ";

                        expiredHtml = content.Replace("[OrderData1.InvoiceBarcodeImage]", odm.InvoiceBarcodeImage.ToString())
                            .Replace("[FromWarehouseDetail.CompanyName]", warehouseDetail.CompanyName.ToString())
                            .Replace("[FromWarehouseDetail.GSTin]", warehouseDetail.GSTin.ToString())
                            .Replace("[FromWarehouseDetail.FSSAILicenseNumber]", warehouseDetail.FSSAILicenseNumber.ToString())
                            .Replace("[FromWarehouseDetail.Address]", warehouseDetail.Address.ToString())
                            .Replace("[FromWarehouseDetail.StateName]", warehouseDetail.StateName.ToString())
                            .Replace("[FromWarehouseDetail.Phone]", warehouseDetail.Phone.ToString())
                            .Replace("[OrderData1.ShopName]", odm.ShopName.ToString())
                            .Replace("[OrderData1.BillingAddress]", odm.BillingAddress.ToString())
                            .Replace("[OrderData1.Tin_No]", odm.Tin_No.ToString())
                            .Replace("[OrderData1.CustomerName]", odm.CustomerName.ToString())
                            .Replace("[OrderData1.Skcode]", odm.Skcode.ToString())
                            .Replace("[OrderData1.Customerphonenum]", odm.Customerphonenum.ToString())
                            .Replace("[OrderData1.BillingStateName]", odm.BillingStateName.ToString())
                            .Replace("[OrderData1.BillingStateCode]", odm.BillingStateCode.ToString())
                            //.Replace("[OrderData1.IsPrimeCustomer]", odm.IsPrimeCustomer.ToString())
                            .Replace("[OrderData1.ShippingAddress]", odm.ShippingAddress.ToString())
                            .Replace("[OrderData1.shippingStateName]", odm.shippingStateName.ToString())
                            .Replace("[OrderData1.shippingStateCode]", odm.shippingStateCode.ToString())
                            .Replace("[OrderData1.invoice_no]", odm.invoice_no.ToString())
                            .Replace("[OrderData1.CreatedDate]", odm.CreatedDate.ToString())
                            .Replace("[OrderData1.OrderId]", odm.OrderId.ToString())
                            .Replace("[OrderData1.OrderedDate]", odm.OrderedDate.ToString())
                            //.Replace("[OrderData1.PocCreditNoteDate]", odm.PocCreditNoteDate.ToString())
                            .Replace("[OrderData1.SalesPerson]", odm.SalesPerson.ToString())
                            .Replace("[OrderData1.SalesMobile]", odm.SalesMobile.ToString())
                            .Replace("[OrderData1.DboyName]", odm.DboyName.ToString())
                            .Replace("[OrderData1.DeliveryIssuanceIdOrderDeliveryMaster]", odm.DeliveryIssuanceIdOrderDeliveryMaster.ToString())
                            .Replace("[OrderData1.OrderType]", odm.OrderType.ToString())
                            .Replace("[OrderData1.IsIgstInvoice]", odm.IsIgstInvoice.ToString())
                            .Replace("[OrderData1.deliveryCharge]", odm.deliveryCharge.ToString())
                            .Replace("[OrderData1.paymentThrough]", odm.paymentThrough.ToString())
                            .Replace("[OrderData1.WalletAmount]", odm.WalletAmount.ToString())
                            .Replace("[OrderData1.PocCreditNoteNumber]", odm.PocCreditNoteNumber.ToString())
                            .Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[InvoiceAmountInWord]", odm.InvoiceAmountInWord.ToString())
                            .Replace("[OrderData1.BillDiscountAmount]", odm.BillDiscountAmount.ToString())
                            .Replace("[OrderData1.TCSAmount]", odm.TCSAmount.ToString())
                            .Replace("[OrderData1.GrossAmount]", odm.GrossAmount.ToString())
                            .Replace("[OrderData1.DiscountAmount]", odm.DiscountAmount.ToString())
                            .Replace("[OrderData1.Status]", odm.Status.ToString())
                            .Replace("[InvoiceOrderOffer.BillDiscountAmount]", invoiceOrderOffer.BillDiscountAmount.ToString())
                            .Replace("[InvoiceOrderOffer.OfferCode]", invoiceOrderOffer.OfferCode.ToString())
                            .Replace("[CustomerCriticalInfo]", result.ToString())
                            .Replace("[paymentdetail]", odm.Customerphonenum.ToString())
                            .Replace("[OrderData1.EwayBillNumber]", odm.EwayBillNumber.ToString())
                            .Replace("[OrderData1.offertype]", odm.offertype.ToString())
                            .Replace("[OrderData1.IRNQRCodeUrl]", odm.IRNQRCodeUrl.ToString())
                            .Replace("[OrderData1.POCIRNQRCodeURL]", odm.POCIRNQRCodeURL.ToString())
                                         //.Replace("[OrderData1.IRNNo]", odm.IRNNo.ToString())
                                         //.Replace("[OrderData1.POCIRNNo]", odm.POCIRNNo.ToString())
                                         ;




                        var ExecutiveIds = ordermaster.orderDetails.Where(z => z.ExecutiveId > 0).Select(z => z.ExecutiveId).Distinct().ToList();
                        if (ExecutiveIds != null && ExecutiveIds.Any())
                        {
                            var peoples = db.Peoples.Where(x => ExecutiveIds.Contains(x.PeopleID)).Select(x => new { x.DisplayName, x.Mobile }).ToList();
                            odm.SalesPerson = string.Join(",", peoples.Select(x => x.DisplayName));
                            odm.SalesMobile = string.Join(",", peoples.Select(x => x.Mobile));
                        }

                        odm.WalletAmount = odm.WalletAmount > 0 ? odm.WalletAmount : 0;
                        odm.offertype = odm.offertype != null ? odm.offertype : "";
                        odm.EwayBillNumber = odm.EwayBillNumber != null ? odm.EwayBillNumber : "";
                        odm.IRNQRCodeUrl = odm.IRNQRCodeUrl != null ? odm.IRNQRCodeUrl : "";
                        odm.POCIRNQRCodeURL = odm.POCIRNQRCodeURL != null ? odm.POCIRNQRCodeURL : "";
                        odm.IRNNo = odm.IRNNo != null ? odm.IRNNo : "";
                        odm.POCIRNNo = odm.POCIRNNo != null ? odm.POCIRNNo : "";
                        odm.PocCreditNoteNumber = odm.PocCreditNoteNumber != null ? odm.PocCreditNoteNumber : "";
                        var Amount = odm.GrossAmount - (odm.DiscountAmount > 0 ? odm.DiscountAmount : 0);
                        odm.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee((decimal)Amount);

                        if (invoiceOrderOffer.OfferCode == null)
                        {
                            invoiceOrderOffer.OfferCode = "";
                        }
                        double totalTaxableValue = 0;
                        double totalIGST = 0;
                        double totalCGST = 0;
                        double totalSGST = 0;
                        double totalCess = 0;
                        double TotalIOverall = 0;
                        double totalAmtIncTaxes = 0;
                        var OrderData = odm.orderDetails;
                        foreach (var i in OrderData)
                        {
                            totalTaxableValue = totalTaxableValue + i.AmtWithoutTaxDisc;
                            totalIGST = Math.Round(totalIGST + i.TaxAmmount + i.CessTaxAmount, 2);
                            totalCGST = Math.Round((totalCGST + i.CGSTTaxAmmount), 2);
                            totalSGST = Math.Round((totalSGST + i.SGSTTaxAmmount), 2);
                            totalCess = Math.Round((totalCess + i.CessTaxAmount), 2);
                            TotalIOverall = Math.Round(TotalIOverall + i.AmtWithoutTaxDisc + i.SGSTTaxAmmount + i.CGSTTaxAmmount + i.CessTaxAmount, 2);
                            totalAmtIncTaxes = Math.Round(totalAmtIncTaxes + i.TotalAmt, 2);
                        }
                        string OrderDataRows = "";
                        double TotalDetailQty = 0;

                        if (ordermaster.OrderType == 11)
                        {
                            if (OrderData != null && OrderData.Count() > 0)
                            {
                                int rowNumber = 1;
                                foreach (var orderDetail in OrderData)
                                {
                                    OrderDataRows += @"<tr>"
                                            + "<td>" + rowNumber.ToString() + "</td>"
                                            + "<td>" + orderDetail.itemname + (orderDetail.IsFreeItem ? "Free Item" : "") + "</td>"
                                            + "<td>" + orderDetail.price + "</td>"
                                            + "<td>" + ((orderDetail.UnitPrice == 0.0001 || orderDetail.UnitPrice == 0.01) ? (orderDetail.UnitPrice) : (orderDetail.UnitPrice)) + "</td>"
                                            + "<td>" + orderDetail.Noqty + "</td>"
                                            + "<td>" + Math.Round(orderDetail.AmtWithoutTaxDisc, 2) + "</td>"
                                            + "<td>" + orderDetail.HSNCode + "</td>"
                                              //+ "<td>" + orderDetail.DiscountAmmount + "</td>"
                                              // + "<td>" + orderDetail.HSNCode + "</td>"
                                              //+ "<td>" + (odm.IsIgstInvoice == true ? orderDetail.TaxPercentage + orderDetail.TotalCessPercentage : 0) + "</td>"
                                              //+ "<td>" + (odm.IsIgstInvoice == true ? Math.Round((orderDetail.TaxAmmount + orderDetail.CessTaxAmount),2) : 0) + "</td>"
                                              + "<td>" + (odm.IsIgstInvoice == false ? orderDetail.SGSTTaxPercentage : 0) + "</td>"
                                              + "<td>" + (odm.IsIgstInvoice == false ? Math.Round((orderDetail.SGSTTaxAmmount), 2) : 0) + "</td>"
                                              + "<td>" + (odm.IsIgstInvoice == false ? orderDetail.CGSTTaxPercentage : 0) + "</td>"
                                              + "<td>" + (odm.IsIgstInvoice == false ? Math.Round((orderDetail.CGSTTaxAmmount), 2) : 0) + "</td>"
                                              //+ "<td>" + (orderDetail.CessTaxAmount > 0 && odm.IsIgstInvoice == false ? orderDetail.TotalCessPercentage : 0) + "</td>"
                                              //+ "<td>" + (orderDetail.CessTaxAmount > 0 && odm.IsIgstInvoice == false ? orderDetail.CessTaxAmount : 0) + "</td>"
                                              + "<td>" + Math.Round(orderDetail.TotalAmt, 2) + "</td>"
                                        + "</tr>";
                                    TotalDetailQty = TotalDetailQty + orderDetail.Noqty;
                                    rowNumber++;
                                }
                            }
                            else
                            {
                                //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
                            }
                        }
                        else
                        {
                            if (OrderData != null && OrderData.Count() > 0)
                            {
                                int rowNumber = 1;
                                foreach (var orderDetail in OrderData)
                                {
                                    OrderDataRows += @"<tr>"
                                            + "<td>" + rowNumber.ToString() + "</td>"
                                            + "<td>" + orderDetail.itemname + (orderDetail.IsFreeItem ? "Free Item" : "") + "</td>"
                                            + "<td>" + orderDetail.price + "</td>"
                                            + "<td>" + (orderDetail.PTR > 0 && (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 > 0 ? (orderDetail.price / (1 + orderDetail.PTR / 100)) : orderDetail.UnitPrice) + "</td>"
                                            + "<td>" + (orderDetail.PTR > 0 && (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 > 0 ? (orderDetail.price / (1 + orderDetail.PTR / 100) - orderDetail.UnitPrice) / (orderDetail.price / (1 + orderDetail.PTR / 100)) * 100 : 0) + "</td>"
                                            + "<td>" + ((orderDetail.UnitPrice == 0.0001 || orderDetail.UnitPrice == 0.01) ? (orderDetail.UnitPrice) : (orderDetail.UnitPrice)) + "</td>"
                                            + "<td>" + orderDetail.MinOrderQty + "</td>"
                                            + "<td>" + Math.Round(Convert.ToDouble(orderDetail.qty) / orderDetail.MinOrderQty, 2) + "</td>"
                                            + "<td>" + orderDetail.Noqty + "</td>"
                                            + "<td>" + Math.Round(orderDetail.AmtWithoutTaxDisc, 2) + "</td>"
                                            + "<td>" + orderDetail.HSNCode + "</td>"
                                              //+ "<td>" + orderDetail.DiscountAmmount + "</td>"
                                              // + "<td>" + orderDetail.HSNCode + "</td>"
                                              //+ "<td>" + (odm.IsIgstInvoice == true ? orderDetail.TaxPercentage + orderDetail.TotalCessPercentage : 0) + "</td>"
                                              //+ "<td>" + (odm.IsIgstInvoice == true ? Math.Round((orderDetail.TaxAmmount + orderDetail.CessTaxAmount),2) : 0) + "</td>"
                                              + "<td>" + (odm.IsIgstInvoice == false ? orderDetail.SGSTTaxPercentage : 0) + "</td>"
                                              + "<td>" + (odm.IsIgstInvoice == false ? Math.Round((orderDetail.SGSTTaxAmmount), 2) : 0) + "</td>"
                                              + "<td>" + (odm.IsIgstInvoice == false ? orderDetail.CGSTTaxPercentage : 0) + "</td>"
                                              + "<td>" + (odm.IsIgstInvoice == false ? Math.Round((orderDetail.CGSTTaxAmmount), 2) : 0) + "</td>"
                                              //+ "<td>" + (orderDetail.CessTaxAmount > 0 && odm.IsIgstInvoice == false ? orderDetail.TotalCessPercentage : 0) + "</td>"
                                              //+ "<td>" + (orderDetail.CessTaxAmount > 0 && odm.IsIgstInvoice == false ? orderDetail.CessTaxAmount : 0) + "</td>"
                                              + "<td>" + Math.Round(orderDetail.TotalAmt, 2) + "</td>"
                                        + "</tr>";
                                    TotalDetailQty = TotalDetailQty + orderDetail.Noqty;
                                    rowNumber++;
                                }
                            }
                            else
                            {
                                //assignmentRows = @"<td colspan="5" style ='text -aligh=center'>No record found</td>";
                            }
                        }

                        string ordertype = "";
                        if (odm.OrderType == 1 || odm.OrderType == 0)
                        {
                            ordertype = "General order";
                        }
                        else if (odm.OrderType == 2)
                        {
                            ordertype = "Bundle order";
                        }
                        else if (odm.OrderType == 3)
                        {
                            ordertype = "Return order";
                        }
                        else if (odm.OrderType == 4)
                        {
                            ordertype = "Distributer order";
                        }
                        else if (odm.OrderType == 6)
                        {
                            ordertype = "Damage order";
                        }
                        else
                        {
                            ordertype = "General order";
                        }
                        string AmtFrom = "";
                        foreach (var item in paymentdetail)
                        {
                            AmtFrom += item.PaymentFrom + "  " + item.amount + " ₹ /-   ";

                        }

                        expiredHtml = content.Replace("[OrderData1.InvoiceBarcodeImage]", odm.InvoiceBarcodeImage.ToString()).Replace("[FromWarehouseDetail.CompanyName]", warehouseDetail.CompanyName.ToString()).Replace("[FromWarehouseDetail.GSTin]", warehouseDetail.GSTin.ToString()).Replace("[FromWarehouseDetail.FSSAILicenseNumber]", warehouseDetail.FSSAILicenseNumber.ToString()).Replace("[FromWarehouseDetail.Address]", warehouseDetail.Address.ToString()).Replace("[FromWarehouseDetail.StateName]", warehouseDetail.StateName.ToString()).Replace("[FromWarehouseDetail.Phone]", warehouseDetail.Phone.ToString()).Replace("[OrderData1.ShopName]", odm.ShopName.ToString()).Replace("[OrderData1.BillingAddress]", odm.BillingAddress.ToString()).Replace("[OrderData1.Tin_No]", odm.Tin_No.ToString()).Replace("[OrderData1.CustomerName]", odm.CustomerName.ToString())
                            .Replace("[OrderData1.Skcode]", odm.Skcode.ToString()).Replace("[OrderData1.Customerphonenum]", odm.Customerphonenum.ToString()).Replace("[OrderData1.BillingStateName]", odm.BillingStateName.ToString()).Replace("[OrderData1.BillingStateCode]", odm.BillingStateCode.ToString()).Replace("[OrderData1.IsPrimeCustomer]", odm.IsPrimeCustomer.ToString()).Replace("[OrderData1.ShippingAddress]", odm.ShippingAddress.ToString()).Replace("[OrderData1.shippingStateName]", odm.shippingStateName.ToString()).Replace("[OrderData1.shippingStateCode]", odm.shippingStateCode.ToString()).Replace("[OrderData1.invoice_no]", odm.invoice_no.ToString()).Replace("[OrderData1.CreatedDate]", odm.CreatedDate.ToString()).Replace("[OrderData1.OrderId]", odm.OrderId.ToString()).Replace("[OrderData1.OrderedDate]", odm.OrderedDate.ToString()).Replace("[OrderData1.PocCreditNoteDate]", odm.PocCreditNoteDate.ToString()).Replace("[OrderData1.SalesPerson]", odm.SalesPerson.ToString())
                            .Replace("[OrderData1.SalesMobile]", odm.SalesMobile.ToString()).Replace("[OrderData1.DboyName]", odm.DboyName.ToString()).Replace("[OrderData1.DeliveryIssuanceIdOrderDeliveryMaster]", odm.DeliveryIssuanceIdOrderDeliveryMaster.ToString()).Replace("[OrderData1.IsIgstInvoice]", odm.IsIgstInvoice.ToString()).Replace("[OrderData1.deliveryCharge]", odm.deliveryCharge.ToString()).Replace("[OrderData1.paymentThrough]", odm.paymentThrough.ToString())
                            .Replace("[OrderData1.WalletAmount]", odm.WalletAmount.ToString()).Replace("[OrderData1.PocCreditNoteNumber]", odm.PocCreditNoteNumber.ToString()).Replace("[CustomerCriticalInfo]", result.ToString()).Replace("[InvoiceAmountInWord]", odm.InvoiceAmountInWord.ToString())
                            .Replace("[OrderData1.BillDiscountAmount]", odm.BillDiscountAmount.ToString()).Replace("[OrderData1.TCSAmount]", odm.TCSAmount.ToString()).Replace("[OrderData1.GrossAmount]", odm.GrossAmount.ToString()).Replace("[OrderData1.DiscountAmount]", odm.DiscountAmount.ToString()).Replace("[OrderData1.Status]", odm.Status.ToString())
                            .Replace("[InvoiceOrderOffer.BillDiscountAmount]", invoiceOrderOffer.BillDiscountAmount.ToString())
                            .Replace("[InvoiceOrderOffer.OfferCode]", invoiceOrderOffer.OfferCode.ToString())
                            .Replace("[CustomerCriticalInfo]", result.ToString())
                        .Replace("[OrderData1.EwayBillNumber]", odm.EwayBillNumber.ToString()).Replace("[OrderData1.offertype]", odm.offertype.ToString())
                        .Replace("[OrderData1.IRNQRCodeUrl]", odm.IRNQRCodeUrl.ToString()).Replace("[OrderData1.POCIRNQRCodeURL]", odm.POCIRNQRCodeURL.ToString()).Replace("[OrderData1.IRNNo]", odm.IRNNo.ToString())
                        .Replace("[OrderData1.POCIRNNo]", odm.POCIRNNo.ToString())
                        .Replace("[InvoiceAmt]", (odm.GrossAmount - odm.DiscountAmount).ToString())
                        .Replace("[amount]", (paymentdetail[0].amount > 0 ? paymentdetail[0].amount : paymentdetail[0].amount).ToString())
                        .Replace("[PaymentFrom]", (paymentdetail[0].PaymentFrom != null ? paymentdetail[0].PaymentFrom : paymentdetail[0].PaymentFrom).ToString())
                        .Replace("[IsOnline]", (paymentdetail[0].amount > 0 && paymentdetail[0].IsOnline ? "Paid" : "Refund"))
                        .Replace("##SumDataHSNROWS##", SumDataHSNDetailRows)
                        .Replace("##OrderDataRows##", OrderDataRows)
                        //.Replace("##getOrderDataRows##", getOrderDataRows)
                        .Replace("[PaymentAmtFrom]", AmtFrom)
                        .Replace("[OrderData1.OrderType]", ordertype)
                        .Replace("[totalTaxableValue]", Math.Round(totalTaxableValue, 2).ToString())
                        .Replace("[totalIGST]", totalIGST.ToString())
                        .Replace("[totalCGST]", totalCGST.ToString())
                        .Replace("[totalSGST]", totalSGST.ToString())
                        .Replace("[totalCess]", totalCess.ToString())
                        .Replace("[TotalIOverall]", TotalIOverall.ToString())
                        .Replace("[KYCNote]", KYCNote.ToString())
                        //.Replace("[OrderLimit]", (CustomerCount != null ? (CustomerCount.MaxOrderLimit - CustomerCount.OrderCount).ToString() : ""))
                        // .Replace("[CurrentorderCount]", (CustomerCount != null ? (CustomerCount.OrderCount / CustomerCount.MaxOrderLimit).ToString() : ""))
                        //.Replace("[CustomerCount.MaxOrderLimit]", (CustomerCount != null ? CustomerCount.MaxOrderLimit.ToString() : ""))
                        //.Replace("[CustomerCount.OrderCount]", (CustomerCount != null ? CustomerCount.OrderCount.ToString() : ""))
                        .Replace("[TotalDetailQty]", TotalDetailQty.ToString());
                    }
                    else
                    {
                        pathToHTMLFile = HttpContext.Current.Server.MapPath("~/Templates") + "/ZilaInvoices.html";
                        content = File.ReadAllText(pathToHTMLFile);
                        List<GetBackendZilaInvoiceDataDc> getBackendZilaInvoiceDataDcs = new List<GetBackendZilaInvoiceDataDc>();
                        int TotalQty = 0;
                        double AmtInclueTaxes = 0;
                        odm = db.OrderDispatchedMasters.Where(x => x.OrderId == id).Include(x => x.orderDetails).FirstOrDefault();
                        var Query = "exec GetOrderpayment " + id;
                        var paymentdetail = db.Database.SqlQuery<PaymentResponseRetailerAppDc>(Query).ToList();
                        OrderDispatchedMasterController ctrl = new OrderDispatchedMasterController();
                        string orderids = Convert.ToString(id);
                        string OrderDataRows = "";

                        //string path = HttpContext.Current.Server.MapPath("~/Content") + "/ZilaLogo.svg";
                        string image = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                         , HttpContext.Current.Request.Url.DnsSafeHost
                                                                         , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                         , "/Content/ZilaLogo.svg");
                        string imagepath = null;
                        
                        imagepath = "<img src='" + image + "'width='220px' height='110px'/>";
                             TextFileLogHelper.TraceLog("image is " + imagepath);

                        string barcodepath = null;

                        getBackendZilaInvoiceDataDcs = ctrl.GetBackendZilaInvoiceData(orderids);
                        var hsnsummarydata = getBackendZilaInvoiceDataDcs.Select(x => x.HsnSummaryDCs).FirstOrDefault();
                        int count = 0;
                        int counts = 0;
                        string PaymentrefNO = "";
                        string PaymentData = "";
                        string SumDataHSNDetailRows = "";
                        bool HideCessColumn = false;
                        double totaltaxablevalue = 0;
                        double cgstamount = 0;
                        double sgstamount = 0;
                        double isgtamount = 0;
                        double cessamoount = 0;
                        double gettotaloverall = 0;
                        double getTotalTaxableValue = 0;
                        double totalTaxableValue = 0;
                        if (odm.orderDetails.Any(x => x.CGSTTaxAmmount > 0))
                        {
                            cgstamount = odm.orderDetails.Sum(x => x.CGSTTaxAmmount);
                        }
                        if (odm.orderDetails.Any(x => x.SGSTTaxAmmount > 0))
                        {
                            sgstamount = odm.orderDetails.Sum(x => x.SGSTTaxAmmount);
                        }
                        if (odm.orderDetails.Any(x => x.TaxAmmount > 0 || x.CessTaxAmount > 0))
                        {
                            double taxamount = odm.orderDetails.Sum(x => x.TaxAmmount);
                            double cesstaxamount = odm.orderDetails.Sum(x => x.CessTaxAmount);
                            isgtamount = taxamount + cesstaxamount;
                        }
                        getTotalTaxableValue = odm.orderDetails.Sum(x => x.AmtWithoutTaxDisc);
                        cessamoount = odm.orderDetails.Sum(x => x.CessTaxAmount);
                        gettotaloverall = odm.orderDetails.Sum(x => x.AmtWithoutTaxDisc) + odm.orderDetails.Sum(x => x.SGSTTaxAmmount) + odm.orderDetails.Sum(x => x.CGSTTaxAmmount) + odm.orderDetails.Sum(x => x.CessTaxAmount);
                        if (ordermaster.orderDetails.Any(x => x.CessTaxAmount > 0))
                        {
                            HideCessColumn = true;
                        }
                        foreach (var item in getBackendZilaInvoiceDataDcs)
                        {
                            string letter = "";
                            letter = IndexToLetter(count);
                            OrderDataRows += @"<tr>"
                                            + "<td colspan='4' style='text-align:left; font-family: Calibri!important;'>" + letter + "&nbsp; CGST@" + item.Cgst + "% SGST@" + item.Cgst + "%" + "</td>"
                                        + "</tr>";
                            count++;
                            foreach (var i in item.OrderDetails)
                            {
                                TotalQty += i.Noqty;
                                AmtInclueTaxes += i.TotalAmountAfterTaxDisc;
                                i.UnitPrice = (i.UnitPrice == 0.0001 || i.UnitPrice == 0.01) ? Math.Round((i.UnitPrice), 4) : Math.Round((i.UnitPrice), 2);
                                OrderDataRows += @"<tr>"
                                                + "<td style='width:26%; font-size:smaller!important; font-family: Calibri!important; white-space:nowrap; text-align:left;'>" +
                                                i.itemNumber + " <br/>" + i.itemname + "</td>" +
                                                "<td style='text-align: center; font-size: smaller!important; font-family: Calibri!important;'>" +
                                                +i.Noqty + "</td>" +
                                                "<td style='font-size:smaller!important; font-family:Calibri!important; text-align:center;'>" +
                                                +i.UnitPrice + "<br/>"
                                                + i.HSNCode + "</td>" +
                                                "<td style='font-size:smaller!important; font-family: Calibri!important; text-align:center;'>" +
                                                +Math.Round(i.TotalAmountAfterTaxDisc, 2) + "<br/>" + Math.Round(i.AmtWithoutTaxDisc, 2) + "</td>  </tr> ";
                                //style = font - size: smaller!important; font - family: Calibri!important; border - bottom: 1px dotted #000 !important;
                            }

                        }
                        if (hsnsummarydata != null && hsnsummarydata.Any())
                        {
                            foreach (var i in hsnsummarydata)
                            {
                                string letters = "";
                                letters = IndexToLetter(counts);
                                totaltaxablevalue += i.TaxableAmount;
                                SumDataHSNDetailRows += @"<tr>" + "<td style='text-align:center!important'>" + letters + "</td>" +
                                    "<td style='text-align:center!important'>" + Math.Round(i.TaxableAmount, 2) + "</td>" +
                                    "<td style='text-align:center!important'>" + (odm.IsIgstInvoice == false ? Math.Round(i.CgstPercent, 2) : 0) + "</td>" +
                                    "<td style='text-align:center!important'>" + (odm.IsIgstInvoice == false ? Math.Round(i.CgstPercent, 2) : 0) + "</td>" +
                                      //"<td>" + (odm.IsIgstInvoice == true ? Math.Round(i.IgstPercent, 2):0)   + "</td>" +
                                      "<td style='text-align:center!important'>" + Math.Round(i.TaxableAmount, 2) + "</td> " + "</tr>";
                                counts++;
                            }
                        }
                        foreach (var item in paymentdetail)
                        {
                            if (!string.IsNullOrEmpty(item.GatewayTransId))
                            {
                                PaymentrefNO += item.GatewayTransId;
                            }
                            if (item.amount > 0)
                            {
                                PaymentData += "<span style='font - size: small!important; font - family: Calibri!important'> " + item.PaymentFrom + " </span>";
                            }
                            if (item.amount < 0 && item.IsOnline)
                            {
                                PaymentData += "<span >," + item.PaymentFrom + "</span><span > &nbsp;</span> ";
                            }
                            if (item.amount > 0)
                            {
                                PaymentData += "<span style = 'font - size: small!important; font - family: Calibri!important' >" + item.amount + "&nbsp;₹ / -";
                                if (item.IsOnline)
                                {
                                    PaymentData += "<span  style = 'font-size: small !important; font-family: Calibri !important' >(Paid)</span> ";
                                }
                                PaymentData += "</span>";
                            }
                            //if(item.amount <0 && item.IsOnline)
                            //{
                            //    PaymentData += "< span  style = font - size: small!important; font - family: Calibri!important >" + item.refundAmount +
                            //        "&nbsp;₹ / -(Refund)</ span > ";
                            //}



                        }
                        double totalCGST = 0;
                        double totalSGST = 0;
                        double TotalIOverall = 0;
                        //TotalSumDataHSNDetailRows += "<td style='border - top:1px dotted black!important; border - bottom: 1px dotted black!important'><b>" + Math.Round(getTotalTaxableValue, 2) + "</b></td>" +
                        //                             //"<td style = 'border-top:1px dotted black !important; border-bottom: 1px dotted black !important'>" + (odm.IsIgstInvoice == true ? Math.Round(isgtamount, 2):0) + "</td>" +
                        //                             "<td style = 'border-top:1px dotted black !important; border-bottom: 1px dotted black !important'>" + (odm.IsIgstInvoice == false ? Math.Round(cgstamount, 2):0)+"</td>" +
                        //                             "<td style = 'border-top:1px dotted black !important; border-bottom: 1px dotted black !important'>" + (odm.IsIgstInvoice == false ? Math.Round(sgstamount, 2):0)  + "</td>" +
                        //                             //"<td style = 'border-top: 1px dotted black !important; border-bottom: 1px dotted black !important'>" + ((odm.IsIgstInvoice == false && HideCessColumn == true) ? Math.Round(cessamoount, 2):0) +"</td>" +
                        //                             "<td style = 'border-top: 1px dotted black !important; border-bottom: 1px dotted black !important'>" + Math.Round(gettotaloverall, 2) + "</td> ";
                        totalTaxableValue = Math.Round(getTotalTaxableValue, 2);
                        totalCGST = (odm.IsIgstInvoice == false ? Math.Round(cgstamount, 2) : 0);
                        totalSGST = (odm.IsIgstInvoice == false ? Math.Round(sgstamount, 2) : 0);
                        TotalIOverall = Math.Round(gettotaloverall, 2);
                        temOrderQBcode code = new temOrderQBcode();
                        string invoiceimage = null;
                        byte[] mybytearray = null;
                        if (!string.IsNullOrEmpty(ordermaster.invoice_no) && ordermaster.OrderType == 11)
                        {
                            code = db.GetBarcode(ordermaster.invoice_no);
                            mybytearray = code.BarcodeImage;
                            var Jsonstring = Convert.ToBase64String(mybytearray);
                            barcodepath = "<img src='data:image/png;base64," + Jsonstring + "' width='220px' height='80px' />";
                            //string utfString = Encoding.UTF8.GetString(code.BarcodeImage, 0, code.BarcodeImage.Length);
                            //a = Encoding.UTF8.GetBytes(code.BarcodeImage.ToString());
                        }

                        expiredHtml = content.Replace("[FromWarehouseDetail.Address]", warehousedata.Address.ToString())
                        .Replace("[FromWarehouseDetail.Phone]", warehousedata.Phone.ToString())
                        .Replace("[FromWarehouseDetail.GSTin]", warehousedata.GSTin.ToString())
                        .Replace("[OrderData1.invoice_no]", odm.invoice_no.ToString())
                        .Replace("[OrderData1.CreatedDate]", odm.CreatedDate.ToString())
                        .Replace("[OrderData1.OrderId]", odm.OrderId.ToString())
                        .Replace("[OrderData1.Skcode]", odm.Skcode.ToString())
                        .Replace("[OrderData1.Customerphonenum]", odm.Customerphonenum.ToString())
                        .Replace("[TotalQty]", TotalQty.ToString())
                        .Replace("[AmtInclueTaxes]", AmtInclueTaxes.ToString())
                        .Replace("[OrderData1.BillDiscountAmount]", odm.BillDiscountAmount.ToString())
                        .Replace("[OrderData1.WalletAmount]", odm.WalletAmount.ToString())
                        .Replace("[OrderData1.GrossAmount]", odm.GrossAmount.ToString())
                        .Replace("[PaymentrefNO]", PaymentrefNO.ToString())
                        .Replace("[PaymentData]", PaymentData.ToString())
                        .Replace("[OrderData1.IsIgstInvoice]", odm.IsIgstInvoice.ToString())
                        .Replace("[HideCessColumn]", HideCessColumn.ToString())
                        .Replace("[totalTaxableValue]", totalTaxableValue.ToString())
                        .Replace("[totalCGST]", totalCGST.ToString())
                        .Replace("[totalSGST]", totalSGST.ToString())
                        .Replace("[imagepath]", imagepath.ToString())
                        .Replace("[barcodepath]", barcodepath.ToString())
                        .Replace("[TotalIOverall]", TotalIOverall.ToString())
                        //.Replace("[OrderData1.InvoiceBarcodeImage]", a.ToString())
                        .Replace(" ##OrderDetailRow##", OrderDataRows)
                        .Replace(" ##SumDataHSNDetailRows##", SumDataHSNDetailRows);
                    }
                    if (!string.IsNullOrEmpty(expiredHtml))
                    {
                        string fileUrl = "";
                        string fullPhysicalPath = "";
                        string thFileName = "";
                        string TartgetfolderPath = "";

                        TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\BO");
                        if (!Directory.Exists(TartgetfolderPath))
                            Directory.CreateDirectory(TartgetfolderPath);


                        thFileName = id + ".pdf";
                        fileUrl = "/BO" + "/" + thFileName;
                        fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;

                        var OutPutFile = Path.Combine(HttpContext.Current.Server.MapPath("~/BO"), thFileName);

                        byte[] pdf = null;



                        pdf = Pdf
                         .From(expiredHtml)
                         //.WithGlobalSetting("orientation", "Landscape")
                         //.WithObjectSetting("web.defaultEncoding", "utf-8")
                         .OfSize(OpenHtmlToPdf.PaperSize.A4)
                         .WithTitle("Invoice")
                         .WithoutOutline()
                         .WithMargins(PaperMargins.All(0.0.Millimeters()))
                         .Portrait()
                         .Comressed()
                         .Content();


                        FileStream file = File.Create(OutPutFile);
                        file.Write(pdf, 0, pdf.Length);
                        file.Close();
                        if (!string.IsNullOrEmpty(odm.Customerphonenum))
                        {
                            bool isSent = false;
                            string Message = "";
                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "StoreOrderDelivered");
                            Message = dltSMS == null ? "" : dltSMS.Template;
                            Message = Message.Replace("{#var1#}", "User");
                            Message = Message.Replace("{#var2#}", odm.OrderId.ToString());
                            string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                             , HttpContext.Current.Request.Url.DnsSafeHost
                                                                             , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                        , "/BO/" + odm.OrderId + ".pdf");
                            TextFileLogHelper.LogError("Error BackEndOrder FileUrl" + FileUrl);

                            //var isGdApi = new IsGdAPI();
                            //string shortUrl = await isGdApi.ShortenUrlAsync(FileUrl);
                            //if (shortUrl == null)
                            //    shortUrl = "";
                            string shortUrl = Helpers.ShortenerUrl.ShortenUrl(FileUrl);
                            //string shortUrl = @is.gd.Url.GetShortenedUrl("https://uat.shopkirana.in/BO/139.pdf", v: true).Result;
                            TextFileLogHelper.LogError("Error BackEndOrder shortUrl" + shortUrl);
                            Message = Message.Replace("{#var3#}", shortUrl);
                            TextFileLogHelper.LogError("Error BackEndOrder SMS" + Message);
                            if (dltSMS != null)
                                isSent = Common.Helpers.SendSMSHelper.SendSMS(odm.Customerphonenum, Message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                            if (isSent)
                            {
                                expiredHtml = "Sent Succesfullly.";
                            }
                            else
                            {
                                expiredHtml = "Not Sent.";
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(Mobile))
                {
                    bool isSent = false;
                    string Message = "";
                    var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.Others, "StoreOrderDelivered");
                    Message = dltSMS == null ? "" : dltSMS.Template;
                    Message = Message.Replace("{#var1#}", "User");
                    Message = Message.Replace("{#var2#}", id.ToString());
                    string FileUrl = string.Format("{0}://{1}{2}/{3}", new Uri((HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.AbsoluteUri : HttpContext.Current.Request.Url.AbsoluteUri)).Scheme
                                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                                     , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                , "/BO/" + id + ".pdf");
                    TextFileLogHelper.LogError("Error BackEndOrder FileUrl" + FileUrl);

                    string shortUrl = Helpers.ShortenerUrl.ShortenUrl(FileUrl);
                    //FileUrl = "https://uat.shopkirana.in/BO/139.pdf";
                    //var isGdApi = new IsGdAPI();
                    //string shortUrl = await isGdApi.ShortenUrlAsync(FileUrl);
                    //if (shortUrl == null)
                    //    shortUrl = "";

                    //string shortUrl =  @is.gd.Url.GetShortenedUrl("https://uat.shopkirana.in/BO/139.pdf", v: true).Result;
                    TextFileLogHelper.LogError("Error BackEndOrder shortUrl" + shortUrl);
                    Message = Message.Replace("{#var3#}", shortUrl);
                    TextFileLogHelper.LogError("Error BackEndOrder SMS" + Message);
                    if (dltSMS != null)
                        isSent = Common.Helpers.SendSMSHelper.SendSMS(Mobile, Message, ((Int32)Common.Enums.SMSRouteEnum.Transactional).ToString(), dltSMS.DLTId);
                    if (isSent)
                    {
                        expiredHtml = "Sent Succesfullly.";
                    }
                    else
                    {
                        expiredHtml = "Not Sent.";
                    }
                }
                else
                {
                    expiredHtml = "Already sent.";
                }

            }

            return expiredHtml;
        }

        [Route("GetQrEnabledData")]
        [HttpGet]
        public async Task<WarehouseStoreDC> GetQrEnabledData(int warehouseid)
        {
            WarehouseStoreDC res = new WarehouseStoreDC();
            using (var db = new AuthContext())
            {
                res = db.Warehouses.Where(x => x.WarehouseId == warehouseid).Select(y => new WarehouseStoreDC { IsQrEnabled = y.IsQREnabled, Storetype = y.StoreType }).FirstOrDefault();
            }
            return res;
        }


        [Route("CheckDuplicateGST")]
        [HttpGet]
        public GSTResponse CheckDuplicateGST(string Mobile, string GST)
        {
            GSTResponse result = new GSTResponse();
            string msg = "";
            using (var db = new AuthContext())
            {
                if (Mobile != null && GST != null)
                {
                    var res = db.Customers.FirstOrDefault(x => x.Mobile != Mobile && x.RefNo == GST);
                    if (res != null)
                    {
                        result.Status = false;
                        result.msg = "This GST no. already exist.";
                    }
                    else
                    {
                        result.Status = true;
                    }
                }
                else
                {
                    result.Status = false;
                    result.msg = "Something went wrong";
                }
            }
            return result;
        }

        private int GetUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            return userid;
        }

        private int GetCompanyId()
        {
            var identity = User.Identity as ClaimsIdentity;
            int CompId = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                CompId = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
            return CompId;
        }

        public double GetConsumerPrice(AuthContext context, int ItemMultiMRPId, double MRP, double UnitPrice, int Warehouseid)
        {
            double consumerPrice = MRP;
            if (ItemMultiMRPId > 0 && Warehouseid > 0)
            {
                var data = context.StorePriceConfigurationDb.Where(x => x.WarehouseId == Warehouseid && x.ItemMultiMRPId == ItemMultiMRPId && x.IsActive == true && x.IsDeleted == false && x.Percentage > 0 && x.Type > 0).FirstOrDefault();
                if (data != null)
                {
                    if (data.Type == 1)
                    {
                        consumerPrice = MRP - ((MRP * data.Percentage) / 100);
                    }
                    else if (data.Type == 2)
                    {
                        consumerPrice = UnitPrice + ((UnitPrice * data.Percentage) / 100);
                    }
                    if (consumerPrice > MRP)
                    {
                        consumerPrice = MRP;
                    }
                }
            }
            return consumerPrice;
        }

        [Route("Searchiteminstoreconfig")]
        [HttpGet]
        public List<SearchItemforStore> SearchItem(string keyword)
        {
            using (var db = new AuthContext())
            {
                List<SearchItemforStore> res = new List<SearchItemforStore>();
                var kd = new SqlParameter("@keyword", keyword);
                res = db.Database.SqlQuery<SearchItemforStore>("EXEC Sp_getsearchitemforstore @keyword", kd).ToList();
                return res;
            }
        }

        [Route("Editstoreconfig")]
        [HttpGet]
        public string Editstoreconfig(long Id, int Type, double Percentage)
        {
            using (var context = new AuthContext())
            {
                int userid = 0;
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                string res = "";
                if (Type > 0 && Percentage > 0)
                {
                    var data = context.StorePriceConfigurationDb.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                    if (data != null)
                    {
                        data.Type = Type;
                        data.Percentage = Percentage;
                        data.ModifiedBy = userid;
                        data.ModifiedDate = DateTime.Now;
                        context.Entry(data).State = EntityState.Modified;
                        if (context.Commit() > 0)
                        {
                            res = "Updated Successfully";
                        }
                        else
                        {
                            res = "Something Went wrong";
                        }
                    }
                    else
                    {
                        res = "Data Not Found";
                    }
                }
                else
                {
                    res = "Please select Correct Type and Percentage";
                }

                return res;
            }
        }

        [Route("Deletestoreconfig")]
        [HttpGet]
        public string Deletestoreconfig(long Id)
        {
            using (var context = new AuthContext())
            {
                int userid = 0;
                var identity = User.Identity as ClaimsIdentity;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                string res = "";

                var data = context.StorePriceConfigurationDb.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                if (data != null)
                {
                    data.IsActive = false;
                    data.IsDeleted = true;
                    data.ModifiedBy = userid;
                    data.ModifiedDate = DateTime.Now;
                    context.Entry(data).State = EntityState.Modified;
                    if (context.Commit() > 0)
                    {
                        res = "Deleted Successfully";
                    }
                    else
                    {
                        res = "Something Went wrong";
                    }
                }
                else
                {
                    res = "Data Not Found";
                }
                return res;
            }
        }

        [Route("GetStoreConfig")]
        [HttpPost]
        public List<GetStoreConfigDC> GetStoreConfig(GetStoreConfigPayload payload)
        {
            using (var db = new AuthContext())
            {
                List<GetStoreConfigDC> res = new List<GetStoreConfigDC>();
                var mrplist = new DataTable();
                mrplist.Columns.Add("IntValue");
                foreach (var obj in payload.ItemmultimrpId)
                {
                    var dr = mrplist.NewRow();
                    dr["IntValue"] = obj;
                    mrplist.Rows.Add(dr);
                }
                var param2 = new SqlParameter("@MrpIds", mrplist);
                param2.SqlDbType = SqlDbType.Structured;
                param2.TypeName = "dbo.IntValues";
                var param3 = new SqlParameter("@WarehouseId", payload.WarehouseId);
                var param4 = new SqlParameter("@Skip", payload.Skip);
                var param5 = new SqlParameter("@Take", payload.Take);
                res = db.Database.SqlQuery<GetStoreConfigDC>("exec Sp_getstoreconfig @MrpIds, @WarehouseId,@Skip,@Take", param2, param3, param4, param5).ToList();
                return res;
            }
        }

        [Route("AddnewStopConfig")]
        [HttpPost]
        public string AddnewStopConfig(AddStoreConfigDC payload)
        {
            string res = "Data not Added";
            if (payload.WarehouseId < 0)
            {
                res = "Please Select WarehouseId";
            }
            else if (payload.Type < 0)
            {
                res = "Please Select Type";
            }
            else if (payload.Type < 0)
            {
                res = "Please Select Type";
            }
            else if (payload.Percentage < 0)
            {
                res = "Please Select Percentage";
            }
            else if (payload.Percentage > 100)
            {
                res = "Please Select Percentage less then 100";
            }
            else
            {
                var mrpids = payload.ItemmultimrpId.ToList();
                if (mrpids != null && mrpids.Count > 0 && mrpids.Any())
                {
                    using (var db = new AuthContext())
                    {
                        int userid = 0;
                        var identity = User.Identity as ClaimsIdentity;
                        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                        var data = db.StorePriceConfigurationDb.Where(x => x.WarehouseId == payload.WarehouseId && mrpids.Contains(x.ItemMultiMRPId) && x.IsActive == true && x.IsDeleted == false).ToList();
                        if (data != null && data.Count() > 0 && data.Any())
                        {
                            string result = "";
                            var mrplist = data.Select(x => x.ItemMultiMRPId).ToList();
                            result = String.Join(",", mrplist);
                            res = "MrpIds is exist for this ItemmultiMrpId" + result;
                        }
                        else
                        {
                            List<StorePriceConfiguration> list = new List<StorePriceConfiguration>();
                            foreach (var item in payload.ItemmultimrpId)
                            {
                                StorePriceConfiguration s = new StorePriceConfiguration();
                                s.ItemMultiMRPId = item;
                                s.Type = payload.Type;
                                s.Percentage = payload.Percentage;
                                s.WarehouseId = payload.WarehouseId;
                                s.CreatedBy = userid;
                                s.CreatedDate = DateTime.Now;
                                s.IsDeleted = false;
                                s.IsActive = true;
                                list.Add(s);
                            }

                            if (list != null && list.Any())
                            {
                                db.StorePriceConfigurationDb.AddRange(list);
                                if (db.Commit() > 0)
                                {
                                    res = "Add Successfully";
                                }
                                else
                                {
                                    res = "Something Went Wrong";
                                }
                            }
                            else
                            {
                                res = "please Select Something";
                            }
                        }
                    }
                }
                else
                {
                    res = "Please Select MrpIds";
                }
            }
            return res;
        }


        string col0, col1, col2;
        [Route("UploadStorePriceConfiguration")]
        [HttpPost]
        public IHttpActionResult UploadStorePriceConfiguration(int wid)
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                try
                {
                    DateTime cdate = DateTime.Now;
                    var formData1 = HttpContext.Current.Request.Form["compid"];
                    // Access claims
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    string MSG = "";
                    if (httpPostedFile != null)
                    {
                        using (AuthContext context = new AuthContext())
                        {
                            string ext = Path.GetExtension(httpPostedFile.FileName);
                            if (ext == ".xlsx" || ext == ".xls")
                            {
                                string path = HttpContext.Current.Server.MapPath("~/UploadedFiles/StorePriceConfiguration");
                                string a1, b;

                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                                a1 = DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + httpPostedFile.FileName;
                                b = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/StorePriceConfiguration/"), a1);
                                httpPostedFile.SaveAs(b);

                                byte[] buffer = new byte[httpPostedFile.ContentLength];

                                using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                                {
                                    br.Read(buffer, 0, buffer.Length);
                                }
                                XSSFWorkbook hssfwb;
                                List<StorePriceDC> uploaditemlist = new List<StorePriceDC>();
                                using (MemoryStream memStream = new MemoryStream())
                                {
                                    BinaryFormatter binForm = new BinaryFormatter();
                                    memStream.Write(buffer, 0, buffer.Length);
                                    memStream.Seek(0, SeekOrigin.Begin);
                                    hssfwb = new XSSFWorkbook(memStream);
                                    string sSheetName = hssfwb.GetSheetName(0);
                                    ISheet sheet = hssfwb.GetSheet(sSheetName);
                                    IRow rowData;
                                    ICell cellData = null;

                                    int? PriceTypeIndex = null;
                                    int? ItemmultimrpIndex = null;
                                    int? PercentageIndex = null;


                                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                                    {

                                        if (iRowIdx == 0)
                                        {
                                            rowData = sheet.GetRow(iRowIdx);
                                            if (rowData != null)
                                            {
                                                string strJSON = null;
                                                string field = string.Empty;
                                                field = rowData.GetCell(0).ToString();
                                                //WarehousenameIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Warehouse Name") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Warehouse Name").ColumnIndex : (int?)null;
                                                //if (!WarehousenameIndex.HasValue)
                                                //{
                                                //    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Warehouse Name does not exist..try again");
                                                //    return Created(strJSON, strJSON);
                                                //}

                                                ItemmultimrpIndex = rowData.Cells.Any(x => x.ToString().Trim() == "ItemMultiMrpId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "ItemMultiMrpId").ColumnIndex : (int?)null;

                                                if (!ItemmultimrpIndex.HasValue)
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("ItemMultiMrpId does not exist..try again");
                                                    return Created(strJSON, strJSON);
                                                }

                                                PriceTypeIndex = rowData.Cells.Any(x => x.ToString().Trim() == "PriceType") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "PriceType").ColumnIndex : (int?)null;

                                                if (!PriceTypeIndex.HasValue)
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("PriceType does not exist..try again");
                                                    return Created(strJSON, strJSON);
                                                }

                                                PercentageIndex = rowData.Cells.Any(x => x.ToString().Trim() == "Percentage") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "Percentage").ColumnIndex : (int?)null;

                                                if (!PercentageIndex.HasValue)
                                                {
                                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("Percentage does not exist..try again");
                                                    return Created(strJSON, strJSON);
                                                }
                                            }

                                        }
                                        else
                                        {

                                            StorePriceDC uploadDC = new StorePriceDC();
                                            rowData = sheet.GetRow(iRowIdx);

                                            if (rowData != null)
                                            {
                                                var c = rowData.GetCell(0);
                                                if (c.NumericCellValue > 0)
                                                {
                                                    cellData = rowData.GetCell(ItemmultimrpIndex.Value);
                                                    col0 = (cellData == null || string.IsNullOrEmpty(cellData.ToString())) ? "0" : cellData.ToString();
                                                    uploadDC.Mrpid = Convert.ToInt16(col0);
                                                    logger.Info("Transaction Status :" + uploadDC.Mrpid);

                                                    string ptype = "";
                                                    cellData = rowData.GetCell(PriceTypeIndex.Value);
                                                    col1 = (cellData == null || string.IsNullOrEmpty(cellData.ToString())) ? "" : cellData.ToString();
                                                    ptype = Convert.ToString(col1);
                                                    uploadDC.Pricetype = ptype.Trim().ToLower();
                                                    logger.Info("Transaction Date :" + uploadDC.Pricetype);

                                                    cellData = rowData.GetCell(PercentageIndex.Value);
                                                    col2 = (cellData == null || string.IsNullOrEmpty(cellData.ToString())) ? "0" : cellData.ToString();
                                                    uploadDC.Percentage = Convert.ToDouble(col2);
                                                    logger.Info("ReferenceNumber :" + uploadDC.Percentage);

                                                    uploadDC.Type = 0;

                                                    uploaditemlist.Add(uploadDC);
                                                }

                                            }
                                        }
                                    }
                                }

                                if (uploaditemlist != null && uploaditemlist.Any())
                                {
                                    List<StorePriceConfiguration> excelstoredata = new List<StorePriceConfiguration>();
                                    List<Warehouse> warehouselist = context.Warehouses.Where(x => x.IsStore).ToList();
                                    List<string> pricetypelist = new List<string>();
                                    string mrp = "mrp minus";
                                    string unit = "unit price plus";
                                    pricetypelist.Add(mrp);
                                    pricetypelist.Add(unit);
                                    bool ispricetype = uploaditemlist.Any(x => !pricetypelist.Contains(x.Pricetype));
                                    if (!ispricetype)
                                    {
                                        if (uploaditemlist.Any(x => x.Percentage == 0))
                                        {
                                            MSG = "Percentage can not be zero."; return Created(MSG, MSG);
                                        }
                                        if (!uploaditemlist.Any(x => x.Mrpid == 0))
                                        {
                                            List<StorePriceConfiguration> storePriceConfigurations = new List<StorePriceConfiguration>();
                                            List<StorePriceConfiguration> storePriceConfig = new List<StorePriceConfiguration>();
                                            List<int> mrpids = uploaditemlist.Where(x => x.Mrpid > 0).Select(y => y.Mrpid).ToList();
                                            if (mrpids != null && mrpids.Any() && mrpids.Count > 0)
                                            {
                                                excelstoredata = context.StorePriceConfigurationDb.Where(x => x.WarehouseId == wid && x.IsActive == true && x.IsDeleted == false && mrpids.Contains(x.ItemMultiMRPId)).ToList();
                                            }
                                            foreach (var i in uploaditemlist)
                                            {
                                                if (excelstoredata != null && excelstoredata.Any())
                                                {
                                                    var data = excelstoredata.Where(x => x.ItemMultiMRPId == i.Mrpid && x.WarehouseId == wid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                                                    if (data != null)
                                                    {
                                                        data.Percentage = i.Percentage;
                                                        data.Type = i.Pricetype == "mrp minus" ? 1 : 2;
                                                        data.ModifiedBy = userid;
                                                        data.ModifiedDate = DateTime.Now;
                                                        context.Entry(data).State = EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        int types = i.Pricetype == "mrp minus" ? 1 : 2;
                                                        if (storePriceConfig != null && storePriceConfig.Any() && storePriceConfig.Any(x => x.WarehouseId == wid && x.ItemMultiMRPId == i.Mrpid && x.Type == types && x.IsActive == true && x.IsDeleted == false))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            StorePriceConfiguration store = new StorePriceConfiguration();
                                                            store.WarehouseId = wid;
                                                            store.ItemMultiMRPId = i.Mrpid;
                                                            store.Type = i.Pricetype == "mrp minus" ? 1 : 2;
                                                            store.Percentage = i.Percentage;
                                                            store.CreatedBy = userid;
                                                            store.CreatedDate = DateTime.Now;
                                                            store.IsActive = true;
                                                            store.IsDeleted = false;
                                                            store.ModifiedBy = null;
                                                            store.ModifiedDate = null;
                                                            storePriceConfig.Add(store);
                                                            storePriceConfigurations.Add(store);
                                                        }

                                                    }
                                                }
                                                else
                                                {
                                                    int types = i.Pricetype == "mrp minus" ? 1 : 2;
                                                    if (storePriceConfig != null && storePriceConfig.Any() && storePriceConfig.Any(x => x.WarehouseId == wid && x.ItemMultiMRPId == i.Mrpid && x.Type == types && x.IsActive == true && x.IsDeleted == false))
                                                    {

                                                    }
                                                    else
                                                    {
                                                        StorePriceConfiguration store = new StorePriceConfiguration();
                                                        store.WarehouseId = wid;
                                                        store.ItemMultiMRPId = i.Mrpid;
                                                        store.Type = i.Pricetype == "mrp minus" ? 1 : 2;
                                                        store.Percentage = i.Percentage;
                                                        store.CreatedBy = userid;
                                                        store.CreatedDate = DateTime.Now;
                                                        store.IsActive = true;
                                                        store.IsDeleted = false;
                                                        store.ModifiedBy = null;
                                                        store.ModifiedDate = null;
                                                        storePriceConfig.Add(store);
                                                        storePriceConfigurations.Add(store);
                                                    }

                                                }
                                                context.StorePriceConfigurationDb.AddRange(storePriceConfigurations);
                                            }
                                            if (context.Commit() > 0)
                                            {
                                                MSG = "Uploaded Successfully"; return Created(MSG, MSG);
                                            }
                                            else
                                            {
                                                MSG = "Some error Occured"; return Created(MSG, MSG);
                                            }
                                        }
                                        else
                                        {
                                            MSG = "ItemmultimrpId can not be zero."; return Created(MSG, MSG);
                                        }
                                    }
                                    else
                                    {
                                        MSG = "Price Type is Different. You can enter only (MRP Minus,Unit Price Plus)"; return Created(MSG, MSG);
                                    }
                                }
                                else
                                {
                                    MSG = "Data Not Found"; return Created(MSG, MSG);
                                }
                            }
                            else
                            {
                                return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    return Created(ex.Message, ex.Message);
                }


                return Created("Error", "Error");
            }
            else
            {
                return Created("Error", "Error");
            }
        }


        [Route("DownloadConfiguration")]
        [HttpGet]
        public string DownloadConfiguration()
        {
            string fileUrl = "";
            string fileName = "StorePriceConfigurationSampleDownloadFile" + ".xlsx";
            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
            if (File.Exists(path))
            {
                //string path = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                , string.Format("ExcelGeneratePath/{0}", fileName));
            }
            else
            {
                List<DownloadStorePriceDC> Result = new List<DownloadStorePriceDC>();
                DownloadStorePriceDC mrp = new DownloadStorePriceDC();
                mrp.ItemMultiMrpId = 1;
                mrp.PriceType = "mrp minus";
                mrp.Percentage = 1;
                Result.Add(mrp);
                DownloadStorePriceDC unit = new DownloadStorePriceDC();
                unit.ItemMultiMrpId = 2;
                unit.PriceType = "unit price plus";
                unit.Percentage = 2;
                Result.Add(unit);
                DataTable dt = ListtoDataTableConverter.ToDataTable(Result);
                string paths = Path.Combine(HttpContext.Current.Server.MapPath("~/ExcelGeneratePath"), fileName);
                ExcelGenerator.DataTable_To_Excel(dt, "StorePriceConfigurationSample", paths);
                fileUrl = string.Format("{0}://{1}{2}/{3}", new Uri(HttpContext.Current.Request.UrlReferrer.AbsoluteUri).Scheme
                                                                , HttpContext.Current.Request.Url.DnsSafeHost
                                                                , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                , string.Format("ExcelGeneratePath/{0}", fileName));
            }

            //if(Result != null)
            //{

            //    ExcelGenerator.DataTable_To_Excel(dt, "ForecastData", path);

            //}
            return fileUrl;
        }

        [Route("ApplynewOffer")]
        [HttpPost]
        public async Task<ReturnShoppingCart> ApplynewOffer(ApplyNewOfferDC offerDC)
        {
            ReturnShoppingCart returnShoppingCart = new ReturnShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                CustomerShoppingCart customerShoppingCart = new CustomerShoppingCart();
                if (offerDC.iBODetails != null && offerDC.iBODetails.Any())
                {
                    customerShoppingCart = new CustomerShoppingCart
                    {
                        IsActive = true,
                        CartTotalAmt = 0,
                        CreatedBy = offerDC.CustomerId,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        CustomerId = offerDC.CustomerId,
                        PeopleId = userid,
                        DeamPoint = 0,
                        DeliveryCharges = 0,
                        GrossTotalAmt = 0,
                        IsDeleted = false,
                        TotalDiscountAmt = 0,
                        TotalTaxAmount = 0,
                        WalletAmount = 0,
                        WarehouseId = offerDC.WarehouseId,
                        ShoppingCartItems = offerDC.iBODetails.Select(x => new ShoppingCartItem
                        {
                            CreatedBy = offerDC.CustomerId,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            IsActive = true,
                            IsDeleted = false,
                            IsFreeItem = false,
                            ModifiedBy = offerDC.CustomerId,
                            ItemId = x.ItemId,
                            qty = x.qty,
                            UnitPrice = x.UnitPrice,
                            IsPrimeItem = false,
                            IsDealItem = false,
                            TaxAmount = 0
                        }).ToList()
                    };
                    bool Isvalid = offerDC.ExistingOfferId.Any() ? await IsValidOffer(offerDC.ExistingOfferId, offerDC.OfferId) : true;

                    if (Isvalid)
                    {
                        if (!offerDC.ExistingOfferId.Any(x => x == offerDC.OfferId))
                        {
                            offerDC.ExistingOfferId.Add(offerDC.OfferId);
                        }
                        customerShoppingCartDc = await BackendOrderRefreshCart(customerShoppingCart, context, userid, offerDC.ExistingOfferId);
                        if (customerShoppingCartDc != null && !customerShoppingCartDc.ApplyOfferId.Contains(offerDC.OfferId.ToString()))
                        {
                            string offermsg = "OfferNotEligible";//!string.IsNullOrEmpty(lang) && lang == "hi" ? "आप इस ऑफर के लिए एलिजिबल नहीं हो!" : "You are not eligible for this offer!";
                            if (customerShoppingCartDc.DiscountDetails != null && customerShoppingCartDc.DiscountDetails.Any(x => !customerShoppingCartDc.ApplyOfferId.Contains(offerDC.OfferId.ToString())))
                                offermsg = "OfferNotEligibleUserWithOther";
                            returnShoppingCart.Status = false;
                            returnShoppingCart.Message = customerShoppingCartDc.NotEligiblePrimeOffer ? "MemberOfferNotEligible" : offermsg;//"You are not eligible for " + MemberShipName + " offer." : offermsg;
                            returnShoppingCart.Message = SCMsgEnum.SCEnum[returnShoppingCart.Message].Key;
                        }
                        else
                        {
                            returnShoppingCart.Status = true;
                            returnShoppingCart.Message = "Offer Applied Successfully";
                            returnShoppingCart.Cart = customerShoppingCartDc;
                        }
                    }
                    else
                    {
                        returnShoppingCart.Status = false;
                        returnShoppingCart.Message = "OfferNotEligibleUserWithOther";
                        returnShoppingCart.Message = SCMsgEnum.SCEnum[returnShoppingCart.Message].Key;
                        return returnShoppingCart;
                    }


                }
                else
                {
                    returnShoppingCart.Status = false;
                    returnShoppingCart.Message = "Please Add at Least One Item";
                    return returnShoppingCart;
                }
            }
            return returnShoppingCart;
        }

        public async Task<CustomerShoppingCartDc> BackendOrderRefreshCart(CustomerShoppingCart customerShoppingCart, AuthContext context, int peopleId, List<int> ExistingOfferId)
        {
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            List<ShoppingCartItemDc> ShoppingCartItemDcs = new List<ShoppingCartItemDc>();
            List<ShoppingCartDiscount> ShoppingCartDiscounts = new List<ShoppingCartDiscount>();
            List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = new List<DataContracts.Masters.BillDiscountOfferDc>();
            List<ItemMaster> itemmasterdata = new List<ItemMaster>();
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
                //var deliveryCharges = context.DeliveryChargeDb.Where(x => x.WarehouseId == customerShoppingCart.WarehouseId && x.isDeleted == false && x.IsActive && !x.IsDistributor).ToList();
                //string type = "Consumer";
                Customer ActiveCustomer = new Customer();
                bool inActiveCustomer = false;
                double walletpointconfig = 0;
                double retailerconfig = 0;
                double consumerconfig = 0;
                string Customertype = "";
                ActiveCustomer = context.Customers.FirstOrDefault(x => x.CustomerId == customerShoppingCart.CustomerId);
                Customertype = ActiveCustomer.CustomerType;
                var warehousedata = context.Warehouses.FirstOrDefault(x => x.WarehouseId == customerShoppingCart.WarehouseId);
                if (warehousedata.StoreType == 1)
                {
                    Customertype = "Consumer";
                    inActiveCustomer = ActiveCustomer != null && ActiveCustomer.IsB2CApp == false && ActiveCustomer.Deleted == true ? true : false;
                }
                //if (!string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType.ToLower() == "consumer")
                //{
                //    inActiveCustomer = ActiveCustomer != null && ActiveCustomer.IsB2CApp == false && ActiveCustomer.Deleted == true ? true : false;
                //}
                else
                {
                    inActiveCustomer = ActiveCustomer != null && ActiveCustomer.Active == false && ActiveCustomer.Deleted == true ? true : false;
                }

                int ConsumerWarehouseid = customerShoppingCartDc.WarehouseId;
                
                var cashconversion = context.CashConversionDb.ToList();
                retailerconfig = cashconversion != null && cashconversion.Any() && cashconversion.Any(x => x.IsConsumer == false) ? cashconversion.FirstOrDefault(x => x.IsConsumer == false).point : 0;
                consumerconfig = cashconversion != null && cashconversion.Any() && cashconversion.Any(x => x.IsConsumer == true) ? cashconversion.FirstOrDefault(x => x.IsConsumer == true).point : 0;
                if (warehousedata.StoreType == 1)
                {
                    walletpointconfig = consumerconfig > 0 ? consumerconfig : 10;
                }
                else
                {
                    walletpointconfig = !string.IsNullOrEmpty(ActiveCustomer.CustomerType) && ActiveCustomer.CustomerType == "Consumer" ? consumerconfig : retailerconfig;
                }
                

                List<int> itemids = customerShoppingCart.ShoppingCartItems.Where(x => !x.IsFreeItem && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).Select(x => x.ItemId).ToList();
                itemmasterdata = context.itemMasters.Where(x => itemids.Contains(x.ItemId) && x.WarehouseId == ConsumerWarehouseid).ToList();
                itemids = itemmasterdata.Select(x => x.ItemId).Distinct().ToList();

                foreach (var d in customerShoppingCart.ShoppingCartItems)
                {
                    d.ItemMultiMRPId = itemmasterdata.Any(x => x.ItemId == d.ItemId) ? itemmasterdata.FirstOrDefault(x => x.ItemId == d.ItemId).ItemMultiMRPId : 0;
                }


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

                string apptype = "BackendStore";

                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                List<OrderOfferDc> activeOfferids = new List<OrderOfferDc>();
                activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && (x.OfferAppType == apptype)).Select(x => new OrderOfferDc { OfferId = x.OfferId, QtyAvaiable = x.QtyAvaiable, IsDispatchedFreeStock = x.IsDispatchedFreeStock, MinOrderQuantity = x.MinOrderQuantity, NoOffreeQuantity = x.NoOffreeQuantity }).ToList() : null;

                DateTime CurrentDate = DateTime.Now;
                int hrs = !ActiveCustomer.IsPrimeCustomer ? MemberShipHours : 0;
                BackendOrderController backendOrderController = new BackendOrderController();
                if (Customertype == "Consumer")
                {
                    foreach (var it in newdata)
                    {
                        if (customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == it.ItemId))
                        {
                            it.UnitPrice = customerShoppingCart.ShoppingCartItems.FirstOrDefault(x => x.ItemId == it.ItemId).UnitPrice;
                        }
                        else
                        {
                            double cprice = backendOrderController.GetConsumerPrice(context, it.ItemMultiMRPId, it.price, it.UnitPrice, Convert.ToInt16(ConsumerWarehouseid));
                            it.UnitPrice = SkCustomerType.GetPriceFromType(Customertype, it.UnitPrice
                                                                           , it.WholeSalePrice ?? 0
                                                                           , it.TradePrice ?? 0, cprice);
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

                            if (it.OfferType != "FlashDeal")
                            {
                                if (activeOfferids != null && activeOfferids.Any() && activeOfferids.Any(x => x.OfferId == it.OfferId) && it.IsOffer)
                                    it.IsOffer = true;
                                else
                                    it.IsOffer = false;
                            }

                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
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
                    }
                }


                RetailerAppManager retailerAppManager = new RetailerAppManager();
                CustomersManager manager = new CustomersManager();
                List<int> offerIds = new List<int>();
                if (ExistingOfferId != null && ExistingOfferId.Any())
                {
                    offerIds = ExistingOfferId;
                }
                billDiscountOfferDcs = offerIds.Any() ? GetApplyBillDiscountById(offerIds, customerShoppingCart.CustomerId, context) : new List<DataContracts.Masters.BillDiscountOfferDc>();
                if (billDiscountOfferDcs.Any())
                {
                    foreach (var Offer in billDiscountOfferDcs.OrderByDescending(x => x.IsUseOtherOffer))
                    {

                        bool isEligable = true;
                        if (isEligable)
                        {
                            ShoppingCartDiscount ShoppingCartDiscount = new ShoppingCartDiscount();
                            double totalamount = 0;
                            var OrderLineItems = 0;
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
                                        WalletPoint = WalletPoint * Convert.ToInt32(walletpointconfig);
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
                                        ShoppingCartDiscount.DiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / walletpointconfig);
                                        ShoppingCartDiscount.NewBillingWalletPoint = 0;
                                    }
                                }
                                if (Offer.MaxDiscount > 0)
                                {
                                    var walletmultipler = 1;

                                    if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && (Offer.BillDiscountOfferOn != "Percentage" && Offer.BillDiscountOfferOn != "DynamicAmount"))
                                    {
                                        walletmultipler = Convert.ToInt32(walletpointconfig);
                                    }
                                    if (Offer.BillDiscountOfferOn != "DynamicAmount")
                                    {
                                        if (Offer.MaxDiscount < ShoppingCartDiscount.DiscountAmount)
                                        {
                                            ShoppingCartDiscount.DiscountAmount = Offer.MaxDiscount;
                                        }
                                        if (Offer.MaxDiscount < ShoppingCartDiscount.NewBillingWalletPoint / walletpointconfig)
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
                        x.UnitPrice = newdata.FirstOrDefault(y => y.ItemId == x.ItemId).UnitPrice;

                    }
                });
                ShoppingCartItemDcs = newdata.Select(a => new ShoppingCartItemDc
                {
                    BaseCategoryId = a.BaseCategoryId,
                    IsItemLimit = customerShoppingCart.ShoppingCartItems.Any(x => x.ItemId == a.ItemId && x.IsDealItem.HasValue && x.IsDealItem.Value) ? true : a.IsItemLimit,
                    ItemlimitQty = a.ItemlimitQty,
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
                    MinOrderQty = 1,
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

                    if (valid)
                    {
                        if (item.UnitPrice != item.CartUnitPrice)
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
                customerShoppingCart.SkCode = ActiveCustomer != null ? ActiveCustomer.Skcode : "";
                customerShoppingCart.Mobile = ActiveCustomer != null ? ActiveCustomer.Mobile : "";
                customerShoppingCart.ShopName = ActiveCustomer != null ? ActiveCustomer.ShopName : "";
                customerShoppingCart.City = ActiveCustomer != null ? ActiveCustomer.City : "";
                double TotalAmount = customerShoppingCart.GrossTotalAmt + customerShoppingCart.TotalDiscountAmt;

                var storeIds = ShoppingCartItemDcs.Where(x => x.qty > 0).Select(x => x.StoreId).Distinct().ToList();

            }
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


            if (customerShoppingCartDc.ShoppingCartItemDcs != null && customerShoppingCartDc.ShoppingCartItemDcs.Any(x => x.IsSuccess == false))
            {
                foreach (var cartItem in customerShoppingCartDc.ShoppingCartItemDcs.Where(x => x.IsSuccess == false).ToList())
                {
                    cartItem.MessageKey = cartItem.Message;
                    cartItem.Message = SCMsgEnum.SCEnum[cartItem.Message].Key;
                }

            }


            return customerShoppingCartDc;

        }

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

        [Route("GetAllOffer")]
        [HttpGet]
        public async Task<OfferdataDc> RetailerCommonDiscountOffer(int CustomerId, int WarehouseId, string lang = "en")
        {
            List<OfferDc> FinalBillDiscount = new List<OfferDc>();
            OfferdataDc res;
            using (AuthContext context = new AuthContext())
            {
                string customertype = "";
                CustomersManager manager = new CustomersManager();
                var customerdata = context.Customers.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
                customertype = customerdata.CustomerType;
                var warehousedata = context.Warehouses.FirstOrDefault(x => x.WarehouseId == WarehouseId);
                if (warehousedata.StoreType == 1)
                    customertype = "Consumer";
                if (customertype == "Consumer")
                {
                    List<AngularJSAuthentication.DataContracts.Masters.BillDiscountOfferDc> billDiscountOfferDcs = manager.GetBackEndOfferList(CustomerId, WarehouseId, "BackendStore");
                    if (billDiscountOfferDcs.Any())
                    {
                        var offerIds = billDiscountOfferDcs.Where(x => x.BillDiscountOfferOn == "FreeItem").Select(x => x.OfferId).ToList();
                        List<BillDiscountFreeItem> BillDiscountFreeItems = offerIds.Any() ? context.BillDiscountFreeItem.Where(x => offerIds.Contains(x.offerId) && x.RemainingOfferStockQty < x.OfferStockQty).ToList() : new List<BillDiscountFreeItem>();
                        var offertypeConfigs = context.OfferTypeDefaultConfigs.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                        foreach (var billDiscountOfferDc in billDiscountOfferDcs)
                        {
                            var OfferDefaultdata = billDiscountOfferDc.OfferOn == "Item" ? offertypeConfigs.Where(x => x.OfferType == billDiscountOfferDc.OfferOn && x.IsActive == true && x.IsDeleted == false).FirstOrDefault()
                                : offertypeConfigs.Where(x => x.OfferType == billDiscountOfferDc.OfferOn && x.DiscountOn == billDiscountOfferDc.BillDiscountOfferOn && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            OfferDefaultdata = OfferDefaultdata != null ? OfferDefaultdata : new Model.SalesApp.OfferTypeDefaultConfig();
                            var bdcheck = new OfferDc
                            {
                                OfferId = billDiscountOfferDc.OfferId,
                                OfferName = billDiscountOfferDc.OfferName,
                                OfferCode = billDiscountOfferDc.OfferCode,
                                OfferCategory = billDiscountOfferDc.OfferCategory,
                                OfferOn = billDiscountOfferDc.OfferOn,
                                start = billDiscountOfferDc.start,
                                end = billDiscountOfferDc.end,
                                DiscountPercentage = billDiscountOfferDc.DiscountPercentage,
                                BillAmount = billDiscountOfferDc.BillAmount,
                                LineItem = billDiscountOfferDc.LineItem,
                                Description = billDiscountOfferDc.Description,
                                BillDiscountOfferOn = billDiscountOfferDc.BillDiscountOfferOn,
                                BillDiscountWallet = billDiscountOfferDc.BillDiscountWallet,
                                IsMultiTimeUse = billDiscountOfferDc.IsMultiTimeUse,
                                IsUseOtherOffer = billDiscountOfferDc.IsUseOtherOffer,
                                IsScratchBDCode = billDiscountOfferDc.IsScratchBDCode,
                                BillDiscountType = billDiscountOfferDc.BillDiscountType,
                                OfferAppType = billDiscountOfferDc.OfferAppType,
                                ApplyOn = billDiscountOfferDc.ApplyOn,
                                WalletType = billDiscountOfferDc.WalletType,
                                MaxDiscount = billDiscountOfferDc.MaxDiscount,
                                ApplyType = billDiscountOfferDc.ApplyType,
                                ColorCode = !string.IsNullOrEmpty(billDiscountOfferDc.ColorCode) ? billDiscountOfferDc.ColorCode : OfferDefaultdata.ColorCode,
                                ImagePath = !string.IsNullOrEmpty(billDiscountOfferDc.ImagePath) ? billDiscountOfferDc.ImagePath : OfferDefaultdata.ImagePath,
                                IsBillDiscountFreebiesItem = billDiscountOfferDc.IsBillDiscountFreebiesItem,
                                IsBillDiscountFreebiesValue = billDiscountOfferDc.IsBillDiscountFreebiesValue,
                                offeritemname = billDiscountOfferDc.offeritemname,
                                offerminorderquantity = billDiscountOfferDc.offerminorderquantity,
                                OfferBillDiscountItems = billDiscountOfferDc.OfferBillDiscountItems.Select(y => new OfferBillDiscountItemDc
                                {
                                    CategoryId = y.CategoryId,
                                    Id = y.Id,
                                    IsInclude = y.IsInclude,
                                    SubCategoryId = y.SubCategoryId
                                }).ToList(),
                                OfferItems = billDiscountOfferDc.OfferItems.Select(y => new OfferItemdc
                                {
                                    IsInclude = y.IsInclude,
                                    itemId = y.itemId
                                }).ToList(),
                                RetailerBillDiscountFreeItemDcs = BillDiscountFreeItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).Select(x => new RetailerBillDiscountFreeItemDc
                                {
                                    ItemId = x.ItemId,
                                    ItemName = x.ItemName,
                                    Qty = x.Qty
                                }).ToList(),
                                BillDiscountRequiredItems = billDiscountOfferDc.BillDiscountRequiredItems.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList(),
                                OfferLineItemValueDcs = billDiscountOfferDc.OfferLineItemValueDcs.Where(x => x.offerId == billDiscountOfferDc.OfferId).ToList()
                            };

                            if (bdcheck.BillDiscountOfferOn == "FreeItem" && bdcheck.RetailerBillDiscountFreeItemDcs.Any())
                                FinalBillDiscount.Add(bdcheck);
                            else
                                FinalBillDiscount.Add(bdcheck);

                        }


                    }
                    res = new OfferdataDc()
                    {
                        offer = FinalBillDiscount.OrderBy(x => x.start).ToList(),
                        Status = true,
                        Message = "Success"
                    };
                }
                else
                {
                    res = new OfferdataDc()
                    {

                        Status = false,
                        Message = "No Offer Found"
                    };
                }
                return res;
            }

        }

        [Route("CreateNewCustomer")]
        [HttpPost]
        public CustomerDetailsDC CreateNewCustomer(CreateCustomer create)
        {
            CustomerDetailsDC customerDetailsDC = new CustomerDetailsDC();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var context = new AuthContext())
            {
                if (create != null)
                {
                    var people = context.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Deleted == false && x.Active);
                    Warehouse warehouse = context.Warehouses.FirstOrDefault(w => w.WarehouseId == create.WarehouseId);
                    Cluster cluster = context.Clusters.FirstOrDefault(x => x.WarehouseId == create.WarehouseId && x.CityId == warehouse.Cityid && x.Active);
                    var cust = context.Customers.FirstOrDefault(c => c.Mobile.Trim().Equals(create.MobileNo.Trim()));
                    if (cust == null)
                    {
                        GetCutomerDetailsDC detailsDC = new GetCutomerDetailsDC();
                        MobileRetailerAppController mobileRetailerAppController = new MobileRetailerAppController();
                        var Skcode = mobileRetailerAppController.skcode();
                        Customer c = new Customer();
                        c.Mobile = create.MobileNo;
                        c.ShippingAddress = create.ShippingAddress;
                        c.BillingAddress = create.ShippingAddress;
                        c.Name = create.CustomerName;
                        c.Skcode = Skcode;
                        c.Warehouseid = create.WarehouseId;
                        c.CompanyId = 1;
                        c.Cityid = warehouse.Cityid;
                        c.CreatedDate = DateTime.Now;
                        c.UpdatedDate = DateTime.Now;
                        c.Active = true;
                        c.Deleted = false;
                        c.CustomerType = warehouse.StoreType == 1 ? "Consumer" : "Retailer";
                        c.lat = warehouse.latitude;
                        c.lg = warehouse.longitude;
                        c.ClusterId = cluster.ClusterId;
                        c.ClusterName = cluster.ClusterName;
                        c.RefNo = create.RefNo;
                        context.Customers.Add(c);

                        
                        if (context.Commit() > 0)
                        {
                            #region add consumeraddress for zila
                            if (warehouse.StoreType == 1)
                            {
                                var addr = new ConsumerAddress
                                {
                                    Cityid = warehouse.Cityid,
                                    CityName = warehouse.CityName,
                                    WarehouseId = warehouse.WarehouseId,
                                    StateId = warehouse.Stateid,
                                    StateName = warehouse.StateName,
                                    Address1 = create.ShippingAddress,
                                    CompleteAddress = create.ShippingAddress,
                                    Default = false,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CreatedBy = people != null && people.PeopleID > 0 ? people.PeopleID : 0,
                                    CreatedDate = DateTime.Now,
                                    CustomerId = c.CustomerId,
                                    lat = warehouse.latitude,
                                    lng = warehouse.longitude,
                                    PersonName = create.CustomerName
                                };
                                context.ConsumerAddressDb.Add(addr);
                                context.Commit();
                            }
                            #endregion

                            customerDetailsDC.Status = true;
                            customerDetailsDC.Message = "Add Successfully";
                            //detailsDC = Mapper.Map(c).ToANew<GetCutomerDetailsDC>();
                            detailsDC.CustomerId = c.CustomerId;
                            detailsDC.WarehouseId = c.Warehouseid;
                            detailsDC.Mobile = c.Mobile;
                            detailsDC.ShippingAddress = c.ShippingAddress;
                            detailsDC.Name = c.Name;
                            detailsDC.RefNo = c.RefNo;
                            detailsDC.Skcode = c.Skcode;
                            detailsDC.WalletPoint = 0;
                            detailsDC.CustomerType = c.CustomerType;
                            customerDetailsDC.getCutomerDetailsDC = detailsDC;
                        }
                        else
                        {
                            customerDetailsDC.Status = false;
                            customerDetailsDC.Message = "Something Went Wrong";
                        }

                    }
                    else
                    {
                        customerDetailsDC.Status = false;
                        customerDetailsDC.Message = "customer Already Exists";
                    }

                }
                else
                {
                    customerDetailsDC.Status = false;
                    customerDetailsDC.Message = "Please Fill All Mandatory Field";
                }
            }
            return customerDetailsDC;
        }

        public async Task<BackendOrderResponseMsg> BackendPushBOOrder(ShoppingCartBODc sc, AuthContext context, Customer Customers, People people, Warehouse warehouse, TransactionScope scope, List<int> OfferIds)
        {
            BackendOrderResponseMsg result = new BackendOrderResponseMsg();
            result.Status = false;
            CustomerShoppingCart customerShoppingCart = new CustomerShoppingCart();
            CustomerShoppingCartDc customerShoppingCartDc = new CustomerShoppingCartDc();
            var placeOrderResponse = new BackendOrderPlaceOrderResponse();
            customerShoppingCart = new CustomerShoppingCart
            {
                IsActive = true,
                CartTotalAmt = 0,
                CreatedBy = Customers.CustomerId,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CustomerId = Customers.CustomerId,
                PeopleId = people.PeopleID,
                DeamPoint = 0,
                DeliveryCharges = 0,
                GrossTotalAmt = 0,
                IsDeleted = false,
                TotalDiscountAmt = 0,
                TotalTaxAmount = 0,
                WalletAmount = 0,
                WarehouseId = warehouse.WarehouseId,
                ShoppingCartItems = sc.itemDetails.Select(x => new ShoppingCartItem
                {
                    CreatedBy = Customers.CustomerId,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    IsFreeItem = false,
                    ModifiedBy = Customers.CustomerId,
                    ItemId = x.ItemId,
                    qty = x.qty,
                    UnitPrice = x.UnitPrice,
                    IsPrimeItem = false,
                    IsDealItem = false,
                    TaxAmount = 0
                }).ToList()
            };
            var walletconfiglist = context.CashConversionDb.ToList();
            double retailerconfig = walletconfiglist != null && walletconfiglist.Any() && walletconfiglist.Any(x => x.IsConsumer == false) ? walletconfiglist.FirstOrDefault(x => x.IsConsumer == false).point : 10;
            double consumerconfig = walletconfiglist != null && walletconfiglist.Any() && walletconfiglist.Any(x => x.IsConsumer == true) ? walletconfiglist.FirstOrDefault(x => x.IsConsumer == true).point : 10;
            double customerwalletconfig = 0;
            if(warehouse.StoreType == 1)
            {
                customerwalletconfig = consumerconfig > 0 ? consumerconfig : 10;
            }
            else
            {
                customerwalletconfig = !string.IsNullOrEmpty(Customers.CustomerType) ? Customers.CustomerType == "Consumer" ? consumerconfig : retailerconfig : 10;
            }
            
            double billdiscountsamount = sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0;
            double usedwalletpoint = sc.UsedWalletAmount > 0 ? sc.UsedWalletAmount : 0;
            #region less then 0 limit check 
            var scitemIds = sc.itemDetails.Select(x => x.ItemId).ToList();
            var scitemMastersList = context.itemMasters.Where(x => scitemIds.Contains(x.ItemId) && x.WarehouseId == warehouse.WarehouseId).ToList();
            var IteMultiMRPIds = scitemMastersList.Select(x => x.ItemMultiMRPId).Distinct().ToList();
            var ItemLimits = context.ItemLimitMasterDB.Where(x => IteMultiMRPIds.Contains(x.ItemMultiMRPId) && x.WarehouseId == warehouse.WarehouseId && x.IsItemLimit).ToList();
            foreach (var combo in sc.itemDetails)
            {
                var itemss = scitemMastersList.Where(x => x.ItemId == combo.ItemId).FirstOrDefault();
                if (itemss.active && itemss.UnitPrice > 0)
                {
                    if (ItemLimits != null && ItemLimits.Count() > 0)
                    {
                        ItemLimitMaster limit = ItemLimits.FirstOrDefault(x => x.ItemNumber == itemss.ItemNumber && x.ItemMultiMRPId == itemss.ItemMultiMRPId && x.WarehouseId == warehouse.WarehouseId && x.IsItemLimit);
                        if (limit != null && (limit.ItemlimitQty <= 0 || limit.ItemlimitQty < combo.qty))
                        {
                            result.Status = false;
                            result.Message = "The available item limit of itemname-(" + itemss.itemname + "), is less than the ordered quantity.";
                            return result;
                        }
                    }
                }

            }
            #endregion

            customerShoppingCartDc = await BackendOrderRefreshCart(customerShoppingCart, context, people.PeopleID, OfferIds);
            if (customerShoppingCartDc != null)
            {
                bool error = customerShoppingCartDc.ShoppingCartItemDcs.Any(x => x.IsSuccess == false) ? true : false;
                if (error)
                {
                    var data = customerShoppingCartDc.ShoppingCartItemDcs.Where(x => x.IsSuccess == false).FirstOrDefault();
                    result.Status = false;
                    result.Message = data.MessageKey == "BillLimitReach" ? data.itemname + " Maximum Qty " + (data.BillLimitQty) + " Per Bill" : data.itemname + " " + data.Message;
                    return result;
                }
            }
            List<int> Deactiavteofferids = new List<int>();
            OrderMaster objOrderMaster = new OrderMaster();
            List<BillDiscount> BillDiscounts = new List<BillDiscount>();
            List<BillDiscountFreebiesItemQtyDC> FreeQtyList = new List<BillDiscountFreebiesItemQtyDC>();
            var itemIds = sc.itemDetails.Select(x => x.ItemId).Distinct().ToList();
            List<AngularJSAuthentication.Model.ItemMaster> itemMastersList = context.itemMasters.Where(x => itemIds.Contains(x.ItemId)).Distinct().ToList();
            var freeItemIds = customerShoppingCartDc.ShoppingCartItemDcs.Where(x => x.FreeItemId > 0 && x.TotalFreeItemQty > 0).Select(x => x.FreeItemId).ToList();
            var FreeitemsList = context.itemMasters.Where(x => freeItemIds.Contains(x.ItemId)).Select(x => x).ToList();
            if (customerShoppingCartDc != null)
            {
                placeOrderResponse = BackendValidateShoppingCart(customerShoppingCartDc, context, sc, Customers, warehouse, FreeitemsList, out Deactiavteofferids, out objOrderMaster, out BillDiscounts, out FreeQtyList);
            }

            if (placeOrderResponse.IsSuccess)
            {
                if (people != null)
                {
                    objOrderMaster.OrderTakenSalesPersonId = people.PeopleID;
                    objOrderMaster.OrderTakenSalesPerson = people.DisplayName;
                }

                var billOffers = new List<Offer>();
                List<FlashDealItemConsumed> flashDealItemConsumedList = new List<FlashDealItemConsumed>();
                List<GenricEcommers.Models.OfferItem> offerItemsList = new List<GenricEcommers.Models.OfferItem>();
                var OfferUpdate = new List<Offer>();
                double offerWalletPoint = 0;
                bool iscustomeractive = (!string.IsNullOrEmpty(Customers.CustomerType) && Customers.CustomerType == "Consumer") ? Customers.IsB2CApp == false && Customers.Deleted == true ? false : true : Customers.Active;

                //if (Customers.Active)
                if (iscustomeractive)
                {
                    foreach (var i in placeOrderResponse.cart.ShoppingCartItemDcs)
                    {
                        //#region flashdeal
                        ItemLimitFreebiesDc ItemLimitFreebiesconsume = new ItemLimitFreebiesDc();

                        var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();

                        if (items != null && items.IsOffer && i.TotalFreeItemQty > 0)
                        {
                            if (i.TotalFreeItemQty > items.OfferQtyAvaiable)
                            {
                                result.Status = false;
                                result.Message = "Free Item Expired !! Available Quantity of Free Item " + items.OfferFreeItemName + " is (" + items.OfferQtyAvaiable + " )";
                                return result;
                            }
                            items.OfferQtyAvaiable = items.OfferQtyAvaiable - i.TotalFreeItemQty;
                            items.OfferQtyConsumed = items.OfferQtyConsumed + i.TotalFreeItemQty;
                            if (items.OfferQtyAvaiable.Value <= 0)
                            {
                                items.IsOffer = false;
                            }
                            context.Entry(items).State = EntityState.Modified;

                        }
                        #region Free Item and offer Walletpoint

                        if (i.IsOffer == true && i.FreeItemId > 0 && i.TotalFreeItemQty > 0)
                        {
                            #region Add if validated
                            var offer = context.OfferDb.Where(x => x.OfferId == items.OfferId).SingleOrDefault();

                            //to consume qty of freebiese if stock hit from currentstock in offer
                            if (offer != null && !offer.IsDispatchedFreeStock)
                            {
                                ItemLimitFreebiesconsume.ItemMultiMrpId = FreeitemsList.FirstOrDefault(f => f.ItemId == i.FreeItemId).ItemMultiMRPId;
                                ItemLimitFreebiesconsume.Qty = i.TotalFreeItemQty;
                            }


                            //freesqtylimit
                            if (offer != null && i.TotalFreeItemQty <= offer.FreeItemLimit)
                            {
                                if (offer.QtyAvaiable < i.TotalFreeItemQty)
                                {
                                    result.Status = false;
                                    result.Message = "Free Item Expired !! Available Quantity is (" + offer.QtyAvaiable + " )";
                                    return result;
                                }

                                offer.QtyAvaiable = offer.QtyAvaiable - i.TotalFreeItemQty;
                                offer.QtyConsumed = offer.QtyConsumed + i.TotalFreeItemQty;
                                if (offer.QtyAvaiable <= 0)
                                {
                                    offer.IsActive = false;
                                }
                                OfferUpdate.Add(offer);
                            }

                            GenricEcommers.Models.OfferItem ff = new GenricEcommers.Models.OfferItem();
                            ff.CompanyId = i.CompanyId;
                            ff.WarehouseId = i.WarehouseId;
                            ff.itemId = items.ItemId;
                            ff.itemname = items.itemname;
                            ff.MinOrderQuantity = offer.MinOrderQuantity;
                            ff.NoOffreeQuantity = i.TotalFreeItemQty;
                            ff.FreeItemId = offer.FreeItemId;
                            ff.FreeItemName = offer.FreeItemName;
                            ff.FreeItemMRP = offer.FreeItemMRP;
                            ff.IsDeleted = false;
                            ff.CreatedDate = indianTime;
                            ff.UpdateDate = indianTime;
                            ff.CustomerId = Customers.CustomerId;
                            //ff.OrderId = placeOrderResponse.OrderMaster.OrderId;
                            ff.OfferType = "ItemMaster";
                            ff.ReferOfferId = offer.OfferId;
                            //offerItemId.Add(ff.OfferId);
                            offerItemsList.Add(ff);

                            #endregion
                        }


                        if (i.IsOffer == true && i.TotalFreeWalletPoint > 0)
                        {
                            //If offer is on wallet point then update is wallet point.
                            offerWalletPoint = offerWalletPoint + Convert.ToDouble(i.TotalFreeWalletPoint);
                            var offerdata = context.OfferDb.Where(x => x.OfferId == items.OfferId).SingleOrDefault();
                            GenricEcommers.Models.OfferItem offerItem = new GenricEcommers.Models.OfferItem();

                            offerItem.CompanyId = i.CompanyId;
                            offerItem.WarehouseId = i.WarehouseId;
                            offerItem.itemId = items.ItemId;
                            offerItem.itemname = items.itemname;
                            offerItem.MinOrderQuantity = offerdata.MinOrderQuantity;
                            offerItem.NoOffreeQuantity = i.TotalFreeItemQty;
                            offerItem.FreeItemId = offerdata.FreeItemId;
                            offerItem.FreeItemName = offerdata.FreeItemName;
                            offerItem.FreeItemMRP = offerdata.FreeItemMRP;
                            offerItem.IsDeleted = false;
                            offerItem.CreatedDate = indianTime;
                            offerItem.UpdateDate = indianTime;
                            offerItem.CustomerId = Customers.CustomerId;
                            //offerItem.OrderId = objOrderMaster.OrderId;
                            offerItem.WallentPoint = Convert.ToInt32(i.TotalFreeWalletPoint);
                            offerItem.ReferOfferId = offerdata.OfferId;
                            offerItem.OfferType = "WalletPoint";
                            offerItemsList.Add(offerItem);
                            //offerItemId.Add(offerItem.OfferId);

                        }


                        #endregion
                        //For Item Deactive
                        #region Add if Validated
                        ItemLimitMaster ItemLimitMaster = context.ItemLimitMasterDB.Where(x => x.ItemNumber == items.Number && x.WarehouseId == items.WarehouseId && x.ItemMultiMRPId == items.ItemMultiMRPId).FirstOrDefault();
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

                            //logger.Error("OrderQtyLimitcheck  " + i.qty + " ItemlimitQty" + ItemLimitMaster.ItemlimitQty);
                            if (i.qty < ItemLimitMaster.ItemlimitQty || i.qty == 0)
                            {
                                //logger.Error("OrderQtyLimitcheck1");
                                ItemLimitMaster.ItemlimitQty = ItemLimitMaster.ItemlimitQty - i.qty;     // by kapil
                                ItemLimitMaster.ItemLimitSaleQty = ItemLimitMaster.ItemLimitSaleQty + i.qty;
                                context.Entry(ItemLimitMaster).State = EntityState.Modified;
                            }
                            else
                            {
                                //logger.Error("OrderQtyLimitcheck2222222222222222222esle  " + ItemLimitMaster.IsItemLimit);
                                ItemLimitMaster.ItemlimitQty = ItemLimitMaster.ItemlimitQty - i.qty;
                                ItemLimitMaster.ItemLimitSaleQty = ItemLimitMaster.ItemLimitSaleQty + i.qty;
                                ItemLimitMaster.IsItemLimit = false;//08/07/2019
                                context.Entry(ItemLimitMaster).State = EntityState.Modified;

                                if (ItemLimitMaster.ItemlimitQty <= 0 || items.MinOrderQty > ItemLimitMaster.ItemlimitQty)
                                {
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

                        ItemLimitMaster freeItemLimitMaster = context.ItemLimitMasterDB.Where(x => x.WarehouseId == items.WarehouseId && x.ItemMultiMRPId == ItemLimitFreebiesconsume.ItemMultiMrpId && x.ItemMultiMRPId != items.ItemMultiMRPId).FirstOrDefault();
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
                                    GenricEcommers.Models.OfferItem ff = new GenricEcommers.Models.OfferItem();
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
                                    ff.CustomerId = Customers.CustomerId;
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
                    foreach (var i in placeOrderResponse.cart.ShoppingCartItemDcs)
                    {
                        var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
                        if (i.IsOffer == true && i.FreeItemId > 0 && i.TotalFreeItemQty > 0)
                        {
                            #region Add if validated
                            var offer = context.OfferDb.Where(x => x.OfferId == items.OfferId).SingleOrDefault();
                            //freesqtylimit not 
                            //if (offer != null && i.FreeItemqty <= offer.FreeItemLimit)
                            //{
                            //    offer.QtyAvaiable = offer.QtyAvaiable - i.FreeItemqty;
                            //    offer.QtyConsumed = offer.QtyConsumed + i.FreeItemqty;
                            //    if (offer.QtyAvaiable <= 0)
                            //    {
                            //        offer.IsActive = false;
                            //    }
                            //    OfferUpdate.Add(offer);
                            //}

                            GenricEcommers.Models.OfferItem ff = new GenricEcommers.Models.OfferItem();
                            ff.CompanyId = i.CompanyId;
                            ff.WarehouseId = i.WarehouseId;
                            ff.itemId = items.ItemId;
                            ff.itemname = items.itemname;
                            ff.MinOrderQuantity = offer.MinOrderQuantity;
                            ff.NoOffreeQuantity = i.TotalFreeItemQty;
                            ff.FreeItemId = offer.FreeItemId;
                            ff.FreeItemName = offer.FreeItemName;
                            ff.FreeItemMRP = offer.FreeItemMRP;
                            ff.IsDeleted = false;
                            ff.CreatedDate = indianTime;
                            ff.UpdateDate = indianTime;
                            ff.CustomerId = Customers.CustomerId;
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

                //objOrderMaster.deliveryCharge = sc.deliveryCharge;
                objOrderMaster.deliveryCharge = 0;
                objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt) + objOrderMaster.deliveryCharge.Value, 2);
                objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
                objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
                objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
                objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero);
                objOrderMaster.DiscountAmount = 0;//System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmountAfterTaxDisc), 2);


                //objOrderMaster.OrderType = !string.IsNullOrEmpty(Customers.CustomerType) && Customers.CustomerType == "Consumer" ? 11 : 1;

                objOrderMaster.OrderType = warehouse.StoreType == 1 ? 11 : 1;

                //add cluster to ordermaster
                objOrderMaster.WalletAmount = usedwalletpoint;
                objOrderMaster.ClusterId = Customers.ClusterId ?? 0;
                objOrderMaster.ClusterName = Customers.ClusterName;
                objOrderMaster.IsPrimeCustomer = Customers.IsPrimeCustomer;

                double rewPoint = 0;
                double rewAmount = 0;





                // call function

                //removerd by Harry ( on 21)
                //objOrderMaster = RewardAndWalletPointForPlacedOrder(placeOrderResponse.cart, offerWalletPoint, objOrderMaster, rewardpoint, cust, walletUsedPoint1, rewPoint, rewAmount, cash);

                context.DbOrderMaster.Add(objOrderMaster);
                context.Commit();

                //objOrderMaster = context.RewardAndWalletPointForPlacedOrder(placeOrderResponse.cart, offerWalletPoint, objOrderMaster, rewardpoint, Customers, walletUsedPoint1, rewPoint, rewAmount, cash);
                if (offerWalletPoint > 0)
                {
                    objOrderMaster.RewardPoint = (rewardpoint > 0 ? rewardpoint : 0) + offerWalletPoint;
                }
                else
                {
                    objOrderMaster.RewardPoint = rewardpoint;
                }

                if (OfferUpdate != null && OfferUpdate.Any())
                {
                    foreach (var Offers in OfferUpdate)
                    {
                        context.Entry(Offers).State = EntityState.Modified;
                    }
                }
                #endregion
                objOrderMaster.Status = "Payment Pending";
                //objOrderMaster.Status = cust.Active && cluster != null && cluster.Active ? objOrderMaster.Status : (IsStopActive ? objOrderMaster.Status : "Inactive");
                double? RecalculateBillDiscountAmount = 0;
                #region Bill Discount
                if (BillDiscounts.Any())
                {
                    List<int> billdiscountofferids = BillDiscounts.Select(x => x.OfferId).ToList();
                    List<Offer> Offers = context.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.BillDiscountFreeItems).ToList();
                    List<BillDiscount> dbBillDiscounts = context.BillDiscountDb.Where(x => billdiscountofferids.Contains(x.OfferId) && x.CustomerId == Customers.CustomerId && x.OrderId == 0 && x.IsActive).ToList();



                    #region BillDiscount Free Item

                    if (Offers != null)
                    {
                        //List<int> offerids = new List<int>();
                        //if (Offers.Any(x => x.BillDiscountOfferOn == "FreeItem"))
                        //{
                        //    foreach (var item in Offers.Where(x => x.BillDiscountOfferOn == "FreeItem").SelectMany(x => x.BillDiscountFreeItems).ToList())
                        //    {
                        //        item.RemainingOfferStockQty += item.Qty;
                        //        this.Entry(item).State = EntityState.Modified;
                        //        if (item.RemainingOfferStockQty >= item.OfferStockQty)
                        //        {
                        //            Deactiavteofferids.Add(item.offerId);
                        //        }
                        //    }
                        //}

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

                        //if (Deactiavteofferids.Any())
                        //{
                        //    foreach (var item in Offers.Where(x => Deactiavteofferids.Contains(x.OfferId)))
                        //    {
                        //        item.UpdateDate = indianTime;
                        //        item.IsActive = false;
                        //        this.Entry(item).State = EntityState.Modified;
                        //    }
                        //}
                    }
                    #endregion

                    foreach (var offer in BillDiscounts)
                    {
                        var scritchcartoffer = !Offers.Any(x => x.OfferId == offer.OfferId && x.BillDiscountOfferOn == "DynamicAmount") ?
                            dbBillDiscounts.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == Customers.CustomerId && x.OrderId == 0)
                            : dbBillDiscounts.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == Customers.CustomerId && x.BillDiscountAmount == offer.BillDiscountAmount && x.OrderId == 0);

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
                                var scritchcartoffercrm = dbBillDiscounts.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == Customers.CustomerId && x.OrderId == 0 && x.IsDeleted == false && x.IsActive == true);
                                if (scritchcartoffercrm != null)
                                {
                                    scritchcartoffercrm.BillDiscountTypeValue = offer.BillDiscountTypeValue;
                                    scritchcartoffercrm.IsUsedNextOrder = offer.IsUsedNextOrder;
                                    scritchcartoffercrm.OrderId = objOrderMaster.OrderId;
                                    scritchcartoffercrm.BillDiscountAmount = offer.BillDiscountAmount;
                                    scritchcartoffercrm.ModifiedBy = Customers.CustomerId;
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
                                scritchcartoffer.ModifiedBy = Customers.CustomerId;
                                scritchcartoffer.ModifiedDate = indianTime;
                                context.Entry(scritchcartoffer).State = EntityState.Modified;

                                RecalculateBillDiscountAmount += offer.BillDiscountAmount;
                            }
                        }
                    }
                    //sc.BillDiscountAmount = BillDiscounts.Sum(x => x.BillDiscountAmount);

                }
                sc.BillDiscountAmount += RecalculateBillDiscountAmount;

                // 08Dec2021 Offer bies changes
                if (Deactiavteofferids.Any())
                {
                    foreach (var offerid in Deactiavteofferids)
                    {
                        var item = context.OfferDb.FirstOrDefault(x => x.OfferId == offerid);
                        if (item != null)
                        {
                            item.UpdateDate = indianTime;
                            item.IsActive = false;
                            context.Entry(item).State = EntityState.Modified;
                        }
                    }
                }
                #endregion
                //sc.BillDiscountAmount += billdiscountsamount;
                objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0) + objOrderMaster.TCSAmount - usedwalletpoint;
                objOrderMaster.BillDiscountAmount = sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0;
                objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0, MidpointRounding.AwayFromZero);

                #region Credit Note Apply
                if (sc.CreditNoteNumber != null && objOrderMaster.OrderId > 0)
                {
                    var CreditNoteData = context.ConsumerCreditnoteDb.FirstOrDefault(x => x.CreditNoteNumber == sc.CreditNoteNumber && x.IsUsed == false);
                    if (CreditNoteData != null)
                    {

                        CreditNoteData.UsedOrderId = objOrderMaster.OrderId;
                        CreditNoteData.CNOrderValueUsed = objOrderMaster.GrossAmount <= CreditNoteData.Ordervalue ? objOrderMaster.GrossAmount : 0;
                        CreditNoteData.IsUsed = true;
                        CreditNoteData.ModifiedDate = DateTime.Now;
                        context.Entry(CreditNoteData).State = EntityState.Modified;

                        if (CreditNoteData.CNOrderValueUsed > 0)
                        {
                            objOrderMaster.TotalAmount = 0;
                            objOrderMaster.GrossAmount = 0;
                        }
                        else
                        {
                            objOrderMaster.TotalAmount -= CreditNoteData.Ordervalue;
                            objOrderMaster.GrossAmount -= CreditNoteData.Ordervalue;
                        }
                        //objOrderMaster.BillDiscountAmount += CreditNoteData.Ordervalue;
                    }

                }
                #endregion

                if (objOrderMaster.OrderId != 0)
                {
                    #region Offer, FlashDeal
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
                                    OrderDetailsId = objOrderMaster.orderDetails.FirstOrDefault(x => x.ItemId == data.FreeItemId && x.UnitPrice == 0.0001 && (x.OfferId.HasValue && x.OfferId.Value == data.ReferOfferId)).OrderDetailsId;
                                }
                            }

                            data.OrderDetailsId = OrderDetailsId;


                        }

                        context.OfferItemDb.AddRange(offerItemsList);
                    }

                    #endregion

                    string Borderid = Convert.ToString(objOrderMaster.OrderId);
                    string BorderCodeId = Borderid.PadLeft(11, '0');
                    temOrderQBcode code = context.GetBarcode(BorderCodeId);
                    objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;
                    context.Commit();

                    OrderMasterHistories h1 = new OrderMasterHistories();
                    h1.orderid = objOrderMaster.OrderId;
                    h1.Status = "Payment Pending";
                    h1.Description = "BO";
                    h1.Reasoncancel = null;
                    h1.Warehousename = warehouse.WarehouseName;
                    h1.username = people.DisplayName;
                    h1.userid = people.PeopleID;
                    h1.CreatedDate = DateTime.Now;
                    context.OrderMasterHistoriesDB.Add(h1);
                    #region
                    var BillDiscount = context.BackendOrderBillDiscountConfigurations.FirstOrDefault(x => x.WarehouseId == warehouse.WarehouseId && x.CreatedDate.Month == DateTime.Now.Month);
                    if (BillDiscount != null && billdiscountsamount > 0)
                    {
                        BackendOrderBillDiscountDetails bo = new BackendOrderBillDiscountDetails();
                        bo.Orderid = objOrderMaster.OrderId;
                        bo.BackendOrderBillDiscountMasterId = BillDiscount.Id;
                        bo.BillDiscount = billdiscountsamount;
                        bo.CreatedDate = DateTime.Now;
                        context.BackendOrderBillDiscountDetails.Add(bo);
                    }
                    #endregion

                    #region Update IRN Check 
                    IRNHelper irnHelper = new IRNHelper();

                    if (irnHelper.IsGenerateIRN(context, Customers.CustomerId))
                    {
                        #region ClearTaxIntegrations
                        ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration();
                        clearTaxIntegration.OrderId = objOrderMaster.OrderId;
                        clearTaxIntegration.IsActive = true;
                        clearTaxIntegration.CreateDate = DateTime.Now;
                        clearTaxIntegration.IsProcessed = false;
                        clearTaxIntegration.APIType = "GenerateIRN";
                        context.ClearTaxIntegrations.Add(clearTaxIntegration);
                        #endregion
                    }
                    #endregion

                    #region update wallet point
                    if (objOrderMaster.WalletAmount > 0)
                    {
                        Wallet wallet = context.WalletDb.FirstOrDefault(c => c.CustomerId == objOrderMaster.CustomerId);
                        if (wallet != null && wallet.TotalAmount > 0 && wallet.TotalAmount >= objOrderMaster.WalletAmount && objOrderMaster.WalletAmount > 0)
                        {
                            CustomerWalletHistory CWH = new CustomerWalletHistory();
                            CWH.WarehouseId = objOrderMaster.WarehouseId > 0 ? objOrderMaster.WarehouseId : 0;
                            CWH.CompanyId = objOrderMaster.CompanyId > 0 ? objOrderMaster.CompanyId : 0;
                            CWH.CustomerId = wallet.CustomerId;
                            CWH.Through = "Used On Order";
                            CWH.NewOutWAmount = objOrderMaster.WalletAmount * customerwalletconfig;
                            CWH.TotalWalletAmount = wallet.TotalAmount - CWH.NewOutWAmount;
                            CWH.TotalEarningPoint = 0;
                            CWH.CreatedDate = indianTime;
                            CWH.UpdatedDate = indianTime;
                            CWH.OrderId = objOrderMaster.OrderId;
                            context.CustomerWalletHistoryDb.Add(CWH);
                            //update in wallet
                            wallet.TotalAmount -= CWH.NewOutWAmount;
                            wallet.TransactionDate = indianTime;
                            context.Entry(wallet).State = EntityState.Modified;

                            objOrderMaster.walletPointUsed = objOrderMaster.WalletAmount * 10;
                            //objOrderMaster.WalletAmount = sc.UsedWalletAmount;
                        }
                    }
                    #endregion


                    if (context.Commit() > 0)
                    {
                        #region ParentItemCutByCurrentStock
                        MultiStockHelper<Stock_OnPickedDc> MultiStockHelpers = new MultiStockHelper<Stock_OnPickedDc>();
                        List<Stock_OnPickedDc> rtdStockList = new List<Stock_OnPickedDc>();
                        List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
                        List<FreeBatchCodeList> FreeBatchCodeList = new List<FreeBatchCodeList>();
                        foreach (var StockHit in objOrderMaster.orderDetails.Where(x => x.qty > 0))
                        {
                            rtdStockList.Add(new Stock_OnPickedDc
                            {
                                ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                OrderId = StockHit.OrderId,
                                Qty = StockHit.qty,
                                UserId = people.PeopleID,
                                WarehouseId = StockHit.WarehouseId,
                                IsFreeStock = StockHit.IsFreeItem == true && StockHit.IsDispatchedFreeStock == true ? true : false,
                                RefStockCode = StockHit.IsDispatchedFreeStock == false ? "C" : "F"
                            });
                            if (StockHit.IsFreeItem == false)
                            {
                                foreach (var Batchlist in sc.itemDetails.Where(x => x.ItemId == StockHit.ItemId))
                                {
                                    foreach (var bitem in Batchlist.batchdetails.Where(x => x.qty > 0).ToList())
                                    {
                                        TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                                        {
                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                            Qty = bitem.qty * (-1),
                                            StockBatchMasterId = bitem.StockBatchMasterId,
                                            WarehouseId = warehouse.WarehouseId,
                                            ObjectId = StockHit.OrderId,
                                            ObjectIdDetailId = StockHit.OrderDetailsId
                                        });
                                    }
                                }
                            }

                        }

                        var param = new SqlParameter("@orderid", objOrderMaster.OrderId);
                        FreeBatchCodeList = context.Database.SqlQuery<FreeBatchCodeList>("exec Sp_getfreebatchCodelist @orderid", param).ToList();
                        if (FreeBatchCodeList != null && FreeBatchCodeList.Any())
                        {
                            foreach (var stock in objOrderMaster.orderDetails.Where(x => x.IsFreeItem == true && x.qty > 0))
                            {
                                var freedata = FreeBatchCodeList.Where(x => x.OrderDetailsId == stock.OrderDetailsId).ToList();
                                if (freedata != null && freedata.Any())
                                {
                                    if (freedata.Sum(x => x.Qty) >= stock.qty)
                                    {
                                        int qty = stock.qty;
                                        foreach (var free in freedata)
                                        {
                                            if (qty > 0)
                                            {
                                                if (qty >= free.Qty)
                                                {
                                                    TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                                                    {
                                                        ItemMultiMRPId = free.ItemMultiMRPId,
                                                        Qty = free.Qty * (-1),
                                                        StockBatchMasterId = free.StockBatchMasterId,
                                                        WarehouseId = free.WarehouseId,
                                                        ObjectId = free.OrderId,
                                                        ObjectIdDetailId = free.OrderDetailsId
                                                    });
                                                    qty -= free.Qty;
                                                }
                                                else
                                                {
                                                    TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                                                    {
                                                        ItemMultiMRPId = free.ItemMultiMRPId,
                                                        Qty = qty * (-1),
                                                        StockBatchMasterId = free.StockBatchMasterId,
                                                        WarehouseId = free.WarehouseId,
                                                        ObjectId = free.OrderId,
                                                        ObjectIdDetailId = free.OrderDetailsId
                                                    });
                                                    qty = 0;
                                                }
                                            }


                                        }
                                    }
                                    else
                                    {
                                        result.Message = "Can't Dispatched Due to batchstock qty not available!!";
                                        return result;
                                    }
                                }
                                else
                                {
                                    result.Message = "Can't Dispatched Due to batchstock qty not available!!";
                                    return result;
                                }
                            }
                        }

                        if (rtdStockList.Any())
                        {
                            BatchMasterManager batchMasterManager = new BatchMasterManager();

                            bool ress = MultiStockHelpers.MakeEntry(rtdStockList, "Stock_OnPicked", context, scope);

                            var StockTxnType = context.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "OrderPlannedOutCurrent" && x.IsDeleted == false);

                            bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMasterList, context, people.PeopleID, StockTxnType.Id);
                            if (!ress || !batchRes)
                            {
                                if (!batchRes)
                                {
                                    result.Message = "Can't Dispatched Due to batchstock qty not available!!";
                                    return result;
                                }
                                else
                                {
                                    result.Message = "Can't Dispatched, Due to qty";
                                    return result;
                                }
                            }
                            else
                            {
                                context.Commit();
                                result.Status = true;
                                result.Data = objOrderMaster;
                            }
                        }
                        #endregion
                    }

                }
            }
            else
            {
                result.Status = false;
                result.Message = placeOrderResponse.Message;
            }

            return result;
        }

        public BackendOrderPlaceOrderResponse BackendValidateShoppingCart(CustomerShoppingCartDc cart, AuthContext context, ShoppingCartBODc sc, Customer Customers, Warehouse warehouses, List<ItemMaster> FreeitemsList, out List<int> ExpireofferId, out OrderMaster objOrderMaster, out List<BillDiscount> BillDiscounts, out List<BillDiscountFreebiesItemQtyDC> FreeQtyList)
        {
            var placeOrderResponse = new BackendOrderPlaceOrderResponse { IsSuccess = true, Message = string.Empty, cart = cart };

            ExpireofferId = new List<int>();
            string MemberShipName = Common.Constants.AppConstants.MemberShipName;
            int MemberShipHours = Common.Constants.AppConstants.MemberShipHours;
            objOrderMaster = new OrderMaster();
            BillDiscounts = new List<BillDiscount>();
            FreeQtyList = new List<BillDiscountFreebiesItemQtyDC>();
            int salespersonid = 0;
            int wareid = sc.itemDetails.Select(x => x.WarehouseId).FirstOrDefault();
            var salespersondata = context.Peoples.FirstOrDefault(x => x.WarehouseId == wareid && x.Department == "Sales" && x.Active == true);
            if (salespersondata != null) { salespersonid = salespersondata.PeopleID; }
            var cust = Customers;

            #region Preparing Order
            objOrderMaster.CompanyId = warehouses.CompanyId;
            objOrderMaster.WarehouseId = warehouses.WarehouseId;
            objOrderMaster.WarehouseName = warehouses.WarehouseName;
            objOrderMaster.CustomerCategoryId = 2;
            objOrderMaster.Status = "Payment Pending";
            objOrderMaster.CustomerName = sc.CustomerName;
            objOrderMaster.ShopName = sc.CustomerName;
            objOrderMaster.BillingAddress = sc.ShippingAddress;
            objOrderMaster.ShippingAddress = sc.ShippingAddress;
            objOrderMaster.Customerphonenum = Customers.Mobile;
            objOrderMaster.LandMark = cust.LandMark;
            objOrderMaster.Skcode = cust.Skcode;
            objOrderMaster.CustomerType = cust.CustomerType;
            objOrderMaster.CustomerId = cust.CustomerId;
            objOrderMaster.CityId = cust.Cityid;
            objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
            objOrderMaster.ClusterName = cust.ClusterName;
            objOrderMaster.OrderTakenSalesPerson = "Self";
            objOrderMaster.comments = "BO";
            objOrderMaster.active = true;
            objOrderMaster.CreatedDate = indianTime;
            objOrderMaster.Tin_No = sc.RefNo;
            objOrderMaster.Deliverydate = indianTime;
            objOrderMaster.UpdatedDate = indianTime;
            objOrderMaster.Deleted = false;
            objOrderMaster.OrderTakenSalesPersonId = salespersonid;
            #endregion

            #region wallet congifuration
            double walletpointconfig = 0;
            double retailerconfig = 0;
            double consumerconfig = 0;

            var cashconversion = context.CashConversionDb.ToList();
            retailerconfig = cashconversion != null && cashconversion.Any() && cashconversion.Any(x => x.IsConsumer == false) ? cashconversion.FirstOrDefault(x => x.IsConsumer == false).point : 0;
            consumerconfig = cashconversion != null && cashconversion.Any() && cashconversion.Any(x => x.IsConsumer == true) ? cashconversion.FirstOrDefault(x => x.IsConsumer == true).point : 0;
            if(warehouses.StoreType == 1)
            {
                walletpointconfig =  consumerconfig ;
            }
            else
            {
                walletpointconfig = !string.IsNullOrEmpty(Customers.CustomerType) && Customers.CustomerType == "Consumer" ? consumerconfig : retailerconfig;
            }
            
            #endregion

            objOrderMaster.orderDetails = new List<OrderDetails>();
            double finaltotal = 0;
            double finalTaxAmount = 0;
            double finalSGSTTaxAmount = 0;
            double finalCGSTTaxAmount = 0;
            double finalGrossAmount = 0;
            double finalTotalTaxAmount = 0;
            double finalCessTaxAmount = 0;
            //var rewardpoint = 0;
            double unitPrice = 0;
            List<int> offerItemId = new List<int>();
            List<int> FlashDealOrderId = new List<int>();
            List<ItemMaster> itemMastersList = new List<ItemMaster>();
            List<int> cartItemids = cart.ShoppingCartItemDcs.Where(x => x.ItemId > 0).Select(x => x.ItemId).ToList();
            itemMastersList = context.itemMasters.Where(x => cartItemids.Contains(x.ItemId)).ToList();
            var itemnumbers = itemMastersList.Select(x => x.Number).Distinct().ToList();
            var itembarcodes = context.ItemBarcodes.Where(x => itemnumbers.Contains(x.ItemNumber) && x.IsActive == true && x.IsDeleted == false).ToList();
            var CustomerChannels = context.CustomerChannelMappings.Where(x => x.CustomerId == cust.CustomerId && x.IsActive == true).ToList();
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
            List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();

            #region fill orderdetails and freebies
            foreach (var i in placeOrderResponse.cart.ShoppingCartItemDcs.Where(x => x.OfferCategory != 2).Select(x => x))
            {
                unitPrice = 0;
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
                        //BackendOrderController backendOrderController = new BackendOrderController();

                        //double cprice = backendOrderController.GetConsumerPrice(context, items.ItemMultiMRPId, items.price, items.UnitPrice, warehouses.WarehouseId);
                        //items.UnitPrice = SkCustomerType.GetPriceFromType(cust.CustomerType, items.UnitPrice
                        //                                            , items.WholeSalePrice ?? 0
                        //                                            , items.TradePrice ?? 0, cprice);
                        if (sc.itemDetails.Any(x => x.ItemId == i.ItemId))
                        {
                            items.UnitPrice = sc.itemDetails.FirstOrDefault(x => x.ItemId == i.ItemId).UnitPrice;
                            unitPrice = items.UnitPrice;
                        }
                        else
                        {
                            i.IsSuccess = false;
                            i.Message = "Unit Price Mismatch";
                        }
                        bool isOffer = items.IsOffer;
                        if (!items.active || items.Deleted)
                        {
                            i.IsSuccess = false;
                            i.Message = "Item is not Active.";
                        }
                        //var limit = itemLimits.FirstOrDefault(x => x.ItemNumber == items.ItemNumber && x.ItemMultiMRPId == items.ItemMultiMRPId);
                        ////if (limit != null && mongoItemLimitValidationList.Any() && mongoItemLimitValidationList != null && (FreeitemsList == null || FreeitemsList.Count() == 0))
                        ////{
                        ////    int blockLimitQty = mongoItemLimitValidationList.Any(x => x.ItemId == i.ItemId && x.ItemMultiMRPId == items.ItemMultiMRPId && x.WarehouseId == i.WarehouseId) ? mongoItemLimitValidationList.FirstOrDefault(x => x.ItemId == i.ItemId && x.ItemMultiMRPId == items.ItemMultiMRPId && x.WarehouseId == i.WarehouseId).Qty : 0;
                        ////    if (blockLimitQty == 0 || (limit.ItemlimitQty - blockLimitQty) < i.qty)
                        ////    {
                        ////        i.IsSuccess = false;
                        ////        i.Message = "Order qty greater than limit qty.";
                        ////    }
                        ////}
                        //if (limit != null && limit.ItemlimitQty < i.qty)
                        //{
                        //    i.IsSuccess = false;
                        //    i.Message = "Item is not Active.";
                        //}
                        //var mod = Convert.ToDecimal(i.qty) % items.MinOrderQty;
                        //if (mod != 0)
                        //{
                        //    i.IsSuccess = false;
                        //    i.Message = "Item qty is not multiples of min order qty.";
                        //}

                        if (i.IsSuccess)
                        {
                            //var primeitem = PrimeItemDetails != null && PrimeItemDetails.Any(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) ? PrimeItemDetails.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) : null;
                            //var dealItem = DealItems != null && i.OfferCategory == 3 && DealItems.Any(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) ? DealItems.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) : null;
                            //if (cust.IsPrimeCustomer && primeitem != null)
                            //{
                            //    primeitem.PrimePrice = primeitem.PrimePercent > 0 ? Convert.ToDecimal(items.UnitPrice - (items.UnitPrice * Convert.ToDouble(primeitem.PrimePercent) / 100)) : primeitem.PrimePrice;

                            //    if (i.UnitPrice != Convert.ToDouble(primeitem.PrimePrice))
                            //    {
                            //        i.IsSuccess = false;
                            //        i.Message = "Item " + MemberShipName + " Unit Price has changed.";
                            //        i.NewUnitPrice = Convert.ToDouble(primeitem.PrimePrice);
                            //    }
                            //}
                            //else if (i.OfferCategory == 3 && dealItem == null)
                            //{
                            //    i.IsSuccess = false;
                            //    i.Message = "Deal Item has expired.";
                            //    i.NewUnitPrice = Convert.ToDouble(dealItem.DealPrice);
                            //}
                            //else if (i.OfferCategory == 3 && dealItem != null)
                            //{
                            //    if (i.UnitPrice != Convert.ToDouble(dealItem.DealPrice))
                            //    {
                            //        i.IsSuccess = false;
                            //        i.Message = "Item Deal Price has changed.";
                            //        i.NewUnitPrice = Convert.ToDouble(dealItem.DealPrice);
                            //    }
                            //}
                            //else 
                            //if (i.UnitPrice != items.UnitPrice)
                            //{
                            //    i.IsSuccess = false;
                            //    i.Message = "Item Unit Price has changed.";
                            //    i.NewUnitPrice = items.UnitPrice;
                            //}
                        }

                        if (i.IsSuccess)
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
                                        if (clusterStoreExecutiveDc != null)
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
                            od.Status = "Payment Pending";
                            od.CompanyId = warehouses.CompanyId;
                            od.WarehouseId = warehouses.WarehouseId;
                            od.WarehouseName = warehouses.WarehouseName;
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
                            od.Barcode = itembarcodes.Any(x => x.ItemNumber == items.Number) && itembarcodes != null ? itembarcodes.FirstOrDefault(x => x.ItemNumber == items.Number).Barcode : "";
                            //var primeitem = PrimeItemDetails != null && PrimeItemDetails.Any(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) ? PrimeItemDetails.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) : null;
                            //var dealItem = DealItems != null && i.OfferCategory == 3 && DealItems.Any(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) ? DealItems.FirstOrDefault(x => x.ItemMultiMRPId == items.ItemMultiMRPId && x.MinOrderQty == items.MinOrderQty) : null;
                            //if (cust.IsPrimeCustomer && primeitem != null)
                            //    od.UnitPrice = Convert.ToDouble(primeitem.PrimePercent > 0 ? Convert.ToDecimal(items.UnitPrice - (items.UnitPrice * Convert.ToDouble(primeitem.PrimePercent) / 100)) : primeitem.PrimePrice);
                            //else if (i.OfferCategory == 3 && dealItem != null)
                            //{
                            //    od.UnitPrice = Convert.ToDouble(dealItem.DealPrice);
                            //}
                            //else
                            od.UnitPrice = items.UnitPrice;

                            od.price = items.price;
                            od.ActualUnitPrice = items.UnitPrice;
                            od.MinOrderQty = 1;
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
                            //var schemeptr = itemPTR.Any(y => y.ItemMultiMRPId == items.ItemMultiMRPId) ? itemPTR.FirstOrDefault(y => y.ItemMultiMRPId == items.ItemMultiMRPId).PTR : 0;
                            //if (schemeptr > 0)
                            //{
                            //    od.PTR = Math.Round((schemeptr - 1) * 100, 2); //percent
                            //}
                            //////////////////////////////////////////////////////////////////////////////////////////////
                            //if (!items.IsOffer)
                            //{
                            //    /// Dream Point Logic && Margin Point relogic from 22April2019
                            //    int? MP, PP;
                            //    double xPoint = 0;

                            //    if (salespersonid == 0)
                            //    {
                            //        xPoint = xPointValue * 10; //Customer (0.2 * 10=1)
                            //    }
                            //    else
                            //    {
                            //        xPoint = xPointValue * 10; //Salesman (0.2 * 10=1)
                            //    }

                            //    if (items.promoPerItems.Equals(null) && items.promoPerItems == null)
                            //    {
                            //        PP = 0;
                            //    }
                            //    else
                            //    {
                            //        PP = items.promoPerItems;
                            //    }
                            //    if (items.marginPoint.Equals(null) && items.promoPerItems == null)
                            //    {
                            //        MP = 0;
                            //    }
                            //    else
                            //    {
                            //        double WithTaxNetPurchasePrice = Math.Round(items.NetPurchasePrice * (1 + (items.TotalTaxPercentage / 100)), 3);//With tax
                            //        MP = Convert.ToInt32((items.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
                            //    }
                            //    if (PP > 0 && MP > 0)
                            //    {
                            //        int? PP_MP = PP + MP;
                            //        items.dreamPoint = PP_MP;
                            //    }
                            //    else if (MP > 0)
                            //    {
                            //        items.dreamPoint = MP;
                            //    }
                            //    else if (PP > 0)
                            //    {
                            //        items.dreamPoint = PP;
                            //    }
                            //    else
                            //    {
                            //        items.dreamPoint = 0;
                            //    }
                            //}
                            //od.marginPoint = items.dreamPoint * od.qty;//dp point multiplyby order qty
                            //rewardpoint += od.marginPoint.GetValueOrDefault();


                            //List<ItemClassificationDC> objclassificationDc = new List<ItemClassificationDC>();
                            //objclassificationDc = GetItemClassifications(items.ItemNumber, warehouses.WarehouseId);
                            //od.ABCClassification = objclassificationDc.Any() ? objclassificationDc.Select(x => x.Category).FirstOrDefault() : "D";

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
                            if (i.IsOffer == true && i.FreeItemId > 0 && i.TotalFreeItemQty > 0)
                            {
                                //When there is a free item then we add this item in order detail
                                //Calculate its unit price as 0.
                                ItemMaster Freeitem = FreeitemsList.Where(x => x.ItemId == i.FreeItemId).FirstOrDefault();
                                var freeItemOffer = context.OfferDb.FirstOrDefault(x => items.OfferId == x.OfferId && x.WarehouseId == warehouses.WarehouseId);

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
                                        int? FreeOrderqty = i.TotalFreeItemQty;
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
                                                        if (clusterStoreExecutiveDc != null)
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
                                            od1.Status = "Payment Pending";
                                            od1.CompanyId = warehouses.CompanyId;
                                            od1.WarehouseId = warehouses.WarehouseId;
                                            od1.WarehouseName = warehouses.WarehouseName;
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
                                            od1.MinOrderQty = 1;
                                            od1.UnitPrice = 0.0001;
                                            od1.price = Freeitem.price;
                                            od1.MinOrderQtyPrice = (od1.MinOrderQty * od1.UnitPrice);
                                            od1.qty = Convert.ToInt32(i.TotalFreeItemQty);
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
                                            //var freeschemeptr = itemPTR.Any(y => y.ItemMultiMRPId == Freeitem.ItemMultiMRPId) ? itemPTR.FirstOrDefault(y => y.ItemMultiMRPId == Freeitem.ItemMultiMRPId).PTR : 0;
                                            //if (freeschemeptr > 0)
                                            //{
                                            //    od1.PTR = Math.Round((freeschemeptr - 1) * 100, 2); //percent
                                            //}
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

                                            //objclassificationDc = GetItemClassifications(Freeitem.ItemNumber, warehouse.WarehouseId);
                                            //od1.ABCClassification = objclassificationDc.Any() ? objclassificationDc.Select(x => x.Category).FirstOrDefault() : "D";
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
            #endregion

            #region deal items
            //foreach (var i in placeOrderResponse.cart.ShoppingCartItemDcs.Where(x => x.OfferCategory == 2).Select(x => x))
            //{
            //    unitPrice = 0;
            //    //try
            //    //{

            //    i.IsSuccess = true;
            //    if (cartItemIds.Contains(i.ItemId))
            //    {
            //        if (i.qty <= 0)
            //        {

            //            i.IsSuccess = false;
            //            i.Message = "Quantity is 0.";
            //        }
            //        else if (i.qty != 0 && i.qty > 0)
            //        {
            //            var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
            //            if (items == null)
            //            {
            //                i.IsSuccess = false;
            //                i.Message = "Item is not found.";
            //            }
            //            else
            //            {
            //                unitPrice = items.UnitPrice;
            //                bool isOffer = items.IsOffer;
            //                AuthContext context = new AuthContext();
            //                BackendOrderController backendOrderController = new BackendOrderController();
            //                double cprice = backendOrderController.GetConsumerPrice(context, items.ItemMultiMRPId, items.price, items.UnitPrice, warehouse.WarehouseId);
            //                items.UnitPrice = SkCustomerType.GetPriceFromType(cust.CustomerType, items.UnitPrice
            //                                                            , items.WholeSalePrice ?? 0
            //                                                            , items.TradePrice ?? 0, cprice);
            //                if (!items.active || items.Deleted)
            //                {
            //                    i.IsSuccess = false;
            //                    i.Message = "Item is not Active.";
            //                }
            //                if (supplierItemlist != null && supplierItemlist.Any(x => x == items.ItemMultiMRPId))
            //                {
            //                    i.IsSuccess = false;
            //                    i.Message = "Supplier not eligible to purchase this item!!";
            //                }
            //                var limit = itemLimits.FirstOrDefault(x => x.ItemNumber == items.ItemNumber && x.ItemMultiMRPId == items.ItemMultiMRPId);

            //                if (limit != null && limit.ItemlimitQty < i.qty)
            //                {
            //                    i.IsSuccess = false;
            //                    i.Message = "Item is not Active.";
            //                }
            //                var mod = Convert.ToDecimal(i.qty) % items.MinOrderQty;
            //                if (mod != 0)
            //                {
            //                    i.IsSuccess = false;
            //                    i.Message = "Item qty is not multiples of min order qty.";
            //                }
            //                if (i.IsSuccess && i.UnitPrice != items.UnitPrice)
            //                {
            //                    i.IsSuccess = false;
            //                    i.Message = "Item Unit Price has changed.";
            //                    i.NewUnitPrice = items.UnitPrice;
            //                }
            //                else
            //                {
            //                    OrderDetails od = new OrderDetails();
            //                    if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid))
            //                    {
            //                        var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid);
            //                        od.StoreId = store.StoreId;
            //                        if (CustomerChannels != null && CustomerChannels.Any(x => x.StoreId == od.StoreId))
            //                            od.ChannelMasterId = CustomerChannels.FirstOrDefault(x => x.StoreId == store.StoreId).ChannelMasterId;
            //                        if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId) && CustomerChannels != null)
            //                        {
            //                            if (CustomerChannels.Any(x => x.StoreId == od.StoreId))
            //                            {
            //                                var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId && a.ChannelMasterId == od.ChannelMasterId);
            //                                if (clusterStoreExecutiveDc != null)
            //                                {
            //                                    od.ExecutiveId = clusterStoreExecutiveDc.ExecutiveId;
            //                                    od.ExecutiveName = clusterStoreExecutiveDc.ExecutiveName;
            //                                }

            //                            }

            //                        }
            //                    }
            //                    else
            //                    {
            //                        od.StoreId = 0;
            //                        od.ExecutiveId = 0;
            //                        od.ExecutiveName = "";
            //                    }
            //                    od.CustomerId = cust.CustomerId;
            //                    od.CustomerName = cust.Name;
            //                    od.CityId = cust.Cityid;
            //                    od.Mobile = cust.Mobile;
            //                    od.OrderDate = indianTime;
            //                    od.Status = cust.Active ? "Pending" : "Inactive";
            //                    od.CompanyId = warehouse.CompanyId;
            //                    od.WarehouseId = warehouse.WarehouseId;
            //                    od.WarehouseName = warehouse.WarehouseName;
            //                    od.NetPurchasePrice = items.NetPurchasePrice + ((items.NetPurchasePrice * items.TotalTaxPercentage) / 100);
            //                    od.ItemId = items.ItemId;
            //                    od.ItemMultiMRPId = items.ItemMultiMRPId;
            //                    od.Itempic = items.LogoUrl;
            //                    od.itemname = items.itemname;
            //                    od.SupplierName = items.SupplierName;
            //                    od.SellingUnitName = items.SellingUnitName;
            //                    od.CategoryName = items.CategoryName;
            //                    od.SubsubcategoryName = items.SubsubcategoryName;
            //                    od.SubcategoryName = items.SubcategoryName;
            //                    od.SellingSku = items.SellingSku;
            //                    od.City = items.CityName;
            //                    od.itemcode = items.itemcode;
            //                    od.HSNCode = items.HSNCode;
            //                    od.itemNumber = items.Number;
            //                    od.Barcode = items.itemcode;

            //                    od.UnitPrice = items.FlashDealSpecialPrice ?? items.UnitPrice;
            //                    od.ActualUnitPrice = items.UnitPrice;
            //                    var schemeptr = itemPTR.Any(y => y.ItemMultiMRPId == items.ItemMultiMRPId) ? itemPTR.FirstOrDefault(y => y.ItemMultiMRPId == items.ItemMultiMRPId).PTR : 0;
            //                    if (schemeptr > 0)
            //                    {
            //                        od.PTR = Math.Round((schemeptr - 1) * 100, 2); //percent
            //                    }
            //                    //If OfferCategory is 2 then it is a flash deal then We Decrease the Quantity from item master and apphomeitem

            //                    od.price = items.price;
            //                    od.MinOrderQty = items.MinOrderQty;
            //                    od.MinOrderQtyPrice = (od.MinOrderQty * items.UnitPrice);
            //                    od.qty = Convert.ToInt32(i.qty);
            //                    od.SizePerUnit = items.SizePerUnit;
            //                    od.TaxPercentage = items.TotalTaxPercentage;
            //                    if (od.TaxPercentage >= 0)
            //                    {
            //                        od.SGSTTaxPercentage = od.TaxPercentage / 2;
            //                        od.CGSTTaxPercentage = od.TaxPercentage / 2;
            //                    }
            //                    od.Noqty = od.qty; // for total qty (no of items)    
            //                    od.TotalAmt = System.Math.Round(od.UnitPrice * od.qty, 2);

            //                    if (items.TotalCessPercentage > 0)
            //                    {
            //                        od.TotalCessPercentage = items.TotalCessPercentage;
            //                        double tempPercentagge = items.TotalCessPercentage + items.TotalTaxPercentage;

            //                        od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge / 100)) / 100;


            //                        od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
            //                        od.CessTaxAmount = (od.AmtWithoutAfterTaxDisc * od.TotalCessPercentage) / 100;
            //                    }


            //                    double tempPercentagge2 = items.TotalCessPercentage + items.TotalTaxPercentage;

            //                    od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge2 / 100)) / 100;
            //                    od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
            //                    od.TaxAmmount = (od.AmtWithoutAfterTaxDisc * od.TaxPercentage) / 100;
            //                    if (od.TaxAmmount >= 0)
            //                    {
            //                        od.SGSTTaxAmmount = od.TaxAmmount / 2;
            //                        od.CGSTTaxAmmount = od.TaxAmmount / 2;
            //                    }
            //                    //for cess
            //                    if (od.CessTaxAmount > 0)
            //                    {
            //                        double tempPercentagge3 = items.TotalCessPercentage + items.TotalTaxPercentage;
            //                        //double temp = od.TaxPercentage + od.TotalCessPercentage;
            //                        od.AmtWithoutTaxDisc = ((100 * od.UnitPrice * od.qty) / (1 + tempPercentagge3 / 100)) / 100;
            //                        od.AmtWithoutAfterTaxDisc = (100 * od.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);
            //                        od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.CessTaxAmount + od.TaxAmmount;
            //                    }
            //                    else
            //                    {
            //                        od.TotalAmountAfterTaxDisc = od.AmtWithoutAfterTaxDisc + od.TaxAmmount;
            //                    }
            //                    od.DiscountPercentage = 0;// items.PramotionalDiscount;
            //                    od.DiscountAmmount = 0;// (od.NetAmmount * items.PramotionalDiscount) / 100;
            //                    double DiscountAmmount = od.DiscountAmmount;
            //                    double NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
            //                    od.NetAmtAfterDis = (od.NetAmmount - DiscountAmmount);
            //                    double TaxAmmount = od.TaxAmmount;
            //                    od.Purchaseprice = items.PurchasePrice;
            //                    od.CreatedDate = indianTime;
            //                    od.UpdatedDate = indianTime;
            //                    od.Deleted = false;

            //                    //////////////////////////////////////////////////////////////////////////////////////////////
            //                    if (!items.IsOffer)
            //                    {
            //                        /// Dream Point Logic && Margin Point relogic from 22April2019
            //                        int? MP, PP;
            //                        double xPoint = 0;

            //                        if (cart.SalesPersonId == 0)
            //                        {
            //                            xPoint = xPointValue * 10; //Customer (0.2 * 10=1)
            //                        }
            //                        else
            //                        {
            //                            xPoint = xPointValue * 10; //Salesman (0.2 * 10=1)
            //                        }

            //                        if (items.promoPerItems.Equals(null) && items.promoPerItems == null)
            //                        {
            //                            PP = 0;
            //                        }
            //                        else
            //                        {
            //                            PP = items.promoPerItems;
            //                        }
            //                        if (items.marginPoint.Equals(null) && items.promoPerItems == null)
            //                        {
            //                            MP = 0;
            //                        }
            //                        else
            //                        {
            //                            double WithTaxNetPurchasePrice = Math.Round(items.NetPurchasePrice * (1 + (items.TotalTaxPercentage / 100)), 3);//With tax
            //                            MP = Convert.ToInt32((items.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
            //                        }
            //                        if (PP > 0 && MP > 0)
            //                        {
            //                            int? PP_MP = PP + MP;
            //                            items.dreamPoint = PP_MP;
            //                        }
            //                        else if (MP > 0)
            //                        {
            //                            items.dreamPoint = MP;
            //                        }
            //                        else if (PP > 0)
            //                        {
            //                            items.dreamPoint = PP;
            //                        }
            //                        else
            //                        {
            //                            items.dreamPoint = 0;
            //                        }
            //                    }
            //                    od.marginPoint = items.dreamPoint * od.qty;//dp point multiplyby order qty
            //                    rewardpoint += od.marginPoint.GetValueOrDefault();
            //                    List<ItemClassificationDC> objclassificationDc = new List<ItemClassificationDC>();
            //                    objclassificationDc = GetItemClassifications(items.ItemNumber, warehouse.WarehouseId);
            //                    od.ABCClassification = objclassificationDc.Any() ? objclassificationDc.Select(x => x.Category).FirstOrDefault() : "D";
            //                    objOrderMaster.orderDetails.Add(od);
            //                    if (od.CessTaxAmount > 0)
            //                    {
            //                        finalCessTaxAmount = finalCessTaxAmount + od.CessTaxAmount;
            //                        finalTaxAmount = finalTaxAmount + od.TaxAmmount + od.CessTaxAmount;
            //                    }
            //                    else
            //                    {
            //                        finalTaxAmount = finalTaxAmount + od.TaxAmmount;
            //                    }
            //                    finaltotal = finaltotal + od.TotalAmt;
            //                    finalSGSTTaxAmount = finalSGSTTaxAmount + od.SGSTTaxAmmount;
            //                    finalCGSTTaxAmount = finalCGSTTaxAmount + od.CGSTTaxAmmount;
            //                    finalGrossAmount = finalGrossAmount + od.TotalAmountAfterTaxDisc;
            //                    finalTotalTaxAmount = finalTotalTaxAmount + od.TotalAmountAfterTaxDisc;

            //                }

            //                items.IsOffer = isOffer;
            //                items.UnitPrice = unitPrice;
            //            }

            //        }
            //    }
            //    else
            //    {
            //        i.IsSuccess = false;
            //        i.Message = "Flash Deal Expired!";
            //    }
            //}
            #endregion

            #region billdiscountofferid
            if (!string.IsNullOrEmpty(cart.ApplyOfferId))
            {
                List<int> billdiscountofferids = cart.ApplyOfferId.Split(',').Select(x => Convert.ToInt32(x)).ToList();
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

                if (placeOrderResponse.cart.ShoppingCartItemDcs.All(x => x.IsSuccess) && placeOrderResponse.IsSuccess)
                {
                    //Due to assign cart offer and category on item
                    var PreItem = itemMastersList.Select(x => new { x.ItemId, x.IsOffer, x.OfferCategory }).ToList();
                    foreach (var item in itemMastersList)
                    {
                        var cartitem = placeOrderResponse.cart.ShoppingCartItemDcs.FirstOrDefault(p => p.ItemId == item.ItemId);
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
                        //if (Offer.OfferOn != "ScratchBillDiscount")
                        //{
                        var CItemIds = itemMastersList.Select(x => x.ItemId).ToList();
                        if (!string.IsNullOrEmpty(Offer.IncentiveClassification))
                        {
                            var classifications = Offer.IncentiveClassification.Split(',').ToList();
                            CItemIds = itemMastersList.Where(x => classifications.Contains(x.IncentiveClassification)).Select(x => x.ItemId).ToList();
                        }
                        List<int> Itemids = new List<int>();
                        if (Offer.BillDiscountType == "category" && Offer.BillDiscountOfferSections.Any())
                        {
                            //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                            //var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();


                            var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();
                            var ids = Offer.BillDiscountOfferSections.Where(x => x.IsInclude).Select(x => x.ObjId).ToList();
                            var notids = Offer.BillDiscountOfferSections.Where(x => !x.IsInclude).Select(x => x.ObjId).ToList();

                            //if (cart.APPType == "SalesApp")
                            //{
                            //    Itemids = itemMastersList.Where(x =>
                            //    (!ids.Any() || ids.Contains(x.Categoryid)) && !notids.Contains(x.Categoryid)
                            //    && !itemoutofferlist.Contains(x.ItemId)
                            //    && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                            //    ).Select(x => x.ItemId).ToList();
                            //}
                            //else
                            //{
                            Itemids = itemMastersList.Where(x => (!ids.Any() || ids.Contains(x.Categoryid)) && !notids.Contains(x.Categoryid)
                            && !itemoutofferlist.Contains(x.ItemId)
                            && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                            && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                            //}
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
                            //AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                            //List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);
                            List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = new List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc>();

                            var offerid = new SqlParameter("@offerId", Offer.OfferId);
                            offerCatSubCats = context.Database.SqlQuery<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc>("exec GetOfferSectionById @offerId", offerid).ToList();



                            //var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                            var itemoutofferlist = Offer.OfferItemsBillDiscounts.Where(x => !x.IsInclude).Select(x => x.itemId).ToList();
                            var iteminofferlist = Offer.OfferItemsBillDiscounts.Where(x => x.IsInclude).Select(x => x.itemId).ToList();

                            //if (cart.APPType == "SalesApp")
                            //{
                            //    Itemids = itemMastersList.Where(x =>
                            //     (!offerCatSubCats.Where(y => y.IsInclude).Any() || offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                            //     && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                            //    && !itemoutofferlist.Contains(x.ItemId)
                            //    && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                            //    ).Select(x => x.ItemId).ToList();
                            //}
                            //else
                            //{
                            Itemids = itemMastersList.Where(x =>
                            (!offerCatSubCats.Where(y => y.IsInclude).Any() || offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid))
                             && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid)
                            && !itemoutofferlist.Contains(x.ItemId)
                            && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                            && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                            //}
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

                            //AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                            //List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);
                            List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = new List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc>();

                            var offerid = new SqlParameter("@offerId", Offer.OfferId);
                            offerCatSubCats = context.Database.SqlQuery<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc>("exec GetOfferSectionById @offerId", offerid).ToList();

                            //if (cart.APPType == "SalesApp")
                            //{
                            //    Itemids = itemMastersList.Where(x =>
                            //    (
                            //     !offerCatSubCats.Where(y => y.IsInclude).Any() ||
                            //    offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                            //    )
                            //    && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                            //    && !itemoutofferlist.Contains(x.ItemId)
                            //    && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                            //    ).Select(x => x.ItemId).ToList();
                            //}
                            //else
                            //{
                            Itemids = itemMastersList.Where(x =>
                            (
                             !offerCatSubCats.Where(y => y.IsInclude).Any() ||
                            offerCatSubCats.Where(y => y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                            )
                            && !offerCatSubCats.Where(y => !y.IsInclude).Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid)
                            && !itemoutofferlist.Contains(x.ItemId)
                            && (!iteminofferlist.Any() || iteminofferlist.Contains(x.ItemId))
                            && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                            //}
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
                            //if (cart.APPType == "SalesApp")
                            //{
                            //    incluseItemIds = itemMastersList.Select(x => x.ItemId).ToList();
                            //}
                            //else
                            //{
                            incluseItemIds = itemMastersList.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                            //}
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
                            //if (cart.APPType == "SalesApp")
                            //{
                            //    incluseItemIds = itemMastersList.Select(x => x.ItemId).ToList();
                            //}
                            //else
                            //{
                            incluseItemIds = itemMastersList.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                            //}

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
                                //AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                                //List<BrandCategorySubCategory> BrandCategorySubCategorys = manager.GetCatSubCatByMappingId(ids);
                                List<BrandCategorySubCategory> BrandCategorySubCategorys = new List<BrandCategorySubCategory>();


                                var mappingid = new DataTable();
                                mappingid.Columns.Add("IntValue");
                                foreach (var item in ids)
                                {
                                    var dr = mappingid.NewRow();
                                    dr["IntValue"] = item;
                                    mappingid.Rows.Add(dr);
                                }
                                var gdnparam = new SqlParameter("MappingIds", mappingid);
                                gdnparam.SqlDbType = SqlDbType.Structured;
                                gdnparam.TypeName = "dbo.IntValues";
                                BrandCategorySubCategorys = context.Database.SqlQuery<BrandCategorySubCategory>("exec GetCategorySubCatByBrandMappingId MappingIds", mappingid).ToList();
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

                        #region comment
                        //if (Offer.OfferOn == "ScratchBillDiscount")
                        //{
                        //    var billdiscount = BillDiscountDb.FirstOrDefault(x => x.OfferId == Offer.OfferId && x.CustomerId == cust.CustomerId);
                        //    if (billdiscount != null && (Offer.BillDiscountOfferOn == "DynamicWalletPoint" || Offer.BillDiscountOfferOn == "DynamicAmount"))
                        //    {
                        //        Offer.BillDiscountWallet = billdiscount.BillDiscountTypeValue;
                        //    }
                        //}



                        //}
                        //else
                        //{
                        //    List<int> Itemids = new List<int>();
                        //    if (cart.APPType == "SalesApp")
                        //    {
                        //        Itemids = itemMastersList.Select(x => x.ItemId).ToList();
                        //    }
                        //    else
                        //    {
                        //        Itemids = itemMastersList.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                        //    }
                        //    totalamount = objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                        //    var billdiscount = BillDiscountDb.FirstOrDefault(x => x.OfferId == Offer.OfferId && x.CustomerId == cust.CustomerId);

                        //    if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                        //    {
                        //        totalamount = Offer.MaxBillAmount;
                        //    }
                        //    else if (Offer.BillAmount > totalamount)
                        //    {
                        //        totalamount = 0;
                        //    }

                        //    if (billdiscount != null && Offer.BillDiscountOfferOn == "DynamicWalletPoint")
                        //    {
                        //        Offer.BillDiscountWallet = billdiscount.BillDiscountTypeValue;
                        //    }
                        //}
                        #endregion

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
                                                        if (clusterStoreExecutiveDc != null)
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
                                            od1.CompanyId = warehouses.CompanyId;
                                            od1.WarehouseId = warehouses.WarehouseId;
                                            od1.WarehouseName = warehouses.WarehouseName;
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
                                            //od1.MinOrderQty = 0;
                                            //od1.MinOrderQtyPrice = 0;
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
                                            List<ItemClassificationDC> objclassificationDc = new List<ItemClassificationDC>();
                                            //objclassificationDc = GetItemClassifications(Freeitem.Number, warehouse.WarehouseId);
                                            //od1.ABCClassification = objclassificationDc.Any() ? objclassificationDc.Select(x => x.Category).FirstOrDefault() : "D";
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
                                            //objOrderMaster.orderDetails.Add(od1);
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
                            BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble((offerbilldiscounts.FirstOrDefault(x => x.OfferId == Offer.OfferId && x.OrderId == 0).BillDiscountTypeValue) / walletpointconfig);
                        }
                        else
                        {
                            int WalletPoint = 0;
                            if (Offer.WalletType == "WalletPercentage")
                            {
                                WalletPoint = Convert.ToInt32(Math.Truncate(totalamount * ((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0) / 100)));
                                WalletPoint = WalletPoint * Convert.ToInt32(walletpointconfig);
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
                                BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / walletpointconfig);
                                BillDiscount.IsUsedNextOrder = false;
                            }
                        }
                        if (Offer.MaxDiscount > 0)
                        {
                            var walletmultipler = 1;

                            if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && (Offer.BillDiscountOfferOn != "Percentage" && Offer.BillDiscountOfferOn != "DynamicAmount"))
                            {
                                walletmultipler = Convert.ToInt32(walletpointconfig);
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


                        //else
                        //{
                        //BillDiscount.BillDiscountTypeValue = Offer.BillDiscountWallet;
                        //    BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Offer.BillDiscountWallet / 10;
                        //}
                        //if (Offer.MaxDiscount > 0 && Offer.MaxDiscount < BillDiscount.BillDiscountAmount)
                        //{
                        //    BillDiscount.BillDiscountAmount = Offer.MaxDiscount;
                        //}
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
            #endregion

            if (placeOrderResponse.cart.ShoppingCartItemDcs.Any(x => !x.IsSuccess))
            {
                placeOrderResponse.IsSuccess = false;
                placeOrderResponse.Message = string.Join(", ", placeOrderResponse.cart.ShoppingCartItemDcs.Where(x => !x.IsSuccess).Select(x => x.Message).Distinct());
            }



            return placeOrderResponse;

        }

        private async Task<List<ItemDataDC>> ItemValidate(List<ItemDataDC> newdata, Customer ActiveCustomer, AuthContext db, bool IsSalesApp = false)
        {
            List<ItemDataDC> returnItems = new List<ItemDataDC>();
            if (ActiveCustomer != null && newdata.Any())
            {
                if (newdata != null && newdata.Any(s => !s.active))
                {
                    var InactiveItems = newdata.Where(s => !s.active);
                    InactiveItems = InactiveItems.GroupBy(x => x.ItemMultiMRPId).Select(x => x.FirstOrDefault()).ToList();
                    newdata = newdata.Where(s => s.active).ToList();
                    newdata.AddRange(InactiveItems);
                }
                //var IsPrimeCustomer = db.PrimeCustomers.Any(x => x.CustomerId == ActiveCustomer.CustomerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.StartDate <= indianTime && x.EndDate >= indianTime);

                DateTime CurrentDate = indianTime;
                var inActiveCustomer = ActiveCustomer != null && (ActiveCustomer.IsB2CApp == false || ActiveCustomer.Deleted == true) ? true : false;
                var warehouseId = ActiveCustomer != null && ActiveCustomer.Warehouseid.HasValue ? ActiveCustomer.Warehouseid.Value : 0;


                //string sqlquery = "SELECT a.[FlashDealId] AS [FlashDealId], a.[ItemId] AS [ItemId] FROM [dbo].[FlashDealItemConsumeds] AS a inner join AppHomeSectionItems c on a.FlashDealId = c.sectionItemId inner join dbo.AppHomeSections b on b.SectionID = c.apphomesections_SectionID  and b.Active=1 and b.[Deleted]=0  and b.WarehouseID=" + warehouseId +
                //                     " WHERE a.[CustomerId]=" + ActiveCustomer.CustomerId;
                //var FlashDealWithItemIds = db.Database.SqlQuery<FlashDealWithItem>(sqlquery).ToList();



                #region block Barnd
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                var custtype = ActiveCustomer.IsKPP ? 1 : 2;
                var blockBarnds = retailerAppManager.GetBlockBrand(custtype, 1, warehouseId);
                if (blockBarnds != null && blockBarnds.Any())
                {
                    newdata = newdata.Where(x => !(blockBarnds.Select(y => y.CatId).Contains(x.Categoryid) && blockBarnds.Select(y => y.SubCatId).Contains(x.SubCategoryId) && blockBarnds.Select(y => y.SubSubCatId).Contains(x.SubsubCategoryid))).ToList();
                }
                #endregion
                var AppType = "BackendStore";
                var offerids = newdata.Where(x => x.OfferId > 0).Select(x => x.OfferId).Distinct().ToList();
                var activeOfferids = new List<int>();
                activeOfferids = offerids != null && offerids.Any() ? db.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.OfferOn == "Item" && x.IsActive && !x.IsDeleted && (x.OfferAppType == AppType)).Select(x => x.OfferId).ToList() : new List<int>();


                List<ItemDataDC> freeItems = null;
                if (activeOfferids.Any())
                {
                    var freeItemIds = newdata.Where(x => x.OfferId.HasValue && x.OfferId > 0 && activeOfferids.Contains(x.OfferId.Value)).Select(x => x.OfferFreeItemId).ToList();
                    freeItems = db.itemMasters.Where(x => freeItemIds.Contains(x.ItemId)).Select(x => new ItemDataDC
                    {
                        ItemId = x.ItemId,
                        itemname = x.itemname,
                        HindiName = x.HindiName,
                        IsSensitive = x.IsSensitive,
                        IsSensitiveMRP = x.IsSensitiveMRP,
                        price = x.price,
                        UnitofQuantity = x.UnitofQuantity,
                        UOM = x.UOM,
                        LogoUrl = x.LogoUrl
                    }).ToList();
                }

                var itemMultiMRPIds = newdata.Select(x => x.ItemMultiMRPId).Distinct().ToList();

                foreach (var it in newdata.Where(x => (x.ItemAppType == 0 || x.ItemAppType == 1)))
                {
                    BackendOrderController backendOrderController = new BackendOrderController();
                    double cprice = backendOrderController.GetConsumerPrice(db, it.ItemMultiMRPId, it.price, it.UnitPrice, warehouseId);
                    it.UnitPrice = SkCustomerType.GetPriceFromType(ActiveCustomer.CustomerType, it.UnitPrice
                                                                   , it.WholeSalePrice ?? 0
                                                                   , it.TradePrice ?? 0, cprice);

                    //Condition for offer end.
                    if (!inActiveCustomer)
                    {
                        it.IsFlashDealStart = false;
                        if (it.OfferStartTime.HasValue)
                            it.NoPrimeOfferStartTime = it.OfferStartTime.Value.AddHours(MemberShipHours);
                        it.CurrentStartTime = indianTime;
                        //if (IsPrimeCustomer)
                        //{
                        //    it.IsFlashDealStart = it.OfferStartTime.Value <= indianTime;
                        //}
                        //else
                        //{
                        //    it.IsFlashDealStart = it.NoPrimeOfferStartTime <= indianTime;
                        //}

                        if (!(it.OfferStartTime <= CurrentDate && it.OfferEndTime >= indianTime))
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
                        else if ((it.OfferStartTime <= CurrentDate && it.OfferEndTime >= indianTime) && it.OfferCategory == 2)
                        {
                            it.IsFlashDealUsed = false;
                            //if (FlashDealWithItemIds != null && FlashDealWithItemIds.Any(x => x.ItemId == it.ItemId))
                            //{
                            //    it.IsFlashDealUsed = true;
                            //}
                        }

                        if (it.OfferCategory == 1)
                        {
                            if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
                            {
                                it.IsOffer = true;
                                if (freeItems != null && freeItems.Any(y => y.ItemId == it.OfferFreeItemId))
                                {
                                    it.OfferFreeItemName = freeItems.FirstOrDefault(y => y.ItemId == it.OfferFreeItemId).itemname;
                                    it.OfferFreeItemImage = freeItems.FirstOrDefault(y => y.ItemId == it.OfferFreeItemId).LogoUrl;
                                }
                            }
                            else
                                it.IsOffer = false;
                        }
                    }
                    else
                    {
                        if (it.OfferCategory == 1)
                        {
                            if (!(it.OfferStartTime <= CurrentDate && it.OfferEndTime >= indianTime) || !(activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId)))
                            {
                                it.IsOffer = false;
                                it.OfferCategory = 0;
                            }
                        }
                        else
                        {
                            it.IsOffer = false;
                            it.FlashDealSpecialPrice = 0;
                            it.OfferCategory = 0;
                        }
                    }

                    try
                    {

                        var unitprice = it.UnitPrice;
                        if (it.OfferCategory == 2 && it.IsOffer && it.FlashDealSpecialPrice.HasValue && it.FlashDealSpecialPrice > 0)
                        {
                            unitprice = it.FlashDealSpecialPrice.Value;
                        }
                        if (it.price > unitprice)
                        {
                            it.marginPoint = unitprice > 0 ? (((it.price - unitprice) * 100) / unitprice) : 0;//MP;  we replce marginpoint value by margin for app here 

                            //if (ItemSchemes != null && ItemSchemes.Any(x => x.ItemMultiMRPId == it.ItemMultiMRPId && x.PTR > 0))
                            //{
                            //    var scheme = ItemSchemes.FirstOrDefault(x => x.ItemMultiMRPId == it.ItemMultiMRPId);
                            //    var ptrPercent = Math.Round((scheme.PTR - 1) * 100, 2);
                            //    var UPMRPMargin = it.marginPoint.Value;
                            //    if (UPMRPMargin - (ptrPercent + scheme.BaseScheme) > 0)
                            //        it.Scheme = ptrPercent + "% PTR + " + Math.Round(UPMRPMargin - ptrPercent, 2) + "% Extra";
                            //}

                        }
                        else
                        {
                            it.marginPoint = 0;
                        }

                    }
                    catch { }
                    returnItems.Add(it);
                }
            }
            return returnItems;
        }

        private List<BillDiscountOfferDc> GetApplyBillDiscountById(List<int> OfferIds, int CustomerId, AuthContext context)
        {
            List<BillDiscountOfferDc> billDiscountOfferDcs = new List<BillDiscountOfferDc>();
            List<OfferBillDiscountItemDc> offerBillDiscountItems = new List<OfferBillDiscountItemDc>();
            List<OfferItemDc> offerItems = new List<OfferItemDc>();
            List<BillDiscountRequiredItemDc> BillDiscountRequiredItemDcs = new List<BillDiscountRequiredItemDc>();
            List<OfferLineItemValueDc> OfferLineItemValueDcs = new List<OfferLineItemValueDc>();

            if (context.Database.Connection.State != ConnectionState.Open)
                context.Database.Connection.Open();
            var IdDt = new DataTable();
            IdDt.Columns.Add("IntValue");
            foreach (var item in OfferIds)
            {
                var dr = IdDt.NewRow();
                dr["IntValue"] = item;
                IdDt.Rows.Add(dr);
            }
            var offerparam = new SqlParameter("offerIds", IdDt);
            offerparam.SqlDbType = SqlDbType.Structured;
            offerparam.TypeName = "dbo.IntValues";
            var custparam = new SqlParameter("@customerId", CustomerId);
            var cmds = context.Database.Connection.CreateCommand();
            cmds.CommandText = "[dbo].[GetApplyBillDiscountById]";
            cmds.CommandType = System.Data.CommandType.StoredProcedure;
            cmds.Parameters.Add(offerparam);
            cmds.Parameters.Add(custparam);
            var readers = cmds.ExecuteReader();
            billDiscountOfferDcs = ((IObjectContextAdapter)context).ObjectContext.Translate<BillDiscountOfferDc>(readers).ToList();
            readers.NextResult();
            offerBillDiscountItems = ((IObjectContextAdapter)context).ObjectContext.Translate<OfferBillDiscountItemDc>(readers).ToList();
            readers.NextResult();
            offerItems = ((IObjectContextAdapter)context).ObjectContext.Translate<OfferItemDc>(readers).ToList();
            readers.NextResult();
            BillDiscountRequiredItemDcs = ((IObjectContextAdapter)context).ObjectContext.Translate<BillDiscountRequiredItemDc>(readers).ToList();
            readers.NextResult();
            OfferLineItemValueDcs = ((IObjectContextAdapter)context).ObjectContext.Translate<OfferLineItemValueDc>(readers).ToList();
            billDiscountOfferDcs.ForEach(x =>
            {
                x.OfferItems = offerItems.Any() ? offerItems.Where(y => y.OfferId == x.OfferId).ToList() : new List<OfferItemDc>();
                x.OfferBillDiscountItems = offerBillDiscountItems.Any() ? offerBillDiscountItems.Where(y => y.OfferId == x.OfferId).ToList() : new List<OfferBillDiscountItemDc>();
                x.BillDiscountRequiredItems = BillDiscountRequiredItemDcs.Any() ? BillDiscountRequiredItemDcs.Where(y => y.offerId == x.OfferId).ToList() : new List<BillDiscountRequiredItemDc>();
                x.OfferLineItemValueDcs = OfferLineItemValueDcs.Any() ? OfferLineItemValueDcs.Where(y => y.offerId == x.OfferId).ToList() : new List<OfferLineItemValueDc>();

            });
            return billDiscountOfferDcs;
        }

        [Route("WalletPointCheck")]
        [HttpGet]
        public WalletPointDC GetWalletDiscount(int WarehouseId)
        {
            List<CashConversion> cashConversion = new List<CashConversion>();
            WalletPointDC walletPoint = new WalletPointDC();
            double ConfigWalletUsePercent = 0;
            MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.WalletHundredPercentUse> mongoDbHelper_W = new MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.WalletHundredPercentUse>(); //!x.GeneratedOrderId.HasValue
            var WalletHPer = mongoDbHelper_W.Select(x => x.WarehouseId == WarehouseId).FirstOrDefault();
            ConfigWalletUsePercent = WalletHPer != null && WalletHPer.WalletPer.HasValue ? WalletHPer.WalletPer.Value : Convert.ToDouble(ConfigurationManager.AppSettings["ConsumerWalletUseOfOrderValue"]);
            walletPoint.OfferWalletConfig = ConfigWalletUsePercent;
            double WalletValue = 0;
            WalletValue = Convert.ToDouble(ConfigurationManager.AppSettings["ConsumerMaxWalletPointUsed"]);
            walletPoint.OfferWalletValue = WalletValue;
            using (var db = new AuthContext())
            {
                cashConversion = db.CashConversionDb.ToList();
                walletPoint.RetailerWalletPoint = cashConversion != null && cashConversion.Any() && cashConversion.Any(x => x.IsConsumer == false) ? cashConversion.FirstOrDefault(x => x.IsConsumer == false).point : 0;
                walletPoint.ConsumerWalletPoint = cashConversion != null && cashConversion.Any() && cashConversion.Any(x => x.IsConsumer == true) ? cashConversion.FirstOrDefault(x => x.IsConsumer == true).point : 0;
            }
            return walletPoint;

        }

        [Route("CalculateReturnOrderBillDiscount")]
        [HttpPost]
        public ResponseCalulateOfferValueDC CalculateReturnOrderBillDiscount(ReturnCalulateOfferValueDC sc)
        {
            ResponseCalulateOfferValueDC res = new ResponseCalulateOfferValueDC();
            if (sc.OrderId > 0)
            {
                using (var db = new AuthContext())
                {
                    var CreditNotExpiry = db.ConsumerCompanyDetailDB.FirstOrDefault();
                    var orderDetails = db.DbOrderDetails.Where(x => x.OrderId == sc.OrderId).ToList();
                    var orderItemDiscountMapping = db.Database.SqlQuery<orderItemDiscountMappingDc>("select * from  OrderItemDiscountMapping where orderId=" + sc.OrderId + "").ToList();
                    if (CreditNotExpiry != null)
                    {
                        res.ValidTill = DateTime.Now.AddDays(CreditNotExpiry.CreditNoteUsedDays);
                    }
                    if (orderDetails.Any() && orderDetails != null)
                    {
                        foreach (var item in sc.returnItemDetails.Where(x => x.Returnqty > 0))
                        {
                            foreach (var odItem in orderDetails.Where(x => x.ItemId == item.ItemId && x.ItemMultiMRPId == item.ItemMultiMRPId && x.IsFreeItem == item.IsFreeItem))
                            {
                                var ritem = sc.returnItemDetails.Where(x => x.Returnqty > 0 && x.OrderDetailsId == odItem.OrderDetailsId).FirstOrDefault();
                                if (orderItemDiscountMapping.Any() && orderItemDiscountMapping != null && ritem != null)
                                {
                                    var orderItemDiscount = orderItemDiscountMapping.Where(y => y.OrderDetailId == odItem.OrderDetailsId).ToList();
                                    if (orderItemDiscount.Any() && orderItemDiscount != null)
                                    {
                                        res.CalculateBillDiscount += ((orderItemDiscount.Sum(x => x.BillDiscountAmount) / orderItemDiscount.FirstOrDefault().DispatchQty) * ritem.Returnqty);
                                    }
                                }
                                res.ReturnAmount += ritem != null ? ritem.Returnqty * odItem.UnitPrice : 0;
                            }
                        }
                    }
                };
            }
            return res;
        }
        [Route("PrintCreditNoteInvoice")] // Print order Invoice
        [HttpGet]
        public CreditNoteDetailsDC CheckCreditNote(int Orderid)
        {
            CreditNoteDetailsDC res = new CreditNoteDetailsDC();
            using (var db = new AuthContext())
            {
                if (Orderid > 0)
                {
                    var orderData = db.DbOrderMaster.Where(x => x.OrderId == Orderid).FirstOrDefault();
                    var Data = db.ConsumerCreditnoteDb.FirstOrDefault(x => x.CNOrderId == Orderid);
                    if (Data != null && orderData != null)
                    {
                        res.Status = true;
                        res.CreditNoteNumber = Data.CreditNoteNumber;
                        res.Amount = Data.Ordervalue;
                        res.CreditNoteValidTill = Data.ExpriryDate;
                        res.CreatedDate = Data.CreatedDate;
                        res.MobileNo = orderData.Customerphonenum;
                        res.SKCode = orderData.Skcode;
                    }
                    else
                    {
                        res = null;
                    }
                }
                else
                {
                    res = null;
                }
            }
            return res;
        }

        [Route("CheckCreditNote")] // create order time check
        [HttpGet]
        public CreditNoteDetailsDC CheckCreditNote(string CreditNoteNo)
        {
            CreditNoteDetailsDC res = new CreditNoteDetailsDC();
            using (var db = new AuthContext())
            {
                if (CreditNoteNo != null)
                {
                    var Data = db.ConsumerCreditnoteDb.FirstOrDefault(x => x.CreditNoteNumber.ToLower() == CreditNoteNo.ToLower());
                    if (Data != null)
                    {
                        if (DateTime.Now.Date > Data.ExpriryDate.Date)
                        {
                            res.Message = "Credit Note Expired.";
                            res.Status = false;
                        }
                        else if (Data.IsUsed == true)
                        {
                            res.Message = "Already Used.";
                            res.Status = false;
                        }
                        else
                        {
                            res.Message = "Verified Successfully.";
                            res.Status = true;
                            res.Amount = Data.Ordervalue;
                            res.CreditNoteValidTill = Data.ExpriryDate;
                        }
                    }
                    else
                    {
                        res.Message = "Credit Note Invalid.";
                        res.Status = false;
                    }
                }
                else
                {
                    res.Message = "Something went wrong.";
                    res.Status = false;
                }

            }
            return res;
        }

        [Route("ReturnItemBatchList")] // create order time check
        [HttpGet]
        public ResponseItemWiseBatchCode ReturnItemBatchList(string keyword, int? warehouseid, int orderId, bool IsFreeItem)
        {
            ResponseItemWiseBatchCode res = new ResponseItemWiseBatchCode()
            {
                ItemWiseBatchCode = new List<ItemWiseBatchCode>(),
                Status = false,
                Message = "Something Went Wrong"
            };

            using (var db = new AuthContext())
            {
                if (keyword != null && warehouseid != null)
                {
                    var paramKeyword = new SqlParameter("@keyword", keyword);
                    var paramorderId = new SqlParameter("@orderId", orderId);
                    var paramWarehouseId = new SqlParameter("@warehouseid", warehouseid);
                    var Isfree = new SqlParameter("@IsFreeItem", IsFreeItem);


                    var data = db.Database.SqlQuery<ItemWiseBatchCode>(
                        "exec itemwiseBatchcodeqty @keyword, @warehouseid,@orderId,@IsFreeItem", paramKeyword, paramWarehouseId, paramorderId, Isfree).ToList();

                    if (data != null && data.Any())
                    {
                        foreach (var d in data)
                        {
                            res.ItemWiseBatchCode.Add(d);
                        }
                        res.Status = true;
                        res.Message = "Data Found";
                    }
                    else
                    {
                        res.Message = "Data not Found";
                    }
                }
                else
                {
                    res.Message = "Required parameters are null.";
                }
            }
            return res;
        }


        [Route("CreateConsumerReturnOrder")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<APIResponse> CreateConsumerReturnOrder(ReturnConsumerDetailDC salesReturn)
        {
            KKReturnReplaceController kKReturnReplaceController = new KKReturnReplaceController();
            if (!kKReturnReplaceController.IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            OrderOutPublisher Publisher = new OrderOutPublisher();

            Customer custdata = new Customer();
            Warehouse wh = new Warehouse();
            OrderDispatchedMaster OrderDispatchedMaster = new OrderDispatchedMaster();
            List<OrderDispatchedDetails> orderDispatchedDetails = new List<OrderDispatchedDetails>();
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            OrderMaster orderMaster = new OrderMaster();
            List<POCStockEntryDc> POCStockList = new List<POCStockEntryDc>();
            string CNNumber = "";
            bool IsCommit = false;
            double totalAmt = 0;
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                if(salesReturn.returnItemDetails.Any(x => x.Returnqty > 0))
                {
                    using (AuthContext context = new AuthContext())
                    {
                        ReturnDetailDC data = new ReturnDetailDC();
                        custdata = context.Customers.Where(x => x.CustomerId == salesReturn.CustomerId).FirstOrDefault();
                        wh = context.Warehouses.Where(x => x.WarehouseId == custdata.Warehouseid).FirstOrDefault();

                        var OrderDetailIds = salesReturn.returnItemDetails.Where(x => x.Returnqty > 0).Select(x => x.OrderDetailsId).ToList();
                        var dispatchDetailList = context.OrderDispatchedDetailss.Where(x => OrderDetailIds.Contains(x.OrderDetailsId)).ToList();
                        var OrderdetailStoreids = context.DbOrderDetails.Where(x => OrderDetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.StoreId, x.IsFreeItem, x.IsDispatchedFreeStock }).ToList();
                        DataTable dt = new DataTable();

                        if (salesReturn != null && salesReturn.returnItemDetails.Count > 0)
                        {
                            //var OrderIdList = salesReturn.;
                            dt.Columns.Add("IntValue");
                            {
                                var dr = dt.NewRow();
                                dr["IntValue"] = salesReturn.OrderId;
                                dt.Rows.Add(dr);
                            }
                        }
                        List<OrderBillDiscountDc> list = new List<OrderBillDiscountDc>();

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 300;
                        cmd.CommandText = "[dbo].[Consumer_BillDiscountCalculationForSalesReturnOrder]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        var orderid = new SqlParameter("orderid", dt);
                        cmd.Parameters.Add(orderid);

                        var reader = cmd.ExecuteReader();

                        list = ((IObjectContextAdapter)context).ObjectContext.Translate<OrderBillDiscountDc>(reader).ToList();
                        context.Database.Connection.Close();


                        if (list != null && list.Count > 0)
                        {
                            foreach (var item in dispatchDetailList)
                            {
                                item.UnitBillDisAmt = list.FirstOrDefault(x => x.OrderDetailId == item.OrderDetailsId) != null ? list.FirstOrDefault(x => x.OrderDetailId == item.OrderDetailsId).UnitbilldiscountAmt : 0;
                                item.UnitWalletDisAmt = list.FirstOrDefault(x => x.OrderDetailId == item.OrderDetailsId) != null ? list.FirstOrDefault(x => x.OrderDetailId == item.OrderDetailsId).UnitWalletAmount : 0;
                                item.FinalItemAmount = (item.qty * item.UnitPrice) - item.UnitBillDisAmt;
                            }
                        };

                        data.OrderDetails = dispatchDetailList;
                        orderDispatchedDetails = dispatchDetailList;

                        //var CreditNoteData = context.ConsumerCreditnoteDb.FirstOrDefault(x => x.CNOrderId == salesReturn.OrderId && x.IsUsed == true);
                        //double CreditDiscount = 0;
                        //if(CreditNoteData != null)
                        //{
                        //    CreditDiscount = (CreditNoteData.CNOrderValueUsed > 0 ? CreditNoteData.CNOrderValueUsed : CreditNoteData.Ordervalue) / dispatchDetailList.Sum(x => x.qty);
                        //}

                        salesReturn.returnItemDetails.ForEach(x =>
                        {
                            dispatchDetailList.ForEach(d =>
                            {
                                if (x.OrderDetailsId == d.OrderDetailsId)
                                {
                                    totalAmt += (x.Returnqty * d.UnitPrice) - (x.Returnqty * d.UnitBillDisAmt);
                                }
                            });
                        });

                        #region wallet point return
                        var wallet = context.WalletDb.FirstOrDefault(x => x.CustomerId == custdata.CustomerId);
                        if (wallet != null)
                        {
                            int TotalReturnQty = salesReturn.returnItemDetails.Sum(x => x.Returnqty);
                            double TotalWallet = dispatchDetailList.Sum(x => x.UnitWalletDisAmt);
                            wallet.TotalAmount = wallet.TotalAmount + (TotalReturnQty * TotalWallet);
                            context.Entry(wallet).State = EntityState.Modified;

                            CustomerWalletHistory obj = new CustomerWalletHistory();
                            obj.CustomerId = custdata.CustomerId;
                            obj.WarehouseId = (int)custdata.Warehouseid;
                            obj.OrderId = dispatchDetailList.FirstOrDefault().OrderId;
                            obj.NewAddedWAmount = (TotalReturnQty * TotalWallet);
                            obj.TotalWalletAmount += obj.NewAddedWAmount;
                            obj.Through = "Backend Return order ";
                            obj.TransactionDate = DateTime.Now;
                            obj.CreatedDate = DateTime.Now;
                            context.CustomerWalletHistoryDb.Add(obj);
                        }

                        #endregion

                        #region OrderMaster

                        orderMaster.CompanyId = 1;
                        orderMaster.CustomerId = salesReturn.CustomerId;
                        orderMaster.CustomerName = custdata.Name;
                        orderMaster.Skcode = custdata.Skcode;
                        orderMaster.ShopName = custdata.ShopName;
                        orderMaster.CustomerType = custdata.CustomerType;
                        orderMaster.LandMark = custdata.LandMark;
                        orderMaster.Customerphonenum = custdata.Mobile;
                        orderMaster.BillingAddress = custdata.BillingAddress;
                        orderMaster.ShippingAddress = custdata.ShippingAddress;
                        orderMaster.TotalAmount = totalAmt;
                        orderMaster.WarehouseId = (int)custdata.Warehouseid;
                        orderMaster.WarehouseName = wh.WarehouseName;
                        orderMaster.CityId = custdata.Cityid;
                        orderMaster.active = true;
                        //orderMaster.Status = "Pending";
                        orderMaster.Status = "Post Order Canceled";
                        orderMaster.CreatedDate = DateTime.Now;
                        orderMaster.Deleted = false;
                        orderMaster.ClusterId = (int)custdata.ClusterId;
                        orderMaster.ClusterName = custdata.ClusterName;
                        orderMaster.OrderType = 3;
                        orderMaster.Lat = custdata.lat;
                        orderMaster.Lng = custdata.lg;
                        orderMaster.IsPrimeCustomer = custdata.IsPrimeCustomer;
                        orderMaster.userid = 1;
                        orderMaster.OrderDate = DateTime.Now;
                        orderMaster.CustomerCategoryId = custdata.CustomerCategoryId != null ? (int)custdata.CustomerCategoryId : 0;
                        orderMaster.GrossAmount = Math.Round(totalAmt, 0);// OrderDispatchedMaster.GrossAmount;
                        orderMaster.DiscountAmount = OrderDispatchedMaster.DiscountAmount;
                        orderMaster.TaxAmount = 0;
                        orderMaster.SGSTTaxAmmount = 0;
                        orderMaster.CGSTTaxAmmount = 0;
                        orderMaster.Deliverydate = DateTime.Now;
                        orderMaster.UpdatedDate = DateTime.Now;
                        orderMaster.DeliveredDate = DateTime.Now;
                        orderMaster.ReDispatchCount = 0;
                        orderMaster.DivisionId = 0;
                        orderMaster.ShortAmount = 0;
                        orderMaster.orderProcess = true;
                        orderMaster.accountProcess = true;
                        orderMaster.chequeProcess = false;
                        orderMaster.Savingamount = 0;
                        orderMaster.OnlineServiceTax = 0;
                        orderMaster.TCSAmount = 0;
                        orderMaster.IsFirstOrder = false;
                        orderMaster.ParentOrderId = salesReturn.OrderId;
                        orderMaster.DispatchAmount = Math.Round(totalAmt, 0);

                        foreach (var x in data.OrderDetails)
                        {
                            OrderDetails orderDetail = new OrderDetails();
                            orderDetail.OrderId = orderMaster.OrderId;
                            orderDetail.CustomerId = salesReturn.CustomerId;
                            orderDetail.CustomerName = custdata.Name;
                            orderDetail.City = custdata.City;
                            orderDetail.CityId = custdata.Cityid;
                            orderDetail.Mobile = custdata.Mobile;
                            orderDetail.CompanyId = 1;
                            orderDetail.WarehouseId = (int)custdata.Warehouseid;
                            orderDetail.WarehouseName = wh.WarehouseName;
                            orderDetail.CategoryName = x.CategoryName;
                            orderDetail.SubcategoryName = x.SubcategoryName;
                            orderDetail.SubsubcategoryName = x.SubsubcategoryName;
                            orderDetail.SellingSku = x.SellingSku;
                            orderDetail.ItemId = x.ItemId;
                            orderDetail.itemname = x.itemname;
                            orderDetail.itemcode = x.itemcode;
                            orderDetail.SellingUnitName = x.SellingUnitName;
                            orderDetail.itemNumber = x.itemNumber;
                            orderDetail.HSNCode = x.HSNCode;
                            orderDetail.Barcode = x.Barcode;
                            orderDetail.price = x.price;
                            orderDetail.UnitPrice = x.UnitPrice;
                            orderDetail.Purchaseprice = x.Purchaseprice;
                            orderDetail.Noqty = x.Noqty;
                            orderDetail.MinOrderQty = x.MinOrderQty;
                            orderDetail.MinOrderQtyPrice = x.MinOrderQtyPrice;
                            orderDetail.qty = salesReturn.returnItemDetails.Where(r => r.OrderDetailsId == x.OrderDetailsId && r.ItemMultiMRPId == x.ItemMultiMRPId).Sum(y => y.Returnqty);//&& r.ItemMultiMRPId == x.ItemMultiMRPId
                            orderDetail.TotalAmt = (x.UnitPrice * orderDetail.qty);
                            orderDetail.CreatedDate = DateTime.Now;
                            orderDetail.Deleted = false;
                            orderDetail.OrderDate = DateTime.Now;
                            orderDetail.Status = "Post Order Canceled";
                            orderDetail.ItemMultiMRPId = x.ItemMultiMRPId;
                            orderDetail.ABCClassification = ""; // x.OrderDetails.ABCClassification;
                            orderDetail.CurrentStock = 0;
                            orderDetail.SupplierName = ""; // x.OrderDetails.SupplierName;
                            orderDetail.ActualUnitPrice = 0; // x.OrderDetails.ActualUnitPrice;
                            orderDetail.StoreId = OrderdetailStoreids.Where(y => y.OrderDetailsId == x.OrderDetailsId).Select(y => y.StoreId).FirstOrDefault();
                            orderDetail.ExecutiveId = 0;// x.OrderDetails.ExecutiveId;
                            orderDetail.ExecutiveName = ""; // x.OrderDetails.ExecutiveName;
                            orderDetail.NetAmmount = (x.UnitPrice * orderDetail.qty);
                            orderDetail.UpdatedDate = DateTime.Now;
                            orderDetail.SizePerUnit = 1;
                            orderDetail.IsFreeItem = OrderdetailStoreids.Where(y => y.OrderDetailsId == x.OrderDetailsId).Select(y => y.IsFreeItem).FirstOrDefault(); ;
                            orderDetail.IsDispatchedFreeStock = OrderdetailStoreids.Where(y => y.OrderDetailsId == x.OrderDetailsId).Select(y => y.IsDispatchedFreeStock).FirstOrDefault(); ;
                            orderDetail.ActualUnitPrice = 0;
                            orderDetail.Deleted = false;
                            orderDetail.marginPoint = 0;
                            orderDetail.promoPoint = 0;
                            orderDetail.PTR = x.PTR;
                            orderDetail.OldOrderDetailId = x.OrderDetailsId;
                            orderDetails.Add(orderDetail);
                        }
                        orderMaster.orderDetails = orderDetails;
                        context.DbOrderMaster.Add(orderMaster);
                        //bool resStatus = salesReturnOrderHelper.PostOrderStatus(orderMaster.OrderId, "ReturnPending", userid, context);

                        if (context.Commit() > 0)
                            IsCommit = true;
                        else
                            IsCommit = false;
                    }
                    #endregion
                    if (IsCommit)
                    {
                        using (AuthContext db = new AuthContext())
                        {
                            #region Create Credit Note
                            if (salesReturn != null)
                            {
                                var CosumerComapny = db.ConsumerCompanyDetailDB.FirstOrDefault();
                                if (CosumerComapny != null)
                                {
                                    ConsumerCreditnote obj = new ConsumerCreditnote();
                                    CNNumber = "RO_" + DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + Guid.NewGuid().ToString("N").ToUpper().Substring(0, 10);
                                    obj.CreditNoteNumber = CNNumber;
                                    obj.CNOrderId = orderMaster.OrderId;
                                    obj.Ordervalue = orderMaster.GrossAmount;
                                    obj.UsedOrderId = 0;
                                    obj.ExpriryDate = DateTime.Now.AddDays(CosumerComapny.CreditNoteUsedDays);
                                    obj.CreatedDate = DateTime.Now;
                                    obj.CreatedBy = 0;
                                    obj.IsActive = true;
                                    obj.IsDeleted = false;
                                    obj.IsUsed = false;
                                    db.ConsumerCreditnoteDb.Add(obj);
                                }
                            }
                            #endregion
                            //bar code start
                            var odm = db.DbOrderMaster.Where(x => x.OrderId == orderMaster.OrderId && x.active == true && x.Deleted == false).Include(x => x.orderDetails).FirstOrDefault();
                            if (odm != null)
                            {
                                string Borderid = Convert.ToString(odm.OrderId);
                                string BorderCodeId = Borderid.PadLeft(11, '0');
                                temOrderQBcode code = db.GetBarcode(BorderCodeId);
                                odm.InvoiceBarcodeImage = code.BarcodeImage;
                                db.Entry(odm).State = EntityState.Modified;
                            }
                            OrderDispatchedMaster.InvoiceBarcodeImage = odm.InvoiceBarcodeImage;
                            // bar code end
                            OrderDispatchedMaster.OrderId = odm.OrderId;
                            OrderDispatchedMaster.CompanyId = odm.CompanyId;
                            OrderDispatchedMaster.CustomerId = odm.CustomerId;
                            OrderDispatchedMaster.CustomerName = custdata.Name;
                            OrderDispatchedMaster.ShopName = custdata.ShopName;
                            OrderDispatchedMaster.Skcode = custdata.Skcode;
                            OrderDispatchedMaster.Status = "Post Order Canceled";
                            OrderDispatchedMaster.ReDispatchedStatus = null;
                            OrderDispatchedMaster.CanceledStatus = null;
                            OrderDispatchedMaster.OrderDate = indianTime;
                            OrderDispatchedMaster.CustomerType = custdata.CustomerType;
                            OrderDispatchedMaster.deliveryCharge = 0;
                            OrderDispatchedMaster.Deliverydate = odm.Deliverydate;
                            OrderDispatchedMaster.CreatedDate = indianTime;
                            OrderDispatchedMaster.OrderedDate = indianTime;
                            OrderDispatchedMaster.UpdatedDate = indianTime;
                            OrderDispatchedMaster.WarehouseId = odm.WarehouseId;
                            OrderDispatchedMaster.WarehouseName = wh.WarehouseName;
                            #region invoice number generate
                            //invoice number start
                            string invoiceNumber = " ";
                            if (OrderDispatchedMaster.WarehouseId != 67 && OrderDispatchedMaster.WarehouseId != 80)
                            {
                                invoiceNumber = db.Database.SqlQuery<string>("EXEC spGetCurrentNumber 'Invoice', " + wh.Stateid).FirstOrDefault();
                            }
                            //invoice number end
                            #endregion
                            OrderDispatchedMaster.Trupay = odm.Trupay;
                            OrderDispatchedMaster.CustomerCategoryId = odm.CustomerCategoryId;
                            OrderDispatchedMaster.CustomerCategoryName = odm.CustomerCategoryName;
                            OrderDispatchedMaster.Customerphonenum = odm.Customerphonenum;
                            OrderDispatchedMaster.BillingAddress = odm.BillingAddress;
                            OrderDispatchedMaster.ShippingAddress = odm.ShippingAddress;
                            OrderDispatchedMaster.comments = odm.comments;
                            OrderDispatchedMaster.CityId = odm.CityId;
                            OrderDispatchedMaster.active = odm.active;
                            OrderDispatchedMaster.Deleted = odm.Deleted;
                            OrderDispatchedMaster.DivisionId = odm.DivisionId;
                            OrderDispatchedMaster.ClusterId = odm.ClusterId;
                            OrderDispatchedMaster.ClusterName = odm.ClusterName;
                            OrderDispatchedMaster.WalletAmount = odm.WalletAmount;
                            OrderDispatchedMaster.RewardPoint = odm.RewardPoint;
                            OrderDispatchedMaster.ShortAmount = odm.ShortAmount;
                            OrderDispatchedMaster.OnlineServiceTax = odm.OnlineServiceTax;
                            OrderDispatchedMaster.OrderTakenSalesPersonId = odm.OrderTakenSalesPersonId;
                            OrderDispatchedMaster.OrderTakenSalesPerson = odm.OrderTakenSalesPerson;
                            OrderDispatchedMaster.Tin_No = odm.Tin_No;
                            OrderDispatchedMaster.Savingamount = odm.Savingamount;
                            OrderDispatchedMaster.orderProcess = odm.orderProcess;
                            OrderDispatchedMaster.paymentThrough = odm.paymentThrough;
                            OrderDispatchedMaster.paymentMode = odm.paymentMode;
                            OrderDispatchedMaster.BillDiscountAmount = odm.BillDiscountAmount;
                            OrderDispatchedMaster.offertype = odm.offertype;
                            OrderDispatchedMaster.TCSAmount = odm.TCSAmount;

                            OrderDispatchedMaster.DBoyId = 0;
                            OrderDispatchedMaster.DboyName = null;
                            OrderDispatchedMaster.DboyMobileNo = null;
                            OrderDispatchedMaster.DboycheckRecived = false;
                            OrderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster = 0;
                            OrderDispatchedMaster.ReDispatchCount = 0;
                            OrderDispatchedMaster.ReAttemptCount = 0;
                            OrderDispatchedMaster.GrossAmount = Math.Round(totalAmt, 0);
                            OrderDispatchedMaster.IsReAttempt = false;
                            OrderDispatchedMaster.Deliverydate = DateTime.Now;
                            OrderDispatchedMaster.EwayBillFileUrl = null;
                            OrderDispatchedMaster.EwayBillNumber = null;
                            OrderDispatchedMaster.PocCreditNoteNumber = CNNumber;
                            OrderDispatchedMaster.PocCreditNoteDate = DateTime.Now;
                            OrderDispatchedMaster.IsPocVerified = false;
                            OrderDispatchedMaster.IRNNo = null;
                            OrderDispatchedMaster.IRNQRCode = null;
                            OrderDispatchedMaster.POCIRNNo = null;
                            OrderDispatchedMaster.POCIRNQRCode = null;
                            OrderDispatchedMaster.POCIRNQRCodeURL = null;
                            OrderDispatchedMaster.IsGenerateIRN = false;
                            OrderDispatchedMaster.IsEwayBillRequired = false;
                            OrderDispatchedMaster.IRNQRCodeUrl = null;
                            OrderDispatchedMaster.IsGenerateCN = false;
                            //OrderDispatchedMaster.InvoiceBarcodeImage = null;
                            OrderDispatchedMaster.invoice_no = invoiceNumber;
                            OrderDispatchedMaster.TotalAmount = totalAmt;
                            OrderDispatchedMaster.orderDetails = new List<OrderDispatchedDetails>();
                            #region Tax Calculation
                            List<int> itemids = null;
                            List<ItemMaster> itemslist = null;

                            itemids = odm.orderDetails.Select(x => x.ItemId).Distinct().ToList();
                            itemslist = db.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();

                            foreach (var pc in odm.orderDetails)
                            {
                                OrderDispatchedDetails odd = new OrderDispatchedDetails();

                                odd.OrderId = pc.OrderId;
                                odd.CustomerId = pc.CustomerId;
                                odd.CustomerName = pc.CustomerName;
                                odd.City = pc.City;
                                odd.CityId = pc.CityId;
                                odd.Mobile = pc.Mobile;
                                odd.CompanyId = 1;
                                odd.WarehouseId = (int)pc.WarehouseId;
                                odd.WarehouseName = wh.WarehouseName;
                                odd.CategoryName = pc.CategoryName;
                                odd.SubcategoryName = pc.SubcategoryName;
                                odd.SubsubcategoryName = pc.SubsubcategoryName;
                                odd.SellingSku = pc.SellingSku;
                                odd.ItemId = pc.ItemId;
                                odd.itemname = pc.itemname;
                                odd.itemcode = pc.itemcode;
                                odd.SellingUnitName = pc.SellingUnitName;
                                odd.itemNumber = pc.itemNumber;
                                odd.HSNCode = pc.HSNCode;
                                odd.Barcode = pc.Barcode;
                                odd.price = pc.price;
                                odd.UnitPrice = pc.UnitPrice;
                                odd.Purchaseprice = pc.Purchaseprice;
                                odd.Noqty = pc.Noqty;
                                odd.MinOrderQty = pc.MinOrderQty;
                                odd.MinOrderQtyPrice = pc.MinOrderQtyPrice;
                                odd.qty = pc.qty;
                                odd.TotalAmt = (pc.UnitPrice * pc.qty);
                                odd.CreatedDate = DateTime.Now;
                                odd.OrderDate = DateTime.Now;
                                odd.ItemMultiMRPId = pc.ItemMultiMRPId;
                                odd.NetAmmount = (pc.UnitPrice * pc.qty);
                                odd.UpdatedDate = DateTime.Now;
                                odd.SizePerUnit = 1;
                                odd.IsFreeItem = pc.IsFreeItem;
                                odd.IsDispatchedFreeStock = pc.IsDispatchedFreeStock;
                                odd.PTR = pc.PTR;

                                int MOQ = pc.MinOrderQty;
                                odd.MinOrderQtyPrice = MOQ * pc.UnitPrice;
                                odd.TaxPercentage = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).TotalTaxPercentage : 0;
                                odd.TotalCessPercentage = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).TotalCessPercentage : 0;// items.TotalCessPercentage;
                                if (odd.TaxPercentage >= 0)
                                {
                                    odd.SGSTTaxPercentage = odd.TaxPercentage / 2;
                                    odd.CGSTTaxPercentage = odd.TaxPercentage / 2;
                                }
                                odd.HSNCode = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).HSNCode : null;//items.HSNCode;
                                odd.Noqty = pc.qty;//qty; // for total qty (no of items)    
                                odd.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
                                //if there is cess for that item

                                if (odd.TotalCessPercentage > 0)
                                {
                                    // pc.TotalCessPercentage = odd.TotalCessPercentage;
                                    double tempPercentagge = odd.TotalCessPercentage + odd.TaxPercentage;

                                    odd.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge / 100)) / 100;
                                    odd.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
                                    odd.CessTaxAmount = (odd.AmtWithoutAfterTaxDisc * odd.TotalCessPercentage) / 100;
                                }
                                double tempPercentagge2 = odd.TotalCessPercentage + odd.TaxPercentage;
                                odd.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge2 / 100)) / 100;
                                odd.AmtWithoutAfterTaxDisc = (100 * odd.AmtWithoutTaxDisc) / (100 + pc.DiscountPercentage);
                                odd.TaxAmmount = (odd.AmtWithoutAfterTaxDisc * odd.TaxPercentage) / 100;
                                if (odd.TaxAmmount >= 0)
                                {
                                    odd.SGSTTaxAmmount = odd.TaxAmmount / 2;
                                    odd.CGSTTaxAmmount = odd.TaxAmmount / 2;
                                }

                                //for cess
                                if (odd.CessTaxAmount > 0)
                                {
                                    double tempPercentagge3 = odd.TotalCessPercentage + odd.TaxPercentage;
                                    odd.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                    odd.AmtWithoutAfterTaxDisc = (100 * odd.AmtWithoutTaxDisc) / (100 + 0);
                                    odd.TotalAmountAfterTaxDisc = odd.AmtWithoutAfterTaxDisc + odd.CessTaxAmount + odd.TaxAmmount;
                                }
                                else
                                {
                                    odd.TotalAmountAfterTaxDisc = odd.AmtWithoutAfterTaxDisc + odd.TaxAmmount;
                                }

                                odd.DiscountAmmount = 0;
                                odd.NetAmtAfterDis = 0;
                                odd.Deleted = false;

                                odd.OrderDetailsId = pc.OrderDetailsId;
                                odd.OrderId = pc.OrderId;
                                odd.OrderDate = indianTime;
                                OrderDispatchedMaster.orderDetails.Add(odd);
                            }
                            #endregion
                            //bool resReturnStatus = salesReturnOrderHelper.PostOrderStatus(orderMaster.OrderId, "Return_Ready_to_Dispatch", userid, db);
                            //if (db.Commit() > 0)
                            //{
                            db.OrderDispatchedMasters.Add(OrderDispatchedMaster);
                            db.Commit();
                            bool status = false;
                            if (orderMaster != null && orderMaster.OrderType != 5)
                            {
                                var BatchList = new List<BatchCodeSubjectDc>();
                                MultiStockHelper<POCStockEntryDc> MultiStockHelpers = new MultiStockHelper<POCStockEntryDc>();
                                List<TransferOrderItemBatchMasterDc> TransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();
                                List<TransferOrderItemBatchMasterDc> FreeStockTransferOrderItemBatchMasterList = new List<TransferOrderItemBatchMasterDc>();

                                foreach (var StockHit in OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0))
                                {
                                    //var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                    bool isFree = orderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    var dispatchId = new SqlParameter("@EntityId", StockHit.OrderDispatchedDetailsId);
                                    var MultiMRPid = new SqlParameter("@ItemMultiMrpId", StockHit.ItemMultiMRPId);
                                    var warehouseid = new SqlParameter("@WarehouseId", StockHit.WarehouseId);
                                    var ReturnQty = new SqlParameter("@Qty", StockHit.qty);
                                    var Userid = new SqlParameter("@UserId", userid);
                                    var RefStockCode = new SqlParameter("@RefStockCode", isFree == true ? "F" : "C");
                                    db.Database.ExecuteSqlCommand("EXEC InsertReturnDeliveryCancelStocks " +
                                                                    " @EntityId,@ItemMultiMrpId,@WarehouseId,@Qty,@UserId,@RefStockCode",
                                                                    dispatchId, MultiMRPid, warehouseid, ReturnQty, Userid, RefStockCode);
                                    db.Commit();
                                    status = true;
                                }
                                if (status)
                                {
                                    {
                                        #region stock Hit on poc
                                        foreach (var StockHit in OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0))
                                        {
                                            var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                            bool isFree = orderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                            if (isFree) { RefStockCode = "F"; }
                                            else if (orderMaster.OrderType == 6) //6 Damage stock
                                            {
                                                RefStockCode = "D";
                                            }
                                            else if (orderMaster.OrderType == 9) //9 Non Sellable Stock
                                            {
                                                RefStockCode = "N";
                                            }
                                            //foreach (var item in OrderDispatchedMaster.orderDetails.Where(d => d.OrderDispatchedDetailsId == StockHit.OrderDispatchedDetailsId))
                                            //{
                                            if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                            {
                                                var FreeStockId = db.FreeStockDB.FirstOrDefault(x => x.ItemMultiMRPId == StockHit.ItemMultiMRPId && x.WarehouseId == StockHit.WarehouseId).FreeStockId;
                                                foreach (var item in salesReturn.returnItemDetails.Where(x => x.Returnqty > 0 && x.ItemMultiMRPId == StockHit.ItemMultiMRPId && x.IsFreeItem == StockHit.IsFreeItem).SelectMany(x => x.ItemWiseBatchCodeLists))
                                                {
                                                    var StockBatchMasterId = db.StockBatchMasters.FirstOrDefault(x => x.StockId == FreeStockId && x.StockType == "F").Id;
                                                    FreeStockTransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                                                    {

                                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                        Qty = item.Returnqty,
                                                        StockBatchMasterId = StockBatchMasterId,
                                                        WarehouseId = StockHit.WarehouseId,
                                                        ObjectId = StockHit.OrderId,
                                                        ObjectIdDetailId = StockHit.OrderDispatchedDetailsId
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                var CurrentStockId = db.DbCurrentStock.FirstOrDefault(x => x.ItemMultiMRPId == StockHit.ItemMultiMRPId && x.WarehouseId == StockHit.WarehouseId).StockId;
                                                foreach (var item in salesReturn.returnItemDetails.Where(x => x.Returnqty > 0 && x.ItemMultiMRPId == StockHit.ItemMultiMRPId && x.IsFreeItem == StockHit.IsFreeItem).SelectMany(x => x.ItemWiseBatchCodeLists))
                                                {
                                                    var StockBatchMasterId = db.StockBatchMasters.FirstOrDefault(x => x.StockId == CurrentStockId && x.StockType == "C").Id;
                                                    TransferOrderItemBatchMasterList.Add(new TransferOrderItemBatchMasterDc
                                                    {

                                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                        Qty = item.Returnqty,
                                                        StockBatchMasterId = StockBatchMasterId,
                                                        WarehouseId = StockHit.WarehouseId,
                                                        ObjectId = StockHit.OrderId,
                                                        ObjectIdDetailId = StockHit.OrderDispatchedDetailsId
                                                    });
                                                }
                                            }
                                            //foreach (var batch in salesReturn.returnItemDetails.Where(x => x.Returnqty > 0 && x.ItemMultiMRPId == StockHit.ItemMultiMRPId && x.IsFreeItem == StockHit.IsFreeItem).SelectMany(x => x.ItemWiseBatchCodeLists))
                                            //{
                                            //    BatchList.Add(new BatchCodeSubjectDc
                                            //    {
                                            //        ObjectDetailId = StockHit.OrderDispatchedDetailsId,
                                            //        ObjectId = StockHit.OrderDispatchedDetailsId,
                                            //        StockType = RefStockCode,
                                            //        Quantity = batch.Returnqty,
                                            //        WarehouseId = StockHit.WarehouseId,
                                            //        ItemMultiMrpId = StockHit.ItemMultiMRPId
                                            //    });
                                            //}
                                            //}
                                        }
                                        if (TransferOrderItemBatchMasterList.Any())
                                        {
                                            BatchMasterManager batchMasterManager = new BatchMasterManager();
                                            foreach (var StockHit in OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0))
                                            {
                                                var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                bool isFree = OrderDispatchedMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                if (isFree) { RefStockCode = "F"; }
                                                else if (orderMaster.OrderType == 6) //6 Damage stock
                                                {
                                                    RefStockCode = "D";
                                                }
                                                else if (orderMaster.OrderType == 9) //9 Non Sellable Stock
                                                {
                                                    RefStockCode = "N";
                                                }
                                                POCStockList.Add(new POCStockEntryDc
                                                {
                                                    ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                                    OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                                    OrderId = StockHit.OrderId,
                                                    Qty = StockHit.qty,
                                                    UserId = userid,
                                                    WarehouseId = StockHit.WarehouseId,
                                                    RefStockCode = RefStockCode
                                                });
                                            }
                                            bool res = MultiStockHelpers.MakeEntry(POCStockList, "Stock_OnPOC_New", db, dbContextTransaction);
                                            if (res)
                                            {
                                                if (FreeStockTransferOrderItemBatchMasterList.Any())
                                                {
                                                    var FreeStockTxnType = db.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "OrderInFree" && x.IsDeleted == false);
                                                    bool Res = batchMasterManager.AddQty(FreeStockTransferOrderItemBatchMasterList, db, userid, FreeStockTxnType.Id);
                                                    if (!Res)
                                                    {
                                                        return new APIResponse { Status = false, Message = "Somthing Went Wrong" };
                                                    }
                                                }
                                                var StockTxnType = db.StockTxnTypeMasters.FirstOrDefault(x => x.IsActive && x.StockTxnType == "OrderInCurrent" && x.IsDeleted == false);
                                                bool batchRes = batchMasterManager.AddQty(TransferOrderItemBatchMasterList, db, userid, StockTxnType.Id);
                                                if (!batchRes)
                                                {
                                                    return new APIResponse { Status = false, Message = "Somthing Went Wrong" };
                                                }
                                            }
                                            if (!res)
                                            {
                                                return new APIResponse { Status = false, Message = "Somthing Went Wrong" };
                                            }
                                            else
                                            {
                                                #region Insert in FIFO
                                                if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                                                {
                                                    List<GrDC> items = OrderDispatchedMaster.orderDetails.Where(x => x.qty > 0).Select(x => new GrDC
                                                    {
                                                        ItemMultiMrpId = x.ItemMultiMRPId,
                                                        WarehouseId = OrderDispatchedMaster.WarehouseId,
                                                        Source = "Cancel In",
                                                        CreatedDate = indianTime,
                                                        POId = x.OrderId,
                                                        Qty = x.qty,
                                                        Price = x.UnitPrice,

                                                    }).ToList();
                                                    foreach (var item in items)
                                                    {
                                                        RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                                        rabbitMqHelper.Publish("POC", item);
                                                    }
                                                }
                                                #endregion
                                                //if (orderMaster.OrderType != 9 && BatchList != null && BatchList.Any())
                                                //{
                                                //    Publisher.PublishOrderIn(BatchList);
                                                //}
                                            }
                                        }
                                        #endregion
                                        string PocinvoiceNumber = " ";
                                        if (orderMaster.WarehouseId != 67 && orderMaster.WarehouseId != 80 && orderMaster.OrderType != 3)
                                        {
                                            PocinvoiceNumber = db.Database.SqlQuery<string>("EXEC spGetPocCNCurrentNumber 'PocCreditNote', " + wh.Stateid).FirstOrDefault();

                                        }
                                        OrderDispatchedMaster.PocCreditNoteDate = OrderDispatchedMaster.PocCreditNoteNumber != null ? OrderDispatchedMaster.PocCreditNoteDate : indianTime;
                                        OrderDispatchedMaster.PocCreditNoteNumber = OrderDispatchedMaster.PocCreditNoteNumber != null ? OrderDispatchedMaster.PocCreditNoteNumber : PocinvoiceNumber;
                                        db.Entry(OrderDispatchedMaster).State = EntityState.Modified;
                                        //db.OrderDispatchedMasters.Add(OrderDispatchedMaster);
                                        if (db.Commit() > 0)
                                        {
                                            dbContextTransaction.Complete();
                                            return new APIResponse { Status = true, Data = orderMaster.OrderId, Message = "Order Generated Successfully" };
                                        }
                                        else
                                        {
                                            dbContextTransaction.Dispose();
                                            return new APIResponse { Status = false, Message = "Somthing Went Wrong" };
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return new APIResponse { Status = false, Message = "Somthing Went Wrong" };
                            }
                            //}
                            //else
                            //{
                            //    return new APIResponse { Status = false, Message = "Somthing Went Wrong" };
                            //}
                        }
                        return new APIResponse { Status = true, Data = orderMaster.OrderId, Message = "Order Generated Successfully" };
                    }
                    else
                        return new APIResponse { Status = false, Message = "Somthing Went Wrong" };
                }
                else
                    return new APIResponse { Status = false, Message = "Return qty can not zero." };

            }
        }

        [Route("ExportStoreConfig")]
        [HttpGet]
        public List<ExportStoreConfigDC> ExportStoreConfig(int Warehouseid)
        {
            using (var db = new AuthContext())
            {
                List<ExportStoreConfigDC> res = new List<ExportStoreConfigDC>();
                var param = new SqlParameter("@warehouseid", Warehouseid);
                res = db.Database.SqlQuery<ExportStoreConfigDC>("exec Sp_Exportstoreconfig  @warehouseid", param).ToList();
                return res;
            }
        }

        [Route("WarehouseQrDeviceAdd")]
        [HttpPost]
        [AllowAnonymous]
        public bool WarehouseQrDevice(WarehouseQrDC warehouseQrDC)
        {
            using (var db = new AuthContext())
            {
                var warehouseqrdata = db.WarehouseQrDevices.FirstOrDefault(x => x.WarehouseId == warehouseQrDC.WarehouseId && x.IsActive);
                if (warehouseqrdata != null)
                {
                    warehouseqrdata.IsActive = false;
                    db.Entry(warehouseqrdata).State = EntityState.Modified;
                    warehouseqrdata = new WarehouseQrDevice();
                    warehouseqrdata.WarehouseId = warehouseQrDC.WarehouseId;
                    warehouseqrdata.FcmId = warehouseQrDC.FcmId;
                    warehouseqrdata.IsActive = true;
                    warehouseqrdata.IsDeleted = false;
                    warehouseqrdata.CreatedDate = DateTime.Now;
                    db.WarehouseQrDevices.Add(warehouseqrdata);
                }
                else
                {
                    warehouseqrdata = new WarehouseQrDevice();
                    warehouseqrdata.WarehouseId = warehouseQrDC.WarehouseId;
                    warehouseqrdata.FcmId = warehouseQrDC.FcmId;
                    warehouseqrdata.IsActive = true;
                    warehouseqrdata.IsDeleted = false;
                    warehouseqrdata.CreatedDate = DateTime.Now;
                    db.WarehouseQrDevices.Add(warehouseqrdata);
                }
                db.Commit();

            }
            return true;
        }


        // for store qr scan app
        [Route("GetstoreWarehouse")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<WarehouseStoreTypeDC>> GetstoreWarehouse()
        {
            var result = new List<WarehouseStoreTypeDC>();
            using (var db = new AuthContext())
            {
                result = db.Warehouses.Where(x => x.StoreType == 1 && x.active && !x.Deleted).Select(y => new WarehouseStoreTypeDC
                {
                    WarehouseId = y.WarehouseId,
                    WarehouseName = y.WarehouseName
                }).ToList();
            }
            return result;
        }

        public string IndexToLetter(int index)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string letter = string.Empty;

            while (index >= 0)
            {
                letter = alphabet[index % 26] + letter;
                index = (index / 26) - 1;
            }
            letter += ")";
            return letter;
        }

        //[Route("testapiforzerostock")]
        //[HttpGet]
        //public bool ConsumerCurrentZeroStockAutoLive()
        //{
        //    ReportManager reportManager = new ReportManager();
        //    bool res = reportManager.ConsumerCurrentZeroStockAutoLive();
        //    return res;
        //}

    }
}

