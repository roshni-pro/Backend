using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.Model;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using AngularJSAuthentication.Common.Helpers;
using Nito.AsyncEx;
using AngularJSAuthentication.Common.Constants;

namespace AngularJSAuthentication.API.Controllers.External.SalesManApp
{
    [RoutePrefix("api/SalesAppOrder")]
    public class SalesAppOrderController : ApiController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("")]
        [HttpGet]
        public List<OrderMaster> Get(int PeopleId, int Skip, int Take)
        {
            logger.Info("start OrderMaster: ");
            List<OrderMaster> ass = new List<OrderMaster>();
            using (var context = new AuthContext())
            {
                ass = context.DbOrderMaster.Where(c => (c.orderDetails.Any(z => z.ExecutiveId > 0 && z.ExecutiveId == PeopleId) || c.OrderTakenSalesPersonId == PeopleId) && c.Deleted == false).OrderByDescending(d => d.CreatedDate).Skip(Skip).Take(Take).Include("orderDetails").ToList();
                logger.Info("End OrderMaster: ");
                return ass;
            }
        }

        [Route("GetOrder")]
        [HttpGet]
        public List<SalesOrder> Get(int PeopleId, int Skip, int Take, string lang, string type = "")
        {

            List<SalesOrder> salesOrders = new List<SalesOrder>();
            List<SalesOrderDetail> SalesOrderDetails = new List<SalesOrderDetail>();


            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesOrder]";

                if (string.IsNullOrEmpty(type)) type = "ALL";

                cmd.Parameters.Add(new SqlParameter("@peopleId", PeopleId));
                cmd.Parameters.Add(new SqlParameter("@skip", Skip));
                cmd.Parameters.Add(new SqlParameter("@take", Take));
                cmd.Parameters.Add(new SqlParameter("@status", type));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                salesOrders = ((IObjectContextAdapter)context)
                               .ObjectContext
                               .Translate<SalesOrder>(reader).ToList();
                reader.NextResult();

                if (reader.HasRows)
                {
                    SalesOrderDetails = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<SalesOrderDetail>(reader).ToList();
                }

                if (lang.Trim() == "hi")
                {
                    if (salesOrders != null && salesOrders.Any() && SalesOrderDetails != null && SalesOrderDetails.Any())
                    {
                        salesOrders.ForEach(x => x.orderDetails = SalesOrderDetails.Where(
                            y => y.OrderId == x.OrderId).ToList()
                            );
                        salesOrders.ForEach(z => z.orderDetails.ForEach(y =>
                            y.itemname = y.ItemHindiName != null ? y.ItemHindiName : y.itemname
                        ));
                    }
                }
                else
                {
                    if (salesOrders != null && salesOrders.Any() && SalesOrderDetails != null && SalesOrderDetails.Any())
                    {
                        salesOrders.ForEach(x => x.orderDetails = SalesOrderDetails.Where(y => y.OrderId == x.OrderId).ToList());
                    }
                }
            }

            return salesOrders;
        }

        [Route("GetOrder/V2")]
        [HttpGet]
        public List<SalesOrder> GetOrderV2(int PeopleId, int Skip, int Take, string lang, string type = "", string keyword = null)
        {

            List<SalesOrder> salesOrders = new List<SalesOrder>();
            List<SalesOrderDetail> SalesOrderDetails = new List<SalesOrderDetail>();
            List<OrderStatusHistoryDc> OrderStatusHistoryDcs = new List<OrderStatusHistoryDc>();

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesOrderNew]";

                if (string.IsNullOrEmpty(type)) type = "ALL";

                cmd.Parameters.Add(new SqlParameter("@peopleId", PeopleId));
                cmd.Parameters.Add(new SqlParameter("@skip", Skip));
                cmd.Parameters.Add(new SqlParameter("@take", Take));
                cmd.Parameters.Add(new SqlParameter("@status", type));
                cmd.Parameters.Add(new SqlParameter("@keyword", keyword));

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                salesOrders = ((IObjectContextAdapter)context)
                               .ObjectContext
                               .Translate<SalesOrder>(reader).ToList();
                reader.NextResult();

                if (reader.HasRows)
                {
                    SalesOrderDetails = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<SalesOrderDetail>(reader).ToList();
                    reader.NextResult();
                }
                if (reader.HasRows)
                {
                    OrderStatusHistoryDcs = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<OrderStatusHistoryDc>(reader).ToList();
                }

                if (lang.Trim() == "hi")
                {
                    if (salesOrders != null && salesOrders.Any() && SalesOrderDetails != null && SalesOrderDetails.Any())
                    {
                        salesOrders.ForEach(x => x.orderDetails = SalesOrderDetails.Where(
                            y => y.OrderId == x.OrderId).ToList()
                            );
                        salesOrders.ForEach(z => z.orderDetails.ForEach(y =>
                            y.itemname = y.ItemHindiName != null ? y.ItemHindiName : y.itemname
                        ));
                    }
                    if (salesOrders != null && salesOrders.Any() && OrderStatusHistoryDcs != null && OrderStatusHistoryDcs.Any())
                    {
                        salesOrders.ForEach(x => x.OrderStatusHistoryDcs = OrderStatusHistoryDcs.Where(y => y.OrderId == x.OrderId).ToList());
                    }
                }
                else
                {
                    if (salesOrders != null && salesOrders.Any() && SalesOrderDetails != null && SalesOrderDetails.Any())
                    {
                        salesOrders.ForEach(x => x.orderDetails = SalesOrderDetails.Where(y => y.OrderId == x.OrderId).ToList());
                    }
                    if (salesOrders != null && salesOrders.Any() && OrderStatusHistoryDcs != null && OrderStatusHistoryDcs.Any())
                    {
                        salesOrders.ForEach(x => x.OrderStatusHistoryDcs = OrderStatusHistoryDcs.Where(y => y.OrderId == x.OrderId).ToList());
                    }
                }
            }

            return salesOrders;
        }

        [Route("GetOrder/V3")]
        [HttpGet]
        public GetOrderResponseDc GetOrderV3(int PeopleId, int Skip, int Take, string lang, string type = "", string keyword = null, string OrderStatus = null)
        {
            GetOrderResponseDc res = new GetOrderResponseDc();
            List<SalesOrder> salesOrders = new List<SalesOrder>();
            List<SalesOrderDetail> SalesOrderDetails = new List<SalesOrderDetail>();
            List<OrderStatusHistoryDc> OrderStatusHistoryDcs = new List<OrderStatusHistoryDc>();
            List<StatusCountDc> StatusLiveCountDcs = new List<StatusCountDc>();
            List<StatusCompletedCountDc> StatusCompletedCountDcs = new List<StatusCompletedCountDc>();


            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();
                context.Database.CommandTimeout = 600;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetSalesOrderNew]";

                if (string.IsNullOrEmpty(type)) type = "ALL";

                cmd.Parameters.Add(new SqlParameter("@peopleId", PeopleId));
                cmd.Parameters.Add(new SqlParameter("@skip", Skip));
                cmd.Parameters.Add(new SqlParameter("@take", Take));
                cmd.Parameters.Add(new SqlParameter("@status", type));
                cmd.Parameters.Add(new SqlParameter("@keyword", keyword));
                cmd.Parameters.Add(new SqlParameter("@OrderStatus", OrderStatus));

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                salesOrders = ((IObjectContextAdapter)context)
                               .ObjectContext
                               .Translate<SalesOrder>(reader).ToList();
                reader.NextResult();

                if (reader.HasRows)
                {
                    SalesOrderDetails = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<SalesOrderDetail>(reader).ToList();
                }
                reader.NextResult();
                if (reader.HasRows)
                {
                    OrderStatusHistoryDcs = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<OrderStatusHistoryDc>(reader).ToList();
                }
                reader.NextResult();
                if (reader.HasRows)
                {
                    StatusLiveCountDcs = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<StatusCountDc>(reader).ToList();
                }
                reader.NextResult();
                if (reader.HasRows)
                {
                    StatusCompletedCountDcs = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<StatusCompletedCountDc>(reader).ToList();
                }

                if (lang.Trim() == "hi")
                {
                    if (salesOrders != null && salesOrders.Any() && SalesOrderDetails != null && SalesOrderDetails.Any())
                    {
                        salesOrders.ForEach(x => x.orderDetails = SalesOrderDetails.Where(
                            y => y.OrderId == x.OrderId).ToList()
                            );
                        salesOrders.ForEach(z => z.orderDetails.ForEach(y =>
                            y.itemname = y.ItemHindiName != null ? y.ItemHindiName : y.itemname
                        ));
                    }
                    if (salesOrders != null && salesOrders.Any() && OrderStatusHistoryDcs != null && OrderStatusHistoryDcs.Any())
                    {
                        salesOrders.ForEach(x => x.OrderStatusHistoryDcs = OrderStatusHistoryDcs.Where(y => y.OrderId == x.OrderId).ToList());
                    }
                }
                else
                {
                    if (salesOrders != null && salesOrders.Any() && SalesOrderDetails != null && SalesOrderDetails.Any())
                    {
                        salesOrders.ForEach(x => x.orderDetails = SalesOrderDetails.Where(y => y.OrderId == x.OrderId).ToList());
                    }
                    if (salesOrders != null && salesOrders.Any() && OrderStatusHistoryDcs != null && OrderStatusHistoryDcs.Any())
                    {
                        salesOrders.ForEach(x => x.OrderStatusHistoryDcs = OrderStatusHistoryDcs.Where(y => y.OrderId == x.OrderId).ToList());
                    }
                }
                res.StatusCountDcs = type.ToUpper() == "LIVE" ? StatusLiveCountDcs.FirstOrDefault() : null;
                res.StatusCompletedCountDcs = type.ToUpper() == "COMPLETED" ? StatusCompletedCountDcs.FirstOrDefault() : null;
                res.salesOrders = salesOrders;
            }

            return res;
        }

        [Route("MyOrderStatusCount")]
        [HttpGet]
        public GetOrderResponseDc MyOrderStatusCount(int PeopleId)
        {
            GetOrderResponseDc res = new GetOrderResponseDc();
            List<OrderStatusHistoryDc> OrderStatusHistoryDcs = new List<OrderStatusHistoryDc>();
            List<StatusCountDc> StatusLiveCountDcs = new List<StatusCountDc>();
            List<StatusCompletedCountDc> StatusCompletedCountDcs = new List<StatusCompletedCountDc>();

            string platformIdxName = $"skorderdata_{AppConstants.Environment}";

            using (var context = new AuthContext())
            {
                var query2 = string.Format("exec IsSalesAppLead {0}", PeopleId);
                var isSalesLead = context.Database.SqlQuery<int>(query2).FirstOrDefault();
                bool Isdigitalexecutive = isSalesLead > 0;

                int warehouseid = context.Peoples.FirstOrDefault(x => x.PeopleID == PeopleId && x.Active == true && x.Deleted == false).WarehouseId;
                string query = $" select (case when a.status in ('Pending', 'Ready to Dispatch', 'ReadyToPick', 'Issued') then 1 else 0 end) PendingCount,"
                    + $"(case when a.status = 'Shipped' then 1 else 0 end) ShippedCount,"
                    + $"(case when a.status = 'Delivery Redispatch' then 1 else 0 end) RedispatchCount, "
                    + $"(case when a.status in ('Post Order Canceled', 'Delivery Canceled', 'Order Canceled') then 1 else 0 end) CanceledCount,"
                    + $"(case when a.status in ('Delivered','sattled') then 1 else 0 end) DeliveredCount "
                    + $"from {platformIdxName} a where a.whid = {warehouseid} and (executiveid = { PeopleId } or ordertakensalespersonid = { PeopleId }) and isnull(isdigitalorder,false)={Isdigitalexecutive} "
                    + $"and status in ('Pending', 'Issued', 'InTransit', 'ReadyToPick', 'Failed', 'Payment Pending', 'Payment Failed', 'Ready to Dispatch', 'Inactive', 'Delivery Redispatch', 'Shipped','sattled','Account settled','Post Order Canceled','Delivery Canceled','Partial receiving -Bounce','Delivered','Partial settled','Order Canceled','Dummy Order Cancelled') "
                    + $"group by orderid,status ";

                ElasticSqlHelper<ElasticOrderStatusCount> elasticSqlHelperData = new ElasticSqlHelper<ElasticOrderStatusCount>();
                var orderStatus = AsyncContext.Run(async () => (await elasticSqlHelperData.GetListAsync(query)).ToList());

                var status = new StatusCountDc()
                {
                    PendingCount = orderStatus.Sum(x => x.PendingCount),
                    RedispatchCount = orderStatus.Sum(x => x.RedispatchCount),
                    ShippedCount = orderStatus.Sum(x => x.ShippedCount),
                };
                var complete = new StatusCompletedCountDc()
                {
                    CanceledCount = orderStatus.Sum(x => x.CanceledCount),
                    DeliveredCount = orderStatus.Sum(x => x.DeliveredCount)
                };

                //using (var context = new AuthContext())
                //{
                //    if (context.Database.Connection.State != ConnectionState.Open)
                //        context.Database.Connection.Open();
                //    context.Database.CommandTimeout = 600;
                //    var cmd = context.Database.Connection.CreateCommand();
                //    cmd.CommandText = "[dbo].[GetMyOrderStatusCount]";

                //    cmd.Parameters.Add(new SqlParameter("@peopleId", PeopleId));
                //    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                //    // Run the sproc
                //    var reader = cmd.ExecuteReader();
                //    StatusLiveCountDcs = ((IObjectContextAdapter)context)
                //                   .ObjectContext
                //                   .Translate<StatusCountDc>(reader).ToList();
                //    reader.NextResult();

                //    if (reader.HasRows)
                //    {
                //        StatusCompletedCountDcs = ((IObjectContextAdapter)context)
                //                            .ObjectContext
                //                            .Translate<StatusCompletedCountDc>(reader).ToList();
                //    }
                //    res.StatusCompletedCountDcs = StatusCompletedCountDcs.FirstOrDefault();
                //    res.StatusCountDcs = StatusLiveCountDcs.FirstOrDefault();
                //}

                res.StatusCompletedCountDcs = complete;
                res.StatusCountDcs = status;
            }

            return res;
        }

        [Route("GetMyOrderDetailByOrderId")]
        [HttpGet]
        public MyOrderItemDc GetMyOrderDetailByOrderId(int OrderId)
        {
            MyOrderItemDc res = new MyOrderItemDc();
            List<OrderStatusHistoryDc> OrderStatusHistoryDcs = new List<OrderStatusHistoryDc>();
            List<SalesOrderDetail> SalesOrderDetail = new List<SalesOrderDetail>();

            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                context.Database.CommandTimeout = 600;
                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[GetMyOrderDetail]";

                cmd.Parameters.Add(new SqlParameter("@OrderId", OrderId));
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Run the sproc
                var reader = cmd.ExecuteReader();
                SalesOrderDetail = ((IObjectContextAdapter)context)
                               .ObjectContext
                               .Translate<SalesOrderDetail>(reader).ToList();
                reader.NextResult();

                if (reader.HasRows)
                {
                    OrderStatusHistoryDcs = ((IObjectContextAdapter)context)
                                        .ObjectContext
                                        .Translate<OrderStatusHistoryDc>(reader).ToList();
                }
                res.PTROrderAmount = SalesOrderDetail.Sum(x => x.PTROrderAmount);
                res.orderDetails = SalesOrderDetail;
                res.OrderStatusHistoryDcs = OrderStatusHistoryDcs;
            }

            return res;
        }
    }
}

