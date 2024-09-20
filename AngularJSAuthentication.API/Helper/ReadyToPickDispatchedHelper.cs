using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.API.Helper.PaymentRefund;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.OrderProcess;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.ClearTax;
using BarcodeLib;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
//using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using AngularJSAuthentication.DataContracts.Masters.PaymentRefund;
using AngularJSAuthentication.DataContracts.Transaction.Ledger.ItemLedger;
using System.Configuration;
using AngularJSAuthentication.BatchManager.Publishers;
using AngularJSAuthentication.DataContracts.BatchCode;
using Nito.AsyncEx;


namespace AngularJSAuthentication.API.Helper
{
    public class ReadyToPickDispatchedHelper
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        #region  for new assignment 
        public bool CreateAssignment(AuthContext context, People user, TransactionScope scope, DeliveryIssuance obj, List<OrderDispatchedMaster> AssOrderDispatchedMaster, List<OrderMaster> ordermasterlist)//add issuance
        {
            bool result = false;
            MultiStockHelper<OnIssuedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnIssuedStockEntryDc>();
            List<OnIssuedStockEntryDc> OnIssuedStockEntryList = new List<OnIssuedStockEntryDc>();
            #region Assignemnt  Delivery History
            OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
            AssginDeli.DeliveryIssuanceId = obj.DeliveryIssuanceId;
            AssginDeli.Cityid = obj.Cityid;
            AssginDeli.city = obj.city;
            AssginDeli.DisplayName = obj.DisplayName;
            AssginDeli.Status = obj.Status;
            AssginDeli.WarehouseId = obj.WarehouseId;
            AssginDeli.PeopleID = obj.PeopleID;
            AssginDeli.VehicleId = obj.VehicleId;
            AssginDeli.VehicleNumber = obj.VehicleNumber;
            AssginDeli.RejectReason = obj.RejectReason;
            AssginDeli.OrderdispatchIds = obj.OrderdispatchIds;
            AssginDeli.OrderIds = obj.OrderIds;
            AssginDeli.Acceptance = obj.Acceptance;
            AssginDeli.IsActive = obj.IsActive;
            AssginDeli.IdealTime = obj.IdealTime;
            AssginDeli.TravelDistance = obj.TravelDistance;
            AssginDeli.CreatedDate = indianTime;
            AssginDeli.UpdatedDate = indianTime;
            AssginDeli.userid = user.PeopleID;
            if (user.DisplayName == null)
            {
                AssginDeli.UpdatedBy = user.PeopleFirstName;
            }
            else
            {
                AssginDeli.UpdatedBy = user.DisplayName;
            }
            context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);

            foreach (var od in AssOrderDispatchedMaster)
            {
                od.Status = "Issued";
                od.ReDispatchedStatus = "Issued";
                od.UpdatedDate = indianTime;
                od.DeliveryIssuanceIdOrderDeliveryMaster = obj.DeliveryIssuanceId;
                context.Entry(od).State = EntityState.Modified;
                #region Code For OrderDeliveryMaster

                OrderDeliveryMaster oDm = new OrderDeliveryMaster();
                oDm.OrderId = od.OrderId;
                oDm.CityId = od.CityId;
                oDm.CompanyId = od.CompanyId;
                oDm.WarehouseId = od.WarehouseId;
                oDm.WarehouseName = od.WarehouseName;
                oDm.DboyMobileNo = od.DboyMobileNo;
                oDm.DboyName = od.DboyName;
                oDm.CustomerId = od.CustomerId;
                oDm.CustomerName = od.CustomerName;
                oDm.Customerphonenum = od.Customerphonenum;
                oDm.ShopName = od.ShopName;
                oDm.Skcode = od.Skcode;
                oDm.Status = "Issued"; //od.Status;
                oDm.ShippingAddress = od.ShippingAddress;
                oDm.BillingAddress = od.BillingAddress;
                oDm.CanceledStatus = od.CanceledStatus;
                oDm.invoice_no = od.invoice_no;
                oDm.OnlineServiceTax = od.OnlineServiceTax;
                oDm.TotalAmount = od.TotalAmount;
                oDm.GrossAmount = od.GrossAmount;
                oDm.TaxAmount = od.TaxAmount;
                oDm.SGSTTaxAmmount = od.SGSTTaxAmmount;
                oDm.CGSTTaxAmmount = od.CGSTTaxAmmount;
                oDm.ReDispatchedStatus = od.ReDispatchedStatus;
                oDm.Trupay = od.Trupay;
                oDm.comments = od.comments;
                oDm.deliveryCharge = od.deliveryCharge;
                oDm.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                oDm.DiscountAmount = od.DiscountAmount;
                oDm.CheckNo = od.CheckNo;
                oDm.CheckAmount = od.CheckAmount;
                oDm.ElectronicPaymentNo = od.ElectronicPaymentNo;
                oDm.ElectronicAmount = od.ElectronicAmount;
                oDm.EpayLaterAmount = 0;
                oDm.CashAmount = od.CashAmount;
                oDm.OrderedDate = od.OrderedDate;
                oDm.WalletAmount = od.WalletAmount;
                oDm.RewardPoint = od.RewardPoint;
                oDm.Tin_No = od.Tin_No;
                oDm.ReDispatchCount = od.ReDispatchCount;
                oDm.UpdatedDate = indianTime;
                oDm.CreatedDate = indianTime;
                context.OrderDeliveryMasterDB.Add(oDm);
                #endregion
                var OrderMaster = ordermasterlist.Where(x => x.OrderId == od.OrderId).FirstOrDefault();
                OrderMaster.OldStatus = OrderMaster.Status;//Sudhir 12-09-2022
                OrderMaster.Status = "Issued";
                foreach (var item in OrderMaster.orderDetails)
                {
                    item.Status = "Issued";
                    item.UpdatedDate = indianTime;
                }
                OrderMaster.UpdatedDate = indianTime;
                context.Entry(OrderMaster).State = EntityState.Modified;


                foreach (var StockHit in od.orderDetails.Where(x => x.qty > 0))
                {
                    string RefStockCode = "";

                    if (OrderMaster.OrderType == 8)
                    {
                        RefStockCode = "CL";
                    }
                    //else if (OrderMaster.OrderType == 10)//non revenus stock
                    //{
                    //    RefStockCode = "NR";
                    //}
                    else
                    {
                        RefStockCode = "C";
                    }
                    bool isFree = OrderMaster.orderDetails.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                    if (isFree) { RefStockCode = "F"; }
                    else if (OrderMaster.OrderType == 6) //6 Damage stock
                    {
                        RefStockCode = "D";
                    }
                    bool IsDeliveryRedispatch = false;
                    if (OrderMaster.OldStatus == "Delivery Redispatch")//Sudhir 12-09-2022
                    {
                        IsDeliveryRedispatch = true;
                    }
                    OnIssuedStockEntryList.Add(new OnIssuedStockEntryDc
                    {
                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                        OrderId = StockHit.OrderId,
                        Qty = StockHit.qty,
                        UserId = user.PeopleID,
                        WarehouseId = StockHit.WarehouseId,
                        IsDeliveryRedispatch = IsDeliveryRedispatch,
                        RefStockCode = RefStockCode,
                    });
                }
            }
            #endregion

            if (OnIssuedStockEntryList.Any())
            {
                bool res = MultiStockHelpers.MakeBulkEntryOnPickerIssued(OnIssuedStockEntryList, context);
                if (!res)
                {
                    return false;
                }
            }

            #region insert ordermaster histories
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("IntValue");
            foreach (var item in ordermasterlist.Distinct())
            {
                var dr = dt.NewRow();
                dr["IntValue"] = item.OrderId;
                dt.Rows.Add(dr);
            }
            var paramorderids = new SqlParameter("orderids", dt);
            paramorderids.SqlDbType = System.Data.SqlDbType.Structured;
            paramorderids.TypeName = "dbo.IntValues";

            var paramStatus = new SqlParameter("Status", "Issued");
            var paramReasoncancel = new SqlParameter("Reasoncancel", "");
            var paramWarehousename = new SqlParameter("WarehouseName", AssOrderDispatchedMaster.FirstOrDefault().WarehouseName);
            var paramusername = new SqlParameter("username", user.DisplayName != null ? user.DisplayName : user.PeopleFirstName);
            var paramuserid = new SqlParameter("userid", System.Data.SqlDbType.Int);
            paramuserid.Value = user.PeopleID;
            var paramDeliveryIssuanceId = new SqlParameter("DeliveryIssuanceId", System.Data.SqlDbType.Int);
            paramDeliveryIssuanceId.Value = obj.DeliveryIssuanceId;
            var paramIsReAttempt = new SqlParameter("IsReAttempt", false);

            var paramDescription = new SqlParameter("Description", user.DisplayName != null ? (" (Issued AssignmentId : " + obj.DeliveryIssuanceId + ") By" + user.DisplayName) : (" (Issued AssignmentId : " + obj.DeliveryIssuanceId + ") By" + user.PeopleFirstName));

            int IsOrderMasterHistories = context.Database.ExecuteSqlCommand("Picker.InsertOrderMasterHistories @userid, @DeliveryIssuanceId, @IsReAttempt, @orderids, @Status, @Reasoncancel, @WarehouseName, @username, @Description ",
                paramuserid, paramDeliveryIssuanceId, paramIsReAttempt, paramorderids, paramStatus, paramReasoncancel, paramWarehousename, paramusername, paramDescription);

            result = context.Commit() > 0;
            #endregion

