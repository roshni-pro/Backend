using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrederProcessReport")]
    public class OrderProcessReportController : ApiController
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        [Route("")]
        [HttpGet]
        public OrderProcessReportDc Get(string WarehouseId)
        {
            OrderProcessReportDc dbReport = new OrderProcessReportDc();
            using (var db = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                var WarehouseIds = WarehouseId.Split(',').Select(Int32.Parse).ToList();

                if (WarehouseIds.Count > 0)
                {
                    var wIds = new DataTable();
                    wIds.Columns.Add("IntValue");
                    foreach (var item in WarehouseIds)
                    {
                        var dr = wIds.NewRow();
                        dr["IntValue"] = item;
                        wIds.Rows.Add(dr);
                    }

                    var param = new SqlParameter("param", wIds);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.IntValues";
                    #region 
                    db.Database.CommandTimeout = 300;
                    var OrderList = db.Database.SqlQuery<OpReportOrderDc>("exec [GetOPReport] @param", param).ToList();
                    #endregion

                    //Date Filter Code
                    DateTime now = DateTime.Now;
                    var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                    //end

                    //Pending 
                    #region

                    var Pending = OrderList.Where(x => x.Status == "Pending").ToList();
                    dbReport.Pending = Pending.Count;
                    dbReport.PendingOrderDetail = Pending;
                    dbReport.TotPendingAmnt = Pending.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));

                  

                    dbReport.TodayPending = Pending.Where(x => x.OrderedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalPendingAmnt = Pending.Where(x => x.OrderedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDPending = Pending.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalPendingAmnt = Pending.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);
                    #endregion

                    //Failed
                    var Failed = OrderList.Where(x => x.Status == "Failed").ToList();
                    dbReport.Failed = Failed.Count;
                    dbReport.TotFailedAmnt = Failed.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.FailedOrderDetail = Failed;
                    dbReport.TodayFailed = Failed.Where(x => x.OrderedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalFailedAmnt = Failed.Where(x => x.OrderedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDFailed = Failed.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalFailedAmnt = Failed.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    //PaymentPending
                    var PaymentPending = OrderList.Where(x => x.Status == "Payment Pending").ToList();
                    dbReport.PaymentPending = PaymentPending.Count;
                    dbReport.TotPaymentPendingAmnt = PaymentPending.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.PaymentPendingOrderDetail = PaymentPending;
                    dbReport.TodayPaymentPending = PaymentPending.Where(x => x.OrderedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalPaymentPendingAmnt = PaymentPending.Where(x => x.OrderedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDPaymentPending = PaymentPending.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalPaymentPendingAmnt = PaymentPending.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    var Issued = OrderList.Where(x => x.Status == "Issued").ToList();
                    dbReport.Issued = Issued.Count;
                    dbReport.TotIssuedAmnt = Issued.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.IssuedOrderDetail = Issued;
                    dbReport.TodayIssued = Issued.Where(x => x.UpdatedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalIssuedAmnt = Issued.Where(x => x.UpdatedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDIssued = Issued.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalIssuedAmnt = Issued.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    //Shipped
                    var Shipped = OrderList.Where(x => x.Status == "Shipped").ToList();
                    dbReport.Shipped = Shipped.Count;
                    dbReport.TotShippedAmnt = Shipped.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.ShippedOrderDetail = Shipped;
                    dbReport.TodayShipped = Shipped.Where(x => x.UpdatedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalShippedAmnt = Shipped.Where(x => x.UpdatedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDShipped = Shipped.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalShippedAmnt = Shipped.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    //Delivered
                    var Delivered = OrderList.Where(x => x.Status == "Delivered").ToList();
                    dbReport.Delivered = Delivered.Count;
                    dbReport.TotDeliveredAmnt = Delivered.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.DeliveredOrderDetail = Delivered;
                    dbReport.TodayDelivered = Delivered.Where(x => x.UpdatedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalDeliveredAmnt = Delivered.Where(x => x.UpdatedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDDelivered = Delivered.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalDeliveredAmnt = Delivered.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);


                    //sattled
                    var sattled = OrderList.Where(x => x.Status == "sattled").ToList();
                    dbReport.sattled = sattled.Count;
                    dbReport.TotsattledAmnt = sattled.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.sattledOrderDetail = sattled;
                    dbReport.Todaysattled = sattled.Where(x => x.UpdatedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalsattledAmnt = sattled.Where(x => x.UpdatedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDsattled = sattled.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalsattledAmnt = sattled.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    //ReadyToDispatch
                    var ReadytoDispatch = OrderList.Where(x => x.Status == "Ready to Dispatch").ToList();
                    dbReport.Readytodispatch = ReadytoDispatch.Count;
                    dbReport.TotReadytodispatchAmnt = ReadytoDispatch.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.ReadytodispatchOrderDetail = ReadytoDispatch;
                    dbReport.TodayReadytodispatch = ReadytoDispatch.Where(x => x.Dispatechdate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalReadytodispatchAmnt = ReadytoDispatch.Where(x => x.Dispatechdate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDReadytodispatch = ReadytoDispatch.Where(x => x.Dispatechdate >= firstDayOfMonth && x.Dispatechdate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalReadytodispatchAmnt = ReadytoDispatch.Where(x => x.Dispatechdate >= firstDayOfMonth && x.Dispatechdate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    //DeliveryRedispatch
                    var DeliveryRedispatch = OrderList.Where(x => x.Status == "Delivery Redispatch" && x.IsReAttempt ==false).ToList();
                    dbReport.DeliveredRedispatched = DeliveryRedispatch.Count;
                    dbReport.TotDeliveredRedispatchedAmnt = DeliveryRedispatch.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.DeliveredRedispatchedOrderDetail = DeliveryRedispatch;
                    dbReport.TodayDeliveredRedispatched = DeliveryRedispatch.Where(x => x.UpdatedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalDeliveredRedispatchedAmnt = DeliveryRedispatch.Where(x => x.UpdatedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDDeliveredRedispatched = DeliveryRedispatch.Where(x => x.UpdatedDate >= firstDayOfMonth && x.UpdatedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalDeliveredRedispatchedAmnt = DeliveryRedispatch.Where(x => x.UpdatedDate >= firstDayOfMonth && x.UpdatedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    // //DeliveryCanceled
                    // string OrderDeliveryCanceledQuery = "select odm.OrderId,odm.Status,odm.WarehouseName,odm.UpdatedDate,odm.GrossAmount,odm.BillDiscountAmount,odm.DiscountAmount,odm.WalletAmount,"
                    //+ "odm.deliveryCharge,odm.Deliverydate,odm.DboyName,odm.DboyMobileNo,odm.CreatedDate as Dispatechdate,odm.UpdatedDate as DeliveryCanceledDate,"
                    //+ "odm.DeliveryIssuanceIdOrderDeliveryMaster as AssignmentId,odm.OrderedDate,odm.invoice_no,odm.ReDispatchCount,Redispatchdate = (select top 1 OH.CreatedDate from OrderMasterHistories OH with(nolock) where OH.orderid = odm.OrderId and OH.Status = 'Delivery Redispatch' order by OH.CreatedDate desc )  from OrderDispatchedMasters odm with(nolock)  where odm.Status = 'Delivery Canceled' and odm.Deleted = 0 and odm.WarehouseId  IN (" + WarehouseId + ")";

                    //  var ODMdataDeliveryCancel = db.Database.SqlQuery<OpReportOrderDc>(OrderDeliveryCanceledQuery).ToList();

                    var DeliveryCanceled = OrderList.Where(x => x.Status == "Delivery Canceled").ToList();
                    dbReport.DeliveryCancelled = DeliveryCanceled.Count;
                    dbReport.TotDeliveryCancelledAmnt = DeliveryCanceled.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.DeliveryCancelledOrderDetail = DeliveryCanceled;
                    dbReport.TodayDeliveryCancelled = DeliveryCanceled.Where(x => x.UpdatedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalDeliveryCancelledAmnt = DeliveryCanceled.Where(x => x.UpdatedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDDeliveryCancelled = DeliveryCanceled.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalDeliveryCancelledAmnt = DeliveryCanceled.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    //readytopick
                    var ReadyToPick = OrderList.Where(x => x.Status == "ReadyToPick").ToList();
                    dbReport.ReadyToPick = ReadyToPick.Count;
                    dbReport.TotReadyToPick = ReadyToPick.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.ReadyToPickDetail = ReadyToPick;
                    dbReport.TodayReadyToPick = ReadyToPick.Where(x => x.UpdatedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalReadyToPick = ReadyToPick.Where(x => x.UpdatedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDReadyToPick = ReadyToPick.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalReadyToPick = ReadyToPick.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);


                    //ReAttemptCount
                    var ReAttempt = OrderList.Where(x => x.Status == "Delivery Redispatch" && x.IsReAttempt == true).ToList();
                    ReAttempt.ForEach(x =>
                         {
                             x.Status = "Reattempt";
                         }
                    );
                    dbReport.ReAttemptCount = ReAttempt.Count;
                    dbReport.TotReattemptCount = ReAttempt.Sum(a => a.GrossAmount += Math.Round((a.DiscountAmount.HasValue ? a.DiscountAmount.Value : 0) + (a.deliveryCharge.HasValue ? a.deliveryCharge.Value : 0) + (a.BillDiscountAmount.HasValue ? a.BillDiscountAmount.Value : 0) + (a.WalletAmount.HasValue ? a.WalletAmount.Value : 0), 0));
                    dbReport.ReattemptDetail = ReAttempt;
                    dbReport.TodayReattemptCount = ReAttempt.Where(x => x.UpdatedDate > DateTime.Today.Date).Count();
                    dbReport.TodayTotalReattemptCount = ReAttempt.Where(x => x.UpdatedDate > DateTime.Today.Date).Sum(a => a.GrossAmount);
                    dbReport.MTDReattemptCount = ReAttempt.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Count();
                    dbReport.MTDTotalReattemptCount = ReAttempt.Where(x => x.OrderedDate >= firstDayOfMonth && x.OrderedDate <= lastDayOfMonth).Sum(a => a.GrossAmount);

                    dbReport.ExportAll = OrderList;
                }

                return dbReport;
            }

        }

        [Route("GetOrderQueueDashboard")]
        [HttpPost]
        public List<OrderQueueReportDc> GetOrderQueue(string warehouseid,int? hours)
        {

            List<OrderQueueReportDc> result = new List<OrderQueueReportDc>();
            using (AuthContext context = new AuthContext())
            {
                var WarehouseIds = warehouseid.Split(',').Select(Int32.Parse).ToList();
                if (WarehouseIds.Count > 0)
                {
                    var roleIdDt = new DataTable();
                    roleIdDt.Columns.Add("intValue");
                    foreach (var item in WarehouseIds)
                    {
                        var dr = roleIdDt.NewRow();
                        dr["intValue"] = item;
                        roleIdDt.Rows.Add(dr);
                    }
                    var param1 = new SqlParameter("@WarehouseId", roleIdDt);
                    param1.SqlDbType = SqlDbType.Structured;
                    param1.TypeName = "dbo.intValues";
                    if (!hours.HasValue)
                        hours = 0;
                    var param2 = new SqlParameter("@Waithrs", hours.Value);
                    result = context.Database.SqlQuery<OrderQueueReportDc>("exec GetOrderQueueDashboard @WarehouseId,@Waithrs", param1, param2).ToList();
                    return result;
                }
            }
            return result;
        }

        [Route("GetOrderQueueDashboardDeatils")]
        [HttpPost]
        public List<OrderQueueReportDetailDc> GetOrderQueueDashboardDeatils(string warehouseid, string status , int hours)
        {

            List<OrderQueueReportDetailDc> result = new List<OrderQueueReportDetailDc>();
            using (AuthContext context = new AuthContext())
            {
                var WarehouseIds = warehouseid.Split(',').Select(Int32.Parse).ToList();
                if (WarehouseIds.Count > 0)
                {
                    var roleIdDt = new DataTable();
                    roleIdDt.Columns.Add("intValue");
                    foreach (var item in WarehouseIds)
                    {
                        var dr = roleIdDt.NewRow();
                        dr["intValue"] = item;
                        roleIdDt.Rows.Add(dr);
                    }
                    var param1 = new SqlParameter("@WarehouseId", roleIdDt);
                    param1.SqlDbType = SqlDbType.Structured;
                    param1.TypeName = "dbo.intValues";

                    var param2 = new SqlParameter("@Status", status ?? (object)DBNull.Value);
                    var param3 = new SqlParameter("@Waithrs", hours);


                    result = context.Database.SqlQuery<OrderQueueReportDetailDc>("exec GetOrderQueueDashboardDetail @WarehouseId, @Status, @Waithrs", param1,param2,param3).ToList();
                    return result;
                }
            }
            return result;
        }


    }

    public class OrderQueueReportDc
    {
        public int TotalAmount { get; set; }
        public string Status { get; set; }
        public int OrderCount { get; set; }
        public int waithr { get; set; }
    }

    public class OrderQueueReportDetailDc
    {
        public string WarehouseName { get; set; }
        public string OrderStatus { get; set; }
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string CustomerType { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? Deliverydate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Status { get; set; }
        public DateTime? CurrentStatusDate { get; set; }
        public int? Wait_Hr { get; set; }
        public int? ReadyToPickCount { get; set; }
    }
    public class OrderProcessReportDc
    {

        public int Failed { get; set; }
        public double TotFailedAmnt { get; set; }
        public int TodayFailed { get; set; }
        public double TodayTotalFailedAmnt { get; set; }
        public int MTDFailed { get; set; }
        public double MTDTotalFailedAmnt { get; set; }
        public List<OpReportOrderDc> FailedOrderDetail { get; set; }

        public int PaymentPending { get; set; }
        public double TotPaymentPendingAmnt { get; set; } //order value
        public int TodayPaymentPending { get; set; }
        public double TodayTotalPaymentPendingAmnt { get; set; }
        public int MTDPaymentPending { get; set; }
        public double MTDTotalPaymentPendingAmnt { get; set; }
        public List<OpReportOrderDc> PaymentPendingOrderDetail { get; set; }

        public int Pending { get; set; }
        public double TotPendingAmnt { get; set; }
        public int TodayPending { get; set; }
        public double TodayTotalPendingAmnt { get; set; }
        public int MTDPending { get; set; }
        public double MTDTotalPendingAmnt { get; set; }
        public List<OpReportOrderDc> PendingOrderDetail { get; set; }

        public int Readytodispatch { get; set; }
        public double TotReadytodispatchAmnt { get; set; }
        public int TodayReadytodispatch { get; set; }
        public double TodayTotalReadytodispatchAmnt { get; set; }
        public int MTDReadytodispatch { get; set; }
        public double MTDTotalReadytodispatchAmnt { get; set; }
        public List<OpReportOrderDc> ReadytodispatchOrderDetail { get; set; }

        public int Issued { get; set; }
        public double TotIssuedAmnt { get; set; }
        public int TodayIssued { get; set; }
        public double TodayTotalIssuedAmnt { get; set; }
        public int MTDIssued { get; set; }
        public double MTDTotalIssuedAmnt { get; set; }

        public List<OpReportOrderDc> IssuedOrderDetail { get; set; }

        public int Shipped { get; set; }
        public double TotShippedAmnt { get; set; }
        public int TodayShipped { get; set; }
        public double TodayTotalShippedAmnt { get; set; }
        public int MTDShipped { get; set; }
        public double MTDTotalShippedAmnt { get; set; }
        public List<OpReportOrderDc> ShippedOrderDetail { get; set; }

        public int DeliveryCancelled { get; set; }
        public double TotDeliveryCancelledAmnt { get; set; }
        public int TodayDeliveryCancelled { get; set; }
        public double TodayTotalDeliveryCancelledAmnt { get; set; }
        public int MTDDeliveryCancelled { get; set; }
        public double MTDTotalDeliveryCancelledAmnt { get; set; }
        public List<OpReportOrderDc> DeliveryCancelledOrderDetail { get; set; }

        public int Delivered { get; set; }
        public double TotDeliveredAmnt { get; set; }
        public int TodayDelivered { get; set; }
        public double TodayTotalDeliveredAmnt { get; set; }
        public int MTDDelivered { get; set; }
        public double MTDTotalDeliveredAmnt { get; set; }

        public int sattled { get; set; }
        public double TotsattledAmnt { get; set; }
        public int Todaysattled { get; set; }
        public double TodayTotalsattledAmnt { get; set; }
        public int MTDsattled { get; set; }
        public double MTDTotalsattledAmnt { get; set; }
        public List<OpReportOrderDc> sattledOrderDetail { get; set; }
        public List<OpReportOrderDc> DeliveredOrderDetail { get; set; }
        public int DeliveredRedispatched { get; set; }
        public double TotDeliveredRedispatchedAmnt { get; set; }
        public int TodayDeliveredRedispatched { get; set; }
        public double TodayTotalDeliveredRedispatchedAmnt { get; set; }
        public int MTDDeliveredRedispatched { get; set; }
        public double MTDTotalDeliveredRedispatchedAmnt { get; set; }
        public List<OpReportOrderDc> DeliveredRedispatchedOrderDetail { get; set; }


        public int ReadyToPick { get; set; }
        public double TotReadyToPick { get; set; }
        public int TodayReadyToPick { get; set; }
        public double TodayTotalReadyToPick { get; set; }
        public int MTDReadyToPick { get; set; }
        public double MTDTotalReadyToPick { get; set; }
        public List<OpReportOrderDc> ReadyToPickDetail { get; set; }

        public List<OpReportOrderDc> ExportAll { get; set; }


        #region reattempt count
        //reattempt count     change on 29-12-2022 vip

        public int ReAttemptCount { get; set; }
        public double TotReattemptCount { get; set; }
        public int TodayReattemptCount { get; set; }
        public double TodayTotalReattemptCount { get; set; }
        public int MTDReattemptCount { get; set; }
        public double MTDTotalReattemptCount { get; set; }
        public List<OpReportOrderDc> ReattemptDetail { get; set; }

        #endregion

        
    }

    public class OpReportOrderDc
    {
        public string Skcode { get; set; }
        public int OrderId { get; set; }
        public string WarehouseName { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Status { get; set; }
        public string DboyName { get; set; }
        public double GrossAmount { get; set; }
        public double? DiscountAmount { get; set; }
        public double? WalletAmount { get; set; }
        public double? BillDiscountAmount { get; set; }
        public int? AssignmentId { get; set; }
        public DateTime? OrderedDate { get; set; }
        public double? deliveryCharge { get; set; }
        public DateTime? Deliverydate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        public DateTime? Dispatechdate { get; set; }
        public DateTime DeliveryCanceledDate { get; set; }
        public DateTime? Redispatchdate { get; set; }
        public int? ReDispatchCount { get; set; }
        public int? ReAttemptCount { get; set; }  //changes on 21-12-2022 for new requirement
        public bool IsReAttempt { get; set; } //changes on 29-12-2022 for new requirement
        public int OrderType { get; set; }
        public bool IsKPP { get; set; }
        public string OrderTypestr
        {
            get
            {
                if (OrderType == 0 || OrderType == 1)
                    return "General"; //General
                else if (OrderType == 2)
                    return "Bundle"; //Bundle
                else if (OrderType == 3)
                    return "Return"; //Return
                else if (OrderType == 4)
                    return "Distributor"; //Distributor
                else if (OrderType == 5)
                    return "Zaruri"; //Zaruri
                else if (OrderType == 6)
                    return "Damage Order"; //Damage Order
                else if (OrderType == 7)
                    return "Franshise"; //Franshise
                else if (OrderType == 8)
                    return "Clearance Order"; //Clearance
                else
                    return "General";
            }
        }
        public string DeliveryCanceledComments { get; set; }
        public string comments { get; set; }
        public string CustomerType { get; set; }
        public string invoice_no { get; set; }
        public string StoreName { get; set; }
        public string PaymentFrom { get; set; }
    }

}