public class ElasticOrderStatusCount
{
    public int PendingCount { get; set; }
    public int ShippedCount { get; set; }
    public int RedispatchCount { get; set; }
    public int CanceledCount { get; set; }
    public int DeliveredCount { get; set; }
}
public class OrderStatusCountDc
{
    public int PendingCount { get; set; }
    public int ShippedCount { get; set; }
    public int RedispatchCount { get; set; }
    public int CanceledCount { get; set; }
    public int DeliveredCount { get; set; }
}

public class SalesOrder
{
    public int OrderId { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime Deliverydate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public DateTime? ReadytoDispatchedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public double GrossAmount { get; set; }
    public double RemainingAmount { get; set; }
    public string CustomerName { get; set; }
    public string ShippingAddress { get; set; }
    public string BillingAddress { get; set; }
    public string Status { get; set; }
    public double? walletPointUsed { get; set; }
    public double? RewardPoint { get; set; }

    public int CustomerId { get; set; }
    public string ShopName { get; set; }
    public string Skcode { get; set; }
    public List<SalesOrderDetail> orderDetails { get; set; }
    public List<OrderStatusHistoryDc> OrderStatusHistoryDcs { get; set; }


    //not used
    public int CompanyId { get; set; }
    public int? SalesPersonId { get; set; }
    public string SalesPerson { get; set; }
    public string SalesMobile { get; set; }

    public string invoice_no { get; set; }
    public string Trupay { get; set; }
    public string paymentThrough { get; set; }
    public string TrupayTransactionId { get; set; }
    public string paymentMode { get; set; }
    public int CustomerCategoryId { get; set; }
    public string CustomerCategoryName { get; set; }
    public string CustomerType { get; set; }
    public string LandMark { get; set; }
    public string Customerphonenum { get; set; }
    public double TotalAmount { get; set; }
    public double DiscountAmount { get; set; }
    public double TaxAmount { get; set; }

    public double TCSAmount { get; set; }

    public double SGSTTaxAmmount { get; set; }
    public double CGSTTaxAmmount { get; set; }
    public int? CityId { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; }
    public bool active { get; set; }
    public bool Deleted { get; set; }
    public int ReDispatchCount { get; set; }
    public int DivisionId { get; set; }
    public string ReasonCancle { get; set; }
    public int ClusterId { get; set; }
    public string ClusterName { get; set; }
    public double? deliveryCharge { get; set; }
    public double? WalletAmount { get; set; }
    public double? UsedPoint { get; set; }
    public double ShortAmount { get; set; }
    public string comments { get; set; }
    public int? OrderTakenSalesPersonId { get; set; }
    public string OrderTakenSalesPerson { get; set; }
    public string Tin_No { get; set; }
    public string ShortReason { get; set; }
    public bool orderProcess { get; set; }
    public bool accountProcess { get; set; }
    public bool chequeProcess { get; set; }
    public bool epaymentProcess { get; set; }
    public double Savingamount { get; set; }
    public double OnlineServiceTax { get; set; }
    public byte[] InvoiceBarcodeImage { get; set; }
    public int OrderType { get; set; } = 1;
    public bool? IsPrimeCustomer { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public int userid { get; set; }
    public string Description { get; set; }
    public bool IsLessCurrentStock { get; set; }
    public double? BillDiscountAmount { get; set; }
    public string offertype { get; set; }
    public double RemainingTime
    { get; set; }

    public bool IsIgstInvoice { get; set; }
    public bool InactiveStatus => Status == "Inactive";
    public List<OrderMasterHistories> OrderMasterHistories { get; set; }
    public int? DeliveryIssuanceIdOrderDeliveryMaster { get; set; }
    public int? OrderDispatchedMasterId { get; set; }
    public DateTime OrderDate { get; set; }
    public double OrderAmount { get; set; }
    public double? DispatchAmount { get; set; }
    public double? DeliveredAmount { get; set; }
    public string OfferCode { get; set; }
    public string EwayBillNumber { get; set; }
    public string CreditNoteNumber { get; set; }
    public DateTime? CreditNoteDate { get; set; }
    public bool RebookOrder { get; set; }
    public bool IsETAEnable { get; set; }

}


public class SalesOrderDetail
{
    public int OrderId { get; set; }
    public int ItemId { get; set; }
    public int qty { get; set; }
    public int CompanyId { get; set; }
    public int WarehouseId { get; set; }
    public string StoreName { get; set; }
    public double UnitPrice { get; set; }
    public string itemname { get; set; }
    public string Itempic { get; set; }
    public double PTRPrice { get; set; }
    public double PTROrderAmount { get; set; }

    ///not used
    public int OrderDetailsId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; }
    public String City { get; set; }
    public string Mobile { get; set; }
    public DateTime OrderDate { get; set; }
    public int? CityId { get; set; }
    public string WarehouseName { get; set; }
    public string CategoryName { get; set; }
    public string SubcategoryName { get; set; }
    public string SubsubcategoryName { get; set; }
    public string SellingSku { get; set; }
    public int? FreeWithParentItemId { get; set; }

    public string SellingUnitName { get; set; }
    public string itemcode { get; set; }
    public string itemNumber { get; set; }
    public string HSNCode { get; set; }
    public string Barcode { get; set; }

    public double price { get; set; }
    public double Purchaseprice { get; set; }

    public int MinOrderQty { get; set; }
    public double MinOrderQtyPrice { get; set; }
    public int Noqty { get; set; }
    public double AmtWithoutTaxDisc { get; set; }
    public double AmtWithoutAfterTaxDisc { get; set; }
    public double TotalAmountAfterTaxDisc { get; set; }

    public double NetAmmount { get; set; }
    public double DiscountPercentage { get; set; }
    public double DiscountAmmount { get; set; }
    public double NetAmtAfterDis { get; set; }
    public double TaxPercentage { get; set; }
    public double TaxAmmount { get; set; }
    public double SGSTTaxPercentage { get; set; }
    public double SGSTTaxAmmount { get; set; }
    public double CGSTTaxPercentage { get; set; }
    public double CGSTTaxAmmount { get; set; }
    //for cess
    public double TotalCessPercentage { get; set; }
    public double CessTaxAmount { get; set; }


    public double TotalAmt { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool Deleted { get; set; }
    public string Status { get; set; }
    public double SizePerUnit { get; set; }
    public int? marginPoint { get; set; }
    public int? promoPoint { get; set; }
    public double NetPurchasePrice { get; set; }

    public bool IsFreeItem { get; set; }
    public bool IsDispatchedFreeStock //  Freeitem dispatched from Freestock (true)or currentstock (false)
    {
        get; set;
    }

    public string ABCClassification { get; set; }
    [NotMapped]
    public double CurrentStock { get; set; }
    [NotMapped]
    public int day { get; set; }
    [NotMapped]
    public int month { get; set; }
    [NotMapped]
    public int year { get; set; }
    [NotMapped]
    public string status { get; set; }
    public string SupplierName { get; set; }

    public int ItemMultiMRPId { get; set; }

    [NotMapped]
    public double? OrderedTotalAmt { get; set; }

    [NotMapped]
    public bool ISItemLimit { get; set; }
    [NotMapped]
    public int ItemLimitQty { get; set; }

    [NotMapped]
    public double SavingAmount { get; set; }
    [NotMapped]
    public string Category { get; set; }

    public double ActualUnitPrice { get; set; }

    public long StoreId { get; set; }
    public int ExecutiveId { get; set; }
    public string ExecutiveName { get; set; }

    public string ItemHindiName { get; set; }
}
public class OrderStatusHistoryDc
{
    public int OrderId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
}
public class StatusCountDc
{
    public int PendingCount { get; set; }
    public int ShippedCount { get; set; }
    public int RedispatchCount { get; set; }

}

public class StatusCompletedCountDc
{
    public int CanceledCount { get; set; }
    public int DeliveredCount { get; set; }
}
public class MyOrderStatusDc
{
    public StatusCountDc StatusCountdcs { get; set; }
    public StatusCompletedCountDc StatusCompletedCountdcs { get; set; }
}
public class GetOrderResponseDc
{
    public StatusCountDc StatusCountDcs { get; set; }
    public StatusCompletedCountDc StatusCompletedCountDcs { get; set; }
    public List<SalesOrder> salesOrders { get; set; }

}

public class MyOrderItemDc
{
    public double PTROrderAmount { get; set; }
    public List<SalesOrderDetail> orderDetails { get; set; }
    public List<OrderStatusHistoryDc> OrderStatusHistoryDcs { get; set; }
}


