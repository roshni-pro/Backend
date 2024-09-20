using AngularJSAuthentication.BusinessLayer.Managers.Masters;
using AngularJSAuthentication.BusinessLayer.Managers.Reports;
using AngularJSAuthentication.DataContracts.Masters;
using AngularJSAuthentication.DataContracts.Shared;
using AngularJSAuthentication.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using static AngularJSAuthentication.API.Controllers.OrderMasterrController;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/OrderDispatchedMaster")]
    public class OrderDispatchedMasterController : ApiController
    {


        public static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);


        [Route("")]
        public List<OrderDispatchedMaster> Get()
        {
            logger.Info("start get all OrderDispatchedMaster: ");
            using (var db = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    List<OrderDispatchedMaster> displist = new List<OrderDispatchedMaster>();
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

                    displist = db.OrderDispatchedMasters.Where(x => x.Status == "Ready to Dispatch" && x.CompanyId == compid).ToList();
                    logger.Info("End  UnitMaster: ");
                    return displist;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall OrderDispatchedMaster " + ex.Message);
                    logger.Info("End getall OrderDispatchedMaster: ");
                    return null;
                }
            }
        }
        [Route("GetFreeItemStock")]
        public int GetFreeItemStock(int orderId)
        {
            logger.Info("start get all OrderDispatchedMaster: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    var lst = context.GetFreeItemStockOnOrderId(orderId);
                    return lst;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in getall OrderDispatchedMaster " + ex.Message);
                    logger.Info("End getall OrderDispatchedMaster: ");
                    return 0;
                }
            }
        }

        [Route("")]
        public async System.Threading.Tasks.Task<OrderDispatchedMaster> Get(string id)
        {
            logger.Info("start PurchaseOrderMaster: ");
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                OrderDispatchedMaster order = new OrderDispatchedMaster();

                if (id != null)
                {
                    int Id = Convert.ToInt32(id);
                    order = db.OrderDispatchedMasters.Where(x => x.OrderId == Id).Include("orderDetails").FirstOrDefault();

                    #region add offer type
                    if (order != null)
                    {
                        try
                        {
                            var ordermasters = db.DbOrderMaster.Where(a => a.OrderId == Id).Select(x => new { x.OrderId, x.OrderType, x.IsPrimeCustomer, x.IsFirstOrder }).FirstOrDefault();

                            if (!string.IsNullOrEmpty(order.invoice_no) && ordermasters.OrderType == 11)
                            {
                                temOrderQBcode code = db.GetBarcode(order.invoice_no);
                                order.InvoiceBarcodeImage = code.BarcodeImage;
                            };

                            order.IsPrimeCustomer = ordermasters.IsPrimeCustomer;
                            order.IsFirstOrder = ordermasters.IsFirstOrder;

                            #region offerdiscounttype
                            if (order.BillDiscountAmount > 0)
                            {
                                var billdiscountOfferId = db.BillDiscountDb.Where(x => x.OrderId == order.OrderId && x.CustomerId == order.CustomerId).Select(z => z.OfferId).ToList();
                                if (billdiscountOfferId.Count > 0)
                                {
                                    List<string> offeron = db.OfferDb.Where(x => billdiscountOfferId.Contains(x.OfferId)).Select(x => new { x.OfferOn, x.OfferCode }).ToList().Select(x => (!string.IsNullOrEmpty(x.OfferCode) ? x.OfferCode : x.OfferOn)).ToList();
                                    order.offertype = string.Join(",", offeron);
                                }
                            }
                            #endregion
                            //for igst case if true then apply condion to hide column of cgst sgst cess
                            if (!string.IsNullOrEmpty(order.Tin_No) && order.Tin_No.Length >= 11)
                            {
                                string CustTin_No = order.Tin_No.Substring(0, 2);

                                //if (!CustTin_No.StartsWith("0"))
                                //{
                                order.IsIgstInvoice = !db.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == order.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No);
                                //}

                            }

                        }
                        catch (Exception ex) { }

                        //#region RazorPayQR get on order

                        //var RazorPayQR = db.RazorpayVirtualAccounts.Where(x => x.OrderId == order.OrderId && x.IsActive == true).FirstOrDefault();
                        //if (RazorPayQR != null && RazorPayQR.QrCodeUrl != null)
                        //{
                        //    order.RazorPayQR = RazorPayQR.QrCodeUrl;
                        //}
                        //#endregion
                    }
                    #endregion
                    if (order != null)
                    {
                        var ExecutiveIds = db.DbOrderDetails.Where(z => z.OrderId == Id && z.ExecutiveId > 0).Select(z => z.ExecutiveId).ToList();
                        if (ExecutiveIds != null && ExecutiveIds.Any())
                        {
                            var peoples = db.Peoples.Where(x => ExecutiveIds.Contains(x.PeopleID)).Select(x => new { x.DisplayName, x.Mobile }).ToList();
                            order.SalesPerson = string.Join(",", peoples.Select(x => x.DisplayName));
                            order.SalesMobile = string.Join(",", peoples.Select(x => x.Mobile));
                        }

                    }
                    //---------S-------------------
                    if (order != null)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("IntValue");
                        var dr = dt.NewRow();
                        dr["IntValue"] = order.CustomerId;
                        dt.Rows.Add(dr);
                        var param = new SqlParameter("CustomerId", dt);
                        param.SqlDbType = SqlDbType.Structured;
                        param.TypeName = "dbo.IntValues";

                        var GetStateCodeList = db.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByCustomerId @CustomerId", param).FirstOrDefault();

                        if (GetStateCodeList != null)
                        {
                            order.shippingStateName = GetStateCodeList.shippingStateName;
                            order.shippingStateCode = GetStateCodeList.shippingStateCode;
                            order.BillingStateName = GetStateCodeList.BillingStateName;
                            order.BillingStateCode = GetStateCodeList.BillingStateCode;
                        }
                    }
                    //---------E-------------------


                    if (order != null && order.DeliveryIssuanceIdOrderDeliveryMaster.HasValue && order.DeliveryIssuanceIdOrderDeliveryMaster.Value > 0)
                    {
                        DeliveryIssuance DeliveryIssuance = db.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == order.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault();
                        if (DeliveryIssuance.Status == "Payment Accepted" || DeliveryIssuance.Status == "Submitted" || DeliveryIssuance.Status == "Pending")
                        {
                            order.DeliveryIssuanceStatus = DeliveryIssuance.Status;
                            return order;
                        }
                    }
                    //if (order != null)
                    //{
                    //    var ExecutiveIds = db.DbOrderDetails.Where(z => z.OrderId == Id && z.ExecutiveId > 0).Select(z => z.ExecutiveId).ToList();
                    //    if (ExecutiveIds != null && ExecutiveIds.Any())
                    //    {
                    //        var peoples = db.Peoples.Where(x => ExecutiveIds.Contains(x.PeopleID)).Select(x => new { x.DisplayName, x.Mobile }).ToList();
                    //        order.SalesPerson = string.Join(",", peoples.Select(x => x.DisplayName));
                    //        order.SalesMobile = string.Join(",", peoples.Select(x => x.Mobile));
                    //    }

                    //}
                    if (order != null)
                    {
                        MastersManager mastersManager = new MastersManager();
                        TripPickerIdDc tripPickerIdDc = await mastersManager.GetPickerId_TripId(order.OrderId);
                        if (tripPickerIdDc != null)
                        {
                            order.OrderPickerMasterId = tripPickerIdDc.PickerId == null ? 0 : tripPickerIdDc.PickerId.OrderPickerMasterId;
                            order.TripPlannerMasterId = tripPickerIdDc.TripId == null ? 0 : tripPickerIdDc.TripId.TripPlannerMasterId;
                        }
                    }


                }
                return order;

            }
        }
        [Route("")]
        public PaggingData Get(int list, int page, string DBoyNo, DateTime? datefrom, DateTime? dateto, int? OrderId, int WarehouseId)
        {
            List<OrderDispatchedMaster> displist = new List<OrderDispatchedMaster>();
            PaggingData paggingData = new PaggingData();
            logger.Info("start OrderSettle: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

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

                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    Warehouse_id = WarehouseId;
                    if (Warehouse_id > 0)
                    {
                        paggingData = context.AllDispatchedOrderMasterPagingWid(list, page, DBoyNo, datefrom, dateto, OrderId, compid, Warehouse_id);
                        logger.Info("End OrderSettle: ");
                        //return lst;
                    }
                    else
                    {
                        paggingData = context.AllDispatchedOrderMasterPaging(list, page, DBoyNo, datefrom, dateto, OrderId, compid);
                        logger.Info("End OrderSettle: ");
                        // return lst;
                    }
                    List<OrderDispatchedMaster> orderdispatch = (List<OrderDispatchedMaster>)paggingData.ordermaster;

                    //If assignment not freezed //show on settle page if Assignment not freezed then it can't settle
                    var DeliveryIssuanceIdList = orderdispatch.Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).ToList();



                    var NotFreezed = context.DeliveryIssuanceDb.Where(x => DeliveryIssuanceIdList.Contains(x.DeliveryIssuanceId) && (x.Status == "Submitted" || x.Status == "Payment Accepted" || x.Status == "Pending"));


                    var orderids = orderdispatch.Select(x => x.OrderId).ToList();

                    var PaymentResponseRetailerApps = context.PaymentResponseRetailerAppDb.Where(x => orderids.Contains(x.OrderId) && x.status == "Success").ToList();
                    var PaymentResponseRetailerAppList = new List<RetailerOrderPaymentDc>();
                    foreach (var item in PaymentResponseRetailerApps.GroupBy(x => new { x.OrderId, x.PaymentFrom, x.GatewayTransId, x.IsOnline, x.status }))
                    {
                        PaymentResponseRetailerAppList.Add(new RetailerOrderPaymentDc
                        {
                            GatewayTransId = item.FirstOrDefault().GatewayTransId,
                            OrderId = item.FirstOrDefault().OrderId,
                            amount = item.Sum(x => x.amount),
                            status = item.FirstOrDefault().status,
                            PaymentFrom = item.FirstOrDefault().PaymentFrom,
                            ChequeImageUrl = item.FirstOrDefault().ChequeImageUrl,
                            ChequeBankName = item.FirstOrDefault().ChequeBankName,
                            IsOnline = item.FirstOrDefault().IsOnline,
                            TxnDate = item.OrderBy(c => c.CreatedDate).FirstOrDefault().CreatedDate
                        });
                    }

                    var paymentdetails = orderids != null && orderids.Any() ? PaymentResponseRetailerAppList.Where(z => orderids.Contains(z.OrderId) && z.status == "Success")
                              .Select(z => new PaymentDto
                              {
                                  Amount = z.amount,
                                  PaymentFrom = z.PaymentFrom,
                                  // TransDate = z.UpdatedDate,
                                  TransRefNo = z.GatewayTransId,
                                  OrderId = z.OrderId
                              }).ToList() : new List<PaymentDto>();
                    foreach (var Os in orderdispatch)
                    {
                        //show on settle page if Assignment not freezed then it can't settle
                        if (NotFreezed.Any(x => x.DeliveryIssuanceId == Os.DeliveryIssuanceIdOrderDeliveryMaster))
                        {
                            Os.DeliveryIssuanceStatus = NotFreezed.FirstOrDefault(x => x.DeliveryIssuanceId == Os.DeliveryIssuanceIdOrderDeliveryMaster).Status;
                        }

                        Os.BasicPaymentDetails = paymentdetails.Where(x => x.OrderId == Os.OrderId).ToList();
                        if (Os.BasicPaymentDetails == null)
                            Os.BasicPaymentDetails = new List<PaymentDto>();

                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CASH"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = Os.CashAmount,
                                PaymentFrom = "Cash",
                                TransRefNo = ""
                            });
                        }
                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CHEQUE"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = Os.CheckAmount,
                                PaymentFrom = "Cheque",
                                TransRefNo = Os.CheckNo
                            });
                        }
                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "EPAYLATER"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = 0,
                                PaymentFrom = "EPaylater",
                                TransRefNo = ""
                            });
                        }
                        else
                        {
                            Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "EPAYLATER").ToList().ForEach(x => x.PaymentFrom = "EPaylater");
                        }
                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "Gullak"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = 0,
                                PaymentFrom = "Gullak",
                                TransRefNo = ""
                            });
                        }
                        else
                        {
                            Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "Gullak").ToList().ForEach(x => x.PaymentFrom = "Gullak");
                        }
                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "MPOS"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = 0,
                                PaymentFrom = "MPos",
                                TransRefNo = ""
                            });
                        }
                        else
                        {
                            Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "MPOS").ToList().ForEach(x => x.PaymentFrom = "MPos");
                        }
                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "HDFC" || x.PaymentFrom.ToUpper() == "TRUEPAY"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = Os.ElectronicAmount,
                                PaymentFrom = "Online",
                                TransRefNo = Os.ElectronicAmount > 0 ? Os.ElectronicPaymentNo : ""
                            });
                        }
                        else
                        {
                            Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "HDFC" || x.PaymentFrom.ToUpper() == "TRUEPAY").ToList().ForEach(x => x.PaymentFrom = "Online");
                        }

                        Os.BasicPaymentDetails = Os.BasicPaymentDetails.OrderBy(x => x.PaymentFrom).ToList();

                        Os.CashAmount = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CASH") ? Os.BasicPaymentDetails.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "CASH").Amount : 0;
                        Os.CheckAmount = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CHEQUE") ? Os.BasicPaymentDetails.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "CHEQUE").Amount : 0;
                        Os.CheckNo = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CHEQUE") ? Os.BasicPaymentDetails.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "CHEQUE").TransRefNo : "";
                        Os.EpayLater = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "EPAYLATER") ? Os.BasicPaymentDetails.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "EPAYLATER").Amount : 0;
                        Os.Online = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "ONLINE") ? Os.BasicPaymentDetails.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "ONLINE").Amount : 0;
                        Os.Empos = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "MPOS") ? Os.BasicPaymentDetails.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "MPOS").Amount : 0;

                    }
                    paggingData.ordermaster = orderdispatch;

                    return paggingData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderSettle " + ex.Message);
                    logger.Info("End  OrderSettle: ");
                    return null;
                }
            }
        }

        [Route("klop")]
        // search with Assignments id
        public PaggingData Get1(int list, int page, string DBoyNo, DateTime? datefrom, DateTime? dateto, int? OrderId, int? DeliveryIssuanceIdOrderDeliveryMaster, int WarehouseId)
        {
            List<OrderDispatchedMaster> displist = new List<OrderDispatchedMaster>();
            PaggingData paggingData = new PaggingData();
            logger.Info("start OrderSettle: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    int Warehouse_id = 0;

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


                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    Warehouse_id = WarehouseId;
                    if (Warehouse_id > 0)
                    {
                        paggingData = context.AllDispatchedOrderMasterPagingWid11(list, page, DBoyNo, datefrom, dateto, OrderId, DeliveryIssuanceIdOrderDeliveryMaster, compid, Warehouse_id);
                        logger.Info("End OrderSettle: ");
                        // return lst;
                    }
                    else
                    {
                        paggingData = context.AllDispatchedOrderMasterPaging11(list, page, DBoyNo, datefrom, dateto, OrderId, DeliveryIssuanceIdOrderDeliveryMaster, compid);
                        logger.Info("End OrderSettle: ");
                        //return lst;
                    }

                    List<OrderDispatchedMaster> orderdispatch = (List<OrderDispatchedMaster>)paggingData.ordermaster;

                    //If assignment not freezed
                    var DeliveryIssuanceIdList = orderdispatch.Select(x => x.DeliveryIssuanceIdOrderDeliveryMaster).ToList();
                    var NotFreezed = context.DeliveryIssuanceDb.Where(x => DeliveryIssuanceIdList.Contains(x.DeliveryIssuanceId) && (x.Status == "Submitted" || x.Status == "Payment Accepted" || x.Status == "Pending"));

                    var orderids = orderdispatch.Select(x => x.OrderId).ToList();
                    var PaymentResponseRetailerApps = context.PaymentResponseRetailerAppDb.Where(x => orderids.Contains(x.OrderId) && x.status == "Success").ToList();
                    var PaymentResponseRetailerAppList = new List<RetailerOrderPaymentDc>();
                    foreach (var item in PaymentResponseRetailerApps.GroupBy(x => new { x.OrderId, x.PaymentFrom, x.GatewayTransId, x.IsOnline, x.status }))
                    {
                        PaymentResponseRetailerAppList.Add(new RetailerOrderPaymentDc
                        {
                            GatewayTransId = item.FirstOrDefault().GatewayTransId,
                            OrderId = item.FirstOrDefault().OrderId,
                            amount = item.Sum(x => x.amount),
                            status = item.FirstOrDefault().status,
                            PaymentFrom = item.FirstOrDefault().PaymentFrom,
                            ChequeImageUrl = item.FirstOrDefault().ChequeImageUrl,
                            ChequeBankName = item.FirstOrDefault().ChequeBankName,
                            IsOnline = item.FirstOrDefault().IsOnline,
                            TxnDate = item.OrderBy(c => c.CreatedDate).FirstOrDefault().CreatedDate
                        });
                    }

                    var paymentdetails = orderids != null && orderids.Any() ? PaymentResponseRetailerAppList.Where(z => orderids.Contains(z.OrderId) && z.status == "Success")
                              .Select(z => new PaymentDto
                              {
                                  Amount = z.amount,
                                  PaymentFrom = z.PaymentFrom,
                                  //TransDate = z.UpdatedDate,
                                  TransRefNo = z.GatewayTransId,
                                  OrderId = z.OrderId,
                                  IsOnline = z.IsOnline
                              }).ToList() : new List<PaymentDto>();

                    foreach (var Os in orderdispatch)
                    {

                        Os.BasicPaymentDetails = PaymentResponseRetailerAppList.Where(z => z.OrderId == Os.OrderId && z.status == "Success")
                                .Select(z => new PaymentDto
                                {
                                    Amount = z.amount,
                                    PaymentFrom = z.PaymentFrom,
                                    //TransDate = z.UpdatedDate,
                                    TransRefNo = z.GatewayTransId,
                                    IsOnline = z.IsOnline
                                }).ToList();

                        //show on settle page if Assignment not freezed then can't settle
                        if (NotFreezed.Any(x => x.DeliveryIssuanceId == Os.DeliveryIssuanceIdOrderDeliveryMaster))
                        {
                            Os.DeliveryIssuanceStatus = NotFreezed.FirstOrDefault(x => x.DeliveryIssuanceId == Os.DeliveryIssuanceIdOrderDeliveryMaster).Status;
                        }

                        Os.BasicPaymentDetails = paymentdetails.Where(x => x.OrderId == Os.OrderId).ToList();
                        if (Os.BasicPaymentDetails == null)
                            Os.BasicPaymentDetails = new List<PaymentDto>();

                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CASH"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = Os.CashAmount,
                                PaymentFrom = "Cash",
                                TransRefNo = ""
                            });
                        }
                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CHEQUE"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = Os.CheckAmount,
                                PaymentFrom = "Cheque",
                                TransRefNo = Os.CheckNo,

                            });
                        }

                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "GULLAK"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = 0,
                                PaymentFrom = "GULLAK",
                                TransRefNo = ""
                            });
                        }
                        else
                        {
                            Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "GULLAK").ToList().ForEach(x => x.PaymentFrom = "GULLAK");
                        }


                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "EPAYLATER"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = 0,
                                PaymentFrom = "EPaylater",
                                TransRefNo = ""
                            });
                        }
                        else
                        {
                            Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "EPAYLATER").ToList().ForEach(x => x.PaymentFrom = "EPaylater");
                        }
                        if (!Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "MPOS"))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = 0,
                                PaymentFrom = "MPos",
                                TransRefNo = ""
                            });
                        }
                        else
                        {
                            Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "MPOS").ToList().ForEach(x => x.PaymentFrom = "MPos");
                        }
                        if (!Os.BasicPaymentDetails.Any(x => x.IsOnline && (x.PaymentFrom.ToUpper() != "EPAYLATER" && x.PaymentFrom.ToUpper() != "GULLAK" && x.PaymentFrom.ToUpper() != "MPOS")))
                        {
                            Os.BasicPaymentDetails.Add(new PaymentDto
                            {
                                Amount = Os.ElectronicAmount,
                                PaymentFrom = "Online",
                                TransRefNo = Os.ElectronicAmount > 0 ? Os.ElectronicPaymentNo : ""
                            });
                        }
                        else
                        {
                            Os.BasicPaymentDetails.Where(x => x.IsOnline && (x.PaymentFrom.ToUpper() != "EPAYLATER" && x.PaymentFrom.ToUpper() != "GULLAK" && x.PaymentFrom.ToUpper() != "MPOS")).ToList().ForEach(x => x.PaymentFrom = "Online");
                        }

                        //Os.BasicPaymentDetails = Os.BasicPaymentDetails.OrderBy(x => x.PaymentFrom).ToList();
                        Os.BasicPaymentDetails = Os.BasicPaymentDetails.GroupBy(x => new { x.OrderId, x.PaymentFrom }).Select(x => new PaymentDto { Amount = x.Sum(y => y.Amount), OrderId = x.Key.OrderId, PaymentFrom = x.Key.PaymentFrom, TransRefNo = string.Join(",", x.Select(y => y.TransRefNo)), TransDate = x.FirstOrDefault().TransDate }).OrderBy(x => x.PaymentFrom).ToList();
                        Os.CashAmount = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CASH") ? Os.BasicPaymentDetails.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "CASH").Amount : 0;
                        Os.CheckAmount = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CHEQUE") ? Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "CHEQUE").Sum(x => x.Amount) : 0;
                        Os.GullakAmount = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "GULLAK") ? Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "GULLAK").Sum(x => x.Amount) : 0;
                        Os.EpayLater = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "EPAYLATER") ? Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "EPAYLATER").Sum(x => x.Amount) : 0;
                        Os.Online = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "ONLINE") ? Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "ONLINE").Sum(x => x.Amount) : 0;
                        Os.Empos = Os.BasicPaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "MPOS") ? Os.BasicPaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "MPOS").Sum(x => x.Amount) : 0;
                        // Os.Fine = Convert.ToInt32(context.ChequeCollection.Where(x => x.Orderid == Os.OrderId && x.ChequeStatus == 6).Select(x => x.Fine).Sum());
                        //var sumdata = context.ChequeCollection.Where(x => x.Orderid == Os.OrderId && x.ChequeStatus == 6).Select(x => x.Fine).ToList();
                        //var finesum = 0;

                        //foreach (var finedata in sumdata)
                        //{
                        //    finesum += finedata;
                        //}
                        //Os.Fine = finesum;
                        var sumdata = (from cheque in context.ChequeCollection
                                       join appoved in context.ChequeFineAppoved
                                       on cheque.Id equals appoved.ChequeCollectionId
                                       where cheque.Orderid == Os.OrderId && cheque.ChequeStatus == 6
                                       select new
                                       {
                                           appoved.FineAmount,
                                           appoved.ChangeAmount
                                       }).ToList();

                        decimal finesum = 0;
                        foreach (var finedata in sumdata)
                        {

                            finesum += finedata.FineAmount;
                        }
                        Os.Fine = Convert.ToInt32(finesum);
                        decimal ChangeFine = 0;
                        foreach (var finedata in sumdata)
                        {
                            ChangeFine += finedata.ChangeAmount;
                        }
                        Os.ChangeFine = Convert.ToInt32(ChangeFine);
                    }

                    paggingData.ordermaster = orderdispatch;

                    return paggingData;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderSettle " + ex.Message);
                    logger.Info("End  OrderSettle: ");
                    return null;
                }
            }
        }
        [Route("")]
        public List<OrderDispatchedMaster> Get(DateTime datefrom, DateTime dateto, int id)
        {
            List<OrderDispatchedMaster> displist = new List<OrderDispatchedMaster>();
            logger.Info("start OrderSettle: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    var lst = context.AllFOrderDispatchedDeliveryDetails(datefrom, dateto, compid);
                    logger.Info("End OrderSettle: ");
                    return lst;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderSettle " + ex.Message);
                    logger.Info("End  OrderSettle: ");
                    return null;
                }
            }
        }

        [Route("")]
        public List<OrderDispatchedMaster> Get(DateTime datefrom, DateTime dateto, string DboyName, int id)
        {
            List<OrderDispatchedMaster> displist = new List<OrderDispatchedMaster>();
            logger.Info("start OrderSettle: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    var lst = context.AllFOrderDispatchedDeliveryBoyDetails(datefrom, dateto, DboyName, compid);
                    logger.Info("End OrderSettle: ");
                    return lst;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderSettle " + ex.Message);
                    logger.Info("End  OrderSettle: ");
                    return null;
                }
            }
        }


        [Route("")]
        [HttpPost]
        public OrderDispatchedMaster add(OrderDispatchedMaster item)
        {
            logger.Info("start OrderDispatchedMaster: ");
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

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
                    item.CompanyId = compid;
                    if (item == null)
                    {
                        throw new ArgumentNullException("item");
                    }



                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    context.AddOrderDispatchedMaster(item);
                    logger.Info("End  UnitMaster: ");
                    return item;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderDispatchedMaster " + ex.Message);
                    logger.Info("End  OrderDispatchedMaster: ");
                    return null;
                }
            }
        }

        [ResponseType(typeof(OrderDispatchedMaster))]
        [Route("")]
        [AcceptVerbs("PUT")]
        public OrderDispatchedMaster put(int id, string DboyNo)
        {

            logger.Info("start OrderDispatchedMaster: ");
            using (var db = new AuthContext())
            {
                using (var context = new AuthContext())
                {
                    try
                    {
                        var identity = User.Identity as ClaimsIdentity;
                        int compid = 0, userid = 0;

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
                        if (id == 0)
                        {
                            throw new ArgumentNullException("item");
                        }
                        OrderDispatchedMaster obj = db.OrderDispatchedMasters.Where(x => x.OrderDispatchedMasterId == id && x.CompanyId == compid).FirstOrDefault();
                        People DeliveryOBJ = db.Peoples.Where(x => x.Mobile == DboyNo && x.CompanyId == compid).FirstOrDefault();
                        obj.DboyName = DeliveryOBJ.DisplayName;
                        obj.DboyMobileNo = DeliveryOBJ.Mobile;
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        context.UpdateOrderDispatchedMaster(obj);
                        logger.Info("End  UnitMaster: ");
                        return obj;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error in OrderDispatchedMaster " + ex.Message);
                        logger.Info("End  OrderDispatchedMaster: ");
                        return null;
                    }
                }
            }
        }
        #region   Oreder details Advance Search

        [HttpGet]
        [Route("GetFreebiesItem")]
        public dynamic GetFreebiesItem(int OrderId, int ParentItemId, int WarehouseId)
        {
            using (var context = new AuthContext())
            {

                var offerdata = (from offitem in context.OfferItemDb
                                 join off in context.OfferDb
                                 on offitem.ReferOfferId equals off.OfferId
                                 //join item in context.itemMasters
                                 //on offitem.itemId equals item.ItemId
                                 where offitem.OrderId == OrderId && offitem.itemId == ParentItemId
                                 select new GetFreebiesInfo
                                 {
                                     itemId = offitem.itemId,
                                     MinOrderQuantity = offitem.MinOrderQuantity,
                                     FreeItemId = offitem.FreeItemId,
                                     NoOffreeQuantity = off.NoOffreeQuantity,
                                     OfferType = offitem.OfferType,
                                     BillDisountNoOffreeQuantity = offitem.NoOffreeQuantity
                                 }).FirstOrDefault();
                return offerdata;
            }

        }

        [Route("GetBackendInvoiceData")]
        [HttpGet]
        public async System.Threading.Tasks.Task<OrderMaster> GetBackendInvoiceData(string id)
        {
            logger.Info("start PurchaseOrderMaster: ");
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                OrderMaster order = new OrderMaster();
                int? DeliveryIssuanceId = null;
                if (id != null)
                {
                    int Id = Convert.ToInt32(id);
                    order = db.DbOrderMaster.Where(x => x.OrderId == Id).Include("orderDetails").FirstOrDefault();
                    order.orderDetails = order.orderDetails.OrderByDescending(z => z.CGSTTaxPercentage).ToList();
                    if (order.Status == "Delivered")
                    {
                        DeliveryIssuanceId = db.OrderDispatchedMasters.FirstOrDefault(x => x.OrderId == Id)?.DeliveryIssuanceIdOrderDeliveryMaster;
                    }

                    #region add offer type
                    if (order != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(order.invoice_no) && order.OrderType == 11)
                            {
                                temOrderQBcode code = db.GetBarcode(order.invoice_no);
                                order.InvoiceBarcodeImage = code.BarcodeImage;
                            };
                            if (DeliveryIssuanceId.HasValue)
                            {
                                order.DeliveryIssuanceIdOrderDeliveryMaster = DeliveryIssuanceId;
                            }
                            #region offerdiscounttype
                            if (order.BillDiscountAmount > 0)
                            {
                                order.offertype = db.Database.SqlQuery<string>("EXEC GetOfferCodebyOrderId " + id).FirstOrDefault();

                                //var billdiscountOfferId = db.BillDiscountDb.Where(x => x.OrderId == order.OrderId && x.CustomerId == order.CustomerId).Select(z => z.OfferId).ToList();
                                //if (billdiscountOfferId.Count > 0)
                                //{
                                //    List<string> offeron = db.OfferDb.Where(x => billdiscountOfferId.Contains(x.OfferId)).Select(x => new { x.OfferOn, x.OfferCode }).ToList().Select(x => (!string.IsNullOrEmpty(x.OfferCode) ? x.OfferCode : x.OfferOn)).ToList();
                                //    order.offertype = string.Join(",", offeron);
                                //}
                            }
                            #endregion
                            //for igst case if true then apply condion to hide column of cgst sgst cess
                            if (!string.IsNullOrEmpty(order.Tin_No) && order.Tin_No.Length >= 11)
                            {
                                string CustTin_No = order.Tin_No.Substring(0, 2);

                                //if (!CustTin_No.StartsWith("0"))
                                //{
                                order.IsIgstInvoice = !db.Warehouses.Any(x => x.GSTin != null && x.WarehouseId == order.WarehouseId && x.GSTin.Substring(0, 2) == CustTin_No);
                                //}

                            }

                        }
                        catch (Exception ex) { }

                        //#region RazorPayQR get on order

                        //var RazorPayQR = db.RazorpayVirtualAccounts.Where(x => x.OrderId == order.OrderId && x.IsActive == true).FirstOrDefault();
                        //if (RazorPayQR != null && RazorPayQR.QrCodeUrl != null)
                        //{
                        //    order.RazorPayQR = RazorPayQR.QrCodeUrl;
                        //}
                        //#endregion
                    }
                    #endregion
                    if (order != null && order.orderDetails != null && order.orderDetails.Any())
                    {
                        var ExecutiveIds = order.orderDetails.Where(z => z.OrderId == Id && z.ExecutiveId > 0).Select(z => z.ExecutiveId).ToList();
                        if (ExecutiveIds != null && ExecutiveIds.Any())
                        {
                            var peoples = db.Peoples.Where(x => ExecutiveIds.Contains(x.PeopleID)).Select(x => new { x.DisplayName, x.Mobile }).ToList();
                            order.SalesPerson = string.Join(",", peoples.Select(x => x.DisplayName));
                            order.SalesMobile = string.Join(",", peoples.Select(x => x.Mobile));
                        }

                    }
                    //---------S-------------------
                    if (order != null)
                    {
                        /*
                         DataTable dt = new DataTable();
                         dt.Columns.Add("IntValue");
                         var dr = dt.NewRow();
                         dr["IntValue"] = order.CustomerId;
                         dt.Rows.Add(dr);
                         var param = new SqlParameter("CustomerId", dt);
                         param.SqlDbType = SqlDbType.Structured;
                         param.TypeName = "dbo.IntValues";

                         var GetStateCodeList = db.Database.SqlQuery<GetStateCodeDc>("EXEC GetStateByCustomerId @CustomerId", param).FirstOrDefault();

                         if (GetStateCodeList != null)
                         {
                             order.shippingStateName = GetStateCodeList.shippingStateName;
                             order.shippingStateCode = GetStateCodeList.shippingStateCode;
                             order.BillingStateName = GetStateCodeList.BillingStateName;
                             order.BillingStateCode = GetStateCodeList.BillingStateCode;
                         }
                         
                       */

                        var ShippingCity = db.Cities.FirstOrDefault(x => x.Cityid == order.CityId);
                        if (ShippingCity != null)
                        {
                            var ShippingState = db.States.FirstOrDefault(x => x.Stateid == ShippingCity.Stateid);
                            order.shippingStateName = ShippingState?.StateName ?? "";
                            order.shippingStateCode = ShippingState?.ClearTaxStateCode;
                        }
                        var billingCity = db.Customers.FirstOrDefault(x => x.CustomerId == order.CustomerId).BillingCity;
                        if (!string.IsNullOrEmpty(billingCity))
                        {
                            var BillingCity = db.Cities.FirstOrDefault(x => x.CityName.ToLower() == billingCity.ToLower());
                            if (BillingCity != null)
                            {
                                var BillingState = db.States.FirstOrDefault(x => x.Stateid == BillingCity.Stateid);
                                order.BillingStateName = BillingState?.StateName ?? "";
                                order.BillingStateCode = BillingState?.ClearTaxStateCode;
                            }
                        }
                    }

                    if (order != null)
                    {
                        MastersManager mastersManager = new MastersManager();
                        TripPickerIdDc tripPickerIdDc = await mastersManager.GetPickerId_TripId(order.OrderId);
                        if (tripPickerIdDc != null)
                        {
                            order.OrderPickerMasterId = tripPickerIdDc.PickerId == null ? 0 : tripPickerIdDc.PickerId.OrderPickerMasterId;
                            order.TripPlannerMasterId = tripPickerIdDc.TripId == null ? 0 : tripPickerIdDc.TripId.TripPlannerMasterId;
                        }
                    }
                    string filenamess = id + ".pdf";
                    string ExcelSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/BO"), filenamess);
                    if (!File.Exists(ExcelSavePath))
                    {
                        order.IsInvoiceSent = false;
                    }
                    else
                    {
                        order.IsInvoiceSent = true;
                    }

                    if (order != null && order.Status == "Delivered")
                    {
                        var ReturnDays = db.ConsumerCompanyDetailDB.FirstOrDefault();
                        //var CreditNoteData = db.ConsumerCreditnoteDb.FirstOrDefault(x => x.CNOrderId == order.OrderId && x.IsActive == true && x.IsDeleted == false);
                        var UsedCreditNoteData = db.ConsumerCreditnoteDb.FirstOrDefault(x => x.UsedOrderId == order.OrderId && x.IsUsed == true && x.IsActive == true && x.IsDeleted == false);
                        if (UsedCreditNoteData != null)
                        {
                            order.CreditNoteAmount = UsedCreditNoteData.CNOrderValueUsed > 0 ? UsedCreditNoteData.CNOrderValueUsed : UsedCreditNoteData.Ordervalue;

                        }
                        if (ReturnDays != null)
                        {
                            if (((DateTime.Now.Date - order.CreatedDate.Date).TotalDays) < ReturnDays.ReturnDays)
                            {
                                order.IsShowReplace = true;
                            }
                        }
                        if (UsedCreditNoteData != null)
                        {
                            order.IsShowReplace = false;
                        }
                        if (db.DbOrderMaster.Any(x => order.WarehouseId == x.WarehouseId && x.ParentOrderId == Id))
                        {
                            order.IsShowReplace = false;
                        }
                    }
                }

                return order;

            }
        }
        public class GetMonthWiseData
        {
            public string skcode { get; set; }
            public string shopname { get; set; }
            public int? warehouseid { get; set; }
            public string warehousename { get; set; }
            public int? executiveid { get; set; }
            public string executivename { get; set; }

            public double Jan { get; set; }
            public double Feb { get; set; }
            public double Mar { get; set; }
            public double Apr { get; set; }
            public double May { get; set; }
            public double June { get; set; }
            public double July { get; set; }
            public double Aug { get; set; }
            public double Sep { get; set; }
            public double Oct { get; set; }
            public double Nov { get; set; }
            public double Dec { get; set; }
            public double TotalAmount { get; set; }
            public int? totalorderCount { get; set; }
        }
        #endregion

        #region   Oreder details download
        [Route("OrderDataDownload")]
        public List<OrderDispatchedMasterDTOMExport> getsData(DateTime datefrom, DateTime dateto, int WarehouseId)
        {

            logger.Info("start OrderSettle: ");
            using (var context = new AuthContext())
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    var ODMList = db.DbOrderMaster.Where(x => x.CreatedDate > datefrom && x.CreatedDate < dateto && x.WarehouseId == WarehouseId && x.Deleted == false).ToList();

                    List<OrderDispatchedMasterDTOMExport> FinalODMList = new List<OrderDispatchedMasterDTOMExport>();
                    List<int> custId = new List<int>();

                    foreach (var order in ODMList)
                    {

                        try
                        {
                            bool isexits = true;
                            if (FinalODMList.Count > 0)
                            {
                                foreach (var ss in custId)
                                {
                                    if (ss == order.CustomerId)
                                    {
                                        var found = FinalODMList.Where(arr => arr.CustomerId == order.CustomerId).FirstOrDefault();
                                        if (found != null)
                                        {

                                            found.OrderCount++;
                                            found.TotalAmount = found.TotalAmount + order.GrossAmount;
                                            isexits = false;
                                            break;

                                        }
                                    }
                                }
                                //
                                if (isexits)
                                {
                                    OrderDispatchedMasterDTOMExport dto = new OrderDispatchedMasterDTOMExport();
                                    dto.CustomerId = order.CustomerId;
                                    dto.OrderCount = 1;
                                    // dto.SalesPerson = order.SalesPerson;
                                    dto.CustomerName = order.CustomerName;
                                    dto.Customerphonenum = order.Customerphonenum;
                                    dto.ShopName = order.ShopName;
                                    dto.Skcode = order.Skcode;
                                    dto.WarehouseName = order.WarehouseName;
                                    dto.ShippingAddress = order.ShippingAddress;
                                    dto.TotalAmount = order.TotalAmount;
                                    FinalODMList.Add(dto);
                                    custId.Add(order.CustomerId);// added record
                                }

                            }
                            else
                            {
                                OrderDispatchedMasterDTOMExport dto = new OrderDispatchedMasterDTOMExport();
                                dto.CustomerId = order.CustomerId;
                                dto.OrderCount = 1;
                                // dto.SalesPerson = order.SalesPerson;
                                dto.CustomerName = order.CustomerName;
                                dto.Customerphonenum = order.Customerphonenum;
                                dto.ShopName = order.ShopName;
                                dto.Skcode = order.Skcode;
                                dto.WarehouseName = order.WarehouseName;
                                dto.ShippingAddress = order.ShippingAddress;
                                dto.TotalAmount = order.TotalAmount;
                                FinalODMList.Add(dto);
                                custId.Add(order.CustomerId);// added record
                            }

                        }
                        catch (Exception st)
                        {
                        }

                    }
                    return FinalODMList;
                    logger.Info("End OrderSettle: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderSettle " + ex.Message);
                    logger.Info("End  OrderSettle: ");
                    return null;
                }
        }
        #endregion



        //BOC Pravesh 08-08-2019
        //Task - SB1-T1157 Order Data Download Only for Kisan Kirana
        #region   Order details download Kisan Kirana
        [Route("OrderDataDownloadKK")]
        [HttpGet]
        public List<OrderDispatchedMasterDTOMExport> OrderDataDownloadKK(DateTime datefrom, DateTime dateto, int WarehouseId)
        {

            logger.Info("start OrderSettle: ");
            using (var context = new AuthContext())
            using (var db = new AuthContext())
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
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
                    var ODMList = (from oMast in db.DbOrderMaster
                                   join oDetail in db.DbOrderDetails on oMast.OrderId equals oDetail.OrderId
                                   where oMast.CreatedDate >= datefrom && oMast.CreatedDate <= dateto && oMast.WarehouseId == WarehouseId && oDetail.SubsubcategoryName == "Kisan Kirana"
                                   select new
                                   {
                                       CustomerId = oMast.CustomerId,
                                       Skcode = oMast.Skcode,
                                       WarehouseName = oMast.WarehouseName,
                                       ShippingAddress = oMast.ShippingAddress,
                                       TotalAmount = oMast.TotalAmount,
                                       ShopName = oMast.ShopName,
                                       SalesPerson = oDetail.ExecutiveName,
                                       GrossAmount = oMast.GrossAmount,
                                       CustomerName = oMast.CustomerName,
                                       Customerphonenum = oMast.Customerphonenum,


                                   }).ToList();

                    List<OrderDispatchedMasterDTOMExport> FinalODMList = new List<OrderDispatchedMasterDTOMExport>();
                    List<int> custId = new List<int>();

                    foreach (var order in ODMList)
                    {

                        try
                        {
                            bool isexits = true;
                            if (FinalODMList.Count > 0)
                            {
                                foreach (var ss in custId)
                                {
                                    if (ss == order.CustomerId)
                                    {
                                        var found = FinalODMList.Where(arr => arr.CustomerId == order.CustomerId).FirstOrDefault();
                                        if (found != null)
                                        {

                                            found.OrderCount++;
                                            found.TotalAmount = found.TotalAmount + order.GrossAmount;
                                            isexits = false;
                                            break;

                                        }
                                    }
                                }
                                //
                                if (isexits)
                                {
                                    OrderDispatchedMasterDTOMExport dto = new OrderDispatchedMasterDTOMExport();
                                    dto.CustomerId = order.CustomerId;
                                    dto.OrderCount = 1;
                                    dto.SalesPerson = order.SalesPerson;
                                    dto.CustomerName = order.CustomerName;
                                    dto.Customerphonenum = order.Customerphonenum;
                                    dto.ShopName = order.ShopName;
                                    dto.Skcode = order.Skcode;
                                    dto.WarehouseName = order.WarehouseName;
                                    dto.ShippingAddress = order.ShippingAddress;
                                    dto.TotalAmount = order.TotalAmount;
                                    FinalODMList.Add(dto);
                                    custId.Add(order.CustomerId);// added record
                                }

                            }
                            else
                            {
                                OrderDispatchedMasterDTOMExport dto = new OrderDispatchedMasterDTOMExport();
                                dto.CustomerId = order.CustomerId;
                                dto.OrderCount = 1;
                                dto.SalesPerson = order.SalesPerson;
                                dto.CustomerName = order.CustomerName;
                                dto.Customerphonenum = order.Customerphonenum;
                                dto.ShopName = order.ShopName;
                                dto.Skcode = order.Skcode;
                                dto.WarehouseName = order.WarehouseName;
                                dto.ShippingAddress = order.ShippingAddress;
                                dto.TotalAmount = order.TotalAmount;
                                FinalODMList.Add(dto);
                                custId.Add(order.CustomerId);// added record
                            }

                        }
                        catch (Exception st)
                        {
                        }

                    }
                    return FinalODMList;
                    logger.Info("End OrderSettle: ");
                }
                catch (Exception ex)
                {
                    logger.Error("Error in OrderSettle " + ex.Message);
                    logger.Info("End  OrderSettle: ");
                    return null;
                }
        }
        #endregion

        [Authorize]
        [Route("OrderDataAdvanceKK")]
        [HttpGet]
        public List<GetMonthWiseData> getsAdvanceDataKK(string Year, int Cityid, int WarehouseId)
        {

            logger.Info("start OrderSettle: ");
            using (var context = new AuthContext())
            {
                //using (var db = new AuthContext())
                //{
                //    try
                //    {
                //        var identity = User.Identity as ClaimsIdentity;
                //        int compid = 0, userid = 0;
                //        foreach (Claim claim in identity.Claims)
                //        {
                //            if (claim.Type == "compid")
                //            {
                //                compid = int.Parse(claim.Value);
                //            }
                //            if (claim.Type == "userid")
                //            {
                //                userid = int.Parse(claim.Value);
                //            }
                //        }


                //        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);

                //        string startyear = "01/01/" + Year;
                //        string endyear = "12/31/" + Year;

                //        CultureInfo provider = CultureInfo.InvariantCulture;
                //        // It throws Argument null exception 
                //        DateTime StartdateTime = DateTime.ParseExact(startyear, "mm/dd/yyyy", provider);
                //        DateTime endDate = StartdateTime.AddMonths(12).AddDays(-1);
                //        int Noofmonth = endDate.Month;
                //        List<GetMonthWiseData> mylist = new List<GetMonthWiseData>();
                //        List<Customer> customer = db.Customers.Where(a => a.Deleted == false && a.Warehouseid == WarehouseId).ToList();
                //        List<int> customerid = customer.Select(x => x.CustomerId).ToList();
                //        var warehouse = db.Warehouses.FirstOrDefault(x => x.WarehouseId == WarehouseId);
                //        var executivename = (from j in db.Customers
                //                             where customerid.Contains(j.CustomerId)
                //                             join i in db.Peoples on j.ExecutiveId equals i.PeopleID
                //                             select new
                //                             {
                //                                 j.CustomerId,
                //                                 i.PeopleID,
                //                                 i.DisplayName
                //                             }).ToList();

                //        foreach (Customer item in customer)
                //        {
                //            Customer cw = db.Customers.Where(a => a.CustomerId == item.CustomerId).FirstOrDefault();

                //            GetMonthWiseData order = new GetMonthWiseData();
                //            //List<OrderMaster> od = db.DbOrderMaster.Where(a => a.CustomerId == item.CustomerId && a.CreatedDate.Year == StartdateTime.Year).ToList();
                //            var od = (from oMast in db.DbOrderMaster
                //                      join oDetail in db.DbOrderDetails on oMast.OrderId equals oDetail.OrderId
                //                      where oMast.CustomerId == item.CustomerId && oMast.CreatedDate.Year == StartdateTime.Year && oDetail.SubsubcategoryName == "Kisan Kirana"
                //                      select new
                //                      {

                //                          CreatedDate = oMast.CreatedDate,
                //                          GrossAmount = oMast.GrossAmount,



                //                      }).ToList();


                //            order.skcode = item.Skcode;
                //            order.shopname = item.ShopName;
                //            order.warehousename = warehouse.WarehouseName + " , " + warehouse.CityName;
                //            order.warehouseid = item.Warehouseid;

                //            try
                //            {
                //                if (executivename != null && executivename.Any(x => x.CustomerId == item.CustomerId))
                //                {
                //                    order.executiveid = executivename.FirstOrDefault(x => x.CustomerId == item.CustomerId).PeopleID;
                //                    order.executivename = executivename.FirstOrDefault(x => x.CustomerId == item.CustomerId).DisplayName;
                //                }
                //                else
                //                {
                //                    order.executiveid = 0;
                //                    order.executivename = null;
                //                }
                //            }
                //            catch (Exception ss)
                //            {

                //            }
                //            for (int i = 1; i <= Noofmonth; i++)
                //            {

                //                var monthdata = od.Where(x => x.CreatedDate.Month == i).Select(b => b.GrossAmount).Sum();
                //                if (i == 1)
                //                    order.Jan = monthdata;
                //                else if (i == 2)
                //                    order.Feb = monthdata;
                //                else if (i == 3)
                //                    order.Mar = monthdata;
                //                else if (i == 4)
                //                    order.Apr = monthdata;
                //                else if (i == 5)
                //                    order.May = monthdata;
                //                else if (i == 6)
                //                    order.June = monthdata;
                //                else if (i == 7)
                //                    order.July = monthdata;
                //                else if (i == 8)
                //                    order.Aug = monthdata;
                //                else if (i == 9)
                //                    order.Sep = monthdata;
                //                else if (i == 10)
                //                    order.Oct = monthdata;
                //                else if (i == 11)
                //                    order.Nov = monthdata;
                //                else if (i == 12)
                //                    order.Dec = monthdata;
                //            }
                //            order.TotalAmount = od.Sum(x => x.GrossAmount);
                //            order.totalorderCount = od.Count();

                //            mylist.Add(order);

                //        }
                //        logger.Info("End  OrderSettle: ");
                //        return mylist;

                //    }
                //    catch (Exception ex)
                //    {
                //        logger.Error("Error in OrderSettle " + ex.Message);
                //        logger.Info("End  OrderSettle: ");
                //        return null;
                //    }
                //}
                return null;
            }
        }


        //EOC Pravesh 08-08-2019

        [Route("GetBackendZilaInvoiceData")]
        [HttpGet]
        public List<GetBackendZilaInvoiceDataDc> GetBackendZilaInvoiceData(string id)
        {
            logger.Info("start PurchaseOrderMaster: ");
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                OrderDispatchedMaster order = new OrderDispatchedMaster();
                List<HsnSummaryDC> hsnSummaryDCs = new List<HsnSummaryDC>();
                List<GetBackendZilaInvoiceDataDc> ReturnOrderList = new List<GetBackendZilaInvoiceDataDc>();
                if (id != null)
                {
                    int Id = Convert.ToInt32(id);
                    //order = db.DbOrderMaster.Where(x => x.OrderId == Id).Include("orderDetails").FirstOrDefault();
                    order= db.OrderDispatchedMasters.Where(x => x.OrderId == Id).Include(x => x.orderDetails).FirstOrDefault();
                    order.orderDetails = order.orderDetails.OrderByDescending(z => z.CGSTTaxPercentage).ToList();
                    var perc = order.orderDetails.Select(z => z.CGSTTaxPercentage).Distinct().ToList();
                    //foreach(var e in perc)
                    //{
                    //    HsnSummaryDC a = new HsnSummaryDC();
                    //    a.CgstPercent = order.orderDetails.Where(x => x.CGSTTaxPercentage == e).Sum(w => w.CGSTTaxPercentage);
                    //    a.IgstPercent = a.CgstPercent;
                    //    a.NetAmount = order.orderDetails.Where(x => x.CGSTTaxPercentage == e).Sum(w => w.TotalAmountAfterTaxDisc);
                    //    a.TaxableAmount = order.orderDetails.Where(x => x.CGSTTaxPercentage == e).Sum(w => w.AmtWithoutAfterTaxDisc);
                    //    hsnSummaryDCs.Add(a);
                    //}
                    foreach (var e in perc)
                    {
                        HsnSummaryDC a = new HsnSummaryDC();
                        a.CgstPercent = order.orderDetails.Where(x => x.CGSTTaxPercentage == e).Sum(w => w.CGSTTaxPercentage);
                        a.IgstPercent = a.CgstPercent;
                        a.NetAmount = order.orderDetails.Where(x => x.CGSTTaxPercentage == e).Sum(w => w.TotalAmountAfterTaxDisc);
                        a.TaxableAmount = order.orderDetails.Where(x => x.CGSTTaxPercentage == e).Sum(w => w.AmtWithoutAfterTaxDisc);
                        hsnSummaryDCs.Add(a);
                        GetBackendZilaInvoiceDataDc Returnorder = new GetBackendZilaInvoiceDataDc();
                        List<OrderDispatchedDetails> LocalDtaa = order.orderDetails.Where(z => z.CGSTTaxPercentage == e).ToList();
                        if (LocalDtaa != null && LocalDtaa.Any())
                        {
                            Returnorder.OrderDetails = LocalDtaa;
                            Returnorder.Cgst = e;
                            Returnorder.HsnSummaryDCs = hsnSummaryDCs;
                            //Returnorder.obj = order;
                            ReturnOrderList.Add(Returnorder);
                        }
                    }
                }
                return ReturnOrderList;
            }
        }



        public class GetBackendZilaInvoiceDataDc
        {
            public dynamic obj { get; set; }
            public double Cgst { get; set; }
            public List<OrderDispatchedDetails> OrderDetails { get; set; }
            public List<HsnSummaryDC> HsnSummaryDCs { get; set; }
        }

        public class OrderDispatchedMasterDTOMExport
        {
            public int CustomerId { get; set; }
            public int OrderCount { get; set; }
            public string SalesPerson { get; set; }
            public string CustomerName { get; set; }
            public string Customerphonenum { get; set; }
            public string ShopName { get; set; }
            public string Skcode { get; set; }
            public string WarehouseName { get; set; }
            public string ShippingAddress { get; set; }
            public double TotalAmount { get; set; }
        }

        public class GetFreebiesInfo
        {
            public int itemId { get; set; }
            public int MinOrderQuantity { get; set; }
            public int FreeItemId { get; set; }
            public int NoOffreeQuantity { get; set; }
            public string OfferType { get; set; }
            public int BillDisountNoOffreeQuantity { get; set; }
        }
        public class HsnSummaryDC
        {
            public double IgstPercent { get; set; }
            public double CgstPercent { get; set; }
            public double NetAmount { get; set; }
            public double TaxableAmount { get; set; }
        }
    }
}