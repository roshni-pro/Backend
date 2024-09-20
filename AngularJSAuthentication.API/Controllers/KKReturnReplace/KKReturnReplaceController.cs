using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.EwayBill;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.EwayBill;
using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.API.Helper.Notification;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.constants;
using AngularJSAuthentication.DataContracts.EwayBill;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.ClearTax;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.BillDiscount;
using AngularJSAuthentication.Model.ClearTax;
using AngularJSAuthentication.Model.PlaceOrder;
using AngularJSAuthentication.Model.SalesApp;
using AngularJSAuthentication.Model.Stocks;
using BarcodeLib;
using GenricEcommers.Models;
using LinqKit;
using Newtonsoft.Json;
using NLog;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Hosting;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.NotificationController;
using static AngularJSAuthentication.API.EwayBill.InternalEwaybillController;
using static AngularJSAuthentication.DataContracts.EwayBill.EwaybillByIRNDc;
using static AngularJSAuthentication.DataContracts.EwayBill.InternalTransferEwaybillDc;
using BuyerDtls = AngularJSAuthentication.DataContracts.EwayBill.EwaybillByIRNDc.BuyerDtls;

namespace AngularJSAuthentication.API.Controllers.KKReturnReplace
{
    [RoutePrefix("api/KKReturnReplace")]
    public class KKReturnReplaceController : ApiController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public double xPointValue = AppConstants.xPoint;

        public string BaseUrl = string.Empty;
        public static string eInvoiceAuthKey = string.Empty;
        public string eInvoiceVersion = string.Empty;
        //public string BaseUrl = string.Empty;
        public static string authtoken = string.Empty;
        #region  Order Dispatched Detail
        /// <summary>
        /// GetList () Staus : ('Delivered', 'sattled',) 
        /// </summary>
        /// <param name="OrderId"></param>
        /// <returns></returns>

