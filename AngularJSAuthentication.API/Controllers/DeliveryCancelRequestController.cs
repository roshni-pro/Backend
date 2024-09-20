
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using MongoDB.Driver;
using NLog;


namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/DeliveryCancelRequest")]
    public class DeliveryCancelRequestController : ApiController
    {

        [Route("Reject")]
        [HttpPost]
        public bool Reject(DeliveryCancelRequestDTO deliveryCancelRequestDTO)
        {
            using (var context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                // var OrderIds = new SqlParameter("@OrderID", deliveryCancelRequestDTO.OrderId);

                //string status = context.Database.SqlQuery<string>("GetOrderStatus @OrderID", OrderIds).FirstOrDefault();
                if (deliveryCancelRequestDTO != null)
                {
                    var People = context.Peoples.Where(x => x.PeopleID == userid).FirstOrDefault();

                    //Order Master
                    // var ordermaster = context.DbOrderMaster.Where(x => x.OrderId == deliveryCancelRequestDTO.OrderId).FirstOrDefault();
                    // ordermaster.Status = status;
                    // context.Entry(ordermaster).State = EntityState.Modified;

                    //Order Dispatched Masters
                    var OrderDispatchedMaster = context.OrderDispatchedMasters.Where(x => x.OrderId == deliveryCancelRequestDTO.OrderId).FirstOrDefault();
                    //OrderDispatchedMaster.Status = status;
                    //OrderDispatchedMaster.DeliveryCanceledComments= deliveryCancelRequestDTO.Comments;
                    //context.Entry(OrderDispatchedMaster).State = EntityState.Modified;

                    //OrderDetails 
                    //var OrderDetails = context.DbOrderDetails.Where(x => x.OrderId == deliveryCancelRequestDTO.OrderId).ToList();
                    //foreach (var item in OrderDetails)
                    //{
                    //    item.status = status;
                    //    context.Entry(item).State = EntityState.Modified;
                    //}
                    //Order Delivery Master 
                    //var OrderDeliveryMaster = context.OrderDeliveryMasterDB.Where(x => x.OrderId == deliveryCancelRequestDTO.OrderId && x.DeliveryIssuanceId == OrderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster).FirstOrDefault();
                    //OrderDeliveryMaster.Status = status;
                    //context.Entry(OrderDeliveryMaster).State = EntityState.Modified;

                    OrderMasterHistories OrderMasterHistories = new OrderMasterHistories();
                    OrderMasterHistories.orderid = OrderDispatchedMaster.OrderId;
                    OrderMasterHistories.Status = OrderDispatchedMaster.Status;
                    OrderMasterHistories.Reasoncancel = "By Customer Delight";
                    OrderMasterHistories.Warehousename = OrderDispatchedMaster.WarehouseName;
                    OrderMasterHistories.DeliveryIssuanceId = OrderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster;
                    OrderMasterHistories.username = People.DisplayName != null ? People.DisplayName : People.PeopleFirstName; ;
                    OrderMasterHistories.userid = People.PeopleID;
                    OrderMasterHistories.CreatedDate = DateTime.Now;
                    context.OrderMasterHistoriesDB.Add(OrderMasterHistories);

                    DeliveryCanceledRequestHistory add = new DeliveryCanceledRequestHistory();
                    add.OrderId = OrderDispatchedMaster.OrderId;
                    add.Comments = deliveryCancelRequestDTO.Comments;
                    add.DeliveryCanceledStatus = deliveryCancelRequestDTO.DeliveryCanceledStatus;
                    add.CreatedBy = userid;
                    add.IsActive = true;
                    add.IsDeleted = false;
                    add.CreatedDate = DateTime.Now;
                    add.ModifiedDate = DateTime.Now;
                    add.ModifiedBy = 0;
                    context.DeliveryCanceledRequestHistoryDb.Add(add);

                    var checkAssigmentStatus = context.DeliveryIssuanceDb.Where(z => z.DeliveryIssuanceId == OrderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster && (z.Status == "Submitted" || z.Status == "Payment Accepted" || z.Status == "Payment Submitted")).FirstOrDefault();
                    if (checkAssigmentStatus != null)
                    {
                        var AssignmentRechangeOrder = context.AssignmentRechangeOrder.FirstOrDefault(x => x.AssignmentId == OrderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster && x.OrderId == OrderDispatchedMaster.OrderId);
                        if (AssignmentRechangeOrder != null)
                        {

                            AssignmentRechangeOrder.Status = 1;
                            AssignmentRechangeOrder.ModifiedDate = DateTime.Now;
                            AssignmentRechangeOrder.ModifiedBy = userid;
                            context.Entry(AssignmentRechangeOrder).State = EntityState.Modified;
                        }
                        else
                        {
                            AssignmentRechangeOrder assignmentRechangeOrder = new AssignmentRechangeOrder
                            {
                                IsActive = true,
                                IsDeleted = false,
                                CreatedDate = DateTime.Now,
                                OrderId = OrderDispatchedMaster.OrderId,
                                Status = 1,
                                AssignmentId = OrderDispatchedMaster.DeliveryIssuanceIdOrderDeliveryMaster ?? 0,
                                CreatedBy = userid,
                                Reason = "By Customer Delight"
                            };
                            context.AssignmentRechangeOrder.Add(assignmentRechangeOrder);
                        }
                    }
                    context.Commit();
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        [Route("getOrderDetails")]
        [HttpGet]
        public dynamic getOrderDetails(int OrderId)
        {
            using (var myContext = new AuthContext())
            {
                var OrderIds = new SqlParameter("@OrderId", OrderId);

                var result = myContext.Database.SqlQuery<OrderPlaceDTO>("getOrderDetails @OrderId", OrderIds).ToList();
                return result;
            }
        }

        [Route("getOrderCallback")]
        [HttpPost]
        public dynamic getOrderDetails(GetOrder getorder)
        {
            using (var myContext = new AuthContext())
            {
                if (string.IsNullOrEmpty(getorder.Skcode))
                {
                    getorder.Skcode = "";
                }
                var Wid = new SqlParameter("@WH", getorder.WarehouseId);
                var OrderId = new SqlParameter("@OrderId", getorder.OrderId);
                var skcode = new SqlParameter("@skcode", getorder.Skcode);
                var result = myContext.Database.SqlQuery<GetOrderDTO>("getOrderCallback @WH,@OrderId,@skcode", Wid, OrderId, skcode).ToList();
                return result;
            }
        }
        [Route("UpdateCallBackdate")]
        [HttpPost]
        public bool UpdateCallBackdate(DeliveryCancelRequestDTO deliverycancelDTO)
        {
            using (var db = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var orderDispathmaster = db.OrderDispatchedMasters.Where(x => x.OrderId == deliverycancelDTO.OrderId).FirstOrDefault();
                orderDispathmaster.DeliveryCanceledComments = deliverycancelDTO.Comments;
                db.Entry(orderDispathmaster).State = EntityState.Modified;

                var cancellist = db.DeliveryCanceledRequestHistoryDb.Where(x => x.OrderId == deliverycancelDTO.OrderId && x.DeliveryCanceledStatus == "Call back" && x.IsDeleted == false && x.IsActive == true).FirstOrDefault();
                if (cancellist != null)
                {
                    cancellist.IsActive = false;
                    cancellist.IsDeleted = true;
                    db.Entry(cancellist).State = EntityState.Modified;

                    DeliveryCanceledRequestHistory add = new DeliveryCanceledRequestHistory();
                    add.OrderId = deliverycancelDTO.OrderId;
                    add.Comments = deliverycancelDTO.Comments;
                    add.ConformationDate = deliverycancelDTO.date;
                    add.DeliveryCanceledStatus = deliverycancelDTO.DeliveryCanceledStatus;
                    add.CreatedBy = userid;
                    add.IsActive = true;
                    add.IsDeleted = false;
                    add.CreatedDate = DateTime.Now;
                    add.ModifiedDate = DateTime.Now;
                    add.ModifiedBy = 0;
                    db.DeliveryCanceledRequestHistoryDb.Add(add);

                }
                else
                {
                    DeliveryCanceledRequestHistory add = new DeliveryCanceledRequestHistory();
                    add.OrderId = deliverycancelDTO.OrderId;
                    add.Comments = deliverycancelDTO.Comments;
                    add.ConformationDate = deliverycancelDTO.date;
                    add.DeliveryCanceledStatus = deliverycancelDTO.DeliveryCanceledStatus;
                    add.CreatedBy = userid;
                    add.IsActive = true;
                    add.IsDeleted = false;
                    add.CreatedDate = DateTime.Now;
                    add.ModifiedDate = DateTime.Now;
                    add.ModifiedBy = 0;
                    db.DeliveryCanceledRequestHistoryDb.Add(add);
                }
                db.Commit();
            }
            return true;
        }

        [Route("GetDCRDailyReport")]
        [HttpGet]
        [AllowAnonymous]
        public List<GetDCRDailyReportDTO> GetDCRDailyReport( string type,DateTime startTime,DateTime dateTime)
        {
            using (var myContext = new AuthContext())
            {
                List<GetDCRDailyReportDTO> list = new List<GetDCRDailyReportDTO>();
                var Type = new SqlParameter("@type", type);
                var StartTime = new SqlParameter("@startTime", startTime);
                var DateTime = new SqlParameter("@dateTime", dateTime);
                list = myContext.Database.SqlQuery<GetDCRDailyReportDTO>("EXEC DCRDailyReport @type,@startTime,@dateTime", Type, StartTime, DateTime).ToList();         
                return list;
            }
        }
    }

    public class GetOrder
    {
        public int WarehouseId { get; set; }
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string MobileNumber { get; set; }
    }
    public class GetOrderDTO
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public double GrossAmount { get; set; }
        public string WarehouseName { get; set; }
        public string Skcode { get; set; }
        public string Customerphonenum { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ReDispatchCount { get; set; }
    }
    public class DeliveryCancelRequestDTO
    {
        public int OrderId { get; set; }
        public string value { get; set; }
        public string Comments { get; set; }
        public DateTime date { get; set; }
        public string DeliveryCanceledStatus { get; set; }



    }

    public class OrderDCR
    {
        public int OrderId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class OrderPlaceDTO
    {
        public int OrderDispatchedMasterId { get; set; }//
        public int OrderId { get; set; }//
        public string Status { get; set; } //
        public int DeliveryIssuanceId { get; set; }//
        public string comments { get; set; }//
        public string CheckNo { get; set; } //
        public double CheckAmount { get; set; }//
        public string ElectronicPaymentNo { get; set; }//
        public double ElectronicAmount { get; set; }//
        public double CashAmount { get; set; }//
        public double RecivedAmount { get; set; }//
        public double OnlineServiceTax { get; set; } //
        public string Signimg { get; set; } // 
        public string DboyMobileNo { get; set; } //
        public string DboyName { get; set; }//
        public int ReDispatchCount { get; set; }//
        public int WarehouseId { get; set; }//
        public bool IsElectronicPayment { get; set; }
        public string ChequeImageUrl { get; set; }
        public string ChequeBankName { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string paymentThrough { get; set; } // 
        public string paymentMode { get; set; }
        public double mPosAmount { get; set; }
        public int? ElectronicPaymentType { get; set; }

        public double? DeliveryLat { get; set; }// added on 08/07/02019 
        public double? DeliveryLng { get; set; }


    }
    public class GetDCRDailyReportDTO
    {
        public int OrderId { get; set; }
        public string Skcode { get; set; }
        public string WarehouseName { get; set; }
        public string DeliveryCanceledStatus { get; set; }
        public string Comments { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ConformationDate { get; set; }
        public string CDExecutiveName { get; set; }
    }

}