            return result;
        }
        #endregion
        public string DOPickerProcessCanceled(int PickerId, int userid, string comment)
        {
            var result = "";
            OrderOutPublisher Publisher = new OrderOutPublisher();
            List<BatchCodeSubjectDc> PublisherPickerRejectStockList = new List<BatchCodeSubjectDc>();
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(120);
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                using (var authContext = new AuthContext())
                {
                    var param = new SqlParameter("PickerId", PickerId);
                    long? TripMasterId = authContext.Database.SqlQuery<long?>("exec operation.IsTripPicker @PickerId", param).FirstOrDefault();
                    People people = null;
                    people = authContext.Peoples.FirstOrDefault(x => x.PeopleID == userid && x.Deleted == false && x.Active);
                    var OrderPickerMaster = authContext.OrderPickerMasterDb.Where(x => x.Id == PickerId).Include(x => x.orderPickerDetails).FirstOrDefault();
                    List<int> RejectedOrderids = OrderPickerMaster.orderPickerDetails.Select(x => x.OrderId).Distinct().ToList();
                    if (TripMasterId > 0)
                    {
                        TripPlannerHelper triphelp = new TripPlannerHelper();
                        bool IsSuccess = triphelp.RemovePickerOrderFromTrip(TripMasterId.Value, PickerId, RejectedOrderids, authContext, userid, true);
                        if (!IsSuccess)
                        {
                            dbContextTransaction.Dispose();
                            return "Unable to remove picker order from trip Picker NO#" + PickerId;
                        }
                    }
                    foreach (var Order in OrderPickerMaster.orderPickerDetails.Select(x => x.OrderId).Distinct())
                    {
                        OrderMaster omCheck = new OrderMaster();
                        OrderDispatchedMaster ODM = null;
                        omCheck = authContext.DbOrderMaster.Where(x => x.OrderId == Order && x.Deleted == false).Include(x => x.orderDetails).FirstOrDefault();
                        ODM = authContext.OrderDispatchedMasters.Where(x => x.OrderId == Order).FirstOrDefault();
                        if (omCheck != null && omCheck.Status == "ReadyToPick" && ODM == null)
                        {
                            foreach (var item in omCheck.orderDetails)
                            {
                                item.Status = "Pending";
                                item.UpdatedDate = indianTime;
                            }
                            omCheck.Status = "Pending";
                            omCheck.UpdatedDate = indianTime;
                            authContext.Entry(omCheck).State = EntityState.Modified;
                            #region Order History
                            OrderMasterHistories h1 = new OrderMasterHistories();
                            h1.orderid = omCheck.OrderId;
                            h1.Status = omCheck.Status;
                            h1.Reasoncancel = null;
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
                            h1.CreatedDate = DateTime.Now;
                            authContext.OrderMasterHistoriesDB.Add(h1);
                            #endregion
                            if (authContext.Commit() > 0)
                            {
                                #region stock Hit
                                MultiStockHelper<OnPickedCancelDc> MultiStockHelpers = new MultiStockHelper<OnPickedCancelDc>();
                                List<OnPickedCancelDc> RTDOnPickedCancelList = new List<OnPickedCancelDc>();
                                foreach (var StockHit in omCheck.orderDetails.Where(x => x.qty > 0))
                                {
                                    int qty = OrderPickerMaster.orderPickerDetails.FirstOrDefault(x => x.OrderDetailsId == StockHit.OrderDetailsId).Qty;
                                    if (qty > 0)
                                    {
                                        bool isfree = false;
                                        string RefStockCode = "";

                                        if (omCheck.OrderType == 8)
                                        {
                                            RefStockCode = "CL";
                                        }
                                        //else if(omCheck.OrderType == 10)//non revenus stock
                                        //    {
                                        //    RefStockCode = "NR";
                                        //}
                                        else
                                        {
                                            RefStockCode = "C";
                                        }

                                        if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                        {
                                            RefStockCode = "F";
                                            isfree = true;
                                        }
                                        RTDOnPickedCancelList.Add(new OnPickedCancelDc
                                        {
                                            ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                            OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                            OrderId = StockHit.OrderId,
                                            Qty = qty,
                                            UserId = userid,
                                            WarehouseId = StockHit.WarehouseId,
                                            IsFreeStock = isfree,
                                            RefStockCode = RefStockCode
                                        });
                                    }

                                }
                                if (RTDOnPickedCancelList.Any())
                                {
                                    bool res = MultiStockHelpers.MakeEntry(RTDOnPickedCancelList, "Stock_OnPickedCancel", authContext, dbContextTransaction);
                                    if (!res)
                                    {
                                        dbContextTransaction.Dispose();
                                        result = "Inventory not reverted on Canceled Picker";
                                        return result;
                                    }
                                    #region BatchCode
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
                                    #endregion
                                }
                                #endregion
                            }
                        }
                    }
                    OrderPickerMaster.IsCanceled = true;
                    OrderPickerMaster.Status = 4;
                    OrderPickerMaster.ModifiedDate = indianTime;
                    OrderPickerMaster.ModifiedBy = userid;
                    OrderPickerMaster.Comment = comment;
                    authContext.Entry(OrderPickerMaster).State = EntityState.Modified;
                    var OrderPickerMasterParam = new SqlParameter
                    {
                        ParameterName = "OrderPickerMasterId",
                        Value = OrderPickerMaster.Id
                    };
                    authContext.Database.ExecuteSqlCommand("Operation.RemoveOrderFromTripOnPickerReject @OrderPickerMasterId ", OrderPickerMasterParam);
                    if (authContext.Commit() > 0)
                    {
                        dbContextTransaction.Complete();
                        result = "Picker Order Canceled Successfully";
                    }
                    else { result = "Something went wrong"; return result; }

                }
            }
            if (PublisherPickerRejectStockList != null && PublisherPickerRejectStockList.Any() && result == "Picker Order Canceled Successfully")
            {
                Publisher.PlannedRejectPublish(PublisherPickerRejectStockList);
            }
            return result;
        }

        #region Order Auto Pick Process 29 Jan 2022 
        public string ReadyToPickDispatchedNEWAsync(List<OrderDispatchedMaster> postOrderDispatch, int userid, long PickerId, Warehouse wh, People people, long? TripMasterId, People Dboyinfo, List<PickerCustomerDc> PickerCustomerlist)
        {
            var result = "";
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(240);
            List<TCSCustomer> lstTCSCust = new List<TCSCustomer>();
            var outList = new List<OutDc>();
            OrderOutPublisher Publisher = new OrderOutPublisher();
            List<BatchCodeSubjectDc> OrderInvoiceQueue = new List<BatchCodeSubjectDc>();
            using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {

                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" Start : {indianTime}").ToString());

                using (var authContext = new AuthContext())
                {
                    List<OrderDispatchedMaster> OrderDispatchedMasterList = new List<OrderDispatchedMaster>();
                    double TotalAssignmentAmount = 0;
                    int stateId = 0;
                    stateId = wh.Stateid;
                    var OrderPickerMaster = authContext.OrderPickerMasterDb.FirstOrDefault(x => x.Id == PickerId && x.IsDeleted == false);
                    var param = new SqlParameter("PickerId", PickerId);
                    List<OnRTDOrderPickerDetailDC> OnRTDOrderPickerDetails = authContext.Database.SqlQuery<OnRTDOrderPickerDetailDC>("exec Picker.GetOnRTDPickerDetails @PickerId", param).ToList();

                    if (OrderPickerMaster != null && !OrderPickerMaster.IsCanceled && OrderPickerMaster.IsComplted && !OrderPickerMaster.IsDispatched && people != null)
                    {
                        var Pickertimer = authContext.PickerTimerDb.Where(x => x.OrderPickerMasterId == PickerId).ToList();//sp

                        var OrderIds = postOrderDispatch.Select(x => x.OrderId).Distinct().ToList();
                        var OrderMasterlist = authContext.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                        var OrderDetailsList = authContext.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                        var OrderDispatchedMasterlist = authContext.OrderDispatchedMasters.Where(x => OrderIds.Contains(x.OrderId) && x.WarehouseId == wh.WarehouseId).Include(x => x.orderDetails).ToList();
                        var Orderpaymentlist = authContext.PaymentResponseRetailerAppDb.Where(z => OrderIds.Contains(z.OrderId) && z.status == "Success").ToList();

                        OrderPickerMaster.Status = 3; //Dispatched(ApprovedDispatched), RTD
                        OrderPickerMaster.IsDispatched = true;
                        OrderPickerMaster.ModifiedDate = indianTime;
                        OrderPickerMaster.ModifiedBy = userid;
                        OrderPickerMaster.AgentId = 0;//
                        OrderPickerMaster.DeliveryIssuanceId = null;
                        OrderPickerMaster.TotalAssignmentAmount = 0;
                        OrderPickerMaster.IsCheckerGrabbed = true;
                        OrderPickerMaster.IsApproved = true;
                        OrderPickerMaster.ApproverId = userid;
                        OrderPickerMaster.ApprovedDate = indianTime;
                        authContext.Entry(OrderPickerMaster).State = EntityState.Modified;
                        MultiStockHelper<RTDOnPickedDc> MultiStockHelpers = new MultiStockHelper<RTDOnPickedDc>();
                        List<RTDOnPickedDc> RTDOnPickedList = new List<RTDOnPickedDc>();
                        #region TCS Calculate
                        string fy = (indianTime.Month >= 4 ? indianTime.Year + 1 : indianTime.Year).ToString();
                        //MongoDbHelper<TCSConfig> mConfigHelper = new MongoDbHelper<TCSConfig>();
                        //var tcsConfig = mConfigHelper.Select(x => x.FinancialYear == fy).FirstOrDefault();
                        #endregion



                        TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Loop Start : {indianTime}").ToString());

                        foreach (var Order in postOrderDispatch)
                        {
                            var cust = PickerCustomerlist.FirstOrDefault(x => x.CustomerId == Order.CustomerId);

                            OrderMaster omCheck = new OrderMaster();
                            OrderDispatchedMaster ODM = null;
                            List<int> itemids = null;
                            omCheck = OrderMasterlist.Where(x => x.OrderId == Order.OrderId).FirstOrDefault();
                            ODM = OrderDispatchedMasterlist.Where(x => x.OrderId == Order.OrderId).FirstOrDefault();
                            bool IsRejectedOrder = OnRTDOrderPickerDetails.Any(x => x.OrderId == Order.OrderId && x.Status != 2);

                            if ((omCheck.Status == "ReadyToPick") && (ODM == null) && !IsRejectedOrder)
                            {
                                itemids = OrderDetailsList.Where(c => c.ItemId > 0 && c.OrderId == Order.OrderId).Select(x => x.ItemId).Distinct().ToList();
                                var itemslist = OnRTDOrderPickerDetails.Where(x => itemids.Contains(x.ItemId) && x.OrderId == omCheck.OrderId).ToList();

                                double finaltotal = 0;
                                double finalTaxAmount = 0;
                                double finalSGSTTaxAmount = 0;
                                double finalCGSTTaxAmount = 0;
                                double finalGrossAmount = 0;
                                double finalTotalTaxAmount = 0;
                                double finalCessTaxAmount = 0;

                                OrderDispatchedMaster dm = Order;
                                OrderDispatchedMasterList.Add(dm);
                                dm.Status = "";
                                dm.CreatedDate = indianTime;
                                dm.UpdatedDate = indianTime;
                                dm.OrderedDate = omCheck.CreatedDate;
                                dm.InvoiceBarcodeImage = omCheck.InvoiceBarcodeImage;

                                dm.DboyMobileNo = TripMasterId > 0 ? Dboyinfo.Mobile : null;
                                dm.DBoyId = TripMasterId > 0 ? Dboyinfo.PeopleID : 0;
                                dm.DboyName = TripMasterId > 0 ? Dboyinfo.DisplayName : null;

                                var orderDetailQtys = OrderDetailsList.Where(z => z.OrderId == Order.OrderId).Sum(z => z.qty);//total order qty
                                var orderdispatchQty = OnRTDOrderPickerDetails.Where(x => x.OrderId == Order.OrderId).Sum(z => z.Qty);//
                                bool isQtyNotChanged = orderDetailQtys == orderdispatchQty;
                                if (!isQtyNotChanged)
                                {

                                    foreach (OrderDispatchedDetails pc in dm.orderDetails)
                                    {
                                        var qty = OnRTDOrderPickerDetails.FirstOrDefault(x => x.OrderDetailsId == pc.OrderDetailsId)?.Qty;//
                                        pc.qty = qty == null ? 0 : qty.Value;
                                        #region calculate free item qty
                                        if (!isQtyNotChanged && pc.IsFreeItem)
                                        {
                                            List<ReadyToDispatchHelper.FreeBillItems> freeBillItems = new List<ReadyToDispatchHelper.FreeBillItems>();
                                            freeBillItems = dm.orderDetails.Select(x => new ReadyToDispatchHelper.FreeBillItems
                                            {
                                                ItemId = x.ItemId,
                                                ItemNumber = x.itemNumber,
                                                OrderdetailId = x.OrderDetailsId,
                                                Qty = x.qty,
                                                UnitPrice = x.UnitPrice,
                                                IsFreeitem = x.IsFreeItem,
                                                OfferId = OrderDetailsList.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? OrderDetailsList.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).OfferId : null
                                            }).ToList();
                                            int ParentItemId = OrderDetailsList.Where(z => z.OrderId == Order.OrderId).FirstOrDefault(x => x.OrderDetailsId == pc.OrderDetailsId).FreeWithParentItemId ?? 0;
                                            if (ParentItemId >= 0)
                                            {
                                                var parent = dm.orderDetails.FirstOrDefault(x => x.ItemId == ParentItemId && x.OrderDetailsId != pc.OrderDetailsId && !x.IsFreeItem);
                                                int ParentOrderDetailsId = parent != null ? parent.OrderDetailsId : 0;
                                                string itemnum = parent != null ? parent.itemNumber : "";
                                                int TotalParentQty = ParentOrderDetailsId > 0 ? OnRTDOrderPickerDetails.FirstOrDefault(x => x.OrderDetailsId == ParentOrderDetailsId).Qty : 0;//
                                                ReadyToDispatchHelper Free = new ReadyToDispatchHelper();
                                                int freeitemqty = Free.getfreebiesitem(Order.OrderId, pc.ItemId, authContext, TotalParentQty, pc.OrderDetailsId, itemnum, freeBillItems);
                                                if (pc.qty <= freeitemqty)
                                                {
                                                    pc.qty = freeitemqty;

                                                }
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
                                            //double temp = pc.TaxPercentage + pc.TotalCessPercentage;
                                            double tempPercentagge3 = pc.TotalCessPercentage + pc.TaxPercentage;
                                            pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                            pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
                                            pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;

                                        }
                                        else
                                        {
                                            pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
                                        }


                                        //pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
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
                                }

                                TotalAssignmentAmount += dm.GrossAmount;
                                authContext.OrderDispatchedMasters.Add(dm);
                                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Commit1 Start : {indianTime}").ToString());

                                authContext.Commit();
                                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Commit1 End : {indianTime}").ToString());


                                //for Planned Stock to RTD Stock
                                //MultiStockHelper<RTDOnPickedDc> MultiStockHelpers = new MultiStockHelper<RTDOnPickedDc>();
                                //List<RTDOnPickedDc> RTDOnPickedList = new List<RTDOnPickedDc>();
                                foreach (var StockHit in dm.orderDetails.Where(x => x.qty > 0))
                                {
                                    string RefStockCode = "";

                                    if (omCheck.OrderType == 8)
                                    {
                                        RefStockCode = "CL";
                                    }
                                    //else if(omCheck.OrderType == 10)//non revenus stock
                                    //{
                                    //    RefStockCode = "NR";
                                    //}
                                    else
                                    {
                                        RefStockCode = "C";
                                    }
                                    bool IsFreeItem = false;
                                    bool isFree = OrderDetailsList.Where(z => z.OrderId == Order.OrderId).Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; IsFreeItem = true; }

                                    RTDOnPickedList.Add(new RTDOnPickedDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = people.PeopleID,
                                        WarehouseId = StockHit.WarehouseId,
                                        IsFreeStock = IsFreeItem,
                                        IsDispatchFromPlannedStock = true,
                                        RefStockCode = RefStockCode
                                    });
                                }
                                if (RTDOnPickedList.Any())
                                {


                                    //bool res = MultiStockHelpers.MakeBulkEntryOnPicker(RTDOnPickedList, authContext);
                                    //if (!res)
                                    //{
                                    //    result = "Can't Dispatched, Due to Dispatched Qty not Available";
                                    //    return result;
                                    //}



                                    //#region BatchCode
                                    //foreach (var s in dm.orderDetails.Where(x => x.qty > 0))
                                    //{
                                    //    PublisherrtdStockList.Add(new BatchCodeSubjectDc
                                    //    {
                                    //        ObjectDetailId = s.OrderDetailsId,
                                    //        ObjectId = s.OrderId,
                                    //        StockType = RTDOnPickedList.FirstOrDefault(c => c.OrderDispatchedDetailsId == s.OrderDispatchedDetailsId).RefStockCode,
                                    //        Quantity = s.qty,
                                    //        WarehouseId = s.WarehouseId,
                                    //        ItemMultiMrpId = s.ItemMultiMRPId
                                    //    });
                                    //}
                                    //#endregion


                                    outList.AddRange(dm.orderDetails.Where(x => x.qty > 0).Select(s => new OutDc
                                    {
                                        ItemMultiMrpId = s.ItemMultiMRPId,
                                        WarehouseId = dm.WarehouseId,
                                        Destination = "Sale",
                                        CreatedDate = indianTime,
                                        ObjectId = dm.OrderId,
                                        Qty = s.qty,
                                        SellingPrice = s.UnitPrice
                                    }));
                                }
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
                                authContext.Entry(omCheck).State = EntityState.Modified;

                                foreach (var ods in OrderDetailsList.Where(z => z.OrderId == Order.OrderId))
                                {
                                    ods.Status = omCheck.Status;
                                    ods.UpdatedDate = indianTime;
                                    authContext.Entry(ods).State = EntityState.Modified;
                                }

                                dm.Status = "Ready to Dispatch";
                                dm.invoice_no = invoiceNumber;
                                finaltotal = finaltotal + dm.deliveryCharge;
                                finalGrossAmount = finalGrossAmount + dm.deliveryCharge;
                                if (!isQtyNotChanged)
                                {
                                    dm.TotalAmount = Math.Round(finaltotal, 2) - dm.WalletAmount.GetValueOrDefault();
                                    dm.TaxAmount = Math.Round(finalTaxAmount, 2);
                                    dm.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
                                    dm.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
                                    dm.GrossAmount = Math.Round((Convert.ToInt32(finalGrossAmount) - dm.WalletAmount.GetValueOrDefault()), 0);
                                }

                                authContext.Entry(dm).State = EntityState.Modified;

                                #region Billdiscountamount
                                var Orderpayments = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId && z.status == "Success").ToList();
                                dm.TCSAmount = omCheck.TCSAmount;

                                double offerDiscountAmount = 0;
                                if (!isQtyNotChanged)
                                {
                                    var billdiscount = authContext.BillDiscountDb.Where(x => x.OrderId == omCheck.OrderId && x.CustomerId == omCheck.CustomerId).ToList();//sp
                                    var offerIds = billdiscount.Select(x => x.OfferId).ToList();
                                    var offers = authContext.OfferDb.Where(x => offerIds.Contains(x.OfferId)).ToList();
                                    List<int> flashdealItems = authContext.FlashDealItemConsumedDB.Where(x => x.OrderId == omCheck.OrderId).Select(x => x.ItemId).ToList();//sp
                                    var billdiscountofferids = billdiscount.Select(x => x.OfferId).ToList();

                                    List<Offer> Offers = authContext.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.OfferItemsBillDiscounts).Include(x => x.BillDiscountOfferSections).Include(y => y.BillDiscountFreeItems).ToList(); //sp

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
                                                    WalletPoint = Convert.ToInt32((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0));
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

                                    var CODcnt = Orderpayments.Count(z => z.OrderId == omCheck.OrderId && !z.IsOnline && z.status == "Success");
                                    var Onlinecnt = Orderpayments.Count(z => z.OrderId == omCheck.OrderId && z.IsOnline && z.status == "Success");
                                    bool IsConsumer = (cust.CustomerType != null && cust.CustomerType.ToLower() == "consumer") ? true : false;
                                    if (CODcnt > 0 && !IsConsumer)
                                    {
                                        //#region TCS Calculate
                                        //if (tcsConfig != null)
                                        //{
                                        //    MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                                        //    var tcsCustomer = mHelper.Select(x => x.CustomerId == omCheck.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                                        //    if (tcsCustomer != null && tcsCustomer.TotalPurchase >= tcsConfig.TCSAmountLimit)
                                        //    {
                                        //        var percent = string.IsNullOrEmpty(cust.PanNo) ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
                                        //        dm.TCSAmount = (dm.TotalAmount - offerDiscountAmount) * percent / 100;
                                        //    }
                                        //}
                                        //#endregion
                                        #region TCS Calculate
                                        GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                                        var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(cust.CustomerId, cust.PanNo, authContext);
                                        if (tcsConfig != null && !cust.IsTCSExemption)
                                        {
                                            var percent = !cust.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
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
                                    dm.TotalAmount = dm.TotalAmount - offerDiscountAmount + dm.TCSAmount;
                                    dm.BillDiscountAmount = offerDiscountAmount;
                                    dm.GrossAmount = System.Math.Round(dm.TotalAmount, 0);
                                }
                                #endregion
                                #region  //if Gross amount is negative due wallet amount more then dispatched amount 

                                if ((dm.GrossAmount < 0 && dm.WalletAmount > dm.GrossAmount))
                                {

                                    double _RefundDiffValue = Math.Abs(dm.GrossAmount);//Convert to positive
                                    var wallet = authContext.WalletDb.Where(c => c.CustomerId == dm.CustomerId).FirstOrDefault(); //Sp
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

                                        dm.GrossAmount = System.Math.Round(dm.TotalAmount, 0);
                                    }
                                }
                                #endregion
                                //if there is no barcode then genearte barcode in dispatched 
                                if (dm.InvoiceBarcodeImage == null) //byte value
                                {
                                    string Borderid = Convert.ToString(dm.OrderId);
                                    string BorderCodeId = Borderid.PadLeft(11, '0');
                                    temOrderQBcode code = GetBarcodeAsyc(BorderCodeId);
                                    dm.InvoiceBarcodeImage = code.BarcodeImage;
                                }
                                var otherMode = Orderpayments.Where(z => z.OrderId == omCheck.OrderId && z.IsOnline && z.status == "Success").ToList();
                                double otherModeAmt = 0;
                                if (otherMode.Count > 0)
                                {
                                    otherModeAmt = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId && z.IsOnline && z.status == "Success").Sum(x => x.amount);
                                }
                                if (omCheck.GrossAmount != dm.GrossAmount && otherModeAmt == 0)
                                {
                                    var cashOldEntries = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId && !z.IsOnline
                                                             && z.status == "Success").ToList();

                                    if (cashOldEntries != null && cashOldEntries.Any() && !cashOldEntries.Any(x => x.PaymentFrom == "RTGS/NEFT"))
                                    {
                                        foreach (var cash in cashOldEntries)
                                        {
                                            cash.status = "Failed";
                                            cash.statusDesc = "Due to Items cut when Auto Ready to Pick";
                                            authContext.Entry(cash).State = EntityState.Modified;
                                        }
                                    }

                                    // var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp();
                                    if (cashOldEntries.Any(x => x.PaymentFrom == "RTGS/NEFT"))
                                    {
                                        foreach (var VANRTGS in cashOldEntries.Where(x => x.PaymentFrom == "RTGS/NEFT"))
                                        {
                                            VANRTGS.status = "Failed";
                                            VANRTGS.statusDesc = "Due to Items cut when Auto Ready to Pick";
                                            authContext.Entry(VANRTGS).State = EntityState.Modified;
                                        }
                                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
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
                                        authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                    }
                                    //else
                                    //{
                                    //    PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                    //    {
                                    //        amount = dm.GrossAmount,
                                    //        CreatedDate = DateTime.Now,
                                    //        currencyCode = "INR",
                                    //        OrderId = dm.OrderId,
                                    //        PaymentFrom = "RTGS/NEFT",
                                    //        status = "Success",
                                    //        UpdatedDate = DateTime.Now,
                                    //        IsRefund = false,
                                    //        statusDesc = "Due to Items cut when Ready to Dispatch"
                                    //    };
                                    //}                                   
                                }

                                else if (omCheck.GrossAmount != dm.GrossAmount && otherModeAmt > 0)
                                {
                                    if (omCheck.paymentThrough.Trim().ToLower() == "paylater")
                                    {
                                        //var paylaterdata = authContext.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == omCheck.OrderId);
                                        //if (paylaterdata != null)
                                        //{
                                        //    var paylaterhistory = authContext.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylaterdata.Id && x.Comment == "Retailer Pay Now").FirstOrDefault();
                                        //    if (paylaterhistory != null)
                                        //    {
                                        //        double totalamount = dm.GrossAmount;
                                        //        double amount = paylaterhistory.Amount;
                                        //        if (amount >= totalamount)
                                        //        {
                                        //            if (amount > totalamount)
                                        //            {
                                        //                PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                        //                var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                        //                {
                                        //                    amount = (-1) * (amount - totalamount),
                                        //                    CreatedDate = DateTime.Now,
                                        //                    currencyCode = "INR",
                                        //                    OrderId = paylaterdata.OrderId,
                                        //                    PaymentFrom = paylaterhistory.PaymentMode,
                                        //                    GatewayTransId = paylaterhistory.RefNo,
                                        //                    GatewayOrderId = Convert.ToString(paylaterdata.OrderId),
                                        //                    status = "Success",
                                        //                    UpdatedDate = DateTime.Now,
                                        //                    IsRefund = false,
                                        //                    IsOnline = true,
                                        //                    statusDesc = "Refund Initiated"
                                        //                };
                                        //                authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                        //                authContext.Commit();
                                        //                var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                        //                {
                                        //                    Amount = (amount - totalamount),
                                        //                    OrderId = PaymentResponseRetailerAppDb.OrderId,
                                        //                    Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                        //                    ReqGatewayTransId = PaymentResponseRetailerAppDb.GatewayTransId,
                                        //                    Status = (int)PaymentRefundEnum.Initiated,
                                        //                    CreatedBy = userid,
                                        //                    CreatedDate = DateTime.Now,
                                        //                    IsActive = true,
                                        //                    IsDeleted = false,
                                        //                    ModifiedBy = userid,
                                        //                    ModifiedDate = DateTime.Now,
                                        //                    PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                        //                };
                                        //                bool IsInserted = PRHelper.InsertPaymentRefundRequest(authContext, PaymentRefundRequestDc);


                                        //                Model.CashManagement.PayLaterCollectionHistory hist = new Model.CashManagement.PayLaterCollectionHistory();
                                        //                hist.Amount = (-1) * (amount - totalamount);
                                        //                hist.PayLaterCollectionId = paylaterdata.Id;
                                        //                hist.CreatedDate = DateTime.Now;
                                        //                hist.CreatedBy = userid;
                                        //                hist.Comment = "Cut Dispatch Refund Online";
                                        //                hist.IsActive = true;
                                        //                hist.IsDeleted = false;
                                        //                hist.CurrencyHubStockId = 0;
                                        //                hist.PaymentMode = "Cash";
                                        //                authContext.PayLaterCollectionHistoryDb.Add(hist);

                                        //                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                        //                paylaterdata.ModifiedDate = DateTime.Now;
                                        //                paylaterdata.ModifiedBy = 1;
                                        //                authContext.Entry(paylaterdata).State = EntityState.Modified;
                                        //            }
                                        //            else
                                        //            {
                                        //                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                        //                paylaterdata.ModifiedDate = DateTime.Now;
                                        //                paylaterdata.ModifiedBy = 1;
                                        //                authContext.Entry(paylaterdata).State = EntityState.Modified;
                                        //            }
                                        //        }
                                        //    }
                                        //}

                                        //var paylaterdata = authContext.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == omCheck.OrderId );
                                        //if (paylaterdata != null)
                                        //{
                                        //    var paylaterhistory = authContext.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylaterdata.Id).ToList();
                                        //    if (paylaterhistory != null && paylaterhistory.Any())
                                        //    {
                                        //        double totalamount = dm.GrossAmount;
                                        //        double amount = paylaterhistory.Sum(x => x.Amount);
                                        //        if (amount >= totalamount)
                                        //        {
                                        //            if (amount > totalamount)
                                        //            {
                                        //                Model.CashManagement.PayLaterCollectionHistory hist = new Model.CashManagement.PayLaterCollectionHistory();
                                        //                hist.Amount = (-1) * (amount - totalamount);
                                        //                hist.PayLaterCollectionId = paylaterdata.Id;
                                        //                hist.CreatedDate = DateTime.Now;
                                        //                hist.CreatedBy = userid;
                                        //                hist.Comment = "CutDispatch";
                                        //                hist.IsActive = true;
                                        //                hist.IsDeleted = false;
                                        //                hist.CurrencyHubStockId = 0;
                                        //                hist.PaymentMode = "Cash";
                                        //                authContext.PayLaterCollectionHistoryDb.Add(hist);

                                        //                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                        //                paylaterdata.ModifiedDate = DateTime.Now;
                                        //                paylaterdata.ModifiedBy = 1;
                                        //                authContext.Entry(paylaterdata).State = EntityState.Modified;


                                        //            }
                                        //            else
                                        //            {
                                        //                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                        //                paylaterdata.ModifiedDate = DateTime.Now;
                                        //                paylaterdata.ModifiedBy = 1;
                                        //                authContext.Entry(paylaterdata).State = EntityState.Modified;
                                        //            }
                                        //        }
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        if (wh.IsOnlineRefundEnabled)
                                        {
                                            // Cut line item Payment refund  -- April2022
                                            #region Cut line item Payment refund  -- April2022
                                            var RefundDays = authContext.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();


                                            // case 1:  Online payment is more than order payment for payment mode in  (chqbook)
                                            if (dm.GrossAmount < Orderpayments.Where(z => z.OrderId == omCheck.OrderId && (z.PaymentFrom == "chqbook") && z.IsOnline && z.status == "Success").Sum(z => z.amount))
                                            {
                                                result = "Can't dispatched due to online payment is more than dispatched amount for Order  : " + omCheck.OrderId;
                                                dbContextTransaction.Dispose();
                                                OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);

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
                                                        result = "Can't dispatch cut item order , because online payment refund days expired for Order  : " + omCheck.OrderId;
                                                        dbContextTransaction.Dispose();
                                                        return result;
                                                    }
                                                    else if (PaymentRefundDays == null && item.PaymentFrom.Trim().ToLower() != "gullak")
                                                    {
                                                        result = "refund apis or refund days not configured for payment mode " + item.PaymentFrom;
                                                        dbContextTransaction.Dispose();
                                                        OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
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
                                            #region old code Gullak
                                            if (omCheck.OrderType == 4)
                                            {
                                                var gullakOldEntries = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId //&& z.PaymentFrom == "Gullak"
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


                                                if (omCheck.paymentMode == "Gullak")
                                                {
                                                    var oldgullak = gullakOldEntries.FirstOrDefault(z => z.PaymentFrom == "Gullak");
                                                    double RemainingGullakAmount = 0;
                                                    double gullakAmount = 0;
                                                    double cashAmount = 0;
                                                    List<PaymentResponseRetailerApp> GullakPaymentResponseRetailerAppDbs = new List<PaymentResponseRetailerApp>();
                                                    if (oldgullak != null)
                                                    {
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

                                                        GullakPaymentResponseRetailerAppDbs = new List<PaymentResponseRetailerApp> {
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
                                            statusDesc = "Due to Items cut when Ready to Dispatch",
                                            IsOnline=true
                                        }
                                        };
                                                    }
                                                    else
                                                    {
                                                        cashAmount = dm.GrossAmount;
                                                    }
                                                    if (cashAmount > 0)
                                                    {
                                                        GullakPaymentResponseRetailerAppDbs = new List<PaymentResponseRetailerApp> {
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
                                             }};
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
                                                else
                                                {

                                                    var cashOldEntries = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom == "Cash"
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
                                            }
                                            else
                                            {
                                                result = "Order amount and dispatch amount is different.It is not allowed in online payment.";
                                                dbContextTransaction.Dispose();
                                                OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                                                return result;
                                                //throw new Exception("Order amount and dispatch amount is different. It is not allowed in online payment.");
                                            }
                                            #endregion
                                        }
                                    }

                                }


                                #region Update IRN Check 
                                IRNHelper irnHelper = new IRNHelper();

                                if (cust.IsGenerateIRN)  //spwithnolock
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

                                authContext.Entry(dm).State = EntityState.Modified;

                                #region if no entry of payment then insert entry in cash 
                                if (!Orderpaymentlist.Any(x => x.OrderId == dm.OrderId && x.status == "Success"))
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

                                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Commit2 Start : {indianTime}").ToString());

                                if (authContext.Commit() > 0)
                                {
                                    TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Commit2 end : {indianTime}").ToString());

                                    try
                                    {

                                        #region VAN notification
                                        if (Orderpaymentlist.Any(x => x.OrderId == dm.OrderId && x.status == "Success" && x.PaymentFrom == "RTGS/NEFT" && string.IsNullOrEmpty(x.GatewayTransId)) && cust != null && cust.FcmId != null)
                                        {
                                            ReadyToDispatchHelper readyToDispatchHelper = new ReadyToDispatchHelper();
                                            readyToDispatchHelper.VANForNotification(dm.OrderId, dm.GrossAmount, cust);
                                        }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Error(ex.Message);
                                    }
                                    #region Customer RDF Amount calculate for TCS
                                    if (lstTCSCust.Any(x => x.CustomerId == dm.CustomerId))
                                    {
                                        lstTCSCust.FirstOrDefault(x => x.CustomerId == dm.CustomerId).TotalPurchase += dm.GrossAmount;
                                    }
                                    else
                                    {
                                        lstTCSCust.Add(new TCSCustomer { CustomerId = dm.CustomerId, TotalPurchase = dm.GrossAmount });
                                    }
                                    #endregion

                                }
                            }
                            else if ((omCheck.Status == "Ready to Dispatch") && (ODM != null) && !IsRejectedOrder && TripMasterId > 0)
                            {
                                TotalAssignmentAmount += ODM.GrossAmount;
                                ODM.DboyMobileNo = TripMasterId > 0 ? Dboyinfo.Mobile : null;
                                ODM.DBoyId = TripMasterId > 0 ? Dboyinfo.PeopleID : 0;
                                ODM.DboyName = TripMasterId > 0 ? Dboyinfo.DisplayName : null;
                                ODM.UpdatedDate = indianTime;
                                authContext.Entry(ODM).State = EntityState.Modified;

                                OrderDispatchedMasterList.Add(ODM);
                                try
                                {

                                    #region VAN notification
                                    if (Orderpaymentlist.Any(x => x.OrderId == ODM.OrderId && x.status == "Success" && x.PaymentFrom == "RTGS/NEFT" && string.IsNullOrEmpty(x.GatewayTransId)) && cust != null && cust.FcmId != null)
                                    {
                                        ReadyToDispatchHelper readyToDispatchHelper = new ReadyToDispatchHelper();
                                        readyToDispatchHelper.VANForNotification(ODM.OrderId, ODM.GrossAmount, cust);

                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex.Message);
                                }
                            }
                        }

                        TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Loop End : {indianTime}").ToString());

                        TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Makebulkentry start : {indianTime}").ToString());

                        bool res = MultiStockHelpers.MakeBulkEntryOnPicker(RTDOnPickedList, authContext);

                        TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Makebulkentry end : {indianTime}").ToString());


                        if (!res)
                        {
                            result = "Can't Dispatched, Due to Dispatched Qty not Available";
                            dbContextTransaction.Dispose();
                            OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                            return result;
                        }
                        #region insert ordermaster histories
                        System.Data.DataTable dt = new System.Data.DataTable();
                        dt.Columns.Add("IntValue");
                        foreach (var item in OrderIds.Distinct())
                        {
                            var dr = dt.NewRow();
                            dr["IntValue"] = item;
                            dt.Rows.Add(dr);
                        }
                        var paramorderids = new SqlParameter("orderids", dt);
                        paramorderids.SqlDbType = System.Data.SqlDbType.Structured;
                        paramorderids.TypeName = "dbo.IntValues";
                        var paramStatus = new SqlParameter("Status", "Ready to Dispatch");
                        var paramReasoncancel = new SqlParameter("Reasoncancel", "");
                        var paramWarehousename = new SqlParameter("WarehouseName", wh.WarehouseName);
                        var paramusername = new SqlParameter("username", people.DisplayName != null ? people.DisplayName : people.PeopleFirstName);

                        var paramuserid = new SqlParameter("userid", System.Data.SqlDbType.Int);
                        paramuserid.Value = userid;

                        var paramDeliveryIssuanceId = new SqlParameter("DeliveryIssuanceId", System.Data.SqlDbType.Int);
                        paramDeliveryIssuanceId.Value = 0;

                        var paramIsReAttempt = new SqlParameter("IsReAttempt", false);
                        var paramDescription = new SqlParameter("Description", "");
                        var RtdDate = new SqlParameter("RtdDate", indianTime);



                        TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for InsertOrderMasterRTDHistories Ready to Dispatch Start : {indianTime}").ToString());


                        int IsOrderMasterHistories = authContext.Database.ExecuteSqlCommand("Picker.InsertOrderMasterRTDHistories @userid, @DeliveryIssuanceId, @IsReAttempt, @orderids, @Status, @Reasoncancel, @WarehouseName, @username, @Description ,@RtdDate ",
                            paramuserid, paramDeliveryIssuanceId, paramIsReAttempt, paramorderids, paramStatus, paramReasoncancel, paramWarehousename, paramusername, paramDescription, RtdDate);
                        TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for InsertOrderMasterHistories Ready to Dispatch end : {indianTime}").ToString());

                        #endregion
                        if (Pickertimer != null && Pickertimer.Any(x => x.EndTime == null && x.Type == 1))
                        {
                            var updatePickertimer = Pickertimer.Where(x => x.EndTime == null && x.Type == 1).ToList();
                            updatePickertimer.ForEach(x =>
                              {
                                  x.EndTime = indianTime;
                                  authContext.Entry(x).State = EntityState.Modified;
                              });
                        }
                        TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for commit3 Start : {indianTime}").ToString());

                        if (authContext.Commit() > 0)
                        {
                            TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for commit3 end : {indianTime}").ToString());

                            if (TripMasterId > 0)
                            {
                                ApprovedDispatchedDC ApprovedAndDispatched = new ApprovedDispatchedDC();
                                ApprovedAndDispatched.UserId = userid;
                                ApprovedAndDispatched.DeliveryBoyId = OrderPickerMaster.DBoyId ?? 0;
                                ApprovedAndDispatched.AgentId = OrderPickerMaster.AgentId ?? 0;
                                ApprovedAndDispatched.PickerId = Convert.ToInt32(OrderPickerMaster.Id);
                                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for GenerateAssigmentForAutoPick Start : {indianTime}").ToString());

                                List<int> AssignmentIdList = GenerateAssigmentForAutoPick(OrderDispatchedMasterList, OrderMasterlist, OrderDetailsList, OrderPickerMaster, authContext, dbContextTransaction, ApprovedAndDispatched, TripMasterId.Value, Dboyinfo, TotalAssignmentAmount, OrderIds);
                                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for GenerateAssigmentForAutoPick end : {indianTime}").ToString());

                                if (AssignmentIdList == null)
                                {
                                    dbContextTransaction.Dispose();
                                    OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);

                                    result = "Unable to create trip assignmentId for Picker #No :" + PickerId;
                                    return result;
                                }

                                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for FinalizeTrip Start : {indianTime}").ToString());

                                foreach (var AssignmentId in AssignmentIdList)
                                {
                                    if (AssignmentId == 0)
                                    {
                                        dbContextTransaction.Dispose();
                                        OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                                        result = "Unable to create trip assignmentId for Picker #No :" + PickerId;
                                        return result;
                                    }
                                    // Update tripPickerMapping record by assignmentId  against TripMasterId
                                    TripPlannerHelper triphelper = new TripPlannerHelper();
                                    bool IsSuccess = triphelper.FinalizeTrip(userid, PickerId, AssignmentId, TripMasterId.Value, authContext);
                                    if (!IsSuccess)
                                    {
                                        dbContextTransaction.Dispose();
                                        OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                                        result = "Issue in Finalize Trip for PickerId #No :" + PickerId;
                                        return result;
                                    }
                                }
                                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for FinalizeTrip end : {indianTime}").ToString());

                                dbContextTransaction.Complete();
                                result = "Approved and RTD and Assignment Created NO# :" + string.Join(",", AssignmentIdList);

                            }
                            else
                            {
                                dbContextTransaction.Complete();
                                result = "#No. " + OrderPickerMaster.Id + "  Approved and RTD Now Create Assignment";
                            }

                            #region Insert in FIFO
                            if (ConfigurationManager.AppSettings["LiveFIFO"] == "1" && outList.Any())
                            {
                                foreach (var item in outList)
                                {
                                    RabbitMqHelper rabbitMqHelper = new RabbitMqHelper();
                                    rabbitMqHelper.Publish("RTD", item);
                                }
                            }
                            #endregion

                            #region FY Parchase Calculate
                            foreach (var item in lstTCSCust)
                            {
                                MongoDbHelper<TCSCustomer> mHelper = new MongoDbHelper<TCSCustomer>();
                                var tcsCustomer = mHelper.Select(x => x.CustomerId == item.CustomerId && x.FinancialYear == fy).FirstOrDefault();
                                if (tcsCustomer != null)
                                {
                                    tcsCustomer.TotalPurchase += item.TotalPurchase;
                                    tcsCustomer.LastUpdatedDate = indianTime;
                                    mHelper.ReplaceWithoutFind(tcsCustomer.Id, tcsCustomer, "TCSCustomer");
                                }
                                else
                                {
                                    tcsCustomer = new TCSCustomer
                                    {
                                        CustomerId = item.CustomerId,
                                        FinancialYear = fy,
                                        LastUpdatedDate = indianTime,
                                        TotalPurchase = item.TotalPurchase
                                    };
                                    mHelper.Insert(tcsCustomer);
                                }
                            }
                            #endregion
                        }
                        else { result = "#No. " + OrderPickerMaster.Id + " something went wrong"; return result; }
                    }
                }
            }

            TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" End : {indianTime}").ToString());
            if (OrderInvoiceQueue != null && OrderInvoiceQueue.Any())
            {
                Publisher.PublishOrderInvoiceQueue(OrderInvoiceQueue);
            }
            return result;
        }
        public async Task<List<OrdersForAutoPick>> GenerateOrderAutoPick(List<OrdersForAutoPick> OrdersForAutoPicks)
        {
            OrderOutPublisher Publisher = new OrderOutPublisher();

            foreach (var item in OrdersForAutoPicks.GroupBy(x => x.OrderId))
            {
                List<BatchCodeSubjectDc> PublisherPickerStockList = new List<BatchCodeSubjectDc>();
                List<CurrentToPlanedStockMoveDC> CurrentToPlanedStockMove = new List<CurrentToPlanedStockMoveDC>();
                List<ReadyToPickOrderDetails> AddReadyToPickOrderDetails = new List<ReadyToPickOrderDetails>();
                List<OrderAutoPickResp> OrderAutoPickRespList = new List<OrderAutoPickResp>();
                var OrderDetailsOb = new List<OrderDetails>();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (var dbContextTransaction = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    try
                    {
                        using (AuthContext context = new AuthContext())
                        {

                            var Ordermaster = context.DbOrderMaster.FirstOrDefault(x => x.OrderId == item.Key);
                            if (Ordermaster != null && Ordermaster.Status == "Pending")
                            {
                                var orderDetails = context.DbOrderDetails.Where(x => x.OrderId == item.Key && x.qty > 0).ToList();

                                foreach (var itemdetail in item.OrderBy(x => x.IsFreeItem).ToList())
                                {
                                    if (itemdetail.IsFreeItem)
                                    {
                                        List<ReadyToDispatchHelper.FreeBillItems> freeBillItems = new List<ReadyToDispatchHelper.FreeBillItems>();
                                        freeBillItems = item.Where(a => a.OrderId == itemdetail.OrderId).Select(x => new ReadyToDispatchHelper.FreeBillItems
                                        {
                                            ItemId = orderDetails.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? orderDetails.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).ItemId : 0,
                                            ItemNumber = orderDetails.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? orderDetails.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).itemNumber : "",
                                            OrderdetailId = x.OrderDetailsId,
                                            Qty = x.Qty,
                                            UnitPrice = x.UnitPrice,
                                            IsFreeitem = x.IsFreeItem,
                                            OfferId = orderDetails.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? orderDetails.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).OfferId : null
                                        }).ToList();
                                        #region calculate free item qty
                                        var Parent = orderDetails.Where(x => x.OrderDetailsId == itemdetail.OrderDetailsId && x.FreeWithParentItemId >= 0).FirstOrDefault();
                                        string itemnum = Parent != null ? Parent.itemNumber : "";
                                        if (Parent != null && Parent.FreeWithParentItemId > 0)
                                        {
                                            var parentdata = orderDetails.Where(x => x.OrderDetailsId != itemdetail.OrderDetailsId && x.ItemId == Parent.FreeWithParentItemId && !x.IsFreeItem).FirstOrDefault();
                                            itemnum = parentdata != null ? parentdata.itemNumber : "";
                                            int Ids = orderDetails.FirstOrDefault(x => x.ItemId == Parent.FreeWithParentItemId && x.OrderId == Parent.OrderId && x.OrderDetailsId != itemdetail.OrderDetailsId && !x.IsFreeItem).OrderDetailsId;
                                            if (Ids > 0)
                                            {
                                                var TotalParentQty = item.Where(x => x.OrderDetailsId == Ids).FirstOrDefault();
                                                if (TotalParentQty != null)
                                                {
                                                    ReadyToDispatchHelper Free = new ReadyToDispatchHelper();
                                                    int freeitemqty = Free.getfreebiesitem(itemdetail.OrderId, Parent.ItemId, context, TotalParentQty.FulfilledQty, itemdetail.OrderDetailsId, itemnum, freeBillItems);//, 
                                                    itemdetail.FulfilledQty = freeitemqty;
                                                }
                                                else if (TotalParentQty == null)
                                                {
                                                    var TotalParentQtys = item.Where(x => x.OrderDetailsId == Ids).FirstOrDefault();
                                                    if (TotalParentQtys != null)
                                                    {
                                                        ReadyToDispatchHelper Free = new ReadyToDispatchHelper();
                                                        int freeitemqty = Free.getfreebiesitem(itemdetail.OrderId, Parent.ItemId, context, TotalParentQtys.FulfilledQty, itemdetail.OrderDetailsId, itemnum, freeBillItems);//, 
                                                        itemdetail.FulfilledQty = freeitemqty;
                                                    }
                                                    else
                                                    {
                                                        itemdetail.FulfilledQty = 0;
                                                    }
                                                }
                                            }
                                        }
                                        else if (Parent != null && Parent.FreeWithParentItemId == 0)
                                        {
                                            ReadyToDispatchHelper Free = new ReadyToDispatchHelper();
                                            int freeitemqty = Free.getfreebiesitem(itemdetail.OrderId, Parent.ItemId, context, 0, itemdetail.OrderDetailsId, itemnum, freeBillItems);
                                            itemdetail.FulfilledQty = freeitemqty;
                                        }
                                        CurrentToPlanedStockMoveDC AddItem = new CurrentToPlanedStockMoveDC();
                                        AddItem.ItemMultiMrpId = itemdetail.ItemMultiMrpId;
                                        AddItem.OrderDetailsId = itemdetail.OrderDetailsId;
                                        AddItem.OrderId = itemdetail.OrderId;
                                        AddItem.Qty = itemdetail.FulfilledQty;
                                        AddItem.IsFreeItem = itemdetail.IsFreeItem;
                                        AddItem.IsDispatchedFreeStock = itemdetail.IsDispatchedFreeStock;
                                        AddItem.WarehouseId = itemdetail.WarehouseId;
                                        CurrentToPlanedStockMove.Add(AddItem);
                                        #endregion
                                    }
                                    else
                                    {
                                        CurrentToPlanedStockMoveDC AddItem = new CurrentToPlanedStockMoveDC();
                                        AddItem.ItemMultiMrpId = itemdetail.ItemMultiMrpId;
                                        AddItem.OrderDetailsId = itemdetail.OrderDetailsId;
                                        AddItem.OrderId = itemdetail.OrderId;
                                        AddItem.Qty = itemdetail.FulfilledQty;
                                        AddItem.IsFreeItem = itemdetail.IsFreeItem;
                                        AddItem.IsDispatchedFreeStock = itemdetail.IsDispatchedFreeStock;
                                        AddItem.WarehouseId = itemdetail.WarehouseId;
                                        CurrentToPlanedStockMove.Add(AddItem);
                                    }

                                    if (!context.ReadyToPickOrderDetailDb.Any(x => x.OrderId == itemdetail.OrderId))
                                    {
                                        ReadyToPickOrderDetails rtpdetails = new ReadyToPickOrderDetails();
                                        rtpdetails.OrderId = itemdetail.OrderId;
                                        rtpdetails.OrderDetailsId = itemdetail.OrderDetailsId;
                                        rtpdetails.ItemMultiMrpId = itemdetail.ItemMultiMrpId;
                                        rtpdetails.IsFreeItem = itemdetail.IsFreeItem;
                                        rtpdetails.IsDispatchedFreeStock = itemdetail.IsDispatchedFreeStock;
                                        rtpdetails.Qty = itemdetail.FulfilledQty;
                                        rtpdetails.ItemNumber = orderDetails.FirstOrDefault(x => x.OrderDetailsId == itemdetail.OrderDetailsId).itemNumber;
                                        rtpdetails.Itemname = orderDetails.FirstOrDefault(x => x.OrderDetailsId == itemdetail.OrderDetailsId).itemname;
                                        rtpdetails.CreatedDate = DateTime.Now;
                                        AddReadyToPickOrderDetails.Add(rtpdetails);
                                    }
                                    else
                                    {  //If Qty Change then update the qty
                                        var RtpOldExists = context.ReadyToPickOrderDetailDb.FirstOrDefault(x => x.OrderDetailsId == itemdetail.OrderDetailsId);
                                        if (RtpOldExists != null && RtpOldExists.Qty != itemdetail.FulfilledQty)
                                        {
                                            RtpOldExists.Qty = itemdetail.FulfilledQty;
                                            RtpOldExists.CreatedDate = DateTime.Now;
                                            context.Entry(RtpOldExists).State = EntityState.Modified;
                                        }
                                    }
                                }


                                Ordermaster.UpdatedDate = indianTime;
                                Ordermaster.Status = "ReadyToPick";

                                foreach (var ods in orderDetails)
                                {
                                    ods.UpdatedDate = DateTime.Now;
                                    ods.Status = "ReadyToPick";
                                    context.Entry(ods).State = EntityState.Modified; ;
                                }

                                OrderMasterHistories h1 = new OrderMasterHistories();
                                h1.orderid = item.Key;
                                h1.Status = "ReadyToPick";
                                h1.userid = 0;
                                h1.Description = "Auto";
                                h1.CreatedDate = DateTime.Now;
                                context.OrderMasterHistoriesDB.Add(h1);
                                context.Entry(Ordermaster).State = EntityState.Modified;

                                //foreach (var odsv1 in OrderDetailsOb)
                                //{
                                //    context.Entry(odsv1).State = EntityState.Modified;
                                //}

                                if (AddReadyToPickOrderDetails != null && AddReadyToPickOrderDetails.Any())
                                {
                                    context.ReadyToPickOrderDetailDb.AddRange(AddReadyToPickOrderDetails);
                                }

                                #region stock Hit
                                //for currentstock
                                MultiStockHelper<Stock_OnPickedDc> MultiStockHelpers = new MultiStockHelper<Stock_OnPickedDc>();
                                List<Stock_OnPickedDc> rtdStockList = new List<Stock_OnPickedDc>();
                                foreach (var StockHit in CurrentToPlanedStockMove.Where(x => x.Qty > 0))
                                {
                                    bool isfree = false;
                                    string RefStockCode = "";

                                    if (Ordermaster.OrderType == 8)
                                    {
                                        RefStockCode = "CL";
                                    }
                                    //else if(Ordermaster.OrderType == 10)//non revenus stock
                                    //{
                                    //    RefStockCode = "NR";
                                    //}
                                    else
                                    {
                                        RefStockCode = "C";
                                    }



                                    if (StockHit.IsFreeItem && StockHit.IsDispatchedFreeStock)
                                    {
                                        RefStockCode = "F";
                                        isfree = true;
                                    }
                                    rtdStockList.Add(new Stock_OnPickedDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMrpId,
                                        OrderDispatchedDetailsId = StockHit.OrderDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.Qty,
                                        UserId = 1,
                                        WarehouseId = StockHit.WarehouseId,
                                        IsFreeStock = isfree,
                                        RefStockCode = RefStockCode
                                    });
                                }


                                bool res = MultiStockHelpers.MakeEntry(rtdStockList, "Stock_OnPicked", context, dbContextTransaction);
                                if (!res)
                                {

                                    dbContextTransaction.Dispose();
                                    item.ToList().ForEach(x =>
                                    {
                                        x.IsProcessed = true;
                                        x.ErrorMsg = "Error Occured during Stock hit";
                                    });
                                    continue;
                                }
                                #endregion
                                if (context.Commit() > 0)
                                {
                                    #region BatchCode
                                    foreach (var s in rtdStockList.Where(x => x.Qty > 0))
                                    {
                                        PublisherPickerStockList.Add(new BatchCodeSubjectDc
                                        {
                                            ObjectDetailId = s.OrderDispatchedDetailsId,
                                            ObjectId = s.OrderId,
                                            StockType = s.RefStockCode,
                                            Quantity = s.Qty,
                                            WarehouseId = s.WarehouseId,
                                            ItemMultiMrpId = s.ItemMultiMRPId
                                        });
                                    }
                                    #endregion
                                    dbContextTransaction.Complete();
                                    item.ToList().ForEach(x =>
                                    {
                                        x.IsProcessed = true;

                                    });
                                }
                                else
                                {
                                    dbContextTransaction.Dispose();
                                    item.ToList().ForEach(x =>
                                    {
                                        x.IsProcessed = true;
                                        x.ErrorMsg = "Error Occured during Commit";
                                    });
                                }

                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        item.ToList().ForEach(x =>
                        {
                            x.IsProcessed = true;
                            x.ErrorMsg = ex.ToString();
                        });
                    }
                }
                if (PublisherPickerStockList != null && PublisherPickerStockList.Any())
                {
                    Publisher.PlannedPublish(PublisherPickerStockList);
                }

            }
            return OrdersForAutoPicks;
        }
        public long GeneratePickerForAutoPickOrder(GeneratePickerAutoPickDc GeneratePickerAutoPickDcs, AuthContext context, TransactionScope scope)
        {

            var ReadyToPickItemLists = new List<ReadyToPickOrderDetails>();
            var DispItemLists = new List<OrderDispatchedDetails>();
            var FreeDispItemLists = new List<OrderDetails>();
            var FreeOrderDetailsIds = new List<int>();
            List<int> OrderIds = GeneratePickerAutoPickDcs.OrderIds;
            var Allorders = context.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId) && x.OrderType != 3).ToList();

            var ReadytopickordersIds = Allorders.Where(x => x.Status == "ReadyToPick").Select(x => x.OrderId).Distinct().ToList();
            if (ReadytopickordersIds != null && ReadytopickordersIds.Any())
            {
                ReadyToPickItemLists = context.ReadyToPickOrderDetailDb.Where(x => ReadytopickordersIds.Contains(x.OrderId)).ToList();
            }
            var DispordersIds = Allorders.Where(x => x.Status == "Ready to Dispatch").Select(x => x.OrderId).Distinct().ToList();
            if (DispordersIds != null && DispordersIds.Any())
            {
                DispItemLists = context.OrderDispatchedDetailss.Where(x => DispordersIds.Contains(x.OrderId)).ToList();
                FreeOrderDetailsIds = DispItemLists.Where(x => x.IsFreeItem == true).Select(x => x.OrderDetailsId).Distinct().ToList();
            }
            if (FreeOrderDetailsIds != null && FreeOrderDetailsIds.Any())
            {
                FreeDispItemLists = context.DbOrderDetails.Where(x => FreeOrderDetailsIds.Contains(x.OrderDetailsId)).ToList();
            }
            List<OrderPickerDetails> LineItems = new List<OrderPickerDetails>();
            OrderPickerMaster orderPickerMaster = new OrderPickerMaster();
            orderPickerMaster.PickerPersonId = GeneratePickerAutoPickDcs.CreatedBy;
            orderPickerMaster.ClusterId = 0; //GeneratePickerAutoPickDcs.ClusterId;
            orderPickerMaster.WarehouseId = GeneratePickerAutoPickDcs.WarehouseId;
            orderPickerMaster.DBoyId = GeneratePickerAutoPickDcs.DboyId;
            orderPickerMaster.CreatedBy = GeneratePickerAutoPickDcs.CreatedBy;
            orderPickerMaster.CreatedDate = DateTime.Now;
            orderPickerMaster.IsActive = true;
            orderPickerMaster.IsDeleted = false;
            orderPickerMaster.Status = 0;
            orderPickerMaster.RePickingCount = 0;
            orderPickerMaster.orderPickerDetails = new List<OrderPickerDetails>();
            foreach (var OrderId in OrderIds.Distinct())
            {
                if (ReadyToPickItemLists != null && ReadyToPickItemLists.Any(x => x.OrderId == OrderId))
                {
                    var ItemLists = ReadyToPickItemLists.Where(c => c.OrderId == OrderId).Select(x => new OrderPickerDetails
                    {
                        ItemMultiMrpId = x.ItemMultiMrpId,
                        ItemName = x.Itemname,
                        OrderDetailsId = x.OrderDetailsId,
                        OrderId = x.OrderId,
                        Qty = x.Qty,
                        IsFreeItem = x.IsFreeItem,
                        IsDispatchedFreeStock = x.IsDispatchedFreeStock,
                        QtyChangeReason = ""
                    }).ToList();
                    LineItems.AddRange(ItemLists);
                }
                else
                {
                    var ItemLists = DispItemLists.Where(c => c.OrderId == OrderId).Select(x => new OrderPickerDetails
                    {
                        ItemMultiMrpId = x.ItemMultiMRPId,
                        ItemName = x.itemname,
                        OrderDetailsId = x.OrderDetailsId,
                        OrderId = x.OrderId,
                        Qty = x.qty,
                        IsFreeItem = x.IsFreeItem,
                        IsDispatchedFreeStock = FreeDispItemLists.Count > 0 ? FreeDispItemLists.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).IsDispatchedFreeStock : false,
                        QtyChangeReason = ""
                    }).ToList();
                    LineItems.AddRange(ItemLists);
                }
            }
            orderPickerMaster.orderPickerDetails = LineItems;
            context.OrderPickerMasterDb.Add(orderPickerMaster);
            if (context.Commit() > 0)
            {
                return orderPickerMaster.Id;
            }
            return 0;
        }
        public List<int> GenerateAssigmentForAutoPick(List<OrderDispatchedMaster> OrderDispatchedMasterList, List<OrderMaster> OrderMasterList, List<OrderDetails> OrderDetailsList, OrderPickerMaster pickerMaster, AuthContext context, TransactionScope dbContextTransaction, ApprovedDispatchedDC ReviewerApprovedAndDispatchedobj, long tripMasterId, People Dboyinfo, double TotalAssignmentAmount, List<int> OrderIds)
        {
            List<int> assignmentIds = new List<int>();
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            if (OrderIds != null && OrderIds.Any())
            {
                var ProcOrderIds = OrderIds.Distinct().ToList();
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Columns.Add("IntValue");
                foreach (var item in ProcOrderIds)
                {
                    var dr = dt.NewRow();
                    dr["IntValue"] = item;
                    dt.Rows.Add(dr);
                }
                var param = new SqlParameter("orderids", dt);
                param.SqlDbType = System.Data.SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";
                var OrderDispatchedDetailssList = context.Database.SqlQuery<OrderDispatchedDetailsDC>("Exec Operation.GetItemAutoPickAssignment @orderids", param).ToList();

                var separateList = OrderDispatchedDetailssList.GroupBy(x => new { x.OrderId })
                      .Select(x => new
                      {
                          OrderId = x.Key.OrderId,
                          subcatid = x.All(z => z.PrepareSeparateAssignment == false) || (x.All(z => z.PrepareSeparateAssignment == true) && x.Select(z => z.SubCategoryId).Distinct().Count() > 1) ? 0 :
                                     x.All(z => z.PrepareSeparateAssignment == true) && x.Select(z => z.SubCategoryId).Distinct().Count() == 1
                                     ? x.FirstOrDefault().SubCategoryId : 0
                      }).ToList();


                List<DeliveryIssuance> DeliveryIssuanceList = separateList.GroupBy(x => x.subcatid)
                    .Select(g => new DeliveryIssuance
                    {
                        userid = Dboyinfo.PeopleID,
                        WarehouseId = Dboyinfo.WarehouseId,
                        DisplayName = Dboyinfo.DisplayName,
                        PeopleID = Dboyinfo.PeopleID,
                        AgentId = ReviewerApprovedAndDispatchedobj.AgentId,
                        Cityid = Dboyinfo.Cityid ?? 0,
                        CreatedDate = indianTime,
                        UpdatedDate = indianTime,
                        TripPlannerConfirmedMasterId = tripMasterId,
                        details = OrderDispatchedDetailssList.Where(z => g.Select(s => s.OrderId).Distinct().ToList().Contains(z.OrderId))
                                    .GroupBy(y => new { y.ItemMultiMRPId, y.IsFreeItem }).Select(t =>
                                     new IssuanceDetails
                                     {
                                         OrderId = string.Join(",", t.Select(s => s.OrderId).Distinct()),
                                         OrderQty = string.Join(",", t.Select(a => String.Format("{0} - {1}", a.OrderId, a.qty)).ToArray()),
                                         OrderDispatchedMasterId = t.Select(x => x.OrderDispatchedMasterId).FirstOrDefault(),
                                         OrderDispatchedDetailsId = t.Select(x => x.OrderDispatchedDetailsId).FirstOrDefault(),
                                         qty = t.Sum(x => x.qty),
                                         itemNumber = t.FirstOrDefault().itemNumber,
                                         ItemId = t.FirstOrDefault().ItemId,
                                         itemname = t.FirstOrDefault().itemname,
                                         IsFreeItem = t.FirstOrDefault().IsFreeItem
                                     }).ToList(),
                        TotalAssignmentAmount = tripPlannerHelper.OrderDispatchAmount(g.Select(s => s.OrderId).Distinct().ToList(), OrderDispatchedMasterList),
                        OrderdispatchIds = string.Join(",", OrderDispatchedDetailssList.Where(z => g.Select(s => s.OrderId).Distinct().ToList().Contains(z.OrderId)).Select(x => x.OrderDispatchedMasterId).Distinct()),
                        OrderIds = string.Join(",", g.Select(s => s.OrderId).Distinct()),
                        Status = "Assigned",
                        IsActive = true,
                        AssignmentType = 5
                    }).ToList();

                context.DeliveryIssuanceDb.AddRange(DeliveryIssuanceList);
                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(pickerMaster.Id).Append($" for assignment commit start : {DateTime.Now}").ToString());

                context.Commit();

                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(pickerMaster.Id).Append($" for assignment commit end : {DateTime.Now}").ToString());


                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(pickerMaster.Id).Append($" for CreateAssignmentForAutoPick Start : {DateTime.Now}").ToString());

                List<bool> isAssignmentCreatedList = new List<bool>();
                foreach (var item in DeliveryIssuanceList)
                {
                    List<int> DispOrderIds = item.OrderIds.Split(',').Select(x => Convert.ToInt32(x)).Distinct().ToList();

                    bool isAssignmentCreated = CreateAssignmentForAutoPick(OrderDispatchedMasterList, OrderMasterList, OrderDetailsList, context, Dboyinfo, Dboyinfo, dbContextTransaction, item, DispOrderIds, pickerMaster.Id);
                    if (!isAssignmentCreated)
                    {

                        return null;
                    }
                    isAssignmentCreatedList.Add(isAssignmentCreated);
                    if (isAssignmentCreated)
                    {
                        assignmentIds.Add(item.DeliveryIssuanceId);
                    }
                }
                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(pickerMaster.Id).Append($" for CreateAssignmentForAutoPick end : {DateTime.Now}").ToString());

                pickerMaster.Status = 6; //Dispatched(ApprovedDispatched),
                pickerMaster.IsDispatched = true;
                pickerMaster.ModifiedDate = indianTime;
                pickerMaster.ModifiedBy = 1;
                pickerMaster.DBoyId = Dboyinfo.PeopleID;//
                pickerMaster.AgentId = DeliveryIssuanceList.FirstOrDefault().AgentId;//
                pickerMaster.MultiDeliveryIssuanceIds = string.Join(",", DeliveryIssuanceList.Select(s => s.DeliveryIssuanceId));
                pickerMaster.TotalAssignmentAmount = DeliveryIssuanceList.Sum(s => s.TotalAssignmentAmount);
                context.Entry(pickerMaster).State = EntityState.Modified;
                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(pickerMaster.Id).Append($" for CreateAssignmentForAutoPick start Commit : {DateTime.Now}").ToString());

                context.Commit();
                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(pickerMaster.Id).Append($" for CreateAssignmentForAutoPick end commit: {DateTime.Now}").ToString());

            }

            return assignmentIds;
        }
        public bool CreateAssignmentForAutoPick(List<OrderDispatchedMaster> OrderDispatchedMasterList, List<OrderMaster> OrderMasterList, List<OrderDetails> OrderDetailsList, AuthContext context, People peopledata, People DBoypeople, TransactionScope scope, DeliveryIssuance obj, List<int> orderids, long PickerId)//add issuance
        {
            MultiStockHelper<OnIssuedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnIssuedStockEntryDc>();
            List<OnIssuedStockEntryDc> OnIssuedStockEntryList = new List<OnIssuedStockEntryDc>();
            #region Assignemnt  Delivery History
            OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
            AssginDeli.DeliveryIssuanceId = obj.DeliveryIssuanceId;
            AssginDeli.Cityid = obj.Cityid;
            AssginDeli.city = obj.city;
            AssginDeli.DisplayName = obj.DisplayName;
            AssginDeli.Status = obj.Status;
            AssginDeli.WarehouseId = obj.WarehouseId;
            AssginDeli.PeopleID = obj.PeopleID;
            AssginDeli.VehicleId = obj.VehicleId;
            AssginDeli.VehicleNumber = obj.VehicleNumber;
            AssginDeli.RejectReason = obj.RejectReason;
            AssginDeli.OrderdispatchIds = obj.OrderdispatchIds;
            AssginDeli.OrderIds = obj.OrderIds;
            AssginDeli.Acceptance = obj.Acceptance;
            AssginDeli.IsActive = obj.IsActive;
            AssginDeli.IdealTime = obj.IdealTime;
            AssginDeli.TravelDistance = obj.TravelDistance;
            AssginDeli.CreatedDate = indianTime;
            AssginDeli.UpdatedDate = indianTime;
            AssginDeli.userid = peopledata.PeopleID;
            if (peopledata.DisplayName == null)
            {
                AssginDeli.UpdatedBy = peopledata.PeopleFirstName;
            }
            else
            {
                AssginDeli.UpdatedBy = peopledata.DisplayName;
            }
            context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
            #endregion
            foreach (var od in orderids)
            {
                var OrderDMaster = OrderDispatchedMasterList.FirstOrDefault(x => x.OrderId == od);

                foreach (var item in OrderDispatchedMasterList.Where(x => x.OrderId == od).ToList())
                {
                    item.Status = "Issued";
                    item.ReDispatchedStatus = "Issued";
                    item.UpdatedDate = indianTime;
                    item.DeliveryIssuanceIdOrderDeliveryMaster = obj.DeliveryIssuanceId;
                    context.Entry(item).State = EntityState.Modified;
                }
                #region Code For OrderDeliveryMaster

                OrderDeliveryMaster oDm = new OrderDeliveryMaster();
                oDm.OrderId = OrderDMaster.OrderId;
                oDm.CityId = OrderDMaster.CityId;
                oDm.CompanyId = OrderDMaster.CompanyId;
                oDm.WarehouseId = OrderDMaster.WarehouseId;
                oDm.WarehouseName = OrderDMaster.WarehouseName;
                oDm.DboyMobileNo = OrderDMaster.DboyMobileNo;
                oDm.DboyName = OrderDMaster.DboyName;
                oDm.CustomerId = OrderDMaster.CustomerId;
                oDm.CustomerName = OrderDMaster.CustomerName;
                oDm.Customerphonenum = OrderDMaster.Customerphonenum;
                oDm.ShopName = OrderDMaster.ShopName;
                oDm.Skcode = OrderDMaster.Skcode;
                oDm.Status = "Issued"; //OrderDMaster.Status;
                oDm.ShippingAddress = OrderDMaster.ShippingAddress;
                oDm.BillingAddress = OrderDMaster.BillingAddress;
                oDm.CanceledStatus = OrderDMaster.CanceledStatus;
                oDm.invoice_no = OrderDMaster.invoice_no;
                oDm.OnlineServiceTax = OrderDMaster.OnlineServiceTax;
                oDm.TotalAmount = OrderDMaster.TotalAmount;
                oDm.GrossAmount = OrderDMaster.GrossAmount;
                oDm.TaxAmount = OrderDMaster.TaxAmount;
                oDm.SGSTTaxAmmount = OrderDMaster.SGSTTaxAmmount;
                oDm.CGSTTaxAmmount = OrderDMaster.CGSTTaxAmmount;
                oDm.ReDispatchedStatus = OrderDMaster.ReDispatchedStatus;
                oDm.Trupay = OrderDMaster.Trupay;
                oDm.comments = OrderDMaster.comments;
                oDm.deliveryCharge = OrderDMaster.deliveryCharge;
                oDm.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                oDm.DiscountAmount = OrderDMaster.DiscountAmount;
                oDm.CheckNo = OrderDMaster.CheckNo;
                oDm.CheckAmount = OrderDMaster.CheckAmount;
                oDm.ElectronicPaymentNo = OrderDMaster.ElectronicPaymentNo;
                oDm.ElectronicAmount = OrderDMaster.ElectronicAmount;
                oDm.EpayLaterAmount = 0;
                oDm.CashAmount = OrderDMaster.CashAmount;
                oDm.OrderedDate = OrderDMaster.OrderedDate;
                oDm.WalletAmount = OrderDMaster.WalletAmount;
                oDm.RewardPoint = OrderDMaster.RewardPoint;
                oDm.Tin_No = OrderDMaster.Tin_No;
                oDm.ReDispatchCount = OrderDMaster.ReDispatchCount;
                oDm.UpdatedDate = indianTime;
                oDm.CreatedDate = indianTime;
                context.OrderDeliveryMasterDB.Add(oDm);
                #endregion


                foreach (var item in OrderMasterList.Where(x => x.OrderId == od).ToList())
                {
                    item.Status = "Issued";
                    item.UpdatedDate = indianTime;
                    context.Entry(item).State = EntityState.Modified;

                }
                foreach (var item in OrderDetailsList.Where(x => x.OrderId == od).ToList())
                {
                    item.Status = "Issued";
                    item.UpdatedDate = indianTime;
                    context.Entry(item).State = EntityState.Modified;

                }

                //for Issued
                //MultiStockHelper<OnIssuedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnIssuedStockEntryDc>();
                //List<OnIssuedStockEntryDc> OnIssuedStockEntryList = new List<OnIssuedStockEntryDc>();
                foreach (var StockHit in OrderDMaster.orderDetails.Where(x => x.qty > 0))
                {
                    //var RefStockCode = (OrderMasterList.FirstOrDefault(z => z.OrderId == StockHit.OrderId).OrderType == 8) ? "CL" : "C";

                    string RefStockCode = "";
                    if ((OrderMasterList.FirstOrDefault(z => z.OrderId == StockHit.OrderId).OrderType == 8))
                    {
                        RefStockCode = "CL";
                    }
                    //else if(OrderMasterList.FirstOrDefault(z => z.OrderId == StockHit.OrderId).OrderType == 10)
                    //{
                    //    RefStockCode = "NR";
                    //}
                    else
                    {
                        RefStockCode = "C";
                    }


                    bool isFree = OrderDetailsList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                    if (isFree) { RefStockCode = "F"; }
                    else if (OrderMasterList.FirstOrDefault(z => z.OrderId == StockHit.OrderId).OrderType == 6) //6 Damage stock
                    {
                        RefStockCode = "D";
                    }
                    //else if (OrderMasterList.FirstOrDefault(z => z.OrderId == StockHit.OrderId).OrderType == 10) //10 NON Revenue stock
                    //{
                    //    RefStockCode = "NR";
                    //}
                    bool IsDeliveryRedispatch = false;
                    if (OrderMasterList.FirstOrDefault(z => z.OrderId == StockHit.OrderId).ReDispatchCount > 0)
                    {
                        IsDeliveryRedispatch = true;
                    }

                    OnIssuedStockEntryList.Add(new OnIssuedStockEntryDc
                    {
                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                        OrderId = StockHit.OrderId,
                        Qty = StockHit.qty,
                        UserId = peopledata.PeopleID,
                        WarehouseId = StockHit.WarehouseId,
                        IsDeliveryRedispatch = IsDeliveryRedispatch,
                        RefStockCode = RefStockCode,
                    });
                }
                //if (OnIssuedStockEntryList.Any())
                //{
                //    bool res = MultiStockHelpers.MakeEntry(OnIssuedStockEntryList, "Stock_OnIssued", context, scope);
                //    if (!res)
                //    {
                //        return false;
                //    }
                //}

                //if (context.Commit() > 0)
                //{
                //}
                //else { return false; };
            }

            TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for MakeBulkEntryOnPickerIssued start  : {DateTime.Now}").ToString());

            if (OnIssuedStockEntryList.Any())
            {
                bool res = MultiStockHelpers.MakeBulkEntryOnPickerIssued(OnIssuedStockEntryList, context);
                if (!res)
                {
                    return false;
                }
            }
            TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for MakeBulkEntryOnPickerIssued end  : {DateTime.Now}").ToString());

            #region insert ordermaster histories
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("IntValue");
            foreach (var item in orderids.Distinct())
            {
                var dr = dt.NewRow();
                dr["IntValue"] = item;
                dt.Rows.Add(dr);
            }
            var paramorderids = new SqlParameter("orderids", dt);
            paramorderids.SqlDbType = System.Data.SqlDbType.Structured;
            paramorderids.TypeName = "dbo.IntValues";

            var paramStatus = new SqlParameter("Status", "Issued");
            var paramReasoncancel = new SqlParameter("Reasoncancel", "");
            var paramWarehousename = new SqlParameter("WarehouseName", OrderDispatchedMasterList.FirstOrDefault().WarehouseName);
            var paramusername = new SqlParameter("username", peopledata.DisplayName != null ? peopledata.DisplayName : peopledata.PeopleFirstName);
            var paramuserid = new SqlParameter("userid", System.Data.SqlDbType.Int);
            paramuserid.Value = peopledata.PeopleID;
            var paramDeliveryIssuanceId = new SqlParameter("DeliveryIssuanceId", System.Data.SqlDbType.Int);
            paramDeliveryIssuanceId.Value = obj.DeliveryIssuanceId;
            var paramIsReAttempt = new SqlParameter("IsReAttempt", false);

            var paramDescription = new SqlParameter("Description", peopledata.DisplayName != null ? (" (Issued AssignmentId : " + obj.DeliveryIssuanceId + ") By" + peopledata.DisplayName) : (" (Issued AssignmentId : " + obj.DeliveryIssuanceId + ") By" + peopledata.PeopleFirstName));


            TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for InsertOrderMasterHistories Issued Start : {DateTime.Now}").ToString());

            int IsOrderMasterHistories = context.Database.ExecuteSqlCommand("Picker.InsertOrderMasterHistories @userid, @DeliveryIssuanceId, @IsReAttempt, @orderids, @Status, @Reasoncancel, @WarehouseName, @username, @Description ",
                paramuserid, paramDeliveryIssuanceId, paramIsReAttempt, paramorderids, paramStatus, paramReasoncancel, paramWarehousename, paramusername, paramDescription);


            TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for InsertOrderMasterHistories Issued end : {DateTime.Now}").ToString());


            TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Issued commit  start  : {DateTime.Now}").ToString());

            if (context.Commit() > 0)
            {
                TextFileLogHelper.TraceLog(new StringBuilder("Picker Id: ").Append(PickerId).Append($" for Issued commit end  : {DateTime.Now}").ToString());

            }
            else { return false; };
            #endregion

            return true;
        }
        public temOrderQBcode GetBarcodeAsyc(string OrderId)
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

        #endregion
        #region Zila Order
        public ResMsg ZilaReadyToPickDispatchedNEWAsync(List<OrderDispatchedMaster> postOrderDispatch, int userid, OrderPickerMaster orderPickerMaster, long? TripMasterId, People Dboyinfo, List<PickerCustomerDc> PickerCustomerlist, AuthContext authContext, TransactionScope dbContextTransaction)
        {

            ResMsg response = new ResMsg();
            List<TCSCustomer> lstTCSCust = new List<TCSCustomer>();
            var outList = new List<OutDc>();
            OrderOutPublisher Publisher = new OrderOutPublisher();
            List<BatchCodeSubjectDc> OrderInvoiceQueue = new List<BatchCodeSubjectDc>();
            {
                //using (var authContext = new AuthContext())
                {
                    int whid = postOrderDispatch.FirstOrDefault().WarehouseId;
                    var wh = authContext.Warehouses.FirstOrDefault(x => x.WarehouseId == whid);
                    List<OrderDispatchedMaster> OrderDispatchedMasterList = new List<OrderDispatchedMaster>();
                    double TotalAssignmentAmount = 0;
                    int stateId = 0;
                    stateId = wh.Stateid;
                    var param = new SqlParameter("PickerId", orderPickerMaster.Id);
                    List<OnRTDOrderPickerDetailDC> OnRTDOrderPickerDetails = authContext.Database.SqlQuery<OnRTDOrderPickerDetailDC>("exec Picker.GetOnRTDPickerDetails @PickerId", param).ToList();

                    if (orderPickerMaster.Id > 0 && Dboyinfo != null)
                    {
                        var OrderIds = postOrderDispatch.Select(x => x.OrderId).Distinct().ToList();
                        var OrderMasterlist = authContext.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                        var OrderDetailsList = authContext.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                        var OrderDispatchedMasterlist = authContext.OrderDispatchedMasters.Where(x => OrderIds.Contains(x.OrderId) && x.WarehouseId == wh.WarehouseId).Include(x => x.orderDetails).ToList();
                        var Orderpaymentlist = authContext.PaymentResponseRetailerAppDb.Where(z => OrderIds.Contains(z.OrderId) && z.status == "Success").ToList();
                        MultiStockHelper<RTDOnPickedDc> MultiStockHelpers = new MultiStockHelper<RTDOnPickedDc>();
                        List<RTDOnPickedDc> RTDOnPickedList = new List<RTDOnPickedDc>();
                        #region TCS Calculate
                        string fy = (indianTime.Month >= 4 ? indianTime.Year + 1 : indianTime.Year).ToString();
                        #endregion                        
                        foreach (var Order in postOrderDispatch)
                        {
                            var cust = PickerCustomerlist.FirstOrDefault(x => x.CustomerId == Order.CustomerId);

                            OrderMaster omCheck = new OrderMaster();
                            List<int> itemids = null;
                            OrderDispatchedMaster ODM = null;
                            omCheck = OrderMasterlist.Where(x => x.OrderId == Order.OrderId).FirstOrDefault();
                            ODM = OrderDispatchedMasterlist.Where(x => x.OrderId == Order.OrderId).FirstOrDefault();
                            bool IsRejectedOrder = OnRTDOrderPickerDetails.Any(x => x.OrderId == Order.OrderId && x.Status != 1);

                            if ((omCheck.Status == "ReadyToPick") && (ODM == null) && !IsRejectedOrder)
                            {
                                itemids = OrderDetailsList.Where(c => c.ItemId > 0 && c.OrderId == Order.OrderId).Select(x => x.ItemId).Distinct().ToList();
                                var itemslist = OnRTDOrderPickerDetails.Where(x => itemids.Contains(x.ItemId) && x.OrderId == omCheck.OrderId).ToList();

                                double finaltotal = 0;
                                double finalTaxAmount = 0;
                                double finalSGSTTaxAmount = 0;
                                double finalCGSTTaxAmount = 0;
                                double finalGrossAmount = 0;
                                double finalTotalTaxAmount = 0;
                                double finalCessTaxAmount = 0;

                                OrderDispatchedMaster dm = Order;
                                OrderDispatchedMasterList.Add(dm);
                                dm.Status = "";
                                dm.CreatedDate = indianTime;
                                dm.UpdatedDate = indianTime;
                                dm.OrderedDate = omCheck.CreatedDate;
                                dm.InvoiceBarcodeImage = omCheck.InvoiceBarcodeImage;

                                dm.DboyMobileNo = TripMasterId > 0 ? Dboyinfo.Mobile : null;
                                dm.DBoyId = TripMasterId > 0 ? Dboyinfo.PeopleID : 0;
                                dm.DboyName = TripMasterId > 0 ? Dboyinfo.DisplayName : null;

                                var orderDetailQtys = OrderDetailsList.Where(z => z.OrderId == Order.OrderId).Sum(z => z.qty);//total order qty
                                var orderdispatchQty = OnRTDOrderPickerDetails.Where(x => x.OrderId == Order.OrderId).Sum(z => z.Qty);//
                                bool isQtyNotChanged = orderDetailQtys == orderdispatchQty;
                                if (!isQtyNotChanged)
                                {

                                    foreach (OrderDispatchedDetails pc in dm.orderDetails)
                                    {
                                        var qty = OnRTDOrderPickerDetails.FirstOrDefault(x => x.OrderDetailsId == pc.OrderDetailsId)?.Qty;//
                                        pc.qty = qty == null ? 0 : qty.Value;
                                        #region calculate free item qty
                                        if (!isQtyNotChanged && pc.IsFreeItem)
                                        {
                                            List<ReadyToDispatchHelper.FreeBillItems> freeBillItems = new List<ReadyToDispatchHelper.FreeBillItems>();
                                            freeBillItems = dm.orderDetails.Select(x => new ReadyToDispatchHelper.FreeBillItems
                                            {
                                                ItemId = x.ItemId,
                                                ItemNumber = x.itemNumber,
                                                OrderdetailId = x.OrderDetailsId,
                                                Qty = x.qty,
                                                UnitPrice = x.UnitPrice,
                                                IsFreeitem = x.IsFreeItem,
                                                OfferId = OrderDetailsList.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId) != null ? OrderDetailsList.FirstOrDefault(y => y.OrderDetailsId == x.OrderDetailsId).OfferId : null
                                            }).ToList();
                                            int ParentItemId = OrderDetailsList.Where(z => z.OrderId == Order.OrderId).FirstOrDefault(x => x.OrderDetailsId == pc.OrderDetailsId).FreeWithParentItemId ?? 0;
                                            if (ParentItemId >= 0)
                                            {
                                                var parent = dm.orderDetails.FirstOrDefault(x => x.ItemId == ParentItemId && x.OrderDetailsId != pc.OrderDetailsId && !x.IsFreeItem);
                                                int ParentOrderDetailsId = parent != null ? parent.OrderDetailsId : 0;
                                                string itemnum = parent != null ? parent.itemNumber : "";
                                                int TotalParentQty = ParentOrderDetailsId > 0 ? OnRTDOrderPickerDetails.FirstOrDefault(x => x.OrderDetailsId == ParentOrderDetailsId).Qty : 0;//
                                                ReadyToDispatchHelper Free = new ReadyToDispatchHelper();
                                                int freeitemqty = Free.getfreebiesitem(Order.OrderId, pc.ItemId, authContext, TotalParentQty, pc.OrderDetailsId, itemnum, freeBillItems);
                                                if (pc.qty <= freeitemqty)
                                                {
                                                    pc.qty = freeitemqty;

                                                }
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
                                            //double temp = pc.TaxPercentage + pc.TotalCessPercentage;
                                            double tempPercentagge3 = pc.TotalCessPercentage + pc.TaxPercentage;
                                            pc.AmtWithoutTaxDisc = ((100 * pc.UnitPrice * pc.qty) / (1 + tempPercentagge3 / 100)) / 100;
                                            pc.AmtWithoutAfterTaxDisc = (100 * pc.AmtWithoutTaxDisc) / (100 + 0);
                                            pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.CessTaxAmount + pc.TaxAmmount;

                                        }
                                        else
                                        {
                                            pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
                                        }


                                        //pc.TotalAmountAfterTaxDisc = pc.AmtWithoutAfterTaxDisc + pc.TaxAmmount;
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
                                }

                                TotalAssignmentAmount += dm.GrossAmount;
                                authContext.OrderDispatchedMasters.Add(dm);


                                authContext.Commit();



                                foreach (var StockHit in dm.orderDetails.Where(x => x.qty > 0))
                                {
                                    string RefStockCode = "";

                                    if (omCheck.OrderType == 8)
                                    {
                                        RefStockCode = "CL";
                                    }

                                    else
                                    {
                                        RefStockCode = "C";
                                    }
                                    bool IsFreeItem = false;
                                    bool isFree = OrderDetailsList.Where(z => z.OrderId == Order.OrderId).Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                    if (isFree) { RefStockCode = "F"; IsFreeItem = true; }

                                    RTDOnPickedList.Add(new RTDOnPickedDc
                                    {
                                        ItemMultiMRPId = StockHit.ItemMultiMRPId,
                                        OrderDispatchedDetailsId = StockHit.OrderDispatchedDetailsId,
                                        OrderId = StockHit.OrderId,
                                        Qty = StockHit.qty,
                                        UserId = userid,
                                        WarehouseId = StockHit.WarehouseId,
                                        IsFreeStock = IsFreeItem,
                                        IsDispatchFromPlannedStock = true,
                                        RefStockCode = RefStockCode
                                    });
                                }

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
                                }
                                omCheck.Status = "Ready to Dispatch";
                                omCheck.ReadytoDispatchedDate = indianTime;
                                omCheck.UpdatedDate = indianTime;
                                omCheck.invoice_no = invoiceNumber;
                                authContext.Entry(omCheck).State = EntityState.Modified;

                                foreach (var ods in OrderDetailsList.Where(z => z.OrderId == Order.OrderId))
                                {
                                    ods.Status = omCheck.Status;
                                    ods.UpdatedDate = indianTime;
                                    authContext.Entry(ods).State = EntityState.Modified;
                                }

                                dm.Status = "Ready to Dispatch";
                                dm.invoice_no = invoiceNumber;
                                finaltotal = finaltotal + dm.deliveryCharge;
                                finalGrossAmount = finalGrossAmount + dm.deliveryCharge;
                                if (!isQtyNotChanged)
                                {
                                    dm.TotalAmount = Math.Round(finaltotal, 2) - dm.WalletAmount.GetValueOrDefault();
                                    dm.TaxAmount = Math.Round(finalTaxAmount, 2);
                                    dm.SGSTTaxAmmount = Math.Round(finalSGSTTaxAmount, 2);
                                    dm.CGSTTaxAmmount = Math.Round(finalCGSTTaxAmount, 2);
                                    dm.GrossAmount = Math.Round((Convert.ToInt32(finalGrossAmount) - dm.WalletAmount.GetValueOrDefault()), 0);
                                }

                                authContext.Entry(dm).State = EntityState.Modified;

                                #region Billdiscountamount
                                var Orderpayments = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId && z.status == "Success").ToList();
                                dm.TCSAmount = omCheck.TCSAmount;

                                double offerDiscountAmount = 0;
                                if (!isQtyNotChanged)
                                {
                                    var billdiscount = authContext.BillDiscountDb.Where(x => x.OrderId == omCheck.OrderId && x.CustomerId == omCheck.CustomerId).ToList();//sp
                                    var offerIds = billdiscount.Select(x => x.OfferId).ToList();
                                    var offers = authContext.OfferDb.Where(x => offerIds.Contains(x.OfferId)).ToList();
                                    List<int> flashdealItems = authContext.FlashDealItemConsumedDB.Where(x => x.OrderId == omCheck.OrderId).Select(x => x.ItemId).ToList();//sp
                                    var billdiscountofferids = billdiscount.Select(x => x.OfferId).ToList();

                                    List<Offer> Offers = authContext.OfferDb.Where(x => billdiscountofferids.Contains(x.OfferId)).Include(x => x.OfferItemsBillDiscounts).Include(x => x.BillDiscountOfferSections).Include(y => y.BillDiscountFreeItems).ToList(); //sp

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
                                                    WalletPoint = Convert.ToInt32((Offer.BillDiscountWallet.HasValue ? Offer.BillDiscountWallet.Value : 0));
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

                                    var CODcnt = Orderpayments.Count(z => z.OrderId == omCheck.OrderId && !z.IsOnline && z.status == "Success");
                                    var Onlinecnt = Orderpayments.Count(z => z.OrderId == omCheck.OrderId && z.IsOnline && z.status == "Success");
                                    bool IsConsumer = (cust.CustomerType != null && cust.CustomerType.ToLower() == "consumer") ? true : false;
                                    if (CODcnt > 0 && !IsConsumer)
                                    {

                                        GetCustomersTotalPurchaseInMongo helper = new GetCustomersTotalPurchaseInMongo();
                                        var tcsConfig = helper.GetCustomersTotalPurchaseForTCS(cust.CustomerId, cust.PanNo, authContext);
                                        if (tcsConfig != null && !cust.IsTCSExemption)
                                        {
                                            var percent = !cust.IsPanVerified ? tcsConfig.NotGSTTCSPercent : tcsConfig.GSTTCSPercent;
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


                                    }
                                    dm.TotalAmount = dm.TotalAmount - offerDiscountAmount + dm.TCSAmount;
                                    dm.BillDiscountAmount = offerDiscountAmount;
                                    dm.GrossAmount = System.Math.Round(dm.TotalAmount, 0);
                                }
                                #endregion
                                #region  //if Gross amount is negative due wallet amount more then dispatched amount 

                                if ((dm.GrossAmount < 0 && dm.WalletAmount > dm.GrossAmount))
                                {

                                    double _RefundDiffValue = Math.Abs(dm.GrossAmount);//Convert to positive
                                    var wallet = authContext.WalletDb.Where(c => c.CustomerId == dm.CustomerId).FirstOrDefault(); //Sp
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

                                        dm.GrossAmount = System.Math.Round(dm.TotalAmount, 0);
                                    }
                                }
                                #endregion
                                //if there is no barcode then genearte barcode in dispatched 
                                if (dm.InvoiceBarcodeImage == null) //byte value
                                {
                                    string Borderid = Convert.ToString(dm.OrderId);
                                    string BorderCodeId = Borderid.PadLeft(11, '0');
                                    temOrderQBcode code = GetBarcodeAsyc(BorderCodeId);
                                    dm.InvoiceBarcodeImage = code.BarcodeImage;
                                }
                                var otherMode = Orderpayments.Where(z => z.OrderId == omCheck.OrderId && z.IsOnline && z.status == "Success").ToList();
                                double otherModeAmt = 0;
                                if (otherMode.Count > 0)
                                {
                                    otherModeAmt = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId && z.IsOnline && z.status == "Success").Sum(x => x.amount);
                                }
                                if (omCheck.GrossAmount != dm.GrossAmount && otherModeAmt == 0)
                                {
                                    var cashOldEntries = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId && !z.IsOnline
                                                             && z.status == "Success").ToList();
                                    if (cashOldEntries != null && cashOldEntries.Any() && !cashOldEntries.Any(x => x.PaymentFrom == "RTGS/NEFT"))
                                    {
                                        foreach (var cash in cashOldEntries)
                                        {
                                            cash.status = "Failed";
                                            cash.statusDesc = "Due to Items cut when Auto Ready to Pick";
                                            authContext.Entry(cash).State = EntityState.Modified;
                                        }
                                    }
                                    // var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp();
                                    if (cashOldEntries.Any(x => x.PaymentFrom == "RTGS/NEFT"))
                                    {
                                        foreach (var VANRTGS in cashOldEntries.Where(x => x.PaymentFrom == "RTGS/NEFT"))
                                        {
                                            VANRTGS.status = "Failed";
                                            VANRTGS.statusDesc = "Due to Items cut when Auto Ready to Pick";
                                            authContext.Entry(VANRTGS).State = EntityState.Modified;
                                        }
                                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
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
                                        authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                    }
                                }
                                else if (omCheck.GrossAmount != dm.GrossAmount && otherModeAmt > 0)
                                {
                                    if (omCheck.paymentThrough.Trim().ToLower() == "paylater")
                                    {
                                        //var paylaterdata = authContext.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == omCheck.OrderId);
                                        //if (paylaterdata != null)
                                        //{
                                        //    var paylaterhistory = authContext.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylaterdata.Id && x.Comment == "Retailer Pay Now").FirstOrDefault();
                                        //    if (paylaterhistory != null)
                                        //    {
                                        //        double totalamount = dm.GrossAmount;
                                        //        double amount = paylaterhistory.Amount;
                                        //        if (amount >= totalamount)
                                        //        {
                                        //            if (amount > totalamount)
                                        //            {
                                        //                PaymentRefundHelper PRHelper = new PaymentRefundHelper();
                                        //                var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                        //                {
                                        //                    amount = (-1) * (amount - totalamount),
                                        //                    CreatedDate = DateTime.Now,
                                        //                    currencyCode = "INR",
                                        //                    OrderId = paylaterdata.OrderId,
                                        //                    PaymentFrom = paylaterhistory.PaymentMode,
                                        //                    GatewayTransId = paylaterhistory.RefNo,
                                        //                    GatewayOrderId = Convert.ToString(paylaterdata.OrderId),
                                        //                    status = "Success",
                                        //                    UpdatedDate = DateTime.Now,
                                        //                    IsRefund = false,
                                        //                    IsOnline = true,
                                        //                    statusDesc = "Refund Initiated"
                                        //                };
                                        //                authContext.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                        //                authContext.Commit();
                                        //                var PaymentRefundRequestDc = new PaymentRefundRequestDc
                                        //                {
                                        //                    Amount = (amount - totalamount),
                                        //                    OrderId = PaymentResponseRetailerAppDb.OrderId,
                                        //                    Source = PaymentResponseRetailerAppDb.PaymentFrom,
                                        //                    ReqGatewayTransId = PaymentResponseRetailerAppDb.GatewayTransId,
                                        //                    Status = (int)PaymentRefundEnum.Initiated,
                                        //                    CreatedBy = userid,
                                        //                    CreatedDate = DateTime.Now,
                                        //                    IsActive = true,
                                        //                    IsDeleted = false,
                                        //                    ModifiedBy = userid,
                                        //                    ModifiedDate = DateTime.Now,
                                        //                    PaymentResponseRetailerAppId = PaymentResponseRetailerAppDb.id
                                        //                };
                                        //                bool IsInserted = PRHelper.InsertPaymentRefundRequest(authContext, PaymentRefundRequestDc);


                                        //                Model.CashManagement.PayLaterCollectionHistory hist = new Model.CashManagement.PayLaterCollectionHistory();
                                        //                hist.Amount = (-1) * (amount - totalamount);
                                        //                hist.PayLaterCollectionId = paylaterdata.Id;
                                        //                hist.CreatedDate = DateTime.Now;
                                        //                hist.CreatedBy = userid;
                                        //                hist.Comment = "Cut Dispatch Refund Online";
                                        //                hist.IsActive = true;
                                        //                hist.IsDeleted = false;
                                        //                hist.CurrencyHubStockId = 0;
                                        //                hist.PaymentMode = "Cash";
                                        //                authContext.PayLaterCollectionHistoryDb.Add(hist);

                                        //                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                        //                paylaterdata.ModifiedDate = DateTime.Now;
                                        //                paylaterdata.ModifiedBy = 1;
                                        //                authContext.Entry(paylaterdata).State = EntityState.Modified;
                                        //            }
                                        //            else
                                        //            {
                                        //                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                        //                paylaterdata.ModifiedDate = DateTime.Now;
                                        //                paylaterdata.ModifiedBy = 1;
                                        //                authContext.Entry(paylaterdata).State = EntityState.Modified;
                                        //            }
                                        //        }
                                        //    }
                                        //}

                                        //var paylaterdata = authContext.PayLaterCollectionDb.FirstOrDefault(x => x.OrderId == omCheck.OrderId );
                                        //if (paylaterdata != null)
                                        //{
                                        //    var paylaterhistory = authContext.PayLaterCollectionHistoryDb.Where(x => x.PayLaterCollectionId == paylaterdata.Id).ToList();
                                        //    if (paylaterhistory != null && paylaterhistory.Any())
                                        //    {
                                        //        double totalamount = dm.GrossAmount;
                                        //        double amount = paylaterhistory.Sum(x => x.Amount);
                                        //        if (amount >= totalamount)
                                        //        {
                                        //            if (amount > totalamount)
                                        //            {
                                        //                Model.CashManagement.PayLaterCollectionHistory hist = new Model.CashManagement.PayLaterCollectionHistory();
                                        //                hist.Amount = (-1) * (amount - totalamount);
                                        //                hist.PayLaterCollectionId = paylaterdata.Id;
                                        //                hist.CreatedDate = DateTime.Now;
                                        //                hist.CreatedBy = userid;
                                        //                hist.Comment = "CutDispatch";
                                        //                hist.IsActive = true;
                                        //                hist.IsDeleted = false;
                                        //                hist.CurrencyHubStockId = 0;
                                        //                hist.PaymentMode = "Cash";
                                        //                authContext.PayLaterCollectionHistoryDb.Add(hist);

                                        //                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                        //                paylaterdata.ModifiedDate = DateTime.Now;
                                        //                paylaterdata.ModifiedBy = 1;
                                        //                authContext.Entry(paylaterdata).State = EntityState.Modified;


                                        //            }
                                        //            else
                                        //            {
                                        //                paylaterdata.Status = Convert.ToInt32(AngularJSAuthentication.API.DataContract.PayCollectionEnum.Paid);
                                        //                paylaterdata.ModifiedDate = DateTime.Now;
                                        //                paylaterdata.ModifiedBy = 1;
                                        //                authContext.Entry(paylaterdata).State = EntityState.Modified;
                                        //            }
                                        //        }
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        if (wh.IsOnlineRefundEnabled)
                                        {
                                            // Cut line item Payment refund  -- April2022
                                            #region Cut line item Payment refund  -- April2022
                                            var RefundDays = authContext.PaymentRefundApis.Where(x => x.DaysForRefundEligible > 0 && x.IsActive && x.IsDeleted == false).Select(x => new { x.ApiName, x.DaysForRefundEligible }).ToList();


                                            // case 1:  Online payment is more than order payment for payment mode in  (chqbook)
                                            if (dm.GrossAmount < Orderpayments.Where(z => z.OrderId == omCheck.OrderId && (z.PaymentFrom == "chqbook") && z.IsOnline && z.status == "Success").Sum(z => z.amount))
                                            {
                                                response = new ResMsg()
                                                {
                                                    Status = false,
                                                    Message = "Can't dispatched due to online payment is more than dispatched amount for Order  : " + omCheck.OrderId
                                                };
                                                //dbContextTransaction.Dispose();
                                                OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                                                return response;
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
                                                        response = new ResMsg()
                                                        {
                                                            Status = false,
                                                            Message = "Can't dispatch cut item order , because online payment refund days expired for Order  : " + omCheck.OrderId
                                                        };
                                                        //dbContextTransaction.Dispose();
                                                        return response;
                                                    }
                                                    else if (PaymentRefundDays == null && item.PaymentFrom.Trim().ToLower() != "gullak")
                                                    {
                                                        response = new ResMsg()
                                                        {
                                                            Status = false,
                                                            Message = "refund apis or refund days not configured for payment mode " + item.PaymentFrom
                                                        };
                                                        //dbContextTransaction.Dispose();
                                                        OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                                                        return response;
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
                                            #region old code Gullak
                                            if (omCheck.OrderType == 4)
                                            {
                                                var gullakOldEntries = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId //&& z.PaymentFrom == "Gullak"
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


                                                if (omCheck.paymentMode == "Gullak")
                                                {
                                                    var oldgullak = gullakOldEntries.FirstOrDefault(z => z.PaymentFrom == "Gullak");
                                                    double RemainingGullakAmount = 0;
                                                    double gullakAmount = 0;
                                                    double cashAmount = 0;
                                                    List<PaymentResponseRetailerApp> GullakPaymentResponseRetailerAppDbs = new List<PaymentResponseRetailerApp>();
                                                    if (oldgullak != null)
                                                    {
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

                                                        GullakPaymentResponseRetailerAppDbs = new List<PaymentResponseRetailerApp> {
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
                                            statusDesc = "Due to Items cut when Ready to Dispatch",
                                            IsOnline=true
                                        }
                                        };
                                                    }
                                                    else
                                                    {
                                                        cashAmount = dm.GrossAmount;
                                                    }
                                                    if (cashAmount > 0)
                                                    {
                                                        GullakPaymentResponseRetailerAppDbs = new List<PaymentResponseRetailerApp> {
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
                                             }};
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
                                                else
                                                {

                                                    var cashOldEntries = Orderpaymentlist.Where(z => z.OrderId == omCheck.OrderId && z.PaymentFrom == "Cash"
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
                                            }
                                            else
                                            {
                                                response = new ResMsg()
                                                {
                                                    Status = false,
                                                    Message = "Order amount and dispatch amount is different.It is not allowed in online payment."
                                                };
                                                // dbContextTransaction.Dispose();
                                                OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                                                return response;
                                                //throw new Exception("Order amount and dispatch amount is different. It is not allowed in online payment.");
                                            }
                                            #endregion
                                        }
                                    }

                                }
                                #region Update IRN Check 
                                IRNHelper irnHelper = new IRNHelper();

                                if (cust.IsGenerateIRN)  //spwithnolock
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

                                authContext.Entry(dm).State = EntityState.Modified;

                                #region if no entry of payment then insert entry in cash 
                                if (!Orderpaymentlist.Any(x => x.OrderId == dm.OrderId && x.status == "Success"))
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
                            }
                            else if ((omCheck.Status == "Ready to Dispatch") && (ODM != null) && !IsRejectedOrder && TripMasterId > 0)
                            {
                                TotalAssignmentAmount += ODM.GrossAmount;
                                ODM.DboyMobileNo = TripMasterId > 0 ? Dboyinfo.Mobile : null;
                                ODM.DBoyId = TripMasterId > 0 ? Dboyinfo.PeopleID : 0;
                                ODM.DboyName = TripMasterId > 0 ? Dboyinfo.DisplayName : null;
                                ODM.UpdatedDate = indianTime;
                                authContext.Entry(ODM).State = EntityState.Modified;

                                OrderDispatchedMasterList.Add(ODM);
                                try
                                {

                                    #region VAN notification
                                    if (Orderpaymentlist.Any(x => x.OrderId == ODM.OrderId && x.status == "Success" && x.PaymentFrom == "RTGS/NEFT" && string.IsNullOrEmpty(x.GatewayTransId)) && cust != null && cust.FcmId != null)
                                    {
                                        ReadyToDispatchHelper readyToDispatchHelper = new ReadyToDispatchHelper();
                                        readyToDispatchHelper.VANForNotification(ODM.OrderId, ODM.GrossAmount, cust);

                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex.Message);
                                }
                            }
                        }
                        bool res = MultiStockHelpers.MakeBulkEntryOnPicker(RTDOnPickedList, authContext);
                        if (!res)
                        {
                            response = new ResMsg()
                            {
                                Status = false,
                                Message = "Can't Dispatched, Due to Dispatched Qty not Available"
                            };
                            dbContextTransaction.Dispose();
                            OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                            return response;
                        }
                        #region insert ordermaster histories
                        System.Data.DataTable dt = new System.Data.DataTable();
                        dt.Columns.Add("IntValue");
                        foreach (var item in OrderIds.Distinct())
                        {
                            var dr = dt.NewRow();
                            dr["IntValue"] = item;
                            dt.Rows.Add(dr);
                        }
                        var paramorderids = new SqlParameter("orderids", dt);
                        paramorderids.SqlDbType = System.Data.SqlDbType.Structured;
                        paramorderids.TypeName = "dbo.IntValues";
                        var paramStatus = new SqlParameter("Status", "Ready to Dispatch");
                        var paramReasoncancel = new SqlParameter("Reasoncancel", "");
                        var paramWarehousename = new SqlParameter("WarehouseName", wh.WarehouseName);
                        var paramusername = new SqlParameter("username", "system");
                        var paramuserid = new SqlParameter("userid", System.Data.SqlDbType.Int);
                        paramuserid.Value = userid;
                        var paramDeliveryIssuanceId = new SqlParameter("DeliveryIssuanceId", System.Data.SqlDbType.Int);
                        paramDeliveryIssuanceId.Value = 0;
                        var paramIsReAttempt = new SqlParameter("IsReAttempt", false);
                        var paramDescription = new SqlParameter("Description", "");
                        var RtdDate = new SqlParameter("RtdDate", indianTime);
                        int IsOrderMasterHistories = authContext.Database.ExecuteSqlCommand("Picker.InsertOrderMasterRTDHistories @userid, @DeliveryIssuanceId, @IsReAttempt, @orderids, @Status, @Reasoncancel, @WarehouseName, @username, @Description ,@RtdDate ",
                            paramuserid, paramDeliveryIssuanceId, paramIsReAttempt, paramorderids, paramStatus, paramReasoncancel, paramWarehousename, paramusername, paramDescription, RtdDate);
                        #endregion                        
                        ApprovedDispatchedDC ApprovedAndDispatched = new ApprovedDispatchedDC();
                        ApprovedAndDispatched.UserId = userid;
                        ApprovedAndDispatched.DeliveryBoyId = Dboyinfo.PeopleID; //OrderPickerMaster.DBoyId ?? 0;
                        ApprovedAndDispatched.AgentId = 1; //OrderPickerMaster.AgentId ?? 0;
                        ApprovedAndDispatched.PickerId = Convert.ToInt32(orderPickerMaster.Id);
                        List<int> AssignmentIdList = GenerateAssigmentForAutoPick(OrderDispatchedMasterList, OrderMasterlist, OrderDetailsList, orderPickerMaster, authContext, dbContextTransaction, ApprovedAndDispatched, 0, Dboyinfo, TotalAssignmentAmount, OrderIds);
                        if (AssignmentIdList == null)
                        {
                            // dbContextTransaction.Dispose();
                            OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                            response = new ResMsg()
                            {
                                Status = false,
                                Message = "Unable to create trip assignmentId for Picker #No :" + orderPickerMaster.Id
                            };
                            return response;
                        }
                        foreach (var AssignmentId in AssignmentIdList)
                        {
                            if (AssignmentId == 0)
                            {
                                //dbContextTransaction.Dispose();
                                OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                                response = new ResMsg()
                                {
                                    Status = false,
                                    Message = "Unable to create trip assignmentId for Picker #No :" + orderPickerMaster.Id
                                };
                                return response;
                            }
                            //// Update tripPickerMapping record by assignmentId  against TripMasterId
                            ////TripPlannerHelper triphelper = new TripPlannerHelper();
                            //bool IsSuccess = triphelper.FinalizeTrip(userid, orderPickerMaster.Id, AssignmentId, TripMasterId.Value, authContext);
                            //if (!IsSuccess)
                            //{
                            //    //dbContextTransaction.Dispose();
                            //    OrderInvoiceQueue.RemoveAll(x => x.ObjectDetailId == stateId);
                            //    response = new ResMsg()
                            //    {
                            //        Status = false,
                            //        Message = "Issue in Finalize Trip for PickerId #No :" + orderPickerMaster.Id
                            //    };
                            //    return response;
                            //}
                        }
                        foreach (var item in orderPickerMaster.orderPickerDetails)
                        {
                            item.Status = 2;
                            authContext.Entry(item).State = EntityState.Modified;
                        }
                        if (authContext.Commit() > 0)
                        {
                            //dbContextTransaction.Complete();
                            response = new ResMsg()
                            {
                                Status = true,
                                Message = "Approved and RTD and Assignment Created NO# :" + string.Join(",", AssignmentIdList)
                            };
                        }
                        else
                        {
                            response = new ResMsg()
                            {
                                Status = false,
                                Message = "#No. " + orderPickerMaster.Id + " something went wrong"
                            };
                            return response;
                        }
                    }
                    else
                    {
                        //dbContextTransaction.Complete();
                        response.Message = "#No. " + orderPickerMaster.Id + "  Approved and RTD Now Create Assignment";
                    }
                }
            }
            if (OrderInvoiceQueue != null && OrderInvoiceQueue.Any())
            {
                Publisher.PublishOrderInvoiceQueue(OrderInvoiceQueue);
            }
            return response;
        }
        #endregion
    }
}