        [Route("")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<OrderDispatchedDetailDc>> GetODList(int CustomerId, int OrderId)
        {
            KKReturnReplaceManager manager = new KKReturnReplaceManager();
            return await manager.GetODList(CustomerId, OrderId);
        }
        #endregion
        [AllowAnonymous]
        [Route("GetLastSevenDaysOrderList")]
        [HttpGet]
        public async Task<List<int>> GetLastSevenDaysOrderList(int CustomerId)
        {
            KKReturnReplaceManager manager = new KKReturnReplaceManager();
            return await manager.GetLastSevenDaysOrderList(CustomerId);
        }

        [Route("PostData")]
        [HttpPost]

        public bool PostData(PostKKReturnReplaceRequestDc data)
        {
            using (AuthContext context = new AuthContext())
            {

                using (var dbContextTransaction = context.Database.BeginTransaction())
                {

                    bool result = false;
                    if (data != null)
                    {
                        var ORDERID = data.OrderId;
                        // var KKRequest = context.KKReturnReplaceRequests.Where(y => y.OrderId == data.OrderId && y.CustomerId == data.CustomerId && y.RequestType == data.RequestType).FirstOrDefault();
                        var oderDiapatched = context.OrderDispatchedMasters.Where(y => y.OrderId == ORDERID).FirstOrDefault();
                        var people = context.Peoples.Where(y => y.Mobile == oderDiapatched.DboyMobileNo).FirstOrDefault();
                        var ordermaster = context.DbOrderMaster.Where(y => y.OrderId == ORDERID).FirstOrDefault();
                        //if (KKRequest == null || KKRequest.CreatedDate.Date != indianTime.Date )
                        //{
                        if (data.Details != null && data.Details.Any())
                        {
                            KKReturnReplaceRequest KKReturnReplaceRequest = new KKReturnReplaceRequest();
                            // KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                            KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                            List<KKReturnReplaceDetail> AddKKReturnReplaceDetail = new List<KKReturnReplaceDetail>();
                            if (ordermaster.Deliverydate.AddDays(7) >= DateTime.Now.AddYears(-2))
                            {
                                KKReturnReplaceRequest.CustomerId = data.CustomerId;
                                KKReturnReplaceRequest.OrderId = data.OrderId;
                                KKReturnReplaceRequest.RequestType = data.RequestType;
                                KKReturnReplaceRequest.Status = data.Status;
                                KKReturnReplaceRequest.Cust_Comment = data.Cust_Comment;
                                KKReturnReplaceRequest.DBoyId = people.PeopleID;
                                KKReturnReplaceRequest.CreatedBy = data.CustomerId;
                                KKReturnReplaceRequest.CreatedDate = indianTime;
                                KKReturnReplaceRequest.ModifiedDate = indianTime;
                                KKReturnReplaceRequest.IsActive = false;
                                context.KKReturnReplaceRequests.Add(KKReturnReplaceRequest);
                                context.Commit();
                                KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(KKReturnReplaceRequest.Id);
                                KKRequestReplaceHistory.Status = data.Status;
                                KKRequestReplaceHistory.CreatedBy = data.CustomerId;
                                KKRequestReplaceHistory.CreatedDate = indianTime;
                                KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                                var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);
                                if (addrequest)
                                {
                                    foreach (var item in data.Details)
                                    {
                                        KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                        KKReturnReplaceDetail.KKRRRequestId = KKReturnReplaceRequest.Id;
                                        KKReturnReplaceDetail.OrderDetailsId = item.OrderDetailsId;
                                        KKReturnReplaceDetail.Qty = item.ReturnQty;///QTY IN TABLE ME RETURN
                                        KKReturnReplaceDetail.ReturnReason = item.ReturnReason;
                                        //KKReturnReplaceRequest.Details.Add(KKReturnReplaceDetail);
                                        AddKKReturnReplaceDetail.Add(KKReturnReplaceDetail);
                                    }
                                    if (AddKKReturnReplaceDetail != null && AddKKReturnReplaceDetail.Any())
                                    {

                                        context.KKReturnReplaceDetails.AddRange(AddKKReturnReplaceDetail);
                                        if (context.Commit() > 0)
                                        {
                                            dbContextTransaction.Commit();
                                            result = true;
                                        }
                                    }
                                }
                                if (!result)
                                {
                                    dbContextTransaction.Rollback();
                                }
                            }
                            //if (context.Commit() > 0)
                            //{

                            //}
                            //else
                            //{ result = false; }
                        }

                        // }
                        //else
                        //{
                        //    List<KKReturnReplaceDetail> AddKKReturnReplaceDetail = new List<KKReturnReplaceDetail>();
                        //    if (data.Details != null && data.Details.Any())
                        //    {
                        //        if (ordermaster.Deliverydate.AddDays(7) >= DateTime.Now)
                        //        {
                        //            foreach (var item in data.Details)
                        //            {
                        //                KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                        //                KKReturnReplaceDetail.KKRRRequestId = KKRequest.Id;
                        //                KKReturnReplaceDetail.OrderDetailsId = item.OrderDetailsId;
                        //                KKReturnReplaceDetail.Qty = item.ReturnQty;///QTY IN TABLE ME RETURN
                        //                KKReturnReplaceDetail.ReturnReason = item.ReturnReason;
                        //                AddKKReturnReplaceDetail.Add(KKReturnReplaceDetail);
                        //            }
                        //            if (AddKKReturnReplaceDetail != null && AddKKReturnReplaceDetail.Any())
                        //            {

                        //                context.KKReturnReplaceDetails.AddRange(AddKKReturnReplaceDetail);
                        //                if (context.Commit() > 0)
                        //                {
                        //                    dbContextTransaction.Commit();
                        //                    result = true;
                        //                }
                        //            }
                        //            if (!result)
                        //            {
                        //                dbContextTransaction.Rollback();
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    return result;

                }

            }
        }


        [Route("GetReturnReplaceOrderList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<GetReturnReplaceOrderDC>> GetReturnReplaceOrderList(int CustomerId)
        {

            KKReturnReplaceManager manager = new KKReturnReplaceManager();
            return await manager.GetReturnReplaceOrderList(CustomerId);

        }

        [Route("GetReturnReplaceOrderForDBoy")]
        [HttpGet]
        public async Task<List<GetReturnReplaceOrderForDBoyDC>> GetReturnReplaceOrderForDBoy(int DboyId)
        {

            KKReturnReplaceManager manager = new KKReturnReplaceManager();
            return await manager.GetReturnReplaceOrderForDBoy(DboyId);

        }

        [Route("GetWarehouseRejectOrderForDBoy")]
        [HttpGet]
        public async Task<List<GetReturnReplaceOrderForDBoyDC>> GetWarehouseRejectOrderForDBoy(int DboyId)
        {

            KKReturnReplaceManager manager = new KKReturnReplaceManager();
            return await manager.GetWarehouseRejectOrderForDBoy(DboyId);

        }

        [Route("ChangeStatus")]
        [HttpGet]
        public bool CancelKKReturn(int KKReturnReplaceId, string Status, int? dboyId, string picker_comment, string ReturnReplaceImage)
        {
            using (AuthContext db = new AuthContext())
            {
                var KKReturnReplaceRequests = db.KKReturnReplaceRequests.Where(x => x.Id == KKReturnReplaceId).FirstOrDefault();
                if (KKReturnReplaceRequests != null)
                {

                    KKReturnReplaceRequests.Status = Status;  // string status
                    KKReturnReplaceRequests.ModifiedDate = DateTime.Now;
                    if (dboyId != 0)
                    {
                        KKReturnReplaceRequests.ModifiedBy = dboyId;
                        KKReturnReplaceRequests.Picker_PersonId = dboyId ?? 0;
                        KKReturnReplaceRequests.Picker_Comment = picker_comment;
                        KKReturnReplaceRequests.ReturnReplaceImage = ReturnReplaceImage;
                    }
                    else
                    {
                        KKReturnReplaceRequests.ModifiedBy = KKReturnReplaceRequests.CustomerId;
                    }

                    // for history 
                    KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                    KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(KKReturnReplaceRequests.Id);
                    KKRequestReplaceHistory.Status = Status;  // string status 
                    KKRequestReplaceHistory.CreatedDate = DateTime.Now;

                    if (dboyId != 0)
                    {
                        KKRequestReplaceHistory.CreatedBy = dboyId ?? 0;
                    }
                    else
                    {
                        KKRequestReplaceHistory.CreatedBy = KKReturnReplaceRequests.CustomerId;
                    }

                    // for history 
                    db.KKRequestReplaceHistorys.Add(KKRequestReplaceHistory);
                    db.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        [Route("GetReturnReplaceOrderForDashboard")]
        [HttpGet]
        public async Task<List<KKReturnReplaceDashboardDC>> GetReturnReplaceOrderForDashboard(int warehouseid, bool IsPlanner = false)
        {
            if (!IsSalesReturn())
            {
                return new List<KKReturnReplaceDashboardDC>();
            }
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<string> roleNames = new List<string>();

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            int roleId = 0;

            if (roleId == 0)
            {
                if (roleNames.Any(x => x == "Inbound Lead") && !IsPlanner)
                    roleId = 1;
                else if (roleNames.Any(x => x == "Buyer"))
                    roleId = 3;
                else if (roleNames.Any(x => x == "WH delivery planner" || x == "Hub delivery planner") && IsPlanner)
                    roleId = 5;
                else
                    roleId = 0;
            }

            KKReturnReplaceManager manager = new KKReturnReplaceManager();
            return await manager.GetReturnReplaceOrderForDashboard(warehouseid, roleId, userid);

        }


        [Route("UpdateDeliveryBoy")]
        [HttpPut]
        public bool UpdateDeliveryBoy(int DboyId, int KkRequestid)
        {
            using (AuthContext db = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var KKRequest = db.KKReturnReplaceRequests.Where(y => y.Id == KkRequestid).FirstOrDefault();
                KKRequest.DBoyId = DboyId;
                KKRequest.ModifiedBy = userid;
                KKRequest.ModifiedDate = indianTime;
                db.Entry(KKRequest).State = EntityState.Modified;
                db.Commit();
                return true;
            }


        }


        [Route("UpdateKKReturnReplace")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateData(KKReturnReplaceDashboardDC kk)
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(90);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
            {
                using (AuthContext db = new AuthContext())
                {

                    bool result = false;
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;
                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    List<PhysicalStockUpdateRequestDc> StockTransfertoDamage = new List<PhysicalStockUpdateRequestDc>();

                    var KKReturnReplaceRequests = db.KKReturnReplaceRequests.Where(x => x.Id == kk.KkRequestId && x.Status != kk.Status).FirstOrDefault();

                    if (KKReturnReplaceRequests != null)
                    {

                        KKReturnReplaceRequests.Status = kk.Status;  // string status
                        KKReturnReplaceRequests.ModifiedDate = DateTime.Now;
                        KKReturnReplaceRequests.ModifiedBy = userid;

                        var ShoppingCarts = new ShoppingCart();
                        var OrderDetailsId = db.KKReturnReplaceDetails.Where(x => x.KKRRRequestId == KKReturnReplaceRequests.Id).Select(x => x.OrderDetailsId).ToList();
                        var OrderDetails = db.OrderDispatchedDetailss.Where(x => OrderDetailsId.Contains(x.OrderDetailsId)).ToList();
                        var OrderLineItemCount = db.OrderDispatchedDetailss.Where(x => x.OrderId == KKReturnReplaceRequests.OrderId).Count();
                        var KKRequestDetails = db.KKReturnReplaceDetails.Where(x => x.KKRRRequestId == KKReturnReplaceRequests.Id).ToList();
                        var customer = db.Customers.Where(x => x.CustomerId == KKReturnReplaceRequests.CustomerId).FirstOrDefault();
                        var people = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                        var ordermaster = db.DbOrderMaster.Where(x => x.OrderId == KKReturnReplaceRequests.OrderId).Include(x => x.orderDetails).FirstOrDefault();
                        var orderDispatchMaster = db.OrderDispatchedMasters.Where(x => x.OrderId == KKReturnReplaceRequests.OrderId).FirstOrDefault();

                        var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == ordermaster.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");


                        if (kk.Status == "Received at Warehouse")//  kk.Warehouse_Comment != null && kk.HQ_Comment == null"
                        {

                            KKReturnReplaceRequests.Receiver_PersonId = userid;
                            KKReturnReplaceRequests.Warehouse_Comment = kk.Warehouse_Comment;
                        }
                        else if (kk.Status == "Settled by QC")// kk.Warehouse_Comment != null && kk.HQ_Comment != null"
                        {
                            KKReturnReplaceRequests.Settled_PersonId = userid;
                            KKReturnReplaceRequests.HQ_Comment = kk.HQ_Comment;
                            if (KKReturnReplaceRequests.Status != "Cancel")
                            {
                                if (KKReturnReplaceRequests.RequestType == 1)
                                {
                                    bool isInventoryIn = UpdateCurrentInventory(ordermaster, OrderDetails, KKRequestDetails, people, KKReturnReplaceRequests, db, dbContextTransaction);
                                    if (isInventoryIn)
                                    {
                                        Wallet wallet = db.WalletDb.Where(c => c.CustomerId == KKReturnReplaceRequests.CustomerId).FirstOrDefault();
                                        double price = 0;//;OrderDetails.Sum(x => x.TotalAmt);
                                        foreach (var i in KKRequestDetails)
                                        {
                                            var od = db.OrderDispatchedDetailss.Where(c => c.OrderDetailsId == i.OrderDetailsId).FirstOrDefault();
                                            price += od.UnitPrice * i.Qty;
                                        }
                                        wallet.CreditAmount = (price * 10);
                                        wallet.Through = "Replace";
                                        wallet.CompanyId = customer.CompanyId != null ? Convert.ToInt32(customer.CompanyId) : 0;
                                        wallet.WarehouseId = Convert.ToInt32(customer.Warehouseid);
                                        var walletAmount = wallet.CreditAmount / 10;
                                        var addWalletPoints = postWalletbyCustomeridWid(wallet, userid, db, kk.OrderId);
                                        if (addWalletPoints != null)
                                        {
                                            ShoppingCarts.itemDetails = OrderDetails.Select(x => new IDetail
                                            {
                                                qty = db.KKReturnReplaceDetails.Where(y => y.OrderDetailsId == x.OrderDetailsId).Select(y => y.Qty).FirstOrDefault(),
                                                ItemId = x.ItemId,
                                                CompanyId = x.CompanyId,
                                                WarehouseId = x.WarehouseId,
                                                FreeItemId = 0,
                                                FreeItemqty = 0,
                                                OfferId = 0,
                                                OfferWalletPoint = 0,
                                                OfferCategory = 0,
                                                UnitPrice = x.UnitPrice,
                                                NewUnitPrice = 0,
                                            }).ToList();
                                            ShoppingCarts.Customerphonenum = customer.Mobile;
                                            ShoppingCarts.CustomerName = customer.Name;
                                            ShoppingCarts.CustomerType = customer.CustomerType;
                                            ShoppingCarts.ShopName = customer.ShopName;
                                            ShoppingCarts.ShippingAddress = customer.ShippingAddress;
                                            ShoppingCarts.Skcode = customer.Skcode;
                                            ShoppingCarts.OrderId = KKReturnReplaceRequests.OrderId;
                                            ShoppingCarts.status = "Replace";
                                            ShoppingCarts.deliveryCharge = 0;
                                            ShoppingCarts.WalletAmount = Convert.ToDouble(walletAmount);
                                            ShoppingCarts.walletPointUsed = Convert.ToDouble(wallet.CreditAmount);
                                            ShoppingCarts.DialEarnigPoint = 0;
                                            ShoppingCarts.TotalAmount = 0;
                                            ShoppingCarts.CreatedDate = indianTime;
                                            ShoppingCarts.Savingamount = 0;
                                            ShoppingCarts.OnlineServiceTax = 0;
                                            ShoppingCarts.BillDiscountAmount = 0;
                                            ShoppingCarts.paymentMode = "Cash";
                                            var order = await db.PushOrderMasterV6(ShoppingCarts);
                                            if (order == null)
                                            {
                                                dbContextTransaction.Dispose();
                                                return Request.CreateResponse(HttpStatusCode.OK, false);

                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbContextTransaction.Dispose();
                                        return Request.CreateResponse(HttpStatusCode.OK, false);
                                    }
                                }
                                else if (KKReturnReplaceRequests.RequestType == 0)
                                {
                                    var wallet = db.WalletDb.Where(c => c.CustomerId == KKReturnReplaceRequests.CustomerId).FirstOrDefault();
                                    var walletHistory = db.CustomerWalletHistoryDb.Where(c => c.CustomerId == KKReturnReplaceRequests.CustomerId && c.OrderId == KKReturnReplaceRequests.OrderId && ((c.Through == "From Order Delivered") || (c.Through == "Used On Order"))).ToList();

                                    var removePoints = walletHistory.Sum(x => x.NewAddedWAmount != null ? x.NewAddedWAmount : 0 + x.NewOutWAmount != null ? x.NewOutWAmount : 0);
                                    double price = 0;//OrderDetails.Sum(x => x.TotalAmt);

                                    int? ReducePoint = 0;
                                    foreach (var i in KKRequestDetails)
                                    {
                                        var od = db.OrderDispatchedDetailss.Where(c => c.OrderDetailsId == i.OrderDetailsId).FirstOrDefault();
                                        price += od.UnitPrice * i.Qty;
                                        if (ordermaster.orderDetails != null && ordermaster.orderDetails.Any(x => x.OrderDetailsId == i.OrderDetailsId))
                                        {
                                            var oditem = ordermaster.orderDetails.Where(x => x.OrderDetailsId == i.OrderDetailsId).SingleOrDefault();
                                            if (oditem != null)
                                            {
                                                int? MP = 0;
                                                double xPoint = 0;

                                                if (ordermaster.OrderTakenSalesPersonId == 0)
                                                {
                                                    xPoint = xPointValue * 10; //Customer (0.2  10=1)
                                                }
                                                else
                                                {
                                                    xPoint = xPointValue * 10; //Salesman (0.02  10=1)
                                                }
                                                double WithTaxNetPurchasePrice = Math.Round(oditem.NetPurchasePrice * (1 + (oditem.TaxPercentage / 100)), 3);
                                                MP = Convert.ToInt32((oditem.UnitPrice - WithTaxNetPurchasePrice) * xPoint);
                                                ReducePoint += MP * i.Qty;
                                            }
                                        }
                                    }
                                    if (ReducePoint > 0)
                                    {
                                        wallet.DebitAmount = ReducePoint;
                                        wallet.Through = "Return";
                                        wallet.CompanyId = customer.CompanyId != null ? Convert.ToInt32(customer.CompanyId) : 0;
                                        wallet.WarehouseId = Convert.ToInt32(customer.Warehouseid);
                                        ReduceWalletPoint(wallet, userid, db, kk.OrderId);
                                    }
                                    wallet.DebitAmount = 0;
                                    wallet.CreditAmount = (price * 10);
                                    //}
                                    wallet.Through = "Return";
                                    wallet.CompanyId = customer.CompanyId != null ? Convert.ToInt32(customer.CompanyId) : 0;
                                    wallet.WarehouseId = Convert.ToInt32(customer.Warehouseid);
                                    wallet.IsNotExpirable = true;
                                    var isInventoryIn = UpdateCurrentInventory(ordermaster, OrderDetails, KKRequestDetails, people, KKReturnReplaceRequests, db, dbContextTransaction);
                                    if (isInventoryIn)
                                    {
                                        var addWalletPoints = postWalletbyCustomeridWid(wallet, userid, db, kk.OrderId);
                                        ordermaster.OrderType = 3;
                                        db.Entry(ordermaster).State = EntityState.Modified;
                                        //db.Commit();
                                        //var orderid = new SqlParameter
                                        //{
                                        //    ParameterName = "orderid",
                                        //    Value = KKReturnReplaceRequests.OrderId
                                        //};

                                        //string  ProcnoteNumber = db.Database.SqlQuery<string>("GenerateCreditNoteAfterReturn @OrderID", orderid).FirstOrDefault();
                                    }
                                    else
                                    {
                                        dbContextTransaction.Dispose();
                                        return Request.CreateResponse(HttpStatusCode.OK, false);
                                    }
                                }
                            }

                        }
                        else if (kk.Status == "Settled Rejected Order by QC")
                        {
                            KKReturnReplaceRequests.Settled_PersonId = userid;
                            KKReturnReplaceRequests.HQ_Comment = kk.HQ_Comment;
                            KKReturnReplaceRequests.ManualWalletPoint = kk.ManualWalletPoint >= 0 ? kk.ManualWalletPoint : 0;
                        }
                        else if (kk.Status == "Warehouse Accept Reject Order")
                        {
                            KKReturnReplaceRequests.Receiver_PersonId = userid;
                            KKReturnReplaceRequests.Warehouse_Comment = kk.Warehouse_Comment;
                            List<DamageStockHistory> AddDamageStock = new List<DamageStockHistory>();
                            var wallet = db.WalletDb.Where(c => c.CustomerId == KKReturnReplaceRequests.CustomerId).FirstOrDefault();
                            foreach (var i in OrderDetails)
                            {
                                var KKDetail = db.KKReturnReplaceDetails.Where(x => x.OrderDetailsId == i.OrderDetailsId).SingleOrDefault();
                                DamageStock dst = db.DamageStockDB.Where(x => x.ItemNumber == i.itemNumber && x.WarehouseId == i.WarehouseId && x.ItemMultiMRPId == i.ItemMultiMRPId).SingleOrDefault();
                                if (dst == null)
                                {
                                    DamageStock objst = new DamageStock();
                                    objst.WarehouseId = i.WarehouseId;
                                    objst.WarehouseName = i.WarehouseName;
                                    objst.ItemId = i.ItemId;
                                    objst.MRP = i.price;
                                    objst.ItemMultiMRPId = i.ItemMultiMRPId;
                                    objst.ItemNumber = i.itemNumber;
                                    objst.ItemName = i.itemname;
                                    objst.DamageInventory = KKDetail.Qty;
                                    double netUnitPrice = Math.Round(i.UnitPrice, 2);
                                    objst.UnitPrice = netUnitPrice;
                                    objst.PurchasePrice = netUnitPrice;
                                    objst.ReasonToTransfer = KKReturnReplaceRequests.RequestType == 1 ? "Replace" : "Return";
                                    objst.CreatedDate = indianTime;
                                    objst.CompanyId = i.CompanyId;
                                    objst.Deleted = false;
                                    db.DamageStockDB.Add(objst);
                                    db.Commit();

                                    PhysicalStockUpdateRequestDc obj = new PhysicalStockUpdateRequestDc();

                                    obj.ItemMultiMRPId = objst.ItemMultiMRPId;
                                    obj.WarehouseId = objst.WarehouseId;
                                    obj.Qty = KKDetail.Qty;
                                    StockTransfertoDamage.Add(obj);

                                }
                                else
                                {


                                    PhysicalStockUpdateRequestDc obj = new PhysicalStockUpdateRequestDc();

                                    obj.ItemMultiMRPId = dst.ItemMultiMRPId;
                                    obj.WarehouseId = dst.WarehouseId;
                                    obj.Qty = KKDetail.Qty;
                                    StockTransfertoDamage.Add(obj);


                                }
                            }
                            if (AddDamageStock != null && AddDamageStock.Any())
                            {

                                db.DamageStockHistoryDB.AddRange(AddDamageStock);
                                result = true;
                            }

                            wallet.CreditAmount = KKReturnReplaceRequests.ManualWalletPoint;
                            wallet.Through = KKReturnReplaceRequests.RequestType == 1 ? "Replace" : "Return";
                            wallet.CompanyId = customer.CompanyId != null ? Convert.ToInt32(customer.CompanyId) : 0;
                            wallet.WarehouseId = Convert.ToInt32(customer.Warehouseid);
                            wallet.IsNotExpirable = true;
                            var addWalletPoints = postWalletbyCustomeridWid(wallet, userid, db, kk.OrderId);
                            if (addWalletPoints == null)
                            {
                                dbContextTransaction.Dispose();
                                return Request.CreateResponse(HttpStatusCode.OK, false);
                            }
                        }
                        // for history 
                        KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                        KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(KKReturnReplaceRequests.Id);
                        KKRequestReplaceHistory.Status = kk.Status;  // string status 
                        KKRequestReplaceHistory.CreatedDate = DateTime.Now;
                        KKRequestReplaceHistory.CreatedBy = userid;
                        // for history 
                        db.KKRequestReplaceHistorys.Add(KKRequestReplaceHistory);


                        if (db.Commit() > 0)
                        {
                            if (StockTransfertoDamage != null && StockTransfertoDamage.Any())
                            {
                                bool isInventoryIn = UpdateCurrentInventory(ordermaster, OrderDetails, KKRequestDetails, people, KKReturnReplaceRequests, db, dbContextTransaction);
                                if (isInventoryIn)
                                {
                                    foreach (var st in StockTransfertoDamage)
                                    {
                                        StockTransactionHelper helper = new StockTransactionHelper();
                                        PhysicalStockUpdateRequestDc StockTransferToDamageobj = new PhysicalStockUpdateRequestDc();
                                        StockTransferToDamageobj.ItemMultiMRPId = st.ItemMultiMRPId;
                                        StockTransferToDamageobj.WarehouseId = st.WarehouseId;
                                        StockTransferToDamageobj.Qty = st.Qty;
                                        StockTransferToDamageobj.SourceStockType = StockTypeTableNames.CurrentStocks;// "CurrentStocks";
                                        StockTransferToDamageobj.DestinationStockType = StockTypeTableNames.DamagedStock;// "DamagedStocks";
                                        StockTransferToDamageobj.StockTransferType = StockTransferTypeName.DamagedStocks;
                                        StockTransferToDamageobj.Reason = KKReturnReplaceRequests.RequestType == 1 ? "Replace" : "Return";
                                        bool isupdated = helper.TransferBetweenPhysicalStocks(StockTransferToDamageobj, userid, db, dbContextTransaction);
                                        if (isupdated)
                                        {
                                            result = true;
                                        }
                                        else
                                        {
                                            result = false;
                                        }
                                    }
                                }
                            }
                            dbContextTransaction.Complete();
                            result = true;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }
        }

        public bool UpdateCurrentInventory(OrderMaster _OrderMaster, List<OrderDispatchedDetails> OrderDetails, List<KKReturnReplaceDetail> KKRequestDetails, People people, KKReturnReplaceRequest KKReturnReplaceRequests, AuthContext db, TransactionScope dbContextTransaction)
        {
            bool result = false;
            try
            {
                List<OrderDispatchedDetails> ReturnList = new List<OrderDispatchedDetails>();// AddFreeStockHistory
                foreach (var i in OrderDetails)
                {
                    var KKDetail = KKRequestDetails.Where(x => x.OrderDetailsId == i.OrderDetailsId).SingleOrDefault();
                    if (KKDetail != null)
                    {
                        bool freeItemMultiMrpId = _OrderMaster.orderDetails.Any(x => x.IsDispatchedFreeStock && x.OrderDetailsId == i.OrderDetailsId);
                        if (freeItemMultiMrpId)
                        {
                            if (i.qty > 0)
                            {
                                i.IsDispatchedFreeStock = true;
                                i.IsFreeItem = true;
                                i.qty = KKDetail.Qty;
                                ReturnList.Add(i);
                            }
                        }
                        else
                        {

                            if (i.qty > 0)
                            {
                                i.IsDispatchedFreeStock = false;
                                i.IsFreeItem = false;
                                i.qty = KKDetail.Qty;
                                ReturnList.Add(i);

                            }
                        }
                    }
                }

                #region stock Hit on kk return
                //for currentstock
                MultiStockHelper<POCStockEntryDc> MultiStockHelpers = new MultiStockHelper<POCStockEntryDc>();
                List<POCStockEntryDc> POCStockList = new List<POCStockEntryDc>();
                foreach (var StockHit in ReturnList.Where(x => x.qty > 0))
                {
                    var RefStockCode = "C";

                    if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock) { RefStockCode = "F"; }
                    else if (_OrderMaster.OrderType == 6) //6 Damage stock
                    {
                        RefStockCode = "D";
                    }
                    POCStockList.Add(new POCStockEntryDc
                    {
                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                        OrderId = StockHit.OrderId,
                        Qty = StockHit.qty,
                        UserId = people.PeopleID,
                        WarehouseId = StockHit.WarehouseId,
                        RefStockCode = RefStockCode
                    });
                }

                if (POCStockList.Any())
                {
                    bool res = MultiStockHelpers.MakeEntry(POCStockList, "Stock_OnPOC_New", db, dbContextTransaction);
                    if (!res)
                    {
                        dbContextTransaction.Dispose();
                        result = false;

                    }
                    else
                    {
                        result = true;
                    }
                }

                #endregion


                if (db.Commit() > 0)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                dbContextTransaction.Dispose();
                throw ex;
            }
            return result;
        }

        //Add Wallet Points

        public Wallet postWalletbyCustomeridWid(Wallet wallet, int PeopleId, AuthContext db, int orderid)
        {
            bool result = false;
            try
            {
                var walt = db.WalletDb.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                if (walt != null)
                {
                    CustomerWalletHistory od = new CustomerWalletHistory();
                    try
                    {
                        od.CustomerId = wallet.CustomerId;
                        //op by user
                        try
                        {
                            People People = db.Peoples.Where(c => c.PeopleID == PeopleId).SingleOrDefault();
                            od.PeopleId = People.PeopleID;
                            od.PeopleName = People.DisplayName;

                        }
                        catch (Exception ss) { }
                        od.WarehouseId = wallet.WarehouseId;
                        od.Through = wallet.Through;
                        od.CompanyId = wallet.CompanyId;
                        od.comment = wallet.Comment;
                        od.OrderId = orderid;
                        if (wallet.CreditAmount >= 0)
                        {
                            od.NewAddedWAmount = wallet.CreditAmount;
                            od.TotalWalletAmount = walt.TotalAmount + wallet.CreditAmount;
                        }
                        if (wallet.CreditAmount < 0)
                        {
                            od.NewAddedWAmount = wallet.CreditAmount;
                            od.TotalWalletAmount = walt.TotalAmount + (wallet.CreditAmount);
                        }
                        od.UpdatedDate = indianTime;
                        od.TransactionDate = indianTime;
                        od.CreatedDate = indianTime;
                        db.CustomerWalletHistoryDb.Add(od);
                        db.Commit();
                    }
                    catch (Exception exx)
                    {

                    }
                    walt.CustomerId = wallet.CustomerId;
                    if (wallet.CreditAmount < 0)
                    {
                        walt.TotalAmount += wallet.CreditAmount;
                        walt.UpdatedDate = indianTime;
                    }
                    if (wallet.CreditAmount >= 0)
                    {
                        walt.TotalAmount += wallet.CreditAmount;
                        walt.UpdatedDate = indianTime;
                        try
                        {
                            //Wallet Trigger :wallet amount added more than Rs.5000
                            if (wallet.CreditAmount > 5000)
                            {
                                var cust = db.Customers.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                                SendMailCreditWalletNotification(wallet.CreditAmount, od.PeopleName, cust.Name, cust.Skcode, indianTime, wallet.Through);
                            }
                            ForWalletNotification(wallet.CustomerId, wallet.CreditAmount, db);
                        }
                        catch (Exception ex) { }

                    }
                    if (wallet.DebitAmount > 0)
                    {
                        walt.TotalAmount -= wallet.DebitAmount;
                        walt.TransactionDate = indianTime;
                    }
                    //WalletDb.Attach(walt);
                    db.Entry(walt).State = EntityState.Modified;
                    db.Commit();

                }
                else
                {
                    wallet.CustomerId = wallet.CustomerId;

                    if (wallet.CreditAmount > 0)
                    {
                        wallet.TotalAmount = wallet.CreditAmount;
                        wallet.UpdatedDate = indianTime;
                        try
                        {
                            ForWalletNotification(wallet.CustomerId, wallet.CreditAmount, db);
                        }
                        catch (Exception ex) { }

                    }
                    wallet.CreatedDate = indianTime;
                    wallet.UpdatedDate = indianTime;
                    wallet.Deleted = false;
                    db.WalletDb.Add(wallet);
                    db.Commit();

                    CustomerWalletHistory od = new CustomerWalletHistory();
                    od.CustomerId = wallet.CustomerId;
                    //op by user
                    try
                    {
                        People People = db.Peoples.Where(c => c.PeopleID == PeopleId).SingleOrDefault();
                        od.PeopleId = People.PeopleID;
                        od.PeopleName = People.DisplayName;
                    }
                    catch (Exception ss) { }

                    //op by Cust
                    try
                    {
                        //CustWarehouse cust = CustWarehouseDB.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                        Customer cust = db.Customers.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                        //od.WarehouseId = cust.WarehouseId ?? 0;
                        od.WarehouseId = cust.Warehouseid ?? 0;
                        od.CompanyId = cust.CompanyId ?? 0;
                    }
                    catch (Exception cs) { }

                    od.Through = wallet.Through;
                    if (wallet.CreditAmount != 0)
                    {
                        od.NewAddedWAmount = wallet.CreditAmount;
                        od.TotalWalletAmount = walt.TotalAmount + wallet.CreditAmount;
                        //Wallet Trigger :wallet amount added more than Rs.5000
                        if (wallet.CreditAmount > 5000)
                        {
                            var cust = db.Customers.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                            SendMailCreditWalletNotification(wallet.CreditAmount, od.PeopleName, cust.Name, cust.Skcode, indianTime, wallet.Through);
                        }
                    }
                    od.UpdatedDate = indianTime;
                    od.TransactionDate = indianTime;
                    od.CreatedDate = indianTime;
                    od.OrderId = orderid;
                    db.CustomerWalletHistoryDb.Add(od);
                    db.Commit();
                }

                return walt;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }

        }

        public bool ReduceWalletPoint(Wallet wallet, int PeopleId, AuthContext db, int orderid)
        {
            bool result = false;
            try
            {
                var walt = db.WalletDb.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                if (walt != null)
                {
                    CustomerWalletHistory od = new CustomerWalletHistory();

                    od.CustomerId = wallet.CustomerId;
                    //op by user

                    People People = db.Peoples.Where(c => c.PeopleID == PeopleId).SingleOrDefault();
                    od.PeopleId = People.PeopleID;
                    od.PeopleName = People.DisplayName;



                    od.WarehouseId = wallet.WarehouseId;
                    od.Through = wallet.Through;
                    od.CompanyId = wallet.CompanyId;
                    od.comment = wallet.Comment;
                    od.OrderId = orderid;
                    if (wallet.DebitAmount >= 0)
                    {
                        od.NewOutWAmount = wallet.DebitAmount;
                        od.TotalWalletAmount = walt.TotalAmount - wallet.DebitAmount;
                    }
                    od.UpdatedDate = indianTime;
                    od.TransactionDate = indianTime;
                    od.CreatedDate = indianTime;
                    if (od != null)
                    {
                        db.CustomerWalletHistoryDb.Add(od);
                        result = true;
                    }
                    else
                    {
                        return false;
                    }


                    walt.CustomerId = wallet.CustomerId;
                    //if (wallet.CreditAmount < 0)
                    //{
                    //    walt.TotalAmount -= wallet.CreditAmount;
                    //    walt.UpdatedDate = indianTime;
                    //}
                    //if (wallet.CreditAmount >= 0)
                    //{
                    //    walt.TotalAmount += wallet.CreditAmount;
                    //    walt.UpdatedDate = indianTime;
                    //    //Wallet Trigger :wallet amount added more than Rs.5000
                    //    if (wallet.CreditAmount > 5000)
                    //    {
                    //        var cust = db.Customers.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                    //        AuthContext.SendMailCreditWalletNotification(wallet.CreditAmount, od.PeopleName, cust.Name, cust.Skcode, indianTime, wallet.Through);
                    //    }
                    //    ForWalletNotification(wallet.CustomerId, wallet.CreditAmount, db);



                    //}
                    if (wallet.DebitAmount >= 0)
                    {
                        walt.TotalAmount -= wallet.DebitAmount;
                        walt.TransactionDate = indianTime;
                    }
                    //WalletDb.Attach(walt);
                    if (walt != null)
                    {
                        db.Entry(walt).State = EntityState.Modified;
                        result = true;
                    }
                    else
                    {
                        return false;
                    }


                }
                //else
                //{
                //    wallet.CustomerId = wallet.CustomerId;

                //    if (wallet.CreditAmount > 0)
                //    {
                //        wallet.TotalAmount = wallet.CreditAmount;
                //        wallet.UpdatedDate = indianTime;

                //        ForWalletNotification(wallet.CustomerId, wallet.CreditAmount, db);


                //    }
                //    wallet.CreatedDate = indianTime;
                //    wallet.UpdatedDate = indianTime;
                //    wallet.Deleted = false;
                //    if (wallet != null)
                //    {
                //        db.WalletDb.Add(wallet);
                //        result = true;
                //    }
                //    else
                //    {
                //        return false;
                //    }


                //    CustomerWalletHistory od = new CustomerWalletHistory();
                //    od.CustomerId = wallet.CustomerId;
                //    //op by user

                //    People People = db.Peoples.Where(c => c.PeopleID == PeopleId).SingleOrDefault();
                //    od.PeopleId = People.PeopleID;
                //    od.PeopleName = People.DisplayName;


                //    //op by Cust

                //    //CustWarehouse cust = CustWarehouseDB.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                //    Customer cust = db.Customers.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                //    //od.WarehouseId = cust.WarehouseId ?? 0;
                //    od.WarehouseId = cust.Warehouseid ?? 0;
                //    od.CompanyId = cust.CompanyId ?? 0;


                //    od.Through = wallet.Through;
                //    if (wallet.CreditAmount != 0)
                //    {
                //        od.NewAddedWAmount = wallet.CreditAmount;
                //        od.TotalWalletAmount = walt.TotalAmount + wallet.CreditAmount;
                //        //Wallet Trigger :wallet amount added more than Rs.5000
                //        if (wallet.CreditAmount > 5000)
                //        {
                //            var cust1 = db.Customers.Where(c => c.CustomerId == wallet.CustomerId).SingleOrDefault();
                //            AuthContext.SendMailCreditWalletNotification(wallet.CreditAmount, od.PeopleName, cust1.Name, cust1.Skcode, indianTime, wallet.Through);
                //        }
                //    }
                //    od.UpdatedDate = indianTime;
                //    od.TransactionDate = indianTime;
                //    od.CreatedDate = indianTime;
                //    if (od != null)
                //    {
                //        db.CustomerWalletHistoryDb.Add(od);
                //        result = true;
                //    }
                //    else
                //    {
                //        return false;
                //    }

                //}

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }

        }

        public bool PushOrderMasterV6(ShoppingCart sc, AuthContext db)
        {

            bool Result = false;
            try
            {
                Customer cust = new Customer();
                var rsaKey = string.Empty;
                var hdfcOrderId = string.Empty;
                var eplOrderId = string.Empty;

                OrderMaster objOrderMaster = new OrderMaster();
                var placeOrderResponse = new PlaceOrderResponse();
                List<BillDiscount> BillDiscounts = new List<BillDiscount>();
                cust = db.Customers.FirstOrDefault(c => !c.Deleted && c.Mobile.Equals(sc.Customerphonenum));

                if (cust == null)
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Customer not found.";
                    placeOrderResponse.cart = null;
                    return Result;
                }
                var currentappVersion = db.appVersionDb.FirstOrDefault(x => x.isCompulsory);
                if (sc.SalesPersonId.HasValue && sc.SalesPersonId.Value == 0 && string.IsNullOrEmpty(sc.APPVersion) && currentappVersion != null && currentappVersion.App_version != sc.APPVersion)
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = "Please update you App. before placing order.";
                    placeOrderResponse.cart = null;
                    return Result;
                }

                var cluster = cust.ClusterId.HasValue ? db.Clusters.FirstOrDefault(x => x.ClusterId == cust.ClusterId.Value) : null;

                List<int> offerItemId = new List<int>();
                List<int> FlashDealOrderId = new List<int>();



                if (cust.CompanyId > 0)
                {
                    var warehouse = db.Warehouses.Where(w => w.WarehouseId == cust.Warehouseid).Select(c => c).SingleOrDefault();

                    var isWareHouseLive = db.GMWarehouseProgressDB.FirstOrDefault(x => x.WarehouseID == warehouse.WarehouseId)?.IsLaunched;

                    if (isWareHouseLive.HasValue && !isWareHouseLive.Value)
                    {
                        placeOrderResponse.IsSuccess = false;
                        placeOrderResponse.Message = "We are coming soon to your location.";
                        placeOrderResponse.cart = null;
                        return Result;
                    }

                    //var appHome = AppHomeDynamicDb.Where(x => x.Wid == warehouse.WarehouseId && x.delete == false && x.active == true && x.detail.Any(y => y.IsFlashDeal == true && y.active == true && y.StartOfferDate <= indianTime
                    //                              && y.EndOfferDate >= indianTime)).Include("detail").ToList();
                    //List<AppHomeItem> appHomeItems = appHome.SelectMany(x => x.detail).ToList();


                    var appHome = db.AppHomeSectionsDB.Where(x => x.WarehouseID == warehouse.WarehouseId && x.Deleted == false && x.Active == true && x.AppItemsList.Any(y => y.IsFlashDeal == true && y.Active == true && y.OfferStartTime <= indianTime
                                                      && y.OfferEndTime >= indianTime)).Include(x => x.AppItemsList).ToList();
                    List<AppHomeItem> appHomeItems = appHome.SelectMany(x => x.AppItemsList.Select(y => new AppHomeItem
                    {
                        active = y.Active,
                        ItemId = y.ItemId,
                        id = y.SectionItemID,
                        IsFlashDeal = y.IsFlashDeal
                    })).ToList();

                    List<int> appHomeItemids = appHomeItems.Where(x => x.active == true).Select(y => y.ItemId).ToList(); //appHome.SelectMany(x => x.detail.Select(y => y.ItemId)).ToList();
                    var cartItemIds = sc.itemDetails.Where(x => x.OfferCategory == 2 && appHomeItemids.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                    var itemIds = sc.itemDetails.Select(x => x.ItemId).ToList();
                    var freeItemIds = sc.itemDetails.Where(x => x.FreeItemId > 0 && x.FreeItemqty > 0).Select(x => x.FreeItemId).ToList();
                    var itemMastersList = db.itemMasters.Where(x => itemIds.Contains(x.ItemId) && x.WarehouseId == cust.Warehouseid).ToList();
                    var itemNumbers = itemMastersList.GroupBy(x => new { x.Number, x.ItemMultiMRPId }).ToList();
                    var FreeitemsList = db.itemMasters.Where(x => freeItemIds.Contains(x.ItemId) && x.WarehouseId == cust.Warehouseid).Select(x => x).ToList();

                    List<ItemLimitMaster> itemLimits = new List<ItemLimitMaster>();

                    itemNumbers.ForEach(x =>
                    {
                        var itemLimit = db.ItemLimitMasterDB.FirstOrDefault(z => z.ItemNumber == x.Key.Number && z.ItemMultiMRPId == x.Key.ItemMultiMRPId && z.WarehouseId == cust.Warehouseid && z.IsItemLimit);
                        if (itemLimit != null)
                            itemLimits.Add(itemLimit);
                    });



                    placeOrderResponse = ValidateShoppingCartHDFC(sc, warehouse, cust, appHomeItems, cartItemIds, itemMastersList, FreeitemsList, itemLimits, out objOrderMaster, out BillDiscounts, db);

                    if (placeOrderResponse.IsSuccess)
                    {

                        List<FlashDealItemConsumed> flashDealItemConsumedList = new List<FlashDealItemConsumed>();
                        List<OfferItem> offerItemsList = new List<OfferItem>();
                        var OfferUpdate = new List<Offer>();
                        //objOrderMaster = placeOrderResponse.OrderMaster;
                        double offerWalletPoint = 0;
                        if (cust.Active)
                        {
                            foreach (var i in placeOrderResponse.cart.itemDetails)
                            {

                                var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
                                if (i.OfferCategory == 2 && i.IsOffer)
                                {
                                    AppHomeSectionItems appHomeItem = db.AppHomeSectionItemsDB.Where(x => x.ItemId == i.ItemId && !x.Deleted && x.Active && x.IsFlashDeal == true && x.IsFlashDeal == true && x.OfferStartTime <= indianTime
                                            && x.OfferEndTime >= indianTime).FirstOrDefault();


                                    //AppHomeItem appHomeItem = appHomeItems.Where(x => x.ItemId == i.ItemId && x.IsFlashDeal == true && x.active == true && x.StartOfferDate <= indianTime
                                    //        && x.EndOfferDate >= indianTime).FirstOrDefault();
                                    //AppHomeItem appHomeItem = AppHomeItemDb.Where(x => x.ItemId == i.ItemId && x.IsFlashDeal == true && x.active == true && x.StartOfferDate <= indianTime
                                    //           && x.EndOfferDate >= indianTime).FirstOrDefault();
                                    if (appHomeItem != null)
                                    {

                                        items.OfferQtyAvaiable = items.OfferQtyAvaiable - i.qty;
                                        items.OfferQtyConsumed = items.OfferQtyConsumed + i.qty;
                                        if (items.OfferQtyAvaiable.Value <= 0)
                                        {
                                            items.IsOffer = false;
                                        }
                                        db.Entry(items).State = EntityState.Modified;

                                        appHomeItem.FlashDealQtyAvaiable = appHomeItem.FlashDealQtyAvaiable - i.qty;
                                        db.Entry(appHomeItem).State = EntityState.Modified;

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

                                #region Add if validate

                                if (i.IsOffer == true && i.FreeItemId > 0 && i.FreeItemqty > 0)
                                {
                                    #region Add if validated
                                    var offer = db.OfferDb.Where(x => x.OfferId == i.OfferId).SingleOrDefault();
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
                                    ff.itemId = offer.itemId;
                                    ff.itemname = offer.itemname;
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
                                    ff.ReferOfferId = i.OfferId;
                                    //offerItemId.Add(ff.OfferId);
                                    offerItemsList.Add(ff);

                                    #endregion
                                }


                                if (i.IsOffer == true && i.OfferWalletPoint > 0)
                                {
                                    //If offer is on wallet point then update is wallet point.
                                    offerWalletPoint = offerWalletPoint + i.OfferWalletPoint;
                                    var offerdata = db.OfferDb.Where(x => x.OfferId == i.OfferId).SingleOrDefault();
                                    OfferItem offerItem = new OfferItem();

                                    offerItem.CompanyId = i.CompanyId;
                                    offerItem.WarehouseId = i.WarehouseId;
                                    offerItem.itemId = offerdata.itemId;
                                    offerItem.itemname = offerdata.itemname;
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
                                    offerItem.OfferId = i.OfferId;
                                    offerItem.OfferType = "WalletPoint";
                                    offerItemsList.Add(offerItem);
                                    //offerItemId.Add(offerItem.OfferId);

                                }
                                #endregion
                                //For Item Deactive
                                #region Add if Validated
                                ItemLimitMaster ItemLimitMaster = db.ItemLimitMasterDB.Where(x => x.ItemNumber == items.Number && x.WarehouseId == items.WarehouseId && x.ItemMultiMRPId == items.ItemMultiMRPId).FirstOrDefault();
                                if (ItemLimitMaster != null && ItemLimitMaster.IsItemLimit == true)
                                {
                                    if (i.qty < ItemLimitMaster.ItemlimitQty || i.qty == 0)
                                    {
                                        ItemLimitMaster.ItemlimitQty = ItemLimitMaster.ItemlimitQty - i.qty;
                                        ItemLimitMaster.ItemLimitSaleQty = ItemLimitMaster.ItemLimitSaleQty + i.qty;
                                        db.Entry(ItemLimitMaster).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        ItemLimitMaster.ItemlimitQty = ItemLimitMaster.ItemlimitQty - i.qty;
                                        ItemLimitMaster.ItemLimitSaleQty = ItemLimitMaster.ItemLimitSaleQty + i.qty;
                                        ItemLimitMaster.IsItemLimit = false;//08/07/2019
                                        db.Entry(ItemLimitMaster).State = EntityState.Modified;

                                        ItemMaster itemlist = db.itemMasters.Where(x => x.Number == ItemLimitMaster.ItemNumber && x.WarehouseId == ItemLimitMaster.WarehouseId && x.ItemMultiMRPId == ItemLimitMaster.ItemMultiMRPId).FirstOrDefault();

                                        if (ItemLimitMaster.ItemlimitQty <= 0 || itemlist.MinOrderQty < ItemLimitMaster.ItemlimitQty)
                                        {
                                            //deactive
                                            List<ItemMaster> itemsDeactive = db.itemMasters.Where(x => x.Number == ItemLimitMaster.ItemNumber && x.WarehouseId == ItemLimitMaster.WarehouseId && x.ItemMultiMRPId == ItemLimitMaster.ItemMultiMRPId).ToList();
                                            foreach (var Ditem in itemsDeactive)
                                            {
                                                Ditem.active = false;
                                                Ditem.UpdatedDate = indianTime;
                                                Ditem.UpdateBy = "Auto Dective";
                                                db.Entry(Ditem).State = EntityState.Modified;
                                            }

                                        }
                                    }
                                }
                                #endregion
                            }
                        }

                        #region Rewards, Offers, FlashDeals, Wallet etc....

                        double rewardpoint = (double)objOrderMaster.orderDetails.Sum(x => x.marginPoint);

                        objOrderMaster.deliveryCharge = sc.deliveryCharge;

                        objOrderMaster.TotalAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmt) + objOrderMaster.deliveryCharge.Value, 2);
                        objOrderMaster.TaxAmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TaxAmmount + x.CessTaxAmount), 2);
                        objOrderMaster.SGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.SGSTTaxAmmount), 2);
                        objOrderMaster.CGSTTaxAmmount = System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.CGSTTaxAmmount), 2);
                        objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0);
                        objOrderMaster.DiscountAmount = 0;//System.Math.Round(objOrderMaster.orderDetails.Sum(x => x.TotalAmountAfterTaxDisc), 2);
                        objOrderMaster.OrderType = sc.status == "Replace" ? 3 : 1;
                        //add cluster to ordermaster
                        objOrderMaster.ClusterId = cust.ClusterId ?? 0;
                        objOrderMaster.ClusterName = cust.ClusterName;

                        var walletUsedPoint1 = sc.walletPointUsed;
                        var walletAmount1 = sc.WalletAmount;
                        CashConversion cash = db.CashConversionDb.FirstOrDefault(x => x.IsConsumer == false);

                        double rewPoint = 0;
                        double rewAmount = 0;


                        // call function

                        //removerd by Harry ( on 21)
                        //objOrderMaster = RewardAndWalletPointForPlacedOrder(placeOrderResponse.cart, offerWalletPoint, objOrderMaster, rewardpoint, cust, walletUsedPoint1, rewPoint, rewAmount, cash);

                        db.DbOrderMaster.Add(objOrderMaster);
                        // this.Commit();

                        objOrderMaster = RewardAndWalletPointForPlacedOrder(placeOrderResponse.cart, offerWalletPoint, objOrderMaster, rewardpoint, cust, walletUsedPoint1, rewPoint, rewAmount, cash, db);


                        if (OfferUpdate != null && OfferUpdate.Any())
                        {
                            foreach (var Offers in OfferUpdate)
                            {
                                db.Entry(Offers).State = EntityState.Modified;
                            }
                        }

                        if (cust != null)
                        {
                            cust.ShippingAddress = sc.ShippingAddress;
                            cust.ordercount = cust.ordercount + 1;
                            cust.MonthlyTurnOver = cust.MonthlyTurnOver + objOrderMaster.GrossAmount;
                            db.Entry(cust).State = EntityState.Modified;
                        }


                        //for first order

                        #endregion

                        if (!string.IsNullOrEmpty(sc.paymentThrough) && (sc.paymentThrough.ToLower().Contains("hdfc") || sc.paymentThrough.ToLower().Contains("truepay") || sc.paymentThrough.ToLower().Contains("epaylater")))//by Ashwin
                        {
                            objOrderMaster.Status = "Payment Pending";
                            objOrderMaster.paymentThrough = sc.paymentThrough;
                            objOrderMaster.paymentMode = "Online";
                        }
                        else
                        {
                            objOrderMaster.paymentMode = "COD";
                            objOrderMaster.Status = "Pending";
                        }

                        objOrderMaster.Status = cust.Active && cluster != null && cluster.Active ? objOrderMaster.Status : "Inactive";

                        #region Bill Discount
                        if (!string.IsNullOrEmpty(sc.BillDiscountOfferId))
                        {
                            foreach (var offer in BillDiscounts)
                            {
                                offer.OrderId = objOrderMaster.OrderId;
                                var scritchcartoffer = db.BillDiscountDb.FirstOrDefault(x => x.OfferId == offer.OfferId && x.CustomerId == cust.CustomerId && x.OrderId == 0 && x.BillDiscountType == "ScratchBillDiscount");

                                if (scritchcartoffer == null)
                                    db.BillDiscountDb.Add(offer);
                                else
                                {
                                    scritchcartoffer.OrderId = objOrderMaster.OrderId;
                                    scritchcartoffer.BillDiscountAmount = offer.BillDiscountAmount;
                                    scritchcartoffer.ModifiedBy = cust.CustomerId;
                                    scritchcartoffer.ModifiedDate = indianTime;
                                    db.Entry(scritchcartoffer).State = EntityState.Modified;
                                }
                            }
                            sc.BillDiscountAmount = BillDiscounts.Sum(x => x.BillDiscountAmount);
                        }

                        objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - (sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0);
                        objOrderMaster.BillDiscountAmount = sc.BillDiscountAmount.HasValue ? sc.BillDiscountAmount.Value : 0;
                        objOrderMaster.GrossAmount = System.Math.Round(objOrderMaster.TotalAmount, 0);
                        #endregion


                        bool sendNotification = false;

                        if (objOrderMaster.OrderId != 0)
                        {
                            MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart>();
                            var cartPredicate = PredicateBuilder.New<AngularJSAuthentication.Model.CustomerShoppingCart.CustomerShoppingCart>(x => x.CustomerId == objOrderMaster.CustomerId && x.WarehouseId == objOrderMaster.WarehouseId && !x.GeneratedOrderId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                            var customerShoppingCart = mongoDbHelper.Select(cartPredicate, null, null, null, collectionName: "CustomerShoppingCart").FirstOrDefault();
                            if (customerShoppingCart != null)
                            {
                                customerShoppingCart.GeneratedOrderId = objOrderMaster.OrderId;
                                bool status = mongoDbHelper.Replace(customerShoppingCart.Id, customerShoppingCart, "CustomerShoppingCart");
                            }
                            //Insert in PaymentHDFCPayments if PaymentThrough == "HDFC"

                            #region Offer, FlashDeal

                            if (offerItemsList != null && offerItemsList.Any())
                            {
                                foreach (var data in offerItemsList)
                                {
                                    data.OrderId = objOrderMaster.OrderId;
                                    //this.Entry(offerd).State = EntityState.Modified;

                                }

                                db.OfferItemDb.AddRange(offerItemsList);
                            }

                            //Update OrderId in FlashDealItemConsumedDB
                            if (flashDealItemConsumedList != null && flashDealItemConsumedList.Any())
                            {
                                // FlashDealOrderId = flashDealItemConsumedList.Select(x => x.FlashDealItemConsumedId).ToList();
                                foreach (var FlashDealOrderIdData in flashDealItemConsumedList)
                                {
                                    FlashDealOrderIdData.OrderId = objOrderMaster.OrderId;
                                }
                                db.FlashDealItemConsumedDB.AddRange(flashDealItemConsumedList);
                            }

                            #endregion

                            if (!string.IsNullOrEmpty(sc.paymentThrough))//  sc.paymentThrough == "HDFC")//by sudhir
                            {
                                var paymentThroughs = sc.paymentThrough.Split(',').ToList();

                                if (paymentThroughs.Count == 1 && paymentThroughs.Any(x => x.ToLower() == "cash"))
                                {
                                    sendNotification = true;
                                    db.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                    {
                                        amount = Math.Round(objOrderMaster.TotalAmount, 0),
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
                            }

                            if (string.IsNullOrEmpty(sc.paymentThrough))
                            {
                                sendNotification = true;
                                db.PaymentResponseRetailerAppDb.Add(new PaymentResponseRetailerApp
                                {
                                    amount = Math.Round(objOrderMaster.TotalAmount, 0),
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

                            //var od = DbOrderMaster.Where(x => x.OrderId == objOrderMaster.OrderId).FirstOrDefault();

                            string Borderid = Convert.ToString(objOrderMaster.OrderId);
                            string BorderCodeId = Borderid.PadLeft(11, '0');
                            temOrderQBcode code = db.GetBarcode(BorderCodeId);
                            objOrderMaster.InvoiceBarcodeImage = code.BarcodeImage;



                            //   objOrderMaster.invoice_no = "Od_" + Convert.ToString(objOrderMaster.OrderId);
                        }

                        db.Entry(objOrderMaster).State = EntityState.Modified;

                        //string invoice = objOrderMaster.invoice_no.ToString();

                        //Comment By Rakshit
                        //if (sendNotification)
                        //{
                        //    try
                        //    {
                        //        #region for first order
                        //        if (cust.ordercount == 1)//if this is customer first order
                        //        {
                        //            db.FirstCustomerOrder(cust, objOrderMaster);
                        //        }
                        //        #endregion

                        //        if (cust.ordercount > 1)
                        //        {
                        //            db.ForNotification(cust.CustomerId, objOrderMaster.GrossAmount);
                        //        }
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        logger.Error(ex.Message);
                        //    }
                        //}
                    }
                    //objOrderMaster = PrepareOrderMasterToInsert(objOrderMaster, warehouse, sc, cust);
                }


                if (placeOrderResponse.IsSuccess && objOrderMaster.OrderId > 0)
                {
                    PlacedOrderMasterDTM order = new PlacedOrderMasterDTM();
                    order.OrderId = objOrderMaster.OrderId;
                    order.CustomerId = objOrderMaster.CustomerId;
                    order.Skcode = objOrderMaster.Skcode;
                    order.WarehouseId = objOrderMaster.WarehouseId;
                    order.TotalAmount = objOrderMaster.TotalAmount;
                    var totalamt = objOrderMaster.TotalAmount + objOrderMaster.BillDiscountAmount ?? 0 + objOrderMaster.WalletAmount ?? 0;
                    order.WheelCount = Convert.ToInt32(Math.Floor(totalamt / 4000));
                    order.WheelAmountLimit = 4000;
                    order.DialEarnigPoint = 0;
                    decimal KisanDaanAmount = 0;
                    if (objOrderMaster.orderDetails != null && objOrderMaster.orderDetails.Any(x => !string.IsNullOrEmpty(x.SubcategoryName) && x.SubcategoryName.ToLower() == "kisan kirana"))
                    {
                        var KKAmount = Convert.ToDecimal(objOrderMaster.orderDetails.Where(x => !x.IsFreeItem && !string.IsNullOrEmpty(x.SubcategoryName) && x.SubcategoryName.ToLower() == "kisan kirana").Sum(x => x.qty * x.UnitPrice));
                        if (KKAmount > 0)
                        {
                            var KisanDaan = db.kisanDanMaster.FirstOrDefault(x => (x.OrderFromAmount <= KKAmount && x.OrderToAmount >= KKAmount) && x.IsActive);
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

                    placeOrderResponse.OrderMaster = order;
                    placeOrderResponse.EarnWalletPoint = BillDiscounts != null && BillDiscounts.Any(x => x.IsUsedNextOrder) ? Convert.ToInt32(BillDiscounts.Where(x => x.IsUsedNextOrder).Sum(x => x.BillDiscountTypeValue)) : 0;
                    placeOrderResponse.KisanDaanAmount = KisanDaanAmount;
                    placeOrderResponse.NotServicing = cluster == null || !cluster.Active;
                    if (placeOrderResponse.NotServicing)
                        placeOrderResponse.Message = "We are currently not servicing in your area. Our team will contact you soon.";

                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message.ToString());
                throw ex;
            }


        }

        //[Route("UpdateData")]
        //[HttpPut]
        //public dynamic UdateData(KKReturnReplaceRequestDC data)
        //{
        //    using (AuthContext context = new AuthContext())
        //    {
        //        KKReturnReplaceRequest KKReturnReplaceRequest = context.KKReturnReplaceRequests.Where(x => x.Id == data.Id).FirstOrDefault();
        //        context.Entry(KKReturnReplaceRequest).State = EntityState.Modified;
        //        context.Commit();
        //        return 0;
        //    }
        //}

        private async Task<bool> ForWalletNotification(int CustomerId, double? CreditAmount, AuthContext db)
        {
            bool res = false;
            try
            {
                //Notification notification = new Notification();
                //notification.title = "बधाई हो ! ";
                //notification.Message = "बधाई हो ! " + CreditAmount + " पॉइंट आपके वॉलेट में ऐड हुआ है ";
                //notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";
                //var customers = db.Customers.Where(x => x.fcmId != null && x.CustomerId == CustomerId).SingleOrDefault();
                ////AddNotification(notification);

                //string Key = ConfigurationManager.AppSettings["FcmApiKey"];
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
                    title = "बधाई हो ! ",
                    body = "बधाई हो ! " + CreditAmount + " पॉइंट आपके वॉलेट में ऐड हुआ है ",
                    image_url= "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png",
                    icon = "",
                    notificationCategory = "",
                    notificationType = "",
                    notify_type = "logout",
                    url = "",
                };
                string Key = ConfigurationManager.AppSettings["FcmApiKey"];
                var customers = db.Customers.Where(x => x.fcmId != null && x.CustomerId == CustomerId).SingleOrDefault();
                var firebaseService = new FirebaseNotificationServiceHelper(Key);
                var Result = await firebaseService.SendNotificationForApprovalAsync(customers.fcmId, data);
                if (Result != null)
                {
                    res = true;
                }
                else
                {
                    res = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error 2001 :Wallet Notification not send due to" + ex.Message);
            }
            return res;
        }

        private PlaceOrderResponse ValidateShoppingCartHDFC(ShoppingCart cart, Warehouse warehouse, Customer cust, List<AppHomeItem> appHomeItems, List<int> cartItemIds, List<ItemMaster> itemMastersList, List<ItemMaster> FreeitemsList, List<ItemLimitMaster> itemLimits, out OrderMaster objOrderMaster, out List<BillDiscount> BillDiscounts, AuthContext db)
        {
            try
            {
                var placeOrderResponse = new PlaceOrderResponse { IsSuccess = true, Message = string.Empty, cart = cart };
                objOrderMaster = new OrderMaster();
                objOrderMaster = PrepareOrderMasterToInsertHDFC(warehouse, cart, cust, db);
                objOrderMaster.orderDetails = new List<OrderDetails>();
                BillDiscounts = new List<BillDiscount>();
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
                if (apphomeitem != null)
                {
                    FlashDealItemConsumed = db.FlashDealItemConsumedDB.Where(x => itemids.Contains(x.ItemId) && x.FlashDealId == apphomeitem.id && x.CompanyId == cust.CustomerId).ToList();
                }


                var rewardpoint = 0;

                List<int> offerItemId = new List<int>();
                List<int> FlashDealOrderId = new List<int>();
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                List<DataContracts.Masters.Store.StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
                List<DataContracts.Masters.Store.ClusterStoreExecutiveDc> clusterStoreExecutiveDcs = retailerAppManager.GetStoreClusterExecutiveDetail();
                foreach (var i in placeOrderResponse.cart.itemDetails.Where(x => x.OfferCategory != 2).Select(x => x))
                {


                    i.IsSuccess = true;
                    if (i.qty <= 0)
                    {

                        i.IsSuccess = false;
                        i.Message = "Quantity is 0";
                    }
                    else if (i.qty != 0 && i.qty > 0)
                    {
                        var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
                        bool isOffer = items.IsOffer;
                        if (items == null)
                        {
                            i.IsSuccess = false;
                            i.Message = "Item is not found";
                        }
                        else
                        {
                            if (FlashDealItemConsumed != null && FlashDealItemConsumed.Any(x => x.ItemId == items.ItemId) && items.IsOffer)
                            {
                                items.IsOffer = false;
                            }

                            if (!items.active || items.Deleted)
                            {
                                i.IsSuccess = false;
                                i.Message = "Item is not Active";
                            }

                            var limit = itemLimits.FirstOrDefault(x => x.ItemNumber == items.ItemNumber && x.ItemMultiMRPId == items.ItemMultiMRPId);

                            if (limit != null && limit.ItemlimitQty < i.qty)
                            {
                                i.IsSuccess = false;
                                i.Message = "Item is not Active";
                            }


                            if (i.IsSuccess && i.UnitPrice != items.UnitPrice && cart.status != "Replace")
                            {
                                i.IsSuccess = false;
                                i.Message = "Item Unit Price has changed";
                                i.NewUnitPrice = items.UnitPrice;
                            }
                            else
                            {
                                OrderDetails od = new OrderDetails();
                                if (StoreCategorySubCategoryBrands != null && StoreCategorySubCategoryBrands.Any(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid))
                                {
                                    var store = StoreCategorySubCategoryBrands.First(x => x.Categoryid == items.Categoryid && x.SubCategoryId == items.SubCategoryId && x.BrandId == items.SubsubCategoryid);
                                    od.StoreId = store.StoreId;
                                    if (store.StoreId > 0 && clusterStoreExecutiveDcs != null && clusterStoreExecutiveDcs.Any(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId))
                                    {
                                        var clusterStoreExecutiveDc = clusterStoreExecutiveDcs.FirstOrDefault(a => a.StoreId == store.StoreId && a.ClusterId == cust.ClusterId);
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
                                od.UnitPrice = items.UnitPrice;
                                od.price = items.price;
                                od.MinOrderQty = items.MinOrderQty;
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
                                    var freeItemOffer = db.OfferDb.FirstOrDefault(x => i.OfferId == x.OfferId && x.WarehouseId == cust.Warehouseid);

                                    if (freeItemOffer == null || Freeitem == null)
                                    {
                                        i.IsSuccess = false;
                                        i.Message = "Item is not found";
                                    }
                                    if (Freeitem.Deleted || freeItemOffer.start > indianTime || freeItemOffer.end < indianTime || !freeItemOffer.IsActive)
                                    {
                                        i.IsSuccess = false;
                                        i.Message = "Free Item expired";
                                    } // Also check stock
                                    else
                                    {
                                        int? FreeOrderqty = i.FreeItemqty;
                                        if (freeItemOffer.QtyAvaiable < FreeOrderqty)
                                        {
                                            i.IsSuccess = false;
                                            i.Message = "Free Item expired";
                                        }
                                        else
                                        {
                                            OrderDetails od1 = new OrderDetails();
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
                                            od1.IsFreeItem = true;
                                            od1.FreeWithParentItemId = i.ItemId;
                                            od1.IsDispatchedFreeStock = db.OfferDb.SingleOrDefault(x => x.OfferId == i.OfferId).IsDispatchedFreeStock;//true mean stock hit from Freestock
                                            od1.UnitPrice = 0.01;
                                            od1.price = Freeitem.price;
                                            od1.MinOrderQty = 0;
                                            od1.MinOrderQtyPrice = 0;
                                            od1.qty = Convert.ToInt32(i.FreeItemqty);
                                            od1.SizePerUnit = 0;
                                            od1.TaxPercentage = 0;
                                            od1.SGSTTaxPercentage = 0;
                                            od1.CGSTTaxPercentage = 0;
                                            od1.Noqty = od1.qty; // for total qty (no of items)    
                                            od1.TotalAmt = 0;

                                            od1.TotalCessPercentage = 0;
                                            od1.AmtWithoutTaxDisc = 0;
                                            od1.AmtWithoutAfterTaxDisc = 0;
                                            od1.CessTaxAmount = 0;
                                            tempPercentagge2 = 0;
                                            od1.AmtWithoutTaxDisc = 0;
                                            od1.AmtWithoutAfterTaxDisc = 0;
                                            od1.TaxAmmount = 0;
                                            if (od1.TaxAmmount >= 0)
                                            {
                                                od1.SGSTTaxAmmount = 0;
                                                od1.CGSTTaxAmmount = 0;
                                            }
                                            //for cess
                                            if (od1.CessTaxAmount > 0)
                                            {
                                                //double temp = od.TaxPercentage + od.TotalCessPercentage;
                                                od1.AmtWithoutTaxDisc = 0;
                                                od1.AmtWithoutAfterTaxDisc = 0;
                                                od1.TotalAmountAfterTaxDisc = 0;
                                            }
                                            else
                                            {
                                                od1.TotalAmountAfterTaxDisc = 0;
                                            }
                                            od1.DiscountPercentage = 0;
                                            od1.DiscountAmmount = 0;
                                            DiscountAmmount = 0;
                                            NetAmtAfterDis = 0;
                                            od1.NetAmtAfterDis = 0;
                                            TaxAmmount = 0;
                                            od1.Purchaseprice = 0;
                                            od1.CreatedDate = indianTime;
                                            od1.UpdatedDate = indianTime;
                                            od1.Deleted = false;
                                            od1.marginPoint = 0;

                                            objOrderMaster.orderDetails.Add(od1);

                                        }


                                    }
                                }
                            }

                            items.IsOffer = isOffer;
                        }
                    }
                }

                foreach (var i in placeOrderResponse.cart.itemDetails.Where(x => x.OfferCategory == 2).Select(x => x))
                {


                    i.IsSuccess = true;
                    if (cartItemIds.Contains(i.ItemId))
                    {
                        if (i.qty <= 0)
                        {

                            i.IsSuccess = false;
                            i.Message = "Quantity is 0";
                        }
                        else if (i.qty != 0 && i.qty > 0)
                        {
                            var items = itemMastersList.Where(x => x.ItemId == i.ItemId && x.WarehouseId == i.WarehouseId).FirstOrDefault();
                            if (items == null)
                            {
                                i.IsSuccess = false;
                                i.Message = "Item is not found";
                            }
                            else
                            {
                                if (!items.active || items.Deleted)
                                {
                                    i.IsSuccess = false;
                                    i.Message = "Item is not Active";
                                }

                                var limit = itemLimits.FirstOrDefault(x => x.ItemNumber == items.ItemNumber && x.ItemMultiMRPId == items.ItemMultiMRPId);

                                if (limit != null && limit.ItemlimitQty < i.qty)
                                {
                                    i.IsSuccess = false;
                                    i.Message = "Item is not Active";
                                }

                                if (i.IsSuccess && i.UnitPrice != items.UnitPrice)
                                {
                                    i.IsSuccess = false;
                                    i.Message = "Item Unit Price has changed";
                                    i.NewUnitPrice = items.UnitPrice;
                                }
                                else
                                {
                                    OrderDetails od = new OrderDetails();
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

                                    //If OfferCategory is 2 then it is a flash deal then We Decrease the Quantity from item master and apphomeitem

                                    od.price = items.price;
                                    od.MinOrderQty = items.MinOrderQty;
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
                    List<Offer> Offers = db.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId) && x.IsDeleted == false && x.IsActive == true && x.end > indianTime).Include(x => x.OfferItemsBillDiscounts).Include(x => x.BillDiscountOfferSections).ToList();

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
                            if (db.BillDiscountDb.Any(x => x.OfferId == item.OfferId && x.CustomerId == cust.CustomerId && x.OrderId > 0))
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

                        foreach (var Offer in Offers)
                        {
                            var BillDiscount = new BillDiscount();
                            BillDiscount.CustomerId = cust.CustomerId;
                            BillDiscount.OfferId = Offer.OfferId;
                            BillDiscount.BillDiscountType = Offer.OfferOn;
                            double totalamount = 0;
                            var OrderLineItems = 0;
                            if (Offer.OfferOn != "ScratchBillDiscount")
                            {
                                List<int> Itemids = new List<int>();
                                if (Offer.BillDiscountType == "category" && Offer.BillDiscountOfferSections.Any())
                                {
                                    var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                    var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
                                    Itemids = itemMastersList.Where(x => ids.Contains(x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                                }
                                else if (Offer.BillDiscountType == "subcategory" && Offer.BillDiscountOfferSections.Any())
                                {
                                    AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                                    List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

                                    var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                    Itemids = itemMastersList.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.CategoryId).Contains(x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();

                                    totalamount = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                                }
                                else if (Offer.BillDiscountType == "brand" && Offer.BillDiscountOfferSections.Any())
                                {
                                    var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                    AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager manager = new AngularJSAuthentication.BusinessLayer.Managers.Masters.CustomersManager();
                                    List<AngularJSAuthentication.DataContracts.Masters.OfferBillDiscountItemDc> offerCatSubCats = manager.GetOfferBillDiscountItemById(Offer.OfferId);

                                    Itemids = itemMastersList.Where(x => offerCatSubCats.Select(y => y.Id + " " + y.SubCategoryId + " " + y.CategoryId).Contains(x.SubsubCategoryid + " " + x.SubCategoryId + " " + x.Categoryid) && !iteminofferlist.Contains(x.ItemId) && !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                                }
                                else if (Offer.BillDiscountType == "items")
                                {
                                    var iteminofferlist = Offer.OfferItemsBillDiscounts.Select(x => x.itemId).ToList();
                                    if (Offer.OfferItemsBillDiscounts.FirstOrDefault().IsInclude)
                                    {
                                        Itemids = itemMastersList.Where(x => iteminofferlist.Contains(x.ItemId)).Select(x => x.ItemId).ToList();
                                    }
                                    var incluseItemIds = itemMastersList.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : 0;
                                    OrderLineItems = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
                                }
                                else
                                {
                                    var ids = Offer.BillDiscountOfferSections.Select(x => x.ObjId).ToList();
                                    Itemids = itemMastersList.Where(x => ids.Contains(x.Categoryid)).Select(x => x.ItemId).ToList();
                                    var incluseItemIds = itemMastersList.Where(x => !(x.IsOffer && x.OfferType == "FlashDeal")).Select(x => x.ItemId).ToList();
                                    totalamount = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId) && incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice) : objOrderMaster.orderDetails.Where(x => incluseItemIds.Contains(x.ItemId)).Sum(x => x.qty * x.UnitPrice);
                                    OrderLineItems = Itemids.Any() ? objOrderMaster.orderDetails.Where(x => !Itemids.Contains(x.ItemId)).Count() : objOrderMaster.orderDetails.Count();
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
                            }
                            else
                            {
                                totalamount = objOrderMaster.orderDetails.Sum(x => x.qty * x.UnitPrice);
                                if (Offer.MaxBillAmount > 0 && totalamount > Offer.MaxBillAmount)
                                {
                                    totalamount = Offer.MaxBillAmount;
                                }
                                else if (Offer.BillAmount > totalamount)
                                {
                                    totalamount = 0;
                                }
                            }

                            if (Offer.BillDiscountOfferOn == "Percentage")
                            {
                                BillDiscount.BillDiscountTypeValue = Offer.DiscountPercentage;
                                BillDiscount.BillDiscountAmount = totalamount * Offer.DiscountPercentage / 100;
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
                                    BillDiscount.BillDiscountAmount = totalamount == 0 ? 0 : Convert.ToDouble(WalletPoint / 10);
                                    BillDiscount.IsUsedNextOrder = false;
                                }
                            }
                            if (Offer.MaxDiscount > 0)
                            {
                                var walletmultipler = 1;

                                if (!string.IsNullOrEmpty(Offer.BillDiscountOfferOn) && Offer.BillDiscountOfferOn != "Percentage")
                                {
                                    walletmultipler = 10;
                                }
                                if (Offer.MaxDiscount < BillDiscount.BillDiscountAmount)
                                {
                                    BillDiscount.BillDiscountAmount = Offer.MaxDiscount * walletmultipler;
                                }
                                if (Offer.MaxDiscount < BillDiscount.BillDiscountTypeValue)
                                {
                                    BillDiscount.BillDiscountTypeValue = Convert.ToInt32(Offer.MaxDiscount * walletmultipler);
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
                    }

                }
                if (placeOrderResponse.cart.itemDetails.Any(x => !x.IsSuccess))
                {
                    placeOrderResponse.IsSuccess = false;
                    placeOrderResponse.Message = string.Join(", ", placeOrderResponse.cart.itemDetails.Where(x => !x.IsSuccess).Select(x => x.Message));
                }
                return placeOrderResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private OrderMaster PrepareOrderMasterToInsertHDFC(Warehouse warehouse, ShoppingCart sc, Customer cust, AuthContext db)
        {
            try
            {
                OrderMaster objOrderMaster = new OrderMaster();
                objOrderMaster.CompanyId = warehouse.CompanyId;
                objOrderMaster.WarehouseId = warehouse.WarehouseId;
                objOrderMaster.WarehouseName = warehouse.WarehouseName;
                objOrderMaster.CustomerCategoryId = 2;
                objOrderMaster.Status = "Payment Pending";
                objOrderMaster.CustomerName = cust.Name;
                objOrderMaster.ShopName = cust.ShopName;
                objOrderMaster.LandMark = cust.LandMark;
                objOrderMaster.Skcode = cust.Skcode;
                objOrderMaster.Tin_No = cust.RefNo;
                objOrderMaster.CustomerType = cust.CustomerType;
                objOrderMaster.CustomerId = cust.CustomerId;
                objOrderMaster.CityId = cust.Cityid;
                objOrderMaster.Customerphonenum = (sc.Customerphonenum);
                // MRP-Actual Price
                objOrderMaster.Savingamount = System.Math.Round(sc.Savingamount, 2);
                objOrderMaster.ClusterId = Convert.ToInt32(cust.ClusterId);
                objOrderMaster.OnlineServiceTax = sc.OnlineServiceTax;
                var clstr = db.Clusters.Where(x => x.ClusterId == cust.ClusterId).SingleOrDefault();
                if (clstr != null)
                {
                    objOrderMaster.ClusterName = clstr.ClusterName;
                }
                People p = new People();

                //if (p != null && sc.SalesPersonId != null && sc.SalesPersonId != 0)
                //{

                //    People POrderPunchName = db.Peoples.Where(x => x.PeopleID == sc.SalesPersonId && x.Deleted == false && x.Active == true).SingleOrDefault(); // Add by Y

                //    objOrderMaster.OrderTakenSalesPersonId = POrderPunchName.PeopleID; // change by Y
                //    objOrderMaster.OrderTakenSalesPerson = POrderPunchName.PeopleFirstName + " " + POrderPunchName.PeopleLastName; // Change by Y
                //    //objOrderMaster.SalesMobile = p.Mobile;
                //    //objOrderMaster.SalesPersonId = p.PeopleID;
                //    //objOrderMaster.SalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;


                //}
                //else
                //{

                //    if (p != null)
                //    {
                //        //objOrderMaster.SalesMobile = p.Mobile;
                //        //objOrderMaster.SalesPersonId = p.PeopleID;
                //        //objOrderMaster.SalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                //        objOrderMaster.OrderTakenSalesPersonId = 0;
                //        objOrderMaster.OrderTakenSalesPerson = "Self";
                //    }

                //    else
                //    {
                //        objOrderMaster.OrderTakenSalesPersonId = 0;
                //        objOrderMaster.OrderTakenSalesPerson = "Self";
                //    }

                //}
                if (sc.SalesPersonId == 0)
                {
                    objOrderMaster.OrderTakenSalesPersonId = 0;
                    objOrderMaster.OrderTakenSalesPerson = "Self";
                }
                else
                {
                    p = db.Peoples.Where(x => x.PeopleID == sc.SalesPersonId && x.Deleted == false && x.Active == true).SingleOrDefault();
                    objOrderMaster.OrderTakenSalesPersonId = p.PeopleID; // change by Y
                    objOrderMaster.OrderTakenSalesPerson = p.PeopleFirstName + " " + p.PeopleLastName;
                }


                objOrderMaster.BillingAddress = sc.ShippingAddress;
                objOrderMaster.ShippingAddress = sc.ShippingAddress;
                objOrderMaster.active = true;
                objOrderMaster.CreatedDate = indianTime;

                if (indianTime.Hour > 16)
                {
                    objOrderMaster.Deliverydate = indianTime.AddDays(2);
                }
                else
                {
                    objOrderMaster.Deliverydate = indianTime.AddDays(1);
                }


                objOrderMaster.UpdatedDate = indianTime;
                objOrderMaster.Deleted = false;
                return objOrderMaster;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        #region SendMailCreditWalletNotification
        //SendMailCreditWalletNotification
        public static void SendMailCreditWalletNotification(double? CreditAmount, string PeopleName, string Name, string Skcode, DateTime indianTime, string Through)
        {
            try
            {

                string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
                string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];

                string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                body += "<h3 style='background-color: rgb(241, 89, 34);'>Alert! added more than 5000 point</h3>";
                body += "Hello,";
                body += "<p><strong>";
                body += CreditAmount + "</strong>" + " Wallet Point added. Through(Reason -> )" + Through + "</p>";
                body += "<p>Customer Name : <strong>" + Name + " </strong> " + Skcode + "<br/>from the user <strong>" + PeopleName + " </strong> Date <strong>" + indianTime + "</strong></p>";
                body += "Thanks,";
                body += "<br />";
                body += "<b>IT Team</b>";
                body += "</div>";

                var Subj = "Alert! added more than 5000 point to" + Name + "  " + Skcode;
                var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, body);
                //msg.To.Add("deepak@shopkirana.com");
                msg.To.Add("manasi@shopkirana.com");
                msg.IsBodyHtml = true;
                var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                smtpClient.EnableSsl = true;
                smtpClient.Send(msg);

            }
            catch (Exception ss) { }


        }
        #endregion

        //#region Add Wallet Notification
        //private void ForWalletNotification(int CustomerId, double? CreditAmount,AuthContext db)
        //{
        //    try
        //    {
        //        Notification notification = new Notification();
        //        notification.title = "बधाई हो ! ";
        //        notification.Message = "बधाई हो ! " + CreditAmount + " पॉइंट आपके वॉलेट में ऐड हुआ है ";
        //        notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";
        //        var customers = db.Customers.Where(x => x.fcmId != null && x.CustomerId == CustomerId).SingleOrDefault();
        //        //AddNotification(notification);

        //        string Key = ConfigurationManager.AppSettings["FcmApiKey"];
        //        string id11 = ConfigurationManager.AppSettings["FcmApiId"];

        //        WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
        //        tRequest.Method = "post";
        //        var objNotification = new
        //        {
        //            to = customers.fcmId,
        //            notification = new
        //            {
        //                title = notification.title,
        //                body = notification.Message,
        //                icon = notification.Pic
        //            }
        //        };

        //        string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);
        //        Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
        //        tRequest.Headers.Add(string.Format("Authorization: key={0}", Key));
        //        tRequest.Headers.Add(string.Format("Sender: id={0}", id11));
        //        tRequest.ContentLength = byteArray.Length;
        //        tRequest.ContentType = "application/json";
        //        using (Stream dataStream = tRequest.GetRequestStream())
        //        {
        //            dataStream.Write(byteArray, 0, byteArray.Length);
        //            using (WebResponse tResponse = tRequest.GetResponse())
        //            {
        //                using (Stream dataStreamResponse = tResponse.GetResponseStream())
        //                {
        //                    using (StreamReader tReader = new StreamReader(dataStreamResponse))
        //                    {
        //                        String responseFromFirebaseServer = tReader.ReadToEnd();
        //                        FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
        //                        if (response.success == 1)
        //                        {
        //                            Console.Write(response);
        //                        }
        //                        else if (response.failure == 1)
        //                        {
        //                            Console.Write(response);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error 2001 :Wallet Notification not send due to" + ex.Message);
        //    }
        //}
        //#endregion

        public OrderMaster RewardAndWalletPointForPlacedOrder(ShoppingCart sc, double offerWalletPoint, OrderMaster objOrderMaster, double rewardpoint, Customer cust, double walletUsedPoint1, double rewPoint, double rewAmount, CashConversion cash, AuthContext db)
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

            var rpoint = db.RewardPointDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();
            if (rpoint != null)
            {
                if (rewardpoint > 0)
                {

                    rpoint.EarningPoint += rewardpoint;
                    rpoint.UpdatedDate = indianTime;
                    db.Entry(rpoint).State = EntityState.Modified;
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
                db.RewardPointDb.Add(point);
            }
            #endregion

            Wallet wallet = db.WalletDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();

            if (objOrderMaster.RewardPoint > 0 || walletUsedPoint1 > 0)
            {
                var rpointWarehouse = db.WarehousePointDB.Where(c => c.WarehouseId == objOrderMaster.WarehouseId).SingleOrDefault();
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
                    db.Entry(rpointWarehouse).State = EntityState.Modified;
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
                    db.WarehousePointDB.Add(wPoint);
                    //this.SaveChanges();
                }
            }


            objOrderMaster.GrossAmount = System.Math.Round((objOrderMaster.GrossAmount - rewAmount), 0);
            objOrderMaster.TotalAmount = objOrderMaster.TotalAmount - rewAmount;

            if (sc.walletPointUsed > 0)
            {

                var rpoint1 = db.RewardPointDb.Where(c => c.CustomerId == cust.CustomerId).SingleOrDefault();
                //var WData = WalletDb.Where(x => x.CustomerId == cust.CustomerId).SingleOrDefault();
                if (rpoint1 != null)
                {
                    if (wallet != null && wallet.TotalAmount > 0 && wallet.TotalAmount >= sc.walletPointUsed)
                    {
                        rpoint1.UsedPoint += sc.walletPointUsed;
                        rpoint1.UpdatedDate = indianTime;
                        db.Entry(rpoint1).State = EntityState.Modified;
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
                    db.CustomerWalletHistoryDb.Add(CWH);

                    //update in wallet
                    wallet.TotalAmount -= sc.walletPointUsed;
                    wallet.TransactionDate = indianTime;
                    db.Entry(wallet).State = EntityState.Modified;

                }
            }

            return objOrderMaster;
        }

        #region Sales Return

        [HttpGet]
        [Route("GetBrandList")]
        [AllowAnonymous]
        public async Task<APIResponse> GetBrandList()
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            using (AuthContext context = new AuthContext())
            {
                var data = context.SubsubCategorys.Where(x => x.IsActive == true && x.Deleted == false).Select(x => new { x.SubsubCategoryid, x.SubsubcategoryName }).ToList();
                if (data != null)
                    return new APIResponse { Data = data, Status = true };
                else
                    return new APIResponse { Status = false, Message = "Brand Not Found" };
            }
        }

        [HttpGet]
        [Route("GetCategoryList")]
        [AllowAnonymous]
        public async Task<APIResponse> GetCategoryList(int BrandId)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            using (AuthContext context = new AuthContext())
            {
                var brandid = new SqlParameter("@BrandId", BrandId);
                var data = context.Database.SqlQuery<GetCategoryListDc>("GetCategoryListByBrandId @BrandId", brandid).ToList();
                if (data != null)
                    return new APIResponse { Data = data, Status = true };
                else
                    return new APIResponse { Status = false, Message = "Category Not Found" };
            }
        }

        [HttpPost]
        [Route("InsertUpdateSalesReturn")]
        [AllowAnonymous]
        public async Task<APIResponse> InsertUpdateSalesReturn(SalesReturnFilterDC salesReturn)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext context = new AuthContext())
                {
                    if (salesReturn != null && salesReturn.Id > 0)
                    {
                        var data = context.SalesReturnConfigs.Where(x => x.Id == salesReturn.Id).FirstOrDefault();
                        data.QtyPercent = salesReturn.QtyPercent;
                        data.IsPreExpiry = salesReturn.IsPreExpiry;
                        data.IsPostExpiry = salesReturn.IsPostExpiry;
                        data.DayBeforeExpiry = salesReturn.DayBeforeExpiry;
                        data.DayAfterExpiry = salesReturn.DayAfterExpiry;
                        data.DurationOrderDate = salesReturn.DurationOrderDate;
                        data.DurationDeliveryDate = salesReturn.DurationDeliveryDate;
                        data.ModifiedBy = userid;
                        data.ModifiedDate = DateTime.Now;
                        context.Entry(data).State = EntityState.Modified;
                    }
                    else
                    {
                        foreach (var catid in salesReturn.CategoryId)
                        {
                            var SalesReturnData = context.SalesReturnConfigs.Where(x => x.BrandId == salesReturn.BrandId && x.CategoryId == catid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
                            if (SalesReturnData == null)
                            {
                                var Obj = Mapper.Map(salesReturn).ToANew<SalesReturnConfig>();
                                Obj.CategoryId = catid;
                                Obj.IsActive = true;
                                Obj.IsDeleted = false;
                                Obj.CreatedBy = userid;
                                Obj.CreatedDate = DateTime.Now;
                                Obj.ModifiedBy = userid;
                                Obj.ModifiedDate = DateTime.Now;
                                context.SalesReturnConfigs.Add(Obj);
                            }
                            else
                            {
                                SalesReturnData.CategoryId = catid;
                                SalesReturnData.QtyPercent = salesReturn.QtyPercent;
                                SalesReturnData.IsPreExpiry = salesReturn.IsPreExpiry;
                                SalesReturnData.IsPostExpiry = salesReturn.IsPostExpiry;
                                SalesReturnData.DayBeforeExpiry = salesReturn.DayBeforeExpiry;
                                SalesReturnData.DayAfterExpiry = salesReturn.DayAfterExpiry;
                                SalesReturnData.DurationOrderDate = salesReturn.DurationOrderDate;
                                SalesReturnData.DurationDeliveryDate = salesReturn.DurationDeliveryDate;
                                SalesReturnData.ModifiedBy = userid;
                                SalesReturnData.ModifiedDate = DateTime.Now;
                                context.Entry(SalesReturnData).State = EntityState.Modified;
                            }
                        }
                    }

                    if (context.Commit() > 0)
                        return new APIResponse { Status = true, Message = "Data Saved Successfully" };
                    else
                        return new APIResponse { Status = false, Message = "Data Not Saved" };
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }

        [HttpPost]
        [Route("GetSalesReturnList")]
        [AllowAnonymous]
        public async Task<APIResponse> GetSalesReturnLists(SalesFilterDC obj)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            using (AuthContext context = new AuthContext())
            {
                var Skip = new SqlParameter("@Skip", obj.skip);
                var Take = new SqlParameter("@Take", obj.take);
                var brandId = new SqlParameter("@BrandId", obj.BrandId);
                var Cateid = new SqlParameter("@CategoryId", obj.CategoryId);
                var keyvalue = new SqlParameter("@KeyValue", obj.KeyValue);
                var data = context.Database.SqlQuery<GetSalesReturnDC>("GetSalesReturnList @Skip,@Take,@BrandId,@CategoryId,@KeyValue", Skip, Take, brandId, Cateid, keyvalue).ToList();
                if (data != null && data.Count > 0)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false, Message = "Data Not Found!" };
            }
        }

        [HttpGet]
        [Route("GetSalesReturnExport")]
        [AllowAnonymous]
        public async Task<APIResponse> GetSalesReturnExport(string KeyValue)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            using (AuthContext context = new AuthContext())
            {
                var keyvalue = new SqlParameter("@KeyValue", KeyValue ?? (object)DBNull.Value);
                var data = context.Database.SqlQuery<GetSalesReturnExportDC>("GetSalesReturnExportList @KeyValue", keyvalue).ToList();
                if (data != null && data.Count > 0)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false, Message = "Data Not Found!" };
            }
        }

        [HttpGet]
        [Route("DeleteSalesReturnId")]
        [AllowAnonymous]
        public async Task<APIResponse> DeleteReturnId(int id)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var db = new AuthContext())
            {
                var Res = db.SalesReturnConfigs.Where(x => x.Id == id).FirstOrDefault();
                Res.IsActive = false;
                Res.IsDeleted = true;
                Res.ModifiedBy = userid;
                Res.ModifiedDate = DateTime.Now;
                db.Entry(Res).State = EntityState.Modified;
                if (db.Commit() > 0)
                {
                    return new APIResponse { Status = true, Message = "Data Deleted Successfuly!" };
                }
                else
                    return new APIResponse { Status = false, Message = "Data Not Deleted!" };
            }
        }

        [HttpGet]
        [Route("ItemSearch")]
        [AllowAnonymous]
        public async Task<List<ItemListdc>> ItemSearch(int CustomerId, int warehouseid, string KeyValue)
        {
            using (var db = new AuthContext())
            {
                var custid = new SqlParameter("@CustomerId", CustomerId);
                var wid = new SqlParameter("@WarehouseId", warehouseid);
                var keyvalue = new SqlParameter("@KeyValue", KeyValue ?? (object)DBNull.Value);
                var item = await db.Database.SqlQuery<ItemListdc>("Exec SaleReturnAvailableItemList @CustomerId,@WarehouseId,@KeyValue", custid, wid, keyvalue).ToListAsync();
                return item;
            }
        }

        [HttpGet]
        [Route("SaleReturnOrderBatchItem")]
        [AllowAnonymous]
        public async Task<List<RetunOrderBatchItemDc>> SaleReturnOrderBatchItem(int CustomerId, int ItemMultiMrpId)
        {
            using (var db = new AuthContext())
            {
                var custid = new SqlParameter("@CustomerId", CustomerId);
                var multimrpid = new SqlParameter("@ItemMultiMrpId", ItemMultiMrpId);
                var item = await db.Database.SqlQuery<RetunOrderBatchItemDc>("Exec SaleReturnOrderBatchItem @CustomerId,@ItemMultiMrpId", custid, multimrpid).ToListAsync();
                return item;
            }
        }

        [HttpGet]
        [Route("SalesReturnList")]
        [AllowAnonymous]
        public async Task<List<SalesReturnListDC>> SalesReturnList(int CustomerId)
        {
            if (!IsSalesReturn())
            {
                return new List<SalesReturnListDC>();
            }
            using (AuthContext context = new AuthContext())
            {
                /* var data = (from s in context.KKReturnReplaceRequests
                             join d in context.KKReturnReplaceDetails
                             on s.Id equals d.KKRRRequestId
                             join o in context.OrderDispatchedDetailss
                             on d.OrderDetailsId equals o.OrderDetailsId
                             where s.CustomerId == CustomerId
                             select new SalesReturnListDC
                             {
                                 RequestId = s.Id,
                                 OrderId = d.OrderId,
                                 OrderValue = (d.Qty * o.UnitPrice),
                                 OrderDate = s.CreatedDate
                             }).ToList();*/
                var custid = new SqlParameter("@CustomerId", CustomerId);
                var data = context.Database.SqlQuery<SalesReturnListDC>("SalesReturnRequestList @CustomerId", custid).ToList();

                data.ForEach(item =>
                {
                    List<int> statuslist = new List<int>();

                    statuslist = context.KKReturnReplaceDetails.Where(x => x.KKRRRequestId == item.RequestId).Select(x => x.Status).ToList();

                    if (statuslist != null && statuslist.Count > 0 && statuslist.Any(x => x == 7))  // 7 For POC
                    {
                        item.Status = "Completed";
                    }
                    else if (statuslist != null && statuslist.Count > 0 && statuslist.Any(x => x == 6)) // 6 For Delivery Canceled
                    {
                        item.Status = "Picked";
                    }
                    else if (statuslist != null && statuslist.Count > 0 && statuslist.Any(x => x == 3))
                    {
                        item.Status = "Approve";
                    }
                    else if (statuslist != null && statuslist.Count > 0 && statuslist.Any(x => x == 2 || x == 4))
                    {
                        item.Status = "Rejected";
                    }
                    else if (statuslist != null && statuslist.Count > 0 && statuslist.Any(x => x == 5))
                    {
                        item.Status = "Ready To Pick";
                    }
                    else if (statuslist != null && statuslist.Count > 0 && statuslist.Any(x => x == 0))
                    {
                        item.Status = "Pending";
                    }
                    else //Order Canceled
                    {
                        item.Status = "Order Canceled";
                    }
                });
                return data != null ? data.OrderBy(x => x.RequestId).ToList() : new List<SalesReturnListDC>();
            }
        }

        [HttpGet]
        [Route("SalesReturnDetailList")]
        [AllowAnonymous]
        public async Task<List<ReturnDetailListDC>> SalesReturnDetailList(int RequestId)
        {
            if (!IsSalesReturn())
            {
                return new List<ReturnDetailListDC>();
            }
            using (AuthContext context = new AuthContext())
            {
                var data = (from d in context.KKReturnReplaceDetails
                            join o in context.DbOrderDetails
                            on d.OrderDetailsId equals o.OrderDetailsId
                            where d.KKRRRequestId == RequestId
                            select new ReturnDetailListDC
                            {
                                ItemName = o.itemname,
                                Qty = d.Qty,
                                Rate = o.UnitPrice,
                                TotalValue = (d.Qty * o.UnitPrice)
                            }).ToList();
                return data != null ? data : new List<ReturnDetailListDC>();
            }
        }

        #endregion

        #region Retailer api
        //[HttpPost]
        //[Route("PostSalesReturnRequest")]
        //[AllowAnonymous]
        //public async Task<APIResponse> PostSalesReturnRequest(PostKKReturnReplaceRequestDc data)
        //{
        //    if (!IsSalesReturn())
        //    {
        //        return new APIResponse { Status = false, Message = "Failed" };
        //    }
        //    using (AuthContext context = new AuthContext())
        //    {
        //        using (var dbContextTransaction = context.Database.BeginTransaction())
        //        {
        //            List<SalesReturnItemImage> imglist = new List<SalesReturnItemImage>();
        //            bool result = false;
        //            if (data != null)
        //            {
        //                if (data.Details != null && data.Details.Any())
        //                {
        //                    var ReqOrderIds = data.Details.Select(x => x.OrderId).Distinct().ToList();
        //                    var ReqOrderDetailIds = data.Details.Select(x => x.OrderDetailsId).Distinct().ToList();                            
        //                    var existKKReturnReplaceRequests = context.KKReturnReplaceRequests.Where(x => ReqOrderIds.Contains(x.OrderId)).ToList();
        //                    List<int> remainingReqOrderIds = null;

        //                    if(existKKReturnReplaceRequests.Count > 0)
        //                    {
        //                        existKKReturnReplaceRequests.ForEach(ex =>
        //                        {
        //                            var ReqOrderDetailDatas = data.Details.Where(x => x.OrderId == x.OrderId).ToList();
        //                        });
        //                        var existKKReturnReplaceDetailData = context.KKReturnReplaceDetails.Where(x => ReqOrderDetailIds.Contains(x.OrderDetailsId) && x.Status == 0).ToList();
        //                        //var remainingReturnDetailOrderDetailIds = ReqOrderDetailIds.Where(a=>a != )
        //                        existKKReturnReplaceDetailData.ForEach(x =>
        //                        {   
        //                            var updateExistReturnData = data.Details.Where(y => y.OrderId == x.OrderId).Distinct().ToList();
        //                            updateExistReturnData.ForEach(c =>
        //                            {
        //                                remainingReqOrderIds = ReqOrderIds.Where(r => r != x.OrderId).ToList();
        //                                x.Qty = c.Qty;
        //                                x.BatchCode = c.BatchCode;
        //                                x.BatchId = c.BatchMasterId;
        //                                context.Entry(x).State = EntityState.Modified;
        //                            });
        //                        });
        //                    }
        //                    foreach (var orderid in remainingReqOrderIds)
        //                    {
        //                        KKReturnReplaceRequest KKReturnReplaceRequest = new KKReturnReplaceRequest();
        //                        KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
        //                        KKReturnReplaceRequest.CustomerId = data.CustomerId;
        //                        KKReturnReplaceRequest.OrderId = orderid;
        //                        KKReturnReplaceRequest.RequestType = 0;
        //                        KKReturnReplaceRequest.Status = "Pending";
        //                        //KKReturnReplaceRequest.Cust_Comment = data.Cust_Comment;
        //                        //KKReturnReplaceRequest.DBoyId = people.PeopleID;
        //                        KKReturnReplaceRequest.CreatedBy = data.CustomerId;
        //                        KKReturnReplaceRequest.CreatedDate = indianTime;
        //                        KKReturnReplaceRequest.ModifiedDate = indianTime;
        //                        KKReturnReplaceRequest.IsActive = true;
        //                        KKReturnReplaceRequest.IsDeleted = false;
        //                        context.KKReturnReplaceRequests.Add(KKReturnReplaceRequest);
        //                        context.Commit();
        //                        KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(KKReturnReplaceRequest.Id);
        //                        KKRequestReplaceHistory.Status = "Pending";
        //                        KKRequestReplaceHistory.CreatedBy = data.CustomerId;
        //                        KKRequestReplaceHistory.CreatedDate = indianTime;
        //                        KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
        //                        var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);
        //                        if (addrequest)
        //                        {
        //                            var OrderDetailList = data.Details.Where(x => x.OrderId == orderid).ToList();
        //                            foreach (var item in OrderDetailList)
        //                            {
        //                                KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
        //                                KKReturnReplaceDetail.KKRRRequestId = KKReturnReplaceRequest.Id;
        //                                KKReturnReplaceDetail.OrderDetailsId = item.OrderDetailsId;
        //                                KKReturnReplaceDetail.OrderId = item.OrderId;
        //                                KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
        //                                KKReturnReplaceDetail.Qty = item.ReturnQty;///QTY IN TABLE ME RETURN
        //                                KKReturnReplaceDetail.ReturnReason = item.ReturnReason;
        //                                KKReturnReplaceDetail.BatchCode = item.BatchCode;
        //                                KKReturnReplaceDetail.BatchId = item.BatchMasterId;
        //                                context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);

        //                                context.Commit();

        //                                foreach (var itemImage in item.ItemImages)
        //                                {
        //                                    SalesReturnItemImage img = new SalesReturnItemImage();
        //                                    img.KKRRDetailId = KKReturnReplaceDetail.Id;
        //                                    img.OrderDetailsId = item.OrderDetailsId;
        //                                    img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
        //                                    imglist.Add(img);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    if (imglist != null && imglist.Any())
        //                    {
        //                        context.SalesReturnItemImages.AddRange(imglist);
        //                        dbContextTransaction.Commit();
        //                        result = true;
        //                    }

        //                    if (!result)
        //                    {
        //                        dbContextTransaction.Rollback();
        //                    }
        //                }
        //                return new APIResponse { Status = true, Message = "Request Generated" };
        //            }
        //            else
        //                return new APIResponse { Status = false, Message = "Somthing went wrong" };
        //        }
        //    }
        //}

        [HttpPost]
        [Route("PostSalesReturnRequestTest")]
        [AllowAnonymous]
        public async Task<APIResponse> PostSalesReturnRequest(PostKKReturnReplaceRequestDc data)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            using (AuthContext context = new AuthContext())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    List<SalesReturnItemImage> imglist = new List<SalesReturnItemImage>();
                    bool result = false;
                    if (data != null)
                    {
                        if (data.Details != null && data.Details.Any())
                        {
                            //var ReqOrderIds = data.Details.Select(x => x.OrderId).Distinct().ToList();
                            var ReqDetailDataList = data.Details.ToList();
                            var returnOrderIds = ReqDetailDataList.Select(x => x.OrderId).Distinct().ToList();

                            var OrderDetailIds = ReqDetailDataList.Select(x => x.OrderDetailsId).ToList();
                            var itemNumbers = context.DbOrderDetails.Where(x => OrderDetailIds.Contains(x.OrderDetailsId)).Select(x => x.itemNumber).ToList();
                            var batchids = context.BatchMasters.Where(x => itemNumbers.Contains(x.ItemNumber)).ToList();

                            var existRequest = context.KKReturnReplaceRequests.Where(x => returnOrderIds.Contains(x.OrderId)).ToList();
                            var existOrderIds = existRequest.Select(x => x.OrderId).ToList();
                            var remainingRequestOrderIds = returnOrderIds.Where(x => !existOrderIds.Contains(x));
                            if (existRequest.Count > 0)
                            {
                                existRequest.ForEach(reqOrderId =>
                                {
                                    var ReqOrderDetailIds = data.Details.Select(x => x.OrderDetailsId).Distinct().ToList();
                                    var existKKReturnReplaceDetailData = context.KKReturnReplaceDetails.Where(x => ReqOrderDetailIds.Contains(x.OrderDetailsId) && x.Status == 0 && x.KKRRRequestId == reqOrderId.Id).Distinct().ToList();

                                    if (existKKReturnReplaceDetailData.Count > 0)
                                    {
                                        existKKReturnReplaceDetailData.ForEach(x =>
                                        {
                                            ReqDetailDataList.ForEach(r =>
                                            {
                                                if (r.ItemImages.Count > 0)
                                                {
                                                    if (x.OrderDetailsId == r.OrderDetailsId)
                                                    {
                                                        if (x.BatchCode == r.BatchCode)
                                                        {
                                                            x.Qty = x.Qty + r.Qty;
                                                            context.Entry(x).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                                            KKReturnReplaceDetail.KKRRRequestId = reqOrderId.Id;
                                                            KKReturnReplaceDetail.OrderDetailsId = r.OrderDetailsId;
                                                            KKReturnReplaceDetail.OrderId = r.OrderId;
                                                            KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
                                                            KKReturnReplaceDetail.Qty = r.ReturnQty;///QTY IN TABLE ME RETURN
                                                            KKReturnReplaceDetail.ReturnReason = r.ReturnReason;
                                                            KKReturnReplaceDetail.BatchCode = r.BatchCode;
                                                            KKReturnReplaceDetail.BatchId = batchids.FirstOrDefault(y => y.BatchCode == r.BatchCode).Id;  //r.BatchMasterId;
                                                            context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);
                                                        }
                                                        r.ItemImages.ForEach(img =>
                                                        {
                                                            SalesReturnItemImage imgs = new SalesReturnItemImage();
                                                            imgs.KKRRDetailId = x.Id;
                                                            imgs.OrderDetailsId = x.OrderDetailsId;
                                                            imgs.ItemReturnImage = "/UploadSalesReturnImage/" + img;
                                                            imglist.Add(imgs);
                                                        });
                                                    }
                                                    else
                                                    {
                                                        KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                                        KKReturnReplaceDetail.KKRRRequestId = reqOrderId.Id;
                                                        KKReturnReplaceDetail.OrderDetailsId = r.OrderDetailsId;
                                                        KKReturnReplaceDetail.OrderId = r.OrderId;
                                                        KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
                                                        KKReturnReplaceDetail.Qty = r.ReturnQty;///QTY IN TABLE ME RETURN
                                                        KKReturnReplaceDetail.ReturnReason = r.ReturnReason;
                                                        KKReturnReplaceDetail.BatchCode = r.BatchCode;
                                                        KKReturnReplaceDetail.BatchId = batchids.FirstOrDefault(y => y.BatchCode == r.BatchCode).Id; //r.BatchMasterId;
                                                        context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);
                                                        //foreach (var itemImage in ReqDetailDataList)
                                                        //{
                                                        r.ItemImages.ForEach(itemImage =>
                                                            {
                                                                SalesReturnItemImage img = new SalesReturnItemImage();
                                                                img.KKRRDetailId = KKReturnReplaceDetail.Id;
                                                                img.OrderDetailsId = r.OrderDetailsId;
                                                                img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
                                                                imglist.Add(img);
                                                            });
                                                    }
                                                    context.Commit();
                                                }
                                                else
                                                {
                                                    result = false;
                                                }

                                            });
                                        });

                                    }
                                    else
                                    {
                                        ReqDetailDataList.ForEach(selectedData =>
                                       {

                                           KKReturnReplaceRequest KKReturnReplaceRequest = new KKReturnReplaceRequest();
                                           KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                                           KKReturnReplaceRequest.CustomerId = data.CustomerId;
                                           KKReturnReplaceRequest.OrderId = selectedData.OrderId;
                                           KKReturnReplaceRequest.RequestType = 0;
                                           KKReturnReplaceRequest.Status = "Pending";
                                           KKReturnReplaceRequest.CreatedBy = data.CustomerId;
                                           KKReturnReplaceRequest.CreatedDate = indianTime;
                                           KKReturnReplaceRequest.ModifiedDate = indianTime;
                                           KKReturnReplaceRequest.IsActive = true;
                                           KKReturnReplaceRequest.IsDeleted = false;
                                           context.KKReturnReplaceRequests.Add(KKReturnReplaceRequest);
                                           context.Commit();
                                           KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(KKReturnReplaceRequest.Id);
                                           KKRequestReplaceHistory.Status = "Pending";
                                           KKRequestReplaceHistory.CreatedBy = data.CustomerId;
                                           KKRequestReplaceHistory.CreatedDate = indianTime;
                                           KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                                           var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);
                                           if (addrequest)
                                           {
                                               KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                               KKReturnReplaceDetail.KKRRRequestId = KKReturnReplaceRequest.Id;
                                               KKReturnReplaceDetail.OrderDetailsId = selectedData.OrderDetailsId;
                                               KKReturnReplaceDetail.OrderId = selectedData.OrderId;
                                               KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
                                               KKReturnReplaceDetail.Qty = selectedData.ReturnQty;///QTY IN TABLE ME RETURN
                                               KKReturnReplaceDetail.ReturnReason = selectedData.ReturnReason;
                                               KKReturnReplaceDetail.BatchCode = selectedData.BatchCode;
                                               KKReturnReplaceDetail.BatchId = batchids.FirstOrDefault(y => y.BatchCode == selectedData.BatchCode).Id; //selectedData.BatchMasterId;
                                               context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);

                                               context.Commit();

                                               foreach (var itemImage in selectedData.ItemImages)
                                               {
                                                   SalesReturnItemImage img = new SalesReturnItemImage();
                                                   img.KKRRDetailId = KKReturnReplaceDetail.Id;
                                                   img.OrderDetailsId = selectedData.OrderDetailsId;
                                                   img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
                                                   imglist.Add(img);
                                               }
                                           }

                                       });

                                        //KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                                        //KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(reqOrderId.Id);
                                        //KKRequestReplaceHistory.Status = "Pending";
                                        //KKRequestReplaceHistory.CreatedBy = data.CustomerId;
                                        //KKRequestReplaceHistory.CreatedDate = indianTime;
                                        //KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                                        //var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);
                                        //if (addrequest)
                                        //{
                                        //    foreach (var item in data.Details)
                                        //    {
                                        //        KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                        //        KKReturnReplaceDetail.KKRRRequestId = reqOrderId.Id;
                                        //        KKReturnReplaceDetail.OrderDetailsId = item.OrderDetailsId;
                                        //        KKReturnReplaceDetail.OrderId = item.OrderId;
                                        //        KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
                                        //        KKReturnReplaceDetail.Qty = item.ReturnQty;///QTY IN TABLE ME RETURN
                                        //        KKReturnReplaceDetail.ReturnReason = item.ReturnReason;
                                        //        KKReturnReplaceDetail.BatchCode = item.BatchCode;
                                        //        KKReturnReplaceDetail.BatchId = item.BatchMasterId;
                                        //        context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);

                                        //        context.Commit();

                                        //        foreach (var itemImage in item.ItemImages)
                                        //        {
                                        //            SalesReturnItemImage img = new SalesReturnItemImage();
                                        //            img.KKRRDetailId = KKReturnReplaceDetail.Id;
                                        //            img.OrderDetailsId = item.OrderDetailsId;
                                        //            img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
                                        //            imglist.Add(img);
                                        //        }
                                        //    }
                                        //}
                                    }
                                });
                            }
                            else if ((existRequest.Count > 0 && returnOrderIds.Count != existRequest.Count) || existRequest.Count == 0)
                            {
                                var remainingData = ReqDetailDataList.Where(x => remainingRequestOrderIds.Contains(x.OrderId)).ToList();

                                remainingData.ForEach(selectedData =>
                                {

                                    KKReturnReplaceRequest KKReturnReplaceRequest = new KKReturnReplaceRequest();
                                    KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                                    KKReturnReplaceRequest.CustomerId = data.CustomerId;
                                    KKReturnReplaceRequest.OrderId = selectedData.OrderId;
                                    KKReturnReplaceRequest.RequestType = 0;
                                    KKReturnReplaceRequest.Status = "Pending";
                                    //KKReturnReplaceRequest.Cust_Comment = data.Cust_Comment;
                                    //KKReturnReplaceRequest.DBoyId = people.PeopleID;
                                    KKReturnReplaceRequest.CreatedBy = data.CustomerId;
                                    KKReturnReplaceRequest.CreatedDate = indianTime;
                                    KKReturnReplaceRequest.ModifiedDate = indianTime;
                                    KKReturnReplaceRequest.IsActive = true;
                                    KKReturnReplaceRequest.IsDeleted = false;
                                    context.KKReturnReplaceRequests.Add(KKReturnReplaceRequest);
                                    context.Commit();
                                    KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(KKReturnReplaceRequest.Id);
                                    KKRequestReplaceHistory.Status = "Pending";
                                    KKRequestReplaceHistory.CreatedBy = data.CustomerId;
                                    KKRequestReplaceHistory.CreatedDate = indianTime;
                                    KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                                    var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);
                                    if (addrequest)
                                    {
                                        //var OrderDetailList = data.Details.Where(x => x.OrderId == data.OrderId).ToList();
                                        //foreach (var item in data.Details)
                                        //{
                                        KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                        KKReturnReplaceDetail.KKRRRequestId = KKReturnReplaceRequest.Id;
                                        KKReturnReplaceDetail.OrderDetailsId = selectedData.OrderDetailsId;
                                        KKReturnReplaceDetail.OrderId = selectedData.OrderId;
                                        KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
                                        KKReturnReplaceDetail.Qty = selectedData.ReturnQty;///QTY IN TABLE ME RETURN
                                        KKReturnReplaceDetail.ReturnReason = selectedData.ReturnReason;
                                        KKReturnReplaceDetail.BatchCode = selectedData.BatchCode;
                                        KKReturnReplaceDetail.BatchId = batchids.FirstOrDefault(y => y.BatchCode == selectedData.BatchCode).Id; //selectedData.BatchMasterId;
                                        context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);

                                        context.Commit();

                                        foreach (var itemImage in selectedData.ItemImages)
                                        {
                                            SalesReturnItemImage img = new SalesReturnItemImage();
                                            img.KKRRDetailId = KKReturnReplaceDetail.Id;
                                            img.OrderDetailsId = selectedData.OrderDetailsId;
                                            img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
                                            imglist.Add(img);
                                        }
                                        //}
                                    }

                                });

                            }
                            if (imglist != null && imglist.Any())
                            {
                                context.SalesReturnItemImages.AddRange(imglist);
                                context.Commit();
                                dbContextTransaction.Commit();
                                result = true;
                            }

                            if (!result)
                            {
                                dbContextTransaction.Rollback();
                            }
                        }
                        return new APIResponse { Status = true, Message = "Request Generated" };
                    }
                    else
                        return new APIResponse { Status = false, Message = "Somthing went wrong" };
                }
            }
        }
        #endregion

        #region Dashboard Details

        [HttpPost]
        [Route("ApproveOrReject")]
        [AllowAnonymous]
        public async Task<APIResponse> ApproveOrReject(ApproveRejectDC data)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            string Message = "";
            using (AuthContext context = new AuthContext())
            {
                var obj = context.KKReturnReplaceDetails.Where(x => x.Id == data.KKReturnDetailId).FirstOrDefault();
                if (obj != null)
                {
                    if (data.IsWarehoues)
                    {
                        obj.Status = data.Status ? 1 : 2;
                        obj.Warehouse_Comment = data.Comment;
                        Message = data.Status ? "Warehouse Approved Successfully" : "Warehouse Rejected Successfully";
                    }
                    else
                    {
                        obj.Status = data.Status ? 3 : 4;
                        obj.Buyer_Comment = data.Comment;
                        Message = data.Status ? "Buyer Approved Successfully" : "Buyer Rejected Successfully";
                    }
                    context.Entry(obj).State = EntityState.Modified;

                    var kk = context.KKRequestReplaceHistorys.Where(x => x.Status == "Pending" && x.KKRequestId == obj.KKRRRequestId).FirstOrDefault();

                    KKRequestReplaceHistory kKRequestReplaceHistory = new KKRequestReplaceHistory();
                    kKRequestReplaceHistory.KKRequestDetailId = obj.Id;
                    kKRequestReplaceHistory.KKRequestId = (int)obj.KKRRRequestId;
                    kKRequestReplaceHistory.Status = Enum.GetName(typeof(StatusType), obj.Status);
                    kKRequestReplaceHistory.CreatedBy = userid;
                    kKRequestReplaceHistory.CreatedDate = DateTime.Now;
                    kKRequestReplaceHistory.ExecutiveId = kk.ExecutiveId;
                    kKRequestReplaceHistory.CustomerId = kk.CustomerId;

                    context.KKRequestReplaceHistorys.Add(kKRequestReplaceHistory);
                    if (context.Commit() > 0)
                        return new APIResponse { Status = true, Message = Message };
                    else
                        return new APIResponse { Status = false, Message = "Something Went Wrong" };
                }
                else
                {
                    return new APIResponse { Status = false, Message = "Something Went Wrong" };
                }
            }
        }

