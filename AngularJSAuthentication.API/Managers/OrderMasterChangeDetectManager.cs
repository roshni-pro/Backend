using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers;
using AngularJSAuthentication.API.Controllers.External.Gamification;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helper.EwayBill;
using AngularJSAuthentication.API.Helper.IRN;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.External;
using AngularJSAuthentication.DataContracts.Masters.Store;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.ServiceRequestParam;
using AngularJSAuthentication.DataContracts.Transaction.MessageQueue;
using AngularJSAuthentication.DataContracts.Transaction.Mongo;
using AngularJSAuthentication.Model;
using Hangfire;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers
{
    public class OrderMasterChangeDetectManager
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static readonly object LockObject = new object();
        internal static string SyncOrderCron = Convert.ToString(ConfigurationManager.AppSettings["SyncOrderCron"]);
        internal static int SyncOrderCount = Convert.ToInt32(ConfigurationManager.AppSettings["SyncOrderCount"]);
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        static DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddMinutes(1), INDIAN_ZONE);
        public static void StartDetect()
        {
            try
            {

                string cronExpression = SyncOrderCron;//"0 0/" + SyncOrderInterval + " * 1/1 * ? *";

                RecurringDateRangeJob.RemoveIfExists("SyncOrderMaster");

                RecurringDateRangeJob.AddOrUpdate("SyncOrderMaster",
                                               () => SyncOrdersInMongo(), Cron.MinuteInterval(1),
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime//DateTime.Now.AddMinutes(1)
                             );

                RecurringDateRangeJob.RemoveIfExists("PostInvoiceToClearTax");

                RecurringDateRangeJob.AddOrUpdate("PostInvoiceToClearTax",
                                               () => PostInvoiceToClearTax(), Cron.MinuteInterval(5),
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime//DateTime.Now.AddMinutes(1)
                             );
                RecurringDateRangeJob.RemoveIfExists("PostInternalTransferInvoiceToClearTax");

                RecurringDateRangeJob.AddOrUpdate("PostInternalTransferInvoiceToClearTax",
                                               () => PostInternalTransferInvoiceToClearTax(), Cron.MinuteInterval(5),
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime//DateTime.Now.AddMinutes(1)
                             );                

                RecurringDateRangeJob.RemoveIfExists("ConsumerCurrentNetStockAutoLive");

                RecurringDateRangeJob.AddOrUpdate("ConsumerCurrentNetStockAutoLive",
                                               () => ConsumerCurrentNetStockAutoLive(), Cron.MinuteInterval(30),
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime//DateTime.Now.AddMinutes(1)
                             );
                

                RecurringDateRangeJob.RemoveIfExists("GamificationUpdateCutomerRewardStatusJob");

                RecurringDateRangeJob.AddOrUpdate("GamificationUpdateCutomerRewardStatusJob",
                                               () => GamificationUpdateCutomerRewardStatusJob(), Cron.MinuteInterval(2),
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime//DateTime.Now.AddMinutes(1)
                             );

                RecurringDateRangeJob.RemoveIfExists("GeneratedCurrentMonthInOut");

                RecurringDateRangeJob.AddOrUpdate("GeneratedCurrentMonthInOut",
                                       () => GenerateCurrentMonthInOut(), Cron.Daily(05, 01),
                                       timeZone: INDIAN_ZONE,
                                       startDate: indianTime//DateTime.Now.AddMinutes(1)
                                       );

                RecurringDateRangeJob.RemoveIfExists("GenerateCustomerStoreSales");

                RecurringDateRangeJob.AddOrUpdate("GenerateCustomerStoreSales",
                                       () => GenerateCustomerStoreSales(), Cron.Daily(3, 00),
                                       timeZone: INDIAN_ZONE,
                                       startDate: indianTime
                                       );

                //RecurringDateRangeJob.RemoveIfExists("invoiceNoNotgnerated");
                //RecurringDateRangeJob.AddOrUpdate("invoiceNoNotgnerated",
                //                       () => invoiceNoNotgnerated(), Cron.HourInterval(3),
                //                       timeZone: INDIAN_ZONE,
                //                       startDate: indianTime
                //                       );



                RecurringDateRangeJob.RemoveIfExists("GenerateSellerMonthlyCharges");

                RecurringDateRangeJob.AddOrUpdate("GenerateSellerMonthlyCharges",
                                      () => GenerateSellerMonthlyCharges(), Cron.Daily(3, 30),
                                      timeZone: INDIAN_ZONE,
                                      startDate: indianTime
                                      );

                RecurringDateRangeJob.RemoveIfExists("DeliveredOrderToZaruriStock");


                RecurringDateRangeJob.AddOrUpdate("DeliveredOrderToZaruriStock",
                                               () => DeliveredOrderToZaruriStock(), Cron.Minutely,
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime//DateTime.Now.AddMinutes(1)
                             );

                RecurringDateRangeJob.RemoveIfExists("DailyFIFO");


                RecurringDateRangeJob.AddOrUpdate("DailyFIFO",
                                             () => DailyFIFO(), Cron.Daily(00, 01),
                                             timeZone: INDIAN_ZONE,
                                             startDate: indianTime//DateTime.Now.AddMinutes(1)
                                             );


                RecurringDateRangeJob.RemoveIfExists("InsertRetailerLogData");

                RecurringDateRangeJob.AddOrUpdate("InsertRetailerLogData",
                                           () => InsertRetailerLogData(), Cron.Daily(02, 00),
                                           timeZone: INDIAN_ZONE,
                                           startDate: indianTime//DateTime.Now.AddMinutes(1)
                                           );

                RecurringDateRangeJob.AddOrUpdate("InsertAbcClassification",
                                           () => InsertAbcClassification(), Cron.Daily(00, 30),
                                           timeZone: INDIAN_ZONE,
                                           startDate: indianTime//DateTime.Now.AddMinutes(1)
                                           );

                RecurringDateRangeJob.RemoveIfExists("UpdateYesterdayOrdersInMongo");

                RecurringDateRangeJob.AddOrUpdate("UpdateYesterdayOrdersInMongo",
                                          () => UpdateYesterdayOrdersInMongo(), Cron.Daily(02, 00),
                                          timeZone: INDIAN_ZONE,
                                          startDate: indianTime//DateTime.Now.AddMinutes(1)
                                          );

                RecurringDateRangeJob.RemoveIfExists("TopMarginItemNotification");

                //RecurringDateRangeJob.AddOrUpdate("TopMarginItemNotification",
                //                        () => TopMarginItemNotification(), Cron.Daily(07, 30),
                //                        timeZone: INDIAN_ZONE,
                //                        startDate: indianTime//DateTime.Now.AddMinutes(1)
                //                        );
                RecurringDateRangeJob.RemoveIfExists("ShoppingCartNotification");

                //RecurringDateRangeJob.AddOrUpdate("ShoppingCartNotification",
                //                        () => ShoppingCartNotification(), Cron.Daily(09, 00),
                //                        timeZone: INDIAN_ZONE,
                //                        startDate: indianTime//DateTime.Now.AddMinutes(1)
                //                        );

                RecurringDateRangeJob.RemoveIfExists("CustomerTargetNotification");

                RecurringDateRangeJob.AddOrUpdate("CustomerTargetNotification",
                                       () => CustomerTargetNotification(), Cron.Daily(09, 30),
                                       timeZone: INDIAN_ZONE,
                                       startDate: indianTime//DateTime.Now.AddMinutes(1)
                                       );
                RecurringDateRangeJob.RemoveIfExists("TopMarginItemNotificationafterNoon");

                //RecurringDateRangeJob.AddOrUpdate("TopMarginItemNotificationafterNoon",
                //                  () => TopMarginItemNotification(), Cron.Daily(15),
                //                  timeZone: INDIAN_ZONE,
                //                  startDate: indianTime//DateTime.Now.AddMinutes(1)
                //                  );


                RecurringDateRangeJob.RemoveIfExists("GenerateCFRReport");

                RecurringDateRangeJob.AddOrUpdate("GenerateCFRReport",
                                  () => GenerateCFRReport(), Cron.Daily(08, 00),
                                  timeZone: INDIAN_ZONE,
                                  startDate: indianTime//DateTime.Now.AddMinutes(1)
                                  );

                //RecurringDateRangeJob.RemoveIfExists("GenerateItemClassificationInActiveReport");

                //RecurringDateRangeJob.AddOrUpdate("GenerateItemClassificationInActiveReport",
                //                 () => GenerateItemClassificationInActiveReport(), Cron.Daily(08, 20),
                //                 timeZone: INDIAN_ZONE,
                //                 startDate: indianTime//DateTime.Now.AddMinutes(1)
                //                 );

                RecurringDateRangeJob.RemoveIfExists("ItemLimitlessMoqAutoInactive");

                RecurringDateRangeJob.AddOrUpdate("ItemLimitlessMoqAutoInactive",
                                () => ItemLimitlessMoqAutoInactive(), Cron.Daily(07, 20),
                                timeZone: INDIAN_ZONE,
                                startDate: indianTime//DateTime.Now.AddMinutes(1)
                                );
                RecurringDateRangeJob.RemoveIfExists("InsertItemliveDashboard");

                RecurringDateRangeJob.AddOrUpdate("InsertItemliveDashboard",
                                () => InsertItemliveDashboard(), Cron.Daily(15, 20),
                                timeZone: INDIAN_ZONE,
                                startDate: indianTime//DateTime.Now.AddMinutes(1)
                                );
                RecurringDateRangeJob.RemoveIfExists("CreateCustomerRetentionData");

                RecurringDateRangeJob.AddOrUpdate("CreateCustomerRetentionData",
                                          () => CreateCustomerRetentionData(), Cron.Daily(01, 00),
                                          timeZone: INDIAN_ZONE,
                                          startDate: indianTime
                                          );
                RecurringDateRangeJob.RemoveIfExists("RemoveGamePoint");

                RecurringDateRangeJob.AddOrUpdate("RemoveGamePoint",
                                          () => RemoveGamePoint(), Cron.MinuteInterval(15),
                                          timeZone: INDIAN_ZONE,
                                          startDate: indianTime
                                          );

                RecurringDateRangeJob.RemoveIfExists("AssignUpdateCustomerCompanyTarget");

                RecurringDateRangeJob.AddOrUpdate("AssignUpdateCustomerCompanyTarget",
                                          () => AssignUpdateCustomerCompanyTarget(), Cron.Daily(02, 00),
                                          timeZone: INDIAN_ZONE,
                                          startDate: indianTime
                                          );
                RecurringDateRangeJob.RemoveIfExists("InActiveSubCatTarget");

                RecurringDateRangeJob.AddOrUpdate("InActiveSubCatTarget",
                                         () => InActiveSubCatTarget(), Cron.Daily(03, 00),
                                         timeZone: INDIAN_ZONE,
                                         startDate: indianTime
                                         );
                RecurringDateRangeJob.RemoveIfExists("AddPrimeCustomerWalletPoint");

                RecurringDateRangeJob.AddOrUpdate("AddPrimeCustomerWalletPoint",
                                         () => AddPrimeCustomerWalletPoint(), Cron.Daily(23, 30),
                                         timeZone: INDIAN_ZONE,
                                         startDate: indianTime
                                         );

                RecurringDateRangeJob.RemoveIfExists("GamePointExpireNotification");

                //RecurringDateRangeJob.AddOrUpdate("GamePointExpireNotification",
                //                       () => GamePointExpireNotification(), Cron.Hourly(1),
                //                       timeZone: INDIAN_ZONE,
                //                       startDate: indianTime
                //                       );

                RecurringDateRangeJob.RemoveIfExists("SyncPageVisitsFromMongoToSql");

                RecurringDateRangeJob.AddOrUpdate("SyncPageVisitsFromMongoToSql",
                                         () => SyncPageVisitsFromMongoToSql(), Cron.Daily(00, 01),
                                         timeZone: INDIAN_ZONE,
                                         startDate: indianTime
                                         );

                RecurringDateRangeJob.RemoveIfExists("AutoSettleOrders");


                RecurringDateRangeJob.AddOrUpdate("AutoSettleOrders",
                                       () => AutoSettleOrders(), "*/05 * * * *",
                                       timeZone: INDIAN_ZONE,
                                       startDate: indianTime
                                       );



                RecurringDateRangeJob.RemoveIfExists("ConvertAdvancePOtoCredit");


                RecurringDateRangeJob.AddOrUpdate("ConvertAdvancePOtoCredit",
                                       () => ConvertAdvancePOtoCredit(), Cron.Daily(04, 00),
                                       timeZone: INDIAN_ZONE,
                                       startDate: indianTime
                                       );

                RecurringDateRangeJob.RemoveIfExists("ErrorCheckingAndProcessItemScheme");

                RecurringDateRangeJob.AddOrUpdate("ErrorCheckingAndProcessItemScheme",
                                            () => ErrorCheckingAndProcessItemScheme(), Cron.MinuteInterval(5),
                                            timeZone: INDIAN_ZONE,
                                            startDate: indianTime
                                             );

                RecurringDateRangeJob.RemoveIfExists("DeactiveItemScheme");


                RecurringDateRangeJob.AddOrUpdate("DeactiveItemScheme",
                                            () => DeactiveItemScheme(), Cron.MinuteInterval(90),
                                            timeZone: INDIAN_ZONE,
                                            startDate: indianTime
                                             );

                RecurringDateRangeJob.RemoveIfExists("SendLastDayOrderOtpAccessEmail");

                RecurringDateRangeJob.AddOrUpdate("SendLastDayOrderOtpAccessEmail",
                                       () => SendLastDayOrderOtpAccessEmail(), Cron.Daily(02, 30),
                                       timeZone: INDIAN_ZONE,
                                       startDate: indianTime
                                       );

                RecurringDateRangeJob.RemoveIfExists("WHLicenseExpDateAlert");

                RecurringDateRangeJob.AddOrUpdate("WHLicenseExpDateAlert",
                                         () => WHLicenseExpDateAlert(), Cron.Daily(11, 00),
                                         timeZone: INDIAN_ZONE,
                                         startDate: indianTime
                                         );


                RecurringDateRangeJob.RemoveIfExists("SentNotifyItemNotification");

                //RecurringDateRangeJob.AddOrUpdate("SentNotifyItemNotification",
                //                         () => SentNotifyItemNotification(), Cron.Hourly(1),
                //                         timeZone: INDIAN_ZONE,
                //                         startDate: indianTime
                //                         );

                RecurringDateRangeJob.RemoveIfExists("BuyerData");

                RecurringDateRangeJob.AddOrUpdate("BuyerData",
                                  () => BuyerData(), Cron.MinuteInterval(5),
                                  timeZone: INDIAN_ZONE,
                                  startDate: indianTime//DateTime.Now.AddMinutes(1)
                                  );

                RecurringDateRangeJob.RemoveIfExists("WalletPointHistory");

                RecurringDateRangeJob.AddOrUpdate("WalletPointHistory",
                                  () => WalletPointHistory(), Cron.MinuteInterval(5),
                                  timeZone: INDIAN_ZONE,
                                  startDate: indianTime//DateTime.Now.AddMinutes(1)
                                  );

                RecurringDateRangeJob.RemoveIfExists("FreebiesData");

                RecurringDateRangeJob.AddOrUpdate("FreebiesData",
                                  () => FreebiesData(), Cron.MinuteInterval(5),
                                  timeZone: INDIAN_ZONE,
                                  startDate: indianTime//DateTime.Now.AddMinutes(1)
                                  );

                RecurringDateRangeJob.RemoveIfExists("DailyDamageMovementReport");

                RecurringDateRangeJob.AddOrUpdate("DailyDamageMovementReport",
                                  () => DailyDamageMovementReport(), Cron.MinuteInterval(5),
                                  timeZone: INDIAN_ZONE,
                                  startDate: indianTime//DateTime.Now.AddMinutes(1)
                                  );


                RecurringDateRangeJob.RemoveIfExists("DeliveredOrderToSkFranchise");

                RecurringDateRangeJob.AddOrUpdate("DeliveredOrderToSkFranchise",
                                               () => DeliveredOrderToSkFranchise(), Cron.MinuteInterval(5),
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime//DateTime.Now.AddMinutes(1)
                             );

                RecurringDateRangeJob.RemoveIfExists("OnsubscribeError");

                RecurringDateRangeJob.AddOrUpdate("OnsubscribeError",
                                               () => OnsubscribeError(), Cron.HourInterval(12),
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime
                             );

            }
            catch (System.Exception ex)
            {
                logger.Error("OrderMasterChangeDetectManager.StartDetect() error --> " + ex.ToString());
            }
        }


        public static void GenerateConsent()
        {
            try
            {
                string cronExpression = Convert.ToString(ConfigurationManager.AppSettings["CustomerConsentCron"]);//"0 0/" + SyncOrderInterval + " * 1/1 * ? *";
                CustomerLedgerConsentHelper helper = new CustomerLedgerConsentHelper();

                RecurringDateRangeJob.AddOrUpdate("SyncGenerateConsent",
                                               () => helper.GenerateConsent(), cronExpression,
                                               timeZone: INDIAN_ZONE,
                                               startDate: indianTime//DateTime.Now.AddMinutes(1)
                             );

            }
            catch (System.Exception ex)
            {
                logger.Error("OrderMasterChangeDetectManager.GenerateConsent() error --> " + ex.ToString());
            }
        }

        public void UpdateOrderInMongo(OrderUpdateQueue order)
        {
            //lock (LockObject)
            //{
            try
            {
                var dbOrder = new OrderMaster();
                RetailerAppManager retailerAppManager = new RetailerAppManager();
                List<StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
                using (var context = new AuthContext())
                {
                    dbOrder = context.DbOrderMaster.Include("orderDetails").Where(x => x.OrderId == order.OrderId).FirstOrDefault();
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

                    dbOrder.SalesPerson = string.Join(",", dbOrder.orderDetails.Select(a => a.ExecutiveName).Distinct());

                    if (order.Status == "Ready to Dispatch" || order.Status == "Post Order Canceled" || order.Status == "Delivery Redispatch"
                            || order.Status == "Account settled" || order.Status == "Delivered"
                            || order.Status == "Delivery Canceled"
                            || order.Status == "Issued"
                            || order.Status == "Partial settled"
                            || order.Status == "Shipped"
                            || order.Status == "sattled"
                            )
                    {
                        var orderDispatch = context.OrderDispatchedMasters.Include("orderDetails").Where(x => x.OrderId == order.OrderId).FirstOrDefault();

                        var newdbOrder = new OrderMaster
                        {
                            OrderId = orderDispatch.OrderId,
                            CompanyId = orderDispatch.CompanyId,
                            //SalesPersonId = orderDispatch.SalesPersonId,
                            SalesPerson = string.Join(",", dbOrder.orderDetails.Select(a => a.ExecutiveName).Distinct()),
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
                            InvoiceBarcodeImage = orderDispatch.InvoiceBarcodeImage,
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
                                StoreId = dbOrder.orderDetails.FirstOrDefault(e => e.ItemId == z.ItemId).StoreId,
                                StoreName = StoreCategorySubCategoryBrands.Where(y => y.StoreId == dbOrder.orderDetails.FirstOrDefault(a => a.ItemId == z.ItemId).StoreId)?.FirstOrDefault()?.StoreName,
                                ExecutiveName = dbOrder.orderDetails.FirstOrDefault(e => e.ItemId == z.ItemId).ExecutiveName,
                                ExecutiveId = dbOrder.orderDetails.FirstOrDefault(e => e.ItemId == z.ItemId).ExecutiveId,
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
                                OrderedTotalAmt = dbOrder.orderDetails.FirstOrDefault(e => e.ItemId == z.ItemId).TotalAmt,
                                CreatedDate = dbOrder.orderDetails.FirstOrDefault(x => x.ItemMultiMRPId == z.ItemMultiMRPId && x.itemNumber == z.itemNumber).CreatedDate,
                                UpdatedDate = dbOrder.orderDetails.FirstOrDefault(x => x.ItemMultiMRPId == z.ItemMultiMRPId && x.itemNumber == z.itemNumber).UpdatedDate,
                                Deleted = z.Deleted,
                                //Status = z.Status,
                                SizePerUnit = z.SizePerUnit,
                                marginPoint = dbOrder.orderDetails.FirstOrDefault(x => x.ItemMultiMRPId == z.ItemMultiMRPId && x.itemNumber == z.itemNumber)?.marginPoint,
                                promoPoint = dbOrder.orderDetails.FirstOrDefault(x => x.ItemMultiMRPId == z.ItemMultiMRPId && x.itemNumber == z.itemNumber)?.promoPoint,
                                NetPurchasePrice = dbOrder.orderDetails.FirstOrDefault(x => x.ItemMultiMRPId == z.ItemMultiMRPId && x.itemNumber == z.itemNumber).NetPurchasePrice,
                                SupplierName = dbOrder.orderDetails.FirstOrDefault(x => x.ItemMultiMRPId == z.ItemMultiMRPId && x.itemNumber == z.itemNumber).SupplierName,
                                ItemMultiMRPId = z.ItemMultiMRPId

                            }).ToList()
                        };


                        dbOrder = newdbOrder;
                    }

                    var history = context.OrderMasterHistoriesDB.Where(x => x.orderid == order.OrderId).ToList();
                    dbOrder.OrderMasterHistories = history;
                }

                MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                var mongoOrder = mongoDbHelper.Select(x => x.OrderId == order.OrderId, collectionName: "OrderMaster").FirstOrDefault();

                // var mongoDbHelperOrder = new MongoDbHelper<OrderMaster>();
                var MOrder = Mapper.Map(dbOrder).ToANew<MongoOrderMaster>();
                if (mongoOrder != null)
                {
                    mongoDbHelper.Replace(mongoOrder.Id, MOrder, "OrderMaster");
                }
                else
                {
                    mongoDbHelper.Insert(MOrder, "OrderMaster");
                }
            }
            catch (Exception ex)
            {
                TextFileLogHelper.LogError("Error in UpdateOrderInMongo for Order Id: " + order.OrderId + Environment.NewLine + ex.ToString());
                //logger.Error("Error in UpdateOrderInMongo for Order Id: " + order.OrderId + Environment.NewLine + ex.ToString());
                //RabbitMqHelper helper = new RabbitMqHelper();
                //order.Error = ex.ToString();
                //helper.Publish(order);

            }
            //}
            logger.Info("Update in Mongo Finished ");
        }


        public static void SyncOrdersInMongo()
        {
            MongoDbHelper<OrdersToSync> mongoDbHelperOrdersSync = new MongoDbHelper<OrdersToSync>();

            var mongoOrders = mongoDbHelperOrdersSync.Select(x => !x.IsProcessed, x => x.OrderBy(z => z.CreateOrUpdateDate), 0, SyncOrderCount);

            if (mongoOrders != null && mongoOrders.Any())
            {
                var orderIds = mongoOrders.GroupBy(x => x.OrderId).Select(x => x.Key).ToList();
                var orders = PrepareOrderMasters(orderIds);

                foreach (var item in orders)
                {
                    try
                    {
                        var dbOrder = item;

                        MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                        var mongoOrder = mongoDbHelper.Select(x => x.OrderId == item.OrderId, collectionName: "OrderMaster").FirstOrDefault();
                        // var mongoDbHelperOrder = new MongoDbHelper<MongoOrderMaster>();
                        if (mongoOrder != null)
                        {
                            mongoDbHelper.Delete(mongoOrder.Id, collectionName: "OrderMaster");
                        }
                        var MOrder = Mapper.Map(dbOrder).ToANew<MongoOrderMaster>();

                        mongoDbHelper.Insert(MOrder, "OrderMaster");

                    }
                    catch (Exception ex)
                    {
                        TextFileLogHelper.LogError("Error in SyncOrdersInMongo for Order Id: " + item + Environment.NewLine + ex.ToString());

                    }

                }

                foreach (var ordersToSync in mongoOrders)
                {
                    ordersToSync.IsProcessed = true;
                    ordersToSync.ProcessedDate = DateTime.Now;
                    mongoDbHelperOrdersSync.ReplaceWithoutFind(ordersToSync.Id, ordersToSync);

                }

            }

        }

        public static void DeliveredOrderToZaruriStock()
        {
            using (var db = new AuthContext())
            {
                List<SellerOrderDelivered> ChangeOrderLists = db.SellerOrderDelivereds.Where(x => x.IsProcessed == false).ToList();
                if (ChangeOrderLists != null && ChangeOrderLists.Any())
                {
                    var tradeUrl = ConfigurationManager.AppSettings["tradeAPIurl"] + "api/Bid/AddStock";

                    TextFileLogHelper.TraceLog(tradeUrl);

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
                                ItemNumber = x.itemNumber,
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
                                    //TextFileLogHelper.TraceLog("Before posting to Trade");

                                    var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));

                                    response.EnsureSuccessStatusCode();
                                    string responseBody = response.Content.ReadAsStringAsync().Result;
                                    //TextFileLogHelper.TraceLog("Posted to trade");

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
        }



        #region  DeliveredOrderToSkFranchise    ----CreatePoDoGRN
        public static void DeliveredOrderToSkFranchise()
        {
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
                                    //TextFileLogHelper.TraceLog("Before posting to Trade");
                                    var response = AsyncContext.Run(() => client.PostAsync(tradeUrl, content));
                                    response.EnsureSuccessStatusCode();
                                    string responseBody = response.Content.ReadAsStringAsync().Result;
                                    //TextFileLogHelper.TraceLog("Posted to trade");
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

        }
        #endregion

        public static void DailyFIFO()
        {
            var manager = new ItemLedgerManager();
            if (DateTime.Now.Day == 1)
            {
                manager.WarehouseTransStartStop(1);
            }

            try
            {
                var isInsertInout = AsyncContext.Run(() => manager.InsertInOutData());
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- InsertInOutData Error", ex.ToString(), "");
            }

            //wait for inout done
            // System.Threading.Thread.Sleep(30 * 60 * 1000);

            try
            {
                WorkingCapitalManager cmContoller = new WorkingCapitalManager();
                var isWCGenerated = cmContoller.CashManagementEOD();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com;sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " --DailyFIFO Error while EOD and WC", ex.ToString(), "");

            }


            try
            {
                TextFileLogHelper.LogError("Error AccountMonthEndData Hangfire Start");
                ReportManager report = new ReportManager();
                TextFileLogHelper.LogError("Error AccountMonthEndData Hangfire ReportManager Start");
                var result = report.GetAccountMonthEndData();
                TextFileLogHelper.LogError("Error AccountMonthEndData Hangfire end");
            }
            catch (Exception ex)
            {
                string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                EmailHelper.SendMail(AppConstants.MasterEmail, "s.patil@shopkirana.com;sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " --Month report  error:", error, "");

            }

            if (DateTime.Now.Day == 1)
            {
                manager.WarehouseTransStartStop(0);
            }


            if (ConfigurationManager.AppSettings["Environment"] == "Production")
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com;sudeep.solanki@shopkirana.com;s.patil@shopkirana.com", "", "DailyFIFO has been completed", "DailyFIFO has been completed..", "");


            //#region Job For daily  DailyOrderInventoryStatus 
            //try
            //{
            //    WorkingCapitalManager wc = new WorkingCapitalManager();
            //    var IsDailyOrderInventoryStatus = wc.DailyOrderInventoryStatus();
            //}
            //catch (Exception ex)
            //{
            //    EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " --DailyOrderInventoryStatus Error", ex.ToString(), "");
            //}
            //#endregion


            //try
            //{
            //    var UpdateIrPrice = AsyncContext.Run(() => manager.UpdateIRPriceInInqueue());
            //}
            //catch (Exception ex)
            //{
            //    EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com", "", "UpdateIRPriceInInqueue Error", ex.ToString(), "");
            //}
        }

        public static void DailyOtherStockFIFO()
        {
            bool result = false;
            var manager = new ItemLedgerManager();
            List<string> OtherStocks = new List<string>();
            OtherStocks.Add("Free");
            OtherStocks.Add("Damage");
            OtherStocks.Add("NonSellable");
            OtherStocks.Add("Clearance");
            OtherStocks.Add("NonRevenue");

            ParallelLoopResult parellelResult = Parallel.ForEach(OtherStocks, (stocktype) =>
            {
                try
                {
                    var isInsertInout = AsyncContext.Run(() => manager.InsertOtherStockInOutDate(stocktype: stocktype));
                }
                catch (Exception ex)
                {
                    EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com;sudeep.solanki@shopkirana.com", "", System.Configuration.ConfigurationManager.AppSettings["Environment"] + " -- DailyOtherStockFIFO " + stocktype + " Error", ex.ToString(), "");
                }
            });

            if (parellelResult.IsCompleted)
                result = true;

            if (ConfigurationManager.AppSettings["Environment"] == "Production" && result)
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com;sudeep.solanki@shopkirana.com", "", "DailyOtherStockFIFO has been completed", "DailyOtherStockFIFO has been completed..", "");


        }

        public static void GenerateCurrentMonthInOut()
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

                    if (param.EndDate.Day == DateTime.DaysInMonth(param.EndDate.Year, param.EndDate.Month))
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
                    dt.Columns.Add("PtrPrice", typeof(double));
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
                    dt.Columns.Add("SaleOnPtr", typeof(double));
                    dt.Columns.Add("PilferageQty", typeof(int));
                    dt.Columns.Add("PilferageAmount", typeof(double));
                    dt.Columns.Add("CancelInQty", typeof(int));
                    dt.Columns.Add("CancelInAmount", typeof(double));
                    dt.Columns.Add("CancelOnPtr", typeof(double));
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
                    dt.Columns.Add("DamageInQty", typeof(int));
                    dt.Columns.Add("DamageInAmount", typeof(double));
                    dt.Columns.Add("DamageOutQty", typeof(int));
                    dt.Columns.Add("DamageOutAmount", typeof(double));

                    dt.Columns.Add("ClearanceInQty", typeof(int));
                    dt.Columns.Add("ClearanceInAmount", typeof(double));
                    dt.Columns.Add("ClearanceOutQty", typeof(int));
                    dt.Columns.Add("ClearanceOutAmount", typeof(double));

                    dt.Columns.Add("NonRevenueInQty", typeof(int));
                    dt.Columns.Add("NonRevenueInAmount", typeof(double));
                    dt.Columns.Add("NonRevenueOutQty", typeof(int));
                    dt.Columns.Add("NonRevenueOutAmount", typeof(double));

                    dt.Columns.Add("NonSellableInQty", typeof(int));
                    dt.Columns.Add("NonSellableInAmount", typeof(double));
                    dt.Columns.Add("NonSellableOutQty", typeof(int));
                    dt.Columns.Add("NonSellableOutAmount", typeof(double));

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
                    dt.Columns.Add("PtrMargin", typeof(double));
                    dt.Columns.Add("OpeningClaim", typeof(double));
                    dt.Columns.Add("ClaimsGenerated", typeof(double));
                    dt.Columns.Add("MonthClaimBackMargin", typeof(double));
                    dt.Columns.Add("ClosingClaim", typeof(double));
                    dt.Columns.Add("Difference", typeof(double));
                    dt.Columns.Add("Totalmargin", typeof(double));


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
                        dr["PtrPrice"] = x.PtrPrice ?? 0;

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
                        dr["SaleOnPtr"] = x.SaleOnPtr ?? 0;

                        dr["PilferageQty"] = x.PilferageQty ?? 0;
                        dr["PilferageAmount"] = x.PilferageAmount ?? 0;
                        dr["CancelInQty"] = x.CancelInQty ?? 0;
                        dr["CancelInAmount"] = x.CancelInAmount ?? 0;
                        dr["CancelOnPtr"] = x.CancelOnPtr ?? 0;

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
                        dr["DamageInQty"] = x.DamageInQty ?? 0;
                        dr["DamageInAmount"] = x.DamageInAmount ?? 0;
                        dr["DamageOutQty"] = x.DamageOutQty ?? 0;
                        dr["DamageOutAmount"] = x.DamageOutAmount ?? 0;
                        dr["ClearanceInQty"] = x.ClearanceInQty ?? 0;
                        dr["ClearanceInAmount"] = x.ClearanceInAmount ?? 0;
                        dr["ClearanceOutQty"] = x.ClearanceOutQty ?? 0;
                        dr["ClearanceOutAmount"] = x.ClearanceOutAmount ?? 0;
                        dr["NonRevenueInQty"] = x.NonRevenueInQty ?? 0;
                        dr["NonRevenueInAmount"] = x.NonRevenueInAmount ?? 0;
                        dr["NonRevenueOutQty"] = x.NonRevenueOutQty ?? 0;
                        dr["NonRevenueOutAmount"] = x.NonRevenueOutAmount ?? 0;

                        dr["NonSellableInQty"] = x.NonSellableInQty ?? 0;
                        dr["NonSellableInAmount"] = x.NonSellableInAmount ?? 0;
                        dr["NonSellableOutQty"] = x.NonSellableOutQty ?? 0;
                        dr["NonSellableOutAmount"] = x.NonSellableOutAmount ?? 0;

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
                        dr["PtrMargin"] = x.PtrMargin ?? 0;
                        dr["OpeningClaim"] = x.OpeningClaim ?? 0;
                        dr["ClaimsGenerated"] = x.ClaimsGenerated ?? 0;
                        dr["MonthClaimBackMargin"] = x.MonthClaimBackMargin ?? 0;
                        dr["ClosingClaim"] = x.ClosingClaim ?? 0;
                        dr["Difference"] = x.Difference ?? 0;
                        dr["Totalmargin"] = x.Totalmargin ?? 0;

                        dt.Rows.Add(dr);
                    }

                    ExcelGenerator.DataTable_To_Excel(dt, "InOut", path);
                }
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- GenerateCurrentMonthInOut Error", ex.ToString(), "");

            }
        }

        public static void GenerateCurrentMonthInOutForOtherStock()
        {
            List<string> OtherStocks = new List<string>();
            OtherStocks.Add("Free");
            OtherStocks.Add("Damage");
            OtherStocks.Add("NonSellable");
            OtherStocks.Add("Clearance");

            ParallelLoopResult parellelResult = Parallel.ForEach(OtherStocks, (stocktype) =>
            {
                //foreach (var stocktype in OtherStocks)
                //{            
                try
                {
                    var param = new BuyerDashboardParams();
                    param.StartDate = new DateTime(DateTime.Now.Date.AddDays(-1).Year, DateTime.Now.Date.AddDays(-1).Month, 1);
                    param.EndDate = DateTime.Now.Date.AddSeconds(-1);
                    param.StockType = stocktype;

                    var fileName = stocktype + "InOut_" + param.EndDate.ToString("yyyyddMM") + ".xlsx";
                    string folderPath = Path.Combine(HttpRuntime.AppDomainAppPath, "ExcelGeneratePath", "GeneratedInOut", stocktype);

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string path = Path.Combine(folderPath, fileName);
                    if (!File.Exists(path))
                    {
                        if (param.EndDate.Day == DateTime.DaysInMonth(param.EndDate.Year, param.EndDate.Month))
                        {
                            string[] files = Directory.GetFiles(folderPath, stocktype + "InOut_" + param.EndDate.ToString("yyyy") + "*" + param.EndDate.ToString("MM") + ".xlsx", SearchOption.TopDirectoryOnly);
                            if (files != null && files.Count() > 0)
                            {
                                files.ToList().ForEach(x =>
                                {
                                    File.Delete(x);
                                });
                            }
                        }

                        ItemLedgerManager manager = new ItemLedgerManager();
                        var data = AsyncContext.Run(() => manager.GetOtherStockDataFromDb(param));

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
                        dt.Columns.Add("OpeningQty", typeof(int));
                        dt.Columns.Add("OpeningAmount", typeof(double));
                        dt.Columns.Add("POInwardQty", typeof(int));
                        dt.Columns.Add("POInwardAmount", typeof(double));
                        dt.Columns.Add("SaleQty", typeof(int));
                        dt.Columns.Add("SaleAmount", typeof(double));
                        dt.Columns.Add("CancelInQty", typeof(int));
                        dt.Columns.Add("CancelInAmount", typeof(double));

                        if (stocktype == "Free")
                        {
                            dt.Columns.Add("FreeInQty", typeof(int));
                            dt.Columns.Add("FreeInAmount", typeof(double));
                            dt.Columns.Add("FreeOutQty", typeof(int));
                            dt.Columns.Add("FreeOutAmount", typeof(double));
                        }
                        else if (stocktype == "Damage")
                        {
                            dt.Columns.Add("DamageInQty", typeof(int));
                            dt.Columns.Add("DamageInAmount", typeof(double));
                            dt.Columns.Add("DamageOutQty", typeof(int));
                            dt.Columns.Add("DamageOutAmount", typeof(double));
                        }
                        else if (stocktype == "Clearance")
                        {
                            dt.Columns.Add("ClearanceInQty", typeof(int));
                            dt.Columns.Add("ClearanceInAmount", typeof(double));
                            dt.Columns.Add("ClearanceOutQty", typeof(int));
                            dt.Columns.Add("ClearanceOutAmount", typeof(double));
                        }
                        else if (stocktype == "NonSellable")
                        {
                            dt.Columns.Add("NonSellableInQty", typeof(int));
                            dt.Columns.Add("NonSellableInAmount", typeof(double));
                            dt.Columns.Add("NonSellableOutQty", typeof(int));
                            dt.Columns.Add("NonSellableOutAmount", typeof(double));
                        }

                        dt.Columns.Add("ErrorInQty", typeof(int));
                        dt.Columns.Add("ErrorInAmount", typeof(double));
                        dt.Columns.Add("ClosingQty", typeof(int));
                        dt.Columns.Add("ClosingAmount", typeof(double));
                        dt.Columns.Add("TotalInQty", typeof(int));
                        dt.Columns.Add("TotalOutQty", typeof(int));
                        dt.Columns.Add("InOutDiffQty", typeof(int));
                        dt.Columns.Add("InOutDiffAmount", typeof(double));
                        dt.Columns.Add("FrontMargin", typeof(double));
                        dt.Columns.Add("OpeningClaim", typeof(double));
                        dt.Columns.Add("ClaimsGenerated", typeof(double));
                        dt.Columns.Add("MonthClaimBackMargin", typeof(double));
                        dt.Columns.Add("ClosingClaim", typeof(double));
                        dt.Columns.Add("Difference", typeof(double));
                        dt.Columns.Add("Totalmargin", typeof(double));

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
                            dr["OpeningQty"] = x.OpeningQty ?? 0;
                            dr["OpeningAmount"] = x.OpeningAmount ?? 0;
                            dr["POInwardQty"] = x.POInwardQty ?? 0;
                            dr["POInwardAmount"] = x.POInwardAmount ?? 0;
                            dr["SaleQty"] = x.SaleQty ?? 0;
                            dr["SaleAmount"] = x.SaleAmount ?? 0;
                            dr["CancelInQty"] = x.CancelInQty ?? 0;
                            dr["CancelInAmount"] = x.CancelInAmount ?? 0;

                            if (stocktype == "Free")
                            {
                                dr["FreeInQty"] = x.FreeInQty;
                                dr["FreeInAmount"] = x.FreeInAmount ?? 0;
                                dr["FreeOutQty"] = x.FreeOutQty;
                                dr["FreeOutAmount"] = x.FreeOutAmount ?? 0;
                            }
                            else if (stocktype == "Damage")
                            {
                                dr["DamageInQty"] = x.DamageInQty ?? 0;
                                dr["DamageInAmount"] = x.DamageInAmount ?? 0;
                                dr["DamageOutQty"] = x.DamageOutQty ?? 0;
                                dr["DamageOutAmount"] = x.DamageOutAmount ?? 0;
                            }
                            else if (stocktype == "Clearance")
                            {
                                dr["ClearanceInQty"] = x.ClearanceInQty ?? 0;
                                dr["ClearanceInAmount"] = x.ClearanceInAmount ?? 0;
                                dr["ClearanceOutQty"] = x.ClearanceOutQty ?? 0;
                                dr["ClearanceOutAmount"] = x.ClearanceOutAmount ?? 0;
                            }
                            else if (stocktype == "NonSellable")
                            {
                                dr["NonSellableInQty"] = x.NonSellableInQty ?? 0;
                                dr["NonSellableInAmount"] = x.NonSellableInAmount ?? 0;
                                dr["NonSellableOutQty"] = x.NonSellableOutQty ?? 0;
                                dr["NonSellableOutAmount"] = x.NonSellableOutAmount ?? 0;
                            }


                            dr["ErrorInQty"] = x.ErrorInQty ?? 0;
                            dr["ErrorInAmount"] = x.ErrorInAmount ?? 0;
                            dr["ClosingQty"] = x.ClosingQty ?? 0;
                            dr["ClosingAmount"] = x.ClosingAmount ?? 0;
                            dr["TotalInQty"] = x.TotalInQty ?? 0;
                            dr["TotalOutQty"] = x.TotalOutQty ?? 0;
                            dr["InOutDiffQty"] = x.InOutDiffQty ?? 0;
                            dr["InOutDiffAmount"] = x.InOutDiffAmount ?? 0;
                            dr["FrontMargin"] = x.FrontMargin ?? 0;
                            dr["OpeningClaim"] = x.OpeningClaim ?? 0;
                            dr["ClaimsGenerated"] = x.ClaimsGenerated ?? 0;
                            dr["MonthClaimBackMargin"] = x.MonthClaimBackMargin ?? 0;
                            dr["ClosingClaim"] = x.ClosingClaim ?? 0;
                            dr["Difference"] = x.Difference ?? 0;
                            dr["Totalmargin"] = x.Totalmargin ?? 0;

                            dt.Rows.Add(dr);
                        }

                        ExcelGenerator.DataTable_To_Excel(dt, stocktype + "InOut", path);
                    }
                }
                catch (Exception ex)
                {
                    EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- GenerateCurrentMonthInOut Error", ex.ToString(), "");

                }
            }
            );
            if (parellelResult.IsCompleted)
            {
            }

        }


        public static void InsertRetailerLogData()
        {
            try
            {
                // MongoDbHelper<Model.CustomerShoppingCart.CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<Model.CustomerShoppingCart.CustomerShoppingCart>();
                var CollectionName = "TraceLog_" + DateTime.Now.AddDays(-1).ToString(@"MMddyyyy");
                // var cmd = new JsonCommand<BsonDocument>("{ eval: \"InsertRetailerLog('" + CollectionName + "')\" }");
                ReportManager reportManager = new ReportManager();
                AsyncContext.Run(() => reportManager.InsertRetailerTraceLog(CollectionName));
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com;sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- InsertRetailerLogData Error", ex.ToString(), "");

            }

        }
        public static void InsertTodayRetailerLogData()
        {
            try
            {
                var CollectionName = "TraceLog_" + DateTime.Now.ToString(@"MMddyyyy");
                ReportManager reportManager = new ReportManager();
                AsyncContext.Run(() => reportManager.InsertTodayRetailerTraceLog(CollectionName));
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com;sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- InsertTodayRetailerTraceLog Error", ex.ToString(), "");

            }

        }

        public static void InsertAbcClassification()
        {
            var manager = new ItemLedgerManager();
            try
            {
                var isInsertInout = AsyncContext.Run(() => manager.InsertAbcClassification());
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- InsertAbcClassification Error", ex.ToString(), "");

            }
        }

        public static void UpdateYesterdayOrdersInMongo()
        {
            List<int> orderIds = new List<int>();

            using (var authContext = new AuthContext())
            {
                var query = "select orderid from ordermasters with(nolock) where cast(createddate as date) = cast(getdate()-1 as date) or cast(updateddate as date) = cast(getdate()-1 as date)";
                orderIds = authContext.Database.SqlQuery<int>(query).ToList();
            }

            if (orderIds != null && orderIds.Any())
            {
                var orders = PrepareOrderMasters(orderIds);

                foreach (var item in orders)
                {
                    try
                    {
                        var dbOrder = item;

                        if (dbOrder != null)
                        {
                            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                            var mongoOrder = mongoDbHelper.Select(x => x.OrderId == item.OrderId, collectionName: "OrderMaster").FirstOrDefault();

                            //var mongoDbHelperOrder = new MongoDbHelper<OrderMaster>();
                            if (mongoOrder != null)
                            {
                                mongoDbHelper.Delete(mongoOrder.Id, collectionName: "OrderMaster");
                            }
                            var MOrder = Mapper.Map(dbOrder).ToANew<MongoOrderMaster>();
                            mongoDbHelper.Insert(MOrder, "OrderMaster");

                        }

                    }
                    catch (Exception ex)
                    {
                        TextFileLogHelper.LogError("Error in UpdateYesterdayOrdersInMongo for Order Id: " + item + Environment.NewLine + ex.ToString());

                    }

                }
            }


        }

        public static void UpdateLastThreemonthOrdersInMongo()
        {
            List<int> orderIds = new List<int>();

            using (var authContext = new AuthContext())
            {
                var query = "select orderid from ordermasters with(nolock) where cast(createddate as date) >= cast(DATEADD(month,-3, getdate()) as date) ";
                orderIds = authContext.Database.SqlQuery<int>(query).ToList();
            }

            if (orderIds != null && orderIds.Any())
            {
                var orders = PrepareOrderMasters(orderIds);

                foreach (var item in orders)
                {
                    try
                    {
                        var dbOrder = item;

                        if (dbOrder != null)
                        {
                            MongoDbHelper<MongoOrderMaster> mongoDbHelper = new MongoDbHelper<MongoOrderMaster>();
                            var mongoOrder = mongoDbHelper.Select(x => x.OrderId == item.OrderId, collectionName: "OrderMaster").FirstOrDefault();

                            //var mongoDbHelperOrder = new MongoDbHelper<OrderMaster>();
                            if (mongoOrder != null)
                            {
                                mongoDbHelper.Delete(mongoOrder.Id, collectionName: "OrderMaster");
                            }
                            var MOrder = Mapper.Map(dbOrder).ToANew<MongoOrderMaster>();
                            mongoDbHelper.Insert(MOrder, "OrderMaster");

                        }

                    }
                    catch (Exception ex)
                    {
                        TextFileLogHelper.LogError("Error in UpdateLastThreeMonthInMongo for Order Id: " + item + Environment.NewLine + ex.ToString());

                    }

                }
            }


        }

        private static List<OrderMaster> PrepareOrderMasters(List<int> orderIds)
        {
            List<OrderMaster> orderMasters = new List<OrderMaster>();
            RetailerAppManager retailerAppManager = new RetailerAppManager();
            List<StoreCategorySubCategoryBrand> StoreCategorySubCategoryBrands = retailerAppManager.GetStoreWithDetail();
            using (var context = new AuthContext())
            {
                foreach (var orderid in orderIds)
                {
                    List<OrderMaster> orders = context.DbOrderMaster.Include("orderDetails").Where(x => x.OrderId == orderid).ToList();
                    foreach (var dbOrder in orders)
                    {
                        try
                        {
                            var newdbOrder = new OrderMaster();

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
                                if (context.Database.Connection.State != ConnectionState.Open)
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
                                    var orderDispatch = context.OrderDispatchedMasters.Include("orderDetails").Where(x => x.OrderId == dbOrder.OrderId).FirstOrDefault();

                                    newdbOrder = new OrderMaster
                                    {
                                        OrderId = orderDispatch.OrderId,
                                        CompanyId = orderDispatch.CompanyId,
                                        //SalesPersonId = orderDispatch.SalesPersonId,
                                        SalesPerson = string.Join(",", dbOrder.orderDetails.Select(a => a.ExecutiveName).Distinct()),
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
                                        OrderDate = dbOrder.CreatedDate,
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
                                        IsPrimeCustomer = dbOrder.IsPrimeCustomer,
                                        Lat = dbOrder.Lat,
                                        Lng = dbOrder.Lng,
                                        IsDigitalOrder = dbOrder.IsDigitalOrder,
                                        orderDetails = orderDispatch.orderDetails.Select(z => new Model.OrderDetails
                                        {
                                            ExecutiveName = dbOrder.orderDetails.FirstOrDefault(e => e.ItemId == z.ItemId).ExecutiveName,
                                            ExecutiveId = dbOrder.orderDetails.FirstOrDefault(e => e.ItemId == z.ItemId).ExecutiveId,
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
                                            OrderedTotalAmt = dbOrder.orderDetails.FirstOrDefault(e => e.OrderDetailsId == z.OrderDetailsId)?.TotalAmt ?? 0,
                                            CreatedDate = dbOrder.orderDetails.FirstOrDefault(x => x.OrderDetailsId == z.OrderDetailsId).CreatedDate,
                                            UpdatedDate = dbOrder.orderDetails.FirstOrDefault(x => x.OrderDetailsId == z.OrderDetailsId).UpdatedDate,
                                            Deleted = z.Deleted,
                                            //Status = z.Status,
                                            SizePerUnit = z.SizePerUnit,
                                            marginPoint = dbOrder.orderDetails.FirstOrDefault(x => x.OrderDetailsId == z.OrderDetailsId)?.marginPoint,
                                            promoPoint = dbOrder.orderDetails.FirstOrDefault(x => x.OrderDetailsId == z.OrderDetailsId)?.promoPoint,
                                            NetPurchasePrice = dbOrder.orderDetails.FirstOrDefault(x => x.OrderDetailsId == z.OrderDetailsId).NetPurchasePrice,
                                            SupplierName = dbOrder.orderDetails.FirstOrDefault(x => x.OrderDetailsId == z.OrderDetailsId).SupplierName,
                                            ItemMultiMRPId = z.ItemMultiMRPId,
                                            ActualUnitPrice = dbOrder.orderDetails.FirstOrDefault(x => x.OrderDetailsId == z.OrderDetailsId).ActualUnitPrice,
                                            StoreId = dbOrder.orderDetails.FirstOrDefault(x => x.ItemId == z.ItemId).StoreId,
                                            StoreName = StoreCategorySubCategoryBrands.Where(y => y.StoreId == dbOrder.orderDetails.FirstOrDefault(x => x.ItemId == z.ItemId).StoreId)?.FirstOrDefault()?.StoreName
                                        }).ToList()
                                    };

                                }
                                else
                                {
                                    newdbOrder = dbOrder;
                                    newdbOrder.SalesPerson = string.Join(",", dbOrder.orderDetails.Select(a => a.ExecutiveName).Distinct());
                                    foreach (var item in newdbOrder.orderDetails)
                                    {
                                        item.StoreName = StoreCategorySubCategoryBrands.Where(y => y.StoreId == item.StoreId)?.FirstOrDefault()?.StoreName;
                                    }
                                }
                                var history = context.OrderMasterHistoriesDB.Where(x => x.orderid == dbOrder.OrderId).ToList();
                                newdbOrder.OrderMasterHistories = history;
                                orderMasters.Add(newdbOrder);

                            }
                        }
                        catch (Exception ex)
                        {
                            TextFileLogHelper.LogError("Error in PrepareOrderMasters for Order Id: " + dbOrder.OrderId + Environment.NewLine + ex.ToString());
                        }
                    }
                }
                return orderMasters;
            }
        }

        public static void ShoppingCartNotification()
        {
            AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
            try
            {
                var result = autoNotificationManager.ShoppingCartNotification();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- ShoppingCartNotification Error", ex.ToString(), "");

            }
        }
        public static void TopMarginItemNotification()
        {
            AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
            try
            {
                var result = autoNotificationManager.TopMarginItemNotification();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- TopMarginItemNotification Error", ex.ToString(), "");

            }
        }

        public static void CustomerTargetNotification()
        {
            AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
            try
            {
                var result = autoNotificationManager.CustomerTargetNotification();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com;sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- CustomerTargetNotification Error", ex.ToString(), "");

            }
        }

        public static void CreateTraceAndShardIt()
        {
            try
            {
                MongoDbHelper<Model.CustomerShoppingCart.CustomerShoppingCart> mongoDbHelper = new MongoDbHelper<Model.CustomerShoppingCart.CustomerShoppingCart>();
                //MongoDB.Driver.IMongoDatabase db = mongoDbHelper.dbClient.GetDatabase(ConfigurationManager.AppSettings["mongoDbName"]);
                var CollectionName = "TraceLog_" + DateTime.Now.AddDays(1).ToString(@"MMddyyyy");
                var createCollectionCmd = new JsonCommand<BsonDocument>("{ eval: \"db." + CollectionName + ".ensureIndex( { _id : 'hashed' } )\" }");
                var result = mongoDbHelper.mongoDatabase.RunCommand(createCollectionCmd);
                var shardCollectionCmd = new JsonCommand<BsonDocument>("{ eval: \"sh.shardCollection('SK." + CollectionName + "',{ '_id' : 'hashed' } )\" }");
                var result2 = mongoDbHelper.mongoDatabase.RunCommand(shardCollectionCmd);



                //var commandDict = new Dictionary<string, object>();
                //commandDict.Add("shardCollection", $"SK.{CollectionName}");
                //commandDict.Add("key", new Dictionary<string, object>() { { "_id", "hashed" } });
                //var bsonDocument = new MongoDB.Bson.BsonDocument(commandDict);
                //var commandDoc = new BsonDocumentCommand<MongoDB.Bson.BsonDocument>(bsonDocument);
                //var response = mongoDbHelper.mongoDatabase.RunCommand(commandDoc);


            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com;sudeep.solanki@shopkirana.com", "", "CreateTraceAndShardIt Error", ex.ToString(), "");

            }

        }

        public static void GenerateItemClassificationInActiveReport()
        {
            #region GenerateItemClassificationInActiveReportTrigger
            try
            {
                var reportManager = new Managers.ReportManager();
                var IsGenerated = AsyncContext.Run(() => reportManager.GenerateItemClassificationInActiveReport());
            }
            catch (Exception exs)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- GenerateItemClassificationInActiveReportTrigger Error", exs.ToString(), "");
            }
            #endregion
        }

        public static void GenerateCFRReport()
        {
            #region GenerateCFRReport
            try
            {
                var reportManager = new Managers.ReportManager();
                var IsCRFReportGenerated = reportManager.GenerateCFRReport();
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- GenerateCFRReport Error", exss.ToString(), "");
            }
            #endregion
        }
        public static void DailyDamageMovementReport()
        {
            #region DailyDamageMovementReport
            try
            {
                var obj = new ReportController();
                var dailyDamageMovementReport = obj.DailyDamageMovementReport();
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- DailyDamageMovementReport Error", exss.ToString(), "");
            }
            #endregion
        }
        public static void FreebiesData()
        {
            #region FreebiesData
            try
            {
                var obj = new ReportController();
                var freebiesData = obj.FreebiesData();
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- FreebiesData Error", exss.ToString(), "");
            }
            #endregion
        }
        public static void WalletPointHistory()
        {
            #region WalletPointHistory
            try
            {
                var obj = new ReportController();
                var walletPointHistory = obj.WalletPointHistory();
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- WalletPointHistory Error", exss.ToString(), "");
            }
            #endregion
        }
        public static void BuyerData()
        {
            #region BuyerData
            try
            {
                var obj = new ReportController();
                var buyerData = obj.BuyerData();
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- BuyerData Error", exss.ToString(), "");
            }
            #endregion
        }
        public static void ItemLimitlessMoqAutoInactive()
        {
            try
            {
                using (var context = new AuthContext())
                {
                    List<int> ItemIds = context.Database.SqlQuery<int>("exec ItemLimitlessMoqAutoInactive").ToList();
                    if (ItemIds != null && ItemIds.Any())
                    {
                        List<ItemMaster> itemlist = context.itemMasters.Where(x => ItemIds.Contains(x.ItemId)).ToList();
                        if (itemlist != null && itemlist.Any())
                        {
                            foreach (var item in itemlist)
                            {
                                item.active = false;
                                item.Reason = "Item Limit less then Moq Auto Inactive of MRPID : " + item.ItemMultiMRPId;
                                item.UpdatedDate = indianTime;
                                context.Entry(item).State = EntityState.Modified;
                            }
                        }
                        context.Commit();
                    }
                }
            }
            catch (Exception es)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- ItemLimitlessMoqAutoInactive Error", es.ToString(), "");

            }

        }


        public static void InsertItemliveDashboard()
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var manager = new ItemLedgerManager();
                    var isInsertInout = AsyncContext.Run(() => manager.InsertItemliveDashboard());
                }
            }
            catch (Exception es)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- InsertItemliveDashboard Error", es.ToString(), "");

            }

        }


        public static void CreateCustomerRetentionData()
        {
            #region GenerateCFRReport
            try
            {
                var reportManager = new Managers.ReportManager();
                var result = AsyncContext.Run(() => reportManager.CreateCustomerRetentionData());
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- CreateCustomerRetentionData Error", exss.ToString(), "");
            }
            #endregion
        }


        public static void RemoveGamePoint()
        {
            try
            {
                var reportManager = new Managers.ReportManager();
                var result = AsyncContext.Run(() => reportManager.RemoveGamePoint());
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com;harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- RemoveGamePoint Error", exss.ToString(), "");
            }
        }



        public static void GamePointExpireNotification()
        {
            try
            {
                AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
                var result = autoNotificationManager.GamePointExpireNotification();
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com;harry@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- GamePointExpireNotification Error", exss.ToString(), "");
            }
        }

        public static void SentNotifyItemNotification()
        {
            try
            {
                AutoNotificationManager autoNotificationManager = new AutoNotificationManager();
                var result = autoNotificationManager.SentNotifyItemNotification();
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- SentNotifyItemNotification Error", exss.ToString(), "");
            }
        }


        public static void AssignUpdateCustomerCompanyTarget()
        {
            try
            {
                var reportManager = new Managers.ReportManager();
                var result = AsyncContext.Run(() => reportManager.AssignUpdateCustomerCompanyTarget());
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com;atish.singh@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- AssignUpdateCustomerCompanyTarget Error", exss.ToString(), "");
            }
        }

        public static void InActiveSubCatTarget()
        {
            try
            {
                var reportManager = new Managers.ReportManager();
                var result = AsyncContext.Run(() => reportManager.InActiveSubCatTarget());
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com;atish.singh@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- InActiveSubCatTarget Error", exss.ToString(), "");
            }
        }

        public static void AddPrimeCustomerWalletPoint()
        {
            try
            {
                var reportManager = new Managers.ReportManager();
                var result = AsyncContext.Run(() => reportManager.AddPrimeCustomerWalletPoint());
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com;atish.singh@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- AddPrimeCustomerWalletPoint Error", exss.ToString(), "");
            }
        }

        public static void SyncPageVisitsFromMongoToSql()
        {
            var manager = new ErpPageVisitManager();
            try
            {
                var isSynced = AsyncContext.Run(() => manager.SyncPageVisitsFromMongoToSql());
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "atish.singh@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- SyncPageVisitsFromMongoToSql Error", ex.ToString(), "");
            }
        }

        public static void AutoSettleOrders()
        {
            try
            {
                AutoSettleHelper autoSettleHelper = new AutoSettleHelper();
                var result = autoSettleHelper.AutoSettleOrders();
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "amit.jain@shopkirana.com;shailesh.sharma@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- AutoSettleOrdersNotification Error", exss.ToString(), "");
            }
        }
        public static void ConvertAdvancePOtoCredit()
        {
            try
            {
                AdvancePOtoCreditConvertHelper autoSettleHelper = new AdvancePOtoCreditConvertHelper();
                AsyncContext.Run(() => autoSettleHelper.ConvertAdvancePOtoCredit());
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "shailesh.sharma@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- AutoSettleOrdersNotification Error", exss.ToString(), "");
            }
        }
        public static void PostInvoiceToClearTax()
        {
            IRNHelper iRNHelper = new IRNHelper();
            AsyncContext.Run(() => iRNHelper.PostIRNToClearTax());
        }
        public static void PostInternalTransferInvoiceToClearTax()
        {
            InternalEwayBillHelper internalEwayBillhelper = new InternalEwayBillHelper();
            AsyncContext.Run(() => internalEwayBillhelper.PostInternalTransferIRNToClearTax());
        }

        public static void GamificationUpdateCutomerRewardStatusJob()
        {
            #region UpdateCutomerRewardStatusDailyJob
            try
            {
                GamificationHelper helper = new GamificationHelper();
                AsyncContext.Run(() => helper.UpdateCutomerRewardStatusTwoMinJob());
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "abhishek.jain@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- UpdateCutomerRewardStatusDailyJob Error", exss.ToString(), "");
            }
            #endregion
        }
        public static void ErrorCheckingAndProcessItemScheme()
        {
            ItemSchemeHelper itemSchemeHelper = new ItemSchemeHelper();
            AsyncContext.Run(() => itemSchemeHelper.ErrorCheckingAndProcess());
        }

        public static void DeactiveItemScheme()
        {
            ItemSchemeHelper itemSchemeHelper = new ItemSchemeHelper();
            AsyncContext.Run(() => itemSchemeHelper.DeactiveItemScheme());
        }

        public static void SendLastDayOrderOtpAccessEmail()
        {
            ReportManager reportManager = new ReportManager();
            AsyncContext.Run(() => reportManager.SendLastDayOrderOtpAccessEmail());
        }
        public static void WHLicenseExpDateAlert()
        {
            try
            {
                WHLicenseExpAlert wHLicenseExp = new WHLicenseExpAlert();
                AsyncContext.Run(() => wHLicenseExp.WHLicenseExpDateAlert());
            }
            catch (Exception exss)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "shailesh.sharma@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- WHLicenseExpDateAlert Error", exss.ToString(), "");
            }
        }

        public static void GenerateCustomerStoreSales()
        {
            try
            {
                ReportManager reportManager = new ReportManager();
                reportManager.GenerateCustomerStoreSales();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com", "", ConfigurationManager.AppSettings["Environment"] + " -- GenerateCustomerStoreSales Error", ex.ToString(), "");

            }
        }


        public static void GenerateSellerMonthlyCharges()
        {
            try
            {
                ReportManager reportManager = new ReportManager();
                reportManager.GenerateSellerMonthlyCharges();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "sudeep.solanki@shopkirana.com", "harry@shopkirana.com", ConfigurationManager.AppSettings["Environment"] + " -- GenerateSellerMonthlyCharges Error", ex.ToString(), "");

            }
        }

        public static void invoiceNoNotgnerated()
        {
            try
            {
                ReportManager reportManager = new ReportManager();
                reportManager.invoiceNoNotgnerated();
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "s.patil@shopkirana.com", "harry@shopkirana.com", ConfigurationManager.AppSettings["Environment"] + " -- invoiceNoNotgnerated Error", ex.ToString(), "");

            }
        }

        public static void ConsumerCurrentNetStockAutoLive()
        {
            ReportManager reportManager = new ReportManager();
            try
            {
                AsyncContext.Run(() => reportManager.ConsumerCurrentZeroStockAutoInactive());
                
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "neelesh.mittal@shopkirana.com", ConfigurationManager.AppSettings["Environment"] + " -- ConsumerCurrentNetStockAutoLive Error", ex.ToString(), "");

            }
            try
            {
                AsyncContext.Run(() => reportManager.ConsumerCurrentNetStockAutoLive());
            }
            catch(Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "harry@shopkirana.com", "neelesh.mittal@shopkirana.com", ConfigurationManager.AppSettings["Environment"] + " -- ConsumerCurrentNetStockAutoLive Error", ex.ToString(), "");
            }
        }
       


        //onsubscribeError
        public static void OnsubscribeError()
        {
            try
            {
                ReportManager reportManager = new ReportManager();
                AsyncContext.Run(() => reportManager.OnSubscribeErrorSendMail());
            }
            catch (Exception ex)
            {
                EmailHelper.SendMail(AppConstants.MasterEmail, "s.patil@shopkirana.com", "harry@shopkirana.com", ConfigurationManager.AppSettings["Environment"] + " -- onSubscribeError", ex.ToString(), "");

            }
        }

    }


}