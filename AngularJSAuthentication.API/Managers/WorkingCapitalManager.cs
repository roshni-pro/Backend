using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.DataContracts.Transaction.Reports;
using AngularJSAuthentication.Model.CashManagement;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers
{
    public class WorkingCapitalManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public bool UpdateWorkingCapital()
        {

            bool result = false;
            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
            MongoDbHelper<WorkingCapitalData> mongoDbHelperWorkingCapital = new MongoDbHelper<WorkingCapitalData>();
            var orderMasters = new List<MongoOrderMaster>();
            var salesorderMasters = new List<MongoOrderMaster>();
            var orderStatusAmount = new List<OrderStatusAmount>();
            var orderNotReconsileAmount = new List<OrderStatusAmount>();
            var salesorderStatusAmount = new List<OrderStatusAmount>();
            var hubInventory = new List<HubAvgInventory>();
            var CashInOperation = new List<WarehouseCashAmount>();
            var WarehouseChequeAmount = new List<WarehouseChequeAmount>();
            var WarehouseSupplierAmount = new List<WarehouseCashAmount>();
            var WarehouseSupplierAdvanceAmount = new List<WarehouseCashAmount>();
            var WarehouseIRPendingBuyerAmount = new List<WarehouseCashAmount>();
            var WarehouseAgentAmount = new List<WarehouseCashAmount>();
            var DamageInventory = new List<WarehouseCashAmount>();
            var NonSellableInventory = new List<WarehouseCashAmount>();
            var PendingGRN = new List<WarehouseCashAmount>();
            var GoodsReceivedNotInvoiced = new List<WarehouseCashAmount>();
            var WarehouseOnlinAmount = new List<WarehouseCashAmount>();
            var onlinePaymentV2WithpaymentFrom = new List<OnlinePaymentV2WithpaymentFrom>();
            DateTime WCDate = DateTime.Now.AddDays(-1).Date;
            var startDate = new DateTime(WCDate.Year, WCDate.Month, WCDate.Day);
            var endDate = startDate.AddDays(1).AddMilliseconds(-1);
            var taskList = new List<Task>();
            var AssignmentList = new List<int>();
            bool isGenerated = mongoDbHelperWorkingCapital.Select(x => x.CreateDate == WCDate
                                , collectionName: "WorkingCapitalData")
                                .Any();

            //isGenerated = false;
            if (!isGenerated)
            {


                var task1 = Task.Factory.StartNew(() =>
                {
                    //orderMasters = mongoDbHelper.Select(x => !x.Deleted && (x.Status == "Shipped" || x.Status == "Issued" || x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch"
                    //                                    || x.Status == "Delivery Canceled")
                    //                , collectionName: "OrderMaster")
                    //                .ToList();



                    using (var context = new AuthContext())
                    {
                        //AssignmentList = context.DeliveryIssuanceDb.Where(x => x.Status == "Payment Submitted" || x.Status == "Payment Accepted" || x.Status == "Submitted").Select(x => x.DeliveryIssuanceId).ToList();
                        //var ReconciledOrder = orderMasters.Where(x => x.DeliveryIssuanceIdOrderDeliveryMaster.HasValue && AssignmentList.Contains(x.DeliveryIssuanceIdOrderDeliveryMaster.Value) && x.Status== "Delivered").Select(x => new { x.WarehouseId, x.OrderId, Totalamount = (x.DispatchAmount ?? x.GrossAmount) }).ToList();
                        //var OrderIds = ReconciledOrder.Select(x => x.OrderId).ToList();
                        //orderNotReconsileAmount = ReconciledOrder.GroupBy(x => x.WarehouseId).Select(x => new OrderStatusAmount { WarehouseId = x.Key, TotalAmount = x.Sum(z => z.Totalamount) }).ToList();
                        context.Database.CommandTimeout = 600;
                        var query = "select a.WarehouseId,sum(a.GrossAmount) TotalAmount from OrderDispatchedMasters a with (nolock) inner join DeliveryIssuances b with (nolock) on a.DeliveryIssuanceIdOrderDeliveryMaster=b.DeliveryIssuanceId and b.Status in ('Payment Submitted','Payment Accepted','Submitted') and a.Status ='Delivered'  group by a.WarehouseId";
                        orderNotReconsileAmount = context.Database.SqlQuery<OrderStatusAmount>(query).ToList();
                        query = "select a.WarehouseId,a.Status,sum(a.GrossAmount) TotalAmount from OrderDispatchedMasters a with (nolock) where a.Status in ('Shipped','Issued','Ready to Dispatch','Delivery Redispatch','Delivery Canceled') and a.active=1 and a.Deleted=0 group by a.WarehouseId,a.Status";
                        orderStatusAmount = context.Database.SqlQuery<OrderStatusAmount>(query).ToList();

                        //orderStatusAmount = orderMasters.GroupBy(x => new { x.WarehouseId, x.Status }).Select(x => new OrderStatusAmount
                        //{
                        //    WarehouseId = x.Key.WarehouseId,
                        //    Status = x.Key.Status,
                        //    TotalAmount = x.Sum(z => z.DispatchAmount ?? z.GrossAmount)
                        //}).ToList();
                    }
                });

                taskList.Add(task1);


                var task2 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        //var query = "select c.WarehouseId, sum(c.CurrentInventory * item.purchasePrice) as Inventory from CurrentStocks c with (nolock) inner join GMWarehouseProgresses gw with (nolock) on c.WarehouseId=gw.WarehouseID and gw.IsLaunched=1 cross apply(select max(i.netpurchaseprice) purchasePrice from  itemmasters i  with (nolock) where c.WarehouseId = i.WarehouseId and c.ItemNumber = i.Number and c.ItemMultiMRPId = i.ItemMultiMRPId and i.Deleted=0 group by i.WarehouseId, i.ItemMultiMRPId ) item group by c.WarehouseId";
                        //var query = "select C.WarehouseId,sum(remqty * case when cancelinpp >0 then cancelinpp else price end) as Inventory from inqueue c with (nolock)  where cast(createddate as date) =  cast(DATEADD(day,-1,getdate()) as date) group by c.WarehouseId";
                        var query = "Exec GetInventoryForWorkingCapital";
                        hubInventory = context.Database.SqlQuery<HubAvgInventory>(query).ToList();
                    }
                });

                taskList.Add(task2);

                var task3 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        var query = "select c.Warehouseid,Cast(sum(a.openingcurrencycount * b.Value) as decimal(18,2)) as Amount  from CurrencyHubStocks c with(nolock) inner join  HubCashCollections a  with(nolock)  on c.id=a.CurrencyHubStockId and cast(c.BOD as date)=cast(getdate() as date) inner join CurrencyDenominations b  with(nolock)  on a.CurrencyDenominationId=b.Id group by c.Warehouseid";
                        CashInOperation = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });

                taskList.Add(task3);

                var task4 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        var query = "select a.ChequeStatus,Cast(Sum(a.ChequeAmt) as decimal(18,2)) as Amount,b.WarehouseId from ChequeCollections a  with(nolock) inner join  ordermasters b  with(nolock) on a.orderid=b.OrderId and b.Status<>'sattled'  where a.ChequeStatus in (1,2,4) and a.IsActive=1 and (a.IsDeleted is null or a.IsDeleted=0) group by b.WarehouseId,a.ChequeStatus";
                        WarehouseChequeAmount = context.Database.SqlQuery<WarehouseChequeAmount>(query).ToList();
                    }
                });

                taskList.Add(task4);
                var task5 = Task.Factory.StartNew(() =>
                {

                    //salesorderMasters = mongoDbHelper.Select(x => !x.Deleted && (x.Status != "Payment Pending" && x.Status != "Inactive" && x.Status != "Failed" && x.Status != "Dummy Order Cancelled" )
                    //                       && x.CreatedDate >= startDate && x.CreatedDate <= endDate, collectionName: "OrderMaster")
                    //                        .ToList();


                    //salesorderStatusAmount = salesorderMasters.GroupBy(x => new { x.WarehouseId }).Select(x => new OrderStatusAmount
                    //{
                    //    WarehouseId = x.Key.WarehouseId,
                    //    TotalAmount = x.Sum(z => z.GrossAmount + (z.BillDiscountAmount.HasValue ? z.BillDiscountAmount.Value : 0) + (z.WalletAmount.HasValue ? z.WalletAmount.Value : 0))
                    //}).ToList();


                    using (var context = new AuthContext())
                    {

                        context.Database.CommandTimeout = 600;
                        var query = "Select WarehouseId,OrderId,(grossamount + isnull(BillDiscountAmount,0)+ isnull(WalletAmount,0) - isnull(TCSAmount,0)) DispatchAmount from OrderMasters with (nolock) where status not In ('Payment Pending','Inactive','Failed','Dummy Order Cancelled')  and active=1 and Deleted=0 and CreatedDate >= '" + startDate.ToString("yyyy-MM-dd HH:mm:ss") + "' and CreatedDate <= '" + endDate.ToString("yyyy-MM-dd HH:mm:ss") + "' ";

                        List<OrderSummaryStatusWise> OrderSummaryStatusWise = context.Database.SqlQuery<OrderSummaryStatusWise>(query).ToList();
                        salesorderStatusAmount = OrderSummaryStatusWise != null && OrderSummaryStatusWise.Any() ? OrderSummaryStatusWise.GroupBy(x => x.WarehouseId).Select(x => new OrderStatusAmount { WarehouseId = x.Key, TotalAmount = x.Sum(y => y.DispatchAmount.Value) }).ToList() : new List<OrderStatusAmount>();


                        //var orderIdDt = new DataTable();
                        //orderIdDt.Columns.Add("IntValue");
                        //foreach (var item in OrderSummaryStatusWise)
                        //{
                        //    var dr = orderIdDt.NewRow();
                        //    dr["IntValue"] = item.OrderId;
                        //    orderIdDt.Rows.Add(dr);
                        //}
                        //var param = new SqlParameter("orderIds", orderIdDt);
                        //param.SqlDbType = SqlDbType.Structured;
                        //param.TypeName = "dbo.IntValues";
                        //WarehouseOnlinAmount = context.Database.SqlQuery<WarehouseCashAmount>("exec GetOnlineOrderPayment @orderIds", param).ToList();

                        //var param2 = new SqlParameter("orderIds", orderIdDt);
                        //param2.SqlDbType = SqlDbType.Structured;
                        //param2.TypeName = "dbo.IntValues";
                        onlinePaymentV2WithpaymentFrom = context.Database.SqlQuery<OnlinePaymentV2WithpaymentFrom>("exec OnlinePaymentV2WithpaymentFrom").ToList();

                        onlinePaymentV2WithpaymentFrom = onlinePaymentV2WithpaymentFrom.GroupBy(x => new { x.paymentFrom, x.WarehouseId }).Select(y => new OnlinePaymentV2WithpaymentFrom
                        {
                            paymentFrom = y.Key.paymentFrom,
                            Amount = y.Sum(z => z.Amount),
                            WarehouseId = y.Key.WarehouseId
                        }).ToList();


                    }

                });

                taskList.Add(task5);

                var task6 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        //var query = "select Cast(Sum(IsNull(LE.Credit, 0))as decimal(18,2))-Cast(Sum(IsNull(LE.Debit, 0))as decimal(18,2)) as Amount ,isnull(ir.WarehouseId,0) WarehouseId    from LadgerEntries LE INNER JOIN Ladgers L On L.ID = LE.LagerID   and l.ObjectType ='Supplier'  left join IRMasters ir on ir.id=le.ObjectID group by ir.WarehouseId";
                        var query = "Select Sum(Amount) as Amount, WarehouseId FROM ( select  Cast(Sum(IsNull(LE.Debit, 0))as decimal(18,2))-Cast(Sum(IsNull(LE.Credit, 0))as decimal(18,2)) as Amount ," +
                                      " case when isnull(ir.WarehouseId,0) <> 0 THEN ir.WarehouseId when isnull(POM.WarehouseId,0) <> 0 THEN POM.WarehouseId ELSE 78 END as WarehouseId" +
                                      " from LadgerEntries LE  with(nolock)	INNER JOIN Ladgers L  with(nolock)	 On L.ID = LE.LagerID   and l.ObjectType ='Supplier'  left join IRMasters ir  with(nolock) on ir.id=le.ObjectID and le.ObjectType = 'IR'" +
                                      " LEFT JOIN PurchaseOrderMasters POM  with(nolock) ON LE.ObjectID = POM.PurchaseOrderId and le.ObjectType = 'PR' group by ir.WarehouseId, POM.WarehouseId ) X group by WarehouseId";

                        WarehouseSupplierAmount = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });

                taskList.Add(task6);

                var task7 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        var query = "select  Cast(Sum(IsNull(LE.Debit, 0))as decimal(18,2)) - Cast(Sum(IsNull(LE.Credit, 0))as decimal(18,2))  as Amount,a.WarehouseId    from LadgerEntries LE INNER JOIN Ladgers L On L.ID = LE.LagerID inner join DeliveryIssuances a on a.DeliveryIssuanceId=le.ObjectID where L.ObjectType ='Agent' group by a.WarehouseId";
                        WarehouseAgentAmount = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });

                taskList.Add(task7);


                var task8 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        //var query = " select a.WarehouseId, cast(sum(DamageInventory * isnull(item.purchaseprice,0))as decimal(18,2)) Amount from DamageStocks a with(nolock) outer apply  (select min(PurchasePrice) purchaseprice from ItemMasters b with(nolock) where a.Deleted=0 and a.ItemNumber=b.Number and a.WarehouseId=b.WarehouseId) item  group by a.WarehouseId";
                        //DamageInventory = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                        var query = "Exec GetDamageInventoryForWorkingCapital";
                        DamageInventory = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });
                

                taskList.Add(task8);                

                var task9 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        var query = " select b.WarehouseId, cast(sum(a.Price * a.qty) as decimal(18,2)) Amount  from GoodsReceivedDetails a inner join PurchaseOrderDetails b on a.PurchaseOrderDetailId=b.PurchaseOrderDetailId where a.Status=1 group by b.WarehouseId ";
                        PendingGRN = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });

                taskList.Add(task9);

                var task10 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        var query = "Exec GetWHGRIRDifference ";
                        GoodsReceivedNotInvoiced = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });

                taskList.Add(task10);

                var task11 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;

                        var query = " SELECT	Cast(SUM(AMOUNT) - Cast(ISNULL(sum(gr.gramount),0)as decimal(18,2)) as decimal(18,2)) as Amount  , WarehouseId FROM ( SELECT	SUM(AMOUNT) AMOUNT," +
                                    " PurchaseOrderId, WarehouseId FROM ( select  Sum(IsNull(LE.Debit, 0)) Amount, PRP.PurchaseOrderId, PRP.WarehouseId  " +
                                    " from LadgerEntries LE with(nolock)	INNER JOIN Ladgers L with(nolock) On L.ID = LE.LagerID   and l.ObjectType ='Supplier'" +
                                    " INNER JOIN IRMasters IRM with(nolock) ON LE.ObjectID = IRM.Id AND LE.ObjectType = 'IR' Inner JOIN PurchaseRequestPayments PRP  with(nolock) " +
                                    " ON IRM.PurchaseOrderId = PRP.PurchaseOrderId group by PRP.PurchaseOrderId , PRP.WarehouseId UNION select  Sum(IsNull(LE.Debit, 0)) Amount," +
                                    " PRP.PurchaseOrderId ,PRP.WarehouseId from LadgerEntries LE with(nolock)	INNER JOIN Ladgers L with(nolock) On L.ID = LE.LagerID   and l.ObjectType ='Supplier'" +
                                    " Inner JOIN PurchaseRequestPayments PRP  with(nolock) ON LE.ObjectID = PRP.Id and le.ObjectType = 'PR' group by PRP.PurchaseOrderId , PRP.WarehouseId" +
                                    " ) Y group by PurchaseOrderId , WarehouseId )X outer apply ( Select Sum(a.Qty*a.Price) gramount from PurchaseOrderDetails b  with(nolock)" +
                                    " Inner join GoodsReceivedDetails a  with(nolock) on a.PurchaseOrderDetailId=b.PurchaseOrderDetailId and  b.PurchaseOrderId=X.PurchaseOrderId " +
                                    " and a.IsActive=1 and a.IsDeleted=0 and a.Status=2 ) gr  GROUP BY	WarehouseId";
                        WarehouseSupplierAdvanceAmount = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });

                taskList.Add(task11);

                var task12 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;

                        var query = " Select im.WarehouseId, Cast(isnull( Sum((((ira.IRQuantity * ira.Price)-  " +
                                      " (case when ira.DiscountPercent is null or ira.DiscountPercent = 0 then ira.DiscountAmount else (ira.IRQuantity * ira.Price) * ira.DiscountPercent /100 end)) " +
                                      "  * ((ira.TotalTaxPercentage + isnull(ira.CessTaxPercentage,0)) / 100)) " +
                                      " + ((ira.IRQuantity * ira.Price)- (case when ira.DiscountPercent is null or ira.DiscountPercent = 0 then ira.DiscountAmount else (ira.IRQuantity * ira.Price) * ira.DiscountPercent /100 end))),0)  " +
                                      "  as decimal(18,2)) as Amount	 from InvoiceReceiptDetails ira  with(nolock) inner join IRMasters im with(nolock)  on im.Id = ira.IRMasterId  and im.IRStatus='Pending from Buyer side'  where  ira.IsActive=1 and (ira.IsDeleted=0 or ira.IsDeleted is null)  and ira.IRQuantity>0  group by im.WarehouseId ";

                        WarehouseIRPendingBuyerAmount = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });

                taskList.Add(task12);

                var task13 = Task.Factory.StartNew(() =>
                {
                    using (var context = new AuthContext())
                    {
                        context.Database.CommandTimeout = 600;
                        var query = "Exec GetNonSellableInventoryForWorkingCapital";
                        NonSellableInventory = context.Database.SqlQuery<WarehouseCashAmount>(query).ToList();
                    }
                });
                taskList.Add(task13);

                Task.WaitAll(taskList.ToArray());

                List<int> warehouseIds = orderStatusAmount.Select(x => x.WarehouseId).Distinct().ToList();

                if (hubInventory != null && hubInventory.Any())
                    warehouseIds.AddRange(hubInventory.Select(x => x.WarehouseId).ToList());

                warehouseIds = warehouseIds.Select(x => x).Distinct().ToList();

                var warehouses = new List<WarehouseMinDc>();

                using (var context = new AuthContext())
                {
                    warehouses = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).Select(x => new WarehouseMinDc
                    {
                        WarehouseId = x.WarehouseId,
                        WarehouseName = x.WarehouseName
                    }).ToList();
                }

                List<WorkingCapitalData> WorkingCapitalData = new List<WorkingCapitalData>();


                foreach (var item in warehouses)
                {
                    var data = new WorkingCapitalData();

                    data.WarehouseName = item.WarehouseName;
                    data.WarehouseId = item.WarehouseId;
                    data.DeliveryCanceledAmount = orderStatusAmount.Any(x => x.Status == "Delivery Canceled" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Delivery Canceled" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
                    data.DeliveryRedispatchAmount = orderStatusAmount.Any(x => x.Status == "Delivery Redispatch" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Delivery Redispatch" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
                    data.IssuedAmount = orderStatusAmount.Any(x => x.Status == "Issued" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Issued" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
                    data.ReadyToDispatchAmount = orderStatusAmount.Any(x => x.Status == "Ready to Dispatch" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Ready to Dispatch" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
                    data.ShippedAmount = orderStatusAmount.Any(x => x.Status == "Shipped" && x.WarehouseId == item.WarehouseId) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Shipped" && x.WarehouseId == item.WarehouseId).TotalAmount : 0;
                    data.InventoryAmount = hubInventory.Any(x => x.WarehouseId == item.WarehouseId) ? hubInventory.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Inventory : 0;
                    data.DeliveredButNotReconciled = orderNotReconsileAmount.Any(x => x.WarehouseId == item.WarehouseId) ? orderNotReconsileAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).TotalAmount : 0;
                    data.CashInOperation = Convert.ToDouble(CashInOperation.Any(x => x.WarehouseId == item.WarehouseId) ? CashInOperation.First(x => x.WarehouseId == item.WarehouseId).Amount : 0);
                    data.SupplierCredit = WarehouseSupplierAmount.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(WarehouseSupplierAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
                    data.AgentDues = WarehouseAgentAmount.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(WarehouseAgentAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
                    data.AvgSale = salesorderStatusAmount.Any(x => x.WarehouseId == item.WarehouseId) ? salesorderStatusAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).TotalAmount : 0;
                    data.ChequeInOperation = WarehouseChequeAmount.Any(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 1) ? WarehouseChequeAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 1).Amount : 0;
                    data.ChequeInBank = WarehouseChequeAmount.Any(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 2) ? WarehouseChequeAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 2).Amount : 0;
                    data.ChequeBounce = WarehouseChequeAmount.Any(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 4) ? WarehouseChequeAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId && x.ChequeStatus == 4).Amount : 0;
                    data.CreateDate = DateTime.Now.AddDays(-1).Date;
                    data.OnlinePrePaidAmount = onlinePaymentV2WithpaymentFrom.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(onlinePaymentV2WithpaymentFrom.Where(x => x.WarehouseId == item.WarehouseId).Sum(x => x.Amount)) : 0;
                    data.OnlinePayments = onlinePaymentV2WithpaymentFrom.Any(x => x.WarehouseId == item.WarehouseId) ? onlinePaymentV2WithpaymentFrom.Where(x => x.WarehouseId == item.WarehouseId).Select(x => new OnlinePayment { PaymentFrom = x.paymentFrom, Amount = Convert.ToDouble(x.Amount) }).ToList() : new List<OnlinePayment>();
                    //data.OnlinePrePaidAmountePaylater = onlinePaymentV2WithpaymentFrom.Any(x => x.paymentFrom == "ePaylater" && x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(onlinePaymentV2WithpaymentFrom.FirstOrDefault(x => x.paymentFrom == "ePaylater" && x.WarehouseId == item.WarehouseId).Amount) : 0;
                    //data.OnlinePrePaidAmounthdfc = onlinePaymentV2WithpaymentFrom.Any(x => x.paymentFrom == "hdfc" && x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(onlinePaymentV2WithpaymentFrom.FirstOrDefault(x => x.paymentFrom == "hdfc" && x.WarehouseId == item.WarehouseId).Amount) : 0;
                    //data.OnlinePrePaidAmountmPos = onlinePaymentV2WithpaymentFrom.Any(x => x.paymentFrom == "mPos" && x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(onlinePaymentV2WithpaymentFrom.FirstOrDefault(x => x.paymentFrom == "mPos" && x.WarehouseId == item.WarehouseId).Amount) : 0;
                    data.PendingGRNAmount = PendingGRN.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(PendingGRN.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
                    data.DamageStockAmount = DamageInventory.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(DamageInventory.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
                    data.NonSellableStockAmount= NonSellableInventory.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(NonSellableInventory.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;


                    data.GoodsReceivedNotInvoiced = GoodsReceivedNotInvoiced.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(GoodsReceivedNotInvoiced.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
                    data.SupplierAdvances = WarehouseSupplierAdvanceAmount.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(WarehouseSupplierAdvanceAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
                    data.IRPendingBuyerSide = WarehouseIRPendingBuyerAmount.Any(x => x.WarehouseId == item.WarehouseId) ? Convert.ToDouble(WarehouseIRPendingBuyerAmount.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Amount) : 0;
                    WorkingCapitalData.Add(data);
                }

                if (WorkingCapitalData != null && WorkingCapitalData.Any())
                {
                    result = mongoDbHelperWorkingCapital.InsertMany(WorkingCapitalData);
                }
            }
            return result;
        }

        public bool CashManagementEOD()
        {
            bool result = false;

            try
            {
                using (AuthContext context = new AuthContext())
                {
                    var warehouseids = context.Warehouses.Where(x => x.active && !x.Deleted && (x.IsKPP == false || x.IsKppShowAsWH == true)).Select(x => x.WarehouseId).ToList();
                    if (warehouseids != null && warehouseids.Any())
                    {
                        var TodayHubCurrencyCollections = context.CurrencyHubStock.Where(x => warehouseids.Contains(x.Warehouseid ) && EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(DateTime.Now)).ToList();

                        foreach (var warehouseid in warehouseids)
                        {
                            var TodayHubCurrencyCollection = TodayHubCurrencyCollections.FirstOrDefault(x => x.Warehouseid == warehouseid );
                            if (TodayHubCurrencyCollection == null)
                            {
                                var yesterdaydate = DateTime.Now.AddDays(-1);
                                var YesterdayHubCurrencyCollections = context.CurrencyHubStock.Where(x => x.Warehouseid == warehouseid
                                                                                     && (EntityFunctions.TruncateTime(x.BOD) == EntityFunctions.TruncateTime(yesterdaydate) || x.Status != "ClosingTransfer")).Include(x => x.HubCashCollections).Include(x => x.ChequeCollections).ToList();


                                CurrencyHubStock currencyHubStock = new CurrencyHubStock
                                {
                                    BOD = DateTime.Now,
                                    CreatedBy = 0,
                                    CreatedDate = DateTime.Now,
                                    IsActive = true,
                                    IsDeleted = false,
                                    TotalCashAmt = 0,
                                    TotalCheckAmt = 0,
                                    TotalDeliveryissueAmt = 0,
                                    TotalOnlineAmt = 0,
                                    TotalDueAmt = 0,
                                    Status = "StartDay",
                                    Warehouseid = warehouseid
                                };
                                currencyHubStock.HubCashCollections = new List<HubCashCollection>();
                                if (YesterdayHubCurrencyCollections != null && YesterdayHubCurrencyCollections.Any())
                                {
                                    foreach (var YesterdayHubCurrencyCollection in YesterdayHubCurrencyCollections)
                                    {
                                        if (YesterdayHubCurrencyCollection != null && YesterdayHubCurrencyCollection.HubCashCollections != null && YesterdayHubCurrencyCollection.HubCashCollections.Any())
                                        {
                                            foreach (var item in YesterdayHubCurrencyCollection.HubCashCollections.Where(x => x.IsActive && ((x.IsDeleted.HasValue && !x.IsDeleted.Value) || !x.IsDeleted.HasValue)))
                                            {
                                                if (!currencyHubStock.HubCashCollections.Any(x => x.CurrencyDenominationId == item.CurrencyDenominationId))
                                                {
                                                    HubCashCollection hubCashCollection = new HubCashCollection();
                                                    hubCashCollection.OpeningCurrencyCount = item.OpeningCurrencyCount + item.CurrencyCount - item.BankSendCurrencyCount + (item.ExchangeCurrencyCount);
                                                    hubCashCollection.CurrencyCount = 0;
                                                    hubCashCollection.CurrencyDenominationId = item.CurrencyDenominationId;
                                                    hubCashCollection.BankSendCurrencyCount = 0;
                                                    hubCashCollection.CreatedBy = 0;
                                                    hubCashCollection.CreatedDate = DateTime.Now;
                                                    hubCashCollection.IsActive = true;
                                                    hubCashCollection.IsDeleted = false;
                                                    context.HubCashCollection.Add(hubCashCollection);
                                                    currencyHubStock.HubCashCollections.Add(hubCashCollection);
                                                }
                                            }

                                            YesterdayHubCurrencyCollection.Status = "ClosingTransfer";
                                        }

                                        if (YesterdayHubCurrencyCollection.ChequeCollections.Any(x => x.ChequeStatus == Convert.ToInt32(ChequeStatusEnum.Operation)))
                                        {
                                            currencyHubStock.ChequeCollections = new List<ChequeCollection>();
                                            foreach (var item in YesterdayHubCurrencyCollection.ChequeCollections)
                                            {
                                                currencyHubStock.ChequeCollections.Add(item);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var denominations = context.CurrencyDenomination.Where(x=>x.IsActive).ToList();
                                    var hubCashCollections = denominations.Select(x =>
                                               new HubCashCollection
                                               {
                                                   OpeningCurrencyCount = 0,
                                                   CurrencyCount = 0,
                                                   CurrencyDenominationId = x.Id,
                                                   BankSendCurrencyCount = 0,
                                                   CreatedBy = 0,
                                                   CreatedDate = DateTime.Now,
                                                   IsActive = true,
                                                   IsDeleted = false,
                                                   ExchangeCurrencyCount = 0,
                                               }).ToList();
                                    if (hubCashCollections != null)
                                    {
                                        context.HubCashCollection.AddRange(hubCashCollections);
                                        foreach (var item in hubCashCollections)
                                        {
                                            currencyHubStock.HubCashCollections.Add(item);
                                        }
                                    }

                                }
                                context.CurrencyHubStock.Add(currencyHubStock);


                                if (context.Commit() > 0)
                                {
                                    logger.Info("Warehouse " + warehouseid + " Beigning Of Day (BOD) successfully.");
                                }
                                else
                                {
                                    logger.Info("Warehouse " + warehouseid + " Beigning Of Day (BOD) Not done.");
                                }

                            }
                        }
                    }
                    result = true;

                }

                //WorkingCapitalManager cmContoller = new WorkingCapitalManager();
                //var isWCGenerated = cmContoller.UpdateWorkingCapital();

                var isWCGenerated = UpdateWorkingCapital();
            }
            catch (Exception ex)
            {
                string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();

                logger.Error("Error in WarehouseDayStart Method: " + error);
                result = false;
            }

            return result;
        }

        //public bool DailyOrderInventoryStatus()
        //{
        //    MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
        //    var orderMasters = new List<MongoOrderMaster>();
        //    var orderStatusAmount = new List<OrderStatusAmount>();
        //    var hubInventory = new List<HubAvgInventory>();
        //    string ExcelSavePath = HttpContext.Current.Server.MapPath("~/ExcelGeneratePath/");
        //    var taskList = new List<Task>();

        //    var task1 = Task.Factory.StartNew(() =>
        //    {
        //        orderMasters = mongoDbHelper.Select(x => x.WarehouseId != 67 && !x.Deleted && (x.Status == "Shipped" || x.Status == "Issued" || x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch"
        //                                            || x.Status == "Delivery Canceled" || x.Status == "Delivered")
        //                        , collectionName: "OrderMaster")
        //                        .ToList();

        //        orderStatusAmount = orderMasters.GroupBy(x => new { x.WarehouseId, x.Status }).Select(x => new OrderStatusAmount
        //        {
        //            WarehouseId = x.Key.WarehouseId,
        //            Status = x.Key.Status,
        //            TotalAmount = x.Sum(z => z.DispatchAmount ?? z.GrossAmount)
        //        }).ToList();
        //    });

        //    taskList.Add(task1);


        //    var task2 = Task.Factory.StartNew(() =>
        //    {
               
        //        using (var context = new AuthContext())
        //        {
        //            context.Database.CommandTimeout = 600;
        //            var query = "Exec GetInventoryForWorkingCapital";
        //            hubInventory = context.Database.SqlQuery<HubAvgInventory>(query).ToList();
        //        }
        //    });

        //    taskList.Add(task2);
        //    Task.WaitAll(taskList.ToArray());

        //    var warehouseIds = orderStatusAmount.Select(x => x.WarehouseId).Distinct().ToList();

        //    if (hubInventory != null && hubInventory.Any())
        //        warehouseIds.AddRange(hubInventory.Select(x => x.WarehouseId).ToList());

        //    warehouseIds = warehouseIds.Select(x => x).Distinct().ToList();

        //    var warehouses = new List<WarehouseMinDc>();

        //    using (var context = new AuthContext())
        //    {
        //        warehouses = context.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).Select(x => new WarehouseMinDc
        //        {
        //            WarehouseId = x.WarehouseId,
        //            WarehouseName = x.WarehouseName
        //        }).ToList();
        //    }

        //    List<DailyOrderInventoryData> reportData = new List<DailyOrderInventoryData>();


        //    foreach (var item in warehouses)
        //    {
        //        var data = new DailyOrderInventoryData();

        //        data.WarehouseName = item.WarehouseName;
        //        data.DeliveryCanceledAmount = orderStatusAmount.Any(x => x.Status == "Delivery Canceled" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Delivery Canceled" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
        //        data.DeliveryRedispatchAmount = orderStatusAmount.Any(x => x.Status == "Delivery Redispatch" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Delivery Redispatch" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
        //        data.IssuedAmount = orderStatusAmount.Any(x => x.Status == "Issued" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Issued" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
        //        data.ReadyToDispatchAmount = orderStatusAmount.Any(x => x.Status == "Ready to Dispatch" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Ready to Dispatch" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
        //        data.ShippedAmount = orderStatusAmount.Any(x => x.Status == "Shipped" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Shipped" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
        //        data.InventoryAmount = hubInventory.Any(x => x.WarehouseId == item.WarehouseId) ? hubInventory.FirstOrDefault(x => x.WarehouseId == item.WarehouseId).Inventory : 0;
        //        data.DeliveredButNotReconciled = orderStatusAmount.Any(x => x.Status == "Payment Submitted" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67) ? orderStatusAmount.FirstOrDefault(x => x.Status == "Payment Submitted" && x.WarehouseId == item.WarehouseId && x.WarehouseId != 67).TotalAmount : 0;
        //        reportData.Add(data);
        //    }

        //    if (!Directory.Exists(ExcelSavePath))
        //        Directory.CreateDirectory(ExcelSavePath);


        //    var dataTables = new List<DataTable>();
        //    DataTable dt = ClassToDataTable.CreateDataTable(reportData);
        //    dt.TableName = "DailyOrderStatus_Inventory";
        //    dataTables.Add(dt);

        //    foreach (var item in orderMasters.GroupBy(x => x.Status))
        //    {
        //        List<OrderSummaryStatusWise> statusWiseOrders = new List<OrderSummaryStatusWise>();
        //        DataTable statusDatatable = new DataTable();
        //        foreach (var order in item)
        //        {
        //            statusWiseOrders.Add(new OrderSummaryStatusWise
        //            {
        //                OrderId = order.OrderId,
        //                Status = item.Key,
        //                WarehouseId = order.WarehouseId,
        //                WarehouseName = warehouses.FirstOrDefault(x => x.WarehouseId == order.WarehouseId)?.WarehouseName,
        //                DispatchAmount = order.DispatchAmount ?? order.GrossAmount
        //            });
        //        }

        //        statusDatatable = ClassToDataTable.CreateDataTable(statusWiseOrders);
        //        statusDatatable.TableName = item.Key;
        //        dataTables.Add(statusDatatable);
        //    }


        //    string filePath = ExcelSavePath + "DailyOrderStatus_Inventory_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".xlsx";

        //    if (ExcelGenerator.DataTable_To_Excel(dataTables, filePath))
        //    {

        //        string connectionString = ConfigurationManager.ConnectionStrings["AuthContext"].ConnectionString;
        //        string To = "", From = "", Bcc = "";
        //        DataTable emaildatatable = new DataTable();
        //        using (var connection = new SqlConnection(connectionString))
        //        {
        //            using (var command = new SqlCommand("Select * from EmailRecipients where EmailType='DailyOrderStatusInventory'", connection))
        //            {

        //                if (connection.State != ConnectionState.Open)
        //                    connection.Open();

        //                SqlDataAdapter da = new SqlDataAdapter(command);
        //                da.Fill(emaildatatable);
        //                da.Dispose();
        //                connection.Close();
        //            }
        //        }
        //        if (emaildatatable.Rows.Count > 0)
        //        {
        //            To = !string.IsNullOrEmpty(emaildatatable.Rows[0]["To"].ToString()) ? emaildatatable.Rows[0]["To"].ToString() : To;
        //            From = !string.IsNullOrEmpty(emaildatatable.Rows[0]["From"].ToString()) ? emaildatatable.Rows[0]["From"].ToString() : From;
        //            Bcc = !string.IsNullOrEmpty(emaildatatable.Rows[0]["Bcc"].ToString()) ? emaildatatable.Rows[0]["Bcc"].ToString() : "";
        //        }
        //        string subject = DateTime.Now.ToString("dd MMM yyyy") + " Daily Order Status & Inventory Report";
        //        string message = "Please find attach Daily Order Status & Inventory Report";
        //        if (!string.IsNullOrEmpty(To) && !string.IsNullOrEmpty(From))
        //            EmailHelper.SendMail(From, To, Bcc, subject, message, filePath);
        //        else
        //            logger.Error("Daily Order Status & Inventory Report To and From empty");

        //    }

        //    return true;


        //}
    }
}