        [HttpPost]
        [Route("MultiApproveOrReject")]
        [AllowAnonymous]
        public async Task<APIResponse> MultiApproveOrReject(List<MultiReturnOrderApproveRejectDC> data)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            List<string> roleNames = new List<string>();
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames") && identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value != "")
                roleNames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').ToList();

            int roleId = 0;

            if (roleId == 0)
            {
                if (roleNames.Any(x => x == "Inbound Lead"))
                    roleId = 1;
                else if (roleNames.Any(x => x == "Buyer"))
                    roleId = 3;
            }
            string Message = "";
            using (AuthContext context = new AuthContext())
            {
                var KKReturnDetailIds = data.Select(x => x.KKReturnDetailId).ToList();
                List<KKReturnReplaceDetail> kKReturnReplaceDetails = context.KKReturnReplaceDetails.Where(x => KKReturnDetailIds.Contains(x.Id)).ToList();
                if (kKReturnReplaceDetails.Count > 0)
                {
                    kKReturnReplaceDetails.ForEach(x =>
                    {
                        if (roleId == 1)
                        {
                            x.Status = data.Where(dt => dt.KKReturnDetailId == x.Id).FirstOrDefault().Status ? 1 : 2;
                            x.Warehouse_Comment = data.Where(dt => dt.KKReturnDetailId == x.Id).FirstOrDefault().Comment;
                            Message = data.Where(dt => dt.KKReturnDetailId == x.Id).FirstOrDefault().Status ? "Warehouse Approved Successfully" : "Warehouse Rejected Successfully";
                        }
                        else if (roleId == 3)
                        {
                            x.Status = data.Where(dt => dt.KKReturnDetailId == x.Id).FirstOrDefault().Status ? 3 : 4;
                            x.Buyer_Comment = data.Where(dt => dt.KKReturnDetailId == x.Id).FirstOrDefault().Comment;
                            Message = data.Where(dt => dt.KKReturnDetailId == x.Id).FirstOrDefault().Status ? "Buyer Approved Successfully" : "Buyer Rejected Successfully";
                        }
                        context.Entry(x).State = EntityState.Modified;
                        var kk = context.KKRequestReplaceHistorys.Where(req => req.Status == "Pending" && req.KKRequestId == x.KKRRRequestId).FirstOrDefault();

                        KKRequestReplaceHistory kKRequestReplaceHistory = new KKRequestReplaceHistory();
                        kKRequestReplaceHistory.KKRequestDetailId = x.Id;
                        kKRequestReplaceHistory.KKRequestId = (int)x.KKRRRequestId;
                        kKRequestReplaceHistory.Status = Enum.GetName(typeof(StatusType), x.Status);
                        kKRequestReplaceHistory.CreatedBy = userid;
                        kKRequestReplaceHistory.CreatedDate = DateTime.Now;
                        kKRequestReplaceHistory.ExecutiveId = kk.ExecutiveId;
                        kKRequestReplaceHistory.CustomerId = kk.CustomerId;

                        context.KKRequestReplaceHistorys.Add(kKRequestReplaceHistory);
                    });

                    if (context.Commit() > 0)
                        return new APIResponse { Status = true, Message = Message };
                    else
                        return new APIResponse { Status = false, Message = "Something Went Wrong" };
                }
                else
                {
                    return new APIResponse { Status = false, Message = "Something Went Wrong" };
                }
            }
        }


        [HttpGet]
        [Route("BuyerApprovedCustList")]
        [AllowAnonymous]
        public async Task<APIResponse> BuyerApprovedCustList()
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            using (AuthContext context = new AuthContext())
            {
                var data = (from k in context.KKReturnReplaceRequests
                            join d in context.KKReturnReplaceDetails
                            on k.Id equals d.KKRRRequestId
                            join c in context.Customers
                            on k.CustomerId equals c.CustomerId
                            where d.Status == 3 //Buyer Approved
                            select new
                            {
                                Skcode = c.Name + " (" + c.Skcode + ")",
                                CustomerId = c.CustomerId
                            }).Distinct().ToList();
                if (data != null)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false };
            }
        }

        [HttpPost]
        [Route("CreateReturnOrder")]
        [AllowAnonymous]
        public async Task<APIResponse> CreateDispatchOrder(SalesReturnDetailDC salesReturn)
        {
            //if (!IsSalesReturn())
            //{
            //    return new APIResponse { Status = false, Message = "Failed" };
            //}
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            Customer custdata = new Customer();
            Warehouse wh = new Warehouse();
            SalesReturnOrderHelper salesReturnOrderHelper = new SalesReturnOrderHelper();
            OrderDispatchedMaster OrderDispatchedMaster = new OrderDispatchedMaster();
            List<KKReturnReplaceDetail> kKReturnReplaceDetail = new List<KKReturnReplaceDetail>();
            List<OrderDispatchedDetails> orderDispatchedDetails = new List<OrderDispatchedDetails>();
            List<OrderDetails> orderDetails = new List<OrderDetails>();
            OrderMaster orderMaster = new OrderMaster();
            bool IsCommit = false;
            double totalAmt = 0;
            List<long> KKRequestIds = new List<long>();
            using (AuthContext context = new AuthContext())
            {
                ReturnDetailDC data = new ReturnDetailDC();
                custdata = context.Customers.Where(x => x.CustomerId == salesReturn.CustomerId).FirstOrDefault();
                wh = context.Warehouses.Where(x => x.WarehouseId == custdata.Warehouseid).FirstOrDefault();

                var returnDetailList = (from k in context.KKReturnReplaceRequests
                                        join d in context.KKReturnReplaceDetails
                                        on k.Id equals d.KKRRRequestId
                                        //join od in context.OrderDispatchedDetailss
                                        //on d.OrderDetailsId equals od.OrderDetailsId
                                        where k.Id == d.KKRRRequestId && k.CustomerId == salesReturn.CustomerId
                                        && k.OrderId == salesReturn.OrderId && d.Status == 3 //Buyer Approved
                                        select d).Distinct().ToList();
                if (returnDetailList.Count > 0)
                {
                    var OrderDetailIds = returnDetailList.Select(x => x.OrderDetailsId).ToList();
                    var ParentDispatchData = context.OrderDispatchedMasters.Where(x => x.OrderId == salesReturn.OrderId).FirstOrDefault();
                    var dispatchDetailList = context.OrderDispatchedDetailss.Where(x => OrderDetailIds.Contains(x.OrderDetailsId)).ToList();
                    var OrderdetailData = context.DbOrderDetails.Where(x => OrderDetailIds.Contains(x.OrderDetailsId)).Select(x => new { x.OrderDetailsId, x.StoreId,x.OfferId }).ToList();
                    DataTable dt = new DataTable();
                    //List<int?> orderids = new List<int?>();

                    if (returnDetailList != null && returnDetailList.Count > 0)
                    {
                        var OrderIdList = returnDetailList.Select(x => x.OrderId).Distinct().ToList();
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
                    cmd.CommandText = "[dbo].[BillDiscountCalculationForSalesReturnOrder]";
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
                            item.FinalItemAmount = (item.qty * item.UnitPrice) - (item.UnitBillDisAmt* item.qty) - (item.UnitWalletDisAmt * item.qty);
                        }
                    };

                    data.ReturnDetails = returnDetailList;
                    data.OrderDetails = dispatchDetailList;
                    orderDispatchedDetails = dispatchDetailList;
                    kKReturnReplaceDetail = returnDetailList;
                    //OrderDispatchedMaster = context.OrderDispatchedMasters.Where(X => X.OrderId == salesReturn.OrderId).FirstOrDefault();

                    double WalletAmt = 0;
                    double BillDisAmt = 0;
                    int noOfReturnQty = 0;
                    returnDetailList.ForEach(x =>
                    {
                        dispatchDetailList.ForEach(d =>
                        {
                            if (x.OrderDetailsId == d.OrderDetailsId)
                            {
                                totalAmt += (x.Qty * d.UnitPrice) - (d.UnitBillDisAmt* x.Qty)- (d.UnitWalletDisAmt * x.Qty) - ParentDispatchData.TCSAmount - ParentDispatchData.deliveryCharge;
                                //noOfReturnQty += x.Qty;
                                WalletAmt += (d.UnitWalletDisAmt * x.Qty);
                                BillDisAmt += (d.UnitBillDisAmt * x.Qty);
                            }
                        });
                    });

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
                    orderMaster.Status = "Ready to Dispatch";
                    orderMaster.CreatedDate = DateTime.Now;
                    orderMaster.Deleted = false;
                    orderMaster.ClusterId = (int)custdata.ClusterId;
                    orderMaster.ClusterName = custdata.ClusterName;
                    orderMaster.OrderType = 3;
                    orderMaster.Lat = custdata.lat;
                    orderMaster.Lng = custdata.lg;
                    orderMaster.Tin_No = custdata.RefNo;
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
                    orderMaster.TCSAmount = ParentDispatchData.TCSAmount;
                    orderMaster.IsFirstOrder = false;
                    orderMaster.ParentOrderId = salesReturn.OrderId;
                    orderMaster.DispatchAmount = Math.Round(totalAmt, 0);
                    orderMaster.WalletAmount = WalletAmt;
                    orderMaster.BillDiscountAmount = BillDisAmt;

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
                        //orderDetail.Itempic= x.ReturnDetails;
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
                        orderDetail.qty = data.ReturnDetails.Where(r => r.OrderDetailsId == x.OrderDetailsId && r.ItemMultiMRPId == x.ItemMultiMRPId).Sum(y => y.Qty);//&& r.ItemMultiMRPId == x.ItemMultiMRPId
                        orderDetail.TotalAmt = (x.UnitPrice * orderDetail.qty);
                        orderDetail.OfferId = OrderdetailData.Where(y => y.OrderDetailsId == x.OrderDetailsId).Select(y => y.OfferId).FirstOrDefault();
                        orderDetail.CreatedDate = DateTime.Now;
                        orderDetail.Deleted = false;
                        orderDetail.OrderDate = DateTime.Now;
                        orderDetail.Status = "Ready to Dispatch";
                        orderDetail.ItemMultiMRPId = x.ItemMultiMRPId;
                        orderDetail.ABCClassification = ""; // x.OrderDetails.ABCClassification;
                        orderDetail.CurrentStock = 0;
                        orderDetail.SupplierName = ""; // x.OrderDetails.SupplierName;
                        orderDetail.ActualUnitPrice = 0; // x.OrderDetails.ActualUnitPrice;
                        orderDetail.StoreId = OrderdetailData.Where(y => y.OrderDetailsId == x.OrderDetailsId).Select(y => y.StoreId).FirstOrDefault();
                        orderDetail.ExecutiveId = 0;// x.OrderDetails.ExecutiveId;
                        orderDetail.ExecutiveName = ""; // x.OrderDetails.ExecutiveName;
                        orderDetail.NetAmmount = (x.UnitPrice * orderDetail.qty);
                        //orderDetail.Status = "Pending";
                        //orderDetail.AmtWithoutAfterTaxDisc = x.AmtWithoutAfterTaxDisc;
                        //orderDetail.AmtWithoutTaxDisc = x.AmtWithoutTaxDisc;
                        //orderDetail.TotalAmountAfterTaxDisc = x.TotalAmountAfterTaxDisc;
                        //orderDetail.DiscountPercentage = x.DiscountPercentage;
                        //orderDetail.DiscountAmmount = x.DiscountAmmount;
                        //orderDetail.NetAmtAfterDis = x.NetAmtAfterDis;
                        //orderDetail.TaxPercentage = x.TaxPercentage;
                        //orderDetail.TaxAmmount = x.TaxAmmount;
                        //orderDetail.SGSTTaxAmmount = x.SGSTTaxAmmount;
                        //orderDetail.SGSTTaxPercentage = x.SGSTTaxPercentage;
                        //orderDetail.CGSTTaxAmmount = x.CGSTTaxAmmount;
                        //orderDetail.CGSTTaxPercentage = x.CGSTTaxPercentage;
                        //orderDetail.NetPurchasePrice = x.Purchaseprice;
                        //orderDetail.TotalCessPercentage = x.TotalCessPercentage;
                        //orderDetail.CessTaxAmount = x.CessTaxAmount;
                        orderDetail.UpdatedDate = DateTime.Now;
                        orderDetail.SizePerUnit = 1;
                        orderDetail.IsFreeItem = x.IsFreeItem;
                        orderDetail.IsDispatchedFreeStock = x.IsDispatchedFreeStock;
                        orderDetail.ActualUnitPrice = 0;
                        orderDetail.Deleted = false;
                        orderDetail.marginPoint = 0;
                        orderDetail.promoPoint = 0;
                        orderDetail.PTR = x.PTR;
                        orderDetail.OldOrderDetailId = x.OrderDetailsId;
                        orderDetails.Add(orderDetail);

                        //x.qty = x.qty - orderDetail.qty;
                        //x.TotalAmt = x.TotalAmt - orderDetail.TotalAmt; //(x.UnitPrice * x.qty);
                        //x.CreatedDate = DateTime.Now;
                        //x.UpdatedDate = DateTime.Now;
                        //x.Noqty = x.qty;

                        //KKRequestId = data.ReturnDetails.Where(r => r.OrderDetailsId == x.OrderDetailsId).Select(y => y.KKRRRequestId).FirstOrDefault();
                        var KKReqIds = data.ReturnDetails.Where(r => r.OrderDetailsId == x.OrderDetailsId).Select(y => y.KKRRRequestId).ToList();
                        if (KKReqIds.Any())
                            KKRequestIds.AddRange(KKReqIds);
                        #region KKOrderDetails Status

                        #endregion

                    }
                    returnDetailList.ForEach(x =>
                    {
                        if (salesReturn.OrderDetailsId.Contains(x.OrderDetailsId))
                        {
                            x.Status = 5;
                        }
                        context.Entry(x).State = EntityState.Modified;
                    });
                    orderMaster.orderDetails = orderDetails;
                    context.DbOrderMaster.Add(orderMaster);



                    bool resStatus = salesReturnOrderHelper.PostOrderStatus(salesReturn.OrderId, "ReturnPending", userid, context);

                    if (context.Commit() > 0)
                        IsCommit = true;
                    else
                        IsCommit = false;
                }
            }
            #endregion
            if (IsCommit)
            {
                using (AuthContext db = new AuthContext())
                {
                    //bar code start
                    var odm = db.DbOrderMaster.Where(x => x.OrderId == orderMaster.OrderId && x.active == true && x.Deleted == false).Include(x => x.orderDetails).FirstOrDefault();
                    if (odm != null)
                    {
                        string Borderid = Convert.ToString(odm.OrderId);
                        string BorderCodeId = Borderid.PadLeft(11, '0');
                        temOrderQBcode code = GetBarcode(BorderCodeId);
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
                    OrderDispatchedMaster.Status = "Ready to Dispatch";
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
                    OrderDispatchedMaster.PocCreditNoteNumber = null;
                    OrderDispatchedMaster.PocCreditNoteDate = null;
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
                    //double finaltotal = 0;
                    //double finalTaxAmount = 0;
                    //double finalSGSTTaxAmount = 0;
                    //double finalCGSTTaxAmount = 0;
                    //double finalGrossAmount = 0;
                    //double finalTotalTaxAmount = 0;
                    //double finalCessTaxAmount = 0;

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

                    //orderDispatchedDetails.ForEach(x =>
                    //{
                    //    x.OrderDetailsId = orderMaster.orderDetails.FirstOrDefault(y => y.ItemId == x.ItemId && y.ItemMultiMRPId == x.ItemMultiMRPId).OrderDetailsId;
                    //    x.OrderId = orderMaster.OrderId;
                    //});

                    //var reqData = db.KKReturnReplaceRequests.Where(x => x.Id == KKRequestId).FirstOrDefault();
                    var reqData = db.KKReturnReplaceRequests.Where(x => KKRequestIds.Contains(x.Id)).ToList();
                    reqData.ForEach(r =>
                    {
                        r.NewGeneratedOrderId = orderMaster.OrderId;
                        r.Status = "Pending";
                        db.Entry(r).State = EntityState.Modified;
                    });
                    //reqData.NewGeneratedOrderId = orderMaster.OrderId;
                    //reqData.Status = "Pending";
                    //db.Entry(reqData).State = EntityState.Modified;
                    db.OrderDispatchedMasters.Add(OrderDispatchedMaster);
                    var KKdetailids = kKReturnReplaceDetail.Select(x => x.Id).Distinct().ToList();
                    var kKReturnDetails = db.KKReturnReplaceDetails.Where(x => KKdetailids.Contains(x.Id)).ToList();
                    foreach (var x in kKReturnDetails)
                    {
                        int orderdetailid = 0;
                        KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                        KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(x.KKRRRequestId);
                        KKRequestReplaceHistory.KKRequestDetailId = Convert.ToInt32(x.Id);
                        KKRequestReplaceHistory.Status = "Return Order Generated";
                        KKRequestReplaceHistory.CreatedBy = userid;
                        KKRequestReplaceHistory.CreatedDate = indianTime;
                        KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                        var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);
                        orderdetailid = orderMaster.orderDetails.FirstOrDefault(y => y.OldOrderDetailId == x.OrderDetailsId).OrderDetailsId;
                        x.NewOrderDetailsId = orderdetailid;
                        db.Entry(x).State = EntityState.Modified;
                    }
                    //OrderDispatchedMaster.orderDetails = Mapper.Map(orderDispatchedDetails).ToANew<List<OrderDispatchedDetails>>();

                    bool resReturnStatus = salesReturnOrderHelper.PostOrderStatus(orderMaster.OrderId, "Return_Ready_to_Dispatch", userid, db);




                    db.Commit();
                }
                return new APIResponse { Status = true, Data = orderMaster.OrderId, Message = "Order Generated Successfully" };
            }
            else
                return new APIResponse { Status = false, Message = "Somthing Went Wrong" };
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
        [HttpGet]
        [Route("GetReturnListByCustId")]
        [AllowAnonymous]
        public async Task<APIResponse> GetReturnListByCustId(int CustomerId)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            using (AuthContext context = new AuthContext())
            {
                var data = (from k in context.KKReturnReplaceRequests
                            join d in context.KKReturnReplaceDetails
                            on k.Id equals d.KKRRRequestId
                            join o in context.DbOrderDetails
                            on d.OrderDetailsId equals o.OrderDetailsId
                            where k.CustomerId == CustomerId && d.Status == 3 //Buyer Approved
                            select new GetReturnReplaceItemDc
                            {
                                KKReturnDetailId = d.Id,
                                OrderId = d.OrderId,
                                OrderDetailsId = d.OrderDetailsId,
                                ItemId = o.ItemId,
                                ItemName = o.itemname,
                                //price = Math.Round((d.Qty * o.UnitPrice), 2),
                                price = Math.Round(o.UnitPrice, 2),
                                Qty = d.Qty,
                                BatchCode = d.BatchCode,
                                Status = d.Status,
                                CreatedDate = k.CreatedDate,
                                KKRequestId = d.KKRRRequestId
                            }).ToList();
                if (data != null && data.Count > 0)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false };
            }
        }
        #endregion

        //[HttpPost]
        //[Route("PickedReturnOrderByDBoy")]
        //public async Task<APIResponse> PickedReturnOrderByDBoy(ItemPickByDBoyDc itemlist)
        //{
        //    if (!IsSalesReturn())
        //    {
        //        return new APIResponse { Status = false, Message = "Failed" };
        //    }
        //    using (AuthContext db = new AuthContext())
        //    {
        //        APIResponse res = new APIResponse();
        //        bool ReturnStatus = false;
        //        var identity = User.Identity as ClaimsIdentity;
        //        int userid = 0;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        if (itemlist.OrderDetaileList.Count > 0)
        //        {
        //            foreach (var item in itemlist.OrderDetaileList)
        //            {
        //                var kkd = db.KKReturnReplaceDetails.Where(x => x.KKRRRequestId == item.KKRRRequestId && x.NewOrderDetailsId == item.NewOrderDetailId && x.BatchId == item.BatchId).FirstOrDefault();
        //                if (kkd.Qty < item.Qty)
        //                {
        //                    res.Message = "Batchcode quantity not Greater than actual quantity for Batch : " + kkd.BatchCode;
        //                    res.Status = false;
        //                    ReturnStatus = true;
        //                    break;
        //                }
        //            }
        //            if (ReturnStatus)
        //            {
        //                return res;
        //            }

        //            var OrderIdList = itemlist.OrderDetaileList.Select(x => x.NewOrderId).Distinct().ToList();
        //            var orderDeliveryOTPData = db.OrderDeliveryOTP.Where(x => OrderIdList.Contains(x.OrderId) && x.OTP == itemlist.OTP && x.IsActive == true && x.IsUsed == false).ToList();
        //            var PeopleData = db.Peoples.Where(x => x.PeopleID == itemlist.PeopleId).FirstOrDefault();

        //            if (orderDeliveryOTPData.Count > 0)
        //            {
        //                var OrderMasterData = db.DbOrderMaster.Where(x => OrderIdList.Contains(x.OrderId)).Include(x => x.orderDetails).ToList();
        //                DataTable dt = new DataTable();
        //                List<int?> orderids = new List<int?>();
        //                List<OrderDispatchedMaster> orderDispatchedMasterNew = new List<OrderDispatchedMaster>();
        //                if (itemlist.OrderDetaileList != null && itemlist.OrderDetaileList.Count > 0)
        //                {
        //                    //var OrderIdList = itemlist.OrderDetaileList.Select(x => x.NewOrderId).Distinct().ToList();

        //                    //orderids = db.DbOrderMaster.Where(x => OrderIdList.Contains(x.OrderId)).Select(x => x.ParentOrderId).ToList();

        //                    orderids = OrderMasterData.Select(x => x.ParentOrderId).Distinct().ToList();
        //                    dt.Columns.Add("IntValue");

        //                    foreach (var id in orderids)
        //                    {
        //                        var dr = dt.NewRow();
        //                        dr["IntValue"] = id;
        //                        dt.Rows.Add(dr);
        //                    }
        //                }
        //                List<OrderBillDiscountDc> list = new List<OrderBillDiscountDc>();

        //                if (db.Database.Connection.State != ConnectionState.Open)
        //                    db.Database.Connection.Open();

        //                var cmd = db.Database.Connection.CreateCommand();
        //                cmd.CommandTimeout = 300;
        //                cmd.CommandText = "[dbo].[BillDiscountCalculationForSalesReturnOrder]";
        //                cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //                var orderid = new SqlParameter("orderid", dt);
        //                cmd.Parameters.Add(orderid);

        //                var reader = cmd.ExecuteReader();

        //                list = ((IObjectContextAdapter)db).ObjectContext.Translate<OrderBillDiscountDc>(reader).ToList();
        //                db.Database.Connection.Close();
        //                var NewOrderIds = itemlist.OrderDetaileList.Select(x => x.NewOrderId).Distinct().ToList();
        //                var NewOrderDetailIds = itemlist.OrderDetaileList.Select(x => x.NewOrderDetailId).Distinct().ToList();
        //                var DeliveryCancelData = db.DeliveryCancelledDraftDB.Where(x => NewOrderIds.Contains(x.OrderId)).ToList();
        //                if (list != null && list.Count > 0)
        //                {
        //                    List<ReturnOrderBillDiscount> returnOrderBillDiscountList = new List<ReturnOrderBillDiscount>();
        //                    List<DeliveryCancelledDraft> deliveryCancelledDraftList = new List<DeliveryCancelledDraft>();
        //                    foreach (var item in itemlist.OrderDetaileList)
        //                    {
        //                        ReturnOrderBillDiscount returnOrderBillDiscount = new ReturnOrderBillDiscount();
        //                        var billdiscount = list.Where(x => x.NewGeneratedOrderId == item.NewOrderId && x.NewOrderDetailsId == item.NewOrderDetailId).FirstOrDefault();
        //                        var finalbilldiscount = billdiscount != null ? item.Qty * billdiscount.UnitbilldiscountAmt : 0;
        //                        returnOrderBillDiscount.OrderId = item.NewOrderId;
        //                        returnOrderBillDiscount.OrderDetailId = item.NewOrderDetailId;
        //                        returnOrderBillDiscount.Qty = item.Qty;
        //                        returnOrderBillDiscount.BillDiscountAmount = finalbilldiscount;
        //                        returnOrderBillDiscount.TotalAmount = (item.Qty * item.unitprice) - finalbilldiscount;
        //                        returnOrderBillDiscount.CreatedBy = userid;
        //                        returnOrderBillDiscount.CreatedDate = DateTime.Now;
        //                        returnOrderBillDiscount.IsActive = true;
        //                        returnOrderBillDiscount.IsDeleted = false;
        //                        returnOrderBillDiscount.WalletAmount = billdiscount != null ? item.Qty * billdiscount.WalletAmount : 0;
        //                        returnOrderBillDiscountList.Add(returnOrderBillDiscount);

        //                        DeliveryCancelledDraft imageUpload = new DeliveryCancelledDraft();
        //                        var ImgRes = DeliveryCancelData.Where(x => x.OrderId == item.NewOrderId).FirstOrDefault();
        //                        if (ImgRes == null)
        //                        {
        //                            imageUpload.OrderId = item.NewOrderId;
        //                            imageUpload.OrderItemImage = item.PickItemImage;
        //                            imageUpload.IsActive = true;
        //                            imageUpload.IsDeleted = false;
        //                            imageUpload.CreatedBy = userid;
        //                            imageUpload.CreatedDate = DateTime.Now;
        //                            deliveryCancelledDraftList.Add(imageUpload);
        //                        }
        //                    }

        //                    orderDispatchedMasterNew = db.OrderDispatchedMasters.Where(x => NewOrderIds.Contains(x.OrderId)).Include(x => x.orderDetails).ToList();
        //                    var KKreturnDetailData = db.KKReturnReplaceDetails.Where(x => NewOrderDetailIds.Contains(x.NewOrderDetailsId) && x.Status == 5).ToList();

        //                    foreach (var oid in NewOrderIds)
        //                    {
        //                        var od = orderDispatchedMasterNew.Where(x => x.OrderId == oid).FirstOrDefault();
        //                        var om = OrderMasterData.Where(x => x.OrderId == oid).FirstOrDefault();
        //                        if (od != null)
        //                        {
        //                            if (od.orderDetails.Where(x => x.qty > 0).Any())
        //                            {
        //                                if (od.PocCreditNoteNumber == null || od.PocCreditNoteNumber == "")
        //                                {
        //                                    var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == od.WarehouseId);

        //                                    string PocinvoiceNumber = "";
        //                                    if (od.WarehouseId != 67 && od.WarehouseId != 80)
        //                                    {
        //                                        PocinvoiceNumber = db.Database.SqlQuery<string>("EXEC spGetPocCNCurrentNumber 'PocCreditNote', " + warehouse.Stateid).FirstOrDefault();
        //                                    }
        //                                    if (PocinvoiceNumber != null)
        //                                    {
        //                                        db.ClearTaxIntegrations.Add(new ClearTaxIntegration
        //                                        {
        //                                            OrderId = Convert.ToInt32(oid),
        //                                            APIType = "GenerateIRN",
        //                                            IsProcessed = false,
        //                                            Error = null,
        //                                            IRNNo = null,
        //                                            EwayBillNo = null,
        //                                            RequestId = 0,
        //                                            ResponseId = 0,
        //                                            IsActive = true,
        //                                            CreateDate = DateTime.Now,
        //                                            ProcessDate = null,
        //                                            IsOnline = false,
        //                                            APITypes = 3
        //                                        });
        //                                    }
        //                                    od.PocCreditNoteNumber = PocinvoiceNumber;
        //                                    od.PocCreditNoteDate = indianTime;
        //                                }

        //                                var BillDisAmt = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.BillDiscountAmount);
        //                                od.BillDiscountAmount = BillDisAmt;
        //                                var WalletDisAmt = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.WalletAmount);
        //                                od.WalletAmount = WalletDisAmt;
        //                                od.TotalAmount = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.TotalAmount);
        //                                od.GrossAmount = Math.Round(returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.TotalAmount), 0);
        //                                od.Status = "Delivery Canceled";

        //                                List<int> itemids = null;
        //                                List<ItemMaster> itemslist = null;

        //                                itemids = od.orderDetails.Select(x => x.ItemId).Distinct().ToList();
        //                                itemslist = db.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();

        //                                foreach (var pc in od.orderDetails)
        //                                {
        //                                    pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);

        //                                    int MOQ = pc.MinOrderQty;
        //                                    pc.MinOrderQtyPrice = MOQ * pc.UnitPrice;
        //                                    pc.TaxPercentage = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).TotalTaxPercentage : 0;
        //                                    pc.TotalCessPercentage = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).TotalCessPercentage : 0;// items.TotalCessPercentage;
        //                                    if (pc.TaxPercentage >= 0)
        //                                    {
        //                                        pc.SGSTTaxPercentage = pc.TaxPercentage / 2;
        //                                        pc.CGSTTaxPercentage = pc.TaxPercentage / 2;
        //                                    }
        //                                    pc.HSNCode = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).HSNCode : null;//items.HSNCode;
        //                                    pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
        //                                    pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
        //                                    //if there is cess for that item

        //                                    if (pc.TotalCessPercentage > 0)
        //                                    {
        //                                        // pc.TotalCessPercentage = odd.TotalCessPercentage;
        //                                        double tempPercentagge = pc.TotalCessPercentage + pc.TaxPercentage;

        //                                        pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge / 100)) / 100;
        //                                        pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
        //                                        pc.CessTaxAmount = (pc.AmtWithoutAfterTaxDisc * pc.TotalCessPercentage) / 100;
        //                                    }
        //                                    double tempPercentagge2 = pc.TotalCessPercentage + pc.TaxPercentage;
        //                                    pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge2 / 100)) / 100;
        //                                    pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + pc.DiscountPercentage);
        //                                    pc.TaxAmmount = (pc.AmtWithoutAfterTaxDisc * pc.TaxPercentage) / 100;
        //                                    if (pc.TaxAmmount >= 0)
        //                                    {
        //                                        pc.SGSTTaxAmmount = pc.TaxAmmount / 2;
        //                                        pc.CGSTTaxAmmount = pc.TaxAmmount / 2;
        //                                    }

        //                                    //for cess
        //                                    if (pc.CessTaxAmount > 0)
        //                                    {
        //                                        double tempPercentagge3 = pc.TotalCessPercentage + pc.TaxPercentage;
        //                                        pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
        //                                        pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
        //                                        pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;
        //                                    }
        //                                    else
        //                                    {
        //                                        pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
        //                                    }
        //                                }
        //                                db.Entry(od).State = EntityState.Modified;

        //                                om.Status = "Delivery Canceled";
        //                                foreach (var pc in om.orderDetails)
        //                                {
        //                                    pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);

        //                                    int MOQ = pc.MinOrderQty;
        //                                    pc.MinOrderQtyPrice = MOQ * pc.UnitPrice;
        //                                    pc.TaxPercentage = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).TotalTaxPercentage : 0;
        //                                    pc.TotalCessPercentage = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).TotalCessPercentage : 0;// items.TotalCessPercentage;
        //                                    if (pc.TaxPercentage >= 0)
        //                                    {
        //                                        pc.SGSTTaxPercentage = pc.TaxPercentage / 2;
        //                                        pc.CGSTTaxPercentage = pc.TaxPercentage / 2;
        //                                    }
        //                                    pc.HSNCode = itemslist.Any(p => p.ItemId == pc.ItemId) ? itemslist.FirstOrDefault(p => p.ItemId == pc.ItemId).HSNCode : null;//items.HSNCode;
        //                                    pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
        //                                    pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
        //                                    //if there is cess for that item

        //                                    if (pc.TotalCessPercentage > 0)
        //                                    {
        //                                        // pc.TotalCessPercentage = odd.TotalCessPercentage;
        //                                        double tempPercentagge = pc.TotalCessPercentage + pc.TaxPercentage;

        //                                        pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge / 100)) / 100;
        //                                        pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
        //                                        pc.CessTaxAmount = (pc.AmtWithoutAfterTaxDisc * pc.TotalCessPercentage) / 100;
        //                                    }
        //                                    double tempPercentagge2 = pc.TotalCessPercentage + pc.TaxPercentage;
        //                                    pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge2 / 100)) / 100;
        //                                    pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + pc.DiscountPercentage);
        //                                    pc.TaxAmmount = (pc.AmtWithoutAfterTaxDisc * pc.TaxPercentage) / 100;
        //                                    if (pc.TaxAmmount >= 0)
        //                                    {
        //                                        pc.SGSTTaxAmmount = pc.TaxAmmount / 2;
        //                                        pc.CGSTTaxAmmount = pc.TaxAmmount / 2;
        //                                    }

        //                                    //for cess
        //                                    if (pc.CessTaxAmount > 0)
        //                                    {
        //                                        double tempPercentagge3 = pc.TotalCessPercentage + pc.TaxPercentage;
        //                                        pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
        //                                        pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
        //                                        pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;
        //                                    }
        //                                    else
        //                                    {
        //                                        pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
        //                                    }
        //                                }

        //                                foreach (var item in itemlist.OrderDetaileList)
        //                                {
        //                                    var kkr = db.KKReturnReplaceDetails.Where(x => x.NewOrderDetailsId == item.NewOrderDetailId && x.BatchId == item.BatchId && x.KKRRRequestId == item.KKRRRequestId).FirstOrDefault();
        //                                    if (kkr != null)
        //                                    {
        //                                        kkr.Qty = item.Qty;
        //                                        kkr.ModifiedDate = DateTime.Now;
        //                                    }
        //                                    db.Entry(kkr).State = EntityState.Modified;
        //                                }

        //                                db.Entry(od).State = EntityState.Modified;
        //                                #region Order Master History for Status Delivery Canceled
        //                                OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
        //                                OrderMasterHistories.orderid = od.OrderId;
        //                                OrderMasterHistories.Status = od.Status;
        //                                OrderMasterHistories.Reasoncancel = null;
        //                                OrderMasterHistories.Warehousename = od.WarehouseName;
        //                                OrderMasterHistories.DeliveryIssuanceId = od.DeliveryIssuanceIdOrderDeliveryMaster;
        //                                OrderMasterHistories.username = PeopleData.DisplayName != null ? PeopleData.DisplayName : PeopleData.PeopleFirstName;
        //                                OrderMasterHistories.userid = userid;
        //                                OrderMasterHistories.CreatedDate = DateTime.Now;
        //                                db.OrderMasterHistoriesDB.Add(OrderMasterHistories);
        //                                #endregion
        //                            }
        //                            else
        //                            {
        //                                foreach (var pc in od.orderDetails)
        //                                {
        //                                    pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);
        //                                    pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
        //                                    pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
        //                                }
        //                                db.Entry(od).State = EntityState.Modified;
        //                            }
        //                        }

        //                        var kkRequestdetail = KKreturnDetailData.Where(x => NewOrderDetailIds.Contains(x.NewOrderDetailsId)).ToList();
        //                        foreach (var x in kkRequestdetail)
        //                        {
        //                            x.Status = 6;
        //                            db.Entry(x).State = EntityState.Modified;
        //                        }
        //                    }

        //                    var totalAmt = returnOrderBillDiscountList.Sum(x => x.TotalAmount);

        //                    db.ReturnOrderBillDiscounts.AddRange(returnOrderBillDiscountList);
        //                    db.DeliveryCancelledDraftDB.AddRange(deliveryCancelledDraftList);

        //                    //db.Commit();
        //                }

        //                var tripDetailData = db.TripPlannerConfirmedDetails.Where(x => x.TripPlannerConfirmedMasterId == itemlist.TripPlannerConfirmedMasterId && x.CommaSeparatedOrderList != "").ToList();
        //                var assignmentids = orderDispatchedMasterNew.Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).Distinct().ToList();
        //                var orderDeliveryMaster = db.OrderDeliveryMasterDB.Where(x => OrderIdList.Contains(x.OrderId) && assignmentids.Contains(x.DeliveryIssuanceId)).ToList();

        //                /*orderDeliveryMaster.ForEach(x =>
        //                {   
        //                    x.Status = "Delivery Canceled";
        //                    logger.Info(x.Status);
        //                    db.Entry(x).State = EntityState.Modified;
        //                });*/
        //                foreach (var x in orderDeliveryMaster)
        //                {
        //                    x.Status = "Delivery Canceled";
        //                    db.Entry(x).State = EntityState.Modified;
        //                }

        //                orderDeliveryOTPData.ForEach(x =>
        //                {
        //                    x.IsUsed = true;
        //                    db.Entry(x).State = EntityState.Modified;
        //                });
        //                if (db.Commit() > 0)
        //                {
        //                    foreach(var DispatchedMaster in orderDispatchedMasterNew)
        //                    {
        //                        foreach (var StockHit in DispatchedMaster.orderDetails)
        //                        {
        //                            string RefCode = (StockHit.IsFreeItem == true && StockHit.IsDispatchedFreeStock == true) ? "F" : "C";
        //                            var dispatchId = new SqlParameter("@EntityId", StockHit.OrderDispatchedDetailsId);
        //                            var MultiMRPid = new SqlParameter("@ItemMultiMrpId", StockHit.ItemMultiMRPId);
        //                            var warehouseid = new SqlParameter("@WarehouseId", StockHit.WarehouseId);
        //                            var ReturnQty = new SqlParameter("@Qty", StockHit.qty);
        //                            var Userid = new SqlParameter("@UserId",itemlist.PeopleId);
        //                            var RefStockCode = new SqlParameter("@RefStockCode", RefCode);

        //                            db.Database.ExecuteSqlCommand("EXEC InsertReturnDeliveryCancelStocks " +
        //                                                            " @EntityId,@ItemMultiMrpId,@WarehouseId,@Qty,@UserId,@RefStockCode",
        //                                                            dispatchId,MultiMRPid,warehouseid,ReturnQty, Userid, RefStockCode);
        //                            db.Commit();
        //                        }
        //                    };
        //                    foreach (var item in itemlist.OrderDetaileList)
        //                    {
        //                        var TripPlannerConfirmedDetaildata = tripDetailData.Where(x => x.CommaSeparatedOrderList.Contains(item.NewOrderId.ToString())).FirstOrDefault();

        //                        var tripPlannerConfirmedDetailId = new SqlParameter("@TripPlannerConfirmedDetailId", TripPlannerConfirmedDetaildata.Id);
        //                        var OrderId = new SqlParameter("@OrderId", item.NewOrderId);
        //                        var userId = new SqlParameter("@userid", userid);
        //                        var DeliveryLat = new SqlParameter("@DeliveryLat", itemlist.DeliveryLat != null ? itemlist.DeliveryLat.Value : 0);
        //                        var DeliveryLng = new SqlParameter("@DeliveryLng", itemlist.DeliveryLat != null ? itemlist.DeliveryLng.Value : 0);
        //                        var WorkingStatus = new SqlParameter("@WorkingStatus", 5);//Convert.ToInt32(WorKingStatus.failed);
        //                        var OrderStatus = new SqlParameter("@OrderStatus", 2);//Convert.ToInt32(OrderStatusEnum.DeliveryCanceled);
        //                        db.Database.ExecuteSqlCommand("EXEC [Operation].[TripPlanner_SingleOrderUpdateForLastMileApp] " +
        //                            "@TripPlannerConfirmedDetailId,@OrderId,@userid,@DeliveryLat,@DeliveryLng,@WorkingStatus,@OrderStatus",
        //                            tripPlannerConfirmedDetailId, OrderId, userId, DeliveryLat, DeliveryLng, WorkingStatus, OrderStatus);
        //                        db.Commit();
        //                        //TripPlannerConfirmedDetaildata.IsProcess = true;
        //                        //TripPlannerConfirmedDetaildata.CustomerTripStatus = 4;
        //                        //db.Entry(TripPlannerConfirmedDetaildata).State = EntityState.Modified;

        //                    }
        //                    return new APIResponse { Status = true, Message = "Success" };
        //                }
        //                else
        //                {
        //                    return new APIResponse { Status = false, Message = "Failed" };
        //                }
        //            }
        //            else
        //            {
        //                return new APIResponse { Status = false, Message = "OTP Not Verified!!!" };
        //            }
        //        }
        //        else
        //        {
        //            return new APIResponse { Status = false, Message = "OrderId Not Found!!!" };
        //        }
        //    }
        //}

        [HttpPost]
        [Route("ReturnOrderCreditNote")]
        public async Task<APIResponse> ReturnOrderFinalAmount(List<ReturnOrderIdPostDc> data)
        {
            using (var Context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                DataTable dt = new DataTable();
                List<int?> orderids = new List<int?>();

                if (data != null && data.Count > 0)
                {
                    var OrderIdList = data.Select(x => x.OrderId).Distinct().ToList();

                    orderids = Context.DbOrderMaster.Where(x => OrderIdList.Contains(x.OrderId)).Select(x => x.ParentOrderId).ToList();

                    dt.Columns.Add("IntValue");

                    foreach (var id in orderids)
                    {
                        var dr = dt.NewRow();
                        dr["IntValue"] = id;
                        dt.Rows.Add(dr);
                    }
                }
                List<OrderBillDiscountDc> list = new List<OrderBillDiscountDc>();

                if (Context.Database.Connection.State != ConnectionState.Open)
                    Context.Database.Connection.Open();

                var cmd = Context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 300;
                cmd.CommandText = "[dbo].[BillDiscountCalculationForSalesReturnOrder]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var orderid = new SqlParameter("orderid", dt);
                cmd.Parameters.Add(orderid);

                var reader = cmd.ExecuteReader();

                list = ((IObjectContextAdapter)Context).ObjectContext.Translate<OrderBillDiscountDc>(reader).ToList();
                Context.Database.Connection.Close();
                if (list != null && list.Count > 0)
                {
                    List<ReturnOrderBillDiscount> returnOrderBillDiscountList = new List<ReturnOrderBillDiscount>();
                    List<DeliveryCancelledDraft> deliveryCancelledDraftList = new List<DeliveryCancelledDraft>();
                    foreach (var item in data)
                    {
                        ReturnOrderBillDiscount returnOrderBillDiscount = new ReturnOrderBillDiscount();
                        var billdiscount = list.Where(x => x.NewGeneratedOrderId == item.OrderId && x.NewOrderDetailsId == item.OrderDetailId).FirstOrDefault();
                        var finalbilldiscount = billdiscount != null ? item.qty * billdiscount.UnitbilldiscountAmt : 0;
                        returnOrderBillDiscount.OrderId = item.OrderId;
                        returnOrderBillDiscount.OrderDetailId = item.OrderDetailId;
                        returnOrderBillDiscount.Qty = item.qty;
                        returnOrderBillDiscount.BillDiscountAmount = finalbilldiscount;
                        returnOrderBillDiscount.TotalAmount = (item.qty * item.unitprice) - finalbilldiscount;
                        //returnOrderBillDiscount.CreatedBy = userid;
                        //returnOrderBillDiscount.CreatedDate = DateTime.Now;
                        //returnOrderBillDiscount.IsActive = true;
                        //returnOrderBillDiscount.IsDeleted = false;
                        returnOrderBillDiscount.WalletAmount = billdiscount != null ? item.qty * billdiscount.WalletAmount : 0;
                        returnOrderBillDiscountList.Add(returnOrderBillDiscount);
                    }

                    var totalAmt = Math.Round(returnOrderBillDiscountList.Sum(x => x.TotalAmount), 0);
                    return new APIResponse { Status = true, Message = "success", Data = totalAmt };
                }
                else
                {
                    return new APIResponse { Status = false, Message = "Failed" };
                }

            }

        }

        [HttpPost]
        [Route("CreateSalesReturnConfig")]
        public async Task<bool> CreateSalesReturnConfig()
        {
            bool res = false;

            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            MongoDbHelper<SalesReturnConfiguration> mongohelper = new MongoDbHelper<SalesReturnConfiguration>();

            SalesReturnConfiguration obj = new SalesReturnConfiguration();
            obj.IsSalesReturn = true;
            obj.IsActive = true;
            obj.IsDeleted = false;
            obj.CreatedDate = DateTime.Now;
            obj.CreatedBy = userid;
            res = mongohelper.Insert(obj);

            return res;
        }
        public bool IsSalesReturn()
        {
            bool IsReturn = false;
            MongoDbHelper<SalesReturnConfiguration> mongohelper = new MongoDbHelper<SalesReturnConfiguration>();
            var res = mongohelper.Select(x => x.IsActive == true && x.IsDeleted == false).FirstOrDefault();
            if (res != null)
            {
                IsReturn = res.IsSalesReturn;
            }
            return IsReturn;
        }

        #region Generate Random OTP For Return Order Confirmation
        [AllowAnonymous]
        [HttpPost]
        [Route("GenerateOTPForReturnOrder")]
        public async Task<APIResponse> GenerateOTPForReturnOrder(SalesReturnOTPDc otp)
        {
            string sRandomOTP = "";
            bool result = false;
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                using (AuthContext context = new AuthContext())
                {
                    var orderids = otp.OrderId.Select(x => x).Distinct().ToList();
                    if (context.OrderDispatchedMasters.Any(x => orderids.Contains(x.OrderId) && x.Status == otp.Status))
                    {
                        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);

                        if (!string.IsNullOrEmpty(sRandomOTP))
                        {
                            var orderDeliveryOTPs = context.OrderDeliveryOTP.Where(x => orderids.Contains(x.OrderId) && x.IsActive);
                            if (orderDeliveryOTPs != null)
                            {
                                foreach (var orderDeliveryOTP in orderDeliveryOTPs)
                                {

                                    orderDeliveryOTP.ModifiedDate = DateTime.Now;
                                    orderDeliveryOTP.ModifiedBy = userid;
                                    orderDeliveryOTP.IsActive = false;
                                    context.Entry(orderDeliveryOTP).State = EntityState.Modified;
                                }
                            }
                            foreach (var oid in orderids)
                            {
                                OrderDeliveryOTP OrderDeliveryOTP = new OrderDeliveryOTP
                                {
                                    CreatedBy = userid,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    OrderId = oid,
                                    OTP = sRandomOTP,
                                    Status = otp.Status,
                                    lat = otp.lat,
                                    lg = otp.lg
                                };
                                context.OrderDeliveryOTP.Add(OrderDeliveryOTP);
                                result = context.Commit() > 0;
                            }
                        }

                        DataTable orderdt = new DataTable();
                        orderdt.Columns.Add("IntValue");

                        if (orderids != null && orderids.Count > 0)
                        {
                            foreach (var id in otp.OrderId)
                            {
                                var dr = orderdt.NewRow();
                                dr["IntValue"] = id;
                                orderdt.Rows.Add(dr);
                            }
                        }
                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();

                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 300;
                        cmd.CommandText = "[dbo].[ConfirmReturnOrderOTP]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        var orderid = new SqlParameter("OrderId", orderdt);

                        cmd.Parameters.Add(orderid);

                        var reader = cmd.ExecuteReader();
                        var orderMobiledetail = ((IObjectContextAdapter)context).ObjectContext.Translate<ConfirmReturnOrderOTPRes>(reader).FirstOrDefault();

                        if (orderMobiledetail != null)
                        {
                            string message = ""; //"{#var1#} is Return pickup Code for Order No. {#var2#} for Total Qty {#var3#} and Value of Rs. {#var4#}. Shopkirana";
                            var dltSMS = SMSTemplateHelper.getTemplateText((int)AppEnum.DeliveryApp, "Return Pickup");
                            message = dltSMS == null ? "" : dltSMS.Template;

                            message = message.Replace("{#var#1}", sRandomOTP);
                            message = message.Replace("{#var#2}", orderMobiledetail.OrderId.ToString());
                            message = message.Replace("{#var#3}", orderMobiledetail.TotalQty.ToString());
                            message = message.Replace("{#var#4}", orderMobiledetail.TotalGrossAmount.ToString());
                            if (dltSMS != null)
                                Common.Helpers.SendSMSHelper.SendSMS(orderMobiledetail.Mobile, message, ((Int32)Common.Enums.SMSRouteEnum.OTP).ToString(), dltSMS.DLTId);
                            result = true;
                            return new APIResponse { Status = result, Message = "success" };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in GenerateOTPForOrder Method: " + ex.Message);
                result = false;
            }
            return new APIResponse { Status = result, Message = "success" }; ;
        }
        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }
        #endregion

        [HttpGet]
        [Route("CheckCatBrandConfig")]
        public APIResponse CheckBrandConfig(int CategoryId, int BrandId)
        {
            using (var context = new AuthContext())
            {
                var data = context.SalesReturnConfigs.Where(x => x.BrandId == BrandId && x.CategoryId == CategoryId && x.IsActive && !(bool)x.IsDeleted).FirstOrDefault();
                if (data != null)
                    return new APIResponse { Status = true };
                else
                    return new APIResponse { Status = false };
            }
        }
        #region history
        [HttpGet]
        [Route("GetKKReturnRequestDetailHistory")]
        [AllowAnonymous]
        public async Task<APIResponse> GetKKReturnRequestDetailHistory(int KKReturnDetailId)
        {
            using (AuthContext context = new AuthContext())
            {
                var KKDetailId = new SqlParameter("@KKReturnDetailId", KKReturnDetailId);
                var data = context.Database.SqlQuery<GetReturnDetailHistoryDc>("EXEC ReturnRequestDetailHistory @KKReturnDetailId", KKDetailId).ToList();

                if (data != null && data.Count > 0)
                    return new APIResponse { Status = true, Data = data };
                else
                    return new APIResponse { Status = false };
            }
        }
        #endregion

        [HttpPost]
        [Route("PostSalesReturnRequest")]
        public async Task<APIResponse> postrequestNew(PostKKReturnReplaceRequestDc data)
        {

            using (AuthContext context = new AuthContext())
            {
                bool ReturnEnable = context.CustomerSalesReturnConfigs.Any(x => x.CustomerId == data.CustomerId && x.IsActive && x.IsDeleted == false);
                if (!ReturnEnable)
                {
                    return new APIResponse { Status = false, Message = "Sales return option not available." };
                }
                List<SalesReturnItemImage> imglist = new List<SalesReturnItemImage>();
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    bool result = false;
                    var kkreturndetail = data.Details.ToList();
                    var orderids = kkreturndetail.Select(x => x.OrderId).Distinct().ToList();
                    var OrderDetailIds = data.Details.Select(x => x.OrderDetailsId).ToList();
                    var itemNumbers = context.DbOrderDetails.Where(x => OrderDetailIds.Contains(x.OrderDetailsId)).Select(x => x.itemNumber).ToList();
                    //var batchids = context.BatchMasters.Where(x => itemNumbers.Contains(x.ItemNumber)).ToList();
                    var ReturnReplaceRequestData = context.KKReturnReplaceRequests.Where(x => orderids.Contains(x.OrderId) && x.IsActive == true && x.IsDeleted == false && x.Status == "Pending" && x.CustomerId == data.CustomerId).ToList();

                    foreach (var orderid in orderids)
                    {
                        var ReturnReplaceRequest = ReturnReplaceRequestData.Where(x => x.OrderId == orderid).ToList();
                        if (ReturnReplaceRequest.Count == 0)
                        {
                            // add new request and detail

                            KKReturnReplaceRequest KKReturnReplaceRequest = new KKReturnReplaceRequest();
                            KKReturnReplaceRequest.CustomerId = data.CustomerId;
                            KKReturnReplaceRequest.OrderId = orderid;
                            KKReturnReplaceRequest.RequestType = 0;
                            KKReturnReplaceRequest.Status = "Pending";
                            KKReturnReplaceRequest.CreatedBy = data.CustomerId;
                            KKReturnReplaceRequest.CreatedDate = indianTime;
                            KKReturnReplaceRequest.ModifiedDate = indianTime;
                            KKReturnReplaceRequest.IsActive = true;
                            KKReturnReplaceRequest.IsDeleted = false;
                            context.KKReturnReplaceRequests.Add(KKReturnReplaceRequest);
                            context.Commit();


                            foreach (var i in kkreturndetail.Where(x => x.OrderId == orderid).ToList())
                            {
                                KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                KKReturnReplaceDetail.KKRRRequestId = KKReturnReplaceRequest.Id;
                                KKReturnReplaceDetail.OrderDetailsId = i.OrderDetailsId;
                                KKReturnReplaceDetail.OrderId = i.OrderId;
                                KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
                                KKReturnReplaceDetail.Qty = i.ReturnQty;///QTY IN TABLE ME RETURN
                                KKReturnReplaceDetail.ReturnReason = i.ReturnReason;
                                KKReturnReplaceDetail.BatchCode = i.BatchCode;
                                KKReturnReplaceDetail.BatchId = i.BatchMasterId; //batchids.FirstOrDefault(y => y.BatchCode == i.BatchCode).Id; //selectedData.BatchMasterId;
                                KKReturnReplaceDetail.CreatedDate = DateTime.Now;
                                KKReturnReplaceDetail.ItemMultiMRPId = i.ItemMultiMRPId;
                                context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);
                                context.Commit();

                                KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                                KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(KKReturnReplaceRequest.Id);
                                KKRequestReplaceHistory.KKRequestDetailId = Convert.ToInt32(KKReturnReplaceDetail.Id);
                                KKRequestReplaceHistory.Status = "Pending";
                                KKRequestReplaceHistory.CreatedBy = data.ExecutiveId > 0 ? data.ExecutiveId : data.CustomerId;
                                KKRequestReplaceHistory.CreatedDate = indianTime;
                                KKRequestReplaceHistory.ExecutiveId = data.ExecutiveId;
                                KKRequestReplaceHistory.CustomerId = data.CustomerId;
                                KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                                var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);

                                if (i.ItemImages.Count > 0)
                                {
                                    foreach (var itemImage in i.ItemImages)
                                    {
                                        SalesReturnItemImage img = new SalesReturnItemImage();
                                        img.KKRRDetailId = KKReturnReplaceDetail.Id;
                                        img.OrderDetailsId = i.OrderDetailsId;
                                        img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
                                        imglist.Add(img);
                                    }
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                        }
                        else
                        {
                            bool kkd = false;
                            long kkRequestId = 0;
                            var detail = context.KKReturnReplaceDetails.Where(x => x.OrderId == orderid).ToList();
                            //update line item
                            foreach (var item in ReturnReplaceRequest)
                            {
                                kkd = detail.Where(x => x.KKRRRequestId == item.Id).All(x => x.Status == 0);
                                if (kkd)
                                {
                                    kkRequestId = item.Id;
                                    break;
                                }
                            }
                            if (kkd && kkRequestId > 0)
                            {
                                // update line item qty batchcode wise
                                var KKReturnReplaceDetailList = detail.Where(x => x.KKRRRequestId == kkRequestId).ToList();
                                if (KKReturnReplaceDetailList != null && KKReturnReplaceDetailList.Any())
                                {
                                    foreach (var m in kkreturndetail.Where(x => x.OrderId == orderid).ToList())
                                    {
                                        var kKReturnReplaceDetail = detail.Where(x => x.KKRRRequestId == kkRequestId && x.OrderId == m.OrderId && x.OrderDetailsId == m.OrderDetailsId && x.BatchCode == m.BatchCode).FirstOrDefault();
                                        if (kKReturnReplaceDetail != null)
                                        {
                                            kKReturnReplaceDetail.Qty += m.ReturnQty;
                                            kKReturnReplaceDetail.ModifiedDate = DateTime.Now;
                                            context.Entry(kKReturnReplaceDetail).State = EntityState.Modified;

                                            KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                                            KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(kkRequestId);
                                            KKRequestReplaceHistory.KKRequestDetailId = Convert.ToInt32(kKReturnReplaceDetail.Id);
                                            KKRequestReplaceHistory.Status = "Pending";
                                            KKRequestReplaceHistory.CreatedBy = data.ExecutiveId > 0 ? data.ExecutiveId : data.CustomerId;
                                            KKRequestReplaceHistory.CreatedDate = indianTime;
                                            KKRequestReplaceHistory.ExecutiveId = data.ExecutiveId;
                                            KKRequestReplaceHistory.CustomerId = data.CustomerId;
                                            KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                                            var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);

                                            foreach (var itemImage in m.ItemImages)
                                            {
                                                SalesReturnItemImage img = new SalesReturnItemImage();
                                                img.KKRRDetailId = kKReturnReplaceDetail.Id;
                                                img.OrderDetailsId = kKReturnReplaceDetail.OrderDetailsId;
                                                img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
                                                imglist.Add(img);
                                            }
                                        }
                                        else
                                        {
                                            //foreach (var i in kkreturndetail.Where(x => x.OrderId == m.OrderId))
                                            {
                                                KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                                KKReturnReplaceDetail.KKRRRequestId = kkRequestId;
                                                KKReturnReplaceDetail.OrderDetailsId = m.OrderDetailsId;
                                                KKReturnReplaceDetail.OrderId = m.OrderId;
                                                KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
                                                KKReturnReplaceDetail.Qty = m.ReturnQty;///QTY IN TABLE ME RETURN
                                                KKReturnReplaceDetail.ReturnReason = m.ReturnReason;
                                                KKReturnReplaceDetail.BatchCode = m.BatchCode;
                                                KKReturnReplaceDetail.BatchId = m.BatchMasterId; //batchids.FirstOrDefault(y => y.BatchCode == m.BatchCode).Id;
                                                KKReturnReplaceDetail.ItemMultiMRPId = m.ItemMultiMRPId;
                                                KKReturnReplaceDetail.CreatedDate = DateTime.Now;
                                                context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);
                                                context.Commit();

                                                KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                                                KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(kkRequestId);
                                                KKRequestReplaceHistory.KKRequestDetailId = Convert.ToInt32(KKReturnReplaceDetail.Id);
                                                KKRequestReplaceHistory.Status = "Pending";
                                                KKRequestReplaceHistory.CreatedBy = data.ExecutiveId > 0 ? data.ExecutiveId : data.CustomerId;
                                                KKRequestReplaceHistory.CreatedDate = indianTime;
                                                KKRequestReplaceHistory.ExecutiveId = data.ExecutiveId;
                                                KKRequestReplaceHistory.CustomerId = data.CustomerId;
                                                KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                                                var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);

                                                if (m.ItemImages.Count > 0)
                                                {
                                                    foreach (var itemImage in m.ItemImages)
                                                    {
                                                        SalesReturnItemImage img = new SalesReturnItemImage();
                                                        img.KKRRDetailId = KKReturnReplaceDetail.Id;
                                                        img.OrderDetailsId = m.OrderDetailsId;
                                                        img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
                                                        imglist.Add(img);
                                                    }
                                                }
                                                else
                                                {
                                                    result = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //new request and detail
                                KKReturnReplaceRequest KKReturnReplaceRequest = new KKReturnReplaceRequest();
                                KKReturnReplaceRequest.CustomerId = data.CustomerId;
                                KKReturnReplaceRequest.OrderId = orderid;
                                KKReturnReplaceRequest.RequestType = 0;
                                KKReturnReplaceRequest.Status = "Pending";
                                KKReturnReplaceRequest.CreatedBy = data.CustomerId;
                                KKReturnReplaceRequest.CreatedDate = indianTime;
                                KKReturnReplaceRequest.ModifiedDate = indianTime;
                                KKReturnReplaceRequest.IsActive = true;
                                KKReturnReplaceRequest.IsDeleted = false;
                                context.KKReturnReplaceRequests.Add(KKReturnReplaceRequest);

                                if (context.Commit() > 0)
                                {
                                    foreach (var i in kkreturndetail.Where(x => x.OrderId == orderid))
                                    {
                                        KKReturnReplaceDetail KKReturnReplaceDetail = new KKReturnReplaceDetail();
                                        KKReturnReplaceDetail.KKRRRequestId = KKReturnReplaceRequest.Id;
                                        KKReturnReplaceDetail.OrderDetailsId = i.OrderDetailsId;
                                        KKReturnReplaceDetail.OrderId = i.OrderId;
                                        KKReturnReplaceDetail.Status = Convert.ToInt32(StatusType.Pending);
                                        KKReturnReplaceDetail.Qty = i.ReturnQty;///QTY IN TABLE ME RETURN
                                        KKReturnReplaceDetail.ReturnReason = i.ReturnReason;
                                        KKReturnReplaceDetail.BatchCode = i.BatchCode;
                                        KKReturnReplaceDetail.BatchId = i.BatchMasterId; // batchids.FirstOrDefault(y => y.BatchCode == i.BatchCode).Id;
                                        KKReturnReplaceDetail.ItemMultiMRPId = i.ItemMultiMRPId;
                                        KKReturnReplaceDetail.CreatedDate = DateTime.Now;
                                        context.KKReturnReplaceDetails.Add(KKReturnReplaceDetail);
                                        context.Commit();

                                        KKRequestReplaceHistory KKRequestReplaceHistory = new KKRequestReplaceHistory();
                                        KKRequestReplaceHistory.KKRequestId = Convert.ToInt32(KKReturnReplaceRequest.Id);
                                        KKRequestReplaceHistory.KKRequestDetailId = Convert.ToInt32(KKReturnReplaceDetail.Id);
                                        KKRequestReplaceHistory.Status = "Pending";
                                        KKRequestReplaceHistory.CreatedBy = data.ExecutiveId > 0 ? data.ExecutiveId : data.CustomerId;
                                        KKRequestReplaceHistory.CreatedDate = indianTime;
                                        KKRequestReplaceHistory.ExecutiveId = data.ExecutiveId;
                                        KKRequestReplaceHistory.CustomerId = data.CustomerId;
                                        KKReturnReplaceHistoryController KKReturnReplaceHistoryController = new KKReturnReplaceHistoryController();
                                        var addrequest = KKReturnReplaceHistoryController.AddKKReturnReplaceHistory(KKRequestReplaceHistory);

                                        if (i.ItemImages.Count > 0)
                                        {
                                            foreach (var itemImage in i.ItemImages)
                                            {
                                                SalesReturnItemImage img = new SalesReturnItemImage();
                                                img.KKRRDetailId = KKReturnReplaceDetail.Id;
                                                img.OrderDetailsId = i.OrderDetailsId;
                                                img.ItemReturnImage = "/UploadSalesReturnImage/" + itemImage;
                                                imglist.Add(img);
                                            }
                                        }
                                        else
                                        {
                                            result = false;
                                        }
                                    }
                                }
                            }
                            //}
                        }
                    }
                    if (imglist != null && imglist.Any())
                    {
                        context.SalesReturnItemImages.AddRange(imglist);
                        context.Commit();
                        dbContextTransaction.Commit();
                        result = true;
                    }

                    if (!result)
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
            return new APIResponse { Status = true, Data = data };
        }

        [HttpGet]
        [Route("SalesReturnDashboard")]
        public async Task<APIResponse> SalesReturnDashboard(int WarehouseId, int OrderId)
        {
            using (var context = new AuthContext())
            {
                var warehouseid = new SqlParameter("@WarehouseId", WarehouseId);
                var orderId = new SqlParameter("@OrderId", OrderId);
                var data = context.Database.SqlQuery<SalesReturnDashboardDC>("SalesReturnDashboard @WarehouseId, @OrderId", warehouseid, orderId).ToList();
                if (data != null && data.Count > 0)
                {
                    return new APIResponse { Status = true, Data = data };
                }
                else
                {
                    return new APIResponse { Status = false, Message = "Data Not Found!" };
                }
            }
        }
        [HttpGet]
        [Route("SalesReturnDashboardDetail")]
        public async Task<APIResponse> SalesReturnDashboardDetail(int OrderId)
        {
            using (var context = new AuthContext())
            {
                var orderid = new SqlParameter("@OrderId", OrderId);
                var data = context.Database.SqlQuery<SalesReturnDashboardDetailDC>("SalesReturnDashboardDetail @OrderId", orderid).ToList();
                if (data != null && data.Count > 0)
                {
                    return new APIResponse { Status = true, Data = data };
                }
                else
                {
                    return new APIResponse { Status = false, Message = "Data Not Found!" };
                }
            }
        }

        [HttpGet]
        [Route("SalesReturnExport")]
        public async Task<APIResponse> SalesReturnExport(int warehouseid)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var wareid = new SqlParameter("@warehouseid", warehouseid);
                    var data = context.Database.SqlQuery<SalesReturnExportDC>("SalesReturnExport @warehouseid", wareid).ToList();
                    if (data != null && data.Any())
                        return new APIResponse { Status = true, Data = data, Message = "Download Excel File." };
                    else
                        return new APIResponse { Status = false, Message = "Data Not Found." };
                }
            }
            catch (Exception ex)
            {
                return new APIResponse { Status = false, Message = ex.Message };
            }
        }

        [HttpPost]
        [Route("PickedReturnOrderByDBoy")]
        public async Task<APIResponse> PickedReturnOrderByDBoy(ItemPickByDBoyDc itemlist)
        {
            if (!IsSalesReturn())
            {
                return new APIResponse { Status = false, Message = "Failed" };
            }
            using (AuthContext db = new AuthContext())
            {
                APIResponse res = new APIResponse();
                bool ReturnStatus = false;
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (itemlist.OrderDetaileList.Count > 0)
                {
                    foreach (var item in itemlist.OrderDetaileList)
                    {
                        var kkd = db.KKReturnReplaceDetails.Where(x => x.KKRRRequestId == item.KKRRRequestId && x.NewOrderDetailsId == item.NewOrderDetailId && x.BatchId == item.BatchId).FirstOrDefault();
                        if (kkd.Qty < item.Qty)
                        {
                            res.Message = "Batchcode quantity not Greater than actual quantity for Batch : " + kkd.BatchCode;
                            res.Status = false;
                            ReturnStatus = true;
                            break;
                        }
                    }
                    if (ReturnStatus)
                    {
                        return res;
                    }

                    var OrderIdList = itemlist.OrderDetaileList.Select(x => x.NewOrderId).Distinct().ToList();
                    var orderDeliveryOTPData = db.OrderDeliveryOTP.Where(x => OrderIdList.Contains(x.OrderId) && x.OTP == itemlist.OTP && x.IsActive == true && x.IsUsed == false).ToList();
                    var PeopleData = db.Peoples.Where(x => x.PeopleID == itemlist.PeopleId).FirstOrDefault();

                    if (orderDeliveryOTPData.Count > 0)
                    {
                        List<int> ZeroOrderIds = new List<int>();
                        List<int> NotZeroOrderIds = new List<int>();
                        var notzerorderdata = itemlist.OrderDetaileList.Where(x => x.Qty > 0).GroupBy(y => y.NewOrderId).Select(a => a.FirstOrDefault()).ToList();
                        if (notzerorderdata != null && notzerorderdata.Any())
                        {
                            var ids = notzerorderdata.Select(z => z.NewOrderId).Distinct().ToList();
                            NotZeroOrderIds.AddRange(ids);
                        }
                        var zids = OrderIdList.Where(x => !NotZeroOrderIds.Contains(x)).Distinct().ToList();
                        if (zids != null && zids.Any())
                            ZeroOrderIds.AddRange(zids);

                        #region For Not Zero Quantity Orders
                        if (NotZeroOrderIds != null && NotZeroOrderIds.Any())
                        {
                            var OrderMasterData = db.DbOrderMaster.Where(x => NotZeroOrderIds.Contains(x.OrderId)).Include(x => x.orderDetails).ToList();
                            DataTable dt = new DataTable();
                            List<int?> orderids = new List<int?>();
                            List<OrderDispatchedMaster> orderDispatchedMasterNew = new List<OrderDispatchedMaster>();
                            if (itemlist.OrderDetaileList != null && itemlist.OrderDetaileList.Count > 0)
                            {
                                //var OrderIdList = itemlist.OrderDetaileList.Select(x => x.NewOrderId).Distinct().ToList();

                                //orderids = db.DbOrderMaster.Where(x => OrderIdList.Contains(x.OrderId)).Select(x => x.ParentOrderId).ToList();

                                orderids = OrderMasterData.Select(x => x.ParentOrderId).Distinct().ToList();
                                dt.Columns.Add("IntValue");

                                foreach (var id in orderids)
                                {
                                    var dr = dt.NewRow();
                                    dr["IntValue"] = id;
                                    dt.Rows.Add(dr);
                                }
                            }
                            List<OrderBillDiscountDc> list = new List<OrderBillDiscountDc>();

                            if (db.Database.Connection.State != ConnectionState.Open)
                                db.Database.Connection.Open();

                            var cmd = db.Database.Connection.CreateCommand();
                            cmd.CommandTimeout = 300;
                            cmd.CommandText = "[dbo].[BillDiscountCalculationForSalesReturnOrder]";
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            var orderid = new SqlParameter("orderid", dt);
                            cmd.Parameters.Add(orderid);

                            var reader = cmd.ExecuteReader();
                            list = ((IObjectContextAdapter)db).ObjectContext.Translate<OrderBillDiscountDc>(reader).ToList();
                            db.Database.Connection.Close();
                            var NewOrderIds = itemlist.OrderDetaileList.Where(x => NotZeroOrderIds.Contains(x.NewOrderId)).Select(x => x.NewOrderId).Distinct().ToList();
                            var NewOrderDetailIds = itemlist.OrderDetaileList.Where(x => NotZeroOrderIds.Contains(x.NewOrderId)).Select(x => x.NewOrderDetailId).Distinct().ToList();
                            var DeliveryCancelData = db.DeliveryCancelledDraftDB.Where(x => NewOrderIds.Contains(x.OrderId)).ToList();
                            if (list != null && list.Count > 0)
                            {
                                List<ReturnOrderBillDiscount> returnOrderBillDiscountList = new List<ReturnOrderBillDiscount>();
                                List<DeliveryCancelledDraft> deliveryCancelledDraftList = new List<DeliveryCancelledDraft>();
                                foreach (var item in itemlist.OrderDetaileList.Where(x => NotZeroOrderIds.Contains(x.NewOrderId)).ToList())
                                {
                                    ReturnOrderBillDiscount returnOrderBillDiscount = new ReturnOrderBillDiscount();
                                    var billdiscount = list.Where(x => x.NewGeneratedOrderId == item.NewOrderId && x.NewOrderDetailsId == item.NewOrderDetailId).FirstOrDefault();
                                    var finalbilldiscount = billdiscount != null ? item.Qty * billdiscount.UnitbilldiscountAmt : 0;
                                    returnOrderBillDiscount.OrderId = item.NewOrderId;
                                    returnOrderBillDiscount.OrderDetailId = item.NewOrderDetailId;
                                    returnOrderBillDiscount.Qty = item.Qty;
                                    returnOrderBillDiscount.BillDiscountAmount = finalbilldiscount;
                                    returnOrderBillDiscount.TotalAmount = (item.Qty * item.unitprice) - finalbilldiscount;
                                    returnOrderBillDiscount.CreatedBy = userid;
                                    returnOrderBillDiscount.CreatedDate = DateTime.Now;
                                    returnOrderBillDiscount.IsActive = true;
                                    returnOrderBillDiscount.IsDeleted = false;
                                    returnOrderBillDiscount.WalletAmount = billdiscount != null ? item.Qty * billdiscount.WalletAmount : 0;
                                    returnOrderBillDiscountList.Add(returnOrderBillDiscount);

                                    DeliveryCancelledDraft imageUpload = new DeliveryCancelledDraft();
                                    var ImgRes = DeliveryCancelData.Where(x => x.OrderId == item.NewOrderId).FirstOrDefault();
                                    if (ImgRes == null)
                                    {
                                        imageUpload.OrderId = item.NewOrderId;
                                        imageUpload.OrderItemImage = item.PickItemImage;
                                        imageUpload.IsActive = true;
                                        imageUpload.IsDeleted = false;
                                        imageUpload.CreatedBy = userid;
                                        imageUpload.CreatedDate = DateTime.Now;
                                        deliveryCancelledDraftList.Add(imageUpload);
                                    }
                                }

                                orderDispatchedMasterNew = db.OrderDispatchedMasters.Where(x => NewOrderIds.Contains(x.OrderId)).Include(x => x.orderDetails).ToList();
                                var KKreturnDetailData = db.KKReturnReplaceDetails.Where(x => NewOrderDetailIds.Contains(x.NewOrderDetailsId) && x.Status == 5).ToList();

                                foreach (var oid in NewOrderIds)
                                {
                                    var od = orderDispatchedMasterNew.Where(x => x.OrderId == oid).FirstOrDefault();
                                    var om = OrderMasterData.Where(x => x.OrderId == oid).FirstOrDefault();
                                    if (od != null)
                                    {
                                        if (od.orderDetails.Where(x => x.qty > 0).Any())
                                        {
                                            if (od.PocCreditNoteNumber == null || od.PocCreditNoteNumber == "")
                                            {
                                                var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == od.WarehouseId);

                                                string PocinvoiceNumber = "";
                                                if (od.WarehouseId != 67 && od.WarehouseId != 80)
                                                {
                                                    PocinvoiceNumber = db.Database.SqlQuery<string>("EXEC spGetPocCNCurrentNumber 'PocCreditNote', " + warehouse.Stateid).FirstOrDefault();
                                                }
                                                if (PocinvoiceNumber != null)
                                                {
                                                    db.ClearTaxIntegrations.Add(new ClearTaxIntegration
                                                    {
                                                        OrderId = Convert.ToInt32(oid),
                                                        APIType = "GenerateIRN",
                                                        IsProcessed = false,
                                                        Error = null,
                                                        IRNNo = null,
                                                        EwayBillNo = null,
                                                        RequestId = 0,
                                                        ResponseId = 0,
                                                        IsActive = true,
                                                        CreateDate = DateTime.Now,
                                                        ProcessDate = null,
                                                        IsOnline = false,
                                                        APITypes = 3
                                                    });
                                                }
                                                //od.PocCreditNoteNumber = PocinvoiceNumber;
                                                //od.PocCreditNoteDate = indianTime;
                                            }

                                            var BillDisAmt = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.BillDiscountAmount);
                                            od.BillDiscountAmount = BillDisAmt;
                                            var WalletDisAmt = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.WalletAmount);
                                            od.WalletAmount = WalletDisAmt;
                                            od.TotalAmount = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.TotalAmount);
                                            od.GrossAmount = Math.Round(returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.TotalAmount), 0);
                                            od.Status = "Delivery Canceled";

                                            List<int> itemids = null;
                                            List<ItemMaster> itemslist = null;

                                            itemids = od.orderDetails.Select(x => x.ItemId).Distinct().ToList();
                                            itemslist = db.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();

                                            foreach (var pc in od.orderDetails)
                                            {
                                                pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);

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
                                                    // pc.TotalCessPercentage = odd.TotalCessPercentage;
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
                                            }
                                            db.Entry(od).State = EntityState.Modified;

                                            om.Status = "Delivery Canceled";
                                            foreach (var pc in om.orderDetails)
                                            {
                                                pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);

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
                                                    // pc.TotalCessPercentage = odd.TotalCessPercentage;
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
                                            }

                                            foreach (var item in itemlist.OrderDetaileList.Where(x => NotZeroOrderIds.Contains(x.NewOrderId)).ToList())
                                            {
                                                var kkr = db.KKReturnReplaceDetails.Where(x => x.NewOrderDetailsId == item.NewOrderDetailId && x.BatchId == item.BatchId && x.KKRRRequestId == item.KKRRRequestId).FirstOrDefault();
                                                if (kkr != null)
                                                {
                                                    kkr.Qty = item.Qty;
                                                    kkr.ModifiedDate = DateTime.Now;
                                                }
                                                db.Entry(kkr).State = EntityState.Modified;
                                            }

                                            db.Entry(od).State = EntityState.Modified;
                                            #region Order Master History for Status Delivery Canceled
                                            OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                            OrderMasterHistories.orderid = od.OrderId;
                                            OrderMasterHistories.Status = od.Status;
                                            OrderMasterHistories.Reasoncancel = null;
                                            OrderMasterHistories.Warehousename = od.WarehouseName;
                                            OrderMasterHistories.DeliveryIssuanceId = od.DeliveryIssuanceIdOrderDeliveryMaster;
                                            OrderMasterHistories.username = PeopleData.DisplayName != null ? PeopleData.DisplayName : PeopleData.PeopleFirstName;
                                            OrderMasterHistories.userid = userid;
                                            OrderMasterHistories.CreatedDate = DateTime.Now;
                                            db.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                            #endregion
                                        }
                                        else
                                        {
                                            foreach (var pc in od.orderDetails)
                                            {
                                                pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);
                                                pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
                                                pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
                                            }
                                            db.Entry(od).State = EntityState.Modified;
                                        }
                                    }

                                    var kkRequestdetail = KKreturnDetailData.Where(x => NewOrderDetailIds.Contains(x.NewOrderDetailsId)).ToList();
                                    foreach (var x in kkRequestdetail)
                                    {
                                        x.Status = 6;
                                        db.Entry(x).State = EntityState.Modified;
                                    }
                                }

                                var totalAmt = returnOrderBillDiscountList.Sum(x => x.TotalAmount);

                                db.ReturnOrderBillDiscounts.AddRange(returnOrderBillDiscountList);
                                db.DeliveryCancelledDraftDB.AddRange(deliveryCancelledDraftList);

                                //db.Commit();
                            }

                            var tripDetailData = db.TripPlannerConfirmedDetails.Where(x => x.TripPlannerConfirmedMasterId == itemlist.TripPlannerConfirmedMasterId && x.CommaSeparatedOrderList != "").ToList();
                            var assignmentids = orderDispatchedMasterNew.Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).Distinct().ToList();
                            var orderDeliveryMaster = db.OrderDeliveryMasterDB.Where(x => NotZeroOrderIds.Contains(x.OrderId) && assignmentids.Contains(x.DeliveryIssuanceId)).ToList();

                            /*orderDeliveryMaster.ForEach(x =>
                            {   
                                x.Status = "Delivery Canceled";
                                logger.Info(x.Status);
                                db.Entry(x).State = EntityState.Modified;
                            });*/
                            foreach (var x in orderDeliveryMaster)
                            {
                                x.Status = "Delivery Canceled";
                                db.Entry(x).State = EntityState.Modified;
                            }

                            orderDeliveryOTPData.Where(x => NotZeroOrderIds.Contains(x.OrderId)).ToList().ForEach(x =>
                              {
                                  x.IsUsed = true;
                                  db.Entry(x).State = EntityState.Modified;
                              });
                            if (db.Commit() > 0)
                            {
                                foreach (var DispatchedMaster in orderDispatchedMasterNew)
                                {
                                    foreach (var StockHit in DispatchedMaster.orderDetails)
                                    {
                                        string RefCode = (StockHit.IsFreeItem == true && StockHit.IsDispatchedFreeStock == true) ? "F" : "C";
                                        var dispatchId = new SqlParameter("@EntityId", StockHit.OrderDispatchedDetailsId);
                                        var MultiMRPid = new SqlParameter("@ItemMultiMrpId", StockHit.ItemMultiMRPId);
                                        var warehouseid = new SqlParameter("@WarehouseId", StockHit.WarehouseId);
                                        var ReturnQty = new SqlParameter("@Qty", StockHit.qty);
                                        var Userid = new SqlParameter("@UserId", itemlist.PeopleId);
                                        var RefStockCode = new SqlParameter("@RefStockCode", RefCode);

                                        db.Database.ExecuteSqlCommand("EXEC InsertReturnDeliveryCancelStocks " +
                                                                        " @EntityId,@ItemMultiMrpId,@WarehouseId,@Qty,@UserId,@RefStockCode",
                                                                        dispatchId, MultiMRPid, warehouseid, ReturnQty, Userid, RefStockCode);
                                        db.Commit();
                                    }
                                };
                                foreach (var item in itemlist.OrderDetaileList.Where(x => NotZeroOrderIds.Contains(x.NewOrderId)).ToList())
                                {
                                    var TripPlannerConfirmedDetaildata = tripDetailData.Where(x => x.CommaSeparatedOrderList.Contains(item.NewOrderId.ToString())).FirstOrDefault();

                                    var tripPlannerConfirmedDetailId = new SqlParameter("@TripPlannerConfirmedDetailId", TripPlannerConfirmedDetaildata.Id);
                                    var OrderId = new SqlParameter("@OrderId", item.NewOrderId);
                                    var userId = new SqlParameter("@userid", userid);
                                    var DeliveryLat = new SqlParameter("@DeliveryLat", itemlist.DeliveryLat != null ? itemlist.DeliveryLat.Value : 0);
                                    var DeliveryLng = new SqlParameter("@DeliveryLng", itemlist.DeliveryLat != null ? itemlist.DeliveryLng.Value : 0);
                                    var WorkingStatus = new SqlParameter("@WorkingStatus", 5);//Convert.ToInt32(WorKingStatus.failed);
                                    var OrderStatus = new SqlParameter("@OrderStatus", 2);//Convert.ToInt32(OrderStatusEnum.DeliveryCanceled);
                                    db.Database.ExecuteSqlCommand("EXEC [Operation].[TripPlanner_SingleOrderUpdateForLastMileApp] " +
                                        "@TripPlannerConfirmedDetailId,@OrderId,@userid,@DeliveryLat,@DeliveryLng,@WorkingStatus,@OrderStatus",
                                        tripPlannerConfirmedDetailId, OrderId, userId, DeliveryLat, DeliveryLng, WorkingStatus, OrderStatus);
                                    db.Commit();
                                    //TripPlannerConfirmedDetaildata.IsProcess = true;
                                    //TripPlannerConfirmedDetaildata.CustomerTripStatus = 4;
                                    //db.Entry(TripPlannerConfirmedDetaildata).State = EntityState.Modified;

                                }
                                ReturnStatus = false;
                            }
                            else
                            {
                                ReturnStatus = true;
                            }
                        }

                        #endregion


                        #region For Zero Quantity Order
                        if (!ReturnStatus)
                        {
                            if (ZeroOrderIds != null && ZeroOrderIds.Any())
                            {
                                var OrderMasterData = db.DbOrderMaster.Where(x => ZeroOrderIds.Contains(x.OrderId)).Include(x => x.orderDetails).ToList();
                                DataTable dt = new DataTable();
                                List<int?> orderids = new List<int?>();
                                List<OrderDispatchedMaster> orderDispatchedMasterNew = new List<OrderDispatchedMaster>();
                                if (itemlist.OrderDetaileList != null && itemlist.OrderDetaileList.Count > 0)
                                {
                                    //var OrderIdList = itemlist.OrderDetaileList.Select(x => x.NewOrderId).Distinct().ToList();

                                    //orderids = db.DbOrderMaster.Where(x => OrderIdList.Contains(x.OrderId)).Select(x => x.ParentOrderId).ToList();

                                    orderids = OrderMasterData.Select(x => x.ParentOrderId).Distinct().ToList();
                                    dt.Columns.Add("IntValue");

                                    foreach (var id in orderids)
                                    {
                                        var dr = dt.NewRow();
                                        dr["IntValue"] = id;
                                        dt.Rows.Add(dr);
                                    }
                                }
                                List<OrderBillDiscountDc> list = new List<OrderBillDiscountDc>();

                                if (db.Database.Connection.State != ConnectionState.Open)
                                    db.Database.Connection.Open();

                                var cmd = db.Database.Connection.CreateCommand();
                                cmd.CommandTimeout = 300;
                                cmd.CommandText = "[dbo].[BillDiscountCalculationForSalesReturnOrder]";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                var orderid = new SqlParameter("orderid", dt);
                                cmd.Parameters.Add(orderid);

                                var reader = cmd.ExecuteReader();
                                list = ((IObjectContextAdapter)db).ObjectContext.Translate<OrderBillDiscountDc>(reader).ToList();
                                db.Database.Connection.Close();
                                var NewOrderIds = itemlist.OrderDetaileList.Where(x => ZeroOrderIds.Contains(x.NewOrderId)).Select(x => x.NewOrderId).Distinct().ToList();
                                var NewOrderDetailIds = itemlist.OrderDetaileList.Where(x => ZeroOrderIds.Contains(x.NewOrderId)).Select(x => x.NewOrderDetailId).Distinct().ToList();
                                var DeliveryCancelData = db.DeliveryCancelledDraftDB.Where(x => NewOrderIds.Contains(x.OrderId)).ToList();
                                if (list != null && list.Count > 0)
                                {
                                    //List<ReturnOrderBillDiscount> returnOrderBillDiscountList = new List<ReturnOrderBillDiscount>();
                                    //List<DeliveryCancelledDraft> deliveryCancelledDraftList = new List<DeliveryCancelledDraft>();
                                    //foreach (var item in itemlist.OrderDetaileList.Where(x => NotZeroOrderIds.Contains(x.NewOrderId)).ToList())
                                    //{
                                    //    ReturnOrderBillDiscount returnOrderBillDiscount = new ReturnOrderBillDiscount();
                                    //    var billdiscount = list.Where(x => x.NewGeneratedOrderId == item.NewOrderId && x.NewOrderDetailsId == item.NewOrderDetailId).FirstOrDefault();
                                    //    var finalbilldiscount = billdiscount != null ? item.Qty * billdiscount.UnitbilldiscountAmt : 0;
                                    //    returnOrderBillDiscount.OrderId = item.NewOrderId;
                                    //    returnOrderBillDiscount.OrderDetailId = item.NewOrderDetailId;
                                    //    returnOrderBillDiscount.Qty = item.Qty;
                                    //    returnOrderBillDiscount.BillDiscountAmount = finalbilldiscount;
                                    //    returnOrderBillDiscount.TotalAmount = (item.Qty * item.unitprice) - finalbilldiscount;
                                    //    returnOrderBillDiscount.CreatedBy = userid;
                                    //    returnOrderBillDiscount.CreatedDate = DateTime.Now;
                                    //    returnOrderBillDiscount.IsActive = true;
                                    //    returnOrderBillDiscount.IsDeleted = false;
                                    //    returnOrderBillDiscount.WalletAmount = billdiscount != null ? item.Qty * billdiscount.WalletAmount : 0;
                                    //    returnOrderBillDiscountList.Add(returnOrderBillDiscount);

                                    //    DeliveryCancelledDraft imageUpload = new DeliveryCancelledDraft();
                                    //    var ImgRes = DeliveryCancelData.Where(x => x.OrderId == item.NewOrderId).FirstOrDefault();
                                    //    if (ImgRes == null)
                                    //    {
                                    //        imageUpload.OrderId = item.NewOrderId;
                                    //        imageUpload.OrderItemImage = item.PickItemImage;
                                    //        imageUpload.IsActive = true;
                                    //        imageUpload.IsDeleted = false;
                                    //        imageUpload.CreatedBy = userid;
                                    //        imageUpload.CreatedDate = DateTime.Now;
                                    //        deliveryCancelledDraftList.Add(imageUpload);
                                    //    }
                                    //}

                                    orderDispatchedMasterNew = db.OrderDispatchedMasters.Where(x => NewOrderIds.Contains(x.OrderId)).Include(x => x.orderDetails).ToList();
                                    var KKreturnDetailData = db.KKReturnReplaceDetails.Where(x => NewOrderDetailIds.Contains(x.NewOrderDetailsId) && x.Status == 5).ToList();

                                    foreach (var oid in NewOrderIds)
                                    {
                                        var od = orderDispatchedMasterNew.Where(x => x.OrderId == oid).FirstOrDefault();
                                        var om = OrderMasterData.Where(x => x.OrderId == oid).FirstOrDefault();
                                        if (od != null)
                                        {
                                            //if (od.orderDetails.Where(x => x.qty > 0).Any())
                                            //{
                                            //if (od.PocCreditNoteNumber == null || od.PocCreditNoteNumber == "")
                                            //{
                                            //    var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == od.WarehouseId);

                                            //    string PocinvoiceNumber = "";
                                            //    if (od.WarehouseId != 67 && od.WarehouseId != 80)
                                            //    {
                                            //        PocinvoiceNumber = db.Database.SqlQuery<string>("EXEC spGetPocCNCurrentNumber 'PocCreditNote', " + warehouse.Stateid).FirstOrDefault();
                                            //    }
                                            //    if (PocinvoiceNumber != null)
                                            //    {
                                            //        db.ClearTaxIntegrations.Add(new ClearTaxIntegration
                                            //        {
                                            //            OrderId = Convert.ToInt32(oid),
                                            //            APIType = "GenerateIRN",
                                            //            IsProcessed = false,
                                            //            Error = null,
                                            //            IRNNo = null,
                                            //            EwayBillNo = null,
                                            //            RequestId = 0,
                                            //            ResponseId = 0,
                                            //            IsActive = true,
                                            //            CreateDate = DateTime.Now,
                                            //            ProcessDate = null,
                                            //            IsOnline = false,
                                            //            APITypes = 3
                                            //        });
                                            //    }
                                            //    od.PocCreditNoteNumber = PocinvoiceNumber;
                                            //    od.PocCreditNoteDate = indianTime;
                                            //}

                                            //var BillDisAmt = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.BillDiscountAmount);
                                            od.BillDiscountAmount = 0;
                                            //var WalletDisAmt = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.WalletAmount);
                                            od.WalletAmount = 0;
                                            //od.TotalAmount = returnOrderBillDiscountList.Where(x => x.OrderId == oid).Sum(x => x.TotalAmount);
                                            od.GrossAmount = 0;
                                            od.Status = "Return Canceled";

                                            List<int> itemids = null;
                                            List<ItemMaster> itemslist = null;

                                            itemids = od.orderDetails.Select(x => x.ItemId).Distinct().ToList();
                                            itemslist = db.itemMasters.Where(x => itemids.Contains(x.ItemId)).ToList();

                                            foreach (var pc in od.orderDetails)
                                            {
                                                pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);

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
                                                    // pc.TotalCessPercentage = odd.TotalCessPercentage;
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
                                            }
                                            db.Entry(od).State = EntityState.Modified;

                                            om.Status = "Return Canceled";
                                            foreach (var pc in om.orderDetails)
                                            {
                                                pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);

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
                                                    // pc.TotalCessPercentage = odd.TotalCessPercentage;
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
                                            }

                                            foreach (var item in itemlist.OrderDetaileList.Where(x => ZeroOrderIds.Contains(x.NewOrderId)).ToList())
                                            {
                                                var kkr = db.KKReturnReplaceDetails.Where(x => x.NewOrderDetailsId == item.NewOrderDetailId && x.BatchId == item.BatchId && x.KKRRRequestId == item.KKRRRequestId).FirstOrDefault();
                                                if (kkr != null)
                                                {
                                                    kkr.Qty = item.Qty;
                                                    kkr.ModifiedDate = DateTime.Now;
                                                }
                                                db.Entry(kkr).State = EntityState.Modified;
                                            }

                                            db.Entry(od).State = EntityState.Modified;
                                            #region Order Master History for Status Delivery Canceled
                                            OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                                            OrderMasterHistories.orderid = od.OrderId;
                                            OrderMasterHistories.Status = od.Status;
                                            OrderMasterHistories.Reasoncancel = null;
                                            OrderMasterHistories.Warehousename = od.WarehouseName;
                                            OrderMasterHistories.DeliveryIssuanceId = od.DeliveryIssuanceIdOrderDeliveryMaster;
                                            OrderMasterHistories.username = PeopleData.DisplayName != null ? PeopleData.DisplayName : PeopleData.PeopleFirstName;
                                            OrderMasterHistories.userid = userid;
                                            OrderMasterHistories.CreatedDate = DateTime.Now;
                                            db.OrderMasterHistoriesDB.Add(OrderMasterHistories);
                                            #endregion
                                            //}
                                            //else
                                            //{
                                            //    foreach (var pc in od.orderDetails)
                                            //    {
                                            //        pc.qty = itemlist.OrderDetaileList.Where(x => x.NewOrderDetailId == pc.OrderDetailsId).Sum(y => y.Qty);
                                            //        pc.Noqty = pc.qty;//qty; // for total qty (no of items)    
                                            //        pc.TotalAmt = System.Math.Round(pc.UnitPrice * pc.qty, 2);
                                            //    }
                                            //    db.Entry(od).State = EntityState.Modified;
                                            //}
                                        }

                                        var kkRequestdetail = KKreturnDetailData.Where(x => NewOrderDetailIds.Contains(x.NewOrderDetailsId)).ToList();
                                        foreach (var x in kkRequestdetail)
                                        {
                                            x.Status = 8;
                                            db.Entry(x).State = EntityState.Modified;
                                        }
                                    }

                                    //var totalAmt = returnOrderBillDiscountList.Sum(x => x.TotalAmount);

                                    //db.ReturnOrderBillDiscounts.AddRange(returnOrderBillDiscountList);
                                    //db.DeliveryCancelledDraftDB.AddRange(deliveryCancelledDraftList);

                                    //db.Commit();
                                }

                                var tripDetailData = db.TripPlannerConfirmedDetails.Where(x => x.TripPlannerConfirmedMasterId == itemlist.TripPlannerConfirmedMasterId && x.CommaSeparatedOrderList != "").ToList();
                                var assignmentids = orderDispatchedMasterNew.Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).Distinct().ToList();
                                var orderDeliveryMaster = db.OrderDeliveryMasterDB.Where(x => ZeroOrderIds.Contains(x.OrderId) && assignmentids.Contains(x.DeliveryIssuanceId)).ToList();

                                /*orderDeliveryMaster.ForEach(x =>
                                {   
                                    x.Status = "Delivery Canceled";
                                    logger.Info(x.Status);
                                    db.Entry(x).State = EntityState.Modified;
                                });*/
                                foreach (var x in orderDeliveryMaster)
                                {
                                    x.Status = "Return Canceled";
                                    db.Entry(x).State = EntityState.Modified;
                                }

                                orderDeliveryOTPData.Where(x => ZeroOrderIds.Contains(x.OrderId)).ToList().ForEach(x =>
                                {
                                    x.IsUsed = true;
                                    db.Entry(x).State = EntityState.Modified;
                                });
                                if (db.Commit() > 0)
                                {
                                    //foreach (var DispatchedMaster in orderDispatchedMasterNew)
                                    //{
                                    //    foreach (var StockHit in DispatchedMaster.orderDetails)
                                    //    {
                                    //        string RefCode = (StockHit.IsFreeItem == true && StockHit.IsDispatchedFreeStock == true) ? "F" : "C";
                                    //        var dispatchId = new SqlParameter("@EntityId", StockHit.OrderDispatchedDetailsId);
                                    //        var MultiMRPid = new SqlParameter("@ItemMultiMrpId", StockHit.ItemMultiMRPId);
                                    //        var warehouseid = new SqlParameter("@WarehouseId", StockHit.WarehouseId);
                                    //        var ReturnQty = new SqlParameter("@Qty", StockHit.qty);
                                    //        var Userid = new SqlParameter("@UserId", itemlist.PeopleId);
                                    //        var RefStockCode = new SqlParameter("@RefStockCode", RefCode);

                                    //        db.Database.ExecuteSqlCommand("EXEC InsertReturnDeliveryCancelStocks " +
                                    //                                        " @EntityId,@ItemMultiMrpId,@WarehouseId,@Qty,@UserId,@RefStockCode",
                                    //                                        dispatchId, MultiMRPid, warehouseid, ReturnQty, Userid, RefStockCode);
                                    //        db.Commit();
                                    //    }
                                    //};
                                    foreach (var item in itemlist.OrderDetaileList.Where(x => ZeroOrderIds.Contains(x.NewOrderId)).ToList())
                                    {
                                        var TripPlannerConfirmedDetaildata = tripDetailData.Where(x => x.CommaSeparatedOrderList.Contains(item.NewOrderId.ToString())).FirstOrDefault();

                                        var tripPlannerConfirmedDetailId = new SqlParameter("@TripPlannerConfirmedDetailId", TripPlannerConfirmedDetaildata.Id);
                                        var OrderId = new SqlParameter("@OrderId", item.NewOrderId);
                                        var userId = new SqlParameter("@userid", userid);
                                        var DeliveryLat = new SqlParameter("@DeliveryLat", itemlist.DeliveryLat != null ? itemlist.DeliveryLat.Value : 0);
                                        var DeliveryLng = new SqlParameter("@DeliveryLng", itemlist.DeliveryLat != null ? itemlist.DeliveryLng.Value : 0);
                                        var WorkingStatus = new SqlParameter("@WorkingStatus", 5);//Convert.ToInt32(WorKingStatus.failed);
                                        var OrderStatus = new SqlParameter("@OrderStatus", 2);//Convert.ToInt32(OrderStatusEnum.DeliveryCanceled);
                                        db.Database.ExecuteSqlCommand("EXEC [Operation].[TripPlanner_SingleOrderUpdateForLastMileApp] " +
                                            "@TripPlannerConfirmedDetailId,@OrderId,@userid,@DeliveryLat,@DeliveryLng,@WorkingStatus,@OrderStatus",
                                            tripPlannerConfirmedDetailId, OrderId, userId, DeliveryLat, DeliveryLng, WorkingStatus, OrderStatus);
                                        db.Commit();
                                        //TripPlannerConfirmedDetaildata.IsProcess = true;
                                        //TripPlannerConfirmedDetaildata.CustomerTripStatus = 4;
                                        //db.Entry(TripPlannerConfirmedDetaildata).State = EntityState.Modified;

                                    }
                                    ReturnStatus = false;

                                }
                                else
                                {
                                    ReturnStatus = true;
                                }
                            }
                        }
                        #endregion

                        if (!ReturnStatus)
                        {
                            return new APIResponse { Status = true, Message = "Success" };
                        }
                        else
                        {
                            return new APIResponse { Status = false, Message = "Failed" };
                        }
                    }
                    else
                    {
                        return new APIResponse { Status = false, Message = "OTP Not Verified!!!" };
                    }
                }
                else
                {
                    return new APIResponse { Status = false, Message = "OrderId Not Found!!!" };
                }
            }
        }

        [HttpGet]
        [Route("CheckReturnEnable")]
        public async Task<APIResponse> CheckReturnEnable(int CustomerId)
        {
            using (var db = new AuthContext())
            {
                var ReturnEnable = db.CustomerSalesReturnConfigs.Any(x => x.CustomerId == CustomerId && x.IsActive && x.IsDeleted == false);
                if(ReturnEnable)
                    return new APIResponse { Status = true,Data= ReturnEnable };
                else
                    return new APIResponse { Status = false, Message = "Sales return option not available." };
            }
        }
    }
}
public enum RequestType
{
    Return = 0,
    Replace = 1
}

public enum StatusType
{
    Pending = 0,
    Warehouse_Approved = 1,
    Warehouse_Reject = 2,
    Buyer_Approved = 3,
    Buyer_Reject = 4,
    Planner = 5,
    Picked = 6,
    Complete = 7,
    ReturnCancel = 8
}
public enum CustStatusType
{
    Pending = 0,
    Approved = 1,
    Reject = 2,
    ReadyToPick = 3
}
public class EwayBillBackendDc
{
    public long TripPlannerConfirmedMasterid { get; set; }
    public string TransportGST { get; set; }
    public string TransportName { get; set; }
    public string vehicleno { get; set; }
    public long VehicleId { get; set; }
    public int distance { get; set; }
    public int OrderId { get; set; }
    public bool? IsReplacementVehicleNo { get; set; }
    public string ReplacementVehicleNo { get; set; }
}
