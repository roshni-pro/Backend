using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.BusinessLayer.Managers.Transactions;
using AngularJSAuthentication.DataContracts.Transaction;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OnlineTransaction")]
    public class OnlineTransactionDashboardController : BaseAuthController
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Get Data on Filter old code
        [Route("GetData")]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(int value, int Skip, int take)
        {
            using (var db = new AuthContext())
            {
                onlineTxnPaggingDTO onlineTxnPaggingDTOList = new onlineTxnPaggingDTO();
                List<OnlineTxnDC> onlineTxnDCList = new List<OnlineTxnDC>();
                int count = 0;
                try
                {
                    var manager = new OnlineTransactionDashboardManager();
                    onlineTxnDCList = manager.GetOnlineTransactionDataList(value, Skip, take, out count);
                    onlineTxnPaggingDTOList.OnlineTxns = onlineTxnDCList;
                    onlineTxnPaggingDTOList.Total_Count = count;

                    return Request.CreateResponse(HttpStatusCode.OK, onlineTxnPaggingDTOList);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
            }
            #region old code
            //using (var db = new AuthContext())
            //{
            //    //dynamic result = null;
            //    paggingDTO addlist = new paggingDTO();
            //    List<OnlineTxn> result = new List<OnlineTxn>();
            //    int count = 0;
            //    try
            //    {
            //        if (value == 1)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.status == "Success" && a.IsOnline/*PaymentFrom != "Cash" && a.PaymentFrom != "Cheque"*/)
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.status == "Success" && a.IsOnline/*PaymentFrom != "Cash" && a.PaymentFrom != "Cheque"*/)
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();
            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 2)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.PaymentFrom == "ePayLater" && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0

            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.PaymentFrom == "ePayLater" && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();

            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 3)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.PaymentFrom == "hdfc" && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0

            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.PaymentFrom == "hdfc" && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();

            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 4)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where ((a.PaymentFrom == "truepay" || a.PaymentFrom == "trupay") && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where ((a.PaymentFrom == "truepay" || a.PaymentFrom == "trupay") && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();
            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 5)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.PaymentFrom == "mPos" && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.PaymentFrom == "mPos" && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();
            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 6)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.PaymentFrom == "credit hdfc" && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.PaymentFrom == "credit hdfc" && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();
            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 7)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.PaymentFrom == "Razorpay QR" && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.PaymentFrom == "Razorpay QR" && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();
            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 8)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.PaymentFrom == "Gullak" && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.PaymentFrom == "Gullak" && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();
            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 9)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.PaymentFrom == "Chqbook" && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.PaymentFrom == "Chqbook" && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();
            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        else if (value == 10)
            //        {
            //            result = (from a in db.PaymentResponseRetailerAppDb
            //                      where (a.PaymentFrom == "DirectUdhar" && a.status == "Success")
            //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                      from c in payment.DefaultIfEmpty()
            //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                      from d in order.DefaultIfEmpty()
            //                      select new OnlineTxn
            //                      {
            //                          OrderId = a.OrderId,
            //                          Date = a.CreatedDate,
            //                          TxnAomunt = a.amount,
            //                          TxnStatus = a.status,
            //                          TxnID = a.GatewayTransId,
            //                          SettleDate = a.SettleDate,
            //                          IsSettled = a.IsSettled,
            //                          MOP = a.PaymentFrom,
            //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
            //                          Skcode = c.Skcode,
            //                          Warehouse = c.WarehouseName,
            //                          SettlComments = a.SettleComments,
            //                          UploadId = a.UploadId,
            //                          Orderstatus = c.Status,
            //                          IsRefund = a.IsRefund,
            //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
            //                      }).OrderByDescending(x => x.Date).Skip((Skip - 1) * take).Take(take).ToList();

            //            count = (from a in db.PaymentResponseRetailerAppDb
            //                     where (a.PaymentFrom == "DirectUdhar" && a.status == "Success")
            //                     join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
            //                     from c in payment.DefaultIfEmpty()
            //                     join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
            //                     from d in order.DefaultIfEmpty()
            //                     select new OnlineTxn
            //                     {
            //                     }).Count();
            //            addlist.OnlineTxns = result;
            //            addlist.Total_Count = count;
            //        }
            //        return Request.CreateResponse(HttpStatusCode.OK, addlist);
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error("Error in Customers Report  " + ex.Message);
            //        logger.Info("End  Customers Report: ");
            //        return null;
            //    }
            //}
            #endregion
        }
        #endregion

        [Route("GetExportData")]
        [HttpGet]
        public async Task<List<OnlineTxnDC>> GetExportData(int value, DateTime? start, DateTime? end)
        {
            List<OnlineTxnDC> result = new List<OnlineTxnDC>();
            using (var db = new AuthContext())
            {
                var manager = new OnlineTransactionDashboardManager();
                result = manager.GetOnlineTransactionDataExport(value, start, end);
            }
            return result;
        }

        #region Search data on behalf of OrderId,Skcode

        [Route("MultiSearchData")]
        [HttpGet]

        public async Task<HttpResponseMessage> getAllData(int? value, int? OrderId, string SKCode, DateTime? start, DateTime? end)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<OnlineTxnDC> newdata = new List<OnlineTxnDC>();
                    var manager = new OnlineTransactionDashboardManager();
                    newdata = manager.GetMultiSearchDataList(value, OrderId, SKCode, start, end);

                    return Request.CreateResponse(HttpStatusCode.OK, newdata);

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }

            }

        }
        #endregion

        [Route("GetUPITransactionData")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<TotalTransactionsDTO> getAllUPIData(GetUPITransactionDataDC getUPITransactionDataDC)
        {
            //string OrderId, DateTime? Fromdate, DateTime? Todate, int Skip,int take, List< int > warehouses
            using (var db = new AuthContext())
            {
                TotalTransactionsDTO Data = new TotalTransactionsDTO();
                var manager = new OnlineTransactionDashboardManager();

                Data = manager.GetSearchUPITransactionList(getUPITransactionDataDC);
                return Data;
            }

        }

        [Route("ExportGetUPITransactionData")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<TotalTransactionsDTO> getAllUPIData(ExportGetUPITransactionDataDC exportGetUPITransactionDataDC)
        {
            using (var db = new AuthContext())
            {
                TotalTransactionsDTO Data = new TotalTransactionsDTO();
                var manager = new OnlineTransactionDashboardManager();

                Data = manager.ExportGetSearchUPITransactionList(exportGetUPITransactionDataDC);
                return Data;
            }

        }
        //#region Search data on behalf of OrderId,Skcode

        //[Route("MultiSearchData")]
        //[HttpGet]
        //public HttpResponseMessage getAllData(int? value, int? OrderId, string SKCode, DateTime? start, DateTime? end)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {
        //            List<OnlineTxn> newdata = new List<OnlineTxn>();
        //            if (value > 0)
        //            {
        //                if (value == 1)
        //                {
        //                    newdata = (from a in db.PaymentResponseRetailerAppDb
        //                               where (a.status == "Success" && a.PaymentFrom != "Cash" && a.PaymentFrom != "Cheque" &&
        //                               a.CreatedDate >= start && a.CreatedDate <= end)
        //                               join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                               from c in payment.DefaultIfEmpty()
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = a.OrderId,
        //                                   Date = a.CreatedDate,
        //                                   TxnAomunt = a.amount,
        //                                   TxnStatus = a.status,
        //                                   TxnID = a.GatewayTransId,
        //                                   SettleDate = a.SettleDate,
        //                                   IsSettled = a.IsSettled,
        //                                   MOP = a.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
        //                                   Skcode = c.Skcode,
        //                                   Warehouse = c.WarehouseName,
        //                                   SettlComments = a.SettleComments,
        //                                   UploadId = a.UploadId,
        //                                   Orderstatus = c.Status,
        //                                   IsRefund = a.IsRefund
        //                               }).OrderByDescending(x => x.Date).ToList();
        //                }
        //                else if (value == 2)
        //                {
        //                    newdata = (from a in db.PaymentResponseRetailerAppDb
        //                               where (a.PaymentFrom == "ePayLater" && a.status == "Success"
        //                               && a.CreatedDate >= start && a.CreatedDate <= end)
        //                               join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                               from c in payment.DefaultIfEmpty()
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = a.OrderId,
        //                                   Date = a.CreatedDate,
        //                                   TxnAomunt = a.amount,
        //                                   TxnStatus = a.status,
        //                                   TxnID = a.GatewayTransId,
        //                                   SettleDate = a.SettleDate,
        //                                   IsSettled = a.IsSettled,
        //                                   MOP = a.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
        //                                   Skcode = c.Skcode,
        //                                   Warehouse = c.WarehouseName,
        //                                   SettlComments = a.SettleComments,
        //                                   UploadId = a.UploadId,
        //                                   Orderstatus = c.Status,
        //                                   IsRefund = a.IsRefund
        //                               }).OrderByDescending(x => x.Date).ToList();
        //                }
        //                else if (value == 3)
        //                {
        //                    newdata = (from a in db.PaymentResponseRetailerAppDb
        //                               where (a.PaymentFrom == "hdfc" && a.status == "Success"
        //                               && a.CreatedDate >= start && a.CreatedDate <= end)
        //                               join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                               from c in payment.DefaultIfEmpty()
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = a.OrderId,
        //                                   Date = a.CreatedDate,
        //                                   TxnAomunt = a.amount,
        //                                   TxnStatus = a.status,
        //                                   TxnID = a.GatewayTransId,
        //                                   SettleDate = a.SettleDate,
        //                                   IsSettled = a.IsSettled,
        //                                   MOP = a.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
        //                                   Skcode = c.Skcode,
        //                                   Warehouse = c.WarehouseName,
        //                                   SettlComments = a.SettleComments,
        //                                   UploadId = a.UploadId,
        //                                   Orderstatus = c.Status,
        //                                   IsRefund = a.IsRefund
        //                               }).OrderByDescending(x => x.Date).ToList();
        //                }
        //                else if (value == 4)
        //                {
        //                    newdata = (from a in db.PaymentResponseRetailerAppDb
        //                               where ((a.PaymentFrom == "truepay" || a.PaymentFrom == "trupay") && a.status == "Success"
        //                               && a.CreatedDate >= start && a.CreatedDate <= end)
        //                               join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                               from c in payment.DefaultIfEmpty()
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = a.OrderId,
        //                                   Date = a.CreatedDate,
        //                                   TxnAomunt = a.amount,
        //                                   TxnStatus = a.status,
        //                                   TxnID = a.GatewayTransId,
        //                                   SettleDate = a.SettleDate,
        //                                   IsSettled = a.IsSettled,
        //                                   MOP = a.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
        //                                   Skcode = c.Skcode,
        //                                   Warehouse = c.WarehouseName,
        //                                   SettlComments = a.SettleComments,
        //                                   UploadId = a.UploadId,
        //                                   Orderstatus = c.Status
        //                               }).OrderByDescending(x => x.Date).ToList();
        //                }
        //                else if (value == 5)
        //                {
        //                    newdata = (from a in db.PaymentResponseRetailerAppDb
        //                               where (a.PaymentFrom == "mPos" && a.status == "Success"
        //                               && a.CreatedDate >= start && a.CreatedDate <= end)
        //                               join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                               from c in payment.DefaultIfEmpty()
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = a.OrderId,
        //                                   Date = a.CreatedDate,
        //                                   TxnAomunt = a.amount,
        //                                   TxnStatus = a.status,
        //                                   TxnID = a.GatewayTransId,
        //                                   SettleDate = a.SettleDate,
        //                                   IsSettled = a.IsSettled,
        //                                   MOP = a.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
        //                                   Skcode = c.Skcode,
        //                                   Warehouse = c.WarehouseName,
        //                                   SettlComments = a.SettleComments,
        //                                   UploadId = a.UploadId,
        //                                   Orderstatus = c.Status
        //                               }).OrderByDescending(x => x.Date).ToList();
        //                }


        //            }
        //            else
        //            {
        //                if ((OrderId != 0 && OrderId != null && SKCode != null && SKCode != "undefined" && start != null))
        //                {
        //                    newdata = (from a in db.DbOrderMaster
        //                               where (a.Skcode == SKCode && a.OrderId == OrderId)
        //                               join c in db.PaymentResponseRetailerAppDb on a.OrderId equals c.OrderId
        //                               where (c.PaymentFrom != "Cash" && c.PaymentFrom != "Cheque" && c.status == "Success" && c.CreatedDate >= start && c.CreatedDate <= end)
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = a.OrderId,
        //                                   Date = c.CreatedDate,
        //                                   TxnAomunt = c.amount,
        //                                   TxnStatus = c.status,
        //                                   TxnID = c.GatewayTransId,
        //                                   SettleDate = c.SettleDate,
        //                                   IsSettled = c.IsSettled,
        //                                   MOP = c.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : a.GrossAmount,
        //                                   Skcode = a.Skcode,
        //                                   Warehouse = a.WarehouseName,
        //                                   SettlComments = c.SettleComments,
        //                                   UploadId = c.UploadId,
        //                                   Orderstatus = a.Status,
        //                                   IsRefund = c.IsRefund

        //                               }).OrderByDescending(x => x.Date).ToList();  /*a.orderDetails,*/
        //                }
        //                else if ((OrderId != 0 && OrderId != null))
        //                {
        //                    newdata = (from a in db.PaymentResponseRetailerAppDb
        //                               where (a.OrderId == OrderId && a.status == "Success" && a.PaymentFrom != "Cash" && a.PaymentFrom != "Cheque")
        //                               join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                               from c in payment.DefaultIfEmpty()
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = a.OrderId,
        //                                   Date = a.CreatedDate,
        //                                   TxnAomunt = a.amount,
        //                                   TxnStatus = a.status,
        //                                   TxnID = a.GatewayTransId,
        //                                   SettleDate = a.SettleDate,
        //                                   IsSettled = a.IsSettled,
        //                                   MOP = a.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : c.GrossAmount,
        //                                   Skcode = c.Skcode,
        //                                   Warehouse = c.WarehouseName,
        //                                   SettlComments = a.SettleComments,
        //                                   UploadId = a.UploadId,
        //                                   Orderstatus = c.Status,
        //                                   IsRefund = a.IsRefund
        //                               }).OrderByDescending(x => x.Date).ToList();   /*a.orderDetails,*/

        //                }
        //                else if ((OrderId != 0 && start != null && OrderId != null))
        //                {
        //                    newdata = (from a in db.PaymentResponseRetailerAppDb
        //                               where (a.CreatedDate >= start && a.CreatedDate <= end && a.OrderId == OrderId
        //                               && a.status == "Success" && a.PaymentFrom != "Cash" && a.PaymentFrom != "Cheque")
        //                               join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                               from c in payment.DefaultIfEmpty()
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = a.OrderId,
        //                                   Date = a.CreatedDate,
        //                                   TxnAomunt = a.amount,
        //                                   TxnStatus = a.status,
        //                                   TxnID = a.GatewayTransId,
        //                                   SettleDate = a.SettleDate,
        //                                   IsSettled = a.IsSettled,
        //                                   MOP = a.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : c.GrossAmount,
        //                                   Skcode = c.Skcode,
        //                                   Warehouse = c.WarehouseName,
        //                                   SettlComments = a.SettleComments,
        //                                   UploadId = a.UploadId,
        //                                   Orderstatus = c.Status,
        //                                   IsRefund = a.IsRefund
        //                               }).OrderByDescending(x => x.Date).ToList();   /*a.orderDetails,*/
        //                }
        //                else if ((SKCode != null && start != null))
        //                {
        //                    newdata = (from a in db.DbOrderMaster
        //                               where (a.Skcode == SKCode)
        //                               join c in db.PaymentResponseRetailerAppDb on a.OrderId equals c.OrderId
        //                               where (c.PaymentFrom != "Cash" && c.PaymentFrom != "Cheque" && c.status == "Success"
        //                               && c.CreatedDate >= start && c.CreatedDate <= end)
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = c.OrderId,
        //                                   Date = c.CreatedDate,
        //                                   TxnAomunt = c.amount,
        //                                   TxnStatus = c.status,
        //                                   TxnID = c.GatewayTransId,
        //                                   SettleDate = c.SettleDate,
        //                                   IsSettled = c.IsSettled,
        //                                   MOP = c.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : a.GrossAmount,
        //                                   Skcode = a.Skcode,
        //                                   Warehouse = a.WarehouseName,
        //                                   SettlComments = c.SettleComments,
        //                                   UploadId = c.UploadId,
        //                                   Orderstatus = a.Status,
        //                                   IsRefund = c.IsRefund

        //                               }).OrderByDescending(x => x.Date).ToList();  /*a.orderDetails,*/
        //                }

        //                else
        //                {
        //                    newdata = (from a in db.DbOrderMaster
        //                               where (a.Skcode == SKCode)
        //                               join c in db.PaymentResponseRetailerAppDb on a.OrderId equals c.OrderId
        //                               where (c.PaymentFrom != "Cash" && c.PaymentFrom != "Cheque" &&
        //                               c.status == "Success" && a.CreatedDate >= start && a.CreatedDate <= end)
        //                               join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                               from d in order.DefaultIfEmpty()
        //                               select new OnlineTxn
        //                               {
        //                                   OrderId = c.OrderId,
        //                                   Date = c.CreatedDate,
        //                                   TxnAomunt = c.amount,
        //                                   TxnStatus = c.status,
        //                                   TxnID = c.GatewayTransId,
        //                                   SettleDate = c.SettleDate,
        //                                   IsSettled = c.IsSettled,
        //                                   MOP = c.PaymentFrom,
        //                                   OrderAmount = d != null ? d.GrossAmount : a.GrossAmount,
        //                                   Skcode = a.Skcode,
        //                                   Warehouse = a.WarehouseName,
        //                                   SettlComments = c.SettleComments,
        //                                   UploadId = c.UploadId,
        //                                   Orderstatus = a.Status,
        //                                   IsRefund = c.IsRefund
        //                               }).OrderByDescending(x => x.Date).ToList();   /*a.orderDetails,*/
        //                }
        //                if (newdata.Count == 0)
        //                {
        //                    return Request.CreateResponse(HttpStatusCode.OK, "");
        //                }


        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, newdata);

        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //        }

        //    }

        //}
        //#endregion

        #region Search data on behalf of OrderId,Skcode
        [Route("SearchData")]
        [HttpGet]
        public async Task<HttpResponseMessage> getData(string OrderId, string SKCode, DateTime? start, DateTime? end)
        {
            using (var db = new AuthContext())
            {
                try
                {
                    List<OnlineTxnDC> newdata = new List<OnlineTxnDC>();
                    var manager = new OnlineTransactionDashboardManager();
                    newdata = manager.GetPaymentReconcileSearchDataList(OrderId, SKCode, start, end);

                    if (newdata.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "");
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, newdata);

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }

        }
        #endregion

        //#region Search data on behalf of OrderId,Skcode

        //[Route("SearchData")]
        //[HttpGet]
        //public HttpResponseMessage getData(int? OrderId, string SKCode, DateTime? start, DateTime? end)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        try
        //        {

        //            List<OnlineTxn> newdata = new List<OnlineTxn>();
        //            if ((OrderId != 0 && OrderId != null && SKCode != null && SKCode != "undefined" && start != null))
        //            {
        //                newdata = (from a in db.DbOrderMaster
        //                           where (a.Skcode == SKCode && a.OrderId == OrderId)
        //                           join c in db.PaymentResponseRetailerAppDb on a.OrderId equals c.OrderId
        //                           where (c.PaymentFrom != "Cash" && c.PaymentFrom != "Cheque" && c.status == "Success" && c.CreatedDate >= start && c.CreatedDate <= end)
        //                           join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                           from d in order.DefaultIfEmpty()
        //                           select new OnlineTxn
        //                           {
        //                               OrderId = a.OrderId,
        //                               Date = c.CreatedDate,
        //                               TxnAomunt = c.amount,
        //                               TxnStatus = c.status,
        //                               TxnID = c.GatewayTransId,
        //                               SettleDate = c.SettleDate,
        //                               IsSettled = c.IsSettled,
        //                               MOP = c.PaymentFrom,
        //                               OrderAmount = d != null ? d.GrossAmount : a.GrossAmount,
        //                               Skcode = a.Skcode,
        //                               Warehouse = a.WarehouseName,
        //                               SettlComments = c.SettleComments,
        //                               UploadId = c.UploadId,
        //                               Orderstatus = a.Status,
        //                               IsRefund = c.IsRefund,
        //                               DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0

        //                           }).OrderByDescending(x => x.Date).ToList();  /*a.orderDetails,*/
        //            }
        //            else if ((OrderId != 0 && OrderId != null))
        //            {
        //                newdata = (from a in db.PaymentResponseRetailerAppDb
        //                           where (a.OrderId == OrderId && a.status == "Success" && a.PaymentFrom != "Cash" && a.PaymentFrom != "Cheque")
        //                           join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                           from c in payment.DefaultIfEmpty()
        //                           join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                           from d in order.DefaultIfEmpty()
        //                           select new OnlineTxn
        //                           {
        //                               OrderId = a.OrderId,
        //                               Date = a.CreatedDate,
        //                               TxnAomunt = a.amount,
        //                               TxnStatus = a.status,
        //                               TxnID = a.GatewayTransId,
        //                               SettleDate = a.SettleDate,
        //                               IsSettled = a.IsSettled,
        //                               MOP = a.PaymentFrom,
        //                               OrderAmount = d != null ? d.GrossAmount : c.GrossAmount,
        //                               Skcode = c.Skcode,
        //                               Warehouse = c.WarehouseName,
        //                               SettlComments = a.SettleComments,
        //                               UploadId = a.UploadId,
        //                               Orderstatus = c.Status,
        //                               IsRefund = a.IsRefund,
        //                               DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
        //                           }).OrderByDescending(x => x.Date).ToList();   /*a.orderDetails,*/

        //            }
        //            else if ((OrderId != 0 && start != null && OrderId != null))
        //            {
        //                newdata = (from a in db.PaymentResponseRetailerAppDb
        //                           where (a.CreatedDate >= start && a.CreatedDate <= end && a.OrderId == OrderId
        //                           && a.status == "Success" && a.PaymentFrom != "Cash" && a.PaymentFrom != "Cheque")
        //                           join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                           from c in payment.DefaultIfEmpty()
        //                           join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                           from d in order.DefaultIfEmpty()
        //                           select new OnlineTxn
        //                           {
        //                               OrderId = a.OrderId,
        //                               Date = a.CreatedDate,
        //                               TxnAomunt = a.amount,
        //                               TxnStatus = a.status,
        //                               TxnID = a.GatewayTransId,
        //                               SettleDate = a.SettleDate,
        //                               IsSettled = a.IsSettled,
        //                               MOP = a.PaymentFrom,
        //                               OrderAmount = d != null ? d.GrossAmount : c.GrossAmount,
        //                               Skcode = c.Skcode,
        //                               Warehouse = c.WarehouseName,
        //                               SettlComments = a.SettleComments,
        //                               UploadId = a.UploadId,
        //                               Orderstatus = c.Status,
        //                               IsRefund = a.IsRefund,
        //                               DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
        //                           }).OrderByDescending(x => x.Date).ToList();   /*a.orderDetails,*/
        //            }
        //            else if ((SKCode != null && start != null))
        //            {
        //                newdata = (from a in db.DbOrderMaster
        //                           where (a.Skcode == SKCode)
        //                           join c in db.PaymentResponseRetailerAppDb on a.OrderId equals c.OrderId
        //                           where (c.PaymentFrom != "Cash" && c.PaymentFrom != "Cheque" && c.status == "Success"
        //                           && c.CreatedDate >= start && c.CreatedDate <= end)
        //                           join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                           from d in order.DefaultIfEmpty()
        //                           select new OnlineTxn
        //                           {
        //                               OrderId = c.OrderId,
        //                               Date = c.CreatedDate,
        //                               TxnAomunt = c.amount,
        //                               TxnStatus = c.status,
        //                               TxnID = c.GatewayTransId,
        //                               SettleDate = c.SettleDate,
        //                               IsSettled = c.IsSettled,
        //                               MOP = c.PaymentFrom,
        //                               OrderAmount = d != null ? d.GrossAmount : a.GrossAmount,
        //                               Skcode = a.Skcode,
        //                               Warehouse = a.WarehouseName,
        //                               SettlComments = c.SettleComments,
        //                               UploadId = c.UploadId,
        //                               Orderstatus = a.Status,
        //                               IsRefund = c.IsRefund,
        //                               DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0

        //                           }).OrderByDescending(x => x.Date).ToList();  /*a.orderDetails,*/
        //            }

        //            else
        //            {
        //                newdata = (from a in db.DbOrderMaster
        //                           where (a.Skcode == SKCode)
        //                           join c in db.PaymentResponseRetailerAppDb on a.OrderId equals c.OrderId
        //                           where (c.PaymentFrom != "Cash" && c.PaymentFrom != "Cheque" &&
        //                           c.status == "Success" && a.CreatedDate >= start && a.CreatedDate <= end)
        //                           join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                           from d in order.DefaultIfEmpty()
        //                           select new OnlineTxn
        //                           {
        //                               OrderId = c.OrderId,
        //                               Date = c.CreatedDate,
        //                               TxnAomunt = c.amount,
        //                               TxnStatus = c.status,
        //                               TxnID = c.GatewayTransId,
        //                               SettleDate = c.SettleDate,
        //                               IsSettled = c.IsSettled,
        //                               MOP = c.PaymentFrom,
        //                               OrderAmount = d != null ? d.GrossAmount : a.GrossAmount,
        //                               Skcode = a.Skcode,
        //                               Warehouse = a.WarehouseName,
        //                               SettlComments = c.SettleComments,
        //                               UploadId = c.UploadId,
        //                               Orderstatus = a.Status,
        //                               IsRefund = c.IsRefund,
        //                               DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
        //                           }).OrderByDescending(x => x.Date).ToList();   /*a.orderDetails,*/
        //            }
        //            if (newdata.Count == 0)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, "");
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, newdata);

        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //        }
        //    }

        //}
        //#endregion

        #region
        [Route("SearchAssignmentWise")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchId(string AssignmentID)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            int CompanyId = compid;
            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
            using (var db = new AuthContext())
            {
                List<OnlineTxnDC> result = new List<OnlineTxnDC>();
                try
                {
                    var manager = new OnlineTransactionDashboardManager();
                    result = manager.GetSearchAssignmentWiseList(AssignmentID);

                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in PurchaseOrderMaster " + ex.Message);
                    logger.Info("End  PurchaseOrderMaster: ");
                    return null;
                }
            }
        }

        #endregion
        //#region
        //[Route("SearchAssignmentWise")]
        //[HttpGet]
        //public dynamic SearchId(string AssignmentID)
        //{
        //    try
        //    {
        //        var identity = User.Identity as ClaimsIdentity;
        //        int compid = 0, userid = 0;
        //        dynamic result = null;
        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
        //            compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

        //        if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
        //            userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

        //        int CompanyId = compid;
        //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
        //        int AssignmentId = Convert.ToInt32(AssignmentID);
        //        using (var db = new AuthContext())
        //        {
        //            result = (from a in db.PaymentResponseRetailerAppDb
        //                      where (a.status == "Success" && a.PaymentFrom != "Cash" && a.PaymentFrom != "Cheque")
        //                      join c in db.DbOrderMaster on a.OrderId equals c.OrderId into payment
        //                      from c in payment.DefaultIfEmpty()
        //                      join d in db.OrderDispatchedMasters on a.OrderId equals d.OrderId into order
        //                      from d in order.DefaultIfEmpty()
        //                      where d.DeliveryIssuanceIdOrderDeliveryMaster == AssignmentId
        //                      select new OnlineTxn
        //                      {
        //                          OrderId = a.OrderId,
        //                          Date = a.CreatedDate,
        //                          TxnAomunt = a.amount,
        //                          TxnStatus = a.status,
        //                          TxnID = a.GatewayTransId,
        //                          SettleDate = a.SettleDate,
        //                          IsSettled = a.IsSettled,
        //                          MOP = a.PaymentFrom,
        //                          OrderAmount = d != null ? d.GrossAmount : c != null ? c.GrossAmount : 0,
        //                          Skcode = c.Skcode,
        //                          Warehouse = c.WarehouseName,
        //                          SettlComments = a.SettleComments,
        //                          UploadId = a.UploadId,
        //                          Orderstatus = c.Status,
        //                          IsRefund = a.IsRefund,
        //                          DeliveryIssuanceId = d != null ? d.DeliveryIssuanceIdOrderDeliveryMaster : 0
        //                      }).OrderByDescending(x => x.Date).ToList();
        //        }
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error in PurchaseOrderMaster " + ex.Message);
        //        logger.Info("End  PurchaseOrderMaster: ");
        //        return null;
        //    }
        //}

        //#endregion


        #region Update Isreconcile Status
        [Route("StatusData")]
        [AcceptVerbs("PUT")]
        public PaymentResponseRetailerApp Actsupp(PaymentResponseRetailerApp pay)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                userid = identity.Claims.Any(x => x.Type == "userid") ? int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value) : 0;

                pay.userid = userid;

                using (var db = new AuthContext())
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();


                    PaymentResponseRetailerApp act = db.PaymentResponseRetailerAppDb.Where(x => x.OrderId == pay.OrderId && x.GatewayTransId == pay.GatewayTransId && x.amount > 0).FirstOrDefault();
                    if (act != null)
                    {
                        act.IsSettled = pay.IsSettled;
                        act.SettleComments = pay.SettleComments;
                        //act.SettledBy = pay.SettledBy;
                        if (string.IsNullOrEmpty(peopledata.DisplayName))
                        {
                            act.SettledBy = peopledata.PeopleFirstName;

                        }
                        else
                        {
                            act.SettledBy = peopledata.DisplayName;
                        }
                        if (pay.IsSettled == true)
                        {
                            act.SettleDate = indianTime;
                        }
                        act.UpdatedDate = indianTime;
                        db.Entry(act).State = EntityState.Modified;
                        db.Commit();

                    }

                    return pay;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteSupplier " + ex.Message);
                return null;
            }
        }

        #endregion

        #region
        [Route("EnterComment")]
        [AcceptVerbs("PUT")]
        public PaymentResponseRetailerApp epaycommnt(PaymentResponseRetailerApp epay)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                userid = identity.Claims.Any(x => x.Type == "userid") ? int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value) : 0;

                epay.userid = userid;

                using (var db = new AuthContext())
                {
                    var peopledata = db.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();


                    PaymentResponseRetailerApp act = db.PaymentResponseRetailerAppDb.Where(x => x.OrderId == epay.OrderId && x.PaymentFrom == epay.PaymentFrom && x.amount > 0).FirstOrDefault();
                    if (act != null)
                    {

                        act.SettleComments = epay.SettleComments;
                        //act.SettledBy = pay.SettledBy;
                        if (string.IsNullOrEmpty(peopledata.DisplayName))
                        {
                            act.SettledBy = peopledata.PeopleFirstName;

                        }
                        else
                        {
                            act.SettledBy = peopledata.DisplayName;
                        }
                        act.UpdatedDate = indianTime;
                        db.Entry(act).State = EntityState.Modified;
                        db.Commit();

                    }

                    return epay;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error in deleteSupplier " + ex.Message);
                return null;
            }
        }

        #endregion

        #region Show Upload File on Filter
        [Route("OnchangeStatus")]
        [HttpGet]
        public dynamic Getvalue(int value, bool stvalue)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (value == 1)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "ePayLater" && x.IsReconcile == stvalue).OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 2)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC" && x.IsReconcile == stvalue).OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 3)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "Mpos" && x.IsReconcile == stvalue).OrderByDescending(x => x.UploadDate).ToList();
                    }
                    //else if (value == 4)
                    //{
                    //    result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_UPI" && x.IsReconcile == stvalue).OrderByDescending(x=>x.UploadDate).ToList();
                    //}
                    else if (value == 4)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_NetBanking" && x.IsReconcile == stvalue).OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 6)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "credit hdfc" && x.IsReconcile == stvalue).OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 7)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "Razorpay QR" && x.IsReconcile == stvalue).OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 9)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "Razorpay QR" && x.IsReconcile == stvalue).OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 8)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "UPI" && x.IsReconcile == stvalue).OrderByDescending(x => x.UploadDate).ToList();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion

        #region
        #region Show  Latest Epay Filter
        [Route("OnchangeEpay")]
        [HttpGet]
        public dynamic Getstepay(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.EpaylaterUploadDB
                              where (a.UplaodId == UploadId)
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()

                              select new EpayUpload
                              {
                                  TXNID = a.transaction_id,
                                  TXNDate = a.transaction_date,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.Amount,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.TXNDate).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion

        #region Show  Latest hdfc Filter
        [Route("OnchangeUPI")]
        [HttpGet]
        public dynamic GetDetailOnchangeUPI(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.UPIUploadDB
                              where (a.UploadId == UploadId)
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()
                              select new EpayUpload
                              {
                                  TXNID = a.transaction_id,
                                  TXNDate = a.transaction_date,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.Amount,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.TXNDate).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion

        #region Show  Latest hdfc Filter
        [Route("Onchangehdfc")]
        [HttpGet]
        public dynamic Getsthdfc(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.HDFCUploadDB
                              where (a.UploadId == UploadId)
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()
                              select new EpayUpload
                              {
                                  TXNID = a.transaction_id,
                                  TXNDate = a.transaction_date,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.Amount,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.TXNDate).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion
        #region Show  Latest HDFC-Credit Filter
        [Route("OnchangehdfcCredit")]
        [HttpGet]
        public dynamic OnchangehdfcCredit(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.HDFCCreditUploadDB
                              where (a.UploadId == UploadId)
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.CCAvenueRef equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()
                              select new EpayUpload
                              {
                                  TXNID = a.CCAvenueRef,
                                  TXNDate = a.Date,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.Amount,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.TXNDate).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion
        #region Show  Latest RazorpayQR Filter
        [Route("OnchangeRazorpayQR")]
        [HttpGet]
        public dynamic OnchangeRazorpayQR(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.RazorpayQRUploadDB
                              where (a.UploadId == UploadId)
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.GatewayTransId equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()
                              select new EpayUpload
                              {
                                  TXNID = a.GatewayTransId,
                                  TXNDate = a.Date,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.Amount,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.TXNDate).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion
        #region Show  Latest Epay Filter
        [Route("Onchangempos")]
        [HttpGet]
        public dynamic Getstmpos(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.MposUploadDB
                              where (a.UploadId == UploadId)
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()
                              select new EpayUpload
                              {
                                  TXNID = a.transaction_id,
                                  TXNDate = a.transaction_date,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.Amount,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.TXNDate).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion
        #region Show  Latest HDFC UPI Filter
        //[Route("OnchangeHDFCUPI")]
        //[HttpGet]
        //public dynamic GethdfcUPI(int UploadId, bool status)
        //{
        //    dynamic result = null;
        //    logger.Info("start Get Report2: ");
        //    using (AuthContext db = new AuthContext())
        //    {
        //        try
        //        {
        //            var identity = User.Identity as ClaimsIdentity;
        //            int compid = 0, userid = 0;

        //            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

        //            result = (from a in db.HDFCUPIUploadDB
        //                      where (a.UploadId == UploadId)
        //                      join c in db.PaymentResponseRetailerAppDb on a.transaction_id equals c.GatewayTransId into payment
        //                      from c in payment.DefaultIfEmpty()
        //                      where (c.IsSettled == status)
        //                      join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
        //                      from p in order.DefaultIfEmpty()
        //                      select new EpayUpload
        //                      {
        //                          TXNID = a.transaction_id,
        //                          TXNDate = a.transaction_date,
        //                          paymentxnId = c != null ? c.GatewayTransId : null,
        //                          TxnAomunt = c != null ? c.amount : 0,
        //                          OrderId = p != null ? p.OrderId : 0,
        //                          OrderAmount = p != null ? p.GrossAmount : 0,
        //                          UploadAmount = a.Amount,
        //                          Status = c != null ? c.IsSettled : false,
        //                          Skcode = p != null ? p.Skcode : "",
        //                          Comment = p != null ? c.SettleComments : ""
        //                      }).OrderByDescending(x => x.TXNDate).ToList();

        //            return result;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in Customers Report " + ex.Message);
        //            logger.Info("End Customers Report: ");
        //            return null;
        //        }
        //    }
        //}
        #endregion

        #region Show  Latest HDFC Net Banking Filter
        [Route("OnchangeHDFCNet")]
        [HttpGet]
        public dynamic GethdfNetbanking(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.HDFCNetBankingUploadDB
                              where (a.UploadId == UploadId && a.transaction_id != "")
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()
                              select new EpayUpload
                              {
                                  TXNID = a.transaction_id,
                                  TXNDate = a.transaction_date,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.Amount,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.TXNDate).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion

        #endregion
        #region
        [Route("Epaystatuschange")]
        [AcceptVerbs("PUT")]
        public OnlineTxn Put(OnlineTxn item)
        {
            using (var ac = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int userid = 0;

                    userid = identity.Claims.Any(x => x.Type == "userid") ? int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value) : 0;

                    item.userid = userid;


                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", userid);
                    try
                    {
                        var peopledata = ac.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                        PaymentResponseRetailerApp pd = ac.PaymentResponseRetailerAppDb.Where(x => x.GatewayTransId == item.TxnID && x.amount > 0).SingleOrDefault();
                        pd.IsSettled = item.IsSettled;
                        pd.SettleComments = item.SettlComments;

                        if (string.IsNullOrEmpty(peopledata.DisplayName))
                        {
                            pd.SettledBy = peopledata.PeopleFirstName;

                        }
                        else
                        {
                            pd.SettledBy = peopledata.DisplayName;
                        }
                        if (item.IsSettled == true)
                        {
                            item.SettleDate = indianTime;
                        }
                        pd.UpdatedDate = indianTime;
                        ac.Entry(pd).State = EntityState.Modified;
                        ac.Commit();

                    }
                    catch (Exception ex) { }
                    try
                    {
                        EpaylaterUpload Ps = ac.EpaylaterUploadDB.Where(x => x.transaction_id == item.TxnID).SingleOrDefault();
                        if (Ps != null)
                        {
                            if (item.IsSettled == true)
                            {
                                Ps.IsReconcile = true;
                                ac.Commit();
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                    try
                    {
                        UploadFileReconcile PC = ac.UploadFileReconcileDB.Where(x => x.UploadId == item.userid).SingleOrDefault();
                        if (PC != null)
                        {
                            if (item.IsSettled == true)
                            {
                                PC.IsReconcile = true;
                                ac.Commit();
                            }
                        }


                    }
                    catch (Exception ex)
                    { }


                    return item; // context.PutPeoplebyAdmin(item);
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Add Peoples " + ex.Message);

                    return null;
                }
        }



        #endregion
        #region Show Upload File on Filter
        [Route("UploadFile")]
        [HttpGet]
        public dynamic Getvalue(int value, bool? stvalue, DateTime? start, DateTime? end)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (value != 0 && stvalue != null && start != null)
                    {
                        if (value == 1)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "ePayLater" && x.IsReconcile == stvalue && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 2)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC" && x.IsReconcile == stvalue && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 3)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "Mpos" && x.IsReconcile == stvalue && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        //else if (value == 4)
                        //{
                        //    result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_UPI" && x.IsReconcile == stvalue && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x=>x.UploadDate).ToList();
                        //}
                        else if (value == 4)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_NetBanking" && x.IsReconcile == stvalue && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 6)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "credit hdfc" && x.IsReconcile == stvalue && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 7)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "Razorpay QR" && x.IsReconcile == stvalue && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 8)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "UPI" && x.IsReconcile == stvalue && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                    }
                    else
                    {
                        if (value == 1)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "ePayLater" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 2)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 3)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "Mpos" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        //else if (value == 4)
                        //{
                        //    result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_UPI" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x=>x.UploadDate).ToList();
                        //}
                        else if (value == 4)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_NetBanking" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 6)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "credit hdfc" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 7)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "Razorpay QR" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 8)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "UPI" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                    }
                    return result;

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion


        #region Show Upload File on Filter
        [Route("UploadFileSearch")]
        [HttpGet]
        public dynamic Getvalue(int value, DateTime? start, DateTime? end)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    if (value > 0)
                    {

                        if (value == 1)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "ePayLater" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 2)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        else if (value == 3)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "Mpos" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                        //else if (value == 4)
                        //{
                        //    result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_UPI" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x=>x.UploadDate).ToList();
                        //}
                        else if (value == 4)
                        {
                            result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_NetBanking" && x.UploadDate >= start && x.UploadDate <= end).OrderByDescending(x => x.UploadDate).ToList();
                        }
                    }

                    return result;
                }

                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion

        #region Show Upload File on Filter
        [Route("select")]
        [HttpGet]
        public dynamic GetData(int value)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    if (value == 1)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "ePayLater").OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 2)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC").OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 3)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "Mpos").OrderByDescending(x => x.UploadDate).ToList();
                    }
                    //else if (value == 4)
                    //{
                    //    result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_UPI").OrderByDescending(x=>x.UploadDate).ToList();
                    //}
                    else if (value == 4)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "HDFC_NetBanking").OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 6)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "credit hdfc").OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 7)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "Razorpay QR").OrderByDescending(x => x.UploadDate).ToList();
                    }
                    else if (value == 8)
                    {
                        result = db.UploadFileReconcileDB.Where(x => x.FileType == "UPI").OrderByDescending(x => x.UploadDate).ToList();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion



        #region Ge Epay html Details

        [Route("GetEpayDetails")]
        [HttpGet]
        public dynamic Getepy(int UploadId)
        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                try
                {

                    //List<EpaylaterUpload> payment = new List<EpaylaterUpload>();
                    // var list = db.EpaylaterUploadDB.Where(p => p.UplaodId == UploadId).ToList();
                    list = (from a in db.EpaylaterUploadDB
                            where (a.UplaodId == UploadId)
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                            from c in payment.DefaultIfEmpty()
                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            select new EpayUpload
                            {
                                TXNID = a.transaction_id,
                                TXNDate = a.transaction_date,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.Amount,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : ""
                            }).OrderByDescending(x => x.TXNDate).ToList();

                    //return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return list;
            }
        }
        #endregion

        #region Get HDFC Details
        [Route("GethdfcDetails")]
        [HttpGet]
        public dynamic Gethdfc(int UploadId)
        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                try
                {

                    //List<EpaylaterUpload> payment = new List<EpaylaterUpload>();
                    // var list = db.EpaylaterUploadDB.Where(p => p.UplaodId == UploadId).ToList();
                    list = (from a in db.HDFCUploadDB
                            where (a.UploadId == UploadId)
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                            from c in payment.DefaultIfEmpty()
                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            select new EpayUpload
                            {
                                TXNID = a.transaction_id,
                                TXNDate = a.transaction_date,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.Amount,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : ""
                            }).OrderByDescending(x => x.TXNDate).ToList();

                    //return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return list;
            }
        }
        #endregion

        #region Get mPos Deatils
        [Route("GetmPosDetails")]
        [HttpGet]
        public dynamic Getmpos(int UploadId)
        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                try
                {

                    //List<EpaylaterUpload> payment = new List<EpaylaterUpload>();
                    // var list = db.EpaylaterUploadDB.Where(p => p.UplaodId == UploadId).ToList();
                    list = (from a in db.MposUploadDB
                            where (a.UploadId == UploadId)
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                            from c in payment.DefaultIfEmpty()

                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            select new EpayUpload
                            {
                                TXNID = a.transaction_id,
                                TXNDate = a.transaction_date,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.Amount,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : ""
                            }).OrderByDescending(x => x.TXNDate).ToList();

                    //return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return list;
            }
        }

        #region Get UPI Details
        [Route("GetUPIDetails")]
        [HttpGet]
        public UPIDetailDCList GetUPIDetails(int UploadId, int Skip, int take)
        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                UPIDetailDCList res = new UPIDetailDCList();
                try
                {
                    list = (from a in db.UPIUploadDB
                            where (a.UploadId == UploadId)
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.CCAvenueRef equals c.GatewayOrderId into payment
                            from c in payment.DefaultIfEmpty()
                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            join ut in db.UPITransactions on c.OrderId equals ut.OrderId into UPITXN
                            from ut in UPITXN.DefaultIfEmpty()
                            where ut.Status == "SUCCESS"
                            select new EpayUpload
                            {
                                TXNID = a.transaction_id,
                                TXNDate = a.transaction_date,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.Amount,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : "",
                                TxnNo = p != null && ut != null ? ut.TxnNo : ""
                            }).OrderByDescending(x => x.TXNDate).Skip((Skip - 1) * take).Take(take).ToList();
                    var cnt = (from a in db.UPIUploadDB
                               where (a.UploadId == UploadId)
                               join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.CCAvenueRef equals c.GatewayOrderId into payment
                               from c in payment.DefaultIfEmpty()
                               join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                               from p in order.DefaultIfEmpty()
                               join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                               from d in dispatch.DefaultIfEmpty()
                               join ut in db.UPITransactions on c.OrderId equals ut.OrderId into UPITXN
                               from ut in UPITXN.DefaultIfEmpty()
                               where ut.Status == "SUCCESS"
                               select new EpayUpload
                               {
                               }).Count();
                    //return list;
                    res.list = list;
                    res.totalCount = cnt;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return res;
            }
        }
        #endregion

        #region Get ICICI Details
        [Route("GetICICIDetails")]
        [HttpGet]
        public dynamic GetIcici(int UploadId)
        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                try
                {
                    list = (from a in db.ICICIUploadDB
                            where (a.UploadId == UploadId)
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.TransactionID equals c.GatewayTransId into payment
                            from c in payment.DefaultIfEmpty()
                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            select new EpayUpload
                            {
                                TXNID = a.TransactionID,
                                TXNDate = a.TransmissionDateTime,
                                MerchantRefNo = a.MerchantRefNo,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.Amount,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : ""
                            }).OrderByDescending(x => x.TXNDate).ToList();

                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return list;
            }
        }
        #endregion

        #region Show  Latest icici Filter
        [Route("OnchangeIcici")]
        [HttpGet]
        public dynamic OnchangeIcici(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.ICICIUploadDB
                              where (a.UploadId == UploadId)
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.TransactionID equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()
                              select new EpayUpload
                              {
                                  TXNID = a.TransactionID,
                                  MerchantRefNo=a.MerchantRefNo,
                                  TXNDate = a.TransmissionDateTime,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.Amount,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.TXNDate).ToList();
                    
                    if(result.Count == 0 && status == false)
                    {
                        result = (from a in db.ICICIUploadDB
                                  where (a.UploadId == UploadId)
                                  join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.TransactionID equals c.GatewayTransId into payment
                                  from c in payment.DefaultIfEmpty()
                                  join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                                  from p in order.DefaultIfEmpty()
                                  join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                                  from d in dispatch.DefaultIfEmpty()
                                  select new EpayUpload
                                  {
                                      TXNID = a.TransactionID,
                                      MerchantRefNo = a.MerchantRefNo,
                                      TXNDate = a.TransmissionDateTime,
                                      paymentxnId = c != null ? c.GatewayTransId : null,
                                      TxnAomunt = 0,
                                      OrderId = 0,
                                      OrderAmount = 0,
                                      UploadAmount = a.Amount,
                                      Status = c != null ? c.IsSettled : false,
                                      Skcode = "",
                                      Comment = p != null ? c.SettleComments : ""
                                  }).OrderByDescending(x => x.TXNDate).ToList();
                    };
                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }

        #endregion
        #endregion


        #region Get HDFC UPI Details
        //[Route("GethdfcUPIDetails")]
        //[AllowAnonymous]
        //[HttpGet]
        //public dynamic GethdfcUPI(int UploadId)
        //{
        //    using (var db = new AuthContext())
        //    {
        //        dynamic list = null;
        //        try
        //        {

        //            //List<EpaylaterUpload> payment = new List<EpaylaterUpload>();
        //            // var list = db.EpaylaterUploadDB.Where(p => p.UplaodId == UploadId).ToList();
        //            list = (from a in db.HDFCUPIUploadDB
        //                    where (a.UploadId == UploadId)
        //                    join c in db.PaymentResponseRetailerAppDb on a.transaction_id equals c.GatewayTransId into payment
        //                    from c in payment.DefaultIfEmpty()
        //                    join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
        //                    from p in order.DefaultIfEmpty()
        //                    select new EpayUpload
        //                    {
        //                        TXNID = a.transaction_id,
        //                        TXNDate = a.transaction_date,
        //                        paymentxnId = c != null ? c.GatewayTransId : null,
        //                        TxnAomunt = c != null ? c.amount : 0,
        //                        OrderId = p != null ? p.OrderId : 0,
        //                        OrderAmount = p != null ? p.GrossAmount : 0,
        //                        UploadAmount = a.Amount,
        //                        Status = c != null ? c.IsSettled : false,
        //                        Skcode = p != null ? p.Skcode : "",
        //                        Comment = p != null ? c.SettleComments : ""
        //                    }).OrderByDescending(x => x.TXNDate).ToList();

        //            //return list;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Error in Customers Report  " + ex.Message);
        //            logger.Info("End  Customers Report: ");
        //            return null;
        //        }
        //        return list;
        //    }
        //}
        #endregion

        #region Get HDFC Net Banking Details
        [Route("GethdfcNetDetails")]
        [HttpGet]
        public dynamic GethdfcNetBanking(int UploadId)
        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                try
                {

                    //List<EpaylaterUpload> payment = new List<EpaylaterUpload>();
                    // var list = db.EpaylaterUploadDB.Where(p => p.UplaodId == UploadId).ToList();
                    list = (from a in db.HDFCNetBankingUploadDB
                            where (a.UploadId == UploadId && a.transaction_id != "")
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.transaction_id equals c.GatewayTransId into payment
                            from c in payment.DefaultIfEmpty()
                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            select new EpayUpload
                            {
                                TXNID = a.transaction_id,
                                TXNDate = a.transaction_date,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.Amount,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : ""
                            }).OrderByDescending(x => x.TXNDate).ToList();

                    //return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return list;
            }
        }
        #endregion



        #region Get Mpos Details
        [Route("Getpaymentid")]
        [HttpGet]
        public IEnumerable<PaymentResponseRetailerApp> getpayment(string txnid)
        {
            logger.Info("start PaymentResponse: ");
            using (var db = new AuthContext())
            {
                List<PaymentResponseRetailerApp> payment = new List<PaymentResponseRetailerApp>();
                var list = db.PaymentResponseRetailerAppDb.Where(p => p.status == "Success" && p.GatewayTransId == txnid).ToList();
                list.ForEach(x => x.amount = Math.Round(x.amount, 0));
                return list;
            }
        }
        #endregion
        #region Get HDFC Credit Details
        [Route("GethdfcCreditDetails")]
        [HttpGet]
        public dynamic GethdfcCreditDetails(int UploadId)

        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                try
                {

                    //List<EpaylaterUpload> payment = new List<EpaylaterUpload>();
                    // var list = db.EpaylaterUploadDB.Where(p => p.UplaodId == UploadId).ToList();
                    list = (from a in db.HDFCCreditUploadDB
                            where (a.UploadId == UploadId)
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.CCAvenueRef equals c.GatewayTransId into payment
                            from c in payment.DefaultIfEmpty()
                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            select new EpayUpload
                            {
                                TXNID = a.CCAvenueRef,
                                TXNDate = a.Date,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.Amount,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : ""
                            }).OrderByDescending(x => x.TXNDate).ToList();

                    //return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return list;
            }
        }
        #endregion
        #region Get Razorpay QR Details
        [Route("GetRazorpayQRDetails")]
        [HttpGet]
        public dynamic GetRazorpayQRDetails(int UploadId)

        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                try
                {

                    //List<EpaylaterUpload> payment = new List<EpaylaterUpload>();
                    // var list = db.EpaylaterUploadDB.Where(p => p.UplaodId == UploadId).ToList();
                    list = (from a in db.RazorpayQRUploadDB
                            where (a.UploadId == UploadId)
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.GatewayTransId equals c.GatewayTransId into payment
                            from c in payment.DefaultIfEmpty()

                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            select new EpayUpload
                            {
                                TXNID = a.GatewayTransId,
                                TXNDate = a.Date,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.Amount,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : ""
                            }).OrderByDescending(x => x.TXNDate).ToList();

                    //return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return list;
            }
        }
        #endregion




        #region Get ChequeUpload Details
        [Route("GetChequeUploadDetails")]
        [HttpGet]
        public dynamic ChequeUploadDetails(int UploadId)

        {
            using (var db = new AuthContext())
            {
                dynamic list = null;
                try
                {

                    //List<EpaylaterUpload> payment = new List<EpaylaterUpload>();
                    // var list = db.EpaylaterUploadDB.Where(p => p.UplaodId == UploadId).ToList();
                    list = (from a in db.ChqBookUploads
                            where (a.UploadId == UploadId)
                            join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.TRANSACTION_ID equals c.GatewayTransId into payment
                            from c in payment.DefaultIfEmpty()
                            join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                            from p in order.DefaultIfEmpty()
                            join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                            from d in dispatch.DefaultIfEmpty()
                            select new ChequeUploaddetails
                            {
                                TXNID = a.TRANSACTION_ID,
                                Orderid = a.Orderid,
                                InvoiceAmt = a.InvoiceAmt,
                                paymentxnId = c != null ? c.GatewayTransId : null,
                                TxnAomunt = c != null ? c.amount : 0,
                                OrderId = p != null ? p.OrderId : 0,
                                OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                UploadAmount = a.InvoiceAmt,
                                Status = c != null ? c.IsSettled : false,
                                Skcode = p != null ? p.Skcode : "",
                                Comment = p != null ? c.SettleComments : ""
                            }).OrderByDescending(x => x.OrderId).ToList();

                    //return list;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report  " + ex.Message);
                    logger.Info("End  Customers Report: ");
                    return null;
                }
                return list;
            }
        }
        #endregion

        #region Show  
        [Route("OnchangeChequeUpload")]
        [HttpGet]
        public dynamic GetstChequeUpload(int UploadId, bool status)
        {
            dynamic result = null;
            logger.Info("start Get Report2: ");
            using (AuthContext db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                    result = (from a in db.ChqBookUploads
                              where (a.UploadId == UploadId)
                              join c in db.PaymentResponseRetailerAppDb.Where(x => x.amount > 0) on a.TRANSACTION_ID equals c.GatewayTransId into payment
                              from c in payment.DefaultIfEmpty()
                              where (c.IsSettled == status)
                              join p in db.DbOrderMaster on c.OrderId equals p.OrderId into order
                              from p in order.DefaultIfEmpty()
                              join d in db.OrderDispatchedMasters on c.OrderId equals d.OrderId into dispatch
                              from d in dispatch.DefaultIfEmpty()
                              select new ChequeUploaddetails
                              {
                                  TXNID = a.TRANSACTION_ID,
                                  Orderid = a.Orderid,
                                  InvoiceAmt = a.InvoiceAmt,
                                  paymentxnId = c != null ? c.GatewayTransId : null,
                                  TxnAomunt = c != null ? c.amount : 0,
                                  OrderId = p != null ? p.OrderId : 0,
                                  OrderAmount = d != null ? d.GrossAmount : p != null ? p.GrossAmount : 0,
                                  UploadAmount = a.InvoiceAmt,
                                  Status = c != null ? c.IsSettled : false,
                                  Skcode = p != null ? p.Skcode : "",
                                  Comment = p != null ? c.SettleComments : ""
                              }).OrderByDescending(x => x.OrderId).ToList();

                    return result;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in Customers Report " + ex.Message);
                    logger.Info("End Customers Report: ");
                    return null;
                }
            }
        }
        #endregion


        public class paggingDTO
        {
            public List<OnlineTxn> OnlineTxns { get; set; }
            public int Total_Count { get; set; }
        }
        public class OnlineTxn
        {
            public int OrderId
            {
                get; set;
            }
            public string Skcode
            {
                get; set;
            }
            public DateTime Date
            {
                get; set;

            }
            public string Warehouse
            {
                get; set;
            }
            public double OrderAmount
            {
                get; set;
            }
            public double TxnAomunt
            {
                get; set;
            }
            public string TxnStatus
            {
                get; set;
            }
            public string TxnID
            {
                get; set;
            }
            public DateTime? SettleDate
            {
                get; set;
            }
            public string MOP
            {
                get; set;
            }
            public bool IsSettled
            {
                get; set;
            }
            public string SettlComments
            {
                get; set;
            }
            public int userid
            {
                get; set;
            }
            public int UploadId
            {
                get; set;
            }
            public string Orderstatus
            {
                get; set;
            }
            public bool IsRefund
            {
                get; set;
            }
            public int? DeliveryIssuanceId
            {
                get; set;
            }
        }

        public class EpayUpload
        {

            public string MerchantRefNo { get; set; }
            public string TXNID
            {
                get; set;
            }
            public DateTime TXNDate
            {
                get; set;

            }
            public int OrderId
            {
                get; set;
            }
            public double OrderAmount
            {
                get; set;
            }
            public double TxnAomunt
            {
                get; set;
            }
            public double UploadAmount
            {
                get; set;
            }
            public bool Status
            {
                get; set;
            }
            public string Skcode
            {
                get; set;
            }
            public string Comment
            {
                get; set;
            }
            public string paymentxnId
            {
                get; set;
            }
            public string TxnNo { get; set; }

        }

        public class UPIDetailDCList
        {
            public List<EpayUpload> list { get; set; }
            public int totalCount { get; set; }
        }

    }
    
}
