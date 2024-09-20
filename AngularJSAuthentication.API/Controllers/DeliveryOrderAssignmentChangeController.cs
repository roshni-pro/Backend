using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.DataContracts.Masters;
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
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeliveryOrderAssignmentChange")]
    public class DeliveryOrderAssignmentChangeController : BaseAuthController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage Get(Int32 DeliveryIssuanceId)//get all 
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                        compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                    if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                        userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                    int CompanyId = compid;
                    List<DBoySummary> SUmmarylist = new List<DBoySummary>();
                    DeliveryIssuance DeliveryIssuance = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == DeliveryIssuanceId).FirstOrDefault();
                    if (DeliveryIssuance != null)
                    {
                        var param = new SqlParameter("DeliveryIssuanceId", DeliveryIssuanceId);
                        bool IsTripAssignment = context.Database.SqlQuery<bool>("exec operation.IsTripAssignment @DeliveryIssuanceId", param).FirstOrDefault();

                        if (DeliveryIssuance != null)
                        {
                            try
                            {
                                string Borderid = Convert.ToString(DeliveryIssuance.DeliveryIssuanceId);
                                string BorderCodeId = Borderid.PadLeft(9, '0');
                                temOrderQBcode code = context.AssignmentGenerateBarcode(BorderCodeId);
                                DeliveryIssuance.AssignmentBarcodeImage = code.BarcodeImage;//for assignment barcode

                            }
                            catch (Exception es) { }
                        }
                        string[] idss = DeliveryIssuance.OrderIds.Split(',');
                        int[] OrderIdList = DeliveryIssuance.OrderIds.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                        var orderdipatchmasterList = context.OrderDeliveryMasterDB.Where(x => OrderIdList.Contains(x.OrderId) && x.CompanyId == compid && x.DeliveryIssuanceId == DeliveryIssuanceId).ToList();
                        var orderdipatchedList = context.OrderDispatchedMasters.Where(x => OrderIdList.Contains(x.OrderId) && x.DeliveryIssuanceIdOrderDeliveryMaster == DeliveryIssuanceId).Select(x => new { x.OrderId, x.DeliveryCanceledComments, x.DeliveryIssuanceIdOrderDeliveryMaster, x.DBoyId }).ToList();
                        //var PaymentResponseRetailerAppList = context.PaymentResponseRetailerAppDb.Where(x => OrderIdList.Contains(x.OrderId) && x.status == "Success").ToList();
                        var PaymentResponseRetailerApps = context.PaymentResponseRetailerAppDb.Where(x => OrderIdList.Contains(x.OrderId) && x.status == "Success").ToList();
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


                        foreach (var od in idss)
                        {
                            var oid = Convert.ToInt32(od);
                            var orderdipatchmaster = orderdipatchmasterList.Where(x => x.OrderId == oid && x.DeliveryIssuanceId == DeliveryIssuanceId).FirstOrDefault();
                            var orderdipatcedhmaster = orderdipatchedList.Where(x => x.OrderId == oid && x.DeliveryIssuanceIdOrderDeliveryMaster == DeliveryIssuanceId).FirstOrDefault();
                            DBoySummary Os = new DBoySummary();
                            if (orderdipatchmaster != null)
                            {
                                Os.chequeNo = orderdipatchmaster.CheckNo;
                                Os.CustomerId = orderdipatchmaster.CustomerId;
                                Os.CustomerName = orderdipatchmaster.CustomerName;
                                Os.DBoyId = orderdipatcedhmaster != null ? orderdipatcedhmaster.DBoyId : 0;
                                Os.DBoyName = orderdipatchmaster.DboyName;
                                Os.DboyMobileNo = orderdipatchmaster.DboyMobileNo;
                                Os.GrossAmount = orderdipatchmaster.GrossAmount;
                                Os.OrderId = orderdipatchmaster.OrderId;
                                //Os.SalesPerson = orderdipatchmaster.SalesPerson;
                                //Os.SalesPersonId = orderdipatchmaster.SalesPersonId;
                                Os.Status = orderdipatchmaster.Status;
                                Os.Skcode = orderdipatchmaster.Skcode;
                                Os.invoice_no = orderdipatchmaster.invoice_no;
                                Os.ShopName = orderdipatchmaster.ShopName;
                                Os.cashAmount = orderdipatchmaster.CashAmount;
                                Os.ElectronicAmount = orderdipatchmaster.ElectronicAmount;
                                Os.chequeAmount = orderdipatchmaster.CheckAmount;
                                // Os.GullakAmount = orderdipatchmaster.GullakAmount;
                                Os.PaymentDetails = PaymentResponseRetailerAppList.Where(z => z.OrderId == Os.OrderId && z.status == "Success")
                                .Select(z => new PaymentDto
                                {
                                    Amount = z.amount,
                                    PaymentFrom = z.PaymentFrom,
                                    //TransDate = z.UpdatedDate,
                                    TransRefNo = z.GatewayTransId,
                                    IsOnline = z.IsOnline
                                }).ToList();
                                if (Os.PaymentDetails == null)
                                    Os.PaymentDetails = new List<PaymentDto>();

                                if (!Os.PaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CASH"))
                                {
                                    Os.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = Os.cashAmount,
                                        PaymentFrom = "Cash",
                                        TransRefNo = ""
                                    });
                                }
                                if (!Os.PaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "CHEQUE"))
                                {
                                    Os.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = Os.chequeAmount,
                                        PaymentFrom = "Cheque",
                                        TransRefNo = Os.chequeNo
                                    });
                                }
                                if (!Os.PaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "Gullak"))
                                {
                                    Os.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = 0,
                                        PaymentFrom = "Gullak",
                                        GatewayOrderId = ""
                                    });
                                }
                                else
                                {
                                    Os.PaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "Gullak").ToList().ForEach(x => x.PaymentFrom = "Gullak");
                                }
                                if (!Os.PaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "EPAYLATER"))
                                {
                                    Os.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = 0,
                                        PaymentFrom = "EPaylater"
                                    });
                                }
                                else
                                {
                                    Os.PaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "EPAYLATER").ToList().ForEach(x => x.PaymentFrom = "EPaylater");
                                }
                                if (!Os.PaymentDetails.Any(x => x.PaymentFrom.ToUpper() == "MPOS"))
                                {
                                    Os.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = 0,
                                        PaymentFrom = "MPos"
                                    });
                                }
                                else
                                {
                                    Os.PaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "MPOS").ToList().ForEach(x => x.PaymentFrom = "MPos");
                                }

                                if (!Os.PaymentDetails.Any(x => x.IsOnline && (x.PaymentFrom.ToUpper() != "EPAYLATER" && x.PaymentFrom.ToUpper() != "GULLAK" && x.PaymentFrom.ToUpper() != "MPOS")))
                                {
                                    Os.PaymentDetails.Add(new PaymentDto
                                    {
                                        Amount = Os.ElectronicAmount,
                                        PaymentFrom = "Online"
                                    });
                                }
                                else
                                {
                                    //Os.PaymentDetails.Where(x => x.PaymentFrom.ToUpper() == "HDFC" || x.PaymentFrom.ToUpper() == "TRUEPAY" || x.PaymentFrom.ToUpper() == "Gullak").ToList().ForEach(x => x.PaymentFrom = "Online");

                                    Os.PaymentDetails.Where(x => x.IsOnline && (x.PaymentFrom.ToUpper() != "EPAYLATER" && x.PaymentFrom.ToUpper() != "GULLAK" && x.PaymentFrom.ToUpper() != "MPOS")).ToList().ForEach(x => x.PaymentFrom = "Online");
                                }
                                // Os.PaymentDetails = Os.PaymentDetails.OrderBy(x => x.PaymentFrom).ToList();
                                Os.PaymentDetails = Os.PaymentDetails.GroupBy(x => new { x.OrderId, x.PaymentFrom }).Select(x => new PaymentDto { Amount = x.Sum(y => y.Amount), OrderId = x.Key.OrderId, PaymentFrom = x.Key.PaymentFrom, TransRefNo = string.Join(",", x.Select(y => y.TransRefNo)), TransDate = x.FirstOrDefault().TransDate }).OrderBy(x => x.PaymentFrom).ToList();

                                Os.comments = orderdipatchmaster.comments;
                                Os.DeliveryCanceledComments = orderdipatcedhmaster == null ? null : orderdipatcedhmaster.DeliveryCanceledComments;
                                Os.ReDispatchCount = orderdipatchmaster.ReDispatchCount;
                                Os.CreatedDate = orderdipatchmaster.CreatedDate;
                                Os.DeliveryIssuanceId = orderdipatchmaster.DeliveryIssuanceId;
                                Os.DeliveryIssuanceStatus = DeliveryIssuance.Status;//DeliveryIssuance Status from DeliveryIssuance
                                Os.UploadedFileName = DeliveryIssuance.UploadedFileName;//DeliveryIssuance Status from DeliveryIssuance
                                Os.AssignmentBarcodeImage = DeliveryIssuance.AssignmentBarcodeImage;

                                SUmmarylist.Add(Os);
                            }
                        }

                        var AssignmentRechangeOrder = context.AssignmentRechangeOrder.Where(x => x.AssignmentId == DeliveryIssuanceId).ToList();
                        SUmmarylist.ForEach(x =>
                        {
                            x.IsTripAssignment = IsTripAssignment;

                            if (AssignmentRechangeOrder.Any(y => y.OrderId == x.OrderId))
                            {
                                x.OrderRejectStatus = AssignmentRechangeOrder.FirstOrDefault(y => y.OrderId == x.OrderId).Status == 1;
                            }
                            else
                                x.OrderRejectStatus = false;
                        });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, SUmmarylist);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

        [Route("update")]
        [HttpPut]
        [Authorize]
        public dynamic update(TempOrderDTO TempOrderDTO)
        {
            logger.Info("Update: ");
            using (AuthContext context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                string D_Name = null;
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
                    if (claim.Type == "DisplayName")
                    {
                        D_Name = (claim.Value);
                    }
                }

                logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                //OrderDliveryAssignmentUpdate
                People people = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();
                OrderDeliveryMaster OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(x => x.OrderId == TempOrderDTO.OrderId && x.DeliveryIssuanceId == TempOrderDTO.IssuanceId).SingleOrDefault();
                OrderDeliveryMaster.Status = TempOrderDTO.status;
                if (TempOrderDTO.status != null)
                {
                    if (TempOrderDTO.status == "Delivery Redispatch")
                    {
                        OrderDeliveryMaster.ReDispatchCount++;
                    }

                    if (TempOrderDTO.Cashamount == 0)
                    {
                        OrderDeliveryMaster.CashAmount = 0;
                    }
                    OrderDeliveryMaster.comments = TempOrderDTO.comment;
                    OrderDeliveryMaster.userid = userid;
                    OrderDeliveryMaster.UpdatedDate = DateTime.Now;
                    context.Entry(OrderDeliveryMaster).State = EntityState.Modified;

                    #region PaymentResponse
                    var PaymentResponse = context.PaymentResponseRetailerAppDb.Where(x => x.OrderId == TempOrderDTO.OrderId && x.status == "Success").ToList();

                    if (PaymentResponse.Any(x => x.PaymentFrom.ToUpper() == "CASH"))
                    {
                        if (TempOrderDTO.Cashamount == 0 && PaymentResponse.Count(x => x.PaymentFrom.ToUpper() == "CASH") > 0)
                        {
                            foreach (var Orderp in PaymentResponse.Where(x => x.PaymentFrom.ToUpper() == "CASH").ToList())
                            {
                                var payment = Orderp;
                                payment.UpdatedDate = indianTime;
                                payment.amount = TempOrderDTO.Cashamount;
                                payment.statusDesc = TempOrderDTO.comment;
                                payment.status = TempOrderDTO.Cashamount > 0 ? payment.status : "Failed";
                                context.Entry(payment).State = EntityState.Modified;
                            }
                        }
                        else
                        {
                            var payment = PaymentResponse.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "CASH");
                            payment.UpdatedDate = indianTime;
                            payment.amount = TempOrderDTO.Cashamount;
                            payment.statusDesc = TempOrderDTO.comment;
                            payment.status = TempOrderDTO.Cashamount > 0 ? payment.status : "Failed";
                            context.Entry(payment).State = EntityState.Modified;

                        }

                    }
                    else if (TempOrderDTO.Cashamount > 0)
                    {
                        var PaymentResponseRetailerAppDbCash = new PaymentResponseRetailerApp
                        {
                            amount = TempOrderDTO.Cashamount,
                            CreatedDate = indianTime,
                            currencyCode = "INR",
                            OrderId = TempOrderDTO.OrderId,
                            PaymentFrom = "Cash",
                            status = "Success",
                            UpdatedDate = indianTime,
                            statusDesc = TempOrderDTO.comment,
                            IsRefund = false

                        };
                        context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDbCash);

                        if (OrderDeliveryMaster.CashAmount > 0)
                        {
                            OrderDeliveryMaster.CashAmount = 0;
                        }
                    }
                    if (PaymentResponse.Any(x => x.PaymentFrom.ToUpper() == "CHEQUE"))
                    {
                        var payment = PaymentResponse.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "CHEQUE");
                        payment.amount = TempOrderDTO.chequeamount;
                        payment.GatewayTransId = TempOrderDTO.chequeno;
                        payment.UpdatedDate = indianTime;
                        payment.status = TempOrderDTO.chequeamount == 0 ? "Failed" : payment.status;
                        payment.statusDesc = TempOrderDTO.comment;
                        context.Entry(payment).State = EntityState.Modified;
                    }
                    else if (TempOrderDTO.chequeamount > 0)
                    {
                        var PaymentResponseRetailerAppDbCheck = new PaymentResponseRetailerApp
                        {
                            amount = TempOrderDTO.chequeamount,
                            GatewayTransId = TempOrderDTO.chequeno,
                            CreatedDate = indianTime,
                            currencyCode = "INR",
                            OrderId = TempOrderDTO.OrderId,
                            PaymentFrom = "Cheque",
                            status = "Success",
                            UpdatedDate = indianTime,
                            statusDesc = TempOrderDTO.comment,
                            IsRefund = false
                        };
                        context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDbCheck);
                    }
                    if (PaymentResponse.Any(x => x.PaymentFrom.ToUpper() == "MPOS"))
                    {
                        var payment = PaymentResponse.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "MPOS");
                        payment.UpdatedDate = indianTime;
                        payment.statusDesc = TempOrderDTO.comment;
                        payment.amount = TempOrderDTO.mPosAmount;
                        payment.status = TempOrderDTO.mPosAmount == 0 ? "Failed" : payment.status;
                        payment.GatewayTransId = TempOrderDTO.mPosTransId;
                        context.Entry(payment).State = EntityState.Modified;
                    }
                    else if (TempOrderDTO.mPosAmount > 0)
                    {
                        var PaymentResponseRetailerAppDbEmpos = new PaymentResponseRetailerApp
                        {
                            amount = TempOrderDTO.mPosAmount,
                            GatewayTransId = TempOrderDTO.mPosTransId,
                            CreatedDate = indianTime,
                            currencyCode = "INR",
                            OrderId = TempOrderDTO.OrderId,
                            PaymentFrom = "mPos",
                            status = "Success",
                            UpdatedDate = indianTime,
                            statusDesc = TempOrderDTO.comment,
                            IsRefund = false,
                            IsOnline = true
                        };
                        context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDbEmpos);
                    }
                    if (PaymentResponse.Any(x => x.PaymentFrom.ToUpper() == "EPAYLATER"))
                    {
                        var payment = PaymentResponse.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "EPAYLATER");
                        payment.UpdatedDate = indianTime;
                        payment.amount = TempOrderDTO.EpayLaterAmount;
                        payment.statusDesc = TempOrderDTO.comment;
                        payment.GatewayTransId = TempOrderDTO.EpayLaterTransId;
                        context.Entry(payment).State = EntityState.Modified;
                    }
                    else if (TempOrderDTO.EpayLaterAmount > 0)
                    {
                        var PaymentResponseRetailerAppDbEpayLater = new PaymentResponseRetailerApp
                        {
                            amount = TempOrderDTO.EpayLaterAmount,
                            GatewayTransId = TempOrderDTO.EpayLaterTransId,
                            CreatedDate = indianTime,
                            currencyCode = "INR",
                            OrderId = TempOrderDTO.OrderId,
                            PaymentFrom = "ePaylater",
                            status = "Success",
                            UpdatedDate = indianTime,
                            statusDesc = TempOrderDTO.comment,
                            IsRefund = false,
                            IsOnline = true
                        };
                        context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDbEpayLater);
                    }

                    if (PaymentResponse.Any(x => x.PaymentFrom.ToUpper() == "HDFC" || x.PaymentFrom.ToUpper() == "TRUEPAY"))
                    {
                        var payment = PaymentResponse.FirstOrDefault(x => x.PaymentFrom.ToUpper() == "HDFC" || x.PaymentFrom.ToUpper() == "TRUEPAY");
                        payment.amount = TempOrderDTO.ElectronicAmount;
                        payment.GatewayTransId = TempOrderDTO.ElectronicTransId;
                        payment.UpdatedDate = indianTime;
                        payment.statusDesc = TempOrderDTO.comment;
                        context.Entry(payment).State = EntityState.Modified;
                    }
                    else if (TempOrderDTO.ElectronicAmount > 0)
                    {
                        var PaymentResponseRetailerAppDbOnline = new PaymentResponseRetailerApp
                        {
                            amount = TempOrderDTO.ElectronicAmount,
                            GatewayTransId = TempOrderDTO.ElectronicTransId,
                            CreatedDate = indianTime,
                            currencyCode = "INR",
                            OrderId = TempOrderDTO.OrderId,
                            PaymentFrom = "hdfc",
                            status = "Success",
                            UpdatedDate = indianTime,
                            statusDesc = TempOrderDTO.comment,
                            IsRefund = false,
                            IsOnline = true
                        };
                        context.PaymentResponseRetailerAppDb.Add(PaymentResponseRetailerAppDbOnline);
                    }

                    context.Commit();

                    #endregion

                }
                DOCDTO result = new DOCDTO
                {
                    OrderDeliveryMaster = OrderDeliveryMaster,
                };

                return result;

            }
        }



        //if Dboy Reject Assignment Bill
        [Route("Rejected")]
        [HttpPut]
        public HttpResponseMessage Rejected(DeliveryIssuance DeliveryIssuance)//get all 
        {
            using (AuthContext context = new AuthContext())
            {
                try
                {
                    DeliveryIssuance DeliveryIssuanceUpdate = context.DeliveryIssuanceDb.Where(x => x.DeliveryIssuanceId == DeliveryIssuance.DeliveryIssuanceId && x.PeopleID == DeliveryIssuance.PeopleID
                    && x.Status == "Payment Accepted").FirstOrDefault();
                    if (DeliveryIssuance.ReasonOfreject != null && DeliveryIssuanceUpdate != null)
                    {

                        #region  History 
                        var User = context.Peoples.Where(x => x.PeopleID == DeliveryIssuance.PeopleID).FirstOrDefault();

                        OrderDeliveryMasterHistories OrderDeliveryMasterHistories = new OrderDeliveryMasterHistories();
                        if (User != null)
                        {
                            OrderDeliveryMasterHistories.Cityid = DeliveryIssuanceUpdate.Cityid;
                            OrderDeliveryMasterHistories.city = DeliveryIssuanceUpdate.city;
                            OrderDeliveryMasterHistories.DisplayName = DeliveryIssuanceUpdate.DisplayName;
                            OrderDeliveryMasterHistories.Status = DeliveryIssuanceUpdate.Status;
                            OrderDeliveryMasterHistories.WarehouseId = DeliveryIssuanceUpdate.WarehouseId;
                            OrderDeliveryMasterHistories.PeopleID = DeliveryIssuanceUpdate.PeopleID;
                            OrderDeliveryMasterHistories.VehicleId = DeliveryIssuanceUpdate.VehicleId;
                            OrderDeliveryMasterHistories.VehicleNumber = DeliveryIssuanceUpdate.VehicleNumber;
                            OrderDeliveryMasterHistories.RejectReason = DeliveryIssuanceUpdate.RejectReason;
                            OrderDeliveryMasterHistories.OrderdispatchIds = DeliveryIssuanceUpdate.OrderdispatchIds;
                            OrderDeliveryMasterHistories.OrderIds = DeliveryIssuanceUpdate.OrderIds;
                            OrderDeliveryMasterHistories.Acceptance = DeliveryIssuanceUpdate.Acceptance;
                            OrderDeliveryMasterHistories.IsActive = DeliveryIssuanceUpdate.IsActive;
                            OrderDeliveryMasterHistories.IdealTime = DeliveryIssuanceUpdate.IdealTime;
                            OrderDeliveryMasterHistories.TravelDistance = DeliveryIssuanceUpdate.TravelDistance;
                            OrderDeliveryMasterHistories.DeliveryIssuanceId = DeliveryIssuance.DeliveryIssuanceId;
                            OrderDeliveryMasterHistories.Status = "Submitted";
                            OrderDeliveryMasterHistories.Description = DeliveryIssuance.DeliveryIssuanceId + " :Assignment Billing Rejected";
                            OrderDeliveryMasterHistories.Reason = DeliveryIssuance.ReasonOfreject;
                            OrderDeliveryMasterHistories.username = User.DisplayName;
                            OrderDeliveryMasterHistories.userid = User.PeopleID;
                            OrderDeliveryMasterHistories.CreatedDate = indianTime;
                            OrderDeliveryMasterHistories.UpdatedDate = indianTime;
                            context.OrderDeliveryMasterHistoriesDB.Add(OrderDeliveryMasterHistories);
                            context.Commit();
                        }
                        #endregion
                        DeliveryIssuanceUpdate.Status = "Submitted";
                        DeliveryIssuanceUpdate.UpdatedDate = DateTime.Now;
                        //context.DeliveryIssuanceDb.Attach(DeliveryIssuanceUpdate);
                        context.Entry(DeliveryIssuanceUpdate).State = EntityState.Modified;
                        context.Commit();

                        return Request.CreateResponse(HttpStatusCode.OK, DeliveryIssuanceUpdate);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, false);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }

    }

    public class DOCDTO
    {
        public OrderDeliveryMaster OrderDeliveryMaster { get; set; }
        public OrderMaster OrderMaster { get; set; }
        public List<OrderDetails> OrderDetails { get; set; }
        public OrderDispatchedMaster OrderDispatchedMaster { get; set; }
        public List<OrderDispatchedDetails> OrderDispatchedDetails { get; set; }
    }


    public class TempOrderDTO
    {
        public int OrderId { get; set; }
        public int IssuanceId { get; set; }
        public double Cashamount { get; set; }

        public double chequeamount { get; set; }
        public string chequeno { get; set; }
        public double ElectronicAmount { get; set; }
        public string ElectronicTransId { get; set; }

        public double EpayLaterAmount { get; set; }
        public string EpayLaterTransId { get; set; }

        public double mPosAmount { get; set; }
        public string mPosTransId { get; set; }

        public string comment { get; set; }
        public string status { get; set; }

    }
}



