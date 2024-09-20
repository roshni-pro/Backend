using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Controllers.PurchaseOrder;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.API.Managers;
using AngularJSAuthentication.API.Managers.CRM;
using AngularJSAuthentication.BatchManager;
using AngularJSAuthentication.BusinessLayer.Helpers.ElasticDataHelper;
using AngularJSAuthentication.BusinessLayer.Managers;
//using AngularJSAuthentication.BatchManager;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions.BatchCode;
using AngularJSAuthentication.BusinessLayer.Managers.TripPlanner;
using AngularJSAuthentication.Caching;
using AngularJSAuthentication.Common.Enums;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.BatchCode;
using AngularJSAuthentication.DataContracts.ElasticSearch;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters.UPIPayment;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Account;
using AngularJSAuthentication.Model.Base.Audit;
using AngularJSAuthentication.Model.PurchaseOrder;
using AngularJSAuthentication.Model.PurchaseRequestPayments;
using AngularJSAuthentication.ORTools.ViewModel;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QRCoder;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using AngularJSAuthentication.API.Helper.Notification;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/Test")]
    public class TestController : BaseAuthController
    {

        [Route("WhatsappApi")]
        [HttpPost]
        [AllowAnonymous]
        public async Task WhatsappApi()
        {
            string connStr = "mongodb+srv://pankaj:SkAdmin123@cluster0.hoqwr.mongodb.net/SK?retryWrites=true";
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connStr));
            //settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            settings.ConnectTimeout = TimeSpan.FromMinutes(1);
            settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(10);
            var dbClient = new MongoClient(settings);
            var database = dbClient.GetDatabase("SK");
            var data = await Request.Content.ReadAsByteArrayAsync();
            var str = System.Text.Encoding.Default.GetString(data);
            var document = BsonSerializer.Deserialize<BsonDocument>(str);
            var collection = database.GetCollection<BsonDocument>("WhatsAppWebhookResponse");
            await collection.InsertOneAsync(document);

        }

        string strJSON = null;
        string col0;

        [Route("GenerateInvoiceNo/{OrderId}/{StateId}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GenerateInvoiceNo(long OrderId, int StateId)
        {
            bool result = false;
            if (OrderId > 0 && StateId > 0)
            {
                using (var context = new AuthContext())
                {
                    var odm = context.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == OrderId);
                    if (odm.invoice_no.TrimEnd() == "")
                    {
                        BusinessLayer.Managers.OrderMasterManager manager = new BusinessLayer.Managers.OrderMasterManager();
                        result = await manager.InvoiceNoGenerate(OrderId, StateId);
                    }
                }
            }
            return result;
        }


        [Route("ProcessedBatchDelete")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> ProcessedBatchDelete()
        {
            MongoDbHelper<BatchCodeSubjectMongoQueue> mongoDbHelper = new MongoDbHelper<BatchCodeSubjectMongoQueue>();
            var filter = Builders<BatchCodeSubjectMongoQueue>.Filter.Where(x => x.IsProcess == true && x.TransactionDate <= DateTime.Now.AddMonths(-1));
            var _collection = mongoDbHelper.mongoDatabase.GetCollection<BatchCodeSubjectMongoQueue>("BatchCodeSubjectMongoQueue");
            DeleteResult result = await _collection.DeleteManyAsync(filter);

            //var subjectList = mongoDbHelper.Select(x => x.IsProcess == true && x.TransactionDate <= DateTime.Now.AddMonths(-1)).ToList();
            //if (subjectList != null && subjectList.Any())
            //{
            //    foreach (var item in subjectList)
            //    {
            //        await mongoDbHelper.DeleteAsync(item.Id, collectionName: "BatchCodeSubjectMongoQueue");
            //    }
            //}
            return true;
        }

        //[Route("InsertManualOutEntry")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<bool> InsertManualOutEntry()
        //{
        //    List<int> Odetails = new List<int>
        //    {

        //    };
        //    foreach (var item in Odetails)
        //    {
        //        try
        //        {
        //            StockBatchTransactionManager manager = new StockBatchTransactionManager();
        //            await manager.InsertManualOutEntry(item);
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //    }
        //    return true;


        //}



        [Route("OrderOtpAccess")]
        [HttpGet]
        [AllowAnonymous]
        public bool OrderOtpAccess()
        {
            ReportManager autoSettleHelper = new ReportManager();
            return autoSettleHelper.SendLastDayOrderOtpAccessEmail();
        }


        [Route("OnedayFareAmount")]
        [HttpGet]
        [AllowAnonymous]
        public double OnedayFareAmount(long VehicleMasterId = 9)
        {
            TripPlannerVechicleAttandanceManager oneDayFareAmt = new TripPlannerVechicleAttandanceManager();
            return oneDayFareAmt.GetOneDayFareAmount(VehicleMasterId);
        }

        //[Route("PRConvertToCredit")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<int> PRConvertToCredit(int POid)
        //{
        //    AdvancePOtoCreditConvertHelper autoSettleHelper = new AdvancePOtoCreditConvertHelper();
        //    return await autoSettleHelper.ConvertAdvancePOtoCredit(POid);
        //}


        [Route("SetAutoClosePO")]
        [HttpGet]
        [AllowAnonymous]
        public bool SetAutoClosePO()
        {
            PurchaseOrderNewController autoSettleHelper = new PurchaseOrderNewController();
            return autoSettleHelper.AutoClosePO();
        }

        [Route("Auto")]
        [HttpGet]
        [AllowAnonymous]
        public bool AutoSettled()
        {
            AutoSettleHelper autoSettleHelper = new AutoSettleHelper();
            return autoSettleHelper.AutoSettleOrders();
        }






        //[Route("wh")]
        //[HttpGet]
        //[AllowAnonymous]
        //public bool WhTest(int id)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        //#region checkAjustmentPOPaymentSettle
        //        ////  var ispaid = db.Database.SqlQuery<int>("select b.PurchaseOrderId from PrPaymentTransfers A  join PurchaseOrderMasters B on A.ToPurchaseOrderId = B.PurchaseOrderId join AdjustmentPODetails C on B.PurchaseOrderId = c.PurchaseRequestId  and PurchaseOrderId = "+id  +" and ETotalAmount <= a.TransferredAmount").FirstOrDefault();
        //        //var ispaid = db.Database.SqlQuery<int>("exec CheckAdjustmentClosedPOPaid " + id).FirstOrDefault();
        //        //#endregion

        //        //bool a = (ispaid > 0) ? true : false;
        //        //47340
        //    }

        //    //WHLicenseExpAlert wHLicense = new WHLicenseExpAlert();
        //    //wHLicense.WHLicenseExpDateAlert();
        //    return true;
        //}

        [Route("TestRedisMq")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> TestRedisMq()
        {
            //using (var mqClient = RedisMqManager.RedisMq.CreateMessageQueueClient())
            //{
            //    IMessage message = new Message<TestMQ>();
            //    message.Body = new TestMQ { PropName = "Client 1" };
            //    mqClient.Publish("TestMQ", message);
            //    //mqClient.Notify("TestMQ", message);
            //}

            RedisCacheProvider redisCacheProvider = new RedisCacheProvider();

            ISubscriber sub = redisCacheProvider.RedisConnection.GetSubscriber();
            await sub.PublishAsync("Stock.TestMQ", JsonConvert.SerializeObject(new TestMQ { PropName = "Client 1" }));
            return "OK";

        }


        [Route("MigrateOrdersToMongo")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> MigrateOrdersToMongo()
        {
            int orderCount = 0;
            using (var context = new AuthContext())
            {
                orderCount = context.DbOrderMaster.Count();
            }
            MongoDbHelper<OrderMaster> mongoDbHelper = new MongoDbHelper<OrderMaster>();


            for (int i = 0; i < orderCount; i += 10000)
            {
                using (var context = new AuthContext())
                {
                    var orders = context.DbOrderMaster.Include("orderDetails").OrderBy(x => x.OrderId).Skip(i).Take(10000).ToList();
                    var orderIds = orders.Select(x => x.OrderId).ToList();

                    var orderHistories = context.OrderMasterHistoriesDB.Where(x => orderIds.Contains(x.orderid)).ToList();
                    var orderDispatch = context.OrderDispatchedMasters.Where(x => orderIds.Contains(x.OrderId)).ToList();
                    orders.ForEach(x =>
                    {
                        var history = orderHistories.Where(z => z.orderid == x.OrderId);
                        var dispatch = orderDispatch.FirstOrDefault(z => z.OrderId == x.OrderId);
                        x.OrderMasterHistories = history != null ? history.ToList() : null;
                        x.OrderDispatchedMasterId = dispatch?.OrderDispatchedMasterId;

                    });

                    var abc = await mongoDbHelper.InsertManyAsync(orders);
                }

            }

            return true;
        }

        [Route("UpdateCreditNoteInMongoOrder")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> UpdateCreditNoteInMongoOrder()
        {
            int orderCount = 0;
            using (var context = new AuthContext())
            {
                orderCount = context.OrderDispatchedMasters.Count();
            }
            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();

            var mongoOrders = mongoDbHelper.Select(x => x.Status == "Post Order Canceled" || (x.OrderDispatchedMasterId.HasValue && !x.DispatchAmount.HasValue), collectionName: "OrderMaster").ToList();
            orderCount = mongoOrders.Count;


            for (int i = 0; i < orderCount; i += 1000)
            {
                var orders = mongoOrders.Skip(i).Take(1000).ToList();

                var orderIds = orders.Select(x => x.OrderId);

                using (var context = new AuthContext())
                {
                    var orderDispatch = context.Database.SqlQuery<DispatchOrder>("select OrderId,GrossAmount,CreatedDate,PocCreditNoteDate,PocCreditNoteNumber" +
                        " from OrderDispatchedMasters where orderid in (" + string.Join(",", orderIds) + ")").ToList();

                    //.OrderDispatchedMasters.Where(x => orderIds.Contains(x.OrderId)).ToList();
                    orders.ForEach(x =>
                    {
                        x.DispatchAmount = orderDispatch.FirstOrDefault(z => z.OrderId == x.OrderId)?.GrossAmount;
                        x.ReadytoDispatchedDate = orderDispatch.FirstOrDefault(z => z.OrderId == x.OrderId)?.CreatedDate;
                        x.CreditNoteDate = orderDispatch.FirstOrDefault(z => z.OrderId == x.OrderId)?.PocCreditNoteDate;
                        x.CreditNoteNumber = orderDispatch.FirstOrDefault(z => z.OrderId == x.OrderId)?.PocCreditNoteNumber;

                        mongoDbHelper.ReplaceWithoutFind(x.Id, x, collectionName: "OrderMaster");
                    });

                }

            }

            return true;
        }

        [Route("UpdateOrderInMongoById")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> UpdateOrderInMongoById(List<int> orderIds)
        {
            using (var context = new AuthContext())
            {
                var orders = context.DbOrderMaster.Where(x => orderIds.Contains(x.OrderId)).ToList();

                foreach (var order in orders)
                {
                    order.comments = order.comments + " ";
                    context.Entry(order).State = EntityState.Modified;
                }
                context.Commit();
            }
            return true;
        }


        [Route("DeliveredOrderToZaruriStock")]
        [HttpGet]
        [AllowAnonymous]
        public bool DeliveredOrderToZaruriStock()
        {
            using (var db = new AuthContext())
            {

                List<SellerOrderDelivered> ChangeOrderLists = db.SellerOrderDelivereds.Where(x => x.OrderId == 402896).ToList();
                if (ChangeOrderLists != null && ChangeOrderLists.Any())
                {
                    var tradeUrl = ConfigurationManager.AppSettings["TradeURL"] + "/api/Bid/AddStock";

                    var orderIds = ChangeOrderLists.Select(x => x.OrderId).Distinct().ToList();
                    var orders = db.OrderDispatchedDetailss.Where(x => orderIds.Contains(x.OrderId)).ToList();

                    var itemNumbers = orders.Select(x => x.itemNumber).Distinct().ToList();

                    var centralItems = db.ItemMasterCentralDB.Where(x => itemNumbers.Contains(x.Number)).ToList();

                    foreach (var item in orders.GroupBy(x => x.CustomerId))
                    {
                        ManageBidDc bidToSync = new ManageBidDc
                        {
                            SellerId = item.Key,
                            StockItems = item.Select(x => new ManageBidItems
                            {
                                CentraltemId = centralItems.FirstOrDefault(z => z.Number == x.itemNumber).Id,
                                MRP = (decimal)x.price,
                                Qty = x.qty,
                                PurchasePrice = (decimal)x.UnitPrice
                            }).ToList()
                        };

                        var orderdetailIds = item.Select(x => x.OrderDispatchedDetailsId).ToList();
                        var ChangeOrderListsToUpdate = ChangeOrderLists.Where(x => orderdetailIds.Contains(x.DispatchDetailId)).ToList();

                        try
                        {
                            using (var client = new HttpClient())
                            {
                                var newJson = JsonConvert.SerializeObject(bidToSync);
                                using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
                                {
                                    var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));

                                    response.EnsureSuccessStatusCode();
                                    string responseBody = response.Content.ReadAsStringAsync().Result;

                                    var result = JsonConvert.DeserializeObject<ResponseMetaData>(responseBody);
                                    if (result.Status != "Error")
                                    {
                                        ChangeOrderListsToUpdate.ForEach(x =>
                                        {
                                            x.IsProcessed = true;
                                            x.UpdatedDate = DateTime.Now;
                                            db.Entry(x).State = EntityState.Modified;
                                        });
                                    }
                                    else
                                    {
                                        ChangeOrderListsToUpdate.ForEach(x =>
                                        {
                                            x.IsProcessed = true;
                                            x.UpdatedDate = DateTime.Now;
                                            x.Error = result.ErrorMessage;
                                            db.Entry(x).State = EntityState.Modified;
                                        });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ChangeOrderListsToUpdate.ForEach(x =>
                            {
                                x.IsProcessed = true;
                                x.UpdatedDate = DateTime.Now;
                                x.Error = ex.ToString();
                                db.Entry(x).State = EntityState.Modified;
                            });
                        }

                    }

                    db.Commit();
                }
            }

            return true;
        }


        [Route("SettleIR_ApprovedFromBuyerSideWithSamePR")]
        [HttpGet]
        [AllowAnonymous]
        public bool SettleIR_ApprovedFromBuyerSideWithSamePR()
        {
            int userID = 1717;
            DateTime currentTime = DateTime.Now;
            using (var db = new AuthContext())
            {
                var query = @"select	LE.Debit, 
		                                IRM.TotalAmountRemaining - ISNULL(X.CreditAmount, 0) PayingAmount, 
		                                IRM.ID IRMasterId 
                                from LadgerEntries LE 
                                INNER JOIN PurchaseRequestPayments PRP ON LE.PRPaymentId = PRP.ID AND LE.ObjectType = 'PR' and LE.Debit > 0
                                INNER JOIN IRMasters IRM ON PRP.PurchaseOrderId = IRM.PurchaseOrderId AND IRM.IRStatus = 'Approved from Buyer side'
                                OUTER APPLY (
			                                SELECT IsNULL( SUM( (D.IRPrice * (D.ShortQty + D.DamageQty + D.ExpiryQty)) 
						                                +  IsNull(D.TotalTaxPercentage , 0)* (D.IRPrice * (D.ShortQty + D.DamageQty + D.ExpiryQty))/100
						                                +  IsNull(D.CessTaxPercentage , 0)* (D.IRPrice * (D.ShortQty + D.DamageQty + D.ExpiryQty))/100) 
						                                , 0) CreditAMount
			                                FROM IRCreditNoteMasters M
			                                INNER JOIN IRCreditNoteDetails D ON M.Id = D.IRCreditNoteMasterId  AND D.IsActive = 1 AND D.IsDeleted = 0
			                                WHERE M.IRMasterId = IRM.Id AND M.IsActive = 1 AND M.IsDeleted = 0
		                                ) X
                                WHERE IRM.TotalAmountRemaining - ISNULL(X.CreditAmount, 0) >10";
                List<TestIRSettle> settleLIst = db.Database.SqlQuery<TestIRSettle>(query).ToList();
                if (settleLIst != null && settleLIst.Any())
                {
                    foreach (var item in settleLIst)
                    {
                        IRMaster irm = db.IRMasterDB.FirstOrDefault(x => x.Id == item.IRMasterId);
                        PurchaseRequestSettlementHelper purchaseRequestSettlementHelper = new PurchaseRequestSettlementHelper();
                        purchaseRequestSettlementHelper.SettleAmount(db, irm, item.PayingAmount, userID, currentTime, Guid.NewGuid(), false);
                    }

                }
            }
            return true;
        }


        [Route("UpdateSupplierDataNew")]
        [HttpGet]
        [AllowAnonymous]
        public bool UpdateSupplierDataNew()
        {
            using (var db = new AuthContext())
            {
                List<int> irList = new List<int>() { 153
                        ,3173
                        ,9175
                        ,9519
                        ,9864
                        ,9943
                        ,9944
                        ,9954
                        ,9965
                        ,9988
                        ,10870
                        ,10933
                        ,13214
                        ,14235
                        ,15734
                        ,15928
                        ,16689
                        ,22164
                        ,22403
                        ,23557
                        ,24002
                        ,31867
                    };
                foreach (var ir in irList)
                {
                    var irMastet = db.IRMasterDB.First(x => x.Id == ir);
                    //IRHelper.UpdateSupplierDataNew(9954, 29203, 1, db, 1717);
                    IRHelper.UpdateSupplierDataNew(irMastet.Id, irMastet.PurchaseOrderId, 1, db, 1717);
                }

            }
            return true;
        }

        [Route("RTDLedger")]
        [HttpGet]
        //[AllowAnonymous]
        public bool RTDLedger()
        {

            string oldstatus = "";
            string status = "Ready to Dispatch";
            List<long> vList = new List<long> { 2, 3 };
            var query = @"select OM.OrderId from OrderMasters OM 
                            LEFT JOIN LadgerEntries LE ON OM.OrderId = LE.ObjectID AND LE.ObjectType = 'Order' AND LE.VouchersTypeID = 1
                            where 
                            OM.CreatedDate > '2020-01-01' and 
                            OM.Status NOT IN ('Pending', 'Inactive', 'Failed', 'Dummy Order Cancelled', 'init', 'Order Canceled', 'Payment Pending')
                            and LE.ID IS NULL 
                            order by OM.CreatedDate DESC";

            List<int> orderList = null;
            using (var db = new AuthContext())
            {
                orderList = db.Database.SqlQuery<int>(query).ToList();
            }


            foreach (var orderId in orderList)
            {


                if (true)
                {
                    int redispatchCount = 0;
                    using (var db = new AuthContext())
                    {
                        var order = db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderId);
                        oldstatus = order.Status;
                        order.Status = "Pendinggggggggg";
                        redispatchCount = order.ReDispatchCount;

                        order.ReDispatchCount = 0;
                        db.SaveChanges();
                    }

                    using (var db = new AuthContext())
                    {
                        var order = db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderId);
                        order.Status = status;
                        //order.ReDispatchCount = redispatchCount;
                        db.Commit();

                        var leList = db.LadgerEntryDB.Where(x => x.ObjectID == orderId && x.ObjectType == "Order" && x.Date >= DateTime.Today).ToList();
                        var date = db.OrderMasterHistoriesDB.Where(x => x.orderid == orderId && x.Status == status).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate.Date;

                        foreach (var item in leList)
                        {
                            item.Date = date;
                        }
                        db.Commit();
                    }


                    using (var db = new AuthContext())
                    {
                        var order = db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderId);
                        order.ReDispatchCount = redispatchCount;
                        order.Status = oldstatus;
                        db.SaveChanges();
                    }

                }

            }


            return true;
        }


        [Route("SettleLedger")]
        [HttpGet]
        //[AllowAnonymous]
        public bool SettleLedger()
        {
            string status = "sattled";
            List<long> vList = new List<long> { 2, 3 };
            var query = @"select OM.OrderId from OrderMasters OM 
                            LEFT JOIN LadgerEntries LE 
	                            ON OM.OrderId = LE.ObjectID AND LE.ObjectType = 'Order'  and Le.VouchersTypeID in (2, 3) and LE.Credit > 0
                            WHERE OM.Status in ('sattled') AND LE.ID IS NULL
                            AND OM.DeliveredDate > '2019-03-31'
                            ORDER BY OM.DeliveredDate DESC";

            List<int> orderList = null;
            using (var db = new AuthContext())
            {
                orderList = db.Database.SqlQuery<int>(query).ToList();
            }


            foreach (var orderId in orderList)
            {
                var q = @"Select Isnull (SUM(pay.amount),0) As Amount from PaymentResponseRetailerApps pay 
                            inner join OrderDispatchedMasters odm 
                            on odm.OrderId=pay.OrderId
                            Where pay.OrderId="
                            + orderId.ToString()
                            + " and pay.status='Success'group by odm.invoice_no";

                bool isPaymentDone = false;
                using (var db = new AuthContext())
                {
                    var l = db.Database.SqlQuery<double>(q).ToList();

                    if (l != null && l.Any())
                    {
                        isPaymentDone = true;
                    }
                }

                if (isPaymentDone)
                {
                    using (var db = new AuthContext())
                    {
                        var order = db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderId);
                        order.Status = "Deliveredd";
                        db.Commit();
                    }

                    using (var db = new AuthContext())
                    {
                        var order = db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderId);
                        order.Status = status;
                        db.Commit();

                        var leList = db.LadgerEntryDB.Where(x => x.ObjectID == orderId && x.ObjectType == "Order" && vList.Contains(x.VouchersTypeID.Value)).ToList();
                        var date = db.OrderMasterHistoriesDB.Where(x => x.orderid == orderId && x.Status == status).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate.Date;

                        foreach (var item in leList)
                        {
                            item.Date = date;
                        }
                        db.Commit();
                    }
                }

            }


            return true;
        }

        [Route("POCLedger")]
        [HttpGet]
        //[AllowAnonymous]
        public bool POCLedger()
        {
            string status = "Post Order Canceled";
            List<long> vList = new List<long> { 20, 4 };
            DateTime currentdata = DateTime.Today;


            var query = @"select OM.OrderId from OrderMasters OM 
                            LEFT JOIN LadgerEntries LE 
	                            ON OM.OrderId = LE.ObjectID AND LE.ObjectType = 'Order'  and Le.VouchersTypeID in (20) and LE.Debit > 0
                            WHERE OM.Status in ('Post Order Canceled') AND LE.ID IS NULL
                            AND OM.CreatedDate > '2019-03-31'
                            ORDER BY OM.DeliveredDate DESC";

            List<int> orderList = null;
            using (var db = new AuthContext())
            {
                orderList = db.Database.SqlQuery<int>(query).ToList();
            }


            foreach (var orderId in orderList)
            {

                bool isPaymentDone = true;

                if (isPaymentDone)
                {
                    using (var db = new AuthContext())
                    {
                        var order = db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderId);
                        order.Status = "Deliveredd";
                        db.Commit();
                    }

                    using (var db = new AuthContext())
                    {
                        var order = db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == orderId);
                        order.Status = status;
                        db.Commit();

                        var leList = db.LadgerEntryDB.Where(x => x.ObjectID == orderId && x.ObjectType == "Order" && vList.Contains(x.VouchersTypeID.Value) && x.Date == currentdata).ToList();
                        var date = db.OrderMasterHistoriesDB.Where(x => x.orderid == orderId && x.Status == status).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate.Date;

                        foreach (var item in leList)
                        {
                            item.Date = date;
                        }
                        db.Commit();
                    }
                }

            }


            return true;
        }

        [Route("SettleIRM")]
        [AllowAnonymous]
        [HttpGet]
        //[AllowAnonymous]
        public bool SettleIRM()
        {

            var query = @"select ToPurchaseOrderId from PrPaymentTransfers
                            EXCEPT
                            select FromPurchaseOrderId from PrPaymentTransfers";

            List<long> POList = null;
            using (var db = new AuthContext())
            {
                POList = db.Database.SqlQuery<long>(query).ToList();
                //POList = new List<long>();
                //POList.Add(48337);
                if (POList != null && POList.Count > 0)
                {
                    foreach (var poId in POList)
                    {
                        var prQuery = "select PaidAmount from PurchaseRequestPayments where PrPaymentStatus = 'Approved' AND PurchaseOrderId = " + poId.ToString();
                        List<int> paidAmountList = db.Database.SqlQuery<int>(prQuery).ToList();
                        var purchaseRequestPayment = db.PurchaseRequestPaymentsDB.FirstOrDefault(x => x.PrPaymentStatus == "Approved" && x.PurchaseOrderId == poId);
                        if (paidAmountList != null && paidAmountList.Count == 1)
                        {
                            var prtQuery = from prt in db.PrPaymentTransferDB
                                           join pr in db.PurchaseRequestPaymentsDB
                                              on prt.SourcePurchaseRequestPaymentId equals pr.Id
                                           where prt.ToPurchaseOrderId == poId
                                           && pr.PrPaymentStatus == "Approved"
                                           select prt;
                            PrPaymentTransfer xfer = prtQuery.FirstOrDefault();


                            var irSumQuery = @"select SUM(DEBIT) DB 
                                            from LadgerEntries LE
                                            where VouchersTypeID = 6 AND ObjectType = 'IR' "
                                        + " AND LE.RefNo = '"
                                        + purchaseRequestPayment.RefNo
                                        + "'";
                            double? irSum = db.Database.SqlQuery<double?>(irSumQuery).FirstOrDefault();
                            if ((xfer.TransferredAmount - (xfer.OutAmount.HasValue ? xfer.OutAmount.Value : 0)) >= (irSum.HasValue ? irSum.Value : 0))
                            {
                                xfer.SettledAmount = irSum.HasValue ? irSum.Value : xfer.SettledAmount;
                                db.Commit();
                            }
                            else
                            {

                            }
                        }
                        else
                        {

                        }
                    }
                }

            }


            return true;
        }

        [Route("AllInActiveUserLogoutNotification")]
        [HttpGet]
        [AllowAnonymous]
        public bool AllInActiveUserLogoutNotification()
        {
            using (AuthContext db = new AuthContext())
            {
                var FCMIds = db.Peoples.Where(x => x.Active == false && x.FcmId != null && x.Deleted == false).Select(x => x.FcmId).ToList();

                if (FCMIds.Any())
                {
                    string Key = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                    //var objNotificationList = FCMIds.Distinct().Select(x => new
                    //{
                    //    to = x,
                    //    PeopleId = 0,
                    //    data = new
                    //    {
                    //        title = "",
                    //        body = "",
                    //        icon = "",
                    //        typeId = "",
                    //        notificationCategory = "",
                    //        notificationType = "",
                    //        notificationId = "",
                    //        notify_type = "logout",
                    //        url = "",
                    //    }
                    //}).ToList();
                    var data = new FCMData
                    {
                        title = "",
                        body = "",
                        icon = "",
                        notificationCategory = "",
                        notificationType = "",
                        notify_type = "logout",
                        url = "",
                    };
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    ParallelLoopResult parellelResult = Parallel.ForEach(FCMIds.Distinct(), (x) =>
                    {
                        try
                        {

                            //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                            var result = firebaseService.SendNotificationForApprovalAsync(x, data);
                            if (result != null)
                            {
                                //AutoNotification.IsSent = true;
                            }
                            else
                            {
                                //AutoNotification.IsSent = false;
                            }
                            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                            //tRequest.Method = "post";
                            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
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
                            //                AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse>(responseFromFirebaseServer);
                            //                if (response.success == 1)
                            //                {
                            //                    //totalSent.Add(1);
                            //                }
                            //                else if (response.failure == 1)
                            //                {
                            //                    //totalNotSent.Add(1);
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                        }
                        catch (Exception asd)
                        {
                        }
                    });

                }
            }

            return true;
        }

        [Route("AllSalesLogoutNotification")]
        [HttpGet]
        [AllowAnonymous]
        public bool AllSalesLogoutNotification()
        {
            using (AuthContext db = new AuthContext())
            {
                var FCMIds = db.Peoples.Where(x => x.Active == true && x.Type == "Sales" && x.Department == "Sales" && x.FcmId != null).Select(x => x.FcmId).ToList();

                if (FCMIds.Any())
                {
                    string Key = ConfigurationManager.AppSettings["SalesFcmApiKey"];
                    //var objNotificationList = FCMIds.Distinct().Select(x => new
                    //{
                    //    to = x,
                    //    PeopleId = 0,
                    //    data = new
                    //    {
                    //        title = "",
                    //        body = "",
                    //        icon = "",
                    //        typeId = "",
                    //        notificationCategory = "",
                    //        notificationType = "",
                    //        notificationId = "",
                    //        notify_type = "logout",
                    //        url = "",
                    //    }
                    //}).ToList();
                    var data = new FCMData
                    {
                        title = "",
                        body = "",
                        icon = "",
                        notificationCategory = "",
                        notificationType = "",
                        notify_type = "logout",
                        url = "",
                    };
                    var firebaseService = new FirebaseNotificationServiceHelper(Key);
                    ParallelLoopResult parellelResult = Parallel.ForEach(FCMIds.Distinct(), (x) =>
                    {
                        try
                        {
                            //var fcmid = "fZGIeP5dTKGd-2JBZttcDj:APA91bGINtqXqKuCcEHd4qXZMN-VtX5KC2g98KkytmGdpc28_-duDu8Ry1P6Kk_Xb9RgRG0iDWrp8DkwE1EPCOPG0OLz2uRjo-HBg-Ysg-mDaMErlYLjJGZE7ScXKIkjmWZ0xNO6UyxN";
                            var result = firebaseService.SendNotificationForApprovalAsync(x, data);
                            if (result != null)
                            {
                                //AutoNotification.IsSent = true;
                            }
                            else
                            {
                                //AutoNotification.IsSent = false;
                            }
                            //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send") as HttpWebRequest;
                            //tRequest.Method = "post";
                            //string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(x);
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
                            //                AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<AngularJSAuthentication.API.Controllers.NotificationController.FCMResponse>(responseFromFirebaseServer);
                            //                if (response.success == 1)
                            //                {
                            //                    //totalSent.Add(1);
                            //                }
                            //                else if (response.failure == 1)
                            //                {
                            //                    //totalNotSent.Add(1);
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                        }
                        catch (Exception asd)
                        {
                        }
                    });

                }
            }

            return true;
        }

        [Route("CreateIndexNew")]
        [HttpGet]
        [AllowAnonymous]
        public bool CreateIndexNew()
        {
            BatchManager.Helpers.ElasticBatchHelper helper = new BatchManager.Helpers.ElasticBatchHelper();
            helper.CreateIndex();
            return true;
        }

        [Route("DeleteIndexNew")]
        [HttpGet]
        [AllowAnonymous]
        public bool DeleteIndexNew()
        {
            BatchManager.Helpers.ElasticBatchHelper helper = new BatchManager.Helpers.ElasticBatchHelper();
            helper.DeleteIndex();
            return true;
        }

        [Route("GetCRMPDetail")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetCRMPDetail()
        {
            bool res = false;
            CRMManager cRMManager = new CRMManager();
            res = await cRMManager.GetCRMPDetail();
            return res;
        }

        private class DispatchOrder
        {
            public int OrderId { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? PocCreditNoteDate { get; set; }
            public string PocCreditNoteNumber { get; set; }
            public double GrossAmount { get; set; }
        }



        [Route("MigrateOfferCodeToOrdersMongo")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> MigrateOfferCodeOrdersToMongo()
        {
            int orderCount = 0;

            // MongoDbHelper<OrderMaster> mongoDbHelper = new MongoDbHelper<OrderMaster>();



            var orderMasters = new List<OrderMaster>();
            List<OrderOffer> OrderOffers = new List<OrderOffer>();
            using (var context = new AuthContext())
            {

                string query = "select a.OrderId,b.OfferCode from BillDiscounts a  inner join Offers b on a.OfferId = b.OfferId and b.OfferCode is not null and a.OrderId >0 	Union all 	select  orderid,'Flash Deal' from   FlashDealItemConsumeds group by orderid";
                OrderOffers = context.Database.SqlQuery<OrderOffer>(query).ToList();
            }



            if (OrderOffers != null && OrderOffers.Any())
            {
                foreach (var item in OrderOffers.GroupBy(x => x.OrderId))
                {
                    MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                    var mongoOrder = mongoDbHelper.Select(x => x.OrderId == item.Key, collectionName: "OrderMaster").FirstOrDefault();

                    //var filter = Builders<OrderMaster>.Filter.Eq(s => s.OrderId, item.Key);
                    //var update = Builders<OrderMaster>.Update.Set("OfferCode", string.Join("," ,item.Select(x=>x.OfferCode).ToList()));
                    //var result = await collection.UpdateOneAsync(filter, update);
                    if (mongoOrder != null)
                    {
                        mongoOrder.OfferCode = string.Join(",", item.Select(x => x.OfferCode).ToList());
                        var res = await mongoDbHelper.ReplaceAsync(mongoOrder.Id, mongoOrder);
                    }
                }
            }
            return true;
        }


        [Route("testIRN")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> testIRN()
        {
            var manager = new IRNHelper();
            var isSynced = AsyncContext.Run(() => manager.PostIRNToClearTax(318394));
            return "";
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<string> testApi()
        {
            //var manager = new IRNHelper();
            //var isSynced = AsyncContext.Run(() => manager.PostIRNToClearTax());

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var settings = new ConnectionSettings(new Uri("https://a878d9d03709431fb0feb2a8fd006e40.ent-search.asia-south1.gcp.elastic-.com/api/as/v1/engines/retailersearch/search")).DefaultIndex("retailersearch").DisableDirectStreaming(true);

            var client = new ElasticClient(settings);

            var items = client.Search<ElasticItems>(s =>
                                                    s.Query(a => a.Match(m =>
                                                                     m.Field(f => f.subcategoryname)
                                                                     .Field(f => f.subsubcategoryname)
                                                                     .Field(f => f.title)
                                                                     .Query("dabur")
                                                                 )
                                                            )
                                                    );

            return "";
        }



        [Route("MigrateAuditsToMongo")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> MigrateAuditsToMongo()
        {
            int auditCount = 0;
            List<string> entityNames = new List<string>();
            using (var context = new AuthContext())
            {
                entityNames = context.Database.SqlQuery<string>("Select distinct AuditEntity from audits").ToList();
            }
            MongoDbHelper<Audit> mongoDbHelper = new MongoDbHelper<Audit>();

            foreach (var item in entityNames)
            {
                using (var context = new AuthContext())
                {
                    auditCount = context.Database.SqlQuery<int>("Select count(AuditId) from audits where AuditEntity = '" + item + "'").FirstOrDefault();
                }

                for (int i = 0; i < auditCount; i += 10000)
                {
                    using (var context = new AuthContext())
                    {
                        var query = "Select distinct * " +
                            " from audits as a" +
                            " inner join[dbo].[AuditFields] as b on a.guid = b.auditguid and a.auditentity = '" + item + "' " +
                            " order by auditid offset " + i + " rows fetch next " + 10000 + " rows only";

                        var audits = context.Database.SqlQuery<AuditDC>(query).ToList();

                        List<Audit> auditList = audits.GroupBy(x => new
                        {
                            x.AuditId,
                            x.PkValue,
                            x.PkFieldName,
                            x.AuditDate,
                            x.UserName,
                            x.AuditAction,
                            x.AuditEntity,
                            x.TableName,
                            x.GUID
                        }).Select(x => new Audit
                        {
                            AuditAction = x.Key.AuditAction,
                            AuditDate = x.Key.AuditDate,
                            AuditEntity = x.Key.AuditEntity,
                            AuditId = x.Key.AuditId,
                            GUID = x.Key.GUID,
                            PkFieldName = x.Key.PkFieldName,
                            PkValue = x.Key.PkValue,
                            TableName = x.Key.TableName,
                            UserName = x.Key.UserName,
                            AuditFields = x.Select(z => new AuditFields
                            {
                                AuditFieldId = z.AuditFieldId,
                                AuditGuid = z.AuditGuid,
                                FieldName = z.FieldName,
                                NewValue = z.NewValue,
                                OldValue = z.OldValue
                            }).ToList()

                        }).ToList();


                        await mongoDbHelper.InsertManyAsync(auditList, string.Format("{0}_Audit", item));

                        // var abc = await mongoDbHelper.InsertMany(orders);
                    }

                }
            }




            return true;
        }



        [Route("InsertInRabbitMq")]
        [HttpGet]
        public async Task<bool> InsertInRabbitMq()
        {
            RabbitMqHelper helper = new RabbitMqHelper();
            using (var context = new AuthContext())
            {
                var order = context.DbOrderMaster.Include("orderDetails").OrderBy(x => x.OrderId).Skip(0).Take(1).FirstOrDefault();
                helper.Publish<OrderMaster>("order", order);
                return true;
            }
        }


        [Route("UpdateMongoOrderDetailsOrderedItemTotalAmt")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> MigrateOrderedTotalAmtToMongoOrderDetails()
        {
            int count = 0;
            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
            //IMongoDatabase db = mongoDbHelper.dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
            var collection = mongoDbHelper.mongoDatabase.GetCollection<MongoOrderMaster>("OrderMaster");

            List<DataContracts.Transaction.Mongo.OrderDetails> OrderDetails = new List<DataContracts.Transaction.Mongo.OrderDetails>();
            using (var context = new AuthContext())
            {
                string query = "select OrderId,OrderDetailsId,TotalAmt from OrderDetails where CreatedDate>='2019-04-01 00:00:43'";
                OrderDetails = context.Database.SqlQuery<DataContracts.Transaction.Mongo.OrderDetails>(query).ToList();
            }
            if (OrderDetails != null && OrderDetails.Any())
            {
                foreach (var item in OrderDetails)
                {
                    try
                    {
                        var filter = Builders<MongoOrderMaster>.Filter;
                        var Query = filter.And(filter.Eq(x => x.OrderId, item.OrderId),
                                    filter.ElemMatch(x => x.orderDetails, c => c.OrderDetailsId == item.OrderDetailsId));

                        //find Record exits
                        var Recordexits = collection.Find(Query).SingleOrDefault();
                        if (Recordexits != null && Recordexits.orderDetails.Any(x => x.OrderDetailsId == item.OrderDetailsId))
                        {
                            count++;
                            // update with positional 
                            var update = Builders<MongoOrderMaster>.Update;
                            var updateSetData = update.Set("orderDetails.$.OrderedTotalAmt", item.TotalAmt);


                            var result = await collection.UpdateOneAsync(Query, updateSetData);
                        }
                    }
                    catch (Exception ss)
                    {

                    }

                }
            }
            return true;
        }

        [Route("UpdateOrderInMongo/{startDate}/{endDate}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> UpdateOrderInMongo(DateTime startDate, DateTime endDate)
        {

            using (var context = new AuthContext())
            {
                var orders = context.DbOrderMaster.Where(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(startDate) && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(endDate)).ToList();

                foreach (var order in orders)
                {
                    order.comments = order.comments + " ";
                    context.Entry(order).State = EntityState.Modified;
                }
                context.Commit();
            }



            return true;
        }

        [Route("SyncOrdersInMongo/{OrderId}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> SyncOrdersInMongo(int OrderId)
        {

            MongoDbHelper<OrdersToSync> mongoDbHelperOrdersSync = new MongoDbHelper<OrdersToSync>();

            var mongoOrders = mongoDbHelperOrdersSync.Select(x => x.OrderId == OrderId && !x.IsProcessed, x => x.OrderBy(z => z.CreateOrUpdateDate), 0, 50);

            if (mongoOrders != null && mongoOrders.Any())
            {

                foreach (var order in mongoOrders.GroupBy(x => x.OrderId))
                {
                    try
                    {
                        var dbOrder = new OrderMaster();
                        using (var context = new AuthContext())
                        {
                            dbOrder = context.DbOrderMaster.Include("orderDetails").Where(x => x.OrderId == order.Key).FirstOrDefault();
                            if (dbOrder != null)
                            {
                                dbOrder.OrderDate = dbOrder.CreatedDate;
                                dbOrder.OrderAmount = dbOrder.GrossAmount;
                                foreach (var orderdetail in dbOrder.orderDetails)
                                {
                                    orderdetail.OrderedTotalAmt = orderdetail.TotalAmt;//update per item totalamount
                                }
                                DataTable orderIdDt = new DataTable();
                                orderIdDt.Columns.Add("IntValue");

                                var dr = orderIdDt.NewRow();
                                dr["IntValue"] = dbOrder.OrderId;
                                orderIdDt.Rows.Add(dr);
                                List<OrderOffer> OrderOffers = new List<OrderOffer>();
                                SqlParameter param = new SqlParameter("OrderIds", orderIdDt);
                                param.SqlDbType = SqlDbType.Structured;
                                param.TypeName = "dbo.IntValues";
                                var cmd = context.Database.Connection.CreateCommand();
                                cmd.CommandText = "[dbo].[GetOrderOffer]";
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.Parameters.Add(param);
                                context.Database.Connection.Open();
                                using (var reader = cmd.ExecuteReader())
                                {
                                    OrderOffers = ((IObjectContextAdapter)context)
                                                    .ObjectContext
                                                    .Translate<OrderOffer>(reader).ToList();
                                }

                                if (OrderOffers != null && OrderOffers.Any(x => x.OrderId == dbOrder.OrderId))
                                {
                                    var offerCodes = OrderOffers.Where(x => x.OrderId == dbOrder.OrderId).Select(x => x.OfferCode).ToList();
                                    dbOrder.OfferCode = string.Join(",", offerCodes);
                                }
                                if (dbOrder.Status == "Ready to Dispatch" || dbOrder.Status == "Post Order Canceled" || dbOrder.Status == "Delivery Redispatch"
                                        || dbOrder.Status == "Account settled" || dbOrder.Status == "Delivered"
                                        || dbOrder.Status == "Delivery Canceled"
                                        || dbOrder.Status == "Issued"
                                        || dbOrder.Status == "Partial settled"
                                        || dbOrder.Status == "Shipped"
                                        || dbOrder.Status == "sattled"
                                        )
                                {
                                    var orderDispatch = context.OrderDispatchedMasters.Include("orderDetails").Where(x => x.OrderId == order.Key).FirstOrDefault();

                                    var newdbOrder = new OrderMaster
                                    {
                                        OrderId = orderDispatch.OrderId,
                                        CompanyId = orderDispatch.CompanyId,
                                        //SalesPersonId = orderDispatch.SalesPersonId,
                                        //SalesPerson = orderDispatch.SalesPerson,
                                        //SalesMobile = orderDispatch.SalesMobile,
                                        CustomerId = orderDispatch.CustomerId,
                                        CustomerName = orderDispatch.CustomerName,
                                        Skcode = orderDispatch.Skcode,
                                        ShopName = orderDispatch.ShopName,
                                        Status = orderDispatch.Status,
                                        invoice_no = orderDispatch.invoice_no,
                                        Trupay = orderDispatch.Trupay,
                                        paymentThrough = orderDispatch.paymentThrough,
                                        TrupayTransactionId = orderDispatch.TrupayTransactionId,
                                        paymentMode = orderDispatch.paymentMode,
                                        CustomerCategoryId = orderDispatch.CustomerCategoryId,
                                        CustomerCategoryName = orderDispatch.CustomerCategoryName,
                                        CustomerType = orderDispatch.CustomerType,
                                        LandMark = dbOrder.LandMark,
                                        Customerphonenum = orderDispatch.Customerphonenum,
                                        BillingAddress = orderDispatch.BillingAddress,
                                        ShippingAddress = orderDispatch.ShippingAddress,
                                        TotalAmount = orderDispatch.TotalAmount,
                                        GrossAmount = orderDispatch.GrossAmount,
                                        OrderAmount = dbOrder.GrossAmount,
                                        DeliveredAmount = dbOrder.Status == "Delivered" || dbOrder.Status == "Partial settled" || dbOrder.Status == "sattled" ? orderDispatch.GrossAmount : (double?)null,
                                        DispatchAmount = orderDispatch.GrossAmount,
                                        DiscountAmount = orderDispatch.DiscountAmount,
                                        TaxAmount = orderDispatch.TaxAmount,
                                        SGSTTaxAmmount = orderDispatch.SGSTTaxAmmount,
                                        CGSTTaxAmmount = orderDispatch.CGSTTaxAmmount,
                                        CityId = orderDispatch.CityId,
                                        WarehouseId = orderDispatch.WarehouseId,
                                        WarehouseName = orderDispatch.WarehouseName,
                                        active = orderDispatch.active,
                                        CreatedDate = dbOrder.CreatedDate,
                                        OrderDate = orderDispatch.OrderDate,
                                        Deliverydate = orderDispatch.Deliverydate,
                                        UpdatedDate = orderDispatch.UpdatedDate,
                                        ReadytoDispatchedDate = dbOrder.ReadytoDispatchedDate,
                                        DeliveredDate = dbOrder.DeliveredDate,
                                        Deleted = orderDispatch.Deleted,
                                        ReDispatchCount = orderDispatch.ReDispatchCount,
                                        DivisionId = orderDispatch.DivisionId,
                                        ReasonCancle = dbOrder.ReasonCancle,
                                        ClusterId = orderDispatch.ClusterId,
                                        ClusterName = orderDispatch.ClusterName,
                                        deliveryCharge = orderDispatch.deliveryCharge,
                                        WalletAmount = orderDispatch.WalletAmount,
                                        walletPointUsed = dbOrder.walletPointUsed,
                                        UsedPoint = dbOrder.UsedPoint,
                                        RewardPoint = orderDispatch.RewardPoint,
                                        ShortAmount = orderDispatch.ShortAmount,
                                        comments = orderDispatch.comments,
                                        OrderTakenSalesPersonId = orderDispatch.OrderTakenSalesPersonId,
                                        OrderTakenSalesPerson = orderDispatch.OrderTakenSalesPerson,
                                        Tin_No = orderDispatch.Tin_No,
                                        ShortReason = dbOrder.ShortReason,
                                        orderProcess = orderDispatch.orderProcess,
                                        accountProcess = dbOrder.accountProcess,
                                        chequeProcess = dbOrder.chequeProcess,
                                        epaymentProcess = dbOrder.epaymentProcess,
                                        Savingamount = orderDispatch.Savingamount,
                                        OnlineServiceTax = orderDispatch.OnlineServiceTax,
                                        InvoiceBarcodeImage = orderDispatch.InvoiceBarcodeImage != null ? orderDispatch.InvoiceBarcodeImage : new List<byte>().ToArray(),
                                        userid = orderDispatch.userid,
                                        Description = dbOrder.Description,
                                        IsLessCurrentStock = dbOrder.IsLessCurrentStock,
                                        BillDiscountAmount = orderDispatch.BillDiscountAmount,
                                        offertype = orderDispatch.offertype,
                                        DeliveryIssuanceIdOrderDeliveryMaster = orderDispatch.DeliveryIssuanceIdOrderDeliveryMaster,
                                        OrderDispatchedMasterId = orderDispatch.OrderDispatchedMasterId,
                                        OfferCode = dbOrder.OfferCode,
                                        EwayBillNumber = orderDispatch.EwayBillNumber,
                                        CreditNoteNumber = orderDispatch.PocCreditNoteNumber,
                                        CreditNoteDate = orderDispatch.PocCreditNoteDate,
                                        OrderType = dbOrder.OrderType,
                                        orderDetails = orderDispatch.orderDetails.Select(z => new Model.OrderDetails
                                        {
                                            OrderDetailsId = z.OrderDetailsId,
                                            OrderId = z.OrderId,
                                            CustomerId = z.CustomerId,
                                            CustomerName = z.CustomerName,
                                            City = z.City,
                                            Mobile = z.Mobile,
                                            OrderDate = z.OrderDate,
                                            CompanyId = z.CompanyId,
                                            CityId = z.CityId,
                                            WarehouseId = z.WarehouseId,
                                            WarehouseName = z.WarehouseName,
                                            CategoryName = z.CategoryName,
                                            SubcategoryName = z.SubcategoryName,
                                            SubsubcategoryName = z.SubsubcategoryName,
                                            SellingSku = z.SellingSku,
                                            ItemId = z.ItemId,
                                            Itempic = z.Itempic,
                                            itemname = z.itemname,
                                            SellingUnitName = z.SellingUnitName,
                                            itemcode = z.itemcode,
                                            itemNumber = z.itemNumber,
                                            HSNCode = z.HSNCode,
                                            Barcode = z.Barcode,
                                            price = z.price,
                                            UnitPrice = z.UnitPrice,
                                            Purchaseprice = z.Purchaseprice,
                                            MinOrderQty = z.MinOrderQty,
                                            MinOrderQtyPrice = z.MinOrderQtyPrice,
                                            qty = z.qty,
                                            Noqty = z.Noqty,
                                            AmtWithoutTaxDisc = z.AmtWithoutTaxDisc,
                                            AmtWithoutAfterTaxDisc = z.AmtWithoutAfterTaxDisc,
                                            TotalAmountAfterTaxDisc = z.TotalAmountAfterTaxDisc,
                                            NetAmmount = z.NetAmmount,
                                            DiscountPercentage = z.DiscountPercentage,
                                            DiscountAmmount = z.DiscountAmmount,
                                            NetAmtAfterDis = z.NetAmtAfterDis,
                                            TaxPercentage = z.TaxPercentage,
                                            TaxAmmount = z.TaxAmmount,
                                            SGSTTaxPercentage = z.SGSTTaxPercentage,
                                            SGSTTaxAmmount = z.SGSTTaxAmmount,
                                            CGSTTaxPercentage = z.CGSTTaxPercentage,
                                            CGSTTaxAmmount = z.CGSTTaxAmmount,
                                            TotalCessPercentage = z.TotalCessPercentage,
                                            CessTaxAmount = z.CessTaxAmount,
                                            TotalAmt = z.TotalAmt,
                                            OrderedTotalAmt = dbOrder.orderDetails.FirstOrDefault(e => e.ItemId == z.ItemId)?.TotalAmt ?? 0,
                                            CreatedDate = dbOrder.orderDetails.FirstOrDefault(x => x.ItemId == z.ItemId).CreatedDate,
                                            UpdatedDate = dbOrder.orderDetails.FirstOrDefault(x => x.ItemId == z.ItemId).UpdatedDate,
                                            Deleted = z.Deleted,
                                            //Status = z.Status,
                                            SizePerUnit = z.SizePerUnit,
                                            marginPoint = dbOrder.orderDetails.FirstOrDefault(x => x.ItemId == z.ItemId)?.marginPoint,
                                            promoPoint = dbOrder.orderDetails.FirstOrDefault(x => x.ItemId == z.ItemId)?.promoPoint,
                                            NetPurchasePrice = dbOrder.orderDetails.FirstOrDefault(x => x.ItemId == z.ItemId).NetPurchasePrice,
                                            SupplierName = dbOrder.orderDetails.FirstOrDefault(x => x.ItemId == z.ItemId).SupplierName,
                                            ItemMultiMRPId = z.ItemMultiMRPId
                                        }).ToList()
                                    };
                                    dbOrder = newdbOrder;
                                }

                                var history = context.OrderMasterHistoriesDB.Where(x => x.orderid == order.Key).ToList();
                                dbOrder.OrderMasterHistories = history;
                            }
                        }
                        if (dbOrder != null)
                        {
                            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                            var mongoOrder = mongoDbHelper.Select(x => x.OrderId == order.Key, collectionName: "OrderMaster").FirstOrDefault();

                            //var mongoDbHelperOrder = new MongoDbHelper<OrderMaster>();
                            if (mongoOrder != null)
                            {
                                mongoDbHelper.Delete(mongoOrder.Id, collectionName: "OrderMaster");

                            }

                            var MOrder = Mapper.Map(dbOrder).ToANew<MongoOrderMaster>();

                            mongoDbHelper.Insert(MOrder, "OrderMaster");

                            foreach (var item in order)
                            {
                                item.IsProcessed = true;
                                item.ProcessedDate = DateTime.Now;
                                mongoDbHelperOrdersSync.ReplaceWithoutFind(item.Id, item);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        TextFileLogHelper.LogError("Error in UpdateOrderInMongo for Order Id: " + order.Key + Environment.NewLine + ex.ToString());

                    }


                }
            }

            return true;
        }

        [Route("SyncThreeMonthInMongo")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> SyncThreeMonthInMongo()
        {
            OrderMasterChangeDetectManager.UpdateLastThreemonthOrdersInMongo();
            return true;
        }

        [Route("SyncIR/{WarehouseId}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> SyncIR(int WarehouseId)
        {
            var IRSyncs = new List<IRSync>();
            using (var context = new AuthContext())
            {
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[MigrateIRMasterNew]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("WarehouseId", WarehouseId));
                cmd.CommandTimeout = 600;
                context.Database.Connection.Open();
                context.Database.CommandTimeout = 600;
                using (var reader = cmd.ExecuteReader())
                {
                    IRSyncs = ((IObjectContextAdapter)context)
                                    .ObjectContext
                                    .Translate<IRSync>(reader).ToList();
                }
                var poIds = IRSyncs.Select(x => x.PurchaseOrderId).Distinct().ToList();

                var GrsDb = context.GoodsReceivedDetail.Where(x => poIds.Contains(x.PurchaseOrderDetail.PurchaseOrderId) && x.Qty > 0 && x.Status == 2 && (!x.IsDeleted.HasValue || !x.IsDeleted.Value) && x.IsActive).Include(x => x.PurchaseOrderDetail).ToList();
                List<GoodsReceivedDetailNewDc> Grs = Mapper.Map(GrsDb).ToANew<List<GoodsReceivedDetailNewDc>>();
                List<InvoiceReceiptDetail> InvoiceReceiptDetails = new List<InvoiceReceiptDetail>();

                foreach (var POGr in Grs.OrderBy(x => x.PurchaseOrderDetail.PurchaseOrderId).ThenBy(x => x.GrSerialNumber))
                {
                    if (IRSyncs != null && IRSyncs.Any(x => x.PurchaseOrderId == POGr.PurchaseOrderDetail.PurchaseOrderId && x.ItemMultiMRPId == POGr.ItemMultiMRPId && x.PoInwardQty > 0))
                    {
                        var Ir = IRSyncs.Where(x => x.PurchaseOrderId == POGr.PurchaseOrderDetail.PurchaseOrderId && x.ItemMultiMRPId == POGr.ItemMultiMRPId && x.PoInwardQty > 0).OrderBy(x => x.IrNumber).FirstOrDefault();
                        if (Ir != null)
                        {
                            InvoiceReceiptDetails.Add(new InvoiceReceiptDetail
                            {
                                ApprovedBy = Ir.Ir1PersonId ?? 0,
                                CessTaxPercentage = Ir.CessTaxPercentage,
                                CreatedBy = Ir.Ir1PersonId ?? 0,
                                CreatedDate = Ir.Ir1Date.Value,
                                DiscountAmount = Ir.DisscountAmount,
                                DiscountPercent = Ir.DiscountPercent,
                                GSTPercentage = Ir.TotalTaxPercentage ?? 0,
                                IsFreeItem = false,
                                Price = Ir.IrPrice,
                                IRMasterId = Ir.Id,
                                IRQuantity = POGr.Qty <= Ir.PoInwardQty ? POGr.Qty : Ir.PoInwardQty,
                                TotalTaxPercentage = Ir.TotalTaxPercentage ?? 0,
                                GoodsReceivedDetailId = POGr.Id,
                                IsActive = true,
                                IsDeleted = false,
                                Status = 1,
                                CessTaxAmount = 0,
                                TotalTaxAmount = 0,
                                GSTAmount = 0
                            });
                            if (POGr.Qty < Ir.PoInwardQty)
                            {
                                Ir.PoInwardQty = Ir.PoInwardQty - POGr.Qty;
                                POGr.Qty = 0;
                            }
                            else if (POGr.Qty > Ir.PoInwardQty)
                            {
                                POGr.Qty -= Ir.PoInwardQty;
                                Ir.PoInwardQty = 0;
                                while (POGr.Qty > 0)
                                {
                                    var Ir1 = IRSyncs.Where(x => x.PurchaseOrderId == POGr.PurchaseOrderDetail.PurchaseOrderId && x.ItemMultiMRPId == POGr.ItemMultiMRPId && x.PoInwardQty > 0).OrderBy(x => x.IrNumber).FirstOrDefault();
                                    if (Ir1 != null)
                                    {
                                        InvoiceReceiptDetails.Add(new InvoiceReceiptDetail
                                        {
                                            ApprovedBy = Ir1.Ir1PersonId ?? 0,
                                            CessTaxPercentage = Ir1.CessTaxPercentage,
                                            CreatedBy = Ir1.Ir1PersonId ?? 0,
                                            CreatedDate = Ir1.Ir1Date.Value,
                                            DiscountAmount = Ir.DisscountAmount,
                                            DiscountPercent = Ir.DiscountPercent,
                                            GSTPercentage = Ir1.TotalTaxPercentage ?? 0,
                                            IsFreeItem = false,
                                            Price = Ir1.IrPrice,
                                            IRMasterId = Ir1.Id,
                                            IRQuantity = POGr.Qty <= Ir1.PoInwardQty ? POGr.Qty : Ir1.PoInwardQty,
                                            TotalTaxPercentage = Ir1.TotalTaxPercentage ?? 0,
                                            GoodsReceivedDetailId = POGr.Id,
                                            IsActive = true,
                                            IsDeleted = false,
                                            Status = 1,
                                            CessTaxAmount = 0,
                                            TotalTaxAmount = 0,
                                            GSTAmount = 0
                                        });
                                        if (POGr.Qty <= Ir1.PoInwardQty)
                                        {
                                            Ir1.PoInwardQty = Ir1.PoInwardQty - POGr.Qty;
                                            POGr.Qty = 0;
                                        }
                                        else
                                        {
                                            POGr.Qty -= Ir1.PoInwardQty;
                                            Ir1.PoInwardQty = 0;
                                        }
                                    }
                                    else
                                        POGr.Qty = 0;
                                }
                            }
                            else
                                Ir.PoInwardQty = 0;
                        }
                    }

                }

                InvoiceReceiptDetails.ForEach(x =>
                {
                    if (x.CessTaxPercentage.HasValue && x.CessTaxPercentage.Value > 0)
                    {
                        x.CessTaxAmount = x.IRQuantity * x.Price * (x.CessTaxPercentage.Value / 100);
                    }
                    if (x.TotalTaxPercentage > 0)
                    {
                        x.TotalTaxAmount = x.IRQuantity * x.Price * (x.TotalTaxPercentage / 100);
                        x.GSTAmount = x.TotalTaxAmount;
                    }

                });

                context.InvoiceReceiptDetail.AddRange(InvoiceReceiptDetails);
                context.Commit();
            }
            return true;
        }

        [Route("AutoSettleSupplierLedger/{supplierid}")]
        [HttpGet]
        [AllowAnonymous]
        public void AutoSettleSupplierLedger(int supplierid)
        {
            List<string> supplierList = new List<string>
            {
                "SKS1749"
            };

            foreach (var item in supplierList)
            {

                using (AuthContext db = new AuthContext())
                {
                    var supplier = db.Suppliers.FirstOrDefault(x => x.SUPPLIERCODES.ToLower() == item.ToLower());

                    if (supplier != null)
                    {
                        supplierid = supplier.SupplierId;
                        List<IRPaymentDetailsDCTemp> irPaymentDetailList = db.IRPaymentDetailsDB.Where(x => x.SupplierId == supplierid && x.TotalReaminingAmount != 0 && x.Deleted == false && x.IsActive == true && x.PaymentDate > new DateTime(2019, 04, 01) && x.PaymentDate < new DateTime(2020, 4, 1) && x.IsIROutstandingPending != true).
                        //List<IRPaymentDetailsDCTemp> irPaymentDetailList = db.IRPaymentDetailsDB.Where(x => x.SupplierId == supplierid && x.TotalReaminingAmount != 0 && x.Deleted == false && x.IsActive == true && x.IsIROutstandingPending != true).
                        Select(x => new IRPaymentDetailsDCTemp
                        {
                            BankId = x.BankId,
                            BankName = x.BankName,
                            RefNo = x.RefNo,
                            SupplierId = x.SupplierId,
                            TotalAmount = x.TotalAmount,
                            TotalReaminingAmount = x.TotalReaminingAmount,
                            Id = x.Id,
                            Guid = x.Guid,
                            PaymentDate = x.PaymentDate.Value
                        }).OrderBy(x => x.PaymentDate).ToList();

                        if (irPaymentDetailList != null && irPaymentDetailList.Count > 0)
                        {
                            foreach (var irPaymentDetail in irPaymentDetailList)
                            {
                                var query = @"select MAX(II.InvoiceDate) ,
                                            IRM.Id
                                           ,IRM.[PurchaseOrderId]
                                           ,IRM.[IRID]
                                           ,IRM.[supplierId]
                                           ,IRM.[SupplierName]
                                           ,IRM.[WarehouseId]
                                           ,IRM.[TotalAmount]
                                           ,IRM.[IRStatus]
                                           ,IRM.[Gstamt]
                                           ,IRM.[TotalTaxPercentage]
                                           ,IRM.[Discount]
                                           ,IRM.[IRAmountWithTax]
                                           ,IRM.[IRAmountWithOutTax]
                                           ,IRM.[TotalAmountRemaining]
                                           ,IRM.[PaymentStatus]
                                           ,IRM.[PaymentTerms]
                                           ,IRM.[IRType]
                                           ,IRM.[CreatedBy]
                                           ,IRM.[CreationDate]
                                           ,IRM.[Deleted]
                                           ,IRM.[Progres]
                                           ,IRM.[Remark]
                                           ,IRM.[RejectedComment]
                                           ,IRM.[BuyerId]
                                           ,IRM.[BuyerName]
                                           ,IRM.[ApprovedComment]
                                           ,IRM.[OtherAmount]
                                           ,IRM.[OtherAmountRemark]
                                           ,IRM.[ExpenseAmount]
                                           ,IRM.[ExpenseAmountRemark]
                                           ,IRM.[RoundofAmount]
                                           ,IRM.[ExpenseAmountType]
                                           ,IRM.[OtherAmountType]
                                           ,IRM.[RoundoffAmountType]
                                           ,IRM.[DueDays]
                                           ,IRM.[CashDiscount]
                                           ,IRM.[FreightAmount]
                                           ,IRM.[IrSerialNumber]
                                           ,IRM.[InvoiceNumber]
                                           ,IRM.[InvoiceDate]
                                           , 0 as UpdatedBy 
                                           , null as UpdatedDate 
                                           ,IRM.IRApprovedDate from IRMasters IRM
                                INNER JOIN InvoiceImages II 
	                                ON IRM.PurchaseOrderId = II.PurchaseOrderId
	                                AND IRM.IRID = II.InvoiceNumber
	                                AND IRM.IRStatus = 'Approved from Buyer side'
                                where irm.supplierId = @supplierId
                                group by    IRM.Id
                                           ,IRM.[PurchaseOrderId]
                                           ,IRM.[IRID]
                                           ,IRM.[supplierId]
                                           ,IRM.[SupplierName]
                                           ,IRM.[WarehouseId]
                                           ,IRM.[TotalAmount]
                                           ,IRM.[IRStatus]
                                           ,IRM.[Gstamt]
                                           ,IRM.[TotalTaxPercentage]
                                           ,IRM.[Discount]
                                           ,IRM.[IRAmountWithTax]
                                           ,IRM.[IRAmountWithOutTax]
                                           ,IRM.[TotalAmountRemaining]
                                           ,IRM.[PaymentStatus]
                                           ,IRM.[PaymentTerms]
                                           ,IRM.[IRType]
                                           ,IRM.[CreatedBy]
                                           ,IRM.[CreationDate]
                                           ,IRM.[Deleted]
                                           ,IRM.[Progres]
                                           ,IRM.[Remark]
                                           ,IRM.[RejectedComment]
                                           ,IRM.[BuyerId]
                                           ,IRM.[BuyerName]
                                           ,IRM.[ApprovedComment]
                                           ,IRM.[OtherAmount]
                                           ,IRM.[OtherAmountRemark]
                                           ,IRM.[ExpenseAmount]
                                           ,IRM.[ExpenseAmountRemark]
                                           ,IRM.[RoundofAmount]
                                           ,IRM.[ExpenseAmountType]
                                           ,IRM.[OtherAmountType]
                                           ,IRM.[RoundoffAmountType]
                                           ,IRM.[DueDays]
                                           ,IRM.[CashDiscount]
                                           ,IRM.[FreightAmount]
                                           ,IRM.[IrSerialNumber]
                                           ,IRM.[InvoiceNumber]
                                           ,IRM.[InvoiceDate]
                                           ,IRM.IRApprovedDate
                                order by MAX(II.InvoiceDate)";
                                query = query.Replace("@supplierId", (supplierid.ToString() + " "));
                                List<IRMaster> irList = db.Database.SqlQuery<IRMaster>(query).ToList();

                                if (irPaymentDetail.TotalReaminingAmount != null && irPaymentDetail.TotalReaminingAmount > 0.99)
                                {
                                    irPaymentDetail.TotalReaminingAmount = Math.Round(irPaymentDetail.TotalReaminingAmount.Value);
                                    List<IRMasterDTO> irMasterDTOList = new List<IRMasterDTO>();
                                    if (irList != null && irList.Count > 0)
                                    {

                                        foreach (IRMaster master in irList)
                                        {
                                            if (irPaymentDetail.TotalReaminingAmount > 0)
                                            {
                                                IRMasterDTO tempMaster = new IRMasterDTO
                                                {
                                                    BuyerName = master.BuyerName,
                                                    CreatedBy = master.CreatedBy,
                                                    CreationDate = master.CreationDate,
                                                    Discount = master.Discount,
                                                    Gstamt = master.Gstamt,
                                                    IRAmountWithOutTax = master.IRAmountWithOutTax,
                                                    IRAmountWithTax = master.IRAmountWithTax,
                                                    Id = master.Id,
                                                    IRID = master.IRID,
                                                    IRStatus = master.IRStatus,
                                                    IRType = master.IRType,
                                                    PaymentStatus = "",
                                                    PaymentTerms = master.PaymentTerms,
                                                    PurchaseOrderId = master.PurchaseOrderId,
                                                    ReamainingAmt = master.TotalAmountRemaining,
                                                    TotalTaxPercentage = master.TotalTaxPercentage,
                                                    RejectedComment = master.RejectedComment,
                                                    supplierId = master.supplierId,
                                                    SupplierName = master.SupplierName,
                                                    TotalAmount = master.TotalAmount,
                                                    WarehouseId = master.WarehouseId,
                                                    TotalAmountRemaining = master.TotalAmountRemaining
                                                };
                                                irMasterDTOList.Add(tempMaster);
                                                if (Math.Round(tempMaster.ReamainingAmt.Value) > irPaymentDetail.TotalReaminingAmount)
                                                {
                                                    tempMaster.TotalAmountRemaining = irPaymentDetail.TotalReaminingAmount.Value;
                                                    tempMaster.ReamainingAmt -= irPaymentDetail.TotalReaminingAmount;
                                                    irPaymentDetail.TotalReaminingAmount = 0;
                                                    tempMaster.PaymentStatus = "partial paid";
                                                    tempMaster.IRStatus = "Approved from Buyer side";
                                                }
                                                else
                                                {
                                                    irPaymentDetail.TotalReaminingAmount -= Math.Round(tempMaster.ReamainingAmt.Value);
                                                    tempMaster.ReamainingAmt = 0;
                                                    tempMaster.PaymentStatus = "paid";
                                                    tempMaster.IRStatus = "Paid";
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    if (irPaymentDetail.TotalReaminingAmount >= 0)
                                    {
                                        JavaScriptSerializer js = new JavaScriptSerializer();

                                        irPaymentDetail.RefNo = irPaymentDetail.RefNo;
                                        irPaymentDetail.Remark = "Auto Settle";
                                        irPaymentDetail.IRList = js.Serialize(irMasterDTOList);
                                        IRHelper irHelper = new IRHelper();

                                        IRPaymentDetails detail = new IRPaymentDetails
                                        {
                                            BankId = irPaymentDetail.BankId,
                                            BankName = irPaymentDetail.BankName,
                                            RefNo = irPaymentDetail.RefNo,
                                            SupplierId = irPaymentDetail.SupplierId,
                                            TotalAmount = irPaymentDetail.TotalAmount,
                                            TotalReaminingAmount = irPaymentDetail.TotalReaminingAmount,
                                            Id = irPaymentDetail.Id,
                                            Guid = irPaymentDetail.Guid,
                                            IRList = irPaymentDetail.IRList,

                                        };

                                        IRPaymentDetails irdetails = irHelper.SettlerPaymentList(detail, 0);
                                    }

                                    if (irPaymentDetail.TotalReaminingAmount > 0)
                                    {
                                        DateTime pymentDate = db.IRPaymentDetailsDB.First(x => x.Id == irPaymentDetail.Id).PaymentDate.Value;
                                        IRHelper.DebitLedgerEntryIRPayment(irPaymentDetail.SupplierId, db, irPaymentDetail.TotalReaminingAmount.Value, null, 0, 0, 0, "", irPaymentDetail.RefNo, irPaymentDetail.Remark, irPaymentDetail.Guid, irPaymentDetail.BankId, pymentDate, irPaymentDetail.Id);
                                    }

                                }

                            }
                        }

                    }
                }
            }
        }


        [Route("RollBackIssue")]
        [HttpGet]
        [AllowAnonymous]
        public void RollBackIssue()
        {
            using (var scope = new TransactionScope())
            {
                using (var context = new AuthContext())
                {
                    City city = new City
                    {
                        active = true,
                        aliasName = "a",
                        CityLatitude = 1.1,
                        CityLongitude = 1.2,
                        CityName = "aa",
                        Code = "x",
                        CompanyId = 1,
                        CreatedBy = "a",
                        Deleted = false,
                        CreatedDate = DateTime.Now,
                        IsSupplier = false,
                        Stateid = 1,
                        StateName = "x",
                        UpdatedDate = DateTime.Now
                    };

                    context.Cities.Add(city);
                    context.Commit();

                    city = new City
                    {
                        active = true,
                        aliasName = "a",
                        CityLatitude = 1.1,
                        CityLongitude = 1.2,
                        CityName = "aa",
                        Code = "x",
                        CompanyId = 1,
                        CreatedBy = "a",
                        Deleted = false,
                        CreatedDate = DateTime.Now,
                        IsSupplier = false,
                        Stateid = 1,
                        StateName = "x",
                        UpdatedDate = DateTime.Now
                    };

                    context.Cities.Add(city);
                    context.Commit();


                    city = new City
                    {
                        active = true,
                        aliasName = "a",
                        CityLatitude = 1.1,
                        CityLongitude = 1.2,
                        CityName = "aa",
                        Code = "x",
                        CompanyId = 1,
                        CreatedBy = "a",
                        Deleted = false,
                        CreatedDate = DateTime.Now,
                        IsSupplier = false,
                        Stateid = 1,
                        StateName = "x",
                        UpdatedDate = DateTime.Now
                    };

                    context.Cities.Add(city);
                    scope.Dispose();


                }
            }
        }


        [Route("GetItem/{pageSize}/{pageNo}")]
        [HttpGet]
        [AllowAnonymous]
        public PaggingData GetItem(int pageSize, int pageNo)
        {
            using (var context = new AuthContext())
            {

                try
                {

                    var itemPagedList = context.AllItemMasterForPaging(pageSize, pageNo, 1, "Default");
                    return itemPagedList;
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }

        [Route("DailyFifo")]
        [HttpGet]
        [AllowAnonymous]
        public void DailyFifo()
        {
            OrderMasterChangeDetectManager.DailyFIFO();
        }

        [Route("OtherStockInOut")]
        [HttpGet]
        [AllowAnonymous]
        public void OtherStockInOut()
        {
            OrderMasterChangeDetectManager.DailyOtherStockFIFO();
        }

        [Route("GenerateInOutForOtherStock")]
        [HttpGet]
        [AllowAnonymous]
        public void GenerateCurrentMonthInOutForOtherStock()
        {
            OrderMasterChangeDetectManager.GenerateCurrentMonthInOutForOtherStock();
        }


        [Route("GenerateInOutFile")]
        [HttpGet]
        [AllowAnonymous]
        public void GenerateInOutFile()
        {
            var param = new BuyerDashboardParams();
            param.StartDate = new DateTime(DateTime.Now.Date.AddDays(-1).Year, DateTime.Now.Date.AddDays(-1).Month, 1);
            param.EndDate = DateTime.Now.Date.AddSeconds(-1);

            var fileName = "InOut_" + param.EndDate.ToString("yyyyddMM") + ".xlsx";
            string folderPath = Path.Combine(HttpRuntime.AppDomainAppPath, "ExcelGeneratePath", "GeneratedInOut");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string path = Path.Combine(folderPath, fileName);
            if (!File.Exists(path))
            {
                ItemLedgerManager manager = new ItemLedgerManager();
                var data = AsyncContext.Run(() => manager.GetDataFromDb(param));

                DataTable dt = ListtoDataTableConverter.ToDataTable(data);

                ExcelGenerator.DataTable_To_Excel(dt, "InOut", path);
            }
        }

        [Route("GetOrderForORToolList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<DeliveryResult>> GetOrderForORToolList(int clusterId, DateTime startDate, DateTime endDate, bool isTestModel)
        {
            int userid = GetLoginUserId();
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            var result = await tripPlannerHelper.GenerateTrip(clusterId, DateTime.Today, endDate, isTestModel, userid);
            return result;
        }

        [Route("GetAllOrderForORToolList")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetAllOrderForORToolList()
        {
            int userid = GetLoginUserId();
            TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
            var result = await tripPlannerHelper.GenerateAllTrip(userid, false);
            return result.IsSuceess;
        }

        [Route("UpdateTripSequence")]
        [HttpGet]
        [AllowAnonymous]
        public void UpdateTripSequence(long id)
        {
            try
            {
                using (var authcontext = new AuthContext())
                {
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    //tripPlannerHelper.UpdateTripSequence(id, authcontext);
                    tripPlannerHelper.UpdateTripSequenceNew(id, authcontext);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [Route("TestFDB")]
        [HttpGet]
        [AllowAnonymous]
        public void DeliveredOrderToSkFranchiseTest()
        {
            #region  DeliveredOrderToSkFranchise    ----CreatePoDoGRN

            using (var db = new AuthContext())
            {
                List<DeliveredOrderToFranchise> ChangeOrderLists = db.DeliveredOrderToFranchises.Where(x => x.IsProcessed == false).ToList();
                if (ChangeOrderLists != null && ChangeOrderLists.Any())
                {
                    var tradeUrl = ConfigurationManager.AppSettings["FranchiseAPIurl"] + "api/FranchisePurchaseOrder/CreatePoDoGRN";

                    TextFileLogHelper.TraceLog(tradeUrl);
                    var orderIds = ChangeOrderLists.Select(x => x.OrderId).Distinct().ToList();
                    var orders = db.OrderDispatchedDetailss.Where(x => orderIds.Contains(x.OrderId)).ToList();
                    var itemNumbers = orders.Select(x => x.itemNumber).Distinct().ToList();
                    var warehouseIds = orders.Select(x => x.WarehouseId).Distinct().ToList();
                    var customerWarehouses = db.Warehouses.Where(x => warehouseIds.Contains(x.WarehouseId)).ToList();
                    var centralItems = db.ItemMasterCentralDB.Where(x => itemNumbers.Contains(x.Number) && x.Deleted == false).Distinct().ToList();
                    var itemmultimrpids = orders.Select(x => x.ItemMultiMRPId).Distinct().ToList();
                    var ItemMultiMRPLists = db.ItemMultiMRPDB.Where(x => itemmultimrpids.Contains(x.ItemMultiMRPId)).ToList();
                    foreach (var item in orders.GroupBy(x => new { x.CustomerId, x.WarehouseId, x.OrderId }))
                    {
                        var orderItemnumbers = item.Select(x => x.itemNumber).ToList();
                        FranchiseMaster ItemToSync = new FranchiseMaster
                        {
                            CustomerId = item.Key.CustomerId,
                            OrderId = item.Key.OrderId,
                            Warehouse = Mapper.Map(customerWarehouses.FirstOrDefault(x => x.WarehouseId == item.Key.WarehouseId)).ToANew<WarehouseDc>(),
                            ItemMasterCentrals = Mapper.Map(centralItems.Where(x => orderItemnumbers.Contains(x.Number)).Distinct().ToList()).ToANew<List<ItemMasterCentralDc>>(),
                            FranchiseItems = item.Select(x => new FranchiseItem
                            {
                                ItemNumber = x.itemNumber,
                                MRP = x.price,
                                Qty = x.qty,
                                UnitofQuantity = ItemMultiMRPLists.FirstOrDefault(z => z.ItemMultiMRPId == x.ItemMultiMRPId).UnitofQuantity,
                                UOM = ItemMultiMRPLists.FirstOrDefault(z => z.ItemMultiMRPId == x.ItemMultiMRPId).UOM,
                                PurchasePrice = x.UnitPrice
                            }).ToList()
                        };
                        var OrderIdslist = item.Select(x => x.OrderId).ToList();
                        var ChangeOrderListsToUpdate = ChangeOrderLists.Where(x => OrderIdslist.Contains(x.OrderId)).ToList();
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                var newJson = JsonConvert.SerializeObject(ItemToSync);
                                using (var content = new StringContent(newJson, Encoding.UTF8, "application/json"))
                                {
                                    var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));
                                    response.EnsureSuccessStatusCode();
                                    string responseBody = response.Content.ReadAsStringAsync().Result;
                                    var result = JsonConvert.DeserializeObject<ResponseMetaData>(responseBody);
                                    if (result.Status != "Error")
                                    {
                                        ChangeOrderListsToUpdate.ForEach(x =>
                                        {
                                            x.IsProcessed = true;
                                            x.Error = null;
                                            x.UpdatedDate = DateTime.Now;
                                            db.Entry(x).State = EntityState.Modified;
                                        });
                                    }
                                    else
                                    {
                                        ChangeOrderListsToUpdate.ForEach(x =>
                                        {
                                            x.IsProcessed = false;
                                            x.UpdatedDate = DateTime.Now;
                                            x.Error = result.ErrorMessage;
                                            db.Entry(x).State = EntityState.Modified;
                                        });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ChangeOrderListsToUpdate.ForEach(x =>
                            {
                                x.IsProcessed = true;
                                x.UpdatedDate = DateTime.Now;
                                x.Error = ex.ToString();
                                db.Entry(x).State = EntityState.Modified;
                            });
                        }
                    }
                    db.Commit();
                }
            }

            #endregion
        }


        [Route("InsertRetailerTraceLog")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> InsertRetailerTraceLog(string tracelogname)
        {
            ReportManager reportManager = new ReportManager();
            return await reportManager.InsertRetailerTraceLog(tracelogname);
        }

        [Route("BulkInsertRetailerTraceLog")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> BulkInsertRetailerTraceLog(DateTime startDate, DateTime endDate)
        {
            bool result = false;
            var CollectionName = "TraceLog_" + DateTime.Now.AddDays(-1).ToString(@"MMddyyyy");
            ReportManager reportManager = new ReportManager();
            List<DateTime> DateLst = new List<DateTime>();
            for (DateTime counter = startDate; counter <= endDate; counter = counter.AddDays(1))
            {
                DateLst.Add(counter);
            }
            ParallelLoopResult loopResult = Parallel.ForEach(DateLst, (dt) =>
            {
                CollectionName = "TraceLog_" + dt.ToString(@"MMddyyyy");
                AsyncContext.Run(() => reportManager.InsertRetailerTraceLog(CollectionName));
            });
            if (loopResult.IsCompleted)
                result = true;

            return result;
        }


        [Route("IRupdate")]
        [HttpGet]
        [AllowAnonymous]
        public bool UpdateLegerandIRpaid()
        {
            List<int> ids = new List<int>()
            {  47148  };
            using (var authContext = new AuthContext())
            {


                foreach (var item in ids)
                {
                    IRMaster irdata = authContext.IRMasterDB.Where(x => x.Id == item).FirstOrDefault();

                    irdata.PaymentStatus = "paid";
                    irdata.IRStatus = "Paid";
                    authContext.Entry(irdata).State = EntityState.Modified;

                    #region ledger entry
                    LadgerHelper ladgerHelper = new LadgerHelper();

                    Ladger ledger = ladgerHelper.GetOrCreateLadgerTypeAndLadger("Supplier", irdata.supplierId, 1, authContext);
                    long? ladgerID = ledger.ID;

                    var voucherid = authContext.LadgerEntryDB.Where(x => x.ObjectID == irdata.Id && x.ObjectType == "IR" && x.VouchersTypeID == 5).FirstOrDefault()?.VouchersNo;


                    LadgerEntry ledgerEntry = new LadgerEntry();
                    ledgerEntry.LagerID = ladgerID;
                    ledgerEntry.Debit = irdata.TotalAmountRemaining;
                    ledgerEntry.Active = true;
                    ledgerEntry.CreatedBy = 1;
                    ledgerEntry.CreatedDate = DateTime.Now;
                    ledgerEntry.UpdatedBy = 1;
                    ledgerEntry.UpdatedDate = DateTime.Now;
                    ledgerEntry.Date = irdata.InvoiceDate;
                    ledgerEntry.AffectedLadgerID = 24317;
                    ledgerEntry.VouchersTypeID = 22;
                    ledgerEntry.VouchersNo = voucherid;
                    ledgerEntry.Particulars = irdata.InvoiceNumber;
                    ledgerEntry.ObjectID = (long)irdata.Id;
                    ledgerEntry.ObjectType = "IR";
                    ledgerEntry.UploadGUID = null;
                    ledgerEntry.RefNo = null;
                    ledgerEntry.Remark = "Remaining Amount Adjust from Backend";
                    ledgerEntry.IrPaymentDetailsId = null;
                    authContext.LadgerEntryDB.Add(ledgerEntry);

                    LadgerEntry oppositeLedgerEntry = new LadgerEntry();
                    oppositeLedgerEntry.LagerID = ledgerEntry.AffectedLadgerID;
                    oppositeLedgerEntry.Credit = irdata.TotalAmountRemaining;
                    oppositeLedgerEntry.Active = true;
                    oppositeLedgerEntry.CreatedBy = 1;
                    oppositeLedgerEntry.CreatedDate = DateTime.Now;
                    oppositeLedgerEntry.UpdatedBy = 1;
                    oppositeLedgerEntry.UpdatedDate = DateTime.Now;
                    oppositeLedgerEntry.Date = irdata.InvoiceDate;
                    oppositeLedgerEntry.AffectedLadgerID = ledgerEntry.LagerID;
                    oppositeLedgerEntry.VouchersTypeID = 22;
                    oppositeLedgerEntry.VouchersNo = voucherid;
                    oppositeLedgerEntry.Particulars = irdata.InvoiceNumber;
                    oppositeLedgerEntry.ObjectID = (long)irdata.Id;
                    oppositeLedgerEntry.ObjectType = "IR";
                    oppositeLedgerEntry.UploadGUID = null;
                    oppositeLedgerEntry.RefNo = null;
                    oppositeLedgerEntry.Remark = "Remaining Amount Adjust from Backend";
                    oppositeLedgerEntry.IrPaymentDetailsId = null;
                    authContext.LadgerEntryDB.Add(oppositeLedgerEntry);
                    #endregion

                    authContext.Commit();


                }
            }
            return true;
        }

        [Route("GenerateInOut")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GenerateInOut()
        {
            try
            {
                var param = new BuyerDashboardParams();
                param.StartDate = new DateTime(DateTime.Now.Date.AddDays(-1).Year, DateTime.Now.Date.AddDays(-1).Month, 1);
                param.EndDate = DateTime.Now.Date.AddSeconds(-1);

                var fileName = "InOut_" + param.EndDate.ToString("yyyyddMM") + ".xlsx";
                string folderPath = Path.Combine(HttpRuntime.AppDomainAppPath, "ExcelGeneratePath", "GeneratedInOut");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string path = Path.Combine(folderPath, fileName);
                if (!File.Exists(path))
                {
                    if (param.EndDate.Day == 9 /*DateTime.DaysInMonth(param.EndDate.Year, param.EndDate.Month)*/)
                    {
                        string[] files = Directory.GetFiles(folderPath, "InOut_" + param.EndDate.ToString("yyyy") + "*" + param.EndDate.ToString("MM") + ".xlsx", SearchOption.TopDirectoryOnly);
                        if (files != null && files.Count() > 0)
                        {
                            files.ToList().ForEach(x =>
                            {
                                File.Delete(x);
                            });
                        }
                    }

                    ItemLedgerManager manager = new ItemLedgerManager();
                    var data = AsyncContext.Run(() => manager.GetDataFromDb(param));

                    //DataTable dt = ListtoDataTableConverter.ToDataTable(data);

                    DataTable dt = new DataTable();
                    dt.Columns.Add("StartDate");
                    dt.Columns.Add("EndDate");
                    dt.Columns.Add("ItemMultiMrpId", typeof(int));
                    dt.Columns.Add("ItemName");
                    dt.Columns.Add("ItemCode");
                    dt.Columns.Add("BrandId", typeof(int));
                    dt.Columns.Add("Brand");
                    dt.Columns.Add("CategoryId", typeof(int));
                    dt.Columns.Add("Category");
                    dt.Columns.Add("WarehouseId", typeof(int));
                    dt.Columns.Add("WarehouseName");
                    dt.Columns.Add("BuyerId", typeof(int));
                    dt.Columns.Add("BuyerName");
                    dt.Columns.Add("MRP", typeof(double));
                    dt.Columns.Add("TaxRate", typeof(double));
                    dt.Columns.Add("SellingPrice", typeof(double));
                    dt.Columns.Add("OpeningQty", typeof(int));
                    dt.Columns.Add("OpeningAmount", typeof(double));
                    dt.Columns.Add("POInwardQty", typeof(int));
                    dt.Columns.Add("POInwardAmount", typeof(double));
                    dt.Columns.Add("WhInQty", typeof(int));
                    dt.Columns.Add("WhInAmount", typeof(double));
                    dt.Columns.Add("WhOutQty", typeof(int));
                    dt.Columns.Add("WhOutAmount", typeof(double));
                    dt.Columns.Add("SaleQty", typeof(int));
                    dt.Columns.Add("SaleAmount", typeof(double));
                    dt.Columns.Add("PilferageQty", typeof(int));
                    dt.Columns.Add("PilferageAmount", typeof(double));
                    dt.Columns.Add("CancelInQty", typeof(int));
                    dt.Columns.Add("CancelInAmount", typeof(double));
                    dt.Columns.Add("FreeInQty", typeof(int));
                    dt.Columns.Add("FreeInAmount", typeof(double));
                    dt.Columns.Add("FreeOutQty", typeof(int));
                    dt.Columns.Add("FreeOutAmount", typeof(double));
                    dt.Columns.Add("POReturnQty", typeof(int));
                    dt.Columns.Add("POReturnAmount", typeof(double));
                    dt.Columns.Add("ManualInQty", typeof(int));
                    dt.Columns.Add("ManualInAmount", typeof(double));
                    dt.Columns.Add("ErrorInQty", typeof(int));
                    dt.Columns.Add("ErrorInAmount", typeof(double));
                    dt.Columns.Add("ManualOutQty", typeof(int));
                    dt.Columns.Add("ManualOutAmount", typeof(double));
                    dt.Columns.Add("DamageOutQty", typeof(int));
                    dt.Columns.Add("DamageOutAmount", typeof(double));
                    dt.Columns.Add("ExpiryOutQty", typeof(int));
                    dt.Columns.Add("ExpiryOutAmount", typeof(double));
                    dt.Columns.Add("StockTransferInQty", typeof(int));
                    dt.Columns.Add("StockTransferInAmount", typeof(double));
                    dt.Columns.Add("StockTransferOutQty", typeof(int));
                    dt.Columns.Add("StockTransferOutAmount", typeof(double));
                    dt.Columns.Add("ClosingQty", typeof(int));
                    dt.Columns.Add("ClosingAmount", typeof(double));
                    dt.Columns.Add("IntransitQty", typeof(int));
                    dt.Columns.Add("IntransitAmount", typeof(double));
                    dt.Columns.Add("TotalInQty", typeof(int));
                    dt.Columns.Add("TotalOutQty", typeof(int));
                    dt.Columns.Add("InOutDiffQty", typeof(int));
                    dt.Columns.Add("InOutDiffAmount", typeof(double));
                    dt.Columns.Add("FrontMargin", typeof(double));
                    dt.Columns.Add("InvoiceDiscount", typeof(double));


                    foreach (var x in data)
                    {
                        DataRow dr = dt.NewRow();

                        dr["StartDate"] = x.StartDate;
                        dr["EndDate"] = x.EndDate;
                        dr["ItemMultiMrpId"] = x.ItemMultiMrpId;
                        dr["ItemName"] = x.ItemName;
                        dr["ItemCode"] = x.ItemCode;
                        dr["BrandId"] = x.BrandId;
                        dr["Brand"] = x.Brand;
                        dr["CategoryId"] = x.CategoryId;
                        dr["Category"] = x.Category;
                        dr["WarehouseId"] = x.WarehouseId;
                        dr["WarehouseName"] = x.WarehouseName;
                        dr["BuyerId"] = x.BuyerId;
                        dr["BuyerName"] = x.BuyerName;
                        dr["MRP"] = x.MRP ?? 0;
                        dr["TaxRate"] = x.TaxRate ?? 0;
                        dr["SellingPrice"] = x.SellingPrice ?? 0;
                        dr["OpeningQty"] = x.OpeningQty ?? 0;
                        dr["OpeningAmount"] = x.OpeningAmount ?? 0;
                        dr["POInwardQty"] = x.POInwardQty ?? 0;
                        dr["POInwardAmount"] = x.POInwardAmount ?? 0;
                        dr["WhInQty"] = x.WhInQty ?? 0;
                        dr["WhInAmount"] = x.WhInAmount ?? 0;
                        dr["WhOutQty"] = x.WhOutQty ?? 0;
                        dr["WhOutAmount"] = x.WhOutAmount ?? 0;
                        dr["SaleQty"] = x.SaleQty ?? 0;
                        dr["SaleAmount"] = x.SaleAmount ?? 0;
                        dr["PilferageQty"] = x.PilferageQty ?? 0;
                        dr["PilferageAmount"] = x.PilferageAmount ?? 0;
                        dr["CancelInQty"] = x.CancelInQty ?? 0;
                        dr["CancelInAmount"] = x.CancelInAmount ?? 0;
                        dr["FreeInQty"] = x.FreeInQty;
                        dr["FreeInAmount"] = x.FreeInAmount ?? 0;
                        dr["FreeOutQty"] = x.FreeOutQty;
                        dr["FreeOutAmount"] = x.FreeOutAmount ?? 0;
                        dr["POReturnQty"] = x.POReturnQty ?? 0;
                        dr["POReturnAmount"] = x.POReturnAmount ?? 0;
                        dr["ManualInQty"] = x.ManualInQty ?? 0;
                        dr["ManualInAmount"] = x.ManualInAmount ?? 0;
                        dr["ErrorInQty"] = x.ErrorInQty ?? 0;
                        dr["ErrorInAmount"] = x.ErrorInAmount ?? 0;
                        dr["ManualOutQty"] = x.ManualOutQty ?? 0;
                        dr["ManualOutAmount"] = x.ManualOutAmount ?? 0;
                        dr["DamageOutQty"] = x.DamageOutQty ?? 0;
                        dr["DamageOutAmount"] = x.DamageOutAmount ?? 0;
                        dr["ExpiryOutQty"] = x.ExpiryOutQty ?? 0;
                        dr["ExpiryOutAmount"] = x.ExpiryOutAmount ?? 0;
                        dr["StockTransferInQty"] = x.StockTransferInQty ?? 0;
                        dr["StockTransferInAmount"] = x.StockTransferInAmount ?? 0;
                        dr["StockTransferOutQty"] = x.StockTransferOutQty ?? 0;
                        dr["StockTransferOutAmount"] = x.StockTransferOutAmount ?? 0;
                        dr["ClosingQty"] = x.ClosingQty ?? 0;
                        dr["ClosingAmount"] = x.ClosingAmount ?? 0;
                        dr["IntransitQty"] = x.IntransitQty ?? 0;
                        dr["IntransitAmount"] = x.IntransitAmount ?? 0;
                        dr["TotalInQty"] = x.TotalInQty ?? 0;
                        dr["TotalOutQty"] = x.TotalOutQty ?? 0;
                        dr["InOutDiffQty"] = x.InOutDiffQty ?? 0;
                        dr["InOutDiffAmount"] = x.InOutDiffAmount ?? 0;
                        dr["FrontMargin"] = x.FrontMargin ?? 0;
                        dr["InvoiceDiscount"] = x.InvoiceDiscount ?? 0;

                        dt.Rows.Add(dr);
                    }


                    ExcelGenerator.DataTable_To_Excel(dt, "InOut", path);
                }
            }
            catch (Exception ex)
            {
                //EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- GenerateCurrentMonthInOut Error", ex.ToString(), "");

            }

            return true;
        }


        [Route("GetActiveWarehouseCity")]
        [HttpGet]
        [AllowAnonymous]
        public List<WarehousesCityDTO> GetActiveWarehouseCity()
        {
            using (AuthContext db = new AuthContext())
            {
                string Sql = "select distinct a.Cityid,a.CityName from Warehouses a with(nolock) inner join GMWarehouseProgresses b  with(nolock) on a.WarehouseId=b.WarehouseID where active=1 and Deleted=0 and IsKPP=0 and b.IsLaunched=1";
                List<WarehousesCityDTO> cityData = db.Database.SqlQuery<WarehousesCityDTO>(Sql).ToList();
                return cityData;
            }
        }


        [HttpGet]
        [Route("GetClusterCityWise")]
        [AllowAnonymous]
        public dynamic GetClusterCityWise(int cityid)
        {
            using (AuthContext db = new AuthContext())
            {
                List<Cluster> c = new List<Cluster>();
                c.Add(new Cluster
                {
                    ClusterId = 0,
                    ClusterName = "Out of Cluster",
                    LtLng = new List<LtLng>()
                });

                c.AddRange(db.Clusters.Include("LtLng").Where(a => a.CityId == cityid).ToList());


                return c.Select(x =>
                  new
                  {
                      x.ClusterId,
                      x.ClusterName,
                      clusterlatlng = x.LtLng.Select(y => new { lat = y.latitude, lng = y.longitude }).ToList()
                  }).ToList();
            }
        }


        [HttpPost]
        [Route("GetClusterCustomers")]
        [AllowAnonymous]
        public async Task<List<CustomerCluster>> GetClusterCustomers(string cityName, List<int> clusterids)
        {
            using (AuthContext db = new AuthContext())
            {
                List<CustomerCluster> CustomerClusters = new List<CustomerCluster>();
                if (clusterids.Any())
                {
                    var cityIdParam = new SqlParameter("cityname", cityName);

                    var clusteridslist = new DataTable();
                    clusteridslist.Columns.Add("IntValue");
                    foreach (var item in clusterids)
                    {
                        var dr = clusteridslist.NewRow();
                        dr["IntValue"] = item;
                        clusteridslist.Rows.Add(dr);
                    }
                    var CIds = new SqlParameter("Clusterids", clusteridslist);
                    CIds.SqlDbType = SqlDbType.Structured;
                    CIds.TypeName = "dbo.IntValues";
                    CustomerClusters = await db.Database.SqlQuery<CustomerCluster>("exec GetByClusteridsCustomers @cityname, @Clusterids", cityIdParam, CIds).ToListAsync();

                    //string Sql = "Select a.customerId,a.Name,a.mobile,a.Skcode,a.ShopName,a.ShippingAddress,a.LandMark,a.lat,a.lg from customers a with(nolock) where a.ClusterId in (" + string.Join(",", clusterids) + ") and a.Active = 1 and not exists(select 1 from CustomerLatLngVerifies b with(nolock) where a.CustomerId = b.CustomerId)";
                    //CustomerClusters = db.Database.SqlQuery<CustomerCluster>(Sql).ToList();
                }
                return CustomerClusters;
            }
        }

        [HttpPost]
        [Route("UpdateCustomer")]
        [AllowAnonymous]
        public bool UpdateCustomer(CustomerLatLngVerify Customer)
        {
            bool result = false;

            if (!Customer.CreatedBy.HasValue)
                return false;
            using (AuthContext db = new AuthContext())
            {
                var cust = db.Customers.FirstOrDefault(x => x.CustomerId == Customer.CustomerId);
                if (cust != null)
                {
                    var customerLatLngVerify = db.CustomerLatLngVerify.FirstOrDefault(x => x.CustomerId == Customer.CustomerId && x.AppType == (int)AppEnum.SalesApp);
                    Customer.IsActive = true;
                    Customer.IsDeleted = false;
                    if (Customer.IsNotCheckedInYet)
                    {
                        var customerStatus = db.Customers.Where(x => x.Active == true && x.Deleted == false).FirstOrDefault();
                        if (customerStatus != null)
                        {
                            CustomerStatusHistory obj = new CustomerStatusHistory();
                            obj.CustomerVerify = customerStatus.CustomerVerify;
                            obj.CustomerId = customerStatus.CustomerId;
                            obj.SkCode = customerStatus.Skcode;
                            obj.CreatedDate = DateTime.Now;
                            obj.CreatedBy = Convert.ToInt32(Customer.CreatedBy);
                            obj.AppType = (int)AppEnum.SalesApp;
                            db.CustomerStatusHistoryDb.Add(obj);
                        }
                        cust.CustomerVerify = "Not Verified";
                        cust.LastModifiedBy = Convert.ToString(Customer.CreatedBy);
                        cust.LastModifiedBy = Convert.ToString(DateTime.Now);
                        db.Entry(cust).State = EntityState.Modified;
                    }
                    if (customerLatLngVerify == null)
                    {
                        Customer.Status = 1;  // 1 Request
                        Customer.CreatedDate = DateTime.Now;
                        Customer.AppType = (int)AppEnum.SalesApp;
                        db.CustomerLatLngVerify.Add(Customer);
                    }
                    else
                    {
                        customerLatLngVerify.CaptureImagePath = Customer.CaptureImagePath;
                        customerLatLngVerify.NewShippingAddress = Customer.NewShippingAddress;
                        customerLatLngVerify.Newlat = Customer.Newlat;
                        customerLatLngVerify.Newlg = Customer.Newlg;
                        customerLatLngVerify.ShopFound = Customer.ShopFound;
                        customerLatLngVerify.Status = 1;    // 1 Request
                        customerLatLngVerify.Aerialdistance = Customer.Aerialdistance;
                        customerLatLngVerify.ModifiedDate = DateTime.Now;
                        customerLatLngVerify.IsActive = true;
                        customerLatLngVerify.IsDeleted = false;
                        customerLatLngVerify.ModifiedBy = Customer.CreatedBy;
                        //mbd
                        customerLatLngVerify.AreaName = Customer.AreaName;
                        //if (!customerLatLngVerify.CreatedBy.HasValue)
                        customerLatLngVerify.CreatedBy = Customer.CreatedBy;
                        db.Entry(customerLatLngVerify).State = EntityState.Modified;
                    }
                    result = db.Commit() > 0;
                }

                return result;
            }
        }


        [Route("UploadCustomerShopImage")]
        [HttpPost]
        [AllowAnonymous]
        public string UploadCustomerShopImage()
        {
            string LogoUrl = "";

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                if (httpPostedFile != null)
                {
                    if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/CustomerShopImage")))
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/CustomerShopImage"));

                    string extension = Path.GetExtension(httpPostedFile.FileName);

                    string fileName = httpPostedFile.FileName.Substring(0, httpPostedFile.FileName.LastIndexOf('.')) + DateTime.Now.ToString("ddMMyyyyHHmmss") + extension;

                    LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/CustomerShopImage"), fileName);

                    httpPostedFile.SaveAs(LogoUrl);
                    AngularJSAuthentication.Common.Helpers.FileUploadHelper.Upload(fileName, "~/CustomerShopImage", LogoUrl);

                    LogoUrl = "/CustomerShopImage/" + fileName;
                }
            }
            return LogoUrl;
        }


        //[Route("TestChqbookBookRefund")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<string> ChqbookBookRefund()
        //{
        //    ChqbookBookRefundHelper helper = new ChqbookBookRefundHelper();
        //    return await helper.ChqbookBookRefund();
        //}




        [Route("TestItemLimitlessMoqAutoInactive")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> ItemLimitlessMoqAutoInactive()
        {
            using (var context = new AuthContext())
            {
                List<int> ItemIds = await context.Database.SqlQuery<int>("exec ItemLimitlessMoqAutoInactive").ToListAsync();
                if (ItemIds != null && ItemIds.Any())
                {
                    List<ItemMaster> itemlist = await context.itemMasters.Where(x => ItemIds.Contains(x.ItemId)).ToListAsync();
                    if (itemlist != null && itemlist.Any())
                    {
                        foreach (var item in itemlist)
                        {
                            item.active = false;
                            item.Reason = "Item Limit less then Moq Auto Inactive of MRPID : " + item.ItemMultiMRPId;
                            item.UpdatedDate = DateTime.Now;
                            context.Entry(item).State = EntityState.Modified;
                        }
                    }
                    await context.CommitAsync();
                }
            }
            return true;
        }

        [Route("InsertTodayRetailerTraceLog")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> InsertTodayRetailerTraceLog(string tracelogname)
        {
            ReportManager reportManager = new ReportManager();
            return await reportManager.InsertTodayRetailerTraceLog(tracelogname);
        }

        [Route("Publish")]
        [HttpPost]
        [AllowAnonymous]
        public bool PublishInRabbitMQ(BatchCodeSubject subject)
        {
            Publisher publisher = new Publisher();
            publisher.PublishInBatchCode(subject);
            return true;
        }


        [Route("CreateIndex")]
        [HttpGet]
        [AllowAnonymous]
        public bool CreateIndex()
        {
            BatchManager.Helpers.ElasticBatchHelper elasticBatchHelper = new BatchManager.Helpers.ElasticBatchHelper();
            elasticBatchHelper.CreateIndex();
            return true;
        }

        [Route("GenerateOrderAmtQRCode")]
        [HttpGet]
        [AllowAnonymous]
        public QRPaymentResponseDc GenerateOrderAmtQRCode(int orderId)
        {
            QRPaymentResponseDc result = new QRPaymentResponseDc();
            using (var context = new AuthContext())
            {
                double amount = context.PaymentResponseRetailerAppDb.Any(x => x.OrderId == orderId && x.IsOnline == false && x.status == "Success" && x.IsRefund == false) ? context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == orderId && x.IsOnline == false && x.status == "Success" && x.IsRefund == false).Sum(x => x.amount) : 0;

                if (amount > 0)
                {
                    string UPIUrl = ConfigurationManager.AppSettings["HDFCUPIUrl"];
                    if (!string.IsNullOrEmpty(UPIUrl))
                    {

                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/OrderQRCode")))
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/OrderQRCode"));

                        string fileName = orderId.ToString() + '_' + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".jpeg";
                        string LogoUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/OrderQRCode"), fileName);
                        UPIUrl = UPIUrl.Replace("[TrancId]", orderId.ToString()).Replace("[Amount]", string.Format("{0:0.00}", amount));
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(UPIUrl, QRCodeGenerator.ECCLevel.Q);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, true);
                        qrCodeImage.Save(LogoUrl, System.Drawing.Imaging.ImageFormat.Jpeg);
                        result.Status = true;
                        result.QRCodeurl = string.Format("{0}://{1}:{2}/{3}", new Uri(HttpContext.Current.Request.Url.AbsoluteUri).Scheme
                                                                    , HttpContext.Current.Request.Url.DnsSafeHost
                                                                    , (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443 ? ":" + HttpContext.Current.Request.Url.Port : "")
                                                                     , "/OrderQRCode/" + fileName);
                    }
                    else
                    {
                        result.Status = false;
                        result.msg = "QR not generated due to UPI Url not found.";
                    }
                }
                else
                {
                    result.Status = false;
                    result.msg = "QR not generated due to already paid order amount.";
                }
                return result;
            }
        }
        [HttpPost]
        [Route("ETDDateUpadteOrder")]
        public IHttpActionResult ETDDateUpadteOrder(DateTime ETDDate)
        {
            strJSON = "Data Not Updated!!";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Get the uploaded image from the Files collection
                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                    // Validate the uploaded image(optional)
                    byte[] buffer = new byte[httpPostedFile.ContentLength];

                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;
                    //   XSSFWorkbook workbook1;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                    }
                    if (ETDDate != null)
                    {
                        string sSheetName = hssfwb.GetSheetName(0);
                        ISheet sheet = hssfwb.GetSheet(sSheetName);

                        IRow rowData;
                        ICell cellData = null;

                        List<ETADateUpdateOrderDc> eTADateUpdateOrderDc = new List<ETADateUpdateOrderDc>();
                        int? txnIdCellIndex = null;
                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            if (iRowIdx == 0)
                            {
                                rowData = sheet.GetRow(iRowIdx);

                                if (rowData != null)
                                {
                                    txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "OrderId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "OrderId").ColumnIndex : (int?)null;
                                    if (!txnIdCellIndex.HasValue)
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("OrderId does not exist..try again");
                                        return Created(strJSON, strJSON);
                                    }
                                }
                            }
                            else
                            {
                                rowData = sheet.GetRow(iRowIdx);
                                cellData = rowData.GetCell(0);
                                rowData = sheet.GetRow(iRowIdx);
                                if (rowData != null)
                                {
                                    ETADateUpdateOrderDc trnfrorder = new ETADateUpdateOrderDc();
                                    try
                                    {
                                        //int requestTowarehouseId;
                                        cellData = rowData.GetCell(txnIdCellIndex.Value);
                                        col0 = cellData == null ? "" : cellData.ToString();
                                        trnfrorder.OrderId = Convert.ToInt32(col0);
                                        eTADateUpdateOrderDc.Add(trnfrorder);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                            }
                        }
                        using (var context = new AuthContext())
                        {
                            if (eTADateUpdateOrderDc.Any() && eTADateUpdateOrderDc.Any())
                            {
                                var OrderIds = eTADateUpdateOrderDc.Select(x => x).Distinct().ToList();
                                var eTADate = new SqlParameter("@ETADate", ETDDate);

                                var IdDt = new DataTable();
                                IdDt = new DataTable();
                                IdDt.Columns.Add("IntValue");
                                foreach (var item in OrderIds)
                                {
                                    DataRow dr = IdDt.NewRow();
                                    dr["IntValue"] = (item.OrderId);
                                    IdDt.Rows.Add(dr);
                                }
                                var orderids = new SqlParameter
                                {
                                    ParameterName = "OrderIds",

                                    SqlDbType = SqlDbType.Structured,
                                    TypeName = "dbo.IntValues",
                                    Value = IdDt
                                };
                                context.Database.ExecuteSqlCommand("EXEC UpdateETADateOrder @ETADate,@OrderIds", eTADate, orderids);
                                strJSON = "Order Update Successfully!!";
                            }
                            return Created(strJSON, strJSON);
                        }
                    }
                    //return Created(strJSON, strJSON);
                }
            }
            return Created(strJSON, strJSON);
        }



        [HttpPost]
        [Route("RTDUpdateddate")]
        public IHttpActionResult RTDUpdateddate(DateTime RtdDate, int Wid)
        {
            strJSON = "Data Not Updated!!";
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Get the uploaded image from the Files collection
                System.Web.HttpPostedFile httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    var FileUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles"), httpPostedFile.FileName);
                    // Validate the uploaded image(optional)
                    byte[] buffer = new byte[httpPostedFile.ContentLength];

                    using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                    {
                        br.Read(buffer, 0, buffer.Length);
                    }
                    XSSFWorkbook hssfwb;
                    //   XSSFWorkbook workbook1;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStream.Write(buffer, 0, buffer.Length);
                        memStream.Seek(0, SeekOrigin.Begin);
                        hssfwb = new XSSFWorkbook(memStream);
                    }
                    if (RtdDate != null)
                    {
                        string sSheetName = hssfwb.GetSheetName(0);
                        ISheet sheet = hssfwb.GetSheet(sSheetName);

                        IRow rowData;
                        ICell cellData = null;

                        List<ETADateUpdateOrderDc> eTADateUpdateOrderDc = new List<ETADateUpdateOrderDc>();
                        int? txnIdCellIndex = null;
                        for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                        {
                            if (iRowIdx == 0)
                            {
                                rowData = sheet.GetRow(iRowIdx);

                                if (rowData != null)
                                {
                                    txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "OrderId") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "OrderId").ColumnIndex : (int?)null;
                                    if (!txnIdCellIndex.HasValue)
                                    {
                                        JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("OrderId does not exist..try again");
                                        return Created(strJSON, strJSON);
                                    }
                                }
                            }
                            else
                            {
                                rowData = sheet.GetRow(iRowIdx);
                                cellData = rowData.GetCell(0);
                                rowData = sheet.GetRow(iRowIdx);
                                if (rowData != null)
                                {
                                    ETADateUpdateOrderDc trnfrorder = new ETADateUpdateOrderDc();
                                    try
                                    {
                                        //int requestTowarehouseId;
                                        cellData = rowData.GetCell(txnIdCellIndex.Value);
                                        col0 = cellData == null ? "" : cellData.ToString();
                                        trnfrorder.OrderId = Convert.ToInt32(col0);
                                        eTADateUpdateOrderDc.Add(trnfrorder);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                            }
                        }
                        using (var context = new AuthContext())
                        {
                            if (eTADateUpdateOrderDc.Any() && eTADateUpdateOrderDc.Any())
                            {
                                //var data = context.OrderMasterbymobile.Where(x => x.ItemMultiMRPId == i.Mrpid && x.WarehouseId == wid && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();



                                var OrderIds = eTADateUpdateOrderDc.Select(x => x).Distinct().ToList();
                                var rTADate = new SqlParameter("@RtdDate", RtdDate);
                                var WarehouseId = new SqlParameter("@WarehouseId", Wid);

                                var IdDt = new DataTable();
                                IdDt = new DataTable();
                                IdDt.Columns.Add("IntValue");
                                foreach (var item in OrderIds)
                                {
                                    DataRow dr = IdDt.NewRow();
                                    dr["IntValue"] = (item.OrderId);
                                    IdDt.Rows.Add(dr);
                                }
                                var orderids = new SqlParameter
                                {
                                    ParameterName = "OrderIds",

                                    SqlDbType = SqlDbType.Structured,
                                    TypeName = "dbo.IntValues",
                                    Value = IdDt
                                };
                                context.Database.ExecuteSqlCommand("EXEC UpdateRtdDateOrder @RtdDate,@OrderIds ,@WarehouseId", rTADate, orderids, WarehouseId);
                                strJSON = "Order Update Successfully!!";

                            }
                            return Created(strJSON, strJSON);
                        }
                    }
                    //return Created(strJSON, strJSON);
                }
            }
            return Created(strJSON, strJSON);
        }



        #region UpdateCustomerLatLong by Sudhir 06-12-2022
        [Route("UpdateCustomerLatLong")]
        [HttpGet]
        public bool UpdateCustomerLatLong()//string skcode
        {
            bool result = false;
            using (AuthContext myContext = new AuthContext())
            {
                //var querys = "select * from [dbo].[SkcodeFromLatLng] where skcode='" + skcode.Trim() + "'";
                //var custdata = myContext.Database.SqlQuery<UpdateCustomerLatLongDc>(querys).ToList();
                var querys = " select * from [dbo].[SkcodeFromLatLng] ";//ORDER BY Skcode OFFSET 3000 ROWS FETCH NEXT 1000 ROWS ONLY;
                var custdata = myContext.Database.SqlQuery<UpdateCustomerLatLongDc>(querys).ToList();
                var skcodes = custdata.Select(x => x.Skcode.Trim()).Distinct().ToList();
                List<Customer> customer = myContext.Customers.Where(x => skcodes.Contains(x.Skcode)).ToList();
                //List<Customer> customer = myContext.Customers.Where(x => x.Skcode == skcode).ToList();
                var CustomerIds = customer.Select(x => x.CustomerId).ToList();
                var ordermaster = myContext.DbOrderMaster.Where(x => CustomerIds.Contains(x.CustomerId) && x.Status == "Pending").ToList();
                var ALlGetClusterData = myContext.Clusters.ToList();
                customer.ForEach(item =>
                {
                    //foreach (var item in customer)
                    //{
                    var order = ordermaster.Where(x => x.CustomerId == item.CustomerId).FirstOrDefault();
                    if (order == null)
                    {
                        double lt = 0, lng = 0;
                        var filecust = custdata.Where(x => x.Skcode == item.Skcode).FirstOrDefault();
                        if (filecust != null)
                        {
                            var latlngarr = filecust.LatLong.Split(',');
                            if (latlngarr.Length == 2)
                            {
                                lt = Convert.ToDouble(latlngarr[0].Trim());
                                lng = Convert.ToDouble(latlngarr[1].Trim());
                            }
                            else
                            {
                                lt = 0; lng = 0;
                            }
                            if (item.lat != lt && item.lg != lng && item.lat != 0 && item.lg != 0)
                            {

                                var query = new StringBuilder("select [dbo].[GetClusterFromLatLng]('").Append(lt).Append("', '").Append(lng).Append("')");
                                myContext.Database.CommandTimeout = 300;
                                var clusterId = myContext.Database.SqlQuery<int?>(query.ToString()).FirstOrDefault();
                                if (clusterId.HasValue)
                                {
                                    var getClusterData = ALlGetClusterData.Where(x => x.ClusterId == clusterId).FirstOrDefault();
                                    if (getClusterData != null)
                                    {
                                        if (item.ClusterId == getClusterData.ClusterId)
                                        {
                                            item.lat = lt;
                                            item.lg = lng;
                                        }
                                        else
                                        {
                                            item.lat = lt;
                                            item.lg = lng;
                                            item.Warehouseid = getClusterData.WarehouseId;
                                            item.WarehouseName = getClusterData.WarehouseName;
                                            item.ClusterId = getClusterData.ClusterId;
                                            item.ClusterName = getClusterData.ClusterName;
                                        }
                                        myContext.Entry(item).State = EntityState.Modified;
                                    }
                                }
                            }
                        }
                    }
                });
                result = myContext.Commit() > 0;
                return result;
            }
        }
        #endregion

        [HttpGet]
        [Route("InsertAllItem")]
        public bool InsertAllItemToElastic()
        {
            bool Result = false;
            using (AuthContext context = new AuthContext())
            {
                var elasticSearchItems = context.Database.SqlQuery<DataContracts.ElasticSearch.ElasticSearchItem>("Exec GetAllItemForElasticSearch").ToList();
                ElasticSearchHelper elasticHelper = new ElasticSearchHelper();
                Result = elasticHelper.DeleteAllRecoredElasticIndex(ConfigurationManager.AppSettings["ElasticSearchIndexName"]);
                if (Result)
                    Result = elasticHelper.BulkInsert(elasticSearchItems);
            }
            return Result;
        }

        [Route("GetCRMPDetailL")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> GetCRMPDetailL()
        {
            bool res = false;
            CRMManager cRMManager = new CRMManager();
            res = await cRMManager.GetCRMPDetail();
            return res;
        }

        [Route("WarehouseTransStartStop")]
        [HttpGet]
        [AllowAnonymous]
        public bool WarehouseTransStartStop(int IsStop)
        {
            ItemLedgerManager mg = new ItemLedgerManager();
            return mg.WarehouseTransStartStop(IsStop);
        }
        [Route("GenrateAccountMonthEndData")]
        [HttpGet]
        [AllowAnonymous]
        public bool GenrateAccountMonthEndData()
        {
            ReportManager report = new ReportManager();
            return report.GetAccountMonthEndData();
        }
        [Route("AutoBatchSubscribe")]
        [HttpGet]
        [AllowAnonymous]
        public bool AutoBatchSubscribe()
        {
            ReportManager report = new ReportManager();
            return report.AutoBatchSubscribe();
        }


        [Route("OrderPDF")]
        [HttpGet]
        [AllowAnonymous]
        public async Task OrderPDF(int orderid)
        {
            BusinessLayer.Managers.OrderMasterManager manager = new BusinessLayer.Managers.OrderMasterManager();
            await manager.GetOrderInvoiceHtml(orderid);
        }


        [Route("CompleteOrder")]
        [HttpGet]
        [AllowAnonymous]
        public async Task CompleteOrder(string TransactionId, bool IsSuccess)
        {
            ScaleUpManager scaleUpManager = new ScaleUpManager();
            var resp = await scaleUpManager.OrderCompleted(TransactionId, IsSuccess);
        }



        [Route("TestConsumerItem")]
        [HttpGet]
        [AllowAnonymous]
        public bool TestConsumerItem(int ItemId)
        {
            ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
            elasticSalesAppClusterItem.InsertInElasticWithItemId(new ItemIdCls { ItemId = ItemId });
            return true;
        }

        [Route("InsertConsumerWarehouseItem")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> InsertConsumerWarehouseItem(int WhId)
        {
            ElasticSalesAppClusterItemDataHelper elasticSalesAppClusterItem = new ElasticSalesAppClusterItemDataHelper();
            await elasticSalesAppClusterItem.InsertAllConsumerItem(WhId);
            return true;
        }

        [Route("DeleteAllRecoredElasticIndex")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> DeleteAllRecoredElasticIndex(string indexName)
        {
            ElasticSearchHelper elasticHelper = new ElasticSearchHelper();
            elasticHelper.DeleteAllRecoredElasticIndex(indexName);            
            return true;
        }

       

        [Route("TestZilaOrder")]
        [HttpGet]
        [AllowAnonymous]
        public async Task TestZilaOrder(int OrderId)
        {
            OrderMasterManager manager = new OrderMasterManager();
            var resp = await manager.ZilaOrderProcess(OrderId);
        }

    }
}

public class UpdateCustomerLatLongDc
{
    public string Skcode { get; set; }
    public string LatLong { get; set; }
}

public class CustomerCluster
{

    public string Name { get; set; }
    public int CustomerId { get; set; }
    public string Skcode { get; set; }
    public string ShopName { get; set; }
    public string Mobile { get; set; }
    public string ShippingAddress { get; set; }
    public string LandMark { get; set; }
    public double lat { get; set; }
    public double lg { get; set; }
    public string CityName { set; get; }
}


public class WarehousesCityDTO
{
    public string CityName { get; set; }
    public int Cityid { get; set; }

}

internal class IRPaymentDetailsDCTemp
{
    public int Id { get; set; }
    public int BankId { get; set; }
    public string BankName { get; set; }
    public string RefNo { get; set; }
    public double TotalAmount { get; set; }
    public double? TotalReaminingAmount { get; set; }
    public string Remark { get; set; }
    public string IRList { get; set; }
    public string Guid { get; set; }
    public int SupplierId { get; set; }
    public DateTime PaymentDate { get; set; }
}

internal class AuditDC
{
    public long AuditId { get; set; }
    public string PkValue { get; set; }
    public string PkFieldName { get; set; }
    public DateTime AuditDate { get; set; }
    public string UserName { get; set; }
    public string AuditAction { get; set; }
    public string AuditEntity { get; set; }
    public string TableName { get; set; }
    public string GUID { get; set; }
    public long AuditFieldId { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string AuditGuid { get; set; }
}

public class IRSync
{
    public int ItemMultiMRPId { get; set; }
    public string PurchaseSku { get; set; }
    public int WarehouseId { get; set; }
    public int PurchaseOrderId { get; set; }
    public int IrNumber { get; set; }
    public int PoInwardQty { get; set; }
    public double IrPrice { get; set; }
    public double? TotalTaxPercentage { get; set; }
    public double? CessTaxPercentage { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime? Ir1Date { get; set; }
    public int? Ir1PersonId { get; set; }
    public double? DiscountPercent { get; set; }
    public double? DisscountAmount { get; set; }
    public int Id { get; set; }
}


public class GoodsReceivedDetailNewDc
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public int? ModifiedBy { get; set; }
    public int PurchaseOrderDetailId { get; set; }
    public int ItemMultiMRPId { get; set; }
    public int Qty { get; set; }
    public int DamageQty { get; set; }
    public int ExpiryQty { get; set; }
    public bool IsFreeItem { get; set; }
    public int GrSerialNumber { get; set; } //gr serial number
    public double Price { get; set; }
    public int Status { get; set; } // 1= Pending for Checker Side, 2=Approved , 3=Reject
    public int CurrentStockHistoryId { get; set; }
    public string BatchNo { get; set; }
    public DateTime? MFGDate { get; set; }
    public string Barcode { get; set; }
    public int ApprovedBy { get; set; }//approved by or rejectby
    public string VehicleType { get; set; }
    public string VehicleNumber { get; set; }
    public int? WithPurchaseOrderDetailId { get; set; }
    public string Comment { get; set; }
    public PurchaseOrderDetailSyncDc PurchaseOrderDetail { get; set; }
}

public class PurchaseOrderDetailSyncDc
{

    public int PurchaseOrderDetailId { get; set; }
    public int? CompanyId { get; set; }
    public int PurchaseOrderId { get; set; }
    public long? PurchaseOrderNewId { get; set; }
    public int SupplierId { get; set; }
    public int? WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public string SupplierName { get; set; }
    public string SellingSku { get; set; }
    public int ItemId { get; set; }
    public string ItemNumber { get; set; }
    public string HSNCode { get; set; }
    public string SKUCode { get; set; }
    public string ItemName { get; set; }
    public string itemBaseName { get; set; }
    public double Price { get; set; }
    public double MRP { get; set; }
    public int MOQ { get; set; }
    public int TotalQuantity { get; set; }
    public string PurchaseSku { get; set; }
    public double TaxAmount { get; set; }
    public double TotalAmountIncTax { get; set; }
    public string Status { get; set; }
    public DateTime CreationDate { get; set; }
    public string CreatedBy { get; set; }
    public int ConversionFactor { get; set; }
    public string PurchaseName { get; set; }
    public double PurchaseQty { get; set; }
    public int QtyRecived { get; set; }
    public int ItemMultiMRPId { get; set; }
    public int? DepoId { get; set; }
    public string DepoName { get; set; }
    public string Barcode { get; set; }
    public bool IsCommodity { get; set; }
    public bool IsDeleted { get; set; }

}


public class TestIRSettle
{
    public double Debit { get; set; }
    public double PayingAmount { get; set; }
    public int IRMasterId { get; set; }
}

public class ElasticItems
{
    public string id { get; set; }
    public string title { get; set; }
    public string categoryname { get; set; }
    public string subcategoryname { get; set; }
    public string subsubcategoryname { get; set; }
    public string logourl { get; set; }
    public string warehouseid { get; set; }
    public string itemmultimrpid { get; set; }
    public string minorderqty { get; set; }

}
public class ETADateUpdateOrderDc
{
    public int OrderId { get; set; }
}

