using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Transaction.EpayLater;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using GenricEcommers.Models;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Description;
using AngularJSAuthentication.DataContracts.Masters.PaymentRefund;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.API.Helper.PaymentRefund;
using static AngularJSAuthentication.API.Controllers.Reporting.CustomerWalletPointController;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using AngularJSAuthentication.API.Helper.Notification;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderDispatchedDetailsReturn")]
    public class OrderDispatchedDetailsReturnController : ApiController
    {

        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [ResponseType(typeof(ReturnOrderDispatchedDetails))]
        [Route("")]
        [AcceptVerbs("POST")]
        [Authorize]
        public HttpResponseMessage add(List<ReturnOrderDispatchedDetails> po)
        {
            bool result = false;
            People people = null;
            OrderMaster om = null;
            List<PaymentResponseRetailerApp> payments = null;
            bool isDispatched = true;
            List<OrderDispatchedDetails> dispatchedobj = null;
            // Access claims
            OrderDispatchedMaster odmMDataD = null;
            int stateId = 0;
            DeliveryIssuance DeliveryIssuance = null;
            Int32 Oid = 0;
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (userid > 0 && po != null)
            {

                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(90);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.Required, option))
                {
                    using (var db = new AuthContext())
                    {
                        Oid = po[0].OrderId;
                        people = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                        odmMDataD = db.OrderDispatchedMasters.Where(x => x.OrderId == Oid && x.Status == "Delivery Canceled" && x.Deleted == false).Include("orderDetails").FirstOrDefault();

                        var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == odmMDataD.WarehouseId);

                        if (warehouse.IsStopCurrentStockTrans)
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Inventory Transactions are currently disabled for this warehouse... Please try after some time");

                        if (odmMDataD != null && odmMDataD.DeliveryIssuanceIdOrderDeliveryMaster.HasValue && odmMDataD.DeliveryIssuanceIdOrderDeliveryMaster.Value > 0)
                        {
                            DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == odmMDataD.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault();
                            if (DeliveryIssuance.Status == "Payment Accepted" || DeliveryIssuance.Status == "Submitted" || DeliveryIssuance.Status == "Pending")
                            {
                                odmMDataD.DeliveryIssuanceStatus = DeliveryIssuance.Status;
                                isDispatched = false;
                                return Request.CreateResponse(HttpStatusCode.OK);

                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        om = db.DbOrderMaster.Where(x => x.OrderId == odmMDataD.OrderId && x.Deleted == false && x.CompanyId == compid).Include("orderDetails").FirstOrDefault();
                        payments = db.PaymentResponseRetailerAppDb.Where(x => x.OrderId == odmMDataD.OrderId && x.status == "Success" && x.IsOnline /*(x.PaymentFrom == "ePaylater" || x.PaymentFrom == "hdfc")*/).ToList();

                        #region Zaruri order
                        if (om.OrderType == 5)
                        {
                            om.Status = "Post Order Canceled";
                            om.UpdatedDate = indianTime;
                            db.Entry(om).State = EntityState.Modified;

                            odmMDataD.Status = "Post Order Canceled";
                            odmMDataD.UpdatedDate = indianTime;
                            db.Entry(odmMDataD).State = EntityState.Modified;

                            #region Order Master History for Status Post Order Canceled  

                            OrderMasterHistories h1 = new OrderMasterHistories();

                            h1.orderid = odmMDataD.OrderId;
                            h1.Status = odmMDataD.Status;
                            h1.Reasoncancel = null;
                            h1.Warehousename = odmMDataD.WarehouseName;
                            h1.username = people.DisplayName;
                            h1.userid = userid;
                            h1.CreatedDate = indianTime;
                            db.OrderMasterHistoriesDB.Add(h1);
                            if (db.Commit() > 0)
                            {
                                try
                                {
                                    UpdateConsumerOrders updateConsumerOrders = new UpdateConsumerOrders
                                    {
                                        CartStatus = "Cancelled",
                                        InvoiceNo = om.invoice_no
                                    };
                                    var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/ShoppingCart/SKUpdateCartStatus";
                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Add("CustomerId", "1");
                                        client.DefaultRequestHeaders.Add("NoEncryption", "1");
                                        var newJson = JsonConvert.SerializeObject(updateConsumerOrders);
                                        using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
                                        {
                                            var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));
                                            response.EnsureSuccessStatusCode();
                                            string responseBody = response.Content.ReadAsStringAsync().Result;
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    TextFileLogHelper.LogError("Error while Update cart status in Trade: " + ex.ToString());
                                }
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, po);
                            #endregion
                        }
                        #endregion
                        var warehouses = db.Warehouses.FirstOrDefault(x => x.WarehouseId == odmMDataD.WarehouseId);

                        if (Oid > 0 && odmMDataD != null && isDispatched && odmMDataD.Status == "Delivery Canceled" && people != null && people.PeopleID > 0)
                        {
                            dispatchedobj = odmMDataD.orderDetails.Where(x => x.qty > 0).ToList();
                            List<FinalOrderDispatchedDetails> Finalobj = new List<FinalOrderDispatchedDetails>();
                            List<ReturnOrderDispatchedDetails> AddReturnOrderDispatchedDetails = new List<ReturnOrderDispatchedDetails>();
                            for (var i = 0; i < dispatchedobj.Count; i++)
                            {
                                int itemIDmaster = dispatchedobj[i].ItemId;
                                int WarehouseId = dispatchedobj[i].WarehouseId;
                                FinalOrderDispatchedDetails newfinal = new FinalOrderDispatchedDetails();
                                newfinal.OrderDispatchedDetailsId = dispatchedobj[i].OrderDispatchedDetailsId;
                                newfinal.OrderDetailsId = dispatchedobj[i].OrderDetailsId;
                                newfinal.OrderId = dispatchedobj[i].OrderId;
                                newfinal.OrderDispatchedMasterId = dispatchedobj[i].OrderDispatchedMasterId;
                                newfinal.CustomerId = dispatchedobj[i].CustomerId;
                                newfinal.CustomerName = dispatchedobj[i].CustomerName;
                                newfinal.City = dispatchedobj[i].City;
                                newfinal.Mobile = dispatchedobj[i].Mobile;
                                newfinal.OrderDate = dispatchedobj[i].OrderDate;
                                newfinal.CompanyId = dispatchedobj[i].CompanyId;
                                newfinal.CityId = dispatchedobj[i].CityId;
                                newfinal.WarehouseId = dispatchedobj[i].WarehouseId;
                                newfinal.WarehouseName = dispatchedobj[i].WarehouseName;
                                newfinal.CategoryName = dispatchedobj[i].CategoryName;

                                newfinal.ItemId = dispatchedobj[i].ItemId;
                                newfinal.Itempic = dispatchedobj[i].Itempic;
                                newfinal.itemname = dispatchedobj[i].itemname;
                                newfinal.SellingUnitName = dispatchedobj[i].SellingUnitName;
                                newfinal.itemcode = dispatchedobj[i].itemcode;
                                newfinal.Barcode = dispatchedobj[i].Barcode;
                                newfinal.UnitPrice = dispatchedobj[i].UnitPrice;
                                newfinal.Purchaseprice = dispatchedobj[i].Purchaseprice;
                                newfinal.MinOrderQty = dispatchedobj[i].MinOrderQty;
                                newfinal.MinOrderQtyPrice = dispatchedobj[i].MinOrderQtyPrice;
                                newfinal.qty = dispatchedobj[i].qty; //TempQty;// dispatchedobj[i].qty;
                                newfinal.price = dispatchedobj[i].price;
                                newfinal.MinOrderQty = dispatchedobj[i].MinOrderQty;
                                int MOQ = dispatchedobj[i].MinOrderQty;
                                newfinal.MinOrderQtyPrice = MOQ * dispatchedobj[i].UnitPrice;
                                newfinal.qty = Convert.ToInt32(dispatchedobj[i].qty);

                                int qty = 0;
                                qty = Convert.ToInt32(newfinal.qty);

                                newfinal.TaxPercentage = dispatchedobj[i].TaxPercentage;
                                //........CALCULATION FOR NEW SHOPKIRANA.............................
                                newfinal.Noqty = qty; // for total qty (no of items)

                                // STEP 1  (UNIT PRICE * QTY)     - SHOW PROPERTY                  
                                newfinal.TotalAmt = System.Math.Round(newfinal.UnitPrice * qty, 2);

                                // STEP 2 (AMOUT WITHOU TEX AND WITHOUT DISCOUNT ) - SHOW PROPERTY
                                newfinal.AmtWithoutTaxDisc = ((100 * newfinal.UnitPrice * qty) / (1 + newfinal.TaxPercentage / 100)) / 100;

                                // STEP 3 (AMOUNT WITHOUT TAX AFTER DISCOUNT) - UNSHOW PROPERTY
                                newfinal.AmtWithoutAfterTaxDisc = (100 * newfinal.AmtWithoutTaxDisc) / (100 + 0);

                                //STEP 4 (TAX AMOUNT) - UNSHOW PROPERTY
                                newfinal.TaxAmmount = (newfinal.AmtWithoutAfterTaxDisc * newfinal.TaxPercentage) / 100;

                                //STEP 5(TOTAL TAX AMOUNT) - UNSHOW PROPERTY
                                newfinal.TotalAmountAfterTaxDisc = newfinal.AmtWithoutAfterTaxDisc + newfinal.TaxAmmount;

                                //...............Calculate Discount.............................
                                newfinal.DiscountPercentage = 0;// items.PramotionalDiscount;
                                newfinal.DiscountAmmount = 0;
                                newfinal.NetAmtAfterDis = 0;
                                //...................................................................
                                newfinal.Purchaseprice = dispatchedobj[i].Purchaseprice;
                                //newfinal.VATTax = items.VATTax;
                                newfinal.CreatedDate = Convert.ToDateTime(dispatchedobj[i].CreatedDate);
                                newfinal.UpdatedDate = Convert.ToDateTime(dispatchedobj[i].CreatedDate);
                                newfinal.Deleted = false;
                                Finalobj.Add(newfinal);
                            }
                            db.FinalOrderDispatchedDetailsDb.AddRange(Finalobj);
                            om.Status = "Post Order Canceled";
                            #region Order Master History for Status Post Order Canceled  
                            OrderMasterHistories h1 = new OrderMasterHistories();
                            if (odmMDataD != null)
                            {
                                h1.orderid = odmMDataD.OrderId;
                                h1.Status = "Post Order Canceled";
                                h1.Reasoncancel = null;
                                h1.Warehousename = odmMDataD.WarehouseName;
                                h1.username = people.DisplayName;
                                h1.userid = userid;
                                h1.CreatedDate = indianTime;
                                db.OrderMasterHistoriesDB.Add(h1);
                            }
                            #endregion

                            foreach (ReturnOrderDispatchedDetails pc in po)
                            {


                                //FreeStock;
                                var FreeItem = om.orderDetails.Where(x => x.ItemId == pc.ItemId && x.IsDispatchedFreeStock && x.OrderId == pc.OrderId && x.OrderDetailsId == pc.OrderDetailsId).SingleOrDefault();
                                if (FreeItem != null)
                                {
                                    if (pc.qty > 0)
                                    {
                                        pc.IsDispatchedFreeStock = true;
                                        pc.IsFreeItem = true;
                                        AddReturnOrderDispatchedDetails.Add(pc);
                                    }
                                }
                                else
                                {

                                    if (pc.qty > 0)
                                    {
                                        pc.IsDispatchedFreeStock = false;
                                        pc.IsFreeItem = false;
                                        AddReturnOrderDispatchedDetails.Add(pc);
                                    }
                                }

                                var ord = om.orderDetails.Where(r => r.OrderDetailsId == pc.OrderDetailsId).SingleOrDefault();
                                ord.Status = "Post Order Canceled";
                                ord.UpdatedDate = indianTime;
                                db.Entry(ord).State = EntityState.Modified;
                            }
                            db.ReturnOrderDispatchedDetailsDb.AddRange(AddReturnOrderDispatchedDetails);

                            if (om.RewardPoint > 0)
                            {
                                var rpoint = db.RewardPointDb.Where(c => c.CustomerId == om.CustomerId).FirstOrDefault();
                                if (rpoint != null)
                                {
                                    if (om.RewardPoint > 0)
                                    {
                                        rpoint.EarningPoint -= om.RewardPoint;
                                        if (rpoint.EarningPoint < 0)
                                            rpoint.EarningPoint = 0;
                                        rpoint.UpdatedDate = indianTime;
                                        rpoint.TransactionDate = indianTime;

                                        db.Entry(rpoint).State = EntityState.Modified;

                                    }
                                }
                            }

                            if (om.Status == "Post Order Canceled")
                            {
                                if (om.WalletAmount > 0)
                                {
                                    var data123 = db.WalletDb.Where(x => x.CustomerId == om.CustomerId).SingleOrDefault();
                                    if (om.walletPointUsed > 0 && om.walletPointUsed != null)
                                    {
                                        data123.TotalAmount = data123.TotalAmount + om.walletPointUsed;

                                        db.Entry(data123).State = EntityState.Modified;
                                        CustomerWalletHistory CWH = new CustomerWalletHistory();
                                        //op by user
                                        CWH.PeopleId = people.PeopleID;
                                        CWH.PeopleName = people.DisplayName;
                                        CWH.OrderId = om.OrderId;
                                        //op by Cust
                                        CWH.WarehouseId = om.WarehouseId;
                                        CWH.CompanyId = 1;
                                        CWH.CustomerId = data123.CustomerId;
                                        CWH.NewAddedWAmount = om.walletPointUsed;
                                        CWH.TotalWalletAmount = data123.TotalAmount;
                                        CWH.Through = "From post order cancellation";
                                        CWH.CreatedDate = indianTime;
                                        CWH.UpdatedDate = indianTime;
                                        db.CustomerWalletHistoryDb.Add(CWH);
                                    }
                                }


                            }

                            om.Status = "Post Order Canceled";
                            om.UpdatedDate = indianTime;
                            db.Entry(om).State = EntityState.Modified;

                            odmMDataD.Status = "Post Order Canceled";
                            odmMDataD.UpdatedDate = indianTime;
                            db.Entry(odmMDataD).State = EntityState.Modified;
                            db.Commit();
                            #region stock Hit on poc

                            MultiStockHelper<POCStockEntryDc> MultiStockHelpers = new MultiStockHelper<POCStockEntryDc>();
                            List<POCStockEntryDc> POCStockList = new List<POCStockEntryDc>();
                            foreach (var StockHit in po.Where(x => x.qty > 0))
                            {
                                var RefStockCode = om.OrderType == 8 ? "CL" : "C";
                                bool isFree = om.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                if (isFree) { RefStockCode = "F"; }
                                else if (om.OrderType == 6) //6 Damage stock
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
                                bool resFreeitems = MultiStockHelpers.MakeEntry(POCStockList, "Stock_OnPOC_New", db, dbContextTransaction);
                                if (!resFreeitems)
                                {
                                    result = false;
                                    dbContextTransaction.Dispose();
                                    return Request.CreateResponse(HttpStatusCode.OK, po);

                                }
                                else
                                {
                                    result = true;
                                }
                            }
                            //Not Create Poc Note for Test Warehouse
                            string PocinvoiceNumber = " ";
                            if (odmMDataD.WarehouseId != 67 && odmMDataD.WarehouseId != 80)
                            {
                                PocinvoiceNumber = db.Database.SqlQuery<string>("EXEC spGetPocCNCurrentNumber 'PocCreditNote', " + warehouses.Stateid).FirstOrDefault();
                            }

                            //odmMDataD.PocCreditNoteNumber = odmMDataD.invoice_no + '/' + invoiceNumber;
                            odmMDataD.PocCreditNoteNumber = PocinvoiceNumber;

                            odmMDataD.PocCreditNoteDate = indianTime;
                            db.Entry(odmMDataD).State = EntityState.Modified;

                            if (om.paymentThrough.Trim().ToLower() == "paylater")
                            {
                                var paylaterdata = db.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == odmMDataD.OrderId);
                                if (paylaterdata != null)
                                {
                                    Model.CashManagement.PayLaterCollectionHistory history = new Model.CashManagement.PayLaterCollectionHistory();
                                    history.Amount = paylaterdata.Amount;
                                    history.PayLaterCollectionId = paylaterdata.Id;
                                    history.CreatedDate = DateTime.Now;
                                    history.RefNo = "";
                                    history.IsActive = true;
                                    history.IsDeleted = false;
                                    history.CurrencyHubStockId = 0;
                                    history.PaymentStatus = 1;
                                    history.PaymentMode = "";
                                    history.Comment = "Post Order Canceled";
                                    history.CreatedBy = people.PeopleID;
                                    db.PayLaterCollectionHistoryDb.Add(history);

                                    var param = new SqlParameter("@OrderId", paylaterdata.OrderId);
                                    AngularJSAuthentication.API.Controllers.OrderMasterrController.RefundPaylaterDc refund = db.Database.SqlQuery<AngularJSAuthentication.API.Controllers.OrderMasterrController.RefundPaylaterDc>("exec Sp_GetRefundDetail @OrderId", param).FirstOrDefault();
                                    if (refund != null && refund.RefundAmount > 0)
                                    {
                                        PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                        {
                                            amount = (-1) * (refund.RefundAmount),
                                            CreatedDate = DateTime.Now,
                                            currencyCode = "INR",
                                            OrderId = paylaterdata.OrderId,
                                            PaymentFrom = refund.PaymentMode,
                                            GatewayTransId = refund.RefNo,
                                            GatewayOrderId = Convert.ToString(paylaterdata.OrderId),
                                            status = "Success",
                                            UpdatedDate = DateTime.Now,
                                            IsRefund = false,
                                            IsOnline = true,
                                            statusDesc = "Refund Initiated"
                                        };
                                        db.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                        var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                        {
                                            Amount = (refund.RefundAmount),
                                            OrderId = PaymentResponseRetailerAppDb.OrderId,
                                            Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                            ReqGatewayTransId = PaymentResponseRetailerAppDb.GatewayTransId,
                                            Status = (int)PaymentRefundEnum.Initiated,
                                            CreatedBy = people.PeopleID,
                                            CreatedDate = DateTime.Now,
                                            IsActive = true,
                                            IsDeleted = false,
                                            ModifiedBy = people.PeopleID,
                                            ModifiedDate = DateTime.Now,
                                            PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                        };
                                        bool IsInserted = PRHelper.InsertPaymentRefundRequest(db, PaymentRefundRequestDc);
                                    }
                                    db.Commit();
                                }
                            }
                            else
                            {
                                var OnlineEntries = db.PaymentResponseRetailerAppDb.Where(z => z.OrderId == odmMDataD.OrderId && z.IsOnline && z.status == "Success").ToList();
                                if (warehouses.IsOnlineRefundEnabled)
                                {
                                    if (db.PaymentRefundApis.Any(x => x.IsActive == true) && OnlineEntries != null && OnlineEntries.Any(x => x.IsOnline == true))
                                    {
                                        #region Post order cancel payment refund  -- April2022
                                        //case 1 : online payment list
                                        var RefundDays = db.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();
                                        if (OnlineEntries != null && OnlineEntries.Any() && OnlineEntries.Sum(x => x.amount) > 0)
                                        {
                                            PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                            foreach (var item in OnlineEntries.GroupBy(c => c.GatewayTransId))
                                            {
                                                int RefundType = (int)RefundTypeEnum.Auto;
                                                var PaymentRefundDays = RefundDays.FirstOrDefault(x => x.ApiName.Trim().ToLower() == item.FirstOrDefault().PaymentFrom.Trim().ToLower());
                                                if (PaymentRefundDays != null && PaymentRefundDays.DaysForRefundEligible > 0 && item.FirstOrDefault().CreatedDate.AddDays(PaymentRefundDays.DaysForRefundEligible) < DateTime.Now)
                                                {
                                                    RefundType = (int)RefundTypeEnum.Manual;
                                                }
                                                else if (PaymentRefundDays == null && item.FirstOrDefault().PaymentFrom.Trim().ToLower() != "gullak")
                                                {
                                                    RefundType = (int)RefundTypeEnum.Manual;
                                                }
                                                double RefundAmount = item.Sum(x => x.amount);
                                                if (RefundAmount > 0)
                                                {
                                                    var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                                    {
                                                        amount = (-1) * RefundAmount,
                                                        CreatedDate = DateTime.Now,
                                                        currencyCode = "INR",
                                                        OrderId = odmMDataD.OrderId,
                                                        PaymentFrom = item.FirstOrDefault().PaymentFrom,
                                                        GatewayTransId = item.FirstOrDefault().GatewayTransId,
                                                        GatewayOrderId = item.FirstOrDefault().GatewayOrderId,
                                                        status = "Success",
                                                        UpdatedDate = DateTime.Now,
                                                        IsRefund = false,
                                                        IsOnline = true,
                                                        statusDesc = "Refund Initiated"
                                                    };
                                                    db.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                                    db.Commit();
                                                    // addd Refund request
                                                    var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                                    {
                                                        Amount = RefundAmount,
                                                        OrderId = PaymentResponseRetailerAppDb.OrderId,
                                                        Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                                        Status = (int)PaymentRefundEnum.Initiated,
                                                        ReqGatewayTransId = item.FirstOrDefault().GatewayTransId,
                                                        CreatedBy = people.PeopleID,
                                                        CreatedDate = DateTime.Now,
                                                        IsActive = true,
                                                        IsDeleted = false,
                                                        ModifiedBy = people.PeopleID,
                                                        ModifiedDate = DateTime.Now,
                                                        RefundType = RefundType,
                                                        PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                                    };
                                                    bool IsInserted = PRHelper.InsertPaymentRefundRequest(db, PaymentRefundRequestDc);
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                                else
                                {
                                    #region Gullak Return

                                    var cashOldEntries = db.PaymentResponseRetailerAppDb.Where(z => z.OrderId == om.OrderId && z.PaymentFrom == "Gullak"
                                                                  && z.status == "Success").ToList();
                                    double totalAmount = 0;
                                    if (cashOldEntries != null && cashOldEntries.Any())
                                    {
                                        totalAmount = cashOldEntries.Sum(x => x.amount);
                                        foreach (var cash in cashOldEntries)
                                        {
                                            cash.statusDesc = "Due to Post order cancel Gullak amount refunded.";
                                            db.Entry(cash).State = EntityState.Modified;
                                        }
                                    }

                                    if (totalAmount > 0)
                                    {
                                        var customerGullak = db.GullakDB.FirstOrDefault(x => x.CustomerId == om.CustomerId);
                                        if (customerGullak != null)
                                        {
                                            db.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
                                            {
                                                CreatedDate = indianTime,
                                                CreatedBy = om.CustomerId,
                                                Comment = "Order cancel : " + om.OrderId.ToString(),
                                                Amount = totalAmount,
                                                GullakId = customerGullak.Id,
                                                CustomerId = om.CustomerId,
                                                IsActive = true,
                                                IsDeleted = false,
                                                ObjectId = om.OrderId.ToString(),
                                                ObjectType = "Order"
                                            });

                                            customerGullak.TotalAmount += totalAmount;
                                            customerGullak.ModifiedBy = customerGullak.CustomerId;
                                            customerGullak.ModifiedDate = indianTime;
                                            db.Entry(customerGullak).State = EntityState.Modified;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            

                            db.Commit();

                            #region Update IRN Check 
                            IRNHelper irnHelper = new IRNHelper();

                            if (irnHelper.IsGenerateIRN(db, odmMDataD.CustomerId))
                            {
                                odmMDataD.IsGenerateCN = true;
                                ClearTaxIntegration clearTaxIntegration = new ClearTaxIntegration
                                {
                                    APIType = "GenerateCN",
                                    CreateDate = indianTime,
                                    OrderId = odmMDataD.OrderId,
                                    IsActive = true,
                                };

                                db.ClearTaxIntegrations.Add(clearTaxIntegration);
                                db.Entry(odmMDataD).State = EntityState.Modified;
                                db.Commit();
                            }

                            #endregion

                            #endregion
                            if (odmMDataD != null)
                            {
                                CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
                                helper.OnCancel(odmMDataD.OrderId, userid, db, om.CustomerId);
                                if (odmMDataD.TotalAmount >= 50000 && odmMDataD.EwayBillNumber != null)
                                {
                                    bool isemailed = SendMailForEWAYBillDeactivate(odmMDataD.OrderId, odmMDataD.TotalAmount);
                                }
                            }
                            //// neha refund payment 
                            //if (payments != null && payments.Any())
                            //{
                            //    RefundAmount(Oid, payments);
                            //}
                        }
                        if (result)
                        {

                            dbContextTransaction.Complete();
                            #region Insert in FIFO

                            if (ConfigurationManager.AppSettings["LiveFIFO"] == "1")
                            {
                                List<GrDC> items = odmMDataD.orderDetails.Where(x => x.qty > 0).Select(x => new GrDC
                                {
                                    ItemMultiMrpId = x.ItemMultiMRPId,
                                    WarehouseId = odmMDataD.WarehouseId,
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
                        }
                        else
                        {
                            dbContextTransaction.Dispose();
                        }
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, po);
        }

        #region old code



        //[ResponseType(typeof(ReturnOrderDispatchedDetails))]
        //[Route("")]
        //[AcceptVerbs("POST")]
        //public List<ReturnOrderDispatchedDetails> add(List<ReturnOrderDispatchedDetails> po)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 1, userid = 0;
        //            // Access claims
        //            foreach (Claim claim in identity.Claims)
        //            {
        //                if (claim.Type == "compid")
        //                {
        //                    compid = int.Parse(claim.Value);
        //                }
        //                if (claim.Type == "userid")
        //                {
        //                    userid = int.Parse(claim.Value);
        //                }
        //            }
        //            Int32 Oid = po[0].OrderId;
        //            var people = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
        //            bool isDispatched = true;
        //            var odmMDataD = db.OrderDispatchedMasters.Where(x => x.OrderId == Oid && x.Status == "Delivery Canceled" && x.Deleted == false).FirstOrDefault();

        //            if (odmMDataD != null && odmMDataD.DeliveryIssuanceIdOrderDeliveryMaster.HasValue && odmMDataD.DeliveryIssuanceIdOrderDeliveryMaster.Value > 0)
        //            {
        //                DeliveryIssuance DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == odmMDataD.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault();

        //                if (DeliveryIssuance.Status == "Payment Accepted" || DeliveryIssuance.Status == "Submitted" || DeliveryIssuance.Status == "Pending")
        //                {
        //                    odmMDataD.DeliveryIssuanceStatus = DeliveryIssuance.Status;
        //                    isDispatched = false;
        //                    return null;
        //                }
        //            }
        //            if (Oid > 0 && odmMDataD != null && isDispatched)
        //            {
        //                OrderMaster om = new OrderMaster();
        //                List<int> ItemIds = new List<int>();


        //                om = db.DbOrderMaster.Where(x => x.OrderId == odmMDataD.OrderId && x.Deleted == false && x.CompanyId == compid).Include("orderDetails").FirstOrDefault();
        //                om.Status = "Post Order Canceled";
        //                om.UpdatedDate = indianTime;
        //                db.Entry(om).State = EntityState.Modified;

        //                OrderDispatchedMaster odm = db.OrderDispatchedMasters.Where(x => x.OrderId == odmMDataD.OrderId && x.Deleted == false && x.CompanyId == compid).FirstOrDefault();
        //                odm.Status = "Post Order Canceled";
        //                odm.UpdatedDate = indianTime;
        //                db.Entry(odm).State = EntityState.Modified;

        //                #region Order Master History for Status Post Order Canceled  

        //                OrderMasterHistories h1 = new OrderMasterHistories();

        //                h1.orderid = odm.OrderId;
        //                h1.Status = odm.Status;
        //                h1.Reasoncancel = null;
        //                h1.Warehousename = odm.WarehouseName;
        //                h1.username = people.DisplayName;
        //                h1.userid = userid;
        //                h1.CreatedDate = indianTime;
        //                db.OrderMasterHistoriesDB.Add(h1);

        //                #endregion

        //                if (om.OrderType == 5)
        //                {
        //                    if (db.Commit() > 0)
        //                    {
        //                        try
        //                        {
        //                            UpdateConsumerOrders updateConsumerOrders = new UpdateConsumerOrders
        //                            {
        //                                CartStatus = "Cancelled",
        //                                InvoiceNo = om.invoice_no
        //                            };                                   
        //                            var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/ShoppingCart/SKUpdateCartStatus";                                   
        //                            using (var client = new HttpClient())
        //                            {
        //                                client.DefaultRequestHeaders.Add("CustomerId", "1");
        //                                client.DefaultRequestHeaders.Add("NoEncryption", "1");
        //                                var newJson = JsonConvert.SerializeObject(updateConsumerOrders);
        //                                using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
        //                                {
        //                                    var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));                                           
        //                                    response.EnsureSuccessStatusCode();
        //                                    string responseBody = response.Content.ReadAsStringAsync().Result;                                            
        //                                }
        //                            }

        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            TextFileLogHelper.LogError("Error while Update cart status in Trade: " + ex.ToString());
        //                        }
        //                    }
        //                    return po;
        //                }

        //                List<OrderDispatchedDetails> dispatchedobj = new List<OrderDispatchedDetails>();
        //                dispatchedobj = db.OrderDispatchedDetailss.Where(x => x.OrderId == Oid && x.CompanyId == compid).ToList();
        //                List<FinalOrderDispatchedDetails> Finalobj = new List<FinalOrderDispatchedDetails>();
        //                for (var i = 0; i < dispatchedobj.Count; i++)
        //                {
        //                    //for (var k = 0; k < po.Count; k++)
        //                    //{
        //                    // if (po[k].isDeleted != true && dispatchedobj[i].ItemId == po[k].ItemId && dispatchedobj[i].MinOrderQty == po[k].MinOrderQty)
        //                    // {
        //                    try
        //                    {
        //                        // dispatchedobj[i].qty = dispatchedobj[i].qty - po[k].qty;
        //                        //var TempQty = dispatchedobj[i].qty - po[k].qty;
        //                        Int64 itemIDmaster = dispatchedobj[i].ItemId;
        //                        int WarehouseId = dispatchedobj[i].WarehouseId;
        //                        ItemMaster items = db.itemMasters.Where(x => x.ItemId == itemIDmaster && x.CompanyId == compid && x.WarehouseId == WarehouseId).FirstOrDefault();
        //                        FinalOrderDispatchedDetails newfinal = new FinalOrderDispatchedDetails();
        //                        newfinal.OrderDispatchedDetailsId = dispatchedobj[i].OrderDispatchedDetailsId;
        //                        newfinal.OrderDetailsId = dispatchedobj[i].OrderDetailsId;
        //                        newfinal.OrderId = dispatchedobj[i].OrderId;
        //                        newfinal.OrderDispatchedMasterId = dispatchedobj[i].OrderDispatchedMasterId;
        //                        newfinal.CustomerId = dispatchedobj[i].CustomerId;
        //                        newfinal.CustomerName = dispatchedobj[i].CustomerName;
        //                        newfinal.City = dispatchedobj[i].City;
        //                        newfinal.Mobile = dispatchedobj[i].Mobile;
        //                        newfinal.OrderDate = dispatchedobj[i].OrderDate;
        //                        newfinal.CompanyId = dispatchedobj[i].CompanyId;
        //                        newfinal.CityId = dispatchedobj[i].CityId;
        //                        newfinal.WarehouseId = dispatchedobj[i].WarehouseId;
        //                        newfinal.WarehouseName = dispatchedobj[i].WarehouseName;
        //                        newfinal.CategoryName = dispatchedobj[i].CategoryName;

        //                        newfinal.ItemId = dispatchedobj[i].ItemId;
        //                        newfinal.Itempic = dispatchedobj[i].Itempic;
        //                        newfinal.itemname = dispatchedobj[i].itemname;
        //                        newfinal.SellingUnitName = dispatchedobj[i].SellingUnitName;
        //                        newfinal.itemcode = dispatchedobj[i].itemcode;
        //                        newfinal.Barcode = dispatchedobj[i].Barcode;
        //                        newfinal.UnitPrice = dispatchedobj[i].UnitPrice;
        //                        newfinal.Purchaseprice = dispatchedobj[i].Purchaseprice;
        //                        newfinal.MinOrderQty = dispatchedobj[i].MinOrderQty;
        //                        newfinal.MinOrderQtyPrice = dispatchedobj[i].MinOrderQtyPrice;
        //                        newfinal.qty = dispatchedobj[i].qty; //TempQty;// dispatchedobj[i].qty;
        //                        newfinal.price = dispatchedobj[i].price;
        //                        newfinal.MinOrderQty = dispatchedobj[i].MinOrderQty;
        //                        int MOQ = dispatchedobj[i].MinOrderQty;
        //                        newfinal.MinOrderQtyPrice = MOQ * dispatchedobj[i].UnitPrice;
        //                        newfinal.qty = Convert.ToInt32(dispatchedobj[i].qty);

        //                        int qty = 0;
        //                        qty = Convert.ToInt32(newfinal.qty);

        //                        newfinal.TaxPercentage = items.TotalTaxPercentage;
        //                        //........CALCULATION FOR NEW SHOPKIRANA.............................
        //                        newfinal.Noqty = qty; // for total qty (no of items)

        //                        // STEP 1  (UNIT PRICE * QTY)     - SHOW PROPERTY                  
        //                        newfinal.TotalAmt = System.Math.Round(newfinal.UnitPrice * qty, 2);

        //                        // STEP 2 (AMOUT WITHOU TEX AND WITHOUT DISCOUNT ) - SHOW PROPERTY
        //                        newfinal.AmtWithoutTaxDisc = ((100 * newfinal.UnitPrice * qty) / (1 + newfinal.TaxPercentage / 100)) / 100;

        //                        // STEP 3 (AMOUNT WITHOUT TAX AFTER DISCOUNT) - UNSHOW PROPERTY
        //                        newfinal.AmtWithoutAfterTaxDisc = (100 * newfinal.AmtWithoutTaxDisc) / (100 + items.PramotionalDiscount);

        //                        //STEP 4 (TAX AMOUNT) - UNSHOW PROPERTY
        //                        newfinal.TaxAmmount = (newfinal.AmtWithoutAfterTaxDisc * newfinal.TaxPercentage) / 100;

        //                        //STEP 5(TOTAL TAX AMOUNT) - UNSHOW PROPERTY
        //                        newfinal.TotalAmountAfterTaxDisc = newfinal.AmtWithoutAfterTaxDisc + newfinal.TaxAmmount;

        //                        //...............Calculate Discount.............................
        //                        newfinal.DiscountPercentage = items.PramotionalDiscount;
        //                        newfinal.DiscountAmmount = 0;
        //                        newfinal.NetAmtAfterDis = 0;
        //                        //...................................................................
        //                        newfinal.Purchaseprice = items.price;
        //                        //newfinal.VATTax = items.VATTax;
        //                        newfinal.CreatedDate = Convert.ToDateTime(dispatchedobj[i].CreatedDate);
        //                        newfinal.UpdatedDate = Convert.ToDateTime(dispatchedobj[i].CreatedDate);
        //                        newfinal.Deleted = false;
        //                        Finalobj.Add(newfinal);

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                    }
        //                    // }
        //                    //}
        //                }
        //                foreach (FinalOrderDispatchedDetails x1 in Finalobj)
        //                {

        //                    db.FinalOrderDispatchedDetailsDb.Add(x1);
        //                    int id = db.Commit();
        //                }

        //                foreach (ReturnOrderDispatchedDetails pc in po)
        //                {


        //                    //FreeStock;
        //                    var FreeItem = om.orderDetails.Where(x => x.ItemId == pc.ItemId && x.IsDispatchedFreeStock && x.OrderId == pc.OrderId && x.OrderDetailsId == pc.OrderDetailsId).SingleOrDefault();
        //                    if (FreeItem != null)
        //                    {
        //                        FreeStock fStock = db.FreeStockDB.Where(x => x.ItemNumber == FreeItem.itemNumber && x.WarehouseId == FreeItem.WarehouseId && x.ItemMultiMRPId == FreeItem.ItemMultiMRPId).FirstOrDefault(); ;
        //                        if (fStock != null && pc.qty > 0)
        //                        {
        //                            fStock.CurrentInventory = fStock.CurrentInventory + pc.qty;
        //                            fStock.UpdatedDate = indianTime;
        //                            fStock.CreatedBy = people.DisplayName;
        //                            db.Entry(fStock).State = EntityState.Modified;

        //                            FreeStockHistory itemHistory = new FreeStockHistory();
        //                            itemHistory.FreeStockId = fStock.FreeStockId;
        //                            itemHistory.WarehouseId = fStock.WarehouseId;
        //                            itemHistory.ItemNumber = fStock.ItemNumber;
        //                            itemHistory.OdOrPoId = Oid;
        //                            itemHistory.ManualReason = "From Delivery Canceled";
        //                            itemHistory.ItemMultiMRPId = fStock.ItemMultiMRPId;
        //                            itemHistory.CurrentInventory = fStock.CurrentInventory;
        //                            itemHistory.OrderCancelInventoryIn = Convert.ToInt32(pc.qty);
        //                            itemHistory.TotalInventory = Convert.ToInt32(fStock.CurrentInventory);
        //                            itemHistory.itemname = fStock.itemname;
        //                            itemHistory.MRP = fStock.MRP;
        //                            itemHistory.userid = people.PeopleID;
        //                            itemHistory.CreationDate = indianTime;
        //                            db.FreeStockHistoryDB.Add(itemHistory);
        //                            db.Commit();

        //                            var ord = db.DbOrderDetails.Where(r => r.OrderDetailsId == pc.OrderDetailsId).SingleOrDefault();
        //                            ord.Status = "Post Order Canceled";
        //                            ord.UpdatedDate = indianTime;
        //                            db.Entry(ord).State = EntityState.Modified;
        //                            db.Commit();

        //                            db.AddReturnOrderDispatchedDetails(pc);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ////
        //                        ItemMaster master = db.itemMasters.Where(c => c.ItemId == pc.ItemId && c.CompanyId == compid).SingleOrDefault();
        //                        CurrentStock itemm = db.DbCurrentStock.FirstOrDefault(x => x.ItemNumber == master.Number && x.WarehouseId == pc.WarehouseId && x.ItemMultiMRPId == pc.ItemMultiMRPId);
        //                        if (itemm != null && pc.qty > 0)
        //                        {
        //                            CurrentStockHistory Oss = new CurrentStockHistory();
        //                            if (itemm != null)
        //                            {
        //                                Oss.StockId = itemm.StockId;
        //                                Oss.ItemNumber = itemm.ItemNumber;
        //                                Oss.itemname = itemm.itemname;
        //                                Oss.ItemMultiMRPId = pc.ItemMultiMRPId;
        //                                Oss.OdOrPoId = pc.OrderId;
        //                                Oss.CurrentInventory = itemm.CurrentInventory;
        //                                Oss.OrderCancelInventoryIn = Convert.ToInt32(pc.qty);
        //                                Oss.TotalInventory = Convert.ToInt32(itemm.CurrentInventory + pc.qty);
        //                                Oss.WarehouseName = itemm.WarehouseName;
        //                                Oss.Warehouseid = itemm.WarehouseId;
        //                                Oss.CompanyId = itemm.CompanyId;
        //                                Oss.userid = people.PeopleID;
        //                                Oss.UserName = people.DisplayName;
        //                                Oss.CreationDate = DateTime.Now;
        //                                db.CurrentStockHistoryDb.Add(Oss);
        //                                int id = db.Commit();
        //                            }

        //                            itemm.CurrentInventory = Convert.ToInt32(itemm.CurrentInventory + (pc.qty));
        //                            UpdateCurrentStockInOffer(itemm);

        //                        }
        //                        var ord = db.DbOrderDetails.Where(r => r.OrderDetailsId == pc.OrderDetailsId).SingleOrDefault();
        //                        ord.Status = "Post Order Canceled";
        //                        ord.UpdatedDate = indianTime;
        //                        db.Entry(ord).State = EntityState.Modified;
        //                        db.Commit();

        //                        db.AddReturnOrderDispatchedDetails(pc);

        //                    }
        //                }


        //                om = db.DbOrderMaster.Where(x => x.OrderId == Oid && x.Deleted == false && x.CompanyId == compid).FirstOrDefault();
        //                var rpoint = db.RewardPointDb.Where(c => c.CustomerId == om.CustomerId).FirstOrDefault();
        //                if (rpoint != null)
        //                {
        //                    if (om.RewardPoint > 0)
        //                    {
        //                        rpoint.EarningPoint -= om.RewardPoint;
        //                        if (rpoint.EarningPoint < 0)
        //                            rpoint.EarningPoint = 0;
        //                        rpoint.UpdatedDate = indianTime;
        //                        rpoint.TransactionDate = indianTime;
        //                        db.RewardPointDb.Attach(rpoint);
        //                        db.Entry(rpoint).State = EntityState.Modified;
        //                        db.Commit();
        //                    }
        //                }
        //                //
        //                // on pre order canceeled 
        //                if (om.Status == "Post Order Canceled")
        //                {
        //                    var data123 = db.WalletDb.Where(x => x.CustomerId == om.CustomerId).SingleOrDefault();
        //                    if (om.walletPointUsed > 0 && om.walletPointUsed != null)
        //                    {
        //                        data123.TotalAmount = data123.TotalAmount + om.walletPointUsed;
        //                        db.WalletDb.Attach(data123);
        //                        db.Entry(data123).State = EntityState.Modified;
        //                        db.Commit();
        //                        CustomerWalletHistory CWH = new CustomerWalletHistory();
        //                        //op by user

        //                        CWH.PeopleId = people.PeopleID;
        //                        CWH.PeopleName = people.DisplayName;
        //                        CWH.OrderId = om.OrderId;
        //                        //op by Cust
        //                        CWH.WarehouseId = om.WarehouseId;
        //                        CWH.CompanyId = 1;
        //                        CWH.CustomerId = data123.CustomerId;
        //                        CWH.NewAddedWAmount = om.walletPointUsed;
        //                        CWH.TotalWalletAmount = data123.TotalAmount;
        //                        CWH.Through = "From post order cancellation";
        //                        CWH.CreatedDate = indianTime;
        //                        CWH.UpdatedDate = indianTime;
        //                        db.CustomerWalletHistoryDb.Add(CWH);
        //                        db.Commit();
        //                        BackgroundTaskManager.Run(() => ForNotificationpostordercancellation(om, db));
        //                    }


        //                    #region Gullak Return

        //                    var cashOldEntries = db.PaymentResponseRetailerAppDb.Where(z => z.OrderId == om.OrderId && z.PaymentFrom == "Gullak"
        //                                                  && z.status == "Success").ToList();
        //                    double totalAmount = 0;
        //                    if (cashOldEntries != null && cashOldEntries.Any())
        //                    {
        //                        totalAmount = cashOldEntries.Sum(x => x.amount);
        //                        foreach (var cash in cashOldEntries)
        //                        {
        //                            cash.statusDesc = "Due to Post order cancel Gullak amount refunded.";
        //                            db.Entry(cash).State = EntityState.Modified;
        //                        }
        //                    }

        //                    if (totalAmount > 0)
        //                    {
        //                        var customerGullak = db.GullakDB.FirstOrDefault(x => x.CustomerId == om.CustomerId);
        //                        if (customerGullak != null)
        //                        {
        //                            db.GullakTransactionDB.Add(new Model.Gullak.GullakTransaction
        //                            {
        //                                CreatedDate = indianTime,
        //                                CreatedBy = om.CustomerId,
        //                                Comment = "Due to Post order cancel",
        //                                Amount = totalAmount,
        //                                GullakId = customerGullak.Id,
        //                                CustomerId = om.CustomerId,
        //                                IsActive = true,
        //                                IsDeleted = false,
        //                                ObjectId = om.OrderId.ToString(),
        //                                ObjectType = "Order"
        //                            });

        //                            customerGullak.TotalAmount += totalAmount;
        //                            customerGullak.ModifiedBy = customerGullak.CustomerId;
        //                            customerGullak.ModifiedDate = indianTime;
        //                            db.Entry(customerGullak).State = EntityState.Modified;
        //                        }
        //                    }
        //                    #endregion

        //                }




        //                if (odmMDataD != null)
        //                {
        //                    CustomerLedgerHelperAuto helper = new CustomerLedgerHelperAuto();
        //                    helper.OnCancel(odmMDataD.OrderId, userid, db, om.CustomerId);
        //                    if (odmMDataD.TotalAmount >= 50000)
        //                    {
        //                        SendMailForEWAYBillDeactivate(odmMDataD.OrderId, odmMDataD.TotalAmount);
        //                    }

        //                }

        //                // neha refund payment 
        //                try
        //                {
        //                    var payments = db.PaymentResponseRetailerAppDb.Where(x => x.OrderId == odmMDataD.OrderId && x.status == "Success" && (x.PaymentFrom == "ePaylater" || x.PaymentFrom == "hdfc")).ToList();
        //                    if (payments != null && payments.Any())
        //                    {
        //                        RefundAmount(Oid, payments);
        //                    }
        //                }
        //                catch (Exception ee)
        //                {
        //                    logger.Error("Error in refund Amount for Order:  " + Oid + Environment.NewLine + ee.ToString());
        //                }


        //                return po;
        //            }

        //            else
        //            {
        //                return null;
        //            }
        //        }
        //        catch (Exception exe)
        //        {
        //            return null;
        //        }

        //    }
        //}

        #endregion
        private bool RefundAmount(int orderid, List<PaymentResponseRetailerApp> payments)
        {
            using (var db = new AuthContext())
            {
                List<PaymentResponseRetailerApp> refunds = new List<PaymentResponseRetailerApp>();
                var companyDetail = db.CompanyDetailsDB.FirstOrDefault(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                var url = "transaction/v2/marketplaceorderid/[[GatewayOrderId]]/returned";
                foreach (var item in payments)
                {
                    var response = new EpayLaterRefundResponseDc();
                    EpayLaterRefundRequestDc request = new EpayLaterRefundRequestDc();

                    if (item.PaymentFrom == "ePaylater")
                    {
                        url = url.Replace("[[GatewayOrderId]]", item.GatewayOrderId);
                        request = new EpayLaterRefundRequestDc
                        {
                            marketplaceOrderId = item.GatewayOrderId,
                            refundDate = DateTime.Now,
                            returnAcceptedDate = DateTime.Now,
                            returnAmount = item.amount * 100,
                            returnShipmentReceivedDate = DateTime.Now,
                            returnType = "full"

                        };

                        List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>> { new KeyValuePair<string, IEnumerable<string>>
                        (
                          "Authorization",
                          new List<string> { companyDetail.BEARER_TOKEN }
                        )};


                        using (GenericRestHttpClient<EpayLaterRefundRequestDc, string> memberClient
                        = new GenericRestHttpClient<EpayLaterRefundRequestDc, string>(companyDetail.ePayLaterEndpoint,
                        url, extraDataAsHeader))
                        {

                            response = AsyncContext.Run(() => memberClient.PutAsync<EpayLaterRefundResponseDc>(request));

                        }
                    }


                    PaymentResponseRetailerApp RefundHistory = new PaymentResponseRetailerApp();

                    RefundHistory.GatewayOrderId = item.GatewayOrderId;
                    RefundHistory.amount = item.amount;
                    RefundHistory.GatewayTransId = item.GatewayTransId;
                    RefundHistory.GatewayRequest = item.PaymentFrom == "ePaylater" ? JsonConvert.SerializeObject(request) : null;
                    RefundHistory.GatewayResponse = item.PaymentFrom == "ePaylater" ? JsonConvert.SerializeObject(response) : null;
                    RefundHistory.PaymentFrom = item.PaymentFrom;
                    RefundHistory.status = item.status;
                    RefundHistory.OrderId = item.OrderId;
                    RefundHistory.CreatedDate = DateTime.Now;
                    RefundHistory.UpdatedDate = DateTime.Now;
                    RefundHistory.IsRefund = true;

                    refunds.Add(RefundHistory);


                }

                db.PaymentResponseRetailerAppDb.AddRange(refunds);
                db.Commit();

                return true;

            }
        }


        //private void UpdateCurrentStockInOffer(CurrentStock itemm)
        //{

        //    itemm.UpdatedDate = indianTime;
        //    //cc.DbCurrentStock.Attach(itemm);
        //    cc.Entry(itemm).State = EntityState.Modified;
        //    cc.Commit();
        //}

        private async Task<bool> ForNotificationpostordercancellation(OrderMaster om, AuthContext db)
        {
            bool Result = false;
            Notification notification = new Notification();
            notification.title = "Your OrderId  " + om.OrderId + " is Delivery Cancelled";
            notification.Message = om.walletPointUsed + " Point is added to your Wallet";
            notification.Pic = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-image-128.png";
            var customers = db.Customers.Where(x => x.fcmId != null && x.CustomerId == om.walletPointUsed).SingleOrDefault();
            //AddNotification(notification);
            string Key = ConfigurationManager.AppSettings["FcmApiKey"];
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
            //            }
            //        }
            //    }
            //}
            var data = new FCMData
            {
                title = notification.title,
                body = notification.Message,
                icon = notification.Pic
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
            return Result;
        }

        [Authorize]
        [Route("")]
        public IEnumerable<ReturnOrderDispatchedDetails> GetallReturndispatchDetailbyId(string id)
        {
            logger.Info("start : ");
            List<ReturnOrderDispatchedDetails> ass = new List<ReturnOrderDispatchedDetails>();
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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    int idd = Int32.Parse(id);
                    ass = context.AllReturnOrderDispatchedDetails(idd, compid).ToList();
                    logger.Info("End  : ");
                    return ass;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in returnorderby id " + ex.Message);
                    logger.Info("End  returnorderby id: ");
                    return null;
                }
            }

        }
        public bool SendMailForEWAYBillDeactivate(int orderId, double TotalAmount)
        {

            string masteremail = ConfigurationManager.AppSettings["MasterEmail"];
            string masterpassword = ConfigurationManager.AppSettings["MasterPassword"];


            if (masteremail != null && masterpassword != null)
            {
                string body = "<div style='background: #FAFAFA; color: #333333; padding-left: 30px;font-family: arial,sans-serif; font-size: 14px;'>";
                body += "<img style='padding-top: 10px;' src='http://shopkirana.com/wp-content/uploads/2015/07/ShopKirana-Logo11.png'><br/>";
                body += "<h3 style='background-color: rgb(241, 89, 34);'>Deactivate EWAY Bill</h3>";
                body += "Hello,";
                body += "<p><strong>";
                body += "</strong>" + " With Reference To" + orderId + "</p>";
                body += "<p>Of Amount : <strong>" + TotalAmount + " </strong> <br/>Please Deactivate EWAYBill For The Same. </p>";
                body += "Thanks,";
                body += "<br />";
                body += "<b></b>";
                body += "</div>";

                var Subj = "Alert! Deactivate EWAY Bill For " + orderId;
                var msg = new MailMessage("donotreply_backend@shopkirana.com", "donotreply_backend@shopkirana.com", Subj, body);
                msg.To.Add("vijaykabra.accounts@shopkirana.com");
                msg.To.Add("shweta.soni@shopkirana.com");
                //msg.To.Add("ravikant.dhamne@shopkirana.com");
                msg.IsBodyHtml = true;
                var smtpClient = new SmtpClient("smtp.gmail.com", 587); //if your from email address is "from@hotmail.com" then host should be "smtp.hotmail.com"
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new NetworkCredential(masteremail, masterpassword);
                smtpClient.EnableSsl = true;
                smtpClient.Send(msg);
                return true;
            }
            else
            {
                return false;
            }




        }
    }

    public class UpdateConsumerOrders
    {
        public string CartStatus { get; set; }
        public string InvoiceNo { get; set; }
    }
}
