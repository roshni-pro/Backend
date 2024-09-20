
using AgileObjects.AgileMapper;
using AngularJSAuthentication.API.Controllers.Agent;
using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.DataContracts.Transaction.Stocks;
using AngularJSAuthentication.Model;
using GenricEcommers.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using LinqKit;
using MongoDB.Bson;
using Newtonsoft.Json;
using Nito.AspNetBackgroundTasks;
using Nito.AsyncEx;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Http;
using static AngularJSAuthentication.API.Controllers.OrderMasterrController;
using static AngularJSAuthentication.API.Controllers.OrderMastersAPIController;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeliveryIssuance")]
    public class DeliveryIssuanceController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        //nlogger
        public static Logger logger = LogManager.GetCurrentClassLogger();
        //public static List<int> AssignmentInProcess = new List<int>();

        [Route("")]
        [HttpGet]
        public PaggingData_Delivery Get(int list, int page, int id, DateTime? start, DateTime? end)//get all Issuances which b/w dates the delivery boy
        {
            using (var context = new AuthContext())
            {
                try
                {

                    if (start != null && end != null)
                    {

                        var data = context.AllDataDeliveryIssurance(list, page, id, start, end);
                        return data;
                    }
                    else
                    {
                        var data = context.AllDataDeliveryIssurance(list, page, id, null, null);
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        [Route("GetOrderamount")]
        [HttpGet]
        public HttpResponseMessage GetOrderamount(string ids)//get OrderAmount
        {
            using (var context = new AuthContext())
            {
                try
                {
                    List<int> values = ids.Split(',').Select(sValue => Convert.ToInt32(sValue.Trim())).ToList();
                    List<orderamount> list = new List<orderamount>();
                    list = context.OrderDispatchedMasters.Where(x => x.active == true && values.Contains(x.OrderId)).Select(DBoyorders =>
                     new orderamount()
                     {
                         orderid = DBoyorders.OrderId,
                         ShopName = DBoyorders.ShopName,
                         amount = DBoyorders.GrossAmount
                     }
                    ).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("GetAssingmentamount")]
        [HttpGet]
        public HttpResponseMessage GetAssingmentamount(int id)//get OrderAmount
        {
            using (var context = new AuthContext())
            {
                try
                {

                    List<orderamount> list = new List<orderamount>();
                    list = context.OrderDispatchedMasters.Where(x => x.active == true && x.DeliveryIssuanceIdOrderDeliveryMaster == id).Select(DBoyorders =>
                     new orderamount()
                     {
                         orderid = DBoyorders.OrderId,
                         ShopName = DBoyorders.ShopName,
                         amount = DBoyorders.GrossAmount,
                         DeliveryBoy = DBoyorders.DboyName,
                         AssignmentId = DBoyorders.DeliveryIssuanceIdOrderDeliveryMaster
                     }
                    ).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("GetAssOrder")]
        [HttpGet]
        public HttpResponseMessage getAssOrder(int AssignmentId)
        {
            using (var context = new AuthContext())
            {

                AssignmentPrintResultDc Result = new AssignmentPrintResultDc();
                List<OrderDispatchedMaster> finalList = new List<OrderDispatchedMaster>();
                var list = context.OrderDispatchedMasters.Where(a => a.DeliveryIssuanceIdOrderDeliveryMaster == AssignmentId).Include("orderDetails").ToList();
                //var OrderId = list.Select(x => x.OrderId).FirstOrDefault();
                var OrderIds = list.Select(x => x.OrderId).Distinct().ToList();
                var customerIds = list.Select(x => x.CustomerId).Distinct().ToList();
                var ordermasters = context.DbOrderMaster.Where(a => OrderIds.Contains(a.OrderId)).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer, x.IsFirstOrder, x.paymentThrough });

                //bool IsZaruriOrder = context.DbOrderMaster.Any(a => a.OrderId == OrderId && a.OrderType == 5);

                var ordermastersdetails = context.DbOrderDetails.Where(a => OrderIds.Contains(a.OrderId)).Select(x => new { x.OrderId, x.OrderDetailsId, x.PTR, x.ExecutiveName }).ToList();
                DataTable dt = new DataTable();
                dt.Columns.Add("IntValue");
                foreach (var item in customerIds)
                {
                    var dr = dt.NewRow();
                    dr["IntValue"] = item;
                    dt.Rows.Add(dr);
                }
                var param = new SqlParameter("CustomerId", dt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";

                List<GetStateCodeDc> GetStateCodeList = context.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByCustomerId @CustomerId", param).ToList();

                bool IsZaruriOrder = ordermasters.Any(a => a.OrderType == 5);
                list.ForEach(x =>
                {
                    if (ordermasters.Any(y => y.OrderId == x.OrderId))
                    {
                        x.OrderType = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).OrderType;
                        x.IsPrimeCustomer = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).IsPrimeCustomer;
                        x.IsFirstOrder = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).IsFirstOrder;
                        x.SalesPerson = string.Join(",", ordermastersdetails.Where(z => z.OrderId == x.OrderId && !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct());
                        if (!string.IsNullOrEmpty(x.SalesPerson))
                        {
                            var SalesPersonName = x.SalesPerson.Split(',').ToList();
                            var qquery = from c in context.Peoples
                                         where SalesPersonName.Contains(c.DisplayName)
                                         select c.Mobile;
                            x.SalesMobile = string.Join(",", qquery);
                        }
                        if (x.orderDetails != null)
                        {
                            foreach (var orderDetaillst in x.orderDetails)
                            {
                                orderDetaillst.PTR = ordermastersdetails.Where(z => z.OrderDetailsId == orderDetaillst.OrderDetailsId).Select(z => z.PTR).FirstOrDefault();
                            }
                        }

                    }
                });


                List<TradeInvoiceDc> files = new List<TradeInvoiceDc>();

                string FileName = "";
                string fileUrl = "";
                string folderPath = "";
                string fullPhysicalPath = "";
                string thFileName = "";

                var OrderPayments = context.PaymentResponseRetailerAppDb.Where(p => p.status == "Success" && OrderIds.Contains(p.OrderId)).ToList();
                var billdiscountOfferId = context.BillDiscountDb.Where(x => OrderIds.Contains(x.OrderId) && customerIds.Contains(x.CustomerId)).Select(z => new { z.OfferId, z.OrderId }).ToList();
                List<orderOfferDTO> OrderOffers = new List<orderOfferDTO>();
                if (billdiscountOfferId.Count > 0)
                {
                    var offerids = billdiscountOfferId.Select(x => x.OfferId).ToList();
                    OrderOffers = context.OfferDb.Where(x => offerids.Contains(x.OfferId)).Select(x => new orderOfferDTO { Offerid = x.OfferId, OfferOn = x.OfferOn, OfferCode = x.OfferCode }).ToList();
                    OrderOffers.ForEach(x => x.Orderid = billdiscountOfferId.FirstOrDefault(y => y.OfferId == x.Offerid).OrderId);
                }
                foreach (var od in list)
                {
                    if (IsZaruriOrder)
                    {
                        od.OrderType = 5;

                        FileName = od.invoice_no.Replace('/', '_') + ".pdf";
                        folderPath = HttpContext.Current.Server.MapPath(@"~\ZaruriReportDownloads");

                        string TartgetfolderPath = "";

                        TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ZaruriAssignmentDownloads");
                        if (!Directory.Exists(TartgetfolderPath))
                            Directory.CreateDirectory(TartgetfolderPath);


                        thFileName = "Asss_" + AssignmentId + ".pdf";
                        fileUrl = "/ZaruriAssignmentDownloads" + "/" + thFileName;
                        fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;

                        TradeInvoiceDc dc = new TradeInvoiceDc();
                        dc.OrderId = od.OrderId;
                        dc.FileName = folderPath + "\\" + FileName;
                        dc.barcode = od.InvoiceBarcodeImage;
                        dc.invoice_no = od.invoice_no;
                        files.Add(dc);
                    }
                    else
                    {
                        od.OrderType = ordermasters.FirstOrDefault(x => x.OrderId == od.OrderId).OrderType; // 1;
                        od.paymentThrough = ordermasters.FirstOrDefault(x => x.OrderId == od.OrderId).paymentThrough;
                        if (od.BillDiscountAmount > 0)
                        {
                            if (OrderOffers.Any(x => x.Orderid == od.OrderId))
                            {
                                od.offertype = string.Join(",", OrderOffers.Where(x => x.Orderid == od.OrderId).Select(x => x.OfferOn));
                            }
                            //var billdiscountOfferId = context.BillDiscountDb.Where(x => x.OrderId == od.OrderId).Select(z => z.OfferId).ToList();
                            //if (billdiscountOfferId.Count > 0)
                            //{
                            //    List<string> offeron = context.OfferDb.Where(x => billdiscountOfferId.Contains(x.OfferId)).Select(x => new { x.OfferOn, x.OfferCode }).ToList().Select(x => (!string.IsNullOrEmpty(x.OfferCode) ? x.OfferCode : x.OfferOn)).ToList();
                            //    od.offertype = string.Join(",", offeron);
                            //}

                        }
                        //for igst case if true then apply condion to hide column of cgst sgst cess
                        if (!string.IsNullOrEmpty(od.Tin_No) && od.Tin_No.Length >= 11)
                        {
                            string CustTin_No = od.Tin_No.Substring(0, 2);
                            od.IsIgstInvoice = !context.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == od.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No);

                        }
                        od.PaymentDetails = OrderPayments.Where(p => p.OrderId == od.OrderId).ToList();
                        if (!(string.IsNullOrEmpty(od.paymentThrough)) && od.paymentThrough.ToLower() == "paylater")
                        {
                            List<PaymentResponseRetailerAppDc> res = new List<PaymentResponseRetailerAppDc>();
                            var orderid = new SqlParameter("@orderId", od.OrderId);
                            res = context.Database.SqlQuery<PaymentResponseRetailerAppDc>("exec GetOrderpayment @orderId", orderid).ToList();
                            if (res != null)
                            {
                                od.PaymentDetails.ForEach(a =>
                                {
                                    a.statusDesc = res.Any(x => x.id == a.id) ? res.FirstOrDefault(x => x.id == a.id).DueDaysDesc : a.statusDesc;
                                });
                            }
                        }

                        //---------E-------------------
                        if (GetStateCodeList != null && GetStateCodeList.Any(x => x.CustomerId == od.CustomerId))
                        {
                            od.shippingStateName = GetStateCodeList.FirstOrDefault(x => x.CustomerId == od.CustomerId).shippingStateName;
                            od.shippingStateCode = GetStateCodeList.FirstOrDefault(x => x.CustomerId == od.CustomerId).shippingStateCode;
                            od.BillingStateName = GetStateCodeList.FirstOrDefault(x => x.CustomerId == od.CustomerId).BillingStateName;
                            od.BillingStateCode = GetStateCodeList.FirstOrDefault(x => x.CustomerId == od.CustomerId).BillingStateCode;
                        }
                    }

                    decimal Amount = Convert.ToDecimal(od.GrossAmount - od.DiscountAmount);
                    if (Amount > 0)
                        od.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee(Amount);
                    else
                        od.InvoiceAmountInWord = "";

                }
                if (IsZaruriOrder)
                {

                    if (!File.Exists(fullPhysicalPath))
                    {
                        bool iss = MergePDFs(files, fullPhysicalPath);
                        if (!iss)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Pdf not created");
                        }

                    }
                    fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                     , HttpContext.Current.Request.Url.Port
                                                     , fileUrl);

                    Result.OrderTYpe = 5;
                    Result.PdfPath = fileUrl;
                    return Request.CreateResponse(HttpStatusCode.OK, Result);
                }
                Result.OrderTYpe = 1;
                Result.OrderDispatchedMasters = list;
                return Request.CreateResponse(HttpStatusCode.OK, Result);
            }
        }
        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get(string all, int id)//get all issuabces within one month
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var dt = indianTime.Date;
                    var st = dt.AddDays(-30);
                    var DBoyorders = context.DeliveryIssuanceDb.Where(x => x.CreatedDate > st && x.CreatedDate <= dt && x.PeopleID == id).Include("details").ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, DBoyorders);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }



        #region Accept reject assignment by delivery app 
        /// <summary>
        /// Created by 07-03-2019
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("V2")]
        [HttpPut]
        public HttpResponseMessage PutAccetanceV2(AssignmentAccRejDTO obj)
        {
            ResMsg res;
            if (obj != null && obj.DeliveryIssuanceId > 0)
            {
                //if (AssignmentInProcess.Any(x => x == obj.DeliveryIssuanceId))
                //{
                //    res = new ResMsg()
                //    {
                //        Status = false,
                //        Message = "Assignment #: " + obj.DeliveryIssuanceId + " is already in process..",
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, res);
                //}
                //else
                //{
                //    AssignmentInProcess.Add(obj.DeliveryIssuanceId);
                //}
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (var context = new AuthContext())
                    {
                        var DBoyorders = context.DeliveryIssuanceDb.FirstOrDefault(x => x.DeliveryIssuanceId == obj.DeliveryIssuanceId && x.IsActive == true);
                        var people = context.Peoples.FirstOrDefault(x => x.PeopleID == DBoyorders.PeopleID);

                        if (DBoyorders != null && people.Active)
                        {
                            string[] ids = DBoyorders.OrderdispatchIds.Split(',');
                            List<int> AcceptOrderDispatchedMasterIds = new List<int>();
                            foreach (var AcceptOrderDispatchedMasterId in ids)
                            {
                                AcceptOrderDispatchedMasterIds.Add(Convert.ToInt32(AcceptOrderDispatchedMasterId));
                            }

                            var orderDispatchMasterList = context.OrderDispatchedMasters.Where(x => AcceptOrderDispatchedMasterIds.Contains(x.OrderDispatchedMasterId)).Distinct().ToList();
                            List<int> OrderIds = new List<int>();


                            OrderIds = orderDispatchMasterList.Select(x => x.OrderId).Distinct().ToList();
                            var orderDispatchMasterDetailList = context.OrderDispatchedDetailss.Where(x => OrderIds.Contains(x.OrderId)).ToList();

                            var orderMasterList = context.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                            var orderMasterDetailList = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();


                            DBoyorders.Acceptance = obj.Acceptance;
                            if (obj.Acceptance)
                            {
                                DBoyorders.Status = "Accepted";
                            }
                            else
                            {
                                DBoyorders.Status = "Rejected";
                            }
                            DBoyorders.RejectReason = obj.RejectReason;
                            DBoyorders.IsActive = false;
                            DBoyorders.UpdatedDate = indianTime;
                            context.Entry(DBoyorders).State = EntityState.Modified;

                            #region  DeliveryHistory
                            OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                            AssginDeli.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                            //AssginDeli.OrderId = delivery.o
                            AssginDeli.Cityid = DBoyorders.Cityid;
                            AssginDeli.city = DBoyorders.city;
                            AssginDeli.DisplayName = DBoyorders.DisplayName;
                            AssginDeli.Status = DBoyorders.Status;
                            AssginDeli.WarehouseId = DBoyorders.WarehouseId;
                            AssginDeli.PeopleID = people.PeopleID;
                            AssginDeli.VehicleId = DBoyorders.VehicleId;
                            AssginDeli.VehicleNumber = DBoyorders.VehicleNumber;
                            AssginDeli.RejectReason = DBoyorders.RejectReason;
                            //AssginDeli.OrderdispatchIds = obj.OrderdispatchIds;
                            AssginDeli.OrderIds = DBoyorders.OrderIds;
                            AssginDeli.Acceptance = DBoyorders.Acceptance;
                            AssginDeli.IsActive = DBoyorders.IsActive;
                            AssginDeli.IdealTime = DBoyorders.IdealTime;
                            AssginDeli.TravelDistance = DBoyorders.TravelDistance;
                            AssginDeli.CreatedDate = indianTime;
                            AssginDeli.UpdatedDate = indianTime;
                            AssginDeli.userid = people.PeopleID;
                            if (people.DisplayName == null)
                            {
                                AssginDeli.UpdatedBy = people.PeopleFirstName;
                            }
                            else
                            {
                                AssginDeli.UpdatedBy = people.DisplayName;
                            }
                            context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                            #endregion
                            if (obj.Acceptance)
                            {
                                #region  Agent Entry for Assignment Accepted
                                BackgroundTaskManager.Run(() =>
                                {
                                    AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
                                    if (DBoyorders.AgentId > 0)
                                    {
                                        agentLedgerHelper.OnAssignmentAccepted(DBoyorders.AgentId, Convert.ToDouble(DBoyorders.TotalAssignmentAmount), DBoyorders.DisplayName, 0, DBoyorders.DeliveryIssuanceId, DBoyorders.OrderIds.ToString());
                                    }
                                });
                                #endregion


                                foreach (var orderdipatchmaster in orderDispatchMasterList.Where(x => AcceptOrderDispatchedMasterIds.Contains(x.OrderDispatchedMasterId)).Distinct().ToList())
                                {
                                    if (orderdipatchmaster.Status != "Shipped" && orderdipatchmaster.Status == "Issued")
                                    {

                                        orderdipatchmaster.Status = "Shipped";
                                        orderdipatchmaster.ReDispatchedStatus = "Shipped";
                                        orderdipatchmaster.UpdatedDate = indianTime;
                                        context.Entry(orderdipatchmaster).State = EntityState.Modified;
                                        foreach (var orderMaster in orderMasterList.Where(x => x.OrderId == orderdipatchmaster.OrderId).ToList())
                                        {
                                            orderMaster.Status = "Shipped";
                                            orderMaster.UpdatedDate = indianTime;
                                            context.Entry(orderMaster).State = EntityState.Modified;

                                            foreach (var item in orderMasterDetailList.Where(c => c.OrderId == orderMaster.OrderId))
                                            {
                                                item.UpdatedDate = indianTime;
                                                item.status = "Shipped";
                                                context.Entry(item).State = EntityState.Modified;

                                            }


                                            if (orderMaster != null && orderMaster.OrderType != 5)
                                            {
                                                #region stock Hit

                                                //for Issued
                                                MultiStockHelper<OnShippedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedStockEntryDc>();
                                                List<OnShippedStockEntryDc> OnShippedStockEntryList = new List<OnShippedStockEntryDc>();
                                                foreach (var StockHit in orderDispatchMasterDetailList.Where(x => x.qty > 0 && x.OrderId == orderdipatchmaster.OrderId))
                                                {
                                                    var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                    bool isFree = orderMasterDetailList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                    if (isFree) { RefStockCode = "F"; }
                                                    else if (orderMaster.OrderType == 6) //6 Damage stock
                                                    {
                                                        RefStockCode = "D";
                                                    }
                                                    else if (orderMaster.OrderType == 9) //9 Non Selllable stock
                                                    {
                                                        RefStockCode = "N";
                                                    }

                                                    OnShippedStockEntryList.Add(new OnShippedStockEntryDc
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
                                                if (OnShippedStockEntryList.Any())
                                                {

                                                    bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_OnShipped", context, scope);
                                                    if (!ress)
                                                    {
                                                        res = new ResMsg()
                                                        {
                                                            Status = false,
                                                            Message = "Can't Shipped, Something went wrong."
                                                        };
                                                        // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                                        return Request.CreateResponse(HttpStatusCode.OK, res); ;
                                                    }
                                                }

                                                #endregion
                                            }

                                            if (orderMaster != null && orderMaster.OrderType == 5)
                                            {
                                                try
                                                {
                                                    UpdateConsumerOrders updateConsumerOrders = new UpdateConsumerOrders
                                                    {
                                                        CartStatus = "Shipped",
                                                        InvoiceNo = orderMaster.invoice_no
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
                                        }
                                    }
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

                                var paramStatus = new SqlParameter("Status", "Shipped");
                                var paramReasoncancel = new SqlParameter("Reasoncancel", "");
                                var paramWarehousename = new SqlParameter("WarehouseName", orderDispatchMasterList.FirstOrDefault().WarehouseName);
                                var paramusername = new SqlParameter("username", people.DisplayName != null ? people.DisplayName : people.PeopleFirstName);
                                var paramuserid = new SqlParameter("userid", System.Data.SqlDbType.Int);
                                paramuserid.Value = people.PeopleID;
                                var paramDeliveryIssuanceId = new SqlParameter("DeliveryIssuanceId", System.Data.SqlDbType.Int);
                                paramDeliveryIssuanceId.Value = obj.DeliveryIssuanceId;
                                var paramIsReAttempt = new SqlParameter("IsReAttempt", false);
                                var paramDescription = new SqlParameter("Description", "");

                                int IsOrderMasterHistories = context.Database.ExecuteSqlCommand("Picker.InsertOrderMasterHistories @userid, @DeliveryIssuanceId, @IsReAttempt, @orderids, @Status, @Reasoncancel, @WarehouseName, @username, @Description ",
                                    paramuserid, paramDeliveryIssuanceId, paramIsReAttempt, paramorderids, paramStatus, paramReasoncancel, paramWarehousename, paramusername, paramDescription);


                                #endregion

                                context.Commit();

                            }
                            else
                            {
                                List<OrderMasterHistories> AddOrderMasterHistories = new List<OrderMasterHistories>();
                                foreach (var orderdipatchmaster in orderDispatchMasterList.Where(x => AcceptOrderDispatchedMasterIds.Contains(x.OrderDispatchedMasterId)).Distinct().ToList())
                                {
                                    foreach (var orderMaster in orderMasterList.Where(x => x.OrderId == orderdipatchmaster.OrderId).ToList())
                                    {
                                        if (orderdipatchmaster.Status != "Shipped" && orderdipatchmaster.Status == "Issued")
                                        {

                                            if (orderdipatchmaster.ReDispatchCount > 0 && !orderdipatchmaster.IsReAttempt)
                                            {
                                                orderdipatchmaster.Status = "Delivery Redispatch";
                                                orderdipatchmaster.ReDispatchedStatus = "Delivery Redispatch";
                                                orderdipatchmaster.ReDispatchCount += 1;
                                                orderdipatchmaster.UpdatedDate = indianTime;
                                                context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                                orderMaster.Status = "Delivery Redispatch";
                                                orderMaster.ReDispatchCount += 1;
                                                orderMaster.UpdatedDate = indianTime;
                                                foreach (var item in orderMasterDetailList.Where(x => x.OrderId == orderdipatchmaster.OrderId).ToList())
                                                {
                                                    item.UpdatedDate = indianTime;
                                                    item.status = "Delivery Redispatch";
                                                    context.Entry(orderMaster).State = EntityState.Modified;

                                                }
                                                context.Entry(orderMaster).State = EntityState.Modified;
                                            }
                                            else if (orderdipatchmaster.IsReAttempt && orderdipatchmaster.ReAttemptCount > 0)
                                            {
                                                orderdipatchmaster.Status = "Delivery Redispatch";
                                                orderdipatchmaster.ReDispatchedStatus = "Delivery Redispatch";
                                                orderdipatchmaster.ReAttemptCount += 1;
                                                orderdipatchmaster.IsReAttempt = true;
                                                orderdipatchmaster.UpdatedDate = indianTime;
                                                context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                                orderMaster.Status = "Delivery Redispatch";
                                                orderMaster.UpdatedDate = indianTime;
                                                foreach (var item in orderMasterDetailList.Where(x => x.OrderId == orderdipatchmaster.OrderId).ToList())
                                                {
                                                    item.UpdatedDate = indianTime;
                                                    item.status = "Delivery Redispatch";
                                                    context.Entry(orderMaster).State = EntityState.Modified;

                                                }
                                                context.Entry(orderMaster).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                orderdipatchmaster.Status = "Ready to Dispatch";
                                                orderdipatchmaster.ReDispatchedStatus = "Ready to Dispatch";
                                                orderdipatchmaster.UpdatedDate = indianTime;
                                                context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                                orderMaster.Status = "Ready to Dispatch";
                                                orderMaster.UpdatedDate = indianTime;
                                                foreach (var item in orderMasterDetailList.Where(c => c.OrderId == orderMaster.OrderId))
                                                {
                                                    item.UpdatedDate = indianTime;
                                                    item.status = "Ready to Dispatch";
                                                    context.Entry(item).State = EntityState.Modified;

                                                }
                                                context.Entry(orderMaster).State = EntityState.Modified;
                                            }

                                            OrderMasterHistories h1 = new OrderMasterHistories();

                                            h1.orderid = orderdipatchmaster.OrderId;
                                            h1.Status = orderdipatchmaster.Status;
                                            h1.Description = "Due to Assignment Reject from Dapp";
                                            h1.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                            h1.Warehousename = orderdipatchmaster.WarehouseName;
                                            h1.username = DBoyorders.DisplayName;
                                            h1.userid = DBoyorders.PeopleID;
                                            h1.CreatedDate = indianTime;
                                            AddOrderMasterHistories.Add(h1);

                                            if (orderMaster != null && orderMaster.OrderType != 5)
                                            {
                                                #region stock Hit
                                                //for Issued to Delivery Redispatched
                                                if (orderdipatchmaster.ReDispatchCount > 0 && !orderdipatchmaster.IsReAttempt)
                                                {
                                                    MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                                    List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();
                                                    foreach (var StockHit in orderDispatchMasterDetailList.Where(x => x.qty > 0 && x.OrderId == orderdipatchmaster.OrderId))
                                                    {
                                                        var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                        bool isFree = orderMasterDetailList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                        if (isFree) { RefStockCode = "F"; }
                                                        else if (orderMaster.OrderType == 6) //6 Damage stock
                                                        {
                                                            RefStockCode = "D";
                                                        }
                                                        else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                                        {
                                                            RefStockCode = "N";
                                                        }
                                                        OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
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
                                                    if (OnShippedStockEntryList.Any())
                                                    {
                                                        bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_RedispatchOnAssignmentRejectFromIssued", context, scope);
                                                        if (!ress)
                                                        {
                                                            res = new ResMsg()
                                                            {
                                                                Status = false,
                                                                Message = "Can't Reject Issued, Something went wrong."
                                                            };
                                                            // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                                            return Request.CreateResponse(HttpStatusCode.OK, res); ;
                                                        }
                                                    }
                                                }
                                                else if (orderdipatchmaster.IsReAttempt && orderdipatchmaster.ReAttemptCount > 0)
                                                {
                                                    MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                                    List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();
                                                    foreach (var StockHit in orderDispatchMasterDetailList.Where(x => x.qty > 0 && x.OrderId == orderdipatchmaster.OrderId))
                                                    {
                                                        var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                        bool isFree = orderMasterDetailList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                        if (isFree) { RefStockCode = "F"; }
                                                        else if (orderMaster.OrderType == 6) //6 Damage stock
                                                        {
                                                            RefStockCode = "D";
                                                        }
                                                        else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                                        {
                                                            RefStockCode = "N";
                                                        }
                                                        OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
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
                                                    if (OnShippedStockEntryList.Any())
                                                    {
                                                        bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_RedispatchOnAssignmentRejectFromIssued", context, scope);
                                                        if (!ress)
                                                        {
                                                            res = new ResMsg()
                                                            {
                                                                Status = false,
                                                                Message = "Can't Reject Issued, Something went wrong."
                                                            };
                                                            // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                                            return Request.CreateResponse(HttpStatusCode.OK, res); ;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //for Issued to RTD
                                                    MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                                    List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();

                                                    foreach (var StockHit in orderDispatchMasterDetailList.Where(x => x.qty > 0 && x.OrderId == orderdipatchmaster.OrderId))
                                                    {
                                                        var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                        bool isFree = orderMasterDetailList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                        if (isFree) { RefStockCode = "F"; }
                                                        else if (orderMaster.OrderType == 6) //6 Damage stock
                                                        {
                                                            RefStockCode = "D";
                                                        }
                                                        else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                                        {
                                                            RefStockCode = "N";
                                                        }
                                                        OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
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
                                                    if (OnShippedStockEntryList.Any())
                                                    {
                                                        bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_OnShippedReject", context, scope);
                                                        if (!ress)
                                                        {
                                                            res = new ResMsg()
                                                            {
                                                                Status = false,
                                                                Message = "Can't Reject Issued, Something went wrong."
                                                            };
                                                            // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                                            return Request.CreateResponse(HttpStatusCode.OK, res); ;
                                                        }
                                                    }
                                                }
                                                #endregion
                                            }


                                        }

                                    }
                                }
                                if (AddOrderMasterHistories != null && AddOrderMasterHistories.Any())
                                {
                                    context.OrderMasterHistoriesDB.AddRange(AddOrderMasterHistories);
                                }
                                context.Commit();

                            }
                            scope.Complete();

                            res = new ResMsg()
                            {
                                Status = true,
                                Message = "Success."
                            };
                        }
                        else
                        {
                            res = new ResMsg()
                            {
                                Status = false,
                                Message = "Dboy is deactive"
                            };
                        }
                        // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            else
            {
                res = new ResMsg()
                {
                    Status = false,
                    Message = "Something went wrong..",
                };
                return Request.CreateResponse(HttpStatusCode.OK, res); ;
            }

        }
        #endregion
        [HttpGet]
        [Route("AssignmentInProgress")]
        public bool AssignmentInProgress(int DeliveryIssuanceId)
        {
            bool status = false;
            //if (AssignmentInProcess.Any(x => x == DeliveryIssuanceId))
            //{
            //    AssignmentInProcess.RemoveAll(x => x == DeliveryIssuanceId);
            //    status = true;
            //}
            return status;
        }
        [Route("AssignmentAcceptNew")]
        [HttpPut]
        public HttpResponseMessage AssignmentAcceptNew(AssignmentAccRejDTO obj)
        {
            ResMsgNew res;
            if (obj != null && obj.DeliveryIssuanceId > 0)
            {
                //if (AssignmentInProcess.Any(x => x == obj.DeliveryIssuanceId))
                //{
                //    res = new ResMsgNew()
                //    {
                //        CheckAssignmentEwayBillAndIRNno = null,
                //        Status = false,
                //        Message = "Assignment #: " + obj.DeliveryIssuanceId + " is already in process..",
                //    };
                //    return Request.CreateResponse(HttpStatusCode.OK, res);
                //}
                //else
                //{
                //    AssignmentInProcess.Add(obj.DeliveryIssuanceId);
                //}
                List<int> OrderIds = new List<int>();
                TransactionOptions option = new TransactionOptions();
                option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
                option.Timeout = TimeSpan.FromSeconds(120);
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
                {
                    using (var context = new AuthContext())
                    {

                        var DBoyorders = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == obj.DeliveryIssuanceId && x.IsActive == true).SingleOrDefault();

                        if (DBoyorders != null)
                        {
                            var DeliveryIssuanceId = new SqlParameter("@DeliveryIssuanceId", DBoyorders.DeliveryIssuanceId);
                            var checkAssignmentEwayBillAndIRNno = context.Database.SqlQuery<CheckAssignmentEwayBillAndIRNnoDC>("EXEC Operation.TripPlanner_CheckAssignmentEwayBillAndIRNno @DeliveryIssuanceId", DeliveryIssuanceId).ToList();
                            if (checkAssignmentEwayBillAndIRNno.Count() == 0)
                            {
                                string[] ids = DBoyorders.OrderdispatchIds.Split(',');
                                List<int> AcceptOrderDispatchedMasterIds = new List<int>();
                                foreach (var AcceptOrderDispatchedMasterId in ids)
                                {
                                    AcceptOrderDispatchedMasterIds.Add(Convert.ToInt32(AcceptOrderDispatchedMasterId));
                                }

                                var orderDispatchMasterList = context.OrderDispatchedMasters.Where(x => AcceptOrderDispatchedMasterIds.Contains(x.OrderDispatchedMasterId)).Distinct().ToList();
                                OrderIds = orderDispatchMasterList.Select(x => x.OrderId).Distinct().ToList();
                                var orderDispatchMasterDetailList = context.OrderDispatchedDetailss.Where(x => OrderIds.Contains(x.OrderId)).ToList();

                                var orderMasterList = context.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                                var orderMasterDetailList = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();


                                var people = context.Peoples.Where(x => x.PeopleID == DBoyorders.PeopleID).SingleOrDefault();
                                DBoyorders.Acceptance = obj.Acceptance;
                                if (obj.Acceptance)
                                {
                                    DBoyorders.Status = "Accepted";
                                }
                                else
                                {
                                    DBoyorders.Status = "Rejected";
                                }
                                DBoyorders.RejectReason = obj.RejectReason;
                                DBoyorders.IsActive = false;
                                DBoyorders.UpdatedDate = indianTime;
                                context.Entry(DBoyorders).State = EntityState.Modified;
                                #region  DeliveryHistory
                                OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                                AssginDeli.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                                //AssginDeli.OrderId = delivery.o
                                AssginDeli.Cityid = DBoyorders.Cityid;
                                AssginDeli.city = DBoyorders.city;
                                AssginDeli.DisplayName = DBoyorders.DisplayName;
                                AssginDeli.Status = DBoyorders.Status;
                                AssginDeli.WarehouseId = DBoyorders.WarehouseId;
                                AssginDeli.PeopleID = people.PeopleID;
                                AssginDeli.VehicleId = DBoyorders.VehicleId;
                                AssginDeli.VehicleNumber = DBoyorders.VehicleNumber;
                                AssginDeli.RejectReason = DBoyorders.RejectReason;
                                //AssginDeli.OrderdispatchIds = obj.OrderdispatchIds;
                                AssginDeli.OrderIds = DBoyorders.OrderIds;
                                AssginDeli.Acceptance = DBoyorders.Acceptance;
                                AssginDeli.IsActive = DBoyorders.IsActive;
                                AssginDeli.IdealTime = DBoyorders.IdealTime;
                                AssginDeli.TravelDistance = DBoyorders.TravelDistance;
                                AssginDeli.CreatedDate = indianTime;
                                AssginDeli.UpdatedDate = indianTime;
                                AssginDeli.userid = people.PeopleID;
                                if (people.DisplayName == null)
                                {
                                    AssginDeli.UpdatedBy = people.PeopleFirstName;
                                }
                                else
                                {
                                    AssginDeli.UpdatedBy = people.DisplayName;
                                }
                                context.OrderDeliveryMasterHistoriesDB.Add(AssginDeli);
                                #endregion
                                if (obj.Acceptance)
                                {
                                    #region  Agent Entry for Assignment Accepted
                                    if (DBoyorders.AssignmentType != 4)
                                    {
                                        BackgroundTaskManager.Run(() =>
                                        {
                                            AgentLedgerHelper agentLedgerHelper = new AgentLedgerHelper();
                                            if (DBoyorders.AgentId > 0)
                                            {
                                                agentLedgerHelper.OnAssignmentAccepted(DBoyorders.AgentId, Convert.ToDouble(DBoyorders.TotalAssignmentAmount), DBoyorders.DisplayName, 0, DBoyorders.DeliveryIssuanceId, DBoyorders.OrderIds.ToString());
                                            }
                                        });
                                    }
                                    #endregion


                                    foreach (int oid in AcceptOrderDispatchedMasterIds)
                                    {
                                        foreach (var orderdipatchmaster in orderDispatchMasterList.Where(x => x.OrderDispatchedMasterId == oid).ToList())
                                        {
                                            if (orderdipatchmaster.Status != "Shipped" && orderdipatchmaster.Status == "Issued")
                                            {

                                                orderdipatchmaster.Status = "Shipped";
                                                orderdipatchmaster.ReDispatchedStatus = "Shipped";
                                                orderdipatchmaster.UpdatedDate = indianTime;
                                                context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                                foreach (var orderMaster in orderMasterList.Where(x => x.OrderId == orderdipatchmaster.OrderId).ToList())
                                                {
                                                    orderMaster.Status = "Shipped";
                                                    orderMaster.UpdatedDate = indianTime;
                                                    context.Entry(orderMaster).State = EntityState.Modified;
                                                    foreach (var item in orderMasterDetailList.Where(c => c.OrderId == orderMaster.OrderId))
                                                    {
                                                        item.UpdatedDate = indianTime;
                                                        item.status = "Shipped";
                                                        context.Entry(item).State = EntityState.Modified;

                                                    }

                                                    if (orderMaster != null && orderMaster.OrderType != 5 && orderMaster.OrderType != 3)
                                                    {
                                                        #region stock Hit

                                                        //for Issued
                                                        MultiStockHelper<OnShippedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedStockEntryDc>();
                                                        List<OnShippedStockEntryDc> OnShippedStockEntryList = new List<OnShippedStockEntryDc>();
                                                        foreach (var StockHit in orderDispatchMasterDetailList.Where(x => x.qty > 0 && x.OrderId == orderdipatchmaster.OrderId))
                                                        {
                                                            var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                            bool isFree = orderMasterDetailList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                            if (isFree) { RefStockCode = "F"; }
                                                            else if (orderMaster.OrderType == 6) //6 Damage stock
                                                            {
                                                                RefStockCode = "D";
                                                            }
                                                            else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                                            {
                                                                RefStockCode = "N";
                                                            }
                                                            OnShippedStockEntryList.Add(new OnShippedStockEntryDc
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
                                                        if (OnShippedStockEntryList.Any())
                                                        {

                                                            bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_OnShipped", context, scope);
                                                            if (!ress)
                                                            {
                                                                res = new ResMsgNew()
                                                                {
                                                                    CheckAssignmentEwayBillAndIRNno = null,
                                                                    Status = false,
                                                                    Message = "Can't Shipped, Something went wrong."
                                                                };
                                                                // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                                                return Request.CreateResponse(HttpStatusCode.OK, res); ;
                                                            }
                                                        }

                                                        #endregion
                                                    }

                                                    if (orderMaster != null && orderMaster.OrderType == 5)
                                                    {
                                                        try
                                                        {
                                                            UpdateConsumerOrders updateConsumerOrders = new UpdateConsumerOrders
                                                            {
                                                                CartStatus = "Shipped",
                                                                InvoiceNo = orderMaster.invoice_no
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
                                                }
                                            }
                                        }
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

                                    var paramStatus = new SqlParameter("Status", "Shipped");
                                    var paramReasoncancel = new SqlParameter("Reasoncancel", "");
                                    var paramWarehousename = new SqlParameter("WarehouseName", orderDispatchMasterList.FirstOrDefault().WarehouseName);
                                    var paramusername = new SqlParameter("username", people.DisplayName != null ? people.DisplayName : people.PeopleFirstName);
                                    var paramuserid = new SqlParameter("userid", System.Data.SqlDbType.Int);
                                    paramuserid.Value = people.PeopleID;
                                    var paramDeliveryIssuanceId = new SqlParameter("DeliveryIssuanceId", System.Data.SqlDbType.Int);
                                    paramDeliveryIssuanceId.Value = obj.DeliveryIssuanceId;
                                    var paramIsReAttempt = new SqlParameter("IsReAttempt", false);
                                    var paramDescription = new SqlParameter("Description", "");

                                    int IsOrderMasterHistories = context.Database.ExecuteSqlCommand("Picker.InsertOrderMasterHistories @userid, @DeliveryIssuanceId, @IsReAttempt, @orderids, @Status, @Reasoncancel, @WarehouseName, @username, @Description ",
                                        paramuserid, paramDeliveryIssuanceId, paramIsReAttempt, paramorderids, paramStatus, paramReasoncancel, paramWarehousename, paramusername, paramDescription);


                                    #endregion
                                    context.Commit();
                                }
                                else
                                {

                                    List<OrderMasterHistories> AddOrderMasterHistories = new List<OrderMasterHistories>();
                                    foreach (var orderdipatchmaster in orderDispatchMasterList.Where(x => AcceptOrderDispatchedMasterIds.Contains(x.OrderDispatchedMasterId)).ToList())
                                    {
                                        foreach (var orderMaster in orderMasterList.Where(x => x.OrderId == orderdipatchmaster.OrderId).ToList())
                                        {
                                            if (orderdipatchmaster.Status != "Shipped" && orderdipatchmaster.Status == "Issued")
                                            {
                                                if (orderdipatchmaster.Status != "Shipped" && orderdipatchmaster.Status == "Issued")
                                                {
                                                    if (orderdipatchmaster.ReDispatchCount > 0 && !orderdipatchmaster.IsReAttempt)
                                                    {
                                                        orderdipatchmaster.Status = "Delivery Redispatch";
                                                        orderdipatchmaster.ReDispatchedStatus = "Delivery Redispatch";
                                                        orderdipatchmaster.ReDispatchCount += 1;
                                                        orderdipatchmaster.UpdatedDate = indianTime;
                                                        context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                                        orderMaster.Status = "Delivery Redispatch";
                                                        orderMaster.ReDispatchCount += 1;
                                                        orderMaster.UpdatedDate = indianTime;
                                                        foreach (var item in orderMasterDetailList.Where(c => c.OrderId == orderMaster.OrderId))
                                                        {
                                                            item.UpdatedDate = indianTime;
                                                            item.Status = orderMaster.Status;
                                                            context.Entry(item).State = EntityState.Modified;

                                                        }
                                                        context.Entry(orderMaster).State = EntityState.Modified;
                                                    }
                                                    else if (orderdipatchmaster.IsReAttempt && orderdipatchmaster.ReAttemptCount > 0)
                                                    {
                                                        orderdipatchmaster.Status = "Delivery Redispatch";
                                                        orderdipatchmaster.ReDispatchedStatus = "Delivery Redispatch";
                                                        orderdipatchmaster.ReAttemptCount += 1;
                                                        orderdipatchmaster.IsReAttempt = true;
                                                        orderdipatchmaster.UpdatedDate = indianTime;
                                                        context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                                        orderMaster.Status = "Delivery Redispatch";
                                                        orderMaster.UpdatedDate = indianTime;
                                                        foreach (var item in orderMasterDetailList.Where(c => c.OrderId == orderMaster.OrderId))
                                                        {
                                                            item.UpdatedDate = indianTime;
                                                            item.Status = orderMaster.Status;
                                                            context.Entry(item).State = EntityState.Modified;

                                                        }
                                                        context.Entry(orderMaster).State = EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        orderdipatchmaster.Status = "Ready to Dispatch";
                                                        orderdipatchmaster.ReDispatchedStatus = "Ready to Dispatch";
                                                        orderdipatchmaster.UpdatedDate = indianTime;
                                                        context.Entry(orderdipatchmaster).State = EntityState.Modified;

                                                        orderMaster.Status = "Ready to Dispatch";
                                                        orderMaster.UpdatedDate = indianTime;
                                                        foreach (var item in orderMasterDetailList.Where(c => c.OrderId == orderMaster.OrderId))
                                                        {
                                                            item.UpdatedDate = indianTime;
                                                            item.Status = orderMaster.Status;
                                                            context.Entry(item).State = EntityState.Modified;

                                                        }
                                                        context.Entry(orderMaster).State = EntityState.Modified;
                                                    }

                                                    OrderMasterHistories h1 = new OrderMasterHistories();
                                                    h1.orderid = orderdipatchmaster.OrderId;
                                                    h1.Status = orderdipatchmaster.Status;
                                                    h1.Description = "Due to Assignment Reject from Dapp";
                                                    h1.DeliveryIssuanceId = obj.DeliveryIssuanceId;
                                                    h1.Warehousename = orderdipatchmaster.WarehouseName;
                                                    h1.username = DBoyorders.DisplayName;
                                                    h1.userid = DBoyorders.PeopleID;
                                                    h1.CreatedDate = indianTime;
                                                    AddOrderMasterHistories.Add(h1);

                                                    if (orderMaster != null && orderMaster.OrderType != 5 && orderMaster.OrderType != 3)
                                                    {
                                                        #region stock Hit
                                                        //for Issued to Delivery Redispatched
                                                        if (orderdipatchmaster.ReDispatchCount > 0 && !orderdipatchmaster.IsReAttempt)
                                                        {
                                                            MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                                            List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();
                                                            foreach (var StockHit in orderDispatchMasterDetailList.Where(x => x.qty > 0 && x.OrderId == orderdipatchmaster.OrderId))
                                                            {
                                                                var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                                bool isFree = orderMasterDetailList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                                if (isFree) { RefStockCode = "F"; }
                                                                else if (orderMaster.OrderType == 6) //6 Damage stock
                                                                {
                                                                    RefStockCode = "D";
                                                                }
                                                                else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                                                {
                                                                    RefStockCode = "N";
                                                                }
                                                                OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
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
                                                            if (OnShippedStockEntryList.Any())
                                                            {
                                                                bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_RedispatchOnAssignmentRejectFromIssued", context, scope);
                                                                if (!ress)
                                                                {
                                                                    res = new ResMsgNew()
                                                                    {
                                                                        CheckAssignmentEwayBillAndIRNno = null,
                                                                        Status = false,
                                                                        Message = "Can't Reject Issued, Something went wrong."
                                                                    };
                                                                    // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                                                    return Request.CreateResponse(HttpStatusCode.OK, res); ;
                                                                }
                                                            }
                                                        }
                                                        else if (orderdipatchmaster.IsReAttempt && orderdipatchmaster.ReAttemptCount > 0)
                                                        {
                                                            MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                                            List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();
                                                            foreach (var StockHit in orderDispatchMasterDetailList.Where(x => x.qty > 0 && x.OrderId == orderdipatchmaster.OrderId))
                                                            {
                                                                var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                                bool isFree = orderMasterDetailList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                                if (isFree) { RefStockCode = "F"; }
                                                                else if (orderMaster.OrderType == 6) //6 Damage stock
                                                                {
                                                                    RefStockCode = "D";
                                                                }
                                                                else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                                                {
                                                                    RefStockCode = "N";
                                                                }
                                                                OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
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
                                                            if (OnShippedStockEntryList.Any())
                                                            {
                                                                bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_RedispatchOnAssignmentRejectFromIssued", context, scope);
                                                                if (!ress)
                                                                {
                                                                    res = new ResMsgNew()
                                                                    {
                                                                        CheckAssignmentEwayBillAndIRNno = null,
                                                                        Status = false,
                                                                        Message = "Can't Reject Issued, Something went wrong."
                                                                    };
                                                                    // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                                                    return Request.CreateResponse(HttpStatusCode.OK, res); ;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //for Issued to RTD
                                                            MultiStockHelper<OnShippedRejectEntryDc> MultiStockHelpers = new MultiStockHelper<OnShippedRejectEntryDc>();
                                                            List<OnShippedRejectEntryDc> OnShippedStockEntryList = new List<OnShippedRejectEntryDc>();

                                                            foreach (var StockHit in orderDispatchMasterDetailList.Where(x => x.qty > 0))
                                                            {
                                                                var RefStockCode = orderMaster.OrderType == 8 ? "CL" : "C";
                                                                bool isFree = orderMasterDetailList.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                                if (isFree) { RefStockCode = "F"; }
                                                                else if (orderMaster.OrderType == 6) //6 Damage stock
                                                                {
                                                                    RefStockCode = "D";
                                                                }
                                                                else if (orderMaster.OrderType == 9) //9 Non sellable stock
                                                                {
                                                                    RefStockCode = "N";
                                                                }
                                                                OnShippedStockEntryList.Add(new OnShippedRejectEntryDc
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
                                                            if (OnShippedStockEntryList.Any())
                                                            {
                                                                bool ress = MultiStockHelpers.MakeEntry(OnShippedStockEntryList, "Stock_OnShippedReject", context, scope);
                                                                if (!ress)
                                                                {
                                                                    res = new ResMsgNew()
                                                                    {
                                                                        CheckAssignmentEwayBillAndIRNno = null,
                                                                        Status = false,
                                                                        Message = "Can't Reject Issued, Something went wrong."
                                                                    };
                                                                    // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);

                                                                    return Request.CreateResponse(HttpStatusCode.OK, res); ;
                                                                }
                                                            }
                                                        }
                                                        #endregion
                                                    }
                                                    context.Commit();
                                                }
                                            }
                                        }
                                    }

                                }
                                scope.Complete();
                                try
                                {
                                    BusinessLayer.Managers.OrderMasterManager manager = new BusinessLayer.Managers.OrderMasterManager();
                                    ParallelLoopResult parellelResult = Parallel.ForEach(OrderIds, (x) =>
                                    {
                                        BackgroundTaskManager.Run(() => manager.GetOrderInvoiceHtml(x));
                                    });
                                    if (parellelResult.IsCompleted) ;

                                }
                                catch (Exception ex)
                                {
                                }
                            }
                            else
                            {
                                scope.Dispose();
                                res = new ResMsgNew()
                                {
                                    CheckAssignmentEwayBillAndIRNno = checkAssignmentEwayBillAndIRNno,
                                    Status = true,
                                    Message = "Success"
                                };
                                // AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);
                                return Request.CreateResponse(HttpStatusCode.OK, res);

                            }
                        }
                        res = new ResMsgNew()
                        {
                            CheckAssignmentEwayBillAndIRNno = null,
                            Status = true,
                            Message = "Success."
                        };
                        //  AssignmentInProcess.RemoveAll(x => x == obj.DeliveryIssuanceId);
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
            }
            else
            {
                res = new ResMsgNew()
                {
                    CheckAssignmentEwayBillAndIRNno = null,
                    Status = false,
                    Message = "Something went wrong..",
                };
                return Request.CreateResponse(HttpStatusCode.OK, res); ;
            }

        }
        #region get all Issuances which are active for the delivery boy
        /// <summary>
        /// Created By 07-03-2019
        /// get all Issuances which are active for the delivery boy
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("V2")]
        [HttpGet]
        public HttpResponseMessage GetV2(int id)
        {
            ResListObj res;
            using (var context = new AuthContext())
            {
                try
                {
                    var DBoyorders = context.DeliveryIssuanceDb.Where(x => x.IsActive == true && x.PeopleID == id).Include("details").ToList();
                    if (DBoyorders.Count == 0)
                    {
                        res = new ResListObj()
                        {
                            DI = null,
                            status = false,
                            Message = "Data not exist."
                        };
                    }
                    else
                    {
                        res = new ResListObj()
                        {
                            DI = DBoyorders,
                            status = true,
                            Message = "Success"
                        };
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    res = new ResListObj()
                    {
                        DI = null,
                        status = false,
                        Message = "Failed. " + ex.Message + ""
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion



        #region get by AssignmentId data for delivery boy app 
        /// <summary>
        /// Get ordermaster data and 
        /// Created date 20/04/2019
        /// created By Raj
        /// </summary>
        /// <returns></returns>
        [Route("getAssignid")]
        public HttpResponseMessage Getassid(int AssignmentId, int DBoyId)
        {
            logger.Info("start Delivery Boy api: ");
            ResListObj res;
            using (var context = new AuthContext())
            {
                try
                {
                    var DBoyorders = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == AssignmentId && x.PeopleID == DBoyId).Include("details").ToList();
                    if (DBoyorders.Count == 0)
                    {
                        res = new ResListObj()
                        {
                            DI = null,
                            status = false,
                            Message = "Data not exist."
                        };
                    }
                    else
                    {
                        res = new ResListObj()
                        {
                            DI = DBoyorders,
                            status = true,
                            Message = "Success"
                        };
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, res);

                }
                catch (Exception ex)
                {
                    res = new ResListObj()
                    {
                        DI = null,
                        status = false,
                        Message = "Failed. " + ex.Message + ""
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, res);
                }
            }
        }
        #endregion

        #region get by order Id data for delivery boy app 
        /// <summary>
        /// Get ordermaster data and 
        /// Created date 20/04/2019
        /// Created By Raj
        /// </summary>
        /// <returns></returns>
        [Route("getorderid")]
        public HttpResponseMessage Getorderid(int orderId, int DBoyId)
        {
            logger.Info("start Delivery Boy api: ");
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
                    var DeliveryBoydata = context.Peoples.Where(x => x.PeopleID == DBoyId && x.Active == true).FirstOrDefault(); //get Delivery Boy data 
                    var orderdata = context.OrderDispatchedMasters.Where(x => x.OrderId == orderId && x.DboyMobileNo == DeliveryBoydata.Mobile).FirstOrDefault();


                    if (orderdata != null)
                    {
                        var orderdatadate = context.OrderDeliveryMasterDB.Where(x => x.DeliveryIssuanceId == orderdata.DeliveryIssuanceIdOrderDeliveryMaster && x.DboyMobileNo == DeliveryBoydata.Mobile).ToList();
                        var ordercount = orderdatadate.Count();//calculate  order count this assignment id based
                        var oddetailcount = context.DbOrderDetails.Where(x => x.OrderId == orderdata.OrderId).Count();
                        orderdataassignmentDTO orderdetailsorderid = new orderdataassignmentDTO();

                        orderdetailsorderid.OrderId = orderdata.OrderId;
                        orderdetailsorderid.OddetailCount = oddetailcount;
                        orderdetailsorderid.CustomerName = orderdata.CustomerName;
                        orderdetailsorderid.DeliveryIssuanceId = orderdata.DeliveryIssuanceIdOrderDeliveryMaster;
                        orderdetailsorderid.ShopName = orderdata.ShopName;
                        orderdetailsorderid.Status = orderdata.Status;
                        orderdetailsorderid.orderCount = ordercount;
                        orderdetailsorderid.CreatedDate = orderdatadate[0].CreatedDate;
                        orderdetailsorderid.Customerphonenum = orderdata.Customerphonenum;
                        orderdetailsorderid.TotalAmount = orderdata.TotalAmount;
                        orderdetailsorderid.ShippingAddress = orderdata.ShippingAddress;


                        orderidserachBased orderidbaseddata = new orderidserachBased()
                        {
                            orderdata = orderdetailsorderid,
                            status = true,
                            Message = "success."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, orderidbaseddata);
                    }
                    else
                    {
                        orderidserachBased orderidbaseddata = new orderidserachBased()
                        {
                            orderdata = null,
                            status = false,
                            Message = "Data not exist."
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, orderidbaseddata);


                    }




                }
                catch (Exception ex)
                {
                    orderidserachBased orderidbaseddata = new orderidserachBased()
                    {
                        orderdata = null,
                        status = false,
                        Message = "Something went wrong ."
                    };

                    return Request.CreateResponse(HttpStatusCode.BadRequest, orderidbaseddata);



                }
            }
        }
        #endregion


        #region  for new assignment Backend Api  create Assignment By Harry
        [Route("CreateAssignment/V1")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage CreateAssignment(DeliveryIssuance obj)//add issuance
        {
            TransactionOptions option = new TransactionOptions();
            option.IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead;
            option.Timeout = TimeSpan.FromSeconds(120);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, option))
            {
                DeliveryIssuance newDeliveryIssuance = null;
                DeliveryIssuance AddDeliveryIssuance = new DeliveryIssuance();
                string errorMessage = "";
                using (var context = new AuthContext())
                {

                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0, Warehouseid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid"))
                        Warehouseid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value);
                    var peopledata = context.Peoples.Where(x => x.PeopleID == userid && x.Deleted == false).FirstOrDefault(); //userdata
                    if (peopledata != null)
                    {
                        if (!peopledata.Active)
                        {
                            errorMessage = "Inactive user can't  create Assignment";
                            return Request.CreateResponse(HttpStatusCode.OK, errorMessage); ;
                        }

                        obj.userid = userid;
                        obj.CompanyId = compid;
                        if (Warehouseid == 0)
                        {
                            obj.WarehouseId = obj.WarehouseId;
                        }
                        else
                        {
                            obj.WarehouseId = Warehouseid;
                        }
                        obj.CreatedDate = indianTime;
                        obj.UpdatedDate = indianTime;
                        obj.OrderdispatchIds = "";
                        obj.OrderIds = "";
                        obj.AssignedOrders[0].userid = userid;
                        double TotalAssignmentAmount = 0;

                        var orderIds = obj.AssignedOrders.Select(x => x.OrderId).Distinct().ToList();
                        var orderLists = context.OrderDispatchedMasters.Where(c => orderIds.Contains(c.OrderId) && (c.Status == "Ready to Dispatch" || c.Status == "Delivery Redispatch")).ToList();

                        List<OrderMasterHistories> addOrderMasterHistories = new List<OrderMasterHistories>();
                        List<OrderDeliveryMaster> addOrderDeliveryMasterlist = new List<OrderDeliveryMaster>();

                        var DBoypeople = context.Peoples.Where(x => x.PeopleID == obj.PeopleID).FirstOrDefault();

                        AddDeliveryIssuance = obj;
                        List<OrderDispatchedMaster> orderDispatchedMasterList = new List<OrderDispatchedMaster>();
                        foreach (var o in obj.AssignedOrders.Distinct())
                        {
                            var DispatchedOder = orderLists.Where(x => x.OrderId == o.OrderId && x.DboyMobileNo == DBoypeople.Mobile && (x.Status == "Ready to Dispatch" || x.Status == "Delivery Redispatch")).FirstOrDefault();//

                            if (DispatchedOder != null)
                            {
                                if (obj.OrderdispatchIds == "" && obj.OrderIds == "")
                                {
                                    obj.OrderdispatchIds = Convert.ToString(o.OrderDispatchedMasterId);
                                    obj.OrderIds = Convert.ToString(o.OrderId);
                                    orderDispatchedMasterList.Add(o);
                                }
                                else
                                {
                                    obj.OrderdispatchIds = obj.OrderdispatchIds + "," + Convert.ToString(o.OrderDispatchedMasterId);
                                    obj.OrderIds = obj.OrderIds + "," + Convert.ToString(o.OrderId);
                                    orderDispatchedMasterList.Add(o);
                                }
                                TotalAssignmentAmount = TotalAssignmentAmount + DispatchedOder.GrossAmount;
                            }
                        }
                        AddDeliveryIssuance.AssignedOrders = orderDispatchedMasterList;
                        var OrderIds = orderDispatchedMasterList.Select(x => x.OrderId).Distinct().ToList();
                        var OrderMasterlist = context.DbOrderMaster.Where(x => OrderIds.Contains(x.OrderId)).ToList();
                        var OrderDetaillist = context.DbOrderDetails.Where(x => OrderIds.Contains(x.OrderId)).ToList();

                        var paymentlists = context.PaymentResponseRetailerAppDb.Where(x => OrderIds.Contains(x.OrderId) && x.status == "success" && x.PaymentFrom.ToLower() == "epaylater");

                        if (!string.IsNullOrEmpty(obj.OrderdispatchIds))
                        {
                            obj.Status = "SavedAsDraft";
                            obj.TotalAssignmentAmount = TotalAssignmentAmount;
                            obj.IsActive = true;

                            var ProcOrderIds = orderDispatchedMasterList.Select(x => x.OrderId).Distinct().ToList();
                            DataTable dt = new DataTable();
                            dt.Columns.Add("IntValue");
                            foreach (var item in ProcOrderIds)
                            {
                                var dr = dt.NewRow();
                                dr["IntValue"] = item;
                                dt.Rows.Add(dr);
                            }
                            var param = new SqlParameter("orderids", dt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.IntValues";
                            var OrderDispatchedDetailssList = context.Database.SqlQuery<OrderDispatchedDetailsDC>("Exec GetItemToCreatePickList @orderids", param).ToList();
                            obj.details = new List<IssuanceDetails>();
                            obj.details = OrderDispatchedDetailssList
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
                                                 }).ToList();

                            var DBoyorders = context.DeliveryIssuanceDb.Add(obj);
                            context.Commit();
                            #region  DeliveryHistory
                            OrderDeliveryMasterHistories AssginDeli = new OrderDeliveryMasterHistories();
                            AssginDeli.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                            //AssginDeli.OrderId = delivery.o
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
                            AssginDeli.userid = userid;
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

                            if (DBoyorders.DeliveryIssuanceId > 0)
                            {
                                var redispatchedcharge = new List<DeliveryRedispatchChargeConfDc>();
                                //for DeliveryRedispatched Charge
                                if (AddDeliveryIssuance.AssignedOrders != null && AddDeliveryIssuance.AssignedOrders.Any(x => x.ReDispatchCount > 0))
                                {
                                    var CityId = AddDeliveryIssuance.AssignedOrders.Select(x => x.CityId).FirstOrDefault();
                                    var result = context.DeliveryRedispatchChargeConfs.Where(x => x.IsActive == true && x.IsDeleted == false && x.CityId == CityId).ToList();
                                    redispatchedcharge = Mapper.Map(result).ToANew<List<DeliveryRedispatchChargeConfDc>>();
                                }
                                AddDeliveryIssuance.AssignedOrders[0].DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                                foreach (var od in AddDeliveryIssuance.AssignedOrders)
                                {

                                    foreach (var OrderDMaster in orderLists.Where(x => x.OrderDispatchedMasterId == od.OrderDispatchedMasterId))
                                    {
                                        OrderDMaster.Status = "Issued";
                                        OrderDMaster.ReDispatchedStatus = "Issued";
                                        OrderDMaster.UpdatedDate = indianTime;
                                        OrderDMaster.DeliveryIssuanceIdOrderDeliveryMaster = DBoyorders.DeliveryIssuanceId;
                                        foreach (var OrderMaster in OrderMasterlist.Where(z => z.OrderId == OrderDMaster.OrderId).ToList())
                                        {
                                            OrderMaster.Status = "Issued";
                                            OrderMaster.UpdatedDate = indianTime;
                                            //  for DelievryRedispatchedAmount
                                            if (OrderDMaster.ReDispatchCount > 0 && redispatchedcharge != null && redispatchedcharge.Any() && redispatchedcharge.Any(s => s.RedispatchCount == OrderDMaster.ReDispatchCount))
                                            {
                                                double RedispatchedAmount = 0;
                                                RedispatchedAmount = redispatchedcharge.FirstOrDefault(s => s.RedispatchCount == OrderDMaster.ReDispatchCount).RedispatchCharge;

                                                if (RedispatchedAmount > 0)
                                                {
                                                    OrderMaster.deliveryCharge += RedispatchedAmount;
                                                    OrderMaster.GrossAmount += RedispatchedAmount;
                                                    OrderMaster.TotalAmount += RedispatchedAmount;

                                                    OrderDMaster.deliveryCharge += RedispatchedAmount;
                                                    OrderDMaster.GrossAmount += RedispatchedAmount;
                                                    OrderDMaster.TotalAmount += RedispatchedAmount;


                                                    var cashOldEntries = context.PaymentResponseRetailerAppDb.Where(z => z.OrderId == OrderDMaster.OrderId && z.PaymentFrom == "Cash" && z.status == "Success").ToList();
                                                    double cashAmount = 0;
                                                    if (cashOldEntries != null && cashOldEntries.Any())
                                                    {
                                                        foreach (var cash in cashOldEntries)
                                                        {
                                                            cashAmount += cash.amount;
                                                            cash.status = "Failed";
                                                            cash.statusDesc = "Due to delivery redispatched Charge (added)" + RedispatchedAmount;
                                                            context.Entry(cash).State = EntityState.Modified;
                                                        }

                                                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                                        {
                                                            amount = cashAmount + RedispatchedAmount,
                                                            CreatedDate = DateTime.Now,
                                                            currencyCode = "INR",
                                                            OrderId = OrderDMaster.OrderId,
                                                            PaymentFrom = "Cash",
                                                            status = "Success",
                                                            UpdatedDate = DateTime.Now,
                                                            IsRefund = false,
                                                            statusDesc = "Due to delivery redispatched Charge (added)" + RedispatchedAmount
                                                        };
                                                        context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                                    }
                                                    else
                                                    {
                                                        var PaymentResponseRetailerAppDb = new PaymentResponseRetailerApp
                                                        {
                                                            amount = RedispatchedAmount,
                                                            CreatedDate = DateTime.Now,
                                                            currencyCode = "INR",
                                                            OrderId = OrderDMaster.OrderId,
                                                            PaymentFrom = "Cash",
                                                            status = "Success",
                                                            UpdatedDate = DateTime.Now,
                                                            IsRefund = false,
                                                            statusDesc = "Due to delivery redispatched Charge (added)" + RedispatchedAmount
                                                        };
                                                        context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDb);
                                                    }
                                                }
                                            }

                                            context.Entry(OrderDMaster).State = EntityState.Modified;
                                            context.Entry(OrderMaster).State = EntityState.Modified;
                                            foreach (var item in OrderDetaillist.Where(e => e.OrderId == OrderDMaster.OrderId).ToList())
                                            {
                                                item.UpdatedDate = indianTime;
                                                item.Status = "Issued";
                                                context.Entry(item).State = EntityState.Modified;
                                            }


                                            #region Code For OrderDeliveryMaster

                                            var payments = paymentlists.Where(x => x.OrderId == OrderDMaster.OrderId && x.status == "success" && x.PaymentFrom.ToLower() == "epaylater");
                                            double? epaylateramt = payments.Any() ? payments.Sum(x => x.amount) : 0;

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
                                            oDm.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                                            oDm.DiscountAmount = OrderDMaster.DiscountAmount;
                                            oDm.CheckNo = OrderDMaster.CheckNo;
                                            oDm.CheckAmount = OrderDMaster.CheckAmount;
                                            oDm.ElectronicPaymentNo = OrderDMaster.ElectronicPaymentNo;
                                            oDm.ElectronicAmount = OrderDMaster.ElectronicAmount;
                                            oDm.EpayLaterAmount = epaylateramt;
                                            oDm.CashAmount = OrderDMaster.CashAmount;
                                            oDm.OrderedDate = OrderDMaster.OrderedDate;
                                            oDm.WalletAmount = OrderDMaster.WalletAmount;
                                            oDm.RewardPoint = OrderDMaster.RewardPoint;
                                            oDm.Tin_No = OrderDMaster.Tin_No;
                                            oDm.ReDispatchCount = OrderDMaster.ReDispatchCount;
                                            oDm.UpdatedDate = indianTime;
                                            oDm.CreatedDate = indianTime;
                                            addOrderDeliveryMasterlist.Add(oDm);
                                            #endregion

                                            #region Order Master History
                                            OrderMasterHistories hh1 = new OrderMasterHistories();
                                            if (OrderDMaster != null)
                                            {
                                                hh1.orderid = OrderDMaster.OrderId;
                                                hh1.Status = OrderDMaster.Status;
                                                hh1.Reasoncancel = null;

                                                hh1.Warehousename = OrderDMaster.WarehouseName;
                                                if (peopledata.DisplayName == null || peopledata.DisplayName == "")
                                                {
                                                    hh1.username = peopledata.PeopleFirstName;
                                                    hh1.Description = " (Issued AssignmentId : " + DBoyorders.DeliveryIssuanceId + ") By" + peopledata.PeopleFirstName;
                                                }
                                                else
                                                {
                                                    hh1.username = peopledata.DisplayName;
                                                    hh1.Description = " (Issued AssignmentId : " + DBoyorders.DeliveryIssuanceId + ") By" + peopledata.DisplayName;
                                                }
                                                hh1.DeliveryIssuanceId = DBoyorders.DeliveryIssuanceId;
                                                hh1.userid = userid;
                                                hh1.CreatedDate = indianTime;
                                                addOrderMasterHistories.Add(hh1);

                                            }

                                            #endregion

                                            //for Issued

                                            MultiStockHelper<OnIssuedStockEntryDc> MultiStockHelpers = new MultiStockHelper<OnIssuedStockEntryDc>();
                                            List<OnIssuedStockEntryDc> OnIssuedStockEntryList = new List<OnIssuedStockEntryDc>();
                                            foreach (var StockHit in OrderDispatchedDetailssList.Where(x => x.qty > 0 && x.OrderId == OrderDMaster.OrderId))
                                            {
                                                var RefStockCode = OrderMaster.OrderType == 8 ? "CL" : "C";
                                                bool isFree = OrderDetaillist.Any(c => c.OrderDetailsId == StockHit.OrderDetailsId && c.IsFreeItem && c.IsDispatchedFreeStock);
                                                if (isFree) { RefStockCode = "F"; }
                                                else if (OrderMaster.OrderType == 6) //6 Damage stock
                                                {
                                                    RefStockCode = "D";
                                                }
                                                else if (OrderMaster.OrderType == 9) //9 Non Sellable Stock
                                                {
                                                    RefStockCode = "N";
                                                }
                                                bool IsDeliveryRedispatch = false;
                                                if (OrderMaster.ReDispatchCount > 0)
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
                                            if (OnIssuedStockEntryList.Any())
                                            {

                                                bool res = MultiStockHelpers.MakeEntry(OnIssuedStockEntryList, "Stock_OnIssued", context, scope);
                                                if (!res)
                                                {
                                                    var result = "Can't Dispatched, Something went wrong";
                                                    return Request.CreateResponse(HttpStatusCode.OK, result); ;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (addOrderMasterHistories != null && addOrderMasterHistories.Any()) { context.OrderMasterHistoriesDB.AddRange(addOrderMasterHistories); }
                                if (addOrderDeliveryMasterlist != null && addOrderDeliveryMasterlist.Any()) { context.OrderDeliveryMasterDB.AddRange(addOrderDeliveryMasterlist); }
                                context.Commit();
                                string Borderid = Convert.ToString(DBoyorders.DeliveryIssuanceId);
                                string BorderCodeId = Borderid.PadLeft(9, '0');
                                temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                                DBoyorders.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode
                                newDeliveryIssuance = DBoyorders;
                            }
                        }
                        else
                        {
                            logger.Error("Error Please refesh page then create Assignment by User" + obj.AssignedOrders[0].userid);
                            errorMessage = "Please refesh page then create Assignment";
                            scope.Dispose();
                            return Request.CreateResponse(HttpStatusCode.OK, errorMessage); ;
                        }
                    }
                    else
                    {
                        errorMessage = "Please refesh page then create Assignment";
                        scope.Dispose();
                        return Request.CreateResponse(HttpStatusCode.OK, errorMessage);
                    }
                }
                scope.Complete();
                return Request.CreateResponse(HttpStatusCode.OK, newDeliveryIssuance);

            }
        }
        #endregion

        #region get OrderDeliveryHistory In Assignment Process
        /// <summary>
        /// AssignmentProcess History
        /// By Ashwin
        /// </summary>
        /// <param name="DeliveryIssuanceId"></param>
        /// <returns></returns>
        [Route("DeliveryIssuanceHistory")]
        [HttpGet]
        public dynamic Issuancehistory(int DeliveryIssuanceId)
        {

            try
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0, Warehouse_id = 0;
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
                    if (claim.Type == "Warehouseid")
                    {
                        Warehouse_id = int.Parse(claim.Value);
                    }
                }
                int CompanyId = compid;
                using (AuthContext db = new AuthContext())
                {
                    var data = db.OrderDeliveryMasterHistoriesDB.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId).OrderBy(x => x.id).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
        //New  Delivery app with New Assignment Proccessing System from 15May2019

        [Route("PendingAssignment/V1")]
        [HttpGet]
        public HttpResponseMessage GetAllAssignedAssignment(int id)//get all Get All AssignmentAssigned which are active for the delivery boy & available for Accept
        {
            DeliveryIssuanceDTOApp res;
            using (var context = new AuthContext())
            {
                try
                {
                    List<DeliveryIssuance> DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.IsActive == true && x.Status == "Assigned" && x.PeopleID == id && x.TripPlannerConfirmedMasterId == 0).Include("details").ToList(); //x.TripPlannerConfirmedMasterId==0  mean trip not included


                    if (DeliveryIssuance.Count > 0)
                    {
                        foreach (var Sta in DeliveryIssuance)
                        {

                            var war = context.Warehouses.Where(z => z.active && z.Deleted == false && z.WarehouseId == Sta.WarehouseId).FirstOrDefault();

                            Sta.latitude = war?.latitude;
                            Sta.longitude = war?.longitude;

                            List<ShortItemAssignment> ShortItemAssignment = context.ShortItemAssignmentDB.Where(x => x.DeliveryIssuanceId == Sta.DeliveryIssuanceId).ToList();

                            if (ShortItemAssignment.Count > 0)
                            {
                                Sta.ShortItemAssignment = ShortItemAssignment;

                            }
                        }

                        res = new DeliveryIssuanceDTOApp()
                        {
                            DeliveryIssuance = DeliveryIssuance,
                            status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {

                        res = new DeliveryIssuanceDTOApp()
                        {
                            DeliveryIssuance = null,
                            status = false,
                            Message = "No Record found."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }
                }
                catch (Exception ex)
                {
                    res = new DeliveryIssuanceDTOApp()
                    {
                        DeliveryIssuance = null,
                        status = false,
                        Message = "There is Something wrong"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }


        // get Assigment List for Status Submitted Dapp By Harry Dropdown 
        [Route("SubmittedAssignment/V1")]
        [HttpGet]
        public HttpResponseMessage SubmittedAssignment(int id)//get Assigment List for Status Submitted Dapp By Harry
        {
            DeliveryIssuanceDTOApp res;
            using (var context = new AuthContext())
            {
                try
                {
                    List<DeliveryIssuance> DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.Status == "Payment Accepted" && x.PeopleID == id).Include("details").ToList();
                    if (DeliveryIssuance.Count > 0)
                    {
                        res = new DeliveryIssuanceDTOApp()
                        {
                            DeliveryIssuance = DeliveryIssuance,
                            status = true,
                            Message = "Success."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else
                    {

                        res = new DeliveryIssuanceDTOApp()
                        {
                            DeliveryIssuance = null,
                            status = false,
                            Message = "No Record found."
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }
                }
                catch (Exception ex)
                {
                    res = new DeliveryIssuanceDTOApp()
                    {
                        DeliveryIssuance = null,
                        status = false,
                        Message = "There is Something wrong"
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
        }



        #region get all Accpetd and inprogress order  which are active for the delivery boy
        /// <summary>
        /// Created By 27-06-2019
        /// get all Accpetd and inprogress order  which are active for the delivery boy
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("AcceptedInprogress/V1")]
        [HttpGet]
        public HttpResponseMessage GetAccpetdInprogressV1(int id)
        {
            ResListAcceptPending res;

            try
            {
                using (AuthContext authContext = new AuthContext())
                {
                    DateTime StartOf = new DateTime(2019, 06, 26);
                    var data = (from a in authContext.DeliveryIssuanceDb
                                where (a.IsActive == false && a.PeopleID == id && (a.Status == "Accepted" || a.Status == "Pending") && a.CreatedDate > StartOf)
                                join i in authContext.DeliveryEclipseTimeDB
                                on a.DeliveryIssuanceId equals i.DeliveryIssuanceId into ps
                                from i in ps.DefaultIfEmpty()
                                select new AssignmentAcceptPendingDTO
                                {
                                    DeliveryIssuanceId = a.DeliveryIssuanceId,
                                    AssignmentDate = a.CreatedDate,
                                    StartDateTime = i.StartDateTime,
                                    Start = i.Start,
                                    EndDateTime = i.EndDateTime,
                                    End = i.End
                                }).Where(i => !i.End.HasValue || i.End.Value == false).ToList();

                    if (data.Count == 0)
                    {
                        res = new ResListAcceptPending()
                        {
                            AssignmentAcceptPending = null,
                            status = false,
                            Message = "Data not exist."
                        };
                    }
                    else
                    {
                        MongoDbHelper<AssignmentDirection> mongoDbHelper = new MongoDbHelper<AssignmentDirection>();
                        var assignmentIds = data.Select(x => x.DeliveryIssuanceId).ToList();
                        var predicate = PredicateBuilder.New<AssignmentDirection>(x => assignmentIds.Contains(x.AssignmentId));
                        var assignmentDirection = mongoDbHelper.Select(predicate).ToList();
                        foreach (var item in data)
                        {
                            if (assignmentDirection.Any(x => x.AssignmentId == item.DeliveryIssuanceId))
                            {
                                var assignDir = assignmentDirection.FirstOrDefault(x => x.AssignmentId == item.DeliveryIssuanceId);
                                item.AssignmentDistance = assignDir.AssignmentDistance;
                                item.AssignmentDuration = assignDir.AssignmentDuration;
                                item.ReturnDistance = assignDir.ReturnDistance;
                                item.ReturnDuration = assignDir.ReturnDuration;
                                item.TotalUnloadingDuration = assignDir.TotalUnloadingDuration;
                                item.IsDirectionExist = true;
                            }
                        }
                        res = new ResListAcceptPending()
                        {
                            AssignmentAcceptPending = data,
                            status = true,
                            Message = "Success"
                        };

                    }

                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }

            }
            catch (Exception ex)
            {
                res = new ResListAcceptPending()
                {
                    AssignmentAcceptPending = null,
                    status = false,
                    Message = "Failed. " + ex.Message + ""
                };
                return Request.CreateResponse(HttpStatusCode.BadRequest, res);
            }
        }
        #endregion

        #region InActivePeople Delivery App Profile
        [Route("DeliveryAppProfile")]
        [HttpGet]
        [AllowAnonymous]
        public bool DeliveryAppProfile(string Mobile, int AppType)
        {
            bool activeStatus = false;
            People person = new People();
            if (Mobile != null)
            {
                using (var db = new AuthContext())
                {
                    string AppName = "";
                    if (AppType == 0)
                    {
                        AppName = "DeliveryApp";
                    }
                    else if (AppType == 1)
                    {
                        AppName = "New Delivery App";
                    }
                    person = db.Peoples.FirstOrDefault(u => u.Mobile == Mobile && u.Active && !u.Deleted);
                    if (person != null)
                    {
                        AppVisits appVisit = new AppVisits();
                        MongoDbHelper<AppVisits> mongoDbHelper = new MongoDbHelper<AppVisits>();
                        appVisit.UserName = Mobile;
                        appVisit.UserId = person.PeopleID;
                        appVisit.AppType = AppName;
                        appVisit.VisitedOn = DateTime.Now;
                        var Status = mongoDbHelper.InsertAsync(appVisit);

                        return person.Active;
                    }
                    else
                    {
                        return activeStatus;
                    }
                }
            }

            return activeStatus;
        }

        #endregion

        #region send for approval Redispatch order Id
        /// <summary>
        /// Created Date:09/10/2019
        /// Created by Raj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("approvalorder")]
        [HttpPost]
        public HttpResponseMessage redispatchorderapproval(OrderRedispatchCountApproval obj)//add issuance
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            using (AuthContext context = new AuthContext())
            {
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                try
                {

                    OrderRedispatchCountApproval orderredata = new OrderRedispatchCountApproval();
                    orderredata.OrderId = obj.OrderId;
                    orderredata.CreatedDate = indianTime;
                    orderredata.UpdateDate = indianTime;
                    orderredata.IsApproved = false;
                    orderredata.userId = userid;
                    orderredata.Redispatchcount = obj.Redispatchcount;
                    context.orderRedispatchCountApprovalDB.Add(orderredata);
                    context.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, orderredata);

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, obj);

                }
            }
        }
        #endregion
        #region get Order Redispatch Count Approval data
        /// <summary>
        /// Created By Raj
        /// Created Date:09/10/2019
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("GetRedispatchdata")]
        [HttpGet]
        public HttpResponseMessage Redisaptchdataget()
        {
            List<OrderRedispatchCountApproval> orderredispatchcountapproval = null;
            using (var context = new AuthContext())
            {
                try
                {
                    orderredispatchcountapproval = context.orderRedispatchCountApprovalDB.Where(x => x.IsApproved == false).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, orderredispatchcountapproval);

                }
                catch (Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.BadRequest, orderredispatchcountapproval);
                }
            }
        }
        #endregion
        #region approval Redispatch order Id
        /// <summary>
        /// Created Date:09/10/2019
        /// Created by Raj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Route("approvedredispatch")]
        [HttpPut]
        public HttpResponseMessage approvedredispatch(OrderRedispatchCountApproval obj)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            using (AuthContext context = new AuthContext())
            {
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                try
                {

                    OrderRedispatchCountApproval orderredata = context.orderRedispatchCountApprovalDB.Where(x => x.Id == obj.Id).FirstOrDefault(); ;
                    orderredata.UpdateDate = indianTime;
                    orderredata.IsApproved = true;
                    //orderredata.userId = userid;
                    context.Entry(orderredata).State = EntityState.Modified;
                    context.Commit();
                    return Request.CreateResponse(HttpStatusCode.OK, orderredata);

                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, obj);

                }
            }
        }
        #endregion


        #region search assignment by ids

        [Route("SearchAssignment")]
        [HttpGet]
        public PaggingData_Delivery SearchAssignmentbyId(string AssignmentIds, string OrderId)//get all Issuances which b/w dates the delivery boy
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                List<int> Warehouseids = new List<int>();
                List<string> rolenames = null;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseids") && identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value != "")
                    Warehouseids = (identity.Claims.FirstOrDefault(x => x.Type == "Warehouseids").Value).Split(',').Select(x => Convert.ToInt32(x)).ToList();

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "Warehouseid") && identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value != "")
                    Warehouseids.Add(Convert.ToInt32((identity.Claims.FirstOrDefault(x => x.Type == "Warehouseid").Value)));

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "RoleNames"))
                    rolenames = (identity.Claims.FirstOrDefault(x => x.Type == "RoleNames").Value).Split(',').Select(x => x).ToList();

                var predicate = PredicateBuilder.True<DeliveryIssuance>();
                //predicate = predicate.And(x => x.IsActive);


                if (!string.IsNullOrEmpty(AssignmentIds))
                {
                    if (AssignmentIds.Length > 1)
                    {
                        int[] ids = Array.ConvertAll(AssignmentIds.Split(','), int.Parse);

                        if (ids != null && ids.Any())
                            predicate = predicate.And(x => ids.Contains(x.DeliveryIssuanceId));
                    }
                }

                if (!string.IsNullOrEmpty(OrderId))
                    predicate = predicate.And(x => x.OrderIds.Contains(OrderId));

                if (Warehouseids != null && Warehouseids.Any() && rolenames.Contains("Hub Cashier"))
                    predicate = predicate.And(x => x.WarehouseId.HasValue && Warehouseids.Contains(x.WarehouseId.Value));

                List<DeliveryIssuance> newdata = new List<DeliveryIssuance>();
                var listOrders = context.DeliveryIssuanceDb.Where(predicate).OrderByDescending(x => x.CreatedDate).Include("details").ToList();

                var WarehouseList = context.Warehouses.Where(x => Warehouseids.Contains(x.WarehouseId)).ToList();
                newdata = listOrders;
                if (newdata != null)
                {
                    foreach (var Issuance in newdata)
                    {
                        try
                        {
                            Issuance.IsStore = WarehouseList != null ? WarehouseList.Where(x => x.WarehouseId == Issuance.WarehouseId).Select(y => y.IsStore).FirstOrDefault() : false;
                            string Borderid = Convert.ToString(Issuance.DeliveryIssuanceId);
                            string BorderCodeId = Borderid.PadLeft(9, '0');
                            temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                            Issuance.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode

                        }
                        catch (Exception es) { }
                    }
                }
                PaggingData_Delivery obj = new PaggingData_Delivery();
                obj.total_count = listOrders.Count();
                obj.historyamount = newdata;
                return obj;
            }
        }
        #endregion


        public bool MergePDFs(List<TradeInvoiceDc> files, string targetPdf)
        {

            bool mergedPdf = false;
            using (FileStream stream = new FileStream(targetPdf, FileMode.Create, FileAccess.Write))
            {
                Document document = new Document();
                PdfCopy pdf = new PdfCopy(document, stream);
                PdfReader reader = null;
                try
                {
                    document.Open();
                    foreach (var file in files)
                    {
                        try
                        {
                            reader = new PdfReader(file.FileName);
                        }
                        catch (Exception d)
                        {
                            if (reader == null)
                            {
                                GetTradeInvoiceHelper helper = new GetTradeInvoiceHelper();
                                bool allInvoiceCreated = helper.GetAndGenearteTradeInvoice(files);

                                reader = new PdfReader(file.FileName);
                            }
                        }
                        pdf.AddDocument(reader);
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    //LogHelper.LogError(new StringBuilder("Error while Merging Pdf: ").Append(ex.ToString()).ToString(), true);
                    // merged = false;
                    if (reader != null)
                        reader.Close();
                }
                finally
                {
                    if (document != null)
                        document.Close();
                }
                mergedPdf = true;
            }
            return mergedPdf;


        }


        [Route("AssignmentProductsList")]
        [HttpGet]
        public List<AssignmentDispatchedItemsDc> AssignmentProductsList(int AssignmentId)
        {
            var result = new List<AssignmentDispatchedItemsDc>();
            if (AssignmentId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    //old AssignmentProductsList
                    var DispatchedLineItems = context.Database.SqlQuery<AssignmentDispatchedItemsDc>("EXEC BatchCode.AssignmentBatchProductsList " + AssignmentId).ToList();

                    //Newly
                    result.AddRange(DispatchedLineItems.Where(x => x.ReDispatchCount == 0 && x.IsReAttempt == false).GroupBy(x => new { x.ItemMultiMRPId, x.IsFreeItem, x.BatchCode, x.isClearance }).Select(t =>
                          new AssignmentDispatchedItemsDc
                          {
                              orderIds = string.Join(",", DispatchedLineItems.Where(i => i.ItemMultiMRPId == t.Key.ItemMultiMRPId && i.BatchCode == t.Key.BatchCode && i.isClearance == t.Key.isClearance && i.IsFreeItem == t.Key.IsFreeItem && i.ReDispatchCount == 0 && i.IsReAttempt == false).Select(x => x.OrderId).ToArray()),
                              qty = t.Sum(x => x.qty),
                              ItemMultiMRPId = t.Key.ItemMultiMRPId,
                              BatchCode = t.Key.BatchCode,
                              SellingUnitName = t.Min(x => x.SellingUnitName),
                              IsReDispatched = false,
                              PurchaseMinOrderQty = t.Min(x => x.PurchaseMinOrderQty),
                              UnitPrice = t.Min(x => x.UnitPrice),
                              IsFreeItem = t.Key.IsFreeItem,
                              isClearance = t.Key.isClearance
                          }).OrderBy(x => x.SellingUnitName).ToList());

                    //Redispatched
                    result.AddRange(DispatchedLineItems.Where(x => (x.ReDispatchCount > 0 || x.IsReAttempt == true)).GroupBy(x => new { x.ItemMultiMRPId, x.IsFreeItem, x.BatchCode, x.isClearance }).Select(t =>
                         new AssignmentDispatchedItemsDc
                         {
                             orderIds = string.Join(",", DispatchedLineItems.Where(i => i.ItemMultiMRPId == t.Key.ItemMultiMRPId && i.BatchCode == t.Key.BatchCode && i.isClearance == t.Key.isClearance && i.IsFreeItem == t.Key.IsFreeItem && (i.ReDispatchCount > 0 || i.IsReAttempt == true)).Select(i => i.OrderId).ToArray()),
                             qty = t.Sum(x => x.qty),
                             ItemMultiMRPId = t.Key.ItemMultiMRPId,
                             BatchCode = t.Key.BatchCode,
                             SellingUnitName = t.Min(x => x.SellingUnitName),
                             IsReDispatched = true,
                             PurchaseMinOrderQty = t.Min(x => x.PurchaseMinOrderQty),
                             UnitPrice = t.Min(x => x.UnitPrice),
                             IsFreeItem = t.Key.IsFreeItem,
                             isClearance = t.Key.isClearance
                         }).OrderBy(x => x.SellingUnitName).ToList());
                }
            }
            return result;
        }

        [Route("TripAssignmentProductsList")]
        [HttpGet]
        public TripDeliveryIssuanceDC TripAssignmentProductsList(int TripPlannerMasterId)
        {
            List<orderamount> list = new List<orderamount>();
            var result = new TripDeliveryIssuanceDC();
            if (TripPlannerMasterId > 0)
            {
                using (AuthContext context = new AuthContext())
                {
                    long tripPlannerConfirmedMasterId = context.TripPlannerConfirmedMasters.FirstOrDefault(x => x.TripPlannerMasterId == TripPlannerMasterId).Id;
                    List<TripBlockedOrderVM> blockedOrderList = null;
                    TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                    blockedOrderList = tripPlannerHelper.GetBlockedOrderList(tripPlannerConfirmedMasterId, context);
                    if (blockedOrderList != null && blockedOrderList.Any())
                    {
                        return new TripDeliveryIssuanceDC
                        {
                            IsSuccess = false,
                            AssignmentBarcodeImage = null,
                            CreatedDate = DateTime.Now,
                            DboyName = "",
                            DeliveryIssuanceId = "",
                            Itemlist = null,
                            ReDispatchItemlist = null,
                            TotalAssignmentAmount = null,
                            WarehouseName = "",
                            BlockedOrderList = blockedOrderList
                        };
                    }

                    var tripPlannerConfirmedMasters = context.TripPlannerConfirmedMasters.Where(x => x.TripPlannerMasterId == TripPlannerMasterId && x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).FirstOrDefault();
                    var deliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasters).ToList();
                    var deliveryIssuanceIds = deliveryIssuance.Select(x => x.DeliveryIssuanceId).ToList();
                    var warehoueIds = deliveryIssuance.Select(c => c.WarehouseId).ToList();
                    var warehouseName = context.Warehouses.Where(x => warehoueIds.Contains(x.WarehouseId)).Select(x => x.WarehouseName).FirstOrDefault();
                    var dIdDtw = new DataTable();
                    dIdDtw.Columns.Add("IntValue");

                    double? amount = 0;
                    foreach (var item in deliveryIssuance)
                    {
                        var dr = dIdDtw.NewRow();
                        dr["IntValue"] = item.DeliveryIssuanceId;
                        dIdDtw.Rows.Add(dr);
                        amount += item.TotalAssignmentAmount;
                    }
                    result.DeliveryIssuanceId = string.Join(",", deliveryIssuanceIds);
                    result.TotalAssignmentAmount = amount;
                    result.WarehouseName = warehouseName;
                    result.CreatedDate = deliveryIssuance[0].CreatedDate;
                    result.DboyName = deliveryIssuance[0].DisplayName;
                    result.IsSuccess = true;
                    var IssuanceId = new SqlParameter
                    {
                        ParameterName = "AssignmentId",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IntValues",
                        Value = dIdDtw
                    };
                    var DispatchedLineItems = context.Database.SqlQuery<AssignmentDispatchedItemsDc>("EXEC BatchCode.TripAssignmentBatchProductsList @AssignmentId", IssuanceId).ToList();

                    if (deliveryIssuance != null && deliveryIssuance.Any())
                    {
                        string Borderid = Convert.ToString(deliveryIssuance[0].DeliveryIssuanceId);
                        string BorderCodeId = Borderid.PadLeft(9, '0');
                        temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                        result.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode
                    }
                    result.NewDeliveryIssuanceId = DispatchedLineItems.Count() > 0 ? string.Join(",", DispatchedLineItems.Where(x => x.ReDispatchCount == 0 && x.IsReAttempt == false).Select(x => x.DeliveryIssuanceId).Distinct()) : "";
                    result.RedispatchedDeliveryIssuanceId = DispatchedLineItems.Count() > 0 ? string.Join(",", DispatchedLineItems.Where(x => (x.ReDispatchCount > 0 || x.IsReAttempt == true)).Select(x => x.DeliveryIssuanceId).Distinct()) : "";
                    //Newly
                    List<AssignmentDispatchedItemsDc> assignmentDispatchedItemsDc = new List<AssignmentDispatchedItemsDc>();
                    List<RedispatchAssignmentDispatchedItemsDc> redispatchAssignmentDispatchedItemsDc = new List<RedispatchAssignmentDispatchedItemsDc>();
                    assignmentDispatchedItemsDc.AddRange(DispatchedLineItems.Where(x => x.ReDispatchCount == 0 && x.IsReAttempt == false).GroupBy(x => new { x.ItemMultiMRPId, x.IsFreeItem, x.BatchCode, x.isClearance, x.OrderType }).Select(t =>
                          new AssignmentDispatchedItemsDc
                          {
                              orderIds = string.Join(",", DispatchedLineItems.Where(i => i.ItemMultiMRPId == t.Key.ItemMultiMRPId && i.IsFreeItem == t.Key.IsFreeItem
                              && i.isClearance == t.Key.isClearance && i.BatchCode == t.Key.BatchCode && i.ReDispatchCount == 0 && i.IsReAttempt == false).Select(i => i.OrderId).ToArray()),
                              qty = t.Sum(x => x.qty),
                              ItemMultiMRPId = t.Key.ItemMultiMRPId,
                              BatchCode = t.Key.BatchCode,
                              SellingUnitName = t.Min(x => x.SellingUnitName),
                              IsReDispatched = false,
                              PurchaseMinOrderQty = t.Min(x => x.PurchaseMinOrderQty),
                              UnitPrice = t.Min(x => x.UnitPrice),
                              IsFreeItem = t.Key.IsFreeItem,
                              isClearance = t.Key.OrderType == 8 ? true : false
                          }).OrderBy(x => x.SellingUnitName).ToList());
                    //Redispatched
                    result.Itemlist = assignmentDispatchedItemsDc;
                    redispatchAssignmentDispatchedItemsDc.AddRange(DispatchedLineItems.Where(x => (x.ReDispatchCount > 0 || x.IsReAttempt == true)).GroupBy(x => new { x.ItemMultiMRPId, x.IsFreeItem, x.BatchCode, x.isClearance, x.OrderType }).Select(t =>
                        new RedispatchAssignmentDispatchedItemsDc
                        {
                            orderIds = string.Join(",", DispatchedLineItems.Where(i => i.ItemMultiMRPId == t.Key.ItemMultiMRPId && i.IsFreeItem == t.Key.IsFreeItem
                            && i.isClearance == t.Key.isClearance && i.BatchCode == t.Key.BatchCode && (i.ReDispatchCount > 0 || i.IsReAttempt == true)).Select(i => i.OrderId).ToArray()),
                            qty = t.Sum(x => x.qty),
                            ItemMultiMRPId = t.Key.ItemMultiMRPId,
                            BatchCode = t.Key.BatchCode,
                            SellingUnitName = t.Min(x => x.SellingUnitName),
                            IsReDispatched = true,
                            PurchaseMinOrderQty = t.Min(x => x.PurchaseMinOrderQty),
                            UnitPrice = t.Min(x => x.UnitPrice),
                            IsFreeItem = t.Key.IsFreeItem,
                            isClearance = t.Key.OrderType == 8 ? true : false
                        }).OrderBy(x => x.SellingUnitName).ToList());
                    result.ReDispatchItemlist = redispatchAssignmentDispatchedItemsDc;
                    if (deliveryIssuance != null && deliveryIssuance.Any())
                    {
                        list = context.OrderDispatchedMasters.Where(x => x.active == true && deliveryIssuanceIds.Contains(x.DeliveryIssuanceIdOrderDeliveryMaster ?? 0)).Select(DBoyorders =>
                             new orderamount()
                             {
                                 orderid = DBoyorders.OrderId,
                                 ShopName = DBoyorders.ShopName,
                                 amount = DBoyorders.GrossAmount,
                                 DeliveryBoy = DBoyorders.DboyName,
                                 AssignmentId = DBoyorders.DeliveryIssuanceIdOrderDeliveryMaster
                             }
                      ).ToList();
                    }
                    result.orderamount = list;
                }
            }
            return result;
        }
        [Route("TripGetAssignmentOrder")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage TripGetAssignmentOrder(int TripPlannerMasterId)
        {
            using (var context = new AuthContext())
            {

                //-----s----

                AssignmentPrintResultDc Result = new AssignmentPrintResultDc();
                List<OrderDispatchedMaster> finalList = new List<OrderDispatchedMaster>();
                var tripPlannerConfirmedMasters = context.TripPlannerConfirmedMasters.Where(x => x.TripPlannerMasterId == TripPlannerMasterId && x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).FirstOrDefault();

                TripPlannerHelper tripPlannerHelper = new TripPlannerHelper();
                var blockedOrderList = tripPlannerHelper.GetBlockedOrderList(tripPlannerConfirmedMasters, context);
                if (blockedOrderList != null && blockedOrderList.Any())
                {
                    Result = new AssignmentPrintResultDc
                    {
                        BlockedOrderList = blockedOrderList,
                        IsSuccess = false,
                        OrderDispatchedMasters = null,
                        OrderTYpe = 0,
                        PdfPath = null,
                        warehouse = null
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, Result);
                }


                //var deliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.TripPlannerConfirmedMasterId == tripPlannerConfirmedMasters).ToList();
                var deliveryIssuanceId = tripPlannerHelper.GetdeliveryIssuanceId(tripPlannerConfirmedMasters, context);
                var list = context.OrderDispatchedMasters.Where(a => deliveryIssuanceId.Contains(a.DeliveryIssuanceIdOrderDeliveryMaster.Value)).Include("orderDetails").ToList();
                //var OrderId = list.Select(x => x.OrderId).FirstOrDefault();
                var OrderIds = list.Select(x => x.OrderId).Distinct().ToList();
                var warehouseId = list.Select(x => x.WarehouseId).FirstOrDefault();
                var customerId = list.Select(x => x.CustomerId).Distinct().ToList();
                var warehouse = context.Warehouses.Where(x => x.WarehouseId == warehouseId).FirstOrDefault();
                var ordermasters = context.DbOrderMaster.Where(a => OrderIds.Contains(a.OrderId)).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer, x.IsFirstOrder, x.paymentThrough });

                //bool IsZaruriOrder = context.DbOrderMaster.Any(a => a.OrderId == OrderId && a.OrderType == 5);

                var ordermastersdetails = context.DbOrderDetails.Where(a => OrderIds.Contains(a.OrderId)).Select(x => new { x.OrderId, x.OrderDetailsId, x.PTR, x.ExecutiveName }).ToList();


                DataTable dt = new DataTable();
                dt.Columns.Add("IntValue");
                foreach (var item in customerId)
                {
                    var dr = dt.NewRow();
                    dr["IntValue"] = item;
                    dt.Rows.Add(dr);
                }
                var param = new SqlParameter("CustomerId", dt);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.IntValues";


                // var CustomerIds = new SqlParameter("@CustomerId", customerId);

                var GetStateCodeList = context.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByCustomerId @CustomerId", param).ToList();
                bool IsZaruriOrder = ordermasters.Any(a => a.OrderType == 5);
                list.ForEach(x =>
                {
                    if (ordermasters.Any(y => y.OrderId == x.OrderId))
                    {

                        x.shippingStateName = GetStateCodeList.Count() > 0 ? GetStateCodeList.FirstOrDefault(y => y.CustomerId == x.CustomerId).shippingStateName : "";
                        x.shippingStateCode = GetStateCodeList.Count() > 0 ? GetStateCodeList.FirstOrDefault(y => y.CustomerId == x.CustomerId).shippingStateCode : "";
                        x.BillingStateCode = GetStateCodeList.Count() > 0 ? GetStateCodeList.FirstOrDefault(y => y.CustomerId == x.CustomerId).BillingStateCode : "";
                        x.BillingStateName = GetStateCodeList.Count() > 0 ? GetStateCodeList.FirstOrDefault(y => y.CustomerId == x.CustomerId).BillingStateName : "";
                        x.OrderType = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).OrderType;
                        x.IsPrimeCustomer = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).IsPrimeCustomer;
                        x.IsFirstOrder = ordermasters.FirstOrDefault(y => y.OrderId == x.OrderId).IsFirstOrder;
                        x.SalesPerson = string.Join(",", ordermastersdetails.Where(z => z.OrderId == x.OrderId && !string.IsNullOrEmpty(z.ExecutiveName)).Select(z => z.ExecutiveName).Distinct());
                        if (!string.IsNullOrEmpty(x.SalesPerson))
                        {
                            var SalesPersonName = x.SalesPerson.Split(',').ToList();
                            var qquery = from c in context.Peoples
                                         where SalesPersonName.Contains(c.DisplayName)
                                         select c.Mobile;
                            x.SalesMobile = string.Join(",", qquery);
                        }
                        if (x.orderDetails != null)
                        {
                            foreach (var orderDetaillst in x.orderDetails)
                            {
                                orderDetaillst.PTR = ordermastersdetails.Where(z => z.OrderDetailsId == orderDetaillst.OrderDetailsId).Select(z => z.PTR).FirstOrDefault();
                            }
                        }
                    }
                });


                List<TradeInvoiceDc> files = new List<TradeInvoiceDc>();

                string FileName = "";
                string fileUrl = "";
                string folderPath = "";
                string fullPhysicalPath = "";
                string thFileName = "";

                var OrderPayments = context.PaymentResponseRetailerAppDb.Where(p => p.status == "Success" && OrderIds.Contains(p.OrderId)).ToList();
                var customerIds = list.Select(x => x.CustomerId).Distinct().ToList();
                var billdiscountOfferId = context.BillDiscountDb.Where(x => OrderIds.Contains(x.OrderId) && customerIds.Contains(x.CustomerId)).Select(z => new { z.OfferId, z.OrderId }).ToList();
                List<orderOfferDTO> OrderOffers = new List<orderOfferDTO>();
                if (billdiscountOfferId.Count > 0)
                {
                    var offerids = billdiscountOfferId.Select(x => x.OfferId).ToList();
                    OrderOffers = context.OfferDb.Where(x => offerids.Contains(x.OfferId)).Select(x => new orderOfferDTO { Offerid = x.OfferId, OfferOn = x.OfferOn, OfferCode = x.OfferCode }).ToList();
                    OrderOffers.ForEach(x => x.Orderid = billdiscountOfferId.FirstOrDefault(y => y.OfferId == x.Offerid).OrderId);
                }
                foreach (var od in list)
                {


                    if (IsZaruriOrder)
                    {


                        od.OrderType = 5;

                        FileName = od.invoice_no.Replace('/', '_') + ".pdf";
                        folderPath = HttpContext.Current.Server.MapPath(@"~\ZaruriReportDownloads");

                        string TartgetfolderPath = "";

                        TartgetfolderPath = HttpContext.Current.Server.MapPath(@"~\ZaruriAssignmentDownloads");
                        if (!Directory.Exists(TartgetfolderPath))
                            Directory.CreateDirectory(TartgetfolderPath);


                        thFileName = "Asss_" + TripPlannerMasterId + ".pdf";
                        fileUrl = "/ZaruriAssignmentDownloads" + "/" + thFileName;
                        fullPhysicalPath = TartgetfolderPath + "\\" + thFileName;

                        TradeInvoiceDc dc = new TradeInvoiceDc();
                        dc.OrderId = od.OrderId;
                        dc.FileName = folderPath + "\\" + FileName;
                        dc.barcode = od.InvoiceBarcodeImage;
                        dc.invoice_no = od.invoice_no;
                        files.Add(dc);
                    }
                    else
                    {
                        od.OrderType = ordermasters.FirstOrDefault(s => s.OrderId == od.OrderId).OrderType; // 1;
                        od.paymentThrough = ordermasters.FirstOrDefault(x => x.OrderId == od.OrderId).paymentThrough;

                        if (od.BillDiscountAmount > 0)
                        {
                            if (OrderOffers.Any(x => x.Orderid == od.OrderId))
                            {
                                od.offertype = string.Join(",", OrderOffers.Where(x => x.Orderid == od.OrderId).Select(x => x.OfferOn));
                            }
                            //var billdiscountOfferId = context.BillDiscountDb.Where(x => x.OrderId == od.OrderId).Select(z => z.OfferId).ToList();
                            //if (billdiscountOfferId.Count > 0)
                            //{
                            //    List<string> offeron = context.OfferDb.Where(x => billdiscountOfferId.Contains(x.OfferId)).Select(x => new { x.OfferOn, x.OfferCode }).ToList().Select(x => (!string.IsNullOrEmpty(x.OfferCode) ? x.OfferCode : x.OfferOn)).ToList();
                            //    od.offertype = string.Join(",", offeron);
                            //}
                        }
                        //for igst case if true then apply condion to hide column of cgst sgst cess
                        if (!string.IsNullOrEmpty(od.Tin_No) && od.Tin_No.Length >= 11)
                        {
                            string CustTin_No = od.Tin_No.Substring(0, 2);
                            od.IsIgstInvoice = !context.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == od.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No);

                        }
                        od.PaymentDetails = OrderPayments.Where(p => p.OrderId == od.OrderId).ToList();
                        if (!(string.IsNullOrEmpty(od.paymentThrough)) && od.paymentThrough.ToLower() == "paylater")
                        {
                            List<PaymentResponseRetailerAppDc> res = new List<PaymentResponseRetailerAppDc>();
                            var orderid = new SqlParameter("@orderId", od.OrderId);
                            res = context.Database.SqlQuery<PaymentResponseRetailerAppDc>("exec GetOrderpayment @orderId", orderid).ToList();
                            if (res != null)
                            {
                                od.PaymentDetails.ForEach(a =>
                                {
                                    a.statusDesc = res.Any(x => x.id == a.id) ? res.FirstOrDefault(x => x.id == a.id).DueDaysDesc : a.statusDesc;
                                });
                            }
                        }


                        /////vip

                    }

                    decimal Amount = Convert.ToDecimal(od.GrossAmount - od.DiscountAmount);
                    if (Amount > 0)
                        od.InvoiceAmountInWord = ConvertNumberToWord.ConvToWordRupee(Amount);
                    else
                        od.InvoiceAmountInWord = "";

                }
                if (IsZaruriOrder)
                {

                    if (!File.Exists(fullPhysicalPath))
                    {
                        bool iss = MergePDFs(files, fullPhysicalPath);
                        if (!iss)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, "Pdf not created");
                        }

                    }
                    fileUrl = string.Format("{0}://{1}:{2}/{3}", HttpContext.Current.Request.Url.Scheme
                                                     , HttpContext.Current.Request.Url.DnsSafeHost
                                                     , HttpContext.Current.Request.Url.Port
                                                     , fileUrl);

                    Result.OrderTYpe = 5;
                    Result.PdfPath = fileUrl;
                    return Request.CreateResponse(HttpStatusCode.OK, Result);
                }

                //var customerId = 




                //  return Request.CreateResponse(HttpStatusCode.OK, finalListData);

                Result.warehouse = warehouse;
                Result.OrderTYpe = 1;
                Result.IsSuccess = true;
                Result.OrderDispatchedMasters = list;
                return Request.CreateResponse(HttpStatusCode.OK, Result);
            }
        }
    }

    public class TripDeliveryIssuanceDC
    {
        public bool IsSuccess { get; set; }
        public List<TripBlockedOrderVM> BlockedOrderList { get; set; }
        public string DeliveryIssuanceId { get; set; }
        public string DboyName { get; set; }
        public string WarehouseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public double? TotalAssignmentAmount { get; set; }
        public byte[] AssignmentBarcodeImage { get; set; }
        public List<AssignmentDispatchedItemsDc> Itemlist { get; set; }
        public List<RedispatchAssignmentDispatchedItemsDc> ReDispatchItemlist { get; set; }
        public List<orderamount> orderamount { get; set; }
        public string NewDeliveryIssuanceId { get; set; }
        public string RedispatchedDeliveryIssuanceId { get; set; }

    }
    public class AssignmentDispatchedItemsDc
    {
        public int qty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string OrderId { get; set; }
        public long OrderDetailsId { get; set; }
        public int ReDispatchCount { get; set; }
        public string SellingUnitName { get; set; }
        public string orderIds { get; set; }
        public bool IsReDispatched { get; set; }
        public int PurchaseMinOrderQty { get; set; }
        public int? DeliveryIssuanceId { get; set; }
        public bool IsReAttempt { get; set; }
        public double UnitPrice { get; set; }
        public bool IsFreeItem { get; set; }
        public string BatchCode { get; set; }
        public bool isClearance { get; set; }
        public int OrderType { get; set; }

    }
    public class RedispatchAssignmentDispatchedItemsDc
    {
        public int qty { get; set; }
        public int ItemMultiMRPId { get; set; }
        public string OrderId { get; set; }
        public int ReDispatchCount { get; set; }
        public string SellingUnitName { get; set; }
        public string orderIds { get; set; }
        public bool IsReDispatched { get; set; }
        public int PurchaseMinOrderQty { get; set; }
        public double UnitPrice { get; set; }
        public bool IsFreeItem { get; set; }
        public string BatchCode { get; set; }

        public bool isClearance { get; set; }

    }
    public class AssignmentPrintResultDc
    {
        public bool IsSuccess { get; set; }
        public List<TripBlockedOrderVM> BlockedOrderList { get; set; }
        public List<OrderDispatchedMaster> OrderDispatchedMasters { get; set; }
        public int OrderTYpe { get; set; }
        public string PdfPath { get; set; }
        public Warehouse warehouse { get; set; }


    }
    public class TradeInvoiceDc
    {
        public string FileName { get; set; }
        public int OrderId { get; set; }
        public byte[] barcode { get; set; }
        public string invoice_no { get; set; }

    }

    public class DeliveryIssuanceDTOApp
    {
        public List<DeliveryIssuance> DeliveryIssuance { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
    }
    public class DeliveryIssuanceDTO
    {
        public DeliveryIssuance DeliveryIssuance { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
    }

    public class orderOfferDTO
    {
        public int Orderid { get; set; }
        public int Offerid { get; set; }
        public string OfferOn { get; set; }
        public string OfferCode { get; set; }
    }



    public class orderamount
    {
        public int orderid { get; set; }
        public double amount { get; set; }
        public string ShopName { get; set; }
        public string DeliveryBoy { get; set; }
        public int? AssignmentId { get; set; }
    }

    public class ResObj
    {
        public DeliveryIssuance DI { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
    }

    public class ResListObj
    {
        public List<DeliveryIssuance> DI { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
    }

    public class AssignmentAccRejDTO
    {
        public int DeliveryIssuanceId { get; set; }
        public string RejectReason { get; set; }
        public bool Acceptance { get; set; }
    }

    public class ResMsg
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public AgentPitchMasterDc AgentPitchMasters { get; internal set; }
    }


    public class orderidserachBased
    {
        public orderdataassignmentDTO orderdata { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
    }
    #region for use order data get Assignment based 
    public class orderdataassignmentDTO
    {
        public int OrderId { get; set; }
        public int OddetailCount { get; set; }
        public string CustomerName { get; set; }
        public int? DeliveryIssuanceId { get; set; }
        public string ShopName { get; set; }
        public string Status { get; set; }
        public int orderCount { get; set; }

        public DateTime CreatedDate { get; set; }
        public string Skcode { get; set; }
        public string Customerphonenum { get; set; }
        public double TotalAmount { get; set; }
        public string ShippingAddress { get; set; }

    }


    #endregion


    public class ResListAcceptPending
    {
        public List<AssignmentAcceptPendingDTO> AssignmentAcceptPending { get; set; }
        public bool status { get; set; }
        public string Message { get; set; }
    }
    public class AssignmentAcceptPendingDTO
    {
        public int DeliveryIssuanceId { get; set; }
        public DateTime? StartDateTime { get; set; }
        public bool? Start { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool? End { get; set; }
        public DateTime AssignmentDate { get; set; }

        public bool IsDirectionExist { get; set; }
        public double AssignmentDistance { get; set; }
        public double ReturnDistance { get; set; }
        public double AssignmentDuration { get; set; }
        public double ReturnDuration { get; set; }
        public double TotalUnloadingDuration { get; set; }
    }
    public class ResMsgNew
    {
        public List<CheckAssignmentEwayBillAndIRNnoDC> CheckAssignmentEwayBillAndIRNno { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
    public class CheckAssignmentEwayBillAndIRNnoDC
    {
        public int OrderId { get; set; }
        public string Reson { get; set; }
    }
}
